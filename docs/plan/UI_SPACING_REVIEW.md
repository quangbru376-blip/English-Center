# UI spacing review — 2026-10-15

Owner assigned a frontend placement/spacing pass. Reviewed against
UI_SPACING_TASK_CARD, UI_REFRESH_PLAN, requirements FE03–FE19 and local skills.

## Changes

- 8px label/control gap for nested native inputs/selects, 12px action gaps,
  16px detail/modal flow and 24px page-section spacing using existing tokens.
- Explicit wrapping action groups replace adjacent standalone buttons across
  staff pagination, details, recovery and confirmation forms.
- Shared modal separates header/body; descriptions and confirmation buttons
  no longer touch. Original dialog handlers and focus behavior are preserved.
- Calendar event cards, narrow weekly cells, attendance/grade forms, Student
  invoice/result summaries and filter rows have consistent internal spacing.
- Remove duplicated action/filter margins; reduce outer mobile card padding to
  preserve usable width while increasing space between controls.

## Verified

- 183 existing frontend tests PASS, no failures/skips; typecheck, lint and
  isolated production build PASS. Dependencies/backend untouched by this pass.
- Installed Edge: 13 staff destinations at 375/768/1280 (39 landing cases),
  Student rich Home/course fixtures, shared actual forms/calendar/modal,
  loading/empty/error and anonymous login/signup. 57 before/66 final after
  screenshots; no page overflow/uncaught JavaScript errors/unexpected API calls.
  Visible nested select labels and modal paragraph/action gaps measured.
  Dev-only weekly-view toggle was added after baseline for additional coverage.
- Existing styleguide keyboard tabs, dialog Escape/opener focus and reduced
  motion checks PASS. Existing StrictMode staff13 destinations and Student6
  read families PASS, one scoped initial read per endpoint and no mutations.
- AST comparison of all16 changed TSX files against HEAD: logic/text unchanged
  after excluding className attributes and div presentation wrappers; all other
  JSX attributes independently unchanged. Three stylesheets contain the styling.
- Synthetic browser transport is not SQL/authentication/load acceptance. This
  pass does not rerun every populated business detail/command combination or
  certify full WCAG. NV04, owner UIgate4 and M4 recovery stay separate.

Review verdict: **PASS**. Live demo packaging also PASS:32release file hashes
match; actual anonymous Edge at375/768/1280 loads the reviewed CSS through normal
trusted HTTPS; no page overflow/JavaScript errors/API mutations. Readiness200ready.
Managed demo launcher session83377 retained at https://localhost:8444. Mutable
data, existing custody/profile/package pins and owner sessions were preserved;
original UI retained in .local-demo/ui-spacing-previous-dist. No SQL/backend suite
rerun, reset, password operation or session revocation in this pass.

Prior running.json was stale; one recorded PID belonged to a newer unrelated
application. The initial PID-only guard refused replacement. Exact host identity
and absent listener were then checked; no unrelated process was stopped. The old
stopped.json predates this task and is not fresh controlled-stop evidence. Initial
Node HTTP request-context trust differed from Windows browser trust; final asset
hash fetch runs through trusted same-origin Edge, with no TLS bypass/raw headers.

Initial sandbox Edge launch/loopback isolation failures were environmental;
final browser ran against an independent loopback Vite5179. A measurement first
included closed-dialog fields; final measurement filters invisible labels. AST
comparison normalizes CRLF and ignores empty presentation-only attribute nodes.
No product guard or business assertion was relaxed.
