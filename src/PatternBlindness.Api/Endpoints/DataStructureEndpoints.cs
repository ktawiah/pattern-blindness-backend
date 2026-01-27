using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using PatternBlindness.Application.DTOs.Responses;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Domain.Interfaces;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for managing data structures.
/// </summary>
public static class DataStructureEndpoints
{
  public static void MapDataStructureEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/data-structures")
        .WithTags("Data Structures");

    group.MapGet("/", GetAllDataStructures)
        .WithName("GetAllDataStructures")
        .WithDescription("Get all available data structures")
        .Produces<IReadOnlyList<DataStructureResponse>>(StatusCodes.Status200OK);

    group.MapGet("/{id:guid}", GetDataStructure)
        .WithName("GetDataStructure")
        .WithDescription("Get a data structure by ID")
        .Produces<DataStructureResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

    group.MapGet("/category/{category}", GetDataStructuresByCategory)
        .WithName("GetDataStructuresByCategory")
        .WithDescription("Get data structures by category")
        .Produces<IReadOnlyList<DataStructureBriefResponse>>(StatusCodes.Status200OK);

    group.MapGet("/search", SearchDataStructures)
        .WithName("SearchDataStructures")
        .WithDescription("Search data structures by name or description")
        .Produces<IReadOnlyList<DataStructureBriefResponse>>(StatusCodes.Status200OK);
  }

  private static async Task<Ok<IReadOnlyList<DataStructureResponse>>> GetAllDataStructures(
      IDataStructureRepository dataStructureRepository,
      CancellationToken ct)
  {
    var dataStructures = await dataStructureRepository.GetAllAsync(ct);

    var responses = dataStructures.Select(MapToResponse).ToList();

    return TypedResults.Ok<IReadOnlyList<DataStructureResponse>>(responses);
  }

  private static async Task<Results<Ok<DataStructureResponse>, NotFound>> GetDataStructure(
      Guid id,
      IDataStructureRepository dataStructureRepository,
      CancellationToken ct)
  {
    var dataStructure = await dataStructureRepository.GetByIdAsync(id, ct);

    if (dataStructure is null)
      return TypedResults.NotFound();

    return TypedResults.Ok(MapToResponse(dataStructure));
  }

  private static async Task<Ok<IReadOnlyList<DataStructureBriefResponse>>> GetDataStructuresByCategory(
      DataStructureCategory category,
      IDataStructureRepository dataStructureRepository,
      CancellationToken ct)
  {
    var dataStructures = await dataStructureRepository.GetByCategoryAsync(category, ct);

    var responses = dataStructures.Select(ds => new DataStructureBriefResponse(
        ds.Id,
        ds.Name,
        ds.Category))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<DataStructureBriefResponse>>(responses);
  }

  private static async Task<Ok<IReadOnlyList<DataStructureBriefResponse>>> SearchDataStructures(
      string query,
      IDataStructureRepository dataStructureRepository,
      CancellationToken ct)
  {
    var dataStructures = await dataStructureRepository.SearchAsync(query, ct);

    var responses = dataStructures.Select(ds => new DataStructureBriefResponse(
        ds.Id,
        ds.Name,
        ds.Category))
        .ToList();

    return TypedResults.Ok<IReadOnlyList<DataStructureBriefResponse>>(responses);
  }

  private static DataStructureResponse MapToResponse(Domain.Entities.DataStructure ds)
  {
    return new DataStructureResponse(
        ds.Id,
        ds.Name,
        ds.Description,
        ds.Category,
        ds.WhatItIs,
        ParseOperations(ds.Operations),
        ds.WhenToUse,
        ds.Tradeoffs,
        ParseJsonArray(ds.CommonUseCases),
        ds.Implementation,
        ParseResourceLinks(ds.Resources),
        ParseGuidArray(ds.RelatedStructureIds));
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

  private static OperationInfo[] ParseOperations(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<OperationInfo[]>(json, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      }) ?? [];
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
