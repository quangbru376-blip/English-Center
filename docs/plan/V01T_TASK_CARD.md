# V01-T — maintenance SQL effects and authority

2026-10-08. Assigned M0 completion, ADR177/163. Codex manager only: privileged
SQL verification is not delegated or shared externally. Full M0 stays incomplete.

Exact code scope SqlMaintenanceService.cs and Readiness/MaintenanceSqlTests.cs.
Extract existing two cleanup statements into private constants, consumed by the
same production method; tests read these constants and PrincipalSql by reflection
to exercise actual SQL without bypassing production authentication/TLS checks.
Review direct-grant exclusion: WITH GRANT OPTION must never qualify as narrow
maintenance permission. No new public seam, route, package, schema or defaults.

SQL scope existing OwnedSqlFixture on .\\SQLEXPRESS: GUID-owned synthetic scratch
databases only, existing six migrations, existing synthetic role/account seeds.
Capture database ID/GUID before any cleanup; exact identity-checked DROP in finally.
No retained D07 writes. Diagnostic fixture connection is not runtime TLS/auth proof.
Temporary WITHOUT LOGIN user V01T_maintenance_fixture_0 and narrow maintenance
grants only inside rollback transaction. No server login, OS identity or permanent
grant. Verify exact expiry boundary, future rows, mandatory audit retention, expired
IP clearing, independent replay cleanup, repeat idempotence, and rollback restoration.
Verify narrow grant exclusion versus grant-option, business SELECT and role membership.
No real scheduler/principal/PKI activation claim; service guards remain mandatory.

implementation_skills backend-clean-code; review_skills code-review.
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs, requirements retention, E04 runtime,
NOP/S contracts, actual maintenance/SQL guard, mappings and OwnedSqlFixture.
applicable_overrides exact SQL Server/90d security-IP/mandatory retained/24h replay,
cookie auth unchanged; no SQLite/new package/default provisioning.
skill_verification complete skill reads; implementation review/checks pending.

Verification manager/main build --no-restore -warnaserror, default full suite and
explicit opt-in MaintenanceSqlTests. D07 baseline before/after and four owner-file
hashes unchanged. Evidence ignored test-results/T-* plus sanitized T summary;
docs this card/notes and affected current state/TODO/handoffs/closure only.
