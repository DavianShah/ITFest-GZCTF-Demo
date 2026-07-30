using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260611020000_AddLiveScoreboard")]
public partial class AddLiveScoreboard : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameLiveScoreboardConfigs",
            columns: table => new
            {
                GameId = table.Column<int>(type: "integer", nullable: false),
                Enabled = table.Column<bool>(type: "boolean", nullable: false),
                Title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false,
                    defaultValue: "ITFest Live Scoreboard"),
                Subtitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                SoundEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                Volume = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.75),
                VisualIntensity = table.Column<byte>(type: "smallint", nullable: false),
                SoundSpin = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundCategorySelected = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundGameStart = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundHintDrop = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundFirstBlood = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundSecondBlood = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundThirdBlood = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundCorrectSubmit = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundWrongSubmit = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundReminder = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundCountdownTick = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundOvertime = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundRoundFinished = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                SoundScoreUpdate = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameLiveScoreboardConfigs", x => x.GameId);
                table.ForeignKey("FK_GameLiveScoreboardConfigs_Games_GameId", x => x.GameId, "Games", "Id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable("GameLiveScoreboardConfigs");
}
