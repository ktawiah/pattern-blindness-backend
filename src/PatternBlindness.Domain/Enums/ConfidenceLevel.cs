namespace PatternBlindness.Domain.Enums;

/// <summary>
/// Represents the user's confidence level when choosing a pattern.
/// Used to track overconfidence and fragile understanding.
/// </summary>
public enum ConfidenceLevel
{
  /// <summary>User is guessing, no real confidence</summary>
  Guessing = 1,

  /// <summary>User thinks this might be right but isn't sure</summary>
  Uncertain = 2,

  /// <summary>User is fairly confident in the choice</summary>
  Confident = 3,

  /// <summary>User is certain this is the correct approach</summary>
  VeryConfident = 4
}
