# 6.3.5 — Đối chiếu ràng buộc liên quan hệ và giao dịch

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Đối chiếu toàn bộ các quy tắc trong 6.3.5

Các dòng danh sách placement và công thức tổng được gộp theo đúng một quy tắc để đọc được; không bỏ các điều kiện ở báo cáo. Chú ý FK/UNIQUE/CHECK đơn bảng không tự thực thi mọi quy tắc dưới đây.

| Quy tắc trong báo cáo | Đánh giá | Hiện tại/cách sửa | Tầng thực thi |
| --- | --- | --- | --- |
| BandAssignment.best_attempt_id phải trỏ đến một EntryTestAttempt có status = COMPLETED, thuộc cùng registration_id và có điểm cao nhất trong tối đa hai lượt thi của hồ sơ đó. | Sai vị trí status; bổ sung tie-break | Attempt không có status; kết quả đầy đủ + completed_booking đúng lượt/trạng thái và highest completed score. Hòa chọn Attempt1. | FK ghép + result bundle + TeacherEntryService; best selection trong transaction. |
| BandAssignment.band_id phải là Band có min_score lớn nhất nhưng không vượt quá điểm của best_attempt_id. | Khớp; bổ sung freeze | Band có min_score lớn nhất≤best score; seed0/25/45/65/85, first completed result freeze set. | FN02 ufn_BandForScore + BANDSET gate/TG03 + service. |
| Registration chỉ được chuyển sang BAND_ASSIGNED khi đã có BandAssignment. | Khớp; cần atomic | First score/result/best/Band cùng chuyển BAND_ASSIGNED; điểm lượt 2 giữ BAND_ASSIGNED. | TeacherEntryService + Registration/BandAssignment transaction. |
| Registration chỉ được chuyển sang CONFIRMED khi đang ở trạng thái BAND_ASSIGNED và học viên thực hiện xác nhận nhập học. | Khớp nhưng thiếu guard lượt chờ | Student own BAND_ASSIGNED có Band, không lượt SCHEDULED/taken-ungraded; CONFIRMED chưa Invoice. | StudentEnrollmentService current result/booking/RV guards. |
| Registration chỉ được chuyển sang ADMITTED khi đang ở trạng thái CONFIRMED, đã có Band hợp lệ và được tài khoản có vai trò tuyển sinh duyệt. | Khớp | CONFIRMED có Band và current Admissions NORMAL; literal role Admissions. | AdmissionsService kiểm role/state/versions. |
| Khi Registration chuyển sang ADMITTED, hệ thống phải tạo đúng một Invoice trong cùng transaction. Invoice.amount bằng Course.base_price của hồ sơ tại thời điểm phát hành. | Khớp; bổ sung counter/due | ADMITTED + một Invoice UNPAID + counter + audit/replay cùng giao dịch, học phí snapshot tại issue và ngày VN+7. | UQ_Invoice_Registration/Number + SP07 allocator + AdmissionsService. |
| EntryTestSession.grader_id và EntryTestAttempt.graded_by phải trỏ đến tài khoản có vai trò TEACHER. Người nhập điểm phải là giáo viên được phân công cho ca thi. | Khớp một phần; cần current task/time | Teacher current grader; initial score/ABSENT từ ca end; correction original grader vẫn assigned, reason/trước CONFIRMED. | TeacherEntryService; FK actor không kiểm role/time. |
| Số EntryTestAttempt đang chiếm chỗ trong một ca thi không được vượt EntryTestSession.capacity. Các lượt CANCELLED không chiếm chỗ. | Sai đối tượng đếm | Đếm EntryTestBooking SCHEDULED của Session, không EntryTestAttempt trong ca. Booking CANCELLED không chiếm chỗ; attempts không có session_id. | SqlStudentEnrollmentRepository capacity recheck/gates. |
| Class.teacher_id phải trỏ đến tài khoản có vai trò TEACHER. | Khớp; lịch sử không phụ thuộc ACTIVE mãi | Teacher role/ACTIVE khi phân công; Account bị block không tự đổi assignment. | ClassService/Teacher guards; FK chỉ existence. |
| StudentClassAssignment chỉ được tạo khi: Registration.status = ADMITTED. Hồ sơ có Invoice.status = PAID. Band của hồ sơ khớp TrainingProgram.band_id của lớp. Registration.course_id khớp TrainingProgram.course_id của lớp. Class.status ∈ {OPEN, ONGOING} theo quy tắc vận hành. Sĩ số hiện tại nhỏ hơn Class.max_size. | Sai cho phép ONGOING; thiếu guard | Placement chỉ Class OPEN trước buổi đầu; ADMITTED/PAID/cùng Course/Band, Student ACTIVE/current Teacher hợp lệ/còn chỗ/không trùng, chưa assignment. | ClassPreviews.Placement + ClassService + UQ_Assignment_Registration. |
| Việc xếp ca thi và xếp lớp phải được thực hiện trong transaction có khóa hoặc cơ chế kiểm soát đồng thời để hai nhân viên thao tác cùng lúc không làm vượt sức chứa. | Khớp; bổ sung scope/gates | Domain gates/state locks, Account/SID authority, phiên bản/manifests, deadline và commit recheck; không chỉ check capacity ở form. | Enrollment/Class executors và transaction repositories; không riêng FK/UNIQUE. |
| Schedule không được chồng lấn với một lịch khác nếu dùng cùng phòng, cùng lớp hoặc các lớp có cùng giáo viên phụ trách. | Thiếu cross-type/Student | Half-open [start,end); phòng/Teacher/Class và Student. Kiểm cả ca thi với lớp, occurrence current và booking effective phù hợp. | SqlClassTimetable/SqlSchedulePreviews + transaction/gates. |
| EntryTestSession không được chồng lấn với lịch lớp hoặc ca thi khác nếu dùng cùng phòng hoặc cùng giáo viên. | Khớp; bổ sung Student/half-open | Ca thi đối chiếu phòng/grader với ca/lớp; Student booking đối chiếu test-and-class lịch; end=start không overlap. | EntrySession resource guards + Student timetable guards. |
| Attendance.schedule_id phải trỏ đến Schedule có session_type = LESSON. | Khớp nhưng thiếu current/window | Attendance chỉ current LESSON không NOT_HELD; từ start đến trước Class COMPLETED, current Teacher. | TG04 relation/ended Class + AttendanceService time/authority. |
| Lớp của Attendance.class_assignment_id phải trùng với lớp của Attendance.schedule_id. | Khớp | Schedule.ClassId=Assignment.ClassId; FK từng cột riêng không chứng minh bằng nhau. | TG04 và service validation. |
| Score.component_id phải thuộc TrainingProgram của lớp mà class_assignment_id đang theo học. | Khớp; thêm kind/link | Component cùng Class.Program; GENERAL scheduleNULL, LESSON/FINAL_EXAM current cùngClass/type khôngNOT_HELD. | TG05 và GradeService/GradeWire.ScheduleValid. |
| Score.entered_by phải là giáo viên phụ trách lớp tương ứng, trừ trường hợp sửa dữ liệu được phân quyền đặc biệt và ghi đầy đủ vào AuditLog. | Sai ngoại lệ override | Chỉ current assigned Teacher nhập/sửa draft trong window/state cho phép; không quyền sửa đặc biệt Admin/Giáo vụ. entered_by latest actual writer không actor tùy nhập. | GradeService authority/task/provenance; audit actual changes. |
| GradeReport.final_score phải bằng: Σ(Score.score_value × ScoreComponentConfig.weight / 100) trên toàn bộ thành phần điểm của chương trình tương ứng. | Thiếu completeness/round/storage | Decimal weighted total trên đủ frozen components; half-up2 một lần. Thiếu điểm là Chưa đủ điểm; total draft tính khi đọc, final_score chỉ lưu khi FINALIZED; passed≥50. | FN05 ufn_GradeTotal + GradeService FINALIZE/CK_FinalResult. |
| GradeReport chỉ được chuyển sang SUBMITTED khi đã có đủ các thành phần điểm bắt buộc. | Thiếu whole-Class/COMPLETED | Teacher gửi toàn cohort Class COMPLETED, mọi report DRAFT/RETURNED đủ điểm/current links/versions, không unsaved edit; không auto/individual submit. | GradeWire.WorkflowEligible + GradeService transition + audit/replay. |
| Chỉ tài khoản có vai trò giáo vụ mới được chuyển bảng điểm từ SUBMITTED sang RETURNED hoặc FINALIZED. | Khớp nhưng thiếu whole cohort/reason | AcademicAffairs RETURN/FINALIZE toàn Class COMPLETED/toàn SUBMITTED; RETURN cần reason; FINALIZED immutable, không tier Director. | GradeService/grade preview/manifests và mandatory grouped audit. |
| Học viên chỉ được xem bảng điểm khi GradeReport.status = FINALIZED. | Khớp; thêm ownership và projection | Student chỉ own FINALIZED list/detail; không DRAFT/SUBMITTED/RETURNED, peer hoặc quản trị context. | Student grade reader hiện hành + current ownership/session guards. |
| Invoice.status = PAID chỉ hợp lệ khi tồn tại đúng một Payment tương ứng. | Khớp ở workflow, không CHECK tự động | Invoice PAID cùng đúng một Payment theo transaction chuẩn; UQ_Payment_Invoice bảo vệ tốiđa1. Không chỉ FK là tự đổi PAID. | FinanceService confirmation transaction + unique. |
| Payment.amount_paid phải bằng đúng Invoice.amount. | Khớp ở service | Payment.amount_paid đúng Invoice snapshot đầy đủ một lần; both >0. CK amount>0 không chứng minh equality cross-table. | Finance transaction validation/locks. |
| Payment.confirmed_by phải là tài khoản có vai trò ACCOUNTANT. | Khớp role; cần current authority | Literal Accountant, current NORMAL/ACTIVE authority; confirmedBy/At do server, giữ khi metadata correction. | FinanceService; FK existence không role. |
| Việc tạo Payment và chuyển Invoice sang PAID phải diễn ra trong cùng một transaction. | Khớp; bổ sung audit/replay | Payment + Invoice PAID + mandatory audit + completion receipt cùng commit; lỗi rollback tất cả, retry không payment2. | Finance executor/service + ReplayOperation/UQ. |
| Các thao tác duyệt hồ sơ, nhập điểm thi, xác định Band, xác nhận thanh toán, xếp lớp, trả lại/chốt bảng điểm và khóa/mở tài khoản phải tạo bản ghi AuditLog. | Khớp cho action trong danh mục, không mọi read | Mandatory audit theo action catalog trong cùng transaction. Login/logout/security-only best-effort; Attendance không AuditLog history riêng; Profile audit chỉ changedFields. | AuditPolicy/AttendanceService/SqlOwnProfileExecutor và service từng action. |

## 2. Quy tắc cần bổ sung vì báo cáo còn thiếu

- Tạo assignment đồng thời tạo GradeReport DRAFT; Registration vẫn ADMITTED và Đã xếp lớp chỉ là projection. Không tạo Score/Attendance0 khi placement.
- Unfinished=0 khi REJECTED/CANCELLED hoặc lượt học Class COMPLETED + GradeReport FINALIZED, kể cả không đạt. Trước đó không đăng ký trùng Student/Course; sau kết thúc có thể tạo Registration mới, không tái dùng kết quả/Band cũ.
- Program ACTIVE cần weight 100/đúng 1 FINAL_EXAM/LESSON count dương; first Class dùng freeze core/components/weights/count kể cả Class CANCELLED. Không duplicate Course/Band Program né freeze.
- Weekly schedule save all-or-none; preview không giữ chỗ/clock, candidate nào start≤now làm cả batch không lưu. Move/replacement giữ nghĩa vụ; source NOT_HELD/history, không đếm replacement thành buổi thêm.
- HELD do Giáo vụ khi now≥end và Class ONGOING/current PLANNED; Attendance/time không tự HELD. Class COMPLETE cần mọi current obligations HELD và đủrequired Attendance; chưa yêu cầu submit/finalize.
- Draft per Student khác whole-Class submit/return/finalize. Missing score khác0, missing Attendance khácVắng; full supplied validation trước no-change. GradeReport RV được touch khi Score/comment đổi thực.
- Account reset/change-password/lock/disable thu hồi phiên theo policy; unlock/restore không hồi sinh SID. Profile edit không revoke hoặc replay marker. Last ACTIVE Admin và self-lock guard thuộc transaction, không FK.
- Reports dùng consistent SNAPSHOT query; revenue theo Payment confirmation, billed noncancelled Invoice issue, debt current UNPAID. Tổng toàn filtered set; CSV query mới; Director aggregate-only.
- Unknown outcome sau gửi không đồng nghĩa rollback. Manual lookup/read state đúng quyền, không automatic resend; receipt không chứa plaintext credential.

## 3. Transaction granularity nên ghi trong báo cáo

| Thao tác | Đơn vị nguyên tử |
| --- | --- |
| Đặt/đổi/hủy booking | Attempt cần thiết + old/new Booking + Registration/chỗ + audit theo action/replay. |
| Chấm/sửa score đầu vào | Result bundle + completed Booking + best/Band/Registration/freeze + audit/replay. |
| Duyệt hồ sơ | ADMITTED + Invoice + counter + mandatory audit/replay. |
| Xác nhận thu tiền | Payment + PAID + audit/replay. |
| Xếp lớp | Assignment + DRAFT GradeReport + audit/replay. |
| Lập lịch tuần/thay buổi | Cả batch hoặc old NOT_HELD+new replacement cùng nghĩa vụ + audit/replay. |
| Điểm danh | Actual changed subset của supplied items một LESSON + enteredBy/At + replay; không AuditLog Attendance history riêng. |
| Điểm nháp | Scores/comment một Student + parent report RV + audit/replay. |
| Gửi/trả/chốt điểm | Toàn cohort reports; khi chốt thêm totals/publication/unfinished + grouped audit/replay. |
| Profile | Account/Profile cùng hai RV + changedFields audit; không replay completion hoặc revoke. |
| Quản trị credential/status | Account/securityVersion/temp state + related revocations + mandatory audit; secret không lưu vào receipt. |

Không viết “mọi bước từ đăng ký tới chốt điểm là một transaction”; đó là nhiều lượt của nhiều actor. SQL triggers đòi gate preheld nhưng không thay service kiểm current authority/clock hoặc tự triển khai workflow.


Nguồn dùng chung và phương pháp: [README](README.md).
