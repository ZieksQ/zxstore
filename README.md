# E-Commerce Backend API

this backend API i'm building is about either anime or pc shop theme e-commerce website. since I have previously
learned authentication and authorization and JWT Refresh and Access Tokens, now I wanted to create a project
and this really fits the authentication and autorization thing, but it is not a small or for review project
just like what I did before. This will take a long time for me to build and even long time if I have to
refactor things.

## About

- I will leave this blank for now

## The Plan

My Initial Plan is to build A Backend for it using Minimal API but I will consider using WebApi since this is
kind of Mid to Large Project.

- Web APIs (Maybe)
- JWT

### Features

1. Login & Sign up
2. Products
3. Add to Cart
4. Checkout
5. User Information - such as name, mobile number, address, etc.

- optional: order tracking

## Structure

```tree
SRC
|- /Controllers
|- /Entities
|- /DTOs
|- /Services
|- /Interfaces
|- /Data
|- /Middleware
|- /Auth
```

### Others

- **Error Handling**: Global Exception
- **Validation**: Data Annotation (Fluent Validation maybe?)
- **Password Hashing**: BCrypt
- **Rate Limiting**: Optional or Before Hosting
