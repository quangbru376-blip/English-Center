# UI refresh giai đoạn 4 — phần Student còn lại PASS

13/10/2026. Owner “tiếp tục” duyệt cổng3 và giao phần Student còn lại theo
UI_REFRESH_PLAN/mẫu giai đoạn3. Codex triển khai và review theo frontend-ui,
code-review/SKILL_ROUTING; không worker. Working tree nhánh ui-refresh,
chưa commit/apply main. Giai đoạn4 không có cổng duyệt riêng.

## Kết quả

- Profile: thẻ thông tin có nhãn, form hai cột/stack mobile; email chỉ đọc,
  raw input/validation/maxLength/2RV và đối chiếu unknown giữ nguyên.
- Lịch thi: agenda từng lượt, trạng thái đặt/chấm/vắng/hủy rõ, liên kết đúng
  hồ sơ và phân trang theo server. Sửa bộ lọc bỏ dữ liệu đọc cũ, áp dụng chủ động;
  date controls khóa khi pending để không nhận snapshot lệch với bộ lọc mới.
- Invoice: tự đọc danh sách lúc mở, filter rõ, list/detail có nhãn/trạng thái và
  tiền digit-string nguyên vẹn. Đọc mới rồi in, lỗi đọc mới không in dữ liệu cũ;
  UNPAID vẫn không là xác nhận thu tiền, không lộ transactionReference nhân sự.
  Print body/CSS/handler giữ nguyên, không thêm PDF/export/cách thanh toán mới.
- Classes/schedules: Student tự đọc danh sách/lịch own current lúc mở. Class
  cards chỉ public facts; không sĩ số/guard/provenance/peer. Shared pages giữ
  staff handlers và layout; CSS/initial reads mới có điều kiện role Student.
  Calendar vẫn phân biệt current/history/NOT_HELD/replacement, không tự suy HELD.
- FINALIZED results: tự đọc list/direct detail, hàng tổng quan và detail cùng
  mẫu; finalScore/passed/components/comments do server cung cấp. Nhận xét dài
  wrap/giữ xuống dòng và escape plain text; không tính lại điểm hoặc lộ DRAFT.
- Own password giữ giao diện và hợp đồng đã kiểm chứng ở giai đoạn2. Không
  dependency mới, không thay role/menu/route/DTO/client/auth/session/validation.

## Kiểm chứng

| Kiểm tra | Kết quả / evidence stage4 |
|---|---|
| Frontend toàn bộ |181PASS/0fail/0skip; frontend-tests-reviewed.log |
| Types/build/lint |PASS/0errors/0warnings; build-final/lint-final.log |
| Real Edge/HTTPS/owned SQL |21unique PASS/0fail/0skip,8m55; reviewed-browser/stage4-reviewed-edge.trx |
| Initial reads dưới main DEV StrictMode |HTTP mock riêng:home/course/invoice/class/schedule/grade mỗi loại1,mutation0/unexpected0; strict-mode.json |
| States/keyboard/unknown |375/768/1280; existing selectors/rights/print/lookup assertions giữ; capture adapter còn loading/empty/read-error/profile-dirty/filter-invalidation checks |
| Source/scope |105source/package hashes ổn định;id/name/data-testid mất0; owner protected drift0; scope-review.json |
| Photos |593final PNG hashes verified,8roles inclAnonymous/3widths;14selected Student images in student-preview.html |
| Scratch |42created/42ID+GUID-paired dropped/unpaired0; master independent live0/listener8443=0; scratch-cleanup.json |

Ảnh dùng synthetic data trong SQL scratch thật, không seed catalog operational.
Các ca89/88 chứng minh paid/unpaid print, lỗi đọc mới không in và không lộ finance
reference;90/91 own classes/history/replacement/no-management;94 published detail,
comment1000chars/escape/threshold server;85/87 bảo toàn profile/book/unknown/confirm.
Các ca khác kiểm hồi quy staff/shared shell và identity ở cả3widths.
Strict mock kiểm effect lifecycle, không thay thế proof SQL/auth. Không chạy lại
backend default (source/test/backend/lib không đổi); giữ evidence giai đoạn2,
không gọi1438/325 là số đo mới của giai đoạn4.

## Findings đã sửa

Lần đầu21cases:18PASS/3FAIL. Invoice tiền18digits làm grid rộng hơn375px (89/88);
thêm nhãn Đạt trong list gây trùng exact text ở detail94. Đã sửa minmax(0,1fr),
min-width/overflow-wrap cho số tiền (không hide overflow/ellipsis/chuyển Number),
và ghi rõ “Kết quả: Đạt” ở list. Giữ nguyên assertions, xây snapshot riêng,
chạy lại21ca kèm ảnh: toàn bộ PASS. First logs/TRX/captures giữ riêng, không
tính là PASS hay đưa vào final gallery.

Review xác nhận initial effects deferred/cancel timer để tránh StrictMode replay;
không phụ thuộc draft filter/render nên không polling hoặc tự renew idle; existing
owned task abort/epoch cleanup vẫn chặn late response. Filter mới không được gọi
là kết quả đã áp dụng; command/profile/print callbacks giữ nguyên save/recovery.
StudentClassCards/StudentPersonal là trình bày props thuần. Không schema/grants/
runtime/key custody/profile-pins/owner-file edits/staging/install/push/deploy.

Verdict **PASS cho giai đoạn4**. M0–M3/M4 report/audit trạng thái trước giữ;
M4 recovery vẫn chờ operator inputs. Vite5178 preview nền còn chạy; test hosts
dừng. Bước tiếp theo giai đoạn5 các màn nhân sự theo mẫu đã duyệt, sau đó
giai đoạn6 nghiệm thu chung/cổng4; chưa claim toàn UI refresh hoàn thành.
