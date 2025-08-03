# RBAC API - Role-Based Access Control API

A .NET 8 Web API implementing role-based access control for product and order management.

## Features

- **Authentication & Authorization**: JWT-based authentication with Microsoft Identity
- **Role-Based Access Control**: Three roles (User, Admin, SuperAdmin) with different permissions
- **Product Management**: CRUD operations with approval workflow
- **Order Management**: Users can place orders, admins can manage order status
- **PostgreSQL Database**: Entity Framework Core with PostgreSQL provider

## Roles & Permissions

### User
- Register and login
- View approved products
- Place orders
- View own orders
- Cancel own orders

### Admin
- All User permissions
- Create products (requires SuperAdmin approval)
- Edit own products
- Delete own products
- View all orders
- Update order status

### SuperAdmin
- All Admin permissions
- Approve/reject pending products
- Edit/delete any products
- Full product management

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Products
- `GET /api/products` - Get all approved products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create product (Admin+)
- `PUT /api/products/{id}` - Update product (Admin+)
- `DELETE /api/products/{id}` - Delete product (Admin+)
- `GET /api/products/pending` - Get pending products (SuperAdmin only)
- `POST /api/products/{id}/approve` - Approve/reject product (SuperAdmin only)

### Orders
- `GET /api/orders` - Get orders (own orders for User, all for Admin+)
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create order
- `PUT /api/orders/{id}/status` - Update order status (Admin+)
- `DELETE /api/orders/{id}` - Cancel order

## Setup

### Prerequisites
- .NET 8 SDK
- PostgreSQL server

### Configuration

1. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=RBACApiDb;Username=postgres;Password=postgres"
  }
}
```

2. Update JWT settings in `appsettings.json`:
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()",
    "Issuer": "RBACApi",
    "Audience": "RBACApiUsers",
    "ExpiryInHours": "24"
  }
}
```

### Database Setup

1. Apply migrations:
```bash
dotnet ef database update
```

### Running the Application

```bash
dotnet run
```

The API will be available at `https://localhost:7071` (or check the console output).

## Default Accounts

Default accounts are created on startup:

### SuperAdmin
- **Email**: superadmin@rbac.com
- **Password**: SuperAdmin123!

### Admin Accounts
- **Admin 1**: admin1@rbac.com / Admin123!
- **Admin 2**: admin2@rbac.com / Admin123!

## Product Workflow

1. **Admin creates product** → Status: Pending
2. **SuperAdmin approves/rejects** → Status: Approved/Rejected
3. **Users can only see approved products**
4. **Users can order approved products**

## Testing with Swagger

Navigate to `/swagger` when running in development mode to test the API endpoints interactively.

## Security Features

- JWT token-based authentication
- Role-based authorization policies
- Password complexity requirements
- Secure password storage with Identity
- Input validation with data annotations
- SQL injection prevention with Entity Framework

## Technologies Used

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Microsoft Identity
- JWT Authentication
- Swagger/OpenAPI