# Demo localhost — 15/10/2026
1. tools/demo là công cụ operator, không phải endpoint hoặc bộ seed tự động khi chạy ứng dụng.
2. SelectDemo chọn binding cố định một lần trước mọi lần đọc; host thường vẫn chọn LOCAL_DEV.
3. Demo dùng localhost8444, catalog EnglishCenter_DEMO_LOCAL và thư mục custody riêng.
4. Runtime/maintenance vẫn chạy bằng hai tài khoản Windows không phải Admin và đủ9 readiness probes.
5. Seed tạo catalog mới bằng6 migrations hiện có; không tắt trigger/FK/check hoặc sửa schema.
6. Giao dịch fixture tạo dữ liệu giả lập, giữ các application locks; lịch sử không có audit giả.
7. Receipt ghi databaseID/GUID; chỉ phục hồi lần seed lỗi khi catalog đúng và còn trống.
8. Credentials lưu DPAPI; nhân viên đổi mật khẩu qua UI và sổ được cập nhật từng kết quả thành công.
9. Trang bàn giao chỉ nằm trong thư mục riêng của chủ máy, ngoài Git và static webroot.
10. Launcher giữ stdin của hai host, đóng stdin để dừng êm và giữ nguyên dữ liệu khi restart.
11. Validate-Demo.sql kiểm tra số lượng, Band/điểm, unfinished, xếp lớp/học phí và lịch.
12. Không có Internet/tunnel, reset dữ liệu tự động hoặc bằng chứng backup/restore trong đợt này.
