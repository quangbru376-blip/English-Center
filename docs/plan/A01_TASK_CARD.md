# A01 — mandatory audit persistence and security fallback — reviewed PASS

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
skill_verification: AGY7426ce5d-f17 SUCCESS/exit0, one63line helper; all3complete context reads before write, zero observed violations. Codex independently authored persistence/projection/tests and reviewed final scope PASS. Original worker/manager trees retained.

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

## Owner-approved isolated SQL scope

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

## Review and verification — 2026-10-05

Owner explicitly approved D13→A01 exact payload and isolated rollback-only SQL scope.
Codex review PASS after correcting canonical action/field names, per-action Account
and Payment-correction projections, positive child parent references, UTCms/IP bounds,
reason NFC/trim1000code-points and finite store timeout/null constructor checks.
54SC02 mandatory action policies (API133 deliberately excluded under E04 section7),
exact root/entity/child field projections; no full DTO, password/hash/token/sid/digest.
Details frozen at construction; mandatory SUCCESS only, verified actor/reference/
operation; required reason only accepted command families; child `.EFFECT` requires
projected parent IDs. Security technical actions AUTH.LOGIN/LOGOUT/ACCESS_DENIED/
THROTTLED selected by enum, no request action/payload. Four fixed saturating counters.
Final manager513unit+366SQL=879PASS/0fail0skip/buildclean; adds18unit+6SQL to D13.
Actual grouped program/component history shares existing TX, no writer commit;
mandatory FK failure/cancellation rollback all fixture account/session/counter/replay/
business rows. Actual DENY INSERT fails mandatory append safely. Independent audit
connection/rollback TX denied SECURITY append leaves business session revoked and
increments logout incident. Security NULLactor/exact90days/IP expiry verified.
Counter concurrency16000incidents/no cross-kind loss/longMAX saturation verified.
All user0/membership/permission/data/schema fixtures rollback; no extra principal.
Test-only internals visibility lets SQL tests exercise the actual parameterized adapter,
not a duplicated SQL implementation. Production security store owns a fresh supplied
connection; no real runtime profile/DI or committed security seed is configured.
Caller replay/no-op suppression and non-SC02 Account password/bootstrap integration
belong to later slices. No whole workflow/API/cleanup coverage claim; no A02 authority.
Owner walkthrough: docs/notes/D13_A01.md. Apply only reviewed stage via ADR163,
then reverify main and physical D12 catalog/profile/module bodies/102permissions.

Applied/reverified main74e3085, reviewed detached manager0116002. Final batch main513unit+366SQL PASS; unchanged backend tree67ffc5d0fde4c150f0f5b71287701886a3315fbc. Post-fixture full catalog/profile and zero residual fixture users/memberships/business rows verified. Main evidence: A01-evidence/main-verification.json.
