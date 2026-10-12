M3 batch3 COMPLETE/review PASS/applied ADR163/main reverified sourceb32a8e3; whole M3 COMPLETE. API145 adds FINALIZE whole-Class preview, API147 POST /api/v1/academics/grade-reviews/classes/{id}/finalize (AcademicAffairs NORMAL/CSRF/UUID/full grade+Registration manifest); stored FN05 two-decimal final totals +FN03 unfinished +audit/T23 atomic. API148 GET /api/v1/students/me/grade-reports own FINALIZED paged filters courseId/classId/fromDate/toDate/sort/page/pageSize; API149 GET .../{id} exact minimal published components/comment. Student NORMAL/private no-store, unpublished/foreign404, no grade provenance/RV. No schema/default host/package change.

2026-10-11 M2 batch1 Accountant API048–053 implemented/reviewed PASS; main integration receipts in docs/plan/M2-finance-evidence/summary.json.
Mounted only in the explicit existing LOCAL_DEV host. All require live Accountant NORMAL authority/no-store; unsafe POST/PUT require same-origin framework CSRF.
POST /api/v1/finance/invoices/search accepts exact DTO11.3 paged filters in RAM/body; real VW06/FN06 count/items share SNAPSHOT and final live authority.
GET /api/v1/finance/invoices/{id} returns exact16field InvoiceAccountantView; POST /{id}/collection-preview returns6fields/full two-RV+Registration/paymentId:null manifest or blocked/null.
POST /api/v1/finance/invoices/{id}/collect requires UUID and full positive digit-string amount/method/reference/manifest;201 first,200 matched replay.
PUT /api/v1/finance/payments/{id}/metadata requires UUID/method/explicit nullable reference/reason/payment+invoice versions;200, immutable amount/link/paidAt/confirmer.
GET /api/v1/finance/command-results/{requestId}?action=FINANCE.INVOICE.COLLECT or FINANCE.PAYMENT.UPDATE_METADATA returns caller-scoped SAFE_REFERENCE invoiceId/paymentId only;404 is not rollback proof.
Collect/correction share owned atomic effects+mandatory audit+T23; current authority/link before replay digest; stale409 precedes contextual amount400; no automatic retry.
Student invoice/print API046–047 privacy retained. No Class/schedules/report/export/partial payment/refund/gateway or new schema/dependencies.
Historical checkpoints below are provenance.
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

2026-10-11 M2 batch2 assigned API091–099/102–118 (excluding100/101), mounted only by the existing explicit LOCAL_DEV profile:
- AcademicAffairs: GET/POST `/api/v1/academics/classes`, GET `/{id}`, PUT `/{id}/capacity`; POST `/{id}/teacher-change-preview`, `/change-teacher`, `/transition-preview`, `/open`, `/cancel`, `/roster/search`, `/placement-preview`, `/assignments`, `/schedule-preview`, `/schedules`; GET `/{id}/scheduling-context`. GET `/api/v1/academics/class-teacher-options` optionally `classId`.
- AcademicAffairs: POST `/api/v1/academics/placement-candidates/search` with contact/paging/course/band filters in body; GET `/api/v1/academics/placement-candidates/{id}` and `/{id}/classes`. GET `/api/v1/academics/schedules` with inclusive local dates/current-history/paging filters and `/{id}` provenance/detail.
- Teacher: GET `/api/v1/academics/teachers/me/classes` and `/{id}`, POST `/{id}/roster/search`; current Teacher assignment only, no invoice/entry-score/management tokens.
- Student: GET `/api/v1/students/me/classes` current/history and `/{id}`; own assignment only, Teacher display name only, no peer/cohort/management data.
ClassCreate/ClassAssignment/ScheduleCreate first201; replay200, capacity/Teacher/open/cancel200. All reads200/no-store; cookie/CSRF/live NORMAL role, strict query/body/header UUID and canonical manifests remain mandatory. Preview never writes or dispatches a UUID.
Shared GET `/api/v1/admissions/command-results/{requestId}?action=...` adds only ACADEMIC.CLASS.{CREATE,UPDATE_CAPACITY,CHANGE_TEACHER,OPEN,CANCEL,PLACE_REGISTRATION} and ACADEMIC.SCHEDULE.CREATE to the retained Program actions. SAFE_REFERENCE only; lookup404 is not rollback proof.
All previews return canonical full retained membership/RVs plus minimal scoped timetable/resource guards. Mutations reload locks and check final authority/deadline/business cutoff; mandatory effects/audits/T23 commit atomically. No START/COMPLETE/weekly/move/replacement routes or automatic teaching-state/attendance/score effects.
2026-10-11 M2 batch3 API119–124/127–130, using the retained explicit LOCAL_DEV mount:
- AcademicAffairs POST `/api/v1/academics/classes/{id}/weekly-schedule-preview` and `/weekly-schedules`: full inclusive ISO-weekday LESSON plan, whole-plan manifest and atomic creation; no skipped conflicts or automatic exam.
- AcademicAffairs POST `/api/v1/academics/schedules/{id}/move-preview`, PUT `/{id}/time-room`; POST `/{id}/replacement-preview` and `/{id}/replace`. Move retains the row/type/key before start; replacement at/after start needs reason and retains obligation/Class/type through an atomic NOT_HELD predecessor and future current successor.
- Teacher GET `/api/v1/academics/teachers/me/schedules` and `/{id}`; Student GET `/api/v1/students/me/schedules` and `/{id}`. Inclusive local dates, current/history, Class filter and paging; current ownership rechecked before publication. Student Teacher reference is displayName only; both roles omit management tokens, dependency flags and actor/reason provenance.
Weekly/replacement first201 and matched replay200; move200. Shared090 SAFE_REFERENCE adds exactly ACADEMIC.SCHEDULE.CREATE_WEEKLY/MOVE/REPLACE. All cookie/CSRF/live NORMAL role, strict full C1 manifest, no-store and final deadline/clock checks remain mandatory. Target absence guards and before/proposed resource tokens are retained; conflict computation excludes only the edited target. Preview/commit use the retained10000 work-unit budget: exhaustion503 with no partial result/effects. No125/126 HELD or teaching lifecycle/attendance/grades routes.

2026-10-12 M3 batch1 API100/101/125/126/131–135 extends the explicit LOCAL_DEV mount:
- AcademicAffairs POST `/api/v1/academics/classes/{id}/start` and `/complete`; preview098 now provides START/COMPLETE manifests. START requires the full current required LESSON set, nonempty placed cohort and first LESSON start reached; no FINAL_EXAM prerequisite. COMPLETE requires all current required LESSON and exactly one FINAL_EXAM HELD plus full LESSON×cohort recorded Attendance; absent is valid, no exam Attendance or grade prerequisite.
- AcademicAffairs POST `/api/v1/academics/schedules/{id}/held-preview` with empty body; POST `/confirm-held` with full current ScheduleHeldManifest. Current PLANNED occurrence/Class ONGOING/at-or-after scheduled end, explicit fact confirmation; no default HELD from clock or Attendance. HELD actor/time/mandatory audit/T23 atomic.
- Teacher GET `/api/v1/academics/teachers/me/classes/{id}/attendance` with current/history, sort and paging; GET/PUT `/{id}/lessons/{schedule}/attendance`. GET sheet full max15 cohort with contacts/latest provenance, actual null record and current edit context; PUT exact delta +full cohort/changed existing RVs/bindings/new absence, one UUID, all-or-none. From LESSON start to before Class COMPLETED/CANCELLED, current Teacher/current LESSON/non-NOT_HELD; no ONGOING-only/24-hour restriction.
- AcademicAffairs GET `/api/v1/academics/classes/{id}/attendance` and `/{id}/lessons/{schedule}/attendance` read-only; excludes Teacher editing/writeContext/Attendance RV and all grade values. Summary coverage is independent of selected page/history view.
Commands200/no-store/cookie/CSRF/current NORMAL role; required Class/HELD audit and all T23/effects/activity share owned transaction. Attendance records latest provenance, no new mandatory Attendance AuditLog history per ADR149/E04 final. All-equal delta400/no marker; equal supplied rows still validated, mixed changes preserve equal provenance. COMPLETE locks Attendance without submitting/finalizing grades or clearing Registration.is_unfinished.
Canonical shared090 `/api/v1/academics/command-results/{requestId}?action=...` routes retained Class/Schedule actions and new ACADEMIC.CLASS.START/COMPLETE, ACADEMIC.SCHEDULE.CONFIRM_HELD and Teacher TEACHER.ATTENDANCE.SAVE_DELTA. Role and current target rights precede marker disclosure/stale-write guards; safe references only, no cached response or automatic resend. Historical M2 admissions-path lookup remains compatible with its original actions.
# M3 đợt2 — Teacher draft và Academic review

Explicit LOCAL_DEV only; default host remains unchanged. Teacher NORMAL/current assigned Class: GET `/api/v1/academics/teachers/me/classes/{id}/gradebook`136, GET/PUT `.../assignments/{assignment}/grade-draft`137/139, GET `.../components/{component}/schedule-options`138, POST `.../grade-submission-preview`140 (SUBMIT/RESUBMIT), POST `.../submit-grades`141 and `.../resubmit-grades`142. AcademicAffairs NORMAL: GET `/api/v1/academics/grade-reviews`143, GET `.../classes/{id}`144, POST `.../classes/{id}/review-preview`145 RETURN only, POST `.../classes/{id}/return`146 with reason. Shared090 now supports139/141/142/146 for their exact role/action pair, safe refs only.

Canonical DTO25–28/full config/cohort/score/report/current Schedule manifests; no Assignment RV. Per-Student changed scores/comment atomic with report RV touch/mandatory actual score history/T23. Missing≠0/omit keeps/no score delete; GENERAL null schedule, linked current same-Class/type/from start. All-equal valid intent400 NO_CHANGE/no marker; strict2decimal input, saved totals from FN05. Draft ONGOING/COMPLETED +DRAFT/RETURNED; whole submit/resubmit COMPLETED/nonempty/full valid scores/all DRAFT or RETURNED; return all SUBMITTED+reason. SP03 five SNAPSHOT result sets supply non-DRAFT review values; no draft placeholders/Teacher context in review. Grouped Class/per-report workflow audit, retained return reason/comment/scores, final_score null and unfinished unchanged. Cookie/CSRF/Origin/no-store/current authority and original deadline to COMMIT; preview no UUID, commands UUID, manual090/no automatic resend.

No147/finalize/own Student148–149/new schema/grants/packages/public default mounting. Final batch evidence M3_02_TASK_CARD/REVIEW and M3-grading-evidence/summary.json.
