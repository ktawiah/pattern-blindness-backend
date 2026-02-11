# Pattern Blindness Backend

[![Build Status](https://img.shields.io/github/actions/workflow/status/ktawiah/pattern-blindness-backend/.github%2Fworkflows%2Fci.yml?branch=main)](https://github.com/ktawiah/PatternBlindness/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/download)

A deliberate practice platform for interview preparation that trains engineers to approach problems systematically rather than panic.

## Table of Contents

- [Pattern Blindness Backend](#pattern-blindness-backend)
  - [Table of Contents](#table-of-contents)
  - [Features](#features)
  - [Tech Stack](#tech-stack)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Quick Start](#quick-start)
      - [Using Docker Compose (Recommended)](#using-docker-compose-recommended)
      - [Manual Setup](#manual-setup)
    - [Local Development](#local-development)
  - [Database](#database)
    - [Migrations](#migrations)
  - [Project Structure](#project-structure)
  - [API Documentation](#api-documentation)
    - [Authentication](#authentication)
    - [Patterns](#patterns)
    - [Problems](#problems)
    - [Attempts](#attempts)
  - [Configuration](#configuration)
    - [Environment Variables](#environment-variables)
  - [Development](#development)
    - [Running Tests](#running-tests)
    - [Code Style](#code-style)
    - [Building](#building)
  - [Deployment](#deployment)
    - [Docker](#docker)
    - [Render (Recommended)](#render-recommended)
  - [Troubleshooting](#troubleshooting)
    - [Database Connection Issues](#database-connection-issues)
    - [Migration Errors](#migration-errors)
    - [API Not Starting](#api-not-starting)
  - [Contributing](#contributing)
    - [Pull Request Process](#pull-request-process)
  - [License](#license)

## Features

- **Cold Start Commitment** - 90-second forced thinking period before coding
- **Wrong-But-Reasonable Reveal** - Shows common wrong approaches after solving
- **Confidence vs Correctness Tracking** - Analytics on overconfidence and underconfidence
- **Clean Architecture** - Maintainable and testable codebase

## Tech Stack

| Component     | Technology            | Version              |
| ------------- | --------------------- | -------------------- |
| Runtime       | .NET                  | 9.0                  |
| Web Framework | ASP.NET Core          | Minimal APIs         |
| Database      | PostgreSQL            | 16+                  |
| ORM           | Entity Framework Core | Latest               |
| Auth          | ASP.NET Core Identity | JWT Tokens           |
| Architecture  | Clean Architecture    | Domain-Driven Design |

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet) or higher
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) (optional, for PostgreSQL)
- [PostgreSQL 16+](https://www.postgresql.org/download/) (if not using Docker)

### Quick Start

#### Using Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/ktawiah/PatternBlindness.git
cd PatternBlindness/pattern-blindness-backend

# Start the entire stack (API + Database)
docker-compose up -d

# API will be available at http://localhost:8080
# Swagger UI at http://localhost:8080/swagger
```

#### Manual Setup

```bash
# Start PostgreSQL
docker run -d \
  --name pattern-blindness-db \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=patternblindness \
  -p 5432:5432 \
  postgres:16-alpine

# Run migrations
dotnet ef database update --startup-project src/PatternBlindness.Api

# Start the API
cd src/PatternBlindness.Api
dotnet run

# Access the API
# Swagger UI: http://localhost:5000/swagger
# Health check: http://localhost:5000/health
```

### Local Development

1. **Install dependencies**
   ```bash
   dotnet restore
   ```

2. **Configure environment**
   ```bash
   cp .env.example .env
   # Edit .env with your local settings
   ```

3. **Start the database**
   ```bash
   docker-compose up -d postgres
   ```

4. **Run migrations**
   ```bash
   dotnet ef database update --startup-project src/PatternBlindness.Api
   ```

5. **Start the API**
   ```bash
   dotnet run --project src/PatternBlindness.Api
   ```

## Database

### Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName \
  --project src/PatternBlindness.Infrastructure \
  --startup-project src/PatternBlindness.Api \
  --output-dir Migrations

# Apply pending migrations
dotnet ef database update \
  --project src/PatternBlindness.Infrastructure \
  --startup-project src/PatternBlindness.Api

# Rollback last migration
dotnet ef database update LastSuccessfulMigration \
  --project src/PatternBlindness.Infrastructure \
  --startup-project src/PatternBlindness.Api
```

## Project Structure

```
src/
├── PatternBlindness.Api              # Presentation layer (endpoints, middleware)
├── PatternBlindness.Application      # Business logic (services, DTOs, validators)
├── PatternBlindness.Domain           # Core entities, enums, value objects
└── PatternBlindness.Infrastructure   # Data access, repositories, EF Core config

tests/
├── PatternBlindness.UnitTests        # Unit tests for services & utilities
└── PatternBlindness.IntegrationTests # Integration tests for endpoints
```

## API Documentation

### Authentication

| Method | Endpoint             | Description                 |
| ------ | -------------------- | --------------------------- |
| POST   | `/api/auth/register` | Register a new user         |
| POST   | `/api/auth/login`    | Login and receive JWT token |
| GET    | `/api/auth/profile`  | Get current user profile    |

### Patterns

| Method | Endpoint                      | Description                                  |
| ------ | ----------------------------- | -------------------------------------------- |
| GET    | `/api/patterns`               | List all coding patterns                     |
| GET    | `/api/patterns/{id}`          | Get pattern details with associated problems |
| GET    | `/api/patterns/{id}/weakness` | Get user's weakness analysis for pattern     |

### Problems

| Method | Endpoint                              | Description                         |
| ------ | ------------------------------------- | ----------------------------------- |
| GET    | `/api/problems/{id}`                  | Get problem details                 |
| GET    | `/api/problems/{id}/wrong-approaches` | Reveal common mistakes (post-solve) |
| GET    | `/api/problems/random`                | Get random problem for practice     |

### Attempts

| Method | Endpoint                        | Description                                |
| ------ | ------------------------------- | ------------------------------------------ |
| POST   | `/api/attempts/start`           | Start an attempt (begins cold start timer) |
| POST   | `/api/attempts/{id}/cold-start` | Submit cold start thinking                 |
| POST   | `/api/attempts/{id}/complete`   | Complete and score the attempt             |
| GET    | `/api/attempts/dashboard`       | Get confidence/correctness analytics       |

**Full OpenAPI specification available at `/swagger` after starting the server.**

## Configuration

### Environment Variables

Create a `.env` file in the backend directory:

```env
# Database
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=patternblindness;Username=postgres;Password=postgres

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000

# CORS
Cors__AllowedOrigins__0=http://localhost:3000
Cors__AllowedOrigins__1=https://yourfrontend.com

# External Services
OPENAI_API_KEY=your_key_here
OAUTH__GITHUB__CLIENTID=your_id_here
OAUTH__GITHUB__CLIENTSECRET=your_secret_here
```

See `.env.example` for all available options.

## Development

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/PatternBlindness.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Code Style

This project follows Microsoft's [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

```bash
# Format code
dotnet format

# Analyze code style
dotnet format --verify-no-changes
```

### Building

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release
```

## Deployment

### Docker

```bash
# Build Docker image
docker build -t pattern-blindness:latest .

# Run container
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="your-connection-string" \
  pattern-blindness:latest
```

### Render (Recommended)

1. Connect your GitHub repository to Render
2. Set environment variables in Render dashboard
3. Set Root Directory to `pattern-blindness-backend`
4. Set Docker Build Context to `pattern-blindness-backend`
5. Configure database connection with Render PostgreSQL

See [Render Documentation](https://render.com/docs) for detailed deployment steps.

## Troubleshooting

### Database Connection Issues

**Error: "Connection timeout"**
- Ensure PostgreSQL is running: `docker ps | grep postgres`
- Check connection string in `.env`
- Verify database exists: `createdb patternblindness`

### Migration Errors

**Error: "The command could not be loaded"**
```bash
# Install Entity Framework Tools
dotnet tool install --global dotnet-ef

# Or update if already installed
dotnet tool update --global dotnet-ef
```

### API Not Starting

**Error: "Port already in use"**
```bash
# Change ASPNETCORE_URLS in .env or
dotnet run --project src/PatternBlindness.Api -- --urls "http://+:5001"
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -am 'Add feature'`
4. Push to branch: `git push origin feature/your-feature`
5. Submit a Pull Request

### Pull Request Process

- Ensure all tests pass: `dotnet test`
- Format code: `dotnet format`
- Update documentation as needed
- Link related issues with `Fixes #123`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Questions?** Open an issue on [GitHub](https://github.com/ktawiah/PatternBlindness/issues) or contact the maintainers.

