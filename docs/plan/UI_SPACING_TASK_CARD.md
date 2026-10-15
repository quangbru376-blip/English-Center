# UI spacing pass — 2026-10-15

Owner assigned one pass over frontend placement/spacing after finding controls too
close together. Scope: shared presentation CSS, modal body, calendar, nested staff
detail/recovery groups, pagination and component form spacing. No business/API/auth
handler, field/selector, dependency, database or protected Word/account change.
Existing dirty demo/operator/memory/owner files are preserved. No delegation.

implementation_skills: frontend-ui; review_skills: code-review.
skill_sources (SHA256):
- .agents/skills/frontend-ui/SKILL.md: 448079D6EF3B5B514BBCBA4F2A8D5AAF0FC6CC8063F79798F28A6DBBE5128F90
- .agents/skills/code-review/SKILL.md: 04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70
- docs/plan/SKILL_ROUTING.md: ADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915

read_context: AGENTS + five memory files in order, requirements (especially
4.6–4.10), UI_REFRESH_PLAN, applicable frontend styles/components/pages and
existing browser fixtures. applicable_overrides: existing native/client/cookie/
CSRF/RAM/epochs/manual lookup/exact-money contracts; token spacing/44px controls;
no added libraries, automatic writes, reset or owner-session revocation.

Plan: inspect missing internal flow/label/action spacing; capture before with
synthetic dev views and real shared form/calendar components; apply small semantic
layout classes; check all seven role destinations and auth at 375/768/1280,
loading/empty/error, wrapped actions, dialog Escape/focus, no page overflow; run
existing frontend tests/typecheck/lint and isolated production build; review diff.
Browser mock evidence is presentation evidence, not SQL/auth/load acceptance.
skill_verification/review/results: COMPLETE / Codex review PASS. Production scope
is19frontend files (16TSX listed in ast-parity.json and components/student/workbench
CSS).183existing tests, types/lint/build, 66final DEV screenshots, existing
styleguide +staff/Student StrictMode checks PASS. Live demo new32-file UI release
verified by SHA256 and anonymous trusted Edge at375/768/1280; ready200.
Launcher retained session83377. No business mutations/reset/session revocation;
original UI retained at .local-demo/ui-spacing-previous-dist. Prior running.json
was stale and a PID had been reused by another application: identity guard refused
replacement until exact owned-host absence was established; no unrelated process
was stopped. No fresh controlled-stop claim based on the historical stopped.json.
Evidence: ui-spacing-evidence/; review UI_SPACING_REVIEW.md.
