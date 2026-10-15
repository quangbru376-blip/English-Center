# T13 — BandAssignment: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 13 — BandAssignment.

**Kết luận cấu trúc:** 6 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| band_assignment_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_BandAssignment; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| registration_id | int; NULL=N; FK Registration, UQ | int; NULL=N; FK: FK_BandAssignment_Attempt_BestAttemptRegistration; FK: FK_BandAssignment_Registration_registration_id | Khớp tên/kiểu/NULL |
| best_attempt_id | int; NULL=N; FK composite EntryTestAttempt | int; NULL=N; FK: FK_BandAssignment_Attempt_BestAttemptRegistration | Khớp tên/kiểu/NULL |
| band_id | int; NULL=N; FK Band | int; NULL=N; FK: FK_BandAssignment_Band_band_id | Khớp tên/kiểu/NULL |
| assigned_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; DEFAULT (sysutcdatetime()) | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_BandAssignment | PK | band_assignment_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_BandAssignment_Band | Không | band_id | Không filtered |
| IX_BandAssignment_BestAttempt | Không | best_attempt_id, registration_id | Không filtered |
| UQ_BandAssignment_Registration | Có | registration_id | Không filtered |

Bảng không có CHECK đơn bảng riêng; không suy ra là không có kiểm tra nghiệp vụ.

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- FK ghép best_attempt_id/registration_id chặn Attempt khác hồ sơ; service chọn best completed score, hòa chọn lượt 1.
- Không có UQ riêng best_attempt_id. Không được viết tên unique index không tồn tại; uniqueness theo nghĩa nghiệp vụ suy từ FK ghép và UQ Registration.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/BandAssignmentConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
