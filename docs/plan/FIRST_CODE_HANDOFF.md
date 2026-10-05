# Handoff thực thi đầu tiên — P01 → P02

Historical initial scope below. Latest2026-10-05: D06 applied/reverified PASS,
main543fcf0 full474tests PASS; HANDOFF.md/D06_TASK_CARD.md/ADR163/164. D01–D05/
prior stages PASS. Complete24table/230column EF mapping; next proposed D07 not
started. Migration/isolated SQL execution require explicit scope; no automatic
SQL/migrations/rest M0/install/push/deploy permission from this checkpoint.

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
