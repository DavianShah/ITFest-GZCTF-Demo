using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace GZCTF.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260730000000_AddOnboardingInviteMonitoring")]
public partial class AddOnboardingInviteMonitoring : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "LastEmailQueued",
            table: "CaptainOnboardingInvites",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastSentAtUtc",
            table: "CaptainOnboardingInvites",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "OpenedAtUtc",
            table: "CaptainOnboardingInvites",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "RevokedAtUtc",
            table: "CaptainOnboardingInvites",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SendCount",
            table: "CaptainOnboardingInvites",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql(
            """
            UPDATE "CaptainOnboardingInvites"
            SET "LastSentAtUtc" = "CreatedAtUtc",
                "SendCount" = 1
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastEmailQueued",
            table: "CaptainOnboardingInvites");

        migrationBuilder.DropColumn(
            name: "LastSentAtUtc",
            table: "CaptainOnboardingInvites");

        migrationBuilder.DropColumn(
            name: "OpenedAtUtc",
            table: "CaptainOnboardingInvites");

        migrationBuilder.DropColumn(
            name: "RevokedAtUtc",
            table: "CaptainOnboardingInvites");

        migrationBuilder.DropColumn(
            name: "SendCount",
            table: "CaptainOnboardingInvites");
    }
}
