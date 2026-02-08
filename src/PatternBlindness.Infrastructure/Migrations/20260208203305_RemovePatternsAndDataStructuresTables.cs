using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatternBlindness.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePatternsAndDataStructuresTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop all foreign key constraints first
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_Patterns_ChosenPatternId",
                table: "Attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_ChosenPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_RejectedPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_SecondaryPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Problems_Patterns_CorrectPatternId",
                table: "Problems");

            migrationBuilder.DropForeignKey(
                name: "FK_WrongApproaches_Patterns_WrongPatternId",
                table: "WrongApproaches");

            // Drop the tables - data now lives in frontend JSON files
            migrationBuilder.DropTable(name: "DataStructures");
            migrationBuilder.DropTable(name: "Patterns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attempts_Pattern_ChosenPatternId",
                table: "Attempts");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Pattern_ChosenPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Pattern_RejectedPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ColdStartSubmissions_Pattern_SecondaryPatternId",
                table: "ColdStartSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Problems_Pattern_CorrectPatternId",
                table: "Problems");

            migrationBuilder.DropForeignKey(
                name: "FK_WrongApproaches_Pattern_WrongPatternId",
                table: "WrongApproaches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pattern",
                table: "Pattern");

            migrationBuilder.RenameTable(
                name: "Pattern",
                newName: "Patterns");

            migrationBuilder.AlterColumn<string>(
                name: "TriggerSignals",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "TimeComplexity",
                table: "Patterns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SpaceComplexity",
                table: "Patterns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Resources",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "RelatedPatternIds",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Patterns",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Patterns",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommonUseCases",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CommonMistakes",
                table: "Patterns",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Patterns",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Patterns",
                table: "Patterns",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DataStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CommonUseCases = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Implementation = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Operations = table.Column<string>(type: "jsonb", nullable: false),
                    RelatedStructureIds = table.Column<string>(type: "jsonb", nullable: false),
                    Resources = table.Column<string>(type: "jsonb", nullable: false),
                    Tradeoffs = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WhatItIs = table.Column<string>(type: "text", nullable: false),
                    WhenToUse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataStructures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patterns_Category",
                table: "Patterns",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Patterns_Name",
                table: "Patterns",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataStructures_Category",
                table: "DataStructures",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DataStructures_Name",
                table: "DataStructures",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Attempts_Patterns_ChosenPatternId",
                table: "Attempts",
                column: "ChosenPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_ChosenPatternId",
                table: "ColdStartSubmissions",
                column: "ChosenPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_RejectedPatternId",
                table: "ColdStartSubmissions",
                column: "RejectedPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ColdStartSubmissions_Patterns_SecondaryPatternId",
                table: "ColdStartSubmissions",
                column: "SecondaryPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Problems_Patterns_CorrectPatternId",
                table: "Problems",
                column: "CorrectPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WrongApproaches_Patterns_WrongPatternId",
                table: "WrongApproaches",
                column: "WrongPatternId",
                principalTable: "Patterns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
