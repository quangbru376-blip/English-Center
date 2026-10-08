# English Center Management System — Project Memory

Latest owner assignment2026-10-08: COMPLETE M0, not only the next two packages.
J/K/L/M applied/reverified PASS main47c9c7f/fecb90e; manager4ff6f12/7d99619,
1132defaultPASS/58intentional opt-in SKIP/buildclean. Fresh runtime/artifact, JWT
material, protected read-only DPAPI keys and strict reference-profile reader now
implemented but unmounted. Six concrete probes exist; actual transport/execution/
maintenance/bindings still absent. Required environment values were asked and are
pending; preserve accepted D12/D13 deferrals, no guessed budgets/origin/port or
diagnostic SQL fallback/grant repair. Continue assigned M0 after input; no M1–M4.
See M0_COMPLETION_TASK_CARD/M0_ENVIRONMENT_INPUTS/J-M evidence/ADR181/current handoff.
Four owner files hash-identical/unshared/untracked/unstaged; no SQL/package/default
entry changes. Full V01/M0 INCOMPLETE; older narrow scope statements are historical.

This file is the entry point for agents working in this workspace. Before any
non-trivial task, read these files in order:

1. `docs/PROJECT_CONTEXT.md`
2. `docs/ARCHITECTURE.md`
3. `docs/CURRENT_STATE.md`
4. `docs/DECISIONS.md`
5. `docs/TODO.md`

Then read the applicable execution plan in `docs/plan/` and only the
instruction files or skills relevant to the task.

For product planning, implementation, and review, also read
`docs/requirements.md`. It is the canonical product specification, normalized
from the official report and the owner's accepted decisions. Source provenance
is in `docs/reference/REPORT_BASELINE.md`. Do not modify the source Word report.

The full design baseline E01–E05 is Accepted (ADR-160/161), tracked in
`docs/plan/design-baseline.md`. The current stage is quality-based implementation
planning in `docs/plan/implementation-plan.md` and `docs/plan/M0_QUALITY_PLAN.md`.
P02/P03/P05/P04/P06 remain applied PASS. On2026-10-04 the owner requested
three next families P07→P08→D01 in one turn; this batch is complete/reviewed
PASS/applied/reverified, final code mainf6fd902 full400tests PASS. See
CURRENT_STATE and docs/plan/P07_TASK_CARD.md, P08_TASK_CARD.md, D01_TASK_CARD.md.
P07 supplies safe error/binding primitives; P08 pinned offline/raw credential
helpers; D01 four-table EF mapping without SQL execution or migrations. One
D01 AGY overread run rejected; Codex independently completed the final mappings.
On2026-10-05 D02 continuation completed/reviewed PASS/applied/reverified main5a4ba3d:
Course/Band/TrainingProgram/ScoreComponentConfig mapping, full8tables/56columns,
410tests PASS/build0warnings0errors. Four AGY runs had zero observed scope violations;
Codex fixed/reviewed and independently tested. See docs/plan/D02_TASK_CARD.md.
D03 continuation2026-10-05 completed/reviewed PASS/applied/reverified main244353f:
Registration/EntryTestSession/EntryTestAttempt/EntryTestBooking/BandAssignment,
full13tables/116columns/2AK/23NO ACTION FK/37indexes/34checks/12RV;
427tests PASS/build0warnings0errors. Three AGY runs had zero observed violations;
Codex independently tested and reviewed. See docs/plan/D03_TASK_CARD.md.
FK indexes are explicit under ADR-164; full lookup coverage verified.
D04 continuation2026-10-05 completed/reviewed PASS/applied/reverified main e741176:
Class/StudentClassAssignment/Schedule mapping, full16tables/151columns/33FK/
51indexes/46checks/14RV;440tests PASS/build0warnings0errors. Three AGY runs had
zero observed scope violations; Codex independently tested/fixed/reviewed.
See docs/plan/D04_TASK_CARD.md.
D05 continuation2026-10-05 completed/reviewed PASS/applied/reverified main9e843b8:
Attendance/Score/GradeReport mapping, full19tables/180columns/43FK/62indexes/
52checks/17RV;454tests PASS/build0warnings0errors. Three AGY runs had zero
observed scope violations; Codex independently tested/fixed/reviewed.
See docs/plan/D05_TASK_CARD.md.
D06 continuation2026-10-05 completed/reviewed PASS/applied/reverified main543fcf0:
Invoice/Payment/AuditLog/ReplayOperation/InvoiceNumberCounter mapping,
complete24tables/230columns/50FK/78indexes/70checks/20RV;474tests PASS/build clean.
Three bounded AGY runs stayed within observed scopes; Codex independent tests/
fixes/review. See docs/plan/D06_TASK_CARD.md.
D07→D08→D09 owner-requested batch2026-10-05 complete/reviewed PASS/applied/
reverified code maina7561dd:481unit+102SQL PASS/build0warnings0errors. D07 adds
private Design/localdotnet-ef10.0.4 from cache/disconnected160factory/InitialSchema;
owner explicitly approved isolated SQL new EnglishCenter_D07_20261005 only,
fail-if-exists/profile160/collation/snapshotON/migration+object rollback fixtures.
Up→Down→Up/schema PASS:24tables230cols50FK78IX70CHECK20RV; all230columns verified.
D08 exact5set-based preheld-gate guards, D09 six pure exact-signature UDFs;
11bounded AGY SUCCESS/zero observed violations, Codex independent fixes/review.
Three migrations; fixtures rolled back, database retained/no DROP DB. See D07/
D08/D09_TASK_CARD and evidence. D10/D11 subsequently owner-authorized2026-10-05,
complete/reviewed PASS/applied/reverified main6aaa4d7:483unit+182SQL PASS/buildclean;
6views/7SP/5migrations, exact47params/live24module bodies. Exact AGY payload +
existing isolated SQL scope explicitly approved;13SUCCESS/1ERROR rejected, no
observed scope violations. See D10/D11_TASK_CARD/evidence. Owner subsequently
requested D11/D12 and approved exact D12 payload/isolated roles and four WITHOUT
LOGIN test users. D12 roles/grants reviewed PASS/applied/reverified code mainfc9227f:
484unit+335SQL=819PASS/buildclean; four roles/exact102permissions/six migrations.
Four AGY SUCCESS/zero observed worker violations, Codex independent review/tests.
Down→D11→Up and fixture/user/membership rollback PASS. Initial extra manager probe
rolled back/verified absent and discarded; see D12_TASK_CARD/evidence. Login-based
auth/demo bindings and owner-context health are unverified operator prerequisites;
DDL/history15517 is not alone permission proof. Owner-approved D13→A01 batch now
complete/reviewed PASS/applied/reverified2026-10-05 main74e3085:513unit+366SQL=879PASS/
buildclean; manager0116002 identical backend67ffc5d0fde4c150f0f5b71287701886a3315fbc.
Two AGY SUCCESS/full bounded context reads/zero observed scope violations; Codex
independent exact catalog/model/SQL review and audit failure/rollback/fallback tests.
D13 catalog/EF structural diagnostic PASS; full runtime profile/login readiness
remains blocked, runtime role invisible catalog fails closed/no grant added. A01
mandatory caller-TX and independent security/fixed incident-counter primitives only;
no real auth/API/business callers or cleanup/DI. All approved SQL fixtures rollback;
0retained business rows/users/memberships, unchanged complete catalog/102permissions.
See D13/A01_TASK_CARD/evidence. This earlier batch did not open A02/A04;
no runtime/server logins/owner changes/other DB/restM0/push/deploy authority.
Owner-approved A02→A04 scope2026-10-05 now complete/reviewed PASS/applied/reverified
maina8ad1d5:545unit+375SQL=920PASS/buildclean; managerd1612ed same backend
8a7e73f1bf9114311c9484f5c7c16d639ebd5916. Two bounded AGY SUCCESS/full authorized context
reads/zero observed violations; Codex independent review/rollback/host tests. A02
operator-only caller-TX bootstrap preserves existing Admin; A04 explicit framework
CSRF API001 boundary fails closed. Default host unchanged, no real Admin/runtime
activation. Generated RAM-only test secrets; all approved SQL fixtures rollback,
0business rows/users/role members; exact catalog/102permissions unchanged. No new
schema/migration/grants/profile/owner/logins/keys/other DB/push/deploy. See A02/A04
cards/evidence, ADR169. Next A03 signup→A05 login/session remains unstarted; actual
operator provisioning/production keys/profile/resolver/full readiness deferred.
Owner-approved A03→A05 scope2026-10-06 completed/reviewed PASS/applied/reverified
maina9abdc6; manager e5ff366 identical backend53e36afc0d1ad2b9232d04010222e805ca009410.
586unit/HTTP+409SQL=995PASS/build0warnings0errors. Two bounded AGY SUCCESS/full
context before write/zero observed violations; Codex independent fixes/review/tests.
A03 Account+Profile+mandatory audit atomic signup; A05 generic5/15/15 throttle,
fresh locked account/session issuance, supplied JWT signer and postcommit cookie.
Explicit adapters remain unmounted; no real Admin/production keys/profile/runtime.
All approved SQL fixtures rollback;0retained dbo rows except history/0test users/
role members;4roles102permissions6migrations/profile unchanged. Owned SQL commit
paths source-reviewed plus HTTP fixture seams, not persisted runtime/stress proof.
See A03/A05 cards/evidence, ADR170. Next A06 current-session/me→A07 logout unstarted;
no A08/restM0/schema/grants/owner/logins/other DB/installation/push/deploy authority.
Owner-approved A06→A07 completed/reviewed PASS/applied/reverified2026-10-06
mained40a4b, manager633671b, identical backend0d407841beeb719bbd753e828a9a5d408549a1bc;
632unit/HTTP+438SQL=1070PASS/build0warnings0errors. Two bounded AGY SUCCESS/
zero observed scope violations; Codex independently integrated/reviewed/tested.
Fresh JWT/current Account→Session authority, exact me context/non-renewing reads,
explicit activity hook/current-SID logout and independent audit. Invalid cookie401
cleanup permits login recovery. Default Program unmounted; SQL caller-TX fixtures
rollback only; owned COMMIT paths source-reviewed/HTTP fixture seams, no persisted
runtime or committed stress proof. Retained rows/users/members0; exact catalog/
102permissions/profile unchanged. See A06/A07 cards/evidence, ADR171.
Next A08 password change→R01 canonical replay registry unstarted.
Owner-approved A08→R01 completed/reviewed PASS/applied/reverified2026-10-06
code main3b46460, manager18c0af7; identical backend4fb65a2e2f0e3ef9fc95dd040cf4438c4156be24.
733unit/HTTP/C1+462SQL=1195PASS/build0warnings0errors on manager and main.
Two bounded AGY SUCCESS/full authorized context/zero observed scope violations;
Codex independently fixed/integrated/reviewed/tested. A08 exact API004 raw credentials,
fresh normal/restricted proof/RV, Account→binary SID locks, atomic hash/version/temp
clear/all-own-session revoke/mandatory audit; postcommit204 cookie deletion.
R01 disconnected exact55 registry/6lookup groups/schema-aware EC-C1 bytes/SHA256;
no T23 completion storage or live replay routes. Program unmounted; SQL rollback-only,
owned COMMIT source-reviewed/HTTP fixture seams, no persisted runtime/stress claim.
Retained business rows/test users/members0;4roles102permissions6migrations/profile
unchanged. No schema/grants/owner/logins/other DB/install/push/deploy. See A08/R01
cards/evidence, ADR172. Next R02 completion/TX/lookup→R03 Admin lock pilot unstarted.
Owner-approved R02→R03 completed/reviewed PASS/applied/reverified2026-10-06
code mainfcdd21e, manager1cb6ebc; identical backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a.
891unit/HTTP/C1+495SQL=1386PASS/build0warnings0errors on manager and main.
Two bounded AGY SUCCESS/full3contexts before write/zero observed scope violations;
Codex independently integrated/fixed/reviewed/tested. R02 exact append-only completion/
24h/request gate/mandatory current target authority before digest; no response snapshots.
R03 explicit007detail SNAPSHOT+final live authority/010atomic Admin lock/014safe6action
lookup. Request→ADMINSET→ascending Account union→binary SIDs; stale RV bypass only
matched active replay, no repeat effects/audit/activity. Status/security version/all-own
SID revoke/audit/T23 share caller TX; self/last-active guards and strict UUID/RV retained.
Program unmounted; SQL rollback-only, owned COMMIT source-reviewed/HTTP fixture seams;
no persisted runtime/committed stress proof. Retained business rows/test users/members0;
4roles102permissions6migrations/full catalog/profile unchanged. See R02/R03 cards/evidence,
ADR173. Next U02→U01 unstarted; no frontend/restM0/runtime/install/push/deploy authority.
Owner-authorized U02→U01 completed/reviewed PASS/applied ADR163/reverified2026-10-07
code mainfb748e7/manager48ff8a0, identical frontendde38145e6127d32cfcf20629387b18415e2c5746.
24frontend tests/types/lint/build PASS; Edge375/768/1280 keyboard/dialog/contrast PASS.
Two bounded AGY SUCCESS/full3contexts before writes/zero observed scope violations;
Codex integrated/fixed/reviewed independently. Native controls/shell and explicit RAM
HTTP/auth request epoch seams only; unavailable App, no mounted real identity UI.
Backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a unchanged; SQL not rerun.
Four owner untracked files unchanged/unshared/unstaged. See U02/U01 cards/evidence,
ADR174. At that checkpoint U03→U04 was unstarted; latest state follows.
Owner-authorized U03→U04 completed/reviewed PASS/applied ADR163/reverified2026-10-07
code maind51564e/managerd18a34a, identical frontend851877ef7396885e51d39518697659277fa0effc.
38frontend tests/types/lint/build/Edge375/768/1280 PASS. Two bounded AGY SUCCESS,
full3contexts before write/zero observed scope violations; Codex independent review.
Explicit signup201→login/no auto-login and login/me/logout/native signals/deadline
seams; default App/main unmounted. Backend unchanged; SQL not rerun; four owner
files unchanged/unshared/untracked/unstaged. See U03/U04 cards/evidence, ADR175.
At that checkpoint U05→U06 was unstarted; latest state follows.
Owner-authorized U05→U06 completed/reviewed PASS/applied ADR163/reverified2026-10-08
code mainf83a7d6/manager0ff2ae7; identical frontend4050338d256ad268fe11cc1de7180e39f08a1458.
48frontend tests/types/lint/build/Edge375/768/1280 PASS. Two bounded AGY SUCCESS,
full3contexts before writes/zero observed scope violations; Codex independent fixes/review.
U05 raw own password/RV/204->login/no upgrade; U06 Admin NORMAL detail/lock/frozen
UUID/manual safe lookup/no polling or replay. Stale/role/restricted/unknown verified.
Default App/main/backend unchanged; SQL not rerun; owner files preserved untracked.
See U05/U06 cards/evidence, ADR176. V01 preflight/V01-A2026-10-08 reviewed PASS:
891non-SQL+120SQL/0fail0skip. Owner approved exact five existing SQL classes/
rollback only; before/after catalog/profile/102permissions identical,24empty
tables/0fixture users/members. See V01_TASK_CARD. Integrated V01 incomplete;
no runtime/owned COMMIT/restM0/packages/push/deploy authority.
Owner-authorized V01-B/C complete/reviewed PASS/applied ADR163/reverified2026-10-08
code mainbca0e4e/manager7ed9f85, identical test tree32decf9a213277debd9e6c9cc17783efec711aea.
27HTTP→SQL PASS on manager/main; default891PASS/27opt-in SKIP/buildclean. Exact2AGY
payloads and both SQL rollback scopes explicitly approved; bounded SUCCESS71/84lines,
Codex independent fixes/review/tests. Catalog/profile/102permissions unchanged,
24empty tables/0fixture users/members; owner files preserved. Production backend/src/
frontend unchanged; full V01/M0 still incomplete. See V01BC_TASK_CARD/verification;
no browser HTTPS/runtime principals/owned COMMIT/restM0/install/push/deploy authority.
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
Historical V01-E checkpoint:
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
Historical initial scope is retained in
FIRST_CODE_HANDOFF; latest checkpoint and explicit current owner scope govern.
Owner expanded standing task authority2026-10-08 (ADR-177): each assigned task
includes the necessary implementation, bounded delegation/context sharing,
verification and operational aspects to finish that task. Do not re-ask already
authorized aspects. Prepare concrete scopes and preserve task boundaries/owner
files; this does not assign unrelated product tasks or imply push/deploy.
For current V01-D this supersedes the pending payload/HTTPS/rollback SQL gate.
Historical narrower gates below describe their checkpoints, not a new veto.
Standing owner authority ADR-163: apply Codex-reviewed PASS stages already
authorized for implementation to main without asking again. Report issues
requiring owner action. Keep unreviewed/rejected worker output detached.
This authority does not open unrelated tasks, SQL/migrations, installation,
push or deployment. Original reviewed/rejected trees are retained as evidence.
Project-local skills live in `.agents/skills/`. Read the selected skill's
complete `SKILL.md` before using it. Installing a skill does not automatically
start its connection or delegation workflow.

Before every code-writing task, follow `docs/plan/SKILL_ROUTING.md`. Select and
read the applicable complete skills: backend-clean-code for backend/DTO/API/SQL
work, frontend-ui for frontend/UI work, both for changes spanning both scopes.
Codex uses code-review before code tasks or integrated slices are marked done.
Record selected skills, source versions, canonical overrides and verification
in the task card. Worker contracts must explicitly include the selected skill
files/policy in authorized read context and require their use; skills grant no
additional write/command/network/merge authority. Codex verifies actual behavior.
Accepted E03/E04/requirements override stale skill guidance, including JWT-cookie
transport, SQL Server-specific tests, full-set DTOs and exact money representations.
Use antigravity-worker only for bounded AGY delegation, with Codex as manager.
codex-with-chatgpt remains opt-in; reviewing a skill never starts its integration.

`instructions/` is an input inbox supplied by the owner. It is not the source
tree, Project Memory, or an automatically active rule set. An instruction file
becomes authoritative only when the owner says so or Project Memory explicitly
adopts it. Currently adopted English Center inputs are listed in
`docs/PROJECT_CONTEXT.md`.

Older instruction-kit requirements, diagrams, and rules are references only
where compatible with canonical memory. Do not reintroduce scholarships,
ten-point grading, retakes after confirmation, or automated debt penalties.
The report's illustrative forms are not a requirements source. Unresolved
business rules are tracked in `docs/plan/questions.md`, not silently guessed.

Multiple skills and agent integrations may be used together. Follow each
selected skill's scope and handoff protocol; do not assume one integration
owns the whole project. Durable results and handoff state must return to
Project Memory or the applicable execution plan.

The implementation is the strongest evidence of current behavior. If code and
documentation disagree, verify the behavior and update stale documentation;
do not silently reverse an accepted decision.

After meaningful work, update only the memory affected by the change:

- `CURRENT_STATE.md` for implementation status, blockers, and the next action.
- `TODO.md` for short-term work and priority changes.
- `DECISIONS.md` for significant technical or contract decisions.
- `ARCHITECTURE.md` when the implemented architecture changes.
- The active file in `docs/plan/` when a multi-step task advances or changes.

Keep memory concise, current, and actionable. Never store chat transcripts,
hidden reasoning, secrets, temporary logs, or facts that have not been
verified.
