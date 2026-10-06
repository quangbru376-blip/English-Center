# Implemented API exposure

2026-10-05 A04: API-001 GET /api/v1/auth/csrf is available as explicit MapCsrfContext
integration only; unchanged default Program does not expose it. No other business
route is activated by A02/A04. Accepted full contracts remain in E04_DTO_SPEC.md.

An integrated host must supply explicit canonical HTTPS origin, framework key provider,
verified identity resolver and UseCsrfBoundary after routing. Missing boundary fails503.
Request has no body or query. Response200 is exactly {requestToken:string}; no-store.
Anonymous or valid current identity is allowed; an unverified auth cookie fails401.
Invalid Origin/token gives403 CSRF_INVALID; bad shape400 VALIDATION_ERROR;
unconfigured transport/profile503 SERVICE_UNAVAILABLE, all via the safe envelope.
Unsafe protected routes require CsrfRoutePolicy, exact single Origin and header token.
Unclassified API routes fail closed. Real auth/session/profile integration is deferred.

2026-10-06 A03/A05: explicit MapStudentSignup and MapLogin integration functions;
default Program remains unchanged and exposes none of these auth/business routes.
Integration order: routing → P07 safe errors → AuthJsonBoundary (supplied byte limit)
→ framework CsrfBoundary → adapters. Supply verified identity, HTTPS origin/keys,
operation executors; login also requires signer/deadlines/trusted IP profile. No defaults.
POST /api/v1/auth/register (API018) is anonymous-only: exactly fullName,dateOfBirth,
email,phone,password required, address optional.201 returns only outcome CREATED/message,
no cookie/ID/Location. Invalid fields400, normalized email/phone conflict409 with safe
field error, missing Student role503, failed mandatory audit500 and whole TX rollback.
POST /api/v1/auth/login (API002) accepts only email/password after Origin/CSRF. Current
normal identity or anonymous allowed; restricted identity rejected by A04 normal-scope
policy. Generic401 covers unknown/wrong/status/temp/account throttle; IP101 in fixed
one-minute framework window429 plus Retry-After, independent of5/15/15 account throttle.
200 contains exactly accountId/fullName/email/role/scope/accountVersion/serverTime/
absoluteExpiresAt/effectiveExpiresAt. Supplied signed JWT only in __Host-ec-auth cookie,
Secure/HttpOnly/Strict/Path=/ and bounded Expires; no-store, no token/sid in JSON.
Missing body/CSRF/trusted-IP profile fails closed503. Rejected counter changes and
accepted sessions commit before adapter output; cancellation/exception rolls back.
NumericDate floor may reject subsecond temporary lifetime; no deadline extension.
No API003/me, API004/password change, API005/logout or runtime route activation yet.
