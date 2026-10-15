# Danh sách 37 đặc tả theo chức năng nghiệp vụ

Ngày cập nhật: 15/10/2026. Chủ dự án chọn bám 37 chức năng nghiệp vụ làm khung báo cáo. Danh sách này thay thế đề xuất 83 mục; không thay đổi phạm vi sản phẩm hoặc khẳng định 37 nhóm đều là mục tiêu tác nhân độc lập theo UML.

Tiến độ biên soạn 15/10/2026: đã có đủ 37/37 bảng chính bản thảo trong bốn tệp `DAC_TA_USECASE_001_010.md`, `DAC_TA_USECASE_011_020.md`, `DAC_TA_USECASE_021_030.md` và `DAC_TA_USECASE_031_037.md`. Đã bổ sung `THAO_TAC_CHUNG.md` cho đăng xuất/hết phiên, tự đổi mật khẩu và sửa hồ sơ cá nhân, không thêm NV hoặc tăng số bảng chính. [Chỉ mục bộ tài liệu](README.md) dẫn tới từng đợt. Đã rà đủ chín mục/bảng, mapping 37 NV còn hiệu lực đúng một lần và liên kết chỉ mục; nội dung có đối chiếu nguồn liên quan. Chưa được chủ dự án duyệt nội dung từng bảng hoặc dàn trang/chèn Word; không coi ghi chú điểm mở UI NV04 là đã sửa sản phẩm.

Đề xuất mã đặc tả UC-001–UC-037 liên tục và giữ cột NV gốc để truy vết; không đánh lại số NV sau khi loại NV10/NV11. Tên và phạm vi bảng bám E01; câu chữ tiếng Việt được chuẩn hóa cho báo cáo. Mã UC là mã đặc tả đề xuất, chưa đổi mã sơ đồ UC00–UC09.

| Mã đặc tả đề xuất | Mã nghiệp vụ | Tên bảng đặc tả |
| --- | --- | --- |
| UC-001 | NV01 | Tự đăng ký tài khoản học viên và đăng nhập |
| UC-002 | NV02 | Xem khóa học, tạo và hủy hồ sơ đăng ký của mình |
| UC-003 | NV03 | Đặt, xem, hủy, đổi ca thi đầu vào và đăng ký thi lại |
| UC-004 | NV04 | Xem kết quả thi đầu vào và Band |
| UC-005 | NV05 | Xác nhận nhập học |
| UC-006 | NV06 | Xem và in hóa đơn của mình |
| UC-007 | NV07 | Xem lớp và lịch học của mình |
| UC-008 | NV08 | Xem bảng điểm đào tạo đã chốt |
| UC-009 | NV09 | Tra cứu và kiểm tra hồ sơ tuyển sinh |
| UC-010 | NV12 | Duyệt, từ chối và hủy hồ sơ theo quyền Tuyển sinh |
| UC-011 | NV13 | Quản lý khóa học, chương trình đào tạo và cấu hình điểm |
| UC-012 | NV14 | Tạo, mở, đóng, hủy và điều chỉnh ca thi đầu vào |
| UC-013 | NV15 | Theo dõi kết quả thi và Band phục vụ quản lý |
| UC-014 | NV16 | Tạo, quản lý và chuyển trạng thái lớp học |
| UC-015 | NV17 | Phân công hoặc đổi giáo viên phụ trách lớp |
| UC-016 | NV18 | Xếp học viên vào lớp |
| UC-017 | NV19 | Lập, dời, thay lịch và xác nhận buổi thực tế |
| UC-018 | NV20 | Trả lại hoặc chốt bảng điểm cả lớp |
| UC-019 | NV21 | Xem ca thi đầu vào được phân công |
| UC-020 | NV22 | Nhập, sửa điểm đầu vào và ghi nhận vắng thi |
| UC-021 | NV23 | Xem lớp, lịch dạy và học viên được phân công |
| UC-022 | NV24 | Lưu hoặc cập nhật điểm danh một buổi học |
| UC-023 | NV25 | Lưu điểm nháp, nhận xét và gửi bảng điểm cả lớp |
| UC-024 | NV26 | Sửa và gửi lại bảng điểm cả lớp bị trả lại |
| UC-025 | NV27 | Tra cứu hóa đơn nghiệp vụ |
| UC-026 | NV28 | Xác nhận thanh toán và sửa thông tin thanh toán được phép |
| UC-027 | NV29 | Xem và xuất báo cáo tài chính |
| UC-028 | NV30 | Xem báo cáo tuyển sinh tổng hợp |
| UC-029 | NV31 | Xem báo cáo đào tạo và kết quả đã chốt |
| UC-030 | NV32 | Xem báo cáo tài chính quản trị |
| UC-031 | NV33 | Cấp tài khoản nhân sự và đặt lại mật khẩu |
| UC-032 | NV34 | Khóa, mở khóa, ngừng và khôi phục tài khoản |
| UC-033 | NV35 | Tra cứu nhật ký và chi tiết nhóm thao tác |
| UC-034 | NV36 | Tính và cập nhật Band từ kết quả thi đầu vào |
| UC-035 | NV37 | Phát hành hóa đơn khi duyệt hồ sơ nhập học |
| UC-036 | NV38 | Tính tổng điểm từ các thành phần |
| UC-037 | NV39 | Ghi nhật ký nghiệp vụ, tài khoản và sự kiện bảo mật |

## Cách biên soạn

- Tổng 37 bảng gồm 33 nhóm chức năng gắn với tác nhân và 4 xử lý tự động NV36–NV39. Bốn xử lý tự động có mục đích hỗ trợ/tích hợp, chỉ được kích hoạt bởi nghiệp vụ liên quan; không thêm vai trò Hệ thống, menu hoặc lệnh người dùng tự chạy.
- Một bảng có thể bao gồm nhiều thao tác của cùng nhóm NV, nhưng mỗi thao tác phải rõ điều kiện, luồng hoặc nhánh và hậu điều kiện. Không chỉ viết chung mở trang/chọn nghiệp vụ/lưu.
- Riêng NV01 bao gồm tự đăng ký Student và đăng nhập bảy vai trò; quyền của mỗi luồng phải phân biệt. Bản viết thử Đăng nhập trước đây là phần đăng nhập, chưa đại diện đủ bảng NV01. Không tự đăng nhập sau đăng ký.
- Đăng xuất, tự đổi mật khẩu và sửa hồ sơ cá nhân là các chức năng dùng chung bổ sung tại E01 mục 8, không có mã NV mới. Chúng vẫn phải được mô tả ở phụ mục thao tác chung và dẫn chiếu từ bảng liên quan; 37 là số bảng chính theo NV, không là lý do bỏ các chức năng này.
- Giữ chín mục theo quy ước đã đề xuất. Khi nhiều thao tác khác nhau không thể trình bày rõ trong một luồng, chia các tiểu luồng có tên trong cùng bảng; luồng phụ không tự được coi là include/extend UML.
- Danh mục Band hiện chỉ đọc; NV13 không cho phép sửa ngưỡng thủ công. NV04 còn mục mở về hiển thị tường minh điểm tốt nhất/lượt được chọn, không được mô tả như sửa UI đã hoàn tất.
- Chức năng tự động phải nêu nghiệp vụ kích hoạt, dữ liệu vào, kết quả, điều kiện và giao dịch liên quan; tác nhân ghi vai trò khởi tạo nghiệp vụ liên quan và làm rõ tham gia gián tiếp. Không mô tả Hệ thống như tác nhân bên ngoài.
- Khi soạn từng bảng, đối chiếu requirements/ADR/E01/E02/E04 và mã triển khai liên quan. Nguồn sơ đồ cũ không mở thêm quyền hoặc khôi phục NV10/NV11/F15.

Nguồn: docs/design/FUNCTION_SPEC.md; docs/design/FUNCTION_CATALOG.md; docs/REPORT_AGENT_HANDOFF.md. Lựa chọn 37 bảng chính là chỉ đạo biên soạn báo cáo của chủ dự án ngày 15/10/2026, không phải thay đổi nghiệp vụ.
