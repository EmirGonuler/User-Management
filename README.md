# UserManagement System

A full-stack user management application built with Clean Architecture principles.

## Tech Stack
- **Backend:** .NET 10, ASP.NET Core Web API
- **Frontend:** ASP.NET Core MVC
- **Database:** SQL Server (Code First via Entity Framework Core)
- **Testing:** xUnit, Moq, FluentAssertions

## Architecture
This solution follows Clean Architecture (N-Tier) with the following projects:

| Project | Responsibility |
|---|---|
| `UserManagement.Domain` | Entities, Interfaces, Enums |
| `UserManagement.Infrastructure` | EF Core DbContext, Repositories, Migrations |
| `UserManagement.API` | REST API Controllers, DTOs, Swagger |
| `UserManagement.Web` | MVC Frontend (Razor Views) |
| `UserManagement.Tests` | Unit & Integration Tests |

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/)

### Setup
1. Clone the repository
```bash
   git clone https://github.com/EmirGonuler/UserManagement.git
```
2. Update the connection string in `UserManagement.API/appsettings.json`
3. Apply migrations
```bash
   cd UserManagement.API
   dotnet ef database update
```
4. Run the API project
5. Run the Web project

## Features
- ✅ User Management (Add, Edit, Delete)
- ✅ Group Management with Permissions
- ✅ Many-to-Many User ↔ Group relationships
- ✅ REST API with Swagger documentation
- ✅ Unit & Integration Tests