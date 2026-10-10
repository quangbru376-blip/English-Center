# E04 — Schema DTO theo miền, Accepted trong E04-v1

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản: E04-D10 trong E04-v1, ngày03/10/2026. Trạng thái: **Accepted trong E04-v1 ADR-160; E05 Accepted ADR-161**.

Phụ lục của [SYSTEM_ARCHITECTURE_SPEC](SYSTEM_ARCHITECTURE_SPEC.md), nối ID
trong [E04_CONTRACT_CATALOG](E04_CONTRACT_CATALOG.md). Nguồn chuẩn:
[requirements](../requirements.md), [DECISIONS](../DECISIONS.md), E01/E02/E03.
Không thêm quyền/trường DB, không code/SQL/OpenAPI triển khai hoặc package.

D10 có154/154 schema drafts API-001–154: giữ149 D9, thêm5 Reports/CSV.
Đã phủ schema miền cho mọi API; SC-01 đã có rà tổng hợp và
[final contract](E04_FINAL_CONTRACT.md), Accepted actual E04 ADR-160.
Field names/object shapes/enum/error strings/search semantics/manifest keys/
decimal-input spelling mới là **refinement dự thảo để owner rà actual E04**,
không suy accepted từ ADR-140–145/153 hoặc23 checkbox. Policy/password/
ownership/precision/role scope đã accepted giữ nguyên. RF03 được duyệt riêng
ngày03/10/2026 và ghi ADR-156, không suy duyệt các wire refinements khác.
API-ID/capability keys là ID tài liệu/UI, không T23 canonical actioncode.
E04-RF03 root/weekly-create và move-destination future cutoff đã Accepted
ADR-156 ngày03/10/2026, đồng bộ requirements/NV19/W16; RF01/02 Accepted ADR-157/158; RF04 Accepted159.
Không coi lời duyệt RF03 là nghiệm thu toàn bộ D7/E04/E05.

## 1. Quy ước đọc và kiểu wire

Current final refinement: [E04-v1 mục2–6](E04_FINAL_CONTRACT.md) áp dụng
pipeline/error precedence/strict UUID-RV/scalars/current authorization/replay
unions/CSV cap. Các ghi chú “còn SC-02/03/04” của D2–D10 bên dưới là lịch sử;
now resolved by SC02–04 +final contract, không yêu cầu viết lại phụ lục.
Domain fields/required/NULL/sort/full manifests giữ tại từng schema; exact
technical choices vẫn proposed cho actual E04 acceptance, không auto Accepted.

R = property phải có; O = được bỏ qua; nullable = JSON null được phép.
Required và nullable là hai vấn đề khác nhau. Với PUT profile,4 editable
properties phải có, address có thể null; thiếu không là lệnh xóa. Với signup,
address là optional, omitted/blank normalize→NULL. Password giữ nguyên
code points/space/case, không trim/NFC hoặc stringify/coerce input khác string.

| Alias | Kiểu JSON / constraint / nguồn |
| --- | --- |
| BusinessId | integer dương trong range SQL int; chỉ target đã được kiểm scope; không UUID/code/user-supplied actor. |
| RowVersion | string Base64 decode đúng8 byte; opaque; không order/time/security_version. |
| Instant | string UTC ISO-8601 có Z, precision theo datetime2(3); server clock/Asia_Ho_Chi_Minh conversion giữ ADR-091. |
| LocalDate | string YYYY-MM-DD là date thật, không time/offset; DOB không sau ngày VN hiện tại. |
| Money | string nguyên đồng dương từ decimal(18,0), không vượt999999999999999999; không number hoặc định dạng nghìn/ký hiệu ₫. ReportAmount không âm decimal38 được đặc tả ở mục31.1. |
| AccountRole | một trong Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin; không role array/switcher. |
| AuthScope | NORMAL hoặc PASSWORD_CHANGE; chỉ dùng Auth context, không DB/business state mới. |
| OptionalText | string theo limit/code-point/NFC/trim của trường; empty→NULL khi optional. Không HTML/rich text. |

Đề xuất schema bind **allowlist properties**, từ chối unknown properties400
thay vì mass assignment. Enum case và shape errors không tự coerce về state
khác; không nhận snake_case aliases/EF entity properties để sửa DB. Đây là
chi tiết wire đang rà, không tuyên bố runtime binder đã được cấu hình.

### 1.1. ErrorResponse dự thảo (D02 đã chốt envelope; shape chi tiết để rà)

| Property | Required / kiểu | Nội dung / không được chứa |
| --- | --- | --- |
| code | R string | Machine-readable code; proposed strings trong bảng HTTP mục1.1, không raw SQL number hoặc internal exception. |
| message | R string | Thông báo tiếng Việt đúng scope; không secret/full payload/thông tin target ngoài quyền. |
| fieldErrors | R object map field-path→`array<string>` | Map rỗng khi không field error; chỉ field names công khai của request và message, không echo input values/password/PII. |
| traceId | R string | Correlation để hỗ trợ; không secret/session token hoặc cấp quyền đọc logs. Format cuối còn rà logging contract SC-03. |

Không thêm error.details chứa EF entity/old request/body/token/version bí mật.
403 CSRF_INVALID và404 RESULT_NOT_CONFIRMED là codes đã trình ở bản chính;
những tên khác dưới đây là đề xuất registry thống nhất, chưa ADR mới.

| HTTP / proposed code | Khi dùng / UI |
| --- | --- |
| 400 VALIDATION_ERROR | Shape/type/required/range/precision/token encoding sai; giữ form, field errors theo allowlist. |
| 401 AUTHENTICATION_FAILED | Login credentials không hợp lệ/không được dùng: một thông báo chung, không email-exists/status/throttle nội bộ. |
| 401 AUTHENTICATION_REQUIRED | Protected request invalid/expired/revoked: clear sensitive RAM/preview/secret rồi login. Không tự replay. |
| 403 FORBIDDEN | Role hoặc restricted scope không được phép; không fetch thêm dữ liệu bị cấm. |
| 403 CSRF_INVALID | Header/token hoặc Origin invalid; lấy context/token lại, user gửi chủ đích, không auto retry mutation. |
| 404 NOT_FOUND | Target thiếu/ngoài ownership/read scope; cùng thông báo, không lộ đối tượng ngoài quyền. |
| 409 CONFLICT | Stale versions/state/uniqueness race; error field khi phù hợp nhưng không lộ ID của người khác; reload/review không overwrite. Subcodes từng miền còn làm tiếp. |
| 429 RATE_LIMITED | Login IP cap theo ADR-100 +Retry-After; không thay Account LOCKED/idle/session. |
| 500 INTERNAL_ERROR | Unexpected failure: message chung +traceId, không stack/SQL/secret; mutation transport/server result không tự coi chắc rollback. |

Đây không phải policy bắt mọi lỗiSQL thành409. Validation/race có mapping
known; unexpected lỗi giữ500 và rollback owner transaction khi còn khả năng.
Không hứa HTTP500/timeout đồng nghĩa chưa commit. Logout/password/register
unknown flows dùng accepted recovery, không ép vào replay lookup.

### 1.2. Pagination envelope draft cho các lists đã đặc tả

Query: page O integer≥1 (default1); pageSize O integer1–100 (default20).
Đề xuất response: items R `array<projection>`, page/pageSize R integer theo
request đã áp dụng, totalCount R integer≥0 trên toàn scoped filter set.
items=[] là kết quả hợp lệ, không404; không tự trả/trộn peer rows. Không thêm
infinite scroll/cursor hoặc arbitrary sort expressions. Chỉ các paginated lists
đã ghi page/pageSize ở mục5–34 dùng envelope, gồm Class/candidate/roster và
calendar/scoped reads. Auth/profile, full BandReferenceSet/ClassTeacherOptionSet,
membership/preview/weekly-plan/Attendance sheets/gradebook/selector arrays
không tự paginate hoặc bị cắt tại100; scoped cohort có giới hạn15 theo Class.
Reports dùng groups Page/dual Training page ở mục32–34; CSV không page.
Không áp ngầm envelope hoặc cắt selector metadata.

## 2. Auth — API-001–005 / W02

Mọi POST có CSRF/Origin; cookie JWT host-only/HttpOnly/Secure/SameSite Strict,
không body JWT/Authorization-token cache hoặc refresh JWT endpoint. Request
token từ API-001 chỉ RAM, header X-CSRF-TOKEN, lấy lại khi identity đổi.
Auth context/bootstrap không gia hạn idle; không heartbeat. Responses no-store.
Thứ tự framework validation/middleware chi tiết còn SC-03, không cấu hình ở đây.

### 2.1. API-001 GET /auth/csrf

Request: không body, không accountId/studentId/role/business query.
Response200 CsrfContext: requestToken R string opaque. Không AuthContext,
AuthSession issuance, expiration extension hoặc business data. Có thể bootstrap
anonymous hoặc identity hợp lệ hiện tại theo bản chính; không dùng requestToken
để chọn account. Token failure ở subsequent POST dùng403 CSRF_INVALID.

### 2.2. API-002 POST /auth/login

LoginRequest: email R string (địa chỉ ASCII chuẩn theo ADR-075/091), password
R string unchanged. Missing/wrong type→400; không echo password. Credential
verification failures dùng một401 message chung, không trả account/status hoặc
phân biệt email chưa tồn tại/LOCKED/DISABLED/temp expired trước xác thực.
Account bad-password throttle và IP429 riêng theo accepted Auth policy; không
đặt timer số mới. Không dùng Idempotency-Key/T23 hoặc credential digest.

Response200: AuthContext + JWT cookie server-set. Context sau xác thực không
thêm auth rights cho restricted scope; đọc dưới đây cho login và /auth/me.

| AuthContext field | Required / kiểu | Source / quyền |
| --- | --- | --- |
| accountId | R BusinessId | Verified Account; không studentId hoặc impersonation selector. |
| fullName | R string | Current Account.full_name, không snapshot/profile toàn bộ. |
| email | R string | Current immutable login email; chỉ sau xác thực. |
| role | R AccountRole | Single fixed role từ Account/Role; client không sửa. |
| scope | R AuthScope | NORMAL hoặc PASSWORD_CHANGE theo verified session. |
| accountVersion | R RowVersion | Account token dùng own password form; không security_version. |
| serverTime | R Instant | Thời điểm context được dựng; không deadline do client cấp. |
| absoluteExpiresAt | R Instant | Deadline session server; normal2h, restricted theo accepted max10m/temp boundary. |
| effectiveExpiresAt | R Instant | Min deadlines thực tế do server; chỉ trình bày, không promise session còn valid khi command tới server. |

Không JWT/raw sid/hash/security_version/failed counter/session list, DOB/
address/phone/grade/Invoice/role-switch data. Xong login lấy CSRF mới; NORMAL
vào menu đầu role đã chốt, restricted chỉ password-change/logout. Không auto
upgrade restricted thành NORMAL hoặc auto-login từ signup.

### 2.3. API-003 GET /auth/me

Request không business input; authenticated cookie cần valid Account/session/
version/deadlines.200 cùng AuthContext đúng9 fields; invalid401 envelope không
trả context cũ. Restricted được context tối thiểu trên, không own business data.
No-store; load/bootstrap/refetch không idle activity hoặc credential replay.

### 2.4. API-004 POST /auth/change-password

| ChangePasswordRequest field | Required / kiểu | Validation/effects |
| --- | --- | --- |
| currentPassword | R string | Current credential nguyên văn; verify server, không trim/NFC/digest/replay hoặc log. |
| newPassword | R string |15–128 code points/password policy/offline blocklist đã chốt; không composition hoặc routine expiry mới. |
| expectedVersions | R object | Allowlist một property account. |
| expectedVersions.account | R RowVersion | Từ AuthContext đã load; missing/malformed400, stale409. |

NORMAL/PASSWORD_CHANGE; session/account guarded tới TX13 commit. Đổi hash/
security_version/temp bundle/session revoke/mandatory audit all-or-none.
Success204 no body, clear current JWT cookie; client clear password/auth/context/
CSRF/sensitive RAM và login lại. Không server tự nâng scope hoặc trả credential.
Malformed/blocked new password400 fieldErrors không echo; invalid session401,
CSRF403, stale409. Đề xuất refinement: current credential sai dùng400
VALIDATION_ERROR ở field currentPassword, không401 (phiên có thể vẫn hợp lệ);
không ghi credential value hoặc thay policy login throttle. Owner rà lựa chọn
wire này trong bản E04; không coi đây là decision mới đã accepted.
Unknown response: clear raw passwords, không auto-send hoặc caller-result
lookup; người dùng thử login chủ đích bằng mật khẩu mới/Admin policy khi cần.

### 2.5. API-005 POST /auth/logout

Request không business body/UUID. Success204 revoke current session rồi clear
cookie; pure-security log best-effort không khôi phục phiên khi lỗi. Session
đã mất/hết hạn401, UI vẫn về logged-out/clear sensitive RAM. Không revoke
thiết bị/profile khác, trừ các accepted revoke-all events khác. CSRF/Origin
lỗi403 không báo logout server đã thành công. Unknown: không tự replay; clear
local sensitive state và xử lý valid session trên lần login/user request mới.

## 3. Signup — API-018 / W01 / F02

Anonymous Student self-registration, CSRF/Origin/no-store; không nhân sự đăng
ký thay. Request không role/status/accountId/studentId/mustChangePassword/
tempPassword/business fields; unknown properties theo allowlist draft mục1.

| RegisterStudentRequest field | Required / kiểu | Normalization / validation nguồn |
| --- | --- | --- |
| fullName | R string | Unicode/NFC/trim1–100 code points; không bắt2 từ hoặc cắt input. |
| dateOfBirth | R LocalDate | Date thật/không tương lai theo ngày VN; không age limit mới. |
| email | R string | ASCII local@domain≤254, trim/canonical CI toàn Account; không display name/control/provider dot/+tag rewriting; immutable sau tạo. |
| phone | R string | Vietnamese mobile rule0/+84 tương đương, bỏ separators hợp lệ/canonical0xxxxxxxxx; required và unique toàn Account khácNULL. |
| password | R string |15–128 code points, nguyên văn/no NFC/trim/case conversion; accepted blocklist, không plaintext log/T23. |
| address | O string hoặc null | NFC/trim≤255 code points; omitted/blank/null→NULL; không geography dropdown bắt buộc. |

TX01 tạo Account Student ACTIVE +StudentProfile +mandatory account audit;
không AuthSession/temporary password/autologin. Normalized unique email/phone
race→409 safe field errors, không echo owner ID; validate400; CSRF403; role/
existing scope không được register theo anonymous-only boundary.
Success201 RegisterStudentResult: outcome R literal CREATED, message R string
Vietnamese; không password/profile/Account IDs/JWT/secret. UI xóa password và
chuyển F01. Không Location chứa PII hoặc API email-exists/recovery mới.
Unknown: không gửi lại tự động; owner chủ đích login bằng credentials vừa tạo
hoặc Admin reset theo policy; không kết luận timeout là creation failure.

## 4. Own profile — API-019–020 / W04 / account menu

Chỉ Student NORMAL; owner từ verified Account→StudentProfile, không studentId
input. Không staff/admin edit hộ hoặc /students/{id} toàn hệ thống.

| StudentProfileView field | Required / kiểu | Source / giới hạn |
| --- | --- | --- |
| accountId | R BusinessId | Own T02.account_id. |
| studentId | R BusinessId | Own T03.student_id; không accountId alias. |
| fullName | R string | Current Account name. |
| email | R string | Immutable Account email, read-only. |
| dateOfBirth | R LocalDate | Current Profile DOB. |
| phone | R string | Current Account canonical Student phone, không nullable Student. |
| address | R string hoặc null | Current Profile optional address; không snapshot lịch sử. |
| versions | R object | Properties account/profile, cả hai RowVersion R. |

GET API-019: không body/business query,200 view; invalid401/role hoặc restricted403,
no scoped Profile404; no-store. Không health/heartbeat/Student contact directory.

PUT API-020 UpdateStudentProfileRequest: fullName/dateOfBirth/phone/address
R với constraints như signup (address nullable); expectedVersions R object
với account/profile R RowVersion. Không email/password/role/status/business
state hoặc chỉ gửi1 token khi đổi một phía. Missing property/malformed token400;
wrong ownership404; stale/phone unique race409; session401/CSRF-scope403.

W04 atomic update Account name/phone +Profile DOB/address +mandatory audit;
2 rowversions rechecked trước ghi. Không tăng security_version/revoke phiên chỉ
vì đổi liên hệ; CONFIRMED không khóa4 editable fields này.200 StudentProfileView
mới/cả2 tokens, không responseEF hoặc secret. Omitted address400 ở PUT;
explicit null/blank hợp lệ là optional-address clear, không xóa Score/field khác.
Unknown: GET own profile mới rồi người dùng đối chiếu draft RAM/intent; không
storage/replay profile snapshot hoặc silent merge/auto-save lại form cũ.

## 5. Course selection — API-022–023 / W05 / F04

Student NORMAL chỉ ACTIVE selection; own Registration/history còn thấy Course
đã tham chiếu qua projections khác, không dùng INACTIVE selection detail404
để xóa history. Đây không API quản lý Course/config của AcademicAffairs.

| CourseSelectionView field | Required / kiểu | Source / giới hạn |
| --- | --- | --- |
| id | R BusinessId | T05.course_id. |
| code | R string | Immutable normalized ASCII Course code≤30; không input edit quyền Student. |
| name | R string | Current Course name≤150 code points. |
| tuition | R Money | Current base price cho display selection; chưa Invoice snapshot/approval amount promise. |
| status | R literal ACTIVE | Chỉ catalog selection; không Program lifecycle/Teacher/Student/config metadata. |

API-022 GET /students/me/courses query page/pageSize theo draft mục1.2;
sort code ASC/id ASC cố định,200 paginated CourseSelectionView. Không PII
search query hoặc tự thêm arbitrary price/name filter. API-023 GET /students/me/
courses/{id}: id positive BusinessId,200 cùng fields; INACTIVE/missing404,
invalid ID400 theo shared validation; phiên/role401/403 như /me. Không body
selector role/student/Account. Backend gửi exact tuition string, client không
đoán invoice/hạn thu/đủ điều kiện create Registration từ catalogue stale.

## 6. Admin Account — API-006–014 / W01,W03 / F32

Chỉ Admin NORMAL, kể cả search/detail/lookup. Target có thể Student hoặc
nhân sự; tạo mới chỉ sáu role nhân sự. Không quyền sửa role/email/Profile/
nghiệp vụ, không nhận actor/security_version/mật khẩu do Admin tự chọn.
Tất cả paths trong mục này dưới /api/v1; POST/search có CSRF/Origin/no-store.

### 6.1. Read projections và search — API-006–007

| AccountAdminView property | Required / kiểu | Nguồn / giới hạn |
| --- | --- | --- |
| id | R BusinessId | T02 Account, không StudentProfile ID. |
| fullName, email | R string mỗi trường | Current Account; email bất biến. |
| phone | R string hoặc null | Canonical phone; Student required, staff có thể NULL. |
| role | R AccountRole | Một role cố định, không permission editor. |
| status | R enum ACTIVE/LOCKED/DISABLED | Account status; không tự suy từ session/temp expiry. |
| createdAt | R Instant | Account creation UTC. |
| mustChangePassword | R boolean | Không hash/secret hoặc password history. |
| temporaryPasswordExpiresAt | R Instant hoặc null | Từ temp bundle; hết hạn không làm status thành LOCKED. |
| version | R RowVersion | Account optimistic token, không session/security version. |
| allowedActions | R `array<AdminAccountAction>` | Quyền và state hiện tại, không bảo đảm command sau sẽ thành công. |
| disabledReasons | R object action→string | Chỉ các action không khả dụng trong allowlist; lý do không lộ Account khác. |

AdminAccountAction dự thảo: RESET_PASSWORD, LOCK, UNLOCK, DISABLE, RESTORE.
Đây là key UI capability, **không** canonical T23 action_code. Không action
DELETE/EDIT_ROLE/EDIT_PROFILE, không expose AuthSession/failed counters.

API-006 POST /admin/accounts/search: AccountSearchRequest có name/email/phone
O string; name NFC/trim tối đa100, email ASCII/trim tối đa254, phone normalize
theo Account rule. Đây là filter, không bắt email/name là giá trị hoàn chỉnh
như form tạo Account. Đề xuất name/email dùng contains parameterized/literal
(ký tự SQL wildcard không là quyền viết pattern); phone canonical exact.
Blank filter→omitted. role O AccountRole; status O enum trên; page/pageSize
O theo mục1.2. Không DOB/address/Student selector/arbitrary sort/payload search.
200 envelope mục1.2 items AccountAdminView; cố định id ASC. Filter sai400;
items rỗng200, không404. PII body/RAM, không URL/log hoặc persisted filter.

API-007 GET /admin/accounts/{id}: positive BusinessId; không body hoặc business
query.200 cùng AccountAdminView, missing404. Cùng projection list/detail
ngăn list lộ dữ liệu mà detail chặn; không proxy đọc nghiệp vụ Student.

### 6.2. Tạo staff và reset — API-008–009

CreateStaffRequest: fullName/email R theo constraints signup; role R chỉ
Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin; phone O string
hoặc null, blank→NULL, có giá trị thì canonical/unique toàn Account.
Không DOB/address/StudentProfile/password/status/mustChangePassword input.
API-008 POST /admin/staff-accounts: UUID Idempotency-Key bắt buộc; TX01 Account
ACTIVE +mandatory audit +T23 completion cùng commit, không AuthSession/auto-login.
Malformed400, normalized email/phone conflict409; không lộ owner của giá trị trùng.

ResetPasswordRequest: expectedVersions R object với account R RowVersion,
không business field khác. API-009 POST /admin/accounts/{id}/reset-password:
UUID bắt buộc; missing/malformed400, missing target404, stale/state guard409.
TX13 reset hash/temp bundle/security_version/revoke-all/audit/replay atomic;
**giữ nguyên** ACTIVE/LOCKED/DISABLED, không unlock/restore hoặc hủy assignment.
Guard self/last-active-Admin của lock/disable không được áp thành lệnh cấm
reset mới; các guard đã chốt vẫn kiểm đúng intent.

First success API-008201, API-009200: CredentialIssueResult đúng7 properties:
accountId R BusinessId, email R string, role R AccountRole, temporaryPassword
R string, temporaryPasswordExpiresAt R Instant, operationId R UUID, accountVersion
R RowVersion. Server sinh password32 ký tự theo ADR-114, expiry24h theo ADR-054; response
chỉ dựng **sau commit**. Không thêm secret URL/Location, JWT, profile hoặc sid.

FE18: secret chỉ RAM khi kết quả đầu tiên đang mở; copy chủ đích/ack/clear khi
đóng, logout hoặc hết hạn. Không log, audit, browser storage, marker hoặc GET
retrieval. Không lưu response để dedup. Completed replay cả hai trả200
SafeCommandCompletion mục6.4, resultMode SECRET_RESULT_UNAVAILABLE, không
tạo secret mới/trả plaintext cũ. Cần cấp secret khác là intent reset mới do
Admin chủ đích, với context/key mới sau khi xác định trạng thái cũ.

### 6.3. Bốn state commands — API-010–013

Mỗi body chỉ expectedVersions R object với account R RowVersion; UUID header
bắt buộc. Không generic status PATCH, lý do bắt buộc mới hoặc target role edit.

| API / POST path | State nguồn→đích | Guard/effects cùng TX13 |
| --- | --- | --- |
| API-010 /admin/accounts/{id}/lock | ACTIVE→LOCKED | Chặn self và last ACTIVE Admin theo ADMINSET; revoke-all. |
| API-011 /admin/accounts/{id}/unlock | LOCKED→ACTIVE | Không khôi phục AuthSession đã revoke hoặc tự cấp secret. |
| API-012 /admin/accounts/{id}/disable | ACTIVE hoặc LOCKED→DISABLED | Chặn self/last ACTIVE Admin theo guard; revoke-all. |
| API-013 /admin/accounts/{id}/restore | DISABLED→ACTIVE | Không restore session/assignment/nghiệp vụ cũ hoặc tạo role mới. |

Version/state/self/last-active race409, missing input400, missing target404;
session401, role/scope/CSRF403 chung. Success200 AdminAccountCommandResult:
account R AccountAdminView mới, operationId R UUID. Mandatory audit/T23/state/
security effects phải atomic; failure không partial Account hoặc báo success.
Same completed replay dựng current authorized AccountAdminView và operationId
cũ, không phát lại effects/audit. View có thể khác state ngay sau intent cũ;
không gọi đó là snapshot kết quả lúc commit. Target không còn được phép đọc
thì không dùng stored response để vượt scope.

### 6.4. Caller result — API-014 (và shape dùng API-037)

GET /admin/command-results/{requestId}?action=...: requestId R UUID path,
action R allowlisted family selector. Chỉ caller hiện xác thực Admin; đủ sáu
families staff-create/reset/lock/unlock/disable/restore, không union quyền của
endpoint khác. Selector string registry/T23 canonical action_code còn SC-02;
không đoán chuỗi DB từ API-ID, URL hoặc capability keys mục6.1.

| SafeCommandCompletion property | Required / kiểu | Giới hạn |
| --- | --- | --- |
| outcome | R literal COMPLETED | Chỉ đã có marker committed hợp lệ, không PENDING/FAILED/job status. |
| operationId | R UUID | Operation server của intent đã commit. |
| resultMode | R enum SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE | T23, không credential retry quyền mới. |
| target | R object | Allowlist tham chiếu; Admin chỉ accountId R BusinessId. |

Lookup200 shape trên, **không** AccountAdminView/PII/secret/hash/input_digest/
actor selector/expiry hoặc password copy button. Malformed UUID/missing hoặc
unknown action400; valid family nhưng không thuộc caller/action/window404
RESULT_NOT_CONFIRMED cùng message. Không thấy marker không chứng minh rollback:
có thể đang commit/chưa thấy/cleanup/window khác. Không status polling/auto-retry
hoặc tự reset. Dùng GET Account/audit đúng quyền để đối chiếu rồi quyết định
intent mới; marker tối thiểu24h không bảo đảm dedup vô hạn.

## 7. Admin Audit — API-015–017 / W21 / F33

Chỉ Admin NORMAL; read-only, no-store. Không CRUD/purge/undo/export log.
Audit giữ event-grain và operationId grouping; không đổi thành một row/operation.

### 7.1. API-015 GET /admin/audit-events

Query allowlist fromDate/toDate O LocalDate (ngày VN, inclusive; convert thành
[from start, ngày sau to start) UTC); một bound có thể omitted, from>to400.
actorId O BusinessId; action/entityType O string theo server registry, không
arbitrary pattern hoặc JSON path; entityId O string tối đa100 theo T22 safe
reference, không raw sid/email/name/payload. result O SUCCESS/FAILURE.
page/pageSize theo mục1.2, cố định occurredAt DESC/id DESC. Không implicit date
window/cắt lịch sử; filter không tự mở rộng retention/IP access.
200 envelope items AuditEventSummary dưới đây; empty200. action/entityType
registry đầy đủ/field-change mappings cần SC-03, không thêm thao tác vào registry
chỉ vì client gửi string. Không unknown-field pass-through/raw details_json.

| AuditEventSummary property | Required / kiểu | Nguồn / giới hạn |
| --- | --- | --- |
| id | R BusinessId | T22.audit_id. |
| occurredAt | R Instant | Event time, không thời gian profile hiện tại. |
| actorId | R BusinessId hoặc null | NULL chỉ sự kiện chưa xác thực; UI “Chưa xác thực”, không đoán email. |
| action, entityType | R string mỗi trường | Server accepted action/entity registry; không raw exception. |
| entityId | R string hoặc null | Safe target reference; không raw session identifier. |
| result | R enum SUCCESS/FAILURE | Không trạng thái nghiệp vụ target. |
| operationId | R UUID | Cùng ID cho effects trong operation; không quyền đọc target nghiệp vụ. |

Không join full Account/Profile để đổi event thành current-contact snapshot.
Actor/target IDs không cho Admin quyền đọc điểm/Payment/Student records.

### 7.2. API-016 GET /admin/audit-events/{id}

Positive BusinessId,200 AuditEventDetail = summary + reason R string hoặc null,
changes R `array<AuditFieldChange>`, ipAddress R string hoặc null.
AuditFieldChange draft: field R public allowlisted string; oldValue/newValue R
JSON scalar (string/number/boolean/null). ID/time/enum/money/score scalars giữ
wire type tương ứng, không stringify arbitrary entity/request. Một change chỉ
field được chấp nhận cho action/event; không nested payload/person directory.
Không có changes/reason hợp lệ thì []/null, không fabricated old values.
Reason và before/after được giữ từ event, không tạo lại bằng trạng thái hiện tại.

Đây là projected field-change schema, **chưa** action-by-action audit field
registry SC-03. Password/hash/temporaryPassword/JWT/CSRF/raw sid/full request/
response bị cấm tuyệt đối; không xuất cả details_json nếu chưa có mapper.
IPv4/IPv6 tối đa45 từ T22; hết retention→null và UI “Không còn lưu”; không
giữ IP lâu hơn90d chỉ để detail khác list. Mandatory business/account history
không tự purge90d; SECURITY rows theo retention policy. Missing event404, không
giả NULL actor là Account đã bị xóa.

### 7.3. API-017 GET /admin/operations/{operationId}/audit-events

UUID path, page/pageSize theo mục1.2; sort occurredAt DESC/id DESC và200 cùng
AuditEventSummary page chỉ operation đó. UUID sai400; operation không có event
còn trong quyền/retention→200 items=[]/totalCount=0 (đề xuất collection semantics),
không kết luận operation chưa commit. Không operation-owner selector/cấp quyền
business details hoặc gom mất child audit. API-016 detail vẫn cùng allowlist.

## 8. Student Home — API-021 / W04 / F03

GET /api/v1/students/me/home; Student NORMAL/own, page/pageSize theo mục1.2.
200 page StudentHomeCard, sort Registration.createdAt DESC/id DESC; mỗi card
một Registration của mình, gồm history, Course referenced INACTIVE không bị
xóa khỏi home. Chưa có hồ sơ→items=[] và UI chọn Course, không tự đăng ký.

| StudentHomeCard property | Required / kiểu | Nguồn / giới hạn |
| --- | --- | --- |
| registrationId | R BusinessId | Own T09; không studentId chọn từ client. |
| course | R CourseReference | id/code/name/status ACTIVE hoặc INACTIVE theo CourseReference mục9.1; không selection-only ACTIVE literal. |
| status | R RegistrationStatus | Bảy state mục9.1; không progress-percent hoặc COMPLETED Registration mới. |
| createdAt | R Instant | Để stable sorting/history. |
| currentStep | R string | Nhãn suy từ state/workflow; không persisted enum mới. |
| nextAction | R object hoặc null | action R StudentRegistrationAction, target R scoped ID refs mục9.1; không arbitrary external URL. |
| waitingReason | R string hoặc null | Khi chờ grading/approval/payment/placement hoặc hết action; không event inbox. |
| nearestEvent | R HomeEvent hoặc null | Ca thi/buổi lớp gần nhất còn phù hợp với hồ sơ hiện tại; không peer roster. |
| unpaidInvoice | R HomeInvoice hoặc null | Own UNPAID: id/number/amount/dueDate/isOverdue, không Payment metadata. |
| finalizedReport | R object hoặc null | gradeReportId/registrationId/classId BusinessId refs, chỉ FINALIZED; không score chưa công bố. |

CourseReference không tuition promise. HomeInvoice: id R BusinessId, number R
string theo Invoice number, amount R Money, dueDate R LocalDate, isOverdue R
boolean server theo FN06; không status OVERDUE mới. finalizedReport refs đều R
BusinessId khi object khácnull, không report score hoặc fabricated ID. HomeEvent đề xuất
kind ENTRY_TEST hoặc CLASS_SESSION (presentation discriminator), registrationId
BusinessId, startAt/endAt Instant, room string đều R; ENTRY_TEST có bookingId R
BusinessId, CLASS_SESSION có scheduleId/classId R BusinessId. Không nhận ID
khác kind. Không session roster/phone/grader credentials. Chính sách time:
ENTRY_TEST là own SCHEDULED Booking/Session không CANCELLED; CLASS_SESSION là
own current Schedule occurrence_state PLANNED/Class không CANCELLED. Cả hai
còn endAt>serverNow; chọn earliest startAt ASC/kind ASC/target ID ASC để tie
ổn định giữa hai loại. Event đang diễn ra có thể
hiện nhưng không là permission hủy. Không dùng nearestEvent để ẩn lịch sử hoặc
tự confirm occurrence. CANCELLED/REJECTED hồ sơ không gợi ý event như việc mới.

nextAction hoặc waitingReason giải thích việc tiếp theo theo FE11; exact key/
priority là draft presentation, không làm Student thành Admissions/Accountant/
AcademicAffairs. Khi không có việc phù hợp, nextAction=null; waitingReason có
nhãn lịch sử/hoàn tất hoặc chờ theo guard, không suy missing score=0. Nếu cần
xem kết quả/lịch sử thì dùng links own đã chốt, không gọi nó là lệnh mutation.
StudentRegistrationAction mục9.1 cũng dùng capability/link keys, không T23 action.
Không đọc GradeReport DRAFT/SUBMITTED/RETURNED values kể cả để suy home card.
Home là read model, không ghi trạng thái/idle/polling; detail/preview recheck
quyền và điều kiện, link không giữ chỗ hoặc chứa JWT/PII. Projection coherent
tại lần đọc; exact shared-read isolation setting còn SC-03, không runtime claim.

## 9. Student Registration & Entry Test — API-024–037 / W05,W07–09

Mọi paths mục này dưới /api/v1/students/me. Student NORMAL, resolve Student
từ session; nested rId/bId phải own và đúng parent. Scope kiểm **trước** projection/
token: ngoài ownership404, không leak từ FK error hoặc child lookup. Không
employee đăng ký/đặt thay, grading hay approve qua các routes này. POST đều
CSRF/Origin/no-store; sáu commands UUID, preview/search/read không marker.

### 9.1. Kiểu projection dùng chung

RegistrationStatus = PENDING/TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED/REJECTED/
CANCELLED; BookingStatus = SCHEDULED/COMPLETED/ABSENT/CANCELLED. Không lưu
attempt status mới. EntryScore đề xuất string decimal chính xác hai chữ số
thập phân 0.00–100.00; không float, không thiếu/vắng→0.00. UUID là string
canonical UUID (format parser chi tiết SC-02), không BusinessId alias.

Mọi property liệt kê trong các projections sau là required, trừ khi đánh dấu
O; nullable phải được nêu rõ, không omitted=absence cho context đã đọc.

| Projection | Required properties / nullable / giới hạn |
| --- | --- |
| CourseReference | id BusinessId, code/name string, status ACTIVE/INACTIVE; lịch sử giữ referenced Course. |
| BandReference | id BusinessId, code string; UI label Band theo config, không Course/GradeReport Band tùy ý. |
| EntrySessionView | id BusinessId, startAt/endAt Instant, room string≤30, status OPEN/CLOSED/CANCELLED, effectiveStatus cùng enum, version RowVersion; không roster/grader contact. |
| EntryBookingView | id/registrationId/attemptId/sessionId BusinessId, attemptNumber integer1–2, status BookingStatus, bookedAt Instant, version RowVersion, session EntrySessionView. |
| VersionedReference | id BusinessId, version RowVersion; exact IDs trong manifest, không actor/foreign ID tự cấp quyền. |
| RegistrationSummary | id BusinessId, course CourseReference, status RegistrationStatus, createdAt Instant, admissionConfirmedAt Instant hoặc null, band BandReference hoặc null, invoiceId/classId BusinessId hoặc null, version RowVersion. |

EntryBookingView.session là ca tham chiếu hiện tại (không snapshot room/time),
status/effectiveStatus không ghi CLOSED chỉ vì GET. Cancellation/absence details
ở detail nếu cần chỉ own history, không ảnh hưởng dữ liệu để tính score.
Các refs/membership giữ đủ parent IDs để đối chiếu, không tin client khai liên kết.

StudentRegistrationAction draft allowlist: BOOK_ENTRY_TEST, CANCEL_ENTRY_BOOKING,
RESCHEDULE_ENTRY_BOOKING, CANCEL_REGISTRATION, CONFIRM_REGISTRATION, VIEW_ENTRY_RESULTS,
VIEW_INVOICE, VIEW_CLASS, VIEW_FINALIZED_REPORT. Mutation actions cần current
guards; VIEW_* chỉ khi target có và hiện được đọc. target object allowlist:
registrationId R; bookingId/invoiceId/classId/gradeReportId O BusinessId chỉ
khi action cần. Không “approve/pay/place/grade” capability hoặc arbitrary URL.
Mục8 có nextAction, mục9 detail có allowedActions/disabledReasons; đây là UI
keys, **không** accepted canonical replay actionstrings hoặc quyền từ client.

RegistrationDetail = RegistrationSummary + các required properties:
entry R EntryContext, invoice R StudentInvoiceReference hoặc null,
class R object hoặc null, rejection R object hoặc null, cancellation R object
hoặc null, admissionApprovedAt R Instant hoặc null,
allowedActions R `array<StudentRegistrationAction>`,
disabledReasons R object action→string.
StudentInvoiceReference: id BusinessId, number string, amount Money,
issuedAt Instant, dueDate LocalDate, status UNPAID/PAID/CANCELLED, isOverdue boolean;
không method/transactionReference/paidBy hoặc Payment.
class: classId/assignmentId BusinessId, code string, status PLANNED/OPEN/ONGOING/
COMPLETED/CANCELLED; không classmates/contact directory. Assignment không RV.
rejection: at Instant, reason string1–1000; cancellation: at Instant, reason
string hoặc null (Student không mandatory reason). Giữ lý do/timestamp lịch sử,
không raw audit actor/IP/details_json hoặc hồ sơ staff. Reference IDs khớp
summary; absent nullable không giả giá trị0.

EntryContext: attempts R `array<EntryAttemptView>` đầy đủ tối đa2, order
attemptNumber ASC; bandAssignment R BandAssignmentView hoặc null;
bestAttemptId R BusinessId hoặc null.
EntryAttemptView: id BusinessId, attemptNumber1–2, version RowVersion,
score R EntryScore hoặc null, gradedAt R Instant hoặc null,
completedBookingId R BusinessId hoặc null, currentBooking R EntryBookingView
hoặc null, latestBooking R EntryBookingView hoặc null.
score/gradedAt/completedBookingId cùng null hoặc cùng có; chỉ COMPLETED result.
currentBooking chỉ SCHEDULED; latestBooking theo bookedAt DESC/id DESC, có thể
bằng currentBooking; giữ history qua API-033, không thêm third attempt.
BandAssignmentView: id/bestAttemptId BusinessId, band BandReference, assignedAt
Instant, version RowVersion; score lấy từ best COMPLETED Attempt, không duplicate
bestScore DB. Missing result/ABSENT/CANCELLED không có score mới.

### 9.2. Danh sách/tạo/detail — API-024–026

API-024 GET /registrations: courseId O BusinessId, status O RegistrationStatus,
page/pageSize mục1.2; sort createdAt DESC/id DESC.200 page RegistrationSummary
own mọi history, gồm Course INACTIVE; filter chỉ IDs đúng own referenced domain,
không name/email/Student selector, không bỏ history vì catalogue404.

API-025 POST /registrations: CreateRegistrationRequest chỉ courseId R BusinessId,
UUID header bắt buộc. Reload Course ACTIVE và unfinished predicate/unique tại
commit; không client tuition/Band/status/isUnfinished/new-row RV. Invalid400,
missing Course404, Course đổi INACTIVE hoặc duplicate unfinished409 an toàn.
W05 atomic Registration PENDING/is_unfinished +audit/T23; không copy attempt/
Band hồ sơ cũ, không tạo booking/Invoice/Class. First201 RegistrationCommandResult:
registration R RegistrationDetail mới, operationId R UUID. Completed replay200
current authorized detail/operationId cũ; không tạo row thứ hai.

API-026 GET /registrations/{rId}: id BusinessId, không body,200 RegistrationDetail.
Ownership404 kể cả nested references; guards source từ E01/E02/E03, no-store.
Detail đủ entry/source tokens cho thao tác cancel/reschedule Booking; không
assume RV Registration thay khi mọi child thay. AvailableActions là gợi ý
hiện tại, không authorization/reservation cho mutation sau.

### 9.3. Hủy hồ sơ — API-027–028 / TX06

API-027 POST /registrations/{rId}/cancellation-preview: không business body.
200 RegistrationCancellationPreview properties required:
eligible boolean, blockedReasons `array<string>`, asOfUtc Instant,
effects object {futureBookings: `array<EntryBookingView>`, invoice:
StudentInvoiceReference hoặc null}, manifest CancellationManifest hoặc null.
Blocked→eligible=false/reasons có nội dung/manifest=null; không trả command
context như permission hợp lệ. History không bị hủy không đưa vào effect set;
để xem dùng detail/calendar. Preview không có operationId/audit mutation/UUID.

CancellationManifest draft: expectedVersions R object với registration R RowVersion,
bookings R `array<VersionedReference>`, sessions R `array<VersionedReference>`,
invoice R VersionedReference hoặc null; expectedSet R object với futureBookingIds R
`array<BusinessId>`, invoiceId R BusinessId hoặc null. []/null là expected
absence cụ thể, không omitted hay wildcard. IDs không trùng; bookings exactly
futureBookingIds, sessions exactly distinct source Sessions của effects, invoice
id/version cùng presence; arrays sorted ID để hiển thị, canonical bytes SC-02.
Manifest full-set, **không page/truncate**, effects chỉ own targets đã scoped.

API-028 POST /registrations/{rId}/cancel: body đúng hai required properties
expectedVersions/expectedSet theo CancellationManifest; không reason/status/
courseId/selected-subset mới. Student chỉ PENDING/TESTING/BAND_ASSIGNED và guards
chưa CONFIRMED; hủy future SCHEDULED booking kể cả nguồn CLOSED trước start,
own UNPAID Invoice nếu có, không hủy started/COMPLETED history hoặc PAID.
Parent transition/dependents/unfinished/audit/replay cùng TX06.
Success200 RegistrationCommandResult. Missing/malformed/incoherent manifest400;
versions/state/set membership/time/capacity/paid race409, không silent bỏ child.
Server recompute effects tại commit, không chỉ compare Registration RV; clock
đi qua start dù RV không đổi vẫn409/repreview. Cancelled/rejected hồ sơ không
cho reuse như Registration mới; completed replay không chạy cancellation lần2.

### 9.4. Options và đặt/rebook — API-029–030 / TX02

API-029 GET /registrations/{rId}/entry-test-options query fromDate/toDate O
LocalDate theo ngày VN inclusive, from>to400; bookingId O BusinessId; page/pageSize mục1.2.
Refinement kỹ thuật 10/10/2026 trong phạm vi hoàn tất W07: khi có bookingId,
chỉ đọc ca đích cho Booking thuộc own Registration này, SCHEDULED/current,
Attempt chưa có điểm và source OPEN/chưa bắt đầu/trước CONFIRMED. Khác parent
hoặc Booking không tồn tại404; source bị chặn trả blocked workflow như dưới.
Loại source Session khỏi ca đích; bỏ riêng Booking được thay khỏi overlap guard,
vẫn kiểm mọi Booking/lịch học khác, Teacher ACTIVE/capacity/time như đặt ca.
Không giữ chỗ; API032 kiểm lại toàn bộ source/target guards atomic.
Ca eligible OPEN/chưa start/còn chỗ và Student conflict guards; sort startAt ASC/
id ASC.200 EntryOptions: registrationId R BusinessId, registrationVersion R
RowVersion, eligible R boolean, blockedReasons R `array<string>`, attemptContext
R BookAttemptContext hoặc null, items/page/pageSize/totalCount R theo mục1.2.
items OptionSession = EntrySessionView + availableSlots R integer0–capacity;
đếm tại đọc, không giữ chỗ hoặc hứa capacity còn khi POST. Không Teacher roster,
mọi trạng thái CLOSED không biến thành active slot; no silent choose target.
Blocked workflow→eligible=false/attemptContext=null/items=[]; guard hợp lệ mà
không ca phù hợp→eligible=true/items=[], không tự đổi Course/lượt. Exact filter
date/read-consistency là refinement draft, không thêm query cap bắt buộc mới.

BookAttemptContext properties R: attemptNumber integer1–2, attemptId BusinessId
hoặc null, attemptVersion RowVersion hoặc null, currentBookingId literal null
trong luồng book/rebook không có query bookingId. Trong options đổi lịch,
currentBookingId R BusinessId bằng bookingId, attemptId/version phải cùng có
và thuộc logical Attempt của Booking nguồn. Không tạo logical Attempt mới.
attemptId/version cùng null khi cần tạo logical attempt; existing cùng có.
Lượt server xác định: rebook ABSENT/CANCELLED cùng lượt, lượt2 cần score
COMPLETED lượt1; không cho arbitrary attempt3/replace current Booking bằng book.
Presence/no-current-booking luôn kiểm lại dưới guards/filtered unique.
API030 luôn yêu cầu currentBookingId literal null; options đổi lịch chỉ dùng
để chọn target cho API032, không cho thay Booking qua API030.

API-030 POST /registrations/{rId}/entry-test-bookings: body targetSessionId R
BusinessId, attemptContext R BookAttemptContext đúng context đã đọc,
expectedVersions R object với registration/targetSession R RowVersion.
Không client slotcount/score/createdAt/status/actor. UUID header R, TX02; missing/
malformed/attempt context shape400; source state/attempt membership/presence/
capacity/time/conflict/stale context409; foreign parent404. First201
EntryBookingCommandResult {booking: EntryBookingView R, entry: EntryContext R,
registrationStatus: RegistrationStatus R, registrationVersion: RowVersion R,
operationId: UUID R}. Booking mới SCHEDULED; lần đầu PENDING→TESTING, lượt2
giữ BAND_ASSIGNED. Audit/T23/parent/attempt/booking atomic; replay200 current
authorized result/ref, không thêm attempt/booking hoặc count-cache.

### 9.5. Hủy/đổi một Booking — API-031–032 / TX02

CancelBookingRequest: expectedVersions R object với registration/attempt/booking/
sourceSession R RowVersion. Target binding từ rId/bId; current SCHEDULED và đúng
Attempt/owner, không nhận từ client foreign Student/attempt ID để đổi parent.
API-031 POST /registrations/{rId}/entry-test-bookings/{bId}/cancel:
UUID R; nguồn OPEN/trước start, Registration chưa CONFIRMED và state guard.
Không lý do bắt buộc mới, không score/ABSENT/status tùy chọn.200
EntryBookingCommandResult với booking target đã CANCELLED, updated EntryContext/
parent tokens/operationId. Giữ attempt và booking history, giải phóng slot,
không auto quay Registration về PENDING hoặc xóa Band.

API-032 POST /registrations/{rId}/entry-test-bookings/{bId}/reschedule:
RescheduleBookingRequest có targetSessionId R BusinessId, expectedVersions R
object cùng bốn tokens trên +targetSession R RowVersion. Đề xuất target phải
khác source, cùng source gửi là400 field error targetSessionId (wire refinement,
không new business permission). Nguồn OPEN/trước start và đích OPEN/chưa start/
còn chỗ/không conflict; tạo Booking mới cùng logical Attempt và hủy Booking cũ
**một** TX02, không cancel-then-book hai requests hoặc overwrite FK.
200 RescheduleBookingResult: oldBooking/newBooking R EntryBookingView,
entry R EntryContext, registrationStatus R RegistrationStatus,
registrationVersion R RowVersion, operationId R UUID. No extra attempt.
400 required/encoding;404 wrong nested parent/owner;409 token/state/time/capacity/
conflict. Failed mutation giữ old slot/booking; mandatory audit failure rollback.
Session RV không thay capacity recount; live source/destination locks bắt buộc.
Same completed replay200 safe current projection hoặc refs nếu view không còn
actionable, không tạo Booking thứ ba; no stale-draft overwrite/auto-selected slot.

### 9.6. Own calendar và kết quả — API-033–034

API-033 GET /entry-test-bookings: fromDate/toDate O LocalDate (ngày start VN),
registrationId O BusinessId own, view O enum current/history (draft default
current), page/pageSize mục1.2; sort session.startAt ASC/booking.id ASC.
current chỉ SCHEDULED bookings, không tự biến past ungraded thành ABSENT;
history tất cả statuses trong scoped filter.200 page EntryBookingView.
Foreign registration filter404 nhất quán nested scope, malformed400, empty200.
Không read→ghi CLOSED, roster/filterStudent/contact search hoặc activity heartbeat.

API-034 GET /registrations/{rId}/entry-results: no body/query,200 EntryContext
+registrationId R BusinessId, registrationStatus R RegistrationStatus,
registrationVersion R RowVersion. Attempt arrays full≤2/not paged; score chỉ
COMPLETED, absent/ungraded null, best tie lượt1 và Band live đã lưu theo policy.
Current and latest Booking tách riêng để rebook đúng lượt; không tự grade/change
Band hoặc cho Teacher correction qua Student endpoint. BandAssignment không
có khi chưa completed score; no GradeReport draft values. Clock qua end không
sinh score0/vắng. Foreign parent404; nếu own chưa có kết quả200 context rỗng/
null đúng nghĩa, không404 hoặc fabricated Band.

### 9.7. Xác nhận — API-035–036 / W09

API-035 POST /registrations/{rId}/confirmation-preview: không business body.
200 RegistrationConfirmationPreview: eligible R boolean, blockedReasons R
`array<string>`, asOfUtc R Instant, entry R EntryContext,
manifest R ConfirmationManifest hoặc null. Chỉ BAND_ASSIGNED/Band hợp lệ/no
pending retake; lượt2 SCHEDULED hoặc đã thi chưa chấm chặn kể cả CLOSED.
Blocked không cấp manifest, nhưng own completed result projection vẫn đọc được.

ConfirmationManifest expectedVersions R object:
registration R RowVersion, bandAssignment R RowVersion,
attempts R `array<VersionedReference>`, bookings R `array<VersionedReference>`.
expectedSet R object: bandAssignmentId R BusinessId, bestAttemptId R BusinessId,
attemptIds R `array<BusinessId>`, scheduledBookingIds R `array<BusinessId>`.
Full exact logical attempts và current SCHEDULED Bookings, không pagination
hoặc client chọn best/subset. Version arrays ID match sets; required [] không
bỏ qua presence check; bestAttemptId phải là own completed Attempt và khớp
BandAssignment. Manifest không score/Band threshold/tuition/auth token.
Nguồn/session-time và latest history cần live guard được server resolve từ
bindings, không giả chỉ parent RV bao toàn bộ. Không reservation/preview table.

API-036 POST /registrations/{rId}/confirm: chỉ expectedVersions/expectedSet
ConfirmationManifest, UUID R. Missing/malformed/context shape400, foreign404,
stale membership/token/result/Band/pending-retake/state409. Service recompute
best/Band/guards dưới gates; không tin client chọn best từ input.
Success200 RegistrationCommandResult có admissionConfirmedAt server UTC
trong detail. CONFIRMED/audit/replay atomic, **không** ADMITTED/Invoice/Payment/
Class hoặc profile freeze. Completed replay200 current scoped result/old operation,
không confirm lại. Preview blocked là200; command guard409, không biến thành
403 role mới.

### 9.8. Caller completion — API-037 / recovery

GET /command-results/{requestId}?action=... chỉ caller Student NORMAL/own;
UUID/action required và semantics mục6.4. Sáu family selectors create/cancel
Registration, book/cancel/reschedule Booking, confirm; registry exact strings/
canonical bytes còn SC-02, không lấy Admin allowlist.200 SafeCommandCompletion
với resultMode SAFE_REFERENCE; target R object có registrationId R BusinessId,
bookingId O BusinessId khi command booking (reschedule: oldBookingId và
newBookingId R thay bookingId). Khi target booking/ref không còn current vẫn
history bound own và quyền recheck; không detail snapshot/PII/score/secret.
Missing/outside caller-action-window404 RESULT_NOT_CONFIRMED không proof
failure; malformed400. No marker/request header cho lookup hoặc pending status.

Mất response: GET Registration/EntryContext/calendar hoặc caller result đúng
scope để đối chiếu intent, không auto retry/new key/book tiếp. User retry cùng
intent phải giữ UUID/content/versions-set; khác content cùng key409. Marker chỉ
completed transaction, tối thiểu24h và replay no duplicate audit/effects; target
current view có thể khác commit state. Replay current rights trước stale checks
của mutation **mới**, không bypass Account/session/ownership. Exact completed
response unions/digest/error fixtures toàn55 commands còn SC-02, không hứa
các proposed shapes ở đây đã có implementation.

### 9.9. Ví dụ wire an toàn (refinement draft)

Registration cancellation với chưa có future Booking/Invoice: empty sets là
giá trị đã thấy, không “bỏ qua phụ thuộc”. Token dưới đây minh họa8 byte,
ID/UUID giả; không preview authorization hoặc fixture đã chạy backend.

```json
{
  "expectedVersions": {
    "registration": "AAAAAAAAAAE=",
    "bookings": [],
    "sessions": [],
    "invoice": null
  },
  "expectedSet": {
    "futureBookingIds": [],
    "invoiceId": null
  }
}
```

Caller-result Admin reset chỉ safe refs, secret không thể đọc lại:

```json
{
  "outcome": "COMPLETED",
  "operationId": "ea6cfb65-0fd7-4a0e-b66d-6a6d52912f6b",
  "resultMode": "SECRET_RESULT_UNAVAILABLE",
  "target": { "accountId": 101 }
}
```

## 10. Admissions — API-038–045 / W10–11 / F13–14

Các paths dưới /api/v1. API-038–044 chỉ Admissions NORMAL; không AcademicAffairs/
Teacher/Admin dùng namespace để có quyền tuyển sinh. API-045 là shared lookup
có cặp role/action riêng ở mục10.6. Không Student selector để nhân sự tạo hồ sơ,
không sửa Profile/entry score/Band/training data hoặc xuất/in hồ sơ.
Nguồn: requirements/ADR-083/144, WORKFLOW_SPEC W10–11, DATABASE_SPEC T09/T13/
T20/T22–24/TX04–06. Keys/shapes/sort/status dưới đây là refinement dự thảo.

### 10.1. Read projections theo Admissions

Mọi property nêu dưới đây required trừ O; nullable phải hiện rõ. Không dump
RegistrationDetail Student hoặc Finance DTO để “ẩn field” ở frontend.

| Type | Properties / giới hạn |
| --- | --- |
| StudentContactReference | studentId BusinessId, fullName/email/phone string hiện tại; phone Student không nullable. Không DOB/address/Account credentials. |
| PlacedClassReference | classId/assignmentId BusinessId, code string≤30, status PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED. Không peers/Teacher contact/Attendance/grades. |
| AdmissionsRegistrationSummary | id BusinessId, student StudentContactReference, course CourseReference mục9.1, status RegistrationStatus, createdAt Instant, admissionConfirmedAt/admissionApprovedAt Instant hoặc null, band BandReference hoặc null, invoice AdmissionsInvoiceReference hoặc null, class PlacedClassReference hoặc null, version RowVersion. |
| AdmissionsInvoiceReference | id BusinessId, number string HD-YYYY-NNNNNNNN, amount Money, status UNPAID/PAID/CANCELLED; không Payment object/method/reference/confirmer/paidAt. |
| AdmissionsRegistrationDetail | Summary +entry EntryContext mục9.1, rejection {at: Instant, reason: string1–1000} hoặc null, cancellation {at: Instant, reason: string hoặc null} hoặc null, allowedActions `array<AdmissionsRegistrationAction>`, disabledReasons object action→string. |

EntryContext là own-target entry results/context theo phép đọc Admissions của
ADR-083, không khẳng định caller là Student và không mở Student /me routes.
Chỉ completed scores; thiếu/vắng vẫn null, không đào tạo DRAFT/SUBMITTED/
RETURNED/FINALIZED. Contact là current Account fields, không snapshot contact
tại đăng ký hay hồ sơ cá nhân toàn trung tâm. History CANCELLED/REJECTED vẫn
xem được với nguồn/cancellation/rejection đã lưu, không xóa Course INACTIVE.

AdmissionsRegistrationAction draft: APPROVE, REJECT, CANCEL. Đây là UI keys,
không T23 canonical action_code hoặc quyền truyền từ client. disabledReasons
chỉ keys trong allowlist, scope-safe; command luôn live recheck.
AdmissionsInvoiceReference chỉ tiền/state/reference, không dùng full Finance
projection cho detail/preview/replay. Assignment không RV theo E03.

### 10.2. Search và detail — API-038–039

API-038 POST /admissions/registrations/search: AdmissionsSearchRequest allowlist:

| Field | Required / type | Validation / semantics dự thảo |
| --- | --- | --- |
| name, email, phone | O string mỗi trường | Name NFC/trim≤100, email ASCII/trim≤254, phone canonical nếu có; blank→omitted. Name/email literal contains parameterized, không SQL wildcard pattern; phone exact. Search không bắt name/email là hồ sơ/email đầy đủ. |
| courseId | O BusinessId | Referenced Course, gồm INACTIVE/history; không Student directory filter. |
| status | O RegistrationStatus | Đủ bảy state, không default chỉ CONFIRMED. |
| createdFromDate, createdToDate | O LocalDate mỗi trường | Ngày tạo Registration VN inclusive; một bound có thể omitted; from>to400, không kỳ hóa đơn/doanh thu. |
| sort | O enum CREATED_DESC/CREATED_ASC | Default CREATED_DESC; createdAt và id cùng hướng, không arbitrary property/expression. |
| page, pageSize | O integer | Mục1.2, default1/20/max100. |

200 page AdmissionsRegistrationSummary, empty200. Dates convert start-of-day
VN thành UTC half-open range; không implicit history cutoff hoặc query-as-of
archive. Name/email/phone body/RAM, no URL/storage/full request logs.
SP01 thật nhận validated filters/sort/pagination; actor/role từ verified service,
không request field. Signatures/mapping chi tiết còn SC-03, không SQL đã chạy.

API-039 GET /admissions/registrations/{id}: BusinessId path, không body/query
business;200 AdmissionsRegistrationDetail. Missing/scoped-out404, invalid input400,
session401/role-restricted403. No-store và cùng projection policy ở mọi source.
Khả dụng trên detail không là reservation; không tự chuyển CONFIRMED→ADMITTED
qua GET, không cho Admissions ghi score/Profile hoặc đọc Finance Payment.

### 10.3. Duyệt cùng Invoice — API-040–041 / TX04

API-040 POST /admissions/registrations/{id}/approval-preview: không business
body hoặc UUID.200 AdmissionApprovalPreview:
registrationId R BusinessId, eligible R boolean, blockedReasons R `array<string>`,
asOfUtc R Instant, course R ApprovalCourseView, band R BandReference hoặc null,
manifest R ApprovalManifest hoặc null.
ApprovalCourseView: id BusinessId, code/name string, status ACTIVE/INACTIVE,
tuition Money, version RowVersion (đều R). Dữ liệu hiện tại để duyệt, không
Course selection ACTIVE-only hoặc permission sửa cấu hình.
Blocked workflow→eligible=false/reasons/manifest=null; không cấp invoiceNumber/
issuedAt/dueDate/operationId hoặc giữ chỗ. Giá trong preview chưa Invoice snapshot.

| ApprovalManifest property | Required / type | Binding / absence |
| --- | --- | --- |
| expectedVersions | R object | registration/course/bandAssignment đều R RowVersion. |
| expectedSet | R object | courseId/bandAssignmentId đều R BusinessId; invoiceId R literal null. |
| expectedSet.invoiceId | R literal null | Đã thấy chưa có Invoice, không omitted=unknown hoặc lệnh xóa Invoice. |

Binding IDs là context đã thấy, không quyền chọn Course/Band khác cho Registration.
Manifest chỉ khi CONFIRMED/Band hợp lệ; Course INACTIVE không tự chặn hồ sơ đang
có. Giá dương hiện tại và absence Invoice phải recheck tại commit.

API-041 POST /admissions/registrations/{id}/approve: body đúng expectedVersions/
expectedSet ApprovalManifest, UUID Idempotency-Key R. Không client amount/number/
issue year/due/issuedAt/status/actor. Missing/malformed context400; stale version/
state/Band/Invoice presence/price hoặc context binding đổi409, không silent chốt
giá mới sau preview. Missing target404; same role/session/CSRF policy chung.

Success200 AdmissionsRegistrationCommandResult: registration R
AdmissionsRegistrationDetail, operationId R UUID. Detail đã ADMITTED và invoice
reference UNPAID. Backend lấy tuition server tại issuance, SP07 allocate năm
ngày VN issuance và số HD-YYYY-NNNNNNNN trong **cùng TX04**; Invoice amount/
issuedAt/dueDate immutable, due=ngày phát hành VN+7. Không client cấp số hoặc
giả năm từ timezone laptop. Counter/Invoice/state/audit/T23 all-or-none;
allocator overflow/failure không partial ADMITTED hoặc reset/tái dùng số đã phát.
Không thêm Invoice-create API hoặc frontend approve-then-create transaction.

Completed replay200 dựng current Admissions detail/operationId cũ sau current
rights, không phát số/Invoice thứ hai hoặc so RV cũ như new mutation. Invoice
có thể PAID/CANCELLED ở current view; không gọi đó là snapshot UNPAID khi commit.

### 10.4. Từ chối — API-042 / W10 atomic

POST /admissions/registrations/{id}/reject: RejectRegistrationRequest reason R
string NFC/trim1–1000 code points, giữ xuống dòng; expectedVersions R object
với registration R RowVersion. UUID R. Không input state/actor/Band/score hoặc
chọn Course khác. CONFIRMED/có Band theo W10, không reject ADMITTED/PAID/history.
Required/reason/token shape400, stale/state/concurrent approve-cancel409; target404.

200 AdmissionsRegistrationCommandResult: REJECTED/reason/server timestamp trong
detail, is_unfinished=false và audit/T23 cùng save unit W10. Reason được Student
chủ hồ sơ xem theo accepted policy; không biến thành private note hay trộn audit
PII. Giữ score/Band/history, không tạo Invoice/Payment/Class. Không tự thêm TX14
vào E03 chỉ để đặt tên transaction. Mandatory audit failure rollback whole intent;
replay current rights/no effects như approve, không duplicate rejection audit.

### 10.5. Hủy hồ sơ tuyển sinh — API-043–044 / TX06

API-043 POST /admissions/registrations/{id}/cancellation-preview: không business
body/UUID.200 AdmissionsCancellationPreview:
registrationId R BusinessId, eligible R boolean, blockedReasons R `array<string>`,
asOfUtc R Instant, effects R object {futureBookings R `array<EntryBookingView>`,
invoice R AdmissionsInvoiceReference hoặc null}, manifest R CancellationManifest
hoặc null theo mục9.3.
Full exact SCHEDULED future Booking effects, distinct source Session tokens và
Invoice ID/version hoặc explicit absence; không dùng paginated search làm set.
Manifest chứa versions/IDs, không Payment metadata hoặc StudentProfile.

Chỉ CONFIRMED hoặc ADMITTED có Invoice UNPAID theo guards W10; CONFIRMED chưa
có Invoice là absence hợp lệ. Blocked→manifest=null và lý do đúng scope; PAID
không cho hủy hoặc gợi ý refund. Clock/time và effect membership không đóng băng.

API-044 POST /admissions/registrations/{id}/cancel:
CancelAdmissionsRegistrationRequest reason R string1–1000 theo mục10.4,
expectedVersions/expectedSet R CancellationManifest. UUID R. Không phép bỏ reason,
chọn subset Booking, bỏ Invoice token/absence hoặc ghi Payment mới.
Tại commit recompute future SCHEDULED bookings/source sessions, no Payment/
Invoice UNPAID, state và exact sets; đi qua start/membership thay đổi409 dù
Registration RV chưa đổi. Hủy future kể cả Session CLOSED khác Student hủy một
Booking; started/results/Band/history giữ, chặn grading hồ sơ đã hủy theo W10.

200 AdmissionsRegistrationCommandResult CANCELLED; Registration/unfinished/
Invoice cancellation/future Bookings/released capacity/provenance/audit/T23 cùng
TX06 và operationId. Không PAID cancellation, delete history, transfer placement,
refund hoặc reason snapshot ngoài accepted object/audit bundles. Missing/invalid
shape400, target404, stale/state/payment/start/effect race409.
Collection-vs-cancellation serialize/live recheck: chỉ một outcome hợp lệ,
không Invoice PAID cùng Payment mà Registration bị hủy theo intent UNPAID cũ.

### 10.6. Shared caller result — API-045 / không union quyền

GET /admissions/command-results/{requestId}?action=...: UUID/action required,
no body hoặc actor selector. Dùng SafeCommandCompletion mục6.4, resultMode
SAFE_REFERENCE; lookup không trả Registration/Session/Payment/PII snapshot.

| Verified caller | Families allowed | target allowlist trong200 completion |
| --- | --- | --- |
| Admissions NORMAL | approve/reject/cancel Registration, API-041/042/044 | registrationId R BusinessId; invoiceId O BusinessId chỉ khi command có Invoice reference đã commit. |
| AcademicAffairs NORMAL | create/edit-schedule/open/close/cancel/change-grader EntryTestSession, API-055/057/058/059/061/064 | sessionId R BusinessId, không Student contact/results/Invoice amount/Payment. |

Cùng path **không** cho Admissions quản lý Session hoặc AcademicAffairs duyệt
Registration. Dispatch theo verified role +allowlisted action family; target
current read scope recheck, không caller từ JSON/string role. Registry exact
actionstrings/canonical bytes còn SC-02; bảng này là families, không T23 string.

UUID/action shape/missing/unknown family400; family thuộc role khác403, không
probe marker để tiết lộ completion. Marker không tồn tại/hết hạn/not caller của
valid own-role family404 RESULT_NOT_CONFIRMED: không proof thất bại/rollback.
200 completion chỉ IDs/outcome/operationId, không raw request digest/expiry.
Không polling/auto new UUID/auto-create invoice để đoán kết quả. Schemas Session
commands API-054–073 vẫn chưa được đặc tả chỉ vì lookup đã có role pair.

## 11. Invoice & Payment — API-046–053 / W11–12 / F10,F28–29

Tất cả paths dưới /api/v1. Student NORMAL chỉ own Invoice API-046–047;
Accountant NORMAL API-048–053. Admissions chỉ projection mục10, các role khác
không được Finance search/Payment metadata. POST/PUT CSRF/Origin/no-store;
search/preview không replay, collect/correct UUID R. Không gateway/partial/
refund/report CSV/directory export/in nhân sự hoặc Dedicated PDF endpoint.

### 11.1. Invoice/Payment allowlists theo vai trò

InvoiceStatus = UNPAID/PAID/CANCELLED, PaymentMethod = CASH/TRANSFER. Các property
trong bảng là R; nullable đã ghi rõ. InvoiceNumber string HD-YYYY-NNNNNNNN do
server phát; không là editable field. Money/Instant/LocalDate theo mục1, không
JavaScript float/round/backdate. Compact refs ở mục8–10 không thay full views này.

| Type | Properties / source / giới hạn |
| --- | --- |
| InvoiceStudentView | id/registrationId BusinessId, course CourseReference, number InvoiceNumber, amount Money, issuedAt Instant, dueDate LocalDate, status InvoiceStatus, isOverdue boolean, overdueDays integer≥0. Own T20 +current referenced Course, không Payment/method/reference/confirmer/paidAt. |
| InvoiceStudentDetail | InvoiceStudentView +currentStudentName string, paymentGuidance object {message: string}. Current Account name, không DOB/address/profile dump; guidance chỉ liên hệ/thu bên ngoài, không invented bank account/integration. |
| FinancePaymentView | id/invoiceId BusinessId, amountPaid Money, method PaymentMethod, transactionReference string hoặc null, confirmedBy PaymentActorReference, paidAt Instant, lastCorrection PaymentCorrectionReference hoặc null, version RowVersion. Accountant-only. |
| PaymentActorReference | accountId BusinessId, fullName string hiện tại của Accountant; không email/phone/credential hoặc quyền lookup Account toàn hệ thống. Immutable actor ID, current name có thể đổi, không historical-name snapshot. |
| PaymentCorrectionReference | by PaymentActorReference, at Instant, reason string1–1000 từ latest correction bundle; full correction history vẫn audit, không new timeline endpoint. |
| InvoiceAccountantView | InvoiceStudentView +student StudentContactReference, registrationStatus RegistrationStatus, payment FinancePaymentView hoặc null, versions object {registration: RowVersion, invoice: RowVersion}, allowedActions `array<FinanceAction>`, disabledReasons object action→string. Không entry/grades/Attendance. |
| InvoiceAccountantDetail | Cùng InvoiceAccountantView; có đủ Payment/current contact/tokens. Không vì route detail mà thêm raw EF/audit/security fields. |

InvoiceStudentView có tên Course hiện tại và tiền snapshot bất biến, không
current Course tuition thay amount. Student detail không dùng FinancePaymentView
kể cả PAID; status PAID đúng dữ liệu không cho quyền xem confirmer/reference.
Student list/detail không token sửa Invoice/Payment; Accountant versions phục vụ
preview/commands, không credential hoặc quyền từ client.

isOverdue/overdueDays từ FN06 với today_vn server chung cho snapshot:
UNPAID và dueDate<today_vn thì true và số ngày chênh; còn lại false/0. Đến ngày
hạn chưa overdue; không OVERDUE status, tăng amount/phạt/khóa Account hay auto
cancel Registration. Không suy client timezone hoặc timeout là đã thu tiền.
PAID↔Payment existence phải coherent theo W12; UNPAID/CANCELLED có payment=null,
không read merge hai trạng thái từ hai lần query khác nhau.
FinanceAction draft: COLLECT/CORRECT_PAYMENT_METADATA; key UI không T23 actioncode,
live guard luôn recheck. Correction metadata không thêm role override.

### 11.2. Student own list/detail/print — API-046–047

API-046 GET /students/me/invoices query registrationId O BusinessId own,
status O InvoiceStatus, page/pageSize mục1.2; fixed issuedAt DESC/id DESC.
Resolve owner session, không studentId/name/email/phone/invoiceNumber search
query hoặc staff đọc /me.200 page InvoiceStudentView, empty200 kể cả chưa
được duyệt; foreign Registration filter404, invalid input400.
Lịch sử CANCELLED/PAID và Course INACTIVE vẫn hiện theo scope; không chỉ UNPAID.

API-047 GET /students/me/invoices/{id}: positive BusinessId, không business body/
query,200 InvoiceStudentDetail; missing/non-own404 cùng message; session401/
role-restricted403. Current name đọc từ Account không Invoice contact snapshot.
paymentGuidance.message là hướng dẫn thu bên ngoài chung, không client chọn
provider/reference/Accountant hoặc website ngân hàng.

Nút in F10 fetch API-047 fresh authorized thành công trước dựng A4; lỗi/mất quyền
thì không in cached data như mới. Chỉ số/tên current/khóa/amount snapshot/ngày/
hạn/state và overdue cảnh báo theo FE17; không Payment fields hoặc vẽ giả receipt/
signature/stamp. UNPAID “Chưa thanh toán — không phải xác nhận đã thu tiền”,
CANCELLED “Đã hủy”, PAID “Đã thanh toán”. Bấm in không mutate PAID/idle/operation.
Không API PDF, file/signed print token/export/in cho Admissions/Accountant/Admin.
Browser print-to-PDF không là feature PDF API mới.

### 11.3. Accountant search/detail — API-048–049

API-048 POST /finance/invoices/search: FinanceInvoiceSearchRequest allowlist:

| Field | Required / type | Validation / semantics dự thảo |
| --- | --- | --- |
| invoiceNumber | O string | Trim/ASCII tối đa16; đề xuất literal prefix để tìm HD-/năm/số chưa gõ đủ, không bắt search đủ pattern issuance. Không SQL wildcard/client expression. Blank→omitted. |
| courseId | O BusinessId | Course referenced, gồm INACTIVE; không filter đào tạo/entry-result. |
| name, email, phone | O string mỗi trường | Current Student contact, normalization/match như mục10.2; PII chỉ body/RAM. |
| status | O InvoiceStatus | Mọi state nếu omitted, gồm CANCELLED; không default chỉ UNPAID. |
| issueFromDate, issueToDate | O LocalDate mỗi trường | Ngày phát hành VN inclusive/optional từng bound/from>to400; không paidAt hoặc revenue period. |
| overdueOnly | O boolean | Default false; true∧các filters khác, không tự đổi status filter hoặc status Invoice. |
| sort | O enum ISSUED_DESC/ISSUED_ASC | Default ISSUED_DESC, issuedAt/id cùng hướng; không arbitrary sort/SQL property. |
| page, pageSize | O integer | Mục1.2 default1/20/max100. |

status=PAID hoặc CANCELLED với overdueOnly=true là filter hợp lệ/empty200,
không silently override status.200 page InvoiceAccountantView, totalCount cùng
scoped filters/snapshot, không lấy tổng bằng cộng trang hoặc cộng Payment rows.
No new export/print directory action trên list. VW06 projection một Invoice/tối
đa một Payment; không join entry attempts/cohort khiến nhân tiền. FN06 overdue
call thật theo today_vn, không chỉ viết hàm để rubric. SQL bindings/settings/
performance và full signatures còn SC-03, không runtime evidence.

API-049 GET /finance/invoices/{id}: BusinessId, no business body/query,200
InvoiceAccountantDetail đúng allowlist. Missing404, input400, session401/role-
restricted403. Invoice+Payment+Registration/token dùng cùng consistent snapshot
read ở bản chính§8/ADR-144; không one-by-one GET rồi trộn thời điểm.
Read-only transaction không giữ qua người dùng confirmation; commands reload
under accepted E03 gates. Audit/secret/Student Profile không proxy vào detail.

### 11.4. Preview và thu đủ tiền bên ngoài — API-050–051 / TX05

API-050 POST /finance/invoices/{id}/collection-preview: không business body/
UUID.200 CollectionPreview:
invoiceId R BusinessId, eligible R boolean, blockedReasons R `array<string>`,
asOfUtc R Instant, amount R Money, manifest R CollectionManifest hoặc null.
Current UNPAID/Registration ADMITTED hợp lệ/no Payment→eligible và manifest;
PAID/CANCELLED/invalid workflow→false/reasons/null, không gợi ý “thu lần nữa”.
Preview không phát Payment ID/paidAt/operationId, không giữ Invoice hoặc
chứng minh đã nhận tiền, không double-confirmation UI mới.

CollectionManifest expectedVersions R object với registration/invoice R RowVersion;
expectedSet R object với registrationId R BusinessId, paymentId R literal null.
Binding đúng Invoice→Registration; null là đã thấy chưa có Payment, không
omitted/nhận Payment ID người khác. Amount preview server chỉ full snapshot, không
front-end tính discount/penalty/change amount.

API-051 POST /finance/invoices/{id}/collect: CollectionRequest:

| Field | Required / type | Constraint |
| --- | --- | --- |
| amount | R Money | Actual externally reconciled full amount; digits string nguyên VND/range mục1; phải bằng Invoice.amount hiện hợp lệ, không number/partial/round. |
| method | R PaymentMethod | Chỉ CASH hoặc TRANSFER, không GATEWAY/card/custom enum. |
| transactionReference | O string hoặc null | NFC/trim≤100 code points; optional blank/null/omitted→NULL cho CASH; TRANSFER phải nonempty. Giữ case/dấu câu, không bank pattern hoặc global unique. |
| expectedVersions, expectedSet | R object mỗi trường | Đúng CollectionManifest, không user-declared Invoice state/actor/paidAt. |

UUID R; không thêm required reason cho collect. Required/type/range/reference/
money mismatch400 field error
amount nếu context còn hợp lệ; stale versions/state/absence/parent-binding/paid-
cancel race409 trước khi nói amount hiện tại. Không lỗi409 tiết lộ Invoice
người khác hoặc dựa bank reference là replay match.

Success201 CollectionCommandResult: invoice R InvoiceAccountantDetail,
payment R FinancePaymentView, operationId R UUID. Hai projections cùng Payment/
Invoice IDs/versions; paidAt/confirmer server, Invoice PAID +Payment amount bằng
snapshot +audit/T23 cùng TX05. Actor từ session không body/selected Accountant.
Không set actual bank receipt date/ngày xác nhận lùi/Invoice issuedAt mới.
Rollback khi mandatory audit hoặc Payment/Invoice save lỗi; không success nửa
Invoice PAID thiếu Payment. Unique one Payment/Invoice/domain guard vẫn bảo vệ
ngoài replay window, không reference unique. Replay200 current authorized
CollectionCommandResult/old operationId; response union/ref chi tiết còn SC-02.
Mất read scope thì không trả projection/ref ngoài quyền, không dùng marker
để vượt scope. Không Payment lần2/SP07/phát hóa đơn. Unknown không kết luận
đã/chưa thu và không auto-payment/gateway check/new intent.

### 11.5. Sửa method/reference — API-052 / W12 atomic

PUT /finance/payments/{id}/metadata: PaymentMetadataCorrectionRequest
method R PaymentMethod, transactionReference R string hoặc null, reason R
string NFC/trim1–1000, expectedVersions R object với payment/invoice R RowVersion.
Mục11.4 reference rule giữ nguyên, TRANSFER không thể null/blank; CASH nullable.
Ở PUT property transactionReference phải có: explicit null/blank là clear
optional CASH reference, omitted400, không accidental PATCH hoặc giữ draft cũ.
Đây là wire refinement dự thảo; không thay policy optional CASH/mandatory TRANSFER.

UUID R; no amount/invoiceId/registrationId/paidAt/confirmedBy/status/lastCorrectedAt
editable fields. Resolve current Payment→Invoice server, PAID/same Payment và
versions recheck; missing input/shape/reason/reference400, missing target404,
stale/state/concurrent correction409. Không đòi client Registration RV mới
chưa trình; live parent guards/gates theo E03/W12 vẫn giữ.

Success200 PaymentMetadataCommandResult: invoice R InvoiceAccountantDetail,
payment R FinancePaymentView, operationId R UUID. Method/reference/latest
correction bundle +mandatory old-new-actor-time-reason audit/T23 atomic; giữ
same Payment/amount/paidAt/confirmedBy/link/Invoice PAID. LastCorrection không
thay toàn audit history hoặc quyền Admin audit. Không refund/delete/reopenInvoice/
tạo Payment mới; metadata correction không sửa doanh thu hoặc ngày kỳ revenue.
Completed replay200 current permitted view/operationId cũ, no effects/history
mới. Unknown đọc lại API-049 đúng Invoice để đối chiếu, không auto overwrite
metadata từ RAM hoặc tự xác nhận thu lại.

### 11.6. Accountant caller result — API-053 / recovery

GET /finance/command-results/{requestId}?action=...: Accountant NORMAL only,
UUID/action R, no caller selector/body. Families collect/correct-payment-metadata;
registry strings/digest còn SC-02.200 SafeCommandCompletion mục6.4 với resultMode
SAFE_REFERENCE, target R {invoiceId: BusinessId R, paymentId: BusinessId R};
không finance/person/method/reference/secret snapshot hay audit privileges.

Malformed/missing/unknown family400; role-restricted403; valid own-role family
missing/expired/not actor marker404 RESULT_NOT_CONFIRMED. Absence không proof
rollback, có thể intent còn commit/marker đã cleanup/outside caller-action.
Sau timeout đọc API-049/caller result trước khi user quyết định tiếp; không tự
collect lần nữa, auto retry hoặc dùng mã giao dịch làm evidence. Retry chủ đích
cùng intent giữ UUID/content/versions/absence; khác content cùng key409.
Completion committed cùng effects/audit; replay hiện recheck Account/session/
role/current rights trước stale checks của mutation mới. Marker≥24h không bảo
đảm dedup vô hạn, one Payment/domain state vẫn giữ. Không receipt/history endpoint
mới hoặc permanent PII response cache.

### 11.7. Ví dụ wire và mục tiêu kiểm chứng — chưa chạy ứng dụng

Thu chuyển khoản theo context đã xem; ID/token dưới đây giả, Body không actor/
paidAt/InvoiceNumber và amount là string, không bằng chứng ngân hàng.

```json
{
  "amount": "1500000",
  "method": "TRANSFER",
  "transactionReference": "DEMO-REF-001",
  "expectedVersions": {
    "registration": "AAAAAAAAAAE=",
    "invoice": "AAAAAAAAAAI="
  },
  "expectedSet": {
    "registrationId": 201,
    "paymentId": null
  }
}
```

Sửa metadata sang CASH, xóa reference optional một cách explicit; paidAt/amount
không sửa. Lý do là dữ liệu nghiệp vụ, không log cả raw request.

```json
{
  "method": "CASH",
  "transactionReference": null,
  "reason": "Đối chiếu lại phương thức thu.",
  "expectedVersions": {
    "payment": "AAAAAAAAAAM=",
    "invoice": "AAAAAAAAAAQ="
  }
}
```

Mục tiêu **sau triển khai**: projection parity ba vai trò ở list/detail/preview/
replay; approved-no-Invoice hoặc PAID-no-Payment không xuất hiện; SP01/SP07/
VW06/FN06 actual callers; price-change stale preview409, allocator/audit failures
rollback; midnight VN/due+7/overdue days; collect-vs-cancel/correct races; exact
decimal18 string and full amount; TRANSFER reference/nonunique/CASH clear/PUT
omission400; immutable money/time/confirmer; caller-action pair isolation API-045;
missing marker not failure; fresh Student print/no Payment leakage. Chỉ parse
examples và kiểm cấu trúc tài liệu ở lượt này, không contract/DB/browser tests.

## 12. Giáo vụ quản lý ca thi — API-054–065 / W06 / F16–17

Mọi paths dưới /api/v1; chỉ AcademicAffairs NORMAL, không role Admissions dù
namespace là /admissions. Sáu commands UUID: create/schedule/open/close/cancel/
change-grader. Lookup dùng API-045 đã đặc tả mục10.6, không thêm endpoint hoặc
union quyền. Nguồn ADR-083/133/145, requirements, W06 và E03 T10–13/T22–23.
New keys/shapes/filters/context dưới đây vẫn **refinement dự thảo**.

### 12.1. Types/projections và thời gian

| Type | Required properties / nullable / giới hạn |
| --- | --- |
| MinuteTime | JSON string HH:mm, giờ24h/ngày thật; không seconds/offset. SessionDate là LocalDate VN, date/startTime/endTime convert UTC cùng ngày, start<end; UTC response theo Instant mục1. |
| GraderReference | accountId BusinessId, displayName string current Account name; không email/phone/hash/Profile/Account entity. |
| GraderOption | GraderReference +eligible boolean, blockedReasons `array<string>`; chỉ tình trạng chọn đúng scope, không Teacher calendar/PII của conflict. |
| EntryTaskCounts | scheduled/completed/absent/cancelled integer≥0: số Booking rows từng status, không số Student/Attempt distinct; trên toàn Session trong scope, không đếm trang. |
| AcademicSessionView | id BusinessId, date LocalDate, startTime/endTime MinuteTime, startAt/endAt Instant, room string mã chuẩn1–30, capacity integer1–20, status/effectiveStatus OPEN/CLOSED/CANCELLED, grader GraderReference, graderAvailable boolean, availabilityReasons `array<string>`, scheduledBookingCount integer≥0, availableSlots integer≥0, taskCounts EntryTaskCounts, version RowVersion. |
| AcademicSessionDetail | AcademicSessionView +cancellation {at: Instant, reason: string1–1000} hoặc null, hasBookingHistory boolean, allowedActions `array<AcademicSessionAction>`, disabledReasons object action→string. |

Mọi property trên R, trừ nullable đã nêu. Date/time và UTC instants là cùng
mốc lịch server, không timezone laptop/client. Room trim/uppercase ASCII
A–Z/0–9/_/- theo ADR-080, không bảng Room/cấp quyền sửa phòng mới.
graderAvailable cảnh báo Account ACTIVE/availability theo nhiệm vụ hiện tại,
không tự bỏ assignment/đổi người khi Account LOCKED/DISABLED.
scheduledBookingCount là SCHEDULED count theo E03, availableSlots=max(0,
capacity-count) tại lần đọc; không booked_count cache/reservation hoặc enum FULL.
COMPLETED/ABSENT/CANCELLED vẫn lịch sử, không biến count-history thành chỗ giữ
mới. Capacity chỉ input lúc create trong inventory này, không thêm capacity edit.

EffectiveStatus dùng FN01(status,startAt,asOfUtc): từ start ca chưa hủy CLOSED;
GET không ghi stored status. CLOSED không khóa post-end Teacher work; CANCELLED
khác CLOSED. Time tới cutoff có thể đổi guard mà RowVersion không đổi.
AcademicSessionAction draft: EDIT_SCHEDULE, OPEN, CLOSE, CANCEL, CHANGE_GRADER,
VIEW_ROSTER. Chỉ UI keys, không T23 canonical actionstrings/permission input.
hasBookingHistory tính any Booking kể cả đã hủy; không current count=0 suy
“chưa từng có Booking”. Detail không embedded paginated Student roster.

### 12.2. List/detail và create — API-054–056

API-054 GET /admissions/entry-test-sessions query fromDate/toDate O LocalDate VN
inclusive (ngày start), effectiveStatus O enum OPEN/CLOSED/CANCELLED,
graderId O BusinessId, sort O START_ASC/START_DESC (draft default START_ASC,
startAt/id cùng hướng), page/pageSize mục1.2. Một bound date có thể omitted,
from>to400; không implicit history cutoff/PII search/query-as-of input.
200 page AcademicSessionView, empty200; counts/effective state cùng server
asOfUtc/snapshot, không tự close hoặc mở ca. Không thêm role/Student selector.

API-055 POST /admissions/entry-test-sessions: CreateEntrySessionRequest date/
startTime/endTime/room/graderId/capacity R theo mục12.1. graderId BusinessId
Teacher ACTIVE đúng role; server validate same-day/minute/start<end/capacity1–20
và room/Teacher overlaps theo EC:TIMETABLE. Ca mới CLOSED theo W06;
không client status/startAtUTC/actor/sessionId/booking_count/RowVersion row mới.
ADR-157/RF01: tạo chỉ startAt>serverNow khi kiểm tra lưu trong transaction;
equality/passed409, malformed date/time400, no backfill. Preview/RV không freeze clock.
UUID R; shape/range/date/type400; candidate thiếu404, role/state/resources/time
guard hoặc concurrent conflict409 đúng scope, không leak lịch Student khác.
First201 AcademicSessionCommandResult: session R AcademicSessionDetail mới
CLOSED, operationId R UUID. Session/audit/T23 cùng save unit W06; không auto
OPEN/book Student, tạo lớp hoặc tự gọi Teacher grading.

API-056 GET /admissions/entry-test-sessions/{sId}: BusinessId, không body/query
business,200 AcademicSessionDetail. Missing404/input400/session401/role-
restricted403 chung. Current grader unavailable vẫn giữ name/reference/history/
warning; không bất ngờ chuyển assignment hoặc gọi Admin Account endpoint.

### 12.3. Sửa lịch/mở/đóng — API-057–059

API-057 PUT /admissions/entry-test-sessions/{sId}/schedule:
EntrySessionScheduleRequest date/startTime/endTime/room R, expectedVersions R
object với session R RowVersion. Không capacity/grader/status/result fields.
UUID R. Chỉ pre-start và **chưa từng** Booking, kể cả CANCELLED Booking;
recheck room/Teacher/current resources và same-day/minute/time tại commit.
400 shape/required;404 target;409 version/state/time/history/overlap race.
200 AcademicSessionCommandResult; date/time/room/audit/T23 cùng transaction,
không sửa lịch hẹn cũ hoặc push Student sang Session mới bằng update FK.

API-058 POST /admissions/entry-test-sessions/{sId}/open và API-059 POST
/admissions/entry-test-sessions/{sId}/close: body đúng expectedVersions R object
session R RowVersion, UUID R; không reason mandatory mới/capacity/actor/state.
Open/reopen CLOSED chưa start, resources/ACTIVE grader/occupancy≤capacity;
CANCELLED hoặc đã start409, đầy chỗ vẫn OPEN không FULL. Close chỉ OPEN pre-start,
giữ mọi Booking/Attempt/Band; không cancel bookings hoặc Teacher grading.
200 AcademicSessionCommandResult, status/state/security guards live/audit/T23
all-or-none;400 token/shape,404 missing,409 stale/window/guard. Không GET tự ghi
CLOSED; next permitted write persist effective closure theo bản chính, không
gán success cho OPEN/close trái cutoff.

Replay cả create/schedule/open/close trả200 current authorized session/old
operationId, không effects/audit lần2. Token/status current view có thể khác
lúc commit; không state snapshot cache hoặc tự reset ca sau unknown result.

### 12.4. Preview/hủy Session — API-060–061

API-060 POST /admissions/entry-test-sessions/{sId}/cancellation-preview:
không business body/UUID.200 SessionCancellationPreview: sessionId R BusinessId,
eligible R boolean, blockedReasons R `array<string>`, asOfUtc R Instant,
effects R `array<AcademicRosterRow>` (mục12.7) chỉ SCHEDULED bookings sẽ hủy,
manifest R SessionMembershipManifest hoặc null.
Pre-start/non-CANCELLED; blocked→manifest=null/reasons. Full effects/context
không paginated/truncated; không dùng roster page1 như toàn bộ Booking set.
No Payment/Invoice amount/Profile; preview không mutate/state close/audit/T23.

SessionMembershipManifest expectedVersions R object:
session R RowVersion, bookings/attempts/registrations R `array<VersionedReference>`.
expectedSet R object: scheduledBookingIds R `array<BusinessId>`, bindings R
`array<SessionBookingBinding>`.
SessionBookingBinding: bookingId/attemptId/registrationId R BusinessId, server-
resolved joins. booking versions exactly scheduledBookingIds/bindings; Attempt/
Registration arrays distinct IDs đúng bindings, no duplicate/foreign/context
subset. Required [] nghĩa expected empty, không omit/unknown. ID sorting chỉ
presentation; canonical bytes/set order contract còn SC-02.
Parent/Attempt RV không được giả tự đổi khi mọi child Booking thêm/hủy.

API-061 POST /admissions/entry-test-sessions/{sId}/cancel: reason R string NFC/
trim1–1000 theo ADR-081/091, expectedVersions/expectedSet R manifest trên, UUID R.
Không chọn subset/rebookTargetSession/Registration status hoặc grade input.
Live pre-start/exact SCHEDULED membership/versions/bindings recheck dưới gates;
booking mới sau preview hoặc clock vượt start409 dù Session RV không đổi.
Session CANCELLED +dependent Booking CANCELLED/provenance/capacity release/
mandatory audit/T23 cùng W06 transaction và operationId, không loop UI child
cancellations/two-step partial.200 AcademicSessionCommandResult.
Giữ Attempt/Registration/score/Band/history; Student tự đặt lại cùng logical
attempt, không auto rebook/lượt3/cancel Registration/Invoice hoặc xóa Booking.
400 malformed/reason/context;404 target;409 state/time/set/versions/guard;
audit failure rollback entire operation. Lookup API-045 AA-only Session family.

### 12.5. Teacher options — API-062

GET /admissions/entry-test-grader-options query date/startTime/endTime R theo
mục12.1, sessionId O BusinessId, page/pageSize O mục1.2 (pagination refinement
dự thảo cho picker). Không email/phone/name directory search/arbitrary role.
Với sessionId phải scope-valid và resolve current Session; chỉ bỏ chính ca đó
trong availability check, không bỏ lịch lớp/ca khác hoặc chọn arbitrary exclude.
Missing Session404; malformed time/date/ID400.

200 GraderOptions: currentGrader R GraderOption hoặc null (null khi tạo),
items R `array<GraderOption>`, page/pageSize/totalCount R theo mục1.2.
items chỉ eligible Teacher ACTIVE/no-overlap cho requested window, sort accountId
ASC; currentGrader riêng có thể unavailable/không trong items nên không mất
assignment vì đổi trang. GraderOption không Account RV cần command; target
grader Account version sẽ được lấy ở change preview. Tạo ca không RV row mới,
live Account/availability check vẫn bắt buộc. Không hứa options giữ chỗ hoặc
trả conflict Class/Student/contacts ngoài scope. Empty200 và không auto chọn
Teacher khác/đổi role; no background polling/Account directory.

### 12.6. Đổi grader có lý do — API-063–064

API-063 POST /admissions/entry-test-sessions/{sId}/grader-change-preview:
body graderId R BusinessId, không UUID hoặc reason mandatory cho preview.
200 GraderChangePreview: sessionId R BusinessId, oldGrader R GraderReference,
newGrader R GraderReference, eligible R boolean, blockedReasons R `array<string>`,
conflicts R `array<SafeScheduleConflict>`, asOfUtc R Instant,
manifest R GraderChangeManifest hoặc null.
SafeScheduleConflict draft: resource R enum GRADER/ROOM, message R string;
không peer Student/roster/contact/payload hoặc tùy ý internal resource key.
Missing candidate404/malformed400; valid candidate không eligible→200 blocked.
Đề xuất cùng grader hiện tại→blocked200, không intent “đổi” no-op/audit giả;
preview shape/UI refinements vẫn chưa Accepted riêng; new command same grader
400 theo RF02/ADR-158 sau valid current-context checks.

GraderChangeManifest = SessionMembershipManifest +expectedVersions.grader R
RowVersion của Account candidate và expectedSet.currentGraderId R BusinessId.
Only pre-start/non-CANCELLED/chưa score hoặc ABSENT thuộc **Session này**;
không lấy ABSENT lịch sử của cùng Attempt ở ca khác làm lý do cấm đặt lại.
ACTIVE/no overlap candidate; manifest đầy đủ active Booking bindings/context,
không session token thay mọi child/presence guard. Current grader Account
blocked không tự xóa assignment; đổi hợp lệ là command riêng.

API-064 POST /admissions/entry-test-sessions/{sId}/change-grader:
graderId R BusinessId, reason R NFC/trim1–1000, expectedVersions/expectedSet R
GraderChangeManifest, UUID R. Candidate ID phải là object đã preview/token
binding đúng; old current grader membership check, not client-declared actor.
Pre-start/no Session score/ABSENT/live ACTIVE/no overlaps/exact active Booking
set và tokens recheck, mismatch409/repreview.400 missing/malformed/reason,
404 target/candidate,409 stale/window/state/history/resource/bindings.
200 AcademicSessionCommandResult; change grader/old-new-actor-time-reason audit/
T23 cùng W06 transaction, giữ Bookings/Attempt/Band/Registration. Không update
Student contacts/score/Invoice, đổi sau start/chấm/vắng hoặc quyền override
correction. Same replay200 no new assignment/history; lookup API-045 scoped AA.
New same-grader intent400 fieldErrors graderId theo ADR-158 sau valid current
context; no DML/provenance/audit/new completion, stale/window/member409 giữ.

### 12.7. Academic roster — API-065 / SP02

POST /admissions/entry-test-sessions/{sId}/roster/search:
RosterSearchRequest name/email/phone O string theo normalization/literal match
mục10.2, bookingStatus O BookingStatus, view O current/history (draft default
current), sort O BOOKED_ASC/BOOKED_DESC (default BOOKED_ASC, bookedAt/id cùng
hướng), page/pageSize mục1.2. current = SCHEDULED bookings; history = tất cả
Booking statuses trong ca, có thể còn SCHEDULED. No global Student selector.
POST read-only/CSRF/Origin/no-store, không UUID/T23 hoặc Student directory search.

| AcademicRosterRow property | Required / type | Scope / meaning |
| --- | --- | --- |
| bookingId, attemptId, registrationId | R BusinessId mỗi trường | Grain một Booking; joins đúng parent/Session. |
| attemptNumber | R integer1–2 | Logical attempt, không tăng từ ABSENT/CANCELLED. |
| bookedAt | R Instant | Booking creation/sort. |
| bookingStatus | R BookingStatus | History giữ, không thiếu score→ABSENT. |
| registrationStatus | R RegistrationStatus | CANCELLED/REJECTED vẫn history, không pending grading mới. |
| student | R StudentContactReference | Internal Student ID/current name-email-phone trong task; không Profile DOB/address. |
| course | R CourseReference | Referenced history, không selection ACTIVE-only. |
| targetScore, targetGradedAt | R EntryScore/Instant hoặc null | Chỉ result với completed_booking_id=bookingId và booking COMPLETED, không score của Booking đã rebook ở ca khác. |
| currentBand | R BandReference hoặc null | Current parent Band, không gọi là target Booking score. |
| invoiceStatus | R UNPAID/PAID/CANCELLED hoặc null | AcademicAffairs status-only theo ADR-083; không ID/amount/Payment. |

200 page AcademicRosterRow; full set count cho pagination không totals tiền.
Empty200; missing Session404/input400/session401/role-restricted403. No grading/
absence/correction rights từ roster; no Profile export/print.
SP02 actual caller trusted verified AcademicAffairs/role, typed params/sort/
projection allowlist; VW02 Booking grain/FN01 consistent asOfUtc, không join
hai Attempts làm nhân rows/score. Compound roster/preview coherent read context
như bản chính§9.3, không hứa setting đã chạy; signatures/isolation/grants SC-03.
No persisted status close/idle heartbeat từ search; consent không code/SQL.

## 13. Teacher nhiệm vụ thi đầu vào — API-066–073 / W08 / F25

Paths dưới /api/v1/admissions/teachers/me. Teacher NORMAL và **current assigned**
Session của target ở list/detail/roster/command/replay/lookup. Original grader
cũ không có quyền vĩnh viễn sau reassignment; sửa điểm cần cả original và current.
Account blocked/expired/revoked không giữ session read bằng DTO/token cũ.
Không quản lý Session/Invoice/Payment/Profile hoặc override AcademicAffairs.

### 13.1. Teacher-only types và Session reads — API-066–067

| Type | Required properties / nullable / giới hạn |
| --- | --- |
| TeacherSessionView | id BusinessId, date LocalDate, startTime/endTime MinuteTime, startAt/endAt Instant, room string, status/effectiveStatus OPEN/CLOSED/CANCELLED, taskCounts EntryTaskCounts, version RowVersion. Không Admin Account/grader picker/capacity edit/Invoice. |
| TeacherSessionDetail | TeacherSessionView +cancellation {at: Instant, reason: string1–1000} hoặc null; không management allowedActions. |
| TeacherRosterRow | AcademicRosterRow **trừ hoàn toàn property invoiceStatus**; không trả null thay cho field cấm/serialize cùng EF entity. Các fields task/entry/contact còn lại cùng đúng grain. |
| RelatedEntryAttempt | id BusinessId, attemptNumber1–2, version RowVersion, score EntryScore hoặc null, gradedAt Instant hoặc null, completedBookingId BusinessId hoặc null. Full≤2 logical attempts; không embedded roster/calendar của Session khác. |
| EntryResultProvenance | score EntryScore, gradedAt Instant, originalGraderId BusinessId; chỉ Booking COMPLETED của đúng Attempt, không current Teacher giả làm original. |
| EntryAbsenceProvenance | at Instant, actorId BusinessId từ target Booking; không guessed score0/auto ABSENT. |

API-066 GET /entry-test-sessions query fromDate/toDate O LocalDate VN inclusive
ngày start/from>to400, effectiveStatus O OPEN/CLOSED/CANCELLED, sort O START_ASC/
START_DESC default START_ASC, page/pageSize mục1.2. Không graderId/studentId/
accountId để chọn scope.200 page TeacherSessionView chỉ current assigned,
gồm history trong current assignment; reassigned mất list/detail access dù đã
từng chấm. Empty200, role/restricted403/session401, validation400 chung.

API-067 GET /entry-test-sessions/{sId}: BusinessId, no business body/query,
200 TeacherSessionDetail; missing/not currently assigned404 cùng message.
No Role=Teacher query credential/history override. FN01 effective CLOSED từ
start, GET không persist; session CLOSED không tự cấm grade, cancelled history
chỉ read/no new grading. Cùng no-store/role-task allowlist trên list/detail.

### 13.2. Teacher roster — API-068

POST /entry-test-sessions/{sId}/roster/search: cùng RosterSearchRequest filters/
defaults/pagination/sort mục12.7, scope Student rows chỉ Session hiện assigned.
200 page TeacherRosterRow, không Invoice field (kể cả amount/status null), DOB/
address hoặc training scores. Missing/not assigned404; invalid400, CSRF/role403,
session401. No UUID/mutation marker. Filter PII body/RAM/no URL/raw logs/storage.

TargetScore chỉ thuộc completed Booking row, không map Attempt.score của lượt
đặt lại vào row ABSENT/CANCELLED. Current parent Band được gắn nhãn riêng;
empty/ungraded null không grade0. Registration CANCELLED/REJECTED vẫn history/
disabled task, không lén drop history khỏi page hoặc tự nhập vắng từ ô trống.
SP02 thật resolve trusted caller/current assignment; no roster toàn trung tâm/
other Teacher current/history/calendar. Same projection cho current/history/
detail, không chỉ hide finance ở UI hoặc broaden scope để SP dễ viết.

### 13.3. Task detail/guard context — API-069

GET /entry-test-bookings/{bId}: BusinessId, no business body/query,
200 TeacherEntryTaskDetail required properties:

| Field | Type / nullable | Source / guard |
| --- | --- | --- |
| booking | EntryBookingView mục9.1 | Target Session/current assigned; binding immutable Booking→Attempt→Registration. |
| student, course | StudentContactReference/CourseReference | Contact hiện tại +target Course, không editable Profile/tuition/Invoice. |
| registrationStatus | RegistrationStatus | Parent current state; chưa CONFIRMED mới chấm/correction windows giữ. |
| registrationVersion | RowVersion | Không bao trùm every child result/membership. |
| relatedAttempts | `array<RelatedEntryAttempt>` | Full exact≤2/order attemptNumber ASC; scoped parent context để tính best/Band, không Session/roster của người khác. |
| bandAssignment | BandAssignmentView hoặc null | Current best/Band/version hoặc expected absence, không Teacher tự chọn Band. |
| targetResult | EntryResultProvenance hoặc null | Chỉ completed_booking_id đúng target bId; có score thật0.00 vẫn COMPLETED. |
| absence | EntryAbsenceProvenance hoặc null | Chỉ target Booking ABSENT; không suy từ endAt/score null. |
| cancellation | {at: Instant, reason: string hoặc null} hoặc null | Own task Booking history; Student cancellation không mandatory reason. |
| allowedActions | `array<TeacherEntryAction>` | Draft GRADE/MARK_ABSENT/CORRECT_SCORE, không T23 strings. |
| disabledReasons | object action→string | Scope-safe reasons, no foreign object existence/contact leakage. |
| gradeManifest, absenceManifest, correctionManifest | Typed manifests mục13.4 hoặc null | Chỉ manifest tương ứng current eligible intent; không read token cấp quyền khi command tới sau. |

Missing/not current assigned404 trước projection/tokens, invalid400/session401/
role403. Null manifest khi blocked không làm mất completed result/history được
phép đọc. Current assigned có thể xem target original grader provenance, nhưng
correction chỉ originalGraderId=verified Teacher AND still assigned/guard đạt.
Không dùng data từ client hoặc original grader ID như credential; không permanent
access/cache giữ nhiệm vụ cũ. Detail không mutation/operationId/preview table.

### 13.4. Related-version manifests — không DB table/state mới

Mọi expected versions Base64 decode8 bytes. Missing/malformed/binding-shape400,
shape đúng nhưng stale token/set/state/clock guard409; outside task404 trước
token errors. Các IDs là optimistic membership đã thấy, không chọn target khác
hoặc gửi score/Band/actor trong set. Full arrays không paginated/selected subset.

| Manifest | expectedVersions R object | expectedSet R object |
| --- | --- | --- |
| EntryGradeManifest (initial) | session/registration/attempt/booking R RowVersion; relatedAttempts R `array<VersionedReference>`; bandAssignment R VersionedReference hoặc null. | attemptIds R `array<BusinessId>`; bandAssignmentId R BusinessId hoặc null; currentBookingId R BusinessId (=target bId); completedBookingId R literal null. |
| EntryAbsenceManifest | session/registration/attempt/booking R RowVersion; relatedAttempts R `array<VersionedReference>`. | attemptIds R `array<BusinessId>`; currentBookingId R BusinessId (=target bId); completedBookingId R literal null. Không Band token vì absence không recalculation/xóa Band. |
| EntryCorrectionManifest | session/registration/attempt/booking R RowVersion; relatedAttempts R `array<VersionedReference>`; bandAssignment R VersionedReference (non-null). | attemptIds R `array<BusinessId>`; bandAssignmentId R BusinessId non-null; currentBookingId R literal null; completedBookingId R BusinessId (=target completed Booking). |

relatedAttempts exactly full current parent logical Attempt set, IDs unique,
versions khớp attemptIds; includes target Attempt và token trùng expectedVersions.
attemptNumber do server join, không client tạo/chọn lượt3. Band presence/version
và ID đều cùng null hoặc cùng có cho initial; completed Booking thuộc đúng Attempt/
Registration/Session, no parent-only guard. Current SCHEDULED/COMPLETED membership
live recheck dưới locks/unique; Registration RV không giả track child-only writes.

FN02/BANDSET serialization bảo vệ threshold set/first-completed-result race,
không bắt Teacher gửi Band threshold version hoặc pick bestAttemptId/BandId.
Server tính best với cả scores COMPLETED, tie chọn attempt1 và ngưỡng lớn nhất
≤score. Absence giữ Band cũ và lượt, không yêu cầu BandVersion để ngầm sửa nó.
Confirmation/cancellation/close clock/other result correction có thể làm context
stale; re-read task, không merge/overwrite hoặc auto chọn command khác.
Canonical digest bytes/action registry toàn55 commands còn SC-02; manifest
không signed authorization/reservation hoặc giữ snapshot qua confirmation UI.

### 13.5. Initial score — API-070 / TX03

POST /entry-test-bookings/{bId}/score: InitialEntryScoreRequest score R
EntryScoreInput, expectedVersions/expectedSet R EntryGradeManifest, UUID R.
EntryScoreInput draft: string decimal0–100 với tối đa2 chữ số thập phân,
không exponent/NaN/Infinity/comma/negative/thousands/number coercion hoặc
precision rounding. Cho “80”, “80.5”, “80.50”; parsed decimal cùng giá trị,
response EntryScore luôn hai chữ số. Exact canonical bytes/normalization
fixtures sẽ rà SC-02, không hash raw JSON spelling. Empty/null/invalid400
fieldErrors score, không chuyển ô trống thành0/vắng.

Live từ Session endAt (inclusive)/non-CANCELLED/current assigned Teacher,
Booking SCHEDULED/no completed Attempt result/correct parent links;
Registration chưa CONFIRMED/REJECTED/CANCELLED theo W08. CLOSED được chấm nếu
các guard đạt. Không deadline24/48h hoặc Role override; time/RV independent.
400 malformed/input/token,404 outside assignment,409 stale/result/state/end/
membership. Không edit completed result bằng initial route.

200 TeacherEntryCommandResult: task R TeacherEntryTaskDetail mới,
operationId R UUID. TX03 Booking COMPLETED/Attempt score+original grader/time/
completedBookingId +Registration BAND_ASSIGNED/best Band/first-use threshold
freeze/mandatory audit/T23 cùng transaction. Điểm0.00 là result hợp lệ, khác
ABSENT/null. Không separate frontend “save score→set Band”, copy prior Course
score hoặc commit partial grade/Band. Replay200 current authorized task/old
operationId no effects; mất task scope không dùng marker để trả ngoài quyền.

### 13.6. Xác nhận vắng — API-071 / TX03

POST /entry-test-bookings/{bId}/absence: body đúng expectedVersions/expectedSet
EntryAbsenceManifest, UUID R. Không score/Band/Student selector/mandatory reason
mới. Một confirmation UI theo accepted policy, không double confirmation hoặc
auto ABSENT khi tới end/chưa nhập score.

Live từ Session endAt/current assigned/non-CANCELLED, SCHEDULED/no Attempt result/
valid pre-confirmation Registration/bindings; CLOSED tự nó không cấm.400 missing/
shape,404 not assigned,409 stale/state/result/time/membership.
200 TeacherEntryCommandResult: target Booking ABSENT +actor/time/operation/
audit/T23 all-or-none theo W08/TX03, no Attempt score/grader-result bundle/0.00/
new attempt/Band reset. Logical attempt retained để Student rebook cùng lượt;
Registration không quay PENDING/xóa Band hoặc tự tạo Booking khác.
Mandatory audit failure rollback; replay no duplicate absence/audit. Result
unknown không markAbsent lặp/new UUID hoặc tự grade0 để “sửa”.

### 13.7. Correction có lý do — API-072 / TX03

PUT /entry-test-attempts/{tId}/score: EntryScoreCorrectionRequest score R
EntryScoreInput, reason R string NFC/trim1–1000, expectedVersions/expectedSet R
EntryCorrectionManifest, UUID R. Server resolve completed_booking_id của target
Attempt rồi Session/current assignment; client không chọn original grader hoặc
completed Booking khác. Target không owned-by-task404 trước result/state errors.

Chỉ COMPLETED Attempt result, original score Teacher vẫn assigned và
Registration TESTING/BAND_ASSIGNED. Cần reason, score range/precision giữ như
initial; không ABSENT→COMPLETED, không sau CONFIRMED/REJECTED/CANCELLED, không
AcademicAffairs/Admin override, không window24/48h mới. Same-value input không
được tạo giả new result hoặc thay provenance; ADR-158/RF02 đã Accepted:
same parsed score400 fieldErrors score sau valid current-context checks,
no mutation/history/completion; stale/time/member409 không bị equality bypass.

200 TeacherEntryCommandResult; TX03 score/best/Band recalculation/mandatory old-
new-reason-actor-time audit/T23 atomic. Giữ original graded_by/graded_at và
completedBookingId, audit giữ correction event; không ghi đè initial provenance
bằng người/thời gian sửa. Missing/reason/precision/token shape400; scope404;
stale/result/Band/attempt set/state/confirmation race409. Không client chọn best/
Band hoặc gửi current roster thay full parent Attempt context. Replay200 task/
old operation current scope/no recalculate hoặc correction history lần2.

### 13.8. Teacher caller completion — API-073 / recovery

GET /command-results/{requestId}?action=... dưới prefix Teacher của mục13:
Teacher NORMAL/current task assignment, UUID/action R; không caller/body hoặc
original-grader-history selector. Chỉ initial-score/absence/correction families.
200 SafeCommandCompletion mục6.4/resultMode SAFE_REFERENCE; target R object
bookingId/attemptId/registrationId R BusinessId, đúng joined task.
Correction resolve target completed Booking, caller không trỏ arbitrary Session.
Không Student contact/score/Band/secret hoặc response PII snapshot trong marker.

Malformed/missing/unknown family400; role/restricted403; valid role/action
missing/not caller/expired marker404 RESULT_NOT_CONFIRMED. Current assignment
mất cũng404 cùng scope-safe response, không giữ access nhờ actor từng chấm.
Lookup recheck current role/task read permission; không đòi Registration còn
pre-confirmation chỉ để đọc safe completion khi task vẫn được phép xem.
Original-grader guard của correction luôn giữ cho mutation/replay theo policy,
không union với initial-grading families để override.

Sau timeout GET task/roster/caller result đúng scope để đối chiếu intent;
absence marker không proof rollback, có thể còn commit/cleanup/window khác.
Không auto score/ABSENT/retry/new UUID hoặc reassign Teacher để lấy response.
Retry chủ đích cùng intent giữ UUID/normalized content/versions-set; khác content
cùng key409. Completed replay rights trước stale checks new mutation, no effects/
duplicate audit; marker≥24h không hứa dedup vô hạn hoặc khôi phục PII snapshot.

### 13.9. Ví dụ wire và kiểm chứng thiết kế

Initial score của lượt1 khi chưa có Band, full logical Attempt context.
IDs/tokens giả; expected completed absence không skip guard. UUID ở header theo
shared contract, không body actor/Band hay giả thời điểm Teacher được chấm.

```json
{
  "score": "80.50",
  "expectedVersions": {
    "session": "AAAAAAAAAAE=",
    "registration": "AAAAAAAAAAI=",
    "attempt": "AAAAAAAAAAM=",
    "booking": "AAAAAAAAAAQ=",
    "relatedAttempts": [
      { "id": 301, "version": "AAAAAAAAAAM=" }
    ],
    "bandAssignment": null
  },
  "expectedSet": {
    "attemptIds": [301],
    "bandAssignmentId": null,
    "currentBookingId": 401,
    "completedBookingId": null
  }
}
```

Hủy ca với không có Booking vẫn gửi full empty membership, không omit parent/
child guard. Không cancel Registration/Invoice hoặc tự đặt lại ca khác.

```json
{
  "reason": "Trung tâm không thể tổ chức ca này.",
  "expectedVersions": {
    "session": "AAAAAAAAAAE=",
    "bookings": [],
    "attempts": [],
    "registrations": []
  },
  "expectedSet": {
    "scheduledBookingIds": [],
    "bindings": []
  }
}
```

Mục tiêu **sau triển khai**: role/task projection parity/current assignment và
original-grader correction; no Teacher Invoice/Academic financial amount; actual
SP02/VW02/FN01/FN02/TG03 caller; start/end boundaries/full OPEN/read-only CLOSED;
any canceled booking blocks schedule-edit; no auto ABSENT/score0/lượt3; related
Attempt full-set/tie-first/best-recalculation/Band first-use races; cancel Session
vs new Booking/grader-change/start/Student confirmation; atomic effects/audit/
replay and rollback; loss assignment cuts list/detail/lookup/replay; targetScore
not borrowed from rebooked Booking; timestamp provenance unchanged after correction.
Đây là targets/JSON examples, không executed API/SQL/contract/browser tests.

## 14. AcademicAffairs Course/Band — API-074–080 / W13 / F18

Paths dưới /api/v1/academics; chỉ AcademicAffairs NORMAL, no-store.
Catalogue price là quyền cấu hình Course, **không** Invoice/Payment amount của
Student. Không dùng Student ACTIVE-only projection cho màn quản lý; Teacher/
Admissions/Admin không được cấp quyền từ việc DTO có các fields này.
Write có CSRF/Origin/UUID như shared contract; read không replay marker.

### 14.1. Course types và catalogue — API-074

| Type / property | Required / kiểu | Giới hạn |
| --- | --- | --- |
| AcademicCourseView.id | R BusinessId | T05, server cấp khi create. |
| code | R string | Uppercase1–30, A–Z/0–9/_/- theo E03; unique toàn ACTIVE/INACTIVE, bất biến/no reuse. |
| name | R string | NFC/trim1–150; không unique toàn catalogue. |
| basePrice | R Money | Exact nguyên đồng dương; không zero/default/free Course hoặc Invoice snapshot. |
| status | R ACTIVE/INACTIVE | Chỉ Course lifecycle; không Registration/Program/Class status. |
| version | R RowVersion | Course token, không tự tăng bởi client. |
| allowedActions | R array CourseAction | Draft keys EDIT_DETAILS/DEACTIVATE/ACTIVATE, không T23 action strings. |
| disabledReasons | R object CourseAction→string | Chỉ keys blocked của cùng Course, không Student/debt/payment details. |

CourseDetail = AcademicCourseView; CourseCommandResult có course R CourseDetail,
operationId R UUID server. DTO không Account/Profile/Student directory/Invoice/
Payment/cancellation history. Actions là presentation hint, server kiểm lại khi
commit. Không dựa UI ẩn field để chống mass assignment.

API-074 GET /courses: query code O string theo code syntax/normalize trên,
đề xuất exact-code match; name O string NFC/trim1–150, literal substring theo
CI accent-sensitive, không raw SQL wildcard/expression; status O ACTIVE/INACTIVE,
omitted gồm cả hai. sort O CODE_ASC/CODE_DESC default CODE_ASC; code rồi id
cùng chiều để stable; page/pageSize mục1.2. Empty/invalid provided filter400,
không blank→toàn catalogue ngầm.200 page AcademicCourseView, items=[] hợp lệ.
Search/sort spelling là draft refinement, không thay code uniqueness hoặc quyền.

### 14.2. Tạo và đọc Course — API-075–076

API-075 POST /courses: CreateCourseRequest code/name/basePrice/status đều R,
đúng types/limits mục14.1, không id/version/actor/expectedVersions. Status input
phải ACTIVE hoặc INACTIVE, không server đoán từ empty/omitted. Normalize code/
name trước uniqueness/validation; giữ accepted Course code policy, không auto
generate suffix hoặc tái dùng code của INACTIVE Course.

201 CourseCommandResult; một save unit Course +mandatory allowlist audit +T23,
không tạo Program/Registration/Invoice kèm.400 invalid/unknown property/
money precision;409 normalized duplicate code hoặc race. Session401/role-
restricted/CSRF403 theo shared. Exact replay HTTP outcome registry còn SC-02;
không tạo row/audit thứ hai hoặc giả row mới từ code khác khi UUID trùng.

API-076 GET /courses/{cId}: BusinessId, không business body/query,
200 CourseDetail cùng catalogue price/actions/version. Missing404; malformed
ID400, auth401/403. Course INACTIVE vẫn detail quản lý hợp lệ, không lọc mất
lịch sử hoặc nới quyền đọc financial records.

### 14.3. Name/price save — API-077

PUT /courses/{cId}/details: UpdateCourseDetailsRequest name R string1–150,
basePrice R Money, expectedVersions R object {course R RowVersion}; UUID R.
Không code/status/Student/Invoice/actor hoặc endpoint thay code. Cả name/price
bắt buộc để save explicit một form; null/omitted không là “giữ giá cũ”.
Course ACTIVE/INACTIVE đều edit danh mục theo quyền hiện có.

200 CourseCommandResult, atomic Course old-new audit/T23.400 input/token shape,
404 missing,409 stale/guard conflict. Giá mới chỉ dùng cho Invoice phát hành
**sau** theo issuance guard; Invoice đã phát hành không sửa amount/rowversion,
không tự charge Payment/đổi PAID. Course edit và issuance dùng chung Course row
guard tới commit, không coi GET/RV là reservation về giá. No-op ADR-159 đã Accepted:
all-equal business values400 sau valid context;no DML/provenance/audit/new marker.

### 14.4. Lifecycle tách biệt — API-078–079

Cả hai POST /courses/{cId}/deactivate hoặc /activate có body duy nhất
expectedVersions R {course R RowVersion}, UUID R. Không client status/price/
code/mandatory reason hoặc cascade flags. Một confirmation đúng đối tượng/
ảnh hưởng theo frontend policy đã duyệt; explicit intent, không autosave.

API-078 chỉ ACTIVE→INACTIVE;200 CourseCommandResult status INACTIVE.
Chặn Registration **mới**, không tự hủy Registration/Invoice, Program/Class/
lịch hoặc Teaching/grading của dữ liệu cũ. API-079 chỉ INACTIVE→ACTIVE;
200 cùng type/status ACTIVE, giữ record/code/history/Program freeze.
Wrong current source state hoặc stale409; malformed400/missing404/auth chung.
Completed replay kiểm current rights trước guards dành cho mutation mới; không
chạy transition lần hai hoặc coi state hiện tại khác là proof rollback.

### 14.5. Seeded Band read — API-080

GET /bands: không business body/query, không pagination/action/editor.
200 BandReferenceSet: items R full array BandConfigView; mỗi item id R
BusinessId, code R string theo T06, minScore R EntryScore0.00–100.00,
version R RowVersion. Sort minScore ASC/id ASC; năm seed Band1–5 với ngưỡng
0.00/25.00/45.00/65.00/85.00 lấy từ DB, không hardcode thuật toán chọn Band.
isThresholdSetFrozen R boolean suy từ kết quả đầu vào COMPLETED đầu tiên theo
E03; compound read coherent, không new DB flag/clock selector/asOf query.

Không maxScore/weight/edit link/create-delete/unlock/reset API. RV phục vụ
optimistic relation context ở Program, không grant Band mutation. First-use
freeze dùng BANDSET/TG03 và kết quả thực, không “đã có Session/Booking/score
input” hoặc score null; result0.00 cũng COMPLETED. Không có kết quả vẫn không
suy web editor. Missing/inconsistent seed là runtime/integrity issue cần rà
SC-03/04, không auto seed/repair bằng GET hoặc tự invent Band ngoài baseline.

## 15. TrainingProgram và caller result — API-081–090 / W13 / F18

Chỉ AcademicAffairs NORMAL cho API-081–089; API-090 có **role/action pairs**
riêng ở mục15.9, không union quyền AcademicAffairs/Teacher.
Program lifecycle DRAFT/ACTIVE/INACTIVE; Course-Band unique toàn status.
Used/frozen suy từ **bất kỳ Class, kể cả CANCELLED**; không cờ client/DB mới.
Rename/status hợp lệ không mở khóa Course/Band/components/count.

### 15.1. Program/component types và search — API-081

Mọi property trong bảng là R, nullable chỉ nơi ghi rõ; references của mục9.1
không mang Course price/Student data. Không serialize T07/T08 EF entities.

| Type | Fields / kiểu | Giới hạn |
| --- | --- | --- |
| ProgramSummary | id BusinessId; name string1–150 NFC/trim; course CourseReference; band BandReference; status DRAFT/ACTIVE/INACTIVE; expectedLessonCount integer dương SQL int; isConfigurationFrozen boolean; version RowVersion. | Summary không component/score/student roster; freeze live derived, không chỉ count lớp đang active. |
| ComponentView | id BusinessId; name string1–100 NFC/trim; weight ComponentWeight; kind GENERAL/LESSON/FINAL_EXAM; version RowVersion. | Name CI accent-sensitive unique per Program; không displayOrder/Score/Schedule binding DB mới. |
| ComponentWeight | string decimal lớn hơn0, ≤100, output luôn2 chữ số thập phân. | T08 decimal(5,2), không number/float, không percentage symbol;0.00 không hợp lệ. |
| ComponentWeightInput | string decimal lớn hơn0, ≤100, tối đa2 chữ số thập phân; chấp nhận “40”, “40.5”, “40.50”. | Không exponent/comma/negative/NaN/number coercion/rounding; canonical bytes chưa chốt SC-02. |
| ConfigurationValidation | totalWeight string decimal không âm, fixed2; finalExamCount integer≥0; meetsActivationRules boolean; issues array ConfigurationIssue. | totalWeight có thể0.00 hoặc >100.00 ở DRAFT; exact sum server decimal, không alias ComponentWeight hoặc clamp. |
| ConfigurationIssue | field string public field-path; message string. | Không echo raw input/foreign object IDs/internal constraint errors. |
| ProgramDetail | ProgramSummary +components array ComponentView; relationVersions {course: RowVersion, band: RowVersion}; validation ConfigurationValidation; allowedActions array ProgramAction; disabledReasons object ProgramAction→string; configurationManifest ProgramConfigurationManifest hoặc null; activationManifest ProgramActivationManifest hoặc null. | Full coherent components, id ASC; manifests đúng current eligible intent, null khi blocked; không full Class list/Student/Teacher grades. |

Draft ProgramAction keys EDIT_CONFIGURATION/RENAME/PREVIEW_ACTIVATION/ACTIVATE/
RETURN_TO_DRAFT/DEACTIVATE chỉ presentation, không canonical T23 strings.
ProgramCommandResult: program R ProgramDetail, operationId R UUID server.
ConfigurationValidation kiểm từng field/relation/count, component uniqueness,
max one FINAL_EXAM, và activation exact sum100/one FINAL_EXAM; không tự fix.
meetsActivationRules nói **cấu hình**, không bỏ qua lifecycle/freeze/role/
current relations khi command tới. Component name/kind/weight/list khóa sau
first Class; LESSON component không tự tạo Score/Attendance mỗi buổi.

API-081 GET /training-programs query courseId/bandId O BusinessId exact filter,
name O string NFC/trim1–150 literal substring CI accent-sensitive; status O
DRAFT/ACTIVE/INACTIVE (omitted tất cả). sort O NAME_ASC/NAME_DESC default
NAME_ASC, name rồi id cùng chiều; page/pageSize mục1.2. Filters không chọn actor/
Student/Teacher hoặc Program version lịch sử.200 page ProgramSummary, empty200.
Provided malformed/blank400; no foreign filter existence detail hoặc Invoice
amount. Search/sort/keys vẫn proposed wire refinement, không business rule mới.

### 15.2. Tạo DRAFT và đọc đầy đủ — API-082–083

API-082 POST /training-programs: CreateProgramRequest:

| Property | Required / type | Constraint |
| --- | --- | --- |
| courseId, bandId | R BusinessId mỗi trường | Existing selected relations; không Course-Band duplicate kể cả INACTIVE. |
| name | R string | NFC/trim1–150. |
| expectedLessonCount | R integer dương SQL int | Luôn >0, không0/null placeholder. |
| components | R array NewComponentInput | Mỗi item name R string1–100, weight R ComponentWeightInput, kind R component enum; không id/programId/version/client key. |
| expectedVersions | R object | course R RowVersion và band R RowVersion của đúng selected IDs; new Program/components chưa token. |

Status server DRAFT, IDs server cấp; input status/isConfigurationFrozen/actor
bị từ chối400. DRAFT được lưu cấu hình chưa đủ100/chưa FINAL_EXAM, nhưng từng
item phải valid/unique và tối đa một FINAL_EXAM. Đề xuất wire cho incomplete
draft: components=[] hợp lệ, không dùng null/omit; không suy thêm business cap
số components hoặc giới hạn totalWeight≤100 khi draft. Empty draft không đủ
activation. Không auto thêm exam/scale weights/expected lesson placeholder.

201 ProgramCommandResult; selected relations thiếu404, invalid/token shape400,
stale relations/duplicate pair/unique component name/kind race409. Không invent
Course ACTIVE guard cho create/activate ngoài E01/E02; INACTIVE Course không
cascade chặn dữ liệu cũ. Unique pair không cho tạo “Program version2” để né used
freeze; không restore record khác bằng create. All components +Program/audit/
T23 một transaction, lỗi một item rollback whole create.

API-083 GET /training-programs/{pId}: BusinessId/no business body/query,
200 ProgramDetail. components đầy đủ không paginated/hidden invalid draft,
relationVersions lấy từ current Course/Band, full membership manifests mục15.3.
DRAFT/ACTIVE/INACTIVE đều readable với AcademicAffairs, frozen không che mất
cấu hình/history reference. Missing404/ID400/auth chung. Compound read coherent
không giữ snapshot/locks qua thời gian owner chỉnh form; không mutation/touch RV.

### 15.3. Whole-configuration và activation manifests

Đây là đề xuất exact wire keys, không bảng/state/role mới. Các IDs/RVs là
optimistic context, không authorization. Full sets không thay bằng page đang
xem, parent RV hoặc một token của component “đang sửa”.

| Manifest | expectedVersions R object | expectedSet R object |
| --- | --- | --- |
| ProgramConfigurationManifest | program R RowVersion; course/band R RowVersion; existingComponents R array VersionedReference. | existingComponentIds R array BusinessId, full source membership. |
| ProgramActivationManifest | program/course/band R RowVersion mỗi trường; components R array VersionedReference. | courseId/bandId R BusinessId; componentIds R array BusinessId, full current membership. |

ConfigurationManifest trả từ detail dùng Course/Band hiện tại. Khi chọn đổi
relation trong form, request courseId/bandId là target và course/band tokens
phải lấy cho **selected target** qua API-076/080, không token cũ hoặc client tự
chế. Program token bảo vệ current parent links/name/count/state; existingComponents/
existingComponentIds luôn **full trước-save**, kể cả component sẽ bỏ khỏi tập
đích. Arrays unique IDs, version bindings one-to-one; [] gửi explicit nếu chưa
component, không omitted. Không giả parent RV track mọi child/set/Class writes.

ActivationManifest bindings courseId/bandId phải đúng persisted Program,
components/RVs đúng full current set. Thiếu/sai shape/token encoding/duplicate
IDs/bindings nội bộ request400; live stale/member-set/parent links/state409;
unknown/cross-Program component không được nhập vào tập, response scope-safe404
không lộ object ngoài target. Valid source membership changed do actor khác409,
không merge đích hoặc xóa component mới mà editor chưa từng thấy.

Live no-first-Class/no dependent data recheck dưới EC:PROGRAM/config gates,
không expectedSet.classCount=0 hoặc gửi Class RV rỗng để chứng minh chưa dùng.
Một Class tạo xen giữa GET và save/return-to-draft phải làm command conflict;
CANCELLED Class vẫn retained. Relation versions không thay BANDSET guard của
first COMPLETED entry result hoặc fake web quyền edit Band thresholds.

### 15.4. Lưu toàn bộ config — API-084

PUT /training-programs/{pId}/configuration: UpdateProgramConfigurationRequest
name/courseId/bandId/expectedLessonCount/components đều R theo mục15.2,
expectedVersions/expectedSet R ProgramConfigurationManifest, UUID R.
Target components là array ExistingOrNewComponentInput:
id R BusinessId **hoặc null**; name/weight/kind R cùng types trên.
id=null là item mới server cấp ID; positive existing ID phải thuộc full source
set/Program, giữ ID qua edit. Không temporary negative ID/client UUID hoặc
version trong từng target item thay full source manifest.

Server so whole desired target với full source: bỏ ID cũ khỏi target chỉ là
remove draft component trong save unit này khi **DRAFT/no Class/no dependencies**.
Không được bỏ ID đó khỏi source version guard. New item null có thể nhiều;
existing ID không lặp. Mapping new rows nhận qua fresh full response (unique
normalized names), không tạo client-key/display-order DB field. Array là toàn
tập muốn lưu, không patch/delta “chỉ component đang sửa”.

200 ProgramCommandResult, one atomic Program+insert/update/remove components+
old-new allowlist audit+T23. DRAFT incomplete giữ như create, không tự activate.
ACTIVE unused phải explicit return-to-draft trước; INACTIVE/frozen không có edit
quyền mới.400 invalid/unknown/precision/shape;404 target/scoped component thiếu;
409 state/freeze/dependency/unique Course-Band/name/max-exam/stale/full-set/race.
Một child stale hoặc error rollback whole unit, không UI loop per-component.

Giữ trigger TG01/TG02 backstops và filtered unique FINAL_EXAM/name constraints.
Đổi chéo name/kind hoặc replace final-exam cần DML order/EF trigger OUTPUT
integration SC-03; không claim một SaveChanges tự xử lý mọi intermediate unique
constraint. First-Class creation chia gate với config/return-to-draft. Không
EC:TIMETABLE chỉ vì rename/config chưa dùng, không delete Course/Program history.

### 15.5. Rename không mở khóa — API-085

PUT /training-programs/{pId}/name: RenameProgramRequest name R string NFC/trim
1–150, expectedVersions R {program R RowVersion}, UUID R.
Không components/courseId/bandId/lesson count/status/isFrozen/reason mới.
200 ProgramCommandResult, name audit/T23 atomic; lifecycle DRAFT/ACTIVE/INACTIVE
và used/frozen không tự cấm **rename** hợp lệ. Thay tên không đổi first-use
history hoặc cấu hình đã khóa; không copy Program/score/Class.
Invalid/token400, missing404, stale409. Same-value/no-op400 đã Accepted ADR-159 sau valid current context,
không tạo giả config mutation hoặc hỏi lại policy đã accepted.

### 15.6. Saved-config preview và activation — API-086–087

API-086 POST /training-programs/{pId}/activation-preview: **không business
input/body**, không gửi unsaved components/status/selected Course khác.
CSRF/Origin/no-store; read-only, không UUID/T23/“preview reservation”.
200 ProgramActivationPreview: program R ProgramSummary, validation R
ConfigurationValidation, eligible R boolean, blockedReasons R array string,
manifest R ProgramActivationManifest hoặc null.
Eligible chỉ DRAFT/INACTIVE +cấu hình/relations valid; ACTIVE trả eligible=false
với manifest=null, không mutate hoặc fake save. Invalid draft trả200 issues,
không400 chỉ vì sum chưa100/exam chưa có; request malformed400/missing404/auth
chung. Return saved exact full membership/tokens coherent, not arbitrary subset.
UI còn edits chưa lưu thì không activate; preview không âm thầm ghi local form.

API-087 POST /training-programs/{pId}/activate: body đúng expectedVersions/
expectedSet ProgramActivationManifest, UUID R. Không components/name/status/
“force”/client-calculated sum input. Guard current DRAFT→ACTIVE hoặc
INACTIVE→ACTIVE restoration, all fields/relations valid, exact decimal sum100
và **đúng một** FINAL_EXAM/count>0/current full set. Recheck không chỉ tin
eligible=true hoặc Course/Band IDs tồn tại. Không invent Course ACTIVE rule.

200 ProgramCommandResult status ACTIVE; restoration giữ used config freeze/
same identity/history. Valid-shaped current config không đạt/state/stale/
changed set409; malformed400/missing404. Không normalize weights/round excess
precision/add exam/auto open Class/đổi Course hay Band. Preview/race/time không
chứng minh commit; same UUID completed replay giữ rights/current projection,
không chạy activation lại hoặc reset state để match response cũ.

### 15.7. Explicit lifecycle — API-088–089

Hai POST có body duy nhất expectedVersions R {program R RowVersion}, UUID R;
không configuration/set/reason/force flags. API-088 /return-to-draft chỉ
ACTIVE **chưa bất kỳ Class** →DRAFT, live absence/dependencies/config gate;
200 ProgramCommandResult. Used/CANCELLED-Class/INACTIVE không returnDRAFT để
mở config. API-089 /deactivate chỉ ACTIVE→INACTIVE;200 cùng type status
INACTIVE; chặn Class mới/mở theo E01/E02, không dừng lớp ongoing/teaching/
Attendance/grading hoặc sửa GradeReport/Student status.
Return/disable explicit confirmation theo policy; invalid400/missing404/
stale/state/freeze409. Rename vẫn route riêng, no lifecycle implicit từ PUT.

### 15.8. Wire examples — whole save unit, không SQL/code

Ví dụ create DRAFT valid để activation sau: IDs/tokens giả8 bytes, không actor/
status/Student hoặc field EF. UUID ở header theo shared convention.

```json
{
  "courseId": 11,
  "bandId": 22,
  "name": "Chương trình nền tảng",
  "expectedLessonCount": 12,
  "components": [
    { "name": "Quá trình", "weight": "40.00", "kind": "GENERAL" },
    { "name": "Cuối khóa", "weight": "60.00", "kind": "FINAL_EXAM" }
  ],
  "expectedVersions": {
    "course": "AAAAAAAAAAE=",
    "band": "AAAAAAAAAAI="
  }
}
```

Whole config edit sau read, retain cả hai component IDs/RVs, target relation
không đổi. Không giả parent RV đủ bao trùm component-only writes/Class first-use.

```json
{
  "name": "Chương trình nền tảng",
  "courseId": 11,
  "bandId": 22,
  "expectedLessonCount": 12,
  "components": [
    { "id": 31, "name": "Quá trình", "weight": "35.00", "kind": "GENERAL" },
    { "id": 32, "name": "Cuối khóa", "weight": "65.00", "kind": "FINAL_EXAM" }
  ],
  "expectedVersions": {
    "program": "AAAAAAAAAAM=",
    "course": "AAAAAAAAAAE=",
    "band": "AAAAAAAAAAI=",
    "existingComponents": [
      { "id": 31, "version": "AAAAAAAAAAQ=" },
      { "id": 32, "version": "AAAAAAAAAAU=" }
    ]
  },
  "expectedSet": { "existingComponentIds": [31, 32] }
}
```

### 15.9. Shared academic caller completion — API-090

GET /command-results/{requestId}?action=... dưới /api/v1/academics.
requestId R UUID path/action R known family selector; không body/actor/role/
Class selector để tự đổi scope. Single endpoint có các pairs sau từ catalogue/
bản chính; bảng là documentary family names, **chưa** canonical action strings.
API-ID không T23 code, không cho phép arbitrary string qua registry.

| Caller NORMAL | Command families / API reference | Safe target R object, all fields R BusinessId |
| --- | --- | --- |
| AcademicAffairs | Course create/details/deactivate/activate:075/077/078/079. | courseId |
| AcademicAffairs | Program create/configuration/name/activate/return-to-draft/deactivate:082/084/085/087/088/089. | programId |
| AcademicAffairs | Class create/capacity/Teacher-change/open/start/complete/cancel:092/094/097/099/100/101/102. | classId |
| AcademicAffairs | Placement save:108. | classId, registrationId, classAssignmentId |
| AcademicAffairs | Individual Schedule create/move/held:118/122/126. | classId, scheduleId |
| AcademicAffairs | Weekly Schedule create:120. | classId (fresh Class schedule read, không persisted whole response array). |
| AcademicAffairs | Schedule replace:124. | classId, oldScheduleId, newScheduleId |
| AcademicAffairs | Whole-Class grade return/finalize:146/147. | classId; không Teacher DRAFT scores/comment snapshot. |
| Teacher, current assigned Class | Attendance save:133. | classId, scheduleId; no Student score/contact snapshot. |
| Teacher, current assigned Class | Per-student draft save:139. | classId, classAssignmentId; no grade/comment/financial amount. |
| Teacher, current assigned Class | Whole-Class submit/resubmit:141/142. | classId; không selected-student subset. |

25 AcademicAffairs families =4 Course+6 Program+8 Class/placement+5 Schedule+
2 reviewer;4 Teacher families. Không expose Teacher draft/submit mutations cho
AcademicAffairs hoặc Class/placement/Schedule/reviewer cho Teacher. Các role
khác403, restricted session403/invalid session401; known other-role family403
trước marker probing. Unknown/missing/malformed selector/UUID400.
Valid authorized family nhưng missing/not-caller/expired/outside current target
scope404 RESULT_NOT_CONFIRMED, same scope-safe message; không dùng actor lịch sử
để giữ quyền sau reassignment hoặc nhận target refs ngoài scope.

200 SafeCommandCompletion mục6.4: outcome COMPLETED, resultMode SAFE_REFERENCE,
operationId server UUID, target đúng allowlisted shape/family trên; không raw
entity/PII/request/input_digest/password hoặc response snapshot. Current caller/
role/action/target read scope kiểm lại trên lookup/replay. Teacher must still
assigned Class; marker từng tạo không là quyền lâu dài hay union AcademicAffairs.
Không bắt target vẫn ở source state của transition mới để đọc completed marker
hợp lệ, nhưng không bỏ current access guard. Exact normalized action registry/
completion-ref encoding/outcome rules còn SC-02; đây là draft shape để rà.

Unknown outcome: đọc current object/task và lookup đúng role/action để đối chiếu
intent, marker absence không proof rollback/có thể còn commit/cleanup/window.
Không auto retry/new UUID/re-save config/re-activate/đổi status hoặc reassign
Teacher để lấy kết quả. Retry chủ đích cùng intent giữ UUID và normalized content/
versions/set; khác content cùng key409. Replay quyền hiện tại trước stale checks
mutation mới, không effects/audit thứ hai;≥24h marker không dedup vô hạn.

API-090 định nghĩa lookup chung **không** hoàn thành schemas Class/Schedule/
Attendance/draft/whole-Class grades API-091–149. Các target keys trên vẫn draft
refinements cần đồng bộ actual per-domain schemas sau, không endpoint thứ155.

### 15.10. Transaction và kiểm chứng thiết kế

W13 Course/Program save units dùng một Service/connection/transaction cho all
effects/mandatory allowlist old-new audit/T23. Serialize price edit–Invoice
issuance; EC:PROGRAM first-Class–config/return-to-draft; BANDSET first-result/
threshold freeze, TG01–03 backstops. Không invoice recompute/cascade/trigger
audit substitutes. Read/preview coherence, trigger OUTPUT/DML order, exact audit
fields/SQL signatures/grants/locks/isolation còn SC-03, chưa provision/run SQL.

Targets **sau triển khai**: immutable normalized Course code/duplicate inactive;
all role projections/catalogue-vs-Student financial privacy; price vs issuance;
DRAFT empty/incomplete/invalid precision/no autoscale; duplicate Course-Band;
rename/state used allowed vs protected config; cancelled Class still freeze;
exact full component memberships/RVs/removal/add IDs/no partial save; unique
name/final-exam swap order; activation sum/one exam/live stale; Class-first-use
races; Band DB thresholds/first-completed0.00 freeze; lost assignment cuts
Teacher lookup/replay; role/action-pair isolation; unknown/replay/expiry/
mandatory audit rollback. Examples là JSON/doc checks, không executed API/SQL/
browser/concurrency tests hoặc acceptance evidence.

## 16. AcademicAffairs Class — API-091–103 / W14 / F19–20

Mọi path dưới /api/v1/academics. Chỉ AcademicAffairs NORMAL; CSRF/Origin cho
POST/PUT, no-store, UUID cho8 commands Class/placement nêu trong catalogue.
Không generic PATCH/code/program/state/Teacher edit; không delete/reopen/
transfer/remove-assignment hoặc tự suy Class state theo clock. Class program/
code bất biến, một Teacher; capacity1–15, size đếm Assignment, không client input.
API-090 lookup đã đặc tả ở mục15.9; không thêm caller-result route mới.

### 16.1. Class projections và list — API-091

ClassStatus = PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED. Mọi property trong bảng R,
nullable chỉ khi nêu rõ. Class references không chứa Student/financial/grade
data; Teacher reference không là Account Admin DTO.

| Type | Fields / kiểu | Scope / meaning |
| --- | --- | --- |
| ClassProgramReference | id BusinessId; name string1–150; course CourseReference; band BandReference; status DRAFT/ACTIVE/INACTIVE; expectedLessonCount integer dương SQL int. | Parent program hiện tại; Course price/components/Student grade values không có. |
| ClassTeacherReference | accountId BusinessId; displayName string1–100; status ACTIVE/LOCKED/DISABLED. | Chỉ người phụ trách/Teacher options trong task; không email/phone/hash/security_version/throttle/temp credential/profile. |
| AcademicClassSummary | id BusinessId; code string uppercase1–30 theo ADR-079; program ClassProgramReference; teacher ClassTeacherReference; status ClassStatus; maxSize integer1–15; placedCount integer0–15; createdAt Instant; version RowVersion. | Count coherent từ Assignment, không count joins Schedule/Attendance hoặc gán cached current_size. |
| ClassPrerequisites | expectedLessonCount integer>0; currentLessonCount/heldLessonCount integer≥0; currentFinalExamCount/heldFinalExamCount integer≥0; markedLessonAttendanceCount/requiredLessonAttendanceCount integer≥0; firstLessonStartAt Instant hoặc null. | Counts theo current obligations/cohort; absent=false vẫn marked, chưa nhập không thành vắng; không score/publication counts. |
| AssignmentReference | id BusinessId; registrationId BusinessId. | T15 immutable, **không RowVersion**; refs không được tự thêm/gỡ bởi client. |
| ScheduleGuardReference | id/classId BusinessId; obligationKey UUID; sessionType LESSON/FINAL_EXAM; isCurrent boolean; replacesScheduleId BusinessId hoặc null; version RowVersion. | Chỉ lịch của target Class; full retained set khác current obligation set, không count replacement chain thành nhiều buổi. |
| AcademicClassGuardContext | program/teacher RowVersion mỗi trường; assignments array AssignmentReference; schedules array ScheduleGuardReference. | Full exact target context, không page roster hoặc Teacher Score/GradeReport DRAFT snapshots. |
| AcademicClassDetail | AcademicClassSummary +prerequisites ClassPrerequisites; cancellation {at: Instant, reason: string1–1000} hoặc null; allowedActions array AcademicClassAction; disabledReasons object action→string; guardContext AcademicClassGuardContext. | Context để capacity/preview; chưa full Schedule DTO/API schemas gói sau, không cấp quyền grade/Student profile/Payment. |

classAssignmentId trong các DTO mới và class.assignmentId của own Registration
mục9.1 đều map cùng T15.class_assignment_id, không hai business identifiers
hoặc thêm field DB; spelling per-route vẫn draft để rà compatibility SC-02.

Draft AcademicClassAction CREATE_SCHEDULE/EDIT_CAPACITY/CHANGE_TEACHER/
PREVIEW_TRANSITION/OPEN/START/COMPLETE/CANCEL/VIEW_ROSTER/PLACE_REGISTRATION
là capability keys, không canonical T23 strings hoặc client authorization.
Future timetable action chỉ link đến scoped workflow, không claim API-114–130
đã có schema. ClassCommandResult: class R AcademicClassDetail, operationId R UUID.
Blocked Teacher vẫn giữ reference/cảnh báo, không auto chọn người khác/hủy lớp.
Count/prerequisite presentation không là reservation hoặc commit guarantee.

API-091 GET /classes query classCode O normalized exact-code string1–30,
courseId/bandId/programId/teacherId O BusinessId exact filters, status O
ClassStatus (omitted tất cả). sort O CODE_ASC/CODE_DESC default CODE_ASC,
code/id cùng chiều; page/pageSize mục1.2. Chỉ non-PII filters ở URL; không
Student name/email/phone query.200 page AcademicClassSummary, empty200;
invalid/malformed/blank supplied filter400/auth401/403. Search/sort spelling
vẫn proposed wire refinement, không business uniqueness/lifecycle thay đổi.

### 16.2. Create PLANNED và detail — API-092–093

API-092 POST /classes: CreateClassRequest classCode R string theo ADR-079,
programId/teacherId R BusinessId, maxSize R integer1–15,
expectedVersions R {program: RowVersion, teacher: RowVersion,
components: array VersionedReference}, expectedSet R {componentIds:
array BusinessId}; UUID R.
Program/Teacher tokens bound đúng body IDs; components full saved Program set,
unique IDs/one-to-one RV matching; new Class chưa RV. Đọc Program API-083 và
Teacher options API-095, không client tự tạo token. Missing/type/encoding/
unknown property400, unknown scoped relations404, stale/set/state/unique409.

Program ACTIVE/cấu hình valid exact sum100/one FINAL_EXAM/count>0, Teacher
ACTIVE và đúng role; Program INACTIVE không tạo Class mới. Không invent Course
ACTIVE guard để cắt quy trình cũ. Code normalize trim/uppercase trước unique
toàn lịch sử, no suffix generation/reuse CANCELLED code. Body không status/id/
program freeze flag/placedCount/Student array/name mới cho Class.
201 ClassCommandResult status PLANNED; TX07 Class+first-use config gate/freeze+
mandatory audit+T23 atomic. Chưa có Schedule/Assignment/GradeReport mặc định.
Class đầu tiên, kể cả hủy sau đó, khóa Program protected config; tạo Class khác
cùng Program không reset first use hoặc tạo version mới. Gates/TG01–02 vẫn
bảo vệ concurrent component/config/state changes, không chỉ GET/RV.

API-093 GET /classes/{cId}: BusinessId, no business body/query,
200 AcademicClassDetail mọi lifecycle. Missing404/ID400/auth chung.
Full retained schedules/immutable cohort refs trong guardContext đọc coherent,
không giả Class RV đã track child-only Schedule/Attendance/Assignment changes.
Read không transition/touch RV/HELD/audit mutation hoặc trả Teacher DRAFT values.

### 16.3. Capacity save — API-094

PUT /classes/{cId}/capacity: maxSize R integer1–15, expectedVersions R
{class: RowVersion}, expectedSet R {assignmentIds: array BusinessId}; UUID R.
Full current cohort IDs, explicit[] nếu trống, unique/no page subset; IDs immutable
không fake Assignment RV. Class RV bảo vệ source capacity/state/Teacher, live
count/membership serialize với placement dưới EC:CLASS, không tin placedCount.

Chỉ PLANNED/OPEN trong accepted pre-ONGOING/pre-teaching window; maxSize không
dưới placed size. Không implicit đổi code/program/Teacher/status hoặc “force
shrink”/remove Student.200 ClassCommandResult, Class capacity/audit/T23 atomic.
400 range/type/token/duplicate set IDs;404 missing;409 stale/member change/
window/state/under-count. Same-value/no-op400 đã Accepted ADR-159 sau valid current context, không invent
mandatory reason hoặc fake capacity-history workflow riêng.

### 16.4. Teacher options — API-095

GET /class-teacher-options query classId O BusinessId; không business body/
arbitrary role/student/email/name search. Omitted là create context; có classId
thì target Class phải readable, dùng full current schedule/task window.
200 ClassTeacherOptionSet: items R array ClassTeacherOption, currentTeacherId R
BusinessId hoặc null (create=null).
ClassTeacherOption = ClassTeacherReference +version R RowVersion,
eligible R boolean, blockedReasons R array string. Đề xuất full minimal set,
displayName ASC/accountId ASC; không pagination ngầm cắt Teachers tại100.
Only Teacher accounts trong task picker, không global Account directory dump.

Role/status/window/conflicts kiểm theo context; blocked/current Teacher có
thể được hiển thị rõ nhưng không selectable khi không eligible. Chọn same-current
không là reassign. Option không full Teacher calendar/contact/current student's
data hoặc Admin credentials. Missing Class404/malformed400/auth chung.
Không reservation/guarantee available lúc save hoặc quyền enable/reset Account.

### 16.5. Teacher-change preview/command — API-096–097

API-096 POST /classes/{cId}/teacher-change-preview: teacherId R BusinessId,
no UUID/no versions/actor/reason input. Existing source/current Teacher và
proposed Teacher resolver theo task, không generic staff substitution.
200 ClassTeacherChangePreview: class R AcademicClassSummary; proposedTeacher R
ClassTeacherReference; eligible R boolean; blockedReasons R array string;
conflicts R array ClassConflictSummary; manifest R TeacherChangeManifest hoặc null.
Known Teacher blocked/window/conflict/same-current trả eligible=false, no marker/
DML. Malformed400/target relation missing404/auth chung. Read coherent/no lock
qua confirmation; change selection invalidates preview.

ClassConflictSummary mọi properties R: kind enum CLASS/ROOM/TEACHER/STUDENT;
startAt/endAt Instant; message string scope-safe. No arbitrary foreign entity/
Student identity/calendar/contact/grade/Payment fields; không source object
refs ngoài bốn trường đã liệt kê hoặc raw conflict row từ query. Chẩn đoán
target riêng chỉ qua detail được phép. Exact keys vẫn draft, không extra
visibility từ guard protocol.

TeacherChangeManifest: expectedVersions R {class/program/currentTeacher/
proposedTeacher: RowVersion, schedules: array VersionedReference};
expectedSet R {assignmentIds: array BusinessId, scheduleBindings:
array ScheduleGuardReference **trừ version**, hasAttendance: literal false,
hasScores: literal false}.
Schedules/token bindings full retained target Class context; current occupancy
only current applicable schedules, NOT_HELD history không reserve resources.
Parent/Teachers đúng preview/body teacherId, IDs/version arrays one-to-one.
No false flags prove DB absence; server phải kiểm lại no Attendance/Score.

API-097 POST /classes/{cId}/change-teacher: teacherId R BusinessId,
expectedVersions/expectedSet R manifest trên, UUID R. Chỉ trước ONGOING/actual
teaching/no Attendance/Score, proposed Teacher ACTIVE/role Teacher, no overlap
với **toàn bộ** applicable Class/test assignments của người mới; room/Student
invariants giữ. No mandatory reason hoặc Teacher-change history workflow mới.
Clock không sinh RV; absence/actual-teaching/time predicates theo E01/E02 phải
recheck live, không suy quyền từ preview và không auto state transition.
Same-current Teacher new intent400 theo RF04/ADR-159 sau valid current context;
no reassignment/provenance/audit/completion; stale/window vẫn409.

200 ClassCommandResult; Teacher link+mandatory scoped audit+T23 atomic.
400 malformed/token/binding;404 missing;409 state/window/dependencies/stale/
membership/conflict/Teacher status change. Account blocked không auto reassign;
ngoài window chờ Admin restore hợp lệ, không override giữa khóa. Teacher cũ
mất Class list/detail/roster/replay/lookup ngay theo current assignment; lịch
sử từng dạy không giữ task access. Không sửa Attendance/Score/profiles.

### 16.6. Transition preview và related context — API-098

POST /classes/{cId}/transition-preview body action R OPEN/START/COMPLETE/CANCEL,
không reason/versions/client actualStart/HELD/input score. CSRF/no-store/read-only,
không UUID/T23, không persist “preview approved”.
200 ClassTransitionPreview: class R AcademicClassSummary; action R same enum;
prerequisites R ClassPrerequisites; eligible R boolean; blockedReasons R array
string; effects R array string scope-safe; manifest R typed manifest theo action
hoặc null. Valid request nhưng thiếu prerequisite/wrong source state trả200
blocked, không sửa Class/lịch/điểm danh.400 unknown action/shape,404missing/auth chung.
No full DRAFT GradeReport hoặc per-Student scores trong effect/guard response.

| Action / manifest | expectedVersions R object | expectedSet R object |
| --- | --- | --- |
| OPEN / ClassOpenManifest | class/program/teacher RowVersion; schedules array VersionedReference. | assignmentIds array BusinessId; scheduleBindings full array ScheduleGuardReference trừ version. |
| START / ClassStartManifest | class/program/teacher RowVersion; schedules array VersionedReference. | assignmentIds array BusinessId; scheduleBindings full array ScheduleGuardReference trừ version. |
| CANCEL / ClassCancelManifest | class/program/teacher RowVersion; schedules array VersionedReference. | assignmentIds full array (eligible phải[]); scheduleBindings full array; hasAttendance/hasScores literal false. |
| COMPLETE / ClassCompleteManifest | class/program/teacher RowVersion; schedules array VersionedReference; attendances array VersionedReference. | assignmentIds array BusinessId; scheduleBindings full array; attendanceBindings array {attendanceId/scheduleId/classAssignmentId R BusinessId}. |

scheduleBindings trong **mọi manifest** là ScheduleGuardReference trừ version;
tokens keyed id tương ứng full retained schedules, membership includes history.
Class.program/Teacher bindings resolve từ persisted Class, không selectors để
đổi target; parent RV không substitute child tokens/sets. Cohort IDs unique/full/
immutable; array order canonicalization SC-02, không ký/giữ chỗ bằng manifest.
AttendanceBindings COMPLETE exactly mỗi current LESSON×current Assignment,
tương ứng attendances IDs/RVs one-to-one; không FINAL_EXAM/history NOT_HELD/
peer Class rows, không present=true prerequisite. Unrecorded thiếu row chặn
eligible/manifest=null, không fabricate null/0/absent hoặc checksum counts thay
coverage. Child value correction RV stale cũng409 dù parent Class RV không đổi.

Program/Teacher context không tự đặt ACTIVE như điều kiện mới cho **mọi**
transition; OPEN áp published ACTIVE/Teacher-valid guard, các hành động còn lại
giữ riêng E01/E02. Blocked teacher cảnh báo/current rights như baseline, không
auto replacement hoặc thêm grade-finalized để complete. ClassStatus không
suy từ clock; server checks now/actual-teaching evidence/children dưới gates.
Missing/invalid tokens/duplicate IDs/internal bindings400; live changed set/
stale/time/state/prerequisites409. Read preview không reservation qua confirm.

### 16.7. Open/start/complete/cancel — API-099–102

Mỗi route body R expectedVersions/expectedSet đúng action manifest mục16.6,
UUID R; không action/status property trong command khác route. API-102 thêm
reason R string NFC/trim1–1000, plain text; các route còn lại không mandatory
reason. Không client actor/time/code/program/roster selection hoặc force.

| API / POST suffix | Live prerequisites / effect |
| --- | --- |
| API-099 /classes/{cId}/open | PLANNED→OPEN; Program ACTIVE/Teacher hợp lệ, ít nhất một current valid future LESSON và resource conflicts cleared; **chưa** bắt đủ expected LESSON hoặc FINAL_EXAM tại opening. |
| API-100 /classes/{cId}/start | OPEN→ONGOING, user xác nhận actual teaching, server now không trước first current LESSON;≥1 properly placed Student, đủ expected LESSON schedules. FINAL_EXAM riêng, không tự tạo exam hoặc invent full-exam-before-start prerequisite. |
| API-101 /classes/{cId}/complete | ONGOING→COMPLETED; đúng full required LESSON obligations và một FINAL_EXAM, mọi current required occurrence HELD, LESSON coverage đủ cohort. Marked present=false hợp lệ; no-score/submit/finalize/pass prerequisite. |
| API-102 /classes/{cId}/cancel | PLANNED/OPEN trống/chưa dạy/no Assignment; reason mandatory. CANCELLED giữ schedules/history, release effective Class/Teacher/room occupancy; không refund/transfer/cancel paid waiting Registrations hoặc reset Program freeze. |

200 ClassCommandResult cho mutation thành công, current scoped detail+server
operationId. Input/token/reason shape400, target404, stale/state/capacity/
prerequisites/clock/race409. Một confirmation đúng tác động, không tự mark
HELD/Attendance hoặc start/complete vì lịch đã qua. COMPLETE chỉ khóa Attendance;
Registration chưa kết thúc/is_unfinished chưa clear tới GradeReport FINALIZED,
không auto submit/publication. FINAL_EXAM completion không cần Attendance mới.
Cancelled old chain không hiện upcoming/held progress; no physical purge.
Replay current rights trước guards new transition, no duplicate effects/audit;
same-value state command không làm implicit reopen/no-op transition.

### 16.8. Academic roster — API-103

POST /classes/{cId}/roster/search: ClassRosterSearchRequest name/email/phone O
string normalization/literal match mục10.2, page/pageSize mục1.2, sort O
ASSIGNED_ASC/ASSIGNED_DESC default ASSIGNED_ASC (assignedAt/id cùng chiều).
Không bookingStatus/view/filterCourse để lẫn roster EntryTestSession; no GET PII.
200 page AcademicClassRosterRow; targetClass missing404/shape400/auth chung,
empty200. Read-only/no UUID/T23, no global Student directory/export/print.

Mọi fields R: classAssignmentId/registrationId BusinessId; assignedAt Instant;
student StudentContactReference; course CourseReference; band BandReference;
entryScore EntryScore; invoiceStatus UNPAID/PAID/CANCELLED.
Band/entryScore là parent best completed entry result phục vụ xếp/quản lý lớp,
không Teacher training draft. Assignment từ valid ADMITTED/PAID/Band placement,
không default seed/snapshot Student; current profile contact theo ADR-039.
No Invoice ID/amount/Payment metadata, DOB/address, GradeReport DRAFT values/
scores/comment/hidden fields null. Reads không recalculate result/change Band;
inconsistent persisted placement links là integrity issue, không fabricate0.

## 17. Placement — API-104–108 / W15 / F22

Chỉ AcademicAffairs NORMAL dưới /api/v1/academics; Student không self-place,
Teacher không placement/candidate directory. Single Registration save unit,
no bulk/transfer/remove/auto-class/no late placement. Invoice **status-only**,
opaque token/context không Finance amount/Payment method/reference permission.

### 17.1. Candidate search/detail — API-104–105

CandidateSummary mọi fields R: registrationId BusinessId; student
StudentContactReference; course CourseReference; band BandReference;
entryScore EntryScore; registrationStatus literal ADMITTED;
invoiceStatus literal PAID; admittedAt Instant; registrationVersion RowVersion.
Projection chỉ currently eligible **unassigned** ADMITTED/PAID/correct Band
Registration, không toàn bộ StudentProfile hoặc source enrollment histories.
Academic CandidateDetail = summary +bandAssignment R {id: BusinessId,
version: RowVersion}; invoice R {id: BusinessId, status: literal PAID,
version: RowVersion}; assignment R literal null.
Invoice ID/token ở targeted guard detail dùng binding/concurrency, không number/
amount/due-date/Payment metadata hoặc mở proxy Finance API.

API-104 POST /placement-candidates/search: name/email/phone O string mục10.2,
courseId/bandId O BusinessId exact filters; page/pageSize mục1.2; sort O
ADMITTED_ASC/ADMITTED_DESC default ADMITTED_ASC (admittedAt/registrationId cùng
chiều). All omitted = full scoped eligible set; malformed400; empty200 page
CandidateSummary, không automatic create-Class/refund/change Band.
PII body/RAM/no URL/browser durable draft/raw logs; read-only no UUID/T23.

API-105 GET /placement-candidates/{rId}: BusinessId/no business body/query,
200 CandidateDetail khi còn candidate scope. Missing/already assigned/no longer
eligible404 same scope-safe response (không GradeReport DRAFT hoặc record dump).
Command đã prepare nhưng live eligibility đổi thì409 theo guards, không dùng
404 detail để suy placement thành công/thất bại. Lookup/refetch task/own marker
đúng scope để đối chiếu; static candidate detail không reservation.

### 17.2. Suitable Class options — API-106

GET /placement-candidates/{rId}/classes page/pageSize mục1.2,
sort O CODE_ASC/CODE_DESC default CODE_ASC, Class code/id cùng chiều.
200 page SuitableClassOption: class R AcademicClassSummary, eligible R boolean,
blockedReasons R array string, conflicts R array ClassConflictSummary.
Query candidate hiện eligible scope; missing/not eligible404/input400/auth chung.
Base options Class OPEN/same Course-Band/remaining capacity; live Student
conflicts/related eligibility reflected before selecting, no hidden foreign
Student contact/calendar in summary. Không PLANNED/ONGOING/COMPLETED/CANCELLED
hoặc full Class list để tự bypass matching; entries không reserving last slot.

No options/items=[] nghĩa đang chờ lớp phù hợp, không trạng thái Registration
mới hoặc lỗi buộc tạo lớp/đổi Band/chuyển lớp/hoàn tiền. Existing OPEN workflow
không tự dừng bởi Course/Program INACTIVE ngoài accepted lifecycle gates.
Option eligible=false chỉ nói task/conflicts đã biết; server vẫn recheck commit.

### 17.3. Placement preview và full context — API-107

POST /classes/{cId}/placement-preview: registrationId R BusinessId; không
Student selector/amount/PAID flag/versions/actor/body target khác.
200 PlacementPreview: candidate R CandidateDetail; class R AcademicClassSummary;
eligible R boolean; blockedReasons R array string; conflicts R array
ClassConflictSummary; manifest R PlacementManifest hoặc null.
Target Class readable nhưng wrong state/matching/slot/conflict trả200 blocked;
candidate missing/outside currently eligible scope404, malformed400/auth chung.
No Assignment/GradeReport/audit-mutation/replay marker/resource reservation.

StudentTimetableGuard là **scoped minimal concurrency context** của Student
resolve từ target Registration, không general read API:
bookings R array StudentBookingGuard; assignments R array StudentAssignmentGuard;
schedules R array StudentScheduleGuard.
StudentBookingGuard fields R bookingId/attemptId/sessionId BusinessId,
bookingVersion/sessionVersion RowVersion. Chỉ relevant SCHEDULED active/non-
cancelled/effective reservations, current Student; không score/contact/roster.
StudentAssignmentGuard fields R classAssignmentId/classId BusinessId,
classVersion RowVersion; immutable link không fake Assignment RV.
StudentScheduleGuard fields R scheduleId/classId BusinessId, version RowVersion;
relevant current effective Class schedules cho Student assignments. No other
Students/contact/DRAFT values/Payment or room/Teacher profile dump.
Exact relevance boundary cùng server conflict policy E03; không cap theo page/
only dates mà bỏ relevant occupancy; không claim past test history reserves.

PlacementManifest expectedVersions R object:
registration/bandAssignment/invoice/class/program/teacher R RowVersion mỗi trường;
classSchedules R array VersionedReference.
expectedSet R object:
bandAssignmentId/invoiceId R BusinessId; registrationAssignmentId R literal null;
classAssignmentIds R full array BusinessId;
classScheduleBindings R full array ScheduleGuardReference trừ version;
studentTimetable R StudentTimetableGuard.
Full target-Class retained schedule tokens/set cùng source membership như
mục16.6; Student guard refs full relevant current bindings/versions, keyed unique/
IDs resolve từ verified candidate joins. No client studentId/Payment token.
Target Invoice/BandAssignment IDs/RVs đúng Registration; Class Program/Teacher
resolve parent, student assignments/booking refs không tự thêm quyền target.

Preview coherent chỉ trong read; mutations tới sau lấy REG/CLASS/TIMETABLE/
Invoice/current Account guards rồi reload/recheck full relevant sets. Rowversions
không proofabsence Assignment hoặc detect inserted future Schedule/Booking;
registrationAssignmentId=null/full sets và live uniqueness/conflicts phải được
kiểm lại, không tin caller's absence flag/size/count. New conflicting booking/
Schedule/time/status/membership ⇒409, không auto đổi Class/Band hoặc merge.
Manifest keys/relevance projection chưa accepted mới; SQL exact read/lock
mapping SC-03 còn phải chứng minh không lộ task khác qua raw guards.

### 17.4. One-Registration placement — API-108

POST /classes/{cId}/assignments: registrationId R BusinessId,
expectedVersions/expectedSet R PlacementManifest, UUID R.
Chỉ Class OPEN/same Course-Band/còn chỗ, Registration ADMITTED/Invoice PAID/
unassigned, relationships/valid resources/no effective Student overlaps; server
time/now và interval [start,end) dùng current timetable. No source Student/
Class/course/Band/status override/automatic waiting reservation.

201 PlacementCommandResult: assignment R {classAssignmentId/classId/
registrationId R BusinessId, assignedAt R Instant}; class R AcademicClassSummary;
operationId R UUID. TX08 **Assignment+DRAFT GradeReport+mandatory audit+T23**
all-or-none, same connection/TX; response không GradeReport id/status/score/
comment DRAFT, Student/Teacher visibility chỉ theo existing scoped APIs.
Không tạo Score/Attendance default hoặc default0/absent. Registration vẫn
ADMITTED, chỉ “đã xếp” derived từ immutable Assignment, placedCount re-read.

400 invalid/token/binding shape,404 missing visible target,409 stale/state/
PAID mismatch/changed set/duplicate assignment/last-slot/conflict. Một failure
rollback cả assignment/report/audit/completion, không create-report call2 từ UI.
Concurrent candidate detail mất scope không revoke committed caller's quyền
đọc safe placed-Class refs; unknown/replay dùng API-090 action-pair/current
AcademicAffairs rights, no result không proof rollback. Exact replay response
outcome SC-02, no second assignment/report/audit hoặc auto new UUID.
No endpoint undo/remove/reassign, kể cả chưa dạy; paid waiting records không
được hủy/refund qua placement/Class cancellation để né rule.

## 18. Teacher/Student Class reads — API-109–113 / F24/F11

Hai prefixes khác nhau và DTO allowlist riêng, không serialize chung EF entity
rồi set property cấm=null. NORMAL session/current Teacher assignment hoặc
Student ownership phải enforce mọi list/detail/roster/replay/lookup. No-store,
PII only body/RAM; read không UUID/T23/mutation/persist state.

### 18.1. Teacher current-task list/detail — API-109–110

TeacherClassSummary mọi fields R: id BusinessId, code string,
program ClassProgramReference, teacher ClassTeacherReference (verified current
Teacher), status ClassStatus, maxSize1–15, placedCount0–15, createdAt Instant.
TeacherClassDetail = summary +prerequisites R ClassPrerequisites,
cancellation R {at: Instant, reason: string1–1000} hoặc null.
Không AcademicClassGuardContext/management action/other Teacher options/
Invoice/Payment/DOB/address/global profiles hoặc GradeReport DRAFT từ Class read;
gradebook/Attendance routes gói sau trả task projection theo quyền riêng.
No Class-management mutation entitlement từ Teacher status/context fields.

API-109 GET /academics/teachers/me/classes query status O ClassStatus,
courseId O BusinessId, sort O CODE_ASC/CODE_DESC default CODE_ASC (code/id
cùng chiều), page/pageSize mục1.2. Scope **current assigned** qua teacher_id;
history COMPLETED/CANCELLED vẫn readable nếu còn được phân công, không rights
vĩnh viễn từ từng dạy.200 page TeacherClassSummary/empty200; invalid400/auth chung.

API-110 GET /academics/teachers/me/classes/{cId}: BusinessId/no business body/
query,200 TeacherClassDetail. Missing/not currently assigned404 cùng response,
không original-Teacher-ID query override/access nhờ audit/replay marker.
Account locked/disabled/revoked session chặn auth, không restore data từ RAM.
After reassignment clear stale task/form state, không request other Class scope.

### 18.2. Teacher roster riêng — API-111

POST /academics/teachers/me/classes/{cId}/roster/search: ClassRosterSearchRequest
mục16.8, current Teacher assignment; CSRF/Origin/no-store/read-only/no UUID.
200 page TeacherClassRosterRow fields R classAssignmentId/registrationId
BusinessId, assignedAt Instant, student StudentContactReference.
Không invoiceStatus (kể cả null)/amount/Payment/DOB/address/global Student dump,
không copy Academic roster finance field hoặc standalone profile edit.
Entry/context hoặc gradebook task data chỉ qua routes/projections đã đặc tả
theo task cần thiết, không invent Grade DRAFT visibility qua Class roster.
Missing/not assigned404/shape400/auth chung/empty200.
Read current profile contact, no snapshots; pagination không scope enlargement.

### 18.3. Student own current/history — API-112–113

StudentOwnClassSummary fields R id BusinessId, code string,
program ClassProgramReference, teacher R {displayName: string},
status ClassStatus, assignment R {classAssignmentId/registrationId: BusinessId,
assignedAt: Instant}. Teacher contact/status/admin token không Student directory.
StudentOwnClassDetail = summary +cancellation R {at: Instant,
reason: string1–1000} hoặc null. No peer roster/cohort IDs/placedCount/contact/
capacity editor/management allowedActions/guard manifests/unfinalized grades.
Link đến lịch/FINALIZED grade workflow riêng không giả gradebook schema đã có.

API-112 GET /students/me/classes query view O current/history (draft default
current), courseId O BusinessId, sort O ASSIGNED_ASC/ASSIGNED_DESC default
ASSIGNED_DESC (assignedAt/classAssignmentId cùng chiều), page/pageSize mục1.2.
Đề xuất current=own PLANNED/OPEN/ONGOING; history=own COMPLETED/CANCELLED.
Các buckets là read filter, không stored Class state hoặc unplaced candidate
Class. Ownership qua Assignment→Registration→verified Student, no studentId/
accountId query.200 page StudentOwnClassSummary, empty200; malformed400/auth chung.
Class hoàn thành chỉ xem; nhiều khóa/lượt chọn đúng own Assignment, không chuyển
lớp/học lại bằng đổi ClassID trên record.

API-113 GET /students/me/classes/{cId}: BusinessId, no business body/query,
200 StudentOwnClassDetail kể cả own history; missing/not-own404 cùng message.
Unassigned class không thuộc Student scope. Không Student caller-result Class
command hoặc selected Teacher task override. Teacher name/current Program refs
không sinh quyền đọc contact/other Students/GradeReport DRAFT.
Schedule/history/linked non-held replacement chi tiết theo next API schemas;
không double-count đã học/auto HELD hoặc công bố khi mới COMPLETED Class.

### 18.4. JSON examples và kiểm chứng thiết kế

Create Class dùng Program saved config full set và selected Teacher RV.
IDs/tokens giả, Idempotency-Key header theo shared contract, không body actor/
status/class RV mới hoặc initialized GradeReport/Score/Attendance.

```json
{
  "classCode": "L-NEN-01",
  "programId": 21,
  "teacherId": 51,
  "maxSize": 15,
  "expectedVersions": {
    "program": "AAAAAAAAAAE=",
    "teacher": "AAAAAAAAAAI=",
    "components": [
      { "id": 31, "version": "AAAAAAAAAAM=" },
      { "id": 32, "version": "AAAAAAAAAAQ=" }
    ]
  },
  "expectedSet": { "componentIds": [31, 32] }
}
```

Capacity edit của cohort hai Assignment immutable, không parent-only save hoặc
chọn subset theo trang. No client placedCount/assignment version.

```json
{
  "maxSize": 12,
  "expectedVersions": { "class": "AAAAAAAAAAU=" },
  "expectedSet": { "assignmentIds": [61, 62] }
}
```

Targets **sau triển khai**: all7 roles/three list-detail-roster projections;
first-Class vs config/Program activation; immutable code/program/no cancelled
code reuse; capacity vs last slot/start/cancel; exact immutable assignment/full
schedule/current-obligation sets, replacements counted once; Teacher change
vs Account block/related data/overlap and former Teacher access; explicit
opening partial LESSON/start full LESSON/no clock auto-state; completion vs
HELD/Attendance correction/full coverage, absent≠missing/no grade prerequisite;
candidate scope ADMITTED/PAID/no assigned, Invoice amount/Payment privacy;
placement duplicate/last-slot/conflicts/cancel/start, one-TX DRAFT no visibility/
defaults; lost response/current rights/replay expiry; Student no peers/only own
history/FINALIZED result scope. Exact SQL/audit/lock/isolation/outcome bytes
SC-02/03, no executed API/SQL/browser/concurrency tests.

## 19. Academic Schedule/calendar — API-114–116 / W16 / F21

Mọi path dưới /api/v1/academics; chỉ AcademicAffairs NORMAL. Calendar/detail
không roster, financial data hoặc Teacher DRAFT values. Teacher/Student có
paths/projections riêng ở mục23. No-store; GET không mutation, touch occurrence
hoặc T23. Lịch ca đầu vào dùng API đã đặc tả trước, không trộn Schedule/Booking.

### 19.1. Wire types và Academic calendar — API-114

R cho mọi property dưới đây, nullable chỉ nơi ghi rõ. Date/time là LocalDate VN/
MinuteTime mục12.1, cùng ngày/startTime<endTime, precision phút; UTC startAt/
endAt là Instant của **cùng** date/time, không timezone thiết bị.
Room trim/uppercase1–30 A–Z/0–9/_/- theo ADR-080, no Room entity/alias merging.
ScheduleType=LESSON/FINAL_EXAM; OccurrenceState=PLANNED/HELD/NOT_HELD.
Không Schedule CANCELLED enum: Class cancellation giữ row/history.

| Type | Fields / kiểu | Giới hạn |
| --- | --- | --- |
| AcademicScheduleClassReference | id BusinessId; code string; status ClassStatus; program ClassProgramReference; teacher ClassTeacherReference. | Parent Class/current Teacher, không Teacher contact/Account admin hoặc Student identities. |
| AcademicScheduleView | id BusinessId; class AcademicScheduleClassReference; type ScheduleType; date LocalDate; startTime/endTime MinuteTime; startAt/endAt Instant; room string mã chuẩn; occurrenceState OccurrenceState; isCurrent boolean; obligationKey UUID; replacesScheduleId/replacementScheduleId BusinessId hoặc null; currentScheduleId BusinessId; version RowVersion. | Direct predecessor/successor khác current end-of-chain; server-owned links, không counted history as extra LESSON. |
| ScheduleChainLink | scheduleId BusinessId; replacesScheduleId BusinessId hoặc null; date LocalDate; startTime/endTime MinuteTime; occurrenceState OccurrenceState; isCurrent boolean. | Full chain cùng Class/type/key; root→successor order, không Teacher/Student profile/management reason. |
| ScheduleDependencySummary | hasAttendance/hasLinkedScore boolean mỗi field. | Presence cho guard, không Score id/value/comment/DRAFT payload hoặc cohort PII. |
| AcademicScheduleDetail | AcademicScheduleView +held {actorAccountId: BusinessId, at: Instant} hoặc null; notHeld {actorAccountId: BusinessId, at: Instant, reason: string1–1000} hoặc null; replacementChain array ScheduleChainLink; dependencies ScheduleDependencySummary; allowedActions array AcademicScheduleAction; disabledReasons object action→string. | HELD/non-held provenance chỉ quản lý; no arbitrary entity/raw audit payload; target chains không mở rights của Class khác. |

Draft AcademicScheduleAction MOVE/REPLACE/CONFIRM_HELD/VIEW_CLASS là UI keys,
không T23 canonical action strings hoặc client status setters.
replacementScheduleId chỉ direct successor; currentScheduleId là terminal current
occurrence cùng nghĩa vụ. Root/current có successor=null; historical predecessor
không giả successor=current nếu chuỗi nhiều lần. Class CANCELLED vẫn có retained
isCurrent/link nhưng không giữ resource/upcoming hoặc count delivered lessons.
PLANNED tới giờ vẫn PLANNED cho tới command hợp lệ; missing Attendance/Score
không tự NOT_HELD/HELD hoặc điểm0.

API-114 GET /schedules query fromDate/toDate O LocalDate inclusive theo ngày
Schedule VN; from>to400. Đề xuất khi **cả hai omitted** dùng tuần VN hiện tại
thứ2–chủ nhật; khi chỉ một bound provided giữ one-sided filter, không cắt ngầm.
classId/courseId/teacherId O BusinessId; view O current/history default current;
sort O START_ASC/START_DESC default START_ASC (date/startTime/id cùng chiều);
page/pageSize mục1.2. Blank/malformed/unknown query400/auth401/403.
Current draft filter: isCurrent=true và Class PLANNED/OPEN/ONGOING; history:
toàn retained rows trong scoped date/filter, gồm current và COMPLETED/CANCELLED.
History có thể overlap current, không cộng hai buckets thành progress.
Các bucket/default/sort là wire proposals, không business state/lifecycle mới.

200 AcademicCalendarPage = page envelope AcademicScheduleView +dateWindow R
{fromDate: LocalDate hoặc null, toDate: LocalDate hoặc null}, các bounds đã áp
dụng. Empty200; calendar pages **không** full Class obligations proof.
Week grid/agenda phải chỉ rõ page/totalCount, không coi20/100 rows là toàn tuần
hoặc tự infinite-scroll/export/drag-resize. Query không Student name/email/
phone/actor/grade filters hoặc global read quyền role khác.

### 19.2. Academic detail — API-115

GET /schedules/{sId}: BusinessId/no business body/query,200
AcademicScheduleDetail current/history trong target Class read scope.
Missing/outside404/ID400/auth chung; old NOT_HELD và CANCELLED-Class records
readable nhưng no mutation actions. Full chain same Class/type/obligation,
provenance đúng old event/current links, không đổi Teacher lịch sử từ input
hoặc thêm separate Teacher-change history workflow.
hasAttendance/hasLinkedScore không data dump: AcademicAffairs không được đọc
DRAFT score value chỉ vì cần biết move/replace có bị chặn không.
Read không self-repair links/isCurrent/HELD hoặc reset dependencies.

### 19.3. Full scheduling context — API-116

GET /classes/{cId}/scheduling-context: BusinessId/no business body/query,
200 ClassSchedulingContext fields R:
class AcademicClassSummary; progress ObligationProgress; schedules array
ScheduleGuardReference; currentObligations array ScheduleGuardReference;
assignments array AssignmentReference; expectedVersions object
{class/program/teacher: RowVersion, schedules: array VersionedReference}.
ObligationProgress fields R expectedLessonCount integer>0, lessonObligationCount/
heldLessonObligationCount/remainingLessonObligationCount integer≥0,
finalExamScheduleId BusinessId hoặc null, heldFinalExamCount integer0–1.

schedules/versions full retained Class rows, currentObligations exact current
one/key, assignments full immutable cohort. No page/date-selected subset.
Count LESSON distinct obligations, old NOT_HELD/replacements không cộng thêm;
FINAL_EXAM riêng không dùng bù LESSON, same obligation/type/key links.
remaining=expected−current LESSON nghĩa vụ khi invariant đạt, không cap silently/
invent negative-to-zero repair. Missing/inconsistent DB set là integration
issue, không GET auto regenerate exam/rows.
Context độc lập calendar filters; membership/RVs là optimistic hints, không
reservation hoặc Class RV bao child insert/update. Target missing404/ID400/auth
chung. No Student/Teacher DRAFT/Invoice data; conflict-window resources/cohort
timelines chỉ prepare qua scoped preview của lệnh tương ứng, không global dump.

VW03/vw_CurrentScheduleOccurrences phục vụ current effective reads, history
query từ retained T16/Class; FN04/ufn_ClassObligationProgress cho counts/
context như E03. Trusted caller/current task scope enforced ở service/query,
không raw reporting role mở quyền web. Exact columns/signatures/EF keyless/
snapshot/grants/read-isolation còn SC-03, chưa SQL/runtime evidence.

## 20. Create-one và concrete weekly plan — API-117–120 / W16

AcademicAffairs NORMAL, CSRF/Origin/no-store cho POST; previews không UUID/
mutation marker, writes UUID. Roots server cấp obligationKey/id/current/state.
Không client isCurrent/occurrence/Teacher/Class override, không recurrence/
holiday job/auto FINAL_EXAM/partial date skipping.

### 20.1. Candidate, preview và common manifests — API-117

ScheduleCandidate fields R type LESSON/FINAL_EXAM, date LocalDate,
startTime/endTime MinuteTime, room normalized code.
API-117 POST /classes/{cId}/schedule-preview body candidate R ScheduleCandidate,
không expected versions/actor/arbitrary obligation key. Cấu trúc ngày/giờ/type/
room invalid400; Class missing404/auth chung. Live count/state/resource problems
trả200 ScheduleCreationPreview blocked, không fake mutation/row reservation.

ScheduleCreationPreview fields R class AcademicClassSummary, candidate
ScheduleCandidate normalized, progress ObligationProgress, eligible boolean,
blockedReasons array string, conflicts array ClassConflictSummary mục16.5,
manifest ScheduleCreateManifest hoặc null. Không Class/Teacher/Student contact
của conflict hoặc DRAFT/financial values. Khi input đổi phải re-preview; clock
qua cutoff vẫn recheck live, no client asOfUtc/deadline credential.

Create LESSON chỉ lấp nghĩa vụ còn thiếu, không vượt expected. FINAL_EXAM root
riêng/tối đa một current, sau last current LESSON: exam start≥last LESSON end
theo adjacency [start,end); chưa có LESSON để xác định ordering thì blocked.
Không tự đòi full LESSON count ở exam-create ngoài full-before-teaching rule,
không add exam cùng LESSON. Mọi post-plan current LESSON/exam ordering phải
hợp lệ; giữ lại exam, không tự dời/delete/sửa loại để xử lý.
Class không COMPLETED/CANCELLED, resources/current Teacher/assigned Student
invariants theo nguồn. Program INACTIVE không auto dừng Class đã tồn tại;
blocked Teacher báo rõ/no automatic reassignment/override.

**Clock refinement E04-RF03 đã Accepted03/10/2026 — ADR-156**: root create
LESSON/FINAL_EXAM chỉ khi candidate startAt>serverNow lúc kiểm tra lưu trong
transaction, cùng mọi row của weekly plan; destination của move cũng >now.
Tới/qua start sau input hợp lệ409; malformed date/time400. Một candidate lỗi
thì weekly batch không lưu phần nào/không skip/trim/auto move. Preview không
giữ clock/resource; server recheck độc lập RV. Không backfill; requirements/
NV19/W16 có addendum sau nghiệm thu cũ. Existing old-slot move/replacement/
HELD cutoffs giữ nguyên. Chỉ RF03 được duyệt, các wire refinements khác còn rà.

| Guard type | Required fields / nội dung |
| --- | --- |
| ResourceClassScheduleGuard | scheduleId/classId BusinessId; scheduleVersion/classVersion RowVersion. Relevant current effective occupancy, Class state/Teacher change phải được kiểm. |
| ResourceEntrySessionGuard | sessionId BusinessId; version RowVersion. Relevant non-CANCELLED Session room/grader occupancy, không roster/score/booking identities hoặc coi CLOSED đã release resources. |
| ResourceTimetableGuard | classSchedules array ResourceClassScheduleGuard; entrySessions array ResourceEntrySessionGuard. Full relevant resources cho normalized candidate windows/rooms/current Teacher, không page hay arbitrary center-wide request. |
| CohortTimetableGuardItem | classAssignmentId BusinessId; timetable StudentTimetableGuard mục17.3. Server resolve Student qua immutable T15→Registration, không Student selector/contact/grade. |

ScheduleBaseManifest:
expectedVersions R {class/program/teacher: RowVersion,
schedules: array VersionedReference};
expectedSet R {assignmentIds: array BusinessId,
scheduleBindings: array ScheduleGuardReference trừ version}.
Full target-Class retained schedules/cohort, current one/key predicate, unique
bindings/RVs one-to-one, [] explicit khi empty. No Assignment RowVersion.
ScheduleCreateManifest = base +expectedSet.resources R ResourceTimetableGuard,
expectedSet.cohortTimetables R array CohortTimetableGuardItem.
CohortTimetables exactly full target cohort IDs, each relevant current Student
booking/assignment/schedules IDs/versions; no page subset/foreign Student flags.

Resource/cohort relevance cùng conflict query của candidate/whole concrete plan,
không “chỉ rows đang xem” hoặc client whitelist exclude object để né conflict.
Minimal refs/RVs không grant đọc contact/calendar ngoài quyền; chỉ AcademicAffairs
preview, Teacher/Student calendars không nhận manifests. Context phải chứng minh
scope/read coherence/relevance mapping ở SC-03; không raw Score/Payment/PII rows.
Missing/invalid encoding/duplicate IDs/inconsistent request bindings400; live
changed membership/version/state/count/resources/clock guards409. New booking/
Schedule insert không tự touch parent RV: exact requery/live predicates dưới
TIMETABLE/CLASS gates mới bảo vệ, không chỉ token trên record đã tồn tại.

### 20.2. Create one — API-118

POST /classes/{cId}/schedules: candidate R ScheduleCandidate, expectedVersions/
expectedSet R ScheduleCreateManifest, UUID R. Same normalized intent như preview,
không “accept all conflicts”/client ROOT state/linked replacement ID.
Server reload full sources, check role/current parent/relationships/count/
ordering/overlaps [start,end)/clock theo rules đã chốt, gồm RF03/ADR-156.
Valid-shaped stale/state/count/conflict409; malformed400/missing404/auth chung.

201 ScheduleCommandResult: schedule R AcademicScheduleDetail, operationId R
UUID server. Root PLANNED/isCurrent=true/key server, Class/type fixed theo route/
input allowed, one-TX Schedule+mandatory allowlist audit+T23. No Score/Attendance
default, HELD/Class start/complete/automatic exam. Reusing same UUID completed
checks current caller rights/current projection, no second root/obligation/
audit. Different normalized content409; exact replay status/ref/outcome SC-02.
No-upgrade from create root into reasoned replace, no delete to recover conflicts.

### 20.3. Weekly preview — API-119

POST /classes/{cId}/weekly-schedule-preview: rule R WeeklyScheduleRule.
All rule fields R: fromDate/toDate LocalDate inclusive, from≤to;
weekdays array integer ISO1–7 (thứ2=1/chủ nhật=7), nonempty/unique;
startTime/endTime MinuteTime cùng ngày/start<end; room normalized code.
Không Teacher/holiday calendar/timezone/recurrence/selected-to-skip array.
Draft key/day spelling là wire choice, không class/calendar library mới.

200 WeeklySchedulePreview fields R class AcademicClassSummary, rule normalized
WeeklyScheduleRule, candidates array ScheduleCandidate (mọi type LESSON),
progress ObligationProgress, candidateCount integer≥0, eligible boolean,
blockedReasons array string, conflicts array WeeklyCandidateConflict,
manifest ScheduleCreateManifest hoặc null.
WeeklyCandidateConflict fields R candidateIndex integer≥0 (0-based),
candidate ScheduleCandidate, conflicts array ClassConflictSummary.
All concrete dates incl bounds và selected weekdays, sort date ASC/startTime;
candidateCount=candidates.length; no truncation đến remaining count, no skip
holidays/already scheduled/conflicting dates hoặc auto FINAL_EXAM.

No matching date →candidates=[]/eligible=false/manifest=null; valid range nhưng
quá remaining/một conflict →whole preview blocked, show all candidate conflicts.
Không partial eligible subset hoặc stored previewId/plan hash reservation.
Invalid rule400/target404/auth chung; read-only no UUID/T23. Count/plan render
không paginated subset; request size/timeouts còn SC-04, không tự chốt business
max-duration/max-lessons cap khác program. Owner đổi rule →invalidate preview.

### 20.4. Weekly whole-plan commit — API-120

POST /classes/{cId}/weekly-schedules: rule R WeeklyScheduleRule,
candidates R nonempty array ScheduleCandidate, expectedVersions/expectedSet R
ScheduleCreateManifest, UUID R. Mỗi item type literal LESSON, full concrete plan
đã xem; không editable count/skip dates/arbitrary obligation keys/temp IDs.
Server normalize/recompute dates từ rule và đối chiếu **đúng toàn plan**:
không added/omitted/duplicate/changed slot/type/room; malformed or rule-plan
mismatch400 field errors (draft wire outcome), live versions/resources/count/
state/clock409. Empty commit plan400, no DML/audit/marker mới để fake success.

201 WeeklyScheduleCommandResult fields R created array
{scheduleId: BusinessId, obligationKey: UUID}; context ClassSchedulingContext;
operationId UUID. created đầy đủ theo concrete plan order, mỗi root fresh server
key/ID; no persisted request snapshot/PII. TX09 mọi Schedule/audit/T23 cùng
connection/TX; một date/count/guard/audit failure rollback cả batch.
Không save some rows/auto trim/chuyển ngày sau preview để làm valid; user sửa
rule/re-preview hoặc tạo từng buổi bằng intent riêng. Race last remaining
obligation/count/resources không silent retry. Timeout không proof rollback;
refetch full Class context/lookup API-090 đúng caller/action để đối chiếu.

## 21. Move và reasoned replacement — API-121–124 / W16

AcademicAffairs NORMAL, no-store/CSRF; previews không UUID/DML, writes UUID.
Target current PLANNED occurrence; không move old NOT_HELD/HELD/history/
Class COMPLETED/CANCELLED hoặc xóa Attendance/Score để mở guard.
Old Class/type/obligation/root identity resolve server, không body selector.

### 21.1. Pre-start move preview — API-121

POST /schedules/{sId}/move-preview: newDate R LocalDate, startTime/endTime R
MinuteTime, room R normalized code. Không reason/type/Class/obligation/actor/
expectedVersions input. Known target guard/window/dependency/order/conflict
blocked →200 ScheduleMovePreview; malformed400/missing404/auth chung.

ScheduleMovePreview fields R before AcademicScheduleDetail, proposed
ScheduleCandidate (type server giữ), eligible boolean, blockedReasons array
string, conflicts array ClassConflictSummary, manifest ScheduleMoveManifest
hoặc null. Before/detail no Teacher DRAFT values; Class/Teacher current refs.
Old now<old startAt, current/no Attendance/no linked Score/non-ended Class
theo accepted ADR-032/148. New valid date/time/resources/post-plan exam ordering;
new startAt>now đã Accepted RF03/ADR-156 ngày03/10, kiểm lại lúc lưu;
old future predicate không thay thế việc kiểm destination.

ScheduleMoveManifest = ScheduleCreateManifest +expectedVersions.schedule R
RowVersion +expectedSet.targetScheduleId R BusinessId,
expectedSet.obligationKey R UUID, expectedSet.dependencies R
{scheduleId: BusinessId, hasAttendance: literal false, hasLinkedScore: literal false}.
Target schedule token bằng full schedules token cùng ID, binding đúng parent/
current occurrence/key. Resources/cohort windows của before+proposed để đối
chiếu occupancy; presence false không proof absence và không token trên row
chưa có. Live CLASS/TIMETABLE guard serialize với Teacher Attendance/linked
Score insert ngay cả khi parent RV không đổi.

### 21.2. Same-row move — API-122

PUT /schedules/{sId}/time-room: newDate/startTime/endTime/room R như preview,
expectedVersions/expectedSet R ScheduleMoveManifest, UUID R.
Server áp substitution đúng target khi kiểm conflicts/order; exclude **chính**
old slot khỏi self-overlap calculation, không omit nó khỏi full source versions/
membership hoặc ignore other current obligations/resources/Student bookings.

200 ScheduleCommandResult (mục20.2), same id/Class/type/key/isCurrent;
update date/time/room +old-new allowlist audit/T23 atomic. Không delete-create,
mark NOT_HELD/migrate dependencies/auto move exam/Class state changes.
Input/token400/missing404/stale/currentness/dependency/state/time/order/conflict
409. Clock crossed cutoff vẫn409 dù RV unchanged; completed same-key replay
current rights trước live checks cho new mutation. Same-value move400 theo
RF04/ADR-159 sau valid current context; no historical move/audit/new marker.

### 21.3. Replacement preview — API-123

POST /schedules/{sId}/replacement-preview: reason R plain text NFC/trim1–1000,
newDate/startTime/endTime/room R như move, không versions/Teacher/type/new key.
Old current PLANNED, now≥old startAt, Class chưa COMPLETED/CANCELLED,
**không Attendance hoặc linked Score**. Giáo vụ xác nhận cả lớp không tổ chức,
không infer từ ô trống/thời gian; new startAt>now future đã accepted ADR-036/148,
không phụ thuộc RF03. LESSON và FINAL_EXAM đều dùng same-obligation protocol.

200 ScheduleReplacementPreview fields R before AcademicScheduleDetail,
proposed ScheduleCandidate giữ type, reason normalized string1–1000,
eligible boolean, blockedReasons array string, conflicts array
ClassConflictSummary, manifest ScheduleReplacementManifest hoặc null.
Valid request bị blocked trả200/no mutation; malformed/missing reason400/
target404/auth chung. No fake ABSENT/0/xóa điểm/thay khác Class để bypass.
ScheduleReplacementManifest = ScheduleMoveManifest (full source/binding/
absence/resource/cohort context), nhưng action/source clock **khác move**;
không đem preview pre-start move làm authorization replacement hoặc reverse.

Preview tính post-plan bằng **thay** target old trong current nghĩa vụ, không
append extra LESSON. New same Class/type/key; old retained, replacement chain
no fork/cycle, one current/key tại commit. Exam ordering với latest post-plan
LESSON vẫn giữ; không tự dời exam đã lưu/chèn second exam hoặc multi-move.
Conflicts/check time/membership recheck, read không reserve new slot.

### 21.4. Atomic old/new replacement — API-124

POST /schedules/{sId}/replace: reason/newDate/startTime/endTime/room R,
expectedVersions/expectedSet R ScheduleReplacementManifest, UUID R.
Server resolve current predecessor/parent/type/key, verify exact full context/
currentness/no dependent data/both clocks/post-plan resources/exam ordering.
400 malformed/reason/token,404 missing,409 stale/state/time/absence/dependency/
current-chain/unique fork/count/order/conflict; no dependency erase/override.

201 ScheduleReplacementCommandResult fields R oldScheduleId/newScheduleId/
classId BusinessId; obligationKey UUID; operationId UUID.
TX09 old NOT_HELD/isCurrent=false/actor/time/reason +new PLANNED/isCurrent=true/
replaces_schedule_id=old ID/same Class-type-key +mandatory audit/T23 atomic.
Failure new insert/resource/audit/chain enforcement rollback old change;
no standalone “mark not held” committed trước tạo replacement.
Old date/time/room retained; no Attendance/Score carry-over/fake data/default.
One obligation unchanged, history row count có thể >expected; no add root key.
Current refs/provenance read qua scoped detail sau commit, không PII snapshot.
Repeated same-key no second successor/audit; new intent chống double replacement
bằng currentness/unique predecessor/locks, không hứa replay dedup vô hạn.

## 22. Confirm actual HELD — API-125–126 / W16

AcademicAffairs NORMAL, không Teacher/Student management right. HELD là xác
nhận riêng từng occurrence, khác Class complete/Attendance/Score. No undo/
automatic elapsed-time inference hoặc default HELD từ complete draft form.

### 22.1. After-end preview — API-125

POST /schedules/{sId}/held-preview: no business input/body, CSRF/no-store,
không UUID/T23.200 ScheduleHeldPreview fields R schedule AcademicScheduleDetail,
eligible boolean, blockedReasons array string, effects array string,
manifest ScheduleHeldManifest hoặc null.
Guard current PLANNED/non-ended Class ONGOING và scheduled end đã tới (now≥
endAt); không trước end, không infer occurrence từ Attendance/Score/null.
Exact inclusive boundary spelling là draft wire refinement của after-end rule,
không thêm24/48h deadline. Clock/RV independent, no preview reservation.

ScheduleHeldManifest = ScheduleBaseManifest +expectedVersions.schedule R
RowVersion +expectedSet.targetScheduleId R BusinessId,
expectedSet.obligationKey R UUID. Full target Schedule token/key/current binding
như move, nhưng **không** dependencies=false/resources/cohort-timetables inputs
để ép no Attendance/Score/Attendance-complete prerequisite cho HELD.
Class/program/Teacher context không tự thêm ACTIVE guard mới ngoài after-end/
ONGOING/current PLANNED rule đã chốt, blocked assignment không tự reassign.
Valid blocked context200/manifest=null, malformed400/missing404/auth chung.

### 22.2. Explicit HELD mutation — API-126

POST /schedules/{sId}/confirm-held body R expectedVersions/expectedSet đúng
ScheduleHeldManifest, UUID R. Không actor/time/HELD flag/present array/grade.
User xác nhận thực tế buổi đã tổ chức; server recheck after-end/ONGOING/current
PLANNED/source versions/membership/account quyền tới commit.

200 ScheduleCommandResult, target HELD/held actor-time +mandatory audit/T23
atomic. No undo/second HELD event/new obligation/auto Class COMPLETED/grade
submit/finalize/Attendance rows. Missing404/input-token400/stale/state/time/
currentness409; same-key completed rights/replay không confirm lần hai.
Class completion riêng API-101 vẫn cần mọi current required HELD và LESSON
Attendance coverage; HELD **không** đòi Attendance đủ để thực hiện. FINAL_EXAM
không tạo Attendance requirement mới. Serialize occurrence với Class complete/
replacement/other writes theo CLASS gates; no independent audit/replay commit.

## 23. Teacher/Student calendars — API-127–130 / F24/F11

NORMAL/current Teacher Class assignment hoặc Student own Assignment trên mọi
list/detail/linked-history access. Không scope bằng actor/Student query IDs,
không proxy Academic calendar. Class refs/projections role riêng; metadata/
guard context/management actor-reason/dependency/Score fields không trả null
thay cho property cấm. PII absent, no-store/no mutation/UUID/T23.

### 23.1. Role-specific calendar shapes và Teacher list — API-127

TeacherScheduleView = AcademicScheduleView **trừ version**, class thay bằng
TeacherScheduleClassReference {id: BusinessId, code: string, status:
ClassStatus, program: ClassProgramReference, teacher: ClassTeacherReference}.
Chỉ Class teacher_id=verified caller hiện tại; không other Teacher calendar/
profiles/contact hoặc event histories cấp quyền lâu dài.
TeacherScheduleDetail = TeacherScheduleView +replacementChain R array
ScheduleChainLink. No held/notHeld actor-reason/allowedActions/disabledReasons/
dependency booleans/manifests/Student contact/financial hoặc DRAFT grade payload.
Teacher Attendance/grade routes gói sau mới trả context viết đúng task, không
Calendar mutation entitlement hoặc HELD override.

API-127 GET /academics/teachers/me/schedules: fromDate/toDate O LocalDate,
classId O BusinessId, view O current/history, sort O START_ASC/START_DESC,
page/pageSize và omitted date-window semantics như mục19.1.
Không courseId/teacherId/room/global task filters ngoài inventory này.
200 TeacherCalendarPage = page envelope TeacherScheduleView +dateWindow R
{fromDate/toDate: LocalDate hoặc null}, applied scoped filter.
Provided classId missing/not currently assigned404, malformed400/auth chung;
omitted classId list chỉ current assignments/empty200.
History retained NOT_HELD/ended Class chỉ nếu **vẫn assigned**, không former-
Teacher access qua history/marker/bookmark/cached calendar hoặc chain links.

### 23.2. Teacher detail — API-128

GET /academics/teachers/me/schedules/{sId}: BusinessId/no body/query,
200 TeacherScheduleDetail, same current Class assignment predicate before
detail/chain projection. Missing/not assigned404 cùng message/ID400/auth chung.
Class COMPLETE/CANCELLED history không mutation permission; cancelled rows
không upcoming/current effective resource. Teacher reassigned thì mất toàn
target chain quyền xem, không call Academic detail để fetch reason/tokens.
Account blocked/revoked chặn session/task, clear stale RAM, no auto replacement.

### 23.3. Student own calendar list/detail — API-129–130

StudentScheduleView = AcademicScheduleView trừ version, class thay bằng
StudentScheduleClassReference {id: BusinessId, code: string, status:
ClassStatus, program: ClassProgramReference, teacher: {displayName: string}}.
StudentScheduleDetail = view +replacementChain R array ScheduleChainLink.
No Teacher accountId/status/contact/management action/actor/reason/dependency/
guard manifests/peer roster/unfinalized GradeReport hoặc Teacher draft values.
Own Class/occurrence/type/date/room/key/direct/current chain refs phục vụ lịch,
không API ca đầu vào giả làm Class Schedule.

API-129 GET /students/me/schedules query fromDate/toDate/classId/view/sort/
page/pageSize đúng Teacher input types/defaults/dateWindow (không teacherId/
accountId/studentId). Ownership Assignment→Registration→verified Student,
many own Classes/retakes giữ đúng context.200 StudentCalendarPage = page envelope
StudentScheduleView +dateWindow R applied bounds. Explicit Class outside-own/
missing404, malformed400/auth chung; omitted ClassId chỉ own set/empty200.
Completed history chỉ xem, old NOT_HELD generic label/linked replacement,
không double-count old/new hoặc render cancelled/upcoming as active.

API-130 GET /students/me/schedules/{sId}: BusinessId/no body/query,
200 StudentScheduleDetail; missing/not-own404 cùng message, invalid400/auth chung.
Every chain ref same owned Class, no guessing external sId/permanent Teacher
rights or peers. No managerial reason/actor fields kể cả null placeholder.
Grade finalized visibility dùng dedicated schema sau, không Class/Schedule
COMPLETED state tự công bố điểm. Read không mark HELD/debt/absence/idle heartbeat.

### 23.4. JSON examples, replay và kiểm chứng thiết kế

Weekly command tạo đúng hai LESSON concrete từ rule, fake IDs/tokens8 bytes.
Assume target Class valid/non-ended/expected count còn≥2/no conflicts; source
context full empty schedules/cohort. RF03 đã Accepted ADR-156, ví dụ không
imply runtime đã chạy hoặc nghiệm thu các wire schemas còn lại.

```json
{
  "rule": {
    "fromDate": "2026-10-05",
    "toDate": "2026-10-07",
    "weekdays": [1, 3],
    "startTime": "18:00",
    "endTime": "19:30",
    "room": "P101"
  },
  "candidates": [
    { "type": "LESSON", "date": "2026-10-05", "startTime": "18:00", "endTime": "19:30", "room": "P101" },
    { "type": "LESSON", "date": "2026-10-07", "startTime": "18:00", "endTime": "19:30", "room": "P101" }
  ],
  "expectedVersions": {
    "class": "AAAAAAAAAAE=",
    "program": "AAAAAAAAAAI=",
    "teacher": "AAAAAAAAAAM=",
    "schedules": []
  },
  "expectedSet": {
    "assignmentIds": [],
    "scheduleBindings": [],
    "resources": { "classSchedules": [], "entrySessions": [] },
    "cohortTimetables": []
  }
}
```

Replacement safe committed refs, no persisted PII/request snapshot/default grades.
Same UUID khác intent409; header Idempotency-Key không body actor/key/clock.

```json
{
  "oldScheduleId": 71,
  "newScheduleId": 72,
  "classId": 41,
  "obligationKey": "d10a34f1-7176-4b1b-b521-ea19f27a1c01",
  "operationId": "fc12994b-d625-488a-91a6-f21e6f548a9d"
}
```

API-090 allowlist Schedule5 vẫn current AcademicAffairs action pairs mục15.9:
create-one/move/HELD classId+scheduleId refs, weekly classId, replacement old/
new/class refs. Calendar Teacher/Student không nhận lookup mutation rights.
Unknown read full scoped Class scheduling context/detail hoặc caller lookup
để đối chiếu, no result không proof rollback/timeout no guarantee failed.
Retry chủ đích cùng intent/UUID/content/versions-sets, no auto partial save/
new UUID/re-HELD/replacement. Replay current rights trước stale checks new
mutation, current safe projection/no effects/audit thứ hai; marker≥24h không
dedup vô hạn hoặc recover PII snapshot.

Mục tiêu **sau triển khai**: all7 roles/three calendar-detail-chain projections/
no grade-finance/contact leakage; VN current-week/bounds/sort/no pagination
as full-set proof; minute/end adjacency/order/resource conflicts/clock RV-free
races; weekly full plan/input change/no skip/one-error rollback; root count/
one exam/last-current LESSON/old-history chains counted once/no fork;
moves/dependencies vs Attendance/Score insertion; replacement old/new rollback/
future/no score migration; Class cancelled release/upcoming exclusion;
Teacher reassignment cuts list/detail/chain/replay; own Student/history/no
management reason; HELD after-end/ONGOING/no Attendance prerequisite/no undo/
no auto-complete. RF03 create/move-target future cutoff đã Accepted ADR-156;
RF01/02 Accepted ADR-157/158; RF04 Accepted159; exact hashes/action/outcomes/SQL read/grants/lock/trigger/audit/runtime
SC-02/03/04 còn mở. JSON examples/static checks không executed API/SQL/tests.

## 24. Attendance một LESSON — API-131–135 / W17 / F26

Nguồn E01 NV24/E02 W17/ADR-033/083/124/135/149 và E03 T17/TX10/TG04.
Teacher NORMAL hiện assigned Class được xem/ghi; AcademicAffairs NORMAL chỉ
đọc để quản lý/completeness. Không Student Attendance route, Director/Admin/
Admissions/Accountant rights mới. GET no-store/không UUID/DML/T23; PUT
CSRF/Origin/no-store/UUID. IDs là BusinessId, không Student/actor override.

### 24.1. Wire types, counts và cửa sổ

R mọi property trừ ghi O; nullable chỉ nơi nói rõ. Types ở đây là wire drafts,
không thêm DB enum/fields hoặc biến trạng thái “Chưa ghi” thành Attendance row.

| Type | Fields / kiểu | Phạm vi và semantics |
| --- | --- | --- |
| AttendanceLessonReference | id/classId BusinessId; type literal LESSON; date LocalDate; startTime/endTime MinuteTime; startAt/endAt Instant; room string mã chuẩn; occurrenceState PLANNED/HELD/NOT_HELD; isCurrent boolean; obligationKey UUID; replacesScheduleId/replacementScheduleId BusinessId hoặc null; currentScheduleId BusinessId. | Same VN date/time như mục19; no Teacher/Student DRAFT/finance hoặc managerial reason/audit dump. |
| AttendanceRecord | id BusinessId; present integer literal0 hoặc1; enteredByAccountId BusinessId; enteredAt Instant; version RowVersion. | T17 provenance lần ghi gần nhất, không full Account/entity hoặc toàn history. Missing record là null, không present=null hoặc thêm enum ABSENT/UNRECORDED. |
| AttendanceCounts | rosterCount/recordedCount/unrecordedCount/presentCount/absentCount integer0–15; isComplete boolean. | recorded=present+absent; unrecorded=roster−recorded; isComplete=(unrecorded=0). Dùng full current immutable Assignment cohort, không selected delta/page. |
| AttendanceEditStatus | canEdit boolean; opensAt Instant; blockedReasons array string. | OpensAt=scheduled start, không end/HELD. No fixed per-lesson deadline; Class COMPLETED/CANCELLED/currentness/current Teacher được kiểm riêng. |
| LessonAttendanceSummaryItem | lesson AttendanceLessonReference; counts AttendanceCounts; countsTowardCompletion boolean; editing AttendanceEditStatus. | Historical/noncurrent/NOT_HELD và CANCELLED Class không count required current LESSON completeness. isComplete riêng không proof Class completion/HELD/minimum cohort. |
| TeacherAttendanceRow | classAssignmentId BusinessId; student StudentContactReference mục10; attendance AttendanceRecord hoặc null. | Current name/email/phone trong assigned task, không DOB/address/Invoice/Payment/grade/report fields. |
| AcademicAttendanceRecord | AttendanceRecord trừ version. | Giáo vụ được đọc recorded value/provenance, không token/chức năng Teacher mutation. |
| AcademicAttendanceRow | classAssignmentId BusinessId; student StudentContactReference; attendance AcademicAttendanceRecord hoặc null. | Full scoped cohort, không Teacher Score/DRAFT/comment/report-total kể cả null placeholders. |
| AcademicLessonAttendanceSummaryItem | lesson AttendanceLessonReference; counts AttendanceCounts; countsTowardCompletion boolean. | Không editing/writeContext/Teacher command actions. |

Counts là projection cho cohort hiện tại, **không** snapshot hồ sơ/roster tại
ngày quá khứ. Với historical occurrence vẫn ghi rõ không tính completeness
hiện hành, không lấy số missing làm buổi đã dạy/chưa dạy. Class không có cohort:
counts0/isComplete=true theo arithmetic, nhưng không đủ các guard hoàn thành
Class; không tự tạo attendance/HELD để làm count đẹp. Integrity mismatch
(ngoài cohort/current links sai) không GET self-repair hoặc clamp số liệu.

Teacher entry window: now≥current LESSON start, Class không COMPLETED/CANCELLED,
current occurrence không NOT_HELD, Teacher vẫn phụ trách. Không thêm Class
ONGOING-only prerequisite; PLANNED/OPEN không tự chuyển ONGOING vì nhập điểm
danh. PLANNED hoặc HELD occurrence hiện hành có thể ghi trong window hợp lệ.
Không đợi end/HELD, không hạn24/48h. Completed/history chỉ đọc, không undo/
override/delete Attendance hoặc nhập absent để thay non-held workflow.
Attendance correction không thay Class lifecycle hoặc GradeReport workflow;
GradeReport state không được tự thêm làm guard W17.

### 24.2. Teacher summary — API-131

GET /academics/teachers/me/classes/{cId}/attendance:
view O current/history default current; sort O START_ASC/START_DESC default
START_ASC; page/pageSize mục1.2. Draft current=isCurrent LESSON, kể cả Class
COMPLETED/history của Class còn assigned; history=toàn retained LESSON rows
gồm current. Không reuse active-calendar filter để giấu completed Attendance.
Bucket/default là wire refinement; không FINAL_EXAM/Student name/grade filter.

200 TeacherLessonAttendanceSummary fields R class TeacherClassSummary,
items array LessonAttendanceSummaryItem và page/pageSize/totalCount;
completionCoverage R AttendanceCompletionCoverage:
currentLessonCount integer≥0, requiredRecordCount/recordedRecordCount/
missingRecordCount integer≥0, isComplete boolean.
Coverage full current required LESSON×full cohort độc lập view/page; NOT_HELD
predecessors không cộng thêm obligations. Khi Class CANCELLED không coi retained
rows là obligation còn phải thực hiện: coverage counts0/isComplete=true là
vacuous projection, không permission complete. Không clamp coverage trên page
hoặc bắt Teacher fill mọi future lesson ngay lúc đọc.

List stable date/startTime/scheduleId cùng chiều, không duplicate chain counts.
Empty200; invalid query/ID400, missing/not currently assigned404 cùng response,
invalid session401/wrong role/restricted403. Không lịch sử quyền từ “từng dạy”.
GET không tự mark absence/HELD hoặc source completeness repair.

### 24.3. Teacher full sheet và chuẩn bị delta — API-132

GET /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance:
no business query/body, đúng Class/current Teacher; sId thuộc LESSON cùng Class.
200 TeacherAttendanceSheet fields R class TeacherClassSummary,
lesson AttendanceLessonReference, rows array TeacherAttendanceRow,
counts AttendanceCounts, countsTowardCompletion boolean, editing
AttendanceEditStatus, writeContext TeacherAttendanceWriteContext hoặc null.
Full cohort tối đa15 rows, sort classAssignmentId ASC, không paginate/filter
roster rồi gọi đó là whole cohort. Zero assignments trả sheet rows=[]; GET
không tạo defaults. Historical row vẫn readable nếu scoped, canEdit=false/
writeContext=null. FINAL_EXAM hoặc schedule ngoài Class/missing404, ID400/auth chung.

TeacherAttendanceWriteContext fields R:
expectedVersions {class: RowVersion, teacher: RowVersion, schedule: RowVersion};
expectedSet {assignmentIds: array BusinessId}.
Context chỉ non-null khi edit window hợp lệ; assignmentIds full immutable
T15 cohort, không Assignment RV. AttendanceRecord.id/version theo từng row
để chọn existing deltas; attendance=null để chuẩn bị expected absence.
GET full sheet **không** reserve absence/cohort/clock. Context/task/RVs phải
coherent theo SC-03, không tự giả parent RV tăng khi Attendance insert.

### 24.4. One-LESSON changed-values save — API-133

PUT cùng path API-132 body AttendanceDeltaRequest, UUID R ở header.
Fields R changes nonempty array AttendanceChange, expectedVersions và
expectedSet AttendanceSaveManifest.
AttendanceChange fields R classAssignmentId BusinessId, present integer0/1;
không studentId/attendanceId/enteredBy/enteredAt/state/actor/comment input.
Mỗi assignment tối đa một item, cùng roster/Class/LESSON. Changed selection
là save unit; cho một phần cohort, không yêu cầu mọi assignment có delta.
Empty/duplicate/non-integer/boolean/null/blank/unknown field400 (wire drafts);
omitted assignment giữ nguyên, không xóa/đánh absent/default present.

AttendanceSaveManifest = writeContext mục24.3, thêm:
expectedVersions.attendance R array VersionedReference của **existing changed**
Attendance rows; expectedSet.attendanceBindings R array
{classAssignmentId: BusinessId, attendanceId: BusinessId};
expectedSet.unrecordedAssignmentIds R array BusinessId cho **new changed** rows.
Bindings và RV one-to-one, unique IDs; changed assignment IDs = existing binding
IDs ∪ unrecordedAssignmentIds, disjoint/exact. Không bắt lấy RV Attendance
của Student không sửa, không đem page/selected changes thay full assignmentIds.
All arrays R, [] khi không có category; new absence không fake RowVersion.
Record IDs resolve đúng schedule/assignment, không chỉ FK tồn tại.

Shape/token/manifest association invalid400, target/task404/auth chung;
valid-shaped live RV/absence/cohort/currentness/state/time mismatch409 và
rollback **tất cả deltas**. T17 unique pair/server live query+CLASS gate
chặn concurrent insert, không silent upsert/update row người khác vừa tạo.
Class/Teacher/Schedule reloaded và Account/session guard tới commit; now
checked độc lập RV. Save serialize với Class complete/Schedule move-replace/
Teacher reassign-block và placement/cohort changes theo E03 protocol.

200 AttendanceSaveResult fields R classId/scheduleId BusinessId,
saved array {classAssignmentId: BusinessId, attendanceId: BusinessId,
version: RowVersion}, sheet TeacherAttendanceSheet, operationId UUID.
saved chỉ deltas đã lưu theo classAssignmentId ASC, sheet full refreshed
cohort/context đúng scope; không cached old response là historical snapshot.
TX10 tất cả row/provenance +audit khi accepted event contract yêu cầu +T23
completion cùng connection/TX. Không invent mandatory full-payload audit
mọi Attendance read/save; exact event field registry SC-03 còn mở.
Missing values còn thiếu, no GradeReport mutation/HELD/auto-complete.
RF04/ADR-159: validate toàn request trước equality; all-equal400/no marker,
mixed save actual changed subset atomically, unchanged giữ provenance;
stale một item aborts whole batch dù value equal. saved chỉ actual changed refs.

### 24.5. AcademicAffairs read-only summary/sheet — API-134–135

API-134 GET /academics/classes/{cId}/attendance: view/sort/page/pageSize như
API-131;200 AcademicAttendanceSummary fields R class AcademicClassSummary,
items array AcademicLessonAttendanceSummaryItem, page/pageSize/totalCount,
completionCoverage AttendanceCompletionCoverage. Full coverage độc lập page.
Scope AcademicAffairs management, không proxy Teacher hoặc actor selector.
Empty200/ID-query400/target404/auth401/403; no-store/no mutation.

API-135 GET /academics/classes/{cId}/lessons/{sId}/attendance:
no body/query,200 AcademicAttendanceSheet fields R class AcademicClassSummary,
lesson AttendanceLessonReference, rows array AcademicAttendanceRow,
counts AttendanceCounts, countsTowardCompletion boolean.
Historical/current LESSON cùng Class, full max15 roster/id ASC; wrong Class/
type/missing404 và input400/auth chung. No editing/writeContext/RowVersion
Attendance tokens/Teacher save routes/draft grade values. Read only;
derived counts không quyền sửa/chốt Grade/Class hoặc mark HELD tự động.

## 25. Teacher gradebook và per-Student draft — API-136–139 / W18 / F27

Nguồn NV25–26/NV38/ADR-024/028/034/083/124/135/149 và T07/T08/T14–19/
TX11/TG05/FN05. Teacher NORMAL **currently assigned** Class. Grade save khác
whole-Class submission/review và Student-own FINALIZED read API-140–149
ở mục27–29, không per-Student save tự chuyển whole-Class workflow. No-store, GET no UUID/mutation; PUT UUID/CSRF/Origin.
No Admin/AcademicAffairs DRAFT access qua DTO/base refs/lookup/cache.

### 25.1. Grade wire types, aggregate và editability

| Type | Fields / kiểu (R trừ ghi khác) | Semantics / giới hạn |
| --- | --- | --- |
| GradeScore | string decimal0–100, output fixed2, như EntryScore numeric constraint mục9. | Thang100; không float/number/percent/comma. Đây chỉ reuse representation, không entry-test Booking/Attempt semantics. |
| GradeScoreInput | string decimal0–100/max2 digits như EntryScoreInput mục13. | “0”, “49.99”, “100.00” hợp lệ; no blank/null/negative/exponent/comma/overprecision/rounding/coercion. Exact canonical spelling/digest SC-02. |
| GradeStudentReference | studentId BusinessId; fullName string current. | ID-name tối thiểu cho gradebook; no email/phone/DOB/address/financial snapshots/global profile access. |
| GradeAssignmentReference | classAssignmentId/registrationId BusinessId; student GradeStudentReference. | Immutable Class cohort context; không studentId selector trên write/GradeReport mới client cấp. |
| GradeScheduleReference | id/classId BusinessId; type LESSON/FINAL_EXAM; obligationKey UUID; date LocalDate; startTime/endTime MinuteTime; startAt/endAt Instant; room string mã chuẩn; occurrenceState PLANNED/HELD/NOT_HELD; isCurrent boolean; replacesScheduleId BusinessId hoặc null; version RowVersion. | Same Class/current đúng component kind khi ghi; read không expose managerial reason/actor/ref của Class khác. |
| TeacherScoreView | id/componentId BusinessId; score GradeScore; schedule GradeScheduleReference hoặc null; enteredByAccountId BusinessId; enteredAt Instant; version RowVersion. | One T18 row/component/assignment; missing là không có item. GENERAL schedule=null, linked có scoped Schedule; no default zero. |
| GradeReportStatus | DRAFT/SUBMITTED/RETURNED/FINALIZED. | Existing T19 states, không SAVED/INCOMPLETE enum mới. |
| TeacherGradeReportView | id BusinessId; status GradeReportStatus; teacherComment string≤1000 hoặc null; submittedAt/reviewedAt/finalizedAt Instant hoặc null; latestReturnReason string1–1000 hoặc null; finalScore GradeScore hoặc null; version RowVersion. | Latest metadata không toàn audit history; reason có thể retained sau resubmit theo T19. finalScore/finalizedAt chỉ có khi FINALIZED, không persist draft total. |
| GradeComputation | requiredComponentCount/enteredComponentCount integer≥0; missingComponentIds array BusinessId; isComplete boolean; total GradeScore hoặc null. | Full frozen configured set vs all saved scores, thiếu→total=null, no subtotal hoặc invented 0. Complete server decimal/half-up một lần. |
| GradeEditStatus | canEdit boolean; blockedReasons array string. | Class ONGOING/COMPLETED và report DRAFT/RETURNED/current Teacher; linked scores kiểm từng Schedule window riêng. |
| TeacherGradebookStudentRow | assignment GradeAssignmentReference; report TeacherGradeReportView; scores array TeacherScoreView; computation GradeComputation; editing GradeEditStatus; writeContext GradeDraftWriteContext hoặc null. | Mỗi assigned Student một row, kể cả report chưa có Score. No Student publication từ calculated total/status COMPLETED. |

ComponentView/ComponentWeight mục14 giữ full frozen Program components,
kinds GENERAL/LESSON/FINAL_EXAM/positive weights sum100; không thêm displayOrder/
component minima/10-point scale/pass Attendance threshold. Components order id
ASC, scores componentId ASC, missing IDs ASC. Teacher thấy own-task stored
DRAFT/RETURNED/SUBMITTED/FINALIZED; other roles không gọi Teacher route để xem.
Score/provenance/history không mở quyền sửa record ngoài target assignment.

GradeComputation.total từ **saved aggregate**, không unsaved form values.
Counts exactly full configured components, no sum row weights from filtered
page hoặc round each contribution. Required config invalid/missing mandatory
GradeReport dù Assignment đã tồn tại là integrity/integration issue; GET
không tự tạo report/Score/defaults/fix weight. Không thay missing→0/false pass.
Class COMPLETED không auto submit/finalize; FINALIZED passing chỉ finalScore≥50,
không đem draft total qua Student/report pass-rate routes.

GradeEditStatus.canEdit xét parent workflow, không nói mọi linked component
đã tới giờ hoặc mọi option hợp lệ. GENERAL không Schedule/time window ngoài
Class/report; LESSON/FINAL_EXAM từ scheduled start, current/type/cùng Class/
không NOT_HELD, không cần end/HELD. Program INACTIVE không auto dừng Class
đã tồn tại, không đòi new ACTIVE guard cho mọi draft save. SUBMITTED/FINALIZED
chỉ đọc; RETURNED vẫn editable theo window, giữ state/reason cho tới resubmit.
Không Teacher reason mới để sửa nháp/RETURNED nếu baseline không bắt.

### 25.2. Full assigned-Class gradebook — API-136

GET /academics/teachers/me/classes/{cId}/gradebook: no business body/query,
200 TeacherGradebook fields R class TeacherClassSummary, components array
ComponentView, items array TeacherGradebookStudentRow,
assignmentIds array BusinessId, context GradeConfigurationContext.
GradeConfigurationContext fields R expectedVersions
{class: RowVersion, program: RowVersion, teacher: RowVersion,
components: array VersionedReference}; expectedSet {componentIds: array BusinessId}.
Full frozen components, all assigned max15 Students/id ASC, no page/search/
selected Student subset. assignmentIds full current cohort for readable
gradebook, **không** biến save một Student thành all-cohort report save.
Zero assigned →items=[]/assignmentIds=[]200, no synthetic GradeReports.
No score averages/rank/peer profiles or import/export endpoints.
Missing/not assigned404/syntax400/auth chung; current Teacher check kể cả
COMPLETED Class và every row, no permanent access qua formerly assigned.
Gradebook array không chịu max100 pagination hoặc “load visible rows only”
để chứng minh completeness. Context coherence/read-isolation còn SC-03.

### 25.3. One-Student detail/context — API-137

GET /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft:
no body/query. aId immutable Assignment **thuộc route Class**;200
TeacherStudentGradeDetail fields R class TeacherClassSummary,
components array ComponentView, assignment GradeAssignmentReference,
report TeacherGradeReportView, scores array TeacherScoreView,
computation GradeComputation, editing GradeEditStatus,
writeContext GradeDraftWriteContext hoặc null.
Same row/aggregate semantics như API-136; SUBMITTED/FINALIZED vẫn read-only
Teacher task detail, tên route “grade-draft” không grant editing.
No report/Student selector để truy cập Class khác hoặc reset FINALIZED về DRAFT.
Class/assignment missing/not owned task404, ID400/auth chung. Existing required
report thiếu là integrity issue, không GET repair/201 auto-create.

GradeDraftWriteContext fields R:
expectedVersions {class/program/teacher/report: RowVersion,
components: array VersionedReference};
expectedSet {classAssignmentId/reportId: BusinessId,
componentIds: array BusinessId}.
class/program/teacher và full component tokens/set từ GradeConfigurationContext;
report token/ID của **một target**. Assignment không RV. writeContext null khi
parent edit window không hợp lệ, không chứa versions/scores của peer reports.
Stored Score IDs/versions và schedule refs trong detail để chuẩn bị deltas;
selector API-138 bổ sung token của new selected Schedule. No GradeReport
version touch do GET hoặc new default Score để lấy token.

### 25.4. Matching Schedule options — API-138

GET /academics/teachers/me/classes/{cId}/components/{componentId}/schedule-options:
no body/query; component thuộc full frozen Program của Class.
200 GradeScheduleOptionSet fields R classId/componentId BusinessId,
kind literal LESSON hoặc FINAL_EXAM, items array GradeScheduleOption,
context GradeConfigurationContext.
GradeScheduleOption fields R schedule GradeScheduleReference,
eligible boolean, blockedReasons array string.
Full current matching Schedule set đúng kind/Class, sort date/startTime/id ASC,
không historical NOT_HELD/other Class/global resource list hoặc page cắt tập.
No matching →items=[]200; future occurrences có thể listed eligible=false.
Eligibility ở đây chỉ Class/current Schedule/clock/Teacher, **không** permission
report của Student cụ thể vì path không có aId; API-139 còn kiểm target report.

GENERAL không có Schedule: UI không gọi selector, write scheduleId=null.
Đề xuất valid GENERAL component request400 field componentId (not applicable);
unknown/foreign component hoặc Class not assigned404 cùng response; syntax400/
auth chung. Đây là wire outcome draft, không business prohibition mới đối với
GENERAL grading. Không invent UUID preview/reservation hoặc Class Schedule
creation rights cho Teacher để có option. Dời lịch khi không còn valid token/
clock/candidate →reload chọn lại có chủ đích, không auto remap current successor.

### 25.5. Changed scores + optional comment all-or-none — API-139

PUT /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft:
GradeDraftDeltaRequest fields R scores array GradeScoreChange (có thể[]),
expectedVersions/expectedSet GradeDraftSaveManifest; teacherComment O
string≤1000 hoặc null. Có ít nhất một scores item **hoặc** property comment
provided; cả hai không có intent400 (draft wire). No total/finalScore/weight/
state/reason/actor/submitted/review metadata input; UUID R header, CSRF/Origin.

GradeScoreChange fields R componentId BusinessId, score GradeScoreInput,
scheduleId BusinessId hoặc null.
Mỗi component tối đa một delta, same target frozen set. ScheduleId R để nói
rõ GENERAL=null hoặc linked ID, không omit để giả default last Schedule.
Omitted component giữ Score cũ; new omitted component vẫn missing; score blank/
null400, không xóa Score hoặc đổi thành0. Actual score “0” là dữ liệu hợp lệ.
Explicit changed score có thể chọn current occurrence khác đúng kind/window;
không buộc cả cohort chọn cùng LESSON cho component khi E03 không đặt invariant.
Schedule selection không tự tạo Attendance/HELD, không migrate old scores
qua non-held replacement hoặc unlock old source data.

Comment omitted giữ nguyên; provided string trim/NFC/plain text/max1000 code
points, blank hoặc explicit null clear→NULL theo optional-text policy.
Comment-only save scores=[] được phép nếu parent window hợp lệ; không bắt
kèm mọi Score/Student/report. Một Score/token/error thất bại thì comment **cũng**
không được lưu; clear comment không xóa Score. RF04/ADR-159 đã Accepted: validate whole request trước equality; all-equal400/
no marker, mixed actual changes atomic, unchanged provenance giữ; stale item
aborts all. Parent report RV touch chỉ cho actual score/comment change.

GradeDraftSaveManifest = context mục25.3, thêm:
expectedVersions.scores R array VersionedReference của existing **changed**
Score rows; expectedVersions.schedules R array VersionedReference của full
relevant linked schedules (all saved target Score links + new selected IDs).
expectedSet.scoreBindings R array
{componentId: BusinessId, scoreId: BusinessId, scheduleId: BusinessId hoặc null}
nói **before-save** binding; expectedSet.unrecordedComponentIds R array BusinessId;
expectedSet.scheduleBindings R array ScheduleGuardReference trừ version, full
relevant Schedule set như trên.
Binding dùng id/classId/obligationKey/isCurrent/replacesScheduleId của GradeScheduleReference;
sessionType=reference.type (mục16), version chuyển vào expectedVersions.schedules.
Không gửi cả type/sessionType aliases hoặc thiếu predecessor binding.
All arrays R/[] when empty, unique/binding/version one-to-one. Changed component
IDs exactly existing binding IDs ∪ expected-absence IDs/disjoint. componentIds/
component versions luôn full frozen required set, không delta subset.
Target report/assignment/Program relationships checked server; no peer report RV.
Report RV bảo vệ full score membership/values/comment aggregate kể cả chỉ
Score đổi; service touch parent report version trong **cùng TX11** để concurrent
save/submit không dùng stale aggregate. Attendance save không tự touch report.

Full relevant Schedule sets gồm các links saved aggregate và new selections,
không full center/paged calendar hoặc imaginary token của absent Score.
Không cần all unselected future schedules/Student reports để lưu target.
Comment-only vẫn dùng full saved-linked Schedule guards và empty changed Score/
absence arrays, không yêu cầu mọi component có Score. Dữ liệu child insert/
absence được service requery dưới CLASS/report locks; không chỉ parent token
hoặc client false flags. Exact read snapshot/EF parent-touch/lock ordering
SC-03 còn mở, không invented EC:REPORT gate mới thay E03 protocol.

Validate all deltas/comment/manifest trước DML, recheck current assignment/
Class ONGOING or COMPLETED/report DRAFT or RETURNED, frozen config/component
kind/schedule/currentness/start/live absence and all saved aggregate links.
Malformed/token/duplicate/precision/comment/association400; scoped target404/
auth chung; live stale set/version/absence/state/clock/dependency conflict409.
Một lỗi rollback toàn per-Student save, không silent upsert overwrite hoặc
merge/cắt/round. Không chuyển report status/submitted/finalized metadata.

200 GradeDraftSaveResult fields R detail TeacherStudentGradeDetail,
operationId UUID. Detail current saved computation/report RV/component links
theo caller scope; finalScore vẫn null nếu chưa FINALIZED, no “đã công bố”.
TX11 changed scores/comment/provenance +parent report RV touch +mandatory
allowlisted score history/audit +T23 completion cùng connection/transaction.
Missing components còn thiếu; no automatic whole-Class send/return/resubmit/
finalize/Class complete/attendance defaults/financial effects.
TG05 là backstop config/Class/kind/report lock, không thay Teacher/clock/TX
guards. FN05/ufn_GradeTotal dự kiến gọi scoped read/total/completeness đúng E03;
không tạo SP grading mới hoặc source signatures/EF mapping đã implement.

## 26. Replay, UI và ví dụ — Attendance/draft

API-090 giữ Teacher **current assigned Class** action pairs133/139: safe target
classId+scheduleId hoặc classId+classAssignmentId, không union AcademicAffairs
command rights. Same UUID/content completed check current caller/task trước
stale checks của mutation mới, dựng current safe refs/projection; không ghi
lần hai/audit thứ hai hoặc response snapshot/old draft payload để giữ quyền.
Action strings/canonical digest/status-completion bytes/outcome registry SC-02.
Gradebook/Attendance GET không mutation marker; sensitive data no-store/RAM,
không URL/name searches hoặc localStorage/sessionStorage/offline sync.

Unknown response/500/timeout không proof rollback: lookup đúng role/action và
refetch full sheet/target detail để đối chiếu intent; missing marker có thể
in-flight/expired, không khẳng định failed. User retry chủ đích cùng UUID/
normalized intent/versions-set, no auto retry/new UUID/overwrite. Assignment
loss/account block/session revoke →clear stale task/RAM, không trả cached draft
qua calendar/Academic routes/result marker. Retention≥24h không dedup forever.

FE14 giữ desktop per-row “Lưu học viên”, mobile same one-Student page; Attendance
“Lưu điểm danh buổi”. Show Chưa ghi/Có mặt/Vắng và Chưa đủ điểm/Chưa lưu/Đã lưu,
không browser-computed total làm official. Unsaved edits warning, explicit save,
no autosave/Excel import. Student không xem peers/draft; Giáo vụ Attendance
read-only, không nút nhập thay. Linked selection chưa mở báo lý do; HELD không
là prerequisite mới. Local edits phải được xử lý trước whole-Class submit sau
này, server không kiểm được edit RAM ở browser/tab/device khác.

### 26.1. JSON — mixed Attendance delta

Fake Class41/LESSON71 có full cohort61,62,63. Chỉ61(existing) và62(unrecorded)
được đánh dấu;63 vẫn chưa ghi. UUID nằm ở header, không JSON; token8 bytes
minh họa, không runtime/reservation proof.

```json
{
  "changes": [
    { "classAssignmentId": 61, "present": 1 },
    { "classAssignmentId": 62, "present": 0 }
  ],
  "expectedVersions": {
    "class": "AAAAAAAAAAE=",
    "teacher": "AAAAAAAAAAI=",
    "schedule": "AAAAAAAAAAM=",
    "attendance": [ { "id": 81, "version": "AAAAAAAAAAQ=" } ]
  },
  "expectedSet": {
    "assignmentIds": [61, 62, 63],
    "attendanceBindings": [ { "classAssignmentId": 61, "attendanceId": 81 } ],
    "unrecordedAssignmentIds": [62]
  }
}
```

### 26.2. JSON — grade score creation + comment

Fake target assignment61/report91, frozen Program components31 GENERAL40% và32
FINAL_EXAM60%, full saved scores trước lệnh rỗng. Nhập0 ở GENERAL31/nhận xét;
component32 còn missing nên total=null, state vẫn DRAFT/RETURNED.
Không schedule/attendance tự tạo hoặc “thiếu→0”, không submit/finalize.

```json
{
  "scores": [
    { "componentId": 31, "score": "0", "scheduleId": null }
  ],
  "teacherComment": "Cần luyện tập thêm.",
  "expectedVersions": {
    "class": "AAAAAAAAAAE=",
    "program": "AAAAAAAAAAI=",
    "teacher": "AAAAAAAAAAM=",
    "report": "AAAAAAAAAAQ=",
    "components": [
      { "id": 31, "version": "AAAAAAAAAAU=" },
      { "id": 32, "version": "AAAAAAAAAAY=" }
    ],
    "scores": [],
    "schedules": []
  },
  "expectedSet": {
    "classAssignmentId": 61,
    "reportId": 91,
    "componentIds": [31, 32],
    "scoreBindings": [],
    "unrecordedComponentIds": [31],
    "scheduleBindings": []
  }
}
```

Các ví dụ chỉ JSON/schema consistency checks, không HTTP request/SQL/tests đã
chạy hoặc owner nghiệm thu wire spelling/manifest. Targets sau triển khai:
all7 role/list-sheet-gradebook-detail-selector-save-replay scopes;
Attendance before/exact start/end/HELD/COMPLETED boundaries/PLANNED-OPEN no new
ONGOING gate; missing≠absent/present/full roster vs changed subset;
one malformed/stale row/absence insert race rollback all/provenance;
AA no draft/Teacher mutation tokens; GENERAL null vs wrong Class/type/current/
future NOT_HELD Schedule; missing≠0/blank≠delete/precision/comment clear;
score+comment rollback/report RV touch/score-only vs comment-only/submit race;
DRAFT-RETURNED edits/COMPLETED grades vs read-only Attendance;
FN05 complete/incomplete/49.995–49.994 totals/one-round;
Teacher reassignment/block/revoke cuts detail/options/lookup/current replay;
unknown same-key/different intent/expiry/no auto effects; mandatory audit/T23
failure rollback. Trigger/EF/grants/isolation/signatures/per-command audits
vẫn SC-03; no executed integration/browser/load/recovery evidence.

## 27. Teacher gửi/gửi lại cả Class — API-140–142 / W18 / F27

Nguồn NV25–26/NV38, ADR-024/028/034/125/135/150 và E03 T14–19/TX12/FN05.
Teacher NORMAL **current assigned** Class; không AcademicAffairs/Student/
Admin/Director transition qua Teacher path. Class COMPLETED, đủ mọi Student/
component và report DRAFT/RETURNED mới gửi; no per-Student selection.
Mọi path dưới /api/v1; id ở inventory là Class BusinessId, không report ID.
POST preview/commands CSRF/Origin/no-store, preview không UUID/mutation/T23,
commands UUID header. Unknown properties/shape400 theo draft mục1.

### 27.1. Whole-cohort wire types và full guard manifest

R mọi property dưới đây, nullable chỉ nơi ghi rõ; kinds/decimal/reference
reuse mục14/16/25. Đây là refinements draft để rà actual E04, không SQL/
canonical action strings hoặc thêm Class/GradeReport state.

| Type | Required fields / kiểu | Semantics |
| --- | --- | --- |
| GradeSubmissionAction | SUBMIT/RESUBMIT. | Preview/UI intent, không T23 persisted actioncode. |
| GradeCohortStates | draft/submitted/returned/finalized integer0–15 mỗi field. | Teacher-only đầy đủ own-task metadata, counts sum cohort size; không đem DRAFT count/value projection này cho Student/reviewer. |
| GradeSubmissionStudent | assignment GradeAssignmentReference; report TeacherGradeReportView; computation GradeComputation. | Full assigned cohort/id ASC, saved-only completeness/total/latest return reason; không unsaved browser values hoặc peer Classes. |
| GradeWorkflowIssue | classAssignmentId/componentId BusinessId hoặc null; message string. | Teacher issue đúng task; null khi Class/config/cohort-wide, không raw SQL/full payload. Issue names chưa canonical error codes. |
| GradeSubmissionPreview | class TeacherClassSummary; action GradeSubmissionAction; cohortSize integer0–15; states GradeCohortStates; items array GradeSubmissionStudent; eligible boolean; blockedReasons array string; issues array GradeWorkflowIssue; effects array string; manifest ClassGradeWorkflowManifest hoặc null. | Full preview không page/filter/selected rows; allowed command giữ expected context, không previewId/reservation. |
| GradeWorkflowCommandResult | classId BusinessId; reportIds array BusinessId; reportCount integer1–15; status SUBMITTED/RETURNED/FINALIZED; operationId UUID. | Safe IDs/current outcome của cả operation, không stored Score/comment/Student response snapshot. |

ClassGradeWorkflowManifest fields R:
expectedVersions {class/program/teacher: RowVersion,
components/reports/scores/schedules: array VersionedReference};
expectedSet {assignmentIds: array BusinessId, componentIds: array BusinessId,
reportBindings: array GradeWorkflowReportBinding,
scoreBindings: array GradeWorkflowScoreBinding,
scheduleBindings: array ScheduleGuardReference trừ version}.
GradeWorkflowReportBinding fields R classAssignmentId/reportId BusinessId,
status GradeReportStatus (expected **before** transition).
GradeWorkflowScoreBinding fields R classAssignmentId/componentId/scoreId
BusinessId, scheduleId BusinessId hoặc null (stored before link).

assignmentIds full immutable Class T15 cohort, reportBindings đúng một report/
assignment và report IDs/RVs full one-to-one; không Assignment RV.
componentIds/versions full frozen Program config, scores/bindings full saved
cohort×configured scores, không chỉ changed/visible rows như per-Student save.
Relevant schedules full distinct links của mọi saved linked Score, same Class/
current/type/key và RV one-to-one; GENERAL=null. Mapping GradeScheduleReference
type→ScheduleGuardReference.sessionType như mục25.5; no duplicate aliases.
All arrays R/[] khi appropriate; eligible phải đủ saved scores/mọi report.
Server kiểm actual memberships/relationships/absence/invariants, không chỉ IDs
client gửi; missing parent report không tự create tại preview/commit.
Report RV tăng khi Score/comment save (mục25), nhưng full child bindings/live
query vẫn cần; parent/Class RV không chứng minh full cohort/set hay clock.
Chỉ giữ version của rows hiện có, không fake tokens cho data thiếu.
Program INACTIVE không auto dừng existing grade workflow; Teacher token/context
không tự thêm guard mới ngoài current auth/task/accepted workflow.

### 27.2. Preview cả lớp — API-140

POST /academics/teachers/me/classes/{id}/grade-submission-preview:
body action R SUBMIT/RESUBMIT, không student/report selection/total/versions/
reason/hasUnsavedEdits boolean để bypass. Frontend phải xử lý mọi local
unsaved edit trước gọi preview theo FE15; server chỉ đọc saved DB state,
không chứng minh browser/tab/device khác không có edit RAM.

200 GradeSubmissionPreview kể cả valid target bị blocked vì Class/report/
missing components; manifest=null khi không eligible. Issues có all missing
component IDs theo đúng assignment, không thiếu→0 hoặc skip Student để có
subset “đủ điểm”. Full max15 students, configured components không truncate100.
Empty cohort không thành eligible empty command; required GradeReport thiếu
là integrity error, không nullable/synthetic GradeSubmissionStudent để fake
preview missing-score. Điều kiện Class đã COMPLETED vẫn đòi workflow đã chốt,
không GET/preview repair.
Malformed action/ID400, missing/not current assigned404, auth401/403.
Scope kiểm trước projection mọi report/issues, history Teacher không giữ quyền.

Gửi/gửi lại kế thừa accepted source states DRAFT/RETURNED cho **mọi** report,
không còn SUBMITTED/FINALIZED trong source cohort. RESUBMIT thể hiện luồng
sau RETURNED của NV26; không thêm Teacher reason, reset scores/comment hoặc
giới hạn chỉ gửi lại một lần. Wire action/presentation không bổ sung requirement
mọi source report đều DRAFT mới được SUBMIT, hoặc dùng partial source subset.
Preview→confirmation Class/count/states/effects, input/action/context đổi thì
invalidate, server recheck live; không giữ DB lock qua lúc user xem/xác nhận.

### 27.3. Commands tách biệt — API-141–142

API-141 POST /academics/teachers/me/classes/{id}/submit-grades;
API-142 POST .../{id}/resubmit-grades.
Body fields R expectedVersions/expectedSet ClassGradeWorkflowManifest,
UUID R header. Action xác định bởi path, không nhận client report target
status/actor/time/score/comment/total/reason mới. Unknown extra field400.
Route intent preview và command phải thống nhất, không đổi action silently.

Recheck current Teacher/session/Class COMPLETED, full cohort/required config/
all reports DRAFT or RETURNED/all required saved Score links/decimal completeness.
Từng valid Student không cho phép bỏ Student invalid; unexpected mixed
SUBMITTED/FINALIZED source409, không “gửi nốt phần còn lại”.
Exact manifest/version/association syntax400; missing scope404/auth chung;
live state/set/RV/data completeness/dependency conflict409 rollback cả lớp.

200 GradeWorkflowCommandResult status SUBMITTED, reportIds full/id ASC,
reportCount=cohort size. TX12 mọi report→SUBMITTED/submitted_by/at/shared
submitted_operation_id, parent versions và mandatory per-report workflow
history/group operation/T23 completion cùng transaction.
Retain Score/comment/latest applicable return metadata theo T19; không clear
reason/audit hoặc overwrite Score từ client payload. final_score/finalized_at
vẫn null; no Student publication/Class complete/Attendance changes.
Một report/audit/marker failure không chuyển report nào. Current completed
same-key replay không submit lần hai; fresh command vào SUBMITTED409, không
invent state no-op success hoặc auto RETURN để mở sửa.

## 28. AcademicAffairs review cả Class — API-143–147 / W19 / F23

Nguồn NV20/W19/ADR-034/083/135/150 và T09/T14–19/TX12/SP03/FN03/FN05.
AcademicAffairs NORMAL; Teacher không review/finalize, Director/Admin không
approval tier/override. Read SUBMITTED/RETURNED/FINALIZED; **không DRAFT
scores/comment/totals/provenance/guard snapshots**, không set cấm thành null
trong shared Teacher row rồi gửi. No-store, reads không UUID/DML/T23.

### 28.1. Review-specific projections và paginated list — API-143

| Type | Required fields / kiểu | Privacy / source |
| --- | --- | --- |
| ReviewVisibleStates | submitted/returned/finalized integer0–15. | Không Teacher GradeCohortStates/DRAFT count hoặc DRAFT value fields. |
| GradeReviewClassSummary | class AcademicClassSummary; cohortSize/visibleReportCount integer0–15; states ReviewVisibleStates; hasCompleteVisibleCohort boolean; latestSubmittedAt/latestReviewedAt Instant hoặc null. | Counts full same-Class scoped sources; không Grade DRAFT payload/Student list hoặc finance. |
| AcademicReviewReportView | TeacherGradeReportView với status chỉ SUBMITTED/RETURNED/FINALIZED. | Những states được phép đọc, không Teacher writeContext/edit permissions; latest timestamps không raw AuditLog. |
| AcademicReviewScoreView | TeacherScoreView chỉ từ report được phép đọc. | Same-Class/configured component/link/provenance/RVs cho review guard; không DRAFT Score kể cả ID/value/token. |
| AcademicGradeReviewStudent | assignment GradeAssignmentReference; report AcademicReviewReportView; scores array AcademicReviewScoreView; computation GradeComputation. | ID-name tối thiểu đúng Class, không Student contact/DOB/address/Invoice/global profile. |
| GradeReviewAction | RETURN/FINALIZE. | UI/preview intent, không persisted canonical actioncode. |
| GradeReviewDetail | class AcademicClassSummary; components array ComponentView; cohortSize/visibleReportCount integer0–15; hasCompleteVisibleCohort boolean; items array AcademicGradeReviewStudent; allowedActions array GradeReviewAction; disabledReasons object action→string. | Rows non-DRAFT only/full visible set; counts/rows không tự giả proof đủ cohort. Manifest chỉ từ eligible scoped preview API-145. |

API-143 GET /academics/grade-reviews query reviewStatus O
SUBMITTED/RETURNED/FINALIZED (draft default SUBMITTED), courseId/classId/
teacherId O BusinessId; sort O CLASS_CODE_ASC/CLASS_CODE_DESC default
CLASS_CODE_ASC, page/pageSize mục1.2. Safe non-identifying filters, không
name/email/phone URL/search hoặc arbitrary EF sort/status DRAFT.
Draft list contains Class COMPLETED có **ít nhất một** visible report khớp
filter; states/visibleReportCount vẫn full allowed same-Class set độc lập
status-filter/page. Không giả counts của matching rows là toàn cohort.
These default/bucket/query spellings là wire proposals, không business state mới.

200 page GradeReviewClassSummary, stable Class code/classId cùng chiều;
empty200, malformed enum/ID/query400, auth chung. Current programme/Teacher
refs giữ history kể cả INACTIVE, không filter mất old finalized results.
Lớp chỉ DRAFT không được list thành GradeReview row; đọc Class/roster qua
route có quyền riêng, không đọc Teacher endpoint để lấy missing grade values.

### 28.2. Full visible review detail — API-144

GET /academics/grade-reviews/classes/{id}: no business body/query,
200 GradeReviewDetail. Class/read scope và ít nhất một non-DRAFT report,
components full frozen set, visible items classAssignmentId ASC/max15/
không paginate/filter Students. Same visible states rule independent list
filter; RETURNED corrected values vẫn thuộc scope RETURNED đã accepted.
Missing/Class ngoài review scope/không report được phép đọc404 cùng response,
syntax400/auth chung; không DRAFT placeholder row/scores/comment tokens.
Integrity mismatch không GET tạo GradeReports/default scores/self-repair.

cohortSize từ full immutable Assignment cohort, visibleReportCount từ allowed
reports; hasCompleteVisibleCohort=(visibleReportCount=cohortSize).
Nếu thiếu visible member, không dựng whole-cohort proof/manifest, allowedActions
empty/disabled reasons đúng scope. Không expose DRAFT per-Student missing-score
issues cho reviewer để giải thích. Counts/visibility không thay all SUBMITTED/
Class COMPLETED/component-valid rule; đầy visible RETURNED/FINALIZED chỉ đọc,
không review lại khi chưa được gửi SUBMITTED hoặc reopen FINALIZED.

SP03/usp_GetClassGradeReview là path đã chốt cho values/computation; trusted
actor_id từ auth service, không request override. Minimal Class/cohort metadata
và allowed-values query phải coherent/read-authorized theo SC-03, không raw
SELECT DRAFT để frontend tự lọc hoặc miễn service auth vì SQL role cho EXEC.

### 28.3. Preview RETURN/FINALIZE và finalize guard — API-145

POST /academics/grade-reviews/classes/{id}/review-preview:
body action R RETURN/FINALIZE only, CSRF/no-store/no UUID/T23.
200 GradeReviewPreview fields R detail GradeReviewDetail, action
GradeReviewAction, eligible boolean, blockedReasons array string, effects
array string, manifest GradeReturnManifest hoặc GradeFinalizeManifest hoặc null.
Manifest type theo returned action, không client target state/set nào cũng
dùng được. Valid scoped state blocked200/null; missing outside same review
scope404, action/input400/auth chung. No preview reservation/user-time TX.

Both review commands: Class COMPLETED, full nonempty cohort đúng một report/
assignment, mọi report SUBMITTED và all required components/current links/
valid saved aggregate. Không review một phần hoặc trả/chốt RETURNED/FINALIZED;
không sửa Score/comment thay Teacher. Preview FINALIZE totals từ FN05 và
phân biệt tính toán hiện tại với immutable final_score chưa lưu.
Không extra passing prerequisite/minimum component/chuyên cần.

GradeReturnManifest = ClassGradeWorkflowManifest mục27.1 với all expected
reportBindings.status SUBMITTED, chỉ allowed visible cohort; không DRAFT IDs/
tokens/bindings trong response. GradeFinalizeManifest = GradeReturnManifest,
thêm expectedVersions.registrations R array VersionedReference;
expectedSet.registrationBindings R array
{classAssignmentId: BusinessId, registrationId: BusinessId}.
Full one-to-one Registration/Assignment cohort đúng relationships/versions,
không PII/profile/financial fields/current is_unfinished input để client ép.
Finalization actual related Registration predicate requery trong TX; parent
Class/report tokens không tự bảo vệ mutable Registration projection.
RETURN không đổi unfinished, không tự đòi Registration versions của finalize
cho command return; all arrays still exact/full appropriate operation.

Return preview chỉ action/context/effects, không fake default reason.
UI reason bắt buộc trong confirmation và API-146; user không có reason hợp lệ
không gửi command. Preview không stage/commit “trả” trước user nhập lý do.
FINALIZE preview không nhận final totals/pass/status mà client tính; server
recompute trước commit, valid RV không thay formula/config/current bindings.

### 28.4. Return whole cohort with reason — API-146

POST /academics/grade-reviews/classes/{id}/return:
reason R plain-text Unicode/NFC/trim1–1000 code points, expectedVersions/
expectedSet R GradeReturnManifest, UUID R header. Không scores/comment/actor/
selected Student/target status hoặc nullable/blank reason.
Input/token/shape400; missing scoped404/auth chung; live state/set/version/
cohort/data conflict409, rollback toàn cohort không chuyển phần valid.

200 GradeWorkflowCommandResult status RETURNED, report IDs full/id ASC.
TX12 all reports→RETURNED, same reviewed_by/at/review_operation_id and
mandatory reason +per-report grouped workflow history/audit/T23 atomic.
Keep Scores/comment/final_score=null/finalized_at=null; Teacher có thể chỉnh
per-Student theo window và resubmit cả Class, không auto correction/notification/
resubmit hoặc một reason event đè mất mọi history cũ.
Repeat fresh RETURN vào RETURNED409, no no-op/audit repeat; completed same-key
lookup/replay kiểm current caller rights trước stale mutation checks.

### 28.5. Finalize + publication + unfinished — API-147

POST /academics/grade-reviews/classes/{id}/finalize:
expectedVersions/expectedSet R GradeFinalizeManifest, UUID R header.
Không client finalScore/pass/Registration state/isUnfinished/reason/override.
Same live Class/all SUBMITTED/full component/schedule/score/config/report
guards và full Registration relationships/predicate;400 malformed/association,
404 scope/auth chung,409 stale/state/set/score/config/projection race.

200 GradeWorkflowCommandResult status FINALIZED, complete reportIds/count.
TX12 service re-evaluates FN05 saved totals bằng decimal intermediate, half-up
2 decimals **một lần**, sets every final_score/finalized_at/reviewed_by/at/
shared review_operation_id/report versions; grouped mandatory workflow audit+
ReplayOperation completion và Registration.is_unfinished theo FN03 **cùng TX**.
FN05 total đã rounded thì service dùng đúng kết quả đó, không round từng
contribution/áp một công thức khác hoặc round stage thứ hai.
Passing derived FINALIZED final_score≥50, không stored pass flag.
Class COMPLETED + FINALIZED chính lượt học làm hết unfinished dù final_score
dưới50; không Registration COMPLETED enum/paid-refund/copy score/new registration.
Rowversion updates thực theo mutations, không client tự unset projection.

All reports published atomically at commit, Student never sees partial cohort
từ operation này; authorized read snapshot/coherence vẫn SC-03 cần kiểm.
Score/comment/report FINALIZED immutable, no reopen/override/partial finalizing/
late-placement/attendance correction/Class reset. Error bất kỳ report/
predicate/audit/marker làm rollback cả final scores/states/projections/history.
Không chốt stored draft total, dùng float/round per contribution hoặc publish
trước finish audit. Fresh FINALIZE vào FINALIZED409, no fabricated re-finalize
event; completed replay đúng current caller quyền không DML/audit lần hai.

## 29. Student own FINALIZED — API-148–149 / NV08 / F12

Student NORMAL, ownership Assignment→Registration→verified Student và status
FINALIZED/Class COMPLETED. Không Teacher/Academic role “proxy Student” hoặc
query studentId/accountId. Read-only/no UUID/T23/mutation/no-store. Grade DRAFT/
SUBMITTED/RETURNED **không** row/null placeholders/aggregate/preview ở own API.
Chưa có kết quả là chưa công bố, không GradeScore0/false pass.

| Type | Required fields / kiểu | Giới hạn |
| --- | --- | --- |
| StudentFinalizedGradeSummary | id BusinessId (report); class StudentOwnClassSummary mục18.3; finalizedAt Instant; finalScore GradeScore; passed boolean. | Chọn đúng own Class/lượt Assignment, no peers/cohortCount/Teacher account status or tokens; passing từ immutable finalScore≥50. |
| StudentGradeScheduleReference | id BusinessId; type LESSON/FINAL_EXAM; date LocalDate; startTime/endTime MinuteTime; room string mã chuẩn. | Own graded occurrence context, không RV/provenance/management reason/Student conflicts/teacherAccountId. |
| StudentFinalizedComponent | componentId BusinessId; name string1–100; kind GENERAL/LESSON/FINAL_EXAM; weight ComponentWeight; score GradeScore; schedule StudentGradeScheduleReference hoặc null. | Full frozen components/scores, GENERAL=null; no Score id/RV/entered-by/history chưa cấp quyền. |
| StudentFinalizedGradeDetail | StudentFinalizedGradeSummary +components array StudentFinalizedComponent; teacherComment string≤1000 hoặc null. | Published comment; không submitted/reviewed actors, internal return reason/audit/current draft total/write manifests/actions. |

### 29.1. Own list/filter — API-148

GET /students/me/grade-reports query courseId/classId O BusinessId,
fromDate/toDate O LocalDate inclusive trên ngày VN của finalizedAt,
sort O FINALIZED_ASC/FINALIZED_DESC default FINALIZED_DESC,
page/pageSize mục1.2. Omitted dates không bound/current-month cutoff ngầm;
from>to400. Filter/default spelling là wire proposals; không status DRAFT/
Student identifiers/grade reviewer ID/search name hoặc unofficial-total filter.

200 page StudentFinalizedGradeSummary, stable finalizedAt/reportId cùng chiều,
empty200 với thông báo “Chưa có kết quả được công bố”/no-match đúng filter,
không đưa số unpublished reports/grades/missing components của người khác.
Explicit classId không own/missing404 cùng response; valid course filter
không matching own result trả empty200, không khai Student identities.
Malformed query400/auth401/403. No export/PDF/email/grade-unlock/history snapshots;
retakes/multiple Course registrations chỉ chọn đúng own Assignment/Report.
Số report rows không Student pass-rate/cohort aggregate hoặc quyền peer access.

### 29.2. Own finalized detail — API-149

GET /students/me/grade-reports/{id}: BusinessId report/no business body/query.
200 StudentFinalizedGradeDetail, full components/id ASC/one saved score per
configured component. FINALIZED finalScore từ T19 immutable publication, không
tính lại bằng config/score client rồi overwrite kết quả đã chốt.
Owned historical Class/program kể cả INACTIVE vẫn read đúng rights.
Null comment là chưa có comment, không missing score. Passing so sánh decimal
finalScore đã rounded,49.995→50.00 đạt và49.994→49.99 không đạt theo nguồn;
đó là totals intermediate minh họa, không input scores >2 decimals.

Missing/not own/not FINALIZED404 **cùng** response, không tiết lộ report tồn tại/
chưa chốt hoặc returned reason. Syntax400/auth chung, no-store/clear RAM khi
mất session/ownership. No peer Assignment IDs/contact/account tokens/entered
Score provenance/audit linked events/Academic grade review context.
Không lấy Class COMPLETED làm proof published hoặc công bố qua home/calendar
một total nháp. Finalized integrity problem không GET create/repair/default0.

## 30. Whole-Class transaction, replay, ví dụ và kiểm chứng

API-090 giữ4 role/action pairs: Teacher current assigned Class submit/resubmit
141/142 và AcademicAffairs return/finalize146/147, safe target chỉ classId.
Không union Teacher/reviewer quyền hoặc expose stored scores/comment snapshot/
unpublished totals từ result marker. Canonical action strings/bytes/outcome
compatibility/retention wire còn SC-02, ID API không persisted actioncode.
Marker/current read rights checked before treating completed same-key as new
mutation; valid completed replay refs/current projection, no repeated effects/
audit. Missing marker/timeout/500 không proof rollback hoặc dedup forever.

One Service/DbContext connection/TX12 cho every report/history/audit/T23 và
Registration projection khi finalize. Request-key lock trước sorted business
gates (EC:CLASS và related EC:REG khi predicate mutated, exact set/order SC-03),
reload coherent full parents/children, Account/session guard tới commit.
Serialize per-Student Score/comment save/report RV touch vs submit; submit vs
return/finalize/new-intent duplicates; unfinished clear vs new enrollment;
current Teacher/task/Account block/revoke. No independent SP commit/connect
khi đọc predicate/total hoặc giữ transaction qua preview/confirmation.
TG05 Score locks là backstop, không thay all-Class rights/state/full-set protocol.
SP03/FN03/FN05 paths giữ E03 caller/purpose, exact signatures/results/EF/
grants/read-isolation/audit field allowlists/locks/timeouts còn SC-03.

FE15: xử lý local unsaved edits trước preview, không gửi “saved=false” flag
làm auth; một confirmation Class/code/count/state/effects +reason cho RETURN.
Button theo actor, điều kiện thiếu disabled/lý do đúng privacy, không checkbox
chọn Student để gửi/chốt. Preview/action/context đổi invalidate và re-preview;
409 reload/rà lại, no merge/overwrite/auto retry. Unknown tra caller completion/
scoped Class gradebook/review hoặc own finalized detail trước retry chủ đích
cùng UUID/normalized intent/versions-set; không UUID mới vì chưa nhận response.
Server không thể chứng minh browser khác không còn unsaved edits.
Mọi grade/read role projections tách riêng, no durable browser draft/grade cache.

### 30.1. JSON — full-cohort finalize request

Fake Class41 COMPLETED, Teacher current, reports91/92 SUBMITTED, assignments61/
62, registrations51/52; full Program components31 GENERAL và32 FINAL_EXAM,
mỗi Student đủ hai saved scores. Current FINAL_EXAM81 cùng Class/HELD; không
root extra Schedule trong binding. Tokens8 bytes giả; UUID header không body.
Các ID/key/sets không cấp quyền, service vẫn kiểm sources/predicate/live states.

```json
{
  "expectedVersions": {
    "class": "AAAAAAAAAAE=",
    "program": "AAAAAAAAAAI=",
    "teacher": "AAAAAAAAAAM=",
    "components": [
      { "id": 31, "version": "AAAAAAAAAAQ=" },
      { "id": 32, "version": "AAAAAAAAAAU=" }
    ],
    "reports": [
      { "id": 91, "version": "AAAAAAAAAAY=" },
      { "id": 92, "version": "AAAAAAAAAAc=" }
    ],
    "scores": [
      { "id": 101, "version": "AAAAAAAAAAg=" },
      { "id": 102, "version": "AAAAAAAAAAk=" },
      { "id": 103, "version": "AAAAAAAAAAo=" },
      { "id": 104, "version": "AAAAAAAAAAs=" }
    ],
    "schedules": [ { "id": 81, "version": "AAAAAAAAAAw=" } ],
    "registrations": [
      { "id": 51, "version": "AAAAAAAAAA0=" },
      { "id": 52, "version": "AAAAAAAAAA4=" }
    ]
  },
  "expectedSet": {
    "assignmentIds": [61, 62],
    "componentIds": [31, 32],
    "reportBindings": [
      { "classAssignmentId": 61, "reportId": 91, "status": "SUBMITTED" },
      { "classAssignmentId": 62, "reportId": 92, "status": "SUBMITTED" }
    ],
    "scoreBindings": [
      { "classAssignmentId": 61, "componentId": 31, "scoreId": 101, "scheduleId": null },
      { "classAssignmentId": 61, "componentId": 32, "scoreId": 102, "scheduleId": 81 },
      { "classAssignmentId": 62, "componentId": 31, "scoreId": 103, "scheduleId": null },
      { "classAssignmentId": 62, "componentId": 32, "scoreId": 104, "scheduleId": 81 }
    ],
    "scheduleBindings": [
      {
        "id": 81,
        "classId": 41,
        "obligationKey": "d10a34f1-7176-4b1b-b521-ea19f27a1c01",
        "sessionType": "FINAL_EXAM",
        "isCurrent": true,
        "replacesScheduleId": null
      }
    ],
    "registrationBindings": [
      { "classAssignmentId": 61, "registrationId": 51 },
      { "classAssignmentId": 62, "registrationId": 52 }
    ]
  }
}
```

### 30.2. JSON — all-cohort safe result

200 GradeWorkflowCommandResult dưới đây chỉ safe refs/status cho authorized
caller; không Score/comment request-response snapshot trong replay store.

```json
{
  "classId": 41,
  "reportIds": [91, 92],
  "reportCount": 2,
  "status": "FINALIZED",
  "operationId": "fc12994b-d625-488a-91a6-f21e6f548a9d"
}
```

Targets **sau triển khai**:7 roles/Teacher current assignment/list/detail/
preview-command-lookup/replay/Student own strict finalized access;
full cohort vs page/subset/absent report/changed component-score-report links;
missing≠0/SUBMITTED not published/RETURNED editable/current values scope;
decimal round once49.995/49.994/passing50/final_score stored only FINALIZED;
one report/audit/T23/predicate failure rollback all; no partial publication/
Registration projection clear; finalized failed course can reenroll/no passing
gate/unique unfinished race; score-only/report touch vs submit/return/finalize;
fresh duplicate409/completed replay current rights vs stale context;
loss Teacher assignment/block/revoke/CSRF/unknown marker expiry;
no Student peers/managerial reason/DRAFT null placeholders;
no AA DRAFT Score/comment/guard leakage/visible subset mistaken full Class;
unsaved edits/confirmation/action input change/mobile grade UI separation.
JSON/static structure checks không HTTP/SQL/xUnit/browser/race/load tests
đã chạy; SQL caller/grants/lock/read snapshot/error/audit details vẫn SC-02/03.

## 31. Reports — conventions dùng chung / W20 / F30–F31

Nguồn: E01 NV29–32, E02 W20, E03 VW01/VW03–VW06/FN04/FN06/SP04–06,
ADR-063–072/091/094/101/126/151. Mục32–35 bổ sung đúng5 API-150–154.
Tên fields/filters/options/page/header/CSV columns/error subcode dưới đây là
**đề xuất wire để rà actual E04**, không mới Accepted hoặc metric/quyền mới.

Mọi API dưới /api/v1, GET/read-only/NORMAL, no-store; không business body,
Idempotency-Key/T23 marker/expectedVersions/preview reservation/queryId/asOf
input hoặc export job. Accountant chỉ150/154, Director chỉ151–153; năm role
khác không kế thừa quyền vì tên miền/SQL principal (tổng cộng7 roles).
Không report routes cho Admin/Admissions/AcademicAffairs/Teacher/Student.
Runtime principal không thay authenticated actor/app authorization.

### 31.1. Scalar, ngày và scope

| Type / field | Required / kiểu / semantics |
| --- | --- |
| ReportAmount | string nguyên đồng không âm từ decimal(38,0): 0–99999999999999999999999999999999999999; canonical "0" hoặc digits không leading zero/sign/separators/exponent/suffix. Không Money dương decimal18, không JSON number/float. |
| ReportCount | JSON integer không âm, trong0–2147483647; cast/check overflow khi SQL aggregate, không wrap/truncate hoặc đổi kiểu lúc vượt. Không client sum/page-derived count. Count transport limit là draft, không tăng giới hạn nghiệp vụ. |
| ReportPercent | string decimal fixed2 trong0.00–100.00 hoặc null khi mẫu số không áp dụng/rỗng; half-up một lần (100×numerator/denominator), exact decimal, không round counts. |
| queriedAt | R Instant từ server cho snapshot query này; UI hiển thị queried_at theo giờ VN. Không request filter, historical-as-of guarantee hoặc persisted report. |
| todayVn | R LocalDate suy từ cùng server queriedAt, dùng dueDate<todayVn cho overdue; dueDate==todayVn không quá hạn. Không đổi ngày giữa các query trong cùng report. |
| ReportCourse | R id BusinessId, code/name string theo Course nguồn; không tuition, RowVersion, Student refs hoặc lifecycle commands. |
| ReportClass | R id BusinessId, code string; aggregate group/filter ref, không Assignment/Student/GradeReport IDs. |
| ReportTeacherOption | R id BusinessId (Account), displayName string; chỉ current-Class filter, không email/phone/status/security_version/Admin actions. |

Mọi fields liệt kê trong projections mục31–35 là R trừ khi O ở request;
nullable chỉ khi ghi rõ. Money/count/rate không được thiếu→0 hoặc null tùy ý.
SUM money cast đầu vào decimal38 **trước** SUM, cùng numeric rules E03;
không SUM join đã nhân Invoice/Payment/Assignment/Schedule/GradeReport.
Nếu dữ liệu vượt scalar range/integrity thì controlled500 +traceId, không
file/totals một phần hoặc giá trị làm tròn giả.

Các period query nhận LocalDate from/to theo ngày VN inclusive, service
chuyển [from 00:00 VN, ngày sau to 00:00 VN) sang UTC. Đề xuất: cả hai omitted
→đầu/cuối tháng VN tại queriedAt; gửi chỉ một bound400, from>to400. Không
invent max duration, all-time default, inventory/debt period hoặc timezone
client. Date thật/range conversion không biểu diễn được400, không tràn ngày
cuối SQL range thành500; exact SQL predicates/bound params còn SC-03.
Query repeated scalar/unknown key/blank value/"null"/malformed bool/enum/
decimal ID400, không lấy silently first/last. Omitted ID→all, không ID0.
Filters là non-PII identifiers/date/state; không text Student/Invoice search,
paymentMethod/confirmingAccountId/asOf/created-Class-date fields.

Selector metadata không tạo thêm endpoint155 hoặc cho Director gọi catalog/
Class/Teacher management endpoints. Đề xuất options trong response report:
- finance/admissions: courses R full array CourseReference mục9.1;
- training: courses như trên; bands R full array BandReference; classes R
  full array {id,code,courseId,bandId: BusinessId trừ code string}; teachers R
  full array ReportTeacherOption có Class hiện đang tham chiếu.
Choices thuộc một center/report read scope, giữ referenced historical/INACTIVE
Course và ended Class/currently assigned historical Teacher; không cắt100
vì report table paginated, không Student labels/tuition/contact/RVs.
Courses ACTIVE chưa có facts có thể hiện để chọn (kết quả rỗng hợp lệ);
options không phải eligible enrollment/assignment candidates.
Role kiểm trước lookup. Missing/outside report-scope referenced filter ID404
cùng safe message; các ID individually valid nhưng kết hợp không có cohort
→200 no data, không tự bỏ filter hoặc thay bằng all. Options và result đọc cùng
snapshot; lựa chọn stale sau đó không là quyền giữ snapshot/quyền ghi.

### 31.2. Response consistency, errors và UI

Mỗi request mới một transaction SNAPSHOT trên cùng connection/service clock;
totals/groups/count/page/options và hai phần Training đọc cùng snapshot.
SP04/05/06 không tự connect/commit để chia count/totals/pages. Không lấy một
RCSI statement làm proof consistent whole report. No shared "all fields"
response rồi frontend mới giấu Student/Invoice data.
Whole-set totals độc lập page; request page ngoài last vẫn items=[]200 và
totals đúng. Sort cố định như từng API dưới đây, ID tie; không arbitrary sort
SQL/groupBy/raw column injection. Trang khác là query mới, không snapshot
cross-request hoặc query retention promise.

Syntax/filters400, session expired/revoked401, role/restricted403, missing/
outside filter refs404; đọc lại không dùng stale version409. Unexpected500
envelope mục1.1, không partial report. CSV limit có riêng409 draft mục35.
Malformed request không được leak options/filter targets cho actor thiếu quyền.
No query writes/audit nghiệp vụ/T23; accepted security denied-access events
vẫn đúng D08/D09, không full query payload/CSV/file contents/identities vào log.
Hủy request/timeout không "0 rows"; read không cần mutation recovery marker.

FE16: tách filter draft/appliedFilters/result/queriedAt; Áp dụng query mới/reset
page1, commit toàn response đúng request mới nhất, không response cũ overwrite
newer intent hoặc ghép cards/rows khác query. Edit chưa Áp dụng giữ nhãn/time
của kết quả cũ; query lỗi giữ context cũ và lỗi rõ, không coi cũ là query mới.
CSV gửi appliedFilters của financial result (explicit resolved dates), không
filter draft/page hoặc browser-generated rows; fresh result có thể khác.
Cards/group tables/no chart/PDF/XLSX/library mới; tiền exact strings/vi-VN,
rates2 decimals; null "Chưa có dữ liệu"/"Không áp dụng" theo meaning, không
0% giả. Responses/download nhạy cảm chỉ RAM cho session, clear khi mất quyền.

## 32. Accountant/Director finance — API-150–151 / NV29,NV32

API-150 GET /reports/accountant/finance: Accountant NORMAL.
API-151 GET /reports/director/finance: Director NORMAL.
Hai response types FinanceAccountantReport / FinanceDirectorReport độc lập
allowlist; metric/group fields giống ở gói này, không union operational Invoice
lookup/Payment correction/person-detail/export quyền của Accountant.

| FinancialReportQuery | Required / kiểu / default |
| --- | --- |
| fromDate, toDate | O LocalDate mỗi field; cả pair hoặc cùng omitted, current VN month theo31.1. Một period áp revenue confirmation và billed issuance trên đúng hai event columns, không debt. |
| courseId | O BusinessId; omitted mọi Course trong scope, kể cả referenced INACTIVE. |
| overdueOnly | O boolean true/false, default false; chỉ lọc current-debt cohort, không revenue/billed cohort. |
| page, pageSize | O integer theo1.2; nhóm Course, không Invoice rows. Không query sort/groupBy khác. |

FinanceAppliedFilters fields R fromDate/toDate LocalDate, courseId BusinessId
hoặc null (all), overdueOnly boolean. Không nullable date/all-time debt date.

| Response / nested shape | Fields / semantics |
| --- | --- |
| FinanceAccountantReport / FinanceDirectorReport | R queriedAt Instant, todayVn LocalDate, appliedFilters FinanceAppliedFilters, options {courses: array CourseReference}, totals FinanceMetrics, groups Page<FinanceCourseGroup>. Không capabilities/Invoice link/export URL hoặc students trong Director payload. |
| FinanceMetrics | R revenueAmount, billedAmount, currentDebtAmount, overdueDebtAmount: ReportAmount mỗi field. Empty qualifying cohort→"0", không null. |
| FinanceCourseGroup | R course ReportCourse; R metrics FinanceMetrics. Một row/Course của union ba metric cohorts; không Band/Class financial allocation đoán từ Registration chưa placed. |
| groups Page | R items array FinanceCourseGroup, page/pageSize integer đã áp, totalCount ReportCount toàn matching Course groups. Sort course.code ASC/course.id ASC. Không zero rows synthetic cho mọi Course không có qualifying facts. |

Revenue: SUM Payment.amount theo T21.paid_at trong period, Course qua Invoice/
Registration link hợp lệ, không actual-receipt timestamp tự thêm.
Billed: SUM Invoice.amount có current state UNPAID hoặc PAID theo T20.issued_at
trong period; CANCELLED loại kể cả issued từng thuộc kỳ. Không gross trước hủy.
Current debt: SUM current UNPAID Invoice.amount của Course, **không** from/to;
overdueOnly=true giới hạn debt cohort dueDate<todayVn.
Overdue debt: subset current debt cohort có dueDate<todayVn; false flag trả cả
current debt +overdue subset, true flag hai debt amounts bằng nhau.
Label UI "Công nợ hiện tại — chỉ quá hạn" khi true, không gọi tổng all debt.
Không nợ suy từ billed−revenue, không expense/profit hoặc delinquent Student state.

Groups lấy union Course có revenue/billed/debt facts sau từng predicate;
không lấy Invoice chỉ lọc issued period làm common base khiến receipt/debt
ngoài kỳ biến mất. Totals tính whole qualifying set, sum Course amount groups
được nhưng không sum visible page. Director không Invoice/Payment/Registration/
Student/Assignment/Score/GradeReport identifiers, names/contact/single-Student
drill row—even if một group chỉ có1 kết quả; group totals vẫn authorized.
Accountant report cũng không operational Invoice/profile dump; xem Invoice
phải API-048/049 quyền riêng. No Student invoice print permission cho staff.
200 full response; common400/401/403/404/500 như31, no UUID/version/replay.

## 33. Director admissions — API-152 / NV30

GET /reports/director/admissions, Director NORMAL; không Admissions staff.
Query fields O fromDate/toDate LocalDate (pair/default tháng VN hiện tại),
courseId BusinessId (omitted all), status RegistrationStatus mục9.1 (omitted
all7, gồm CANCELLED/REJECTED), page/pageSize theo1.2.
Cohort là Registration.created_at trong period +Course +**current** status;
không approval/booking/payment date hoặc reconstructed past state.

| AdmissionsReport shape | Required / kiểu / semantics |
| --- | --- |
| queriedAt, todayVn | R Instant/LocalDate theo31; todayVn dùng context default, không deadline/state mutation. |
| appliedFilters | R {fromDate/toDate LocalDate, courseId BusinessId hoặc null, status RegistrationStatus hoặc null}; null status=all, không null individual state. |
| options | R {courses: full array CourseReference}; no Student/Registration options. |
| totals | R AdmissionsMetrics: registrationCount/distinctStudentCount ReportCount mỗi field toàn cohort. |
| groups | R Page<AdmissionsCourseGroup>, fields Page theo32; sort Course.code ASC/id ASC. |
| AdmissionsCourseGroup | R course ReportCourse, metrics AdmissionsMetrics tính **cùng** filtered Course cohort. Không record refs/person data/state-history payload. |

Group chỉ Course có Registration thuộc cohort, không Course empty giả tăng
totalCount. Repeated Registration cho cùng Student/Course: records tăng,
distinct group một; một Student hai Course: mỗi group1, overall1, không2.
Overall distinct query riêng toàn cohort, **không SUM group distinct**, không
DISTINCT Score/Booking/Invoice hoặc distinct Student chỉ current page.
Totals empty {registrationCount:0,distinctStudentCount:0}, groups.items=[];
không0% conversion, mới "Chưa có dữ liệu" cho table/card context.
Không response Student IDs để browser tự COUNT DISTINCT; các retained states
là counted aggregates, không lọt Registration detail chỉ vì lọc theo status.
200 result; malformed400, auth401/403, invalid-scope filter404, unexpected500.

## 34. Director training — API-153 / NV31

GET /reports/director/training, Director NORMAL. Hai phần inventory và results
trong một response/snapshot, không bắt Director gọi Teacher gradebook/AA Class
detail/Student own results để dựng báo cáo.

| TrainingReportQuery | Required / kiểu / default |
| --- | --- |
| courseId, bandId, classId, teacherId | O BusinessId mỗi field, omitted all; Course/Band/Class/current assigned Teacher filters giao nhau, không Teacher tại ngày chốt. |
| status | O ClassStatus mục16, omitted all5 kể cả CANCELLED; không Program/GradeReport/Registration state hoặc state list tùy ý. |
| finalizedFromDate, finalizedToDate | O LocalDate pair như31.1, default tháng VN hiện tại; **chỉ** results, không filter inventory bằng ngày tạo/hoàn tất lớp. |
| page, pageSize | O integer theo1.2; cùng matched-Class group page cho hai phần, không independently fetching hai snapshots. Không invented gradeReportId/studentId/search/sort. |

TrainingAppliedFilters fields R courseId/bandId/classId/teacherId BusinessId
hoặc null; status ClassStatus hoặc null; finalizedFromDate/finalizedToDate
LocalDate. Current Teacher filter vẫn áp results vì chọn current Class set;
không nhìn audit để reconstruct assignment tại finalized date.

| TrainingReport / nested shape | Required fields / meaning |
| --- | --- |
| TrainingReport | queriedAt Instant, todayVn LocalDate, appliedFilters TrainingAppliedFilters, options theo31.1, inventory TrainingInventorySection, results TrainingResultsSection. |
| TrainingGroupRef | course ReportCourse, band BandReference, class ReportClass, status ClassStatus. Không roster/Teacher account payload/Program version/Student IDs. |
| TrainingInventorySection | totals {classCount ReportCount, classCountsByState TrainingClassCounts}; groups Page<TrainingInventoryGroup>. Totals toàn Class set, không finalized-period filter. |
| TrainingClassCounts | PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED ReportCount mỗi key, gồm zero, SUM bằng classCount. Keys enum draft, không state mới. |
| TrainingInventoryGroup | group TrainingGroupRef, maxSize integer1–15, placedCount integer0–15, lessons {expectedCount integer dương SQL int, heldCount ReportCount hoặc null, progressPercent ReportPercent}, finalExam {currentCount/heldCount ReportCount hoặc null}. |
| TrainingResultsSection | totals TrainingResultMetrics toàn finalized cohort, groups Page<TrainingResultsGroup>. Không list GradeReports/scores/Student distinct. |
| TrainingResultsGroup | group TrainingGroupRef; metrics TrainingResultMetrics. Cùng matched-Class IDs/page/order với inventory, kể cả class không có finalized result. |
| TrainingResultMetrics | finalizedResultCount/passingResultCount ReportCount, passRatePercent ReportPercent. Numerator≤denominator; null iff finalizedResultCount==0. |

Matched Class set dùng inventory filters, gồm ended classes; page sort class.code
ASC/class.id ASC, totalCount **số matched Classes** bằng nhau ở hai groups Pages.
Cùng page parameters và group IDs nhưng metrics giữ hai cohorts riêng;
results row có zero/null chỉ báo class chưa có kết quả trong kỳ, không failed.
Nhờ vậy date đổi không làm biến mất inventory table; results.totals không
count classes rỗng thành results. Empty Class set: cả pages items=[]/count0,
classCounts5key0, finalized/passing0 và passRatePercent=null.

Inventory mỗi Class một row: LESSON expectedCount từ frozen Program, heldCount
distinct obligations có current LESSON occurrence HELD theo FN04/VW04.
Replacement chains count một obligation, historical NOT_HELD/SUPERSEDED
không cộng, elapsed/Attendance không occurrence. Progress=100×held/expected
fixed2; non-CANCELLED no HELD→0.00, không tự null vì chưa bắt đầu.
FINAL_EXAM riêng currentCount0/1, heldCount0/1≤currentCount, không denominator
LESSON, không "thi xong" suy từ có Score/giờ đã qua.
CANCELLED giữ count/group/status/maxSize/placedCount/program expected count;
lessons.heldCount/progressPercent và finalExam.currentCount/heldCount=null =
không áp dụng delivered-progress, không count buổi đã dạy hoặc zero giả.
Không average các class percentages làm metric tổng mới; inventory totals chỉ
classCount/state breakdown đã chốt.

Results: T19 FINALIZED +finalized_at thuộc VN period +same Class filter set;
mỗi GradeReport/ClassAssignment/enrollment result một lần, không unique people.
Passing so sánh **stored rounded** final_score≥50.00; SUBMITTED/RETURNED/DRAFT
không failed/denominator, không names/scores/comment/report IDs/unfinished flags.
Empty denominator null/"Chưa có dữ liệu"; denominator>0/passing0→"0.00".
Overall rate tính từ toàn cohort count, không average/sum group/page rates.
Group gồm class code không Student-identifying labels hoặc riêng một Student row.
200 response/common errors31; read không finalize/clear unfinished/create snapshots.

## 35. Fresh whole-results finance CSV — API-154 / NV29

GET /reports/accountant/finance/csv, Accountant NORMAL. Chỉ FinancialReportQuery
business filters fromDate/toDate/courseId/overdueOnly như32; **không** page/
pageSize/sort/fields/format/queryId/asOf/expectedVersions/client totals/body.
Unknown pagination parameters400, không silently chỉ xuất trang đã xem.
Fresh service query theo same definitions/group grain/SNAPSHOT/SP04/FN06;
authorization mới mỗi request, không caller-result lookup, stored job/file URL
hoặc T23/audit mutation. Không CSV Admissions/training/Director/Admin audit.

### 35.1. Wire/file layout đề xuất

| 200 response field/header | Shape / scope |
| --- | --- |
| Content-Type | text/csv; charset=utf-8; body binary UTF-8, BOM EF BB BF trước header record. Không JSON success envelope/base64 hoặc HTML error download. |
| Content-Disposition | attachment; filename="finance-YYYYMMDD-HHmmss.csv"; date/time UTC từ queriedAt, ASCII server-generated, không user labels/email/path. |
| Cache-Control | no-store, như report; request/error không lưu durable browser cache. |
| X-Report-Queried-At | Instant của **export** snapshot; UI không dùng report page queriedAt gắn vào file mới. |
| X-Report-From-Date / X-Report-To-Date | LocalDate đã resolved, applied revenue/billed period; không debt/as-of boundary. |
| X-Report-Course-Id | decimal BusinessId đã applied hoặc literal ALL; không PII. |
| X-Report-Overdue-Only | literal true/false đã applied; chỉ current-debt subset. |
| X-CSV-Data-Row-Count | integer text số data records thực sự phát, gồm total row dưới đây, không header/physical line count hoặc UTF8 bytes. |

Không thêm report context columns ẩn: query/time giữ trong response headers,
UI download state RAM và filename. Request từ UI gửi resolved applied dates
để default tháng không đổi âm thầm nếu đang xem kết quả tháng trước.
Download chỉ success khi status200/content type CSV và whole body đọc xong;
ErrorResponse JSON khác status không lưu thành finance.csv/thông báo thành công.

Đề xuất fixed6 table/CSV columns, đúng thứ tự, cũng hiển thị ở F30:
"Mã khóa", "Tên khóa", "Doanh thu trong kỳ (VND)", "Phải thu trong kỳ (VND)",
"Công nợ hiện tại (VND)", "Công nợ quá hạn (VND)".
Nguồn mỗi Course row: course.code/name, metrics.revenueAmount/billedAmount/
currentDebtAmount/overdueDebtAmount. overdueOnly=true UI/table label làm rõ
hai debt columns đang filtered quá hạn; header kỳ ngày không áp debt.
Không ID/status/profile/InvoiceNo/Payment ref/contact/hidden link columns.

Toàn matching groups sorted code/id như150, không Page.size limit100.
Một footer cũng hiển thị ở table: code="", name="Tổng toàn bộ tập lọc",
4 FinanceMetrics whole-set totals. Footer không giả Course hoặc làm group
totalCount tăng1; CSV data row cap **tính footer** (rows=groupCount+1), header
không tính. Empty cohort→header+zero totals footer200, không fake Course row.
Footer/header/columns/byte convention ở đây là wire refinement, chưa owner
duyệt actual E04. Không sửa ADR-072/101 thành hidden data dumps.

### 35.2. Limits, quoting và formula safeguards

10.000 data records hoặc5MB cap theoADR-101: vượt **một** cap thì reject whole
file, không truncate/page-only/stream valid prefix rồi báo success.
Đề xuất cụ thể5MB=5.000.000 **UTF8 body bytes**, gồm BOM/header/footer/quotes/
apostrophe escapes/comma/CRLF; đúng cap được xuất, vượt1byte/row không.
Đơn vị bytes/footer counting là refinement để rà, không khẳng định ADR từng
đã chọn SI-vs-MiB hoặc total-row layout.
Dựng/check bounded whole output trước gửi200/CSV headers/body; vượt→409
CSV_EXPORT_LIMIT ErrorResponse (proposed subcode), fieldErrors map rỗng,
message đề nghị thu hẹp **Course/kỳ phù hợp**, không expose observed row/IDs
ngoài quyền; đổi period không hứa giảm date-free debt. Không413 do request
payload quá lớn hoặc successful empty file. No background jobs/temp export
store/package mới. Timeout/cancel/internal500 không phần CSV200; streaming
transport sau header nếu bị đứt là failed download, không partial success.

Mọi cell kể cả headers/money được quote bằng double quote, embedded quote
doubled; delimiter comma, record CRLF, cell comma/quote/newline giữ đúng một
cell. Numeric cells là canonical ReportAmount digits, không thousand separator/
VND symbol/scientific notation/float. Count cap là logical CSV records, không
split text lines khi Course name chứa newline hoặc multibyte Unicode.
Tên Course là plain text từ DB, không execute/normalize/cắt nội dung trong DB
để export. Đề xuất guard text cells: nếu có TAB/CR/LF hoặc first non-whitespace
character là =,+,-,@ hay full-width ＝,＋,－,＠ thì prepend một apostrophe **ở đầu
cell trước quote/escape**; leading whitespace inspection không xóa text gốc.
Static trusted header/footer cũng qua cùng renderer, không công thức SUM CSV.

Kế thừa [OWASP CSV Injection](https://community.owasp.org/attacks/CSV_Injection):
escaping quote/delimiter và formula-leading/full-width chars đều cần xét;
save/reopen có thể làm mitigation mất hiệu lực, không universal guarantee.
Giữ apostrophe policy đã accepted ADR-101, không tự đổi sang tab-prefix theo
một alternative trên trang nguồn. Target Excel/LibreOffice import/open/save/
reopen/locale phải kiểm **sau triển khai**; chưa có browser/spreadsheet test.
UTF8 CSV giữ exact digits, nhưng app không hứa spreadsheet tự giữ precision
decimal38; hướng dẫn import tiền dưới dạng Text, không round DB/output để vừa
numeric cell. Cảnh báo giới hạn khi chỉnh/lưu/mở lại theo đã duyệt.

## 36. Reports/CSV — ví dụ và mục tiêu kiểm chứng

### 36.1. JSON — finance, nhóm union +debt không kỳ ngày

Fake Accountant report, tháng10 VN; revenue100 là Payment confirm trong kỳ
của Invoice issue trước kỳ; billed200 gồm Invoice issue trong kỳ; debt300
có overdue100 và current non-overdue200, có thể ngoài kỳ issued.
Không được suy currentDebtAmount=200−100=100. Nhóm Course1 cùng whole totals
vì fixture chỉ một Course; queriedAt03/10 09:00 VN.

```json
{
  "queriedAt": "2026-10-03T02:00:00.000Z",
  "todayVn": "2026-10-03",
  "appliedFilters": {
    "fromDate": "2026-10-01",
    "toDate": "2026-10-31",
    "courseId": null,
    "overdueOnly": false
  },
  "options": {
    "courses": [
      { "id": 1, "code": "ENG-A", "name": "Tiếng Anh A", "status": "INACTIVE" }
    ]
  },
  "totals": {
    "revenueAmount": "100",
    "billedAmount": "200",
    "currentDebtAmount": "300",
    "overdueDebtAmount": "100"
  },
  "groups": {
    "items": [
      {
        "course": { "id": 1, "code": "ENG-A", "name": "Tiếng Anh A" },
        "metrics": {
          "revenueAmount": "100",
          "billedAmount": "200",
          "currentDebtAmount": "300",
          "overdueDebtAmount": "100"
        }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1
  }
}
```

### 36.2. JSON — admissions, distinct không cộng nhóm

Fixture một Student có2 Registration Course1 (gồm học lại) và1 Registration
Course2 trong kỳ; groups mỗi Course distinct1, overall distinct1 chứ không2.
IDs Student/Registration của fixture **không** nằm trong response Director.

```json
{
  "queriedAt": "2026-10-03T02:00:00.000Z",
  "todayVn": "2026-10-03",
  "appliedFilters": {
    "fromDate": "2026-10-01",
    "toDate": "2026-10-31",
    "courseId": null,
    "status": null
  },
  "options": {
    "courses": [
      { "id": 1, "code": "ENG-A", "name": "Tiếng Anh A", "status": "INACTIVE" },
      { "id": 2, "code": "ENG-B", "name": "Tiếng Anh B", "status": "ACTIVE" }
    ]
  },
  "totals": { "registrationCount": 3, "distinctStudentCount": 1 },
  "groups": {
    "items": [
      {
        "course": { "id": 1, "code": "ENG-A", "name": "Tiếng Anh A" },
        "metrics": { "registrationCount": 2, "distinctStudentCount": 1 }
      },
      {
        "course": { "id": 2, "code": "ENG-B", "name": "Tiếng Anh B" },
        "metrics": { "registrationCount": 1, "distinctStudentCount": 1 }
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 2
  }
}
```

### 36.3. CSV — course rows/whole-set footer

Từ **fresh** query ra cùng fixture finance trên, body sau BOM được minh họa
bằng các record bên dưới. File thật là UTF8BOM/CRLF, markdown không chứng minh
bytes/delimiter/Excel behavior. X-CSV-Data-Row-Count=2: một Course +một footer,
không header; không metadata/profile columns.

```csv
"Mã khóa","Tên khóa","Doanh thu trong kỳ (VND)","Phải thu trong kỳ (VND)","Công nợ hiện tại (VND)","Công nợ quá hạn (VND)"
"ENG-A","Tiếng Anh A","100","200","300","100"
"","Tổng toàn bộ tập lọc","100","200","300","100"
```

Targets **sau triển khai**:7-role direct/list/filter/options/export negative
access; no individual IDs/contact/Student scores; read-only SP caller grants;
one SNAPSHOT/options/count/totals/page vs concurrent pay/cancel/grade-finalize/
occurrence; course union tránh mất receipt/debt ngoài kỳ; current CANCELLED
billed exclusion; overdue today/equality/VN midnight/default month/UTC bounds;
debt flag không filter revenue/billed; decimal38 zero/max/overflow/large digits;
distinct repeat/multi-Course/overall not summed; current Teacher reassignment;
no date inventory; CANCELLED non-delivery but retained class counts; replacement
single obligation/FINAL_EXAM separate; FINALIZED final-date cohort/rounded50/
empty null vs nonempty0.00/rate from whole-cohort counts not averaged groups;
page20/max100/empty page/totals all/dual Training page sameIDs; stale options/
valid incompatible filters/no hidden eligibility or manager endpoint access;
export fresh applied filters vs draft/old-page response; full CSV no page cap,
header/footer/10000/10001 records/5MB±1 bytes/BOM/multibyte/CRLF/quotes/comma/
newline/leading whitespace/full-width/TAB/CR/LF formulas; DB text unchanged;
Excel/LibreOffice import/precision/open/save/reopen warning; reject before
200/no prefix success/error JSON not file/timeout/cancel/auth expired download.
Không claim HTTP/SQL/Excel/browser/race/load tests đã chạy.

## 37. Checkpoint D10 — kiểm cấu trúc và việc còn lại ở thời điểm đó

D10 có154/154 schema drafts API-001–154: giữ149 D9, thêm5 Reports/CSV entries;
không còn API chưa có schema miền, **chưa** proof whole SC-01 complete/accepted.
Inventory154 patterns/55 UUID commands/6 caller-result paths không đổi.
New report options/filter/date/group/percent/CSV columns/footer/bytes/header/
error refinements đều draft để rà actual E04. Không thêm metric/report rights,
schema DB/ADR mới hoặc tick E04/E05. RF03 accepted ADR-156, RF01/02/04 Accepted ADR-157–159.

SC-01 còn rà whole154 registry/field/sort/null/guard/error/example parity cùng
SC-02 action→normalized DTO/canonical UTF8 bytes/digest/55 commands/6 lookups/
outcomes/no-op/compatibility; SC-03 SQL signatures/EF/grants/locks/read-snapshot/
audit field allowlists; SC-04 typed config/runtime/runbook/prerequisites và
D12/D13 deferred. Checkpoint SC02–04-v1 ngày03/10/2026 đã có các phụ lục
dự thảo bên dưới và rà tổng hợp SC01; next actual E04 refinement review; RF01/02/04 Accepted157–159.
Chỉ kiểm cấu trúc tài liệu; đủ154 drafts không actual E04/E05 nghiệm thu,
backend/frontend/SQL/tests. Không code/SQL/OpenAPI implementation/Git/scaffold/
Slice0/implementation plan cho đến khi full baseline được owner nghiệm thu.


## 38. Addendum SC02–04-v1 — 03/10/2026

154 schema miền D10 không thêm/xóa API. Những câu “còn SC-02/03” trong lịch sử
các mục trên là điểm bàn giao của D2–D10, hiện nối các draft refinements sau:

- [SC02 actions/normalization/bytes/result/replay](E04_REPLAY_SPEC.md):55 keys,
  target/lookup allowlists,codec EC-C1/NULL-vs-omitted/sets,first/replay status và
  SafeCommandCompletion fallback union,expiry/compatibility;no secrets persisted.
- [SC03 SQL/EF/grants/locks/audit](E04_SQL_INTEGRATION_SPEC.md):7SP/6FN,
  decimal38 aggregate→digits string,5trigger mappings,24table grants/55command locks.
- [SC04 typed profile/readiness/deferred](E04_RUNTIME_PROFILE.md).
- [SC01 consolidated parity review](E04_PARITY_REVIEW.md):findings và owner choices.

Các exact selectors/bytes/SP/EF/error/outcome refinements **vẫn proposed**,
không claim runtime/schema implementation hoặc automatic E04 acceptance.
SC02 authoritative draft refinement cho selector strings/replay union thay
placeholder “registry chưa viết”; từng first CommandResult D10 giữ nguyên.
Completed replay200 có thể return SafeCommandCompletion cho weekly (only classId
marker) hoặc current safe refs khi full CommandResult không dựng trong quyền;
union nhận diện `outcome=COMPLETED`/resultMode/target đối với completion, không
PII/secret snapshot/created[] giả. Lookup200 luôn completion,không full detail.
RF01/02/04 Accepted ADR-157–159; RF03 giữ ADR-156. Field literal/body limits/deadlines/
environment values phải rà actual E04/D12/D13 đúng scope,không guessing policy.
## 39. Accepted RF addendum — 03/10/2026

Owner duyệt RF01/RF02/RF04 →ADR-157/158/159; requirements4.9/NV/W đã synced.
RF01/API055 future start cutoff; RF02/API064/072 equality400; RF04 P/G no-change
behavior tại SC02 mục5 đã Accepted. Exact wire literal/shape/codec/SP/EF/ref
unions chưa được nghiệm thu toàn E04. Không extra endpoints/schema/rights.
API133 mixed save `saved` chỉ actual changed rows; API139 parentRV touch chỉ
actual score/comment mutation. Toàn supplied intent/versions/absence/full-set
vẫn validate/hash, không discard equal stale items. All-equal400/no new marker;
completed same-key replay không evaluate equality thành new no-change request.
RF03 giữ156;E04/E05 chưa Accepted,D12/D13 deferred,no code/SQL/tests.
