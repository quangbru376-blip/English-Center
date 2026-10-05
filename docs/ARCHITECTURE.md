# Architecture

## Current implemented architecture

Latest checkpoint2026-10-05: D05 applied/reverified PASS main9e843b8;
locked restore/build0warnings0errors/full454tests PASS. Domain/Training adds
Attendance/Score/GradeReport, three explicit configurations and DbSets.
Full19tables/180columns/19PK/2AK/43NO ACTION FK/62indexes/52CHECKs/17rowversions.
D01–D04 signatures retained; Admissions composite ownership links and Schedule
conditional current/history/predecessor shapes preserved. Attendance unique
Schedule/Assignment and Score unique Assignment/Component keep missing as no row;
no defaults for present/score/last-write timestamps. Score.Schedule optional FK
has nonnull filtered lookup; same Class/current/type/kind is future TG05/service.
GradeReport unique Assignment stores optional comments/latest submission/review/
return metadata, decimal(5,2) final_score only FINALIZED with complete review and
same final/review timestamp. Complete latest submit required outside DRAFT;
NULL-safe review bundle and retained return reason support resubmission history.

All43FK lookups explicitly covered under ADR-164, including filtered nonunique
Score.Schedule lookup; unique completed-booking coverage retained. No other EF
conventions changed. See [D05 card](plan/D05_TASK_CARD.md). Parent report RV touch,
TG04/TG05/cross-table Class/type/kind/role/window/locks, cohort all-or-none,
half-up total/history/audit/atomic DRAFT creation remain later service/TX/SQL.
DbContext still lacks runtime connection/profile/DI wiring; no SQL/migrations/
seed/business routes. Disconnected metadata/DDL does not prove live constraints/
TX/auth/CSRF/readiness. Original reviewed worker/manager trees retained.

The checkpoints below describe their original stage, before these additions.

On 2026-10-04 P02 was implemented/reviewed in detached commit d9eb194, then
applied to main as df7cd9a with explicit owner authorization and reverified;
see [P02 task card](plan/P02_TASK_CARD.md). Its net10.0 API has only
CreateBuilder/Build/Run plus public partial Program; no application endpoints.
The xUnit executable references the real API and uses WebApplicationFactory's
in-process TestServer for ten verified missing-route cases in two environments.
At the original P02 checkpoint primary had no EF model, migration, auth, SQL
connection or ready profile. Latest additions are described above.
P02 startup tests remain unchanged; P05 adds31time tests (41total main PASS).

P03 frontend applied/reverified on main6cb2590 (reviewed c5e4eac); see
[task card](plan/P03_TASK_CARD.md). React19.3/Vite8.3/TypeScript6/Tailwind4
entry renders one semantic noninteractive Vietnamese unavailable-functionality
page; no router/API/auth/storage/business UI. npm lock/typecheck/lint/build,
audit0 and three-width built-page checks verified.

P05 applied/reverified on main8784686 (reviewed 6e5abe5); see
[task card](plan/P05_TASK_CARD.md). Common/Time has a concrete BusinessClock
injected with built-in TimeProvider and static VietnamTime conversion helper.
Clock formatting/storage outputs UTC millisecond precision by truncation;
cutoff comparison preserves raw instant, equality expired. Explicit
Asia/Ho_Chi_Minh (Windows mapping fallback) handles Vietnam dates, minute
schedules and inclusive date filters as half-open UTC bounds, rejecting
unrepresentable/invalid/ambiguous conversions. No host timezone/clock fallback.
No DI/endpoint consumer yet; those integrate the primitives in future tasks.
No new packages. Original detached P02/P03/P05 trees retained for evidence.
P04 applied/reverified on main d9840bd (reviewed6a5269c); see
[task card](plan/P04_TASK_CARD.md). Common/Validation adds NonsecretText
(UTF16 validation, trim/NFC/codepoint length), ContactNormalizer (ASCII CI email,
canonical0 phone) and StudentBirthDateValidator using P05 Vietnam clock.
Helpers do not process passwords, perform uniqueness queries or expose routes.
No new dependencies.79P04 tests bring main total to120pass/0fail/0skip.
P06 applied/reverified on main94dd832/4e0a0fa/5afaf34; see
[task card](plan/P06_TASK_CARD.md). Common/Serialization now has strict
UUID-D/single-header, LocalDate/MinuteTime and JSON Int32 lexeme parsers;
RowVersionCodec canonical padded Base64 preserves8bytes. ExactNumbers uses
decimal for positive whole VND18 and score/weight2precision; ReportAmountCodec
uses built-in BigInteger for canonical nonnegative decimal38 totals. No float,
new packages or binder/endpoint wiring. Main287tests pass/0fail/0skip.
Successful AGY scalar/report parts and independent manager numeric recovery
are retained as separate review snapshots; rejected AGY outputs never applied.
`instructions/` is a directive/reference inbox and is not part of the planned
runtime architecture.

## Approved target constraints

The following is a target layout from the project instructions, not evidence
of completed implementation:

```text
backend/src/EnglishCenter.Api/
  Modules/<Module>/
  Domain/
  Infrastructure/
  Common/
backend/tests/EnglishCenter.Tests/
frontend/src/
  app/
  features/<module>/
  components/ui/
  lib/
  styles/
```

Backend dependency direction is planned as:

```text
Controller -> Service -> Repository -> AppDbContext
```

Planned business modules are Students, Admissions, Academics, Finance,
Reports, and Administration. These names come from the project-structure
instruction and should be verified against the scaffold once code exists.

## Planned data and request flow

- React pages call the API through one shared HTTP wrapper and React Query.
- API controllers bind requests and delegate to one service operation.
- Services own business rules, authorization checks involving ownership, and
  multi-step transactions.
- Repositories contain EF Core persistence queries only.
- EF Core entities and migrations are the future schema of record.
- Sensitive actions are planned to write `AuditLog` entries from the
  application layer.

None of these flows can be verified until the application is scaffolded.

## Accepted target data model — report baseline plus A–C

Updated 2026-09-28: A01–A10/B01–B07/C01–C16 are accepted in ADR-083–ADR-094.
The original 20 entities are extended with EntryTestBooking and AuthSession:
22 target entities, not deployed tables or a final E03 dictionary/ERD. C13 may
require a technical replay-result store; its physical mapping belongs in the
complete dictionary, not an additional product feature. E03-v1 (Accepted 2026-10-01) at
docs/design/DATABASE_SPEC.md adds ReplayOperation/InvoiceNumberCounter:
owner accepted the overall 24-table model and A consistency conventions on
2026-10-01 (ADR-131), then presented B account/profile/session persistence
(ADR-132), presented C configuration/entry-test storage (ADR-133), and presented
D Class/assignment/Schedule storage (ADR-134), and presented E Attendance/Score/
GradeReport storage (ADR-135), and presented F Invoice/Payment/audit/replay/counter
storage (ADR-136). Presented G1 SQL-object purposes/counts/application paths
are accepted (ADR-137): 5 triggers,6 views,6 UDFs,7 procedures, actual callers,
service-owned authorization and no audit trigger. Presented G2 SQL roles,
shared transactions, application locks, report snapshots and query-driven index/
verification strategy are accepted (ADR-138). Runtime uses least privilege,
reporting roles do not add web rights, and schema/maintenance principal is
separate. Owner accepted the actual consolidated dictionary/trace E03-v1 on
2026-10-01; database stage3 is done. Presented E04 package1 is accepted
(ADR-139): opaque Base64 DTO versions/expectedVersions, UUID Idempotency-Key
for same-intent replay without secrets, and POST-body PII search/non-PII GET.
Existing rights/save units/error policy remain unchanged. E04-v0.1 is at
docs/design/SYSTEM_ARCHITECTURE_SPEC.md; presented Auth/session2a accepted
ADR-140: five Auth routes/minimal context, framework CSRF token RAM/header/
identity refetch,403 CSRF_INVALID, no automatic retry/credential replay.
Presented Administration2b accepted ADR-141: separate account commands,
UUID/version/atomic audit-replay, safe calling-Admin completion lookup and
paginated audit reads, no secret replay/new Admin business rights.
Presented Students3a accepted ADR-142: six scoped Student routes, atomic
self-registration without auto-login, two-version profile saves, no signup/
profile Idempotency-Key or form/credential replay; own home/ACTIVE catalogue
and historical refs, deliberate unknown-response handling.
Presented Student Admissions3b accepted ADR-143: own Registration/test/
confirmation groups, read-only previews/related-version membership sets,
atomic Booking reschedule, six UUID write commands with atomic audit/completion,
caller-only result lookup; no preview reservation/automatic stale retry.
Presented Admissions/Finance3c accepted ADR-144: role-specific search/detail,
atomic admission-Invoice and Payment-PAID, reasoned cancellation/metadata
correction; five UUID commands/related versions/atomic audit-completion/caller
lookup, consistent financial snapshot reads and fresh Student-own browser print.
Presented Entry sessions/Teacher grading4a accepted ADR-145: AcademicAffairs
Session commands/roster and Teacher currently-assigned per-student initial
score/absence/reasoned correction; nine UUID writes/related versions-set,
atomic result-best Band/history/audit-completion, live guards/caller result
lookup and adopted SQL callers, no automatic effects.
Presented Course/program/configuration4b accepted ADR-146: catalogue reads/
commands, whole-program configuration TX, separate lifecycle/name edits,
seed Band reads/no editor-unlock, ten UUID writes/related component set/
atomic audit-completion/caller lookup/current guards/no automatic activation.
Presented Class/placement/scoped reads4c accepted ADR-147: explicit Class
lifecycle/Teacher/capacity commands and previews, one-Registration placement
with atomic Assignment-DRAFT report-audit-completion, role-specific reads,
eight UUID writes/related versions-set/live checks/caller lookup/no auto effects.
Presented Schedule/scoped calendar4d accepted ADR-148: role-scoped current/
history reads, concrete individual/weekly previews and atomic weekly batch,
pre-start move, reasoned same-obligation replacement, manual after-end HELD;
five UUID writes/related versions/live recheck/caller lookup/no auto effects.
Presented Attendance/per-Student draft4e accepted ADR-149: Teacher task-scoped
and AcademicAffairs read-only Attendance, one-LESSON delta batch, one-Student
scores-comment all-or-none, missing-value semantics, two UUID writes/related
versions-parent report/live guards/caller lookup/no automatic send-publication.
Owner checked all23 displayed E04-R1 rows and explicitly accepted them on
2026-10-01: package4f/5/6/7/8/9 recorded respectively as ADR-150–155 (whole-Class
grades, reports/CSV, frontend state/auth, backend/SQL/replay/audit, laptop
configuration/runbooks and trace/verification/consolidation process). This
accepts only the displayed contracts, not unseen refinements. Complete the
actual E04 artifact and explicit gaps/deferred items for separate acceptance,
then E05; D12/D13 stay deferred. No implementation or executed tests.
The complete architecture artifact is not accepted;
precise API/signatures/mapping/timeout/grants and measured behavior remain E04
or future implementation evidence, not implemented architecture.
SQL rubric is adopted ADR-130.
Fifteen D choices are
accepted in ADR-095–ADR-102/114–120; D12/D13 are deferred and E remains open.
Use canonical `docs/requirements.md`; old Word schema images/forms do not
supersede its rules. Scholarships remain excluded.

| Entity | Attributes / responsibility |
|---|---|
| Role | role_id, role_code, role_name; seven fixed account roles. |
| Account | account_id, role_id, email, phone, password_hash, full_name, status, created_at; security_version, failed_login_count, failure_window_started_at, login_throttled_until, must_change_password, temp_issued_at, temp_expires_at. No plaintext credentials. |
| StudentProfile | student_id, account_id, date_of_birth, address; one profile per student account. |
| Course | course_id, course_code, course_name, base_price, status. |
| Band | band_id, band_code, min_score; minimum threshold only, no max_score. |
| TrainingProgram | program_id, course_id, band_id, program_name, status, expected_lesson_count; first-Class configuration freeze. |
| ScoreComponentConfig | component_id, program_id, component_name, weight, kind GENERAL/LESSON/FINAL_EXAM; frozen after first Class use. |
| Registration | registration_id, student_id, course_id, status, is_unfinished, admission_confirmed_at, admission_approved_by, admission_approved_at, created_at; cancellation/rejection actor/time/reason/prior-state and parent operation attribution where required. |
| EntryTestSession | session_id, start_at, end_at, room, capacity, grader_id, status. |
| EntryTestAttempt | attempt_id, registration_id, attempt_number, score, graded_by, graded_at, completed_booking_id; logical result/attempt, display status derived from result/current booking. Historical appointments are not overwritten here. |
| EntryTestBooking | booking_id, attempt_id, session_id, status SCHEDULED/COMPLETED/ABSENT/CANCELLED, booked_at; absence/cancellation actor/time/reason/prior-state/operation attribution where required. One current SCHEDULED booking per unfinished attempt. |
| BandAssignment | band_assignment_id, registration_id, best_attempt_id, band_id, assigned_at; current best completed attempt for the same record. |
| Class | class_id, program_id, teacher_id, class_code, status, max_size. |
| StudentClassAssignment | class_assignment_id, class_id, registration_id, assigned_by, assigned_at; immutable enrollment-to-class link/provenance (ADR-134). |
| Schedule | schedule_id, class_id, session_type LESSON/FINAL_EXAM, session_date, start_time, end_time, room; obligation_key, replaces_schedule_id, occurrence_state PLANNED/HELD/NOT_HELD, occurrence actor/time and non-held actor/time/reason. One current schedule per obligation. |
| Attendance | attendance_id, schedule_id, class_assignment_id, present, entered_by, entered_at; no row/default for unrecorded attendance (ADR-135). |
| Score | score_id, class_assignment_id, component_id, schedule_id, score_value, entered_by, entered_at; schedule required for LESSON/FINAL_EXAM, NULL for GENERAL. |
| GradeReport | report_id, class_assignment_id, final_score, status, teacher_comment, submitted_by, submitted_at, reviewed_by, reviewed_at, returned_reason, finalized_at. |
| Invoice | invoice_id, invoice_number, registration_id, amount, issued_at, due_date, status; amount snapshot at issuance. |
| Payment | payment_id, invoice_id, amount_paid, method, transaction_reference, confirmed_by, paid_at. Confirmation time only, no separate actual-receipt time (ADR-046). |
| AuditLog | audit_id, actor_account_id, action, entity_type, entity_id, result, details, occurred_at, ip_address, operation_id; required old/new history and each whole-class workflow event, never secrets/full payloads. |
| AuthSession | sid (random key), account_id, issued_at, expires_at, last_activity_at, revoked_at, security_version, restricted-session scope if applicable; DB-checked revocation/status/expiry. D03/D04 activity/shared-tab/restricted lifetime accepted in ADR-097/098. |

E03 C refinement accepted ADR-133: Registration starts PENDING, first booking
moves it to TESTING and first scored result to BAND_ASSIGNED; attempt two
keeps BAND_ASSIGNED while pending-result guards still apply. Configuration
freeze comes from retained use history, not a second flag; entry-session
closure/occupancy are computed, and result/booking composite relationships
plus atomic Band projection preserve existing history and permissions.
E03 D storage accepted ADR-134: derive size from assignments, create DRAFT
GradeReport with placement/mandatory audit (no default Score/Attendance or new
DRAFT visibility), retain move/replacement/HELD history and current obligations.
Cancelled Class schedules remain historical and release resource occupancy.
E03 E storage accepted ADR-135: final_score is NULL until FINALIZED; interim
score totals are calculated from stored components. No Score row means missing,
not0; last-entered provenance and rowversion protect writes, Score/comment changes
update parent report version. Latest submit/review fields do not replace grouped
mandatory history; passing is derived, not a separate stored flag.

Main dependencies: Account → Role; StudentProfile → Account; Registration →
StudentProfile/Course; TrainingProgram → Course/Band; Class → TrainingProgram/
teacher Account; StudentClassAssignment → Registration/Class. There is no
direct StudentProfile-to-Course or class-assignment-to-profile foreign key.

### Relational integrity and history — accepted C01–C06/C10/C15/C16

- PKs for business entities: int IDENTITY. sid/operation identifiers are random
  technical identifiers; Student display code is the internal ID, not a new
  manual code. FK deletion restricts historical data; no cascade history purge.
  Unused draft configuration can be removed only with no dependency and an
  existing authorized operation; this creates no blanket delete permission.
- Unique normalized Account.email and phone when present; one StudentProfile/
  Account. Unique Course/Class codes, Band codes/minimums, (Course,Band) program
  and (program,component name). Component comparison ignores case, not accents.
- Unique (Registration,attempt_number), with attempt 1–2. Booking holds session
  history; a completed result references that attempt's COMPLETED booking.
  At most one current SCHEDULED booking for an unfinished logical attempt.
  Absence/cancellation/rebooking do not create another attempt or fake score.
- Registration.is_unfinished is server-owned and indexed with filtered unique
  (student_id,course_id) when true. Update atomically with cancellation/rejection
  or Class COMPLETED AND student's GradeReport FINALIZED; verify the underlying
  predicate on writes. No Registration COMPLETED enum or passing prerequisite.
- At most one class assignment, BandAssignment and Invoice/Registration;
  one GradeReport/class assignment; one Payment/Invoice. Attendance unique
  (schedule,class assignment); Score unique (class assignment,component).
- Required business relationships are NOT NULL once that object exists; missing
  Score is no row, not zero. Band/Payment/finalized result exist only at their
  lifecycle prerequisites. Audit actor NULL only when unauthenticated; guessed
  login email is not a verified actor.
- Validate related entities' roles/ownership/same Class/program in services,
  not merely FK existence. Attendance joins LESSON of its own Class; linked
  Score joins the component's required type/current Class schedule. GENERAL
  has no schedule FK value. Scope/refined visibility is ADR-083/084.
- Course/Class codes remain immutable and unique even for ended records;
  no global uniqueness of Payment.transaction_reference. Course price edits
  never rewrite an issued Invoice snapshot. Invoice numbers server-generated
  HD-YYYY-NNNNNNNN with Vietnam issue year and safe database counter, never
  reused; gaps are allowed.
- Program course/Band/components/kind/weights/expected LESSON count freeze when
  any Class references it, including CANCELLED. Band thresholds freeze on first
  COMPLETED entry result. Serialize first-use and config edits in one transaction;
  no version entity/reset-by-history-deletion or used-config override.
- FK/state/date/Teacher/room and stable-pagination query patterns receive indexes;
  check constraints cover valid enum/range values. Precise names/order/DDL and
  every field's nullability still belong to the full E03 dictionary.

### Occurrence, scheduling and score dependencies — ADR-085–ADR-088

- Class.program_id never changes after creation; max_size edits only before
  ONGOING, within 1–15 and not under placed size. Existing teacher-change/halt/
  cancellation/placement rules and no transfers remain canonical requirements.
- Each Class has fixed LESSON obligations plus one FINAL_EXAM after its current
  last LESSON; the Program has one FINAL_EXAM component. Replacement chains share
  obligation_key, class/type and replaces_schedule_id. One current schedule/
  obligation; do not count old NOT_HELD rows or replacement chains as extra lessons.
- AcademicAffairs confirms each actual LESSON/FINAL_EXAM HELD only after its
  scheduled end while Class ONGOING, storing actor/time; no HELD undo UI or
  inference from Attendance/elapsed time. Complete the Class only when all
  required obligations are HELD and LESSON Attendance is complete.
- Preserve pre-start/no-dependency schedule edits. Post-start non-held linked
  replacement now applies to FINAL_EXAM as well as LESSON: AcademicAffairs,
  reason/history, non-ended Class and no old dependent data; replacement valid
  future date/current Teacher/conflict checks/all-or-none. No individual retake.
- ScoreComponentConfig.kind is GENERAL/LESSON/FINAL_EXAM. Linked Score entry
  opens at its schedule start, blocks NOT_HELD, and retains DRAFT/RETURNED/class
  lifecycle guards; submission/review/publication remains whole-class.
- Same Vietnam calendar day, minute precision, start < end; intervals [start,end)
  permit adjacent appointments. Guard Class/Teacher/room overlaps across class
  and entry tests; additionally guard the Student's bookings/assigned schedules
  when booking/placing and preserve that invariant when schedules change.
- Weekly rules generate concrete rows only: preview dates/obligation count/all
  conflicts, commit the entire valid batch or none. No recurrence/holiday entity,
  future recurrence job, automatic holiday skipping or additional LESSONs.
- Entry sessions effectively CLOSED at start regardless of stored status. Reads
  compute effective state without mutating DB; the next permitted write persists
  CLOSED in its transaction, always using server time. No cron prerequisite.
- Initial entry grading is from session end only; absence/correction retain
  existing ADR-027/029 guards. A blocked assigned account causes no automatic
  reassignment/cancellation; restoration or allowed pre-start reassignment only.

### Numeric, temporal and normalization contracts — ADR-091

| Value | Accepted target representation |
|---|---|
| Single tuition/Invoice/Payment | decimal(18,0), positive to 999999999999999999 VND |
| Financial sum | decimal(38,0), exact; API money is a decimal digit string |
| Entry/component/final score and weight | decimal(5,2); score 0–100; weight >0–100, sum exactly 100 |
| Expected LESSON count | int, positive, frozen on first Class |
| Event instant | UTC datetime2(3), ISO-8601 Z over API |
| DOB/due date | date |
| Class schedule | Vietnam business date + time(0), with minute/same-day guards |

Weighted calculation uses decimal intermediates and one final half-up two-place
rounding, never per-contribution rounding; reject overflow/excess input precision.
Vietnam date/filter/time conversions remain Asia/Ho_Chi_Minh independent of device.

Count Unicode code points, reject invalid surrogate sequences; NFC ordinary text,
not passwords. Allocate nvarchar for the full permitted code-point limit, not
just that many UTF-16 units. Passwords are exact: no trim/NFC/case conversion.
Creation email is one ASCII local@domain address, no display name/whitespace/
control characters; trimmed case-insensitive comparison, no provider rewriting.
Phone canonical 0xxxxxxxxx; optional trimmed-empty strings map to NULL, required
empty strings fail. Preserve accepted field lengths and staff/Student scope.

### Authentication state — ADR-092; D mechanisms remain open

E03 package B's presented storage design is accepted ADR-132: email varchar(254),
phone varchar(10), name nvarchar(200), address nvarchar(510), hash varchar(512)
subject to runtime capacity verification, random256-bit AuthSession key and
atomic Account/Profile edit checking both versions. Policies below are unchanged;
dictionary/CHECK/index candidates are accepted through E03-v1. Executable SQL
enforcement/mapping and runtime verification still belong to E04/later.

Account.security_version and AuthSession sid/version/DB state enforce status,
absolute/idle expiry and server revocation on every authenticated request.
ADR-049 events increment version/revoke existing sessions; unlock/reactivation
never restores them. No Redis/device-management module. D03 activity/tab identity,
D04 restricted lifetime and D06 transport/CSRF are separately accepted in
ADR-097–ADR-099; C11 did not previously approve them.

failed_login_count/failure_window_started_at/login_throttled_until use UTC and
atomic updates independently of manual LOCKED/DISABLED. must_change_password/
temp_issued_at/temp_expires_at support the accepted 24-hour temporary credential
gate; hashes only. The former target locked_until must not be interpreted as an
automatic manual-lock expiry or share the separate throttle timer.

### Transactions, replay safety and audits — ADR-093

- Short transactions + rowversion for stale edits + serialized object/resource/
  capacity guards; unique indexes and rowversion alone cannot enforce overlaps
  or cross-entity lifecycle checks. Protect booking/placement/payment/cancellation/
  grading-confirmation/first-use freezes/account revocation and Admin availability.
- Preserve atomic approval/Invoice, Payment/PAID, cancellation/UNPAID Invoice/
  future booking release, non-held/replacement, score/best Band and whole-class
  grade workflow. Failures cannot expose a partially transitioned aggregate.
- Critical business actions use request_id scoped to actor/action; store the
  replay result with the transaction for at least 24 hours. Same key/content
  returns no new side effect; conflicting content returns 409; recheck current
  rights before returning a recorded result. Never persist/replay temporary secrets.
  D02 HTTP/error/pagination contract is separately accepted in ADR-096.
- Audits keep actor/time/reason/prior-state and mandatory changed-field old/new
  allowlist. operation_id links parent/dependent cancellation and every student's
  report in one class operation, preserving each send/return/finalize event.
  No secrets/full payload, log-edit UI or class-teacher history workflow.
- D08/D09 are now separately accepted ADR-115/116. Mandatory business/account
  audit is atomic with mutations (including related revocation); failure rolls
  all back. Pure auth/logout/denial events are best-effort with bounded secret-free
  operational fallback, never bypassing checks or restoring a logged-out session.
  Keep mandatory history; expire pure security logs/IP after 90 days via operations,
  retaining mandatory fields when removing IP. Record processed events; throttled
  requests may aggregate count/IP/minute but must preserve throttle counters.
  Unauthenticated actor is NULL. No app purge/export or compliance assertion.

### Reports — ADR-094

One consistent DB snapshot per report/export query plus queried_at. Export is a
fresh query with the same filters/report columns, not a frozen earlier page.
Current-state/issue-confirmation-date/current-debt/distinct/result-cohort rules
remain ADR-063–ADR-072; no historical as-of snapshots. Display ratios/progress
half-up to two places without rounding counts; empty denominator is no data.
D10 encoding/size/limits/formula safeguards are separately accepted in ADR-101.

## Accepted system/frontend contracts — ADR-095–ADR-102

Accepted 2026-09-29 via Gói 1; target choices only. Complete API/security
and verification artifacts still require E04.

- D01 / ADR-095: modular monolith with one DB. Students owns the personal
  profile; Admissions registration/entry tests; Academics configuration/classes/
  schedules/grades; Finance Invoices/Payments; Reports report queries;
  Administration accounts/sessions/audit. DTOs enter application workflows/
  transactions then EF Core/SQL Server. Server decides rules/rights/exact money/
  totals; React organizes by feature and handles inputs/presentation.
- D02 / ADR-096: /api/v1, separate DTOs; errors code/message/fieldErrors/traceId,
  no secrets/SQL/stack trace. 400 invalid input, 401 missing/expired auth,
  403 wrong role, 404 outside visible record scope, 409 state/concurrency.
  Default 20/max 100 items with stable sort and ID tie-breaker; enforce rights
  and ownership on each endpoint.
- D03 / ADR-097: only user-initiated authenticated business requests update
  last_activity. Background polling/healthchecks/timers/mouse/typing heartbeat
  do not extend idle. Tabs in one browser profile share cookie/session;
  logout revokes that shared session. Other devices/profiles have separate sessions.
- D04 / ADR-098: a valid temporary credential grants at most ten minutes of
  restricted change-password/logout access, capped by its 24-hour expiry.
  Change revokes all sessions and returns to normal login, with no upgrade.
- D06 / ADR-099: same origin; signed JWT with issuer/audience/exp/sid/version,
  host-only HttpOnly/Secure/SameSite=Strict cookie, CSRF token/Origin checks on
  login and writes, no business mutation through GET. HTTPS, escaped plain text,
  parameterized queries, secrets outside source; no durable browser token/
  profile/grade caches or full sensitive payload logs.
- D07 / ADR-100: generic login response before authentication; IP login rate
  limit 100/minute, 429/Retry-After, separate from the accepted account throttle.
  Trust forwarded IP only from configured proxies; no new CAPTCHA/OTP module.
- D10 / ADR-101: UTF-8 BOM CSV, comma, CRLF, quoted/escaped cells; same filtered
  financial report columns/all results. Reject the entire file above 10,000
  data rows or 5 MB and request narrower filters. Quote and prefix dangerous
  formula text including full-width triggers with an apostrophe; DB values stay
  unchanged. Verify target Excel/LibreOffice, with the accepted limitation after
  spreadsheet edits/save/reopen. Export query remains fresh under ADR-094.
- D15 / ADR-102: Student flows prioritize 375 px, dense staff screens 1280 px;
  all are usable and checked at 375/768/1280. Vietnamese labels/errors, keyboard
  traversal and non-color cues, current stable Chrome/Edge. Future xUnit
  business/API checks, dedicated SQL Server integration/race checks and manual
  seven-role end-to-end/time/money/CSV checks; no new test framework or tests run.

### Shared frontend shell — FE01a / ADR-103

Accepted 2026-09-29: one shell for seven roles. At 1280 px use a left sidebar
about 240 px with icon/label and a header above content; at 768 px about
72 px navigation expandable to show labels; at 375 px compact header/menu
button/left overlay drawer with single-column content. Header shows screen/
account area; mobile name/role is inside the account menu ADR-084.
Content has page title/primary action/data. Mark selected item/focus; close
drawer on selection/backdrop/Escape and return focus to the opener.

### Role menus and login landing — FE01b / ADR-104

Accepted 2026-09-29. Existing F screens are grouped as follows; sequence also
defines the fresh-login landing (first item). Actions live inside their pages.

| Role | Menu → existing F screens |
| --- | --- |
| Student | Trang chủ F03; Khóa học & hồ sơ F04–F05; Thi đầu vào F06–F09; Hóa đơn F10; Lớp & lịch F11; Bảng điểm F12. |
| Admissions | Hồ sơ tuyển sinh F13–F14. |
| AcademicAffairs | Ca thi đầu vào F16–F17; Khóa & chương trình F18 (Band/weights); Lớp học F19–F20; Lịch học & thi F21; Xếp lớp F22; Duyệt bảng điểm F23. |
| Teacher | Lớp & lịch dạy F24; Thi đầu vào F25; Điểm danh F26; Sổ điểm F27. |
| Accountant | Hóa đơn & thanh toán F28–F29; Báo cáo học phí F30. |
| Director | Báo cáo tổng hợp F31 (admissions/training/finance sections). |
| Admin | Tài khoản F32; Nhật ký F33. |

F01/F02 precede login. Account menu retains ADR-084 actions separately.
No new F codes/staff dashboards or permissions. Later Gói 2 choices are now
accepted in ADR-106–ADR-113 below; Gói 3 is separately accepted ADR-121–ADR-129.

### Page structure and navigation — FE02 / ADR-105

Accepted 2026-09-29: separate URLs for lists/details/complex forms, supporting
direct access, reload and Back/Forward. Details have object/state summaries
and authorized sections/tabs. Complex forms use pages; confirmations, reasons
and short forms use modals, potentially almost full-screen on mobile.
Preserve list filters/page/sort on return. Only non-identifying course/Band/
state/date-period filters and page/sort may be in URLs; name/email/phone
searches stay in working memory, never URL/localStorage/sessionStorage.
Provide breadcrumbs/return paths; preserve D02 scope/ownership/error contracts.
No new routing package or deployment decision is approved.

### Complete common frontend direction — Gói 2 / ADR-106–ADR-113

Accepted 2026-09-29 by approval of the original Gói 2 FE01–FE10. Preserve
ADR-103–ADR-105 detail/PII constraints, no bottom navigation/real role switcher,
and paginate without infinite scroll. These are targets, not implemented UI.

| Item / ADR | Accepted frontend contract |
| --- | --- |
| FE03 / ADR-106 | 1280 full authorized table; 768 reduced columns/separate detail; 375 labelled cards/lists. Gradebook desktop matrix/fixed Student column, mobile per-Student. FE14 save granularity is separately accepted ADR-124 below. |
| FE04 / ADR-107 | Hide actions outside rights; disable temporarily ineligible actions with reasons; explicit read-only presentation and blocked-account warnings. Backend remains authoritative. |
| FE05 / ADR-108 | Explicit save/no autosave; client early/server final validation; inline field/top form errors; warn unsaved exit. Permitted code uppercase on blur/phone canonical after save; no truncation/rounding/password normalization/stale merge or durable sensitive drafts. |
| FE06 / ADR-109 | Initial skeleton, no-data versus no matches, disabled/progress submit; visible 401/403/404/409 feedback, fieldErrors/traceId and persistent important errors. Preserve private login errors/server replay safety. |
| FE07 / ADR-110 | One sensitive confirmation pattern with target/state/effects/required reasons/action verb; no ordinary double confirmation. One-time secret delivery FE18 is separately accepted ADR-128 below. |
| FE08 / ADR-111 | Accessible local Tailwind components; no new UI/form/table/calendar/chart libraries. Package additions require a concrete need and owner approval. |
| FE09 / ADR-112 | Vietnamese labels/internal English enums, dd/MM/yyyy/HH:mm/Vietnam time, vi-VN display. Money digit strings remain exact, existing calculation/storage contracts unchanged. |
| FE10 / ADR-113 | Light compact administrative UI, system fonts/white-gray/blue, labelled or icon status cues/clear focus/about 44 px minimum touch targets/reduced motion. Not a brand identity or accessibility pass. |

Full accepted rules: requirements section 4.6; OWNER_REVIEW keeps the original
approval rows and sole E04/E05 stage gates. The owner-requested
E04_REMAINING_REVIEW separately holds remaining package proposals/marks. Gói 3 FE11–FE19 is separately accepted below; D12/D13 deployment
choices remain deferred and E outputs unaccepted.

### Additional security and operational targets — ADR-114–ADR-120

Separately accepted 2026-09-29 from OWNER_REVIEW checks and one explicit D17
operator confirmation. Full contracts: requirements section 4.7.

| Choice | Accepted target and boundary |
|---|---|
| D05 / ADR-114 | ASP.NET Core PasswordHasher/runtime parameters; cryptographic 32-character temporary password, Admin one-time display/no plaintext storage/logs. Offline merged SecLists 10k-most-common/probable-v2_top-12000, pinned revision/license preserved/quarterly review. Finite coverage, no external password service. |
| D08 / ADR-115 | Mandatory business/account audit shares the mutation transaction; failure rolls back related changes/revocation. Pure security-event logging is best-effort, bounded secret-free fallback; no access-check bypass or undo of logout revocation. |
| D09 / ADR-116 | Mandatory audit retained with project history. Operational expiry of pure security logs/IP at 90 days, mandatory fields preserved. Needed IP only, unauthenticated actor NULL, processed events retained/rate-limit aggregates preserve counters. No app deletion or legal-compliance claim. |
| D11 / ADR-117 | One-center reference envelope: 10,000 students/50 concurrent users with retained history. No multi-branch/tenant/distribution, real-user count, measured capacity or business hard cap. |
| D14 / ADR-118 | Daily encrypted SQL backups separate from primary data drive; seven daily/four weekly copies, monthly restore trial. Reference RPO <=24 hours/RTO <=2 hours, not verified guarantees. Owner must choose destination/access before operation; laptop-off missed runs must be addressed. |
| D16 / ADR-119/162 | .NET 10 LTS/EF Core 10, installed SQL Server 2025 Express for local non-production dev/test/internal demo; accepted React stack/xUnit retained. Lock compatible supported patches after full-baseline acceptance; production edition/license needs separate choice. No setup now. |
| D17 / ADR-120 | Project owner is default sole release/secrets/backup-restore/maintenance operator; re-ask only on owner's requested change. Not app Admin/DBA rights. Setup/rights/security/recovery/traceability handoff and periodic dependency/secret/blocklist review; no automatic integration/data transmission. |

D12/D13 stay deferred until hosting/software environment is settled. Actual
tunnel provider, HTTPS/proxy topology, startup/shutdown and backup destination/
access configuration are not approved by these policies. No runtime installation,
deployment, data copy or executed verification has occurred.


### Accepted domain interactions — Gói 3 / ADR-121–ADR-129

Explicitly accepted by the owner on 2026-09-29, not inferred from Gói 2 or
blank review rows. Full canonical contracts: requirements section 4.8.
These refine the target UI/API workflow boundaries, not implemented architecture.

| Item / ADR | Accepted target contract |
| --- | --- |
| FE11 / ADR-121 | Per-own-registration cards/current step/eligible action/next event/unpaid invoice/finalized link; multiple courses keep context. Derived display, no new state, unpublished grades or notifications. |
| FE12 / ADR-122 | Scoped local week desktop/agenda mobile-tablet for F07/F11/F21/F24, date/class selection and authorized detail; no drag/resize/library, HELD inference or replacement double-count. |
| FE13 / ADR-123 | Input → concrete LESSON preview/count/conflicts → valid all-or-none commit. Input edits invalidate preview; server revalidates/409 re-preview, no resource reservation, partial save, holiday skip or auto FINAL_EXAM. |
| FE14 / ADR-124 | Explicit one-LESSON attendance batch and one-Student changed component scores/comment save, each all-or-none. Missing attendance stays unrecorded, missing score not zero; partial draft allowed, blank not automatic score deletion. Whole-class submission is separate. |
| FE15 / ADR-125 | Separate draft save from submit/resubmit/return/finalize whole class; unsaved edits block submission, mandatory return reason, server all-or-none/revalidation, finalized-only Student visibility and immutable finalization. |
| FE16 / ADR-126 | Authorized cards/group tables, explicit apply filters/queried_at and no mixed queries. Preserve metrics/cohorts/whole-set totals/non-identifying Director/no export and fresh bounded Accountant CSV; no chart package. |
| FE17 / ADR-127 | Student-owner only, fetch fresh authorized Invoice before A4 browser print; accurate state/UNPAID not payment proof. No staff print rights, extra Payment data, fake signatures, receipt or PDF API. |
| FE18 / ADR-128 | Dedicated F32 result block with email/role/temporary credential/server deadline, explicit optional copy/manual fallback/close acknowledgment. Clear plaintext state on close/exit/logout/expiry, no storage/log/cache/replay; clipboard not guaranteed cleared. Unknown response → inspect state, intentional confirmed reset if credential lost, no automatic repeat/reset. |
| FE19 / ADR-129 | Newest-first paginated event rows, authorized allowlisted old/new details and paginated operation_id links; unauthenticated actor not input email, expired IP not reconstructed. No raw payload/secrets/edit/delete/undo/export. |

No new roles/F codes/entities/packages or automated workflow. FE14 must be
reflected in final DTO/transaction design; FE18 never permits persisted or
replayed plaintext credentials. E03-v1 dictionary is accepted; API design
remains E04 work.

### Acceptance boundaries

A–C choices are accepted, not implemented. E01-v1 function and E02-v1 workflow
artifacts were accepted on 2026-09-30; complete E03-v1 dictionary/ERD was accepted
on 2026-10-01. System/API architecture and full-baseline consistency still need
E04/E05 review. D12/D13 remain deferred by
owner notes. D01–D04/D06–D07/D10/D15 are accepted in ADR-095–ADR-102;
D05/D08/D09/D11/D14/D16/D17 are accepted in ADR-114–ADR-120. Do not re-ask
accepted policies or the default operator. Gói 3 interactions are accepted
ADR-121–ADR-129; remaining E04/E05 output acceptance is still separate.
Other canonical business invariants, role scopes, score freezes, payment equality,
debt warning-only behavior and no scholarships/refunds/transfers remain in
requirements/ADRs, not implicitly removed by this consolidated target description.
No Git initialization/scaffolding/migration/code is authorized.

## Documentation locations

- Architecture and current implementation state: this file and
  `docs/CURRENT_STATE.md`.
- Product requirements: `docs/requirements.md`.
- Official report source/corrections: `docs/reference/REPORT_BASELINE.md`.
- System/API design baseline: `docs/design/SYSTEM_ARCHITECTURE_SPEC.md`
  (E04-v0.1 incomplete; presented package1 through Attendance/per-Student draft4e ADR-139–149 accepted;
  23 displayed remaining-package rows accepted ADR-150–155 in
  docs/plan/E04_REMAINING_REVIEW.md; complete E04 still unaccepted).
- Implementation-era API reference remains planned at `docs/api.md` (not created yet).
- Execution planning: `docs/plan/`.
- Owner-supplied instruction inputs awaiting or supporting adoption:
  `instructions/`.
