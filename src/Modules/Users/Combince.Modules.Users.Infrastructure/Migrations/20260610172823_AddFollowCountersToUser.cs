using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combince.Modules.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowCountersToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FollowersCount",
                schema: "users",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FollowingCount",
                schema: "users",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FollowersCount",
                schema: "users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FollowingCount",
                schema: "users",
                table: "Users");
        }
    }
}
