using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoriaIdentityProvider.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NullableUserSessionRefreshToken_Id : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_SESSION_REFRESH_TOKENS_RefreshTokenId",
                table: "USER_SESSION");

            migrationBuilder.DropIndex(
                name: "IX_USER_SESSION_RefreshTokenId",
                table: "USER_SESSION");

            migrationBuilder.AlterColumn<Guid>(
                name: "RefreshTokenId",
                table: "USER_SESSION",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_USER_SESSION_RefreshTokenId",
                table: "USER_SESSION",
                column: "RefreshTokenId",
                unique: true,
                filter: "[RefreshTokenId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_USER_SESSION_REFRESH_TOKENS_RefreshTokenId",
                table: "USER_SESSION",
                column: "RefreshTokenId",
                principalTable: "REFRESH_TOKENS",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_SESSION_REFRESH_TOKENS_RefreshTokenId",
                table: "USER_SESSION");

            migrationBuilder.DropIndex(
                name: "IX_USER_SESSION_RefreshTokenId",
                table: "USER_SESSION");

            migrationBuilder.AlterColumn<Guid>(
                name: "RefreshTokenId",
                table: "USER_SESSION",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_SESSION_RefreshTokenId",
                table: "USER_SESSION",
                column: "RefreshTokenId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_SESSION_REFRESH_TOKENS_RefreshTokenId",
                table: "USER_SESSION",
                column: "RefreshTokenId",
                principalTable: "REFRESH_TOKENS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
