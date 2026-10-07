# Handoff cho chat tiếp theo — 2026-10-07

## Checkpoint hiện tại

Owner đã giao tiếp tục U05→U06 ngày2026-10-07. Hai cards/worker contracts mới
validate PASS; exact external consent đang chờ trả lời, chưa dispatch AGY.
Manager WIP2397b85 tại C:/Users/asus/AppData/Local/Temp/codex-u05-u06/manager;
29subset tests PASS cho manager primitives. Chưa full types/lint/build/browser,
chưa review PASS/apply. Tiếp tục từ câu hỏi cho phép hai payload; không suy thời
gian chờ là approval. U05 PASS trước U06 worker. Main code chưa đổi bên dưới.

U03→U04 hoàn tất/review PASS/apply ADR163/reverify main. Code main
`d51564e1a19bcef22e61bfd778ad359b42ae30e0`, manager
`d18a34a2f74a901c886d2f662df68a9573737f05`; frontend giống nhau
`851877ef7396885e51d39518697659277fa0effc`. Backend giữ nguyên
`10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a`.38frontend tests/0fail0skip,
typecheck/lint/build và Edge375/768/1280 PASS trên manager/main. SQL không chạy lại.
Workspace C:/Study/SOEN/main_workplace, PowerShell, branch main; chỉ bốn file owner
untracked bên dưới. Commit seal tài liệu có thể nằm sau code checkpoint này.

U03: native signup/raw credentials/exact API018 DTO và CREATED/message201;
xóa draft rồi tới login, không tự login.400/409/CSRF/500/lost/stale và cleanup PASS.
U04: exported IdentityApp nhận một HttpClient, login/me/logout, first-F role/gate
PASSWORD_CHANGE, timer theo khoảng server chỉ đọc me, hidden/focus revalidation.
BroadcastChannel chỉ IDENTITY_CHANGED, không account/PII/token/deadline hoặc echo.
Logout403 không báo thành công; unknown xóa RAM/báo chưa xác nhận server.
Default App/main vẫn unavailable; backend API không mount; fake API browser fixtures
không chứng minh production HTTPS/cookie/profile/keys/logins/live revocation.
U05 form đổi mật khẩu, U06 Admin UI, V01/restM0 chưa triển khai/mở scope.

## Thứ tự đọc

1. AGENTS.md.
2. docs/PROJECT_CONTEXT.md.
3. docs/ARCHITECTURE.md.
4. docs/CURRENT_STATE.md.
5. docs/DECISIONS.md.
6. docs/TODO.md.
7. docs/requirements.md, file này, docs/plan/HANDOFF.md.
8. docs/plan/implementation-plan.md, M0_QUALITY_PLAN.md, SKILL_ROUTING.md;
   E04_FINAL_CONTRACT/E04_DTO_SPEC auth/frontend tương ứng.
9. Toàn bộ skills frontend-ui/code-review/antigravity-worker và contract reference
   trước code/delegation. Không tự mở codex-with-chatgpt hoặc đọc instructions inbox.

## Quyền và điểm tiếp tục

Owner giao hai gói tiếp theo U03→U04 và đã phê duyệt hai payload AGY exact.
Mỗi lượt chỉ own WORKER.md + frontend-ui SKILL.md + SKILL_ROUTING.md, sole pure form;
không source/report/owner/secrets/commands/network/SQL. Quyền đó đã dùng xong,
không tự áp sang payload external khác. ADR163 vẫn cho apply reviewed PASS stage
được giao mà không hỏi lại. Codex là manager; antigravity-worker là bounded worker.
Next U05→U06 cần owner giao scope; chuẩn bị card/contract/phạm vi chia sẻ cụ thể
trước dispatch, chỉ bổ sung consent nếu phạm vi mới chưa có quyền. Không hỏi lại
quyết định product đã chốt hoặc quyền apply PASS. Không runtime/SQL/packages/push/
deploy/profile/principals/keys/backend mount từ quyền frontend vừa hoàn tất.

## Evidence và worktrees giữ lại

- docs/plan/U03_TASK_CARD.md và U04_TASK_CARD.md; docs/notes/U03.md, U04.md.
- docs/plan/U03-evidence và U04-evidence: form-contract, browser-check,
  verification-summary (receipt/event SHA256, skills, owner hashes, giới hạn proof).
- Manager C:/Users/asus/AppData/Local/Temp/codex-u03-u04/manager, detached d18a34a.
  U03 PASS f77d34d trước dispatch U04. Giữ original worker trees, không xóa.
- AGY c1486cf8-494 (SignupForm117lines) và eadd353c-600 (LoginForm67lines),
  SUCCESS/exit0/full3contexts trước write/zero observed violations; không phải
  OS-enforced read sandbox hoặc chứng minh tuyệt đối nội dung được hiểu.
- Worker trees C:/Users/asus/AppData/Local/Temp/codex-antigravity-worker/worktrees/
  manager-c1486cf8-494 và manager-eadd353c-600; runs/<runId>/receipt/events.
- Native default fetch receiver bug đã được Codex sửa bằng lexical wrapper và
  browser regression thật. Test helpers không cài packages. Verify từ frontend:
  ../docs/plan/U04-evidence/verify.ps1 -BuildDirectory <absolute temporary path>.
  Browser env EC_PLAYWRIGHT trỏ bundled playwright, EC_UI_URL loopback fixture;
  cả hai browser scripts bắt mọi API bằng dữ liệu tổng hợp. Server fixture đã dừng.

## Bốn file owner phải giữ nguyên

Không stage/stash/delete/adopt/share/đưa vào worktree worker. SHA256 đã xác minh:

| File | SHA256 |
| --- | --- |
| docs/Nhom18_ThietKeXuLy.docx | C7DE4DE6338670A94C99280DF89BDDC8E06FAE53A63E3B0405BBFE9DA0149F6C |
| docs/reference/PROCESSING_REPORT_MEMORY_COMPARISON.md | 4EFDC55AD820A5E92A9A95A619F875D6191EA74BDCC05AC06BF8CDE1AB65A54C |
| docs/reference/REFERENTIAL_INTEGRITY_IMPACT.md | 40466A2B841CF7E5DCC796ACDCC122B20E649B3A9DEA9814C1BECCD733E8CFB0 |
| docs/reference/REPORT_REVISION_GUIDE.md | D5EA14D09082EB776BE243EABC7E4CD8D375CC1586214417FB3542AB2A138FEA |

Historical FIRST_CODE_HANDOFF và các checkpoint cũ trong HANDOFF không đổi current
scope. Không lặp tests/SQL cũ để suy ra readiness; chưa có real operator production proof.
