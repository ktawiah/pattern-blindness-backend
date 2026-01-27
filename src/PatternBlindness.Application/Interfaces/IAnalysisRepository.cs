using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository for problem analyses and reflections.
/// </summary>
public interface IAnalysisRepository
{
  /// <summary>
  /// Gets the analysis for a cached problem.
  /// </summary>
  Task<ProblemAnalysis?> GetAnalysisByProblemIdAsync(Guid leetCodeProblemCacheId, CancellationToken ct = default);

  /// <summary>
  /// Adds a new problem analysis.
  /// </summary>
  Task<ProblemAnalysis> AddAnalysisAsync(ProblemAnalysis analysis, CancellationToken ct = default);

  /// <summary>
  /// Gets the reflection for an attempt.
  /// </summary>
  Task<Reflection?> GetReflectionByAttemptIdAsync(Guid attemptId, CancellationToken ct = default);

  /// <summary>
  /// Adds a new reflection.
  /// </summary>
  Task<Reflection> AddReflectionAsync(Reflection reflection, CancellationToken ct = default);
}
