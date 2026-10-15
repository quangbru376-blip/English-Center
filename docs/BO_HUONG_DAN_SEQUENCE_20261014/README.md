# Bộ hướng dẫn vẽ sequence theo dự án — 14/10/2026

Ghi chú đóng gói15/10/2026: hình SVG/PNG, viewer và script `SQ03_A_Dang_Nhap`
là bản minh họa đang biên soạn, chưa nghiệm thu nội dung nghiệp vụ. Đoạn cuối
hình/script hiện ghi cấp phiên thường sau đổi mật khẩu tạm; cần sửa trước khi
dùng làm hình chính thức: API004/ADR049/098 thu hồi các phiên và yêu cầu đăng
nhập lại, không nâng phiên hạn chế thành phiên thường. Đối chiếu
[SQ04](SQ04_PASSWORD_CHANGE_GUIDE.md) và mã thực tế. Lượt đồng bộ này giữ nguyên
nội dung các hình/script để nhóm tiếp tục chỉnh, không thay hợp đồng sản phẩm.

Mỗi mã SQ01–SQ25 có **một file Markdown độc lập**. Các thao tác khác actor/điểm bắt đầu/đơn vị giao dịch tách thành hình A/B/C trong cùng file. Không cần vẽ chung tất cả nhánh vào một hình lớn.

Bộ này lấy SQ01_REGISTRATION_GUIDE.md làm mẫu: mục tiêu và ranh giới → lifeline EA → dữ liệu/guard → bảng thông điệp và đường gạch → note → cách dựng → checklist. Chín hướng dẫn đã có được giữ phần thông điệp chi tiết, cập nhật nguồn/ngoại lệ hiện hành trong bản sao mới; mười sáu hướng dẫn còn lại được viết riêng theo các workflow và code đã triển khai. File hướng dẫn cũ và báo cáo Word không thay đổi.

## Danh mục

| Mã | Hướng dẫn |
| --- | --- |
| SQ01 | [Hướng dẫn vẽ sequence tự đăng ký tài khoản học viên](SQ01_REGISTRATION_GUIDE.md) |
| SQ02 | [Admin cấp tài khoản nhân sự](SQ02_STAFF_ACCOUNT_CREATION_GUIDE.md) |
| SQ03 | [Hướng dẫn vẽ sequence đăng nhập và đăng xuất](SQ03_LOGIN_LOGOUT_GUIDE.md) |
| SQ04 | [Hướng dẫn vẽ sequence tự đổi mật khẩu](SQ04_PASSWORD_CHANGE_GUIDE.md) |
| SQ05 | [Hướng dẫn vẽ sequence Admin quản lý tài khoản](SQ05_ACCOUNT_ADMINISTRATION_GUIDE.md) |
| SQ06 | [Hồ sơ cá nhân và trang chủ Student](SQ06_STUDENT_PROFILE_HOME_GUIDE.md) |
| SQ07 | [Hướng dẫn vẽ sequence đăng ký khóa học và hủy hồ sơ](SQ07_COURSE_REGISTRATION_GUIDE.md) |
| SQ08 | [Giáo vụ quản lý ca thi đầu vào](SQ08_ENTRY_SESSION_MANAGEMENT_GUIDE.md) |
| SQ09 | [Student đặt, đổi, hủy và đặt lại ca thi](SQ09_ENTRY_TEST_BOOKING_GUIDE.md) |
| SQ10 | [Teacher chấm, ghi vắng, sửa điểm và tính Band](SQ10_ENTRY_SCORE_BAND_GUIDE.md) |
| SQ11 | [Hướng dẫn vẽ sequence học viên xác nhận nhập học](SQ11_ENROLLMENT_CONFIRMATION_GUIDE.md) |
| SQ12 | [Tuyển sinh duyệt, từ chối hoặc hủy hồ sơ](SQ12_ADMISSIONS_DECISION_GUIDE.md) |
| SQ13 | [Hướng dẫn vẽ sequence tra cứu và in hóa đơn](SQ13_INVOICE_LOOKUP_PRINT_GUIDE.md) |
| SQ14 | [Kế toán xác nhận thu tiền và sửa metadata](SQ14_PAYMENT_CONFIRMATION_GUIDE.md) |
| SQ15 | [Hướng dẫn vẽ sequence quản lý khóa và cấu hình đào tạo](SQ15_TRAINING_CONFIGURATION_GUIDE.md) |
| SQ16 | [Giáo vụ quản lý lớp và giáo viên lớp](SQ16_CLASS_LIFECYCLE_GUIDE.md) |
| SQ17 | [Giáo vụ xếp học viên vào lớp](SQ17_STUDENT_CLASS_PLACEMENT_GUIDE.md) |
| SQ18 | [Lập lịch, dời/thay buổi, xác nhận HELD và xem lịch](SQ18_CLASS_SCHEDULE_GUIDE.md) |
| SQ19 | [Teacher lưu điểm danh một LESSON](SQ19_LESSON_ATTENDANCE_GUIDE.md) |
| SQ20 | [Teacher lưu điểm nháp và nhận xét một học viên](SQ20_STUDENT_GRADE_DRAFT_GUIDE.md) |
| SQ21 | [Teacher gửi và gửi lại bảng điểm cả lớp](SQ21_CLASS_GRADE_SUBMISSION_GUIDE.md) |
| SQ22 | [Giáo vụ trả lại/chốt và Student xem điểm đã công bố](SQ22_CLASS_GRADE_REVIEW_FINALIZATION_GUIDE.md) |
| SQ23 | [Hướng dẫn vẽ sequence báo cáo tài chính và xuất CSV](SQ23_FINANCIAL_REPORT_CSV_GUIDE.md) |
| SQ24 | [Giám đốc xem ba báo cáo tổng hợp](SQ24_DIRECTOR_AGGREGATE_REPORTS_GUIDE.md) |
| SQ25 | [Admin tra cứu nhật ký và nhóm cùng thao tác](SQ25_AUDIT_VIEWER_GUIDE.md) |

## Cách dùng

1. Mở file SQ cần vẽ, đọc mục tiêu/guard trước khi kéo lifeline.
2. Với file có nhiều hình, tạo từng Sequence Diagram SQxx-A/B/C; đặt đúng actor của hình đó. Một vai trò được nhắc trong dữ liệu không nhất thiết là actor tham gia.
3. Đặt Actor → Boundary → Control từ trái sang phải. Dùng self-message ở Control để chỉ kiểm/hiệu ứng; return nét đứt; activation kết thúc tại phản hồi của từng nhánh.
4. Các mã A01/B01/M01 không là API; thứ tự trên bảng và vị trí fragment có thẩm quyền. Nhánh lỗi và thành công loại trừ nhau, không vẽ nối tiếp hết mọi mã.
5. Phác thảo đường gạch giúp bố cục; mỗi hình vẫn lấy **nhãn cụ thể ở bảng thông điệp**, không dùng tất cả mũi tên chung như “Xử lý”. Với form cục bộ không gọi server chỉ để dựng form; với read không thêm read-context rồi lại query nếu code chỉ có một request.
6. Note/ref dùng cho cơ chế chung; không thêm lifeline DB/controller/service trừ khi chủ động vẽ một sơ đồ kiến trúc khác mức.

## Cơ chế chung phải giữ

### Phiên và quyền

Backend kiểm quyền/ownership/task hiện tại; UI capability hoặc preview không là cấp quyền. Student/staff dùng NORMAL trừ PASSWORD_CHANGE gate chỉ đổi mật khẩu/logout. NORMAL tối đa 2 giờ/idle 30 phút; restricted tối đa 10 phút không vượt hạn mật khẩu tạm.

Đến ngày 14/10, focus cửa sổ đơn thuần và visibility event lặp cùng trạng thái không kiểm phiên. Hidden pause/xóa RAM; hidden→visible kiểm me/CSRF mới. Startup/deadline/BroadcastChannel/Back-Forward/manual và 401 giữ cơ chế hiện tại. Không polling hoặc keep-alive để kéo dài idle. Hết quyền/phân công thì không trả dữ liệu cũ như đã xác minh.

### Preview, giao dịch và no-change

Preview/read không giữ chỗ/tài nguyên/đồng hồ; recheck lúc lưu. Backend kiểm supplied versions/absence/full membership và quan hệ trước no-change; không dùng only-changed subset để bỏ qua item supplied stale. New intent toàn business values giống bị từ chối, không marker/audit mutation mới. Reset mật khẩu chủ động là ngoại lệ thao tác thật theo policy.

Phạm vi no-change/replay trên áp dụng các command có cơ chế đó. **Profile SQ06 có hợp đồng riêng**: kiểm hai RV, audit changedFields; không replay completion/receipt, không nhánh backend reject-all-equal, không revoke-all. Không gắn quy tắc chung vào API Profile hoặc credential.

Các đơn vị lưu khác nhau: Attendance một LESSON/tập supplied changes; draft score/comment một Student; submit/return/finalize toàn Class; weekly lịch toàn batch. Loop xử lý items bên trong **một** đơn vị nguyên tử không là gọi/commit từng item độc lập. Không commit một phần nếu một item không hợp lệ.

### Audit đúng loại

Mandatory audit thuộc đúng danh mục action, lưu cùng mutation và rollback khi lỗi. Login/logout/security-only best-effort riêng. **Attendance hiện tại không có AuditLog lịch sử nghiệp vụ riêng**, giữ latest enteredBy/enteredAt/replay; không copy audit của Grade. Read/search/report/print/export không tự audit mọi page-view. Không secrets/raw payload trong log.

### Mất phản hồi và replay

Mất phản hồi sau gửi lệnh khác lỗi rollback đã được xác định. Không tự gửi lại hoặc đổi request key để thử lại mù. Tra cứu state/operation theo scope/họ endpoint hiện hành; nếu cần vẽ chi tiết dùng ref khôi phục thủ công. Cùng key/content hoàn tất không sinh hiệu ứng lần hai; quyền hiện tại vẫn kiểm. Không bịa lookup endpoint chung. Không replay plaintext credential, không xem lại mật khẩu tạm từ log/receipt. SQ01 không có lookup account công khai để kiểm secret.

### Read và khởi tạo màn hình

UI hiện tại có scoped initial reads. Khi hình bắt đầu bằng mở màn hình, có thể gửi query bộ lọc mặc định ngay; không vẽ click Áp dụng giả. Các POST search vẫn là read. Chỉnh filter chưa áp dụng không thay context kết quả cũ. Response cũ bị loại theo request/epoch; không biến cancellation read thành rollback mutation.

## Những ranh giới dễ vẽ sai

- Student xác nhận nhập học chưa tạo Invoice. Admissions duyệt mới ADMITTED + Invoice UNPAID nguyên tử; payment không tạo Invoice.
- Phương thức tiền trong code: CASH/TRANSFER, không BANK_TRANSFER; dùng nhãn Tiền mặt/Chuyển khoản nếu vẽ tiếng Việt.
- ABSENT không là score 0; Booking history khác Attempt, tối đa 2 Attempt không giới hạn số lịch hẹn bằng 2.
- Program first-Class freeze không được giải khi Class CANCELLED; Band reference-only không UI editor.
- HELD do Giáo vụ sau end; Attendance/time không tự HELD. Class COMPLETED chưa grade FINALIZED.
- Draft total tính khi đọc, final_score chỉ lưu khi chốt; Student chỉ own FINALIZED.
- Director aggregate-only không CSV/phê duyệt; Accountant CSV query mới toàn filtered set, 6 cột + footer, 10.000 data records gồm footer/5.000.000 byte.
- 7 roles cố định, 37 NV còn hiệu lực (trừ NV10/NV11), 32 F (trừ F15), 21 W. NV39 xuyên các action có audit, không actor thứ tám.

## Nguồn và phạm vi kiểm tra

[Danh mục cập nhật](../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md), [FUNCTION_SPEC](../design/FUNCTION_SPEC.md), [WORKFLOW_SPEC](../design/WORKFLOW_SPEC.md), [requirements](../requirements.md), các hợp đồng E04 và code từng module được dẫn trong file.

M0–M3 và phần reports/CSV/audit viewer M4 đã triển khai trên main/host LOCAL_DEV tường minh. UI refresh đã áp dụng, owner gate4 còn pending; backup/restore M4 cần đầu vào vận hành, không là sequence đã hoàn thành trong bộ này. Hướng dẫn là tài liệu vẽ, không tuyên bố chạy lại bộ kiểm thử sản phẩm.
