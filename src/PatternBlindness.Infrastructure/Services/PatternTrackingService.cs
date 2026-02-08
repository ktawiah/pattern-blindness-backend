using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Services;

/// <summary>
/// Service for tracking pattern usage statistics and identifying blind spots.
/// </summary>
public class PatternTrackingService : IPatternTrackingService
{
    private readonly ApplicationDbContext _context;

    public PatternTrackingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DecayingPatternInfo>> GetDecayingPatternsAsync(
        string userId,
        int daysThreshold = 30,
        CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysThreshold);

        // Get all patterns the user has used
        var userPatternUsage = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .GroupBy(a => new { a.ChosenPatternId, a.ChosenPatternName })
            .Select(g => new
            {
                PatternId = g.Key.ChosenPatternId!.Value,
                PatternName = g.Key.ChosenPatternName ?? "Unknown",
                LastUsedAt = g.Max(a => a.CompletedAt ?? a.StartedAt),
                TotalTimesUsed = g.Count(),
                CorrectCount = g.Count(a => a.IsPatternCorrect)
            })
            .ToListAsync(ct);

        // Filter for decaying patterns
        var decayingPatterns = userPatternUsage
            .Where(p => p.LastUsedAt < cutoffDate)
            .ToList();

        if (decayingPatterns.Count == 0)
            return [];

        return decayingPatterns
            .Select(p => new DecayingPatternInfo(
                p.PatternId,
                p.PatternName,
                p.LastUsedAt,
                (int)(DateTime.UtcNow - p.LastUsedAt).TotalDays,
                p.TotalTimesUsed,
                p.TotalTimesUsed > 0 ? (double)p.CorrectCount / p.TotalTimesUsed : 0))
            .OrderByDescending(p => p.DaysSinceLastUse)
            .ToList();
    }

    public async Task<IReadOnlyList<DefaultPatternInfo>> GetDefaultPatternsAsync(
        string userId,
        int minOccurrences = 5,
        CancellationToken ct = default)
    {
        var totalAttempts = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .CountAsync(ct);

        if (totalAttempts == 0)
            return [];

        var patternUsage = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .GroupBy(a => new { a.ChosenPatternId, a.ChosenPatternName })
            .Select(g => new
            {
                PatternId = g.Key.ChosenPatternId!.Value,
                PatternName = g.Key.ChosenPatternName ?? "Unknown",
                TimesChosen = g.Count(),
                CorrectCount = g.Count(a => a.IsPatternCorrect)
            })
            .Where(p => p.TimesChosen >= minOccurrences)
            .ToListAsync(ct);

        if (patternUsage.Count == 0)
            return [];

        // Calculate consecutive choices for each pattern (simplified - checks last N attempts)
        var consecutiveChoices = new Dictionary<Guid, int>();
        var recentAttempts = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .OrderByDescending(a => a.CompletedAt ?? a.StartedAt)
            .Take(10)
            .Select(a => a.ChosenPatternId!.Value)
            .ToListAsync(ct);

        var patternIds = patternUsage.Select(p => p.PatternId).ToList();
        foreach (var patternId in patternIds)
        {
            var count = 0;
            foreach (var recentPatternId in recentAttempts)
            {
                if (recentPatternId == patternId)
                    count++;
                else
                    break;
            }
            consecutiveChoices[patternId] = count;
        }

        return patternUsage
            .Select(p => new DefaultPatternInfo(
                p.PatternId,
                p.PatternName,
                p.TimesChosen,
                consecutiveChoices.GetValueOrDefault(p.PatternId, 0),
                (double)p.TimesChosen / totalAttempts * 100,
                p.TimesChosen > 0 ? (double)p.CorrectCount / p.TimesChosen : 0))
            .OrderByDescending(p => p.PercentageOfTotal)
            .ToList();
    }

    public async Task<IReadOnlyList<AvoidedPatternInfo>> GetAvoidedPatternsAsync(
        string userId,
        CancellationToken ct = default)
    {
        // Get patterns that were correct answers but user never chose them
        // This requires looking at problems with known correct patterns

        // Get all patterns the user has chosen
        var chosenPatternIds = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .Select(a => a.ChosenPatternId!.Value)
            .Distinct()
            .ToListAsync(ct);

        // Get patterns that were correct answers for problems the user attempted
        var correctPatternStats = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ProblemId.HasValue
                && a.Problem != null
                && a.Problem.CorrectPatternId != Guid.Empty)
            .GroupBy(a => a.Problem!.CorrectPatternId)
            .Select(g => new
            {
                PatternId = g.Key,
                TimesCorrectAnswer = g.Count(),
                TimesUserChoseIt = g.Count(a => a.ChosenPatternId == g.Key)
            })
            .ToListAsync(ct);

        // Filter for avoided patterns (correct answer but user rarely/never chose it)
        var avoidedPatterns = correctPatternStats
            .Where(p => p.TimesUserChoseIt == 0 || (double)p.TimesUserChoseIt / p.TimesCorrectAnswer < 0.3)
            .Select(p => p.PatternId)
            .ToList();

        if (avoidedPatterns.Count == 0)
            return [];

        // Get pattern names from user's attempts where they chose these patterns
        var patternNames = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.ChosenPatternId.HasValue && avoidedPatterns.Contains(a.ChosenPatternId.Value))
            .GroupBy(a => a.ChosenPatternId!.Value)
            .Select(g => new { PatternId = g.Key, PatternName = g.First().ChosenPatternName ?? "Unknown" })
            .ToDictionaryAsync(p => p.PatternId, p => p.PatternName, ct);

        return correctPatternStats
            .Where(p => avoidedPatterns.Contains(p.PatternId))
            .Select(p => new AvoidedPatternInfo(
                p.PatternId,
                patternNames.GetValueOrDefault(p.PatternId, "Unknown"),
                p.TimesCorrectAnswer,
                p.TimesUserChoseIt))
            .OrderByDescending(p => p.TimesCorrectAnswer)
            .ToList();
    }

    public async Task<PatternUsageStatsResult> GetPatternUsageStatsAsync(
        string userId,
        CancellationToken ct = default)
    {
        var decaying = await GetDecayingPatternsAsync(userId, 30, ct);
        var defaults = await GetDefaultPatternsAsync(userId, 3, ct);
        var avoided = await GetAvoidedPatternsAsync(userId, ct);

        var totalAttempts = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId && a.Status == AttemptStatus.Solved)
            .CountAsync(ct);

        var uniquePatternsPracticed = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .Select(a => a.ChosenPatternId!.Value)
            .Distinct()
            .CountAsync(ct);

        // Note: totalPatterns is hardcoded to 16 since patterns are now in frontend JSON
        const int totalPatterns = 16;

        return new PatternUsageStatsResult(
            decaying,
            defaults,
            avoided,
            totalAttempts,
            uniquePatternsPracticed,
            totalPatterns);
    }

    public async Task<PatternNudge?> CheckForPatternNudgeAsync(
        string userId,
        Guid patternId,
        int consecutiveThreshold = 3,
        CancellationToken ct = default)
    {
        // Get recent attempts ordered by date
        var recentPatternIds = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.UserId == userId
                && a.Status == AttemptStatus.Solved
                && a.ChosenPatternId.HasValue)
            .OrderByDescending(a => a.CompletedAt ?? a.StartedAt)
            .Take(consecutiveThreshold)
            .Select(a => a.ChosenPatternId!.Value)
            .ToListAsync(ct);

        // Check if all recent attempts used the same pattern as the one being chosen
        if (recentPatternIds.Count < consecutiveThreshold)
            return null;

        if (!recentPatternIds.All(id => id == patternId))
            return null;

        // Get pattern name from the most recent attempt with this pattern ID
        var patternName = await _context.Attempts
            .AsNoTracking()
            .Where(a => a.ChosenPatternId == patternId)
            .Select(a => a.ChosenPatternName)
            .FirstOrDefaultAsync(ct) ?? "Unknown";

        return new PatternNudge(
            patternId,
            patternName,
            recentPatternIds.Count,
            $"You've chosen {patternName} for your last {recentPatternIds.Count} problems. Consider if another pattern might apply.");
    }
}
