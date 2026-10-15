# Bộ 27 giao diện quan trọng và bảng mô tả thành phần

Mở [bản xem có ảnh và bảng](index.html) hoặc [bản Markdown để biên soạn](MO_TA_27_GIAO_DIEN.md).

Có **27 giao diện chính**, **27 bảng riêng + 1 bảng dùng chung**, **303 dòng mô tả** theo đúng bốn cột STT – Loại – Ý nghĩa – Biến cố/Xử lý. Đủ khách và bảy vai trò. Đây là 27 mục giao diện, không cam kết tài liệu in chỉ dài 27 trang.

Thư mục `images` có **59 ảnh được chọn**, gồm 27 ảnh chính và các ảnh bổ sung/ảnh thành phần chung. Ảnh sao chép nguyên byte từ bộ 117 ảnh; mã ảnh nguồn được giữ để đối chiếu. Bấm ảnh trong HTML để mở PNG gốc. Các trạng thái bổ sung được giải thích trong cùng mục, không tăng thành giao diện hoặc use case mới.

| Mã | Giao diện chính | Ảnh nguồn |
|---|---|---|
| GD01 | Đăng nhập | [Ảnh 001](images/001.png) |
| GD02 | Đăng ký tài khoản học viên | [Ảnh 003](images/003.png) |
| GD03 | Trang chủ học viên | [Ảnh 005](images/005.png) |
| GD04 | Chọn khóa học và đăng ký khóa | [Ảnh 012](images/012.png) |
| GD05 | Chi tiết hồ sơ, kết quả đầu vào và xác nhận nhập học | [Ảnh 098](images/098.png) |
| GD06 | Chọn hoặc đổi ca kiểm tra đầu vào | [Ảnh 104](images/104.png) |
| GD07 | Lịch thi đầu vào của học viên | [Ảnh 112](images/112.png) |
| GD08 | Hóa đơn và hướng dẫn thanh toán của học viên | [Ảnh 008](images/008.png) |
| GD09 | Lớp và thời khóa biểu của học viên | [Ảnh 114](images/114.png) |
| GD10 | Kết quả học tập đã công bố | [Ảnh 011](images/011.png) |
| GD11 | Tra cứu hồ sơ tuyển sinh | [Ảnh 021](images/021.png) |
| GD12 | Chi tiết và xét duyệt hồ sơ tuyển sinh | [Ảnh 105](images/105.png) |
| GD13 | Quản lý ca kiểm tra đầu vào | [Ảnh 027](images/027.png) |
| GD14 | Quản lý khóa học và tra cứu Band | [Ảnh 031](images/031.png) |
| GD15 | Quản lý chương trình đào tạo | [Ảnh 034](images/034.png) |
| GD16 | Quản lý lớp và lập lịch học | [Ảnh 116](images/116.png) |
| GD17 | Xếp học viên vào lớp phù hợp | [Ảnh 090](images/090.png) |
| GD18 | Duyệt và công bố bảng điểm | [Ảnh 086](images/086.png) |
| GD19 | Lớp và lịch dạy của giáo viên | [Ảnh 045](images/045.png) |
| GD20 | Nhiệm vụ và chấm kiểm tra đầu vào | [Ảnh 078](images/078.png) |
| GD21 | Điểm danh buổi học | [Ảnh 084](images/084.png) |
| GD22 | Sổ điểm và nhập điểm thành phần | [Ảnh 081](images/081.png) |
| GD23 | Hóa đơn và xác nhận thanh toán | [Ảnh 053](images/053.png) |
| GD24 | Báo cáo tài chính và xuất CSV | [Ảnh 092](images/092.png) |
| GD25 | Báo cáo tổng hợp của giám đốc | [Ảnh 096](images/096.png) |
| GD26 | Quản lý tài khoản và cấp nhân sự | [Ảnh 061](images/061.png) |
| GD27 | Nhật ký kiểm toán và chi tiết thao tác | [Ảnh 065](images/065.png) |

## Cách dùng trong báo cáo

Chèn ảnh chính rồi đặt bảng ngay dưới. Có thể sao chép bảng từ HTML sang Word và chọn giữ định dạng. Chỉ thêm ảnh phụ khi cần giải thích trạng thái tạo/sửa/xác nhận. Với ảnh quá dài, chia hình thành a/b trong cùng mục. Bản máy tính là nội dung chính; bộ 117 ảnh vẫn giữ các ảnh điện thoại để đưa vào phần thích ứng thiết bị.

Nội dung đã đối chiếu handler/điều hướng trong mã nguồn; không ghi các nút xem chi tiết là luôn chuyển trang, hoặc các bộ lọc là luôn tự truy vấn. Các nút lưu/xác nhận được mô tả từ mã, không được thực thi để tạo tài liệu. Word gốc, sản phẩm và dữ liệu demo không bị sửa.

Bản thảo cần chủ dự án duyệt câu chữ trước khi đưa vào Word cuối. Khoảng thiếu UI NV04 và các trạng thái chưa có ảnh được ghi trong tài liệu chi tiết. Không bổ sung thành phần tưởng tượng vào ảnh.

## Đối chiếu và tái tạo

`selection.json` lưu 27 ảnh chính, hash ảnh đã sao chép, số dòng bảng và hash các nguồn đối chiếu. Hai mươi bảy bảng riêng có STT bắt đầu lại từ 1. Liên kết nguồn trong Markdown dùng khi mở ở workspace; ảnh/bảng của bộ ZIP là tự chứa.

Chạy `python build_guide.py` trong workspace để tạo lại HTML, mục lục, bản chọn ảnh và ZIP từ Markdown/bộ ảnh hiện có. Chỉ dùng thư viện chuẩn Python; không mở demo hoặc đọc thông tin đăng nhập. Sau khi giải nén riêng, mở `index.html` để đọc; cần workspace gốc nếu muốn tái tạo.
