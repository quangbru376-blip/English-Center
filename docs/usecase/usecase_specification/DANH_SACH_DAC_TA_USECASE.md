# Danh sách đề xuất đặc tả use case

> ĐÃ ĐƯỢC THAY THẾ ngày 15/10/2026: chủ dự án chọn bám 37 chức năng nghiệp vụ. Dùng `DANH_SACH_37_DAC_TA_USECASE.md` trong cùng thư mục. Danh sách 83 mục dưới đây chỉ giữ làm checklist thao tác lịch sử, không là mục lục đặc tả chính thức.

Ngày lập: 15/10/2026. Trạng thái: Đề xuất để chủ dự án xem xét. UC-001 đã có bản viết thử trong trao đổi; chưa chốt toàn bộ mã hoặc đổi sơ đồ.

Mỗi mục dưới đây là một bảng đặc tả dự kiến. Các thao tác cùng mục tiêu nhưng có hướng chuyển trạng thái khác nhau có thể được mô tả bằng nhánh, guard và hậu điều kiện riêng trong cùng bảng. Những mục tiêu khác vai trò hoặc ảnh hưởng nghiệp vụ quan trọng được tách bảng. Khi soạn, đối chiếu đủ từng lệnh trong các mục ghép; không chỉ viết một luồng quản lý chung.

## Tài khoản và hồ sơ cá nhân

Tác nhân theo từng chức năng: Mọi vai trò; Học viên; Quản trị viên.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-001 | Đăng nhập |
| UC-002 | Đăng xuất phiên hiện tại |
| UC-003 | Tự đăng ký tài khoản học viên |
| UC-004 | Tự đổi mật khẩu |
| UC-005 | Cập nhật hồ sơ cá nhân của mình |
| UC-006 | Cấp tài khoản nhân sự |
| UC-007 | Đặt lại mật khẩu tạm |
| UC-008 | Khóa hoặc mở khóa tài khoản |
| UC-009 | Ngừng sử dụng tài khoản |
| UC-010 | Khôi phục tài khoản ngừng sử dụng |

## Hồ sơ đăng ký và tuyển sinh

Tác nhân theo từng chức năng: Học viên; Tuyển sinh.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-011 | Xem danh sách và chi tiết khóa học đang hoạt động |
| UC-012 | Tạo hồ sơ đăng ký khóa học |
| UC-013 | Xem hồ sơ đăng ký của mình |
| UC-014 | Hủy hồ sơ của mình trước xác nhận nhập học |
| UC-015 | Xác nhận nhập học |
| UC-016 | Tra cứu và kiểm tra hồ sơ tuyển sinh |
| UC-017 | Duyệt hồ sơ nhập học |
| UC-018 | Từ chối hồ sơ nhập học |
| UC-019 | Hủy hồ sơ theo quyền Tuyển sinh |

## Học viên đăng ký và theo dõi thi đầu vào

Tác nhân theo từng chức năng: Học viên.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-020 | Tra cứu ca thi đầu vào phù hợp |
| UC-021 | Xem lịch thi đầu vào của mình |
| UC-022 | Đăng ký ca thi đầu vào lần đầu |
| UC-023 | Đăng ký lượt thi đầu vào thứ hai |
| UC-024 | Đổi ca thi đã đăng ký |
| UC-025 | Hủy đặt ca thi |
| UC-026 | Đặt lại ca cho lượt thi chưa hoàn thành |
| UC-027 | Xem kết quả các lượt thi và Band của mình |

## Tổ chức và chấm thi đầu vào

Tác nhân theo từng chức năng: Giáo vụ; Giáo viên.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-028 | Tra cứu ca thi và danh sách đăng ký |
| UC-029 | Tạo ca thi đầu vào |
| UC-030 | Điều chỉnh lịch ca thi đầu vào |
| UC-031 | Mở hoặc đóng ca thi đầu vào |
| UC-032 | Hủy ca thi đầu vào |
| UC-033 | Phân công hoặc đổi giáo viên chấm thi |
| UC-034 | Theo dõi tiến độ chấm, kết quả và Band |
| UC-035 | Xem ca thi và danh sách được phân công chấm |
| UC-036 | Nhập điểm thi đầu vào |
| UC-037 | Ghi nhận vắng thi đầu vào |
| UC-038 | Sửa điểm thi đầu vào có lý do |

## Hóa đơn và thanh toán

Tác nhân theo từng chức năng: Học viên; Kế toán.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-039 | Xem hóa đơn và hướng dẫn thanh toán của mình |
| UC-040 | In hóa đơn của mình |
| UC-041 | Tra cứu hóa đơn và thông tin thanh toán |
| UC-042 | Xác nhận thanh toán học phí |
| UC-043 | Sửa phương thức hoặc mã giao dịch thanh toán |

## Khóa học và chương trình đào tạo

Tác nhân theo từng chức năng: Giáo vụ.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-044 | Tra cứu khóa học, chương trình và danh mục Band |
| UC-045 | Tạo khóa học |
| UC-046 | Cập nhật tên và học phí khóa học |
| UC-047 | Ngừng hoặc khôi phục khóa học |
| UC-048 | Tạo chương trình đào tạo theo khóa và Band |
| UC-049 | Cấu hình số buổi, thành phần điểm và trọng số |
| UC-050 | Đổi tên chương trình đào tạo |
| UC-051 | Chuyển trạng thái chương trình đào tạo |

## Lớp học và xếp lớp

Tác nhân theo từng chức năng: Giáo vụ.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-052 | Tra cứu lớp học và danh sách học viên |
| UC-053 | Tạo lớp học |
| UC-054 | Điều chỉnh sức chứa lớp học |
| UC-055 | Mở lớp học |
| UC-056 | Bắt đầu giảng dạy lớp học |
| UC-057 | Hoàn tất lớp học |
| UC-058 | Hủy lớp học |
| UC-059 | Phân công hoặc đổi giáo viên phụ trách lớp |
| UC-060 | Xếp học viên đủ điều kiện vào lớp |

## Lịch học và lịch thi cuối khóa

Tác nhân theo từng chức năng: Giáo vụ.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-061 | Tra cứu lịch hiện hành và lịch sử buổi học, buổi thi |
| UC-062 | Lập lịch học theo tuần |
| UC-063 | Thêm lịch từng buổi học hoặc thi cuối khóa |
| UC-064 | Dời lịch buổi chưa bắt đầu |
| UC-065 | Ghi nhận buổi không tổ chức và tạo buổi thay thế |
| UC-066 | Xác nhận buổi học hoặc buổi thi đã tổ chức |

## Giảng dạy, điểm danh và kết quả học tập

Tác nhân theo từng chức năng: Giáo viên; Giáo vụ; Học viên.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-067 | Xem lớp, danh sách học viên và lịch giảng dạy được phân công |
| UC-068 | Ghi hoặc cập nhật điểm danh một buổi học |
| UC-069 | Lưu điểm nháp và nhận xét của một học viên |
| UC-070 | Gửi bảng điểm cả lớp |
| UC-071 | Sửa và gửi lại bảng điểm cả lớp bị trả lại |
| UC-072 | Xem và kiểm tra bảng điểm theo quyền Giáo vụ |
| UC-073 | Trả lại bảng điểm cả lớp có lý do |
| UC-074 | Chốt và công bố bảng điểm cả lớp |
| UC-075 | Xem lớp và lịch học của mình |
| UC-076 | Xem kết quả học tập đã chốt của mình |

## Báo cáo

Tác nhân theo từng chức năng: Giám đốc; Kế toán.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-077 | Xem báo cáo tuyển sinh tổng hợp |
| UC-078 | Xem báo cáo đào tạo và kết quả học tập tổng hợp |
| UC-079 | Xem báo cáo tài chính theo quyền Giám đốc |
| UC-080 | Xem báo cáo tài chính theo quyền Kế toán |
| UC-081 | Xuất CSV báo cáo tài chính |

## Nhật ký hoạt động

Tác nhân theo từng chức năng: Quản trị viên.

| Mã đề xuất | Tên use case |
| --- | --- |
| UC-082 | Tra cứu nhật ký hoạt động |
| UC-083 | Xem chi tiết sự kiện và các sự kiện cùng thao tác |

## Giới hạn và truy vết

- Tổng cộng 83 bảng dự kiến. Đây là cách phân rã phục vụ báo cáo, không phải bổ sung 83 chức năng sản phẩm hoặc khẳng định tất cả đã được audit lại.
- Mã UC00–UC09 của bộ sơ đồ hiện có là mã hình/nhóm; các mã UC-001… trong danh sách này là mã đặc tả chi tiết đề xuất. Cần ánh xạ trước khi sửa hoặc xuất bản sơ đồ.
- Các phép đọc danh sách/chi tiết có cùng mục tiêu được trình bày cùng bảng. Các trạng thái/lệnh trong mục ghép phải được mô tả đủ nhánh, quyền và hậu điều kiện.
- UC-008 gồm nhánh khóa và mở khóa, phải giữ tác động thu hồi phiên chỉ cho nhánh khóa. UC-031 gồm mở/mở lại và đóng ca với guard riêng. Các nhóm chuyển trạng thái chương trình phải có nhánh kích hoạt, về nháp, ngừng và khôi phục đúng hợp đồng.
- Chức năng tra cứu tài khoản để chọn đối tượng được mô tả trong các use case quản trị tài khoản, không phải quyền xem toàn bộ dữ liệu nghiệp vụ.
- Danh mục Band chỉ đọc theo hợp đồng đã triển khai. Không thêm use case sửa ngưỡng Band từ nhãn cũ trong model sơ đồ; phải đối chiếu E04 và audit mới nhất.
- Phát hành hóa đơn nằm trong duyệt hồ sơ; tạo bảng điểm nháp nằm trong xếp lớp; tính lại điểm tốt nhất/Band nằm trong nhập/sửa điểm đầu vào; tính tổng chính thức nằm trong chốt điểm. Ghi nhật ký và thu hồi phiên được mô tả tại nghiệp vụ liên quan. Không tạo nút/menu độc lập cho các xử lý này. Nếu cần đặc tả include riêng theo sơ đồ, lập phụ mục xử lý dùng chung và truy vết riêng, không tự thêm tác nhân Hệ thống.
- Xem kết quả đầu vào phải phân biệt yêu cầu nghiệp vụ với điểm mở NV04 về nhãn điểm tốt nhất/lượt được chọn. Không tuyên bố đã sửa UI từ danh sách này.
- Không thêm đăng ký thay học viên, sửa điểm thay giáo viên, sửa Band thủ công, chuyển lớp, hoàn tiền, thanh toán trực tuyến, học bổng, xuất báo cáo Giám đốc hoặc xuất nhật ký.

Nguồn: docs/REPORT_AGENT_HANDOFF.md; docs/requirements.md; docs/design/FUNCTION_SPEC.md; docs/design/WORKFLOW_SPEC.md; docs/reference/usecase-update-20261014/model.json. Đây là chỉ mục biên soạn; khi đặc tả từng mục cần truy vết NV/W/F/ADR và đối chiếu mã liên quan.
