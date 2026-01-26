namespace PatternBlindness.Domain.Enums;

/// <summary>
/// Represents the current status of an attempt.
/// </summary>
public enum AttemptStatus
{
  /// <summary>Attempt is in progress, user hasn't completed cold start yet</summary>
  InProgress = 1,

  /// <summary>Cold start phase completed, user is now coding</summary>
  ColdStartCompleted = 2,

  /// <summary>User successfully solved the problem</summary>
  Solved = 3,

  /// <summary>User gave up on the problem</summary>
  GaveUp = 4,

  /// <summary>Attempt timed out</summary>
  TimedOut = 5
}
