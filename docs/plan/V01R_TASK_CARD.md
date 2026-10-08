# V01-R — rollback-only catalog visibility feasibility

2026-10-08. Necessary M0 dependency review under ADR177, Codex only/no AGY sharing.
Purpose: determine whether explicit read-only metadata permissions can satisfy
the unchanged exact catalog without widening runtime business DML or owner-mode
fallback. This experiment is not permanent provisioning or positive runtime auth.

Exact SQL scope .\\SQLEXPRESS/EnglishCenter_D07_20261005 only. Installed SQLCMD
-E -C diagnostic connection; no runtime certificate-trust claim. Baseline.sql
before/after. One caller-owned transaction, fail if fixture user already exists,
CREATE USER [V01R_catalog_fixture_0] WITHOUT LOGIN, membership ec_app_runtime,
GRANT VIEW DEFINITION and VIEW SECURITY DEFINITION on this database plus SELECT
on only __EFMigrationsHistory(MigrationId,ProductVersion). EXECUTE AS USER only
for fresh fixed ReadCatalog.sql queries; REVERT then ROLLBACK in success/error.
No COMMIT, business DML, cleanup, login/OS account/auth mode, server permission,
schema/migration/role-definition change, other DB or actual principal activation.
Original Login/admin token remains diagnostic: user impersonation is NOT genuine
least-privileged login proof and must never count as runtime readiness PASS.

Ignored local outputs only: test-results/R-before.txt,R-script.sql,R-catalog.txt,
R-after.txt. Query export changes output encoding to one JSON key/value per row,
not fact selection. Compare exact key/value manifest after unchanged module-header/
line-ending normalization; report fixed counts/difference categories, never module
bodies, identities or secret material. Verify fixture USER_ID absent and catalog/
profile/exact4role102permissions/retained rows before-after byte-identical.

Docs this card/R sanitized evidence/operator integration proposal/current handoff.
No production code changed in this experiment. Missing or extra catalog evidence
remains a failure, not a reason to trim the expected manifest or silently add grants.
Any permanent metadata allowlist/guard update needs its own concrete reviewed scope.
Same backend/review/routing skill versions/overrides as V01Q, complete skills read.
read_context requirements/E04 runtime/I guard/ReadCatalog/ExpectedCatalog/baseline.
Status IN PROGRESS; skill_verification pending actual SQL/catalog/rollback checks.
