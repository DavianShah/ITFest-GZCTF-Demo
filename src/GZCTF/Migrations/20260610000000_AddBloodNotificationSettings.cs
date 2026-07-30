using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260610000000_AddBloodNotificationSettings")]
public partial class AddBloodNotificationSettings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BloodDiscordWebhookUrl",
            table: "Games",
            type: "character varying(512)",
            maxLength: 512,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "BloodNotificationEnabled",
            table: "Games",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "BloodNotificationMaxRank",
            table: "Games",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<string>(
            name: "BloodNotificationTemplate",
            table: "Games",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: false,
            defaultValue: "Challenge **{challenge}** has been blooded!");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BloodDiscordWebhookUrl", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationEnabled", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationMaxRank", table: "Games");
        migrationBuilder.DropColumn(name: "BloodNotificationTemplate", table: "Games");
    }
}
