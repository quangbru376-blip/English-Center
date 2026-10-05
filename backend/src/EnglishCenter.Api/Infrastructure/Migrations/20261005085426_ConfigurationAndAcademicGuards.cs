using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurationAndAcademicGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(SqlMigrationScript.Batch("D08.TG01.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D08.TG02.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D08.TG03.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D08.TG04.sql"));
            migrationBuilder.Sql(SqlMigrationScript.Batch("D08.TG05.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER [dbo].[trg_Score_ComponentScheduleGradeGuard];");
            migrationBuilder.Sql("DROP TRIGGER [dbo].[trg_Attendance_ClassScheduleGuard];");
            migrationBuilder.Sql("DROP TRIGGER [dbo].[trg_Band_ThresholdSetFreezeGuard];");
            migrationBuilder.Sql("DROP TRIGGER [dbo].[trg_Component_UsedConfigurationGuard];");
            migrationBuilder.Sql("DROP TRIGGER [dbo].[trg_Program_UsedConfigurationGuard];");
        }
    }
}
