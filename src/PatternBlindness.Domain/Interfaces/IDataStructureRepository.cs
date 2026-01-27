using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Domain.Interfaces;

/// <summary>
/// Repository interface for data structure operations.
/// </summary>
public interface IDataStructureRepository
{
  /// <summary>
  /// Gets all data structures.
  /// </summary>
  Task<IReadOnlyList<DataStructure>> GetAllAsync(CancellationToken ct = default);

  /// <summary>
  /// Gets a data structure by ID.
  /// </summary>
  Task<DataStructure?> GetByIdAsync(Guid id, CancellationToken ct = default);

  /// <summary>
  /// Gets a data structure by name.
  /// </summary>
  Task<DataStructure?> GetByNameAsync(string name, CancellationToken ct = default);

  /// <summary>
  /// Gets all data structures in a category.
  /// </summary>
  Task<IReadOnlyList<DataStructure>> GetByCategoryAsync(DataStructureCategory category, CancellationToken ct = default);

  /// <summary>
  /// Searches data structures by name or description.
  /// </summary>
  Task<IReadOnlyList<DataStructure>> SearchAsync(string query, CancellationToken ct = default);

  /// <summary>
  /// Adds a new data structure.
  /// </summary>
  Task<DataStructure> AddAsync(DataStructure dataStructure, CancellationToken ct = default);

  /// <summary>
  /// Updates an existing data structure.
  /// </summary>
  Task UpdateAsync(DataStructure dataStructure, CancellationToken ct = default);

  /// <summary>
  /// Deletes a data structure.
  /// </summary>
  Task DeleteAsync(Guid id, CancellationToken ct = default);

  /// <summary>
  /// Checks if a data structure exists by name.
  /// </summary>
  Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
}
