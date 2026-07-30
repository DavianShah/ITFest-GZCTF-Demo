using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260729000000_AddSubmissionAiUsageDisclosure")]
public partial class AddSubmissionAiUsageDisclosure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AiUsageDisclosure",
            table: "Submissions",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AiUsageDisclosure",
            table: "Submissions");
    }
}
