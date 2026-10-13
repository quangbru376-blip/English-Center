# Giai đoạn1 — review PASS / chuẩn bị hoàn tất / cổng1 chờ owner duyệt

Ngày13/10/2026. Source baseline26067e0, branch ui-refresh. Phạm vi giai đoạn1
(giai đoạn0 kế hoạch gốc) là kiểm kê/đóng băng/capture, không redesign. Owner giao
thực hiện giai đoạn đầu; không có worker. Codex review theo code-review skill,
UI_REFRESH_STAGE_1_TASK_CARD và SKILL_ROUTING; schema/migrations N/A.

## Đầu ra

- UI_REFRESH_PLAN.md: sáu giai đoạn đủ lớn, bốn output gates; lựa chọn1A–6A giữ.
- UI_REFRESH_INVENTORY.md:32F và F15 removed, seven-role route/local-view/action/
  component/state mapping, proposal nav/initial-read/session recovery và risks.
- UI-refresh-evidence/source-manifest.json:97source/config/package files frozen,
  literal selectors và route literals; dynamic selectors được rà bằng source.
- UI-refresh-evidence/index.html: local gallery lọc role/viewport/search;536PNG
  representative, seven roles+Anonymous,375/768/1280,15business groups81–95.
  captures.json là manifest de-duplicated theo filename cuối; tất cả536SHA256
  đã khớp bytes. Append-only raw manifest giữ các attempts cũ; không lấy hash
  một entry bị overwrite ở attempt cũ làm hash ảnh hiện tại.
- Ba scripts test-only: adapter capture cho existing driver, source manifest,
  local gallery. Không thêm UI production route, runtime DI hoặc dependency.

## Kiểm chứng thực tế

| Check | Kết quả mới |
|---|---|
| Frontend npm build (typecheck+Vite) | PASS |
| npm lint | PASS |
| Existing frontend Node tests | 170PASS/0fail/0skip |
| Backend default dotnet test | 1438PASS/325intentional opt-inSKIP/0fail |
| Backend explicit build | PASS/0warnings/0errors |
| Edge M1 groups81–88 + M2/M3 groups89–94 | 14PASS/0fail/0skip, browser-all.trx |
| Edge M4 group95 | 1PASS, browser-smoke-final.trx |
| Identity/theory baseline groups1/2/4/5/6/9/81–88 | 14PASS, browser-theories.trx; eight overlap prior set |
| Student additional read/profile/booking confirmation/unknown capture | Final group85 1PASS, browser-student-final.trx; earlier tour1PASS retained |
| Unique browser cases | 21 across successful sets, not sum of repeated runs |
| Source/package rehash | 97/97match; productSourceDrift0 |
| Capture digests | 536/536match |
| Owned SQL fixture receipts | 32created/32dropped, exact name+ID+GUID, badpairs0 |
| Independent master read-only query | live scratch0 |
| Listener8443 after acceptance | 0 |
| git diff --check / script syntax | PASS |

Logs/TRX/ownership records dưới UI-refresh-evidence. Scratch SQL uses existing6
migrations with synthetic identities and owned fixture COMMIT; operational catalog,
D07, production keys/principals/profile remain untouched. Captures use real installed
Edge and HTTPS fixture pin; negative untrusted/wrong-pin checks still execute. This
does not claim genuine LOCAL_DEV/production readiness was newly reverified.

Initial harness failures retained: main.tsx CSS import in generic compile recipe,
sandbox Windows access, first browser TEMP mismatch. Corrected recipe/environment
without product fixes; failed browser fixture identity-cleaned. No unresolved test
failure in the final sets. No blanket rerun of SQL/regression beyond scoped browser.

## Coverage và giới hạn

| Screen family | Before/check references |
|---|---|
| F01/F02 auth | Anonymous login/signup3widths; existing signup/password/session tests; identity theory normal/restricted/revocation |
| F03–F09 Student enrollment | groups85/87; home/course/profile supplemental; booking confirmation/unknown/manual lookup/calendar/result/confirm |
| F10 Student invoices/print | groups88/89; fresh print and UNPAID/PAID state contract |
| F11/F19–F22/F24 Class/placement/schedules | groups90/91/92; current tasks and Academic/Teacher/Student scoped pages |
| F12/F23/F26/F27 grades/attendance | groups92/93/94; draft/missing, readonly, cohort review/publication |
| F13/F14 Admissions | group88; search/detail/approval/cancellation preview |
| F16/F17/F18 Session/Course/Program/Band | groups82/83/84, source traces |
| F25 Teacher entry | groups86/87; actual assigned task/absence/correction/confirmation freeze |
| F28/F29/F30 Finance | groups89/95; payment/correction and reports/CSV |
| F31 Director / F32 Admin / F33 audit | groups95/81; all role landing snapshots, aggregate-only/audit grouping/secret mask |

Inventory covers all32F; screenshots are selected representative views at meaningful
checkpoints, not one independent URL per F or screenshots of every possible state.
Transient loading/all validation errors/each modal variant are not exhaustive capture
coverage; record and check these on each changed screen in later implementation.
Tests and source review cover behavior separately; screenshots cannot certify audit,
rights, unknown semantics or pixel parity. Synthetic clocks/IDs/data differ by group.
No layout/contrast/accessibility PASS claimed for the old UI; findings motivate redesign.

Manual inspection of375login,375Student landing,1280Director confirms old system font/
blue tokens, duplicated Student nav and manual load. Password inputs/temporary password
paragraphs are masked in captures; magenta rectangles are redaction, not product style.
No plaintext passwords/JWT/CSRF/API bodies saved to gallery. Synthetic contact text only.

## Cổng1 — đề xuất cụ thể để owner duyệt

1. Một menu Student, bỏ hai hàng trùng; desktop thanh trên/mobile drawer. Items:
   Trang chủ, Khóa học, Hồ sơ & thi đầu vào, Lớp & lịch, Hóa đơn, Kết quả.
   Account menu riêng cho Profile/password/logout. Current routes/views giữ nguyên.
2. Initial scoped reads khi mở home/courses/profile/calendar có query hợp lệ;
   “Tải lại” phụ và Apply filters rõ. Không polling/write auto/idle renewal.
3. “Kiểm tra lại phiên” giữ ở recovery dưới nhãn thân thiện, không xóa checkSession.
4. Ưu tiên component nền và auth/shell đồng bộ ở giai đoạn2; cards/status/nav styles
   mới, giữ tất cả critical handlers/selectors/unsaved/unknown boundaries.

Review verdict **PASS cho giai đoạn chuẩn bị**. Cổng1 còn chờ owner duyệt đầu ra
này theo UI_REFRESH_PLAN.md: “Chỉ sau phản hồi mới thực hiện phần nền/layout phụ
thuộc đầu ra đã duyệt.” Không bắt đầu giai đoạn2 từ việc PASS tooling baseline.
M4 recovery vẫn chờ operator inputs; không backup/restore/load/deploy claim.
