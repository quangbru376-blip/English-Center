# M0 W2 — dedicated LOCAL_DEV catalog and Windows SQL contexts

2026-10-09; whole M0 ADR177, LOCAL_DEV ADR186, reviewed PASS apply ADR163.
Codex manager only; no AGY receives OS credentials, secrets or privileged scripts.

W2-A exact tools/operator/Initialize-M0Catalog.ps1: read W1 ownership manifest,
verify current operator and all three local SIDs; existing EcM0Runtime/Maint/Schema
are only targets. Fail if EnglishCenter_M0_LOCALDEV or any exact Windows SQL login
already exists. Create only that catalog on .\SQLEXPRESS with collation
Latin1_General_100_CI_AS_SC, compatibility160, snapshotON, AUTO_CLOSE OFF.
Create only three FROM WINDOWS logins, no server role/CONTROL/server grant added.
Write catalog database_id/database_guid/W1 ownerId nonsecret ownership record in
protected W1 vault and sanitized count receipt under ignored evidence. On failure
retain owned database/logins with identity receipt for exact recovery; no DROP,
reuse, arbitrary database, authentication-mode/service/registry/TLS changes.
Provisioning uses owner's diagnostic integrated SQL with TrustServerCertificate
true explicitly; this is not runtime/maintenance or positive TLS/readiness proof.

W2-B follow-on: apply exactly existing six migrations using pinned installed
local dotnet-ef10.0.4 --no-build and explicit owner-integrated provisioning
connection to owned new catalog. No install/restore/new migration/startup DDL.
Then exact users M0RuntimeUser/M0MaintUser/M0SchemaUser mapped to W1 logins.
Runtime only ec_app_runtime + CONNECT/VIEW DEFINITION + SELECT two migration
history columns in state G; preserve four roles/exact102 role permissions.
Maintenance no database role: CONNECT, DELETE AuditLog/ReplayOperation,
SELECT AuditLog(audit_category,security_expires_at,ip_expires_at), Replay(expires_at),
UPDATE AuditLog(ip_address,ip_expires_at). Schema db_owner only in new catalog,
separate offline migration identity never supplied to application or readiness.
No seed/business rows/Admin provisioning in W2. Genuine per-user connections
must later prove runtime/maintenance policies/catalog; impersonation fixtures
cannot substitute. Strict SQL certificate validation stays pending W3.

implementation_skills backend-clean-code; review_skills code-review.
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory, NEXT/HANDOFF, requirements, E04 runtime, W1 receipt,
existing6migrations/D12/runtime7facts/maintenance9facts and retained baseline.
applicable_overrides SQL Server real identities, accepted catalog/CSRF/JWT,
no SQLite/packages/default startup auto repair. No external read context.
skill_verification pending AST/exact scope/SQL profile and owner hashes/baseline.
Only W2-A helper/card first; review before applying and running each derived step.
No M1–M4/packages/install/push/deploy or protected owner-file changes.

W2-A pre-execution review PASS: AST0errors, all fixed SID/login/root guards,
new catalog only, identity receipt immediately after CREATE, no DROP/DDL reuse,
no server role grants or secrets exported. Actual catalog/profile acceptance pending.
