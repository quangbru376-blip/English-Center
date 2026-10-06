# R03 — Admin detail / lock / caller-result pilot

Canonical family M0_QUALITY_PLAN R03; dependencies A06,A01,R02,D12 applied or in batch.

## Authority, skills and verification

Owner requested continuation R02→R03 on2026-10-06 after applied A08/R01.
Status: concrete scope prepared; exact new external payload and rollback SQL consent
pending. No worker/SQL/implementation run for this batch. Earlier approval is not
blanket external sharing or permission to append T23 fixtures. Accepted product
decisions are unchanged; no new business question. ADR163 permits applying authorized
Codex-reviewed PASS implementation to main without another apply question.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
integration: antigravity-worker, Codex manager; complete skill files read.
skill_sources:
- .agents/skills/backend-clean-code/SKILL.md SHA256 726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b
- .agents/skills/code-review/SKILL.md SHA256 04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70
- docs/plan/SKILL_ROUTING.md SHA256 adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915
- antigravity-worker SHA256 eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b
applicable_overrides: accepted E03/E04/requirements, SQL2025 Express/160, exact
DTO/routes/errors/C1/full sets/raw clocks, cookie JWT/live current-session authority/
framework CSRF; parameterized EF/SQL and finite caller transaction deadlines.
No SQLite evidence, package/seed assumptions, generic REST/DTO replacement.
read_context: ordered Project Memory, requirements auth/Admin/replay, HANDOFF,
FIRST_CODE_HANDOFF,M0_QUALITY_PLAN; E04 final/DTO/replay/SQL/runtime and existing
A01/A04–A08/R01/T02/T03/T22/T23 mappings/tests. Workers read ONLY own worker card,
complete backend skill and routing, plus sole output if it exists; no links/AGENTS/
search/list/other sources/secrets/commands/Git/SQL/tests/network.
skill_verification: pending observed read-event/diff review and independent Codex
build/unit/HTTP/real SQL checks. Claims by worker alone are not evidence.

Approved external union exactly docs/plan/R02_WINDOW_WORKER.md,
docs/plan/R03_GUARD_WORKER.md, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md. Each writes one pure helper in a detached tree.
Keep rejected/original worker trees and manager review tree; main stays protected
until independent PASS. Worker skill grants no extra authority.

Approved SQL scope ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005: synthetic Role/
Account/AuthSession/AuditLog/ReplayOperation fixtures, including two or more temporary
Admin accounts for last-active/self guards; passwords/keys/tokens generated RAM-only.
Every fixture DML caller-TX rollback; finite multiple connections for request/
ADMINSET/ascending Account/binary SID locking, contention and revoke/expiry checks.
Full existing regression may use ONLY D12_fixture_user_0..3 WITHOUT LOGIN and
temporary membership/DENY/prior module/schema drift, all transaction rollback.
No new principals, real Admin, retained rows/keys, schema/migrations/SQL modules/
packages/grants/profile/owner/server logins/other DB/install/runtime/push/deploy.
Owned executor COMMIT paths source-reviewed with explicit HTTP fixture seams only;
no persisted owned SQL commits or committed runtime/stress proof.
Post-check:0dbo business rows except migration history,0fixture users/members,
exact4roles102permissions6migrations/complete catalog/profile160/collation/snapshotON.

Baseline code main3b46460/docs5432e29, backend4fb65a2e2f0e3ef9fc95dd040cf4438c4156be24,
733unit/HTTP/C1+462SQL=1195PASS/build0warnings0errors. Full regressions required.
Missing checks mean incomplete. Existing cached/locked restore only if needed;
no new dependency/vulnerability-assessment claim. Default Program/config unchanged/
unmounted; no runtime/profile/key/principal/full-M0 readiness claim.
After PASS: notes<=15lines, docs/api/plan and affected memory/evidence updated.

## Implementation and acceptance

Worker sole helper Common/Security/AdminLockGuard.cs. Codex owns Services/Administration
detail/lock/lookup + DTO/projection, Infrastructure/Administration repository/executor,
Api/Administration explicit adapters; relevant tests/docs. Narrow auth repository reuse
only for fresh tracked reload and shared ascending Account→binary SID protocol.
AuditPolicy/AuditEntry only existing canonical ADMIN.ACCOUNT.LOCK projection as needed.
No API006 search,008staff-create,009reset,011unlock,012disable,013restore mutations,
frontend/U stages, other modules, migration/package/Program/profile changes.

API007 GET /api/v1/admin/accounts/{id}: positive exact BusinessId/no body/business query;
Admin NORMAL/live proof, missing404, no activity renewal. AccountAdminView exact current
12-field projection per E04_DTO6.1: id/fullName/email/nullable phone/role/status/createdAt/
mustChangePassword/nullable temporaryPasswordExpiresAt/version/allowedActions/
disabledReasons. No hash/security version/SID/counters/DOB/address/other business data.
Accepted five UI capability names may be described as metadata; they do not mount
unimplemented mutations. Self/last-active/state guards determine capabilities; current
authorization before buffered delivery. Coherent read + live final authorization.

API010 POST /api/v1/admin/accounts/{id}/lock: Admin NORMAL/live proof and framework
CSRF/Origin before detailed body. Exact body expectedVersions.account8byte RV only;
required strict UUID Idempotency-Key header, no body actor/action/key/reason/status.
Canonical R01 API10 envelope binds trusted accountId target. Request gate first then
EC:ADMINSET then ALL involved Account rows ascending IDs including caller/target/active
Admin predicate rows, followed by all required Session rows deterministic binary SID.
Fresh caller status/version/role/SID/revocation/raw lifetime rechecked after locks and
before DML/commit. Admin set count predicate serialized by ADMINSET and guarded query.

Recognize active marker via authorized current target before digest/RV/new-source
checks; replay200 current AccountAdminView+old operationId, no repeated audit/revoke/
version touch or credential. Safe refs fallback only when current refs still readable.
New command requires target ACTIVE, current RV, target!=actor, last ACTIVE Admin
protected under gate; invalid state/self/last-active/RV409, target missing404. Overflow
security version fails closed. Atomic target ACTIVE→LOCKED/securityVersion+1/all own
existing sessions revoke/mandatory canonical audit/T23 completion. Preserve hash/
temp bundle/role/email/Profile/throttle/other accounts and already-revoked timestamps.
Caller interactive activity only through existing explicit A06 hook in same TX; reads/
lookup/replay do not renew. Time checks raw equality expires, no session resurrection.
Required audit/T23/DML fault/cancel/time failure rolls back whole unit. Success200
AccountAdminCommandResult account+operationId emitted AFTER known commit, no secret.
Commit acknowledgement uncertainty does not promise rollback or auto-retry.

API014 GET /api/v1/admin/command-results/{requestId}?action=<canonical action>:
strict UUID, required single exact action only, no body/caller/target/extra query.
Admin NORMAL; exact six ADMIN.* recognized per accepted registry (no new mutations).
Malformed/unknown400; known outside role/endpoint403 before marker probe. Own unexpired
current target-scope marker200 SafeCommandCompletion only; absent/othercaller/expired/
scope404 RESULT_NOT_CONFIRMED. No AccountAdminView/PII/secret/digest/timestamps at lookup.
Generated fixtures for secret modes only safe refs, no real temporary credentials.

Tests: role matrix including restricted; strict request/header/query/parser/error
precedence/no-store/CSRF; detail12fields/PII allowlist; every7target roles lock/revoke;
self/last-active/RV/overflow/wrong-state failures; exact caller6action lookup,24h/
retained-expired/mismatch/current rights; replay after RV/state change old operation
without repeat effects; audit/T23 failure whole rollback; finite request/ADMINSET/
account/SID contention and auth-vs-revoke order; no committed stress claim.
Lost-response recovery uses fixture commit seams plus same-TX SQL completions; actual
owned SQL COMMIT remains source-review only under proposed rollback permission.


## Implementation review checkpoint

Worker WINDOW134d7619-b9d SUCCESS46diff lines/4observed reads1write;
GUARD9dc8e503-214 SUCCESS42diff lines/5reads1write. Full authorized3contexts
before write; zero observed scope violations. Compact evidence retained, no raw
thinking/transcripts/logs. Codex source integration and independent verification.

Review corrections: stored-target access before changed-route digest mismatch;
prelock union of caller/request/stored target/admin set ascending then binary SIDs;
malformed marker diagnostics after live proof; detail SNAPSHOT buffer with separate
live final authority; every negative applock result aborts with accepted503/500.
No additional packages/schema/SQL modules/audit policies or mounted Program changes.
SQL validation includes actual post-completion faults/cancel rollback, denied DML/
audit/T23, SQL unique backstop, full catalog/regression and finite competing locks.
SQL owned COMMIT paths remain source-reviewed; HTTP uses real crypto/CSRF/service
with explicit in-memory commit seams. Not persisted runtime/committed stress proof.
Next U02→U01 unstarted; no frontend/restM0/runtime/installation/push/deploy authority.

## Completion review — 2026-10-06

Verdict PASS; baseline1195, added158unit/HTTP/protocol+33SQL=191 meaningful checks.
Full manager891unit/HTTP/C1+495SQL=1386PASS/0fail0skip, build0warnings0errors.
Retained business rows/test users/memberships0;4roles102permissions6migrations/profile
unchanged; full catalog/module/column/permission regression PASS. Scope/business/
backend/security/owner-notes/docs checklist PASS; frontend/migrations/money/pagination
N/A to this bounded pilot. Existing Role/Account DTO projection is exact12fields;
SQL snapshot buffers are discarded on final current authority failure. Only request
and ADMINSET gates are introduced at service caller, with explicit shared lock order.
Owned SQL COMMIT wrappers source-reviewed and HTTP fixture seams; no persisted or
committed stress/runtime readiness claim. Other domains/current-scope integrations,
Admin mutations/U families/production profile and real principals remain deferred.
Original worker/manager trees retained. Authorized PASS applied under ADR163; full main suites reverified. No further owner product decision required.
