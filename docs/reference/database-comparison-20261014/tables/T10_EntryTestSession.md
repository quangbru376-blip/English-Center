# T10 — EntryTestSession: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 10 — EntryTestSession.

**Kết luận cấu trúc:** 12 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| session_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_EntryTestSession; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| start_at | datetime2(3); NULL=N; - | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| end_at | datetime2(3); NULL=N; - | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| room | varchar(30); NULL=N; - | varchar(30); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| capacity | int; NULL=N; - | int; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| grader_id | int; NULL=N; FK Account | int; NULL=N; FK: FK_EntryTestSession_Account_grader_id | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default CLOSED | varchar(16); NULL=N; DEFAULT ('CLOSED') | Khớp tên/kiểu/NULL |
| cancelled_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_EntryTestSession_Account_cancelled_by | Khớp tên/kiểu/NULL |
| cancelled_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_EntryTestSession | PK | session_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Session_CancelledBy | Không | cancelled_by | Không filtered |
| IX_Session_GraderStart | Không | grader_id, start_at, end_at, session_id | Không filtered |
| IX_Session_RoomStart | Không | room, start_at, end_at, session_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_EntryTestSession_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancellation_reason] IS NOT NULL AND len([cancellation_reason])>(0) AND [cancellation_operation_id] IS NOT NULL OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancellation_operation_id] IS NULL) |
| CK_EntryTestSession_Capacity | ([capacity]>=(1) AND [capacity]<=(20)) |
| CK_EntryTestSession_Room | (datalength([room])>=(1) AND datalength([room])<=(30) AND datalength([room])=len([room]) AND NOT ([room]) collate Latin1_General_100_BIN2 like '%[^A-Z0-9_-]%') |
| CK_EntryTestSession_Status | ((([status]) collate Latin1_General_100_BIN2='CANCELLED' OR ([status]) collate Latin1_General_100_BIN2='CLOSED' OR ([status]) collate Latin1_General_100_BIN2='OPEN') AND datalength([status])=len([status])) |
| CK_EntryTestSession_Time | ([start_at]<[end_at] AND datepart(second,[start_at])=(0) AND datepart(millisecond,[start_at])=(0) AND datepart(second,[end_at])=(0) AND datepart(millisecond,[end_at])=(0) AND CONVERT([date],dateadd(minute,(420),[start_at]))=CONVERT([date],dateadd(minute,(420),[end_at]))) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Mới CLOSED; OPEN trước start cho booking; effectively CLOSED từ start không đồng nghĩa read tự UPDATE.
- start_at/end_at UTC datetime2(3), không session_date/session_time của hình 6.4. Grader hiện tại và overlap/capacity là guard/service.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/EntryTestSessionConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
