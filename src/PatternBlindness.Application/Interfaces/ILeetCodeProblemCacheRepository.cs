using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository for cached LeetCode problems.
/// </summary>
public interface ILeetCodeProblemCacheRepository
{
  /// <summary>
  /// Gets a cached problem by its title slug.
  /// </summary>
  Task<LeetCodeProblemCache?> GetBySlugAsync(string titleSlug, CancellationToken ct = default);

  /// <summary>
  /// Gets a cached problem by its ID.
  /// </summary>
  Task<LeetCodeProblemCache?> GetByIdAsync(Guid id, CancellationToken ct = default);

  /// <summary>
  /// Gets a cached problem by its LeetCode ID.
  /// </summary>
  Task<LeetCodeProblemCache?> GetByLeetCodeIdAsync(string leetCodeId, CancellationToken ct = default);

  /// <summary>
  /// Gets a cached problem with its analysis by title slug.
  /// </summary>
  Task<LeetCodeProblemCache?> GetWithAnalysisAsync(string titleSlug, CancellationToken ct = default);

  /// <summary>
  /// Gets a cached problem with its analysis by ID.
  /// </summary>
  Task<LeetCodeProblemCache?> GetWithAnalysisAsync(Guid id, CancellationToken ct = default);

  /// <summary>
  /// Adds a new cached problem.
  /// </summary>
  Task<LeetCodeProblemCache> AddAsync(LeetCodeProblemCache problem, CancellationToken ct = default);

  /// <summary>
  /// Updates an existing cached problem.
  /// </summary>
  Task UpdateAsync(LeetCodeProblemCache problem, CancellationToken ct = default);

  /// <summary>
  /// Searches cached problems by title.
  /// </summary>
  Task<IReadOnlyList<LeetCodeProblemCache>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);

  /// <summary>
  /// Gets problems that need analysis (no ProblemAnalysis attached).
  /// </summary>
  Task<IReadOnlyList<LeetCodeProblemCache>> GetProblemsNeedingAnalysisAsync(int limit = 10, CancellationToken ct = default);
}
