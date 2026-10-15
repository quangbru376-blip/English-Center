# T22 — AuditLog: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 22 — AuditLog.

**Kết luận cấu trúc:** 13 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| audit_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_AuditLog; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| actor_account_id | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_AuditLog_Account_actor_account_id | Khớp tên/kiểu/NULL |
| action | varchar(80); NULL=N; - | varchar(80); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| entity_type | varchar(64); NULL=N; - | varchar(64); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| entity_id | varchar(100); NULL=Y; - | varchar(100); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| result | varchar(16); NULL=N; - | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| details | nvarchar(max); NULL=Y; CK ISJSON khi có | nvarchar(max); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| occurred_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| ip_address | varchar(45); NULL=Y; - | varchar(45); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| operation_id | uniqueidentifier; NULL=N; server | uniqueidentifier; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| audit_category | varchar(16); NULL=N; - | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| security_expires_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| ip_expires_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_AuditLog | PK | audit_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Audit_ActorTime | Không | actor_account_id, occurred_at, audit_id | Không filtered |
| IX_Audit_EntityTime | Không | entity_type, entity_id, occurred_at, audit_id | Không filtered |
| IX_Audit_Operation | Không | operation_id, audit_id | Không filtered |
| IX_Audit_Time | Không | occurred_at, audit_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_AuditLog_Category | ((([audit_category]) collate Latin1_General_100_BIN2='SECURITY' OR ([audit_category]) collate Latin1_General_100_BIN2='MANDATORY') AND datalength([audit_category])=len([audit_category])) |
| CK_AuditLog_Details | ([details] IS NULL OR isjson([details])=(1)) |
| CK_AuditLog_IPBundle | ([ip_address] IS NULL AND [ip_expires_at] IS NULL OR [ip_address] IS NOT NULL AND [ip_expires_at] IS NOT NULL AND [ip_expires_at]=dateadd(day,(90),[occurred_at])) |
| CK_AuditLog_Result | ((([result]) collate Latin1_General_100_BIN2='FAILURE' OR ([result]) collate Latin1_General_100_BIN2='SUCCESS') AND datalength([result])=len([result])) |
| CK_AuditLog_SecurityExpiry | ([audit_category]='SECURITY' AND [security_expires_at] IS NOT NULL AND [security_expires_at]>=dateadd(day,(90),[occurred_at]) OR [audit_category]='MANDATORY' AND [security_expires_at] IS NULL) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Actor NULL khi chưa xác thực; không biến Hệ thống thành account role. Polymorphic entity_type/entity_id và operation_id không FK sang mọi business entity.
- MANDATORY lưu cùng mutation; SECURITY best-effort/retention riêng. Admin viewer không edit/purge/export; vận hành có cleanup security/IP theo policy.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AuditLogConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
