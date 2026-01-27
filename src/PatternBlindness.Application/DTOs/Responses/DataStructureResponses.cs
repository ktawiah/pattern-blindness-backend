using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Operation info for a data structure.
/// </summary>
public record OperationInfo(
    string Name,
    string TimeComplexity,
    string Description);

/// <summary>
/// Response containing full data structure details.
/// </summary>
public record DataStructureResponse(
    Guid Id,
    string Name,
    string Description,
    DataStructureCategory Category,
    string WhatItIs,
    OperationInfo[] Operations,
    string WhenToUse,
    string Tradeoffs,
    string[] CommonUseCases,
    string Implementation,
    ResourceLink[] Resources,
    Guid[] RelatedStructureIds);

/// <summary>
/// Brief data structure response for lists.
/// </summary>
public record DataStructureBriefResponse(
    Guid Id,
    string Name,
    DataStructureCategory Category);
