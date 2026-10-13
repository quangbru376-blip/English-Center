# UI refresh giai đoạn 3 — Student pilot PASS

13/10/2026. Owner “tiếp tục đi” duyệt cổng 2 và giao giai đoạn 3 theo
UI_REFRESH_PLAN. Codex thực hiện, kiểm thử và tự review bằng frontend-ui,
code-review và SKILL_ROUTING; không worker. Kết quả nằm trong working tree
nhánh ui-refresh; chưa commit/apply main. Cổng 3 chờ owner xem mẫu.

## Kết quả có thể xem

- Trang chủ tự đọc hồ sơ khi mở: thẻ riêng cho từng Registration, bước hiện tại,
  việc tiếp theo từ server, ca thi/buổi học gần nhất, Invoice chưa thanh toán,
  liên kết kết quả đã công bố. Không suy tổng từ trang đã tải, không tiến độ giả,
  không coi chưa có điểm là 0, không đọc kết quả chưa FINALIZED.
- Khóa học tự đọc khi mở, thẻ mã/tên/học phí nguyên đồng và đăng ký chủ động.
  Tiền vẫn là chuỗi chính xác; không Number-convert hoặc tạo hóa đơn trước duyệt.
- Hồ sơ/attempt/Band, chọn ca/đổi ca, xác nhận và vùng đối chiếu unknown dùng
  bố cục nền đã duyệt. Không chọn sẵn ca. Sửa bộ lọc ngày xóa lựa chọn cũ, phải
  đọc lại trước khi chọn/xác nhận. Giữ UUID/manifest/RV và các save unit hiện có.
- Profile/calendar có initial read theo contract cũ. Profile sửa chưa lưu cảnh báo
  cả shell/local/back navigation; hủy rời trang giữ bản nháp. Unknown vẫn đối chiếu
  chủ động, không tự gửi lại mutation. Hoàn thiện thiết kế Profile/calendar và
  Invoice/lớp/kết quả chi tiết thuộc giai đoạn 4.
- Tách StudentOverview trình bày thuần, student.css dùng token hiện có; nâng cấp
  không thay lib/client/auth/controller/API parser/validation hay dependencies.

Xem mẫu trực tiếp: http://127.0.0.1:5178/tests/ui-refresh-student.html.
Đây là trang DEV dùng dữ liệu minh họa, không đăng nhập/call API/lưu dữ liệu;
có thể đổi loading/empty/error và thử menu/dialog. Không có production route mới.
Ảnh chức năng thật bằng Edge/HTTPS/SQL scratch ở UI-refresh-evidence/stage3/index.html;
178 ảnh có manifest/hash, tách khỏi 18 ảnh preview minh họa.

## Kiểm chứng thực tế

| Kiểm tra | Kết quả / bằng chứng |
|---|---|
| Frontend toàn bộ |178 PASS/0 FAIL/0 SKIP; frontend-tests-reviewed.log |
| Types/build/lint |PASS/0 errors/0 warnings; build-final/lint-final.log |
| Edge hồi quy cuối |21 ca duy nhất PASS/0 FAIL/0 SKIP, 8m02; final-browser/stage3-final-edge.trx |
| Student có thêm assertions/ảnh |85/87/88/94:4 PASS; reviewed-browser/stage3-reviewed-edge.trx; không cộng thành25ca |
| Initial read/states/dirty guard |Home và courses mỗi màn1read lúc mở; loading/empty/error, không retry lỗi, bỏ card cũ khi lỗi, hủy rời trang giữ draft, chấp nhận rời xóa draft; state-checks.jsonl |
| StrictMode thực ở main DEV |HTTP mock riêng:Home1/courses1/mutation0/unexpected0; click lại cùng view không xóa màn/đọc lặp; strict-mode.json |
| Responsive/keyboard |375/768/1280, không tràn ngang; drawer/dialog Escape/Tab/focus, account menu; assertions giữ trong adapter |
| Preview riêng |18 ảnh/3widths/loading/empty/error/dialog PASS; apiCalls0; preview/receipt.json |
| Scratch |46 tạo/46 xóa đúng name+ID+GUID, unpaired0, master đọc độc lập live0/listener8443=0; scratch-cleanup.json |
| Scope/source |103 source/package hashes không lệch; literal id/name/data-testid mất0; scope-review.json |

Backend source/test/schema/grants và lib/client/auth/session/validation không đổi.
Không rerun backend default ở giai đoạn này;1438PASS/325intentional opt-in SKIP
của giai đoạn 2 là evidence tham chiếu, không phải số đo mới của giai đoạn 3.
Fixtures xác nhận COMMIT, mất ACK/lookup, reschedule/cancel/confirm, quyền/ownership,
published results và hồi quy điều hướng; không là genuine LOCAL_DEV/profile pin
acceptance hoặc backup/recovery/load/deploy proof.

## Findings và giới hạn

Review đã sửa: không tải lặp khi effect/render; không xóa dữ liệu khi click lại view
hiện tại; CTA Home đồng bộ menu shell; warning bản nháp không xác nhận hai lần;
profile read lỗi bỏ context cũ nhưng giữ draft; bộ lọc ca đổi làm mất hiệu lực
lựa chọn; focus vào hồ sơ/ca vừa đọc; success và unknown có phản hồi bền.

Lần đầu bộ21 có20PASS/1FAIL ở85: adapter kiểm bộ lọc mới được cập nhật trong khi
fixture còn dùng bản dựng cũ. Đây là lỗi tổ chức lần chạy, không được tính PASS.
Đã tách bản dựng/adapter, chạy4Student có ảnh trên bản đã review rồi chạy21cuối
trên đúng snapshot: tất cả PASS. Logs/TRX lần đầu được giữ trong browser/edge.log.
Lần compile test thiếu --ignoreConfig và lint thiếu dependency cũng được sửa;
chỉ lấy kết quả compile/tests/lint cuối, không che các lần lỗi trung gian.

Owner4files, source Word report, runbook/user-guide thay đổi trước task không bị
sửa/stage. Không thêm thư viện, dữ liệu operational, route/quyền nghiệp vụ, polling,
storage/secret sharing hoặc runtime/custody/profile binding mới. Vite5178 để owner
xem mẫu còn chạy; test hosts/8443 dừng. M4 recovery vẫn chờ operator inputs.

Verdict **PASS cho giai đoạn 3**. Owner duyệt mẫu ở cổng3 theo kế hoạch trước
giai đoạn4. Chưa claim toàn bộ Student/staff UI hoặc toàn UI refresh hoàn thành.
