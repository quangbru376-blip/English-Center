# E03 — Thiết kế cơ sở dữ liệu

Phiên bản: E03-v1, ngày01/10/2026. Trạng thái: **owner nghiệm thu toàn bộ bản thực tế ngày01/10/2026 — Accepted**.

Các gói A–G2 trong phạm vi đã trình được duyệt ADR-131–138. Bản này tổng hợp
ERD/dictionary/constraint/SQL/quyền/giao dịch/rubric/trace; mục14 nêu chi tiết
vật lý, kết quả kiểm tài liệu và ranh giới chuyển E04. Sau G2, owner đã đồng ý
nghiệm thu **toàn bộ E03-v1 thực tế** trong chat; E03 [x]/stage3 done. Lời duyệt
này không cấp quyền SQL/code hoặc xác nhận test pass; tiếp thiết kế E04.

Nguồn: [requirements](../requirements.md), [DECISIONS](../DECISIONS.md),
[FUNCTION_SPEC E01-v1](FUNCTION_SPEC.md), [WORKFLOW_SPEC E02-v1](WORKFLOW_SPEC.md).
Rubric: [DBMS330284](<../../instructions/HuongDan_Project_Rubric_DBMS330284.docx (1).pdf>),
trang 1–3, mục 3–4, được áp dụng theo ADR-130. Tài liệu này mô tả thiết kế;
không có DDL, stored procedure, trigger, migration hoặc ứng dụng được tạo/chạy.

## 1. Phạm vi, giả định và sổ quyết định

Ngày 01/10/2026 owner chọn cách 1: theo miền nghiệp vụ, kèm bảng đối chiếu rubric.
Mỗi bảng có từ điển trường và ràng buộc; phần xuyên suốt mô tả giao dịch,
lịch sử, concurrency, SQL object và quyền DB. E01/E02 và ADR-001–130 là
ràng buộc đầu vào. Owner duyệt gói A đã trình ngày01/10/2026 (ADR-131): mô
hình24 bảng, hai store kỹ thuật, quan hệ/current-profile/tuition snapshot,
current Schedule projection và parent rowversion. Gói B đã trình được duyệt
cùng ngày (ADR-132): tài khoản/profile/session, kiểu lưu đã nêu và giao dịch
nhất quán. Phần C đã trình được duyệt ADR-133 cùng ngày, gồm cấu hình,
tuyển sinh/thi đầu vào và PENDING→TESTING. Phần D đã trình duyệt ADR-134:
Class/xếp lớp/current Schedule/history và tạo DRAFT report cùng xếp lớp.
Phần E đã trình duyệt ADR-135: Attendance/Score/GradeReport, final_score chỉ
lưu khi FINALIZED, phiên bản aggregate và lịch sử. Phần F đã trình duyệt ADR-136:
Invoice/Payment, audit allowlist, safe replay và counter theo năm VN. G1 danh
mục/mục đích/đường gọi SQL objects đã trình duyệt ADR-137. G2 cơ chế quyền DB/
giao dịch/khóa/snapshot/index/kiểm chứng đã trình duyệt ADR-138. Các chi tiết
vật lý không được duyệt riêng đã được chốt qua lời nghiệm thu toàn bộ bản
E03-v1 thực tế, gồm mục14.2. E03 Accepted/stage3 done; SQL signatures/mapping/
grants/runtime và bằng chứng chạy vẫn thuộc E04/later.

Một trung tâm, phạm vi kiểm chứng 10.000 học viên/50 người đồng thời (ADR-117),
SQL Server 2025 Express đang có cho dev/test/demo (ADR-162 thay SQL choice
của ADR-119); xem [assessment](../plan/SQL_INSTANCE_ASSESSMENT.md). Target DB/
EF compatibility160, không đổi các kiểu/constraints E03; chưa cấu hình DB.
Hiệu năng chưa được đo.
Quyền/ownership, bảo vệ credential và audit theo ADR-092/099/115/116; phiên
thường 2 giờ/idle 30 phút, phiên hạn chế tối đa 10 phút. Laptop chạy theo buổi;
backup và owner vận hành theo ADR-118/120, D12/D13 giữ hoãn. Không phát sinh
multi-tenant, version cấu hình, phòng/ngày nghỉ, học bổng, refund/chuyển lớp,
retake sau CONFIRMED, quyền xóa lịch sử hoặc thang điểm đào tạo khác 0–100.

| Nội dung | Tình trạng | Phương án/lý do |
| --- | --- | --- |
| Áp rubric kỹ thuật DB và tích hợp ứng dụng | Đã duyệt ADR-130 | Dùng tiêu chí môn DBMS, giữ nguồn nghiệp vụ hiện có; quy định nộp báo cáo riêng không được nhập vào sản phẩm. |
| Tổ chức tài liệu | Owner chọn cách 1 ngày 01/10/2026 | Miền nghiệp vụ + đối chiếu rubric; so với SQL-object-first ít phân tán hành trình; so với use-case-first ít lặp bảng/ràng buộc. |
| 22 thực thể + 2 bảng kỹ thuật; current Schedule và parent rowversion | Đã duyệt gói A, ADR-131 | ReplayOperation/InvoiceNumberCounter cụ thể hóa ADR-090/093; current flag bảo vệ unique/history, parent version phát hiện child edits. |
| Account/Profile/AuthSession trong phạm vi đã trình | Đã duyệt gói B, ADR-132 | Tách dữ liệu Student và security state; kiểm hai version/giao dịch chung tránh ghi đè; dung lượng hash chờ kiểm chứng runtime. |
| Cấu hình/Registration/ca-lượt-booking/Band trong phạm vi đã trình | Đã duyệt gói C, ADR-133 | Lịch sử riêng, khóa cấu hình theo sử dụng; PENDING→TESTING làm rõ đường còn thiếu ở E02. |
| Class/xếp lớp/Schedule trong phạm vi đã trình | Đã duyệt gói D, ADR-134 | Sĩ số từ assignment, DRAFT report tạo cùng xếp lớp; chuỗi lịch giữ một nghĩa vụ. |
| Attendance/Score/GradeReport trong phạm vi đã trình | Đã duyệt gói E, ADR-135 | Thiếu khác0/vắng; total trước chốt được tính, latest metadata không thay audit lịch sử. |
| Invoice/Payment/Audit/replay/counter trong phạm vi đã trình | Đã duyệt gói F, ADR-136 | Snapshot tiền, audit cùng transaction, replay không secret, counter không tái dùng số đã phát hành. |
| Field/type/NULL/default/FK/unique/index còn lại | Owner nghiệm thu E03-v1 thực tế, gồm mục14.2 | Chốt thiết kế dictionary/constraints; index vẫn là candidates cần chọn/đo, không SQL triển khai hoặc blanket approval E04. |
| SQL objects trong phạm vi đã trình | Đã duyệt gói G1, ADR-137 | 5 trigger/6 view/6 UDF/7 SP có mục đích/đường gọi thực tế; giữ auth/audit ở service. |
| Quyền DB và enforcement/verification trong phạm vi đã trình | Đã duyệt gói G2, ADR-138 | Transaction/gates/snapshot/index/rubric; E04 chốt DTO/signature/mapping/quyền chi tiết/timeout, chưa bằng chứng chạy. |

## 2. Gói A — mô hình tổng thể và quy ước

### 2.1. Danh mục 24 bảng và chủ sở hữu dữ liệu

Tên bảng đề xuất là số ít, schema `dbo`; module là ownership của ứng dụng,
không yêu cầu tách database hoặc SQL schema. PK/FK dùng tên trong từ điển.

| Miền | Bảng | W liên quan |
| --- | --- | --- |
| Administration / Students | Role, Account, StudentProfile, AuthSession | W01–W04 |
| Admissions | Registration, EntryTestSession, EntryTestAttempt, EntryTestBooking, BandAssignment | W05–W10 |
| Academics / cấu hình | Course, Band, TrainingProgram, ScoreComponentConfig | W13 |
| Academics / vận hành | Class, StudentClassAssignment, Schedule, Attendance, Score, GradeReport | W14–W19 |
| Finance | Invoice, Payment, InvoiceNumberCounter | W10–W12 |
| Xuyên suốt / Administration điều phối | AuditLog, ReplayOperation | W01–W21 |

Course/Band là dữ liệu Academics; Admissions chỉ tham chiếu. Reports đọc dữ liệu,
không sở hữu bảng fact sao chép hoặc snapshot cá nhân. Hai bảng kỹ thuật không
được biến thành màn hình, quyền mới hoặc entity sản phẩm thứ 23/24.

### 2.2. ERD tổng thể

```mermaid
erDiagram
    Role ||--o{ Account : role_id
    Account ||--o| StudentProfile : account_id
    Account ||--o{ AuthSession : account_id
    StudentProfile ||--o{ Registration : student_id
    Course ||--o{ Registration : course_id
    Course ||--o{ TrainingProgram : course_id
    Band ||--o{ TrainingProgram : band_id
    TrainingProgram ||--o{ ScoreComponentConfig : program_id
    TrainingProgram ||--o{ Class : program_id
    Account ||--o{ Class : teacher_id
    Account ||--o{ EntryTestSession : grader_id
    Registration ||--o{ EntryTestAttempt : registration_id
    EntryTestAttempt ||--o{ EntryTestBooking : attempt_id
    EntryTestSession ||--o{ EntryTestBooking : session_id
    EntryTestBooking o|--o| EntryTestAttempt : completed_booking_id
    Registration ||--o| BandAssignment : registration_id
    EntryTestAttempt ||--o| BandAssignment : best_attempt_id
    Band ||--o{ BandAssignment : band_id
    Registration ||--o| Invoice : registration_id
    Invoice ||--o| Payment : invoice_id
    Registration ||--o| StudentClassAssignment : registration_id
    Class ||--o{ StudentClassAssignment : class_id
    Class ||--o{ Schedule : class_id
    Schedule o|--o| Schedule : replaces_schedule_id
    Schedule ||--o{ Attendance : schedule_id
    StudentClassAssignment ||--o{ Attendance : class_assignment_id
    StudentClassAssignment ||--o{ Score : class_assignment_id
    ScoreComponentConfig ||--o{ Score : component_id
    Schedule o|--o{ Score : schedule_id
    StudentClassAssignment ||--o| GradeReport : class_assignment_id
    Account o|--o{ AuditLog : actor_account_id
    Account ||--o{ ReplayOperation : actor_account_id
```

Sơ đồ bỏ các FK actor phụ để dễ đọc; từ điển phần 3–8 liệt kê đầy đủ.
InvoiceNumberCounter không có FK sang Invoice: đây là bộ cấp số, không là
bản ghi invoice. Mỗi child có parent bắt buộc trừ FK được ghi NULL rõ ràng.
GradeReport tối đa một/assignment theo cardinality; đã duyệt tạo DRAFT cùng
giao dịch xếp lớp để mỗi assignment đang học có report (ADR-134).

### 2.3. Quy ước kiểu, NULL và default

| Nhóm | Thiết kế và nguồn |
| --- | --- |
| PK nghiệp vụ | `int IDENTITY(1,1) NOT NULL`; không tái dùng ID, không coi liên tục là invariant (ADR-090). |
| ID kỹ thuật | `uniqueidentifier` cho request/operation/obligation; server tạo operation/obligation ngẫu nhiên. `AuthSession.sid binary(32)` là đề xuất định danh phiên ngẫu nhiên 256 bit, không hiển thị ra UI. |
| Tiền | `decimal(18,0)`, >0; phép SUM cast đầu vào sang `decimal(38,0)` trước tổng. API vẫn chuỗi số (ADR-091). |
| Điểm/weight | `decimal(5,2)`; score 0–100, weight >0–100; trung gian weighted sum đề xuất `decimal(28,10)`, giữ chính xác tích/hệ số trước half-up cuối. Chặn input dư hai số lẻ trước khi DB convert, không để DB tự làm tròn (ADR-028/091). |
| Sự kiện | UTC `datetime2(3)`; ngày nghiệp vụ `date`; giờ lịch VN `time(0)` với giây=0, cùng ngày và start<end (ADR-088/091). |
| Unicode | Hạn 100/150/255/1000 code point cấp `nvarchar(200/300/510/2000)`; NFC và code-point validator tại service. `Latin1_General_100_CI_AS_SC` đề xuất cho tên; mã dùng so sánh ASCII chuẩn hóa. Không dùng `LEN` đơn lẻ để thay validator. |
| Email/mã/phòng | ASCII `varchar`, email chuẩn hóa trim/lowercase và unique CI; mã/phòng uppercase theo ADR-075/079/080/091. Không sửa email hoặc Course/Class code hiện có. |
| NULL | Chưa xảy ra sự kiện → cả bundle actor/time/reason/prior-state NULL; khi xảy ra → actor/time bắt buộc. Một Score/Attendance chưa ghi là thiếu row; giá trị 0/vắng vẫn hợp lệ khi chủ động nhập. |
| rowversion | Mọi bảng mutable trong từ điển có `row_version rowversion NOT NULL`, tự sinh; đây là concurrency token, không là thời gian. Component mutation phải chạm Program version; Score/comment save phải chạm GradeReport version kể cả chỉ đổi Score. Immutable/append-only tables không cần form token. [Microsoft rowversion](https://learn.microsoft.com/en-us/sql/t-sql/data-types/rowversion-transact-sql?view=sql-server-ver16). |
| Default | Chỉ server/DB cấp ID, initial state/counters và thời điểm khi được mô tả. Không default điểm, điểm danh, actor, Band, Invoice amount hoặc Payment amount. |
| Xóa/FK | FK `NO ACTION`; không cascade xóa lịch sử. Không có endpoint delete dữ liệu lịch sử. Chỉ cấu hình nháp chưa dùng/không phụ thuộc được xóa khi quyền hiện có cho phép. |

`NOT NULL` không tự đảm bảo chuỗi không rỗng, enum đúng, role đúng hay quyền.
PK/FK/unique/check bảo vệ cấu trúc; service giao dịch bảo vệ quyền và predicate
liên bảng; trigger phần 10 là lớp chặn một số invariant cụ thể.

## 3. Gói B — tài khoản, hồ sơ và phiên

### 3.0. Phần B đã trình được duyệt ngày01/10/2026 — ADR-132

Owner duyệt phần gói B đã trình ngày01/10/2026 (ADR-132). T01–T04 diễn đạt
thiết kế đó; chi tiết bổ sung/index/CHECK/protocol đã được tổng hợp và owner
nghiệm thu cùng E03-v1. Mapping/enforcement thực thi và hash capacity cần
E04 hoặc kiểm chứng runtime, không mở lại nghiệm thu E03.

Nhóm này gồm Role, Account, StudentProfile và AuthSession. Bảy role cố định
được seed với unique role_code, Account có một role_id bắt buộc; không hardcode
ID của role. Account giữ email/name/phone/hash và security state; StudentProfile
giữ DOB/address, account_id unique. SĐT có thể NULL với staff nhưng service
bắt buộc với Student; unique có điều kiện bảo vệ các số thực sự có giá trị.

Đề xuất kiểu vật lý: email varchar254, phone varchar10, name nvarchar200,
address nvarchar510, hash varchar512 và security_version bigint khởi tạo1.
Storage đủ Unicode code point đã chốt; hash format/dung lượng phải được
xác minh khi khóa runtime. Temp issue/expiry chỉ có khi forced-change=true,
hai mốc đủ bundle và chênh24h; login failure window/throttle có cột riêng.

AuthSession dùng sid binary32 ngẫu nhiên256bit, account FK/version snapshot,
UTC issued/expires/activity/revoked và scope NORMAL/PASSWORD_CHANGE. Index
account/revoked phục vụ revoke; expiry index phục vụ vận hành. Kiểm DB trên
mỗi authenticated request theo chính sách đã duyệt; cập nhật activity đơn
điệu, không overwrite revoke hoặc gia hạn expires.

Tự đăng ký Student tạo Account+Profile+audit bắt buộc cùng transaction.
Profile edit recheck **cả** Account/Profile rowversions rồi lưu thay đổi name/
phone/DOB/address all-or-none. Admin status/credential changes và revocation
giữ transaction chung theo W03; role/email/status/credential không đi qua
generic profile patch. T01–T04 bên dưới là dictionary để đối chiếu khi rà tổng hợp.

Quy ước cột: `N` = NOT NULL; `Y` = NULL có chủ đích. FK không cascade.
`ID` trong cột khóa nghĩa là `IDENTITY(1,1)`; `RV` là rowversion tự sinh.

### T01 — Role

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| role_id | int | N | PK, ID | Khóa nội bộ. |
| role_code | varchar(32) | N | UQ_Role_Code | Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin. |
| role_name | nvarchar(200) | N | — | Nhãn Việt ngữ, giới hạn kỹ thuật đề xuất 100 code point. |

Seed đúng bảy role; code CHECK trong tập trên. Không CRUD role/RBAC động;
không hardcode role_id. Tài khoản có đúng một FK role (W01–W03).

### T02 — Account

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| account_id | int | N | PK, ID | Khóa Account. |
| role_id | int | N | FK Role | Role cố định của tài khoản. |
| email | varchar(254) | N | UQ_Account_Email | ASCII chuẩn hóa, bất biến; unique toàn Account kể cả blocked. |
| phone | varchar(10) | Y | UQ_Account_Phone khi khác NULL | Chuẩn 0xxxxxxxxx; service bắt buộc đối với Student. |
| password_hash | varchar(512) | N | — | Chỉ PasswordHasher output; dung lượng kỹ thuật đề xuất, xác minh khi khóa runtime. |
| full_name | nvarchar(200) | N | — | 1–100 code point. |
| status | varchar(16) | N | default ACTIVE | ACTIVE/LOCKED/DISABLED. |
| created_at | datetime2(3) | N | server UTC | Thời điểm tạo. |
| security_version | bigint | N | default 1 | Tăng khi phải revoke mọi session; không giảm. |
| failed_login_count | int | N | default 0 | Bộ đếm độc lập manual lock. |
| failure_window_started_at | datetime2(3) | Y | — | Cửa sổ sai credential. |
| login_throttled_until | datetime2(3) | Y | — | Hạn throttle login mới; không tự unlock Account. |
| must_change_password | bit | N | server | Student password thường false; cấp/reset tạm true. |
| temp_issued_at | datetime2(3) | Y | — | Bắt buộc khi must_change_password=true. |
| temp_expires_at | datetime2(3) | Y | — | issued+24h; cả hai NULL khi credential thường. |
| row_version | rowversion | N | RV | Admin/profile/security update recheck ở service. |

CK: valid status, version>0, counter≥0, temp bundle đầy đủ và đúng 24h,
phone ASCII 10 số/bắt đầu 0 nếu có. Validate email/NFC/surrogate/name/password
ở service trước ghi. Trạng thái/role/email/credential không được cập nhật
qua generic profile patch. Index `(role_id,status,account_id)` cho guard
last ACTIVE Admin và danh sách. Login counter update phải atomic; thành công/
hết hạn reset theo ADR-052. Một Account Student ↔ một StudentProfile cùng
giao dịch; Account staff không có StudentProfile.

### T03 — StudentProfile

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| student_id | int | N | PK, ID | Mã học viên hiển thị là ID này. |
| account_id | int | N | FK Account, UQ_StudentProfile_Account | Chỉ Account Student. |
| date_of_birth | date | N | — | Không sau ngày VN hiện tại, không giới hạn tuổi. |
| address | nvarchar(510) | Y | — | ≤255 code point, optional-empty→NULL. |
| row_version | rowversion | N | RV | Sửa DOB/address cùng Account name/phone theo W04. |

Service kiểm role Student và ngày VN; profile edit cần expected rowversion
của cả Account và StudentProfile, dù form chỉ đổi một phía. Lịch sử Registration dùng Account/Profile
hiện tại; không sao chép họ tên/DOB/phone/address vào Registration/Invoice.

### T04 — AuthSession

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| sid | binary(32) | N | PK | Random 256 bit, đề xuất vật lý của khóa phiên ADR-092. |
| account_id | int | N | FK Account | Chủ session. |
| issued_at | datetime2(3) | N | server UTC | Login time. |
| expires_at | datetime2(3) | N | server | Normal issued+2h; restricted min(issued+10min,temp expiry). |
| last_activity_at | datetime2(3) | N | initial issued_at | Chỉ request nghiệp vụ do user thao tác, cập nhật đơn điệu. |
| revoked_at | datetime2(3) | Y | — | Logout hoặc revocation. |
| security_version | bigint | N | snapshot Account | Phải bằng version hiện tại khi xác thực. |
| session_scope | varchar(24) | N | server | NORMAL/PASSWORD_CHANGE; là phạm vi kỹ thuật, không role mới. |
| row_version | rowversion | N | RV | Không đưa vào form nghiệp vụ; activity update không overwrite revoke. |

CK scope, issued≤activity≤expires, expires>issued, revoked≥issued nếu có.
Index `(account_id,revoked_at)` phục vụ revoke và `(expires_at)` phục vụ bảo trì.
Every authenticated request kiểm Account ACTIVE/version, revoke, absolute/idle
expiry và scope. Không gia hạn expires, không refresh hoặc hồi sinh session.
Không lưu cookie/JWT/CSRF/plaintext password trong bảng này.

## 4. Gói C — cấu hình và tuyển sinh/thi đầu vào

### 4.0. Phần C đã trình được duyệt ngày01/10/2026 — ADR-133

Owner duyệt phần C đã trình ngày01/10/2026 (ADR-133). PENDING→TESTING đã
đồng bộ requirements/E02; chi tiết bổ sung đã được nghiệm thu cùng E03-v1.
SQL syntax/mapping và bằng chứng runtime còn thuộc E04/later.

Gói C gồm T05–T13; các chính sách nghiệp vụ đã duyệt là đầu vào. Course giữ
mã/tên/giá/trạng thái; Band giữ mã/ngưỡng tối thiểu; TrainingProgram unique
theo Course/Band và sở hữu ScoreComponentConfig. Tên khóa/chương trình dùng
nvarchar(300), tên component nvarchar(200); tiền decimal(18,0), điểm/weight
decimal(5,2). UQ/CHECK bảo vệ mã, cặp liên kết, loại/range và tối đa một
FINAL_EXAM component; service kiểm đủ một và tổng weight=100 khi ACTIVE.
Freeze kiểm bằng lịch sử sử dụng, không thêm frozen flag dễ lệch; serialize
first use với sửa cấu hình theo cơ chế G2 đã duyệt; protocol chi tiết ở mục9.

Registration dùng filtered unique Student/Course khi is_unfinished=1; lưu
actor/time/prior-state/reason/operation của hủy/từ chối theo quyền hiện có.
Cụ thể hóa đã duyệt: PENDING mới tạo → TESTING khi booking đầu tiên →
BAND_ASSIGNED khi có điểm. Lượt hai giữ BAND_ASSIGNED và vẫn chặn confirmation
khi còn chờ; hủy/vắng không kéo lùi trạng thái hoặc xóa Band. Đường PENDING
trước đây chưa được ma trận E02 liệt kê, đã đồng bộ theo ADR-133.

EntryTestSession giữ giờ UTC từ lịch VN, phòng/grader/capacity1–20/state;
effective CLOSED được suy theo giờ, không cột phụ hoặc booked_count cache.
Attempt chỉ lưu lượt1–2 và result bundle; booking giữ từng lịch hẹn, đổi ca
tạo row mới. Unique bảo vệ một booking đang đặt/một booking hoàn thành mỗi
lượt; composite FK chặn result trỏ booking của lượt khác và best attempt của
hồ sơ khác. BandAssignment có một row/hồ sơ khi có kết quả, không copy
best_score; chấm/sửa cập nhật result + best/Band + audit cùng giao dịch.
Vắng/chưa chấm không có điểm, không tự ghi0. Chi tiết dictionary theo sau;
SQL objects/chống tranh chỗ/xung đột tài nguyên theo G1/G2 đã trình duyệt;
chi tiết vật lý được rà cùng bản tổng hợp, không phải bằng chứng chạy.

### T05 — Course

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| course_id | int | N | PK, ID | Khóa nội bộ. |
| course_code | varchar(30) | N | UQ_Course_Code | Mã uppercase 1–30; bất biến/không tái dùng. |
| course_name | nvarchar(300) | N | — | 1–150 code point; không global unique. |
| base_price | decimal(18,0) | N | — | Dương, VND; không default 0. |
| status | varchar(16) | N | server | ACTIVE/INACTIVE; explicit khi tạo. |
| row_version | rowversion | N | RV | Sửa giá/name/status theo W13. |

CK price>0, valid status và mã ASCII hợp lệ. Giá thay đổi không sửa Invoice đã
phát hành. Index `(status,course_id)`; FK Course từ Program/Registration có index.

### T06 — Band

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| band_id | int | N | PK, ID | Khóa nội bộ. |
| band_code | varchar(30) | N | UQ_Band_Code | Mã kỹ thuật đề xuất ≤30 ASCII; UI dùng nhãn Band tương ứng. |
| min_score | decimal(5,2) | N | UQ_Band_MinScore | Ngưỡng tối thiểu 0–100; không max_score. |
| row_version | rowversion | N | RV | Freeze toàn bộ set sau completed entry result đầu tiên. |

Seed Band 1–5 với 0/25/45/65/85 (ADR-028); không hardcode ngưỡng trong thuật
toán. Cấu hình hợp lệ phải phủ score=0; chọn max min_score≤score. CK range,
unique ngưỡng. Trigger TG03 và first-result/config transaction bảo vệ freeze.

### T07 — TrainingProgram

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| program_id | int | N | PK, ID | Khóa nội bộ. |
| course_id | int | N | FK Course | Khóa liên kết; frozen sau first Class. |
| band_id | int | N | FK Band | Band liên kết; frozen sau first Class. |
| program_name | nvarchar(300) | N | — | 1–150 code point. |
| status | varchar(16) | N | default DRAFT | DRAFT/ACTIVE/INACTIVE. |
| expected_lesson_count | int | N | — | >0, chỉ LESSON, frozen. |
| row_version | rowversion | N | RV | Config/status transaction. |

UQ `(course_id,band_id)` cả khi INACTIVE; CK valid state/count>0. ACTIVE cần
cấu hình hợp lệ, weight sum đúng 100 và đúng một FINAL_EXAM component.
Trước first Class, cấu hình ACTIVE muốn sửa phải về DRAFT; first-use freeze
khóa course/Band/list/kind/weights/count, không cấm quản lý status được phép.
Không lưu frozen flag dễ lệch; kiểm existence Class kể cả CANCELLED. Index
band_id và `(status,program_id)` bổ sung UQ course/Band.

### T08 — ScoreComponentConfig

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| component_id | int | N | PK, ID | Khóa component. |
| program_id | int | N | FK TrainingProgram | Parent configuration. |
| component_name | nvarchar(200) | N | UQ trong program, CI_AS_SC | 1–100 code point; không phân biệt case, có phân biệt dấu. |
| weight | decimal(5,2) | N | — | >0 và ≤100. |
| kind | varchar(16) | N | — | GENERAL/LESSON/FINAL_EXAM. |
| row_version | rowversion | N | RV | Config edit trước freeze. |

UQ `(program_id,component_name)`; filtered UQ program_id khi kind=FINAL_EXAM
bảo đảm tối đa một, service bảo đảm có một khi ACTIVE. TG02 bảo vệ toàn danh
sách đã dùng. LESSON component không tự tạo một điểm cho mỗi buổi; Score vẫn
unique assignment/component. Không tự bắt mọi Student dùng cùng schedule
cho một LESSON component nếu baseline chưa quy định binding đó.

### T09 — Registration

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| registration_id | int | N | PK, ID | Một lượt đăng ký/học, giữ lịch sử. |
| student_id | int | N | FK StudentProfile | Chủ hồ sơ. |
| course_id | int | N | FK Course | Bất biến từ lúc tạo. |
| status | varchar(16) | N | default PENDING, đường chuyển duyệt ADR-133 | Enum đã chốt, không COMPLETED. |
| is_unfinished | bit | N | initial 1, server-owned | Predicate ADR-022/090; filtered UQ. |
| created_at | datetime2(3) | N | server UTC | Khởi tạo. |
| admission_confirmed_at | datetime2(3) | Y | — | Khi Student-own CONFIRMED; actor trong audit/chủ sở hữu bất biến. |
| admission_approved_by | int | Y | FK Account | Admissions duyệt. |
| admission_approved_at | datetime2(3) | Y | — | Khi ADMITTED, giữ sau hủy. |
| rejected_by | int | Y | FK Account | Admissions; bundle bắt buộc khi REJECTED. |
| rejected_at | datetime2(3) | Y | — | UTC. |
| rejection_reason | nvarchar(2000) | Y | — | 1–1000 code point khi REJECTED. |
| rejected_from_status | varchar(16) | Y | — | CONFIRMED. |
| rejection_operation_id | uniqueidentifier | Y | — | Cùng audit rejection. |
| cancelled_by | int | Y | FK Account | Student-own hoặc Admissions theo guard. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Bắt buộc Admissions cancellation; không bắt Student nhập lý do mới. |
| cancelled_from_status | varchar(16) | Y | — | State trước khi hủy. |
| cancellation_operation_id | uniqueidentifier | Y | — | Cùng dependent Invoice/booking/audit. |
| row_version | rowversion | N | RV | Aggregate state/config result races. |

UQ `(student_id,course_id)` WHERE is_unfinished=1; không permanent unique cặp
này. CK enum, rejection/cancellation bundle, approved actor/time cùng NULL
hoặc cùng có. Service xác minh predicate gốc khi ghi; is_unfinished=false
chỉ CANCELLED/REJECTED hoặc assigned Class COMPLETED và report FINALIZED.
Không cần đạt≥50. Index `(course_id,status,created_at,registration_id)` và
`(student_id,created_at,registration_id)`.

Chi tiết đã duyệt ADR-133 cho default: PENDING là mới đăng ký theo mô tả enum trong
báo cáo nguồn VI.3; booking đầu tiên chuyển TESTING; kết quả đầu tiên hợp lệ
chuyển BAND_ASSIGNED; booking lượt hai giữ BAND_ASSIGNED và guard confirmation
xét lượt chưa chấm. Hủy booking/vắng không kéo state về PENDING hoặc xóa Band.
Đây là cụ thể hóa đường PENDING từng thiếu trong ma trận E02; đã đồng bộ.

### T10 — EntryTestSession

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| session_id | int | N | PK, ID | Khóa ca. |
| start_at | datetime2(3) | N | — | UTC từ ngày/giờ VN theo phút. |
| end_at | datetime2(3) | N | — | Cùng ngày VN/start<end. |
| room | varchar(30) | N | — | Mã phòng chuẩn hóa. |
| capacity | int | N | — | 1–20. |
| grader_id | int | N | FK Account | Teacher hiện phân công; Account blocked không tự mất assignment. |
| status | varchar(16) | N | default CLOSED | OPEN/CLOSED/CANCELLED. |
| cancelled_by | int | Y | FK Account | AcademicAffairs khi hủy. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| cancellation_operation_id | uniqueidentifier | Y | — | Parent cancellation/audit. |
| row_version | rowversion | N | RV | Open/close/cancel/edit. |

CK status/capacity/time; application xác minh cùng ngày VN/phút bằng conversion
chuẩn, CHECK có thể kiểm biểu thức UTC+07 cho các mốc hiện đại được hỗ trợ.
Index `(grader_id,start_at,end_at,session_id)` và `(room,start_at,end_at,session_id)`.
Effective CLOSED tại start không phải cột thứ hai; UDF FN01 suy diễn theo
as_of_utc, read không ghi. Capacity đếm SCHEDULED booking dưới lock session,
không thêm booked_count cache. Hoàn thành/vắng vẫn giữ lịch sử occupancy.

### T11 — EntryTestAttempt

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| attempt_id | int | N | PK, ID | Logical attempt. |
| registration_id | int | N | FK Registration | Parent hồ sơ. |
| attempt_number | int | N | UQ với registration_id | 1 hoặc 2. |
| score | decimal(5,2) | Y | — | Thi chưa chấm/vắng: NULL; 0 khi thi được nhập 0. |
| graded_by | int | Y | FK Account | Original score Teacher; không đổi khi correction. |
| graded_at | datetime2(3) | Y | — | Initial grading time; correction giữ trong audit. |
| completed_booking_id | int | Y | FK composite EntryTestBooking | Booking COMPLETED của đúng attempt. |
| row_version | rowversion | N | RV | Initial grade/correction. |

CK attempt 1–2, score range và result bundle cả bốn trường score/graded_by/
graded_at/completed_booking_id cùng NULL hoặc cùng có. UQ registration/number;
alternate UQ `(attempt_id,registration_id)` hỗ trợ BandAssignment composite FK.
Completed booking có filtered UQ completed_booking_id khi có; thêm alternate
UQ Booking `(booking_id,attempt_id)` để FK `(completed_booking_id,attempt_id)`
chặn trỏ booking của lượt khác. Cycle insert giải bằng attempt chưa result →
booking → ghi result; tất cả FK NO ACTION. Display state suy từ result/current
booking/latest history, không thêm persisted attempt.status.

### T12 — EntryTestBooking

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| booking_id | int | N | PK, ID | Mỗi appointment riêng. |
| attempt_id | int | N | FK EntryTestAttempt | Logical attempt, immutable. |
| session_id | int | N | FK EntryTestSession | Ca cụ thể, immutable; đổi ca tạo row mới. |
| status | varchar(16) | N | default SCHEDULED | SCHEDULED/COMPLETED/ABSENT/CANCELLED. |
| booked_at | datetime2(3) | N | server UTC | Lúc đặt. |
| absent_by | int | Y | FK Account | Assigned Teacher xác nhận. |
| absent_at | datetime2(3) | Y | — | Sau end, không suy từ thiếu điểm. |
| absent_operation_id | uniqueidentifier | Y | — | Nhóm audit. |
| cancelled_by | int | Y | FK Account | Actor hủy/đổi hoặc parent cancellation. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Parent reason khi có; Student booking cancel không bắt thêm lý do. |
| cancelled_from_status | varchar(16) | Y | — | SCHEDULED cho đường hợp lệ hiện tại. |
| cancellation_operation_id | uniqueidentifier | Y | — | Parent Registration/session hoặc booking-change operation. |
| row_version | rowversion | N | RV | State mutation. |

CK enum/absence-cancellation bundles; filtered UQ attempt_id WHERE status=
SCHEDULED, filtered UQ attempt_id WHERE status=COMPLETED. Service chặn booking
cho completed attempt và kiểm result↔COMPLETED consistency cùng transaction.
Index `(session_id,status,booking_id)` cho count/roster. Thứ tự latest history
`booked_at DESC,booking_id DESC`; không overwrite appointment cũ. ABSENT hoặc
CANCELLED không tự tăng attempt_number.

### T13 — BandAssignment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| band_assignment_id | int | N | PK, ID | Current best/Band projection được lưu. |
| registration_id | int | N | FK Registration, UQ | Một assignment Band/hồ sơ. |
| best_attempt_id | int | N | FK composite EntryTestAttempt | `(best_attempt_id,registration_id)` cùng hồ sơ. |
| band_id | int | N | FK Band | Kết quả ngưỡng lớn nhất≤best score. |
| assigned_at | datetime2(3) | N | server UTC | Lần recalculation gần nhất; history trong AuditLog. |
| row_version | rowversion | N | RV | Recalculate/confirmation race. |

Không có row khi chưa có completed score. Best tie chọn attempt 1; không
copy best_score vào BandAssignment. Index band_id và best_attempt_id; service
recheck completed result và freeze/confirmation trong W08–W09.

## 5. Gói D — lớp, xếp lớp và lịch

### 5.0. Phần D đã trình được duyệt ngày01/10/2026 — ADR-134

Owner duyệt phần D đã trình ngày01/10/2026 (ADR-134). Tạo DRAFT cùng xếp
lớp đã đồng bộ W15; chi tiết bổ sung đã được nghiệm thu cùng E03-v1,
không chứng minh implementation/SQL đã chạy.

Gói D gồm Class, StudentClassAssignment và Schedule; quy tắc lớp/xếp lớp/lịch
đã duyệt là đầu vào. Class giữ program_id, teacher_id, mã varchar(30) unique,
status khởi tạo PLANNED, max_size1–15 và rowversion; lưu thông tin hủy theo
quyền/reason/history hiện có. Sĩ số đọc từ assignment, không current_size cache.

StudentClassAssignment unique registration_id, FK Class/Registration, thêm
assigned_by AcademicAffairs và assigned_at UTC để truy vết lần xếp. Không
student_id trùng nguồn, không update/gỡ/chuyển và không cần rowversion riêng.
Đã duyệt tạo GradeReport DRAFT cùng assignment + mandatory audit trong W15
transaction, không tạo Score hoặc Attendance mặc định. Recheck PAID, đúng
Course/Band, OPEN/capacity và Student conflicts tại commit; cơ chế khóa ở mục9.

Schedule lưu date/time(0) theo VN (giây0), room varchar(30), type
LESSON/FINAL_EXAM, obligation_key, replaces_schedule_id, is_current,
occurrence_state và actor/time/reason theo hành động. Dời trước start sửa
date/time/room trên row hiện có và audit; non-held replacement giữ row cũ
NOT_HELD/noncurrent, tạo row mới future PLANNED cùng Class/type/obligation.
Unique chặn nhiều current/nghĩa vụ, nhiều successor/predecessor hoặc nhiều
current FINAL_EXAM/lớp; service bảo đảm đúng một current cho nghĩa vụ đã tạo.
Các nghĩa vụ LESSON chưa lập lịch không tự sinh placeholder row.

HELD lưu người/thời điểm xác nhận đúng guard đã chốt, không suy từ đồng hồ/
Attendance. Class CANCELLED giữ lịch sử nhưng không giữ chỗ tài nguyên.
Weekly batch/move/replacement + mandatory audit cùng transaction; preview
không giữ chỗ, commit kiểm lại Class/Teacher/room/Student và thứ tự FINAL_EXAM
sau current last LESSON. Test thiết kế: stale edits/chỗ cuối/fork replacement/
rollback/đếm một nghĩa vụ thay thế; SQL/index/locks chi tiết trình ở mục9–12.

### T14 — Class

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| class_id | int | N | PK, ID | Khóa lớp. |
| program_id | int | N | FK TrainingProgram | Bất biến; first Class khóa cấu hình program. |
| teacher_id | int | N | FK Account | Teacher hiện tại; giữ assignment khi Account blocked. |
| class_code | varchar(30) | N | UQ_Class_Code | ASCII uppercase, bất biến kể cả CANCELLED. |
| status | varchar(16) | N | default PLANNED | PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED. |
| max_size | int | N | — | 1–15; không thấp hơn placed count. |
| created_at | datetime2(3) | N | server UTC | Danh sách/trace tạo lớp. |
| cancelled_by | int | Y | FK Account | AcademicAffairs. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| cancellation_operation_id | uniqueidentifier | Y | — | Audit class cancellation. |
| row_version | rowversion | N | RV | State/capacity/Teacher mutation. |

CK enum/max_size/cancellation bundle. Index `(program_id,status,class_id)` và
`(teacher_id,status,class_id)`. Không lưu current_size/frozen program/status
suy theo đồng hồ. Service W14 kiểm opening/start/completion/cancel và lock
với placement/Attendance/Score; Teacher role/state kiểm khi assign/change.
Không thêm bảng lịch sử đổi Teacher; các audit đã bắt buộc giữ đúng phạm vi.

### T15 — StudentClassAssignment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| class_assignment_id | int | N | PK, ID | Một lượt học trong lớp. |
| class_id | int | N | FK Class | Lớp được xếp, bất biến. |
| registration_id | int | N | FK Registration, UQ | Tối đa một lớp/hồ sơ. |
| assigned_by | int | N | FK Account | AcademicAffairs, đề xuất trường provenance. |
| assigned_at | datetime2(3) | N | server UTC | Khi xếp. |

UQ registration_id, index `(class_id,class_assignment_id)` phục vụ cohort/size.
Assignment không sửa/gỡ/chuyển theo ADR-040 nên không có rowversion; insert
dưới Class/Registration/Student resource locks. Không thêm student_id vì
Registration đã quyết định Student. Tạo GradeReport DRAFT trong cùng W15
transaction đã duyệt ADR-134; không tạo Score/Attendance mặc định.

### T16 — Schedule

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| schedule_id | int | N | PK, ID | Một occurrence cụ thể, kể cả history. |
| class_id | int | N | FK Class | Parent lớp. |
| session_type | varchar(16) | N | — | LESSON/FINAL_EXAM. |
| obligation_key | uniqueidentifier | N | server | Root và mọi replacement cùng nghĩa vụ/cùng Class/type. |
| replaces_schedule_id | int | Y | FK Schedule, filtered UQ khi có | NULL ở root; một predecessor có tối đa một successor. |
| is_current | bit | N | initial 1, server-owned | Projection đã duyệt ADR-131/134; filtered unique/current query. |
| session_date | date | N | — | Ngày VN. |
| start_time | time(0) | N | — | Giây=0, start<end. |
| end_time | time(0) | N | — | Cùng session_date. |
| room | varchar(30) | N | — | Mã phòng chuẩn hóa. |
| occurrence_state | varchar(16) | N | default PLANNED | PLANNED/HELD/NOT_HELD. |
| held_by | int | Y | FK Account | AcademicAffairs, khi HELD. |
| held_at | datetime2(3) | Y | — | Sau scheduled end, Class ONGOING. |
| not_held_by | int | Y | FK Account | AcademicAffairs khi NOT_HELD. |
| not_held_at | datetime2(3) | Y | — | Tại/sau scheduled start. |
| not_held_reason | nvarchar(2000) | Y | — | Mandatory 1–1000. |
| replacement_operation_id | uniqueidentifier | Y | — | Cùng old/new replacement/audit. |
| row_version | rowversion | N | RV | Move/replacement/HELD. |

CK enums/time-minute/state bundles/self-pointer≠self. PLANNED/HELD current=1;
NOT_HELD current=0 có đầy đủ non-held bundle. UQ `(class_id,obligation_key)`
WHERE is_current=1; filtered UQ class_id WHERE is_current=1 AND session_type=
FINAL_EXAM. Class CANCELLED vẫn giữ current lịch sử nhưng không chiếm resource.
Không áp UQ lên tổng history/obligation vì replacement phải cùng khóa.

Service W16 giữ class/type/key bất biến trong chain, chặn fork/cycle, đảm bảo
old NOT_HELD và new future PLANNED cùng transaction. Tại commit phải có đúng
một current occurrence/nghĩa vụ; filtered unique chỉ bảo đảm *tối đa* một.
Pre-start move sửa date/time/room trên cùng row và audit cũ/mới; non-held
replacement tạo row mới. Không delete history để thay buổi. Index
`(class_id,is_current,session_date,start_time,schedule_id)` và
`(room,session_date,start_time,end_time,schedule_id)`; Teacher lấy qua Class.

## 6. Gói E — điểm danh và kết quả

### 6.0. Phần E đã trình được duyệt ngày01/10/2026 — ADR-135

Owner duyệt phần E đã trình ngày01/10/2026 (ADR-135). Finalized-only
final_score đã đồng bộ requirements/E02; chi tiết bổ sung đã được nghiệm thu
cùng E03-v1, mapping/enforcement thực thi và tests còn thuộc E04/later.

Gói E gồm Attendance, Score và GradeReport. Attendance unique theo Schedule/
assignment, present bit NOT NULL không default; chưa ghi không có row. Score
unique assignment/component, score_value decimal(5,2) NOT NULL không default,
thiếu điểm không có row. Cả hai giữ entered_by/entered_at UTC lần ghi gần
nhất và rowversion; lịch sử theo đúng các audit bắt buộc, không tự thêm audit
mọi read. Service kiểm actor/window/state/assignment/Class/program/current
Schedule; FK existence không đủ để bảo vệ các quan hệ xuyên bảng này.

Score GENERAL có schedule_id NULL; LESSON/FINAL_EXAM bắt buộc đúng occurrence
hiện hành thuộc lớp và đúng loại, mở từ start và không NOT_HELD. Không tự
buộc mọi học viên chọn cùng Schedule cho một LESSON component khi baseline
chưa có quy tắc binding đó. Batch Attendance một LESSON và score/comment
save một học viên là hai giao dịch độc lập all-or-none đúng ADR-124; blank
không xóa Score cũ hoặc thành0. Save score/comment chạm parent GradeReport
version (ADR-131), stale edit trả conflict chứ không overwrite.

GradeReport unique assignment, DRAFT tạo cùng xếp lớp theo ADR-134. Đã duyệt
teacher_comment/returned_reason nvarchar(2000), NULL có chủ đích; final_score
decimal(5,2) chỉ lưu khi FINALIZED, còn total nháp/SUBMITTED tính từ điểm đã
lưu, thiếu component thì Chưa đủ điểm. Tính decimal/half-up2 một lần cuối;
passing suy từ FINALIZED và final_score≥50, không lưu cờ đạt trùng dữ liệu.

Các cột submit/review actor/time/operation_id giữ lần gần nhất; mỗi lần
gửi/gửi lại/trả/chốt từng report có audit riêng với chung operation của lớp.
Không thêm GradeHistory table. Gửi/trả/chốt kiểm cả cohort và chuyển toàn lớp
cùng audit; FINALIZED còn cập nhật is_unfinished nhất quán. FINALIZED khóa
Score/comment/report; Student chỉ xem own finalized, Giáo vụ không xem DRAFT.
Test thiết kế: thiếu-vs0, stale save-vs-submit, return/resubmit history,
partial-failure rollback và publish atomic; SQL enforcement/index/locks ở mục9–12.

### T17 — Attendance

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| attendance_id | int | N | PK, ID | Record đã chủ động ghi. |
| schedule_id | int | N | FK Schedule | Current LESSON của cùng Class. |
| class_assignment_id | int | N | FK StudentClassAssignment | Student của lớp. |
| present | bit | N | Không default | 1=Có mặt, 0=Vắng; chưa nhập=không row. |
| entered_by | int | N | FK Account | Teacher hiện assigned, provenance duyệt ADR-135. |
| entered_at | datetime2(3) | N | server UTC | Lần ghi gần nhất, provenance duyệt ADR-135. |
| row_version | rowversion | N | RV | Batch delta theo một LESSON. |

UQ `(schedule_id,class_assignment_id)`; index assignment_id cho Student-own/
cohort query. TG04 chặn sai Class/type/current và mutation sau Class COMPLETED;
service kiểm Actor/start window/status/W17. Batch chứa các giá trị đã nhập,
all-or-none; không tự tạo absent/present cho roster còn lại. Giữ đúng phạm vi
audit đã có, không tự audit mọi read hoặc thêm điều kiện đạt môn.

### T18 — Score

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| score_id | int | N | PK, ID | Một Score/học viên/component. |
| class_assignment_id | int | N | FK StudentClassAssignment | Lượt học. |
| component_id | int | N | FK ScoreComponentConfig | Component thuộc Class.program. |
| schedule_id | int | Y | FK Schedule | NULL chỉ GENERAL; LESSON/FINAL_EXAM bắt buộc. |
| score_value | decimal(5,2) | N | Không default | 0–100 đã validate≤2 decimals. |
| entered_by | int | N | FK Account | Current Teacher. |
| entered_at | datetime2(3) | N | server UTC | Lần lưu gần nhất; history đã bắt buộc trong audit. |
| row_version | rowversion | N | RV | Per-Student draft all-or-none. |

UQ `(class_assignment_id,component_id)`; index component_id và filtered
schedule_id khi khác NULL. TG05 kiểm cùng program/Class/current/kind và khóa
Score khi report SUBMITTED/FINALIZED. GENERAL không cần lịch; linked score
mở từ lịch start, không NOT_HELD. FK existence không thay cross-table guard.
Không lưu row rỗng, không nhận blank như xóa hoặc 0. Không endpoint DELETE
Score; classroom grade locks/check actor/state vẫn tại service W18.

### T19 — GradeReport

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| report_id | int | N | PK, ID | Bảng điểm/lượt học. |
| class_assignment_id | int | N | FK StudentClassAssignment, UQ | Một report/assignment. |
| final_score | decimal(5,2) | Y | — | Chỉ có khi FINALIZED; draft/submitted total là tính toán. |
| status | varchar(16) | N | default DRAFT | DRAFT/SUBMITTED/RETURNED/FINALIZED. |
| teacher_comment | nvarchar(2000) | Y | — | ≤1000 code point, optional-empty→NULL. |
| submitted_by | int | Y | FK Account | Teacher gửi/gửi lại gần nhất. |
| submitted_at | datetime2(3) | Y | — | Lần gửi gần nhất. |
| submitted_operation_id | uniqueidentifier | Y | — | Cùng mọi report của Class trong lần gửi. |
| reviewed_by | int | Y | FK Account | AcademicAffairs trả/chốt gần nhất. |
| reviewed_at | datetime2(3) | Y | — | Lần review gần nhất. |
| review_operation_id | uniqueidentifier | Y | — | Cùng mọi report khi return/finalize. |
| returned_reason | nvarchar(2000) | Y | — | Mandatory khi RETURNED; latest reason có thể giữ sau resubmit. |
| finalized_at | datetime2(3) | Y | — | Khi FINALIZED, bằng thời điểm review chốt. |
| row_version | rowversion | N | RV | Draft/comment/whole-Class workflow. |

CK valid status, final_score range và `final_score/finalized_at` có iff
FINALIZED; submitted/review bundles đầy đủ theo state. DRAFT có thể chưa gửi/
review; SUBMITTED/RETURNED/FINALIZED phải có latest submit. RETURNED bắt buộc
reason/review bundle; FINALIZED có review bundle. Latest timestamps không
là toàn bộ lịch sử: từng submit/resubmit/return/finalize của từng report có
AuditLog riêng với chung operation_id của Class. Không thêm GradeHistory table
vì AuditLog đã sở hữu lịch sử này. Không overwrite/delete event cũ.

Per-Student score/comment save khóa Class/report/cohort. Whole-Class submit,
return/finalize recheck mọi report/components và all-or-none. FINALIZED cấm
sửa Score/comment/report; chỉ Student-own đọc finalized values. Giáo vụ chỉ
thấy giá trị SUBMITTED/RETURNED/FINALIZED đúng ADR-083. Passing là derived
FINALIZED && final_score≥50, không một cột lưu khác. Index
`(status,finalized_at,report_id)` cho review/cohort; UQ assignment đã hỗ trợ FK.

## 7. Gói F — hóa đơn, thanh toán, audit và bảng kỹ thuật

### 7.0. Phần F đã trình được duyệt ngày01/10/2026 — ADR-136

Gói F gồm Invoice/Payment/AuditLog và ReplayOperation/InvoiceNumberCounter;
giữ chính sách tài chính, audit/replay và hai store đã duyệt làm đầu vào.
Owner duyệt phần F đã trình qua ADR-136; sau đó nghiệm thu toàn bộ E03-v1,
gồm dictionary/CHECK/index candidates và cơ chế locking. Exact hash
serialization, cleanup grants và implementation mapping vẫn thuộc E04.
Invoice unique Registration và invoice_number varchar(16), snapshot tiền
decimal(18,0), issued_at UTC/due_date VN và UNPAID/PAID/CANCELLED; không lưu
OVERDUE hoặc snapshot profile. Cancel giữ parent actor/time/reason/operation.
NumberCounter khóa theo năm VN và cấp HD-YYYY-NNNNNNNN trong giao dịch duyệt;
số đã phát hành không tái dùng, hết99.999.999 chặn/rollback chứ không reset.

Payment unique Invoice, amount_paid bằng Invoice.amount; confirmed_by/paid_at
UTC bất biến, không ngày nhận tiền riêng. method CASH/TRANSFER, reference
nvarchar(200) tối đa100 code point, TRANSFER cần reference nhưng không unique.
Metadata correction giữ actor/time/reason gần nhất và audit từng lần; không
đổi link/tiền/confirmation, xóa/hủy/refund. Tạo Payment + PAID + audit cùng
giao dịch; cancellation vs collection recheck state/version theo cơ chế mục9.

AuditLog ghi từ application service, details JSON allowlist old/new/reason,
actor NULL chỉ khi chưa xác thực; actor/action/entity/result/UTC/operation
hỗ trợ tra cứu và grouped history. audit_category tách MANDATORY/SECURITY;
security_expires_at/IP expiry giúp vận hành dọn đúng90 ngày, giữ mandatory
history. Không trigger audit hoặc endpoint sửa/xóa/export log, không raw
payload/hash/password/token/secret. Principal vận hành riêng theo G2;
quyền dọn DB cụ thể thuộc E04, không tự thêm app purge.

ReplayOperation unique actor/action/request_id; lưu input_digest, operation,
completed_at/expires_at và result_mode/reference JSON an toàn cùng transaction
thành công, hạn ít nhất24h. Cùng key/nội dung không tạo hiệu ứng mới; nội dung
khác409. Trả lại phải recheck quyền/ownership và dựng từ reference có quyền,
không response snapshot PII/secret. Cấp/reset credential chỉ giữ marker
SECRET_RESULT_UNAVAILABLE, không phục hồi hoặc phát lại mật khẩu tạm.
Cleanup marker hết hạn không xóa business/audit history; domain uniqueness
vẫn bảo vệ sau cửa sổ. Kiểm chứng thiết kế: duplicate approval/payment,
allocation race/overflow, rollback cả mutation/audit và secret-safe replay.

### T20 — Invoice

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| invoice_id | int | N | PK, ID | Khóa Invoice. |
| invoice_number | varchar(16) | N | UQ_Invoice_Number | HD-YYYY-NNNNNNNN, server cấp, không tái dùng. |
| registration_id | int | N | FK Registration, UQ | Chỉ tạo cùng admission approval. |
| amount | decimal(18,0) | N | — | Course price snapshot dương bất biến. |
| issued_at | datetime2(3) | N | server UTC | Issue time, không backdate. |
| due_date | date | N | server | Ngày VN issued+7 calendar days. |
| status | varchar(16) | N | default UNPAID | UNPAID/PAID/CANCELLED, không OVERDUE. |
| cancelled_by | int | Y | FK Account | Actor cancellation parent Registration. |
| cancelled_at | datetime2(3) | Y | — | UTC. |
| cancellation_reason | nvarchar(2000) | Y | — | Parent reason được yêu cầu tại workflow. |
| cancelled_from_status | varchar(16) | Y | — | UNPAID. |
| cancellation_operation_id | uniqueidentifier | Y | — | Chung Registration/booking/audit. |
| row_version | rowversion | N | RV | Payment vs cancellation. |

CK amount>0/state/bundle/number format; date calculation và năm số hóa đơn
được service/allocator kiểm từ issued_at VN. Không snapshot contact/profile.
Payment exists iff PAID được duy trì cùng W12 transaction; unique FK chỉ bảo
đảm tối đa một. Không hủy PAID/sửa due/amount/issued/link/number. Index
`(status,due_date,invoice_id)` và `(issued_at,invoice_id)` cho debt/billed query.

### T21 — Payment

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| payment_id | int | N | PK, ID | Payment đã xác nhận. |
| invoice_id | int | N | FK Invoice, UQ | Tối đa một Payment/Invoice. |
| amount_paid | decimal(18,0) | N | — | Bằng Invoice.amount; bất biến. |
| method | varchar(16) | N | — | CASH/TRANSFER. |
| transaction_reference | nvarchar(200) | Y | — | ≤100 code point; bắt buộc TRANSFER, không unique. |
| confirmed_by | int | N | FK Account | Accountant xác nhận. |
| paid_at | datetime2(3) | N | server UTC | Confirmation time bất biến, không ngày nhận tiền riêng. |
| last_corrected_by | int | Y | FK Account | Accountant sửa method/reference gần nhất. |
| last_corrected_at | datetime2(3) | Y | — | UTC correction. |
| last_correction_reason | nvarchar(2000) | Y | — | Mandatory 1–1000 khi correction. |
| row_version | rowversion | N | RV | Metadata correction conflict. |

CK amount>0/method/TRANSFER nonempty reference/correction bundle. Service
W12 equality amount, UNPAID guard và metadata-only update. Audit ghi old/new
method/reference/reason/actor/time mỗi correction; latest fields không thay
history. Không delete/cancel/refund hoặc thay confirmed_by/paid_at/link/amount.
Index `(paid_at,payment_id)` và confirmed_by. Không dedup qua bank reference.

### T22 — AuditLog

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| audit_id | int | N | PK, ID | Event; không aggregate whole-Class thành một event mất chi tiết. |
| actor_account_id | int | Y | FK Account | NULL chỉ khi chưa xác thực. |
| action | varchar(80) | N | — | Mã action allowlist, giới hạn kỹ thuật đề xuất. |
| entity_type | varchar(64) | N | — | Loại đối tượng allowlist. |
| entity_id | varchar(100) | Y | — | int/UUID/session reference khi hợp lệ; NULL nếu chưa có đối tượng xác minh. |
| result | varchar(16) | N | — | SUCCESS/FAILURE đề xuất; denial/throttle phân biệt bằng action/details. |
| details | nvarchar(max) | Y | CK ISJSON khi có | Structured allowlist old/new/reason/prior-state; không raw payload. |
| occurred_at | datetime2(3) | N | server UTC | Event time. |
| ip_address | varchar(45) | Y | — | IP thực sự cần cho security event, proxy tin cậy theo E04. |
| operation_id | uniqueidentifier | N | server | Nhóm parent/dependent hoặc một security request. |
| audit_category | varchar(16) | N | — | MANDATORY/SECURITY, kỹ thuật retention. |
| security_expires_at | datetime2(3) | Y | — | SECURITY→occurred+90d; MANDATORY→NULL. |
| ip_expires_at | datetime2(3) | Y | — | Có IP→occurred+90d; không IP→NULL. |

Audit append-only từ service, không trigger audit. CK result/category/JSON/
retention bundles; mandatory fields không bị xóa sau 90 ngày. Vận hành có
quyền riêng dọn security-only rows và clear IP đã hết hạn; không endpoint app
purge/edit/export. Rate-limited requests có thể event gộp window/count trong
allowlist details; không bỏ counters. Không lưu passwords/password hashes,
JWT/CSRF/sid bí mật, guessed email như verified actor hoặc full request-response.
entity_id cho session phải dùng reference không tiết lộ raw sid. actor/time
old/new của history giữ trong event ngay cả khi profile hiện tại đổi.

Index `(occurred_at DESC,audit_id DESC)`, `(operation_id,audit_id)`,
`(actor_account_id,occurred_at DESC,audit_id DESC)`,
`(entity_type,entity_id,occurred_at DESC,audit_id DESC)`; chỉ bổ sung action/result
index nếu execution plan cho thấy cần. Index security_expires_at/ip_expires_at
cho maintenance nếu đo cho thấy lợi ích; không hứa mọi filter đều index seek.

### T23 — ReplayOperation (bảng kỹ thuật)

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| replay_id | int | N | PK, ID | Khóa lưu kết quả. |
| actor_account_id | int | N | FK Account | Actor đã xác thực; không dùng chung theo browser. |
| action_code | varchar(80) | N | UQ với actor/request_id | Canonical action, không URL tùy ý. |
| request_id | uniqueidentifier | N | UQ với actor/action | Khóa request do client tạo cho một intent. |
| input_digest | binary(32) | N | — | SHA-256 canonical validated nonsecret business input/target/expected versions. |
| operation_id | uniqueidentifier | N | server | Chung effects và audit. |
| result_mode | varchar(32) | N | — | SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE. |
| result_reference | nvarchar(max) | N | CK ISJSON | IDs, outcome code/operation; không response có PII hay secret. |
| completed_at | datetime2(3) | N | server UTC | Chỉ persist cùng transaction thành công. |
| expires_at | datetime2(3) | N | server | Ít nhất completed+24h; đề xuất đúng 24h cho maintenance. |

UQ `(actor_account_id,action_code,request_id)`, index expires_at và operation_id;
CK mode/JSON/expiry≥24h. Lock request scope trước khi làm effects; rollback
không để completed marker. Cùng digest không mutation mới, khác digest 409;
recheck account/session/current role/ownership và tạo response từ references
được phép thay vì replay một PII snapshot cũ. Maintenance hết hạn không xóa
business/audit history; domain unique/state vẫn chống duplicate sau cửa sổ.

Admin cấp/reset dùng nonsecret input target/role/intent, completion marker
SECRET_RESULT_UNAVAILABLE không chứa credential và không sinh secret mới
khi replay. Đổi mật khẩu tự phục vụ không hash/lưu credential input ở đây:
session/security_version/current credential guard của W02 chặn retry cũ sau
revocation; E04 mô tả endpoint và phản hồi không rõ theo ADR-128. Không tái dùng
table này để cache login, password/JWT hoặc lưu response secret.

### T24 — InvoiceNumberCounter (bảng kỹ thuật)

| Trường | Kiểu SQL | NULL | Khóa/default | Ý nghĩa |
| --- | --- | --- | --- | --- |
| issue_year | smallint | N | PK | Năm ngày phát hành VN, 1–9999. |
| last_value | int | N | initial 0 | Counter 0–99.999.999; number mới tăng một. |
| row_version | rowversion | N | RV | Internal; allocator dùng row/range lock, không form edit. |

W10 allocator trong transaction cấp HD-YYYY-NNNNNNNN, unique Invoice number
là backstop. Serialize năm chưa có row để tránh double insert; số đã phát
hành không tái dùng. Overflow chặn approval và rollback, không reset/truncate
hoặc chuyển âm. Counter rollback cho allocation chưa phát hành được phép;
không hứa liên tục và không coi IDENTITY invoice_id là số invoice. Không API
reset counter, không hardcode năm thiết bị/client.

## 8. Khóa, cardinality, chuẩn hóa và lịch sử

### 8.1. Ma trận unique/alternate key đề xuất

Mỗi PK trong T01–T24 là một unique key. Các key dưới đây là bổ sung PK;
filtered unique với NULL phải được EF migration mô tả rõ, không dựa vào
convention ngầm. Tên logic được đề xuất, migration sau này phải khớp.

| Key | Bảng/cột hoặc điều kiện | Invariant |
| --- | --- | --- |
| UQ_Role_Code | Role(role_code) | Bảy role cố định không trùng. |
| UQ_Account_Email | Account(email) CI | Email toàn Account, kể cả inactive/blocked. |
| UQ_Account_Phone | Account(phone) WHERE phone IS NOT NULL | Optional staff phone không tạo duplicate-null blocker. |
| UQ_StudentProfile_Account | StudentProfile(account_id) | Tối đa một profile/account. |
| UQ_Course_Code | Course(course_code) | Bất biến/không tái dùng. |
| UQ_Band_Code / UQ_Band_MinScore | Band(band_code) / Band(min_score) | Band/ngưỡng không trùng. |
| UQ_Program_CourseBand | TrainingProgram(course_id,band_id) | Không tạo config/version trùng cặp. |
| UQ_Component_ProgramName | ScoreComponentConfig(program_id,component_name) CI_AS_SC | Tên trong một program. |
| UQ_Component_FinalExam | ScoreComponentConfig(program_id) WHERE kind=FINAL_EXAM | Tối đa một; ACTIVE phải có một. |
| UQ_Registration_Unfinished | Registration(student_id,course_id) WHERE is_unfinished=1 | Một hồ sơ đang làm/học theo predicate. |
| UQ_Attempt_RegistrationNumber | EntryTestAttempt(registration_id,attempt_number) | Logical attempt 1–2. |
| AK_Attempt_IdRegistration | EntryTestAttempt(attempt_id,registration_id) | Đích composite FK best_attempt. |
| UQ_Attempt_CompletedBooking | EntryTestAttempt(completed_booking_id) WHERE NOT NULL | Một result không mượn booking khác. |
| AK_Booking_IdAttempt | EntryTestBooking(booking_id,attempt_id) | Đích composite FK completed_booking. |
| UQ_Booking_Current / UQ_Booking_Completed | EntryTestBooking(attempt_id) WHERE status=SCHEDULED / status=COMPLETED | Tối đa một active appointment/result booking/lượt. |
| UQ_BandAssignment_Registration | BandAssignment(registration_id) | Current best/Band tối đa một. |
| UQ_Class_Code | Class(class_code) | Mã lớp mọi trạng thái. |
| UQ_Assignment_Registration | StudentClassAssignment(registration_id) | Không chuyển/gỡ/xếp lớp thứ hai. |
| UQ_Schedule_CurrentObligation | Schedule(class_id,obligation_key) WHERE is_current=1 | Không double current occurrence. |
| UQ_Schedule_CurrentFinalExam | Schedule(class_id) WHERE is_current=1 AND session_type=FINAL_EXAM | Tối đa một nghĩa vụ thi hiện hành/lớp. |
| UQ_Schedule_Predecessor | Schedule(replaces_schedule_id) WHERE NOT NULL | Không fork replacement chain. |
| UQ_Attendance_ScheduleAssignment | Attendance(schedule_id,class_assignment_id) | Một record/lượt học/buổi. |
| UQ_Score_AssignmentComponent | Score(class_assignment_id,component_id) | Một Score/component/lượt học. |
| UQ_GradeReport_Assignment | GradeReport(class_assignment_id) | Tối đa một report. |
| UQ_Invoice_Registration / UQ_Invoice_Number | Invoice(registration_id) / Invoice(invoice_number) | Một Invoice/hồ sơ; số phát hành không trùng. |
| UQ_Payment_Invoice | Payment(invoice_id) | Một lần thu đủ. |
| UQ_Replay_ActorActionRequest | ReplayOperation(actor_account_id,action_code,request_id) | Replay scope đúng actor/action. |

Payment.transaction_reference **không** unique. Không permanent unique
Registration(student,course). Schedule không unique (class,type) cho toàn
history; max_size/capacity không thể bảo vệ bằng unique index đơn lẻ.

### 8.2. FK và các quan hệ bắt buộc

Từ điển T01–T24 là danh sách FK đầy đủ; mọi cột ghi FK dùng NO ACTION.
Tên FK đề xuất theo mẫu `FK_<Child>_<Parent>_<Column>`; hai composite FK
là `FK_Attempt_Booking_CompletedBookingAttempt` và
`FK_BandAssignment_Attempt_BestAttemptRegistration`. Không tạo thêm scalar FK
trùng với composite FK chỉ để tăng số constraint.
Các actor FK về Account không tự chứng minh đúng role. Service kiểm role,
status và sở hữu trong transaction; hai composite FK chặn khác Registration/
attempt bằng DB. Audit entity_type/entity_id và operation references là
polymorphic correlation, không tạo giả FK sang một business table duy nhất.
Course không có FK ngược trực tiếp Student; ClassAssignment không thêm
student_id/StudentProfile FK. Nullability là optional event/conditional link,
không nullable mặc định cho mọi quan hệ.

### 8.2a. Danh mục CHECK và default theo bảng

Tên CHECK theo `CK_<Table>_<Subject>` dưới đây. Predicate NULL cần viết
IS NULL/IS NOT NULL rõ khi có SQL thật; không dựa vào UNKNOWN được CHECK
chấp nhận để lọt bundle nửa có/nửa không. Code-point/NFC/precision đầu vào,
role/ownership và cross-table state không thể thay bằng CHECK đơn bảng.

| Bảng | Subject và predicate thiết kế | Default khi INSERT |
| --- | --- | --- |
| Role | Code: một trong bảy code; Name: nonempty sau validation | IDENTITY |
| Account | Status: ACTIVE/LOCKED/DISABLED; SecurityVersion>0; FailureCount≥0; Phone: NULL hoặc 10 ASCII số/bắt đầu0; TemporaryBundle: forced-change có issued/expires đúng24h, credential thường hai mốc NULL | status ACTIVE, version1, failed_count0; forced-change do server xác định |
| StudentProfile | DOB check ở service theo ngày VN hiện tại; Address optional validated | IDENTITY; không default DOB/address |
| AuthSession | Scope: NORMAL/PASSWORD_CHANGE; Temporal: issued≤activity≤expires và expires>issued, revoke NULL hoặc≥issued; Version>0 | activity=issued, expiry/scope do server |
| Course | Price>0; Status valid; Code: length1–30/uppercase ASCII pattern | Không default giá hoặc code; trạng thái explicit |
| Band | MinScore0–100; Code nonempty ASCII≤30 | Seed ngưỡng, không default score |
| TrainingProgram | Status DRAFT/ACTIVE/INACTIVE; ExpectedLessons>0 | DRAFT |
| ScoreComponentConfig | Kind valid; Weight>0≤100; Name nonempty | Không default weight/kind |
| Registration | Status trong enum; ApprovedBundle actor/time đều có hoặc đều NULL; RejectedBundle iff REJECTED gồm actor/time/reason/prior CONFIRMED/operation; CancelledBundle iff CANCELLED gồm actor/time/prior/operation, reason theo actor/flow ở service | PENDING/unfinished1 đề xuất; created server UTC |
| EntryTestSession | Status valid; Capacity1–20; Time start<end/phút/cùng ngày VN; CancellationBundle iff CANCELLED có actor/time/reason/operation | CLOSED |
| EntryTestAttempt | Number1–2; Score NULL hoặc0–100; ResultBundle bốn trường đều NULL hoặc đều có | Không default score/result |
| EntryTestBooking | Status valid; AbsenceBundle iff ABSENT có actor/time/operation; CancellationBundle iff CANCELLED có actor/time/prior SCHEDULED/operation | SCHEDULED/booked server UTC |
| BandAssignment | Không CHECK best result cross-table; FK composite + service FN02/first-use guard | assigned server UTC |
| Class | Status valid; MaxSize1–15; CancellationBundle iff CANCELLED actor/time/reason/operation | PLANNED/created server UTC |
| StudentClassAssignment | Quan hệ/state/capacity là cross-table guard | assigned server UTC |
| Schedule | Type/state valid; Time start<end và giây0; NoSelfReplacement; CurrentState: current1 cho PLANNED/HELD,0 cho NOT_HELD; HeldBundle iff HELD actor/time; NotHeldBundle iff NOT_HELD actor/time/reason/replacement operation | PLANNED/current1; obligation do server |
| Attendance | bit có mặt/vắng NOT NULL; không CHECK thay TG04/cross-table | Không default present |
| Score | Score0–100; dependency kind/Class qua TG05 | Không default score/schedule |
| GradeReport | Status valid; FinalResult iff FINALIZED có final_score0–100/finalized_at/review bundle; SubmitBundle bắt buộc SUBMITTED/RETURNED/FINALIZED; ReviewBundle đủ hoặc NULL; RETURNED phải có nonempty reason | DRAFT, chưa có final_score |
| Invoice | Amount>0; Status valid; NumberPattern HD/năm4/số8; CancelledBundle iff CANCELLED actor/time/prior UNPAID/operation | UNPAID, issued/due/number/amount server |
| Payment | Amount>0; Method CASH/TRANSFER; Reference nonempty khi TRANSFER; CorrectionBundle ba trường cùng NULL hoặc cùng có/nonempty reason | paid/confirmed server; không default method/amount |
| AuditLog | Result SUCCESS/FAILURE; Category MANDATORY/SECURITY; Details NULL hoặc JSON; SecurityExpiry chỉ SECURITY và≥90d; IPBundle có IP thì ip expiry đúng90d | occurred/operation server |
| ReplayOperation | Mode valid; ResultReference JSON; Expiry≥completed+24h | completed/expiry/operation server |
| InvoiceNumberCounter | Year1–9999; Value0–99.999.999 | Initial last_value0 |

Một số CHECK đơn bảng kiểm range/pattern đủ như trên; điều kiện lịch VN,
due date+7 và bundle theo role còn service kiểm cùng transaction. Không ép
trạng thái tài khoản ACTIVE hiện tại làm CHECK trên historical actor FK;
actor sau này bị khóa vẫn phải giữ lịch sử. Mỗi temporal event do server lấy
cùng clock context, không nhận client-declared actor/time.

### 8.3. Chứng minh chuẩn hóa và phần dư có chủ đích

| Nhóm | Phụ thuộc và cách kiểm |
| --- | --- |
| Account/StudentProfile/Role | Role label thuộc Role; DOB/address chỉ ở StudentProfile. Account email/phone/name hiện tại không nhân bản vào hồ sơ/hóa đơn. |
| Course/Program/Component | Price thuộc Course; Course/Band/count thuộc Program; kind/weight thuộc Component. Không lặp weight trong Score hoặc GradeReport. |
| Registration/Attempt/Booking | Course/Student thuộc Registration; số lượt/kết quả thuộc Attempt; appointment/state history thuộc Booking. Session time/room/grader không copy vào Attempt. |
| Assignment/Schedule/Attendance/Score | Assignment quyết định Student/Class; Schedule quyết định occurrence; Attendance unique pair và Score unique component pair. Không lưu current_size hoặc attendance boolean mặc định trên Student. |
| Invoice/Payment | Invoice amount là giá đã phát hành, không phụ thuộc *giá Course hiện tại*; Payment method/reference/time là sự kiện thu. Snapshot tài chính có ý nghĩa nghiệp vụ riêng. |
| Kết quả/lịch sử | final_score là kết quả công bố frozen tại FINALIZED; AuditLog là sự kiện độc lập; latest event metadata không thay event history. |

Thiết kế entity theo phụ thuộc chức năng trên nhằm đạt 3NF. Cần ghi rõ ngoại
lệ vật lý: is_unfinished là projection cross-table đã duyệt ADR-090;
Schedule.is_current là projection đã duyệt ADR-131/134 cho filtered unique; BandAssignment
lưu best/Band theo policy; event dates/prior-state và counter/replay là dữ liệu
sự kiện/kỹ thuật. Những projection này không được gọi là 3NF thuần túy mà
không giải thích: lưu nguồn có thẩm quyền, update atomic và kiểm lệch trong
verification. Nếu giảng viên yêu cầu mọi bảng tuyệt đối 3NF kể cả projection,
cần trình bày riêng mô hình logical 3NF và lựa chọn physical đã được owner
duyệt, không xóa is_unfinished để né rubric hoặc tự đổi ADR.

### 8.4. Nguồn lịch sử và freeze

| Dữ liệu | Giữ ở đâu | Ai/điều kiện bảo vệ |
| --- | --- | --- |
| Appointment đã hủy/vắng/hoàn thành | EntryTestBooking + result pointer | Không ghi đè session_id hoặc tăng attempt vì vắng. |
| Buổi không tổ chức/thay thế | Schedule chain và event bundle | Cùng obligation; không delete/reset NOT_HELD/HELD. |
| Rejection/cancellation và effects | Entity bundles + AuditLog chung operation_id | Actor/time/prior-state/reason theo từng workflow, all-or-none. |
| Grade submit/resubmit/return/finalize | AuditLog riêng mỗi report/lần, cùng operation_id Class | Latest fields trên report chỉ tiện query; giữ mọi event bắt buộc. |
| Payment metadata corrections | Payment latest bundle + AuditLog từng lần | amount/paid_at/invoice immutable. |
| Program freeze | Existence bất kỳ Class.program_id | TG01/TG02 + serialized first Class/config edit; kể cả CANCELLED. |
| Band-set freeze | Existence completed EntryTestAttempt result | TG03 + first grading/config lock; không bỏ history mở khóa. |
| Tài khoản/thu hồi phiên | Account version/AuthSession + audit phù hợp | Manual lock không timed auto-unlock, restoration không hồi sinh session. |

## 9. Giao dịch và enforcement đồng thời

### 9.0. Phần G2 đã trình được duyệt ngày01/10/2026 — ADR-138

Đầu vào đã chốt: một trung tâm, mốc kiểm chứng10.000 học viên/50 đồng thời,
laptop chạy theo buổi, owner vận hành; 7 role web và chính sách audit/replay/
backup giữ nguyên. G1 đã trình duyệt ADR-137; cơ chế/mục tiêu kiểm chứng G2
đã trình duyệt ADR-138. Protocol/grant/index candidates chi tiết bên dưới thuộc
bản tổng hợp E03-v1 đã nghiệm thu, không SQL thực thi hoặc tự duyệt chi tiết E04.

| Nội dung G2 | Cơ chế đã trình và giới hạn |
| --- | --- |
| 4 SQL roles/principals tối thiểu | Runtime chỉ quyền bảng/SP/UDF cần thiết; finance-reporting đọc VW06/SP04; academics-reporting đọc VW03/VW04/SP06; director-reporting chỉ SP04–06 tổng hợp. Không db_owner/sysadmin cho runtime, không nhập/chốt/thu tiền bằng các reporting users. Bốn user/login minh họa dùng DB test với dữ liệu giả; không thêm tài khoản/quyền web. |
| Schema/maintenance riêng | Owner chạy schema/backup/cleanup bằng principal vận hành riêng, không credential trong browser/source hoặc dùng nó làm runtime. Cleanup chỉ security/marker hết hạn và IP theo retention đã chốt, giữ mandatory/business history; không mở app purge API. E04 chốt danh tính/quyền thao tác/timeout cụ thể. |
| Transaction chung | EF, SP allocator, mandatory audit và replay dùng cùng connection/transaction; 13 nhóm TX01–13 giữ đơn vị lưu đã chốt. Recheck quyền/session/trạng thái/thời gian/rowversion ở commit; lỗi bất kỳ rollback toàn đơn vị. Không NOLOCK cho guard/report, không retry để lấy lại secret. |
| Khóa chống race | sp_getapplock theo transaction cho request và aggregate/config; parent rows và unique/CHECK/FK bổ sung. Một gate TIMETABLE cho mọi thay occupancy; acquire theo thứ tự chung, kiểm kết quả lỗi và rollback. Hai lịch không liên quan có thể chờ nhau: chấp nhận độ đơn giản ban đầu, đo50 users; nếu cần đổi resource/day phải rà lại thiết kế. |
| Snapshot báo cáo | Một transaction SNAPSHOT cho count/group/totals/page của mỗi report/export, bật ALLOW_SNAPSHOT_ISOLATION khi được phép triển khai; export là query mới. Không coi RCSI từng statement đủ cho cả report hoặc queried_at là historical-as-of. Đo chi phí version store. |
| Index và bằng chứng | Index phục vụ query thực, không tạo trùng PK/UQ; danh mục mục11 là candidates để rà redundancy/plan rồi chọn, không mặc định tạo tất cả. Minh chứng ít nhất5 query trước–sau,5 nghiệp vụ BEGIN/COMMIT/ROLLBACK và forced failure,4 principal GRANT/REVOKE/DENY, race hai connection, app gọi SQL thật, trigger/EF và scope/secret/retention cases DC01–18. |

Đơn vị transaction minh họa tối thiểu: tranh chỗ ca thi (TX02), duyệt/cấp hóa
đơn (TX04), thu tiền (TX05), lịch tuần/thay thế (TX09), chốt điểm cả lớp (TX12).
Đọc output thực tế qua view/SP khi kiểm quyền; DENY bảng gốc không tự chứng
minh module đã grant không lộ dữ liệu qua ownership chain (mục10.5).

Lý do/đánh đổi: object-level SQL grants + server auth giữ scope mà không dựng
7 SQL logins theo role web; schema/runtime riêng giảm quyền runtime. Khóa lịch
chung dễ rà invariant hơn khóa resource/day ngay từ đầu, nhưng giảm đồng thời.
Chỉ rowversion/unique không bảo vệ mọi overlap/capacity; SERIALIZABLE toàn bộ
workflow dễ tăng blocking, không chọn thay protocol gate cụ thể. SNAPSHOT giữ
báo cáo nhiều query nhất quán nhưng cần version store; index cải thiện query có
thể tăng chi phí ghi/dung lượng. Mọi lợi ích đều phải đo, chưa có test pass.

Duyệt G2 là chốt cơ chế/mục tiêu kiểm chứng đã trình, không suy nghiệm thu mọi
field/SQL/signature chưa rà, hiệu năng/backup thực tế hoặc E03. Sau G2 tổng hợp
các chi tiết còn lại và trace để owner rà bản E03 thực tế; E04/E05 còn tiếp theo.

### 9.1. Cơ chế khóa G2 và protocol chi tiết để rà tổng hợp

Services tiếp tục sở hữu quyền và workflow (ADR-095). Persistence bằng EF
Core và SQL Server cùng connection/transaction; allocator và SQL reads không
tạo connection riêng khiến audit/replay tách commit. Rowversion dùng cho
stale form; unique/check/FK là backstop. Không dùng NOLOCK cho guard/report.

Đề xuất `sp_getapplock` với LockOwner=Transaction cho request/aggregate và
config invariants. SQL lock tự giải phóng khi commit/rollback; phải kiểm return
code timeout/deadlock trước DML. Resource names chuẩn hóa, không chứa PII.
Mọi return code âm là không lấy được khóa: caller chủ động rollback, không
coi deadlock của applock tự rollback. Timeout phải hữu hạn, E04 chốt giá trị;
không dùng mặc định chờ vô hạn. Không tự retry operation trả credential secret.
Nguồn kỹ thuật: [Microsoft sp_getapplock](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16).

Để dễ chứng minh xung đột lịch cho phạm vi một trung tâm, đề xuất ban đầu
một gate `EC:TIMETABLE` cho mọi mutation thay occupancy/timetable: W05/W06/
W07/W10 cancellation/W14 resource changes/W15/W16. Gate serialize room,
Teacher, Class và Student test/class conflicts; bên trong vẫn khóa các parent
rows và recheck predicates. Nhược điểm là booking ở hai ca không liên quan
cũng chờ nhau; cần đo tại 50 users, không khẳng định đạt throughput. Nếu đo
không đạt sẽ trình phương án khóa resource/day cụ thể, không bỏ guard.

Các gate khác: `EC:BANDSET` cho config/first result; `EC:PROGRAM:<id>` cho
config/first Class; `EC:CLASS:<id>` cho placement/state/attendance/grade;
`EC:REG:<id>` cho booking/result/confirm/approval/cancellation;
`EC:INVOICE:<id>` cho Payment/cancel/metadata; `EC:ADMINSET` cho last active
Admin; request scope dùng actor/action/request_id. Sử dụng cùng protocol
cho **mọi** đường ghi tương ứng, gồm các persistence SP sau này nếu có.

Trình tự: validate/auth read → mở transaction → request-key lock → tập
business gates theo thứ tự resource name thống nhất → khóa/reload parent
rows, account/session và expected rowversions → kiểm lại rights/state/time/
relationships/capacity/overlap → DML → mandatory audit → replay marker →
commit. Xác định tập gate trước DML; nếu reload làm tập resource thay đổi,
rollback và chuẩn bị lại, không lấy khóa lộn thứ tự. Session/Account guard
được giữ tới commit để serialize với revoke; idle update không overwrite
revoked_at. Lỗi lock/unique/rowversion/business invariant phải rollback và
map 409 thích hợp ở E04, không trả raw SQL. Không tự retry operation có
credential secret để lấy lại response.

### 9.2. Ma trận transaction — nhiều hơn tối thiểu năm nghiệp vụ

| TX | W | Phải commit/rollback cùng nhau | Guard/race và minh chứng cần có |
| --- | --- | --- | --- |
| TX01 | W01 | Account + StudentProfile hoặc staff temp state + mandatory account audit | Unique email/phone đua; forced error sau Account không để orphan Profile/credential state. |
| TX02 | W07 | Attempt nếu cần + old/new Booking + state/capacity + audit theo contract + replay | Hai actor tranh chỗ cuối; cùng attempt hai request; đua session close/start; một booking hợp lệ. |
| TX03 | W08 | Entry result + completed Booking + best Band + Registration state + first-use freeze + audit/replay | Edit Band vs first result, correction vs confirmation; không partial result/Band. |
| TX04 | W10/W11 | Registration ADMITTED + Invoice/counter + audit/replay | Same approval retry; price snapshot race; forced audit/allocator failure rollback tất cả. |
| TX05 | W12 | Payment + Invoice PAID + audit/replay | Payment vs cancellation; unique one Payment; không PAID thiếu Payment hoặc over-payment. |
| TX06 | W05/W10 | Registration CANCELLED + is_unfinished + future Booking/UNPAID Invoice release + grouped audit/replay | Paid/start boundary guard; preserve started/graded history; một effects lỗi không partial. |
| TX07 | W13/W14 | Config validation/first-use lock + Class creation + mandatory audit/replay | ACTIVE config sum100/one exam; concurrent first-Class vs component/threshold change. |
| TX08 | W15 | Assignment + DRAFT GradeReport + audit/replay | Hai hồ sơ tranh chỗ Class cuối; duplicate assignment; timetable Student conflicts. |
| TX09 | W16 | Toàn bộ weekly Schedule batch hoặc old NOT_HELD + current replacement + audit/replay | Overlap/counter/old dependent data, chain fork/replay; một lỗi rollback toàn batch. |
| TX10 | W17 | Một LESSON Attendance delta batch + metadata/audit nếu contract yêu cầu | Stale một value rollback batch; completion race; missing rows không default absent. |
| TX11 | W18 | Changed Score + Teacher comment của một assignment + required audit/replay | Rowversion/component/schedule/report guards; không lưu comment mà mất score changes. |
| TX12 | W18/W19 | Mọi report của Class + totals khi final + is_unfinished + grouped mandatory audit/replay | Submit/edit/return/finalize race; all-or-none publish; không Student thấy một phần. |
| TX13 | W02/W03 | Credential/status/version + related session revoke + mandatory audit | Last ACTIVE Admin, reset/lock/password race; audit fail rollback, không restored sessions. |

Pure login/logout/denial security logging có best-effort policy riêng ADR-115;
không kéo nó vào mandatory rollback rule để khôi phục session đã logout.
Tối thiểu TX02/TX04/TX05/TX09/TX12 phải có minh họa BEGIN/COMMIT/ROLLBACK
và forced-failure trên database test tương ứng path thật; SQL trace/transaction
demo không thay application integration tests. E04 sẽ chốt cách service gọi
commands và ranh giới transaction, chưa viết demo scripts ở E03.

### 9.3. Snapshot báo cáo

Đề xuất bật ALLOW_SNAPSHOT_ISOLATION trong môi trường sau khi triển khai,
dùng **một transaction SNAPSHOT** cho toàn bộ count/group/totals/page của mỗi
report/export. RCSI một mình chỉ bảo vệ từng statement, không chứng minh
nhiều query cùng snapshot. queried_at là thời điểm query do server ghi, không
là feature historical-as-of. Export query mới, giữ filters/metrics đúng scope.
Nguồn: [Microsoft transaction isolation](https://learn.microsoft.com/en-us/sql/t-sql/statements/set-transaction-isolation-level-transact-sql?view=sql-server-ver16).
Chưa bật DB option hoặc đo version-store overhead.

## 10. Gói G — SQL objects thực sự phục vụ rubric

Đây là **danh mục thiết kế**, tên/signature/logic chưa là SQL chạy được.
Giữ EF Core code-first; migration tương lai quản lý cả tables/index/constraints
và lifecycle của SQL objects. Services giữ auth/ownership/business workflow;
repositories gọi query/allocator parameterized. Không chuyển mọi nghiệp vụ
sang procedure chỉ vì có rubric.

### 10.0. Phần G1 đã trình được duyệt ngày01/10/2026 — ADR-137

Tách G thành G1 (mục10.1–10.4: vai trò, số lượng và đường dùng thực tế của SQL
objects) và G2 (quyền SQL, transaction/concurrency/snapshot, index và kiểm chứng
rubric ở mục9/10.5/11/12). Owner đã duyệt phần G1/G2 đã trình, ADR-137/138.
Owner đã nghiệm thu riêng toàn bộ E03-v1, gồm chi tiết vật lý mục14.2;
không suy duyệt SQL signatures/mapping/grants thực thi thuộc E04/later.

| Nhóm G1 | Mục đích/đường dùng đã trình |
| --- | --- |
| 5 trigger | Khóa cấu hình Program, component và tập ngưỡng Band theo lịch sử sử dụng; bảo vệ quan hệ/trạng thái Attendance và Score. Không audit trigger, không thay service auth/ownership hoặc tự chứng minh chống race. |
| 6 view | Fact tuyển sinh, booking thi, lịch hiện hành, lớp/tiến độ, điểm FINALIZED và hóa đơn/thanh toán. Xác định grain một row/đối tượng; tổng hợp trước join tránh nhân số. |
| 6 UDF | Trạng thái ca theo thời điểm, chọn Band, hồ sơ chưa kết thúc, tiến độ nghĩa vụ, tổng điểm và cảnh báo quá hạn. Read-only, không thêm enum/phạt; thiếu điểm không là0. |
| 7 SP | Tìm hồ sơ, roster ca thi, bảng điểm cho giáo vụ, báo cáo tài chính/tuyển sinh/đào tạo và cấp số hóa đơn trong transaction duyệt của caller. |

Services vẫn quyết định quyền/ownership/workflow; views không tự là API hay
quyền đọc fact cho Director. SP query chỉ trả projection đúng scope; ba SP
báo cáo không trả danh sách định danh cho Director. SP07 không tự duyệt hay
commit độc lập. Mọi SP có TRY…CATCH/THROW, tham số và sort allowlist.
Trigger phải xử lý cả batch; audit/replay đi cùng transaction service như đã
duyệt ở F. Protocol khóa/chống race được rà G2, không mặc định an toàn từ trigger.

Repository phải gọi thực sự SP/UDF theo mapping mục10.3–10.4, không tạo object
chỉ để đủ rubric. EF code-first giữ quản lý vòng đời SQL objects qua migration
sau này. E04 còn chốt DTO/signature/mapping, tương thích ghi các bảng có trigger;
kiểm chứng SQL thật và bằng chứng app gọi chỉ có sau khi được phép triển khai.
Tên SQL ở dưới là tên đề xuất; duyệt G1 không suy duyệt mọi chi tiết enforcement
hoặc nghiệm thu E03. Lý do chọn: tái sử dụng query/công thức và đủ rubric có
nghiệp vụ thực; đánh đổi: phải duy trì mapping, quyền và kiểm parity service/SQL.
Phương án chỉ LINQ/query inline không chứng minh yêu cầu app gọi SP/UDF;
dồn mọi workflow vào SP làm phân tán quyền/audit/replay nên không chọn.

### 10.1. Năm trigger

| ID / tên đề xuất | Bảng/event | Logic và giới hạn | Luồng/ca kiểm chứng |
| --- | --- | --- | --- |
| TG01 / trg_Program_UsedConfigurationGuard | TrainingProgram AFTER UPDATE,DELETE | So inserted/deleted chặn đổi course_id/band_id/expected_lesson_count hoặc delete khi có bất kỳ Class. Không chặn name/status hợp lệ chỉ vì đã dùng. | W13/W14; cả Class CANCELLED vẫn freeze, multi-row một invalid rollback statement. |
| TG02 / trg_Component_UsedConfigurationGuard | ScoreComponentConfig AFTER INSERT,UPDATE,DELETE | Chặn thay list/program/name/weight/kind của program đã có Class; trước use vẫn cần service DRAFT/ACTIVE checks. | W13/W14; insert/delete/move component sau freeze không bypass. |
| TG03 / trg_Band_ThresholdSetFreezeGuard | Band AFTER INSERT,UPDATE,DELETE | Chặn thay set code/min_score sau bất kỳ completed entry result. First-result và config dùng cùng BANDSET gate. | W08/W13; first-use race, thêm threshold mới sau first result bị chặn. |
| TG04 / trg_Attendance_ClassScheduleGuard | Attendance AFTER INSERT,UPDATE,DELETE | Record phải thuộc assignment Class/current LESSON không NOT_HELD; chặn sửa/xóa khi Class COMPLETED/CANCELLED. Service vẫn kiểm Teacher/start và không cho delete UI. | W17/W14/W16; mixed valid-invalid batch, khác Class/FINAL_EXAM, completed mutation. |
| TG05 / trg_Score_ComponentScheduleGradeGuard | Score AFTER INSERT,UPDATE,DELETE | Cùng program/Class; GENERAL→NULL schedule, linked→current đúng type không NOT_HELD; chặn mutations khi report SUBMITTED/FINALIZED. | W18/W19; wrong kind/Class, score edit sau submit/finalize; valid batch và invalid rollback. |

Trigger dùng tập inserted/deleted, không giả định một row hoặc cursor; THROW
khi vi phạm, không swallow/commit riêng và không phát result set. Không tạo
audit bằng trigger. Config triggers phối hợp cùng gate với first-use paths;
relationship triggers phối hợp parent Class/report locking. Chỉ trigger AFTER
đọc existence không có protocol chung sẽ không tự chứng minh race-safe.
Nguồn: [Microsoft multi-row triggers](https://learn.microsoft.com/en-us/sql/relational-databases/triggers/create-dml-triggers-to-handle-multiple-rows-of-data?view=sql-server-ver16).

EF Core SQL Server OUTPUT path không tương thích một số bảng có trigger;
mapping E04 phải cấu hình persistence phù hợp cho cả năm bảng và kiểm
insert/update/generated ID/rowversion với SQL Server thật. Cơ sở kỹ thuật:
[UseSqlOutputClause, EF Core 10](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.sqlserverentitytypebuilderextensions.usesqloutputclause?view=efcore-10.0).
Đây là hạng mục cần thiết kế/kiểm chứng, chưa thay EF mapping hoặc runtime.

### 10.2. Sáu view

| ID / tên đề xuất | Grain/dữ liệu | Sử dụng và scope |
| --- | --- | --- |
| VW01 / vw_AdmissionsRegistrationFacts | Một row/Registration, current status/Course/Student reference/Band/confirmation/approval; không profile snapshot | SP01/SP05, admissions cohort/count distinct đúng kỳ; không trực tiếp trả fact rows cho Director. |
| VW02 / vw_EntryTestBookingFacts | Một row/Booking + attempt/session/result liên quan | SP02/F25 roster, capacity/history/effective session qua FN01; không coi ABSENT là score0. |
| VW03 / vw_CurrentScheduleOccurrences | Một current Schedule/obligation; Class/Teacher/date/time/room; loại Class CANCELLED khỏi active timetable | Calendar/conflict read và training progress; history riêng vẫn truy từ Schedule. |
| VW04 / vw_TrainingClassFacts | Một row/Class, Course/Band/state/max_size và pre-aggregated placed/expected/current/HELD LESSON, FINAL_EXAM flags | F19/F20/SP06; progress denominator expected LESSON, không đếm replacement hai lần; aggregate trước joins tránh nhân số. |
| VW05 / vw_FinalizedGradeFacts | Một FINALIZED report/lượt học, final_score/pass/finalized_at và Course/Band/Class/Student reference | Kết quả training cohort SP06; Student detail tiếp tục ownership qua service. Không DRAFT/SUBMITTED values. |
| VW06 / vw_FinanceInvoiceFacts | Một Invoice + tối đa một Payment; amount/status/issue/due/paid time/Course/reference IDs | SP04/F28/F30: billed, revenue, current debt; không dùng billed−period revenue làm debt. |

Views không có filter owner tùy tiện từ client và không là API/public endpoints.
Runtime service/procedure project đúng fields/aggregate; SQL reporting roles chỉ
được grant objects cụ thể phần 10.5. Không grant Director SELECT facts có
Student/Invoice/Payment IDs. Mỗi view có query thực tế và expected sample
result để kiểm sự khác biệt state/date/cohort khi implementation.

### 10.3. Sáu user-defined function

| ID / tên đề xuất | Loại và tham số | Kết quả/ứng dụng |
| --- | --- | --- |
| FN01 / ufn_EffectiveEntryTestState | Scalar(status,start_at,as_of_utc) | CANCELLED giữ CANCELLED; từ start→CLOSED; trước start giữ stored state. F06/F07/SP02 không ghi DB khi đọc. |
| FN02 / ufn_BandForScore | Inline TVF(score) | band_id/min_score của ngưỡng lớn nhất≤score; input range validated, không hardcode seed. W08 result recalculation. |
| FN03 / ufn_RegistrationUnfinished | Inline TVF(registration_id) | Predicate gốc từ state/Class/GradeReport; W05/W19 đối chiếu is_unfinished, không phụ thuộc passing. |
| FN04 / ufn_ClassObligationProgress | Inline TVF(class_id) | Expected/current/HELD LESSON và current/HELD FINAL_EXAM counts; VW04/SP06/W14 completion read. |
| FN05 / ufn_GradeTotal | Inline TVF(class_assignment_id) | Configured/entered counts, is_complete và decimal weighted total; khi thiếu thì total=NULL, half-up một lần khi đủ. W18 preview/W19 finalize/SP03. |
| FN06 / ufn_InvoiceDebtFlags | Inline TVF(status,due_date,today_vn) | is_overdue/days_overdue chỉ UNPAID và due<today; F10/F28/SP04; không thêm state hoặc phạt. |

as_of_utc/today_vn do server tính một lần mỗi request/snapshot, không GETDATE
bên trong function khiến các hàng lệch boundary. Functions read-only, không
state mutation/audit/side effect. FN05 có phạm vi riêng: nhiều component cùng
schedule không làm nhân sum; decimal intermediate đủ range, chỉ round cuối.
Backend tính theo cùng công thức để bảo vệ commands; kiểm parity giữa service
và function bằng data cases, không để hai thuật toán độc lập trôi khác nhau.

### 10.4. Bảy stored procedure

| ID / tên đề xuất | Tham số logic | Kết quả và path ứng dụng |
| --- | --- | --- |
| SP01 / usp_SearchAdmissions | course/state/date/search/page/sort từ DTO; actor đã xác thực do service cung cấp | F13/F14 allowed operational fields; amount/status Invoice nhưng không Payment/DOB/address/academic grades. VW01; stable ID tie-breaker. |
| SP02 / usp_GetEntryTestRoster | session_id, trusted actor_id, as_of_utc | F25 Teacher hiện assigned; AcademicAffairs đọc theo scope hiện có; current/history bookings và score đúng quyền. VW02/FN01. |
| SP03 / usp_GetClassGradeReview | class_id, trusted actor_id | F23 AcademicAffairs chỉ SUBMITTED/RETURNED/FINALIZED, không DRAFT values. FN05; response không mở quyền nhập Score. |
| SP04 / usp_GetFinancialReport | issue/confirmation period, course, overdue-only debt flag, today_vn | F30/F31 aggregates theo allowed report columns, không Invoice/Student dump. VW06/FN06; SUM decimal38, queried_at từ service. |
| SP05 / usp_GetAdmissionsReport | created period/course/state | F31 số Registration/distinct Student đúng cohort, tổng distinct riêng; VW01, không identifiable rows. |
| SP06 / usp_GetTrainingReport | inventory course/Band/Class/current Teacher/state và results finalization period riêng | F31/VW04/VW05/FN04; inventory không date period, results theo finalized_at; no-data denominator khác0%. |
| SP07 / usp_AllocateInvoiceNumber | issue_year do server từ issued_at VN | W10/W11 cấp một number qua T24; caller đã mở transaction; không commit riêng. |

SP01–SP06 đọc/query; SP07 là persistence allocator nhỏ, không tự approval.
Mọi SP có TRY...CATCH/THROW, parameterized input, không dynamic concatenation
search/sort. Sort thuộc allowlist, SQL details không trả client.
Khối xử lý lỗi theo [Microsoft TRY…CATCH](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/try-catch-transact-sql?view=sql-server-ver16);
không coi nó bắt được mọi lỗi/mất kết nối hoặc thay rollback của caller.
SP đọc nhiều statement report dùng transaction SNAPSHOT của caller; SP07 bắt buộc caller
transaction, khóa year row/range, fail overflow; không nested commit tách
approval/audit/replay. Ownership checks ở SP02/SP03 dùng trusted actor_id
được service lấy từ verified session; actor_id tự khai trong SQL/HTTP không
chứng minh quyền. Không mở grant execute hai SP này cho SQL demo users.

Rubric yêu cầu app gọi thật: SP01–SP06 được repository query dùng ở paths
trên; SP07 cùng approval transaction. FN01/FN04/FN05/FN06 được gọi qua SP/
views; FN02/FN03 được gọi trong persistence command/query cho W08/W05/W19.
E04 chốt DTO, mapping keyless query và API response; test tích hợp sau này
phải có bằng chứng app thực sự gọi SP/UDF, không chỉ tồn tại object trong DB.

### 10.5. Bốn database role và tài khoản SQL minh họa

Role dữ liệu T01 bảy hàng phục vụ user login/application authorization.
Database roles dưới đây phục vụ SQL Server least privilege, khác với bảy
role người dùng. Browser chỉ gọi API; connection pool dùng một runtime
principal, không chứa login SQL per Student hoặc credential lấy từ browser.

| DB role đề xuất | Quyền được grant | Quyền bị hạn chế / minh chứng |
| --- | --- | --- |
| ec_app_runtime | SELECT/INSERT/UPDATE cần cho EF trên bảng xác định, AuditLog INSERT/SELECT, reads/SP/UDF cụ thể; DELETE chỉ config draft path được cho phép | Không db_owner/sysadmin/DDL hoặc history-delete. Runtime audit SELECT hỗ trợ API Admin nhưng service vẫn enforce role. |
| ec_finance_reporting | SELECT VW06 và EXEC SP04 | DENY direct Account/StudentProfile/Score/GradeReport; không DML Payment/Invoice hoặc credential access. |
| ec_academics_reporting | SELECT VW03/VW04 và EXEC SP06 | Không Account/StudentProfile/Finance/Score base tables, không nhập/chốt điểm bằng SQL demo role. |
| ec_director_reporting | EXEC SP04/SP05/SP06 tổng hợp | Không SELECT fact views hoặc business tables, không SP01/SP02/SP03/SP07, không mutation/DDL/export entitlement. |

SQL demo permissions là quyền vận hành minh họa trên môi trường test, không
tài khoản web hoặc quyền nhân sự bổ sung. Provision ít nhất bốn DB users với
logins/Windows principals phù hợp (tên/secret cụ thể do E04/vận hành quyết định);
mỗi user gắn role khác để chứng minh quyền thực tế. Owner deploy/schema
principal tách runtime, không đưa mật khẩu/connection string vào source.

GRANT demo: cấp reporting SELECT/EXEC objects được phép. REVOKE demo: bỏ
grant đã cấp và chứng minh không còn đường cấp quyền khác. DENY demo: role
không đọc Account/StudentProfile base tables, thử query phải thất bại. Chứng
minh permissions qua view/procedure output thực tế, không chỉ kiểm metadata:
ownership chaining có thể cho đọc dữ liệu qua module/view đã grant dù base
table bị deny, nên phải rà chính các cột/output được grant. Không dùng DENY
base table như bằng chứng rằng một view chứa PII tự động an toàn. Nguồn:
[Microsoft ownership chains](https://learn.microsoft.com/en-us/sql/relational-databases/tutorial-ownership-chains-and-context-switching?view=sql-server-ver16).
Chi tiết
principal/module permissions và E04 integration vẫn phải được thiết kế/rà
trong E04; cơ chế phân nhóm G2 đã duyệt không thay ma trận grant thực thi.

## 11. Index và cách chứng minh lợi ích

PK/unique indexes đã liệt kê không được tạo duplicate index chỉ để tăng count.
Đề xuất các non-PK indexes sau; INCLUDE chọn sau khi chốt projection E04 và
execution plan, không INCLUDE hash/audit JSON hoặc mọi trường nhạy cảm.

| IX đề xuất | Key columns | Query/lý do |
| --- | --- | --- |
| IX_Registration_StudentCreated | student_id,created_at,registration_id | Student-own hồ sơ theo thời gian, stable page. |
| IX_Registration_CourseStateCreated | course_id,status,created_at,registration_id | Admissions/cohort lọc đúng Course/state/date. |
| IX_Session_GraderStart | grader_id,start_at,end_at,session_id | Lịch Teacher và overlap test/class. |
| IX_Session_RoomStart | room,start_at,end_at,session_id | Kiểm tài nguyên phòng. |
| IX_Booking_SessionState | session_id,status,booking_id | Capacity/current roster; giữ histories khi query status khác. |
| IX_Class_ProgramState | program_id,status,class_id | Class candidates matching Course/Band/program. |
| IX_Class_TeacherState | teacher_id,status,class_id | Teacher current Class/tasks. |
| IX_Assignment_Class | class_id,class_assignment_id | Class cohort/count; unique Registration riêng. |
| IX_Schedule_ClassCurrentDate | class_id,is_current,session_date,start_time,schedule_id | Calendar/current obligations. |
| IX_Schedule_RoomDate | room,session_date,start_time,end_time,schedule_id | Range overlap kiểm room, không coi index tự enforce overlap. |
| IX_Attendance_Assignment | class_assignment_id,attendance_id | Student-own/cross-LESSON completeness; UQ schedule/assignment riêng. |
| IX_Score_Component | component_id,score_id | FK component checks/query; UQ assignment/component riêng. |
| IX_Score_Schedule | schedule_id,score_id WHERE schedule_id IS NOT NULL | Linked-score dependencies block move/replacement. |
| IX_GradeReport_StateFinalized | status,finalized_at,report_id | Finalized cohort; report Class queue join qua assignment. |
| IX_Invoice_StateDue | status,due_date,invoice_id | Current debt/overdue; UQ Registration không thay filter. |
| IX_Invoice_Issued | issued_at,invoice_id | Period billed. |
| IX_Payment_Paid | paid_at,payment_id | Revenue confirmation period. |
| IX_AuthSession_AccountRevoked | account_id,revoked_at | Revoke all/account session checks. |
| IX_AuthSession_Expiry | expires_at | Operational maintenance; no app session listing. |
| IX_Audit_Time / IX_Audit_Operation | occurred_at DESC,audit_id DESC / operation_id,audit_id | F33 stable paging và grouped effects. |
| IX_Audit_ActorTime / IX_Audit_EntityTime | actor_account_id,occurred_at DESC,audit_id DESC / entity_type,entity_id,occurred_at DESC,audit_id DESC | Structured Admin filters. |
| IX_Replay_Expiry / IX_Replay_Operation | expires_at / operation_id | Expiry maintenance và unknown-response inspection. |

Ngoài danh mục trên, các FK không được prefix bởi PK/UQ/IX phải có index
cần thiết: Account.role_id; Program.band_id; BandAssignment.band_id và
best_attempt_id; các actor FK phục vụ guard/queries. Rà redundant indexes
trước migration; không nói mọi FK luôn cần index riêng bất kể workload.

Minh chứng rubric sau này chọn tối thiểu năm query: Student registrations,
Admissions search, booking capacity, room overlap, debt/revenue report.
Trên dữ liệu test đủ lớn nhưng trong phạm vi ADR-117, lưu SQL/parameters,
actual execution plan, logical reads/CPU/elapsed nhiều lần cùng workload
trước–sau index ở DB test; giữ data/result giống nhau. Chỉ rõ trường hợp
scan vẫn hợp lý và chi phí write/storage của index. Chưa có số đo trong E03.

## 12. Đối chiếu rubric và kế hoạch kiểm chứng thiết kế

| Tiêu chí PDF | Đầu ra thiết kế E03-v1 | Minh chứng còn cần khi triển khai |
| --- | --- | --- |
| ERD/schema, ≥8 related tables, 3NF | 22 entity tables + 2 technical; phần 2–8, giải thích projections/FD | Migration/schema khớp dictionary; rà 3NF/ngoại lệ vật lý. |
| ≥5 CHECK/UNIQUE/DEFAULT | Ranges/enums/event bundles, ≥5 domain UQ và initial defaults; phần 3–8 | Valid/invalid inserts/multi-row operations với SQL Server thật. |
| ≥5 trigger | TG01–TG05 | Active trigger names/count, multi-row/state/freeze tests và EF OUTPUT mapping. |
| ≥5 view | VW01–VW06 | Query sample, grain đúng, no join multiplication/PII leakage. |
| ≥5 index, có non-PK, lý do/hiệu quả | Phần 11 và unique keys | Before/after plans/reads trên ít nhất5 queries; không tự tuyên bố nhanh. |
| ≥5 SP và TRY...CATCH | SP01–SP07 | Execute success/failure đúng, map errors, app calls bằng integration evidence. |
| ≥5 UDF | FN01–FN06 | Biên ngưỡng/time/rounding/NULL/parity và real callers. |
| ≥5 transaction nghiệp vụ | TX01–TX13, chọn TX02/04/05/09/12 demo tối thiểu | BEGIN/COMMIT/ROLLBACK, forced failure, mandatory audit/replay cùng transaction. |
| Concurrency hoặc recovery | Last-seat booking, pay-vs-cancel, config freeze và whole-Class publish | Hai connections barrier-controlled, committed state sau race; restore theo ADR-118 nếu chọn thêm. |
| ≥4 SQL Role/Login, GRANT/REVOKE/DENY | Bốn DB roles/users với object permissions phần10.5 | Login as từng principal, allowed/denied executions/outputs. |
| App calls SP/UDF, search/report/error | Ma trận path phần10.3–10.4 | E04 contracts rồi chạy demo SQL connection/config/error, không mock thay DB. |

### 12.1. Ma trận kiểm chứng trọng yếu

| Case | Dữ liệu/đua cần thiết | Kết quả phải thấy |
| --- | --- | --- |
| DC01 | Hai request cùng email/phone, Student-profile create fail | Không duplicate hoặc orphan; role/status policy đúng. |
| DC02 | Một Student/Course repeat sau FINALIZED fail/pass và khi CANCELLED/REJECTED | Unique chỉ unfinished; không cần passing; is_unfinished không lệch. |
| DC03 | Attempt 1/2, ABSENT/center cancel/rebooking/tie | Tối đa hai logical attempts, current/complete booking pointer đúng; absent≠0. |
| DC04 | Two actors last seat, same actor same request khác content | Không overcapacity; không side effect lặp; changed content409. |
| DC05 | Shared Teacher/room/Student class+test overlap, adjacent endpoints | Overlap reject, [a,b)/[b,c) hợp lệ; weekly batch rollback toàn bộ. |
| DC06 | Multi-hop replacement LESSON/FINAL_EXAM, old dependent Score/Attendance | Current unique/no fork/no cycle; không tăng obligations; dependency chặn. |
| DC07 | Class completion vs Attendance write/HELD/replacement | Không completed thiếu required HELD/Attendance; khóa sau completion. |
| DC08 | Config edit vs first Class/first entry score, Class sau đó CANCELLED | Freeze không bypass bằng concurrency/history deletion. |
| DC09 | Grade thiếu component, 0 thật, round49.995/49.994, wrong schedule kind | Missing≠0; round50.00/49.99; range/precision input rejected. |
| DC10 | Per-Student Score/comment fail; Class submit/return/finalize race | Không partial draft/whole-Class publish; Student thấy FINALIZED own only. |
| DC11 | Payment vs cancel, duplicate request, metadata correction | One Payment đúng amount; paid_at/snapshot bất biến, reason/history đủ. |
| DC12 | Invoice issue year VN vs UTC, +7 calendar days, overflow/year-row race | Number format/year/due đúng; no reused issued number hoặc partial approval. |
| DC13 | Mandatory AuditLog write fail; pure logout log fail | Mandatory rollback mutation/revoke; logout không resurrect vì pure log fail. |
| DC14 | 90-day security/IP cleanup trên mandatory row, grouped Grade history | Mandatory events giữ nguyên; clear IP đúng, không xóa history để né freeze. |
| DC15 | Shared session tabs/devices, throttle/expiry/revoke/last Admin race | No session revival/auto unlock; normal/restricted lifetime đúng; còn ACTIVE Admin. |
| DC16 | Multi-query report + concurrent payment/finalize, fresh export | Snapshot consistent/queried_at, aggregate/no PII Director, exact money. |
| DC17 | SQL users và view/procedure ownership chain | Allowed outputs đúng; denied base tables không bị coi là đủ khi view lộ fields. |
| DC18 | EF writes tables có trigger, ID/RV updates, invalid multi-row write | Compatible persistence, concurrency exceptions và rollback đúng. |

### 12.2. Trace đầy đủ W → dữ liệu

| W | Tables/SQL objects/enforcement chính |
| --- | --- |
| W01 | T01–T03/T22/T23, TX01/unique email-phone. |
| W02 | T02/T04/T22, TX13/session-throttle/version; không replay credential input. |
| W03 | T02/T04/T22/T23 safe marker, TX13/ADMINSET. |
| W04 | T02/T03 và projection T09/T20/T15/T16/T19, ownership/no snapshots. |
| W05 | T05/T09/T11–T13/T20/T22/T23, FN03/TX06/unfinished unique. |
| W06 | T10/T12/T22/T23, FN01/TIMETABLE/session guard. |
| W07 | T09–T12/T15/T16/T22/T23, TX02/current-booking/capacity. |
| W08 | T06/T09/T11–T13/T22/T23, FN02/TG03/TX03/BANDSET. |
| W09 | T09/T11–T13/T22/T23, REG confirmation/result guards. |
| W10 | T09/T20–T24, SP07/TX04–TX06. |
| W11 | T05/T09/T20/T24, FN06/SP07/Invoice snapshot. |
| W12 | T09/T20–T23, TX05/metadata lock and history. |
| W13 | T05–T08/T14/T22/T23, TG01–TG03/config gates. |
| W14 | T07/T14–T19/T22/T23, FN04/Class state/cohort lock. |
| W15 | T07/T09/T13–T16/T19–T23, TX08/assignment unique/capacity. |
| W16 | T10/T12/T14–T18/T22/T23, TX09/TIMETABLE/current obligation. |
| W17 | T14–T17/T22/T23 theo contract, TG04/TX10. |
| W18 | T07/T08/T14–T19/T22/T23, FN05/TG05/TX11–TX12. |
| W19 | T09/T14–T19/T22/T23, FN03/FN05/SP03/TX12/grouped history. |
| W20 | VW01/VW03–VW06/FN04–FN06/SP04–SP06, one SNAPSHOT/report. |
| W21 | T22/T23 plus entity event bundles, mandatory-vs-security audit retention. |

NV/F trace kế thừa bảng W→37 NV/32 F của WORKFLOW_SPEC phần10; các W trên
không tạo NV/F mới. E05 phải kiểm requirement→NV/F→W→field/constraint/API
và các new physical details đã được owner duyệt, không chỉ kiểm số bảng.

## 13. Gói rà và ranh giới nghiệm thu

| Gói tài liệu | Phạm vi owner rà | Tình trạng |
| --- | --- | --- |
| A | Mô hình24 bảng, hai bảng kỹ thuật, quan hệ/current-profile/snapshot, current Schedule và parent rowversion đã trình | Owner duyệt01/10/2026, ADR-131; không suy duyệt chi tiết gói khác. |
| B | Role/Account/Profile/AuthSession, kiểu lưu và giao dịch trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-132; bổ sung chốt qua E03-v1. SQL implementation/hash capacity còn kiểm chứng. |
| C | Config/Registration/entry test/Band, chi tiết PENDING→TESTING trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-133; đã đồng bộ E02. Bổ sung chốt qua E03-v1; implementation thuộc E04/later. |
| D | Class/placement/current Schedule/replacement và DRAFT cùng xếp lớp trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-134; đã đồng bộ W15. Bổ sung chốt qua E03-v1; implementation thuộc E04/later. |
| E | Attendance/Score/GradeReport/history, finalized-only final_score trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-135; đã đồng bộ E02. Bổ sung chốt qua E03-v1; implementation thuộc E04/later. |
| F | Invoice/Payment/Audit/replay/counter trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-136; bổ sung chốt qua E03-v1, implementation thuộc E04/later. |
| G1 | Vai trò/số lượng/đường gọi 5 trigger,6 view,6 UDF,7 SP trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-137; chi tiết mục14.2 chốt qua E03-v1, executable signatures/mapping/enforcement thuộc E04/later. |
| G2 | Cơ chế DB principals, concurrency/snapshot/index, transaction và verification rubric trong phạm vi đã trình | Owner duyệt01/10/2026, ADR-138; protocol/chi tiết vật lý chốt qua E03-v1, chi tiết E04 và bằng chứng chạy chưa được duyệt/xác minh. |

Các gói là cách chia tài liệu thảo luận, không tạo bảng checkbox phê duyệt
thứ hai. OWNER_REVIEW là nơi duy nhất tích nghiệm thu E03. Chỉ khi mọi phần
được rà, các khác biệt/gap được giải quyết và owner chấp nhận **bản tổng hợp
thực tế** mới đổi E03-v1 Accepted/stage3 done. Mô hình24 bảng/quy ước A đã
được duyệt, phần B–F đã trình duyệt ADR-132–136 và G1 mục đích/số lượng/đường
dùng đã trình duyệt ADR-137; cơ chế G2 đã trình duyệt ADR-138. Không suy chấp
nhận toàn bộ chi tiết từ các gói: mục14 trình phần tổng hợp cần nghiệm thu;
signature/grant/runtime/enforcement thực thi và bằng chứng chạy thuộc E04/later.
E04/API/triển khai và
E05 full baseline còn tiếp theo; không có quyền code từ tài liệu này.

## 14. Bản tổng hợp E03-v1 đã được owner nghiệm thu

### 14.1. Phạm vi đã nghiệm thu ngày01/10/2026

Owner đã duyệt các phần A–G2 được trình trong ADR-131–138; không hỏi lại các
quyết định đó. Owner đã đồng ý nghiệm thu **bản DATABASE_SPEC E03-v1
thực tế này**, gồm phần1–13 và các chi tiết vật lý mục14.2 không được duyệt
riêng; lời duyệt không áp dụng tài liệu/API/SQL tương lai. Đây là baseline thiết kế DB,
không phải schema đã deploy hoặc xác nhận mọi tiêu chí rubric đã chạy đạt.

| Nhóm đầu ra | Có trong bản tổng hợp |
| --- | --- |
| Mô hình/dictionary | 24 bảng T01–T24,230 trường; ERD và ownership, type/NULL/key/default, provenance và rowversion. 22 thực thể nghiệp vụ + ReplayOperation/InvoiceNumberCounter kỹ thuật. |
| Integrity/lịch sử | PK/FK NO ACTION, hai composite FK cho completed booking/best attempt, UQ/filtered UQ và CHECK/default theo24 bảng; quan hệ liên bảng do service/trigger/gates bảo vệ. History không bị xóa để né freeze, thiếu Score/Attendance không có row. |
| Chuẩn hóa/projections | Phụ thuộc chức năng mục8.3, dữ liệu cá nhân hiện tại không copy vào hồ sơ/hóa đơn; snapshot tiền/finalized score và projection unfinished/current/best-Band có nguồn và transaction bảo vệ. Không tuyên bố mọi projection là logical3NF thuần túy. |
| SQL dùng thực tế | 5 trigger,6 view,6 UDF,7 SP với grain/logic/role/path ứng dụng; audit do service, allocator không commit riêng; bảng có trigger cần EF mapping tương thích. |
| Nhất quán/quyền | 13 nhóm transaction, cơ chế khóa request/aggregate/TIMETABLE, một report/export SNAPSHOT;4 SQL permission groups, runtime/operator riêng và output đúng scope. |
| Rubric/kiểm chứng/trace | Mục12 đối chiếu rubric ADR-130,18 ca DC01–18 và trace đủ21 W; kế thừa37 NV/32 F của E02. Index candidates có query/lý do và yêu cầu đo, không danh mục bắt buộc tạo hết. |

### 14.2. Chi tiết vật lý được nghiệm thu cùng bản E03-v1, không duyệt riêng

Các chi tiết dưới đây cụ thể hóa policy đã chốt, không thêm màn hình, quyền,
nghiệp vụ hay package/framework. Tên cột/key/SQL objects trong dictionary là
tên thiết kế của bản này; SQL syntax và mapping thực thi vẫn thuộc bước sau.

| Chi tiết còn lại | Thiết kế trong bản E03-v1 |
| --- | --- |
| Kiểu/lưu kỹ thuật | Dictionary là danh sách đầy đủ230 trường. Role code varchar32; scope varchar24; action/action_code varchar80; entity_type varchar64/entity_id varchar100; result/category varchar16; safe result mode varchar32. Các độ dài này là giới hạn kỹ thuật nội bộ, không tăng quyền/độ dài input nghiệp vụ. |
| Text/decimal | NFC/code-point validation ở service; nvarchar cấp đủ UTF-16 theo hạn đã chốt, component name CI/AS. Collation đề xuất Latin1_General_100_CI_AS_SC cho tên; mã/email dùng normalization/so sánh đã chốt. Weighted intermediate decimal28,10, tiền tổng decimal38,0; kiểm input trước conversion, không cắt/round ngầm. |
| NULL/default/bundles | Mục8.2a kiểm đủ actor/time/reason/operation theo sự kiện, NULL có chủ đích; enum/range/pattern và default initial state/counters. Auth normal/restricted và revoke độc lập throttle; không default score/present/amount/actor. Domain invariants liên bảng không bị coi là CHECK đơn bảng. |
| Audit/replay | Audit result SUCCESS/FAILURE, category MANDATORY/SECURITY; details nullable JSON, safe replay references JSON không PII/secret. Digest binary32 SHA-256 trên input nghiệp vụ đã validate, không credential; exact canonical serialization thuộc E04. Replay giữ ít nhất24h, mục7 đề xuất cleanup từ24h; security/IP theo90 ngày đã chốt, giữ mandatory history. |
| Keys/locks/projections | Tên UQ/AK/FK/CHECK và điều kiện filtered unique theo mục8; cập nhật current Schedule/unfinished/best-Band và parent version cùng transaction. Protocol/tên gate ở mục9 là chi tiết trình trong bản tổng hợp, không tự thay invariant hoặc cho phép một đường ghi bỏ gate. |
| Views/functions/SP/index | Tên/grain/path ở mục10; tham số ở mức logic, chưa SQL signature/DTO. Mục11 là index candidates, không tạo trùng PK/UQ; INCLUDE/dedup/selection theo E04 projection và phép đo thật, không chốt tất cả index hoặc cam kết tốc độ. |

### 14.3. Đối chiếu và kiểm tra tài liệu ngày01/10/2026

Đã kiểm cấu trúc: T01–T24 đủ24 bảng/230 trường, mỗi bảng một PK và không
trùng field name trong bảng; mọi bảng có hàng CHECK/default; IDs đủ TG01–05,
VW01–06,FN01–06,SP01–07,TX01–13,DC01–18 và4 DB roles. Bảng trace có đúng
W01–21, không tham chiếu T ngoài01–24; nối với trace E02 giữ37 NV/32 F,
không NV10/NV11/F15 hoặc chức năng mới.

Đã rà các invariant trọng yếu với requirements/E02/ADRs: PENDING→TESTING và
lượt hai giữ BAND_ASSIGNED; initial GradeReport DRAFT cùng xếp lớp; score
thiếu≠0/final_score chỉ FINALIZED; one obligation/current replacement và HELD
explicit; Payment/PAID/audit/replay atomic; snapshot Invoice không profile
snapshot; whole-Class publish/unfinished; freeze dựa history; session revoke
không hồi sinh và Director không định danh. Không phát hiện quyết định nghiệp
vụ mới cần mở lại trong lượt tổng hợp này.

Đây là kiểm cấu trúc/đối chiếu tài liệu, **không** SQL compile, schema introspection,
application/permission/concurrency/EF tests hoặc phép đo hiệu năng. Chưa tạo/
chạy SQL, migration, app, test hoặc vận hành. Counts không tự chứng minh rubric
đạt hoặc mọi implementation đúng; ca DC/TX cần bằng chứng sau này.

### 14.4. Việc chuyển E04 hoặc kiểm chứng sau triển khai

| Phần chưa thuộc nghiệm thu thiết kế DB | Việc phải làm ở đúng giai đoạn |
| --- | --- |
| API/service/repository | E04 chốt endpoint/DTO/fields/ownership/errors/expected versions/request_id, transaction/caller và exact SP/UDF signatures. |
| EF/SQL integration | E04 chốt keyless projections và trigger-compatible writes/generated IDs/rowversions; SQL SUM decimal38 phải được chuyển sang chuỗi tiền chính xác theo ADR-091, không thu hẹp qua C#/JS numeric ngoài range. Chỉ triển khai mới kiểm SQL thật/parity và app callers. |
| Security/operations | E04 chốt principal/module grants cụ thể, bounded lock timeout, canonical digest/JSON allowlist/secret-free session references, hash/runtime capacity và maintenance workflow. Không đặt secret trong tài liệu. |
| Index/report/performance | Chọn index/INCLUDE qua projection/workload và before–after plans/reads/CPU/elapsed; kiểm snapshot/version-store overhead và50-user contention. Chưa số đo/throughput guarantee. |
| Deployment/backup | D12/D13 vẫn hoãn; E04 và vận hành phải chốt tunnel/HTTPS/proxy/startup, nơi backup/quyền/missed runs và minh chứng restore theo policy đã duyệt. Không tự provision/public exposure. |

Cơ sở cho mục kiểm range/mapping, không là thay đổi money contract ADR-091:
[SQL Server decimal precision](https://learn.microsoft.com/en-us/sql/t-sql/data-types/decimal-and-numeric-transact-sql?view=sql-server-ver16)
và [System.Decimal range](https://learn.microsoft.com/en-us/dotnet/api/system.decimal?view=net-10.0).
E04 phải chứng minh projection giữ nguyên chuỗi tổng tiền chính xác, không
giả định SQL decimal38 luôn materialize được bằng System.Decimal.

### 14.5. Điều kiện chuyển bước

Owner đồng ý toàn bộ **bản E03-v1 thực tế** ngày01/10/2026. Tài liệu Accepted,
E03 trong OWNER_REVIEW [x], stage3 done; chuyển thiết kế E04, stage4 doing.
E04/E05 vẫn [ ] và cần đầu ra/lời nghiệm thu riêng; E05 không tự cấp quyền
code, kế hoạch triển khai chỉ bàn khi owner yêu cầu sau baseline. Không hỏi
lại A–G2 hoặc E03 đã nghiệm thu; không có SQL/code/test/deploy từ lời duyệt.
