using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Requests;

/// <summary>
/// Request to start a new problem attempt.
/// </summary>
public record StartAttemptRequest(Guid ProblemId);

/// <summary>
/// Request to submit the cold start thinking phase.
/// </summary>
public record SubmitColdStartRequest(
    /// <summary>JSON array of signals identified from the problem.</summary>
    string IdentifiedSignals,

    /// <summary>The pattern chosen to solve the problem.</summary>
    Guid ChosenPatternId,

    /// <summary>A pattern considered but rejected (optional).</summary>
    Guid? RejectedPatternId,

    /// <summary>Why the rejected pattern was not chosen.</summary>
    string? RejectionReason,

    /// <summary>User's confidence in their pattern choice.</summary>
    ConfidenceLevel Confidence,

    /// <summary>Time spent thinking in seconds (must be >= 90).</summary>
    int ThinkingDurationSeconds);

/// <summary>
/// Request to complete an attempt.
/// </summary>
public record CompleteAttemptRequest(bool IsPatternCorrect);
