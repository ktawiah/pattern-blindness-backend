using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// LLM-generated analysis of a LeetCode problem.
/// Cached permanently after first analysis to avoid repeated LLM calls.
/// </summary>
public class ProblemAnalysis : Entity
{
  private ProblemAnalysis() { } // EF Core

  /// <summary>
  /// The cached problem this analysis belongs to.
  /// </summary>
  public Guid LeetCodeProblemCacheId { get; private set; }

  /// <summary>
  /// Navigation property for the problem.
  /// </summary>
  public LeetCodeProblemCache? Problem { get; private set; }

  /// <summary>
  /// Primary patterns applicable to this problem.
  /// JSON array of pattern identifiers (e.g., ["DynamicProgramming", "Memoization"]).
  /// </summary>
  public string PrimaryPatterns { get; private set; } = "[]";

  /// <summary>
  /// Secondary/alternative patterns that could work.
  /// JSON array of pattern identifiers.
  /// </summary>
  public string SecondaryPatterns { get; private set; } = "[]";

  /// <summary>
  /// Key signals/keywords that indicate which pattern to use.
  /// JSON array of signal objects with signal and explanation.
  /// </summary>
  public string KeySignals { get; private set; } = "[]";

  /// <summary>
  /// Common mistakes or wrong approaches for this problem.
  /// JSON array of mistake objects with description and why it fails.
  /// </summary>
  public string CommonMistakes { get; private set; } = "[]";

  /// <summary>
  /// Time complexity analysis.
  /// </summary>
  public string TimeComplexity { get; private set; } = string.Empty;

  /// <summary>
  /// Space complexity analysis.
  /// </summary>
  public string SpaceComplexity { get; private set; } = string.Empty;

  /// <summary>
  /// The key insight or invariant needed to solve the problem.
  /// </summary>
  public string KeyInsight { get; private set; } = string.Empty;

  /// <summary>
  /// Step-by-step approach explanation (not full solution code).
  /// </summary>
  public string ApproachExplanation { get; private set; } = string.Empty;

  /// <summary>
  /// Similar problems for further practice.
  /// JSON array of problem slugs or descriptions.
  /// </summary>
  public string SimilarProblems { get; private set; } = "[]";

  /// <summary>
  /// The LLM model used for this analysis.
  /// </summary>
  public string ModelUsed { get; private set; } = string.Empty;

  /// <summary>
  /// When the analysis was generated.
  /// </summary>
  public DateTime AnalyzedAt { get; private set; }

  /// <summary>
  /// Raw LLM response (for debugging/auditing).
  /// </summary>
  public string? RawLlmResponse { get; private set; }

  /// <summary>
  /// Creates a new problem analysis.
  /// </summary>
  public static ProblemAnalysis Create(
      Guid leetCodeProblemCacheId,
      string primaryPatterns,
      string secondaryPatterns,
      string keySignals,
      string commonMistakes,
      string timeComplexity,
      string spaceComplexity,
      string keyInsight,
      string approachExplanation,
      string similarProblems,
      string modelUsed,
      string? rawLlmResponse = null)
  {
    if (leetCodeProblemCacheId == Guid.Empty)
      throw new ArgumentException("Problem cache ID is required.", nameof(leetCodeProblemCacheId));

    return new ProblemAnalysis
    {
      Id = Guid.NewGuid(),
      LeetCodeProblemCacheId = leetCodeProblemCacheId,
      PrimaryPatterns = primaryPatterns,
      SecondaryPatterns = secondaryPatterns,
      KeySignals = keySignals,
      CommonMistakes = commonMistakes,
      TimeComplexity = timeComplexity,
      SpaceComplexity = spaceComplexity,
      KeyInsight = keyInsight,
      ApproachExplanation = approachExplanation,
      SimilarProblems = similarProblems,
      ModelUsed = modelUsed,
      RawLlmResponse = rawLlmResponse,
      AnalyzedAt = DateTime.UtcNow
    };
  }
}
