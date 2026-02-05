# Pattern Blindness Backend - Copilot Instructions

## Project Overview

Pattern Blindness is an interview preparation platform that trains deliberate problem-solving. The backend is built with ASP.NET Core using Clean Architecture principles.

### MVP Features

1. **Cold Start Commitment** - 90-second forced thinking before coding
2. **Wrong-But-Reasonable Reveal** - Show common wrong approaches after solving
3. **Confidence vs Correctness Tracking** - Track pattern choices with confidence ratings

---

## Architecture Guidelines

### Clean Architecture Layers

```
PatternBlindness.Api           → Presentation (Minimal APIs, endpoints)
PatternBlindness.Application   → Use Cases (services, DTOs, interfaces)
PatternBlindness.Domain        → Core Business Logic (entities, value objects, enums)
PatternBlindness.Infrastructure → Data Access (EF Core, repositories)
```

### Dependency Rules

- Domain has NO dependencies
- Application depends only on Domain
- Infrastructure depends on Domain and Application
- Api depends on Application and Infrastructure

---

## C# Coding Standards

### Language Version

- Use C# 13 features (latest stable)
- Enable nullable reference types
- Use file-scoped namespaces

### Naming Conventions

- **PascalCase**: Classes, methods, public properties, constants
- **camelCase**: Private fields, local variables, parameters
- **\_camelCase**: Private fields with underscore prefix
- **I prefix**: Interfaces (e.g., `IUserRepository`)
- **Async suffix**: Async methods (e.g., `GetUserAsync`)

### Code Style

```csharp
// Preferred: File-scoped namespace
namespace PatternBlindness.Domain.Entities;

// Preferred: Primary constructors for simple classes
public class Pattern(Guid id, string name)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
}

// Preferred: Records for immutable DTOs
public record CreateAttemptRequest(Guid ProblemId, Guid ChosenPatternId, ConfidenceLevel Confidence);

// Preferred: Expression-bodied members when simple
public string FullName => $"{FirstName} {LastName}";

// Preferred: Pattern matching
if (user is null) return NotFound();
if (result is { IsSuccess: true, Value: var value }) return Ok(value);
```

### Nullable Reference Types

```csharp
// Always declare intent explicitly
public string Name { get; set; } = string.Empty;  // Non-null
public string? Description { get; set; }           // Nullable

// Use 'is null' pattern
if (entity is null) throw new ArgumentNullException(nameof(entity));
if (entity is not null) Process(entity);
```

---

## Domain-Driven Design Guidelines

### Entities

- Have identity (Guid Id)
- Encapsulate business rules
- Use private setters with public methods for state changes
- Raise domain events when significant state changes occur

```csharp
public class Attempt : Entity
{
    public Guid ProblemId { get; private set; }
    public ConfidenceLevel Confidence { get; private set; }
    public bool IsPatternCorrect { get; private set; }

    private Attempt() { } // EF Core

    public static Attempt Create(Guid problemId, Guid patternId, ConfidenceLevel confidence)
    {
        var attempt = new Attempt
        {
            Id = Guid.NewGuid(),
            ProblemId = problemId,
            Confidence = confidence,
            StartedAt = DateTime.UtcNow
        };
        attempt.AddDomainEvent(new AttemptStartedEvent(attempt.Id));
        return attempt;
    }

    public void Complete(bool isCorrect)
    {
        IsPatternCorrect = isCorrect;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new AttemptCompletedEvent(Id, isCorrect));
    }
}
```

### Value Objects

- Immutable (use records)
- No identity
- Compared by value

```csharp
public record ConfidenceScore(int Value)
{
    public int Value { get; } = Value is >= 0 and <= 100
        ? Value
        : throw new ArgumentOutOfRangeException(nameof(Value));
}
```

### Aggregates

- Attempt is an aggregate root (contains ColdStartSubmission)
- Problem is an aggregate root (contains WrongApproaches)
- Access child entities only through aggregate root

---

## ASP.NET Core Minimal APIs

### Endpoint Organization

```csharp
// Group endpoints by feature
public static class AttemptEndpoints
{
    public static void MapAttemptEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/attempts")
            .WithTags("Attempts")
            .RequireAuthorization();

        group.MapGet("/", GetAttempts)
            .WithName("GetAttempts")
            .WithOpenApi();

        group.MapPost("/", CreateAttempt)
            .WithName("CreateAttempt")
            .WithOpenApi();
    }
}
```

### Request/Response Patterns

```csharp
// Use TypedResults for strong typing
private static async Task<Results<Ok<AttemptResponse>, NotFound, BadRequest<ProblemDetails>>>
    GetAttempt(Guid id, IAttemptService service)
{
    var result = await service.GetByIdAsync(id);

    return result switch
    {
        { IsSuccess: true } => TypedResults.Ok(result.Value),
        { Error.Code: "NotFound" } => TypedResults.NotFound(),
        _ => TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message })
    };
}
```

### Validation

- Use FluentValidation for complex validation
- Return RFC 7807 Problem Details for errors

---

## Entity Framework Core

### DbContext Configuration

```csharp
public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<Pattern> Patterns => Set<Pattern>();
    public DbSet<Attempt> Attempts => Set<Attempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Entity Configurations

```csharp
public class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Confidence)
            .HasConversion<string>();

        builder.HasOne(a => a.Problem)
            .WithMany(p => p.Attempts)
            .HasForeignKey(a => a.ProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ColdStartSubmission)
            .WithOne(c => c.Attempt)
            .HasForeignKey<ColdStartSubmission>(c => c.AttemptId);
    }
}
```

### Query Best Practices

- Use `AsNoTracking()` for read-only queries
- Use projection (`Select`) to minimize data transfer
- Avoid N+1 queries with `Include()` or explicit loading
- Use `AsSplitQuery()` for multiple includes

---

## Testing Standards

### Test Naming Convention

```csharp
// Pattern: MethodName_Condition_ExpectedResult
[Fact]
public async Task CreateAttempt_WithValidData_ReturnsSuccessResult()
{
    // Arrange
    var request = new CreateAttemptRequest(problemId, patternId, ConfidenceLevel.Confident);

    // Act
    var result = await _service.CreateAsync(request);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
}

[Fact]
public async Task CompleteAttempt_WhenAlreadyCompleted_ReturnsFailure()
{
    // ...
}
```

### Test Organization

```
tests/
├── PatternBlindness.UnitTests/
│   ├── Domain/
│   │   └── AttemptTests.cs
│   └── Application/
│       └── AttemptServiceTests.cs
└── PatternBlindness.IntegrationTests/
    └── Endpoints/
        └── AttemptEndpointsTests.cs
```

---

## Async/Await Best Practices

```csharp
// Always use async/await for I/O operations
public async Task<Result<Attempt>> GetByIdAsync(Guid id, CancellationToken ct = default)
{
    var attempt = await _context.Attempts
        .Include(a => a.ColdStartSubmission)
        .FirstOrDefaultAsync(a => a.Id == id, ct);

    return attempt is null
        ? Result.Failure<Attempt>(AttemptErrors.NotFound)
        : Result.Success(attempt);
}

// Avoid async void except for event handlers
// Pass CancellationToken through the call chain
// Use ConfigureAwait(false) in library code
```

---

## Error Handling

### Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
```

### Domain Errors

```csharp
public static class AttemptErrors
{
    public static readonly Error NotFound = new("Attempt.NotFound", "Attempt not found");
    public static readonly Error AlreadyCompleted = new("Attempt.AlreadyCompleted", "Attempt is already completed");
    public static readonly Error ColdStartRequired = new("Attempt.ColdStartRequired", "Cold start submission is required");
}
```

---

## Security

### Authentication

- Use ASP.NET Core Identity with JWT Bearer tokens
- Store tokens securely (HttpOnly cookies or secure storage)
- Implement refresh token rotation

### Authorization

- Use policy-based authorization
- Validate user owns the resource before allowing access
- Never expose internal IDs in URLs without authorization checks

---

## Performance Considerations

### Caching

- Cache pattern taxonomy (rarely changes)
- Cache problem metadata (read-heavy)
- Use distributed cache (Redis) for multi-instance deployments

### Database

- Index frequently queried columns (UserId, ProblemId, CreatedAt)
- Use pagination for list endpoints
- Implement soft deletes for audit trail

---

## API Documentation

### OpenAPI/Swagger

- Add meaningful descriptions to endpoints
- Document all response types
- Include example requests/responses

```csharp
group.MapPost("/", CreateAttempt)
    .WithName("CreateAttempt")
    .WithDescription("Start a new problem attempt")
    .Produces<AttemptResponse>(StatusCodes.Status201Created)
    .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
    .WithOpenApi();
```

## Agent Instructions

- Never invent APIs, tools, files, or behaviors.
- If required context is missing, stop and request it or use an MCP.
- Use MCPs when external knowledge, state, or verification is needed.
- If an MCP is unavailable, search in the mcp registry and install it globally to use
- Clearly label assumptions, unverified code, and version-dependent logic.
- Prefer correctness over completeness.
