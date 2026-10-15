# Đồng bộ tài liệu cho nhóm — 15/10/2026

Owner yêu cầu commit/push những phần cần thiết để chia sẻ với thành viên nhóm.
Remote main đã có `e8945a9` (source nghiệp vụ/UI/demo và bàn giao trước); lần này
bổ sung phần báo cáo mới, không lặp lại triển khai hoặc khởi động lại demo.

Skills: `commit`, `create-branch`, `code-review`; prefix `codex/` theo workspace.
Nhánh đóng gói `codex/team-report-sync-20261015`, áp dụng fast-forward main rồi
push main theo quyền đồng bộ hiện hành. Không rewrite lịch sử/force push.

Phạm vi:37bảng đặc tả use case và phụ mục chung,117ảnh giao diện và bản chọn
27giao diện/bảng mô tả, hai đoạn văn báo cáo, SVG/PNG/viewer/script SQ03-A đang
biên soạn, Word hiện tại, bản in, chỉ mục bàn giao. Mười Word use case cũ được
chuyển nguyên byte vào `docs/usecase/usecase/`, không xóa nội dung. File tmp,
Office lock/recovery, credentials/password/keys/DPAPI/database/build không đưa
vào commit. Không truy cập sổ tài khoản hoặc dùng trang bàn giao mật khẩu.

Word được đọc theo chế độ chia sẻ, tạo snapshot byte ổn định khi owner vẫn có
thể mở tài liệu; không ghi lại nội dung hoặc metadata Word. Snapshot đồng bộ:
`BFCEE90AB805071C6E4E4B300D69FC5032A2A36D291DDEE5138A2520348ADDC2`.
Hash khác snapshot36C9468A trước; đã cập nhật chỉ mục, không coi audit cũ là
nghiệm thu lại Word hiện tại.

Kiểm đóng gói: ZIP/DOCX CRC và XML/SVG/Python syntax, ảnh so với manifest,
liên kết Markdown của các tệp trong gói, di chuyển Word nguyên byte, staged
paths, kích thước blob và quét hữu hạn chữ ký private-key/provider-token/
JWT/password-literal. Không in/lưu giá trị bí mật. Không có finding từ lần
quét này; đây không phải bảo đảm tuyệt đối mọi secret hoặc audit nội dung.

Giới hạn: SQ03-A minh họa đang ghi sai nhánh cấp phiên thường sau đổi mật khẩu
tạm. Ghi chú tại README/bàn giao yêu cầu sửa trước dùng chính thức; giữ nguyên
hình/script owner để nhóm tiếp tục biên tập. Source sản phẩm và hướng dẫn SQ04
mới là căn cứ: thu hồi phiên, yêu cầu đăng nhập lại. Các đặc tả/văn bản mới là
bản thảo; không đồng nghĩa owner đã duyệt từng bảng. Không sửa Word/sản phẩm
để làm chúng khớp với hình.

Review đóng gói: PASS sau sửa liên kết và xác nhận staged snapshot. Không chạy
lại backend/SQL/browser suite vì chỉ chia sẻ tài liệu/ảnh và công cụ vẽ; không
claim fresh product/load/recovery acceptance. Giữ nguyên mutable demo/custody/
phiên owner. M4 recovery, UIgate4, NV04 vẫn mở. Remote SHA và trạng thái publish
được xác nhận bằng GitHub thực tế và trả trong phản hồi cuối.
