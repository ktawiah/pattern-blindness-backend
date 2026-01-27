using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents the user's thinking during the cold start phase.
/// Captures the deliberate thinking before coding begins.
/// Supports multiple hypothesis mode with primary and secondary pattern choices.
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
  /// Minimum is adaptive based on user performance.
  /// </summary>
  public int ThinkingDurationSeconds { get; private set; }

  /// <summary>
  /// Signals the user identified from the problem.
  /// Free-form text notes.
  /// </summary>
  public string IdentifiedSignals { get; private set; } = "";

  /// <summary>
  /// The primary pattern the user chose to solve the problem.
  /// </summary>
  public Guid ChosenPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the chosen pattern.
  /// </summary>
  public Pattern? ChosenPattern { get; private set; }

  /// <summary>
  /// The secondary/backup pattern hypothesis (multiple hypothesis mode).
  /// </summary>
  public Guid? SecondaryPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the secondary pattern.
  /// </summary>
  public Pattern? SecondaryPattern { get; private set; }

  /// <summary>
  /// Brief reasoning for why primary was ranked over secondary.
  /// </summary>
  public string? PrimaryVsSecondaryReason { get; private set; }

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
  /// Creates a new cold start submission with multiple hypothesis support.
  /// </summary>
  public static ColdStartSubmission Create(
      Guid attemptId,
      string identifiedSignals,
      Guid chosenPatternId,
      Guid? secondaryPatternId,
      string? primaryVsSecondaryReason,
      Guid? rejectedPatternId,
      string? rejectionReason,
      int thinkingDurationSeconds,
      int minimumDurationSeconds = 30)
  {
    if (attemptId == Guid.Empty)
      throw new ArgumentException("Attempt ID is required.", nameof(attemptId));

    if (chosenPatternId == Guid.Empty)
      throw new ArgumentException("Chosen pattern ID is required.", nameof(chosenPatternId));

    // Adaptive minimum - defaults to 30s, can be raised based on performance
    // Allow a small tolerance (5 seconds) for timing differences between frontend and backend
    const int tolerance = 5;
    if (thinkingDurationSeconds < minimumDurationSeconds - tolerance)
      throw new ArgumentException($"Thinking duration must be at least {minimumDurationSeconds - tolerance} seconds.", nameof(thinkingDurationSeconds));

    return new ColdStartSubmission
    {
      Id = Guid.NewGuid(),
      AttemptId = attemptId,
      SubmittedAt = DateTime.UtcNow,
      ThinkingDurationSeconds = thinkingDurationSeconds,
      IdentifiedSignals = identifiedSignals ?? "",
      ChosenPatternId = chosenPatternId,
      SecondaryPatternId = secondaryPatternId,
      PrimaryVsSecondaryReason = primaryVsSecondaryReason?.Trim(),
      RejectedPatternId = rejectedPatternId,
      RejectionReason = rejectionReason?.Trim()
    };
  }
}
