"""Build the separate, reviewed sequence-guide set; never touch the report."""
from pathlib import Path
import re, hashlib, json, zipfile, textwrap

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'docs/design/sequence-guides-20261014'
DEST.mkdir(parents=True, exist_ok=True)
GUIDES = []

def bullets(items):
    return '\n'.join('- ' + x for x in items)

def flow(name, actor, boundary, query, check, request, effect, result, fail, *, read=False, secret=False):
    return dict(name=name, actor=actor, boundary=boundary, query=query, check=check,
                request=request, effect=effect, result=result, fail=fail, read=read, secret=secret)

def guide(number, filename, title, goal, trace, sources, rules, flows, notes, checklist, exclusions):
    GUIDES.append(dict(number=number, filename=filename, title=title, goal=goal,
                       trace=trace, sources=sources, rules=rules, flows=flows,
                       notes=notes, checklist=checklist, exclusions=exclusions))

guide(2, 'SQ02_STAFF_ACCOUNT_CREATION_GUIDE.md', 'Admin cấp tài khoản nhân sự',
      'Admin tạo tài khoản cho nhân sự bằng vai trò cố định, nhận mật khẩu tạm một lần để bàn giao ngoài ứng dụng. Thành công chỉ được báo sau khi Account và audit bắt buộc đã commit; tài khoản chưa có phiên đăng nhập.',
      'NV33; W01; F32; UC01', ['Services/Administration/StaffAccountService.cs', 'Api/Administration/StaffAccountEndpoints.cs'],
      [('Quyền', 'Admin đang có phiên NORMAL hợp lệ; không có đăng ký nhân sự tự phục vụ.'),
       ('Thông tin', 'Họ tên, email, vai trò nhân sự theo lựa chọn hệ thống cho phép; SĐT tùy chọn theo hợp đồng staff. Không tạo StudentProfile cho tài khoản nhân sự.'),
       ('Duy nhất', 'Email và SĐT khi có phải chưa sử dụng toàn Account; kiểm cả tình huống hai yêu cầu đồng thời.'),
       ('Credential', 'Hệ thống sinh mật khẩu tạm ngẫu nhiên 32 ký tự; chỉ lưu hash, hạn 24 giờ, bắt đổi mật khẩu khi login. Không cho Admin nhập mật khẩu thường thay cơ chế tạm.'),
       ('Bàn giao', 'Hiển thị plaintext trong RAM một lần sau thành công; không có API xem lại secret, không lưu vào storage/log.'),
       ('Nguyên tử', 'Account ACTIVE với role đã chọn + metadata mật khẩu tạm + mandatory audit + dấu hoàn tất thao tác cùng giao dịch.')],
      [flow('A — Tạo tài khoản và bàn giao mật khẩu tạm', 'Admin', 'Giao diện tài khoản nhân sự',
            'Mở form cấp nhân sự và đọc các lựa chọn vai trò được phép',
            'Kiểm Admin hiện tại, dữ liệu, vai trò được cấp, email/SĐT duy nhất',
            'Nhập họ tên/email/vai trò/SĐT nếu có và chọn cấp tài khoản',
            'Sinh mật khẩu tạm; tạo Account ACTIVE, hash/hạn 24 giờ/bắt đổi; ghi audit bắt buộc và hoàn tất thao tác nguyên tử',
            'Thông tin tài khoản đã cấp và mật khẩu tạm chỉ trong phản hồi bàn giao đầu tiên',
            'Sai quyền/dữ liệu/vai trò; email hoặc SĐT trùng kể cả khi phát sinh đồng thời', secret=True)],
      ['Mật khẩu tạm chỉ hiển thị một lần. Copy/đóng bàn giao là thao tác trên Boundary, không phải request đọc lại secret.',
       'Nếu mất phản hồi, kiểm Account/thao tác được phép trước. Nếu đã cấp nhưng mất secret, Admin chủ động reset mới ở SQ05; không tự cấp lại hoặc replay plaintext.'],
      ['Tài khoản nhân sự không có StudentProfile.', 'Secret chỉ xuất hiện sau commit và không nằm trong audit.',
       'Không auto-login; người nhận dùng SQ03 rồi SQ04.', 'Có nhánh trùng đồng thời và rollback bắt buộc.'],
      'Không thêm email/OTP, cấp Student thay, chọn nhiều role hoặc xem lại mật khẩu tạm.')

guide(6, 'SQ06_STUDENT_PROFILE_HOME_GUIDE.md', 'Hồ sơ cá nhân và trang chủ Student',
      'Student đọc trang chủ theo từng Registration và sửa các trường cá nhân được phép. Tách đọc trang chủ với lưu hồ sơ: mở trang không là lệnh cập nhật và không tạo tiến độ nghiệp vụ mới.',
      'Thao tác chung; phép chiếu NV02–08; W04; F03 và menu hồ sơ; UC01–07',
      ['Api/Enrollment/StudentEnrollmentEndpoints.cs', 'Infrastructure/Operator/LocalRuntimeServices.cs'],
      [('Ownership', 'Phiên NORMAL của Student; backend xác định Student của phiên, không tin studentId do người dùng tự nhập.'),
       ('Trường được sửa', 'Họ tên 1–100 code point, ngày sinh không sau ngày Việt Nam hiện tại, SĐT di động VN duy nhất, địa chỉ tùy chọn ≤255. Email chỉ đọc.'),
       ('Đơn vị lưu', 'Thông tin Account và StudentProfile lưu cùng thao tác, kiểm phiên bản cả hai; không merge âm thầm form cũ.'),
       ('Trang chủ', 'Trình bày theo mỗi Registration và next action do quyền/điều kiện hiện tại xác định; không gộp các lần học thành một state chung.'),
       ('Điểm', 'Chỉ có link/giá trị Student được đọc; không đưa DRAFT/SUBMITTED/RETURNED vào payload trang chủ.'),
       ('Không thay đổi', 'Dữ liệu sau chuẩn hóa giống hiện tại thì không lưu/no-change; stale hoặc sai quyền phải được kiểm trước kết luận no-change.')],
      [flow('A — Đọc trang chủ', 'Học viên', 'Giao diện trang chủ học viên',
            'Đọc các hồ sơ, tình trạng hóa đơn/lớp/lịch và liên kết kết quả được phép',
            'Kiểm Student hiện tại và phạm vi sở hữu của từng dữ liệu trả về',
            'Mở trang chủ', 'Dựng các phép chiếu theo Registration; lấy số liệu/khả năng thao tác hiện tại từ hệ thống',
            'Các thẻ hồ sơ và liên kết đúng quyền; không có dữ liệu kết quả chưa công bố', 'Hết phiên hoặc không có quyền Student', read=True),
       flow('B — Sửa hồ sơ cá nhân', 'Học viên', 'Giao diện hồ sơ cá nhân',
            'Đọc tên/email/ngày sinh/SĐT/địa chỉ và thông tin phiên bản hiện tại',
            'Kiểm ownership, phiên bản Account/Profile, dữ liệu hợp lệ, SĐT chưa dùng và có thay đổi thực',
            'Sửa các trường được phép và chọn lưu hồ sơ',
            'Cập nhật tên/SĐT của Account và ngày sinh/địa chỉ của Profile; audit bắt buộc và dấu hoàn tất cùng giao dịch',
            'Hồ sơ cá nhân mới và phiên bản mới', 'Sai dữ liệu, SĐT trùng, phiên bản cũ hoặc toàn bộ giá trị không đổi')],
      ['Email đăng nhập bất biến. Sửa hồ sơ cá nhân không đổi Course/Band/kết quả và không thu hồi phiên như đổi mật khẩu.',
       'Trang chủ đọc các dữ liệu có sẵn; không auto-book, auto-confirm hoặc tự tạo audit page-view.'],
      ['Hai hình đọc/lưu có điểm bắt đầu riêng.', 'Thể hiện kiểm cả Account và Profile trước lưu.',
       'SĐT trùng đồng thời không gây lưu một phần.', 'Không công bố điểm nháp qua trang chủ.'],
      'Không thêm đổi email/avatar/upload, tiến độ lưu riêng hoặc staff sửa Profile thay Student.')

guide(8, 'SQ08_ENTRY_SESSION_MANAGEMENT_GUIDE.md', 'Giáo vụ quản lý ca thi đầu vào',
      'Giáo vụ chuẩn bị, mở/đóng, hủy ca hoặc thay grader. Mỗi action là lần xử lý riêng; Teacher chỉ đọc ca được phân công ở hình E, không tham gia duyệt lệnh Giáo vụ.',
      'NV14–15,21; W06; F16–17,F25; UC03',
      ['Services/Academics/EntrySessionService.cs', 'Services/Enrollment/TeacherEntryService.cs'],
      [('Tạo ca', 'Ca mới CLOSED; ngày/giờ VN cùng ngày, start<end và start>serverNow lúc lưu; phòng chuẩn hóa, capacity 1–20, Teacher ACTIVE, không trùng phòng/grader với ca thi hoặc lớp.'),
       ('Sửa', 'Date/time/room chỉ trước start và chưa có booking; sức chứa/những action khác phải theo dữ liệu phụ thuộc và khả năng hiện tại. Không dùng sửa như đổi grader ngầm.'),
       ('Mở/đóng', 'Mở/reopen trước start, đủ tài nguyên/chỗ; đóng OPEN trước start giữ booking. Đến start effectively CLOSED, read không tự ghi trạng thái.'),
       ('Hủy', 'Chỉ trước start, cần lý do; hủy mọi booking hoạt động của ca trong cùng giao dịch, giữ logical Attempt/history; không tự đặt ca khác.'),
       ('Đổi grader', 'Trước start, chưa có score/ABSENT, cần lý do và Teacher thay hợp lệ/không xung đột. Cùng parsed grader là no-change, lý do khác không biến thành thay đổi.'),
       ('Thẩm quyền', 'Nút hoặc preview cũ không bảo đảm action còn hợp lệ; kiểm full membership/phiên bản/đồng hồ tại lưu.')],
      [flow('A — Tạo hoặc sửa thông tin ca', 'Giáo vụ', 'Giao diện ca thi đầu vào',
            'Đọc danh mục ca và lựa chọn phòng/Teacher; khi sửa đọc ca và phụ thuộc hiện tại',
            'Kiểm phiên bản/phụ thuộc, thời gian tương lai, dữ liệu ca và toàn bộ xung đột tài nguyên',
            'Nhập ngày/giờ/phòng/capacity/Teacher theo form và chọn lưu',
            'Tạo ca CLOSED hoặc sửa các trường được phép; audit bắt buộc và hoàn tất thao tác cùng giao dịch',
            'Ca đã lưu và phiên bản mới', 'Dữ liệu sai, thời điểm đã tới, có booking chặn sửa, trùng tài nguyên hoặc form cũ'),
       flow('B — Mở hoặc đóng ca', 'Giáo vụ', 'Giao diện ca thi đầu vào',
            'Đọc ca, trạng thái, khả năng mở/đóng và ảnh hưởng hiện tại',
            'Kiểm action/source state, trước start, Teacher/phòng/capacity và phiên bản ca',
            'Chọn một action Mở hoặc Đóng và xác nhận',
            'Đổi trạng thái ca theo action; giữ các booking đang có; audit bắt buộc nguyên tử',
            'Ca OPEN hoặc CLOSED theo action đã chọn', 'Sai source state, đã start, tài nguyên hoặc phiên bản thay đổi'),
       flow('C — Hủy ca và các booking hoạt động', 'Giáo vụ', 'Giao diện ca thi đầu vào',
            'Đọc ca và preview đầy đủ booking/Registration sẽ bị ảnh hưởng',
            'Kiểm trước start, lý do, ca và toàn bộ tập booking/Registration đúng phiên bản',
            'Nhập lý do và xác nhận hủy ca',
            'Đổi ca CANCELLED, hủy booking hoạt động, giữ Attempt/history; audit nhóm và dấu hoàn tất trong một giao dịch',
            'Ca đã hủy và danh sách ảnh hưởng đã thực hiện', 'Ca đã start, membership mới, booking/Registration đổi hoặc thiếu lý do'),
       flow('D — Thay giáo viên chấm', 'Giáo vụ', 'Giao diện ca thi đầu vào',
            'Đọc grader hiện tại, danh sách Teacher thay và ảnh hưởng/xung đột',
            'Kiểm trước start/chưa score hoặc ABSENT, Teacher thay, lý do, phiên bản và thay đổi thực',
            'Chọn Teacher khác, nhập lý do và xác nhận đổi grader',
            'Thay grader và ghi old/new/reason audit cùng giao dịch; không sửa kết quả thi',
            'Ca với grader mới', 'Không còn điều kiện thay, trùng giờ, grader không hợp lệ hoặc cùng grader'),
       flow('E — Teacher xem ca được giao', 'Giáo viên', 'Giao diện ca thi được phân công',
            'Đọc danh sách/chi tiết ca được phân công cho Teacher hiện tại',
            'Kiểm vai trò Teacher và phân công hiện tại của ca',
            'Mở danh sách hoặc chọn ca thi', 'Chiếu thông tin ca/booking và khả năng chấm theo quyền Teacher',
            'Ca được giao và các học viên trong phạm vi', 'Hết quyền/phân công hoặc ca ngoài phạm vi', read=True)],
      ['CLOSED vẫn cho chấm/vắng/sửa điểm khi guard SQ10 đạt. CLOSED không xóa booking hoặc score.',
       'Hủy ca không tăng attempt_number và không gọi Student để tự động đặt lại.'],
      ['OPEN/CLOSED/CANCELLED là action/state riêng.', 'Hủy kiểm tập booking đầy đủ, không chỉ trang đang nhìn.',
       'Đến start không có cron/read tự ghi CLOSED.', 'Teacher không sửa ca hoặc đổi grader.'],
      'Không auto-rebook, thông báo email, Teacher phê duyệt ca hoặc đổi grader sau kết quả.')

guide(9, 'SQ09_ENTRY_TEST_BOOKING_GUIDE.md', 'Student đặt, đổi, hủy và đặt lại ca thi',
      'Student quản lý appointment của lượt thi logic thuộc Registration của mình. Tách đặt ca, đổi ca, hủy ca; đặt lại dùng cùng lượt, còn lượt hai là một Attempt mới đúng điều kiện.',
      'NV03; W07; F06–07; UC03', ['Services/Enrollment/StudentEnrollmentService.cs'],
      [('Chủ sở hữu', 'Student NORMAL, Registration của mình chưa CONFIRMED/REJECTED/CANCELLED; backend xác định lượt đủ điều kiện.'),
       ('Ca đích', 'OPEN, chưa start, còn chỗ, không trùng ca/lịch lớp của Student. Tối đa một booking SCHEDULED hiện tại cho một Attempt.'),
       ('Lượt hai', 'Lượt một có completed score; tối đa 2 Attempt; đặt lượt hai giữ BAND_ASSIGNED nhưng chặn xác nhận khi còn chờ.'),
       ('Lượt và history', 'Booking là lịch hẹn; ABSENT hoặc center cancellation cho đặt lại cùng Attempt. Không tạo Attempt thứ ba từ việc đổi/hủy/đặt lại.'),
       ('Ca nguồn', 'Student hủy/đổi booking chỉ khi ca nguồn OPEN và trước start; CLOSED chặn dù còn thời gian. Hủy Registration ở SQ07 có hợp đồng phụ thuộc khác.'),
       ('Trạng thái', 'Booking đầu chuyển PENDING→TESTING; hủy/vắng không tự kéo hồ sơ về PENDING hoặc xóa Band. Đổi sang cùng ca là no-change sau kiểm context.')],
      [flow('A — Đặt ca, gồm lượt đầu/lượt hai/đặt lại', 'Học viên', 'Giao diện đặt ca thi',
            'Đọc hồ sơ/lượt hiện tại và các ca OPEN phù hợp theo bộ lọc',
            'Kiểm ownership, trạng thái hồ sơ/lượt, completed score nếu lượt hai, chỗ và xung đột ca đích trước start',
            'Chọn ca và xác nhận đặt cho lượt đang đủ điều kiện',
            'Tạo Attempt nếu cần theo quy tắc lượt; tạo Booking SCHEDULED; cập nhật Registration cần thiết, chỗ, audit và hoàn tất nguyên tử',
            'Booking hiện tại và số lượt thực tế', 'Hồ sơ đã xác nhận, chưa có điểm lượt một, chỗ cuối bị lấy hoặc trùng lịch'),
       flow('B — Đổi ca trong cùng lượt', 'Học viên', 'Giao diện đặt ca thi',
            'Đọc booking nguồn và ca đích cùng điều kiện đổi hiện tại',
            'Kiểm nguồn OPEN/trước start, đích OPEN/trước start/còn chỗ, cùng Attempt, không xung đột và có thay đổi ca',
            'Chọn ca mới và xác nhận đổi',
            'Hủy booking nguồn, tạo booking đích cùng Attempt; cập nhật chỗ hai ca và audit trong một giao dịch',
            'Booking mới cùng lượt và history booking cũ', 'Nguồn vừa đóng/start, đích hết chỗ/trùng lịch, dữ liệu cũ hoặc cùng ca'),
       flow('C — Hủy booking', 'Học viên', 'Giao diện đặt ca thi',
            'Đọc booking và khả năng hủy của hồ sơ riêng',
            'Kiểm booking SCHEDULED, nguồn OPEN trước start, hồ sơ chưa bị khóa và phiên bản hiện tại',
            'Chọn hủy booking và xác nhận',
            'Đổi booking CANCELLED, giải phóng chỗ, giữ Attempt/history/Band và ghi audit nguyên tử',
            'Booking đã hủy; quyền đặt tiếp theo được tính lại', 'Ca CLOSED/đã start hoặc hồ sơ/booking đã đổi')],
      ['Đặt lại sau ABSENT/center-cancelled là nhánh A với Attempt có sẵn; không gọi chấm điểm hoặc tự đánh vắng.',
       'Thay bộ lọc ngày làm lựa chọn ca cũ hết hiệu lực. Ca hiển thị còn chỗ không giữ chỗ cho Student.'],
      ['Phân biệt Attempt với Booking trong note.', 'Đổi ca all-or-none, không hủy nguồn trước rồi hy vọng đặt đích.',
       'Nhánh hai người tranh chỗ cuối không quá capacity.', 'CLOSED nguồn chặn hủy/đổi booking; không nhầm với SQ07.'],
      'Không lịch thi online, tự đặt thay, tự thi lại sau xác nhận hoặc tự chọn ca khác khi xung đột.')

guide(10, 'SQ10_ENTRY_SCORE_BAND_GUIDE.md', 'Teacher chấm, ghi vắng, sửa điểm và tính Band',
      'Teacher được phân công xử lý một kết quả đầu vào sau ca kết thúc. Hệ thống đồng thời cập nhật Booking/Attempt, kết quả tốt nhất và Band; Student/ Giáo vụ xem là lượt đọc độc lập.',
      'NV04,15,22,36; W08; F08,F16–17,F25; UC03', ['Services/Enrollment/TeacherEntryService.cs'],
      [('Thời điểm', 'Initial score và ABSENT chỉ từ session end; ca không CANCELLED, Registration chưa CONFIRMED/REJECTED/CANCELLED; CLOSED không chặn.'),
       ('Điểm', '0–100, tối đa 2 số lẻ; 0 là điểm thật. ABSENT không có score và không hoàn thành kết quả.'),
       ('Sửa', 'Result COMPLETED; original score Teacher vẫn là grader được giao; hồ sơ TESTING/BAND_ASSIGNED; cần reason. Điểm bằng parsed score cũ là no-change, không ghi audit mutation chỉ vì reason khác.'),
       ('Best/Band', 'Chọn score COMPLETED cao nhất; hòa chọn Attempt 1. Band có min_score lớn nhất ≤ best; seed 0/25/45/65/85.'),
       ('Hiệu ứng', 'Điểm đầu chuyển BAND_ASSIGNED; điểm lượt hai giữ trạng thái. First completed result khóa cả bộ ngưỡng Band. Sửa hợp lệ tính lại best/Band nguyên tử.'),
       ('Phân quyền đọc', 'Student chỉ kết quả của hồ sơ mình; Giáo vụ theo dõi không có quyền chấm/sửa thay Teacher.')],
      [flow('A — Nhập điểm đầu vào', 'Giáo viên', 'Giao diện chấm thi đầu vào',
            'Đọc ca được giao, booking học viên và khả năng nhập điểm',
            'Kiểm grader hiện tại, end cutoff, Registration/booking, score hợp lệ và các phiên bản',
            'Nhập điểm và chọn lưu kết quả',
            'Ghi score/provenance, booking COMPLETED; tính best/Band, cập nhật BAND_ASSIGNED và first-use freeze; audit và hoàn tất nguyên tử',
            'Điểm đã lưu, lượt tốt nhất và Band hiện tại', 'Chưa end, mất phân công, hồ sơ đã khóa, score sai hoặc booking đã xử lý'),
       flow('B — Ghi vắng thi', 'Giáo viên', 'Giao diện chấm thi đầu vào',
            'Đọc booking chưa xử lý trong ca được phân công',
            'Kiểm từ session end, phân công và booking chưa hoàn tất, hồ sơ còn hợp lệ',
            'Chọn xác nhận vắng cho booking',
            'Ghi booking ABSENT và người/thời điểm xác nhận; giữ score NULL/Attempt; audit bắt buộc nguyên tử',
            'Booking ABSENT; học viên có thể tự đặt lại cùng lượt nếu đủ điều kiện', 'Chưa end hoặc booking đã COMPLETED/ca bị hủy/hồ sơ khóa'),
       flow('C — Sửa điểm có lý do', 'Giáo viên', 'Giao diện chấm thi đầu vào',
            'Đọc kết quả COMPLETED và context sửa hiện tại',
            'Kiểm original grader còn assigned, hồ sơ TESTING/BAND_ASSIGNED, reason, score/phiên bản và có delta thực',
            'Nhập điểm mới, lý do và chọn sửa điểm',
            'Sửa score, giữ provenance nhập đầu; tính lại best/Band; ghi old/new/reason audit và hoàn tất cùng giao dịch',
            'Điểm mới và Band tính lại', 'Đã CONFIRMED, sai grader, thiếu reason, stale hoặc điểm không đổi'),
       flow('D — Student xem kết quả', 'Học viên', 'Giao diện kết quả đầu vào',
            'Đọc kết quả các lượt và Band của hồ sơ riêng',
            'Kiểm ownership và phạm vi DTO Student', 'Mở kết quả đầu vào',
            'Chiếu các kết quả/histories được phép và best/Band hiện tại',
            'Kết quả của mình; vắng/chưa chấm hiển thị đúng nghĩa', 'Hồ sơ ngoài scope/hết phiên', read=True),
       flow('E — Giáo vụ theo dõi kết quả', 'Giáo vụ', 'Giao diện quản lý ca thi',
            'Đọc ca/kết quả/Band trong phạm vi quản lý',
            'Kiểm vai trò và dữ liệu được phép quản lý', 'Mở phần kết quả của ca/hồ sơ',
            'Chiếu trạng thái và kết quả quản lý, không cấp action chấm/sửa',
            'Kết quả/Band để theo dõi', 'Không còn quyền truy cập', read=True)],
      ['Không gọi Student xác nhận như bước tự động sau chấm. Xác nhận là SQ11 ở thời điểm khác.',
       'Một lỗi tính Band/freeze/audit làm rollback cả kết quả; không hiển thị score mới với Band cũ.'],
      ['Chấm từ end, không từ start.', 'ABSENT riêng với score 0.', 'Hòa score giữ best Attempt 1.',
       'Sửa không thay original graded_by/graded_at; lịch sử correction nằm trong audit.'],
      'Không Admin/Giáo vụ override điểm, tái chấm sau CONFIRMED hoặc tự ghi vắng từ ô trống.')

guide(12, 'SQ12_ADMISSIONS_DECISION_GUIDE.md', 'Tuyển sinh duyệt, từ chối hoặc hủy hồ sơ',
      'Admissions tra cứu hồ sơ và chọn một quyết định. Duyệt là điểm phát hành Invoice duy nhất, khác Student xác nhận nhập học và khác Accountant ghi nhận thanh toán.',
      'NV09,12,37; W10–11; F13–14; UC02', ['Services/Admissions/AdmissionsService.cs'],
      [('Duyệt/từ chối', 'Registration CONFIRMED có Band; phiên Admissions NORMAL. Từ chối cần lý do, không có Director approval tier.'),
       ('Phát hành', 'Duyệt chuyển ADMITTED và phát hành đúng một Invoice UNPAID: số duy nhất không tái dùng, snapshot học phí dương tại issue, due_date ngày VN +7 lịch; gaps số được phép.'),
       ('Hủy', 'CONFIRMED hoặc ADMITTED còn Invoice UNPAID theo guard; lý do bắt buộc; đã PAID/đã xếp lớp chặn. Không hủy hồ sơ tùy trạng thái.'),
       ('Phụ thuộc', 'Hủy Invoice UNPAID nếu có và active future bookings kể cả ca CLOSED trước start; giữ kết quả/histories đã phát sinh và chặn grading tiếp trên hồ sơ CANCELLED.'),
       ('Scope', 'Chỉ dữ liệu tuyển sinh được phép; Invoice ở màn này là trạng thái cần thiết, không phải toàn finance detail.'),
       ('Đồng thời', 'Kiểm Band/current state, invoice uniqueness, payment, membership và phiên bản cuối. Race duyệt/giá hoặc hủy/thanh toán không lưu nửa thao tác.')],
      [flow('A — Duyệt và phát hành Invoice', 'Tuyển sinh', 'Giao diện chi tiết hồ sơ tuyển sinh',
            'Đọc hồ sơ CONFIRMED, Band và preview điều kiện duyệt',
            'Kiểm Admissions, hồ sơ/Band hiện tại, học phí tại phát hành, chưa Invoice trùng và các phiên bản',
            'Chọn duyệt hồ sơ và xác nhận',
            'Chuyển ADMITTED; cấp số và Invoice UNPAID snapshot học phí/due date; audit nhóm và hoàn tất trong một giao dịch',
            'Hồ sơ ADMITTED cùng thông tin phát hành hóa đơn được phép xem', 'Hồ sơ không CONFIRMED, thiếu Band, giá/phiên bản đổi hoặc Invoice đã có'),
       flow('B — Từ chối hồ sơ', 'Tuyển sinh', 'Giao diện chi tiết hồ sơ tuyển sinh',
            'Đọc hồ sơ và điều kiện từ chối', 'Kiểm CONFIRMED, quyền, lý do và phiên bản',
            'Nhập lý do và xác nhận từ chối',
            'Chuyển REJECTED, ghi lý do/người/thời gian và kết thúc unfinished theo policy; audit bắt buộc nguyên tử',
            'Hồ sơ REJECTED với lý do chủ hồ sơ được xem', 'Sai trạng thái, thiếu lý do hoặc dữ liệu cũ'),
       flow('C — Hủy hồ sơ theo quyền Tuyển sinh', 'Tuyển sinh', 'Giao diện chi tiết hồ sơ tuyển sinh',
            'Đọc preview tác động hủy lên hồ sơ, Invoice và booking tương lai',
            'Kiểm trạng thái được hủy, lý do, chưa PAID/chưa placement, toàn tập phụ thuộc và giờ booking',
            'Nhập lý do và xác nhận hủy',
            'Chuyển CANCELLED, hủy Invoice UNPAID/booking tương lai hợp lệ, xử lý unfinished, audit nhóm và hoàn tất nguyên tử',
            'Hồ sơ đã hủy và ảnh hưởng tương ứng', 'Đã PAID/xếp lớp, sai source state, dependency/race hoặc reason thiếu')],
      ['Duyệt + ADMITTED + Invoice UNPAID là một giao dịch. Không có bước commit duyệt trước rồi gọi Accountant tạo hóa đơn.',
       'Student xác nhận SQ11 không tạo Invoice; Thanh toán SQ14 không phát hành Invoice.'],
      ['Ba action là lựa chọn, không nối tiếp trong một lần.', 'Không có Payment hoặc PAID trong nhánh duyệt.',
       'Replay duyệt không cấp Invoice thứ hai.', 'Hủy giữ lịch sử thi/Band và không refund.'],
      'Không ghi danh thay, Director duyệt, chia nhỏ giao dịch phát hành hoặc mở quyền finance detail cho Admissions.')

guide(14, 'SQ14_PAYMENT_CONFIRMATION_GUIDE.md', 'Kế toán xác nhận thu tiền và sửa metadata',
      'Kế toán ghi nhận khoản tiền đã nhận ngoài ứng dụng cho Invoice đã phát hành, hoặc sửa method/reference của Payment hiện có với lý do. Hai action có guard và hiệu ứng khác nhau.',
      'NV28; W12; F29; UC04', ['Services/Finance/FinanceService.cs', 'Common/Replay/CanonicalSchemas.cs'],
      [('Thu tiền', 'Invoice UNPAID, Registration hợp lệ; số tiền đúng toàn bộ amount snapshot, VND nguyên đồng, chỉ một Payment/Invoice.'),
       ('Phương thức', 'Mã code hiện hành CASH hoặc TRANSFER (không BANK_TRANSFER); TRANSFER cần transactionReference. Trên hình có thể ghi Tiền mặt/Chuyển khoản.'),
       ('Ngày/người', 'confirmedBy và confirmedAt từ phiên/đồng hồ server; không cho người dùng chọn ngày doanh thu quá khứ.'),
       ('Sửa metadata', 'Chỉ method/reference, reason bắt buộc; không đổi amount/paid_at/Invoice link. All-equal normalized metadata là no-change, reason khác chưa đủ tạo delta.'),
       ('Nguyên tử', 'Tạo Payment + Invoice PAID + mandatory audit + hoàn tất thao tác; sửa metadata + old/new/reason audit là giao dịch riêng.'),
       ('Race', 'Invoice/Registration/version được kiểm lại khi lưu; cancellation và payment không đồng thời thắng.')],
      [flow('A — Xác nhận đã thu đủ tiền', 'Kế toán', 'Giao diện xác nhận thanh toán',
            'Đọc Invoice, Registration, amount snapshot và khả năng xác nhận',
            'Kiểm Invoice UNPAID/chưa Payment, hồ sơ hợp lệ, amount đúng, method/reference và phiên bản',
            'Nhập phương thức/reference/số tiền và xác nhận đã thu đủ',
            'Tạo Payment với confirmer/time server; đổi Invoice PAID; audit và dấu hoàn tất nguyên tử',
            'Payment đã ghi và Invoice PAID', 'Amount khác snapshot, thiếu reference, đã PAID/hủy, dữ liệu cũ hoặc Payment trùng'),
       flow('B — Sửa phương thức/mã giao dịch', 'Kế toán', 'Giao diện xác nhận thanh toán',
            'Đọc Payment và Invoice với method/reference hiện tại',
            'Kiểm quyền, Payment/Invoice versions, metadata hợp lệ, reason và thay đổi thực',
            'Sửa method/reference, nhập lý do và xác nhận lưu',
            'Chỉ cập nhật method/reference, giữ amount/time/link; audit old/new/reason và dấu hoàn tất nguyên tử',
            'Payment với metadata mới', 'Reason thiếu, reference sai/thiếu, phiên bản cũ hoặc metadata không đổi')],
      ['Thu tiền diễn ra bên ngoài ứng dụng. Không vẽ gọi ngân hàng/cổng thanh toán rồi tự nhận callback.',
       'Metadata correction không đổi doanh thu theo ngày xác nhận; không cần tính lại amount hoặc phát hành Invoice.'],
      ['Chỉ CASH/TRANSFER theo code.', 'Payment tạo một lần và PAID cùng commit.',
       'Không làm mới confirmedAt khi sửa metadata.', 'Race hủy/thu tiền không cho PAID Invoice CANCELLED.'],
      'Không gateway, trả góp, refund, số tiền âm, ngày thu do người dùng chọn hoặc biên lai riêng mới.')

guide(16, 'SQ16_CLASS_LIFECYCLE_GUIDE.md', 'Giáo vụ quản lý lớp và giáo viên lớp',
      'Giáo vụ tạo lớp, quản lý khả năng mở/bắt đầu/hoàn tất/hủy và thay Teacher trước khi dạy. Thời gian trôi qua không tự chuyển Class state; mỗi chuyển trạng thái là một lệnh có chủ đích.',
      'NV16–17,23; W14; F19–20,F24; UC06–07',
      ['Services/Classes/ClassService.cs', 'Infrastructure/Classes/SqlClassRepository.cs'],
      [('Tạo', 'Class PLANNED, code thủ công duy nhất/bất biến, Program và Teacher hợp lệ, max_size 1–15. First Class use khóa cấu hình Program, kể cả lớp sau đó CANCELLED.'),
       ('OPEN', 'Từ PLANNED; Program ACTIVE, Teacher hợp lệ, ít nhất một LESSON tương lai; không tự mở ngay sau tạo.'),
       ('ONGOING', 'Từ OPEN; không trước first LESSON actual start, ít nhất 1 placed Student, đủ expected LESSON schedule. Không tự start do clock.'),
       ('COMPLETED', 'Từ ONGOING; mọi nghĩa vụ LESSON/FINAL_EXAM cần thiết HELD và required LESSON Attendance đầy đủ; chưa yêu cầu SUBMITTED/FINALIZED.'),
       ('CANCELLED', 'Chỉ PLANNED/OPEN trống, chưa dạy; lý do; giữ history, giải phóng upcoming resources. Không hủy lớp có Student rồi hoàn tiền/chuyển lớp.'),
       ('Teacher/capacity', 'Đổi Teacher trước ONGOING/actual teaching, chưa Attendance/Score và không xung đột tài nguyên. Capacity không thấp hơn placed count. Edit giống hiện tại không tạo mutation mới.')],
      [flow('A — Tạo lớp PLANNED', 'Giáo vụ', 'Giao diện quản lý lớp',
            'Đọc Program/Teacher có thể chọn và danh mục lớp',
            'Kiểm Program/Teacher hợp lệ, code duy nhất, capacity, cấu hình và phiên bản phụ thuộc',
            'Nhập code, Program, Teacher, sức chứa và chọn tạo lớp',
            'Tạo Class PLANNED, bảo vệ first-use freeze của Program, audit và hoàn tất nguyên tử',
            'Lớp PLANNED đã tạo', 'Code trùng, Program/Teacher không hợp lệ, cấu hình/phụ thuộc đổi hoặc capacity sai'),
       flow('B — Mở hoặc bắt đầu lớp', 'Giáo vụ', 'Giao diện quản lý lớp',
            'Đọc lớp và preview riêng cho OPEN hoặc START',
            'Kiểm source state và điều kiện OPEN/ONGOING trong bảng guard; roster, lịch, Program, Teacher và giờ hiện tại',
            'Chọn một action Mở lớp hoặc Bắt đầu lớp và xác nhận',
            'Đổi PLANNED→OPEN hoặc OPEN→ONGOING theo action; audit và hoàn tất nguyên tử',
            'Trạng thái lớp mới', 'Sai source state, chưa first start, thiếu Student/lịch hoặc phiên bản thay đổi'),
       flow('C — Hoàn tất lớp', 'Giáo vụ', 'Giao diện quản lý lớp',
            'Đọc preview completion với đầy đủ nghĩa vụ lịch và coverage điểm danh',
            'Kiểm Class ONGOING, mọi nghĩa vụ HELD và mọi required Attendance, roster/current schedule và các phiên bản',
            'Xác nhận hoàn tất lớp',
            'Chuyển Class COMPLETED và audit nguyên tử; chặn sửa Attendance, giữ GradeReport và Registration theo trạng thái hiện có',
            'Class COMPLETED; Teacher có thể gửi điểm khi đạt SQ21', 'Còn NOT_HELD/PLANNED/thiếu điểm danh hoặc dependency thay đổi'),
       flow('D — Hủy lớp chưa có học viên/chưa dạy', 'Giáo vụ', 'Giao diện quản lý lớp',
            'Đọc preview hủy lớp và lịch liên quan',
            'Kiểm PLANNED/OPEN trống, chưa teaching/Attendance/Score, reason và tập phụ thuộc hiện tại',
            'Nhập lý do và xác nhận hủy lớp',
            'Chuyển CANCELLED, ghi cancel metadata và audit; giữ history, giải phóng tài nguyên tương lai theo policy',
            'Lớp CANCELLED', 'Lớp có placement/đã dạy, source state sai hoặc dependency thay đổi'),
       flow('E — Đổi Teacher hoặc sức chứa', 'Giáo vụ', 'Giao diện quản lý lớp',
            'Đọc Teacher/capacity và khả năng sửa hiện tại',
            'Kiểm guard action, Teacher/resource conflicts hoặc capacity≥placed count, versions và delta thực',
            'Chọn một thay đổi được phép và xác nhận lưu',
            'Cập nhật trường của action, audit old/new và hoàn tất nguyên tử; không đổi Program hoặc roster',
            'Lớp với thông tin mới', 'Đã dạy, Teacher/xung đột không hợp lệ, giảm capacity dưới roster hoặc không đổi'),
       flow('F — Teacher xem lớp được giao', 'Giáo viên', 'Giao diện lớp được phân công',
            'Đọc các lớp/roster/lịch thuộc phân công hiện tại', 'Kiểm Teacher và current assignment',
            'Mở danh sách hoặc chi tiết lớp được giao', 'Chiếu roster/lịch và khả năng giảng dạy theo quyền Teacher',
            'Lớp được giao, không có quyền lifecycle Giáo vụ', 'Không còn phân công/hết phiên', read=True)],
      ['Class COMPLETED chưa công bố điểm; SQ21/SQ22 là các thao tác tiếp theo, không gọi tự động trong COMPLETE.',
       'Teacher bị khóa tài khoản không tự mất assignment hoặc được tự thay bằng người khác.'],
      ['Guard đủ từng transition, không dùng set status tùy ý.', 'COMPLETE kiểm whole coverage, không chỉ trang điểm danh.',
       'Hủy chỉ lớp trống/chưa dạy.', 'First-use freeze không được giải sau hủy lớp.'],
      'Không Director approval, automatic lifecycle, chuyển lớp/refund hoặc đổi Teacher sau teaching.')

guide(17, 'SQ17_STUDENT_CLASS_PLACEMENT_GUIDE.md', 'Giáo vụ xếp học viên vào lớp',
      'Giáo vụ chọn hồ sơ đã đủ điều kiện và lớp phù hợp. Một lần lưu tạo assignment và GradeReport DRAFT cùng giao dịch; Student không tự đăng ký lớp.',
      'NV18; W15; F22; UC06', ['Services/Classes/ClassService.cs'],
      [('Hồ sơ', 'Registration ADMITTED, Invoice PAID, đúng Student, chưa assignment; còn unfinished hợp lệ.'),
       ('Lớp', 'Class OPEN trước buổi LESSON đầu, Program/Course/Band phù hợp, còn chỗ với max_size≤15.'),
       ('Lịch', 'Student không trùng lớp/ca thi; kiểm các lịch hiện tại và Teacher/tài nguyên theo hợp đồng placement.'),
       ('Tạo dữ liệu', 'StudentClassAssignment + GradeReport DRAFT + audit bắt buộc + hoàn tất nguyên tử. Không tạo Score/Attendance hoặc điểm 0 ban đầu.'),
       ('Trạng thái', 'Registration vẫn ADMITTED; Đã xếp lớp là chỉ báo từ assignment, không status mới. Size đếm từ assignment.'),
       ('Không có lớp', 'Hồ sơ chờ lớp; không auto mở lớp, đổi Band, đổi Course hoặc hoàn tiền.')],
      [flow('A — Xếp một hồ sơ đủ điều kiện', 'Giáo vụ', 'Giao diện xếp học viên vào lớp',
            'Đọc hồ sơ đủ điều kiện và các lớp phù hợp; lấy preview placement cho hồ sơ/lớp đã chọn',
            'Kiểm ADMITTED/PAID/chưa assignment, same Course/Band, OPEN/trước first lesson/còn chỗ, toàn lịch và các phiên bản',
            'Chọn hồ sơ và lớp, xác nhận xếp',
            'Tạo assignment và DRAFT GradeReport, audit và dấu hoàn tất cùng giao dịch; giữ Registration ADMITTED',
            'Assignment, DRAFT report và roster/size hiện tại', 'Chưa PAID, sai Band/Course, lớp đã start/đầy, trùng lịch hoặc hồ sơ đã xếp')],
      ['Assignment và DRAFT report cùng thành công hoặc rollback. Student chưa được thấy giá trị bảng điểm DRAFT.',
       'Hai lệnh tranh chỗ cuối/hồ sơ cùng lúc phải kiểm lại; không over-capacity hoặc duplicate assignment.'],
      ['Không PAID→xếp lớp tự động.', 'Đếm capacity từ roster thực.', 'Không gọi Teacher tạo report sau placement.',
       'Không xóa/transfer assignment như nhánh bổ sung.'],
      'Không Student tự chọn lớp, chuyển/gỡ lớp sau xếp, auto mở lớp hoặc copy điểm từ lần học cũ.')

guide(18, 'SQ18_CLASS_SCHEDULE_GUIDE.md', 'Lập lịch, dời/thay buổi, xác nhận HELD và xem lịch',
      'Giáo vụ quản lý các buổi có ngày cụ thể và nghĩa vụ buổi học. Tách lịch tuần, buổi riêng, dời trước start, replacement sau buổi không tổ chức, xác nhận HELD và đọc lịch đúng vai trò.',
      'NV07,19,23; W16; F21,F11,F24; UC06–07',
      ['Services/Classes/ScheduleService.cs', 'Services/Classes/ClassService.cs'],
      [('Thời gian', 'Ngày/giờ VN theo phút, start<end cùng ngày; mọi root/candidate tuần/đích dời start>serverNow tại kiểm lưu, không backfill.'),
       ('Nghĩa vụ', 'LESSON count theo Program; FINAL_EXAM riêng sau last current LESSON. Replacement cùng Class/type/obligation, không tính thành thêm một nghĩa vụ.'),
       ('Xung đột', 'Khoảng [start,end), kiểm phòng/Teacher/Class/Student trên cả ca thi và lịch lớp. Hai buổi liền kề ở end=start không trùng.'),
       ('Preview', 'Ngày/count/conflicts cụ thể, không giữ chỗ/giữ đồng hồ; đổi input làm preview cũ hết hiệu lực. Một candidate sai chặn cả weekly batch.'),
       ('Move', 'Buổi nguồn trước start, không Attendance/linked Score, lớp chưa ended; đích tương lai. Toàn bộ ngày/giờ/phòng giống là no-change sau kiểm context.'),
       ('Replacement', 'Buổi đã tới start nhưng không tổ chức: reason, giữ NOT_HELD/history và tạo future replacement liên kết trong một thao tác, không xóa buổi cũ.'),
       ('HELD', 'AcademicAffairs xác nhận explicit sau scheduled end, Class ONGOING; không undo. Thời gian/Attendance không tự xác nhận HELD.')],
      [flow('A — Preview và lưu batch lịch tuần LESSON', 'Giáo vụ', 'Giao diện lịch lớp',
            'Đọc lớp/Program/lịch hiện tại; nhập ngày đầu-cuối, thứ, giờ, phòng và yêu cầu preview để xem candidate/count/conflicts',
            'Kiểm lại toàn candidates còn tương lai, count không vượt, lớp/phụ thuộc/versions và mọi xung đột',
            'Giữ đúng input đã preview và xác nhận lưu toàn batch tuần',
            'Tạo toàn bộ root LESSON/obligations và audit nhóm cùng giao dịch; một lỗi rollback toàn batch',
            'Toàn bộ lịch tuần đã lưu', 'Một ngày đã start, count vượt, xung đột mới hoặc preview/input/phiên bản không còn đúng'),
       flow('B — Tạo một buổi LESSON hoặc FINAL_EXAM', 'Giáo vụ', 'Giao diện lịch lớp',
            'Đọc lịch và điều kiện tạo buổi riêng',
            'Kiểm type/count/chronology, FINAL_EXAM sau last current LESSON, future start và xung đột',
            'Nhập ngày/giờ/phòng/type và xác nhận tạo',
            'Tạo root Schedule PLANNED/current và obligation; audit bắt buộc nguyên tử',
            'Buổi riêng đã tạo', 'Giờ không hợp lệ, quá count, sai chronology hoặc trùng tài nguyên'),
       flow('C — Dời buổi trước start', 'Giáo vụ', 'Giao diện lịch lớp',
            'Đọc occurrence nguồn, đích dời và preview tác động/xung đột',
            'Kiểm nguồn trước start/chưa Attendance hoặc linked Score, đích tương lai, chronology/resources/versions và delta thực',
            'Chọn ngày/giờ/phòng mới và xác nhận dời',
            'Dời buổi theo hợp đồng, giữ cùng nghĩa vụ và history/audit nguyên tử',
            'Lịch hiện tại sau dời', 'Nguồn đã start/có dữ liệu, đích đã tới/trùng lịch, stale hoặc không đổi'),
       flow('D — Ghi không tổ chức và tạo buổi thay thế', 'Giáo vụ', 'Giao diện lịch lớp',
            'Đọc occurrence không tổ chức và preview replacement tương lai',
            'Kiểm source occurrence/window/dependencies, reason, đích future hợp lệ/không trùng và cùng Class/type/obligation',
            'Nhập lý do, ngày/giờ/phòng buổi thay và xác nhận',
            'Giữ occurrence cũ NOT_HELD/history, tạo linked replacement current cùng obligation; audit nhóm/hoàn tất nguyên tử',
            'Buổi cũ không tổ chức và buổi thay thế tương lai', 'Nguồn/đích không hợp lệ, có dữ liệu chặn replacement, thiếu reason hoặc phụ thuộc đổi'),
       flow('E — Xác nhận đã tổ chức', 'Giáo vụ', 'Giao diện lịch lớp',
            'Đọc occurrence và preview HELD', 'Kiểm Class ONGOING, sau end, occurrence hiện tại chưa kết thúc trái policy và versions',
            'Xác nhận buổi đã tổ chức', 'Ghi occurrence HELD với metadata/audit bắt buộc và hoàn tất nguyên tử',
            'Buổi HELD; coverage nghĩa vụ được tính lại', 'Chưa end, Class không ONGOING, NOT_HELD/history hoặc occurrence đã đổi'),
       flow('F — Student xem lịch riêng', 'Học viên', 'Giao diện lớp và lịch học',
            'Đọc lớp/lịch theo StudentClassAssignment của mình', 'Kiểm Student ownership và scope đọc',
            'Mở lớp hoặc lịch học của mình', 'Chiếu lịch/nghĩa vụ hiện tại và history được phép',
            'Lịch học riêng, không có action dời/HELD', 'Ngoài ownership/hết phiên', read=True),
       flow('G — Teacher xem lịch giảng dạy', 'Giáo viên', 'Giao diện lớp và lịch dạy',
            'Đọc lịch theo current Teacher assignment', 'Kiểm Teacher/phân công hiện tại',
            'Mở lịch lớp được giao', 'Chiếu lịch và context giảng dạy được phép',
            'Lịch được giao, không có action Giáo vụ', 'Mất phân công/hết phiên', read=True)],
      ['Preview là một read riêng trước lệnh lưu. Khi vẽ A, tách request preview và return candidates trước opt xác nhận lưu; không đặt toàn batch preview và save trong một transaction.',
       'loop [Mỗi candidate tuần] chỉ đặt trong kiểm/tạo của Hệ thống; commit đặt sau loop và rollback toàn bộ nếu bất kỳ candidate không đạt.',
       'NOT_HELD cũ + replacement là một obligation. HELD là action Giáo vụ, không suy từ việc Teacher nhập Attendance.'],
      ['Từng action có guard thời gian riêng.', 'Weekly all-or-none, không skip ngày xung đột.', 'Nguồn NOT_HELD không tính progress cùng replacement.',
       'Không thêm tự bù lịch ngày nghỉ hoặc undo HELD.', 'Hai actor đọc Student/Teacher đặt ở hình riêng.'],
      'Không recurring record không ngày, cron tự HELD, holiday automation hoặc tạo extra lesson thay replacement.')

guide(19, 'SQ19_LESSON_ATTENDANCE_GUIDE.md', 'Teacher lưu điểm danh một LESSON',
      'Teacher lưu các giá trị Có mặt/Vắng được chọn cho một LESSON. Cho phép lưu một phần roster có chủ đích, nhưng tất cả giá trị gửi trong một lần phải cùng thành công hoặc rollback.',
      'NV24; W17; F26; UC07', ['Services/Classes/AttendanceService.cs'],
      [('Cửa sổ', 'Teacher hiện assigned; từ đúng LESSON start đến trước Class COMPLETED; Class không CANCELLED, LESSON không NOT_HELD.'),
       ('Đơn vị', 'Một save = một LESSON và tập changes người dùng gửi; không phải cả lớp qua mọi buổi và không ép điền mọi Student ngay lần đầu.'),
       ('Giá trị', 'Present=1 Có mặt, Present=0 Vắng; không gửi Student chưa chọn. Missing record khác Vắng; row chưa có với 0 vẫn là delta tạo mới.'),
       ('Kiểm request', 'Kiểm đầy đủ roster/phiên bản/expected absence của mọi item supplied trước no-change, kể cả item bằng giá trị cũ. Một item stale/sai abort tất cả.'),
       ('No-change', 'Chỉ actual changed subset cập nhật enteredBy/enteredAt; unchanged giữ provenance. Toàn supplied values giống thì từ chối no-change, không marker mới.'),
       ('Audit hiện tại', 'Không ghi AuditLog nghiệp vụ Attendance history riêng. Lưu latest enteredBy/enteredAt và replay completion theo code; đừng sao chép audit bắt buộc của Grade sang Attendance.')],
      [flow('A — Đọc sheet và lưu các thay đổi của một LESSON', 'Giáo viên', 'Giao diện điểm danh theo buổi',
            'Đọc LESSON được giao, roster, các Attendance đã có và capability/versions',
            'Kiểm current Teacher, giờ/occurrence/Class, full supplied versions/absence/roster và có actual delta',
            'Chọn Có mặt/Vắng cho các Student cần ghi/sửa và chọn lưu buổi',
            'Lưu actual changed Attendance với enteredBy/enteredAt và replay completion nguyên tử; không ghi AuditLog Attendance riêng',
            'Sheet mới, saved subset và phiên bản thực tế', 'Mất phân công, ngoài window, row/member stale, input sai hoặc không thay đổi')],
      ['Chưa ghi không là Vắng. Một lần save cho một LESSON; các giá trị được gửi cùng thành công hoặc rollback.',
       'Attendance không tự HELD. Thiếu required Attendance chặn Class COMPLETE; sau COMPLETE không được sửa Attendance.'],
      ['Không default-all hoặc blank→absent.', 'Có nhánh item stale dù item không đổi.', 'Có mặt/Vắng đều là thao tác chủ động.',
       'Không vẽ AuditLog history riêng hoặc Student xem điểm danh nếu không có quyền.', 'Không autosave/tự mở cửa sổ 24/48 giờ.'],
      'Không điểm danh FINAL_EXAM, đi trễ/có phép/không phép hoặc tự xác nhận buổi đã tổ chức.')

guide(20, 'SQ20_STUDENT_GRADE_DRAFT_GUIDE.md', 'Teacher lưu điểm nháp và nhận xét một học viên',
      'Teacher lưu một Student trong bảng điểm lớp được giao. Đây là đơn vị lưu Score/comment, không là gửi toàn lớp hoặc công bố kết quả.',
      'NV25–26,38; W18; F27; UC07', ['Services/Classes/GradeService.cs'],
      [('Trạng thái', 'Current Teacher assignment, Class ONGOING/COMPLETED và report DRAFT/RETURNED. SUBMITTED/FINALIZED không cho sửa nháp.'),
       ('Component', 'Theo frozen Program: GENERAL không có Schedule; LESSON/FINAL_EXAM liên kết đúng current occurrence/type, đã đến start và không NOT_HELD.'),
       ('Điểm', '0–100 tối đa 2 số lẻ; missing khác 0. Không tự xóa điểm đã lưu khi input bỏ trống hoặc tự làm tròn input sai.'),
       ('Comment', 'Bỏ trường comment: giữ hiện tại; null có chủ đích: xóa comment theo hợp đồng. Các score delta và comment của một Student cùng đơn vị lưu.'),
       ('Versions', 'Kiểm report, components, supplied rows/expected absence, linked schedules và current cohort/dependencies; sửa thực làm touch parent GradeReport version.'),
       ('Tổng', 'Tổng nháp tính khi đọc bằng server, thiếu component hiển thị Chưa đủ điểm; final_score vẫn NULL trước FINALIZED.'),
       ('No-change/audit', 'Full request validation trước no-change. Chỉ actual changed subset ghi provenance/audit theo policy; score/comment thật thay đổi cùng report RV/audit/replay nguyên tử.')],
      [flow('A — Lưu nháp một Student', 'Giáo viên', 'Giao diện sổ điểm lớp',
            'Đọc gradebook được giao, cấu hình frozen, Student report, score/comment và linked schedule context',
            'Kiểm Teacher/state/versions/full supplied intent, kind/schedule/time hợp lệ và score/comment có delta thực',
            'Nhập/sửa các điểm và nhận xét của một Student rồi chọn lưu học viên',
            'Lưu changed scores/comment của Student, touch parent report RV; audit actual effects và hoàn tất cùng giao dịch',
            'Điểm/nhận xét đã lưu, phiên bản mới và tổng nháp tính từ server khi đọc',
            'Score sai, link không hợp lệ/chưa start, report không editable, dependency stale hoặc không có delta')],
      ['Một lần lưu thuộc một Student. Gửi cả lớp là SQ21 và chỉ sau Class COMPLETED.',
       'Tổng nháp là kết quả tính khi đọc, không lưu final_score; Student không thấy DRAFT/RETURNED/SUBMITTED.'],
      ['GENERAL không có scheduleId; hai kind còn lại đúng occurrence/type.', 'Comment omitted khác null.',
       'Chưa đủ điểm khác tổng 0.', 'Touch report RV khi thay đổi thực; không touch khi no-change.',
       'Không commit từng Score rồi comment sau.'],
      'Không individual submit/publish, xóa score bằng blank, Teacher tự chốt hoặc round từng thành phần trước tổng.')

guide(21, 'SQ21_CLASS_GRADE_SUBMISSION_GUIDE.md', 'Teacher gửi và gửi lại bảng điểm cả lớp',
      'Teacher xác nhận gửi toàn bộ cohort bảng điểm. Gửi lần đầu và gửi lại sau RETURNED là hai điểm vào cho whole-Class transition, không một lệnh cho từng học viên.',
      'NV25–26; W18; F27; UC07', ['Services/Classes/GradeService.cs'],
      [('Lớp', 'Current Teacher assignment; Class COMPLETED. Đủ điểm chưa tự submit, ONGOING không submit.'),
       ('Toàn cohort', 'Mọi assignment/report hiện tại, đủ mọi required component và score-link/context đúng. Nút gửi không thay backend validation toàn lớp.'),
       ('Source state', 'Initial submit từ cohort DRAFT đúng hợp đồng; resubmit từ RETURNED. Không ép chuyển một Student hoặc bỏ qua report chưa đủ điều kiện.'),
       ('Form', 'Không còn sửa chưa lưu tại UI; phải lưu nháp từng Student ở SQ20 trước. Preview chỉ đọc và không giữ quyền gửi.'),
       ('Nguyên tử', 'Chuyển mọi report thành SUBMITTED, metadata submit/grouped audit/replay cùng giao dịch. Lỗi một report rollback mọi report.'),
       ('Công bố', 'SUBMITTED chưa visible cho Student; final_score chưa lưu. RETURNED giữ scores/comment, không bắt nhập lại từ đầu.')],
      [flow('A — Gửi lần đầu toàn lớp', 'Giáo viên', 'Giao diện sổ điểm lớp',
            'Đọc toàn bộ context lớp/gradebook và preview SUBMIT với cohort đủ điều kiện',
            'Kiểm Class COMPLETED, current Teacher, cohort DRAFT đủ score, versions/manifests và không local unsaved edit',
            'Xem ảnh hưởng và xác nhận gửi bảng điểm cả lớp',
            'Chuyển toàn cohort SUBMITTED với submit metadata/audit nhóm và completion trong một giao dịch',
            'Cả lớp SUBMITTED, chờ Giáo vụ review', 'Lớp chưa COMPLETE, thiếu score, còn edit chưa lưu hoặc report/cohort/link thay đổi'),
       flow('B — Gửi lại sau RETURNED', 'Giáo viên', 'Giao diện sổ điểm lớp',
            'Đọc lý do trả, scores/comment giữ lại; sau lưu sửa SQ20 đọc preview RESUBMIT',
            'Kiểm cohort RETURNED, Class COMPLETED, current Teacher, đủ toàn bộ điểm và context/versions hiện tại',
            'Xác nhận gửi lại bảng điểm cả lớp',
            'Chuyển toàn cohort RETURNED→SUBMITTED, cập nhật submit metadata và audit nhóm nguyên tử',
            'Cả lớp SUBMITTED ở lần gửi mới', 'Report không RETURNED, điểm/link thiếu hợp lệ hoặc có thay đổi đồng thời')],
      ['Từng Score save ở SQ20 và whole-Class submit ở SQ21 là giao dịch riêng. Không gom các lần nhập điểm trước đó vào transaction submit.',
       'RETURNED→sửa/lưu→resubmit→review có thể lặp qua nhiều lần tương tác; không bao thành vòng lặp backend tự động.'],
      ['loop [Mỗi report trong cohort] nằm trong xử lý Hệ thống và cùng một commit.', 'Cohort membership đầy đủ, không chỉ trang hiện tại.',
       'Không publish hoặc lưu final_score khi SUBMITTED.', 'Một invalid report không cho gửi phần còn lại.'],
      'Không Teacher gửi một Student, Director duyệt, auto-submit khi đủ điểm hoặc tự chốt sau resubmit.')

guide(22, 'SQ22_CLASS_GRADE_REVIEW_FINALIZATION_GUIDE.md', 'Giáo vụ trả lại/chốt và Student xem điểm đã công bố',
      'Giáo vụ review bảng điểm lớp COMPLETED đã SUBMITTED, rồi chọn RETURN hoặc FINALIZE. Student đọc FINALIZED ở thời điểm khác, không tham gia xác nhận hoặc chờ một cuộc gọi đồng bộ từ Giáo vụ.',
      'NV08,20,38; W19; F23,F12; UC07', ['Services/Classes/GradeService.cs'],
      [('Review', 'AcademicAffairs NORMAL, Class COMPLETED, toàn cohort SUBMITTED cho RETURN/FINALIZE; quyền review không gồm Teacher DRAFT values.'),
       ('RETURN', 'Reason bắt buộc, cả cohort RETURNED; giữ score/comment, metadata/reason/audit history. Không tự sửa điểm thay Teacher.'),
       ('FINALIZE', 'Kiểm toàn current cohort, Score/config/schedule/report/Registration dependencies. Tính decimal weighted total, half-up một lần 2 chữ số; lưu final_score chỉ tại đây.'),
       ('Đạt', 'FINALIZED rounded final_score≥50; không minimum component hoặc attendance passing gate mới. Thiếu điểm chặn chốt, khác kết quả không đạt.'),
       ('Hồ sơ', 'Class COMPLETED + report FINALIZED làm hết unfinished theo policy kể cả không đạt; Registration vẫn không có status COMPLETED mới.'),
       ('Bất biến', 'FINALIZED không reopen/override; không quyền chốt cho Teacher/Director/Admin. Toàn lớp + totals/metadata/unfinished/audit/replay commit cùng nhau.'),
       ('Student', 'Chỉ own FINALIZED list/detail, server total/passed/components/comment/context đã công bố; không peer/DRAFT/SUBMITTED/RETURNED values.')],
      [flow('A — Trả lại bảng điểm cả lớp', 'Giáo vụ', 'Giao diện duyệt bảng điểm',
            'Đọc review cohort và preview RETURN hiện tại',
            'Kiểm Class COMPLETED, toàn reports SUBMITTED, cohort/manifests/versions và reason hợp lệ',
            'Nhập lý do và xác nhận trả lại cả lớp',
            'Đổi toàn reports RETURNED, lưu reviewer/time/reason, giữ score/comment; grouped audit và completion nguyên tử',
            'Cả lớp RETURNED với lý do cho Teacher sửa/gửi lại', 'Thiếu reason, cohort không toàn SUBMITTED hoặc dữ liệu review cũ'),
       flow('B — Chốt/công bố bảng điểm cả lớp', 'Giáo vụ', 'Giao diện duyệt bảng điểm',
            'Đọc preview FINALIZE toàn cohort với các tổng tính từ server',
            'Kiểm COMPLETED/toàn SUBMITTED, đủ mọi component/current valid links, frozen weights và toàn manifests/versions',
            'Xác nhận chốt bảng điểm toàn lớp',
            'Tính/lưu final_score rounded một lần; mọi report FINALIZED/review metadata, xử lý unfinished và audit nhóm/completion nguyên tử',
            'Toàn lớp FINALIZED; Student được đọc kết quả riêng sau commit', 'Thiếu điểm/config không hợp lệ, report/score/roster/schedule đổi hoặc lỗi tính/ghi'),
       flow('C — Student đọc bảng điểm đã chốt', 'Học viên', 'Giao diện kết quả học tập',
            'Đọc list kết quả FINALIZED của mình, sau đó chọn detail',
            'Kiểm ownership/Student và FINALIZED hiện tại, projection không chứa quản trị hoặc peer data',
            'Mở kết quả học tập và chọn một lần học đã chốt',
            'Chiếu official score/passed/components/comment/lịch công bố được phép từ hệ thống',
            'Kết quả riêng FINALIZED; dữ liệu lịch sử hợp lệ vẫn được xem', 'Ngoài ownership, chưa FINALIZED hoặc hết phiên', read=True)],
      ['Chốt là all-or-none cho cả lớp. Không thông báo một Student thành công trong lúc các report còn đang lưu.',
       'Decimal intermediate, half-up hai chữ số một lần rồi so ≥50. Không dùng floating point hoặc round từng component.',
       'Student không nhận message tự động từ Giáo vụ; hình C là query mới sau khi đã FINALIZED.'],
      ['RETURN và FINALIZE là hai action riêng.', 'Không đồng nhất Class COMPLETE với grade FINALIZE.',
       'Unfinished được xử lý cả khi không đạt.', 'Không dữ liệu unpublished trong Student payload.',
       'Có nhánh race return/finalize và rollback toàn cohort.'],
      'Không individual finalize, mở lại FINALIZED, Teacher tự sửa khi SUBMITTED hoặc approval tier Director.')

guide(24, 'SQ24_DIRECTOR_AGGREGATE_REPORTS_GUIDE.md', 'Giám đốc xem ba báo cáo tổng hợp',
      'Director áp dụng bộ lọc để đọc tổng hợp tuyển sinh, đào tạo hoặc tài chính. Ba hình A/B/C dùng cùng kiểu query nhưng cohort và định nghĩa chỉ số khác nhau; không gộp quyền Accountant vào Director.',
      'NV30–32; W20; F31; UC08', ['Api/Reports/ReportEndpoints.cs', 'Services/Reports/SqlReportsExecutor.cs'],
      [('Scope', 'Director NORMAL; cards/group tables không định danh Student, không hidden individual payload hoặc link qua Invoice/Payment/Profile.'),
       ('Tuyển sinh', 'Cohort theo ngày tạo Registration, current state ở queried_at; people distinct Student trên toàn tập. Một Student nhiều Course: nhóm mỗi nơi 1, overall people vẫn 1.'),
       ('Đào tạo', 'Current Class inventory không lọc kỳ ngày tạo lớp; tiến độ HELD obligations/expected count, FINAL_EXAM riêng. NOT_HELD + replacement chỉ một nghĩa vụ.'),
       ('Kết quả', 'FINALIZED theo ngày finalize, kỳ mặc định tháng VN; grain enrollment-result. Không chốt chưa phải không đạt. Denominator rỗng: Chưa có dữ liệu, nonempty không ai đạt mới 0%.'),
       ('Tài chính', 'Revenue theo Payment confirmation date; billed theo Invoice issue date loại current CANCELLED; debt/overdue UNPAID hiện tại không áp kỳ. Không billed−period revenue để tính nợ.'),
       ('Query', 'Giữ bộ lọc/result/queried_at một snapshot nhất quán, totals toàn filtered set. Course lịch sử INACTIVE vẫn chọn theo phạm vi; edit filter chưa thay result trước Áp dụng.')],
      [flow('A — Báo cáo tuyển sinh', 'Giám đốc', 'Giao diện báo cáo tuyển sinh tổng hợp',
            'Mở báo cáo với bộ lọc kỳ tạo Registration/Course/state và nhóm được phép',
            'Kiểm Director, bộ lọc/cohort và projection chỉ tổng hợp', 'Áp dụng bộ lọc tuyển sinh',
            'Query snapshot toàn tập; đếm hồ sơ và distinct Student đúng cohort, nhóm/current states tại queried_at',
            'Cards/bảng nhóm/tổng/queried_at không định danh', 'Sai quyền/bộ lọc hoặc lỗi truy vấn', read=True),
       flow('B — Báo cáo đào tạo và kết quả', 'Giám đốc', 'Giao diện báo cáo đào tạo tổng hợp',
            'Mở bộ lọc current inventory và kỳ finalized-results riêng',
            'Kiểm Director và định nghĩa hai tập current inventory/finalized cohort', 'Áp dụng bộ lọc đào tạo/kết quả',
            'Query inventory/lớp/HELD obligations và cohort FINALIZED; tính tỷ lệ đạt với mẫu số đúng',
            'Tổng hợp lớp/progress/kết quả và Chưa có dữ liệu khi denominator rỗng', 'Sai bộ lọc hoặc query không hoàn tất', read=True),
       flow('C — Báo cáo tài chính quản trị', 'Giám đốc', 'Giao diện báo cáo tài chính tổng hợp',
            'Mở bộ lọc kỳ revenue/billed, Course và current overdue-debt',
            'Kiểm Director, bộ lọc và projection aggregate-only', 'Áp dụng bộ lọc tài chính',
            'Query snapshot; tính revenue/billed/current debt/overdue theo đúng ngày và tập dữ liệu',
            'Tổng hợp tài chính và queried_at, không danh sách Invoice/Payment cá nhân', 'Sai bộ lọc hoặc mất quyền/query fail', read=True)],
      ['Ba query không sửa dữ liệu, không export. Director không tham gia approval Course/Class/Invoice/grade.',
       'Overall distinct Student phải tính trực tiếp toàn tập; không cộng distinct của từng nhóm.',
       'UI hiện tại có thể tải bộ lọc mặc định khi mở. Không vẽ actor bấm Áp dụng giả cho request khởi tạo; query chủ động sau đó là lần riêng.'],
      ['Ba loại báo cáo giữ cohort khác nhau.', 'Không dùng ngày tạo Class làm inventory kỳ.',
       'Không coi unpublished grade là fail.', 'Không CSV/PDF/XLSX hoặc individual drill-down.',
       'Totals từ server, không cộng current page.'],
      'Không danh sách Student/Invoice, phê duyệt, báo cáo lợi nhuận/chi phí hoặc tự thêm biểu đồ.')

guide(25, 'SQ25_AUDIT_VIEWER_GUIDE.md', 'Admin tra cứu nhật ký và nhóm cùng thao tác',
      'Admin đọc danh sách audit, chi tiết sự kiện và operation group. Ghi audit của các SQ khác là xử lý nội bộ; Admin không phải người được gọi để ghi log hoặc người duyệt giao dịch.',
      'NV35; NV39 xuyên suốt; W21; F33; UC09', ['Api/Administration/AuditEndpoints.cs', 'Services/Administration/SqlAuditViewer.cs'],
      [('Quyền', 'Admin NORMAL, kiểm lại quyền trước trả list/detail/group. Quyền audit không cấp quyền truy cập đối tượng nghiệp vụ gốc.'),
       ('Danh sách', 'Bộ lọc theo hợp đồng hiện hành, newest-first/paging ổn định; không tải toàn bộ log xuống rồi lọc trên client.'),
       ('Chi tiết', 'Old/new/reason theo allowlist action/entity, actor/time/result/operation metadata được phép; không secret/hash/JWT/raw payload.'),
       ('Nhóm', 'Các sự kiện liên quan cùng operation_id, phân trang/giới hạn đúng hợp đồng. Đọc một nhóm không giả định từng sự kiện có action giống nhau.'),
       ('Retention', 'Mandatory business/account history giữ; pure-security/IP retention 90 ngày do vận hành. IP đã hết retention hiện Không còn lưu; app không tự khôi phục.'),
       ('Ghi log', 'Mandatory mutation audit fail rollback cả thao tác; security-only best-effort riêng. Read audit không tự audit mọi page-view.')],
      [flow('A — Tra cứu danh sách audit', 'Admin', 'Giao diện nhật ký hoạt động',
            'Mở danh sách, áp bộ lọc hoặc chọn trang theo yêu cầu', 'Kiểm Admin và bộ lọc/phân trang hợp lệ',
            'Xem/áp dụng bộ lọc nhật ký', 'Query audit summaries được phép theo thứ tự ổn định',
            'Danh sách/tổng/phân trang đúng bộ lọc', 'Sai quyền/bộ lọc hoặc lỗi đọc', read=True),
       flow('B — Xem chi tiết sự kiện', 'Admin', 'Giao diện nhật ký hoạt động',
            'Chọn audit event trong danh sách để đọc chi tiết hiện tại', 'Kiểm Admin, event tồn tại và projection fields được phép',
            'Chọn xem chi tiết sự kiện', 'Chiếu old/new/reason/actor/time/result theo action/entity allowlist, áp retention IP',
            'Chi tiết không chứa secrets hoặc quyền mở object gốc', 'Event không có/không còn truy cập hoặc query fail', read=True),
       flow('C — Xem các sự kiện cùng operation', 'Admin', 'Giao diện nhật ký hoạt động',
            'Chọn operation_id từ event để đọc nhóm sự kiện', 'Kiểm Admin, operation_id và paging/context hợp lệ',
            'Chọn xem nhóm cùng thao tác', 'Query các summaries liên quan cùng operation và trả page đúng scope',
            'Nhóm sự kiện của thao tác, không thay dữ liệu gốc', 'Sai operation/paging, mất quyền hoặc query fail', read=True)],
      ['Ghi audit bắt buộc nằm trong transaction action nguồn, không có message từ action sang Admin để xin ghi.',
       'Nhóm operation giúp giải thích dependent effects; không có chức năng undo bằng audit detail.',
       'Login/logout/security-only log có best-effort riêng, không gán chung rollback mutation cho mọi loại log.'],
      ['Mọi hình chỉ đọc; không arrow lưu/xóa audit.', 'Không link chi tiết đối tượng ngoài scope Admin.',
       'Không secrets/raw payload trong return.', 'Group theo operation_id, không gộp theo thời gian gần nhau.',
       'IP hết hạn thể hiện đúng Không còn lưu.'],
      'Không export/purge/edit/undo log, endpoint ghi log tùy ý hoặc xem lại secret từ nhật ký.')

def render_flow(f, letter):
    a, b = f['actor'], f['boundary']
    rows = []
    def msg(src, dst, label, position):
        rows.append((f'{letter}{len(rows)+1:02}', f'{src} → {dst}', label, position))
    if f['read']:
        msg(a,b,f['request'],'Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả.')
        msg(b,'Hệ thống',f['query'],'Request đọc; bắt đầu activation.')
        msg('Hệ thống','Hệ thống',f['check'],'Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại.')
        msg('Hệ thống',b,'Trả lỗi truy cập hoặc dữ liệu truy vấn',f'`alt [Không đủ điều kiện đọc: {f["fail"]}]`; không đi nhánh query thành công.')
        msg(b,a,'Hiển thị thông báo phù hợp','Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope.')
        msg('Hệ thống','Hệ thống',f['effect'],'`else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state.')
        msg('Hệ thống',b,f['result'],'`alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt.')
        msg(b,a,'Hiển thị kết quả với đúng context của request','Kết thúc thành công; rỗng hợp lệ vẫn là thành công.')
        msg('Hệ thống',b,'Trả lỗi không thể hoàn tất đọc','`else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả.')
        msg(b,a,'Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới','Kết thúc nhánh lỗi xử lý.')
    else:
        msg(a,b,'Mở chức năng ' + f['name'].split(' — ')[-1],'Điểm bắt đầu.')
        if f.get('local_form'):
            msg(b,a,'Hiển thị biểu mẫu và các vai trò được phép chọn','Dựng form tại UI; không tự thêm endpoint đọc mật khẩu/vai trò.')
        else:
            msg(b,'Hệ thống',f['query'],'Đọc/preview trước lệnh; không commit thay đổi ở bước này.')
            msg('Hệ thống','Hệ thống','Kiểm quyền/phạm vi đọc và lấy context hiện tại','Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi.')
            msg('Hệ thống',b,'Trả dữ liệu/điều kiện/phiên bản hiện tại','Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau.')
            msg(b,a,'Hiển thị dữ liệu và khả năng thao tác','Chỉ xem chưa làm thay đổi trạng thái.')
        if f.get('weekly_preview'):
            msg(a,b,'Nhập khoảng ngày, thứ, giờ, phòng và chọn xem trước lịch tuần','Chưa gửi lệnh lưu; đây là input của preview.')
            msg(b,'Hệ thống','Yêu cầu preview lịch tuần theo rule vừa nhập','Request đọc riêng trước opt xác nhận lưu.')
            msg('Hệ thống','Hệ thống','Mở rộng rule thành candidate cụ thể; kiểm count/thời gian/xung đột và xây manifest','Không tạo Schedule hoặc giữ chỗ; input sai trả lỗi, conflict trả context blocked theo hợp đồng.')
            msg('Hệ thống',b,'Trả candidate/count/conflicts/eligible và manifest khi đủ điều kiện','Chỉ preview, không ghi dữ liệu.')
            msg(b,a,'Hiển thị preview toàn batch; chỉ cho xác nhận khi đủ điều kiện','Sửa rule làm preview cũ hết hiệu lực; nếu preview bị chặn thì dừng để sửa/read lại.')
        msg(a,b,f['request'],'`opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu.')
        msg(b,'Hệ thống',f.get('command','Gửi yêu cầu ' + f['name'].split(' — ')[-1]),
            'Lệnh lưu Profile với hai phiên bản hiện tại; không request key/replay receipt.' if f.get('no_replay') else 'Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này.')
        msg('Hệ thống','Hệ thống',f['check'],'Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này.')
        msg('Hệ thống',b,'Trả lỗi điều kiện hiện tại',f'`alt [Không đạt: {f["fail"]}]`; không thực hiện hiệu ứng.')
        msg(b,a,'Hiển thị lỗi/lý do và cho tải lại context được phép','Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác.')
        msg('Hệ thống','Hệ thống',f['effect'],'`else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng.')
        msg('Hệ thống',b,f['result'],'`alt [Toàn bộ thao tác commit thành công]`; return sau commit.')
        msg(b,a,'Hiển thị kết quả đã hoàn tất' + (' và bàn giao mật khẩu tạm một lần' if f['secret'] else ''),'Kết thúc thành công; không gọi actor của bước tiếp theo tự động.')
        msg('Hệ thống','Hệ thống','Hoàn tác toàn bộ hiệu ứng của lần lưu','`else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit.')
        msg('Hệ thống',b,'Trả kết quả không thể hoàn tất','Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được.')
        msg(b,a,'Hiển thị thông báo không thể hoàn tất','Kết thúc nhánh lỗi xử lý.')
    table='| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |\n| --- | --- | --- | --- |\n'
    table+='\n'.join('| '+' | '.join(r)+' |' for r in rows)
    # Generate a scenario-specific sketch using the exact message codes and labels.
    # Short aliases keep the three lifelines legible even for long Vietnamese labels.
    diagram=['```text',f'N = {a}; G = {b}; H = Hệ thống',
             'N (Actor)              G (Boundary)               H (Control)',
             '    |                       |                          |']
    active_opt=False
    for code,direction,label,position in rows:
        if position.startswith('`opt'):
            diagram.append('+-- opt [Người dùng chủ động thực hiện] ---------------------+')
            active_opt=True
        if position.startswith('`alt [Không'):
            diagram.append('+-- alt [Guard không đạt] ----------------------------------+')
        elif position.startswith('`else [Được') or position.startswith('`else [Đủ'):
            diagram.append('|-- else [Guard đạt] ---------------------------------------|')
        elif position.startswith('`alt [Query') or position.startswith('`alt [Toàn'):
            diagram.append('| +-- alt [Xử lý/query hoàn tất] --------------------------+ |')
        elif position.startswith('`else [Query') or position.startswith('`else [Lỗi'):
            diagram.append('| |-- else [Không thể hoàn tất] ---------------------------| |')
        src,dst=direction.split(' → ')
        brief=textwrap.shorten(label,width=72,placeholder='…')
        if src==a and dst==b: line=f'    |-- {code} --> G : {brief}'
        elif src==b and dst==a: line=f'    N <.. {code} ..| : {brief}'
        elif src==b and dst=='Hệ thống': line=f'                            |-- {code} --> H : {brief}'
        elif src=='Hệ thống' and dst==b: line=f'                            G <.. {code} ..| : {brief}'
        else: line=f'                                                       H --+ {code}: {brief}\n                                                       H <--+'
        diagram.append(line)
    diagram+=['| +------------------ hết alt kết quả --------------------+ |',
              '+--------------------- hết alt guard ------------------------+']
    if active_opt: diagram.append('+--------------------- hết opt ------------------------------+')
    diagram.append('```')
    sketch='\n\n'+'\n'.join(diagram)+'\n\nTên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.\n'
    return f'### {letter}. {f["name"].split(" — ")[-1]}\n\n'+table+sketch

def render(g):
    actors = []
    for f in g['flows']:
        pair=(f['actor'],f['boundary'])
        if pair not in actors: actors.append(pair)
    source_fixes = {'Api/Enrollment/StudentEnrollmentEndpoints.cs':'Api/Enrollment/EnrollmentEndpoints.cs',
                    'Services/Reports/SqlReportsExecutor.cs':'Infrastructure/Reports/SqlReportsExecutor.cs',
                    'Services/Administration/SqlAuditViewer.cs':'Infrastructure/Administration/SqlAuditViewerExecutor.cs'}
    sources=[source_fixes.get(x,x) for x in g['sources']]
    for x in sources: assert (ROOT/'backend/src/EnglishCenter.Api'/x).is_file(), x
    s=f'# SQ{g["number"]:02} — Hướng dẫn vẽ sequence {g["title"]}\n\n'
    s+='Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.\n\n'
    s+=f'**Truy vết:** {g["trace"]}.\n\nNguồn đối chiếu:\n\n'
    s+='- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).\n- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.\n- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).\n'
    s+='\n'.join(f'- [Code: {Path(x).name}](../../../backend/src/EnglishCenter.Api/{x}).' for x in sources)
    s+='\n\n## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc\n\n'+g['goal']+'\n\n'
    s+='**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.\n\n**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.\n\n**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.\n\n'
    s+='**Cách chia hình:**\n\n'+bullets([f'**SQ{g["number"]:02}-{chr(65+i)} — {f["name"].split(" — ")[-1]}**; actor {f["actor"]}.' for i,f in enumerate(g['flows'])])+'\n\n'
    s+='Không nối các hình này như những bước luôn xảy ra sau nhau. '+g['exclusions']+'\n\n'
    s+='## 2. Các thành phần cần đặt trong EA\n\n| Trái → phải | Lifeline | Loại và trách nhiệm |\n| --- | --- | --- |\n'
    s+='| 1 | '+ '; '.join(dict.fromkeys(x[0] for x in actors))+' | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |\n'
    s+='| 2 | '+ '; '.join(dict.fromkeys(x[1] for x in actors))+' | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |\n'
    s+='| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |\n\n'
    s+='Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.\n\n'
    s+='## 3. Dữ liệu và guard nghiệp vụ\n\n| Nội dung | Quy tắc phải giữ |\n| --- | --- |\n'+'\n'.join('| '+a+' | '+b+' |' for a,b in g['rules'])+'\n\n'
    s+='Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.\n\n'
    s+='## 4. Thứ tự thông điệp và phác thảo từng hình\n\n'
    s+='Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.\n\n'
    s+='\n'.join(render_flow(f,chr(65+i)) for i,f in enumerate(g['flows']))
    s+='\n## 5. Các note nên gắn nguyên văn\n\n'
    s+='\n'.join(f'{i+1}. **“{n}”**' for i,n in enumerate(g['notes']))+'\n\n'
    if any(not f['read'] for f in g['flows']):
        s+='Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.\n\n'
        if g['number']==6:
            s+='**Riêng lưu Profile hiện tại:** không có replay receipt/lookup thao tác cho API này. Mất phản hồi thì đọc lại hồ sơ và so trạng thái hiện tại; không vẽ ref tra cứu receipt hoặc tự gửi lại PATCH với request key. Audit Profile chỉ ghi tên các trường thay đổi, không old/new PII.\n\n'
    else:
        s+='Đọc lỗi không làm dữ liệu nghiệp vụ bị hoàn tác vì đây không phải mutation. Không tự audit mọi lần đọc; khi không còn quyền, không trình bày dữ liệu cũ như kết quả mới đã xác minh.\n\n'
    s+='## 6. Thứ tự dựng sơ đồ trong EA\n\n1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.\n2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.\n3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.\n4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.\n5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.\n6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.\n\n'
    s+='## 7. Checklist trước khi hoàn tất\n\n'+bullets(g['checklist'])+'\n'
    s+='- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.\n- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.\n- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.\n\n'
    s+='Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.\n'
    return s

EXISTING = [1,3,4,5,7,11,13,15,23]
extras = {
 1: 'Giữ ngoại lệ mất phản hồi ở cuối hướng dẫn: không thêm lookup tài khoản công khai và không replay mật khẩu. Đăng ký thành công vẫn không auto-login.',
 3: 'Cập nhật 14/10: focus cửa sổ đơn thuần không gọi kiểm tra phiên. Visibility cùng trạng thái bị bỏ qua; hidden dừng thao tác/xóa RAM, hidden→visible mới kiểm lại me/CSRF. Startup/deadline/BroadcastChannel/Back-Forward/manual/401 giữ cơ chế hiện tại. Không vẽ polling hoặc idle renewal.',
 4: 'Không áp cơ chế replay plaintext hoặc lookup mật khẩu. Thành công thu hồi mọi phiên; phiên PASSWORD_CHANGE không được nâng thành NORMAL tại chỗ.',
 5: 'Reset cố ý là một action thật dù status không đổi: vẫn cấp mật khẩu tạm mới/revoke/audit. Không áp no-change chỉ vì Account status giống. Mất secret sau phản hồi hoặc đóng bàn giao: xác minh Account/operation trước; reset mới chỉ khi Admin chủ động xác nhận.',
 7: 'Student hủy Registration trước CONFIRMED có thể hủy booking tương lai ngay cả ca CLOSED; Student hủy/đổi Booking riêng ở SQ09 chỉ được ca OPEN. Không gộp hai guard này. Course create chỉ PENDING; không auto-book hoặc tạo Invoice.',
 11: 'Band hiện tại không tự cho phép xác nhận: đặt lượt hai vẫn giữ BAND_ASSIGNED nhưng pending booking/chưa chấm chặn CONFIRMED. Hệ thống kiểm kết quả/booking/manifests và phiên bản lúc lưu, không tự hủy ca trong action xác nhận.',
 13: 'In Student phải đọc mới; mất quyền hoặc fresh read lỗi thì không in bản cache như chứng từ mới. Accountant chỉ operational lookup ở hình C; không kế thừa quyền in của Student. Các POST search hiện hành vẫn là read, không mutation.',
 15: 'No-change sau kiểm quyền/state/versions: Course/Program edit toàn business values giống thì không tạo mutation/audit/replay marker mới; reason hoặc thay biểu diễn số không đủ. Band là reference-only trong UI hiện tại. Đổi tên Program là action riêng và không giải first-Class freeze.',
 23: 'Giới hạn CSV chính xác là 10.000 data records tính cả footer; 5.000.000 byte tính toàn encoded file gồm BOM/header/footer/quotes/CRLF. Đúng cap được xuất, vượt một cap từ chối toàn file trước trả CSV. Query CSV là fresh whole-filtered set, không reuse report snapshot.'
}

EXAMPLES = {
 1: [('Hai người dùng cùng email', 'Một yêu cầu được tạo, yêu cầu trùng bị chặn; không có hai Account hoặc Profile lẻ.'), ('Đăng ký thành công', 'Quay về đăng nhập theo thao tác người dùng; không tạo phiên hoặc Registration khóa học.'), ('Audit bắt buộc lỗi', 'Rollback Account và Profile; không báo thành công.')],
 2: [('Email đã được cấp', 'Không tạo Account thứ hai; không sinh/bàn giao secret thành công giả.'), ('Cấp thành công rồi đóng bàn giao', 'Không đọc lại plaintext; nếu mất secret và cần cấp lại thì Admin reset có chủ đích.'), ('Đăng nhập bằng secret sau 24 giờ', 'SQ03 chặn credential tạm hết hạn; SQ02 không gửi gia hạn tự động.')],
 3: [('Login bình thường ở thiết bị thứ hai', 'Phiên thiết bị thứ nhất không bị thu hồi chỉ vì login mới.'), ('Logout ở một browser profile', 'Thu hồi SID hiện tại; profile khác có SID riêng không bị logout-all.'), ('Focus 20 lần nhưng tab vẫn visible', 'Không thêm request kiểm phiên vì focus đơn thuần; hidden→visible mới có kiểm lại.')],
 4: [('Restricted session đổi mật khẩu thành công', 'Thu hồi mọi phiên, về login mới; không nâng restricted tại chỗ.'), ('Mật khẩu hiện tại sai', 'Không đổi hash/must-change/session state.'), ('Audit bắt buộc lỗi', 'Không để hash mới nhưng revocation cũ hoặc ngược lại.')],
 5: [('Admin khóa chính mình', 'Bị chặn; không đổi Account hoặc revoke một phần.'), ('Reset tài khoản LOCKED', 'Cấp mật khẩu tạm/revoke theo action nhưng Account vẫn LOCKED.'), ('Mở LOCKED', 'ACTIVE cho login mới, không hồi sinh SID đã revoke.')],
 6: [('Form cũ chỉ sửa địa chỉ nhưng tên đã đổi ở client khác', 'Kiểm Account/Profile versions; không ghi đè âm thầm từ form cũ.'), ('Student thay email trên request tự tạo', 'Không có quyền sửa email; không lưu field ngoài hợp đồng.'), ('Trang chủ có một report SUBMITTED', 'Không trả các giá trị chưa công bố cho Student.')],
 7: [('Hai lệnh tạo cùng Student/Course chưa kết thúc', 'Chỉ một Registration được tạo; không duplicate unfinished.'), ('Hủy Registration trước CONFIRMED có ca CLOSED chưa start', 'Hợp đồng hủy hồ sơ xử lý booking tương lai; không áp nhầm guard hủy Booking riêng.'), ('Student hủy hồ sơ CONFIRMED', 'Bị chặn; nếu cần action tuyển sinh dùng SQ12 đúng quyền.')],
 8: [('Đóng ca có 10 booking', 'Giữ 10 booking; ngăn Student đặt/hủy/đổi riêng; Teacher vẫn có thể chấm khi đạt guard.'), ('Hủy ca trước start', 'Hủy booking hoạt động nguyên tử, giữ Attempt cho Student tự đặt lại; không auto-book.'), ('Đổi grader sang cùng Teacher nhưng reason khác', 'No-change sau kiểm context; không ghi mutation audit mới.')],
 9: [('Attempt 1 ABSENT, Student chọn ca mới', 'Tạo Booking khác cho Attempt 1; không biến thành Attempt 2.'), ('Lượt 1 chưa có completed score', 'Không đặt lượt 2.'), ('Hai Student tranh chỗ cuối', 'Không vượt capacity; lệnh còn lại conflict/read lại, không tự chọn ca khác.')],
 10: [('Hai lượt đều 65 điểm', 'Best Attempt 1; Band min_score=65 theo seed; vẫn giữ cả hai kết quả.'), ('Student không tới thi, không nhập score', 'Teacher phải chủ động ghi ABSENT sau end; ô trống không tự thành vắng hoặc 0.'), ('Original grader sửa 65→64 trước CONFIRMED', 'Tính lại best/Band nguyên tử; giữ provenance chấm đầu và audit correction reason.')],
 11: [('Có Band lượt 1 nhưng lượt 2 đang SCHEDULED', 'Chặn xác nhận dù Registration vẫn BAND_ASSIGNED.'), ('Có Band, không còn lượt chờ', 'CONFIRMED + audit, chưa Invoice/ADMITTED.'), ('Grader sửa hoặc Student đặt lượt hai đồng thời', 'Kiểm lại manifests/state; không xác nhận với context nửa cũ nửa mới.')],
 12: [('Student CONFIRMED, Admissions duyệt', 'ADMITTED và đúng một Invoice UNPAID cùng commit; không PAID/placement.'), ('Payment thắng race trước hủy', 'Lệnh hủy bị chặn; không tạo Invoice CANCELLED đã PAID.'), ('Audit phát hành lỗi', 'Không giữ Registration ADMITTED mà thiếu Invoice hoặc ngược lại.')],
 13: [('Invoice đang xem UNPAID vừa được thu tiền', 'Student in đọc mới và nhận trạng thái hiện tại; không in cache như dữ liệu mới.'), ('Student đổi ID sang Invoice người khác', 'Không trả detail hoặc cho in.'), ('Fresh read khi in lỗi', 'Không fallback in bản cũ như chứng từ đã xác minh.')],
 14: [('Invoice 2.000.000 VND, nhập 1.000.000', 'Không partial payment; không tạo Payment hoặc PAID.'), ('TRANSFER thiếu reference', 'Lỗi validation, không ghi.'), ('Sửa reference sau khi PAID', 'Giữ amount/confirmedAt/link; doanh thu theo ngày xác nhận cũ.')],
 15: [('Program đã có Class nhưng Class CANCELLED', 'Cấu hình vẫn freeze; hủy lớp không mở quyền sửa weight/count.'), ('DRAFT tổng weight 90', 'Có thể lưu các field hợp lệ, nhưng không ACTIVE cho đến tổng 100 và đúng một FINAL_EXAM.'), ('Course tăng học phí', 'Invoice đã issue giữ snapshot; chỉ Invoice phát hành sau lấy giá mới.')],
 16: [('Lớp đủ HELD nhưng thiếu một Attendance bắt buộc', 'COMPLETE bị chặn, không tự ghi Vắng cho row thiếu.'), ('Class COMPLETE khi GradeReport còn DRAFT', 'Được nếu coverage giảng dạy/Attendance đạt; không tự FINALIZE.'), ('Lớp OPEN có 1 Student, chọn CANCEL', 'Bị chặn; không tự chuyển Student hoặc refund.')],
 17: [('ADMITTED nhưng Invoice UNPAID', 'Không xếp lớp.'), ('Hai hồ sơ tranh vị trí thứ 15', 'Không over-capacity; tạo assignment/report nguyên tử cho yêu cầu hợp lệ.'), ('Tạo assignment xong gặp lỗi tạo report', 'Rollback assignment, không giữ Student đã xếp mà thiếu DRAFT report.')],
 18: [('Preview thứ Hai/thứ Tư đạt, khi lưu thứ Tư trùng phòng', 'Rollback cả batch, không chỉ lưu thứ Hai.'), ('Buổi không tổ chức được thay', 'Old NOT_HELD và future replacement cùng obligation; progress không đếm 2 buổi.'), ('serverNow đúng end của occurrence current PLANNED, Class ONGOING', 'HELD được phép theo predicate ≥end; không đòi chờ thêm phút.')],
 19: [('Chưa có Attendance, Teacher chọn Vắng', 'Tạo row present=0 là thay đổi thật; missing không tương đương 0.'), ('Một item unchanged nhưng RV stale trong batch', 'Validation chặn toàn lần lưu, không bỏ qua item để lưu phần khác.'), ('LESSON HELD, Class chưa COMPLETE', 'Có thể sửa Attendance trong cửa sổ; HELD không tự khóa như Class COMPLETED.')],
 20: [('Hai component 50/50, mới có một score', 'Tổng hiển thị Chưa đủ điểm; không gán score thiếu thành 0.'), ('Comment omitted và scores có delta', 'Giữ comment; null explicit mới xóa comment theo hợp đồng.'), ('Nhập lại score đã lưu với same schedule, comment không đổi', 'No-change sau full validation; không touch parent RV hoặc tạo marker mới.')],
 21: [('14 Student đủ điểm, Student thứ 15 thiếu một component', 'Không submit 14 người trước; cả lớp bị chặn.'), ('Class ONGOING nhưng đã nhập đủ điểm', 'Không submit; phải Class COMPLETED.'), ('Teacher đang có local edits chưa lưu', 'UI không gửi toàn lớp; lưu nháp rồi đọc preview/context mới.')],
 22: [('Hai score 49,99 và 50,00, weight mỗi component 50%', 'Tổng decimal 49,995; half-up một lần thành 50,00 và đạt sau FINALIZED.'), ('Kết quả chốt 40,00', 'Không đạt nhưng report FINALIZED và hồ sơ hết unfinished theo policy, không giữ hồ sơ mở chỉ vì trượt.'), ('Một report lỗi khi finalize', 'Rollback cả cohort/totals/unfinished/audit; Student không thấy công bố từng phần.')],
 23: [('9.999 dòng nhóm + 1 footer', 'Đúng 10.000 data records; được xuất nếu encoded bytes không vượt cap.'), ('10.000 dòng nhóm + footer', '10.001 data records; từ chối toàn file, không cắt footer.'), ('Payment mới sau query trang nhưng trước export', 'Fresh CSV có thể khác report đang mở, queried_at riêng; không reuse snapshot cũ.')],
 24: [('Một Student đăng ký 2 Course trong cohort', 'Distinct mỗi nhóm Course là 1, overall people là 1; không cộng thành 2.'), ('Không có kết quả FINALIZED trong kỳ', 'Pass rate Chưa có dữ liệu, không giả 0%.'), ('Director yêu cầu CSV hoặc Invoice detail', 'Ngoài scope; không cấp quyền vì dùng cùng metric tài chính.')],
 25: [('Admin mở event Grade finalize', 'Xem allowed audit detail/group; không tự có quyền xem sổ điểm Student qua object link.'), ('IP hết retention', 'Hiển thị Không còn lưu, không bịa IP hoặc phục hồi dữ liệu.'), ('Một operation có nhiều dependent events', 'Group theo operation_id, không gộp chỉ vì cùng timestamp.')],
}

def example_section(n):
    return '\n\n## Ví dụ tự kiểm tra khi vẽ\n\nCác tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.\n\n| Tình huống | Kết quả phải thể hiện |\n| --- | --- |\n' + '\n'.join('| '+a+' | '+b+' |' for a,b in EXAMPLES[n])+'\n'

def copy_reviewed_existing(n):
    matches=list((ROOT/'docs/design').glob(f'SQ{n:02}_*_GUIDE.md'))
    assert len(matches)==1
    src=matches[0]
    content=src.read_text(encoding='utf-8').replace('Ngày: 02/10/2026.', 'Ngày cập nhật: 14/10/2026.')
    # Preserve the detailed manually authored messages while making all source links valid in the new folder.
    def fix_link(m):
        label,target=m.group(1),m.group(2)
        if target.startswith(('http:', 'https:', '#')) or target.startswith('SQ'): return m.group(0)
        if target=='SEQUENCE_PLAN.md': target='../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md'
        else: target='../'+target
        return f'[{label}]({target})'
    content=re.sub(r'\[([^\]]+)\]\(([^)]+)\)',fix_link,content)
    content=content.replace('không sử dụng các refinements E04 chưa duyệt.',
                            'đối chiếu thêm hợp đồng và triển khai hiện hành khi kiểm tra phiên bản và kết quả thao tác.')
    content=content.replace('không sử\ndụng DTO/refinement mới chưa duyệt làm quy tắc nghiệp vụ.',
                            'đối chiếu thêm hợp đồng và triển khai hiện hành; cách vẽ không tạo quy tắc mới.')
    content+='\n\n## Đối chiếu triển khai hiện tại — 14/10/2026\n\n'+extras[n]+'\n\n'
    content+='Bộ hướng dẫn này là tài liệu vẽ mức nghiệp vụ; các đoạn thể hiện cơ chế triển khai dùng note/ref theo [README](README.md). Quyền, phiên bản, cohort/membership và điều kiện thời gian được backend kiểm hiện tại; nút/preview cũ không bảo đảm ghi thành công. Cùng key/content đã hoàn tất không tạo hiệu ứng thứ hai; không tự gửi lại khi mất phản hồi. Mandatory audit có rollback theo action; login/logout security-only vẫn best-effort, không audit mọi read.\n\n'
    content+='Nguồn cập nhật: [CURRENT_STATE](../../CURRENT_STATE.md), [SESSION_FOCUS_REVIEW](../../plan/SESSION_FOCUS_REVIEW.md), [E04_REPLAY_SPEC](../E04_REPLAY_SPEC.md), [E04_FINAL_CONTRACT](../E04_FINAL_CONTRACT.md) và [LocalRuntimeServices](../../../backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs). Các hướng dẫn SQ cũ và Word nguồn được giữ nguyên.\n'
    if n==23:
        content+='\nSáu cột CSV đúng code: Mã khóa; Tên khóa; Doanh thu trong kỳ (VND); Phải thu trong kỳ (VND); Công nợ hiện tại (VND); Công nợ quá hạn (VND). Footer Tổng toàn bộ tập lọc. [FinanceCsv.cs](../../../backend/src/EnglishCenter.Api/Services/Reports/FinanceCsv.cs) là điểm đối chiếu mã hóa/cap.\n'
    content+=example_section(n)
    (DEST/src.name).write_text(content,encoding='utf-8')
    return src.name,content.splitlines()[0].removeprefix('# ')

def build():
    GUIDES[0]['flows'][0]['local_form']=True
    # Refinements verified against current Class/Attendance/Grade/Schedule predicates.
    by_num={g['number']:g for g in GUIDES}
    by_num[6]['sources'].append('Infrastructure/Enrollment/SqlOwnProfileExecutor.cs')
    by_num[6]['rules'][5]=('Hợp đồng Profile riêng', 'Backend hiện tại kiểm hai phiên bản rồi lưu/audit changedFields; không nhánh reject toàn dữ liệu giống như các lệnh replay. Không securityVersion increment/revoke-all và không replay completion/receipt. Không ép quy tắc no-change chung vào SQ06.')
    by_num[6]['flows'][1]['check']='Kiểm ownership, phiên bản Account/Profile, dữ liệu hợp lệ và SĐT duy nhất; không tự áp reject-all-equal của lệnh replay'
    by_num[6]['flows'][1]['effect']='Cập nhật các trường Account/Profile được phép và mandatory audit chỉ changedFields trong cùng giao dịch; không revocation hoặc replay completion'
    by_num[6]['flows'][1]['fail']='Sai dữ liệu, SĐT trùng hoặc phiên bản Account/Profile cũ'
    by_num[6]['flows'][1]['no_replay']=True
    by_num[6]['notes'].append('Lưu Profile không có replay completion hoặc lookup receipt. Audit chỉ tên trường thay đổi; nếu request toàn giá trị giống thì không tự vẽ nhánh backend NO_CHANGE không có trong code.')
    by_num[16]['rules'][0]=('Tạo', 'Class PLANNED, code thủ công duy nhất/bất biến, Program ACTIVE với cấu hình hợp lệ, Teacher ACTIVE, max_size 1–15. First Class use khóa cấu hình Program, kể cả lớp sau đó CANCELLED.')
    by_num[16]['rules'][5]=('Teacher/capacity', 'Đổi Teacher hoặc capacity chỉ khi BeforeTeaching: PLANNED/OPEN, chưa Attendance/Score, chưa current LESSON tới start hoặc HELD. Teacher mới ACTIVE/không xung đột; capacity 1–15 và không thấp hơn placed count. All-equal edit không tạo mutation mới.')
    by_num[16]['rules'][3]=('COMPLETED', 'Từ ONGOING, có Student; current LESSON count đúng expected, đúng một current FINAL_EXAM; tất cả current obligations HELD và Attendance đủ mỗi Student/mỗi LESSON. Chưa yêu cầu grades SUBMITTED/FINALIZED.')
    by_num[17]['rules'][0]=('Hồ sơ', 'Registration ADMITTED, Invoice PAID, Student Account ACTIVE, chưa assignment; còn unfinished hợp lệ.')
    by_num[17]['rules'][1]=('Lớp', 'Class OPEN trước current LESSON đầu, Teacher ACTIVE hợp lệ, cùng Course/Band, còn chỗ với max_size≤15.')
    by_num[18]['rules'][4]=('Move', 'Nguồn current PLANNED trước start, không Attendance/linked Score, Class không COMPLETED/CANCELLED và Teacher hợp lệ; đích future đúng chronology. All-equal ngày/giờ/phòng là no-change sau kiểm context.')
    by_num[18]['rules'][5]=('Replacement', 'Nguồn current PLANNED đã tới start, chưa Attendance/linked Score, Class chưa ended và Teacher hợp lệ; reason bắt buộc, đích future đúng chronology. Giữ nguồn NOT_HELD/history và tạo linked replacement cùng Class/type/obligation nguyên tử.')
    by_num[18]['rules'][6]=('HELD', 'Giáo vụ explicit, Class ONGOING, occurrence current PLANNED và serverNow≥end (ngay end được phép); không undo. Thời gian/Attendance không tự HELD.')
    by_num[18]['flows'][0]['query']='Đọc lớp/Program/current schedules và context lập lịch tuần'
    by_num[18]['flows'][0]['weekly_preview']=True
    by_num[18]['flows'][4]['check']='Kiểm Class ONGOING, serverNow≥end, occurrence current PLANNED và full manifest/versions'
    by_num[19]['rules'][0]=('Cửa sổ', 'Teacher hiện assigned; serverNow≥LESSON start; Class không COMPLETED/CANCELLED, LESSON current và không NOT_HELD. LESSON HELD vẫn được sửa trong cửa sổ nếu Class chưa COMPLETED.')
    by_num[21]['rules'][2]=('Source state', 'Mỗi report của toàn cohort phải DRAFT hoặc RETURNED theo predicate backend hiện tại; không SUBMITTED/FINALIZED. Hình A minh họa gửi đầu, hình B minh họa gửi lại sau return; không thêm một invariant đồng nhất khác mà code không kiểm.')
    for f in by_num[21]['flows']:
        f['check']='Kiểm Class COMPLETED, current Teacher, full cohort/versions/manifests, đủ mọi score/current links; mỗi report DRAFT hoặc RETURNED theo predicate và không local unsaved edit'
    commands={
        2:['Gửi yêu cầu cấp tài khoản nhân sự'],
        6:[None,'Gửi yêu cầu lưu hồ sơ cá nhân'],
        8:['Gửi yêu cầu tạo hoặc sửa thông tin ca','Gửi lệnh mở hoặc đóng ca đã chọn','Gửi yêu cầu hủy ca với lý do','Gửi yêu cầu đổi giáo viên chấm với lý do',None],
        9:['Gửi yêu cầu đặt ca cho lượt đủ điều kiện','Gửi yêu cầu đổi booking sang ca mới cùng lượt','Gửi yêu cầu hủy booking hiện tại'],
        10:['Gửi yêu cầu lưu điểm đầu vào','Gửi yêu cầu ghi vắng booking','Gửi yêu cầu sửa điểm với lý do',None,None],
        12:['Gửi yêu cầu duyệt hồ sơ','Gửi yêu cầu từ chối hồ sơ với lý do','Gửi yêu cầu hủy hồ sơ với lý do'],
        14:['Gửi yêu cầu xác nhận thu đủ tiền','Gửi yêu cầu sửa method/reference với lý do'],
        16:['Gửi yêu cầu tạo lớp PLANNED','Gửi lệnh OPEN hoặc START đã chọn','Gửi lệnh COMPLETE lớp','Gửi lệnh CANCEL lớp với lý do','Gửi lệnh đổi Teacher hoặc capacity đã chọn',None],
        17:['Gửi yêu cầu xếp hồ sơ vào lớp'],
        18:['Gửi yêu cầu lưu toàn bộ lịch tuần đã preview','Gửi yêu cầu tạo một buổi cụ thể','Gửi yêu cầu dời occurrence trước start','Gửi yêu cầu ghi NOT_HELD và tạo replacement','Gửi yêu cầu xác nhận occurrence HELD',None,None],
        19:['Gửi yêu cầu lưu tập điểm danh một LESSON'],
        20:['Gửi yêu cầu lưu điểm và nhận xét một Student'],
        21:['Gửi yêu cầu SUBMIT cả lớp','Gửi yêu cầu RESUBMIT cả lớp'],
        22:['Gửi yêu cầu RETURN cả lớp với lý do','Gửi yêu cầu FINALIZE cả lớp',None]
    }
    for n,labels in commands.items():
        assert len(labels)==len(by_num[n]['flows'])
        for f,label in zip(by_num[n]['flows'],labels):
            if label: f['command']=label
    for f,label in zip(by_num[24]['flows'],[
        'Yêu cầu báo cáo tuyển sinh theo kỳ tạo Registration/Course/state',
        'Yêu cầu báo cáo inventory hiện tại và kết quả theo kỳ FINALIZED riêng',
        'Yêu cầu báo cáo tài chính tổng hợp theo kỳ revenue/billed và lọc current debt']):
        f['query']=label;f['fail']='Sai quyền Director hoặc bộ lọc không hợp lệ'
    by_num[22]['flows'][2]['name']='C — Student xem danh sách kết quả đã chốt'
    by_num[22]['flows'][2]['request']='Mở danh sách kết quả học tập đã chốt'
    by_num[22]['flows'][2]['query']='Yêu cầu danh sách FINALIZED của Student hiện tại theo lọc/phân trang'
    by_num[22]['flows'][2]['result']='Danh sách các lần học riêng đã FINALIZED, đúng paging và scope'
    by_num[22]['flows'].append(flow('D — Student xem chi tiết một kết quả đã chốt', 'Học viên', 'Giao diện kết quả học tập',
            'Yêu cầu chi tiết kết quả FINALIZED đã chọn',
            'Kiểm Student hiện tại, ownership, report FINALIZED và scope dữ liệu công bố',
            'Chọn một kết quả đã chốt để xem chi tiết',
            'Chiếu official score/passed/components/comment/context lịch công bố được phép, không peer hoặc management data',
            'Chi tiết lần học riêng FINALIZED', 'Report không FINALIZED hoặc ngoài ownership', read=True))
    for f,label,fail in zip(by_num[25]['flows'],[
        'Yêu cầu danh sách audit theo bộ lọc và trang hiện tại',
        'Yêu cầu chi tiết audit event đã chọn',
        'Yêu cầu danh sách sự kiện cùng operation_id với phân trang'],[
        'Sai quyền Admin hoặc bộ lọc/phân trang sai', 'Sai quyền Admin hoặc event không truy cập được', 'Sai quyền Admin hoặc operation/paging không hợp lệ']):
        f['query']=label;f['fail']=fail
    entries=[]
    for g in GUIDES:
        (DEST/g['filename']).write_text(render(g)+example_section(g['number']),encoding='utf-8')
        entries.append((g['number'],g['filename'],f'SQ{g["number"]:02} — {g["title"]}'))
    for n in EXISTING:
        name,title=copy_reviewed_existing(n);entries.append((n,name,title))
    entries.sort()
    assert [x[0] for x in entries]==list(range(1,26))
    readme='''# Bộ hướng dẫn vẽ sequence theo dự án — 14/10/2026

Mỗi mã SQ01–SQ25 có **một file Markdown độc lập**. Các thao tác khác actor/điểm bắt đầu/đơn vị giao dịch tách thành hình A/B/C trong cùng file. Không cần vẽ chung tất cả nhánh vào một hình lớn.

Bộ này lấy SQ01_REGISTRATION_GUIDE.md làm mẫu: mục tiêu và ranh giới → lifeline EA → dữ liệu/guard → bảng thông điệp và đường gạch → note → cách dựng → checklist. Chín hướng dẫn đã có được giữ phần thông điệp chi tiết, cập nhật nguồn/ngoại lệ hiện hành trong bản sao mới; mười sáu hướng dẫn còn lại được viết riêng theo các workflow và code đã triển khai. File hướng dẫn cũ và báo cáo Word không thay đổi.

## Danh mục

| Mã | Hướng dẫn |
| --- | --- |
'''
    readme+='\n'.join(f'| SQ{n:02} | [{title.split(" — ",1)[-1]}]({name}) |' for n,name,title in entries)
    readme+='''

## Cách dùng

1. Mở file SQ cần vẽ, đọc mục tiêu/guard trước khi kéo lifeline.
2. Với file có nhiều hình, tạo từng Sequence Diagram SQxx-A/B/C; đặt đúng actor của hình đó. Một vai trò được nhắc trong dữ liệu không nhất thiết là actor tham gia.
3. Đặt Actor → Boundary → Control từ trái sang phải. Dùng self-message ở Control để chỉ kiểm/hiệu ứng; return nét đứt; activation kết thúc tại phản hồi của từng nhánh.
4. Các mã A01/B01/M01 không là API; thứ tự trên bảng và vị trí fragment có thẩm quyền. Nhánh lỗi và thành công loại trừ nhau, không vẽ nối tiếp hết mọi mã.
5. Phác thảo đường gạch giúp bố cục; mỗi hình vẫn lấy **nhãn cụ thể ở bảng thông điệp**, không dùng tất cả mũi tên chung như “Xử lý”. Với form cục bộ không gọi server chỉ để dựng form; với read không thêm read-context rồi lại query nếu code chỉ có một request.
6. Note/ref dùng cho cơ chế chung; không thêm lifeline DB/controller/service trừ khi chủ động vẽ một sơ đồ kiến trúc khác mức.

## Cơ chế chung phải giữ

### Phiên và quyền

Backend kiểm quyền/ownership/task hiện tại; UI capability hoặc preview không là cấp quyền. Student/staff dùng NORMAL trừ PASSWORD_CHANGE gate chỉ đổi mật khẩu/logout. NORMAL tối đa 2 giờ/idle 30 phút; restricted tối đa 10 phút không vượt hạn mật khẩu tạm.

Đến ngày 14/10, focus cửa sổ đơn thuần và visibility event lặp cùng trạng thái không kiểm phiên. Hidden pause/xóa RAM; hidden→visible kiểm me/CSRF mới. Startup/deadline/BroadcastChannel/Back-Forward/manual và 401 giữ cơ chế hiện tại. Không polling hoặc keep-alive để kéo dài idle. Hết quyền/phân công thì không trả dữ liệu cũ như đã xác minh.

### Preview, giao dịch và no-change

Preview/read không giữ chỗ/tài nguyên/đồng hồ; recheck lúc lưu. Backend kiểm supplied versions/absence/full membership và quan hệ trước no-change; không dùng only-changed subset để bỏ qua item supplied stale. New intent toàn business values giống bị từ chối, không marker/audit mutation mới. Reset mật khẩu chủ động là ngoại lệ thao tác thật theo policy.

Phạm vi no-change/replay trên áp dụng các command có cơ chế đó. **Profile SQ06 có hợp đồng riêng**: kiểm hai RV, audit changedFields; không replay completion/receipt, không nhánh backend reject-all-equal, không revoke-all. Không gắn quy tắc chung vào API Profile hoặc credential.

Các đơn vị lưu khác nhau: Attendance một LESSON/tập supplied changes; draft score/comment một Student; submit/return/finalize toàn Class; weekly lịch toàn batch. Loop xử lý items bên trong **một** đơn vị nguyên tử không là gọi/commit từng item độc lập. Không commit một phần nếu một item không hợp lệ.

### Audit đúng loại

Mandatory audit thuộc đúng danh mục action, lưu cùng mutation và rollback khi lỗi. Login/logout/security-only best-effort riêng. **Attendance hiện tại không có AuditLog lịch sử nghiệp vụ riêng**, giữ latest enteredBy/enteredAt/replay; không copy audit của Grade. Read/search/report/print/export không tự audit mọi page-view. Không secrets/raw payload trong log.

### Mất phản hồi và replay

Mất phản hồi sau gửi lệnh khác lỗi rollback đã được xác định. Không tự gửi lại hoặc đổi request key để thử lại mù. Tra cứu state/operation theo scope/họ endpoint hiện hành; nếu cần vẽ chi tiết dùng ref khôi phục thủ công. Cùng key/content hoàn tất không sinh hiệu ứng lần hai; quyền hiện tại vẫn kiểm. Không bịa lookup endpoint chung. Không replay plaintext credential, không xem lại mật khẩu tạm từ log/receipt. SQ01 không có lookup account công khai để kiểm secret.

### Read và khởi tạo màn hình

UI hiện tại có scoped initial reads. Khi hình bắt đầu bằng mở màn hình, có thể gửi query bộ lọc mặc định ngay; không vẽ click Áp dụng giả. Các POST search vẫn là read. Chỉnh filter chưa áp dụng không thay context kết quả cũ. Response cũ bị loại theo request/epoch; không biến cancellation read thành rollback mutation.

## Những ranh giới dễ vẽ sai

- Student xác nhận nhập học chưa tạo Invoice. Admissions duyệt mới ADMITTED + Invoice UNPAID nguyên tử; payment không tạo Invoice.
- Phương thức tiền trong code: CASH/TRANSFER, không BANK_TRANSFER; dùng nhãn Tiền mặt/Chuyển khoản nếu vẽ tiếng Việt.
- ABSENT không là score 0; Booking history khác Attempt, tối đa 2 Attempt không giới hạn số lịch hẹn bằng 2.
- Program first-Class freeze không được giải khi Class CANCELLED; Band reference-only không UI editor.
- HELD do Giáo vụ sau end; Attendance/time không tự HELD. Class COMPLETED chưa grade FINALIZED.
- Draft total tính khi đọc, final_score chỉ lưu khi chốt; Student chỉ own FINALIZED.
- Director aggregate-only không CSV/phê duyệt; Accountant CSV query mới toàn filtered set, 6 cột + footer, 10.000 data records gồm footer/5.000.000 byte.
- 7 roles cố định, 37 NV còn hiệu lực (trừ NV10/NV11), 32 F (trừ F15), 21 W. NV39 xuyên các action có audit, không actor thứ tám.

## Nguồn và phạm vi kiểm tra

[Danh mục cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md), [FUNCTION_SPEC](../FUNCTION_SPEC.md), [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), [requirements](../../requirements.md), các hợp đồng E04 và code từng module được dẫn trong file.

M0–M3 và phần reports/CSV/audit viewer M4 đã triển khai trên main/host LOCAL_DEV tường minh. UI refresh đã áp dụng, owner gate4 còn pending; backup/restore M4 cần đầu vào vận hành, không là sequence đã hoàn thành trong bộ này. Hướng dẫn là tài liệu vẽ, không tuyên bố chạy lại bộ kiểm thử sản phẩm.
'''
    (DEST/'README.md').write_text(readme,encoding='utf-8')
    return entries

if __name__=='__main__':
    result=build()
    print(f'Built {len(result)} guides plus README: {DEST}')
