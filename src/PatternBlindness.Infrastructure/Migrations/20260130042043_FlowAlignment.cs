using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FlowAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyInvariant",
                table: "ColdStartSubmissions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryRisk",
                table: "ColdStartSubmissions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstFailure",
                table: "Attempts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "Attempts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwitchReason",
                table: "Attempts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SwitchedApproachMidSolve",
                table: "Attempts",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DsaProblemsCompleted = table.Column<int>(type: "integer", nullable: false),
                    IsQualified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    QualifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentPhase = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CompletedAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    WasGrandfathered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    InterviewReadinessOptIn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IsQualified",
                table: "UserProfiles",
                column: "IsQualified");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "KeyInvariant",
                table: "ColdStartSubmissions");

            migrationBuilder.DropColumn(
                name: "PrimaryRisk",
                table: "ColdStartSubmissions");

            migrationBuilder.DropColumn(
                name: "FirstFailure",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "SwitchReason",
                table: "Attempts");

            migrationBuilder.DropColumn(
                name: "SwitchedApproachMidSolve",
                table: "Attempts");
        }
    }
}
