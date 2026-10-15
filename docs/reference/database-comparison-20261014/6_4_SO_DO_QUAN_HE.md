# 6.4 — Đối chiếu sơ đồ quan hệ trong báo cáo

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Kết luận

**Ảnh 6.4 là snapshot sơ đồ cũ, không phải hình của schema đang triển khai.** Không dùng ảnh này để chứng minh database hiện tại hoặc để sửa lại code cho giống báo cáo. Mục 6.2 và danh mục FK bằng chữ đã mới hơn ảnh.

[Ảnh gốc trích từ 6.4](report-1011-0.png). Chưa tạo hình thay thế trong lượt đối chiếu này.

## 2. Đối chiếu những nhóm cột/quan hệ nhìn thấy trong ảnh

| Đối tượng | Ảnh cũ | Hiện tại | Đánh giá |
| --- | --- | --- | --- |
| ScholarshipGrant/Candidate/Criteria | Có đủ ba bảng/quan hệ trong ảnh | Loại khỏi hình; không bảng trong source và requirements ngoài phạm vi | Sai scope. |
| Account.locked_until | Có trong ảnh | Không có; failure_window_started_at/login_throttled_until/security_version/temp fields/RV | Thay tập cột tài khoản. |
| Band.max_score | Có trong ảnh | Không có; min_score unique và threshold selection | Bỏ max_score, giữ min_score. |
| EntryTestAttempt.session_id | Có FK trực tiếp ca thi | Không có; Booking.attempt_id/session_id nối hai bảng | Bỏ session_id và thêm Booking. |
| EntryTestSession | session_date/session_time, thiếugrader/state | start_at/end_at/room/capacity/grader_id/status/cancel bundle/RV | Thay bằng schema bảng 12 cột hiện tại. |
| StudentClassAssignment | assignment_id/class_id/student_id | class_assignment_id/class_id/registration_id/assigned_by/assigned_at | Thay PK và FK Student bằng Registration. |
| GradeReport | assignment_id/approved_by/approved_at | class_assignment_id/reviewed_by/reviewed_at/review_operation_id, submitoperation/reason/comment/finalized/RV | Bỏ approval tier; giữ reviewer hiện hành. |
| Class | approved_by/approved_at | Không có; status/cancel metadata/RV; explicit lifecycle Giáo vụ | Bỏ hai cột approval. |
| TrainingProgram | approved_by/approved_at | Không có; expected_lesson_count/RV/frozen configuration | Bỏ approval; thêm cấu hình đúng. |
| Score | assignment_id, thiếu schedule_id/RV | class_assignment_id + component_id + schedule_id nullable theo kind + provenance/RV | Cập nhật tên/link. |
| Attendance | assignment_id, thiếu entered metadata/RV | class_assignment_id/entered_by/entered_at/RV | Cập nhật tập cột 7 cột. |
| Invoice | Thiếu số và issuance/cancel fields | invoice_number/issued_at/amountsnapshot/due/cancelbundle/RV | Vẽ đủ 13 cột hoặc dùng bản rút gọn có chú giải. |
| Payment | Thiếu reference/correction/RV | transaction_reference/correction metadata/RV | Vẽ đủ 11 cột; methodTRANSFER. |
| AuthSession/AuditLog | Chưa có | Thêm các bảng với đúng field/FK/nullability | Thiếu. |
| ReplayOperation/InvoiceNumberCounter | Chưa có | Thêm hai bảng kỹ thuật; Counter không FK | Thiếu. |
| Rowversion/operation IDs/filtered keys | Không thể hiện đầy đủ | Phản ánh 20 RV, 2 AK và conditional indexes theo schema source | Bổ sung hoặc chú giải rút gọn rõ. |

## 3. Danh sách phải có khi vẽ lại

Dùng đủ 24 bảng và 230 cột ở [6.2](6_2_CHI_TIET_BANG.md) và file chi tiết từng bảng. Dùng 50 FK ở [6.3.4](6_3_4_RANG_BUOC_THAM_CHIEU.md). Không gõ tay từ ảnh cũ; đặc biệt các tên assignment_id, session_date, approved_by đã không khớp model.

Các quan hệ 0..1 cần thấy rõ:

- Account có tối đa một StudentProfile; staff không có Profile theo service.
- Registration có tối đa một BandAssignment, Invoice và StudentClassAssignment; chưa đi tới bước đó thì có thể chưa có.
- Invoice có tối đa một Payment; UNPAID chưa có Payment.
- StudentClassAssignment có tối đa một GradeReport theo unique; placement tạo DRAFT cùng transaction để assignment vận hành luôn có report.
- Attempt có completed Booking optional theo result bundle; Booking lịch sử nối cùng Attempt bắt buộc, không giới hạn history bằng số logical Attempt.
- Schedule root không predecessor; mỗi predecessor có tối đa một successor; một current occurrence/obligation, tối đa một current FINAL_EXAM/Class.

Hai FK ghép phải ghi cả hai cột, không vẽ scalar link thay thế. AuthSession SID binary32 và Counter issue_year PK không IDENTITY; các PK int nghiệp vụ/Replay có IDENTITY. AuditLog actor optional; Replay actor bắt buộc.

## 4. Cách trình bày dễ đọc và đủ kiểm chứng

1. Một trang tổng thể 24 bảng, ưu tiên đường nghiệp vụ chính; actor FK nhiều đường có thể tách trang phụ.
2. Trang chi tiết từng miền: identity/session; cấu hình/entry; lớp/lịch/kết quả; finance/audit/replay. Bảng có field/PK/AK/FK/RV đầy đủ.
3. Chú giải ghi PK, FK ghép, nullable, unique/filter; không dùng một màu chung khiến optional/mandatory khó đọc khi in.
4. Dùng NO ACTION cho các FK; không cascade-delete dữ liệu lịch sử.
5. InvoiceNumberCounter đứng riêng: allocator là xử lý, không relationship FK. Audit entity/operation correlation ghi bằng note, không nối FK giả.

## 5. Đoạn thuyết minh thay thế

Sơ đồ quan hệ phản ánh model triển khai hiện hành gồm 24 bảng do ứng dụng quản lý, 24 khóa chính, hai alternate key phục vụ FK ghép và 50 khóa ngoại NO ACTION. Các liên kết của lịch thi đi qua EntryTestBooking; liên kết học viên với lớp đi qua Registration và StudentClassAssignment. Metadata xử lý, lịch sử occurrence/replacement, optimistic rowversion và hai bảng kỹ thuật bảo vệ giao dịch được giữ theo schema. Một số điều kiện như quyền actor, capacity/overlap, freeze và whole-Class finalization được kiểm ở transaction/service/trigger, không chỉ thể hiện bằng đường FK trên hình.


Nguồn dùng chung và phương pháp: [README](README.md).
