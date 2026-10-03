# Yêu cầu nghiệp vụ chính thức — Quản lý trung tâm Anh ngữ

Cập nhật: 03/10/2026. Đây là nguồn yêu cầu dùng để lập kế hoạch và triển khai.
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

- ADR-133: hồ sơ mới tạo PENDING; đặt ca đầu tiên chuyển TESTING; có kết quả
  điểm đầu tiên chuyển BAND_ASSIGNED. Đặt lượt hai giữ BAND_ASSIGNED và vẫn
  chặn xác nhận khi còn lượt chờ kết quả. Hủy đặt chỗ/ghi vắng không kéo lùi
  trạng thái hoặc xóa Band; hủy cả hồ sơ vẫn theo đường CANCELLED đã chốt.
- Học viên tự tạo tài khoản, đăng ký khóa và ca thi. Nhân viên tư vấn/hướng dẫn
  tại quầy không là nghiệp vụ đăng ký riêng; vẫn dùng luồng và quyền Học viên.
  Không có luồng/API/menu tuyển sinh ghi danh hoặc đăng ký thi thay học viên.
- Mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng khóa. Có thể đăng ký
  các khóa khác đồng thời và học lại khóa cũ sau khi hoàn thành. ADR-022:
  hoàn thành khi lớp được xếp COMPLETED và bảng điểm của học viên FINALIZED,
  không phụ thuộc đạt môn. CANCELLED/REJECTED cũng là hồ sơ đã kết thúc để
  được tạo hồ sơ mới; giữ nguyên hồ sơ và lịch sử cũ. ADMITTED chưa phải hoàn
  thành. Biểu diễn is_unfinished/filtered unique đã chốt ADR-090, không thêm trạng thái.
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

- ADR-135: final_score chỉ lưu khi GradeReport FINALIZED; tổng nháp/SUBMITTED
  tính từ các điểm đã lưu, thiếu thành phần hiển thị Chưa đủ điểm. Không có
  Score row không là điểm0; không lưu cờ đạt trùng kết quả. Giữ làm tròn,
  quyền xem, đơn vị lưu và khóa đã chốt.
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
  thành (ADR-026). Occurrence/link nghĩa vụ đã chốt ADR-087; dictionary E03-v1 đã nghiệm thu01/10/2026, chưa có schema chạy.
- Một lớp có đúng một giáo viên, sức chứa 1–15. Không thêm bảng/workflow/UI
  lịch sử đổi giáo viên riêng (ADR-023/058); mandatory mutation audit của E02
  W14 đã nghiệm thu vẫn giữ. ADR-023: giáo vụ chỉ đổi giáo viên trước ONGOING và
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
- ADR-156 / E04-RF03, duyệt03/10/2026: mỗi Schedule root mới (LESSON hoặc
  FINAL_EXAM), mọi LESSON cụ thể trong lịch tuần và thời điểm đích khi dời
  phải có startAt>giờ server tại lúc kiểm tra lưu trong transaction. Tới đúng/
  qua giờ bắt đầu sau input hợp lệ thì409; sai cấu trúc ngày/giờ thì400.
  Preview không giữ chỗ/thời gian; server kiểm lại clock độc lập RowVersion.
  Một buổi trong batch không đạt thì không lưu buổi nào, không bỏ/cắt/dời ngày
  tự động. Không mở luồng nhập lịch quá khứ để nhập bù Attendance/Score/HELD.
  Guard buổi cũ trước start, replacement future và HELD sau end/ONGOING giữ
  nguyên; chỉ RF03 được chốt, RF01/RF02 không được duyệt từ quyết định này.
- Số buổi dự kiến thuộc TrainingProgram theo khóa/Band, là số nguyên dương,
  chỉ tính LESSON, không tính FINAL_EXAM. Khóa cấu hình số buổi khi lớp đầu tiên
  sử dụng chương trình, kể cả lớp sau đó bị hủy (ADR-032/090). Số buổi là
  số nguyên dương, khóa cấu hình theo ADR-090/091; dictionary E03-v1 đã nghiệm thu,
  chưa tạo schema/migration hoặc version chương trình.
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
  không cộng thành hai LESSON; không thay đổi số buổi chương trình. Trạng thái
  buổi, khóa nghĩa vụ, chuỗi thay thế và lịch sử/audit đã chốt ADR-087/093;
  dictionary E03-v1 đã nghiệm thu01/10/2026, chưa tạo schema/migration.
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
  thành lớp; giáo vụ xác nhận buổi thực tế HELD sau khi kết thúc theo ADR-087.

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
  chính xác điểm/trọng số ADR-028; kiểu/range/API đã chốt ADR-091; chưa có schema chạy thực tế.
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
  qua email. Bộ kiểm tra/đếm Unicode/lưu chuẩn hóa đã chốt ADR-091.
- Email đăng nhập không được sửa trên tài khoản hiện có, kể cả bởi Admin và
  đối với nhân sự (ADR-051). Không dùng xóa/tạo lại tài khoản để né giới hạn.
- Sai mật khẩu 5 lần liên tiếp trong 15 phút thì chặn đăng nhập mới 15 phút;
  đăng nhập thành công hoặc hết hạn đặt lại bộ đếm/cửa sổ (ADR-052). Không đổi
  Account thành LOCKED/DISABLED hoặc hủy phiên đang dùng chỉ vì nhập sai.
  LOCKED thủ công vẫn cần Admin mở; DISABLED không tự khôi phục sau thời hạn.
- Mật khẩu 15–128 ký tự, cho phép Unicode/khoảng trắng, không bắt buộc phối hợp
  chữ hoa/số/ký tự đặc biệt; chặn mật khẩu phổ biến/đã lộ, không ép đổi định kỳ
  khi chưa có sự cố (ADR-053). Không cắt ngắn mật khẩu; yêu cầu đổi mật khẩu tạm
  trước nghiệp vụ vẫn giữ. Cách đếm đã chốt ADR-091; hash/generator và danh sách
  chặn offline đã duyệt ADR-114 (mục 4.7), không gửi mật khẩu ra dịch vụ ngoài.
  Danh sách hữu hạn không phát hiện mọi mật khẩu từng lộ.
- Mật khẩu tạm hết hạn sau 24 giờ tính từ lúc cấp/reset; hết hạn cần Admin reset
  lại, không dùng mật khẩu hết hạn để đăng nhập/đổi qua cửa bắt buộc đổi (ADR-054).
  Không tự đổi trạng thái tài khoản, mở khóa/khôi phục hoặc gửi lại qua email.
- Phiên đăng nhập thông thường hết sau 2 giờ từ đăng nhập hoặc 30 phút không
  hoạt động, mốc nào đến trước; hoạt động không kéo dài hạn tuyệt đối (ADR-055).
  Thực thi tại server, phải đăng nhập lại, không refresh/tự gia hạn/ghi nhớ đăng
  nhập. Hoạt động/shared-tab đã chốt ADR-097; phiên đổi mật khẩu tạm ADR-098.
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
  tác đã được xác minh; actor chưa xác thực là NULL. Nhóm thao tác/lịch sử
  đã chốt ADR-093; lỗi ghi/lưu giữ/IP/chống ngập đã duyệt ADR-115/116 (mục 4.7).
- Chỉ Admin tra cứu nhật ký; lọc theo khoảng ngày nghiệp vụ, người thao tác,
  hành động, loại/mã đối tượng và kết quả, phân trang mới nhất trước (ADR-060).
  Không thêm tìm kiếm tự do toàn payload; ngày theo ADR-047.
- Chi tiết nhật ký gồm thời gian/người/thao tác/đối tượng/kết quả-lý do và trường
  thay đổi cần thiết khi áp dụng, giữ audit cũ/mới đã bắt buộc (ADR-061).
  Không lưu mật khẩu/hash mật khẩu/JWT/bí mật phiên hoặc toàn request/response
  vào log; hạn chế dữ liệu cá nhân. Hash mật khẩu chỉ ở nơi lưu thông tin xác
  thực, không ở log. Không bỏ lịch sử bắt buộc hoặc thêm snapshot cá nhân ADR-039.
- Admin chỉ xem/tra cứu nhật ký trong ứng dụng, không xuất CSV/PDF (ADR-062).
  Không sửa/xóa log qua nghiệp vụ thông thường. Audit bắt buộc giữ cùng lịch
  sử dự án; log bảo mật thuần túy/IP dọn bằng vận hành sau 90 ngày (ADR-116),
  không xóa audit bắt buộc theo hạn này. Backup vận hành đã duyệt ADR-118,
  chưa chọn nơi lưu hoặc thực hiện backup/restore.
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
  Giờ đã qua/điểm danh không tự chứng minh đã diễn ra; xác nhận/biểu diễn ADR-087.
  Lớp CANCELLED vẫn có trong số lớp theo trạng thái, không tính buổi đã dạy.
- Tỷ lệ đạt = GradeReport FINALIZED đạt / tổng GradeReport FINALIZED trong kỳ
  chốt điểm, trong bộ lọc khóa/lớp đã chọn (ADR-067). Mỗi kết quả lượt học tính
  riêng, không đếm người duy nhất qua học lại. Loại chưa chốt; không có kết quả
  FINALIZED hiển thị "Chưa có dữ liệu", không 0%. Có kết quả nhưng không ai đạt
  mới là 0%; tỷ lệ hiển thị 2 số lẻ half-up (ADR-094).
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
- Truy vấn/xuất nhất quán/tỷ lệ đã chốt ADR-094, biểu diễn thực tế ADR-087.
  Cấu trúc/giới hạn/mã hóa/an toàn CSV D10 đã duyệt ADR-101; chưa triển khai.

## 4.1. Bổ sung A–C đã được duyệt — 28/09/2026

Owner đã tích [x] cả 33 mục A01–A10/B01–B07/C01–C16 và xác nhận bằng lời;
không có phương án thay thế. Các quy tắc dưới đây đã có hiệu lực, ghi ADR-083–
ADR-094; nhãn A/B/C truy vết tới file OWNER_REVIEW, không còn là câu hỏi.
Tại thời điểm duyệt A–C, D/E chưa duyệt. Gói 1 D01–D04/D06–D07/D10/D15
được duyệt riêng ngày 29/09/2026 (mục 4.2); E01-v1 được owner duyệt
ngày30/09/2026; E02-v1 được owner duyệt cùng ngày, E03-v1 được nghiệm thu
ngày01/10/2026. E04-v1/E05-v1 đã nghiệm thu03/10/2026 (ADR-160/161).
Chi tiết mô hình mục tiêu tương ứng được đồng bộ tại ARCHITECTURE.md.

### ADR-083 — Define operational detail visibility

- A01: Tra cứu mọi trạng thái hồ sơ, kể cả đã kết thúc/CANCELLED/REJECTED. Chi tiết: tên/email/SĐT hiện tại, khóa, kết quả đầu vào/Band, xác nhận/duyệt/trạng thái, số tiền và trạng thái hóa đơn, lớp đã xếp. Không xem phương thức/mã giao dịch Payment, điểm danh/bảng điểm đào tạo, ngày sinh/địa chỉ hoặc danh sách hồ sơ cá nhân toàn hệ thống; không thêm quyền sửa/đăng ký thay/xuất.
- A02: Xem tên/email/SĐT hiện tại, khóa/kết quả đầu vào/Band phục vụ ca thi, xếp và quản lý lớp. Hóa đơn chỉ hiển thị trạng thái, không số tiền/Payment. Xem lịch/điểm danh và bảng điểm SUBMITTED/RETURNED/FINALIZED, không giá trị nháp DRAFT của giáo viên. Không thêm quyền nhập/sửa điểm, sửa hồ sơ hoặc tài chính.
- A03: Tra mọi trạng thái gồm CANCELLED; lọc số hóa đơn/khóa/tên-email-SĐT học viên/trạng thái/khoảng ngày phát hành tùy chọn/chỉ quá hạn. Chi tiết hóa đơn, Payment và người/thời điểm xác nhận cùng thông tin liên hệ cần thiết; không điểm/điểm danh. Tách tra cứu nghiệp vụ này khỏi kỳ báo cáo và quyền CSV; không thêm xuất riêng hồ sơ/hóa đơn, in hoặc sửa ngoài quyền đã chốt.
- A06: Trong ca/lớp đang được phân công, xem mã học viên nội bộ, tên/email/SĐT hiện tại và dữ liệu thi/điểm danh/điểm thuộc nhiệm vụ của mình; không ngày sinh/địa chỉ/hóa đơn/Payment hoặc danh sách học viên toàn trung tâm. Không còn được phân công thì mất quyền xem theo nhiệm vụ đó, không thêm quyền sửa hồ sơ.

### ADR-084 — Place account actions and Student schedule history in existing UI

- A04: Có hai phần Hiện tại/Lịch sử, chọn lớp của chính mình khi học nhiều khóa hoặc học lại. Lớp hoàn thành chỉ xem; buổi không tổ chức ghi rõ và liên kết buổi thay thế, không hiện như lịch sắp tới hoặc hai buổi đã dạy. Không xem lớp người khác/điểm chưa FINALIZED; không ngầm chốt enum lưu lịch.
- A05: Dùng menu tài khoản chung trên giao diện hiện có: mọi vai trò tự đổi mật khẩu; chỉ Học viên sửa hồ sơ cá nhân theo các trường đã chốt. Không thêm mã F hoặc quyền sửa thông tin nhân sự. Giữ kiểm tra mật khẩu hiện tại, email bất biến, hủy phiên và bắt đổi mật khẩu tạm.

### ADR-085 — Define catalogue lifecycle and immutable class-program links

- A07: Giáo vụ sửa tên/giá cơ sở; giá mới chỉ áp dụng hóa đơn phát hành sau đó, không sửa snapshot tiền đã phát hành. Course INACTIVE chặn đăng ký mới; các hồ sơ đã tồn tại tiếp tục quy trình hợp lệ, không tự hủy hồ sơ/lớp/chương trình. Cho khôi phục ACTIVE trên cùng khóa; mã khóa vẫn bất biến.
- A08: Trước lớp đầu tiên: cấu hình ở DRAFT; chuyển ACTIVE khi Band/khóa hợp lệ, số LESSON dương, thành phần hợp lệ và tổng trọng số 100. Muốn sửa cấu hình ACTIVE chưa dùng thì đưa về DRAFT và kiểm tra lại. INACTIVE chặn tạo/mở lớp mới, không dừng lớp đã dạy/chấm; khôi phục ACTIVE không xóa khóa cấu hình đã sử dụng. Không tạo chương trình trùng khóa/Band hoặc version để né khóa.
- A09: Không đổi TrainingProgram của Class sau khi tạo; chọn sai thì xử lý hủy lớp hợp lệ/tạo lớp mới, không chuyển học viên. Giáo vụ sửa sức chứa chỉ trước ONGOING, trong 1–15 và không dưới sĩ số đã xếp. Giữ nguyên giới hạn đổi giáo viên, mã lớp và điều kiện hủy đã chốt.

### ADR-086 — Gate initial entry grading at test end and enforce effective closure

- A10: Chỉ cho giáo viên được phân công nhập điểm đầu vào lần đầu từ khi ca kết thúc; không áp hạn 24/48 giờ, nhưng phải còn hồ sơ hợp lệ trước xác nhận và ca không CANCELLED. Không nới cửa sổ sửa điểm ADR-029; ABSENT vẫn chỉ sau ca kết thúc và không suy từ thiếu điểm. Điểm đào tạo giữ cửa sổ lớp ONGOING/COMPLETED và DRAFT/RETURNED đã chốt.
- B04: API luôn tính trạng thái hiệu lực: ca chưa hủy và đã đến giờ bắt đầu là CLOSED dù dữ liệu cũ còn OPEN. Ghi CLOSED cùng thao tác ghi hợp lệ kế tiếp; truy vấn chỉ đọc không âm thầm sửa dữ liệu. Không cần cron mới để quyền đóng ca đúng giờ; tất cả thao tác kiểm tra giờ server trong transaction, không mở lại ca đã bắt đầu.

### ADR-087 — Confirm actual occurrence and model linked lessons and final exams

- B01: Giáo vụ xác nhận riêng từng LESSON/FINAL_EXAM sau giờ kết thúc dự kiến và khi lớp ONGOING; lưu người/thời điểm. Không tự suy từ thời gian hoặc điểm danh. Không có nút đảo HELD về chưa diễn ra; lịch chưa được xác nhận tiếp tục chặn hoàn thành. Không thay cửa sổ nhập điểm danh đã duyệt.
- B02: Mỗi lớp có một nghĩa vụ FINAL_EXAM, riêng số LESSON, sau LESSON cuối theo lịch hiện hành; cấu hình có một thành phần điểm loại FINAL_EXAM. Nếu thi cả lớp không tổ chức, mở rộng quy trình thay thế có lý do/liên kết sang FINAL_EXAM, chỉ khi chưa điểm phụ thuộc và lớp chưa kết thúc; giữ buổi cũ, một nghĩa vụ, không thi lại cá nhân. Bổ sung đã duyệt cho FINAL_EXAM ngoài LESSON của ADR-036.
- C02: Schedule có occurrence_state PLANNED/HELD/NOT_HELD, người/thời điểm xác nhận và lý do/người/thời điểm không tổ chức; có mã nghĩa vụ và replaces_schedule_id cùng lớp/loại. Một lịch hiện hành cho mỗi nghĩa vụ; chuỗi thay thế giữ lịch sử nhưng không tăng số buổi. Lịch lớp CANCELLED bị loại bằng trạng thái lớp, không suy HELD từ điểm danh hoặc đồng hồ.
- C03: ScoreComponentConfig có kind GENERAL/LESSON/FINAL_EXAM; loại này cũng khóa từ lớp đầu tiên. Score có schedule_id bắt buộc với LESSON/FINAL_EXAM, cùng lớp/loại và buổi hiện hành; GENERAL để NULL. Vẫn một Score/học viên/thành phần, không biến thành điểm mỗi buổi vô hạn. Điểm gắn buổi chỉ nhập sau khi buổi đã bắt đầu; không nhập cho buổi NOT_HELD. Phối hợp B02 đã duyệt.

### ADR-088 — Use minute-precision same-day intervals and atomic timetable generation

- B03: Mỗi ca/buổi bắt đầu-kết thúc trong cùng ngày Việt Nam, bắt đầu trước kết thúc, độ chính xác phút; khoảng thời gian dạng [bắt đầu, kết thúc) nên được học/thi nối tiếp đúng giờ. Ngoài xung đột phòng/giáo viên đã chốt, chặn đặt ca/xếp lớp làm học viên trùng ca hoặc lịch lớp còn hiệu lực; không tự chuyển lớp/ca để xử lý.
- B05: Nhập khoảng ngày/thứ/giờ/phòng, xem trước các buổi cụ thể, số nghĩa vụ còn thiếu và mọi xung đột; không tự bỏ ngày nghỉ. Chỉ lưu toàn bộ nếu tất cả hợp lệ và không vượt số LESSON; nếu lỗi không lưu phần nào. Không tự sinh lịch sau này và không tạo entity quy tắc lặp; thêm từng buổi vẫn được phép.

### ADR-089 — Preserve blocked assignments and require sensitive-action conflict previews

- B06: Không tự đổi người phụ trách, hủy lớp/ca/hồ sơ hoặc xóa dữ liệu. Người bị chặn không thực hiện nghiệp vụ; giao diện báo rõ tài khoản phụ trách không khả dụng. Đổi người chỉ trong cửa sổ đã chốt; ngoài cửa sổ thì chờ Admin mở/khôi phục tài khoản hợp lệ, không thêm quyền đổi giáo viên giữa khóa hoặc sửa điểm ngoại lệ.
- B07: Trước thanh toán/chốt hoặc trả bảng điểm cả lớp/hủy/reset/khóa-ngừng tài khoản, hiển thị đối tượng và ảnh hưởng; lý do chỉ ở nơi đã bắt buộc. Khi dữ liệu thay đổi sau khi mở form, báo xung đột và yêu cầu tải lại/kiểm tra, không ghi đè tự động. Không thêm người phê duyệt thứ hai; preview hủy hồ sơ ADR-031 vẫn giữ.

### ADR-090 — Separate test bookings and enforce relational lifecycle integrity

- C01: Thêm EntryTestBooking liên kết EntryTestAttempt và EntryTestSession; mỗi lần đặt/hủy/vắng có bản ghi riêng, không ghi đè lịch sử. Attempt duy nhất theo hồ sơ/số lượt 1–2; một booking SCHEDULED hiện tại/lượt chưa hoàn thành, kết quả trỏ booking đã hoàn thành. API suy diễn trạng thái lượt từ kết quả/booking hiện tại; vắng/đặt lại không tăng lượt, không tạo lượt ba.
- C04: Lưu Registration.is_unfinished do server quản lý; unique có điều kiện trên student_id/course_id khi true. Cập nhật cờ cùng transaction hủy/từ chối hoặc khi Class COMPLETED và GradeReport FINALIZED; kiểm tra cả dữ liệu gốc khi ghi để tránh cờ lệch. Không thêm Registration COMPLETED, không yêu cầu đạt môn.
- C06: Dùng int IDENTITY cho các bảng nghiệp vụ; khóa kỹ thuật phiên/operation là định danh ngẫu nhiên. FK hạn chế xóa dữ liệu lịch sử, không cascade xóa hồ sơ/điểm/Invoice/Payment/audit. Mã học viên hiển thị là ID nội bộ, không tạo mã tự nhập mới; Course/Class code vẫn tách khỏi PK. Chỉ bỏ cấu hình nháp chưa dùng khi không có phụ thuộc và quyền hiện có cho phép.
- C10: Đặt unique cho email chuẩn hóa/SĐT có giá trị, code khóa-lớp, cặp khóa-Band, tên thành phần trong chương trình, hồ sơ-số lượt, assignment/hồ sơ, điểm/học viên-thành phần, điểm danh/học viên-buổi, Invoice/hồ sơ và Payment/Invoice. Tên thành phần so không phân biệt hoa-thường nhưng có phân biệt dấu; không unique Payment reference. Index FK/cột lọc trạng thái-ngày/Teacher/phòng và phân trang; kiểm tra đúng vai trò/cùng lớp-chương trình bằng server trong transaction, không chỉ FK tồn tại.
- C15: Khi tạo Class, khóa kiểm tra/cấu hình TrainingProgram cùng transaction và từ đó suy khóa bất biến bằng sự tồn tại của bất kỳ Class, kể cả CANCELLED. Bộ Band bị khóa khi có bất kỳ kết quả đầu vào COMPLETED; tuần tự hóa thao tác cấu hình và chấm đầu tiên. Không nút mở khóa/xóa lịch sử để né freeze, không thêm bảng version hoặc snapshot hồ sơ cá nhân.
- C16: Số hóa đơn server cấp HD-YYYY-NNNNNNNN, năm theo ngày phát hành Việt Nam, bộ đếm DB tăng an toàn; không yêu cầu liên tục không khoảng trống/không tái dùng. Quan hệ một hồ sơ tối đa một Invoice, một Invoice tối đa một Payment, một assignment tối đa một GradeReport; các FK nghiệp vụ bắt buộc NOT NULL khi đối tượng đó tồn tại, actor có thể NULL chỉ với sự kiện chưa xác thực. Score chưa nhập là thiếu bản ghi, không 0; finalized_score/Band/Payment chỉ có khi đạt điều kiện tương ứng. Dictionary cuối sẽ ghi rõ từng trường, không mặc định tất cả nullable.

### ADR-091 — Specify exact numeric, time and normalized text representations

- C07: Tiền đơn decimal(18,0), dương và không vượt 999.999.999.999.999.999 VND; tổng báo cáo decimal(38,0), API tiền dạng chuỗi số để frontend không mất chính xác. Điểm/trọng số decimal(5,2): điểm 0–100, trọng số >0 đến 100/tổng đúng 100; số LESSON int dương. Tính tổng điểm bằng decimal trung gian rồi half-up một lần như ADR-028; từ chối vượt range, không tự cắt/làm tròn đầu vào.
- C08: Các thời điểm sự kiện lưu UTC datetime2(3), API ISO-8601 có Z. Ngày sinh/hạn thu dùng date; lịch dùng ngày Việt Nam và time(0), ràng buộc phút/same-day B03, chuyển UTC khi so thời điểm sự kiện. Backend áp Asia/Ho_Chi_Minh cho ranh giới ngày/kỳ; không phụ thuộc timezone máy học viên hoặc server.
- C09: Đếm ký tự theo Unicode code point, không byte/UTF-16 unit; từ chối surrogate không hợp lệ, chuẩn hóa NFC văn bản không phải mật khẩu, cấp nvarchar đủ dung lượng tương ứng. Mật khẩu giữ nguyên, không trim/NFC/đổi hoa-thường. Email nhận một địa chỉ ASCII dạng local@domain, không tên hiển thị/khoảng trắng/ký tự điều khiển; trim/so trùng không phân biệt hoa-thường nhưng không bỏ dấu/chấm/+tag. SĐT chuẩn 0xxxxxxxxx từ quy tắc đã duyệt; chuỗi tùy chọn trim rỗng thành NULL, trường bắt buộc rỗng báo lỗi.

### ADR-092 — Persist revocable authentication sessions and independent login timers

- C11: Thêm AuthSession với sid ngẫu nhiên, account_id, issued/expires/last_activity/revoked_at và phiên hạn chế nếu có; Account có security_version. JWT mang sid/version, mọi yêu cầu kiểm tra DB/status/hạn phiên; reset/đổi mật khẩu/khóa/ngừng tăng version và hủy phiên theo ADR-049. Không chỉ đợi JWT hết hạn, không Redis hoặc module quản lý thiết bị.
- C12: Lưu failed_login_count, failure_window_started_at và login_throttled_until độc lập status tài khoản; tính UTC/cập nhật nguyên tử. Lưu must_change_password/temp_issued_at/temp_expires_at, chỉ hash mật khẩu; không lưu bản rõ để Admin xem lại. Throttle/expiry không tự unlock/enable; giữ nguyên 5 lần/15 phút và mật khẩu tạm 24 giờ đã duyệt.

### ADR-093 — Group audit history and serialize critical writes with replay protection

- C05: Lưu actor/time/reason/prior-state ở đối tượng khi cần, AuditLog giữ trường cũ/mới theo allowlist. Dùng operation_id chung cho thao tác mẹ và thay đổi phụ thuộc, gồm mọi GradeReport trong gửi/gửi lại/trả/chốt cả lớp. Giữ lịch sử lần gửi/trả/chốt, không chỉ timestamp cuối; không thêm workflow lịch sử đổi giáo viên lớp hoặc nút sửa/xóa log.
- C13: Transaction ngắn; rowversion phát hiện form cũ, đồng thời khóa tuần tự tài nguyên/capacity/đối tượng để xử lý đặt ca-xếp lớp-thanh toán-hủy-chấm/xác nhận/đổi cấu hình và thu hồi phiên. Không coi rowversion một mình chống được trùng lịch. Thao tác nghiệp vụ quan trọng có request_id theo actor/action, lưu dấu kết quả cùng transaction tối thiểu 24 giờ: gửi lại cùng nội dung không tạo tác động mới, khác nội dung báo 409; kiểm tra quyền hiện tại khi nhận lại kết quả. Không lưu/phát lại mật khẩu tạm. Cơ sở kỹ thuật: [EF Core concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency).

### ADR-094 — Read consistent report snapshots and display two-decimal rates

- C14: Mỗi lượt báo cáo/export đọc một snapshot DB nhất quán, trả queried_at; CSV chạy một truy vấn mới theo cùng bộ lọc/cột, không hứa giữ nguyên ảnh chụp trang xem trước. Tỷ lệ đạt/tiến độ hiển thị 2 chữ số lẻ half-up, không làm tròn các thành phần đếm; mẫu số rỗng giữ “Chưa có dữ liệu”. Giữ nguyên nhóm thời gian/trạng thái hiện tại và tổng distinct đã duyệt.

## 4.2. Gói 1 frontend được duyệt — 29/09/2026

Owner duyệt các đề xuất Gói 1, gồm sửa cách diễn đạt D15. Gói 2 ban đầu được
bàn từng phần: FE01a/FE01b/FE02 duyệt ADR-103–ADR-105 (mục 4.3–4.5).
Owner sau đó đồng ý hoàn toàn Gói 2 FE01–FE10, ghi thêm ADR-106–ADR-113
(mục 4.6); chi tiết đã chốt về shell/menu/PII giữ nguyên. Gói 3 đã duyệt ở mục 4.8.
Các lựa chọn sau là contract thiết kế, chưa có code hoặc test đã chạy.

### ADR-95 / D01 — Module và trách nhiệm server

Một backend modular monolith dùng stack và tên module mục tiêu hiện có: Students quản lý hồ sơ riêng; Admissions xử lý đăng ký/thi đầu vào; Academics quản lý cấu hình/lớp/lịch/điểm; Finance quản lý hóa đơn/thanh toán; Reports đọc báo cáo; Administration quản lý tài khoản/phiên/audit. API DTO → application workflow/transaction → EF Core/SQL Server; quy tắc/quyền/tiền/tổng điểm do server quyết định, frontend chỉ nhập/hiển thị. React tổ chức theo chức năng, không microservice/event bus/framework kiến trúc mới; liên module cùng DB để giữ all-or-none.

### ADR-96 / D02 — API, lỗi, phân trang và ownership

API /api/v1; DTO riêng, lỗi code/message/fieldErrors/traceId, không stack trace/SQL/payload bí mật. 400 dữ liệu sai, 401 chưa/hết xác thực, 403 sai vai trò, 404 bản ghi không thuộc phạm vi được xem, 409 trạng thái/cạnh tranh; danh sách mặc định 20, tối đa 100/trang, sort ổn định có ID phụ. Server kiểm tra role và ownership ở mọi endpoint, không chỉ ẩn nút.

### ADR-97 / D03 — Hoạt động idle và phiên chung giữa tab

Chỉ request nghiệp vụ được xác thực do người dùng thao tác mới cập nhật last_activity; không background polling/healthcheck/timer giữ phiên. Các tab cùng browser profile chia cookie/phiên, đăng xuất ở một tab hủy phiên chung; thiết bị/profile khác có phiên riêng. Giữ 2 giờ tuyệt đối/30 phút idle, không refresh; không tạo heartbeat chỉ từ di chuột/gõ phím.

### ADR-98 / D04 — Phiên hạn chế đổi mật khẩu tạm

Đăng nhập bằng mật khẩu tạm còn hạn chỉ cấp phiên hạn chế tối đa 10 phút, không vượt hạn 24 giờ của mật khẩu; chỉ được đổi mật khẩu tạm/đăng xuất, không nghiệp vụ. Sau đổi, hủy mọi phiên rồi về đăng nhập bình thường; không tự nâng phiên hạn chế thành phiên thường. Reset/hết hạn vẫn do Admin, không email khôi phục.

### ADR-99 / D06 — Cookie JWT, HTTPS, CSRF và dữ liệu cá nhân

JWT ký/kiểm issuer-audience-exp/sid-version, đặt cookie host-only HttpOnly/Secure/SameSite=Strict, không localStorage/sessionStorage. Frontend/API cùng origin; yêu cầu đổi dữ liệu và login có CSRF token/kiểm Origin, không thao tác nghiệp vụ qua GET. HTTPS, escape văn bản thuần, truy vấn tham số hóa, secret ngoài mã nguồn, không lưu hồ sơ/điểm/tokens bền trong browser cache hoặc log đầy đủ. Nguồn: [OWASP Session](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html), [OWASP CSRF](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html).

### ADR-100 / D07 — Lỗi đăng nhập và giới hạn IP

Thông báo đăng nhập chung, không tiết lộ email có tồn tại hoặc trạng thái trước xác thực. Bổ sung giới hạn request login theo IP: 100/phút, vượt trả 429/Retry-After; đây là ngưỡng hạ tầng đề xuất, tách khỏi throttle tài khoản 5 lần/15 phút đã duyệt và không hủy phiên. IP chỉ tin từ proxy được cấu hình; không CAPTCHA/OTP/module cảnh báo mới.

### ADR-101 / D10 — CSV tài chính và giới hạn/an toàn ô

CSV UTF-8 BOM, dấu phẩy, CRLF, quote/escape đúng ô; cùng bộ lọc/cột và toàn bộ kết quả theo ADR-072. Giới hạn 10.000 dòng dữ liệu hoặc 5 MB: vượt thì từ chối toàn file và đề nghị thu hẹp lọc, không xuất âm thầm một phần. Ô văn bản nguy hiểm công thức kể cả ký tự full-width được thêm dấu nháy đơn bên trong ô đã quote; giữ dữ liệu DB gốc, kiểm thử trên Excel/LibreOffice mục tiêu và cảnh báo không bảo đảm an toàn sau chỉnh/lưu/mở lại ở mọi phần mềm. Không XLSX/PDF/quyền xuất mới. Nguồn: [OWASP CSV Injection](https://community.owasp.org/attacks/CSV_Injection).

### ADR-102 / D15 — Responsive theo vai trò và phạm vi kiểm chứng

Thiết kế bộ kiểm chứng xUnit nghiệp vụ/API, integration trên SQL Server test riêng cho ràng buộc/transaction/races; kiểm tra thủ công 7 vai trò và luồng đầu-cuối/boundary thời gian/số tiền/CSV. UI tiếng Việt; luồng Học viên ưu tiên 375 px, màn hình nghiệp vụ dày của nhân sự ưu tiên 1280 px; tất cả dùng được và kiểm tra ở 375/768/1280 px, nhãn/lỗi rõ, tab bằng bàn phím, không chỉ dựa màu; kiểm thử Chrome/Edge bản ổn định hiện hành. Không thêm test framework, test code hoặc tuyên bố kiểm thử đã chạy.

## 4.3. Khung giao diện chung FE01a được duyệt — 29/09/2026

ADR-103: dùng chung một khung cho 7 vai trò, hiển thị menu đúng quyền.
1280 px: sidebar trái khoảng 240 px, icon/tên mục và header trên nội dung.
768 px: menu trái thu gọn khoảng 72 px, có nhãn khi hover/focus và nút mở rộng.
375 px: header gọn/nút menu/drawer trượt từ trái phủ nội dung, trang một cột.
Header hiển thị tên màn hình/khu vực tài khoản; mobile đưa tên/vai trò vào
menu tài khoản ADR-084. Nội dung gồm tiêu đề, hành động chính và dữ liệu.
Menu đang chọn/focus rõ; drawer đóng khi chọn mục/chạm nền/Escape và trả
focus về nút mở. FE01b nhóm/tên menu theo vai trò đã duyệt ADR-104.

## 4.4. Menu theo vai trò FE01b được duyệt — 29/09/2026

ADR-104: thứ tự sau cũng là thứ tự menu; đăng nhập mới mở mục đầu của vai trò.

| Vai trò | Nhóm menu và màn hình |
| --- | --- |
| Học viên | Trang chủ F03; Khóa học & hồ sơ F04–F05; Thi đầu vào F06–F09; Hóa đơn F10; Lớp & lịch F11; Bảng điểm F12. |
| Tuyển sinh | Hồ sơ tuyển sinh F13–F14. |
| Giáo vụ | Ca thi đầu vào F16–F17; Khóa & chương trình F18; Lớp học F19–F20; Lịch học & thi F21; Xếp lớp F22; Duyệt bảng điểm F23. |
| Giáo viên | Lớp & lịch dạy F24; Thi đầu vào F25; Điểm danh F26; Sổ điểm F27. |
| Kế toán | Hóa đơn & thanh toán F28–F29; Báo cáo học phí F30. |
| Giám đốc | Báo cáo tổng hợp F31. |
| Admin | Tài khoản F32; Nhật ký F33. |

Thi đầu vào của Học viên chứa đặt ca/lịch/kết quả-Band/xác nhận nhập học.
Khóa & chương trình của Giáo vụ gồm Band/trọng số; Báo cáo tổng hợp chứa
tuyển sinh/đào tạo/tài chính. Các hành động tạo/sửa/duyệt/thanh toán nằm
trong màn hình tương ứng. F01/F02 là xác thực trước đăng nhập; menu tài
khoản ADR-084 nằm riêng ở header. Không tạo mã F/dashboard nhân sự mới;
quyền và guard đã chốt giữ nguyên. FE02 cấu trúc trang/đường đi ở mục 4.5.

## 4.5. Cấu trúc trang và đường đi FE02 được duyệt — 29/09/2026

ADR-105: danh sách, chi tiết và form phức tạp có URL riêng, hỗ trợ mở trực
tiếp, tải lại và Back/Forward; chưa chọn thêm thư viện. Chi tiết có khối
tóm tắt đối tượng/trạng thái và các phần hoặc tab dữ liệu đúng quyền.
Form nhiều trường như chương trình/lớp/lịch/sổ điểm dùng trang riêng;
modal dành cho xác nhận, lý do hoặc form ngắn, mobile có thể gần toàn màn hình.
Quay lại danh sách giữ bộ lọc/trang/sắp xếp. Bộ lọc không định danh
(khóa/Band/trạng thái/kỳ ngày), page/sort có thể ở URL; tên/email/SĐT chỉ
giữ trong bộ nhớ khi sử dụng, không đưa vào URL/localStorage/sessionStorage.
Danh sách/chi tiết có breadcrumb và đường quay lại rõ. Mọi URL giữ quyền,
ownership và hợp đồng lỗi D02; không thay đổi quyền hoặc workflow nghiệp vụ.

## 4.6. Toàn bộ Gói 2 FE01–FE10 được duyệt — 29/09/2026

Owner xác nhận đồng ý toàn bộ đề xuất Gói 2 gốc trong cùng chat. Bổ sung
FE03–FE10 ở OWNER_REVIEW và ADR-106–ADR-113; không suy từ file tự host.
Giữ FE01a/FE01b/FE02 đã chốt, không bottom navigation/role switcher thật,
không infinite scroll. Gói 3 FE11–FE19 duyệt riêng ADR-121–ADR-129 (mục 4.8);
không nghiệm thu E hoặc code.

### FE03 / ADR-106 — Danh sách responsive

1280 px dùng bảng đầy đủ đúng quyền; 768 px rút gọn cột và mở chi tiết riêng;
375 px dùng card/list có nhãn trường, tránh bảng ngang. Sổ điểm desktop dùng
ma trận với cột học viên cố định, mobile chuyển sang từng học viên. Phân trang
theo D02, giữ context/PII ADR-105. Đơn vị lưu điểm/điểm danh chốt riêng FE14,
ADR-124 (mục 4.8), không suy từ FE03.

### FE04 / ADR-107 — Quyền, điều kiện và chỉ xem

Ẩn hành động hoàn toàn ngoài quyền. Hành động đúng vai trò nhưng chưa đủ
điều kiện thì disabled và giải thích cụ thể. Dữ liệu chỉ xem ghi “Chỉ xem”,
không giả dạng form bị khóa. Cảnh báo tài khoản phụ trách LOCKED/DISABLED,
không đề xuất đổi người ngoài cửa sổ đã chốt. Server vẫn kiểm tra mọi guard;
lý do không tiết lộ dữ liệu ngoài quyền.

### FE05 / ADR-108 — Form và validation

Không autosave; nút lưu rõ. Client kiểm tra sớm nhưng server kết luận; lỗi
trường dưới trường và lỗi toàn form ở đầu. Cảnh báo rời trang còn thay đổi
chưa lưu, không hứa phục hồi sau tắt máy/hết phiên. Chuyển hoa mã khóa/lớp/
phòng khi rời trường trong phạm vi được phép; mã đã tạo vẫn bất biến.
Không tự cắt/làm tròn giá trị vượt giới hạn hoặc chuẩn hóa mật khẩu. SĐT
nhập dạng hợp lệ, lưu xong hiển thị chuẩn. Không tự merge/ghi đè form cũ hoặc
lưu nháp nhạy cảm bền trong browser. Đơn vị lưu batch FE14 chốt riêng ADR-124.

### FE06 / ADR-109 — Loading, empty, error, submitting

Tải lần đầu dùng skeleton theo cấu trúc; tách chưa có dữ liệu và không có
kết quả theo bộ lọc. Submit khóa nút/hiện tiến trình, không nhấn lặp; vẫn giữ
chống ghi lặp server ADR-093 và kiểm tra trạng thái sau mất phản hồi ghi.
409 giải thích xung đột dữ liệu/trạng thái, yêu cầu tải lại/kiểm tra, không
ghi đè. 401/hết phiên xác thực về đăng nhập với lý do rõ; 403 không đủ quyền;
404 chung không tồn tại/ngoài phạm vi. Lỗi đăng nhập trước xác thực vẫn chung
theo D07. Thành công thông báo ngắn; lỗi quan trọng không chỉ toast dễ mất.
Giữ fieldErrors/traceId D02, không lộ bí mật/payload/stack.

### FE07 / ADR-110 — Xác nhận nhạy cảm

Một mẫu chung có đối tượng/trạng thái hiện tại, ảnh hưởng trực tiếp và dữ
liệu phụ thuộc. Lý do chỉ nơi đã bắt buộc, nút dùng đúng động từ nghiệp vụ
thay “OK”. Không hai lớp xác nhận cho thao tác thông thường; mật khẩu tạm
hiển thị một lần là ngoại lệ riêng; thiết kế bàn giao/copy/đóng FE18 đã duyệt
riêng ADR-128 (mục 4.8). Không thêm người duyệt thứ hai, lý do/quyền mới;
commit vẫn kiểm tra lại.

### FE08 / ADR-111 — Component cục bộ

Tailwind và component cục bộ có khả năng truy cập; chưa thêm UI/form/table/
calendar/chart library. Chỉ xin package mới khi nhu cầu cụ thể không thể đáp
ứng hợp lý. Không đổi dependency đã thuộc stack; chưa tạo component/code.

### FE09 / ADR-112 — Ngôn ngữ và định dạng

Nhãn trạng thái tiếng Việt, enum tiếng Anh nội bộ. Ngày dd/MM/yyyy, giờ 24 giờ
HH:mm; ghi “Giờ Việt Nam” nơi dễ nhầm. Hiển thị vi-VN như 1.234.567 ₫ và
75,50/100; tiền API vẫn chuỗi chữ số, không qua kiểu số làm mất chính xác.
Không đổi range/precision/rounding/validation/timezone; định dạng không tự tính
lại hoặc thay dữ liệu lưu.

### FE10 / ADR-113 — Phong cách

Giao diện hành chính–học vụ sáng, gọn; font hệ thống, nền trắng/xám và màu
chính xanh. Trạng thái đỏ/vàng/xanh có chữ hoặc icon; focus rõ, vùng chạm
tối thiểu khoảng 44 px, hỗ trợ giảm chuyển động. Không là bộ nhận diện mới
hoặc bằng chứng accessibility đã đạt; contrast/keyboard vẫn phải kiểm chứng.

## 4.7. Bảo mật và vận hành bổ sung — ADR-114–ADR-120

Ngày 29/09/2026 owner tích D05/D08/D09/D11/D14/D16 trong OWNER_REVIEW,
sau đó trả lời đồng ý phụ trách toàn bộ D17. Đây là lựa chọn thiết kế được
duyệt, không là bằng chứng đã cài đặt, chạy kiểm chứng hoặc nghiệm thu E.
D12/D13 giữ chưa duyệt, hoãn đến khi chốt nơi chạy web/phần mềm.

### D05 / ADR-114 — Hash, mật khẩu tạm và blocklist

Dùng PasswordHasher của ASP.NET Core với tham số theo runtime được chọn,
ghi rõ khi khóa dependency triển khai. Mật khẩu tạm 32 ký tự sinh bằng bộ
ngẫu nhiên mật mã, hiển thị Admin một lần, không lưu/log plaintext. Blocklist
offline hợp nhất SecLists 10k-most-common và probable-v2_top-12000; pin revision,
giữ giấy phép khi tích hợp, rà cập nhật mỗi quý. Không gửi mật khẩu ra dịch vụ
ngoài; danh sách hữu hạn không phát hiện mọi mật khẩu từng lộ. Không đổi cách
đếm/Unicode/khoảng trắng/expiry/quy tắc mật khẩu; bàn giao UI FE18 đã duyệt
riêng ADR-128 (mục 4.8).

### D08 / ADR-115 — Lỗi ghi audit

Audit nghiệp vụ/thay đổi tài khoản bắt buộc cùng transaction với thao tác;
lỗi ghi rollback toàn bộ, gồm trạng thái/credential/thu hồi phiên liên quan.
Sự kiện đăng nhập/đăng xuất/từ chối quyền ghi best-effort với fallback vận
hành có giới hạn/không bí mật. Không bỏ kiểm tra bảo mật hoặc khôi phục phiên
logout đã hủy vì log lỗi. Ghi nhận sự cố để người vận hành xử lý, không thêm
hệ thống thông báo cho người dùng.

### D09 / ADR-116 — Lưu giữ, IP và chống ngập log

Audit nghiệp vụ/thay đổi tài khoản giữ cùng lịch sử dự án, không nút xóa trong
app. Log xác thực/quyền thuần túy và IP giữ 90 ngày rồi dọn bằng vận hành;
không xóa audit bắt buộc để áp hạn này. Chỉ lưu IP cho sự kiện bảo mật cần
thiết; bỏ IP hết hạn nhưng giữ phần audit bắt buộc. Ghi mọi sự kiện thực sự
xử lý; request đã bị rate-limit có thể gộp số lần/IP/cửa sổ phút, không bỏ
bộ đếm throttle. Actor chưa xác thực là NULL, không email nhập. Đây là chính
sách dự án được duyệt, không kết luận tuân thủ pháp lý.

### D11 / ADR-117 — Phạm vi kiểm chứng tải

Giả định một trung tâm, tối đa 10.000 học viên và 50 người dùng đồng thời;
giữ lịch sử nhiều khóa/lượt. Không multi-branch, đa tenant hoặc kiến trúc
phân tán. Đây là phạm vi kiểm chứng dự kiến, không số người dùng thực tế,
kết quả đo hoặc ràng buộc chặn học viên thứ 10.001; D12 vẫn hoãn.

### D14 / ADR-118 — Backup và phục hồi

Khi đã có hệ thống, backup SQL hằng ngày, mã hóa và giữ bản riêng khỏi ổ dữ
liệu chính; giữ 7 bản ngày/4 bản tuần, thử restore mỗi tháng. Mục tiêu trên
môi trường tham chiếu: RPO tối đa 24 giờ, RTO tối đa 2 giờ, chưa là bảo đảm
đã đo. Owner chỉ định nơi lưu/quyền vận hành trước khi chạy thật; người phụ
trách mặc định đã chốt D17. Quy trình phải xử lý lịch backup khi laptop tắt/
bỏ lỡ lần chạy, không tự hạ tần suất hoặc tuyên bố đạt mục tiêu. Chưa backup,
sao chép dữ liệu hoặc restore ở giai đoạn thiết kế này.

### D16 / ADR-119 — Phiên bản và phạm vi cấp phép

Chọn .NET 10 LTS/EF Core 10 và SQL Server 2022 Developer cho dev/test/demo
nội bộ không dùng dữ liệu/vận hành production. Giữ React/Vite/TypeScript/
Tailwind/xUnit. Sau nghiệm thu toàn bộ baseline, chốt patch tương thích
được hỗ trợ và lock dependency trong hợp đồng triển khai; không hỏi riêng
mỗi patch. Production phải chọn edition/license riêng trước khi vận hành.
Chưa cài đặt, pin patch hoặc xác minh tương thích máy.

### D17 / ADR-120 — Người vận hành mặc định

Chủ dự án phụ trách toàn bộ release, secret, backup/restore và bảo trì đồ án.
Đã hỏi và chốt một lần; chỉ hỏi lại người phụ trách khi owner yêu cầu thay
đổi. Đây là trách nhiệm vận hành, không thêm quyền Admin ứng dụng hoặc DBA.
Bàn giao tài liệu setup/cấu hình, ma trận quyền, reset/thu hồi phiên, phục hồi/
xử lý lỗi và truy vết yêu cầu → workflow → data/API/test. Rà dependency/
secret/blocklist định kỳ trước release, giữ nhịp rà blocklist mỗi quý D05.
Không tự kích hoạt integration/coding agent hoặc gửi dữ liệu dự án ra ngoài.


## 4.8. Gói 3 FE11–FE19 đã được duyệt — 29/09/2026

Owner xác nhận trực tiếp ghi nhận toàn bộ Gói 3, không nêu ngoại lệ. Cập nhật
chín ô FE11–FE19 trong OWNER_REVIEW và lưu ADR-121–ADR-129; không suy từ
ô trống hoặc từ Gói 2. Các quy tắc dưới đây đã được duyệt, chưa triển khai.
Giữ 7 vai trò/32 mã F, không thêm module, quyền, dependency hoặc nghiệm thu E.

### FE11 / ADR-121 — Trang chủ Học viên F03.

Dùng các thẻ theo từng hồ sơ/khóa của chính học viên: bước hiện tại, việc có thể làm tiếp, ca thi/buổi học gần nhất, hóa đơn chưa thanh toán/quá hạn và liên kết kết quả đã FINALIZED. Mỗi hồ sơ có nút dẫn tới thao tác hợp lệ hoặc lời giải thích đang chờ chấm/duyệt/thanh toán/xếp lớp; không ép một tiến trình chung khi học nhiều khóa. Tiến trình chỉ là cách trình bày dữ liệu hiện có, không enum/điểm tiến độ mới; không hiện điểm DRAFT/SUBMITTED/RETURNED cho học viên. Chưa có hồ sơ thì dẫn tới chọn khóa. Không thêm dashboard nhân sự, hộp thư hoặc thông báo tự động.

### FE12 / ADR-122 — Lịch thi/học/dạy/quản lý.

Dùng component lịch cục bộ tái sử dụng trong F07/F11/F21/F24, chỉ dữ liệu đúng scope từng màn hình. Desktop có lịch tuần và danh sách theo ngày; 375/768 px ưu tiên danh sách theo ngày, nút Hôm nay/chọn ngày và lọc lớp của mình hoặc lớp được phép quản lý. Click buổi mở chi tiết và hành động hợp lệ; không kéo-thả/resize để đổi lịch, không thêm calendar library. Phân biệt LESSON/FINAL_EXAM/ca đầu vào bằng chữ kèm màu. Lịch Học viên giữ Hiện tại/Lịch sử đã chốt; lịch bị hủy/NOT_HELD nằm ở lịch sử, liên kết buổi thay thế và không tính đôi. Chọn ngày không tự suy buổi đã HELD; không đổi quyền đặt ca CLOSED hoặc cửa sổ sửa lịch.

### FE13 / ADR-123 — Xem trước tạo lịch tuần F21.

Hai bước Nhập quy tắc → Xem trước: khoảng ngày/thứ/giờ/phòng cho LESSON, danh sách ngày cụ thể, số nghĩa vụ đã có/còn thiếu/sắp tạo và xung đột phòng/giáo viên/lớp/học viên trong phạm vi được xem. Nút Tạo toàn bộ lịch chỉ khả dụng khi batch hợp lệ; sửa đầu vào làm mất hiệu lực preview. Có lỗi thì quay lại sửa/tạo lại preview hoặc dùng luồng thêm buổi riêng, không tự bỏ dòng lỗi/ngày nghỉ hoặc lưu phần hợp lệ. Server kiểm tra lại lúc commit; preview không giữ chỗ tài nguyên. 409 yêu cầu xem trước lại, không ghi đè. FINAL_EXAM quản lý riêng, không tự sinh kèm LESSON; không thêm quy tắc lặp lưu DB/automation.

### FE14 / ADR-124 — Đơn vị lưu điểm danh và điểm nháp F26/F27.

Điểm danh: nút Lưu điểm danh buổi gửi các giá trị đã đánh dấu/thay đổi của một LESSON trong một batch all-or-none; có lỗi thì không lưu phần nào. Ban đầu Chưa ghi, giáo viên chọn Có mặt/Vắng; không mặc định cả lớp có mặt/vắng, không biến chưa ghi thành dữ liệu. Có thể lưu khi còn người chưa ghi, vẫn chặn hoàn thành lớp đến khi đầy đủ; Chưa ghi không là enum Attendance mới. Điểm nháp: Lưu học viên lưu các điểm thành phần đã nhập/thay đổi và nhận xét của một học viên cùng nhau, all-or-none cho lần lưu đó; desktop nút từng hàng, mobile cùng đơn vị trong trang học viên. Cho thiếu thành phần khi lưu nháp; ô chưa có điểm không thành 0, bỏ trống không tự xóa điểm đã lưu. Tổng từ server, thiếu điểm ghi Chưa đủ điểm. Hiện Chưa lưu/Đã lưu, cảnh báo rời khi còn sửa; không autosave/import Excel/lưu browser bền/merge form cũ. Gửi bảng điểm toàn lớp là thao tác riêng FE15, không đổi cửa sổ/quyền hay thêm quyền xóa điểm.

### FE15 / ADR-125 — Gửi, trả lại và chốt bảng điểm cả lớp F27/F23.

Tách rõ Lưu nháp và Gửi/Gửi lại bảng điểm cả lớp. Giáo viên xem trạng thái lưu/thiếu thành phần theo từng học viên; chặn gửi khi còn sửa chưa lưu, lớp chưa COMPLETED, điểm thiếu hoặc trạng thái chưa hợp lệ. Giáo vụ xem bảng SUBMITTED/RETURNED/FINALIZED theo quyền, không giá trị DRAFT. Gửi/trả/chốt dùng một confirmation có mã lớp, số học viên, trạng thái và ảnh hưởng; trả lại bắt buộc lý do và giữ điểm/nhận xét. Không checkbox chọn riêng người gửi/trả/chốt, không duyệt hai tầng. Server kiểm tra mọi học viên và chuyển toàn lớp hoặc không ai; 409 tải lại/rà lại, không tự ghi đè. Student chỉ thấy FINALIZED; chốt bất biến, không thêm mở khóa/đảo trạng thái.

### FE16 / ADR-126 — Trình bày báo cáo F30/F31.

Ưu tiên thẻ chỉ số và bảng chi tiết nhóm, chưa cần biểu đồ hoặc chart library. F31 có Tuyển sinh/Đào tạo/Tài chính; F30 chỉ tài chính theo quyền. Nút Áp dụng bộ lọc chạy query mới; khi đang sửa lọc, kết quả cũ ghi rõ bộ lọc đang áp dụng và queried_at, không trộn hai lần query. Tách doanh thu/tổng phải thu trong kỳ, công nợ hiện tại không kỳ ngày, tiến độ LESSON/FINAL_EXAM và kỳ kết quả chốt. Mẫu số rỗng là Chưa có dữ liệu, không 0%; tổng từ toàn tập lọc, không cộng trang/nhóm distinct. Director chỉ drill-down nhóm không định danh, không export. Accountant CSV theo bộ lọc đã áp dụng/toàn tập/cột đã chốt, query mới có thể khác trang trước, giữ giới hạn/an toàn D10. Không thêm metric, profile dump, xuất XLSX/PDF, lưu report-as-of hoặc quyền báo cáo mới.

### FE17 / ADR-127 — In hóa đơn của Học viên F10.

Nút In hóa đơn chỉ cho học viên sở hữu; lấy lại dữ liệu hợp lệ trước khi mở bản in, lỗi/không còn quyền thì không in bản cũ như dữ liệu mới. Bố cục A4 dọc gọn/đen-trắng rõ, ẩn menu/nút; chỉ số hóa đơn, tên hiện tại của chủ hồ sơ, khóa, tiền snapshot, ngày phát hành/hạn và trạng thái được phép xem. UNPAID ghi Chưa thanh toán — không phải xác nhận đã thu tiền; CANCELLED ghi Đã hủy; PAID ghi Đã thanh toán đúng dữ liệu. Cảnh báo quá hạn theo quy tắc hiện có, không tự đánh PAID khi bấm in. Không thêm mẫu biên lai, chữ ký/đóng dấu giả, trường Payment ngoài scope, API/file PDF hoặc quyền in cho nhân sự; dùng chức năng in của trình duyệt.

### FE18 / ADR-128 — Bàn giao mật khẩu tạm một lần F32.

Sau cấp/reset thành công, hiển thị khối kết quả riêng trong F32 có email/vai trò, mật khẩu tạm và hạn 24 giờ từ server, cảnh báo không thể xem lại. Nút Sao chép chỉ ghi clipboard khi Admin chủ động bấm và trình duyệt cho phép; nếu bị chặn thì chọn chữ/sao chép thủ công. Không tự copy/đọc clipboard/gửi email/in hoặc tải file secret; cảnh báo clipboard có thể còn giữ mật khẩu sau đóng, không hứa tự xóa clipboard. Có checkbox Tôi hiểu sau khi đóng sẽ không xem lại được và nút Đã ghi nhận — Đóng; không coi tick là bằng chứng người dùng đã nhận mật khẩu. Đóng/rời màn hình/đăng xuất/hết phiên xóa plaintext khỏi state, không URL/storage/cache/log/replay secret; refresh/tắt tab có thể làm mất và không cam kết ngăn được. Mất phản hồi cấp/reset: kiểm tra trạng thái trước, không tự lặp để lấy secret; nếu không nhận được mật khẩu thì Admin xác nhận reset mới có chủ đích. Không tự mở khóa tài khoản; không đổi chính sách generator/expiry/quyền đã chốt.

### FE19 / ADR-129 — Chi tiết nhật ký Admin F33.

Giữ danh sách từng sự kiện mới nhất trước/phân trang và bộ lọc đã chốt; chi tiết mở riêng với thời gian/người/hành động/đối tượng/kết quả/lý do, trường cũ → mới theo allowlist. Sự kiện chưa xác thực ghi Chưa xác thực, không gán email nhập làm actor. Chi tiết có liên kết xem các sự kiện cùng operation_id để hiểu thao tác cả lớp/hủy phụ thuộc, vẫn phân trang/đúng quyền và không đổi đơn vị trang thành nhóm tùy ý. Không raw request-response/secret/full payload, không sửa/xóa/khôi phục dữ liệu hoặc xuất file. IP chỉ khi được lưu hợp lệ và chưa hết hạn; đã dọn thì ghi Không còn lưu, không dựng lại IP. Bản ghi gộp rate-limit nếu có ghi số lần/cửa sổ, không giả thành từng actor đã xác thực.

## 4.9. RF01/RF02/RF04 đã được duyệt — 03/10/2026

Owner duyệt ba mục riêng, ghi ADR-157/158/159. RF03 giữ ADR-156.
Đây là quyết định cutoff/no-change, không nghiệm thu toàn E04/E05 hoặc chốt
codec/actions/signatures/shape mới chưa rà. Không sửa schema E03 hoặc Word.

- RF01 / ADR-157: AcademicAffairs chỉ tạo EntryTestSession khi startAt>serverNow
  tại kiểm tra lưu trong transaction; equality/passed409, malformed date/time400.
  Không backfill ca cũ; mới CLOSED, capacity/resources/current Teacher và mọi
  booking/grading windows giữ nguyên. Preview/RV không đóng băng clock.
- RF02 / ADR-158: new intent đổi sang grader hiện tại hoặc correction bằng
  cùng parsed score trả400 với field error graderId/score sau auth/task/valid
  current-context checks; không mutation/provenance/audit/completion mới.
  Stale/state/time/membership409 vẫn ưu tiên, reason/original-current-grader/
  pre-confirmation locks giữ nguyên. Completed same-key replay là nhánh riêng.
- RF04 / ADR-159: các value edits còn lại bằng toàn bộ business values trả400
  không effects/marker mới. Với Attendance/grade-draft deltas, kiểm **toàn bộ**
  request/items/versions/absence/cohort/relationships/state/clock trước equality;
  mixed batch lưu actual changes cùng save unit, unchanged giữ provenance,
  invalid/stale một item rollback tất cả. All-equal400, no marker; new row0
  vẫn actual change. TeacherComment omitted giữ khác clearNULL; clear đangNULL
  là unchanged. Parent report RV chỉ touch cho actual score/comment mutation.
  Wrong-source state transitions bằng key mới409, không fake completion.
  Create không coi row tồn tại là create success; Admin reset chủ đích vẫn
  cấp credential mới/revoke theo policy, completed replay không reset.

RF04 value-edit scope: Booking reschedule, Payment metadata, Session schedule,
Course details, Program config/name, Class capacity/Teacher, Schedule move.
Reason một mình không tạo thay đổi. Không thêm quyền/tables/fields/SQL objects/
result modes hoặc history bypass; real mutations vẫn audit/rollback đã chốt.
Wire literals NO_CHANGE/STATE_CONFLICT và actual E04 refinements nay Accepted
ADR-160; behavior400/409 vẫn ADR-158/159,không hỏi lại RF01/02/04.

## 4.10. Nghiệm thu actual E04-v1 và complete baseline — 03/10/2026

Owner nghiệm thu [E04-v1 actual package](design/SYSTEM_ARCHITECTURE_SPEC.md)
+[final contract](design/E04_FINAL_CONTRACT.md) trước, sau đó
[E05-v1 cross-check](design/E05_BASELINE_REVIEW.md) và complete design baseline,
Accepted ADR-160/161. E01–E05 done;OWNER_REVIEW73/75accepted,còn D12/D13 hoãn.

Actual E04 contract gồm catalog154/DTO154/SC01–04:55canonicalactions/6lookups,
strictUUID-D/8byteBase64RV/scalars,EC-C1 normalized full-intent UTF8/SHA256,
exact24h retention/expired retained-key409/no overwrite,first/replay completion
unions/current authorization/error precedence,7SP/6FN/EF-trigger mappings/
24tablegrants/locks/audit/frontend/runtime/verification contracts. Fresh CSV
fixed6reportcolumns/footer;10000datarecords tính footer không header,
5000000encodedUTF8bodybytes gồmBOM/header/footer/quotes/escapes/CRLF;vượt một
cap rejectwhole409 trướcCSV200. Không đổi rights/saveunits/E03schema/rubric/
RF156–159;exact technical contract authoritative tại accepted E04 package.

D12/D13 và DF01–05 vẫn owner-deferred/environment/evidence prerequisites:
không numericalSLO/provider/origin/port/proxy/backupdestination/patchpin/config
values đã chọn hoặc test/race/load/restore pass. Missing required activeprofile
failclosed;operational values/authority phải có trước chạy thật. Chấp nhận
thiết kế không tự cấp quyền implementationplanning/code/SQL/Git/scaffold/
packages/deploy/exposure/backuprestore/integration. Chờ owner direction riêng.

## 5. Trạng thái từ thiết kế báo cáo

| Đối tượng | Trạng thái |
|---|---|
| Account | ACTIVE, LOCKED, DISABLED |
| Course | ACTIVE, INACTIVE |
| TrainingProgram | DRAFT, ACTIVE, INACTIVE |
| Registration | PENDING, TESTING, BAND_ASSIGNED, CONFIRMED, ADMITTED, REJECTED, CANCELLED |
| EntryTestSession | OPEN, CLOSED, CANCELLED |
| EntryTestAttempt (hiển thị suy diễn) | SCHEDULED, COMPLETED, ABSENT, CANCELLED; từ kết quả/booking hiện tại (ADR-090) |
| EntryTestBooking | SCHEDULED, COMPLETED, ABSENT, CANCELLED |
| Schedule.occurrence_state | PLANNED, HELD, NOT_HELD (ADR-087) |
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
không làm đổi ranh giới ngày nghiệp vụ; lưu/serialize đã chốt ADR-091.
Menu tài khoản chung chứa đổi mật khẩu mọi vai trò/sửa cá nhân chỉ Student
(ADR-084), không thêm mã F. Khung chung FE01a/menu FE01b/cấu trúc trang FE02
đã duyệt ADR-103–ADR-105; FE03–FE10 duyệt ADR-106–ADR-113 (mục 4.6).
Mẫu tương tác theo nghiệp vụ Gói 3 FE11–FE19 đã duyệt ADR-121–ADR-129
(mục 4.8), chưa triển khai hoặc nghiệm thu E.
Loading, empty, error và submitting phải được xử lý. Role switcher trong
mockup là công cụ preview, không là cơ chế đổi vai trò của tài khoản thật.
Mockup và biểu mẫu không chứng minh chức năng đã code hoặc kết nối database.

## 7. Baseline đã chốt và những phần giữ hoãn

E01/E02 Accepted30/09/2026,E03 Accepted01/10/2026. Owner đã nghiệm thu
actual [E04-v1](design/SYSTEM_ARCHITECTURE_SPEC.md)/[final contract](design/E04_FINAL_CONTRACT.md)
trước (ADR-160),rồi [E05-v1](design/E05_BASELINE_REVIEW.md)/complete baseline
(ADR-161)03/10/2026,trên đúng các outputs đã trình,không sửa phương án.
[OWNER_REVIEW](plan/OWNER_REVIEW.md)73/75accepted,còn D12/D13 hoãn.
Không còn yêu cầu duyệt lại policy/RF/FE/operator hoặc actual E04/E05.

DF01–05 tại E04 final contract giữ environment/evidence prerequisites: numerical
performance/deploymentvalues,backupmedia/access/key/isolatedrestore,compatible
patch/blocklistpins/profilecompatibility/config và future SQL/API/EF/grants/
race/browser/CSV/load/recovery evidence. Không xem các slots chưa chọn là
installedvalues hoặc waivedguards;D12/D13 giữ originalnotes,không hỏi lại lúc này.

Q001/002 implementationphases/Slice0 vẫn chỉ khi có owner request riêng;
Q003integration chỉ khi owner invokesworkflow. Baseline acceptance không tự
cấp quyền implementationplan/code/SQL/Git/scaffold/packages/deploy/exposure/
backuprestore. Chờ owner instruction;[design-baseline](plan/design-baseline.md)
done,chưa implementation/runtime tests. Side report/sequence/Word unchanged.

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
  chỉ vì giờ dự kiến đã qua; dùng buổi HELD và nghĩa vụ/thay thế theo ADR-087.
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
  đăng nhập (ADR-055). Hoạt động theo ADR-097; typing chưa gửi request không gia hạn idle.
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

### Kiểm chứng bổ sung A–C (thiết kế, chưa chạy test)

- Ma trận quyền ADR-083/084: chỉ trường đúng phạm vi; lịch sử riêng, không
  DRAFT tới Giáo vụ/Student hoặc finance tới Teacher; menu không quyền mới.
- Catalogue trạng thái/giá không sửa Invoice lịch sử/tự hủy nghiệp vụ; Program
  freeze/Class.program_id và code bất biến; max_size không dưới số đã xếp.
- Điểm đầu vào từ end_at; HELD chỉ Giáo vụ sau end/ONGOING, không tự từ điểm danh;
  một FINAL_EXAM/nghĩa vụ, thay thế không dữ liệu phụ thuộc/không điểm giả.
- [start,end) nối tiếp hợp lệ; trùng học viên/phòng/Teacher bị chặn khi đặt/xếp
  và khi tạo/dời/thay lịch có học viên; weekly batch lỗi không lưu một phần.
- Rebooking/vắng không thêm lượt; is_unfinished chống tạo đồng thời; replacement
  không hai lịch hiện hành/nghĩa vụ hoặc vòng lặp/xóa lịch sử.
- Tiền chuỗi qua API giữ chính xác; UTC/ngày Việt Nam/Unicode code point/NFC và
  NULL/ASCII email/SĐT canonical đúng, không đổi mật khẩu hoặc cắt ngắn đầu vào.
- Revocation/session version và timer không đổi status do throttle; transaction/
  locks/rowversion chống đua; request_id không lặp side effects/hiện lại mật khẩu.
- operation_id nối audit hủy phụ thuộc/toàn lớp; giữ mọi lần gửi/trả/chốt/cũ-mới.
- Snapshot query/queried_at/tỷ lệ hai số lẻ/no-data; D10 đã duyệt ADR-101,
  chưa được coi đã triển khai/kiểm chứng.

### Kiểm chứng bổ sung Gói 1 (thiết kế, chưa chạy test)

- DTO/errors/status/fieldErrors/traceId và phân trang 20/100 ổn định; server
  role/ownership/tiền/tổng điểm và transaction đúng module.
- Request người dùng mới gia hạn idle; polling/timer/typing không; tab chung
  logout chung, device/profile riêng; hạn tuyệt đối 2 giờ vẫn giữ.
- Phiên hạn chế hết tại mốc sớm hơn login +10 phút hoặc temp expiry; chỉ
  đổi mật khẩu/logout, đổi xong hủy mọi phiên và đăng nhập lại.
- Cookie JWT đúng flags và claims; CSRF/Origin ở login/write, không GET
  mutation; không lưu token/profile/grade bền hoặc log payload nhạy cảm.
- Login response không lộ account trước xác thực; IP 100/phút trả 429/
  Retry-After, tách timer account; proxy không tin tùy ý.
- CSV UTF-8 BOM/comma/CRLF/escape, toàn tập/cột hợp lệ; giới hạn 10.000 dòng/
  5 MB lỗi toàn file; ô công thức/full-width theo ADR-101, không sửa DB.
- Bộ kiểm chứng ADR-102: Student ưu tiên mobile/staff desktop, đủ 375/768/1280;
  keyboard/labels/non-color và Chrome/Edge, manual 7 role/E2E, SQL test riêng.

### Kiểm chứng bổ sung Gói 2 (thiết kế, chưa chạy test)

- Bảng/card/list và sổ điểm từng học viên cho cùng dữ liệu/quyền ở ba kích
  thước; phân trang/back context đúng, không PII trong URL/storage.
- Không quyền thì ẩn; thiếu điều kiện thì disabled/lý do; chỉ xem/cảnh báo
  phụ trách bị chặn rõ mà không tiết lộ dữ liệu hoặc thêm quyền đổi người.
- Lưu rõ/không autosave; warning chưa lưu, lỗi trường/toàn form, mã/SĐT
  chuẩn đúng; không cắt/round/merge/overwrite hoặc lưu nháp nhạy cảm bền.
- Loading/no-data/no-match/submitting và 401/403/404/409 đúng; thông báo
  quan trọng bền đủ để xử lý, không lặp tác động khi mất phản hồi.
- Xác nhận thể hiện đối tượng/ảnh hưởng/động từ/lý do đúng phạm vi; không
  xác nhận hai lớp thông thường; ngoại lệ bàn giao mật khẩu tạm FE18 theo ADR-128.
- Component cục bộ, font hệ thống/vi-VN/tiền chính xác/giờ Việt Nam; focus,
  non-color cues, touch target khoảng 44 px và reduced motion cần kiểm chứng.
- Kiểm chứng PasswordHasher/tham số runtime, generator 32 ký tự/ngẫu nhiên mật
  mã/hiển thị một lần và blocklist offline/pin/license; không lộ plaintext,
  không đổi ngữ nghĩa mật khẩu hoặc gửi dịch vụ ngoài (ADR-114).
- Giả lập lỗi audit bắt buộc: rollback nghiệp vụ/tài khoản/thu hồi liên quan;
  lỗi security-event log không bỏ quyền hoặc phục hồi phiên logout. Fallback
  giới hạn/không bí mật; không sinh thông báo người dùng mới (ADR-115).
- Dọn log bảo mật thuần túy/IP sau 90 ngày vẫn giữ audit bắt buộc; actor NULL,
  gộp rate-limit không mất bộ đếm, không mở quyền xóa/xuất app (ADR-116).
- Kiểm chứng tải 10.000 học viên/50 đồng thời với lịch sử, không áp trần nghiệp
  vụ; ghi môi trường/kết quả đo sau khi chốt D12/D13, chưa test pass (ADR-117).
- Kiểm chứng mã hóa/bản riêng/7 ngày-4 tuần/restore tháng và RPO 24 giờ/RTO
  2 giờ, kể cả lần backup bị lỡ khi laptop tắt; chốt nơi lưu/quyền trước vận
  hành. Chưa bảo đảm hoặc chạy backup/restore (ADR-118).
- Kiểm tra patch tương thích/lock và SQL Developer chỉ non-production; owner
  là người vận hành mặc định, không thêm quyền ứng dụng (ADR-119/120).

### Kiểm chứng bổ sung Gói 3 (thiết kế, chưa chạy test)

- Trang chủ/lịch đúng ownership, nhiều hồ sơ không lẫn ngữ cảnh; không dữ liệu
  chưa FINALIZED tới Student, HELD tự suy hoặc lịch thay thế đếm đôi.
- Preview lịch mất hiệu lực khi sửa đầu vào; commit kiểm tra lại, lỗi batch
  không lưu một phần và không tự bỏ ngày lỗi/ngày nghỉ.
- FE14: chưa ghi không thành Có mặt/Vắng/0; lưu một LESSON hoặc một học viên
  all-or-none cho dữ liệu đã nhập/thay đổi, không blank→xóa điểm. Không autosave.
- FE15: sửa chưa lưu chặn gửi; kiểm tra đủ điểm và chuyển toàn lớp hoặc không ai,
  giữ RETURNED có lý do và FINALIZED bất biến, 409 không ghi đè.
- Báo cáo phân biệt bộ lọc nháp/đã áp dụng/queried_at; tổng toàn tập lọc,
  Director không định danh/export, Accountant CSV vẫn theo D10 và query mới.
- In hóa đơn lấy lại dữ liệu có quyền; UNPAID không là xác nhận thu tiền,
  lỗi lấy mới không in cũ như mới; không API PDF/quyền in nhân sự.
- FE18: copy chủ động/bị chặn→thủ công, ack không chứng minh giao thành công;
  đóng/rời/logout/expiry xóa state secret nhưng không hứa xóa clipboard.
  Mất phản hồi kiểm tra trạng thái, không tự retry/reset hoặc replay secret.
- Nhật ký từng sự kiện phân trang/liên kết operation_id đúng scope, allowlist,
  actor NULL/retention-IP đúng; không secrets/full payload/edit/delete/undo/export.
- Kiểm tra tương đương dữ liệu/hành động, keyboard/focus và nhãn ở 375/768/1280;
  không tuyên bố kiểm chứng đã chạy chỉ từ hợp đồng thiết kế này.
