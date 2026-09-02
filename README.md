# OrderManagementApi

A Order Management REST API built with .NET 8, ASP.NET Core Web API, Entity Framework Core, and SQL Server.

The API allows administrators to manage products and inventory, while authenticated customers can place orders.

## Architecture

The application follows a simple layered structure:

```text
OrderManagement.Api
│
├── Controllers    -> HTTP endpoints
├── Services       -> Business logic
├── Models         -> Database entities
├── Data           -> EF Core DbContext
├── Migrations     -> EF Core database migrations
└── Program.cs     -> Application configuration

The Request flow is:

Client
  ↓
Controller
  ↓
Service
  ↓
Entity Framework Core
  ↓
SQL Server

Technologies
.NET 8
ASP.NET Core Web API
C#
Entity Framework Core
SQL Server
JWT Authentication
Swagger

## API Endpoints

### Authentication

**Login**

```http
POST /api/Auth/login

Example:

{
  "username": "admin",
  "password": "admin123"
}

The login returns a JWT token which is required for protected APIs.

Products
GET    /api/Products
GET    /api/Products/{id}
POST   /api/Products
PUT    /api/Products/{id}
DELETE /api/Products/{id}

Product creation, update, and delete operations require Admin access.

Orders
POST /api/Orders

An order requires an Idempotency-Key header.
Example:

Idempotency-Key: Order-001
Request body:
[
  {
    "productId": 1,
    "quantity": 2
  }
]

The API checks stock, creates the order, calculates the total, and updates inventory.

Database: SQL Server is used as the relational database with Entity Framework Core.

Main tables/entities:

Users
Products
Orders
OrderItems

The main relationship is:

User
 └── Orders
      └── OrderItems
           └── Product
Run Locally
1. .NET 8 SDK, SQL Server, Visual Studio, SQL Server Management Studio
2. Configure the database

Open: appsettings.json
Update the SQL Server connection string according to your local SQL Server setup.

For example:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
3. Build the project
dotnet build
4. Apply the database migration
dotnet ef database update

If a new migration is required, create one first:
dotnet ef migrations add InitialCreate

Then apply it:
dotnet ef database update
5. Run the API
dotnet run
The API will start on the URL For example:https://localhost:7228
