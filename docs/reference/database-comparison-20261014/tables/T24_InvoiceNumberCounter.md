# T24 — InvoiceNumberCounter: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** chưa được liệt kê trong mục 6.2; cần bổ sung bảng kỹ thuật, không thêm màn hình nghiệp vụ.

**Kết luận cấu trúc:** Bổ sung 3 cột của bảng hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| issue_year | Chưa có | smallint; NULL=N; PK: PK_InvoiceNumberCounter | Bổ sung |
| last_value | Chưa có | int; NULL=N; DEFAULT ((0)) | Bổ sung |
| row_version | Chưa có | rowversion; NULL=N; Concurrency token tự sinh | Bổ sung |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_InvoiceNumberCounter | PK | issue_year |

Không có index riêng ngoài index của PK/AK trên bảng này.

| CHECK | Predicate SQL |
| --- | --- |
| CK_InvoiceNumberCounter_Value | ([last_value]>=(0) AND [last_value]<=(99999999)) |
| CK_InvoiceNumberCounter_Year | ([issue_year]>=(1) AND [issue_year]<=(9999)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Thiếu ở 6.2 cũ. Một row/năm VN, issue_year smallint PK, last_value 0–99.999.999 và internal RV.
- Allocator row/range lock trong duyệt, không reset/reuse số đã phát hành; không hứa số liên tục và không FK sang Invoice.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/InvoiceNumberCounterConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
