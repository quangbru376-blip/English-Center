# Danh mục chức năng nghiệp vụ

Cập nhật: 28/09/2026. Trạng thái: bản đối chiếu để chủ dự án rà soát;
giai đoạn 1 vẫn đang thực hiện, chưa chốt toàn bộ đặc tả chức năng.

Nguồn: phần I–III và V của báo cáo chính thức, đối chiếu với
`../requirements.md` và ADR-007–ADR-082. Không dùng phần IV (biểu mẫu).
Quy tắc đã chốt vẫn thuộc requirements/ADRs; tài liệu này trình bày chi tiết
chức năng và khoảng trống, không tự tạo quyền hoặc thay thế quyết định đó.

## Cách đọc

- NV01–NV39 giữ thứ tự 39 chức năng tại phần III của báo cáo để truy vết.
  NV10/NV11 đã loại bỏ: còn 37 mục chức năng từ danh mục gốc. F15 đã loại bỏ:
  còn 32 mã màn hình; không đánh lại số. Một màn hình có thể phục vụ
  nhiều chức năng; một chức năng có thể xuất hiện trong nhiều màn hình.
- Dữ liệu vào/ra dưới đây là dữ liệu nghiệp vụ, chưa phải DTO hoặc API contract.
- Các mục ghi **Chưa chốt** là câu hỏi, không phải yêu cầu đã được chấp nhận.
- Hệ thống tự động không là vai trò tài khoản và không có menu riêng.
- Tất cả quyền xem/thao tác phải được kiểm tra tại backend, gồm ownership.

## 1. Học viên — NV01–NV08

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV01 | Đăng ký và đăng nhập | Họ tên/ngày sinh/email/SĐT, địa chỉ tùy chọn, mật khẩu; đăng nhập email/mật khẩu → tài khoản Student và phiên đúng vai trò | ADR-019/073–077: tên Unicode trim 1–100; ngày sinh hợp lệ/không tương lai/không tuổi tối thiểu; email <=254 so trùng không phân biệt hoa-thường/không gộp nhà cung cấp; SĐT di động VN 10 số 0/+84 chuẩn hóa-so trùng; địa chỉ tùy chọn <=255. Không xác minh email/OTP/đăng nhập SĐT/chọn vai trò nhân sự. ADR-048/051/052: email bất biến/reset tạm/bắt đổi/chặn 5 lần sai; ADR-053–056: mật khẩu 15–128 Unicode-khoảng trắng/chặn phổ biến; tạm 24 giờ, phiên 2 giờ hoặc idle 30 phút/không refresh/nhiều thiết bị. Cơ chế kiểm tra/lưu còn kỹ thuật. | F01–F02 |
| NV02 | Đăng ký khóa học | Học viên + khóa chọn → Registration của học viên | Chỉ khóa ACTIVE; một hồ sơ chưa kết thúc/học viên/khóa (ADR-015). ADR-022: đăng ký lại sau CANCELLED/REJECTED hoặc lớp COMPLETED và bảng điểm học viên FINALIZED, không cần đạt môn; giữ lịch sử. Học viên hủy hồ sơ của mình trước CONFIRMED theo ADR-031. Không sửa khóa trên hồ sơ hiện có; hủy hợp lệ/đăng ký mới/thi lại từ đầu (ADR-038). Biểu diễn database: Q-005. | F04–F05 |
| NV03 | Đăng ký/xem/hủy/đổi lịch thi đầu vào/thi lại | Hồ sơ + ca chọn → đặt ca cho lượt thi | ADR-021/027/030: hủy/đổi chỉ từ ca OPEN trước bắt đầu và CONFIRMED; CLOSED chặn mọi thao tác đặt chỗ của học viên. Vắng/ca bị trung tâm hủy không tăng lượt, tự đặt lại cùng lượt chưa hoàn thành; giữ lịch sử. Ca mới OPEN còn chỗ; tối đa hai lượt/hồ sơ, lượt hai sau điểm lượt một. Biểu diễn lịch sử còn Q-004. | F06–F07 |
| NV04 | Xem kết quả thi và Band | Hồ sơ của mình → điểm từng lượt, lượt tốt nhất và Band | Chỉ lượt COMPLETED góp vào Band; thang 100. ADR-028 khóa bộ ngưỡng sau kết quả đầu tiên, không làm tròn điểm thành số nguyên. ADR-029: đồng điểm cao nhất chọn lượt một, giữ/hiển thị cả hai; sửa điểm có audit chỉ trước xác nhận theo quyền đã chốt. | F08 |
| NV05 | Xác nhận nhập học | Hồ sơ BAND_ASSIGNED → CONFIRMED và thời điểm xác nhận | Học viên tự xác nhận, có Band hợp lệ. ADR-021/030: chặn khi lượt hai đang đặt lịch/đã thi chưa chấm, kể cả ca CLOSED; chờ kết quả hoặc hủy đặt chỗ hợp lệ từ ca OPEN trước bắt đầu để xác nhận bằng kết quả hiện có. | F09 |
| NV06 | Xem/in hóa đơn và hướng dẫn thanh toán | Hồ sơ/hóa đơn của mình → số tiền, hạn, trạng thái, hướng dẫn và cảnh báo quá hạn | Trả đủ một lần ngoài ứng dụng; học viên không tự xác nhận PAID. ADR-041: in qua trình duyệt, đúng trạng thái; không PDF/biên lai riêng. ADR-043–045/047: tiền dương/VND nguyên đồng, hạn ngày phát hành +7 ngày lịch theo giờ Việt Nam, chỉ cảnh báo quá hạn. | F10 |
| NV07 | Xem thời khóa biểu | Lớp được xếp → lịch học/thi của lớp | Chỉ dữ liệu lớp của mình; lịch mới nhất khi giáo vụ cập nhật. Cách hiển thị lịch sử lớp cần thống nhất trong workflow. | F11 |
| NV08 | Xem kết quả học tập | Lớp/hồ sơ của mình → bảng điểm FINALIZED | ADR-024: chỉ công bố sau chốt, lớp COMPLETED; điểm tổng kết >= 50 đạt, dưới 50 không đạt, không thêm mức xếp loại. | F12 |

## 2. Tuyển sinh — NV09–NV12

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV09 | Tra cứu và kiểm tra hồ sơ | Tên/SĐT/email, tiêu chí hồ sơ → danh sách/chi tiết và trạng thái tuyển sinh | Kiểm tra trùng theo ADR-015; biểu diễn vòng đời còn Q-005. Tuyển sinh không sửa thông tin cá nhân thay học viên (ADR-016). | F13–F14 |
| NV10 | Đã loại bỏ: hỗ trợ ghi danh trực tiếp | Không triển khai nghiệp vụ tuyển sinh tạo hồ sơ thay | ADR-012: tư vấn tại quầy vẫn là học viên tự đăng ký qua NV01/NV02; tuyển sinh không có quyền tạo tài khoản/hồ sơ thay. | F15 đã loại bỏ |
| NV11 | Đã loại bỏ: hỗ trợ đăng ký thi | Không triển khai nghiệp vụ tuyển sinh đặt ca thay | ADR-012: đăng ký thi theo luồng học viên NV03; không có quyền/menu/API tuyển sinh đặt ca thay. | Không có màn hình triển khai |
| NV12 | Duyệt, từ chối hoặc hủy hồ sơ nhập học | Hồ sơ CONFIRMED → ADMITTED và một Invoice, REJECTED kèm lý do, hoặc CANCELLED theo ADR-022 | Duyệt/tạo hóa đơn cùng transaction. ADR-017: từ chối chỉ CONFIRMED, lý do học viên xem được. ADR-022: tuyển sinh hủy CONFIRMED hoặc ADMITTED có Invoice UNPAID, bắt buộc lý do; hủy hóa đơn UNPAID cùng hồ sơ, không hủy đã trả tiền/hoàn tiền. ADR-031: hủy đặt chỗ tương lai kể cả CLOSED cùng transaction; giữ lịch sử đã bắt đầu/điểm/Band và khóa xử lý kết quả trên hồ sơ đã hủy. Q-007 còn biểu diễn dữ liệu. | F14 |

## 3. Giáo vụ — NV13–NV20

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV13 | Quản lý khóa và chương trình theo Band | Thông tin khóa, học phí, Band, chương trình, thành phần/trọng số, số LESSON dự kiến → danh mục có trạng thái | Học phí dương/VND nguyên đồng, không khóa miễn phí (ADR-043/044). Trọng số chương trình/tổng 100 trước ACTIVE; không phê duyệt BGĐ/xóa lịch sử. ADR-028 khóa khóa/Band/thành phần/trọng số sau lớp đầu tiên, ngưỡng Band sau kết quả đầu tiên. ADR-032: số LESSON nguyên dương/chương trình, không FINAL_EXAM, khóa sau lớp đầu tiên. | F18 |
| NV14 | Tổ chức, mở/đóng, hủy và điều chỉnh ca thi đầu vào | Thời gian, phòng, sức chứa, giáo viên chấm/lý do → ca thi và lịch sử | Sức chứa 1–20; không trùng tài nguyên. ADR-027: hủy trước bắt đầu có lý do/giữ lịch sử; sửa ngày/giờ/phòng chỉ trước bắt đầu/chưa từng đặt, sau đặt phải hủy/tạo mới. ADR-030: tạo CLOSED, giáo vụ mở/mở lại trước bắt đầu với tài nguyên/Teacher ACTIVE/sức chứa hợp lệ, đầy vẫn OPEN; đóng giữ đặt chỗ/chặn học viên, đến giờ bắt đầu CLOSED/không mở lại. Đổi giáo viên chỉ trước bắt đầu/mọi điểm-ABSENT, có lý do/audit và kiểm tra lịch, giữ đặt chỗ. Biểu diễn dữ liệu còn Q-004. | F16–F17 |
| NV15 | Theo dõi kết quả thi và Band | Ca/hồ sơ → tiến độ chấm, điểm và Band | Quyền theo dõi không đồng nghĩa quyền giáo vụ nhập/sửa điểm thay giáo viên. ADR-029 không cho giáo vụ quyền sửa ngoại lệ. | F16–F17; vị trí xem chi tiết cần rà soát |
| NV16 | Quản lý lớp học | Chương trình, mã lớp, giáo viên, sĩ số → lớp và trạng thái | Sức chứa 1–15; một giáo viên/lớp. ADR-025: giáo vụ mở/bắt đầu/kết thúc theo điều kiện đã chốt, không tự chuyển theo ngày/giờ, không yêu cầu điểm đã chốt để kết thúc. ADR-026: hủy chỉ PLANNED/OPEN trống/chưa dạy, phải có lý do, giữ lịch sử, lịch hủy không giữ chỗ. Biểu diễn buổi thực tế còn Q-008. | F19–F20 |
| NV17 | Phân công giáo viên | Lớp + giáo viên → người phụ trách | ADR-023: giáo vụ chỉ đổi trước ONGOING/bắt đầu dạy và chưa có điểm danh/điểm; kiểm tra toàn bộ lịch lớp với lịch lớp/ca thi của giáo viên mới. Không có luồng lịch sử đổi giáo viên. | F20 |
| NV18 | Xếp học viên vào lớp | Hồ sơ đủ điều kiện + lớp → StudentClassAssignment | ADR-023: lớp OPEN, ADMITTED, Invoice PAID, đúng khóa/Band, còn chỗ. Giữ ADMITTED, hiển thị đã xếp lớp từ liên kết; không nhận muộn khi ONGOING. Không có lớp thì chờ; không chuyển lớp sau khi xếp, kể cả trước dạy (ADR-040). | F22 |
| NV19 | Quản lý lịch học và lịch thi | Lớp, quy tắc tuần hoặc buổi riêng; buổi không tổ chức + lý do/lịch thay thế → Schedule/lịch sử liên kết | ADR-014/032: sinh lịch tuần, dời buổi trước bắt đầu/chưa dữ liệu phụ thuộc, lớp chưa COMPLETED/CANCELLED; đủ LESSON trước dạy, FINAL_EXAM riêng. ADR-035/036: sau giờ dự kiến nếu LESSON không tổ chức, giáo vụ giữ lịch cũ/lý do/người/thời gian và tạo buổi thay thế cùng lớp, giáo viên hiện tại/kiểm tra trùng lịch; chặn nếu dữ liệu phụ thuộc/lớp đã kết thúc, ghi nhất quán. Buổi thay thế phải diễn ra/đủ điểm danh trước hoàn thành. ADR-037: không thêm nghĩa vụ LESSON vượt số buổi chương trình; cặp cũ/thay thế vẫn một, không giới hạn tổng bản ghi lịch sử. Không module ngày nghỉ/automation/học bù cá nhân. Dữ liệu liên kết/audit còn bước database. | F21 |
| NV20 | Kiểm tra và chốt bảng điểm | Bảng điểm SUBMITTED toàn lớp + kết quả rà soát/lý do → RETURNED hoặc FINALIZED toàn lớp | ADR-024/034: chỉ lớp COMPLETED; trả lại có lý do hoặc chốt/công bố cả lớp trong một thao tác, không riêng học viên; toàn bộ thành công hoặc không chuyển phần nào. FINALIZED bất biến. | F23 |

## 4. Giáo viên — NV21–NV26

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV21 | Xem ca thi được phân công | Tài khoản giáo viên → ca thi/danh sách thi được giao | Chỉ ca có grader_id là giáo viên hiện tại. | F25 |
| NV22 | Nhập/sửa điểm thi đầu vào và ghi nhận vắng | Lượt thi trong ca được giao + điểm/lý do sửa/xác nhận vắng → COMPLETED và Band/audit, hoặc ABSENT | 0–100; không chấm ca khác hoặc CANCELLED; CLOSED vẫn cho xử lý kết quả (ADR-030), trừ hồ sơ CANCELLED khóa cả nhập/sửa điểm và ghi vắng (ADR-031). ADR-027: ghi vắng sau kết thúc, không suy từ thiếu điểm/không ghi 0. ADR-029: sửa COMPLETED bởi người nhập vẫn phụ trách ca, chỉ TESTING/BAND_ASSIGNED, có lý do/audit, tính lại Band; khóa sau xác nhận/hủy/từ chối, không ngoại lệ. | F25 |
| NV23 | Xem lịch dạy và danh sách lớp | Tài khoản giáo viên → lớp, học viên và lịch được phân công | Không xem lớp không phụ trách; đổi giáo viên chỉ trước giảng dạy và chưa có điểm danh/điểm theo ADR-023. | F24 |
| NV24 | Điểm danh | Buổi LESSON + trạng thái có/vắng của học viên → Attendance | Một bản ghi/học viên/buổi; đúng lớp phụ trách. ADR-033: nhập/sửa từ lúc LESSON bắt đầu đến trước Class COMPLETED, không hạn 24/48 giờ mỗi buổi; sau đó chỉ xem dù bảng điểm chưa gửi/chốt. Không nhập trước buổi bắt đầu/không cần chờ kết thúc. ADR-025: đủ điểm danh trước hoàn thành lớp; sửa lịch theo ADR-032. | F26 |
| NV25 | Nhập và gửi bảng điểm | Điểm thành phần theo chương trình → tổng kết, bảng điểm DRAFT/SUBMITTED | ADR-024/034: nhập nháp từng giá trị khi đang dạy; gửi toàn lớp khi COMPLETED, mọi học viên đủ thành phần và DRAFT/RETURNED; không gửi riêng học viên. SUBMITTED đóng sửa; giáo viên không tự chốt. Thang 100, trọng số chương trình. | F27 |
| NV26 | Sửa và gửi lại bảng điểm | Bảng điểm RETURNED toàn lớp + điểm/chú thích sửa → SUBMITTED toàn lớp | ADR-024/034: sửa giá trị cần sửa sau giáo vụ trả lại cả lớp kèm lý do, giữ dữ liệu và gửi lại toàn lớp trong một thao tác; không sửa SUBMITTED/FINALIZED. Không giới hạn một lần gửi trong vòng đời. | F27 |

## 5. Kế toán — NV27–NV29

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV27 | Tra cứu hóa đơn | Tiêu chí hóa đơn → danh sách/chi tiết, UNPAID/PAID và quá hạn | Quá hạn là chỉ báo, không là trạng thái; không tự phạt học viên. ADR-018: công nợ từ Invoice UNPAID, quá hạn là phần vượt hạn. Chi tiết tra cứu nghiệp vụ còn Q-011; bộ lọc báo cáo ADR-070 không tự áp sang NV27. | F28 |
| NV28 | Đối chiếu/xác nhận và sửa thông tin thanh toán | Invoice + số tiền, tiền mặt/chuyển khoản, mã giao dịch → một Payment và Invoice PAID | Tiền dương/VND nguyên đồng/đủ bằng amount; chuyển khoản cần mã giao dịch (ADR-043/044). Transaction/chống lặp/race với hủy (ADR-022/031). ADR-042: chỉ kế toán sửa phương thức/mã giao dịch có lý do/audit; khóa tiền/hóa đơn/ngày xác nhận, không hủy/xóa/hoàn tiền. ADR-046: paid_at chỉ thời điểm xác nhận, không ngày thu riêng hoặc nhập lùi. | F29 |
| NV29 | Xuất báo cáo tài chính | Kỳ/khóa/chỉ quá hạn → doanh thu, tổng phải thu, công nợ/quá hạn và file CSV | ADR-013/018/063/064/070/072: mặc định tháng hiện tại, doanh thu ngày xác nhận/tổng phải thu kỳ phát hành UNPAID+PAID loại CANCELLED; nợ hiện tại không kỳ ngày, lọc khóa nhất quán/chỉ quá hạn cho nợ. CSV toàn kết quả lọc/cột báo cáo, không chỉ trang/không trường ẩn/hồ sơ cá nhân hoặc xuất riêng hồ sơ-hóa đơn. Cơ chế xuất còn Q-012. Không chi phí/lợi nhuận/as-of. | F30 |

## 6. Giám đốc — NV30–NV32

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV30 | Xem báo cáo tuyển sinh | Khóa/ngày đăng ký/trạng thái → số hồ sơ, số học viên duy nhất | ADR-018/063/065/068/071: mặc định tháng Việt Nam hiện tại/mọi trạng thái, giữ khóa ngừng cho lịch sử; học viên duy nhất mỗi khóa/tổng toàn tập không cộng nhóm, số hồ sơ riêng. Chỉ nhóm phù hợp, không hồ sơ/định danh từng học viên hoặc xuất. | F31 |
| NV31 | Xem báo cáo đào tạo | Khóa/Band/lớp/giáo viên hiện tại/trạng thái; kỳ chốt kết quả → hiện trạng/tiến độ LESSON, FINAL_EXAM riêng, tổng hợp FINALIZED/tỷ lệ đạt | ADR-018/024/063/066/067/069/071: hiện trạng mọi trạng thái/không kỳ ngày; tiến độ LESSON thực tế/thay thế một nghĩa vụ. Kết quả kỳ chốt mặc định tháng hiện tại/mỗi lượt học riêng, không dữ liệu khác 0%, đạt >=50. Chỉ nhóm phù hợp, không hồ sơ/định danh học viên hoặc xuất; hiển thị/cơ chế còn Q-012. | F31 |
| NV32 | Xem báo cáo tài chính | Kỳ/khóa/chỉ quá hạn → doanh thu ngày xác nhận, tổng phải thu kỳ phát hành, công nợ hiện tại/quá hạn | ADR-018/063/064/070/071: cùng cơ sở/lọc với Kế toán, mặc định kỳ tháng hiện tại; nợ hiện tại không kỳ ngày. Chỉ nhóm phù hợp, không hồ sơ/hóa đơn/định danh từng học viên, xuất hoặc phê duyệt/as-of. Không trừ khác tập để tính nợ. | F31 |

## 7. Admin — NV33–NV35

| Mã | Chức năng | Dữ liệu vào → Kết quả | Điều kiện / điểm còn mở | Màn hình liên quan |
|---|---|---|---|---|
| NV33 | Cấp tài khoản nhân sự và reset mật khẩu | Thông tin tài khoản + một vai trò nhân sự có sẵn/thao tác reset → Account/mật khẩu tạm | Không RBAC động/nhiều vai trò. ADR-048: mật khẩu tạm ngẫu nhiên, Admin xem một lần/giao trực tiếp; bắt buộc đổi trước nghiệp vụ, không log mật khẩu/reset tự mở khóa. ADR-049: reset hủy mọi phiên cũ; ADR-051: không sửa email hiện có. ADR-053/054: chính sách mật khẩu 15–128 ký tự; tạm hết hạn 24 giờ/cần Admin reset lại, không đổi trạng thái. Cơ chế còn Q-010/Q-011. | F32 |
| NV34 | Khóa/mở/ngừng sử dụng/khôi phục tài khoản | Tài khoản + thao tác → trạng thái truy cập | ADR-020: bảo vệ Admin chính mình/cuối cùng; không xóa dữ liệu. ADR-049/050: khóa/ngừng dùng hủy mọi phiên, mở/khôi phục không phục hồi phiên cũ; Admin khôi phục DISABLED → ACTIVE cùng tài khoản/giữ vai trò-dữ liệu-lịch sử. ADR-052: chặn đăng nhập tạm không đổi status. Cơ chế thực thi còn kiến trúc. | F32 |
| NV35 | Tra cứu nhật ký | Khoảng ngày, người, hành động, loại/mã đối tượng, kết quả → AuditLog phân trang mới nhất trước | Chỉ Admin, không sửa/xóa hoặc xuất CSV/PDF nhật ký. ADR-058–062: phạm vi nghiệp vụ/bảo mật và chi tiết cần thiết, không bí mật/toàn payload. Biểu diễn/lỗi ghi/chống ngập/lưu giữ còn database/kiến trúc. | F33 |

## 8. Xử lý tự động — NV36–NV39

| Mã | Chức năng | Sự kiện → Kết quả | Quy tắc |
|---|---|---|---|
| NV36 | Xác định Band | Điểm đầu vào hợp lệ → BandAssignment | Điểm cao nhất trong các lượt COMPLETED, ngưỡng lớn nhất không vượt điểm; không chấm bài tự động. ADR-028 khóa ngưỡng sau kết quả đầu tiên, không làm tròn thành số nguyên. ADR-029: tính lại cùng transaction khi sửa hợp lệ, đồng điểm chọn lượt một; khóa điểm/Band sau xác nhận hoặc hủy/từ chối. |
| NV37 | Tạo hóa đơn | Tuyển sinh duyệt hồ sơ → Invoice | Một hóa đơn/hồ sơ; snapshot học phí dương/VND nguyên đồng; cùng transaction với ADMITTED. Hạn ngày phát hành +7 ngày lịch theo UTC+7 (ADR-043–045/047), không tự gia hạn/sửa hạn. |
| NV38 | Tính điểm tổng kết | Điểm thành phần + cấu hình chương trình → final_score | Tổng score_value × weight / 100, thang 100. ADR-028: dữ liệu nhập tối đa hai số lẻ; tính bằng decimal, không làm tròn từng đóng góp, tổng làm tròn một lần half-up hai số lẻ để lưu/hiển thị/so ngưỡng đạt. Cấu hình khóa sau lớp đầu tiên; không tự công bố. |
| NV39 | Ghi nhật ký | Thao tác nhạy cảm/sự kiện bảo mật → AuditLog | Ghi tầng ứng dụng, không trigger. ADR-058/059: giữ audit bắt buộc, thêm thay đổi cấu hình/đời lớp/lịch và đăng nhập thành công-thất bại/chặn tạm/đăng xuất/mật khẩu-tài khoản/từ chối quyền; không mỗi lượt xem hoặc gán actor từ email chưa xác thực. ADR-061: chi tiết cần thiết/cũ-mới bắt buộc, không mật khẩu/hash/JWT/bí mật phiên/toàn payload. Biểu diễn/IP/lỗi ghi/chống ngập/lưu giữ còn database/kiến trúc. |

## 9. Chức năng chung và giới hạn phạm vi

- Đăng nhập dành cho mọi vai trò; NV01 còn gồm tự đăng ký của học viên.
- Đăng xuất trong khung chung, hủy phiên hiện tại tại server, không hủy phiên
  khác hoặc thêm nút đăng xuất tất cả (ADR-057). Phiên thường tối đa 2 giờ hoặc
  idle 30 phút, không refresh/ghi nhớ; nhiều thiết bị đồng thời (ADR-055/056).
  Admin đặt lại mật khẩu theo requirements.
- Người đã đăng nhập tự đổi mật khẩu của mình, phải nhập đúng mật khẩu hiện
  tại (ADR-020), hủy mọi phiên và phải đăng nhập lại (ADR-049). Không phải
  luồng quên mật khẩu, không khôi phục qua email;
  vị trí UI còn cần chốt, chưa tự thêm màn hình/mã chức năng gốc báo cáo.
- Học viên được sửa họ tên/ngày sinh/SĐT/địa chỉ của mình, không tự đổi email
  đăng nhập; số mới phải hợp lệ và không trùng (ADR-016/019). Nơi đặt thao tác
  trên giao diện còn cần chốt; không tạo lại NV10/NV11.
- Trang chủ, danh sách khóa và trạng thái hồ sơ là các cách xem dữ liệu đã có,
  không tự tạo thêm nghiệp vụ chỉ vì có màn hình riêng.
- Không học bổng, thi trực tuyến, cổng thanh toán, trả góp, RBAC động hoặc
  phê duyệt lớp/chương trình bởi Giám đốc. Không mở rộng quản lý chi phí,
  lợi nhuận, phụ huynh hoặc thông báo tự động từ các biểu mẫu minh họa.
- Không mở quyền nhân sự sửa hồ sơ, hoàn tiền hoặc sửa điểm đã chốt. Không
  chuyển lớp sau xếp (ADR-040); sửa Payment chỉ kế toán/phương thức/mã giao dịch
  có lý do/audit, không sửa tiền/hóa đơn/ngày xác nhận (ADR-042).

## 10. Nội dung cần chủ dự án chốt tiếp

Q-011 trong `../plan/questions.md` gom các chi tiết chức năng thiếu hoặc không
thống nhất; Q-012 dành cho định nghĩa báo cáo. Các câu hỏi workflow và dữ liệu
Q-004–Q-010 còn các chi tiết mở (Q-006/Q-009 đã chốt nghiệp vụ), không được
xem là đã trả lời toàn bộ bởi danh mục này.

Đã chốt ngày 27/09/2026:

1. Bỏ hoàn toàn luồng tuyển sinh đăng ký thay (NV10/NV11); bỏ F15. Tư vấn tại
   quầy không tạo nghiệp vụ khác với học viên tự đăng ký (ADR-012).
2. File xuất báo cáo là CSV, không yêu cầu XLSX (ADR-013).
3. Tạo lịch theo quy tắc tuần thành các buổi cụ thể; thêm/điều chỉnh từng buổi,
   kiểm tra xung đột trước lưu và không tối ưu tự động (ADR-014).

4. Một hồ sơ chưa kết thúc/học viên/khóa; đăng ký khóa khác đồng thời và học
   lại khóa cũ sau hoàn thành (ADR-015/ADR-022). Tiêu chí kết thúc đã chốt;
   cách biểu diễn database còn Q-005.
5. Học viên sửa thông tin cá nhân của mình, tuyển sinh không sửa thay; không
   đổi khóa sau xác nhận nhập học (ADR-016). Chi tiết trường đã chốt ADR-019.
6. Tuyển sinh từ chối hồ sơ CONFIRMED phải nhập lý do học viên xem được;
   không từ chối sau duyệt/thu tiền (ADR-017). Quyền hủy hồ sơ đã chốt ADR-022;
   hủy lớp đã chốt ADR-026; tác động đặt ca phụ thuộc đã chốt ADR-031.

7. Phạm vi/cơ sở thống kê ba nhóm báo cáo đã chốt theo ADR-018; công thức tiến
   độ buổi/tập kết quả qua môn/đếm người/phạm vi thời gian được chốt tiếp
   ADR-063–ADR-067; bộ lọc/quyền chi tiết/xuất chốt ADR-068–ADR-072. Cơ chế truy
   vấn/xuất và hiển thị còn thiết kế; ngưỡng đạt >=50/100 ADR-024.

8. Email/mật khẩu để đăng nhập; hồ sơ học viên bắt buộc họ tên/ngày sinh/email/
   SĐT, địa chỉ tùy chọn. Sửa họ tên/ngày sinh/SĐT/địa chỉ, không tự đổi email;
   SĐT mới phải hợp lệ, không trùng (ADR-019).

9. Tự đổi mật khẩu khi đăng nhập với mật khẩu hiện tại; quên mật khẩu nhờ
   Admin, không khôi phục email. Bảo vệ Admin chính mình/cuối cùng; định nghĩa
   ACTIVE/LOCKED/DISABLED và giữ dữ liệu (ADR-020). Nhóm reset/revocation/
   khôi phục/email/chặn đăng nhập được chốt tiếp tại ADR-048–ADR-052.

10. Hủy/đổi ca trước giờ bắt đầu và xác nhận không thêm lượt; vắng không ghi 0,
    đặt lại cùng lượt; giữ lịch sử và chặn xác nhận khi thi lại đang chờ (ADR-021).
    ADR-030 yêu cầu ca nguồn OPEN để hủy/đổi; CLOSED chặn thao tác đặt chỗ.
    Ai ghi vắng/trung tâm hủy ca đã chốt ADR-027; biểu diễn lịch sử còn Q-004.

11. Học viên hủy hồ sơ của mình trước CONFIRMED; tuyển sinh hủy CONFIRMED hoặc
    ADMITTED có hóa đơn UNPAID, phải có lý do và hủy hóa đơn cùng hồ sơ. Không
    hủy sau thanh toán/hoàn tiền. Lớp COMPLETED và bảng điểm học viên FINALIZED
    là hoàn thành, không cần đạt môn; CANCELLED/REJECTED cũng cho đăng ký mới.
    Giữ hồ sơ cũ (ADR-022); không tự thêm trạng thái Registration COMPLETED.

12. Chỉ xếp vào lớp OPEN, đúng khóa/Band, còn chỗ, hồ sơ ADMITTED và hóa đơn
    PAID. Giữ ADMITTED, hiển thị đã xếp lớp từ liên kết. Giáo vụ chỉ đổi giáo viên
    trước ONGOING/bắt đầu dạy và chưa có điểm danh/điểm, kiểm tra toàn bộ lịch
    với lịch lớp/ca thi của giáo viên mới (ADR-023). Vòng đời lớp đã chốt ADR-025;
    biểu diễn buổi thực tế còn Q-008.

13. Nhập nháp khi đang dạy; chỉ gửi khi lớp COMPLETED và đủ điểm thành phần;
    giáo vụ chốt SUBMITTED của lớp COMPLETED sau kiểm tra. Chỉ sửa điểm/chú
    thích ở DRAFT/RETURNED; SUBMITTED chờ duyệt, FINALIZED bất biến. Đạt khi
    tổng kết FINALIZED >= 50/100; không thêm điều kiện chuyên cần, điểm tối thiểu
    thành phần hoặc mức xếp loại (ADR-024). Q-009 đã giải quyết.

14. Giáo vụ mở PLANNED → OPEN khi chương trình ACTIVE, có giáo viên và ít nhất
    một LESSON tương lai hợp lệ; bắt đầu OPEN → ONGOING khi thực tế dạy, không
    trước buổi đầu, có ít nhất một học viên hợp lệ; kết thúc ONGOING → COMPLETED
    sau mọi buổi học/thi đã diễn ra và đủ điểm danh LESSON. Không tự chuyển theo
    ngày/giờ, không yêu cầu gửi/chốt điểm để kết thúc (ADR-025).

15. Giáo vụ hủy chỉ PLANNED/OPEN chưa có học viên/chưa dạy, phải có lý do;
    không hủy lớp có học viên hoặc ONGOING/COMPLETED, không thêm chuyển lớp/
    hoàn tiền. Giữ lịch sử, lịch lớp CANCELLED không giữ chỗ phòng/giáo viên,
    không vào lịch dạy sắp tới hoặc số buổi hoàn thành (ADR-026).

16. Giáo viên chấm được phân công ghi vắng sau ca kết thúc, không suy từ thiếu
    điểm. Giáo vụ hủy ca trước bắt đầu với lý do, hủy đặt ca hoạt động và giữ
    lịch sử; học viên tự đặt lại cùng lượt chưa hoàn thành. Chỉ sửa ngày/giờ/
    phòng trước bắt đầu khi chưa từng có đặt; đã có đặt thì hủy/tạo mới.
    Không hủy/đổi sau bắt đầu hoặc xóa điểm đã chấm (ADR-027).

17. Khóa khóa/Band/thành phần/trọng số chương trình sau lớp đầu tiên (kể cả lớp
    bị hủy), khóa bộ ngưỡng Band sau kết quả đầu tiên. Điểm/trọng số nhập tối đa
    hai số lẻ; tổng làm tròn một lần half-up hai số lẻ trước lưu/hiển thị/xét đạt
    (ADR-028). Không thêm module version hoặc quyền sửa điểm đã chấm.

18. Chỉ người nhập điểm vẫn phụ trách ca sửa COMPLETED ở TESTING/BAND_ASSIGNED,
    có lý do/audit, tính lại điểm cao nhất/Band cùng transaction, không tăng lượt.
    Khóa sau xác nhận/hủy/từ chối, không ngoại lệ; hai lượt đồng điểm cao nhất
    chọn lượt một và giữ/hiển thị cả hai (ADR-029). Q-006 đã chốt nghiệp vụ.

19. Ca mới CLOSED; giáo vụ mở/mở lại chỉ trước bắt đầu với tài nguyên/Teacher
    ACTIVE/sức chứa hợp lệ, đầy vẫn OPEN. CLOSED giữ đặt chỗ/chặn học viên
    đăng ký-hủy-đổi, vẫn cho chấm/ghi vắng/sửa điểm theo ADR-027/029. Đến giờ
    bắt đầu áp dụng CLOSED/không mở lại; đổi giáo viên chỉ giáo vụ trước bắt
    đầu/mọi kết quả, lý do/audit/kiểm tra lịch, giữ đặt chỗ (ADR-030).

20. Hủy hồ sơ hợp lệ hủy đặt chỗ tương lai cả OPEN/CLOSED cùng hồ sơ/hóa đơn
    UNPAID và giải phóng chỗ, có xác nhận ảnh hưởng/audit. Giữ lịch sử đã bắt
    đầu/điểm/ABSENT/Band, dừng xử lý điểm/vắng sau CANCELLED; chưa chấm hiển
    thị hồ sơ đã hủy/chưa có kết quả. Đăng ký lại thi từ đầu trên hồ sơ mới,
    không chuyển điểm/Band; một phần thất bại thì không hủy phần nào (ADR-031).

ADR-031 áp dụng cho hủy hồ sơ học viên (NV02) và tuyển sinh (NV12), đồng thời
chặn NV03 đặt tiếp/NV22 nhập-sửa điểm/ghi vắng trên hồ sơ CANCELLED, kể cả
ca CLOSED. Q-007 đã chốt nghiệp vụ; còn biểu diễn lịch sử/audit ở bước database.

21. Số buổi chương trình theo khóa/Band nguyên dương, chỉ LESSON, khóa từ lớp
    đầu tiên. Đủ lịch LESSON trước bắt đầu dạy, không cần đủ để mở lớp.
    Giáo vụ chỉ sửa ngày/giờ/phòng buổi chưa bắt đầu/chưa có điểm danh hoặc
    điểm liên quan, lớp chưa COMPLETED/CANCELLED, kiểm tra xung đột (ADR-032).
    Quy tắc này áp dụng NV13/NV16/NV19; hạn đóng điểm danh theo ADR-033.

22. Giáo viên phụ trách nhập/sửa điểm danh từ lúc LESSON bắt đầu đến trước
    Class COMPLETED, không áp hạn
    cố định 24/48 giờ mỗi buổi; khi COMPLETED chỉ xem dù bảng điểm chưa gửi/
    chốt. Vẫn đủ điểm danh trước hoàn thành, kiểm soát cập nhật đồng thời
    (ADR-033). Chặn nhập trước buổi bắt đầu, không cần chờ buổi kết thúc.

23. Gửi/gửi lại, trả lại có lý do và chốt/công bố bảng điểm bắt buộc cả lớp
    trong một thao tác; đủ điểm bắt buộc của mọi học viên trước gửi, chuyển
    trạng thái toàn bộ hoặc không phần nào. Vẫn nhập/sửa từng giá trị nháp,
    được sửa rồi gửi lại sau trả bảng điểm, giữ khóa ADR-024 (ADR-034).

24. Buổi cả lớp không tổ chức (gồm nghỉ lễ) thuộc NV19/F21, không thêm module
    ngày nghỉ/lịch nghỉ toàn trung tâm, tự bỏ ngày lễ/sinh lịch bù hoặc học bù
    cá nhân. Không tính đã dạy hoặc ghi cả lớp vắng để thay thế (ADR-035).
    Cách dời/thay LESSON sau đó đã chốt ADR-036, không đảo giới hạn ADR-032.

25. Trước giờ bắt đầu dời LESSON hiện có theo ADR-032. Sau giờ dự kiến nếu
    buổi không tổ chức, giáo vụ nhập lý do, giữ lịch cũ/người/thời gian và
    tạo buổi thay thế liên kết cùng lớp; chỉ khi chưa dữ liệu phụ thuộc và
    lớp chưa COMPLETED/CANCELLED. Dùng giáo viên hiện tại/kiểm tra trùng lịch,
    lưu nhất quán, không tự xóa dữ liệu. Buổi thay thế thực hiện một nghĩa vụ
    LESSON, không tăng số buổi; hoàn thành lớp cần nó diễn ra/đủ điểm danh,
    không cần điểm danh giả buổi cũ (ADR-036). Dữ liệu còn bước database.

26. Không thêm LESSON vượt số buổi chương trình; tạo lịch tuần/thêm từng buổi
    chỉ để đủ nghĩa vụ LESSON, không có ngoại lệ buổi bổ sung/tự chọn. Buổi
    thay thế vẫn cùng nghĩa vụ, không đếm lịch cũ/thay thế hai lần hoặc áp
    giới hạn lên tổng bản ghi lịch sử; FINAL_EXAM riêng (ADR-037).

27. Khóa chọn bất biến trên mọi Registration hiện có; đổi lựa chọn bằng hủy
    hợp lệ/đăng ký hồ sơ mới/thi đầu vào lại, giữ lịch sử cũ (ADR-038).

28. Không snapshot thông tin cá nhân khi đăng ký; hồ sơ cũ hiển thị thông tin
    hiện tại. Giữ snapshot học phí hóa đơn/lịch sử nghiệp vụ/audit (ADR-039).

29. Không chuyển lớp sau xếp, kể cả trước bắt đầu dạy (ADR-040).

30. Học viên in hóa đơn của mình qua trình duyệt, đúng trạng thái thanh toán;
    không chức năng PDF riêng hoặc phát hành biên lai riêng (ADR-041).

31. Kế toán sửa phương thức/mã giao dịch Payment đã xác nhận có lý do/audit
    cũ-mới/người/giờ; khóa số tiền/hóa đơn/ngày xác nhận, giữ Invoice PAID và
    cùng Payment, không hủy/xóa/hoàn tiền (ADR-042).

32. Học phí/Invoice/Payment dương, không khóa miễn phí; tiền VND nguyên đồng,
    không nhận phần lẻ hoặc tự làm tròn tiền (ADR-043/044).

33. Hạn thu ngày phát hành +7 ngày lịch kể cả ngày nghỉ; không gia hạn/sửa hạn,
    chỉ quá hạn từ ngày kế tiếp nếu UNPAID, vẫn chỉ cảnh báo (ADR-045).

34. Chỉ lưu thời điểm xác nhận thanh toán, không ngày thu thực tế riêng hoặc
    nhập lùi; ngày xác nhận bất biến, doanh thu theo ngày này (ADR-046).

35. Giờ nghiệp vụ Asia/Ho_Chi_Minh (UTC+7) cho lịch/ngày/hạn thu/lọc báo cáo,
    không phụ thuộc múi giờ thiết bị; lưu/serialize còn bước kiến trúc (ADR-047).

Đã chốt thêm ngày 28/09/2026:

36. Cấp nhân sự/reset sinh mật khẩu tạm ngẫu nhiên, Admin xem một lần/giao
    trực tiếp; đổi trước nghiệp vụ, không log mật khẩu/reset tự mở khóa (ADR-048).

37. Khóa/ngừng dùng/reset/đổi mật khẩu hủy mọi phiên cũ, kể cả phiên hiện tại/
    JWT chưa hết hạn; mở/khôi phục không làm sống lại (ADR-049).

38. Admin khôi phục DISABLED → ACTIVE cùng tài khoản, giữ vai trò/dữ liệu/lịch
    sử, không khôi phục phiên hoặc hồ sơ nghiệp vụ đã kết thúc (ADR-050).

39. Không sửa email đăng nhập tài khoản hiện có, kể cả Admin/nhân sự (ADR-051).

40. Sai 5 lần liên tiếp trong 15 phút chặn đăng nhập 15 phút; thành công/hết
    hạn đặt lại bộ đếm, không đổi status/hủy phiên chỉ vì nhập sai. LOCKED thủ
    công cần Admin mở, DISABLED không tự khôi phục (ADR-052).

41. Mật khẩu 15–128 ký tự, cho Unicode/khoảng trắng, không bắt tổ hợp hoặc đổi
    định kỳ khi chưa sự cố; chặn phổ biến/đã lộ (ADR-053).

42. Mật khẩu tạm hết hạn sau 24 giờ từ cấp/reset, cần Admin reset lại; không
    tự khóa/ngừng dùng/mở/khôi phục tài khoản (ADR-054).

43. Phiên thường hết sau 2 giờ từ đăng nhập hoặc idle 30 phút, mốc nào trước;
    hoạt động không kéo dài hạn tuyệt đối, không refresh/ghi nhớ (ADR-055).

44. Cho nhiều thiết bị đồng thời, đăng nhập mới không đẩy phiên cũ ra (ADR-056).

45. Đăng xuất hủy phiên hiện tại tại server kể cả token chưa hết hạn, phiên
    khác giữ nguyên theo hạn/ADR-049; không nút đăng xuất tất cả (ADR-057).

46. Giữ audit bắt buộc, thêm thay đổi cấu hình khóa/chương trình/Band, trạng
    thái lớp và tạo/dời/thay lịch thành công; không mọi lượt xem (ADR-058).

47. Ghi đăng nhập thành công/thất bại/chặn tạm/đăng xuất/đổi-reset mật khẩu/
    cấp nhân sự-trạng thái tài khoản/từ chối quyền; không gán actor đã xác
    thực từ email chưa xác thực (ADR-059).

48. Admin lọc ngày/người/hành động/loại-ID/kết quả, phân trang mới nhất trước
    (ADR-060).

49. Chi tiết sự kiện/trường thay đổi cần thiết, giữ cũ/mới bắt buộc; không mật
    khẩu/hash/JWT/bí mật phiên/toàn payload, hạn chế dữ liệu cá nhân (ADR-061).

50. Admin chỉ tra cứu nhật ký trong ứng dụng, không xuất CSV/PDF; giữ quyền
    không sửa/xóa log qua nghiệp vụ thông thường (ADR-062).

51. Báo cáo theo trạng thái hiện tại lúc tra cứu, không tái dựng tại ngày quá
    khứ; giữ lịch sử nghiệp vụ/audit và snapshot tài chính (ADR-063).

52. Tổng phải thu kỳ phát hành từ UNPAID+PAID/loại CANCELLED; nợ hiện tại riêng,
    không áp kỳ thanh toán hoặc trừ khác tập với doanh thu (ADR-064).

53. Học viên duy nhất trong mỗi nhóm khóa và tổng toàn tập lọc; không cộng
    nhóm, số hồ sơ riêng/học lại không tự tăng số người (ADR-065).

54. Tiến độ LESSON đã xác nhận diễn ra/số LESSON chương trình, buổi không tổ
    chức không tính/thay thế một nghĩa vụ; FINAL_EXAM riêng (ADR-066).

55. Tỷ lệ đạt trên GradeReport FINALIZED trong kỳ chốt, mỗi lượt học riêng/
    loại chưa chốt; không dữ liệu không phải 0% (ADR-067).

56. Tuyển sinh mặc định tháng Việt Nam hiện tại/mọi trạng thái, giữ khóa
    ngừng để tra lịch sử, không thêm quyền cho Tuyển sinh (ADR-068).

57. Đào tạo lọc khóa/Band/lớp/giáo viên hiện tại/trạng thái, hiện trạng không
    kỳ ngày; kết quả thêm kỳ chốt mặc định tháng hiện tại (ADR-069).

58. Tài chính mặc định tháng hiện tại theo cơ sở đã chốt, lọc khóa nhất quán;
    công nợ hiện tại có chỉ quá hạn/không lọc kỳ ngày (ADR-070).

59. Giám đốc chỉ nhóm phù hợp/không hồ sơ-hóa đơn-định danh học viên, xuất
    hoặc phê duyệt; tra cứu hóa đơn nghiệp vụ Kế toán không đổi (ADR-071).

60. CSV Kế toán toàn kết quả lọc/cột báo cáo, không chỉ trang/trường ẩn/hồ sơ
    cá nhân hoặc xuất riêng hồ sơ-hóa đơn-học viên dưới quyền này (ADR-072).

61. Tên Unicode/trim/1–100 ký tự, không bắt hai từ/ASCII, không thêm quyền
    sửa nhân sự (ADR-073).

62. Ngày sinh học viên có thật/không tương lai theo giờ Việt Nam, không giới
    hạn tuổi hoặc bắt nhân sự có ngày sinh (ADR-074).

63. Email tạo tài khoản trim/định dạng/<=254, so trùng toàn tài khoản không
    phân biệt hoa-thường/không gộp dấu chấm hoặc +tag theo nhà cung cấp (ADR-075).

64. SĐT liên hệ chỉ phạm vi di động Việt Nam 10 số bắt đầu 0/+84 tương đương,
    chuẩn hóa trước so trùng; không quốc tế/bàn/OTP hoặc bắt nhân sự có SĐT (ADR-076).

65. Địa chỉ học viên tùy chọn/trim/Unicode<=255/tự do, không danh mục địa phương
    hoặc snapshot cá nhân (ADR-077).

66. Tên khóa/chương trình Unicode trim/1–150, thành phần điểm 1–100; giữ
    unique thành phần theo chương trình/khóa cấu hình (ADR-078).

67. Mã khóa/lớp tự nhập/trim/hoa/1–30 A–Z-số-_-gạch, duy nhất trong loại kể
    cả ngừng/hủy, không đổi sau tạo/tái dùng lịch sử (ADR-079).

68. Mã phòng cùng định dạng, chuẩn hóa chung lịch lớp-ca, không tự gộp bí danh
    hoặc module/danh mục phòng (ADR-080).

69. Lý do 1–1000 chỉ nơi bắt buộc; nhận xét/chú thích tùy chọn <=1000, Unicode/
    văn bản thuần/trim/giữ xuống dòng, không rich text hoặc cắt ngắn (ADR-081).

70. Mã giao dịch trim/<=100/giữ nội dung, chuyển khoản bắt buộc/tiền mặt có thể
    bỏ trống, không unique toàn hệ thống; vẫn một Payment/hóa đơn (ADR-082).

File duyệt tập trung: `../plan/OWNER_REVIEW.md`, 55 mục chưa tích ban đầu.
Phạm vi chi tiết Tuyển sinh/Giáo vụ, tra cứu hóa đơn Kế toán, lịch sử lịch
Học viên/menu tài khoản chung là A01–A05; danh mục/vòng đời cấu hình/trường
danh sách Giáo viên là A06–A09. Đọc lựa chọn owner tại đó, không hỏi lại từng nhóm;
chỉ đưa vào catalog sau đồng ý, hoàn thiện đầu ra rồi mới xin nghiệm thu E01.
nhật ký còn biểu diễn/lưu/lỗi ghi/chống ngập/lưu giữ/backup ở bước kỹ thuật,
định nghĩa hoạt động/phiên hạn chế và cơ chế thực thi còn bước kiến trúc,
gom câu hỏi theo nhóm; không tự coi giai đoạn
workflow đã được chốt hoặc bắt đầu triển khai code.
Không đánh dấu giai đoạn 1 hoàn tất chỉ vì đã chốt các nội dung trên.
