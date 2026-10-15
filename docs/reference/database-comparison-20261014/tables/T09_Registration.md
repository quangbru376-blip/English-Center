# T09 — Registration: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 09 — Registration.

**Kết luận cấu trúc:** 20 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| registration_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Registration; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| student_id | int; NULL=N; FK StudentProfile | int; NULL=N; FK: FK_Registration_StudentProfile_student_id | Khớp tên/kiểu/NULL |
| course_id | int; NULL=N; FK Course | int; NULL=N; FK: FK_Registration_Course_course_id | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default PENDING, đường chuyển duyệt ADR-133 | varchar(16); NULL=N; DEFAULT ('PENDING') | Khớp tên/kiểu/NULL |
| is_unfinished | bit; NULL=N; initial 1, server-owned | bit; NULL=N; DEFAULT (CONVERT([bit],(1))) | Khớp tên/kiểu/NULL |
| created_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| admission_confirmed_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| admission_approved_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Registration_Account_admission_approved_by | Khớp tên/kiểu/NULL |
| admission_approved_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| rejected_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Registration_Account_rejected_by | Khớp tên/kiểu/NULL |
| rejected_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| rejection_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| rejected_from_status | varchar(16); NULL=Y; - | varchar(16); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| rejection_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancelled_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Registration_Account_cancelled_by | Khớp tên/kiểu/NULL |
| cancelled_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancelled_from_status | varchar(16); NULL=Y; - | varchar(16); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Registration | PK | registration_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Registration_ApprovedBy | Không | admission_approved_by | Không filtered |
| IX_Registration_CancelledBy | Không | cancelled_by | Không filtered |
| IX_Registration_CourseStateCreated | Không | course_id, status, created_at, registration_id | Không filtered |
| IX_Registration_RejectedBy | Không | rejected_by | Không filtered |
| IX_Registration_StudentCreated | Không | student_id, created_at, registration_id | Không filtered |
| UQ_Registration_Unfinished | Có | student_id, course_id | ([is_unfinished]=(1)) |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Registration_ApprovedBundle | ([admission_approved_by] IS NULL AND [admission_approved_at] IS NULL OR [admission_approved_by] IS NOT NULL AND [admission_approved_at] IS NOT NULL) |
| CK_Registration_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND (([cancelled_from_status]) collate Latin1_General_100_BIN2='ADMITTED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='CONFIRMED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='BAND_ASSIGNED' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='TESTING' OR ([cancelled_from_status]) collate Latin1_General_100_BIN2='PENDING') AND datalength([cancelled_from_status])=len([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL) |
| CK_Registration_RejectedBundle | ([status]='REJECTED' AND [rejected_by] IS NOT NULL AND [rejected_at] IS NOT NULL AND [rejection_reason] IS NOT NULL AND len([rejection_reason])>(0) AND [rejected_from_status] IS NOT NULL AND ([rejected_from_status]) collate Latin1_General_100_BIN2='CONFIRMED' AND datalength([rejected_from_status])=len([rejected_from_status]) AND [rejection_operation_id] IS NOT NULL OR [status]<>'REJECTED' AND [rejected_by] IS NULL AND [rejected_at] IS NULL AND [rejection_reason] IS NULL AND [rejected_from_status] IS NULL AND [rejection_operation_id] IS NULL) |
| CK_Registration_Status | ((([status]) collate Latin1_General_100_BIN2='CANCELLED' OR ([status]) collate Latin1_General_100_BIN2='REJECTED' OR ([status]) collate Latin1_General_100_BIN2='ADMITTED' OR ([status]) collate Latin1_General_100_BIN2='CONFIRMED' OR ([status]) collate Latin1_General_100_BIN2='BAND_ASSIGNED' OR ([status]) collate Latin1_General_100_BIN2='TESTING' OR ([status]) collate Latin1_General_100_BIN2='PENDING') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- is_unfinished là predicate có lưu và filtered unique Student/Course; không permanent unique để chặn học lại. Không thêm Registration COMPLETED.
- Chỉ CONFIRMED→ADMITTED khi Admissions duyệt; ADMITTED + Invoice nguyên tử. Metadata confirmation/approval giữ khi hủy sau đó, không xóa lịch sử.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/RegistrationConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
