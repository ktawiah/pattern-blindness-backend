namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Response containing user profile and qualification status.
/// </summary>
public record UserProfileResponse(
    string UserId,
    bool IsQualified,
    int DsaProblemsCompleted,
    DateTime? QualifiedAt,
    int CurrentPhase,
    int CompletedAttempts,
    bool WasGrandfathered,
    bool InterviewReadinessOptIn,
    FeatureAccessResponse FeatureAccess);

/// <summary>
/// Response indicating which features are accessible based on current phase.
/// </summary>
public record FeatureAccessResponse(
    /// <summary>Current phase (1-4).</summary>
    int Phase,

    /// <summary>Problems completed in current phase.</summary>
    int ProblemsInPhase,

    /// <summary>Problems needed to advance to next phase.</summary>
    int ProblemsToNextPhase,

    /// <summary>Whether confidence metrics are visible.</summary>
    bool ShowConfidenceMetrics,

    /// <summary>Whether pattern usage stats are visible.</summary>
    bool ShowPatternUsageStats,

    /// <summary>Whether blind spot radar is visible.</summary>
    bool ShowBlindSpots,

    /// <summary>Whether pattern decay tracking is visible.</summary>
    bool ShowPatternDecay,

    /// <summary>Whether thinking replay is available.</summary>
    bool ShowThinkingReplay,

    /// <summary>Whether interview readiness mode is available.</summary>
    bool ShowInterviewReadiness);

/// <summary>
/// Response for qualification check (used for gate enforcement).
/// </summary>
public record QualificationCheckResponse(
    bool IsQualified,
    bool NeedsQualification,
    string? Message);

/// <summary>
/// Response indicating an active attempt that must be completed.
/// </summary>
public record ActiveAttemptResponse(
    Guid AttemptId,
    string ProblemTitle,
    DateTime StartedAt,
    string Status);
