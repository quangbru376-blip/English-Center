# T02 — Account: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 02 — Account.

**Kết luận cấu trúc:** 16 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| account_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Account; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| role_id | int; NULL=N; FK Role | int; NULL=N; FK: FK_Account_Role_role_id | Khớp tên/kiểu/NULL |
| email | varchar(254); NULL=N; UQ_Account_Email | varchar(254); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| phone | varchar(10); NULL=Y; UQ_Account_Phone khi khác NULL | varchar(10); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| password_hash | varchar(512); NULL=N; - | varchar(512); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| full_name | nvarchar(200); NULL=N; - | nvarchar(200); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default ACTIVE | varchar(16); NULL=N; DEFAULT ('ACTIVE') | Khớp tên/kiểu/NULL |
| created_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| security_version | bigint; NULL=N; default 1 | bigint; NULL=N; DEFAULT (CONVERT([bigint],(1))) | Khớp tên/kiểu/NULL |
| failed_login_count | int; NULL=N; default 0 | int; NULL=N; DEFAULT ((0)) | Khớp tên/kiểu/NULL |
| failure_window_started_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| login_throttled_until | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| must_change_password | bit; NULL=N; server | bit; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| temp_issued_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| temp_expires_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Account | PK | account_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Account_Role_Status_Id | Không | role_id, status, account_id | Không filtered |
| UQ_Account_Email | Có | email | Không filtered |
| UQ_Account_Phone | Có | phone | ([phone] IS NOT NULL) |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Account_FailedLoginCount | ([failed_login_count]>=(0)) |
| CK_Account_Phone | ([phone] IS NULL OR datalength([phone])=(10) AND left([phone],(1))='0' AND NOT ([phone]) collate Latin1_General_100_BIN2 like '%[^0-9]%') |
| CK_Account_SecurityVersion | ([security_version]>(0)) |
| CK_Account_Status | ((([status]) collate Latin1_General_100_BIN2='DISABLED' OR ([status]) collate Latin1_General_100_BIN2='LOCKED' OR ([status]) collate Latin1_General_100_BIN2='ACTIVE') AND datalength([status])=len([status])) |
| CK_Account_TempBundle | ([must_change_password]=(1) AND [temp_issued_at] IS NOT NULL AND [temp_expires_at] IS NOT NULL AND [temp_expires_at]=dateadd(hour,(24),[temp_issued_at]) OR [must_change_password]=(0) AND [temp_issued_at] IS NULL AND [temp_expires_at] IS NULL) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không có locked_until. LoginThrottledUntil là timer chặn login mới, khác Account LOCKED; failed_login_count không tự chuyển Account LOCKED.
- Mật khẩu tạm, securityVersion và revoke-all được thực thi theo lệnh tài khoản; CHECK temp bundle không tự đổi credential hoặc thu hồi SID.
- Phone nullable ở SQL cho staff; Student signup/profile yêu cầu phone tại validation/service.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AccountConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
