using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReadOnlyBusinessFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN01.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN02.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN03.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN04.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN05.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D09.FN06.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_InvoiceDebtFlags];");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_GradeTotal];");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_ClassObligationProgress];");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_RegistrationUnfinished];");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_BandForScore];");
            migrationBuilder.Sql("DROP FUNCTION [dbo].[ufn_EffectiveEntryTestState];");
        }
    }
}
