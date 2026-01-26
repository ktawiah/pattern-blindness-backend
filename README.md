# Pattern Blindness Backend

A deliberate practice platform for interview preparation that trains engineers to approach problems systematically rather than panic.

## MVP Features

1. **Cold Start Commitment** - 90-second forced thinking period before coding
2. **Wrong-But-Reasonable Reveal** - Shows common wrong approaches after solving
3. **Confidence vs Correctness Tracking** - Analytics on overconfidence and underconfidence

## Tech Stack

- .NET 9 / ASP.NET Core with Minimal APIs
- PostgreSQL with Entity Framework Core
- Clean Architecture (Domain, Application, Infrastructure, Api)
- ASP.NET Core Identity for authentication

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) (for PostgreSQL)

### Quick Start with Docker

```bash
# Start the entire stack
docker-compose up -d

# API will be available at http://localhost:8080
# Swagger UI at http://localhost:8080/swagger
```

### Local Development

1. **Start PostgreSQL**

```bash
# Using Docker
docker run -d \
  --name pattern-blindness-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=pattern_blindness \
  -p 5432:5432 \
  postgres:16-alpine
```

2. **Run the API**

```bash
cd src/PatternBlindness.Api
dotnet run
```

3. **Access the API**
- Swagger UI: http://localhost:5000/swagger
- Health check: http://localhost:5000/health

### Database Migrations

```bash
# Create a new migration
cd src/PatternBlindness.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../PatternBlindness.Api

# Apply migrations (happens automatically in development)
dotnet ef database update --startup-project ../PatternBlindness.Api
```

## Project Structure

- PatternBlindness.Api           → Presentation (APIs, endpoints)
- PatternBlindness.Application   → Use Cases (services, DTOs, interfaces)
- PatternBlindness.Domain        → Core Business Logic (entities, value objects, enums)
- PatternBlindness.Infrastructure → Data Access (EF Core, repositories)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get JWT

### Patterns
- `GET /api/patterns` - List all patterns
- `GET /api/patterns/{id}` - Get pattern details with problems
- `GET /api/patterns/{id}/weakness` - Get user's weakness analysis

### Problems
- `GET /api/problems/{id}` - Get problem details
- `GET /api/problems/{id}/wrong-approaches` - Reveal common mistakes (post-solve)
- `GET /api/problems/random` - Get random problem for practice

### Attempts
- `POST /api/attempts/start` - Start an attempt (begins cold start timer)
- `POST /api/attempts/{id}/cold-start` - Submit cold start thinking
- `POST /api/attempts/{id}/complete` - Complete the attempt
- `GET /api/attempts/dashboard` - Get confidence/correctness analytics

## Environment Variables

| Variable                               | Description                  | Default               |
| -------------------------------------- | ---------------------------- | --------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | See appsettings.json  |
| `Cors__AllowedOrigins__0`              | First allowed CORS origin    | http://localhost:3000 |
| `ASPNETCORE_ENVIRONMENT`               | Runtime environment          | Development           |

