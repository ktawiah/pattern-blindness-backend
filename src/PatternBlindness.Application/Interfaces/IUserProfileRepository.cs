using PatternBlindness.Domain.Entities;

namespace PatternBlindness.Application.Interfaces;

/// <summary>
/// Repository interface for UserProfile entity operations.
/// </summary>
public interface IUserProfileRepository
{
    /// <summary>
    /// Gets a user profile by user ID.
    /// </summary>
    Task<UserProfile?> GetByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    Task<UserProfile> AddAsync(UserProfile profile, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user profile.
    /// </summary>
    Task UpdateAsync(UserProfile profile, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user profile exists for the given user ID.
    /// </summary>
    Task<bool> ExistsAsync(string userId, CancellationToken ct = default);
}
