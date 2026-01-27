using Microsoft.EntityFrameworkCore;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;
using PatternBlindness.Domain.Interfaces;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for data structure operations.
/// </summary>
public class DataStructureRepository : IDataStructureRepository
{
  private readonly ApplicationDbContext _context;

  public DataStructureRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<IReadOnlyList<DataStructure>> GetAllAsync(CancellationToken ct = default)
  {
    return await _context.DataStructures
        .OrderBy(ds => ds.Category)
        .ThenBy(ds => ds.Name)
        .ToListAsync(ct);
  }

  public async Task<DataStructure?> GetByIdAsync(Guid id, CancellationToken ct = default)
  {
    return await _context.DataStructures.FindAsync([id], ct);
  }

  public async Task<DataStructure?> GetByNameAsync(string name, CancellationToken ct = default)
  {
    return await _context.DataStructures
        .FirstOrDefaultAsync(ds => ds.Name == name, ct);
  }

  public async Task<IReadOnlyList<DataStructure>> GetByCategoryAsync(DataStructureCategory category, CancellationToken ct = default)
  {
    return await _context.DataStructures
        .Where(ds => ds.Category == category)
        .OrderBy(ds => ds.Name)
        .ToListAsync(ct);
  }

  public async Task<IReadOnlyList<DataStructure>> SearchAsync(string query, CancellationToken ct = default)
  {
    var normalizedQuery = query.ToLowerInvariant();

    return await _context.DataStructures
        .Where(ds => ds.Name.ToLower().Contains(normalizedQuery) ||
                     ds.Description.ToLower().Contains(normalizedQuery))
        .OrderBy(ds => ds.Name)
        .ToListAsync(ct);
  }

  public async Task<DataStructure> AddAsync(DataStructure dataStructure, CancellationToken ct = default)
  {
    _context.DataStructures.Add(dataStructure);
    await _context.SaveChangesAsync(ct);
    return dataStructure;
  }

  public async Task UpdateAsync(DataStructure dataStructure, CancellationToken ct = default)
  {
    _context.DataStructures.Update(dataStructure);
    await _context.SaveChangesAsync(ct);
  }

  public async Task DeleteAsync(Guid id, CancellationToken ct = default)
  {
    var dataStructure = await GetByIdAsync(id, ct);
    if (dataStructure is not null)
    {
      _context.DataStructures.Remove(dataStructure);
      await _context.SaveChangesAsync(ct);
    }
  }

  public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
  {
    return await _context.DataStructures.AnyAsync(ds => ds.Name == name, ct);
  }
}
