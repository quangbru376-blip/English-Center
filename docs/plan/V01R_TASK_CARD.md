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
Status feasibility review PASS; skill_verification actual SQL/catalog/rollback checks complete.

First candidate PASS:880/880 exact facts, rollback/user absence/baseline equality.
One additional ablation case is authorized for independent least-privilege review:
same per-case owned rollback transaction/user/queries, omit explicit VIEW SECURITY
DEFINITION only. R-ablation-script.sql/R-ablation-catalog.txt/R-ablation-after.txt
ignored outputs. Determine whether database VIEW DEFINITION already supplies that
metadata visibility; do not recommend a redundant grant. No permanent apply.

## Independent feasibility review PASS, no permanent deployment
Candidate and ablation each880expected/880actual facts,0missing/0different/0extra.
Exact key/value comparison and original module header/newline normalization used.
Both rollback markers and fixture-user absence verified; D07 byte-identical digest
10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E before/after.
Explicit VIEW SECURITY DEFINITION unnecessary: do not recommend it alongside
VIEW DEFINITION plus the two history-column reads. Metadata only, not actual login.
SQLCMD initial incompatible -h/-y options rejected before executing the first
transaction; retried without -h, with JSON fact rows, no predicate/manifest change.
Skill verification: no external sharing, exact fixed catalog queries and rollback
scopes honored; source module contents remain ignored/local. See QRS summary.
