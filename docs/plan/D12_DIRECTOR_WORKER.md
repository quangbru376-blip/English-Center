# D12 DIRECTOR — ec_director_reporting

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md BEFORE writing. Read ONLY these three paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex tests/reviews/applies. E03/E04 SQLServer160/object rights override stale SQLite/API/seed guidance. Skills grant no extra authority.

Write ONLY backend/src/EnglishCenter.Api/Infrastructure/Sql/D12/DIRECTOR.sql, at most220lines. Static batch: no GO/USE/transactions/dynamic SQL/modules/migration/users/logins/membership/business DML. Start fail-if-DATABASE_PRINCIPAL_ID('ec_director_reporting') IS NOT NULL THROW51212. CREATE ROLE [ec_director_reporting] AUTHORIZATION [dbo]. Never adopt existing role. Explicit dbo object-level permissions only:
GRANT EXECUTE on dbo.usp_GetFinancialReport, dbo.usp_GetAdmissionsReport, dbo.usp_GetTrainingReport.
DENY SELECT on each dbo.Account, dbo.StudentProfile, dbo.Score, dbo.GradeReport.
No other grants/DENYs/objects/schema/database/server permissions or fixed-role membership. No SELECT facts/business tables, SP01/02/03/07, UDFs, DML/DDL/export entitlement, credentials or runtime identity/config.
These procedures have no actor/identifying selector parameters and return accepted aggregate/options outputs. Ownership chaining permits accepted reports despite base DENY; DENY alone is not privacy proof. Codex tests output and forbidden actual operations.
