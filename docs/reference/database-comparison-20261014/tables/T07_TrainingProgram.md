# T07 — TrainingProgram: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 07 — TrainingProgram.

**Kết luận cấu trúc:** 7 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| program_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_TrainingProgram; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| course_id | int; NULL=N; FK Course | int; NULL=N; FK: FK_TrainingProgram_Course_course_id | Khớp tên/kiểu/NULL |
| band_id | int; NULL=N; FK Band | int; NULL=N; FK: FK_TrainingProgram_Band_band_id | Khớp tên/kiểu/NULL |
| program_name | nvarchar(300); NULL=N; - | nvarchar(300); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default DRAFT | varchar(16); NULL=N; DEFAULT ('DRAFT') | Khớp tên/kiểu/NULL |
| expected_lesson_count | int; NULL=N; - | int; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_TrainingProgram | PK | program_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Program_Band | Không | band_id | Không filtered |
| IX_Program_Status_Id | Không | status, program_id | Không filtered |
| UQ_Program_CourseBand | Có | course_id, band_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_TrainingProgram_ExpectedLessons | ([expected_lesson_count]>(0)) |
| CK_TrainingProgram_Status | ((([status]) collate Latin1_General_100_BIN2='INACTIVE' OR ([status]) collate Latin1_General_100_BIN2='ACTIVE' OR ([status]) collate Latin1_General_100_BIN2='DRAFT') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không approved_by/approved_at hoặc Director approval. Activation cần cấu hình đã lưu tổng weight 100, đúng1 FINAL_EXAM và LESSON count dương.
- First Class tham chiếu khóa cấu hình, kể cả lớp CANCELLED; đổi tên Program là action riêng không giải freeze.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/TrainingProgramConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
