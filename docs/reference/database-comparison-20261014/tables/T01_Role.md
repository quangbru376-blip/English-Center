# T01 — Role: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 01 — Role.

**Kết luận cấu trúc:** 3 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| role_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Role; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| role_code | varchar(32); NULL=N; UQ_Role_Code | varchar(32); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| role_name | nvarchar(200); NULL=N; - | nvarchar(200); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Role | PK | role_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| UQ_Role_Code | Có | role_code | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Role_Code | ((([role_code]) collate Latin1_General_100_BIN2='Admin' OR ([role_code]) collate Latin1_General_100_BIN2='Director' OR ([role_code]) collate Latin1_General_100_BIN2='Accountant' OR ([role_code]) collate Latin1_General_100_BIN2='Teacher' OR ([role_code]) collate Latin1_General_100_BIN2='AcademicAffairs' OR ([role_code]) collate Latin1_General_100_BIN2='Admissions' OR ([role_code]) collate Latin1_General_100_BIN2='Student') AND datalength([role_code])=len([role_code])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Bảng 6.2 dùng đúng role code, nhưng 6.3.1 viết STUDENT/ACADEMIC_AFFAIRS… sai literal. SQL CHECK chấp nhận Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin, đúng case.
- Bảy account role cố định khác bốn database role phân quyền SQL; không suy ra một bảng permission hoặc RBAC động.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/RoleConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
