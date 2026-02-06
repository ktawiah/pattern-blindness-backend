using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeetCodeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, add missing columns to Attempts table if they don't exist
            migrationBuilder.AddColumn<Guid>(
                name: "LeetCodeProblemCacheId",
                table: "Attempts",
                type: "uuid",
                nullable: true);


            migrationBuilder.AddColumn<Guid>(
                name: "ChosenPatternId",
                table: "Attempts",
                type: "uuid",
                nullable: true);

            // Create LeetCodeProblemCache table
            migrationBuilder.CreateTable(
                name: "LeetCodeProblemCache",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeetCodeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FrontendId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TitleSlug = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    Examples = table.Column<string>(type: "jsonb", nullable: false),
                    Hints = table.Column<string>(type: "jsonb", nullable: false),
                    AcceptanceRate = table.Column<double>(type: "double precision", nullable: false),
                    CachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRefreshedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeetCodeProblemCache", x => x.Id);
                });

            // Create ProblemAnalyses table
            migrationBuilder.CreateTable(
                name: "ProblemAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeetCodeProblemCacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryPatterns = table.Column<string>(type: "jsonb", nullable: false),
                    SecondaryPatterns = table.Column<string>(type: "jsonb", nullable: false),
                    KeySignals = table.Column<string>(type: "jsonb", nullable: false),
                    CommonMistakes = table.Column<string>(type: "jsonb", nullable: false),
                    TimeComplexity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SpaceComplexity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    KeyInsight = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApproachExplanation = table.Column<string>(type: "text", nullable: true),
                    SimilarProblems = table.Column<string>(type: "jsonb", nullable: false),
                    ModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawLlmResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProblemAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProblemAnalyses_LeetCodeProblemCache_LeetCodeProblemCacheId",
                        column: x => x.LeetCodeProblemCacheId,
                        principalTable: "LeetCodeProblemCache",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Reflections table
            migrationBuilder.CreateTable(
                name: "Reflections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserColdStartSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WasPatternCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    CorrectIdentifications = table.Column<string>(type: "jsonb", nullable: false),
                    MissedSignals = table.Column<string>(type: "jsonb", nullable: false),
                    NextTimeAdvice = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatternTips = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConfidenceCalibration = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RawLlmResponse = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reflections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reflections_Attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add LeetCodeProblemCacheId column to Attempts table if it doesn't exist
            migrationBuilder.AddColumn<Guid>(
                name: "LeetCodeProblemCacheId",
                table: "Attempts",
                type: "uuid",
                nullable: true);

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_LeetCodeProblemCache_LeetCodeId",
                table: "LeetCodeProblemCache",
                column: "LeetCodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeetCodeProblemCache_TitleSlug",
                table: "LeetCodeProblemCache",
                column: "TitleSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeetCodeProblemCache_FrontendId",
                table: "LeetCodeProblemCache",
                column: "FrontendId");

            migrationBuilder.CreateIndex(
                name: "IX_ProblemAnalyses_LeetCodeProblemCacheId",
                table: "ProblemAnalyses",
                column: "LeetCodeProblemCacheId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reflections_AttemptId",
                table: "Reflections",
                column: "AttemptId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attempts_LeetCodeProblemCacheId",
                table: "Attempts",
                column: "LeetCodeProblemCacheId");

            // Add foreign key for LeetCodeProblemCacheId in Attempts
            migrationBuilder.AddForeignKey(
                name: "FK_Attempts_LeetCodeProblemCache_LeetCodeProblemCacheId",
                table: "Attempts",
                column: "LeetCodeProblemCacheId",
                principalTable: "LeetCodeProblemCache",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_LeetCodeProblemCache_LeetCodeProblemCacheId",
                table: "Attempts");

            migrationBuilder.DropTable(
                name: "ProblemAnalyses");

            migrationBuilder.DropTable(
                name: "Reflections");

            migrationBuilder.DropTable(
                name: "LeetCodeProblemCache");

            migrationBuilder.DropIndex(
                name: "IX_Attempts_LeetCodeProblemCacheId",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "LeetCodeProblemCacheId",
                table: "Attempts");
        }
    }
}
