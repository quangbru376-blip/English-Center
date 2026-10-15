# T17 — Attendance: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 17 — Attendance.

**Kết luận cấu trúc:** 7 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| attendance_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Attendance; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| schedule_id | int; NULL=N; FK Schedule | int; NULL=N; FK: FK_Attendance_Schedule_schedule_id | Khớp tên/kiểu/NULL |
| class_assignment_id | int; NULL=N; FK StudentClassAssignment | int; NULL=N; FK: FK_Attendance_StudentClassAssignment_class_assignment_id | Khớp tên/kiểu/NULL |
| present | bit; NULL=N; Không default | bit; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| entered_by | int; NULL=N; FK Account | int; NULL=N; FK: FK_Attendance_Account_entered_by | Khớp tên/kiểu/NULL |
| entered_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Attendance | PK | attendance_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Attendance_Assignment | Không | class_assignment_id, attendance_id | Không filtered |
| IX_Attendance_EnteredBy | Không | entered_by | Không filtered |
| UQ_Attendance_ScheduleAssignment | Có | schedule_id, class_assignment_id | Không filtered |

Bảng không có CHECK đơn bảng riêng; không suy ra là không có kiểm tra nghiệp vụ.

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không row là chưa ghi; bit0 là Vắng có chủ đích, không default present. Một lần save tập supplied changes của một LESSON nguyên tử.
- Không AuditLog nghiệp vụ Attendance history riêng trong code hiện tại: latest enteredBy/At và replay; không thêm audit theo suy đoán.
- TG04 chặn sai Class/type/current occurrence và Class ended; Teacher hiện tại/thời gian/RV ở service.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AttendanceConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
