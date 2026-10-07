# U04 — F01 login/me/logout, session deadlines and cross-tab signals

Status Codex review PASS2026-10-07, detached; U03 PASS f77d34d before U04 run.
Base8ff239d; U01/U02 applied PASS; backend unchanged. Main apply pending batch seal.
Owner approved both exact payloads2026-10-07; Codex manager, bounded pure LoginForm.
implementation_skills: frontend-ui; review_skills: code-review.
skill_sources/read_context: full same verified hashes as U03/U02 cards;
worker exactly U04_WORKER + frontend-ui + SKILL_ROUTING, no source expansion.
applicable_overrides: exact nine-field context, cookie-only JWT/CSRF RAM;
normal/restricted authority, generic login privacy/429, no refresh/remember-me/
heartbeat/password retries/storage. No new router/query/signaling packages.
skill_verification: PASS; full3contexts before sole67-line write, own output read;
no observed extra reads/commands/violations. Runner review_required; Codex PASS.

## Exact manager scope

Worker sole features/identity/components/LoginForm.tsx. Manager permitted:
frontend/src/features/identity/{sessionController,sessionSignals}.ts,
frontend/src/features/identity/pages/{LoginPage,SessionPage}.tsx,
frontend/src/app/{IdentityApp.tsx,identityNavigation.ts},
frontend/src/lib/{auth,http,requestScope}.ts only dependency integration/fixes,
frontend/tests/{session,sessionSignals,http,auth,requestScope}.test.mjs,
frontend/tests/identity-fixture.{html,tsx}, docs/notes/U04.md,
this card/worker/contract/evidence and affected memory. No default App/main mount,
backend/package/config/schema changes. Explicit IdentityApp supplied HttpClient;
same-origin fixture proves interfaces only, not real production cookies/session.

Login email/password once, raw password/no automatic retry; generic401,429
Retry-After, explicit CSRF recovery, unknown deliberately probe me/login again.
Success/new-session boundary clears old RAM even same account, fetch fresh CSRF
without resending credentials. Load/me non-renewing; stale responses/401 cannot
publish or clear newer identity. No PII URLs, JWT/SID parsing or durable storage.
Normal landing maps accepted first F by fixed role; unimplemented business content
clearly unavailable, no fake menus/actions. Restricted goes password-change gate
with logout only; U05 form unstarted, no role upgrade or business fetch.
Logout204 or current401 clears local RAM;403 does not claim server success;
unknown clears sensitive local content and reports unconfirmed revocation.
Explicit cleanup registration clears forms/previews/secrets on each boundary.

Cross-tab: native BroadcastChannel, a fixed nonsecret identity-change signal only;
never transmit context/account/token/PII/credentials/deadlines. Receive clears RAM
and revalidates via me before render; never echo/rebroadcast incoming signal.
Close listeners/channel on dispose. Unsupported channel uses focus/visibility
revalidation without storage fallback. Server owns session status/activity/deadlines.
Timers use serverTime/effectiveExpiresAt/absoluteExpiresAt and monotonic elapsed
time; at boundary clear protected RAM and check me without idle activity, no refresh,
heartbeat, auto-logout POST or claim client clock proves server expiry. Hidden tabs
revalidate before showing protected content. Inconsistent/unknown proof fails closed.
No mouse/typing listener renews session; no business activity endpoint invented.

## Checks

Node tests: generic401/429/CSRF/unknown, exactly one credential/logout send,
same-account/new-session and stale responses, deadlines/equality/dispose, no idle
renewal, signal payload/malformed/echo/fallback and cleanup after callback throw.
Browser3widths/password spaces/pending/focus/signup→login/no auto-login/normal and
restricted gates/logout401/403/unknown and two same-profile tabs via fake auth API.
Real other-device/production runtime/revocation SQL evidence explicitly unverified.
Existing deps only; npm types/lint/build; notes<=15; Codex code-review PASS before
ADR163 apply/main reverify. Preserve four owner files. No U05/U06/V01/restM0,
runtime/SQL/profile/principals/packages/push/deploy authority.

## Independent review result

38frontend tests/0fail0skip, lint/typecheck/build PASS; Edge375/768/1280 PASS.
Raw/single credential send, generic401/429, explicit CSRF recovery, signup201→
login/no auto-login, keyboard, restricted/normal landing, logout401/403/unknown,
two-tab native channel/no echo/no storage verified. Unit injected clocks prove
deadline/equality/late proof/dispose; SQL/live cookies/other-device readiness N/A.
Codex fixed effect lifecycle/client-owner guard, deadline equality and dispose RAM;
no default App mount or backend/API activation. Notes12lines; scope/security/docs
review PASS with these fixes verified. Retain original worker tree and receipts.
