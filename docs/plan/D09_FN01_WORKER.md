# D09 / FN01 — ufn_EffectiveEntryTestState

Implementation skills: backend-clean-code; review: code-review (Codex only).
Read COMPLETE this task card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. Read ONLY these three paths and the exact output path if it exists. Do not follow links or read AGENTS/other cards/memory/source. No commands, search/scans, network, SQL execution, Git, MCP, browser, subagents or secrets. Only view_file and write_to_file/replace_file_content. Codex independently validates/reviews/applies.
Accepted overrides: SQL Server2025 Express compatibility160, exact E03/E04 behavior, no SQLite substitute, no generic API changes or seed/audit logic. No packages. SQL file contains one CREATE OR ALTER object, SET NOCOUNT ON for trigger, no GO/USE/transaction BEGIN/COMMIT/ROLLBACK (BEGIN block allowed), no grants or time functions. THROW for violations, no result sets in triggers, set-based multi-row logic (no scalar row assignment/cursors). No migration files: manager integrates through EF migration.

Exact authorized output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN01.sql

Accepted behavior:
Scalar @status varchar(16),@startAt datetime2(3),@asOfUtc datetime2(3) -> varchar(16). CANCELLED first unchanged; now>=start CLOSED else stored status. Inputs nonnull contract, no GETDATE/state mutations.

Schema source: manager copied exact generated D07 initial SQL table definitions. No links may be followed.

