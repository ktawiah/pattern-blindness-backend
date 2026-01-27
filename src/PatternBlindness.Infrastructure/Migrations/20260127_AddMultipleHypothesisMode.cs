using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
  /// <inheritdoc />
  public partial class AddMultipleHypothesisMode : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Add SecondaryPatternId column
      migrationBuilder.AddColumn<Guid>(
          name: "SecondaryPatternId",
          table: "ColdStartSubmissions",
          type: "uuid",
          nullable: true);

      // Add PrimaryVsSecondaryReason column
      migrationBuilder.AddColumn<string>(
          name: "PrimaryVsSecondaryReason",
          table: "ColdStartSubmissions",
          type: "character varying(500)",
          maxLength: 500,
          nullable: true);

      // Create index for SecondaryPatternId
      migrationBuilder.CreateIndex(
          name: "IX_ColdStartSubmissions_SecondaryPatternId",
          table: "ColdStartSubmissions",
          column: "SecondaryPatternId");

      // Add foreign key for SecondaryPatternId
      migrationBuilder.AddForeignKey(
          name: "FK_ColdStartSubmissions_Patterns_SecondaryPatternId",
          table: "ColdStartSubmissions",
          column: "SecondaryPatternId",
          principalTable: "Patterns",
          principalColumn: "Id",
          onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      // Remove foreign key
      migrationBuilder.DropForeignKey(
          name: "FK_ColdStartSubmissions_Patterns_SecondaryPatternId",
          table: "ColdStartSubmissions");

      // Remove index
      migrationBuilder.DropIndex(
          name: "IX_ColdStartSubmissions_SecondaryPatternId",
          table: "ColdStartSubmissions");

      // Remove columns
      migrationBuilder.DropColumn(
          name: "SecondaryPatternId",
          table: "ColdStartSubmissions");

      migrationBuilder.DropColumn(
          name: "PrimaryVsSecondaryReason",
          table: "ColdStartSubmissions");
    }
  }
}
