# Architecture

## Current implemented architecture

No application architecture is implemented yet. As of 2026-09-27, this
workspace contains requirements, agent instructions, and Project Memory only.
There is no backend, frontend, database migration, test project, or runnable
application to inspect.

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

## Target data model from the official report

This is the normalized design baseline, not deployed tables. The schema images
in the Word report are older than the field descriptions; use this model and
`docs/requirements.md`. Scholarships have been removed by owner decision.

| Entity | Attributes / responsibility |
|---|---|
| Role | role_id, role_code, role_name; seven fixed account roles. |
| Account | account_id, role_id, email, phone, password_hash, full_name, status, failed_login_count, locked_until, created_at. |
| StudentProfile | student_id, account_id, date_of_birth, address; one profile per student account. |
| Course | course_id, course_code, course_name, base_price, status. |
| Band | band_id, band_code, min_score; minimum threshold only, no max_score. |
| TrainingProgram | program_id, course_id, band_id, program_name, status. |
| ScoreComponentConfig | component_id, program_id, component_name, weight. |
| Registration | registration_id, student_id, course_id, status, admission_confirmed_at, admission_approved_by, admission_approved_at, created_at. |
| EntryTestSession | session_id, start_at, end_at, room, capacity, grader_id, status. |
| EntryTestAttempt | attempt_id, registration_id, session_id, attempt_number, status, score, graded_by, graded_at, registered_at. |
| BandAssignment | band_assignment_id, registration_id, best_attempt_id, band_id, assigned_at; current best completed attempt for the same record. |
| Class | class_id, program_id, teacher_id, class_code, status, max_size. |
| StudentClassAssignment | class_assignment_id, class_id, registration_id, assigned_at; enrollment-to-class link. |
| Schedule | schedule_id, class_id, session_type, session_date, start_time, end_time, room; LESSON or FINAL_EXAM. |
| Attendance | attendance_id, schedule_id, class_assignment_id, present. |
| Score | score_id, class_assignment_id, component_id, score_value, entered_by, entered_at. |
| GradeReport | report_id, class_assignment_id, final_score, status, teacher_comment, submitted_by, submitted_at, reviewed_by, reviewed_at, returned_reason, finalized_at. |
| Invoice | invoice_id, invoice_number, registration_id, amount, issued_at, due_date, status; amount snapshot at issuance. |
| Payment | payment_id, invoice_id, amount_paid, method, transaction_reference, confirmed_by, paid_at. Confirmation time only, no separate actual-receipt time (ADR-046). |
| AuditLog | audit_id, actor_account_id, action, entity_type, entity_id, details, occurred_at, ip_address. |

Main dependencies: Account → Role; StudentProfile → Account; Registration →
StudentProfile/Course; TrainingProgram → Course/Band; Class → TrainingProgram/
teacher Account; StudentClassAssignment → Registration/Class. There is no
direct StudentProfile-to-Course or class-assignment-to-profile foreign key.

### Integrity and transaction boundaries

- ADR-031: authorized Registration cancellation cancels active pre-start bookings
  and releases capacity even in CLOSED; leave session/other bookings unchanged.
  Commit Registration/any issued UNPAID Invoice/future booking changes together,
  tracing dependent cancellations to the parent action and storing actor/time/
  prior state/reason where required. Preserve started booking/attempt history and
  existing scores/ABSENT/Band; stop all result mutations on CANCELLED Registration,
  including initial score and absence entry in CLOSED. Ungraded ended history is
  a display condition, not a new enum or fake result. A new Registration does not
  reuse scores/Band. Guard cancellation against booking/start/grading/confirmation/
  approval/payment races; detailed history/audit representation remains Q-004/Q-007.
- ADR-030: sessions start CLOSED; AcademicAffairs opens/reopens only before
  start with valid time/room, ACTIVE assigned Teacher, no resource conflicts
  and active bookings <= capacity. Full OPEN sessions stay OPEN. CLOSED blocks
  student booking/cancellation/change; keep bookings and teacher grading,
  ADR-027 absence and ADR-029 correction rights. At start enforce effective
  CLOSED regardless of stale stored status; never reopen CANCELLED or started
  sessions. Grader replacement requires AcademicAffairs/reason/old-new actor-time
  audit, pre-start and no score/ABSENT, ACTIVE teacher and schedule checks.
  Guard against racing bookings, closure/start, grading and resource assignments.
  Booking/history/audit representation and closure persistence remain design work.
- ADR-029 permits COMPLETED entry-score correction only by the original grader
  still assigned to that session and only in TESTING/BAND_ASSIGNED. Require
  reason, old/new score, actor/time; atomically recalculate highest score/Band
  with frozen thresholds. Guard against simultaneous confirmation/cancellation/
  grading. CONFIRMED/ADMITTED/CANCELLED/REJECTED entry results/Band are locked,
  with no Admin/AcademicAffairs override. Equal highest COMPLETED scores select
  the lower attempt_number for best_attempt_id, retaining both attempts/results.
  Exact audit-history storage and transactional enforcement await database design.
- ADR-028 freezes program course/Band/components/weights once any Class uses
  it, including later-cancelled classes; freeze the whole Band threshold set
  after the first completed entry test. Enforce against concurrent first-use
  and configuration edits. No version entity is authorized in this scope.
  Entry/component scores and weights accept at most two decimals, rejecting
  excess precision. Use decimal arithmetic, no contribution-level rounding;
  round final totals once half-up to two decimals for storage/display/passing.
  Entry Band comparison does not round scores to integers. Exact database
  numeric types remain for the database design stage.
- ADR-027: assigned grader records absence only after session end, not inferred
  from missing score. AcademicAffairs cancels non-cancelled sessions only before
  start with reason, cancelling active bookings but preserving history and
  unfinished logical-attempt eligibility. No automatic reassignment/staff booking.
  Date/time/room edits require pre-start and no booking history; otherwise use
  cancellation/new session. Guard mutation versus booking/grading/start-time
  races; CANCELLED sessions cannot be graded. Cancellation attribution/history
  fields still need database design; grader replacement and closure are ADR-030.
- ADR-026 permits AcademicAffairs cancellation only of PLANNED/OPEN classes
  with no assigned students and no teaching begun, with a mandatory reason.
  Block cancellation against racing placement; no transfer/refund workflow.
  Preserve class/schedule history, but CANCELLED-class schedules no longer
  reserve teacher/room time, appear as upcoming teaching or count as completed
  sessions. Class cancellation attribution and schedule representation remain
  database-design work; no additional Schedule enum is implicitly accepted.
- ADR-025 makes AcademicAffairs responsible for explicit class transitions:
  PLANNED → OPEN requires ACTIVE program, assigned teacher and at least one
  conflict-free future LESSON; OPEN → ONGOING requires actual teaching start,
  not before first LESSON, and at least one valid placed student. ONGOING →
  COMPLETED requires confirmation every scheduled LESSON/FINAL_EXAM occurred
  and all LESSON attendance is recorded, with ADR-036's non-held LESSON refinement:
  keep the old schedule as history; its linked replacement must occur with
  attendance instead, without counting the pair twice. No automatic date-based transitions
  or submitted/finalized-grade prerequisite. Guard these checks against racing
  schedule/placement/attendance changes. Actual-session occurrence storage,
  full schedule/expected-count policies are ADR-032; cancellation
  rights/history/scheduling effects are resolved by ADR-026.
- ADR-024 allows draft scoring during teaching but submission only for
  COMPLETED classes with all required component scores. AcademicAffairs
  finalizes only SUBMITTED reports from COMPLETED classes after review.
  Teacher edits to component scores/comments require DRAFT or RETURNED;
  SUBMITTED is frozen until reasoned return, FINALIZED is immutable. Protect
  score edits and report transitions against races; UI-only checks are insufficient.
- ADR-034 requires whole-class submission/resubmission, reasoned return and
  finalization, with all-or-none report transitions and simultaneous publication.
  Keep per-student GradeReports and individual draft/correction entry; group
  persistence/constraints and audit representation remain later design work.
- Passing derives from FINALIZED final_score >= 50, not draft results,
  attendance or component minima. Additional classification bands are out of
  scope (ADR-024). Compare the stored two-decimal rounded final total (ADR-028);
  pass-rate cohort follows ADR-067 (FINALIZED enrollment results by finalization date);
  do not change completion eligibility under ADR-022 or add a new grade state.
- ADR-023 retains Registration ADMITTED after placement; derive placement
  display from StudentClassAssignment, not a new registration state. Only OPEN
  classes accept new placement, with matching course/Band, available capacity,
  ADMITTED Registration and PAID Invoice. Guard placement against concurrent
  class-state changes as well as competing capacity reservations.
- AcademicAffairs may replace the responsible teacher only before ONGOING/
  teaching begins and before attendance/grades exist. Check every existing
  class schedule against the new teacher's class/test assignments (ADR-023).
  No teacher-change history workflow is authorized; class transitions follow
  ADR-025. Actual-session representation remains Q-008; ADR-040 excludes student
  class transfers, even before teaching, with no remove/reassign bypass.
- ADR-021 separates logical EntryTestAttempt (at most two per Registration)
  from historical session bookings. The listed single session_id/status per
  attempt is insufficient to assume full rebooking/absence history is preserved.
  Review history representation and related FKs in database design; no new
  history table/entity count has been accepted yet. Do not overwrite old session
  data or create attempt three to accommodate a rebooking.
- Entry-test capacity must count active bookings, not all historical absent/
  cancelled bookings. Booking changes need consistent capacity release/reserve
  and guards against racing confirmation, grading or session start. Final
  transaction/constraint design remains part of the full design review.
- ADR-020 requires protecting the actor Admin and last active Admin from
  manual lock/disable, including concurrent requests. ACTIVE is required for
  access; LOCKED/DISABLED preserve all business history. Business revocation and
  timed throttling follow ADR-049/052; normal lifetimes follow ADR-055 and
  mechanisms remain security design.
- ADR-015 requires at most one unfinished Registration per student/course,
  while preserving repeat enrollment. ADR-022 defines ended records as
  CANCELLED/REJECTED or assigned Class COMPLETED AND that student's GradeReport
  FINALIZED, regardless of passing. ADMITTED alone is insufficient. Encoding
  this cross-entity predicate and concurrency/index design remains Q-005;
  do not add permanent pair uniqueness or silently add a COMPLETED state.
- ADR-022 requires Student ownership/pre-CONFIRMED cancellation and reasoned
  Admissions cancellation of CONFIRMED or ADMITTED with UNPAID Invoice. Cancel
  any issued UNPAID Invoice atomically with Registration and guard against
  concurrent payment. Keep prior confirmation/approval/history. Cancellation
  reason/actor/time fields await database design; booking effects follow ADR-031;
  no paid-record cancellation/refund workflow is authorized.
- ADR-017 requires a student-visible rejection reason and traceable rejection
  action. The Registration attributes above are not yet complete for this;
  finalize fields, actor/time storage and constraints during database design.
- Profile-edit authorization follows ADR-016: Student ownership, no Admissions
  editing; confirmed registration Course cannot change. ADR-019 permits edits
  to name/birth date/phone/address, not student self-change of login email.
  ADR-038 locks the selected Course on every existing Registration; valid
  cancellation/new registration preserves old history and starts testing anew.
  ADR-039 excludes personal-profile snapshots: historical records display current
  personal information, without changing Invoice amount snapshots or business audits.
- ADR-019 requires student name/birth date/email/phone and optional address;
  login uses email/password. Final schema constraints must enforce student
  requirements without making unapproved staff fields universally mandatory.
- Unique account email and phone when present; one StudentProfile per account.
- ADR-073/074/075/076/077 constrain profile entry: trimmed Unicode names 1–100,
  real Student DOB not after current Vietnam date/no age restriction, trimmed
  format-valid email <=254 with account-wide case-insensitive uniqueness/no
  provider rewriting, Vietnamese-mobile phone scope (0 + nine digits/equivalent
  +84 form; strip accepted spaces/hyphens and compare canonically), optional
  trimmed Unicode Student address <=255/free text. No verification/OTP or new
  staff required fields/editing rights. Exact character counting/validators,
  consistent email login comparison, canonical persistence/null/SQL types await
  database/architecture design; do not silently truncate accepted fields.
- Unique course/class codes, band codes/minimums, and invoice numbers.
- ADR-078 bounds trimmed Unicode Course/program names 1–150/component names
  1–100, retaining component-name uniqueness per program without new global name
  uniqueness. ADR-079 Course/Class codes are manually entered/trimmed/uppercased,
  1–30 A–Z/digits/_/-, immutable after creation and unique including ended history.
- ADR-080 uses the same trimmed/uppercased 1–30 A–Z/digits/_/- room codes across
  class/test conflict checks; no automatic alias equivalence or room catalogue.
  ADR-081 reasons are plain-text trimmed Unicode 1–1000 only where required;
  optional comments <=1000, line breaks retained. ADR-082 trimmed Payment references
  <=100 preserve content/case, transfer required/cash optional, no global unique
  key. Preserve all prior freezes/permissions/Invoice-Payment constraints; exact
  counting/validators/storage/rendering and code-generation for IDs/Invoices remain
  later design. No code/schema is introduced.
- Unique (course_id, band_id) program in the report baseline; configuration
  is frozen after first Class use (ADR-028). Versioning is not in current scope;
  changing used configuration requires an explicit design/uniqueness decision.
- Unique (program_id, component_name), (registration_id, attempt_number),
  (schedule_id, class_assignment_id), and (class_assignment_id, component_id).
- One current BandAssignment, class assignment, and Invoice per Registration;
  one GradeReport per class assignment; one Payment per Invoice.
- Attendance must join the same class as its LESSON; score components must
  belong to that class's program. Grading/approval/payment actors must have
  the appropriate role and assignment, not just a valid Account FK.
- Admission and invoice creation are one transaction. Payment insertion and
  invoice PAID update are one transaction. Capacity checks must include
  concurrency control, not an unprotected count followed by insert.
- Scheduling checks span classroom lessons/exams and entry-test sessions.
- Weekly schedule entry generates concrete dated Schedule records; individual
  additions/edits also require conflict checks. Rule persistence remains open;
  edits to sessions' date/time/room follow ADR-032: pre-start, no attendance/related
  scores and non-COMPLETED/non-CANCELLED class. Program-level positive LESSON
  count freezes after first Class use; full LESSON schedule is required before
  teaching start, not opening. Count field and score-session dependency
  representation await database design; no schema attributes are added here.
- ADR-035 keeps non-held class-wide lessons within existing scheduling, without
  a holiday-calendar module/automation or individual makeup workflow. It adds
  no entity/enum and does not relax ADR-032. ADR-036 resolves the combined
  pre-start move/post-start linked replacement workflow and LESSON counting;
  data/history/actual-occurrence representation remains Q-011/Q-008.
- ADR-036's post-start non-held LESSON operation requires AcademicAffairs and
  reason, a non-ended class and no old attendance/related scores. Preserve the
  original schedule/actor/time and create a linked same-class replacement using
  the current teacher and normal conflict checks; commit the required changes
  together, guard races/repeated requests and do not duplicate a teaching
  obligation. Storage/constraints/audit are not finalized; no schema attributes
  or Schedule enum are added. ADR-037 excludes extra non-replacement LESSONs.
- ADR-037 caps LESSON teaching obligations at the program's fixed expected
  count, not the raw number of historical Schedule rows. Weekly/individual
  additions fill the required timetable, never extra obligations. Linked
  replacement preserves one obligation; FINAL_EXAM is separate. Count/history/
  linkage and concurrency constraints remain database design, with no new
  attributes or permission override introduced here.
- Only COMPLETED attempts contribute to Band. Program weights total 100 and
  all scores use 0–100. Finalization requires complete scores and review data.
- FK/unique/range rules belong in database constraints; workflow, aggregate
  totals, overlap checks, and role/ownership checks also need service logic.
- ADR-041 permits only Student-owned Invoice browser printing with accurate
  amount/payment state, no dedicated application PDF download/export or separate
  receipt issuance. Report-export permissions are not broadened.
- ADR-042 permits Accountant correction only of confirmed Payment method/reference,
  with mandatory reason and old/new/actor/time audit. Amount, Invoice link and
  confirmation time remain immutable; retain same Payment and Invoice PAID.
  Transfer requires a reference. Correction/audit must be atomic and safe against
  concurrent changes; no Payment cancellation/deletion/refund or second Payment.
  These are target contracts, not implemented schema/API behavior.
- ADR-043/044 require positive tuition/Invoice/Payment amounts in VND whole dong;
  reject fractional money, retain exact snapshots/totals and full-payment equality.
  No free-course/zero-value payment path; final numeric types/ranges await database
  design. This does not change grade precision under ADR-028.
- ADR-045 fixes due_date at the local issue date plus seven calendar days,
  including holidays, with no separate extension/edit workflow. UNPAID is overdue
  only after that date; warnings only. ADR-046 keeps paid_at as confirmation time,
  no separate actual-receipt timestamp or backdating right.
- ADR-047 applies Asia/Ho_Chi_Minh (UTC+7) to business dates/schedules, Invoice
  date/deadline comparisons and date-based report filters independent of device
  timezone. Storage/serialization, date-filter predicates and platform timezone
  mapping remain architecture-stage work; no new schema or runtime code.
- ADR-048 gates business access until temporary-password change after staff
  creation/Admin reset. Show the random temporary password once for manual delivery,
  never plaintext in logs/audits, store only hashes; reset does not unlock/enable.
  Temporary expiry is 24 hours under ADR-054; restricted-authentication and field
  representation are not finalized.
- ADR-049 requires invalidating every existing session/current session/unexpired
  JWT on manual lock/disable/reset/own password change. Validate revocation beyond
  the UI; unlock/reactivation must not revive old tokens. No token persistence
  model is accepted yet; request enforcement and races await design. Normal
  lifetimes and session scope follow ADR-055–ADR-057.
- ADR-050 permits Admin DISABLED → ACTIVE on the same account, preserving role/
  history/data, no restored sessions. ADR-051 excludes changes to existing login
  email, including Admin/staff; creation validation and account history remain.
- ADR-052 blocks new login fifteen minutes after five consecutive incorrect
  passwords within fifteen minutes, resetting the counter/window on success/expiry.
  Do not change Account status or revoke sessions for failed-login throttling alone.
  Manual LOCKED/Admin unlock and DISABLED remain separate. Counter/window/timer,
  privacy and concurrency design remain open; no entity/enum/migration/code added.
- ADR-053 requires 15–128-character Unicode/space-friendly passwords without
  mandatory composition or routine rotation; reject common/compromised values.
  Exact counting/denylist/hash design remains open; no external password service
  or package is authorized. ADR-054 expires temporary credentials after 24 hours
  from issue/reset, requiring Admin reset without account-state changes.
- ADR-055 requires server-enforced normal-session expiry at login +2 hours or
  idle +30 minutes, whichever first; activity never extends the absolute limit.
  No refresh/remember-me. Activity definition/background requests, restricted-
  authentication lifetimes and state/request/race handling remain design work.
- ADR-056 allows concurrent devices without kicking out prior sessions on login.
  ADR-057 revokes the current authentication session server-side on logout,
  including unexpired tokens; other sessions retain their deadlines/ADR-049
  revocation. No logout-all/device-management/session-list feature or new
  session entity/schema/API implementation is implied.
- ADR-058 extends mandatory business audits to successful configuration changes,
  class lifecycle and schedule creation/moves/replacements, not every ordinary
  read. ADR-059 adds login successes/failures/timed-block activation/logout/
  password/account operations/denied authorization. Distinguish unauthenticated
  identifiers from verified actors; no guessed-email actor attribution.
- ADR-060 requires Admin-only structured filters by business dates/actor/action/
  entity type-ID/result and newest-first pagination. ADR-061 requires relevant
  event details/mandatory old-new history, no password/hash/JWT/session secret or
  full request/response payload in logs. Minimize personal data without erasing
  required business history or adding profile snapshots.
- ADR-062 excludes audit CSV/PDF export. Event/result/actor representation,
  field allowlists/sanitization/IP handling, pagination tie-breakers, required
  audit transactional enforcement, security-log failure/anti-flooding, retention/
  disposal and backups remain database/architecture work. No new enum/entity,
  class-teacher history workflow, purge/alert module or code is introduced.
- ADR-063 evaluates current report states, excluding historical as-of reporting
  without removing mandatory history/snapshots. ADR-064 separates issuance-period
  non-CANCELLED billing (UNPAID + PAID), confirmation-period revenue and current
  UNPAID debt, not a subtraction across different cohorts.
- ADR-065 requires distinct student counts inside each Course group and distinct
  overall counts, not summed groups; Registration count remains separate.
- ADR-066 progress counts confirmed-held LESSON obligations/program expected
  count; non-held originals excluded/replacement once, FINAL_EXAM separate.
  Occurrence representation remains Q-008; cancelled classes retain state counts
  without delivered progress. ADR-067 pass rate counts FINALIZED enrollment
  results by finalization date, not distinct people or class-completion dates;
  empty denominator is no data. Query consistency/date predicates, numeric
  display and interfaces await later design;
  no report code, new field/state or historical snapshot module is introduced.
- ADR-068 defaults admissions reporting to current Vietnam month/all states,
  retaining inactive historical course choices. ADR-069 separates date-free
  current training inventory (Course/Band/Class/current Teacher/state) from
  finalization-period results, default current month. ADR-070 defaults financial
  flow periods to current month, consistently filters Course, and provides
  current-debt overdue-only filtering without a date period.
- ADR-071 limits Director report drill-down to applicable non-identifying group
  aggregates, no individual registrations/Invoices/PII, export or approval.
  Accountant operational invoice lookup is unchanged. ADR-072 financial CSV
  exports all matching report results/columns, no hidden/profile fields or
  separate record dumps. No new staff report rights. Query/export consistency,
  file layout/limits/encoding/injection safeguards remain architecture design.
- No physical deletion of records with dependent business history; no normal
  business UI for editing/deleting AuditLog.

SQL types, nullability refinements, snapshot/version rules and undefined
transitions must be resolved during full design agreement, before discussing
implementation planning. Outstanding product questions are tracked in
`docs/plan/questions.md`; the active design plan is `docs/plan/design-baseline.md`.
The model above is a target, not a finalized or implemented database schema.
EF entities/migrations will become the implemented schema of record once verified.

## Documentation locations

- Architecture and current implementation state: this file and
  `docs/CURRENT_STATE.md`.
- Product requirements: `docs/requirements.md`.
- Official report source/corrections: `docs/reference/REPORT_BASELINE.md`.
- API contracts after implementation begins: `docs/api.md` (not created yet).
- Execution planning: `docs/plan/`.
- Owner-supplied instruction inputs awaiting or supporting adoption:
  `instructions/`.
