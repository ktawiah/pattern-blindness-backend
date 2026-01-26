using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Response containing pattern details.
/// </summary>
public record PatternResponse(
    Guid Id,
    string Name,
    string Description,
    PatternCategory Category,
    string[] TriggerSignals,
    string[] CommonMistakes);

/// <summary>
/// Brief pattern response for lists.
/// </summary>
public record PatternBriefResponse(
    Guid Id,
    string Name,
    PatternCategory Category);
