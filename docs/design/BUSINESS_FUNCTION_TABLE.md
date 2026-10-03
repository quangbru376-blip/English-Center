# III. Bảng chức năng nghiệp vụ

Cập nhật ngày 02/10/2026 theo các dữ kiện đã chốt trong requirements và đặc
tả chức năng E01, workflow E02, cùng các quyết định bổ sung đã duyệt.

Giữ cấu trúc bảng và STT của báo cáo để đối chiếu. Có **37 chức năng còn
hiệu lực**; STT 10 và 11 (ghi danh, đặt ca thi thay học viên) đã loại.
Cột **Biểu mẫu** chỉ tên nội dung hiển thị liên quan, không cấp thêm chức
năng tạo chứng từ, ký duyệt hoặc xuất file. Các vai trò nhân sự chỉ truy cập
dữ liệu trong phạm vi được phân quyền; mỗi tài khoản có một vai trò cố định.

## 1. Học viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 1 | Đăng ký, đăng nhập và quản lý thông tin cá nhân | Học viên tự tạo tài khoản bằng họ tên, ngày sinh, email, số điện thoại, mật khẩu; địa chỉ tùy chọn. Email và số điện thoại không trùng. Đăng nhập bằng email/mật khẩu khi tài khoản ACTIVE. Chỉ sửa họ tên, ngày sinh, số điện thoại, địa chỉ của mình; email đăng nhập không đổi. | — | Đăng nhập dùng chung cho 7 vai trò. Người dùng được đổi mật khẩu bằng mật khẩu hiện tại và đăng xuất; quên mật khẩu do Admin xử lý. Không tự đăng ký tài khoản nhân sự. |
| 2 | Xem khóa học, đăng ký và hủy hồ sơ đăng ký | Chỉ tạo hồ sơ cho Course ACTIVE; mỗi học viên chỉ có một hồ sơ chưa kết thúc cho cùng khóa. Học viên chỉ hủy trước CONFIRMED. Khóa học của hồ sơ đã tạo không được đổi. | Hồ sơ đăng ký khóa học | Chọn khóa khác cần hủy hợp lệ và tạo hồ sơ mới, thi đầu vào lại; không sao chép điểm/Band cũ. Hủy giữ lịch sử, đồng thời hủy booking tương lai và Invoice UNPAID nếu có. Hồ sơ học chỉ kết thúc khi lớp COMPLETED và bảng điểm FINALIZED, kể cả không đạt; CANCELLED/REJECTED cũng là kết thúc. |
| 3 | Đăng ký, đổi, hủy và xem lịch thi đầu vào | Ca thi phải OPEN, chưa bắt đầu, còn chỗ và không trùng lịch của học viên. Tối đa hai lượt thi mỗi hồ sơ; lượt hai chỉ đặt sau khi có điểm lượt một và trước CONFIRMED. Đổi/hủy booking chỉ khi ca nguồn OPEN và chưa bắt đầu. | Lịch và kết quả thi đầu vào | Vắng hoặc ca bị trung tâm hủy được đặt lại cùng lượt, không tính thành lượt thứ ba. Lịch sử booking khác số lượt thi. Ca CLOSED chặn thao tác booking của học viên. |
| 4 | Xem kết quả thi đầu vào và Band | Chỉ xem kết quả hồ sơ của mình; hiển thị điểm từng lượt đã chấm, điểm tốt nhất và Band tương ứng. | Lịch và kết quả thi đầu vào | Chọn điểm cao nhất trong các lượt COMPLETED; bằng điểm chọn lượt một và vẫn giữ cả hai kết quả. Vắng thi không được tính là điểm 0. |
| 5 | Xác nhận nhập học | Chỉ xác nhận hồ sơ BAND_ASSIGNED có Band hợp lệ, không còn lượt hai chờ thi hoặc đã thi chưa chấm. | Hồ sơ đăng ký khóa học | Sau xác nhận, hồ sơ chuyển CONFIRMED và khóa thi lại/sửa kết quả đầu vào. Bước này chưa duyệt nhập học, chưa tạo hóa đơn và chưa xếp lớp. |
| 6 | Xem, in hóa đơn và hướng dẫn thanh toán | Chỉ xem/in Invoice của mình. Thanh toán đủ một lần bên ngoài hệ thống; học viên không tự xác nhận PAID. | Hóa đơn học phí | In qua trình duyệt từ dữ liệu mới hợp lệ; không chức năng xuất PDF riêng hoặc phát hành biên lai. Invoice UNPAID không là xác nhận đã thu tiền. Quá hạn chỉ cảnh báo. |
| 7 | Xem lớp và thời khóa biểu cá nhân | Chỉ xem lớp đã được xếp và lịch thuộc lớp của mình. | Thời khóa biểu | Hiển thị lịch hiện hành khi dữ liệu được cập nhật; học viên không tự xếp, chuyển lớp hoặc sửa lịch. |
| 8 | Xem kết quả học tập | Chỉ xem bảng điểm FINALIZED của mình, sau khi lớp COMPLETED và Giáo vụ chốt. | Bảng điểm cá nhân | Không xem điểm DRAFT/SUBMITTED/RETURNED. Điểm thang 0–100; tổng đã chốt từ 50 trở lên là đạt. Không chức năng tải phiếu điểm riêng. |

## 2. Nhân viên tuyển sinh

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 9 | Tra cứu và kiểm tra hồ sơ | Tra cứu theo thông tin được phép, kiểm tra hồ sơ và theo dõi trạng thái tuyển sinh. | Hồ sơ đăng ký khóa học | Không tạo tài khoản, ghi danh, đặt ca hoặc sửa hồ sơ cá nhân thay học viên. Hỗ trợ tại quầy vẫn hướng dẫn học viên tự thực hiện. |
| 12 | Duyệt, từ chối hoặc hủy hồ sơ nhập học | Duyệt/từ chối chỉ khi Registration CONFIRMED có Band hợp lệ. Từ chối có lý do. Tuyển sinh được hủy CONFIRMED hoặc ADMITTED có Invoice UNPAID, kèm lý do; không hủy hồ sơ đã PAID. | Hồ sơ đăng ký khóa học | Duyệt chuyển ADMITTED và tạo đúng một Invoice UNPAID trong cùng giao dịch. Hủy đồng thời xử lý booking tương lai/Invoice UNPAID, giữ lịch sử. Duyệt không đồng nghĩa đã thanh toán hoặc đã xếp lớp. |

## 3. Giáo vụ

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 13 | Quản lý khóa học, chương trình đào tạo và cấu hình Band | Quản lý Course ACTIVE/INACTIVE; TrainingProgram DRAFT/ACTIVE/INACTIVE theo cặp Course/Band duy nhất. Chương trình ACTIVE phải có số buổi LESSON nguyên dương, tổng trọng số 100 và đúng một thành phần FINAL_EXAM. | — | Không qua phê duyệt Ban giám đốc. Program khóa Course/Band, số buổi và cấu hình điểm từ lần đầu tạo Class, kể cả Class về sau bị hủy. Toàn bộ ngưỡng Band khóa từ kết quả đầu vào COMPLETED đầu tiên. Giá mới chỉ áp dụng Invoice phát hành sau; INACTIVE không tự hủy hồ sơ/lớp hiện có. |
| 14 | Tổ chức và quản lý ca thi đầu vào | Tạo ca CLOSED, phòng, giáo viên chấm ACTIVE và sức chứa 1–20; không trùng phòng/giáo viên với ca thi hoặc lịch lớp. Mở/đóng/hủy ca chỉ trước giờ bắt đầu theo điều kiện hợp lệ. | Lịch và kết quả thi đầu vào | Đến giờ bắt đầu, ca được coi CLOSED. Hủy ca giữ lịch sử, giải phóng booking và để học viên tự đặt lại. Đổi grader cần lý do, trước start và trước mọi điểm/vắng; sửa giờ/phòng trước start và khi chưa có booking. |
| 15 | Theo dõi kết quả thi đầu vào và Band | Theo dõi ca thi, tiến độ chấm, điểm từng lượt và Band theo phạm vi Giáo vụ. | Lịch và kết quả thi đầu vào | Không nhập hoặc ghi đè điểm/Band thay giáo viên/hệ thống. CLOSED không ngăn việc chấm hợp lệ. |
| 16 | Tạo và quản lý lớp học | Lớp thuộc một TrainingProgram, một giáo viên và max_size từ 1–15. Vòng đời: PLANNED → OPEN → ONGOING → COMPLETED; hủy chỉ PLANNED/OPEN còn trống và chưa dạy. | Danh sách lớp | Mở lớp cần program ACTIVE, giáo viên hợp lệ và ít nhất một LESSON tương lai. Bắt đầu không trước buổi đầu, có học viên và đủ lịch LESSON. Hoàn tất khi mọi nghĩa vụ LESSON/FINAL_EXAM đã HELD và đủ điểm danh LESSON; không tự chuyển trạng thái theo thời gian. |
| 17 | Phân công hoặc đổi giáo viên phụ trách | Mỗi lớp một giáo viên hợp lệ; không xung đột lịch. Chỉ đổi trước ONGOING/thực tế giảng dạy và chưa có Attendance/Score. | Danh sách lớp / Thời khóa biểu | Khóa/ngừng tài khoản không tự đổi giáo viên hoặc phân công lại. Không thay giáo viên sau khi đã bắt đầu dạy. |
| 18 | Xếp học viên vào lớp | Registration ADMITTED, Invoice PAID, đúng Course/Band; Class OPEN, còn chỗ và không trùng lịch thi/lớp của học viên. | Danh sách lớp | Một assignment mỗi hồ sơ; tạo GradeReport DRAFT cùng giao dịch, chưa tạo Score/Attendance. Registration vẫn ADMITTED. Không xếp ONGOING, chuyển/gỡ lớp hoặc tự mở lớp nếu chưa có lớp phù hợp. |
| 19 | Quản lý lịch học, lịch thi và buổi thay thế | Lập lịch LESSON theo tuần hoặc từng buổi; FINAL_EXAM riêng sau buổi LESSON cuối. Không trùng phòng, lớp, giáo viên hoặc lịch học viên. Lịch tuần lưu toàn bộ hoặc không buổi nào. | Thời khóa biểu | Không thêm LESSON vượt số nghĩa vụ chương trình. Dời trước start khi chưa điểm danh/điểm liên quan. Buổi không tổ chức ghi NOT_HELD, lý do và replacement tương lai cùng nghĩa vụ khi đủ điều kiện. Giáo vụ xác nhận HELD sau giờ kết thúc khi lớp ONGOING; không tự HELD từ đồng hồ/điểm danh. |
| 20 | Kiểm tra, trả lại và chốt bảng điểm | Chỉ xử lý Class COMPLETED với toàn bộ bảng điểm SUBMITTED. Trả lại bắt buộc lý do; trả/chốt cả lớp trong một thao tác thành công toàn bộ hoặc không ai. | Bảng điểm cá nhân | Trả lại thành RETURNED để giáo viên sửa/gửi lại. Chốt thành FINALIZED, lưu tổng điểm và công bố cho học viên; không sửa điểm thay giáo viên, không mở lại bảng đã chốt hoặc thêm tầng phê duyệt. |

## 4. Giáo viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 21 | Xem ca thi được phân công | Chỉ xem ca thi và danh sách thí sinh thuộc ca mình đang được phân công chấm. | Lịch và kết quả thi đầu vào | Không truy cập ca thi ngoài phạm vi phân công. |
| 22 | Nhập, sửa điểm thi đầu vào hoặc ghi vắng | Giáo viên được phân công nhập điểm 0–100 hoặc ABSENT từ khi ca kết thúc. Sửa kết quả COMPLETED chỉ bởi người chấm ban đầu còn được phân công, khi hồ sơ TESTING/BAND_ASSIGNED và có lý do. | Lịch và kết quả thi đầu vào | Hồ sơ CONFIRMED/REJECTED/CANCELLED không được chấm/sửa; ca CANCELLED không được chấm. Sửa hợp lệ tính lại best score/Band và ghi audit cùng giao dịch. Không tự ghi vắng từ điểm trống. |
| 23 | Xem lịch dạy và danh sách lớp | Chỉ lớp mình được phân công và danh sách học viên/lịch thuộc lớp đó. | Danh sách lớp / Thời khóa biểu | Không xem hoặc xử lý lớp ngoài phạm vi phụ trách. |
| 24 | Ghi và cập nhật điểm danh | Ghi Có mặt/Vắng từng học viên cho buổi LESSON từ giờ bắt đầu đến trước Class COMPLETED; đúng lớp phụ trách, buổi không NOT_HELD. | Bảng điểm danh | Một record mỗi học viên/LESSON; chưa ghi khác vắng. Lưu batch một buổi thành công toàn bộ hoặc rollback; được ghi một phần danh sách, nhưng thiếu điểm danh chặn hoàn tất lớp. Điểm danh không tự xác nhận HELD. |
| 25 | Nhập điểm, lưu nháp và gửi bảng điểm | Điểm thành phần 0–100, tối đa hai chữ số thập phân, theo cấu hình program. Lưu nháp từng học viên khi lớp ONGOING/COMPLETED và bảng DRAFT/RETURNED; điểm gắn lịch chỉ nhập từ start của buổi hợp lệ. Chỉ gửi cả lớp khi COMPLETED, đủ điểm và không còn sửa chưa lưu. | Bảng điểm cá nhân | Điểm/nhận xét một học viên lưu cùng nhau; thiếu điểm không là 0, ô trống không xóa điểm. Gửi toàn lớp thành SUBMITTED hoặc không ai; học viên chưa thấy điểm. final_score chưa lưu trước FINALIZED. |
| 26 | Sửa và gửi lại bảng điểm bị trả | Giáo viên hiện phụ trách sửa nháp khi RETURNED; gửi lại cả lớp sau khi đủ điểm, Class COMPLETED và không còn sửa chưa lưu. | Bảng điểm cá nhân | Giữ điểm, nhận xét, lý do trả và lịch sử; chu kỳ trả/sửa/gửi lại có thể lặp. Không sửa SUBMITTED/FINALIZED hoặc gửi riêng một học viên. |

## 5. Kế toán

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 27 | Tra cứu hóa đơn và theo dõi công nợ | Tra cứu Invoice UNPAID/PAID/CANCELLED theo phạm vi tài chính; quá hạn khi UNPAID và due_date trước ngày hiện tại tại Việt Nam. | Hóa đơn học phí | Hạn thanh toán là ngày phát hành tại Việt Nam +7 ngày lịch. Quá hạn chỉ cảnh báo; không trạng thái OVERDUE riêng, phạt tiền hoặc tự đổi trạng thái học viên. |
| 28 | Đối chiếu, xác nhận thanh toán và sửa thông tin phương thức | Đối chiếu tiền nhận bên ngoài; tạo một Payment đúng toàn bộ tiền Invoice UNPAID và chuyển PAID cùng giao dịch. Phương thức CASH/TRANSFER; TRANSFER bắt buộc mã tham chiếu. | Hóa đơn học phí / Thông tin xác nhận thanh toán | Không trả góp, nhiều lần thu, gateway hoặc biên lai riêng. Sau xác nhận chỉ sửa method/reference với lý do/audit; tiền, liên kết hóa đơn và thời điểm xác nhận bất biến. Không hủy/xóa Payment hoặc refund. |
| 29 | Xem và xuất báo cáo tài chính | Doanh thu theo Payment được xác nhận trong kỳ; tổng phải thu theo Invoice không CANCELLED phát hành trong kỳ; công nợ hiện tại là tổng Invoice UNPAID, quá hạn là tập con. | Báo cáo tổng hợp học phí | Không lấy phải thu trong kỳ trừ doanh thu trong kỳ làm công nợ. Chỉ Kế toán xuất CSV theo toàn bộ kết quả/cột/bộ lọc được phép; tối đa 10.000 dòng hoặc 5 MB, vượt thì từ chối toàn file. Không XLSX, chi phí/lợi nhuận hoặc số dư lịch sử tại ngày quá khứ. |

## 6. Ban giám đốc

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 30 | Xem báo cáo tuyển sinh | Chỉ xem tổng hợp theo bộ lọc đã chốt; đếm học viên phân biệt trong từng khóa và toàn báo cáo, không cộng nhóm để suy tổng. | Báo cáo tuyển sinh | Không định danh học viên, xem hồ sơ cá nhân, duyệt hồ sơ hoặc xuất file. Trạng thái là hiện tại, không tái dựng trạng thái quá khứ. |
| 31 | Xem báo cáo đào tạo | Xem tổng hợp lớp, tiến độ nghĩa vụ LESSON đã HELD so với số buổi chương trình; FINAL_EXAM theo dõi riêng. Tỷ lệ đạt theo kết quả FINALIZED trong kỳ ngày chốt. | Báo cáo đào tạo | Không lấy thời gian trôi qua/điểm danh thay HELD. Chưa chốt không tính đạt/rớt; mẫu số rỗng hiển thị Chưa có dữ liệu. Không dữ liệu cá nhân, chỉnh sửa hoặc xuất file. |
| 32 | Xem báo cáo tài chính | Chỉ xem tổng hợp doanh thu, phải thu trong kỳ, công nợ/quá hạn hiện tại theo định nghĩa chung với Kế toán. | Báo cáo tổng hợp học phí | Không xem Invoice/Payment cá nhân, xác nhận thanh toán hoặc xuất file; không chức năng phê duyệt tài chính. |

## 7. Quản trị viên

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 33 | Cấp tài khoản nhân sự và đặt lại mật khẩu | Cấp một vai trò nhân sự có sẵn; email đăng nhập bất biến. Cấp/reset tạo mật khẩu tạm ngẫu nhiên 32 ký tự, hạn 24 giờ, chỉ hiển thị một lần; bắt đổi trước truy cập nghiệp vụ. | — | Admin có thể reset mật khẩu tài khoản theo quyền; không tự mở khóa/kích hoạt lại. Reset thu hồi mọi phiên. Không gửi email recovery, lưu/xuất mật khẩu hoặc xem lại secret; phiên bắt đổi chỉ cho đổi mật khẩu/đăng xuất, tối đa 10 phút và không vượt hạn secret. |
| 34 | Khóa, mở, ngừng và khôi phục tài khoản | Quản lý ACTIVE/LOCKED/DISABLED; không khóa/ngừng chính mình hoặc Admin ACTIVE cuối cùng. Không đổi dữ liệu nghiệp vụ hay phân công khi đổi trạng thái tài khoản. | — | Khóa/ngừng thu hồi mọi phiên; mở/khôi phục cùng tài khoản giữ role/history nhưng cần đăng nhập mới. Throttle sai mật khẩu không là LOCKED. Không xóa vật lý tài khoản có lịch sử nghiệp vụ. |
| 35 | Tra cứu nhật ký hoạt động | Chỉ Admin xem/tìm/lọc nhật ký, chi tiết sự kiện và nhóm thao tác liên quan theo phạm vi được phép. | Nhật ký hệ thống | Không sửa/xóa/undo/export log, không từ log mở quyền xem nghiệp vụ khác. Không hiển thị mật khẩu, hash, token hoặc toàn bộ payload nhạy cảm. |

## 8. Chức năng tự động của hệ thống

Nhóm này mô tả xử lý tự động, không phải vai trò tài khoản thứ tám.

| STT | Tên chức năng | Quy định / Điều kiện | Biểu mẫu | Ghi chú |
| --- | --- | --- | --- | --- |
| 36 | Tự động xác định Band | Chọn điểm cao nhất từ tối đa hai lượt COMPLETED của cùng hồ sơ; lấy Band có min_score lớn nhất nhưng không vượt quá điểm. | Lịch và kết quả thi đầu vào | Ngưỡng đã chốt: 0, 25, 45, 65, 85; bằng điểm chọn lượt một. Tính lại khi correction hợp lệ, không tự chấm bài thi hoặc sửa Band sau CONFIRMED. Toàn bộ ngưỡng khóa từ kết quả COMPLETED đầu tiên. |
| 37 | Tự động phát hành hóa đơn | Khi Tuyển sinh duyệt hồ sơ thành ADMITTED, tạo đúng một Invoice UNPAID trong cùng giao dịch, lấy học phí Course tại thời điểm phát hành. | Hóa đơn học phí | Tiền snapshot dương, VND nguyên đồng; không đổi khi Course đổi giá. Hạn ngày phát hành VN +7 ngày lịch; duyệt lặp không tạo hóa đơn thứ hai. |
| 38 | Tự động tính điểm tổng kết và kết quả đạt | Tổng = Σ(điểm thành phần × trọng số /100) theo TrainingProgram đã khóa cấu hình. Chỉ khi FINALIZED mới lưu final_score; làm tròn half-up một lần đến hai chữ số thập phân. | Bảng điểm cá nhân | Trước chốt chỉ tính tổng nháp từ Score khi đọc, final_score NULL. Tổng đã chốt ≥50 là đạt; không thêm điều kiện tỷ lệ chuyên cần/điểm tối thiểu từng thành phần. Thiếu điểm không được coi là 0. |
| 39 | Ghi nhật ký nghiệp vụ và sự kiện bảo mật | Mutation nghiệp vụ/tài khoản thuộc danh mục bắt buộc ghi actor, hành động, đối tượng, thời điểm, old/new/lý do theo loại sự kiện và mã thao tác. Audit bắt buộc cùng giao dịch; lỗi thì rollback mutation. | Nhật ký hệ thống | Security-only như login/logout/throttle/denied-access dùng best-effort. Giữ lịch sử nghiệp vụ/tài khoản; security log/IP giữ 90 ngày theo chính sách vận hành. Không log mọi lần đọc, bí mật hoặc toàn bộ request/response. |

## Nguồn đối chiếu

- [Requirements hiện hành](../requirements.md).
- [Đặc tả chức năng E01](FUNCTION_SPEC.md) và [workflow E02](WORKFLOW_SPEC.md).
- [Các quyết định đã duyệt](../DECISIONS.md).
- [Bảng đối chiếu báo cáo nguồn](../reference/BUSINESS_FUNCTION_COMPARISON.md).

Bảng này trình bày lại nghiệp vụ đã chốt phục vụ báo cáo; không bổ sung quyết
định nghiệp vụ mới hoặc thay thế nghiệm thu toàn bộ thiết kế E04/E05.
