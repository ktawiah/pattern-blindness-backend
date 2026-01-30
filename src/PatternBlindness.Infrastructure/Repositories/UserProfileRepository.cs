using Microsoft.EntityFrameworkCore;
using PatternBlindness.Application.Interfaces;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Infrastructure.Data;

namespace PatternBlindness.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly ApplicationDbContext _context;

    public UserProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfile?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId, ct);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile, CancellationToken ct = default)
    {
        await _context.UserProfiles.AddAsync(profile, ct);
        await _context.SaveChangesAsync(ct);
        return profile;
    }

    public async Task UpdateAsync(UserProfile profile, CancellationToken ct = default)
    {
        _context.UserProfiles.Update(profile);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserProfiles
            .AnyAsync(up => up.UserId == userId, ct);
    }
}
