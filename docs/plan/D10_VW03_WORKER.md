# D10 / VW03 — vw_CurrentScheduleOccurrences

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/memory/other source. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQL Server160, exact decimal/NULL/time/full-set accepted E03/E04 overrides stale SQLite/API/seed guidance. No packages, grants, migration files, GO/USE, GETDATE, audit, or mutation. One CREATE OR ALTER VIEW, explicit projection, no SELECT *. CamelCase view aliases specified below are manager integration choices, not web rights.

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW03.sql

One current Schedule row per obligation: Schedule.is_current=1 and Class.status<>CANCELLED. Exact aliases scheduleId,classId,classCode,classStatus,teacherId,type,obligationKey,date,startTime,endTime,room,occurrenceState,replacesScheduleId,version. version Schedule.row_version. Retain all current occurrence states including NOT_HELD; occupancy consumers later apply state/time filter. No GETDATE, no history rows, no filtering completed classes or HELD by wall clock.

Exact initial schema:

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