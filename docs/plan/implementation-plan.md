# Implementation planning — discussion draft

Full M0 remains IN PROGRESS. V01-N/O/P component review PASS/applied ADR163/
reverified2026-10-08 main d145072 / manager63cff80:37new +1132existing =
1169PASS/58intentional opt-in SKIP/build0warnings0errors on both checkouts.
Identical src9585d9f65871b305aa3dc19fa7b305e0d31ca3fc,
tests6650877cfb965564b0df7d29ec45d656ccf5ba3a; frontend unchanged.
Explicit bounded RAM request/cooperative deadline boundary, direct local HTTPS/
CSRF binding with actual TLS leaf-pin/SAN/time/EKU probe, and separate maintenance
scheduler/strict authority probe implemented, all unmounted. No positive SQL trust,
runtime login/catalog or cleanup-effect proof claimed. P read-only SELECT returned
exact9facts and denied privileged diagnostic identity; before/after D07 byte-identical
10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E.
AGY8c2c2519-d14 SUCCESS/sole27line guard/full3reads/zero observed violations;
Codex fixed/reviewed/tested; original retained, integrated EOF normalized only.
Four owner hashes unchanged/unshared/untracked/unstaged. No package/default entry/
schema/grant/login/OS-trust changes or cleanup writes. See V01NOP_TASK_CARD/
NOP-verification-summary/notes/V01NOP and ADR182.
Next select concrete M0_LOCAL_DEV_PROPOSAL (new exact-value question pending), then
materialize actual runtime/catalog/PKI/key-store/maintenance/startup contract and
rerun integrated evidence. E04 owner-deferred values are not guessed or promoted
from fixtures; no M1–M4/push/deploy. All9 concrete categories now exist, unregistered.

Historical V01-J/K/L/M checkpoint:
Owner assigned FULL M0 completion2026-10-08; status IN PROGRESS, not two-package scope.
V01-J/K/L/M completed/reviewed PASS/applied ADR163/reverified: JKL main47c9c7f /
manager4ff6f12; M mainfecb90e / manager7d99619. Identical backend src
51d5c80913fe325ee9d9db7d70c943e5a8b8003f, tests ae4f240eb42662384dd58e74851d76a9a57d4a24.
JKL47new + M24new; manager/main1132PASS/58intentional opt-in SKIP/build0warnings0errors.
J fresh loaded runtime/approved artifact hashes and bound SQL-version resolver;
K actual generated RAM signer/validator/rotation proof; L existing DPAPI-encrypted
framework keys/restart decryption through read-only snapshot; M strict explicit local
reference-profile reader without defaults/secret resolution. Six concrete probes
now exist but are unregistered; transport/execution/maintenance implementation and
actual binding still absent. J SQL version tests synthetic, no positive SQL readiness.
AGY82e61f9f-153 SUCCESS/sole96line policy/full3reads/zero observed violations;
Codex independent review/fixes/tests, original retained (EOF normalization only).
SQL not rerun; D07/default Program/frontend/packages/owner4 files unchanged; owner
hash-identical/unshared/untracked/unstaged. No M1–M4/push/deploy opened.
Required actual profile values remain pending (missing-values question sent):
kind/origin/listen/port/budgets/body/preview; runtime identity/catalog visibility,
PKI/key custody and maintenance bindings remain unresolved. No diagnostic fallback
or grant repair. Continue full assigned M0 when inputs arrive, not unrelated tasks.
See M0_COMPLETION_TASK_CARD, M0_ENVIRONMENT_INPUTS, J-M-verification-summary,
notes/V01JKLM and ADR181. Full V01/M0 remains INCOMPLETE.

Historical V01-H/I checkpoint:
V01-H→I owner-assigned two-package batch completed/reviewed PASS/applied ADR163/
reverified2026-10-08: H main7df8ba8/managerf403cec, I maind59e905/manager0713687.
Final identical src20afd339ee6b2f0316fb52d0c1d08f88393f794b /
testse11e2656b48ab3d2d62afbc4d96bc05f9cc5a20a. H real BCL UTC+7 and fresh pinned
metadata/license/corpus probes; bounded async P08 loader retains sync behavior.
H45new/1028defaultPASS; I33new/1061defaultPASS+3real readonly SQL denial cases
PASS on manager/main,58intentional opt-in SKIP/build0warnings0errors. I binds RAM
connection resolver/embedded manifest, owns fresh EF160/local shared-memory context,
strict known runtime/no extra effective privilege guards and runtime-only inspector.
SQL diagnostic rejected; counter proves4fresh principal rows/checkout, no ready claim.
Two bounded AGY SUCCESS/94+23line policies/full3reads/zero observed scope violations;
Codex independent integration/review/tests. D07 before/after byte-identical catalog/
profile/102permissions/24empty tables/0fixture users/members. Owner4 hashes preserved,
unshared/untracked/unstaged. Default Program/DI/routes/frontend/packages/schema/
inspector/corpus unchanged; no provisioning/grant repair/exposure/runtime mounting.
See V01HI_TASK_CARD/HI-verification-summary/notes/V01HI.md/ADR180/closure matrix.
Full V01/M0 incomplete: next bounded runtime/patch manifest and key/protected-store
probe contract; transport/execution/maintenance probes also absent, actual principal/
catalog visibility/PKI and accepted D12/D13/DF01–03 remain deferred/unresolved.

Historical V01-G checkpoint:
V01-G completed/reviewed PASS/applied ADR163/reverified2026-10-08 code main233a274,
managerd497384; identical backend src aa437f569dfef69b392f0f88fd9a0e225b7b046c,
tests7c3b3d2afb6c29c9602f9f308005edb9d726fde6. Unmounted immutable essential
profile descriptors, fail-closed validation/remaining deadline gates and fresh
nine-category readiness orchestration; missing/ambiguous/failed/timed-out probes
return not_ready/503, caller cancellation propagates, fixed secret-free diagnostics.
Manager/main92new+891existing=983PASS/55opt-in SKIP/build0warnings0errors.
AGY2b90a6c8-151 SUCCESS/full3reads/sole37line deadline helper/zero observed scope
violations; Codex independent fixes/review/tests. No concrete probes registered,
secret/config loading, Program/DI/route/frontend/SQL/schema/grant/package changes.
Four owner files hash-identical/unshared/untracked/unstaged; SQL not rerun.
See V01G_TASK_CARD/G-verification-summary/notes/V01G.md, ADR179 and closure matrix.
Full V01/M0 incomplete: next prepare bounded concrete read-only dependency-probe
contract; runtime principal/catalog visibility, protected stores/PKI and accepted
D12/D13/DF01–03 environment values remain unresolved/deferred. No repeat questions.

Historical V01-F checkpoint:
V01-F completed/reviewed PASS/applied ADR163/reverified2026-10-08 code main652e48a,
manager1da7463; identical backend test tree7251f84550e652456497d592c9fb317fa3bc2b18.
Manager/main F6real Edge HTTPS→UI/API→ownedSQL COMMIT + E12regression =18PASS;
default891PASS/55intentional opt-in SKIP/build0warnings0errors. Signup3widths,
seven roles, tabs/profiles/logout, normal/restricted password and lost-lock-ack
manual completion now observe committed effects through fresh SQL connections.
Independent LOGIN/LOGOUT/ACCESS_DENIED history persists; mandatory counts stay
separate. AGYefc6e268-2ca SUCCESS/full3reads/sole69line helper/zero observed scope
violations; Codex fixed/reviewed/tested. All54F scratch databases identity-cleaned;
D07 baseline/profile/catalog/102permissions unchanged. Production/default entries/
packages/browser driver/owner4files unchanged; owner files unshared/untracked/unstaged.
See V01F_TASK_CARD/F-verification-summary/notes/V01F.md/V01_READINESS_CLOSURE.
Full V01/M0 remains incomplete: next prepare unmounted fail-closed typed profile/
readiness implementation contract; actual runtime identity/catalog visibility,
protected key stores/PKI and owner-deferred DF01–03 values are unresolved.
Keep accepted D12/D13 deferrals; no repeat questions or unrelated product families.

Historical checkpoints below:
V01-E completed/reviewed PASS/applied ADR163/reverified2026-10-08 code main95bb665,
manager3a98377; identical backend test tree55255cc6d158eeb3c6b35b4d373ddfc48d7a89dd.
Manager/main12owned SQL COMMIT cases PASS/0fail0skip; default891PASS/49opt-in SKIP,
build0warnings0errors. Actual separate SNAPSHOT/live final authority, independent
security audit persistence/fallback and four bounded two-client races verified.
Each case uses fresh synthetic scratch DB/existing6migrations/identity-checked DROP;
all36created databases removed (35normal/1exact recovery after AUTO_CLOSE issue).
D07 digest/profile/catalog/102permissions/24empty tables unchanged; owner4files
unchanged/unshared/untracked/unstaged. Production src/defaults/packages unchanged.
AGYe3b66916-9de SUCCESS/full3reads/sole76line helper/zero observed scope violations;
Codex independent fixes/review/tests. See V01E_TASK_CARD/E-verification-summary,
notes/V01E.md and ADR178. Full V01/M0 incomplete: next prepare readiness closure
matrix/contract for production profile/principals/PKI and combined browser→owned
COMMIT evidence. Diagnostic Windows SQL identity is not runtime-principal proof.
ADR177 covers assigned aspects; no repeat permission gates or unrelated families.

Historical V01-D checkpoint:
V01-D reviewed PASS/applied ADR163/reverified2026-10-08 main8920098/manager3d40160:
37real HTTPS/browser+HTTP→SQL rollback cases PASS; default891PASS/37opt-in SKIP;
frontend48tests/types/lint/build PASS, buildclean. Baseline/owner files unchanged.
Bounded AGY49915a6c-e94 sole53lines/full3reads; Codex independent fixes/review.
Default entries/production src/packages/schema unchanged; test listeners stopped.
Full V01/M0 incomplete: next V01-E owned COMMIT/cleanup/SNAPSHOT/security-audit/race
contract. Standing owner completion authority ADR177 applies; no repeat permission
questions for assigned aspects. See V01D_TASK_CARD/D-verification-summary.json.

Earlier checkpoints below are historical.


V01-B/C applied/reverified PASS2026-10-08 code mainbca0e4e/manager7ed9f85:
27HTTP→SQL PASS/default891PASS/27opt-in SKIP/buildclean. Exact AGY/SQL scopes approved,
rollback catalog/profile/102permissions unchanged/0retained rows/users/members.
Test-only caller-TX bridge; default production entries unchanged. Full V01/M0
incomplete: browser HTTPS/principals/owned COMMIT/races need scoped authority/evidence.
See V01BC_TASK_CARD/BC-verification-summary. Earlier family checkpoints below historical.

## Latest U05/U06 checkpoint — applied and reverified PASS

Owner-authorized U05→U06 complete/reviewed PASS/applied ADR163/reverified2026-10-08.
Code mainf83a7d6 / manager0ff2ae7; identical frontend4050338d256ad268fe11cc1de7180e39f08a1458.
48frontend tests/0fail0skip, typecheck/lint/build and Edge375/768/1280 PASS on both.
Two bounded AGY SUCCESS/full3contexts before sole writes/zero observed violations;
Codex independent integration/fixes/review; originals and receipt/event hashes retained.
U05 exact raw password/RV/local-only confirmation, normal/restricted scope,204→login;
no auto-login/upgrade/retry/storage;400/403/409/500/lost/dirty navigation verified.
U06 explicit Admin NORMAL detail/lock pilot, frozen target/RV/UUID after confirmation,
current response and manual safe lookup;404 is not rollback, no polling/write replay.
All6other roles/restricted Admin blocked; stale identity response clears preview.
Default App/main unmounted; backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a unchanged.
SQL not rerun; real cookie/HTTPS/profile/keys/principals/runtime proof unverified.
Four owner files unchanged/unshared/untracked/unstaged. See U05/U06 cards/evidence,
notes and ADR176. Next V01 unstarted; no restM0/runtime/SQL/packages/push/deploy scope.

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
At the R02/R03 checkpoint U02/U01 were unstarted; latest state is above.

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
