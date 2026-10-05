# D08 / TG02 — trg_Component_UsedConfigurationGuard

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG02.sql

Accepted behavior:
ScoreComponentConfig AFTER INSERT,UPDATE,DELETE. For every affected old/new program require caller preheld Exclusive Transaction public EC:PROGRAM:<id> (NULL must reject), do not acquire gates after DML. For used program any Class including cancelled, reject insert/delete/move program/change component_name/weight/kind. Compare inserted/deleted by component_id; consider BOTH old and new program IDs; exact name change including case must be detected with BIN2+DATALENGTH. No-op update allowed; unused program scratch names/neutralization/delete and additions allowed for later full-set service config. No audit/weight-sum restriction here. Error gate51011,invariant51012.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

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