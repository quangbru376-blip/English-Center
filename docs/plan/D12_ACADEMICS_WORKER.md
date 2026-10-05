# D12 ACADEMICS — ec_academics_reporting

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md BEFORE writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex tests/reviews/applies. E03/E04 SQLServer160/object rights override stale SQLite/API/seed guidance. Skills grant no extra authority.

Write ONLY backend/src/EnglishCenter.Api/Infrastructure/Sql/D12/ACADEMICS.sql, at most220lines. Static batch: no GO/USE/transactions/dynamic SQL/modules/migration/users/logins/membership/business DML. Start fail-if-DATABASE_PRINCIPAL_ID('ec_academics_reporting') IS NOT NULL THROW51212. CREATE ROLE [ec_academics_reporting] AUTHORIZATION [dbo]. Never adopt existing role. Explicit dbo object-level permissions only:
GRANT SELECT on dbo.vw_CurrentScheduleOccurrences and dbo.vw_TrainingClassFacts.
GRANT EXECUTE on dbo.usp_GetTrainingReport.
DENY SELECT on each dbo.Account, dbo.StudentProfile, dbo.Score, dbo.GradeReport.
No other grants/DENYs/objects/schema/database/server permissions or fixed-role membership. Invoice/Payment and other base tables remain ungranted; no grade DML, credentials or runtime identity/config.
Ownership chaining permits accepted views/procedure despite base DENY; DENY alone is not output privacy proof. Codex tests actual allowed output and forbidden direct operations.
