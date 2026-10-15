# 6.3.4 — Đối chiếu ràng buộc tham chiếu và bảng tầm ảnh hưởng

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Kết luận

**49 quan hệ tham chiếu được viết bằng chữ trong báo cáo đều khớp endpoints/cột với source hiện tại**, kể cả hai FK ghép. Không cần xóa/vẽ lại toàn bộ danh mục này vì hai hình cũ sai. Schema có **50 FK**; quan hệ bị thiếu là ReplayOperation.actor_account_id → Account.account_id.

FK catalogue nguồn ghi delete/update NO ACTION, không disabled/untrusted. Đây là đối chiếu baseline nguồn, không là truy vấn trạng thái FK live mới.

## 2. Đối chiếu từng FK trong báo cáo

| STT báo cáo | Quan hệ | Tên FK hiện tại | NULL phía con | Kết quả |
| --- | --- | --- | --- | --- |
| 1 | Account(role_id) → Role(role_id) | FK_Account_Role_role_id | role_id=N | Khớp endpoints; NO ACTION delete/update |
| 2 | StudentProfile(account_id) → Account(account_id) | FK_StudentProfile_Account_account_id | account_id=N | Khớp endpoints; NO ACTION delete/update |
| 3 | AuthSession(account_id) → Account(account_id) | FK_AuthSession_Account_account_id | account_id=N | Khớp endpoints; NO ACTION delete/update |
| 4 | TrainingProgram(course_id) → Course(course_id) | FK_TrainingProgram_Course_course_id | course_id=N | Khớp endpoints; NO ACTION delete/update |
| 5 | TrainingProgram(band_id) → Band(band_id) | FK_TrainingProgram_Band_band_id | band_id=N | Khớp endpoints; NO ACTION delete/update |
| 6 | ScoreComponentConfig(program_id) → TrainingProgram(program_id) | FK_ScoreComponentConfig_TrainingProgram_program_id | program_id=N | Khớp endpoints; NO ACTION delete/update |
| 7 | Registration(student_id) → StudentProfile(student_id) | FK_Registration_StudentProfile_student_id | student_id=N | Khớp endpoints; NO ACTION delete/update |
| 8 | Registration(course_id) → Course(course_id) | FK_Registration_Course_course_id | course_id=N | Khớp endpoints; NO ACTION delete/update |
| 9 | Registration(admission_approved_by) → Account(account_id) | FK_Registration_Account_admission_approved_by | admission_approved_by=Y | Khớp endpoints; NO ACTION delete/update |
| 10 | Registration(rejected_by) → Account(account_id) | FK_Registration_Account_rejected_by | rejected_by=Y | Khớp endpoints; NO ACTION delete/update |
| 11 | Registration(cancelled_by) → Account(account_id) | FK_Registration_Account_cancelled_by | cancelled_by=Y | Khớp endpoints; NO ACTION delete/update |
| 12 | EntryTestSession(grader_id) → Account(account_id) | FK_EntryTestSession_Account_grader_id | grader_id=N | Khớp endpoints; NO ACTION delete/update |
| 13 | EntryTestSession(cancelled_by) → Account(account_id) | FK_EntryTestSession_Account_cancelled_by | cancelled_by=Y | Khớp endpoints; NO ACTION delete/update |
| 14 | EntryTestAttempt(registration_id) → Registration(registration_id) | FK_Attempt_Registration_registration_id | registration_id=N | Khớp endpoints; NO ACTION delete/update |
| 15 | EntryTestAttempt(graded_by) → Account(account_id) | FK_Attempt_Account_graded_by | graded_by=Y | Khớp endpoints; NO ACTION delete/update |
| 16 | EntryTestAttempt(completed_booking_id, attempt_id) → EntryTestBooking(booking_id, attempt_id) | FK_Attempt_Booking_CompletedBookingAttempt | completed_booking_id=Y, attempt_id=N | Khớp endpoints; NO ACTION delete/update |
| 17 | EntryTestBooking(attempt_id) → EntryTestAttempt(attempt_id) | FK_Booking_Attempt_attempt_id | attempt_id=N | Khớp endpoints; NO ACTION delete/update |
| 18 | EntryTestBooking(session_id) → EntryTestSession(session_id) | FK_Booking_Session_session_id | session_id=N | Khớp endpoints; NO ACTION delete/update |
| 19 | EntryTestBooking(absent_by) → Account(account_id) | FK_Booking_Account_absent_by | absent_by=Y | Khớp endpoints; NO ACTION delete/update |
| 20 | EntryTestBooking(cancelled_by) → Account(account_id) | FK_Booking_Account_cancelled_by | cancelled_by=Y | Khớp endpoints; NO ACTION delete/update |
| 21 | BandAssignment(registration_id) → Registration(registration_id) | FK_BandAssignment_Registration_registration_id | registration_id=N | Khớp endpoints; NO ACTION delete/update |
| 22 | BandAssignment(best_attempt_id, registration_id) → EntryTestAttempt(attempt_id, registration_id) | FK_BandAssignment_Attempt_BestAttemptRegistration | best_attempt_id=N, registration_id=N | Khớp endpoints; NO ACTION delete/update |
| 23 | BandAssignment(band_id) → Band(band_id) | FK_BandAssignment_Band_band_id | band_id=N | Khớp endpoints; NO ACTION delete/update |
| 24 | Class(program_id) → TrainingProgram(program_id) | FK_Class_TrainingProgram_program_id | program_id=N | Khớp endpoints; NO ACTION delete/update |
| 25 | Class(teacher_id) → Account(account_id) | FK_Class_Account_teacher_id | teacher_id=N | Khớp endpoints; NO ACTION delete/update |
| 26 | Class(cancelled_by) → Account(account_id) | FK_Class_Account_cancelled_by | cancelled_by=Y | Khớp endpoints; NO ACTION delete/update |
| 27 | StudentClassAssignment(class_id) → Class(class_id) | FK_Assignment_Class_class_id | class_id=N | Khớp endpoints; NO ACTION delete/update |
| 28 | StudentClassAssignment(registration_id) → Registration(registration_id) | FK_Assignment_Registration_registration_id | registration_id=N | Khớp endpoints; NO ACTION delete/update |
| 29 | StudentClassAssignment(assigned_by) → Account(account_id) | FK_Assignment_Account_assigned_by | assigned_by=N | Khớp endpoints; NO ACTION delete/update |
| 30 | Schedule(class_id) → Class(class_id) | FK_Schedule_Class_class_id | class_id=N | Khớp endpoints; NO ACTION delete/update |
| 31 | Schedule(replaces_schedule_id) → Schedule(schedule_id) | FK_Schedule_Schedule_replaces_schedule_id | replaces_schedule_id=Y | Khớp endpoints; NO ACTION delete/update |
| 32 | Schedule(held_by) → Account(account_id) | FK_Schedule_Account_held_by | held_by=Y | Khớp endpoints; NO ACTION delete/update |
| 33 | Schedule(not_held_by) → Account(account_id) | FK_Schedule_Account_not_held_by | not_held_by=Y | Khớp endpoints; NO ACTION delete/update |
| 34 | Attendance(schedule_id) → Schedule(schedule_id) | FK_Attendance_Schedule_schedule_id | schedule_id=N | Khớp endpoints; NO ACTION delete/update |
| 35 | Attendance(class_assignment_id) → StudentClassAssignment(class_assignment_id) | FK_Attendance_StudentClassAssignment_class_assignment_id | class_assignment_id=N | Khớp endpoints; NO ACTION delete/update |
| 36 | Attendance(entered_by) → Account(account_id) | FK_Attendance_Account_entered_by | entered_by=N | Khớp endpoints; NO ACTION delete/update |
| 37 | Score(class_assignment_id) → StudentClassAssignment(class_assignment_id) | FK_Score_StudentClassAssignment_class_assignment_id | class_assignment_id=N | Khớp endpoints; NO ACTION delete/update |
| 38 | Score(component_id) → ScoreComponentConfig(component_id) | FK_Score_ScoreComponentConfig_component_id | component_id=N | Khớp endpoints; NO ACTION delete/update |
| 39 | Score(entered_by) → Account(account_id) | FK_Score_Account_entered_by | entered_by=N | Khớp endpoints; NO ACTION delete/update |
| 40 | Score(schedule_id) → Schedule(schedule_id) | FK_Score_Schedule_schedule_id | schedule_id=Y | Khớp endpoints; NO ACTION delete/update |
| 41 | GradeReport(class_assignment_id) → StudentClassAssignment(class_assignment_id) | FK_GradeReport_StudentClassAssignment_class_assignment_id | class_assignment_id=N | Khớp endpoints; NO ACTION delete/update |
| 42 | GradeReport(submitted_by) → Account(account_id) | FK_GradeReport_Account_submitted_by | submitted_by=Y | Khớp endpoints; NO ACTION delete/update |
| 43 | GradeReport(reviewed_by) → Account(account_id) | FK_GradeReport_Account_reviewed_by | reviewed_by=Y | Khớp endpoints; NO ACTION delete/update |
| 44 | Invoice(registration_id) → Registration(registration_id) | FK_Invoice_Registration_registration_id | registration_id=N | Khớp endpoints; NO ACTION delete/update |
| 45 | Invoice(cancelled_by) → Account(account_id) | FK_Invoice_Account_cancelled_by | cancelled_by=Y | Khớp endpoints; NO ACTION delete/update |
| 46 | Payment(invoice_id) → Invoice(invoice_id) | FK_Payment_Invoice_invoice_id | invoice_id=N | Khớp endpoints; NO ACTION delete/update |
| 47 | Payment(confirmed_by) → Account(account_id) | FK_Payment_Account_confirmed_by | confirmed_by=N | Khớp endpoints; NO ACTION delete/update |
| 48 | Payment(last_corrected_by) → Account(account_id) | FK_Payment_Account_last_corrected_by | last_corrected_by=Y | Khớp endpoints; NO ACTION delete/update |
| 49 | AuditLog(actor_account_id) → Account(account_id) | FK_AuditLog_Account_actor_account_id | actor_account_id=Y | Khớp endpoints; NO ACTION delete/update |

## 3. Quan hệ phải bổ sung

| Quan hệ | Tên constraint | NULL | Delete/update |
| --- | --- | --- | --- |
| ReplayOperation(actor_account_id) → Account(account_id) | FK_ReplayOperation_Account_actor_account_id | N | NO ACTION / NO ACTION |

InvoiceNumberCounter không có FK sang Invoice. Không thêm giả FK AuditLog(entity_id), ReplayOperation(result_reference) hoặc các operation_id sang một bảng business để làm đủ số quan hệ.

## 4. Hai FK ghép cần ghi/vẽ đúng

1. EntryTestAttempt(completed_booking_id,attempt_id) → EntryTestBooking(booking_id,attempt_id): completed_booking_id nullable, attempt_id bắt buộc. AK_Booking_IdAttempt là principal key; CK result bundle buộc link có cùng score/by/time. FK chứng minh cùng lượt, **không** chứng minh Booking.status COMPLETED: service kiểm điều đó.
2. BandAssignment(best_attempt_id,registration_id) → EntryTestAttempt(attempt_id,registration_id): cả hai bắt buộc. AK_Attempt_IdRegistration chặn chọn Attempt của hồ sơ khác. Best completed/highest/tie-break/threshold là service/FN02, không chỉ FK.

Không vẽ thêm scalar FK trùng composite chỉ để tăng số constraint. Vòng Attempt↔Booking được tạo theo thứ tự Attempt chưa kết quả → Booking → result, không tự cascade insert hai row.

## 5. Bảng tầm ảnh hưởng Thêm/Xóa/Sửa

Quy ước bảng con/cha trong báo cáo vẫn dùng được nếu cập nhật tên cột theo danh mục trên:

- INSERT/UPDATE FK phía con: kiểm parent hoặc link ghép tồn tại; nullable đi kèm CHECK bundle tương ứng.
- DELETE/UPDATE referenced key phía cha: NO ACTION chặn nếu còn tham chiếu; không cascade hoặc SET NULL làm mất lịch sử.
- DELETE row con không bị chính FK đó chặn, nhưng có thể bị FK khác trỏ vào, trigger/permission hoặc service policy; dấu “−” không đồng nghĩa ứng dụng có nút xóa.
- Mọi Account actor FK chỉ chứng minh Account tồn tại. Role ACTIVE/quyền/current task/ownership phải kiểm theo thao tác; không đặt CHECK buộc actor lịch sử luôn ACTIVE vì sau này tài khoản có thể bị khóa.

## 6. Các ghi chú phải thống nhất với phần khác

Danh mục FK của báo cáo đã dùng Booking/Registration và các trường reviewer mới, trong khi hình 6.4 còn session_id/student_id/approved_by cũ. Lấy danh mục FK này và source column ở 6.2 làm chuẩn để vẽ lại hình. actor nullable của AuditLog là ngữ cảnh chưa xác thực; operational retention security/IP không phải ứng dụng được purge mandatory history.


Nguồn dùng chung và phương pháp: [README](README.md).
