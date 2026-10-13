# UI refresh — inventory và bản đồ tương tác baseline

Baseline source HEAD26067e0; frontend tree b2fb22a757468e0f29aa6adc8269cec48c3e24bc.
Ngày13/10/2026. Kiểm kê từ source thực tế, không suy mỗi F là một route/file.
Routes: `frontend/src/app/IdentityApp.tsx`, guards `identityNavigation.ts`;
entry production `main.tsx -> App -> IdentityApp`. Shell `IdentityShell.tsx`.
Root dưới đây đều tương đối với `frontend/src/features/`.

## Phạm vi32F

| F | Vai trò | Route / view thực tế | Component chính | Dạng / thao tác |
|---|---|---|---|---|
| F01 | Anonymous/all | /login | identity/pages/LoginPage + LoginForm | Form/login/session recovery |
| F02 | Anonymous | /register | identity/pages/SignupPage + SignupForm | Form/signup, không auto-login |
| F03 | Student | /student/home, view home | enrollment/StudentEnrollmentPage | Thẻ từng Registration/loadHome/page |
| F04 | Student | /student/home, view courses | enrollment/StudentEnrollmentPage | Catalogue/list/readCourses |
| F05 | Student | /student/home, registration local state | enrollment/StudentEnrollmentPage | Create/own detail/cancel preview |
| F06 | Student | registration/options/moving local state | enrollment/StudentEnrollmentPage | Book/rebook/reschedule/cancel dialog |
| F07 | Student | /student/home, view calendar | enrollment/StudentEnrollmentPage | Booking agenda/current-history/date filters |
| F08 | Student | registration.entry local state | enrollment/StudentEnrollmentPage | Own attempts/results/Band, missing khác0 |
| F09 | Student | registration/confirmation preview | enrollment/StudentEnrollmentPage | Confirm all-or-none; unknown lookup |
| F10 | Student | /student/invoices, /:id, /:id/print | invoices/StudentInvoicesPage | List/detail/fresh browser print |
| F11 | Student | /student/classes | classes/ClassesPage + SchedulesPanel | Own classes/obligation calendar/history |
| F12 | Student | /student/grade-reports, /:id | classes/StudentGradesPage via GradePanels | List/detail own FINALIZED only |
| F13 | Admissions | /admissions/registrations | admissions/AdmissionsPage | POST PII search/list/filters/page |
| F14 | Admissions | /admissions/registrations/:id + local detail | admissions/AdmissionsPage + AdmissionReasonForm | Approve/reject/cancel/previews |
| F15 | Removed | Không triển khai | Không component | Không ghi danh thay Student |
| F16 | AcademicAffairs | /academics/entry-test-sessions | entrySessions/pages/EntrySessionPage | Session search/list/options |
| F17 | AcademicAffairs | cùng route, selected session/dialogs | EntrySessionPage + EntrySessionForm | Create/edit/open/close/grader/cancel/roster |
| F18 | AcademicAffairs | /academics/courses, /academics/training-programs | catalogue/pages/CataloguePage; programs/pages/ProgramPage | Course/Band/program/components/lifecycle |
| F19 | AcademicAffairs | /academics/classes | classes/ClassesPage | List/class details/lifecycle |
| F20 | AcademicAffairs | cùng route, create/detail/preview state | ClassesPage + components/ClassFields | Create/capacity/teacher/open/cancel/start/complete |
| F21 | AcademicAffairs | cùng route, schedule panels | classes/SchedulesPanel + WeeklyFields | Calendar/individual/weekly/move/replace/HELD |
| F22 | AcademicAffairs | cùng route, placement preview state | classes/ClassesPage | Eligible admissions/one Registration placement |
| F23 | AcademicAffairs | /academics/grade-reviews | classes/GradeReviewPage via GradePanels | Non-DRAFT review/return/finalize whole cohort |
| F24 | Teacher | /teacher/classes | classes/ClassesPage + SchedulesPanel | Current task/classes/calendar |
| F25 | Teacher | /teacher/entry-test-sessions | entryTasks/TeacherEntryPage + EntryScoreForm | Roster/detail/score/absence/correction |
| F26 | Teacher; Academic readonly | /teacher/classes/:id/attendance, /lessons/:sid/attendance; embedded Academic class | classes/AttendancePanel + AttendanceFields | LESSON deltas/current task/read-only |
| F27 | Teacher | /teacher/classes/:id/gradebook, /assignments/:id/grade-draft | classes/GradebookPanel + GradeDraftFields | Per-Student draft/whole-class submit/resubmit |
| F28 | Accountant | /accountant/invoices, /:id | finance/FinancePage | Search/detail/Invoice-Payment |
| F29 | Accountant | /accountant/invoices/:id/collect, /accountant/payments/:id/edit | FinancePage + PaymentFields | Collect/payment metadata preview/confirm |
| F30 | Accountant | /accountant/reports | reports/ReportsPage via ReportPanels | Finance/applied filters/fresh whole CSV |
| F31 | Director | /reports; local finance/admissions/training tabs | reports/ReportsPage via ReportPanels | Aggregate-only/no export/dual training pages |
| F32 | Admin | /admin/accounts, /:id | administration/pages/StaffAdminPage | Search/detail/create/reset/status/secret handoff |
| F33 | Admin | /admin/audit-events, /:id, /admin/operations/:uuid/audit-events | audit/AuditPage via ReportPanels | Event/detail/group/pagination/no export |

Account actions ngoài mã F: `/account/change-password` cho NORMAL/all7roles và
PASSWORD_CHANGE scope; `identity/pages/PasswordChangePage`. Profile Student là
view profile trong StudentEnrollmentPage. Không thêm F hoặc staff profile edit.

## States và component nền

Source shared: `lib/useOwnedTask.ts`, `lib/http.ts`, `lib/auth.ts`,
`lib/requestScope.ts`, identity/sessionController/signals, `components/ui/`.
Đã có Button/TextField/SelectField/Feedback/Modal/PageHeader/LocalCalendar.
Chưa có hệ component Card/Badge/Table/Pagination/Skeleton/Avatar thống nhất.
Reports/grades/attendance dùng lazy panels và Suspense loading. Các feature có
task.pending/task.notice, dữ liệu null trước đọc, empty theo response, form
validation và capability/disabled; cách trình bày không đồng đều. Một số màn
loading chỉ disabled, không skeleton. Lỗi validation có nơi dùng notice toàn
form thay lỗi từng field. Giai đoạn2 phải chuẩn hóa phần trình bày, giữ nguyên
điều kiện kiểm tra và error contract.

Mutation modules có unknown/manual completion controls; auth/signup/password
có recovery riêng và raw-secret cleanup. Không xóa các controls vì trông kỹ thuật.
Các screenshots chỉ ghi states mà harness thực sự đi qua; không coi trạng thái
chưa chụp là đã kiểm UI. Xem manifest ảnh và bảng coverage trong evidence.

## Bản đồ tương tác đề xuất cho cổng1

| Hiện tại / bằng chứng source | Đề xuất được owner cho phép | Điều cần giữ |
|---|---|---|
| IdentityApp có nav Student4nút; EnrollmentPage có nav6nút | Một nav chính, account menu riêng | Invoice/classes/results URLs và view callbacks hiện có |
| “Hồ sơ và lịch thi” mở /student/home; “Lịch thi” setView(calendar) | “Trang chủ”, “Khóa học”, “Hồ sơ & thi đầu vào”, “Lớp & lịch”, “Hóa đơn”, “Kết quả”; drawer mobile | Hồ sơ khác calendar; không gộp mất booking/results/confirmation |
| readHome không chạy khi mount; home ban đầu null | Initial loadHome có scope khi mở home; nút “Tải lại” phụ | Không lặp theo render, không polling, pending/epoch/cleanup |
| courses/profile/calendar cũng tải qua nút/form | Initial read courses/profile khi mở; calendar initial query hợp lệ, Apply khi sửa filter | Không bypass validated filters/profileUnknown/unsaved draft |
| “Kiểm tra lại phiên” gọi checkSession: clear RAM -> me -> CSRF | “Thử xác minh lại phiên” trong vùng recovery, không thường trực ở form đang bình thường | Unknown/401/restricted/CSRF, không resend/renew session |
| password/logout nằm lặp trong feature pages | Một account menu ở shell; giữ callbacks và quyền | Logout chỉ current SID; password change clear all own sessions |
| Child nav chủ yếu local state, nhiều detail cùng route | Cho nav shell chọn view qua callback hiện có | Không thêm route/query chứa PII, không làm mới form mất unsaved guard |

Menu là đề xuất cụ thể để owner duyệt tại cổng1, chưa sửa handler hoặc DOM.
Không thay labels/id/data-testid phục vụ binding hoặc selector bằng cách mù quáng.

## Rủi ro cần xử lý ở các giai đoạn sau

1. Tự tải khi remount/focus có thể phát nhiều read: giữ one-request/epoch và auth
   boundary, không effect loop hoặc tự renew idle.
2. Nav trong shell phải tôn trọng unknown/dirty state và quyền, đặc biệt Profile,
   Attendance/grade drafts; không reset sensitive RAM ngoài boundary đã xác định.
3. Tiền có thể38digits: formatting chuỗi, không Number/Recharts conversion.
4. SVG/font/license và contrast token là việc giai đoạn2, chưa verified.
5. Cần giữ before data fixture/source/state mapping để so after cùng tình huống;
   không yêu cầu business ID/time pixel-identical qua các lần tạo scratch.
6. Existing credential paragraph aria-label Mật khẩu tạm và password inputs
   phải bị mask khi screenshot; không lưu JWT/CSRF/raw API payload.
7. Không thêm biểu đồ/phụ huynh/route/schema để bù dữ liệu không có trong API.
