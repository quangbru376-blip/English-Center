using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LeastPrivilegeRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlMigrationScript.Batch("D12.RUNTIME.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D12.FINANCE.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D12.ACADEMICS.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D12.DIRECTOR.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Preflight every membership before removing any role.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.database_role_members AS membership
                    JOIN sys.database_principals AS role ON role.principal_id=membership.role_principal_id
                    WHERE role.name IN ('ec_app_runtime','ec_finance_reporting','ec_academics_reporting','ec_director_reporting')
                ) THROW 51213,'D12 roles still have members; rollback refused',1;
                DROP ROLE [ec_director_reporting];
                DROP ROLE [ec_academics_reporting];
                DROP ROLE [ec_finance_reporting];
                DROP ROLE [ec_app_runtime];
                """);
        }
    }
}
