# T15 — StudentClassAssignment: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 15 — StudentClassAssignment.

**Kết luận cấu trúc:** 5 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| class_assignment_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_StudentClassAssignment; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| class_id | int; NULL=N; FK Class | int; NULL=N; FK: FK_Assignment_Class_class_id | Khớp tên/kiểu/NULL |
| registration_id | int; NULL=N; FK Registration, UQ | int; NULL=N; FK: FK_Assignment_Registration_registration_id | Khớp tên/kiểu/NULL |
| assigned_by | int; NULL=N; FK Account | int; NULL=N; FK: FK_Assignment_Account_assigned_by | Khớp tên/kiểu/NULL |
| assigned_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_StudentClassAssignment | PK | class_assignment_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Assignment_AssignedBy | Không | assigned_by | Không filtered |
| IX_Assignment_Class | Không | class_id, class_assignment_id | Không filtered |
| UQ_Assignment_Registration | Có | registration_id | Không filtered |

Bảng không có CHECK đơn bảng riêng; không suy ra là không có kiểm tra nghiệp vụ.

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- PK class_assignment_id, FK registration_id thay student_id/assignment_id của hình cũ. Student xác định qua Registration.student_id.
- Placement ADMITTED/PAID/Class OPEN trước buổi đầu/cùng Course/Band/còn chỗ/không trùng; tạo DRAFT GradeReport cùng commit. Không chuyển/gỡ lớp sau xếp.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/StudentClassAssignmentConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
