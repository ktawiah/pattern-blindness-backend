namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Service interface for interacting with LeetCode API.
/// </summary>
public interface ILeetCodeService
{
  /// <summary>
  /// Fetches a list of problems from LeetCode.
  /// </summary>
  Task<IReadOnlyList<LeetCodeProblem>> GetProblemsAsync(int limit = 50, int skip = 0, CancellationToken ct = default);

  /// <summary>
  /// Searches problems on LeetCode by query string.
  /// </summary>
  Task<IReadOnlyList<LeetCodeProblem>> SearchProblemsAsync(string query, int limit = 20, CancellationToken ct = default);

  /// <summary>
  /// Searches problems filtered by algorithmic tags only.
  /// </summary>
  Task<IReadOnlyList<LeetCodeProblem>> SearchAlgorithmicProblemsAsync(string query, int limit = 20, CancellationToken ct = default);

  /// <summary>
  /// Fetches a specific problem's details including description.
  /// </summary>
  Task<LeetCodeProblemDetail?> GetProblemDetailAsync(string titleSlug, CancellationToken ct = default);

  /// <summary>
  /// Syncs LeetCode problems to our database, mapping tags to patterns.
  /// </summary>
  Task<SyncResult> SyncProblemsAsync(int count = 50, CancellationToken ct = default);
}

/// <summary>
/// LeetCode problem summary from the list query.
/// </summary>
public record LeetCodeProblem(
    string QuestionId,
    string FrontendId,
    string Title,
    string TitleSlug,
    string Difficulty,
    double AcceptanceRate,
    IReadOnlyList<string> Tags
);

/// <summary>
/// LeetCode problem with full details.
/// </summary>
public record LeetCodeProblemDetail(
    string QuestionId,
    string FrontendId,
    string Title,
    string TitleSlug,
    string Content,
    string Difficulty,
    IReadOnlyList<string> Tags,
    IReadOnlyList<LeetCodeExample> Examples
);

/// <summary>
/// A code example from LeetCode.
/// </summary>
public record LeetCodeExample(
    string Input,
    string Output,
    string? Explanation
);

/// <summary>
/// Result of syncing problems from LeetCode.
/// </summary>
public record SyncResult(
    int TotalFetched,
    int NewlyCreated,
    int Skipped,
    int Failed,
    IReadOnlyList<string> Errors
);
