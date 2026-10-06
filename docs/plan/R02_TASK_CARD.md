# R02 — Completion storage/TX protocol and scoped lookup primitives

Canonical family M0_QUALITY_PLAN R02; dependencies D06,D07,A01,A06,R01 applied PASS.

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

Worker sole helper Common/Replay/ReplayWindow.cs. Codex owns completion DTO/reference
codec, protocol service and Infrastructure/Replay caller-TX repository/gate integration,
tests/notes/API docs; no new schemas or changes to accepted55 registry/DTO definitions.
Reuse CanonicalIntent copy-out SHA25632 and immutable action metadata. Completion
partition exact actor/action/request; request gate EC:REQUEST:<positive actor>:<exact
action_code>:<canonical lowercase UUID>, Exclusive/Transaction/DbPrincipal public.
Explicit positive finite timeout; every negative sp_getapplock aborts whole TX;
-1/-2/-3 known503, -999/configuration/unexpected500; no automatic retry.
Request gate precedes sorted ordinal business gates and Account→Session rows.

Append T23 only after effects+mandatory audit same caller connection/TX. Digest
binary32, operation UUID server generated, completion UTC datetime2(3), expiry EXACT
completed+24h. JSON exact outcome COMPLETED + target safe registry IDs, no body/
response/PII/secret/digest snapshots or PENDING/FAILED marker. References from server,
required/optional exact registry keys validated, immutable/copy-out DTO boundaries.
No overwrite/delete/purge/repair of marker, no new result mode or schema.

Active marker command: current auth/role plus current target/task-read authority and
relationships before digest compare; same digest200 old operation/no DML/audit, different
digest409 IDEMPOTENCY_CONTENT_MISMATCH. Do not compare old request RV/live mutation
source state before recognizing replay. Retained expired row command409 CONFLICT,
never reuse partition. Lookup absent/not-caller/expired/outside-read-scope404
RESULT_NOT_CONFIRMED same safe message. Future/malformed marker fails closed, no repair.
Required live-scope callback has no allow-all default; registry membership is metadata
only. Current scope rechecked before buffered delivery; no stale snapshot authorization.

Expose reusable lookup primitive with six exact accepted endpoint/role/action allowlists;
no full domain lookup adapters or mutations for unimplemented Student/Teacher/etc.
R03 supplies real Admin API014 integration; other scopes must fail closed until their
domain callers exist. Completion target IDs alone never grant ownership/task access.
SafeCommandCompletion exact4fields outcome/operationId/resultMode/target; no dates/
actor/digest/PII. SECRET_RESULT_UNAVAILABLE supported metadata/reference without any
new staff-create/reset mutation or credentials. Preserve first-vs-replay discriminator,
expired lookup absence does not assert rollback, no automatic resubmission.

Checks: all55 target schemas/modes and six allowlists; exact24h/equality/raw clock/
future/boundary, strict references/unknown/duplicate/extra secret keys; caller/action/
key isolation, immutable copies, scope-before-mismatch and revoked current-session
denial; actual SQL append uniqueness/FK/digest/profile constraints, caller rollback
on failed audit/storage/cancel/DENY, request-gate contention/negative handling.
No proof of persisted commits from rollback-only tests. R03 is the first real caller.


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
Original worker/manager trees retained. Apply authorized PASS under ADR163 and rerun
full main suites before closing. No further owner product decision required.
