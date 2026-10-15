# T04 — AuthSession: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 04 — AuthSession.

**Kết luận cấu trúc:** 9 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| sid | binary(32); NULL=N; PK | binary(32); NULL=N; PK: PK_AuthSession | Khớp tên/kiểu/NULL |
| account_id | int; NULL=N; FK Account | int; NULL=N; FK: FK_AuthSession_Account_account_id | Khớp tên/kiểu/NULL |
| issued_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| expires_at | datetime2(3); NULL=N; server | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| last_activity_at | datetime2(3); NULL=N; initial issued_at | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| revoked_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| security_version | bigint; NULL=N; snapshot Account | bigint; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| session_scope | varchar(24); NULL=N; server | varchar(24); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_AuthSession | PK | sid |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_AuthSession_Account_Revoked | Không | account_id, revoked_at | Không filtered |
| IX_AuthSession_Expires | Không | expires_at | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_AuthSession_Lifetime | ([issued_at]<=[last_activity_at] AND [last_activity_at]<=[expires_at] AND [expires_at]>[issued_at]) |
| CK_AuthSession_RevokedAt | ([revoked_at] IS NULL OR [revoked_at]>=[issued_at]) |
| CK_AuthSession_Scope | ((([session_scope]) collate Latin1_General_100_BIN2='PASSWORD_CHANGE' OR ([session_scope]) collate Latin1_General_100_BIN2='NORMAL') AND datalength([session_scope])=len([session_scope])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- sid là binary(32), không PK int/IDENTITY. Phiên NORMAL tối đa 2h/idle30m; PASSWORD_CHANGE tối đa10m không vượt hạn secret tạm.
- Me/CSRF/background focus không renew idle. Revoked/absolute expiry/securityVersion/current status được kiểm server; CK temporal không tự chứng minh session còn quyền.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AuthSessionConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
