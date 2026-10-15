# 6.3.3 — Đối chiếu ràng buộc liên bộ trong cùng quan hệ

Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

## 1. Kết luận

Phần lớn uniqueness trong báo cáo khớp nhưng chưa đủ filtered unique/AK thực tế. Hai dòng chống chồng lịch và dòng tổng weight 100 đang đặt lẫn với UNIQUE; chúng là invariant/guard khác. Best_attempt_id không có một UNIQUE index riêng dù cardinality tương ứng được suy ra từ các ràng buộc phối hợp.

## 2. Đối chiếu từng dòng của phần 6.3.3

| Báo cáo | Đối chiếu/cách sửa |
| --- | --- |
| Role.role_code. | Khớp UQ_Role_Code; literal role_code sửa ở 6.3.1. |
| Account.email. | Khớp UQ_Account_Email, CI collation + normalization, kể cả LOCKED/DISABLED. |
| Account.phone khi có giá trị. | Khớp filtered UQ_Account_Phone WHERE phone IS NOT NULL. |
| StudentProfile.account_id. | Khớp UQ_StudentProfile_Account; service thêm rule chỉ Student. |
| Course.course_code. | Khớp UQ_Course_Code; bất biến không tái dùng. |
| Band.band_code. | Khớp UQ_Band_Code. |
| Band.min_score. | Khớp UQ_Band_MinScore. |
| Class.class_code. | Khớp UQ_Class_Code, kể cả CANCELLED. |
| Invoice.invoice_number. | Khớp UQ_Invoice_Number; allocator không dùng invoice_id làm số. |
| TrainingProgram: cặp (course_id, band_id) phải duy nhất. Một khóa học chỉ có một chương trình hiện hành cho mỗi Band. | Khớp UQ_Program_CourseBand trên mọi trạng thái. Không version/new Program cùng cặp để né INACTIVE/freeze. |
| ScoreComponentConfig: cặp (program_id, component_name) phải duy nhất. Tổng weight của các thành phần thuộc cùng một chương trình phải bằng 100 trước khi chương trình chuyển sang ACTIVE. | Tên component unique trong Program bằng UQ_Component_ProgramName. Tổng weight 100 thuộc activation transaction, không cùng loại UNIQUE; thêm filtered unique tối đa1 FINAL_EXAM. |
| EntryTestAttempt: cặp (registration_id, attempt_number) phải duy nhất. Nếu tồn tại lượt số 2 thì phải tồn tại lượt số 1 của cùng hồ sơ. | Khớp UQ_Attempt_RegistrationNumber + CHECK number1/2; lượt 2 cần lượt 1 completed score và trước CONFIRMED ở service, không chỉ tồn tại row1. |
| BandAssignment.registration_id phải duy nhất. Mỗi hồ sơ chỉ có một kết quả Band hiện hành. | Khớp UQ_BandAssignment_Registration. |
| BandAssignment.best_attempt_id phải duy nhất. Một lượt thi không được dùng làm lượt tốt nhất của nhiều hồ sơ khác nhau. | Không có standalone UQ best_attempt_id. Tính duy nhất về nghĩa được suy từ FK (best_attempt_id,registration_id) + unique registration_id; không thêm index giả vào danh mục. |
| StudentClassAssignment.registration_id phải duy nhất vì phạm vi hiện tại không hỗ trợ chuyển lớp hoặc xếp một hồ sơ vào nhiều lớp. | Khớp UQ_Assignment_Registration; không transfer/delete assignment qua UI. |
| Attendance: cặp (schedule_id, class_assignment_id) phải duy nhất. Một học viên chỉ được điểm danh một lần trong một buổi học. | Khớp UQ_Attendance_ScheduleAssignment; thiếu record khác Vắng. |
| Score: cặp (class_assignment_id, component_id) phải duy nhất. Một học viên chỉ có một điểm hiện hành cho mỗi thành phần. | Khớp UQ_Score_AssignmentComponent; component/link guards khác UNIQUE. |
| GradeReport.class_assignment_id phải duy nhất. Mỗi học viên trong một lớp chỉ có một bảng điểm tổng hợp. | Khớp UQ_GradeReport_Assignment; service placement còn tạo report DRAFT cùng commit. |
| Invoice.registration_id phải duy nhất. Mỗi hồ sơ được duyệt chỉ có một hóa đơn. | Khớp UQ_Invoice_Registration; issuance cùng admission approval. |
| Payment.invoice_id phải duy nhất. Mỗi hóa đơn chỉ được thanh toán một lần. | Khớp UQ_Payment_Invoice; đủ amount/fullpayment ở service transaction. |
| Các lịch thuộc Schedule không được chồng lấn khi có cùng phòng, cùng lớp hoặc cùng giáo viên. | Đúng hướng nghiệp vụ nhưng không là UNIQUE đơn bảng; kiểm khoảng [start,end), resource/gates và dữ liệu liên bảng trong transaction. Bổ sung Student và test-versus-class overlap. |
| Các ca thi thuộc EntryTestSession không được chồng lấn khi có cùng phòng hoặc cùng giáo viên chấm. | Đúng hướng nghiệp vụ nhưng không là UNIQUE đơn bảng; kiểm khoảng [start,end), resource/gates và dữ liệu liên bảng trong transaction. Bổ sung Student và test-versus-class overlap. |

## 3. Danh mục unique indexes hiện tại (ngoài PK/AK)

| Bảng | Index | Cột | Filter |
| --- | --- | --- | --- |
| Role | UQ_Role_Code | role_code | Không filtered |
| Account | UQ_Account_Email | email | Không filtered |
| Account | UQ_Account_Phone | phone | ([phone] IS NOT NULL) |
| StudentProfile | UQ_StudentProfile_Account | account_id | Không filtered |
| Course | UQ_Course_Code | course_code | Không filtered |
| Band | UQ_Band_Code | band_code | Không filtered |
| Band | UQ_Band_MinScore | min_score | Không filtered |
| TrainingProgram | UQ_Program_CourseBand | course_id, band_id | Không filtered |
| ScoreComponentConfig | UQ_Component_FinalExam | program_id | ([kind]='FINAL_EXAM') |
| ScoreComponentConfig | UQ_Component_ProgramName | program_id, component_name | Không filtered |
| Registration | UQ_Registration_Unfinished | student_id, course_id | ([is_unfinished]=(1)) |
| EntryTestAttempt | UQ_Attempt_CompletedBooking | completed_booking_id | ([completed_booking_id] IS NOT NULL) |
| EntryTestAttempt | UQ_Attempt_RegistrationNumber | registration_id, attempt_number | Không filtered |
| EntryTestBooking | UQ_Booking_Completed | attempt_id | ([status]='COMPLETED') |
| EntryTestBooking | UQ_Booking_Current | attempt_id | ([status]='SCHEDULED') |
| BandAssignment | UQ_BandAssignment_Registration | registration_id | Không filtered |
| Class | UQ_Class_Code | class_code | Không filtered |
| StudentClassAssignment | UQ_Assignment_Registration | registration_id | Không filtered |
| Schedule | UQ_Schedule_CurrentFinalExam | class_id | ([is_current]=(1) AND [session_type]='FINAL_EXAM') |
| Schedule | UQ_Schedule_CurrentObligation | class_id, obligation_key | ([is_current]=(1)) |
| Schedule | UQ_Schedule_Predecessor | replaces_schedule_id | ([replaces_schedule_id] IS NOT NULL) |
| Attendance | UQ_Attendance_ScheduleAssignment | schedule_id, class_assignment_id | Không filtered |
| Score | UQ_Score_AssignmentComponent | class_assignment_id, component_id | Không filtered |
| GradeReport | UQ_GradeReport_Assignment | class_assignment_id | Không filtered |
| Invoice | UQ_Invoice_Number | invoice_number | Không filtered |
| Invoice | UQ_Invoice_Registration | registration_id | Không filtered |
| Payment | UQ_Payment_Invoice | invoice_id | Không filtered |
| ReplayOperation | UQ_Replay_ActorActionRequest | actor_account_id, action_code, request_id | Không filtered |

## 4. Alternate keys cho hai FK ghép

| Bảng | AK | Cột | Lý do |
| --- | --- | --- | --- |
| EntryTestAttempt | AK_Attempt_IdRegistration | attempt_id, registration_id | Principal key cho best_attempt_id/registration_id |
| EntryTestBooking | AK_Booking_IdAttempt | booking_id, attempt_id | Principal key cho completed_booking_id/attempt_id |

## 5. Những UNIQUE không nên tự bổ sung

- Không permanent unique Registration(student_id,course_id): sẽ chặn học lại sau lượt trước đã kết thúc. Dùng filtered is_unfinished=1 hiện tại.
- Payment.transaction_reference không unique; mã giao dịch/tiền mặt có hợp đồng validation và correction riêng.
- Không unique Schedule(class_id,session_type) cho toàn history: có nhiều LESSON và các replacement. Dùng current obligation/current exam/predecessor đúng filter.
- Không unique score_value, BandAssignment.best_attempt_id riêng hoặc bảng StudentClassAssignment(student_id) khi không có cột student_id.
- Capacity/max_size và overlaps không thể thay bằng unique indexes đã liệt kê. Kiểm trước read chưa đủ chống hai lệnh tranh chỗ cuối.

Ràng buộc SQL bảo vệ cấu trúc, service bảo vệ trạng thái và thời điểm; không mô tả index như tác nhân tự chọn ca/lớp khi conflict.


Nguồn dùng chung và phương pháp: [README](README.md).
