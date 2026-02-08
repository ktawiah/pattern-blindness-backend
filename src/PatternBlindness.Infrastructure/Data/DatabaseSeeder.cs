namespace PatternBlindness.Infrastructure.Data;

/// <summary>
/// Database seeder for initial data.
/// Note: Patterns and DataStructures are now loaded from JSON files in the frontend
/// (pattern-blindness-frontend/src/data/patterns.json and data-structures.json)
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Add other seeding as needed (user roles, etc.)
        await Task.CompletedTask;
    }
}
