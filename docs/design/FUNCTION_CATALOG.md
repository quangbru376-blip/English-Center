# Danh mục chức năng nghiệp vụ

Cập nhật: 30/09/2026. Trạng thái: **chỉ mục/truy vết E01-v1 đã duyệt**.
Đặc tả chi tiết được owner duyệt: [FUNCTION_SPEC](FUNCTION_SPEC.md).
Stage1/2 done; [E02-v1 workflow](WORKFLOW_SPEC.md) đã được owner nghiệm thu
ngày30/09/2026, E03–E05 vẫn chưa có đủ đầu ra.

Nguồn: phần I–III và V của báo cáo chính thức, đối chiếu với
`../requirements.md` và ADR-007–ADR-129. Không dùng phần IV (biểu mẫu).
Quy tắc đã chốt vẫn thuộc requirements/ADRs; tài liệu này trình bày chi tiết
chức năng và khoảng trống, không tự tạo quyền hoặc thay thế quyết định đó.


A01–A10/B01–B07/C01–C16 đã duyệt ngày 28/09/2026 (ADR-083–ADR-094).
Catalog đã đồng bộ lựa chọn; E01 được owner nghiệm thu 30/09/2026. Các lựa chọn thuộc
Q-004–Q-010 đã được giải quyết bởi A–C ở mục 10 dưới đây, không hỏi lại.
Gói 1 D01–D04/D06–D07/D10/D15 duyệt ADR-095–ADR-102 ngày 29/09/2026.
D05/D08/D09/D11/D14/D16/D17 duyệt riêng ADR-114–ADR-120; D17 owner phụ
trách vận hành mặc định, chỉ hỏi lại khi owner yêu cầu đổi. D12/D13 hoãn
đến khi chốt nơi chạy web/phần mềm; E03–E05 chưa duyệt. Toàn bộ Gói 2 FE01–FE10
đã duyệt: FE01a/FE01b/FE02 ADR-103–ADR-105 giữ nguyên, FE03–FE10 thêm
ADR-106–ADR-113. Gói 3 FE11–FE19 đã duyệt ADR-121–ADR-129 (mục 11);
chưa code/nghiệm thu E03–E05.

## Cách đọc

- NV01–NV39 giữ thứ tự 39 chức năng tại phần III của báo cáo để truy vết.
  NV10/NV11 đã loại bỏ: còn 37 mục chức năng từ danh mục gốc. F15 đã loại bỏ:
  còn 32 mã màn hình; không đánh lại số. Một màn hình có thể phục vụ
  nhiều chức năng; một chức năng có thể xuất hiện trong nhiều màn hình.
- Bảng dưới đây là chỉ mục NV/nguồn; [FUNCTION_SPEC](FUNCTION_SPEC.md) ghi
  actor/module, inputs/outputs, quyền, guard/validation/effects cho37 mục,
  thao tác chung và bảng trace từng32 F. Dữ liệu nghiệp vụ chưa là DTO/API contract.
- Các mục ghi **Chưa chốt** là câu hỏi, không phải yêu cầu đã được chấp nhận.
- Hệ thống tự động không là vai trò tài khoản và không có menu riêng.
- Tất cả quyền xem/thao tác phải được kiểm tra tại backend, gồm ownership.

## 1. Học viên — NV01–NV08

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV01 | Đăng ký và đăng nhập | Họ tên/ngày sinh/email/SĐT, địa chỉ tùy chọn, mật khẩu; đăng nhập email/mật khẩu → tài khoản Student và phiên đúng vai trò | ADR-019/073–077: tên Unicode trim 1–100; ngày sinh hợp lệ/không tương lai/không tuổi tối thiểu; email <=254 so trùng không phân biệt hoa-thường/không gộp nhà cung cấp; SĐT di động VN 10 số 0/+84 chuẩn hóa-so trùng; địa chỉ tùy chọn <=255. Không xác minh email/OTP/đăng nhập SĐT/chọn vai trò nhân sự. ADR-048/051/052: email bất biến/reset tạm/bắt đổi/chặn 5 lần sai; ADR-053–056: mật khẩu 15–128 Unicode-khoảng trắng/chặn phổ biến; tạm 24 giờ, phiên 2 giờ hoặc idle 30 phút/không refresh/nhiều thiết bị. ADR-091/092: Unicode code point/NFC trừ mật khẩu, email ASCII/SĐT canonical/NULL; AuthSession+security_version và timer throttle độc lập. Idle/shared-tab/phiên hạn chế/cookie-CSRF đã chốt ADR-097–ADR-099; D05 hash/blocklist đã duyệt ADR-114: PasswordHasher/runtime, SecLists offline/pin/license/rà quý, không dịch vụ ngoài. | F01–F02 |
| NV02 | Đăng ký khóa học | Học viên + khóa chọn → Registration của học viên |Chỉ khóa ACTIVE; một hồ sơ chưa kết thúc/học viên/khóa (ADR-015). ADR-022: đăng ký lại sau CANCELLED/REJECTED hoặc lớp COMPLETED và bảng điểm học viên FINALIZED, không cần đạt môn; giữ lịch sử. Học viên hủy hồ sơ của mình trước CONFIRMED theo ADR-031. Không sửa khóa trên hồ sơ hiện có; hủy hợp lệ/đăng ký mới/thi lại từ đầu (ADR-038). Biểu diễn đã chốt ADR-090; E03 dictionary còn chờ. ADR-090: is_unfinished/filtered unique và predicate gốc cùng transaction; không Registration COMPLETED. | F04–F05 |
| NV03 | Đăng ký/xem/hủy/đổi lịch thi đầu vào/thi lại | Hồ sơ + ca chọn → đặt ca cho lượt thi | ADR-021/027/030: hủy/đổi chỉ từ ca OPEN trước bắt đầu và trước khi CONFIRMED; CLOSED chặn mọi thao tác đặt chỗ của học viên. Vắng/ca bị trung tâm hủy không tăng lượt, tự đặt lại cùng lượt chưa hoàn thành; giữ lịch sử. Ca mới OPEN còn chỗ; tối đa hai lượt/hồ sơ, lượt hai sau điểm lượt một. Biểu diễn đã chốt ADR-090. ADR-086/088/090: trạng thái đóng hiệu lực không phụ thuộc cron; EntryTestBooking riêng giữ lịch sử; chặn trùng lịch học viên. | F06–F07 |
| NV04 | Xem kết quả thi và Band | Hồ sơ của mình → điểm từng lượt, lượt tốt nhất và Band | Chỉ lượt COMPLETED góp vào Band; thang 100. ADR-028 khóa bộ ngưỡng sau kết quả đầu tiên, không làm tròn điểm thành số nguyên. ADR-029: đồng điểm cao nhất chọn lượt một, giữ/hiển thị cả hai; sửa điểm có audit chỉ trước xác nhận theo quyền đã chốt. | F08 |
| NV05 | Xác nhận nhập học | Hồ sơ BAND_ASSIGNED → CONFIRMED và thời điểm xác nhận | Học viên tự xác nhận, có Band hợp lệ. ADR-021/030: chặn khi lượt hai đang đặt lịch/đã thi chưa chấm, kể cả ca CLOSED; chờ kết quả hoặc hủy đặt chỗ hợp lệ từ ca OPEN trước bắt đầu để xác nhận bằng kết quả hiện có. | F09 |
| NV06 | Xem/in hóa đơn và hướng dẫn thanh toán | Hồ sơ/hóa đơn của mình → số tiền, hạn, trạng thái, hướng dẫn và cảnh báo quá hạn | Trả đủ một lần ngoài ứng dụng; học viên không tự xác nhận PAID. ADR-041: in qua trình duyệt, đúng trạng thái; không PDF/biên lai riêng. ADR-043–045/047: tiền dương/VND nguyên đồng, hạn ngày phát hành +7 ngày lịch theo giờ Việt Nam, chỉ cảnh báo quá hạn. | F10 |
| NV07 | Xem thời khóa biểu | Lớp được xếp → lịch học/thi của lớp |Chỉ dữ liệu lớp của mình; lịch mới nhất khi giáo vụ cập nhật. Lịch sử đã chốt ADR-084. ADR-084: Hiện tại/Lịch sử, chọn lớp của mình nhiều khóa/lượt; hoàn thành chỉ xem, buổi không tổ chức/thay thế ghi rõ một nghĩa vụ. | F11 |
| NV08 | Xem kết quả học tập | Lớp/hồ sơ của mình → bảng điểm FINALIZED | ADR-024: chỉ công bố sau chốt, lớp COMPLETED; điểm tổng kết >= 50 đạt, dưới 50 không đạt, không thêm mức xếp loại. | F12 |

## 2. Tuyển sinh — NV09–NV12

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV09 | Tra cứu và kiểm tra hồ sơ | Tên/SĐT/email, tiêu chí hồ sơ → danh sách/chi tiết và trạng thái tuyển sinh |Kiểm tra trùng theo ADR-015; biểu diễn vòng đời ADR-090. Tuyển sinh không sửa thông tin cá nhân thay học viên (ADR-016). ADR-083: mọi trạng thái; tên/email/SĐT/khóa/đầu vào-Band/xác nhận-duyệt/Invoice tiền-trạng thái/lớp; không Payment/điểm đào tạo/điểm danh/DOB/địa chỉ/quyền sửa hoặc xuất. | F13–F14 |
| NV10 | Đã loại bỏ: hỗ trợ ghi danh trực tiếp | Không triển khai nghiệp vụ tuyển sinh tạo hồ sơ thay | ADR-012: tư vấn tại quầy vẫn là học viên tự đăng ký qua NV01/NV02; tuyển sinh không có quyền tạo tài khoản/hồ sơ thay. | F15 đã loại bỏ |
| NV11 | Đã loại bỏ: hỗ trợ đăng ký thi | Không triển khai nghiệp vụ tuyển sinh đặt ca thay | ADR-012: đăng ký thi theo luồng học viên NV03; không có quyền/menu/API tuyển sinh đặt ca thay. | Không có màn hình triển khai |
| NV12 | Duyệt, từ chối hoặc hủy hồ sơ nhập học | Hồ sơ CONFIRMED → ADMITTED và một Invoice, REJECTED kèm lý do, hoặc CANCELLED theo ADR-022 |Duyệt/tạo hóa đơn cùng transaction. ADR-017: từ chối chỉ CONFIRMED, lý do học viên xem được. ADR-022: tuyển sinh hủy CONFIRMED hoặc ADMITTED có Invoice UNPAID, bắt buộc lý do; hủy hóa đơn UNPAID cùng hồ sơ, không hủy đã trả tiền/hoàn tiền. ADR-031: hủy đặt chỗ tương lai kể cả CLOSED cùng transaction; giữ lịch sử đã bắt đầu/điểm/Band và khóa xử lý kết quả trên hồ sơ đã hủy. Nhóm dữ liệu/audit đã chốt ADR-090/093. ADR-090/093: is_unfinished/cancellation attribution/operation_id nối phụ thuộc, transaction+rowversion/locks/request_id. | F14 |

## 3. Giáo vụ — NV13–NV20

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV13 | Quản lý khóa và chương trình theo Band | Thông tin khóa, học phí, Band, chương trình, thành phần/trọng số, số LESSON dự kiến → danh mục có trạng thái |Học phí dương/VND nguyên đồng, không khóa miễn phí (ADR-043/044). Trọng số chương trình/tổng 100 trước ACTIVE; không phê duyệt BGĐ/xóa lịch sử. ADR-028 khóa khóa/Band/thành phần/trọng số sau lớp đầu tiên, ngưỡng Band sau kết quả đầu tiên. ADR-032: số LESSON nguyên dương/chương trình, không FINAL_EXAM, khóa sau lớp đầu tiên. ADR-085: vòng đời Course/Program, giá mới không đổi Invoice cũ, ACTIVE cấu hình chưa dùng muốn sửa phải về DRAFT; INACTIVE không tự dừng lớp/hồ sơ. ADR-087/090: một FINAL_EXAM component, kind/freeze/LESSON count. | F18 |
| NV14 | Tổ chức, mở/đóng, hủy và điều chỉnh ca thi đầu vào | Thời gian, phòng, sức chứa, giáo viên chấm/lý do → ca thi và lịch sử |Sức chứa 1–20; không trùng tài nguyên. ADR-027: hủy trước bắt đầu có lý do/giữ lịch sử; sửa ngày/giờ/phòng chỉ trước bắt đầu/chưa từng đặt, sau đặt phải hủy/tạo mới. ADR-030: tạo CLOSED, giáo vụ mở/mở lại trước bắt đầu với tài nguyên/Teacher ACTIVE/sức chứa hợp lệ, đầy vẫn OPEN; đóng giữ đặt chỗ/chặn học viên, đến giờ bắt đầu CLOSED/không mở lại. Đổi giáo viên chỉ trước bắt đầu/mọi điểm-ABSENT, có lý do/audit và kiểm tra lịch, giữ đặt chỗ. Biểu diễn đã chốt ADR-086/090. ADR-086/088/090: hiệu lực CLOSED từ start, chỉ ghi khi write hợp lệ; lịch cùng ngày/phút, booking-history riêng, actor/time/audit. | F16–F17 |
| NV15 | Theo dõi kết quả thi và Band | Ca/hồ sơ → tiến độ chấm, điểm và Band |Quyền theo dõi không đồng nghĩa quyền giáo vụ nhập/sửa điểm thay giáo viên. ADR-029 không cho giáo vụ quyền sửa ngoại lệ. ADR-083: tên/email/SĐT/khóa/đầu vào-Band phục vụ ca/xếp/lớp; Invoice chỉ trạng thái, không tiền/Payment; không quyền sửa điểm. | F16–F17; chi tiết theo ADR-083 |
| NV16 | Quản lý lớp học | Chương trình, mã lớp, giáo viên, sĩ số → lớp và trạng thái |Sức chứa 1–15; một giáo viên/lớp. ADR-025: giáo vụ mở/bắt đầu/kết thúc theo điều kiện đã chốt, không tự chuyển theo ngày/giờ, không yêu cầu điểm đã chốt để kết thúc. ADR-026: hủy chỉ PLANNED/OPEN trống/chưa dạy, phải có lý do, giữ lịch sử, lịch hủy không giữ chỗ. Buổi thực tế ADR-087. ADR-085/087: program bất biến, max_size trước ONGOING >=số đã xếp; Giáo vụ xác nhận HELD từng buổi sau end/ONGOING; một FINAL_EXAM riêng. | F19–F20 |
| NV17 | Phân công giáo viên | Lớp + giáo viên → người phụ trách | ADR-023: giáo vụ chỉ đổi trước ONGOING/bắt đầu dạy và chưa có điểm danh/điểm; kiểm tra toàn bộ lịch lớp với lịch lớp/ca thi của giáo viên mới. Không có luồng lịch sử đổi giáo viên. ADR-089: bị khóa/ngừng tài khoản không tự đổi người; ngoài cửa đổi thì chờ mở/khôi phục hợp lệ. | F20 |
| NV18 | Xếp học viên vào lớp | Hồ sơ đủ điều kiện + lớp → StudentClassAssignment | ADR-023: lớp OPEN, ADMITTED, Invoice PAID, đúng khóa/Band, còn chỗ. Giữ ADMITTED, hiển thị đã xếp lớp từ liên kết; không nhận muộn khi ONGOING. Không có lớp thì chờ; không chuyển lớp sau khi xếp, kể cả trước dạy (ADR-040). ADR-088/090/093: chặn lịch học viên trùng ca/lớp; capacity/assignment unique/serialized guards, không tự chuyển lớp. | F22 |
| NV19 | Quản lý lịch học và lịch thi | Lớp, quy tắc tuần hoặc buổi riêng; buổi không tổ chức + lý do/lịch thay thế → Schedule/lịch sử liên kết | ADR-014/032: sinh lịch tuần, dời buổi trước bắt đầu/chưa dữ liệu phụ thuộc, lớp chưa COMPLETED/CANCELLED; đủ LESSON trước dạy, FINAL_EXAM riêng. ADR-035/036: sau giờ dự kiến nếu LESSON không tổ chức, giáo vụ giữ lịch cũ/lý do/người/thời gian và tạo buổi thay thế cùng lớp, giáo viên hiện tại/kiểm tra trùng lịch; chặn nếu dữ liệu phụ thuộc/lớp đã kết thúc, ghi nhất quán. Buổi thay thế phải diễn ra/đủ điểm danh trước hoàn thành. ADR-037: không thêm nghĩa vụ LESSON vượt số buổi chương trình; cặp cũ/thay thế vẫn một, không giới hạn tổng bản ghi lịch sử. Không module ngày nghỉ/automation/học bù cá nhân. Liên kết/lịch sử/audit đã chốt ADR-087/093; dictionary E03 còn chờ. ADR-087/088: HELD/NOT_HELD+nghĩa vụ/chuỗi replacement cả FINAL_EXAM, same-day/phút/[start,end); tuần preview/toàn bộ hoặc không phần nào/không entity lặp; kiểm lịch học viên hiện có khi lịch đổi. | F21 |
| NV20 | Kiểm tra và chốt bảng điểm | Bảng điểm SUBMITTED toàn lớp + kết quả rà soát/lý do → RETURNED hoặc FINALIZED toàn lớp | ADR-024/034: chỉ lớp COMPLETED; trả lại có lý do hoặc chốt/công bố cả lớp trong một thao tác, không riêng học viên; toàn bộ thành công hoặc không chuyển phần nào. FINALIZED bất biến. ADR-083/089/093: Giáo vụ không xem Teacher DRAFT, chỉ SUBMITTED/RETURNED/FINALIZED; preview/409 form cũ và operation_id audit mọi lần toàn lớp. | F23 |

## 4. Giáo viên — NV21–NV26

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV21 | Xem ca thi được phân công | Tài khoản giáo viên → ca thi/danh sách thi được giao |Chỉ ca có grader_id là giáo viên hiện tại. ADR-083: ID nội bộ/tên/email/SĐT trong ca đang được giao, không DOB/địa chỉ/finance/toàn danh sách học viên. | F25 |
| NV22 | Nhập/sửa điểm thi đầu vào và ghi nhận vắng | Lượt thi trong ca được giao + điểm/lý do sửa/xác nhận vắng → COMPLETED và Band/audit, hoặc ABSENT |0–100; không chấm ca khác hoặc CANCELLED; CLOSED vẫn cho xử lý kết quả (ADR-030), trừ hồ sơ CANCELLED khóa cả nhập/sửa điểm và ghi vắng (ADR-031). ADR-027: ghi vắng sau kết thúc, không suy từ thiếu điểm/không ghi 0. ADR-029: sửa COMPLETED bởi người nhập vẫn phụ trách ca, chỉ TESTING/BAND_ASSIGNED, có lý do/audit, tính lại Band; khóa sau xác nhận/hủy/từ chối, không ngoại lệ. ADR-086: điểm lần đầu từ ca kết thúc, không hạn 24/48h; ADR-090: kết quả trỏ booking COMPLETED, ABSENT không tăng lượt. | F25 |
| NV23 | Xem lịch dạy và danh sách lớp | Tài khoản giáo viên → lớp, học viên và lịch được phân công |Không xem lớp không phụ trách; đổi giáo viên chỉ trước giảng dạy và chưa có điểm danh/điểm theo ADR-023. ADR-083: ID nội bộ/tên/email/SĐT và dữ liệu nhiệm vụ trong lớp đang phụ trách, không DOB/địa chỉ/finance/toàn trung tâm. | F24 |
| NV24 | Điểm danh | Buổi LESSON + trạng thái có/vắng của học viên → Attendance | Một bản ghi/học viên/buổi; đúng lớp phụ trách. ADR-033: nhập/sửa từ lúc LESSON bắt đầu đến trước Class COMPLETED, không hạn 24/48 giờ mỗi buổi; sau đó chỉ xem dù bảng điểm chưa gửi/chốt. Không nhập trước buổi bắt đầu/không cần chờ kết thúc. ADR-025: đủ điểm danh trước hoàn thành lớp; sửa lịch theo ADR-032. | F26 |
| NV25 | Nhập và gửi bảng điểm | Điểm thành phần theo chương trình → tổng kết, bảng điểm DRAFT/SUBMITTED | ADR-024/034: nhập nháp từng giá trị khi đang dạy; gửi toàn lớp khi COMPLETED, mọi học viên đủ thành phần và DRAFT/RETURNED; không gửi riêng học viên. SUBMITTED đóng sửa; giáo viên không tự chốt. Thang 100, trọng số chương trình. ADR-087/091/093: kind và schedule_id cho điểm phụ thuộc/from start/not NOT_HELD, decimal(5,2)/operation audit/history. | F27 |
| NV26 | Sửa và gửi lại bảng điểm | Bảng điểm RETURNED toàn lớp + điểm/chú thích sửa → SUBMITTED toàn lớp | ADR-024/034: sửa giá trị cần sửa sau giáo vụ trả lại cả lớp kèm lý do, giữ dữ liệu và gửi lại toàn lớp trong một thao tác; không sửa SUBMITTED/FINALIZED. Không giới hạn một lần gửi trong vòng đời. ADR-093: giữ mỗi lần gửi/trả/chốt qua operation_id, không chỉ timestamp cuối, chống form cũ/gửi lặp. | F27 |

## 5. Kế toán — NV27–NV29

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV27 | Tra cứu hóa đơn | Tiêu chí hóa đơn → danh sách/chi tiết, UNPAID/PAID và quá hạn |Quá hạn là chỉ báo, không là trạng thái; không tự phạt học viên. ADR-018: công nợ từ Invoice UNPAID, quá hạn là phần vượt hạn. Chi tiết tra cứu đã chốt ADR-083; bộ lọc báo cáo ADR-070 không tự áp sang NV27. ADR-083: mọi trạng thái gồm CANCELLED; lọc số hóa đơn/khóa/tên-email-SĐT/status/ngày phát hành tùy chọn/quá hạn; Invoice/Payment/confirmation/liên hệ, không điểm/điểm danh/quyền in-xuất thêm. | F28 |
| NV28 | Đối chiếu/xác nhận và sửa thông tin thanh toán | Invoice + số tiền, tiền mặt/chuyển khoản, mã giao dịch → một Payment và Invoice PAID |Tiền dương/VND nguyên đồng/đủ bằng amount; chuyển khoản cần mã giao dịch (ADR-043/044). Transaction/chống lặp/race với hủy (ADR-022/031). ADR-042: chỉ kế toán sửa phương thức/mã giao dịch có lý do/audit; khóa tiền/hóa đơn/ngày xác nhận, không hủy/xóa/hoàn tiền. ADR-046: paid_at chỉ thời điểm xác nhận, không ngày thu riêng hoặc nhập lùi. ADR-089/090/091/093: preview, một Payment/Invoice, tiền chuỗi API/decimal(18,0), request_id/audit chống lặp và tranh chấp. | F29 |
| NV29 | Xuất báo cáo tài chính | Kỳ/khóa/chỉ quá hạn → doanh thu, tổng phải thu, công nợ/quá hạn và file CSV | ADR-013/018/063/064/070/072: mặc định tháng hiện tại, doanh thu ngày xác nhận/tổng phải thu kỳ phát hành UNPAID+PAID loại CANCELLED; nợ hiện tại không kỳ ngày, lọc khóa nhất quán/chỉ quá hạn cho nợ. CSV toàn kết quả lọc/cột báo cáo, không chỉ trang/không trường ẩn/hồ sơ cá nhân hoặc xuất riêng hồ sơ-hóa đơn. Cơ chế file CSV D10 đã duyệt ADR-101. Không chi phí/lợi nhuận/as-of. ADR-094: snapshot mỗi query/export mới+queried_at, không frozen trang trước; D10 CSV layout/limits/formula safeguards đã duyệt ADR-101. | F30 |

## 6. Giám đốc — NV30–NV32

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV30 | Xem báo cáo tuyển sinh | Khóa/ngày đăng ký/trạng thái → số hồ sơ, số học viên duy nhất | ADR-018/063/065/068/071: mặc định tháng Việt Nam hiện tại/mọi trạng thái, giữ khóa ngừng cho lịch sử; học viên duy nhất mỗi khóa/tổng toàn tập không cộng nhóm, số hồ sơ riêng. Chỉ nhóm phù hợp, không hồ sơ/định danh từng học viên hoặc xuất. | F31 |
| NV31 | Xem báo cáo đào tạo | Khóa/Band/lớp/giáo viên hiện tại/trạng thái; kỳ chốt kết quả → hiện trạng/tiến độ LESSON, FINAL_EXAM riêng, tổng hợp FINALIZED/tỷ lệ đạt | ADR-018/024/063/066/067/069/071: hiện trạng mọi trạng thái/không kỳ ngày; tiến độ LESSON thực tế/thay thế một nghĩa vụ. Kết quả kỳ chốt mặc định tháng hiện tại/mỗi lượt học riêng, không dữ liệu khác 0%, đạt >=50. Chỉ nhóm phù hợp, không hồ sơ/định danh học viên hoặc xuất; hiển thị/query ADR-094; không quyền CSV cho Director. ADR-087/094: actual occurrence theo nghĩa vụ, tỷ lệ hai số lẻ half-up và queried_at; không thay cohort quyền đã chốt. | F31 |
| NV32 | Xem báo cáo tài chính | Kỳ/khóa/chỉ quá hạn → doanh thu ngày xác nhận, tổng phải thu kỳ phát hành, công nợ hiện tại/quá hạn | ADR-018/063/064/070/071: cùng cơ sở/lọc với Kế toán, mặc định kỳ tháng hiện tại; nợ hiện tại không kỳ ngày. Chỉ nhóm phù hợp, không hồ sơ/hóa đơn/định danh từng học viên, xuất hoặc phê duyệt/as-of. Không trừ khác tập để tính nợ. ADR-091/094: tổng tiền decimal(38,0)/API chuỗi, snapshot query+queried_at, không as-of. | F31 |

## 7. Admin — NV33–NV35

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV33 | Cấp tài khoản nhân sự và reset mật khẩu | Thông tin tài khoản + một vai trò nhân sự có sẵn/thao tác reset → Account/mật khẩu tạm |Không RBAC động/nhiều vai trò. ADR-048: mật khẩu tạm ngẫu nhiên, Admin xem một lần/giao trực tiếp; bắt buộc đổi trước nghiệp vụ, không log mật khẩu/reset tự mở khóa. ADR-049: reset hủy mọi phiên cũ; ADR-051: không sửa email hiện có. ADR-053/054: chính sách mật khẩu 15–128 ký tự; tạm hết hạn 24 giờ/cần Admin reset lại, không đổi trạng thái. D04/D06 đã duyệt ADR-098/099; D05 hash/generator/blocklist đã duyệt ADR-114, mật khẩu tạm ngẫu nhiên mật mã 32 ký tự/không plaintext. ADR-091/092: hash-only/temp fields và phiên/version; D04/D06 đã duyệt; D05 đã duyệt ADR-114; tương tác bàn giao FE18 đã duyệt ADR-128 (requirements mục 4.8). | F32 |
| NV34 | Khóa/mở/ngừng sử dụng/khôi phục tài khoản | Tài khoản + thao tác → trạng thái truy cập | ADR-020: bảo vệ Admin chính mình/cuối cùng; không xóa dữ liệu. ADR-049/050: khóa/ngừng dùng hủy mọi phiên, mở/khôi phục không phục hồi phiên cũ; Admin khôi phục DISABLED → ACTIVE cùng tài khoản/giữ vai trò-dữ liệu-lịch sử. ADR-052: chặn đăng nhập tạm không đổi status. Cơ chế phiên/thu hồi đã chốt ADR-092/097–100; audit tài khoản bắt buộc cùng transaction/lỗi rollback cả thu hồi liên quan (ADR-115). ADR-089/092/093: không tự chuyển phụ trách/hủy nghiệp vụ; phiên/version revoke/timer throttle riêng/serialized bảo vệ Admin cuối. | F32 |
| NV35 | Tra cứu nhật ký | Khoảng ngày, người, hành động, loại/mã đối tượng, kết quả → AuditLog phân trang mới nhất trước |Chỉ Admin, không sửa/xóa hoặc xuất CSV/PDF nhật ký. ADR-058–062: phạm vi nghiệp vụ/bảo mật và chi tiết cần thiết, không bí mật/toàn payload. Biểu diễn lịch sử đã chốt ADR-093; lỗi ghi/chống ngập/lưu giữ đã duyệt ADR-115/116. ADR-093: operation_id/old-new history, actor NULL khi chưa xác thực; ADR-115/116: audit bắt buộc atomic/rollback và giữ lịch sử; pure security events best-effort, pure logs/IP 90 ngày dọn vận hành, không xóa audit bắt buộc. | F33 |

## 8. Xử lý tự động — NV36–NV39

| Mã | Chức năng | Sự kiện → Kết quả | Quy tắc |
|---|---|---|---|
| NV36 | Xác định Band | Điểm đầu vào hợp lệ → BandAssignment | Điểm cao nhất trong các lượt COMPLETED, ngưỡng lớn nhất không vượt điểm; không chấm bài tự động. ADR-028 khóa ngưỡng sau kết quả đầu tiên, không làm tròn thành số nguyên. ADR-029: tính lại cùng transaction khi sửa hợp lệ, đồng điểm chọn lượt một; khóa điểm/Band sau xác nhận hoặc hủy/từ chối. |
| NV37 | Tạo hóa đơn | Tuyển sinh duyệt hồ sơ → Invoice |Một hóa đơn/hồ sơ; snapshot học phí dương/VND nguyên đồng; cùng transaction với ADMITTED. Hạn ngày phát hành +7 ngày lịch theo UTC+7 (ADR-043–045/047), không tự gia hạn/sửa hạn. ADR-085/090/091: snapshot giá tại phát hành, số HD-YYYY-NNNNNNNN, safe DB counter/không tái dùng, range/API tiền đã duyệt. |
| NV38 | Tính điểm tổng kết | Điểm thành phần + cấu hình chương trình → final_score |Tổng score_value × weight / 100, thang 100. ADR-028: dữ liệu nhập tối đa hai số lẻ; tính bằng decimal, không làm tròn từng đóng góp, tổng làm tròn một lần half-up hai số lẻ để lưu/hiển thị/so ngưỡng đạt. Cấu hình khóa sau lớp đầu tiên; không tự công bố. ADR-087/091: component kind/schedule dependency và decimal chuẩn; không thêm điểm bắt buộc ngoài cấu hình. |
| NV39 | Ghi nhật ký | Thao tác nhạy cảm/sự kiện bảo mật → AuditLog |Ghi tầng ứng dụng, không trigger. ADR-058/059: giữ audit bắt buộc, thêm thay đổi cấu hình/đời lớp/lịch và đăng nhập thành công-thất bại/chặn tạm/đăng xuất/mật khẩu-tài khoản/từ chối quyền; không mỗi lượt xem hoặc gán actor từ email chưa xác thực. ADR-061: chi tiết cần thiết/cũ-mới bắt buộc, không mật khẩu/hash/JWT/bí mật phiên/toàn payload. Biểu diễn lịch sử đã chốt ADR-093; IP/lỗi ghi/chống ngập/lưu giữ đã duyệt ADR-115/116. ADR-093: operation_id nối mẹ-phụ thuộc/cả lớp, lịch sử mọi lần; không secret/toàn payload. ADR-115/116: mandatory audit rollback; pure security events best-effort/fallback giới hạn không bí mật, không bỏ quyền/phục hồi logout. Pure logs/IP 90 ngày, giữ audit bắt buộc; gộp rate-limit giữ bộ đếm/actor NULL. |

## 9. Chức năng chung và giới hạn phạm vi

- Đăng nhập dành cho mọi vai trò; NV01 còn gồm tự đăng ký của học viên.
- Đăng xuất trong khung chung, hủy phiên hiện tại tại server, không hủy phiên
  khác hoặc thêm nút đăng xuất tất cả (ADR-057). Phiên thường tối đa 2 giờ hoặc
  idle 30 phút, không refresh/ghi nhớ; nhiều thiết bị đồng thời (ADR-055/056).
  Admin đặt lại mật khẩu theo requirements.
- Người đã đăng nhập tự đổi mật khẩu của mình, phải nhập đúng mật khẩu hiện
  tại (ADR-020), hủy mọi phiên và phải đăng nhập lại (ADR-049). Không phải
  luồng quên mật khẩu, không khôi phục qua email;
  menu tài khoản chung ADR-084, không thêm mã F.
- Học viên được sửa họ tên/ngày sinh/SĐT/địa chỉ của mình, không tự đổi email
  đăng nhập; số mới phải hợp lệ và không trùng (ADR-016/019). Nơi đặt thao tác
  ở menu tài khoản chung ADR-084; không tạo lại NV10/NV11.
- Trang chủ, danh sách khóa và trạng thái hồ sơ là các cách xem dữ liệu đã có,
  không tự tạo thêm nghiệp vụ chỉ vì có màn hình riêng.
- Không học bổng, thi trực tuyến, cổng thanh toán, trả góp, RBAC động hoặc
  phê duyệt lớp/chương trình bởi Giám đốc. Không mở rộng quản lý chi phí,
  lợi nhuận, phụ huynh hoặc thông báo tự động từ các biểu mẫu minh họa.
- Không mở quyền nhân sự sửa hồ sơ, hoàn tiền hoặc sửa điểm đã chốt. Không
  chuyển lớp sau xếp (ADR-040); sửa Payment chỉ kế toán/phương thức/mã giao dịch
  có lý do/audit, không sửa tiền/hóa đơn/ngày xác nhận (ADR-042).

## 10. Lựa chọn đã duyệt và phần cần chốt tiếp

33 mục A–C và tám mục D Gói 1 đã được duyệt; bảng OWNER_REVIEW còn
D12/D13 hoãn và E03–E05; E01-v1/E02-v1 đã duyệt, toàn bộ Gói 2 FE01–FE10 đã duyệt ADR-103–ADR-113.
Các ADR dưới đây liên kết lựa chọn gốc, không tạo chức năng/mã NV/F mới:

| ADR | Mục đã duyệt | Tác động |
|---|---|---|
| ADR-083 | A01, A02, A03, A06 | Define operational detail visibility |
| ADR-084 | A04, A05 | Place account actions and Student schedule history in existing UI |
| ADR-085 | A07, A08, A09 | Define catalogue lifecycle and immutable class-program links |
| ADR-086 | A10, B04 | Gate initial entry grading at test end and enforce effective closure |
| ADR-087 | B01, B02, C02, C03 | Confirm actual occurrence and model linked lessons and final exams |
| ADR-088 | B03, B05 | Use minute-precision same-day intervals and atomic timetable generation |
| ADR-089 | B06, B07 | Preserve blocked assignments and require sensitive-action conflict previews |
| ADR-090 | C01, C04, C06, C10, C15, C16 | Separate test bookings and enforce relational lifecycle integrity |
| ADR-091 | C07, C08, C09 | Specify exact numeric, time and normalized text representations |
| ADR-092 | C11, C12 | Persist revocable authentication sessions and independent login timers |
| ADR-093 | C05, C13 | Group audit history and serialize critical writes with replay protection |
| ADR-094 | C14 | Read consistent report snapshots and display two-decimal rates |

Đặc tả chuẩn: `../requirements.md` mục 4.1. Mô hình/contract dữ liệu:
`../ARCHITECTURE.md`. Đầu ra [E01-v1](FUNCTION_SPEC.md) đã hoàn thiện ở mức
chức năng và được owner duyệt E01 ngày30/09/2026; E02 được duyệt riêng,
không suy E03–E05 đã duyệt. D01–D04/D06–D07/D10/D15 đã duyệt
ADR-095–ADR-102; D05/D08/D09/D11/D14/D16/D17 duyệt ADR-114–ADR-120,
D12/D13 hoãn theo owner. Toàn bộ Gói 2
FE01–FE10 đã duyệt; responsive lists/quyền/form/states/confirmation/component/
format/style ở requirements mục 4.6 và ADR-106–ADR-113. Không tạo NV/F/quyền
mới. Đơn vị lưu FE14/bàn giao FE18 được duyệt riêng ADR-124/128;
toàn bộ Gói 3 đã duyệt ADR-121–ADR-129 (mục 11). Nền tảng hosting vẫn chỉ
tư vấn, không triển khai. Không hỏi lại quyết định
đã chốt hoặc bắt đầu code/Slice 0/Git/scaffold.

## 11. Mẫu tương tác nghiệp vụ Gói 3 đã duyệt — ADR-121–ADR-129

Nguồn: owner xác nhận trực tiếp ngày 29/09/2026, không nêu ngoại lệ; chín ô
OWNER_REVIEW được cập nhật theo lời duyệt. Contract đầy đủ tại requirements
mục 4.8 và ADRs, không suy từ Word/mockup hoặc Gói 2. Không thêm mã NV/F.

| Mục / ADR | Trace chức năng/màn hình | Hợp đồng đã duyệt |
| --- | --- | --- |
| FE11 / ADR-121 | F03 | Thẻ riêng theo hồ sơ/việc tiếp theo đúng quyền; không điểm chưa FINALIZED hoặc nghiệp vụ thông báo mới. |
| FE12 / ADR-122 | F07/F11/F21/F24 | Lịch tuần/agenda đúng scope; lịch sử/liên kết thay thế, không kéo-thả hoặc tự HELD. |
| FE13 / ADR-123 | F21 / NV19 | Nhập→preview mất hiệu lực khi sửa→commit toàn bộ hợp lệ; server kiểm tra lại, không giữ chỗ hoặc bỏ ngày lỗi. |
| FE14 / ADR-124 | F26/F27 / NV24–NV26 | Lưu điểm danh một LESSON và nháp điểm/nhận xét một học viên all-or-none; chưa ghi/thiếu không thành present/0, blank không tự xóa. |
| FE15 / ADR-125 | F27/F23 / NV20/NV25–NV26 | Lưu nháp khác gửi/gửi lại/trả/chốt cả lớp; sửa chưa lưu chặn gửi, return có lý do và finalization bất biến. |
| FE16 / ADR-126 | F30/F31 / NV29–NV32 | Thẻ/bảng, Áp dụng lọc/queried_at; giữ metric/cohort/quyền và fresh CSV toàn tập có giới hạn. |
| FE17 / ADR-127 | F10 / NV06 | In Student-own lấy lại dữ liệu có quyền, A4/trạng thái rõ; UNPAID không là xác nhận thu tiền, không PDF API. |
| FE18 / ADR-128 | F32 / NV33 | Khối mật khẩu tạm một lần/copy chủ động-thủ công/ack, không secret durable/replay hoặc auto-reset khi mất phản hồi. |
| FE19 / ADR-129 | F33 / NV35 | Chi tiết từng sự kiện phân trang/cùng operation_id/allowlist; actor NULL/IP-retention đúng, không edit/delete/undo/export. |

Các lựa chọn trên đã được đưa vào [đặc tả E01-v1](FUNCTION_SPEC.md):37 chức
năng/7 thành phần mỗi mục, thao tác chung, validation matrix và trace32 F.
Đã kiểm tra cấu trúc/độ phủ tài liệu; owner accepted E01-v1 ngày30/09/2026,
chưa test ứng dụng.
Đơn vị lưu/all-or-none và ngoại lệ mất phản hồi được tổng hợp ở
[E02-v1](WORKFLOW_SPEC.md) và được owner nghiệm thu; E03/E04 cần mapping dữ liệu/API/
transaction/replay không chứa secret. D12/D13 hoãn;
E03–E05 chưa nghiệm thu, không code/Git/scaffold/migration/test pass.
