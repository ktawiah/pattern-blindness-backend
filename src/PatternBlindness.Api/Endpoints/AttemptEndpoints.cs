using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PatternBlindness.Application.DTOs.Requests;
using PatternBlindness.Application.DTOs.Responses;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for managing problem attempts - the core MVP functionality.
/// </summary>
public static class AttemptEndpoints
{
  public static void MapAttemptEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/attempts")
        .WithTags("Attempts")
        .RequireAuthorization();

    group.MapPost("/", StartAttempt)
        .WithName("StartAttempt")
        .WithDescription("Start a new problem attempt")
        .Produces<AttemptResponse>(StatusCodes.Status201Created)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/{id:guid}/cold-start", SubmitColdStart)
        .WithName("SubmitColdStart")
        .WithDescription("Submit the cold start thinking phase with adaptive timer and multiple hypothesis support")
        .Produces<AttemptResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/{id:guid}/complete", CompleteAttempt)
        .WithName("CompleteAttempt")
        .WithDescription("Mark an attempt as completed and get solution reveal")
        .Produces<ProblemWithSolutionResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/{id:guid}/give-up", GiveUpAttempt)
        .WithName("GiveUpAttempt")
        .WithDescription("Give up on the current attempt")
        .Produces<AttemptResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/{id:guid}", GetAttempt)
        .WithName("GetAttempt")
        .WithDescription("Get attempt details by ID")
        .Produces<AttemptResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/{id:guid}/reveal", GetWrongApproachReveal)
        .WithName("GetWrongApproachReveal")
        .WithDescription("Get the wrong-but-reasonable reveal after completing")
        .Produces<WrongApproachRevealResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/", GetUserAttempts)
        .WithName("GetUserAttempts")
        .WithDescription("Get all attempts for the current user")
        .Produces<IReadOnlyList<AttemptResponse>>(StatusCodes.Status200OK);

    group.MapGet("/dashboard", GetConfidenceDashboard)
        .WithName("GetConfidenceDashboard")
        .WithDescription("Get confidence vs correctness dashboard for the current user")
        .Produces<ConfidenceDashboardResponse>(StatusCodes.Status200OK);

    group.MapGet("/cold-start-settings", GetColdStartSettings)
        .WithName("GetColdStartSettings")
        .WithDescription("Get adaptive cold start settings based on user's performance")
        .Produces<ColdStartSettingsResponse>(StatusCodes.Status200OK);

    // New endpoint for LeetCode-based reflection generation
    group.MapPost("/{id:guid}/reflection", GenerateReflection)
        .WithName("GenerateReflection")
        .WithDescription("Generate a personalized reflection for a completed LeetCode attempt")
        .Produces<ReflectionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/{id:guid}/reflection", GetReflection)
        .WithName("GetReflection")
        .WithDescription("Get the reflection for a completed attempt")
        .Produces<ReflectionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
  }

  private static async Task<Results<Created<AttemptResponse>, NotFound, BadRequest<ProblemDetails>>> StartAttempt(
      StartAttemptRequest request,
      IAttemptRepository attemptRepository,
      IProblemRepository problemRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "User not authenticated" });

    var problem = await problemRepository.GetByIdAsync(request.ProblemId, ct);
    if (problem is null)
      return TypedResults.NotFound();

    var attempt = Attempt.Create(userId, request.ProblemId);
    await attemptRepository.AddAsync(attempt, ct);

    var response = MapToResponse(attempt, problem.Title);
    return TypedResults.Created($"/api/attempts/{attempt.Id}", response);
  }

  private static async Task<Results<Ok<AttemptResponse>, NotFound, BadRequest<ProblemDetails>>> SubmitColdStart(
      Guid id,
      SubmitColdStartRequest request,
      IAttemptRepository attemptRepository,
      IPatternRepository patternRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    // Validate chosen pattern exists
    if (!await patternRepository.ExistsAsync(request.ChosenPatternId, ct))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Chosen pattern does not exist" });

    // Validate secondary pattern if provided
    if (request.SecondaryPatternId.HasValue && !await patternRepository.ExistsAsync(request.SecondaryPatternId.Value, ct))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Secondary pattern does not exist" });

    // Validate rejected pattern if provided
    if (request.RejectedPatternId.HasValue && !await patternRepository.ExistsAsync(request.RejectedPatternId.Value, ct))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Rejected pattern does not exist" });

    // Calculate adaptive minimum duration based on user's recent performance
    var minimumDuration = await CalculateAdaptiveMinimumAsync(userId!, attemptRepository, ct);

    var result = attempt.SubmitColdStart(
        request.IdentifiedSignals,
        request.ChosenPatternId,
        request.SecondaryPatternId,
        request.PrimaryVsSecondaryReason,
        request.RejectedPatternId,
        request.RejectionReason,
        request.Confidence,
        request.ThinkingDurationSeconds,
        minimumDuration);

    if (result.IsFailure)
      return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

    await attemptRepository.UpdateAsync(attempt, ct);

    var updatedAttempt = await attemptRepository.GetByIdWithColdStartAsync(id, ct);
    var response = MapToResponseWithColdStart(updatedAttempt!);
    return TypedResults.Ok(response);
  }

  /// <summary>
  /// Calculates adaptive minimum cold start duration based on user's recent performance.
  /// - 30s: Default for new users or users with good accuracy (>70%)
  /// - 90s: For users with moderate accuracy (50-70%)
  /// - 180s: For users struggling with accuracy (<50%)
  /// </summary>
  private static async Task<int> CalculateAdaptiveMinimumAsync(
      string userId,
      IAttemptRepository attemptRepository,
      CancellationToken ct)
  {
    const int MinDurationNewUser = 30;
    const int MinDurationModerate = 90;
    const int MinDurationStruggling = 180;

    var recentAttempts = await attemptRepository.GetRecentByUserIdAsync(userId, 10, ct);

    if (recentAttempts.Count < 5)
      return MinDurationNewUser;

    var completedAttempts = recentAttempts
        .Where(a => a.Status == Domain.Enums.AttemptStatus.Solved)
        .ToList();

    if (completedAttempts.Count == 0)
      return MinDurationModerate;

    var accuracy = (double)completedAttempts.Count(a => a.IsPatternCorrect) / completedAttempts.Count;

    return accuracy switch
    {
      >= 0.70 => MinDurationNewUser,
      >= 0.50 => MinDurationModerate,
      _ => MinDurationStruggling
    };
  }

  private static async Task<Results<Ok<ProblemWithSolutionResponse>, NotFound, BadRequest<ProblemDetails>>> CompleteAttempt(
      Guid id,
      CompleteAttemptRequest request,
      IAttemptRepository attemptRepository,
      IProblemRepository problemRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    var result = attempt.Complete(request.IsPatternCorrect, request.Confidence);

    if (result.IsFailure)
      return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

    await attemptRepository.UpdateAsync(attempt, ct);

    // Get problem with solution details (legacy flow only)
    if (!attempt.ProblemId.HasValue)
      return TypedResults.BadRequest(new ProblemDetails { Detail = "This endpoint is for legacy problem attempts only" });

    var problem = await problemRepository.GetByIdWithWrongApproachesAsync(attempt.ProblemId.Value, ct);
    if (problem is null)
      return TypedResults.NotFound();

    var response = new ProblemWithSolutionResponse(
        problem.Id,
        problem.Title,
        problem.Description,
        problem.Difficulty,
        ParseJsonArray(problem.Signals),
        ParseJsonArray(problem.Constraints),
        ParseJsonArray(problem.Examples),
        problem.CorrectPatternId,
        problem.CorrectPattern?.Name ?? "",
        problem.KeyInvariant,
        problem.SolutionExplanation,
        problem.WrongApproaches
            .OrderByDescending(w => w.FrequencyPercent)
            .Select(w => new WrongApproachDto(
                w.WrongPatternId,
                w.WrongPattern?.Name ?? "",
                w.Explanation,
                w.FrequencyPercent))
            .ToList());

    return TypedResults.Ok(response);
  }

  private static string[] ParseJsonArray(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<string[]>(json) ?? [];
    }
    catch
    {
      return [];
    }
  }

  private static async Task<Results<Ok<AttemptResponse>, NotFound>> GiveUpAttempt(
      Guid id,
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    attempt.GiveUp();
    await attemptRepository.UpdateAsync(attempt, ct);

    var response = MapToResponse(attempt, attempt.Problem?.Title ?? "");
    return TypedResults.Ok(response);
  }

  private static async Task<Results<Ok<AttemptResponse>, NotFound>> GetAttempt(
      Guid id,
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdWithColdStartAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    var response = MapToResponseWithColdStart(attempt);
    return TypedResults.Ok(response);
  }

  private static async Task<Results<Ok<WrongApproachRevealResponse>, NotFound, BadRequest<ProblemDetails>>> GetWrongApproachReveal(
      Guid id,
      IAttemptRepository attemptRepository,
      IProblemRepository problemRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    // Only reveal after attempt is completed
    if (attempt.Status is not (AttemptStatus.Solved or AttemptStatus.GaveUp or AttemptStatus.TimedOut))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Cannot reveal before attempt is completed" });

    // Legacy flow only
    if (!attempt.ProblemId.HasValue)
      return TypedResults.BadRequest(new ProblemDetails { Detail = "This endpoint is for legacy problem attempts only" });

    var problem = await problemRepository.GetByIdWithWrongApproachesAsync(attempt.ProblemId.Value, ct);
    if (problem is null)
      return TypedResults.NotFound();

    var response = new WrongApproachRevealResponse(
        problem.Id,
        problem.Title,
        problem.CorrectPatternId,
        problem.CorrectPattern?.Name ?? "",
        problem.SolutionExplanation,
        problem.KeyInvariant,
        problem.WrongApproaches
            .OrderByDescending(w => w.FrequencyPercent)
            .Select(w => new WrongApproachResponse(
                w.WrongPatternId,
                w.WrongPattern?.Name ?? "",
                w.Explanation,
                w.FrequencyPercent))
            .ToList());

    return TypedResults.Ok(response);
  }

  private static async Task<Ok<IReadOnlyList<AttemptResponse>>> GetUserAttempts(
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    var attempts = await attemptRepository.GetByUserIdAsync(userId, ct);

    var responses = attempts
        .Select(a => MapToResponse(a, a.Problem?.Title ?? ""))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<AttemptResponse>>(responses);
  }

  private static async Task<Ok<ConfidenceDashboardResponse>> GetConfidenceDashboard(
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    var stats = await attemptRepository.GetConfidenceStatsAsync(userId, ct);
    var overconfident = await attemptRepository.GetOverconfidentPatternsAsync(userId, 2, ct);
    var fragile = await attemptRepository.GetFragilePatternsAsync(userId, 2, ct);

    var response = new ConfidenceDashboardResponse(
        stats.Select(s => new ConfidenceStatsResponse(
            s.Confidence,
            s.TotalAttempts,
            s.CorrectAttempts,
            s.WrongAttempts,
            s.TotalAttempts > 0 ? Math.Round((double)s.CorrectAttempts / s.TotalAttempts * 100, 1) : 0))
            .ToList(),
        overconfident.Select(p => new PatternWeaknessResponse(
            p.PatternId,
            p.PatternName,
            p.TotalAttempts,
            p.WrongCount,
            p.WrongPercentage,
            $"You're often confident but wrong when choosing {p.PatternName}"))
            .ToList(),
        fragile.Select(p => new PatternWeaknessResponse(
            p.PatternId,
            p.PatternName,
            p.TotalAttempts,
            p.WrongCount,
            p.WrongPercentage,
            $"You get {p.PatternName} right but lack confidence - study the trigger signals"))
            .ToList());

    return TypedResults.Ok(response);
  }

  private static async Task<Ok<ColdStartSettingsResponse>> GetColdStartSettings(
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    var recentAttempts = await attemptRepository.GetRecentByUserIdAsync(userId, 10, ct);
    var completedAttempts = recentAttempts
        .Where(a => a.Status == Domain.Enums.AttemptStatus.Solved)
        .ToList();

    // Calculate performance metrics
    int attemptsSampled = recentAttempts.Count;
    double accuracy = completedAttempts.Count > 0
        ? (double)completedAttempts.Count(a => a.IsPatternCorrect) / completedAttempts.Count * 100
        : 0;

    // Determine tier and duration
    string tier;
    int duration;
    bool recommendMultipleHypothesis;

    if (attemptsSampled < 5)
    {
      tier = "new";
      duration = 30;
      recommendMultipleHypothesis = false;
    }
    else if (accuracy >= 70)
    {
      tier = "good";
      duration = 30;
      recommendMultipleHypothesis = true; // Recommend for advanced users
    }
    else if (accuracy >= 50)
    {
      tier = "moderate";
      duration = 90;
      recommendMultipleHypothesis = true;
    }
    else
    {
      tier = "struggling";
      duration = 180;
      recommendMultipleHypothesis = false; // Focus on one pattern first
    }

    // Select interview-style prompt based on tier
    string interviewPrompt = tier switch
    {
      "new" => "Take 30 seconds to read the problem. What patterns come to mind? Don't rush to code.",
      "good" => "You have 30 seconds. Walk me through your initial approach. What signals do you see?",
      "moderate" => "I want you to spend 90 seconds thinking before touching the keyboard. What's your hypothesis?",
      "struggling" => "Let's slow down. Take 3 minutes to really understand the problem. What constraints matter most?",
      _ => "Before we start coding, walk me through how you'd approach this problem."
    };

    var response = new ColdStartSettingsResponse(
        duration,
        tier,
        Math.Round(accuracy, 1),
        attemptsSampled,
        recommendMultipleHypothesis,
        interviewPrompt);

    return TypedResults.Ok(response);
  }

  private static async Task<Results<Ok<ReflectionResponse>, NotFound, BadRequest<ProblemDetails>>> GenerateReflection(
      Guid id,
      GenerateReflectionRequest request,
      IAttemptRepository attemptRepository,
      ILeetCodeProblemCacheRepository cacheRepository,
      IAnalysisRepository analysisRepository,
      ILlmService llmService,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdWithReflectionAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    // Verify this is a LeetCode-based attempt
    if (!attempt.LeetCodeProblemCacheId.HasValue)
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Reflection only available for LeetCode-based attempts" });

    // Check if reflection already exists
    if (attempt.Reflection is not null)
    {
      return TypedResults.Ok(MapToReflectionResponse(attempt.Reflection));
    }

    // Get the cached problem with analysis
    var cached = await cacheRepository.GetWithAnalysisAsync(attempt.LeetCodeProblemCacheId.Value, ct);
    if (cached?.Analysis is null)
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Problem analysis not found. Analyze the problem first." });

    // Parse the analysis
    var primaryPatterns = JsonSerializer.Deserialize<List<string>>(cached.Analysis.PrimaryPatterns) ?? [];
    var keySignalsJson = JsonSerializer.Deserialize<List<SignalInfo>>(cached.Analysis.KeySignals) ?? [];
    var keySignals = keySignalsJson.Select(s => s.Signal).ToList();

    // Generate reflection
    var reflectionResult = await llmService.GenerateReflectionAsync(
        cached.Title,
        primaryPatterns,
        keySignals,
        request.ChosenPattern,
        request.IdentifiedSignals,
        request.ConfidenceLevel,
        ct
    );

    // Save reflection
    var reflection = Reflection.Create(
        attempt.Id,
        request.IdentifiedSignals, // userColdStartSummary
        reflectionResult.IsCorrectPattern, // wasPatternCorrect
        reflectionResult.Feedback,
        JsonSerializer.Serialize(reflectionResult.CorrectIdentifications),
        JsonSerializer.Serialize(reflectionResult.MissedSignals),
        reflectionResult.NextTimeAdvice,
        reflectionResult.PatternTips,
        reflectionResult.ConfidenceCalibration,
        "gpt-4o",
        reflectionResult.RawResponse
    );

    await analysisRepository.AddReflectionAsync(reflection, ct);

    // Update attempt with chosen pattern
    attempt.SetChosenPattern(request.ChosenPattern);
    attempt.SetPatternCorrectness(reflectionResult.IsCorrectPattern);
    await attemptRepository.UpdateAsync(attempt, ct);

    return TypedResults.Ok(MapToReflectionResponse(reflection));
  }

  private static async Task<Results<Ok<ReflectionResponse>, NotFound>> GetReflection(
      Guid id,
      IAttemptRepository attemptRepository,
      ClaimsPrincipal user,
      CancellationToken ct)
  {
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    var attempt = await attemptRepository.GetByIdWithReflectionAsync(id, ct);

    if (attempt is null)
      return TypedResults.NotFound();

    if (attempt.UserId != userId)
      return TypedResults.NotFound();

    if (attempt.Reflection is null)
      return TypedResults.NotFound();

    return TypedResults.Ok(MapToReflectionResponse(attempt.Reflection));
  }

  private static ReflectionResponse MapToReflectionResponse(Reflection reflection)
  {
    return new ReflectionResponse(
        reflection.Id,
        reflection.Feedback,
        reflection.CorrectIdentifications,
        reflection.MissedSignals,
        reflection.NextTimeAdvice,
        reflection.PatternTips,
        reflection.ConfidenceCalibration,
        reflection.WasPatternCorrect,
        reflection.GeneratedAt
    );
  }

  private static AttemptResponse MapToResponse(Attempt attempt, string problemTitle)
  {
    return new AttemptResponse(
        attempt.Id,
        attempt.ProblemId,
        attempt.LeetCodeProblemCacheId,
        problemTitle,
        attempt.Status,
        attempt.Confidence == default ? null : attempt.Confidence,
        attempt.Status is AttemptStatus.Solved or AttemptStatus.GaveUp or AttemptStatus.TimedOut
            ? attempt.IsPatternCorrect
            : null,
        attempt.StartedAt,
        attempt.CompletedAt,
        attempt.TotalTimeSeconds,
        null);
  }

  private static AttemptResponse MapToResponseWithColdStart(Attempt attempt)
  {
    ColdStartResponse? coldStart = null;
    if (attempt.ColdStartSubmission is not null)
    {
      var cs = attempt.ColdStartSubmission;
      coldStart = new ColdStartResponse(
          cs.Id,
          cs.IdentifiedSignals,
          cs.ChosenPatternId,
          cs.ChosenPattern?.Name ?? "",
          cs.SecondaryPatternId,
          cs.SecondaryPattern?.Name,
          cs.PrimaryVsSecondaryReason,
          cs.RejectedPatternId,
          cs.RejectedPattern?.Name,
          cs.RejectionReason,
          cs.ThinkingDurationSeconds,
          cs.SubmittedAt);
    }

    return new AttemptResponse(
        attempt.Id,
        attempt.ProblemId,
        attempt.LeetCodeProblemCacheId,
        attempt.Problem?.Title ?? attempt.LeetCodeProblem?.Title ?? "",
        attempt.Status,
        attempt.Confidence == default ? null : attempt.Confidence,
        attempt.Status is AttemptStatus.Solved or AttemptStatus.GaveUp or AttemptStatus.TimedOut
            ? attempt.IsPatternCorrect
            : null,
        attempt.StartedAt,
        attempt.CompletedAt,
        attempt.TotalTimeSeconds,
        coldStart);
  }
}
