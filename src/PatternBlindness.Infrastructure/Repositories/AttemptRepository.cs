using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class AttemptRepository : IAttemptRepository
{
  private readonly ApplicationDbContext _context;

  public AttemptRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Attempt?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Attempts
        .Include(a => a.Problem)
        .Include(a => a.ChosenPattern)
        .FirstOrDefaultAsync(a => a.Id == id, ct);
  }

  public async Task<Attempt?> GetByIdWithColdStartAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Attempts
        .Include(a => a.Problem)
        .Include(a => a.ChosenPattern)
        .Include(a => a.ColdStartSubmission)
            .ThenInclude(c => c!.ChosenPattern)
        .Include(a => a.ColdStartSubmission)
            .ThenInclude(c => c!.RejectedPattern)
        .FirstOrDefaultAsync(a => a.Id == id, ct);
  }

  public async Task<Attempt?> GetByIdWithReflectionAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Attempts
        .Include(a => a.Problem)
        .Include(a => a.LeetCodeProblem)
        .Include(a => a.Reflection)
        .FirstOrDefaultAsync(a => a.Id == id, ct);
  }

  public async Task<IReadOnlyList<Attempt>> GetByUserIdAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId)
        .Include(a => a.Problem)
        .Include(a => a.LeetCodeProblem)
        .Include(a => a.ChosenPattern)
        .OrderByDescending(a => a.StartedAt)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Attempt>> GetByUserIdAndStatusAsync(string userId, AttemptStatus status, CancellationToken ct = default)
  {
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId && a.Status == status)
        .Include(a => a.Problem)
        .Include(a => a.ChosenPattern)
        .OrderByDescending(a => a.StartedAt)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Attempt>> GetRecentByUserIdAsync(string userId, int count = 10, CancellationToken ct = default)
  {
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId)
        .Include(a => a.Problem)
        .Include(a => a.ChosenPattern)
        .OrderByDescending(a => a.StartedAt)
        .Take(count)
        .ToListAsync(ct);
  }

  public async Task<Attempt> AddAsync(Attempt attempt, CancellationToken ct = default)
  {
    await _context.Attempts.AddAsync(attempt, ct);
    await _context.SaveChangesAsync(ct);
    return attempt;
  }

  public async Task UpdateAsync(Attempt attempt, CancellationToken ct = default)
  {
    _context.Attempts.Update(attempt);
    await _context.SaveChangesAsync(ct);
  }

  public async Task<Attempt?> GetActiveAttemptByUserIdAsync(string userId, CancellationToken ct = default)
  {
    // An "active" attempt is one that is InProgress or ColdStartCompleted (not yet solved/abandoned)
    return await _context.Attempts
        .Include(a => a.Problem)
        .Include(a => a.LeetCodeProblem)
        .FirstOrDefaultAsync(a =>
            a.UserId == userId &&
            (a.Status == AttemptStatus.InProgress || a.Status == AttemptStatus.ColdStartCompleted),
            ct);
  }

  public async Task<int> GetCompletedAttemptCountAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Attempts
        .CountAsync(a => a.UserId == userId && a.Status == AttemptStatus.Solved, ct);
  }

  public async Task<IReadOnlyList<ConfidenceStats>> GetConfidenceStatsAsync(string userId, CancellationToken ct = default)
  {
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId && a.Status == AttemptStatus.Solved)
        .GroupBy(a => a.Confidence)
        .Select(g => new ConfidenceStats(
            g.Key,
            g.Count(),
            g.Count(a => a.IsPatternCorrect),
            g.Count(a => !a.IsPatternCorrect)))
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<PatternWeakness>> GetOverconfidentPatternsAsync(string userId, int limit = 2, CancellationToken ct = default)
  {
    // High confidence (Confident or VeryConfident) + wrong pattern = overconfidence
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId
            && a.Status == AttemptStatus.Solved
            && a.ChosenPatternId.HasValue
            && (a.Confidence == ConfidenceLevel.Confident || a.Confidence == ConfidenceLevel.VeryConfident))
        .GroupBy(a => new { a.ChosenPatternId, a.ChosenPattern!.Name })
        .Select(g => new
        {
          PatternId = g.Key.ChosenPatternId!.Value,
          PatternName = g.Key.Name,
          TotalAttempts = g.Count(),
          WrongCount = g.Count(a => !a.IsPatternCorrect)
        })
        .Where(x => x.WrongCount > 0 && x.TotalAttempts >= 2)
        .OrderByDescending(x => (double)x.WrongCount / x.TotalAttempts)
        .Take(limit)
        .Select(x => new PatternWeakness(
            x.PatternId,
            x.PatternName,
            x.TotalAttempts,
            x.WrongCount,
            Math.Round((double)x.WrongCount / x.TotalAttempts * 100, 1)))
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<PatternWeakness>> GetFragilePatternsAsync(string userId, int limit = 2, CancellationToken ct = default)
  {
    // Low confidence (Guessing or Uncertain) + right pattern = fragile understanding
    return await _context.Attempts
        .AsNoTracking()
        .Where(a => a.UserId == userId
            && a.Status == AttemptStatus.Solved
            && a.ChosenPatternId.HasValue
            && a.IsPatternCorrect
            && (a.Confidence == ConfidenceLevel.Guessing || a.Confidence == ConfidenceLevel.Uncertain))
        .GroupBy(a => new { a.ChosenPatternId, a.ChosenPattern!.Name })
        .Select(g => new
        {
          PatternId = g.Key.ChosenPatternId!.Value,
          PatternName = g.Key.Name,
          TotalAttempts = g.Count()
        })
        .Where(x => x.TotalAttempts >= 2)
        .OrderByDescending(x => x.TotalAttempts)
        .Take(limit)
        .Select(x => new PatternWeakness(
            x.PatternId,
            x.PatternName,
            x.TotalAttempts,
            x.TotalAttempts, // All are "fragile" since they got it right but weren't confident
            100.0))
        .ToListAsync(ct);
  }
}
