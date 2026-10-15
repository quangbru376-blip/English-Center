# T23 — ReplayOperation: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** chưa được liệt kê trong mục 6.2; cần bổ sung bảng kỹ thuật, không thêm màn hình nghiệp vụ.

**Kết luận cấu trúc:** Bổ sung 10 cột của bảng hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| replay_id | Chưa có | int; NULL=N; PK: PK_ReplayOperation; IDENTITY(1,1) | Bổ sung |
| actor_account_id | Chưa có | int; NULL=N; FK: FK_ReplayOperation_Account_actor_account_id | Bổ sung |
| action_code | Chưa có | varchar(80); NULL=N; Không default/PK/FK riêng | Bổ sung |
| request_id | Chưa có | uniqueidentifier; NULL=N; Không default/PK/FK riêng | Bổ sung |
| input_digest | Chưa có | binary(32); NULL=N; Không default/PK/FK riêng | Bổ sung |
| operation_id | Chưa có | uniqueidentifier; NULL=N; Không default/PK/FK riêng | Bổ sung |
| result_mode | Chưa có | varchar(32); NULL=N; Không default/PK/FK riêng | Bổ sung |
| result_reference | Chưa có | nvarchar(max); NULL=N; Không default/PK/FK riêng | Bổ sung |
| completed_at | Chưa có | datetime2(3); NULL=N; Không default/PK/FK riêng | Bổ sung |
| expires_at | Chưa có | datetime2(3); NULL=N; Không default/PK/FK riêng | Bổ sung |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_ReplayOperation | PK | replay_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Replay_Expiry | Không | expires_at | Không filtered |
| IX_Replay_Operation | Không | operation_id | Không filtered |
| UQ_Replay_ActorActionRequest | Có | actor_account_id, action_code, request_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_ReplayOperation_Expiry | ([expires_at]>=dateadd(hour,(24),[completed_at])) |
| CK_ReplayOperation_Mode | ((([result_mode]) collate Latin1_General_100_BIN2='SECRET_RESULT_UNAVAILABLE' OR ([result_mode]) collate Latin1_General_100_BIN2='SAFE_REFERENCE') AND datalength([result_mode])=len([result_mode])) |
| CK_ReplayOperation_ResultReference | (isjson([result_reference])=(1)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Thiếu hoàn toàn ở 6.2 cũ. Completion receipt chống hiệu ứng ghi lặp theo actor/action/request, cùng transaction mutation; no marker khi rollback.
- Không lưu credential/raw PII response; SECRET_RESULT_UNAVAILABLE không replay plaintext. Không mọi API đều dùng bảng này: Profile và self-password có hợp đồng riêng.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/ReplayOperationConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
