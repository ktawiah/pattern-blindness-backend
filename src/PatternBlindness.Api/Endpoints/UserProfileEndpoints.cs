using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PatternBlindness.Application.DTOs.Requests;
using PatternBlindness.Application.DTOs.Responses;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for user profile and qualification management.
/// Implements the qualification gate and phased feature reveal.
/// </summary>
public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile")
            .WithTags("UserProfile")
            .RequireAuthorization();

        group.MapGet("/", GetProfile)
            .WithName("GetUserProfile")
            .WithDescription("Get the current user's profile and phase information")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/qualify", SubmitQualification)
            .WithName("SubmitQualification")
            .WithDescription("Submit qualification answers to unlock the platform")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/check", CheckQualification)
            .WithName("CheckQualification")
            .WithDescription("Check if the user needs to complete qualification")
            .Produces<QualificationCheckResponse>(StatusCodes.Status200OK);

        group.MapGet("/active-attempt", GetActiveAttempt)
            .WithName("GetActiveAttempt")
            .WithDescription("Get the user's active attempt (for loop enforcement)")
            .Produces<ActiveAttemptResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/interview-readiness", OptInInterviewReadiness)
            .WithName("OptInInterviewReadiness")
            .WithDescription("Opt into interview readiness mode (Phase 4+)")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<UserProfileResponse>, NotFound>> GetProfile(
        IUserProfileRepository profileRepository,
        IAttemptRepository attemptRepository,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.NotFound();

        var profile = await profileRepository.GetByUserIdAsync(userId, ct);
        if (profile is null)
            return TypedResults.NotFound();

        var response = MapToResponse(profile);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<UserProfileResponse>, BadRequest<ProblemDetails>>> SubmitQualification(
        QualifyUserRequest request,
        IUserProfileRepository profileRepository,
        IAttemptRepository attemptRepository,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.BadRequest(new ProblemDetails { Detail = "User not found" });

        // Check if profile already exists
        var existingProfile = await profileRepository.GetByUserIdAsync(userId, ct);
        if (existingProfile is not null)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Detail = "Profile already exists. Qualification can only be submitted once."
            });
        }

        // Create new profile
        var profile = UserProfile.Create(userId, request.DsaProblemsCompleted);

        // Check qualification threshold
        if (!profile.IsQualified)
        {
            // Still save the profile but return a message about not qualifying
            await profileRepository.AddAsync(profile, ct);
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Insufficient Experience",
                Detail = "This tool is designed for engineers who have solved at least 50 DSA problems. " +
                         "We recommend practicing more on LeetCode, HackerRank, or similar platforms first. " +
                         "This isn't a coding practice tool - it trains pattern recognition for those who already have a foundation."
            });
        }

        await profileRepository.AddAsync(profile, ct);
        var response = MapToResponse(profile);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<QualificationCheckResponse>> CheckQualification(
        IUserProfileRepository profileRepository,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return TypedResults.Ok(new QualificationCheckResponse(
                IsQualified: false,
                NeedsQualification: true,
                Message: "Please complete qualification to access the platform."));
        }

        var profile = await profileRepository.GetByUserIdAsync(userId, ct);
        if (profile is null)
        {
            return TypedResults.Ok(new QualificationCheckResponse(
                IsQualified: false,
                NeedsQualification: true,
                Message: "Please complete qualification to access the platform."));
        }

        if (!profile.IsQualified)
        {
            return TypedResults.Ok(new QualificationCheckResponse(
                IsQualified: false,
                NeedsQualification: false, // Already submitted but didn't qualify
                Message: "This tool requires 50+ DSA problems solved. Please build your foundation first."));
        }

        return TypedResults.Ok(new QualificationCheckResponse(
            IsQualified: true,
            NeedsQualification: false,
            Message: null));
    }

    private static async Task<Results<Ok<ActiveAttemptResponse>, NoContent>> GetActiveAttempt(
        IAttemptRepository attemptRepository,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.NoContent();

        var activeAttempt = await attemptRepository.GetActiveAttemptByUserIdAsync(userId, ct);
        if (activeAttempt is null)
            return TypedResults.NoContent();

        var problemTitle = activeAttempt.Problem?.Title
            ?? activeAttempt.LeetCodeProblem?.Title
            ?? "Unknown Problem";

        return TypedResults.Ok(new ActiveAttemptResponse(
            AttemptId: activeAttempt.Id,
            ProblemTitle: problemTitle,
            StartedAt: activeAttempt.StartedAt,
            Status: activeAttempt.Status.ToString()));
    }

    private static async Task<Results<Ok<UserProfileResponse>, BadRequest<ProblemDetails>>> OptInInterviewReadiness(
        IUserProfileRepository profileRepository,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.BadRequest(new ProblemDetails { Detail = "User not found" });

        var profile = await profileRepository.GetByUserIdAsync(userId, ct);
        if (profile is null)
            return TypedResults.BadRequest(new ProblemDetails { Detail = "Profile not found" });

        if (profile.CurrentPhase < 4)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Detail = $"Interview readiness mode is available after completing 30 problems. You've completed {profile.CompletedAttempts}."
            });
        }

        profile.OptInToInterviewReadiness();
        await profileRepository.UpdateAsync(profile, ct);

        var response = MapToResponse(profile);
        return TypedResults.Ok(response);
    }

    private static UserProfileResponse MapToResponse(UserProfile profile)
    {
        var featureAccess = CalculateFeatureAccess(profile);
        return new UserProfileResponse(
            UserId: profile.UserId,
            IsQualified: profile.IsQualified,
            DsaProblemsCompleted: profile.DsaProblemsCompleted,
            QualifiedAt: profile.QualifiedAt,
            CurrentPhase: profile.CurrentPhase,
            CompletedAttempts: profile.CompletedAttempts,
            WasGrandfathered: profile.WasGrandfathered,
            InterviewReadinessOptIn: profile.InterviewReadinessOptIn,
            FeatureAccess: featureAccess);
    }

    private static FeatureAccessResponse CalculateFeatureAccess(UserProfile profile)
    {
        var phase = profile.CurrentPhase;
        var completed = profile.CompletedAttempts;

        // Calculate problems to next phase
        int problemsToNext = phase switch
        {
            1 => 5 - completed,
            2 => 15 - completed,
            3 => 30 - completed,
            _ => 0
        };

        return new FeatureAccessResponse(
            Phase: phase,
            ProblemsInPhase: completed,
            ProblemsToNextPhase: Math.Max(0, problemsToNext),
            ShowConfidenceMetrics: phase >= 2,
            ShowPatternUsageStats: phase >= 2,
            ShowBlindSpots: phase >= 3,
            ShowPatternDecay: phase >= 3,
            ShowThinkingReplay: phase >= 3,
            ShowInterviewReadiness: phase >= 4 && profile.InterviewReadinessOptIn);
    }
}
