using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents an algorithmic pattern (e.g., Two Pointers, Sliding Window, BFS).
/// </summary>
public class Pattern : Entity
{
  private Pattern() { }

  /// <summary>
  /// The name of the pattern (e.g., "Two Pointers", "Sliding Window").
  /// </summary>
  public string Name { get; private set; } = string.Empty;

  /// <summary>
  /// Detailed description of when and how to use this pattern.
  /// </summary>
  public string Description { get; private set; } = string.Empty;

  /// <summary>
  /// The category this pattern belongs to.
  /// </summary>
  public PatternCategory Category { get; private set; }

  /// <summary>
  /// Key signals that indicate this pattern might be applicable.
  /// Stored as JSON array.
  /// </summary>
  public string TriggerSignals { get; private set; } = "[]";

  /// <summary>
  /// Common mistakes or anti-patterns when using this approach.
  /// </summary>
  public string CommonMistakes { get; private set; } = "[]";

  /// <summary>
  /// Problems that use this pattern as the correct approach.
  /// </summary>
  public ICollection<Problem> Problems { get; private set; } = [];

  /// <summary>
  /// Creates a new pattern.
  /// </summary>
  public static Pattern Create(
      string name,
      string description,
      PatternCategory category,
      string? triggerSignals = null,
      string? commonMistakes = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Pattern name is required.", nameof(name));

    return new Pattern
    {
      Id = Guid.NewGuid(),
      Name = name.Trim(),
      Description = description?.Trim() ?? string.Empty,
      Category = category,
      TriggerSignals = triggerSignals ?? "[]",
      CommonMistakes = commonMistakes ?? "[]"
    };
  }

  /// <summary>
  /// Updates the pattern details.
  /// </summary>
  public void Update(string name, string description, string? triggerSignals = null, string? commonMistakes = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Pattern name is required.", nameof(name));

    Name = name.Trim();
    Description = description?.Trim() ?? string.Empty;

    if (triggerSignals is not null)
      TriggerSignals = triggerSignals;

    if (commonMistakes is not null)
      CommonMistakes = commonMistakes;

    UpdatedAt = DateTime.UtcNow;
  }
}
