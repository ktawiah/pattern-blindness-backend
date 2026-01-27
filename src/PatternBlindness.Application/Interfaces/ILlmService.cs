using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Service for LLM-powered problem analysis and reflection generation.
/// </summary>
public interface ILlmService
{
  /// <summary>
  /// Analyzes a LeetCode problem to identify patterns, signals, and common mistakes.
  /// </summary>
  Task<ProblemAnalysisResult> AnalyzeProblemAsync(
      string title,
      string content,
      IReadOnlyList<string> tags,
      string difficulty,
      CancellationToken ct = default);

  /// <summary>
  /// Generates a personalized reflection based on the user's cold start and the problem analysis.
  /// </summary>
  Task<ReflectionResult> GenerateReflectionAsync(
      string problemTitle,
      string problemContent,
      ProblemAnalysis analysis,
      ColdStartSubmission coldStart,
      bool wasPatternCorrect,
      int confidenceLevel,
      CancellationToken ct = default);

  /// <summary>
  /// Generates a personalized reflection for the new LeetCode flow (without ColdStartSubmission entity).
  /// </summary>
  Task<ReflectionResult> GenerateReflectionAsync(
      string problemTitle,
      IReadOnlyList<string> correctPatterns,
      IReadOnlyList<string> keySignals,
      string userChosenPattern,
      string userIdentifiedSignals,
      int confidenceLevel,
      CancellationToken ct = default);
}

/// <summary>
/// Result of LLM problem analysis.
/// </summary>
public record ProblemAnalysisResult(
    IReadOnlyList<string> PrimaryPatterns,
    IReadOnlyList<string> SecondaryPatterns,
    IReadOnlyList<SignalInfo> KeySignals,
    IReadOnlyList<MistakeInfo> CommonMistakes,
    string TimeComplexity,
    string SpaceComplexity,
    string KeyInsight,
    string ApproachExplanation,
    IReadOnlyList<string> SimilarProblems,
    string RawResponse
);

/// <summary>
/// A signal that indicates a pattern.
/// </summary>
public record SignalInfo(
    string Signal,
    string Explanation,
    string IndicatesPattern
);

/// <summary>
/// A common mistake for a problem.
/// </summary>
public record MistakeInfo(
    string Mistake,
    string WhyItFails,
    string BetterApproach
);

/// <summary>
/// Result of LLM reflection generation.
/// </summary>
public record ReflectionResult(
    string Feedback,
    IReadOnlyList<string> CorrectIdentifications,
    IReadOnlyList<string> MissedSignals,
    string NextTimeAdvice,
    string PatternTips,
    string ConfidenceCalibration,
    bool IsCorrectPattern,
    string RawResponse
);
