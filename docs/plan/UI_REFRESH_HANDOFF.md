# Bàn giao giao diện — Trung tâm tiếng Anh

Bản tân trang dùng teal/vàng, Be Vietnam Pro cục bộ và icon lucide. Học viên có
menu trên/drawer mobile; nhân sự có sidebar; menu tài khoản chứa hồ sơ/đổi mật
khẩu/đăng xuất. Các màn tự đọc dữ liệu ban đầu; bộ lọc chỉ áp dụng khi bấm nút.
Không tự lưu, gửi lại lệnh ghi hoặc tính lại tổng báo cáo từ trang đang xem.

## Xem kết quả

Mở UI-refresh-evidence/stage6/before-after.html để xem30cặp trước/sau, hoặc
stage6/index.html để lọc593ảnh theo vai trò/kích thước/trạng thái. Các ảnh dùng
SQL scratch/dữ liệu giả, mật khẩu đã che; không phải dữ liệu vận hành.
Preview http://127.0.0.1:5178/tests/ui-refresh-styleguide.html và
/tests/ui-refresh-student.html chỉ minh họa giao diện; không thực hiện nghiệp vụ.

## Chạy sản phẩm LOCAL_DEV

Từ PowerShell7 ở C:\Study\SOEN\main_workplace:

```powershell
.\tools\operator\Start-M0Local.ps1
```

Mở https://localhost:8443, dùng tài khoản đã được cấp theo quy trình hiện có.
Không tạo tài khoản giả/khóa thật từ dữ liệu kiểm thử. Enter ở terminal dừng cả
hai host. Chạy launcher có VerifyBrowser chỉ theo nhiệm vụ nghiệm thu: nó đăng
nhập/đăng xuất Admin và kiểm maintenance/audit thật; không phải preview ảnh.
Profile từ chối khi artifact drift; helper Refresh-UIArtifactPins là thao tác
bàn giao cố định cho đúng bản UI này, không là công cụ tự chấp nhận package mới.
Không lặp helper sau khi đã có receipt; không đổi pins/custody để bỏ qua refusal.

## Khi thao tác

Rời trang có dữ liệu chưa lưu cần xác nhận. Sau mất phản hồi ghi, dùng nút
“Kiểm tra kết quả”/tra cứu thủ công; không gửi lại tự động. Hóa đơn in từ lần
đọc mới; CSV tài chính đọc toàn bộ bộ lọc đã áp dụng và giữ hợp đồng an toàn
ô; cột tiền lớn cần nhập dạng Text. Giám đốc chỉ xem tổng hợp, không xuất file.
Kết quả học viên chỉ hiện sau khi đã chốt. Mật khẩu tạm hiển thị một lần.

## Giới hạn

Nghiệm thu trong Edge cài sẵn/3757681280 và môi trường LOCAL_DEV đã chốt;
không tuyên bố mọi browser/screen reader, tải50users, LAN, backup/restore hay
triển khai production. M4 recovery còn chờ đầu vào vận hành riêng. Xem review/
summary stage6 để biết trạng thái apply và kết quả kiểm chứng thực tế.
