using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PatternBlindness.Application.DTOs.Responses;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for managing patterns.
/// </summary>
public static class PatternEndpoints
{
  public static void MapPatternEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/patterns")
        .WithTags("Patterns");

    group.MapGet("/", GetAllPatterns)
        .WithName("GetAllPatterns")
        .WithDescription("Get all available patterns")
        .Produces<IReadOnlyList<PatternResponse>>(StatusCodes.Status200OK);

    group.MapGet("/{id:guid}", GetPattern)
        .WithName("GetPattern")
        .WithDescription("Get a pattern by ID")
        .Produces<PatternResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/category/{category}", GetPatternsByCategory)
        .WithName("GetPatternsByCategory")
        .WithDescription("Get patterns by category")
        .Produces<IReadOnlyList<PatternBriefResponse>>(StatusCodes.Status200OK);
  }

  private static async Task<Ok<IReadOnlyList<PatternResponse>>> GetAllPatterns(
      IPatternRepository patternRepository,
      CancellationToken ct)
  {
    var patterns = await patternRepository.GetAllAsync(ct);

    var responses = patterns.Select(MapToResponse).ToList();

    return TypedResults.Ok<IReadOnlyList<PatternResponse>>(responses);
  }

  private static async Task<Results<Ok<PatternResponse>, NotFound>> GetPattern(
      Guid id,
      IPatternRepository patternRepository,
      CancellationToken ct)
  {
    var pattern = await patternRepository.GetByIdAsync(id, ct);

    if (pattern is null)
      return TypedResults.NotFound();

    return TypedResults.Ok(MapToResponse(pattern));
  }

  private static async Task<Ok<IReadOnlyList<PatternBriefResponse>>> GetPatternsByCategory(
      PatternCategory category,
      IPatternRepository patternRepository,
      CancellationToken ct)
  {
    var patterns = await patternRepository.GetByCategoryAsync(category, ct);

    var responses = patterns.Select(p => new PatternBriefResponse(
        p.Id,
        p.Name,
        p.Category))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<PatternBriefResponse>>(responses);
  }

  private static PatternResponse MapToResponse(Domain.Entities.Pattern pattern)
  {
    return new PatternResponse(
        pattern.Id,
        pattern.Name,
        pattern.Description,
        pattern.Category,
        pattern.WhatItIs,
        pattern.WhenToUse,
        pattern.WhyItWorks,
        ParseJsonArray(pattern.CommonUseCases),
        pattern.TimeComplexity,
        pattern.SpaceComplexity,
        pattern.PseudoCode,
        ParseJsonArray(pattern.TriggerSignals),
        ParseJsonArray(pattern.CommonMistakes),
        ParseResourceLinks(pattern.Resources),
        ParseGuidArray(pattern.RelatedPatternIds));
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

  private static ResourceLink[] ParseResourceLinks(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<ResourceLink[]>(json, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      }) ?? [];
    }
    catch
    {
      return [];
    }
  }

  private static Guid[] ParseGuidArray(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<Guid[]>(json) ?? [];
    }
    catch
    {
      return [];
    }
  }
}
