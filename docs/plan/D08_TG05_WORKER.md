# D08 / TG05 — trg_Score_ComponentScheduleGradeGuard

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG05.sql

Accepted behavior:
Score AFTER INSERT,UPDATE,DELETE. Every affected old/new assignment Class requires caller preheld Exclusive Transaction public EC:CLASS:<id> and EC:PROGRAM:<id>; NULL rejects; do not acquire gates. Reject any mutation when old OR new assignment GradeReport SUBMITTED/FINALIZED (missing report does not invent lock). Validate inserted component belongs to assignment Class.program_id. GENERAL requires NULL schedule; LESSON/FINAL_EXAM requires NONNULL current same-Class schedule of exact matching session_type and occurrence_state<>NOT_HELD. Validate joins explicitly so NULL/missing joined schedule cannot pass UNKNOWN. Do not require Class non-COMPLETED because grade editing after completed until submit is allowed. No parent report touch/audit/auth here (future TX11). Error gate51041,invariant51042.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

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