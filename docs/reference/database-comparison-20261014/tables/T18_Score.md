# T18 — Score: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 18 — Score.

**Kết luận cấu trúc:** 8 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| score_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_Score; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| class_assignment_id | int; NULL=N; FK StudentClassAssignment | int; NULL=N; FK: FK_Score_StudentClassAssignment_class_assignment_id | Khớp tên/kiểu/NULL |
| component_id | int; NULL=N; FK ScoreComponentConfig | int; NULL=N; FK: FK_Score_ScoreComponentConfig_component_id | Khớp tên/kiểu/NULL |
| schedule_id | int; NULL=Y; FK Schedule | int; NULL=Y; FK: FK_Score_Schedule_schedule_id | Khớp tên/kiểu/NULL |
| score_value | decimal(5,2); NULL=N; Không default | decimal(5,2); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| entered_by | int; NULL=N; FK Account | int; NULL=N; FK: FK_Score_Account_entered_by | Khớp tên/kiểu/NULL |
| entered_at | datetime2(3); NULL=N; server UTC | datetime2(3); NULL=N; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_Score | PK | score_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_Score_Component | Không | component_id, score_id | Không filtered |
| IX_Score_EnteredBy | Không | entered_by | Không filtered |
| IX_Score_Schedule | Không | schedule_id, score_id | ([schedule_id] IS NOT NULL) |
| UQ_Score_AssignmentComponent | Có | class_assignment_id, component_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_Score_Score | ([score_value]>=(0) AND [score_value]<=(100)) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- GENERAL schedule_id NULL; LESSON/FINAL_EXAM cần current occurrence cùngClass/type, khôngNOT_HELD, từ start. TG05 bảo vệ relation, service bảo vệ clock/quyền.
- Lưu actual changes + comment một Student/touch GradeReport RV; không submit một Student. Không override score đặc biệt cho Admin/Giáo vụ.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/ScoreConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
