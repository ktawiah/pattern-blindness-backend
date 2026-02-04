using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNet.Security.OAuth.GitHub;
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
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
        policy.WithOrigins(
            builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline

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
app.MapPatternEndpoints();
app.MapPatternTrackingEndpoints();
app.MapDataStructureEndpoints();
app.MapProblemEndpoints();
app.MapAttemptEndpoints();
app.MapLeetCodeEndpoints();
app.MapUserProfileEndpoints();

// Root redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Apply migrations on startup (dev only - use proper migrations in production)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

app.Run();

// Make Program class partial for integration tests
public partial class Program { }
