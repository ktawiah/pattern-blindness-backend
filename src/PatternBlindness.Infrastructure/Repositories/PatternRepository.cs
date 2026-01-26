using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class PatternRepository : IPatternRepository
{
  private readonly ApplicationDbContext _context;

  public PatternRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Pattern?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Patterns
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == id, ct);
  }

  public async Task<Pattern?> GetByNameAsync(string name, CancellationToken ct = default)
  {
    return await _context.Patterns
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Name == name, ct);
  }

  public async Task<IReadOnlyList<Pattern>> GetAllAsync(CancellationToken ct = default)
  {
    return await _context.Patterns
        .AsNoTracking()
        .OrderBy(p => p.Category)
        .ThenBy(p => p.Name)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<Pattern>> GetByCategoryAsync(PatternCategory category, CancellationToken ct = default)
  {
    return await _context.Patterns
        .AsNoTracking()
        .Where(p => p.Category == category)
        .OrderBy(p => p.Name)
        .ToListAsync(ct);
  }

  public async Task<Pattern> AddAsync(Pattern pattern, CancellationToken ct = default)
  {
    await _context.Patterns.AddAsync(pattern, ct);
    await _context.SaveChangesAsync(ct);
    return pattern;
  }

  public async Task UpdateAsync(Pattern pattern, CancellationToken ct = default)
  {
    _context.Patterns.Update(pattern);
    await _context.SaveChangesAsync(ct);
  }

  public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.Patterns.AnyAsync(p => p.Id == id, ct);
  }
}
