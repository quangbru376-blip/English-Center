# 6.3.1 — Đối chiếu ràng buộc miền giá trị

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Đối chiếu các miền giá trị

| Đối tượng | Báo cáo 6.3.1 | Hiện tại | Kết luận/sửa |
| --- | --- | --- | --- |
| Role | STUDENT/ADMISSIONS/ACADEMIC_AFFAIRS… | Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin; CK_Role_Code dùng BIN2 | Sai literal; thay toàn bộ tập role_code. |
| Account | Email hợp lệ; ACTIVE/LOCKED/DISABLED; count≥0; mô tả khóa theo đếm sai | Enum/count khớp; thêm security_version>0/phone/temp bundle. Login throttle riêng, không manual LOCKED | Khớp một phần; sửa giải thích đếm sai/khóa. |
| Course | base_price≥0; ACTIVE/INACTIVE | CK_Course_Price: base_price>0; enum khớp | Sai biên tiền; 0 không hợp lệ. |
| Band | min_score0–100, không max_score | CK_Band_MinScore khớp, ngưỡng min lớn nhất≤score; seed/freeze | Khớp; thêm first-result freeze và seed. |
| TrainingProgram | DRAFT/ACTIVE/INACTIVE, không Director approval | Enum khớp; expected_lesson_count>0; activation/freeze ở service/trigger | Khớp; bổ sung count và tầng enforcement. |
| ScoreComponentConfig | weight>0≤100 | CK_Weight khớp; thêm kind GENERAL/LESSON/FINAL_EXAM và component name | Thiếu miền kind. |
| Registration | Bảy trạng thái đã chốt | PENDING/TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED/REJECTED/CANCELLED; is_unfinished bit riêng | Khớp; không thêm COMPLETED hoặc trạng thái Đã xếp lớp. |
| EntryTestSession | capacity1–20, OPEN/CLOSED/CANCELLED | Khớp; ca mới CLOSED; start/end phút UTC cùng ngày VN, room/membership/current grader guard | Khớp; thêm effective CLOSED từ start. |
| EntryTestAttempt | attempt1/2; status SCHEDULED/COMPLETED/ABSENT/CANCELLED | Attempt chỉ number/score/bundle. Status nằm EntryTestBooking | Sai đối tượng; chuyển status sang Booking. |
| EntryTestBooking | Chưa có mục miền riêng | SCHEDULED/COMPLETED/ABSENT/CANCELLED, bundle absence/cancel và indexes theo Attempt | Thiếu mục; bổ sung. |
| Class | max1–15, PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED | Khớp CK; lifecycle guards riêng, no approval state | Khớp. |
| Schedule | LESSON/FINAL_EXAM | Thêm PLANNED/HELD/NOT_HELD, is_current/obligation, minute/window rules | Thiếu occurrence state và nghĩa vụ. |
| Attendance | present0/1 | bit bắt buộc, không default; thiếu row không Vắng | Khớp; nêu rõ missing khác0. |
| Score | 0–100 | Khớp CK decimal(5,2); input dư2 số lẻ bị chặn trước lưu; kind/schedule guard | Khớp; không để SQL convert làm tròn input sai. |
| GradeReport | DRAFT/SUBMITTED/RETURNED/FINALIZED, final0–100 | Khớp enum/range; final_score NULL trước FINALIZED, total draft tính khi đọc | Khớp; thêm điều kiện lưu final. |
| Invoice | amount≥0; UNPAID/PAID/CANCELLED | amount>0; enum khớp, number pattern/due+7; overdue không state | Sai biên tiền. |
| Payment | amount_paid≥0; CASH/BANK_TRANSFER | amount_paid>0; CASH/TRANSFER; reference khi TRANSFER | Sai biên và literal phương thức. |
| AuditLog | action/entity/time bắt buộc, actor NULL khi tự động, IP hợp lệ | actor NULL khi chưa xác thực; thêm SUCCESS/FAILURE, MANDATORY/SECURITY, JSON và retention bundles | Cần sửa ý nghĩa actor NULL và bổ sung miền kỹ thuật. |
| AuthSession | Chưa có mục miền riêng | NORMAL/PASSWORD_CHANGE, temporal bounds, binary32 SID, security version; auth deadline/idle ở service | Thiếu; bổ sung. |
| ReplayOperation | Chưa có | SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE, JSON result_reference, expires≥completed+24h | Thiếu bảng và miền. |
| InvoiceNumberCounter | Chưa có | issue_year1–9999; last_value0–99.999.999, initial0 | Thiếu bảng và miền. |

## 2. Cách phân tầng kiểm tra khi viết lại

1. SQL type/NOT NULL/CHECK bảo vệ range, enum hoặc bundle trên row.
2. Validation kiểm định dạng email/mobile VN, Unicode/NFC/code point và precision input; capacity nvarchar không thay validator.
3. Service transaction kiểm role/ownership/current task/trạng thái liên bảng/cutoff thời gian.
4. Trigger bảo vệ các invariant cụ thể và đòi caller đã giữ gate; không tự kiểm tất cả quyền hoặc xử lý workflow.

Không nói “SQL CHECK bảo đảm email đúng/Student có Profile/grader đúng Teacher” nếu predicate chỉ kiểm scalar hoặc FK. Không nói bit0 là Vắng khi row chưa tồn tại.

## 3. Câu sửa ưu tiên

- Role.role_code thuộc đúng bảy literal triển khai: Student, Admissions, AcademicAffairs, Teacher, Accountant, Director, Admin.
- Course.base_price, Invoice.amount và Payment.amount_paid là decimal(18,0) dương; counter kỹ thuật last_value được phép0, không áp quy tắc tiền dương vào counter.
- Payment.method thuộc CASH hoặc TRANSFER; TRANSFER cần transaction_reference.
- EntryTestAttempt.attempt_number thuộc1/2; trạng thái lịch hẹn SCHEDULED/COMPLETED/ABSENT/CANCELLED thuộc EntryTestBooking.status. Một Attempt có kết quả khi bundle result đầy đủ và trỏ completed Booking đúng lượt.
- Audit actor_account_id nullable cho ngữ cảnh chưa xác thực; sự kiện tự động không phải lý do để tùy tiện bỏ actor đã xác minh của action nguồn.

Predicate CK thật của mỗi bảng có trong [các file bảng](6_2_CHI_TIET_BANG.md), lấy trực tiếp source catalog để không viết lại sai tên/biên.


Nguồn dùng chung và phương pháp: [README](README.md).
