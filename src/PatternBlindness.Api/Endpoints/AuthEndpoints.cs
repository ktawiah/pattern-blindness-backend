using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Authentication endpoints using ASP.NET Core Identity.
/// </summary>
public static class AuthEndpoints
{
  public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
  {
    // Use the built-in Identity API endpoints
    // This provides: /register, /login, /refresh, /confirmEmail, etc.
    app.MapGroup("/api/auth")
        .WithTags("Authentication")
        .MapIdentityApi<ApplicationUser>();
  }
}