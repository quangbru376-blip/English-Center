# A04 — API001 CSRF and safe HTTP/cookie boundary — preparation

Second authorized family in A02→A04 batch. A04 is CSRF/HTTP boundary; Student
registration is A03, not A04. Correct stale TODO wording; no product decision change.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete skills/routing read; versions/hashes same
as D13/A01. Overrides: E04 exact API001, framework antiforgery/RAM request token,
same-origin Secure cookies/trusted explicit profile/no bearer/CORS/secret defaults.
read_context: memory/requirements/M0_QUALITY_PLAN/HANDOFF/FIRST_CODE_HANDOFF,
E04 DTO/auth/architecture/final pipeline/runtime slots and P07 error/host tests.
skill_verification: preparation only; not dispatched/implemented/PASS yet.

AGY writes only Common/Security/SameOrigin.cs from A04_ORIGIN_WORKER.md plus backend
skill/routing, one file/160lines; no commands/SQL/secrets/other reads. Codex writes
framework boundary under Common/Security and API/Auth, repository-independent
host tests and affected docs. Framework/current packages only; no JWT/login/me/
signup/logout/DB/runtime provisioning or frontend. No fake current session auth.

Explicit integration functions require configured canonical HTTPS same origin and
supplied DataProtection/key provider; no production fallback ephemeral key/port/
proxy/DB profile. GET /api/v1/auth/csrf returns only {requestToken}, anonymous or
verified identity resolver; never creates/touches/renews sessions. Missing resolver
for cookie/auth context fails closed, no stale claim accepted as verified identity.
Framework antiforgery cookie HttpOnly/Secure/SameSiteStrict/host-only; JWT cookie
option helpers same flags/host path, no body JWT/JS cookie, no new auth session.
POST/PUT and all unsafe methods require exact one Origin and framework token in
X-CSRF-TOKEN; no Referer/Host/forwarded fallback. E04 precedence retained: host/
transport, known auth boundary/session, CSRF/Origin, role/scope, shape/business.
Reject invalid token/origin403 CSRF_INVALID using current safe Vietnamese envelope;
missing unsafe profile503, invalid current session401, no unconfigured public route.
Responses no-store; no CORS, auth/CSRF cookies/credentials in logs or trace fields.

Host fixture supplies test-only HTTPS origin and ephemeral framework key provider,
not production configured secrets; integration route remains unexposed in default
host until operator key/profile and A05/A06 resolver are authorized and available.
Test framework round-trip, cookie flags, missing/duplicate/cross-origin tokens,
identity transition, invalid session/profile and safe ordering/error/no-store.
No SQL mutation required beyond explicitly approved regression scope from A02.
Owner walkthrough and independent code-review/test gates before PASS/apply.
