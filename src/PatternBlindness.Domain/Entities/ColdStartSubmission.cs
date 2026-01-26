using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents the user's thinking during the cold start phase.
/// Captures the deliberate thinking before coding begins.
/// </summary>
public class ColdStartSubmission : Entity
{
  private ColdStartSubmission() { } // EF Core

  /// <summary>
  /// The attempt this submission belongs to.
  /// </summary>
  public Guid AttemptId { get; private set; }

  /// <summary>
  /// Navigation property for the attempt.
  /// </summary>
  public Attempt? Attempt { get; private set; }

  /// <summary>
  /// When the cold start was submitted.
  /// </summary>
  public DateTime SubmittedAt { get; private set; }

  /// <summary>
  /// How long the user spent thinking (in seconds).
  /// Must be at least 90 seconds.
  /// </summary>
  public int ThinkingDurationSeconds { get; private set; }

  /// <summary>
  /// Signals the user identified from the problem.
  /// Stored as JSON array.
  /// </summary>
  public string IdentifiedSignals { get; private set; } = "[]";

  /// <summary>
  /// The pattern the user chose to solve the problem.
  /// </summary>
  public Guid ChosenPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the chosen pattern.
  /// </summary>
  public Pattern? ChosenPattern { get; private set; }

  /// <summary>
  /// The pattern the user considered but rejected.
  /// </summary>
  public Guid? RejectedPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the rejected pattern.
  /// </summary>
  public Pattern? RejectedPattern { get; private set; }

  /// <summary>
  /// Why the user rejected the alternative pattern.
  /// </summary>
  public string? RejectionReason { get; private set; }

  /// <summary>
  /// Creates a new cold start submission.
  /// </summary>
  public static ColdStartSubmission Create(
      Guid attemptId,
      string identifiedSignals,
      Guid chosenPatternId,
      Guid? rejectedPatternId,
      string? rejectionReason,
      int thinkingDurationSeconds)
  {
    if (attemptId == Guid.Empty)
      throw new ArgumentException("Attempt ID is required.", nameof(attemptId));

    if (chosenPatternId == Guid.Empty)
      throw new ArgumentException("Chosen pattern ID is required.", nameof(chosenPatternId));

    if (thinkingDurationSeconds < 90)
      throw new ArgumentException("Thinking duration must be at least 90 seconds.", nameof(thinkingDurationSeconds));

    return new ColdStartSubmission
    {
      Id = Guid.NewGuid(),
      AttemptId = attemptId,
      SubmittedAt = DateTime.UtcNow,
      ThinkingDurationSeconds = thinkingDurationSeconds,
      IdentifiedSignals = identifiedSignals ?? "[]",
      ChosenPatternId = chosenPatternId,
      RejectedPatternId = rejectedPatternId,
      RejectionReason = rejectionReason?.Trim()
    };
  }
}
