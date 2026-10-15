# T14 — Class: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 14 — Class.

**Kết luận cấu trúc:** 12 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| class_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Class; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| program_id | int; NULL=N; FK TrainingProgram | int; NULL=N; FK: FK_Class_TrainingProgram_program_id | Khớp tên/kiểu/NULL |
| teacher_id | int; NULL=N; FK Account | int; NULL=N; FK: FK_Class_Account_teacher_id | Khớp tên/kiểu/NULL |
| class_code | varchar(30); NULL=N; UQ_Class_Code | varchar(30); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default PLANNED | varchar(16); NULL=N; DEFAULT ('PLANNED') | Khớp tên/kiểu/NULL |
| max_size | int; NULL=N; - | int; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| created_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| cancelled_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Class_Account_cancelled_by | Khớp tên/kiểu/NULL |
| cancelled_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Class | PK | class_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Class_CancelledBy | Không | cancelled_by | Không filtered |
| IX_Class_ProgramState | Không | program_id, status, class_id | Không filtered |
| IX_Class_TeacherState | Không | teacher_id, status, class_id | Không filtered |
| UQ_Class_Code | Có | class_code | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Class_CancellationBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND len([cancellation_reason])>(0) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| CK_Class_Code | (datalength([class_code])>=(1) AND datalength([class_code])<=(30) AND datalength([class_code])=len([class_code]) AND NOT ([class_code]) collate Latin1_General_100_BIN2 like '%[^A-Z0-9_-]%') |
| CK_Class_MaxSize | ([max_size]>=(1) AND [max_size]<=(15)) |
| CK_Class_Status | ((([status]) collate Latin1_General_100_BIN2='CANCELLED' OR ([status]) collate Latin1_General_100_BIN2='COMPLETED' OR ([status]) collate Latin1_General_100_BIN2='ONGOING' OR ([status]) collate Latin1_General_100_BIN2='OPEN' OR ([status]) collate Latin1_General_100_BIN2='PLANNED') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED; không approved_by/approved_at. OPEN nhận placement, ONGOING không nhận mới.
- COMPLETE kiểm đủ current obligations HELD và required Attendance; chưa chốt điểm. Hủy chỉ lớp PLANNED/OPEN trống/chưa teaching.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/ClassConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
