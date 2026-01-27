using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class AnalysisRepository : IAnalysisRepository
{
  private readonly ApplicationDbContext _context;

  public AnalysisRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<ProblemAnalysis?> GetAnalysisByProblemIdAsync(Guid leetCodeProblemCacheId, CancellationToken ct = default)
  {
    return await _context.ProblemAnalyses
        .FirstOrDefaultAsync(a => a.LeetCodeProblemCacheId == leetCodeProblemCacheId, ct);
  }

  public async Task<ProblemAnalysis> AddAnalysisAsync(ProblemAnalysis analysis, CancellationToken ct = default)
  {
    _context.ProblemAnalyses.Add(analysis);
    await _context.SaveChangesAsync(ct);
    return analysis;
  }

  public async Task<Reflection?> GetReflectionByAttemptIdAsync(Guid attemptId, CancellationToken ct = default)
  {
    return await _context.Reflections
        .FirstOrDefaultAsync(r => r.AttemptId == attemptId, ct);
  }

  public async Task<Reflection> AddReflectionAsync(Reflection reflection, CancellationToken ct = default)
  {
    _context.Reflections.Add(reflection);
    await _context.SaveChangesAsync(ct);
    return reflection;
  }
}
