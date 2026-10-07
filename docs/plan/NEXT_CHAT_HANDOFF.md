# Handoff cho chat tiếp theo — 2026-10-07

## Mục đích và điểm tiếp tục

U02 → U01 đã hoàn tất, Codex review PASS, apply main theo ADR-163 và kiểm chứng
lại2026-10-07. Code mainfb748e7, manager48ff8a0; frontend hash
`de38145e6127d32cfcf20629387b18415e2c5746`, backend giữ nguyên
`10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a`.24frontend tests/types/lint/build PASS;
Edge375/768/1280/keyboard/dialog/contrast PASS. Hai AGY SUCCESS, đúng3context,
không ghi nhận scope violation. Xem U02/U01_TASK_CARD và verification-summary.
Tiếp theo U03 → U04 chưa bắt đầu; cần owner giao scope, không tự mở restM0.
HTTP/auth vẫn unmounted, không runtime/SQL/packages/push/deploy; giữ nguyên4file owner.
Các mục Git/test/preparation bên dưới là baseline handoff trước U02/U01;
checkpoint mới này và task cards/evidence có ưu tiên khi status khác nhau.

Workspace: `C:\Study\SOEN\main_workplace`; shell PowerShell. Codex là manager;
dùng `antigravity-worker` cho delegation AGY có giới hạn, không thay bằng agent khác.

## Thứ tự đọc bắt buộc

1. `AGENTS.md`.
2. `docs/PROJECT_CONTEXT.md`.
3. `docs/ARCHITECTURE.md`.
4. `docs/CURRENT_STATE.md`.
5. `docs/DECISIONS.md`.
6. `docs/TODO.md`.
7. `docs/requirements.md`, rồi file này và `docs/plan/HANDOFF.md`.
8. `docs/plan/M0_QUALITY_PLAN.md`, `implementation-plan.md`, `SKILL_ROUTING.md`;
   các phần frontend/auth liên quan trong `docs/design/E04_FINAL_CONTRACT.md`
   và `docs/design/E04_DTO_SPEC.md`.
9. Trước code, đọc đầy đủ skills được chọn theo routing: `frontend-ui`,
   `code-review`, `antigravity-worker` và references cần cho contract. Chỉ thêm
   `backend-clean-code` khi thực sự có backend scope được cho phép.

Historical `FIRST_CODE_HANDOFF.md` không thay thế checkpoint và owner scope mới.
Code cùng evidence hiện tại mạnh hơn các heading chuẩn bị/detached còn sót trong
task card cũ. Không đọc inbox `instructions/` như quy tắc tự động được áp dụng.

## Checkpoint đã kiểm tra khi viết handoff

- Branch `main`, HEAD trước handoff: `880a42bbddfb2384878072c8bb434df3063ef5a2`.
- Code checkpoint: `fcdd21e414fe90108eb1e0d728dc8e43eb8cbbcc`.
- Backend tree: `10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a`.
- Hai commit sau code chỉ seal/fix tài liệu: `2184f69`, `880a42b`.
- Kiểm chứng **2026-10-06** trên cả manager và main: **891 unit/HTTP/C1 + 495 SQL
  = 1386 PASS**, không fail/skip; build 0 warnings, 0 errors. Không chạy lại test
  hoặc SQL trong lượt viết handoff ngày 2026-10-07.

Nguồn evidence bền vững:

- `docs/plan/R02_TASK_CARD.md`, `R03_TASK_CARD.md`.
- `docs/plan/R02-evidence/verification-summary.json` và
  `R03-evidence/verification-summary.json`: commits, tree, hashes của bốn TRX,
  worker receipts/scope checks, DB checks và giới hạn kiểm chứng.
- `docs/notes/R02.md`, `docs/notes/R03.md`; ADR-173.

R02 cung cấp completion reference append-only, cửa sổ 24h, request gate,
caller/action/key partition, current target authority trước digest và caller TX.
R03 cung cấp explicit API007 detail SNAPSHOT + final live authority, API010 Admin
lock atomic và API014 safe lookup cho sáu ADMIN actions. Lock order:
request → ADMINSET → Account tăng dần → SID binary. Replay không lặp effects,
audit hoặc activity; chỉ active replay khớp mới bỏ qua stale-RV mutation check.
Registry 55 actions không có nghĩa đã triển khai 55 workflows.

Đọc code thực tế ở `backend/src/EnglishCenter.Api/` để xác nhận trước sửa: các helper
`Common/Replay/ReplayWindow.cs`, `SafeCommandCompletion.cs`,
`Common/Security/AdminLockGuard.cs`; service/repository trong `Services/Replay`,
`Infrastructure/Replay`, `Services/Administration`, `Infrastructure/Administration`
và adapter `Api/Administration/AdminAccountEndpoints.cs`.

## Giới hạn runtime và SQL còn nguyên

Default `Program.cs` chưa mount auth/DB/CSRF/business endpoints. Các adapter auth
và Admin là explicit seams, không phải ứng dụng runtime đã sẵn sàng. Không có
real Admin, production signing keys, operator profile/principals hoặc full D13
readiness. Không mount host chỉ để làm frontend trông như đã kết nối thật.

SQL lần cuối được kiểm chứng ngày 2026-10-06 chỉ trên
`.\SQLEXPRESS/EnglishCenter_D07_20261005`: business rows ngoài migration history 0,
test users 0, role memberships 0; 4 roles, 102 permissions, 6 migrations;
compatibility 160, collation `Latin1_General_100_CI_AS_SC`, snapshot ON,
complete catalog không đổi. Đây là trạng thái cuối đã kiểm chứng, không phải
live DB assertion ngày 2026-10-07.

Fixtures đều rollback. Owned COMMIT paths được source-review và HTTP fixture
seams kiểm chứng; chưa có persisted runtime hoặc committed stress proof.
Không tự chạy lại SQL suite cho U02 frontend hoặc dùng quyền R02/R03 để mở
schema/migrations/grants/profile/owner/login/database khác.

## Frontend hiện có và scope đề nghị kế tiếp

P03 scaffold đã apply PASS. `frontend/src/app/App.tsx` chỉ hiển thị tiêu đề
“Quản lý trung tâm tiếng Anh” và “Các chức năng hiện chưa khả dụng.”; có
`src/main.tsx`, `styles/index.css`, system fonts và Tailwind. Chưa có auth UI,
shell nghiệp vụ hoặc domain pages.

Manifest hiện pin React/react-dom 19.3.0, Vite 8.3.2, TypeScript 6.0.3,
Tailwind 4.3.3; package manager npm 11.13.0, Node >=24/npm >=11. Scripts:
`npm run typecheck`, `npm run lint`, `npm run build`. Xác minh runtime/dependencies
có sẵn trước chạy; manifest không chứng minh host đã cài đủ. Không tự cài thêm
router, UI kit, test library hoặc package khác.

**U02:** shared controls/form/shell primitives cần cho identity UI, dựa P03 và
accepted FE contract. Local accessible components, nhãn tiếng Việt, system fonts,
visible focus, reduced motion, touch target khoảng 44px. Kiểm layout 375/768/1280
và keyboard/focus/labels; build/lint/typecheck. Không tự mở signup/login pages.

**U01:** shared fetch/error/auth state và session scope boundary dựa P03/A04–A07.
JWT chỉ trong HttpOnly cookie; CSRF/context giữ RAM. Không lưu token/PII hoặc đưa
PII lên URL. Discard response thuộc session cũ/request đã supersede; clear caches,
drafts/previews/secrets khi logout/revoke/identity đổi. Không tự retry credentials,
write/replay hoặc tạo UUID mới để retry; xử lý rõ 401/403/409/unknown outcomes.
Thiết kế test seams phù hợp khi backend runtime chưa mount, không giả báo readiness.

U01/U02 task cards và worker contracts chưa có khi viết handoff. Sau hai gói này
mới đến U03 → U04 → U05 → U06 → V01; các gói đó chưa được handoff này cho phép.
Accepted requirements/E04 override stale skill guidance: JWT-cookie, full-set DTO,
money exact digit strings, no autosave/persistent PII drafts/optimistic saved,
không thêm confirmation/reason/workflow ngoài quyết định đã chốt.

## Quyền và quy trình nối tiếp

ADR-163 giữ nguyên: gói đã được owner cho phép triển khai, Codex review PASS thì
apply main và reverify, không hỏi lại quyền apply. Chỉ báo vấn đề cần owner xử lý.
Worker output chưa review hoặc rejected luôn detached.

Phê duyệt R02/R03 chỉ bao gồm hai worker cards tương ứng cùng backend skill/routing
và isolated SQL fixtures đã nêu; **không phải blanket approval cho frontend payload**.
Chat sau chuẩn bị U02/U01 cards/contracts cụ thể, ghi exact authorized read/write
sets, skill versions/hashes, canonical overrides và checks, validate trước dispatch.
Nếu routing/AGY skill yêu cầu consent cho external sharing mới, xin đúng payload
đã chuẩn bị có thể review; không hỏi lại quyết định sản phẩm đã Accepted.
Workers chỉ đọc context cho phép, viết sole outputs detached; không commands,
SQL, secrets, thêm context, merge hoặc mở scope. Codex kiểm hành vi thực tế,
independent integration/tests/review rồi mới apply PASS và cập nhật memory.

## Workspace cần bảo toàn

Khi viết handoff, không có tracked edits trước lượt này, nhưng có bốn owner files
untracked. Giữ nguyên, không tự add/commit/stash/delete/adopt làm canonical input:

- `docs/Nhom18_ThietKeXuLy.docx`
- `docs/reference/PROCESSING_REPORT_MEMORY_COMPARISON.md`
- `docs/reference/REFERENTIAL_INTEGRITY_IMPACT.md`
- `docs/reference/REPORT_REVISION_GUIDE.md`

Nếu AGY runner yêu cầu clean repository, dùng clean detached manager worktree từ
HEAD đã xác minh, chuẩn bị scoped cards/contracts tại đó. Không dọn owner files
để vượt precondition. Không sửa source Word report hoặc gửi nó ra worker.

Trees giữ làm evidence, đã thấy tồn tại ngày 2026-10-07:

- Manager: `C:/Users/asus/AppData/Local/Temp/codex-r02-r03/manager`, commit
  `1cb6ebc66a6a480d39d7e28735e5b887b0fda59f`, backend tree khớp main.
- AGY R02: `%TEMP%/codex-antigravity-worker/worktrees/main_workplace-134d7619-b9d`.
- AGY R03: `%TEMP%/codex-antigravity-worker/worktrees/main_workplace-9dc8e503-214`.
- Raw TRX/logs: `%TEMP%/codex-r02-r03`; không commit logs/thinking/secrets.

Các temp paths chỉ hỗ trợ điều tra, không là prerequisite của chat mới;
committed code và compact evidence là nguồn bền vững.

## Việc đầu tiên cho chat sau

1. Đọc đúng thứ tự, kiểm HEAD/diff/untracked và xác nhận checkpoint không bị đổi.
2. Đối chiếu frontend source, FE/auth contracts, skill routing và dependencies.
3. Khi owner giao tiếp tục, chuẩn bị bounded U02 rồi U01 cards/contracts, exact
   sharing scope và validate; giải quyết consent còn thiếu trước dispatch.
4. AGY detached → Codex integrate/verify/review → apply PASS theo ADR-163 →
   reverify main → cập nhật CURRENT_STATE/TODO và active cards.
5. Dừng đúng cuối scope owner giao. Không runtime/install/SQL/push/deploy hoặc U03
   trở đi bằng cách suy diễn từ quyền cũ.
