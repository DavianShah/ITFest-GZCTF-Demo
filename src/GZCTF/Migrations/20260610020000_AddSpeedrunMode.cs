using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260610020000_AddSpeedrunMode")]
public partial class AddSpeedrunMode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<byte>("Mode", "Games", "smallint", nullable: false, defaultValue: (byte)0);
        migrationBuilder.AddColumn<int>("SpeedrunDefaultRoundDurationMinutes", "Games", "integer", nullable: false, defaultValue: 30);
        migrationBuilder.AddColumn<int>("SpeedrunOvertimeMinutes", "Games", "integer", nullable: false, defaultValue: 5);
        migrationBuilder.AddColumn<bool>("SpeedrunAllowManualExtend", "Games", "boolean", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<bool>("SpeedrunHideInactiveChallenges", "Games", "boolean", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<bool>("SpeedrunEmergencyHintEnabled", "Games", "boolean", nullable: false, defaultValue: true);
        migrationBuilder.AddColumn<string>("SpeedrunEmergencyHintText", "Games", "character varying(1000)", maxLength: 1000,
            nullable: false, defaultValue: "Overtime unlocked! Unsolved challenges remain available for 5 more minutes.");

        migrationBuilder.CreateTable(
            name: "SpeedrunCategories",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GameId = table.Column<int>(type: "integer", nullable: false),
                Category = table.Column<byte>(type: "smallint", nullable: false),
                Used = table.Column<bool>(type: "boolean", nullable: false),
                Included = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SpeedrunCategories", x => x.Id);
                table.ForeignKey(
                    name: "FK_SpeedrunCategories_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_SpeedrunCategories_GameId_Category", "SpeedrunCategories", new[] { "GameId", "Category" }, unique: true);

        migrationBuilder.CreateTable(
            name: "SpeedrunRounds",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                GameId = table.Column<int>(type: "integer", nullable: false),
                Category = table.Column<byte>(type: "smallint", nullable: false),
                Status = table.Column<byte>(type: "smallint", nullable: false),
                StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                EndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                OvertimeEndsAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                OvertimeMinutes = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                FinishedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                SelectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                ManuallyExtendedMinutes = table.Column<int>(type: "integer", nullable: false),
                OvertimeNoticeSent = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SpeedrunRounds", x => x.Id);
                table.ForeignKey(
                    name: "FK_SpeedrunRounds_Games_GameId",
                    column: x => x.GameId,
                    principalTable: "Games",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateIndex("IX_SpeedrunRounds_GameId_Status", "SpeedrunRounds", new[] { "GameId", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("SpeedrunCategories");
        migrationBuilder.DropTable("SpeedrunRounds");
        migrationBuilder.DropColumn("Mode", "Games");
        migrationBuilder.DropColumn("SpeedrunDefaultRoundDurationMinutes", "Games");
        migrationBuilder.DropColumn("SpeedrunOvertimeMinutes", "Games");
        migrationBuilder.DropColumn("SpeedrunAllowManualExtend", "Games");
        migrationBuilder.DropColumn("SpeedrunHideInactiveChallenges", "Games");
        migrationBuilder.DropColumn("SpeedrunEmergencyHintEnabled", "Games");
        migrationBuilder.DropColumn("SpeedrunEmergencyHintText", "Games");
    }
}
