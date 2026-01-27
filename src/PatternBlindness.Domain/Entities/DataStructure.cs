using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents a data structure (e.g., Array, Hash Table, Binary Tree).
/// </summary>
public class DataStructure : Entity
{
  private DataStructure() { }

  /// <summary>
  /// The name of the data structure (e.g., "Hash Table", "Binary Search Tree").
  /// </summary>
  public string Name { get; private set; } = string.Empty;

  /// <summary>
  /// Brief description/summary of the data structure.
  /// </summary>
  public string Description { get; private set; } = string.Empty;

  /// <summary>
  /// The category this data structure belongs to.
  /// </summary>
  public DataStructureCategory Category { get; private set; }

  /// <summary>
  /// Detailed explanation of what this data structure is and how it works.
  /// </summary>
  public string WhatItIs { get; private set; } = string.Empty;

  /// <summary>
  /// Operations supported by this data structure with their complexities.
  /// JSON array of {operation, timeComplexity, description} objects.
  /// </summary>
  public string Operations { get; private set; } = "[]";

  /// <summary>
  /// When to use this data structure - best use cases and scenarios.
  /// </summary>
  public string WhenToUse { get; private set; } = string.Empty;

  /// <summary>
  /// Tradeoffs and limitations of this data structure.
  /// </summary>
  public string Tradeoffs { get; private set; } = string.Empty;

  /// <summary>
  /// Common real-world use cases. JSON array.
  /// </summary>
  public string CommonUseCases { get; private set; } = "[]";

  /// <summary>
  /// Example implementation code.
  /// </summary>
  public string Implementation { get; private set; } = string.Empty;

  /// <summary>
  /// External resources for learning more about this data structure.
  /// JSON array of {title, url, type} objects.
  /// </summary>
  public string Resources { get; private set; } = "[]";

  /// <summary>
  /// IDs of related data structures.
  /// JSON array of GUIDs.
  /// </summary>
  public string RelatedStructureIds { get; private set; } = "[]";

  /// <summary>
  /// Creates a new data structure.
  /// </summary>
  public static DataStructure Create(
      string name,
      string description,
      DataStructureCategory category,
      string? whatItIs = null,
      string? operations = null,
      string? whenToUse = null,
      string? tradeoffs = null,
      string? commonUseCases = null,
      string? implementation = null,
      string? resources = null,
      string? relatedStructureIds = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Data structure name is required.", nameof(name));

    return new DataStructure
    {
      Id = Guid.NewGuid(),
      Name = name.Trim(),
      Description = description?.Trim() ?? string.Empty,
      Category = category,
      WhatItIs = whatItIs?.Trim() ?? string.Empty,
      Operations = operations ?? "[]",
      WhenToUse = whenToUse?.Trim() ?? string.Empty,
      Tradeoffs = tradeoffs?.Trim() ?? string.Empty,
      CommonUseCases = commonUseCases ?? "[]",
      Implementation = implementation ?? string.Empty,
      Resources = resources ?? "[]",
      RelatedStructureIds = relatedStructureIds ?? "[]"
    };
  }

  /// <summary>
  /// Updates the data structure details.
  /// </summary>
  public void Update(
      string name,
      string description,
      string? whatItIs = null,
      string? operations = null,
      string? whenToUse = null,
      string? tradeoffs = null,
      string? commonUseCases = null,
      string? implementation = null,
      string? resources = null,
      string? relatedStructureIds = null)
  {
    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Data structure name is required.", nameof(name));

    Name = name.Trim();
    Description = description?.Trim() ?? string.Empty;

    if (whatItIs is not null)
      WhatItIs = whatItIs.Trim();
    if (operations is not null)
      Operations = operations;
    if (whenToUse is not null)
      WhenToUse = whenToUse.Trim();
    if (tradeoffs is not null)
      Tradeoffs = tradeoffs.Trim();
    if (commonUseCases is not null)
      CommonUseCases = commonUseCases;
    if (implementation is not null)
      Implementation = implementation;
    if (resources is not null)
      Resources = resources;
    if (relatedStructureIds is not null)
      RelatedStructureIds = relatedStructureIds;

    UpdatedAt = DateTime.UtcNow;
  }
}
