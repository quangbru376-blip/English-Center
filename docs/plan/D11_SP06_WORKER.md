# D11 / SP06 — usp_GetTrainingReport

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP06.sql

Accepted exact signature/result contract:
dbo.usp_GetTrainingReport → API-153

Parameters courseId/bandId/classId/teacherId int Y, status E03(T14.status) Y,
finalizedFromUtc/finalizedToExclusiveUtc datetime2(3) N, page/pageSize int N.
Inventory date-free, current Teacher for both cohorts.
RS1 totalCount/classCount/plannedCount/openCount/ongoingCount/completedCount/
cancelledCount/finalizedResultCount/passingResultCount bigint N,
passRatePercent decimal(5,2) Y (one row).
RS2 one matched-Class page row (same inventory/results group key):
classId/courseId/bandId int N, classCode/courseCode varchar(30) N,
courseName nvarchar(300) N, bandCode E03(T06.band_code) N,
bandMinScore decimal(5,2) N, classStatus E03(T14.status) N,
maxSize/placedCount/expectedCount int N, heldCount bigint Y,
progressPercent decimal(5,2) Y, examCurrentCount/examHeldCount int Y,
finalizedResultCount/passingResultCount bigint N, passRatePercent decimal(5,2) Y.
CANCELLED held/progress/exam counts NULL; results still finalized-cohort semantics.
Order classCode/classId ASC. RS3 courses same SP04;
RS4 bands bandId int N, bandCode E03(T06.band_code) N,minScore decimal(5,2) N;
RS5 classes classId/courseId/bandId int N,classCode varchar(30) N;
RS6 teachers teacherId int N,displayName nvarchar(200) N, referenced current
Class Teachers only. Full option sets no pagination. Service splits RS2 into
same-group pages; no single Student/Score/GradeReport identifiers.


Manager integration requirements:
Inventory allClassmatched optionalcourse/band/class/currentTeacher/status; independent finalization dates. Results FINALIZED finalizedAt halfopen among SAME matchedClassset, passing storedfinalScore>=50. Separate aggregate first toavoid assignment/schedule/report multiplication. Totals andcounts bigint across full matchedset, five Classstates5keyszero. Ratios decimal arithmetic roundhalfup2; denominator0NULL; progress100*held/expected (programpositive). Cancelled row held/progress/exam counts typedNULL but expected/max/placed/state and finalizedresults retained. Pageinventory/results samerowClass/ordercode/id. Fulloptions: Course scopedbelow; allBands; allretainedClasses+Course/Band; TeacherAccounts currently referencedanyClass inclinactive (full option set independentfilters/paging). Output exact6resultsets names/types (no student/report/score IDs). No extra metrics or datefilteredinventory.
Full report Course options (accepted DTO31.1): ACTIVE Courses plus INACTIVE Courses referenced by retained Registration or Class/Program/Invoice facts, independent current filters/page. Do not return unreferenced inactive Course. Order code/id. Invoice/Registration factcoursecoversallregistrations; Class/Program adds referencedcourse. ExactAlias courseId,courseCode,courseName,status.


Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW04.sql

One row/Class including CANCELLED. Join Program/Course/Band, CROSS APPLY dbo.ufn_ClassObligationProgress(class_id) exact FN04. Count placed assignments separately (aggregate before joining; explicit COUNT and int result) to avoid schedules x assignments multiplication. Aliases classId,programId,teacherId,classCode,courseId,courseCode,courseName,courseStatus,bandId,bandCode,bandMinScore,classStatus,maxSize,placedCount,expectedCount,currentCount,heldCount,examCurrentCount,examHeldCount. expectedCount int=FN expectedLessonCount; currentCount/heldCount bigint; exams int. Preserve raw cancelled counts; SP06 later NULLs cancelled progress. No calculated progress percent here.

Referenced view projection contract (views generated separately):
backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW05.sql

One FINALIZED GradeReport/enrollment; WHERE report.status=FINALIZED only. Join assignment/registration/class/program. Exact aliases reportId,classAssignmentId,registrationId,studentId,classId,courseId,bandId,teacherId,finalScore,isPassing,finalizedAt. isPassing CAST(CASE WHEN final_score>=50 THEN1 ELSE0 END AS bit). Keep cancelled-class finalized history and immutable stored final_score (never recompute from editable scores). No DRAFT/SUBMITTED/RETURNED values or comments/contacts.

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

CREATE TABLE [dbo].[Band] (
    [band_id] int NOT NULL IDENTITY,
    [band_code] varchar(30) NOT NULL,
    [min_score] decimal(5,2) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Band] PRIMARY KEY ([band_id]),
    CONSTRAINT [CK_Band_Code] CHECK (DATALENGTH([band_code]) BETWEEN 1 AND 30 AND [band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%' AND LEN([band_code]) > 0),
    CONSTRAINT [CK_Band_MinScore] CHECK ([min_score] >= 0 AND [min_score] <= 100)
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

CREATE TABLE [dbo].[Account] (
    [account_id] int NOT NULL IDENTITY,
    [role_id] int NOT NULL,
    [email] varchar(254) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    [phone] varchar(10) NULL,
    [password_hash] varchar(512) NOT NULL,
    [full_name] nvarchar(200) NOT NULL,
    [status] varchar(16) NOT NULL DEFAULT 'ACTIVE',
    [created_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    [security_version] bigint NOT NULL DEFAULT CAST(1 AS bigint),
    [failed_login_count] int NOT NULL DEFAULT 0,
    [failure_window_started_at] datetime2(3) NULL,
    [login_throttled_until] datetime2(3) NULL,
    [must_change_password] bit NOT NULL,
    [temp_issued_at] datetime2(3) NULL,
    [temp_expires_at] datetime2(3) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Account] PRIMARY KEY ([account_id]),
    CONSTRAINT [CK_Account_FailedLoginCount] CHECK ([failed_login_count] >= 0),
    CONSTRAINT [CK_Account_Phone] CHECK ([phone] IS NULL OR (DATALENGTH([phone]) = 10 AND LEFT([phone], 1) = '0' AND [phone] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^0-9]%')),
    CONSTRAINT [CK_Account_SecurityVersion] CHECK ([security_version] > 0),
    CONSTRAINT [CK_Account_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE', 'LOCKED', 'DISABLED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [CK_Account_TempBundle] CHECK (([must_change_password] = 1 AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at] = DATEADD(hour, 24, [temp_issued_at])) OR ([must_change_password] = 0 AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL)),
    CONSTRAINT [FK_Account_Role_role_id] FOREIGN KEY ([role_id]) REFERENCES [dbo].[Role] ([role_id])
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