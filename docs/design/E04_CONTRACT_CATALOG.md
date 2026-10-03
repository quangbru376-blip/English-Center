# E04 — Danh mục hợp đồng API, frontend và truy vết

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản: E04-C1 trong E04-v1, cập nhật03/10/2026. Trạng thái: **Accepted trong E04-v1 ADR-160; E05 Accepted ADR-161**.

Đây là phụ lục của [SYSTEM_ARCHITECTURE_SPEC](SYSTEM_ARCHITECTURE_SPEC.md),
không phải kế hoạch triển khai, source code hoặc OpenAPI đã chạy.
Nguồn: [requirements](../requirements.md), [ADRs](../DECISIONS.md),
[E01-v1](FUNCTION_SPEC.md), [E02-v1](WORKFLOW_SPEC.md), [E03-v1](DATABASE_SPEC.md),
[23 mục E04 đã duyệt](../plan/E04_REMAINING_REVIEW.md).
ADR-001–155 giữ nguyên; không hỏi lại các policy/menu/save-unit đã chốt.
OWNER_REVIEW vẫn là nơi nghiệm thu E04/E05 duy nhất; D12/D13 vẫn hoãn.

## 1. Phạm vi và kết quả kiểm cấu trúc

- Gom140 dòng route trong bảng mục4–13 của bản chính; tách4 Admin status
  commands vốn nằm chung một dòng, hợp nhất các caller-result paths lặp lại.
- Viết đầy đủ10 paths bảng điểm ở mục14 từ contract minh họa đã trình,
  không giữ ký hiệu `...`; thêm5 report/export paths đã trình ở mục15.
- Danh mục có **154 method/path duy nhất**:76 READ,18 PREVIEW,58 COMMAND,
  2 AUTH (login/logout). Trong58 COMMAND,55 dùng UUID Idempotency-Key;
  signup/profile/password-change là3 ngoại lệ đã chốt. Các nhãn này là
  phân loại tài liệu, không enum DB/action code hoặc dependency mới.
- Nối32 F (F01–F33 trừ F15),21 W và37 retained NV theo bảng nguồn E01/E02;
  giữ NV10/NV11/F15 excluded và không renumber. Coverage ở đây là đối chiếu
  tài liệu, **không** chứng minh mọi DTO/guard/SQL contract đã đủ hoặc tests pass.
- Số154 không là mục tiêu rubric hoặc yêu cầu phải làm đủ154 controllers.
  Không tạo endpoint để tăng số lượng; nhiều actions có thể cùng controller.

Phần route/API hành vi có nguồn ở bản chính đã trình; tên URL frontend,
ID tham chiếu API và cách nối các bảng là chi tiết tổng hợp mới để rà trong
bản E04 thực tế, **chưa được duyệt trước bởi23 dấu tick**. Request/response
trong bảng là contract logic đang có, chưa thay schema field-by-field cuối cùng.
Current refinement và trạng thái tài liệu ở mục8; [final contract](E04_FINAL_CONTRACT.md) giải quyết các checkpoint placeholders, không auto nghiệm thu.

## 2. Hợp đồng chung và ranh giới trách nhiệm

Mọi API path dưới `/api/v1`. Controller bind DTO/validate shape; Service kiểm
identity/role/ownership, state/time/relations, sở hữu transaction và effects;
Repository dùng connection/transaction caller cho EF/SP/UDF. Không repository
hoặc module phụ commit audit/replay/allocator độc lập. Reports chỉ đọc,
không tạo fact store hoặc cá nhân snapshot. Backend quyết tiền/điểm/quyền;
UI allowedActions/disabledReasons không là authority.

| Nhóm | Quy tắc đã chốt được áp dụng khi hoàn thiện schema |
| --- | --- |
| Identity | Actor/time/session từ server; không nhận actorId/role/status/sid/hash hoặc total/pass do client quyết định. Mỗi role projection riêng; route prefix không cấp role. |
| Version | Base64 opaque của rowversion8 byte; version không cấp quyền. Form dùng đúng tokens/context đã load; parent RV không thay child membership/expected absence hoặc live clock. |
| Collections | Default20/max100, stable sort có ID tie-breaker; tên/email/SĐT chỉ POST body/RAM, không URL/storage/log payload. Whole-set guard không lấy paginated subset. |
| Errors | code/message/fieldErrors/traceId;400 malformed/validation,401 invalid session,403 role hoặc CSRF_INVALID đúng context,404 missing/out-of-scope,409 stale/state/race. Login429/Retry-After theo ADR-100; không raw SQL/stack/value bí mật. |
| Precision/time | VND integer digit strings, DB decimal; score0–100/weight>0–100 tối đa2 decimals và half-up final total một lần; UTC instants/ISO Z, ngày/giờ VN/minute boundary. Không dùng JS money arithmetic/blank→0/round input. |
| Read/preview | GET và POST search/preview không business mutation/reservation hoặc replay marker. Activity/session checks giữ policy Auth; không đóng băng thời gian, không giữ TX qua user confirmation. |
| Write | Required fields/versions/context kiểm lại dưới locks; toàn save unit commit hoặc rollback với mandatory audit/completion. Không generic status setter/delete/history purge/on-behalf hoặc business action tự chạy theo UI state. |
| Unknown/replay | Một UUID/intent/content; unknown giữ intent để tra caller result, không retry tự động. Completed replay recheck quyền hiện tại rồi dựng safe refs/projection, không secret/PII snapshot; absence/expiry marker không chứng minh chưa commit. |
| Auth exceptions | Signup, profile, own password change không UUID credential/form replay; không auto retry. Login/logout theo scope riêng; bootstrapping/me/health/timer không gia hạn idle. |

`TX01–13` là những save units trong E03; không tự thêm TX14. Khi E03 không
đặt ID riêng (profile, confirm, Session management, metadata, config, một số
Class/Schedule actions), ghi workflow atomic + shared protocol, không gán
bừa TX07/TX09 chỉ vì cùng module. Exact command/guard mapping vẫn phải rà.

## 3. Danh mục method/path thống nhất

READ gồm GET và POST search; PREVIEW là read-only POST preview; COMMAND là
nghiệp vụ ghi; AUTH là login/logout. Dấu UUID áp dụng Idempotency-Key; dấu
“không UUID” không có nghĩa không transaction/auth/CSRF. Query action của
caller lookup không tạo route riêng cho từng action. `{cId}/{id}` cùng vị trí
resource là cùng path pattern; giữ cách ghi nguồn, không thêm alias routes.
API-xxx chỉ là ID tài liệu; không tự dùng nó làm persisted replay action.

### 3.1. Gói2a — API xác thực và phiên đã trình được duyệt

Nguồn contract: bản chính mục4; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-001 | GET /auth/csrf | Anonymous hoặc identity hiện tại; W02 | Không business input → requestToken | READ; —; không UUID |
| API-002 | POST /auth/login | Trước login; W02 | email/password → AuthContext, đặt JWT cookie | AUTH; —; không UUID |
| API-003 | GET /auth/me | 7 role NORMAL/PASSWORD_CHANGE, scope Auth; W02 | Cookie → AuthContext | READ; —; không UUID |
| API-004 | POST /auth/change-password | 7 role NORMAL/PASSWORD_CHANGE, scope Auth; W02 | currentPassword/newPassword/expectedVersions.account →204 | COMMAND; TX13; không UUID |
| API-005 | POST /auth/logout | 7 role NORMAL/PASSWORD_CHANGE, scope Auth; W02 | Không business input →204 | AUTH; —; không UUID |

### 3.2. Gói2b — Administration đã trình được duyệt

Nguồn contract: bản chính mục5; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-006 | POST /admin/accounts/search | Admin NORMAL; W03 | name/email/phone nếu cần + role/status/page/pageSize → trang AccountSummary | READ; —; không UUID |
| API-007 | GET /admin/accounts/{id} | Admin NORMAL; W03 | ID → AccountAdminDetail/version | READ; —; không UUID |
| API-008 | POST /admin/staff-accounts | Admin NORMAL; W01 | fullName/email/role/phone tùy chọn → first credential result | COMMAND; TX01; UUID |
| API-009 | POST /admin/accounts/{id}/reset-password | Admin NORMAL; W03 | expectedVersions.account → first credential result | COMMAND; TX13; UUID |
| API-010 | POST /admin/accounts/{id}/lock | Admin NORMAL; W03 | expectedVersions.account → AccountAdminDetail mới/operationId | COMMAND; TX13; UUID |
| API-011 | POST /admin/accounts/{id}/unlock | Admin NORMAL; W03 | expectedVersions.account → AccountAdminDetail mới/operationId | COMMAND; TX13; UUID |
| API-012 | POST /admin/accounts/{id}/disable | Admin NORMAL; W03 | expectedVersions.account → AccountAdminDetail mới/operationId | COMMAND; TX13; UUID |
| API-013 | POST /admin/accounts/{id}/restore | Admin NORMAL; W03 | expectedVersions.account → AccountAdminDetail mới/operationId | COMMAND; TX13; UUID |
| API-014 | GET /admin/command-results/{requestId} | Admin NORMAL; W01/W03 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |
| API-015 | GET /admin/audit-events | Admin NORMAL; W21 | fromDate/toDate/actorId/action/entityType/entityId/result/page/pageSize → event page | READ; —; không UUID |
| API-016 | GET /admin/audit-events/{id} | Admin NORMAL; W21 | ID → event detail allowlist | READ; —; không UUID |
| API-017 | GET /admin/operations/{operationId}/audit-events | Admin NORMAL; W21 | operation UUID/page/pageSize → event page | READ; —; không UUID |

### 3.3. Gói3a — Student API cơ bản đã trình được duyệt

Nguồn contract: bản chính mục6; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-018 | POST /auth/register | Tự đăng ký Student, trước login; W01 | fullName/dateOfBirth/email/phone/password/address tùy chọn →201, outcome CREATED | COMMAND; TX01; không UUID |
| API-019 | GET /students/me/profile | Student NORMAL, own; W04 | Không studentId input → own StudentProfileView | READ; —; không UUID |
| API-020 | PUT /students/me/profile | Student NORMAL, own; W04 | fullName/dateOfBirth/phone/address + expectedVersions.account/profile → profile mới/tokens | COMMAND; W04 atomic; không UUID |
| API-021 | GET /students/me/home | Student NORMAL, own; W04 | page/pageSize → trang thẻ own Registration | READ; —; không UUID |
| API-022 | GET /students/me/courses | Student NORMAL, own; W05 | page/pageSize → trang CourseSummary ACTIVE | READ; —; không UUID |
| API-023 | GET /students/me/courses/{id} | Student NORMAL, own; W05 | Course ID → CourseDetail ACTIVE trong scope chọn khóa | READ; —; không UUID |

### 3.4. Gói3b — Student Registration/thi đầu vào/xác nhận đã trình được duyệt

Nguồn contract: bản chính mục7; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-024 | GET /registrations | Student NORMAL, own; W05 | courseId/status/page/pageSize → own RegistrationSummary page | READ; —; không UUID |
| API-025 | POST /registrations | Student NORMAL, own; W05 | courseId →201 Registration mới/operationId | COMMAND; W05 atomic; UUID |
| API-026 | GET /registrations/{rId} | Student NORMAL, own; W05 | ID → own RegistrationDetail/version/allowedActions | READ; —; không UUID |
| API-027 | POST /registrations/{rId}/cancellation-preview | Student NORMAL, own; W05 | Không business input → eligible/effects/guard manifest | PREVIEW; —; không UUID |
| API-028 | POST /registrations/{rId}/cancel | Student NORMAL, own; W05 | expectedVersions/expectedSet từ preview →200 detail/operationId | COMMAND; TX06; UUID |
| API-029 | GET /registrations/{rId}/entry-test-options | Student NORMAL, own; W07 | fromDate/toDate/page/pageSize → attempt context/eligible session page | READ; —; không UUID |
| API-030 | POST /registrations/{rId}/entry-test-bookings | Student NORMAL, own; W07 | targetSessionId/attemptContext/expectedVersions →201 own Booking/operationId | COMMAND; TX02; UUID |
| API-031 | POST /registrations/{rId}/entry-test-bookings/{bId}/cancel | Student NORMAL, own; W07 | expectedVersions →200 updated own entry context/operationId | COMMAND; TX02; UUID |
| API-032 | POST /registrations/{rId}/entry-test-bookings/{bId}/reschedule | Student NORMAL, own; W07 | targetSessionId/expectedVersions →200 old-new Booking refs/operationId | COMMAND; TX02; UUID |
| API-033 | GET /entry-test-bookings | Student NORMAL, own; W07 | fromDate/toDate/view current-history/registrationId tùy chọn/page/pageSize → own calendar page | READ; —; không UUID |
| API-034 | GET /registrations/{rId}/entry-results | Student NORMAL, own; W08 | ID → own attempt results/current best/Band | READ; —; không UUID |
| API-035 | POST /registrations/{rId}/confirmation-preview | Student NORMAL, own; W09 | Không business input → eligible/Band/attempt context/guard manifest | PREVIEW; —; không UUID |
| API-036 | POST /registrations/{rId}/confirm | Student NORMAL, own; W09 | expectedVersions/expectedSet từ preview →200 CONFIRMED/server time/operationId | COMMAND; W09 atomic; UUID |
| API-037 | GET /command-results/{requestId} | Student NORMAL, own; W05/W07/W09 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |

### 3.5. Gói3c — Admissions/Invoice/thu tiền ngoài ứng dụng đã trình được duyệt

Nguồn contract: bản chính mục8; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-038 | POST /admissions/registrations/search | Admissions NORMAL; W10 | name/email/phone/courseId/status/valid date filters/page/sort → AdmissionsRegistrationPage | READ; —; không UUID |
| API-039 | GET /admissions/registrations/{id} | Admissions NORMAL; W10 | ID → AdmissionsRegistrationDetail/versions/allowedActions | READ; —; không UUID |
| API-040 | POST /admissions/registrations/{id}/approval-preview | Admissions NORMAL; W10 | Không business input → guard context/tuition/expected versions | PREVIEW; —; không UUID |
| API-041 | POST /admissions/registrations/{id}/approve | Admissions NORMAL; W10 | expectedVersions/expectedSet từ preview →200 ADMITTED/InvoiceSummary/operationId | COMMAND; TX04; UUID |
| API-042 | POST /admissions/registrations/{id}/reject | Admissions NORMAL; W10 | reason/expectedVersions.registration →200 REJECTED/detail/operationId | COMMAND; W10 atomic; UUID |
| API-043 | POST /admissions/registrations/{id}/cancellation-preview | Admissions NORMAL; W10 | Không business input → eligible/dependent effects/version-set | PREVIEW; —; không UUID |
| API-044 | POST /admissions/registrations/{id}/cancel | Admissions NORMAL; W10 | reason/expectedVersions/expectedSet →200 CANCELLED/detail/operationId | COMMAND; TX06; UUID |
| API-045 | GET /admissions/command-results/{requestId} | Admissions NORMAL; AcademicAffairs NORMAL; W10/W06 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |
| API-046 | GET /students/me/invoices | Student NORMAL, own; W11 | own registrationId/status/page/pageSize → InvoiceStudentPage | READ; —; không UUID |
| API-047 | GET /students/me/invoices/{id} | Student NORMAL, own; W11 | ID → InvoiceStudentDetail/print data | READ; —; không UUID |
| API-048 | POST /finance/invoices/search | Accountant NORMAL; W11 | invoiceNumber/courseId/name/email/phone/status/issueFromDate/issueToDate/overdueOnly/page/sort → InvoiceAccountantPage | READ; —; không UUID |
| API-049 | GET /finance/invoices/{id} | Accountant NORMAL; W11 | ID → InvoiceAccountantDetail/versions/allowedActions | READ; —; không UUID |
| API-050 | POST /finance/invoices/{id}/collection-preview | Accountant NORMAL; W12 | Không business input → eligible/exact amount/version context | PREVIEW; —; không UUID |
| API-051 | POST /finance/invoices/{id}/collect | Accountant NORMAL; W12 | amount/method/transactionReference/expectedVersions/expectedSet →201 Payment/PAID/detail/operationId | COMMAND; TX05; UUID |
| API-052 | PUT /finance/payments/{id}/metadata | Accountant NORMAL; W12 | method/transactionReference/reason/expectedVersions.payment-invoice →200 Payment/detail/operationId | COMMAND; W12 atomic metadata; UUID |
| API-053 | GET /finance/command-results/{requestId} | Accountant NORMAL; W12 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |

### 3.6. Gói4a — ca thi đầu vào, phân công và Teacher chấm thi đã trình được duyệt

Nguồn contract: bản chính mục9; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-054 | GET /admissions/entry-test-sessions | AcademicAffairs NORMAL; W06 | VN date range/effective status/graderId/page/sort → AcademicSessionPage | READ; —; không UUID |
| API-055 | POST /admissions/entry-test-sessions | AcademicAffairs NORMAL; W06 | VN date/startTime/endTime/room/capacity/graderId →201 AcademicSessionDetail | COMMAND; W06 atomic; UUID |
| API-056 | GET /admissions/entry-test-sessions/{sId} | AcademicAffairs NORMAL; W06 | ID → AcademicSessionDetail/versions/allowedActions | READ; —; không UUID |
| API-057 | PUT /admissions/entry-test-sessions/{sId}/schedule | AcademicAffairs NORMAL; W06 | VN date/startTime/endTime/room/expectedVersions.session →200 detail | COMMAND; W06 atomic; UUID |
| API-058 | POST /admissions/entry-test-sessions/{sId}/open | AcademicAffairs NORMAL; W06 | expectedVersions.session →200 detail | COMMAND; W06 atomic; UUID |
| API-059 | POST /admissions/entry-test-sessions/{sId}/close | AcademicAffairs NORMAL; W06 | expectedVersions.session →200 detail | COMMAND; W06 atomic; UUID |
| API-060 | POST /admissions/entry-test-sessions/{sId}/cancellation-preview | AcademicAffairs NORMAL; W06 | Không business input → affected Booking set/versions | PREVIEW; —; không UUID |
| API-061 | POST /admissions/entry-test-sessions/{sId}/cancel | AcademicAffairs NORMAL; W06 | reason/expectedVersions/expectedSet →200 cancelled detail/operationId | COMMAND; W06 atomic; UUID |
| API-062 | GET /admissions/entry-test-grader-options | AcademicAffairs NORMAL; W06 | VN date/time/optional sessionId → minimal current/eligible Teacher options | READ; —; không UUID |
| API-063 | POST /admissions/entry-test-sessions/{sId}/grader-change-preview | AcademicAffairs NORMAL; W06 | graderId → old/new grader/context/conflicts/versions | PREVIEW; —; không UUID |
| API-064 | POST /admissions/entry-test-sessions/{sId}/change-grader | AcademicAffairs NORMAL; W06 | graderId/reason/expectedVersions/expectedSet →200 detail/operationId | COMMAND; W06 atomic; UUID |
| API-065 | POST /admissions/entry-test-sessions/{sId}/roster/search | AcademicAffairs NORMAL; W06 | name/email/phone/booking-state/page/sort → AcademicRosterPage | READ; —; không UUID |
| API-066 | GET /admissions/teachers/me/entry-test-sessions | Teacher NORMAL, current assigned/original grader guards; W06 | VN date/effective status/page/sort → TeacherSessionPage | READ; —; không UUID |
| API-067 | GET /admissions/teachers/me/entry-test-sessions/{sId} | Teacher NORMAL, current assigned/original grader guards; W06 | ID → TeacherSessionDetail | READ; —; không UUID |
| API-068 | POST /admissions/teachers/me/entry-test-sessions/{sId}/roster/search | Teacher NORMAL, current assigned/original grader guards; W06 | name/email/phone/state/page/sort → TeacherRosterPage | READ; —; không UUID |
| API-069 | GET /admissions/teachers/me/entry-test-bookings/{bId} | Teacher NORMAL, current assigned/original grader guards; W08 | ID → TeacherEntryTaskDetail/versions/related context | READ; —; không UUID |
| API-070 | POST /admissions/teachers/me/entry-test-bookings/{bId}/score | Teacher NORMAL, current assigned/original grader guards; W08 | score/expectedVersions/expectedSet →200 completed task/operationId | COMMAND; TX03; UUID |
| API-071 | POST /admissions/teachers/me/entry-test-bookings/{bId}/absence | Teacher NORMAL, current assigned/original grader guards; W08 | expectedVersions/expectedSet →200 absent task/operationId | COMMAND; TX03; UUID |
| API-072 | PUT /admissions/teachers/me/entry-test-attempts/{tId}/score | Teacher NORMAL, current assigned/original grader guards; W08 | score/reason/expectedVersions/expectedSet →200 corrected task/operationId | COMMAND; TX03; UUID |
| API-073 | GET /admissions/teachers/me/command-results/{requestId} | Teacher NORMAL, current assigned/original grader guards; W08 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |

### 3.7. Gói4b — Course, chương trình và cấu hình điểm đã trình được duyệt

Nguồn contract: bản chính mục10; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-074 | GET /academics/courses | AcademicAffairs NORMAL; W13 | code/name/status/page/sort → AcademicCoursePage | READ; —; không UUID |
| API-075 | POST /academics/courses | AcademicAffairs NORMAL; W13 | code/name/basePrice/status ACTIVE-or-INACTIVE →201 CourseDetail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-076 | GET /academics/courses/{cId} | AcademicAffairs NORMAL; W13 | ID → CourseDetail/version/allowedActions | READ; —; không UUID |
| API-077 | PUT /academics/courses/{cId}/details | AcademicAffairs NORMAL; W13 | name/basePrice/expectedVersions.course →200 detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-078 | POST /academics/courses/{cId}/deactivate | AcademicAffairs NORMAL; W13 | expectedVersions.course →200 INACTIVE/detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-079 | POST /academics/courses/{cId}/activate | AcademicAffairs NORMAL; W13 | expectedVersions.course →200 ACTIVE/detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-080 | GET /academics/bands | AcademicAffairs NORMAL; W13 | Không business input → BandReferenceSet | READ; —; không UUID |
| API-081 | GET /academics/training-programs | AcademicAffairs NORMAL; W13 | courseId/bandId/name/status/page/sort → ProgramPage | READ; —; không UUID |
| API-082 | POST /academics/training-programs | AcademicAffairs NORMAL; W13 | courseId/bandId/name/expectedLessonCount/components[] →201 ProgramDetail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-083 | GET /academics/training-programs/{pId} | AcademicAffairs NORMAL; W13 | ID → ProgramDetail/components/versions/allowedActions | READ; —; không UUID |
| API-084 | PUT /academics/training-programs/{pId}/configuration | AcademicAffairs NORMAL; W13 | name/courseId/bandId/expectedLessonCount/components[]/expectedVersions/expectedSet →200 detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-085 | PUT /academics/training-programs/{pId}/name | AcademicAffairs NORMAL; W13 | name/expectedVersions.program →200 detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-086 | POST /academics/training-programs/{pId}/activation-preview | AcademicAffairs NORMAL; W13 | Không business input → validation summary/versions/component set | PREVIEW; —; không UUID |
| API-087 | POST /academics/training-programs/{pId}/activate | AcademicAffairs NORMAL; W13 | expectedVersions/expectedSet →200 ACTIVE/detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-088 | POST /academics/training-programs/{pId}/return-to-draft | AcademicAffairs NORMAL; W13 | expectedVersions.program →200 DRAFT/detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-089 | POST /academics/training-programs/{pId}/deactivate | AcademicAffairs NORMAL; W13 | expectedVersions.program →200 INACTIVE/detail/operationId | COMMAND; W13 atomic / config gates; UUID |
| API-090 | GET /academics/command-results/{requestId} | AcademicAffairs NORMAL; Teacher NORMAL, current Class; Teacher/AcademicAffairs NORMAL, action-scoped; W13/W14/W15/W16/W17/W18/W19 | requestId UUID + action query allowlist → caller-scoped safe completion/ref (mục4) | READ; —; không UUID |

### 3.8. Gói4c — Class, xếp lớp và đọc đúng phạm vi

Nguồn contract: bản chính mục11; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-091 | GET /academics/classes | AcademicAffairs NORMAL; W14 | classCode/courseId/bandId/programId/teacherId/status/page/sort → AcademicClassPage | READ; —; không UUID |
| API-092 | POST /academics/classes | AcademicAffairs NORMAL; W14 | classCode/programId/teacherId/maxSize/related versions →201 detail/operationId | COMMAND; TX07; UUID |
| API-093 | GET /academics/classes/{cId} | AcademicAffairs NORMAL; W14 | ID → AcademicClassDetail/versions/allowedActions | READ; —; không UUID |
| API-094 | PUT /academics/classes/{cId}/capacity | AcademicAffairs NORMAL; W14 | maxSize/expectedVersions.class/expectedSet →200 detail/operationId | COMMAND; W14 atomic; UUID |
| API-095 | GET /academics/class-teacher-options | AcademicAffairs NORMAL; W14 | optional classId → minimal Teacher options | READ; —; không UUID |
| API-096 | POST /academics/classes/{cId}/teacher-change-preview | AcademicAffairs NORMAL; W14 | teacherId → old/new/context/conflicts/versions-set | PREVIEW; —; không UUID |
| API-097 | POST /academics/classes/{cId}/change-teacher | AcademicAffairs NORMAL; W14 | teacherId/expectedVersions/expectedSet →200 detail/operationId | COMMAND; W14 atomic; UUID |
| API-098 | POST /academics/classes/{cId}/transition-preview | AcademicAffairs NORMAL; W14 | action OPEN/START/COMPLETE/CANCEL → prerequisites/effects/versions-set | PREVIEW; —; không UUID |
| API-099 | POST /academics/classes/{cId}/open | AcademicAffairs NORMAL; W14 | expectedVersions/expectedSet →200 OPEN/detail/operationId | COMMAND; W14 atomic; UUID |
| API-100 | POST /academics/classes/{cId}/start | AcademicAffairs NORMAL; W14 | expectedVersions/expectedSet →200 ONGOING/detail/operationId | COMMAND; W14 atomic; UUID |
| API-101 | POST /academics/classes/{cId}/complete | AcademicAffairs NORMAL; W14 | expectedVersions/expectedSet →200 COMPLETED/detail/operationId | COMMAND; W14 atomic; UUID |
| API-102 | POST /academics/classes/{cId}/cancel | AcademicAffairs NORMAL; W14 | reason/expectedVersions/expectedSet →200 CANCELLED/detail/operationId | COMMAND; W14 atomic; UUID |
| API-103 | POST /academics/classes/{cId}/roster/search | AcademicAffairs NORMAL; W14 | name/email/phone/page/sort → AcademicRosterPage | READ; —; không UUID |
| API-104 | POST /academics/placement-candidates/search | AcademicAffairs NORMAL; W15 | name/email/phone/courseId/bandId/page/sort → CandidatePage | READ; —; không UUID |
| API-105 | GET /academics/placement-candidates/{rId} | AcademicAffairs NORMAL; W15 | ID → CandidateDetail/versions | READ; —; không UUID |
| API-106 | GET /academics/placement-candidates/{rId}/classes | AcademicAffairs NORMAL; W15 | page/sort → suitable Class options | READ; —; không UUID |
| API-107 | POST /academics/classes/{cId}/placement-preview | AcademicAffairs NORMAL; W15 | registrationId → candidate/class/conflicts/context/versions-set | PREVIEW; —; không UUID |
| API-108 | POST /academics/classes/{cId}/assignments | AcademicAffairs NORMAL; W15 | registrationId/expectedVersions/expectedSet →201 safe assignment/detail/operationId | COMMAND; TX08; UUID |
| API-109 | GET /academics/teachers/me/classes | Teacher NORMAL, current Class; W14 | status/courseId/page/sort → TeacherClassPage | READ; —; không UUID |
| API-110 | GET /academics/teachers/me/classes/{cId} | Teacher NORMAL, current Class; W14 | ID → TeacherClassDetail | READ; —; không UUID |
| API-111 | POST /academics/teachers/me/classes/{cId}/roster/search | Teacher NORMAL, current Class; W14 | name/email/phone/page/sort → TeacherRosterPage | READ; —; không UUID |
| API-112 | GET /students/me/classes | Student NORMAL, own Class; W14 | current/history/courseId/page/sort → own ClassPage | READ; —; không UUID |
| API-113 | GET /students/me/classes/{cId} | Student NORMAL, own Class; W14 | ID → own ClassDetail | READ; —; không UUID |

### 3.9. Gói4d — Schedule, lịch tuần, thay buổi và HELD

Nguồn contract: bản chính mục12; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-114 | GET /academics/schedules | AcademicAffairs NORMAL; W16 | fromDate/toDate/classId/courseId/teacherId/current-history/page/sort → AcademicCalendarPage | READ; —; không UUID |
| API-115 | GET /academics/schedules/{sId} | AcademicAffairs NORMAL; W16 | ID → AcademicScheduleDetail/allowedActions/versions | READ; —; không UUID |
| API-116 | GET /academics/classes/{cId}/scheduling-context | AcademicAffairs NORMAL; W16 | ID → Class/program/Teacher/expected-current-HELD counts/current obligations/versions-set | READ; —; không UUID |
| API-117 | POST /academics/classes/{cId}/schedule-preview | AcademicAffairs NORMAL; W16 | type/date/start/end/room → candidate/count/conflicts/context | PREVIEW; —; không UUID |
| API-118 | POST /academics/classes/{cId}/schedules | AcademicAffairs NORMAL; W16 | candidate/expectedVersions/expectedSet →201 detail/operationId | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-119 | POST /academics/classes/{cId}/weekly-schedule-preview | AcademicAffairs NORMAL; W16 | from/to weekdays/start/end/room → concrete LESSON plan/count/conflicts/versions-set | PREVIEW; —; không UUID |
| API-120 | POST /academics/classes/{cId}/weekly-schedules | AcademicAffairs NORMAL; W16 | same rule/concrete candidate plan/expectedVersions/expectedSet →201 created refs/context/operationId | COMMAND; TX09; UUID |
| API-121 | POST /academics/schedules/{sId}/move-preview | AcademicAffairs NORMAL; W16 | newDate/start/end/room → old-new/conflicts/versions-set | PREVIEW; —; không UUID |
| API-122 | PUT /academics/schedules/{sId}/time-room | AcademicAffairs NORMAL; W16 | newDate/start/end/room/expectedVersions/expectedSet →200 detail/operationId | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-123 | POST /academics/schedules/{sId}/replacement-preview | AcademicAffairs NORMAL; W16 | reason/newDate/start/end/room → old-new/count/conflicts/context | PREVIEW; —; không UUID |
| API-124 | POST /academics/schedules/{sId}/replace | AcademicAffairs NORMAL; W16 | reason/newDate/start/end/room/expectedVersions/expectedSet →201 old-new safe refs/operationId | COMMAND; TX09; UUID |
| API-125 | POST /academics/schedules/{sId}/held-preview | AcademicAffairs NORMAL; W16 | ID → occurrence/context/prerequisites/versions | PREVIEW; —; không UUID |
| API-126 | POST /academics/schedules/{sId}/confirm-held | AcademicAffairs NORMAL; W16 | expectedVersions/expectedSet →200 HELD/detail/operationId | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-127 | GET /academics/teachers/me/schedules | Teacher NORMAL, current Class; W16 | date window/classId/current-history/page/sort → TeacherCalendarPage | READ; —; không UUID |
| API-128 | GET /academics/teachers/me/schedules/{sId} | Teacher NORMAL, current Class; W16 | ID → TeacherScheduleDetail | READ; —; không UUID |
| API-129 | GET /students/me/schedules | Student NORMAL, own Class; W16 | date window/classId/current-history/page/sort → own CalendarPage | READ; —; không UUID |
| API-130 | GET /students/me/schedules/{sId} | Student NORMAL, own Class; W16 | ID → own ScheduleDetail | READ; —; không UUID |

### 3.10. Gói4e — Điểm danh một LESSON và điểm nháp một học viên

Nguồn contract: bản chính mục13; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-131 | GET /academics/teachers/me/classes/{cId}/attendance | Teacher NORMAL, current Class; W17 | current-history LESSON filter → LessonAttendanceSummary | READ; —; không UUID |
| API-132 | GET /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | Teacher NORMAL, current Class; W17 | IDs → TeacherAttendanceSheet/context | READ; —; không UUID |
| API-133 | PUT /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | Teacher NORMAL, current Class; W17 | changed assignment-present values/expectedVersions/expectedSet →200 saved refs/context/operationId | COMMAND; TX10; UUID |
| API-134 | GET /academics/classes/{cId}/attendance | AcademicAffairs NORMAL, read-only; W17 | current-history LESSON filter → AcademicAttendanceSummary | READ; —; không UUID |
| API-135 | GET /academics/classes/{cId}/lessons/{sId}/attendance | AcademicAffairs NORMAL, read-only; W17 | IDs → AcademicAttendanceSheet | READ; —; không UUID |
| API-136 | GET /academics/teachers/me/classes/{cId}/gradebook | Teacher NORMAL, current Class; W18 | IDs → TeacherGradebook/context | READ; —; không UUID |
| API-137 | GET /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | Teacher NORMAL, current Class; W18 | IDs → TeacherStudentGradeDetail/versions | READ; —; không UUID |
| API-138 | GET /academics/teachers/me/classes/{cId}/components/{componentId}/schedule-options | Teacher NORMAL, current Class; W18 | component → current matching occurrences/window | READ; —; không UUID |
| API-139 | PUT /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | Teacher NORMAL, current Class; W18 | changed component-score-schedule values + optional comment delta/expectedVersions/expectedSet →200 saved detail/context/operationId | COMMAND; TX11; UUID |

### 3.11. Gói4f — Gửi, trả lại, chốt và xem bảng điểm

Nguồn contract: bản chính mục14; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-140 | POST /academics/teachers/me/classes/{id}/grade-submission-preview | Teacher NORMAL, current Class; W18 | action SUBMIT/RESUBMIT → full-cohort preview/missing/context | PREVIEW; —; không UUID |
| API-141 | POST /academics/teachers/me/classes/{id}/submit-grades | Teacher NORMAL, current Class; W18 | expectedVersions/expectedSet → whole-Class submitted refs/operationId | COMMAND; TX12; UUID |
| API-142 | POST /academics/teachers/me/classes/{id}/resubmit-grades | Teacher NORMAL, current Class; W18 | expectedVersions/expectedSet → whole-Class resubmitted refs/operationId | COMMAND; TX12; UUID |
| API-143 | GET /academics/grade-reviews | AcademicAffairs NORMAL; W19 | filters/page → scoped Class review list | READ; —; không UUID |
| API-144 | GET /academics/grade-reviews/classes/{id} | AcademicAffairs NORMAL; W19 | ID → full-cohort review/detail/context | READ; —; không UUID |
| API-145 | POST /academics/grade-reviews/classes/{id}/review-preview | AcademicAffairs NORMAL; W19 | action RETURN/FINALIZE → whole-Class effect/context | PREVIEW; —; không UUID |
| API-146 | POST /academics/grade-reviews/classes/{id}/return | AcademicAffairs NORMAL; W19 | reason/expectedVersions/expectedSet → returned cohort/operationId | COMMAND; TX12; UUID |
| API-147 | POST /academics/grade-reviews/classes/{id}/finalize | AcademicAffairs NORMAL; W19 | expectedVersions/expectedSet → finalized cohort/operationId | COMMAND; TX12; UUID |
| API-148 | GET /students/me/grade-reports | Student NORMAL, own FINALIZED; W19 | filters/page → own FINALIZED report list | READ; —; không UUID |
| API-149 | GET /students/me/grade-reports/{id} | Student NORMAL, own FINALIZED; W19 | ID → own FINALIZED components/weights/scores/comment/final_score/pass | READ; —; không UUID |

### 3.12. Gói5 — Reports và CSV

Nguồn contract: bản chính mục15; các paths lookup được hợp nhất ở mục4.

| ID | Method/path | Scope / W | Request → response logic | Loại / TX / replay |
| --- | --- | --- | --- | --- |
| API-150 | GET /reports/accountant/finance | Accountant NORMAL; W20 | applied financial filters/page → metrics/group page/totals/queried_at | READ; —; không UUID |
| API-151 | GET /reports/director/finance | Director NORMAL, aggregate-only; W20 | applied financial filters/page → group-only finance/totals/queried_at | READ; —; không UUID |
| API-152 | GET /reports/director/admissions | Director NORMAL, aggregate-only; W20 | created-period/course/state/page → counts/distinct/group totals/queried_at | READ; —; không UUID |
| API-153 | GET /reports/director/training | Director NORMAL, aggregate-only; W20 | inventory filters + finalized-results period/page → separated inventory/results | READ; —; không UUID |
| API-154 | GET /reports/accountant/finance/csv | Accountant NORMAL; W20 | applied financial filters → fresh all-results CSV | READ; —; không UUID |

## 4. Caller-result và shared-path allowlists

Không dùng union role của một path như quyền gọi mọi action trong module.
Service phải kiểm cặp **verified caller + action**, ownership/current assignment
và quyền projection ở cả initial request, lookup và completed replay. Các
source guards khác nhau vẫn giữ độc lập; không OR chúng thành quyền rộng.

| Path / ID | Caller/action được nối từ các gói đã trình | Giới hạn |
| --- | --- | --- |
| /admin/command-results/{requestId} / API-014 | Admin: staff create/reset/lock/unlock/disable/restore (mục5). | Chỉ marker caller; staff create thuộc W01, state/reset W03; không replay temporary password. |
| /command-results/{requestId} / API-037 | Student: create/cancel Registration, book/cancel/reschedule Booking, confirm (mục7). | Own target/action; không marker của Student khác hoặc credential/profile actions. |
| /admissions/command-results/{requestId} / API-045 | Admissions: approve/reject/cancel Registration (mục8); AcademicAffairs:6 Session commands (mục9). | Role Admissions không được Session commands; AcademicAffairs không được admission approval. |
| /finance/command-results/{requestId} / API-053 | Accountant: collect và payment metadata correction (mục8). | Recheck finance quyền/target; không mở audit/Admin/report dump. |
| /admissions/teachers/me/command-results/{requestId} / API-073 | Teacher: initial entry score/absence/correction (mục9). | Current/original grader windows và target scope hiện tại. |
| /academics/command-results/{requestId} / API-090 | AcademicAffairs:10 config +8 Class/placement +5 Schedule +2 return/finalize; Teacher:2 attendance/draft +2 submit/resubmit (mục10–14). | AcademicAffairs không đọc Teacher DRAFT hoặc save hộ; Teacher không config/placement/schedule/reviewer actions. |

Đây là action families đã trình, chưa là exact string action-code registry.
Trước đóng E04 phải có mapping method/path→stable action→canonical DTO→T23
completion/ref/mode và failure outcomes; không suy các tên đó từ API ID.

## 5. Frontend route/feature manifest — chi tiết URL dự thảo

URL dưới đây là **đề xuất chi tiết để rà bản E04**, không thay menu/quyền
FE01–FE19 hoặc tạo F mới. `:id` là parameter của frontend, không là cơ chế
ủy quyền. List/detail/complex form có URL riêng; confirmation/reason/short
forms dùng pattern đã duyệt. Không chọn/cài thêm router/cache/form library.

Query chỉ giữ non-identifying Course/Band/state/date/page/sort theo ADR-105.
Input tên/email/SĐT, grade drafts, CSRF/JWT/secret không vào URL/storage.
Direct link và Back/Forward tải lại authorized projection, không resurrect cache
của phiên cũ; nguồn tải lỗi không dùng form/cookie context cũ như mới.

| Mã state | Hành vi áp dụng |
| --- | --- |
| S1 — Read/list | idle/loading/success/empty(no data)/empty(no match)/error rõ; page/sort ổn định; discard out-of-order/scope-old response. Skeleton lần đầu; lỗi quan trọng giữ đủ để xử lý. |
| S2 — Form | draft RAM + tokens/context của lần load; explicit save;400 field errors/summary,409 yêu cầu reload/review không merge; rời còn edits cảnh báo. Không autosave hoặc cắt/round sai input. |
| S3 — Preview | input changed→invalidate; read-only/full effects/context, confirmation một lần với đúng action/reason; pending chặn click lặp. Preview không reserve; command rechecks mọi guards. |
| S4 — Command intent | UUID một intent; pending→success/refetch authorized affected data; mạng/timeout→unknown/lookup, không auto retry. Intent resolved rồi sửa content mới là intent mới; absence không chắc rollback. |
| S5 — Credential result | First-response only/RAM; explicit copy/ack; rời/logout/expiry clear; không replay hoặc hứa xóa clipboard. Lost first response kiểm trạng thái, không auto reset. |
| S6 — Report/print | Draft filters khác applied filters; queried_at của server/totals toàn tập; export/print query fresh, lỗi không trình data cũ như kết quả mới. Director không export; Student print own Invoice. |
| S7 — Auth/session | Scope NORMAL vs PASSWORD_CHANGE;401 clear sensitive state→login;403 CSRF_INVALID refresh context/token không replay write; other tabs signal no secrets; backend quyết quyền. |
| S8 — Academic save units | Attendance delta một LESSON; draft scores/comment một Student; missing≠Vắng/0, omitted giữ nguyên/blank không delete stored score. Unsaved edits xử lý trước whole-Class preview; server không thấy draft browser khác. |

| F / actor nguồn E01 | URL frontend dự thảo | API sử dụng | State |
| --- | --- | --- | --- |
| F01 / Trước login; 7 role | /login | API-001–005 | S7 |
| F02 / Tự đăng ký Student | /register | API-018 | S2,S7 |
| F03 / Student | /student | API-021; links tới API own theo từng card | S1 |
| F04 / Student | /student/courses; /student/courses/:courseId | API-022–023 | S1 |
| F05 / Student | /student/registrations; /student/registrations/new; /student/registrations/:registrationId | API-024–028,037 | S1,S2,S3,S4 |
| F06 / Student | /student/registrations/:registrationId/entry-test | API-029–032,037 | S1,S3,S4 |
| F07 / Student | /student/entry-tests/calendar | API-033; Registration context API-026 | S1 |
| F08 / Student | /student/registrations/:registrationId/entry-results | API-034 | S1 |
| F09 / Student | /student/registrations/:registrationId/confirmation | API-035–037 | S1,S3,S4 |
| F10 / Student | /student/invoices; /student/invoices/:invoiceId; /student/invoices/:invoiceId/print | API-046–047; print dùng lại GET authorized fresh | S1,S6 |
| F11 / Student | /student/classes; /student/classes/:classId; /student/calendar | API-112–113,129–130 | S1 |
| F12 / Student | /student/grades; /student/grades/:gradeReportId | API-148–149 | S1 |
| F13 / Admissions | /admissions/registrations | API-038 | S1 |
| F14 / Admissions | /admissions/registrations/:registrationId | API-039–045 | S1,S3,S4 |
| F16 / AcademicAffairs | /academic-affairs/entry-tests | API-054 | S1 |
| F17 / AcademicAffairs | /academic-affairs/entry-tests/new; /academic-affairs/entry-tests/:sessionId; /academic-affairs/entry-tests/:sessionId/edit | API-055–065; role/action-safe lookup API-045 | S1,S2,S3,S4 |
| F18 / AcademicAffairs | /academic-affairs/courses; /academic-affairs/courses/new; /academic-affairs/courses/:courseId; /academic-affairs/programs; /academic-affairs/programs/new; /academic-affairs/programs/:programId; /academic-affairs/programs/:programId/configuration | API-074–090 | S1,S2,S3,S4 |
| F19 / AcademicAffairs | /academic-affairs/classes | API-091 | S1 |
| F20 / AcademicAffairs | /academic-affairs/classes/new; /academic-affairs/classes/:classId; /academic-affairs/classes/:classId/edit | API-092–103; read-only Attendance API-134–135; lookup API-090 | S1,S2,S3,S4 |
| F21 / AcademicAffairs | /academic-affairs/calendar; /academic-affairs/classes/:classId/schedules/new; /academic-affairs/classes/:classId/schedules/weekly; /academic-affairs/schedules/:scheduleId; /academic-affairs/schedules/:scheduleId/edit; /academic-affairs/schedules/:scheduleId/replace | API-114–126; lookup API-090 | S1,S2,S3,S4 |
| F22 / AcademicAffairs | /academic-affairs/placement; /academic-affairs/placement/:registrationId | API-104–108; lookup API-090 | S1,S3,S4 |
| F23 / AcademicAffairs | /academic-affairs/grade-reviews; /academic-affairs/grade-reviews/:classId | API-143–147; lookup API-090 | S1,S3,S4 |
| F24 / Teacher | /teacher/classes; /teacher/classes/:classId; /teacher/calendar | API-109–111,127–128 | S1 |
| F25 / Teacher | /teacher/entry-tests; /teacher/entry-tests/:sessionId; /teacher/entry-tests/bookings/:bookingId | API-066–073 | S1,S2,S4 |
| F26 / Teacher | /teacher/classes/:classId/attendance; /teacher/classes/:classId/lessons/:scheduleId/attendance | API-131–133; lookup API-090 | S1,S2,S4,S8 |
| F27 / Teacher | /teacher/classes/:classId/gradebook; /teacher/classes/:classId/assignments/:assignmentId/grades | API-136–142; lookup API-090 | S1,S2,S3,S4,S8 |
| F28 / Accountant | /accountant/invoices; /accountant/invoices/:invoiceId | API-048–049 | S1 |
| F29 / Accountant | /accountant/invoices/:invoiceId/collect; /accountant/payments/:paymentId/edit | API-050–053 | S1,S2,S3,S4 |
| F30 / Accountant | /accountant/reports/finance | API-150,154 | S1,S6 |
| F31 / Director | /director/reports/admissions; /director/reports/training; /director/reports/finance | API-151–153 | S1,S6 |
| F32 / Admin | /admin/accounts; /admin/accounts/:accountId; /admin/staff-accounts/new | API-006–014 | S1,S2,S4,S5 |
| F33 / Admin | /admin/audit; /admin/audit/:eventId; /admin/operations/:operationId/audit | API-015–017 | S1 |

Menu tài khoản không có F/NV mới: `/account/change-password` dùng API-004
cho7 role NORMAL hoặc PASSWORD_CHANGE đúng scope; `/student/profile` dùng
API-019–020 chỉ Student NORMAL; logout là action API-005, không cần screen mới.
S7 áp dụng xuyên mọi protected routes dù bảng chỉ ghi profile state chính.
Phiên restricted chỉ Auth context/password/logout, không mount/cache business
screens. Student-mobile và staff-desktop vẫn kiểm375/768/1280 +keyboard/focus/
labels/non-color; desktop sổ điểm và mobile Student page cùng data/save unit.

## 6. Trace W → NV/F → API/service → E03 → kiểm chứng

NV/F từ WORKFLOW_SPEC mục10; tables/SQL từ DATABASE_SPEC mục12.2.
Một API có thể nối nhiều W; một NV có thể được đọc/ghi ở nhiều API. Mục này
không phải E05 đã nghiệm thu. Cross-cutting W21/audit và security-denial checks
áp dụng những paths có nghĩa vụ theo contract, không chỉ ba audit read APIs.

| W / NV / F | API entry points | Owner service / dữ liệu & SQL nguồn E03 | Case thiết kế |
| --- | --- | --- | --- |
| W01; NV01, NV33; F02, F32 | API-008, API-014, API-018 | Administration; Administration/caller-result; Students; T01–T03/T22/T23, TX01/unique email-phone. | VC-W01 |
| W02; NV01; thao tác chung đổi mật khẩu/logout; F01 | API-001–005 | Administration/Auth; T02/T04/T22, TX13/session-throttle/version; không replay credential input. | VC-W02 |
| W03; NV33–NV34; F32 | API-006–007, API-009–014 | Administration; Administration/caller-result; T02/T04/T22/T23 safe marker, TX13/ADMINSET. | VC-W03 |
| W04; NV02–NV08 (projection), thao tác chung profile/F03; F03 | API-019–021 | Students; T02/T03 và projection T09/T20/T15/T16/T19, ownership/no snapshots. | VC-W04 |
| W05; NV02; F04–F05 | API-022–028, API-037 | Students read / Academics data; Admissions; Admissions/caller-result; T05/T09/T11–T13/T20/T22/T23, FN03/TX06/unfinished unique. | VC-W05 |
| W06; NV14–NV15, NV21; F16–F17, F25 | API-045, API-054–068 | Admissions/caller-result; Admissions; T10/T12/T22/T23, FN01/TIMETABLE/session guard. | VC-W06 |
| W07; NV03; F06–F07 | API-029–033, API-037 | Admissions; Admissions/caller-result; T09–T12/T15/T16/T22/T23, TX02/current-booking/capacity. | VC-W07 |
| W08; NV04, NV15, NV22, NV36; F08, F16–F17, F25 | API-034, API-069–073 | Admissions; Admissions/caller-result; T06/T09/T11–T13/T22/T23, FN02/TG03/TX03/BANDSET. | VC-W08 |
| W09; NV05; F09 | API-035–037 | Admissions; Admissions/caller-result; T09/T11–T13/T22/T23, REG confirmation/result guards. | VC-W09 |
| W10; NV09, NV12, NV37; F13–F14 | API-038–045 | Admissions / Finance effects; Admissions/caller-result; T09/T20–T24, SP07/TX04–TX06. | VC-W10 |
| W11; NV06, NV27, NV37; F10, F28 | API-046–049 | Finance; T05/T09/T20/T24, FN06/SP07/Invoice snapshot. | VC-W11 |
| W12; NV28; F29 | API-050–053 | Finance; Finance/caller-result; T09/T20–T23, TX05/metadata lock and history. | VC-W12 |
| W13; NV13; F18 | API-074–090 | Academics/configuration; Academics/caller-result; T05–T08/T14/T22/T23, TG01–TG03/config gates. | VC-W13 |
| W14; NV16–NV17, NV23; F19–F20, F24 | API-090–103, API-109–113 | Academics/caller-result; Academics; T07/T14–T19/T22/T23, FN04/Class state/cohort lock. | VC-W14 |
| W15; NV18; F22 | API-090, API-104–108 | Academics/caller-result; Academics; T07/T09/T13–T16/T19–T23, TX08/assignment unique/capacity. | VC-W15 |
| W16; NV07, NV19, NV23; F11, F21, F24 | API-090, API-114–130 | Academics/caller-result; Academics/Schedule; T10/T12/T14–T18/T22/T23, TX09/TIMETABLE/current obligation. | VC-W16 |
| W17; NV24; F26 | API-090, API-131–135 | Academics/caller-result; Academics; T14–T17/T22/T23 theo contract, TG04/TX10. | VC-W17 |
| W18; NV25–NV26, NV38; F27 | API-090, API-136–142 | Academics/caller-result; Academics; Academics/grades; T07/T08/T14–T19/T22/T23, FN05/TG05/TX11–TX12. | VC-W18 |
| W19; NV08, NV20, NV38; F12, F23 | API-090, API-143–149 | Academics/caller-result; Academics/grades; T09/T14–T19/T22/T23, FN03/FN05/SP03/TX12/grouped history. | VC-W19 |
| W20; NV29–NV32; F30–F31 | API-150–154 | Reports; VW01/VW03–VW06/FN04–FN06/SP04–SP06, one SNAPSHOT/report. | VC-W20 |
| W21; NV35, NV39; audit áp mọi mutation phù hợp; F33 | API-015–017; audit/denial xuyên các paths có nghĩa vụ | Administration; T22/T23 plus entity event bundles, mandatory-vs-security audit retention. | VC-W21 |

Không có endpoint “system Band/invoice/grade/audit” cho NV36–NV39; chúng là
effects/read projection của actor workflow có quyền, không menu/action mới.
T01–T24 đều có source obligation trong E03 W trace; xuất hiện trong bảng
không cấp SELECT/DML toàn bảng cho role hoặc chứng minh field coverage đầy đủ.

### 6.1. SQL caller obligations — logical bindings, chưa executable signatures

| Object / API callers dự kiến | Binding và scope bắt buộc | Bằng chứng sau triển khai |
| --- | --- | --- |
| SP01 usp_SearchAdmissions / API-038 | Trusted Admissions context; normalized body filter/page/sort; VW01; chỉ operational allowlist, không Payment/DOB/address/grades. | App call trace + role/projection/filter/paging fixtures; không chỉ SSMS demo. |
| SP02 usp_GetEntryTestRoster / API-065,068 | Trusted AcademicAffairs hoặc current Teacher; session_id/as_of_utc từ request validated/server; VW02/FN01; scoped current/history bookings. | Current-grader revoke/reassignment/ownership tests; parameterization; API fields đúng role. |
| SP03 usp_GetClassGradeReview / API-144 | Trusted AcademicAffairs/class_id; không DRAFT values; FN05; review read không quyền nhập Score. | DRAFT leak denied; SUBMITTED/RETURNED/FINALIZED detail/decimal totals parity. |
| SP04 usp_GetFinancialReport / API-150,151,154 | Issue/confirmation periods khác current debt, Course/overdue/today_vn; one SNAPSHOT cho whole result; Accountant/Director projection khác. | Report/CSV fresh query, concurrent Payment snapshot, no row/identity dump; SUM precision. |
| SP05 usp_GetAdmissionsReport / API-152 | Created-period/Course/state; VW01; Registration count/distinct Student và overall distinct riêng. | Multi-course/repeat enrollment fixtures; không cộng distinct groups thành overall. |
| SP06 usp_GetTrainingReport / API-153 | Date-free current inventory + finalization-period results; VW04/VW05/FN04; current Teacher filters. | Replacement single-count, FINAL_EXAM riêng, no-data vs0%, FINALIZED cohort. |
| SP07 usp_AllocateInvoiceNumber / API-041 | issued_at VN→issue_year do server; caller TX04/shared connection; T24 lock/range/overflow; không commit riêng. | Forced allocator/audit failure rollback admission+Invoice+counter+marker; replay không number thứ hai. |
| FN01 ufn_EffectiveEntryTestState / session reads + SP02 | Server as_of_utc một lần/request; read không persist CLOSED; next allowed write mới persist. | Start-boundary parity với Service/options; cancelled giữ cancelled. |
| FN02 ufn_BandForScore / API-070,072 | Validated score/current threshold set trong TX03/BANDSET; best result/tie policy server-owned. | Range/boundary/tie/first-use race parity; không hardcode Band seed. |
| FN03 ufn_RegistrationUnfinished / W05,W19 (API-025,147) | Predicate gốc để kiểm/đối chiếu is_unfinished; không dependence passing; cập nhật cùng owner TX. | Concurrent create vs finalize/cancel; no COMPLETED Registration enum mới. |
| FN04 ufn_ClassObligationProgress / Class context + SP06 | Expected/current/HELD LESSON và FINAL_EXAM riêng; related current projection/history đúng. | W14/W16/SP06 same obligation count, no double replacement count. |
| FN05 ufn_GradeTotal / API-136–137,140, SP03 và finalize | Required/entered/is_complete; total NULL nếu thiếu; decimal/half-up một lần; Service parity, final_score chỉ publish khi FINALIZED. | Same fixtures incomplete/zero/49.995/weights; no join-multiplication, Score-parent RV touch. |
| FN06 ufn_InvoiceDebtFlags / own/finance Invoice reads + SP04 | status/due_date/today_vn; chỉ UNPAID due<today overdue; không penalty/status mutation. | Midnight-VN, due-date equality, paid/cancelled fixtures parity. |

API callers ở đây là đường **dự kiến** trong thiết kế, không app trace đã thu.
Views/TVFs không API/public resources hoặc phép grant SELECT facts cho Director.
TG01–05 vẫn theo E03: TrainingProgram, ScoreComponentConfig, Band, Attendance,
Score mapping phải tương thích trigger và verified generated ID/rowversion;
chưa chọn/cấu hình EF write strategy bằng việc thêm bảng này. Signature/result
columns/types/keyless mappings/runtime grants cụ thể còn ở gap SC-03.

## 7. Verification contracts — chưa thực thi

Áp dụng VC-SCOPE theo scope từng entry trong154 entries: protected request sai
role403, invalid/revoked session401, missing/out-of-scope target404; public
Auth/signup/CSRF giữ scope riêng đã ghi, không ép authenticated precondition.
Phiên restricted chặn business; current ownership
ở list/detail/preview/command/replay/export. F01 login lỗi trước xác thực vẫn chung;
không lấy lỗi quyền/traceId làm kênh lộ dữ liệu. VC-UI cho32 F +account menu:375/
768/1280, Chrome/Edge mục tiêu, keyboard/focus/labels, Back/Forward/deep link,
RAM/URL privacy và request ordering/session teardown. Đây là checklist thiết kế.

| Case | Điều phải chứng minh sau triển khai |
| --- | --- |
| VC-W01 | Unique email/phone race; lỗi sau Account rollback Profile/staff state/audit; staff result chỉ hiển thị secret lần đầu, Student signup không tự login. |
| VC-W02 | Mốc tuyệt đối/idle/temp; revoked session và CSRF đổi identity; login errors chung/429; logout không phục hồi phiên khi log lỗi; không replay credential. |
| VC-W03 | Self/last ACTIVE Admin; reset giữ LOCKED/DISABLED; revoke toàn phiên; mandatory-audit failure rollback; lost credential response không tự reset. |
| VC-W04 | Student khác không xem/sửa được; thiếu/stale một trong2 tokens không lưu nửa form; đổi liên hệ không revoke; home không unpublished grade/peer data. |
| VC-W05 | Hai tạo Registration tranh unfinished predicate; Course ACTIVE tại create; hủy đúng future set kể cả CLOSED, giữ started history; không sửa Course đã chọn. |
| VC-W06 | OPEN/CLOSED/start boundary; booking xuất hiện sau preview; đổi grader trước start/no result; blocked assignment không tự đổi người; no partial Session cancellation. |
| VC-W07 | Hai Student tranh chỗ cuối; same-attempt booking race; CLOSED chặn Student đổi/hủy; reschedule old-new cùng commit; rebook không thành attempt3. |
| VC-W08 | Đúng original/current Teacher; end/confirmation/cancel boundary; correction reason; best ties attempt1; absence không score0; FN02/config-freeze/result-audit atomic. |
| VC-W09 | Pending retake hoặc changed results/set chặn confirm; concurrent grading/confirm không bypass; client Band/state không quyết định kết quả. |
| VC-W10 | Approve vs cancel/reject; Course price thay sau preview; forced allocator/audit failure rollback ADMITTED/Invoice/counter; UUID replay không cấp Invoice thứ hai. |
| VC-W11 | Student own Invoice vs Accountant detail; current PAID/UNPAID/CANCELLED và due ngày VN; print query mới lỗi không in bản cũ như mới; không PDF mới. |
| VC-W12 | Payment vs cancellation; tiền khớp whole VND/one Payment; metadata giữ amount/paidAt/link; audit failure không PAID partial; no refund. |
| VC-W13 | DRAFT chưa dùng vs freeze dù Class CANCELLED; config whole-set stale rollback; activation đúng100/one exam; rename không mở component edits; TG multi-row. |
| VC-W14 | First-use freeze/config race; teacher/capacity/cohort/schedule change vs start/complete/cancel; completion không cần grade publication và không tự HELD. |
| VC-W15 | Hai placement tranh chỗ cuối; payment/capacity/time-conflict thay sau preview; Assignment + DRAFT GradeReport atomic; không peer/DRAFT leak hoặc transfer. |
| VC-W16 | Weekly một invalid rollback toàn batch; [start,end) boundary; replace one obligation/no fork/no dependent data; HELD explicit sau end; no count đôi/history loss. |
| VC-W17 | Delta một LESSON/correct Teacher/start; missing khác Vắng; new-row absence race; stale một row rollback toàn unit; Class completion khóa entry. |
| VC-W18 | Một Student scores/comment atomic; component/schedule/start đúng; child touch parent RV; missing total=NULL; whole-Class submit không page subset/no unsaved auto-send. |
| VC-W19 | Whole cohort return/finalize atomic; reason RETURN; 49.995→50.00 vs49.994→49.99; final_score/is_unfinished/audit/replay cùng TX12; Student chỉ FINALIZED. |
| VC-W20 | Một SNAPSHOT cho groups/count/totals/page; distinct overall riêng; debt khác period revenue; no-data≠0%; fresh CSV toàn tập/cap/formula-safety/no Director export. |
| VC-W21 | Allowlists/no secrets/full payload; mandatory mutation audit fail rollback; security best-effort không bypass auth; IP/security retention đúng; Admin read không app purge/export. |

VC-REPLAY cho55 commands: same key/content chỉ một effects+audit bundle;
different normalized intent409; stale/current rights recheck không lộ data;
secret first result không lookup/replay; unknown/no marker không auto retry.
Forced failure/races chạy SQL Server test riêng, hai connections, dữ liệu giả;
assert Service/SP/UDF/trigger/TX integration và rubric, không test DB chính.
Health/performance/backup targets cần môi trường/authority và bằng chứng riêng.
Không thêm framework hoặc viết/chạy app/SQL/test scripts ở lượt thiết kế này.

## 8. Checkpoint review-ready trước nghiệm thu E04-v1

SC01–04 documentation complete for review, không Accepted/runtime proof.
[Final contract](E04_FINAL_CONTRACT.md) là kết luận actual package, precedence,
wire/result/read authorization và deferred DF01–05. Không re-vote RF01–04.

| ID / trạng thái | Nội dung đã hoàn thiện | Boundary |
| --- | --- | --- |
| SC-01 / review-ready |[E04_PARITY_REVIEW](E04_PARITY_REVIEW.md):154 route/schema/action trace, rà theo miền/findings;final contract giải quyết ambiguities còn lại.|Coverage không proof mọi field guard được implement; owner actual E04 review còn mở.|
| SC-02 / review-ready |[E04_REPLAY_SPEC](E04_REPLAY_SPEC.md):55actions/bindings,6lookups/C1bytes/SHA256/refs/outcomes/no-op/compatibility;final exact parsing/error/expiry/result unions.|T23/SHA256/save units không đổi;exact technical choices chờ E04 acceptance.|
| SC-03 / review-ready |[E04_SQL_INTEGRATION_SPEC](E04_SQL_INTEGRATION_SPEC.md):7SP/6FN/result sets/EF5trigger/grants24tables/gates55commands/read isolation/audit.|Không DDL/code/grant thực tế;SQL/runtime evidence future.|
| SC-04 / review-ready +DF01–05 |[E04_RUNTIME_PROFILE](E04_RUNTIME_PROFILE.md):typed slots/startup/readiness/cancel/runbook/prerequisites.|D12/D13/environment values và evidence giữ deferred;missing profile fail closed,no guessed deadlines/provider/path.|

32 frontend URLs/state patterns mục5 nằm trong actual E04-v1 review, không
library installation. Trace21W/37NV/32F mục6 nối [E05-v1](E05_BASELINE_REVIEW.md)
đã chuẩn bị theo yêu cầu owner hoàn thiện cả hai output; E05 nghiệm thu sau
E04, rerà nếu E04 sửa. OWNER_REVIEW giữ hai cổng riêng chưa tích.
Không business question mới được phát hiện; review actual contracts rồi
nghiệm thu theo thứ tự, không implementation planning/code/SQL/Git/deploy.

## 9. Nhật ký tài liệu

- 03/10/2026: E04-v1 documentation review-ready; final contract/E05-v1 cross-check prepared, current gaps/status synced. E04/E05 unchecked, D12/D13 deferred, no code/SQL/tests/planning.

- 03/10/2026 (RF approval): RF01→ADR-157/RF02→158/RF04→159 Accepted;
  requirements/NV/W/DTO/no-op registry/current memory synced. Technical codecs/
  signatures/EF/ref unions/E04/E05 still review pending; E03/counts/owner gates
  preserved; no code/SQL/runtime tests.


-03/10/2026 (SC02–04-v1): Added3 contract appendices +SC01 consolidated parity
  review;55canonical actions/6lookup pairs/C1bytes+hash/SP-FN signatures/EF/grants/
  locks/audit/runtime slots explicit. New RF04 no-op proposed,RF01/02 pending,
  RF03 Accepted156. Static doc checks only;E04/E05 unchecked,no code/SQL/tests.


- 03/10/2026 (D10): Đồng bộ154/154 DTO drafts API-001–154, thêm5 Reports/CSV.
  Role-scoped snapshot/time/filters/whole-set totals/decimal38/distinct/LESSON/
  FINALIZED no-data/fresh bounded export và2 JSON/1 CSV illustrations rõ.
  Không còn API thiếu schema miền; SC-01 parity review/SC-02–04 vẫn mở.
  Report options/CSV columns-footer/bytes/header/error là draft, không mới ADR.
  Không đổi154 inventory rows/55 UUID/6 lookup paths/owner marks/nguồn.
  Next SC-02 canonical action/digest/outcomes; no code/SQL/runtime tests.

- 03/10/2026 (D9): Đồng bộ149 DTO drafts API-001–149; thêm10 whole-Class
  submission/review/Student-own FINALIZED,5 Reports schemas còn lại.
  Full cohort/related Registration guards/publication-history-unfinished TX/
  decimal results/role projections/examples rõ. RF03 accepted ADR-156, RF01/02
  open; nguồn/owner marks không đổi. Không đổi154 API patterns/55 UUID/
  6 lookup paths; next Reports/CSV API-150–154.

- 03/10/2026 (D8): Đồng bộ139 DTO drafts API-001–139; thêm5 Attendance/
  read-only Academic và4 Teacher gradebook/detail/selector/draft-save entries,
  15 schemas còn lại. Full cohort/delta/absence guards, score-comment TX/report
  RV touch, configured saved totals/missing semantics/role projections/examples
  rõ. RF03 accepted ADR-156, RF01/02 open; nguồn/owner marks không đổi.
  Không đổi154 API patterns/55 UUID/6 lookup paths; next whole-Class grades140–149.

- 03/10/2026 (RF03): Owner duyệt riêng cutoff root/weekly/move-destination
  startAt>serverNow lúc lưu, ADR-156. Đồng bộ requirements/NV19/W16/E04;
  RF01/02 và các wire schemas khác vẫn proposed, không E04/E05 acceptance.
  Không đổi154 API patterns/55 UUID/6 lookup paths hoặc130/24 schema progress.

- 03/10/2026 (D7): Đồng bộ130 DTO drafts API-001–130; thêm17 Schedule/
  scoped-calendar entries,24 schemas còn lại. Whole weekly plan/full source/
  resource-cohort contexts/move/replacement/HELD/three role/chain projections
  và examples rõ. RF03 root-create/move-target clock proposed, RF01/02 open.
  Không đổi154 API patterns/55 UUID/6 lookup paths; next Attendance/draft131–139.

- 03/10/2026 (D6): Đồng bộ113 DTO drafts API-001–113; thêm23 Class/placement/
  scoped Teacher-Student reads,41 schemas còn lại. Full immutable cohort/Schedule/
  Attendance guards, explicit lifecycle/Teacher change/atomic placement/DRAFT
  privacy/role projections/examples rõ; E04-RF01/02 chưa Accepted. Không đổi
  154 API patterns/55 UUID/6 lookup paths; next Schedule/scoped calendars114–130.

- 02/10/2026 (D5): Đồng bộ90 DTO drafts API-001–090; thêm17 Course/Band/
  Program/shared-result,64 schemas còn lại. Whole source/target component sets/
  relation tokens, activation/freeze/rename-lifecycle/role-action lookup/examples
  rõ; E04-RF01/02 vẫn proposed. Không đổi154 API patterns/55 UUID/6 lookup paths.

- 02/10/2026 (D4): Đồng bộ73 DTO drafts API-001–073; thêm12 AcademicAffairs
  Session/8 Teacher Session-task-grading,81 schemas còn lại. Full membership/
  role-task scopes/score-absence/provenance/recovery và examples viết rõ;
  E04-RF01/02 creation-time/no-op proposed. Không đổi154 API patterns/55 UUID.

- 02/10/2026 (D3): Đồng bộ53 DTO drafts API-001–053; thêm8 Admissions/2 own
  Student Invoice/6 Accountant Invoice-Payment,101 schemas còn lại. Đặc tả
  role allowlists/absence manifests/immutable money-time/fresh print/recovery;
  chỉ structural checks, không Accepted refinements hoặc đổi154 API patterns.

- 02/10/2026 (D2): Đồng bộ SC-01 từ37 DTO drafts API-001–037; bổ sung12 Admin/
  audit,1 Home và14 Student Registration/booking/confirmation.117 schemas còn lại;
  counts là tài liệu, không acceptance/runtime proof, không đổi154 API patterns.

- 02/10/2026: Tạo E04-C1 từ contract đã trình/nguồn E01–E03; normalized154
  method/path,55 UUID commands,6 lookup paths giữ role/action scopes;32 frontend
  F route proposals,21 W→37 NV/32 F→API/service/data/SQL/cases trace. Ghi4 gaps
  SC-01–04 minh bạch. Chỉ kiểm cấu trúc tài liệu; chưa code/SQL/tests hoặc nghiệm thu.
