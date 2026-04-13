# TaskManager API

[![CI](https://github.com/gustavoali/dotnet-showcase/actions/workflows/ci.yml/badge.svg)](https://github.com/gustavoali/dotnet-showcase/actions/workflows/ci.yml)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A production-grade Task Management REST API built with .NET 8 and Clean Architecture. Demonstrates CQRS, JWT authentication, domain-driven design, and comprehensive testing -- the kind of backend system I build professionally.

---

## Architecture

```
+-------------------------------------------------+
|                  API Layer                       |
|  Controllers, Middleware, DI Configuration       |
+-------------------------------------------------+
                       |
+-------------------------------------------------+
|              Application Layer                   |
|  CQRS Commands/Queries, Validators, DTOs,       |
|  MediatR Handlers, Mapping (Mapster)             |
+-------------------------------------------------+
                       |
+-------------------------------------------------+
|              Domain Layer                        |
|  Entities, Value Objects, Enums,                 |
|  Domain Logic (zero dependencies)                |
+-------------------------------------------------+
                       |
+-------------------------------------------------+
|            Infrastructure Layer                  |
|  EF Core, PostgreSQL, JWT Auth,                  |
|  Repository Implementations                      |
+-------------------------------------------------+
```

Each layer depends only on the layers below it. The Domain layer has no external dependencies.

---

## Tech Stack

| Category           | Technology                              |
|--------------------|-----------------------------------------|
| Runtime            | .NET 8 (LTS)                            |
| Language           | C# 12                                   |
| Architecture       | Clean Architecture, CQRS                |
| Mediator           | MediatR                                 |
| Validation         | FluentValidation                        |
| Mapping            | Mapster                                 |
| ORM                | Entity Framework Core 8                 |
| Database           | PostgreSQL 16                           |
| Authentication     | JWT Bearer Tokens                       |
| Logging            | Serilog (structured)                    |
| Containerization   | Docker, Docker Compose                  |
| Testing            | xUnit, FluentAssertions, Moq            |
| API Documentation  | Swagger / OpenAPI                       |
| CI/CD              | GitHub Actions                          |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://docs.docker.com/get-docker/) (for PostgreSQL)

### Option 1: Docker Compose (recommended)

```bash
git clone https://github.com/gustavoali/dotnet-showcase.git
cd dotnet-showcase

docker-compose up -d
```

The API will be available at **http://localhost:5000** and Swagger UI at **http://localhost:5000/swagger**.

### Option 2: Local development

```bash
# Start PostgreSQL
docker-compose up -d db

# Run the API
dotnet run --project src/TaskManager.API

# Access Swagger at https://localhost:5001/swagger
```

### Quick test

```bash
# Register a user
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "demo@test.com", "password": "Demo123!", "displayName": "Demo User"}'

# Login to get a JWT token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "demo@test.com", "password": "Demo123!"}'
```

---

## API Endpoints

### Authentication

| Method | Endpoint               | Description          | Auth |
|--------|------------------------|----------------------|------|
| POST   | `/api/auth/register`   | Register new user    | No   |
| POST   | `/api/auth/login`      | Login, get JWT token | No   |

### Projects

| Method | Endpoint                      | Description              | Auth |
|--------|-------------------------------|--------------------------|------|
| GET    | `/api/projects`               | List projects (paginated)| No   |
| POST   | `/api/projects`               | Create project           | Yes  |
| GET    | `/api/projects/{id}`          | Get project details      | Yes  |
| PUT    | `/api/projects/{id}`          | Update project           | Yes  |
| DELETE | `/api/projects/{id}`          | Delete project           | Yes  |

### Tasks

| Method | Endpoint                           | Description            | Auth |
|--------|------------------------------------|------------------------|------|
| GET    | `/api/projects/{id}/tasks`         | List tasks in project  | Yes  |
| POST   | `/api/projects/{id}/tasks`         | Create task            | Yes  |
| GET    | `/api/tasks/{id}`                  | Get task details       | Yes  |
| PUT    | `/api/tasks/{id}`                  | Update task            | Yes  |
| PATCH  | `/api/tasks/{id}/status`           | Update task status     | Yes  |
| DELETE | `/api/tasks/{id}`                  | Delete task            | Yes  |

### Comments

| Method | Endpoint                      | Description       | Auth |
|--------|-------------------------------|-------------------|------|
| POST   | `/api/tasks/{id}/comments`    | Add comment       | Yes  |
| GET    | `/api/tasks/{id}/comments`    | List comments     | Yes  |

---

## Project Structure

```
dotnet-showcase/
|-- src/
|   |-- TaskManager.Domain/           # Entities, enums, domain logic
|   |-- TaskManager.Application/      # CQRS handlers, validators, DTOs
|   |-- TaskManager.Infrastructure/   # EF Core, PostgreSQL, JWT, repos
|   |-- TaskManager.API/              # Controllers, middleware, config
|-- tests/
|   |-- TaskManager.Domain.Tests/
|   |-- TaskManager.Application.Tests/
|   |-- TaskManager.Infrastructure.Tests/
|   |-- TaskManager.API.Tests/
|-- Dockerfile
|-- docker-compose.yml
|-- TaskManager.sln
```

---

## Testing

```bash
dotnet build --no-incremental
dotnet test --configuration Release
```

**91 tests** across all layers:

- **Domain** -- Entity invariants and business rules
- **Application** -- Command/query handlers, validators, mapping
- **Infrastructure** -- Service implementations
- **API** -- Middleware and controller behavior

---

## Key Design Decisions

- **Clean Architecture** -- Strict dependency inversion keeps the domain free of infrastructure concerns. Changing the database or API framework requires zero changes to business logic.

- **CQRS with MediatR** -- Separating reads from writes simplifies each handler, makes the codebase easier to navigate, and enables independent scaling of read/write paths if needed.

- **FluentValidation pipeline** -- Validation runs as a MediatR behavior, ensuring every command is validated before reaching the handler. No validation logic leaks into controllers.

- **Global exception handling** -- A single middleware maps domain exceptions (NotFound, Validation, ForbiddenAccess) to appropriate HTTP status codes, keeping controllers thin.

- **Structured logging with Serilog** -- JSON-formatted logs with correlation context, ready for production log aggregation.

- **Multi-stage Docker build** -- Optimized image with layer caching for dependencies. Runs as a non-root user.

- **TreatWarningsAsErrors** -- Enforced project-wide via `Directory.Build.props`. No warnings slip through.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

Built by [Gustavo Ali](https://github.com/gustavoali)
