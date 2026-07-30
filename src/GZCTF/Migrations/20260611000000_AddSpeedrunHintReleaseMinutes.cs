using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260611000000_AddSpeedrunHintReleaseMinutes")]
public partial class AddSpeedrunHintReleaseMinutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "SpeedrunHintReleaseMinutes",
            table: "GameChallenges",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SpeedrunHintReleaseMinutes",
            table: "GameChallenges");
    }
}
