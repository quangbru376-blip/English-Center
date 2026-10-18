# English Center Management System

Ứng dụng quản lý trung tâm tiếng Anh cho bảy vai trò: Học viên, Tuyển sinh,
Giáo vụ, Giáo viên, Kế toán, Giám đốc và Quản trị viên.

## Cấu trúc mã nguồn

- `backend/`: ASP.NET Core, EF Core, SQL Server, migration và các bộ kiểm thử.
- `frontend/`: React, TypeScript, Vite, tài nguyên giao diện và kiểm thử.
- `tools/`: công cụ khởi tạo môi trường và vận hành trên Windows.
- `docs/HUONG_DAN_CHAY_SAN_PHAM.md`: hướng dẫn chạy trên máy đã cấu hình.
- `docs/plan/M0_LOCAL_DEV_RUNBOOK.md`: hướng dẫn cấu hình và vận hành LOCAL_DEV.
- `docs/plan/M0_LOCAL_DEV_PROFILE_TEMPLATE.json`: mẫu cấu hình môi trường.
- `docs/plan/M0_PATCH_REVIEW.md`: cấu hình phiên bản và dấu kiểm tra file mà
  công cụ `Install-M0Profiles.ps1` đang đọc trực tiếp.

## Chuẩn bị và chạy

Các phiên bản .NET SDK và thư viện được xác định trong `backend/global.json`,
các file dự án và lockfile. Frontend dùng các phiên bản trong
`frontend/package-lock.json`.

Trên máy phát triển, khôi phục dependency rồi build:

```powershell
cd backend
dotnet restore EnglishCenter.sln --locked-mode
dotnet build EnglishCenter.sln
cd ../frontend
npm ci
npm run build
```

Build thành công chưa đồng nghĩa với môi trường nghiệp vụ đã được cấu hình.
Ứng dụng cần SQL Server, migration, tài khoản Windows/SQL chuyên biệt,
chứng chỉ HTTPS, khóa và profile theo runbook. Chạy entrypoint mặc định bằng
`dotnet run` không thay thế quy trình này.

Trên máy Windows đã được cấu hình LOCAL_DEV đúng đường dẫn và danh tính,
khởi chạy bằng PowerShell 7:

```powershell
pwsh -NoProfile -File tools/operator/Start-M0Local.ps1
```

Mở `https://localhost:8443/login` sau khi launcher báo sẵn sàng. Giữ cửa sổ
launcher mở; nhấn Enter tại cửa sổ đó để dừng các tiến trình do nó khởi tạo.
Chi tiết và cách xử lý lỗi có trong hướng dẫn chạy và runbook phía trên.

Repository không chứa database, mật khẩu, khóa riêng hoặc cấu hình bí mật
của máy đang vận hành. Không sao chép các dữ liệu này vào Git.

## Kiểm thử

```powershell
cd backend
dotnet test EnglishCenter.sln
cd ../frontend
npm run lint
npm run typecheck
```

Các kiểm thử tích hợp SQL, danh tính Windows và trình duyệt có điều kiện môi
trường riêng; xem mã kiểm thử và công cụ tương ứng trước khi chạy. Báo cáo,
ảnh chụp báo cáo, project memory và hướng dẫn agent được giữ ngoài bản hiện
tại của repository. Tài nguyên giao diện và thông báo giấy phép vẫn đi kèm mã nguồn.
