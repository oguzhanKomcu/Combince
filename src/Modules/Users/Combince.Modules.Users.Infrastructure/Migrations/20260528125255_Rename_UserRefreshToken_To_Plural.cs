using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combince.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Rename_UserRefreshToken_To_Plural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRefreshToken_Users_UserId",
                schema: "users",
                table: "UserRefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRefreshToken",
                schema: "users",
                table: "UserRefreshToken");

            migrationBuilder.RenameTable(
                name: "UserRefreshToken",
                schema: "users",
                newName: "UserRefreshTokens",
                newSchema: "users");

            migrationBuilder.RenameIndex(
                name: "IX_UserRefreshToken_UserId",
                schema: "users",
                table: "UserRefreshTokens",
                newName: "IX_UserRefreshTokens_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRefreshTokens",
                schema: "users",
                table: "UserRefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRefreshTokens_Users_UserId",
                schema: "users",
                table: "UserRefreshTokens",
                column: "UserId",
                principalSchema: "users",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRefreshTokens_Users_UserId",
                schema: "users",
                table: "UserRefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserRefreshTokens",
                schema: "users",
                table: "UserRefreshTokens");

            migrationBuilder.RenameTable(
                name: "UserRefreshTokens",
                schema: "users",
                newName: "UserRefreshToken",
                newSchema: "users");

            migrationBuilder.RenameIndex(
                name: "IX_UserRefreshTokens_UserId",
                schema: "users",
                table: "UserRefreshToken",
                newName: "IX_UserRefreshToken_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserRefreshToken",
                schema: "users",
                table: "UserRefreshToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRefreshToken_Users_UserId",
                schema: "users",
                table: "UserRefreshToken",
                column: "UserId",
                principalSchema: "users",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
