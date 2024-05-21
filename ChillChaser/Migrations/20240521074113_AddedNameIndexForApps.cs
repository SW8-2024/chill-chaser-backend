using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChillChaser.Migrations
{
    /// <inheritdoc />
    public partial class AddedNameIndexForApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "name_unique_idx",
                table: "Apps",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "name_unique_idx",
                table: "Apps");
        }
    }
}
