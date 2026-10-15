# T03 — StudentProfile: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 03 — StudentProfile.

**Kết luận cấu trúc:** 5 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| student_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_StudentProfile; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| account_id | int; NULL=N; FK Account, UQ_StudentProfile_Account | int; NULL=N; FK: FK_StudentProfile_Account_account_id | Khớp tên/kiểu/NULL |
| date_of_birth | date; NULL=N; - | date; NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| address | nvarchar(510); NULL=Y; - | nvarchar(510); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_StudentProfile | PK | student_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| UQ_StudentProfile_Account | Có | account_id | Không filtered |

Bảng không có CHECK đơn bảng riêng; không suy ra là không có kiểm tra nghiệp vụ.

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Sửa profile kiểm RV Account và Profile trong cùng giao dịch; audit chỉ changedFields. Không replay completion/receipt hoặc revoke-all trong API Profile hiện tại.
- DOB không sau ngày VN hiện tại do validation/service, không có CHECK gọi đồng hồ. Staff không có Profile dù FK riêng không kiểm role.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/StudentProfileConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
