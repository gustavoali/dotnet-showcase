# SEED: .NET Showcase Portfolio Project

## Vision
Proyecto .NET profesional para portfolio publico en GitHub (gustavoali).
Debe demostrar dominio de .NET moderno, Clean Architecture, buenas practicas
y production-readiness. Pensado para que reclutadores y tech leads lo revisen.

Motivacion: Lateral Group pide "What would you put your name on?"

## Dominio Sugerido
**Task Management API** - Un sistema de gestion de tareas/proyectos estilo mini-Trello.
Dominio simple pero que permite demostrar:
- CRUD completo con relaciones
- Autenticacion/autorizacion (JWT)
- Paginacion, filtros, ordenamiento
- Validaciones de negocio
- Event-driven patterns
- Background jobs

## Stack Tecnico
| Componente | Tecnologia |
|------------|------------|
| Framework | .NET 8 (LTS) |
| Arquitectura | Clean Architecture (4 capas) |
| ORM | Entity Framework Core 8 |
| Base de datos | PostgreSQL (Docker via WSL) |
| Auth | JWT Bearer tokens |
| Validation | FluentValidation |
| Mapping | Mapster o AutoMapper |
| Testing | xUnit + FluentAssertions + Moq + Testcontainers |
| API Docs | Swagger/OpenAPI (Swashbuckle) |
| Logging | Serilog (structured logging) |
| CI/CD | GitHub Actions |
| Container | Docker + docker-compose |

## Estructura Clean Architecture
```
src/
  TaskManager.Domain/           # Entities, Value Objects, Domain Events, Interfaces
  TaskManager.Application/      # Use Cases, DTOs, Validators, Interfaces
  TaskManager.Infrastructure/   # EF Core, Repositories, External Services
  TaskManager.API/              # Controllers, Middleware, DI Configuration

tests/
  TaskManager.Domain.Tests/
  TaskManager.Application.Tests/
  TaskManager.Infrastructure.Tests/
  TaskManager.API.Tests/         # Integration tests
```

## Entidades Core
- **User** (Id, Email, PasswordHash, Name, CreatedAt)
- **Project** (Id, Name, Description, OwnerId, CreatedAt, Status)
- **TaskItem** (Id, Title, Description, ProjectId, AssigneeId, Status, Priority, DueDate, CreatedAt, UpdatedAt)
- **Comment** (Id, TaskItemId, AuthorId, Content, CreatedAt)
- **Tag** (Id, Name) + TaskTag (many-to-many)

## Endpoints Principales
```
POST   /api/auth/register
POST   /api/auth/login
GET    /api/projects
POST   /api/projects
GET    /api/projects/{id}
PUT    /api/projects/{id}
DELETE /api/projects/{id}
GET    /api/projects/{id}/tasks
POST   /api/projects/{id}/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
PATCH  /api/tasks/{id}/status
DELETE /api/tasks/{id}
POST   /api/tasks/{id}/comments
GET    /api/tasks/{id}/comments
```

## Criterios de Calidad
- [ ] Build 0 errors, 0 warnings
- [ ] Test coverage >70%
- [ ] Swagger UI funcional
- [ ] Docker-compose up levanta todo (API + DB)
- [ ] README profesional con: badges, arquitectura, setup, API examples
- [ ] GitHub Actions: build + test en cada push
- [ ] Structured logging con correlation IDs
- [ ] Global error handling middleware
- [ ] Paginacion en todos los list endpoints
- [ ] Seed data para demo

## Fases de Implementacion
1. **Scaffolding** - Crear solucion, proyectos, docker-compose, GitHub repo
2. **Domain + Application** - Entidades, interfaces, use cases, validaciones
3. **Infrastructure** - EF Core, migrations, repositories
4. **API** - Controllers, middleware, auth, Swagger
5. **Testing** - Unit + integration tests
6. **CI/CD** - GitHub Actions pipeline
7. **Polish** - README, seed data, Docker optimizations
