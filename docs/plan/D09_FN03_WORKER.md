# D09 / FN03 — ufn_RegistrationUnfinished

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN03.sql

Accepted behavior:
Inline TVF @registrationId int, 0/1 row isUnfinished bit. Missing Registration0 rows. CANCELLED/REJECTED false; otherwise false iff assigned Class COMPLETED and own GradeReport FINALIZED. Passing independent; do not read stored is_unfinished as source truth. Use EXISTS not multiplying rows.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

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