using PatternBlindness.Domain.Common;

namespace PatternBlindness.Domain.Entities;

/// <summary>
/// Represents a common wrong approach for a problem.
/// These are patterns that seem logical but fail for specific reasons.
/// </summary>
public class WrongApproach : Entity
{
  private WrongApproach() { } // EF Core

  /// <summary>
  /// The problem this wrong approach belongs to.
  /// </summary>
  public Guid ProblemId { get; private set; }

  /// <summary>
  /// Navigation property for the problem.
  /// </summary>
  public Problem? Problem { get; private set; }

  /// <summary>
  /// The pattern that is incorrectly chosen.
  /// </summary>
  public Guid WrongPatternId { get; private set; }

  /// <summary>
  /// Navigation property for the wrong pattern.
  /// </summary>
  public Pattern? WrongPattern { get; private set; }

  /// <summary>
  /// Explanation of why this approach seems logical but fails.
  /// This is the "aha" moment content.
  /// </summary>
  public string Explanation { get; private set; } = string.Empty;

  /// <summary>
  /// What percentage of users typically choose this wrong approach.
  /// Used to prioritize showing the most common mistakes.
  /// </summary>
  public int FrequencyPercent { get; private set; }

  /// <summary>
  /// Creates a new wrong approach.
  /// </summary>
  public static WrongApproach Create(
      Guid problemId,
      Guid wrongPatternId,
      string explanation,
      int frequencyPercent)
  {
    if (problemId == Guid.Empty)
      throw new ArgumentException("Problem ID is required.", nameof(problemId));

    if (wrongPatternId == Guid.Empty)
      throw new ArgumentException("Wrong pattern ID is required.", nameof(wrongPatternId));

    if (frequencyPercent is < 0 or > 100)
      throw new ArgumentOutOfRangeException(nameof(frequencyPercent), "Frequency must be between 0 and 100.");

    return new WrongApproach
    {
      Id = Guid.NewGuid(),
      ProblemId = problemId,
      WrongPatternId = wrongPatternId,
      Explanation = explanation?.Trim() ?? string.Empty,
      FrequencyPercent = frequencyPercent
    };
  }

  /// <summary>
  /// Updates the wrong approach details.
  /// </summary>
  public void Update(string explanation, int frequencyPercent)
  {
    if (frequencyPercent is < 0 or > 100)
      throw new ArgumentOutOfRangeException(nameof(frequencyPercent), "Frequency must be between 0 and 100.");

    Explanation = explanation?.Trim() ?? string.Empty;
    FrequencyPercent = frequencyPercent;
    UpdatedAt = DateTime.UtcNow;
  }
}
