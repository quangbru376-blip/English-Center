# Đối chiếu biểu mẫu với hệ thống hiện tại

Ngày đối chiếu: 14/10/2026. Phạm vi: mục **IV. Biểu mẫu** của [Nhom18_ThietKeXuLy.docx](../Nhom18_ThietKeXuLy.docx), gồm tám mẫu, đến trước **V. Mô tả chức năng**. File Word nguồn được giữ nguyên. Đây là kết quả phân tích và đề xuất cấu trúc cho bản báo cáo khác, chưa tạo các mẫu Word thay thế.

## Kết luận

Khác với bảng chức năng mục III đã cập nhật, **tám biểu mẫu mục IV còn nhiều nội dung không phù hợp với sản phẩm hiện tại**. Cần đồng bộ trường dữ liệu, công thức điểm, trạng thái điểm danh, quyền xem và phạm vi tài chính. Không bổ sung tính năng vào phần mềm chỉ để làm đúng các mẫu này: requirements và các quyết định đã chấp nhận mới là căn cứ nghiệp vụ.

| Mẫu trong Word | Đánh giá | Hướng xử lý trong bản báo cáo mới |
|---|---|---|
| 1. Hồ sơ đăng ký / Form Đăng ký khóa học | Trộn đăng ký tài khoản, đăng ký khóa, lựa chọn lớp và thanh toán; thêm nhiều trường ngoài phạm vi. | Tách mô tả tài khoản/profile và hồ sơ đăng ký một Course; bỏ thông tin phụ huynh/CCCD/ưu đãi/trả góp và quyền chọn lịch lớp khi đăng ký. |
| 2. Danh sách lớp | Có dữ liệu không lưu hoặc vượt scope Teacher; gợi ý bảo lưu/chuyển lớp. | Roster theo role, ID nội bộ/tên/email/phone và thông tin lớp phù hợp; bỏ giới tính/DOB/phụ huynh/bảo lưu/chuyển lớp. |
| 3. Thời khóa biểu | Khung sáng/chiều/tối cố định, chưa thể hiện lịch ngày cụ thể và lịch sử; hứa thông báo Email/Zalo. | Dùng ngày/giờ bắt đầu–kết thúc/phòng/loại buổi/lớp và lịch hiện hành–lịch sử/thay thế; bỏ cam kết gửi tự động. |
| 4. Bảng điểm cá nhân | Thang 10 và hệ số giữa/cuối kỳ cố định không đúng công thức dự án. | Thang 100; components/weights của Program; tổng half-up hai số lẻ, kết quả FINALIZED và đạt từ 50. |
| 5. Bảng điểm danh | Bốn trạng thái có mặt/vắng phép/vắng không phép/đi muộn không đúng mô hình. | Có mặt/Vắng; Chưa ghi là thiếu dữ liệu; một LESSON/save, không tự cảnh báo hoặc ảnh hưởng đạt môn. |
| 6. Biên lai / Phiếu thu điện tử | Không có chức năng hoặc chứng từ riêng tương ứng trong phạm vi sản phẩm. | Loại mẫu phát hành biên lai; nếu giữ vị trí mô tả tài chính, thay bằng màn hình xác nhận Payment của Accountant và ghi rõ không phải phiếu thu được phát hành. |
| 7. Báo cáo tổng hợp tài chính thu chi | Thêm phí tài liệu/chi phí/lương/marketing/lợi nhuận và Director approval. | Báo cáo học phí: doanh thu/phải thu trong kỳ, công nợ/quá hạn hiện tại; tổng hợp theo Course; đúng role/CSV. |
| 8. Hóa đơn học phí | Số INV, nhiều dòng hàng, phí tài liệu/discount/VAT và chữ ký/đóng dấu không đúng bản in ứng dụng. | Một Invoice/Registration, số HD-YYYY-NNNNNNNN, tuition snapshot, issued/due/status; Student in mới qua trình duyệt, không thêm chứng từ thuế/biên lai. |

Không phải mọi ô trống hay phần chữ ký trên mẫu giấy đều là một lỗi phần mềm. Vấn đề là chúng đang khiến người đọc hiểu rằng hệ thống lưu những trường đó, hỗ trợ những nghiệp vụ đó hoặc phát hành chứng từ đó. Cần ghi rõ nếu một tài liệu giấy bên ngoài ứng dụng được giữ làm tham khảo; không trình bày như mẫu đầu vào/đầu ra của sản phẩm hiện tại.

## Nguồn và cách đối chiếu

- File nguồn được đọc trực tiếp từ XML DOCX. SHA-256 lúc bắt đầu: `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`.
- Kiểm lại khi hoàn tất: SHA-256 không đổi; đủ tám tiêu đề mẫu và 15 bảng trong mục IV; các liên kết nguồn trong bản đối chiếu đều tồn tại.
- Mục IV ở các phần tử B100–B224 trong `word/document.xml/w:body`; B225 là tiêu đề V. Bxxx là **chỉ số phần tử XML bắt đầu từ 0**, không phải số trang. Tiêu đề “V. CAM KẾT CỦA HỌC VIÊN / PHỤ HUYNH” tại B123 nằm bên trong mẫu 1, không phải ranh giới kết thúc mục IV.
- Toàn bộ đoạn và bảng của tám mẫu đã được đọc; mục IV không có hình drawing. Đây là kiểm tra nội dung nghiệp vụ, không phải kiểm tra bố cục/in trang Word.
- Chuẩn: [requirements](../requirements.md), [FUNCTION_SPEC](../design/FUNCTION_SPEC.md), [WORKFLOW_SPEC](../design/WORKFLOW_SPEC.md), [E04_DTO_SPEC](../design/E04_DTO_SPEC.md), checkpoint mới nhất [CURRENT_STATE](../CURRENT_STATE.md).
- Đã đối chiếu các UI components/DTO/read projections liên quan được dẫn ở từng mẫu. Không chạy lại SQL, ứng dụng hay kiểm thử trong lượt này.
- Các trường gợi ý bên dưới là lựa chọn từ hợp đồng dữ liệu hiện có. Không mặc định mọi role được xem mọi trường, không cấp thêm quyền in/export hoặc yêu cầu thêm dữ liệu vào schema.

## 1 Hồ sơ đăng ký và form đăng ký khóa học

Vị trí: B101–B127; mã trong nguồn BM-TS-01.

| Nội dung mẫu Word | Hệ thống hiện tại | Đề xuất cho bản mới |
|---|---|---|
| Họ tên “In hoa” | Tên Unicode được chuẩn hóa theo quy tắc tên; không bắt viết hoa toàn bộ. | Ghi Họ và tên; giữ chữ hoa/thường phù hợp với dữ liệu người dùng. |
| Họ tên/DOB/email/phone/address | Là thông tin tài khoản và StudentProfile; address tùy chọn. | Phân biệt form tạo tài khoản với dữ liệu liên hệ được đọc lại trên hồ sơ khóa học. |
| Giới tính, nơi sinh, CCCD/Passport/ngày–nơi cấp, công tác/trường | Không có trường tương ứng trong Student account/profile của dự án. | Bỏ; không tự thêm vào form/schema hoặc yêu cầu cung cấp. |
| Phụ huynh/người bảo lãnh bắt buộc dưới 18 tuổi | Không module phụ huynh, không quy tắc phân luồng tuyển sinh theo tuổi. DOB hợp lệ nhưng không hạn tuổi. | Bỏ khối phụ huynh và điều kiện dưới 18. |
| Hai dòng khóa trong cùng phiếu | Mỗi Registration chọn một Course. Một Student có thể có hồ sơ riêng cho nhiều Course. | Mỗi mẫu hồ sơ chỉ một Course; nếu minh họa nhiều hồ sơ, dùng danh sách các Registration riêng. |
| Lịch học/ngày khai giảng ở lúc đăng ký khóa | Student chọn Course và thi đầu vào trước; Class/Schedule chỉ có sau duyệt, PAID và được Giáo vụ xếp lớp. | Không để Student nhập/chọn lịch lớp ngay trong form tạo Registration. Hiển thị “Chưa xếp lớp” khi chưa có assignment. |
| Học phí gốc/tổng phải đóng/ưu đãi | Giá Course hiện tại chỉ tham khảo trước approval; Invoice lấy giá lúc Admissions duyệt. Không discounts/scholarships. | Phân biệt giá đang niêm yết và tiền Invoice đã phát hành; bỏ ưu đãi và tổng có giảm giá. |
| Thanh toán khác/trả góp/cà thẻ/chia đợt | Thu ngoài hệ thống, đủ một lần; Accountant ghi CASH/TRANSFER. | Tách khỏi form đăng ký; dẫn tới Invoice và hướng dẫn thanh toán khi đã phát hành. |
| Cam kết bảo lưu/hoàn học phí và ba bên ký | Không bảo lưu/refund/nhân viên ghi danh thay; application action lưu actor/time/audit, không chữ ký giấy. | Bỏ chính sách chưa thuộc phạm vi; không coi chữ ký Tuyển sinh/Kế toán là điều kiện tạo hồ sơ Student. |

**Cấu trúc phù hợp:** phần tài khoản/profile gồm họ tên, DOB, email, phone, address tùy chọn; mật khẩu chỉ xuất hiện ở form signup, không được đưa vào hồ sơ kết xuất. Phần tạo Registration chọn một Course ACTIVE. Sau khi tạo, chi tiết hồ sơ có ID, Course, thời điểm/trạng thái, kết quả từng lượt/điểm tốt nhất/Band, thông tin confirmation/approval, Invoice/Class khi đã có và lý do từ chối/hủy theo scope. Đây là nhiều bước nghiệp vụ, không một phiếu chọn khóa–lớp–thu tiền cùng lúc.

Nguồn code: [SignupForm](../../frontend/src/features/identity/components/SignupForm.tsx), [StudentProfileForm](../../frontend/src/features/enrollment/components/StudentProfileForm.tsx), [StudentEnrollmentPage](../../frontend/src/features/enrollment/StudentEnrollmentPage.tsx), [AdmissionsService](../../backend/src/EnglishCenter.Api/Services/Admissions/AdmissionsService.cs). Nguồn nghiệp vụ: requirements ADR-012/019/038/039/040/043–046; W01/W05/W09–W12.

## 2 Danh sách lớp

Vị trí: B130–B139; BM-ĐT-01.

Mã lớp, khóa, giáo viên và danh sách thành viên là đúng hướng. Tuy nhiên, không có giới tính trong dữ liệu Student; Teacher roster không nhận DOB/address. Cột “SĐT (HV/Phụ huynh)” phải là phone của Student, không phone phụ huynh. “Bảo lưu/Chuyển lớp” gợi ý chức năng không có; không có thao tác gỡ/chuyển sau assignment.

Một phòng/ca cố định trong header không thay được Schedule: từng buổi có ngày, start/end và room riêng. Ngày khai giảng không phải trường Class riêng do Student nhập; nếu cần chỉ báo buổi đầu, phải lấy theo lịch và đặt tên rõ.

**Cấu trúc phù hợp:** thông tin lớp gồm Class code, Course/Band/Program, trạng thái, Teacher và sĩ số/sức chứa theo role. Roster Teacher gồm ID Student nội bộ, họ tên, email, phone; có thể trình bày STT như cột hiển thị. Roster AcademicAffairs có thêm Registration, Course/Band/entry score và **Invoice status**, không Invoice amount/Payment. Student không được xem toàn roster của lớp qua quyền này.

Chữ ký Giáo viên/Quản lý đào tạo là bố cục mẫu giấy, không đại diện một workflow ký/phê duyệt danh sách lớp của ứng dụng; cũng không tự tạo quyền staff in/export roster.

Nguồn code: [SqlClassReads](../../backend/src/EnglishCenter.Api/Infrastructure/Classes/SqlClassReads.cs), phương thức Roster/ClassPage; nguồn nghiệp vụ: requirements ADR-023/040/083/085, W14–W15.

## 3 Thời khóa biểu

Vị trí: B141–B147; BM-ĐT-02.

Word dùng ba khung 08:00–10:00, 14:00–16:00, 18:00–20:00 cho cả tuần. Hệ thống không bị giới hạn bởi ba ca này: Schedule có **ngày cụ thể và giờ cụ thể**, minute precision, cùng ngày VN. Ma trận tuần chỉ là cách trình bày; không thể dùng ô “Thứ 2” lặp mãi để thay dữ liệu từng buổi hoặc mặc định những khung giờ minh họa là quy tắc hệ thống.

Mẫu còn thiếu phân biệt LESSON/FINAL_EXAM, PLANNED/HELD/NOT_HELD, lịch hiện hành/lịch sử và liên kết replacement. Dòng Email/Zalo hứa gửi thông báo tự động là ngoài phạm vi; ngày nghỉ không làm hệ thống tự bỏ ngày hay sinh lịch bù.

**Cấu trúc phù hợp:** chọn lớp/khoảng ngày trong scope; mỗi dòng ngày, giờ bắt đầu–kết thúc, Class/Course, loại buổi, room, Teacher hiển thị theo quyền, tình trạng/lịch sử liên quan. Khi NOT_HELD, nêu không tổ chức và liên kết buổi thay thế; không coi cả lịch cũ và replacement là hai buổi học bắt buộc. Desktop có thể tuần, mobile/tablet ưu tiên agenda; không phát sinh email/export/print chỉ vì báo cáo có một “biểu mẫu” lịch.

Nguồn code: [SchedulesPanel](../../frontend/src/features/classes/SchedulesPanel.tsx), [SqlClassReads](../../backend/src/EnglishCenter.Api/Infrastructure/Classes/SqlClassReads.cs) ScheduleView, [ScheduleRules](../../backend/src/EnglishCenter.Api/Services/Classes/ScheduleRules.cs). Nguồn: requirements ADR-035–037/087/088/122, W16.

## 4 Bảng điểm cá nhân

Vị trí: B149–B160; BM-ĐT-03.

Đây là sai lệch công thức rõ ràng: Word ghi **/10**, giữa kỳ hệ số 1/cuối kỳ hệ số 2 và ba nhóm kỹ năng/chuyên cần cố định. Dự án dùng **0–100**, thành phần và trọng số từ TrainingProgram theo Course/Band; không áp công thức cố định 1:2. Không phải chương trình nào cũng có ba nhóm điểm minh họa đó. Component tên chuyên cần chỉ có thể tồn tại khi được cấu hình hợp lệ, không tự biến Attendance thành điểm hoặc điều kiện đạt.

Tổng = Σ(score × weight /100), dùng decimal, không làm tròn từng đóng góp, làm tròn tổng một lần half-up đến hai chữ số. FINALIZED ≥50 đạt; không phân loại Giỏi/Khá/Trung bình hoặc ngưỡng chuyên cần bổ sung. Student chỉ thấy kết quả FINALIZED; nháp/đã gửi/đã trả lại không phải bảng điểm công bố. FINALIZED bất biến, không sửa bằng cách ký lại.

**Cấu trúc phù hợp:** Class/Course/Program và ngày công bố; từng component có tên, kind, weight %, score /100 và lịch liên quan khi được phép; tổng chính thức, Đạt/Chưa đạt, Teacher comment. Trước công bố, form Teacher/review staff là màn hình riêng theo quyền và trạng thái; không dùng cùng một mẫu đọc Student để ngụ ý Student thấy nháp. Không chức năng phát hành/tải phiếu điểm có chữ ký hoặc staff/Student PDF export.

Nguồn code: [StudentGradeResults](../../frontend/src/features/classes/components/StudentGradeResults.tsx), [GradeService](../../backend/src/EnglishCenter.Api/Services/Classes/GradeService.cs), [SqlStudentGradeReads](../../backend/src/EnglishCenter.Api/Infrastructure/Classes/SqlStudentGradeReads.cs). Nguồn: requirements ADR-024/028/034/087/124/125, W18–W19.

## 5 Bảng điểm danh

Vị trí: B163–B172; BM-ĐT-04.

Ký hiệu x/p/k/m của Word tương ứng có mặt, vắng phép, vắng không phép, đi muộn. Hệ thống chỉ lưu **Có mặt (1)/Vắng (0)**; **Chưa ghi** là chưa có row, không một trạng thái vắng thứ ba. Không lưu lý do vắng phép hay loại đi muộn trong Attendance. Ô trống không tự thành Vắng; không mặc định cả lớp Có mặt.

Sáu buổi trong mẫu chỉ là minh họa, không giới hạn số LESSON của Program hoặc đơn vị lưu. Teacher chọn một LESSON hiện hành thuộc lớp đang phụ trách, thao tác từ start đến trước Class COMPLETED; lưu các item thay đổi của một buổi all-or-none, được lưu một phần roster. Học kỳ/tháng không quyết định cửa sổ được nhập. Attendance không tự xác nhận HELD; NOT_HELD giữ lịch sử và buổi replacement mới có attendance hợp lệ.

Các cột “Tổng vắng/Cảnh báo” không thể được coi là chỉ số/luồng tự động hiện có chỉ từ mẫu này. Không có cảnh báo kỷ luật, chặn học viên hoặc điều kiện rớt do số buổi vắng được chấp thuận. Nếu bản báo cáo cần một tổng hợp mới, phải phân biệt đề xuất với màn hình đang có.

**Cấu trúc phù hợp:** Class, LESSON date/start/end/room và trạng thái được phép nhập; từng Student có ID/tên và lựa chọn Chưa ghi/Có mặt/Vắng. Nút Lưu điểm danh buổi; phần chưa ghi hiển thị rõ. Đủ attendance là điều kiện hoàn thành Class, không ngưỡng đạt môn. Chữ ký Teacher không thay actor/time được hệ thống ghi.

Nguồn code: [AttendanceFields](../../frontend/src/features/classes/components/AttendanceFields.tsx), [AttendancePanel](../../frontend/src/features/classes/AttendancePanel.tsx), [AttendanceService](../../backend/src/EnglishCenter.Api/Services/Classes/AttendanceService.cs). Nguồn: requirements ADR-025/033/087/124, W17.

## 6 Biên lai và phiếu thu điện tử

Vị trí: B174–B188; BM-TC-01.

**Không có đầu ra phát hành biên lai/phiếu thu điện tử riêng trong dự án.** Word có số PT, ngày chứng từ, người nộp, địa chỉ, số tiền bằng chữ và hai bên ký. Payment trong hệ thống là bản ghi xác nhận tiền nhận ngoài ứng dụng, gắn một Invoice, không chứng từ PT độc lập. Không sinh số PT, không chức năng kế toán in/xuất phiếu thu.

Payment amount bằng toàn bộ Invoice amount. `paid_at` là giờ server xác nhận, không ngày thu thực tế cho phép nhập lùi. Chỉ CASH hoặc TRANSFER; TRANSFER cần reference. Sửa sau xác nhận chỉ method/reference, có reason/audit; amount/link/time bất biến, không refund/delete hoặc Payment thứ hai.

**Cách thay thế phù hợp:** bỏ mẫu biên lai khỏi danh sách đầu ra ứng dụng. Nếu cần minh họa bước nghiệp vụ, dùng “Thông tin xác nhận thanh toán” dành cho Accountant: Invoice number/Course/contact trong scope, amount đọc từ Invoice, method, reference, confirmer/time sau thành công; reason chỉ khi correction. Ghi rõ đó là giao diện nghiệp vụ, không mẫu chứng từ được phát hành. Không gộp vào bản in Student vốn không được xem Payment method/reference.

Nguồn code: [PaymentFields](../../frontend/src/features/finance/components/PaymentFields.tsx), [FinanceService](../../backend/src/EnglishCenter.Api/Services/Finance/FinanceService.cs), [StudentInvoiceModels](../../backend/src/EnglishCenter.Api/Services/StudentInvoices/StudentInvoiceModels.cs). Nguồn: requirements ADR-041/042/046/083/127, W11–W12.

## 7 Báo cáo tổng hợp

Vị trí: B191–B203; BM-TC-02.

Mẫu “Tài chính thu chi” có phí bán tài liệu, giá trung bình/tỷ trọng thu, lương, điện nước, marketing, văn phòng phẩm, lợi nhuận A−B và Giám đốc phê duyệt. **Hệ thống không quản lý thu chi/lương/chi phí/lợi nhuận hoặc workflow Director approval**. Không lấy các cột này làm bảng dữ liệu cần bổ sung.

**Mẫu tài chính đúng gồm sáu cột:** mã Course, tên Course, doanh thu trong kỳ, phải thu trong kỳ, công nợ hiện tại, công nợ quá hạn. Doanh thu theo Payment confirmation date; phải thu theo Invoice issue date, loại CANCELLED; debt từ Invoice UNPAID hiện tại, không lấy hai tổng trong kỳ trừ nhau. Có tổng toàn bộ tập lọc, không cộng trang; hiển thị bộ lọc áp dụng và queried_at. Các tổng tiền giữ chính xác nguyên VND.

Accountant xem tài chính và xuất CSV toàn kết quả/cột chuẩn; file có header sáu cột và footer tổng, giới hạn 10.000 dòng dữ liệu **gồm footer** và 5.000.000 UTF-8 bytes gồm BOM/header/footer/escape/CRLF. Director chỉ xem aggregates trong ứng dụng, không export hoặc duyệt báo cáo; không thêm chữ ký Kế toán trưởng như một role mới.

Nếu tên “Báo cáo tổng hợp” nhằm bao cả ba nhóm Director, nên minh họa riêng tuyển sinh (Registration/distinct Student), đào tạo (inventory/HELD obligations, FINAL_EXAM riêng, finalized-result pass rate) và tài chính. Không trộn các nhóm thành báo cáo thu chi. Không có kết quả FINALIZED thì tỷ lệ đạt là Chưa có dữ liệu.

Nguồn code: [FinanceReportTable](../../frontend/src/features/reports/components/FinanceReportTable.tsx), [ReportProjection](../../backend/src/EnglishCenter.Api/Services/Reports/ReportProjection.cs), [FinanceCsv](../../backend/src/EnglishCenter.Api/Services/Reports/FinanceCsv.cs), [ReportEndpoints](../../backend/src/EnglishCenter.Api/Api/Reports/ReportEndpoints.cs). Nguồn: requirements ADR-063–072/094/101/126, W20.

## 8 Hóa đơn học phí

Vị trí: B204–B223; BM-TC-03.

| Mẫu Word | Dữ liệu và bản in hiện tại |
|---|---|
| Số INV-…/202… | Server cấp `HD-YYYY-NNNNNNNN`, năm theo ngày VN; unique/không tái dùng, không buộc liên tục. |
| Ba dòng hàng, quantity/unit price, phí tài liệu | Một Invoice cho một Registration/Course; số tiền là tuition snapshot lúc Admissions approval. Không module hàng hóa/phí tài liệu. |
| Subtotal/discount/VAT/total | Một amount dương, nguyên VND; không tính discount/VAT hoặc tổng nhiều dòng. |
| Địa chỉ Student và mã số thuế cá nhân/DN | StudentInvoiceDetail không có address/tax ID; không thêm trường chỉ để khớp mẫu. |
| MST/địa chỉ trung tâm | Không phải cấu hình/field chứng từ đã có trong bản in hiện tại; không tự điền thông tin giả. |
| Tiền bằng chữ | Bản in hiện tại hiển thị exact amount VND, không chức năng chuyển tiền thành chữ. |
| Hai bên ký/đóng dấu | Không workflow chữ ký/dấu hoặc chứng từ thuế trong ứng dụng; không mô tả bản in có tính năng này. |
| Không có due date/status rõ | Bản hiện tại có issuedAt, dueDate, UNPAID/PAID/CANCELLED và overdue indicator. UNPAID phải ghi chưa thanh toán, không xác nhận đã thu tiền. |

**Mẫu phù hợp với bản in đang có:** Invoice number, tên hiện tại của Student, Course code/name, amount snapshot, thời điểm phát hành, due date, trạng thái và số ngày quá hạn khi có. Student sở hữu bấm Đọc mới và in hóa đơn; ứng dụng fetch lại trước `window.print()`, A4, ẩn menu/nút. Không thêm Student ID/address/Payment reference/actor vào bản in nếu DTO/UI hiện tại không có, không mở staff print/export rights.

Tên Student lấy dữ liệu hiện tại; thay tên profile có thể đổi tên trên Invoice đọc/in sau đó, nhưng amount snapshot vẫn giữ. PAID là trạng thái thật sau Accountant confirmation; bấm in không tạo Payment hoặc chuyển PAID. Chức năng Save as PDF của trình duyệt khác API xuất PDF ứng dụng.

Nguồn code: [StudentInvoicesPage](../../frontend/src/features/invoices/StudentInvoicesPage.tsx), [StudentInvoiceModels](../../backend/src/EnglishCenter.Api/Services/StudentInvoices/StudentInvoiceModels.cs), [AdmissionsService](../../backend/src/EnglishCenter.Api/Services/Admissions/AdmissionsService.cs). Nguồn: requirements ADR-039/041/043–047/090/127, W11.

## Những mẫu chưa được minh họa trong mục IV

Bảng chức năng mục III còn tham chiếu “Lịch và kết quả thi đầu vào” và “Nhật ký hệ thống”, nhưng tám mẫu mục IV không có mẫu riêng tương ứng. Đây là **thiếu minh họa trong báo cáo**, không thiếu tính năng phần mềm. Nếu muốn đồng bộ tên Biểu mẫu giữa III và IV, có thể chuẩn bị thêm ở file mới:

- Lịch/booking/kết quả entry test theo role: Registration, attempt number, booking history/status, session start/end/room/effective state, completed score/best/Band; absence khác 0. Teacher/Academic context khác Student-own.
- Audit viewer Admin: event time, verified actor hoặc Chưa xác thực, action/entity/result/reason, allowlisted changes và operation grouping; không secrets/raw payload/export.

Không suy mọi chức năng trong danh mục phải có một mẫu giấy riêng. Signup, profile edit, confirmation, Teacher draft và Accountant payment là các form/thao tác UI, không mặc định chứng từ có số mẫu/chữ ký để in.

## Thứ tự đồng bộ báo cáo đề xuất

Ưu tiên sửa mẫu 4 và 5 vì đang mâu thuẫn trực tiếp với điểm/Attendance; mẫu 6 và 7 vì mô tả nghiệp vụ ngoài phạm vi; mẫu 8 vì không giống Invoice/bản in; rồi mẫu 1–3 để đúng dữ liệu, quyền và trình tự. Có thể giữ mã BM hiện có để truy vết nếu đổi nội dung, nhưng phải ghi rõ loại bỏ/thay mục 6 và đồng bộ tên tham chiếu ở mục III trong **bản báo cáo mới**.

Các phát hiện ở đây không sửa requirements, code, schema, Word nguồn hay những tài liệu owner đang có. Tình trạng M4 recovery inputs/UI gate4 vẫn giữ theo checkpoint mới nhất; lượt đối chiếu biểu mẫu không đóng các gate đó.
