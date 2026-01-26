using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Response containing attempt details.
/// </summary>
public record AttemptResponse(
    Guid Id,
    Guid ProblemId,
    string ProblemTitle,
    AttemptStatus Status,
    ConfidenceLevel? Confidence,
    bool? IsPatternCorrect,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int? TotalTimeSeconds,
    ColdStartResponse? ColdStart);

/// <summary>
/// Response containing cold start submission details.
/// </summary>
public record ColdStartResponse(
    Guid Id,
    string IdentifiedSignals,
    Guid ChosenPatternId,
    string ChosenPatternName,
    Guid? RejectedPatternId,
    string? RejectedPatternName,
    string? RejectionReason,
    int ThinkingDurationSeconds,
    DateTime SubmittedAt);

/// <summary>
/// Response showing the wrong-but-reasonable approaches after completing.
/// </summary>
public record WrongApproachRevealResponse(
    Guid ProblemId,
    string ProblemTitle,
    Guid CorrectPatternId,
    string CorrectPatternName,
    string SolutionExplanation,
    string KeyInvariant,
    IReadOnlyList<WrongApproachResponse> WrongApproaches);

/// <summary>
/// Details of a wrong approach.
/// </summary>
public record WrongApproachResponse(
    Guid PatternId,
    string PatternName,
    string Explanation,
    int FrequencyPercent);

/// <summary>
/// User's confidence vs correctness dashboard.
/// </summary>
public record ConfidenceDashboardResponse(
    IReadOnlyList<ConfidenceStatsResponse> Stats,
    IReadOnlyList<PatternWeaknessResponse> OverconfidentPatterns,
    IReadOnlyList<PatternWeaknessResponse> FragilePatterns);

/// <summary>
/// Statistics for a confidence level.
/// </summary>
public record ConfidenceStatsResponse(
    ConfidenceLevel Confidence,
    int TotalAttempts,
    int CorrectAttempts,
    int WrongAttempts,
    double CorrectPercentage);

/// <summary>
/// A pattern where user shows weakness.
/// </summary>
public record PatternWeaknessResponse(
    Guid PatternId,
    string PatternName,
    int TotalAttempts,
    int WrongCount,
    double WrongPercentage,
    string Insight);
