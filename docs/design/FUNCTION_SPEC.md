# E01 — Đặc tả chức năng để chủ dự án nghiệm thu

Phiên bản: E01-v1, ngày 29/09/2026. Trạng thái: **owner duyệt 30/09/2026**.
Không phải bằng chứng chức năng đã triển khai hoặc kiểm thử đã chạy.

Nguồn chuẩn: [requirements](../requirements.md), [ADR-001–ADR-129](../DECISIONS.md).
Truy vết báo cáo: [FUNCTION_CATALOG](FUNCTION_CATALOG.md) và
[REPORT_BASELINE](../reference/REPORT_BASELINE.md). Word và biểu mẫu minh họa
không được chỉnh sửa hoặc dùng để thêm yêu cầu.

Tài liệu này cụ thể hóa các quyết định đã duyệt thành actor, dữ liệu vào/ra,
quyền xem/sửa, điều kiện và validation của từng chức năng. Không tạo quyết
định sản phẩm mới, vai trò mới, mã NV/F mới hoặc quyền ngoại lệ.
Owner đã xác nhận đồng ý E01-v1 ngày30/09/2026; ô E01 đã được tích.
E02 sẽ mô tả đầy đủ đường đi/transaction/race; E03 dictionary/ERD; E04 DTO/API
và vận hành; E05 rà nhất quán toàn baseline. D12/D13 vẫn hoãn.

## 1. Phạm vi và cách đọc

- Giữ 39 mã NV nguồn: NV10/NV11 bị loại, 37 mục còn trong phạm vi.
  NV36–NV39 là xử lý do hệ thống thực hiện, không là vai trò tài khoản thứ tám.
- Giữ F01–F33, loại F15: 32 mã màn hình, không đánh lại số.
  Trang chi tiết/tab/action dùng mã F hiện có; nhiều URL không tạo mã F mới.
- Mỗi chức năng dưới đây có actor/module, đầu vào, đầu ra/phạm vi xem,
  quyền/điều kiện, validation/ảnh hưởng và nguồn quyết định.
  ID chọn đối tượng là ID nội bộ có kiểm tra quyền, không phải trường để
  người dùng tự tạo quan hệ hoặc khai báo actor/student_id tùy ý.
- Các mục chung ở phần 8 không có mã NV/F mới. “Chưa ghi”, “Chưa đủ điểm”,
  “Đã xếp lớp”, “Quá hạn” là chỉ báo hiển thị, không enum mới.
- “Nháp” không có nghĩa được lưu trong browser; chỉ nút Lưu rõ mới gửi dữ liệu.
  Quyền quyết định tại server; ẩn/disabled/menu không thay authorization.
- Không có học bổng,/10, ghi danh/đặt ca thay, thi trực tuyến, cổng thanh toán,
  trả góp, hoàn tiền, chuyển lớp, buổi LESSON thêm, học bù cá nhân, lịch nghỉ
  tự động, mở lại bảng điểm FINALIZED, RBAC động hoặc phê duyệt bởi Director.

### 1.1. Actor và trách nhiệm module

| Actor | Phạm vi chức năng |
| --- | --- |
| Student | NV01–NV08; hồ sơ/tài khoản của mình; không nhập điểm hoặc xác nhận tiền. |
| Admissions | NV09/NV12; tra và xử lý hồ sơ theo trạng thái; không sửa hồ sơ cá nhân/ghi danh thay. |
| AcademicAffairs | NV13–NV20; ca, cấu hình, lớp/lịch/xếp và duyệt điểm; không nhập/sửa điểm thay Teacher. |
| Teacher | NV21–NV26; chỉ ca/lớp hiện được phân công; không DOB/địa chỉ/tài chính hoặc Student toàn trung tâm. |
| Accountant | NV27–NV29; Invoice/Payment, báo cáo tài chính và CSV đúng phạm vi. |
| Director | NV30–NV32; chỉ số/nhóm không định danh, không export hoặc phê duyệt. |
| Admin | NV33–NV35; tài khoản và nhật ký, không override nghiệp vụ/điểm/thu tiền. |

Một tài khoản có đúng một role cố định. Cùng dữ liệu có các phép chiếu khác nhau:
Admissions được tiền/trạng thái Invoice nhưng không Payment; AcademicAffairs
chỉ trạng thái Invoice, không tiền/Payment; Teacher không tài chính; Student
chỉ dữ liệu của mình, điểm đào tạo chỉ FINALIZED. Không mở quyền xem từ việc
có liên kết, tìm kiếm, preview, lịch hoặc thông báo lỗi (ADR-083/084/096).

Module theo ADR-095: Students quản lý hồ sơ cá nhân; Admissions quản lý đăng
ký/thi đầu vào/Band; Academics quản lý cấu hình/lớp/lịch/điểm; Finance quản lý
Invoice/Payment; Reports đọc báo cáo; Administration quản lý auth/tài khoản/
phiên/audit. Tự đăng ký Student liên kết tạo Account và StudentProfile, không
cấp quyền đăng ký nhân sự; thao tác liên module cùng DB vẫn giữ all-or-none.

### 1.2. Validation và hành vi chung — áp dụng cho mọi chức năng

| Nhóm | Hợp đồng áp dụng |
| --- | --- |
| Họ tên | Trim; 1–100 Unicode code point; cho dấu/khoảng trắng/dấu câu trong tên; không bắt hai từ. |
| Email | Khi tạo: trim, một ASCII local@domain, tối đa 254, không tên hiển thị/control/khoảng trắng bên trong; unique toàn Account không phân biệt hoa-thường; không gộp dấu chấm/+tag. Email hiện có bất biến. |
| SĐT | Student bắt buộc; nhân sự không mặc định bắt buộc. Nhận di động VN 10 số bắt đầu 0 hoặc +84 tương đương/khoảng trắng/gạch; chuẩn 0xxxxxxxxx, unique toàn Account khi có. Không OTP/đăng nhập SĐT. |
| DOB/địa chỉ | Chỉ Student: DOB thật, không sau ngày Việt Nam hiện tại, không điều kiện tuổi; địa chỉ tùy chọn trim ≤255. Không tự bắt nhân sự có DOB/địa chỉ. |
| Mật khẩu | 15–128 code point, giữ nguyên Unicode/khoảng trắng, không trim/NFC/case đổi; không bắt composition/đổi định kỳ; blocklist offline ADR-114, không gửi ra dịch vụ ngoài. |
| Tên danh mục | Course/program 1–150, component 1–100 sau trim; Unicode. Component name unique trong program, không phân biệt hoa-thường nhưng phân biệt dấu; không global unique tên Course/program. |
| Mã Course/Class/phòng | Trim/uppercase, 1–30 A–Z/số/_/gạch; không dấu/khoảng trắng. Course/Class unique kể cả lịch sử và bất biến; phòng dùng một mã nhất quán cho mọi kiểm tra lịch, không danh mục phòng mới. |
| Tiền | VND nguyên đồng, >0 và ≤999999999999999999; không cắt/round phần lẻ. Invoice snapshot; Payment phải bằng Invoice. Tổng chính xác; tiền transport dạng chuỗi, không mất chính xác ở UI. |
| Điểm/trọng số | Điểm 0–100, weight >0–100, tối đa hai số thập phân; không làm tròn đầu vào sai. Tổng weight đúng 100 để ACTIVE; tổng điểm decimal, half-up một lần hai số lẻ, không từng đóng góp. |
| Lý do/nhận xét | Plain text Unicode trim/giữ xuống dòng; lý do 1–1000 chỉ nơi đã bắt buộc; nhận xét tùy chọn ≤1000. Không HTML/rich text hoặc bắt lý do mới. |
| Payment reference | Trim ≤100, giữ case/nội dung; chuyển khoản bắt buộc, tiền mặt có thể trống; không unique toàn hệ thống/không là dedup key. |
| Ngày/giờ | Ngày nghiệp vụ Asia/Ho_Chi_Minh, độc lập thiết bị. Lịch cùng ngày/phút, start < end, khoảng [start,end) cho phép nối sát giờ. Khoảng ngày lọc phải hợp lệ, không đảo đầu/cuối; không tự thêm trần kỳ lọc mới. |
| Văn bản/ô trống | Đếm code point, từ chối surrogate sai; NFC văn bản không phải mật khẩu. Chuỗi tùy chọn trim rỗng thành NULL; trường bắt buộc rỗng lỗi, không tự cắt dữ liệu. Ô điểm trống không là 0 hoặc lệnh xóa điểm. |
| ID/quan hệ | Kiểm tra tồn tại, role/status hiện tại, ownership và cùng hồ sơ/lớp/program/type; không chỉ FK tồn tại. Người thao tác/thời gian/trạng thái/tổng/giá snapshot do server xác định. |
| Danh sách | Phân trang 20 mặc định/tối đa 100, sort ổn định có ID phụ; không infinite scroll. Tên/email/SĐT tìm kiếm chỉ working memory, không URL/localStorage/sessionStorage. |
| Lưu/lỗi | Lưu rõ, không autosave/merge/ghi đè form cũ; cảnh báo sửa chưa lưu. Loading/no-data/no-match/submitting/field errors; 401 hết phiên, 403 sai role, 404 không có/ngoài scope, 409 dữ liệu/trạng thái tranh chấp. Login trước xác thực dùng lỗi chung. |
| Ghi quan trọng | Preview đúng scope không giữ chỗ; server recheck. Request lặp không tạo hiệu ứng mới theo ADR-093, khác nội dung 409; quyền hiện tại vẫn kiểm tra. Không lưu/replay mật khẩu tạm; mất phản hồi kiểm tra trạng thái, không tự thử lại để lấy secret. |
| Audit | Theo phạm vi NV39/ADR-058–062/093/115/116; không tự audit mọi page read hoặc thêm quyền export/purge. Mandatory audit thất bại rollback mutation liên quan. |
| Trình bày | Việt ngữ/nhãn có chữ, dd/MM/yyyy/HH:mm/vi-VN, tiền đúng; 375/768/1280, keyboard/focus/non-color, component cục bộ, không library mới. |

Nguồn validation: ADR-019/028/043–047/053/073–082/090–094/096/099/101–102/
105–113/114–116. Hợp đồng chung được tham chiếu trong từng mục, không lặp
một policy khác theo role. Các kiểm tra ở đây là yêu cầu thiết kế, chưa test pass.

## 2. Student — NV01–NV08

### NV01 — Tự đăng ký Student và đăng nhập

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Chưa đăng nhập: tự tạo Student. Đăng nhập dùng chung 7 role; Administration + Students; F01/F02. |
| Đầu vào | Tạo: họ tên, DOB, email, SĐT, password, địa chỉ tùy chọn. Login: email/password; không nhận role do client chọn hoặc đăng nhập SĐT. |
| Đầu ra/phạm vi xem | Tạo Account Student/StudentProfile của chính người đăng ký; không tài khoản nhân sự. Login trả phiên đúng role hoặc phiên hạn chế bắt đổi mật khẩu tạm, không lộ hash/token bí mật qua giao diện. |
| Quyền/điều kiện | Account truy cập phải ACTIVE, không bị throttle, credential hợp lệ. Temporary password phải còn hạn; chỉ đổi/logout ở phiên hạn chế. Fresh normal login vào menu đầu role theo ADR-104. |
| Validation | Áp validation tài khoản phần 1.2; email/SĐT trùng bị chặn cả khi concurrent. Sai 5 lần/15 phút chặn login mới 15 phút, không LOCKED/revoke phiên cũ. IP100/phút→429/Retry-After, không tin proxy tùy ý. |
| Ảnh hưởng/giới hạn | Phiên thường tối đa2h/idle30min, phiên hạn chế tối đa10min/capped temp expiry; không refresh/remember-me. Nhiều profile/device độc lập, tab cùng profile chung phiên; idle chỉ request nghiệp vụ có thao tác người dùng, không typing/polling. Lỗi trước xác thực chung, không tiết lộ email/status; không email verification/OTP/recovery. |
| Nguồn | ADR-019/048–057/073–077/091–092/097–100/104/114. |

### NV02 — Xem khóa, tạo và hủy hồ sơ của mình

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Admissions quản lý Registration, Students cung cấp hồ sơ; F04/F05, thẻ F03. |
| Đầu vào | Chọn Course đang ACTIVE để tạo; chọn Registration của mình để xem/hủy. Không nhập Band, status, student_id khác hoặc sửa Course của hồ sơ cũ. |
| Đầu ra/phạm vi xem | Thông tin khóa được chọn và Registration riêng: khóa, trạng thái/bước tiếp theo, lý do từ chối chủ hồ sơ được xem, liên kết thi/Invoice/lớp/kết quả đúng scope. Hủy hợp lệ trả CANCELLED và ảnh hưởng phụ thuộc; giữ lịch sử. |
| Quyền/điều kiện | Một hồ sơ chưa kết thúc/Student/Course. Kết thúc khi CANCELLED/REJECTED hoặc Class COMPLETED và GradeReport của lượt học FINALIZED, không cần đạt. Có thể học khóa khác đồng thời. Student chỉ hủy trước CONFIRMED; không paid cancellation/refund. |
| Validation | Course ACTIVE lúc tạo; kiểm predicate gốc/is_unfinished và chống tạo đồng thời. Course của Registration bất biến: muốn đổi phải hủy hợp lệ/tạo mới, thi lại từ đầu, không copy Band/điểm. Tên hiện tại, không snapshot hồ sơ cá nhân. |
| Ảnh hưởng/giới hạn | Preview hủy liệt kê booking tương lai bị ảnh hưởng đúng scope. Hủy Registration + Invoice UNPAID nếu có + booking trước start kể cả CLOSED cùng thao tác; giữ lịch đã bắt đầu/điểm/Band, ngừng nhập/sửa/vắng trên hồ sơ hủy. Không tự chuyển ca/lớp, không mở lại hồ sơ cũ. Thẻ F03 là phép chiếu, không state mới. |
| Nguồn | ADR-015–017/021–022/031/038–039/083–085/090/093/121. |

### NV03 — Đặt, xem, hủy, đổi ca đầu vào/thi lại

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Admissions; F06/F07, liên kết hồ sơ F05. |
| Đầu vào | Registration của mình, lượt chưa hoàn thành và EntryTestSession đích. Hủy/đổi chọn booking hiện tại; không tự tạo attempt_number thứ ba, không nhân viên đặt thay. |
| Đầu ra/phạm vi xem | Booking/lịch cụ thể và trạng thái hiệu lực ca, kết quả lịch sử hủy/vắng; khả năng đặt lại đúng lượt. F07 dùng lịch tuần/agenda đúng scope, không lịch/danh sách Student khác. |
| Quyền/điều kiện | Đích OPEN/chưa start/còn chỗ, hồ sơ chưa CONFIRMED. Lượt hai cần điểm COMPLETED lượt một; tối đa2 lượt/hồ sơ. Hủy/đổi chỉ khi ca nguồn OPEN, trước start và trước CONFIRMED; CLOSED không được đổi/hủy/đặt mới. |
| Validation | Một current SCHEDULED booking/lượt chưa hoàn thành; check capacity1–20, trùng Student với ca/lịch lớp, thời gian/role tài nguyên. Ca đủ chỗ vẫn OPEN; start inclusive làm effective CLOSED dù dữ liệu cũ OPEN. Không đoán ABSENT từ thiếu điểm. |
| Ảnh hưởng/giới hạn | Đổi/hủy/đặt mới giữ lịch sử và giải phóng/chiếm chỗ nhất quán, không tạo hiệu ứng lặp/partial. ABSENT hoặc center-cancelled cho tự đặt lại cùng lượt, không tăng attempt; ca đã bắt đầu không được hủy để né chờ chấm. Không mở lại ca hoặc auto chuyển Student. |
| Nguồn | ADR-008/021/027/030–031/086/088/090/093/122. |

### NV04 — Xem kết quả đầu vào và Band

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Admissions; F08. |
| Đầu vào | Registration của mình, lựa chọn xem lượt/kết quả thuộc hồ sơ đó. |
| Đầu ra/phạm vi xem | Điểm0–100 của từng lượt COMPLETED, booking/lịch sử liên quan, lượt tốt nhất và Band hiện tại. ABSENT/chưa có kết quả ghi rõ, không điểm0 giả; hồ sơ hủy chưa chấm ghi đúng chỉ báo. |
| Quyền/điều kiện | Read-only own record; không sửa điểm/Band, không thấy kết quả Student khác. Kết quả đầu vào không bị nhầm với điểm đào tạo chỉ FINALIZED. |
| Validation | Best chỉ lấy COMPLETED cùng Registration; đồng điểm ưu tiên attempt1. Ngưỡng lớn nhất ≤điểm, seed0/25/45/65/85; không rounding điểm đầu vào thành số nguyên. |
| Ảnh hưởng/giới hạn | Sửa hợp lệ bởi Teacher ở NV22/NV36 làm kết quả hiển thị cập nhật; xác nhận/hủy/từ chối giữ freeze. Không xóa lịch sử hoặc copy sang hồ sơ đăng ký lại. |
| Nguồn | ADR-008–009/028–031/086/090–091. |

### NV05 — Xác nhận nhập học

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Admissions; F09. |
| Đầu vào | Registration của mình đang BAND_ASSIGNED; lệnh xác nhận, không nhập Band/timestamp hoặc giá học phí tự chọn. |
| Đầu ra/phạm vi xem | CONFIRMED/thời điểm server và thông tin đã xác nhận; lúc này chưa là ADMITTED/PAID/xếp lớp. |
| Quyền/điều kiện | Có Band hợp lệ và chưa kết thúc. Chặn nếu lượt hai đang đặt hoặc đã thi chưa chấm, kể cả ca CLOSED; chờ kết quả hoặc hủy booking hợp lệ trước start từ OPEN. |
| Validation | Kiểm lại score/Band/best attempt/trạng thái/booking lúc xác nhận; chặn race chấm sửa/đặt/hủy. Sau CONFIRMED không đăng ký retake hoặc sửa đầu vào/Band; hồ sơ cá nhân riêng vẫn sửa theo phần8. |
| Ảnh hưởng/giới hạn | Ghi xác nhận theo actor/thời gian; không tạo Invoice hoặc tự duyệt. Giữ Course bất biến, không đóng băng toàn bộ personal profile. |
| Nguồn | ADR-016/021/029–031/038/090/093. |

### NV06 — Xem và in Invoice của mình

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Finance; F10. |
| Đầu vào | Registration/Invoice của mình; lệnh in lấy lại dữ liệu có quyền. Hướng dẫn thanh toán là thông tin trình bày, không payment gateway. |
| Đầu ra/phạm vi xem | Số Invoice, tên chủ hồ sơ hiện tại, khóa, tiền snapshot, ngày phát hành/hạn, UNPAID/PAID/CANCELLED, cảnh báo/số ngày quá hạn. Không tự thêm Payment method/reference hoặc dữ liệu nhân sự ngoài scope. |
| Quyền/điều kiện | Chỉ sở hữu; read/browser-print, không ghi PAID, sửa amount/due_date hoặc xuất PDF riêng. Lấy mới lỗi/mất quyền thì không in bản cũ như mới. |
| Validation | Tiền VND nguyên đồng; due=ngày phát hành VN+7 ngày lịch. Quá hạn chỉ khi UNPAID và due<trước ngày VN hiện tại; ngày due chưa quá hạn. |
| Ảnh hưởng/giới hạn | A4 dọc/đen trắng rõ, ẩn chrome/actions. UNPAID ghi không phải xác nhận thu tiền; CANCELLED ghi hủy, PAID đúng dữ liệu. Không biên lai/stamp/chữ ký giả, không quyền in nhân sự; browser-local save-as-PDF không là API PDF. |
| Nguồn | ADR-039/041/043–047/083/091/112/127. |

### NV07 — Xem lớp và lịch của mình

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Academics; F11. |
| Đầu vào | Chọn assignment/lớp của mình khi nhiều khóa/lượt, Hiện tại/Lịch sử, ngày cần xem. |
| Đầu ra/phạm vi xem | Thông tin lớp/Teacher/lịch LESSON/FINAL_EXAM đúng scope; lịch completed chỉ xem; buổi NOT_HELD hoặc thuộc lớp hủy giữ lịch sử/liên kết replacement, không hiển thị như hai buổi đã dạy. |
| Quyền/điều kiện | Chỉ assignment của own Registration; không Student khác, không chuyển lớp, dời lịch, xác nhận HELD hoặc xem training grade chưa FINALIZED. |
| Validation | Phân biệt LESSON nghĩa vụ và FINAL_EXAM; replacement chain chỉ một nghĩa vụ, không suy HELD từ giờ/attendance. Lịch mới nhất từ server sau thay đổi có quyền. |
| Ảnh hưởng/giới hạn | Week desktop/agenda375–768, Hôm nay/chọn ngày/detail theo FE12. Không drag-drop/resize/library, không tự đặt lại ca hay tạo buổi bù riêng. |
| Nguồn | ADR-023/026/032/036–037/040/084/087/122. |

### NV08 — Xem bảng điểm đào tạo đã chốt

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Student; Academics; F12. |
| Đầu vào | Assignment/GradeReport của mình, chọn lượt học/lớp đúng ownership. |
| Đầu ra/phạm vi xem | Các component và điểm được công bố, trọng số, tổng đã rounded0–100, nhận xét được công bố, FINALIZED và đạt/không đạt. Không DRAFT/SUBMITTED/RETURNED hoặc lý do review nội bộ như quyền mới. |
| Quyền/điều kiện | Read-only own FINALIZED result; không điểm Student khác, không sửa/thi lại/reset finalized. |
| Validation | Chỉ FINALIZED, Class COMPLETED; tổng≥50 đạt sau half-up2digits. Không thêm chuyên cần, minimum từng component hoặc mức xếp loại. Chưa finalized không là thất bại. |
| Ảnh hưởng/giới hạn | Phân biệt các lượt học, không lấy tổng chưa chốt làm kết quả chính thức. Hoàn tất lượt học cho phép đăng ký lại theo NV02 kể cả không đạt; không copy điểm. |
| Nguồn | ADR-024/028/034/067/083/091/125. |

## 3. Admissions — NV09/NV12; NV10/NV11 đã loại

### NV09 — Tra cứu và kiểm tra hồ sơ tuyển sinh

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Admissions; Admissions; F13/F14. |
| Đầu vào | Tiêu chí hồ sơ/tên/email/SĐT trong phạm vi tra cứu; chọn Registration để xem. Bộ lọc định danh chỉ working memory. |
| Đầu ra/phạm vi xem | Mọi trạng thái gồm CANCELLED/REJECTED: tên/email/SĐT hiện tại, Course, đầu vào/Band, xác nhận/duyệt/status, tiền/status Invoice, lớp đã xếp. Không DOB/địa chỉ, Payment method/reference, Attendance/training grades hoặc toàn danh sách personal profiles. |
| Quyền/điều kiện | Đọc hồ sơ nghiệp vụ, kiểm tra điều kiện; không sửa Student, nhập điểm, ghi danh/đặt ca thay, thanh toán/in/export. Hành động chỉ ở NV12 đúng guard. |
| Validation | State hiện tại từ server; trùng hồ sơ theo predicate unfinished ở NV02, không coi mọi hồ sơ lịch sử là hồ sơ trùng. ID/list/detail cùng scope, ngoài scope không tiết lộ. |
| Ảnh hưởng/giới hạn | Hiển thị thiếu điều kiện/đang chờ rõ; ordinary reads không mutation hoặc audit every page. Không tạo dashboard nhân sự hoặc quyền từ menu. |
| Nguồn | ADR-012/015–019/022/039/083/096/105/107. |

### NV10/NV11 — Đã loại, không triển khai

NV10 hỗ trợ ghi danh trực tiếp và NV11 nhân sự đặt thi thay không có workflow,
menu/API hoặc input/output triển khai. Tư vấn tại quầy dùng Student tự đăng ký
NV01/NV02/NV03; F15 giữ tombstone để truy vết, không phục hồi quyền Admissions.
Nguồn: ADR-012; 39 mã nguồn còn37 retained functions.

### NV12 — Duyệt, từ chối, hủy hồ sơ theo quyền Tuyển sinh

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Admissions; Admissions + Finance/booking effects; F14. |
| Đầu vào | Registration CONFIRMED để duyệt/từ chối; lý do bắt buộc khi từ chối/hủy. Hủy chọn CONFIRMED hoặc ADMITTED có Invoice UNPAID; không nhập amount/timestamp/actor của server. |
| Đầu ra/phạm vi xem | Duyệt→ADMITTED và một Invoice; từ chối→REJECTED/lý do Student đọc được; hủy→CANCELLED cùng Invoice UNPAID/booking tương lai liên quan. Giữ history/đầu vào/Band. |
| Quyền/điều kiện | Duyệt/từ chối chỉ CONFIRMED, Band hợp lệ. Không từ chối ADMITTED, hủy đã PAID hoặc refund; Admissions không Student registration/profiling/score override. |
| Validation | Recheck state/Band/Invoice/Payment/booking ở commit; lý do phần1.2. Chống duyệt lặp tạo Invoice thứ hai, race thanh toán/hủy/booking/start. Không tự chọn giá khác Course lúc issuance. |
| Ảnh hưởng/giới hạn | Preview hủy liệt kê tác động đúng scope; mutation+mandatory audit+dependent updates all-or-none. Booking chưa start kể cả CLOSED bị hủy/giải phóng, đã start giữ kết quả/history và khóa grading cho hồ sơ hủy; is_unfinished cập nhật đúng. Duyệt không là PAID/xếp lớp. |
| Nguồn | ADR-017/022/031/043–047/083/089–093/115; Invoice NV37. |

## 4. AcademicAffairs — NV13–NV20

### NV13 — Quản lý Course, program/Band và cấu hình điểm

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics; F18. |
| Đầu vào | Course: manual code/name/base_price, lệnh ACTIVE/INACTIVE. Program: Course/Band, name, số LESSON nguyên dương, component name/weight/kind GENERAL-LESSON-FINAL_EXAM, trạng thái DRAFT/ACTIVE/INACTIVE. Bộ Band/ngưỡng seed theo ADR-028; không max_score/scholarships hoặc approval Director. |
| Đầu ra/phạm vi xem | Danh mục/quan hệ Course-Band-program, component/trọng số/số buổi/status và chỉ báo freeze. Giá mới cho Invoice phát hành sau, không thay snapshot cũ. |
| Quyền/điều kiện | Course sửa tên/giá, deactivate/reactivate cùng record; code bất biến. Program DRAFT cấu hình trước first Class; muốn sửa ACTIVE chưa dùng phải về DRAFT. INACTIVE chặn tạo/mở lớp mới, không tự dừng lớp đang dạy; restore không bỏ freeze. |
| Validation | Part1.2; unique Course code, Course/Band program, component name trong program; weight tổng100, số LESSON>0 và đúng một FINAL_EXAM component khi cấu hình hợp lệ. Khóa Course/Band/components/kind/weights/count khi bất kỳ Class dùng kể cả CANCELLED; Band set khóa từ first COMPLETED entry result. |
| Ảnh hưởng/giới hạn | First-use/config-edit phải nhất quán, không version/snapshot/new duplicate program để né freeze. Không global unique Course/program names hoặc xóa history. Unused draft chỉ bỏ khi không phụ thuộc và quyền hiện có cho phép, không blanket delete mới. Audit configuration theo NV39. |
| Nguồn | ADR-009/028/032/043–044/078–079/085/087/090–091/115. |

### NV14 — Tạo, mở/đóng, hủy, điều chỉnh ca đầu vào

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Admissions; F16/F17. |
| Đầu vào | Tạo: ngày/giờ start-end, room code, capacity1–20, Teacher phụ trách hợp lệ. Lệnh mở/đóng/hủy hoặc pre-start sửa lịch; đổi Teacher/hủy có lý do bắt buộc. |
| Đầu ra/phạm vi xem | EntryTestSession mới CLOSED, lịch/trạng thái hiệu lực, số booking/chỗ; lịch sử khi hủy/đổi grader. Không giả session full thành state khác. |
| Quyền/điều kiện | Mở/mở lại trước start, Teacher ACTIVE, resource hợp lệ, active bookings≤capacity. Đóng OPEN trước start giữ booking. Date/time/room edit chỉ trước start và chưa từng booking; sau đó muốn đổi phải hủy hợp lệ/tạo mới. Hủy chỉ trước start; đổi grader chỉ trước start và trước mọi score/ABSENT. |
| Validation | Cùng ngày VN/phút/start<end, room chuẩn; không overlap room/Teacher với cả ca và lớp. Grader replacement check Teacher role/ACTIVE/toàn assignments liên quan. Source started effective CLOSED, không reopen dù stored OPEN. |
| Ảnh hưởng/giới hạn | Hủy ca giữ record/history, hủy booking hoạt động liên quan/cho Student tự chọn lại cùng lượt; không auto rebooking/attempt3. Grader change giữ booking, old/new/actor/time/reason; không grade override. Đóng không hủy booking/cấm grading khi đủ guard. Pure read không tự ghi CLOSED; không cần cron mới. |
| Nguồn | ADR-021/027/030/058/086/088/090/093/115. |

### NV15 — Theo dõi kết quả thi/Band để quản lý

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Admissions; F16/F17 và phần chi tiết đã được phép. |
| Đầu vào | Ca/hồ sơ liên quan ca thi, xếp hoặc quản lý lớp; xem progress chấm/kết quả/Band. |
| Đầu ra/phạm vi xem | Tên/email/SĐT hiện tại, Course, kết quả đầu vào/Band và dữ liệu phục vụ nhiệm vụ; Invoice chỉ status. Không tiền/Payment/DOB/địa chỉ hoặc Teacher DRAFT. |
| Quyền/điều kiện | Chỉ xem theo scope nghiệp vụ, không nhập/sửa/vắng thay Teacher hoặc privileged score correction. Không export/profile edit từ chức năng theo dõi. |
| Validation | Dữ liệu đầu vào0–100/max2attempts/history riêng; progress chưa chấm không biến thành0/ABSENT. Quyền đọc không thay thời gian/ownership người chấm. |
| Ảnh hưởng/giới hạn | Xem biến động từ grading/Band atomic nhưng không tự tạo mutation. Classroom/financial sections chỉ dữ liệu được ADR-083 cho phép. |
| Nguồn | ADR-027/029/030/083/086/090/096. |

### NV16 — Tạo, quản lý và chuyển trạng thái Class

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics; F19/F20. |
| Đầu vào | Tạo Class: manual class_code, TrainingProgram, một Teacher hợp lệ, max_size1–15. Lệnh mở/start/complete/hủy; hủy bắt buộc lý do. Capacity edit trong cửa sổ, không nhập enrollment count/HELD tự đoán. |
| Đầu ra/phạm vi xem | Class PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED, size/program/Teacher và lý do chưa đủ điều kiện. Lịch/trạng thái cần xác nhận liên kết NV19; không Registration COMPLETED. |
| Quyền/điều kiện | program_id/code bất biến. Capacity chỉ trước ONGOING và không dưới placed size. Mở PLANNED→OPEN khi program ACTIVE/có Teacher/ít nhất một valid future LESSON. Start OPEN→ONGOING khi actual start, không trước first LESSON, có ≥1 placed Student và đủ expected LESSON schedule. |
| Validation | Complete chỉ từ ONGOING khi mọi required LESSON/FINAL_EXAM được xác nhận HELD và LESSON đủ Attendance; replacement nghĩa vụ tính một, không yêu cầu grades đã gửi/chốt. Hủy chỉ PLANNED/OPEN trống/chưa dạy, không có assignment hoặc đang ONGOING/COMPLETED. Recheck concurrent placement/data/occurrence. |
| Ảnh hưởng/giới hạn | Status changes explicit, không tự theo clock. Hủy giữ lịch/history nhưng giải phóng resource/lịch upcoming, không tác động paid registrations đang chờ lớp. Complete khóa Attendance; kết thúc Registration chỉ khi GradeReport Student FINALIZED nữa. Không reopen, transfer/refund hoặc threshold minimum size>1. |
| Nguồn | ADR-023/025–026/032–033/036–037/040/058/085/087/090/093/115. |

### NV17 — Phân công/đổi Teacher của Class

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics; F20. |
| Đầu vào | Class và Teacher hợp lệ; không tự chỉ định Teacher đang bị chặn như thao tác khôi phục Account. |
| Đầu ra/phạm vi xem | Người phụ trách hiện tại và lịch/class tương ứng. Không tạo workflow lịch sử đổi Teacher riêng. |
| Quyền/điều kiện | Một Teacher/Class. Đổi chỉ trước ONGOING/actual teaching và chưa Attendance/Score; ngoài cửa sổ phải chờ Admin mở/khôi phục người phụ trách hợp lệ, không ngoại lệ giữa khóa. |
| Validation | Teacher role/status hợp lệ, kiểm toàn bộ Class schedules với assignments ca/lớp của Teacher mới, room/student invariant không được phá; recheck window/data ở commit. |
| Ảnh hưởng/giới hạn | Không auto reassign khi Account LOCKED/DISABLED hoặc auto hủy Class/session/Registration; mất assignment thì Teacher cũ mất task access. Việc đổi không sửa Grade/Attendance/history nghiệp vụ hoặc grant rights staff profile edit. |
| Nguồn | ADR-023/049–050/083/089/093/096. |

### NV18 — Xếp Student vào Class

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics + Admissions/Finance conditions; F22. |
| Đầu vào | Registration đủ điều kiện, Class đích; không Student tùy ý không có hồ sơ hoặc thao tác remove/reassign. |
| Đầu ra/phạm vi xem | StudentClassAssignment, Class size, own Student thấy lớp; Registration vẫn ADMITTED, “Đã xếp lớp” là chỉ báo. |
| Quyền/điều kiện | Registration ADMITTED, Invoice PAID, đúng Course/Band, Class OPEN/còn chỗ. Không nhận muộn PLANNED/ONGOING/ended; không chuyển/gỡ lớp sau xếp kể cả chưa dạy. |
| Validation | Một assignment/Registration, max_size1–15, role/resource/Class/program hợp lệ; Student không trùng ca/lịch lớp có hiệu lực. Recheck capacity/Class transition/Payment/Band và chống concurrent assignment. |
| Ảnh hưởng/giới hạn | Không có Class phù hợp thì chờ, không tự chuyển Band/ca/lớp hoặc mở thêm lớp. Không đổi Registration state, không thêm Invoice/Payment; mandatory audit cùng mutation. |
| Nguồn | ADR-023/040/083/088/090/093/115. |

### NV19 — Tạo/dời/thay lịch và xác nhận buổi thực tế

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics; F21. |
| Đầu vào | Class; LESSON weekly date range/weekdays/times/room hoặc buổi riêng; FINAL_EXAM riêng; pre-start move; non-held replacement reason + valid future date/room; lệnh xác nhận HELD sau end. |
| Đầu ra/phạm vi xem | Preview ngày cụ thể/count/conflicts đúng scope, Schedule hiện hành/history/replacement cùng obligation; occurrence PLANNED/HELD/NOT_HELD và actor/time. Một FINAL_EXAM/Class sau last current LESSON. |
| Quyền/điều kiện | Pre-start date/time/room edit chỉ chưa Attendance/related Score và Class chưa COMPLETED/CANCELLED. Khi đã đến start mà không tổ chức: reason/history + linked replacement, no old dependent data, Class chưa ended. HELD từng buổi chỉ AcademicAffairs sau scheduled end và Class ONGOING; không undo. |
| Validation | Count LESSON obligations≤program expected, đủ trước start teaching, FINAL_EXAM không thay LESSON. Same-day/minute/[start,end), room/class/Teacher overlaps và Student booked/assigned conflicts; replacement cùng Class/type/current Teacher, future hợp lệ, one current/obligation. |
| Ảnh hưởng/giới hạn | Weekly input→preview→Tạo toàn bộ; sửa input invalidate preview, error không skip ngày/lưu partial. Commit server recheck/409 re-preview, preview không giữ chỗ. Replace old NOT_HELD giữ data/history và cùng nghĩa vụ, không count đôi hoặc fake absence. Không extra lessons/holiday recurrence/drag-drop/auto makeup. |
| Nguồn | ADR-014/025/032/035–037/058/087–088/093/115/122–123; addendum ADR-156 ngày03/10/2026. |

Addendum được owner duyệt03/10/2026 — ADR-156/E04-RF03: root LESSON/
FINAL_EXAM mới, mọi candidate của weekly batch và move destination phải có
startAt>serverNow lúc kiểm tra lưu trong transaction. Tới/qua start sau input
hợp lệ409, malformed date/time400; preview không bảo đảm cutoff và server
kiểm clock lại, không chỉ RV. Một candidate thất bại thì batch không lưu
phần nào/không tự bỏ hoặc dời ngày; không thêm backfill. Giữ old-slot move,
replacement/HELD/obligation/conflict rules. E01-v1 nghiệm thu30/09 vẫn được
giữ; đây là bổ sung đã duyệt sau đó, không toàn bộ E04/D7 acceptance.

### NV20 — Trả lại hoặc chốt bảng điểm cả Class

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | AcademicAffairs; Academics; F23. |
| Đầu vào | Class COMPLETED và toàn bộ GradeReports SUBMITTED; lệnh return với mandatory reason hoặc finalize. Không list Student riêng để chọn ai được chốt. |
| Đầu ra/phạm vi xem | Cả cohort Class RETURNED, giữ Score/comment và lý do; hoặc toàn bộ FINALIZED/published cùng lúc, final_score/actor/time/history. Read SUBMITTED/RETURNED/FINALIZED, không Teacher DRAFT values. |
| Quyền/điều kiện | Chỉ review đúng toàn lớp/COMPLETED/SUBMITTED; không Teacher score edits hoặc FINALIZED reopen/override. Director/Admin không approval tier thứ hai. |
| Validation | Mọi report/state/required component cùng cohort hợp lệ; stale preview/score review race phải chặn, không tự overwrite. Điểm tổng0–100/decimal/half-up và ≥50 passing. |
| Ảnh hưởng/giới hạn | Một confirmation Class code/count/current state/effects; mandatory return reason. Server all-or-none transitions + mandatory audit/group operation_id; finalize đồng thời publish, cập nhật unfinished predicate khi Class COMPLETED + Student report FINALIZED. Failure không partially publish hoặc âm thầm finalize thiếu data. |
| Nguồn | ADR-024/028/034/067/083/089–094/110/115/125. |

## 5. Teacher — NV21–NV26

### NV21 — Xem ca đầu vào được phân công

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher; Admissions; F25. |
| Đầu vào | Tài khoản Teacher đã xác thực, chọn ca/booking thuộc grader assignment hiện tại. |
| Đầu ra/phạm vi xem | Ngày/giờ/phòng/status ca, danh sách thi với Student internal ID/tên/email/SĐT hiện tại/Course/đầu vào-Band phục vụ nhiệm vụ, trạng thái cần chấm/vắng/correction. Không DOB/address/finance hoặc toàn Student directory. |
| Quyền/điều kiện | Chỉ ca grader_id là Teacher hiện tại, không xem ca người khác nhờ ID/detail link. Account bị chặn không task access, không auto reassignment. |
| Validation | Compute effective closure/start/end, Registration cancelled chưa chấm là lịch sử không actionable; không biến missing score thành absent. List/detail/current assignment scope giống nhau. |
| Ảnh hưởng/giới hạn | Read-only list; viết chỉ qua NV22 với guard riêng, không thêm Teacher session management hoặc quyền sửa hồ sơ. |
| Nguồn | ADR-027/029–031/083/086/089/096. |

### NV22 — Nhập/sửa điểm đầu vào và ghi vắng

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher hiện được phân công; Admissions; F25. |
| Đầu vào | Lượt/booking đang thuộc ca được giao; initial score0–100 hoặc xác nhận ABSENT; correction chọn COMPLETED score + mandatory reason. |
| Đầu ra/phạm vi xem | COMPLETED result/booking và Band/best attempt sau grading/correction; hoặc booking ABSENT không score, giữ lượt để đặt lại. Actor/time và old-new khi correction; không grade đào tạo. |
| Quyền/điều kiện | Initial score từ session end, còn hồ sơ hợp lệ trước admission confirmation, session không CANCELLED. ABSENT chỉ sau end, Teacher xác nhận thực sự không thi. Correction chỉ original grader vẫn assigned, Registration TESTING/BAND_ASSIGNED, result COMPLETED. Không override Admin/AcademicAffairs. |
| Validation | 0–100/max2decimal, cùng Registration/attempt/booking/ca; không nhập cho CANCELLED/REJECTED/CONFIRMED hoặc record hủy. CLOSED cho grading khi guard đạt. Correct race confirm/cancel/start/config freeze; không đổi ABSENT thành completed hoặc xóa score qua correction. |
| Ảnh hưởng/giới hạn | Score/best-attempt/Band/freeze/audit nhất quán trong một thao tác; tied-best ưu tiên attempt1, giữ cả kết quả. Lượt vắng không score0 hoặc third attempt. Không deadline24/48h và không tự tính absence từ empty score. |
| Nguồn | ADR-008/021/027–031/083/086/090–093/115; NV36. |

### NV23 — Xem Class, lịch dạy và Student được giao

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher; Academics; F24. |
| Đầu vào | Account hiện tại, Class được phân công, ngày cần xem. Không chọn Teacher khác để mở scope. |
| Đầu ra/phạm vi xem | Class/program/size/lịch LESSON-FINAL_EXAM, Student internal ID/tên/email/SĐT và Attendance/Grade phục vụ nhiệm vụ. Không DOB/address/Invoice/Payment hoặc tất cả Student toàn trung tâm. |
| Quyền/điều kiện | Chỉ current teacher_id assignment, mất assignment mất quyền. Xem không là sửa Class/schedule/placement/HELD. Data grade/attendance read-only theo state/window của NV24–26. |
| Validation | History NOT_HELD/replacement/ended đúng nghĩa vụ, không elapsed→HELD. Scope calendar/list/detail giống nhau, no leaking resource/person details qua lỗi. |
| Ảnh hưởng/giới hạn | Local week/agenda/date/detail theo FE12; không drag-drop/resize/import/export hoặc dashboard role mới. |
| Nguồn | ADR-023/026/032–036/083/087/089/096/122. |

### NV24 — Lưu/sửa Attendance của một LESSON

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher của Class hiện tại; Academics; F26. |
| Đầu vào | LESSON hiện hành của Class được giao, danh sách Student assignments đã đánh dấu/thay đổi với Có mặt/Vắng. Ban đầu Chưa ghi; không gửi giả mọi Student present. |
| Đầu ra/phạm vi xem | Attendance một record/Student/LESSON với present1/0 đã lưu, báo Chưa lưu/Đã lưu và Student chưa ghi. Chưa ghi là missing record, không enum hoặc absent. |
| Quyền/điều kiện | Từ đúng LESSON start đến trước Class COMPLETED; không cần chờ end, không hạn24/48h. LESSON không NOT_HELD/Class không CANCELLED; assignment/schedule cùng Class. Sau COMPLETED chỉ xem dù grades chưa finalized. |
| Validation | Chọn giá trị present1/0 thực, no default-all; unique Student/LESSON, own assigned Class, clock/schedule hiện hành và concurrent completion/move. Có thể lưu partial danh sách đã ghi, nhưng completeness chặn Class completion cho tới đủ required Attendance. |
| Ảnh hưởng/giới hạn | Lưu điểm danh buổi là một batch all-or-none cho các giá trị đã nhập/thay đổi. Một giá trị lỗi thì không lưu phần nào; missing Student không bị tự viết absent. Không autosave/durable draft/Excel import/overwrite form cũ; Attendance không tự HELD. |
| Nguồn | ADR-025/032–033/036/083/087/093/106/108/124. |

### NV25 — Lưu điểm nháp/nhận xét và gửi cả Class

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher của Class hiện tại; Academics; F27. |
| Đầu vào | Một Student assignment, các component scores đã nhập/thay đổi, comment tùy chọn; kind LESSON/FINAL_EXAM chọn current schedule cùng Class/type, GENERAL không schedule. Lệnh gửi chọn toàn Class, không selected Student. |
| Đầu ra/phạm vi xem | Lưu một Student cùng scores/comment trong DRAFT (hoặc phần editable RETURNED), server calculated total hoặc Chưa đủ điểm. Gửi hợp lệ chuyển toàn cohort SUBMITTED/history, chưa Student publication. |
| Quyền/điều kiện | Nhập nháp khi Class ONGOING/COMPLETED và report DRAFT/RETURNED; component schedule-linked từ start và không NOT_HELD. SUBMITTED/FINALIZED không sửa. Gửi chỉ Class COMPLETED, mọi Student đủ component và DRAFT/RETURNED. |
| Validation | Score0–100/max2digits, component thuộc frozen program của Class, schedule correct/current/type, one score/Student/component. Cho missing components lúc draft, không blank→0/xóa stored score; comments≤1000. Total do server NV38, không gửi total client làm kết quả. |
| Ảnh hưởng/giới hạn | Lưu học viên: changed scores + comment all-or-none, desktop per-row/mobile per-Student cùng đơn vị; no autosave/import/durable cache. Gửi chặn còn local unsaved edits; confirmation Class/count/state/effects, server all-or-none/race/replay/operation audit. Không auto gửi khi đủ điểm hoặc individual submit. |
| Nguồn | ADR-024/028/034/083/087/090–093/106/108/110/115/124–125; NV38. |

### NV26 — Sửa và gửi lại Class RETURNED

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Teacher current assignment; Academics; F27. |
| Đầu vào | Class với GradeReports RETURNED, các changed component values/comment theo Student; đọc returned reason, lệnh resubmit toàn Class. |
| Đầu ra/phạm vi xem | Lưu sửa vẫn RETURNED cho tới gửi lại; giữ scores/comments/history cũ và calculation mới đúng scope; resubmit toàn Class→SUBMITTED, không FINALIZED. |
| Quyền/điều kiện | Chỉ RETURNED/DRAFT editable theo NV25 và Class ONGOING/COMPLETED/window linked scores; không sửa SUBMITTED/FINALIZED hoặc trả/chốt thay AcademicAffairs. |
| Validation | Part1.2/NV25; complete all Students/components/COMPLETED trước resubmit, local unsaved edits chặn. Chưa nhập không0, blank không xóa; recheck Teacher/current states/data race. |
| Ảnh hưởng/giới hạn | Partial draft per-Student allowed, report workflow whole-Class all-or-none. Return reason bắt buộc ở NV20, không bắt Teacher tạo reason mới khi resubmit nếu chưa có policy. Không giới hạn một lần submit trong lifetime; mỗi return/resubmit có operation history. |
| Nguồn | ADR-024/034/081/083/087/093/108/115/124–125. |

## 6. Accountant/Director — NV27–NV32

### NV27 — Tra cứu Invoice nghiệp vụ

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Accountant; Finance; F28. |
| Đầu vào | Invoice number/Course/name-email-phone Student/current Invoice state/optional issue date range/overdue-only; chọn Invoice. Personal searches volatile, không URL/storage. |
| Đầu ra/phạm vi xem | Mọi state gồm CANCELLED, amount/snapshot/issue/due/overdue; current contact cần thiết; Payment method/reference/amount, confirmer/time được phép. Không Attendance/training grades/EntryTest hoặc personal profiles dump. |
| Quyền/điều kiện | Read Invoice/Payment nghiệp vụ Accountant; writing chỉ NV28. Không Student profile edit, Invoice print/export riêng hoặc report rights cho role khác. |
| Validation | Part1.2/filter dates; overdue UNPAID due<today VN, không status mới. Lookup issue-date filters độc lập revenue period của NV29; chặn ngoài role, paginated/stable. |
| Ảnh hưởng/giới hạn | Detail hành động đúng điều kiện, không auto PAID từ overdue hoặc xem trang, no debt penalties. CSV chỉ reportNV29 chứ không toàn Invoice directory này. |
| Nguồn | ADR-010/018/042–047/083/096/105/107. |

### NV28 — Xác nhận tiền và sửa metadata Payment

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Accountant; Finance; F29. |
| Đầu vào | Invoice UNPAID, amount thực đối chiếu bằng Invoice, CASH/TRANSFER, reference bắt buộc khi TRANSFER. Metadata correction chọn Payment đã xác nhận + method/reference mới + mandatory reason; không paid_at/amount thay thế. |
| Đầu ra/phạm vi xem | Một Payment với confirmer/time server và Invoice PAID; correction giữ same Payment, cập nhật method/reference, history old-new/reason/actor/time. Không tạo receipt/refund hoặc paid date backdating. |
| Quyền/điều kiện | Chỉ xác nhận đủ một lần ngoài ứng dụng; không partial payments/free path/cancel paid Invoice/delete Payment. Correction chỉ method/reference, không amount/link/paid_at; no override Admissions/Admin. |
| Validation | Money nguyên đồng/dương/bằng Invoice snapshot, reference≤100/cash nullable/transfer nonempty; không global reference unique. Recheck Invoice/Registration/state và concurrent cancel/payment/edit; one Payment/Invoice/request replay-safe. |
| Ảnh hưởng/giới hạn | Một confirmation có Invoice/amount/state/effects; Payment creation+PAID+mandatory audit all-or-none, lặp không thu lần2. Correction+audit atomic, revenue date/amount không đổi. Không bank gateway/proof verification tự động. |
| Nguồn | ADR-017/022/031/042–047/081–082/083/089–093/110/115. |

### NV29 — Xem/xuất báo cáo tài chính

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Accountant; Reports đọc Finance; F30. |
| Đầu vào | Kỳ ngày VN cho revenue/billed (mặc định tháng hiện tại), Course; overdue-only cho current debt, không date-period debt. Áp dụng bộ lọc rồi lệnh CSV theo applied filters. |
| Đầu ra/phạm vi xem | Revenue=Payment xác nhận trong kỳ; billed=non-CANCELLED Invoice phát hành trong kỳ; current debt=Invoice UNPAID hiện tại; overdue subset. Thẻ/bảng nhóm phù hợp, tổng toàn filtered set và queried_at; CSV cùng report columns/toàn results, không profile/Invoice dump. |
| Quyền/điều kiện | Chỉ financial report, không Admissions/training/expense-profit/as-of. CSV chỉ Accountant, không Director/Admin/AcademicAffairs/Admissions. Không print staff/PDF/XLSX hoặc extra hidden columns. |
| Validation | Valid filters/Asia-Ho_Chi_Minh boundaries, current state đúng cohort; không billed−period revenue để suy debt. Exact money sums, fresh consistent query; totals không chỉ page. Export max10000data rows hoặc5MB, vượt reject toàn file. |
| Ảnh hưởng/giới hạn | Draft filter không đổi nhãn applied result; Áp dụng chạy query mới, mỗi result/CSV snapshot riêng, queried_at không as-of. CSV UTF8BOM/comma/CRLF/quoted escaped/formula safeguard inclfull-width, original DB không đổi; warning giới hạn spreadsheet/reopen, không đảm bảo mọi phần mềm. |
| Nguồn | ADR-013/018/043–047/063–064/070/072/091/094/101/126. |

### NV30 — Báo cáo tuyển sinh tổng hợp

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Director; Reports; phần Tuyển sinh F31. |
| Đầu vào | Course/registration dates/current Registration states; mặc định tháng VN hiện tại và all states, gồm CANCELLED/REJECTED. Selector giữ Course INACTIVE cho history. |
| Đầu ra/phạm vi xem | Registration counts, distinct Student counts mỗi Course và toàn cohort, breakdown nhóm không định danh theo quyền. Query time/applied filters/totals toàn filtered set; không Student names/email/phone/individual records. |
| Quyền/điều kiện | Read-only applicable group drill-down, không approve/individual search/profile/Invoice details/export. Không thêm report screen cho Admissions vì tên miền tuyển sinh. |
| Validation | Count distinct student_id cùng cohort; Student hai Course mỗi nhóm1/tổng1, repeat enrollments có thể tăng records nhưng không people. States current lúc query, date filter không tái dựng state past. |
| Ảnh hưởng/giới hạn | Cards/group tables/no chart dependency; Áp dụng lọc và giữ queried_at/result context, không sum group distinct thành overall. Empty data rõ, no staff dashboard mới. |
| Nguồn | ADR-018/047/063/065/068/071/094/104/126. |

### NV31 — Báo cáo đào tạo và kết quả chốt

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Director; Reports; phần Đào tạo F31. |
| Đầu vào | Current inventory: Course/Band/Class/current Teacher/Class state (default all), không inventory date period. Finalized results thêm finalization-date period mặc định tháng VN hiện tại; Course INACTIVE referenced vẫn chọn được. |
| Đầu ra/phạm vi xem | Class counts theo state/size, LESSON progress=confirmed-held obligations/expected count, FINAL_EXAM riêng; FINALIZED results/pass rate trong finalization period. Nhóm Course/Band/Class khi phù hợp, không Student-identifying detail/unpublished grades. |
| Quyền/điều kiện | Chỉ read aggregates/non-identifying groups, không grades của Student cá nhân, edit/approval/export. Không historical Teacher reassignment view hoặc Class creation-date metric mới. |
| Validation | NOT_HELD historical/replacement chỉ một obligation, Attendance/elapsed time không HELD; CANCELLED giữ Class-state counts nhưng không delivered progress. Passing FINALIZED rounded score≥50, enrollment-result grain, unfinalized không fail; empty denominator Chưa có dữ liệu, nonempty zero pass mới0%. |
| Ảnh hưởng/giới hạn | Rate/progress display2dec half-up, không round counts; current inventory tách finalized cohort. Query/apply context toàn set, không page/group totals sai; no as-of/chart library. |
| Nguồn | ADR-024–025/028/036–037/047/063/066–067/069/071/087/094/126. |

### NV32 — Báo cáo tài chính quản trị

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Director; Reports; phần Tài chính F31. |
| Đầu vào | Kỳ revenue/billed tháng VN hiện tại mặc định, Course, current-debt overdue-only; no debt date period. |
| Đầu ra/phạm vi xem | Cùng định nghĩa revenue/billed/current debt/overdue NV29, exact totals/queried_at và applicable group aggregates; không Invoice/Payment/Student identifiable records. |
| Quyền/điều kiện | Read-only group insight, không Accountant operational lookup/correction/export/Invoice print hoặc approve. Dù dùng cùng metric cũng không kế thừa Accountant rights. |
| Validation | Invoice current CANCELLED loại billed, Payment confirmation dates revenue, issue dates billed, UNPAID debt hiện tại. Không wrong-cohort subtraction/as-of hoặc fractional/lossy amounts. |
| Ảnh hưởng/giới hạn | Áp dụng filter/card-group tables, một query snapshot nhất quán, không hidden individual payload hoặc CSV/PDF/XLSX. |
| Nguồn | ADR-018/043–047/063–064/070–072/091/094/126. |

## 7. Admin và xử lý hệ thống — NV33–NV39

### NV33 — Cấp nhân sự và reset mật khẩu

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Admin; Administration; F32. |
| Đầu vào | Cấp: full_name/email, một role nhân sự hiện có, SĐT nếu nhập. Reset: Account đích (nhân sự hoặc Student) + lệnh reset xác nhận. Không Admin nhập permanent password, đổi role/email hoặc tạo Student thay tự đăng ký. |
| Đầu ra/phạm vi xem | Account đúng role hoặc credential reset, random temporary password32chars/24h chỉ Admin xem một lần; khối F32 có email/role/deadline server. Không endpoint/view dùng lại plaintext hoặc trả password_hash. |
| Quyền/điều kiện | Chỉ Admin, không đa-role/RBAC động hoặc edit staff personal information từ chức năng cấp. Reset không unlock LOCKED/reactivate DISABLED, không self-service forgotten password/email resend. |
| Validation | Account field rules/unique email/phone toàn hệ thống khi có; temporary generator mật mã/hash only, no plaintext logs/cache/replay. Recheck target/current rights/conflict và mandatory account audit + related revoke atomic. |
| Ảnh hưởng/giới hạn | Reset revoke mọi old sessions/version, phải fresh login và forced change trước business. FE18 explicit optional copy/manual fallback + ack hiểu đóng không xem lại; ack không proof delivery. Close/exit/logout/expiry clear secret state, refresh/tab close có thể mất, clipboard có thể còn. Unknown result inspect operation/Account status; lost secret requires Admin intentional confirmed new reset, không auto retry/reset. |
| Nguồn | ADR-020/048–049/051/053–054/073/075–076/091–093/098–099/110/114–115/128. |

### NV34 — Khóa/mở/ngừng dùng/khôi phục Account

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Admin; Administration; F32. |
| Đầu vào | Account đích + lệnh khóa/mở khóa/ngừng dùng/khôi phục theo trạng thái hiện tại; không client khai actor/security_version hoặc xóa Account. |
| Đầu ra/phạm vi xem | Cùng Account/role/business history với state mới; session invalidation khi lock/disable, explicit messages. Không đổi Registration/Class/test schedule/grade hoặc xóa dữ liệu. |
| Quyền/điều kiện | Không lock/disable chính mình hoặc last ACTIVE Admin; chỉ unlock manual lock, restore same disabled record. Timed failed-login block là timer khác, không unlock Account tự động. No role/email edit hoặc privileged business override. |
| Validation | Recheck current state/role/last Admin invariant ngay lúc commit và concurrent Admin mutations. Action preview target/effects; không thêm mandatory reason ngoài policy đã chốt. |
| Ảnh hưởng/giới hạn | Lock/disable revoke mọi sessions, unlock/restore không revive old tokens; reset/password change cũng revoke ở chức năng tương ứng. Mandatory Account audit failure rollback cả state/revocation liên quan. Responsible Teacher blocked không auto reassignment/cancellation; role workflows chỉ allowed windows. |
| Nguồn | ADR-020/049–052/083/089/092–093/110/115. |

### NV35 — Tra cứu nhật ký và chi tiết theo operation

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Admin; Administration; F33. |
| Đầu vào | Business date range, actor, action, entity type/ID, result, page/sort; detail event và operation_id liên quan. Không full-payload/free-text secret search. |
| Đầu ra/phạm vi xem | Newest-first paginated event rows; time/actor/action/target/result/reason, old→new allowlisted fields, related operation events có phân trang. Unauthenticated ghi Chưa xác thực, không input email thành actor; grouped rate-limit có count/window đúng. |
| Quyền/điều kiện | Admin-only read/search, no export CSV/PDF, edit/delete/undo hoặc phục hồi business từ log. Related operation links không cấp quyền đọc profile/financial record ngoài Admin scope. |
| Validation | Filters/date hợp lệ/phân trang ổn định; data đã loại passwords/hashes/JWT/session secrets/raw requests-responses. IP chỉ khi legitimate stored và chưa retention expiry; removed IP ghi Không còn lưu, không reconstruct. |
| Ảnh hưởng/giới hạn | Xem audit không normal mutation, không thêm app purge hoặc security-alert subsystem. Whole-Class/dependent changes liên kết operation_id, không thay đơn vị page thành arbitrary grouped operation. |
| Nguồn | ADR-058–062/081/093/096/105/115–116/129. |

### NV36 — Tính/cập nhật Band từ kết quả đầu vào

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Hệ thống, không role/menu; Admissions; triggered NV22, hiển thị F08/F14/F16–17/F25 trong scope. |
| Đầu vào | Các COMPLETED scores0–100 của một Registration và Band thresholds hợp lệ; first grade hoặc authorized pre-confirmation correction. |
| Đầu ra/phạm vi xem | Best completed attempt/score/BandAssignment với ngưỡng min lớn nhất≤score; đồng điểm chọn attempt_number nhỏ hơn, không copy result sang Registration khác. |
| Quyền/điều kiện | Chỉ chạy với score mutation được Teacher có quyền chấp nhận; không Admin/AcademicAffairs chỉnh Band trực tiếp hoặc recompute history sau CONFIRMED/CANCELLED/REJECTED. |
| Validation | Max2logical attempts, no ABSENT/missing-as-zero, min thresholds seed0/25/45/65/85 và first-result freeze. Điểm nhận2decimal không integer round; result/best/Band cùng Registration. |
| Ảnh hưởng/giới hạn | Grade/correction+Band+first-use freeze+mandatory audit nhất quán, không publish partially; subsequent eligible retake dùng same frozen set. Giữ cả hai attempts, không auto admission confirmation. |
| Nguồn | ADR-008–009/021/028–029/031/086/090–093/115. |

### NV37 — Cấp Invoice khi admission approval

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Hệ thống; Finance, triggered Admissions NV12; F10/F14/F28–29 theo quyền. |
| Đầu vào | Successful eligible approval của Registration CONFIRMED + current Course tuition >0, issue time server; không client cấp invoice_number/amount tùy ý. |
| Đầu ra/phạm vi xem | Một Invoice UNPAID/Registration với snapshot amount, server number HD-YYYY-NNNNNNNN, issue time/due date; chưa Payment/revenue. |
| Quyền/điều kiện | Chỉ trong approval transaction có actor Admissions đúng quyền. Không tạo lại Invoice khi thanh toán, không free course hoặc manual duplicate issue/module receipt. |
| Validation | Money part1.2; Vietnam issue year, safe unique nonreused number (gaps allowed), due=issue local date+7calendar days inclholidays. Exactly one Invoice per Registration, chống approve replay/config price race. |
| Ảnh hưởng/giới hạn | ADMITTED + Invoice + audit all-or-none; future tuition edit không rewrite amount. Nếu required issuance/audit fail thì approval không thành công. No due-date extension/edit/automated cancellation or debt penalty. |
| Nguồn | ADR-017/031/043–047/058/085/090–093/115. |

### NV38 — Tính tổng điểm từ component

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Hệ thống; Academics, triggered NV25–26/NV20; F27/F23/F12 và aggregate F31 đúng scope. |
| Đầu vào | Component scores cùng Student assignment, frozen program weights/kinds/schedule links, cần đủ components cho official total/workflow. |
| Đầu ra/phạm vi xem | Tổng=sum(score_value×weight/100), decimal intermediate, half-up2decimal một lần. Thiếu component→Chưa đủ điểm, không final score0 giả; official finalized total/passing khi đủ guard. |
| Quyền/điều kiện | Không nhập/override total riêng, không thêm grades0 cho absent hoặc extra score items. Teacher xem draft total của assignment, AcademicAffairs không DRAFT values, Student chỉ FINALIZED; report Director chỉ aggregates. |
| Validation | Weight positive/sum100, score0–100≤2digits, correct frozen config/Class/component/schedule. Không round each contribution;49.995→50.00 pass,49.994→49.99 fail khi FINALIZED. |
| Ảnh hưởng/giới hạn | Calculation không tự submit/finalize/Class complete; lưu nháp và chuyển whole-Class workflow giữ guards/all-or-none. No attendance passing gate/component minimum mới hoặc thang10. |
| Nguồn | ADR-009/024/028/034/067/083/087/090–091/094/124–125. |

### NV39 — Ghi audit nghiệp vụ/tài khoản và sự kiện bảo mật

| Thành phần | Đặc tả chức năng |
| --- | --- |
| Actor/module/màn hình | Hệ thống, Administration nhận events từ module; không menu riêng, Admin đọc F33. |
| Đầu vào | Accepted event/actor đã xác thực hoặc NULL, target/time/result/reason và relevant old-new allowlisted data; shared operation_id cho parent/dependent/whole-Class. Không raw credential/hash/JWT/payload. |
| Đầu ra/phạm vi xem | Mandatory history: approval/cancellation/correction/Payment/placement/grade/account và phạm vi đã chốt; thêm config/Class lifecycle/schedule changes. Security events login success-failure/throttle/logout/password/account/denied access; rate-limit aggregates nếu có giữ count/IP/window/counters. |
| Quyền/điều kiện | App layer ghi, không SQL trigger hoặc người dùng edit/delete/export. Không audit mọi ordinary page reads; guessed login email không verified actor. No alerts/notifications module mới. |
| Validation | Old/new/reason bắt buộc nơi policy yêu cầu, minimize PII, never secret/full request-response. Pure security/IP retention90days ops cleanup, mandatory history giữ; expired IP remove không xóa mandatory fields. |
| Ảnh hưởng/giới hạn | Mandatory mutation audit cùng transaction, failure rollback gồm related revocation. Pure auth/logout/denial event log best-effort bounded secret-free fallback, không bypass auth/restore revoked logout vì log lỗi. Whole-Class mỗi lần submit/return/finalize có history/operation links, không chỉ last timestamp. |
| Nguồn | ADR-029–031/034/042/048–050/058–062/093/100/115–116/129. |

## 8. Thao tác chung và các cách trình bày không có mã NV/F mới

### 8.1. Đăng xuất và hết phiên — account menu, F01 khi cần login lại

Actor: mọi role đang có phiên; Administration. Đầu vào là yêu cầu logout của
phiên hiện tại, không chọn Account khác. Server revoke current session;
các tab cùng profile logout chung, device/profile khác không bị logout-all.
Xóa cookie phía browser không thay server revocation. No refresh/remember-me.

Phiên hết do2h/idle30min/temp restricted deadline/revocation thì business
request bị chặn, UI về login với lý do phù hợp sau xác thực, không phục hồi
nháp/secret bằng storage. Typing/polling không gia hạn idle. Pure logout event
logging best-effort không phục hồi revoked session khi ghi log lỗi.
Nguồn: ADR-049/055–057/092/097–099/109/115.

### 8.2. Tự đổi mật khẩu — account menu, restricted password-change gate

Actor: chính account hiện tại, mọi role; Administration. Đầu vào: current
password và new password hợp lệ phần1.2. Normal session kiểm đúng current
password; restricted session chỉ được đổi credential tạm còn hạn/logout.
Không nhận account_id khác, không đổi email hoặc dùng như forgotten-password.

Đổi thành công lưu hash/must-change state theo policy, hủy mọi existing
sessions/version (kể cả current), về fresh login; không tự nâng restricted
session thành normal. Mandatory Account audit/mutation/revocation all-or-none,
no secret in audit/replay/cache. Password tạm quá24h cần Admin reset mới;
Account LOCKED/DISABLED không tự unlock/reactivate. Không ép đổi định kỳ.
Nguồn: ADR-020/048–049/053–055/091–093/098/114–115.

### 8.3. Sửa hồ sơ cá nhân — menu chỉ Student

Actor: Student sở hữu StudentProfile; Students. Input editable: full_name,
DOB, phone, address tùy chọn. Email hiện tại hiển thị read-only, không role/
state/Account khác hoặc hồ sơ nhân sự. Áp validation/phone uniqueness phần1.2.
Output current profile hợp lệ; lịch sử Registration/Invoice hiển thị tên hiện
tại theo ADR-039, không snapshot PII mới hoặc đổi amount snapshot.

Đã CONFIRMED vẫn được sửa những personal fields này; không sửa Course,
Band, score, Invoice/Payment hoặc staff sửa thay. Save explicit/stale conflict
không overwrite, không autosave/durable draft. Đây không là NV10/NV11 phục hồi.
Nguồn: ADR-016/019/039/073–077/083–084/091/105/108.

### 8.4. Trang chủ Student F03 và danh sách/chi tiết phụ

F03 nhận actor Student và dữ liệu own Registration, booking, Class assignment,
Invoice và FINALIZED grade. Output thẻ từng hồ sơ/khóa, current step,
eligible next action, ca/buổi gần nhất, unpaid/overdue và finalized links.
Nhiều khóa không chung một tiến trình; empty→chọn Course. Wait reasons
chấm/duyệt/thanh toán/xếp rõ, không giả state/progress metric mới hoặc lộ
Grade DRAFT/SUBMITTED/RETURNED. Không inbox/auto notifications/staff dashboard.
Nguồn: ADR-083–084/104/121.

List/detail/complex form có URL/breadcrumb/back; giữ context page/sort và
safe non-identifying filters, personal searches chỉ volatile. Read-only thể
hiện rõ; action ngoài role ẩn, role hợp lệ thiếu guard disabled/lý do đúng scope.
Component/week/agenda/mobile gradebook, confirmations, reports/print/one-time
secret/audit áp FE01–FE19 đã duyệt; không bottom nav/real role switcher,
library mới, drag calendar, auto saves hoặc secondary approver.
Nguồn: ADR-103–113/121–129.

## 9. Truy vết 32 màn hình trong phạm vi

Bảng này đối chiếu từng mã, không tạo screen/role mới. “Chung” là các mục ở
phần8 và NV01, không được tính thành NV40. F03/F04 là phép chiếu dữ liệu,
không tự tạo thêm business mutation.

| Mã | Actor | Dữ liệu/thao tác chính | Đặc tả/nguồn trace |
| --- | --- | --- | --- |
| F01 | Trước login; 7 role | Email/password, normal/restricted login, login errors; login lại sau logout/expiry/change. | NV01, phần8.1–8.2; ADR-019/097–100/104. |
| F02 | Tự đăng ký Student | Name/DOB/email/phone/password/address optional, Account Student/own profile. | NV01; ADR-019/073–077/091/114. |
| F03 | Student | Own registration cards/next action/event/unpaid/finalized links, empty state. | Phần8.4, NV02–NV08; ADR-121. |
| F04 | Student | Course ACTIVE để chọn đăng ký; own context/history giữ thông tin Course đã tham chiếu. | NV02; ADR-085/104. |
| F05 | Student | Tạo/xem Registration riêng, status và hủy trước CONFIRMED theo quyền. | NV02; ADR-015/022/031/038/090. |
| F06 | Student | Đặt/hủy/đổi ca đầu vào/retake cho đúng logical attempt. | NV03; ADR-021/027/030/090. |
| F07 | Student | Own test schedule/current-history/agenda/detail, CLOSED effective guard. | NV03, phần8.4; ADR-086/122. |
| F08 | Student | Scores đầu vào/từng lượt/best/Band/absent history. | NV04/NV36; ADR-028–029. |
| F09 | Student | Confirm BAND_ASSIGNED khi không pending retake; freeze đầu vào. | NV05; ADR-021/029–030. |
| F10 | Student | Own Invoice/external payment guidance, accurate browser print. | NV06/NV37; ADR-041/127. |
| F11 | Student | Own Class/LESSON-FINAL_EXAM/current-history/replacement; no transfer. | NV07; ADR-084/087/122. |
| F12 | Student | Own FINALIZED components/total/comment/result. | NV08/NV38; ADR-024/028. |
| F13 | Admissions | Paginated Registration search, all current states, permitted fields. | NV09; ADR-083/096/105. |
| F14 | Admissions | Registration detail/approve/reject/cancel đúng guard, Invoice effects. | NV09/NV12/NV37; ADR-017/022/031/083. |
| F16 | AcademicAffairs | Entry sessions/list/effective state/grade progress theo quyền. | NV14–NV15; ADR-030/083/086. |
| F17 | AcademicAffairs | Entry session create/schedule/open-close/cancel/grader assignment và result follow-up. | NV14–NV15; ADR-027/030. |
| F18 | AcademicAffairs | Course/program/Band/component weights-kind-count/lifecycle/freeze. | NV13; ADR-028/032/085/087. |
| F19 | AcademicAffairs | Class list/status/size/current program/Teacher. | NV16; ADR-025–026/085. |
| F20 | AcademicAffairs | Class create/update allowed fields/lifecycle, Teacher assignment. | NV16–NV17; ADR-023/025/085. |
| F21 | AcademicAffairs | Weekly/individual schedules/preview/move/replacement/actual occurrence. | NV19; ADR-032/036–037/087–088/122–123. |
| F22 | AcademicAffairs | Eligible paid Registration placement vào OPEN Class, wait if none. | NV18; ADR-023/040/088. |
| F23 | AcademicAffairs | Published-scope grade review/whole-Class return-finalize, no DRAFT values. | NV20/NV38/NV39; ADR-034/083/125. |
| F24 | Teacher | Current assigned Class/Student task fields/teaching calendar. | NV23; ADR-083/122. |
| F25 | Teacher | Assigned test sessions/roster/initial score/correction/absence. | NV21–NV22/NV36; ADR-027/029/086. |
| F26 | Teacher | One LESSON attendance batch, incomplete/missing state and read-only window. | NV24; ADR-033/124. |
| F27 | Teacher | Per-Student draft grades/comment/total; whole-Class submit/resubmit. | NV25–NV26/NV38/NV39; ADR-024/034/124–125. |
| F28 | Accountant | Operational Invoice lookup/full allowed finance detail, not report export. | NV27; ADR-083. |
| F29 | Accountant | Full external payment confirmation, metadata-only correction. | NV28; ADR-042/046. |
| F30 | Accountant | Finance metrics/filters/query context và authorized bounded CSV. | NV29; ADR-063–064/070/072/101/126. |
| F31 | Director | Admissions/training/finance non-identifying group reports, no export/approve. | NV30–NV32; ADR-065–071/126. |
| F32 | Admin | Staff create/reset/one-time secret, Account states, protection/revocation. | NV33–NV34; ADR-020/048–054/115/128. |
| F33 | Admin | Newest-first AuditLog list/detail/operation_id links, allowlist/retention scope. | NV35/NV39; ADR-060–062/116/129. |

F15 và NV10/NV11: **đã loại**, không URL/menu/API ghi danh hoặc đặt ca thay
của Admissions. Không renumber F16…F33. NV36–NV39 được kích hoạt bởi nghiệp
vụ có quyền; bảng mapping các màn hình đọc/ghi không cho phép gọi “system”
actions độc lập để bypass actor/guard (ADR-012/095/096).

## 10. Kết quả rà E01-v1 và phạm vi chuyển sang các E tiếp theo

Đây là kiểm tra tài liệu, không chạy application/test/SQL/CSV/browser.
Owner đã duyệt riêng đầu ra E01-v1 ngày30/09/2026; không suy từ các
phê duyệt policy A–C/D/Gói2/Gói3.

| Tiêu chí E01 | Bằng chứng trong bộ tài liệu |
| --- | --- |
| Đối chiếu 39 mã NV nguồn | Catalog giữ NV01–NV39; đặc tả có37 retained entries, NV10/NV11 explicit tombstones. |
| Đối chiếu 32 mã F | Phần9 từng F01–F33 trừ F15; không thêm dashboard/shared action codes. |
| Actor/module/scope | Phần1.1 + trường Actor/module và Đầu ra/phạm vi xem mỗi NV; automated NV36–NV39 không role mới. |
| Input/output/validation | Phần1.2 + từng NV ghi inputs, kết quả, editable/read-only, guard/state/time và validation/effects. |
| Quyền xem/sửa | ADR-083 restrictions được ghi ngay tại role functions; phần8 account menu/own profile; no leaking qua list/detail/calendar/preview/export. |
| Frontend đã duyệt | FE11–FE19 trong NV/parts8–9; all-or-none draft versus whole-Class grade; secret copy/ack/loss; calendar/report/print/audit scope. |
| Nguồn quyết định | Từng NV có ADR trace; accepted requirement rules/ADRs unchanged, requirements mục7 chỉ cập nhật link/trạng thái E01; không policy/ADR mới. |
| Tính nhất quán cần giữ | Không missing→0/present/absent, no effective-state cron dependency, current-state reports, Grade finalized-only, frozen used config/history preserved. |
| Handoff | Owner duyệt E01-v1 ngày30/09/2026; OWNER_REVIEW có E01 [x], active plan stage1 done; không cấp quyền code. |

Phần còn lại không được gọi là đã hoàn thiện chỉ từ E01:

- E02: full normal/exception flow, state transitions, actor/guards/effects,
  transaction/audit categorization, every concurrent/unknown-response path.
- E03: every field/type/nullability/PK-FK/check/unique/index/cardinality/ERD,
  current schedule/attempt/history/freeze và replay-store physical mapping.
- E04: endpoint/DTO/error/filter/sort/CSV wire contracts, auth/CSRF/PII/audit
  allowlists, routing/components, concrete operational/reference environment.
  D12/D13 theo ghi chú owner vẫn hoãn, không tự chốt tunnel/availability.
- E05: trace toàn requirement→workflow→data→system/verification và full-baseline
  acceptance. Chưa Git/scaffold/Slice0/migration/code/installation/deployment.

Không phát hiện nhu cầu mở lại các policy đã chốt trong lượt chuẩn hóa E01.
Owner đã chấp nhận E01-v1 trực tiếp ngày30/09/2026, stage1 done và tiếp E02.
Nếu sau này owner muốn đổi nội dung, ghi ngoại lệ vào chính dòng E01 ở
OWNER_REVIEW; không tạo bảng phiếu duyệt thứ hai. [E02-v1](WORKFLOW_SPEC.md)
đã được owner nghiệm thu riêng ngày30/09/2026; E03–E05 vẫn chờ đầu ra và
nghiệm thu riêng. Không suy quyền code từ E01/E02.

## 11. Addendum RF01/RF02/RF04 — Accepted 03/10/2026

Owner duyệt RF01/RF02/RF04, ghi ADR-157/158/159; đây là bổ sung có ngày cho
accepted E01, không hủy nghiệm thu E01 hoặc nghiệm thu E04/E05. Chi tiết
nguồn chuẩn [requirements mục4.9](../requirements.md), [ADRs](../DECISIONS.md).

| NV/action liên quan | Contract bổ sung |
| --- | --- |
| NV14 tạo ca | StartAt>serverNow khi kiểm tra lưu; equality/passed409, malformed400; mới CLOSED/no backfill. |
| NV14 đổi grader / NV22 correction entry score | Same grader hoặc parsed equal score400 field error sau valid current context; stale409; no effects/history/marker mới, required reason/original-current grader windows giữ nguyên. |
| NV03 Booking reschedule; NV13 Course/Program edits; NV16/NV17 Class capacity/Teacher; NV19 Schedule move; NV28 Payment metadata | All business values equal sau normalization/current guards400/no mutation/provenance/audit/completion. Reason alone không change. |
| NV24 Attendance / NV25 score-comment draft | Validate whole request/context before equality; mixed batch actual changes atomic, unchanged provenance retained; all-equal400/no marker; stale item aborts all; missing row0 actual change. Omitted comment giữ, NULL clear; parent report RV only real mutation. |
| Shared lifecycle/create/Admin reset | New intent wrong-source409; create không existing=success; deliberate reset theo accepted credential/revoke policy; completed same-key replay no repeated effects/secret. |

Không đổi 37NV/32F actors/rights/save units/audit coverage hoặc E03 storage.
Exact wire literal/shape/codec/signatures vẫn actual E04 refinement review.