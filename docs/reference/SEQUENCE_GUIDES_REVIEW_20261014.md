# Rà soát bộ hướng dẫn sequence — 14/10/2026

Phạm vi: 25 hướng dẫn Markdown SQ01–SQ25 tại `docs/design/sequence-guides-20261014`, một README làm mục lục và ZIP bàn giao. Tài liệu vẽ mức nghiệp vụ trong EA, theo mẫu SQ01; không sinh ảnh hoặc sửa mã sản phẩm.

## Nội dung đã đối chiếu

- Giữ các mã SQ cũ, mỗi nhóm một file; tách hình con cho khác actor/action/giao dịch. Chín hướng dẫn có sẵn được cập nhật trong bản sao; mười sáu hướng dẫn còn lại được viết theo workflow và code hiện hành.
- Quyền: Student own scope; Teacher current task; Admissions quyết định hồ sơ; AcademicAffairs tổ chức/chốt; Accountant thu tiền/CSV; Director aggregate-only; Admin account/audit viewer.
- Registration: tự tạo PENDING, đặt đầu TESTING, score BAND_ASSIGNED, Student CONFIRMED, Admissions ADMITTED + một Invoice UNPAID. Không ghi danh thay hoặc tạo Invoice tại payment.
- Booking/Attempt: tối đa hai lượt logic, vắng không là zero, đặt lại cùng lượt, source CLOSED chặn Student đổi/hủy Booking riêng nhưng Registration cancellation có guard phụ thuộc riêng.
- Class/lịch: creation cần Program ACTIVE/Teacher ACTIVE, first-Class freeze; placement cần Student ACTIVE/ADMITTED/PAID và Teacher hợp lệ. Weekly all-or-none; move/replacement giữ guard occurrence/data; HELD tại `now >= end`, Class ONGOING/current PLANNED.
- Attendance: một LESSON/tập supplied items, missing khác Vắng; kiểm cả unchanged supplied RV trước delta; không thêm AuditLog Attendance history vào sơ đồ.
- Grade: lưu một Student; tổng nháp tính khi đọc; submit/return/finalize toàn cohort; final_score chỉ tại FINALIZED, rounded half-up một lần và so ≥50. Backend submit predicate kiểm mỗi report DRAFT hoặc RETURNED; không tự thêm invariant khác.
- Profile: hai RV Account/Profile, audit changedFields; không revoke-all, replay completion hoặc backend reject-all-equal. Ngoại lệ này được ghi rõ ở SQ06 và README.
- Finance/report: mã phương thức CASH/TRANSFER; correction giữ amount/time/link. CSV fresh whole-filtered set, sáu cột, footer tính trong 10.000 data records và 5.000.000 encoded bytes. Director không export hoặc truy vấn Student detail.
- Phiên ngày 14/10: focus đơn thuần không check; actual hidden→visible check lại, hidden clears RAM; các trigger khác giữ nguyên.
- Mỗi file có ba ví dụ giả định để kiểm hình, không trình bày chúng như kết quả test đã chạy.

## Kiểm tra cơ học

- Đủ một file cho từng SQ01–SQ25, không thiếu/đánh lại số.
- Mỗi file có mục tiêu, lifeline, guard, thông điệp, phác thảo text, note và checklist; code fence cân bằng.
- 916 hàng thông điệp, 58 phác thảo text trong bộ; số này mô tả tài liệu, không phải số endpoint hay test.
- 261 liên kết tương đối trong bộ trỏ đến file tồn tại trong workspace; UTF-8 hợp lệ.
- ZIP có đúng 26 Markdown: 25 hướng dẫn + README, CRC và SHA256 nội dung khớp các file bàn giao.
- Word nguồn giữ SHA256 `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`; các hướng dẫn cũ được giữ nguyên đường dẫn/nội dung.
- Bằng chứng kiểm file: `SEQUENCE_GUIDES_VERIFICATION_20261014.json` trong thư mục này.

Không chạy lại kiểm thử runtime/backend/frontend cho lượt viết tài liệu này. Không tuyên bố thực hiện backup/restore còn thiếu đầu vào vận hành M4. Liên kết tới nguồn nghiệp vụ/code dùng khi đọc trong workspace; ZIP chỉ đóng gói bộ hướng dẫn, không sao chép toàn repository.
