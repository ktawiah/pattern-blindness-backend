using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
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
    var authGroup = app.MapGroup("/api/auth")
        .WithTags("Authentication");

    authGroup.MapIdentityApi<ApplicationUser>();

    // OAuth endpoints
    authGroup.MapGet("/external-providers", GetExternalProviders)
        .WithName("GetExternalProviders")
        .WithDescription("Get list of available external authentication providers");

    authGroup.MapGet("/external-login", ExternalLogin)
        .WithName("ExternalLogin")
        .WithDescription("Initiate external OAuth login");

    authGroup.MapGet("/external-callback", ExternalCallback)
        .WithName("ExternalCallback")
        .WithDescription("Handle OAuth callback from external provider");
  }

  private static async Task<IResult> GetExternalProviders(
      SignInManager<ApplicationUser> signInManager)
  {
    var providers = (await signInManager.GetExternalAuthenticationSchemesAsync())
        .Select(s => new { s.Name, s.DisplayName })
        .ToList();
    return Results.Ok(providers);
  }

  private static IResult ExternalLogin(
      string provider,
      string? returnUrl,
      SignInManager<ApplicationUser> signInManager,
      IConfiguration configuration)
  {
    // Build callback URL
    var callbackUrl = $"/api/auth/external-callback?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";

    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
    return Results.Challenge(properties, [provider]);
  }

  private static async Task<IResult> ExternalCallback(
      string? returnUrl,
      string? remoteError,
      SignInManager<ApplicationUser> signInManager,
      UserManager<ApplicationUser> userManager,
      IConfiguration configuration)
  {
    var frontendUrl = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?.FirstOrDefault()
        ?? "http://localhost:3000";

    if (remoteError != null)
    {
      return Results.Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(remoteError)}");
    }

    var info = await signInManager.GetExternalLoginInfoAsync();
    if (info == null)
    {
      return Results.Redirect($"{frontendUrl}/login?error=External+login+failed");
    }

    // Try to sign in with the external login
    var result = await signInManager.ExternalLoginSignInAsync(
        info.LoginProvider,
        info.ProviderKey,
        isPersistent: false,
        bypassTwoFactor: true);

    ApplicationUser? user = null;

    if (result.Succeeded)
    {
      user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
    }
    else
    {
      // Create new user from external login
      var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
      if (string.IsNullOrEmpty(email))
      {
        return Results.Redirect($"{frontendUrl}/login?error=Email+not+provided+by+provider");
      }

      user = await userManager.FindByEmailAsync(email);
      if (user == null)
      {
        user = new ApplicationUser
        {
          UserName = email,
          Email = email,
          EmailConfirmed = true // Trust OAuth provider's email verification
        };
        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
          var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
          return Results.Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(errors)}");
        }
      }

      // Link external login to user
      var addLoginResult = await userManager.AddLoginAsync(user, info);
      if (!addLoginResult.Succeeded)
      {
        var errors = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
        return Results.Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(errors)}");
      }

      // Sign in the user
      await signInManager.SignInAsync(user, isPersistent: false);
    }

    // Generate tokens for the user (using Identity's token generation)
    if (user != null)
    {
      // For SPA, we need to generate a bearer token
      // Identity API endpoints use BearerTokenHandler, so we redirect with a success indicator
      // The frontend should then call the login endpoint or we generate a token here
      return Results.Redirect($"{frontendUrl}/auth/callback?success=true&email={Uri.EscapeDataString(user.Email ?? "")}");
    }

    return Results.Redirect($"{frontendUrl}/login?error=Authentication+failed");
  }
}