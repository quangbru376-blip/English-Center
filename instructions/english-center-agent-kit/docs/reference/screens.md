# Thống kê form và màn hình giao diện — Trung tâm Anh ngữ

> Phạm vi đã chốt đến 21/09/2026. “Form/màn hình” là một đơn vị cần thiết kế wireframe; danh sách và chi tiết được gộp khi có thể dùng cùng một trang. Các vai trò dùng chung một màn hình chỉ được tính một lần.

## Tổng hợp

| Nhóm | Số màn hình/form | Nhập liệu hoặc xác nhận | Tra cứu/hiển thị |
|---|---:|---:|---:|
| Dùng chung | 1 | 1 | 0 |
| Học viên | 11 | 4 | 7 |
| Tuyển sinh | 3 | 2 | 1 |
| Giáo vụ | 8 | 6 | 2 |
| Giáo viên | 4 | 3 | 1 |
| Kế toán | 3 | 1 | 2 |
| Ban Giám đốc | 1 | 0 | 1 |
| Quản trị viên | 2 | 1 | 1 |
| **Tổng** | **33** | **18** | **15** |

## Danh sách chi tiết

| Mã | Vai trò chính | Form/màn hình | Loại | Nội dung và thao tác chính |
|---|---|---|---|---|
| F01 | Dùng chung | Đăng nhập | Nhập liệu | Email/số điện thoại, mật khẩu; chuyển vào giao diện theo vai trò. |
| F02 | Học viên | Đăng ký tài khoản | Nhập liệu | Tạo tài khoản học viên, kiểm tra thông tin trùng lặp. |
| F03 | Học viên | Trang chủ học viên | Tra cứu | Trạng thái từng hồ sơ đăng ký và bước cần làm tiếp theo. |
| F04 | Học viên | Danh mục và chi tiết khóa học | Tra cứu | Khóa đang mở, học phí và thông tin chương trình. |
| F05 | Học viên | Đăng ký khóa học | Nhập liệu | Tạo hồ sơ `Registration` cho khóa học được chọn. |
| F06 | Học viên | Đăng ký thi đầu vào/thi lại | Nhập liệu | Chọn ca thi còn chỗ; tối đa một lần thi lại. |
| F07 | Học viên | Lịch và chi tiết ca thi | Tra cứu | Ngày, giờ, phòng, số lượt thi đã đăng ký. |
| F08 | Học viên | Kết quả thi và Band | Tra cứu | Điểm từng lượt, điểm cao nhất và Band theo ngưỡng chung. |
| F09 | Học viên | Xác nhận nhập học | Xác nhận | Xem điều kiện, xác nhận trước khi tuyển sinh duyệt hồ sơ. |
| F10 | Học viên | Hóa đơn và hướng dẫn thanh toán | Tra cứu | Số tiền phải thu, hạn đóng, trạng thái; phương thức thanh toán còn cần chốt. |
| F11 | Học viên | Lớp và thời khóa biểu cá nhân | Tra cứu | Lớp đã xếp, giáo viên, phòng và lịch học. |
| F12 | Học viên | Bảng điểm cá nhân | Tra cứu | Chỉ hiển thị kết quả đã được giáo vụ chốt. |
| F13 | Tuyển sinh | Danh sách và tìm kiếm hồ sơ | Tra cứu | Tìm theo tên, email, số điện thoại; lọc theo trạng thái. |
| F14 | Tuyển sinh | Chi tiết và duyệt hồ sơ | Xác nhận | Kiểm tra thông tin, Band, xác nhận của học viên; duyệt để tạo hóa đơn. |
| F15 | Tuyển sinh | Hỗ trợ ghi danh trực tiếp | Nhập liệu | Tạo hồ sơ thay học viên; dùng cùng trường dữ liệu với F05. Hỗ trợ đăng ký thi dùng lại F06. |
| F16 | Giáo vụ | Danh sách ca thi đầu vào | Tra cứu | Theo dõi lịch, phòng, sức chứa và tiến độ nhập điểm. |
| F17 | Giáo vụ | Tạo/quản lý ca thi và phân công chấm | Nhập liệu | Tạo ca/phòng tối đa 20 người, xếp học viên, phân công giáo viên. |
| F18 | Giáo vụ | Khóa học, chương trình theo Band và trọng số | Nhập liệu | Quản lý `Course`, `TrainingProgram`, `ScoreComponentConfig`. |
| F19 | Giáo vụ | Danh sách và chi tiết lớp | Tra cứu | Theo dõi lớp, Band, sĩ số, giáo viên và trạng thái. |
| F20 | Giáo vụ | Tạo/cập nhật lớp và phân công giáo viên | Nhập liệu | Lớp tối đa 15 học viên; giáo viên không trùng lịch. |
| F21 | Giáo vụ | Biên tập lịch học/lịch thi | Nhập liệu | Tạo và điều chỉnh `Schedule`, kiểm tra trùng phòng/giáo viên. |
| F22 | Giáo vụ | Xếp học viên vào lớp | Nhập liệu | Chọn học viên đã đóng học phí, đúng khóa/Band và lớp còn chỗ. |
| F23 | Giáo vụ | Hàng chờ và chi tiết chốt bảng điểm | Xác nhận | Xem điểm đã gửi, trả lại để sửa hoặc chốt và công bố ngay. |
| F24 | Giáo viên | Lớp của tôi và lịch giảng dạy | Tra cứu | Chỉ hiện lớp/ca thi được phân công. |
| F25 | Giáo viên | Nhập điểm thi đầu vào | Nhập liệu | Ghi điểm thi trực tiếp; hệ thống tự xác định Band. |
| F26 | Giáo viên | Điểm danh theo buổi | Nhập liệu | Ghi có mặt/vắng cho từng học viên của lớp. |
| F27 | Giáo viên | Sổ điểm và gửi bảng điểm | Nhập liệu | Nhập điểm thành phần, xem điểm tổng kết, gửi hoặc sửa và gửi lại khi bị trả. |
| F28 | Kế toán | Danh sách và chi tiết hóa đơn | Tra cứu | Tìm hóa đơn chưa/đã thanh toán, xác định quá hạn từ ngày đến hạn. |
| F29 | Kế toán | Đối chiếu và xác nhận thanh toán | Xác nhận | Ghi một lần thanh toán đầy đủ cho hóa đơn sau khi đối chiếu thực tế. |
| F30 | Kế toán | Báo cáo tài chính | Tra cứu | Doanh thu, đã thu, chưa thu; xuất báo cáo. |
| F31 | Ban Giám đốc | Tổng quan báo cáo | Tra cứu | Ba nhóm tuyển sinh, đào tạo và tài chính; quyền chỉ xem. |
| F32 | Quản trị viên | Quản lý tài khoản nhân sự | Nhập liệu | Tạo tài khoản, gán vai trò cố định, khóa/mở tài khoản. |
| F33 | Quản trị viên | Nhật ký hoạt động | Tra cứu | Tìm sự kiện theo người dùng, thao tác và thời gian. |

## Ghi chú phạm vi

- Các bảng học bổng và phê duyệt chương trình/lớp/học bổng chưa tạo form giao diện trong giai đoạn hiện tại.
- Học viên thi trực tiếp; không có form làm bài thi trực tuyến. Giáo viên nhập điểm và hệ thống xếp Band theo điểm cao nhất của tối đa hai lượt.
- Các biểu mẫu xuất như hóa đơn, phiếu thu, bảng điểm và báo cáo là **đầu ra của những màn hình trên**, chưa tính thành màn hình riêng.
- F15 tái sử dụng trường của F05; hỗ trợ đăng ký thi tái sử dụng F06. Cách này tránh đếm trùng cùng một form cho hai vai trò.
- Số màn hình có thể đổi khi chốt phương thức thanh toán, trường dữ liệu hồ sơ và trạng thái bảng điểm trả lại.
