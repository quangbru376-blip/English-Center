# D11 / SP03 — usp_GetClassGradeReview

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP03.sql

Accepted exact signature/result contract:
dbo.usp_GetClassGradeReview → API-144 và authorized review reads

Parameters classId int N, actorId int N. Actor AcademicAffairs NORMAL/service
current access. All result sets cùng short SNAPSHOT, non-DRAFT values only.
Parent metadata/reference versions khác Grade DRAFT fields không phải override.

RS1 một row: classId/programId/teacherId int N, classCode varchar(30) N,
classStatus E03(T14.status) N, classVersion/programVersion binary(8) N,
cohortSize/visibleReportCount int N. Nếu không visible report thì service404.
Minimal parent Course/Band/Teacher references lấy trong cùng snapshot theo
AcademicClassSummary D10 bằng EF, không connection riêng sau snapshot.
RS2 full config: componentId int N, name nvarchar(200) N, kind varchar(16) N,
weight decimal(5,2) N, version binary(8) N; order componentId.
RS3 visible reports: classAssignmentId/registrationId/studentId/reportId int N,
fullName nvarchar(200) N, status varchar(16) N SUBMITTED/RETURNED/FINALIZED,
teacherComment nvarchar(2000) Y, submittedAt/reviewedAt/finalizedAt datetime2(3) Y,
latestReturnReason nvarchar(2000) Y, finalScore decimal(5,2) Y,
version binary(8) N, requiredComponentCount/enteredComponentCount int N,
isComplete bit N, total decimal(5,2) Y. Order classAssignmentId.
RS4 visible saved scores: classAssignmentId/scoreId/componentId int N,
score decimal(5,2) N, scheduleId int Y, enteredByAccountId int N,
enteredAt datetime2(3) N, version binary(8) N; order assignment/component.
RS5 linked schedule refs only visible score links: scheduleId/classId int N,
type varchar(16) N, obligationKey uniqueidentifier N, date date N,
startTime/endTime time(0) N, room varchar(30) N,
occurrenceState varchar(16) N, isCurrent bit N derived,
replacesScheduleId int Y, version binary(8) N; order scheduleId.
Service derives missingComponentIds from RS2−visible saved component set,
startAt/endAt from VN date/time; no DRAFT Score token in any RS.
No per-report action/status setter; full-cohort preview separately reloads guards.


Manager integration requirements:
AA ACTIVE normal only. Missing class/no visible (SUBMITTED/RETURNED/FINALIZED) report THROW51102 before any result. RS1 cohortSize COUNT all assigned learners (int), visible count int. Config RS2 all components. RS3 visible report only joined current student Account, latestReturnReason=returned_reason; required/entered/isComplete/total FN05. RS4 only scores whose own report visible (DRAFT assignments no score token). RS5 DISTINCT Schedule referenced by these visible saved scores, including noncurrent/historical occurrence refs; do NOT use active view which hides old links. No DRAFT comments/versions/scores. All5sets exact columns/order and binary8 versions, no extra parent metadata.


Exact initial schema:

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

CREATE TABLE [dbo].[Role] (
    [role_id] int NOT NULL IDENTITY,
    [role_code] varchar(32) NOT NULL,
    [role_name] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([role_id]),
    CONSTRAINT [CK_Role_Code] CHECK ([role_code] COLLATE Latin1_General_100_BIN2 IN ('Student', 'Admissions', 'AcademicAffairs', 'Teacher', 'Accountant', 'Director', 'Admin') AND DATALENGTH([role_code]) = LEN([role_code]))
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

CREATE TABLE [dbo].[ScoreComponentConfig] (
    [component_id] int NOT NULL IDENTITY,
    [program_id] int NOT NULL,
    [component_name] nvarchar(200) COLLATE Latin1_General_100_CI_AS_SC NOT NULL,
    [weight] decimal(5,2) NOT NULL,
    [kind] varchar(16) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_ScoreComponentConfig] PRIMARY KEY ([component_id]),
    CONSTRAINT [CK_ScoreComponentConfig_Kind] CHECK ([kind] COLLATE Latin1_General_100_BIN2 IN ('GENERAL','LESSON','FINAL_EXAM') AND DATALENGTH([kind]) = LEN([kind])),
    CONSTRAINT [CK_ScoreComponentConfig_Name] CHECK (LEN([component_name]) > 0),
    CONSTRAINT [CK_ScoreComponentConfig_Weight] CHECK ([weight] > 0 AND [weight] <= 100),
    CONSTRAINT [FK_ScoreComponentConfig_TrainingProgram_program_id] FOREIGN KEY ([program_id]) REFERENCES [dbo].[TrainingProgram] ([program_id])
);

CREATE TABLE [dbo].[StudentClassAssignment] (
    [class_assignment_id] int NOT NULL IDENTITY,
    [class_id] int NOT NULL,
    [registration_id] int NOT NULL,
    [assigned_by] int NOT NULL,
    [assigned_at] datetime2(3) NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_StudentClassAssignment] PRIMARY KEY ([class_assignment_id]),
    CONSTRAINT [FK_Assignment_Account_assigned_by] FOREIGN KEY ([assigned_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Assignment_Class_class_id] FOREIGN KEY ([class_id]) REFERENCES [dbo].[Class] ([class_id]),
    CONSTRAINT [FK_Assignment_Registration_registration_id] FOREIGN KEY ([registration_id]) REFERENCES [dbo].[Registration] ([registration_id])
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

CREATE TABLE [dbo].[StudentProfile] (
    [student_id] int NOT NULL IDENTITY,
    [account_id] int NOT NULL,
    [date_of_birth] date NOT NULL,
    [address] nvarchar(510) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_StudentProfile] PRIMARY KEY ([student_id]),
    CONSTRAINT [FK_StudentProfile_Account_account_id] FOREIGN KEY ([account_id]) REFERENCES [dbo].[Account] ([account_id])
);

CREATE TABLE [dbo].[GradeReport] (
    [report_id] int NOT NULL IDENTITY,
    [class_assignment_id] int NOT NULL,
    [final_score] decimal(5,2) NULL,
    [status] varchar(16) NOT NULL DEFAULT 'DRAFT',
    [teacher_comment] nvarchar(2000) NULL,
    [submitted_by] int NULL,
    [submitted_at] datetime2(3) NULL,
    [submitted_operation_id] uniqueidentifier NULL,
    [reviewed_by] int NULL,
    [reviewed_at] datetime2(3) NULL,
    [review_operation_id] uniqueidentifier NULL,
    [returned_reason] nvarchar(2000) NULL,
    [finalized_at] datetime2(3) NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_GradeReport] PRIMARY KEY ([report_id]),
    CONSTRAINT [CK_GradeReport_FinalResult] CHECK (([status] = 'FINALIZED' AND [final_score] IS NOT NULL AND [final_score] >= 0 AND [final_score] <= 100 AND [finalized_at] IS NOT NULL AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL AND [finalized_at] = [reviewed_at]) OR ([status] <> 'FINALIZED' AND [final_score] IS NULL AND [finalized_at] IS NULL)),
    CONSTRAINT [CK_GradeReport_ReturnedReason] CHECK ([status] <> 'RETURNED' OR ([returned_reason] IS NOT NULL AND LEN([returned_reason]) > 0 AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)),
    CONSTRAINT [CK_GradeReport_ReviewBundle] CHECK (([reviewed_by] IS NULL AND [reviewed_at] IS NULL AND [review_operation_id] IS NULL) OR ([reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL)),
    CONSTRAINT [CK_GradeReport_Status] CHECK ([status] COLLATE Latin1_General_100_BIN2 IN ('DRAFT', 'SUBMITTED', 'RETURNED', 'FINALIZED') AND DATALENGTH([status]) = LEN([status])),
    CONSTRAINT [CK_GradeReport_SubmitBundle] CHECK (([submitted_by] IS NOT NULL AND [submitted_at] IS NOT NULL AND [submitted_operation_id] IS NOT NULL) OR ([status] = 'DRAFT' AND [submitted_by] IS NULL AND [submitted_at] IS NULL AND [submitted_operation_id] IS NULL)),
    CONSTRAINT [FK_GradeReport_Account_reviewed_by] FOREIGN KEY ([reviewed_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_GradeReport_Account_submitted_by] FOREIGN KEY ([submitted_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_GradeReport_StudentClassAssignment_class_assignment_id] FOREIGN KEY ([class_assignment_id]) REFERENCES [dbo].[StudentClassAssignment] ([class_assignment_id])
);

CREATE TABLE [dbo].[Score] (
    [score_id] int NOT NULL IDENTITY,
    [class_assignment_id] int NOT NULL,
    [component_id] int NOT NULL,
    [schedule_id] int NULL,
    [score_value] decimal(5,2) NOT NULL,
    [entered_by] int NOT NULL,
    [entered_at] datetime2(3) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Score] PRIMARY KEY ([score_id]),
    CONSTRAINT [CK_Score_Score] CHECK ([score_value] >= 0 AND [score_value] <= 100),
    CONSTRAINT [FK_Score_Account_entered_by] FOREIGN KEY ([entered_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Score_Schedule_schedule_id] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[Schedule] ([schedule_id]),
    CONSTRAINT [FK_Score_ScoreComponentConfig_component_id] FOREIGN KEY ([component_id]) REFERENCES [dbo].[ScoreComponentConfig] ([component_id]),
    CONSTRAINT [FK_Score_StudentClassAssignment_class_assignment_id] FOREIGN KEY ([class_assignment_id]) REFERENCES [dbo].[StudentClassAssignment] ([class_assignment_id])
);

CREATE TABLE [dbo].[Schedule] (
    [schedule_id] int NOT NULL IDENTITY,
    [class_id] int NOT NULL,
    [session_type] varchar(16) NOT NULL,
    [obligation_key] uniqueidentifier NOT NULL,
    [replaces_schedule_id] int NULL,
    [is_current] bit NOT NULL DEFAULT CAST(1 AS bit),
    [session_date] date NOT NULL,
    [start_time] time(0) NOT NULL,
    [end_time] time(0) NOT NULL,
    [room] varchar(30) NOT NULL,
    [occurrence_state] varchar(16) NOT NULL DEFAULT 'PLANNED',
    [held_by] int NULL,
    [held_at] datetime2(3) NULL,
    [not_held_by] int NULL,
    [not_held_at] datetime2(3) NULL,
    [not_held_reason] nvarchar(2000) NULL,
    [replacement_operation_id] uniqueidentifier NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Schedule] PRIMARY KEY ([schedule_id]),
    CONSTRAINT [CK_Schedule_CurrentState] CHECK (([occurrence_state] IN ('PLANNED', 'HELD') AND [is_current] = 1) OR ([occurrence_state] = 'NOT_HELD' AND [is_current] = 0)),
    CONSTRAINT [CK_Schedule_HeldBundle] CHECK (([occurrence_state] = 'HELD' AND [held_by] IS NOT NULL AND [held_at] IS NOT NULL) OR ([occurrence_state] <> 'HELD' AND [held_by] IS NULL AND [held_at] IS NULL)),
    CONSTRAINT [CK_Schedule_NoSelfReplacement] CHECK ([replaces_schedule_id] IS NULL OR [replaces_schedule_id] <> [schedule_id]),
    CONSTRAINT [CK_Schedule_NotHeldBundle] CHECK (([occurrence_state] = 'NOT_HELD' AND [not_held_by] IS NOT NULL AND [not_held_at] IS NOT NULL AND [not_held_reason] IS NOT NULL AND LEN([not_held_reason]) > 0 AND [replacement_operation_id] IS NOT NULL) OR ([occurrence_state] <> 'NOT_HELD' AND [not_held_by] IS NULL AND [not_held_at] IS NULL AND [not_held_reason] IS NULL AND [replacement_operation_id] IS NULL)),
    CONSTRAINT [CK_Schedule_Room] CHECK (DATALENGTH([room]) BETWEEN 1 AND 30 AND DATALENGTH([room]) = LEN([room]) AND [room] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'),
    CONSTRAINT [CK_Schedule_State] CHECK ([occurrence_state] COLLATE Latin1_General_100_BIN2 IN ('PLANNED', 'HELD', 'NOT_HELD') AND DATALENGTH([occurrence_state]) = LEN([occurrence_state])),
    CONSTRAINT [CK_Schedule_Time] CHECK ([start_time] < [end_time] AND DATEPART(second, [start_time]) = 0 AND DATEPART(second, [end_time]) = 0),
    CONSTRAINT [CK_Schedule_Type] CHECK ([session_type] COLLATE Latin1_General_100_BIN2 IN ('LESSON', 'FINAL_EXAM') AND DATALENGTH([session_type]) = LEN([session_type])),
    CONSTRAINT [FK_Schedule_Account_held_by] FOREIGN KEY ([held_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Schedule_Account_not_held_by] FOREIGN KEY ([not_held_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Schedule_Class_class_id] FOREIGN KEY ([class_id]) REFERENCES [dbo].[Class] ([class_id]),
    CONSTRAINT [FK_Schedule_Schedule_replaces_schedule_id] FOREIGN KEY ([replaces_schedule_id]) REFERENCES [dbo].[Schedule] ([schedule_id])
);

CREATE OR ALTER FUNCTION dbo.ufn_GradeTotal
(
    @classAssignmentId int
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        CAST(agg.requiredComponentCount AS int) AS requiredComponentCount,
        CAST(agg.enteredComponentCount AS int) AS enteredComponentCount,
        CAST(CASE
            WHEN agg.requiredComponentCount > 0 AND agg.enteredComponentCount = agg.requiredComponentCount THEN 1
            ELSE 0
        END AS bit) AS isComplete,
        CASE
            WHEN agg.requiredComponentCount > 0 AND agg.enteredComponentCount = agg.requiredComponentCount
                THEN CAST(ROUND(agg.weightedSum / 100.0, 2) AS decimal(5,2))
            ELSE CAST(NULL AS decimal(5,2))
        END AS total
    FROM dbo.StudentClassAssignment sca
    INNER JOIN dbo.Class c ON c.class_id = sca.class_id
    CROSS APPLY
    (
        SELECT
            COUNT(DISTINCT scc.component_id) AS requiredComponentCount,
            COUNT(DISTINCT s.component_id) AS enteredComponentCount,
            SUM(s.score_value * scc.weight) AS weightedSum
        FROM dbo.ScoreComponentConfig scc
        LEFT JOIN dbo.Score s ON s.class_assignment_id = sca.class_assignment_id
                             AND s.component_id = scc.component_id
        WHERE scc.program_id = c.program_id
    ) agg
    WHERE sca.class_assignment_id = @classAssignmentId
);
