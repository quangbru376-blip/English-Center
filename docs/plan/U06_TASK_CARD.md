# U06 — minimal Admin detail and lock pilot

PREPARED2026-10-07; owner continuation U05→U06 from main e48b604; U05 PASS first.
Codex manager; bounded antigravity-worker sole pure AdminAccountPanel150lines.
Exact new sharing gate pending; ADR163 apply PASS standing authority.
implementation_skills: frontend-ui; review_skills: code-review.
skill_sources/read_context: same verified full sources as U05; E04 API007/010/014,
R03 actual DTO/endpoints/replay constants; worker own WORKER+frontend-ui+SKILL_ROUTING.
applicable_overrides: fixed Admin NORMAL guard, cookie/CSRF RAM/exact DTO/error,
only lock command, no optimistic status/write retry/poll/storage/extra packages.
skill_verification: pending tool events/independent integration/tests/review.

Manager exact scope: frontend/src/features/administration/{types,adminApi,lockIntent}.ts,
components/AdminAccountPanel.tsx, pages/AdminAccountPage.tsx;
frontend/src/app/{IdentityApp.tsx,identityNavigation.ts}, features/identity/pages/SessionPage.tsx,
frontend/src/lib/http.ts fixed Admin detail/lock/lock-result descriptors and validated
target/request/header (no arbitrary URLs/query/headers); frontend/tests/{admin,http}.test.mjs,
identity-fixture.{html,tsx}; docs/notes/U06.md, card/worker/evidence and affected memory.
No default App/main/backend/package/config changes. No search/create/reset/unlock/
disable/restore/audit screen. Explicit ID entry and direct detail non-PII path only.
Fresh exact12field AccountAdminView; capabilities advisory, server guards authoritative.
Native shared Modal confirms target+revoke-all/no data deletion; frozen ID/RV/one
new UUID per deliberate confirmed intent, header Idempotency-Key, exact body.
No automatic replay. Unknown retains only safe request/target reference in RAM;
user manually checks exact ADMIN.ACCOUNT.LOCK result and reloads target.404 marker
means RESULT_NOT_CONFIRMED, not rollback;200 safe reference not historical snapshot.
409 closes stale intent/requires reload; no silent merge/newkey resend. Actual200
CommandResult validates operationId/account, current authorized data not optimistic.
Identity boundary/unmount clears previews/IDs/intent/errors; stale response ignored.
Check all roles/restricted/no dispatch, exact paths/body/UUID/RV,400/401/403/404/409/
500/lost/pending/stale, read-only lookup/no polling, three widths/dialog keyboard.
Notes<=15lines; Codex PASS/ADR163 apply/main reverify. No SQL/runtime/packages/V01.

Checkpoint preparation: manager detached df0f86b at
C:/Users/asus/AppData/Local/Temp/codex-u05-u06/manager; manager-only WIP primitives
prepared,29subset tests PASS (5password+5Admin+7session+12HTTP), no worker run.
Full typecheck/lint/build/browser and final review pending worker views.
External exact-payload approval requested; do not treat elapsed time as approval.
Main code remains e48b604; no source applied. Four owner files untouched.
