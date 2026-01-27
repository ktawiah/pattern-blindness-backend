using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class LeetCodeProblemCacheRepository : ILeetCodeProblemCacheRepository
{
  private readonly ApplicationDbContext _context;

  public LeetCodeProblemCacheRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<LeetCodeProblemCache?> GetBySlugAsync(string titleSlug, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .FirstOrDefaultAsync(p => p.TitleSlug == titleSlug, ct);
  }

  public async Task<LeetCodeProblemCache?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<LeetCodeProblemCache?> GetByLeetCodeIdAsync(string leetCodeId, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .FirstOrDefaultAsync(p => p.LeetCodeId == leetCodeId, ct);
  }

  public async Task<LeetCodeProblemCache?> GetWithAnalysisAsync(string titleSlug, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .Include(p => p.Analysis)
        .FirstOrDefaultAsync(p => p.TitleSlug == titleSlug, ct);
  }

  public async Task<LeetCodeProblemCache?> GetWithAnalysisAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .Include(p => p.Analysis)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<LeetCodeProblemCache> AddAsync(LeetCodeProblemCache problem, CancellationToken ct = default)
  {
    _context.LeetCodeProblemCache.Add(problem);
    await _context.SaveChangesAsync(ct);
    return problem;
  }

  public async Task UpdateAsync(LeetCodeProblemCache problem, CancellationToken ct = default)
  {
    _context.LeetCodeProblemCache.Update(problem);
    await _context.SaveChangesAsync(ct);
  }

  public async Task<IReadOnlyList<LeetCodeProblemCache>> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
  {
    var normalizedQuery = query.ToLower().Trim();

    return await _context.LeetCodeProblemCache
        .Where(p => p.Title.ToLower().Contains(normalizedQuery) ||
                    p.TitleSlug.ToLower().Contains(normalizedQuery) ||
                    p.FrontendId == normalizedQuery)
        .OrderBy(p => p.FrontendId)
        .Take(limit)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<LeetCodeProblemCache>> GetProblemsNeedingAnalysisAsync(int limit = 10, CancellationToken ct = default)
  {
    return await _context.LeetCodeProblemCache
        .Where(p => p.Analysis == null)
        .OrderBy(p => p.CachedAt)
        .Take(limit)
        .ToListAsync(ct);
  }
}
