using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// LLM-generated personalized reflection for a user's attempt.
/// Created after the user completes the cold start and reveals the solution.
/// </summary>
public class Reflection : Entity
{
  private Reflection() { } // EF Core

  /// <summary>
  /// The attempt this reflection is for.
  /// </summary>
  public Guid AttemptId { get; private set; }

  /// <summary>
  /// Navigation property for the attempt.
  /// </summary>
  public Attempt? Attempt { get; private set; }

  /// <summary>
  /// Summary of what the user identified during cold start.
  /// </summary>
  public string UserColdStartSummary { get; private set; } = string.Empty;

  /// <summary>
  /// Whether the user's pattern choice was correct.
  /// </summary>
  public bool WasPatternCorrect { get; private set; }

  /// <summary>
  /// LLM-generated feedback on the user's approach.
  /// </summary>
  public string Feedback { get; private set; } = string.Empty;

  /// <summary>
  /// What signals the user correctly identified.
  /// JSON array.
  /// </summary>
  public string CorrectIdentifications { get; private set; } = "[]";

  /// <summary>
  /// What signals the user missed or misinterpreted.
  /// JSON array.
  /// </summary>
  public string MissedSignals { get; private set; } = "[]";

  /// <summary>
  /// Actionable advice for next time.
  /// </summary>
  public string NextTimeAdvice { get; private set; } = string.Empty;

  /// <summary>
  /// Pattern-specific tips based on this problem.
  /// </summary>
  public string PatternTips { get; private set; } = string.Empty;

  /// <summary>
  /// Confidence calibration feedback.
  /// </summary>
  public string ConfidenceCalibration { get; private set; } = string.Empty;

  /// <summary>
  /// The LLM model used for this reflection.
  /// </summary>
  public string ModelUsed { get; private set; } = string.Empty;

  /// <summary>
  /// When the reflection was generated.
  /// </summary>
  public DateTime GeneratedAt { get; private set; }

  /// <summary>
  /// Raw LLM response (for debugging/auditing).
  /// </summary>
  public string? RawLlmResponse { get; private set; }

  /// <summary>
  /// Creates a new reflection.
  /// </summary>
  public static Reflection Create(
      Guid attemptId,
      string userColdStartSummary,
      bool wasPatternCorrect,
      string feedback,
      string correctIdentifications,
      string missedSignals,
      string nextTimeAdvice,
      string patternTips,
      string confidenceCalibration,
      string modelUsed,
      string? rawLlmResponse = null)
  {
    if (attemptId == Guid.Empty)
      throw new ArgumentException("Attempt ID is required.", nameof(attemptId));

    return new Reflection
    {
      Id = Guid.NewGuid(),
      AttemptId = attemptId,
      UserColdStartSummary = userColdStartSummary,
      WasPatternCorrect = wasPatternCorrect,
      Feedback = feedback,
      CorrectIdentifications = correctIdentifications,
      MissedSignals = missedSignals,
      NextTimeAdvice = nextTimeAdvice,
      PatternTips = patternTips,
      ConfidenceCalibration = confidenceCalibration,
      ModelUsed = modelUsed,
      RawLlmResponse = rawLlmResponse,
      GeneratedAt = DateTime.UtcNow
    };
  }
}
