# Service Layer Architecture - Complete Lessons & Code Review

## 📋 Summary of All Lessons Learned

### ✅ What You Did RIGHT

1. **No Repository Pattern** - Correctly chose to use DbContext directly instead of adding unnecessary abstraction
2. **Service Interfaces** - Created clean interfaces for testability and dependency injection
3. **Dependency Injection** - Properly injected dependencies (DbContext, ILogger, other services)
4. **AsNoTracking()** - Used for read-only queries to improve performance
5. **Include()** - Loaded navigation properties when needed
6. **DTOs** - Created separate DTOs with validation attributes
7. **Service Dependencies** - ProductService correctly depends on ICategoryService
8. **Scoped Lifetime** - Registered services as Scoped (correct for DbContext-dependent services)

---

## 🐛 Critical Issues Found (From Code Review)

### 1. **Race Condition in Stock Updates** ⚠️ CRITICAL
**Location:** `ProductService.UpdateProductStockAsync()` (lines 127-139)

**Problem:** Multiple concurrent requests can cause lost updates

**Your Current Code:**
```csharp
public async Task<bool> UpdateProductStockAsync(Guid id, int quantity)
{
  var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
  if (product is null) return false;
  
  product.Stock += quantity; // ❌ RACE CONDITION!
  await _context.SaveChangesAsync();
  return true;
}
```

**What Happens:**
```
Time  | Request A (Add 5)           | Request B (Add 3)           | Stock Value
------|----------------------------|-----------------------------|-----------
T1    | Read stock = 10            |                             | 10
T2    |                            | Read stock = 10             | 10
T3    | Calculate: 10 + 5 = 15     |                             | 10
T4    |                            | Calculate: 10 + 3 = 13      | 10
T5    | Save: stock = 15           |                             | 15
T6    |                            | Save: stock = 13            | 13 ❌ WRONG! Should be 18
```

**Solution:** Use atomic database operations
```csharp
public async Task<bool> UpdateProductStockAsync(Guid id, int quantity)
{
  // ✅ Atomic operation - no race condition
  var rowsAffected = await _context.Products
    .Where(p => p.Id == id)
    .ExecuteUpdateAsync(setters => setters
      .SetProperty(p => p.Stock, p => p.Stock + quantity)
      .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
  
  return rowsAffected > 0;
}
```

---

### 2. **Deleting Products Destroys Order History** ⚠️ CRITICAL
**Location:** `ProductService.DeleteProductAsync()` (lines 107-125)

**Problem:** Your code deletes products that exist in orders, destroying historical data

**Your Current Code:**
```csharp
public async Task<bool> DeleteProductAsync(Guid id)
{
  var product = await _context.Products
    .Include(p => p.CartItems)
    .Include(p => p.OrderItems) // ❌ You include OrderItems but don't check them!
    .FirstOrDefaultAsync(p => p.Id == id);
  
  if (product is null) return false;
  
  _context.CartItems.RemoveRange(product.CartItems);
  _context.Products.Remove(product); // ❌ CASCADE DELETE destroys OrderItems!
  
  await _context.SaveChangesAsync();
  return true;
}
```

**What Your AppDbContext Says:**
```csharp
// AppDbContext.cs line 88-92
modelBuilder.Entity<OrderItem>()
  .HasOne(oi => oi.Product)
  .WithMany(p => p.OrderItems)
  .HasForeignKey(oi => oi.ProductId)
  .OnDelete(DeleteBehavior.Cascade); // ❌ This DELETES order history!
```

**Solutions:**

**Option 1: Prevent Deletion (Recommended for Orders)**
```csharp
public async Task<bool> DeleteProductAsync(Guid id)
{
  var product = await _context.Products
    .Include(p => p.CartItems)
    .Include(p => p.OrderItems)
    .FirstOrDefaultAsync(p => p.Id == id);
  
  if (product is null) return false;
  
  // ✅ Business Rule: Cannot delete products in orders
  if (product.OrderItems.Any())
    throw new InvalidOperationException(
      "Cannot delete product with existing orders. Use soft delete instead.");
  
  _context.CartItems.RemoveRange(product.CartItems);
  _context.Products.Remove(product);
  await _context.SaveChangesAsync();
  return true;
}
```

**Option 2: Soft Delete (Best Practice for E-Commerce)**
```csharp
// Add to Product entity
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }

// Service method
public async Task<bool> DeleteProductAsync(Guid id)
{
  var product = await _context.Products.FindAsync(id);
  if (product is null) return false;
  
  // ✅ Soft delete - keeps historical data
  product.IsDeleted = true;
  product.DeletedAt = DateTime.UtcNow;
  await _context.SaveChangesAsync();
  return true;
}

// Add global query filter in AppDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
  modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted); // ✅ Auto-exclude deleted products
}
```

**Option 3: Change Delete Behavior**
```csharp
// In AppDbContext.cs - change line 88-92
modelBuilder.Entity<OrderItem>()
  .HasOne(oi => oi.Product)
  .WithMany(p => p.OrderItems)
  .HasForeignKey(oi => oi.ProductId)
  .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents deletion if OrderItems exist
```

---

### 3. **Premature Logging** ⚠️ MEDIUM
**Location:** `ProductService.CreateProductAsync()` (line 79)

**Problem:** Logs "Product created" BEFORE database save

**Your Code:**
```csharp
_logger.LogInformation("Product created"); // ❌ Line 79
_context.Products.Add(product);            // Line 80
await _context.SaveChangesAsync();         // Line 81 - might fail!
```

**Fix:**
```csharp
_context.Products.Add(product);
await _context.SaveChangesAsync();
_logger.LogInformation("Product created: {ProductId}", product.Id); // ✅ After success
```

---

### 4. **Stock Can Go Negative** ⚠️ HIGH
**Location:** `ProductService.UpdateProductStockAsync()` (line 134)

**Problem:** No validation that stock won't become negative

**Your Code:**
```csharp
product.Stock += quantity; // ❌ What if Stock=5 and quantity=-10?
```

**Fix:**
```csharp
var newStock = product.Stock + quantity;
if (newStock < 0)
  throw new InvalidOperationException(
    $"Insufficient stock. Available: {product.Stock}, Requested: {Math.Abs(quantity)}");

product.Stock = newStock;
```

---

### 5. **Search Without Validation** ⚠️ MEDIUM
**Location:** `ProductService.SearchProductsAsync()` (line 46-54)

**Problem:** Null searchTerm will throw NullReferenceException

**Your Code:**
```csharp
public async Task<IEnumerable<Product?>> SearchProductsAsync(string searchTerm)
{
  return await _context.Products
    .Where(p => p.Name.Contains(searchTerm)) // ❌ NullReferenceException if searchTerm is null
    ...
}
```

**Fix:**
```csharp
public async Task<IEnumerable<Product?>> SearchProductsAsync(string searchTerm)
{
  if (string.IsNullOrWhiteSpace(searchTerm))
    return Enumerable.Empty<Product>(); // ✅ Return empty list
  
  return await _context.Products
    .Where(p => p.Name.Contains(searchTerm) || 
                (p.Description != null && p.Description.Contains(searchTerm)))
    .Include(p => p.Category)
    .ToListAsync();
}
```

---

## 📚 Complete Tutorial with DIFFERENT Examples

Let's use **Order Management** instead of Product/Category to reinforce learning.

### Example 1: Order Service (Different Entity)

**Entity:**
```csharp
public class Order : BaseEntity
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public User User { get; set; } = null!;
  public DateTime OrderDate { get; set; }
  public OrderStatus Status { get; set; }
  public decimal TotalAmount { get; set; }
  public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public enum OrderStatus
{
  Pending,
  Processing,
  Shipped,
  Delivered,
  Cancelled
}
```

**Interface:**
```csharp
namespace ZxStore.Api.Interfaces;

public interface IOrderService
{
  Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
  Task<Order?> GetOrderByIdAsync(Guid orderId);
  Task<Order> CreateOrderAsync(Guid userId, List<OrderItemDto> items);
  Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);
  Task<bool> CancelOrderAsync(Guid orderId);
  Task<decimal> GetOrderTotalAsync(Guid orderId);
}
```

**Service Implementation:**
```csharp
using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.Entities;
using ZxStore.Api.Interfaces;

namespace ZxStore.Api.Services;

public class OrderService : IOrderService
{
  private readonly AppDbContext _context;
  private readonly ILogger<OrderService> _logger;
  private readonly IProductService _productService;

  public OrderService(
    AppDbContext context, 
    ILogger<OrderService> logger,
    IProductService productService)
  {
    _context = context;
    _logger = logger;
    _productService = productService;
  }

  // ✅ LESSON: Use AsNoTracking() for read-only queries
  public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
  {
    _logger.LogInformation("Fetching orders for user: {UserId}", userId);
    
    return await _context.Orders
      .AsNoTracking() // ✅ Performance optimization
      .Where(o => o.UserId == userId)
      .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product) // ✅ Load nested navigation properties
      .OrderByDescending(o => o.OrderDate)
      .ToListAsync();
  }

  public async Task<Order?> GetOrderByIdAsync(Guid orderId)
  {
    return await _context.Orders
      .AsNoTracking()
      .Include(o => o.User)
      .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
      .FirstOrDefaultAsync(o => o.Id == orderId);
  }

  // ✅ LESSON: Services contain BUSINESS LOGIC
  public async Task<Order> CreateOrderAsync(Guid userId, List<OrderItemDto> items)
  {
    _logger.LogInformation("Creating order for user: {UserId}", userId);

    // 🎯 VALIDATION: Check if items list is empty
    if (items == null || !items.Any())
      throw new ArgumentException("Order must contain at least one item");

    // 🎯 BUSINESS LOGIC: Validate stock availability for all items
    foreach (var item in items)
    {
      var product = await _productService.GetProductByIdAsync(item.ProductId);
      
      if (product == null)
        throw new ArgumentException($"Product {item.ProductId} not found");
      
      if (product.Stock < item.Quantity)
        throw new InvalidOperationException(
          $"Insufficient stock for {product.Name}. Available: {product.Stock}, Requested: {item.Quantity}");
    }

    // 🎯 BUSINESS LOGIC: Calculate total amount
    decimal totalAmount = 0;
    var orderItems = new List<OrderItem>();

    foreach (var item in items)
    {
      var product = await _context.Products.FindAsync(item.ProductId);
      var itemTotal = product!.Price * item.Quantity;
      totalAmount += itemTotal;

      orderItems.Add(new OrderItem
      {
        ProductId = item.ProductId,
        Quantity = item.Quantity,
        UnitPrice = product.Price // ✅ Snapshot price at order time
      });
    }

    // Create order
    var order = new Order
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      OrderDate = DateTime.UtcNow,
      Status = OrderStatus.Pending,
      TotalAmount = totalAmount,
      OrderItems = orderItems
    };

    // 🎯 TRANSACTION: Multiple database operations
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      _context.Orders.Add(order);
      
      // Update stock for each product
      foreach (var item in items)
      {
        await _productService.UpdateProductStockAsync(item.ProductId, -item.Quantity);
      }
      
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();
      
      _logger.LogInformation("Order created: {OrderId}", order.Id); // ✅ Log after success
      return order;
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      _logger.LogError(ex, "Failed to create order for user: {UserId}", userId);
      throw;
    }
  }

  // ✅ LESSON: State machine validation
  public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
  {
    var order = await _context.Orders.FindAsync(orderId);
    if (order == null) return false;

    // 🎯 BUSINESS RULE: Validate status transitions
    var validTransitions = order.Status switch
    {
      OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
      OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
      OrderStatus.Shipped => new[] { OrderStatus.Delivered },
      OrderStatus.Delivered => Array.Empty<OrderStatus>(), // ❌ Final state
      OrderStatus.Cancelled => Array.Empty<OrderStatus>(), // ❌ Final state
      _ => Array.Empty<OrderStatus>()
    };

    if (!validTransitions.Contains(newStatus))
      throw new InvalidOperationException(
        $"Cannot transition from {order.Status} to {newStatus}");

    order.Status = newStatus;
    order.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    _logger.LogInformation("Order {OrderId} status updated: {OldStatus} -> {NewStatus}",
      orderId, order.Status, newStatus);
    
    return true;
  }

  // ✅ LESSON: Complex business logic with transaction rollback
  public async Task<bool> CancelOrderAsync(Guid orderId)
  {
    var order = await _context.Orders
      .Include(o => o.OrderItems)
      .FirstOrDefaultAsync(o => o.Id == orderId);

    if (order == null) return false;

    // 🎯 BUSINESS RULE: Can only cancel Pending or Processing orders
    if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Processing)
      throw new InvalidOperationException(
        $"Cannot cancel order with status: {order.Status}");

    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      // Return stock to products
      foreach (var item in order.OrderItems)
      {
        await _productService.UpdateProductStockAsync(item.ProductId, item.Quantity);
      }

      order.Status = OrderStatus.Cancelled;
      order.UpdatedAt = DateTime.UtcNow;
      
      await _context.SaveChangesAsync();
      await transaction.CommitAsync();

      _logger.LogInformation("Order cancelled: {OrderId}", orderId);
      return true;
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      _logger.LogError(ex, "Failed to cancel order: {OrderId}", orderId);
      throw;
    }
  }

  public async Task<decimal> GetOrderTotalAsync(Guid orderId)
  {
    var order = await _context.Orders
      .AsNoTracking()
      .FirstOrDefaultAsync(o => o.Id == orderId);
    
    return order?.TotalAmount ?? 0;
  }
}
```

---

### Example 2: Cart Service (Another Different Entity)

**Interface:**
```csharp
namespace ZxStore.Api.Interfaces;

public interface ICartService
{
  Task<Cart?> GetActiveCartAsync(Guid userId);
  Task<Cart> AddItemToCartAsync(Guid userId, Guid productId, int quantity);
  Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity);
  Task<bool> RemoveItemFromCartAsync(Guid userId, Guid productId);
  Task<bool> ClearCartAsync(Guid userId);
  Task<int> GetCartItemCountAsync(Guid userId);
}
```

**Service:**
```csharp
using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.Entities;
using ZxStore.Api.Interfaces;

namespace ZxStore.Api.Services;

public class CartService : ICartService
{
  private readonly AppDbContext _context;
  private readonly ILogger<CartService> _logger;
  private readonly IProductService _productService;

  public CartService(
    AppDbContext context,
    ILogger<CartService> logger,
    IProductService productService)
  {
    _context = context;
    _logger = logger;
    _productService = productService;
  }

  // ✅ LESSON: Helper method to get or create cart
  private async Task<Cart> GetOrCreateCartAsync(Guid userId)
  {
    var cart = await _context.Carts
      .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
      .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null)
    {
      cart = new Cart
      {
        Id = Guid.NewGuid(),
        UserId = userId
      };
      _context.Carts.Add(cart);
      await _context.SaveChangesAsync();
      
      _logger.LogInformation("Created new cart for user: {UserId}", userId);
    }

    return cart;
  }

  public async Task<Cart?> GetActiveCartAsync(Guid userId)
  {
    return await _context.Carts
      .AsNoTracking()
      .Include(c => c.CartItems)
        .ThenInclude(ci => ci.Product)
          .ThenInclude(p => p.Category) // ✅ Multiple levels of Include
      .FirstOrDefaultAsync(c => c.UserId == userId);
  }

  // ✅ LESSON: Complex business logic with validation
  public async Task<Cart> AddItemToCartAsync(Guid userId, Guid productId, int quantity)
  {
    // 🎯 VALIDATION
    if (quantity <= 0)
      throw new ArgumentException("Quantity must be greater than zero");

    var product = await _productService.GetProductByIdAsync(productId);
    if (product == null)
      throw new ArgumentException("Product not found");

    if (product.Stock < quantity)
      throw new InvalidOperationException(
        $"Insufficient stock. Available: {product.Stock}");

    var cart = await GetOrCreateCartAsync(userId);

    // Check if item already exists in cart
    var existingItem = cart.CartItems
      .FirstOrDefault(ci => ci.ProductId == productId);

    if (existingItem != null)
    {
      // 🎯 BUSINESS LOGIC: Update existing item quantity
      var newQuantity = existingItem.Quantity + quantity;
      
      if (product.Stock < newQuantity)
        throw new InvalidOperationException(
          $"Cannot add more items. Available: {product.Stock}, In cart: {existingItem.Quantity}");

      existingItem.Quantity = newQuantity;
      _logger.LogInformation("Updated cart item quantity: {ProductId}, New: {Quantity}", 
        productId, newQuantity);
    }
    else
    {
      // Add new item to cart
      var cartItem = new CartItem
      {
        CartId = cart.Id,
        ProductId = productId,
        Quantity = quantity
      };
      cart.CartItems.Add(cartItem);
      
      _logger.LogInformation("Added item to cart: {ProductId}, Quantity: {Quantity}", 
        productId, quantity);
    }

    await _context.SaveChangesAsync();
    
    // ✅ LESSON: Reload to get updated navigation properties
    return await GetActiveCartAsync(userId) ?? cart;
  }

  public async Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity)
  {
    // 🎯 VALIDATION
    if (quantity < 0)
      throw new ArgumentException("Quantity cannot be negative");

    if (quantity == 0)
      return await RemoveItemFromCartAsync(userId, productId);

    var cart = await _context.Carts
      .Include(c => c.CartItems)
      .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null) return false;

    var cartItem = cart.CartItems
      .FirstOrDefault(ci => ci.ProductId == productId);

    if (cartItem == null) return false;

    // 🎯 VALIDATION: Check stock
    var product = await _productService.GetProductByIdAsync(productId);
    if (product!.Stock < quantity)
      throw new InvalidOperationException(
        $"Insufficient stock. Available: {product.Stock}");

    cartItem.Quantity = quantity;
    await _context.SaveChangesAsync();

    _logger.LogInformation("Updated cart item: {ProductId}, Quantity: {Quantity}", 
      productId, quantity);
    
    return true;
  }

  public async Task<bool> RemoveItemFromCartAsync(Guid userId, Guid productId)
  {
    var cart = await _context.Carts
      .Include(c => c.CartItems)
      .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null) return false;

    var cartItem = cart.CartItems
      .FirstOrDefault(ci => ci.ProductId == productId);

    if (cartItem == null) return false;

    cart.CartItems.Remove(cartItem);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Removed item from cart: {ProductId}", productId);
    return true;
  }

  public async Task<bool> ClearCartAsync(Guid userId)
  {
    var cart = await _context.Carts
      .Include(c => c.CartItems)
      .FirstOrDefaultAsync(c => c.UserId == userId);

    if (cart == null) return false;

    _context.CartItems.RemoveRange(cart.CartItems);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Cleared cart for user: {UserId}", userId);
    return true;
  }

  public async Task<int> GetCartItemCountAsync(Guid userId)
  {
    var count = await _context.Carts
      .AsNoTracking()
      .Where(c => c.UserId == userId)
      .SelectMany(c => c.CartItems)
      .SumAsync(ci => ci.Quantity);

    return count;
  }
}
```

---

## 🎯 Key Patterns & Best Practices Summary

### 1. **Service Structure**
```
✅ Interface defines contract
✅ Service implements business logic
✅ Inject dependencies (DbContext, ILogger, other services)
✅ Register as Scoped in Program.cs
```

### 2. **EF Core Query Patterns**
```csharp
// Read-only
.AsNoTracking()

// Load related data
.Include(p => p.Category)
.ThenInclude(c => c.SubCategory)

// Filtering
.Where(p => p.Price > 100)

// Ordering
.OrderBy(p => p.Name)
.OrderByDescending(p => p.CreatedAt)

// Get single or null
.FirstOrDefaultAsync(p => p.Id == id)

// Check existence
.AnyAsync(p => p.Id == id)

// Count
.CountAsync()

// Select specific fields (projection)
.Select(p => new { p.Id, p.Name })
```

### 3. **Business Logic Locations**
```
✅ Services: Validation, business rules, complex logic
❌ Controllers: Only HTTP handling, routing
❌ Entities: Only data structure
✅ DTOs: Input validation attributes
```

### 4. **Transaction Patterns**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
  // Multiple operations
  await _context.SaveChangesAsync();
  await transaction.CommitAsync(); // ✅ Success
}
catch
{
  await transaction.RollbackAsync(); // ❌ Error - rollback
  throw;
}
```

### 5. **Logging Best Practices**
```csharp
// ✅ DO: Log after success
await _context.SaveChangesAsync();
_logger.LogInformation("Order created: {OrderId}", order.Id);

// ❌ DON'T: Log before database operation
_logger.LogInformation("Creating order"); // Can fail after this
await _context.SaveChangesAsync();

// ✅ DO: Use structured logging with parameters
_logger.LogInformation("User {UserId} created order {OrderId}", userId, orderId);

// ❌ DON'T: String concatenation
_logger.LogInformation($"User {userId} created order {orderId}");
```

### 6. **Validation Patterns**
```csharp
// Null checks
if (entity == null) return null;

// Business rule validation
if (order.Status == OrderStatus.Delivered)
  throw new InvalidOperationException("Cannot modify delivered order");

// Input validation
if (quantity <= 0)
  throw new ArgumentException("Quantity must be positive");

// Existence validation
if (!await _categoryService.CategoryExistsAsync(id))
  throw new ArgumentException("Category does not exist");
```

### 7. **Atomic Operations (Race Condition Prevention)**
```csharp
// ❌ BAD: Race condition
var product = await _context.Products.FindAsync(id);
product.Stock += quantity;
await _context.SaveChangesAsync();

// ✅ GOOD: Atomic update
await _context.Products
  .Where(p => p.Id == id)
  .ExecuteUpdateAsync(setters => setters
    .SetProperty(p => p.Stock, p => p.Stock + quantity));
```

---

## 🚀 Registration in Program.cs

```csharp
using ZxStore.Api.Data;
using ZxStore.Api.Interfaces;
using ZxStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.AddAppDb();

// Services - Register all with Scoped lifetime
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();

var app = builder.Build();

app.MigrateDb();
app.Run();
```

---

## ✅ Checklist for Creating a New Service

- [ ] Create interface in `Interfaces/IXxxService.cs`
- [ ] Implement service in `Services/XxxService.cs`
- [ ] Inject required dependencies (AppDbContext, ILogger, other services)
- [ ] Use `AsNoTracking()` for read-only queries
- [ ] Use `Include()` for navigation properties
- [ ] Put business logic and validation in the service
- [ ] Log after successful operations, not before
- [ ] Use transactions for multiple related operations
- [ ] Handle race conditions for stock/quantity updates
- [ ] Validate input parameters
- [ ] Register service in `Program.cs` as `Scoped`
- [ ] Consider soft delete for important entities
- [ ] Protect historical data (orders, transactions)

---

## 🎓 Final Recommendations for Your Project

1. **Fix the race condition in `UpdateProductStockAsync`** - Use `ExecuteUpdateAsync`
2. **Fix the OrderItem cascade delete** - Change to `Restrict` or use soft delete
3. **Move logging after SaveChangesAsync** in `CreateProductAsync`
4. **Add stock validation** in `UpdateProductStockAsync`
5. **Add null check** in `SearchProductsAsync`
6. **Consider implementing soft delete** for Products (keep in commented code)
7. **Test concurrent operations** especially stock updates

---

**End of Lessons** 🎉
