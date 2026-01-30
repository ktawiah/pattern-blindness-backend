namespace PatternBlindness.Domain.Enums;

/// <summary>
/// Represents the outcome of the user's chosen approach after coding.
/// Part of the return gate for capturing what actually happened.
/// </summary>
public enum ApproachOutcome
{
    /// <summary>The chosen approach worked as expected</summary>
    Worked = 1,

    /// <summary>The approach partially worked but needed modifications</summary>
    PartiallyWorked = 2,

    /// <summary>The approach failed and required a different solution</summary>
    Failed = 3
}
