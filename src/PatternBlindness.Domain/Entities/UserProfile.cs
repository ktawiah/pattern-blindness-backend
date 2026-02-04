using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents user qualification and progress tracking data.
/// Tracks DSA experience level and controls phased feature access.
/// </summary>
public class UserProfile : Entity
{
    private UserProfile() { } // EF Core

    /// <summary>
    /// The user this profile belongs to (ASP.NET Identity user ID).
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Number of DSA problems the user has completed before joining.
    /// Used for qualification gate (minimum 50 required).
    /// </summary>
    public int DsaProblemsCompleted { get; private set; }

    /// <summary>
    /// Whether the user passed the qualification gate.
    /// </summary>
    public bool IsQualified { get; private set; }

    /// <summary>
    /// When the user completed qualification.
    /// </summary>
    public DateTime? QualifiedAt { get; private set; }

    /// <summary>
    /// Current phase for progressive feature unlock (1-4).
    /// Phase 1: Problems 1-5 (basic flow)
    /// Phase 2: Problems 6-15 (light stats, nudges)
    /// Phase 3: Problems 16-30 (decay, blind spots)
    /// Phase 4: Problems 31+ (interview readiness)
    /// </summary>
    public int CurrentPhase { get; private set; } = 1;

    /// <summary>
    /// Number of completed attempts (for phase progression).
    /// </summary>
    public int CompletedAttempts { get; private set; }

    /// <summary>
    /// Whether this user was grandfathered (existing user before qualification).
    /// </summary>
    public bool WasGrandfathered { get; private set; }

    /// <summary>
    /// Whether user has opted into interview readiness mode (Phase 4 feature).
    /// </summary>
    public bool InterviewReadinessOptIn { get; private set; }

    /// <summary>
    /// Creates a new user profile for qualification.
    /// NOTE: All users are now auto-qualified. The DSA problems count is tracked for informational purposes only.
    /// </summary>
    public static UserProfile Create(string userId, int dsaProblemsCompleted)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));

        if (dsaProblemsCompleted < 0)
            throw new ArgumentException("DSA problems completed cannot be negative.", nameof(dsaProblemsCompleted));

        // Auto-qualify everyone - no gate
        var isQualified = true;

        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DsaProblemsCompleted = dsaProblemsCompleted,
            IsQualified = isQualified,
            QualifiedAt = DateTime.UtcNow,
            CurrentPhase = 1,
            CompletedAttempts = 0,
            WasGrandfathered = false,
            InterviewReadinessOptIn = false
        };
    }

    /// <summary>
    /// Creates a grandfathered profile for existing users.
    /// </summary>
    public static UserProfile CreateGrandfathered(string userId, int completedAttempts)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID is required.", nameof(userId));

        var phase = CalculatePhase(completedAttempts);

        return new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DsaProblemsCompleted = 100, // Assume qualified
            IsQualified = true,
            QualifiedAt = DateTime.UtcNow,
            CurrentPhase = phase,
            CompletedAttempts = completedAttempts,
            WasGrandfathered = true,
            InterviewReadinessOptIn = false
        };
    }

    /// <summary>
    /// Increments the completed attempts count and updates phase.
    /// </summary>
    public void IncrementCompletedAttempts()
    {
        CompletedAttempts++;
        CurrentPhase = CalculatePhase(CompletedAttempts);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Opts in to interview readiness mode.
    /// </summary>
    public void OptInToInterviewReadiness()
    {
        if (CurrentPhase < 4)
            throw new InvalidOperationException("Must be in Phase 4 to opt into interview readiness.");

        InterviewReadinessOptIn = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculates the phase based on completed attempts.
    /// </summary>
    private static int CalculatePhase(int completedAttempts) => completedAttempts switch
    {
        <= 5 => 1,
        <= 15 => 2,
        <= 30 => 3,
        _ => 4
    };
}

/// <summary>
/// Domain errors for UserProfile entity.
/// </summary>
public static class UserProfileErrors
{
    public static readonly Error NotFound = new("UserProfile.NotFound", "User profile not found.");
    public static readonly Error AlreadyExists = new("UserProfile.AlreadyExists", "User profile already exists.");
    public static readonly Error NotQualified = new("UserProfile.NotQualified", "User has not completed qualification.");
    public static readonly Error InsufficientExperience = new("UserProfile.InsufficientExperience", "This tool is designed for engineers with 50+ DSA problems completed.");
}
