# Bàn giao cho agent hỗ trợ báo cáo

Cập nhật15/10/2026. Owner yêu cầu đồng bộ mã nghiệp vụ và toàn bộ tài liệu phục vụ
báo cáo lên GitHub để agent khác tiếp tục tra cứu, đối chiếu và biên soạn.
Đây là chỉ mục nguồn và giới hạn bằng chứng, không thay đặc tả đã chấp nhận.

## Đọc theo thứ tự

Đọc [AGENTS](../AGENTS.md), rồi Project Memory theo thứ tự bắt buộc:
[PROJECT_CONTEXT](PROJECT_CONTEXT.md) → [ARCHITECTURE](ARCHITECTURE.md) →
[CURRENT_STATE](CURRENT_STATE.md) → [DECISIONS](DECISIONS.md) → [TODO](TODO.md).
Sau đó đọc [requirements](requirements.md), mục liên quan trong các nguồn sau
và execution plan của công việc được giao. Không tự bắt đầu tích hợp agent,
provision môi trường, thay dữ liệu demo hoặc công việc ngoài yêu cầu.

## Nguồn nghiệp vụ và triển khai

| Cần hiểu | Nguồn chính |
|---|---|
| Quyền, quy tắc và phạm vi sản phẩm | [requirements](requirements.md), [DECISIONS](DECISIONS.md) |
| Chức năng37NV/32F (NV10/NV11/F15 bị loại) | [E01 chức năng](design/FUNCTION_SPEC.md), [catalog](design/FUNCTION_CATALOG.md) |
| Workflow21W, chuyển trạng thái và guard | [E02](design/WORKFLOW_SPEC.md) |
| Dictionary24bảng/230cột, quan hệ và SQL objects | [E03](design/DATABASE_SPEC.md), [SQL integration](design/E04_SQL_INTEGRATION_SPEC.md) |
| Hợp đồng154API/DTO, transaction/replay/audit | [E04 final](design/E04_FINAL_CONTRACT.md), [DTO](design/E04_DTO_SPEC.md), [replay](design/E04_REPLAY_SPEC.md) |
| Trace và nhất quán baseline được chấp nhận | [E05](design/E05_BASELINE_REVIEW.md) |
| Hành vi thực tế | [backend/src/EnglishCenter.Api](../backend/src/EnglishCenter.Api/), [frontend/src](../frontend/src/), [CURRENT_STATE](CURRENT_STATE.md) và review/evidence tương ứng |

Không lấy hình giao diện/biểu mẫu minh họa làm quyền nghiệp vụ mới. Khi mô tả
hành vi đã triển khai, đối chiếu mã thực tế và checkpoint mới nhất; nhiều phần
cũ trong Memory là lịch sử, không phải yêu cầu quay lại trạng thái chưa triển khai.

## Tài liệu phục vụ báo cáo

| Công việc | Điểm bắt đầu |
|---|---|
| Sửa theo cấu trúc báo cáo Word | [REPORT_REVISION_GUIDE](reference/REPORT_REVISION_GUIDE.md), [PROCESSING_REPORT_MEMORY_COMPARISON](reference/PROCESSING_REPORT_MEMORY_COMPARISON.md) |
| Bảng chức năng nghiệp vụ | [Đối chiếu bảng nghiệp vụ](reference/BUSINESS_FUNCTION_TABLE_COMPARISON_20261014.md) |
| Đối chiếu chức năng trong Word với web | [Audit15/10](reference/function-web-audit-20261015/REPORT.md) |
| Use case | [Bản cập nhật Word](USE_CASE_CAP_NHAT_THEO_DU_AN_20261014.docx), [PDF](USE_CASE_CAP_NHAT_THEO_DU_AN_20261014.pdf), [nguồn sơ đồ](USE_CASE_NGUON_SO_DO_20261014.zip), [tài liệu usecase](usecase/) |
| Sequence theo nghiệp vụ/code | [Bộ25hướng dẫn](design/sequence-guides-20261014/README.md), [10sequence ưu tiên](design/sequence-guides-20261014/UU_TIEN_10_SEQUENCE.md), [review](reference/SEQUENCE_GUIDES_REVIEW_20261014.md) |
| Biểu mẫu và bản in | [Đối chiếu](reference/FORMS_COMPARISON_20261014.md), [biểu mẫu thiết kế lại](BIEU_MAU_NGHIEP_VU_THIET_KE_LAI_20261014.docx), [bản in Word](BIEU_MAU_BAN_IN_CHUYEN_NGHIEP_20261014.docx), [PDF](BIEU_MAU_BAN_IN_CHUYEN_NGHIEP_20261014.pdf) |
| Dictionary/FK/sơ đồ dữ liệu cho báo cáo | [Bộ đối chiếu database](DOI_CHIEU_DATABASE_BAO_CAO_20261014.zip), [tác động FK](reference/REFERENTIAL_INTEGRITY_IMPACT.md), [sơ đồ thẳng hàng](SO_DO_LOGIC_6_1_THANG_HANG_20261014.pdf), [SVG](SO_DO_LOGIC_6_1_THANG_HANG_20261014.svg) |
| Tạo lại tài liệu đã sinh | Các script `build_*_20261014`, `align_forms_word_pdf_20261014.py`, `export_logic_diagram_20261014.py` tại [reference](reference/) và các nguồn ZIP tương ứng |
| Chạy/demo/giới hạn vận hành | [Hướng dẫn sản phẩm](HUONG_DAN_CHAY_SAN_PHAM.md), [demo localhost](DEMO_LOCALHOST.md), [review demo](plan/LOCAL_DEMO_REVIEW.md) |

Bản Word của owner tại [Nhom18_ThietKeXuLy.docx](Nhom18_ThietKeXuLy.docx)
được chia sẻ nguyên trạng theo yêu cầu đồng bộ báo cáo lần này, không sửa nội dung.
SHA256 của snapshot đã kiểm: `36c9468a717e79e21e4b2367725385bc7d3415f4dcee7e2bee463486b11eea78`.
Các hash/đối chiếu ngày06/10,14/10 hoặc audit15/10 trước đó có thể là snapshot
khác. Không tuyên bố bản Word hiện tại đã được audit lại toàn bộ; kiểm lại phần
được giao trước khi sửa. Source Word và tài liệu xuất không tự ghi đè baseline.

## Trạng thái cần mô tả đúng

- M0–M3 và phần M4 báo cáo/CSV/nhật ký Admin đã triển khai, review PASS theo
  evidence được lưu. M4 backup/recovery còn thiếu đầu vào vận hành và nghiệm thu.
- UI refresh đã qua technical review; owner cổng4 còn mở. Các sửa khoảng cách
  và thông báo lý do sau đăng ký15/10 đã lên demo; [review spacing](plan/UI_SPACING_REVIEW.md)
  và [follow-up](plan/STUDENT_REGISTRATION_FEEDBACK.md) ghi phạm vi cụ thể.
- NV04 hiển thị tường minh điểm đầu vào tốt nhất/lượt được chọn vẫn là mục mở.
  Không mô tả việc sửa thông báo lặp như đã hoàn tất NV04.
- Demo67tài khoản/70hồ sơ là dữ liệu tổng hợp ban đầu; owner đang thực hành nên
  số liệu hiện tại có thể thay đổi. Không reset, seed lại hoặc thu hồi phiên
  owner tự động để tạo ảnh/test. Login/password/key/ledger không nằm trên GitHub.
- Test giả lập UI, test SQL thật, runtime thực và kiểm thủ công là các loại
  bằng chứng khác nhau. Không biến số ảnh/test cũ thành claim fresh load/WCAG/
  RPO/RTO/backup/restore, Internet/public deployment hoặc readiness trên máy khác.

Repository chia sẻ source, tài liệu và evidence đã chọn; không chia sẻ database
đang chạy, mật khẩu/DPAPI/certificate riêng, file Office tạm, `node_modules`,
build output hoặc cấu hình operator bí mật. Clone repo không tự tạo demo dùng
ngay trên máy khác; đọc prerequisite/runbook trước khi thực thi các công cụ.
