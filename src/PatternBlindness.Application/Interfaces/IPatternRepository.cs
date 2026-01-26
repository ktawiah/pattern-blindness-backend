using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository interface for Pattern entity operations.
/// </summary>
public interface IPatternRepository
{
  Task<Pattern?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<Pattern?> GetByNameAsync(string name, CancellationToken ct = default);
  Task<IReadOnlyList<Pattern>> GetAllAsync(CancellationToken ct = default);
  Task<IReadOnlyList<Pattern>> GetByCategoryAsync(PatternCategory category, CancellationToken ct = default);
  Task<Pattern> AddAsync(Pattern pattern, CancellationToken ct = default);
  Task UpdateAsync(Pattern pattern, CancellationToken ct = default);
  Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
