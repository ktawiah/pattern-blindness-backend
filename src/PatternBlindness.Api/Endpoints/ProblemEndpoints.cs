using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PatternBlindness.Application.DTOs.Responses;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for managing problems.
/// </summary>
public static class ProblemEndpoints
{
  public static void MapProblemEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/problems")
        .WithTags("Problems");

    group.MapGet("/", GetAllProblems)
        .WithName("GetAllProblems")
        .WithDescription("Get all active problems")
        .Produces<IReadOnlyList<ProblemBriefResponse>>(StatusCodes.Status200OK);

    group.MapGet("/{id:guid}", GetProblem)
        .WithName("GetProblem")
        .WithDescription("Get a problem by ID (without solution)")
        .Produces<ProblemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/difficulty/{difficulty}", GetProblemsByDifficulty)
        .WithName("GetProblemsByDifficulty")
        .WithDescription("Get problems by difficulty level")
        .Produces<IReadOnlyList<ProblemBriefResponse>>(StatusCodes.Status200OK);

    group.MapGet("/random", GetRandomProblem)
        .WithName("GetRandomProblem")
        .WithDescription("Get a random active problem")
        .Produces<ProblemResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
  }

  private static async Task<Ok<IReadOnlyList<ProblemBriefResponse>>> GetAllProblems(
      IProblemRepository problemRepository,
      CancellationToken ct)
  {
    var problems = await problemRepository.GetAllActiveAsync(ct);

    var responses = problems.Select(p => new ProblemBriefResponse(
        p.Id,
        p.Title,
        p.Difficulty))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<ProblemBriefResponse>>(responses);
  }

  private static async Task<Results<Ok<ProblemResponse>, NotFound>> GetProblem(
      Guid id,
      IProblemRepository problemRepository,
      CancellationToken ct)
  {
    var problem = await problemRepository.GetByIdAsync(id, ct);

    if (problem is null || !problem.IsActive)
      return TypedResults.NotFound();

    // Note: We don't include the solution or correct pattern here
    // That's revealed only after the attempt is completed
    var response = new ProblemResponse(
        problem.Id,
        problem.Title,
        problem.Description,
        problem.Difficulty,
        ParseJsonArray(problem.Signals),
        ParseJsonArray(problem.Constraints),
        ParseJsonArray(problem.Examples));

    return TypedResults.Ok(response);
  }

  private static async Task<Ok<IReadOnlyList<ProblemBriefResponse>>> GetProblemsByDifficulty(
      Difficulty difficulty,
      IProblemRepository problemRepository,
      CancellationToken ct)
  {
    var problems = await problemRepository.GetByDifficultyAsync(difficulty, ct);

    var responses = problems.Select(p => new ProblemBriefResponse(
        p.Id,
        p.Title,
        p.Difficulty))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<ProblemBriefResponse>>(responses);
  }

  private static async Task<Results<Ok<ProblemResponse>, NotFound>> GetRandomProblem(
      IProblemRepository problemRepository,
      Difficulty? difficulty,
      CancellationToken ct)
  {
    var problem = await problemRepository.GetRandomActiveAsync(difficulty, ct);

    if (problem is null)
      return TypedResults.NotFound();

    var response = new ProblemResponse(
        problem.Id,
        problem.Title,
        problem.Description,
        problem.Difficulty,
        ParseJsonArray(problem.Signals),
        ParseJsonArray(problem.Constraints),
        ParseJsonArray(problem.Examples));

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
}
