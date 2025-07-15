# Point of Sale (POS) Server API

A modular, extensible, and production-ready **Point of Sale (POS)** Web API built with **ASP.NET Core (C# 11, .NET 7)** using clean architecture principles, 3-tier separation, and Entity Framework Core. The system supports product management, order processing, soft deletes, image handling, JWT authentication, and is fully tested via Postman.

🚀 **Live Demo**: https://posserverapi.azurewebsites.net/
📮 **Postman Collection**:  
- Online: [Postman Public Workspace](https://www.postman.com/ammar-0/public-workspace-1/collection/y8uul8m/pointofsale)  
- Offline: Available in the `Postman/PointOfSale.postman_collection.json` file

---

## 🔑 Features

- ✅ Modular 3-tier architecture (DAL, BLL, PL)
- 🔐 JWT Authentication & Authorization
- 🖼️ Image Upload/Update/Delete for Products (abstracted for extensibility)
- ♻️ Soft Deletion and Restoration support
- 📊 Pagination and filtering support  
- 🧪 Fully tested via Postman collection
- 🖥️ Ready for local and cloud deployment (Azure-ready)

---

## 🛠 Technologies Used

- **ASP.NET Core 7** with **C# 11**  
- **Entity Framework Core (EF Core)** for ORM and migrations  
- **SQL Server / Azure SQL Server**
- **JWT (JSON Web Tokens)** for security  
- **AutoMapper** for DTO mapping  
- **Postman** for API testing  
- Hosted on **IIS** locally and **Microsoft Azure** in production  
- **LINQ, Async/Await, Dependency Injection**

---

## 🗂️ Repository Structure

This repository includes **3 solutions**:

1. **POS_Server/**  
   Main Web API project containing:
   - `POS_Server_DAL`: Data Access Layer (repositories, EF configurations)  
   - `POS_Server_BLL`: Business Logic Layer (services, validation)  
   - `POS_Server_PL`: Presentation Layer (ASP.NET Core Web API, controllers, DI)

2. **POS_Domains/**  
   Shared models (`ProductModel`, `OrderModel`, etc.) and DTOs used by frontend/backend.

3. **Helper/**  
   Shared utility classes (`OperationResult`, logging, validation helpers, etc.).

Also included:

- **Postman/** — Contains Postman collection JSON file for API testing

---

## 🔐 Authentication & Authorization

All API endpoints are **secured with JWT** (Bearer token), except the `Login` endpoint which is public.

To authenticate:
1. Call the `Login` endpoint to receive a JWT token.
2. Use the token in the `Authorization` header for subsequent requests.

### 🔐 Roles & Authorization

The system implements **role-based access control (RBAC)** to secure and differentiate access to various endpoints. There are two primary user roles:

| Role     | Description                         | Access Scope                                        |
|----------|-------------------------------------|-----------------------------------------------------|
| `Admin`  | System administrator                | Full access to all resources and management actions |
| `Waiter` | Serves customers and handles orders | Limited access Waiter endpoints only                |

- 🔒 All API endpoints are protected with `[Authorize(Roles = "RoleName")]` attributes, except the `Login` endpoint which is public.
- Role information is embedded in JWT tokens upon successful login.

#### Role Initialization

On application startup, the system checks the database for:
- Missing roles (`Admin`, `Waiter`)
- Absence of an Admin user

If needed, it **automatically creates** the required roles and a **default Admin user**.

> This makes it easy to deploy the app without requiring manual database seeding.

---

## 🧩 Database & Migrations

- Uses **Entity Framework Core Code First**
- Connection strings can be set via:
  - **User Secrets** (for local development)  
  - **Environment Variables** (for Azure or other deployment environments)
- Azure live demo uses **Azure SQL Database**

### Migrations
To apply or create migrations locally:

```bash
# Create migration
dotnet ef migrations add Init --project POS_Server_DAL

# Apply migration
dotnet ef database update --project POS_Server_DAL
```

---

## ⚙️ Prerequisites

Before running this project locally, ensure you have the following installed:

- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)  
  (Required to build and run the application)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  
  (Or any compatible local/remote database)
- (Optional) [Postman](https://www.postman.com/)  
  (For testing API endpoints)
- (Optional) [IIS](https://learn.microsoft.com/en-us/iis/install/installing-iis-7/) or Docker  
  (For local server hosting simulation)

---

## 🚀 Getting Started Locally

### 1. Clone the repository

```bash
git clone https://github.com/Ammar-000/PointOfSale.git
cd PointOfSale
```

### 2. Set up the listening prot (optional):

   - Use Secret Manager or set `POSSettings__Port` as an environment variable, or you can leave it as default 8080.

### 3. Set up the Jwt Key for authentication & authorization:

   - Use Secret Manager or set `POSSettings__JwtSettings__Key` as an environment variable.

### 4. Set up the database connection string:

   - Use Secret Manager or set `POSSettings__ConnectionStrings__DefaultConnection` as an environment variable.

### 5. Run database migrations:

```bash
dotnet ef database update --project POS_Server_DAL
```

### 6. Launch the API:

   - Set `POS_Server_PL` as the startup project.
   - Run the API and test it locally via Swagger or Postman.

---

## 🧪 API Testing (Postman)

You can test all endpoints using the included **Postman collection**.

- Import the collection from `/Postman/PointOfSale.postman_collection.json`
- Or access it directly from the [Postman Workspace](https://www.postman.com/ammar-0/public-workspace-1/collection/y8uul8m/pointofsale)

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## 💬 Contributions & Feedback

If you have suggestions, issues, or improvements, feel free to [open an issue](https://github.com/Ammar-000/PointOfSale/issues) or contribute via pull request.

---

## 🧠 Author

Developed by Ammar as a learning project to simulate real-world backend architecture and deployment workflow.
Thanks for checking it out!
