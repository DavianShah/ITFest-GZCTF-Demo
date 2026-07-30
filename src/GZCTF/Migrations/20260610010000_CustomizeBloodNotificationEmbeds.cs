using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260610010000_CustomizeBloodNotificationEmbeds")]
public partial class CustomizeBloodNotificationEmbeds : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationEmbedTitleTemplate", table: "Games", type: "character varying(512)",
            maxLength: 512, nullable: false, defaultValue: "{emoji} {blood} BLOOD!");

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationEmbedDescriptionTemplate", table: "Games", type: "character varying(2000)",
            maxLength: 2000, nullable: false,
            defaultValue: "**{team}** conquered **{challenge}** and claimed rank **#{rank}**!");

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationEmbedColor", table: "Games", type: "character varying(16)",
            maxLength: 16, nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationEmbedFieldsTemplate", table: "Games", type: "character varying(4000)",
            maxLength: 4000, nullable: false,
            defaultValue: "👤 User / Team|{team}|true\n🏁 Challenge|{challenge}|true\n📂 Category|{category}|true\n💯 Points|{score}|true\n🎮 Game|{game}|true\n🏆 Rank|#{rank}|true");

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationEmbedFooterTemplate", table: "Games", type: "character varying(512)",
            maxLength: 512, nullable: false, defaultValue: "Solved at {time} • ITFest CTF");

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationTimeZone", table: "Games", type: "character varying(64)",
            maxLength: 64, nullable: false, defaultValue: "Asia/Jakarta");

        migrationBuilder.Sql("""
            UPDATE "Games"
            SET "BloodNotificationEmbedDescriptionTemplate" = "BloodNotificationTemplate"
            WHERE "BloodNotificationTemplate" IS NOT NULL
              AND "BloodNotificationTemplate" <> '';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BloodNotificationEmbedTitleTemplate", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationEmbedDescriptionTemplate", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationEmbedColor", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationEmbedFieldsTemplate", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationEmbedFooterTemplate", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationTimeZone", table: "Games");
    }
}
