using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GZCTF.Migrations
{
    /// <inheritdoc />
    public partial class AddGameWhitelistGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "WhitelistSource",
                table: "Participations",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<bool>(
                name: "WhitelistOnly",
                table: "Games",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CaptainOnboardingInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    AdditionalGameIds = table.Column<int[]>(type: "integer[]", nullable: false),
                    TeamName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CaptainEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TeamId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaptainOnboardingInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaptainOnboardingInvites_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaptainOnboardingInvites_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WhitelistJoinAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttemptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhitelistJoinAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhitelistJoinAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WhitelistJoinAttempts_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WhitelistJoinAttempts_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaptainOnboardingInvites_GameId_CaptainEmail",
                table: "CaptainOnboardingInvites",
                columns: new[] { "GameId", "CaptainEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_CaptainOnboardingInvites_TeamId",
                table: "CaptainOnboardingInvites",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CaptainOnboardingInvites_TokenHash",
                table: "CaptainOnboardingInvites",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhitelistJoinAttempts_GameId_AttemptedAtUtc",
                table: "WhitelistJoinAttempts",
                columns: new[] { "GameId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WhitelistJoinAttempts_TeamId_AttemptedAtUtc",
                table: "WhitelistJoinAttempts",
                columns: new[] { "TeamId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_WhitelistJoinAttempts_UserId",
                table: "WhitelistJoinAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaptainOnboardingInvites");

            migrationBuilder.DropTable(
                name: "WhitelistJoinAttempts");

            migrationBuilder.DropColumn(
                name: "WhitelistSource",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "WhitelistOnly",
                table: "Games");
        }
    }
}
