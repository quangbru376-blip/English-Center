# Đối chiếu bảng chức năng nghiệp vụ với dự án hiện tại

Ngày đối chiếu: 14/10/2026. Phạm vi: chỉ mục **III. Bảng chức năng nghiệp vụ** của `docs/Nhom18_ThietKeXuLy.docx`, từ tiêu đề này đến trước mục IV. Không sửa file Word nguồn, không đối chiếu biểu mẫu, mô hình dữ liệu hoặc sequence trong lượt này.

## Kết luận

Bảng hiện tại đã được cập nhật đáng kể và **phù hợp về danh mục, vai trò và luồng nghiệp vụ chính** với dự án. Có 37 dòng chức năng: các số 1–9 và 12–39; không còn hai chức năng Tuyển sinh làm thay ở số 10–11. Bảy vai trò người dùng được giữ đúng; nhóm “Hệ thống” mô tả xử lý tự động, không phải vai trò tài khoản thứ tám.

Qua đối chiếu nội dung bảng với yêu cầu chuẩn và các nhánh mã nguồn liên quan, chưa xác định mâu thuẫn lớn ở chức năng cốt lõi. Những khác biệt đáng chú ý là **điều kiện chưa được mô tả đủ**, một số câu cần làm rõ và lỗi diễn đạt. Không nên kết luận bảng này vẫn là bảng cũ có 39 chức năng, Excel/CSV hoặc Tuyển sinh ghi danh thay.

Đây là đối chiếu nội dung và đọc mã nguồn; không chạy lại hệ thống hay bộ kiểm thử. Các checkpoint PASS trong Project Memory là bằng chứng đã lưu từ những lượt trước, không phải kiểm thử mới trong lượt này. Không coi việc đọc mã nguồn là chứng minh mọi trường hợp đã chạy đúng.

## Nguồn và phiên bản

- Word đọc trực tiếp: [Nhom18_ThietKeXuLy.docx](../Nhom18_ThietKeXuLy.docx).
- SHA-256 trước đối chiếu: `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`.
- SHA-256 kiểm lại sau khi lập bản đối chiếu không đổi; mục III không có tracked insertion/deletion trong XML. Đã đọc đủ 37 dòng của tám bảng, không suy nội dung từ bản đối chiếu cũ.
- Mục III có tám bảng theo nhóm, 37 dòng chức năng và tám dòng tiêu đề; năm cột là STT, Tên chức năng, Quy định / Điều kiện, Biểu mẫu, Ghi chú. STT được đối chiếu với mã NV tương ứng, không đánh lại số.
- Chuẩn nghiệp vụ: [requirements](../requirements.md), [FUNCTION_CATALOG](../design/FUNCTION_CATALOG.md), [WORKFLOW_SPEC](../design/WORKFLOW_SPEC.md), [DECISIONS](../DECISIONS.md).
- Trạng thái mới nhất: [CURRENT_STATE](../CURRENT_STATE.md) và phần đầu AGENTS.md; các đoạn lịch sử cũ không thay thế checkpoint mới.
- Mã nguồn được đọc ở checkout main, HEAD quan sát `689ee43`; các tài liệu chưa commit hiện có được giữ nguyên.

Bản [PROCESSING_REPORT_MEMORY_COMPARISON](PROCESSING_REPORT_MEMORY_COMPARISON.md) lập ngày 06/10 dùng hash `C7DE4DE6338670A94C99280DF89BDDC8E06FAE53A63E3B0405BBFE9DA0149F6C`. Hash khác bản hiện tại nên các nhận xét của bản cũ không được áp nguyên trạng cho mục III hiện nay.

## Đối chiếu từng dòng chức năng

“Khớp chính” nghĩa là tác nhân, mục đích và quy tắc chính phù hợp; không có nghĩa dòng đó đã chứa toàn bộ validation, điều kiện thời gian và giới hạn đọc dữ liệu của triển khai. Các mục B bên dưới chỉ ra phần cần làm rõ hoặc bổ sung.

| STT / NV | Chức năng trong Word | Đối chiếu với dự án |
|---|---|---|
| 01 | Đăng ký, đăng nhập và quản lý thông tin cá nhân | Khớp chính: Student tự đăng ký, email bất biến, tự sửa đúng trường. Bổ sung chính sách tài khoản/phiên và validation chung tại B01, B10. |
| 02 | Xem khóa học, đăng ký và hủy hồ sơ đăng ký | Khớp chính: một hồ sơ chưa kết thúc mỗi khóa; đổi khóa bằng hủy hợp lệ và tạo mới; học lại không phụ thuộc đạt. Làm rõ trạng thái và hệ quả hủy tại B02. |
| 03 | Đăng ký, đổi, hủy và xem lịch thi đầu vào | Khớp chính: hai lượt mỗi hồ sơ, booking khác lượt thi, ca nguồn/đích OPEN trước start. Bổ sung không có hai booking SCHEDULED đồng thời và kiểm lại lúc lưu; B02, B09. |
| 04 | Xem kết quả thi đầu vào và mức trình độ | Khớp: kết quả riêng, điểm tốt nhất, hòa chọn lượt một; vắng khác 0. |
| 05 | Xác nhận nhập học | Khớp: BAND_ASSIGNED có Band hợp lệ; chờ lượt hai chặn xác nhận; chưa phát hành hóa đơn. |
| 06 | Xem, in hóa đơn và hướng dẫn thanh toán | Khớp: own Invoice, đọc mới trước in, trả tiền ngoài ứng dụng; không tự xác nhận, không PDF API/biên lai riêng. |
| 07 | Xem lớp và thời khóa biểu cá nhân | Khớp chính: own classes/schedules, không tự xếp/chuyển/sửa lịch. Chưa nói rõ Hiện tại/Lịch sử và buổi NOT_HELD/thay thế; B07. |
| 08 | Xem kết quả học tập | Khớp: chỉ own FINALIZED của lớp COMPLETED; thang 100, đạt từ 50; không lộ nháp. |
| 09 | Tra cứu và kiểm tra hồ sơ | Khớp về quyền đọc, không làm thay Student. “Thông tin được phép” chưa mô tả cụ thể scope; B07. |
| 12 | Duyệt, từ chối hoặc hủy hồ sơ nhập học | Khớp chính: CONFIRMED → ADMITTED + một Invoice nguyên tử; từ chối/hủy có lý do, không hủy PAID. Tách điều kiện hủy CONFIRMED và ADMITTED; B02. |
| 13 | Quản lý khóa học, chương trình đào tạo và cấu hình mức trình độ | Khớp chính: Program theo Course/Band, tổng trọng số 100, một FINAL_EXAM, freeze từ lần dùng đầu. Bổ sung vòng đời chỉnh cấu hình và mã bất biến; B03. |
| 14 | Tổ chức và quản lý ca thi đầu vào | Khớp chính: CLOSED mới tạo, sức chứa 1–20, effective CLOSED từ start, hủy/đổi hợp lệ. Thiếu điều kiện start tương lai khi tạo, lý do hủy và “chưa từng có booking”; B04. |
| 15 | Theo dõi kết quả thi đầu vào và mức trình độ | Khớp: Giáo vụ chỉ theo dõi, không chấm/ghi đè thay Teacher. Cần cụ thể scope dữ liệu; B07. |
| 16 | Tạo và quản lý lớp học | Khớp chính: PLANNED → OPEN → ONGOING → COMPLETED, hủy lớp trống trước dạy. Thiếu liên kết Program bất biến, cửa sổ sửa sức chứa và một nghĩa vụ FINAL_EXAM; B05. |
| 17 | Phân công hoặc đổi giáo viên phụ trách | Khớp: một Teacher, trước dạy và chưa Attendance/Score; không tự phân công lại khi tài khoản bị chặn. |
| 18 | Xếp học viên vào lớp | Khớp chính: ADMITTED + PAID, đúng Course/Band, OPEN/còn chỗ, tạo DRAFT cùng giao dịch; không gỡ/chuyển. Triển khai còn kiểm trước giờ LESSON đầu và tài khoản ACTIVE; B05. |
| 19 | Quản lý lịch học, lịch thi và buổi thay thế | Khớp chính: lịch tuần all-or-none, nghĩa vụ buổi thay thế, HELD do Giáo vụ xác nhận. Thiếu điều kiện thời gian/lớp/dữ liệu của một số thao tác; B06. |
| 20 | Kiểm tra, trả lại và chốt bảng điểm | Khớp: COMPLETED + toàn bộ SUBMITTED; return/finalize cả lớp, lý do return, FINALIZED bất biến. |
| 21 | Xem ca thi được phân công | Khớp: Teacher chỉ ca đang được phân công. Chưa mô tả các trường được xem; B07. |
| 22 | Nhập, sửa điểm thi đầu vào hoặc ghi vắng | Khớp: từ session end, original grader vẫn assigned mới sửa, trước confirmation, reason/audit/recalculate nguyên tử; không suy vắng từ thiếu điểm. Bổ sung tối đa hai chữ số thập phân; B10. |
| 23 | Xem lịch dạy và danh sách lớp | Khớp: chỉ lớp đang phụ trách. Chưa mô tả các trường được xem; B07. |
| 24 | Ghi và cập nhật điểm danh | Khớp chính: từ LESSON start đến trước COMPLETED, có/vắng/chưa ghi tách biệt, lưu một buổi atomic nhưng được chọn một phần roster. Sửa câu phủ định; B11. |
| 25 | Nhập điểm, lưu nháp và gửi bảng điểm | Khớp: một Student/save; Class ONGOING/COMPLETED và report DRAFT/RETURNED; whole-class submission, thiếu điểm khác 0, không auto-delete/auto-publish. |
| 26 | Sửa và gửi lại bảng điểm bị trả | Khớp: sửa RETURNED và resubmit cả lớp; giữ dữ liệu/lý do/lịch sử, không sửa SUBMITTED/FINALIZED. |
| 27 | Tra cứu hóa đơn và theo dõi công nợ | Khớp: UNPAID/PAID/CANCELLED, hạn issue VN +7 ngày, overdue là cảnh báo. Thiếu các bộ lọc cụ thể, không phải sai định nghĩa nợ; B08. |
| 28 | Đối chiếu, xác nhận thanh toán và sửa thông tin phương thức | Khớp: một Payment đủ tiền/Invoice, CASH/TRANSFER, reference khi chuyển khoản; chỉ sửa metadata có lý do, tiền/link/time bất biến. |
| 29 | Xem và xuất báo cáo tài chính | Khớp về chỉ số/quyền CSV. Cần nói chính xác cách tính cap, snapshot mới và bộ lọc; B08. |
| 30 | Xem báo cáo tuyển sinh | Khớp: Director aggregate-only, distinct Student từng nhóm/toàn tập, current state, không export. Thiếu kỳ ngày đăng ký/default và chỉ số số hồ sơ riêng; B08. |
| 31 | Xem báo cáo đào tạo | Khớp: HELD obligations, exam riêng, FINALIZED-date cohort, empty denominator không thành 0%. Thiếu inventory/date/result filters; B08. |
| 32 | Xem báo cáo tài chính | Khớp: Director cùng định nghĩa chỉ số với Accountant nhưng không đọc tài chính cá nhân/xác nhận/export. |
| 33 | Cấp tài khoản nhân sự và đặt lại mật khẩu | Khớp: temporary password 32 ký tự, 24h, một lần hiển thị; revoke-all, restricted tối đa 10 phút, reset không mở khóa. |
| 34 | Khóa, mở, ngừng và khôi phục tài khoản | Khớp: ACTIVE/LOCKED/DISABLED, self/last-active Admin guards, unlock/restore không hồi sinh phiên, không tự đổi phân công. |
| 35 | Tra cứu nhật ký hoạt động | Khớp: Admin-only, event/group scope, không edit/delete/undo/export hoặc mở quyền nghiệp vụ từ log. Thiếu structured filters/newest-first paging; B08. |
| 36 | Tự động xác định mức trình độ | Khớp: best completed score, min_score lớn nhất ≤ score, hòa chọn lượt một, freeze ngưỡng, không auto-grade. “Sửa thông tin hợp lệ” nên là “sửa điểm đầu vào hợp lệ”; B11. |
| 37 | Tự động phát hành hóa đơn | Khớp: approval trigger, snapshot học phí, một Invoice, nguyên VND, due +7 ngày, replay không nhân đôi. Thiếu định dạng số Invoice và không tái dùng số; B10. |
| 38 | Tự động tính điểm tổng kết và kết quả đạt | Khớp: decimal weighted sum, half-up một lần hai chữ số, lưu chính thức lúc FINALIZED, đạt ≥50, không thêm chuyên cần. Nên dùng từ “half-up” hoặc ví dụ 49,995 → 50,00 cho rõ. |
| 39 | Ghi nhật ký nghiệp vụ và sự kiện bảo mật | Khớp: mandatory audit atomic/rollback, security-only best-effort, nghiệp vụ giữ lịch sử, pure-security/IP 90 ngày, không log mỗi lần đọc/bí mật. |

## Điều kiện cần làm rõ hoặc bổ sung

### B01 Chính sách đăng nhập và phiên chưa được mô tả đủ tại NV01

NV01 đang nêu đúng email/password và đổi mật khẩu, nhưng dự án còn có mật khẩu 15–128 ký tự, chặn mật khẩu phổ biến; phiên thường tối đa 2 giờ hoặc 30 phút idle, mốc nào đến trước; chỉ request nghiệp vụ do người dùng thao tác mới tính activity. Logout hủy phiên hiện tại; đổi mật khẩu/reset/lock/disable hủy mọi phiên. Năm lần sai trong 15 phút chặn login mới 15 phút, độc lập LOCKED thủ công. Không bắt buộc dồn hết vào một ô: có thể dẫn tới quy định tài khoản chung của bản báo cáo mới.

Nguồn: requirements phần tài khoản; `Common/Security/SessionIssuanceWindow.cs:28`, `Services/Auth/CurrentSessionService.cs`, WORKFLOW_SPEC W02–W03. Không đưa chi tiết session-focus của UI thành chức năng NV mới.

### B02 Trạng thái hồ sơ và hậu quả hủy cần rõ hơn tại NV02 NV03 NV12

Word chưa ghi đầy đủ PENDING → TESTING ở booking đầu tiên → BAND_ASSIGNED khi có điểm; lượt hai giữ BAND_ASSIGNED nhưng chặn xác nhận nếu còn chờ. Hủy booking hoặc ABSENT không kéo lùi trạng thái hoặc xóa Band.

“Hủy lịch đặt ca thi tương lai” nên nói rõ **booking SCHEDULED của ca chưa bắt đầu, kể cả ca CLOSED**; khác quyền Student hủy riêng booking, vốn đòi ca OPEN. Giữ history của ca đã bắt đầu, kết quả và Band; hồ sơ CANCELLED chặn tiếp chấm/sửa/ghi vắng. NV12 nên tách: hủy CONFIRMED chưa có Invoice, hoặc ADMITTED có Invoice UNPAID; không gắn điều kiện “có hóa đơn chưa thanh toán” cho cả hai nhánh.

Nguồn: `Services/Enrollment/EnrollmentProjection.cs:19`, `:44`; `StudentEnrollmentService.cs`; `Services/Admissions/AdmissionsModels.cs:58`, `:59`; WORKFLOW_SPEC W05–W10.

### B03 Vòng đời cấu hình còn được tóm tắt tại NV13

Program ACTIVE chưa dùng muốn sửa cấu hình phải trở về DRAFT; first Class use khóa Course/Band/count/components/kind/weights, kể cả Class sau đó CANCELLED. Không tạo Program trùng Course/Band hoặc mở khóa bằng INACTIVE/restore. Course code bất biến; giá mới không đổi Invoice cũ. Bảng hiện tại đã nói đúng freeze nhưng chưa thể hiện đủ các đường chỉnh cấu hình.

Nguồn: requirements ADR-085/090; `Services/Academics/ProgramModels.cs`, `ProgramService.cs`, `Services/Catalogue/CourseService.cs`.

### B04 Tạo và chỉnh ca thi cần các điều kiện cụ thể tại NV14

- Ca mới phải có startAt **lớn hơn giờ server lúc lưu**; không tạo ca quá khứ hoặc đúng thời điểm bắt đầu.
- Hủy ca trước start cần lý do, giữ lịch sử.
- Sửa ngày/giờ/phòng chỉ khi **chưa từng có booking**; booking đã hủy cũng không biến ca thành “chưa từng được đặt”. Sau khi đã có booking, dùng hủy có lý do/tạo ca mới.
- Ca đầy vẫn OPEN nhưng hết chỗ; đóng ca giữ booking; không tự hủy hoặc chuyển học viên.

Nguồn: `Services/Academics/EntrySessionService.cs` các nhánh create/schedule/cancel và ValidateResources; requirements ADR-027/030/157. Đây là phần thiếu chi tiết trong mục III, không kết luận quy tắc vắng mặt ở toàn báo cáo.

### B05 Cửa sổ quản lý và xếp lớp chặt hơn câu tóm tắt tại NV16 NV18

NV16 thiếu việc **TrainingProgram của Class bất biến sau tạo**. Sửa max_size chỉ trước dạy, trong 1–15 và không dưới số đã xếp. Lớp có đúng một nghĩa vụ FINAL_EXAM, riêng expected LESSON count; COMPLETE chưa đồng nghĩa đã FINALIZED/kết thúc Registration.

NV18 chỉ ghi OPEN/còn chỗ. Mã nguồn còn kiểm `BeforeTeaching`: không Attendance/Score, chưa HELD và chưa đến start của LESSON hiện hành; kiểm Student ACTIVE và Teacher hợp lệ/ACTIVE. Vì vậy Class còn lưu OPEN nhưng LESSON đầu đã tới giờ không tự tạo quyền nhận thêm học viên. Đây là điểm nên làm rõ nếu bảng cần mô tả đúng điều kiện nút xếp lớp.

Nguồn: `Services/Classes/ClassModels.cs:48`, `:50`, `:51`; `ClassPreviews.cs:24`; `ClassService.cs` API94/108. Khái niệm “đã xếp lớp” vẫn suy từ assignment; không thêm trạng thái Registration mới.

### B06 Điều kiện lịch hiện hành và buổi thay thế cần đủ tại NV19

Mỗi lịch mới, mọi buổi trong batch tuần và đích dời phải ở tương lai lúc server lưu; lịch cùng ngày Việt Nam, chính xác phút, khoảng [start,end) cho phép hai buổi nối tiếp đúng end/start. Preview không giữ chỗ; thay input làm preview cũ mất hiệu lực và server kiểm lại toàn batch.

Dời/thay phải trên lịch hiện hành PLANNED, lớp chưa COMPLETED/CANCELLED, không Attendance/Score phụ thuộc và có Teacher hợp lệ. Dời khi ca nguồn chưa start; thay buổi không tổ chức khi đã tới/qua start, tạo đích tương lai cùng loại/nghĩa vụ; áp cả LESSON lẫn FINAL_EXAM. HELD chỉ từ end khi ONGOING, không có đảo HELD. Word đã mô tả đúng hướng nhưng “khi đủ điều kiện” chưa cho người đọc thấy đủ các giới hạn này.

Nguồn: `Services/Classes/ScheduleRules.cs:114`; `ClassPreviews.cs` Schedule; `ScheduleService.cs`; requirements ADR-087/088/156.

### B07 Phạm vi dữ liệu đọc cần cụ thể tại NV07 NV09 NV15 NV21 NV23

| Vai trò / chức năng | Phạm vi triển khai cần diễn đạt |
|---|---|
| Student NV07 | Lớp/lịch riêng, cả hiện tại và lịch sử; NOT_HELD/thay thế được phân biệt, không đếm đôi hoặc coi lịch cũ là lịch sắp tới. |
| Admissions NV09 | Liên hệ tên/email/phone hiện tại, Course/entry/Band, confirmation/approval/state, Invoice tiền/trạng thái và Class; không DOB/address, Payment method/reference, Attendance/training grade hoặc export. |
| AcademicAffairs NV15 và các trang quản lý | Dữ liệu phục vụ nhiệm vụ; Invoice chỉ trạng thái, không tiền/Payment. Quyền theo dõi không cho nhập/sửa entry score; review không lộ giá trị DRAFT. |
| Teacher NV21/NV23 | ID nội bộ/tên/email/phone và dữ liệu nhiệm vụ thuộc ca/lớp hiện assigned; không DOB/address/finance hoặc danh sách Student toàn trung tâm. Mất assignment thì mất quyền nhiệm vụ đó. |

Thông tin cá nhân trên hồ sơ lịch sử lấy profile hiện tại, không snapshot Registration. Đây là chi tiết còn thiếu ở bảng, không phải bốn quyền đọc mới.

Nguồn: requirements ADR-039/083/084; `Services/Admissions/AdmissionsModels.cs:10`; `Services/Enrollment/TeacherTaskProjection.cs:11`; `Infrastructure/Classes/SqlStudentGradeReads.cs:60`; các DTO/read projections liên quan.

### B08 Báo cáo CSV và tra cứu còn thiếu điều kiện chi tiết

NV29 ghi đúng 10.000 dòng/5 MB nhưng triển khai tính **dòng tổng vào cap 10.000 dòng dữ liệu**, không tính header. Cap bytes là **5.000.000 byte UTF-8**, gồm BOM, header, footer, quotes/escapes và CRLF. CSV có sáu cột nhóm Course và dòng tổng toàn tập, không phải xuất danh sách Invoice/Student. Query export mới có snapshot/queried_at riêng; kết quả có thể khác trang đã đọc. Do đó chỉ ghi “tối đa 10.000 dòng hoặc 5 MB” chưa đủ nếu kiểm đúng biên.

NV30 cần phân biệt số Registration với distinct Student, lọc ngày đăng ký và mặc định tháng Việt Nam hiện tại/mọi trạng thái. NV31 tách inventory không kỳ ngày với result cohort theo ngày FINALIZED; bộ lọc Course/Band/Class/current Teacher/Class status. NV29/NV32 dùng kỳ doanh thu/phải thu mặc định tháng hiện tại; debt hiện tại không chịu kỳ ngày. Course INACTIVE được giữ cho lịch sử.

NV27 cần ghi các bộ lọc Invoice number/Course/contact/status/issue date/overdue nếu bảng đóng vai trò đặc tả tra cứu. NV35 cần structured filters, newest-first paging và chi tiết theo allowlist/operation grouping.

Nguồn: `Services/Reports/FinanceCsv.cs:8`, `:13`, `:17`; `ReportQuery.cs:12`; `ReportProjection.cs:28`; `Api/Reports/ReportEndpoints.cs:13`; requirements ADR-060/068–072/094 và mục4.10. Quyền Director không xuất file và Accountant-only CSV đã đúng trong Word.

### B09 Quy định thao tác lặp dữ liệu cũ và kết quả không rõ chưa có trong bảng

Bảng đã nói “duyệt lặp không tạo Invoice thứ hai” nhưng cơ chế áp rộng hơn: các lệnh quan trọng có request identity; cùng key/nội dung đã hoàn tất không tạo hiệu ứng/audit/activity thứ hai, quyền hiện tại vẫn được kiểm. Form cũ hoặc trạng thái/thành viên thay đổi bị từ chối, không âm thầm merge/ghi đè. Mất phản hồi phải kiểm tra kết quả có quyền theo luồng tra cứu thủ công, không tự gửi lại mutation.

Với intent sửa giá trị không đổi, hệ thống không tạo provenance/marker mới; mixed Attendance/grade draft chỉ cập nhật phần thay đổi thật trong đúng save unit, mọi item vẫn được validate. Không cần đưa tên kỹ thuật vào cột chức năng; một đoạn “quy định chung cho thao tác ghi” trong file mới sẽ dễ đọc hơn.

Nguồn: requirements ADR-093/158/159; `Services/Enrollment/TeacherEntryService.cs:62`; `Services/Classes/GradeService.cs` SaveDraft; `AttendanceService.cs`; `Services/Replay/ReplayService.cs`. Bảng ngắn không cần liệt kê DTO/API để phản ánh quy định này.

### B10 Validation và số hóa đơn chưa có đủ tại NV01 NV13 NV22 NV37

NV01: tên Unicode 1–100, DOB thật/không tương lai theo ngày VN và không giới hạn tuổi, email ASCII ≤254 so trùng không phân biệt hoa/thường, phone di động chuẩn 0/+84, address tùy chọn ≤255. Không trim/NFC mật khẩu. NV13: codes Course/Class bất biến, chuẩn chữ hoa 1–30 A–Z/số/_/-, không tái dùng lịch sử; tiền dương nguyên VND. NV22: entry score 0–100 tối đa hai chữ số thập phân, từ chối độ chính xác vượt mức. NV37: số Invoice `HD-YYYY-NNNNNNNN`, năm nghiệp vụ VN, cấp an toàn/không tái dùng, không yêu cầu liên tục không khoảng trống.

Đây là thiếu mô tả validation, không phải yêu cầu thêm trường hay chức năng. NV25 đã nêu tối đa hai số lẻ cho component score; nên diễn đạt nhất quán với NV22.

Nguồn: `Services/Identity/StudentSignupInput.cs:19`; `Common/Validation`; `Common/Serialization/ExactNumbers.cs`; requirements ADR-073–082/090/091; `Services/Admissions/AdmissionsService.cs` Approve.

### B11 Lỗi diễn đạt trong bảng Word

| Vị trí | Câu hiện tại | Cách diễn đạt đề xuất cho bản mới |
|---|---|---|
| NV12 | “không hủy hồ sơ đã đã thanh toán” | “Không hủy hồ sơ có hóa đơn đã thanh toán.” |
| NV15 | “đóng không ngăn việc chấm hợp lệ” | “Ca thi CLOSED không ngăn giáo viên chấm khi các điều kiện còn hợp lệ.” |
| NV16 | “đã đã tổ chức” | “đã được Giáo vụ xác nhận tổ chức”. |
| NV18 | “Một việc xếp học viên vào lớp mỗi hồ sơ” | “Mỗi hồ sơ chỉ được xếp vào một lớp.” |
| NV19 | “buổi buổi học cuối”, “không tự đã tổ chức” | “buổi học cuối”, “không tự xác nhận buổi đã tổ chức”. |
| NV24 | “buổi buổi học”, “buổi không không tổ chức” | “buổi LESSON”, “buổi hiện hành không ở trạng thái NOT_HELD”. Cụm phủ định kép hiện tại rất dễ đọc sai. |
| NV25 | “trước đã chốt” | “trước khi bảng điểm được chốt”. |
| NV29 | “hóa đơn không đã hủy” | “hóa đơn UNPAID hoặc PAID, loại CANCELLED”. |
| NV31 | “buổi học đã đã tổ chức” | “nghĩa vụ LESSON đã được xác nhận HELD”. |
| NV33 | “không vượt hạn thông tin bí mật” | “không vượt thời điểm hết hạn của mật khẩu tạm”. |
| NV36 | “Tính lại khi sửa thông tin hợp lệ” | “Tính lại khi giáo viên sửa điểm đầu vào hợp lệ.” Sửa profile không phải trigger tính Band. |
| NV37 | “Tiền giá trị được giữ nguyên tại thời điểm phát hành dương” | “Số tiền hóa đơn là học phí dương, nguyên VND tại thời điểm phát hành và được giữ nguyên sau đó.” |

Các sửa chữ ở đây chỉ là đề xuất cho tài liệu riêng. Chưa áp vào Word nguồn.

## Trạng thái dự án cần phân biệt khi dùng bảng

Trong code, [LocalRuntimeServices](../../backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs) gắn các endpoint Student/Teacher entry, Admissions, Finance, Classes/Grades, Reports và Audit vào runtime LOCAL_DEV tường minh; [ClassEndpoints](../../backend/src/EnglishCenter.Api/Api/Classes/ClassEndpoints.cs) gắn GradeEndpoints. Chúng đã có triển khai, không chỉ là thiết kế E01–E05. Program không tham số vẫn không phải cách mở sản phẩm đầy đủ; dùng launcher đúng hợp đồng hiện có.

Theo checkpoint 14/10: M0–M3 COMPLETE; M4 reports/CSV/audit COMPLETE nhưng toàn M4 còn recovery inputs; UI hoàn thành kỹ thuật và chờ owner gate4. Không biến thiếu backup/restore evidence hoặc chưa nghiệm thu UI cổng4 thành kết luận một chức năng trong bảng chưa được viết. Cũng không suy toàn hệ thống production-ready từ việc 37 dòng nghiệp vụ khớp.

## Cách sử dụng kết quả

Giữ 37 chức năng và các số hiện có. Nếu cần chuẩn bị bảng thay thế cho báo cáo, chỉnh câu ở B11 trước, bổ sung các điều kiện ảnh hưởng quyền/thời gian ở B02/B04/B05/B06, rồi cụ thể scope/report/CSV ở B07/B08. Chính sách phiên, validation và xử lý mutation chung có thể đặt ở đoạn quy định chung để bảng không quá dài. Mọi bản chỉnh sửa phải là file mới; Word nguồn và các tài liệu owner hiện có được giữ nguyên.
