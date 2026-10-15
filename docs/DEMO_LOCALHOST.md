# Bản demo để tự trải nghiệm

Ngày dựng dữ liệu: 15/10/2026, giờ Việt Nam. Website: **https://localhost:8444**. Database riêng: `EnglishCenter_DEMO_LOCAL`. Chỉ lắng nghe `127.0.0.1`, chưa có đường dẫn Internet.

## Đăng nhập

Mở danh sách email/mật khẩu tại `C:\Users\asus\AppData\Local\EnglishCenterDemo\tai-khoan-demo.html`. Đây là tài liệu bàn giao riêng trên máy, không được website phục vụ. Sổ gốc được mã hóa DPAPI ở `C:\ProgramData\EnglishCenter\DemoOperator\accounts.dpapi`.

Có đủ 67 tài khoản: 50 Student, 8 Teacher, 2 Admissions, 2 AcademicAffairs, 2 Accountant, 1 Director, 2 Admin. Email theo mẫu `student01@demo.example`, `teacher01@demo.example`, `academicaffairs01@demo.example`… Từng tài khoản có mật khẩu riêng. Cả 17 nhân viên đã thực hiện đổi mật khẩu tạm qua web; mật khẩu hiện tại nằm trong danh sách bàn giao. Student49 bị khóa, Student50 bị vô hiệu hóa có chủ đích.

## Những tình huống nên thử

| Tài khoản | Màn hình / dữ liệu | Bước có thể trải nghiệm |
|---|---|---|
| Student01 | Trang chủ chưa có đăng ký | Chọn khóa ACTIVE, tạo đăng ký, đặt ca tương lai |
| Student02 | Lịch sử kết quả đầu vào | Điểm0, lượt hai35, vắng rồi đặt lại; BandB2 theo điểm tốt nhất |
| Student03 | Đang kiểm tra đầu vào | Chờ Teacher02 chấm ca TEST02 đã kết thúc |
| Student04 | Hồ sơ đã hủy | Xem lịch sử và tạo hồ sơ mới theo điều kiện server |
| Student05–08 | Ca TEST07 tương lai | Ca đã đầy; xem lịch hiện tại hoặc đổi sang ca khác còn chỗ |
| Student09–12 | Đã có Band | Xác nhận đăng ký |
| Student13–15 | CONFIRMED | Admissions01 tìm và duyệt/từ chối |
| Student16 | REJECTED | Xem lý do từ chối |
| Student17–20 | UNPAID quá hạn | Accountant01 tìm hóa đơn và xác nhận thanh toán |
| Student21–23 | UNPAID còn hạn | Trải nghiệm thanh toán; Student24 có hóa đơn/hồ sơ đã hủy |
| Student07, đăng ký phụ | ADMITTED, PAID, BandB2, khóa DEMO01 | AcademicAffairs01 tìm ứng viên, xếp vào lớp OPEN DEMO-07 |
| Student29–32 / Teacher08 | Lớp ONGOING DEMO-08 | Có một buổi HELD, điểm danh và điểm GENERAL nháp; Teacher08 tiếp tục thao tác |
| Student33–36 / Teacher01 | Lớp COMPLETED DEMO-09 | Bảng DRAFT đủ điểm, gửi duyệt |
| Student37–40 / Teacher02 | Lớp COMPLETED DEMO-10 | Bảng RETURNED, sửa và gửi lại |
| Student41–44 / AcademicAffairs01 | Lớp COMPLETED DEMO-11 | Toàn bộ SUBMITTED, trả lại hoặc chốt |
| Student45–48 | Kết quả lớp DEMO-12 | Kết quả FINALIZED; Student45 đạt, Student48 không đạt |
| AcademicAffairs01 | Danh mục / lớp / ca | 6 khóa, 12 chương trình, 12 lớp, 12 ca; có khóa INACTIVE, lớp/ca CANCELLED |
| Accountant01 / Director01 | Báo cáo | Doanh thu, công nợ, tuyển sinh/đào tạo và CSV theo quyền |
| Admin01 | Tài khoản / nhật ký | Quản lý 67 tài khoản; audit đăng nhập/đổi mật khẩu là sự kiện thật của demo |

Tên nhân sự, địa chỉ, giao dịch và lịch sử học tập đều giả lập. Lịch sử được dựng bằng công cụ operator riêng, không tạo audit giả như người thật đã thao tác. Hệ thống tiếp tục ghi audit thật khi bạn dùng web. Các ngày tương lai được tính theo ngày tạo; khi demo để lâu cần lập đợt dữ liệu mới, không đổi đồng hồ hệ thống.

## Bật, tắt và nhận lại tài khoản

Tại thư mục `C:\Study\SOEN\main_workplace`, chạy bằng PowerShell hiện có:

```powershell
# Chạy và giữ cửa sổ này mở; chỉ dùng khi bản demo đang tắt.
& .\tools\demo\Start-LocalDemo.ps1

# Tắt êm từ một cửa sổ PowerShell khác.
& .\tools\demo\Start-LocalDemo.ps1 -Stop

# Tạo lại trang bàn giao từ sổ mã hóa, không đổi mật khẩu.
& .\tools\demo\Show-DemoAccounts.ps1
```

Bản đang bàn giao được chạy nền; có thể dùng lệnh `-Stop` trên. Khởi động lại giữ dữ liệu thực hành. Công cụ seed từ chối nếu đã có dữ liệu; không có lệnh reset/xóa tự động. Nếu bạn tự đổi mật khẩu, sổ bàn giao không tự cập nhật; dùng mật khẩu bạn vừa chọn.

Môi trường LOCAL_DEV ở cổng8443 và D07 được giữ riêng. Các cổng localhost dùng chung hostname/cookie trình duyệt; nên dùng một profile trình duyệt riêng cho demo nếu mở đồng thời hai môi trường. Khóa JWT/CSRF, issuer và database của demo được tách riêng.

Chi tiết kiểm chứng và giới hạn: `docs/plan/LOCAL_DEMO_REVIEW.md`. Backup/restore M4 và kế hoạch mở Internet vẫn là công việc riêng.
