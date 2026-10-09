# Chạy M0 LOCAL_DEV

M0 đã nghiệm thu2026-10-09. Code chính: C:\Study\SOEN\main_workplace.
Chỉ chạy trên laptop này tại https://localhost:8443. M1 gói1–2 đã hoàn tất;
gói3 và các milestone sau chưa được giao trong lượt này.

Mở PowerShell7 bằng tài khoản Windows operator đã provision môi trường:

```powershell
pwsh -NoProfile -File C:\Study\SOEN\main_workplace\tools\operator\Start-M0Local.ps1
```

Giữ terminal mở, truy cập https://localhost:8443. Nhấn Enter trong terminal để dừng
cả hai host. Helper sử dụng credential Windows DPAPI có sẵn, không chạy runtime
với quyền Administrator. Không cần dotnet run, cài package, migration hay seed lại.

Admin ban đầu: admin@englishcenter.local. Xem credential ban đầu chỉ trong terminal
operator trên máy này; không gửi password vào chat hoặc ghi vào source:

```powershell
$admin = Import-Clixml C:\ProgramData\EnglishCenter\M0\admin.credential.clixml
$admin.GetNetworkCredential().Password
$admin.Password.Dispose()
$admin = $null
```

Nếu chủ động đổi password Admin trong ứng dụng, dùng password mới; escrow ban đầu
không tự cập nhật và tool kiểm credential ban đầu sẽ từ chối. Không bootstrap/reset
lại để khôi phục một phản hồi chưa rõ. Role/Account/Audit bootstrap đã tạo một lần.

M0 hỗ trợ đăng ký học viên, login/me/logout, đổi password thường/hạn chế và Admin
xem/khóa/tra kết quả khóa theo ID. M1 bổ sung Admin tìm/tạo nhân sự/reset/mở khóa/
ngừng dùng/kích hoạt lại, và AcademicAffairs quản lý Course/đọc bộ Band.
Đăng ký201 không tự login; đổi password thành công phải login lại. Không có fake
staff/student/demo data trong catalog vận hành, chỉ7roles/Admin/5Band tham chiếu/
audit/session lịch sử. Dữ liệu thử được kiểm tra trong scratch DB đã xóa riêng.

Admin vào “Quản trị tài khoản”, tìm theo tên/email/điện thoại/vai trò/trạng thái rồi
“Xem” để xử lý tài khoản. “Thêm nhân sự” cấp một trong sáu vai trò nhân sự; chỉ tạo
tài khoản thật khi có người nhận. Bàn giao mật khẩu tạm trực tiếp, không gửi qua chat;
đóng hộp thoại sẽ xóa mật khẩu khỏi giao diện, tra kết quả không lấy lại bí mật.
Người nhận login, đổi mật khẩu tạm, rồi login lại. AcademicAffairs chọn “Khóa học
và Band” để quản lý khóa học và xem đủ5Band. Giá VND giữ nguyên chính xác.
Khi lệnh chưa rõ kết quả, dùng “Kiểm tra kết quả”/đối chiếu; không tự gửi lại.
Band đã khởi tạo và xác minh; không cần chạy lại initializer hoặc seed lúc startup.

GET /health/ready trả ready chỉ khi đủ9 dependency thật; mất maintenance/SQL/key/
profile/custody hoặc hết hạn thì503. Stop/restart helper dùng lại keys đã bảo vệ.
Replay chạy60s, audit3600s bằng identity riêng. Nghiệm thu quan sát tick replay thật
và audit COMMIT gọi riêng; không khẳng định đã chờ một giờ/audit90ngày thực tế.
JWT/CSRF hiện có validity90ngày; patch review tiếp theo2027-01-09. Keys/cert/profile
không tự rotate/repair; servicing cần scope operator được review riêng khi đến hạn.
Không có xác nhận tải50user, backup/restore, LAN/Internet hoặc production deployment.

Lượt nghiệm thu tự động đã dừng mọi process/listener; lệnh trên mới khởi chạy ứng dụng.
Xem M0_W5E_ACCEPTANCE_CARD và V01_READINESS_CLOSURE cho bằng chứng/phạm vi đầy đủ.
