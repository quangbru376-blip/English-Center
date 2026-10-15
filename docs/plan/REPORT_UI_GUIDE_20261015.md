# Bộ giao diện quan trọng và bảng mô tả thành phần — 15/10/2026

Owner yêu cầu rút bộ ảnh xuống khoảng20 hoặc hơn giao diện quan trọng, mỗi giao diện mô tả theo mẫu bốn cột STT/Loại/Ý nghĩa/Biến cố/Xử lý. Phạm vi: tài liệu báo cáo, không đổi UI/nghiệp vụ, không chụp lại hoặc ghi dữ liệu demo, không sửa Word gốc.

Kết quả: [27 giao diện](../report-ui-guide-20261015/README.md), [bản chi tiết](../report-ui-guide-20261015/MO_TA_27_GIAO_DIEN.md), [HTML](../report-ui-guide-20261015/index.html). GD01–GD27 là mục báo cáo, bao quát32mãF còn hiệu lực bằng các nhóm; không tăng/đổi37NV/use case. Có27bảng riêng và1bảng điều khiển chung,303dòng;27ảnh chính cùng32ảnh phụ/thành phần chung, sao chép nguyên byte từ bộ117ảnh. Ảnh phụ không tăng số giao diện chính.

Nguồn: Memory đã đọc theo AGENTS; REPORT_AGENT_HANDOFF, requirements, FUNCTION_SPEC phần9, ảnh/manifest15/10 và handler/điều hướng/components thực tế. `selection.json` lưu hash nguồn liên kết, hash ảnh, số bảng/dòng và27ảnh chính.

Skill routing: đã đọc SKILL_ROUTING. Không sửa backend/frontend; không cần ép hai skill triển khai vào tác vụ tài liệu. Áp dụng `code-review` cho bộ xuất tĩnh `build_guide.py` và kết quả. Canonical/mã nguồn thực tế ưu tiên hơn bảng mẫu; bảng mẫu chỉ quy định bốn cột, không đưa các nút hoặc hành vi của mẫu vào sản phẩm.

Rà nội dung: phân biệt chuyển mục/hiển thị chi tiết tại chỗ/mở hộp thoại/lưu; thay ô lọc không tự truy vấn; phân biệt bước xem trước với xác nhận. Đã sửa trong bản thảo quy trình xác nhận đầu vào thành một nút Xác nhận trực tiếp ở hộp thoại xem trước theo mã; trọng số và sĩ số là ô văn bản, không gán nhầm NumberBox. Không coi vắng/thiếu dữ liệu là0; không coi gửi bảng điểm là công bố; chỉ kế toán xuấtCSV. Các thao tác ghi chỉ mô tả từ nguồn, không thực hiện để xác minh tài liệu.

Kiểm tra xuất: build chỉ dùng thư viện chuẩn Python; đếm27mục/GD liên tục,28bảng/đúng4cột/STT liên tục từng bảng/303dòng;27ảnh chính phân biệt đều desktop;59ảnh hash khớp nguồn; liên kết nguồn tồn tại; ZIP kiểm traCRC. Edge thật mở HTML cục bộ:27mục/28bảng/303dòng/4cột/27ảnh chính/27liên kết điều hướng; chọn GD11/GD26 và liên kết ảnh061 đúng; không pageerror/không tràn ngang trang ở390px. `browser-check.json` giữ kết quả. Đây là kiểm tra bản tài liệu tĩnh, không phải kiểm tra lại web nghiệp vụ.

Giới hạn: bản thảo cần owner duyệt câu chữ khi biên tập báo cáo; chưa chèn Word. Không có ảnh bản in trình duyệt/mật khẩu tạm/sửa cấu hình chương trình nháp không tồn tại ở demo; mô tả điều khiển có điều kiện dựa trên mã và biểu mẫu tạo. Khoảng thiếu NV04 nhãn điểm tốt nhất/lượt chọn được giữ rõ. Không có fresh product/SQL/auth/ghi nghiệp vụ acceptance claim.

Trạng thái: COMPLETE; Codex code-review PASS cho bộ xuất tài liệu theo phạm vi và các kiểm tra trên. Nội dung vẫn là bản thảo báo cáo chờ owner biên tập, không là nghiệm thu sản phẩm. Không commit/push/deploy hoặc sửa các file Word/sequence/usecase owner đang thay đổi. Chỉ cập nhật chỉ mục bàn giao và note của task này.
