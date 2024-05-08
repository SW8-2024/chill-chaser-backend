using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChillChaser.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HeartRates_UserId",
                table: "HeartRates");

            migrationBuilder.DropIndex(
                name: "IX_AppUsages_UserId",
                table: "AppUsages");

            migrationBuilder.CreateIndex(
                name: "datetime_idx",
                table: "HeartRates",
                column: "DateTime");

            migrationBuilder.CreateIndex(
                name: "datetime_userid_idx",
                table: "HeartRates",
                columns: new[] { "UserId", "DateTime" });

            migrationBuilder.CreateIndex(
                name: "from_user_idx",
                table: "AppUsages",
                columns: new[] { "UserId", "From" });

            migrationBuilder.CreateIndex(
                name: "to_user_idx",
                table: "AppUsages",
                columns: new[] { "UserId", "To" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "datetime_idx",
                table: "HeartRates");

            migrationBuilder.DropIndex(
                name: "datetime_userid_idx",
                table: "HeartRates");

            migrationBuilder.DropIndex(
                name: "from_user_idx",
                table: "AppUsages");

            migrationBuilder.DropIndex(
                name: "to_user_idx",
                table: "AppUsages");

            migrationBuilder.CreateIndex(
                name: "IX_HeartRates_UserId",
                table: "HeartRates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsages_UserId",
                table: "AppUsages",
                column: "UserId");
        }
    }
}
