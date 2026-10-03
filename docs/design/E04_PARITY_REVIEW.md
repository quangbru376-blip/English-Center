# E04 — Rà tổng hợp SC-01 tại checkpoint SC02–04

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Ngày03/10/2026. **SC-01 Accepted trong E04-v1 ADR-160; E05 Accepted ADR-161.**
Nguồn [154 API inventory](E04_CONTRACT_CATALOG.md), [154 DTO D10](E04_DTO_SPEC.md),
[SC02](E04_REPLAY_SPEC.md), [SC03](E04_SQL_INTEGRATION_SPEC.md),
[SC04](E04_RUNTIME_PROFILE.md), requirements/E01/E02/E03/ADR-001–159,
[final contract](E04_FINAL_CONTRACT.md), [E05 cross-check](E05_BASELINE_REVIEW.md).
Không thêm endpoint/bảng/SQL/code/test hoặc đánh dấu owner acceptance.

## 1. Coverage và giới hạn kiểm tra

Inventory154 unique method/path=76 READ+18 PREVIEW+58 COMMAND+2 AUTH;
55 COMMAND UUID,3 ngoại lệ signup/profile/password change.6 lookups có role/action
pair allowlist6Admin/6Student/9shared-Admissions/2Accountant/3Teacher-entry/
29academics(25AcademicAffairs+4Teacher)=55. Action keys unique≤80ASCII,
DTO IDs đầy đủ001–154, giữ32F/21W/37NV. C1 digest/T23/result unions nối mỗi
command; SP/FN đã có result types/parameters/callers; runtime slots deferred rõ.

Đã kiểm tự động cấu trúc inventory/actions/bindings,18 DTO JSON parse,4 full
canonical fixture byte counts/hashes, local links và fingerprints8 nguồn/owner
review được bảo toàn. Parse/hash là phép kiểm tài liệu, **không codec parity**,
HTTP/SQL/EF/authorization/race/load/browser/export/recovery tests. Việc một ID
xuất hiện trong D10 không tự chứng minh every field guard correct.

## 2. Rà theo miền và kết luận cross-contract

| API range / nhóm | Đối chiếu request-response/null/guard/role/error/sort | Kết quả/việc còn chờ |
| --- | --- | --- |
|001–005|AuthContext/session NORMAL vs PASSWORD_CHANGE; CSRF RAM/refetch; own password2 credentials no digest; revoke-all/current logout; GET auth không idle renewal;401/403/429 không auto resend.|Đã giữ ADR097–100/140, code chưa có; key/origin/deadline slots SC04.|
|006–017|Admin6 canonical commands/2 first-only secrets; expected AccountRV/state/self/last-active; paginated search/audit newest with ID tie; completion no PII; audit group append-only.|SC02 now fixes selectors/mode/replay200. Credential source typo ADR100 corrected to114/054.|
|018–023|Signup no auto login/UUID; profile4 fields+2RV/address R nullable; immutable login email; own home/Course ACTIVE-only selection/current profile vs historical refs.|Giữ field/R/nullable/precision/scopes; no credential-form replay; no new staff required fields.|
|024–037|6Student commands; own/history/Attempt vs Booking; full-set/absence/time/CLOSED guards; cancel atomic future bookings/unpaid Invoice; confirm no admission/Invoice effects.|T23 Student cancel target only registrationId, no invoice ref privilege added. Binding normalization includes old/new route refs/manifests; result syntax updated SC02.|
|038–045|Admissions contact refs/money-state Invoice only;3commands and6Academic session actions share lookup with role pair; search PII POST/count+page; Course price live snapshot, SP07 atomic.|SP01 exact flattened result map; no Payment/DOB/address; approved/rejected history still scoped.|
|046–053|Student-own Invoice/fresh print vs Accountant read/collect/correct; amount exact18 positive integer; no paidAt/amount edit; reference optional CASH/required TRANSFER; Invoice-Registration-Payment absence/RV.|SP/report amounts38 strings distinct18 money; no refund/duplicate Payment or bank-reference dedup. Metadata equality RF04 Accepted159.|
|054–065|6Academic Session commands; same-day minute/capacity1–20/ACTIVE grader; effective CLOSED at start;cancel full Booking source set;grader reason/live relation; roster invoiceStatus-only.|RF01 cutoff Accepted157/064RF02 no-op Accepted158; SP02 AA projection separate from Teacher, no new Student/finance rights.|
|066–073|3Teacher-entry commands; current assigned Session and correction original grader; grade end-inclusive;ABSENT≠zero;result target completed Booking;BEST tie first Attempt;confirmation/cancel locks.|Replay read scope separated from source-state guard, no post-confirmation mutation override. RF02 parsed score no-change Accepted158.|
|074–090|10config commands; codes immutable/price/name/lifecycle;whole unused DRAFT config/source-target sets;ComponentWeight max2decimal,activation exact100/oneexam;freeze after any Class;Band seed no web threshold writer.|SC02 desired component set ordering explicit;SC03 phased config writes avoid intermediate uniqueness guesses.25AA+4Teacher academic lookup not union rights.|
|091–113|8Class/placement commands;PLANNED/OPEN lifecycle/current Teacher/max1–15;complete full HELD+Attendance; placement same CourseBand/paid/unassigned/no clashes;Assignment+DRAFT report atomic/reads3roles.|Full cohort version/absence set not page/classRV alone. Assignment immutable/no transfer; SC03 full lock set/first-use gate.|
|114–130|5Schedule commands;current vs retained chain;date/time/room/version/source dependencies;weekly full ordered plan/no trimming;RF03 live future start root/weekly/move;replacement one obligation;HELD manual afterend.|RF03 remains Accepted156. Weekly replay fallback completion no reconstruct batch from current schedule. No backfill/extra obligations.|
|131–139|2Teacher delta saves;AcademicAttendance read-only;new missing row vs existing RV;present0/1 not bool;score0 actual data;comment omitted unchanged/null clears;frozen component+current schedule links;reportRV touch.|RF04 same-value/mixed delta Accepted159;SC03 Attendance latest provenance retained/no extra mandatory audit policy invented. Parent touch only actual TX11 mutation.|
|140–149|4wholeClass commands;full visible vs full cohort distinction;Teacher send/resend,AA review onlynonDRAFT;ClassCOMPLETED;finalize stored rounded>=50+unfinished predicates;Student own FINALIZED only.|SC03 SP03 excludes DRAFT rows/tokens in every RS; no partial cohort publication/selected report transitions.|
|150–154|Report separate event periods/current debt/inventory/finalized result;course options history;totalswhole set/distinct independently;null no-data vs0%;Director aggregates-only;Accountant fresh all-results bounded CSV.|SC03 decimal38 digits avoids .NETdecimal overflow;SP04–06 result sets no person IDs;D10 CSV layout/options refinements still need actual E04 owner review.|

Bảng trên rà các nguy cơ cross-contract/nguồn ở mọi miền. Không gọi đó là
field-by-field independent proof của toàn293KB D10; remaining exact spelling/
NULL/sort/error/fixtures review là nội dung owner rà actual draft. Các nhóm
đã có specified shapes/guards, không còn thiếu schema miền.

## 3. Findings đã xử lý trong tài liệu

1. API-ID/UI capability không phải persisted action:55 explicit action strings,
   method/path/actor/W/TX bindings,6 lookup selectors và safe targets trong SC02.
2. Existing-version stale check không được chặn completed intent; current rights
   vẫn rechecked, source-state/time guards chỉ new mutation. No-op không lẫn
   replay; RF02/RF04 equality400 vs stale409 đã Accepted ADR-158/159.
3. Optional nullable text không blanket omitted=null:teacherComment omitted giữ,
   clear null là intent khác. Source/desired components và full absence/cohort
   arrays đều hash, không silently dedup/reorder ordered weekly plan.
4. Weekly result marker classId-only không đủ reconstruct created[]; replay
   dùng safe completion/refetch. All safe fallback result unions explicit,
   không lưu response/PII/secret hoặc add T23 fields để khắc phục.
5. Decimal38 report vượt .NETdecimal max;SQL aggregate exact then varchar38
   result/repository string,không float/truncate/EF decimal default.
6. SP02 hai role row schemas và SP03 visible-only child results giữ finance/
   DRAFT privacy. Runtime grants24tables/views/SP/FN không SQL demo role thành
   web rights; T24 chỉ allocator EXEC, T22/T23 no runtime update/delete.
7.5trigger tables cần table-scoped EF OUTPUT alternative, generatedID/RV;
   config swap tên/exam kind cần phased uncommitted writes dưới freeze gate,
   không đoán một SaveChanges tự qua unique intermediate constraint.
8. Gate discovery có thể stale, requires rollback/reprepare trước DML; Account→
   session deterministic locking dùng cho reset/revoke/idle và business guard,
   không session→Account ngược chiều.Compound snapshots/readiness option checked.
9. Runtime slots không tự điền số timeout/port/provider/storage hoặc body-cap
   thành lesson/date business limit; unknown commit vẫn lookup/no auto retry.

## 4. RF đã được owner quyết; actual artifact review còn mở

| ID | Đề xuất / ảnh hưởng / trạng thái |
| --- | --- |
| RF01 Accepted ADR-157 | EntrySession create startAt>serverNow tại saving; equality/passed409/no backfill. Requirements/NV14/W06 addendum đã synced, no backfill. |
| RF02 Accepted ADR-158 | Same grader hoặc parsed equal corrected entry score400/no DML/audit/marker mới; stale context409; reason/source windows unchanged. |
| RF04 Accepted ADR-159 | Other value-edit all-equal400 NO_CHANGE;mixed deltas validate whole request then save actual changes atomically, unchanged rows keep provenance;state transitions wrong source409,new deliberate reset always creates new temp credential. Tránh fake save/correction history;không schema/new permissions. |

Các wire strings/shapes/SC02 codec/SP signatures/grants/EF/write audit mappings/
CSV layout và runtime slot types là **proposed technical contract** để owner
đọc cùng actual E04,không bắt owner bỏ phiếu từng column. Gói139–155 approval
không pre-approve những chi tiết này. RF03 Accepted156; RF01/02/04 Accepted157–159 trong lượt duyệt riêng.
D12/D13 và hosting/deadlines/backup media/access giữ deferred đúng thời điểm.

## 5. Checkpoint readiness trước lượt nghiệm thu

SC01–04 documentation complete for actual E04-v1 review, với final contract
giải quyết error precedence/strict parsers/replay unions/live read rights/
expired retained row/CSV cap và DF01–05. RF01–04 đã được duyệt, không re-vote.
E05-v1 đã chuẩn bị theo yêu cầu hoàn thiện cả hai, nghiệm thu có điều kiện sau
E04; owner sửa E04 phải rerà affected E05. Cả hai vẫn **review pending**. Chưa
implementation planning/Git/scaffold/code/SQL/package/deployment/runtime proof.
## 6. Trace154 từng route tới schema/source/replay

Mỗi dòng chỉ kiểm structural mapping, không tự tick semantic field review.
Mục DTO là section number của [D10](E04_DTO_SPEC.md); types dùng chung tại
mục1/các mục con vẫn áp dụng. Rights/error/sort/output trong schema nguồn;
SC03 map SQL/caller tương ứng,SC04 môi trường chung. All rows draft review.

| API | Method/path | DTO mục | Canonical action hoặc exception/read |
| --- | --- | --- | --- |
| API-001 | GET /auth/csrf | 2 | READ; —; không UUID |
| API-002 | POST /auth/login | 2 | AUTH; —; không UUID |
| API-003 | GET /auth/me | 2 | READ; —; không UUID |
| API-004 | POST /auth/change-password | 2 | COMMAND không UUID (accepted exception) |
| API-005 | POST /auth/logout | 2 | AUTH; —; không UUID |
| API-006 | POST /admin/accounts/search | 6 | READ; —; không UUID |
| API-007 | GET /admin/accounts/{id} | 6 | READ; —; không UUID |
| API-008 | POST /admin/staff-accounts | 6 | ADMIN.STAFF.CREATE |
| API-009 | POST /admin/accounts/{id}/reset-password | 6 | ADMIN.ACCOUNT.RESET_PASSWORD |
| API-010 | POST /admin/accounts/{id}/lock | 6 | ADMIN.ACCOUNT.LOCK |
| API-011 | POST /admin/accounts/{id}/unlock | 6 | ADMIN.ACCOUNT.UNLOCK |
| API-012 | POST /admin/accounts/{id}/disable | 6 | ADMIN.ACCOUNT.DISABLE |
| API-013 | POST /admin/accounts/{id}/restore | 6 | ADMIN.ACCOUNT.RESTORE |
| API-014 | GET /admin/command-results/{requestId} | 6 | READ; —; không UUID |
| API-015 | GET /admin/audit-events | 7 | READ; —; không UUID |
| API-016 | GET /admin/audit-events/{id} | 7 | READ; —; không UUID |
| API-017 | GET /admin/operations/{operationId}/audit-events | 7 | READ; —; không UUID |
| API-018 | POST /auth/register | 3 | COMMAND không UUID (accepted exception) |
| API-019 | GET /students/me/profile | 4 | READ; —; không UUID |
| API-020 | PUT /students/me/profile | 4 | COMMAND không UUID (accepted exception) |
| API-021 | GET /students/me/home | 8 | READ; —; không UUID |
| API-022 | GET /students/me/courses | 5 | READ; —; không UUID |
| API-023 | GET /students/me/courses/{id} | 5 | READ; —; không UUID |
| API-024 | GET /registrations | 9 | READ; —; không UUID |
| API-025 | POST /registrations | 9 | STUDENT.REGISTRATION.CREATE |
| API-026 | GET /registrations/{rId} | 9 | READ; —; không UUID |
| API-027 | POST /registrations/{rId}/cancellation-preview | 9 | PREVIEW; —; không UUID |
| API-028 | POST /registrations/{rId}/cancel | 9 | STUDENT.REGISTRATION.CANCEL |
| API-029 | GET /registrations/{rId}/entry-test-options | 9 | READ; —; không UUID |
| API-030 | POST /registrations/{rId}/entry-test-bookings | 9 | STUDENT.ENTRY_BOOKING.CREATE |
| API-031 | POST /registrations/{rId}/entry-test-bookings/{bId}/cancel | 9 | STUDENT.ENTRY_BOOKING.CANCEL |
| API-032 | POST /registrations/{rId}/entry-test-bookings/{bId}/reschedule | 9 | STUDENT.ENTRY_BOOKING.RESCHEDULE |
| API-033 | GET /entry-test-bookings | 9 | READ; —; không UUID |
| API-034 | GET /registrations/{rId}/entry-results | 9 | READ; —; không UUID |
| API-035 | POST /registrations/{rId}/confirmation-preview | 9 | PREVIEW; —; không UUID |
| API-036 | POST /registrations/{rId}/confirm | 9 | STUDENT.REGISTRATION.CONFIRM |
| API-037 | GET /command-results/{requestId} | 9 | READ; —; không UUID |
| API-038 | POST /admissions/registrations/search | 10 | READ; —; không UUID |
| API-039 | GET /admissions/registrations/{id} | 10 | READ; —; không UUID |
| API-040 | POST /admissions/registrations/{id}/approval-preview | 10 | PREVIEW; —; không UUID |
| API-041 | POST /admissions/registrations/{id}/approve | 10 | ADMISSIONS.REGISTRATION.APPROVE |
| API-042 | POST /admissions/registrations/{id}/reject | 10 | ADMISSIONS.REGISTRATION.REJECT |
| API-043 | POST /admissions/registrations/{id}/cancellation-preview | 10 | PREVIEW; —; không UUID |
| API-044 | POST /admissions/registrations/{id}/cancel | 10 | ADMISSIONS.REGISTRATION.CANCEL |
| API-045 | GET /admissions/command-results/{requestId} | 10 | READ; —; không UUID |
| API-046 | GET /students/me/invoices | 11 | READ; —; không UUID |
| API-047 | GET /students/me/invoices/{id} | 11 | READ; —; không UUID |
| API-048 | POST /finance/invoices/search | 11 | READ; —; không UUID |
| API-049 | GET /finance/invoices/{id} | 11 | READ; —; không UUID |
| API-050 | POST /finance/invoices/{id}/collection-preview | 11 | PREVIEW; —; không UUID |
| API-051 | POST /finance/invoices/{id}/collect | 11 | FINANCE.INVOICE.COLLECT |
| API-052 | PUT /finance/payments/{id}/metadata | 11 | FINANCE.PAYMENT.UPDATE_METADATA |
| API-053 | GET /finance/command-results/{requestId} | 11 | READ; —; không UUID |
| API-054 | GET /admissions/entry-test-sessions | 12 | READ; —; không UUID |
| API-055 | POST /admissions/entry-test-sessions | 12 | ACADEMIC.ENTRY_SESSION.CREATE |
| API-056 | GET /admissions/entry-test-sessions/{sId} | 12 | READ; —; không UUID |
| API-057 | PUT /admissions/entry-test-sessions/{sId}/schedule | 12 | ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE |
| API-058 | POST /admissions/entry-test-sessions/{sId}/open | 12 | ACADEMIC.ENTRY_SESSION.OPEN |
| API-059 | POST /admissions/entry-test-sessions/{sId}/close | 12 | ACADEMIC.ENTRY_SESSION.CLOSE |
| API-060 | POST /admissions/entry-test-sessions/{sId}/cancellation-preview | 12 | PREVIEW; —; không UUID |
| API-061 | POST /admissions/entry-test-sessions/{sId}/cancel | 12 | ACADEMIC.ENTRY_SESSION.CANCEL |
| API-062 | GET /admissions/entry-test-grader-options | 12 | READ; —; không UUID |
| API-063 | POST /admissions/entry-test-sessions/{sId}/grader-change-preview | 12 | PREVIEW; —; không UUID |
| API-064 | POST /admissions/entry-test-sessions/{sId}/change-grader | 12 | ACADEMIC.ENTRY_SESSION.CHANGE_GRADER |
| API-065 | POST /admissions/entry-test-sessions/{sId}/roster/search | 12 | READ; —; không UUID |
| API-066 | GET /admissions/teachers/me/entry-test-sessions | 13 | READ; —; không UUID |
| API-067 | GET /admissions/teachers/me/entry-test-sessions/{sId} | 13 | READ; —; không UUID |
| API-068 | POST /admissions/teachers/me/entry-test-sessions/{sId}/roster/search | 13 | READ; —; không UUID |
| API-069 | GET /admissions/teachers/me/entry-test-bookings/{bId} | 13 | READ; —; không UUID |
| API-070 | POST /admissions/teachers/me/entry-test-bookings/{bId}/score | 13 | TEACHER.ENTRY_RESULT.CREATE |
| API-071 | POST /admissions/teachers/me/entry-test-bookings/{bId}/absence | 13 | TEACHER.ENTRY_BOOKING.ABSENCE |
| API-072 | PUT /admissions/teachers/me/entry-test-attempts/{tId}/score | 13 | TEACHER.ENTRY_RESULT.CORRECT |
| API-073 | GET /admissions/teachers/me/command-results/{requestId} | 13 | READ; —; không UUID |
| API-074 | GET /academics/courses | 14 | READ; —; không UUID |
| API-075 | POST /academics/courses | 14 | ACADEMIC.COURSE.CREATE |
| API-076 | GET /academics/courses/{cId} | 14 | READ; —; không UUID |
| API-077 | PUT /academics/courses/{cId}/details | 14 | ACADEMIC.COURSE.UPDATE_DETAILS |
| API-078 | POST /academics/courses/{cId}/deactivate | 14 | ACADEMIC.COURSE.DEACTIVATE |
| API-079 | POST /academics/courses/{cId}/activate | 14 | ACADEMIC.COURSE.ACTIVATE |
| API-080 | GET /academics/bands | 14 | READ; —; không UUID |
| API-081 | GET /academics/training-programs | 15 | READ; —; không UUID |
| API-082 | POST /academics/training-programs | 15 | ACADEMIC.PROGRAM.CREATE |
| API-083 | GET /academics/training-programs/{pId} | 15 | READ; —; không UUID |
| API-084 | PUT /academics/training-programs/{pId}/configuration | 15 | ACADEMIC.PROGRAM.UPDATE_CONFIGURATION |
| API-085 | PUT /academics/training-programs/{pId}/name | 15 | ACADEMIC.PROGRAM.RENAME |
| API-086 | POST /academics/training-programs/{pId}/activation-preview | 15 | PREVIEW; —; không UUID |
| API-087 | POST /academics/training-programs/{pId}/activate | 15 | ACADEMIC.PROGRAM.ACTIVATE |
| API-088 | POST /academics/training-programs/{pId}/return-to-draft | 15 | ACADEMIC.PROGRAM.RETURN_TO_DRAFT |
| API-089 | POST /academics/training-programs/{pId}/deactivate | 15 | ACADEMIC.PROGRAM.DEACTIVATE |
| API-090 | GET /academics/command-results/{requestId} | 15 | READ; —; không UUID |
| API-091 | GET /academics/classes | 16 | READ; —; không UUID |
| API-092 | POST /academics/classes | 16 | ACADEMIC.CLASS.CREATE |
| API-093 | GET /academics/classes/{cId} | 16 | READ; —; không UUID |
| API-094 | PUT /academics/classes/{cId}/capacity | 16 | ACADEMIC.CLASS.UPDATE_CAPACITY |
| API-095 | GET /academics/class-teacher-options | 16 | READ; —; không UUID |
| API-096 | POST /academics/classes/{cId}/teacher-change-preview | 16 | PREVIEW; —; không UUID |
| API-097 | POST /academics/classes/{cId}/change-teacher | 16 | ACADEMIC.CLASS.CHANGE_TEACHER |
| API-098 | POST /academics/classes/{cId}/transition-preview | 16 | PREVIEW; —; không UUID |
| API-099 | POST /academics/classes/{cId}/open | 16 | ACADEMIC.CLASS.OPEN |
| API-100 | POST /academics/classes/{cId}/start | 16 | ACADEMIC.CLASS.START |
| API-101 | POST /academics/classes/{cId}/complete | 16 | ACADEMIC.CLASS.COMPLETE |
| API-102 | POST /academics/classes/{cId}/cancel | 16 | ACADEMIC.CLASS.CANCEL |
| API-103 | POST /academics/classes/{cId}/roster/search | 16 | READ; —; không UUID |
| API-104 | POST /academics/placement-candidates/search | 17 | READ; —; không UUID |
| API-105 | GET /academics/placement-candidates/{rId} | 17 | READ; —; không UUID |
| API-106 | GET /academics/placement-candidates/{rId}/classes | 17 | READ; —; không UUID |
| API-107 | POST /academics/classes/{cId}/placement-preview | 17 | PREVIEW; —; không UUID |
| API-108 | POST /academics/classes/{cId}/assignments | 17 | ACADEMIC.CLASS.PLACE_REGISTRATION |
| API-109 | GET /academics/teachers/me/classes | 18 | READ; —; không UUID |
| API-110 | GET /academics/teachers/me/classes/{cId} | 18 | READ; —; không UUID |
| API-111 | POST /academics/teachers/me/classes/{cId}/roster/search | 18 | READ; —; không UUID |
| API-112 | GET /students/me/classes | 18 | READ; —; không UUID |
| API-113 | GET /students/me/classes/{cId} | 18 | READ; —; không UUID |
| API-114 | GET /academics/schedules | 19 | READ; —; không UUID |
| API-115 | GET /academics/schedules/{sId} | 19 | READ; —; không UUID |
| API-116 | GET /academics/classes/{cId}/scheduling-context | 19 | READ; —; không UUID |
| API-117 | POST /academics/classes/{cId}/schedule-preview | 20 | PREVIEW; —; không UUID |
| API-118 | POST /academics/classes/{cId}/schedules | 20 | ACADEMIC.SCHEDULE.CREATE |
| API-119 | POST /academics/classes/{cId}/weekly-schedule-preview | 20 | PREVIEW; —; không UUID |
| API-120 | POST /academics/classes/{cId}/weekly-schedules | 20 | ACADEMIC.SCHEDULE.CREATE_WEEKLY |
| API-121 | POST /academics/schedules/{sId}/move-preview | 21 | PREVIEW; —; không UUID |
| API-122 | PUT /academics/schedules/{sId}/time-room | 21 | ACADEMIC.SCHEDULE.MOVE |
| API-123 | POST /academics/schedules/{sId}/replacement-preview | 21 | PREVIEW; —; không UUID |
| API-124 | POST /academics/schedules/{sId}/replace | 21 | ACADEMIC.SCHEDULE.REPLACE |
| API-125 | POST /academics/schedules/{sId}/held-preview | 22 | PREVIEW; —; không UUID |
| API-126 | POST /academics/schedules/{sId}/confirm-held | 22 | ACADEMIC.SCHEDULE.CONFIRM_HELD |
| API-127 | GET /academics/teachers/me/schedules | 23 | READ; —; không UUID |
| API-128 | GET /academics/teachers/me/schedules/{sId} | 23 | READ; —; không UUID |
| API-129 | GET /students/me/schedules | 23 | READ; —; không UUID |
| API-130 | GET /students/me/schedules/{sId} | 23 | READ; —; không UUID |
| API-131 | GET /academics/teachers/me/classes/{cId}/attendance | 24 | READ; —; không UUID |
| API-132 | GET /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | 24 | READ; —; không UUID |
| API-133 | PUT /academics/teachers/me/classes/{cId}/lessons/{sId}/attendance | 24 | TEACHER.ATTENDANCE.SAVE_DELTA |
| API-134 | GET /academics/classes/{cId}/attendance | 24 | READ; —; không UUID |
| API-135 | GET /academics/classes/{cId}/lessons/{sId}/attendance | 24 | READ; —; không UUID |
| API-136 | GET /academics/teachers/me/classes/{cId}/gradebook | 25 | READ; —; không UUID |
| API-137 | GET /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | 25 | READ; —; không UUID |
| API-138 | GET /academics/teachers/me/classes/{cId}/components/{componentId}/schedule-options | 25 | READ; —; không UUID |
| API-139 | PUT /academics/teachers/me/classes/{cId}/assignments/{aId}/grade-draft | 25 | TEACHER.GRADE_DRAFT.SAVE_DELTA |
| API-140 | POST /academics/teachers/me/classes/{id}/grade-submission-preview | 27 | PREVIEW; —; không UUID |
| API-141 | POST /academics/teachers/me/classes/{id}/submit-grades | 27 | TEACHER.CLASS.SUBMIT_GRADES |
| API-142 | POST /academics/teachers/me/classes/{id}/resubmit-grades | 27 | TEACHER.CLASS.RESUBMIT_GRADES |
| API-143 | GET /academics/grade-reviews | 28 | READ; —; không UUID |
| API-144 | GET /academics/grade-reviews/classes/{id} | 28 | READ; —; không UUID |
| API-145 | POST /academics/grade-reviews/classes/{id}/review-preview | 28 | PREVIEW; —; không UUID |
| API-146 | POST /academics/grade-reviews/classes/{id}/return | 28 | ACADEMIC.CLASS.RETURN_GRADES |
| API-147 | POST /academics/grade-reviews/classes/{id}/finalize | 28 | ACADEMIC.CLASS.FINALIZE_GRADES |
| API-148 | GET /students/me/grade-reports | 29 | READ; —; không UUID |
| API-149 | GET /students/me/grade-reports/{id} | 29 | READ; —; không UUID |
| API-150 | GET /reports/accountant/finance | 32 | READ; —; không UUID |
| API-151 | GET /reports/director/finance | 32 | READ; —; không UUID |
| API-152 | GET /reports/director/admissions | 33 | READ; —; không UUID |
| API-153 | GET /reports/director/training | 34 | READ; —; không UUID |
| API-154 | GET /reports/accountant/finance/csv | 35 | READ; —; không UUID |

## 7. Kết quả kiểm tài liệu trước lượt duyệt RF — 03/10/2026

154 inventory rows/55 unique actions/6 role-action lookup groups (6,6,9,2,3,29),
55 exact command-gate bindings và154 individual schema trace rows checked.
18 DTO JSON+6 replay literal JSON parse;4 full canonical fixtures byte lengths/
SHA256 recomputed and matched.74 local file links resolved;Markdown table widths
consistent across13 affected documents.8 protected sources/owner-review SHA256
unchanged (requirements,DECISIONS,E01 FUNCTION_SPEC,E02 WORKFLOW_SPEC,E03
DATABASE_SPEC,OWNER_REVIEW,E04_REMAINING_REVIEW,FUNCTION_CATALOG).
These are structural/document checks only;no application codec/HTTP/SQL/EF/
authorization/race/browser/CSV/load/recovery tests,Accepted ADR or E04/E05 ticks.
## 8. RF resolution — 03/10/2026

Owner duyệt ba đề xuất RF01/02/04 sau lượt gom: ADR-157/158/159 Accepted,
requirements và NV/W/DTO/no-op registry/memory đã đồng bộ. RF03 giữADR-156.
Chỉ behavior cutoff/equality400/stale-wrong-source409/mixed-batch provenance
được duyệt; full technical artifact SC01/02/03/04/E04/E05 chưa nghiệm thu,
D12/D13 vẫn deferred. Không code/SQL/runtime tests/implementation plan.

Kiểm sau ghi nhận duyệt:154 API inventory rows/55 registry actions/24 JSON
examples/105 local links và table widths của16 tài liệu đạt structural checks.
SHA256 E03 DATABASE_SPEC/OWNER_REVIEW/E04_REMAINING_REVIEW/FUNCTION_CATALOG
giữ nguyên; bytes ADR001–156 giữ nguyên trước phần append ADR157–159.
Requirements/E01/E02 đổi có chủ đích để đồng bộ behavior đã duyệt.

## 9. Current E04-v1/E05-v1 document validation — 03/10/2026

Actual E04-v1 package và E05-v1 prepared đã kiểm sau edits:

- 154unique method/path;55UUID commands/55uniqueactions và exact method/path/
  actor/W/TX bindings;6lookup groups6/6/9/2/3/29.
- SC03 gate matrix và audit classification mỗi55commands khớp registry;
  154schema/action trace,32F/21W,37retainedNV (exclude10/11),24E5cases/10findings.
- 24JSON examples parse;4full C1literal UTF8byte lengths/SHA256 matched.
- 131localfilelinks resolve;Markdown table widths consistent across17files.
- SHA2568protectedfiles unchanged:DECISIONS,E01FUNCTION_SPEC,E02WORKFLOW_SPEC,
  E03DATABASE_SPEC,FUNCTION_CATALOG,E04_REMAINING_REVIEW,BUSINESS_FUNCTION_TABLE,
  SEQUENCE_PLAN. Source Word/side guides không edited.
- OWNER_REVIEW75rows/71checked và mọi75approval/notes preserved;73otherrows
  byte-identical,E04/E05 chỉ first2description cells/link outputs updated.
  Requirements editorial changes documented CF07/08;no newAcceptedADR/ticks.

These checks are document-structure/source-preservation evidence,not application
normalizer/binder/HTTP/SQL/EF/grants/races/browser/CSV/load/restore execution.
Domain semantic findings/refinements remain actual ownerE04review;E05 acceptance
conditional afterE04/affected rerà,no claim field-by-field runtime proof.

## 10. Actual acceptance — 03/10/2026

Owner accepted E04-v1 first ADR-160,then E05-v1/full baseline ADR-161,without
amendments. OWNER_REVIEW E04/E05 now[x],73/75accepted,onlyD12/D13deferred.
Checks/JSON/bytefixtures/coverage above are unchanged document evidence;
acceptance adds no runtime verification. Earlier review-pending labels are
historical;DF01–05 operational/evidence prerequisites remain.

## 11. Acceptance recording validation — 03/10/2026

- ADR-160/161 unique and ordered E04 before E05; baseline stages done.
- 7 protected source files unchanged; original ADR001–159 byte prefix preserved.
- OWNER_REVIEW75 rows/73checked/2deferred; all75notes preserved and73otherrows
  unchanged. Only E04/E05 acceptance labels/checks updated.
- 19 affected documents:143 local file links resolve, Markdown table widths
  consistent,24 JSON examples parse. Earlier inventory/codec fixture checks
  remain document evidence; no new runtime verification implied.
- D12/D13 and DF01–05 retained; no code/SQL/Git/scaffold/deployment or
  implementation planning authorized or begun by this acceptance.
