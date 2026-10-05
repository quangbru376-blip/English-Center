# D08 / TG03 — trg_Band_ThresholdSetFreezeGuard

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG03.sql

Accepted behavior:
Band AFTER INSERT,UPDATE,DELETE. Require caller preheld Exclusive Transaction public EC:BANDSET (NULL rejects), do not acquire after DML. After any completed EntryTestAttempt (score IS NOT NULL, completed_booking_id IS NOT NULL), reject any insert/delete or band_code/min_score change. Compare by band_id (not UPDATE(column)); exact code comparison BIN2 plus DATALENGTH, no-op accepted. Other permitted field change not frozen (table schema below). First-result callers in future must use same BANDSET before DML; do not claim trigger alone prevents bypass by unrelated direct first-result writer. Error gate51021,invariant51022.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

CREATE TABLE [dbo].[Band] (
    [band_id] int NOT NULL IDENTITY,
    [band_code] varchar(30) NOT NULL,
    [min_score] decimal(5,2) NOT NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_Band] PRIMARY KEY ([band_id]),
    CONSTRAINT [CK_Band_Code] CHECK (DATALENGTH([band_code]) BETWEEN 1 AND 30 AND [band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%' AND LEN([band_code]) > 0),
    CONSTRAINT [CK_Band_MinScore] CHECK ([min_score] >= 0 AND [min_score] <= 100)
);

CREATE TABLE [dbo].[EntryTestAttempt] (
    [attempt_id] int NOT NULL IDENTITY,
    [registration_id] int NOT NULL,
    [attempt_number] int NOT NULL,
    [score] decimal(5,2) NULL,
    [graded_by] int NULL,
    [graded_at] datetime2(3) NULL,
    [completed_booking_id] int NULL,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_EntryTestAttempt] PRIMARY KEY ([attempt_id]),
    CONSTRAINT [AK_Attempt_IdRegistration] UNIQUE ([attempt_id], [registration_id]),
    CONSTRAINT [CK_EntryTestAttempt_Number] CHECK ([attempt_number] IN (1, 2)),
    CONSTRAINT [CK_EntryTestAttempt_ResultBundle] CHECK (([score] IS NULL AND [graded_by] IS NULL AND [graded_at] IS NULL AND [completed_booking_id] IS NULL) OR ([score] IS NOT NULL AND [graded_by] IS NOT NULL AND [graded_at] IS NOT NULL AND [completed_booking_id] IS NOT NULL)),
    CONSTRAINT [CK_EntryTestAttempt_Score] CHECK ([score] IS NULL OR ([score] >= 0 AND [score] <= 100)),
    CONSTRAINT [FK_Attempt_Account_graded_by] FOREIGN KEY ([graded_by]) REFERENCES [dbo].[Account] ([account_id]),
    CONSTRAINT [FK_Attempt_Registration_registration_id] FOREIGN KEY ([registration_id]) REFERENCES [dbo].[Registration] ([registration_id])
);