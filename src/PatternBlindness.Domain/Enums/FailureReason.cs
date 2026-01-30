namespace PatternBlindness.Domain.Enums;

/// <summary>
/// Represents what broke first when an approach failed.
/// Helps identify patterns in user's failure modes.
/// </summary>
public enum FailureReason
{
    /// <summary>The core invariant/assumption was wrong</summary>
    WrongInvariant = 1,

    /// <summary>Edge case not handled correctly</summary>
    EdgeCase = 2,

    /// <summary>Time complexity was too high (TLE)</summary>
    TimeComplexity = 3,

    /// <summary>Implementation bug (logic error)</summary>
    ImplementationBug = 4,

    /// <summary>Space complexity issue (MLE)</summary>
    SpaceComplexity = 5,

    /// <summary>Other reason not listed</summary>
    Other = 6
}
