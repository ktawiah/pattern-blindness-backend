using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Domain.Events;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents a user's attempt at solving a problem.
/// This is an aggregate root that owns ColdStartSubmission.
/// </summary>
public class Attempt : Entity
{
  private Attempt() { } // EF Core

  /// <summary>
  /// The user who made this attempt.
  /// </summary>
  public string UserId { get; private set; } = string.Empty;

  /// <summary>
  /// The problem being attempted.
  /// </summary>
  public Guid ProblemId { get; private set; }

  /// <summary>
  /// Navigation property for the problem.
  /// </summary>
  public Problem? Problem { get; private set; }

  /// <summary>
  /// When the attempt started.
  /// </summary>
  public DateTime StartedAt { get; private set; }

  /// <summary>
  /// When the attempt was completed (solved, gave up, or timed out).
  /// </summary>
  public DateTime? CompletedAt { get; private set; }

  /// <summary>
  /// Current status of the attempt.
  /// </summary>
  public AttemptStatus Status { get; private set; }

  /// <summary>
  /// The pattern the user chose to solve the problem.
  /// </summary>
  public Guid? ChosenPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the chosen pattern.
  /// </summary>
  public Pattern? ChosenPattern { get; private set; }

  /// <summary>
  /// Whether the chosen pattern was correct.
  /// </summary>
  public bool IsPatternCorrect { get; private set; }

  /// <summary>
  /// User's confidence in their pattern choice.
  /// </summary>
  public ConfidenceLevel Confidence { get; private set; }

  /// <summary>
  /// The cold start submission for this attempt.
  /// </summary>
  public ColdStartSubmission? ColdStartSubmission { get; private set; }

  /// <summary>
  /// Total time spent on the problem in seconds.
  /// </summary>
  public int? TotalTimeSeconds { get; private set; }

  /// <summary>
  /// Creates a new attempt.
  /// </summary>
  public static Attempt Create(string userId, Guid problemId)
  {
    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required.", nameof(userId));

    if (problemId == Guid.Empty)
      throw new ArgumentException("Problem ID is required.", nameof(problemId));

    var attempt = new Attempt
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      ProblemId = problemId,
      StartedAt = DateTime.UtcNow,
      Status = AttemptStatus.InProgress
    };

    attempt.AddDomainEvent(new AttemptStartedEvent(attempt.Id, userId, problemId));
    return attempt;
  }

  /// <summary>
  /// Submits the cold start thinking phase.
  /// </summary>
  public Result SubmitColdStart(
      string identifiedSignals,
      Guid chosenPatternId,
      Guid? rejectedPatternId,
      string? rejectionReason,
      ConfidenceLevel confidence,
      int thinkingDurationSeconds)
  {
    if (Status != AttemptStatus.InProgress)
      return Result.Failure(AttemptErrors.InvalidStatusTransition);

    if (thinkingDurationSeconds < 90)
      return Result.Failure(AttemptErrors.ColdStartTooShort);

    ColdStartSubmission = ColdStartSubmission.Create(
        Id,
        identifiedSignals,
        chosenPatternId,
        rejectedPatternId,
        rejectionReason,
        thinkingDurationSeconds);

    ChosenPatternId = chosenPatternId;
    Confidence = confidence;
    Status = AttemptStatus.ColdStartCompleted;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new ColdStartCompletedEvent(Id, chosenPatternId, confidence));
    return Result.Success();
  }

  /// <summary>
  /// Marks the attempt as solved.
  /// </summary>
  public Result Complete(bool isPatternCorrect)
  {
    if (Status != AttemptStatus.ColdStartCompleted)
      return Result.Failure(AttemptErrors.ColdStartRequired);

    IsPatternCorrect = isPatternCorrect;
    Status = AttemptStatus.Solved;
    CompletedAt = DateTime.UtcNow;
    TotalTimeSeconds = (int)(CompletedAt.Value - StartedAt).TotalSeconds;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new AttemptCompletedEvent(Id, UserId, IsPatternCorrect, Confidence));
    return Result.Success();
  }

  /// <summary>
  /// Marks the attempt as given up.
  /// </summary>
  public Result GiveUp()
  {
    if (Status is AttemptStatus.Solved or AttemptStatus.GaveUp or AttemptStatus.TimedOut)
      return Result.Failure(AttemptErrors.AlreadyCompleted);

    IsPatternCorrect = false;
    Status = AttemptStatus.GaveUp;
    CompletedAt = DateTime.UtcNow;
    TotalTimeSeconds = (int)(CompletedAt.Value - StartedAt).TotalSeconds;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success();
  }

  /// <summary>
  /// Marks the attempt as timed out.
  /// </summary>
  public Result TimeOut()
  {
    if (Status is AttemptStatus.Solved or AttemptStatus.GaveUp or AttemptStatus.TimedOut)
      return Result.Failure(AttemptErrors.AlreadyCompleted);

    IsPatternCorrect = false;
    Status = AttemptStatus.TimedOut;
    CompletedAt = DateTime.UtcNow;
    TotalTimeSeconds = (int)(CompletedAt.Value - StartedAt).TotalSeconds;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success();
  }
}

/// <summary>
/// Domain errors for Attempt entity.
/// </summary>
public static class AttemptErrors
{
  public static readonly Error NotFound = new("Attempt.NotFound", "Attempt not found.");
  public static readonly Error AlreadyCompleted = new("Attempt.AlreadyCompleted", "Attempt is already completed.");
  public static readonly Error ColdStartRequired = new("Attempt.ColdStartRequired", "Cold start submission is required before completing.");
  public static readonly Error ColdStartTooShort = new("Attempt.ColdStartTooShort", "Cold start thinking phase must be at least 90 seconds.");
  public static readonly Error InvalidStatusTransition = new("Attempt.InvalidStatusTransition", "Invalid status transition.");
}
