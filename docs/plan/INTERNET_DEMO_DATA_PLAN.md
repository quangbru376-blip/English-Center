Cập nhật15/10/2026: phần bộ dữ liệu và localhost demo đã triển khai/review PASS; đang chạy tại https://localhost:8444. Xem docs/DEMO_LOCALHOST.md và LOCAL_DEMO_REVIEW.md. Các dòng chưa tạo dữ liệu bên dưới là hồ sơ phê duyệt kế hoạch trước triển khai. Chưa chọn nhà cung cấp hoặc mở Internet; M4 recovery vẫn riêng.

# Dữ liệu mẫu cho trải nghiệm và demo Internet

Ngày lập và duyệt: 15/10/2026. Trạng thái: chủ dự án đã đồng ý quy mô và kịch bản; chưa tạo dữ liệu hoặc tài khoản.

Chủ dự án yêu cầu một bộ nhiều tài khoản, khóa học, lớp học và dữ liệu liên quan để tự trải nghiệm sản phẩm. Bộ này là một đầu ra riêng trong kế hoạch demo Internet, cùng bản ứng dụng demo và cấu hình truy cập công khai. Không đồng nghĩa đã chọn hosting/tunnel hoặc cho phép đưa dữ liệu vào catalog vận hành hiện tại.

## Mục tiêu

Đăng nhập được bằng đủ bảy vai trò, tìm thấy dữ liệu có ý nghĩa ngay trên web và thực hiện được các bước nghiệp vụ tiếp theo. Dữ liệu cần liên kết nhất quán; không chỉ tạo nhiều dòng trống để lấp danh sách. Chỉ dùng danh tính và thông tin liên hệ giả, không sao chép dữ liệu học viên thật.

## Quy mô khởi đầu đã duyệt

Chủ dự án trả lời “đồng ý” với đề xuất ngày15/10/2026. Các số dưới đây là quy mô khởi đầu đã duyệt, không phải kết quả đo tải. Có thể điều chỉnh sau theo nhu cầu thực hành.

| Nhóm | Số lượng dự kiến | Mục đích |
|---|---|---|
| Học viên | 50 | Nhiều khóa, nhiều hồ sơ và các bước tuyển sinh/đào tạo khác nhau |
| Giáo viên | 8 | Có ca thi và lớp phụ trách khác nhau để kiểm tra phạm vi nhiệm vụ |
| Nhân sự khác | 9 | 2 Tuyển sinh, 2 Giáo vụ, 2 Kế toán, 1 Giám đốc, 2 Admin |
| Khóa học | 6 | Có khóa ACTIVE/INACTIVE, giá và mã riêng |
| Band | Bộ 5 tham chiếu hiện hành | Giữ ngưỡng 0/25/45/65/85, không thêm editor hoặc thay chính sách |
| Chương trình | 12 | Các cặp khóa/Band duy nhất, có cấu hình trọng số và số buổi hợp lệ |
| Lớp học | 12 | Phân bố PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED với điều kiện tương ứng |
| Ca thi đầu vào | 12 | Có ca tương lai và lịch sử; OPEN/CLOSED/CANCELLED phù hợp clock và booking |
| Hồ sơ đăng ký | Khoảng 70 | Một người có thể học nhiều khóa hoặc có lịch sử học lại hợp lệ |
| Hóa đơn/thanh toán/điểm danh/điểm | Sinh theo kịch bản | Không tạo tùy tiện số lượng tách rời điều kiện nghiệp vụ |

## Kịch bản bắt buộc

1. Student mới chưa có hồ sơ: tự chọn khóa và đặt ca.
2. Hồ sơ đang chờ thi/chờ chấm; ca có chỗ và ca đầy để thấy khác biệt.
3. Có điểm/Band, gồm điểm0 thật và vắng không có điểm; lượt hai, đặt lại và lịch sử đổi/hủy hợp lệ.
4. Student đủ điều kiện xác nhận; hồ sơ CONFIRMED chờ Tuyển sinh duyệt/từ chối.
5. ADMITTED có Invoice UNPAID: có hóa đơn còn hạn/quá hạn; chờ Kế toán xác nhận đủ tiền.
6. ADMITTED và PAID chờ xếp vào lớp OPEN đúng khóa/Band, còn chỗ và không trùng lịch.
7. Lớp ONGOING có lịch/điểm danh/điểm nháp để Teacher tiếp tục thực hành.
8. Lớp COMPLETED có bảng DRAFT/RETURNED cần gửi/gửi lại và lớp có toàn bộ SUBMITTED chờ Giáo vụ trả/chốt.
9. Kết quả FINALIZED có đạt/không đạt để Student xem và Director xem báo cáo.
10. Hồ sơ/lớp/ca đã hủy, khóa ngừng hoạt động, tài khoản LOCKED/DISABLED để trải nghiệm giới hạn đúng quyền.
11. Accountant có số liệu doanh thu/phải thu/công nợ khác nhau và CSV; Admin có lịch sử nghiệp vụ/bảo mật tương ứng, không lịch sử giả mạo actor.

## Điều kiện thực hiện

- Chọn database demo riêng và cấu hình demo riêng; không seed trực tiếp catalog vận hành LOCAL_DEV hoặc D07 được giữ làm baseline.
- Chọn ngày tham chiếu khi tạo bộ dữ liệu. Lịch tương lai phải còn đủ thời gian để trải nghiệm; lịch sử phải nhất quán với trạng thái/điểm danh/điểm. Không tắt clock guard hoặc thêm API nhập lùi ngày để tạo mẫu.
- Xác định phần dựng trạng thái lịch sử bằng fixture/operator riêng và phần tạo bằng workflow thật. Ghi rõ nguồn dữ liệu synthetic; không giả audit như đã có người thật thao tác.
- Mật khẩu và cách nhận tài khoản phải theo chính sách hiện hành. Nhân sự có mật khẩu tạm hiển thị một lần/hạn24h và bắt đổi; không lưu plaintext trong Git, báo cáo hoặc bộ seed chia sẻ. Cách bàn giao tài khoản demo cụ thể cần được chuẩn bị khi triển khai.
- Mỗi tài khoản giữ một vai trò. Không tạo role switcher thật để thay đăng nhập.
- Công cụ tạo lại dữ liệu chỉ thao tác catalog demo có nhận diện rõ; không reset database theo tên mơ hồ hoặc xóa dữ liệu người dùng đã thực hành mà không chủ động chọn.
- Nếu demo được mở cho người ngoài thao tác, phân biệt tài khoản chủ dự án và khách; lựa chọn phạm vi phát tài khoản/quyền và thời điểm dựng lại dữ liệu trước khi công khai.
- Không coi việc có dataset này là bằng chứng tải 10.000 học viên/50 người đồng thời hoặc hoàn tất backup/restore M4.

## Đầu ra cần bàn giao khi thực hiện

- Bộ dựng dữ liệu có phiên bản, hướng dẫn tạo lại và kiểm tra nhất quán.
- Danh mục tài khoản theo vai trò và hướng dẫn nhận thông tin đăng nhập qua kênh riêng.
- Bản đồ kịch bản: đăng nhập vai trò nào, mở menu nào, chọn đối tượng nào và bước hợp lệ tiếp theo.
- Bằng chứng thao tác web thật cho các kịch bản chính và ba kích thước giao diện.
- Thông tin ngày tạo/ngày tham chiếu và hướng dẫn cập nhật dữ liệu khi lịch demo đã cũ.

Chủ dự án đã duyệt hướng và quy mô bộ mẫu. Việc dựng dữ liệu cần chuẩn bị cấu hình catalog demo riêng, công cụ tạo lại và cách bàn giao tài khoản theo các điều kiện trên. Chưa chạy seed, đổi mật khẩu, tạo database hoặc bật Internet; phê duyệt này không chọn nhà cung cấp/domain/public profile hoặc đóng đầu vào recovery M4.
