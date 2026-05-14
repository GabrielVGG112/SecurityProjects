using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoriaIdentityProvider.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshTokenRelationship : Migration
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

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "USER_SESSION");

            migrationBuilder.AlterColumn<string>(
                name: "RevokedReason",
                table: "USER_SESSION",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_REFRESH_TOKENS_UserSessionId",
                table: "REFRESH_TOKENS",
                column: "UserSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_REFRESH_TOKENS_USER_SESSION_UserSessionId",
                table: "REFRESH_TOKENS",
                column: "UserSessionId",
                principalTable: "USER_SESSION",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_REFRESH_TOKENS_USER_SESSION_UserSessionId",
                table: "REFRESH_TOKENS");

            migrationBuilder.DropIndex(
                name: "IX_REFRESH_TOKENS_UserSessionId",
                table: "REFRESH_TOKENS");

            migrationBuilder.AlterColumn<string>(
                name: "RevokedReason",
                table: "USER_SESSION",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "USER_SESSION",
                type: "bit",
                nullable: false,
                defaultValue: false);

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
    }
}
