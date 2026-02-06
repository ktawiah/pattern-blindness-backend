using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to add columns that may or may not exist, with error handling
            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" ADD COLUMN IF NOT EXISTS ""ChosenPatternName"" character varying(100);",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" ADD COLUMN IF NOT EXISTS ""ChosenPatternId"" uuid;",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" ADD COLUMN IF NOT EXISTS ""ProblemId"" uuid;",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""ColdStartSubmissions"" ADD COLUMN IF NOT EXISTS ""IdentifiedSignals"" jsonb DEFAULT '[]';",
                suppressTransaction: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" DROP COLUMN IF EXISTS ""ChosenPatternName"";",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" DROP COLUMN IF EXISTS ""ChosenPatternId"";",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""Attempts"" DROP COLUMN IF EXISTS ""ProblemId"";",
                suppressTransaction: false);

            migrationBuilder.Sql(
                @"ALTER TABLE ""ColdStartSubmissions"" DROP COLUMN IF EXISTS ""IdentifiedSignals"";",
                suppressTransaction: false);
        }
    }
}
