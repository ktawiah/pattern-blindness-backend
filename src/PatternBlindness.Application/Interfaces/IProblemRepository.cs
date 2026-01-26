using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository interface for Problem entity operations.
/// </summary>
public interface IProblemRepository
{
  Task<Problem?> GetByIdAsync(Guid id, CancellationToken ct = default);
  Task<Problem?> GetByIdWithWrongApproachesAsync(Guid id, CancellationToken ct = default);
  Task<IReadOnlyList<Problem>> GetAllActiveAsync(CancellationToken ct = default);
  Task<IReadOnlyList<Problem>> GetByDifficultyAsync(Difficulty difficulty, CancellationToken ct = default);
  Task<IReadOnlyList<Problem>> GetByPatternAsync(Guid patternId, CancellationToken ct = default);
  Task<Problem?> GetRandomActiveAsync(Difficulty? difficulty = null, CancellationToken ct = default);
  Task<Problem> AddAsync(Problem problem, CancellationToken ct = default);
  Task UpdateAsync(Problem problem, CancellationToken ct = default);
  Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
  Task<int> GetActiveCountAsync(CancellationToken ct = default);
}
