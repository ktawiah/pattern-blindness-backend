using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Resource link for a pattern or data structure.
/// </summary>
public record ResourceLink(
    string Title,
    string Url,
    string Type);

/// <summary>
/// Response containing full pattern details.
/// </summary>
public record PatternResponse(
    Guid Id,
    string Name,
    string Description,
    PatternCategory Category,
    string WhatItIs,
    string WhenToUse,
    string WhyItWorks,
    string[] CommonUseCases,
    string TimeComplexity,
    string SpaceComplexity,
    string PseudoCode,
    string[] TriggerSignals,
    string[] CommonMistakes,
    ResourceLink[] Resources,
    Guid[] RelatedPatternIds);

/// <summary>
/// Brief pattern response for lists.
/// </summary>
public record PatternBriefResponse(
    Guid Id,
    string Name,
    PatternCategory Category);
