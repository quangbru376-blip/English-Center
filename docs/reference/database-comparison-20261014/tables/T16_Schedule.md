# T16 — Schedule: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 16 — Schedule.

**Kết luận cấu trúc:** 18 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| schedule_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Schedule; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| class_id | int; NULL=N; FK Class | int; NULL=N; FK: FK_Schedule_Class_class_id | Khớp tên/kiểu/NULL |
| session_type | varchar(16); NULL=N; - | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| obligation_key | uniqueidentifier; NULL=N; server | uniqueidentifier; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| replaces_schedule_id | int; NULL=Y; FK Schedule, filtered UQ khi có | int; NULL=Y; FK: FK_Schedule_Schedule_replaces_schedule_id | Khớp tên/kiểu/NULL |
| is_current | bit; NULL=N; initial 1, server-owned | bit; NULL=N; DEFAULT (CONVERT([bit],(1))) | Khớp tên/kiểu/NULL |
| session_date | date; NULL=N; - | date; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| start_time | time(0); NULL=N; - | time(0); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| end_time | time(0); NULL=N; - | time(0); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| room | varchar(30); NULL=N; - | varchar(30); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| occurrence_state | varchar(16); NULL=N; default PLANNED | varchar(16); NULL=N; DEFAULT ('PLANNED') | Khớp tên/kiểu/NULL |
| held_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Schedule_Account_held_by | Khớp tên/kiểu/NULL |
| held_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| not_held_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Schedule_Account_not_held_by | Khớp tên/kiểu/NULL |
| not_held_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| not_held_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| replacement_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Schedule | PK | schedule_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Schedule_ClassCurrentDate | Không | class_id, is_current, session_date, start_time, schedule_id | Không filtered |
| IX_Schedule_HeldBy | Không | held_by | Không filtered |
| IX_Schedule_NotHeldBy | Không | not_held_by | Không filtered |
| IX_Schedule_RoomDate | Không | room, session_date, start_time, end_time, schedule_id | Không filtered |
| UQ_Schedule_CurrentFinalExam | Có | class_id | ([is_current]=(1) AND [session_type]='FINAL_EXAM') |
| UQ_Schedule_CurrentObligation | Có | class_id, obligation_key | ([is_current]=(1)) |
| UQ_Schedule_Predecessor | Có | replaces_schedule_id | ([replaces_schedule_id] IS NOT NULL) |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Schedule_CurrentState | (([occurrence_state]='HELD' OR [occurrence_state]='PLANNED') AND [is_current]=(1) OR [occurrence_state]='NOT_HELD' AND [is_current]=(0)) |
| CK_Schedule_HeldBundle | ([occurrence_state]='HELD' AND [held_by] IS NOT NULL AND [held_at] IS NOT NULL OR [occurrence_state]<>'HELD' AND [held_by] IS NULL AND [held_at] IS NULL) |
| CK_Schedule_NoSelfReplacement | ([replaces_schedule_id] IS NULL OR [replaces_schedule_id]<>[schedule_id]) |
| CK_Schedule_NotHeldBundle | ([occurrence_state]='NOT_HELD' AND [not_held_by] IS NOT NULL AND [not_held_at] IS NOT NULL AND [not_held_reason] IS NOT NULL AND len([not_held_reason])>(0) AND [replacement_operation_id] IS NOT NULL OR [occurrence_state]<>'NOT_HELD' AND [not_held_by] IS NULL AND [not_held_at] IS NULL AND [not_held_reason] IS NULL AND [replacement_operation_id] IS NULL) |
| CK_Schedule_Room | (datalength([room])>=(1) AND datalength([room])<=(30) AND datalength([room])=len([room]) AND NOT ([room]) collate Latin1_General_100_BIN2 like '%[^A-Z0-9_-]%') |
| CK_Schedule_State | ((([occurrence_state]) collate Latin1_General_100_BIN2='NOT_HELD' OR ([occurrence_state]) collate Latin1_General_100_BIN2='HELD' OR ([occurrence_state]) collate Latin1_General_100_BIN2='PLANNED') AND datalength([occurrence_state])=len([occurrence_state])) |
| CK_Schedule_Time | ([start_time]<[end_time] AND datepart(second,[start_time])=(0) AND datepart(second,[end_time])=(0)) |
| CK_Schedule_Type | ((([session_type]) collate Latin1_General_100_BIN2='FINAL_EXAM' OR ([session_type]) collate Latin1_General_100_BIN2='LESSON') AND datalength([session_type])=len([session_type])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Root/replacement cùng obligation_key; một current occurrence/nghĩa vụ, một current FINAL_EXAM/Class; không unique Class/type trên toàn lịch sử.
- HELD do Giáo vụ khi now≥end, Class ONGOING/current PLANNED; Attendance hoặc clock không tự HELD. Move/replacement/lịch tuần có các guard riêng.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/ScheduleConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
