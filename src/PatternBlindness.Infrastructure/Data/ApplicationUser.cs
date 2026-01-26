using Microsoft.AspNetCore.Identity;

namespace PatternBlindness.Infrastructure.Data;

/// <summary>
/// Application user extending ASP.NET Core Identity.
/// </summary>
public class ApplicationUser : IdentityUser
{
  /// <summary>
  /// User's display name.
  /// </summary>
  public string DisplayName { get; set; } = string.Empty;

  /// <summary>
  /// When the user account was created.
  /// </summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// When the user account was last updated.
  /// </summary>
  public DateTime? UpdatedAt { get; set; }
}
