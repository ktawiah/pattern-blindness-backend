using System.Text.Json.Serialization;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PatternBlindness.Api.Endpoints;
using PatternBlindness.Infrastructure;
using PatternBlindness.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// Configure JSON serialization to use string enums
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Database & Infrastructure
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Try to load from environment variable directly (Render uses this)
    var envConnString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    if (!string.IsNullOrEmpty(envConnString))
    {
        connectionString = envConnString;
    }
    else
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found. " +
            "Please set ConnectionStrings__DefaultConnection environment variable on Render.");
    }
}

builder.Services.AddInfrastructure(connectionString);

// Identity & Authentication
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure OAuth providers (optional - only if credentials are provided)
var authBuilder = builder.Services.AddAuthentication();

var githubClientId = builder.Configuration["OAuth:GitHub:ClientId"];
var githubClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"];
if (!string.IsNullOrEmpty(githubClientId) && !string.IsNullOrEmpty(githubClientSecret))
{
    authBuilder.AddGitHub(options =>
    {
        options.ClientId = githubClientId;
        options.ClientSecret = githubClientSecret;
        options.Scope.Add("user:email");
    });
}

builder.Services.AddAuthorization(options =>
{
    // For now, AdminOnly policy just requires authentication
    // In production, you'd check for admin role/claim
    options.AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser());
});

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Pattern Blindness API",
        Version = "v1",
        Description = "Interview preparation platform that trains deliberate problem-solving"
    });
});

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks();

// Add exception handling middleware
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";

        var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

        if (ctx.Exception is not null)
        {
            logger.LogError(ctx.Exception, "Unhandled exception occurred");
            ctx.ProblemDetails.Detail = ctx.Exception.Message;

            // Include inner exception details for database errors
            var innerEx = ctx.Exception.InnerException;
            if (innerEx is not null)
            {
                logger.LogError(innerEx, "Inner exception");
                ctx.ProblemDetails.Extensions["innerException"] = innerEx.Message;
                if (innerEx.InnerException is not null)
                {
                    ctx.ProblemDetails.Extensions["rootCause"] = innerEx.InnerException.Message;
                }
            }
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline

// Exception handling - MUST be first in pipeline
app.UseExceptionHandler();

// CORS
app.UseCors("AllowFrontend");

// Swagger (dev only in production, but always for MVP)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pattern Blindness API v1");
    options.RoutePrefix = "swagger";
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health check
app.MapHealthChecks("/health");

// Map API endpoints
app.MapAuthEndpoints();
// Note: Pattern and DataStructure endpoints removed - data now in frontend JSON
app.MapPatternTrackingEndpoints();
app.MapProblemEndpoints();
app.MapAttemptEndpoints();
app.MapLeetCodeEndpoints();
app.MapUserProfileEndpoints();

// Root redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Diagnostic endpoint - shows environment info (temporary, for debugging)
app.MapGet("/api/diagnostic", (IServiceProvider services) =>
{
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var connString = dbContext.Database.GetConnectionString() ?? "NO CONNECTION STRING";

    // Sanitize for display
    var sanitized = System.Text.RegularExpressions.Regex.Replace(
        connString,
        @"Password=([^;]*)",
        "Password=***REDACTED***",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return Results.Ok(new
    {
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
        connectionStringLoaded = !string.IsNullOrEmpty(connString),
        connectionString = sanitized,
        timestamp = DateTime.UtcNow
    });
});

app.Run();

// Make Program class partial for integration tests
public partial class Program { }
// Deployed: Thu Feb  5 18:12:16 CST 2026
