using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoriaIdentityProvider.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersClaim");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "USER_SESSION",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "RevokedReason",
                table: "USER_SESSION",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "USER_SESSION",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "USER_SESSION",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "REFRESH_TOKENS",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "REFRESH_TOKENS",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "REFRESH_TOKENS",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AUDIT_LOGS",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AUDIT_LOGS",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "AUDIT_LOGS",
                type: "rowversion",
                rowVersion: true,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "USER_SESSION");

            migrationBuilder.DropColumn(
                name: "RevokedReason",
                table: "USER_SESSION");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "USER_SESSION");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "USER_SESSION");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "REFRESH_TOKENS");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "REFRESH_TOKENS");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "REFRESH_TOKENS");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AUDIT_LOGS");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AUDIT_LOGS");

            migrationBuilder.CreateTable(
                name: "UsersClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersClaim_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "USERS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersClaim_UserId",
                table: "UsersClaim",
                column: "UserId");
        }
    }
}
