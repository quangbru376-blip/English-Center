# E04 — SQL/EF integration, locks và audit

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản: E04-SC03-v1 trong E04-v1, 03/10/2026. **Accepted ADR-160; chưa SQL implementation.**
Phụ lục [E04](SYSTEM_ARCHITECTURE_SPEC.md), kế thừa [E03](DATABASE_SPEC.md)
24 tables/5 triggers/6 views/6 functions/7 procedures/4 roles và
[DTO D10](E04_DTO_SPEC.md), [action registry](E04_REPLAY_SPEC.md).
Tên/schema đề xuất dbo cho13 FN/SP; đây là signature tài liệu, không DDL.
Các kiểu/nullability ghi dưới đây không tự sửa bảng/fields E03.

[Final contract](E04_FINAL_CONTRACT.md) mục2/4/5 giải quyết current authorization
boundary, error mapping/replay gates/expired retained marker và exact ec_ role
names; không thêm SQL objects/DDL hoặc quyền từ review-ready status.

## 1. Shared calling contract và types

Service sở hữu connection/transaction/auth/projection; Repository không commit
riêng. SP đọc không DML/business audit/replay. SP07 chỉ allocator cùng TX04,
không approval/Invoice creator. actorId do verified service truyền; SQL runtime
credential dùng chung không chứng minh web actor. Browser không gọi SQL.

Ký hiệu N nonnull/Y nullable; int→C# int/JSON integer; bigint counts đọc Int64
và kiểm range ReportCount trước serialize, overflow500; decimal(5,2) score/
weight→C# decimal/fixed2 string; decimal(38,0) report amount **không map .NET
decimal** (chỉ28–29 digits theo [C# decimal](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types)). Report SP xuất amount varchar(38), exact digits
sau SQL SUM decimal38; repository validate pattern/range rồi giữ string.
Không double/JS Number/cast decimal18 trước SUM. Date/time datetime2(3)→UTC
DateTime marked UTC→Instant; SQL date→DateOnly; time(0)→TimeOnly HH:mm.
rowversion binary(8)→byte[8]→Base64; uniqueidentifier→Guid/lowercase D.
Strings nvarchar như E03 column widths (code points có thể2 UTF16 units),
không truncate args. DB NULL→nullable, không default0/date/empty string. SQL [SUM decimal result](https://learn.microsoft.com/en-us/sql/t-sql/functions/sum-transact-sql?view=sql-server-ver16) giữ precision38; bounded conversion thực hiện sau aggregate.

Typed params đúng SQL size/type/precision/scale; positional result sets đúng
thứ tự và column names dưới đây. Khi label `E03(Txx.field)` là **kiểu chính
xác và NULL kế thừa field** trong accepted dictionary, không wildcard SELECT.
Không serialize raw SQL record/EF entity ra web. Missing/extra result columns,
duplicate identity hoặc NULL trái contract→controlled500, no partial DTO/file.

Filters validated defaults ở service. Date inclusive VN chuyển half-open UTC
[startInclusive,endExclusive) một lần; bound SQL datetime2 range check400 trước
query. asOfUtc/todayVn do server một lần/request, không GETDATE trong mỗi row.
Literal contains escape SQL LIKE special characters kể cả escape character;
sort chỉ enum ordinal allowlist, parameter hóa values, không user SQL fragment.
Pagination page1/size20/max100; skip tính Int64 checked. Không page subset guard.

## 2. SP01–SP07 signatures và result sets

### SP01 dbo.usp_SearchAdmissions → API-038

Parameters (tất cả explicit): actorId int N; courseId int Y; status varchar(32)
Y; createdFromUtc/createdToExclusiveUtc datetime2(3) Y mỗi field;
name nvarchar(200) Y; email varchar(254) Y; phone varchar(10) Y;
sort varchar(16) N CREATED_ASC/CREATED_DESC; page/pageSize int N.
Actor hiện Admissions ACTIVE được Service kiểm; procedure defense recheck role,
không expose password/status/security context columns.

RS1 một row totalCount bigint N kể cả0. RS2 trang một row/Registration:
registrationId int N, studentId int N, fullName nvarchar(200) N,
email varchar(254) N, phone varchar(10) N,
courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
courseStatus E03(T05.status) N,
registrationStatus E03(T09.status) N, createdAt datetime2(3) N,
admissionConfirmedAt/admissionApprovedAt datetime2(3) Y,
bandId int Y, bandCode E03(T06.band_code) Y, bandMinScore decimal(5,2) Y,
invoiceId int Y, invoiceNumber E03(T20.invoice_number) Y,
invoiceAmount decimal(18,0) Y, invoiceStatus E03(T20.status) Y,
classId/assignmentId int Y, classCode varchar(30) Y,
classStatus E03(T14.status) Y, registrationVersion binary(8) N.
Nullable joined bundles all-null hoặc complete; page/count cùng snapshot.
Stable createdAt/id cùng chiều; Service map AdmissionsRegistrationSummary,
không add Payment/StudentProfile/academic grade columns.

### SP02 dbo.usp_GetEntryTestRoster → API-065/068

Parameters: sessionId int N, actorId int N, asOfUtc datetime2(3) N;
name nvarchar(200) Y, email varchar(254) Y, phone varchar(10) Y;
bookingStatus varchar(16) Y; view varchar(8) N current/history;
sort varchar(16) N BOOKED_ASC/BOOKED_DESC; page/pageSize int N.
Procedure resolves actual Account role AcademicAffairs/Teacher, Teacher current
Session.grader_id; not passed callerRole. No Session/out-of-task returns tagged
scope miss via service mapping404, **không** empty success pretending valid scope.

RS1 totalCount bigint N; RS2 one scoped Booking row:
bookingId/attemptId/registrationId/studentId/courseId int N;
attemptNumber int N; bookedAt datetime2(3) N;
bookingStatus varchar(16) N, registrationStatus E03(T09.status) N;
fullName nvarchar(200) N, email varchar(254) N, phone varchar(10) N;
courseCode varchar(30) N, courseName nvarchar(300) N,
courseStatus E03(T05.status) N;
targetScore decimal(5,2) Y, targetGradedAt datetime2(3) Y;
bandId int Y, bandCode E03(T06.band_code) Y, bandMinScore decimal(5,2) Y.
AcademicAffairs RS2 thêm invoiceStatus E03(T20.status) Y;
Teacher RS2 **không column invoiceStatus**, không NULL placeholder rights union.
Two keyless row shapes dispatch verified role. targetScore chỉ Booking COMPLETED
và Attempt.completed_booking_id=bookingId; current Band separate, no grade0.
Stable bookedAt/bookingId; history includes current SCHEDULED. FN01 uses same
asOfUtc trong scoped parent context query (không thêm effective state row DTO).

### SP03 dbo.usp_GetClassGradeReview → API-144 và authorized review reads

Parameters classId int N, actorId int N. Actor AcademicAffairs NORMAL/service
current access. All result sets cùng short SNAPSHOT, non-DRAFT values only.
Parent metadata/reference versions khác Grade DRAFT fields không phải override.

RS1 một row: classId/programId/teacherId int N, classCode varchar(30) N,
classStatus E03(T14.status) N, classVersion/programVersion binary(8) N,
cohortSize/visibleReportCount int N. Nếu không visible report thì service404.
Minimal parent Course/Band/Teacher references lấy trong cùng snapshot theo
AcademicClassSummary D10 bằng EF, không connection riêng sau snapshot.
RS2 full config: componentId int N, name nvarchar(200) N, kind varchar(16) N,
weight decimal(5,2) N, version binary(8) N; order componentId.
RS3 visible reports: classAssignmentId/registrationId/studentId/reportId int N,
fullName nvarchar(200) N, status varchar(16) N SUBMITTED/RETURNED/FINALIZED,
teacherComment nvarchar(2000) Y, submittedAt/reviewedAt/finalizedAt datetime2(3) Y,
latestReturnReason nvarchar(2000) Y, finalScore decimal(5,2) Y,
version binary(8) N, requiredComponentCount/enteredComponentCount int N,
isComplete bit N, total decimal(5,2) Y. Order classAssignmentId.
RS4 visible saved scores: classAssignmentId/scoreId/componentId int N,
score decimal(5,2) N, scheduleId int Y, enteredByAccountId int N,
enteredAt datetime2(3) N, version binary(8) N; order assignment/component.
RS5 linked schedule refs only visible score links: scheduleId/classId int N,
type varchar(16) N, obligationKey uniqueidentifier N, date date N,
startTime/endTime time(0) N, room varchar(30) N,
occurrenceState varchar(16) N, isCurrent bit N derived,
replacesScheduleId int Y, version binary(8) N; order scheduleId.
Service derives missingComponentIds from RS2−visible saved component set,
startAt/endAt from VN date/time; no DRAFT Score token in any RS.
No per-report action/status setter; full-cohort preview separately reloads guards.

### SP04 dbo.usp_GetFinancialReport → API-150/151/154

Parameters fromUtc/toExclusiveUtc datetime2(3) N, courseId int Y,
overdueOnly bit N, todayVn date N, page/pageSize int Y.
Both pagination ints N for API150/151; **both null** for CSV all filtered results.
No actor/student/invoice selector. Report projection non-identifying aggregate
for either caller; web role/export checked Service. Report readers SQL roles
get these aggregates only, no method/reference/source IDs.

RS1 one row: totalCount bigint N, revenueAmount/billedAmount/currentDebtAmount/
overdueDebtAmount varchar(38) N. RS2 one Course group row:
courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
four amount columns same varchar(38) N; order courseCode ASC/courseId ASC.
RS3 Course options: courseId int N, courseCode varchar(30) N,
courseName nvarchar(300) N, status E03(T05.status) N; scope D10 incl history.
Amounts exact decimal38 converted in SQL to digit strings after aggregate;
no scientific spelling. UNION cohort Course keys, totals independent pagination.
CSV Service buffers bounded encoded file per D10 before response headers,
not SP row cap/partial aggregation. QueriedAt/today/appliedFilters server context,
not SQL historical-as-of parameters. Empty RS1 four"0", RS2 empty, RS3 options.

### SP05 dbo.usp_GetAdmissionsReport → API-152

Parameters fromUtc/toExclusiveUtc datetime2(3) N, courseId int Y,
status E03(T09.status) Y, page/pageSize int N.
RS1 totalCount/registrationCount/distinctStudentCount bigint N (one row).
RS2 courseId int N, courseCode varchar(30) N, courseName nvarchar(300) N,
registrationCount/distinctStudentCount bigint N; order code/id ASC.
RS3 options same SP04. Whole distinct computed independently, not sum groups;
no Registration/Student/contact/Band score rows. Empty counts0, groups[].

### SP06 dbo.usp_GetTrainingReport → API-153

Parameters courseId/bandId/classId/teacherId int Y, status E03(T14.status) Y,
finalizedFromUtc/finalizedToExclusiveUtc datetime2(3) N, page/pageSize int N.
Inventory date-free, current Teacher for both cohorts.
RS1 totalCount/classCount/plannedCount/openCount/ongoingCount/completedCount/
cancelledCount/finalizedResultCount/passingResultCount bigint N,
passRatePercent decimal(5,2) Y (one row).
RS2 one matched-Class page row (same inventory/results group key):
classId/courseId/bandId int N, classCode/courseCode varchar(30) N,
courseName nvarchar(300) N, bandCode E03(T06.band_code) N,
bandMinScore decimal(5,2) N, classStatus E03(T14.status) N,
maxSize/placedCount/expectedCount int N, heldCount bigint Y,
progressPercent decimal(5,2) Y, examCurrentCount/examHeldCount int Y,
finalizedResultCount/passingResultCount bigint N, passRatePercent decimal(5,2) Y.
CANCELLED held/progress/exam counts NULL; results still finalized-cohort semantics.
Order classCode/classId ASC. RS3 courses same SP04;
RS4 bands bandId int N, bandCode E03(T06.band_code) N,minScore decimal(5,2) N;
RS5 classes classId/courseId/bandId int N,classCode varchar(30) N;
RS6 teachers teacherId int N,displayName nvarchar(200) N, referenced current
Class Teachers only. Full option sets no pagination. Service splits RS2 into
same-group pages; no single Student/Score/GradeReport identifiers.

### SP07 dbo.usp_AllocateInvoiceNumber → API-041/TX04

Parameter issueYear smallint N, server VN year1..9999 consistent issuedAt.
Requires caller TX; absent TX controlled failure, no independent BEGIN/COMMIT.
RS1 exactly one row invoiceNumber E03(T20.invoice_number) N varchar format
HD-YYYY-NNNNNNNN; counter T24 locked row/range, checked8digit overflow failure.
No invoiceId/accountId/replay key/customer/money inputs. Allocation +Invoice+
ADMITTED+audit+completion same TX, duplicate replay never invokes SP07.
Return new row only after caller commit to client, gaps allowed/no reuse.

## 3. FN01–FN06 signatures, NULL và real callers

| FN | Parameters (SQL) | Exact result | Callers / edge contract |
| --- | --- | --- | --- |
| FN01 dbo.ufn_EffectiveEntryTestState scalar | status varchar(16) N,startAt/asOfUtc datetime2(3) N | varchar(16) N OPEN/CLOSED/CANCELLED | Session options/detail/rosters SP02; CANCELLED first, now>=start CLOSED. Read-only no persisted transition. |
| FN02 dbo.ufn_BandForScore inline TVF | score decimal(5,2) N |0/1 row: bandId int N,minScore decimal(5,2) N | API070/072 TX03 BANDSET; highest threshold<=score. Outside0..100 invalid before call; no threshold match integrity failure, not guessed Band1. |
| FN03 dbo.ufn_RegistrationUnfinished inline TVF | registrationId int N |0/1 row:isUnfinished bit N | Registration create uniqueness/current predicate; finalize147/TX12. Missing Registration0 rows; CANCELLED/REJECTED false, otherwise false iff assigned Class COMPLETED and own GradeReport FINALIZED, passing independent. |
| FN04 dbo.ufn_ClassObligationProgress inline TVF | classId int N |0/1 row:expectedLessonCount int N,currentLessonCount/heldLessonCount bigint N,currentFinalExamCount/heldFinalExamCount int N | VW04/SP06 and Class preview/complete101; retained Class missing0 rows, current obligation single count; cancelled projection NULL done by SP06, not artificial0 history. |
| FN05 dbo.ufn_GradeTotal inline TVF | classAssignmentId int N |0/1 row:requiredComponentCount/enteredComponentCount int N,isComplete bit N,total decimal(5,2) Y | gradebook/draft/submit/review/finalize, SP03; count distinct configured component/same-assignment links, missing totalNULL. Complete weighted decimal intermediate (e.g decimal28,12), half-up once;49.995→50.00,49.994→49.99. Empty config incomplete totalNULL. No mutation final_score. |
| FN06 dbo.ufn_InvoiceDebtFlags inline TVF | status varchar(16) N,dueDate/todayVn date N |1 row:isOverdue bit N,daysOverdue int N | Invoice scoped reads/VW06/SP04; UNPAID due<today true/days difference, else false/0. Equality not overdue/no penalties/state changes. |

Missing0 rows distinct aggregate0 counts; Service treats missing command target
404, wrong relation/integrity500 where appropriate, never missing=default row.
All function outputs parity với Service guards; chỉ Service FINALIZE persist
rounded final_score. Views giữ grain E03; SQL queries không join raw Schedule/
Score/Booking làm nhân aggregate. Không UDF write/audit/time-dependent GETDATE.

## 4. EF result mapping và trigger writes

SP01–06 reads dùng distinct keyless no-tracking result types từng RS/role,
parameterized DbCommand trên caller connection/TX và sequential NextResult;
không dùng LINQ composition trên EXEC hoặc auto-map multiple result sets thành
một entity. Single flat view/UDF read có thể keyless query, không fake PK hoặc
tracked rows SaveChanges. Chính Service maps identity-safe DTOs sau all-set
validation. SP07 one allocator scalar row, không keyless business entity.
Theo [EF SQL queries](https://learn.microsoft.com/en-us/ef/core/querying/sql-queries)
và [keyless types](https://learn.microsoft.com/en-us/ef/core/modeling/keyless-entity-types).

Bảng có trigger: T06 Band/T07 TrainingProgram/T08 ScoreComponentConfig/
T17 Attendance/T18 Score. Thiết kế table-scoped UseSqlOutputClause(false)
cho đúng5 bảng, không disable toàn model hoặc chỉ khai HasTrigger rồi giả
provider10 tự chọn. INSERT/UPDATE phải nhận generated ID/new8byte RV trước
result/audit refs; rollback không reuse tracked generated state. T19 report
parent-touch thật trong TX11 mỗi actual score/comment mutation, rowversion
ConcurrencyToken; không set rowversion bằng client. Snapshot/RV returned
sau last DML. Nguồn [EF SQL Server triggers/OUTPUT](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/misc).

Program config swap names/kind: validate desired full set và locks trước DML;
remove obsolete unused components (no Score refs), temporarily rename retained
conflicting component names sang collision-free internal strings trong≤100codepoints
và neutralize FINAL_EXAM to GENERAL, then assign final desired names/kinds và
insert new rows. Intermediate steps uncommitted trong cùng TX, no audit per
scratch name, mandatory old/new source→desired once. No temporary ID/DB field
hoặc committed invalid config. TG02 must accept unused Program path; used
configuration bị chặn trước mọi phase. Unique constraint/trigger failure
rollback whole save, not order-dependent batch SaveChanges guess. Đây là đề xuất
EF write strategy cần SQL integration evidence, không executable instructions.

## 5. Grants chính xác theo24 tables/13 objects

Chỉ ec_app_runtime dùng app pool; principals login/secrets do D13/operator profile
cung cấp sau, không tạo users lúc thiết kế. Default không quyền ngoài allowlist.
S=SELECT/I=INSERT/U=UPDATE/D=DELETE; không blanket schema grants.

| Runtime tables | Grants / path |
| --- | --- |
| T01 Role,T06 Band | S; fixed seeds/thresholds không web writer. |
| T02 Account,T03 StudentProfile,T04 AuthSession | S/I/U scoped services; no D. |
| T05 Course,T07 Program | S/I/U; no D. |
| T08 Components | S/I/U/D, D chỉ whole unused DRAFT config service. |
| T09 Registration,T10 Session,T11 Attempt,T12 Booking,T13 BandAssignment | S/I/U; no D/history purge. |
| T14 Class,T16 Schedule | S/I/U; no D. |
| T15 Assignment | S/I immutable; no U/D. |
| T17 Attendance,T18 Score,T19 GradeReport | S/I/U, no D; no clear score/attendance API. |
| T20 Invoice,T21 Payment | S/I/U; domain Service limits immutable fields, no D. |
| T22 AuditLog | S/I append-only mandatory/security service; no U/D runtime. |
| T23 ReplayOperation | S/I only; no overwrite/update/delete active completion. |
| T24 InvoiceNumberCounter | No direct grant; SP07 ownership-chain allocator only. |

Runtime EXEC SP01–07; SELECT TVFs FN02–06, EXEC scalar FN01; SELECT
VW01–06 only scoped Repository real queries; no ALTER/control/DDL/db_owner/
sysadmin/backups/replay or audit maintenance. Grants table U không tự cho
người dùng quyền đổi mọi cột; web commands allowlists vẫn enforce. DB trigger
backstop không chứng minh service authorization. Seeds/migration/cleanup do
separate operator/schema principal, no creds in runtime config.

Demo roles giữ E03: finance_reporting SELECT VW06+EXEC SP04; academics_reporting
SELECT VW03/VW04+EXEC SP06; director_reporting EXEC SP04–06 **không SELECT
facts**. No SP01–03/07/FN caller/extra DML/grant qua khác role. Reporting base
Account/StudentProfile/Score/GradeReport không granted và explicit DENY theo
E03 minh chứng. Ownership chain có thể cho view đọc dù base DENY: review actual
columns/result sets trước pass, không chứng minh privacy bằng DENY alone.
Naming ec_ prefix đúng E03. Grant/revoke/deny demonstrations chỉ future isolated
test DB, không web rights/production logins. SP04–06 non-identifying signature
chung tránh director role chọn privileged output bằng parameter.

## 6. Transaction, locks và read isolation toàn command registry

R=EC:REQUEST:<actorId>:<action_code>:<UUID-lowercase> (dưới255 chars/noPII),
Exclusive/Transaction cho55 UUID commands; partition đúng T23 unique.
B=EC:BANDSET; T=EC:TIMETABLE; P=EC:PROGRAM:<id>; C=EC:CLASS:<id>;
G=EC:REG:<id>; I=EC:INVOICE:<id>; A=EC:ADMINSET.
Business gates Exclusive Transaction, resource string ordinal ASCII (không
culture/numeric order), R trước tất cả business gates. All writer paths chia
same names/DbPrincipal=public trong current DB. Check every negative applock
return rollback; không -3 tự rollback hoặc infinite default. Nguồn
[Microsoft sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16).
Timeout value hữu hạn thuộc hồ sơ SC04, chưa tự đặt số/D12.

| API commands (đủ55, không routes đọc) | Save unit / business gates ngoài R |
| --- | --- |
|008|TX01; A khi tạo Admin, unique account backstop.|
|009–013|TX13; A cho last-active/self serialization; Account target +all target sessions locks.|
|025|W05 atomic; T+G của cùng unfinished records có trước; Student/Course filtered-unique range lock cả absent future Registration, FN03.|
|028,044|TX06; T+G+I nếu Invoice tồn tại, C nếu assignment relation để check cancel eligibility.|
|030–032|TX02; T+G; source/target EntrySession rows locked, all Attempt/Booking full manifests reload.|
|036|W09 atomic; G; result/attempt/booking/Band projection context dưới same REG writer protocol.|
|041|TX04; G+I existing if any; Course row lock price/state cùng catalog update; counter year range lock trong SP07.|
|042|W10 atomic; G.|
|051–052|TX05 hoặc W12 metadata atomic; G+I; Invoice/Payment absence/live state links.|
|055,057–059,061,064|W06 atomic; T+G cho mọi linked active Booking Registrations bị ảnh hưởng; Session/target grader rows.|
|070–072|TX03; B+G; current Session/Booking/Attempt rows và Band projection, Session assignment resource relation ổn định.|
|075,077–079|W13 atomic; Course row/range khóa, no invented COURSE gate.|
|082,084,085,087–089|W13 atomic; B+P existing target; new Program pair unique/range lock và Course/Band rows. B chỉ cho config/first-use-sensitive checks, harmless rename không cần B.|
|092|TX07; P của selected Program, no new Class gate trước có ID; full config/Teacher account locks.|
|094,097,099–102|W14 atomic; T+C+P+G full assignment cohort khi predicates liên quan; complete101 reload Attendance/current HELD proof.|
|108|TX08; T+C+G+I của Registration, P config khi cần selected relation validation.|
|118,120,122,124,126|W16 atomic hoặc TX09; T+C+P+G full placed cohort, source/current Schedule dependency locks.|
|133|TX10; C+P; full cohort/current LESSON/Attendance absence and changed rows.|
|139|TX11; C+P; target Assignment/report/scores/current component+Schedule full sets; touch parent RV.|
|141,142,146,147|TX12; C+P+G full immutable cohort, all reports/scores; finalize unfinished under REG gates; no selected subset.|

Live resources set được discover trước TX bằng IDs trusted/scoped reads nhưng
chưa authority; sort/de-duplicate all gates, acquire, reload links/set. Nếu
reload làm tập gate khác (new Booking/cohort/link): rollback/reprepare **trước
DML**, không thêm resource out-of-order. No auto-repeat user intent/secret.
Absent Invoice/Payment/Booking/Score/member locks bằng parent gate+predicate
range/unique, không invent RV absent row. New IDs refs server-owned after DML.

Cross-auth lock order để không deadlock Account reset và business gate:
request→sorted business gates→all involved Account rows ascending accountId
(including caller +target Teachers/Admin)→AuthSession rows deterministic sid
binary ascending→domain parent/children consistent table-id/PK ascending.
TX13/reset/login/idle/logout dùng same Account→session order (no own reverse
session→Account lock). Recheck actor session/current Account/security_version/
role/expiry sau locks, giữ tới commit. Target account status guard khóa cùng
writers lock/disable; revoke-all chờ transaction đang hợp lệ commit hoặc thắng
trước rồi business rollback. Không ignore expired/revoked vì đã authenticate.
Time check immediately before DML; preview/versions không freeze clock.

Commands READ COMMITTED với UPDLOCK/HOLDLOCK target/ranges cần predicate guard,
shared gates đến commit; no NOLOCK/SNAPSHOT-only writes vì stale snapshot
không serialize mutation. Compound detail/preview/search với count/children,
reports/CSV SP01–06 dùng short caller SNAPSHOT (ALLOW_SNAPSHOT_ISOLATION là
prerequisite chưa bật); lookup request gate khi needed, current rights vẫn
live đọc cùng coherent scope. Auth status không được authorize từ report
snapshot cũ. Không giữ TX qua confirmation/download user think time.
Single simple read có thể one-statement READ COMMITTED; mọi related guard
manifest cần coherent snapshot. Query cancellations dispose/rollback; commit
uncertain vẫn SC02 UNKNOWN/no automatic retries.

## 7. Audit event field allowlists nối55 commands

Common columns T22: actor verified/null unauthenticated, action allowlist≤80,
entity_type/id đúng refs, result SUCCESS khi committed, occurred_at UTC,
operation_id same operation; mandatory_category MANDATORY/security expiryNULL.
No payload/digest/credentials/hash/JWT/CSRF/raw sid/password/entire profile;
no extra IP cho business history mặc định. Required reason chỉ schema đã chốt.
Audit event action dùng canonical command string SC02; dependent events thêm
suffix `.EFFECT` cùng≤80, entity separates child changes. Details exact
`old`/`new` allowlist dưới đây, `reason` chỉ lệnh required, `parent` IDs cần
liên kết. Unknown key không ghi. Audits không replay response snapshot.

| APIs | Fields allowed old/new và cardinality |
| --- | --- |
|008|Account ref/role/status/mustChangePassword/tempIssuedAt/tempExpiresAt; new only; no email/fullName/phone/temporaryPassword.|
|009–013|Account status/mustChangePassword/temp timestamps/securityVersion, revokedSessionCount; no sid/hash/secret.|
|025,028,036,041,042,044|Registration status/isUnfinished/confirmation-approved-cancel-reject timestamps; parent courseId; reason only042/044; dependent bookingId/state/cancel metadata and Invoice ID/status/amount/dueDate/number when applicable. No personal snapshots.|
|030–032|Attempt ID/number, Booking source-target IDs/session IDs/status/booked/cancel timestamps; Registration state effect when first booking. No contact/score0 for absence.|
|051,052|Invoice/Payment refs, amount/status/method/transactionReference/paidAt; correction only method/reference old-new+required reason, immutable values never written again.|
|055,057–059,061,064|Session date/start/end/room/capacity/graderId/status; reason061/064;061 grouped affected Booking state/cancel metadata per child.|
|070–072|Booking state/completed ref, Attempt score+original grader/time, bestAttemptId/Band ID+threshold/state;072 reason+old/new score/best/Band;071 absence actor/time only/no fake score.|
|075,077–079|Course code/name/basePrice/status.|
|082,084,085,087–089|Program courseId/bandId/name/status/expectedLessonCount; component id/name/kind/weight source→desired per affected component, no intermediate temporary names.|
|092,094,097,099–102|Class programId/teacherId/maxSize/status;102 reason; schedule resource-release refs/history when cancel.|
|108|Class/Registration/Assignment refs/assignedBy/At; new DRAFT GradeReport ID/state (no default values).|
|118,120,122,124,126|Schedule classId/type/date/start/end/room/obligationKey/replacesScheduleId/occurrenceState/occurrence timestamps;124 required reason, old NOT_HELD+new row linked;120 per schedule effect events parent operation;126 manual HELD event.|
|133|Attendance latest enteredBy/At +value stored T17; no additional mandatory AuditLog event presumed from required score/config audit. T23 still atomic TX10. If audit coverage owner adds Attendance history it is new policy, not silently marked accepted.|
|139|Target report ref/comment old-new and each actual Score componentId/scheduleId/value enteredBy/At change; required grade history source, no full peer/cohort payload.|
|141,142,146,147|Class parent event +each GradeReport status/submitted-reviewed-finalized metadata,146 reason;147 final_score +Registration unfinished old-new grouped; no one summary losing individual history.|

Mandatory failures rollback whole operation including T23, counter/all children;
no event on completed replay/accepted no-op under ADR-158/159. Pure auth/login/logout/denied access
SECURITY best-effort/fallback secret-free, actorNULL when unauthenticated, no
guessed email→actor; mandatory Account mutations separate. Security logs/IP90d,
mandatory history retained; maintenance principal only, no app export/purge.
Detail API-016 allowlists same event definitions, not unrestricted stored JSON.

## 8. Kiểm chứng cần có và trạng thái

Thiết kế đã nối55 commands,13 FN/SP signatures/result types,5 EF-trigger
mappings,24-table grants,locks/isolation/audit. **Chưa bằng chứng chạy được**:
real SQL app callers/sequential sets/decimal38-string,trigger multirow/generated
RV,config swaps,auth-vs-revoke deadlock order,last-slot/first-use/absence/cohort
races,allocator/audit forced rollback,report snapshots/privacy/ownership-chain
grants. Exact runtime timeout/profile thuộc SC04/D12/D13. SC01/final contract
đã rà domain/bindings/field-family/result/role parity để owner đọc actual E04-v1;
signature count không nghiệm thu SC03/E04 hoặc chứng minh implementation đúng.
