using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataStructuresAndEnhancedPatterns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommonUseCases",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PseudoCode",
                table: "Patterns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RelatedPatternIds",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Resources",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpaceComplexity",
                table: "Patterns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeComplexity",
                table: "Patterns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatItIs",
                table: "Patterns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhenToUse",
                table: "Patterns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhyItWorks",
                table: "Patterns",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Note: ColdStartSubmissions.IdentifiedSignals, Attempts.ProblemId, 
            // Attempts.ChosenPatternName, Attempts.LeetCodeProblemCacheId already exist in DB

            migrationBuilder.CreateTable(
                name: "DataStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WhatItIs = table.Column<string>(type: "text", nullable: false),
                    Operations = table.Column<string>(type: "jsonb", nullable: false),
                    WhenToUse = table.Column<string>(type: "text", nullable: false),
                    Tradeoffs = table.Column<string>(type: "text", nullable: false),
                    CommonUseCases = table.Column<string>(type: "jsonb", nullable: false),
                    Implementation = table.Column<string>(type: "text", nullable: false),
                    Resources = table.Column<string>(type: "jsonb", nullable: false),
                    RelatedStructureIds = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStructures", x => x.Id);
                });

            // Note: LeetCodeProblemCache, Reflections, ProblemAnalyses tables already exist in DB
            // Note: Indexes on these tables and the FK from Attempts already exist

            migrationBuilder.CreateIndex(
                name: "IX_DataStructures_Category",
                table: "DataStructures",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DataStructures_Name",
                table: "DataStructures",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataStructures");

            migrationBuilder.DropColumn(
                name: "CommonUseCases",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "PseudoCode",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "RelatedPatternIds",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "Resources",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "SpaceComplexity",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "TimeComplexity",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "WhatItIs",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "WhenToUse",
                table: "Patterns");

            migrationBuilder.DropColumn(
                name: "WhyItWorks",
                table: "Patterns");
        }
    }
}
