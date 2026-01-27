using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.DTOs.Requests;

/// <summary>
/// Request to start a new problem attempt.
/// </summary>
public record StartAttemptRequest(Guid ProblemId);

/// <summary>
/// Request to submit the cold start thinking phase.
/// Supports multiple hypothesis mode with primary and secondary pattern choices.
/// </summary>
public record SubmitColdStartRequest(
    /// <summary>Notes about signals identified from the problem.</summary>
    string IdentifiedSignals,

    /// <summary>Free-text notes about patterns the user considered but rejected.</summary>
    string? RejectedPatterns,

    /// <summary>The primary pattern chosen to solve the problem.</summary>
    Guid ChosenPatternId,

    /// <summary>The secondary/backup pattern hypothesis (for multiple hypothesis mode).</summary>
    Guid? SecondaryPatternId,

    /// <summary>Brief reasoning for why primary was ranked over secondary.</summary>
    string? PrimaryVsSecondaryReason,

    /// <summary>A specific pattern ID considered but rejected (optional).</summary>
    Guid? RejectedPatternId,

    /// <summary>Why the rejected pattern was not chosen.</summary>
    string? RejectionReason,

    /// <summary>User's confidence in their pattern choice (optional during cold start).</summary>
    ConfidenceLevel? Confidence,

    /// <summary>Time spent thinking in seconds.</summary>
    int ThinkingDurationSeconds);

/// <summary>
/// Request to complete an attempt.
/// </summary>
public record CompleteAttemptRequest(
    /// <summary>User's confidence level (1-5).</summary>
    int Confidence,

    /// <summary>Whether the user's pattern choice was correct.</summary>
    bool IsPatternCorrect);

/// <summary>
/// Request to generate a reflection for a LeetCode-based attempt.
/// </summary>
public record GenerateReflectionRequest(
    /// <summary>The pattern the user chose for this problem.</summary>
    string ChosenPattern,

    /// <summary>The signals the user identified from the problem.</summary>
    string IdentifiedSignals,

    /// <summary>User's confidence level (0-100).</summary>
    int ConfidenceLevel);
