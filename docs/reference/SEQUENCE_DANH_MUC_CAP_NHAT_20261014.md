# Danh mục sequence cập nhật theo dự án

Ngày đối chiếu: 14/10/2026. Tài liệu riêng phục vụ cập nhật phần sequence của báo cáo; không sửa `Nhom18_ThietKeXuLy.docx`.

Danh mục giữ SQ01–SQ25 của `docs/design/SEQUENCE_PLAN.md` để tiếp tục sử dụng các hướng dẫn vẽ đã có. Đây là **25 nhóm luồng**, không phải cam kết chỉ có 25 hình: các thao tác có điểm bắt đầu, quyền hoặc giao dịch khác nhau nên tách hình con. Tài liệu này cập nhật phạm vi và quy tắc; chưa sinh hình sequence.

## 1. Phần sequence hiện có trong báo cáo

Đọc trực tiếp phần cuối báo cáo: có 10 hình và 10 chú thích sau. Bảng dưới đối chiếu tên/phạm vi cần thay thế; không đánh giá từng thông điệp bên trong ảnh cũ.

| STT | Sequence trong báo cáo | Đưa vào danh mục mới | Phạm vi cần thể hiện khi cập nhật |
| --- | --- | --- | --- |
| 1 | Học viên đăng ký tài khoản | SQ01 | Student tự đăng ký; Account + StudentProfile + audit bắt buộc trong cùng giao dịch. |
| 2 | Đăng nhập | SQ03a | Kiểm tra tài khoản, mật khẩu, giới hạn đăng nhập; phiên NORMAL hoặc PASSWORD_CHANGE. |
| 3 | Đăng xuất | SQ03b | Thu hồi phiên hiện tại; không mô tả là chỉ xóa dữ liệu phía trình duyệt. |
| 4 | Đổi mật khẩu | SQ04 | Kiểm tra mật khẩu hiện tại; gồm phiên bắt đổi mật khẩu tạm; thu hồi mọi phiên và đăng nhập lại. |
| 5 | Đăng ký khóa học | SQ07a | Student tự tạo Registration; khóa ACTIVE; chặn hồ sơ chưa kết thúc trùng Student/Course. |
| 6 | Hủy hồ sơ | SQ07b; SQ12c | Tách Student hủy hồ sơ riêng với Admissions hủy nghiệp vụ; điều kiện và tác động khác nhau. |
| 7 | Xác nhận nhập học | SQ11 | Student xác nhận sau kết quả Band; chặn lượt thi còn chờ; không phát hành Invoice tại đây. |
| 8 | Học viên xem và in hóa đơn | SQ13a–b | Chỉ Invoice của mình; khi in phải đọc dữ liệu hiện tại, không dùng bản xem cũ làm chứng từ. |
| 9 | Kế toán tra cứu hóa đơn | SQ13c | Quyền Accountant, bộ lọc/chi tiết nghiệp vụ; không mặc định có quyền in như Student. |
| 10 | Quản lí khóa học | SQ15a–c | Tách Course, cấu hình Program và kích hoạt/ngừng Program; Band tham chiếu, không thêm editor ngưỡng Band. |

Phần này còn thiếu các luồng của Admin, thi đầu vào, tuyển sinh, thanh toán, lớp/lịch, điểm danh, gửi/trả/chốt điểm, báo cáo và nhật ký. Không lấy 10 hình cũ làm toàn bộ phạm vi hệ thống.

## 2. Danh mục nghiệp vụ cập nhật

### A. Tài khoản, phiên và hồ sơ cá nhân

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ01 | **Student tự đăng ký tài khoản** — khách đăng ký | Chuẩn hóa/kiểm tra dữ liệu, email/điện thoại duy nhất; tạo Account Student + Profile + audit nguyên tử; trả kết quả. Không tự thêm bước đăng nhập. | NV01; W01; UC01 |
| SQ02 | **Admin cấp tài khoản nhân sự** — Admin | Kiểm tra quyền/vai trò được cấp; tạo tài khoản với mật khẩu tạm; bàn giao secret một lần. Mật khẩu tạm 32 ký tự, hạn 24 giờ, bắt đổi khi đăng nhập; không gửi email tự động. | NV33; W01; UC01 |
| SQ03 | **Đăng nhập và đăng xuất** — cả 7 vai trò | Tách đăng nhập/đăng xuất. Sai thông tin trả lỗi chung; giới hạn thử đăng nhập. Login thành công tạo phiên NORMAL hoặc PASSWORD_CHANGE; logout thu hồi đúng phiên hiện tại. NORMAL: tối đa 2 giờ, idle 30 phút; phiên hạn chế tối đa 10 phút và không vượt hạn mật khẩu tạm. | NV01 và thao tác chung; W02; UC01 |
| SQ04 | **Tự đổi mật khẩu** — chủ tài khoản | Điểm vào thường hoặc phiên hạn chế; kiểm tra mật khẩu hiện tại; thay hash, cập nhật trạng thái bắt đổi và thu hồi mọi phiên; yêu cầu đăng nhập mới. Không phục hồi mật khẩu bằng email. | Thao tác chung; W02; UC01 |
| SQ05 | **Reset/khóa/mở/ngừng dùng/khôi phục tài khoản** — Admin | Reset bàn giao mật khẩu tạm một lần và revoke-all; khóa/ngừng dùng thu hồi phiên theo hợp đồng action. Mở/khôi phục không phục hồi phiên cũ. Chặn Admin tự khóa/ngừng dùng và tác động làm mất Admin ACTIVE cuối cùng. | NV33–34; W03; UC01 |
| SQ06 | **Xem/sửa hồ sơ cá nhân và đọc trang chủ** — Student | Đọc đúng chủ sở hữu; sửa tên, ngày sinh, điện thoại, địa chỉ; email đăng nhập bất biến. Lưu Account + Profile theo phiên bản hiện tại trong cùng giao dịch. Trang chủ là phép đọc trạng thái hồ sơ/lớp/hóa đơn/kết quả có sẵn. | Thao tác chung, phép chiếu NV02–08; W04; UC01–07 |

### B. Đăng ký khóa và thi đầu vào

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ07 | **Tạo/hủy hồ sơ đăng ký khóa của mình** — Student | Chọn Course ACTIVE; kiểm tra hồ sơ chưa kết thúc cùng Student/Course; tạo Registration. Hủy là thao tác riêng, kiểm lại trạng thái/booking/Invoice hiện tại; giữ lịch sử. Không ghi danh thay hoặc tự tạo Invoice khi đăng ký. | NV02; W05; UC02 |
| SQ08 | **Tổ chức và quản lý ca thi đầu vào** — AcademicAffairs | Tạo/sửa/mở/đóng/hủy ca; đổi grader; kiểm tra thời gian, phòng, giáo viên, sức chứa và xung đột. Hủy ca xử lý toàn bộ booking liên quan cùng giao dịch, không xóa lịch sử. Teacher tra cứu ca được phân công là luồng đọc riêng. | NV14–15,21; W06; UC03 |
| SQ09 | **Đặt/hủy/đổi ca, thi lần hai và đặt lại** — Student | Chỉ đặt ca OPEN trước giờ bắt đầu; kiểm lại chỗ trống/xung đột. Attempt khác Booking: đổi/hủy/đặt lại sau ABSENT không tự tạo lượt mới. Tối đa 2 lượt; lượt 2 cần điểm lượt 1 và phải trước CONFIRMED. | NV03; W07; UC03 |
| SQ10 | **Chấm điểm/ghi vắng/sửa điểm và xác định Band** — Teacher | Kiểm quyền grader hiện tại và điều kiện giờ/trạng thái; ghi score hoặc ABSENT, không gán vắng thành 0. Sửa điểm cần lý do và original grader còn được phân công, trước CONFIRMED. Tính lại điểm tốt nhất/Band cùng thao tác: hòa chọn lượt 1; ngưỡng lớn nhất ≤ điểm. Student/AcademicAffairs xem kết quả qua luồng đọc riêng. | NV04,15,22,36; W08; UC03 |
| SQ11 | **Student xác nhận nhập học** — Student | Đọc kết quả hiện tại; cần Band và không còn lượt thi SCHEDULED hoặc đã thi nhưng chưa chấm. Xác nhận chuyển CONFIRMED, khóa thi lại/sửa điểm; không duyệt tuyển sinh, không tạo Invoice. | NV05; W09; UC02 |

### C. Tuyển sinh, hóa đơn và thu tiền

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ12 | **Duyệt/từ chối/hủy hồ sơ tuyển sinh** — Admissions | Tra cứu rồi thực hiện action riêng. Duyệt hồ sơ CONFIRMED: ADMITTED + một Invoice UNPAID + audit trong một giao dịch. Snapshot học phí lúc phát hành; hạn thanh toán ngày Việt Nam +7 ngày lịch. Từ chối/hủy cần điều kiện/lý do; hủy hợp lệ xử lý Invoice UNPAID và booking tương lai, chặn hồ sơ đã xếp lớp hoặc PAID. | NV09,12,37; W10–11; UC02 |
| SQ13 | **Tra cứu/xem/in hóa đơn** — Student hoặc Accountant | Student đọc/in Invoice riêng; in yêu cầu đọc mới. Accountant tra cứu/chi tiết theo quyền riêng. Không phát hành lại, không sửa số tiền và không tự thêm quyền in cho staff. Quá hạn là cảnh báo tính từ UNPAID + due_date, không là trạng thái lưu mới. | NV06,27; W11; UC04 |
| SQ14 | **Xác nhận thu tiền và sửa metadata thanh toán** — Accountant | Tiền đã thu bên ngoài ứng dụng; xác nhận đủ toàn bộ amount một lần: Payment + Invoice PAID + audit nguyên tử. Mã code CASH/TRANSFER; chuyển khoản cần reference. Sửa method/reference là thao tác khác, cần lý do; giữ amount, liên kết và thời điểm. Không gateway/trả góp/refund. | NV28; W12; UC04 |

### D. Cấu hình, lớp và lịch

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ15 | **Quản lý Course và cấu hình TrainingProgram** — AcademicAffairs | Tách Course; tạo/lưu toàn cấu hình DRAFT; kích hoạt/ngừng/khôi phục Program. ACTIVE cần LESSON count >0, weight tổng 100, đúng một FINAL_EXAM. First Class use khóa Course/Band/count/components/kinds/weights, kể cả Class CANCELLED. Giá Course mới chỉ áp dụng Invoice phát hành sau. Band là dữ liệu tham chiếu; không bổ sung chức năng sửa ngưỡng. | NV13; W13; UC05 |
| SQ16 | **Quản lý vòng đời lớp và giáo viên lớp** — AcademicAffairs | Tạo PLANNED, mở OPEN, bắt đầu ONGOING, hoàn tất COMPLETED hoặc hủy theo guard; đổi Teacher là action riêng. Hoàn tất cần đủ nghĩa vụ buổi HELD và đủ điểm danh LESSON; chưa tự chốt điểm. Teacher xem lớp được phân công qua luồng đọc riêng. | NV16–17,23; W14; UC06–07 |
| SQ17 | **Xếp học viên vào lớp** — AcademicAffairs | Registration ADMITTED + Invoice PAID; Class OPEN trước buổi đầu, cùng Course/Band, còn chỗ (tối đa 15) và không trùng lịch. Tạo StudentClassAssignment + GradeReport DRAFT nguyên tử. Registration vẫn ADMITTED; không thêm chuyển lớp. | NV18; W15; UC06 |
| SQ18 | **Lập lịch, dời/thay buổi, xác nhận HELD và xem lịch** — AcademicAffairs | Tách preview/lưu lịch tuần, chỉnh/dời/thay buổi và HELD. Preview không giữ chỗ; lưu batch tuần all-or-none, kiểm xung đột phòng/lớp/Teacher/Student theo khoảng [start,end). Lịch mới tương lai; replacement giữ cùng nghĩa vụ. HELD chỉ do Giáo vụ xác nhận sau end khi Class ONGOING. Student/Teacher xem đúng phạm vi qua luồng đọc riêng. | NV07,19,23; W16; UC06–07 |

### E. Điểm danh và bảng điểm

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ19 | **Ghi/cập nhật điểm danh một buổi LESSON** — Teacher | Kiểm phân công hiện tại và roster; từ giờ bắt đầu LESSON đến trước Class COMPLETED. Lưu một buổi theo dữ liệu/phiên bản hiện tại; chưa ghi khác Vắng. Không điểm danh FINAL_EXAM hoặc suy ra điểm danh từ lịch HELD. | NV24; W17; UC07 |
| SQ20 | **Lưu điểm thành phần và nhận xét một học viên** — Teacher | Class ONGOING/COMPLETED, report DRAFT/RETURNED; lưu một Student cùng các điểm/nhận xét liên quan. Điểm 0–100 tối đa 2 số lẻ; thiếu khác 0. Tổng nháp tính khi đọc, không lưu thành final_score hoặc công bố. | NV25–26,38; W18; UC07 |
| SQ21 | **Gửi/gửi lại bảng điểm cả lớp** — Teacher | Class COMPLETED; kiểm toàn roster, đủ mọi điểm, không có sửa chưa lưu. Chuyển cả lớp sang SUBMITTED trong một giao dịch; gửi lại từ RETURNED. Không gửi riêng một học viên hoặc một phần lớp. | NV25–26; W18; UC07 |
| SQ22 | **Trả lại/chốt bảng điểm và xem kết quả đã công bố** — AcademicAffairs; Student ở luồng đọc | Tách RETURN, FINALIZE và Student read. RETURN cần lý do, chuyển toàn lớp về RETURNED. FINALIZE cần cả lớp SUBMITTED + Class COMPLETED, tính/lưu tổng chính thức half-up một lần 2 số lẻ, passed khi ≥50; report FINALIZED + xử lý hồ sơ + audit nguyên tử, kể cả không đạt. FINALIZED bất biến; Student chỉ đọc kết quả riêng đã chốt. | NV08,20,38; W19; UC07 |

### F. Báo cáo và nhật ký

| Mã | Sequence và actor | Nội dung/nhánh bắt buộc | Truy vết |
| --- | --- | --- | --- |
| SQ23 | **Báo cáo tài chính và xuất CSV** — Accountant | Tách query báo cáo và query mới khi xuất toàn bộ bộ lọc; không xuất từ trang/payload cũ. Doanh thu theo ngày xác nhận Payment; billed theo ngày phát hành Invoice không CANCELLED; công nợ UNPAID hiện tại không phụ thuộc kỳ. CSV 6 cột + footer, tối đa 10.000 dòng dữ liệu gồm footer, 5.000.000 byte, bảo vệ công thức/escaping. | NV29; W20; UC08 |
| SQ24 | **Báo cáo tổng hợp tuyển sinh/đào tạo/tài chính** — Director | Ba loại query, chỉ tổng hợp không định danh Student; không export hoặc phê duyệt. Distinct Student tính toàn tập, không cộng số nhóm; tỷ lệ đạt theo cohort ngày FINALIZED, mẫu số rỗng là chưa có dữ liệu. | NV30–32; W20; UC08 |
| SQ25 | **Tra cứu nhật ký/chi tiết/nhóm cùng thao tác** — Admin | Đọc bộ lọc, phân trang, chi tiết và operation group theo quyền Admin. Không sửa/xóa/export nhật ký. Ghi audit là hiệu ứng nội bộ của luồng khác, không gọi Admin để ghi log và không mở endpoint ghi log nghiệp vụ tùy ý. | NV35, tham chiếu NV39; W21; UC09 |

## 3. Những hình nên tách trong cùng mã SQ

Không thay mã gốc và không coi các hình con là chức năng mới:

- SQ03: a Đăng nhập; b Đăng xuất.
- SQ05: a Reset mật khẩu; b Các action trạng thái tài khoản.
- SQ07: a Tạo hồ sơ; b Student hủy hồ sơ.
- SQ08: a Tạo/sửa/mở/đóng ca; b Hủy ca và xử lý booking; c Đổi grader.
- SQ09: a Đặt ca/lượt 2; b Hủy/đổi ca; c Đặt lại cùng lượt sau vắng/hủy ca.
- SQ10: a Chấm điểm/tính Band; b Ghi vắng; c Sửa điểm có lý do.
- SQ12: a Duyệt + Invoice; b Từ chối; c Admissions hủy.
- SQ13: a Student xem; b Student in với đọc mới; c Accountant tra cứu.
- SQ14: a Xác nhận thu tiền; b Sửa metadata.
- SQ15: a Course; b Tạo/lưu cấu hình Program; c Kích hoạt/ngừng/khôi phục Program.
- SQ16: a Tạo/đổi Teacher; b Chuyển trạng thái lớp.
- SQ18: a Preview/lưu lịch tuần; b Chỉnh/dời/thay buổi; c Xác nhận HELD; d Đọc lịch theo vai trò.
- SQ22: a Trả lại cả lớp; b Chốt cả lớp; c Student xem kết quả FINALIZED.
- SQ23: a Query báo cáo; b Query mới và xuất CSV.

## 4. Cơ chế hiện tại cần thể hiện khi vẽ sequence kỹ thuật

Các mục dưới đây là cơ chế dùng chung, **không thêm vào 25 nhóm nghiệp vụ** và không phải actor/vai trò mới. Với báo cáo mức nghiệp vụ, dùng note hoặc `ref`; nếu trình bày kiến trúc triển khai mới cần hình riêng.

| Ref | Nội dung cần giữ theo code hiện tại |
| --- | --- |
| T01 — Phiên/RAM/CSRF | Khi startup, deadline, tín hiệu liên tab, Back/Forward hoặc kiểm tra thủ công: kiểm tra phiên theo controller hiện tại. Tab hidden dừng thao tác và xóa RAM; hidden→visible kiểm lại me/CSRF. Từ cập nhật 14/10, focus cửa sổ đơn thuần và sự kiện visibility trùng trạng thái không tạo request kiểm tra phiên. 401 xóa trạng thái và về đăng nhập. Không vẽ polling/idle renewal hoặc focus→login tự động. |
| T02 — Lệnh ghi và kết quả không rõ | Đọc/preview → xác nhận → gửi lệnh với phiên bản/manifests/request ID theo hợp đồng → backend kiểm quyền/state hiện tại → ghi hiệu ứng + audit bắt buộc + replay receipt nguyên tử → trả kết quả sau commit. Nếu mất phản hồi sau gửi, giữ trạng thái không rõ và tra cứu thủ công đúng operation; không tự gửi lại lệnh hoặc suy ra rollback. Receipt/lookup dùng đúng họ endpoint hiện hành, không gán một endpoint chung cho mọi module. |
| T03 — Audit đúng loại | NV39 xuyên các SQ có action thuộc danh mục mandatory audit: audit thất bại thì rollback cùng thao tác. Security-only events như login/logout/denial có cơ chế best-effort riêng. Không thêm audit bắt buộc cho mọi read; không gửi secret/raw payload vào log. SQ25 chỉ là đọc. |

Các POST search trong UI vẫn là phép đọc; không vẽ chúng như cập nhật dữ liệu. Các lần đọc đầu màn hình dùng phạm vi hiện tại và cơ chế hủy/loại phản hồi cũ, không tự chuyển trạng thái nghiệp vụ.

## 5. Ranh giới và độ bao phủ

- 7 vai trò cố định: Student, Admissions, AcademicAffairs, Teacher, Accountant, Director, Admin.
- 37 NV còn hiệu lực: NV01–NV39 trừ NV10/NV11; 32 F trừ F15; 21 workflow W01–W21. Giữ số gốc, không đánh lại số.
- NV36 nằm tại SQ10; NV37 tại SQ12; NV38 tại SQ20/SQ22; NV39 là hiệu ứng xuyên suốt, không chỉ SQ25.
- Hành trình SQ07 → SQ09 → SQ10 → SQ11 → SQ12 → SQ14 → SQ17 gồm nhiều người và nhiều giao dịch. Không vẽ thành một giao dịch hoặc các actor gọi nhau đồng bộ.
- SQ19/SQ20 → Class COMPLETED ở SQ16 → SQ21 → SQ22; RETURNED có thể quay lại SQ20/SQ21. Hoàn tất lớp và chốt điểm là hai thao tác khác nhau.
- Band tham chiếu có seed 0/25/45/65/85; bộ ngưỡng bị khóa sau completed entry result đầu tiên. Không suy ra có UI editor từ quy tắc dữ liệu.
- M0–M3 và phần báo cáo/CSV/audit viewer M4 đã triển khai trên main, trong host LOCAL_DEV tường minh. UI refresh đã áp dụng; owner gate4 còn pending. Backup/restore M4 còn cần đầu vào vận hành: không đưa vào danh mục như chức năng đã hoàn thành.
- Lượt này chỉ đối chiếu và lập danh mục tài liệu; không chạy lại kiểm thử runtime hoặc thay đổi mã sản phẩm.

## 6. Nguồn đối chiếu

- `AGENTS.md`, `docs/PROJECT_CONTEXT.md`, `docs/ARCHITECTURE.md`, `docs/CURRENT_STATE.md`, `docs/DECISIONS.md`, `docs/TODO.md` — bối cảnh và checkpoint hiện hành.
- `docs/requirements.md`, `docs/design/FUNCTION_SPEC.md`, `docs/design/WORKFLOW_SPEC.md` — quyền, trạng thái và quy tắc nghiệp vụ.
- `docs/design/SEQUENCE_PLAN.md` và các `SQ*_GUIDE.md` — mã SQ và hướng dẫn trước đây, giữ nguyên file gốc.
- `docs/plan/implementation-plan.md`, `docs/plan/SESSION_FOCUS_REVIEW.md` — phạm vi triển khai và thay đổi phiên ngày 14/10.
- `backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs` — composition các adapter hiện tại.
- Các service Identity/Auth/Administration/Enrollment/Academics/Admissions/Finance/Classes/Reports; `frontend/src/app/IdentityApp.tsx`, `frontend/src/features/identity/sessionController.ts` — điểm đối chiếu triển khai, không thay bằng các giả định của báo cáo cũ.
- `docs/USE_CASE_CAP_NHAT_THEO_DU_AN_20261014.docx` — mã UC dùng truy vết với lượt use case vừa cập nhật.
- Báo cáo nguồn `docs/Nhom18_ThietKeXuLy.docx`; SHA256 trước đối chiếu: `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`.
