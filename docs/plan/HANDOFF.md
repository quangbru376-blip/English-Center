# Handoff — E01–E05 và complete design baseline đã Accepted

## Latest checkpoint — P07/P08/D01 batch applied PASS

Updated2026-10-04: owner-requested P07→P08→D01 batch complete, reviewed PASS,
applied/reverified final code mainf6fd902. Locked restore/build0warnings0errors/
400tests PASS/0fail/0skip. Cards: [P07](P07_TASK_CARD.md), [P08](P08_TASK_CARD.md),
[D01](D01_TASK_CARD.md). Error/binding primitives, pinned offline/raw credential
helpers and four-table EF mapping implemented. D01 has no SQL execution/migrations;
real auth/CSRF/credential workflows/SQL constraints/readiness remain integration gates.
Rejected AGY overread output never applied; final mapping completed independently
by Codex in a fresh tree. Earlier P02/P03/P05/P04/P06 PASS remain applied.
ADR-163 permits autoapply after Codex PASS within owner-authorized stages.
Next proposed D02 is not started. No remaining work in this three-family batch;
rest M0/SQL/migrations/runtime installation/push/deployment outside this request.
M0/V01 incomplete.
## Checkpoint code đầu — P02 applied/reverified PASS trên main

P01 prerequisites cho P02 đã hoàn tất; P02 **PASS** sau hai manager fixes,
detached commit d9eb194a7d53c321cbfad691abba730388588385. Xem
[task card/receipt/worktree/verification](P02_TASK_CARD.md) và
[manifest](M0_LOCAL_PROFILE.md). Build0warning/0error, discovery10/tests10pass,
lock/license/hash/NuGet audit đã kiểm. Owner xác nhận apply riêng ngày04/10;
cherry-pick thành main df7cd9a, source/lock hashes khớp review; main locked
restore/build sạch/discovery10/tests10pass. Không push/deploy. Tiếp theo cần
owner direction cho task mới; Full LOCAL_DEV/DB/readiness và M0/V01 chưa hoàn tất.

### Handoff đã thực hiện

Owner ngày04/10/2026 yêu cầu handoff để chat kế tiếp viết giai đoạn code đầu:
đọc [FIRST_CODE_HANDOFF](FIRST_CODE_HANDOFF.md), hoàn thiện P01 rồi viết/review
P02 backend/test skeleton bằng workflow Codex manager/AGY worker. Không tự
apply vào main, mở P03/toàn M0, SQL changes hoặc deployment.
Owner trước đó yêu cầu bàn kế hoạch viết code sau nghiệm thu. Đã mở bản thảo
[implementation-plan.md](implementation-plan.md), đề xuất M0–M4; chưa duyệt
thứ tự/mốc thực thi. Owner chọn04/10/2026 chia task theo chất lượng/độ phức
tạp/giới hạn output; không cần hạn nộp hoặc giờ mỗi tuần để chia việc.
Phân biệt nhóm M0–M4, slice tích hợp và từng task sinh code; Codex kiểm chứng
trước task phụ thuộc, không giảm guard/test để nhét trong một lần sinh.
Planning được yêu cầu riêng; P02 code/package verification đã apply trên main,
không SQL changes/runtime install/deployment.

Owner đã nghiệm thu actual E04-v1 trước (ADR-160),rồi E05-v1/complete baseline
(ADR-161),không sửa outputs đã trình. Không bắt đầu lại D10/SC01–04 hoặc xin
nghiệm thu lại E04/E05. Nghiệm thu này chỉ chốt thiết kế,chưa implementation.

### Tải ngữ cảnh đúng thứ tự

1. AGENTS.md→PROJECT_CONTEXT→ARCHITECTURE→CURRENT_STATE→DECISIONS→TODO.
2. HANDOFF/design-baseline/requirements/questions/OWNER_REVIEW.
3. E01 FUNCTION_SPEC/E02 WORKFLOW_SPEC/E03 DATABASE_SPEC Accepted.
4. SYSTEM_ARCHITECTURE_SPEC E04-v1 +E04_FINAL_CONTRACT,catalog/DTO/SC02replay/
   SC03SQLintegration/SC04runtime/SC01parity;E05_BASELINE_REVIEW E05-v1 Accepted.
5. Side report/sequence files/skills chỉ khi task cần;installation không tự
   activate integration/delegation/agents,instructions là inbox.

### Checkpoint hiện tại

SQL assessment04/10/2026 completed: installed SQL2025 Express17.0.1000.7
retained by ADR-162; see [assessment](SQL_INSTANCE_ASSESSMENT.md).
Read-only inventory/type/decimal/JSON/SHA and system-DB snapshot read succeeded;
no app DB creation/config changes. Current RTM lacks newer patches; patch review
before real data/exposure and encrypted-archive backup prerequisites remain.
P01 package compatibility và P02 đã applied/reverified trên main; còn full runtime slots
trong M0_LOCAL_PROFILE. Các preparation checkpoints phía dưới có trước execution.

- M0 order synchronized04/10/2026: docs/plan/M0_QUALITY_PLAN.md section11 holds
  proposed39family default order and Mermaid; dependencies in sections4–7.
  Start=P01 planning/profile. P05beforeP04, A04beforeA03, D07beforeSQLaudit/replay
  evidence explicit; full readiness/skill gates retained.
  SQL target updated to installed SQL2025 Express by ADR-162; no install/code/dispatch.

- Skill audit04/10/2026: AGENTS and docs/plan/SKILL_ROUTING.md now require
  scope-appropriate backend/frontend skills and Codex code-review for code tasks.
  Task cards/worker contexts include skill sources, canonical overrides and actual
  verification.4local skills audited; C2C inactive, no dispatch/code/skill install.

- M0 detailed planning04/10/2026: docs/plan/M0_QUALITY_PLAN.md records task families,
  dependencies, evidence gates and first3proposed scopes. Local SDK10.0.200/
  Node24.16.0/npm11.13.0 checked; SQL .\SQLEXPRESS version17.0.1000.7 Express
  is now the accepted SQL2025 Express local target (ADR-162), assessed in
  docs/plan/SQL_INSTANCE_ASSESSMENT.md. Keep this instance; no Developer install.
  P01 still needs package/profile checks; full DB readiness retained. No app code,
  installation, migration or dispatch performed.

- Worker readiness04/10/2026: owner approved named context sharing; first run
  stopped/rejected for extra read; narrowed analyze SUCCESS/exit0,2allowedreads,
  zerochanges. Codex events/diffs/detachedHEADs/6runner tests verified.
  [Readiness](antigravity-worker-readiness.md) has durable contract/receipt.
  Actual edit mode untested; no worker changes applied/code/push/deploy.
- All5designstages done;E01–E05 actual outputs Accepted. ADRrecords001–162
  giữ trạng thái riêng;RF03=156,RF01/02/04=157–159,actualE04=160,baselineE05=161.
- OWNER_REVIEW75rows/73accepted;chỉ D12/D13 unchecked/hoãn,originalnotes kept.
  Owner confirmation authorized only2newEchecks;mọi priorchoices/notes giữ.
- Exact actual E04 package/technical refinements Accepted:154API/154schema/
  55UUIDcommands/6lookups/32F/21W/37NV/24tables230fields;C1bytes/actions/refs/
  strictparsing/errorprecedence/liveauthorization/24hretention/expiry409,
  7SP6FN/EF5trigger/24tablegrants/gates/audit/frontend/CSV6columnsfooter/
  10000records inclfooter/5000000encodedbytes/runtime typedcontracts.
- E05 Accepted:37NVindividualtrace/stateparity/24cross-contractcases/10findings/
  excluded/deferred/rubric/evidence. No unresolved in-scope product question.
- DF01–05 môi trường/evidence prerequisites giữ;D12/D13không hỏi lại hiện tại.
  No numericSLO/provider/origin/port/proxy/backupdestination/patchpin installed,
  requiredmissingprofile failclosed. Targets/runbooks not runtime testpasses.
- Source Word/E01/E02/E03/priorADRbytes/sidework kept. Documents/links/tables/
  registry/JSON/literalbytehash checks only;no appSQL/HTTP/EF/grants/browser/
  CSV/race/load/backuprestore execution or implemented architecture claim.

### Việc tiếp theo và giới hạn

SQL đã đánh giá tại chat này; owner dự kiến chat khác bắt đầu code bước đầu.
Chat đó đọc ADR-162/SQL_INSTANCE_ASSESSMENT rồi xử lý P01 trước P02 theo
M0_QUALITY_PLAN section11, giữ skill/quality/worker apply gates. Không xem
assessment là P01 hoàn tất hoặc cấp quyền tạo/migrate DB.
Quyền thực thi giới hạn P01→P02 đã được owner cấp qua yêu cầu handoff04/10;
không hỏi lại có được bắt đầu P02 khi prerequisites đủ. Chi tiết boundaries/
worker consent và apply gate tại FIRST_CODE_HANDOFF.md.
Không suy ra quyền code/SQL/Git/scaffold/Slice0/packages/deploy/tunnel/
backuprestore/config/integration từ yêu cầu thảo luận kế hoạch. Khi owner yêu cầu thay design,update
source/trace/affectedADRs;không hỏi lại quyết định/output đã Accepted.
D12/D13 và prerequisites chỉ revisit khi owner-authorized scope tới chúng.
## Context lịch sử — không override checkpoint hiện tại

Các next-step “bắt đầu SC02”,“E04 incomplete” hoặc “chưa lậpE05” dưới đây
phản ánh thời điểm cũ. Current status/next action ở trên và memory chuẩn.

Memory chuẩn ưu tiên hơn handoff. Chủ dự án đã duyệt Gói 1 và toàn bộ đề xuất
Gói 2 FE01–FE10. FE01a/FE01b/FE02 giữ ADR-103–ADR-105; FE03–FE10 thêm
ADR-106–ADR-113. Owner cập nhật bảng duyệt D05/D08/D09/D11/D14/D16,
đã lưu ADR-114–ADR-119; trả lời riêng D17 chốt owner phụ trách toàn bộ vận
hành mặc định (ADR-120), chỉ hỏi lại khi owner yêu cầu đổi người. D12/D13
hoãn đến khi chốt nơi chạy web/phần mềm. Owner xác nhận ghi nhận toàn bộ
Gói 3 FE11–FE19, không nêu ngoại lệ: lưu ADR-121–ADR-129, cập nhật chín ô
và đồng bộ đặc tả/target/catalogue. Không hỏi lại. Owner đã yêu cầu bắt đầu
E01 và đồng ý E01-v1 trực tiếp ngày30/09/2026; đầu ra tại
docs/design/FUNCTION_SPEC.md đã nghiệm thu, FUNCTION_CATALOG là chỉ mục nguồn.
Owner cũng đồng ý bản E02-v1 hoàn chỉnh ngày30/09/2026 tại
docs/design/WORKFLOW_SPEC.md. OWNER_REVIEW có71/75 dòng đã duyệt;
E03-v1 đã nghiệm thu01/10/2026; D12/D13 hoãn, E04/E05 chưa nghiệm thu; không tích các E còn lại tự động.
Owner cần chạy theo buổi test/chỉnh sửa/báo cáo, không 24/7, có truy cập từ
thiết bị khác. Đã yêu cầu lưu ý tưởng laptop/tunnel vào
docs/design/SELF_HOSTED_DEMO.md; đã duyệt các chính sách D nêu trên, chưa
cho phép cài đặt/công khai; E03 đã nghiệm thu thiết kế, E04/E05 chưa nghiệm thu.
Owner cung cấp rubric DBMS330284 trong `instructions/` và xác nhận áp yêu cầu
kỹ thuật CSDL/tích hợp ứng dụng cho E03–E04 (ADR-130), không nhập quy định
nộp báo cáo học phần khác vào sản phẩm. Ngày01/10/2026 owner chọn cách1:
theo miền nghiệp vụ + đối chiếu rubric. Owner duyệt gói A ngày01/10/2026
(ADR-131): mô hình tổng thể24 bảng và quy ước nhất quán đã trình; phần B
tài khoản/profile/session đã trình cũng duyệt (ADR-132); phần C cấu hình/
thi đầu vào đã trình duyệt ADR-133, PENDING→TESTING đã đồng bộ requirements/E02.
Phần D lớp/xếp lớp/lịch đã trình duyệt ADR-134; W15 đã đồng bộ tạo DRAFT cùng
xếp lớp/audit. Phần E điểm danh/điểm/bảng điểm đã trình duyệt ADR-135;
finalized-only final_score đã đồng bộ requirements/E02. Phần F hóa đơn/thanh
toán/audit/replay/counter đã trình duyệt ADR-136. G1 danh mục/mục đích/đường
gọi 5 trigger,6 view,6 UDF,7 SP đã trình duyệt ADR-137. G2 quyền DB/giao dịch/
khóa/snapshot/index/kiểm chứng đã trình duyệt ADR-138. DATABASE_SPEC đã tổng
hợp E03-v1 và owner nghiệm thu toàn bộ bản thực tế01/10/2026. E03 [x]/stage3
done; mục14 giữ phạm vi/chi tiết vật lý và việc chuyển E04. E04-v0.1 ở
SYSTEM_ARCHITECTURE_SPEC đang soạn; phần Gói1 đã trình được duyệt ADR-139,
Gói2a/2b/3a/3b/3c/4a/4b/4c/4d/4e từ Auth đến Attendance/điểm nháp đã trình
duyệt ADR-140–149; Gói4f/5/6/7/8/9 (23 mục đã trình trong E04-R1) được
owner tích đủ và xác nhận đồng ý toàn bộ, ghi ADR-150–ADR-155. E04 chưa nghiệm thu toàn bộ. SQL object/transaction/DB role phải gắn
nghiệp vụ đã chốt. Không tự hỏi lại phạm vi/cách tổ chức đã chốt.

## Đọc trước khi tiếp tục

Đọc AGENTS.md rồi đúng thứ tự PROJECT_CONTEXT → ARCHITECTURE →
CURRENT_STATE → DECISIONS → TODO trong docs/. Sau đó requirements.md,
plan/design-baseline.md, plan/questions.md, plan/OWNER_REVIEW.md và
design/FUNCTION_CATALOG.md. Tra reference/REPORT_BASELINE.md khi cần nguồn.

## Điểm đã chốt

- ADR-001–ADR-082 giữ nguyên. A01–A10/B01–B07/C01–C16 đã duyệt ngày
  28/09/2026, ghi ADR-083–ADR-094 và đồng bộ.
- Gói 1 frontend đã duyệt ngày 29/09/2026: D01–D04/D06–D07/D10/D15,
  ghi ADR-095–ADR-102. Không hỏi lại:
  - ADR-095 / D01: modular monolith/one DB/module ownership, server quyết định
    business rules/quyền/tiền/tổng điểm; frontend theo feature/nhập-hiển thị.
  - ADR-096 / D02: /api/v1/DTO/errors code-message-fieldErrors-traceId,
    400/401/403/404/409; pagination default20/max100/stable ID tie-breaker.
  - ADR-097 / D03: user-initiated authenticated business requests mới cập nhật
    idle; background/typing/mouse không; tab cùng profile chia phiên/logout.
  - ADR-098 / D04: phiên mật khẩu tạm tối đa10 phút/capped temp expiry,
    chỉ đổi/logout; đổi hủy mọi phiên rồi đăng nhập lại.
  - ADR-099 / D06: same-origin host-only HttpOnly/Secure/SameSite=Strict
    JWT cookie, CSRF/Origin login-write, HTTPS/no durable sensitive browser cache.
  - ADR-100 / D07: login response chung trước xác thực, IP100/phút,
    429/Retry-After/proxy tin cậy; tách throttle account5 lần/15 phút.
  - ADR-101 / D10: CSV UTF-8 BOM/comma/CRLF/escape, max10.000 dòng hoặc5 MB,
    reject toàn file, formula safeguards/full-width, DB không đổi;
    Excel/LibreOffice cần kiểm chứng theo giới hạn đã duyệt.
  - ADR-102 / D15: Học viên ưu tiên375 px/nhân sự dày ưu tiên1280 px,
    tất cả dùng/kiểm tra375/768/1280; keyboard/nhãn/non-color/Chrome-Edge,
    tương lai xUnit/SQL integration/races/manual7 roles/E2E.
- FE01a shared shell duyệt ADR-103: sidebar khoảng240 px/1280,
  rail khoảng72 px/768 có mở rộng, drawer trái/375/nội dung một cột;
  header screen/account, mobile tên-vai trò trong account menu ADR-084;
  selected/focus rõ, drawer đóng chọn/backdrop/Escape/trả focus.
- FE01b menus/landing duyệt ADR-104: sơ đồ và mã F trong OWNER_REVIEW;
  login mới vào mục đầu vai trò, actions trong trang, account menu riêng.
- FE02 cấu trúc trang/đường đi duyệt ADR-105: URL riêng list/detail/complex
  form; summary/tab đúng quyền; modal cho xác nhận/lý do/form ngắn; back giữ
  filter/page/sort, chỉ filter không định danh ở URL, tên/email/SĐT trong
  working memory, không URL/localStorage/sessionStorage; breadcrumb rõ.
- Toàn bộ Gói 2 được owner duyệt sau lượt lưu file tự host; đối chiếu đề xuất
  gốc trong cùng chat, không suy là đồng ý D vận hành:
  - FE03 / ADR-106: bảng1280/rút gọn768/card-list375; sổ điểm matrix desktop/
    từng học viên mobile; phân trang, không infinite scroll/bottom navigation.
  - FE04 / ADR-107: không quyền thì ẩn, thiếu điều kiện thì disabled/lý do;
    “Chỉ xem” và phụ trách bị chặn rõ, không quyền đổi người ngoài cửa sổ.
  - FE05 / ADR-108: lưu rõ/không autosave, lỗi trường/toàn form, warning chưa
    lưu, code hoa khi blur/SĐT chuẩn; không cắt/round/merge/PII drafts bền.
  - FE06 / ADR-109: skeleton/no-data-no-match/submitting/errors401–409;
    lỗi quan trọng không chỉ toast, vẫn giữ quyền/privacy/replay protection.
  - FE07 / ADR-110: một confirmation có đối tượng/ảnh hưởng/động từ/lý do
    đúng phạm vi; không double confirmation thường, bàn giao FE18 duyệt riêng ADR-128.
  - FE08 / ADR-111: Tailwind/component cục bộ accessibility, không thêm UI/
    form/table/calendar/chart library; package mới phải xin phép theo nhu cầu.
  - FE09 / ADR-112: tiếng Việt, dd/MM/yyyy/HH:mm/giờ Việt Nam/vi-VN;
    tiền chuỗi giữ chính xác, không đổi numeric/time/validation contracts.
  - FE10 / ADR-113: sáng/gọn/font hệ thống/trắng-xám/xanh, non-color/focus/
    vùng chạm khoảng44 px/reduced motion; chưa brand/accessibility pass.
- Các D mới duyệt riêng từ bảng và xác nhận D17 (không suy từ Gói 2):
  - D05 / ADR-114: PasswordHasher/tham số runtime; mật khẩu tạm ngẫu nhiên
    mật mã 32 ký tự/hiện Admin một lần; blocklist SecLists offline 10k-most-common
    + probable-v2_top-12000, pin/license/rà quý, không dịch vụ ngoài/bao phủ mọi lộ.
  - D08 / ADR-115: audit nghiệp vụ/tài khoản bắt buộc cùng transaction/lỗi rollback;
    pure security events best-effort/fallback giới hạn-không bí mật, không bỏ
    quyền hoặc phục hồi phiên logout vì lỗi log/không thông báo người dùng mới.
  - D09 / ADR-116: audit bắt buộc giữ lịch sử; log bảo mật thuần túy/IP 90 ngày,
    dọn bằng vận hành/bỏ IP nhưng giữ audit bắt buộc. Actor NULL khi chưa xác
    thực; gộp request rate-limit giữ count/IP/phút và bộ đếm; không app purge.
  - D11 / ADR-117: một trung tâm/10.000 học viên/50 đồng thời giữ lịch sử là
    phạm vi kiểm chứng, không trần nghiệp vụ/kết quả đo/multi-tenant-phân tán.
  - D14 / ADR-118: backup SQL hằng ngày/mã hóa/riêng ổ chính, 7 ngày/4 tuần,
    restore thử mỗi tháng; RPO24h/RTO2h mục tiêu tham chiếu, chưa đạt bằng chứng.
    Nơi lưu/quyền phải chốt trước chạy; xử lý bỏ lỡ backup khi laptop tắt.
  - D16 / ADR-119 (lịch sử): .NET10 LTS/EF10/SQL2022 Developer chỉ dev-test-demo nội bộ; SQL choice được thay bởi ADR-162 ngày04/10/2026
    non-production, giữ stack; patch tương thích/lock sau baseline, production
    cần edition/license riêng. Chưa cài đặt hoặc chốt patch.
  - D17 / ADR-120: owner phụ trách toàn bộ release/secret/backup-restore/bảo trì
    mặc định; chỉ hỏi lại người khi owner yêu cầu đổi. Không thêm Admin/DBA.
    Bàn giao setup/quyền/security/recovery/trace, rà dependency/secret/blocklist,
    không auto integration/coding agent hoặc truyền dữ liệu ra ngoài.
- E03-v1 đã nghiệm thu:24 bảng/230 trường, gồm22 entity + replay/counter,
  full dictionary/ERD/keys/history/SQL/roles/transactions/rubric/trace. Chưa schema chạy.
- Stage1 done: owner accepted E01-v1 ngày30/09/2026;37 retained NV/32 F/
  7 thành phần mỗi chức năng/validation matrix/shared actions đã kiểm cấu trúc/
  trace tài liệu. Stage2 done: owner accepted E02-v1 ngày30/09/2026 tại
  docs/design/WORKFLOW_SPEC.md,21 W/37 NV/32 F đã kiểm cấu trúc/trace.
  Stage3 E03 done theo nghiệm thu01/10/2026; E04 doing, E05 todo. Không Git/
  scaffold/migration/code/application tests.

## Checkpoint lịch sử — hoàn thiện bản E04 thực tế để nghiệm thu

Checkpoint03/10/2026: [E04_CONTRACT_CATALOG E04-C1](../design/E04_CONTRACT_CATALOG.md)
gom154 method/path (55 UUID commands/6 caller-result paths),32 F và21 W→37 NV/
API/service/E03/verification trace; URL frontend mới vẫn là refinement dự thảo.
[E04_DTO_SPEC E04-D10](../design/E04_DTO_SPEC.md) có154/154 schema drafts
API-001–154: giữ149 D9, thêm5 Reports/CSV; không còn API thiếu schema miền.
SC-01 vẫn cần rà whole registry/field/null/sort/error/guard/example parity.
Báo cáo role-scoped/current-state/snapshot/applied filters/whole-set totals,
decimal38/distinct/LESSON-vs-exam/FINALIZED-no-data và fresh bounded CSV rõ.
Report options/columns-footer/bytes/header/error là refinement dự thảo,
không thêm quyền/metric hay sửa nguồn đã chốt. Shared lookups không union rights.
RF03 root/weekly-create/move-destination cutoff được duyệt03/10/2026, ADR-156;
RF01/02/04 nay Accepted ADR-157–159; không suy nghiệm thu D10/E04/E05.
SC-01–04 giữ DTO/guard/replay bytes/SQL/runtime gaps; chỉ kiểm cấu trúc tài liệu,
không tests/runtime proof. E04/E05 chưa nghiệm thu, D12/D13 hoãn.
Owner yêu cầu bàn giao sang chat khác03/10/2026; next SC-02 canonical
action/digest/outcomes/compatibility, rồi SC-03/04 và rà SC-01 toàn154 schemas
trước nghiệm thu actual E04. Không còn API thiếu schema miền, chưa đủ E04.
RF03 future cutoff đã chốt ADR-156, requirements/NV19/W16 có addendum03/10;
không hỏi lại. RF01/02/04 đã duyệt ADR-157–159; questions ghi resolutions.
Không suy duyệt các schema/URL/outcome mới từ lời đồng ý RF03.
Giữ role projection/task scope/absence/immutable money-time/full-set đã viết;
hoàn thiện SC-02 canonical replay/action codes, SC-03 SQL/EF/grants/audit/locks,
SC-04 hồ sơ runtime/deferred. Không mở lại các gói đã duyệt hoặc tick E04/E05.

Gói 2 FE01–FE10 đã duyệt đầy đủ ADR-103–ADR-113; Gói 3 FE11–FE19
đã duyệt đầy đủ ADR-121–ADR-129, không hỏi lại. Nhu cầu hosting đã rõ: theo buổi test/
chỉnh sửa/demo giáo viên và thiết bị khác, không 24/7. Bản tổng hợp tự host
qua local/LAN/tunnel ở docs/design/SELF_HOSTED_DEMO.md; không hỏi lại mục đích
hoặc yêu cầu email trường cho hướng này. D12/D13 giữ hoãn; nhà cung cấp/
HTTPS-proxy/startup và nơi lưu/quyền backup chưa chốt, không triển khai.
D14/D16/D17 đã duyệt chính sách backup/runtime/người vận hành, không hỏi lại.
Owner đã yêu cầu trình một lần toàn bộ Gói 3, sau đó xác nhận ghi nhận toàn
gói, không có ngoại lệ. Chín mục đã tích theo lời duyệt trực tiếp và lưu ADR:
FE11→ADR-121, FE12→122, FE13→123, FE14→124, FE15→125, FE16→126,
FE17→127, FE18→128, FE19→129. Các hợp đồng đã duyệt:

- FE11: thẻ từng hồ sơ/việc tiếp theo đúng quyền, không điểm chưa FINALIZED.
- FE12: tuần desktop + agenda mobile, click chi tiết/không kéo-thả; giữ lịch sử.
- FE13: nhập → preview LESSON cụ thể/conflicts/count → tạo toàn bộ; revalidate.
- FE14: batch điểm danh một LESSON/all-or-none cho các giá trị đã ghi; chưa ghi
  không thành Có mặt/Vắng. Nháp điểm lưu theo một học viên/các giá trị thay đổi
  và nhận xét cùng nhau; cho thiếu thành phần, không blank→0/xóa điểm đã lưu.
- FE15: tách lưu nháp và gửi/gửi lại cả lớp; còn sửa chưa lưu thì chặn gửi,
  đủ điều kiện mới confirmation cả lớp/trả có lý do/chốt công bố/all-or-none.
- FE16: thẻ/bảng không chart; Áp dụng lọc/queried_at, quyền/metrics/CSV giữ nguyên.
- FE17: bản in Student-own A4/fresh authorized data/trạng thái rõ/no PDF API.
- FE18: khối kết quả F32/email-role-temp-expiry, copy chủ động/tùy quyền browser,
  ack hiểu đóng không xem lại. Không hứa xóa clipboard/phục hồi secret, không
  replay/auto-reset để lấy secret sau timeout; kiểm tra rồi reset chủ đích.
- FE19: audit rows newest/pagination, chi tiết cũ-mới allowlist/cùng operation_id;
  actor NULL/retention đúng, không secrets/purge/export.

Đã đồng bộ requirements mục 4.8/ADR/target/catalogue/affected memory; giữ
66 dòng cũ và ghi chú nguyên vẹn. Tổng75 dòng hiện tại:71 đã duyệt,4 còn mở (D12/D13/E04/E05).
FE14/FE18 được duyệt riêng, không suy từ Gói 2. E01-v1 đã được owner duyệt
ngày30/09/2026: FUNCTION_SPEC + FUNCTION_CATALOG có actor/input-output/rights/
validation/scope và trace39 NV nguồn (37 giữ)/32 F, kể cả account/profile/home.
Stage1 done. E02-v1 tại docs/design/WORKFLOW_SPEC.md đã tổng hợp21 flow,
state transitions, actor/guards/effects, audit/transaction, concurrency/
unknown-response, trace đủ37 NV/32 F và ca cần kiểm chứng thiết kế. Đã kiểm
cấu trúc/độ phủ tài liệu, chưa chạy ứng dụng/test; owner đã đồng ý bản E02-v1
hoàn chỉnh và dòng E02 đã tích. Stage2 done. DATABASE_SPEC E03-v1 đã được owner nghiệm thu toàn bộ01/10/2026:
24 bảng/230 trường, keys/constraints/history/projections/locks/SQL objects/
4 SQL roles/index candidates/13 TX/18 DC/rubric/21 W trace. E03 [x], stage3 done.
ADR-131–138 giữ nguyên; đây là nghiệm thu artifact, không thêm ADR/code/tests.
Phần E04 Gói1 đã trình được owner duyệt01/10/2026, lưu ADR-139: Base64/
expectedVersions, UUID header Idempotency-Key/retry cùng intent/no secret
replay và POST-body PII search. Chi tiết bổ sung chưa trình cần rà trong
artifact, không suy duyệt tất cả mục3. Gói2a Auth/session đã trình được owner
đồng ý01/10/2026, ADR-140:5 Auth endpoint/context tối thiểu/CSRF RAM-header,
lấy lại sau login-logout/403 CSRF_INVALID/no automatic retry/no credential
replay. Chi tiết bổ sung chưa trình cần rà trong E04 thực tế, không hỏi lại
phần đã chốt. Gói2b Administration đã trình được owner duyệt01/10/2026,
ADR-141: account search/detail/staff-create/reset/4 state commands, UUID/
version/atomic audit-replay, calling-Admin safe completion lookup/first secret
only/no replay secret, audit list-detail-operation/no new Admin powers.
Exact DTO/status/transitions/action codes/signatures chưa trình vẫn rà cùng
artifact. Gói3a Students đã trình được owner duyệt01/10/2026, ADR-142:
6 endpoint đăng ký/profile/home/catalogue, own /me/Student NORMAL/2 profile
RV/4 fields/atomic signup không auto-login/no signup-profile Idempotency-Key
hoặc credential-form replay/unknown không auto retry/ACTIVE-history đúng quyền.
Exact DTO/cache/error/sort/query chưa trình còn rà cùng artifact.
Gói3b Student Admissions đã trình được owner duyệt01/10/2026, ADR-143:
own Registration/create/cancellation preview-cancel; options/book/cancel/
reschedule atomic old-new Booking; own calendar/results; confirmation
preview-confirm; caller-only result lookup,6 write-command UUID và atomic
effects/audit/completion; related versions/booking-attempt membership,
preview không giữ chỗ, server recheck/409/deliberate reload. Exact routes/DTO/
status/guard-key/digest/SQL signatures chưa trình còn rà cùng artifact,
không suy blanket approval.
Gói3c Admissions/Finance đã trình được owner duyệt01/10/2026, ADR-144:
role-specific search/detail; approval-preview/atomic ADMITTED-Invoice UNPAID;
reasoned rejection/cancellation-preview/atomic cancel; own Invoice/fresh browser
print/no PDF; collection-preview/atomic Payment-PAID/metadata-only reasoned
old-new correction;5 UUID commands/related versions/atomic audit-completion/
caller-only lookup/unknown không proof failure và financial snapshot reads.
Exact routes/DTO/status/guard-key/digest/SQL/settings chưa trình còn rà cùng
artifact; không suy blanket approval.
Gói4a ca thi/Teacher chấm thi đã trình được owner duyệt01/10/2026, ADR-145:
AcademicAffairs Session management/open-close/cancellation preview-atomic cancel/
grader options-preview-reasoned change; task-specific rosters/current Teacher
assignment; per-student initial score/absence/reasoned correction, atomic
result-best Band/history;9 UUID commands/related versions-set/server recheck/
caller lookup/no automatic effects và SQL roster/state/Band callers.
Exact routes/DTO/status/guard-key/SQL/read-isolation/runtime refinements chưa
trình còn rà cùng artifact, không suy blanket approval.
Gói4b Course/chương trình/cấu hình đã trình được owner duyệt01/10/2026,
ADR-146: catalogue list-detail-create/name-price, explicit deactivate/restore;
Program DRAFT/whole-configuration-components TX/activation preview/lifecycle/
separate name edit không mở freeze; Band seed reads/no editor-unlock;
10 UUID writes/Program-component set/atomic audit-completion/current guards/
caller lookup/no automatic activation. Exact routes/DTO/status/guard-key/SQL/
runtime chưa trình còn rà artifact, không suy blanket approval.
Gói4c Class/xếp lớp đã trình được owner duyệt01/10/2026, ADR-147: Class
list-detail/create PLANNED/capacity, Teacher options-change preview/live guards;
explicit open-start-complete-cancel/prerequisite-effect preview/cancel reason;
eligible Registration/suitable Class options, one-Registration placement-preview/
atomic Assignment-DRAFT report-audit-completion/no default Score-Attendance;
AcademicAffairs management/Teacher currently assigned/Student own scoped reads;
8 UUID writes/related versions-set/server recheck/caller lookup/no auto effects.
Exact routes/DTO/status/guard-key/SQL/read-isolation chưa trình còn rà artifact.
Gói4d Schedule đã trình được owner duyệt01/10/2026, ADR-148: scoped current/
history calendars; individual LESSON/FINAL_EXAM preview-create riêng, concrete
weekly all-or-none/no skip; pre-start move/no dependencies, reasoned atomic
non-held replacement cùng obligation/history/no extra LESSON, manual after-end
HELD/ONGOING/no undo-auto-complete;5 UUID writes/related versions/live recheck/
preview no reservation/conflict re-preview/caller lookup trước retry chủ đích.
Exact routes/DTO/status/guard-manifest/SQL/runtime chưa trình còn rà artifact.
Gói4e đã trình được owner duyệt01/10/2026, ADR-149: Teacher assigned-Class/
AcademicAffairs read-only Attendance, one-LESSON delta batch/no default-all;
scoped gradebook/components/return reason/no new DRAFT visibility; one-Student
scores-comment all-or-none/desktop-row-mobile page/missing≠0/blank≠delete/
server total/no auto send-publication;2 UUID writes/related-parent versions/
live guards/caller lookup/no autosave-overwrite. Exact routes/DTO/status/guard-
manifest/SQL/integration chưa trình còn rà artifact.
Gói4f/5/6/7/8/9 đã được owner tích đủ23 mục E04-R1 và xác nhận đồng ý toàn bộ
ngày01/10/2026, lần lượt ADR-150–ADR-155: whole-Class grades, reports/CSV,
frontend state/auth, backend/SQL/replay/audit, laptop config/runbook và trace/
verification/đóng draft. Chỉ nội dung đã trình được duyệt, không mọi refinement
chưa viết. Tiếp theo hoàn thiện bản E04 thực tế và gap/deferred để nghiệm thu
riêng, rồi E05; D12/D13 hoãn, không code/SQL/cài đặt/vận hành hoặc claim test pass.
OWNER_REVIEW đã được đồng bộ lại metadata/E01–E03 từ nghiệm thu đã chốt,
không coi bản lưu cũ/ô trống là hủy quyết định. Giữ nguyên các dòng/ý kiến khác.
Khi owner báo sửa tiếp, đọc toàn file thực tế và OWNER_REVIEW, ghi đúng phạm vi;
không blanket approve refinements tương lai hoặc tự tích E04/E05.
Không code, Git/scaffold/migration hoặc tự kích hoạt integration.

Sơ đồ menu FE01b đã duyệt:
- Student: Trang chủ; Khóa học & hồ sơ; Thi đầu vào; Hóa đơn; Lớp & lịch; Bảng điểm.
- Admissions: Hồ sơ tuyển sinh.
- AcademicAffairs: Ca thi đầu vào; Khóa & chương trình; Lớp học;
  Lịch học & thi; Xếp lớp; Duyệt bảng điểm.
- Teacher: Lớp & lịch dạy; Thi đầu vào; Điểm danh; Sổ điểm.
- Accountant: Hóa đơn & thanh toán; Báo cáo học phí.
- Director: Báo cáo tổng hợp.
- Admin: Tài khoản; Nhật ký.
Đề xuất login mới vào mục đầu vai trò, action trong trang, F01/F02 trước login.
Mã F đầy đủ ở OWNER_REVIEW; sơ đồ đã duyệt ADR-104, không thêm dashboard.
OWNER_REVIEW giữ75 dòng gốc và cổng nghiệm thu E04/E05 duy nhất. Ngoại lệ theo
yêu cầu trực tiếp owner01/10/2026: E04_REMAINING_REVIEW là nơi điền các gói
còn lại, không bản sao các cổng hoặc đồng ý tài liệu chưa có.

## Phần còn mở

D12/D13 giữ chưa duyệt và hoãn theo ghi chú owner đến khi chốt nơi chạy web/
phần mềm: performance/availability và cấu hình triển khai cụ thể. Không hỏi
lại ngay hoặc suy phương án tunnel đã được duyệt. Gói 3 FE11–FE19 đã duyệt,
không còn nằm trong danh sách câu hỏi. Còn bốn dòng mở: D12/D13 và E04/E05.
E01/E02/E03 đã nghiệm thu trực tiếp; E03 [x]/stage3 done. E04/E05 còn chờ
architecture/API → consistency/full baseline; không tích theo đồng ý một gói nhỏ.

## Giới hạn và cách lưu

Giữ7 vai trò/39 NV nguồn (NV10/NV11 loại →37)/32 F (F15 loại, không đánh lại).
Bám quyền xem ADR-083, menu account/lịch hiện tại-lịch sử ADR-084, /100,2 lượt/
hồ sơ; CLOSED chặn Student hủy/đổi booking; attendance từ lesson start,
điểm đầu vào từ test end; HELD do Giáo vụ sau end; whole-class grades.
Giữ no on-behalf/scholarship/retake sau confirm/transfer/refund/automated debt
penalties/holiday automation/individual makeup/extra LESSON/finalized override.

Khi owner duyệt các đầu ra tiếp theo, đồng bộ OWNER_REVIEW, requirements,
ADR đáng kể và affected memory/active plan/catalogue/architecture/provenance.
Không lưu transcript/
secrets/log tạm. Không sửa Word C:\Study\SOEN\final_workplace\Nhom18_ThietKeGiaoDien.docx.
instructions là inbox; Q001/Q002 implementation hoãn, Q003 integration chỉ
khi được yêu cầu. Skill installation không cấp quyền connection/delegation.
Skill architecture-decision-records đã dùng để lưu Gói1/Gói2 và D05/D08/D09/
D11/D14/D16/D17 và Gói 3 ADR-121–ADR-129 có context/reason/consequences;
giữ convention DECISIONS.md,
không tạo cây ADR khác.
Skill brainstorming đã dùng cho lượt đề xuất Gói 3 để nêu context/alternatives/
rủi ro; theo yêu cầu owner hỏi một lần thay cho nhiều lượt incremental.
codex-with-chatgpt đã cài, chưa kích hoạt/kiểm chứng kết nối.
