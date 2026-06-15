# 📝 TodoApi - .NET 10 REST API

[![.NET 10 & Docker CI/CD Pipeline](https://github.com/Nikhil767/todoapi-dotnet/actions/workflows/ci-cd.yaml/badge.svg)](https://github.com/Nikhil767/todoapi-dotnet/actions)
[![Docker Hub Image](https://img.shields.io/docker/v/learningm0nster/todoapi-dotnet?label=Docker%20Hub)](https://hub.docker.com/r/learningm0nster/todoapi-dotnet)

A simple and extensible **Todo API** built with **ASP.NET Core** and **.NET 10**.  
This project provides a RESTful backend for managing todo items, supporting common CRUD operations and serving as a clean foundation for future enhancements.

## ✨ Overview

This repository demonstrates how to build a web API for todo management using modern .NET practices.  
It is designed to be easy to understand, easy to run locally, and easy to extend with features such as authentication, database persistence, validation, and Swagger documentation [web:2][web:4][web:16].

## 🚀 Features

- Create todo items.
- Read all todo items.
- Read a todo item by ID.
- Update todo items.
- Delete todo items.
- RESTful API structure.
- Swagger / OpenAPI support.
- Easy to extend with database integration and authentication.
- Suitable for learning, prototyping, or as a starter backend for a frontend app [web:2][web:4][web:16].

## 🛠 Tech Stack

- ASP.NET Core Web API.
- C#.
- .NET 10.
- Swagger / OpenAPI.
- Entity Framework Core, if used in the project.
- SQL Server / SQLite / In-Memory DB, depending on your configuration [web:2][web:4][web:16].

## 📁 Project Structure

```bash
todoapi-dotnet/
├── .github/workflows/
│   └── ci-cd.yaml
├── TodoApi/
│   ├── Controllers/
│   ├── Models/
│   ├── Program.cs
│   └── appsettings.json
├── TodoApi.Tests/
├── .dockerignore
├── .gitignore
├── Dockerfile
└── TodoApi.slnx
```

### 📌 Folder Purpose

- `Controllers/` contains API endpoints.
- `Models/` stores application entities.
- `Data/` contains database context or seed logic if present.
- `Services/` holds business logic if the project uses a service layer.
- `DTOs/` stores request and response models.
- `Program.cs` configures the app, middleware, and dependency injection.

## 🔗 API Endpoints

Below is a common endpoint set for a Todo API:

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/todos` | Get all todo items. |
| GET | `/api/todos/{id}` | Get a single todo item by ID. |
| POST | `/api/todos` | Create a new todo item. |
| PUT | `/api/todos/{id}` | Update an existing todo item. |
| DELETE | `/api/todos/{id}` | Delete a todo item. |

If your implementation uses a different route name, such as `/todo`, `/tasks`, or `/api/todoitems`, update the table to match your actual controller routes.

## 🧪 Example Request

### Create a Todo

```http
POST /api/todos
Content-Type: application/json
```

```json
{
  "title": "Finish README",
  "description": "Write a complete project README",
  "isCompleted": false
}
```

### Example Response

```json
{
  "id": 1,
  "title": "Finish README",
  "description": "Write a complete project README",
  "isCompleted": false
}
```

## ⚙️ Getting Started

### Prerequisites

Make sure you have the following installed:

- .NET SDK
- Git
- A code editor such as Visual Studio or VS Code

### Clone the Repository

```bash
git clone https://github.com/Nikhil767/todoapi-dotnet.git
cd todoapi-dotnet
```

### Restore Dependencies

```bash
dotnet restore
```

### Run the Application

```bash
dotnet run
```

If Swagger is enabled, open the displayed local URL in your browser and test the endpoints from the Swagger UI.

## 🔧 Configuration

If your project uses `appsettings.json`, you may need to update settings such as:

- Database connection string.
- Logging levels.
- Environment-specific values.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TodoApiDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

## 🗄 Database Setup

If the project uses Entity Framework Core, you may need to run migrations:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If you are using an in-memory database or no database at all, this section can be adjusted accordingly.

## 🧪 Testing

You can test the API using:

- Swagger UI.
- Postman.
- curl.
- A frontend application.

Example using `curl`:

```bash
curl -X GET https://localhost:5001/api/todos
```

## 🌱 Future Improvements

Possible enhancements for this project:

- Add authentication and authorization.
- Add user-specific todo lists.
- Integrate PostgreSQL, SQL Server, or MySQL.
- Add validation and better error handling.
- Add pagination and filtering.
- Add unit and integration tests.
- Add Docker support.
- Add CI/CD pipeline.

## 🤝 Contributing

Contributions are welcome.  
If you want to improve the project, feel free to fork the repository, create a branch, and submit a pull request.

## 📄 License

Add your preferred license here, such as MIT, Apache 2.0, or proprietary.

## 👨‍💻 Author

Developed by **Nikhil Deshmukh**.