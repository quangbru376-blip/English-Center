# E02-v1 — Đặc tả workflow nghiệp vụ để chủ dự án nghiệm thu

Ngày lập: 30/09/2026. Trạng thái: **owner nghiệm thu E02-v1 ngày 30/09/2026**.
Bổ sung đã duyệt 01/10/2026 (ADR-133): đường PENDING→TESTING và lượt hai giữ
BAND_ASSIGNED ở W05/W07/W08/matrix; giữ nghiệm thu E02-v1 và các guard hiện có.
Bổ sung đã duyệt 01/10/2026 (ADR-134): W15 tạo GradeReport DRAFT cùng giao
dịch xếp lớp/audit, không tạo sẵn Score/Attendance hoặc thêm quyền xem DRAFT.
Bổ sung đã duyệt 01/10/2026 (ADR-135): W18/W19 giữ final_score NULL trước
FINALIZED; tổng nháp/SUBMITTED tính từ Score, thiếu điểm không thành0.
Đây là tài liệu thiết kế, không phải hành vi ứng dụng đã chạy hoặc bằng chứng test.
Nguồn chuẩn về quyền, trường dữ liệu và validation: [requirements](../requirements.md),
[FUNCTION_SPEC E01-v1](FUNCTION_SPEC.md), [ADR-001–ADR-129](../DECISIONS.md).
[FUNCTION_CATALOG](FUNCTION_CATALOG.md) giữ 39 mã NV nguồn; NV10/NV11 và F15
đã loại, còn 37 chức năng NV và 32 màn hình F. Không dùng biểu mẫu Word để
thêm nghiệp vụ. Nếu cách diễn đạt E02 và một quyết định đã duyệt xung đột,
quyết định/requirements/E01 được đối chiếu trước; không âm thầm đổi policy.

## 1. Tóm tắt hiểu đúng và ranh giới

- Mục tiêu: diễn đạt các chức năng E01 thành hành trình thường, ngoại lệ và
  các chuyển trạng thái liên vai trò để kiểm tra tính nhất quán trước E03–E05.
- Người dùng: Student, Admissions, AcademicAffairs, Teacher, Accountant,
  Director, Admin; một Account đúng một role, hệ thống không là role thứ tám.
- Năm vòng đời chính: account/session; Registration/entry test;
  admission/Invoice/Payment; Class/placement/Schedule; Attendance/grades.
  Report và audit là các luồng xuyên suốt.
- Mỗi mutation phải kiểm tra lại actor, scope, state, thời gian, dữ liệu và
  xung đột tại commit. Preview không giữ chỗ; mất phản hồi không đồng nghĩa
  thất bại và không cho phép tạo hiệu ứng thứ hai.
- Ngoài phạm vi: thêm chức năng/quyền/trạng thái mới, triển khai, API/DDL
  vật lý, gateway/OTP/email recovery, học bổng, thang điểm 10, retake sau
  CONFIRMED, chuyển lớp/refund, phạt nợ tự động hoặc dịch vụ 24/7.
- E03 sẽ chốt dictionary/ERD, E04 chốt API/kiến trúc và D12/D13 vẫn hoãn
  theo nơi chạy phần mềm. E02 không cấp quyền Git/scaffold/code/deploy.

### Giả định và mục tiêu phi chức năng đã có nguồn

Không giả định nghiệp vụ mới. Mọi chi tiết không lặp lại trong E02 (độ dài
trường, làm tròn, thời gian, quyền đọc theo vai trò) dùng nguyên E01/requirements.
ADR-117 đặt phạm vi *kiểm chứng* một trung tâm, 10.000 học viên và 50 người
đồng thời; chưa có phép đo hiệu năng. Bảo mật/riêng tư theo role, ownership,
session/CSRF/cookie và audit ADR-092/099/115/116, không nới quyền vì URL
hoặc thao tác UI. Khả dụng cần cho buổi test/chỉnh sửa/demo trên laptop,
không hứa 24/7; backup/RPO/RTO chỉ là mục tiêu tham chiếu ADR-118, chưa có
bằng chứng chạy. Chủ dự án phụ trách vận hành mặc định ADR-120; D12/D13 và
chi tiết tunnel/backup destination chưa được chốt hoặc triển khai.

## 2. Sổ quyết định của lượt E02

| Nội dung | Đã thống nhất trong lượt rà | Phương án đã cân nhắc và lý do |
| --- | --- | --- |
| Phạm vi | Toàn bộ 37 NV còn hiệu lực, trace về 32 F; E02 chỉ là workflow. | Không thu hẹp vào một vài màn hình vì sẽ bỏ lỡ bàn giao liên vai trò. |
| Cách tổ chức | Theo năm vòng đời và report/audit xuyên suốt. | Mỗi NV riêng dễ trace nhưng lặp luồng; state/event-first chặt nhưng khó owner rà theo hành trình. Bảng trace khắc phục nhược điểm cách đã chọn. |
| Mẫu chung | Trigger/actor/prior state/guard/normal result/effect/audit/transaction/rejection/race/unknown response. | Không tạo template API hoặc SQL ở E02; chi tiết vật lý để E03/E04. |
| Các phần đã rà | Owner đã đồng ý lần lượt các đoạn account/session, Registration/test, admission/payment, Class/Schedule, Attendance/grades và report/audit/error. | Đồng ý từng đoạn không tự tích E02; chỉ nghiệm thu khi owner rà bản tổng hợp này. |

Sổ này ghi chấp nhận cách mô tả/đối chiếu quyết định đã có, **không tạo ADR
nghiệp vụ mới**. Rủi ro còn phải kiểm soát: bỏ sót nhánh khi nhiều đối tượng
đổi cùng lúc, không rõ kết quả sau timeout, lộ dữ liệu qua read/CSV/audit,
và nhầm trạng thái hiệu lực theo thời gian với state đã lưu.

## 3. Quy ước dùng trong mọi flow

Mã `Wxx` chỉ là mã tài liệu, không là NV/F/API/state mới. `Atomic` nghĩa là
mutation nghiệp vụ, các cập nhật phụ thuộc và audit bắt buộc thành công cùng
giao dịch hoặc cùng rollback theo ADR-093/115. Security-only events có ngoại
lệ best-effort ở W21. Mỗi flow dưới đây có actor, trigger, guard, đường đi
thường, ngoại lệ, ranh giới giao dịch, xung đột và cách xác minh khi mất phản
hồi; các kiểm tra E01 phần 1.2 luôn áp dụng. Mọi list/detail/preview/export
phải áp cùng ownership/scope; 404 che đối tượng ngoài scope, 403 cho role
không quyền, 409 cho state/stale/concurrency, 401 cho phiên không hợp lệ.

Một request quan trọng lặp cùng định danh/nội dung không tạo hiệu ứng mới;
định danh đó không bypass quyền/state hiện tại. Cùng định danh nhưng nội dung
khác trả conflict. Khi mất phản hồi, client đọc lại trạng thái có quyền và
operation context rồi mới quyết định bước tiếp, không đoán thành công/thất
bại; mật khẩu tạm tuyệt đối không lưu/replay để trả lại ở request lặp.
Không tự đồng bộ state bằng cron nếu quy tắc đã dùng effective guard: ca thi
đến start được coi CLOSED khi đọc/ghi dù stored OPEN; HELD chỉ do Giáo vụ
xác nhận, không suy từ giờ trôi qua hay Attendance.

### Ma trận chuyển trạng thái quan trọng

| Đối tượng | Đường hợp lệ chính | Điều kiện khóa/không suy diễn |
| --- | --- | --- |
| Registration | tạo PENDING → TESTING khi đặt ca đầu tiên → BAND_ASSIGNED khi có điểm → CONFIRMED → ADMITTED; CONFIRMED → REJECTED; hủy hợp lệ → CANCELLED. | ADR-133: lượt hai giữ BAND_ASSIGNED, guard chặn xác nhận khi chờ; hủy booking/vắng không kéo lùi hoặc xóa Band. Không enum COMPLETED; `is_unfinished` theo điều kiện đã chốt. CONFIRMED khóa retake/sửa Band; ADMITTED không đồng nghĩa PAID/đã xếp lớp. |
| EntryTestSession | tạo CLOSED; mở/đóng trước start; hủy trước start. | Start tạo effective CLOSED, không tự ghi CLOSED; CLOSED không cấm chấm hợp lệ. |
| EntryTestBooking/attempt | đặt SCHEDULED; hủy/đổi hợp lệ giữ history; vắng xác nhận ABSENT; thi có score COMPLETED. | Booking history khác logical attempt; vắng/center-cancelled đặt lại cùng lượt, tối đa hai lượt thực. |
| Invoice | phát hành UNPAID → PAID hoặc CANCELLED khi hủy hợp lệ. | Quá hạn là predicate/cảnh báo, không state/phạt; PAID không refund/cancel. |
| Class | PLANNED → OPEN → ONGOING → COMPLETED; PLANNED/OPEN trống/chưa dạy → CANCELLED. | Chuyển explicit và có guard; không reopen/transfer. |
| Schedule obligation | PLANNED → HELD hoặc NOT_HELD; NOT_HELD liên kết một replacement cùng nghĩa vụ. | Thời gian/Attendance không tự HELD; replacement không tạo extra obligation. |
| GradeReport | DRAFT → SUBMITTED → RETURNED → SUBMITTED → FINALIZED; vòng RETURNED có thể lặp. | Cả Class cùng chuyển; FINALIZED không mở lại; Student chỉ xem FINALIZED. |
| Account/session | ACTIVE/LOCKED/DISABLED theo Admin guard; session normal/restricted rồi logout/revoke/expire. | Timed login throttle không là LOCKED; unlock/restore không phục hồi phiên cũ. |

## 4. Vòng đời A — tài khoản và phiên

### W01 — Student tự đăng ký; Admin cấp nhân sự

- **Trigger/actor/guard:** khách tự gửi F02 để tạo Student; Admin gửi F32
  để cấp staff role cố định. Email/SĐT và trường bắt buộc theo E01; không
  đăng ký nhân sự tự phục vụ, không Admissions tạo Student thay.
- **Đường thường/effect:** tạo Account cùng StudentProfile của Student,
  hoặc Account nhân sự với mật khẩu tạm ngẫu nhiên 32 ký tự/hạn 24 giờ và
  forced-change. Secret chỉ hiển thị Admin một lần trong FE18.
- **Atomic/audit:** quan hệ Account/Profile, unique email/phone, trạng thái
  forced-change và audit tài khoản nhất quán; lỗi bắt buộc rollback.
- **Ngoại lệ/đua/mất phản hồi:** trùng email/phone kể cả concurrent bị từ chối.
  Với staff secret, không replay response hoặc tự tạo lại để lấy secret;
  kiểm Account/operation và nếu đã mất secret thì Admin thực hiện reset mới
  có xác nhận riêng.

### W02 — Đăng nhập, phiên, đăng xuất và tự đổi mật khẩu

- **Trigger/actor/guard:** 7 role dùng F01 email/password; Account ACTIVE,
  credential hợp lệ, IP/account throttle và temporary expiry còn hợp lệ.
  Server xác định role; lỗi trước xác thực dùng thông báo chung.
- **Đường thường/effect:** tạo AuthSession revocable và cookie bảo vệ theo
  ADR-092/099. Phiên normal tối đa 2 giờ/idle 30 phút; restricted tối đa 10
  phút và không vượt hạn mật khẩu tạm, chỉ đổi mật khẩu/logout. User-initiated
  authenticated business request mới gia hạn idle. Logout revoke current
  session/cùng profile tabs; device/profile khác độc lập.
- **Atomic/audit:** đổi mật khẩu đúng current credential cập nhật hash/
  must-change và revoke mọi session/version cùng audit bắt buộc; trở về login
  mới, không nâng restricted session tại chỗ. Pure login/logout/security log
  best-effort nhưng không bypass auth hay hồi sinh phiên khi log lỗi.
- **Ngoại lệ/đua/mất phản hồi:** 5 lần sai/15 phút chặn login mới 15 phút,
  IP 100/phút trả 429/Retry-After; không đổi Account thành LOCKED hoặc revoke
  phiên hiện có. Hết hạn/revoke chặn business request và UI trở lại login;
  không khôi phục secret/nháp qua browser storage. Retry đọc lại phiên/state,
  không giả định timeout đã đổi mật khẩu thành công.

### W03 — Admin reset, khóa, mở, ngừng dùng và khôi phục Account

- **Trigger/actor/guard:** Admin trên F32, không khóa/disable chính mình
  hoặc Admin ACTIVE cuối cùng. Reset target không tự unlock/reactivate.
- **Đường thường/effect:** reset tạo mật khẩu tạm một lần, revoke mọi phiên;
  lock/disable cũng revoke mọi phiên. Unlock/restore cùng record cho phép
  login mới nhưng không phục hồi token cũ; không tự đổi Teacher/Class/session
  assignment hoặc dữ liệu nghiệp vụ.
- **Atomic/audit:** recheck trạng thái, last-Admin invariant, quyền target và
  phiên liên quan tại commit. Account mutation + revocation + audit bắt buộc
  cùng giao dịch; thao tác cạnh tranh chỉ một kết quả hợp lệ.
- **Ngoại lệ/đua/mất phản hồi:** không hiển thị lại plaintext sau đóng/refresh/
  timeout. Inspect Account/operation trước; không auto retry reset để lấy
  secret. Nếu cần, Admin chủ đích reset lần mới; không thêm recovery email.

### W04 — Hồ sơ cá nhân, trang chủ và các phép đọc chung

Student sửa đúng full_name/DOB/phone/address của mình theo E01 phần 8.3,
email read-only; lưu có validation/ownership và không âm thầm merge bản cũ.
F03 dựng thẻ từ từng Registration và next action đúng guard hiện tại; không
state tiến độ mới, không hiện grade chưa FINALIZED. Account menu đổi mật khẩu/
logout đúng W02. Các phép đọc không tự chuyển state hoặc tạo audit mọi page
view; lỗi quyền không lộ dữ liệu qua list/detail/calendar/print.

## 5. Vòng đời B — hồ sơ đăng ký và thi đầu vào

### W05 — Chọn Course, tạo và hủy Registration của mình

- **Trigger/actor/guard:** Student chọn Course ACTIVE tại F04/F05; chỉ một
  Registration chưa kết thúc cho cùng Student/Course. Course của hồ sơ cũ
  bất biến. Student hủy chỉ trước CONFIRMED; không paid cancellation/refund.
- **Đường thường/effect:** tạo Registration riêng ở PENDING (ADR-133);
  không mang score/Band từ hồ sơ cũ. Hủy hợp lệ thành CANCELLED; hủy active
  future bookings kể cả ca CLOSED trước start và Invoice UNPAID nếu có,
  giải phóng chỗ, giữ started test/score/Band/history và khóa grading trên
  hồ sơ đã hủy. Preview liệt kê ảnh hưởng đúng phạm vi.
- **Atomic/audit:** Registration/is_unfinished, booking/Invoice dependent
  updates, capacity và mandatory audit cùng transaction. Không partial.
- **Ngoại lệ/đua/mất phản hồi:** unique unfinished predicate chặn hai yêu
  cầu tạo đồng thời; race booking start/payment/cancellation được recheck.
  Nếu trạng thái đã đổi, trả conflict và tải lại hồ sơ/booking/Invoice.

### W06 — Giáo vụ quản lý EntryTestSession

- **Trigger/actor/guard:** AcademicAffairs tạo ca với room/Teacher ACTIVE,
  capacity 1–20, giờ cùng ngày hợp lệ và không overlap; ca mới CLOSED.
  Mở/reopen chỉ trước start, đủ tài nguyên/capacity. Đóng OPEN trước start
  giữ booking; hủy chỉ trước start.
- **Đường thường/effect:** mở cho Student đặt; đóng ngăn đặt/hủy/đổi bởi
  Student nhưng vẫn cho Teacher chấm/vắng/correction nếu guard tương ứng đạt.
  Hủy ca giữ history, hủy booking hoạt động và cho Student tự đặt lại cùng
  logical attempt; không auto rebook. Đổi grader có lý do chỉ trước start
  và trước mọi score/ABSENT; sửa date/time/room chỉ trước start và chưa có
  booking. At start, ca effectively CLOSED dù stored OPEN.
- **Atomic/audit:** session/resource, booking effects và mandatory audit
  cùng commit khi mutation yêu cầu; read không tự ghi CLOSED.
- **Ngoại lệ/đua/mất phản hồi:** recheck room/Teacher overlaps, booking
  count, start cutoff và grader hiện tại; stale preview/conflict không lưu
  một phần. Không suy ca đầy thành state khác.

### W07 — Student đặt, hủy, đổi ca và đặt lại cùng lượt

- **Trigger/actor/guard:** Student trên F06/F07 với Registration của mình
  chưa CONFIRMED; ca đích OPEN/chưa start/còn chỗ, không trùng ca thi/lịch
  lớp của Student. Lượt hai cần COMPLETED score lượt một; tối đa hai logical
  attempts. Hủy/đổi booking nguồn chỉ khi OPEN và trước start.
- **Đường thường/effect:** đặt một current SCHEDULED booking cho lượt đang
  làm; đổi/hủy giữ booking history và cập nhật chỗ nhất quán. ABSENT do
  Teacher xác nhận hoặc center-cancelled cho tự đặt lại *cùng lượt*, không
  tính lượt thứ ba. CLOSED hoặc thời điểm start chặn mọi Student booking
  mutation, kể cả ca còn chỗ; Registration cancellation W05 có dependent
  cancellation riêng kể cả CLOSED trước start.
- **Bổ sung ADR-133:** booking đầu tiên chuyển PENDING→TESTING cùng giao
  dịch. Đặt lượt hai giữ BAND_ASSIGNED và guard xác nhận khi còn chờ; hủy
  booking/vắng không kéo về PENDING hoặc xóa Band đã có.
- **Atomic/audit:** Registration state, capacity, current booking/attempt, Student conflict,
  history và audit theo contract C13 trong một thao tác, không partial.
- **Ngoại lệ/đua/mất phản hồi:** hai booking cùng lượt/chỗ cuối, ca vừa
  đóng/bắt đầu, Registration vừa CONFIRMED/hủy phải recheck và conflict.
  Đọc lại booking hiện tại trước khi tiếp tục, không auto chuyển ca.

### W08 — Teacher chấm, ghi vắng, sửa điểm; hệ thống tính Band

- **Trigger/actor/guard:** Teacher hiện assigned tại F25. Initial score
  0–100 hoặc xác nhận ABSENT chỉ từ session end; session không CANCELLED,
  Registration chưa CONFIRMED/REJECTED/CANCELLED. CLOSED không cản chấm.
  Correction cần original score Teacher vẫn assigned, COMPLETED result,
  Registration TESTING/BAND_ASSIGNED và lý do.
- **Đường thường/effect:** score COMPLETED gắn đúng Registration/attempt/
  booking; tính best COMPLETED score và Band theo ngưỡng lớn nhất ≤ score.
  Đồng điểm chọn attempt 1 nhưng giữ cả hai kết quả. First completed result
  khóa Band threshold set theo policy; correction hợp lệ tính lại best/Band
  nguyên tử. ABSENT giữ lượt để Student tự đặt lại, không là score 0.
- **Bổ sung ADR-133:** kết quả điểm đầu tiên chuyển Registration sang
  BAND_ASSIGNED cùng result/best/Band; kết quả lượt hai giữ BAND_ASSIGNED.
- **Atomic/audit:** result/booking, best-score, Band, first-use freeze và
  mandatory old-new/reason audit cùng transaction, không công bố partial.
- **Ngoại lệ/đua/mất phản hồi:** race với confirmation/cancellation/grader
  replacement/config freeze được serialize/recheck; không correction sau
  CONFIRMED, không override bởi Admin/AcademicAffairs, không tự đánh vắng
  từ ô điểm trống. Student F08 chỉ đọc kết quả hồ sơ mình.

### W09 — Student xác nhận nhập học sau Band

- **Trigger/actor/guard:** Student sở hữu Registration BAND_ASSIGNED có
  Band hợp lệ. Không có lượt hai SCHEDULED hoặc đã thi chưa chấm, kể cả
  ca CLOSED; không tự nhập Band/giá/thời gian.
- **Đường thường/effect:** chuyển CONFIRMED với actor/server time; Course
  và kết quả đầu vào của hồ sơ được khóa theo policy. Chưa ADMITTED, chưa
  Invoice/Payment/Class. StudentProfile riêng vẫn sửa được phần cho phép.
- **Atomic/audit/ngoại lệ:** recheck result/best/Band/booking và race W07/W08
  tại commit; conflict giữ trạng thái cũ, audit theo policy. Timeout thì
  đọc Registration trước, không suy xác nhận đã hoặc chưa thành công.

## 6. Vòng đời C — tuyển sinh, hóa đơn và thanh toán

### W10 — Admissions tra cứu, duyệt, từ chối và hủy hồ sơ

- **Trigger/actor/guard:** Admissions F13/F14 chỉ dữ liệu được ADR-083 cho
  xem. Duyệt/từ chối chỉ Registration CONFIRMED có Band; từ chối/hủy cần
  lý do. Hủy CONFIRMED hoặc ADMITTED có Invoice UNPAID; không hủy đã PAID.
- **Đường thường/effect:** duyệt chuyển ADMITTED và kích W11 cấp đúng một
  Invoice UNPAID; từ chối thành REJECTED/lý do chủ hồ sơ được xem. Hủy
  thành CANCELLED cùng Invoice UNPAID/active future bookings, giữ lịch sử
  thi/score/Band; booking đã start giữ history nhưng không tiếp tục grading
  cho hồ sơ hủy. Duyệt không là PAID/xếp lớp.
- **Atomic/audit:** Registration, Invoice issuance hoặc dependent cancellation,
  is_unfinished và mandatory audit cùng operation_id/giao dịch; fail một
  phần thì không chuyển trạng thái.
- **Ngoại lệ/đua/mất phản hồi:** recheck Band, Payment, booking start,
  Invoice uniqueness và current state. Approval replay không tạo Invoice
  thứ hai; race thanh toán/hủy chỉ một kết quả hợp lệ. Timeout đọc lại
  Registration/Invoice/operation trước.

### W11 — Hệ thống phát hành; Student/Accountant xem Invoice

- **Trigger/actor/guard:** W10 approval hợp lệ là nguồn phát hành duy nhất.
  Server lấy Course tuition dương tại issue time, số Invoice duy nhất và
  hạn bằng ngày phát hành VN + 7 ngày lịch, snapshot amount VND nguyên đồng.
- **Đường thường/effect:** một Invoice UNPAID/Registration, không Payment.
  Student F10 chỉ xem/in bản của mình sau khi fetch mới theo quyền, ghi rõ
  UNPAID không là biên nhận; Accountant F28 tra cứu finance detail đúng role.
  Overdue chỉ UNPAID với due date trước ngày VN hiện tại: cảnh báo, không
  state mới, tự hủy, tăng tiền hay phạt.
- **Atomic/audit/ngoại lệ:** issuance nằm trong W10 transaction; race đổi
  giá/approval xử lý snapshot nhất quán, Invoice number unique non-reused
  (gaps allowed). Lỗi đọc mới hoặc mất quyền thì không in bản cũ như mới.

### W12 — Accountant xác nhận thu tiền và sửa metadata

- **Trigger/actor/guard:** tiền nhận bên ngoài ứng dụng. Accountant F29
  chọn Invoice UNPAID/Registration hợp lệ, xác nhận đúng toàn bộ amount
  snapshot một lần; CASH hoặc TRANSFER (reference bắt buộc khi TRANSFER).
- **Đường thường/effect:** một Payment/Invoice có confirmer/time server,
  Invoice thành PAID. Correction sau đó chỉ method/reference với mandatory
  reason và history old-new; không đổi amount/paid_at/link, không refund.
- **Atomic/audit:** Payment creation + PAID + mandatory audit; correction +
  audit riêng, all-or-none. Không duplicate Payment khi replay.
- **Ngoại lệ/đua/mất phản hồi:** recheck Invoice/Registration và race W10
  cancellation/confirmation/metadata edit; stale state conflict. Timeout
  đọc lại Invoice/Payment trước, không gửi lệnh mới mù; không auto gateway,
  partial payment hoặc sửa doanh thu bằng metadata correction.

## 7. Vòng đời D — cấu hình đào tạo, lớp, xếp lớp và lịch

### W13 — Course, TrainingProgram, Band và cấu hình điểm

- **Trigger/actor/guard:** AcademicAffairs F18 quản lý Course ACTIVE/INACTIVE,
  TrainingProgram DRAFT/ACTIVE/INACTIVE theo Course/Band, component và
  weight/kind/expected LESSON count. Course code bất biến; chương trình
  ACTIVE phải hợp lệ theo E01 và muốn sửa khi chưa dùng phải về DRAFT.
- **Đường thường/effect:** giá Course mới chỉ cho Invoice phát hành sau,
  không sửa snapshot cũ. First Class use khóa Course/Band/components/weights/
  count của program kể cả Class sau đó CANCELLED; first COMPLETED entry
  result khóa cả Band threshold set. INACTIVE chặn Class mới theo guard,
  không tự dừng Class đang dạy hoặc giải freeze khi restore.
- **Atomic/audit/ngoại lệ:** unique Course code/program per Course-Band,
  weights sum 100, một FINAL_EXAM component và phụ thuộc được recheck khi
  bật ACTIVE/first use/sửa. Config mutation + freeze + mandatory audit
  nhất quán; race first-use/edit không tạo config nửa cũ nửa mới.

### W14 — Tạo/mở/bắt đầu/hoàn tất/hủy Class và đổi Teacher

- **Trigger/actor/guard:** AcademicAffairs F19/F20 tạo Class PLANNED từ
  program và một Teacher hợp lệ, max_size 1–15. PLANNED→OPEN cần program
  ACTIVE, Teacher hợp lệ, ít nhất một future LESSON. OPEN→ONGOING là lệnh
  explicit không trước first LESSON actual start, có ≥1 placed Student và
  đủ expected LESSON schedule.
- **Đường thường/effect:** Class ONGOING→COMPLETED khi mọi LESSON/FINAL_EXAM
  obligation bắt buộc HELD và required LESSON Attendance đủ; chưa cần grades
  SUBMITTED/FINALIZED. Hủy chỉ PLANNED/OPEN trống, chưa dạy; giữ lịch/history
  nhưng giải phóng upcoming resources, không refund/transfer paid Student.
  Đổi Teacher chỉ trước ONGOING/actual teaching và chưa Attendance/Score;
  Account blocked không tự reassign.
- **Atomic/audit/ngoại lệ:** recheck placement/capacity/program/Teacher/
  schedule/occurrence và last relevant state khi commit; mutation/history/
  mandatory audit cùng giao dịch. Concurrent placement vs cancellation/start,
  Teacher lock/change và held/attendance vs completion gây conflict hợp lệ,
  không tự suy class state từ đồng hồ.

### W15 — Xếp Student đủ điều kiện vào Class

- **Trigger/actor/guard:** AcademicAffairs F22 chọn Registration ADMITTED,
  Invoice PAID, đúng Course/Band và Class OPEN/còn chỗ. Student không được
  tự đặt lớp; không xếp PLANNED/ONGOING/ended hoặc gỡ/chuyển sau xếp.
- **Đường thường/effect:** tạo đúng một StudentClassAssignment/Registration,
  Class size tăng; Registration vẫn ADMITTED và “Đã xếp lớp” là chỉ báo.
  Student F11/Teacher F24 thấy lịch/roster đúng scope. Không có lớp phù hợp
  thì hồ sơ chờ, không auto mở lớp/đổi Band/hoàn tiền.
- **Bổ sung ADR-134:** tạo GradeReport DRAFT của assignment mới cùng giao
  dịch; không khởi tạo Score/Attendance hay mở quyền xem giá trị DRAFT.
- **Atomic/audit/ngoại lệ:** recheck capacity, PAID, program, Student
  conflicts với test/class schedule và Class transition; assignment + DRAFT
  report + mandatory audit cùng transaction. Size được đếm từ assignment,
  không cache riêng (ADR-134). Hai thao tác tranh chỗ cuối/hồ sơ cùng
  lúc không tạo over-capacity/duplicate assignment; timeout đọc lại.

### W16 — Lập lịch, dời/thay buổi và xác nhận occurrence

- **Trigger/actor/guard:** AcademicAffairs F21 nhập LESSON weekly date range/
  weekdays/time/room hoặc từng buổi; FINAL_EXAM riêng sau last current
  LESSON. Preview trả ngày/buổi/count/conflicts cụ thể, không giữ chỗ. Tổng
  LESSON obligations không vượt expected program count; đủ trước teaching.
- **Đường thường/effect:** Tạo tuần lưu toàn bộ hoặc không buổi nào; sửa input
  làm preview cũ hết hiệu lực. Dời pre-start chỉ khi không Attendance/linked
  Score và Class chưa ended. Buổi tới start nhưng không tổ chức ghi lý do,
  giữ NOT_HELD/history và tạo linked future replacement cùng Class/type/
  obligation; old và replacement không tính thành hai buổi. Sau scheduled
  end và Class ONGOING, Giáo vụ xác nhận HELD từng occurrence, không undo.
- **Atomic/audit/ngoại lệ:** kiểm [start,end) theo phút/cùng ngày VN, room/
  Teacher/Class/Student test-and-class overlaps ở preview và commit. Weekly
  batch, move, replacement hoặc HELD cùng dependent history/audit nhất quán.
  409 yêu cầu xem lại preview, không skip ngày/lưu partial. Không cron/elapsed
  time/Attendance tự xác nhận HELD, không holiday automation/extra lesson.

- **Addendum03/10/2026 — ADR-156/E04-RF03:** root LESSON/FINAL_EXAM mới,
  mọi candidate tuần và đích dời phải startAt>serverNow lúc kiểm tra lưu trong
  transaction; equality/passed sau input hợp lệ409, malformed date/time400.
  Preview không giữ clock/resource; recheck independent RV. Một candidate
  không đạt thì weekly batch không lưu phần nào/không skip/trim/auto move.
  Không backfill; old-slot move/replacement future/HELD after-end giữ nguyên.
  Bổ sung này được duyệt sau E02-v1 đã nghiệm thu30/09, không nghiệm thu E04.

## 8. Vòng đời E — Attendance và kết quả đào tạo

### W17 — Teacher ghi/cập nhật Attendance một LESSON

- **Trigger/actor/guard:** Teacher hiện assigned F26, từ đúng LESSON start
  đến trước Class COMPLETED; LESSON không NOT_HELD, Class không CANCELLED.
- **Đường thường/effect:** lưu các Student được chọn Có mặt/Vắng (`1/0`),
  Student chưa ghi vẫn thiếu record, không ngầm có mặt/vắng. Một lần lưu là
  một batch cho *một LESSON*, tất cả giá trị đã nhập/thay đổi cùng thành công
  hoặc rollback. Có thể lưu một phần roster theo chủ đích; thiếu Attendance
  chặn Class completion. Attendance không tự HELD.
- **Atomic/audit/ngoại lệ:** unique Student/LESSON, assignment/current
  schedule/window recheck khi ghi; concurrent Class completion/move/Teacher
  loss conflict. Không autosave, default-all, blank→absent hay tự mở cửa sổ
  24/48 giờ mới.

### W18 — Teacher lưu nháp, gửi và gửi lại điểm cả Class

- **Trigger/actor/guard:** Teacher current assignment F27. Per-Student draft
  có changed component scores/comment khi Class ONGOING/COMPLETED và report
  DRAFT/RETURNED; schedule-linked component từ current relevant occurrence
  start, đúng kind/type và không NOT_HELD. GENERAL không cần schedule.
- **Đường thường/effect:** changed scores + comment của một Student lưu
  all-or-none; thiếu component vẫn là Chưa đủ điểm, blank không thành 0/xóa
  score đã lưu. Server tính total tạm từ Score; final_score vẫn NULL trước
  FINALIZED (ADR-135). Khi Class COMPLETED, đủ mọi
  Student/component, không còn local unsaved edit, Teacher xác nhận gửi *cả
  Class*: toàn bộ DRAFT/RETURNED→SUBMITTED; resubmit RETURNED có thể lặp sau
  mỗi lần return. Không publish cho Student khi SUBMITTED.
- **Atomic/audit/ngoại lệ:** per-Student save và whole-Class submit là hai
  transaction granularity khác nhau. Whole-Class transition + history/
  operation_id/audit bắt buộc all-or-none. Recheck cohort, program freeze,
  assignment/schedule/state và stale edit; không individual submit/auto
  submit khi đủ điểm hoặc ghi đè form cũ.
- **Bổ sung ADR-135:** lưu điểm/nhận xét cập nhật parent GradeReport version;
  metadata giữ lần gửi/review gần nhất, audit giữ mọi lần theo operation lớp.

### W19 — Giáo vụ trả/chốt bảng điểm; hệ thống tính và công bố

- **Trigger/actor/guard:** AcademicAffairs F23 chỉ review Class COMPLETED
  với toàn bộ GradeReports SUBMITTED; không xem Teacher DRAFT values.
- **Đường thường/effect:** return với mandatory reason chuyển cả cohort
  RETURNED, giữ score/comment và history; Teacher sửa/gửi lại ở W18.
  Finalize chuyển mọi report cùng lúc FINALIZED/published, tính final_score
  theo frozen weights với decimal intermediate, half-up hai chữ số *một lần*;
  ≥50 là đạt, không minimum component/attendance passing gate. Student F12
  chỉ thấy own FINALIZED values/comment. Registration chỉ hết unfinished khi
  Class COMPLETED và GradeReport của lượt học đó FINALIZED, dù không đạt.
- **Bổ sung ADR-135:** chỉ khi FINALIZED mới lưu final_score; trước đó total
  được tính khi đọc. Passing suy từ finalized score, không thêm cột đạt.
- **Atomic/audit/ngoại lệ:** whole-Class transition, calculated totals,
  publish, is_unfinished và mandatory grouped audit cùng giao dịch. Recheck
  required components/cohort/current reports/score edits; lỗi hoặc race
  không partially finalize/publish. FINALIZED không reopen/override; Director
  và Admin không approval tier thứ hai.

## 9. Luồng xuyên suốt — báo cáo, audit và lỗi

### W20 — Accountant/Director đọc báo cáo, Accountant xuất CSV

- **Trigger/actor/guard:** Accountant F30 áp dụng bộ lọc báo cáo Finance,
  có thể xuất CSV theo quyền. Director F31 áp dụng bộ lọc ba nhóm Admissions/
  Training/Finance chỉ ở mức tổng hợp không định danh; không export hoặc
  xem Invoice/Payment/Student records cá nhân. Report filters và Course
  history theo E01; các ô đang sửa chưa thay result cho tới khi Áp dụng.
- **Đường thường/effect:** query snapshot nhất quán, `queried_at`, tổng trên
  toàn filtered set chứ không chỉ current page. Admissions count distinct
  Student đúng cohort; Training inventory/current HELD obligations tách
  finalized-results period; Finance revenue theo Payment confirmation date,
  billed theo non-CANCELLED Invoice issue date, debt/overdue theo UNPAID
  *hiện tại*. Không lấy billed−period revenue làm debt/as-of balance.
- **CSV/ngoại lệ:** Accountant CSV chạy query mới cùng định nghĩa/cột report,
  UTF-8 BOM/comma/CRLF/escape/formula safeguards; vượt 10.000 data rows
  hoặc 5 MB thì từ chối toàn file, không partial. Fresh snapshot có thể khác
  trang đang mở và ghi thời điểm riêng. Empty denominator là Chưa có dữ
  liệu, không 0% giả; Director không CSV, mọi report không lộ detail ngoài
  scope hoặc thêm chart/PDF/XLSX.

### W21 — Ghi và xem audit đúng loại sự kiện

- **Trigger/actor/guard:** mutation nghiệp vụ/tài khoản từ W01–W19 sinh
  audit bắt buộc theo ADR-093/115; login/logout/throttle/denied-access thuần
  bảo mật là best-effort. Actor chưa xác thực là NULL, không lấy email vừa
  đoán làm verified actor.
- **Đường thường/effect:** mandatory event lưu cùng transaction với mutation,
  old/new/reason theo allowlist và shared operation_id cho dependent/whole-
  Class effects. Nếu audit bắt buộc lỗi, rollback mutation, gồm liên quan
  session revocation. Pure security-log lỗi dùng fallback có giới hạn/không
  secret, không mở quyền, khôi phục logout hoặc báo sai là nghiệp vụ đã lỗi.
- **Admin read/retention:** F33 newest-first/paginated, event detail và
  related operation links nhưng không mở quyền xem đối tượng nghiệp vụ khác.
  Không password/hash/JWT/raw request-response. Mandatory business/account
  history được giữ; pure security log/IP retention 90 ngày do vận hành dọn,
  IP hết hạn thể hiện Không còn lưu. Không app purge/edit/undo/export audit.

### Quy tắc lỗi và kết quả không rõ cho mọi W

| Tình huống | Phản ứng thiết kế bắt buộc |
| --- | --- |
| Validation field/input | Trả lỗi trường/toàn form; không tự cắt, round hoặc biến blank thành 0/vắng. Không mutation partial. |
| Phiên không hợp lệ/role sai/ngoài ownership | 401/403/404 theo ADR-096; list/detail/preview/export không lộ object ngoài scope. |
| Stale state/capacity/overlap/last-Admin/invariant race | 409, đọc lại current authorized state/preview, giữ lịch sử; không overwrite hoặc tự chọn phương án thay owner. |
| Rate limit login | 429/Retry-After, response trước xác thực không tiết lộ email/status. |
| Replay cùng nội dung | Không tạo hiệu ứng thứ hai; kiểm quyền/current state theo ADR-093. Khác nội dung với cùng request identity →409. |
| Timeout/unknown response | Đọc lại object/operation có quyền; không đoán thất bại. Không replay plaintext secret; Admin reset mới chỉ theo hành động có chủ đích. |
| Mandatory audit/commit fail | Rollback toàn bộ mutation và dependent effects; không báo success giả. Pure security-event logging có cơ chế best-effort riêng. |

## 10. Truy vết và ca kiểm chứng thiết kế

Một NV có thể xuất hiện ở nhiều W khi gồm cả đọc và ghi. Mã F là màn hình
E01 đã duyệt, không tạo route mới từ bảng này. F15/NV10/NV11 bị loại.

| Flow | NV được bao phủ | F có liên quan |
| --- | --- | --- |
| W01 | NV01, NV33 | F02, F32 |
| W02 | NV01; thao tác chung đổi mật khẩu/logout | F01 |
| W03 | NV33–NV34 | F32 |
| W04 | NV02–NV08 (projection), thao tác chung profile/F03 | F03 |
| W05 | NV02 | F04–F05 |
| W06 | NV14–NV15, NV21 | F16–F17, F25 |
| W07 | NV03 | F06–F07 |
| W08 | NV04, NV15, NV22, NV36 | F08, F16–F17, F25 |
| W09 | NV05 | F09 |
| W10 | NV09, NV12, NV37 | F13–F14 |
| W11 | NV06, NV27, NV37 | F10, F28 |
| W12 | NV28 | F29 |
| W13 | NV13 | F18 |
| W14 | NV16–NV17, NV23 | F19–F20, F24 |
| W15 | NV18 | F22 |
| W16 | NV07, NV19, NV23 | F11, F21, F24 |
| W17 | NV24 | F26 |
| W18 | NV25–NV26, NV38 | F27 |
| W19 | NV08, NV20, NV38 | F12, F23 |
| W20 | NV29–NV32 | F30–F31 |
| W21 | NV35, NV39; audit áp mọi mutation phù hợp | F33 |

Các ca sau là **điều cần kiểm chứng về sau**, chưa có ứng dụng hoặc test pass:

| Nhóm | Ca tối thiểu và bất biến cần chứng minh |
| --- | --- |
| Account/session | Email/SĐT trùng đồng thời chỉ một Account; last ACTIVE Admin/self-lock bị chặn; đổi/reset/lock revoke phiên; logout khác device không bị kéo theo; lost reset response không lộ lại secret. |
| Entry booking | Hai Student tranh chỗ cuối; OPEN→CLOSED/start cutoff lúc đặt/đổi; hủy Registration với future CLOSED booking; ABSENT/center-cancelled rebook cùng attempt, không attempt3/missing→0. |
| Entry result | Correction vs Student confirmation/cancellation; original grader condition; tied best attempt1; first completed result freeze Band set; mutation result/Band/audit không partial. |
| Admission/finance | Duyệt lặp chỉ một Invoice; Course price update vs issuance snapshot; cancel ADMITTED UNPAID vs Accountant payment; Payment replay chỉ một; correction metadata không đổi amount/date. |
| Class/placement | Hai placement tranh chỗ cuối; payment/Class state thay đổi cùng lúc; blocked Teacher không auto reassignment; OPEN→ONGOING guard và empty cancel guard. |
| Schedule/attendance | Weekly preview stale hoặc một conflict → không buổi nào tạo; replacement cùng obligation, HELD explicit; Attendance một LESSON batch lỗi rollback, missing không absent; completion race. |
| Grades | Per-Student save all-or-none, whole-Class submit/return/finalize all-or-none; unsaved edit/stale reviewer conflict; 49.995→50.00 đạt và 49.994→49.99 không đạt khi FINALIZED; chưa FINALIZED không lộ Student/Director. |
| Reports/audit/privacy | Totals trên toàn filtered set/query snapshot; current debt khác period revenue; CSV cùng metric/limit toàn file; F33 allowlist/IP retention; mandatory-audit fail rollback, pure security-log fail không bypass auth. |

## 11. Ranh giới nghiệm thu E02-v1

Tài liệu này bao phủ 37 NV còn hiệu lực qua W01–W21 và 32 F qua bảng trace;
quy tắc chi tiết E01/requirements/ADR vẫn có hiệu lực. Sau khi rà các phần,
owner đã đồng ý **bản E02-v1 hoàn chỉnh** ngày 30/09/2026; dòng E02 trong
[OWNER_REVIEW](../plan/OWNER_REVIEW.md) được tích và stage 2 `done`.
Đây là nghiệm thu đầu ra thiết kế, không phải xác nhận ứng dụng đã chạy.
E03 dictionary/ERD, E04 kiến trúc/API và E05 rà toàn baseline vẫn chưa có
đầu ra được nghiệm thu; D12/D13 tiếp tục hoãn. Các ca ở mục 10 là ca kiểm
chứng thiết kế, không phải kết quả test; chưa code/Git/scaffold/migration.





## 12. Addendum RF01/RF02/RF04 — Accepted 03/10/2026

Accepted ADR-157/158/159 bổ sung behavior cho E02 đã nghiệm thu; không sửa
schema E03 hoặc tự nghiệm thu E04/E05. Canonical [requirements mục4.9](../requirements.md)
và [DECISIONS](../DECISIONS.md); không hỏi lại ba RF đã duyệt.

- W06 create: startAt>serverNow tại saving-transaction validation, equality/
  passed409/malformed400, CLOSED/resources/Teacher/capacity/no backfill giữ nguyên.
  W06 grader change và W08 correction same parsed value400 field error sau valid
  current context, no DML/provenance/mandatory mutation audit/new completion;
  stale/time/state/member409 và current-original-grader/reason guards giữ nguyên.
- W07 reschedule/W12 metadata/W13 Course-Program edit/W14 capacity-Teacher/
  W16 move: all-equal business values400/no actual mutation/history/marker.
  Reason alone không change. Current rights/time/versions/relations trước equality.
- W17 Attendance và W18 draft: validate entire request, every supplied item's
  versions/expected absence/full cohort/dependencies/clock trước no-change check.
  Mixed deltas actual changed subset commit cùng save unit, unchanged provenance
  giữ nguyên; một invalid/stale aborts all; entirely equal400/no marker. Missing
  row với0 vẫn change; comment omitted unchanged/null clears; only real TX11
  score/comment changes touch parent report RV. Audit actual changes giữ policy.
- Cross-cutting new-key transition wrong source409; create không row-existing
  success; W03 deliberate reset vẫn cấp temp/revoke theo policy. Completed same
  key/content replay nhận quyền hiện tại trước old-version/new-mutation guards,
  no duplicate effects/audit/secret; no-change new intent không confused replay.

Digest giữ **toàn normalized supplied intent**, không bỏ unchanged items theo
live DB trước hashing; equality thuộc nhánh new mutation. Behavior đã Accepted;
exact codec/error strings/response shapes vẫn draft cho actual E04 review.