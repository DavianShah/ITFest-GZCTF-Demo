using GZCTF.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GZCTF.Migrations;

/// <summary>
/// Repairs databases where CaptainOnboardingInvites was created by an earlier
/// development version before multi-game onboarding added AdditionalGameIds.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260730010000_RepairOnboardingAdditionalGameIds")]
public partial class RepairOnboardingAdditionalGameIds : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE IF EXISTS "CaptainOnboardingInvites"
                ADD COLUMN IF NOT EXISTS "AdditionalGameIds" integer[] NOT NULL DEFAULT ARRAY[]::integer[];

            ALTER TABLE IF EXISTS "CaptainOnboardingInvites"
                ALTER COLUMN "AdditionalGameIds" DROP DEFAULT;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Intentionally left empty. The column belongs to AddGameWhitelistGate's
        // target model, so dropping only this repair would leave that model broken.
    }
}
