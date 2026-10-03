# E04 — Canonical actions, digest bytes và completion/replay

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản: E04-SC02-v1 trong E04-v1, 03/10/2026. **Accepted ADR-160; chưa runtime implementation.**
Nguồn: [E03 T23](DATABASE_SPEC.md#t23--replayoperation-bảng-kỹ-thuật),
[registry](E04_CONTRACT_CATALOG.md), [154 DTO](E04_DTO_SPEC.md),
[quyết định](../DECISIONS.md). Không thay schema T23, thuật toán SHA-256,
55 commands/6 lookups, quyền hoặc save units đã duyệt.

Final refinements: [E04-v1 mục3–5](E04_FINAL_CONTRACT.md) quy định strict
UUID/RV, error registry/precedence, replay discriminator, exact24h Accepted ADR-160,
expired retained row409/no reuse và live read scope; thay checkpoint ambiguities.

## 1. Registry và cách đọc

action_code dưới đây là ASCII ổn định, phân biệt hoa thường, dài dưới80 bytes.
Selector `action` của lookup là **chính chuỗi này**, không alias/API-ID/capability.
Controller chọn action từ method/path, actor lấy từ verified session; body
không nhận action/actor. All paths dưới /api/v1. DTO-xxx nghĩa toàn request
allowlist + types/required/NULL/manifest của API-xxx trong D10, không riêng
summary input trong catalog. Body `expectedVersions`/`expectedSet` đều vào
canonical input khi schema yêu cầu, gồm full source sets và expected absence.
W/TX kế thừa từng dòng registry; không invent TX14 hoặc gán TX theo module.

Registry ghép chính xác method/path/role/W/TX từ catalog và schema D10.
T = safe completion.target; C/S/P/G/K = nhóm no-op mục5. `*` ở target nghĩa
optional chỉ khi reference thực sự đã commit; không null hoặc invented ID.
Secret mode chỉ API-008/009; mọi dòng còn lại SAFE_REFERENCE.

| API / DTO | Canonical action_code | Target đầu vào (route) | T kết quả committed | Lookup | No-op |
| --- | --- | --- | --- | --- | --- |
| API-008 / DTO-008 | ADMIN.STAFF.CREATE | {} | accountId | API-014 | C |
| API-009 / DTO-009 | ADMIN.ACCOUNT.RESET_PASSWORD | accountId | accountId | API-014 | K |
| API-010 / DTO-010 | ADMIN.ACCOUNT.LOCK | accountId | accountId | API-014 | S |
| API-011 / DTO-011 | ADMIN.ACCOUNT.UNLOCK | accountId | accountId | API-014 | S |
| API-012 / DTO-012 | ADMIN.ACCOUNT.DISABLE | accountId | accountId | API-014 | S |
| API-013 / DTO-013 | ADMIN.ACCOUNT.RESTORE | accountId | accountId | API-014 | S |
| API-025 / DTO-025 | STUDENT.REGISTRATION.CREATE | {} | registrationId | API-037 | C |
| API-028 / DTO-028 | STUDENT.REGISTRATION.CANCEL | registrationId | registrationId | API-037 | S |
| API-030 / DTO-030 | STUDENT.ENTRY_BOOKING.CREATE | registrationId | registrationId,bookingId | API-037 | C |
| API-031 / DTO-031 | STUDENT.ENTRY_BOOKING.CANCEL | registrationId,bookingId | registrationId,bookingId | API-037 | S |
| API-032 / DTO-032 | STUDENT.ENTRY_BOOKING.RESCHEDULE | registrationId,bookingId | registrationId,oldBookingId,newBookingId | API-037 | P |
| API-036 / DTO-036 | STUDENT.REGISTRATION.CONFIRM | registrationId | registrationId | API-037 | S |
| API-041 / DTO-041 | ADMISSIONS.REGISTRATION.APPROVE | registrationId | registrationId,invoiceId | API-045 | S |
| API-042 / DTO-042 | ADMISSIONS.REGISTRATION.REJECT | registrationId | registrationId | API-045 | S |
| API-044 / DTO-044 | ADMISSIONS.REGISTRATION.CANCEL | registrationId | registrationId,invoiceId* | API-045 | S |
| API-051 / DTO-051 | FINANCE.INVOICE.COLLECT | invoiceId | invoiceId,paymentId | API-053 | C |
| API-052 / DTO-052 | FINANCE.PAYMENT.UPDATE_METADATA | paymentId | invoiceId,paymentId | API-053 | P |
| API-055 / DTO-055 | ACADEMIC.ENTRY_SESSION.CREATE | {} | sessionId | API-045 | C |
| API-057 / DTO-057 | ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE | sessionId | sessionId | API-045 | P |
| API-058 / DTO-058 | ACADEMIC.ENTRY_SESSION.OPEN | sessionId | sessionId | API-045 | S |
| API-059 / DTO-059 | ACADEMIC.ENTRY_SESSION.CLOSE | sessionId | sessionId | API-045 | S |
| API-061 / DTO-061 | ACADEMIC.ENTRY_SESSION.CANCEL | sessionId | sessionId | API-045 | S |
| API-064 / DTO-064 | ACADEMIC.ENTRY_SESSION.CHANGE_GRADER | sessionId | sessionId | API-045 | P |
| API-070 / DTO-070 | TEACHER.ENTRY_RESULT.CREATE | bookingId | bookingId,attemptId,registrationId | API-073 | C |
| API-071 / DTO-071 | TEACHER.ENTRY_BOOKING.ABSENCE | bookingId | bookingId,attemptId,registrationId | API-073 | S |
| API-072 / DTO-072 | TEACHER.ENTRY_RESULT.CORRECT | attemptId | bookingId,attemptId,registrationId | API-073 | P |
| API-075 / DTO-075 | ACADEMIC.COURSE.CREATE | {} | courseId | API-090 | C |
| API-077 / DTO-077 | ACADEMIC.COURSE.UPDATE_DETAILS | courseId | courseId | API-090 | P |
| API-078 / DTO-078 | ACADEMIC.COURSE.DEACTIVATE | courseId | courseId | API-090 | S |
| API-079 / DTO-079 | ACADEMIC.COURSE.ACTIVATE | courseId | courseId | API-090 | S |
| API-082 / DTO-082 | ACADEMIC.PROGRAM.CREATE | {} | programId | API-090 | C |
| API-084 / DTO-084 | ACADEMIC.PROGRAM.UPDATE_CONFIGURATION | programId | programId | API-090 | P |
| API-085 / DTO-085 | ACADEMIC.PROGRAM.RENAME | programId | programId | API-090 | P |
| API-087 / DTO-087 | ACADEMIC.PROGRAM.ACTIVATE | programId | programId | API-090 | S |
| API-088 / DTO-088 | ACADEMIC.PROGRAM.RETURN_TO_DRAFT | programId | programId | API-090 | S |
| API-089 / DTO-089 | ACADEMIC.PROGRAM.DEACTIVATE | programId | programId | API-090 | S |
| API-092 / DTO-092 | ACADEMIC.CLASS.CREATE | {} | classId | API-090 | C |
| API-094 / DTO-094 | ACADEMIC.CLASS.UPDATE_CAPACITY | classId | classId | API-090 | P |
| API-097 / DTO-097 | ACADEMIC.CLASS.CHANGE_TEACHER | classId | classId | API-090 | P |
| API-099 / DTO-099 | ACADEMIC.CLASS.OPEN | classId | classId | API-090 | S |
| API-100 / DTO-100 | ACADEMIC.CLASS.START | classId | classId | API-090 | S |
| API-101 / DTO-101 | ACADEMIC.CLASS.COMPLETE | classId | classId | API-090 | S |
| API-102 / DTO-102 | ACADEMIC.CLASS.CANCEL | classId | classId | API-090 | S |
| API-108 / DTO-108 | ACADEMIC.CLASS.PLACE_REGISTRATION | classId | classId,registrationId,classAssignmentId | API-090 | C |
| API-118 / DTO-118 | ACADEMIC.SCHEDULE.CREATE | classId | classId,scheduleId | API-090 | C |
| API-120 / DTO-120 | ACADEMIC.SCHEDULE.CREATE_WEEKLY | classId | classId | API-090 | C |
| API-122 / DTO-122 | ACADEMIC.SCHEDULE.MOVE | scheduleId | classId,scheduleId | API-090 | P |
| API-124 / DTO-124 | ACADEMIC.SCHEDULE.REPLACE | scheduleId | classId,oldScheduleId,newScheduleId | API-090 | C |
| API-126 / DTO-126 | ACADEMIC.SCHEDULE.CONFIRM_HELD | scheduleId | classId,scheduleId | API-090 | S |
| API-133 / DTO-133 | TEACHER.ATTENDANCE.SAVE_DELTA | classId,scheduleId | classId,scheduleId | API-090 | G |
| API-139 / DTO-139 | TEACHER.GRADE_DRAFT.SAVE_DELTA | classId,classAssignmentId | classId,classAssignmentId | API-090 | G |
| API-141 / DTO-141 | TEACHER.CLASS.SUBMIT_GRADES | classId | classId | API-090 | S |
| API-142 / DTO-142 | TEACHER.CLASS.RESUBMIT_GRADES | classId | classId | API-090 | S |
| API-146 / DTO-146 | ACADEMIC.CLASS.RETURN_GRADES | classId | classId | API-090 | S |
| API-147 / DTO-147 | ACADEMIC.CLASS.FINALIZE_GRADES | classId | classId | API-090 | S |

Method/path/actor/task/TX không thay thế bằng action code; binding chính xác:

| API | Method/path | Actor / task / W | TX |
| --- | --- | --- | --- |
| API-008 | POST /admin/staff-accounts | Admin NORMAL; W01 | COMMAND; TX01; UUID |
| API-009 | POST /admin/accounts/{id}/reset-password | Admin NORMAL; W03 | COMMAND; TX13; UUID |
| API-010 | POST /admin/accounts/{id}/lock | Admin NORMAL; W03 | COMMAND; TX13; UUID |
| API-011 | POST /admin/accounts/{id}/unlock | Admin NORMAL; W03 | COMMAND; TX13; UUID |
| API-012 | POST /admin/accounts/{id}/disable | Admin NORMAL; W03 | COMMAND; TX13; UUID |
| API-013 | POST /admin/accounts/{id}/restore | Admin NORMAL; W03 | COMMAND; TX13; UUID |
| API-025 | POST /registrations | Student NORMAL, own; W05 | COMMAND; W05 atomic; UUID |
| API-028 | POST /registrations/{rId}/cancel | Student NORMAL, own; W05 | COMMAND; TX06; UUID |
| API-030 | POST /registrations/{rId}/entry-test-bookings | Student NORMAL, own; W07 | COMMAND; TX02; UUID |
| API-031 | POST /registrations/{rId}/entry-test-bookings/{bId}/cancel | Student NORMAL, own; W07 | COMMAND; TX02; UUID |
| API-032 | POST /registrations/{rId}/entry-test-bookings/{bId}/reschedule | Student NORMAL, own; W07 | COMMAND; TX02; UUID |
| API-036 | POST /registrations/{rId}/confirm | Student NORMAL, own; W09 | COMMAND; W09 atomic; UUID |
| API-041 | POST /admissions/registrations/{id}/approve | Admissions NORMAL; W10 | COMMAND; TX04; UUID |
| API-042 | POST /admissions/registrations/{id}/reject | Admissions NORMAL; W10 | COMMAND; W10 atomic; UUID |
| API-044 | POST /admissions/registrations/{id}/cancel | Admissions NORMAL; W10 | COMMAND; TX06; UUID |
| API-051 | POST /finance/invoices/{id}/collect | Accountant NORMAL; W12 | COMMAND; TX05; UUID |
| API-052 | PUT /finance/payments/{id}/metadata | Accountant NORMAL; W12 | COMMAND; W12 atomic metadata; UUID |
| API-055 | POST /admissions/entry-test-sessions | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-057 | PUT /admissions/entry-test-sessions/{sId}/schedule | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-058 | POST /admissions/entry-test-sessions/{sId}/open | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-059 | POST /admissions/entry-test-sessions/{sId}/close | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-061 | POST /admissions/entry-test-sessions/{sId}/cancel | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-064 | POST /admissions/entry-test-sessions/{sId}/change-grader | AcademicAffairs NORMAL; W06 | COMMAND; W06 atomic; UUID |
| API-070 | POST /admissions/teachers/me/entry-test-bookings/{bId}/score | Teacher NORMAL, current assigned/original grader guards; W08 | COMMAND; TX03; UUID |
| API-071 | POST /admissions/teachers/me/entry-test-bookings/{bId}/absence | Teacher NORMAL, current assigned/original grader guards; W08 | COMMAND; TX03; UUID |
| API-072 | PUT /admissions/teachers/me/entry-test-attempts/{tId}/score | Teacher NORMAL, current assigned/original grader guards; W08 | COMMAND; TX03; UUID |
| API-075 | POST /academics/courses | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-077 | PUT /academics/courses/{cId}/details | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-078 | POST /academics/courses/{cId}/deactivate | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-079 | POST /academics/courses/{cId}/activate | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-082 | POST /academics/training-programs | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-084 | PUT /academics/training-programs/{pId}/configuration | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-085 | PUT /academics/training-programs/{pId}/name | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-087 | POST /academics/training-programs/{pId}/activate | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-088 | POST /academics/training-programs/{pId}/return-to-draft | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-089 | POST /academics/training-programs/{pId}/deactivate | AcademicAffairs NORMAL; W13 | COMMAND; W13 atomic / config gates; UUID |
| API-092 | POST /academics/classes | AcademicAffairs NORMAL; W14 | COMMAND; TX07; UUID |
| API-094 | PUT /academics/classes/{cId}/capacity | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-097 | POST /academics/classes/{cId}/change-teacher | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-099 | POST /academics/classes/{cId}/open | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-100 | POST /academics/classes/{cId}/start | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-101 | POST /academics/classes/{cId}/complete | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-102 | POST /academics/classes/{cId}/cancel | AcademicAffairs NORMAL; W14 | COMMAND; W14 atomic; UUID |
| API-108 | POST /academics/classes/{cId}/assignments | AcademicAffairs NORMAL; W15 | COMMAND; TX08; UUID |
| API-118 | POST /academics/classes/{cId}/schedules | AcademicAffairs NORMAL; W16 | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-120 | POST /academics/classes/{cId}/weekly-schedules | AcademicAffairs NORMAL; W16 | COMMAND; TX09; UUID |
| API-122 | PUT /academics/schedules/{sId}/time-room | AcademicAffairs NORMAL; W16 | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-124 | POST /academics/schedules/{sId}/replace | AcademicAffairs NORMAL; W16 | COMMAND; TX09; UUID |
| API-126 | POST /academics/schedules/{sId}/confirm-held | AcademicAffairs NORMAL; W16 | COMMAND; W16 atomic / same Schedule protocol; UUID |
| API-133 | PUT /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | Teacher NORMAL, current Class; W17 | COMMAND; TX10; UUID |
| API-139 | PUT /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | Teacher NORMAL, current Class; W18 | COMMAND; TX11; UUID |
| API-141 | POST /academics/teachers/me/classes/{id}/submit-grades | Teacher NORMAL, current Class; W18 | COMMAND; TX12; UUID |
| API-142 | POST /academics/teachers/me/classes/{id}/resubmit-grades | Teacher NORMAL, current Class; W18 | COMMAND; TX12; UUID |
| API-146 | POST /academics/grade-reviews/classes/{id}/return | AcademicAffairs NORMAL; W19 | COMMAND; TX12; UUID |
| API-147 | POST /academics/grade-reviews/classes/{id}/finalize | AcademicAffairs NORMAL; W19 | COMMAND; TX12; UUID |

## 2. Normalized input và canonical bytes C1

C1 là phiên bản đặc tả trong tài liệu/envelope digest, **không thêm cột T23**.
Chỉ bind shape/type/allowlist, normalize và validate nội bộ request trước
hash; live state/time/RV comparison thuộc nhánh new mutation sau replay lookup.
Không kiểm current RV trước khi nhận ra intent đã hoàn tất. Unknown properties,
duplicate JSON keys, invalid Unicode, number coercion/duplicate set IDs400.
Không parse bằng serializer tự bỏ duplicate fields rồi mới validate.

Envelope chính xác gồm `action`, `body`, `contract`, `target`. contract literal
`EC-C1`; body normalized DTO; target gồm **mọi route business IDs** theo registry,
không include server-resolved/new IDs. Create target={}; courseId/body selected
relations vẫn nằm body. Actor/requestId là partition/lock T23, không vào bytes;
method/path không vào bytes vì đã bind cố định action. Không URL/query/header,
JWT/cookie/CSRF/password/temp secret, preview asOf, allowedActions hoặc traceId.
Staff input PII có thể tham gia digest nhưng không persist plaintext envelope,
không log bytes/hash/body; binary digest chỉ lưu T23, không gọi nó anonymization.

| Kiểu / semantics | Normalization trước serialization |
| --- | --- |
| Required/nullable | Missing R400 kể cả nullable; explicit null giữ null. Không blanket omitted=null. |
| Optional nullable text | Phone staff, payment reference omitted/blank/null→property với null; normalize theo ADR-091. Grade teacherComment **omitted giữ omitted**; provided blank/null→null vì clear khác unchanged. |
| Text | NFC/trim/length theo từng DTO/ADR-091; giữ nội dung khoảng trắng bên trong/case nếu field không có rule. Email ASCII trim/lower invariant, phone equivalent về dạng0, catalogue/room codes uppercase ASCII. Không uppercase names/reference/reason. |
| Score/weight | Validate string decimal tối đa2 decimals, parse exact; canonical string hai decimals: "80"/"80.5"/"80.50"→"80.00"/"80.50"/"80.50". Negative/exponent/comma/3 decimals400, không round trước hash. |
| VND | Validated positive decimal18 integer string, serialize shortest digit spelling; không numeric float. Response report decimal38 không command input. |
| IDs/count/present | JSON integer decimal shortest, không leading +/fraction/exponent coercion. present chỉ0/1, không bool. |
| Boolean/null | lowercase JSON true/false/null. D10 fields boolean giữ boolean. |
| RV | Decode đúng8 bytes rồi standard Base64 padded encode; token opaque. Không integer/reverse endian/change bytes. |
| Date/time/UUID refs | LocalDate YYYY-MM-DD, MinuteTime theo D10 HH:mm; date thật. UUID nhận parser chuẩn rồi lowercase hyphenated D; không time conversion theo browser. |

Objects recursive: sort property names ASCII ordinal ascending; property names
allowlist ASCII. Không whitespace/newline/BOM. JSON strings escape `"` thành
`\"`, backslash thành `\\`; control U+0000–001F thành lowercase `\u00xx`,
không short escape. Các code points khác ghi UTF-8 literal, kể cả tiếng Việt,
slash và U+2028; không escape non-ASCII hoặc normalize lần hai. UTF-8 strict,
không unpaired surrogate/replacement fallback. SHA-256 đúng chuỗi byte đó,
không hash hex/Base64 bytes hoặc newline cuối fenced block. Digest lưu32 byte;
hex64 lowercase chỉ là fixture biểu diễn, không field gửi về client.

Array semantics cố định cho cả55 schemas:

- IDs/full memberships/expected absence/version bindings/schedule bindings:
  set unique theo identity DTO; sort ID số tăng, composite binding sort tuple
  numeric theo các property ID theo tên ASCII tăng (null trước số nếu nullable).
  Reference có id/classAssignmentId/componentId làm primary identity dùng key
  đó trước, remaining ID names ASCII tăng; schedule bindings primary id.
  Empty bắt buộc serialize[]; không bỏ property.
- VersionedReference sort id; Attendance changes sort classAssignmentId;
  grade scores sort componentId. Không dedup duplicate item để che malformed.
- Program desired components là tập: sort existing id tăng, rồi new id=null
  theo normalized name ordinal, kind, weight; names unique theo E03 comparison
  (case-insensitive, accent-sensitive). Không reorder đổi identity/null mới thành
  temporary ID. Full source guard vẫn có component bị bỏ khỏi desired target.
- Weekly weekdays sort ISO1–7. candidates là **ordered concrete plan**, sort
  từ preview date/start theo D10 và reject request không cùng canonical order;
  không sort arbitrary plan để chữa missing/duplicate/date-rule mismatch.
- Không array input nào khác trong55 commands được tự coi set. Shape mới phải
  có semantics và compatibility review trước được phép canonicalize.

## 3. Fixtures byte — dữ liệu giả, không ứng dụng

Fixture F1 API-025 body courseId=7, target={}, EC-C1:

```json
{"action":"STUDENT.REGISTRATION.CREATE","body":{"courseId":7},"contract":"EC-C1","target":{}}
```

Fixture F2 API-009 target accountId=42, RV tám byte0..7:

```json
{"action":"ADMIN.ACCOUNT.RESET_PASSWORD","body":{"expectedVersions":{"account":"AAECAwQFBgc="}},"contract":"EC-C1","target":{"accountId":42}}
```

Fixture F3a/b normalized comment-only distinction (canonical fragments,
không request thiếu manifest để gửi API-139):

```json
{"scores":[]}
```

```json
{"scores":[],"teacherComment":null}
```

F1/F2 literal bytes và SHA256 nằm bảng được tính bên dưới. F3 phải khác digest
sau khi ghép **cùng** valid manifests/target/envelope. F2 đổi UUID header nhưng
content giống→bytes giống, partition request khác; đổi RV một byte→digest khác.
F1 đổi JSON property order/whitespace trước bind→same; courseId8→different.
Các equivalence fixtures thêm: NFC composed/decomposed tên, phone0/+84,
score80.5/80.50, set permutation→same normalized digest; omitted vs null
teacherComment, changed expected absence, old/new target, action family→different.
Invalid score80.500/duplicate binding/unknown field không tạo digest/marker.
Đây là oracle tài liệu, chưa bộ tests normalization hay client/server parity.

## 4. Persisted result và6 caller-result paths

result_reference JSON exact object `{"outcome":"COMPLETED","target":{...}}`:
keys target đúng registry; completed refs từ DML/server joins, không client
assertion. operation_id/mode lấy cột T23 riêng, không duplicate PII snapshot.
Không persist saved arrays/cohort scores/weekly created response/config/comments.
T23 expires>=completed+24h giữ E03; cấu hình exact24h Accepted ADR-160,
cleanup vận hành không xóa audit/business history. Expired marker không serve
replay dù chưa cleanup. Không PENDING/FAILED markers hoặc job status.

| Lookup | Verified action allowlist / current read quyền |
| --- | --- |
| API-014 /admin/command-results/{requestId} | Admin6 ADMIN.*; account scope; create/reset mode SECRET_RESULT_UNAVAILABLE, không secret/PII output. |
| API-037 /command-results/{requestId} | Student6 STUDENT.*; Registration/Booking/history own. Student cancel không expose invoiceId ngoài DTO scope; registry T cho API-028 chỉ registrationId. |
| API-045 /admissions/command-results/{requestId} | Admissions3 ADMISSIONS.* hoặc AcademicAffairs6 ACADEMIC.ENTRY_SESSION.*; dispatch role/action pair, không union rights. |
| API-053 /finance/command-results/{requestId} | Accountant2 FINANCE.*; Invoice/Payment linked và scope hiện tại. |
| API-073 /admissions/teachers/me/command-results/{requestId} | Teacher3 TEACHER.ENTRY_*; current assigned Session/task; correction còn original-grade identity. Không yêu cầu pre-confirmation source state chỉ để đọc committed completion. |
| API-090 /academics/command-results/{requestId} | AcademicAffairs25 config/Class/placement/Schedule/review hoặc Teacher4 Attendance/draft/submit/resubmit, Teacher current Class. Không AcademicAffairs Teacher DRAFT fields/save quyền. |

Lookup request UUID/action R, no body/caller/target selector. Unknown/malformed400;
known action ngoài endpoint/role403 trước marker probe. Authorized pair nhưng
not caller/absent/expired/outside current scope404 RESULT_NOT_CONFIRMED cùng
message. Session401/restricted403, no-store.200 đúng SafeCommandCompletion D10:
outcome COMPLETED, operationId, resultMode, target. Không completedAt/expiry/
input_digest/status-at-commit/credential/PII. Window absence không proof rollback.

Completed command replay: auth/session/current role→shape normalize/digest→
request gate→marker→current read scope/ref relationship→digest compare.
Scope failure không leak mismatch; matched valid marker không apply old RV,
clock/source-state guards của **new mutation**. First response201/200 theo DTO;
completed replay luôn200. API-008/009 trả SafeCommandCompletion secret-unavailable;
các commands khác trả current authorized CommandResult D10 với old operationId.
Weekly replay có thể trả SafeCommandCompletion thay created[]: refs chỉ classId,
không reconstruction sai original batch từ current schedule. Cần refetch lịch.
Nếu current target không dựng được command projection nhưng vẫn có quyền safe
refs, dùng SafeCommandCompletion; không union PII để giữ shape. Các unions này
là refinement SC-02 cần rà actual E04, UI phải đọc resultMode/shape rõ.

## 5. Outcome/error/no-op toàn55 commands

| Nhánh | Outcome / persistence / recovery |
| --- | --- |
| New valid command | First DTO success status, effects+required audit+completion cùng commit. Credential sinh trong TX nhưng chỉ response sau commit; failure không gửi secret. |
| Same actor/action/key + same digest, active marker | Completed200/current scope, old operation, no DML/new audit/SP07/secret generation. |
| Same partition/key + different digest |409 IDEMPOTENCY_CONTENT_MISMATCH sau current access checks; không ghi đè marker. RV/content sửa là new intent/new key, không silent merge. |
| Shape/required/internal bindings |400 theo DTO; no marker. Live RV/member/absence/state/time/conflict409 theo DTO; no marker/partial. Missing/out-of-scope404. |
| Server failure/audit fail trước commit | Rollback tất cả; sanitized500 nếu biết rollback; transport/client vẫn phải xử lý unknown khi mất response. |
| Connection/timeout/cancel quanh commit | Client UNKNOWN (không persisted DB enum), lookup/read trước retry chủ đích. Không thông báo chắc rollback; không automatic same/new-key resubmit. |
| Expired/absent marker |404 lookup; expired retained row command409 CONFLICT/no DML/no overwrite partition. Sau cleanup hoặc absent row không replay guarantee; state/unique invariants không ngăn mọi duplicate root. User đối chiếu history trước deliberate new intent/new key; không tự retry. |

No-op behavior đã **Accepted RF02/RF04, ADR-158/159 ngày03/10/2026**.
Codec/action/ref union và literal NO_CHANGE/STATE_CONFLICT vẫn E04 draft refinements:
C=create luôn tạo fact mới hoặc fail domain invariant, không suy existing=success;
K=Admin reset chủ đích luôn cấp secret mới/revoke theo policy (same key replay
không reset). S=transition phải đúng source, target state đã đạt bằng key khác
409 STATE_CONFLICT; không fake completion của tác vụ khác. EC effective CLOSED
do clock không miễn cutoff/reclassify Close thành thành công sau start.

P=full value equality sau normalize/current guards:400 NO_CHANGE với field
errors phù hợp, reason một mình không là change. G=delta batch: kiểm quyền,
versions/cohort/absence/state/time của **toàn** request trước equality;
no-op items không touch provenance, actual changed subset atomic, toàn bộ bằng
giá trị cũ400 NO_CHANGE/no marker. New row có score0/present0 vẫn actual change.
TeacherComment omitted khác clear; clear NULL đang NULL là no-op. Không
bỏ stale item vì nó nhìn giống giá trị hiện tại. No-op không optional mandatory
audit/history cho real mutation. RF02 và RF04 đã được owner duyệt trực tiếp;
requirements4.9/ADR-158/159 đã đồng bộ, không suy duyệt toàn C1/outcome shapes.
Digest luôn bao gồm toàn normalized supplied intent/manifests; không loại
unchanged items dựa live DB trước hashing. Equality thuộc new mutation branch.

## 6. Compatibility và điều kiện rà SC-02

C1 action/normalization/array/optional rules/result refs giữ bất biến trong
cửa sổ marker. Không đổi codec/action để request cũ thành new intent. Nếu refinement
không tương thích: giữ handler C1 trong toàn window **và không cho write cũ
bypass business rules mới**; hoặc dừng writes, drain window từ last completed
marker, lưu history rồi mới đổi contract công khai. Không thêm digest-version
DB field, không chạy hai codec rồi match tùy ý, không xóa marker sớm để upgrade.
Freeze C1 sau owner acceptance; thay semantic action mới cần deliberate design
review, không chỉ đổi URL. Old markers bị schema drift không guess refs/snapshot:
fail closed sanitized server error và operator investigation, client unknown.

Đã có registry55/6 +byte contract/result/no-op/compatibility draft. Cần owner
đọc unions/refinements trong actual E04; RF01/02/04 đã Accepted157–159. Kiểm normalization
fixtures executable chỉ sau authority implementation. Không tick E04/E05.

| Fixture | UTF8 bytes (no BOM/newline) | SHA256 hex |
| --- | --- | --- |
| F1 | 93 | 4faede809c38071f3a7c46f03466cd09a83e02f0c7d22ba63cc12c9f66c12542 |
| F2 | 141 | 5c14cad40c163d9da7f78016fbedae387ef6c5df3b5306b7dc6001b69f9c6de5 |

Kiểm fixture03/10/2026 chỉ tính byte/hash literal tài liệu; không kiểm codec application.

### Fixtures bổ sung UTF8/negative equality — literal bytes

F4 full nonsecret API008 normalized staff request, composed NFC name,
phone omitted/blank/null có cùng normalized property null; role case fixed:

```json
{"action":"ADMIN.STAFF.CREATE","body":{"email":"demo.teacher@example.test","fullName":"Nguyễn An","phone":null,"role":"Teacher"},"contract":"EC-C1","target":{}}
```

F5 khác F1 đúng business courseId, cùng action/target/codec:

```json
{"action":"STUDENT.REGISTRATION.CREATE","body":{"courseId":8},"contract":"EC-C1","target":{}}
```
| Fixture | UTF8 bytes (no BOM/newline) | SHA256 hex |
| --- | --- | --- |
| F4 | 162 | d08e9ffbc57a80bdbe1496c583febff7b7515a43fde33ef7218cc8d49718fe56 |
| F5 | 93 | a823e7339a2161e37d829a56623eac2e31bbb2062912e9a7658ed4a3f7842c0c |

Hashes tính từ literal normalization đã minh họa,không chứng minh runtime NFC/binder/set codec; F1/F5 different hash. Không log fixtures từ dữ liệu thật.
