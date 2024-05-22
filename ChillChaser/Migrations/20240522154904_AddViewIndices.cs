using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChillChaser.Migrations
{
    /// <inheritdoc />
    public partial class AddViewIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            CREATE UNIQUE INDEX "ReferencialStress_id_ui"
                ON "ReferencialStress" ("AppUsageId");
            """);

            migrationBuilder.Sql("""
            CREATE UNIQUE INDEX "AppUsageStress_id_ui"
                ON "AppUsageStress" ("AppUsageId");
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            DROP INDEX "ReferencialStress_id_ui"
            """);

            migrationBuilder.Sql("""
            DROP INDEX "AppUsageStress_id_ui"
            """);
        }
    }
}
