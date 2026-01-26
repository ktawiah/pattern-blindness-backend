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
        .WithDescription("Submit the cold start thinking phase (90+ seconds of deliberate thinking)")
        .Produces<AttemptResponse>(StatusCodes.Status200OK)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/{id:guid}/complete", CompleteAttempt)
        .WithName("CompleteAttempt")
        .WithDescription("Mark an attempt as completed")
        .Produces<AttemptResponse>(StatusCodes.Status200OK)
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

    // Validate rejected pattern if provided
    if (request.RejectedPatternId.HasValue && !await patternRepository.ExistsAsync(request.RejectedPatternId.Value, ct))
      return TypedResults.BadRequest(new ProblemDetails { Detail = "Rejected pattern does not exist" });

    var result = attempt.SubmitColdStart(
        request.IdentifiedSignals,
        request.ChosenPatternId,
        request.RejectedPatternId,
        request.RejectionReason,
        request.Confidence,
        request.ThinkingDurationSeconds);

    if (result.IsFailure)
      return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

    await attemptRepository.UpdateAsync(attempt, ct);

    var updatedAttempt = await attemptRepository.GetByIdWithColdStartAsync(id, ct);
    var response = MapToResponseWithColdStart(updatedAttempt!);
    return TypedResults.Ok(response);
  }

  private static async Task<Results<Ok<AttemptResponse>, NotFound, BadRequest<ProblemDetails>>> CompleteAttempt(
      Guid id,
      CompleteAttemptRequest request,
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

    var result = attempt.Complete(request.IsPatternCorrect);

    if (result.IsFailure)
      return TypedResults.BadRequest(new ProblemDetails { Detail = result.Error.Message });

    await attemptRepository.UpdateAsync(attempt, ct);

    var response = MapToResponse(attempt, attempt.Problem?.Title ?? "");
    return TypedResults.Ok(response);
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

    var problem = await problemRepository.GetByIdWithWrongApproachesAsync(attempt.ProblemId, ct);
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

  private static AttemptResponse MapToResponse(Attempt attempt, string problemTitle)
  {
    return new AttemptResponse(
        attempt.Id,
        attempt.ProblemId,
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
          cs.RejectedPatternId,
          cs.RejectedPattern?.Name,
          cs.RejectionReason,
          cs.ThinkingDurationSeconds,
          cs.SubmittedAt);
    }

    return new AttemptResponse(
        attempt.Id,
        attempt.ProblemId,
        attempt.Problem?.Title ?? "",
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
