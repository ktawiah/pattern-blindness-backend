using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Responses;

/// <summary>
/// Response containing problem details.
/// </summary>
public record ProblemResponse(
    Guid Id,
    string Title,
    string Description,
    Difficulty Difficulty,
    string[] Signals,
    string[] Constraints,
    string[] Examples);

/// <summary>
/// Brief problem response for lists.
/// </summary>
public record ProblemBriefResponse(
    Guid Id,
    string Title,
    Difficulty Difficulty);

/// <summary>
/// Problem response with solution revealed (shown after completion).
/// </summary>
public record ProblemWithSolutionResponse(
    Guid Id,
    string Title,
    string Description,
    Difficulty Difficulty,
    string[] Signals,
    string[] Constraints,
    string[] Examples,
    Guid CorrectPatternId,
    string CorrectPatternName,
    string KeyInvariant,
    string SolutionExplanation,
    IReadOnlyList<WrongApproachDto> WrongApproaches);

/// <summary>
/// Wrong approach DTO for solution response.
/// </summary>
public record WrongApproachDto(
    Guid PatternId,
    string PatternName,
    string Explanation,
    int FrequencyPercent);
