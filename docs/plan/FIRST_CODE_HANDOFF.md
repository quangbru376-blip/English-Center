# Handoff thực thi đầu tiên — P01 → P02

## Current R02/R03 preparation — 2026-10-06

Owner requested continuation. Task cards R02_TASK_CARD.md/R03_TASK_CARD.md and
bounded R02_WINDOW_WORKER.md/R03_GUARD_WORKER.md prepared. New external payload
and rollback-only ReplayOperation/Admin fixtures await consent. No worker/SQL/
implementation run yet. Accepted product decisions unchanged; ADR163 main apply
only after authorized implementation passes independent review.

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

Cập nhật04/10/2026. Repository: `C:\Study\SOEN\main_workplace`.
Owner yêu cầu handoff để chat kế tiếp hiểu ngữ cảnh và viết giai đoạn code đầu
tiên. Quyền thực thi này tách khỏi nghiệm thu thiết kế: **hoàn thiện P01 rồi
viết/review P02 backend/test skeleton**, chia nhỏ khi cần. Chưa tự mở P03,
D01–D13, nghiệp vụ hoặc toàn M0. Lượt chuẩn bị handoff chưa sinh code.

## 1. Đọc và authority

1. AGENTS.md, rồi PROJECT_CONTEXT → ARCHITECTURE → CURRENT_STATE → DECISIONS
   → TODO đúng thứ tự; tiếp requirements (canonical product specification).
2. HANDOFF, tài liệu này, implementation-plan, M0_QUALITY_PLAN (nhất là4/8/11),
   SKILL_ROUTING, SQL_INSTANCE_ASSESSMENT, antigravity-worker-readiness.
3. Read accepted E01 FUNCTION_SPEC/E02 WORKFLOW_SPEC/E03 DATABASE_SPEC; E04
   SYSTEM_ARCHITECTURE_SPEC/final contract/runtime profile. Catalog/DTO/replay/
   SQL/parity/E05 cross-check là source theo phạm vi task; tải phần liên quan
   trước sửa, không dùng số lượng thay bằng chứng hoặc mở link worker tự do.
4. Read complete selected SKILL.md. Instructions inbox chỉ authoritative khi
   được memory/owner adopt. Không thay Word/PDF nguồn hoặc dùng forms làm rules.

Context dưới đây giúp định hướng; không thay việc đọc canonical sources.

## 2. Những gì đã chốt

- E01–E05 Accepted; actual E04-v1 ADR-160, E05/full baseline ADR-161. Không
  restart E04-D10/SC01–04 hoặc hỏi nghiệm thu lại. RF03=ADR156, RF01/02/04=
  ADR157–159; tất cả đã Accepted. OWNER_REVIEW75rows/73accepted, notes giữ
  nguyên; chỉ owner D12 performance/D13 hosting còn hoãn. M0.D12/D13 là
  task IDs khác, không được nhầm với các quyết định owner này.
- Exact design:37NV/32F/21W;154API/154domainDTO;55UUIDcommands/6caller-result
  paths;24tables230fields/5triggers6views6UDF7SP/13TX/4SQLroles.
- Backend .NET10/EF10/SQLServer, React/Vite/TypeScript/Tailwind, xUnit. Modules
  Students/Admissions/Academics/Finance/Reports/Administration; conventional
  Controller → Service → Repository → AppDbContext, không generic framework.
- Seven single roles; Student tự đăng ký, không Admissions tạo hộ. Email login;
  JWT HttpOnly/Secure cookies, same-origin/CSRF/Origin, server sessions/revocation;
  no browser token persistence/automatic writes replay. Exact full-set DTO/save
  units, secret-free atomic mandatory audit/C1 completion, current rights giữ nguyên.
- No scholarships, no10point grades, no post-confirmation retakes, no automatic
  debt penalties. VND exact strings/decimal18 hoặc38 theo contract; UTC events
  và local VN date/time giữ riêng. SQL tests thật, không SQLite substitutes.
- Thứ tự mặc định39families tại M0 section11 là nguồn duy nhất cho order/diagram:
  P01 P02 P03 P05 P04 P06 P07 P08; D01–D13; A01 A02 A04 A03 A05 A06 A07 A08
  R01 R02 R03; U02 U01 U03 U04 U05 U06 V01. Đây là families, không39prompts
  cố định. Chia theo chất lượng/token/phạm vi, không theo tuần/lịch.

## 3. SQL và môi trường — đừng cài lại

ADR-162 thay riêng SQL choice ADR-119: giữ SQL2025 Express64-bit tại
`.\SQLEXPRESS`, observed17.0.1000.7 RTM. .NET SDK10.0.200, Node24.16.0,
npm11.13.0 đã kiểm; next chat xác minh lại những giá trị cần dùng.
Windows auth và SELECT/type/decimal/JSON/SHA/system-DB SNAPSHOT read đã chạy;
chưa app schema/EF/grants/race/load/backuprestore. Target DB/EF compatibility160
giữ mapping E03; chưa ALTER bất kỳ DB nào. Không suy system DB170 là app config.

Express50GB/database,1410MB buffer pool, lesser1socket/4cores. SQL Agent/native
backup encryption không có: giữ ADR118 bằng external scheduling/encrypted
archive với protected staging/key separation/checked copy/decrypt/restore;
tool/media/key/identity vẫn là prerequisites. Không cần làm backup trong P02.
Build RTM chưa có updates mới: review same17.x patch trước real data/exposure,
không cài Developer2022 hoặc update engine trong scope này. Có17DB khác sẵn,
không chạm chúng. SQL TLS diagnostic -C không thành app certificate policy.

## 4. Bắt đầu từ trạng thái Git thật

HEAD khi chuẩn bị handoff: ad9fc72, main ahead8 so với origin/main; trước
assessment clean. SQL assessment và handoff hiện là **pending documentation
changes**, không được bỏ vì tưởng là file thừa. Recheck git status/diff/log;
review từng file, preserve documentation baseline/skill/sidework. Owner đã yêu
cầu chuẩn bị repo sạch: manager có thể snapshot riêng các tài liệu đã review
bằng local documentation commit trước edit runner; không push và không amend
commit khác. Không dùng git clean/reset/discard hoặc stash để che pending work.
Nếu phát hiện thay đổi mới của user thì giữ và phân biệt phạm vi, không gom
vào commit của mình. Base commit trong contract phải là snapshot actual sau
chuẩn bị, không hardcode ad9fc72 hoặc giả dirty docs đã vào detached worktree.

## 5. P01 — điều kiện cần cho code đầu

Manager-owned; chỉ inventory và tài liệu/manifest nonsecret. Tạo
`docs/plan/M0_LOCAL_PROFILE.md` và task card nếu cần: SDK/package compatibility,
package sources/license/version pins cho đúng accepted stack, solution format
(sln/slnx), test host, actual commands, selected skill source hashes, finite
profile slots/isolation và những prerequisite còn thiếu. Không cài runtime/SQL,
không tạo DB, DDL/DML, bootstrap Admin hoặc secret trong tài liệu.

P02 phụ thuộc **P01 phần SDK/package/test-host/scope**, không đợi D12/D13,
backup destination, toàn profile chạy business hoặc blocklist của P08. Các
slots còn thiếu vẫn được ghi và chặn đúng runtime use; không giả ready, không
tắt security để chạy skeleton. Chọn compatible patches trong accepted policy
không hỏi lại từng patch; nếu cần framework/package mới ngoài stack thì gom
quyết định thật sự cần kèm exact manifest/đề xuất trước restore. Không tiếp tục
planning vô hạn khi các điều kiện đủ cho P02 đã được giải quyết.

## 6. P02 — phạm vi code đầu tiên

Đầu ra: backend/test skeleton .NET10 có thể build và discover/chạy test mục
tiêu thật, đúng layout; **chưa business endpoints/entities/migrations/auth/UI**.
Readiness không được báo success khi profile/schema chưa có; nếu chưa triển
khai health surface thì không bịa ready endpoint. Không cần triển khai toàn
runtime profile trong skeleton hoặc test mirror implementation.

Materialize exact paths sau P01: solution, Api.csproj, Program.cs, test.csproj
và startup test infrastructure theo M0 section8; `.gitignore` nếu cần và được
contract cho phép. `.sln` trong bản đề xuất không là kết luận format với SDK10.
Chỉ package manifest đã review; code/lockfile changes đúng bootstrap scope.
Không folder-wide allowed_paths, không appsettings secrets, business routes,
SQL migrations, packages ngoài manifest, source-report hoặc baseline ADR edits.

Task card chứa dependency evidence, exact files/read scope/forbidden paths,
English objective/acceptance criteria, actual base commit, selected skills/
overrides, line/file caps phù hợp scope và commands Codex sẽ chạy. Nếu cần
chia skeleton/test infrastructure thành hai task nhỏ thì ghi phụ thuộc rõ.

## Execution checkpoint — 2026-10-04

Handoff này đã được thực hiện cho đúng P01 prerequisites → P02. Xem
[P02 task card](P02_TASK_CARD.md) và [P01 manifest](M0_LOCAL_PROFILE.md).
AGY SUCCESS/exit0/scope pass; Codex sửa hai điểm trong paths hiện có, kiểm
build sạch và discovery10/tests10pass, audit locks/licenses/hashes/NuGet.
Final PASS tại detached d9eb194; owner sau đó xác nhận apply vào primary,
main df7cd9a reverified locked restore/build sạch/discovery10/tests10pass.
Không mở P03/SQL/M0; original detached tree được giữ làm evidence.
Các bước workflow bên dưới được giữ làm protocol; mốc edit chưa chạy là trạng
thái lúc handoff được soạn, hiện bounded P02 edit đã kiểm chứng.

## 7. Worker protocol — Codex manager, AGY worker

Dùng skill `C:/Users/asus/.codex/skills/antigravity-worker/SKILL.md`; đọc cả
references/contract.md khi tạo contract. Backend implementer dùng complete
`.agents/skills/backend-clean-code/SKILL.md` cùng SKILL_ROUTING overrides;
Codex dùng `.agents/skills/code-review/SKILL.md`. frontend-ui chỉ khi có frontend
scope sau này. C2C inactive; không tự mở integration hoặc parallel agents.

1. Inspect repository/status, complete reviewable English contract và validate
   bằng skill runner **trước** dispatch. Consent scope phải đủ context/skill files:
   smoke consent8files không blanket consent. Nếu cần quyền chia sẻ thêm, hỏi
   một lần với danh sách file cụ thể sau contract đã hoàn thiện; không đưa secrets.
2. Edit mode: clean repo + new **detached Git worktree**; primary không được
   AGY sửa. Actual edit chưa chạy, analyze smoke mới được kiểm chứng.
3. Chạy AGY ngoài sandbox có kiểm soát để dùng phiên đăng nhập thật trong
   `C:\Users\asus\.gemini` và mạng, qua sandbox escalation với justification
   đúng scope. Không đọc/copy/log credentials; không đặt chúng vào contract.
   **Không --dangerously-skip-permissions** hoặc bypass nếu tool từ chối.
4. Worker không chạy validation commands. Manager theo dõi read/tool events;
   links không cho quyền đọc thêm. Smoke đầu rejected vì extra read; receipt
   alone không chứng minh read policy. Reject nonzero/status failure/violations.
5. Sau completion kiểm **AGY_WORKER_RECEIPT**, git diff/status, allowed_paths,
   forbidden_paths và actual file/line caps; review dependencies/secret handling.
6. Codex tự restore/build/test theo manifest approved và target actual. Backend
   compiler/analyzer/nullability checks và tests dùng P02; frontend lint/typecheck
   N/A có lý do, không claim chạy targets chưa tồn tại. Không gọi SQL tests pass
   khi chưa có schema. Worker claim không là evidence.
7. Dừng với result/diff/test evidence trong worktree để owner duyệt. **Không
   merge/cherry-pick/copy/apply worker changes vào main khi chưa owner cho phép**,
   kể cả P02 đã được cho viết. Apply permission và coding permission khác nhau.

## 8. Kết thúc giai đoạn đầu

Bàn giao task card/P01 manifest, worker receipt/worktree paths, reviewed diff,
commands và kết quả thật, review verdict và residual prerequisites. Update
CURRENT_STATE/TODO/active plan; ARCHITECTURE chỉ khi implementation đã được
verified (nêu rõ nếu chỉ worktree, primary chưa có source). Không mark M0/V01
done từ skeleton. Pending apply vẫn là pending, không fake integrated baseline.

P02 đã reviewed và apply theo owner xác nhận; chờ direction cho task tiếp. Chưa tự dispatch
P03 hoặc cả M0. Chỉ hỏi quyết định mới cần thiết; không mở lại approved design/
SQL/operator, không yêu cầu deadline hoặc tuần làm việc.
