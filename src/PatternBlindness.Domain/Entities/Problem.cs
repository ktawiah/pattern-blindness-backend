using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents a coding problem/interview question.
/// This is an aggregate root that owns WrongApproaches.
/// </summary>
public class Problem : Entity
{
  private readonly List<WrongApproach> _wrongApproaches = [];

  private Problem() { } // EF Core

  /// <summary>
  /// The title of the problem.
  /// </summary>
  public string Title { get; private set; } = string.Empty;

  /// <summary>
  /// The full problem description in Markdown format.
  /// </summary>
  public string Description { get; private set; } = string.Empty;

  /// <summary>
  /// Difficulty level of the problem.
  /// </summary>
  public Difficulty Difficulty { get; private set; }

  /// <summary>
  /// The correct pattern to solve this problem.
  /// </summary>
  public Guid CorrectPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the correct pattern.
  /// </summary>
  public Pattern? CorrectPattern { get; private set; }

  /// <summary>
  /// Key signals that should lead to the correct pattern.
  /// Stored as JSON array.
  /// </summary>
  public string Signals { get; private set; } = "[]";

  /// <summary>
  /// Constraints of the problem (e.g., "1 <= n <= 10^5").
  /// Stored as JSON array.
  /// </summary>
  public string Constraints { get; private set; } = "[]";

  /// <summary>
  /// Example inputs and outputs.
  /// Stored as JSON array.
  /// </summary>
  public string Examples { get; private set; } = "[]";

  /// <summary>
  /// The key invariant or insight needed to solve this problem.
  /// </summary>
  public string KeyInvariant { get; private set; } = string.Empty;

  /// <summary>
  /// Brief explanation of why the correct pattern works.
  /// </summary>
  public string SolutionExplanation { get; private set; } = string.Empty;

  /// <summary>
  /// Whether this problem is active and available to users.
  /// </summary>
  public bool IsActive { get; private set; } = true;

  /// <summary>
  /// Common wrong approaches for this problem.
  /// </summary>
  public IReadOnlyCollection<WrongApproach> WrongApproaches => _wrongApproaches.AsReadOnly();

  /// <summary>
  /// Attempts made on this problem.
  /// </summary>
  public ICollection<Attempt> Attempts { get; private set; } = [];

  /// <summary>
  /// Creates a new problem.
  /// </summary>
  public static Problem Create(
      string title,
      string description,
      Difficulty difficulty,
      Guid correctPatternId,
      string keyInvariant,
      string solutionExplanation,
      string? signals = null,
      string? constraints = null,
      string? examples = null)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentException("Problem title is required.", nameof(title));

    if (correctPatternId == Guid.Empty)
      throw new ArgumentException("Correct pattern ID is required.", nameof(correctPatternId));

    return new Problem
    {
      Id = Guid.NewGuid(),
      Title = title.Trim(),
      Description = description?.Trim() ?? string.Empty,
      Difficulty = difficulty,
      CorrectPatternId = correctPatternId,
      KeyInvariant = keyInvariant?.Trim() ?? string.Empty,
      SolutionExplanation = solutionExplanation?.Trim() ?? string.Empty,
      Signals = signals ?? "[]",
      Constraints = constraints ?? "[]",
      Examples = examples ?? "[]"
    };
  }

  /// <summary>
  /// Adds a wrong approach to this problem.
  /// </summary>
  public WrongApproach AddWrongApproach(
      Guid wrongPatternId,
      string explanation,
      int frequencyPercent)
  {
    var wrongApproach = WrongApproach.Create(Id, wrongPatternId, explanation, frequencyPercent);
    _wrongApproaches.Add(wrongApproach);
    UpdatedAt = DateTime.UtcNow;
    return wrongApproach;
  }

  /// <summary>
  /// Removes a wrong approach from this problem.
  /// </summary>
  public void RemoveWrongApproach(Guid wrongApproachId)
  {
    var wrongApproach = _wrongApproaches.FirstOrDefault(w => w.Id == wrongApproachId);
    if (wrongApproach is not null)
    {
      _wrongApproaches.Remove(wrongApproach);
      UpdatedAt = DateTime.UtcNow;
    }
  }

  /// <summary>
  /// Deactivates the problem.
  /// </summary>
  public void Deactivate()
  {
    IsActive = false;
    UpdatedAt = DateTime.UtcNow;
  }

  /// <summary>
  /// Activates the problem.
  /// </summary>
  public void Activate()
  {
    IsActive = true;
    UpdatedAt = DateTime.UtcNow;
  }
}
