# T20 — Invoice: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 20 — Invoice.

**Kết luận cấu trúc:** 13 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| invoice_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Invoice; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| invoice_number | varchar(16); NULL=N; UQ_Invoice_Number | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| registration_id | int; NULL=N; FK Registration, UQ | int; NULL=N; FK: FK_Invoice_Registration_registration_id | Khớp tên/kiểu/NULL |
| amount | decimal(18,0); NULL=N; - | decimal(18,0); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| issued_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| due_date | date; NULL=N; server | date; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default UNPAID | varchar(16); NULL=N; DEFAULT ('UNPAID') | Khớp tên/kiểu/NULL |
| cancelled_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Invoice_Account_cancelled_by | Khớp tên/kiểu/NULL |
| cancelled_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancelled_from_status | varchar(16); NULL=Y; - | varchar(16); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| cancellation_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Invoice | PK | invoice_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Invoice_CancelledBy | Không | cancelled_by | Không filtered |
| IX_Invoice_Issued | Không | issued_at, invoice_id | Không filtered |
| IX_Invoice_StateDue | Không | status, due_date, invoice_id | Không filtered |
| UQ_Invoice_Number | Có | invoice_number | Không filtered |
| UQ_Invoice_Registration | Có | registration_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Invoice_Amount | ([amount]>(0)) |
| CK_Invoice_CancelledBundle | ([status]='CANCELLED' AND [cancelled_by] IS NOT NULL AND [cancelled_at] IS NOT NULL AND [cancelled_from_status] IS NOT NULL AND [cancellation_operation_id] IS NOT NULL AND ([cancelled_from_status]) collate Latin1_General_100_BIN2='UNPAID' AND datalength([cancelled_from_status])=len([cancelled_from_status]) OR [status]<>'CANCELLED' AND [cancelled_by] IS NULL AND [cancelled_at] IS NULL AND [cancellation_reason] IS NULL AND [cancelled_from_status] IS NULL AND [cancellation_operation_id] IS NULL) |
| CK_Invoice_NumberPattern | (datalength([invoice_number])=(16) AND datalength([invoice_number])=len([invoice_number]) AND ([invoice_number]) collate Latin1_General_100_BIN2 like 'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]') |
| CK_Invoice_Status | ((([status]) collate Latin1_General_100_BIN2='CANCELLED' OR ([status]) collate Latin1_General_100_BIN2='PAID' OR ([status]) collate Latin1_General_100_BIN2='UNPAID') AND datalength([status])=len([status])) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- amount >0; số HD-YYYY-NNNNNNNN do allocator cấp, không invoice_id. Issue/due snapshot, ngày VN+7 lịch.
- Chỉ tạo cùng duyệt tuyển sinh; UNPAID/PAID/CANCELLED, overdue là flag không state. Không Invoice PAID thiếu Payment qua workflow chuẩn.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/InvoiceConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
