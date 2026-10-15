# T06 — Band: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 06 — Band.

**Kết luận cấu trúc:** 4 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| band_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Band; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| band_code | varchar(30); NULL=N; UQ_Band_Code | varchar(30); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| min_score | decimal(5,2); NULL=N; UQ_Band_MinScore | decimal(5,2); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Band | PK | band_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| UQ_Band_Code | Có | band_code | Không filtered |
| UQ_Band_MinScore | Có | min_score | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Band_Code | (datalength([band_code])>=(1) AND datalength([band_code])<=(30) AND NOT ([band_code]) collate Latin1_General_100_BIN2 like '%[^ -~]%' AND len([band_code])>(0)) |
| CK_Band_MinScore | ([min_score]>=(0) AND [min_score]<=(100)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không max_score. Min-score lớn nhất ≤ best score; seed 0/25/45/65/85.
- First completed entry result khóa threshold set qua protocol và trigger; bảng tham chiếu không có editor ngưỡng trong UI hiện tại.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/BandConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
