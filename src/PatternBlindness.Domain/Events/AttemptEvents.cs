using PatternBlindness.Domain.Common;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Domain.Events;

/// <summary>
/// Event raised when a new attempt is started.
/// </summary>
public record AttemptStartedEvent(Guid AttemptId, string UserId, Guid ProblemId) : DomainEvent;

/// <summary>
/// Event raised when the cold start phase is completed.
/// </summary>
public record ColdStartCompletedEvent(Guid AttemptId, Guid ChosenPatternId, ConfidenceLevel Confidence) : DomainEvent;

/// <summary>
/// Event raised when an attempt is completed.
/// </summary>
public record AttemptCompletedEvent(Guid AttemptId, string UserId, bool IsPatternCorrect, ConfidenceLevel Confidence) : DomainEvent;
