# D09 / FN02 — ufn_BandForScore

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN02.sql

Accepted behavior:
Inline TVF @score decimal(5,2). Return TOP(1) band_id AS bandId,min_score AS minScore greatest threshold <=score ORDER BY min_score DESC. Missing threshold 0 rows. Caller validates 0..100 (may defensively filter range), no guessed seed/default.

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