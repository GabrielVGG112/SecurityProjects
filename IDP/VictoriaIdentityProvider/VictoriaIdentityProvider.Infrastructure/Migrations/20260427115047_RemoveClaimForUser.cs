using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoriaIdentityProvider.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClaimForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USER_CLAIMS_USERS_UserId",
                table: "USER_CLAIMS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USER_CLAIMS",
                table: "USER_CLAIMS");

            migrationBuilder.DropIndex(
                name: "IX_USER_CLAIMS_ClaimType",
                table: "USER_CLAIMS");

            migrationBuilder.RenameTable(
                name: "USER_CLAIMS",
                newName: "UsersClaim");

            migrationBuilder.RenameIndex(
                name: "IX_USER_CLAIMS_UserId",
                table: "UsersClaim",
                newName: "IX_UsersClaim_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "UsersClaim",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersClaim",
                table: "UsersClaim",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersClaim_USERS_UserId",
                table: "UsersClaim",
                column: "UserId",
                principalTable: "USERS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersClaim_USERS_UserId",
                table: "UsersClaim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersClaim",
                table: "UsersClaim");

            migrationBuilder.RenameTable(
                name: "UsersClaim",
                newName: "USER_CLAIMS");

            migrationBuilder.RenameIndex(
                name: "IX_UsersClaim_UserId",
                table: "USER_CLAIMS",
                newName: "IX_USER_CLAIMS_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "USER_CLAIMS",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USER_CLAIMS",
                table: "USER_CLAIMS",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_USER_CLAIMS_ClaimType",
                table: "USER_CLAIMS",
                column: "ClaimType",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_USER_CLAIMS_USERS_UserId",
                table: "USER_CLAIMS",
                column: "UserId",
                principalTable: "USERS",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
