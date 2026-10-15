# T19 — GradeReport: đối chiếu chi tiết bảng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

**Vị trí nguồn:** mục 6.2, bảng 19 — GradeReport.

**Kết luận cấu trúc:** 14 cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại. Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.

## 1. Từng cột

| Cột | Báo cáo 6.2 | Source hiện tại | Đánh giá |
| --- | --- | --- | --- |
| report_id | int; NULL=N; PK, ID | int; NULL=N; PK: PK_GradeReport; IDENTITY(1,1) | Khớp tên/kiểu/NULL |
| class_assignment_id | int; NULL=N; FK StudentClassAssignment, UQ | int; NULL=N; FK: FK_GradeReport_StudentClassAssignment_class_assignment_id | Khớp tên/kiểu/NULL |
| final_score | decimal(5,2); NULL=Y; - | decimal(5,2); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| status | varchar(16); NULL=N; default DRAFT | varchar(16); NULL=N; DEFAULT ('DRAFT') | Khớp tên/kiểu/NULL |
| teacher_comment | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| submitted_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_GradeReport_Account_submitted_by | Khớp tên/kiểu/NULL |
| submitted_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| submitted_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| reviewed_by | int; NULL=Y; FK Account | int; NULL=Y; FK: FK_GradeReport_Account_reviewed_by | Khớp tên/kiểu/NULL |
| reviewed_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| review_operation_id | uniqueidentifier; NULL=Y; - | uniqueidentifier; NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| returned_reason | nvarchar(2000); NULL=Y; - | nvarchar(2000); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| finalized_at | datetime2(3); NULL=Y; - | datetime2(3); NULL=Y; Không default/PK/FK riêng | Khớp tên/kiểu/NULL |
| row_version | rowversion; NULL=N; RV | rowversion; NULL=N; Concurrency token tự sinh | Khớp tên/kiểu/NULL |

`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.

## 2. Khóa, indexes và CHECK thực tế

| Tên | Loại | Cột/điều kiện |
| --- | --- | --- |
| PK_GradeReport | PK | report_id |

| Index | Unique | Cột | Filter |
| --- | --- | --- | --- |
| IX_GradeReport_ReviewedBy | Không | reviewed_by | Không filtered |
| IX_GradeReport_StateFinalized | Không | status, finalized_at, report_id | Không filtered |
| IX_GradeReport_SubmittedBy | Không | submitted_by | Không filtered |
| UQ_GradeReport_Assignment | Có | class_assignment_id | Không filtered |

| CHECK | Predicate SQL |
| --- | --- |
| CK_GradeReport_FinalResult | ([status]='FINALIZED' AND [final_score] IS NOT NULL AND [final_score]>=(0) AND [final_score]<=(100) AND [finalized_at] IS NOT NULL AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL AND [finalized_at]=[reviewed_at] OR [status]<>'FINALIZED' AND [final_score] IS NULL AND [finalized_at] IS NULL) |
| CK_GradeReport_ReturnedReason | ([status]<>'RETURNED' OR [returned_reason] IS NOT NULL AND len([returned_reason])>(0) AND [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL) |
| CK_GradeReport_ReviewBundle | ([reviewed_by] IS NULL AND [reviewed_at] IS NULL AND [review_operation_id] IS NULL OR [reviewed_by] IS NOT NULL AND [reviewed_at] IS NOT NULL AND [review_operation_id] IS NOT NULL) |
| CK_GradeReport_Status | ((([status]) collate Latin1_General_100_BIN2='FINALIZED' OR ([status]) collate Latin1_General_100_BIN2='RETURNED' OR ([status]) collate Latin1_General_100_BIN2='SUBMITTED' OR ([status]) collate Latin1_General_100_BIN2='DRAFT') AND datalength([status])=len([status])) |
| CK_GradeReport_SubmitBundle | ([submitted_by] IS NOT NULL AND [submitted_at] IS NOT NULL AND [submitted_operation_id] IS NOT NULL OR [status]='DRAFT' AND [submitted_by] IS NULL AND [submitted_at] IS NULL AND [submitted_operation_id] IS NULL) |

## 3. Mô tả và nghiệp vụ cần giữ/sửa

- Không approved_by/approved_at; reviewer fields và operation IDs đầy đủ. final_score chỉ khi FINALIZED; tổng nháp tính khi đọc.
- RETURNED reason bắt buộc; latest reason có thể giữ sau resubmit và finalize. Không có CHECK bắt returned_reason NULL ở FINALIZED như đoạn 6.3.2 cũ.
- SUBMIT/RETURN/FINALIZE toàn Class, decimal half-up2 mộtlần; passed khi FINALIZED≥50; không reopen.

## 4. Nguồn và liên kết

- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/GradeReportConfiguration.cs).
- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).
- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).
- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.
- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).
