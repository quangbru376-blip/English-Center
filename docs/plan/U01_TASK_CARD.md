# U01 — shared HTTP/errors/auth RAM and session boundary

Status: Codex review PASS2026-10-07 after U02 PASS48adaae. Owner continuation authorized exact payload; base87d1476. Main apply/reverify follows ADR163.
P03/A04–A07 PASS; no backend/runtime activation or SQL test run authorized.

implementation_skills: frontend-ui. review_skills: code-review.
skill_sources: exact same full files/SHA256 as U02_TASK_CARD; selected and read
before preparation. Manager uses antigravity-worker protocol/reference contract.
applicable_overrides: cookie JWT only, CSRF/context RAM, exact E04 envelope/DTO,
no PII URL/storage/logs, money strings/full-set DTO preserved, no automatic writes/
credentials/UUID retry, no optimistic saved/idle heartbeat. Backend remains authority.
read_context: exactly U01_WORKER + frontend skill + SKILL_ROUTING as contract;
manager same canonical context as U02, plus existing Auth DTO/endpoints as needed.
Worker never reads backend, owner files, U02 output or unrelated documentation.

## Bounded implementation

Worker sole requestScope.ts output; manager permitted files:
`frontend/src/lib/{requestScope,http,apiError,auth}.ts`,
`frontend/tests/{requestScope,http,auth}.test.mjs`, `docs/notes/U01.md`,
this card/evidence. No mounted app provider, identity/domain page or routing.
No cross-tab transport/timers/login UI (U04). Register explicit callbacks now;
future callers must clear draft/preview/secret through them.

HTTP: injectable fetch seam; fixed relative /api/v1 paths, same-origin credentials,
cache=no-store, redirect=error; JSON typed through caller decoder, strict safe error
envelope, no payload logging/storage. GET/POST only (exact existing auth routes) with explicit request intent;
no generic unrestricted URLs or PII query helper. Body passed once, never retained
for automatic replay. Unsafe methods require current RAM CSRF. No custom Origin
spoofing, bearer token, CORS, service worker or hidden network side effects.
Request tickets discard old/superseded completions even if abort ignored. Writes
with lost response/cancel/unparseable success report UNKNOWN, never fake rollback.
Do not invent timeout budgets; support caller AbortSignal cancellation explicitly.

Auth state: exact nine-field context accountId/fullName/email/role/scope/
accountVersion/serverTime/absoluteExpiresAt/effectiveExpiresAt; NORMAL or
PASSWORD_CHANGE. New identity/session boundary invalidates tickets/registered RAM
and CSRF before publishing context; no JWT/raw SID or claiming same-account login
is the same session. /me is read-only, identity changes invalidate context; no idle
renewal. 401 AUTHENTICATION_REQUIRED clears sensitive RAM; login's generic401 is
separate.403 FORBIDDEN versus CSRF_INVALID: deliberate context/token recovery,
never write retry.409 stays visible for reload/review;404 safe,429 Retry-After,
network write unknown. Explicit anonymous CSRF bootstrap and supplied context
seams only; no auto-login, session upgrade, UUID storage/generation or mutation
result lookup implementation. Clear credential body references after completion.

## Checks and exclusions

Codex independently tests error classification, raw password preservation,
one-send credentials/writes, CSRF absence/invalid recovery, stale/out-of-order
responses, stale401 cannot clear newer identity, same-account new-session epoch,
all cleanup on logout/identity/revoke including callback throw, restricted business
guard, malformed context/envelope, unknown response, cancellation, safe endpoint
allowlist and no storage/log/token/header leaks. Built-in Node tests via existing
TypeScript compile-to-temp only; no new dependency/config package changes.
Typecheck/lint/build; backend tree must remain exact checkpoint. No SQL/full suite
needed for frontend-only scope. No persisted runtime/real credentials/readiness
claim. Code-review/notes<=15 lines and main reverify required before done/next task.
skill_verification: PASS; see worker-review.json and independent checks below. Apply PASS per ADR163;
rejected worker output never applied. U03/rest M0/runtime/SQL/install/push/deploy closed.

Codex review2026-10-07: AGY94d4426f-e39 SUCCESS, exact3 contexts fully before
sole107-line output; no observed extra reads/commands/scope violations. Original retained.
Codex uses runtime-private fields and independently implements HTTP/error/auth seams.
/me probes cookie authority even when initial RAM context is empty. Same-account new
login explicitly invalidates epoch; changed authority clears RAM before publishing.
Only fixed existing auth endpoints; POST change-password exact backend path/header.
Lost/malformed auth writes clear still-current local authority and report UNKNOWN;
stale lost writes cannot clear newer identity. No claim of server rollback/logout.
24 final Node tests PASS (6U02+18U01); typecheck/lint/build exit0; U02 browser PASS.
Source review: no durable storage/logging/JWT/cookie reads/PII URLs/retries/timers;
raw credentials once, no retained request cache. Client guard is advisory; server owns rights.
Backend/API/schema/migration checks N/A: exact backend tree unchanged, no runtime routes.
Notes U01 <=15 lines; docs/api and memory updated on apply. Verdict PASS.
Unverified: real auth runtime, production cookies/keys/profile, integrated business UI,
cross-tab signaling/timers, SQL and committed stress; these remain outside scope.

Applied/reverified ADR1632026-10-07 code mainfb748e7, manager48ff8a0;
identical frontendde38145e6127d32cfcf20629387b18415e2c5746.
Main24tests/types/lint/build PASS; final Edge375/768/1280/44px/contrast PASS.
Backend unchanged;4owner files exact hashes/untracked; no SQL/runtime/U03 opened.
See U01-evidence/verification-summary.json for final verification and test recipe.
