using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChillChaser.Migrations {
    /// <inheritdoc />
    public partial class AddReferencialStressView : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("""
            CREATE MATERIALIZED VIEW "ReferencialStress" AS 
            SELECT
                au."Id" AS "AppUsageId",
                AVG(hr."Bpm") as "ReferenceStress"
            FROM "AppUsages" au
            LEFT JOIN "HeartRates" hr ON 
                hr."DateTime" >= (au."From" - INTERVAL '1 hour') 
                AND hr."DateTime" <= au."From"
                AND hr."UserId" = au."UserId"
            GROUP BY
                au."Id"
            """);

            migrationBuilder.Sql("""
            CREATE MATERIALIZED VIEW "AppUsageStress" AS 
            SELECT
                au."Id" AS "AppUsageId",
                AVG(hr."Bpm") AS "AverageStress"
            FROM "AppUsages" au
            LEFT JOIN "HeartRates" hr ON
                hr."DateTime" >= au."From"
                AND hr."DateTime" <= au."To"
                AND hr."UserId" = au."UserId"
            GROUP BY
                au."Id"
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.Sql("""
            DROP MATERIALIZED VIEW public."ReferencialStress"
            """);

            migrationBuilder.Sql("""
            DROP MATERIALIZED VIEW public."AppUsageStress"
            """);
        }
    }
}
