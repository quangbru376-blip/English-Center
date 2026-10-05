# D08 / TG04 — trg_Attendance_ClassScheduleGuard

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG04.sql

Accepted behavior:
Attendance AFTER INSERT,UPDATE,DELETE. For all assignments/classes in BOTH inserted and deleted require caller preheld Exclusive Transaction public EC:CLASS:<id> and EC:PROGRAM:<class.program_id>; NULL rejects, do not acquire gates. Reject all mutations affecting old or new Class COMPLETED/CANCELLED (including deletes and moving away from locked old row). Every inserted row schedule must belong to assignment Class, is_current=1, session_type=LESSON, occurrence_state<>NOT_HELD. Do not enforce web Teacher/start/time here; service does it later. Set-based mixed batch rejection, no audit. Error gate51031,invariant51032.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

CREATE TABLE [dbo].[Attendance] (
    [attendance_id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [class_assignment_id] int NOT NULL,
    [present] bit NOT NULL,
    [entered_by] int NOT NULL,
    [entered_at] datetime2(3) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Attendance] PRIMARY KEY ([attendance_id]),
    CONSTRAINT [FK_Attendance_Account_entered_by] FOREIGN KEY ([entered_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Attendance_Schedule_schedule_id] FOREIGN KEY ([schedule_id]) REFERENCES [dbo].[Schedule] ([schedule_id]),
    CONSTRAINT [FK_Attendance_StudentClassAssignment_class_assignment_id] FOREIGN KEY ([class_assignment_id]) REFERENCES [dbo].[StudentClassAssignment] ([class_assignment_id])
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