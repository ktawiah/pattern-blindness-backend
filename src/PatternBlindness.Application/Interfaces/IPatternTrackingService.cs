namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Service for tracking pattern usage statistics and identifying blind spots.
/// </summary>
public interface IPatternTrackingService
{
    /// <summary>
    /// Gets patterns that haven't been used in the specified number of days.
    /// </summary>
    Task<IReadOnlyList<DecayingPatternInfo>> GetDecayingPatternsAsync(
        string userId,
        int daysThreshold = 30,
        CancellationToken ct = default);

    /// <summary>
    /// Gets patterns that the user tends to choose repeatedly (potential over-reliance).
    /// </summary>
    Task<IReadOnlyList<DefaultPatternInfo>> GetDefaultPatternsAsync(
        string userId,
        int minOccurrences = 5,
        CancellationToken ct = default);

    /// <summary>
    /// Gets patterns that the user has never chosen despite them being correct answers.
    /// </summary>
    Task<IReadOnlyList<AvoidedPatternInfo>> GetAvoidedPatternsAsync(
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets complete pattern usage statistics for the user.
    /// </summary>
    Task<PatternUsageStatsResult> GetPatternUsageStatsAsync(
        string userId,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if the user is about to choose a pattern they've chosen many times in a row.
    /// Returns a nudge message if so.
    /// </summary>
    Task<PatternNudge?> CheckForPatternNudgeAsync(
        string userId,
        Guid patternId,
        int consecutiveThreshold = 3,
        CancellationToken ct = default);
}

/// <summary>
/// Information about a pattern that's decaying (not recently practiced).
/// </summary>
public record DecayingPatternInfo(
    Guid PatternId,
    string PatternName,
    DateTime LastUsedAt,
    int DaysSinceLastUse,
    int TotalTimesUsed,
    double SuccessRate);

/// <summary>
/// Information about a pattern the user frequently defaults to.
/// </summary>
public record DefaultPatternInfo(
    Guid PatternId,
    string PatternName,
    int TimesChosen,
    int ConsecutiveChoices,
    double PercentageOfTotal,
    double SuccessRate);

/// <summary>
/// Information about a pattern the user has been avoiding.
/// </summary>
public record AvoidedPatternInfo(
    Guid PatternId,
    string PatternName,
    int TimesCorrectAnswer,
    int TimesUserChoseIt);

/// <summary>
/// Complete pattern usage statistics.
/// </summary>
public record PatternUsageStatsResult(
    IReadOnlyList<DecayingPatternInfo> DecayingPatterns,
    IReadOnlyList<DefaultPatternInfo> DefaultPatterns,
    IReadOnlyList<AvoidedPatternInfo> AvoidedPatterns,
    int TotalAttempts,
    int UniquePatternsPracticed,
    int TotalPatterns);

/// <summary>
/// A nudge to show the user when they're over-relying on a pattern.
/// </summary>
public record PatternNudge(
    Guid PatternId,
    string PatternName,
    int ConsecutiveChoices,
    string Message);
