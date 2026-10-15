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

Bổ sung [Kết quả đạt được và hướng phát triển](report-texts-20261015/KET_QUA_VA_HUONG_PHAT_TRIEN.md) theo yêu cầu owner15/10: kết quả phân tích/thiết kế/chức năng/kỹ thuật/kiểm thử có giới hạn; ưu tiên NV04, phản hồi UI, vận hành/sao lưu-khôi phục và đánh giá tải trước mở rộng. Thông báo, báo cáo nâng cao, nhiều cơ sở/thanh toán trực tuyến chỉ là đề xuất tương lai, không coi đã triển khai hoặc thay phạm vi hiện hành. Không fresh test/production/recovery claim, không sửa Word gốc.

Bản thảo [PHẦN MỞ ĐẦU](report-texts-20261015/PHAN_MO_DAU.md) theo mẫu mục lục owner15/10 gồm6mục: lý do chọn đề tài/mục tiêu/đối tượng/phạm vi/phương pháp/kết quả dự kiến. Bám PROJECT_CONTEXT và requirements:7vai trò, kiểm tra trực tiếp, thu tiền ngoài ứng dụng, demo cục bộ; không thêm khảo sát/phỏng vấn thực tế chưa có bằng chứng hoặc tuyên bố hiệu quả định lượng. Chưa chèn/sửa Word gốc; phần kết quả dự kiến là mục tiêu của đề tài, không phải fresh nghiệm thu sản phẩm.

Theo yêu cầu rút gọn giao diện15/10: [bộ27giao diện quan trọng](report-ui-guide-20261015/README.md), [ảnh và bảng](report-ui-guide-20261015/index.html), [nguồn mô tả](report-ui-guide-20261015/MO_TA_27_GIAO_DIEN.md), [ZIP](report-ui-guide-20261015.zip). Mỗi GD có ảnh chính và bảng STT/Loại/Ý nghĩa/Biến cố/Xử lý;27bảng riêng+1bảng chung/303dòng;27ảnh chính và32ảnh phụ/dùng chung nguyên byte từ bộ117. GD là mục báo cáo, không đổi32F/37NV. Đối chiếu handler thực tế: chi tiết tại chỗ, áp dụng lọc, xem trước/xác nhận được phân biệt; chưa chèn Word/không thực thi lệnh ghi. Task card [REPORT_UI_GUIDE_20261015](plan/REPORT_UI_GUIDE_20261015.md), `selection.json` giữ nguồn/hash và số liệu kiểm tra. Bản thảo nội dung cho owner biên tập; không có fresh product acceptance claim.

Bộ ảnh giao diện15/10/2026: [thư viện](report-screenshots-20261015/index.html), [mục lục và đối chiếu32mãF](report-screenshots-20261015/README.md), [ZIP](report-screenshots-20261015.zip). Có117PNG gốc:90máy tính1440px và27điện thoại390px, đủ7vai trò cùng khách; trang chính/chi tiết/biểu mẫu/xem trước tiêu biểu, ảnh toàn chiều dài. Chụp Edge thật trên demo HTTPS hiện có, không gửi lệnh ghi nghiệp vụ/reset dữ liệu/thu hồi phiên owner. Có nhật ký đăng nhập/đăng xuất phiên chụp riêng. Không chụp mật khẩu tạm hoặc bản in trình duyệt; chương trình nháp hiện0 nên không dựng dữ liệu để mở form sửa. NV04 nhãn điểm tốt nhất vẫn thiếu; không tuyên bố mọi trạng thái hoặc fresh nghiệm thu sản phẩm. `manifest.json` có kích thước/hash ảnh và lịch sử hai lỗi chọn control đã kiểm tra lại. Bộ ảnh mới, không lấy ảnh lịch sử làm trạng thái demo hiện tại.

Ngày 15/10/2026 chủ dự án chọn 37 bảng đặc tả chính theo 37 NV còn hiệu lực, thay đề xuất chia 83 thao tác. Chỉ mục: [37 đặc tả](usecase/usecase_specification/DANH_SACH_37_DAC_TA_USECASE.md); [quy ước](usecase/usecase_specification/QUY_UOC_DAC_TA_USECASE.md). NV36–NV39 là xử lý tự động hỗ trợ; Đăng xuất/Đổi mật khẩu/Sửa hồ sơ riêng ở phụ mục E01 mục8, không bỏ chức năng hoặc thêm NV. Đây là cách tổ chức báo cáo, không thay đổi quyền/nghiệp vụ hoặc tự sửa sơ đồ/Word.

Cập nhật mới nhất 15/10/2026: đã hoàn tất 37/37 bảng chính bản thảo trong [bộ đặc tả](usecase/usecase_specification/README.md), gồm bốn đợt UC-001–010, UC-011–020, UC-021–030 và UC-031–037. Đủ chín mục/bảng, mỗi NV còn hiệu lực xuất hiện đúng một lần; đã rà mapping/liên kết và đối chiếu hợp đồng/mã liên quan. [Phụ mục thao tác chung](usecase/usecase_specification/THAO_TAC_CHUNG.md) có Đăng xuất/hết phiên, Tự đổi mật khẩu và Sửa hồ sơ; không thêm NV/bảng chính thứ38. Bốn bảng tự động không là menu/tác nhân Hệ thống mới. Attendance không thêm AuditLog; CSV sáu cột/toàn tập mới/footer tính vào 10000/5000000byte; mật khẩu tạm không truy xuất lại. Chủ dự án chưa duyệt nội dung từng bảng, chưa dàn trang/chèn Word hoặc đồng bộ mã sơ đồ. NV04 nhãn điểm/lượt tốt nhất vẫn là khoảng thiếu UI, không được coi đã sửa. Không sửa source Word/code/runtime/demo hoặc fresh test/SQL/Excel/load claim. Tiếp theo: review nội dung theo phản hồi chủ dự án, rồi dàn trang/ánh xạ sơ đồ khi được giao.

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
SHA256 của snapshot đồng bộ lần này: `bfcee90ab805071c6e4e4b300d69fc5032a2a36d291ddee5138a2520348addc2`.
Snapshot trước có hash `36c9468a...`; owner tiếp tục biên tập giữa các lần đồng bộ.
Các hash/đối chiếu ngày06/10,14/10 hoặc audit15/10 trước đó có thể là snapshot
khác. Không tuyên bố bản Word hiện tại đã được audit lại toàn bộ; kiểm lại phần
được giao trước khi sửa. Source Word và tài liệu xuất không tự ghi đè baseline.

Các Word use case minh họa cũ đã chuyển vào [usecase/usecase](usecase/usecase/);
bộ37đặc tả bản thảo mới nằm ở thư mục riêng được dẫn phía trên. Hình
[SQ03-A đang biên soạn](BO_HUONG_DAN_SEQUENCE_20261014/README.md) có ghi chú
phải sửa nhánh sau đổi mật khẩu tạm trước khi dùng chính thức; không lấy hình
này để suy ra sản phẩm tự cấp phiên thường. Các nguồn hình và Word được chia
sẻ nguyên trạng để nhóm tiếp tục biên tập, không có fresh nghiệm thu nội dung.

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
