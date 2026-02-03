using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Api.Endpoints;

// JSON serializer options for camelCase (to match frontend expectations)
internal static class JsonOptions
{
  public static readonly JsonSerializerOptions CamelCase = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };
}

/// <summary>
/// Endpoints for LeetCode integration.
/// </summary>
public static class LeetCodeEndpoints
{
  public static void MapLeetCodeEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/leetcode")
        .WithTags("LeetCode");

    // Search endpoint - primary way to find problems
    group.MapGet("/search", SearchProblems)
        .WithName("SearchLeetCodeProblems")
        .WithDescription("Search LeetCode problems by query (filtered to algorithmic problems)")
        .Produces<IReadOnlyList<LeetCodeProblem>>(StatusCodes.Status200OK);

    // Get problems list (for browsing)
    group.MapGet("/problems", GetProblems)
        .WithName("GetLeetCodeProblems")
        .WithDescription("Fetch problems from LeetCode API")
        .Produces<IReadOnlyList<LeetCodeProblem>>(StatusCodes.Status200OK);

    // Get problem details and cache them
    group.MapGet("/problems/{titleSlug}", GetProblemDetail)
        .WithName("GetLeetCodeProblemDetail")
        .WithDescription("Fetch a specific problem's details from LeetCode (caches locally)")
        .Produces<CachedProblemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    // Analyze a problem (triggers LLM analysis if not cached)
    group.MapPost("/problems/{titleSlug}/analyze", AnalyzeProblem)
        .WithName("AnalyzeLeetCodeProblem")
        .WithDescription("Analyze a problem using LLM (caches the analysis)")
        .Produces<ProblemAnalysisResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    // Start an attempt on a LeetCode problem
    group.MapPost("/problems/{titleSlug}/start", StartAttempt)
        .RequireAuthorization()
        .WithName("StartLeetCodeAttempt")
        .WithDescription("Start a practice attempt on a LeetCode problem")
        .Produces<AttemptStartResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    // Admin-only endpoint for syncing (legacy)
    group.MapPost("/sync", SyncProblems)
        .RequireAuthorization("AdminOnly")
        .WithName("SyncLeetCodeProblems")
        .WithDescription("Sync problems from LeetCode to the database (legacy)")
        .Produces<SyncResult>(StatusCodes.Status200OK);
  }

  private static async Task<Ok<IReadOnlyList<LeetCodeProblem>>> SearchProblems(
      ILeetCodeService leetCodeService,
      string query,
      int limit = 20,
      CancellationToken ct = default)
  {
    var problems = await leetCodeService.SearchAlgorithmicProblemsAsync(query, limit, ct);
    return TypedResults.Ok(problems);
  }

  private static async Task<Ok<IReadOnlyList<LeetCodeProblem>>> GetProblems(
      ILeetCodeService leetCodeService,
      int limit = 50,
      int skip = 0,
      CancellationToken ct = default)
  {
    var problems = await leetCodeService.GetProblemsAsync(limit, skip, ct);
    return TypedResults.Ok(problems);
  }

  private static async Task<Results<Ok<CachedProblemResponse>, NotFound>> GetProblemDetail(
      string titleSlug,
      ILeetCodeService leetCodeService,
      ILeetCodeProblemCacheRepository cacheRepository,
      CancellationToken ct = default)
  {
    // Check cache first
    var cached = await cacheRepository.GetWithAnalysisAsync(titleSlug, ct);

    if (cached is not null)
    {
      return TypedResults.Ok(MapToCachedResponse(cached));
    }

    // Fetch from LeetCode
    var problem = await leetCodeService.GetProblemDetailAsync(titleSlug, ct);

    if (problem is null)
      return TypedResults.NotFound();

    // Cache it
    var cacheEntry = LeetCodeProblemCache.Create(
        problem.QuestionId,
        problem.FrontendId,
        problem.Title,
        problem.TitleSlug,
        problem.Difficulty,
        problem.Content,
        JsonSerializer.Serialize(problem.Tags),
        JsonSerializer.Serialize(problem.Examples),
        "[]", // hints
        0 // acceptance rate
    );

    await cacheRepository.AddAsync(cacheEntry, ct);

    return TypedResults.Ok(MapToCachedResponse(cacheEntry));
  }

  private static async Task<Results<Ok<ProblemAnalysisResponse>, NotFound, StatusCodeHttpResult>> AnalyzeProblem(
      string titleSlug,
      ILeetCodeService leetCodeService,
      ILeetCodeProblemCacheRepository cacheRepository,
      IAnalysisRepository analysisRepository,
      ILlmService llmService,
      CancellationToken ct = default)
  {
    // Get or fetch the problem
    var cached = await cacheRepository.GetWithAnalysisAsync(titleSlug, ct);

    if (cached is null)
    {
      // Fetch from LeetCode first
      var problem = await leetCodeService.GetProblemDetailAsync(titleSlug, ct);

      if (problem is null)
        return TypedResults.NotFound();

      // Cache it
      cached = LeetCodeProblemCache.Create(
          problem.QuestionId,
          problem.FrontendId,
          problem.Title,
          problem.TitleSlug,
          problem.Difficulty,
          problem.Content,
          JsonSerializer.Serialize(problem.Tags),
          JsonSerializer.Serialize(problem.Examples),
          "[]",
          0
      );

      await cacheRepository.AddAsync(cached, ct);
    }

    // Check if already analyzed
    if (cached.Analysis is not null)
    {
      return TypedResults.Ok(MapToAnalysisResponse(cached.Analysis));
    }

    // Analyze with LLM
    var tags = JsonSerializer.Deserialize<List<string>>(cached.Tags) ?? [];

    ProblemAnalysisResult analysisResult;
    try
    {
      analysisResult = await llmService.AnalyzeProblemAsync(
          cached.Title,
          cached.Content,
          tags,
          cached.Difficulty,
          ct
      );
    }
    catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("quota") || ex.Message.Contains("insufficient"))
    {
      // Return a 503 Service Unavailable with a descriptive message
      return TypedResults.StatusCode(503);
    }

    // Save analysis - use camelCase for KeySignals and CommonMistakes to match frontend expectations
    var analysis = ProblemAnalysis.Create(
        cached.Id,
        JsonSerializer.Serialize(analysisResult.PrimaryPatterns),
        JsonSerializer.Serialize(analysisResult.SecondaryPatterns),
        JsonSerializer.Serialize(analysisResult.KeySignals, JsonOptions.CamelCase),
        JsonSerializer.Serialize(analysisResult.CommonMistakes, JsonOptions.CamelCase),
        analysisResult.TimeComplexity,
        analysisResult.SpaceComplexity,
        analysisResult.KeyInsight,
        analysisResult.ApproachExplanation,
        JsonSerializer.Serialize(analysisResult.SimilarProblems),
        "gpt-4o-mini",
        analysisResult.RawResponse
    );

    await analysisRepository.AddAnalysisAsync(analysis, ct);

    return TypedResults.Ok(MapToAnalysisResponse(analysis));
  }

  private static async Task<Results<Ok<AttemptStartResponse>, NotFound, UnauthorizedHttpResult, Conflict<Microsoft.AspNetCore.Mvc.ProblemDetails>>> StartAttempt(
      string titleSlug,
      HttpContext httpContext,
      ILeetCodeService leetCodeService,
      ILeetCodeProblemCacheRepository cacheRepository,
      IAttemptRepository attemptRepository,
      CancellationToken ct = default)
  {
    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userId))
      return TypedResults.Unauthorized();

    // LOOP ENFORCEMENT: Check for active attempt
    var activeAttempt = await attemptRepository.GetActiveAttemptByUserIdAsync(userId, ct);
    if (activeAttempt is not null)
    {
      var problemTitle = activeAttempt.Problem?.Title ?? activeAttempt.LeetCodeProblem?.Title ?? "Unknown";
      return TypedResults.Conflict(new Microsoft.AspNetCore.Mvc.ProblemDetails
      {
        Title = "Active Attempt Exists",
        Detail = "You must complete or abandon your current attempt before starting a new one.",
        Extensions = new Dictionary<string, object?>
        {
          ["activeAttemptId"] = activeAttempt.Id,
          ["problemTitle"] = problemTitle,
          ["startedAt"] = activeAttempt.StartedAt
        }
      });
    }

    // Get or fetch the problem
    var cached = await cacheRepository.GetBySlugAsync(titleSlug, ct);

    if (cached is null)
    {
      // Fetch from LeetCode first
      var problem = await leetCodeService.GetProblemDetailAsync(titleSlug, ct);

      if (problem is null)
        return TypedResults.NotFound();

      // Cache it
      cached = LeetCodeProblemCache.Create(
          problem.QuestionId,
          problem.FrontendId,
          problem.Title,
          problem.TitleSlug,
          problem.Difficulty,
          problem.Content,
          JsonSerializer.Serialize(problem.Tags),
          JsonSerializer.Serialize(problem.Examples),
          "[]",
          0
      );

      await cacheRepository.AddAsync(cached, ct);
    }

    // Create attempt
    var attempt = Attempt.CreateForLeetCode(userId, cached.Id);
    await attemptRepository.AddAsync(attempt, ct);

    return TypedResults.Ok(new AttemptStartResponse(
        attempt.Id,
        cached.Id,
        cached.Title,
        cached.TitleSlug,
        cached.Content,
        cached.Difficulty,
        JsonSerializer.Deserialize<List<string>>(cached.Tags) ?? []
    ));
  }

  private static async Task<Ok<SyncResult>> SyncProblems(
      ILeetCodeService leetCodeService,
      int count = 50,
      CancellationToken ct = default)
  {
    var result = await leetCodeService.SyncProblemsAsync(count, ct);
    return TypedResults.Ok(result);
  }

  private static CachedProblemResponse MapToCachedResponse(LeetCodeProblemCache cached)
  {
    return new CachedProblemResponse(
        cached.Id,
        cached.LeetCodeId,
        cached.FrontendId,
        cached.Title,
        cached.TitleSlug,
        cached.Difficulty,
        cached.Content,
        JsonSerializer.Deserialize<List<string>>(cached.Tags) ?? [],
        cached.Analysis is not null,
        cached.CachedAt
    );
  }

  private static ProblemAnalysisResponse MapToAnalysisResponse(ProblemAnalysis analysis)
  {
    return new ProblemAnalysisResponse(
        analysis.Id,
        JsonSerializer.Deserialize<List<string>>(analysis.PrimaryPatterns) ?? [],
        JsonSerializer.Deserialize<List<string>>(analysis.SecondaryPatterns) ?? [],
        analysis.KeySignals,
        analysis.CommonMistakes,
        analysis.TimeComplexity,
        analysis.SpaceComplexity,
        analysis.KeyInsight,
        analysis.ApproachExplanation,
        JsonSerializer.Deserialize<List<string>>(analysis.SimilarProblems) ?? [],
        analysis.AnalyzedAt
    );
  }
}

// Response DTOs
public record CachedProblemResponse(
    Guid Id,
    string LeetCodeId,
    string FrontendId,
    string Title,
    string TitleSlug,
    string Difficulty,
    string Content,
    IReadOnlyList<string> Tags,
    bool HasAnalysis,
    DateTime CachedAt
);

public record ProblemAnalysisResponse(
    Guid Id,
    IReadOnlyList<string> PrimaryPatterns,
    IReadOnlyList<string> SecondaryPatterns,
    string KeySignals,
    string CommonMistakes,
    string TimeComplexity,
    string SpaceComplexity,
    string KeyInsight,
    string ApproachExplanation,
    IReadOnlyList<string> SimilarProblems,
    DateTime AnalyzedAt
);

public record AttemptStartResponse(
    Guid AttemptId,
    Guid ProblemCacheId,
    string Title,
    string TitleSlug,
    string Content,
    string Difficulty,
    IReadOnlyList<string> Tags
);
