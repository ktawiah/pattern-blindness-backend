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
  /// Brief description/summary of the pattern.
  /// </summary>
  public string Description { get; private set; } = string.Empty;

  /// <summary>
  /// The category this pattern belongs to.
  /// </summary>
  public PatternCategory Category { get; private set; }

  /// <summary>
  /// Detailed explanation of what this pattern is and how it works.
  /// </summary>
  public string WhatItIs { get; private set; } = string.Empty;

  /// <summary>
  /// When to use this pattern - problem characteristics and signals.
  /// </summary>
  public string WhenToUse { get; private set; } = string.Empty;

  /// <summary>
  /// Why this pattern works - the intuition and reasoning behind it.
  /// </summary>
  public string WhyItWorks { get; private set; } = string.Empty;

  /// <summary>
  /// Common real-world use cases and problem types. JSON array.
  /// </summary>
  public string CommonUseCases { get; private set; } = "[]";

  /// <summary>
  /// Typical time complexity (e.g., "O(n)", "O(n log n)").
  /// </summary>
  public string TimeComplexity { get; private set; } = string.Empty;

  /// <summary>
  /// Typical space complexity (e.g., "O(1)", "O(n)").
  /// </summary>
  public string SpaceComplexity { get; private set; } = string.Empty;

  /// <summary>
  /// Template/pseudocode showing the pattern structure.
  /// </summary>
  public string PseudoCode { get; private set; } = string.Empty;

  /// <summary>
  /// Key signals that indicate this pattern might be applicable.
  /// Stored as JSON array.
  /// </summary>
  public string TriggerSignals { get; private set; } = "[]";

  /// <summary>
  /// Common mistakes or anti-patterns when using this approach.
  /// Stored as JSON array.
  /// </summary>
  public string CommonMistakes { get; private set; } = "[]";

  /// <summary>
  /// External resources for learning more about this pattern.
  /// JSON array of {title, url, type} objects.
  /// </summary>
  public string Resources { get; private set; } = "[]";

  /// <summary>
  /// IDs of related or complementary patterns.
  /// JSON array of GUIDs.
  /// </summary>
  public string RelatedPatternIds { get; private set; } = "[]";

  /// <summary>
  /// Problems that use this pattern as the correct approach.
  /// </summary>
  public ICollection<Problem> Problems { get; private set; } = [];

  /// <summary>
  /// Creates a new pattern with all fields.
  /// </summary>
  public static Pattern Create(
      string name,
      string description,
      PatternCategory category,
      string? whatItIs = null,
      string? whenToUse = null,
      string? whyItWorks = null,
      string? commonUseCases = null,
      string? timeComplexity = null,
      string? spaceComplexity = null,
      string? pseudoCode = null,
      string? triggerSignals = null,
      string? commonMistakes = null,
      string? resources = null,
      string? relatedPatternIds = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Pattern name is required.", nameof(name));

    return new Pattern
    {
      Id = Guid.NewGuid(),
      Name = name.Trim(),
      Description = description?.Trim() ?? string.Empty,
      Category = category,
      WhatItIs = whatItIs?.Trim() ?? string.Empty,
      WhenToUse = whenToUse?.Trim() ?? string.Empty,
      WhyItWorks = whyItWorks?.Trim() ?? string.Empty,
      CommonUseCases = commonUseCases ?? "[]",
      TimeComplexity = timeComplexity?.Trim() ?? string.Empty,
      SpaceComplexity = spaceComplexity?.Trim() ?? string.Empty,
      PseudoCode = pseudoCode ?? string.Empty,
      TriggerSignals = triggerSignals ?? "[]",
      CommonMistakes = commonMistakes ?? "[]",
      Resources = resources ?? "[]",
      RelatedPatternIds = relatedPatternIds ?? "[]"
    };
  }

  /// <summary>
  /// Updates the pattern details.
  /// </summary>
  public void Update(
      string name,
      string description,
      string? whatItIs = null,
      string? whenToUse = null,
      string? whyItWorks = null,
      string? commonUseCases = null,
      string? timeComplexity = null,
      string? spaceComplexity = null,
      string? pseudoCode = null,
      string? triggerSignals = null,
      string? commonMistakes = null,
      string? resources = null,
      string? relatedPatternIds = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Pattern name is required.", nameof(name));

    Name = name.Trim();
    Description = description?.Trim() ?? string.Empty;

    if (whatItIs is not null)
      WhatItIs = whatItIs.Trim();
    if (whenToUse is not null)
      WhenToUse = whenToUse.Trim();
    if (whyItWorks is not null)
      WhyItWorks = whyItWorks.Trim();
    if (commonUseCases is not null)
      CommonUseCases = commonUseCases;
    if (timeComplexity is not null)
      TimeComplexity = timeComplexity.Trim();
    if (spaceComplexity is not null)
      SpaceComplexity = spaceComplexity.Trim();
    if (pseudoCode is not null)
      PseudoCode = pseudoCode;
    if (triggerSignals is not null)
      TriggerSignals = triggerSignals;
    if (commonMistakes is not null)
      CommonMistakes = commonMistakes;
    if (resources is not null)
      Resources = resources;
    if (relatedPatternIds is not null)
      RelatedPatternIds = relatedPatternIds;

    UpdatedAt = DateTime.UtcNow;
  }
}
