# A01 — mandatory audit persistence and security fallback — preparation

Second family in owner2026-10-05 two-next request: D13→A01. Depends on D06/D07,
P05/P07; do not start A02 bootstrap, login routes or later workflow families.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete skills already read and unchanged hashes
verified: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: E03 AuditLog/E04 section7 exact event allowlists, SQLServer
tests/shared caller transaction, UTCms and90day expiry override generic seeds/SQLite.
read_context: memory/requirements, HANDOFF/FIRST_CODE_HANDOFF, M0 quality/routing,
ADR058–062/093/115/116, E04 audit/replay action registry, existing AuditLog/model.
skill_verification: not dispatched/verified yet; no audit implementation PASS claim.

## Code scope and acceptance

AGY creates only Common/Auditing/AuditIncidentCounter.cs from A01_COUNTER_WORKER.md
and backendSKILL/routing; one file/220lines. Codex implements audit primitives,
exact action/entity/old-new/reason/parent field policy and persistence adapter under
Common/Auditing/ and Infrastructure/Auditing/, plus independent unit/SQL tests/docs.
Only framework/current packages; no arbitrary payload/exception/secret logging.

Mandatory audit is an append within caller's existing connection/transaction,
including operation_id and verified actor; no own commit/independent connection
or failure-swallow. Failure propagates sanitized failure and caller rolls back
business/account/revocation/replay/counter/child changes. Success audit persisted
only atomically with committed mutation; replay/no-op suppression belongs to
callers, no fabricated replay or full workflow coverage claim from this primitive.

Pure security events use independently supplied audit store outside mutation TX,
best effort; logging failure increments bounded secret-free incident counters,
does not restore a revoked session/bypass checks/notify user. Persist only known
action/result/entity projections; unauthenticated actorNULL, never guessed email.
No passwords/hashes/tokens/sid/digests/full DTOs. Event time UTCms; SECURITY expiry
90days, MANDATORY expiryNULL; optional security IP expires90days, no default
business IP. Structured old/new fields per accepted command family, unknown keys
not persisted, reason only where required, no intermediate scratch config names.
No persistent cleanup job, grants/roles/login/bootstrap, endpoint/DI wiring added.

## Isolated SQL proposal awaiting explicit approval

Use existing .\\SQLEXPRESS/EnglishCenter_D07_20261005 only. Fixture inserts AuditLog
and synthetic business/replay/counter/session changes in caller TX; forced audit
failure by test interceptor or temporary DENY INSERT on AuditLog to existing
ec_app_runtime role, with only four D12 fixture-user names. Roll back all fixture
data/users/memberships/permission changes, and verify D12 exact102permissions.
No committed seed/data, extra principals/roles/server logins/owner/database/profile
changes, migrations, runtime secret/profile connection, push/deploy.
Tests demonstrate audit+mutation atomicity and forced failure rollback, grouped
operation/history, NULL actor/security expiry and secret-free serialization,
cancellation and bounded fallback/concurrent counters/no session resurrection.

Owner walkthrough: mandatory append shares the workflow TX and failure aborts it;
security log failure counts an operator incident while the security outcome stays.
Real callers/auth/audit UI/cleanup remain later; no full product audit claim.
