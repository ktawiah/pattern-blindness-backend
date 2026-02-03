using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using PatternBlindness.Application.Interfaces;

namespace PatternBlindness.Api.Endpoints;

/// <summary>
/// Endpoints for pattern usage tracking and blind spot detection.
/// </summary>
public static class PatternTrackingEndpoints
{
    public static void MapPatternTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/patterns/tracking")
            .WithTags("Pattern Tracking")
            .RequireAuthorization();

        group.MapGet("/stats", GetPatternUsageStats)
            .WithName("GetPatternUsageStats")
            .WithDescription("Get complete pattern usage statistics including decaying, default, and avoided patterns")
            .Produces<PatternUsageStatsResponse>(StatusCodes.Status200OK);

        group.MapGet("/decaying", GetDecayingPatterns)
            .WithName("GetDecayingPatterns")
            .WithDescription("Get patterns that haven't been practiced recently")
            .Produces<IReadOnlyList<DecayingPatternResponse>>(StatusCodes.Status200OK);

        group.MapGet("/defaults", GetDefaultPatterns)
            .WithName("GetDefaultPatterns")
            .WithDescription("Get patterns the user tends to choose repeatedly")
            .Produces<IReadOnlyList<DefaultPatternResponse>>(StatusCodes.Status200OK);

        group.MapGet("/nudge/{patternId:guid}", CheckPatternNudge)
            .WithName("CheckPatternNudge")
            .WithDescription("Check if user should be nudged about over-relying on a pattern")
            .Produces<PatternNudgeResponse?>(StatusCodes.Status200OK);
    }

    private static async Task<Ok<PatternUsageStatsResponse>> GetPatternUsageStats(
        IPatternTrackingService trackingService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var stats = await trackingService.GetPatternUsageStatsAsync(userId, ct);

        var response = new PatternUsageStatsResponse(
            stats.DecayingPatterns.Select(p => new DecayingPatternResponse(
                p.PatternId, p.PatternName, p.LastUsedAt, p.DaysSinceLastUse, p.TotalTimesUsed, p.SuccessRate)).ToList(),
            stats.DefaultPatterns.Select(p => new DefaultPatternResponse(
                p.PatternId, p.PatternName, p.TimesChosen, p.ConsecutiveChoices, p.PercentageOfTotal, p.SuccessRate)).ToList(),
            stats.AvoidedPatterns.Select(p => new AvoidedPatternResponse(
                p.PatternId, p.PatternName, p.TimesCorrectAnswer, p.TimesUserChoseIt)).ToList(),
            stats.TotalAttempts,
            stats.UniquePatternsPracticed,
            stats.TotalPatterns);

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<IReadOnlyList<DecayingPatternResponse>>> GetDecayingPatterns(
        IPatternTrackingService trackingService,
        ClaimsPrincipal user,
        int days = 30,
        CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var patterns = await trackingService.GetDecayingPatternsAsync(userId, days, ct);

        var response = patterns.Select(p => new DecayingPatternResponse(
            p.PatternId, p.PatternName, p.LastUsedAt, p.DaysSinceLastUse, p.TotalTimesUsed, p.SuccessRate)).ToList();

        return TypedResults.Ok<IReadOnlyList<DecayingPatternResponse>>(response);
    }

    private static async Task<Ok<IReadOnlyList<DefaultPatternResponse>>> GetDefaultPatterns(
        IPatternTrackingService trackingService,
        ClaimsPrincipal user,
        int minOccurrences = 3,
        CancellationToken ct = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var patterns = await trackingService.GetDefaultPatternsAsync(userId, minOccurrences, ct);

        var response = patterns.Select(p => new DefaultPatternResponse(
            p.PatternId, p.PatternName, p.TimesChosen, p.ConsecutiveChoices, p.PercentageOfTotal, p.SuccessRate)).ToList();

        return TypedResults.Ok<IReadOnlyList<DefaultPatternResponse>>(response);
    }

    private static async Task<Ok<PatternNudgeResponse?>> CheckPatternNudge(
        Guid patternId,
        IPatternTrackingService trackingService,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var nudge = await trackingService.CheckForPatternNudgeAsync(userId, patternId, 3, ct);

        if (nudge == null)
            return TypedResults.Ok<PatternNudgeResponse?>(null);

        return TypedResults.Ok<PatternNudgeResponse?>(new PatternNudgeResponse(
            nudge.PatternId, nudge.PatternName, nudge.ConsecutiveChoices, nudge.Message));
    }
}

// Response DTOs
public record PatternUsageStatsResponse(
    IReadOnlyList<DecayingPatternResponse> DecayingPatterns,
    IReadOnlyList<DefaultPatternResponse> DefaultPatterns,
    IReadOnlyList<AvoidedPatternResponse> AvoidedPatterns,
    int TotalAttempts,
    int UniquePatternsPracticed,
    int TotalPatterns);

public record DecayingPatternResponse(
    Guid PatternId,
    string PatternName,
    DateTime LastUsedAt,
    int DaysSinceLastUse,
    int TotalTimesUsed,
    double SuccessRate);

public record DefaultPatternResponse(
    Guid PatternId,
    string PatternName,
    int TimesChosen,
    int ConsecutiveChoices,
    double PercentageOfTotal,
    double SuccessRate);

public record AvoidedPatternResponse(
    Guid PatternId,
    string PatternName,
    int TimesCorrectAnswer,
    int TimesUserChoseIt);

public record PatternNudgeResponse(
    Guid PatternId,
    string PatternName,
    int ConsecutiveChoices,
    string Message);
