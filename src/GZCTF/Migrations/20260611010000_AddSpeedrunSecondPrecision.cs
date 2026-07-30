using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260611010000_AddSpeedrunSecondPrecision")]
public partial class AddSpeedrunSecondPrecision : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("SpeedrunDefaultRoundDurationSeconds", "Games", "integer",
            nullable: false, defaultValue: 1800);
        migrationBuilder.AddColumn<int>("SpeedrunOvertimeSeconds", "Games", "integer",
            nullable: false, defaultValue: 300);
        migrationBuilder.AddColumn<int>("DurationSeconds", "SpeedrunRounds", "integer",
            nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("OvertimeSeconds", "SpeedrunRounds", "integer",
            nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<int>("ManuallyExtendedSeconds", "SpeedrunRounds", "integer",
            nullable: false, defaultValue: 0);
        migrationBuilder.AddColumn<string>("SpeedrunHintReleaseSeconds", "GameChallenges", "text", nullable: true);

        migrationBuilder.Sql("""
            UPDATE "Games"
            SET "SpeedrunDefaultRoundDurationSeconds" = "SpeedrunDefaultRoundDurationMinutes" * 60,
                "SpeedrunOvertimeSeconds" = "SpeedrunOvertimeMinutes" * 60;

            UPDATE "SpeedrunRounds"
            SET "DurationSeconds" = "DurationMinutes" * 60,
                "OvertimeSeconds" = "OvertimeMinutes" * 60,
                "ManuallyExtendedSeconds" = "ManuallyExtendedMinutes" * 60;

            UPDATE "GameChallenges"
            SET "SpeedrunHintReleaseSeconds" = (
                SELECT jsonb_agg((entry.value::integer) * 60 ORDER BY entry.ordinality)::text
                FROM jsonb_array_elements_text("GameChallenges"."SpeedrunHintReleaseMinutes"::jsonb)
                    WITH ORDINALITY AS entry(value, ordinality)
            )
            WHERE "SpeedrunHintReleaseMinutes" IS NOT NULL;
            """);

        migrationBuilder.CreateTable(
            name: "SpeedrunHintReleaseLogs",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GameId = table.Column<int>(type: "integer", nullable: false),
                RoundId = table.Column<int>(type: "integer", nullable: false),
                ChallengeId = table.Column<int>(type: "integer", nullable: false),
                HintIndex = table.Column<int>(type: "integer", nullable: false),
                ReleasedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SpeedrunHintReleaseLogs", x => x.Id);
                table.ForeignKey("FK_SpeedrunHintReleaseLogs_GameChallenges_ChallengeId", x => x.ChallengeId,
                    "GameChallenges", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_SpeedrunHintReleaseLogs_Games_GameId", x => x.GameId,
                    "Games", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_SpeedrunHintReleaseLogs_SpeedrunRounds_RoundId", x => x.RoundId,
                    "SpeedrunRounds", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_SpeedrunHintReleaseLogs_ChallengeId", "SpeedrunHintReleaseLogs", "ChallengeId");
        migrationBuilder.CreateIndex("IX_SpeedrunHintReleaseLogs_GameId", "SpeedrunHintReleaseLogs", "GameId");
        migrationBuilder.CreateIndex("IX_SpeedrunHintReleaseLogs_RoundId_ChallengeId_HintIndex",
            "SpeedrunHintReleaseLogs", new[] { "RoundId", "ChallengeId", "HintIndex" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SpeedrunHintReleaseLogs");
        migrationBuilder.DropColumn("SpeedrunDefaultRoundDurationSeconds", "Games");
        migrationBuilder.DropColumn("SpeedrunOvertimeSeconds", "Games");
        migrationBuilder.DropColumn("DurationSeconds", "SpeedrunRounds");
        migrationBuilder.DropColumn("OvertimeSeconds", "SpeedrunRounds");
        migrationBuilder.DropColumn("ManuallyExtendedSeconds", "SpeedrunRounds");
        migrationBuilder.DropColumn("SpeedrunHintReleaseSeconds", "GameChallenges");
    }
}
