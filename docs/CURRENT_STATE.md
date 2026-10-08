# Current State

Last updated: 2026-10-09

## Current task

Full M0 remains IN PROGRESS. V01-U scheduler lifecycle reviewed PASS/applied
ADR163/reverified2026-10-09: main8bc6a1a / manager942403b.
One test file only; actual BackgroundService/PeriodicTimer loops exercised using
manual test clock and missing/pending RAM resolvers. Six new cases prove explicit
independent periods/no initial run, fresh later attempts/error counts, per-job
non-overlap/coalesced ticks, shutdown with uncooperative resolver, disposed timers,
and invalid-schedule refusal. No SQL/connection/network or production changes.
Manager/main1203PASS/63intentional opt-in SKIP/build0warnings0errors.
Source unchanged b0b7af270d28330541615436e298b462364f35dd;
test tree70ace516246f1e500232657b8242a08cb186d194 identical; frontend unchanged.
Owner4hashes unchanged/unshared/untracked/unstaged. Actual maintenance login/TLS/
scheduler with SQL and service-owned COMMIT remain unverified; U is synthetic lifecycle evidence.
Next actual profile selection/custody/runtime authentication/TLS/startup integration.
LOCAL_DEV proposal remains pending; owner deferrals preserved, no fixture promotion,
M1–M4/install/push/deploy. See V01U_TASK_CARD/U summary/notes.

Historical T: main1f0f0e9/manager7dbcb4d,5SQL PASS each, database-grant-option
guard fixed,90d/24h effects/rollback verified,10scratch databases identity-cleaned.
D07 baseline byte-identical10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E;
24empty tables/4roles102permissions unchanged; SQL not rerun for U.
Historical Q/R/S: HTTP readiness/metadata whitelist PASS,880/880catalog feasibility.


Historical N/O/P: see V01NOP_TASK_CARD/NOP evidence/ADR182.

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

V01-D completed/reviewed PASS/applied ADR163/reverified2026-10-08 code main8920098,
manager3d40160; identical backend test tree55fb9697cd5b9acab2b4e4bf35604bcc3866d0c9
and browser test tree6be790ba5a5e311964f3011e6b8edad8d5978415. Owner standing
completion authority ADR177 covers assigned task aspects; do not re-ask these.
Manager/main37integration PASS/0fail/0skip (D10+B/C27); default891PASS/37intentional
opt-in SKIP/buildclean; frontend48tests/typecheck/lint/build PASS on both.
Real Edge→per-certificate pinned HTTPS→UI/API→caller-TX SQL; signup3widths/seven
roles/CSRF/tabs+profiles/password/Admin lock/replay/lost-ack safe lookup verified.
Node RAM TLS terminator/private loopback nonce preserves Origin; negative trust/
wrong-pin/direct-bridge checks PASS. Windows transient-key/prototype logging/build
retry issues resolved and documented; final evidence sanitized, originals retained.
AGY49915a6c-e94 SUCCESS/full3reads/sole53line helper/zero observed scope violations;
Codex independent review. Baseline digest/24empty tables/0users/members/4roles/
102permissions/6migrations/profile unchanged; browsers/listeners stopped. Four owner
files unchanged/unshared/untracked/unstaged. Production src/defaults/packages unchanged.
See V01D_TASK_CARD/V01-evidence/D-verification-summary.json/notes/V01D.md.
Full V01/M0 remains incomplete: next prepare V01-E concrete owned COMMIT/cleanup,
separate SNAPSHOT executor/independent security audit/race evidence; production
principal/profile/PKI/readiness remains unverified. ADR177 removes repeated task
permission gates, not the need for bounded contracts and actual verification.

Owner-authorized V01-B identity HTTP/SQL and V01-C Admin HTTP/SQL completed/reviewed
PASS/applied ADR163/reverified2026-10-08 code mainbca0e4e/manager7ed9f85; identical
test tree32decf9a213277debd9e6c9cc17783efec711aea.27actual SQL cases PASS/0fail/0skip
on manager/main; default891PASS/27intentional opt-in SKIP/0fail/buildclean.
Two bounded AGY SUCCESS/full3reads/sole71/84line outputs; Codex fixed C target
object/UTC and independently verified clock/nonrenewal/savepoint/rollback cases.
Catalog/profile/102permissions digest identical;24empty tables/0fixture users/
members/4roles6migrations unchanged. Explicit SQL approval resolved initial
auto-review rejection; no bypass. Production backend/src/frontend/entry/package
unchanged; four owner files unchanged/untracked/unshared. See V01BC_TASK_CARD and
BC-verification-summary. Full V01/M0 incomplete: browser HTTPS/runtime principal/
owned COMMIT/separate SNAPSHOT executor/independent persisted security audit/
committed races still require specific scope; no restM0/runtime/install/push/deploy.

V01 manager preflight/V01-A reviewed PASS2026-10-08; integrated V01 incomplete.
Default entries remain unmounted, no runtime/packages opened.
Verified source separates fixture HTTP/browser evidence from real SQL integration.
Owner approved exact V01-A five-class rollback SQL scope:120PASS/0fail/0skip;
before/after catalog/profile/permissions digest identical,24empty business tables,
0fixture users/members,4roles102permissions6migrations unchanged. No AGY/new external
context dispatched. Current non-SQL test
attempts/results and limits are in V01-evidence/preflight-summary.json.
Fresh backend unit/HTTP891PASS/0fail/0skip after testhost connection retry;
no integrated runtime/browser or owned COMMIT claim. See V01_TASK_CARD.

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

## Previous R02/R03 checkpoint

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
At that checkpoint U02/U01 were unstarted; now completed as recorded above.

## Latest applied A08/R01 checkpoint

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

## Previous A06/A07 applied checkpoint

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
Owner requested A08→R01 on2026-10-06. Task/worker contracts prepared; exact new
external payload and rollback SQL consent pending. No worker/SQL/code run yet;
see A08_TASK_CARD.md and R01_TASK_CARD.md. Product decisions remain accepted.
ADR163 permits applying only already authorized reviewed PASS stages.

## Previous A03/A05 applied checkpoint

Owner-approved A03→A05 implementation completed/reviewed PASS/applied/reverified maina9abdc6
2026-10-06; manager e5ff366, identical backend53e36afc0d1ad2b9232d04010222e805ca009410;
586 unit/HTTP +409 SQL=995 PASS, 0 fail/skip, build0warnings0errors.
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

## Previous A02/A04 applied checkpoint

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
Historical next at A02/A04 completion was A03→A05; see current checkpoint above. ADR163 continues only for authorized reviewed PASS stages. Historical scope below.

## Previous applied checkpoint

Owner-requested three families D07→D08→D09 completed/reviewed PASS/applied/reverified
2026-10-05. Final code maina7561dd (manager same), backend tree
c7045ddbe52e1006b649d03877702515a04c5572 identical; locked build0warnings0errors/
481unit+102SQL PASS/0fail0skip. [D07](plan/D07_TASK_CARD.md),
[D08](plan/D08_TASK_CARD.md),[D09](plan/D09_TASK_CARD.md)/D09-evidence.

D07 offline Design/private10.0.4/localdotnet-ef10.0.4 from cache, disconnected
factory/provider160, EF-generated InitialSchema/cycle/Down. Explicit owner SQL
approval followed initial auto-review rejection; no rejected operation executed.
Only new .\SQLEXPRESS/EnglishCenter_D07_20261005 created with fail-if-exists,
compatibility160/Latin1_General_100_CI_AS_SC/snapshotON, retained/no DROP database.
Initial Up→Down→Up and object rollback chains PASS. Physical24tables230columns/
24PK2AK50NO ACTION FK78indexes70trustedCHECKs20RV;230live column names/types/NULL/
identity/collation/RV match model. Five table-scoped OUTPUTfalse mappings preserved.

D08 exact5enabled triggers/14events, set-based old/new guards, preheld Exclusive
Transaction canonical gates checked before invariant handling; Codex fixed TG01
rename/status gate omission. D09 exact6UDFs/11parameter+return rows, pure decimal/
time/missing/current-obligation/unfinished/debt rules; rounding parity PASS.
11bounded AGY runs SUCCESS/exit0/zero observed scope violations, complete card/
backend skill/routing read proof. Codex independent engine tests/source review;
original worker/manager trees retained.3migrations, all fixture transactions
rollback; no real seeds, runtime DB DI/profile/grants/business callers/SP/views.
Private Design absent from publish-check artifact;20added Design dependency pins/
licenses and normalized archive/lock/assets/cache hashes verified offline, no new
vulnerability audit claim. No push/deployment/other DB changes.

No unresolved D07–D09 blocker at that checkpoint. D10/D11 now complete above; D12/D13,
complete service/TX/authorization/audit/replay/callers/grants/readiness/fullLOCAL_DEV/
M0/V01 remain incomplete. Standing ADR163 only applies authorized reviewed PASS
code; this completed batch does not open restM0. Earlier stages remain PASS.

### Applied P02 checkpoint

P01 prerequisites needed by P02 complete; P02 reviewed **PASS** after two
manager fixes. Owner explicitly confirmed main application on 2026-10-04;
applied as df7cd9ac3b7f8b54e6b2814255ca5c424d768b99, reverified PASS on main.
Original detached evidence retained at d9eb194a7d53c321cbfad691abba730388588385.
See [task card/evidence/worktree](plan/P02_TASK_CARD.md) and
[local manifest](plan/M0_LOCAL_PROFILE.md). AGY run11968703-a8c SUCCESS/exit0,
exact8files/149runner lines, zero scope violations; Codex verified tool events,
source, lock graph/licenses/hashes and final build0warnings/0errors,
discovery10/tests10pass/0fail/0skip, no known NuGet package vulnerabilities.
Manager fixes: xUnit cancellation tokens and same-version mtp-off package for
VSTest/SDK10 compatibility. Prior docs preserved1054888; dispatch base72696e5.
Main source exactly matches reviewed detached source; lock hashes unchanged.
Main locked restore/build0warnings/0errors/discovery10/tests10pass/0fail/0skip
verified after apply. No push/deployment. P03 reviewed detached as recorded above.
Rest M0/SQL changes/installs/deployment remain outside scope. Full LOCAL_DEV
and DB/readiness unconfigured; M0/V01 not done. Historical preparation below
describes state before this execution; edit and main application verified for P02.

SQL assessment04/10/2026 completed: installed SQL2025 Express17.0.1000.7
retained by ADR-162; see [assessment](plan/SQL_INSTANCE_ASSESSMENT.md).
Read-only inventory/type/decimal/JSON/SHA and system-DB snapshot read succeeded;
no app DB creation/config changes. Current RTM lacks newer patches; patch review
before real data/exposure and encrypted-archive backup prerequisites remain.
P02 prerequisite package profile now verified; remaining full runtime slots are
listed in M0_LOCAL_PROFILE and require later authorized work.

M0 order synchronized04/10/2026: docs/plan/M0_QUALITY_PLAN.md section11 holds
proposed39family default order and Mermaid; dependencies in sections4–7.
Start=P01 planning/profile. P05beforeP04, A04beforeA03, D07beforeSQLaudit/replay
evidence explicit; full readiness/skill gates retained.
SQL target updated to installed SQL2025 Express by ADR-162; no install/code/dispatch.

Skill audit04/10/2026: AGENTS and docs/plan/SKILL_ROUTING.md now require
scope-appropriate backend/frontend skills and Codex code-review for code tasks.
Task cards/worker contexts include skill sources, canonical overrides and actual
verification.4local skills audited; C2C inactive, no dispatch/code/skill install.

M0 detailed planning04/10/2026: docs/plan/M0_QUALITY_PLAN.md records task families,
dependencies, evidence gates and first3proposed scopes. Local SDK10.0.200/
Node24.16.0/npm11.13.0 checked; SQL .\SQLEXPRESS version17.0.1000.7 Express
is now the accepted SQL2025 Express local target (ADR-162), assessed in
docs/plan/SQL_INSTANCE_ASSESSMENT.md. Keep this instance; no Developer install.
P01 still needs package/profile checks; full DB readiness retained. No app code,
installation, migration or dispatch performed.

2026-10-04 worker readiness completed for analyze mode: owner approved sharing
named context files; first authenticated run rejected/stopped for extra read,
second narrowed run SUCCESS/exit0,exact2allowedreads,zerochanges/violations.
Codex independently checked events/diffs/detachedHEADs and6/6runner tests.
See docs/plan/antigravity-worker-readiness.md and saved contract/receipt.
Baseline preserved d6724a6; local skills preserved ead88d8. Actual edit mode
and application checks untested; no code/merge/apply/push/deploy performed.

2026-10-03: Owner explicitly accepted the two actual outputs in order:
E04-v1 complete architecture/contracts (ADR-160),then E05-v1 consistency review
and complete E01–E04 design baseline (ADR-161),without amendments.
All design stages1–5 done. OWNER_REVIEW75rows/73accepted;only D12/D13 deferred,
original notes preserved. No further in-scope baseline approval pending.

Accepted E04 package: SYSTEM_ARCHITECTURE_SPEC +E04_FINAL_CONTRACT/catalog/
154domainDTO/SC01parity/SC02replay/SC03SQLintegration/SC04runtime. Exact
presented technical refinements now accepted,not merely139–159policy votes.
E05_BASELINE_REVIEW:37NV/32F/21W trace,state parity/24cases/10findings,
excluded/deferred/rubric/evidence boundaries. Source Word/E01/E02/E03/earlier
ADRs/sidework unchanged. Requirements synced to actual160/161 acceptance.

DF01–05 environmental/evidence prerequisites remain: D12/D13 numerical
performance/deploymentvalues,backupmedia/access/key/isolatedrestore,compatible
patch/blocklistpins/config and future SQL/API/EF/race/browser/CSV/load/recovery
verification. Accepted design is not installed/configured/implemented/tested.
Owner now explicitly requests discussing implementation planning (03/10/2026).
Planning is active in docs/plan/implementation-plan.md; M0–M4 are proposals,
not accepted execution milestones. Owner now requires quality-bounded generation
tasks based on complexity/output limits, not calendar deadlines or weekly hours.
Plan defines group/slice/task distinctions and manager verification gates.
That planning request did not authorize execution. The later owner handoff
request04/10 authorizes only P01→P02 as documented above; P02 is now reviewed
and applied with separate owner consent; broad SQL/deployment/runtime-install scope remains closed.
Do not reopen approved contracts.
Report/sequence/C2C work remains on-demand;skill installation is not integration.
Historical checkpoints below describe their original dates,not current status.

## What has been done

- 2026-10-03: Recorded owner actual E04-v1 acceptance ADR-160 first, then E05-v1/full baseline ADR-161; checked only E04/E05,73/75accepted,D12/D13 notes preserved. All design stages done; no implementation/planning/runtime evidence.

- 2026-10-03: Completed review-ready E04-v1 final contract/SC01–04 and prepared E05-v1 individual-NV trace/state parity/24cases/findings/deferred. Linked actual outputs in owner review with all75checks/notes preserved;E04/E05 remain unchecked. No implementation or tests.

- 2026-10-03 (RF approval): RecordedRF01/02/04 as ADR-157/158/159 Accepted;
  synced requirements/E01/E02/E04/docs/memory/HANDOFF. E03/schema/owner gates
  preserved; RF03 remains156, E04/E05 unaccepted, D12/D13 deferred.


- 2026-10-03 (SC02–04-v1): Added replay,SQL integration,runtime appendices and
  SC01 consolidated parity review; synced actual E04/catalog/DTO/memory/handoff.
  154API/55actions/6lookups/18DTO JSON and byte fixtures checked structurally;
  source/review fingerprints preserved. RF04 proposed; E04/E05 still unchecked.


- 2026-10-03 (handoff after D10): Owner requested continuation in another chat.
  Added a new-chat intake block at the top of docs/plan/HANDOFF.md with loading
  order, verified154/154 draft checkpoint, next SC-02 then SC-03/04/SC-01
  consolidated review, RF01/02 proposed vs RF03 accepted and acceptance/no-code
  boundaries. No further design acceptance or implementation progress;
  destination chat not selected or messaged by this documentation handoff.

- 2026-10-03 (D10): Added5 Reports/CSV DTO drafts API-150–154,154/154 total.
  Current-state/time cohorts/whole-set totals/decimal38/distinct/LESSON-obligation/
  FINALIZED no-data and role-safe selectors/groups documented; CSV fresh
  SNAPSHOT/all-results/columns/limits/quoting/formula-warning specified.
  Two JSON and one CSV illustrations added. New wire details remain draft;
  SC-01 parity review/SC-02–04 and actual E04/E05 acceptance still open.
  Accepted sources/ADR-156/owner marks/side work unchanged; static checks only,
  no code/SQL/runtime tests. Next SC-02, not implementation.

- 2026-10-03 (D9): Added10 whole-Class submission/review/Student-own FINALIZED
  DTO drafts,149 total API-001–149/5 remaining. Full-cohort manifests/related
  Registration finalize guards, scoped preview/review/no DRAFT leakage,
  all-or-none statuses/history/publication/unfinished projection, decimal/
  finalized-only result and two JSON examples documented.
  Protected accepted sources/ADR-156/owner marks/earlier side work; RF01/02
  remain proposals/E04–E05 open. Catalog/memory synced; static checks only,
  no code/SQL/runtime tests. Next Reports API-150–154.

- 2026-10-03 (D8): Added9 Attendance/Teacher gradebook/detail/selector/draft-save
  DTO drafts,139 total API-001–139/15 remaining. Full scoped cohort and changed
  delta/absence guards, no HELD/ONGOING-only Attendance prerequisites,
  per-Student score-comment TX/report version touch, full saved-component
  decimal totals/missing-vs-zero and two JSON examples documented.
  Protected accepted sources/ADR-156/owner marks and earlier side work;
  RF01/02 still proposed, E04/E05 open. Catalog/memory synced; static document
  checks only, no code/SQL/runtime tests. Next whole-Class grades140–149.

- 2026-10-03: Recorded explicit RF03 agreement as ADR-156: new roots/whole
  weekly candidates/move destinations start after serverNow at saving,
  live-clock recheck/no backfill/no skip/partial batch. Synced canonical
  requirements and dated NV19/W16 addenda, DTO/catalog/system/handoff.
  RF01/02 and actual E04/E05 remain open;130 schemas/24 remaining unchanged.
  Earlier accepted ADRs/owner marks/E03 physical model preserved; no code/SQL/
  runtime tests. Next Attendance/per-Student draft API-131–139.

- 2026-10-03 (D7): Added17 Schedule/scoped-calendar DTO drafts,130 total
  API-001–130/24 remaining. Concrete whole-week plan, full obligation/cohort/
  resource contexts, same-row move, atomic old/new same-obligation replacement,
  manual HELD/no Attendance prerequisite and three role/chain projections.
  Added RF03 create/weekly/move-target future cutoff as proposal only, without
  changing accepted cutoffs/sources/review marks/ADR. Clarified pagination
  applies only paginated lists, not full sets/plans. Next Attendance131–139.
  Static document checks/examples only; no code/SQL/runtime tests or E acceptance.

- 2026-10-03 (D6): Added23 Class/placement/Teacher-Student DTO drafts,
  113 total API-001–113/41 remaining. Eight commands' related contexts,
  immutable Assignment/full Schedule and completion Attendance coverage,
  explicit transitions/Teacher change, one-TX placement/internal DRAFT,
  three role projections/PII guards/unknown recovery and examples written.
  Earlier report/sequence work and accepted sources/review marks preserved.
  No new accepted ADR/E04/E05; RF01/02 still proposals. Next Schedule114–130.
  Static document checks only; no code/SQL/runtime tests.

- 2026-10-02 (D5): Added17 proposed Course/Band/Program/shared-result DTO entries,
  90 total API-001–090/64 remaining. Whole-config source/target membership and
  relation tokens, exact weights/activation, used/cancelled-Class freeze,
  rename/lifecycle separation, lookup role/action isolation and examples written.
  Accepted sources/review marks/ADR unchanged; E04-RF01/02 still proposals.
  Stage4 doing/E04–E05 unchecked, next Class/placement/scoped reads API-091–113.
  Static document checks only, no code/SQL/runtime tests.

- 2026-10-02 (D4): Added20 proposed Session/Teacher DTO entries,73 total
  API-001–073/81 remaining. Role/task projections, full cancellation/grading
  manifests, separate ABSENT/score, current-original grader guards/provenance,
  safe examples/recovery written. Creation-time/no-op proposals E04-RF01/02
  tracked without changing accepted sources/review marks/ADR or running tests.
  Stage4 doing/E04–E05 unchecked, next Course/program/config API-074–090.

- 2026-10-02 (D3): Added16 proposed DTO entries Admissions/Invoice/Payment,
  53 total API-001–053/101 remaining. Defined three-role projections, explicit
  Invoice/Payment absence, approval/collection/correction guards and immutable
  money/time, fresh own print, caller-action pairs and safe JSON examples.
  Preserved accepted sources/owner marks; no new accepted ADR or runtime tests.
  Stage4 doing/E04–E05 unchecked, next Entry sessions/Teacher API-054–073.

- 2026-10-02 (D2): Added27 proposed DTO entries (12 Admin/audit,1 Home,14
  Student Registration/booking/confirmation),37 total API-001–037/117 remaining.
  Defined role projections/required-nullable/types/versions-full sets/guards/
  safe secret completion and static wire examples; no new accepted ADR.
  Preserved E01–E03/requirements/owner marks; stage4 doing/E04–E05 unchecked.
  Next API-038–053 Admissions/Finance; no app/SQL/provisioning/runtime tests.

- 2026-10-02: Created E04-C1 contract registry/32 F manifest/21 W trace and
  E04-D1's10 typed schema drafts;154 API patterns/55 UUID/6 lookups consolidated.
  New exact routes/DTO/error refinements remain proposed; SC-01–04 explicit.
  Preserved accepted ADR/DB/E01/E02 and owner checkmarks/notes. Stage4 doing,
  E04/E05 unchecked, D12/D13 deferred; no code/SQL/provisioning/tests.

- 2026-10-01: Recorded all23 owner-checked/displayed E04-R1 rows and explicit
  all-approval as ADR-150–155 (six packages). Preserved owner rows/notes and
  accepted E03; restored stale OWNER_REVIEW metadata/E01–E03 checks from actual
  prior acceptance. Consolidated accepted package content in E04-v0.1; full
  artifact still incomplete, E04/E05 unchecked and D12/D13 deferred. No code/tests.

- 2026-10-01: Recorded presented Attendance/per-Student draft4e approval
  as ADR-149. Created owner-requested E04_REMAINING_REVIEW E04-R1 with six
  remaining packages/23 unchecked proposal rows and editable owner notes.
  Updated review/handoff protocol: package marks in the new file, E04/E05
  stage gates stay in OWNER_REVIEW. No code/SQL/tests or new package approval.

- 2026-10-01: Recorded presented E04 Schedule4d approval as ADR-148:
  scoped current/history calendars, concrete individual/weekly previews,
  all-or-none weekly save, pre-start move, atomic linked replacement and
  manual HELD; five UUID writes/related versions/live recheck/caller lookup.
  Prepared Attendance/per-Student draft4e proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Class/placement/scoped reads4c approval
  as ADR-147: explicit lifecycle/Teacher/capacity commands, one-Registration
  placement with atomic Assignment-DRAFT report, scoped reads, eight UUID
  writes/related versions-set/live checks/caller lookup. Prepared Schedule4d
  proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Course/program/configuration4b approval
  as ADR-146: whole-configuration TX, explicit lifecycle/name commands, seed
  Band reads, ten UUID writes/related set/atomic audit-completion/caller lookup.
  Prepared Class/placement/scoped reads4c proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Entry sessions/Teacher grading4a approval
  as ADR-145: task-specific Session/roster/grade commands, nine UUID writes,
  related context/server rechecks/atomic audit-completion/caller result lookup.
  Prepared Course/program/configuration4b proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Admissions/Finance3c approval as ADR-144:
  role-specific API groups, atomic admission-Invoice/Payment-PAID and reasoned
  metadata/cancellation commands; five UUID writes/related versions/atomic audit-
  completion/caller lookup, financial snapshot reads and fresh own-Invoice print.
  Prepared Entry sessions/Teacher grading4a proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Student Admissions3b approval as ADR-143:
  own Registration/test/confirmation groups, previews/related-version sets,
  atomic reschedule and six UUID commands with transactional audit/completion,
  caller-only lookup and deliberate unknown-response handling.
  Prepared Admissions/Finance3c proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Students3a approval as ADR-142:
  six own registration/profile/home/catalogue routes; atomic signup/no auto-login,
  two profile versions/no form-credential replay, scoped reads/unknown handling.
  Prepared Student Admissions3b proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Administration2b approval as ADR-141:
  account commands/search, UUID/version/atomic audit-replay, safe caller-only
  completion lookup and audit reads; no secret replay or new Admin powers.
  Prepared Students3a proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Auth/session2a approval as ADR-140:
  five Auth routes/minimal context, framework CSRF RAM/header exchange,
  identity refetch/403 CSRF_INVALID/no automatic retry or credential replay.
  Prepared Administration2b proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Owner accepted presented E04 package1, recorded ADR-139:
  DTO Base64/expectedVersions, UUID Idempotency-Key and POST-body PII search.
  Prepared Auth/session package2a proposal; detailed Admin2b follows.
  No full E04/E05 acceptance, code/SQL/migration, packages or executed tests.

- 2026-10-01: Owner explicitly accepted the actual consolidated E03-v1.
  Marked DATABASE_SPEC Accepted, checked only E03, stage3 done/stage4 doing.
  Prepared E04-v0.1 opening API-contract proposal; it remains unaccepted.
  No new ADR (artifact acceptance only), code/SQL/migration or runtime tests.

- 2026-10-01: Recorded presented G2 acceptance (ADR-138), consolidated
  DATABASE_SPEC as E03-v1 (Review) with actual dictionary/key/SQL/rubric/trace
  review scope and E04 handoff limits. Checked24 tables/230 fields and21 W paths;
  E03 remains unchecked/stage3 doing pending explicit artifact acceptance.
  No SQL/migration/application code, runtime tests or deployment.

- 2026-10-01: Recorded presented G1 acceptance (ADR-137): 5 triggers,6 views,
  6 UDFs,7 procedures and real application callers, with service-owned rights.
  Prepared G2 SQL permissions/concurrency/snapshot/index/rubric proposal.
  G2 remains unaccepted; E03 unchecked. No SQL/migration/code or executed tests.

- 2026-10-01: Recorded presented F acceptance (ADR-136): financial snapshots,
  atomic collection/audit, safe replay markers and yearly invoice counter.
  Prepared G1 SQL-object review; G2 permissions/concurrency/verification follows.
  Both remain proposals. E03 unchecked; no SQL/migration/code or executed tests.

- 2026-10-01: Recorded presented E acceptance (ADR-135), synchronized finalized-only
  final_score, interim totals and aggregate/history conventions in requirements/E02.
  Prepared F's financial/audit/replay/counter proposal; F remains unaccepted.
  E03 unchecked; no SQL/migration/application code or executed tests.
- 2026-10-01: Recorded presented package D acceptance (ADR-134), synchronized
  DRAFT GradeReport creation with placement/audit in W15 and target memory.
  Prepared E's Attendance/Score/GradeReport proposal; E remains unaccepted.
  E03 stays unchecked; no SQL/migration/application code or executed tests.
- 2026-10-01: Recorded presented package C acceptance (ADR-133), synchronized
  PENDING/first-booking/first-result and attempt-two state in requirements and
  E02 matrix/W05/W07/W08. Prepared D's Class/assignment/Schedule proposal;
  D remains unaccepted, E03 unchecked. No code/SQL/migration or runtime tests.
- 2026-10-01: Recorded presented package B acceptance as ADR-132; account/
  profile/session persistence and aggregate transactions are accepted, hash
  capacity still requires runtime verification. Prepared C's configuration/
  entry-test proposal including explicit PENDING→TESTING refinement; C remains
  unaccepted. E03 unchecked, no SQL/migration/application code or tests.
- 2026-10-01: Recorded package A acceptance in ADR-131; the overall model and
  conventions are accepted, not the unreviewed B–G fields/SQL-object details.
  Prepared package B's grouped Account/Profile/session proposal for review.
  E03 stays unchecked; no SQL, migration or application code was written.
- 2026-10-01: Recorded the owner's choice of approach 1 and drafted E03-v0.1
  DATABASE_SPEC: 24 proposed tables (22 target entities + replay/counter), ERD,
  field/type/NULL/key/check/index/history/concurrency design and rubric mapping
  for 5 triggers/6 views/6 functions/7 procedures/13 transaction cases/four DB
  roles. Linked the draft from E03 without checking it. Review begins at A;
  counts/structure are document checks only, not running SQL or application tests.

- 2026-09-30: Owner provided the DBMS330284 rubric and confirmed applying its
  database technical and application-integration criteria to E03–E04, not its
  separate report/submission format. Recorded ADR-130 and updated the E03 plan.
  E03 remains open; no SQL object, migration, application code or test exists.

- 2026-09-30: Owner explicitly accepted the complete E02-v1 WORKFLOW_SPEC in
  chat. Checked only E02, marked workflow stage 2 done and database stage 3
  doing. E03–E05 and D12/D13 remain open; this is design acceptance, not
  executable verification or implementation authorization. No new ADR/code.

- 2026-09-30: Consolidated owner-confirmed E02 design sections into
  docs/design/WORKFLOW_SPEC.md (E02-v1, review). Document coverage check found
  21 W flows tracing all37 retained NV and32 F codes; state transitions,
  common error/replay/audit rules and future verification cases are included.
  Linked the E02 owner-review row without checking it; stage 2 is review,
  not done. No new product ADR, code, application test or implementation plan.

- 2026-09-30: Owner explicitly accepted E01-v1 in chat. Checked the E01 review
  row, marked stage 1 done and moved stage 2 workflows to doing. The accepted
  artifact is docs/design/FUNCTION_SPEC.md with FUNCTION_CATALOG as source index.
  E02–E05 and D12/D13 remain open; no new ADR, code or application tests.

- 2026-09-29: Owner requested entering E01. Created docs/design/FUNCTION_SPEC.md
  (E01-v1, review):37 retained NV with actor/module, input/output, rights/guards,
  validation/effects/ADR sources; NV10/NV11 excluded, shared account/profile/home
  actions and32 individual F mappings. Structural document check verified39
  source NV,37 retained/7 fields each and32 F excluding F15. Linked catalog,
  E01 review row and active handoff; stage1 review, not done. E01 remains
  unchecked; no new policy/ADR, code, test execution or implementation planning.

- 2026-09-29: Owner explicitly approved recording the whole Gói 3 FE11–FE19,
  with no exceptions. Added ADR-121–ADR-129 and synced requirements 4.8,
  target architecture, catalogue, owner-review metadata and handoff. Updated
  only the nine Gói 3 checks; preserved all prior rows/owner notes and old ADRs.
  68/75 rows accepted; D12/D13 deferred and five E gates open (seven total).
  FE14 save units and FE18 one-time credential handoff are now accepted design,
  not implementation. E01-v1 is now prepared for review (entry above). No code, packages,
  Git/scaffold, deployment, integration, tests or E acceptance.

- 2026-09-29: Owner requested one bundled frontend Gói 3 review, with changes
  volunteered only for exceptions. Added nine unchecked FE11–FE19 proposal
  rows to OWNER_REVIEW with scope/trade-offs, especially attendance-per-lesson
  and draft-grades-per-student saves plus one-time secret handoff. Used
  brainstorming for context/alternatives/risks; owner-requested single review
  replaces incremental clarification. 75 rows, 59 accepted/16 open.
  All 66 prior rows/owner notes preserved; no new product ADR, code or E approval.

- 2026-09-29: Read owner-updated OWNER_REVIEW: accepted D05/D08/D09/D11/
  D14/D16 as ADR-114–ADR-119. Asked D17 operator once; owner accepted full
  release/secrets/backup-restore/maintenance responsibility by default, ADR-120.
  Updated only D17's checkbox from that reply; preserved all other row content
  and owner notes. Synced affected canonical/design/plan memory. 59/66 rows
  accepted; D12/D13 deferred, five E gates and Gói 3 still unaccepted.
  No application code, installations, deployment, backup or test execution.

- 2026-09-29: Owner approved all original frontend Gói 2 FE01–FE10. Preserved
  FE01a/FE01b/FE02 ADR-103–ADR-105; added eight accepted rows FE03–FE10 and
  ADR-106–ADR-113, synced canonical/target/handoff memory. Gói 3 FE11–FE19,
  nine D and E were open at that point (later D approval above). No code,
  new packages, hosting or public access.

- 2026-09-29: Owner clarified on-demand test/edit/teacher-demo use, with
  access from other devices, and requested a Markdown summary. Created
  docs/design/SELF_HOSTED_DEMO.md covering laptop hosting, local/LAN/tunnel
  request flow, operating cycle, security and demo fallback. This is a
  conceptual guide, not a verified setup or D/E approval; no code/network changes.

- 2026-09-29: Owner approved FE02 page structure/navigation, recorded ADR-105
  and synced affected design memory. Current discussion is student-friendly
  hosting, not deployment approval. Remaining D/E and later frontend choices
  stay open; no code or cloud resources created.

- 2026-09-29: Owner approved FE01b role menu groups/labels and fresh-login
  landing; recorded ADR-104 and synced F traceability/affected memory. Next
  FE02 list/detail/form navigation. Other frontend proposals/E stay open; no code.

- 2026-09-29: Owner approved FE01a shared responsive shell, recorded ADR-103
  and synced affected design memory. Next FE01b role menu grouping/labels;
  other Gói 2/Gói 3 choices and E remain unapproved. No application code.

- 2026-09-29: Owner approved frontend Gói 1: D01–D04/D06–D07/D10/D15,
  recorded ADR-095–ADR-102 and synced accepted contracts. D15 uses
  Student-mobile/staff-desktop priorities with all three responsive sizes.
  Owner requests Gói 2 one part at a time, next FE01a shell. Gói 2/Gói 3,
  nine remaining D choices and E01–E05 have not been approved; no code.

- 2026-09-29: Refreshed docs/plan/HANDOFF.md for the owner's next chat to discuss
  frontend: exact A–C/D–E boundary, relevant UI constraints, grouped proposal
  workflow and a ready-to-use opening message. No new design decision or code.
- 2026-09-28: Owner approved all 33 A–C choices in OWNER_REVIEW ([x], no alternatives),
  recorded ADR-083–ADR-094 and synced requirements/catalogue/target data contracts.
  Target now includes EntryTestBooking and AuthSession (22 entities; possible
  technical replay store still needs dictionary mapping). D01–D17/E01–E05 remain
  unapproved. Owner wants frontend detail after this persistence step; no code.
- Older bullets below describe progress at their respective acceptance dates;
  any earlier statement that A–C details remain open is resolved by the batch above.

- Inspected the workspace and the complete local instruction kit.
- Confirmed both copies of `requirements.md` under `instructions/` are
  byte-identical.
- Confirmed `instructions/english-center-agent-kit/docs/plan/` contains only
  empty scaffolding; there is no `plan.md`, question file, or slice plan yet.
- Created repository-level Project Memory linked to the canonical instruction
  files rather than duplicating them.
- Inspected the downloaded `codex-with-chatgpt` skill and its C2C protocol.
- Created the local `.agents/skills/codex-with-chatgpt/` package; discovery is
  confirmed in this session. This task did not start or verify its connection.
- Clarified that `instructions/` is an owner-managed input inbox, not the
  project source tree or Project Memory.
- Clarified that multiple skills and agent integrations may coexist; their
  responsibilities will be defined per workflow rather than forced into one
  global exclusive role model.
- Read the official Word report's full text/tables and distinct images; excluded
  its illustrative forms from requirements. The source Word remains unchanged.
- Created canonical `docs/requirements.md` and `docs/reference/REPORT_BASELINE.md`.
- Recorded per-record retakes before confirmation, per-program weights, /100
  grading, warning-only overdue behavior and removal of scholarships.
- Normalized the 20-entity target model; ADR-007 supersedes the old requirement
  source. Outstanding questions remain explicitly open in Q-004–Q-010.
- Recorded owner-approved uniqueness/repeat-enrollment, student-owned profile
  editing/course lock and reasoned CONFIRMED-only rejection (ADR-015–ADR-017).
  Detailed lifecycle predicates and rejection-data design remain open.
- Recorded accepted reporting scopes/date/count bases (ADR-018). Q-012 now
  contains remaining detail questions, not the already-agreed core definitions.
- Recorded email/password login and student required/editable fields (ADR-019);
  administrative account operations and validation/security parameters remain open.
- Recorded authenticated password change, Admin availability protection and
  account-state meanings (ADR-020); ADR-048–ADR-052 subsequently resolve delivery,
  revocation, reactivation, login-email immutability and timed throttling.
- Recorded accepted test rebooking/absence/pending-retake rules (ADR-021).
  Logical attempts must be distinguished from bookings; target history schema,
  absence actor and center cancellation details remain unfinalized.
- Recorded enrollment cancellation rights/unpaid-invoice coupling and finished-
  record eligibility (ADR-022). Completion requires Class COMPLETED and the
  student's GradeReport FINALIZED, not passing; CANCELLED/REJECTED also permit
  new enrollment while keeping history. Database encoding remains open.
- Recorded OPEN-only placement, ADMITTED retained/derived placement display,
  and pre-teaching teacher changes with full schedule checks (ADR-023). Class
  lifecycle prerequisites subsequently follow ADR-025 and transfer scope is
  excluded by ADR-040.
- Recorded COMPLETED-only grade submission/finalization, DRAFT/RETURNED edit
  rights, SUBMITTED/FINALIZED locks and finalized >= 50/100 passing (ADR-024).
  Rounding was subsequently resolved by ADR-028 and whole-class operation
  scope by ADR-034; pass-rate cohort subsequently resolved by ADR-067.
- Recorded explicit AcademicAffairs class opening/start/completion prerequisites
  (ADR-025). No automatic date-based transitions or finalized-grade prerequisite;
  session occurrence representation remains open; full-schedule rules were
  subsequently resolved by ADR-032.
- Recorded empty PLANNED/OPEN-only class cancellation by AcademicAffairs with
  reason/history preservation and removal of cancelled schedules from resource
  reservations/upcoming teaching/completed-session counts (ADR-026).
- Recorded assigned-grader post-session absence, pre-start reasoned center
  cancellation with historical bookings/same-attempt self-rebooking, and
  date/time/room edit cutoffs (ADR-027). Source provenance distinguishes report
  CANCELLED-state rules from owner-approved operational additions.
- Recorded first-use program/Band configuration freezes and two-decimal input/
  half-up final-total rounding before storage/display/passing (ADR-028).
  Entry-score correction and tied-best attribution were subsequently resolved
  by ADR-029.
- Recorded audited original/current-grader-only entry-score corrections in
  TESTING/BAND_ASSIGNED, locks after confirmation or cancellation/rejection,
  no privileged override and ties favoring attempt one (ADR-029).
- Prepared `docs/plan/HANDOFF.md` for the next chat to resume full design
  agreement without assuming code or whole-baseline acceptance.
- Recorded session creation as CLOSED, guarded pre-start opening/reopening,
  CLOSED student-booking locks, effective closure at start and audited pre-start/
  pre-result grader replacement (ADR-030). Grading/absence/correction remain
  available under ADR-027/029; pending-retake confirmation guards remain intact.
- Recorded atomic Registration/UNPAID Invoice/future-booking cancellation,
  including pre-start CLOSED, preserved started test/scores/Band history and
  stopped grading/correction/absence on cancelled records (ADR-031). Repeat
  enrollment starts testing anew; Q-007 retains data/audit representation only.
- Recorded program-level positive LESSON counts frozen after first Class use,
  full LESSON timetable required before teaching start (not opening), and
  pre-start/no-dependent-data date/time/room edit cutoffs (ADR-032). Attendance
  entry/correction window was subsequently resolved by ADR-033;
  actual-session occurrence representation remains open.
- Recorded attendance entry/correction cutoff before Class COMPLETED, with
  no fixed 24/48-hour per-lesson deadline; COMPLETED attendance is read-only
  even before grade submission/finalization (ADR-033). Assignment rights and
  complete-attendance prerequisite for class completion remain unchanged.
  Owner also accepted opening attendance at LESSON start, not before or only
  after lesson end; the whole attendance window is resolved under ADR-033.
- Recorded mandatory whole-class grade submission/resubmission, reasoned
  return and finalization/publication, with all-or-none transitions (ADR-034).
  All students require complete component scores before sending; individual
  draft/correction entry and correction/resubmission remain allowed. Existing
  edit locks remain; data representation is deferred, no code was implemented.
- Recorded non-held class-wide lesson scope within NV19/F21, excluding a
  separate holiday module/center-wide calendar, automatic holiday skipping/
  makeup scheduling and individual makeup lessons (ADR-035). Non-held sessions
  are not delivered lessons or a reason to mark all students absent. Concrete
  postponement/replacement rules were subsequently resolved by ADR-036.
- Recorded pre-start moves under ADR-032 and post-start non-held LESSON handling
  by AcademicAffairs with reason, preserved history and linked replacement:
  no old dependent data/non-ended class, current teacher/conflict checks and
  consistent all-or-none writes. One original/replacement LESSON obligation;
  completion checks replacement occurrence/attendance, not fake old attendance
  (ADR-036). Data representation remains open; extra non-replacement LESSONs
  were subsequently excluded by ADR-037. No code/schema was added.
- Recorded exclusion of extra LESSON teaching obligations beyond the fixed
  program count (ADR-037). Weekly/individual additions fill the required count;
  linked replacements preserve one obligation, retained history may exceed the
  raw row count, and FINAL_EXAM remains separate. No code/schema was added.

- Accepted Course immutability/current-profile historical display/no transfers/
  Student-owned Invoice browser printing/Accountant metadata-only Payment
  correction (ADR-038–ADR-042), preserving financial snapshots and cancellation
  limits. No application implementation started.
- Accepted positive tuition/no free-course path, VND whole dong, Invoice due date
  local issue date +7 calendar days, confirmation time only/no separate actual
  receipt field, and UTC+7 business dates/schedules/report filters (ADR-043–ADR-047).
  Synced affected specification/catalog/questions/handoff; no code/schema added.

- Accepted temporary-password delivery/forced change, all-session invalidation
  on lock/disable/reset/password change, same-account DISABLED reactivation,
  immutable login email and timed failed-login throttling (ADR-048–ADR-052).
  Synced affected design memory. No source code/schema or whole-baseline approval.

- Accepted passphrase-friendly 15–128-character passwords, 24-hour temporary
  credentials, two-hour absolute/thirty-minute idle normal sessions, concurrent
  devices and server-side current-session logout/no logout-all (ADR-053–ADR-057).
  Synced affected specification/handoff; no implementation or stage completion.

- Accepted business/security audit coverage, structured Admin filters/pagination,
  relevant secret-free event details and no audit CSV/PDF export (ADR-058–ADR-062).
  Synced affected memory. Prepared five Q-012 metric proposals, not accepted rules;
  implementation and the full-baseline acceptance remain pending.

- Accepted current-state/no historical as-of reporting, issuance-period billing/
  current debt separation, distinct students, LESSON-only progress and FINALIZED
  pass-rate cohorts (ADR-063–ADR-067). Report-filter proposals were reviewed next;
  no code/schema/stage completion.

- Accepted report defaults/filters, Director aggregate-only detail and Accountant
  all-filtered-results/report-columns CSV (ADR-068–ADR-072). Synced affected
  memory. Profile validation was reviewed next; no code, final schema or
  stage/full-baseline completion.

- Accepted name/DOB/email/contact-phone/address validation (ADR-073–ADR-077),
  with no new verification, age restrictions or staff editing/required fields.
  Updated affected design memory; remaining catalogue/reference/reason fields
  and operational viewing still need review. No code or baseline completion.

- Accepted catalogue names, immutable manually entered Course/Class codes,
  consistent room codes, plain-text reasons/optional comments and bounded Payment
  references without global uniqueness (ADR-078–ADR-082). Synced affected memory;
  Q-011 operational viewing/UI/history proposals remain unaccepted. No code,
  schema or stage/full-baseline acceptance.

## Relevant files

- `docs/design/FUNCTION_CATALOG.md` — 39 source references, NV10/NV11 removed;
  catalog and FUNCTION_SPEC are accepted E01-v1 design, not implementation evidence.
- `docs/design/DATABASE_SPEC.md` — E03-v1 accepted, not deployed schema.
- `docs/design/SYSTEM_ARCHITECTURE_SPEC.md` — E04-v1 Accepted ADR-160; current
  final refinements at `E04_FINAL_CONTRACT.md`, conditional whole-baseline
  review at `E05_BASELINE_REVIEW.md`. Earlier package/checkpoint references:
  presented package1 through Attendance/per-Student draft4e accepted ADR-139–149;
  remaining23 displayed rows accepted ADR-150–155 in
  docs/plan/E04_REMAINING_REVIEW.md; actual E04-v1 now Accepted160, E05/full baseline Accepted161.
- `docs/design/SELF_HOSTED_DEMO.md` — owner-requested on-demand laptop/tunnel
  concept and operating guide; no deployment or provider installation authorized.
- `AGENTS.md`
- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/CURRENT_STATE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `docs/plan/plan.md`
- `docs/plan/design-baseline.md`
- `docs/plan/questions.md`
- `docs/plan/E04_REMAINING_REVIEW.md` — owner-requested E04-R1:6 packages,
  23 unchecked proposal rows, no duplicated E04/E05 acceptance boxes.
- `docs/plan/OWNER_REVIEW.md` — 75 rows: 33 A–C + 15 D + 20 FE + E01/E02/E03 accepted (71);
  D12/D13 deferred and E04/E05 open (4).
- `docs/plan/HANDOFF.md` — next-chat resume guide; canonical memory takes precedence.
- `instructions/english-center-agent-kit/`
- `docs/requirements.md`
- `docs/reference/REPORT_BASELINE.md`
- `.agents/skills/codex-with-chatgpt/SKILL.md`

## Known issues and blockers

- Git read-only inspection resolves the existing repository root to
  `C:\Study\SOEN\main_workplace`; this task did not initialize Git or modify
  repository settings. Existing documentation changes are preserved.
- No backend, frontend, database schema/migrations, tests, or runtime config
  exist in the workspace, so implementation status cannot be verified.
- The four-phase breakdown mentioned by the instruction kit is not defined.
- The requested skill name was `chatgpt-with-codex`, while the installed skill
  is named `codex-with-chatgpt`.
- The local skill is present in this session's active catalog. Its runtime
  checkout remains external; the global `c2c` command was not linked at the
  last check, but its documented Node entry point is available.
- The C2C checkout's Git status could not be read in the sandbox because Git
  considers its ownership unsafe. No global Git configuration was changed.
- Report screenshots are UI previews; they do not prove implemented APIs,
  database integration or successful tests.
- A–C resolve the known Q-004–Q-010 choices. E01/E02/E03 outputs are accepted;
  architecture/API and full-baseline outputs are now Accepted160/161; no runtime implementation evidence.
- Function detail gaps Q-011 and report definitions Q-012 were identified by
  rechecking report sections I–III/V. Walk-in registration, CSV and schedule
  entry are resolved ADR-012–ADR-014; operational detail is now accepted ADR-083–094,
  while D12/D13 are deferred and E04/E05 are now Accepted160/161.
- Windows sandbox currently fails setup refresh; documentation changes used
  apply_patch through elevated execution.

## Next action

Complete design baseline is Accepted. Await separate owner instruction for
next work;do not autonomously create implementation plan/code/SQL/Git/scaffold/
deployment/packages or operational configuration. D12/D13 remain deferred and
DF01–05 prerequisite/evidence records remain;revisit them only in applicable
owner-authorized scope. Preserve Accepted ADRs001–161/role/RF/FE/operator rules.
