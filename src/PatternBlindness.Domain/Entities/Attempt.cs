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
  /// The legacy problem being attempted (for backwards compatibility).
  /// </summary>
  public Guid? ProblemId { get; private set; }

  /// <summary>
  /// Navigation property for the legacy problem.
  /// </summary>
  public Problem? Problem { get; private set; }

  /// <summary>
  /// The LeetCode problem being attempted (new dynamic flow).
  /// </summary>
  public Guid? LeetCodeProblemCacheId { get; private set; }

  /// <summary>
  /// Navigation property for the LeetCode problem.
  /// </summary>
  public LeetCodeProblemCache? LeetCodeProblem { get; private set; }

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
  /// The pattern the user chose to solve the problem (legacy, pattern ID).
  /// </summary>
  public Guid? ChosenPatternId { get; private set; }

  /// <summary>
  /// The pattern name the user chose (new flow, string-based).
  /// </summary>
  public string? ChosenPatternName { get; private set; }

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
  /// The personalized reflection for this attempt.
  /// </summary>
  public Reflection? Reflection { get; private set; }

  /// <summary>
  /// Total time spent on the problem in seconds.
  /// </summary>
  public int? TotalTimeSeconds { get; private set; }

  // ===== RETURN GATE FIELDS =====

  /// <summary>
  /// Outcome of the user's chosen approach (Worked/PartiallyWorked/Failed).
  /// Captured in the return gate after coding.
  /// </summary>
  public ApproachOutcome? Outcome { get; private set; }

  /// <summary>
  /// What broke first if the approach failed or partially worked.
  /// </summary>
  public FailureReason? FirstFailure { get; private set; }

  /// <summary>
  /// Whether the user switched approaches mid-solve.
  /// Indicates panic switching behavior.
  /// </summary>
  public bool? SwitchedApproachMidSolve { get; private set; }

  /// <summary>
  /// Reason for switching approaches (if SwitchedApproachMidSolve is true).
  /// </summary>
  public string? SwitchReason { get; private set; }

  /// <summary>
  /// Creates a new attempt for a legacy Problem.
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
  /// Creates a new attempt for a LeetCode problem.
  /// </summary>
  public static Attempt CreateForLeetCode(string userId, Guid leetCodeProblemCacheId)
  {
    if (string.IsNullOrWhiteSpace(userId))
      throw new ArgumentException("User ID is required.", nameof(userId));

    if (leetCodeProblemCacheId == Guid.Empty)
      throw new ArgumentException("LeetCode problem cache ID is required.", nameof(leetCodeProblemCacheId));

    var attempt = new Attempt
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      LeetCodeProblemCacheId = leetCodeProblemCacheId,
      StartedAt = DateTime.UtcNow,
      Status = AttemptStatus.InProgress
    };

    attempt.AddDomainEvent(new AttemptStartedEvent(attempt.Id, userId, leetCodeProblemCacheId));
    return attempt;
  }

  /// <summary>
  /// Submits the cold start thinking phase with multiple hypothesis support.
  /// </summary>
  public Result SubmitColdStart(
      string identifiedSignals,
      Guid chosenPatternId,
      Guid? secondaryPatternId,
      string? primaryVsSecondaryReason,
      Guid? rejectedPatternId,
      string? rejectionReason,
      ConfidenceLevel? confidence,
      int thinkingDurationSeconds,
      int minimumDurationSeconds = 30,
      string? keyInvariant = null,
      string? primaryRisk = null)
  {
    if (Status != AttemptStatus.InProgress)
      return Result.Failure(AttemptErrors.InvalidStatusTransition);

    // Note: Minimum duration is now adaptive (30s → 90s → 180s based on performance)
    // The frontend calculates and enforces the timer, backend validates minimum

    ColdStartSubmission = ColdStartSubmission.Create(
        Id,
        identifiedSignals,
        chosenPatternId,
        secondaryPatternId,
        primaryVsSecondaryReason,
        rejectedPatternId,
        rejectionReason,
        thinkingDurationSeconds,
        minimumDurationSeconds,
        keyInvariant,
        primaryRisk);

    ChosenPatternId = chosenPatternId;
    if (confidence.HasValue)
    {
      Confidence = confidence.Value;
    }
    Status = AttemptStatus.ColdStartCompleted;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new ColdStartCompletedEvent(Id, chosenPatternId, Confidence));
    return Result.Success();
  }

  /// <summary>
  /// Marks the attempt as solved with return gate data.
  /// </summary>
  public Result Complete(
      ApproachOutcome outcome,
      FailureReason? firstFailure,
      bool switchedApproach,
      string? switchReason,
      int? confidenceLevel = null)
  {
    if (Status != AttemptStatus.ColdStartCompleted)
      return Result.Failure(AttemptErrors.ColdStartRequired);

    // Determine if pattern was correct based on outcome
    IsPatternCorrect = outcome == ApproachOutcome.Worked;

    // Set return gate fields
    Outcome = outcome;
    FirstFailure = firstFailure;
    SwitchedApproachMidSolve = switchedApproach;
    SwitchReason = switchReason?.Trim();

    // Set confidence from the complete request if provided
    if (confidenceLevel.HasValue && confidenceLevel.Value >= 1 && confidenceLevel.Value <= 5)
    {
      Confidence = (ConfidenceLevel)confidenceLevel.Value;
    }

    Status = AttemptStatus.Solved;
    CompletedAt = DateTime.UtcNow;
    TotalTimeSeconds = (int)(CompletedAt.Value - StartedAt).TotalSeconds;
    UpdatedAt = DateTime.UtcNow;

    AddDomainEvent(new AttemptCompletedEvent(Id, UserId, IsPatternCorrect, Confidence));
    return Result.Success();
  }

  /// <summary>
  /// Marks the attempt as solved (legacy method for backwards compatibility).
  /// </summary>
  public Result Complete(bool isPatternCorrect, int? confidenceLevel = null)
  {
    if (Status != AttemptStatus.ColdStartCompleted)
      return Result.Failure(AttemptErrors.ColdStartRequired);

    IsPatternCorrect = isPatternCorrect;

    // Infer outcome from isPatternCorrect for legacy calls
    Outcome = isPatternCorrect ? ApproachOutcome.Worked : ApproachOutcome.Failed;

    // Set confidence from the complete request if provided
    if (confidenceLevel.HasValue && confidenceLevel.Value >= 1 && confidenceLevel.Value <= 5)
    {
      Confidence = (ConfidenceLevel)confidenceLevel.Value;
    }

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

  /// <summary>
  /// Sets the chosen pattern name (for LeetCode flow with string-based patterns).
  /// </summary>
  public void SetChosenPattern(string patternName)
  {
    if (string.IsNullOrWhiteSpace(patternName))
      throw new ArgumentException("Pattern name is required.", nameof(patternName));

    ChosenPatternName = patternName;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Sets whether the pattern choice was correct.
  /// </summary>
  public void SetPatternCorrectness(bool isCorrect)
  {
    IsPatternCorrect = isCorrect;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Completes a LeetCode-based attempt after reflection is generated.
  /// LeetCode attempts don't go through cold start, so this completes from InProgress status.
  /// </summary>
  public Result CompleteLeetCodeAttempt(
      bool isPatternCorrect,
      int? confidenceLevel = null,
      ApproachOutcome? outcome = null)
  {
    // LeetCode attempts can be completed from InProgress (no cold start) or ColdStartCompleted
    if (Status != AttemptStatus.InProgress && Status != AttemptStatus.ColdStartCompleted)
      return Result.Failure(AttemptErrors.AlreadyCompleted);

    // Must be a LeetCode attempt
    if (!LeetCodeProblemCacheId.HasValue)
      return Result.Failure(new Error("Attempt.NotLeetCodeAttempt", "This method is only for LeetCode-based attempts"));

    IsPatternCorrect = isPatternCorrect;
    Outcome = outcome ?? (isPatternCorrect ? ApproachOutcome.Worked : ApproachOutcome.Failed);

    if (confidenceLevel.HasValue && confidenceLevel.Value >= 1 && confidenceLevel.Value <= 5)
    {
      Confidence = (ConfidenceLevel)confidenceLevel.Value;
    }

    Status = AttemptStatus.Solved;
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
  public static readonly Error ActiveAttemptExists = new("Attempt.ActiveAttemptExists", "You must complete or abandon your current attempt before starting a new one.");
}
