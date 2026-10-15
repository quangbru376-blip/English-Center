# Đồng bộ nghiệp vụ và tài liệu báo cáo — 15/10/2026

Owner yêu cầu push những công việc liên quan nghiệp vụ và báo cáo lên GitHub cho
agent khác tiếp tục. Phạm vi bao gồm257commit đã có trên main nhưng chưa có ở
origin/main721e142, phần demo/UI mới đã review và tài liệu/nguồn báo cáo còn local.
Không rewrite lịch sử hoặc force push; chuẩn bị trên nhánh đóng gói rồi áp dụng
fast-forward main theo ADR163 và push main theo yêu cầu owner.

Selected skills: commit/create-branch cho đóng gói Git; code-review cho kiểm
phạm vi. Dùng prefix `codex/` của workspace, không thêm approval cho tên nhánh
thường lệ; quyền áp dụng/push đã được owner cấp. Không dispatch agent ngoài.
Backend/frontend review đã lưu trong LOCAL_DEMO_REVIEW, UI_SPACING_REVIEW và
STUDENT_REGISTRATION_FEEDBACK; không thay nghiệp vụ thêm trong lượt đồng bộ.

Đầu ra chính: [REPORT_AGENT_HANDOFF](../REPORT_AGENT_HANDOFF.md) với47liên kết
đã kiểm tồn tại; source precedence,37NV/32F/21W,24bảng/230cột, tài liệu use case/
sequence/biểu mẫu/FK/database, snapshot Word hiện tại và giới hạn audit/evidence.
Không sửa nội dung Word. Snapshot Word có hash khác các audit cũ, được ghi rõ.

Kiểm trước xuất bản: remote fetch không có divergence;4019blob của lịch sử chưa
push/201.06MB raw, không blob>10MB; quét hữu hạn chữ ký private key/provider
token/JWT-cookie/CSRF-cookie không có finding. Pending ZIP/DOCX đọc/CRC PASS,
embedded XML/text được quét cùng source. Đây không phải chứng nhận không thể
còn bất kỳ secret nào. Chỉ dùng kết quả tên file/count, không lưu giá trị secret.
File Office lock/recovery, frontend/.tmp và .local-demo được ignore; không đưa
ledger/password/key/custody/database/installed packages/build vào commit.

Owner Word và các tài liệu báo cáo được chia sẻ nguyên trạng theo yêu cầu hiện
tại; các checkpoint “unshared/untracked” cũ có trước quyền chia sẻ lần này.
Các công cụ demo chỉ được publish dưới dạng source, không chạy seed/provision/
password/reset/backup/restore trong lượt này. Demo đang dùng không bị khởi động
lại hoặc thu hồi phiên vì đồng bộ Git. M4 recovery/UIgate4/NV04 còn mở.

Validation:186frontend tests PASS và production build đã chạy ngay trước lượt
này trong STUDENT_REGISTRATION_FEEDBACK; fresh typecheck/lint kiểm khi đóng gói.
Không claim fresh backend/SQL/browser/load/recovery suite từ thao tác push.
Final staged-tree/path/Word snapshot checks và remote SHA verification là gate
cuối. Publish receipt được trả trong phản hồi GitHub thực tế, không đoán SHA.
