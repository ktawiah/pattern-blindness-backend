using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository interface for Attempt entity operations.
/// </summary>
public interface IAttemptRepository
{
  Task<Attempt?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<Attempt?> GetByIdWithColdStartAsync(Guid id, CancellationToken ct = default);
  Task<Attempt?> GetByIdWithReflectionAsync(Guid id, CancellationToken ct = default);
  Task<IReadOnlyList<Attempt>> GetByUserIdAsync(string userId, CancellationToken ct = default);
  Task<IReadOnlyList<Attempt>> GetByUserIdAndStatusAsync(string userId, AttemptStatus status, CancellationToken ct = default);
  Task<IReadOnlyList<Attempt>> GetRecentByUserIdAsync(string userId, int count = 10, CancellationToken ct = default);
  Task<Attempt> AddAsync(Attempt attempt, CancellationToken ct = default);
  Task UpdateAsync(Attempt attempt, CancellationToken ct = default);

  /// <summary>
  /// Gets the active (in-progress or cold-start-completed) attempt for a user.
  /// Used for loop enforcement - users must complete current attempt before starting a new one.
  /// </summary>
  Task<Attempt?> GetActiveAttemptByUserIdAsync(string userId, CancellationToken ct = default);

  /// <summary>
  /// Gets the count of completed attempts for a user.
  /// Used for phase progression.
  /// </summary>
  Task<int> GetCompletedAttemptCountAsync(string userId, CancellationToken ct = default);

  /// <summary>
  /// Gets the count of attempts by user grouped by correctness and confidence.
  /// Used for confidence vs correctness tracking.
  /// </summary>
  Task<IReadOnlyList<ConfidenceStats>> GetConfidenceStatsAsync(string userId, CancellationToken ct = default);

  /// <summary>
  /// Gets patterns where user is consistently overconfident (high confidence + wrong).
  /// </summary>
  Task<IReadOnlyList<PatternWeakness>> GetOverconfidentPatternsAsync(string userId, int limit = 2, CancellationToken ct = default);

  /// <summary>
  /// Gets patterns where user is fragile (low confidence + right).
  /// </summary>
  Task<IReadOnlyList<PatternWeakness>> GetFragilePatternsAsync(string userId, int limit = 2, CancellationToken ct = default);
}

/// <summary>
/// Statistics for confidence vs correctness tracking.
/// </summary>
public record ConfidenceStats(
    ConfidenceLevel Confidence,
    int TotalAttempts,
    int CorrectAttempts,
    int WrongAttempts);

/// <summary>
/// Represents a pattern weakness (overconfidence or fragile understanding).
/// </summary>
public record PatternWeakness(
    Guid PatternId,
    string PatternName,
    int TotalAttempts,
    int WrongCount,
    double WrongPercentage);
