using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishCenter.Api.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Band",
                schema: "dbo",
                columns: table => new
                {
                    band_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    band_code = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    min_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Band", x => x.band_id);
                    table.CheckConstraint("CK_Band_Code", "DATALENGTH([band_code]) BETWEEN 1 AND 30 AND [band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%' AND LEN([band_code]) > 0");
                    table.CheckConstraint("CK_Band_MinScore", "[min_score] >= 0 AND [min_score] <= 100");
                });

            migrationBuilder.CreateTable(
                name: "Course",
                schema: "dbo",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_code = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    course_name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    base_price = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.course_id);
                    table.CheckConstraint("CK_Course_Code", "DATALENGTH([course_code]) BETWEEN 1 AND 30 AND DATALENGTH([course_code]) = LEN([course_code]) AND [course_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");
                    table.CheckConstraint("CK_Course_Price", "[base_price] > 0");
                    table.CheckConstraint("CK_Course_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceNumberCounter",
                schema: "dbo",
                columns: table => new
                {
                    issue_year = table.Column<short>(type: "smallint", nullable: false),
                    last_value = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceNumberCounter", x => x.issue_year);
                    table.CheckConstraint("CK_InvoiceNumberCounter_Value", "[last_value] BETWEEN 0 AND 99999999");
                    table.CheckConstraint("CK_InvoiceNumberCounter_Year", "[issue_year] BETWEEN 1 AND 9999");
                });

            migrationBuilder.CreateTable(
                name: "Role",
                schema: "dbo",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_code = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.role_id);
                    table.CheckConstraint("CK_Role_Code", "[role_code] COLLATE Latin1_General_100_BIN2 IN ('Student', 'Admissions', 'AcademicAffairs', 'Teacher', 'Accountant', 'Director', 'Admin') AND DATALENGTH([role_code]) = LEN([role_code])");
                });

            migrationBuilder.CreateTable(
                name: "TrainingProgram",
                schema: "dbo",
                columns: table => new
                {
                    program_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    band_id = table.Column<int>(type: "int", nullable: false),
                    program_name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "DRAFT"),
                    expected_lesson_count = table.Column<int>(type: "int", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingProgram", x => x.program_id);
                    table.CheckConstraint("CK_TrainingProgram_ExpectedLessons", "[expected_lesson_count] > 0");
                    table.CheckConstraint("CK_TrainingProgram_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT','ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])");
                    table.ForeignKey(
                        name: "FK_TrainingProgram_Band_band_id",
                        column: x => x.band_id,
                        principalSchema: "dbo",
                        principalTable: "Band",
                        principalColumn: "band_id");
                    table.ForeignKey(
                        name: "FK_TrainingProgram_Course_course_id",
                        column: x => x.course_id,
                        principalSchema: "dbo",
                        principalTable: "Course",
                        principalColumn: "course_id");
                });

            migrationBuilder.CreateTable(
                name: "Account",
                schema: "dbo",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "varchar(254)", unicode: false, maxLength: 254, nullable: false, collation: "Latin1_General_100_CI_AS_SC"),
                    phone = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    password_hash = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    security_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    failed_login_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    failure_window_started_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    login_throttled_until = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    must_change_password = table.Column<bool>(type: "bit", nullable: false),
                    temp_issued_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    temp_expires_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.account_id);
                    table.CheckConstraint("CK_Account_FailedLoginCount", "[failed_login_count] >= 0");
                    table.CheckConstraint("CK_Account_Phone", "[phone] IS NULL OR (DATALENGTH([phone]) = 10 AND LEFT([phone], 1) = '0' AND [phone] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^0-9]%')");
                    table.CheckConstraint("CK_Account_SecurityVersion", "[security_version] > 0");
                    table.CheckConstraint("CK_Account_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE', 'LOCKED', 'DISABLED') AND DATALENGTH([status]) = LEN([status])");
                    table.CheckConstraint("CK_Account_TempBundle", "([must_change_password] = 1 AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at] = DATEADD(hour, 24, [temp_issued_at])) OR ([must_change_password] = 0 AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Account_Role_role_id",
                        column: x => x.role_id,
                        principalSchema: "dbo",
                        principalTable: "Role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "ScoreComponentConfig",
                schema: "dbo",
                columns: table => new
                {
                    component_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    program_id = table.Column<int>(type: "int", nullable: false),
                    component_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, collation: "Latin1_General_100_CI_AS_SC"),
                    weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    kind = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreComponentConfig", x => x.component_id);
                    table.CheckConstraint("CK_ScoreComponentConfig_Kind", "[kind] COLLATE Latin1_General_100_BIN2 IN ('GENERAL','LESSON','FINAL_EXAM') AND DATALENGTH([kind]) = LEN([kind])");
                    table.CheckConstraint("CK_ScoreComponentConfig_Name", "LEN([component_name]) > 0");
                    table.CheckConstraint("CK_ScoreComponentConfig_Weight", "[weight] > 0 AND [weight] <= 100");
                    table.ForeignKey(
                        name: "FK_ScoreComponentConfig_TrainingProgram_program_id",
                        column: x => x.program_id,
                        principalSchema: "dbo",
                        principalTable: "TrainingProgram",
                        principalColumn: "program_id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                schema: "dbo",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    actor_account_id = table.Column<int>(type: "int", nullable: true),
                    action = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    entity_type = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    entity_id = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    result = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    ip_address = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    audit_category = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    security_expires_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    ip_expires_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.audit_id);
                    table.CheckConstraint("CK_AuditLog_Category", "[audit_category] COLLATE Latin1_General_100_BIN2 IN ('MANDATORY', 'SECURITY') AND DATALENGTH([audit_category]) = LEN([audit_category])");
                    table.CheckConstraint("CK_AuditLog_Details", "[details] IS NULL OR ISJSON([details]) = 1");
                    table.CheckConstraint("CK_AuditLog_IPBundle", "([ip_address] IS NULL AND [ip_expires_at] IS NULL) OR ([ip_address] IS NOT NULL AND [ip_expires_at] IS NOT NULL AND [ip_expires_at] = DATEADD(day, 90, [occurred_at]))");
                    table.CheckConstraint("CK_AuditLog_Result", "[result] COLLATE Latin1_General_100_BIN2 IN ('SUCCESS', 'FAILURE') AND DATALENGTH([result]) = LEN([result])");
                    table.CheckConstraint("CK_AuditLog_SecurityExpiry", "([audit_category] = 'SECURITY' AND [security_expires_at] IS NOT NULL AND [security_expires_at] >= DATEADD(day, 90, [occurred_at])) OR ([audit_category] = 'MANDATORY' AND [security_expires_at] IS NULL)");
                    table.ForeignKey(
                        name: "FK_AuditLog_Account_actor_account_id",
                        column: x => x.actor_account_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "AuthSession",
                schema: "dbo",
                columns: table => new
                {
                    sid = table.Column<byte[]>(type: "binary(32)", fixedLength: true, maxLength: 32, nullable: false),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    expires_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    last_activity_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    revoked_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    security_version = table.Column<long>(type: "bigint", nullable: false),
                    session_scope = table.Column<string>(type: "varchar(24)", unicode: false, maxLength: 24, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthSession", x => x.sid);
                    table.CheckConstraint("CK_AuthSession_Lifetime", "[issued_at] <= [last_activity_at] AND [last_activity_at] <= [expires_at] AND [expires_at] > [issued_at]");
                    table.CheckConstraint("CK_AuthSession_RevokedAt", "[revoked_at] IS NULL OR [revoked_at] >= [issued_at]");
                    table.CheckConstraint("CK_AuthSession_Scope", "[session_scope] COLLATE Latin1_General_100_BIN2 IN ('NORMAL', 'PASSWORD_CHANGE') AND DATALENGTH([session_scope]) = LEN([session_scope])");
                    table.ForeignKey(
                        name: "FK_AuthSession_Account_account_id",
                        column: x => x.account_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "Class",
                schema: "dbo",
                columns: table => new
                {
                    class_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    program_id = table.Column<int>(type: "int", nullable: false),
                    teacher_id = table.Column<int>(type: "int", nullable: false),
                    class_code = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "PLANNED"),
                    max_size = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    cancellation_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.class_id);
                    table.CheckConstraint("CK_Class_CancellationBundle", "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
                    table.CheckConstraint("CK_Class_Code", "DATALENGTH([class_code]) BETWEEN 1 AND 30 AND DATALENGTH([class_code]) = LEN([class_code]) AND [class_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");
                    table.CheckConstraint("CK_Class_MaxSize", "[max_size] BETWEEN 1 AND 15");
                    table.CheckConstraint("CK_Class_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
                    table.ForeignKey(
                        name: "FK_Class_Account_cancelled_by",
                        column: x => x.cancelled_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Class_Account_teacher_id",
                        column: x => x.teacher_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Class_TrainingProgram_program_id",
                        column: x => x.program_id,
                        principalSchema: "dbo",
                        principalTable: "TrainingProgram",
                        principalColumn: "program_id");
                });

            migrationBuilder.CreateTable(
                name: "EntryTestSession",
                schema: "dbo",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    start_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    end_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    room = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    grader_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "CLOSED"),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    cancellation_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTestSession", x => x.session_id);
                    table.CheckConstraint("CK_EntryTestSession_CancelledBundle", "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
                    table.CheckConstraint("CK_EntryTestSession_Capacity", "[capacity] BETWEEN 1 AND 20");
                    table.CheckConstraint("CK_EntryTestSession_Room", "DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");
                    table.CheckConstraint("CK_EntryTestSession_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('OPEN', 'CLOSED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
                    table.CheckConstraint("CK_EntryTestSession_Time", "[start_at] < [end_at] AND DATEPART(second, [start_at]) = 0 AND DATEPART(millisecond, [start_at]) = 0 AND DATEPART(second, [end_at]) = 0 AND DATEPART(millisecond, [end_at]) = 0 AND CAST(DATEADD(minute, 420, [start_at]) AS date) = CAST(DATEADD(minute, 420, [end_at]) AS date)");
                    table.ForeignKey(
                        name: "FK_EntryTestSession_Account_cancelled_by",
                        column: x => x.cancelled_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_EntryTestSession_Account_grader_id",
                        column: x => x.grader_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "ReplayOperation",
                schema: "dbo",
                columns: table => new
                {
                    replay_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    actor_account_id = table.Column<int>(type: "int", nullable: false),
                    action_code = table.Column<string>(type: "varchar(80)", unicode: false, maxLength: 80, nullable: false),
                    request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    input_digest = table.Column<byte[]>(type: "binary(32)", fixedLength: true, maxLength: 32, nullable: false),
                    operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    result_mode = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    result_reference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayOperation", x => x.replay_id);
                    table.CheckConstraint("CK_ReplayOperation_Expiry", "[expires_at] >= DATEADD(hour, 24, [completed_at])");
                    table.CheckConstraint("CK_ReplayOperation_Mode", "[result_mode] COLLATE Latin1_General_100_BIN2 IN ('SAFE_REFERENCE', 'SECRET_RESULT_UNAVAILABLE') AND DATALENGTH([result_mode]) = LEN([result_mode])");
                    table.CheckConstraint("CK_ReplayOperation_ResultReference", "ISJSON([result_reference]) = 1");
                    table.ForeignKey(
                        name: "FK_ReplayOperation_Account_actor_account_id",
                        column: x => x.actor_account_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "StudentProfile",
                schema: "dbo",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    account_id = table.Column<int>(type: "int", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    address = table.Column<string>(type: "nvarchar(510)", maxLength: 510, nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfile", x => x.student_id);
                    table.ForeignKey(
                        name: "FK_StudentProfile_Account_account_id",
                        column: x => x.account_id,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                });

            migrationBuilder.CreateTable(
                name: "Schedule",
                schema: "dbo",
                columns: table => new
                {
                    schedule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    session_type = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    obligation_key = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    replaces_schedule_id = table.Column<int>(type: "int", nullable: true),
                    is_current = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    session_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time(0)", precision: 0, nullable: false),
                    room = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    occurrence_state = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "PLANNED"),
                    held_by = table.Column<int>(type: "int", nullable: true),
                    held_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    not_held_by = table.Column<int>(type: "int", nullable: true),
                    not_held_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    not_held_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    replacement_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.schedule_id);
                    table.CheckConstraint("CK_Schedule_CurrentState", "([occurrence_state] IN ('PLANNED', 'HELD') AND [is_current] = 1) OR ([occurrence_state] = 'NOT_HELD' AND [is_current] = 0)");
                    table.CheckConstraint("CK_Schedule_HeldBundle", "([occurrence_state] = 'HELD' AND [held_by] IS NOT NULL AND [held_at] IS NOT NULL) OR ([occurrence_state] <> 'HELD' AND [held_by] IS NULL AND [held_at] IS NULL)");
                    table.CheckConstraint("CK_Schedule_NoSelfReplacement", "[replaces_schedule_id] IS NULL OR [replaces_schedule_id] <> [schedule_id]");
                    table.CheckConstraint("CK_Schedule_NotHeldBundle", "([occurrence_state] = 'NOT_HELD' AND [not_held_by] IS NOT NULL AND [not_held_at] IS NOT NULL AND [not_held_reason] IS NOT NULL AND LEN([not_held_reason]) > 0 AND [replacement_operation_id] IS NOT NULL) OR ([occurrence_state] <> 'NOT_HELD' AND [not_held_by] IS NULL AND [not_held_at] IS NULL AND [not_held_reason] IS NULL AND [replacement_operation_id] IS NULL)");
                    table.CheckConstraint("CK_Schedule_Room", "DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'");
                    table.CheckConstraint("CK_Schedule_State", "[occurrence_state] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'HELD', 'NOT_HELD') AND DATALENGTH([occurrence_state]) = LEN([occurrence_state])");
                    table.CheckConstraint("CK_Schedule_Time", "[start_time] < [end_time] AND DATEPART(second, [start_time]) = 0 AND DATEPART(second, [end_time]) = 0");
                    table.CheckConstraint("CK_Schedule_Type", "[session_type] COLLATE Latin1_General_100_BIN2 IN ('LESSON', 'FINAL_EXAM') AND DATALENGTH([session_type]) = LEN([session_type])");
                    table.ForeignKey(
                        name: "FK_Schedule_Account_held_by",
                        column: x => x.held_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Schedule_Account_not_held_by",
                        column: x => x.not_held_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Schedule_Class_class_id",
                        column: x => x.class_id,
                        principalSchema: "dbo",
                        principalTable: "Class",
                        principalColumn: "class_id");
                    table.ForeignKey(
                        name: "FK_Schedule_Schedule_replaces_schedule_id",
                        column: x => x.replaces_schedule_id,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "schedule_id");
                });

            migrationBuilder.CreateTable(
                name: "Registration",
                schema: "dbo",
                columns: table => new
                {
                    registration_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "PENDING"),
                    is_unfinished = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    admission_confirmed_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    admission_approved_by = table.Column<int>(type: "int", nullable: true),
                    admission_approved_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    rejected_by = table.Column<int>(type: "int", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    rejection_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    rejected_from_status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    rejection_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    cancelled_from_status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    cancellation_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registration", x => x.registration_id);
                    table.CheckConstraint("CK_Registration_ApprovedBundle", "([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL) OR ([admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL)");
                    table.CheckConstraint("CK_Registration_CancelledBundle", "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED') AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)");
                    table.CheckConstraint("CK_Registration_RejectedBundle", "([status] = 'REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND LEN([rejection_reason]) > 0 AND [rejected_from_status] IS NOT NULL AND [rejected_from_status] COLLATE Latin1_General_100_BIN2 = 'CONFIRMED' AND DATALENGTH([rejected_from_status]) = LEN([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL) OR ([status] <> 'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL)");
                    table.CheckConstraint("CK_Registration_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
                    table.ForeignKey(
                        name: "FK_Registration_Account_admission_approved_by",
                        column: x => x.admission_approved_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Registration_Account_cancelled_by",
                        column: x => x.cancelled_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Registration_Account_rejected_by",
                        column: x => x.rejected_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Registration_Course_course_id",
                        column: x => x.course_id,
                        principalSchema: "dbo",
                        principalTable: "Course",
                        principalColumn: "course_id");
                    table.ForeignKey(
                        name: "FK_Registration_StudentProfile_student_id",
                        column: x => x.student_id,
                        principalSchema: "dbo",
                        principalTable: "StudentProfile",
                        principalColumn: "student_id");
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                schema: "dbo",
                columns: table => new
                {
                    invoice_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_number = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    registration_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    issued_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "UNPAID"),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    cancelled_from_status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    cancellation_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.invoice_id);
                    table.CheckConstraint("CK_Invoice_Amount", "[amount] > 0");
                    table.CheckConstraint("CK_Invoice_CancelledBundle", "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancellation_operation_id] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'UNPAID' AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status])) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)");
                    table.CheckConstraint("CK_Invoice_NumberPattern", "DATALENGTH([invoice_number]) = 16 AND DATALENGTH([invoice_number]) = LEN([invoice_number]) AND [invoice_number] COLLATE Latin1_General_100_BIN2 LIKE 'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'");
                    table.CheckConstraint("CK_Invoice_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('UNPAID', 'PAID', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
                    table.ForeignKey(
                        name: "FK_Invoice_Account_cancelled_by",
                        column: x => x.cancelled_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Invoice_Registration_registration_id",
                        column: x => x.registration_id,
                        principalSchema: "dbo",
                        principalTable: "Registration",
                        principalColumn: "registration_id");
                });

            migrationBuilder.CreateTable(
                name: "StudentClassAssignment",
                schema: "dbo",
                columns: table => new
                {
                    class_assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    registration_id = table.Column<int>(type: "int", nullable: false),
                    assigned_by = table.Column<int>(type: "int", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClassAssignment", x => x.class_assignment_id);
                    table.ForeignKey(
                        name: "FK_Assignment_Account_assigned_by",
                        column: x => x.assigned_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Assignment_Class_class_id",
                        column: x => x.class_id,
                        principalSchema: "dbo",
                        principalTable: "Class",
                        principalColumn: "class_id");
                    table.ForeignKey(
                        name: "FK_Assignment_Registration_registration_id",
                        column: x => x.registration_id,
                        principalSchema: "dbo",
                        principalTable: "Registration",
                        principalColumn: "registration_id");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "dbo",
                columns: table => new
                {
                    payment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_id = table.Column<int>(type: "int", nullable: false),
                    amount_paid = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    method = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    transaction_reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    confirmed_by = table.Column<int>(type: "int", nullable: false),
                    paid_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    last_corrected_by = table.Column<int>(type: "int", nullable: true),
                    last_corrected_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    last_correction_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.payment_id);
                    table.CheckConstraint("CK_Payment_Amount", "[amount_paid] > 0");
                    table.CheckConstraint("CK_Payment_CorrectionBundle", "([last_corrected_by] IS NULL AND [last_corrected_at] IS NULL AND [last_correction_reason] IS NULL) OR ([last_corrected_by] IS NOT NULL AND [last_corrected_at] IS NOT NULL AND [last_correction_reason] IS NOT NULL AND LEN([last_correction_reason]) > 0)");
                    table.CheckConstraint("CK_Payment_Method", "[method] COLLATE Latin1_General_100_BIN2 IN ('CASH', 'TRANSFER') AND DATALENGTH([method]) = LEN([method])");
                    table.CheckConstraint("CK_Payment_Reference", "[method] <> 'TRANSFER' OR ([transaction_reference] IS NOT NULL AND LEN([transaction_reference]) > 0)");
                    table.ForeignKey(
                        name: "FK_Payment_Account_confirmed_by",
                        column: x => x.confirmed_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Payment_Account_last_corrected_by",
                        column: x => x.last_corrected_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Payment_Invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalSchema: "dbo",
                        principalTable: "Invoice",
                        principalColumn: "invoice_id");
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                schema: "dbo",
                columns: table => new
                {
                    attendance_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    schedule_id = table.Column<int>(type: "int", nullable: false),
                    class_assignment_id = table.Column<int>(type: "int", nullable: false),
                    present = table.Column<bool>(type: "bit", nullable: false),
                    entered_by = table.Column<int>(type: "int", nullable: false),
                    entered_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.attendance_id);
                    table.ForeignKey(
                        name: "FK_Attendance_Account_entered_by",
                        column: x => x.entered_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Attendance_Schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "schedule_id");
                    table.ForeignKey(
                        name: "FK_Attendance_StudentClassAssignment_class_assignment_id",
                        column: x => x.class_assignment_id,
                        principalSchema: "dbo",
                        principalTable: "StudentClassAssignment",
                        principalColumn: "class_assignment_id");
                });

            migrationBuilder.CreateTable(
                name: "GradeReport",
                schema: "dbo",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_assignment_id = table.Column<int>(type: "int", nullable: false),
                    final_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "DRAFT"),
                    teacher_comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    submitted_by = table.Column<int>(type: "int", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    submitted_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    reviewed_by = table.Column<int>(type: "int", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    review_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    returned_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    finalized_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeReport", x => x.report_id);
                    table.CheckConstraint("CK_GradeReport_FinalResult", "([status] = 'FINALIZED' AND [final_score] IS NOT NULL AND [final_score] >= 0 AND [final_score] <= 100 AND [finalized_at] IS NOT NULL AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL AND [finalized_at] = [reviewed_at]) OR ([status] <> 'FINALIZED' AND [final_score] IS NULL AND [finalized_at] IS NULL)");
                    table.CheckConstraint("CK_GradeReport_ReturnedReason", "[status] <> 'RETURNED' OR ([returned_reason] IS NOT NULL AND LEN([returned_reason]) > 0 AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)");
                    table.CheckConstraint("CK_GradeReport_ReviewBundle", "([reviewed_by] IS NULL AND [reviewed_at] IS NULL AND [review_operation_id] IS NULL) OR ([reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)");
                    table.CheckConstraint("CK_GradeReport_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT', 'SUBMITTED', 'RETURNED', 'FINALIZED') AND DATALENGTH([status]) = LEN([status])");
                    table.CheckConstraint("CK_GradeReport_SubmitBundle", "([submitted_by] IS NOT NULL AND [submitted_at] IS NOT NULL AND [submitted_operation_id] IS NOT NULL) OR ([status] = 'DRAFT' AND [submitted_by] IS NULL AND [submitted_at] IS NULL AND [submitted_operation_id] IS NULL)");
                    table.ForeignKey(
                        name: "FK_GradeReport_Account_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_GradeReport_Account_submitted_by",
                        column: x => x.submitted_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_GradeReport_StudentClassAssignment_class_assignment_id",
                        column: x => x.class_assignment_id,
                        principalSchema: "dbo",
                        principalTable: "StudentClassAssignment",
                        principalColumn: "class_assignment_id");
                });

            migrationBuilder.CreateTable(
                name: "Score",
                schema: "dbo",
                columns: table => new
                {
                    score_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_assignment_id = table.Column<int>(type: "int", nullable: false),
                    component_id = table.Column<int>(type: "int", nullable: false),
                    schedule_id = table.Column<int>(type: "int", nullable: true),
                    score_value = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    entered_by = table.Column<int>(type: "int", nullable: false),
                    entered_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Score", x => x.score_id);
                    table.CheckConstraint("CK_Score_Score", "[score_value] >= 0 AND [score_value] <= 100");
                    table.ForeignKey(
                        name: "FK_Score_Account_entered_by",
                        column: x => x.entered_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Score_Schedule_schedule_id",
                        column: x => x.schedule_id,
                        principalSchema: "dbo",
                        principalTable: "Schedule",
                        principalColumn: "schedule_id");
                    table.ForeignKey(
                        name: "FK_Score_ScoreComponentConfig_component_id",
                        column: x => x.component_id,
                        principalSchema: "dbo",
                        principalTable: "ScoreComponentConfig",
                        principalColumn: "component_id");
                    table.ForeignKey(
                        name: "FK_Score_StudentClassAssignment_class_assignment_id",
                        column: x => x.class_assignment_id,
                        principalSchema: "dbo",
                        principalTable: "StudentClassAssignment",
                        principalColumn: "class_assignment_id");
                });

            migrationBuilder.CreateTable(
                name: "BandAssignment",
                schema: "dbo",
                columns: table => new
                {
                    band_assignment_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    registration_id = table.Column<int>(type: "int", nullable: false),
                    best_attempt_id = table.Column<int>(type: "int", nullable: false),
                    band_id = table.Column<int>(type: "int", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BandAssignment", x => x.band_assignment_id);
                    table.ForeignKey(
                        name: "FK_BandAssignment_Band_band_id",
                        column: x => x.band_id,
                        principalSchema: "dbo",
                        principalTable: "Band",
                        principalColumn: "band_id");
                    table.ForeignKey(
                        name: "FK_BandAssignment_Registration_registration_id",
                        column: x => x.registration_id,
                        principalSchema: "dbo",
                        principalTable: "Registration",
                        principalColumn: "registration_id");
                });

            migrationBuilder.CreateTable(
                name: "EntryTestAttempt",
                schema: "dbo",
                columns: table => new
                {
                    attempt_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    registration_id = table.Column<int>(type: "int", nullable: false),
                    attempt_number = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    graded_by = table.Column<int>(type: "int", nullable: true),
                    graded_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    completed_booking_id = table.Column<int>(type: "int", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTestAttempt", x => x.attempt_id);
                    table.UniqueConstraint("AK_Attempt_IdRegistration", x => new { x.attempt_id, x.registration_id });
                    table.CheckConstraint("CK_EntryTestAttempt_Number", "[attempt_number] IN (1, 2)");
                    table.CheckConstraint("CK_EntryTestAttempt_ResultBundle", "([score] IS NULL AND [graded_by] IS NULL AND [graded_at] IS NULL AND [completed_booking_id] IS NULL) OR ([score] IS NOT NULL AND [graded_by] IS NOT NULL AND [graded_at] IS NOT NULL AND [completed_booking_id] IS NOT NULL)");
                    table.CheckConstraint("CK_EntryTestAttempt_Score", "[score] IS NULL OR ([score] >= 0 AND [score] <= 100)");
                    table.ForeignKey(
                        name: "FK_Attempt_Account_graded_by",
                        column: x => x.graded_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Attempt_Registration_registration_id",
                        column: x => x.registration_id,
                        principalSchema: "dbo",
                        principalTable: "Registration",
                        principalColumn: "registration_id");
                });

            migrationBuilder.CreateTable(
                name: "EntryTestBooking",
                schema: "dbo",
                columns: table => new
                {
                    booking_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    attempt_id = table.Column<int>(type: "int", nullable: false),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false, defaultValue: "SCHEDULED"),
                    booked_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    absent_by = table.Column<int>(type: "int", nullable: true),
                    absent_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    absent_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    cancelled_by = table.Column<int>(type: "int", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    cancelled_from_status = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: true),
                    cancellation_operation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntryTestBooking", x => x.booking_id);
                    table.UniqueConstraint("AK_Booking_IdAttempt", x => new { x.booking_id, x.attempt_id });
                    table.CheckConstraint("CK_EntryTestBooking_AbsenceBundle", "([status] = 'ABSENT' AND [absent_by] IS NOT NULL AND [absent_at] IS NOT NULL AND [absent_operation_id] IS NOT NULL) OR ([status] <> 'ABSENT' AND [absent_by] IS NULL AND [absent_at] IS NULL AND [absent_operation_id] IS NULL)");
                    table.CheckConstraint("CK_EntryTestBooking_CancellationBundle", "([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 = 'SCHEDULED' AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)");
                    table.CheckConstraint("CK_EntryTestBooking_Status", "[status] COLLATE Latin1_General_100_BIN2 IN ('SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])");
                    table.ForeignKey(
                        name: "FK_Booking_Account_absent_by",
                        column: x => x.absent_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Booking_Account_cancelled_by",
                        column: x => x.cancelled_by,
                        principalSchema: "dbo",
                        principalTable: "Account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "FK_Booking_Attempt_attempt_id",
                        column: x => x.attempt_id,
                        principalSchema: "dbo",
                        principalTable: "EntryTestAttempt",
                        principalColumn: "attempt_id");
                    table.ForeignKey(
                        name: "FK_Booking_Session_session_id",
                        column: x => x.session_id,
                        principalSchema: "dbo",
                        principalTable: "EntryTestSession",
                        principalColumn: "session_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Role_Status_Id",
                schema: "dbo",
                table: "Account",
                columns: new[] { "role_id", "status", "account_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Account_Email",
                schema: "dbo",
                table: "Account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Account_Phone",
                schema: "dbo",
                table: "Account",
                column: "phone",
                unique: true,
                filter: "[phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Assignment",
                schema: "dbo",
                table: "Attendance",
                columns: new[] { "class_assignment_id", "attendance_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EnteredBy",
                schema: "dbo",
                table: "Attendance",
                column: "entered_by");

            migrationBuilder.CreateIndex(
                name: "UQ_Attendance_ScheduleAssignment",
                schema: "dbo",
                table: "Attendance",
                columns: new[] { "schedule_id", "class_assignment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Audit_ActorTime",
                schema: "dbo",
                table: "AuditLog",
                columns: new[] { "actor_account_id", "occurred_at", "audit_id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_EntityTime",
                schema: "dbo",
                table: "AuditLog",
                columns: new[] { "entity_type", "entity_id", "occurred_at", "audit_id" },
                descending: new[] { false, false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Operation",
                schema: "dbo",
                table: "AuditLog",
                columns: new[] { "operation_id", "audit_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Time",
                schema: "dbo",
                table: "AuditLog",
                columns: new[] { "occurred_at", "audit_id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_AuthSession_Account_Revoked",
                schema: "dbo",
                table: "AuthSession",
                columns: new[] { "account_id", "revoked_at" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthSession_Expires",
                schema: "dbo",
                table: "AuthSession",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "UQ_Band_Code",
                schema: "dbo",
                table: "Band",
                column: "band_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Band_MinScore",
                schema: "dbo",
                table: "Band",
                column: "min_score",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BandAssignment_Band",
                schema: "dbo",
                table: "BandAssignment",
                column: "band_id");

            migrationBuilder.CreateIndex(
                name: "IX_BandAssignment_BestAttempt",
                schema: "dbo",
                table: "BandAssignment",
                columns: new[] { "best_attempt_id", "registration_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_BandAssignment_Registration",
                schema: "dbo",
                table: "BandAssignment",
                column: "registration_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Class_CancelledBy",
                schema: "dbo",
                table: "Class",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Class_ProgramState",
                schema: "dbo",
                table: "Class",
                columns: new[] { "program_id", "status", "class_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Class_TeacherState",
                schema: "dbo",
                table: "Class",
                columns: new[] { "teacher_id", "status", "class_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Class_Code",
                schema: "dbo",
                table: "Class",
                column: "class_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_Status_Id",
                schema: "dbo",
                table: "Course",
                columns: new[] { "status", "course_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Course_Code",
                schema: "dbo",
                table: "Course",
                column: "course_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attempt_GradedBy",
                schema: "dbo",
                table: "EntryTestAttempt",
                column: "graded_by");

            migrationBuilder.CreateIndex(
                name: "UQ_Attempt_CompletedBooking",
                schema: "dbo",
                table: "EntryTestAttempt",
                column: "completed_booking_id",
                unique: true,
                filter: "[completed_booking_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Attempt_RegistrationNumber",
                schema: "dbo",
                table: "EntryTestAttempt",
                columns: new[] { "registration_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_AbsentBy",
                schema: "dbo",
                table: "EntryTestBooking",
                column: "absent_by");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_AttemptHistory",
                schema: "dbo",
                table: "EntryTestBooking",
                columns: new[] { "attempt_id", "booked_at", "booking_id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_CancelledBy",
                schema: "dbo",
                table: "EntryTestBooking",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_SessionState",
                schema: "dbo",
                table: "EntryTestBooking",
                columns: new[] { "session_id", "status", "booking_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Booking_Completed",
                schema: "dbo",
                table: "EntryTestBooking",
                column: "attempt_id",
                unique: true,
                filter: "[status] = 'COMPLETED'");

            migrationBuilder.CreateIndex(
                name: "UQ_Booking_Current",
                schema: "dbo",
                table: "EntryTestBooking",
                column: "attempt_id",
                unique: true,
                filter: "[status] = 'SCHEDULED'");

            migrationBuilder.CreateIndex(
                name: "IX_Session_CancelledBy",
                schema: "dbo",
                table: "EntryTestSession",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Session_GraderStart",
                schema: "dbo",
                table: "EntryTestSession",
                columns: new[] { "grader_id", "start_at", "end_at", "session_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Session_RoomStart",
                schema: "dbo",
                table: "EntryTestSession",
                columns: new[] { "room", "start_at", "end_at", "session_id" });

            migrationBuilder.CreateIndex(
                name: "IX_GradeReport_ReviewedBy",
                schema: "dbo",
                table: "GradeReport",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "IX_GradeReport_StateFinalized",
                schema: "dbo",
                table: "GradeReport",
                columns: new[] { "status", "finalized_at", "report_id" });

            migrationBuilder.CreateIndex(
                name: "IX_GradeReport_SubmittedBy",
                schema: "dbo",
                table: "GradeReport",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "UQ_GradeReport_Assignment",
                schema: "dbo",
                table: "GradeReport",
                column: "class_assignment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CancelledBy",
                schema: "dbo",
                table: "Invoice",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_Issued",
                schema: "dbo",
                table: "Invoice",
                columns: new[] { "issued_at", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_StateDue",
                schema: "dbo",
                table: "Invoice",
                columns: new[] { "status", "due_date", "invoice_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Invoice_Number",
                schema: "dbo",
                table: "Invoice",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Invoice_Registration",
                schema: "dbo",
                table: "Invoice",
                column: "registration_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ConfirmedBy",
                schema: "dbo",
                table: "Payment",
                column: "confirmed_by");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_LastCorrectedBy",
                schema: "dbo",
                table: "Payment",
                column: "last_corrected_by");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Paid",
                schema: "dbo",
                table: "Payment",
                columns: new[] { "paid_at", "payment_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Payment_Invoice",
                schema: "dbo",
                table: "Payment",
                column: "invoice_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ApprovedBy",
                schema: "dbo",
                table: "Registration",
                column: "admission_approved_by");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_CancelledBy",
                schema: "dbo",
                table: "Registration",
                column: "cancelled_by");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_CourseStateCreated",
                schema: "dbo",
                table: "Registration",
                columns: new[] { "course_id", "status", "created_at", "registration_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Registration_RejectedBy",
                schema: "dbo",
                table: "Registration",
                column: "rejected_by");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_StudentCreated",
                schema: "dbo",
                table: "Registration",
                columns: new[] { "student_id", "created_at", "registration_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Registration_Unfinished",
                schema: "dbo",
                table: "Registration",
                columns: new[] { "student_id", "course_id" },
                unique: true,
                filter: "[is_unfinished] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Replay_Expiry",
                schema: "dbo",
                table: "ReplayOperation",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_Replay_Operation",
                schema: "dbo",
                table: "ReplayOperation",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "UQ_Replay_ActorActionRequest",
                schema: "dbo",
                table: "ReplayOperation",
                columns: new[] { "actor_account_id", "action_code", "request_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Role_Code",
                schema: "dbo",
                table: "Role",
                column: "role_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_ClassCurrentDate",
                schema: "dbo",
                table: "Schedule",
                columns: new[] { "class_id", "is_current", "session_date", "start_time", "schedule_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_HeldBy",
                schema: "dbo",
                table: "Schedule",
                column: "held_by");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_NotHeldBy",
                schema: "dbo",
                table: "Schedule",
                column: "not_held_by");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_RoomDate",
                schema: "dbo",
                table: "Schedule",
                columns: new[] { "room", "session_date", "start_time", "end_time", "schedule_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Schedule_CurrentFinalExam",
                schema: "dbo",
                table: "Schedule",
                column: "class_id",
                unique: true,
                filter: "[is_current] = 1 AND [session_type] = 'FINAL_EXAM'");

            migrationBuilder.CreateIndex(
                name: "UQ_Schedule_CurrentObligation",
                schema: "dbo",
                table: "Schedule",
                columns: new[] { "class_id", "obligation_key" },
                unique: true,
                filter: "[is_current] = 1");

            migrationBuilder.CreateIndex(
                name: "UQ_Schedule_Predecessor",
                schema: "dbo",
                table: "Schedule",
                column: "replaces_schedule_id",
                unique: true,
                filter: "[replaces_schedule_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Score_Component",
                schema: "dbo",
                table: "Score",
                columns: new[] { "component_id", "score_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Score_EnteredBy",
                schema: "dbo",
                table: "Score",
                column: "entered_by");

            migrationBuilder.CreateIndex(
                name: "IX_Score_Schedule",
                schema: "dbo",
                table: "Score",
                columns: new[] { "schedule_id", "score_id" },
                filter: "[schedule_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_Score_AssignmentComponent",
                schema: "dbo",
                table: "Score",
                columns: new[] { "class_assignment_id", "component_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Component_FinalExam",
                schema: "dbo",
                table: "ScoreComponentConfig",
                column: "program_id",
                unique: true,
                filter: "[kind] = 'FINAL_EXAM'");

            migrationBuilder.CreateIndex(
                name: "UQ_Component_ProgramName",
                schema: "dbo",
                table: "ScoreComponentConfig",
                columns: new[] { "program_id", "component_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_AssignedBy",
                schema: "dbo",
                table: "StudentClassAssignment",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_Class",
                schema: "dbo",
                table: "StudentClassAssignment",
                columns: new[] { "class_id", "class_assignment_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Assignment_Registration",
                schema: "dbo",
                table: "StudentClassAssignment",
                column: "registration_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_StudentProfile_Account",
                schema: "dbo",
                table: "StudentProfile",
                column: "account_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Program_Band",
                schema: "dbo",
                table: "TrainingProgram",
                column: "band_id");

            migrationBuilder.CreateIndex(
                name: "IX_Program_Status_Id",
                schema: "dbo",
                table: "TrainingProgram",
                columns: new[] { "status", "program_id" });

            migrationBuilder.CreateIndex(
                name: "UQ_Program_CourseBand",
                schema: "dbo",
                table: "TrainingProgram",
                columns: new[] { "course_id", "band_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BandAssignment_Attempt_BestAttemptRegistration",
                schema: "dbo",
                table: "BandAssignment",
                columns: new[] { "best_attempt_id", "registration_id" },
                principalSchema: "dbo",
                principalTable: "EntryTestAttempt",
                principalColumns: new[] { "attempt_id", "registration_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Attempt_Booking_CompletedBookingAttempt",
                schema: "dbo",
                table: "EntryTestAttempt",
                columns: new[] { "completed_booking_id", "attempt_id" },
                principalSchema: "dbo",
                principalTable: "EntryTestBooking",
                principalColumns: new[] { "booking_id", "attempt_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Role_role_id",
                schema: "dbo",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_Attempt_Account_graded_by",
                schema: "dbo",
                table: "EntryTestAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Account_absent_by",
                schema: "dbo",
                table: "EntryTestBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Account_cancelled_by",
                schema: "dbo",
                table: "EntryTestBooking");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTestSession_Account_cancelled_by",
                schema: "dbo",
                table: "EntryTestSession");

            migrationBuilder.DropForeignKey(
                name: "FK_EntryTestSession_Account_grader_id",
                schema: "dbo",
                table: "EntryTestSession");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Account_admission_approved_by",
                schema: "dbo",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Account_cancelled_by",
                schema: "dbo",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Account_rejected_by",
                schema: "dbo",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfile_Account_account_id",
                schema: "dbo",
                table: "StudentProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Attempt_attempt_id",
                schema: "dbo",
                table: "EntryTestBooking");

            migrationBuilder.DropTable(
                name: "Attendance",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AuditLog",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AuthSession",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BandAssignment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GradeReport",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "InvoiceNumberCounter",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ReplayOperation",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Score",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Invoice",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Schedule",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ScoreComponentConfig",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StudentClassAssignment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Class",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "TrainingProgram",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Band",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Role",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Account",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EntryTestAttempt",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EntryTestBooking",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Registration",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EntryTestSession",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Course",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StudentProfile",
                schema: "dbo");
        }
    }
}
