using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChillChaser.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueAppUsageConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "from_app_user_ui",
                table: "AppUsages",
                columns: new[] { "From", "AppId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "from_app_user_ui",
                table: "AppUsages");
        }
    }
}
