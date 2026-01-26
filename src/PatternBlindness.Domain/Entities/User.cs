using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents the application user.
/// Extends ASP.NET Core Identity user.
/// </summary>
public class User
{
  /// <summary>
  /// Unique identifier (from ASP.NET Core Identity).
  /// </summary>
  public string Id { get; set; } = string.Empty;

  /// <summary>
  /// User's email address.
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// User's display name.
  /// </summary>
  public string DisplayName { get; set; } = string.Empty;

  /// <summary>
  /// When the user account was created.
  /// </summary>
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  /// User's attempts at problems.
  /// </summary>
  public ICollection<Attempt> Attempts { get; set; } = [];
}
