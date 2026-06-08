using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combince.Modules.Ratings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialRatingsCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ratings");

            migrationBuilder.CreateTable(
                name: "PostRatings",
                schema: "ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostRatings_PostId_UserId",
                schema: "ratings",
                table: "PostRatings",
                columns: new[] { "PostId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostRatings",
                schema: "ratings");
        }
    }
}
