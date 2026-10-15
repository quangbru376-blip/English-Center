# 10 sequence ưu tiên cho báo cáo

Chọn từ bộ SQ01–SQ25 theo hành trình nghiệp vụ chính. Mã gốc được giữ; không đánh lại thành SQ01–SQ10. Một nhóm có thể tách thành nhiều hình A/B/C trong cùng hướng dẫn.

| Mã | Hướng dẫn | Vì sao ưu tiên |
| --- | --- | --- |
| SQ03 | [Đăng nhập và đăng xuất](SQ03_LOGIN_LOGOUT_GUIDE.md) | Xác thực, quyền và phiên cho cả 7 vai trò. |
| SQ07 | [Đăng ký khóa và hủy hồ sơ](SQ07_COURSE_REGISTRATION_GUIDE.md) | Bắt đầu hồ sơ học tập của Student. |
| SQ09 | [Đặt, đổi, hủy và đặt lại ca thi](SQ09_ENTRY_TEST_BOOKING_GUIDE.md) | Quản lý lượt thi và lịch hẹn đúng điều kiện. |
| SQ10 | [Chấm đầu vào và xác định Band](SQ10_ENTRY_SCORE_BAND_GUIDE.md) | Kết quả thi quyết định trình độ và luồng nhập học. |
| SQ11 | [Student xác nhận nhập học](SQ11_ENROLLMENT_CONFIRMATION_GUIDE.md) | Điểm chuyển từ kết quả đầu vào sang chờ tuyển sinh. |
| SQ12 | [Duyệt tuyển sinh và phát hành hóa đơn](SQ12_ADMISSIONS_DECISION_GUIDE.md) | ADMITTED + Invoice UNPAID cùng giao dịch. |
| SQ14 | [Xác nhận thu tiền](SQ14_PAYMENT_CONFIRMATION_GUIDE.md) | Payment + Invoice PAID, điều kiện để xếp lớp. |
| SQ17 | [Xếp học viên vào lớp](SQ17_STUDENT_CLASS_PLACEMENT_GUIDE.md) | Assignment + GradeReport DRAFT cùng giao dịch. |
| SQ21 | [Gửi và gửi lại bảng điểm cả lớp](SQ21_CLASS_GRADE_SUBMISSION_GUIDE.md) | Kiểm toàn cohort trước chuyển SUBMITTED. |
| SQ22 | [Trả lại, chốt và xem điểm công bố](SQ22_CLASS_GRADE_REVIEW_FINALIZATION_GUIDE.md) | FINALIZED nguyên tử; Student chỉ thấy điểm đã chốt. |

## Điều kiện và các luồng tham chiếu

- SQ21 cần Teacher lưu điểm nháp từng Student qua SQ20 trước khi gửi; ghi bằng ref SQ20, không bỏ điều kiện đủ điểm hoặc gộp lưu nháp và gửi cả lớp thành một giao dịch.
- SQ17 cần Course/Program/Class/lịch hợp lệ đã chuẩn bị qua SQ15/SQ16/SQ18; đây là tiền điều kiện, không tự mở lớp hoặc lập lịch trong thao tác xếp lớp.
- SQ22 tách Giáo vụ trả lại/chốt và Student đọc kết quả ở các hình riêng; không vẽ một cuộc gọi đồng bộ giữa hai actor.
- SQ01 tự tạo tài khoản vẫn thuộc hệ thống; bộ ưu tiên bắt đầu từ tài khoản đã có hoặc login.
- Chọn 10 nhóm để ưu tiên trình bày, không có nghĩa các sequence khác bị loại khỏi phạm vi sản phẩm.

Bản ZIP chỉ có 10 hướng dẫn và mục lục này. Các liên kết tới hướng dẫn khác, đặc tả hoặc code dùng khi đọc trong workspace/bộ đầy đủ; không đóng gói toàn dự án.
