# T08 — ScoreComponentConfig: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 08 — ScoreComponentConfig.

**Kết luận cấu trúc:** 6 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| component_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_ScoreComponentConfig; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| program_id | int; NULL=N; FK TrainingProgram | int; NULL=N; FK: FK_ScoreComponentConfig_TrainingProgram_program_id | Khớp tên/kiểu/NULL |
| component_name | nvarchar(200); NULL=N; UQ trong program, CI_AS_SC | nvarchar(200); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| weight | decimal(5,2); NULL=N; - | decimal(5,2); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| kind | varchar(16); NULL=N; - | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_ScoreComponentConfig | PK | component_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| UQ_Component_FinalExam | Có | program_id | ([kind]='FINAL_EXAM') |
| UQ_Component_ProgramName | Có | program_id, component_name | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_ScoreComponentConfig_Kind | ((([kind]) collate Latin1_General_100_BIN2='FINAL_EXAM' OR ([kind]) collate Latin1_General_100_BIN2='LESSON' OR ([kind]) collate Latin1_General_100_BIN2='GENERAL') AND datalength([kind])=len([kind])) |
| CK_ScoreComponentConfig_Name | (len([component_name])>(0)) |
| CK_ScoreComponentConfig_Weight | ([weight]>(0) AND [weight]<=(100)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Kind GENERAL/LESSON/FINAL_EXAM. Weight >0 đến100, input≤2 số lẻ; component_name unique trong Program theo collation.
- Filtered unique FINAL_EXAM bảo vệ tối đa1, không bảo đảm có đúng1 khi ACTIVE; activation/service kiểm tổng và sự tồn tại.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/ScoreComponentConfigConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
