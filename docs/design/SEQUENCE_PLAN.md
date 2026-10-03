# Quy hoạch sequence diagram nghiệp vụ

Ngày: 02/10/2026. Trạng thái: đề xuất quy hoạch theo yêu cầu owner, chưa duyệt
số lượng/cách chia sơ đồ. Owner tự vẽ bằng Enterprise Architect; agent cung cấp
thông tin và giải đáp theo yêu cầu, không sinh diagram hoặc mã vẽ.

Nguồn: FUNCTION_SPEC E01-v1, WORKFLOW_SPEC E02-v1 và requirements/ADRs đã
duyệt; DATABASE_SPEC E03-v1 dùng đối chiếu đối tượng/ràng buộc. Không thay
quyết định nghiệp vụ, không nghiệm thu E04/E05; checkpoint DTO D5 giữ nguyên.

## Phạm vi và cách sử dụng

- Mỗi SQ là một mục quy hoạch, không là mã chức năng mới hoặc số lượng hình
  bắt buộc. Các nhánh dài có thể tách thành sơ đồ con cùng mã, không thêm nghiệp vụ.
- Chỉ mô tả tương tác nghiệp vụ: actor, yêu cầu, kiểm tra điều kiện, xử lý và
  kết quả. Hệ thống có thể là một lifeline; đối tượng nghiệp vụ chỉ thêm khi
  giúp diễn đạt. Không cần Controller/Service/Repository/API/SQL.
- Những vai trò tham gia ở các thời điểm khác nhau không được ghép thành một
  cuộc gọi đồng bộ hoặc thông báo tự động không có trong yêu cầu.
- alt dùng cho lựa chọn/kết quả/ngoại lệ; opt cho bước tùy chọn; loop chỉ khi
  nghiệp vụ thật sự lặp. Ghi điều kiện và hiệu ứng all-or-none bằng chú thích
  nghiệp vụ; không cần mô tả lock, HTTP status hay cơ chế DB.
- Xem danh sách/chi tiết liên quan đặt đầu hoặc cuối sequence xử lý. Không
  phải mỗi màn hình hay nút đều cần một sơ đồ riêng. Profile/read/report độc
  lập vẫn được quy hoạch dưới đây để đủ phạm vi.

## Danh mục đề xuất

| Mã | Sequence / phạm vi | Actor chính | Chức năng liên quan | Nguồn |
| --- | --- | --- | --- | --- |
| SQ01 | Student tự đăng ký tài khoản; thành công hoặc trùng/sai dữ liệu | Khách đăng ký | NV01 | W01 |
| SQ02 | Admin cấp tài khoản nhân sự; bàn giao mật khẩu tạm một lần | Admin | NV33 | W01 |
| SQ03 | Đăng nhập/đăng xuất; tài khoản hoặc mật khẩu không hợp lệ; phiên hạn chế | Mọi vai trò | NV01 và thao tác chung | W02 |
| SQ04 | Tự đổi mật khẩu, gồm bắt đổi mật khẩu tạm; đăng nhập mới sau đổi | Chủ tài khoản | Thao tác chung | W02 |
| SQ05 | Reset mật khẩu; khóa/mở/ngừng dùng/khôi phục tài khoản | Admin | NV33–NV34 | W03 |
| SQ06 | Xem/sửa hồ sơ cá nhân; xem trang chủ và trạng thái hồ sơ đúng phạm vi | Student | NV02–NV08 (phép chiếu), thao tác chung | W04 |
| SQ07 | Chọn khóa, tạo hồ sơ đăng ký; Student hủy hồ sơ hợp lệ | Student | NV02 | W05 |
| SQ08 | Tạo/sửa/mở/đóng/hủy ca thi, đổi giáo viên chấm; xem ca được phân công | Giáo vụ; Teacher khi tra cứu | NV14–NV15, NV21 | W06 |
| SQ09 | Đặt/xem/hủy/đổi ca; thi lần hai; đặt lại cùng lượt sau vắng/hủy ca | Student | NV03 | W07 |
| SQ10 | Nhập điểm hoặc ghi vắng; sửa điểm có lý do; tính lại Band; xem kết quả | Teacher; Student khi tra cứu | NV04, NV15, NV22, NV36 | W08 |
| SQ11 | Student xác nhận nhập học; chặn khi còn lượt thi chờ xử lý | Student | NV05 | W09 |
| SQ12 | Tuyển sinh duyệt/từ chối/hủy hồ sơ; duyệt đồng thời phát hành hóa đơn | Tuyển sinh | NV09, NV12, NV37 | W10–W11 |
| SQ13 | Tra cứu hóa đơn; Student xem/in hóa đơn của mình | Student, Kế toán | NV06, NV27 | W11 |
| SQ14 | Xác nhận thu đủ tiền; sửa phương thức/mã giao dịch với lý do | Kế toán | NV28 | W12 |
| SQ15 | Quản lý khóa, Band, chương trình và cấu hình điểm; kích hoạt/khóa sửa theo sử dụng | Giáo vụ | NV13 | W13 |
| SQ16 | Tạo/mở/bắt đầu/hoàn tất/hủy lớp; đổi giáo viên; tra cứu lớp được phân công | Giáo vụ; Teacher khi tra cứu | NV16–NV17, NV23 | W14 |
| SQ17 | Xếp học viên đủ điều kiện vào lớp; tạo bảng điểm DRAFT cùng thao tác | Giáo vụ | NV18 | W15 |
| SQ18 | Lập lịch/preview; dời/thay buổi; xác nhận đã tổ chức; xem lịch đúng phạm vi | Giáo vụ; Student/Teacher khi tra cứu | NV07, NV19, NV23 | W16 |
| SQ19 | Ghi/sửa điểm danh một buổi LESSON | Teacher | NV24 | W17 |
| SQ20 | Lưu điểm thành phần và nhận xét của một học viên; tính tổng tạm | Teacher | NV25–NV26, NV38 | W18 |
| SQ21 | Gửi/gửi lại bảng điểm cả lớp; kiểm tra đủ điều kiện toàn lớp | Teacher | NV25–NV26 | W18 |
| SQ22 | Trả lại hoặc chốt/công bố bảng điểm cả lớp; Student xem kết quả đã chốt | Giáo vụ; Student khi tra cứu | NV08, NV20, NV38 | W19 |
| SQ23 | Xem báo cáo tài chính và xuất CSV theo bộ lọc | Kế toán | NV29 | W20 |
| SQ24 | Xem báo cáo tổng hợp tuyển sinh/đào tạo/tài chính, không định danh học viên | Giám đốc | NV30–NV32 | W20 |
| SQ25 | Tra cứu nhật ký, chi tiết và các sự kiện cùng thao tác | Admin | NV35; tham chiếu NV39 | W21 |

Danh mục bao phủ 37 NV còn hiệu lực; NV10/NV11 loại, không phục hồi.
NV36 tích hợp SQ10; NV37 tích hợp SQ12; NV38 tích hợp SQ20/SQ22.
NV39 là hiệu ứng xuyên suốt tại các thao tác cần audit, không chỉ SQ25 và
không là actor thứ tám. SQ25 là tra cứu; ghi audit không đòi Admin tham gia.

## Quan hệ và những ranh giới cần giữ khi vẽ

- Hành trình chính: SQ07 → SQ09 → SQ10 → SQ11 → SQ12 → SQ14 → SQ17;
  lịch/lớp chuẩn bị qua SQ15/SQ16/SQ18. Đây là quan hệ trước–sau giữa nhiều
  thao tác, không một giao dịch xuyên toàn hành trình.
- Học và kết quả: SQ19/SQ20 trong giai đoạn cho phép → hoàn tất lớp ở SQ16
  → SQ21 → SQ22. Lưu nháp có thể tiếp tục sau lớp COMPLETED khi report còn
  DRAFT/RETURNED. Nhánh RETURNED → SQ20 → SQ21 → SQ22 có thể lặp.
- SQ10: ABSENT không là điểm 0; đặt lại cùng lượt không tiêu hao lượt mới.
  Tối đa hai lượt; đồng điểm chọn lượt một; CONFIRMED khóa thi lại/sửa điểm.
- SQ11 chưa tạo hóa đơn. SQ12 duyệt + ADMITTED + Invoice UNPAID đồng thời;
  SQ14 xác nhận tiền đã nhận bên ngoài ứng dụng, không gọi cổng thanh toán.
- SQ17 cần ADMITTED, PAID, lớp OPEN đúng Course/Band, còn chỗ và không trùng
  lịch; tạo assignment + DRAFT report cùng thao tác, Registration vẫn ADMITTED.
- SQ18: preview không giữ chỗ; một xung đột chặn cả batch lịch tuần.
  Buổi thay thế giữ cùng nghĩa vụ; HELD do Giáo vụ xác nhận sau giờ kết thúc.
- SQ19: từ LESSON start đến trước Class COMPLETED; chưa ghi khác Vắng.
- SQ20 lưu theo một học viên; SQ21/SQ22 chuyển trạng thái cả lớp. Student
  chỉ thấy FINALIZED; FINALIZED không mở lại. Tổng chốt /100, half-up một
  lần hai số lẻ, đạt từ 50; kết thúc hồ sơ không phụ thuộc đạt hay không.
- SQ23 và SQ24 tách quyền: Kế toán có CSV tài chính; Giám đốc chỉ tổng hợp,
  không xuất hoặc phê duyệt. SQ25 không sửa/xóa/export nhật ký.
- Audit bắt buộc thất bại thì hoàn tác thao tác liên quan; nhật ký thuần bảo
  mật có quy tắc best-effort riêng. Không mặc định audit mọi phép xem.

## Cách hỗ trợ các lượt tiếp theo

Owner tiếp tục yêu cầu SQ15/SQ23: có
[SQ15 cấu hình đào tạo](SQ15_TRAINING_CONFIGURATION_GUIDE.md) và
[SQ23 báo cáo tài chính/CSV](SQ23_FINANCIAL_REPORT_CSV_GUIDE.md), với bố cục
đường gạch và mẫu hướng dẫn hiện có. SQ15 tách Course/DRAFT/activation,
Band tham chiếu không editor; SQ23 tách query báo cáo và query mới cho CSV.

Owner yêu cầu tiếp SQ05/SQ13: có
[SQ05 Admin quản lý tài khoản](SQ05_ACCOUNT_ADMINISTRATION_GUIDE.md) và
[SQ13 tra cứu/in hóa đơn](SQ13_INVOICE_LOOKUP_PRINT_GUIDE.md), cùng mẫu
thành phần/thông điệp/guards/note/checklist và phác thảo đường gạch.
SQ05 tách reset/bàn giao secret một lần và action trạng thái; SQ13 tách
Student xem/in với Accountant tra cứu, không mở rộng quyền in cho nhân sự.

Owner tiếp tục yêu cầu SQ07/SQ11: có
[SQ07 đăng ký khóa/hủy hồ sơ](SQ07_COURSE_REGISTRATION_GUIDE.md) và
[SQ11 xác nhận nhập học](SQ11_ENROLLMENT_CONFIRMATION_GUIDE.md), gồm bảng
thông điệp, guard/note/checklist và bố cục đường gạch. SQ07 tách tạo/hủy;
SQ11 không tạo hóa đơn hoặc duyệt tuyển sinh. Không đổi nghiệp vụ/gate E04.

SQ01 có [hướng dẫn vẽ thủ công](SQ01_REGISTRATION_GUIDE.md) theo yêu cầu
owner: actor/boundary/control, thông điệp, guards, note và phác thảo văn bản.
Chưa có hình diagram hoặc thay đổi nghiệp vụ.

Theo yêu cầu tiếp theo của owner, SQ03 có
[hướng dẫn đăng nhập/đăng xuất](SQ03_LOGIN_LOGOUT_GUIDE.md) và SQ04 có
[hướng dẫn tự đổi mật khẩu](SQ04_PASSWORD_CHANGE_GUIDE.md). SQ03 đề nghị tách
hình đăng nhập/đăng xuất; SQ04 giữ hai điểm vào thường/hạn chế. Những mục
này chỉ cụ thể hóa quy tắc đã duyệt để owner vẽ thủ công, không tạo hình.

Khi owner chọn SQ cần vẽ hoặc hỏi, cung cấp đúng phạm vi: mục tiêu, actor/
lifeline gợi ý, điều kiện trước, thứ tự thông điệp nghiệp vụ, nhánh và trạng
thái sau; đối chiếu E01/E02/requirements và quyết định đã duyệt. Có thể viết
hướng dẫn Markdown/phác thảo văn bản khi owner yêu cầu. Không tự triển khai
tất cả 25 mục thành đặc tả chi tiết, không tạo hình/mã diagram.
