# E04 — Kiến trúc hệ thống và hợp đồng API

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản:E04-v1. **Owner nghiệm thu toàn bộ actual output03/10/2026,
Accepted ADR-160**, rồi E05-v1/complete baseline Accepted ADR-161.

Nguồn: [requirements](../requirements.md),[ADRs](../DECISIONS.md),
[E01](FUNCTION_SPEC.md),[E02](WORKFLOW_SPEC.md),[E03](DATABASE_SPEC.md).
Actual accepted package:[final contract](E04_FINAL_CONTRACT.md),catalog154/
DTO154/SC01–04 tại links bên dưới. Các exact refinements đã trình được chấp
nhận cùng actual E04,không chỉ các small-package policies. DF01–05,D12/D13
vẫn deferred/environment/evidence prerequisites;chưa implementation/tests.
[E05](E05_BASELINE_REVIEW.md) được nghiệm thu sau E04 trên đúng artifact,
không sửa phương án. Historical checkpoints bên dưới giữ provenance,
không next-step hoặc current acceptance status. No code/SQL/deploy/planning.

## Bản tổng hợp SC02–04-v1 và review SC01 — 03/10/2026

Actual E04 package gồm bản chính +[catalog154](E04_CONTRACT_CATALOG.md)+
[DTO154 D10](E04_DTO_SPEC.md)+[SC02 replay](E04_REPLAY_SPEC.md)+
[SC03 SQL integration](E04_SQL_INTEGRATION_SPEC.md)+
[SC04 runtime/deferred](E04_RUNTIME_PROFILE.md)+
[SC01 parity findings/trace](E04_PARITY_REVIEW.md).
Các phụ lục hoàn thiện placeholder canonical/signatures/runtime ở các mục
trước, **draft để owner rà**, không mặc nhiên Accepted từ ADR-139–155.
RF03 giữAccepted156; RF01/02/04 behavior Accepted157–159. Byte/ref/result
unions/SP-FN/EF/grants/locks/audit là technical refinements;không thêm business
rights/SQL objects/columns/packages. SC04 environment slots/D12/D13 giữ deferred.
Next actual E04 technical-refinement review rồi separate E04 acceptance;
E05-v1 nay được chuẩn bị có điều kiện; full baseline chưa Accepted. No code/SQL/tests/planning.
## 1. Đầu vào đã chốt — không hỏi lại

- Một trung tâm Anh ngữ,7 role cố định,37 NV/32 F/21 W và24 bảng/230 trường.
- Modular monolith, một SQL Server; ASP.NET Core/EF code-first/JWT và
  React/Vite/TypeScript/Tailwind; Controller → Service → Repository → DbContext.
  Services sở hữu auth/ownership/workflow/transaction, SQL phục vụ persistence/query.
- /api/v1, DTO tách entity, error code/message/fieldErrors/traceId, status400/
  401/403/404/409, stable pagination20/max100; backend là nguồn tiền/tổng điểm/quyền.
- Tiền API là chuỗi số chính xác, UTC instants/ISO Z và ngày/giờ VN theo ADR-091.
  Missing Score/Attendance khác0/Vắng; per-LESSON/per-Student saves và whole-Class
  grade actions giữ nguyên, không autosave/blank-delete/merge stale edits.
- Cookie/JWT/session/CSRF, replay≥24h/secret marker, audit atomic/best-effort/
  retention và reporting/export scopes giữ ADR-092/093/099/101/115/116/128/138.
- Mốc kiểm chứng10.000 Student/50 đồng thời chưa được đo; laptop chạy theo buổi,
  owner vận hành, backup policy/runtime đã chốt. D12/D13 và cấu hình cụ thể vẫn hoãn.
  Không microservices, role mới, OAuth/email recovery, chuyển lớp/refund hoặc24/7.

## 2. Cấu trúc đầu ra — checkpoint lịch sử, xem bản cuối ở trên

| Phần rà thiết kế | Phần còn phải có trước nghiệm thu E04 |
| --- | --- |
| Gói1: API chung | Phần đã trình được duyệt ADR-139; chi tiết bổ sung ở mục3 còn rà trong artifact. |
| Auth/Administration | Endpoint/session/restricted scope/CSRF/password/reset/lock/audit/canonical replay và privacy contract, giữ policy đã chốt. |
| Students/Admissions/Finance | Endpoint/DTO/rights/guard/expected versions/transaction/effects/errors cho W01–W12; SP/UDF callers và safe result handling. |
| Academics | W13–W19 config/Class/placement/Schedule/Attendance/Score/whole-Class review; complete cohort and batch contracts. |
| Reports/frontend/integration | W20/W21 projections/exact sums/query snapshots/CSV; React routing/server-state/forms/auth-expiry; repository SP/UDF signatures and EF trigger writes. |
| Runtime/verification | DB principals/grants/timeouts/digest/blocklist/capacity, startup/HTTPS/tunnel/backup boundaries, trace/API/SQL/browser/race/security proof objectives. |

Đây là cách chia thảo luận, không tạo checkbox thứ hai hoặc giới hạn số lượt
cần để hoàn thiện. OWNER_REVIEW giữ duy nhất cổng nghiệm thu E04. Các phần sau
mục3 còn đang hoàn thiện; mục4–19 Auth2a/Admin2b/Students3a/Student Admissions3b/Admissions-Finance3c/
ca thi-Teacher4a/cấu hình4b/Class-xếp lớp4c/lịch4d/điểm danh-điểm nháp4e,
whole-Class grades4f/reports5/frontend6/SQL7/runbook8/verification9 đã trình được duyệt;
không coi bảng này là API endpoint inventory hoàn chỉnh.

Theo yêu cầu owner01/10/2026, các gói còn lại4f/5/6/7/8/9 được trình trong
[E04_REMAINING_REVIEW E04-R1](../plan/E04_REMAINING_REVIEW.md):6 gói/23 mục
đề xuất có ô duyệt/ý kiến. Đây là ngoại lệ được owner yêu cầu cho review từng
gói; owner đã tích đủ23 mục và xác nhận đồng ý toàn bộ, ghi ADR-150–ADR-155.
Phần đã trình được đồng bộ ở mục14–19; OWNER_REVIEW vẫn giữ cổng nghiệm thu
E04/E05 duy nhất. Không suy duyệt chi tiết tương lai hoặc cấp quyền code.

### 2.1. Checkpoint tổng hợp ngày02/10/2026 — chưa nghiệm thu

Phụ lục [E04_CONTRACT_CATALOG E04-C1](E04_CONTRACT_CATALOG.md) là registry
154 method/path/55 UUID commands/6 caller-result paths,32 F frontend route
proposals và21 W→37 NV→API/service/E03/cases trace. [E04_DTO_SPEC E04-D10](E04_DTO_SPEC.md)
có154/154 schema drafts API-001–154 ngày03/10/2026: giữ149 D9, thêm5 Reports/CSV.
Không thiếu schema miền, SC-01 vẫn cần rà whole-registry parity, chưa nghiệm thu.
Role-safe current-state/snapshot/filter/time/whole-set totals/decimal38/distinct/
LESSON-vs-exam/FINALIZED-no-data/fresh bounded CSV rõ; report options/columns/
footer/bytes/header/error refinements draft; next SC-02 action/digest/outcomes,
SC02–04-v1/parity đã có; RF01/02/04 Accepted ADR-157–159, resolutions ở
[questions](../plan/questions.md). RF03 root/weekly/move-destination future
cutoff được duyệt03/10/2026, ADR-156; không nghiệm thu toàn bộ D10/E04/E05.
Replay bytes/SQL/grants/runtime còn SC-01–04.
Đây là kiểm cấu trúc/tiến độ tài liệu, không claim complete API/test pass.
URL/DTO/error details mới phải rà trong bản E04 thực tế; OWNER_REVIEW giữ
cổng chưa tích, accepted E01–E03/ADR-001–155 không thay.

## 3. Gói1 — lựa chọn đã trình được duyệt ADR-139

Phạm vi đã duyệt và giới hạn các chi tiết bổ sung được ghi ở mục20.

### 3.1. Version trong DTO, không thêm ETag/If-Match song song

Đề xuất response có version dạng Base64 của rowversion cho aggregate mutable
cần form edit. Client coi là chuỗi opaque, không thời gian hoặc số để tăng/
so sánh. Request mutation có expectedVersions theo các aggregate của endpoint;
profile edit phải gửi cả account/profile tokens như E03 đã chốt. JSON DTO dùng
camelCase, không trả row_version/raw EF entity. Danh mục keys/tokens cụ thể của
từng endpoint sẽ được rà ở các gói tương ứng, không yêu cầu mọi read có token.

Kiểm đúng scope trước khi so token; token không cấp quyền. Thiếu/sai định dạng
ở endpoint yêu cầu token là400; token stale là409, không overwrite/merge.
Base64 phải decode đúng8 byte. Response thành công trả projection/token mới
trong scope được phép; không tự thêm field, Grade DRAFT hoặc hash/session secrets.
E03 parent-version touches giữ nguyên; aggregate/child/cohort tokens và set
membership checks cần được nêu rõ cho batch/whole-Class paths sau này.

Lý do chọn: DTO dễ biểu diễn nhiều tokens cho Account/Profile hoặc batch, giữ
409 contract. ETag/If-Match là phương án thay thế nhưng cần thêm cách biểu diễn
multi-aggregate và semantics lỗi; không chọn chạy đồng thời hai cơ chế. Token
trong DTO vẫn cần mapper/validator và test, không thay resource/state locks.
Nguồn: [Microsoft rowversion](https://learn.microsoft.com/en-us/sql/t-sql/data-types/rowversion-transact-sql?view=sql-server-ver16)
mô tả binary8 bytes và không phải date/time. Encoding Base64 là đề xuất wire-format.

### 3.2. request_id đi qua header Idempotency-Key

Đề xuất các critical command có replay contract dùng header Idempotency-Key
chứa UUID cho một intent, ánh xạ request_id trong ReplayOperation. Actor/action
do verified session/endpoint xác định; không tin client-declared actor/role.
Header không là credential, CSRF token, operation_id hoặc bypass quyền.
GET/search read-only không tạo replay mutation marker; endpoint command bắt
buộc/không dùng header sẽ được ghi rõ ở inventory, không ép login/password
input vào ReplayOperation hoặc tự giả định mọi POST là một mutation.

Retry cùng intent giữ UUID và cùng payload/expected versions; sửa dữ liệu để
thực hiện một intent mới tạo UUID mới sau khi xử lý kết quả cũ. Same key/content
không có effect mới, different content409. Digest/canonical DTO serialization
và runtime timeout sẽ được chốt riêng; không đưa credential/CSRF vào digest.

Đối với completed replay, lookup/digest check dưới request-key gate rồi recheck
current session/role/ownership và trả safe references; không bắt token/state
cũ phải hợp lệ như một mutation mới hoặc chạy lại effects. Response dựng đúng
scope hiện tại, không snapshot PII/secret. Cấp/reset chỉ completed marker,
không trả mật khẩu tạm ở replay; unknown result không auto retry/reset để lấy secret.
Không lưu UUID cùng nháp nhạy cảm trong storage bền hoặc tự retry command.

Lý do chọn: header tách metadata chống lặp khỏi business DTO; body request_id
cũng khả thi nhưng lặp trong nhiều DTO. Chọn một nguồn header, đổi tên/key vị
trí không đổi replay policy đã duyệt. E04 phải rà proxy/CORS/client-wrapper
forwarding và kiểm không body/header duplicate hoặc logging payload.

### 3.3. Search có thông tin cá nhân dùng POST body

Đề xuất lọc không định danh như page/pageSize/sort/state/Course/Class/date có
thể ở GET query theo scope đã chốt. Tìm theo tên/email/SĐT và filter có PII
dùng POST vào search endpoint với DTO body; không ghi ở path/query/history/
browser durable storage. Tên route cuối chưa chốt; body và projection phải
allowlist, không phát query tùy ý hoặc thêm quyền export/drill-down.

POST search là read-only query: không state mutation/audit-mutation/replay
marker; vẫn qua auth/ownership/CSRF pipeline thích hợp, privacy/cache/log/rate
guards. POST/body không tự đảm bảo bí mật: không log raw payload, không cache
PII bền và vẫn HTTPS theo security/operational design sẽ hoàn thiện. Tìm kiếm
do user thao tác giữ idle-activity policy; không dùng background polling để gia hạn.

Lý do chọn: cụ thể hóa FE01/02/03 đã cấm PII trong URL/storage; dùng GET cho
mọi search ngắn hơn nhưng đưa filter nhạy cảm vào URL. Dùng POST cho mọi list
cũng có thể làm được nhưng không cần thay các GET lọc không định danh. Đánh
đổi là search body/context cần giữ trong state tạm, không share bằng URL/cache.

## 4. Gói2a — API xác thực và phiên đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-140**;
chi tiết bổ sung chưa trình phải rà trong E04 hoàn chỉnh (mục20).
Tách Auth/Administration thành2a
(xác thực/phiên) rồi2b (Admin tài khoản/reset/audit) để rà đúng phần liên quan,
không hỏi lại policies ADR-048–060/092/097–100/114. Đăng ký Student W01/F02
sẽ có DTO/effects riêng trong gói Students; mục này không hoàn tất toàn bộ W01.

### 4.1. Năm endpoint Auth, một context không chứa JWT

Mọi route dưới /api/v1. Đề xuất contract:

| Method/path | Input → kết quả | Guard/effects |
| --- | --- | --- |
| GET /auth/csrf | Không business input → requestToken | Bootstrap anonymous hoặc identity hợp lệ hiện tại; không đọc hồ sơ, không cấp/gia hạn AuthSession. |
| POST /auth/login | email/password → AuthContext, đặt JWT cookie | CSRF/Origin; kiểm hash/account/throttle đã chốt; tạo NORMAL hoặc PASSWORD_CHANGE session. Lỗi chưa xác thực chung;429/Retry-After giữ ADR-100. |
| GET /auth/me | Cookie → AuthContext | Validate session/account/version/deadlines;401 nếu không hợp lệ. PASSWORD_CHANGE được đọc context tối thiểu cho màn hình đổi mật khẩu, không nghiệp vụ. |
| POST /auth/change-password | currentPassword/newPassword/expectedVersions.account →204 | NORMAL hoặc PASSWORD_CHANGE; token stale409, sai input400; đổi hash/security_version, hủy mọi phiên và audit bắt buộc cùng transaction. Xóa cookie hiện tại; đăng nhập lại, không nâng phiên. |
| POST /auth/logout | Không business input →204 | Hủy phiên hiện tại ở server rồi xóa cookie; audit security best-effort không cản logout. Phiên đã mất/hết hạn trả401, frontend chuyển trạng thái đã đăng xuất; không tạo session/replay mới. |

AuthContext đề xuất chỉ có accountId/fullName/email/role/scope/accountVersion/
serverTime/absoluteExpiresAt/effectiveExpiresAt: role một giá trị cố định,
accountVersion là opaque rowversion để đổi mật khẩu, không security_version.
effectiveExpiresAt là deadline server đã tính theo absolute/idle/temp expiry;
thời gian hiển thị không là quyền gia hạn hoặc bảo đảm phiên vẫn còn hiệu lực.
Không JWT/raw sid/hash/throttle internals/session list. Context không thêm
quyền xem hồ sơ hoặc dữ liệu nghiệp vụ cho restricted session.
Login/GET me cùng projection; UI routing theo scope/role, backend luôn kiểm lại.
Bootstrap/me/refresh context không tính idle activity và không chạy heartbeat.

### 4.2. CSRF built-in, token chỉ ở RAM và lấy lại khi đổi identity

Đề xuất dùng ASP.NET Core IAntiforgery token-pair thay tự viết cơ chế token.
Cookie antiforgery host-only/HttpOnly/Secure/SameSite=Strict; request token trả
JSON từ GET /auth/csrf và chỉ giữ working memory, gửi header X-CSRF-TOKEN.
JWT cookie riêng tiếp tục ADR-099, không JavaScript-readable auth cookie.
Client lấy token trước login/public mutation; sau login hoặc logout lấy lại
token theo identity hiện tại trước request tiếp theo. Đây là CSRF token,
không refresh token JWT, không kéo dài phiên.

Mọi POST mutation/login và POST search đều kiểm token + Origin same-origin.
GET bootstrap không trả account context. Không CORS cross-origin hoặc tin
Origin/forwarded host tùy ý; trusted proxy/origin cấu hình ở gói runtime.
Auth/context/token responses dùng Cache-Control: no-store, không log raw body/
cookie/header secret; không browser storage bền. Token/Origin không hợp lệ
trả403 code CSRF_INVALID theo error envelope; client lấy context/token mới và
yêu cầu user gửi lại chủ đích, không tự retry mutation. Invalid session vẫn401.
Tabs đổi identity phải lấy lại context/token, không dùng token để chọn account.
JSON endpoints phải gọi validation rõ ràng, không giả định defaults/form
middleware tự bảo vệ JSON. Thứ tự middleware/validation còn phải kiểm ở bản
E04 tổng hợp.

Lý do chọn: tận dụng cơ chế framework thay custom CSRF, giữ cookie bảo mật
và một auth context tối thiểu. Token request qua JSON/RAM thay cookie đọc được
bởi JavaScript giảm nơi frontend phải lấy token; tốn bootstrap/refetch khi
đổi identity. [Microsoft antiforgery](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)
mô tả token-pair/header và lấy lại token sau xác thực; các route/header/cache/
projection cụ thể ở đây là đề xuất của dự án, không do tài liệu tự quyết định.

### 4.3. Không đưa mật khẩu vào replay; ranh giới kiểm chứng

Login/change-password/logout không dùng ReplayOperation/Idempotency-Key hoặc
digest chứa credential. Change-password atomic theo W02/TX13, không cần lấy
lại secret qua replay; mất response thì không tự gửi lại, kiểm tra bằng một
lần đăng nhập chủ đích với mật khẩu mới hoặc Admin reset theo policy đã chốt.
Không lưu current/new password để retry; password policy/blocklist giữ nguyên.
Điều này không bỏ request_id cho các command tạo/reset tài khoản của Admin:
contract safe marker/first secret response thuộc Gói2b sau.

Mục tiêu kiểm chứng sau triển khai: login private/429; NORMAL/restricted/
expiry/revocation/race; me/CSRF không kéo dài idle; thay identity giữa tab;
token thiếu/sai/Origin khác bị chặn; đổi mật khẩu stale không overwrite, hủy
mọi phiên và audit atomic; logout audit lỗi vẫn hủy; unknown response không
auto retry/secret cache. Chưa chạy test hoặc cài ASP.NET/package.

## 5. Gói2b — Administration đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-141**;
chi tiết bổ sung chưa trình cần rà cùng artifact E04 hoàn chỉnh (mục20).
Phạm vi NV33–35/F32–33, phần staff
của W01, W03/W21 và TX01/TX13; không thay policies/quyền/FE18–19 đã chốt.
Mọi route dưới /api/v1; chỉ Admin NORMAL hợp lệ. DTO không nhận actor/role
của người thao tác, security_version, hash hoặc plaintext credential do Admin
chọn. Account đích của reset/trạng thái có thể Student hoặc nhân sự theo E01.

### 5.1. Endpoint đọc và command tách theo ý định

| Method/path | DTO/response đề xuất | Service/guard/effects |
| --- | --- | --- |
| POST /admin/accounts/search | name/email/phone nếu cần + role/status/page/pageSize → trang AccountSummary | Query có PII trong body; default20/max100, sort ID ổn định. Không profile/grades/finance. |
| GET /admin/accounts/{id} | ID → AccountAdminDetail/version | Chỉ thông tin quản trị: id/name/email/phone/role/status/createdAt/mustChangePassword/tempExpiry/version và allowedActions/disabledReasons. Không DOB/address/business records/hash/raw session. |
| POST /admin/staff-accounts | fullName/email/role/phone tùy chọn → first credential result | Role một trong6 role nhân sự hiện có, không Student; Account ACTIVE/temp32chars/24h/hash/audit/replay cùng TX01. Không create-on-behalf Student/edit role hoặc staff contact. |
| POST /admin/accounts/{id}/reset-password | expectedVersions.account → first credential result | TX13: reset hash/temp/security_version/revoke all/audit/replay; giữ trạng thái target, không unlock/restore. |
| POST /admin/accounts/{id}/lock, /unlock, /disable, /restore | expectedVersions.account → AccountAdminDetail mới/operationId | Mỗi path command riêng; không arbitrary status PATCH. Lock ACTIVE→LOCKED, unlock LOCKED→ACTIVE, disable ACTIVE hoặc LOCKED→DISABLED, restore DISABLED→ACTIVE theo state guards; stale/state conflict409. Không mandatory reason mới. |
| GET /admin/command-results/{requestId}?action=... | UUID + action trong allowlist Admin → safe completed result hoặc chưa xác nhận | Chỉ marker của verified Admin đang gọi/action cụ thể; không endpoint xem mọi replay của người khác. Mục5.2 quy định absence/expiry/secret. |
| GET /admin/audit-events | fromDate/toDate/actorId/action/entityType/entityId/result/page/pageSize → event page | Bộ lọc cấu trúc đã chốt, newest occurredAt DESC/id DESC; không name/email/free-payload search. |
| GET /admin/audit-events/{id} | ID → event detail allowlist | Time/actor/action/target/result/reason/old-new/operationId/IP nếu còn giữ hợp lệ; không raw JSON secret/full request-response. |
| GET /admin/operations/{operationId}/audit-events | operation UUID/page/pageSize → event page | Cùng operation, vẫn từng event/phân trang/newest-first; không drill-down nghiệp vụ ngoài quyền Admin. |

Role codes là7 giá trị đã chốt, resolve Role FK từ seed, không hardcode SQL ID.
AccountSummary là subset các field quản trị ở detail, không mở quyền danh sách
hồ sơ học viên. Query target không tồn tại/ngoài scope404; role sai403, invalid/
missing versions hoặc input400, conflict409; error envelope chung không SQL/raw
payload. CSRF/Origin cho POST kể cả search, no-store và không log PII/secret.
AllowedActions chỉ để UI; Service phải reload/recheck ở commit.

Lock/disable chặn self/last ACTIVE Admin dưới ADMINSET gate và Account/session
guards cùng transaction. Unlock/restore không phục hồi phiên cũ; state/reset
không đổi Teacher assignment/Class/Registration hoặc xóa lịch sử. Các commands
liên quan session/security/audit/replay không commit tách DbContext/connection.
Transition routing là đề xuất mechanics để hiện thực E01, không thêm state.

### 5.2. Version, safe completion lookup và credential handoff

Đề xuất bắt buộc Idempotency-Key UUID cho staff-create/reset/4 state commands.
Create không có target RV; reset/state gửi expectedVersions.account. Actor/
canonical action do server xác định. Digest từ validated nonsecret DTO, target/
expectedVersions; generator output không vào digest/audit/replay. Gate/version/
unique/last-Admin guards giữ E03; canonical serialization/timeouts rà gói chung
sau. Completion marker commit cùng effects/audit; lỗi rollback không marker.

First staff-create response201, first reset200: accountId/email/role/
temporaryPassword/temporaryPasswordExpiresAt/operationId/accountVersion.
Chỉ dựng sau commit; FE18 hiện một lần/copy chủ động/ack đóng/clear state giữ
nguyên. Không endpoint GET để lấy lại mật khẩu, không secret trong Location/
URL/cache/log/replay. Replay staff-create/reset trả200 với outcome COMPLETED,
operationId/target reference và resultMode SECRET_RESULT_UNAVAILABLE, không
tạo/hash credential mới. State commands trả200 SAFE_REFERENCE, dựng Account
projection/token theo quyền hiện tại, không replay snapshot PII cũ.

Lookup read-only dùng action allowlist cố định cho6 commands của gói này,
requestId và actor từ session. Completion response chỉ outcome/operationId/
resultMode/target accountId được phép, không plaintext hoặc dữ liệu người khác.
Không marker hoặc đã cleanup trả404 code RESULT_NOT_CONFIRMED: **không là
bằng chứng command thất bại**, có thể chưa commit, hết cửa sổ≥24h hoặc không
thuộc actor/action. UI không tự create/reset; đọc Account/audit có quyền rồi
Admin quyết định tiếp. Một reset mới phải là intent/UUID mới, version mới và
xác nhận chủ đích; không tự reset để khôi phục secret. Lookup không heartbeat
hoặc polling gia hạn phiên; rule user-initiated business activity giữ ADR-097.

### 5.3. Audit projection, alternatives và ca kiểm chứng

Audit DTO structured allowlist theo action, không dump details_json; actor NULL
hiển thị Chưa xác thực, IP hết hạn Không còn lưu. Retention90 ngày/security
best-effort/mandatory atomic giữ nguyên; application không delete/undo/export.
Command result lookup không thay audit search hoặc mở quyền xem credential.

Lý do chọn: các command paths làm rõ khác biệt reset/khóa/mở/khôi phục thay
một PATCH nhận tùy ý status/fields; nhiều endpoint hơn nhưng dễ giới hạn quyền/
DTO/effects. Lookup marker read-only hỗ trợ xử lý timeout hơn chỉ đoán từ
Account đang thay đổi; đánh đổi phải giải thích kết quả chưa xác nhận, không
coi nó là chứng minh thất bại. Không chọn lưu full response để replay vì secret
và PII đã bị cấm. Audit giữ từng sự kiện thay group pagination mới.

Mục tiêu sau triển khai:6staff roles, duplicate email/phone, no Student/admin
role edit; stale reset/state; two Admin last-Admin/self-lock/race revoke;
mandatory audit rollback all; first secret/secret-free replay/key-content409;
actor-scoped lookup/unknown404/cleanup; audit ordered paging/operation scope/
expired IP/no exports. Đây là thiết kế kiểm chứng, chưa API/SQL/browser tests.

## 6. Gói3a — Student API cơ bản đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-142**;
chi tiết bổ sung chưa trình cần rà trong E04 hoàn chỉnh (mục20).
Bao phủ NV01 phần tự đăng ký/W01,
W04/F02–04 và menu hồ sơ cá nhân. Registration/booking/confirmation/Invoice/
lịch/grade chi tiết tiếp ở gói theo miền tương ứng; không mở thêm quyền từ
home card. Giữ quy tắc E01/E02/E03 và ADR-019/073–077/083–085/091/099/121.
Mọi route dưới /api/v1; trừ tự đăng ký, chỉ Student NORMAL hợp lệ.

### 6.1. Sáu endpoint và DTO riêng theo màn hình

| Method/path | Input → projection | Guard/transaction/source |
| --- | --- | --- |
| POST /auth/register | fullName/dateOfBirth/email/phone/password/address tùy chọn →201, outcome CREATED | Người chưa đăng nhập tự đăng ký; server cố định Student/ACTIVE, không role/status/accountId client. Account+StudentProfile+mandatory account audit cùng W01/TX01. Không tự login hoặc cấp AuthSession; thành công chuyển F01. |
| GET /students/me/profile | Không studentId input → own StudentProfileView | accountId/studentId/fullName/email/dateOfBirth/phone/address + versions.account/profile; resolve chủ từ verified session, không danh sách/profile người khác. |
| PUT /students/me/profile | fullName/dateOfBirth/phone/address + expectedVersions.account/profile → profile mới/tokens | Save form4 field trong một transaction W04; kiểm cả2 RV dù chỉ đổi một phía. Email/role/status/password/business fields không được gửi để sửa. Recheck ownership/session/unique phone; mandatory audit và Account/Profile commit cùng nhau. Không tăng security_version/revoke chỉ vì sửa liên hệ. |
| GET /students/me/home | page/pageSize → trang thẻ own Registration | Card theo FE11: khóa/trạng thái/current step/next-action hoặc wait reason, event gần nhất, own unpaid/overdue Invoice và FINALIZED link. Không điểm chưa công bố hoặc dữ liệu Student khác; không business mutation/automation. |
| GET /students/me/courses | page/pageSize → trang CourseSummary ACTIVE | Danh mục để chọn đăng ký, gồm id/code/name/tuition/status; code ASC/id ASC,20/max100. Giá dạng chuỗi VND chính xác, không JS numeric tính tiền. |
| GET /students/me/courses/{id} | Course ID → CourseDetail ACTIVE trong scope chọn khóa | Cùng catalog fields được phép, không config nội bộ/Teacher roster/Student khác. INACTIVE/không tồn tại404 ở route chọn mới; tên/thông tin Course đã tham chiếu vẫn giữ trong own Registration/history projections. |

DTO names/JSON mapping cụ thể là đề xuất wire contract, không EF entity hoặc
schema đổi mới. DateOfBirth date YYYY-MM-DD, validation theo ngày VN; mọi
normalization/Unicode/code points/password exactness giữ E01/ADR-091.
PUT là lưu đủ4 editable fields, address blank→NULL theo quy tắc đã chốt;
không JSON Patch tùy ý hoặc thiếu field thành lệnh xóa ngoài form. Version
thiếu/sai400, stale409 không merge; phone/email unique cạnh tranh409 với
lỗi phù hợp, không lộ Account người khác. Unknown properties không dùng để
mass assignment; schema field allowlist phải được kiểm ở artifact E04 cuối.

### 6.2. Kết quả đăng ký/hồ sơ không rõ và ranh giới quyền

Đề xuất registration/profile save không bắt buộc Idempotency-Key trong gói
này: registration chứa password, không lưu credential digest vào T23; signup
unique email/phone và transaction chặn tạo trùng. Profile save dùng2 version,
không append effect tài chính/booking; không tạo ReplayOperation chỉ để lấy
lại form PII. Không tự retry cả hai. Mất signup response thì không báo chắc
thất bại/tự tạo lại: người dùng chủ đích thử đăng nhập bằng thông tin vừa tạo,
hoặc liên hệ Admin theo reset policy đã chốt. Không thêm query email tồn tại/
recovery endpoint. Mất profile response thì GET profile rồi đối chiếu thay đổi
đang có trong RAM trước khi lưu chủ đích tiếp; không đoán timeout rollback.

Signup201 chỉ outcome/message không echo password/profile hoặc tự đăng nhập.
UI xóa password sau success/rời form; không storage/cache/log/body digest.
CSRF/Origin áp cả POST register/PUT profile; no-store cho auth/PII response,
parameterized persistence và privacy-safe error/log giữ policies đã chốt.
400 invalid field,401 invalid session,403 role/scope/CSRF,404 ngoài read scope,
409 stale/state/unique; không raw SQL/entity, không phone/email search qua URL.

Endpoint /me lấy Student từ verified session, không tin client account/student
selector. Owner đã CONFIRMED vẫn sửa4 contact fields; không sửa khóa/Band/
score/Invoice/Payment hoặc staff sửa thay. Course catalogue là read model
Academics, Students home là own projections phối hợp Admissions/Finance/
Academics trong cùng backend; không gateway/microservice/DB riêng.
Các read/home links không tự cấp quyền; mutation/detail sau luôn kiểm lại
ownership/state tại request/commit và dùng current DTO/preview.

### 6.3. Home/read models, alternatives và kiểm chứng

Home cards paginated20/max100 theo registration mới nhất/ID tie-breaker;
không ép chung một progress enum/điểm tiến độ cho nhiều khóa. Wait reason/
next action codes chỉ trình bày guard hiện có, không quyền mới hoặc state mới.
Empty state chọn khóa, no inbox/notifications/staff dashboard. Không background
polling gia hạn idle; giữ filter/page/sort không PII trong URL theo FE02.
NEXT_ACTION/ref link mapping giữa React routes và endpoint sẽ rà gói frontend.
Invoice money/overdue và FINALIZED status backend quyết định, không tính lại
tiền/điểm trên client; home không dùng làm proof giá/chỗ/state vẫn còn hợp lệ.

Lý do chọn: routes /me ngắn và tránh client chọn nhầm chủ profile; DTO tách
profile/home/catalogue thay endpoint khổng lồ trả mọi dữ liệu. Đánh đổi là có
vài request riêng khi chuyển màn hình và các link phải revalidate. Một endpoint
GET /students/{id} đa quyền hoặc full EF entity dễ lẫn personal/business scopes;
không chọn mở API Student toàn trung tâm ở gói này. Hai RV giữ atomic edit,
không cần cache form PII trong replay hoặc tự retry tạo tài khoản.

Mục tiêu sau triển khai: không chọn role khi signup, Account/Profile/audit
all-or-none, unique race, password không trim/digest/log; profile2 tokens/
stale/phone unique/confirmed-edit/no email edit; other role/restricted không
đọc own profile/home/catalogue; multi-registration cards đúng quyền/missing/
FINALIZED; inactive course không chọn mới nhưng own history không mất;
exact tuition/no durable draft/unknown response không tự retry. Chưa chạy tests.

## 7. Gói3b — Student Registration/thi đầu vào/xác nhận đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-143**;
chi tiết bổ sung chưa trình cần rà trong E04 hoàn chỉnh (mục20).
Phạm vi NV02–05/F05–09, W05/W07/W09
và own calendar F07. Chỉ Student NORMAL/own Registration; không employee
đăng ký/đặt thay, Teacher grading hoặc Admissions approval ở gói này.
Giữ max2 logical attempts/CLOSED guards/rebook cùng lượt/state transitions/
best-result và confirmation policies E01/E02/E03; không hỏi lại các quyết định đó.
Prefix P = /api/v1/students/me; rId là Registration ID, bId là Booking ID.

### 7.1. Endpoint inventory đề xuất theo intent

| Method/path sau P | Input/projection | Service/transaction/effects |
| --- | --- | --- |
| GET /registrations | courseId/status/page/pageSize → own RegistrationSummary page | Tất cả lịch sử của mình, mới nhất createdAt DESC/id DESC; no Student selector/PII search. |
| POST /registrations | courseId →201 Registration mới/operationId | Resolve Student từ session; Course ACTIVE và unfinished predicate/filtered unique recheck. W05: PENDING, không copy Band/score; audit/replay cùng transaction. |
| GET /registrations/{rId} | ID → own RegistrationDetail/version/allowedActions | Course/state/current entry/Band/confirmation/rejection reason/Invoice/Class links theo scope Student; không raw entity/unpublished training grades. |
| POST /registrations/{rId}/cancellation-preview | Không business input → eligible/effects/guard manifest | Preview read-only W05: các future booking bị hủy, own Invoice UNPAID nếu có, phần lịch sử giữ lại và reason nếu bị chặn; không giữ chỗ/mutate/audit-mutation/replay. |
| POST /registrations/{rId}/cancel | expectedVersions/expectedSet từ preview →200 detail/operationId | Student chỉ trước CONFIRMED, không mandatory Student reason mới. Registration/unfinished/dependent future booking releases/own UNPAID Invoice nếu có/required audit/replay all-or-none theo W05/TX06, giữ started/result/history. |
| GET /registrations/{rId}/entry-test-options | fromDate/toDate/page/pageSize → attempt context/eligible session page | Server xác định lượt được đặt và scope; ca chọn OPEN/chưa start/chỗ hiện tại/time-room/effectiveStatus/version, không roster/contact người khác. |
| POST /registrations/{rId}/entry-test-bookings | targetSessionId/attemptContext/expectedVersions →201 own Booking/operationId | W07/TX02: resolve hoặc tạo đúng logical attempt, capacity/Student conflicts/one-current-booking; booking đầu tiên PENDING→TESTING, lượt2 giữ BAND_ASSIGNED. |
| POST /registrations/{rId}/entry-test-bookings/{bId}/cancel | expectedVersions →200 updated own entry context/operationId | Chỉ nguồn OPEN/trước start/chưa CONFIRMED. Giữ CANCELLED history/release slot/audit/replay cùng TX02; không suy ABSENT hoặc xóa attempt. |
| POST /registrations/{rId}/entry-test-bookings/{bId}/reschedule | targetSessionId/expectedVersions →200 old-new Booking refs/operationId | Hủy old Booking + tạo new Booking cùng attempt trong một TX02, không overwrite session FK/không two-step partial. Source/destination guards/time/conflict đều recheck. |
| GET /entry-test-bookings | fromDate/toDate/view current-history/registrationId tùy chọn/page/pageSize → own calendar page | Own bookings nhiều khóa/lượt, UTC instants và status/history đúng nghĩa; filter Registration cũng phải own. Không read→ghi CLOSED hoặc auto activity polling. |
| GET /registrations/{rId}/entry-results | ID → own attempt results/current best/Band | Score chỉ COMPLETED; thiếu/vắng không0, tie attempt1, không edit/sửa/chấm bởi Student. Trả versions và guard context cần cho confirmation. |
| POST /registrations/{rId}/confirmation-preview | Không business input → eligible/Band/attempt context/guard manifest | Read-only W09; chỉ BAND_ASSIGNED/Band hợp lệ/no pending retake, kể cả ca CLOSED. Không giữ chỗ hoặc cấp quyền vượt guard. |
| POST /registrations/{rId}/confirm | expectedVersions/expectedSet từ preview →200 CONFIRMED/server time/operationId | W09: recheck result/best/Band/booking under Registration gates/session guard, audit/replay cùng transaction. Không Invoice/ADMITTED/PAID/Class. |
| GET /command-results/{requestId}?action=... | Caller-bound Student UUID/action allowlist → safe outcome/ref | Chỉ6 mutation actions của gói này/actor đang gọi; absence404 RESULT_NOT_CONFIRMED không chứng minh thất bại, giống pattern2b nhưng không quyền Admin. |

GET lists20/max100 và ID tie-breaker; date filters theo ngày VN/UTC conversion
đã chốt, window/sort allowlist/mapping cụ thể còn rà final E04. Preview POST
read-only vẫn CSRF/Origin, không replay marker. Role/session/ownership kiểm
trước projection/token,404 nếu target không phải của Student; không lộ hồ sơ
khác qua lồng rId/bId hoặc action errors.

### 7.2. Guard manifest/version cho form và preview — không thêm bảng

Đề xuất expectedVersions map Base64 theo các object liên quan, expectedSet
là IDs/presence client đã thấy. Manifest chỉ là optimistic context, không
credential, signed authorization hoặc reservation; server kiểm lại mọi thứ.
Không lưu preview entity/cache PII hoặc phát quyền từ client-declared state.

| Intent | Version/context tối thiểu đề xuất |
| --- | --- |
| Create Registration | courseId; không RV của row mới. Reload/guard Course ACTIVE và unfinished predicate tại commit, không tin giá/state client. |
| Book/rebook | Registration RV + target Session RV + attemptContext từ options (attempt ID hoặc NULL/lượt1–2 do server xác định; Attempt RV nếu có) + expected absence current booking. Không cho client chọn tùy ý lượt3. |
| Cancel/reschedule Booking | Registration/Attempt/current Booking/source Session RV; reschedule thêm target Session RV. Cross-check Booking thuộc rId/attempt của mình. |
| Cancel Registration | Registration RV + exact set/versions các future bookings/source sessions trong preview; own Invoice ID/version hoặc absence nếu áp dụng. Compare effect membership/time tại commit. Chỉ bỏ phụ thuộc theo W05, không client chọn subset/paid cancellation. |
| Confirm Registration | Registration/BandAssignment RV, exact IDs/versions của logical attempts và current SCHEDULED bookings, best-attempt reference từ preview. Recompute live guard, không chỉ tin parent RV. |

Registration RV không được giả định tự đổi khi mọi child booking/result thay
đổi; E03 không hứa token parent bao trùm chúng. Session RV bảo vệ edit/state
nhưng **không thay capacity recount**, vì không booked_count cache và time
vẫn trôi khi RV không đổi. Current-booking absence/attempt membership phải
recheck dưới E03 locks/unique. Missing version/context ở endpoint yêu cầu400,
stale token/set/state/capacity/conflict409; không merge/auto đổi ca/chọn lượt.

Preview hết hợp lệ nếu input/effect membership/Band/result/guard thay đổi.
Ví dụ future booking đi qua start: preview hủy cũ phải409/rà lại effects,
không âm thầm hủy booking đã bắt đầu. Cancellation đặc biệt của Registration
vẫn giữ policy hủy future booking kể cả CLOSED, khác Student cancel Booking
phải nguồn OPEN. Confirmation blocked bởi lượt2 đang chờ không được bypass
bằng chỉ gửi RV Registration/Band. Account/Student ownership/session guards
giữ đến commit, actor/time/operationId do server; client không gửi score/Band/
isUnfinished/status/confirmation time để ghi.

### 7.3. Replay/unknown response và SQL integration boundaries

Đề xuất bắt buộc Idempotency-Key UUID cho6 commands create/cancel Registration,
book/cancel/reschedule Booking và confirm Registration. Nonsecret validated
DTO/target/expected versions-set làm digest theo shared canonical contract sẽ
rà sau; không lưu response PII hoặc JWT/CSRF trong marker. Completed replay
lookup/digest rồi current rights/scopes trước stale checks của mutation mới;
trả200 safe current projection/ref/operationId, không effect mới. Khác content
cùng key409; retry do user chủ đích giữ đúng key/content, không tự gửi lại.

Mất response thì GET object hoặc caller-scoped command result; không thấy
marker chưa chứng minh rollback (đang commit/expired/outside actor-action).
Không tạo key mới hoặc tự book/reschedule tiếp khi chưa xác định intent cũ.
Nếu server state đã đổi thì tải context/preview mới và user quyết định intent
mới; không overwrite, auto chọn ca hoặc tự thêm lượt. Marker≥24h; hết hạn
vẫn domain unique/state/history guards, không hứa dedup vô hạn.

Admission booking occupancy/cancellation dùng timetable/Registration/request
gates và parent rows theo E03§9.1, cùng DbContext/connection/transaction với
mandatory audit/replay. Booking creation/reschedule/cancel TX02, Registration
cancel TX06; create/confirm theo save units W05/W09, không tự đổi bảng TX để
tạo implementation plan. Query effective session state dùng FN01 theo
asOfUtc có tham số, repository phải gọi UDF thật; SessionOptions scopes/time/
availableSlots là service/repository query. Không gọi roster SP cho Student
hoặc thêm SP quyền rộng chỉ để đủ rubric. Signatures/EF mapping/timeout/DTO
decimal exactness sẽ rà integration/runtime cuối E04, chưa SQL/code.

### 7.4. Alternatives/verification của gói chưa duyệt

Tách command + read-only preview/options tốt hơn generic PATCH Registration/
Booking vì giữ Course/attempt/session links bất biến và toàn bộ effects.
Đánh đổi thêm request và nhiều token/set khi preview; không chọn server-side
preview table/reservation vì không cần state mới. Reschedule atomic endpoint
tốt hơn UI gọi cancel rồi create, tránh mất old slot khi create mới lỗi.
Read results/calendar riêng ngăn detail khổng lồ và mọi session roster bị lộ.

Mục tiêu sau triển khai: own object binding/invalid parent-child404; duplicate
unfinished, last seat/overlap, boundary OPEN→CLOSED/start; ABSENT/rebook same
attempt/max2; first booking PENDING→TESTING, second không xoá Band; stale
manifest/new child/time effect409, atomic reschedule/cancel/audit rollback;
confirmation pending-retake/teacher correction race/no invoice; real FN01
caller, safe replay/key mismatch/unknown result/privacy. Chưa chạy tests.

## 8. Gói3c — Admissions/Invoice/thu tiền ngoài ứng dụng đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-144**;
chi tiết bổ sung chưa trình cần rà cùng artifact E04 hoàn chỉnh (mục20).
Phạm vi NV06/09/12/27/28/37,
F10/F13–14/F28–29, W10–12 và TX04–06; báo cáo/CSV NV29–32/F30–31 bàn riêng.
Giữ quyền ADR-083, financial snapshot/one Payment/reasons/external collection/
retention/replay E01/E02/E03; không payment gateway/partial/refund hoặc role mới.
Mọi route dưới /api/v1; role đúng và session NORMAL, scope server quyết định.

### 8.1. Endpoint/DTO theo vai trò — không shared entity response

| Method/path | Input/projection | Service/guard/transaction |
| --- | --- | --- |
| POST /admissions/registrations/search | name/email/phone/courseId/status/valid date filters/page/sort → AdmissionsRegistrationPage | NV09 mọi state gồm CANCELLED/REJECTED, current contact/Course/entry-Band/confirm-approval/Invoice amount-status/Class; no DOB/address/Payment/training data/export. SP01 caller với verified actor/scope từ Service. |
| GET /admissions/registrations/{id} | ID → AdmissionsRegistrationDetail/versions/allowedActions | Scope cùng NV09, không proxy GET Finance detail hoặc sửa Student thay. |
| POST /admissions/registrations/{id}/approval-preview | Không business input → guard context/tuition/expected versions | Eligible CONFIRMED/Band, current Course tuition dương và expected absence Invoice; chưa phát số Invoice/issue time hoặc giữ chỗ. No new requirement Course ACTIVE đối với hồ sơ đang có. |
| POST /admissions/registrations/{id}/approve | expectedVersions/expectedSet từ preview →200 ADMITTED/InvoiceSummary/operationId | TX04/W10–11: snapshot Course price server tại issuance, allocate number SP07, Invoice UNPAID + Registration/audit/replay cùng commit; không client amount/number/due/issuedAt. |
| POST /admissions/registrations/{id}/reject | reason/expectedVersions.registration →200 REJECTED/detail/operationId | Chỉ CONFIRMED, reason bắt buộc theo E01; Student chủ hồ sơ xem được. Audit/replay all-or-none; không reject ADMITTED hoặc sửa entry score. |
| POST /admissions/registrations/{id}/cancellation-preview | Không business input → eligible/dependent effects/version-set | CONFIRMED hoặc ADMITTED/Invoice UNPAID, future bookings/Invoice bị hủy và started history giữ lại; no Payment method/reference disclosure. |
| POST /admissions/registrations/{id}/cancel | reason/expectedVersions/expectedSet →200 CANCELLED/detail/operationId | TX06: Registration/unfinished/UNPAID Invoice/future bookings/mandatory audit/replay atomic; no paid cancellation/refund/started-booking deletion. |
| GET /admissions/command-results/{requestId}?action=... | UUID/action allowlist3 → own caller completion/ref | Pattern Gói2b/3b: no result không proof thất bại, current rights và Admissions projection only, không Finance/PII response snapshot. |
| GET /students/me/invoices | own registrationId/status/page/pageSize → InvoiceStudentPage | Student-only/own records, gồm lịch sử; no Payment fields/other owners, no new search name/email. |
| GET /students/me/invoices/{id} | ID → InvoiceStudentDetail/print data | Own current name/Course/number/snapshot amount/issue/due/state/overdue/payment guidance; no method/reference/confirmedBy. Fresh authorized GET trước browser print, không PDF API. |
| POST /finance/invoices/search | invoiceNumber/courseId/name/email/phone/status/issueFromDate/issueToDate/overdueOnly/page/sort → InvoiceAccountantPage | Accountant NV27 mọi state, current contact/Invoice/Payment metadata/confirmer/time; no entry/training/profile dump/print/export directory. PII và invoice-number search trong body. |
| GET /finance/invoices/{id} | ID → InvoiceAccountantDetail/versions/allowedActions | Invoice+Payment projection đúng grain/consistency, no ROW entity/raw sensitive session info. |
| POST /finance/invoices/{id}/collection-preview | Không business input → eligible/exact amount/version context | UNPAID/Registration hợp lệ/no Payment; preview chỉ một confirmation context, không tự ghi nhận tiền/giữ trạng thái. |
| POST /finance/invoices/{id}/collect | amount/method/transactionReference/expectedVersions/expectedSet →201 Payment/PAID/detail/operationId | TX05: actual externally reconciled amount phải bằng Invoice snapshot, CASH/TRANSFER, transfer cần reference; Payment/Invoice PAID/audit/replay cùng commit. Server paidAt/confirmer, no backdate/partial/gateway verification. |
| PUT /finance/payments/{id}/metadata | method/transactionReference/reason/expectedVersions.payment-invoice →200 Payment/detail/operationId | NV28 correction giữ same Payment/amount/paidAt/confirmer/link; method/ref only, reason bắt buộc, old-new audit/replay atomic. Không client sửa Invoice hoặc tạo Payment thứ2. |
| GET /finance/command-results/{requestId}?action=... | UUID/action allowlist2 → own caller completion/ref | Collection/metadata correction only, no others' result hoặc thêm audit/export rights. |

Search/body/no-store/log/privacy/CSRF-Origin cho POST/PUT giữ shared contract;
list20/max100/stable ID tie-breaker, issue-date filter khác revenue date report.
DTO allowlist theo role: Admissions không nhận Payment/DOB/address/Attendance/
training grades, Student Invoice không nhận Payment, Accountant không nhận
đầu vào/đào tạo. Search/detail/preview/replay đều dùng cùng scope, không chỉ
ẩn field trên frontend. Roles còn lại không được gọi các routes gói này.

### 8.2. Versions/effect manifest và transaction boundary

| Command | Context optimistic đề xuất | Mandatory live checks |
| --- | --- | --- |
| Approve | Registration/Course/BandAssignment RV + Invoice absence từ preview | CONFIRMED/Band, Course current positive tuition, Invoice unique/absence; current price change→409/repreview, không silent dùng giá khác. |
| Reject | Registration RV + reason | CONFIRMED/not concurrent approve/cancel, reason valid; giữ entry history. |
| Cancel Registration (Admissions) | Registration RV/own Invoice RV hoặc absence + exact future booking/source-session IDs/RVs từ preview | State allowed, Invoice UNPAID/no Payment, time/effect set unchanged; recheck all under guards vs collection/start. |
| Collect | Registration/Invoice RV + Payment absence từ preview | ADMITTED/valid Registration, UNPAID/positive equal amount, one Payment; cancelled/paid/stale→409. |
| Correct Payment metadata | Payment/Invoice RV + reason/new metadata | Parent Invoice PAID and same Payment, no amount/paidAt/link/confirmer change; no bank-reference uniqueness rule. |

Thiếu/sai required versions/context/input400, stale token/set/state/uniqueness/
capacity race409; permission/ownership401/403/404 theo shared policy. Money
positive VND integer digit string/decimal18 input, không round hoặc JavaScript
numeric để tính lại; mismatch submitted amount400 field error nếu context còn
hợp lệ. Reference trim đầu/cuối/≤100 code points, CASH optional blank→NULL,
TRANSFER nonempty; reason1–1000 theo E01. Không sửa issuedAt/dueDate/number/
snapshot amount hoặc tạo status OVERDUE. Clock server quyết định issued/paid
instant; due=ngày issue VN+7, quá hạn UNPAID due<trước ngày VN hiện tại.

Preview không cấp Invoice number hoặc hứa thời điểm/hạn thu đã cố định;
issue time/due thực được server tính tại commit. Giá Course trong preview có
RV, chưa snapshot Invoice; sau issuance giá mới không đổi Invoice cũ.
Course INACTIVE không tự hủy/chặn hồ sơ đang có hoặc thay snapshot. Cancellation
manifest compare kể cả time/effect membership vì RV không đổi khi clock qua
start; mismatch repreview, không âm thầm hủy started Booking.

Approval orchestration liên Admissions/Finance trong cùng backend/DB connection/
transaction; SP07 nhận issue year server từ issue time VN, không auto approve/
commit riêng. Cancellation giữ timetable/Registration/Invoice request gates;
collection/correction cùng Registration/Invoice gates khi workflow yêu cầu,
giữ Account/session guards tới commit. Xác định/sort gates trước DML/reload
theo E03§9.1, không service/repository mở transaction riêng cho audit/replay.
Audit lỗi mandatory rollback entire save unit, không success giả/partial Invoice.

### 8.3. Replay/unknown response/read consistency và SQL callers

Đề xuất bắt buộc Idempotency-Key UUID cho5 commands approve/reject/cancel/
collect/correct-metadata. Preview/search/read không replay marker. Same intent
giữ key/validated nonsecret content/expected versions-set; different content
same key409. Completed replay không chạy SP07/thu tiền/correction lần2; dựng
safe current response theo role sau session/current-scope checks. Marker chứa
IDs/outcome/operationId, không profile/payment response snapshot hoặc secret.

Timeout đọc Registration/Invoice/Payment đúng scope hoặc caller result trước
khi user quyết định intent mới. Caller lookup chỉ actor/action đã xác thực
cho role tương ứng; no marker404 RESULT_NOT_CONFIRMED không proof rollback.
Không auto retry/gateway check/giả đã hoặc chưa thu; reference không là khóa
dedup hoặc bằng chứng ngân hàng. Replay expiry≥24h vẫn Invoice/Payment unique
và state guards; không hứa khôi phục PII snapshot cũ/đồng bộ nhiều trình duyệt.

SP01 thực sự phục vụ Admissions search; actor không nhận từ client, inputs
parameterized/sort allowlist. Invoice reads dùng VW06 grain một Invoice/tối đa
một Payment với projections riêng; FN06 tính overdue từ status/due/today_vn,
không app tự cập nhật status hoặc sinh phạt. Đề xuất một transaction SNAPSHOT
read-only cho Invoice/Payment detail và compound financial preview; ưu tiên
một relational projection, nhưng không giả định single statement dưới locking
READ COMMITTED là point-in-time snapshot. Không thay write transactions/gates.
Nguồn: [Microsoft isolation levels](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql?view=sql-server-ver16).
Version-store cost và concurrency cần kiểm tại môi trường thực tế. Exact
EF/SP/TVF signatures/mapping/settings còn rà integration/runtime cuối E04.
SP04/report snapshots/CSV chưa đặc tả trong gói này, không tự thêm export.

Student print: fresh own Invoice GET thành công rồi dựng A4 đúng status; fetch
lỗi/mất quyền thì chặn in bản cũ như mới. UNPAID không xác nhận thu, CANCELLED
ghi hủy, PAID đúng state. Không PDF endpoint/receipt/stamp/secret hoặc staff
print privilege. Payment guidance chỉ presentation, không bank integration.

### 8.4. Alternatives và kiểm chứng của đề xuất

Chọn DTO/routes theo role và lệnh explicit thay generic Invoice/Account CRUD,
tránh API dùng chung vô tình lộ Payment cho Admissions/Student hoặc cho sửa
amount/due. Đánh đổi nhiều DTO/requests cần test parity. Single approval
transaction tốt hơn UI gọi approve rồi invoice-create, vì không để ADMITTED
thiếu Invoice. Preview+RV/manifest thêm bước mạng nhưng kiểm được tuition và
dependent cancellation; một confirmation UI cho mỗi command, không tạo thêm
double confirmation hoặc approval role mới.

Mục tiêu sau triển khai: projection parity list/detail/preview/replay đúng3
roles; SP01/SP07/FN06 actual calls; Course-price/approval race/allocator fail/
duplicate approve rollback; collect-vs-cancel only valid outcome; metadata
stale/reason/old-new audit giữ revenue time/amount; clock due boundary/exact
VND/overdue; replay key-content mismatch/expired marker/unknown outcome;
Student fresh print/status/no Payment leakage/no export/personal search URL.
Đây là thiết kế kiểm chứng, chưa SQL/migration/API/browser tests.

## 9. Gói4a — ca thi đầu vào, phân công và Teacher chấm thi đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-145**;
chi tiết bổ sung chưa trình cần rà trong E04 hoàn chỉnh (mục20).
Phạm vi NV14–15/NV21–22/NV36,
F16–17/F25, W06/W08; module Admissions phục vụ AcademicAffairs và Teacher.
Tên module không cấp quyền cho role Admissions. Chỉ session NORMAL hợp lệ,
server kiểm current assignment/ownership; kế thừa E01/E02/E03, không biểu
quyết lại số lượt, time cutoffs, score/Band, role hoặc catalogue policy.
Cấu hình Course/program/Band, Class và timetable đào tạo tiếp ở gói4b trở đi.

### 9.1. Endpoint/DTO đề xuất theo nhiệm vụ

Mọi path dưới /api/v1. sId là Session, bId là Booking, tId là Attempt.

| Method/path | Input/projection | Service/guard/effect |
| --- | --- | --- |
| GET /admissions/entry-test-sessions | VN date range/effective status/graderId/page/sort → AcademicSessionPage | AcademicAffairs; effective state/FN01, time-room/grader availability warning, capacity/active occupancy. Không đọc Account entity/secret hoặc directory Student. |
| POST /admissions/entry-test-sessions | VN date/startTime/endTime/room/capacity/graderId →201 AcademicSessionDetail | AcademicAffairs tạo CLOSED, capacity1–20, same-day/minute/start<end, Teacher ACTIVE/resource overlap live checks; server IDs/time/audit/replay. |
| GET /admissions/entry-test-sessions/{sId} | ID → AcademicSessionDetail/versions/allowedActions | AcademicAffairs; read-only effective state/history/count, không read-persist CLOSED. Grader unavailable giữ assignment/cảnh báo, không auto đổi người. |
| PUT /admissions/entry-test-sessions/{sId}/schedule | VN date/startTime/endTime/room/expectedVersions.session →200 detail | Date/time/room edit chỉ trước start và chưa từng có Booking, kể cả Booking đã hủy. Không generic PUT status/grader/capacity/result hoặc phá lịch hẹn cũ. |
| POST /admissions/entry-test-sessions/{sId}/open | expectedVersions.session →200 detail | Mở/reopen CLOSED trước start, resources/ACTIVE grader/occupancy≤capacity; không mở lại CANCELLED/started. Full vẫn OPEN, không trạng thái FULL. |
| POST /admissions/entry-test-sessions/{sId}/close | expectedVersions.session →200 detail | Đóng OPEN trước start, giữ mọi Booking; không cấm Teacher chấm/vắng/correction khi guard đạt. |
| POST /admissions/entry-test-sessions/{sId}/cancellation-preview | Không business input → affected Booking set/versions | AcademicAffairs, ca chưa hủy/pre-start; liệt kê đúng SCHEDULED bookings sẽ hủy, lịch sử giữ và Student tự đặt lại cùng lượt. Không giữ chỗ/đổi Booking trong preview. |
| POST /admissions/entry-test-sessions/{sId}/cancel | reason/expectedVersions/expectedSet →200 cancelled detail/operationId | Session CANCELLED + đúng active Booking cancellations/provenance/audit/replay trong một TX; giữ Attempt/Registration/score/Band history, không auto đặt lại/đăng ký thay. |
| GET /admissions/entry-test-grader-options | VN date/time/optional sessionId → minimal current/eligible Teacher options | AcademicAffairs tạo ca hoặc đổi grader; accountId/displayName/availability, không email/phone/hash/profile directory hoặc PII của lịch xung đột. Server rechecks ACTIVE/overlap khi commit, sessionId không là quyền bỏ qua conflict khác. |
| POST /admissions/entry-test-sessions/{sId}/grader-change-preview | graderId → old/new grader/context/conflicts/versions | AcademicAffairs; pre-start và chưa mọi score/ABSENT, giữ Bookings; preview không reservation. Candidate conflict chỉ scope-safe summary. |
| POST /admissions/entry-test-sessions/{sId}/change-grader | graderId/reason/expectedVersions/expectedSet →200 detail/operationId | Đúng cửa sổ, Teacher mới ACTIVE/no overlap; giữ booking/ghi old-new-actor-time-reason/audit/replay atomic. Không đổi sau start/chấm/vắng để né correction locks. |
| POST /admissions/entry-test-sessions/{sId}/roster/search | name/email/phone/booking-state/page/sort → AcademicRosterPage | AcademicAffairs; SP02 trusted actor, current/history Booking + Student current contact/Course/entry results/Band; Invoice status only nếu cần theo scope, không amount/Payment/DOB/address/training drafts. |
| GET /admissions/command-results/{requestId}?action=... | UUID/action allowlist mở rộng6 Session commands → caller result/ref | AcademicAffairs role/scope riêng, không nhận completion của Admissions approval hoặc Teacher chỉ vì cùng namespace. |
| GET /admissions/teachers/me/entry-test-sessions | VN date/effective status/page/sort → TeacherSessionPage | Teacher hiện assigned; không ca người khác, no Account details. Past ca chỉ trong current assignment, không suy quyền vĩnh viễn từ từng chấm. |
| GET /admissions/teachers/me/entry-test-sessions/{sId} | ID → TeacherSessionDetail | Current grader check cho detail như list; time/room/status/task counts, no session management hoặc finance. |
| POST /admissions/teachers/me/entry-test-sessions/{sId}/roster/search | name/email/phone/state/page/sort → TeacherRosterPage | SP02 caller; internal Student ID/current contact/Course/entry data thuộc nhiệm vụ, không DOB/address/Invoice/Payment/global directory. |
| GET /admissions/teachers/me/entry-test-bookings/{bId} | ID → TeacherEntryTaskDetail/versions/related context | Current assigned Session, correct Booking/Attempt/Registration join; history/score/provenance/Band/actions, cả read và replay cùng task projection. |
| POST /admissions/teachers/me/entry-test-bookings/{bId}/score | score/expectedVersions/expectedSet →200 completed task/operationId | Initial score từ end, SCHEDULED/no Attempt result, valid pre-confirmation Registration/non-CANCELLED Session; TX03 COMPLETED Booking/result/Registration BAND_ASSIGNED/best Band/first-result threshold freeze/audit/replay atomic. |
| POST /admissions/teachers/me/entry-test-bookings/{bId}/absence | expectedVersions/expectedSet →200 absent task/operationId | Từ end, Teacher xác nhận không dự thi, SCHEDULED/uncompleted Attempt/valid Registration; Booking ABSENT/provenance/audit/replay atomic, no score0/new Attempt/auto rebook. Một confirmation UI, không bắt thêm lý do vắng. |
| PUT /admissions/teachers/me/entry-test-attempts/{tId}/score | score/reason/expectedVersions/expectedSet →200 corrected task/operationId | COMPLETED result, original grader vẫn assigned, Registration TESTING/BAND_ASSIGNED; TX03 old-new audit + score/best Band atomic, giữ original graded_by/graded_at/completed Booking; no ABSENT→COMPLETED/grade override. |
| GET /admissions/teachers/me/command-results/{requestId}?action=... | UUID/action allowlist3 → caller-safe completion/ref | Initial score/absence/correction; current assignment vẫn bắt buộc, no result không proof failure, no permanent score-response PII snapshot. |

PII search trong POST body/RAM, không URL/storage/raw logs; CSRF/Origin cho
POST/PUT và no-store cho sensitive data. Read-only search không replay marker.
Lists default20/max100/stable ID tie-breaker; không trả raw EF/SP all columns.
Roster có current/history/filter trạng thái, missing score vẫn NULL/chưa chấm;
Registration đã hủy chưa chấm ghi rõ lịch sử và không thuộc pending actions.
AcademicAffairs không chấm/vắng/correction từ các read routes. Teacher không
create/edit/open/close/cancel/reassign Session, không sửa hồ sơ cá nhân.

### 9.2. Related versions, time và effect-set checks

| Command | Optimistic context đề xuất | Live guard không được thay bằng token |
| --- | --- | --- |
| Create Session | New object chưa RV; graderId/date-room/clock inputs | Account role/ACTIVE và timetable overlap theo EC:TIMETABLE; không tin availability ở options là giữ chỗ. |
| Edit schedule / open / close | Session RV | Time server, relationship/history/occupancy/teacher/resources; edit chưa từng Booking, reopen chưa start, close giữ Booking. |
| Cancel Session | Session RV + exact active Booking IDs/RVs/related Attempt-Registration context từ preview | Exact SCHEDULED set, source session/pre-start/time; người mới đặt sau preview làm stale dù Session RV không touch khi thêm Booking. Không dùng paginated roster làm full manifest. |
| Change grader | Session RV + target grader Account RV + active Booking membership/context | Pre-start/no score/ABSENT, ACTIVE/no overlap/new identity; old→new và giữ Bookings. Account token không thay live status/assignment guard. |
| Initial score / absence | Session/Registration/Attempt/Booking RV; related attempt IDs/RVs và BandAssignment RV/absence khi ảnh hưởng | Session end/non-CANCELLED/current Teacher, correct joins/no result/pre-confirmation; time không phát RV, old Booking đã ABSENT/CANCELLED không actionable. |
| Correct score | Completed Attempt/Booking/Registration/Session RV, BandAssignment RV + exact related attempts | Original/current grader, TESTING/BAND_ASSIGNED, reason/current threshold set; score change phải tính best/Band với cả kết quả, không tin client chọn Band. |

Các RV/set trên là refinement dự kiến để rà artifact, không tự suy thêm
business field/rights hoặc mọi token đều đã được owner duyệt. Server quyết định
best/Band từ current results/thresholds, tie chọn lượt1; initial score không
yêu cầu Teacher nhập Band/threshold version hoặc submit cả roster để chấm một
người. FN02 và BANDSET serialization bảo vệ first-result/config race.
Score0–100/tối đa2 decimals theo ADR-091, invalid precision400 không tự round.
Thiếu/sai required input/token400, stale token/set/state/time boundary409,
unauthorized assignment401/403/404 theo shared policy. Reason chỉ ở hủy/đổi
grader/correction đã chốt; không thêm reason cho open/close/initial score/ABSENT.
Không mở deadline24/48h, cron, extra attempt, post-confirmation correction.

Ca effectively CLOSED từ start, list/detail/options/roster/commands dùng cùng
server as_of_utc; read không mutate, next permitted write persist effective
closure cùng transaction. State CLOSED tự nó không khóa post-end Teacher work.
Preview không đóng băng thời gian, capacity hoặc Bookings; submit rechecks.
Cancel Session khác cancel Registration: chỉ hủy ca/Booking, không hủy hồ sơ
hoặc Invoice, không reset Band/lượt thi hoặc tự đặt ca mới.

### 9.3. Transaction/replay và đường gọi SQL

Đề xuất9 write commands dùng UUID Idempotency-Key: create Session, edit
schedule, open, close, cancel, change grader, initial score, absence và score
correction. Related effects/mandatory audit/replay completion cùng transaction,
không frontend cancel-then-loop Bookings hoặc ghi score-then-call Band API.
Same key/content no new effect; different content409; completed replay kiểm
current rights/assignment và dựng scope-safe refs/current DTO, không áp token
cũ như new mutation. Unknown lookup caller/action, no marker không proof
rollback; không auto chấm/vắng/reassign/retry để đoán kết quả.

Service→Repository dùng cùng DbContext connection/TX cho EF writes/UDF và
audit/replay. W06 dùng request/EC:TIMETABLE/related REG gates, reload Session/
Booking contexts/current Account theo E03§9.1; initial score/correction dùng
EC:REG/BANDSET và TX03, absence giữ related state/Registration guard; request
gate và session/account guard tới commit. Tập gate sorted trước DML, discovered
set khác thì rollback/reprepare; không khóa rời khiến cancel/confirm/score
cùng commit kết quả trái invariant hoặc audit độc lập.

SP02 thực sự phục vụ Teacher/AcademicAffairs roster, trusted actor do service
cung cấp; SQL vẫn kiểm current assignment theo role/scope, không actor từ body.
VW02 grain Booking, tránh join hai attempts làm score trùng; FN01 effective
closure với server instant; FN02 real call tính Band khi initial/correction.
TG03 bảo vệ first-use Band threshold set; service vẫn sở hữu permission,
validation/serialization/audit, trigger không tạo audit hoặc API permission.
Compound roster/detail/preview lấy coherent read context (đề xuất read-only
SNAPSHOT như financial context), nhưng write reload/checks theo transaction
gates, không giữ read snapshot qua user confirmation. Không thay E03 setting
hoặc thực thi SQL ở đây. Exact SP projection/filter/pagination/UDF/EF trigger
mapping và DB grants/timeouts phải rà ở integration/runtime E04.

### 9.4. Alternatives và ca kiểm chứng thiết kế

Khuyến nghị routes/DTO theo nhiệm vụ + explicit commands thay generic Session
CRUD/status PATCH hoặc chung staff roster; giữ quyền current grader, cancellation
effects và absence vs score rõ. Đánh đổi nhiều DTO/token/preview/API tests.
Single-person grade commands thay bulk toàn roster tránh vô tình chấm/vắng
người chưa nhập; không thay whole-Class training-grade policy bàn gói sau.
Preview + exact effect set thêm lượt mạng nhưng thấy đầy đủ Booking bị hủy;
không reservation/auto reassignment/auto retry khi stale.

Sau triển khai cần kiểm:7 roles/no Teacher finance/Academic no score override;
SP02/VW02/FN01/FN02 actual calls/projection parity; clock start-end/effective
CLOSED read-only/full OPEN; historical canceled Booking blocks schedule edit;
last-slot/close-vs-book/cancel-vs-book/confirm-vs-correction/Band first-use races;
all-or-none Session cancellation/audit failure/replay; ABSENT no score0/same
logical attempt rebook; tied-best/correction original grader/metadata giữ;
blocked/reassigned Teacher mất list/detail/replay access; timeouts/caller scopes/
same-key mismatch/marker expiry/no automatic effects. Đây là mục tiêu kiểm
chứng thiết kế, chưa code/SQL/API/browser/concurrency tests.

## 10. Gói4b — Course, chương trình và cấu hình điểm đã trình được duyệt

Trạng thái: **phần đã trình được owner duyệt01/10/2026, ADR-146**;
chi tiết bổ sung chưa trình cần rà trong E04 hoàn chỉnh (mục20).
Phạm vi NV13/F18/W13/T05–08,
module Academics, chỉ AcademicAffairs NORMAL. Giữ Course code bất biến,
Course-price/Invoice snapshot, Program first-use freeze, seeded Band thresholds,
unique Course-Band và cơ chế audit/replay E01/E02/E03. Không re-vote các rules.
Gói4b chưa bao gồm Class/xếp lớp/lịch/Attendance/whole-Class grades.

### 10.1. API/DTO theo aggregate, không CRUD từng component từ frontend

Mọi path dưới /api/v1; cId là Course, pId là TrainingProgram.

| Method/path | Input/projection | Guard/effect |
| --- | --- | --- |
| GET /academics/courses | code/name/status/page/sort → AcademicCoursePage | Catalogue query không PII; gồm ACTIVE/INACTIVE, default20/max100/stable ID. Không dùng Student ACTIVE-only DTO cho quản lý. |
| POST /academics/courses | code/name/basePrice/status ACTIVE-or-INACTIVE →201 CourseDetail/operationId | Manual normalized unique immutable code, name1–150, positive exact VND digit string; no default0/free course, server ID/audit/replay. |
| GET /academics/courses/{cId} | ID → CourseDetail/version/allowedActions | AcademicAffairs quản lý giá danh mục, không Invoice/Payment/profiles/Student directory. |
| PUT /academics/courses/{cId}/details | name/basePrice/expectedVersions.course →200 detail/operationId | Name/price save atomic, no code/status/FKs in this DTO. Giá mới chỉ ảnh hưởng Invoice phát hành sau; no issued-Invoice rewrite. |
| POST /academics/courses/{cId}/deactivate | expectedVersions.course →200 INACTIVE/detail/operationId | Lệnh riêng, confirmation scope rõ; chặn Registration mới, không cascade hủy hồ sơ/lớp/program hoặc đổi snapshot tiền. |
| POST /academics/courses/{cId}/activate | expectedVersions.course →200 ACTIVE/detail/operationId | Restore cùng record/code, không xóa lịch sử/Program freeze hoặc đăng ký thay Student. |
| GET /academics/bands | Không business input → BandReferenceSet | Seed Band1–5/min_score từ DB, freeze indicator/versions nếu cần; không hardcode ngưỡng trong thuật toán. Không grant edit/override từ read route. |
| GET /academics/training-programs | courseId/bandId/name/status/page/sort → ProgramPage | Include DRAFT/ACTIVE/INACTIVE theo AcademicAffairs; relation/expected LESSON count/derived isConfigurationFrozen, không Student/Teacher draft grades. |
| POST /academics/training-programs | courseId/bandId/name/expectedLessonCount/components[] →201 ProgramDetail/operationId | Tạo DRAFT, unique Course-Band kể cả INACTIVE; all structurally valid components cùng TX, no duplicate program/version để né freeze. |
| GET /academics/training-programs/{pId} | ID → ProgramDetail/components/versions/allowedActions | Coherent full configuration, component IDs/RVs và membership context; isConfigurationFrozen suy từ bất kỳ Class kể cả CANCELLED, không là flag quyền do client gửi. |
| PUT /academics/training-programs/{pId}/configuration | name/courseId/bandId/expectedLessonCount/components[]/expectedVersions/expectedSet →200 detail/operationId | Chỉ DRAFT chưa Class/dependencies. Lưu toàn bộ cấu hình/thành phần và old-new audit/replay cùng TX; giữ component IDs cũ, IDs mới server cấp, không loop API mỗi component. |
| PUT /academics/training-programs/{pId}/name | name/expectedVersions.program →200 detail/operationId | Rename riêng, kể cả config đã dùng khi quyền hợp lệ; không đưa components/count/course/Band vào DTO hoặc mở freeze. |
| POST /academics/training-programs/{pId}/activation-preview | Không business input → validation summary/versions/component set | Read-only kiểm cấu hình đã lưu, sum100/one FINAL_EXAM/count/relations; chưa chuyển ACTIVE hoặc khóa form. Chặn activate khi còn sửa chưa lưu trong UI. |
| POST /academics/training-programs/{pId}/activate | expectedVersions/expectedSet →200 ACTIVE/detail/operationId | DRAFT activation hoặc INACTIVE restoration; recheck all rules/current components, không tự sửa weights hoặc bỏ freeze sau restore. |
| POST /academics/training-programs/{pId}/return-to-draft | expectedVersions.program →200 DRAFT/detail/operationId | ACTIVE chưa có Class, explicit command trước config edit; không tự hạ trạng thái từ một PUT, không mở used config. |
| POST /academics/training-programs/{pId}/deactivate | expectedVersions.program →200 INACTIVE/detail/operationId | ACTIVE→INACTIVE; confirmation không tự dừng lớp đang dạy/chấm, không chuyển GradeReport hoặc Student state. |
| GET /academics/command-results/{requestId}?action=... | UUID/action allowlist10 → caller completion/ref | Course4 + Program6 commands, actor/role/action verified; no result không proof failure, no sensitive snapshot/shared cross-role result. |

Đề xuất10 write commands UUID: Course create/details/deactivate/activate;
Program create/configuration/name/activate/return-to-draft/deactivate.
Reads/activation-preview không replay marker hoặc mutation. DTO không nhận
actor/isFrozen/version tự tăng/status ở config PUT hoặc raw EF entity.
AcademicAffairs không nhận Invoice/Payment từ catalogue detail; catalogue
price là quyền cấu hình NV13, không quyền đọc số tiền Invoice của học viên.
Routes các role khác vẫn dùng own/scoped projections đã có ở gói riêng.

Band read giữ ngưỡng seed đã chốt 0/25/45/65/85 và trạng thái khóa từ kết quả
đầu tiên. Gói này không đề xuất editor đổi seed/thêm-xóa mức Band, max_score
hoặc nút mở khóa; mọi thay đổi phạm vi Band sau này cần owner yêu cầu rõ,
không suy quyền mutation từ bảng có rowversion hoặc trigger. Đây là giới hạn
API được đề xuất, không sửa Band table/SQL hay seed đã nghiệm thu.

### 10.2. Đơn vị lưu, token và cửa sổ cấu hình

| Save unit | Related versions/set đề xuất | Server live guards |
| --- | --- | --- |
| Create Course/Program | New rows không RV; Course/Band RV khi form tham chiếu quan hệ | Unique normalized code/Course-Band, existence/relationships/valid values; duplicate409, không auto tạo code mới/restore record khác. |
| Course details/state | Course RV | Preserve code/price bounds/lifecycle và issued snapshots; serialize price read của issuance bằng Course row guards, không chỉ kiểm RV lúc GET. |
| Program configuration | Program RV + exact existing component IDs/RVs + related Course/Band context | DRAFT/no Class/no dependent data, final component membership and unique names; unknown component ID/cross-program ID không được dùng. |
| Program name/state | Program RV; activation thêm exact component IDs/RVs + related Course/Band RV | Rename không mở protected fields; return-to-draft no retained Class; activation/recovery complete valid config/relations/freeze kept. |

Program parent RV không thay cho child membership/RVs; sửa component không
được tự giả định touch parent nếu chưa thống nhất mapping. Configuration PUT
mang toàn bộ tập đích với IDs cũ, component mới chưa ID; chỉ bỏ component nháp
chưa có phụ thuộc trong đúng save unit/rights hiện có. Không dedicated blanket
DELETE Course/Program/Band/component hoặc history purge. Server validates cả
tập trước ghi; một component stale/lỗi thì rollback toàn save unit và audit.
Exact token keys/absence/set/digest/codes còn rà trong artifact tổng hợp.

Draft có thể chưa đủ total100 hoặc FINAL_EXAM, nhưng từng trường/weight/kind/
name/count luôn hợp lệ và unique, tối đa một FINAL_EXAM theo E03 constraints.
Expected LESSON count luôn int dương, không để0 như placeholder. Activation
bắt buộc đúng100 và đúng một FINAL_EXAM; server dùng decimal, không normalize
weights, auto thêm exam hoặc silent round excess precision. Components names
CI accent-sensitive unique per program, kinds GENERAL/LESSON/FINAL_EXAM.
LESSON component không tự tạo Score mỗi buổi hoặc binding chung cho cả lớp.

Program cấu hình chỉ sửa khi DRAFT/chưa dùng. Đã dùng: name và state hợp lệ
vẫn quản lý được, nhưng Course/Band/list/component name/kind/weights/count
đều frozen. Không route restore nào xóa first-use history; lớp CANCELLED vẫn
khóa. Return-to-draft không đảo used configuration, INACTIVE không tự tạo quyền
edit. Course INACTIVE không tự chặn quy trình của Registration cũ hoặc cascade
Program/Class; không suy thêm Course ACTIVE guard ngoài E01/E02 đã chốt.

Thiếu/sai input/token400/fieldErrors, stale/member-set/unique/state/freeze
conflict409 theo envelope; no overwrite/merge/auto retry. isFrozen/allowedActions
ở GET là thông tin trình bày, không quyền tới commit. UI một confirmation cho
state/activation có đối tượng/ảnh hưởng, không thêm mandatory reason khi rules
chưa yêu cầu. Lưu explicit/no autosave; load fail không dùng form cũ như mới.

### 10.3. Transaction, SQL enforcement và unknown result

Một Service operation dùng chung DbContext connection/TX cho config DML,
mandatory old-new allowlist audit và ReplayOperation completion. W13/first-Class
use dùng EC:PROGRAM theo E03§9.1/TX07; unique Course-Band là backstop cho tạo/
đổi cặp. First Class phải dùng cùng program gate như edits/return-to-draft.
Account/session guard tới commit, request key gate trước business gates;
related resource tập thay đổi khi reload thì rollback/reprepare theo thứ tự
khóa thống nhất. Course-price edit/issuance chia row guard; không audit độc lập.
Không đòi EC:TIMETABLE cho rename không ảnh hưởng occupancy.

TG01 backstop protected Program links/count/delete sau Class, không chặn name/
status hợp lệ chỉ vì used; TG02 bảo vệ used component list/name/weight/kind.
TG03 vẫn bảo vệ seed/freeze Band và phối hợp BANDSET khi first entry result;
không thêm web rights hoặc biến trigger thành audit. EF trigger/OUTPUT mapping,
rename/set update order với unique constraints và SP/UDF/config read signatures
phải kiểm ở integration E04; không hứa một SaveChanges đã xử lý mọi guard.
Program compound read/activation preview đề xuất coherent snapshot không giữ
qua user confirmation; write vẫn reload/recheck sau lấy gates.

Same UUID/content no second mutation; different content409; completed replay
kiểm current caller rights rồi dựng current scope-safe refs/detail, không áp RV
cũ như mutation mới. Unknown đọc catalogue/detail hoặc caller command-result
trước khi quyết định retry chủ đích/intent mới; no marker không proof rollback.
Sau marker expiry vẫn code/pair uniqueness và lifecycle guards, không bảo đảm
khôi phục response snapshot cũ. Không auto sửa configuration/activate để đoán.

### 10.4. Alternatives và kiểm chứng thiết kế

Khuyến nghị whole-configuration save + explicit lifecycle/name commands thay
frontend gọi CRUD từng component rồi đổi status: ít trạng thái nửa cập nhật,
phù hợp form trang riêng. Đánh đổi payload/token set lớn hơn và cần integration
tests cho insert/update/remove/rename unique ordering. Per-field/component API
nhỏ hơn mỗi request nhưng cần thêm orchestration/draft concurrency protocol.
Single name command giữ thao tác hợp lệ sau freeze mà không mở configuration.
Không thêm builder framework/version entity, second approver hoặc package.

Sau triển khai cần kiểm: chỉ AcademicAffairs mutations/all7 role projection;
Course-price-vs-approval snapshot, immutable code/duplicate pair inactive;
configuration atomic failure/one stale child/set-changed rollback; decimal
sum100/one exam/invalid precision/DRAFT incomplete; Class-create-vs-config/
return-to-draft, cancelled Class vẫn freeze; name/status used allowed/no config
bypass; inactive/recovery no auto student/class changes; TG01–03 real behavior/
EF trigger mapping; UUID same-key/different-content/unknown/expiry/caller scope.
Đây là mục tiêu thiết kế, chưa code/SQL/migration/browser/race tests.

## 11. Gói4c — Class, xếp lớp và đọc đúng phạm vi

Trạng thái: **phần đã trình được duyệt ADR-147**; exact routes/DTO/guard-key/
SQL/read-isolation bổ sung chưa trình còn rà bản tổng hợp, không blanket approval.
Phạm vi NV16–18/NV07 và phần Class/roster
NV23; F19–20/F22/F11/F24, W14/W15. Module Academics phối hợp Students/
Admissions/Finance cùng DB, không cấp quyền mới. Kế thừa các guard E01/E02/
E03; timetable mutation/Attendance/Score/whole-Class review bàn gói sau.

### 11.1. Endpoint inventory theo vai trò

Mọi path dưới /api/v1. cId là Class, rId là Registration.

| Method/path | Input/projection | Guard/effect |
| --- | --- | --- |
| GET /academics/classes | classCode/courseId/bandId/programId/teacherId/status/page/sort → AcademicClassPage | AcademicAffairs, mọi state; size derived, program/Teacher/current state, no Grade DRAFT values/finance amount/profile dump. Non-PII query only. |
| POST /academics/classes | classCode/programId/teacherId/maxSize/related versions →201 detail/operationId | AcademicAffairs tạo PLANNED, Teacher hợp lệ, code immutable/unique, max1–15; first-use Program freeze/audit/replay cùng TX07, không tạo GradeReport trước placement. |
| GET /academics/classes/{cId} | ID → AcademicClassDetail/versions/allowedActions | Program/code/teacher/size/lifecycle prerequisites; read không tự chuyển state/HELD, không lộ Teacher DRAFT. |
| PUT /academics/classes/{cId}/capacity | maxSize/expectedVersions.class/expectedSet →200 detail/operationId | Trước ONGOING/actual teaching,1–15 và không dưới placed count; no code/program/status/teacher generic edits. |
| GET /academics/class-teacher-options | optional classId → minimal Teacher options | AcademicAffairs tạo/đổi; ID/name/availability, no credentials/email/phone/global profile directory. Existing schedules dùng kiểm conflicts; không reservation. |
| POST /academics/classes/{cId}/teacher-change-preview | teacherId → old/new/context/conflicts/versions-set | AcademicAffairs, đúng cửa sổ pre-teaching/no Attendance-Score; giữ toàn bộ schedules/assignments, conflict summary đúng scope. |
| POST /academics/classes/{cId}/change-teacher | teacherId/expectedVersions/expectedSet →200 detail/operationId | Live Teacher ACTIVE/role/current schedules overlap check; pre-ONGOING/actual teaching/no related data, audit theo scope đã chốt. Không thêm mandatory reason/history workflow đổi Teacher riêng. |
| POST /academics/classes/{cId}/transition-preview | action OPEN/START/COMPLETE/CANCEL → prerequisites/effects/versions-set | AcademicAffairs; full context không lấy subset từ page roster. Read-only, không giữ chỗ hoặc thay Class/Schedule/Attendance. |
| POST /academics/classes/{cId}/open | expectedVersions/expectedSet →200 OPEN/detail/operationId | PLANNED, Program ACTIVE/Teacher hợp lệ/ít nhất một valid future LESSON; không đòi đủ LESSON ngay khi mở. |
| POST /academics/classes/{cId}/start | expectedVersions/expectedSet →200 ONGOING/detail/operationId | OPEN/actual start không trước first LESSON, ≥1 assignment và đủ expected LESSON schedules; server recheck, không tự theo clock. |
| POST /academics/classes/{cId}/complete | expectedVersions/expectedSet →200 COMPLETED/detail/operationId | ONGOING, mọi required LESSON/FINAL_EXAM HELD và LESSON Attendance đủ; khóa Attendance, không tự submit/finalize grades/finish Registration trước FINALIZED. |
| POST /academics/classes/{cId}/cancel | reason/expectedVersions/expectedSet →200 CANCELLED/detail/operationId | PLANNED/OPEN trống/chưa dạy, no assignment; Class cancellation/resource release/audit/replay atomic, giữ Schedule/history, no refund/paid Registration cancellation. |
| POST /academics/classes/{cId}/roster/search | name/email/phone/page/sort → AcademicRosterPage | AcademicAffairs; current contact/Course/entry-Band/Invoice status only/assignment, no amounts/Payment/DOB/address/Teacher grade drafts. |
| POST /academics/placement-candidates/search | name/email/phone/courseId/bandId/page/sort → CandidatePage | AcademicAffairs F22, ADMITTED/PAID/unassigned candidates; no Student toàn trung tâm, no Payment metadata/money. |
| GET /academics/placement-candidates/{rId} | ID → CandidateDetail/versions | Current contact/Course/Band/Invoice status/placement context; không proxy Finance Invoice detail. |
| GET /academics/placement-candidates/{rId}/classes | page/sort → suitable Class options | OPEN/same Course-Band/available capacity, conflicts/eligibility summary; không tự chọn/xếp hoặc đổi Band, list không giữ chỗ. |
| POST /academics/classes/{cId}/placement-preview | registrationId → candidate/class/conflicts/context/versions-set | One Registration, full live timetable context; no assignment/report/audit-mutation. |
| POST /academics/classes/{cId}/assignments | registrationId/expectedVersions/expectedSet →201 safe assignment/detail/operationId | TX08: OPEN/capacity/same Course-Band/ADMITTED/PAID/unassigned/no Student overlaps; Assignment + DRAFT GradeReport + audit/replay atomic. Không Score/Attendance default, Registration vẫn ADMITTED. |
| GET /academics/command-results/{requestId}?action=... | UUID/action allowlist thêm8 commands → caller completion/ref | AcademicAffairs current rights/role/action, không cấp extra audit/finance/DRAFT visibility. |
| GET /academics/teachers/me/classes | status/courseId/page/sort → TeacherClassPage | Teacher hiện assigned; current/history Class theo current task rights, không quyền vĩnh viễn từ từng dạy. |
| GET /academics/teachers/me/classes/{cId} | ID → TeacherClassDetail | Current assignment guard, Class/program/own duties; no finance/DOB/address/class others. Grade/attendance writes chưa thuộc endpoint này. |
| POST /academics/teachers/me/classes/{cId}/roster/search | name/email/phone/page/sort → TeacherRosterPage | Current Teacher; internal Student ID/current contact/assignment phục vụ lớp, no global directory/finance/profile editing. |
| GET /students/me/classes | current/history/courseId/page/sort → own ClassPage | Own assignment only, Class completed read-only; học nhiều khóa/học lại chọn từng lớp, không peer roster hoặc unfinalized grades. |
| GET /students/me/classes/{cId} | ID → own ClassDetail | Server ownership qua Assignment→Registration→Student; own class/program/Teacher display info/placement, no other Students/contact directory. Lịch/FINALIZED grades tiếp ở gói riêng. |

DTO tách3 roles cho list/detail/preview/replay; search PII POST body/RAM, không
URL/storage/raw logs. POST/PUT kiểm CSRF/Origin; sensitive responses no-store.
Pagination20/max100/stable ID. Teacher mất assignment thì mất detail/roster
access; blocked Teacher không auto đổi người/hủy lớp. Student không self-place.
Một confirmation mỗi command nhạy cảm, lý do chỉ cancel Class đã chốt.
Không remove/transfer Assignment/reopen Class/change program/delete history.

### 11.2. Context optimistic và live guard

Đề xuất8 UUID write commands: create Class/capacity/change Teacher/open/start/
complete/cancel/place one Registration. Effects/mandatory audit/completion cùng
TX, no automatic retry; no marker không proof failure, caller lookup/refetch
rồi quyết định intent chủ đích. Replay dựng safe current DTO sau rights checks,
không trả DRAFT report được tạo nội bộ hoặc snapshot PII.

Class create kiểm Program RV + component membership/config và Teacher Account
context; live relation/config/first-use gate quyết định, không client isFrozen.
Capacity/cancellation/start cần Class RV và exact Assignment membership:
Assignment immutable không RV; insert không tự touch Class RV, không dùng
paginated roster hoặc displayed size làm concurrency proof.

Teacher change/open/start preview cần Program/Class/Teacher context + exact
current Schedule IDs/RVs và Assignment membership. Complete cần đầy đủ current
obligation/Schedule/Attendance/cohort context theo E03; không yêu cầu Teacher
grade values hoặc grade submit/finalized. Attendance child changes không tự
giả định update Class RV. CANCELLED Class schedules bỏ occupancy bằng Class
state, không xóa row hoặc đổi thành HELD.

Placement form mang Registration/BandAssignment/Invoice/Class/Program context,
Assignment absence/Class cohort và relevant current schedules/Student booking-
assignment set. Token/set guards tránh stale preview; server luôn check live
Course-Band/PAID/state/capacity/conflicts dưới timetable/REG/CLASS gates. Không
lộ Payment method/reference qua opaque guard context, không trả Teacher DRAFT.
Exact keys/digest/absence/manifest/read-isolation còn rà artifact, không blanket
approval từ đề xuất nhóm API.

Giờ server không sinh RV; clock qua first LESSON/time cutoff làm preview stale
phải kiểm lại. Invalid input/token400, stale set/state/unique/capacity/conflict409;
401/403/404 shared scope. No silent reselection/merging. Empty suitable list là
đang chờ lớp, không lỗi buộc mở lớp, chuyển Band/ca hoặc refund.

### 11.3. Transaction, SQL paths và alternatives

W14 create dùng TX07/EC:PROGRAM với first-Class config freeze; W15 TX08
Assignment+DRAFT report+mandatory audit/replay cùng connection/TX. Class state/
placement/capacity/Teacher change dùng EC:CLASS, timetable/resource gates khi
ảnh hưởng occupancy, related REG/Invoice guards khi predicate yêu cầu theo
E03§9.1. Account/session guards tới commit, resource set sorted trước DML;
reload làm set khác thì rollback/reprepare, không độc lập audit/replay commits.
Completion serialize với Attendance/occurrence; cancellation serialize với
placement; timetable edits gói sau phải giữ same invariant.

Class counts từ Assignment, joins tránh multiplying Schedule/Attendance.
Program/TG01–02 first-use enforcement thực sự chạy ở create Class; GradeReport
DRAFT internal không có Score hoặc default absence. Các adoption NV/F/TX/SQL
không cần thêm SP chỉ để đủ số lượng; projection/EF trigger mapping/read
snapshot/runtime timeout cần rà integration sau. Chưa thực thi DB/API.

Khuyến nghị one-Registration placement + explicit lifecycle commands thay
generic Class PATCH/bulk auto placement: đơn giản giải thích, preview rõ và
không side effects ngoài lựa chọn. Đánh đổi nhiều routes/contexts/preview calls;
bulk chỉ khi có yêu cầu riêng, không suy từ UI danh sách. Shared entity DTO
ngắn hơn nhưng dễ lộ DRAFT/financial data hoặc task quyền cũ.

Sau triển khai cần kiểm: all7 role/scope3DTO/list-detail-replay; first Class vs
config; two last-slot placements/duplicate Registration; placement vs start/
cancel/capacity; Teacher change vs blocked/old-task access/timetable overlap;
complete vs Attendance/HELD/current replacements; Program inactive existing
workflow; no paid cancellation/refund/transfer; DRAFT internal no leakage/default
rows; lost response/replay expiry/content mismatch/current auth. Đây là thiết kế
kiểm chứng, chưa code/SQL/browser/race tests.

## 12. Gói4d — Schedule, lịch tuần, thay buổi và HELD

Trạng thái: **phần đã trình được duyệt ADR-148**; exact routes/DTO/guard-manifest/
SQL/read-isolation/runtime bổ sung chưa trình còn rà bản tổng hợp, không blanket approval.
Phạm vi NV19/phần lịch NV07/NV23;
F21/F11/F24 và W16, kế thừa ADR-032/035–037/087–088/122–123 và E03 T16/TX09.
Đây là hợp đồng quản lý lịch lớp/thi cuối khóa, không mở lại ca thi đầu vào,
không đổi quyền nhập Attendance/Score hoặc điều kiện hoàn thành Class.

Addendum được owner duyệt03/10/2026 — ADR-156/E04-RF03: root LESSON/
FINAL_EXAM mới, toàn bộ concrete weekly candidates và move destination phải
startAt>serverNow khi kiểm tra lưu trong transaction. Tới/qua start sau input
hợp lệ409, malformed date/time400; clock recheck không chỉ RV/preview.
Một candidate lỗi thì weekly batch không lưu phần nào/không skip/trim/auto
move; không backfill. Existing old-slot/replacement/HELD windows giữ nguyên.
Chỉ RF03 được duyệt riêng, không toàn bộ DTO/mapping hoặc nghiệm thu E04/E05.

### 12.1. Endpoint inventory theo vai trò

Mọi path dưới /api/v1; cId là Class, sId là Schedule. Các DTO calendar chỉ
ngày/giờ/phòng/loại/Class/current Teacher/occurrence và liên kết history đúng
scope; không nhúng roster, Grade DRAFT, financial data hoặc hồ sơ toàn trung tâm.

| Method/path | Input/projection | Guard/effect |
| --- | --- | --- |
| GET /academics/schedules | fromDate/toDate/classId/courseId/teacherId/current-history/page/sort → AcademicCalendarPage | AcademicAffairs; filter khoảng ngày VN, Class CANCELLED/history tách active timetable; đọc không tự chuyển HELD/NOT_HELD. |
| GET /academics/schedules/{sId} | ID → AcademicScheduleDetail/allowedActions/versions | Current/historical occurrence, obligation/replacement refs/người-thời điểm/lý do đúng quyền quản lý, no dependent grade values. |
| GET /academics/classes/{cId}/scheduling-context | ID → Class/program/Teacher/expected-current-HELD counts/current obligations/versions-set | AcademicAffairs; full obligation context độc lập page calendar, không coi subset/filter là toàn bộ lịch lớp. |
| POST /academics/classes/{cId}/schedule-preview | type/date/start/end/room → candidate/count/conflicts/context | LESSON riêng hoặc FINAL_EXAM riêng; kiểm nghĩa vụ/count/order và Class/room/Teacher/assigned Student conflicts, không giữ chỗ. |
| POST /academics/classes/{cId}/schedules | candidate/expectedVersions/expectedSet →201 detail/operationId | AcademicAffairs, live guards; root obligation server cấp, không tin client arbitrary key/current/state. Không tự tạo exam khi thêm LESSON. |
| POST /academics/classes/{cId}/weekly-schedule-preview | from/to weekdays/start/end/room → concrete LESSON plan/count/conflicts/versions-set | Read-only; danh sách ngày cụ thể, đã có/còn thiếu/sắp tạo và mọi conflicts đúng scope. Không tự bỏ ngày nghỉ/lỗi. |
| POST /academics/classes/{cId}/weekly-schedules | same rule/concrete candidate plan/expectedVersions/expectedSet →201 created refs/context/operationId | Toàn bộ concrete LESSON batch hoặc không row nào; server kiểm plan khớp input và recheck, không âm thầm tạo plan khác preview. |
| POST /academics/schedules/{sId}/move-preview | newDate/start/end/room → old-new/conflicts/versions-set | Chỉ pre-start/current/no Attendance-linked Score/Class chưa ended; giữ Class/type/obligation. |
| PUT /academics/schedules/{sId}/time-room | newDate/start/end/room/expectedVersions/expectedSet →200 detail/operationId | Live pre-start guards, sửa cùng row/audit old-new; không delete-create để né giới hạn hoặc tự xóa dependencies. |
| POST /academics/schedules/{sId}/replacement-preview | reason/newDate/start/end/room → old-new/count/conflicts/context | Khi tới/qua start mà cả lớp không tổ chức; current PLANNED/no dependent data/Class chưa ended, new future hợp lệ/current Teacher. |
| POST /academics/schedules/{sId}/replace | reason/newDate/start/end/room/expectedVersions/expectedSet →201 old-new safe refs/operationId | TX09: old NOT_HELD/noncurrent + new PLANNED/current + reason/actor/time/link + mandatory audit/completion atomic, cùng Class/type/obligation. |
| POST /academics/schedules/{sId}/held-preview | ID → occurrence/context/prerequisites/versions | Confirmation context riêng từng buổi; chỉ AcademicAffairs, after scheduled end/Class ONGOING/current PLANNED. Không tự đòi đủ Attendance để xác nhận occurrence. |
| POST /academics/schedules/{sId}/confirm-held | expectedVersions/expectedSet →200 HELD/detail/operationId | Server after-end/ONGOING guard; HELD actor/time/audit/completion cùng TX, không undo hoặc tự điểm danh/chấm điểm/complete Class. |
| GET /academics/command-results/{requestId}?action=... | UUID/action allowlist thêm5 commands → safe caller completion/ref | Current AcademicAffairs rights, no extra history/grade/finance visibility. |
| GET /academics/teachers/me/schedules | date window/classId/current-history/page/sort → TeacherCalendarPage | Chỉ Class đang được phân công hiện tại; không suy quyền vĩnh viễn vì từng dạy, không lấy global resource/student directory. |
| GET /academics/teachers/me/schedules/{sId} | ID → TeacherScheduleDetail | Recheck current Class assignment, own teaching duty/date-room/type/occurrence/replacement refs; không quyền sửa lịch/HELD. |
| GET /students/me/schedules | date window/classId/current-history/page/sort → own CalendarPage | Qua own Assignment, không peer roster/contact hoặc unfinalized grades; lịch ca đầu vào dùng hợp đồng Gói3b riêng. |
| GET /students/me/schedules/{sId} | ID → own ScheduleDetail | Own assigned Class/current-history/replacement display, không reason/actor management payload hoặc global conflicts. |

GET read/search không mutation marker. POST preview kiểm CSRF/Origin nhưng
không cần Idempotency-Key;5 UUID write commands là create-one/create-weekly/
move/replace/confirm-held. DTO calendar/detail tách3 roles, no-store cho dữ liệu
nhạy cảm; pagination20/max100/stable ID, query không chứa PII. API không lấy
page calendar làm proof đủ obligations hay scope của Teacher/Student.

### 12.2. Preview, current obligation và context guards

Frontend dùng lịch tuần/list theo ngày và mobile agenda đã chốt ADR-122;
click detail mở action hợp lệ, không drag-drop/resize/new calendar library.
Weekly form hai bước nhập→preview; sửa input invalidate preview, commit theo
plan đã xem. Preview validation/conflicts không reserve room/Teacher/Student.
Clock vượt cutoff trong lúc user xác nhận vẫn phải bị server kiểm lại.

Một nghĩa vụ chỉ một occurrence hiện hành; replacement chain giữ Class/type/
obligation do server xác định, không fork/đổi loại/đếm history thành extra
LESSON. Root LESSON chỉ lấp số còn thiếu, không vượt expected; FINAL_EXAM
riêng/tối đa một current sau last current LESSON và không bù LESSON thiếu.
Không auto add exam, skip holidays, stored recurrence hoặc tạo lịch nền.
Moves/replacements/weekly additions phải giữ exam ordering theo lịch hiện
hành; không âm thầm dời buổi thi hoặc tạo second exam để hợp lệ hóa request.
Nếu điều chỉnh nhiều buổi, từng command phải giữ invariant, không lưu tạm
trạng thái sai hoặc đề xuất multi-move/bulk remediation ngoài scope này.

Context đề xuất gồm Class/Program/current Teacher Account versions, exact
current obligation/Schedule IDs-RVs và Assignment membership; các predicate
phụ thuộc cần full relevant Student assignments/bookings/resource schedules,
không chỉ Class RV hoặc số buổi hiển thị. Assignment immutable không RV;
child Attendance/Score inserts không tự giả định touch Schedule/Class RV.
Move/replace phải kiểm sự vắng mặt của dữ liệu phụ thuộc ngay trong TX, không
dùng token trên record không tồn tại làm bằng chứng absence. Grade DRAFT
values và Student PII không xuất ra calendar/opaque context.

Same-day VN/minute/start<end và [start,end) cho phép buổi nối tiếp; validate
room/Class/current Teacher/assigned Student đối với lịch lớp và ca đầu vào
còn hiệu lực theo E03. Existing Program inactive không tự dừng lớp đã dạy;
Teacher blocked không auto đổi người hoặc hủy lịch. Không mở override giải
quyết conflicts, late placement hay lịch cho Class COMPLETED/CANCELLED.

Malformed input/token400; stale set/time/state/count/conflicts409 + deliberate
reload/re-preview, shared auth/scope401/403/404. UI không skip lỗi/lưu subset/
overwrite/merge/auto retry; unknown không proof rollback. Caller lookup hoặc
scoped refetch trước quyết định retry cùng intent, no marker không proof failed.
Completed replay dựng current scope-safe refs, không áp RV cũ như mutation mới.

### 12.3. Transaction, SQL paths và alternatives

W16 Service giữ auth/ownership/workflow và dùng shared connection/TX cho DML,
mandatory audit, ReplayOperation completion; weekly batch và replacement theo
TX09. E03 EC:TIMETABLE/resource và EC:CLASS gates phối hợp current Program/
Teacher/assigned Student booking sets theo predicate; request-key trước
business gates, stable resource ordering, reload set khác thì rollback/
reprepare. Serialize với placement/Teacher change/Class start-complete-cancel/
Attendance/linked Score; no independent NOT_HELD/audit/replay commit.

Replacement chỉ tới/qua scheduled start, old PLANNED chưa tổ chức/no Attendance
hoặc linked Score, new future; error ở new occurrence rollback old flag.
HELD chỉ sau scheduled end/Class ONGOING, ghi người-thời điểm riêng buổi;
không suy từ elapsed time/Attendance/Score, không undo, không đòi Attendance
complete như prerequisite mới. Completion của Class vẫn cần đủ current HELD
và LESSON Attendance theo Gói4c/E03. Không fake0/ABSENT hoặc migrate scores
sang replacement; có dữ liệu cũ thì chặn và giải thích, không tự xóa.

VW03/vw_CurrentScheduleOccurrences phục vụ active calendar/conflict projection;
history query riêng giữ old/replacement và CANCELLED Class. FN04/
ufn_ClassObligationProgress dùng count/read context đã định, không tự xác nhận
occurrence hay thay write guards. Unique current-obligation/current-exam/
predecessor chặn duplicates/forks nhưng không tự enforce overlaps hoặc đúng
một current tại commit; Service/transaction vẫn bảo vệ. Existing TG04/TG05
bảo vệ Attendance/Score references, không suy có thêm Schedule trigger/SP.
Exact DTO/guard manifests/read snapshot/EF writes/grants/timeouts/signatures
còn rà artifact E04/integration; đây chưa là SQL/API đã chạy.

Khuyến nghị concrete previews +5 explicit commands thay generic Schedule
PATCH/delete/current-state setter: thấy rõ action/old-new/count, tránh xóa
history hoặc một phần batch. Đánh đổi thêm previews/payload/set checks.
Stored recurrence/holiday jobs đơn giản input dài hạn nhưng thêm entity/
background ownership ngoài scope đã chốt; không chọn. Automatic partial save
giảm lỗi cả nhóm nhưng sai ADR-088/123 và làm preview khác kết quả, không chọn.

Sau triển khai cần kiểm: all7 role/calendar-detail-preview-replay scopes;
VN minute/end adjacency/cutoff races; weekly count/duplicate-date/conflicts/
one invalid rollback/input changed; one obligation/repeated replacement/
no fork/history count; exam ordering/move dependencies; two concurrent
room-Teacher-Student reservations/placement vs timetable; Attendance/Score vs
move-replace; HELD before end/not ONGOING/no undo/no fake Attendance; Class
complete vs HELD/replacement; unknown response/same key/different content/
caller rights changed/no auto effects. Đây là design objectives, chưa tests.

## 13. Gói4e — Điểm danh một LESSON và điểm nháp một học viên

Trạng thái: **phần đã trình được duyệt ADR-149**; exact routes/DTO/guard-manifest/
SQL/integration bổ sung chưa trình còn rà bản tổng hợp, không blanket approval. Phạm vi NV24 và phần draft/correction
NV25–26, Attendance reads của NV23/AcademicAffairs; F26/F27, W17/phần W18.
Kế thừa ADR-033/083/087/091/093/124/135 và E03 T17–19/TX10–11/TG04–05/FN05.
Không xin duyệt lại cửa sổ, đơn vị lưu hoặc rights đã chốt. Gói4f sẽ bàn
whole-Class submit/resubmit/return/finalize và Student-own FINALIZED reads.

### 13.1. Endpoint inventory và phạm vi

Mọi path dưới /api/v1. cId là Class, sId là LESSON Schedule, aId là Assignment.
Teacher hiện assigned được xem/ghi đúng nhiệm vụ; AcademicAffairs chỉ đọc
Attendance, không nhập thay Teacher. Không mở Student attendance API hoặc
DRAFT visibility mới khi E01 chưa cấp; không finance/DOB/address/global directory.

| Method/path | Input/projection | Guard/effect |
| --- | --- | --- |
| GET /academics/teachers/me/classes/{cId}/attendance | current-history LESSON filter → LessonAttendanceSummary | Current Teacher: ngày/giờ/occurrence, required/recorded/missing count và write window. NOT_HELD/history không tính vào completeness hiện hành; không auto HELD. |
| GET /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | IDs → TeacherAttendanceSheet/context | Đúng Class/LESSON, assignment roster trong scope tối đa15; Chưa ghi hoặc Có mặt/Vắng đã ghi, Attendance versions và Class/Schedule/Teacher/cohort context. History chỉ xem. |
| PUT /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | changed assignment-present values/expectedVersions/expectedSet →200 saved refs/context/operationId | One LESSON delta batch TX10, một lỗi rollback toàn batch; hiện assigned/from start/current/not NOT_HELD/Class chưa COMPLETED/CANCELLED. Không default toàn roster/delete Attendance. |
| GET /academics/classes/{cId}/attendance | current-history LESSON filter → AcademicAttendanceSummary | AcademicAffairs management scope, derived completeness phục vụ Class completion; không grade DRAFT values. |
| GET /academics/classes/{cId}/lessons/{sId}/attendance | IDs → AcademicAttendanceSheet | Read-only recorded/missing values/provenance đúng scope, no mutation actions hoặc quyền Teacher. |
| GET /academics/teachers/me/classes/{cId}/gradebook | IDs → TeacherGradebook/context | Frozen components/weights/kinds; assigned Student ID-name, states/scores/comment/completeness/server total trong nhiệm vụ; không client-computed final result. Tối đa15 rows, không global Student search. |
| GET /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | IDs → TeacherStudentGradeDetail/versions | One assignment current report/scores/comment/latest return reason khi có; DRAFT/RETURNED editable đúng window, SUBMITTED/FINALIZED read-only. |
| GET /academics/teachers/me/classes/{cId}/components/{componentId}/schedule-options | component → current matching occurrences/window | Chỉ frozen component cùng Program/Class, LESSON/FINAL_EXAM đúng type; từ occurrence start mới writable/not NOT_HELD. GENERAL không schedule và không gọi selector. |
| PUT /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | changed component-score-schedule values + optional comment delta/expectedVersions/expectedSet →200 saved detail/context/operationId | TX11 cho một học viên: scores+comment cùng thành công; Class ONGOING/COMPLETED và report DRAFT/RETURNED. Parent GradeReport version được touch cả khi chỉ Score thay đổi. |
| GET /academics/command-results/{requestId}?action=... | UUID/action allowlist thêm2 commands → caller completion/ref | Current Teacher role/assignment cho attendance-save/grade-draft-save; không mở audit/Admin/AcademicAffairs DRAFT visibility, không quyền vĩnh viễn từ completed marker. |

Đề xuất2 UUID write commands: save-LESSON Attendance và save-one-Student draft.
GET không mutation marker; writes kiểm CSRF/Origin. Responses no-store, không
PII/tokens/grades trong URL/raw logs/storage bền. Scoped sheets/context có tối
đa15 assignments theo Class capacity, không mở non-paginated global list; khi
dùng page/filter cho read khác vẫn pagination20/max100/stable ID.

### 13.2. Delta payload, dữ liệu thiếu và cửa sổ

Attendance gửi chỉ các giá trị đã chủ động đánh dấu/thay đổi, present1/0; mỗi
assignment tối đa một entry/request, cùng Class và một LESSON. Missing row
là Chưa ghi, không enum mới và không absent. Có thể lưu một phần roster;
chưa đủ vẫn chặn Class completion. Existing row phải kèm RV, new row kiểm
expected absence/unique ngay trong TX. Không endpoint delete/blank→delete
hoặc default-all. Không tự chuyển HELD khi đủ Attendance.

Attendance mở từ đúng scheduled LESSON start tới trước Class COMPLETED,
không cần chờ end/HELD hoặc hạn24/48h; Class CANCELLED/old NOT_HELD không ghi.
Không thêm prerequisite Class ONGOING nếu baseline không đặt điều kiện đó.
Current Teacher quyền được kiểm ở read và commit, không phụ thuộc nút enabled.

Draft gửi changed scores và comment của đúng một assignment. Component phải
thuộc frozen Program; score0–100/max2 decimals, không cắt/làm tròn input.
Score 0 được nhập thực là hợp lệ; omitted component giữ nguyên, missing chưa
nhập vẫn thiếu. Blank không xóa stored Score hoặc thành0; không DELETE Score.
Comment omitted giữ nguyên; comment được chủ động sửa thành rỗng→NULL theo
optional-text policy, không kéo theo xóa điểm. Comment plain text/NFC/≤1000
code points; không mới mandatory reason khi sửa điểm nháp/RETURNED.

GENERAL schedule NULL; LESSON/FINAL_EXAM phải current đúng Class/type, mở từ
start và không NOT_HELD. Không buộc mọi Student chọn cùng LESSON occurrence
cho một component khi E03 không có invariant đó. Server kiểm toàn aggregate
để tính required/entered/completeness; total=NULL/Chưa đủ điểm nếu thiếu.
Total đủ điểm do server tính decimal/half-up một lần; final_score vẫn NULL
trước FINALIZED. Không coi draft total là Student publication/kết quả chốt.

UI giữ FE14: desktop Lưu học viên từng hàng, mobile trang học viên cùng đơn
vị lưu; Attendance dùng Lưu điểm danh buổi. Hiện Chưa lưu/Đã lưu, warn rời khi
có edits, no autosave/import Excel/durable browser draft. Lưu draft không tự
gửi lớp, giữ DRAFT/RETURNED. Local unsaved edits phải xử lý trước whole-Class
submit ở Gói4f; server không thể chứng minh browser khác không có edits.

### 13.3. Version/context, transaction và unknown result

Attendance context gồm Class/current Teacher/Schedule RV, exact Assignment
membership và RV của các Attendance được sửa; unrecorded row phải vẫn vắng
mặt ở commit. Class RV không chứng minh child/roster không đổi; không dùng
displayed counts/paginated subset làm cohort proof. Concurrent insert cùng
assignment/LESSON không được đổi thành silent upsert overwrite.

Draft context gồm target GradeReport RV, changed Score RV hoặc expected absence,
frozen Program/component context và relevant current Schedule IDs/RVs.
Parent report RV bảo vệ aggregate kể cả Score-only/comment-only save; Class/
Teacher/state/assignment và full required component checks vẫn live. Không
cần lấy GradeReport của Student khác để lưu một Student, không suy per-Student
save thành whole-Class workflow. Exact key/manifest/absence wire representation
và coherent read isolation còn rà artifact, không tự chốt SQL signatures.

W17/TX10 và W18/TX11 cùng DbContext connection/TX; request-key gate trước
EC:CLASS/parent/report/resource guards theo E03§9.1, Account/session giữ tới
commit. Mọi entry trong đơn vị save hợp lệ mới DML; lỗi bất kỳ rollback cả
scores-comment hoặc Attendance delta. Score changes/mandatory audit theo
accepted allowlist và completion cùng commit; Attendance provenance/audit
đúng scope đã chốt, không tự mở mandatory full-payload audit mọi điểm danh.
Serialize với Class completion, Schedule move/replace, Teacher reassignment/
blocking và future whole-Class submit/return/finalize. Không audit/replay commit
riêng hoặc giữ transaction qua thời gian user nhập form.

TG04 backstop Attendance cùng Class/current LESSON/window closing, Service
vẫn kiểm Teacher/server clock. TG05 backstop component-Class/current kind/
SUBMITTED-FINALIZED Score lock; EF writes cần trigger-compatible integration
verification, không hứa một SaveChanges đã thay mọi guard. FN05/
ufn_GradeTotal dùng thật cho scoped grade read/total/completeness và later
review, không tạo grading SP mới để đủ rubric. Exact wire decimal/signatures/
read snapshot/timeout/grants còn bàn integration, chưa DB/API chạy được.

Malformed input/token400; stale value/absence/cohort/state/time409 rollback
save unit, yêu cầu reload/rà lại, no merge/overwrite/auto retry. Cùng UUID/content
không ghi/audit thêm; khác content409. Completed replay rechecks current Teacher
assignment rồi dựng safe refs/current projection, không áp old RV như write mới.
Unknown response dùng caller lookup/scoped refetch trước retry chủ đích; no
marker không proof failed. Teacher mất assignment/session thì không trả draft
cũ từ cache/replay. Reload không tự gửi local edits chưa được người dùng rà.

### 13.4. Alternatives và kiểm chứng thiết kế

Khuyến nghị explicit delta batch một LESSON và one-Student aggregate save với
2 UUID intents: giữ FE14, tránh full-roster replacement tạo dữ liệu chưa nhập
hoặc mỗi field autosave khiến scores/comment lưu một phần. Đánh đổi context/
delta tracking và xử lý unknown phức tạp hơn per-field CRUD, payload nhỏ hơn
full-Class save; không thêm import/draft-cache/offline sync. Full-Class draft
save dễ atomic cả bảng nhưng đổi đơn vị đã duyệt; không chọn. Per-cell autosave
ít nút nhưng trái explicit-save/saved-state/unsaved-warning contract; không chọn.

Sau triển khai cần kiểm: all7 roles/list-detail-save-replay/current Teacher;
Attendance before/exact start/after end/before-after Class COMPLETED, no HELD
prerequisite/default rows; one invalid/stale delta rollback/partial roster/
duplicate pair concurrent insert; grade missing≠0/blank≠delete/comment clear/
one invalid score-comment rollback/report RV touch; GENERAL NULL vs wrong
Class/type/current/linked start; DRAFT-RETURNED editable vs SUBMITTED-FINALIZED
locked/COMPLETED score edits still allowed; loss of assignment/block/session;
save vs move-replacement/completion/submit; no AcademicAffairs DRAFT leakage;
FN05 missing NULL/decimal49.995–49.994 rounding; unknown/key mismatch/replay
expiry/current auth/no automatic effects. Đây là design objectives, chưa tests.

## 14. Gói4f — Gửi, trả lại, chốt và xem bảng điểm

Trạng thái: **nội dung 4F01–4F04 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-150**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

- Kế thừa W18/W19, NV20/NV25–26/NV38–39, F27/F23/F12; E03 TX12/FN03/FN05/
  SP03. Không hỏi lại whole-Class, điểm đạt≥50, frozen weights hoặc FINALIZED
  bất biến. Chỉ xin duyệt cách tổ chức contract và phản hồi API.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 4F01 — Teacher gửi/gửi lại | Read-only preview toàn Class trả mã lớp/cohort/state/missing components/versions; hai command riêng Gửi và Gửi lại, không Student selection. Frontend xử lý mọi local unsaved edit trước preview; Class COMPLETED, đủ mọi học viên/component, current Teacher, đúng report states mới ghi. Không tự gửi khi đủ điểm, không auto resubmit, không thêm lý do Teacher khi chưa có policy. |
| 4F02 — Giáo vụ review | Danh sách lớp và chi tiết review đúng quyền qua SP03; không DRAFT values. Preview action RETURN hoặc FINALIZE, rồi command riêng. RETURN bắt buộc lý do/giữ scores-comment; FINALIZE chốt/công bố cả cohort cùng lúc. Một confirmation có Class/count/state/effects; không checkbox chọn vài học viên/duyệt hai tầng/reopen/override. |
| 4F03 — Student xem kết quả | Hai reads own FINALIZED list/detail, chọn đúng lượt học khi học nhiều khóa/học lại. Server trả component/weights/scores/comment/final_score/pass đúng scope; không classmates, DRAFT/SUBMITTED/RETURNED values hoặc export/PDF mới. Trước công bố chỉ hiển thị thông báo chưa có kết quả được công bố khi cần, không gọi total nháp là điểm chính thức. |
| 4F04 — Cohort/TX/replay | Cả4 commands submit/resubmit/return/finalize dùng UUID Idempotency-Key và full Class–Assignment–GradeReport context, không page subset. TX12 all-or-none statuses/history/audit/completion; finalize tính decimal/half-up một lần, lưu final_score và cập nhật Registration.is_unfinished cùng TX. Giữ mỗi lần workflow theo operation_id; 409 rà lại, unknown tra caller result, no automatic effects. |

### Contract minh họa của Gói4f

Mọi path dưới /api/v1; đây là các routes đề xuất để hoàn thiện inventory:

| Nhóm                  | Read/preview                                                                                                                             | Command                                                      |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Teacher hiện assigned | POST /academics/teachers/me/classes/{id}/grade-submission-preview với action SUBMIT/RESUBMIT                                             | POST .../{id}/submit-grades; POST .../{id}/resubmit-grades   |
| AcademicAffairs       | GET /academics/grade-reviews; GET /academics/grade-reviews/classes/{id}; POST .../classes/{id}/review-preview với action RETURN/FINALIZE | POST .../classes/{id}/return; POST .../classes/{id}/finalize |
| Student own           | GET /students/me/grade-reports; GET /students/me/grade-reports/{id}                                                                      | Không write.                                                 |
| Caller result         | GET /academics/command-results/{requestId} với allowlist tương ứng actor/action                                                          | Không quyền mới từ marker.                                   |

Preview không lưu state/giữ khóa qua user confirmation. Full expected set
gồm đúng cohort report IDs/RVs; Score/comment child save đã chạm parent RV.
Server vẫn kiểm live toàn required components/schedules/states, không tin
client isComplete/total/actor. Không buộc đã FINALIZED để Class COMPLETED;
Class completion và grade publication vẫn là hai workflow riêng.

Lựa chọn khuyến nghị: explicit commands + full-cohort preview.
Generic report PATCH hoặc per-Student submit ngắn hơn nhưng mở đường sai
whole-Class invariant; không chọn. Đánh đổi là payload/context lớn hơn, nhưng
Class tối đa15 học viên và phải recheck transaction, không coi nhỏ là miễn race.

## 15. Gói5 — Reports và CSV

Trạng thái: **nội dung 501–503 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-151**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

Kế thừa W20, NV29–32, F30/F31, ADR-063–072/094/101/126 và E03 SP04–06.
Tên nhóm báo cáo không cấp report rights cho Admissions/AcademicAffairs/Admin.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 501 — Báo cáo tài chính | API role-specific Accountant/Director cùng metric đã chốt: revenue theo paid_at, billed theo issued_at của non-CANCELLED Invoice, current UNPAID debt/overdue riêng không period. Response applied filters/queried_at/whole-set totals/group page; exact money strings. Accountant vẫn dùng Invoice nghiệp vụ qua Gói3c, report không biến thành profile/Invoice dump; Director chỉ nhóm không định danh. |
| 502 — Tuyển sinh/đào tạo Director | Hai API riêng Admissions và Training. Admissions Registration count và distinct Student count theo cùng cohort; overall distinct tính riêng, không sum nhóm. Training trả inventory/current obligations tách finalized-results period; HELD LESSON/expected và FINAL_EXAM riêng, replacement không count đôi. No-data khác0%; defaults/history Course theo ADR đã chốt; không hidden Student detail/export. |
| 503 — CSV Accountant | Một read-only export endpoint chạy fresh SNAPSHOT theo applied financial filters/cột report/toàn kết quả, không UUID mutation marker/job/export store. Kiểm quyền mới, max10.000 dòng hoặc5MB; vượt reject toàn file, không stream phần hợp lệ trước rồi báo thành công. UTF8BOM/comma/CRLF/quote/formula safeguards đúng ADR-101; không DB text rewrite, XLSX/PDF hoặc Director export. |

Routes đề xuất:
GET /reports/accountant/finance; GET /reports/director/finance;
GET /reports/director/admissions; GET /reports/director/training;
GET /reports/accountant/finance/csv — tất cả dưới /api/v1.

Mỗi report/export nhiều query dùng một SNAPSHOT nhất quán; queried_at từ
server, không historical-as-of. Filters không PII có thể ở query; page/sort
allowlist và ID ổn định. Kỳ ngày VN chuyển UTC boundaries thống nhất; không
lấy billed−period revenue làm debt. UI giữ kết quả cũ kèm applied filters/time
khi đang sửa bộ lọc; query mới lỗi không trình bản cũ như kết quả mới.

Khuyến nghị endpoints theo report/role, có thể dùng chung query service với
projection quyền riêng. Một endpoint trả mọi fields rồi frontend ẩn bớt ít
routes nhưng dễ leak; không chọn. CSV synchronous có giới hạn đơn giản hơn
background jobs, đổi lại phải kiểm kích thước trước phát file và kiểm timeout.

## 16. Gói6 — Frontend contract để triển khai nhất quán

Trạng thái: **nội dung 601–604 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-152**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

Kế thừa FE01–FE19, không xin duyệt lại menu/sidebar/responsive/page forms/
confirmation/PII/history/calendar/printing/temporary-password UI.
Gói này nối các quyết định UI với API, không tạo mockup hoặc code.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 601 — Route/feature contract | Một route manifest cho F01–F33 trừ F15, paths theo feature/role, list-detail-complex-form URLs hỗ trợ direct link/Back/Forward. Auth context tối thiểu dùng chung shell; menu guards chỉ UX, API vẫn auth. Ghi route→F→role→API→loading/error/empty contract; không staff dashboard mới hoặc thư viện router/cache/form tự thêm. |
| 602 — Request/server-state | Một typed API client chung cho cookie/CSRF/envelope/traceId; trạng thái idle/loading/success/empty/error/unknown tách rõ. Key dữ liệu gồm caller scope/filter/page; discard response cũ khi request mới hoặc phiên đổi. Sau write cập nhật/refetch projections liên quan có quyền, không optimistic update trạng thái quan trọng, background polling giữ phiên hay automatic mutation retry. RAM không là durable storage. |
| 603 — Form/preview/intent | Form giữ draft trong RAM, explicit save; expectedVersions/context cùng dữ liệu đã load. Sửa input invalidate preview. Disable double-click khi pending, UUID một intent; unknown giữ intent/content để tra kết quả, sửa payload là intent mới sau xử lý kết quả cũ. 400 chỉ lỗi trường,409 deliberate reload/review, no merge/overwrite. Unsaved edits warn khi rời; blank không tự0/delete/absent. |
| 604 — Phiên/CSRF/secret UX | 401 xóa auth/sensitive RAM/preview/secret và về login;403 phân biệt CSRF_INVALID với role denial, lấy CSRF/context mới theo Auth contract nhưng không tự phát lại write. Logout/session changes giữa tab chỉ signal không secret, dữ liệu scope cũ bị vô hiệu; quyền cuối cùng server kiểm. Temporary password chỉ first response/one-time RAM/explicit copy/clear khi rời đúng FE18; không replay/login credential cache. |

Quy tắc frontend không chứng minh commit thất bại khi mạng lỗi. Bảng điểm/
Invoice chỉ lấy projection server; nút/hành động derived không bypass guards.
Unknown signup/profile/password vẫn theo ngoại lệ không replay credential-form
đã chốt; không ép tất cả POST vào UUID workflow.

Khuyến nghị tách **server data**, **local form draft**, **preview context** và
**command intent**. Một state object dùng cho mọi thứ ít cấu trúc hơn nhưng
dễ dùng dữ liệu cũ làm quyền hoặc lặp intent; không chọn. Thư viện cụ thể nếu
cần thêm phải có approval dependency riêng ở thời điểm thích hợp sau baseline,
không dùng việc duyệt file này làm quyền cài package.

## 17. Gói7 — Hợp đồng backend/SQL, replay, audit và quyền DB

Trạng thái: **nội dung 701–705 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-153**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

Kế thừa E03-v1 và Gói1–4e. Không thay số bảng/fields/objects/grants policy,
không đưa mọi workflow vào SP, không SQL/migration/connection thật.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 701 — DTO/contract manifest | Hoàn thiện từng endpoint: role/ownership, request-response/error fields, source row/aggregate tokens, full-set/expected-absence semantics, UUID bắt buộc/không dùng, audit và TX. Base64/version không cấp quyền; field chỉ xuất theo role, stable page/sort. Ghi rõ các chi tiết Gói1–4e chưa trình, không suy toàn draft đã duyệt. |
| 702 — Canonical replay digest | Giữ input_digest binary(32)/SHA-256 đã duyệt ở E03 T23, không đổi hash hoặc thêm trường DB. Đề xuất canonical serialization theo DTO đã validate/normalize/allowlist, bao gồm target/expected versions/set/intent content; actor/action từ verified session. Order cố định; set chỉ sort khi semantics không phụ thuộc thứ tự. Không hash raw HTTP JSON hoặc chứa CSRF/JWT/password/temp secret. Marker vẫn completion/ref/mode đúng T23, không response PII snapshot. Exact UTF8 byte representation/contract compatibility trong cửa sổ replay phải trình ở E04. |
| 703 — SQL callers/query mapping | Repository gọi thật SP01–06 đúng paths Admissions/roster/review/reports; SP07 allocator trong approval TX. FN01–06 có callers/parity đã định, không tồn tại chỉ để đủ rubric. Keyless read DTO mappings/parameterization/sort allowlist; actor_id từ verified session, không client tự khai. Trigger tables dùng EF write mapping tương thích; phải có integration evidence sau triển khai, không giả SaveChanges mặc định đã đủ. |
| 704 — TX/locks/read consistency | Command matrix nối mỗi write với TX01–13 và request/aggregate/TIMETABLE gates theo E03§9.1, shared connection/TX cho effects/mandatory audit/replay. Reload/recheck rights/session/time/relationships sau locks; resource set đổi thì rollback/reprepare theo cùng order. Child insert/absence/full cohort không giả Class RV tự thay. Report SNAPSHOT đã chốt; compound preview/detail cần coherent read snapshot ngắn, không giữ qua user confirmation. |
| 705 — Audit/DB privilege contract | Matrix event/allowlisted old-new/reason/operation_id; mandatory fail rollback, pure-security best-effort và retention/IP90 ngày đúng policy. Runtime chỉ grants cần cho real paths, schema/operator riêng;4 SQL demo roles không là7 web roles và không cấp db_owner/sysadmin. Admin audit reads không mở nghiệp vụ khác. Unknown/replay kiểm current rights; không log secret/full payload hoặc app purge/export audit. |

Tổ chức đề xuất: Controller bind DTO → Service auth/workflow/TX → Repository
EF/SP/UDF parameterized trên connection caller. Services khác cùng DB có thể
tham gia operation nhưng không tự commit độc lập. Không microservice/event bus
hoặc “generic repository” che loss of transaction ownership.

SHA-256/input_digest và schema T23 là đầu vào đã Accepted, không xin duyệt
lại. Phần đề xuất702 chỉ hoàn thiện canonical serialization/compatibility:
không hash raw JSON vì thứ tự fields/formatting không nhất thiết là intent khác.
Khuyến nghị DTO allowlist có byte representation xác định và fixtures same-
intent/different-intent; giữ tương thích khi marker cũ còn hạn. Đánh đổi là
phải quản lý contract/canonical normalization thay vì serialize DTO tùy ý.
Không thêm key-id/hash algorithm/ReplayOperation field, không hứa digest tự
giải quyết authorization/concurrency. Nguyên tắc loại secrets khỏi logs tham chiếu
[OWASP Logging](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html).

701/703–705 duyệt cách hoàn thiện contract trong artifact; signature, exact
guard manifest/read snapshot/EF trigger strategy chưa viết phải được owner
đọc ở bản tổng hợp, không coi dấu tick là duyệt mọi refinement tương lai.
Không thay grants thực tế hoặc tạo SQL users tại lượt này.

## 18. Gói8 — Hồ sơ vận hành laptop và bảo mật cấu hình

Trạng thái: **nội dung 801–804 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-154**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

Kế thừa [SELF_HOSTED_DEMO](../design/SELF_HOSTED_DEMO.md), ADR-114–120.
Đây là **thiết kế runbook/config boundaries**, không cài đặt/provisioning.
Không chọn nhà cung cấp tunnel, public domain, chứng chỉ, port/firewall hay
cam kết uptime; D12/D13 vẫn hoãn đến khi nơi chạy/phần mềm được chốt.
Khi đủ bối cảnh, các lựa chọn cụ thể trở về đúng D12/D13 trong OWNER_REVIEW,
không dùng dòng801 để lách hai mục đó.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 801 — Mô hình cùng origin | Soạn hồ sơ dev/demo khác nhau: dev vẫn frontend/API cùng origin qua cấu hình phù hợp; demo đề xuất ASP.NET phục vụ SPA build và /api/v1, frontend gọi đường dẫn tương đối. Chỉ web được đưa qua cổng truy cập đã được phép, SQL không public. LAN/tunnel là profile thiết kế chờ D13, không tự bật. Link thay đổi thì dùng phiên/login mới, không tắt Secure/CSRF để chữa lỗi. |
| 802 — Config/proxy/timeout/health | Config typed và validate khi startup: timezone VN, trusted origin/proxy, JWT/CSRF/DB/crypto keys ngoài repo, exact target runtime theo ADR-119. Fail closed thiếu config; readiness không leak DB/secrets hoặc cập nhật idle. Khóa/query/request có deadline hữu hạn, cancel/timeout→unknown/lookup khi cần; không giữ transaction chờ user. Số timeout/ports/proxy addresses và benchmark cần hồ sơ môi trường trước khi chốt, không tự điền ngưỡng/SLO D12. |
| 803 — Backup/recovery hồ sơ | Giữ daily encrypted SQL backup/7 ngày4 tuần/monthly restore/RPO24h-RTO2h là targets đã chốt. Soạn nơi lưu riêng khỏi ổ primary, quyền operator/key tách backup và checklist restore DB test độc lập. Laptop tắt: ghi nhận missed run, kiểm/catch-up khi bật; không gọi backup cũ là fresh hoặc bảo đảm RPO khi đã lỡ. Đường dẫn/dung lượng/media/access chưa có phải owner xác định trước chạy thật; không backup/restore ở lượt thiết kế. |
| 804 — Runbook demo/bảo trì | Một checklist trước-trong-sau buổi: kiểm bản build/data giả/config/DB/web/readiness/login7roles trên thiết bị khách; chỉ bật tunnel sau được phép; mất mạng/write unknown tra kết quả; tắt đường public sau demo, theo dõi backup/session/secret. Bạn vận hành theo ADR-120; không role Admin/DBA mới. Dependency/security/blocklist review theo chính sách; rollout/rollback chi tiết chỉ lập sau full baseline và yêu cầu triển khai. |

Khuyến nghị một origin cho UI/API, vận hành theo buổi và cấu hình tách secrets.
Separate public frontend/API thêm CORS/cookie/proxy phức tạp và không thuộc
quyết định cùng-origin; không chọn. Hosted24/7/VPS hoặc tự mở port DB không
phải scope hiện tại. Duyệt runbook không cho phép phát link công khai.

**Còn cần dữ kiện trước hoạt động:** provider/domain/HTTPS/trusted proxy,
startup-stop/service account/firewall cần D13; availability/performance cụ thể
cần D12; backup media/path/quyền/restore machine phải có hồ sơ thật. Không hỏi
lại các dữ kiện này ngay ở lượt tạo file và không mặc định đã được cung cấp.
Exact patch/package locks chọn tương thích sau baseline, không pin theo suy đoán.

## 19. Gói9 — Truy vết, thiết kế kiểm chứng và tổng hợp

Trạng thái: **nội dung 901–903 đã trình trong E04-R1 được owner duyệt
ngày01/10/2026, ADR-155**. Chỉ ghi nhận phần hiển thị đã duyệt;
chi tiết bổ sung chưa viết phải rà bản tổng hợp. E04/E05 chưa nghiệm thu.

Kế thừa D15/E03 DC01–18/rubric ADR-130. Duyệt kế hoạch **bằng chứng cần có**,
không phải tuyên bố test hoặc benchmark đã chạy.

| Phần đã duyệt | Contract đã trình |
| --- | --- |
| 901 — Trace hoàn chỉnh | Một matrix37 NV/32 F/21 W→actor/endpoint/DTO→Service/TX→24 tables/SQL objects→UI/errors→verification case. Chỉ rõ removed NV10/11/F15, deferred/out-of-scope và E04 refinements chưa rà. Mỗi source obligation có path thật dự kiến, không ép một SP/NV hoặc đếm file thay độ phủ. E05 sẽ rà matrix thực tế; hiện chưa tạo complete trace mới. |
| 902 — Bộ ca kiểm chứng | Thiết kế xUnit nghiệp vụ/API, SQL Server test riêng/race hai connection/forced failure/audit-replay/trigger-EF/grants-SP-UDF/index before-after theo rubric. All7 roles/ownership/list-detail-preview-replay-export, clocks/decimals/missing/whole-Class/cohort/no partial/secret retention; browser375/768/1280 và Chrome/Edge khi triển khai. Data giả tách DB chính; không framework mới/code tests hoặc claim pass. |
| 903 — Quy trình đóng draft | Sau ghi nhận các dòng được duyệt, tổng hợp E04 đủ endpoint/DTO/mapping/frontend/runtime/verification, đối chiếu những chi tiết cũ chưa trình. Mọi gap/mâu thuẫn liệt kê để xử lý hoặc owner loại/hoãn rõ; trình bản E04 thực tế để nghiệm thu riêng trong OWNER_REVIEW. Sau E04 mới rà toàn baseline/E05 thực tế; không tick E04/E05 từ23 dòng này và không nhảy sang implementation plan. |

Khuyến nghị coverage matrix + risk-based verification, tận dụng DC/TX/rubric
đã chốt. Chỉ kiểm “happy path” dễ làm nhanh nhưng không chứng minh scope,
race, replay hoặc rollback. Chạy full load/browser/SQL ở giai đoạn thiết kế
không có ứng dụng không phải bằng chứng; mọi execution đợi authority sau này.

903 duyệt **quy trình**, không nghiệm thu tài liệu chưa tồn tại. NFR targets/
backup/SQL counts vẫn cần đo chứng minh sau triển khai; E04 là chốt thiết kế,
không biến thành bài kiểm tra runtime hiện tại hoặc cấp quyền vận hành.

## 20. Sổ quyết định và kiểm chứng của bản đang soạn

Mục1 kế thừa quyết định đã Accepted, không xin biểu quyết lại. Owner đã duyệt
**phần Gói1 được trình** ngày01/10/2026, lưu ADR-139: Base64/expectedVersions,
UUID header Idempotency-Key/retry theo intent/no secret replay và POST body cho
PII search. Các chi tiết bổ sung trong mục3 chưa trình riêng (camelCase,
decode8 bytes, key inventory, digest/pipeline/refetch) cần rà trong artifact
E04 hoàn chỉnh; không suy lời duyệt bao trùm chúng. Mục4 Gói2a đã trình được
owner duyệt ADR-140:5 endpoint/context tối thiểu/CSRF RAM-header/lấy lại sau
login-logout/403 CSRF_INVALID/no automatic retry/no credential replay.
Chi tiết bổ sung chưa trình (exact cache/status/middleware/projection fields)
cần rà trong artifact thực tế. Mục5 Admin Gói2b đã trình được duyệt ADR-141:
account search/detail/staff-create/reset/4 state commands/calling-Admin safe
result lookup/audit list-detail-operation; command UUID/reset-state RV,
transactional audit/replay, first-secret-only/replay-no-secret/unknown không
proof thất bại; no email-role edit/delete/export/new business rights.
Các chi tiết chưa trình (exact DTO/status/transition/action codes/signatures)
còn rà cùng artifact, không suy blanket approval. Mục6 Students Gói3a đã
trình được owner duyệt ADR-142:6 endpoint đăng ký/profile/home/catalogue,
no signup auto-login/atomic Account-Profile,2 RV/contact4 fields, own /me/
Student NORMAL, no Idempotency-Key/credential-form replay ở signup-profile,
unknown không auto retry, ACTIVE selection/history và backend rights.
Exact DTO/cache/error/sort/query chi tiết chưa trình còn rà cùng artifact.
Mục7 Student Admissions Gói3b đã trình được owner duyệt ADR-143: own
Registration/create/cancellation preview-cancel; options/book/cancel/reschedule
atomic old-new Booking; own calendar/results; confirmation preview-confirm;
caller-only result lookup,6 write-command UUID, effects/audit/completion một TX,
related versions và booking/attempt membership, preview không giữ chỗ,
server recheck/409 và deliberate reload. Exact routes/DTO/status/guard-key/
digest/SQL signatures bổ sung chưa trình còn rà cùng artifact, không suy
blanket approval. Mục8 Admissions/Finance Gói3c đã trình được owner duyệt
ADR-144: role-specific search/detail; approval preview/atomic ADMITTED-Invoice
UNPAID; reasoned rejection/cancellation preview-atomic cancel; own Invoice/fresh
browser print/no PDF; collection preview/atomic Payment-PAID; metadata-only
reasoned old-new correction;5 UUID commands/related versions/atomic audit-
completion/caller-only lookup/unknown không proof failure, financial snapshot
reads. Exact routes/DTO/status/guard-key/digest/SQL/settings chưa trình còn rà
cùng artifact. Mục9 ca thi/Teacher chấm thi Gói4a đã trình được owner duyệt
ADR-145: Academic Session management/open-close/cancel-preview/atomic cancel/
grader options-change preview/reasoned change; task-specific rosters/current
Teacher assignment; per-student initial score/absence/reasoned correction,
atomic result-best Band/history;9 UUID writes/related versions-set/server
recheck/caller lookup/no automatic effects và adopted roster/state/Band SQL
callers. Exact routes/DTO/status/guard-key/SQL/read-isolation/runtime refinements
chưa trình còn rà trong artifact. Mục10 cấu hình Gói4b đã trình được owner
duyệt ADR-146: Course reads/create/name-price/no code-status generic edit,
separate deactivate/restore; Program DRAFT/full-configuration-component TX,
activation preview/explicit lifecycle/name edit independent of freeze;
Band seed reads/no editor-unlock;10 UUID writes/Program+component set/
atomic audit-completion/current guards/caller lookup/no automatic activation.
Exact routes/DTO/status/guard-key/SQL/runtime chưa trình còn rà trong artifact.
Mục11 Class/xếp lớp Gói4c đã trình được owner duyệt ADR-147: Class list-detail/
create PLANNED/capacity, Teacher options-change preview/live guards, explicit
open-start-complete-cancel/prerequisite-effect previews/cancel reason; eligible
Registration search/suitable Class options, one-Registration placement-preview/
atomic Assignment-DRAFT report-audit-completion/no default Score-Attendance;
AcademicAffairs management/Teacher currently assigned/Student own-Class reads,
8 UUID writes/related versions-set/server recheck/caller lookup/no auto effects.
Exact routes/DTO/status/guard-key/SQL/read-isolation chưa trình còn rà artifact.
Mục12 lịch Gói4d đã trình được owner duyệt ADR-148: role-scoped current/history
calendars, individual LESSON/FINAL_EXAM preview-create riêng, concrete weekly
all-or-none/no holiday skip; pre-start move/no dependent data, reasoned atomic
non-held replacement cùng obligation/history/no extra count, manual after-end
HELD/ONGOING/no undo-auto-complete;5 UUID writes/related versions/server recheck/
preview no reservation/conflict re-preview/caller lookup trước retry chủ đích.
Exact routes/DTO/status/guard-manifest/SQL/runtime chưa trình còn rà artifact.
Mục13 Attendance/per-Student draft Gói4e đã trình được owner duyệt ADR-149:
Teacher assigned-Class/AcademicAffairs read-only Attendance; one-LESSON delta
batch/no default-all, Teacher scoped gradebook/components/return reason/no
new DRAFT visibility; one-Student scores-comment all-or-none/desktop-row-mobile
Student page/missing≠0/blank≠delete/server total/no auto send-publish;
2 UUID writes/related versions-parent report/live guard/caller lookup/no auto
retry. Exact routes/DTO/status/guard-manifest/SQL/integration chưa trình còn rà.
Owner đã tích23 mục E04-R1 và xác nhận đồng ý toàn bộ ngày01/10/2026:
Gói4f/5/6/7/8/9 lần lượt ADR-150–ADR-155, nội dung đã trình ở mục14–19.
Phải hoàn thiện endpoint/DTO/guard/canonical-byte/SQL-signature/trace/runtime
chi tiết thực tế và liệt kê gap/deferred trước nghiệm thu E04 riêng; E05 sau E04.
OWNER_REVIEW giữ hai cổng chưa tích; D12/D13 hoãn, chưa code hay kiểm thử.
Phương án/lý do/đánh đổi giữ ở từng mục, không thêm ADR cho đề xuất chưa duyệt.

Ca kiểm chứng sau triển khai: invalid/missing/stale tokens không bypass scope;
profile hai tokens/all-or-none; Score/comment parent token; cohort membership/
version change chặn whole-Class stale; completed replay không bị token cũ gây
mutation mới; key khác content409; revoked/changed rights không lộ replay;
reset marker không secret; search PII không URL/storage/raw logs; proxy không
làm rơi header. Đây là mục tiêu thiết kế, chưa app/SQL/browser tests.

E04 cần endpoint inventory đủ W01–21/37 NV/32 F và API→service→transaction→
SQL mapping, auth/frontend/runtime/verification hoàn chỉnh trước owner rà
bản tổng hợp. Đồng ý các gói1–9 không tự tích E04/E05 hoặc cấp quyền code.

## 21. RF behavior approval — 03/10/2026

Owner duyệt RF01/RF02/RF04, ghi Accepted ADR-157/158/159; RF03 giữ156.
Requirements4.9 và dated NV/W addenda, API055 create cutoff/API064/072 equality,
SC02 P/G no-op rules đã synced. Valid full context trước equality; unchanged
provenance retained,mixed actual changes atomic,all-equal400,no new marker;
completed same-key replay remains riêng. Full canonical/SP-FN/EF/grants/ref
unions/CSV/runtime draft refinements/E04/E05 chưa nghiệm thu. D12/D13 deferred,
no implementation planning/code/SQL/tests. Không mở lại ba RF đã duyệt.
