using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReadOnlyBusinessViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW01.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW02.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW03.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW04.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW05.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D10.VW06.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_FinanceInvoiceFacts];");
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_FinalizedGradeFacts];");
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_TrainingClassFacts];");
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_CurrentScheduleOccurrences];");
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_EntryTestBookingFacts];");
            migrationBuilder.Sql("DROP VIEW [dbo].[vw_AdmissionsRegistrationFacts];");

        }
    }
}
