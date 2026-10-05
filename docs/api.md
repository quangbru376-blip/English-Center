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
