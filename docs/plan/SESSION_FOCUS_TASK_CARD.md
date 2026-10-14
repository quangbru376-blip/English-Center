# Session focus UX — 2026-10-14

Owner “Ok hãy thử đi” accepts removing focus-only revalidation and coalescing
repeated lifecycle events. Scope IdentityApp.tsx only: no window focus listener;
visibility state transitions hide→pause/RAM clear and visible→fresh me/CSRF;
same-state duplicate events ignored. Friendly checking text. Startup, deadline,
BroadcastChannel identity signals, Back/Forward/manual recovery, sessionController,
auth/client/request epochs/401 handling/CSRF/cookies/backend authority unchanged.
No retained hidden sensitive data, polling/renewal/retry/store/package changes.
implementation_skills frontend-ui; review_skills code-review, already read;
SKILL_ROUTING overrides retain RAM/cookies/CSRF/native/current client/authority.
Relevant requirements ADR097–099; owner narrows only window focus trigger.
Validation: all frontend/types/lint/build; new actual DEV StrictMode browser regression
for focus bursts/form retained, same-visible duplicates, hidden clear, returning
visible one fresh me while delayed, identity signal and401 revocation. Mock is
not real authentication proof. Publish local dist with owned launcher stopped,
restart same identities/profile/unchanged artifact pins, trusted health and public
login browser check; preserve live user's cookie/session/data. Apply ADR163 after
review PASS and update docs/memory. No full backend/SQL rerun claim for untouched
backend. Protect owner4/runbook/guide and prior UI evidence.

Runtime prerequisite finding: launcher probes health immediately while bounded5s
startup readiness is evaluating; interactive run reportedready then runtime exited.
Additional allowlist tools/operator/Start-M0Local.ps1: pace first health probe6s
after runtime start, original20s launch deadline/fresh gates/profile unchanged.
Validate ReadOnlyCheck/stop503 then actual live health and anonymous Edge check.
No backend probe changes/TLS bypass/retry acceptance. Retain prior failed receipts.
