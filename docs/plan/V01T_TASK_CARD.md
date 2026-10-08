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
skill_verification complete: skills read in full, exact two-file diff reviewed,
actual production SQL constants exercised with narrow impersonated permissions;
manager/main checks below. No external dispatch/context sharing.

Verification manager/main build --no-restore -warnaserror, default full suite and
explicit opt-in MaintenanceSqlTests. D07 baseline before/after and four owner-file
hashes unchanged. Evidence ignored test-results/T-* plus sanitized T summary;
docs this card/notes and affected current state/TODO/handoffs/closure only.

## Independent review PASS/applied/reverified

Main1f0f0e9 / manager7dbcb4d; two files169insertions/4deletions. Review found
state-W database grants could match the narrow allowlist; state-G requirement
fixes this. No public testing/auth bypass; tests reflect actual private SQL constants.
Manager/main build0warnings0errors and1197defaultPASS/63intentional opt-in SKIP;
5actual SQL cases PASS each. Actual narrow rights execute cleanup; WITH GRANT
OPTION/business SELECT/runtime-role membership set exclusion1. Exact expiry -1ms/
0/+1ms proves90d security/IP and24h replay predicates, mandatory records retained,
future data untouched, jobs independent, repeat idempotence and rollback restoration.
All10created scratch databases have matching ID/GUID cleanup receipts. Temporary
user/grants/data rolled back before DROP; D07 baseline output byte-identical
10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E,24empty
tables/4roles102permissions unchanged. Four owner hashes unchanged/unshared/untracked.
Source/tests trees b0b7af270d28330541615436e298b462364f35dd /
e825ccea3a4247e0c58c57914d5f07e5e21e6275 identical on both checkouts.
API/DTO/migration/frontend change checklist N/A. Notes <=15lines. Actual authenticated
maintenance principal, strict TLS and running scheduler remain unverified; these
rollback statement tests do not claim service COMMIT/startup readiness. Full M0 open.
