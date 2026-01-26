using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class ProblemRepository : IProblemRepository
{
  private readonly ApplicationDbContext _context;

  public ProblemRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Problem?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Problems
        .AsNoTracking()
        .Include(p => p.CorrectPattern)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<Problem?> GetByIdWithWrongApproachesAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Problems
        .AsNoTracking()
        .Include(p => p.CorrectPattern)
        .Include(p => p.WrongApproaches)
            .ThenInclude(w => w.WrongPattern)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<IReadOnlyList<Problem>> GetAllActiveAsync(CancellationToken ct = default)
  {
    return await _context.Problems
        .AsNoTracking()
        .Where(p => p.IsActive)
        .Include(p => p.CorrectPattern)
        .OrderBy(p => p.Difficulty)
        .ThenBy(p => p.Title)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Problem>> GetByDifficultyAsync(Difficulty difficulty, CancellationToken ct = default)
  {
    return await _context.Problems
        .AsNoTracking()
        .Where(p => p.IsActive && p.Difficulty == difficulty)
        .Include(p => p.CorrectPattern)
        .OrderBy(p => p.Title)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Problem>> GetByPatternAsync(Guid patternId, CancellationToken ct = default)
  {
    return await _context.Problems
        .AsNoTracking()
        .Where(p => p.IsActive && p.CorrectPatternId == patternId)
        .Include(p => p.CorrectPattern)
        .OrderBy(p => p.Difficulty)
        .ThenBy(p => p.Title)
        .ToListAsync(ct);
  }

  public async Task<Problem?> GetRandomActiveAsync(Difficulty? difficulty = null, CancellationToken ct = default)
  {
    var query = _context.Problems
        .AsNoTracking()
        .Where(p => p.IsActive);

    if (difficulty.HasValue)
    {
      query = query.Where(p => p.Difficulty == difficulty.Value);
    }

    var count = await query.CountAsync(ct);
    if (count == 0)
      return null;

    var skip = Random.Shared.Next(count);
    return await query
        .Include(p => p.CorrectPattern)
        .Skip(skip)
        .FirstOrDefaultAsync(ct);
  }

  public async Task<Problem> AddAsync(Problem problem, CancellationToken ct = default)
  {
    await _context.Problems.AddAsync(problem, ct);
    await _context.SaveChangesAsync(ct);
    return problem;
  }

  public async Task UpdateAsync(Problem problem, CancellationToken ct = default)
  {
    _context.Problems.Update(problem);
    await _context.SaveChangesAsync(ct);
  }

  public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Problems.AnyAsync(p => p.Id == id, ct);
  }

  public async Task<int> GetActiveCountAsync(CancellationToken ct = default)
  {
    return await _context.Problems.CountAsync(p => p.IsActive, ct);
  }
}
