# Handoff — tiếp tục chốt thiết kế dự án

Cập nhật: 28/09/2026. Workspace: `C:\Study\SOEN\main_workplace`.
Đây là hướng dẫn tiếp tục, không thay thế đặc tả chuẩn hoặc cấp quyền triển khai.

## 1. Đọc trước khi tiếp tục

Đọc `AGENTS.md`, rồi đúng thứ tự:

1. `docs/PROJECT_CONTEXT.md`
2. `docs/ARCHITECTURE.md`
3. `docs/CURRENT_STATE.md`
4. `docs/DECISIONS.md`
5. `docs/TODO.md`

Tiếp đó đọc đầy đủ `docs/requirements.md`, `docs/plan/design-baseline.md`,
`docs/plan/questions.md`, `docs/design/FUNCTION_CATALOG.md` và handoff này.
Tra nguồn tại `docs/reference/REPORT_BASELINE.md` khi cần đối chiếu báo cáo.
Các đường dẫn trên tính từ workspace. Nếu memory được cập nhật sau handoff,
dùng memory mới nhất, không dùng handoff để đảo quyết định đã chốt.

## 2. Trạng thái và giới hạn

- Đang thống nhất toàn bộ thiết kế: chức năng → workflow → database → kiến trúc
  → kiểm tra nhất quán/chủ dự án chấp nhận → mới bàn kế hoạch triển khai.
- Stage 1 vẫn doing; các stage sau chưa hoàn tất. Những workflow đã chốt riêng
  không có nghĩa toàn bộ giai đoạn workflow/database/kiến trúc đã được duyệt.
- Chưa có ứng dụng/backend/frontend/migration/test chạy được; mô hình 20 entity
  trong ARCHITECTURE là thiết kế mục tiêu, chưa là schema cuối cùng.
- Không khởi tạo Git, scaffold, Slice 0, viết code/migration hoặc kích hoạt coding
  agent/integration trong giai đoạn này. Skill cũ đề nghị pilot không vượt ADR-011.
- Không sửa `C:\Study\SOEN\final_workplace\Nhom18_ThietKeGiaoDien.docx`.
  Báo cáo đã được đọc và chuẩn hóa vào memory; biểu mẫu phần IV không là yêu cầu.
- `instructions/` là inbox, không tự động có hiệu lực. Bộ yêu cầu cũ chỉ tham khảo
  nếu không xung đột `docs/requirements.md` và các ADR được chấp nhận.

## 3. Điểm dừng và quyết định mới nhất

Nhóm trường còn lại đã đồng ý 28/09/2026, ADR-078–ADR-082:

- Tên khóa/chương trình Unicode trim/1–150, thành phần điểm 1–100; giữ tên
  thành phần duy nhất trong chương trình, không thêm unique tên khóa/chương trình.
- Mã khóa/lớp Giáo vụ tự nhập/trim/viết hoa/1–30 A–Z-số-_- dấu gạch, duy nhất
  trong loại kể cả ngừng/hủy, khóa sau tạo/không tái dùng mã lịch sử.
- Mã phòng cùng định dạng/chuẩn hóa cho lịch lớp-ca; một phòng một mã thống
  nhất, không tự gộp bí danh/không danh mục hoặc module phòng.
- Lý do Unicode/văn bản thuần/1–1000 chỉ nơi đang bắt buộc; chú thích/nhận xét
  tùy chọn <=1000, trim/giữ xuống dòng, không HTML/rich text/không cắt ngắn.
- Mã giao dịch trim/<=100/giữ nội dung-hoa thường, chuyển khoản bắt buộc/
  tiền mặt có thể bỏ trống; không unique toàn hệ thống, vẫn một Payment/hóa đơn.

Không hỏi lại; lựa chọn khác không được coi là đã duyệt. Kỹ thuật kiểm tra/
lưu chuẩn hóa còn thiết kế; chưa code/schema hoặc chốt stage 1/toàn baseline.

Nhóm kiểm tra thông tin nhập đã đồng ý 28/09/2026, ADR-073–ADR-077:

- Họ tên bỏ khoảng trắng đầu/cuối, 1–100 ký tự Unicode/dấu câu dùng trong tên,
  không bắt hai từ hoặc chỉ ASCII; không thêm quyền sửa nhân sự.
- Ngày sinh có thật/không tương lai theo giờ Việt Nam, không giới hạn tuổi,
  không bắt nhân sự có ngày sinh.
- Email tạo tài khoản trim/đúng định dạng/tối đa 254 ký tự, so trùng toàn tài
  khoản không phân biệt hoa-thường, không gộp dấu chấm/+tag theo nhà cung cấp.
- SĐT chỉ phạm vi di động Việt Nam 10 số bắt đầu 0, nhận +84 tương đương/
  khoảng trắng-dấu gạch, chuẩn hóa trước so trùng khi có; không số quốc tế/
  điện thoại bàn/OTP/danh mục đầu số, không tự bắt nhân sự nhập SĐT.
- Địa chỉ học viên tùy chọn/trim/tối đa 255 ký tự Unicode/nhập tự do, không
  danh mục tỉnh-huyện, không thêm snapshot hoặc trường bắt buộc nhân sự.

Không hỏi lại. Các lựa chọn khác không được nhận là đã đồng ý; kiểm tra/lưu
chuẩn hóa/cách đếm Unicode còn kỹ thuật. Stage 1 vẫn doing, chưa code/schema.

Nhóm bộ lọc/quyền chi tiết/xuất đã đồng ý 28/09/2026, ADR-068–ADR-072:

- Tuyển sinh mặc định tháng Việt Nam hiện tại/mọi trạng thái, giữ khóa ngừng
  cho lịch sử; không thêm quyền báo cáo cho nhân viên Tuyển sinh.
- Hiện trạng đào tạo lọc khóa/Band/lớp/giáo viên hiện tại/trạng thái, mặc định
  mọi trạng thái/không kỳ ngày; kết quả thêm kỳ chốt mặc định tháng hiện tại.
- Tài chính mặc định tháng hiện tại theo cơ sở xác nhận/phát hành, lọc khóa
  nhất quán; công nợ hiện tại có chỉ quá hạn và không lọc kỳ ngày.
- Giám đốc chỉ chi tiết nhóm phù hợp, không hồ sơ/hóa đơn/định danh học viên,
  không xuất hoặc phê duyệt; tra cứu hóa đơn nghiệp vụ Kế toán không đổi.
- CSV Kế toán toàn kết quả lọc/cột báo cáo, không chỉ trang/không trường ẩn
  hoặc hồ sơ cá nhân, không xuất riêng hồ sơ/hóa đơn/học viên dưới quyền này.

Không hỏi lại. Kiểm tra thông tin nhập chốt tiếp ADR-073–ADR-077; truy vấn/
xuất nhất quán, cấu trúc/giới hạn/an toàn CSV và hiển thị tỷ lệ còn kỹ thuật.
Chưa code hoặc chốt stage 1/toàn baseline.

Nhóm chỉ số báo cáo đã đồng ý 28/09/2026, ADR-063–ADR-067:

- Trạng thái hiện tại lúc tra cứu, không tái dựng trạng thái/công nợ tại ngày
  quá khứ; vẫn giữ lịch sử nghiệp vụ/audit và snapshot tài chính.
- Tổng phải thu: hóa đơn UNPAID+PAID phát hành trong kỳ, loại CANCELLED hiện
  tại. Nợ UNPAID hiện tại tách riêng/không lọc theo ngày thanh toán; doanh thu
  theo xác nhận đã chốt. Không lấy hai tập khác kỳ trừ nhau để tính nợ.
- Mỗi student_id một lần mỗi nhóm khóa; tổng đếm duy nhất toàn tập lọc, không
  cộng nhóm. Số hồ sơ riêng, học lại có thể tăng hồ sơ không tăng người.
- Tiến độ LESSON thực tế xác nhận đã diễn ra/số LESSON chương trình; buổi không
  tổ chức không tính/thay thế một lần; FINAL_EXAM riêng. Không suy từ thời gian
  hoặc điểm danh; Q-008 còn biểu diễn, lớp hủy vẫn có trong thống kê trạng thái.
- Tỷ lệ đạt: kết quả FINALIZED đạt/tổng FINALIZED trong kỳ chốt điểm, mỗi lượt
  học riêng/loại chưa chốt; không dữ liệu hiển thị "Chưa có dữ liệu", không 0%.

Không hỏi lại. Bộ lọc/quyền chi tiết/xuất chốt tiếp ADR-068–ADR-072;
truy vấn/hiển thị còn kỹ thuật, chưa code hoặc chốt toàn baseline.

Nhóm nhật ký đã đồng ý 28/09/2026, ADR-058–ADR-062:

- Giữ audit bắt buộc, thêm thay đổi cấu hình khóa/chương trình/Band, trạng thái
  lớp và tạo/dời/thay lịch thành công; không ghi mọi lượt xem thông thường.
- Ghi đăng nhập thành công/thất bại/chặn tạm/đăng xuất/đổi-reset mật khẩu/cấp
  nhân sự-thao tác trạng thái tài khoản/từ chối quyền; không gán actor đã xác
  thực chỉ từ email được nhập khi chưa xác thực.
- Admin lọc ngày/người/thao tác/loại-ID đối tượng/kết quả; phân trang mới trước.
- Chi tiết thời gian/người/thao tác/đối tượng/kết quả-lý do/trường thay đổi cần
  thiết, giữ cũ/mới bắt buộc; không mật khẩu/hash/JWT/bí mật phiên/toàn payload,
  hạn chế dữ liệu cá nhân, không thêm snapshot hoặc bỏ lịch sử cần giữ.
- Chỉ Admin tra cứu trong ứng dụng, không xuất CSV/PDF nhật ký; không sửa/xóa
  log thông thường. Không thêm luồng lịch sử đổi giáo viên lớp.

Không hỏi lại. Biểu diễn/lưu/lỗi ghi/chống ngập/lưu giữ/backup còn database/
kiến trúc; chưa code hoặc chốt toàn baseline. Chỉ số báo cáo đã chốt tiếp
ADR-063–ADR-067, bộ lọc/quyền chi tiết/xuất ADR-068–ADR-072.

Nhóm mật khẩu/phiên đã đồng ý 28/09/2026, ADR-053–ADR-057:

- Mật khẩu 15–128 ký tự, Unicode/khoảng trắng, không bắt tổ hợp chữ hoa/số/ký
  tự đặc biệt; chặn phổ biến/đã lộ, không ép đổi định kỳ khi chưa sự cố.
- Mật khẩu tạm hết hạn 24 giờ từ cấp/reset, cần Admin reset lại; không tự
  đổi trạng thái/mở khóa/khôi phục. Vẫn bắt đổi trước nghiệp vụ ADR-048.
- Phiên thường tối đa 2 giờ từ đăng nhập hoặc 30 phút không hoạt động, mốc
  nào trước; hoạt động không kéo dài hạn tuyệt đối, không refresh/ghi nhớ.
- Nhiều thiết bị đồng thời, đăng nhập mới không đẩy phiên cũ ra.
- Đăng xuất hủy phiên hiện tại tại server, kể cả JWT chưa hết hạn; phiên khác
  giữ theo hạn/ADR-049. Không nút đăng xuất tất cả, không quản lý thiết bị/phiên.

Không hỏi lại; cách đếm ký tự/nguồn danh sách chặn/định nghĩa hoạt động/phiên
hạn chế/lưu-thực thi còn database/kiến trúc. Chưa code hoặc chốt toàn baseline.

Nhóm tài khoản/bảo mật đã đồng ý 28/09/2026, ADR-048–ADR-052:

- Cấp nhân sự/reset: mật khẩu tạm ngẫu nhiên, Admin xem một lần/giao trực tiếp,
  bắt buộc đổi trước truy cập nghiệp vụ; không log mật khẩu/reset tự mở khóa.
- Khóa thủ công/ngừng dùng/reset/đổi mật khẩu hủy mọi phiên cũ, kể cả phiên
  hiện tại/JWT chưa hết hạn; phải đăng nhập lại, không sống lại khi mở/khôi phục.
- Admin được khôi phục DISABLED → ACTIVE trên cùng tài khoản, giữ vai trò/dữ
  liệu/lịch sử, không phục hồi phiên cũ hoặc hồ sơ nghiệp vụ đã kết thúc.
- Không sửa email đăng nhập của tài khoản hiện có, kể cả Admin/nhân sự.
- Sai 5 lần liên tiếp trong 15 phút chặn đăng nhập 15 phút; thành công/hết hạn
  đặt lại bộ đếm/cửa sổ. Không đổi status/hủy phiên đang dùng chỉ vì nhập sai;
  LOCKED thủ công vẫn cần Admin mở, DISABLED không tự khôi phục.

Không hỏi lại nhóm này; độ mạnh/hạn mật khẩu và thời hạn/số phiên/đăng xuất
đã chốt tiếp ADR-053–ADR-057. Cơ chế lưu/thực thi còn database/kiến trúc.

Nhóm Q-010 đã được chủ dự án đồng ý, lưu ADR-043–ADR-047: học phí dương/không
khóa miễn phí; VND nguyên đồng; hạn ngày phát hành +7 ngày lịch kể cả ngày nghỉ;
chỉ thời điểm xác nhận thanh toán, không thêm ngày thu thực tế hoặc nhập lùi;
Asia/Ho_Chi_Minh (UTC+7) cho lịch/ngày/hạn thu/lọc báo cáo. Không hỏi lại nhóm
này. Kiểu/range/lưu thời gian còn database/kiến trúc, chưa viết code.

Chủ dự án đồng ý cả 5 đề xuất, đã lưu ADR-038–ADR-042:

- Không sửa khóa trên Registration hiện có, kể cả trước xác nhận; hủy hợp lệ
  theo ADR-022/031 rồi đăng ký khóa mới/thi đầu vào lại, giữ lịch sử cũ.
- Không snapshot thông tin cá nhân khi đăng ký; hồ sơ cũ hiển thị thông tin
  hiện tại. Snapshot học phí hóa đơn/lịch sử nghiệp vụ/audit vẫn giữ.
- Không chuyển lớp sau xếp, kể cả trước dạy; không luồng gỡ/xếp lại.
- Học viên in hóa đơn của mình qua trình duyệt, đúng trạng thái thanh toán;
  không xuất/tải PDF riêng hoặc phát hành biên lai riêng.
- Kế toán chỉ sửa phương thức/mã giao dịch Payment đã xác nhận, lý do bắt buộc,
  audit cũ/mới/người/thời gian. Khóa số tiền/hóa đơn/ngày xác nhận; giữ cùng
  Payment và Invoice PAID; chuyển khoản cần mã giao dịch; không hủy/xóa/hoàn tiền.

Không hỏi lại nhóm này; không nhận các phương án thay thế là đã được đồng ý.

Phạm vi buổi cả lớp không tổ chức đã chốt ADR-035: nằm trong NV19/F21,
không module ngày nghỉ/lịch nghỉ toàn trung tâm/automation/học bù cá nhân.
Cách dời/thay LESSON đã chốt ADR-036: trước giờ bắt đầu dời theo ADR-032;
sau giờ dự kiến nếu không tổ chức, giáo vụ ghi lý do/giữ lịch cũ/người/thời gian
và tạo buổi thay thế liên kết cùng lớp. Chỉ khi chưa điểm danh/điểm liên quan,
lớp chưa COMPLETED/CANCELLED; giáo viên hiện tại, kiểm tra trùng lịch/lưu nhất
quán. Buổi thay thế thực hiện một nghĩa vụ LESSON, không tăng số buổi; cần nó
diễn ra/đủ điểm danh trước hoàn thành, không điểm danh giả buổi cũ. Dữ liệu còn
bước database. ADR-037: không cho thêm LESSON vượt số buổi chương trình;
tạo lịch tuần/thêm buổi chỉ để đủ nghĩa vụ LESSON. Buổi thay thế không tăng
nghĩa vụ, không áp giới hạn lên tổng bản ghi lịch sử; FINAL_EXAM riêng.
Chủ dự án yêu cầu gửi/duyệt bảng điểm bắt buộc cả lớp trong một thao tác;
gửi/gửi lại/trả lại/chốt toàn lớp đã lưu ADR-034, không hỏi lại phạm vi.
Điểm danh từ LESSON bắt đầu đến trước lớp COMPLETED đã lưu ADR-033.
Số buổi/lịch đủ và giới hạn sửa lịch lớp đã lưu ADR-032.
Tác động hủy hồ sơ lên đặt ca/lượt thi đã lưu ADR-031,
Q-007 đã chốt nghiệp vụ, còn biểu diễn lịch sử/audit ở bước database. Q-004
còn biểu diễn lịch sử/audit/cơ chế lưu đóng ca. Q-006 đã chốt ADR-028/ADR-029:

- ADR-028: cấu hình khóa/Band/thành phần/trọng số chương trình khóa từ lớp đầu
  tiên, kể cả lớp bị hủy; bộ ngưỡng Band 0/25/45/65/85 khóa sau kết quả đầu tiên.
  Điểm/trọng số nhập tối đa hai số lẻ; tổng tính bằng decimal, không làm tròn
  từng đóng góp, làm tròn tổng một lần half-up hai số lẻ để lưu/hiển thị/xét đạt.
  Tổng 49,995 → 50,00 đạt khi FINALIZED; đầu vào không làm tròn số nguyên để xếp Band.
- ADR-029: chỉ giáo viên đã nhập điểm và vẫn được phân công ca được sửa điểm
  lượt COMPLETED khi hồ sơ TESTING/BAND_ASSIGNED; bắt buộc lý do, điểm cũ/mới,
  người/thời gian; tính lại điểm cao nhất/Band cùng transaction, không tăng lượt.
  Khóa sau CONFIRMED hoặc hủy/từ chối; không ngoại lệ Admin/giáo vụ. Hai lượt
  COMPLETED cùng điểm cao nhất chọn lượt một làm best_attempt_id, giữ cả hai.
- Việc lưu audit, kiểu số và ràng buộc đồng thời sẽ thiết kế ở bước database;
  không hỏi lại chính sách nghiệp vụ này chỉ vì trường dữ liệu chưa hoàn chỉnh.
- ADR-030: ca mới CLOSED; giáo vụ mở/mở lại trước bắt đầu với tài nguyên hợp
  lệ, giáo viên ACTIVE và sức chứa; ca đầy vẫn OPEN. CLOSED giữ đặt chỗ,
  chặn học viên đăng ký/hủy/đổi nhưng vẫn cho giáo viên chấm/ghi vắng/sửa theo
  ADR-027/029. Đến giờ bắt đầu áp dụng CLOSED và không mở lại. Giáo vụ đổi
  giáo viên chấm chỉ trước bắt đầu và mọi điểm/ABSENT, có lý do, audit người
  cũ/mới/người thao tác/thời gian, kiểm tra lịch giáo viên ACTIVE; giữ đặt chỗ.
  CANCELLED không mở lại/chấm; lượt thi lại đang chờ trong CLOSED vẫn chặn xác nhận.
- ADR-031: hủy hồ sơ hợp lệ hủy đặt chỗ hoạt động trong ca chưa bắt đầu,
  kể cả CLOSED, giải phóng chỗ cùng hồ sơ/hóa đơn UNPAID; liệt kê ảnh hưởng
  trước xác nhận. Giữ ca/đặt chỗ người khác. Giữ lịch sử ca đã bắt đầu và
  điểm/ABSENT/Band hiện có; dừng nhập/sửa điểm và ghi vắng sau CANCELLED.
  Chưa chấm hiển thị "Hồ sơ đã hủy — chưa có kết quả", không tạo enum/ghi 0
  hoặc ABSENT. Đăng ký lại dùng hồ sơ mới, thi từ đầu, không chép điểm/Band.
  Lưu actor/time/trạng thái cũ/lý do theo quyền đã chốt, truy vết hủy phụ thuộc;
  một phần bắt buộc thất bại thì không hủy phần nào. Chống đua đặt ca/bắt đầu/
  chấm/xác nhận/duyệt/thanh toán; không cấp quyền hủy riêng đặt chỗ CLOSED.

- ADR-032: số buổi thuộc chương trình theo khóa/Band, số nguyên dương, chỉ
  LESSON không FINAL_EXAM, khóa từ lớp đầu tiên sử dụng. Mở lớp vẫn chỉ cần
  một LESSON tương lai hợp lệ; trước bắt đầu dạy cần đủ lịch LESSON dự kiến.
  Giáo vụ chỉ sửa ngày/giờ/phòng buổi chưa bắt đầu/chưa điểm danh hoặc điểm
  liên quan, lớp chưa COMPLETED/CANCELLED, kiểm tra xung đột. Buổi đã bắt đầu
  hoặc có dữ liệu phụ thuộc giữ nguyên. Trường dữ liệu/ngày nghỉ/buổi thêm-bù
  và thời hạn nhập/sửa điểm danh chưa được quyết định này giải quyết.

- ADR-033: giáo viên phụ trách nhập/sửa điểm danh từ lúc LESSON bắt đầu
  đến trước lớp COMPLETED,
  không hạn cố định 24/48 giờ mỗi buổi. Sau COMPLETED chỉ xem dù bảng điểm
  chưa gửi/chốt; vẫn cần đủ điểm danh LESSON trước khi giáo vụ hoàn thành lớp.
  Chống cập nhật đồng thời với hoàn thành. Không thêm quyền sửa ngoại lệ/mở lại;
  chặn nhập trước buổi bắt đầu, không cần chờ kết thúc. Hai mốc đã chốt,
  không hỏi lại; có điểm danh không tự xác nhận buổi đã diễn ra đầy đủ.

Các quyết định trước vẫn giữ nguyên; đọc requirements/ADRs để lấy chi tiết.
- ADR-034: gửi/gửi lại, giáo vụ trả lại có lý do hoặc chốt/công bố bắt buộc
  toàn lớp trong một thao tác. Mọi học viên đủ điểm bắt buộc và DRAFT/RETURNED
  trước gửi; trả lại/chốt toàn bộ SUBMITTED của lớp COMPLETED, toàn bộ hoặc
  không phần nào. Giữ dữ liệu khi trả, sửa giá trị cần sửa rồi gửi lại cả lớp;
  không hiểu là chỉ được gửi một lần trong vòng đời. Khóa ADR-024 giữ nguyên,
  biểu diễn dữ liệu/thao tác nhóm/audit còn bước database/kiến trúc.
Đặc biệt: không học bổng, không /10, không đăng ký/đặt ca thay bởi tuyển sinh,
không thi lại sau xác nhận, không tự phạt công nợ hoặc hoàn tiền. CSV cho báo cáo.
Tối đa hai lượt logic mỗi hồ sơ, không phải hai lần đặt ca. Kết thúc hồ sơ học
đòi lớp COMPLETED và bảng điểm học viên FINALIZED, không đòi đạt môn.
Xếp lớp chỉ OPEN/đúng khóa-Band/còn chỗ/ADMITTED/PAID; không tự thêm trạng thái.

## 4. Việc chat tiếp theo cần làm

Chủ dự án đã yêu cầu gom toàn bộ nội dung cần đồng ý vào một file, thay cho
hỏi từng nhóm trong chat. Đọc `docs/plan/OWNER_REVIEW.md`: 55 dòng gồm 50 lựa
chọn thiết kế còn mở và 5 mục nghiệm thu đầu ra, ban đầu đều chưa tích.
File có đúng 4 cột: vấn đề, đề xuất, đồng ý, phương án owner nếu không đồng ý.

1. Đọc ô tích/phương án owner trong file; không bắt owner kể lại hoặc trả lời
   lại ADR-001–ADR-082. Ô trống không là đồng ý, phương án agent chưa là quy tắc.
2. Ghi nhận chỉ mục được owner duyệt vào requirements/ADR và tài liệu bị ảnh
   hưởng. Nếu owner ghi phương án khác, xử lý phương án đó, không tự giữ đề xuất
   cũ hoặc tự tích. `questions.md` chỉ giữ nguồn/gap và liên kết, không bảng duyệt thứ hai.
3. Hoàn thiện đầu ra theo chức năng → workflow → database → kiến trúc → rà toàn
   baseline. E01–E05 ghi CHỜ TÀI LIỆU, chỉ duyệt khi có tài liệu hoàn chỉnh thực
   tế để đối chiếu; đồng ý A–D không tự nghiệm thu đầu ra hoặc đánh dấu stage done.
4. Nếu phát hiện khoảng trống mới thật sự, thêm dòng chưa tích vào chính file
   này, giải thích ngắn gọn; không quay lại hỏi từng trường SQL/quyết định đã chốt.

A01–A05 giữ 5 đề xuất Q-011 trước đó chưa duyệt; A06–A10/B01–B07 gom danh mục,
phạm vi xem và workflow còn thiếu. C01–C16 bao phủ lịch sử đặt ca/lượt logic,
buổi thực tế/thay thế, audit/cấu hình/tiền-thời gian/validator/unique/concurrency/
phiên/Invoice và báo cáo. D01–D17 gom hợp đồng API/UI/security/export/NFR/vận hành.
Mô hình mở rộng/giới hạn/phiên bản trong bảng đều là đề xuất, chưa sửa schema
hoặc ARCHITECTURE thành thiết kế đã chốt. Các nguồn kỹ thuật không thay owner duyệt.

Ước lượng chat rounds sau ADR-077 trong design-baseline chỉ còn là bối cảnh
lịch sử. 55 mục là danh sách khoảng trống hiện nhận diện, không cam kết không
phát sinh mâu thuẫn khi hoàn thiện tài liệu. Q-001/Q-002 về triển khai/Slice 0
tiếp tục hoãn; Q-003 chỉ xử lý khi owner yêu cầu integration. Chưa code/Git/
scaffold/migration, chưa hoàn tất stage 1 hoặc toàn baseline.

## 5. Cách lưu kết quả và dùng skill

Trao đổi bằng tiếng Việt; chủ dự án yêu cầu dùng file `OWNER_REVIEW.md` với
4 cột để tích đồng ý hoặc tự ghi phương án khác, tiết kiệm các lượt hỏi chat.
Không hỏi lại quyết định đã chốt. Phân biệt nguồn báo cáo, quyết định đã chốt
và đề xuất chưa chốt. Khi chủ dự án đồng ý: cập nhật requirements, thêm
ADR đáng kể, đồng bộ phần bị ảnh hưởng trong catalog/architecture/current state/
TODO/active plan và đóng đúng phần câu hỏi. Không đánh dấu hoàn tất toàn baseline.

Đọc trọn `instructions/english-center-agent-kit/.agents/skills/project-planning/SKILL.md`
khi dùng cho kế hoạch; hướng dẫn owner và ADR-011 ưu tiên hơn đề nghị bắt đầu pilot.
Skill `codex-with-chatgpt` đã có tại `.agents/skills/codex-with-chatgpt/`;
chưa khởi chạy/kiểm chứng kết nối. Cài skill không tự cấp quyền kết nối/delegation;
chỉ đọc và dùng protocol khi workflow đó được yêu cầu. Không tự tạo chat mới,
gửi tin sang chat khác hoặc phân công coding agent từ handoff này.
