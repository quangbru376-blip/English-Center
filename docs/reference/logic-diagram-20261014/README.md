# Sơ đồ logic dữ liệu mục 6.1

Ngày: 14/10/2026. Sơ đồ thay thế hình cũ theo 24 bảng trong dự án. Word nguồn không thay đổi.

- [PNG độ phân giải cao](../../SO_DO_LOGIC_6_1_20261014.png): chèn vào Word.
- [SVG vector](../../SO_DO_LOGIC_6_1_20261014.svg): phóng to/in rõ và chỉnh sửa bằng công cụ vector.
- [PDF một trang A3 ngang](../../SO_DO_LOGIC_6_1_20261014.pdf): xem/in; khi in A4 ngang chọn Fit to page.
- [Nguồn Graphviz DOT](SO_DO_LOGIC_6_1_20261014.dot): chỉnh tên/nhóm/bố cục từ mô hình.
- [Danh mục 50 FK](DANH_MUC_50_FK.csv): phân biệt 32 FK trên hình và 18 FK trong bảng dưới.

## Cách đọc

Mũi tên đi từ bảng con chứa FK tới bảng cha. Đầu đặc cho FK bắt buộc, đầu rỗng khi FK nullable; không là ký hiệu một/nhiều. Nét đứt chỉ hai FK ghép. Các đường cắt nhau không tạo quan hệ hay junction nếu không đi vào một hộp bảng; hộp nhóm chỉ tổ chức bố cục, không SQL schema khác dbo.

- EntryTestAttempt(completed_booking_id,attempt_id) → EntryTestBooking(booking_id,attempt_id), tùy chọn theo result bundle.
- BandAssignment(best_attempt_id,registration_id) → EntryTestAttempt(attempt_id,registration_id), bắt buộc.
- Schedule self-link là replaces_schedule_id → schedule_id, không tự tạo buổi thay.
- InvoiceNumberCounter không FK sang Invoice. Audit entity_id và operation correlations không được vẽ thành FK giả.

Sơ đồ6.1 chỉ tên bảng/quan hệ logic chính, không mô tả toàn bộ cột hoặc cardinality của sơ đồ quan hệ vật lý6.4. Có đủ24 bảng,32FK chính;18FK người xử lý cùng trỏ Account được liệt kê để giữ khả năng đối chiếu đủ50FK. Teacher của Class và grader của Session được giữ trên hình vì là phân công vận hành chính.

## 18 FK người xử lý được trình bày riêng

| Bảng con | Cột FK | Tham chiếu |
| --- | --- | --- |
| Registration | admission_approved_by | Account(account_id) |
| Registration | cancelled_by | Account(account_id) |
| Registration | rejected_by | Account(account_id) |
| EntryTestSession | cancelled_by | Account(account_id) |
| EntryTestAttempt | graded_by | Account(account_id) |
| EntryTestBooking | absent_by | Account(account_id) |
| EntryTestBooking | cancelled_by | Account(account_id) |
| Class | cancelled_by | Account(account_id) |
| StudentClassAssignment | assigned_by | Account(account_id) |
| Schedule | held_by | Account(account_id) |
| Schedule | not_held_by | Account(account_id) |
| Attendance | entered_by | Account(account_id) |
| Score | entered_by | Account(account_id) |
| GradeReport | reviewed_by | Account(account_id) |
| GradeReport | submitted_by | Account(account_id) |
| Invoice | cancelled_by | Account(account_id) |
| Payment | confirmed_by | Account(account_id) |
| Payment | last_corrected_by | Account(account_id) |

Nullable của từng cột và NO ACTION được xem ở [đối chiếu6.3.4](../database-comparison-20261014/6_3_4_RANG_BUOC_THAM_CHIEU.md). FK về Account chỉ kiểm tồn tại, không tự chứng minh current role/ACTIVE/ownership. Không thêm các actor phụ này như bảng hoặc role mới.

## Kiểm chứng

Nguồn: schema trích và đối chiếu migration/EF snapshot trong lượt database, kiểm lại danh mục FK với ExpectedCatalog hiện tại. 24 tên bảng khớp source; 32 +18 =50FK, hai composite/self-reference được giữ đúng; không học bổng, không Attempt.session_id, không Assignment.student_id. Không chạy SQL/migration hoặc thay dữ liệu.
