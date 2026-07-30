using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260729010000_AddSubmissionSolverUpload")]
public partial class AddSubmissionSolverUpload : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "RequireSolverUpload",
            table: "GameChallenges",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "SolverFileId",
            table: "Submissions",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SolverFileName",
            table: "Submissions",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Submissions_SolverFileId",
            table: "Submissions",
            column: "SolverFileId");

        migrationBuilder.AddForeignKey(
            name: "FK_Submissions_Files_SolverFileId",
            table: "Submissions",
            column: "SolverFileId",
            principalTable: "Files",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Submissions_Files_SolverFileId",
            table: "Submissions");

        migrationBuilder.DropIndex(
            name: "IX_Submissions_SolverFileId",
            table: "Submissions");

        migrationBuilder.DropColumn(
            name: "RequireSolverUpload",
            table: "GameChallenges");

        migrationBuilder.DropColumn(
            name: "SolverFileId",
            table: "Submissions");

        migrationBuilder.DropColumn(
            name: "SolverFileName",
            table: "Submissions");
    }
}
