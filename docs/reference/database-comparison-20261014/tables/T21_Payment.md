# T21 — Payment: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 21 — Payment.

**Kết luận cấu trúc:** 11 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| payment_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Payment; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| invoice_id | int; NULL=N; FK Invoice, UQ | int; NULL=N; FK: FK_Payment_Invoice_invoice_id | Khớp tên/kiểu/NULL |
| amount_paid | decimal(18,0); NULL=N; - | decimal(18,0); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| method | varchar(16); NULL=N; - | varchar(16); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| transaction_reference | nvarchar(200); NULL=Y; - | nvarchar(200); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| confirmed_by | int; NULL=N; FK Account | int; NULL=N; FK: FK_Payment_Account_confirmed_by | Khớp tên/kiểu/NULL |
| paid_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| last_corrected_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_Payment_Account_last_corrected_by | Khớp tên/kiểu/NULL |
| last_corrected_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| last_correction_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Payment | PK | payment_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Payment_ConfirmedBy | Không | confirmed_by | Không filtered |
| IX_Payment_LastCorrectedBy | Không | last_corrected_by | Không filtered |
| IX_Payment_Paid | Không | paid_at, payment_id | Không filtered |
| UQ_Payment_Invoice | Có | invoice_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Payment_Amount | ([amount_paid]>(0)) |
| CK_Payment_CorrectionBundle | ([last_corrected_by] IS NULL AND [last_corrected_at] IS NULL AND [last_correction_reason] IS NULL OR [last_corrected_by] IS NOT NULL AND [last_corrected_at] IS NOT NULL AND [last_correction_reason] IS NOT NULL AND len([last_correction_reason])>(0)) |
| CK_Payment_Method | ((([method]) collate Latin1_General_100_BIN2='TRANSFER' OR ([method]) collate Latin1_General_100_BIN2='CASH') AND datalength([method])=len([method])) |
| CK_Payment_Reference | ([method]<>'TRANSFER' OR [transaction_reference] IS NOT NULL AND len([transaction_reference])>(0)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- amount_paid >0 và bằng Invoice snapshot theo transaction guard; CHECK amount dương chưa chứng minh bằng Invoice.
- Mã đúng CASH/TRANSFER, không BANK_TRANSFER. transaction_reference không unique; TRANSFER cần reference.
- Correction chỉ method/reference với reason; giữ invoice/amount/confirmedBy/paidAt; latest correction và audit.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/PaymentConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
