# Database Design Lessons - E-Commerce API

## 📚 Core Concepts You've Learned

### 1. **Entity Relationships**

#### One-to-Many Relationships
```csharp
// One User has Many Orders
public class User {
    public ICollection<Order> Orders { get; set; } = [];
}

public class Order {
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```
**Key Points:**
- Parent (User) has `ICollection<T>` of children
- Child (Order) has foreign key + navigation property back to parent
- Most common relationship type in databases

#### Many-to-Many Relationships
```csharp
// Products can be in many Carts, Carts can have many Products
// Use a junction table (CartItem) to connect them
public class Cart {
    public ICollection<CartItem> CartItems { get; set; } = [];
}

public class Product {
    public ICollection<CartItem> CartItems { get; set; } = [];
}

public class CartItem {
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    // Additional data specific to this relationship
    public int Quantity { get; set; }
}
```
**Key Points:**
- Junction table has FKs to both entities
- Allows you to store relationship-specific data (like Quantity)
- Configure relationships in DbContext's `OnModelCreating`

---

### 2. **Primary Key Strategies**

#### Simple Primary Key (Recommended for Most Cases)
```csharp
public class OrderItem {
    public Guid Id { get; set; }  // Simple PK
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
}

// In DbContext:
modelBuilder.Entity<OrderItem>()
    .HasIndex(oi => new { oi.OrderId, oi.ProductId })
    .IsUnique(); // Prevent duplicates
```
**Pros:**
- Easy to reference in code
- Simple to understand
- Flexible for future changes

**Cons:**
- Uses extra storage space
- Requires unique index to prevent duplicates

#### Composite Primary Key
```csharp
public class UserRole {
    public Guid UserId { get; set; }  // Part of PK
    public int RoleId { get; set; }   // Part of PK
}

// In DbContext:
modelBuilder.Entity<UserRole>()
    .HasKey(ur => new { ur.UserId, ur.RoleId });
```
**Pros:**
- Natural key (no artificial Id needed)
- Inherently prevents duplicates
- Saves storage space

**Cons:**
- Harder to reference in relationships
- More complex to work with in code

**When to Use Which?**
- Use **Composite Key** for pure junction tables with no extra data (like UserRole)
- Use **Simple Key** for junction tables with extra data (like CartItem with Quantity)

---

### 3. **Data Snapshots - Critical for E-Commerce**

#### Why Snapshot Prices?
```csharp
public class OrderItem {
    public decimal UnitPrice { get; set; }  // Price at order time
    public int Quantity { get; set; }
}
```

**Problem Without Snapshots:**
1. Customer orders product for $20
2. You change product price to $25
3. Historical order now shows wrong price!

**Solution - Snapshot at Order Time:**
```csharp
// When creating order from cart
orderItem.UnitPrice = product.Price; // Capture current price
orderItem.Quantity = cartItem.Quantity;
```

**What to Snapshot:**
- ✅ Product prices in OrderItem
- ✅ Total order amount
- ✅ Tax rates (if they change)
- ✅ Shipping costs
- ✅ Discount percentages

---

### 4. **Enums for Type Safety**

#### String Status (Bad)
```csharp
public string Status { get; set; } = "Pending"; // Typo-prone!

// Elsewhere in code:
if (order.Status == "Pendin") // Typo won't be caught!
```

#### Enum Status (Good)
```csharp
public enum OrderStatus {
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

public class Order {
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
}

// Elsewhere in code:
if (order.Status == OrderStatus.Pending) // Compile-time safety!
```

**Benefits:**
- Autocomplete in IDE
- Compile-time checking (no typos)
- Can't assign invalid values
- Easy to refactor

**Database Storage:**
- EF Core stores as integer by default (0=Pending, 1=Confirmed, etc.)
- Can configure to store as string if needed

---

### 5. **Delete Behaviors - Understanding Cascade**

#### DeleteBehavior.Cascade
```csharp
modelBuilder.Entity<Cart>()
    .HasOne(c => c.User)
    .WithMany(u => u.Carts)
    .OnDelete(DeleteBehavior.Cascade);
```
**Meaning:** When User deleted → All their Carts automatically deleted

**Use When:**
- Child can't exist without parent
- Child data is not important to keep
- Example: Cart (temporary), UserRole (assignments)

#### DeleteBehavior.Restrict
```csharp
modelBuilder.Entity<Order>()
    .HasOne(o => o.User)
    .WithMany(u => u.Orders)
    .OnDelete(DeleteBehavior.Restrict);
```
**Meaning:** Can't delete User if they have Orders

**Use When:**
- Need to preserve historical data
- Child records are important (legal, accounting)
- Example: Orders, Transactions, Audit logs

#### DeleteBehavior.SetNull
```csharp
// Make FK nullable first
public Guid? CategoryId { get; set; }

modelBuilder.Entity<Product>()
    .HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .OnDelete(DeleteBehavior.SetNull);
```
**Meaning:** When Category deleted → Product.CategoryId becomes null

**Use When:**
- Relationship is optional
- Want to keep child but remove association

#### DeleteBehavior.NoAction
**Meaning:** Don't do anything, rely on database constraints

**Use When:**
- Want database to handle it
- Custom deletion logic needed

**Your E-Commerce Rules:**
```
User → Cart: Cascade (carts are temporary)
User → Order: Restrict (keep order history)
User → UserRole: Cascade (role assignments not needed)

Cart → CartItem: Cascade (items belong to cart)
Order → OrderItem: Cascade (items belong to order)

Product → CartItem: Restrict (can't delete product in carts)
Product → OrderItem: Cascade (keep historical data even if product deleted)
Category → Product: Restrict (reassign products first)
Role → UserRole: Restrict (can't delete role in use)
```

---

### 6. **Indexing for Performance**

#### Why Index?
Without index: Database scans every row (slow with millions of records)
With index: Database uses fast lookup structure (like a book index)

#### What to Index

**Foreign Keys (Most Important)**
```csharp
modelBuilder.Entity<Product>()
    .HasIndex(p => p.CategoryId); // Fast "get all products in category"
```
EF Core often creates these automatically, but explicit is better.

**Unique Constraints**
```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique(); // Fast login lookup + prevents duplicates
```

**Frequently Queried Columns**
```csharp
modelBuilder.Entity<Product>()
    .HasIndex(p => p.Price); // Fast price range queries

modelBuilder.Entity<Order>()
    .HasIndex(o => o.OrderDate); // Fast date range queries
```

#### Single vs Composite Indexes

**Single Index (Recommended)**
```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Username).IsUnique();
    
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email).IsUnique();
```
**Use for:** Independent lookups (search by username OR email)

**Composite Index**
```csharp
modelBuilder.Entity<Order>()
    .HasIndex(o => new { o.UserId, o.OrderDate });
```
**Use for:** Queries that always filter by multiple columns together

**Index Rule of Thumb:**
- Index all foreign keys
- Index unique columns (email, username)
- Index columns used in WHERE clauses
- Index columns used in ORDER BY
- Don't over-index (slows down INSERT/UPDATE)

---

### 7. **Decimal Precision for Money**

#### Wrong - Float/Double (Never for Money!)
```csharp
public double Price { get; set; } // ❌ Precision errors!
// 0.1 + 0.2 = 0.30000000000000004
```

#### Correct - Decimal
```csharp
public decimal Price { get; set; }

// In DbContext - specify precision
modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .HasPrecision(18, 2); // 18 total digits, 2 after decimal
```

**Precision Explained:**
- `HasPrecision(18, 2)` means: `9999999999999999.99`
- 18 total digits
- 2 decimal places
- Range: -999,999,999,999,999.99 to +999,999,999,999,999.99

**Apply to All Money Fields:**
- Product.Price
- Order.TotalAmount
- OrderItem.UnitPrice
- Payment.Amount
- Discount amounts

---

### 8. **Audit Trail with BaseEntity**

```csharp
public abstract class BaseEntity {
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; } = false;  // Soft delete
    public DateTime? DeletedAt { get; set; }
}

// Inherit in entities
public class Product : BaseEntity { ... }
```

**Benefits:**
- Track when records created/modified
- Track who made changes (audit trail)
- Soft delete (mark as deleted without removing)
- Comply with data retention regulations
- Debug production issues ("What changed?")

**Soft Delete Benefits:**
- Can "undelete" if mistake
- Preserve historical references
- Meet legal requirements
- Analyze deleted data

**Implementation Tip:**
```csharp
// In SaveChanges override
public override int SaveChanges() {
    foreach (var entry in ChangeTracker.Entries<BaseEntity>()) {
        if (entry.State == EntityState.Modified) {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
    return base.SaveChanges();
}
```

---

### 9. **Business Logic Separation**

#### Wrong - Logic in Entity
```csharp
public class Product {
    private int _stock;
    public int Stock {
        get => _stock;
        set {
            _stock = value < 0 ? 0 : value;
            if (_stock <= 0) IsAvailable = false; // Business logic here!
        }
    }
}
```

**Problems:**
- EF Core might bypass setters when loading from DB
- Hard to test
- Violates Single Responsibility Principle
- Can't easily change logic

#### Correct - Logic in Service
```csharp
// Entity - simple data
public class Product {
    public int Stock { get; set; }
    public bool IsAvailable { get; set; }
}

// Service - business logic
public class ProductService {
    public void UpdateStock(Product product, int newStock) {
        product.Stock = Math.Max(0, newStock);
        product.IsAvailable = product.Stock > 0;
        product.UpdatedAt = DateTime.UtcNow;
    }
}
```

**Benefits:**
- Entities are pure data containers
- Easy to test business logic
- Can change logic without touching entity
- Follows Domain-Driven Design principles

---

### 10. **Navigation Properties - Both Directions**

#### Wrong - One-way Navigation
```csharp
public class User {
    // Missing: public ICollection<Order> Orders { get; set; }
}

public class Order {
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```

#### Correct - Two-way Navigation
```csharp
public class User {
    public ICollection<Order> Orders { get; set; } = []; // ✅
}

public class Order {
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
```

**Why Both Directions?**
```csharp
// Easy querying in both directions
var user = await context.Users
    .Include(u => u.Orders) // Load user's orders
    .FirstAsync(u => u.Id == userId);

var order = await context.Orders
    .Include(o => o.User) // Load order's user
    .FirstAsync(o => o.Id == orderId);
```

**Navigation Property Rules:**
- Parent has `ICollection<Child>`
- Child has single `Parent` property
- Child has foreign key (e.g., `ParentId`)
- Configure relationship in `OnModelCreating`

---

## 🎯 Key Takeaways

1. **Relationships:** One-to-Many (most common), Many-to-Many (needs junction table)
2. **Primary Keys:** Simple for flexibility, Composite for natural keys
3. **Snapshots:** Always capture prices/totals at transaction time
4. **Enums:** Use for status fields (compile-time safety)
5. **Delete Behavior:** Cascade for temporary data, Restrict for historical data
6. **Indexes:** Foreign keys + frequently queried columns
7. **Decimals:** Always use decimal for money, specify precision
8. **Audit Trail:** BaseEntity pattern for tracking changes
9. **Business Logic:** Keep it in services, not entities
10. **Navigation:** Always bidirectional for easy querying

---

## 📖 Further Learning

### Topics to Study Next:
- **Eager vs Lazy Loading** - When to use Include()
- **Query Optimization** - Avoiding N+1 query problem
- **Migrations** - Managing schema changes
- **Seeding Data** - Initial data for development/testing
- **Global Query Filters** - Auto-filter soft-deleted records
- **Value Objects** - Money, Address as value types
- **Domain Events** - Responding to entity changes
- **Repository Pattern** - Abstracting data access
- **Unit of Work** - Transaction management
- **CQRS** - Separating reads from writes

### Recommended Resources:
- Entity Framework Core Documentation
- Domain-Driven Design (DDD) by Eric Evans
- Clean Architecture by Robert C. Martin
- SQL Performance Explained by Markus Winand
