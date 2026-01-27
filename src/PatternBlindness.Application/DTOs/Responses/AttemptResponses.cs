using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Response containing attempt details.
/// </summary>
public record AttemptResponse(
    Guid Id,
    Guid? ProblemId,
    Guid? LeetCodeProblemCacheId,
    string ProblemTitle,
    AttemptStatus Status,
    ConfidenceLevel? Confidence,
    bool? IsPatternCorrect,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int? TotalTimeSeconds,
    ColdStartResponse? ColdStart);

/// <summary>
/// Response containing cold start submission details with multiple hypothesis support.
/// </summary>
public record ColdStartResponse(
    Guid Id,
    string IdentifiedSignals,
    Guid ChosenPatternId,
    string ChosenPatternName,
    Guid? SecondaryPatternId,
    string? SecondaryPatternName,
    string? PrimaryVsSecondaryReason,
    Guid? RejectedPatternId,
    string? RejectedPatternName,
    string? RejectionReason,
    int ThinkingDurationSeconds,
    DateTime SubmittedAt);

/// <summary>
/// Adaptive cold start settings based on user performance.
/// </summary>
public record ColdStartSettingsResponse(
    /// <summary>Recommended minimum duration in seconds (30, 90, or 180).</summary>
    int RecommendedDurationSeconds,
    /// <summary>Performance tier: "new", "good", "moderate", "struggling".</summary>
    string PerformanceTier,
    /// <summary>Recent accuracy as percentage (0-100).</summary>
    double RecentAccuracyPercent,
    /// <summary>Number of recent attempts used for calculation.</summary>
    int AttemptsSampled,
    /// <summary>Whether multiple hypothesis mode is recommended.</summary>
    bool RecommendMultipleHypothesis,
    /// <summary>Interview-style prompt to display.</summary>
    string InterviewPrompt);

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

/// <summary>
/// Response containing LLM-generated reflection for a completed attempt.
/// </summary>
public record ReflectionResponse(
    Guid Id,
    string Feedback,
    string CorrectIdentifications,
    string MissedSignals,
    string NextTimeAdvice,
    string PatternTips,
    string ConfidenceCalibration,
    bool IsCorrectPattern,
    DateTime GeneratedAt);