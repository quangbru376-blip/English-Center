# M0 — kế hoạch task theo chất lượng

Cập nhật: 04/10/2026. Status: doing (lập kế hoạch).

Nguyên tắc chia theo chất lượng đã được owner yêu cầu. Danh sách task/phạm vi
bên dưới là bản thảo cụ thể để rà; chưa cấp quyền sinh code, scaffold, cài đặt,
chạy migration hoặc dispatch worker. M0–M4 là nhóm phạm vi, không lịch làm việc.

## 1. Đầu ra M0 và nguồn hợp đồng

M0 cung cấp nền database/runtime đúng baseline và một luồng tài khoản thật:
Student tự đăng ký, login, me, logout; đổi mật khẩu thường/hạn chế; một command
Admin lock và caller-result lookup để kiểm chứng audit/replay/revocation. Các API
M0 thực hiện:001–005,018;007/010/014 cho pilot Admin. Những command Admin khác,
profile edit/home/catalogue và nhật ký UI đầy đủ tiếp tục ở các slice sau; không
được hiển thị như chức năng đã làm. Bootstrap/test fixtures không là API tạo hộ.

Nguồn: [requirements](../requirements.md), [E03](../design/DATABASE_SPEC.md),
[catalog API/F/W](../design/E04_CONTRACT_CATALOG.md),
[DTO](../design/E04_DTO_SPEC.md), [C1/replay](../design/E04_REPLAY_SPEC.md),
[SQL/locks/grants](../design/E04_SQL_INTEGRATION_SPEC.md),
[runtime/readiness](../design/E04_RUNTIME_PROFILE.md),
[final contract](../design/E04_FINAL_CONTRACT.md), ADR119/140/141/142/160.
W01/W02/W03 là trace chính; API007 chỉ Admin NORMAL, lock010 TX13 không đổi
quyền hoặc thêm API pilot. Lookup014 đúng action đã triển khai; các action chưa
triển khai không được trả success giả. Canonical registry55actions vẫn theo E04.

## 2. Điều đã kiểm tra và prerequisite

- Git clean tại e6f402a trước khi lập bản này; chưa ứng dụng/scaffold.
- .NET SDK10.0.200, Node24.16.0, npm11.13.0 hiện có; chưa kiểm tương thích package.
- Local SQL `.\SQLEXPRESS`: ProductVersion17.0.1000.7, Express Edition64-bit,
  đọc bằng Windows authentication ngày04/10/2026; không đổi DB/config.
- Express instance này không chứng minh target SQL2022 Developer ADR119 đã có.
  Lập profile Developer2022 riêng theo baseline, không tự thay edition/version
  hoặc dùng Express thay bằng chứng. Chưa cài đặt hoặc chọn tên instance mới.
- LOCAL_DEV cần concrete origin/HTTPS/secret refs/finite budgets và test isolation.
  Mọi thiếu sót vẫn fail closed; không âm thầm tắt Secure/CSRF/readiness để demo.
  Không hỏi lại D12/D13; prerequisite local execution sẽ được xử lý riêng khi
  owner cho phép phạm vi tương ứng. Không từ planning mở LAN/Internet.
- Analyze AGY đã kiểm chứng; edit chưa chạy. Contract quyền đọc phải rõ, vì lần
  smoke đầu đọc extra file đã bị loại. Codex giữ bootstrap/secrets/config đặc quyền.

## 3. Cách chia và chọn task kế tiếp

Mỗi task có một đầu ra kiểm được. Tasks bảng dưới là families; chỗ có nhiều
object được bung thành một task/object trước dispatch. Không giao cả M0/nhóm
schema hoặc auth module trong một prompt. Luôn dự trù code, tests và giải thích;
không coi token limit là lý do bỏ failure path. File/diff caps sẽ được chọn từ
phạm vi thực tế trước validate, không dùng một con số cho mọi loại task.

Task chỉ `done` khi kiểm chứng của task đạt. Slice tích hợp chỉ `done` sau
build + real SQL/API/browser evidence cho toàn luồng và toàn hợp đồng liên quan.
Không task phụ thuộc nào dùng baseline chưa review. Thay đổi worker nằm trong
worktree đến khi owner cho phép áp dụng; Codex không tự merge vào main.

## 4. Nền project và biểu diễn

| ID | Đầu ra của một task | Phụ thuộc | Kiểm chứng của Codex |
| --- | --- | --- | --- |
| P01 | Hồ sơ công cụ/package compatibility và LOCAL_DEV prerequisite được trình rõ, không chứa secret | Không | Phiên bản thật; package provenance/pins trước restore; SQL edition/profile thiếu được ghi rõ. |
| P02 | Backend/test skeleton theo layout đã chốt, không business routes | P01 phần SDK/package | Build và discovery test target; không health-ready giả khi profile/DB thiếu. |
| P03 | Frontend skeleton, scripts build/lint/typecheck rõ, chưa màn hình nghiệp vụ | P01 phần Node/package | Các scripts chạy thật; không thêm thư viện chưa duyệt hoặc API mocks thành success. |
| P04 | Bộ chuẩn hóa nonsecret text/email/phone/DOB | P02 | Unicode/code point/NFC/surrogate, blank/NULL, duplicate-equivalence và ngày Việt Nam. |
| P05 | Clock/time abstractions và conversion | P02 | UTC milliseconds, ngày Việt Nam, cutoff equality bằng clock kiểm soát; không tùy timezone máy. |
| P06 | Parser/version và numeric representations | P02 | UUID strict/8-byte padded Base64 RV, tiền chuỗi exact, precision/range; tách RV và numeric thành task riêng nếu lớn. |
| P07 | Error envelope và binding/error precedence | P02,P04–06 | Accepted status/code/fieldErrors/traceId; malformed input; không stack/SQL/PII leak. |
| P08 | Credential hashing/blocklist/password validation | P01,P02 | Nguồn offline/license/hash được chốt trước dùng; giữ password nguyên; generator/hash/secret handling thực tế. |

Các filename/class chưa tồn tại; names tại dispatch phải theo layout hiện có
sau skeleton. Các abstraction phục vụ accepted flow, không tạo generic framework.

## 5. Database foundation — đầy đủ cấu trúc, chưa đủ caller evidence

Đề xuất xây toàn bộ cấu trúc E03 trước readiness tích hợp M0, nhưng sinh từng
nhóm nhỏ rồi rà. Không giao24tables/230fields trong một lần. Các nhóm bên dưới
là ownership mapping, không tự quyết migration order; manifests liệt kê toàn FK,
composite relationships, filtered unique, checks/defaults/RV và index candidates.

| ID family | Phạm vi cấu trúc | Phụ thuộc và kiểm chứng |
| --- | --- | --- |
| D01 | T01–T04 Role/Account/StudentProfile/AuthSession | P02; exact E03 mapping và Account/session constraints. |
| D02 | T05–T08 Course/Band/TrainingProgram/ScoreComponentConfig | D01; kinds, numeric types, uniqueness/config relations. |
| D03 | T09–T13 Registration/EntrySession/Attempt/Booking/BandAssignment | D01,D02; split into Registration/session, Attempt/Booking, BandAssignment tasks; composite/cycle relationships kept. |
| D04 | T14–T16 Class/Assignment/Schedule | D01–D03; obligation/replacement/current schedule relations; split if diff too large. |
| D05 | T17–T19 Attendance/Score/GradeReport | D02,D04; missing versus0, required schedule linkage, finalized-only stored score. |
| D06 | T20–T24 Invoice/Payment/AuditLog/ReplayOperation/InvoiceNumberCounter | D01,D03; split financial/counter and audit/replay tasks; exact digest/retention/secret-free constraints. |
| D07 | Model assembly and reviewed migration chain | D01–D06; FK creation order and indexes reviewed; apply to isolated fresh SQL2022 Developer only when authorized; inspect schema, no runtime auto-migration. |
| D08 | Five accepted triggers, each one bounded task | D07; multi-row semantics and negative tests; exact E03 purpose, no audit trigger. |
| D09 | Six accepted UDFs, each one bounded task | D07; pure/read semantics, precision/missing behavior, exact signature. |
| D10 | Six accepted views, each one bounded task | D07 and referenced objects; correct projection/aggregation; no added web rights. |
| D11 | Seven accepted SPs, each one bounded task | D07–D10 as required; exact signatures/result sets/transaction ownership and SQL case tests. |
| D12 | Four SQL roles/grants and schema/maintenance separation | D07–D11; actual allowed and forbidden principal operations; not dbo runtime. |
| D13 | SQL compatibility/readiness and EF trigger/OUTPUT integration | D07–D12; full expected signature/schema checks and EF writes to5affected tables; isolated failures/missing objects fail closed. |

D01–D06 model fragments may build with technical test hosts; they are not a ready
application. No partial schema readiness exemption is introduced. Keep migration
history readable; do not edit applied migrations to repair later fragments.
D08–D11 creation/SQL unit evidence does not prove application callers implemented:
caller-specific E03/E04 trace and transaction tests remain with M1–M4 functions.
If full readiness requires objects whose implementation depends on a later service,
finish their structural/SQL contracts first or leave readiness blocked; never use
count-only/stub objects to pass. Full24table/5TG/6VW/6FN/7SP target retained.

## 6. Identity backend, audit và replay

| ID | Đầu ra của task | Phụ thuộc | Các ca quan trọng |
| --- | --- | --- | --- |
| A01 | Audit writer: mandatory atomic and security fallback paths | D06,P05,P07 | Mandatory failure rolls back; security failure không phục hồi quyền/phiên; secret-free allowlist. |
| A02 | Bootstrap7roles/Admin và test identities tách biệt | D07,P08 | Không password seed trong source/migration/log; runtime không quyền bootstrap/migrate; repeat không tạo trùng/ghi đè. |
| A03 | API018 Student signup service/API | D07,P04,P05,P07,P08,A01 | TX01 Account+Profile atomic, normalized email/phone race, invalid DOB/password, no auto-login/UUID/retry. |
| A04 | API001 CSRF and safe HTTP/cookie boundary | P01,P07 | Same-origin/Secure flags/Origin, anonymous và authenticated context, no-store, token RAM. |
| A05 | API002 login/throttle and session issuance | A02,A04,D01,P08,A01 | Generic failure privacy;5failures/15min; status vs throttle; restricted/normal scope; concurrent login counters. |
| A06 | API003 me and reusable current-session authorization | A05,P05 | Account/version/revocation/absolute2h/idle30min checks; me không gia hạn idle; deny restricted business access. |
| A07 | API005 current-session logout | A06,A01 | Server revoke, other devices remain; failure/log handling; no credential replay. |
| A08 | API004 password change and restricted scope lifecycle | A06,P08,A01 | Current password, RV conflict, temp24h/restricted10min bounds, atomic all-session invalidation. |
| R01 | Canonical registry/C1 digest primitives | P04–P06 |55actions exact, full intent/ordered versus sets, accepted literal UTF8 hashes and negative equality; no secrets. |
| R02 | Completion storage/TX protocol and scoped lookup primitives | D06,A01,A06,R01 | Caller/action/key uniqueness;24h expiry; retained expired409/lookup404; current rights/order; no response/secret snapshots. |
| R03 | API007 Admin detail, API010 lock and API014 lock-result lookup | A06,A01,R02,D12 | Split detail and atomic lock/lookup integration if needed; self/last-active concurrency, current rights, revoke races, lost-response/different-intent/stale409. |

Backend integration tests require actual SQL2022 Developer for locks/RV/audit/
replay/constraints. Tests that use a pure codec/clock do not require SQL; SQLite
cannot substitute for SQL Server-specific evidence. Do not implement other Admin
commands or return fake successful effects. Partial routes stay unexposed until
required profile/database/authorization is in place.

## 7. Frontend và nghiệm thu slice M0

| ID | Đầu ra của task | Phụ thuộc | Kiểm chứng |
| --- | --- | --- | --- |
| U01 | Shared fetch/error/auth state and session-scope boundary | P03,A04–A07 | No token/PII in URL/storage, stale response discard, RAM cleanup, no automatic credential/write replay. |
| U02 | Common form/control and shell primitives needed for identity | P03 | Accepted FE tokens/system font/labels/focus; build/lint/typecheck; no extra UI kit. |
| U03 | F02 signup UI connected to API018 | U01,U02,A03 | Field/form errors, unknown response/recovery, no auto-login;375/768/1280. |
| U04 | F01 login/me/logout and cross-tab signals | U01,U02,A05–A07 | Normal/restricted routing, timers/activity semantics, cache clearing; browser evidence. |
| U05 | Account password-change UI | U04,A08 | No secret persistence/auto retry; restricted-only actions; must re-login after success. |
| U06 | Minimal F32 Admin detail/lock pilot UI | U04,R03 | Visible actual operation only; preview impact/reload/version conflict and lock-result lookup; no fake menus/actions. |
| V01 | Integrated M0 verification, residual trace and owner walkthrough | D13,A03–A08,R03,U03–U06 | Real SQL/API/browser happy/failure/race paths, all7role scope matrix where applicable, trace W01–03; no app caller evidence claimed for later modules. |

M0 completion is not acceptance of remaining F32 commands, all audit UI, all55UUID
command callers or full product. Mandatory audit/replay used by implemented callers
must be complete; pending caller coverage stays explicit. Never mark V01done merely
because builds pass. Remaining browser/SQL/profile evidence blocks relevant claims.

## 8. Ba task đầu: scope đủ review, chưa dispatch

P01 manager-owned planning/preflight: update only this plan and a future
`docs/plan/M0_LOCAL_PROFILE.md`/compatibility manifest when concrete values are
resolved. Inventory is read-only. Forbid source changes, installs, DB DML/DDL,
secret files and public exposure. Do not put credentials/connection strings/keys
in documents. Current SDK/SQL mismatch is a verified prerequisite, not code task.

P02 backend skeleton proposed scope:
`backend/EnglishCenter.sln`, `backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj`,
`backend/src/EnglishCenter.Api/Program.cs`,
`backend/tests/EnglishCenter.Tests/EnglishCenter.Tests.csproj`,
`backend/tests/EnglishCenter.Tests/Infrastructure/StartupTests.cs` and a nonsecret
`.gitignore` update if required. Project format/package/test-host choice must be
verified before these names and caps become a runnable English worker contract.
No runtime secrets/appsettings literals, generated migrations/business controllers,
package additions beyond approved manifest or modifications of baseline docs.
Manager checks proposed: `dotnet build backend/EnglishCenter.sln` and
`dotnet test backend/tests/EnglishCenter.Tests/EnglishCenter.Tests.csproj`.
These targets do not exist yet; not executed or reported passed in planning.

P03 frontend skeleton proposed scope: `frontend/package.json`, its approved
lockfile, `frontend/index.html`, Vite/TypeScript/lint configs, `frontend/src/main.tsx`
and minimal app/styles entry files, `.gitignore` if specifically included. Exact
package manifest/config filenames and scripts must be reviewed before contract;
lockfile changes allowed only for this authorized bootstrap, not later generic tasks.
Manager checks proposed: build/lint/typecheck using scripts actually added and
reviewed. Forbid business pages/mock success, new UI libraries, design/ADR edits,
credentials and deployment config. Scope may split configuration from code if large.

For every later task Codex materializes a numbered task card with exact paths,
read context/consent, base commit, dependencies, success/failure evidence and caps.
No runner contract with approximate folder-wide permission is dispatched from this
plan. Worker runs validate first, detached edit worktree, no permission bypass,
receipt/events/diff review, manager tests and owner permission before primary apply.

## 9. Những việc chưa cần owner quyết định lại

Product rules/roles/save units/stack remain accepted; D12/D13 deferred. Proposed
full structural DB groundwork is an implementation ordering recommendation, not a
new data model. Before execution, ask only necessary concrete development-profile,
package or bootstrap choices not determined by accepted contracts; bundle choices
with recommendations and actual evidence. Do not request deadline/weekly hours.

## 10. Skill selection gate

Áp dụng [SKILL_ROUTING](SKILL_ROUTING.md) mỗi task viết code. Mapping P/D/A/R/U/V
đã ghi tại policy; task card phải có selected implementation/review skills,
source revision/hash, applicable overrides và actual verification. P01 chỉ
planning không ép code skill; AGY task phải đưa selected skill files/policy vào
read scope hợp lệ trước validate. Không tự activate C2C hoặc broaden đọc/ghi.
