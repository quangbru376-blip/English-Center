# Nguồn báo cáo và cách sử dụng

Ngày tiếp nhận/chốt: 27/09/2026.

## Nguồn chính thức

- Báo cáo: `C:\Study\SOEN\final_workplace\Nhom18_ThietKeGiaoDien.docx`.
- SHA-256 lúc tiếp nhận:
  `7B7F8C0ABC6A44476E70A8D9DE8B03C592B9AA29DC3ED5F97AAB247F0877C258`.
- Đã đọc toàn bộ chữ/bảng và hình có nội dung riêng. Có 52 bảng và 21 media
  assets, trong đó 4 ảnh trùng; có 2.179 đoạn gồm cả ô bảng và text box.

Chủ dự án xác nhận đây là báo cáo nền tảng chính thức. File Word giữ nguyên;
`docs/requirements.md`, `docs/ARCHITECTURE.md` và `docs/DECISIONS.md` chứa
những nội dung chuẩn hóa để thực thi. Agent không cần đọc lại toàn bộ Word
mỗi phiên. Biểu mẫu phần IV không là nguồn yêu cầu.

## Điều chỉnh đã được chủ dự án chốt

| Chủ đề | Quyết định dùng khi triển khai |
|---|---|
| Giới hạn thi | Tối đa hai lượt cho mỗi Registration. |
| Thi lại | Sau khi lượt một có điểm, trước xác nhận nhập học. |
| Trọng số | Theo TrainingProgram của khóa/Band. |
| Thang điểm | Lưu và hiển thị 0–100 cho thi đầu vào và kết quả khóa học. |
| Quá hạn | Cảnh báo/số ngày quá hạn; không tự đổi trạng thái học viên. |
| Học bổng | Bỏ cả chức năng và các bảng dữ liệu khỏi phạm vi. |
| Đăng ký tại quầy | Vẫn là học viên tự đăng ký; bỏ NV10/NV11 và F15, không có tuyển sinh đăng ký thay. |
| Xuất báo cáo | CSV; không yêu cầu XLSX. |
| Tạo lịch | Sinh buổi cụ thể theo quy tắc tuần; thêm/điều chỉnh từng buổi; kiểm tra xung đột, không tối ưu tự động. |
| Hồ sơ trùng/học lại | Một hồ sơ chưa kết thúc/học viên/khóa; được đăng ký khóa khác đồng thời và học lại sau hoàn thành. |
| Sửa thông tin | Học viên sửa thông tin cá nhân của mình; tuyển sinh không sửa thay; khóa đã chọn không đổi sau xác nhận nhập học. |
| Từ chối nhập học | Tuyển sinh chỉ từ chối hồ sơ CONFIRMED, phải có lý do học viên xem được; không từ chối sau duyệt/thu tiền. |

## Cách diễn giải những phần không đồng bộ của báo cáo

- II: tự xác định Band từ điểm giáo viên nhập, không tự chấm bài đầu vào.
- V: duyệt hồ sơ tạo Invoice; xác nhận thanh toán tạo Payment/cập nhật PAID.
- V: sơ đồ use case thiếu luồng thi đầu vào/thi lại/Band/xác nhận; khi lập
  plan phải lấy workflow đầy đủ trong requirements.
- VI.1/VI.4: hình schema là phiên bản cũ; dùng bảng và ràng buộc VI.2/VI.3
  đã chuẩn hóa trong memory, không lấy các trường phê duyệt/max_score/học bổng
  hoặc liên kết sai từ ảnh để scaffold database.
- VII.1: nhánh tài khoản/nhật ký thuộc Admin; xếp lớp chỉ một chức năng.
- VII.2.2: học viên tự tạo tài khoản.
- VII.2.4: hình và thao tác hồ sơ là Tuyển sinh, dù tiêu đề ghi Giáo vụ.
- VII: ảnh bảng điểm /10 phải được thực hiện bằng /100 theo quyết định mới.

## Truy vết bổ sung: hủy ca thi đầu vào — ADR-027

- Báo cáo III, chức năng 14: tạo ca/phòng, giới hạn 20 học viên và phân công
  giáo viên chấm; không mô tả đầy đủ thao tác hủy.
- VI → 6.2, bảng 9 EntryTestSession: status OPEN/CLOSED/CANCELLED.
- VI → 6.3, ràng buộc miền: ca CANCELLED không được tổ chức thi hoặc nhập điểm.
- Chủ dự án đồng ý bổ sung workflow tại ADR-027: giáo viên được phân công ghi
  vắng sau kết thúc; giáo vụ hủy trước bắt đầu với lý do và giữ lịch sử/cho tự
  đặt lại cùng lượt; giới hạn sửa ngày/giờ/phòng theo lịch sử đặt ca.
- Đây là chi tiết bổ sung đã được chấp nhận, không phải toàn bộ nội dung đã có
  sẵn trong Word. Không sửa báo cáo nguồn hoặc dùng biểu mẫu để suy ra quyền.

## Truy vết bổ sung: đóng/mở ca và đổi giáo viên chấm — ADR-030

ADR-030 bổ sung do chủ dự án chấp nhận: tạo ca CLOSED, đóng/mở/mở lại trước
bắt đầu với điều kiện tài nguyên/sức chứa, khóa mọi thao tác đặt chỗ học viên
trong CLOSED, đóng theo giờ bắt đầu và đổi giáo viên chấm trước bắt đầu/kết
quả có lý do/audit. CLOSED vẫn cho chấm/ghi vắng/sửa theo ADR-027/029.
Đây là workflow bổ sung từ quyết định chủ dự án, không suy ra đầy đủ từ Word.

## Truy vết bổ sung: hủy hồ sơ và đặt ca phụ thuộc — ADR-031

ADR-031 là workflow hủy hồ sơ bổ sung được chủ dự án chấp nhận: hủy đặt chỗ
tương lai kể cả CLOSED cùng hồ sơ/hóa đơn UNPAID; giữ lịch sử đã bắt đầu và
kết quả/Band, khóa xử lý kết quả sau hủy; đăng ký lại thi từ đầu trên hồ sơ mới,
audit và transaction đồng bộ. Chi tiết này thuộc quyết định chủ dự án, không
là nội dung suy ra từ biểu mẫu hoặc thay đổi báo cáo Word.

## Truy vết bổ sung: số buổi, lịch đủ và sửa lịch lớp — ADR-032

ADR-032 bổ sung từ quyết định chủ dự án, không suy ra từ biểu mẫu Word:
số LESSON dự kiến thuộc chương trình khóa/Band và khóa sau lớp đầu tiên;
lịch đủ trước bắt đầu dạy, không đổi điều kiện mở lớp; giới hạn sửa ngày/giờ/
phòng theo thời điểm bắt đầu, dữ liệu phụ thuộc và trạng thái lớp. Báo cáo
nguồn không được sửa; chi tiết database vẫn chưa hoàn tất.

## Truy vết bổ sung: hạn nhập/sửa điểm danh — ADR-033

ADR-033 bổ sung từ quyết định chủ dự án: hạn nhập/sửa điểm danh trước lớp
COMPLETED, không hạn cố định 24/48 giờ từng buổi; sau COMPLETED chỉ xem dù
bảng điểm chưa gửi/chốt. Không suy chính sách này từ biểu mẫu hoặc sửa Word;
chủ dự án cũng chốt mở nhập/sửa từ lúc LESSON bắt đầu, không trước đó hoặc
chỉ sau buổi kết thúc. Cả hai mốc của cửa sổ điểm danh được lưu trong ADR-033.

## Truy vết bổ sung: gửi và duyệt bảng điểm toàn lớp — ADR-034

Chủ dự án yêu cầu bắt buộc xử lý cả lớp trong một thao tác khi gửi/duyệt:
ADR-034 áp dụng cho gửi, gửi lại, trả lại có lý do và chốt/công bố toàn lớp,
không xử lý riêng học viên. Giữ luồng sửa rồi gửi lại và quyền khóa điểm
ADR-024. Đây là chi tiết được chủ dự án chốt, không suy từ biểu mẫu Word;
không sửa báo cáo nguồn hoặc coi database/kiến trúc đã hoàn tất.

## Truy vết bổ sung: phạm vi ngày nghỉ/buổi không tổ chức — ADR-035

NV19/F21 đã bao gồm quản lý lịch học và lịch thi. Chủ dự án chấp nhận xử lý
buổi cả lớp không tổ chức trong chức năng này, không thêm module ngày nghỉ,
lịch nghỉ toàn trung tâm, tự động bỏ ngày lễ/sinh lịch bù hoặc học bù cá nhân.
Không tính buổi không tổ chức là đã dạy hoặc ghi cả lớp vắng để thay thế.
ADR-035 chỉ chốt phạm vi; workflow được bổ sung ở ADR-036, dữ liệu còn Q-011/Q-008.
Đây là quyết định bổ sung của chủ dự án, không suy từ biểu mẫu/sửa Word.

## Truy vết bổ sung: dời/thay LESSON không tổ chức — ADR-036

Chủ dự án chấp nhận phương án kết hợp: trước giờ bắt đầu dời buổi hiện có
theo ADR-032; sau giờ dự kiến nếu không tổ chức, giáo vụ ghi nhận lý do,
giữ lịch cũ/người/thời gian và tạo buổi thay thế liên kết cùng lớp, giáo viên
hiện tại/kiểm tra trùng lịch. Chặn nếu có dữ liệu điểm danh/điểm liên quan
hoặc lớp COMPLETED/CANCELLED, không xóa dữ liệu. Buổi thay thế thực hiện một
nghĩa vụ LESSON, không tăng số buổi; hoàn thành lớp xét buổi cần học/thay thế
đã diễn ra/đủ điểm danh thay vì buộc buổi cũ không tổ chức có điểm danh giả.
ADR-036 bổ sung tập buổi xét hoàn thành ADR-025, không nới quyền ghi đè lịch
ADR-032 hoặc thêm module ngoài ADR-035. Đây là quyết định chủ dự án, không
suy từ biểu mẫu hoặc sửa báo cáo Word; chưa có schema/code.

## Truy vết bổ sung: không thêm LESSON ngoài chương trình — ADR-037

Chủ dự án không cho thêm buổi học vượt số LESSON của chương trình. ADR-037
giới hạn tạo lịch tuần/thêm từng buổi vào các nghĩa vụ LESSON cần có, không
tạo buổi bổ sung ngoài chương trình. Buổi thay thế ADR-036 vẫn cùng nghĩa vụ,
không đếm lịch cũ/thay thế hai lần hoặc giới hạn tổng bản ghi lịch sử;
FINAL_EXAM riêng. Đây là quyết định chủ dự án, không suy từ biểu mẫu Word;
không đổi báo cáo nguồn hoặc coi schema/code đã có.

## Truy vết bổ sung: 5 quyết định hồ sơ/lớp/hóa đơn/thanh toán — ADR-038–ADR-042

Chủ dự án đồng ý cả nhóm: không sửa khóa trên hồ sơ hiện có, hủy hợp lệ/đăng
ký mới/thi lại từ đầu; không snapshot thông tin cá nhân, hồ sơ lịch sử dùng
profile hiện tại; không chuyển lớp sau xếp, kể cả trước dạy; học viên in hóa
đơn của mình qua trình duyệt đúng trạng thái, không PDF/biên lai riêng; kế toán
chỉ sửa phương thức/mã giao dịch đã xác nhận có lý do/audit cũ-mới/người/giờ.
Số tiền/hóa đơn/ngày xác nhận bất biến, không hủy/xóa Payment/hoàn tiền; giữ
snapshot học phí/lịch sử nghiệp vụ. Đây là quyết định chủ dự án, không suy từ
biểu mẫu Word; không sửa báo cáo nguồn hoặc coi code/schema đã triển khai.

## Truy vết bổ sung: tiền, hạn thu và thời gian nghiệp vụ — ADR-043–ADR-047

Chủ dự án đồng ý nhóm 5: học phí dương/không khóa miễn phí; VND nguyên đồng
thay phần lẻ tiền trong báo cáo; hạn thu ngày phát hành +7 ngày lịch kể cả ngày
nghỉ; chỉ thời điểm xác nhận thanh toán, không thêm ngày thu thực tế; UTC+7
thống nhất cho ngày/lịch/hạn thu/lọc báo cáo. paid_at giữ ý nghĩa xác nhận trong
báo cáo và cơ sở doanh thu ADR-018; không đổi khóa bất biến ADR-042 hoặc cảnh
báo quá hạn ADR-010. Đây là lựa chọn chủ dự án, không suy từ biểu mẫu Word;
kiểu dữ liệu/lưu thời gian còn bước thiết kế, không sửa báo cáo nguồn.

## Truy vết bổ sung: tài khoản và bảo mật — ADR-048–ADR-052

Ngày 28/09/2026 chủ dự án đồng ý: mật khẩu tạm ngẫu nhiên khi cấp nhân sự/reset,
Admin xem một lần/giao trực tiếp/bắt buộc đổi trước nghiệp vụ; hủy mọi phiên
khi khóa/ngừng dùng/reset/đổi mật khẩu; Admin khôi phục DISABLED trên cùng tài
khoản; không sửa email tài khoản hiện có; 5 lần sai liên tiếp trong 15 phút
chặn đăng nhập 15 phút mà không đổi trạng thái/hủy phiên đang dùng. Reset không
tự mở/khôi phục; phiên cũ không sống lại khi mở/khôi phục; giữ dữ liệu/lịch sử.
Đây là bổ sung của chủ dự án, không suy từ biểu mẫu hoặc sửa Word. Độ mạnh/
thời hạn được chốt tiếp ADR-053–ADR-057; cơ chế lưu/thực thi còn thiết kế.

## Truy vết bổ sung: mật khẩu và phiên — ADR-053–ADR-057

Ngày 28/09/2026 chủ dự án đồng ý: mật khẩu 15–128 ký tự cho Unicode/khoảng
trắng, không bắt tổ hợp/đổi định kỳ khi chưa sự cố, chặn phổ biến/đã lộ;
mật khẩu tạm 24 giờ từ cấp/reset; phiên thường 2 giờ tuyệt đối hoặc 30 phút
nhàn rỗi, không refresh/ghi nhớ; nhiều thiết bị; đăng xuất chỉ hủy phiên hiện
tại tại server/không nút hủy tất cả. ADR-049 vẫn hủy mọi phiên theo sự kiện.
Đây là lựa chọn chủ dự án, không suy từ biểu mẫu hoặc sửa Word; định nghĩa
hoạt động/phiên hạn chế/lưu-thực thi còn database/kiến trúc, chưa schema/code.

## Truy vết bổ sung: phạm vi và tra cứu nhật ký — ADR-058–ADR-062

Ngày 28/09/2026 chủ dự án đồng ý: giữ audit bắt buộc, bổ sung cấu hình/đời
lớp/tạo-dời-thay lịch, không mọi lượt xem; sự kiện đăng nhập thành công-thất
bại/chặn tạm/đăng xuất/mật khẩu-tài khoản/từ chối quyền; lọc có cấu trúc và
phân trang Admin; chi tiết cần thiết/không bí mật hoặc toàn payload; không
xuất CSV/PDF nhật ký. Giữ quyền chỉ Admin xem/không sửa-xóa log thông thường,
cũ-mới bắt buộc và lịch sử nghiệp vụ; không thêm lịch sử đổi giáo viên lớp.
Đây là quyết định bổ sung, không suy từ biểu mẫu hoặc sửa Word. Cơ chế lưu/
lỗi ghi/chống ngập/lưu giữ/backup còn thiết kế database/kiến trúc, chưa code.

## Truy vết bổ sung: chỉ số báo cáo — ADR-063–ADR-067

Ngày 28/09/2026 chủ dự án đồng ý: báo cáo trạng thái hiện tại/không tái dựng tại
ngày quá khứ; tổng phải thu từ hóa đơn UNPAID+PAID phát hành trong kỳ, loại
CANCELLED, công nợ hiện tại tách riêng; học viên duy nhất mỗi khóa/tổng toàn
tập lọc không cộng nhóm; tiến độ LESSON đã xác nhận diễn ra/số buổi chương
trình, thay thế một nghĩa vụ/FINAL_EXAM riêng; tỷ lệ đạt theo kết quả FINALIZED
trong kỳ chốt điểm, mỗi lượt học riêng/không dữ liệu khác 0%. Giữ ngưỡng đạt,
lịch sử nghiệp vụ và snapshot tài chính. Đây là quyết định bổ sung của chủ
dự án, không suy từ biểu mẫu hoặc sửa Word; bộ lọc/quyền chi tiết/xuất chốt
tiếp ADR-068–ADR-072, cơ chế truy vấn/hiển thị còn thiết kế, chưa schema/code.

## Truy vết bổ sung: bộ lọc và quyền báo cáo — ADR-068–ADR-072

Ngày 28/09/2026 chủ dự án đồng ý: mặc định tháng Việt Nam hiện tại cho kỳ báo
cáo, mọi trạng thái/giữ khóa ngừng để xem lịch sử; hiện trạng đào tạo lọc khóa/
Band/lớp/giáo viên hiện tại/trạng thái không kỳ ngày, kết quả theo kỳ chốt;
tài chính lọc khóa nhất quán/công nợ hiện tại không kỳ ngày và có chỉ quá hạn.
Giám đốc chỉ nhóm phù hợp, không hồ sơ/hóa đơn/định danh học viên, xuất/phê
duyệt. Kế toán CSV toàn kết quả lọc/cột báo cáo, không chỉ trang/trường ẩn/hồ
sơ cá nhân hoặc xuất riêng loại hồ sơ khác. Tra cứu hóa đơn Kế toán không đổi,
không thêm quyền báo cáo khác. Đây là quyết định bổ sung, không suy từ biểu
mẫu hoặc sửa Word; truy vấn/xuất nhất quán, cấu trúc/giới hạn/mã hóa/an toàn
CSV và hiển thị tỷ lệ còn thiết kế kỹ thuật.

## Truy vết bổ sung: kiểm tra thông tin nhập — ADR-073–ADR-077

Ngày 28/09/2026 chủ dự án đồng ý: tên trim/1–100 ký tự Unicode/không bắt hai
từ; ngày sinh thật/không tương lai theo giờ Việt Nam/không giới hạn tuổi;
email tạo tài khoản trim/định dạng/<=254, so trùng không phân biệt hoa-thường/
không gộp dấu chấm hoặc +tag theo nhà cung cấp; phạm vi SĐT di động Việt Nam
10 số bắt đầu 0, nhận +84 tương đương/khoảng trắng-dấu gạch và chuẩn hóa-so
trùng; địa chỉ tùy chọn/trim/Unicode<=255/tự do. Không xác minh email/OTP/danh
mục nhà mạng-địa phương hoặc thêm trường bắt buộc/quyền sửa nhân sự. Đây là
các giới hạn project được chủ dự án chọn, không suy từ biểu mẫu, không là
khẳng định mọi số điện thoại Việt Nam có định dạng trên. Word không đổi;
kiểm tra/lưu chính xác còn kỹ thuật, chưa schema/code.

## Truy vết bổ sung: các trường nghiệp vụ còn lại — ADR-078–ADR-082

Ngày 28/09/2026 chủ dự án đồng ý: tên khóa/chương trình Unicode trim/1–150,
thành phần điểm 1–100; mã khóa/lớp tự nhập/trim/hoa/1–30 A–Z-số-_-gạch/duy
nhất trong loại kể cả ngừng-hủy/khóa sau tạo; mã phòng cùng định dạng và chuẩn
hóa chung lịch lớp-ca, không tự gộp bí danh/không module phòng; lý do 1–1000
chỉ nơi bắt buộc, chú thích/nhận xét tùy chọn <=1000/văn bản thuần/giữ xuống
dòng; mã giao dịch trim/<=100/giữ nội dung, không unique toàn hệ thống, vẫn
một Payment/hóa đơn/chuyển khoản có mã. Giữ khóa cấu hình/điểm/quyền/audit,
không cắt ngắn dữ liệu. Đây là giới hạn project chủ dự án chọn, không suy từ
biểu mẫu hoặc chuẩn kỹ thuật. Word không đổi; kiểm tra/lưu còn thiết kế dữ liệu/
kiến trúc, chưa code/schema hoặc chấp nhận toàn baseline.

## Truy vết bổ sung: owner duyệt A–C — ADR-083–ADR-094

Ngày 28/09/2026 owner tích [x] đủ A01–A10/B01–B07/C01–C16 trong
`../plan/OWNER_REVIEW.md` và xác nhận chỉ A–C đã duyệt, D/E còn lại.
Đã ghi nhận 33 lựa chọn vào requirements/ADRs/catalogue/target model: quyền
xem/menu/lịch sử, catalogue/class/score workflows, HELD/NOT_HELD/FINAL_EXAM,
lịch và các hợp đồng dữ liệu/transaction/phiên/báo cáo. EntryTestBooking và
AuthSession mở rộng mô hình báo cáo 20 entity thành 22 target entities;
replay-result store/dictionary đầy đủ chưa nghiệm thu. Đây là quyết định bổ
sung của owner, không khẳng định toàn bộ có trong Word hoặc code đã triển khai.
D01–D17/E01–E05 chưa duyệt. Không sửa báo cáo nguồn hoặc biểu mẫu minh họa.

## Truy vết bổ sung: owner duyệt Gói 1 frontend — ADR-095–ADR-102

Ngày 29/09/2026 owner đồng ý Gói 1: D01–D04/D06–D07/D10/D15. D15 dùng
ưu tiên Học viên 375 px/nhân sự 1280 px, tất cả kiểm tra ba kích thước.
Ghi nhận module/server ownership, API/errors/pagination, interactive idle/
tab chung, restricted session 10 phút, same-origin JWT cookie/CSRF, login
privacy/IP 100 request/phút, CSV 10.000 dòng/5 MB/format/formula safeguards
và phạm vi kiểm chứng tương lai. Đây là lựa chọn bổ sung của owner, không
suy từ Word hoặc chứng cứ triển khai. Gói 2 bàn từng phần; FE01a/FE01b/FE02
đã duyệt riêng ADR-103–ADR-105 từ trao đổi với owner ngày 29/09/2026:
khung responsive/menu theo vai trò và cấu trúc trang/đường đi, không thêm
chức năng hoặc quyền. Tại thời điểm đó các phần còn lại/Gói 3 chưa duyệt. Còn
D05/D08/D09/D11–D14/D16/D17 và E; câu hỏi hosting không là phê duyệt vận hành.

## Truy vết bổ sung: owner duyệt toàn bộ Gói 2 — ADR-106–ADR-113

Ngày 29/09/2026 owner xác nhận đồng ý hoàn toàn Gói 2 đã đề xuất trong chat.
Đối chiếu đề xuất gốc: FE01–FE10 khung giao diện chung, không là tài liệu tự
host hoặc Gói 3. Giữ FE01a/FE01b/FE02 đã chốt, bổ sung FE03–FE10 thành tám
dòng OWNER_REVIEW và ADR-106–ADR-113: responsive lists, quyền/điều kiện,
form/validation, loading/errors, confirmation, component cục bộ, định dạng
Việt Nam và hướng hình ảnh. Không suy các lựa chọn này từ Word/mockup.
Tại thời điểm đó Gói 3 FE11–FE19, chín D và E chưa duyệt; không đổi source report, không
code/packages/hosting hoặc khai báo kiểm chứng đã chạy.

## Truy vết bổ sung: owner cập nhật bảng D và chốt người vận hành — ADR-114–ADR-120

Ngày 29/09/2026 owner báo cập nhật OWNER_REVIEW; đối chiếu thấy D05/D08/D09/
D11/D14/D16 đã tích, giữ đề xuất gốc và không ghi phương án thay thế. Lưu
ADR-114–ADR-119: hash/generator/blocklist offline; mandatory audit rollback/
pure security events best-effort; mandatory history/pure logs-IP 90 ngày;
phạm vi một trung tâm/10.000 học viên/50 đồng thời; backup mã hóa hằng ngày/
7 ngày-4 tuần/restore tháng/RPO24h-RTO2h mục tiêu; .NET10/EF10/SQL2022 Developer
non-production. Đây là quyết định lịch sử29/09; lựa chọn SQL được owner cho
phép đổi sang bản đang có SQL2025 Express ngày04/10/2026 (ADR-162), không
sửa báo cáo Word hoặc nguồn ban đầu. Không suy từ báo cáo hoặc câu hỏi hosting.

D17 ban đầu chưa tích và yêu cầu hỏi-chốt người phụ trách một lần rồi mặc
định. Owner trả lời đồng ý tự phụ trách toàn bộ release/secret/backup-restore/
bảo trì; lưu ADR-120, cập nhật ô D17, giữ ghi chú gốc. Chỉ hỏi lại khi owner
yêu cầu đổi người; không thêm quyền Admin/DBA trong ứng dụng. D12/D13 giữ
chưa tích/ghi chú hoãn đến khi chốt nơi chạy web/phần mềm. Tổng 59/66 dòng
đã duyệt, 2 D và 5 E còn mở; Gói 3 chưa duyệt. Không sửa Word, không code,
cài đặt/triển khai/backup/test hoặc nghiệm thu E.

## Truy vết bổ sung: owner duyệt Gói 3 — ADR-121–ADR-129

Ngày 29/09/2026 owner yêu cầu trình một lượt FE11–FE19, sau đó xác nhận ghi
nhận toàn bộ Gói 3, không nêu ngoại lệ. Lời duyệt trực tiếp là nguồn cập nhật
chín ô OWNER_REVIEW và ADR-121–ADR-129, không suy từ ô trống hoặc Gói 2.
Giữ nguyên 66 dòng trước đó/ghi chú owner và ADR-001–ADR-120; đồng bộ
requirements mục 4.8, target architecture, catalog và kế hoạch/handoff.

Chốt mẫu trang chủ Student, lịch/preview tuần, điểm danh theo LESSON và nháp
điểm theo học viên, thao tác bảng điểm cả lớp, báo cáo, in Invoice của mình,
bàn giao mật khẩu tạm một lần và chi tiết audit. Không thêm quyền, module,
NV/F, entity hoặc package. Có 68/75 dòng duyệt; D12/D13 vẫn hoãn và E01–E05
chờ đầu ra thật. Không sửa Word, không code, vận hành hoặc tuyên bố test pass.

## Tiến độ có thể kết luận

Tài liệu có phân tích nghiệp vụ, thiết kế dữ liệu và mockup. Không đủ bằng
chứng để kết luận API, database hay chức năng thực tế đã được triển khai.
Không lấy chữ "dữ liệu mẫu" hoặc ảnh UI preview làm chứng cứ test pass.

Các vấn đề còn mở được lưu tại `docs/plan/questions.md`; chúng không bị xem
là đã chốt chỉ vì xuất hiện trong nhận xét của agent.
