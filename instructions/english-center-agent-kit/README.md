# Cách dùng bộ này

1. Copy `AGENTS.md` và thư mục `.agents/` vào gốc repo của dự án. Codex và Antigravity đọc chung hai thứ này.
2. Thư mục `docs/` đã có sẵn `requirements.md` (bản nháp), `reference/screens.md` (danh sách màn hình của bạn). Bỏ file SQL T-SQL cũ vào `docs/reference/`. Đọc mục 9 của requirements.md và trả lời các câu hỏi mở trước khi giao cho agent.
3. Kiểm tra: mở repo bằng từng agent, hỏi "liệt kê các skill bạn thấy". Nếu thiếu skill, thử đổi `.agents/` thành `.agent/` (Antigravity hỗ trợ cả hai).
4. Chạy Slice 0 (đăng nhập + phân quyền + danh sách/tạo học viên) qua vòng: Codex lập kế hoạch, Antigravity làm, Codex review. Ghi lại số lần phải sửa rồi mới mở rộng.
5. Khi agent lặp lại một lỗi, thêm đúng quy tắc đó vào skill tương ứng.
