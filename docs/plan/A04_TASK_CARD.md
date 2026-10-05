# A04 — API001 CSRF and safe HTTP/cookie boundary — authorized scope applied/reverified PASS

Second authorized family in A02→A04 batch. A04 is CSRF/HTTP boundary; Student
registration is A03, not A04. Correct stale TODO wording; no product decision change.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete skills/routing read; versions/hashes same
as D13/A01. Overrides: E04 exact API001, framework antiforgery/RAM request token,
same-origin Secure cookies/trusted explicit profile/no bearer/CORS/secret defaults.
read_context: memory/requirements/M0_QUALITY_PLAN/HANDOFF/FIRST_CODE_HANDOFF,
E04 DTO/auth/architecture/final pipeline/runtime slots and P07 error/host tests.
skill_verification: complete source review and independent gates PASS; see verified result below.

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

## Verified result — 2026-10-05

Owner explicitly approved exact A02→A04 payload and rollback-only SQL scope this turn.
No reauthorization inferred from worker claims. ADR163 permits apply after PASS.
Selected complete skill byte SHA256 in detached checkout (Git LF representation):
- backend-clean-code: `76acec868ed4d918dc1c6616904f78ff2710737d6a06ccf605da783bc914dd23`
- code-review: `1dded1e6699f97c7657c27d2b923992d07ac88bd9642fb74ebe4d93a28be2125`
- routing: `adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915`
- antigravity-worker: `eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b`

Worker 35fc2b27-3c9: SUCCESS/exit0, one authorized helper; complete own card/backend/routing
reads before write, zero observed scope violations. Original worker/manager trees retained.
Manager scope, accepted E03/E04 overrides and source review: PASS, no unresolved findings.
Detached full gates:545unit+375SQL=920PASS/0fail0skip; build0warnings0errors; cache-only
locked restore. SQL exclusively EnglishCenter_D07_20261005; fixtures rollback.
No dependency/mapping/migration/SQL-resource/Program/profile change. No real Admin/runtime
provisioning, other DB/owner/login/persistent grants/keys, push or deployment.

API001 is an explicit integration function, unexposed by unchanged default Program.
Host tests mount it with test-only HTTPS origin, ephemeral framework keys and verified
identity resolver fixture. Production key/profile/resolver selection and A05/A06 session
verification remain prerequisites; fixture headers are not production authentication.
Framework antiforgery binds request token to current identity and host-only secure
cookie. Exact single Origin/header token on every unsafe method; form token cannot
replace header, no Host/Referer/forwarded fallback. HTTP/profile/unclassified route
fail503; invalid current identity401 before CSRF; restricted scope403 after CSRF.
Manager fixes: deny unclassified API metadata and unverified principal; require private
boundary marker before API001 token issuance; reject body/query before minting. Token
endpoint uses framework body-detection feature (including chunked body), JSON only
requestToken, explicit AllowAnonymous/current-identity metadata, response metadata and
no-store. No idle/session writes/JWT issuance. Cookie helper supplies only canonical
__Host-ec-auth flags; A05 still owns session expiry and issuance. Origin tests14 + host
boundary17 cases; fresh token required on anonymous→verified identity transition.
Owner walkthrough: docs/notes/A04.md. docs/api.md records implementation exposure.

## Main application and re-verification

Reviewed PASS applied under standing owner ADR163: A04 maina8ad1d5; final code
maina8ad1d5, managerd1612ed, identical backend tree8a7e73f1bf9114311c9484f5c7c16d639ebd5916.
A02 stage main514unit+375SQL PASS; final main545unit+375SQL=920PASS/0fail0skip;
build0warnings0errors. Final exact post-catalog/columns2checks PASS, retained0business
rows/0test users/0ec-role memberships;4roles/6migrations/160/collation/snapshotON unchanged.
No actual Admin/runtime API activation claim. Next A03/A05 unstarted.
