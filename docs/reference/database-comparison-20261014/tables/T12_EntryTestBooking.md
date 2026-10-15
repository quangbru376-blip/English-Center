# T12 — EntryTestBooking: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 12 — EntryTestBooking.

**Kết luận cấu trúc:** 14 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| booking_id | int; NULL=N; PK, ID | int; NULL=N; UQ: AK_Booking_IdAttempt; PK: PK_EntryTestBooking; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| attempt_id | int; NULL=N; FK EntryTestAttempt | int; NULL=N; UQ: AK_Booking_IdAttempt; FK: FK_Booking_Attempt_attempt_id | Khớp tên/kiểu/NULL |
| session_id | int; NULL=N; FK EntryTestSession | int; NULL=N; FK: FK_Booking_Session_session_id | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default SCHEDULED | varchar(16); NULL=N; DEFAULT ('SCHEDULED') | Khớp tên/kiểu/NULL |
| booked_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| absent_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Booking_Account_absent_by | Khớp tên/kiểu/NULL |
| absent_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| absent_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancelled_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Booking_Account_cancelled_by | Khớp tên/kiểu/NULL |
| cancelled_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancelled_from_status | varchar(16); NULL=Y; - | varchar(16); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| AK_Booking_IdAttempt | UQ | booking_id, attempt_id |
| PK_EntryTestBooking | PK | booking_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Booking_AbsentBy | Không | absent_by | Không filtered |
| IX_Booking_AttemptHistory | Không | attempt_id, booked_at, booking_id | Không filtered |
| IX_Booking_CancelledBy | Không | cancelled_by | Không filtered |
| IX_Booking_SessionState | Không | session_id, status, booking_id | Không filtered |
| UQ_Booking_Completed | Có | attempt_id | ([status]='COMPLETED') |
| UQ_Booking_Current | Có | attempt_id | ([status]='SCHEDULED') |

| CHECK | Predicate SQL |
| --- | --- |
| CK_EntryTestBooking_AbsenceBundle | ([status]='ABSENT' AND [absent_by] IS NOT NULL AND [absent_at] IS NOT NULL AND [absent_operation_id] IS NOT NULL OR [status]<>'ABSENT' AND [absent_by] IS NULL AND [absent_at] IS NULL AND [absent_operation_id] IS NULL) |
| CK_EntryTestBooking_CancellationBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND ([cancelled_from_status]) collate Latin1_General_100_BIN2='SCHEDULED' AND datalength([cancelled_from_status])=len([cancelled_from_status]) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| CK_EntryTestBooking_Status | ((([status]) collate Latin1_General_100_BIN2='CANCELLED' OR ([status]) collate Latin1_General_100_BIN2='ABSENT' OR ([status]) collate Latin1_General_100_BIN2='COMPLETED' OR ([status]) collate Latin1_General_100_BIN2='SCHEDULED') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Đây mới là liên kết Attempt→Session và nơi lưu SCHEDULED/COMPLETED/ABSENT/CANCELLED. Hình cũ thiếu bảng này.
- Một SCHEDULED và một COMPLETED tối đa cho Attempt bằng filtered UQ; history có nhiều booking. Rebook sau ABSENT không tạo lượt mới.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/EntryTestBookingConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
