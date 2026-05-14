using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VictoriaIdentityProvider.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDbConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JwtId",
                table: "REFRESH_TOKENS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JwtId",
                table: "REFRESH_TOKENS",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
