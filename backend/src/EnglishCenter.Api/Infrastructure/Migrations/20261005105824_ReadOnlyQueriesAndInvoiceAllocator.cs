using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReadOnlyQueriesAndInvoiceAllocator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP01.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP02.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP03.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP04.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP05.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP06.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D11.SP07.sql"));

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_AllocateInvoiceNumber];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_GetTrainingReport];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_GetAdmissionsReport];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_GetFinancialReport];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_GetClassGradeReview];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_GetEntryTestRoster];");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[usp_SearchAdmissions];");

        }
    }
}
