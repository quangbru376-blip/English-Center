# UI refresh stage3 — Student pilot

2026-10-13: owner “tiếp tục đi” accepts gate2 and assigns stage3 under
UI_REFRESH_PLAN. Branch ui-refresh; stage2 reviewed source manifest preserved.
implementation_skills: frontend-ui; review_skills: code-review.
skill_sources: .agents/skills/frontend-ui/SKILL.md, code-review/SKILL.md;
docs/plan/SKILL_ROUTING.md. Both fully read; hashes recorded in evidence.
applicable_overrides: existing client/cookie/CSRF/RAM/epochs, exact digit money,
44px controls, current routes/DTO/validation/replay; no React Query/package addition.
read_context: AGENTS, PROJECT_CONTEXT, ARCHITECTURE, CURRENT_STATE, DECISIONS,
TODO, requirements (Student/FE states/security), UI_REFRESH_PLAN/INVENTORY,
StudentEnrollmentPage/enrollmentApi, useOwnedTask, IdentityApp, current components,
browser groups85/87/88 and enrollment tests.
skill_verification: implementation and review by Codex, no worker/delegation.

Outcome: scoped initial reads on Student views; Home per-registration cards with
server current step/next action/event/unpaid invoice/published result; course cards;
own registration/attempt/Band and booking selection/confirmation/recovery states.
No invented totals/progress, no mutation from effect, no implicit slot selection,
no polling/idle renewal/storage, no stale query shown as newly loaded after error.
Preserve literal selectors/labels and existing callbacks/atomic save units. Profile
and calendar initial reads use existing contracts; complete visual overhaul of these,
Invoice/classes/published result pages and internal roles stays stages4–5.

Allowlist: enrollment page and new presentation components/style/test helpers;
IdentityApp navigation/unsaved guard and existing Student detail links;
styles/index and new student.css; frontend enrollment/UI tests and browser adapter;
task docs/notes/evidence/memory. No dependency/backend/lib/API parser changes,
SQL schema/grants/runtime/custody/owner files/source report/runbook edits.

Checks: build/types/lint/all frontend tests; relevant real Edge owned-SQL groups
85/87/88/94 (create/book/lostACK/manual lookup/reschedule/cancel/confirm/result),
Student focused loading/empty/error/initial-read/no-retry/dirty/keyboard evidence
375/768/1280; source/selector review and before/after screenshots; ID/GUID cleanup.
PASS requires all assigned checks; gate3 owner reviews pilot before stage4.
