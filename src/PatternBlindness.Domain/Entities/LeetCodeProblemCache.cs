using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Cached LeetCode problem data.
/// This allows us to work with problems without requiring a static Problem entity.
/// Problems are fetched from LeetCode on-demand and cached here.
/// </summary>
public class LeetCodeProblemCache : Entity
{
  private LeetCodeProblemCache() { } // EF Core

  /// <summary>
  /// LeetCode's internal question ID.
  /// </summary>
  public string LeetCodeId { get; private set; } = string.Empty;

  /// <summary>
  /// The frontend display ID (e.g., "1" for Two Sum).
  /// </summary>
  public string FrontendId { get; private set; } = string.Empty;

  /// <summary>
  /// Problem title.
  /// </summary>
  public string Title { get; private set; } = string.Empty;

  /// <summary>
  /// URL-friendly slug (e.g., "two-sum").
  /// </summary>
  public string TitleSlug { get; private set; } = string.Empty;

  /// <summary>
  /// Problem difficulty: Easy, Medium, Hard.
  /// </summary>
  public string Difficulty { get; private set; } = string.Empty;

  /// <summary>
  /// Full problem content/description.
  /// </summary>
  public string Content { get; private set; } = string.Empty;

  /// <summary>
  /// LeetCode tags as JSON array (e.g., ["Array", "Hash Table"]).
  /// </summary>
  public string Tags { get; private set; } = "[]";

  /// <summary>
  /// Example test cases as JSON array.
  /// </summary>
  public string Examples { get; private set; } = "[]";

  /// <summary>
  /// Problem hints from LeetCode as JSON array.
  /// </summary>
  public string Hints { get; private set; } = "[]";

  /// <summary>
  /// Acceptance rate percentage.
  /// </summary>
  public double AcceptanceRate { get; private set; }

  /// <summary>
  /// When the problem was first cached.
  /// </summary>
  public DateTime CachedAt { get; private set; }

  /// <summary>
  /// When the cache was last refreshed.
  /// </summary>
  public DateTime LastRefreshedAt { get; private set; }

  /// <summary>
  /// Associated LLM analysis for this problem (if analyzed).
  /// </summary>
  public ProblemAnalysis? Analysis { get; private set; }

  /// <summary>
  /// Attempts made on this problem.
  /// </summary>
  public ICollection<Attempt> Attempts { get; private set; } = [];

  /// <summary>
  /// Creates a new cached LeetCode problem.
  /// </summary>
  public static LeetCodeProblemCache Create(
      string leetCodeId,
      string frontendId,
      string title,
      string titleSlug,
      string difficulty,
      string content,
      string tags,
      string examples,
      string hints,
      double acceptanceRate)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(leetCodeId);
    ArgumentException.ThrowIfNullOrWhiteSpace(titleSlug);

    var now = DateTime.UtcNow;

    return new LeetCodeProblemCache
    {
      Id = Guid.NewGuid(),
      LeetCodeId = leetCodeId,
      FrontendId = frontendId,
      Title = title,
      TitleSlug = titleSlug,
      Difficulty = difficulty,
      Content = content,
      Tags = tags,
      Examples = examples,
      Hints = hints,
      AcceptanceRate = acceptanceRate,
      CachedAt = now,
      LastRefreshedAt = now
    };
  }

  /// <summary>
  /// Updates the cached data with fresh content from LeetCode.
  /// </summary>
  public void RefreshContent(
      string content,
      string tags,
      string examples,
      string hints,
      double acceptanceRate)
  {
    Content = content;
    Tags = tags;
    Examples = examples;
    Hints = hints;
    AcceptanceRate = acceptanceRate;
    LastRefreshedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }
}
