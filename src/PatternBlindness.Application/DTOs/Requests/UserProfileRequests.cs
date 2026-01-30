namespace PatternBlindness.Application.DTOs.Requests;

/// <summary>
/// Request to submit qualification answers.
/// </summary>
public record QualifyUserRequest(
    /// <summary>Number of DSA problems the user has solved (self-reported).</summary>
    int DsaProblemsCompleted);

/// <summary>
/// Request to opt into interview readiness mode.
/// </summary>
public record OptInInterviewReadinessRequest();
