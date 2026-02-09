# UserManagementAPI - README

A comprehensive **User Management API** built with ASP.NET Core for TechHive Solutions' HR and IT departments.

## Quick Start

### Prerequisites
- .NET 10.0 SDK or later
- Visual Studio Code or Visual Studio 2022+

### Run the API
```bash
cd UserManagementAPI
dotnet restore
dotnet run
```

### Access API
- **Swagger UI**: `https://localhost:7000/swagger`
- **Health Check**: `https://localhost:7000/health`

## API Endpoints

### Users Management
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/department/{department}` - Get users by department
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## Testing

Use the included `requests.http` file with VS Code REST Client or import into Postman.

### Sample Request
```http
GET http://localhost:5000/api/users
```

## Project Structure
- **Models/** - Data models (User)
- **Services/** - Business logic (UserService)
- **Controllers/** - API endpoints (UsersController)
- **requests.http** - HTTP test suite

## Features
✅ Complete CRUD operations  
✅ RESTful API design  
✅ Comprehensive error handling  
✅ Swagger/OpenAPI documentation  
✅ CORS support  
✅ Health check endpoint  
✅ Built-in logging  

## Documentation
See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for detailed documentation including:
- How Microsoft Copilot assisted in development
- API features and capabilities
- Testing procedures
- Future enhancement opportunities

## Architecture
- **Service Layer Pattern** for business logic separation
- **Dependency Injection** for loose coupling
- **In-Memory Storage** for lightweight operations
- **RESTful Design** for standard API conventions

---
**Built with:** ASP.NET Core 10.0 | C# 13
