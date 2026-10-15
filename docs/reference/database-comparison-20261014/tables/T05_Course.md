# T05 — Course: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 05 — Course.

**Kết luận cấu trúc:** 6 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| course_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Course; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| course_code | varchar(30); NULL=N; UQ_Course_Code | varchar(30); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| course_name | nvarchar(300); NULL=N; - | nvarchar(300); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| base_price | decimal(18,0); NULL=N; - | decimal(18,0); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; server | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Course | PK | course_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Course_Status_Id | Không | status, course_id | Không filtered |
| UQ_Course_Code | Có | course_code | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Course_Code | (datalength([course_code])>=(1) AND datalength([course_code])<=(30) AND datalength([course_code])=len([course_code]) AND NOT ([course_code]) collate Latin1_General_100_BIN2 like '%[^A-Z0-9_-]%') |
| CK_Course_Price | ([base_price]>(0)) |
| CK_Course_Status | ((([status]) collate Latin1_General_100_BIN2='INACTIVE' OR ([status]) collate Latin1_General_100_BIN2='ACTIVE') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- base_price >0, không ≥0 như 6.3.1; VND nguyên đồng. Giá mới chỉ Invoice phát hành sau, không sửa snapshot cũ.
- Course code bất biến, trạng thái ACTIVE/INACTIVE giữ lịch sử; name không unique toàn hệ thống.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/CourseConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
