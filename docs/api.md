2026-10-10 packages7–8 COMPLETE/review PASS/applied main ada4a7c/reverified.
Admissions NORMAL API038–044: POST /api/v1/admissions/registrations/search;
GET /{id}; POST /{id}/approval-preview, /approve, /reject,
/cancellation-preview, /cancel. PII body-only; exact current contact/detail DTOs.
Shared API045 /api/v1/admissions/command-results/{requestId}?action= dispatches
Admissions three Registration families or AcademicAffairs six Session families,
never union rights. All three commands return current Registration/operationId.
Approve atomically issues SP07 Invoice/current positive tuition/due VN+7;
reject/cancel require normalized reason/full manifests/audit/T23. No Payment API.
Student NORMAL API046–047 GET /api/v1/students/me/invoices and /{id} own scope,
current names/Course plus immutable snapshot amount; actual FN06 debt flags.
Native list/detail/A4 fresh-print; failed GET never prints cached data.
Explicit existing LOCAL_DEV only; no new default routes/packages/schema/grants.
Manager160M1/14Edge+final88 and finaldate PASS; main1345default/93frontend/
20SQLHTTP+2finaldate/3Edge+genuineLOCALDEV PASS. Whole M1 COMPLETE; see M1_07_08_REVIEW.

2026-10-10 packages5–6 reviewed PASS/applied main e92ef6b/reverified LOCAL_DEV:
Student NORMAL own API019–037 adapters under /api/v1/students/me implement
GET/PUT profile; GET home/courses/courses/{id}/registrations/registrations/{id};
POST registrations; POST registrations/{id}/cancellation-preview or /cancel;
GET registrations/{id}/entry-test-options; POST registrations/{id}/entry-test-bookings;
POST registrations/{id}/entry-test-bookings/{booking}/cancel or /reschedule;
GET entry-test-bookings and registrations/{id}/entry-results;
POST registrations/{id}/confirmation-preview or /confirm;
GET command-results/{requestId}?action= one of the six Student command families.
API029 optional bookingId reads targets for an own current SCHEDULED source;
OPEN/future/pre-confirmation/source and other-overlap guards remain unchanged.
No query preserves book/rebook context with literal-null currentBookingId. The
bounded technical refinement completes accepted W07 under owner completion scope;
API030/C1/command actions/count154 remain unchanged;5–6 review/apply/reverification PASS.
Teacher NORMAL current assignment API066–073 adapters under
/api/v1/admissions/teachers/me implement GET entry-test-sessions, GET {id},
POST entry-test-sessions/{id}/roster/search, GET entry-test-bookings/{id},
POST entry-test-bookings/{id}/score or /absence,
PUT entry-test-attempts/{id}/score, GET command-results/{requestId}?action= one
of the three Teacher command families. Roster excludes financial fields entirely.
Only explicit LOCAL_DEV manager mount; unchanged default no-argument host.
Cookie/CSRF, own/current assignment, two Profile RVs, full command manifests,
mandatory audit/T23 atomicity and manual unknown-result lookup remain required.
Profile PUT has no UUID/T23/securityVersion increment/session revocation; preview
and roster POST are read intents, no UUID or activity renewal. Owned writes finish
before output, first registration/booking201 and replay200, no automatic resend.
Historical main exposure checkpoints below remain the applied product baseline.

2026-10-09 M1 packages3–4: explicit LOCAL_DEV adds AcademicAffairs NORMAL
TrainingProgram API081–089 and six PROGRAM completion actions via API090.
GET/POST /api/v1/academics/training-programs; GET {id}; PUT {id}/configuration
or /name; POST {id}/activation-preview,activate,return-to-draft,deactivate.
Full source component IDs/versions and target Course/Band versions; first201,
replay200; incomplete DRAFT allowed, saved-only activation/full frozen manifests.
AcademicAffairs entry sessions API054–065: GET/POST admissions/entry-test-sessions;
GET {id}; PUT {id}/schedule; POST {id}/open,close,cancellation-preview,cancel,
grader-change-preview,change-grader,roster/search. GET admissions/entry-test-grader-options;
GET admissions/command-results/{requestId}?action= six ENTRY_SESSION actions only.
API090 retains four Course actions; API045 exposes no booking/registration actions.
Previews and roster are CSRF-protected read intents, no UUID or activity renewal.
Roster contacts/filters stay POST body/RAM; safe DTO exposes invoice status only.
Writes require current role/scope, UUID and exact EC-C1/full guards, owned COMMIT
before output; mandatory effects/audit/T23 atomic. GET uses initial live authority,
separate SNAPSHOT and final live authority; no entities/secrets/history snapshots.
Default no-argument host, schema/grants/packages and unrelated APIs unchanged.
Earlier exposure checkpoints below are historical.
2026-10-09 current exposure: explicit LOCAL_DEV adds full Admin API006–014 and
AcademicAffairs Course074–079/Band080/lookup090 restricted to fourCourse actions.
Search accounts POST /api/v1/admin/accounts/search: PII body/read intent/CSRF/no-store.
POST /api/v1/admin/staff-accounts: first201 exact7credential fields, replay200 safe
SECRET_RESULT_UNAVAILABLE; no password storage/retrieval endpoint. POST accounts/{id}/
reset-password,unlock,disable,restore plus existinglock: RV/UUID/liveNORMALAdmin;
search/detail/lookup read without idle renewal. API015–017 not mounted.
GET /api/v1/academics/courses with exact filters/sort/page/pageSize; POST create201,
replay200; GET {id}; PUT {id}/details; POST {id}/deactivate oractivate. Exact positive
VND string, immutablecode, no-op400, fouractionT23/audit/liveAcademicAffairsNORMAL.
GET /api/v1/academics/bands full exact5set/frozenflag, no write/GETrepair; malformed503.
GET academics/command-results/{requestId}?action= onlyCourse75/77/78/79 actions.
All private responses no-store; owned COMMIT before first output; wrongrole403,
restricted/currentinvalid401, malformed400, stale409; no automatic write retry.
No-argument host remains unmounted; LOCAL_DEV runbook/currenttaskcard governs.
Older entries below are historical exposure checkpoints.
# Implemented API exposure

2026-10-09 V01-V adds an optional StartupReadinessService, explicitly registered
alongside Q. It waits ApplicationStarted then performs fresh all9 evaluation;
failure requests host stop. Startup success never bypasses per-request Q checks.
No new route/DTO/default registration or active profile is supplied by this hook.

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

2026-10-06 A06/A07: explicit MapCurrentSession GET /api/v1/auth/me (API003),
MapCurrentSessionLogout POST /api/v1/auth/logout (API005); unchanged Program mounts none.
LiveCsrfIdentityResolver requires supplied JWT profile/reader/clock/A01 recorder; JWT
is taken only from __Host-ec-auth. Current SQL facts establish identity and minimal role
context, not private token claims. Required body/Origin/CSRF/profile ordering remains.
GET /me has no body/query, allows verified NORMAL/PASSWORD_CHANGE context200 with
exact same9fields as login/no-store, no JWT/SID/reissue/idle renewal. Invalid session401,
restricted business403, invalid shape400, missing trusted proof/middleware503.
POST /logout has no business body/query/UUID. Valid current session +Origin/framework
CSRF required before absent-input check and final Account→Session mutation guard.
Success204 empty body/secure host-only expired auth cookie after current-SID commit;
other device remains valid. Existing/missing/expired/revoked session401, bad CSRF403,
extra body/query400; failure500/known lock conflict409 never fake successful logout.
Independent logout/denied security logging uses A01 fixed fallback counter, no secrets.
Live resolver removes an invalid supplied auth cookie on401 (including idle expiry),
allowing deliberate anonymous /csrf bootstrap afterward; it never turns401 into204.
No cookie removal on CSRF403 or failed valid-session commit500. No automatic retries.
Real production profiles/keys/principals/connection readiness remain deferred; API004
password change/A08 and replay tasks are not implemented by this batch.


## A08 password change and R01 canonical primitives — reviewed 2026-10-06

Explicit (unmounted) POST `/api/v1/auth/change-password`: live NORMAL or
PASSWORD_CHANGE cookie/session, exact same-origin/framework CSRF; bounded strict
JSON `{currentPassword,newPassword,expectedVersions:{account}}`, no query/unknowns.
Current credential raw; new credential P08 policy. Invalid session401; CSRF403;
malformed/blocked new/wrong current400 VALIDATION_ERROR field errors without echo;
stale RV409. Atomic new hash/security-version+1/tempclear/all-own-SID revoke and
mandatory audit. Success204/no body, secure cookie deletion only after commit;
no new JWT/automatic login/replay or caller-result. Deliberate new login required.
Default Program unchanged; SQL COMMIT wrapper source-reviewed with HTTP fixtures,
actual SQL core tested rollback-only, no persisted runtime readiness claim.

R01 supplies disconnected exact55action/6lookup registry and EC-C1 full-request
schema normalization/strict canonical UTF8/SHA256 binary32. Trusted route selection,
whole supplied intent/manifests/expected absence, numeric/composite set ordering,
ordered weekly candidates and omitted/null semantics follow accepted E04. Registry
metadata does not confer current rights. No T23/completion/lookup/replay endpoint,
no secret/body/hash logging or plaintext persistence; R02/R03 remain unstarted.


## R02/R03 explicit Admin pilot — 2026-10-06

Implementation is available through explicit adapters; default Program remains unmounted.
All three paths require live NORMAL Admin authority and no-store. Unsafe requests
require framework CSRF and accepted Origin before detailed validation.

| Method/path | Request | Response |
| --- | --- | --- |
| GET /api/v1/admin/accounts/{id} | Positive canonical ID; no body/query |200 exact AccountAdminView12fields, coherent SNAPSHOT plus final live authority; missing404. |
| POST /api/v1/admin/accounts/{id}/lock | Strict Idempotency-Key UUID; body only expectedVersions.account exact8byte RV |200 current AccountAdminCommandResult account+operationId after atomic TX13 commit. |
| GET /api/v1/admin/command-results/{requestId}?action=... | Strict UUID and single canonical action selector; no body/other query |200 SafeCommandCompletion4fields outcome/operationId/resultMode/target; unconfirmed404. |

Lock ACTIVE→LOCKED, self/last-active protection, security version increment/all-own-SID
revocation/mandatory audit/completion are atomic. No role/profile/hash/temp/throttle
edits or new credentials. Matching active replay bypasses old mutation RV/state and
returns current authorized view with old operation; no additional audit/effects.
Different intent409 IDEMPOTENCY_CONTENT_MISMATCH only after stored-target access.
Expired retained command key409 CONFLICT; lookup absent/not-caller/expired/out-of-scope
404 RESULT_NOT_CONFIRMED does not prove rollback. No auto-retry or new-key recovery.

API014 recognizes exact six ADMIN.* selectors; this does not implement other Admin
mutations. Create/reset markers expose SECRET_RESULT_UNAVAILABLE and accountId only.
R02 reusable55action target/mode codec and six lookup allowlists need mandatory live
domain-scope callers; other modules/routes remain deferred. No response/PII/secret/
body snapshot, no pending/failed status, no purge/overwrite. SQL execution evidence
is rollback-only; owned COMMIT wrappers source-reviewed with explicit HTTP fixture
commit seams. Runtime/profile/principals/full readiness and committed stress deferred.

2026-10-07 U01 adds an unmounted frontend HttpClient seam only: fixed csrf/me GET,
register/login/logout/change-password POST, exact existing paths/X-CSRF-TOKEN.
No backend endpoint exposure changed. Same-origin cookie/no-store, RAM CSRF/context,
strict safe envelope; session/CSRF recovery never automatically resends writes.
Lost auth writes report unknown server outcome and clear still-current local authority.
See U01_TASK_CARD/ADR174; production/runtime integration remains deferred.

2026-10-07 U03/U04 add explicit frontend signup/login/me/logout consumers only.
Signup201 is exact CREATED/message and goes login without automatic credentials.
IdentityApp performs non-renewing me/CSRF revalidation, normal/restricted guards
and logout unknown handling. Default App/main and all backend exposure remain
unchanged. Edge fixtures intercept synthetic same-origin APIs; no real production
cookie/runtime claim. See U03/U04_TASK_CARD, verification-summary and ADR175.

2026-10-08 U05/U06 add frontend consumers only: exact API004 raw credentials/
expectedVersions.account and API007/010/014 Admin detail/lock/lock-result.
Local confirmation is not a DTO field; UUID header only confirmed lock intent.
Safe completion is not AccountAdminView or historical effect snapshot; lookup404
is not rollback proof. No automatic write retry/poll or other Admin routes.
Default App/main and backend API exposure unchanged; real cookie/SQL/runtime
integration remains unverified. See U05/U06 cards/evidence, ADR176.

2026-10-08 V01-Q supplies an explicit unmounted RuntimeReadinessBoundary. Its
caller chooses two distinct non-API health paths: GET/HEAD readiness literal
ready/not_ready200/503, liveness alive200, no-store; HEAD has no body, other methods
405. Paths are configuration, not new API-155/business routes or auth authority.
Every /api/v1 request through this installed boundary evaluates a fresh supplied
profile and all9 essential probes; failure returns existing SERVICE_UNAVAILABLE503
before downstream. No cached ready stamp, dependency details, idle touch or automatic
profile/provisioning. Default Program/routes remain unmounted. See V01Q_TASK_CARD.
