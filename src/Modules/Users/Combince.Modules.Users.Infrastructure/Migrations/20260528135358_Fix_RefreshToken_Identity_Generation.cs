using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combince.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_RefreshToken_Identity_Generation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                schema: "users",
                table: "UserRefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                schema: "users",
                table: "UserRefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
