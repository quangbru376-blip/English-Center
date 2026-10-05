# D11 / SP05 — usp_GetAdmissionsReport

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP05.sql

Accepted exact signature/result contract:
dbo.usp_GetAdmissionsReport → API-152

Parameters fromUtc/toExclusiveUtc datetime2(3) N, courseId int Y,
status E03(T09.status) Y, page/pageSize int N.
RS1 totalCount/registrationCount/distinctStudentCount bigint N (one row).
RS2 courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
registrationCount/distinctStudentCount bigint N; order code/id ASC.
RS3 options same SP04. Whole distinct computed independently, not sum groups;
no Registration/Student/contact/Band score rows. Empty counts0, groups[].


Manager integration requirements:
Datefrom<to/page>=1,size1..100/status enumall7. Cohort createdAt halfopen +currentRegistrationstatus+course optional. Group onlyCoursewithfacts. COUNT_BIG(*) registration; distinctStudent COUNT_BIG(DISTINCT studentId) independentlywholecohort, NOT sumgroupdistinct. RS1totalCount numberofCoursegroups, notRegistrationcount. FullCourseoptions shared scope. No identifiableoutputs.
Full report Course options (accepted DTO31.1): ACTIVE Courses plus INACTIVE Courses referenced by retained Registration or Class/Program/Invoice facts, independent current filters/page. Do not return unreferenced inactive Course. Order code/id. Invoice/Registration factcoursecoversallregistrations; Class/Program adds referencedcourse. ExactAlias courseId,courseCode,courseName,status.


Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW01.sql

One row/Registration. Inner join its StudentProfile/Account/Course; left join unique BandAssignment/Band, Invoice, Assignment/Class. Exact aliases: registrationId,studentId,fullName,email,phone,courseId,courseCode,courseName,courseStatus,registrationStatus,createdAt,admissionConfirmedAt,admissionApprovedAt,bandId,bandCode,bandMinScore,invoiceId,invoiceNumber,invoiceAmount,invoiceStatus,classId,assignmentId,classCode,classStatus,registrationVersion. Source current Account full_name/email/phone; registration confirmation/approval timestamps; version its row_version. No DOB/address/password/profile snapshot. All nullable joined bundles reflect real absence; do not coalesce. Include every registration status/current course status. Unique joins preserve grain.

Exact initial schema:

CREATE TABLE [dbo].[Course] (
    [course_id] int NOT NULL IDENTITY,
    [course_code] varchar(30) NOT NULL,
    [course_name] nvarchar(300) NOT NULL,
    [base_price] decimal(18,0) NOT NULL,
    [status] varchar(16) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Course] PRIMARY KEY ([course_id]),
    CONSTRAINT [CK_Course_Code] CHECK (DATALENGTH([course_code]) BETWEEN 1 AND 30 AND DATALENGTH([course_code]) = LEN([course_code]) AND [course_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_Course_Price] CHECK ([base_price] > 0),
    CONSTRAINT [CK_Course_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status]))
);

CREATE TABLE [dbo].[Registration] (
    [registration_id] int NOT NULL IDENTITY,
    [student_id] int NOT NULL,
    [course_id] int NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'PENDING',
    [is_unfinished] bit NOT NULL DEFAULT CAST(1 AS bit),
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [admission_confirmed_at] datetime2(3) NULL,
    [admission_approved_by] int NULL,
    [admission_approved_at] datetime2(3) NULL,
    [rejected_by] int NULL,
    [rejected_at] datetime2(3) NULL,
    [rejection_reason] nvarchar(2000) NULL,
    [rejected_from_status] varchar(16) NULL,
    [rejection_operation_id] uniqueidentifier NULL,
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancelled_from_status] varchar(16) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Registration] PRIMARY KEY ([registration_id]),
    CONSTRAINT [CK_Registration_ApprovedBundle] CHECK (([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL) OR ([admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL)),
    CONSTRAINT [CK_Registration_CancelledBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancelled_from_status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED') AND DATALENGTH([cancelled_from_status]) = LEN([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_Registration_RejectedBundle] CHECK (([status] = 'REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND LEN([rejection_reason]) > 0 AND [rejected_from_status] IS NOT NULL AND [rejected_from_status] COLLATE Latin1_General_100_BIN2 = 'CONFIRMED' AND DATALENGTH([rejected_from_status]) = LEN([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL) OR ([status] <> 'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL)),
    CONSTRAINT [CK_Registration_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_Registration_Account_admission_approved_by] FOREIGN KEY ([admission_approved_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Account_rejected_by] FOREIGN KEY ([rejected_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Registration_Course_course_id] FOREIGN KEY ([course_id]) REFERENCES [dbo].[Course] ([course_id]),
    CONSTRAINT [FK_Registration_StudentProfile_student_id] FOREIGN KEY ([student_id]) REFERENCES [dbo].[StudentProfile] ([student_id])
);

CREATE TABLE [dbo].[Class] (
    [class_id] int NOT NULL IDENTITY,
    [program_id] int NOT NULL,
    [teacher_id] int NOT NULL,
    [class_code] varchar(30) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'PLANNED',
    [max_size] int NOT NULL,
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [cancelled_by] int NULL,
    [cancelled_at] datetime2(3) NULL,
    [cancellation_reason] nvarchar(2000) NULL,
    [cancellation_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Class] PRIMARY KEY ([class_id]),
    CONSTRAINT [CK_Class_CancellationBundle] CHECK (([status] = 'CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND LEN([cancellation_reason]) > 0 AND [cancellation_operation_id] IS NOT NULL) OR ([status] <> 'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL)),
    CONSTRAINT [CK_Class_Code] CHECK (DATALENGTH([class_code]) BETWEEN 1 AND 30 AND DATALENGTH([class_code]) = LEN([class_code]) AND [class_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_Class_MaxSize] CHECK ([max_size] BETWEEN 1 AND 15),
    CONSTRAINT [CK_Class_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_Class_Account_cancelled_by] FOREIGN KEY ([cancelled_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Class_Account_teacher_id] FOREIGN KEY ([teacher_id]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Class_TrainingProgram_program_id] FOREIGN KEY ([program_id]) REFERENCES [dbo].[TrainingProgram] ([program_id])
);

CREATE TABLE [dbo].[TrainingProgram] (
    [program_id] int NOT NULL IDENTITY,
    [course_id] int NOT NULL,
    [band_id] int NOT NULL,
    [program_name] nvarchar(300) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'DRAFT',
    [expected_lesson_count] int NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_TrainingProgram] PRIMARY KEY ([program_id]),
    CONSTRAINT [CK_TrainingProgram_ExpectedLessons] CHECK ([expected_lesson_count] > 0),
    CONSTRAINT [CK_TrainingProgram_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT','ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [FK_TrainingProgram_Band_band_id] FOREIGN KEY ([band_id]) REFERENCES [dbo].[Band] ([band_id]),
    CONSTRAINT [FK_TrainingProgram_Course_course_id] FOREIGN KEY ([course_id]) REFERENCES [dbo].[Course] ([course_id])
);