# T11 — EntryTestAttempt: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 11 — EntryTestAttempt.

**Kết luận cấu trúc:** 8 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| attempt_id | int; NULL=N; PK, ID | int; NULL=N; UQ: AK_Attempt_IdRegistration; PK: PK_EntryTestAttempt; IDENTITY(1,1); FK: FK_Attempt_Booking_CompletedBookingAttempt | Khớp tên/kiểu/NULL |
| registration_id | int; NULL=N; FK Registration | int; NULL=N; UQ: AK_Attempt_IdRegistration; FK: FK_Attempt_Registration_registration_id | Khớp tên/kiểu/NULL |
| attempt_number | int; NULL=N; UQ với registration_id | int; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| score | decimal(5,2); NULL=Y; - | decimal(5,2); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| graded_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Attempt_Account_graded_by | Khớp tên/kiểu/NULL |
| graded_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| completed_booking_id | int; NULL=Y; FK composite EntryTestBooking | int; NULL=Y; FK: FK_Attempt_Booking_CompletedBookingAttempt | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| AK_Attempt_IdRegistration | UQ | attempt_id, registration_id |
| PK_EntryTestAttempt | PK | attempt_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Attempt_GradedBy | Không | graded_by | Không filtered |
| UQ_Attempt_CompletedBooking | Có | completed_booking_id | ([completed_booking_id] IS NOT NULL) |
| UQ_Attempt_RegistrationNumber | Có | registration_id, attempt_number | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_EntryTestAttempt_Number | ([attempt_number]=(2) OR [attempt_number]=(1)) |
| CK_EntryTestAttempt_ResultBundle | ([score] IS NULL AND [graded_by] IS NULL AND [graded_at] IS NULL AND [completed_booking_id] IS NULL OR [score] IS NOT NULL AND [graded_by] IS NOT NULL AND [graded_at] IS NOT NULL AND [completed_booking_id] IS NOT NULL) |
| CK_EntryTestAttempt_Score | ([score] IS NULL OR [score]>=(0) AND [score]<=(100)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không có status hoặc session_id. Logical Attempt giữ attempt_number và bundle score/graded_by/graded_at/completed_booking_id.
- Trạng thái lịch hẹn nằm EntryTestBooking; completed_booking_id FK ghép cùng Attempt. Sửa score giữ original graded_by/graded_at.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/EntryTestAttemptConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
