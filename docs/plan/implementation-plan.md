# Implementation planning — discussion draft

## Latest R02/R03 checkpoint — applied and reverified PASS

Owner-approved R02→R03 completed/reviewed PASS/applied/reverified2026-10-06;891unit/HTTP/C1+495SQL=1386PASS/0fail0skip/build0warnings0errors.
Two bounded AGY SUCCESS/full3contexts before write/zero observed scope violations;
Codex independent integration/fixes/tests/review. R02 exact completion references/
24h/caller-action-key partition/request gate/scope-before-digest/append-only TX.
R03 explicit API007 detail SNAPSHOT+final live authority, API010 lock atomic status/
version/all-own-SID revoke/mandatory audit/T23, API014 safe caller6action lookup.
Request→ADMINSET→ascending Account→binary SID locks; replay never repeats effects,
stale-RV mutation guards or activity. Other55workflow callers/Admin mutations deferred.
Program remains unmounted; production keys/profile/principals/readiness deferred.
SQL fixtures rollback only, owned COMMIT source-reviewed/HTTP fixture commit seams;
no persisted runtime/committed stress proof. Retained business rows/test users/members0,
4roles102permissions6migrations/full catalog/profile unchanged. Original trees retained.
Applied/reverified under ADR163 code mainfcdd21e, manager1cb6ebc; identical backend
10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a. Both full suites passed on manager and main;
see R02/R03-evidence/verification-summary.json for four TRX hashes/retained DB checks.
Next U02→U01 unstarted; no frontend/restM0/installation/push/deploy authority.

## Latest A08/R01 checkpoint — applied and reverified PASS

Owner-approved A08→R01 implementation completed and independently reviewed PASS
2026-10-06;733unit/HTTP+462SQL=1195PASS/0fail0skip/build0warnings0errors.
Two bounded AGY SUCCESS/full3contexts before write/zero observed scope violations;
Codex source integration/fixes/schema parity/independent tests. A08 exact API004,
normal/restricted current proof and RV, raw credentials/P08 policy; mandatory
hash/security-version/temp-clear/all-own-SID revoke/audit caller-TX unit; secure
cookie deletion only after commit. Account first, explicit SID locks binary order;
per-SID round trips intentional for accepted lock protocol, no invented session cap.
AuditEntry narrow fixed AUTH.PASSWORD_CHANGE factory contains no credentials/hashes.
No automatic scope upgrade/new session/login or credential replay. R01 disconnected
55exact canonical registry/6lookup groups, full schema-aware EC-C1 normalized bytes/
SHA25632binary, frozen literal fixtures, full supplied intent/absence/sets vs ordered
weekly plan. No T23 storage/lookup/mutation/replay routes or55executed workflows claim.
Program unchanged/unmounted; no real Admin/production profile/keys/principals/full
runtime readiness. SQL cores rollback-only; owned executor COMMIT source-reviewed
and explicit HTTP fixture commit seams, not persisted/committed stress proof.
All retained dbo rows except history/test users/role memberships0;4roles/exact102
permissions/6migrations/profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged;
exact full catalog/modules/columns regression PASS. No schema/module/package/grants/
profile/owner/logins/other DB/seed/persistent keys/install/push/deploy changes.
Original detached worker/manager evidence retained. See A08/R01_TASK_CARD/evidence,
notes/A08.md,R01.md; ADR172. Applied/reverified under ADR163 code main3b46460, manager18c0af7; identical backend
4fb65a2e2f0e3ef9fc95dd040cf4438c4156be24. Both manager and main full suites PASS;
compact TRX hashes/checks in A08/R01-evidence/verification-summary.json.
Next canonical R02 completion/TX/lookup primitives → R03 Admin lock pilot; unstarted.

## Previous A06/A07 checkpoint — applied and reverified PASS

Owner-approved A06→A07 implementation completed/reviewed PASS/applied/reverified
mained40a4b, manager633671b, identical backend0d407841beeb719bbd753e828a9a5d408549a1bc;
2026-10-06;632unit/HTTP+438SQL=1070PASS/0fail0skip/build0warnings0errors.
Two bounded AGY SUCCESS/full three contexts before write/zero observed violations;
Codex implemented/reviewed/tested JWT/current-state/transaction/HTTP integration.
A06 supplied-key framework JWT validation plus fresh Account→Session SQL guards,
exact9-field API003 /me, live CSRF resolver and explicit caller-TX activity/recheck
hook. Me/CSRF/background never renew idle; restricted scope cannot enter business.
A07 API005 revokes only current SID; commit before204/secure cookie deletion;
independent A01 security audit faults cannot restore revocation or block success.
Invalid auth cookies are removed on401 to permit deliberate anonymous CSRF/login
recovery; response remains401 and does not claim revoke success.403/500 unchanged.
Default Program stays unchanged/unmounted, no real Admin/production profile/keys/
principal or full runtime readiness. SQL cores execute only rollback fixtures;
owned SQL commit paths source-reviewed with explicit HTTP fixture commit seams,
not proof of persisted runtime/committed multi-client concurrency. No A08/restM0.
Retained dbo rows except history/test users/role memberships0;4roles102permissions/
6migrations, profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged; exact catalog
regression PASS. No schema/module/package/grant/profile/owner/logins/other DB/seed/
persistent keys/install/push/deploy change. Original worker/manager detached retained.
See A06/A07_TASK_CARD/evidence, notes/A06.md,A07.md, docs/api.md; ADR171.
Next canonical A08 password change, then R01 canonical replay registry; unstarted.
ADR163 permits applying only already authorized reviewed PASS stages.

## Historical A03/A05 checkpoint

### A03/A05 applied checkpoint (historical) — applied/reverified PASS maina9abdc6

Owner-approved A03→A05 implementation completed/reviewed PASS/applied/reverified maina9abdc6
2026-10-06; 586 unit/HTTP +409 SQL=995 PASS, 0 fail/skip, build0warnings0errors.
Two bounded AGY SUCCESS/full authorized context before write/zero observed violations;
Codex integrated, corrected and tested independently. A03 supplies strict API018
signup, caller-TX Account+StudentProfile+mandatory audit, cross-role uniqueness and
201 without auto-login. A05 supplies generic login/throttle, fresh SQL account locks,
normal/restricted session issuance, supplied JWT signing and secure cookie after commit.
Explicit API adapters remain unmounted in unchanged Program. No real Admin/profile/
keys/login resolver or runtime readiness claim. SQL cores tested in rollback-only
fixtures; HTTP commit boundaries use explicit fixture executors. Owned SQL executor
commit paths source-reviewed, not executed to persist accounts/sessions. Predicate
contention and same-transaction multiple sessions do not prove committed stress.
Only EnglishCenter_D07_20261005 used; retained Account/Profile/Session/Audit0,
D12 fixture users/role members0, four roles/exact102permissions/six migrations,
profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged; exact catalog regression PASS.
No schema/migration/package/grants/owner/profile/logins/other DB/persistent keys/
push/deploy change. Worker and manager detached evidence trees retained.
See A03/A05_TASK_CARD/evidence, docs/notes/A03.md,A05.md, docs/api.md, ADR170.
Next canonical A06 current-session/me → A07 logout; not started by this batch.
ADR163 permits applying only already authorized reviewed PASS stages.

## Historical A02/A04 checkpoint

Updated2026-10-05: owner-approved A02→A04 implementation scope complete/reviewed
PASS/applied/reverified code maina8ad1d5, managerd1612ed; identical backend tree
8a7e73f1bf9114311c9484f5c7c16d639ebd5916. A02 maindf36d98:514unit+375SQL PASS;
final545unit+375SQL=920PASS/0fail0skip/build0warnings0errors/cache-only locked restore.
Two bounded AGY SUCCESS/full3context reads before write/zero observed scope violations;
Codex independent source review, transaction/host checks and fail-closed fixes.
A02 explicit operator bootstrap: exact7roles, first Admin+projected mandatory audit on
caller SERIALIZABLE/EC:ADMINSET; repeat preserves existing Admin credentials/status.
Seven-role synthetic identities use generated RAM-only secrets and rollback. Runtime
role cannot bootstrap. No real Admin provisioned or automatic startup seed/commit.
A04 framework antiforgery API001 integration, exact Origin/header, verified-identity
boundary and safe cookie flags. No-store, no session/idle writes. Explicit origin/keys/
resolver required; private guard rejects token issuance without the boundary. Default
Program remains unmodified and API001 unexposed; test resolver/keys are fixtures only.
Only EnglishCenter_D07_20261005 used; all fixture data/schema/DENY/users/memberships
rolled back, retained0business rows/0test users/0ec-role members. Exact post-fixture
catalog/230columns/24module bodies/102permissions PASS;24tables/6migrations/4roles/
profile160/collation/snapshotON unchanged. No schema/migration/grant/profile/owner/
login/other DB/persistent keys/push/deploy change. Operator real Admin provisioning,
production profile/keys/auth resolver and full runtime readiness remain deferred.
See A02/A04_TASK_CARD/evidence, docs/notes/A02.md, A04.md and docs/api.md; ADR169.
Historical next at A02/A04 completion was A03→A05; see latest checkpoint above. ADR163 continues only for authorized reviewed PASS stages. Historical scope below.

## Authority and baseline

Owner explicitly requested discussing the coding plan after E01–E05 acceptance.
The wider milestone proposal remains a planning baseline. Owner04/10/2026
separately requests next-chat first coding stage: P01 prerequisites then P02
backend/test skeleton under [FIRST_CODE_HANDOFF](FIRST_CODE_HANDOFF.md).
Initial scope implemented/reviewed in detached commit d9eb194; see
[P02 task card](P02_TASK_CARD.md) and [P01 manifest](M0_LOCAL_PROFILE.md).
Owner separately confirmed main application as df7cd9a; locked restore/build
clean and ten real startup tests reverified on main. Original tree retained;
later tasks/SQL/runtime installs/deployment remain outside initial scope,
this application permission grants no later task scope. M0/V01 incomplete.
Accepted requirements, E03 database and E04 contracts remain the source of truth;
ADR-160/161 do not need another design vote. Preserve owner D12/D13 deferral.

## Proposed approach

Build small vertical capabilities: database constraints/persistence → service
transaction and authorization → API → Vietnamese UI → verification and demo.
Use conventional code the owner can explain in the oral examination. Preserve
the agreed stack/module boundaries and avoid new package/framework decisions
during task sizing. Do not equate 154 endpoints with equal-sized tasks.

## Owner direction — quality before calendar throughput

On2026-10-04 the owner explicitly chose task decomposition for code quality,
accounting for output-token limits and uneven task complexity. Deadline and
weekly-hours inputs are not prerequisites for this plan. M0–M4 are coverage
and dependency groups, not calendar commitments or single worker prompts.
The operational decomposition below refines that direction; individual contracts
and the proposed execution sequence remain to be made concrete before coding.

### Three levels of work

- Coverage group: M0–M4 tracks which accepted capabilities must eventually exist.
- Integrated slice: one demonstrable real workflow with all applicable guards,
  persistence/API/UI and required failure/concurrency evidence.
- Generation task: one cohesive code change small enough to implement and review
  completely. A slice can require several tasks; an infrastructure task may have
  a build/testable technical outcome rather than a user-facing screen.

Task completion does not imply slice completion. Partial layers must not be
exposed as a finished workflow, and mocks/stubs do not satisfy integration gates.
Keep every accepted atomic business operation all-or-none even when implementation
is produced in several tasks. Split supporting components, not transaction semantics.

### Size tasks by risk and reviewability

Codex inspects the actual context and estimates implementation plus necessary
tests/documentation before dispatch. Reduce scope when a task combines multiple
independent outcomes, introduces too many unfamiliar dependencies, touches shared
infrastructure and domain rules together, or cannot be fully reviewed and verified
within the available output. Prefer one complex transaction per task; a repetitive
low-risk DTO/mapping group may fit together. File count and diff lines are enforceable
bounds, not proof of task quality; no universal files/lines/token quota is assumed.

Reserve output capacity for failure paths, tests and a concise receipt. Code is
written to files rather than duplicated wholesale in chat. Context readings and
output generation limits are distinct: having read a large baseline does not mean
an entire module can be implemented in one response. Do not omit guards, audit,
replay, errors, tests or explanation just to fit a prompt. If scope no longer fits,
stop at a documented incomplete state, review the partial diff and issue a narrower
follow-up; no silent continuation of an unrelated contract or success claim.

### Required skill routing

[Skill policy](SKILL_ROUTING.md) is a required preflight/review gate for every
code-writing task. Task cards record selected complete skill sources, baseline
overrides and actual verification. Backend/UI skills follow the affected scope;
Codex code-review is required before done. Integration protocols do not replace
implementation skills or authorize extra scope.

### Contract for every generation task

Before code: record the task identifier, verified baseline commit and dependencies,
one observable outcome, exact F/W/API/DTO/TX/data references, allowed/forbidden paths,
conservative file/diff limits, required success/failure/boundary checks, manager-run
validation commands, exclusions and integration obligations. No invented scripts:
commands must exist or be explicitly part of the authorized task being added.
Worker contracts are English and use only the declared read context; following a
linked file requires scope/consent assessment, not implicit broader access.

### Quality gates and continuation

1. Ready: accepted source contracts and dependencies available; task fits output
   and review capacity; no unresolved choice hidden behind a placeholder.
2. Generate: implementation and relevant tests in bounded paths. Worker edit tasks
   use clean Git, validated contracts and detached worktrees. Codex retains secrets,
   architectural decisions and final validation. Analyze smoke test is verified;
   real edit execution still needs validation when first used.
3. Verify: Codex inspects receipt, tool events, changed paths/limits and the actual
   diff; runs applicable build/lint/typecheck/tests independently. Use real SQL
   Server for SQL-specific invariants, locks, triggers, grants and transaction races.
   A failed or unavailable required check leaves that outcome unresolved.
4. Review: behavior matches accepted guards/privacy/errors/save units and is simple
   enough for the owner to explain. Tests cover meaningful edge/failure conditions,
   not just the implementation's happy path. Review dependent callers/interfaces.
5. Record: update the task/slice status, commands/results and remaining integration
   obligations in memory. Next dependent generation starts only on a verified
   baseline; do not accumulate unreviewed dependent diffs. Small independent work
   does not authorize overlapping concurrent worker edits.

Worker changes remain isolated until owner permission to apply to primary. Testing
an approved dependency chain within the same isolated worktree does not itself
approve applying it. No merge/apply permission inferred from the quality principle.

### Illustrative M0 decomposition — proposal, not executable contracts

| Task family | Bounded outcome | Required evidence before dependent work |
| --- | --- | --- |
| Development profile and dependency pins | Concrete compatible development/test configuration and prerequisites | Actual tool/profile checks; do not invent D12/D13 values. |
| Backend skeleton | Agreed backend/test layout builds | Build/test project discovery and a meaningful startup check. |
| Frontend skeleton | Agreed frontend layout and shared HTTP/error boundary build | Existing build/lint/typecheck targets execute; no business success mocks. |
| Identity persistence | A coherent E03 identity/schema/bootstrap foundation | Real SQL migration/constraints and safe bootstrap checks; cross-domain schema dependencies explicit. |
| Shared representations | Exact normalization/time/money/RowVersion contracts, split by complexity | Accepted boundary examples and meaningful invalid-input tests. |
| Student signup | Atomic self-registration service/API | Uniqueness races, rollback, privacy and accepted validation. |
| Session/auth operations | Login/context/logout and session checks, split into bounded operations | Real revocation/timers/CSRF/role checks per operation; no token persistence. |
| Identity UI integration | Real signup/login/own-context/logout flow in small UI tasks | Backend-connected states/permissions and required responsive/browser checks. |
| Restricted password flow | Actual temporary-password lifecycle and relevant UI | One-time secret handling, expiration and all-session invalidation. |
| Audit/replay foundation and first caller | Exact shared contracts then one real authorized Admin command | Atomic completion/audit, current rights, duplicate/conflicting intent and lost-response handling. |

This table is a sizing illustration, not an instruction to generate a whole row at
once. Codex must split further if dependency, error or concurrency complexity warrants.

## Proposed milestones — not an approved execution sequence

| Milestone | Reviewable outcome | Important verification |
| --- | --- | --- |
| M0 — Foundation and identity pilot | Reproducible development setup, agreed schema/migration strategy, seed/bootstrap strategy, common DTO/error/time/money/version conventions, real Student self-registration/login/own context/logout and restricted-password flow; staff/Admin bootstrap strategy explicit. Shared command/replay/audit infrastructure exercised by an actual authorized account command before business UUID commands. | Real SQL Server constraints and trigger/EF behavior for affected tables; session revocation, CSRF, ownership, invalid inputs; no secret replay. Bootstrap is not an Admissions create-Student feature. |
| M1 — Configuration and admissions | Staff account administration needed for actual roles; Course/Band/Program prerequisites; entry sessions; Student Registration/booking; Teacher grading/absence/correction; Band and Student confirmation; Admissions approve/reject/cancel. Approval includes atomic Invoice issuance even before Accountant UI is built. | Last-place and first-use races, two-attempt limits, live time guards, configuration freeze, scope, cancellation dependencies, audit and replay. |
| M2 — Payment and class preparation | Accountant confirmation/correction; Student own Invoice/print; Class lifecycle preparation, placement, concrete/weekly schedules and move/replacement; calendars scoped by role. | Payment-versus-cancellation, duplicate payment, capacity/overlap across tests and lessons, full-batch rollback, current assignment and replay rights. |
| M3 — Teaching and publication | Start/HELD/completion, per-LESSON Attendance, per-Student scores/comment, whole-Class submit/return/resubmit/finalize and Student published results. | Missing versus zero, current obligation chains, grade precision/rounding, whole-cohort atomicity, stale form and completion races, no draft leaks. |
| M4 — Reports and final evidence | Role-scoped reports, bounded CSV, complete audit viewing; close remaining 32F/21W/154API/55commands/6lookups obligations, SQL rubric, responsive/browser/demo and recovery evidence for approved environment. | Snapshot totals/filters, CSV bytes/footer/formula protection, 7-role privacy, all required SQL objects/callers/grants, cross-contract E05 cases, reproducible setup and restore. |

M1–M4 are proposed four implementation phases, not boundaries inherited from
an older kit. Each must be broken into smaller reviewable slices before execution.
Audit writers and security/account controls are implemented with their callers;
M4 completes viewing/reporting/evidence rather than postponing integrity/security.
Each SQL object is introduced with its actual dependent flow. Preserve mandatory
objects and full final schema; decide migration granularity in planning, not by
silently reducing E03. M0 checks affected constraints first, not all future flows.

## Definition of done for each slice

- Trace accepted F/W/API/DTO/TX/data/verification obligations and remaining gaps.
- Use real database/API/UI; no mock-only success for a business transition.
- Verify scope, guards and atomic audit/replay when applicable, stale/conflicting
  writes and meaningful failures; run relevant xUnit/SQL integration checks.
- Demo the flow and preserve evidence; update affected Project Memory.
- Later review does not waive invariants or redefine business rules to pass tests.

## Environment and deferred work

D12 performance targets and D13 concrete hosting/exposure remain deferred.
Agree a runnable development/test profile before execution requiring it; accepted
Secure/same-origin/CSRF and fail-closed rules still apply. Planning may identify
required fields without assigning an unapproved port/provider/budget. Patch pins,
blocklist provenance/license, SQL compatibility, database principals/bootstrap
and test isolation must be resolved for the affected execution slice. Backup
media/keys/isolated restore are prerequisites for actual backup/recovery evidence.
Do not claim load/RPO/RTO success without the approved environment and measurements.

## Next refinement

Detailed quality-based [M0 task plan](M0_QUALITY_PLAN.md) now lists prerequisites,
source trace, small task families/dependencies, verification and proposed scopes
for the first three tasks. Review ordering and materialize each English contract
only after actual prerequisites and file targets are known. Full schema/object
readiness is not replaced by partial-table checks. Existing local SQL2025 Express
is retained by ADR-162 after [local assessment](SQL_INSTANCE_ASSESSMENT.md).
No Developer installation prerequisite; P01 package/profile gates remain.
No deadline input required; no source generation/installation/worker dispatch
from this planning output. M0–M4 and detailed task ordering remain proposals.

## Persisted working order

M0's detailed proposed default sequence and Mermaid live in
[M0_QUALITY_PLAN section11](M0_QUALITY_PLAN.md#11-thứ-tự-mặc-định-đã-lưu-và-sơ-đồ).
Next=P01planning/preflight; schedule is not a prerequisite. Detailed dependency
corrections precede execution. M1–M4 remain coverage proposals pending their own
small task plans. No plan-source disagreement should be resolved from chat alone.
