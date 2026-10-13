# UI refresh giai đoạn 5 — màn nhân sự PASS

13/10/2026. Owner “đồng ý tiếp tục đi” giao giai đoạn5 theo UI_REFRESH_PLAN.
Codex dùng frontend-ui/code-review/SKILL_ROUTING, không worker. Working tree
ui-refresh, chưa commit/apply main. Không thêm cổng duyệt từng vai trò.

## Kết quả

- Sáu vai trò Admissions/AcademicAffairs/Teacher/Accountant/Admin/Director dùng
  workbench thống nhất: khối bộ lọc, bảng, thẻ bản ghi/chi tiết, form, hành động
  và states. Desktop ưu tiên thông tin; mobile stack/scroll trong bảng riêng.
  Student styles giữ điều kiện riêng trên Classes/Schedules.
- Tables Account/Course/Finance có caption/region/focus và cuộn ngang; không
  cắt field/tiền hoặc giấu overflow để đạt viewport. Native forms/labels/raw inputs,
  disabled/capabilities/validation/confirmation và handlers giữ nguyên.
- ReportMetrics dùng server full-filter totals, không cộng trang/đổi money Number
  hoặc biến no-data thành0. Thẻ tài chính/tuyển sinh/đào tạo đi kèm bảng nhóm/
  full-set footer; thời điểm tra cứu, kỳ mặc định server và draft/applied filter rõ.
  Director vẫn aggregate-only/no-export; CSV Accountant vẫn truy vấn mới đúng bộ
  lọc đã áp dụng/escape/BOM/giới hạn và exact38digits.
- List/default-query initial reads trên các màn nhân sự dùng API hiện có. Admin,
  Admissions, Finance POST search giữ intent read; không phải business write.
  Effect deferred/cancel timer, không phụ thuộc input/render; không polling,
  autosave/retry hoặc idle renewal. Direct detail effects được defer tương tự.
- Secret handoff/clear/expiry/clipboard fallback, dirty Attendance/Grade guards,
  whole-class manifests/finalize, sorted save units/unknown/manual lookup giữ nguyên.
  Không sửa API/parser/client/sessionController/permission/schema/backend/packages.

## Kiểm chứng thực tế

| Kiểm tra | Kết quả / evidence stage5 |
|---|---|
| Frontend toàn bộ |183PASS/0fail/0skip; frontend-tests-final.log |
| Types/build/lint |PASS/0errors/0warnings; build-final.log/lint-final.log |
| Real Edge/HTTPS/SQL owned |21unique PASS/0fail/0skip,9m04; final-browser/stage5-final-edge.trx |
| Roles/widths |7roles +Anonymous,375/768/1280; callbacks/rights/replay/unknown/CSV/print/grade/secret assertions giữ |
| StrictMode actual DEV main |13destinations/6staff roles; mỗi expected endpoint1read; POST search read-only/other mutations0; mock transport riêng, strict-mode.json/strict-mode-final.log |
| Metrics precision/text |38digit formatted money giữ nguyên, no-data khác0, escaped labels/note;2new tests trong183 |
| Photos |593PNG hashes verified;22staff selections; index.html/staff-preview.html |
| Source/scope |107source/package hashes bản cuối đã ghi;22existing files changed+2new presentation/style;id/name/data-testid lost0,ownerprotected drift0; scope-review.json |
| Scratch |42created/42name+ID+GUIDpaired dropped/unpaired0; independent masterlive0/listener8443=0; scratch-cleanup.json |

Kiểm actual Edge covers create/reset/lock/one-time secret, Course/Band/Program/
Session/resource/class workflows, Teacher entry/absence/correction/attendance/grade
draft, whole-class submit/return/finalize, lost ACK/manual lookup/no duplicate writes,
Invoice/payment metadata/print, report/CSV/audit and cross-role refusals. M4 training
no-data/pass-rate/current-vs-period context và Student FINALIZED-only được giữ.
Screenshots dùng synthetic scratch data/password mask; không data operational.
Strict mock kiểm effect lifecycle, không giả proof cookie/SQL/auth/runtime.

Lượt đầu21PASS được giữ. Review ảnh sau đó bổ sung loading rõ trên Program,
Session và Teacher entry, thu gọn nút độc lập. Bốn file trình bày được kiểm lại
bằng toàn bộ183frontend,21Edge và13StrictMode; ảnh/gallery dùng final-captures.
Hai lượt tạo/dọn42scratch, không cộng thành42ca Edge độc lập.

## Review và giới hạn

Đã review raw diff/source: business command bodies/callbacks/manifests/UUID/replay
không thay; edits chủ yếu className/wrapper/caption, bounded initial reads và metric
presentation. Headers/nav selectors và dirty markers bảo toàn. Tables hỗ trợ keyboard
scroll, số lớn không ellipsis/Number; bảng vẫn giữ mọi field. Photos Director/Admin
và assertions ba widths được xem trực tiếp. No observed failing checks in this batch.

Backend default không rerun vì backend/src/tests/lib không đổi; không lấy1438/325
cũ thành kết quả mới. Không dependency mới hoặc runtime/profile-pins rebinding;
không fresh genuine LOCAL_DEV/backup/restore/load/install/push/deploy acceptance.
Owner4/source Word/runbook/user-guide tồn tại trước task không sửa/stage. Vite5178
preview nền còn chạy; test hosts dừng. M4 recovery vẫn chờ operator inputs.

Verdict **PASS cho giai đoạn5**. Giai đoạn6 còn phải nghiệm thu chung, đối chiếu
before/after, rà accessibility/cleanup trong scope, bàn giao/apply/main/genuine
runtime theo contract rồi cổng4 owner xem kết quả tổng thể. Chưa gọi toàn UI
refresh hoặc whole M4 complete.
