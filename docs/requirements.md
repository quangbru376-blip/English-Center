# Yêu cầu nghiệp vụ chính thức — Quản lý trung tâm Anh ngữ

Cập nhật: 28/09/2026. Đây là nguồn yêu cầu dùng để lập kế hoạch và triển khai.
Nội dung được chuẩn hóa từ báo cáo `Nhom18_ThietKeGiaoDien.docx`, các quyết
định của chủ dự án và những yêu cầu trước đó không bị thay thế.

Các quy tắc dưới đây đã được chấp nhận trừ mục ghi rõ **Chưa chốt**. Agent
không tự thêm nghiệp vụ để lấp khoảng trống. Quyết định mới phải cập nhật
file này và ADR liên quan. Không cần sửa file Word để thay đổi yêu cầu dự án.

## 1. Phạm vi

Web responsive bằng tiếng Việt, gồm tuyển sinh, thi đầu vào, đào tạo, học phí,
báo cáo và quản trị. Giữ mã gốc F01–F33 để truy vết; F15 đã loại bỏ,
còn 32 mã màn hình trong phạm vi hiện tại.

Ngoài phạm vi: học bổng (cả chức năng và bảng dữ liệu), thi trực tuyến, cổng
thanh toán, trả góp và phê duyệt mở lớp/chương trình của Giám đốc. Không tạo
`ScholarshipGrant`, `ScholarshipCandidate` hoặc `ScholarshipCriteria`.

Biểu mẫu ở phần IV của báo cáo chỉ là minh họa, không định nghĩa chức năng,
trường dữ liệu hoặc công thức. Chúng sẽ được điều chỉnh sau khi chốt sản phẩm.

ADR-035: xử lý buổi cả lớp không tổ chức (gồm nghỉ lễ) trong quản lý lịch NV19/F21,
không thêm module ngày nghỉ/lịch nghỉ toàn trung tâm, tự bỏ ngày lễ, tự sinh lịch
bù hoặc học bù riêng cho học viên vắng. Dời/thay LESSON theo ADR-036.
ADR-037: không cho thêm LESSON vượt số buổi chương trình; buổi thay thế không
tăng nghĩa vụ LESSON, FINAL_EXAM tính riêng.

## 2. Vai trò và trách nhiệm

Mỗi tài khoản có đúng một vai trò cố định. Hệ thống tự động không phải vai
trò tài khoản. Tên mã dưới đây theo convention đã có trong project.

| Vai trò | Mã | Quyền nghiệp vụ |
|---|---|---|
| Học viên | Student | Tự tạo tài khoản; sửa thông tin cá nhân của mình; đăng ký khóa/ca thi; hủy hồ sơ của mình trước CONFIRMED; xem điểm và Band; xác nhận nhập học; xem/in hóa đơn qua trình duyệt, xem lớp, lịch và bảng điểm đã chốt của mình. |
| Tuyển sinh | Admissions | Tra cứu/kiểm tra hồ sơ, theo dõi trạng thái; duyệt hoặc từ chối hồ sơ CONFIRMED; hủy hồ sơ CONFIRMED hoặc ADMITTED chưa thanh toán theo ADR-022. Không sửa thông tin học viên, tạo tài khoản/hồ sơ hoặc đăng ký thi thay. |
| Giáo vụ | AcademicAffairs | Quản lý khóa/chương trình/Band, cấu hình điểm, ca thi, phân công chấm, lớp, lịch, xếp lớp và trả lại/chốt bảng điểm. |
| Giáo viên | Teacher | Xem ca thi/lớp được phân công; nhập điểm thi đầu vào, sửa điểm trong giới hạn ADR-029, ghi nhận vắng thi theo ADR-027; điểm danh, nhập và gửi bảng điểm lớp phụ trách. |
| Kế toán | Accountant | Tra cứu hóa đơn, đối chiếu và xác nhận thanh toán; sửa phương thức/mã giao dịch có lý do/audit theo ADR-042; báo cáo học phí. |
| Giám đốc | Director | Chỉ xem tổng hợp tuyển sinh, đào tạo và tài chính; không phê duyệt nghiệp vụ. |
| Quản trị viên | Admin | Cấp tài khoản nhân sự, reset mật khẩu tạm, khóa/mở/ngừng dùng/khôi phục tài khoản nhân sự và học viên; xem nhật ký. Không sửa email đăng nhập, tự khóa/vô hiệu hóa mình hoặc Admin hoạt động cuối cùng. |

Backend phải kiểm tra quyền và ownership, không dựa vào menu hoặc id do
client cung cấp. Giáo viên chỉ thao tác ca/lớp được phân công; học viên chỉ
truy cập dữ liệu của mình. Không bổ sung quyền sửa điểm đặc biệt khi chưa chốt.

## 3. Workflow chính

1. Học viên tự tạo tài khoản và đăng nhập.
2. Học viên chọn khóa đang hoạt động và tự tạo `Registration`.
3. Học viên tự đăng ký ca thi OPEN còn chỗ do giáo vụ tạo và phân công giáo viên.
4. Thi trực tiếp; giáo viên nhập điểm tổng 0–100. Hệ thống tính Band.
5. Có thể đăng ký lượt hai sau khi lượt một đã có điểm và trước khi xác nhận
   nhập học. Band lấy từ điểm cao nhất của các lượt COMPLETED.
6. Khi đang BAND_ASSIGNED, học viên xác nhận nhập học, chuyển CONFIRMED.
7. Tuyển sinh duyệt hồ sơ CONFIRMED có Band hợp lệ: chuyển ADMITTED và tạo
   đúng một Invoice trong cùng transaction.
8. Học viên trả tiền ngoài hệ thống. Kế toán đối chiếu, tạo đúng một Payment
   bằng số tiền hóa đơn và chuyển Invoice sang PAID trong cùng transaction.
9. Giáo vụ xếp học viên vào lớp OPEN, đúng khóa và Band, còn chỗ; chỉ hồ sơ
   ADMITTED có hóa đơn PAID được xếp. Không có lớp phù hợp thì tiếp tục chờ.
   Hồ sơ giữ ADMITTED; hiển thị đã xếp lớp từ StudentClassAssignment (ADR-023).
10. Giáo viên điểm danh từng buổi LESSON, nhập điểm theo cấu hình chương trình
    khi đang dạy; chỉ gửi bảng điểm đầy đủ cả lớp khi lớp COMPLETED. Giáo vụ
    trả lại kèm lý do hoặc chốt/công bố cả lớp trong một thao tác (ADR-034).

Giáo vụ có thể chuẩn bị chương trình, lớp và lịch trước hoặc song song với
tuyển sinh. Giám đốc xem báo cáo; Admin quản trị tài khoản và nhật ký.

## 4. Quy tắc đã chốt

### Tuyển sinh và thi đầu vào

- Học viên tự tạo tài khoản, đăng ký khóa và ca thi. Nhân viên tư vấn/hướng dẫn
  tại quầy không là nghiệp vụ đăng ký riêng; vẫn dùng luồng và quyền Học viên.
  Không có luồng/API/menu tuyển sinh ghi danh hoặc đăng ký thi thay học viên.
- Mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng khóa. Có thể đăng ký
  các khóa khác đồng thời và học lại khóa cũ sau khi hoàn thành. ADR-022:
  hoàn thành khi lớp được xếp COMPLETED và bảng điểm của học viên FINALIZED,
  không phụ thuộc đạt môn. CANCELLED/REJECTED cũng là hồ sơ đã kết thúc để
  được tạo hồ sơ mới; giữ nguyên hồ sơ và lịch sử cũ. ADMITTED chưa phải hoàn
  thành. Cách biểu diễn/ràng buộc database còn Q-005, không tự thêm trạng thái.
- Học viên được sửa thông tin cá nhân của chính mình; tuyển sinh chỉ kiểm tra,
  không sửa thay. Sau xác nhận nhập học không được đổi khóa đã chọn trên hồ sơ.
  Học viên được sửa họ tên, ngày sinh, số điện thoại và địa chỉ; không tự đổi
  email đăng nhập trong phạm vi hiện tại. Số điện thoại mới hợp lệ, không trùng.
  Không đóng băng toàn bộ thông tin cá nhân sau CONFIRMED.
- Họ tên bỏ khoảng trắng đầu/cuối, 1–100 ký tự sau xử lý, cho phép Unicode/dấu
  tiếng Việt và dấu câu dùng trong tên; không bắt hai từ hoặc chỉ chữ ASCII
  (ADR-073). Áp khi nhập tên tài khoản, không thêm quyền sửa nhân sự.
- Ngày sinh học viên phải là ngày có thật và không sau ngày hiện tại theo giờ
  Việt Nam; không giới hạn tuổi tuyển sinh hoặc bắt nhân sự có ngày sinh (ADR-074).
- SĐT liên hệ giới hạn theo định dạng di động Việt Nam đã chọn: 10 chữ số bắt
  đầu 0; nhận dạng +84 tương đương/khoảng trắng/dấu gạch, chuẩn hóa trước so
  trùng toàn tài khoản khi có SĐT (ADR-076). Không hỗ trợ số nước khác/điện thoại
  bàn dưới quy tắc này, không OTP/xác minh chủ số/danh mục đầu số nhà mạng.
  Đây là giới hạn project, không là mô tả mọi số điện thoại Việt Nam; không
  tự bắt nhân sự nhập SĐT. Địa chỉ học viên tùy chọn, bỏ khoảng trắng đầu/cuối,
  tối đa 255 ký tự Unicode/nhập tự do, không bắt chọn tỉnh/huyện (ADR-077).
- Không sửa khóa trên Registration hiện có, kể cả trước xác nhận (ADR-038).
  Học viên chỉ hủy hồ sơ của mình khi ADR-022/031 cho phép rồi đăng ký khóa mới;
  giữ lịch sử cũ, hồ sơ mới thi đầu vào lại, không chuyển điểm/Band.
  Không mở quyền hủy sau CONFIRMED hoặc hủy hồ sơ đã PAID/hoàn tiền.
- Không snapshot thông tin cá nhân khi đăng ký; hồ sơ hiện tại/lịch sử hiển thị
  họ tên/ngày sinh/SĐT/địa chỉ hiện tại của học viên (ADR-039). Sửa tên cũng đổi
  tên hiển thị trên hồ sơ cũ. Giữ lịch sử nghiệp vụ/audit và snapshot học phí
  hóa đơn; không mở thêm quyền sửa thông tin hoặc đổi email đăng nhập.
- Tuyển sinh được từ chối hồ sơ đang CONFIRMED, chuyển REJECTED; bắt buộc có
  lý do và học viên được xem lý do trên hồ sơ của mình. Không áp dụng thao tác
  từ chối cho hồ sơ đã duyệt hoặc đã thu tiền. Từ chối không là hủy/hoàn tiền.
- Học viên được hủy hồ sơ của mình trước CONFIRMED, chuyển CANCELLED và giữ
  lịch sử. Hủy hồ sơ khác với chỉ hủy một lần đặt ca thi.
- Tuyển sinh được hủy hồ sơ CONFIRMED hoặc ADMITTED có Invoice UNPAID,
  bắt buộc nhập lý do. Nếu đã có hóa đơn UNPAID thì hủy cùng hồ sơ trong một
  transaction. Không có luồng hủy hồ sơ đã thanh toán hoặc hoàn tiền trong
  phạm vi này. Giữ lịch sử xác nhận/duyệt; không xóa hay mở lại hồ sơ cũ.
- ADR-031: khi hủy hồ sơ hợp lệ theo ADR-022, hệ thống hủy các đặt chỗ đang
  hoạt động trong ca chưa bắt đầu và giải phóng chỗ, kể cả ca CLOSED. Màn hình
  xác nhận liệt kê đặt chỗ bị ảnh hưởng. Giữ trạng thái ca và đặt chỗ học viên
  khác; học viên vẫn không có quyền hủy riêng đặt chỗ CLOSED theo ADR-030.
- Với ca đã bắt đầu, giữ lịch sử đặt ca/lượt thi, điểm, kết quả COMPLETED/ABSENT
  và Band hiện có. Không hủy lượt đã thi chưa chấm, xóa điểm hoặc tự ghi 0/ABSENT.
  Sau khi hồ sơ CANCELLED, dừng nhập/sửa điểm và ghi vắng trên hồ sơ đó, kể cả
  ca CLOSED. Hiển thị lịch sử chưa chấm là "Hồ sơ đã hủy — chưa có kết quả",
  không đưa vào danh sách cần chấm; đây không là trạng thái lượt thi mới.
- Hồ sơ đã hủy không được đặt ca tiếp. Nếu đăng ký lại, tạo Registration mới
  và thi từ đầu; không chuyển điểm/Band cũ sang hồ sơ mới (ADR-031).
- Lưu người hủy, thời gian, trạng thái trước hủy và lý do theo quyền đã chốt;
  liên kết việc hủy đặt chỗ phụ thuộc với hành động hủy hồ sơ. Hủy hồ sơ,
  hóa đơn UNPAID đã phát hành và đặt chỗ tương lai/giải phóng chỗ trong cùng
  transaction; một phần thất bại thì không ghi nhận phần nào. Kiểm soát đồng
  thời với đặt ca, ca bắt đầu, chấm điểm, xác nhận/duyệt và thanh toán.
- Giới hạn hai lượt thi tính theo **mỗi hồ sơ**, không theo toàn bộ học viên.
- Lượt hai chỉ được đăng ký khi lượt một đã có điểm và hồ sơ chưa CONFIRMED.
- Không cho đăng ký thi lại sau xác nhận nhập học, duyệt hồ sơ hoặc xếp lớp.
- Học viên được hủy/đổi đặt chỗ khi ca hiện tại OPEN, trước giờ bắt đầu ca
  và trước xác nhận nhập học (ADR-021/ADR-030);
  không tính thêm lượt thi và phải giữ lịch sử đặt ca. Ca mới vẫn phải OPEN,
  còn chỗ và đáp ứng các điều kiện đăng ký đã chốt.
- Vắng thi không có điểm, không ghi thành 0. Được đặt ca khác để hoàn thành
  chính lượt chưa hoàn thành đó, không tạo lượt mới; lượt hai vẫn cần điểm
  lượt một. Phân biệt lượt thi với từng lần đặt ca/hủy/đổi/vắng.
- ADR-027: giáo viên chấm được phân công ghi nhận ABSENT sau khi ca kết thúc
  cho học viên được xác nhận không dự thi. Không tự suy ra vắng từ việc thiếu
  điểm; không ghi điểm 0. Học viên đặt lại cho cùng lượt chưa hoàn thành.
- Giáo vụ chỉ hủy ca chưa bị hủy trước giờ bắt đầu, bắt buộc lý do. Chuyển ca
  CANCELLED và hủy các lượt đặt đang hoạt động liên quan, giữ lịch sử; các lượt
  thi chưa hoàn thành được đặt lại mà không tăng số lượt. Học viên tự chọn ca
  OPEN khác còn chỗ, vẫn theo điều kiện hồ sơ/lượt thi đã chốt. Không tự chuyển
  hoặc cho nhân viên đặt thay. Ca CANCELLED không tổ chức thi hoặc nhập điểm.
- Giáo vụ chỉ sửa ngày/giờ/phòng ca trước giờ bắt đầu và khi chưa từng có đặt
  ca; vẫn kiểm tra xung đột tài nguyên. Đã có đặt ca thì hủy có lý do và tạo ca
  mới, không ghi đè lịch hẹn cũ. Không hủy/đổi ca sau khi bắt đầu hoặc xóa điểm
  đã chấm. Đóng/mở ca và đổi giáo viên chấm theo ADR-030 bên dưới.
- ADR-030: ca mới tạo CLOSED; chỉ giáo vụ mở/mở lại trước giờ bắt đầu khi
  ngày/giờ/phòng hợp lệ, có giáo viên Teacher ACTIVE, không trùng tài nguyên
  và số đặt chỗ hoạt động không vượt sức chứa 1–20. Chỉ OPEN chưa bắt đầu,
  còn chỗ mới nhận đăng ký. Đủ chỗ vẫn OPEN, hiển thị hết chỗ; hủy hợp lệ
  giải phóng chỗ, không cần tự đổi trạng thái theo sĩ số.
- Giáo vụ được đóng OPEN trước giờ bắt đầu, giữ nguyên đặt chỗ. Ca CLOSED:
  học viên chỉ xem lịch/kết quả của mình, không đăng ký thêm, hủy đặt chỗ hoặc
  đổi sang ca khác. Muốn hủy/đổi, ca hiện tại phải được mở lại hợp lệ trước
  giờ bắt đầu; ca đích vẫn phải OPEN, còn chỗ. Đóng ca không hủy đặt chỗ/lượt.
- Đến giờ bắt đầu, ca chưa hủy được xem là CLOSED và không thể mở lại;
  kiểm tra thời gian cả khi trạng thái lưu chưa cập nhật. CLOSED vẫn cho phép
  giáo viên được phân công nhập điểm, ghi vắng sau kết thúc theo ADR-027 và
  sửa điểm trong giới hạn ADR-029. Ca CANCELLED không mở lại hoặc chấm điểm.
- Chỉ giáo vụ được đổi giáo viên chấm của ca chưa hủy trước giờ bắt đầu và
  trước mọi điểm/ghi nhận ABSENT; bắt buộc lý do, lưu giáo viên cũ/mới, người
  thao tác và thời gian. Giáo viên mới phải Teacher ACTIVE, không trùng lịch
  lớp/ca thi. Giữ đặt chỗ; không đổi sau bắt đầu hoặc có kết quả (ADR-030).
- Không cho xác nhận nhập học khi lượt hai đang được đặt lịch hoặc đã thi
  nhưng chờ chấm. Phải chờ kết quả, hoặc hủy ca chưa bắt đầu theo quy tắc trên
  rồi xác nhận bằng kết quả hiện có. Không hủy lượt đã thi chưa chấm để né chờ.
  Lượt hai đang đặt trong ca CLOSED vẫn chặn xác nhận; học viên chỉ được hủy
  sau khi ca được mở lại hợp lệ trước giờ bắt đầu (ADR-030).
  Hủy cả hồ sơ theo ADR-031 kết thúc hồ sơ đó; không xác nhận hoặc mở lại hồ
  sơ cũ. Điều kiện chặn lượt hai đang chờ vẫn áp dụng cho hồ sơ tiếp tục nhập học.
- Ca thi có sức chứa 1–20; chỉ OPEN nhận đăng ký. Thi trực tiếp, không chấm
  bài tự động; hệ thống chỉ tự xác định Band từ điểm giáo viên nhập.
- Điểm thi lưu và hiển thị theo thang 100; chọn điểm cao nhất của lượt COMPLETED.
- Band chỉ lưu ngưỡng tối thiểu; chọn ngưỡng lớn nhất không vượt quá điểm.
  Các ngưỡng đã có: Band 1 = 0, Band 2 = 25, Band 3 = 45, Band 4 = 65,
  Band 5 = 85. Ngưỡng là dữ liệu seed, không viết cứng trong logic.
- ADR-028: giữ các ngưỡng seed 0/25/45/65/85 và khóa toàn bộ bộ ngưỡng sau
  kết quả thi đầu vào COMPLETED đầu tiên của hệ thống. Không tính lại lịch sử
  xếp Band từ thay đổi cấu hình sau đó, không thêm module version ngưỡng.
  Lượt hai hợp lệ vẫn cập nhật điểm cao nhất/Band trước CONFIRMED bằng cùng
  bộ ngưỡng. So sánh Band bằng điểm đầu vào hợp lệ, không làm tròn số nguyên.
- ADR-029: chỉ giáo viên đã nhập điểm, vẫn được phân công ca đó, được sửa điểm
  đầu vào của lượt COMPLETED khi hồ sơ TESTING hoặc BAND_ASSIGNED. Bắt buộc
  lý do, lưu điểm cũ/mới, người sửa và thời gian; tính lại điểm cao nhất/Band
  trong cùng transaction bằng bộ ngưỡng đã khóa. Giữ các lượt thi, không tăng
  lượt. Điểm sửa vẫn 0–100, tối đa hai chữ số thập phân.
- Khóa điểm đầu vào và Band từ CONFIRMED trở đi; không sửa hồ sơ đã hủy hoặc
  từ chối. Không thêm quyền sửa ngoại lệ cho Admin/giáo vụ. Đây không là khóa
  thông tin cá nhân được phép sửa. Kiểm soát sửa điểm đồng thời với xác nhận/hủy.
- Nếu hai lượt COMPLETED cùng điểm cao nhất, chọn lượt có attempt_number nhỏ
  hơn (lượt một) làm best_attempt_id; vẫn giữ và hiển thị cả hai kết quả.
  Đồng điểm không tạo lượt thi mới hoặc đổi cách xác định Band (ADR-029).
- Hồ sơ học viên bắt buộc họ tên, ngày sinh, email và số điện thoại; địa chỉ
  không bắt buộc. Registration có khóa chọn. Không lấy thêm trường bắt buộc
  từ biểu mẫu minh họa hoặc áp yêu cầu ngày sinh học viên cho nhân sự.

### Đào tạo và điểm

- Tên khóa/chương trình bỏ khoảng trắng đầu/cuối, 1–150 ký tự Unicode; tên
  thành phần điểm 1–100. Không ép tên khóa/chương trình duy nhất toàn hệ thống;
  giữ tên thành phần duy nhất trong một chương trình/khóa cấu hình đã chốt
  (ADR-078). Không tự cắt ngắn dữ liệu hoặc mở thêm quyền sửa.
- Mã khóa/lớp do Giáo vụ nhập, bỏ khoảng trắng đầu/cuối và chuyển viết hoa;
  1–30 ký tự A–Z, số, `_`, `-`, không khoảng trắng/dấu tiếng Việt. Duy nhất trong
  từng loại kể cả khóa ngừng/lớp hủy, không đổi sau tạo và không tái dùng mã
  lịch sử (ADR-079). Không quyết định cách sinh ID/số hóa đơn qua quy tắc này.
- Mã phòng 1–30 ký tự cùng định dạng trên, chuẩn hóa chung cho lịch lớp/ca thi;
  p101/P101 là cùng nhãn phòng. Giáo vụ dùng một mã thống nhất cho mỗi phòng,
  không tự gộp bí danh khác nhau/không thêm module-danh mục phòng (ADR-080).
  Giữ kiểm tra trùng tài nguyên và giới hạn sửa lịch đã chốt.
- Lý do/nhận xét/chú thích dùng Unicode/văn bản thuần, bỏ khoảng trắng đầu/
  cuối, giữ xuống dòng. Lý do 1–1000 ký tự chỉ nơi nghiệp vụ đang bắt buộc;
  nhận xét giáo viên/chú thích sửa tùy chọn, tối đa 1000; không editor HTML/
  rich text hoặc thêm thao tác bắt lý do/nhận xét mới (ADR-081). Giữ quyền xem/
  sửa, khóa điểm và audit đã chốt; không cắt ngắn dữ liệu.
- ADR-025: giáo vụ chuyển PLANNED → OPEN khi chương trình ACTIVE, đã có
  giáo viên và ít nhất một buổi LESSON tương lai không trùng lịch. Không yêu cầu
  lịch đầy đủ để mở lớp; trước khi bắt đầu dạy phải đủ lịch theo ADR-032.
- Giáo vụ chuyển OPEN → ONGOING khi thực tế bắt đầu dạy, không trước buổi
  LESSON đầu tiên và có ít nhất một học viên được xếp hợp lệ. Không yêu cầu
  sĩ số tối thiểu cao hơn một, không tự chuyển chỉ vì đến ngày. ADR-032 bổ sung:
  phải có lịch đủ số LESSON dự kiến trước OPEN → ONGOING; FINAL_EXAM tính riêng,
  không thay thế LESSON.
- Giáo vụ chuyển ONGOING → COMPLETED sau xác nhận mọi buổi LESSON/FINAL_EXAM
  còn cần tổ chức đã diễn ra và mỗi buổi LESSON đủ điểm danh cho học viên được
  xếp. ADR-036: buổi cũ đã ghi nhận không tổ chức được giữ làm lịch sử;
  buổi thay thế phải diễn ra/đủ điểm danh thay cho buổi cũ, không ghi điểm
  danh giả cho lịch cũ hoặc đếm cả hai là hai LESSON.
  Không yêu cầu điểm đã gửi/chốt để kết thúc lớp; gửi/chốt diễn ra sau đó.
  Không tự hoàn thành chỉ vì giờ dự kiến đã qua. Hủy lớp theo ADR-026 bên dưới.
- Giáo vụ chỉ hủy lớp PLANNED/OPEN chưa có học viên được xếp và chưa bắt đầu
  dạy, bắt buộc lý do; chuyển CANCELLED, không xóa vật lý. Không hủy lớp đã
  có học viên, đã bắt đầu dạy hoặc ONGOING/COMPLETED. Không thêm luồng chuyển
  lớp/hoàn tiền qua thao tác hủy; hồ sơ đã trả tiền đang chờ xếp không bị tác động.
- Giữ lịch sử lớp và lịch của lớp CANCELLED; lịch này không giữ chỗ phòng/
  giáo viên, không xuất hiện trong lịch dạy sắp tới và không tính là buổi hoàn
  thành (ADR-026). Cách biểu diễn dữ liệu/trạng thái lịch sẽ chốt khi thiết kế.
- Một lớp có đúng một giáo viên, sức chứa 1–15. Không lưu lịch sử đổi giáo viên
  trong phạm vi đã định. ADR-023: giáo vụ chỉ đổi giáo viên trước ONGOING và
  trước khi có điểm danh/điểm; phải kiểm tra toàn bộ lịch lớp với lịch lớp/ca
  thi của giáo viên mới. Không đổi người phụ trách khi đã bắt đầu giảng dạy.
- Chỉ xếp học viên vào lớp OPEN, đúng khóa/Band và còn chỗ; hồ sơ ADMITTED,
  hóa đơn PAID. Không nhận thêm vào PLANNED/ONGOING/COMPLETED/CANCELLED và
  không có luồng nhập học muộn. Xếp lớp không đổi trạng thái Registration;
  hiển thị "Đã xếp lớp" dựa trên StudentClassAssignment, không thêm trạng thái.
- Không triển khai chuyển lớp sau khi đã xếp, kể cả trước bắt đầu dạy (ADR-040).
  Không thêm luồng gỡ/xếp lại hoặc dùng hủy hồ sơ/lớp để chuyển học viên.
- Không chồng lấn lịch của cùng lớp, phòng hoặc giáo viên; kiểm tra cả ca thi
  đầu vào với lịch lớp. Việc kiểm tra chỉ lúc tạo lớp chưa đủ nếu lịch đổi.
- Giáo vụ có thể tạo lịch hàng loạt từ khoảng ngày, các thứ trong tuần, giờ và
  phòng; hệ thống sinh các buổi có ngày cụ thể. Có thể thêm hoặc điều chỉnh
  từng buổi và phải kiểm tra xung đột trước khi lưu. Không tự tối ưu xếp lịch.
  ADR-032: giáo vụ chỉ sửa ngày/giờ/phòng buổi chưa bắt đầu, chưa có điểm danh
  hoặc điểm liên quan, khi lớp chưa COMPLETED/CANCELLED. Buổi đã bắt đầu hoặc
  có dữ liệu phụ thuộc giữ nguyên để bảo toàn lịch sử; vẫn kiểm tra xung đột.
- Số buổi dự kiến thuộc TrainingProgram theo khóa/Band, là số nguyên dương,
  chỉ tính LESSON, không tính FINAL_EXAM. Khóa cấu hình số buổi khi lớp đầu tiên
  sử dụng chương trình, kể cả lớp sau đó bị hủy (ADR-032). Chi tiết trường dữ
  liệu sẽ chốt ở thiết kế database, không tự thêm schema/version.
- ADR-035: buổi cả lớp không tổ chức không tính là đã dạy; không ghi tất cả
  học viên vắng để coi buổi đã diễn ra. Xử lý trong quản lý lịch hiện có, giữ
  số LESSON chương trình, giới hạn sửa lịch ADR-032 và điều kiện hoàn thành
  ADR-025/036. Không cho thêm LESSON ngoài số buổi chương trình (ADR-037).
- ADR-036: trước giờ LESSON bắt đầu, giáo vụ dời ngày/giờ/phòng buổi hiện có
  theo ADR-032, kiểm tra trùng lịch; vẫn một LESSON, không tăng số buổi.
  Khi đã đến/qua giờ dự kiến mà buổi cả lớp không tổ chức, giáo vụ ghi nhận
  không tổ chức với lý do bắt buộc, giữ lịch cũ/người thao tác/thời gian và
  tạo LESSON thay thế có liên kết cùng lớp. Chỉ lớp chưa COMPLETED/CANCELLED,
  buổi cũ chưa có điểm danh/điểm liên quan; có dữ liệu thì chặn, không tự xóa.
  Buổi thay thế có lịch tương lai hợp lệ, dùng giáo viên hiện tại của lớp,
  kiểm tra trùng lớp/phòng/giáo viên với lịch lớp/ca thi. Ghi nhận không tổ chức
  và tạo buổi thay thế phải nhất quán/toàn bộ hoặc không phần nào, chống thao
  tác lặp và cập nhật đồng thời. Buổi thay thế thực hiện nghĩa vụ của buổi cũ,
  không cộng thành hai LESSON; không thay đổi số buổi chương trình. Biểu diễn
  dữ liệu/lịch sử/audit còn bước database; không tự thêm enum/schema.
- ADR-037: tạo lịch tuần/thêm từng LESSON chỉ để đủ số buổi chương trình,
  không vượt số nghĩa vụ LESSON đã cấu hình; trước bắt đầu dạy phải đủ số
  theo ADR-032. Không có buổi bổ sung/ôn tập/tự chọn ngoài số này, không sửa
  số buổi chương trình đã khóa để né giới hạn. Dời/thay buổi theo ADR-036
  vẫn được phép, cặp lịch cũ/thay thế chỉ một nghĩa vụ; không áp giới hạn
  lên tổng bản ghi lịch sử. FINAL_EXAM riêng, không thay thế LESSON.
- Trọng số gắn với **TrainingProgram theo khóa và Band**, không gắn trực tiếp
  với Course. Tổng trọng số phải bằng 100 trước khi chương trình ACTIVE.
- ADR-028: khóa liên kết khóa/Band, danh sách thành phần điểm và trọng số
  của TrainingProgram ngay khi có lớp đầu tiên tham chiếu, kể cả lớp sau đó
  bị hủy. Trước khi có lớp, giáo vụ được cấu hình; ACTIVE vẫn cần tổng 100.
  Không thêm module version trong phạm vi hiện tại. Muốn đổi cấu hình đã dùng
  phải chốt riêng thiết kế version/snapshot và ràng buộc duy nhất khóa/Band;
  không tự tạo chương trình trùng cặp này để né khóa.
- Điểm thành phần, điểm tổng kết và giao diện dùng **thang 100**.
- Công thức tổng kết: tổng của `score_value * weight / 100` trên toàn bộ
  thành phần cần có. Chưa đủ điểm thì không được gửi bảng điểm.
- Điểm đầu vào, điểm thành phần và trọng số nhận tối đa hai chữ số thập phân;
  từ chối độ chính xác vượt mức, không tự làm tròn dữ liệu nhập (ADR-028).
  Tính tổng bằng số thập phân, không làm tròn từng đóng góp; chỉ làm tròn tổng
  một lần đến hai chữ số theo half-up (đúng giữa thì làm tròn lên với số dương)
  để lưu, hiển thị và so ngưỡng đạt. Khi FINALIZED: 49,995 → 50,00 đạt;
  49,994 → 49,99 không đạt. Các ví dụ là tổng tính toán, không là điểm nhập.
- Bảng điểm có luồng DRAFT → SUBMITTED → FINALIZED hoặc RETURNED → SUBMITTED.
  RETURNED phải có lý do cho giáo viên; FINALIZED công bố và không được sửa.
- ADR-024: giáo viên được nhập điểm nháp khi đang dạy; chỉ gửi khi lớp
  COMPLETED và đủ điểm thành phần bắt buộc. Giáo vụ chỉ chốt bảng điểm
  SUBMITTED thuộc lớp COMPLETED sau kiểm tra.
- ADR-034: gửi/gửi lại, giáo vụ trả lại hoặc chốt phải xử lý toàn bộ lớp trong
  một thao tác, không chọn riêng học viên. Gửi chỉ khi mọi học viên trong lớp
  đủ điểm thành phần bắt buộc và bảng điểm DRAFT/RETURNED; trả lại/chốt chỉ
  khi toàn bộ bảng điểm SUBMITTED. Một phần không hợp lệ/thất bại thì không
  chuyển phần nào. Chốt công bố đồng thời cả lớp; trả lại cả lớp có lý do,
  giữ điểm/chú thích hiện có. Vẫn sửa từng giá trị nháp/cần sửa, rồi gửi lại
  toàn lớp; không giới hạn chỉ được gửi một lần trong suốt vòng đời.
- Giáo viên chỉ sửa điểm thành phần/chú thích khi bảng điểm của học viên
  DRAFT hoặc RETURNED. SUBMITTED bị đóng sửa, muốn sửa phải được giáo vụ
  trả lại kèm lý do; FINALIZED bất biến. Không tự thêm quyền sửa điểm đã chốt.
- Đạt môn khi điểm tổng kết đã làm tròn theo ADR-028, FINALIZED >= 50/100;
  dưới 50 là không đạt.
  Không thêm điều kiện chuyên cần, điểm tối thiểu từng thành phần hoặc mức
  xếp loại khác. Chưa chốt điểm không được coi là không đạt. Quy tắc hoàn
  thành/đăng ký lại của ADR-022 không phụ thuộc đạt môn.
- Điểm danh có mặt/vắng (present 1/0), một bản ghi/học viên/buổi LESSON.
- ADR-033: giáo viên phụ trách được nhập/sửa điểm danh từ lúc buổi LESSON bắt
  đầu đến trước khi lớp chuyển
  COMPLETED, không áp hạn cố định 24/48 giờ sau từng buổi. Khi lớp COMPLETED,
  chỉ xem điểm danh, dù bảng điểm chưa gửi/chốt. Vẫn phải đủ điểm danh LESSON
  trước khi hoàn thành lớp theo ADR-025; kiểm soát nhập/sửa đồng thời với hoàn
  thành lớp. Không thêm quyền mở lại/sửa ngoại lệ. Chặn điểm danh trước thời
  điểm bắt đầu buổi theo lịch hiện hành; không cần chờ buổi kết thúc mới nhập.
  Điểm danh từ lúc bắt đầu không tự xác nhận buổi đã diễn ra đầy đủ hoặc hoàn
  thành lớp; biểu diễn buổi thực tế vẫn thuộc Q-008.

### Học phí, tài khoản và nhật ký

- File xuất báo cáo dùng CSV; không yêu cầu XLSX. Quyết định này không tự thêm
  quyền xuất file cho vai trò chỉ xem.
- Học viên được in hóa đơn của mình qua trình duyệt (ADR-041), đúng số tiền và
  trạng thái thanh toán; UNPAID không được thể hiện như đã trả tiền. Không xuất/
  tải PDF riêng từ ứng dụng hoặc phát hành biên lai riêng; không tự thêm quyền
  in/xuất cho nhân sự. Lưu PDF bằng chức năng in của trình duyệt không là API PDF.
- Mỗi hồ sơ được duyệt có một hóa đơn; amount là học phí khóa tại thời điểm
  phát hành. Xác nhận thanh toán không tạo lại hóa đơn.
- Học phí khóa phải lớn hơn 0; không có khóa miễn phí, hóa đơn/Payment 0 đồng
  hoặc luồng tự bỏ qua thanh toán (ADR-043).
- Học phí, hóa đơn, thanh toán và tổng tài chính dùng VND nguyên đồng; không
  nhận phần lẻ của đồng, không tự làm tròn tiền nhập (ADR-044). Không đổi độ
  chính xác điểm/trọng số ADR-028; kiểu/range database còn bước thiết kế dữ liệu.
- Hạn thu = ngày phát hành hóa đơn theo giờ Việt Nam +7 ngày lịch, kể cả cuối
  tuần/ngày nghỉ (ADR-045). Không thêm thao tác gia hạn/sửa hạn; đến hết ngày
  hạn chưa quá hạn, từ ngày kế tiếp nếu UNPAID mới cảnh báo quá hạn.
- Thanh toán đủ một lần bằng tiền mặt/chuyển khoản bên ngoài ứng dụng.
  Payment.amount_paid bằng Invoice.amount; chuyển khoản phải có mã giao dịch.
- Mã giao dịch bỏ khoảng trắng đầu/cuối, tối đa 100 ký tự, giữ nội dung/hoa-
  thường/dấu câu còn lại; không mẫu theo ngân hàng hoặc ép duy nhất toàn hệ
  thống (ADR-082). Chuyển khoản bắt buộc có mã, tiền mặt có thể bỏ trống. Giữ
  một Payment/hóa đơn; mã giao dịch không tự là bằng chứng thu tiền hay khóa
  chống ghi lặp, không thêm ngân hàng/tích hợp/quyền ghi đè.
- paid_at là thời điểm xác nhận thanh toán; không lưu ngày/giờ thu thực tế riêng
  hoặc cho nhập lùi ngày xác nhận (ADR-046). Doanh thu vẫn theo ngày xác nhận;
  sửa phương thức/mã giao dịch không thay ngày này.
- Kế toán chỉ được sửa phương thức/mã giao dịch của Payment đã xác nhận, bắt
  buộc lý do và audit giá trị cũ/mới, người/thời gian (ADR-042). Số tiền, hóa đơn
  liên kết và ngày/giờ xác nhận không đổi; giữ cùng Payment và Invoice PAID.
  Chuyển khoản vẫn cần mã giao dịch; sửa và audit toàn bộ hoặc không phần nào,
  kiểm soát cập nhật đồng thời, không tạo Payment thứ hai. Không hủy/xóa Payment,
  hoàn tiền hoặc mở quyền ngoại lệ cho tuyển sinh/Admin.
- Quá hạn được tính từ Invoice UNPAID và due_date trước ngày hiện tại.
  Chỉ hiển thị cảnh báo và số ngày quá hạn; không tự hủy hồ sơ, khóa tài khoản
  hoặc thay đổi trạng thái học viên vì nợ quá 15 ngày.
- Không có module học bổng hoặc tính năng giảm học phí từ học bổng.
- Học viên tự đăng ký; nhân sự do Admin cấp. Không có tự phục vụ quên mật khẩu;
  Admin đặt lại mật khẩu. Đăng xuất nằm trong khung giao diện chung.
- Khi cấp tài khoản nhân sự hoặc Admin reset mật khẩu, sinh mật khẩu tạm ngẫu
  nhiên, chỉ hiển thị một lần cho Admin giao trực tiếp; bắt buộc người dùng đổi
  trước khi truy cập nghiệp vụ (ADR-048). Không ghi mật khẩu vào log/audit, chỉ
  lưu hash; reset không tự mở LOCKED hoặc khôi phục DISABLED. Học viên tự đăng
  ký vẫn tự đặt mật khẩu; không thêm email khôi phục hay mật khẩu vĩnh viễn do Admin chọn.
- Người dùng đã đăng nhập được tự đổi mật khẩu, bắt buộc nhập đúng mật khẩu
  hiện tại. Đây không phải chức năng quên mật khẩu; không khôi phục qua email.
- Khóa thủ công/ngừng dùng tài khoản, Admin reset hoặc tự đổi mật khẩu hủy mọi
  phiên đang tồn tại của tài khoản, kể cả phiên hiện tại/token chưa hết hạn
  (ADR-049). Phải đăng nhập lại theo trạng thái tài khoản và yêu cầu đổi mật
  khẩu tạm; mở khóa/khôi phục không làm phiên cũ hợp lệ lại.
- Admin không được khóa/vô hiệu hóa tài khoản của chính mình hoặc Admin hoạt
  động cuối cùng. Phải giữ ràng buộc này cả khi thao tác đồng thời.
- Account ACTIVE được truy cập; LOCKED bị chặn và có thể được Admin mở;
  DISABLED là ngừng sử dụng và bị chặn. Cả LOCKED/DISABLED giữ tài khoản cùng
  dữ liệu nghiệp vụ, không xóa. Admin được khôi phục DISABLED → ACTIVE trên
  cùng tài khoản, giữ vai trò/dữ liệu/lịch sử, không khôi phục phiên cũ (ADR-050).
  Không tạo tài khoản mới hoặc mở lại hồ sơ nghiệp vụ đã kết thúc qua thao tác này.
- Đăng nhập bằng email và mật khẩu, không bằng số điện thoại. Số điện thoại là
  thông tin liên hệ. Email/SĐT phải hợp lệ và không trùng theo ràng buộc đã có;
  việc đổi số liên hệ không thay định danh đăng nhập.
- Email khi tạo tài khoản bỏ khoảng trắng đầu/cuối, đúng định dạng và tối đa
  254 ký tự; so trùng không phân biệt hoa/thường trên toàn tài khoản (ADR-075).
  Không gộp dấu chấm hoặc +tag theo nhà cung cấp, không xác minh email/khôi phục
  qua email. Bộ kiểm tra chính xác/cách đếm ký tự/lưu chuẩn hóa còn kỹ thuật.
- Email đăng nhập không được sửa trên tài khoản hiện có, kể cả bởi Admin và
  đối với nhân sự (ADR-051). Không dùng xóa/tạo lại tài khoản để né giới hạn.
- Sai mật khẩu 5 lần liên tiếp trong 15 phút thì chặn đăng nhập mới 15 phút;
  đăng nhập thành công hoặc hết hạn đặt lại bộ đếm/cửa sổ (ADR-052). Không đổi
  Account thành LOCKED/DISABLED hoặc hủy phiên đang dùng chỉ vì nhập sai.
  LOCKED thủ công vẫn cần Admin mở; DISABLED không tự khôi phục sau thời hạn.
- Mật khẩu 15–128 ký tự, cho phép Unicode/khoảng trắng, không bắt buộc phối hợp
  chữ hoa/số/ký tự đặc biệt; chặn mật khẩu phổ biến/đã lộ, không ép đổi định kỳ
  khi chưa có sự cố (ADR-053). Không cắt ngắn mật khẩu; yêu cầu đổi mật khẩu tạm
  trước nghiệp vụ vẫn giữ. Cách đếm ký tự/nguồn danh sách chặn còn thiết kế kỹ thuật,
  không tự thêm dịch vụ kiểm tra mật khẩu bên ngoài.
- Mật khẩu tạm hết hạn sau 24 giờ tính từ lúc cấp/reset; hết hạn cần Admin reset
  lại, không dùng mật khẩu hết hạn để đăng nhập/đổi qua cửa bắt buộc đổi (ADR-054).
  Không tự đổi trạng thái tài khoản, mở khóa/khôi phục hoặc gửi lại qua email.
- Phiên đăng nhập thông thường hết sau 2 giờ từ đăng nhập hoặc 30 phút không
  hoạt động, mốc nào đến trước; hoạt động không kéo dài hạn tuyệt đối (ADR-055).
  Thực thi tại server, phải đăng nhập lại, không refresh/tự gia hạn/ghi nhớ đăng
  nhập. Định nghĩa hoạt động và phiên hạn chế để đổi mật khẩu tạm còn bước kiến trúc.
- Cho phép nhiều thiết bị đăng nhập đồng thời; đăng nhập mới không đẩy phiên
  khác ra (ADR-056). Không thêm màn hình quản lý thiết bị/danh sách phiên.
- Đăng xuất chỉ hủy phiên hiện tại tại server, kể cả token chưa hết hạn; phiên
  khác giữ nguyên theo hạn và quy tắc hủy phiên đã chốt. Không nút đăng xuất
  tất cả (ADR-057). Khóa/ngừng dùng/reset/đổi mật khẩu vẫn hủy mọi phiên ADR-049.
- AuditLog lưu ở database, được ghi từ tầng ứng dụng, không dùng trigger.
  Ghi các thao tác nhạy cảm: duyệt, điểm/Band, thanh toán, xếp lớp, trả lại/chốt
  điểm, khóa/mở tài khoản. Chức năng thường không sửa/xóa nhật ký.
- Giữ mọi audit đã bắt buộc; bổ sung thay đổi cấu hình khóa/chương trình/Band,
  chuyển trạng thái lớp và tạo/dời/thay lịch thành công (ADR-058). Không ghi mọi
  lượt xem trang/đọc thông thường; không thêm luồng lịch sử đổi giáo viên lớp.
- Ghi sự kiện đăng nhập thành công/thất bại, kích hoạt chặn tạm, đăng xuất,
  tự đổi mật khẩu, cấp nhân sự/reset và thao tác trạng thái tài khoản, truy cập
  bị từ chối (ADR-059). Không coi email nhập khi chưa xác thực là người thao
  tác đã được xác minh; cách biểu diễn/chống ngập log/lỗi ghi còn thiết kế kỹ thuật.
- Chỉ Admin tra cứu nhật ký; lọc theo khoảng ngày nghiệp vụ, người thao tác,
  hành động, loại/mã đối tượng và kết quả, phân trang mới nhất trước (ADR-060).
  Không thêm tìm kiếm tự do toàn payload; ngày theo ADR-047.
- Chi tiết nhật ký gồm thời gian/người/thao tác/đối tượng/kết quả-lý do và trường
  thay đổi cần thiết khi áp dụng, giữ audit cũ/mới đã bắt buộc (ADR-061).
  Không lưu mật khẩu/hash mật khẩu/JWT/bí mật phiên hoặc toàn request/response
  vào log; hạn chế dữ liệu cá nhân. Hash mật khẩu chỉ ở nơi lưu thông tin xác
  thực, không ở log. Không bỏ lịch sử bắt buộc hoặc thêm snapshot cá nhân ADR-039.
- Admin chỉ xem/tra cứu nhật ký trong ứng dụng, không xuất CSV/PDF (ADR-062).
  Không sửa/xóa log qua nghiệp vụ thông thường. Thời hạn lưu/xử lý hết hạn và
  backup vận hành còn bước kiến trúc, chưa chốt thời hạn hoặc chức năng dọn log.
- Không xóa vật lý dữ liệu có nghiệp vụ phụ thuộc; dùng trạng thái ngừng sử dụng.

### Báo cáo

- Tuyển sinh: thống kê số Registration theo khóa, ngày đăng ký và trạng thái;
  phân biệt số hồ sơ với số học viên, không dùng hai chỉ số thay thế nhau.
- Báo cáo dùng trạng thái hiện tại lúc tra cứu, không tái dựng trạng thái/công
  nợ tại ngày quá khứ (ADR-063). Lọc ngày chọn tập hồ sơ/sự kiện phù hợp, không
  phục hồi trạng thái cũ; vẫn giữ lịch sử nghiệp vụ/audit và snapshot tài chính.
- Số học viên là student_id duy nhất trong tập Registration đã lọc (ADR-065).
  Trong mỗi khóa tính một lần; tổng toàn báo cáo đếm duy nhất, không cộng nhóm
  khóa. Học lại có thể tăng số hồ sơ nhưng không tăng số học viên duy nhất.
- Tài chính: doanh thu là tổng thanh toán đã xác nhận, lọc theo ngày xác nhận
  thanh toán. Công nợ là tổng amount của Invoice UNPAID; quá hạn là phần công
  nợ có due_date trước ngày hiện tại. Hóa đơn mới phát hành chưa phải doanh thu.
  Công nợ hiện tại và doanh thu trong kỳ có cơ sở thời gian khác nhau, không
  mặc định lấy tổng hóa đơn trong kỳ trừ doanh thu trong kỳ để tính công nợ.
- Tổng phải thu trong kỳ là tổng amount hóa đơn UNPAID + PAID phát hành trong
  kỳ đó, loại CANCELLED theo trạng thái hiện tại (ADR-064). Công nợ UNPAID hiện
  tại tách riêng, không áp kỳ ngày thanh toán; lọc khóa có thể áp nhất quán.
  Tổng phải thu không là doanh thu hoặc công nợ; không thêm báo cáo chi phí/
  lợi nhuận, công nợ tại ngày quá khứ hoặc sổ tổng phát hành trước hủy.
- Đào tạo: số lớp theo trạng thái, sĩ số, tiến độ số buổi và kết quả đã FINALIZED.
  Điều kiện đạt >= 50/100 theo ADR-024/028; chưa chốt không là không đạt.
- Tiến độ LESSON = nghĩa vụ LESSON đã xác nhận thực tế diễn ra / số LESSON
  chương trình (ADR-066). Không tính buổi cũ không tổ chức; thay thế tính một
  lần cho cùng nghĩa vụ. FINAL_EXAM hiển thị riêng, không trong tử/mẫu LESSON.
  Giờ đã qua/điểm danh không tự chứng minh đã diễn ra; biểu diễn còn Q-008.
  Lớp CANCELLED vẫn có trong số lớp theo trạng thái, không tính buổi đã dạy.
- Tỷ lệ đạt = GradeReport FINALIZED đạt / tổng GradeReport FINALIZED trong kỳ
  chốt điểm, trong bộ lọc khóa/lớp đã chọn (ADR-067). Mỗi kết quả lượt học tính
  riêng, không đếm người duy nhất qua học lại. Loại chưa chốt; không có kết quả
  FINALIZED hiển thị "Chưa có dữ liệu", không 0%. Có kết quả nhưng không ai đạt
  mới là 0%; độ chính xác hiển thị tỷ lệ còn thiết kế UI/số.
- Báo cáo tuyển sinh lọc khóa/ngày đăng ký/trạng thái; mặc định tháng Việt Nam
  hiện tại và mọi trạng thái, kể cả CANCELLED/REJECTED. Bộ chọn khóa giữ khóa
  INACTIVE để xem lịch sử (ADR-068); không thêm quyền báo cáo cho Tuyển sinh.
- Hiện trạng/tiến độ đào tạo lọc khóa/Band/lớp/giáo viên hiện được phân công/
  trạng thái lớp, mặc định mọi trạng thái và không lọc kỳ ngày. Kết quả/tỷ lệ
  đạt thêm kỳ chốt điểm, mặc định tháng Việt Nam hiện tại; giữ khóa ngừng có
  dữ liệu lịch sử (ADR-069). Không thêm lịch sử giáo viên hoặc ngày tạo lớp.
- Báo cáo tài chính mặc định kỳ doanh thu/tổng phải thu là tháng Việt Nam hiện
  tại theo ngày xác nhận/phát hành tương ứng. Lọc khóa áp nhất quán; công nợ
  hiện tại có tùy chọn chỉ quá hạn, không chịu bộ lọc kỳ ngày (ADR-070).
- Giám đốc chỉ xem tổng hợp và chi tiết nhóm khóa/Band/lớp khi phù hợp với chỉ
  số; không xem hồ sơ, hóa đơn hoặc thông tin định danh từng học viên qua báo
  cáo. Không xuất file/phê duyệt; quyền tra cứu hóa đơn của Kế toán không đổi
  (ADR-071). Không thêm quyền báo cáo cho Admin/Tuyển sinh/Giáo vụ.
- CSV báo cáo tài chính của Kế toán xuất toàn kết quả theo bộ lọc và cột báo
  cáo, không chỉ trang đang xem/không thêm trường ẩn hoặc dữ liệu hồ sơ cá nhân.
  Không xuất riêng danh sách hồ sơ/hóa đơn/học viên dưới quyền này (ADR-072).
- **Chưa chốt:** cơ chế truy vấn/xuất nhất quán, cấu trúc/giới hạn/mã hóa và an
  toàn CSV, biểu diễn thực tế và hiển thị tỷ lệ; không là quyền triển khai.

## 5. Trạng thái từ thiết kế báo cáo

| Đối tượng | Trạng thái |
|---|---|
| Account | ACTIVE, LOCKED, DISABLED |
| Course | ACTIVE, INACTIVE |
| TrainingProgram | DRAFT, ACTIVE, INACTIVE |
| Registration | PENDING, TESTING, BAND_ASSIGNED, CONFIRMED, ADMITTED, REJECTED, CANCELLED |
| EntryTestSession | OPEN, CLOSED, CANCELLED |
| EntryTestAttempt | SCHEDULED, COMPLETED, ABSENT, CANCELLED |
| Class | PLANNED, OPEN, ONGOING, COMPLETED, CANCELLED |
| GradeReport | DRAFT, SUBMITTED, RETURNED, FINALIZED |
| Invoice | UNPAID, PAID, CANCELLED |

Đây là tập trạng thái thiết kế, không đồng nghĩa mọi chuyển trạng thái đã
được xác định. Hủy hồ sơ/lớp/ca và ghi nhận vắng đã chốt ADR-022/ADR-026/ADR-027;
đóng/mở ca và đổi giáo viên chấm đã chốt ADR-030; chặn đăng nhập tạm ADR-052
không tạo trạng thái tài khoản mới.
Đã xếp lớp được suy ra từ
StudentClassAssignment (ADR-023).
Không tạo trạng thái OVERDUE hoặc APPROVED/REJECTED cho bảng điểm.

## 6. Màn hình và yêu cầu giao diện

| Mã | Nhóm | Màn hình |
|---|---|---|
| F01 | Chung | Đăng nhập |
| F02–F05 | Học viên | Đăng ký tài khoản; trang chủ; khóa học; đăng ký khóa |
| F06–F09 | Học viên | Đăng ký thi/thi lại; lịch ca thi; kết quả/Band; xác nhận nhập học |
| F10–F12 | Học viên | Hóa đơn/hướng dẫn thanh toán; lớp/lịch; bảng điểm |
| F13–F14 | Tuyển sinh | Danh sách hồ sơ; chi tiết/duyệt |
| F15 | Đã loại bỏ | Không triển khai màn hình ghi danh trực tiếp cho tuyển sinh; giữ mã để truy vết. |
| F16–F18 | Giáo vụ | Danh sách ca; tổ chức ca/phân công chấm; khóa/chương trình/trọng số |
| F19–F23 | Giáo vụ | Danh sách lớp; tạo/cập nhật lớp; lịch; xếp lớp; trả lại/chốt bảng điểm |
| F24–F27 | Giáo viên | Lớp/lịch phụ trách; điểm đầu vào; điểm danh; sổ điểm/gửi bảng điểm |
| F28–F30 | Kế toán | Hóa đơn; xác nhận thanh toán; báo cáo học phí |
| F31 | Giám đốc | Báo cáo tổng hợp |
| F32–F33 | Admin | Tài khoản/đặt lại mật khẩu/khóa mở/ngừng sử dụng; nhật ký |

UI tiếng Việt, responsive 375/768/1280 px; ngày dd/MM/yyyy; tiền VND nguyên đồng.
Múi giờ nghiệp vụ thống nhất Asia/Ho_Chi_Minh (UTC+7) cho lịch học/thi, ngày
phát hành/hạn thu/quá hạn và lọc báo cáo theo ngày (ADR-047). Múi giờ thiết bị
không làm đổi ranh giới ngày nghiệp vụ; lưu/serialize thời gian còn bước kiến trúc.
Đổi mật khẩu là thao tác chung cho người dùng đã đăng nhập; vị trí giao diện
sẽ chốt khi thiết kế UI, chưa tự thêm mã màn hình mới.
Loading, empty, error và submitting phải được xử lý. Role switcher trong
mockup là công cụ preview, không là cơ chế đổi vai trò của tài khoản thật.
Mockup và biểu mẫu không chứng minh chức năng đã code hoặc kết nối database.

## 7. Các điểm chưa chốt

Chi tiết và câu hỏi nằm tại `docs/plan/questions.md` (Q-004–Q-010):

- Biểu diễn lịch sử đặt ca/lượt thi và audit đổi giáo viên, cơ chế lưu đóng ca
  theo giờ bắt đầu; nghiệp vụ đóng/mở ca và đổi giáo viên đã chốt ADR-030.
  Học viên hủy/đổi/vắng theo ADR-021/030, người ghi vắng và trung tâm hủy/đổi ca
  đã chốt ADR-027; hủy hồ sơ tác động đặt ca đã chốt ADR-031.
- Cách biểu diễn hồ sơ đã kết thúc để thực thi quy tắc một hồ sơ đang hoạt
  động/khóa trong database; tiêu chí hoàn thành và đăng ký lại sau hủy/từ chối
  đã chốt ADR-022.
- Q-006 đã chốt nghiệp vụ bằng ADR-028/ADR-029: khóa cấu hình/ngưỡng, làm tròn,
  sửa điểm đầu vào có audit và chọn lượt khi đồng điểm. Cách lưu lịch sử audit,
  kiểu số/ràng buộc đồng thời sẽ hoàn thiện ở thiết kế database.
- Q-007 đã chốt nghiệp vụ bằng ADR-022/026/031; còn biểu diễn lịch sử và
  liên kết audit/người/thời điểm/trạng thái trước hủy ở bước database. Không thêm luồng
  hủy sau thanh toán/hoàn tiền hoặc chuyển học viên qua thao tác hủy lớp.
- Chuyển lớp đã loại khỏi phạm vi (ADR-040); cách ghi nhận buổi thực tế còn Q-008.
  Số buổi/lịch đủ
  trước bắt đầu và giới hạn sửa ngày/giờ/phòng đã chốt ADR-032. Hạn đóng nhập/sửa
  điểm danh là trước COMPLETED; mở nhập từ lúc LESSON bắt đầu (ADR-033).
  Phạm vi ngày nghỉ đã chốt ADR-035: trong NV19/F21, không module/automation/
  học bù cá nhân. Dời/thay LESSON không tổ chức và cách đếm đã chốt ADR-036;
  còn biểu diễn lịch sử/liên kết/audit. Buổi thêm vượt số LESSON chương trình
  đã bị loại khỏi phạm vi ADR-037, không hỏi lại.
  Mở/bắt đầu/kết thúc lớp theo ADR-025/032/036, xếp lớp/đổi giáo viên theo ADR-023.
- Gửi/gửi lại/trả lại/chốt toàn lớp đã chốt ADR-034; biểu diễn thao tác nhóm
  và kiểm soát đồng thời sẽ hoàn thiện ở thiết kế database/kiến trúc.
- Họ tên/ngày sinh/email/SĐT/địa chỉ đã chốt ADR-073–ADR-077; không hỏi lại.
  Tên danh mục/mã khóa-lớp/phòng/lý do-chú thích/mã giao dịch đã chốt ADR-078–
  ADR-082; còn chi tiết kiểm tra/lưu dữ liệu kỹ thuật và Q-011 mức chi tiết
  tra cứu/vị trí thao tác/lịch sử lớp-lịch/cấu hình danh mục. Phạm vi nghiệp vụ/sự kiện bảo mật,
  bộ lọc/chi tiết an toàn và không xuất nhật ký đã chốt ADR-058–ADR-062; biểu
  diễn sự kiện/chống ngập/lỗi ghi/lưu giữ còn database/kiến trúc, không hỏi lại phạm vi.
  Mật khẩu tạm/bắt buộc đổi/hủy phiên/khôi phục DISABLED/email bất biến/chặn
  đăng nhập 5 lần sai đã chốt ADR-048–ADR-052. Độ mạnh mật khẩu/hạn tạm 24 giờ/
  phiên thường 2 giờ hoặc nhàn rỗi 30 phút/nhiều thiết bị/đăng xuất chỉ phiên
  hiện tại đã chốt ADR-053–ADR-057, không hỏi lại; cơ chế lưu/thực thi/định nghĩa
  hoạt động/phiên hạn chế còn database/kiến trúc.
  Trường bắt buộc/được sửa của học viên đã chốt tại ADR-019. Học phí dương/VND nguyên
  đồng/hạn thu +7 ngày lịch/không ngày thu riêng/múi giờ Việt Nam đã chốt
  ADR-043–ADR-047, không hỏi lại; kiểu/range/lưu thời gian còn database/kiến trúc.

Không biến các đề xuất của lần review thành quyết định đã chốt. Có thể tiếp tục
phân tích các phần thiết kế độc lập, nhưng phải thống nhất toàn bộ chức năng,
workflow, database và kiến trúc trước khi bàn kế hoạch triển khai. Không bắt đầu
scaffold hoặc viết code trong giai đoạn này. Kế hoạch chốt thiết kế nằm tại
`docs/plan/design-baseline.md`.

## 8. Kiểm chứng cần có khi triển khai

- Kiểm tra Band ở biên ngưỡng và điểm ngoài 0–100.
- Chặn lượt ba, lượt hai trước điểm lượt một và thi lại sau CONFIRMED.
- Chặn đọc/sửa dữ liệu của học viên hoặc lớp/ca không được phân công.
- Duyệt lặp không tạo hóa đơn thứ hai; thanh toán lặp không tạo Payment thứ hai.
- Đồng thời đăng ký ca/xếp lớp không vượt sức chứa.
- Chặn xếp lớp sai khóa/Band, chưa thanh toán hoặc lớp không đủ điều kiện.
- Chặn trọng số không đủ 100; gửi bảng điểm thiếu điểm; công bố trước FINALIZED.
- Quá hạn chỉ tạo cảnh báo; không tự khóa/hủy hoặc thay đổi trạng thái nghiệp vụ.
- Chặn tài khoản tuyển sinh tạo tài khoản/hồ sơ hoặc đặt ca thi thay học viên.
- Sinh lịch theo tuần thành đúng các buổi cụ thể và kiểm tra xung đột cả khi
  tạo hàng loạt, thêm hoặc điều chỉnh từng buổi.
- File xuất báo cáo đúng CSV và không mở rộng quyền xuất ngoài phạm vi đã chốt.
- Chặn hai hồ sơ chưa kết thúc cho cùng học viên/khóa, kể cả tạo đồng thời;
  không chặn đăng ký khóa khác chỉ vì có một hồ sơ đang hoạt động.
- Học viên chỉ sửa thông tin cá nhân của mình; tuyển sinh không sửa thay;
  chặn đổi khóa sau CONFIRMED.
- Từ chối chỉ khi CONFIRMED, phải có lý do học viên xem được; chặn từ chối
  sau duyệt hoặc thu tiền.
- Báo cáo phân biệt số hồ sơ/số học viên; doanh thu theo thanh toán đã xác nhận
  và ngày xác nhận, công nợ chỉ Invoice UNPAID, quá hạn là tập con công nợ.
- Báo cáo học tập chỉ dùng kết quả FINALIZED; không suy ra buổi đã hoàn thành
  chỉ vì giờ dự kiến đã qua khi quy tắc ghi nhận chưa được chốt.
- Đăng nhập bằng email/mật khẩu, không nhận số điện thoại thay email.
- Chặn hồ sơ học viên thiếu họ tên/ngày sinh/email/SĐT; địa chỉ được để trống.
  Chỉ cho học viên sửa các trường được phép của mình; chặn tự đổi email và
  đổi SĐT sang giá trị không hợp lệ hoặc đã được tài khoản khác sử dụng.
- Đổi mật khẩu chỉ cho tài khoản đang đăng nhập và phải xác thực mật khẩu cũ;
  không dùng chức năng này để đặt lại mật khẩu của tài khoản khác.
- Chặn Admin khóa/vô hiệu hóa chính mình hoặc Admin hoạt động cuối cùng,
  kể cả yêu cầu đồng thời; LOCKED/DISABLED không được truy cập hoặc xóa lịch sử.
- Hủy/đổi từ ca OPEN trước giờ bắt đầu không tăng số lượt; đặt lại sau vắng vẫn cùng lượt
  chưa hoàn thành, giữ lịch sử, không ghi điểm 0 hoặc tạo lượt hai trước điểm
  lượt một. Kiểm tra sức chứa ca mới và giải phóng chỗ của lần đặt đã hủy/đổi.
- Chặn xác nhận khi lượt hai có lịch đang chờ hoặc đã thi chưa chấm; không
  cho hủy lượt đã thi chưa chấm để bỏ qua điều kiện này.
- Học viên chỉ hủy hồ sơ của mình trước CONFIRMED; tuyển sinh chỉ hủy
  CONFIRMED hoặc ADMITTED có Invoice UNPAID, phải nhập lý do. Hủy hóa đơn
  UNPAID cùng hồ sơ; chặn hủy sau thanh toán, kể cả khi thanh toán/hủy đồng thời.
- Cho đăng ký lại sau CANCELLED/REJECTED hoặc khi lớp COMPLETED và bảng điểm
  học viên FINALIZED, kể cả không đạt môn. Chỉ một trong hai điều kiện học tập
  không đủ; giữ hồ sơ cũ và không coi ADMITTED là hoàn thành.
- Chỉ xếp vào OPEN, đúng khóa/Band, còn chỗ, ADMITTED và PAID; kiểm soát khi
  xếp lớp đồng thời với chuyển trạng thái lớp. Xếp lớp không đổi ADMITTED.
- Chặn đổi giáo viên khi lớp ONGOING/đã bắt đầu dạy hoặc đã có điểm danh/điểm;
  đổi trước đó phải kiểm tra mọi lịch lớp với lịch lớp/ca thi của giáo viên mới.
- Chặn gửi khi lớp chưa COMPLETED hoặc thiếu điểm; chỉ giáo vụ chốt bảng điểm
  SUBMITTED của lớp COMPLETED. Chặn sửa điểm/chú thích khi SUBMITTED/FINALIZED,
  cho giáo viên phụ trách sửa DRAFT/RETURNED; trả lại bắt buộc lý do.
- Kiểm tra ngưỡng đạt: FINALIZED có điểm 50 đạt, dưới 50 không đạt; không dùng
  kết quả nháp hoặc thêm ràng buộc chuyên cần/điểm tối thiểu thành phần.
- Chỉ giáo vụ mở/bắt đầu/kết thúc lớp theo ADR-025; chặn mở thiếu chương trình
  ACTIVE/giáo viên/buổi LESSON tương lai hợp lệ; chặn bắt đầu trước buổi đầu
  hoặc không có học viên hợp lệ. Không tự chuyển trạng thái theo ngày/giờ.
- Chặn kết thúc khi còn buổi chưa diễn ra hoặc LESSON thiếu điểm danh;
  không yêu cầu bảng điểm đã gửi/chốt để chuyển lớp COMPLETED.
- Chỉ giáo vụ hủy PLANNED/OPEN chưa có học viên/chưa bắt đầu dạy, phải có lý
  do; chặn khi đã xếp học viên hoặc ONGOING/COMPLETED, kể cả hủy và xếp lớp
  đồng thời. Giữ lịch sử, bỏ lịch lớp CANCELLED khỏi giữ chỗ/lịch dạy sắp tới
  và không tính buổi hoàn thành; không tác động hồ sơ đã trả tiền đang chờ lớp.
- Chỉ giáo viên chấm được phân công ghi vắng sau khi ca kết thúc, không tự ghi
  vắng vì thiếu điểm. Chỉ giáo vụ hủy ca trước giờ bắt đầu, phải có lý do; giữ
  lịch sử, hủy lượt đặt hoạt động và cho học viên tự đặt lại cùng lượt thi.
- Chặn sửa ngày/giờ/phòng khi từng có đặt ca hoặc ca đã bắt đầu; chặn hủy sau
  bắt đầu và nhập điểm ca CANCELLED. Kiểm soát hủy/sửa ca đồng thời với đặt
  ca/nhập điểm; không tăng số lượt, ghi đè lịch hẹn cũ hoặc xóa điểm đã chấm.
- Chặn sửa khóa/Band/thành phần/trọng số chương trình sau lớp đầu tiên,
  kể cả lớp bị hủy; chặn sửa toàn bộ bộ ngưỡng Band sau kết quả đầu tiên,
  kể cả khi tạo lớp/chấm điểm và sửa cấu hình đồng thời.
- Từ chối điểm đầu vào/thành phần/trọng số vượt hai chữ số thập phân;
  không làm tròn từng đóng góp, chỉ làm tròn tổng một lần half-up. Kiểm tra
  tổng 49,995 → 50,00 đạt khi FINALIZED, 49,994 → 49,99 không đạt;
  điểm đầu vào 44,99 vẫn Band 2, 45,00 Band 3, không làm tròn thành số nguyên.
- Sửa điểm đầu vào chỉ bởi giáo viên đã nhập và vẫn phụ trách ca, hồ sơ
  TESTING/BAND_ASSIGNED, lượt COMPLETED; bắt buộc lý do và lưu điểm cũ/mới/
  người/thời gian. Tính lại điểm cao nhất/Band cùng transaction, không tăng lượt.
- Chặn sửa sau CONFIRMED hoặc CANCELLED/REJECTED, bởi giáo viên khác hoặc
  Admin/giáo vụ; kiểm tra sửa đồng thời với xác nhận/hủy/chấm điểm. Nếu hai lượt
  COMPLETED đồng điểm cao nhất, best_attempt_id là lượt một; giữ cả hai kết quả.
- Ca mới CLOSED; chỉ giáo vụ mở/mở lại trước bắt đầu với tài nguyên/giáo viên
  ACTIVE/sức chứa hợp lệ. Ca hết chỗ vẫn OPEN; hủy hợp lệ giải phóng chỗ.
- Chặn mọi đăng ký/hủy/đổi đặt chỗ của học viên trong CLOSED, kể cả đồng thời
  với đóng ca; giữ lịch sử/đặt chỗ và không bỏ chặn xác nhận thi lại đang chờ.
  CLOSED vẫn cho chấm/ghi vắng/sửa điểm theo ADR-027/029; CANCELLED bị chặn.
- Đến giờ bắt đầu chặn mở lại và thao tác đặt chỗ dù trạng thái lưu còn OPEN.
  Đổi giáo viên chấm chỉ giáo vụ, ca chưa hủy/chưa bắt đầu/chưa có điểm hoặc
  ABSENT, có lý do/audit, Teacher ACTIVE và kiểm tra lịch lớp/ca thi; giữ đặt
  chỗ, kiểm soát đồng thời với chấm điểm và thay đổi lịch/phân công.
- Hủy hồ sơ hợp lệ hủy đặt chỗ hoạt động trong ca chưa bắt đầu OPEN/CLOSED,
  giải phóng chỗ cùng hồ sơ/hóa đơn UNPAID; giữ ca/đặt chỗ người khác, hiển thị
  ảnh hưởng trước xác nhận. Một cập nhật bắt buộc thất bại thì không hủy phần nào.
- Giữ lịch sử ca đã bắt đầu và mọi điểm/ABSENT/Band đã có khi hủy hồ sơ;
  chặn nhập/sửa điểm và ghi vắng sau CANCELLED, kể cả ca CLOSED. Lượt chưa chấm
  hiển thị hồ sơ đã hủy/chưa có kết quả, không tự ghi 0/ABSENT hoặc xóa lịch sử.
- Chặn đặt ca tiếp trên hồ sơ CANCELLED; đăng ký lại dùng hồ sơ mới, thi từ đầu,
  không sao chép điểm/Band cũ. Lưu audit/hủy phụ thuộc và kiểm soát đồng thời với
  đặt ca, ca bắt đầu, chấm điểm, xác nhận/duyệt và thanh toán (ADR-031).
- Số buổi chương trình là số nguyên dương, chỉ LESSON; chặn sửa sau lớp đầu
  tiên sử dụng, kể cả lớp đã hủy hoặc tạo lớp/sửa cấu hình đồng thời (ADR-032).
- Cho mở lớp với một LESSON tương lai hợp lệ nhưng chặn bắt đầu khi thiếu lịch
  LESSON dự kiến; FINAL_EXAM không bù số LESSON thiếu. Giữ các điều kiện ADR-025.
- Chặn sửa ngày/giờ/phòng buổi đã bắt đầu, có điểm danh/điểm liên quan hoặc
  thuộc lớp COMPLETED/CANCELLED; kiểm tra xung đột và thao tác đồng thời với
  điểm danh/nhập điểm/bắt đầu-kết thúc lớp, không ghi đè lịch sử (ADR-032).
- Không chặn nhập/sửa điểm danh hợp lệ chỉ vì quá 24/48 giờ sau buổi học;
  chặn khi lớp COMPLETED dù bảng điểm chưa gửi/chốt. Giữ quyền lớp phụ trách,
  chặn hoàn thành lớp thiếu điểm danh và bảo vệ cập nhật đồng thời (ADR-033).
- Chặn điểm danh trước giờ LESSON bắt đầu; cho nhập từ đúng thời điểm bắt đầu,
  không bắt chờ đến kết thúc buổi. Kiểm tra lịch hiện hành và cập nhật đồng
  thời với sửa lịch; không tự coi có điểm danh là buổi đã diễn ra đầy đủ.
- Thiếu điểm bắt buộc của một học viên chặn gửi/gửi lại toàn lớp; không cho
  gửi/trả lại/chốt riêng một học viên. Trả lại có lý do giữ nguyên dữ liệu,
  cho sửa rồi gửi lại toàn lớp; chốt công bố tất cả cùng lúc. Lỗi hoặc tranh
  chấp cập nhật không để một phần lớp chuyển trạng thái (ADR-034).
- Buổi cả lớp không tổ chức không được coi là đã dạy hoặc ghi cả lớp vắng
  để vượt điều kiện hoàn thành; không tự bỏ ngày lễ/sinh lịch bù. Phạm vi
  giữ trong NV19/F21, không thêm học bù cá nhân (ADR-035).
- Dời LESSON tương lai theo ADR-032 không tăng số buổi. LESSON đã đến/qua giờ
  dự kiến nhưng không tổ chức giữ nguyên lịch cũ, ghi nhận lý do/người/thời gian
  và tạo buổi thay thế liên kết bởi giáo vụ; chặn nếu có điểm danh/điểm liên
  quan hoặc lớp COMPLETED/CANCELLED, không xóa dữ liệu. Kiểm tra lịch giáo viên
  hiện tại/phòng/lớp, thao tác lặp và cập nhật đồng thời; không lưu một phần.
  Buổi thay thế phải diễn ra/đủ điểm danh trước hoàn thành; không đếm cặp là
  hai LESSON hay yêu cầu điểm danh cho buổi cũ không tổ chức (ADR-036).
- Chặn tạo lịch tuần/thêm LESSON vượt số nghĩa vụ chương trình, kể cả đồng
  thời. Không chặn buổi thay thế hợp lệ chỉ vì tổng bản ghi lịch sử vượt số
  này; không đếm cặp cũ/thay thế hai lần. FINAL_EXAM không tính vào LESSON
  hoặc bù LESSON thiếu; không thay đổi cấu hình đã khóa để né giới hạn (ADR-037).
- Chặn sửa khóa trên hồ sơ hiện có ở mọi trạng thái. Đổi lựa chọn bằng hủy
  hợp lệ/đăng ký mới giữ hồ sơ cũ nhưng không chuyển điểm/Band (ADR-038).
- Sửa thông tin cá nhân được phép cập nhật hiển thị hồ sơ lịch sử, không đổi
  snapshot học phí hoặc sự kiện nghiệp vụ/audit (ADR-039).
- Chặn gỡ/xếp lại học viên để chuyển lớp sau placement, kể cả trước dạy (ADR-040).
- Chỉ học viên sở hữu hóa đơn được in qua trình duyệt; hóa đơn UNPAID không
  thể hiện PAID, không thêm chức năng PDF/biên lai riêng (ADR-041).
- Chặn sửa Payment bởi vai trò khác hoặc sửa số tiền/hóa đơn/ngày xác nhận;
  sửa phương thức/mã giao dịch bởi kế toán cần lý do/audit và mã giao dịch khi
  chuyển khoản. Giữ cùng Payment/Invoice PAID; lỗi/race không lưu một phần hoặc
  sinh Payment mới, không hủy/xóa/hoàn tiền (ADR-042).
- Chặn học phí/Invoice/Payment bằng 0, âm hoặc có phần lẻ của đồng; giữ tiền
  chính xác, không dùng quy tắc làm tròn điểm để làm tròn tiền (ADR-043/044).
- Hóa đơn phát hành 27/09/2026 có hạn 04/10/2026, không dời vì cuối tuần/ngày
  nghỉ; chưa quá hạn ngày 04/10, UNPAID mới quá hạn ngày 05/10 theo giờ Việt Nam
  (ADR-045/047). Không sinh hạn mới khi in/sửa metadata Payment.
- Không nhận ngày thu riêng hoặc ngày xác nhận nhập lùi; doanh thu dựa trên
  thời điểm xác nhận bất biến (ADR-046/042). Kiểm tra ngày sát nửa đêm/lọc theo
  ngày với thiết bị ở múi giờ khác, vẫn theo ranh giới UTC+7 (ADR-047).
- Cấp nhân sự/reset chỉ hiển thị mật khẩu tạm một lần cho Admin, không lộ trong
  log/audit; chặn nghiệp vụ trước khi đổi, chặn cả truy cập trực tiếp không qua UI.
  Reset tài khoản LOCKED/DISABLED không tự mở/khôi phục (ADR-048).
- Khóa/ngừng dùng/reset/đổi mật khẩu làm mọi phiên cũ/token chưa hết hạn bị từ
  chối; kể cả mở khóa/khôi phục sau đó, phiên cũ không sống lại (ADR-049/050).
- Chỉ Admin khôi phục DISABLED trên cùng tài khoản, giữ vai trò/dữ liệu/lịch sử;
  chặn sửa email tài khoản hiện có bởi mọi vai trò, kể cả nhân sự (ADR-050/051).
- Kiểm tra 5 lần sai liên tiếp/cửa sổ 15 phút/thời gian chặn 15 phút và đặt lại
  bộ đếm khi thành công/hết hạn; không đổi status hoặc hủy phiên đang dùng.
  Hết chặn tạm không tự mở LOCKED/khôi phục DISABLED; bảo vệ yêu cầu đồng thời
  khi ghi số lần sai/chặn đăng nhập (ADR-052).
- Kiểm tra biên 15/128 ký tự, Unicode/khoảng trắng, danh sách mật khẩu phổ biến/
  đã lộ; không bắt tổ hợp ký tự hoặc cắt ngắn mật khẩu (ADR-053).
- Chặn mật khẩu tạm tại/sau mốc cấp/reset +24 giờ, cần Admin reset mới; không
  tự LOCKED/DISABLED/mở khóa/khôi phục, giữ cửa đổi trước nghiệp vụ (ADR-054).
- Phiên thường bị từ chối tại hạn đăng nhập +2 giờ hoặc nhàn rỗi 30 phút,
  dù JWT chưa hết hạn; hoạt động không vượt hạn tuyệt đối, không refresh/ghi nhớ
  đăng nhập (ADR-055). Định nghĩa hoạt động cần chốt trước kiểm thử thực thi.
- Đăng nhập thiết bị mới không hủy phiên cũ; đăng xuất chỉ làm phiên hiện tại
  không dùng lại được tại server. Phiên khác vẫn dùng được nếu chưa hết hạn/
  bị hủy; ADR-049 vẫn hủy mọi phiên khi khóa/reset/đổi mật khẩu (ADR-056/057).
- Kiểm tra audit thay đổi cấu hình/trạng thái lớp/tạo-dời-thay lịch và giữ mọi
  audit bắt buộc trước đó; không ghi mỗi lượt xem thông thường (ADR-058).
- Kiểm tra các sự kiện bảo mật thành công/thất bại/chặn tạm/từ chối quyền;
  không gán người thao tác đã xác thực từ email do người chưa đăng nhập nhập
  (ADR-059). Lưu audit không làm thay chính sách khóa hoặc phiên đã chốt.
- Chỉ Admin dùng bộ lọc ngày/người/hành động/loại-ID/kết quả; phân trang mới
  nhất trước. Chặn truy cập nhật ký bởi vai trò khác hoặc xuất CSV/PDF (ADR-060/062).
- Nhật ký giữ các thay đổi cũ/mới bắt buộc nhưng không chứa mật khẩu/hash/JWT/
  bí mật phiên/toàn payload; không tạo snapshot cá nhân hoặc làm mất lịch sử
  nghiệp vụ cần giữ (ADR-061).
- Báo cáo kỳ cũ vẫn đánh giá trạng thái hiện tại; hóa đơn đã hủy không góp tổng
  phải thu dù từng phát hành trong kỳ. Giữ lịch sử, không báo cáo as-of (ADR-063/064).
- Kiểm tra tách kỳ phát hành/tổng phải thu, kỳ xác nhận/doanh thu và công nợ
  UNPAID hiện tại; không lọc công nợ theo ngày thanh toán hoặc trừ sai tập.
- Một học viên có hồ sơ hai khóa: mỗi nhóm một học viên nhưng tổng chung một;
  hai hồ sơ học lại cùng khóa: số hồ sơ hai, số học viên một (ADR-065).
- Tiến độ chỉ nghĩa vụ LESSON thực tế diễn ra, không FINAL_EXAM/buổi không tổ
  chức/cặp lịch sử-thay thế đếm đôi hoặc chỉ dựa giờ đã qua (ADR-066).
- Tỷ lệ đạt chọn kỳ chốt FINALIZED, mỗi lượt học riêng; loại chưa chốt, dùng
  ngưỡng và điểm làm tròn đã chốt. Phân biệt không dữ liệu với 0% đạt (ADR-067).
- Kiểm tra mặc định tháng Việt Nam hiện tại, mọi trạng thái và khóa ngừng vẫn
  chọn được để xem lịch sử; hiện trạng đào tạo không lọc kỳ ngày, kết quả lọc
  kỳ chốt điểm/giáo viên hiện tại (ADR-068/069).
- Đổi kỳ tài chính không lọc công nợ hiện tại; lọc khóa nhất quán và tùy chọn
  quá hạn chỉ chọn phần công nợ đã quá hạn (ADR-070).
- Giám đốc chỉ nhận số liệu nhóm, không hồ sơ/hóa đơn/định danh từng học viên
  hoặc xuất file; quyền tra cứu nghiệp vụ Kế toán không đổi (ADR-071).
- CSV Kế toán gồm toàn kết quả lọc/cột báo cáo, không chỉ trang và không trường
  ẩn/dữ liệu hồ sơ cá nhân; chặn dùng quyền đó xuất loại dữ liệu khác (ADR-072).
- Kiểm tra tên Unicode/trim/1–100 ký tự, không bắt hai từ; ngày sinh hợp lệ,
  không tương lai theo giờ Việt Nam nhưng không áp giới hạn tuổi (ADR-073/074).
- Email tạo mới sai định dạng/quá 254 ký tự hoặc trùng chỉ khác hoa-thường
  phải bị từ chối; không biến đổi dấu chấm/+tag hoặc mở quyền sửa email (ADR-075).
- SĐT dạng 0 và +84 tương đương không được né so trùng, kể cả có khoảng trắng/
  dấu gạch; giữ phạm vi đã chọn/không OTP/không bắt nhân sự có SĐT (ADR-076).
- Địa chỉ trống hợp lệ, quá 255 ký tự bị từ chối, không cắt ngắn hoặc bắt chọn
  địa phương/thêm snapshot cá nhân (ADR-077).
- Kiểm tra tên danh mục Unicode/trim/giới hạn 150 hoặc 100; giữ uniqueness
  thành phần theo chương trình và khóa cấu hình (ADR-078).
- Mã khóa/lớp chuẩn hóa hoa/1–30/định dạng đã chốt, không né trùng qua ngừng/
  hủy/khác hoa-thường hoặc đổi mã sau tạo (ADR-079).
- p101/P101 không né kiểm tra trùng phòng giữa lớp/ca thi; không suy bí danh
  khác là cùng phòng, không mở quyền sửa lịch sau cutoff (ADR-080).
- Lý do bắt buộc không trống/quá 1000; nhận xét tùy chọn, văn bản thuần/giữ
  xuống dòng, không thêm bắt buộc hoặc vượt khóa điểm (ADR-081).
- Mã chuyển khoản không trống/quá 100, giữ nội dung sau trim; không chặn chỉ
  vì trùng mã giữa các hóa đơn. Vẫn chặn Payment thứ hai cùng Invoice và giữ
  đối chiếu thực tế/giới hạn sửa/audit (ADR-082).
