using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Services;

/// <summary>
/// Service for fetching problems from LeetCode GraphQL API.
/// </summary>
public partial class LeetCodeService : ILeetCodeService
{
  private const string GraphqlEndpoint = "https://leetcode.com/graphql";
  private readonly HttpClient _httpClient;
  private readonly IPatternRepository _patternRepository;
  private readonly IProblemRepository _problemRepository;
  private readonly ILogger<LeetCodeService> _logger;

  // Mapping of LeetCode tags to our PatternCategory enum
  private static readonly Dictionary<string, PatternCategory> TagToPatternMapping = new(StringComparer.OrdinalIgnoreCase)
  {
    ["Array"] = PatternCategory.Array,
    ["String"] = PatternCategory.String,
    ["Linked List"] = PatternCategory.LinkedList,
    ["Tree"] = PatternCategory.Tree,
    ["Binary Tree"] = PatternCategory.Tree,
    ["Binary Search Tree"] = PatternCategory.Tree,
    ["Graph"] = PatternCategory.Graph,
    ["Depth-First Search"] = PatternCategory.Graph,
    ["Breadth-First Search"] = PatternCategory.Graph,
    ["Dynamic Programming"] = PatternCategory.DynamicProgramming,
    ["Greedy"] = PatternCategory.Greedy,
    ["Binary Search"] = PatternCategory.BinarySearch,
    ["Two Pointers"] = PatternCategory.TwoPointers,
    ["Sliding Window"] = PatternCategory.SlidingWindow,
    ["Stack"] = PatternCategory.Stack,
    ["Monotonic Stack"] = PatternCategory.MonotonicStack,
    ["Queue"] = PatternCategory.Queue,
    ["Heap (Priority Queue)"] = PatternCategory.Heap,
    ["Heap"] = PatternCategory.Heap,
    ["Priority Queue"] = PatternCategory.Heap,
    ["Trie"] = PatternCategory.Trie,
    ["Backtracking"] = PatternCategory.Backtracking,
    ["Math"] = PatternCategory.Math,
    ["Bit Manipulation"] = PatternCategory.BitManipulation,
    ["Union Find"] = PatternCategory.UnionFind,
    ["Sorting"] = PatternCategory.Array,
    ["Hash Table"] = PatternCategory.Array,
    ["Matrix"] = PatternCategory.Array,
    ["Recursion"] = PatternCategory.Backtracking,
    ["Divide and Conquer"] = PatternCategory.BinarySearch,
    ["Simulation"] = PatternCategory.Array,
    ["Counting"] = PatternCategory.Array,
    ["Prefix Sum"] = PatternCategory.Array,
    ["Memoization"] = PatternCategory.DynamicProgramming,
    ["Interval"] = PatternCategory.Intervals,
    ["Line Sweep"] = PatternCategory.Intervals,
    ["Merge Sort"] = PatternCategory.Array,
    ["Quick Sort"] = PatternCategory.Array,
    ["Topological Sort"] = PatternCategory.Graph,
    ["Shortest Path"] = PatternCategory.Graph,
    ["Minimum Spanning Tree"] = PatternCategory.Graph
  };

  // Pattern-specific signals based on LeetCode tags
  private static readonly Dictionary<PatternCategory, string[]> PatternSignals = new()
  {
    [PatternCategory.TwoPointers] = ["sorted array", "opposite ends", "pairs", "sum to target", "palindrome"],
    [PatternCategory.SlidingWindow] = ["subarray", "substring", "contiguous", "window", "consecutive", "maximum/minimum of k elements"],
    [PatternCategory.BinarySearch] = ["sorted", "find position", "search", "rotated array", "minimize maximum", "maximize minimum"],
    [PatternCategory.DynamicProgramming] = ["optimal", "count ways", "minimum/maximum", "subsequence", "partition", "overlapping subproblems"],
    [PatternCategory.Graph] = ["nodes", "edges", "connected", "path", "cycle", "traversal"],
    [PatternCategory.Tree] = ["root", "parent", "child", "leaves", "depth", "height", "ancestor"],
    [PatternCategory.Stack] = ["next greater", "parentheses", "expression", "LIFO", "undo"],
    [PatternCategory.Heap] = ["kth largest", "kth smallest", "top k", "merge sorted", "median"],
    [PatternCategory.Backtracking] = ["generate all", "permutations", "combinations", "subsets", "choices"],
    [PatternCategory.Greedy] = ["local optimal", "sort first", "always pick", "interval scheduling"],
    [PatternCategory.UnionFind] = ["connected components", "groups", "disjoint sets", "union", "find"]
  };

  public LeetCodeService(
      HttpClient httpClient,
      IPatternRepository patternRepository,
      IProblemRepository problemRepository,
      ILogger<LeetCodeService> logger)
  {
    _httpClient = httpClient;
    _patternRepository = patternRepository;
    _problemRepository = problemRepository;
    _logger = logger;

    // Configure HTTP client for LeetCode
    _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "PatternBlindness/1.0");
    _httpClient.DefaultRequestHeaders.Add("Origin", "https://leetcode.com");
    _httpClient.DefaultRequestHeaders.Add("Referer", "https://leetcode.com/problemset/all/");
  }

  public async Task<IReadOnlyList<LeetCodeProblem>> GetProblemsAsync(int limit = 50, int skip = 0, CancellationToken ct = default)
  {
    var query = """
      query problemsetQuestionList($categorySlug: String, $limit: Int, $skip: Int, $filters: QuestionListFilterInput) {
        problemsetQuestionList: questionList(
          categorySlug: $categorySlug
          limit: $limit
          skip: $skip
          filters: $filters
        ) {
          total: totalNum
          questions: data {
            questionId
            questionFrontendId
            title
            titleSlug
            difficulty
            acRate
            topicTags {
              name
              slug
            }
          }
        }
      }
      """;

    var request = new GraphqlRequest(query, new
    {
      categorySlug = "algorithms",
      limit,
      skip,
      filters = new { }
    });

    try
    {
      var response = await _httpClient.PostAsJsonAsync(GraphqlEndpoint, request, ct);
      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<GraphqlResponse<ProblemListData>>(ct);

      if (result?.Data?.ProblemsetQuestionList?.Questions == null)
      {
        _logger.LogWarning("LeetCode API returned empty or null response");
        return [];
      }

      return result.Data.ProblemsetQuestionList.Questions
          .Select(q => new LeetCodeProblem(
              q.QuestionId,
              q.QuestionFrontendId,
              q.Title,
              q.TitleSlug,
              q.Difficulty,
              q.AcRate,
              q.TopicTags?.Select(t => t.Name).ToList() ?? []))
          .ToList();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch problems from LeetCode");
      return [];
    }
  }

  public async Task<LeetCodeProblemDetail?> GetProblemDetailAsync(string titleSlug, CancellationToken ct = default)
  {
    var query = """
      query questionData($titleSlug: String!) {
        question(titleSlug: $titleSlug) {
          questionId
          questionFrontendId
          title
          titleSlug
          content
          difficulty
          topicTags {
            name
            slug
          }
          exampleTestcaseList
          hints
        }
      }
      """;

    var request = new GraphqlRequest(query, new { titleSlug });

    try
    {
      var response = await _httpClient.PostAsJsonAsync(GraphqlEndpoint, request, ct);
      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<GraphqlResponse<QuestionData>>(ct);

      if (result?.Data?.Question == null)
      {
        _logger.LogWarning("LeetCode API returned null for problem: {TitleSlug}", titleSlug);
        return null;
      }

      var q = result.Data.Question;
      var examples = ParseExamplesFromContent(q.Content);

      return new LeetCodeProblemDetail(
          q.QuestionId,
          q.QuestionFrontendId,
          q.Title,
          q.TitleSlug,
          StripHtml(q.Content ?? ""),
          q.Difficulty,
          q.TopicTags?.Select(t => t.Name).ToList() ?? [],
          examples);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to fetch problem detail from LeetCode: {TitleSlug}", titleSlug);
      return null;
    }
  }

  public async Task<IReadOnlyList<LeetCodeProblem>> SearchProblemsAsync(string query, int limit = 20, CancellationToken ct = default)
  {
    var graphqlQuery = """
      query problemsetQuestionList($categorySlug: String, $limit: Int, $skip: Int, $filters: QuestionListFilterInput) {
        problemsetQuestionList: questionList(
          categorySlug: $categorySlug
          limit: $limit
          skip: $skip
          filters: $filters
        ) {
          total: totalNum
          questions: data {
            questionId
            questionFrontendId
            title
            titleSlug
            difficulty
            acRate
            topicTags {
              name
              slug
            }
          }
        }
      }
      """;

    var request = new GraphqlRequest(graphqlQuery, new
    {
      categorySlug = "algorithms",
      limit,
      skip = 0,
      filters = new { searchKeywords = query }
    });

    try
    {
      var response = await _httpClient.PostAsJsonAsync(GraphqlEndpoint, request, ct);
      response.EnsureSuccessStatusCode();

      var result = await response.Content.ReadFromJsonAsync<GraphqlResponse<ProblemListData>>(ct);

      if (result?.Data?.ProblemsetQuestionList?.Questions == null)
      {
        _logger.LogWarning("LeetCode search API returned empty response for query: {Query}", query);
        return [];
      }

      return result.Data.ProblemsetQuestionList.Questions
          .Select(q => new LeetCodeProblem(
              q.QuestionId,
              q.QuestionFrontendId,
              q.Title,
              q.TitleSlug,
              q.Difficulty,
              q.AcRate,
              q.TopicTags?.Select(t => t.Name).ToList() ?? []))
          .ToList();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to search problems from LeetCode: {Query}", query);
      return [];
    }
  }

  // Tags to exclude (non-algorithmic problems)
  private static readonly HashSet<string> ExcludedTags = new(StringComparer.OrdinalIgnoreCase)
  {
    "Database", "Shell", "Concurrency", "JavaScript", "Pandas"
  };

  public async Task<IReadOnlyList<LeetCodeProblem>> SearchAlgorithmicProblemsAsync(string query, int limit = 20, CancellationToken ct = default)
  {
    // Fetch more than needed to filter
    var problems = await SearchProblemsAsync(query, limit * 2, ct);

    // Filter out non-algorithmic problems
    var filtered = problems
        .Where(p => !p.Tags.Any(tag => ExcludedTags.Contains(tag)))
        .Take(limit)
        .ToList();

    return filtered;
  }

  public async Task<SyncResult> SyncProblemsAsync(int count = 50, CancellationToken ct = default)
  {
    var errors = new List<string>();
    int created = 0, skipped = 0, failed = 0;

    // Fetch problems from LeetCode
    var problems = await GetProblemsAsync(count, 0, ct);

    if (problems.Count == 0)
    {
      return new SyncResult(0, 0, 0, 1, ["Failed to fetch problems from LeetCode API"]);
    }

    // Ensure we have patterns in the database
    var patterns = await _patternRepository.GetAllAsync(ct);
    // Use GroupBy to handle multiple patterns with same category (e.g., BFS and DFS both in Graph)
    var patternByCategory = patterns
        .GroupBy(p => p.Category)
        .ToDictionary(g => g.Key, g => g.First());

    // Create missing patterns
    foreach (var category in Enum.GetValues<PatternCategory>())
    {
      if (!patternByCategory.ContainsKey(category))
      {
        var pattern = Pattern.Create(
            category.ToString().SplitCamelCase(),
            $"Problems using {category.ToString().SplitCamelCase().ToLower()} technique",
            category,
            JsonSerializer.Serialize(PatternSignals.GetValueOrDefault(category, [])));

        var createdPattern = await _patternRepository.AddAsync(pattern, ct);
        patternByCategory[category] = createdPattern;
        _logger.LogInformation("Created pattern: {PatternName}", pattern.Name);
      }
    }

    // Process each problem
    foreach (var lcProblem in problems)
    {
      try
      {
        // Identify pattern from tags
        var patternCategory = IdentifyPatternFromTags(lcProblem.Tags);

        if (!patternByCategory.TryGetValue(patternCategory, out var pattern))
        {
          _logger.LogWarning("No pattern found for category {Category}", patternCategory);
          skipped++;
          continue;
        }

        // Fetch detailed description
        var detail = await GetProblemDetailAsync(lcProblem.TitleSlug, ct);

        if (detail == null)
        {
          _logger.LogWarning("Could not fetch details for {Title}", lcProblem.Title);
          failed++;
          errors.Add($"Failed to fetch details for {lcProblem.Title}");
          continue;
        }

        // Map difficulty
        var difficulty = lcProblem.Difficulty switch
        {
          "Easy" => Difficulty.Easy,
          "Medium" => Difficulty.Medium,
          "Hard" => Difficulty.Hard,
          _ => Difficulty.Medium
        };

        // Generate signals based on pattern and tags
        var signals = GenerateSignals(patternCategory, lcProblem.Tags, detail.Content);

        // Create problem entity
        var problem = Problem.Create(
            $"#{lcProblem.FrontendId}. {lcProblem.Title}",
            detail.Content,
            difficulty,
            pattern.Id,
            GenerateKeyInvariant(patternCategory),
            $"This problem uses {pattern.Name} pattern. Look for: {string.Join(", ", signals.Take(3))}",
            JsonSerializer.Serialize(signals),
            JsonSerializer.Serialize(ExtractConstraints(detail.Content)),
            JsonSerializer.Serialize(detail.Examples.Select(e => new { e.Input, e.Output, e.Explanation })));

        // Add wrong approaches based on common mistakes for this pattern
        AddWrongApproaches(problem, patternCategory, patternByCategory);

        await _problemRepository.AddAsync(problem, ct);
        created++;
        _logger.LogInformation("Created problem: {Title}", problem.Title);

        // Small delay to avoid rate limiting
        await Task.Delay(200, ct);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to sync problem {Title}", lcProblem.Title);
        failed++;
        errors.Add($"Failed to sync {lcProblem.Title}: {ex.Message}");
      }
    }

    return new SyncResult(problems.Count, created, skipped, failed, errors);
  }

  private PatternCategory IdentifyPatternFromTags(IReadOnlyList<string> tags)
  {
    // Priority order for pattern identification
    var priorityTags = new[]
    {
      "Two Pointers", "Sliding Window", "Binary Search", "Dynamic Programming",
      "Backtracking", "Trie", "Union Find", "Heap (Priority Queue)", "Monotonic Stack",
      "Graph", "Depth-First Search", "Breadth-First Search", "Tree", "Stack", "Queue",
      "Greedy", "Bit Manipulation", "Math", "Linked List", "Array", "String"
    };

    foreach (var priorityTag in priorityTags)
    {
      if (tags.Any(t => t.Equals(priorityTag, StringComparison.OrdinalIgnoreCase)))
      {
        if (TagToPatternMapping.TryGetValue(priorityTag, out var category))
        {
          return category;
        }
      }
    }

    // Check for any mapping
    foreach (var tag in tags)
    {
      if (TagToPatternMapping.TryGetValue(tag, out var category))
      {
        return category;
      }
    }

    // Default to Array if no match
    return PatternCategory.Array;
  }

  private List<string> GenerateSignals(PatternCategory category, IReadOnlyList<string> tags, string description)
  {
    var signals = new List<string>();

    // Add pattern-specific signals
    if (PatternSignals.TryGetValue(category, out var patternSpecificSignals))
    {
      signals.AddRange(patternSpecificSignals);
    }

    // Add tag-based signals
    foreach (var tag in tags.Take(5))
    {
      signals.Add($"Tagged as '{tag}'");
    }

    // Extract signals from description
    var descriptionLower = description.ToLowerInvariant();

    if (descriptionLower.Contains("sorted"))
      signals.Add("Input is sorted");
    if (descriptionLower.Contains("subarray"))
      signals.Add("Looking for subarray");
    if (descriptionLower.Contains("substring"))
      signals.Add("Looking for substring");
    if (descriptionLower.Contains("minimum") || descriptionLower.Contains("maximum"))
      signals.Add("Optimization problem");
    if (descriptionLower.Contains("count"))
      signals.Add("Counting problem");
    if (descriptionLower.Contains("path"))
      signals.Add("Path-finding involved");
    if (descriptionLower.Contains("connected"))
      signals.Add("Connectivity problem");

    return signals.Distinct().Take(10).ToList();
  }

  private string GenerateKeyInvariant(PatternCategory category)
  {
    return category switch
    {
      PatternCategory.TwoPointers => "Maintain two pointers moving towards each other or in the same direction to reduce search space",
      PatternCategory.SlidingWindow => "Maintain a window that expands or contracts to satisfy constraints while tracking optimal answer",
      PatternCategory.BinarySearch => "Eliminate half the search space with each comparison by leveraging sorted or monotonic property",
      PatternCategory.DynamicProgramming => "Break into overlapping subproblems, cache results to avoid recomputation",
      PatternCategory.Graph => "Model as vertices and edges, traverse using BFS/DFS to explore all states",
      PatternCategory.Tree => "Leverage hierarchical structure with recursive traversal or iterative level-order processing",
      PatternCategory.Stack => "Use LIFO order to track nested or sequential relationships",
      PatternCategory.Heap => "Maintain partial ordering to efficiently access min/max elements",
      PatternCategory.Backtracking => "Explore all possibilities by making choices and undoing them when constraints are violated",
      PatternCategory.Greedy => "Make locally optimal choices that lead to globally optimal solution",
      PatternCategory.UnionFind => "Track connected components efficiently with path compression and union by rank",
      PatternCategory.Trie => "Use prefix tree to efficiently store and query strings",
      _ => "Identify the core data structure and algorithm pattern to apply"
    };
  }

  private void AddWrongApproaches(Problem problem, PatternCategory correctCategory, Dictionary<PatternCategory, Pattern> patterns)
  {
    // Common wrong approaches for each pattern
    var wrongApproachMap = new Dictionary<PatternCategory, (PatternCategory wrong, string explanation, int frequency)[]>
    {
      [PatternCategory.TwoPointers] = [
        (PatternCategory.Array, "Using nested loops leads to O(n²) time complexity when O(n) is possible", 40),
        (PatternCategory.BinarySearch, "Binary search works but two pointers is more direct for this structure", 20)
      ],
      [PatternCategory.SlidingWindow] = [
        (PatternCategory.TwoPointers, "Two pointers without window state tracking misses the constraint handling", 35),
        (PatternCategory.DynamicProgramming, "DP is overkill - sliding window achieves O(n) with less memory", 25)
      ],
      [PatternCategory.BinarySearch] = [
        (PatternCategory.Array, "Linear search works but misses the O(log n) optimization opportunity", 45),
        (PatternCategory.TwoPointers, "Two pointers doesn't leverage the sorted property efficiently", 20)
      ],
      [PatternCategory.DynamicProgramming] = [
        (PatternCategory.Backtracking, "Backtracking without memoization leads to exponential time", 40),
        (PatternCategory.Greedy, "Greedy doesn't account for overlapping subproblem dependencies", 30)
      ],
      [PatternCategory.Graph] = [
        (PatternCategory.Tree, "Treating graph as tree ignores cycles and multiple paths", 25),
        (PatternCategory.Array, "Flattening graph loses structural relationships", 20)
      ],
      [PatternCategory.Backtracking] = [
        (PatternCategory.DynamicProgramming, "DP requires optimal substructure which generation problems lack", 30),
        (PatternCategory.Greedy, "Greedy can't generate all valid combinations", 25)
      ]
    };

    if (wrongApproachMap.TryGetValue(correctCategory, out var wrongApproaches))
    {
      foreach (var (wrongCategory, explanation, frequency) in wrongApproaches)
      {
        if (patterns.TryGetValue(wrongCategory, out var wrongPattern))
        {
          problem.AddWrongApproach(wrongPattern.Id, explanation, frequency);
        }
      }
    }
  }

  private List<string> ExtractConstraints(string content)
  {
    var constraints = new List<string>();

    // Look for constraint patterns in HTML content
    var constraintMatches = ConstraintRegex().Matches(content);
    foreach (Match match in constraintMatches)
    {
      var constraint = StripHtml(match.Groups[1].Value).Trim();
      if (!string.IsNullOrEmpty(constraint))
      {
        constraints.Add(constraint);
      }
    }

    return constraints.Take(5).ToList();
  }

  private List<LeetCodeExample> ParseExamplesFromContent(string? content)
  {
    if (string.IsNullOrEmpty(content))
      return [];

    var examples = new List<LeetCodeExample>();

    // Extract examples from HTML - LeetCode uses specific patterns
    var exampleMatches = ExampleRegex().Matches(content);

    string? currentInput = null;
    string? currentOutput = null;
    string? currentExplanation = null;

    foreach (Match match in exampleMatches)
    {
      var text = StripHtml(match.Value);

      if (text.StartsWith("Input:", StringComparison.OrdinalIgnoreCase))
      {
        if (currentInput != null && currentOutput != null)
        {
          examples.Add(new LeetCodeExample(currentInput, currentOutput, currentExplanation));
        }
        currentInput = text["Input:".Length..].Trim();
        currentOutput = null;
        currentExplanation = null;
      }
      else if (text.StartsWith("Output:", StringComparison.OrdinalIgnoreCase))
      {
        currentOutput = text["Output:".Length..].Trim();
      }
      else if (text.StartsWith("Explanation:", StringComparison.OrdinalIgnoreCase))
      {
        currentExplanation = text["Explanation:".Length..].Trim();
      }
    }

    if (currentInput != null && currentOutput != null)
    {
      examples.Add(new LeetCodeExample(currentInput, currentOutput, currentExplanation));
    }

    return examples;
  }

  private static string StripHtml(string html)
  {
    if (string.IsNullOrEmpty(html))
      return "";

    // Remove HTML tags
    var text = HtmlTagRegex().Replace(html, " ");
    // Decode HTML entities
    text = System.Net.WebUtility.HtmlDecode(text);
    // Clean up whitespace
    text = WhitespaceRegex().Replace(text, " ");
    return text.Trim();
  }

  [GeneratedRegex(@"<li>([^<]+)</li>", RegexOptions.IgnoreCase)]
  private static partial Regex ConstraintRegex();

  [GeneratedRegex(@"<strong[^>]*>([^<]*(?:Input|Output|Explanation)[^<]*)</strong>[^<]*([^<]+)", RegexOptions.IgnoreCase)]
  private static partial Regex ExampleRegex();

  [GeneratedRegex(@"<[^>]+>")]
  private static partial Regex HtmlTagRegex();

  [GeneratedRegex(@"\s+")]
  private static partial Regex WhitespaceRegex();

  // GraphQL request/response DTOs
  private record GraphqlRequest(
      [property: JsonPropertyName("query")] string Query,
      [property: JsonPropertyName("variables")] object Variables
  );

  private record GraphqlResponse<T>(
      [property: JsonPropertyName("data")] T? Data
  );

  private record ProblemListData(
      [property: JsonPropertyName("problemsetQuestionList")] QuestionList? ProblemsetQuestionList
  );

  private record QuestionList(
      [property: JsonPropertyName("total")] int Total,
      [property: JsonPropertyName("questions")] List<QuestionSummary>? Questions
  );

  private record QuestionSummary(
      [property: JsonPropertyName("questionId")] string QuestionId,
      [property: JsonPropertyName("questionFrontendId")] string QuestionFrontendId,
      [property: JsonPropertyName("title")] string Title,
      [property: JsonPropertyName("titleSlug")] string TitleSlug,
      [property: JsonPropertyName("difficulty")] string Difficulty,
      [property: JsonPropertyName("acRate")] double AcRate,
      [property: JsonPropertyName("topicTags")] List<TopicTag>? TopicTags
  );

  private record QuestionData(
      [property: JsonPropertyName("question")] QuestionDetail? Question
  );

  private record QuestionDetail(
      [property: JsonPropertyName("questionId")] string QuestionId,
      [property: JsonPropertyName("questionFrontendId")] string QuestionFrontendId,
      [property: JsonPropertyName("title")] string Title,
      [property: JsonPropertyName("titleSlug")] string TitleSlug,
      [property: JsonPropertyName("content")] string? Content,
      [property: JsonPropertyName("difficulty")] string Difficulty,
      [property: JsonPropertyName("topicTags")] List<TopicTag>? TopicTags,
      [property: JsonPropertyName("exampleTestcaseList")] List<string>? ExampleTestcaseList,
      [property: JsonPropertyName("hints")] List<string>? Hints
  );

  private record TopicTag(
      [property: JsonPropertyName("name")] string Name,
      [property: JsonPropertyName("slug")] string Slug
  );
}

/// <summary>
/// String extension for camel case splitting.
/// </summary>
public static class StringExtensions
{
  public static string SplitCamelCase(this string str)
  {
    return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
  }
}
