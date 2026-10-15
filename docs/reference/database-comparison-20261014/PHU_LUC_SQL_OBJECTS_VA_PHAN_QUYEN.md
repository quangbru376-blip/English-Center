# Phụ lục — SQL objects và phân quyền database hiện tại

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

Đây là **phụ lục bổ sung**, không giả định báo cáo cũ đã có một mục trigger/view/function/procedure riêng. Phần VI cũ chỉ có hình, từ điển và ràng buộc; cần dẫn cơ chế triển khai SQL để các mô tả “Hệ thống bảo đảm” có nguồn.

## 1. SQL objects trong source hiện tại

- 5 triggers cho used-Program/component freeze, Band threshold freeze, Attendance relation và Score relation/grade guard.
- 6 read-only views; 6 UDF gồm 5 inline TVF và 1 scalar UDF; 7 stored procedures cho queries và invoice number allocator.
- SP có tổng 47 parameters; không dùng tổng parameter của cả functions và SP làm số SP parameters.

| Đối tượng | Loại | Số parameters | Source |
| --- | --- | --- | --- |
| trg_Attendance_ClassScheduleGuard | Trigger | 0 | [TG04.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG04.sql) |
| trg_Band_ThresholdSetFreezeGuard | Trigger | 0 | [TG03.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG03.sql) |
| trg_Component_UsedConfigurationGuard | Trigger | 0 | [TG02.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG02.sql) |
| trg_Program_UsedConfigurationGuard | Trigger | 0 | [TG01.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG01.sql) |
| trg_Score_ComponentScheduleGradeGuard | Trigger | 0 | [TG05.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D08/TG05.sql) |
| ufn_BandForScore | Inline TVF | 1 | [FN02.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN02.sql) |
| ufn_ClassObligationProgress | Inline TVF | 1 | [FN04.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN04.sql) |
| ufn_EffectiveEntryTestState | Scalar UDF | 4 | [FN01.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN01.sql) |
| ufn_GradeTotal | Inline TVF | 1 | [FN05.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN05.sql) |
| ufn_InvoiceDebtFlags | Inline TVF | 3 | [FN06.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN06.sql) |
| ufn_RegistrationUnfinished | Inline TVF | 1 | [FN03.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D09/FN03.sql) |
| usp_AllocateInvoiceNumber | Stored procedure | 1 | [SP07.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP07.sql) |
| usp_GetAdmissionsReport | Stored procedure | 6 | [SP05.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP05.sql) |
| usp_GetClassGradeReview | Stored procedure | 2 | [SP03.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP03.sql) |
| usp_GetEntryTestRoster | Stored procedure | 11 | [SP02.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP02.sql) |
| usp_GetFinancialReport | Stored procedure | 7 | [SP04.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP04.sql) |
| usp_GetTrainingReport | Stored procedure | 9 | [SP06.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP06.sql) |
| usp_SearchAdmissions | Stored procedure | 11 | [SP01.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP01.sql) |
| vw_AdmissionsRegistrationFacts | View | 0 | [VW01.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW01.sql) |
| vw_CurrentScheduleOccurrences | View | 0 | [VW03.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW03.sql) |
| vw_EntryTestBookingFacts | View | 0 | [VW02.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW02.sql) |
| vw_FinalizedGradeFacts | View | 0 | [VW05.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW05.sql) |
| vw_FinanceInvoiceFacts | View | 0 | [VW06.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW06.sql) |
| vw_TrainingClassFacts | View | 0 | [VW04.sql](../../../backend/src/EnglishCenter.Api/Infrastructure/Sql/D10/VW04.sql) |

## 2. Trigger không thay toàn bộ workflow

TG01/TG02 bảo vệ cấu hình đã dùng, TG03 Band freeze, TG04 Attendance class/schedule/ended-Class, TG05 Score component/schedule/submitted/finalized guards. Caller phải prehold đúng transaction gates; trigger kiểm APPLOCK_MODE, không tự acquire sau DML. Các rule current role/session/ownership/deadline, source-state transition và full manifests còn do application transaction thực hiện.

Các function/view là nguồn tính/chiếu đọc, không tự cập nhật Band, finalize report hoặc xác nhận Payment. SP allocator cấp số trong transaction duyệt, không là chức năng phát hành độc lập cho Kế toán.

## 3. Database roles khác account roles

| Database role | Số permissions catalog | GRANT | DENY |
| --- | --- | --- | --- |
| ec_academics_reporting | 7 | 3 | 4 |
| ec_app_runtime | 82 | 82 | 0 |
| ec_director_reporting | 7 | 3 | 4 |
| ec_finance_reporting | 6 | 2 | 4 |

Tổng 102 permission entries trong baseline nguồn; bốn database roles khác bảy account roles trong Role. Không đưa ec_app_runtime hoặc ec_finance_reporting thành role đăng nhập ứng dụng. Nguồn role/grants ở migration LeastPrivilegeRoles và SQL D12. Việc có role/grant trong source không một mình chứng minh một login live đã được bind đúng hoặc readiness hiện tại.

## 4. Những thứ không bổ sung như bảng sản phẩm

Reports không tạo bảng fact riêng, không lịch sử PII snapshot hoặc bảng học bổng; dùng views/queries nhất quán. __EFMigrationsHistory là bảng framework, không bảng nghiệp vụ thứ25. Sao lưu/phục hồi M4 còn cần đầu vào vận hành: không thêm bảng backup hoặc tuyên bố restore hoàn tất từ các migration trên.


Nguồn dùng chung và phương pháp: [README](README.md).
