# Future Features & Entities to Add

## 🔮 Entities Not Yet Implemented

### **1. Address**
- Multiple shipping addresses per user
- Billing address vs shipping address
- Default address selection
- Address validation

```
Properties:
- Id, UserId
- AddressType (Shipping/Billing)
- Street, City, State, ZipCode, Country
- IsDefault
- PhoneNumber
```

---

### **2. ProductReview**
- Customer product ratings
- Review comments
- Review moderation
- Verified purchase indicator

```
Properties:
- Id, ProductId, UserId
- Rating (1-5)
- Title, Comment
- IsVerifiedPurchase
- HelpfulCount
- CreatedAt
```

---

### **3. Wishlist**
- Save products for later
- Wishlist sharing
- Price drop notifications

```
Properties:
- Id, UserId, ProductId
- AddedAt
- NotifyOnPriceDrop
```

---

### **4. Payment**
- Payment method tracking
- Transaction history
- Payment status
- Refund handling

```
Properties:
- Id, OrderId
- PaymentMethod (CreditCard, PayPal, Stripe, etc.)
- Amount, Currency
- Status (Pending, Completed, Failed, Refunded)
- TransactionId, GatewayReference
- ProcessedAt, RefundedAt
```

---

### **5. ProductImage**
- Multiple images per product
- Image ordering
- Alt text for accessibility
- Thumbnail generation

```
Properties:
- Id, ProductId
- ImageUrl
- DisplayOrder
- AltText
- IsPrimary
```

---

### **6. Coupon/Discount**
- Promotional codes
- Percentage or fixed discounts
- Expiration dates
- Usage limits

```
Properties:
- Id
- Code (unique)
- DiscountType (Percentage, Fixed)
- DiscountValue
- MinimumOrderAmount
- MaxUsageCount, CurrentUsageCount
- ValidFrom, ValidUntil
- IsActive
```

---

### **7. OrderDiscount**
- Track which coupons used on orders
- Discount amount applied

```
Properties:
- Id, OrderId, CouponId
- DiscountAmount
- AppliedAt
```

---

### **8. ProductVariant**
- Size, color, material variations
- Separate pricing per variant
- Separate stock per variant

```
Properties:
- Id, ProductId
- Name (e.g., "Large Blue")
- SKU
- Price (override), Stock
- Attributes (JSON: {"size": "L", "color": "blue"})
```

---

### **9. Inventory/StockMovement**
- Track stock changes
- Audit inventory
- Reorder alerts

```
Properties:
- Id, ProductId
- MovementType (Purchase, Sale, Return, Adjustment)
- Quantity (positive or negative)
- Reason, Notes
- CreatedAt, CreatedByUserId
```

---

### **10. ShippingMethod**
- Multiple shipping options
- Carrier tracking
- Shipping cost calculation

```
Properties:
- Id
- Name (Standard, Express, Overnight)
- Carrier (UPS, FedEx, USPS)
- EstimatedDays
- Cost (or calculation logic)
- IsActive
```

---

### **11. Notification**
- Order status updates
- Price drop alerts
- Promotional emails

```
Properties:
- Id, UserId
- Type (Email, SMS, Push)
- Title, Message
- IsRead
- SentAt, ReadAt
```

---

### **12. ReturnRequest**
- Product returns
- Refund processing
- Return reasons

```
Properties:
- Id, OrderId, OrderItemId
- Reason
- Status (Pending, Approved, Rejected, Completed)
- RequestedAt, ProcessedAt
- RefundAmount
```

---

### **13. ProductCategory (Many-to-Many)**
- Products in multiple categories
- Featured categories

```
Properties:
- ProductId, CategoryId
- DisplayOrder
- IsFeatured
```

---

### **14. Tag**
- Flexible product categorization
- Search optimization
- Trending tags

```
Properties:
- Id
- Name
- Slug
- UsageCount
```

---

### **15. ProductTag**
- Associate tags with products

```
Properties:
- ProductId, TagId
```

---

### **16. Subscription**
- Recurring orders
- Auto-renewal
- Subscription plans

```
Properties:
- Id, UserId, ProductId
- BillingCycle (Weekly, Monthly, Yearly)
- NextBillingDate
- Status (Active, Paused, Cancelled)
- StartDate, EndDate
```

---

### **17. GiftCard**
- Store credit
- Gift card purchases
- Balance tracking

```
Properties:
- Id
- Code (unique)
- InitialAmount, CurrentBalance
- IssuedToUserId, PurchasedByUserId
- ExpiresAt
- IsActive
```

---

### **18. VendorStoreOwner**
- Multi-vendor marketplace
- Vendor product management
- Commission tracking

```
Properties:
- Id
- UserId (vendor account)
- StoreName
- Description
- CommissionPercentage
- IsApproved
- CreatedAt
```

---

### **19. ProductQuestion**
- Customer questions on products
- Answers from sellers/support

```
Properties:
- Id, ProductId, UserId
- Question
- Answer (nullable)
- AnsweredByUserId
- AskedAt, AnsweredAt
```

---

### **20. SearchHistory**
- Track user searches
- Improve recommendations
- Analytics

```
Properties:
- Id, UserId (nullable for guests)
- SearchTerm
- ResultCount
- SearchedAt
```

---

## 🎨 Feature Enhancements

### **User Entity Enhancements**
- FirstName, LastName
- PhoneNumber (with verification)
- ProfilePictureUrl
- DateOfBirth
- Gender
- PreferredLanguage, PreferredCurrency
- IsEmailVerified, EmailVerificationToken
- PasswordResetToken, PasswordResetExpires
- LastLoginAt
- AccountStatus (Active, Suspended, Deleted)

### **Product Entity Enhancements**
- SKU (Stock Keeping Unit)
- Barcode, UPC, ISBN
- Brand
- Weight, Dimensions (length, width, height)
- ImageUrl, GalleryImages
- VideoUrl
- IsFeatured, IsNewArrival, IsBestseller
- MetaTitle, MetaDescription, MetaKeywords (SEO)
- AverageRating (calculated field)
- ReviewCount (calculated field)
- ViewCount
- SaleCount
- RelatedProductIds

### **Order Entity Enhancements**
- OrderNumber (human-readable)
- PaymentStatus (Pending, Paid, Refunded)
- ShipmentStatus (Processing, Shipped, InTransit, Delivered)
- ShippingMethodId
- ShippingCost
- TaxAmount
- DiscountAmount
- SubTotal (before tax/shipping)
- GrandTotal (after everything)
- BillingAddressId
- ShippingAddressId
- OrderNotes (customer notes)
- AdminNotes (internal notes)
- IPAddress (fraud prevention)
- CancelledAt, CancelReason

### **Category Entity Enhancements**
- ParentCategoryId (subcategories)
- Description
- ImageUrl
- DisplayOrder
- IsActive, IsFeatured
- Slug (URL-friendly name)
- MetaTitle, MetaDescription

### **Cart Enhancements**
- ExpiresAt (auto-clear old carts)
- SessionId (guest carts)
- SavedForLater flag on CartItem
- AppliedCouponCode

---

## 🚀 Advanced Features

### **1. Real-time Inventory Management**
- Stock alerts when low
- Automatic reordering
- Supplier integration

### **2. Recommendation Engine**
- "Customers also bought"
- "You may also like"
- Based on browsing/purchase history

### **3. Advanced Search**
- Elasticsearch integration
- Faceted search (filters)
- Autocomplete suggestions

### **4. Multi-currency Support**
- Currency conversion
- Regional pricing
- Tax calculations by region

### **5. Loyalty Program**
- Points/rewards system
- Tier-based benefits
- Referral bonuses

### **6. Analytics Dashboard**
- Sales reports
- Customer insights
- Inventory reports
- Revenue tracking

### **7. Email/SMS Marketing**
- Abandoned cart emails
- Order confirmation
- Shipping notifications
- Promotional campaigns

### **8. Social Features**
- Share products on social media
- Social login (Google, Facebook)
- User-generated content

### **9. Multi-language Support**
- Localization
- Translated content
- Regional variations

### **10. Advanced Security**
- Two-factor authentication
- IP blocking
- Fraud detection
- Rate limiting

---

## 📊 Priority Levels

### **High Priority (Core E-Commerce)**
1. Address
2. Payment
3. ProductImage
4. Coupon/Discount
5. ShippingMethod

### **Medium Priority (Enhanced UX)**
6. ProductReview
7. Wishlist
8. Notification
9. ReturnRequest
10. User entity enhancements

### **Low Priority (Advanced Features)**
11. ProductVariant
12. Subscription
13. GiftCard
14. Vendor/Marketplace
15. SearchHistory
16. Advanced features

---

## 💡 Implementation Tips

### **Start Simple**
- Implement core features first
- Test thoroughly before adding complexity
- Get user feedback early

### **Scalability Considerations**
- Plan for large product catalogs
- Consider caching strategies
- Optimize database queries
- Think about microservices later

### **Security First**
- Validate all inputs
- Sanitize user data
- Use parameterized queries
- Implement rate limiting
- Secure payment processing (use established gateways)

### **User Experience**
- Fast page loads
- Mobile-friendly
- Clear error messages
- Simple checkout process
- Good search functionality

---

## 📝 Notes

- Don't implement everything at once
- Focus on MVP (Minimum Viable Product) first
- Gather user feedback
- Iterate based on actual usage
- Performance test with realistic data volumes
- Consider third-party integrations (payment gateways, shipping APIs)
- Document your API as you build
