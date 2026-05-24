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
| AI Assistant       | Official Anthropic SDK + Microsoft.Extensions.AI |
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

### AI Assistant

| Method | Endpoint                          | Description                                   | Auth |
|--------|-----------------------------------|-----------------------------------------------|------|
| POST   | `/api/ai/tasks/draft`             | Draft a structured task from natural language | Yes  |
| POST   | `/api/ai/projects/{id}/summary`   | Stream a natural-language project summary     | Yes  |

The AI Assistant is powered by the official [Anthropic C# SDK](https://www.nuget.org/packages/Anthropic) integrated through [`Microsoft.Extensions.AI`](https://www.nuget.org/packages/Microsoft.Extensions.AI)'s `IChatClient` abstraction. The dependency on the SDK is confined entirely to the Infrastructure layer behind an `IAiAssistant` interface, so the rest of the application stays provider-agnostic.

**Configuration.** Set the model and token budget under the `"Ai"` section of `appsettings.json` (defaults: model `claude-haiku-4-5`, 1024 max output tokens). The API key is **never** stored in the repository -- it is read from the `ANTHROPIC_API_KEY` environment variable (or .NET User Secrets in development):

```bash
# Linux/macOS
export ANTHROPIC_API_KEY="your-key-here"

# Windows (PowerShell)
$env:ANTHROPIC_API_KEY = "your-key-here"

# Or, in development, via User Secrets
dotnet user-secrets set "ANTHROPIC_API_KEY" "your-key-here" --project src/TaskManager.API
```

**Graceful degradation.** If no API key is configured, the AI endpoints return `503 Service Unavailable` and the rest of the API continues to function normally. For the streaming summary endpoint this is enforced eagerly (the assistant's availability is checked before the response is produced), so the `503` is set before any response body is flushed. If the model is reachable but returns an unparseable draft, the draft endpoint returns `502 Bad Gateway` — distinguishing a faulty upstream payload from an unavailable service. The summary endpoint streams its chunks as an incremental JSON array of strings (`application/json`).

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

**128 tests** across all layers:

- **Domain** -- Entity invariants and business rules
- **Application** -- Command/query handlers, validators, mapping
- **Infrastructure** -- Service implementations (including the AI assistant, with a faked `IChatClient`)
- **API** -- Middleware, controller behavior, and `WebApplicationFactory` integration tests (including the no-API-key `503` path)

AI assistant tests use a fake `IChatClient` so they never call the real Anthropic API or consume tokens; CI requires no API key.

---

## Key Design Decisions

- **Clean Architecture** -- Strict dependency inversion keeps the domain free of infrastructure concerns. Changing the database or API framework requires zero changes to business logic.

- **CQRS with MediatR** -- Separating reads from writes simplifies each handler, makes the codebase easier to navigate, and enables independent scaling of read/write paths if needed.

- **FluentValidation pipeline** -- Validation runs as a MediatR behavior, ensuring every command is validated before reaching the handler. No validation logic leaks into controllers.

- **Global exception handling** -- A single middleware maps domain exceptions (NotFound, Validation, ForbiddenAccess, AiUnavailable) to appropriate HTTP status codes, keeping controllers thin.

- **Provider-agnostic AI integration** -- The AI assistant lives behind an `IAiAssistant` abstraction in the Application layer. The Anthropic SDK and `Microsoft.Extensions.AI`'s `IChatClient` are confined to a single Infrastructure service, so the AI provider can be swapped without touching business logic. When no API key is configured, a no-op implementation is registered instead, degrading the feature gracefully without affecting the rest of the API.

- **Structured logging with Serilog** -- JSON-formatted logs with correlation context, ready for production log aggregation.

- **Multi-stage Docker build** -- Optimized image with layer caching for dependencies. Runs as a non-root user.

- **TreatWarningsAsErrors** -- Enforced project-wide via `Directory.Build.props`. No warnings slip through.

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

Built by [Gustavo Ali](https://github.com/gustavoali)
