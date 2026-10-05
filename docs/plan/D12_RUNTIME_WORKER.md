# D12 RUNTIME — ec_app_runtime

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md BEFORE writing. Read ONLY these three paths and the exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex tests/reviews/applies. Accepted E03/E04 SQLServer160/object rights override stale SQLite/API/seed rules. Skills grant no extra authority.

Write ONLY backend/src/EnglishCenter.Api/Infrastructure/Sql/D12/RUNTIME.sql, at most220lines. Produce a static SQL batch, no GO/USE/transactions/dynamic SQL/modules/migration/users/logins/role memberships. Only CREATE ROLE [ec_app_runtime] AUTHORIZATION [dbo] and object-level GRANTs to that role. Start with fail-if-DATABASE_PRINCIPAL_ID(role) IS NOT NULL THROW51212; never adopt or modify an existing role. No schema/server/database grants, DENY, packages, other role references or business DML. Explicit two-part dbo object names.

Exact table matrix (SQL object names, not aliases):
SELECT only: Role, Band.
SELECT, INSERT, UPDATE: Account, StudentProfile, AuthSession, Course, TrainingProgram, Registration, EntryTestSession, EntryTestAttempt, EntryTestBooking, BandAssignment, Class, Schedule, Attendance, Score, GradeReport, Invoice, Payment.
SELECT, INSERT, UPDATE, DELETE: ScoreComponentConfig only. Service later enforces whole unused DRAFT deletion; grants do not prove that rule.
SELECT, INSERT only: StudentClassAssignment, AuditLog, ReplayOperation.
InvoiceNumberCounter: NO direct grant; SP07 ownership-chain allocator only.

EXECUTE on these7procedures: usp_SearchAdmissions, usp_GetEntryTestRoster, usp_GetClassGradeReview, usp_GetFinancialReport, usp_GetAdmissionsReport, usp_GetTrainingReport, usp_AllocateInvoiceNumber.
EXECUTE scalar ufn_EffectiveEntryTestState.
SELECT TVFs ufn_BandForScore, ufn_RegistrationUnfinished, ufn_ClassObligationProgress, ufn_GradeTotal, ufn_InvoiceDebtFlags.
SELECT views vw_AdmissionsRegistrationFacts, vw_EntryTestBookingFacts, vw_CurrentScheduleOccurrences, vw_TrainingClassFacts, vw_FinalizedGradeFacts, vw_FinanceInvoiceFacts.
No other objects, migration history, ALTER/CONTROL/DDL/backups/db_owner/sysadmin/maintenance permissions. Preserve exact table casing. No runtime identity/config/seed.
