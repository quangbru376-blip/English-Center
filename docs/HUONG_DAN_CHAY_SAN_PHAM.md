# Hướng dẫn chạy sản phẩm English Center từ đầu

Cập nhật: 13/10/2026. Mã nguồn chính: `C:\Study\SOEN\main_workplace`.

Hướng dẫn này đưa bạn từ lúc mở máy đến khi vào giao diện, trên **máy Windows đã được cấu hình LOCAL_DEV của dự án**. M0–M3 và phần báo cáo/CSV, nhật ký Admin của M4 đã được kiểm chứng. Backup/restore của M4 còn chưa nghiệm thu.

## 1. Chuẩn bị trước khi chạy

Bạn cần:

- Đăng nhập Windows bằng tài khoản operator đã cấu hình dự án. Credential được bảo vệ theo tài khoản Windows này.
- Có thư mục dự án tại `C:\Study\SOEN\main_workplace`.
- Có PowerShell 7 (`pwsh`), .NET theo bản đã pin của dự án và SQL Server Express instance `SQLEXPRESS` đã cấu hình.
- Có database vận hành `EnglishCenter_M0_LOCALDEV`, chứng chỉ HTTPS được tin cậy và các identity/key/profile đã provision trước đó.
- Có bản backend đã build và frontend `dist` đã build. Máy hiện tại đã có các thành phần này.

Không cần mở Visual Studio để chạy. Không cần cài package, chạy migration, seed lại Admin/Band hoặc mở hai server phát triển riêng.

Mở **Windows Terminal** hoặc **PowerShell**. Bạn có thể tìm “Terminal” trong Start menu. Kiểm tra PowerShell 7 bằng:

```powershell
pwsh --version
```

Nếu không tìm thấy `pwsh`, máy chưa đáp ứng bước chuẩn bị; không dùng PowerShell 5 để chạy trực tiếp helper này.

## 2. Khởi chạy sản phẩm

Chạy lệnh sau trong terminal:

```powershell
pwsh -NoProfile -File "C:\Study\SOEN\main_workplace\tools\operator\Start-M0Local.ps1"
```

Helper khởi chạy backend và maintenance bằng hai identity đã cấu hình, đọc các credential/key được bảo vệ và kiểm tra readiness. Chờ khoảng vài giây; helper cho phép tối đa 20 giây cho readiness.

Khi thành công, terminal hiển thị:

```text
M0 LOCAL_DEV: https://localhost:8443 — press Enter to stop both owned hosts.
```

**Giữ terminal này mở. Chưa nhấn Enter**, vì Enter sẽ dừng sản phẩm. Chỉ mở một phiên khởi chạy; nếu trang đã hoạt động từ phiên trước, tiếp tục dùng phiên đó.

## 3. Mở giao diện

Mở Microsoft Edge hoặc trình duyệt trên máy này, rồi truy cập:

[https://localhost:8443/login](https://localhost:8443/login)

Bạn sẽ thấy màn hình **Đăng nhập**, gồm Email, Mật khẩu và nút Đăng nhập. Trình duyệt dùng kết nối HTTPS và chứng chỉ đã cấu hình trên máy này.

Để kiểm tra backend độc lập với giao diện, mở **một terminal khác** và chạy:

```powershell
Invoke-RestMethod -Uri "https://localhost:8443/health/ready"
```

Kết quả đúng là:

```text
ready
```

Nếu nhận lỗi kết nối, lỗi chứng chỉ hoặc `503/not_ready`, xem mục xử lý lỗi bên dưới.

## 4. Đăng nhập Admin

Tài khoản Admin hiện có:

```text
admin@englishcenter.local
```

Dùng mật khẩu đã được bàn giao/hiển thị trong cửa sổ riêng trên máy. Nếu cần xem lại **mật khẩu ban đầu**, mở một terminal khác bằng đúng tài khoản Windows operator, rồi chạy:

```powershell
$taskAdminCredential = $null
try {
    $taskAdminCredential = Import-Clixml -LiteralPath "C:\ProgramData\EnglishCenter\M0\admin.credential.clixml"
    $taskAdminCredential.GetNetworkCredential().Password
    [void](Read-Host "Sau khi sao chep mat khau, nhan Enter de xoa man hinh")
    Clear-Host
}
finally {
    if ($taskAdminCredential) { $taskAdminCredential.Password.Dispose() }
    $taskAdminCredential = $null
}
```

Mật khẩu chỉ hiện trong terminal trên máy. Không gửi mật khẩu vào chat, chụp ảnh chia sẻ hoặc chép vào tài liệu/repository.

Nếu bạn đã đổi mật khẩu Admin trong ứng dụng, dùng **mật khẩu mới**: file escrow ban đầu không tự cập nhật. Không chạy lại bootstrap để đổi lại mật khẩu.

Quay lại trình duyệt, nhập email/mật khẩu và bấm **Đăng nhập**.

## 5. Xem giao diện chính

Sau khi đăng nhập Admin, bạn có thể:

1. Chọn **Tài khoản** để tìm và xem tài khoản, tạo nhân sự thật hoặc xử lý trạng thái tài khoản theo quyền Admin.
2. Chọn **Nhật ký kiểm toán**, rồi bấm **Đọc lịch sử kiểm toán** để xem các sự kiện.
3. Bấm **Xem sự kiện** để xem chi tiết được phép; **Xem nhóm thao tác** để xem các sự kiện cùng một operation.
4. Dùng **Đổi mật khẩu** hoặc **Đăng xuất** khi cần.

Admin không có quyền xem tất cả màn hình nghiệp vụ. Giao diện phụ thuộc vào vai trò đăng nhập:

| Vai trò | Các phần chính có thể xem/làm |
| --- | --- |
| Admin | Tài khoản nhân sự, trạng thái tài khoản, nhật ký kiểm toán. |
| AcademicAffairs — Giáo vụ | Khóa học/Band, chương trình, ca thi đầu vào, lớp/lịch học, duyệt bảng điểm. |
| Admissions — Tuyển sinh | Hồ sơ tuyển sinh, quyết định nhận/từ chối/hủy theo điều kiện nghiệp vụ. |
| Teacher — Giáo viên | Ca thi được giao, chấm thi đầu vào, lớp/lịch dạy, điểm danh, sổ điểm. |
| Accountant — Kế toán | Hóa đơn/thanh toán, báo cáo tài chính và CSV. |
| Director — Giám đốc | Báo cáo tổng hợp tài chính, tuyển sinh, đào tạo. |
| Student — Học viên | Hồ sơ/đăng ký/lịch thi, hóa đơn của mình, lớp/lịch học, kết quả đã công bố. |

Để dùng giao diện của nhân sự khác, Admin chọn **Thêm nhân sự**, nhập thông tin người nhận thật và chọn vai trò phù hợp. Bàn giao mật khẩu tạm trực tiếp; người nhận đăng nhập, đổi mật khẩu tạm, rồi đăng nhập lại. Không tạo dữ liệu nhân sự giả vào database vận hành để xem thử màn hình.

Học viên có thể chọn **Đăng ký tài khoản học viên** từ màn hình đăng nhập. Đăng ký thành công không tự đăng nhập; quay lại màn hình Đăng nhập để vào tài khoản.

Database vận hành không được cài sẵn bộ dữ liệu demo khóa học/lớp/học viên. Danh sách/báo cáo trống là bình thường khi chưa phát sinh dữ liệu. Muốn thao tác nghiệp vụ, cần tạo các điều kiện trước đó theo đúng quy trình của từng vai trò.

## 6. Xem báo cáo và tải CSV

Với Director:

1. Đăng nhập tài khoản Director.
2. Chọn **Tài chính**, **Tuyển sinh** hoặc **Đào tạo**.
3. Chọn bộ lọc và bấm **Áp dụng bộ lọc**.
4. Xem thời điểm tra cứu và bộ lọc đã áp dụng cùng kết quả.

Với Accountant:

1. Đăng nhập tài khoản Accountant.
2. Chọn **Báo cáo tài chính**.
3. Áp dụng bộ lọc trước, rồi bấm **Tải CSV tài chính**.

CSV truy vấn lại toàn bộ tập lọc đã áp dụng, không chỉ trang đang xem. Sửa bộ lọc trên màn hình chưa làm đổi kết quả hoặc bộ lọc CSV cho đến khi bấm Áp dụng. Công nợ là hiện tại, không bị giới hạn theo kỳ doanh thu/phải thu. Khoảng ngày của báo cáo đào tạo chỉ áp dụng kết quả đã công bố, không lọc tồn kho lớp hiện tại.

CSV có giới hạn 10.000 bản ghi dữ liệu, tính cả dòng tổng, hoặc 5.000.000 byte toàn file. Nếu vượt giới hạn, thu hẹp bộ lọc phù hợp; ứng dụng không xuất một phần rồi báo thành công. Tiền lớn cần nhập vào bảng tính dạng Text để giữ đủ chữ số. Việc chỉnh/lưu/mở lại bằng bảng tính có thể thay đổi cách diễn giải ô.

## 7. Dừng và khởi chạy lại

1. Nếu đang đăng nhập, bấm **Đăng xuất**.
2. Quay về terminal đã chạy `Start-M0Local.ps1`.
3. Nhấn **Enter** và chờ helper dừng cả runtime lẫn maintenance.
4. Muốn chạy lại, dùng lại lệnh ở mục 2.

Dừng host không phải là xóa database. Dữ liệu đã commit vẫn được giữ lại. Khi thao tác báo chưa rõ kết quả, dùng chức năng **Kiểm tra kết quả**/đối chiếu được cung cấp; không tự gửi lại thao tác thanh toán hoặc nghiệp vụ.

## 8. Xử lý lỗi thường gặp

| Hiện tượng | Cách xử lý |
| --- | --- |
| `pwsh` không được nhận diện | Kiểm tra PowerShell 7 đã được cài/cấu hình trên máy. Đây là prerequisite của helper. |
| Không tìm thấy script | Kiểm tra đường dẫn `C:\Study\SOEN\main_workplace`; helper hiện gắn với workspace này. |
| Không mở được `localhost:8443` | Kiểm tra terminal khởi chạy còn mở và đã hiện thông báo thành công; chạy health check ở mục 3. |
| Cổng 8443 đã có listener | Kiểm tra phiên sản phẩm đang chạy; dùng phiên hiện có hoặc dừng đúng phiên đó bằng Enter trước khi chạy lại. Không kết thúc process không xác định. |
| Helper trả `local_launch_or_verification_refused` | Ghi lại `checkpoint` và receipt mà helper tạo trong `docs\plan\V01-evidence\test-results`. Đối chiếu readiness/custody/SQL; không gửi credential/private key. |
| Health trả `503/not_ready` | Có dependency chưa sẵn sàng, ví dụ SQL, maintenance, profile/key/custody hoặc validity. Dừng đúng phiên và kiểm tra cấu hình; không bỏ qua gate hoặc tự tăng SQL grants. |
| Trình duyệt báo lỗi chứng chỉ HTTPS | Kiểm tra đúng URL `https://localhost:8443` và cấu hình tin cậy hiện có. Không bật chế độ bỏ kiểm tra TLS để dùng ứng dụng. |
| `Import-Clixml` không giải mã được | Kiểm tra đúng máy và đúng tài khoản Windows operator. File DPAPI không thể mang sang máy/tài khoản khác rồi dùng trực tiếp. |
| Admin đăng nhập thất bại | Kiểm tra email chính xác, mật khẩu hiện tại và trạng thái tài khoản. Nếu đã đổi mật khẩu, escrow ban đầu không còn là mật khẩu hiện tại. Tránh thử liên tục vì có throttle. |
| Trả 403 hoặc không có menu mong muốn | Kiểm tra vai trò và trạng thái NORMAL; hoàn tất đổi mật khẩu tạm khi được yêu cầu. Không dùng Admin thay cho Director/Teacher/Accountant. |
| Danh sách/báo cáo trống | Kiểm tra bộ lọc đã áp dụng và dữ liệu/quyền hiện có. Database vận hành không có bộ dữ liệu demo nhân tạo. |

Không cần chạy `npm run dev` và `dotnet run` riêng để sử dụng bản LOCAL_DEV này. Đường vào sản phẩm là helper HTTPS ở mục 2.

## 9. Nếu cần build lại mã nguồn đã thay đổi

Chỉ cần bước này khi có thay đổi source hoặc artifact build bị thiếu. Dừng sản phẩm trước khi build. Với các dependency đã có sẵn trên máy:

```powershell
Set-Location "C:\Study\SOEN\main_workplace\backend"
dotnet build EnglishCenter.sln --no-restore
if ($LASTEXITCODE -ne 0) { throw "Backend build failed" }

Set-Location "C:\Study\SOEN\main_workplace\frontend"
npm run build
if ($LASTEXITCODE -ne 0) { throw "Frontend build failed" }
```

Sau khi cả hai build thành công, chạy lại helper ở mục 2. Helper không tự build. Backend đang pin SDK `.NET 10.0.200`; frontend khai báo Node từ 24 và npm từ 11, cùng các package đã khóa trong repository. Nếu dependency/cache/SDK bị thiếu, cần xử lý môi trường theo pin hiện có; không tự cài bản mới nhất hoặc bỏ lockfile.

## 10. Nếu chạy trên một máy Windows mới

Copy source đơn thuần chưa đủ để chạy sản phẩm. Máy mới cần setup và kiểm chứng riêng:

- Toolchain/dependency theo pin hiện có, SQL Express và schema/catalog đúng hợp đồng.
- Identity Windows/SQL riêng cho runtime/maintenance cùng quyền cần thiết.
- SQL TLS và HTTPS localhost được tin cậy đúng cách.
- JWT/CSRF/HTTPS key custody được provision riêng; không sao chép private key hoặc credential DPAPI từ máy cũ.
- Runtime profile, metadata/pins/readiness và bootstrap Admin/Band theo scope operator đã duyệt.

Tham khảo [đầu vào môi trường](plan/M0_ENVIRONMENT_INPUTS.md), [operator closure](plan/M0_OPERATOR_CLOSURE.md) và [runbook LOCAL_DEV](plan/M0_LOCAL_DEV_RUNBOOK.md). Setup máy mới là công việc riêng cần profile cụ thể, không có script “cài tất cả” đã được nghiệm thu trong hướng dẫn này.

Phần [backup/restore M4](plan/M4_03_RECOVERY_TASK_CARD.md) còn cần media/access/key reference và profile restore cô lập trước khi triển khai và nghiệm thu thật.
