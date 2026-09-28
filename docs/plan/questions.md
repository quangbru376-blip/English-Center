# Open Planning Questions

Agents must not answer these by assumption. The owner should decide, then the
answer should be moved into `docs/requirements.md` and an accepted ADR when
significant. The six owner resolutions from 2026-09-27 are already recorded
there; do not ask them again. Only the remaining items below are open; sections
marked resolved are handoff pointers, and new proposals are not accepted rules.
Independent design analysis may continue, but in-scope
business questions must be resolved before accepting the complete design
baseline. Implementation planning comes after that acceptance; see
`design-baseline.md`. Q-001 and Q-002 are deferred implementation-planning
questions, not prerequisites for the current business-design discussion.

Owner review updated 2026-09-28: all known remaining decisions are consolidated
in `OWNER_REVIEW.md` (50 unaccepted choices and 5 gated output reviews). That is
the single editable four-column approval surface; this file retains gap/source
tracking, not a second set of approval boxes. All initial boxes are unchecked;
read owner marks/alternatives before recording accepted answers. Existing ADRs
remain fixed, and unseen stage outputs cannot be approved in advance.

## Q-001: What are the four implementation phases?

`instructions/english-center-agent-kit/AGENTS.md` says the 33 screens are built
in four phases, but no phase boundaries are present in the available files.

## Q-002: What exactly is included in pilot Slice 0?

The project-planning skill says "login + role-based access + Student
list/create". The screen requirements define student self-registration (F02),
while F32 is described as staff account management. Confirm which actor and
screen own "Student list/create" before a slice contract is written.

## Q-003: What is the contract of each future agent integration?

Multiple skills and agent integrations may coexist. For each newly supplied
integration, record its delegated task types, file ownership, handoff format,
verification responsibility, and collision rule before using it on overlapping
work. This is a registration question, not a requirement to choose one global
workflow.

## Q-004: How are accepted rebooking rules and remaining test exceptions modeled?

Partially resolved by ADR-021: student cancellation/change before session start
and confirmation consumes no extra logical attempt; preserve history. Absence
has no score and can be rebooked within the same unfinished attempt. Retake two
still requires first score. Scheduled/taken-ungraded retakes block confirmation;
only eligible unstarted bookings may be cancelled to confirm with the existing
result. ADR-030 additionally requires the source session OPEN.

ADR-027 resolves absence attribution and center cancellation/rescheduling:
assigned grader records confirmed absence after session end, never automatically
from missing score. AcademicAffairs cancels only before start with reason,
cancelling active bookings and preserving history/unfinished attempts. Students
self-rebook into eligible OPEN sessions with capacity, no extra attempt or staff
booking/automatic reassignment. Date/time/room changes require pre-start and
no booking history; otherwise cancellation/new session. No post-start session
cancellation/rescheduling, score deletion or grading of CANCELLED sessions.

ADR-030 resolves opening/closure and grader replacement: create CLOSED;
AcademicAffairs opens/reopens before start with valid resources/ACTIVE Teacher/
capacity checks, closes while retaining bookings; full sessions stay OPEN.
CLOSED blocks every student booking mutation, including cancellation/change.
At start enforce effective CLOSED and no reopening. Grading/ADR-027 absence/
ADR-029 correction remain available. Grader replacement only by AcademicAffairs
before start and any score/ABSENT, with reason/old-new actor-time audit and
ACTIVE Teacher/class-test conflict checks; bookings unchanged. CANCELLED cannot
reopen or be graded; pending-retake confirmation guards remain intact.

Remaining: booking-history representation versus logical-attempt state/FKs,
cancellation/grader-change audit fields and effective-closure persistence.
Finalize concurrency/capacity design before implementation. Registration-to-
booking cancellation effects are ADR-031; audit representation remains Q-007.
Do not re-ask ADR-021/027/029/030/031 or impose
absence penalties, rebooking-count limits, zero score or attempt three.

## Q-005: How is the accepted enrollment uniqueness rule represented?

Business rules resolved by ADR-015/ADR-022: one unfinished record per
student/course; different-course records may coexist. CANCELLED/REJECTED or
assigned Class COMPLETED AND the student's GradeReport FINALIZED end the record
for new enrollment, regardless of passing; preserve old records. Remaining:
database representation and enforcement of this cross-entity rule under concurrency.
Registration currently has no COMPLETED state; do not assume ADMITTED means
course completion or silently add a state.

## Q-006: How are grading configuration and Band history preserved?

Configuration history and precision/rounding resolved by ADR-028:

1. Freeze a TrainingProgram's course/Band association, score-component list and
   weights once any Class references it, including a subsequently cancelled
   class. Before that use, AcademicAffairs may configure components/weights;
   activation still requires total weight exactly 100. Keep historical scores
   reproducible without a versioning module. Changing a used configuration
   would require a separately agreed version/snapshot design, not silently
   creating another program that violates the current course/Band uniqueness.
2. Keep the five seeded Band thresholds 0/25/45/65/85 and freeze the whole
   threshold set after the first completed entry-test result. Do not recalculate
   existing placement history from later configuration changes. A student's
   eligible second result still updates best score/Band using the same set,
   before confirmation. Threshold versioning is not proposed in this scope.
3. Accept entry/component scores and weights with at most two decimal places;
   reject excess precision rather than silently rounding input. Calculate the
   weighted total with decimal arithmetic, without rounding each contribution;
   round once to two decimals, half-up (positive midpoint away from zero),
   for storage/display/pass comparison. For example 49.995 becomes 50.00 and
   passes after finalization; 49.994 becomes 49.99 and fails. Entry-test Band
   comparison uses the accepted entry score, with no integer rounding.

Entry-score correction and tied-best attribution resolved by ADR-029:

1. Permit correction of a COMPLETED entry-test score only by the teacher who
   entered it and remains assigned to that session, while Registration is
   TESTING or BAND_ASSIGNED. Require a reason and record old/new score, actor
   and time; recalculate highest score/Band atomically using frozen thresholds.
   Preserve both logical attempts and do not consume an extra attempt.
2. Freeze entry scores and assigned Band after admission confirmation; no
   correction of CONFIRMED/ADMITTED or ended CANCELLED/REJECTED records, and no
   Admin/AcademicAffairs override in this scope. Protect against simultaneous
   confirmation/correction. This does not freeze editable personal information.
3. If both COMPLETED attempts have the same highest score, select the lower
   attempt_number (attempt one) for best-attempt attribution; retain and display
   both results. Tied scores yield the same Band, not a new attempt or rule.

Q-006's business policies are resolved; do not re-ask them. Exact audit-history
representation, numeric types and concurrency enforcement belong to the later
database design stage; no schema/version entity is authorized now.

## Q-007: What are the cancellation rules?

Registration cancellation resolved by ADR-022: Student owns the record and
may cancel before CONFIRMED; Admissions may cancel CONFIRMED or ADMITTED with
UNPAID Invoice, mandatory reason. Cancel any issued UNPAID Invoice atomically;
no paid-record cancellation/refund workflow. Preserve confirmation/approval
history. Finished-record eligibility is resolved by ADR-022 (see Q-005).
Rejection remains distinct under ADR-017: Admissions, only CONFIRMED, mandatory
student-visible reason; never after approval/payment.

Class cancellation resolved by ADR-026: AcademicAffairs only, PLANNED/OPEN,
no assigned students/teaching begun, mandatory reason. No ONGOING/COMPLETED or
populated-class cancellation, no transfer/refund workflow. Preserve history;
CANCELLED-class schedules do not reserve resources, show as upcoming teaching
or count as completed. Paid records waiting for placement remain unaffected.

Dependent-booking/history policies resolved by ADR-031:

1. When an authorized Registration cancellation under ADR-022 succeeds, cancel
   its active bookings in sessions that have not started, releasing capacity in
   the same transaction as Registration/any UNPAID Invoice cancellation. Apply
   this dependent effect to both OPEN and pre-start CLOSED sessions; preserve
   session state and other students' bookings. This is the accepted
   Registration-cancellation effect, not student permission to cancel/change
   an individual CLOSED booking under ADR-030. Show affected bookings before
   cancellation confirmation. Do not allow rebooking on the ended Registration.
2. For sessions already started, preserve booking/attempt history and existing
   COMPLETED/ABSENT results. Do not cancel an already-taken ungraded booking,
   erase scores, assign zero/ABSENT from missing scores or reopen old results.
   After Registration CANCELLED, stop further score entry/correction and absence
   recording for that record; show ungraded history as ended with the cancelled
   Registration, rather than actionable grading work. Exact storage/display
   representation belongs to database design; no new attempt status is accepted
   by this decision. Preserve any previously assigned Band. New enrollment uses
   a new Registration and does not copy old scores/Band. ADR-022 re-enrollment
   eligibility remains intact, as do ADR-021 pending-retake confirmation guards
   for continuing registrations; cancellation never confirms the old record.
3. Preserve cancellation actor/time, prior state and the reason where already
   mandatory under accepted actor rules; trace dependent booking cancellations
   to the Registration cancellation. Guard the combined operation against racing
   bookings, session start, grading, admission confirmation/approval and payment.
   If any required update fails, commit none of the cancellation changes.

Q-007's business policies are resolved by ADR-022/026/031; do not re-ask them.
Remaining: cancellation actor/time/prior-state/reason fields, parent-action links
and historical ungraded display/transaction enforcement in database design.
Ungraded history displays "Hồ sơ đã hủy — chưa có kết quả"; it is not a new
attempt enum. No score deletion, paid-invoice cancellation, refunds or automatic
debt penalties are authorized.

## Q-008: What are the remaining class lifecycle prerequisites?

Placement and teacher-change rules resolved by ADR-023: keep ADMITTED and
derive placement from StudentClassAssignment; place only into OPEN with
matching course/Band, capacity, ADMITTED record and PAID Invoice. No late
placement. AcademicAffairs changes the teacher only before ONGOING/teaching
begins and before attendance/grades, checking all existing schedules against
the new teacher's class/test assignments. No teacher-change history workflow.

Class transition rules resolved by ADR-025: AcademicAffairs explicitly opens
when program ACTIVE, teacher assigned and at least one future conflict-free
LESSON exists; starts when actual teaching begins, not before first LESSON,
with at least one valid placed student; completes after confirming all scheduled
LESSON/FINAL_EXAM occurred and all LESSON attendance is recorded. No automatic
date-based transition or submitted/finalized-grade prerequisite.
ADR-036 refines the completion set for non-held LESSONs: keep the original as
history and require its linked replacement to occur with complete attendance,
not the non-held original or two counted LESSONs for that obligation.

Remaining: representation/verification of actual session occurrence, not just
elapsed scheduled time. Full schedule/expected-count rules are ADR-032;
cancellation policy ADR-026 with attribution data in Q-007; transfers excluded
by ADR-040.
Do not re-ask accepted lifecycle rules.

## Q-009: When may grades be submitted/finalized, and what is passing?

Resolved by ADR-024: draft scoring during teaching; submission only after Class
COMPLETED with all required scores; AcademicAffairs finalizes SUBMITTED reports
from COMPLETED classes after review. Teacher score/comment edits only in
DRAFT/RETURNED; SUBMITTED frozen until reasoned return, FINALIZED immutable.
Passing is finalized final_score >= 50/100, with no attendance/component minima
or extra classification bands. Completion/re-enrollment remains independent
under ADR-022. Do not re-ask these policies. Remaining related details are
Q-012 (pass-rate cohort); ADR-028 resolves rounding before the passing
comparison. ADR-034 resolves mandatory whole-class submission/resubmission,
return and finalization; do not re-ask individual versus whole-class scope.

## Q-010: What operational defaults and field constraints are required?

Student field requirements resolved by ADR-019: name, birth date, email and
phone required, address optional. Profile-validation policies are resolved by
ADR-073–ADR-077; remaining-field policies are resolved by ADR-078–ADR-082.
Exact validators/counting/storage remain technical design.
Money/deadline/business-time rules resolved by ADR-043–ADR-047:

1. Tuition/Invoice/Payment amounts are positive; no free-course/zero-value path.
2. VND whole-dong amounts, no fractional dong; monetary scale conflict resolved
   in favor of whole dong, not the report's two decimals. Final SQL types/ranges
   remain database-stage work; grade precision is unchanged.
3. due_date is the local issue date plus seven calendar days, including holidays;
   overdue only after that date while UNPAID, warning-only. No deadline extension
   or editing workflow.
4. paid_at is confirmation time only; no separate actual-receipt timestamp or
   backdated confirmation. Revenue basis and ADR-042 immutability unchanged.
5. Asia/Ho_Chi_Minh (UTC+7) for business dates/schedules/deadlines/report filters;
   timestamp storage/serialization remains architecture-stage work.

Do not re-ask these five. Other catalogue/code/room/reason/comment/reference
policies are accepted below; technical validation/storage details remain.

Profile-validation batch accepted 2026-09-28 — ADR-073–ADR-077:

1. Name: trim; 1–100 characters, allow Unicode/diacritics and name punctuation,
   no two-word/ASCII-only rule. Apply wherever full_name is entered, without new
   staff editing rights (ADR-073). The 200-character alternative was not selected.
2. Student birth date: real date, not after today's Vietnam business date,
   no minimum/maximum age (ADR-074), rather than age-restricted admission.
   Do not require staff DOB.
3. Creation email: trim/validate format, at most 254 characters, case-insensitive
   uniqueness across accounts. No provider-specific dot/+tag rewriting, email
   verification or recovery (ADR-075), rather than case-sensitive uniqueness. Existing
   email remains immutable; exact validator/counting remains technical design.
4. Contact phone: accepted Vietnamese mobile-number scope, ten digits starting
   with 0; accept equivalent +84 form and spaces/hyphens, normalize before
   account-wide uniqueness checks when present. No OTP/ownership verification.
   International/fixed-line support was not selected (ADR-076).
   This is a project limit, not a claim about all Vietnam phone numbers;
   no carrier-prefix catalogue is assumed.
5. Optional Student address: trim, at most 255 Unicode characters/free text,
   empty allowed, no province/district catalogue (ADR-077), rather than 500 characters.
   No staff address requirement or personal-profile snapshot.

Do not re-ask these five or required/editable fields/email immutability. Exact
validators/counting/storage/SQL types remain technical design. Remaining-field
policies are accepted below; Q-011 operational viewing/UI placement/history
and catalogue lifecycle now await owner review in `OWNER_REVIEW.md`, A01–A09.
Stage 1 remains doing; the old discussion estimate in design-baseline is
historical, not a fixed number of unresolved requirements.

Remaining-field batch accepted 2026-09-28 — ADR-078–ADR-082:

1. Catalogue names: trimmed Unicode Course/TrainingProgram names 1–150 characters;
   score-component names 1–100. Do not require global unique Course/program names;
   retain the accepted component-name uniqueness inside one program. Existing
   first-use configuration freezes still apply; no extra name-editing permission.
   ADR-078; the 255-character alternative was not selected.
2. Course/Class business codes: entered manually, trim and uppercase; 1–30 ASCII
   letters A–Z/digits/underscore/hyphen, no spaces or diacritics. Retain unique
   codes within each entity type, including inactive/cancelled records; require
   immutability after creation for stable references. Example: IELTS-B1-01.
   ADR-079; system-generated codes were not selected. This is not a new ID/Invoice
   number algorithm or a change to program/Band configuration freezes.
3. Room labels: use consistently entered room codes, 1–30 A–Z/digits/underscore/
   hyphen, trim and uppercase, no spaces/diacritics. Apply the same normalization
   to class and entry-test resource checks: p101/P101 are the same label. Do not
   treat different labels/aliases as the same physical room automatically; staff
   must consistently use one room code. No new room-management module.
   ADR-080; a managed room-selection catalogue was not selected.
4. Reasons/comments: trimmed Unicode plain text, retain line breaks. Reasons
   1–1000 characters only where already required; Teacher comments/correction
   notes optional, at most 1000 characters. Do not require reasons on additional
   operations or comments on every result; no rich-text/HTML editor (ADR-081).
   The 2000-character alternative was not selected. Existing result locks remain.
5. Payment.transaction_reference: trim, at most 100 characters, preserve remaining
   content/case/punctuation, no provider-specific pattern or global uniqueness
   requirement. Transfer still requires a nonempty reference; cash may omit it.
   Keep one Payment per Invoice/full-payment/confirmation and correction audits;
   reference alone is not a new deduplication key or proof of receipt (ADR-082).
   Globally unique references were not selected. No new bank identity/integration
   or payment override is assumed.

Do not re-ask these five. All limits are accepted project choices, not source-
report or technical-standard claims. Q-011 operational viewing/UI/history and
catalogue-lifecycle gaps are in `OWNER_REVIEW.md`, A01–A09, for owner marks.
SQL types/Unicode counting/normalization enforcement remain technical design.

Password policy is resolved by ADR-053. Timed throttling is resolved by ADR-052 (five consecutive
failures in fifteen minutes, fifteen-minute login-only block, no status/session
change); detailed types/ranges and
time representation belong to database/architecture design, not coding now.

## Q-011: Which detailed function behaviors belong to the agreed scope?

Report sections III/V add details not fully transferred to memory. See
`../design/FUNCTION_CATALOG.md` for function-level references and proposals.
These questions grant no new permissions until the owner decides:

- Walk-in registration scope is resolved by ADR-012: no Admissions registration
  on behalf of students; consultation uses the normal Student workflow.
- Report export format is resolved by ADR-013: CSV, no XLSX requirement.
  ADR-041 allows Student browser printing of their own Invoice with accurate
  payment state; no application PDF download/export or separate receipt issuance.
- Schedule-entry method is resolved by ADR-014: generate dated sessions from
  weekly rules and add/edit individual sessions. ADR-032 resolves the expected
  count as a positive integer at TrainingProgram/course-Band level, LESSON only,
  frozen after first Class use. Require full LESSON schedules before teaching
  starts, not opening; FINAL_EXAM is separate. AcademicAffairs date/time/room
  edits require a session not started/no attendance or related scores and a
  class neither COMPLETED nor CANCELLED, with resource-conflict checks.
  Do not re-ask these policies; count fields/dependencies await database design.
  ADR-035 resolves scope: handle class-wide non-held lessons within NV19/F21;
  no separate holiday module/center-wide calendar, automatic holiday skipping/
  makeup scheduling or individual makeup lessons. A non-held lesson is not
  delivered; do not mark all students absent to bypass completion. ADR-036
  resolves pre-start moves under ADR-032 and post-start non-held LESSON handling:
  AcademicAffairs/reason, preserve old schedule/actor/time, create linked same-class
  replacement with the current teacher and conflict checks; no dependent data,
  class not COMPLETED/CANCELLED, consistent all-or-none writes. The replacement
  fulfills one original LESSON obligation, must occur with complete attendance
  before completion; no fake attendance for the old lesson. Do not re-ask these
  policies. ADR-037 excludes non-replacement LESSONs beyond the fixed program
  count: weekly/individual creation only fills required obligations; replacements
  fulfill existing ones, history rows may exceed the count, FINAL_EXAM separate.
  Do not re-ask extra-lesson scope. Remaining: data/history/link/audit and count
  enforcement representation during database design.
- Profile editing is resolved at permission level by ADR-016: students edit
  their own personal information; Admissions may not edit on their behalf;
  course choice is locked after confirmation. ADR-019 resolves editable fields
  as name/birth date/phone/address, with no student self-change of login email.
  ADR-038 excludes direct Course changes on any existing Registration: valid
  cancellation/new registration starts testing anew and retains history.
  ADR-039 excludes personal-profile snapshots, displaying current information
  on historical records while retaining Invoice amount snapshots and audits.
  ADR-051 excludes Admin email changes on existing accounts, including staff.
- Student test-booking cancellation/change and absence rebooking are accepted
  by ADR-021; absence attribution and center cancellation/rescheduling are
  accepted by ADR-027; closure/reopening and grader replacement are ADR-030.
  Q-004 retains history/audit representation and effective-closure persistence.
  Entry-score corrections are resolved by ADR-029: original/current grader,
  TESTING/BAND_ASSIGNED only with reason/audit; lock after confirmation or ended
  records, ties favor attempt one. ADR-040 excludes student class transfers even
  before teaching. ADR-042 permits only Accountant correction of confirmed Payment
  method/reference with reason and old/new/actor/time audit; amount, invoice link
  and confirmation time stay immutable, no cancel/delete/refund or second Payment.
- Grade operation scope resolved by ADR-034: mandatory whole-class submission,
  resubmission, reasoned return and finalization; all-or-none transitions,
  all required scores before sending, existing locks preserved. Individual
  draft/correction entry remains allowed; storage/audit representation is
  deferred to database/architecture, not a new unresolved business choice.
- Attendance window resolved by ADR-033: assigned Teacher enters/corrects
  from LESSON start until before Class COMPLETED, no fixed 24/48-hour per-lesson limit;
  then read-only even before grade submission/finalization. ADR-025 still
  requires complete LESSON attendance before completion. Block entry before
  lesson start, allow it at start without waiting for the end; do not re-ask
  these boundaries or infer permission to mark future sessions.
  Schedule date/time/room edit cutoffs are resolved by ADR-032; actual-session
  occurrence representation remains Q-008.
- Login identifier resolved by ADR-019: email/password, phone for contact only.
  ADR-020 resolves authenticated password changes with current-password check,
  Admin-only forgotten-password reset without email recovery, Admin self/last-
  active protection and ACTIVE/LOCKED/DISABLED access meanings. ADR-048–ADR-052
  resolve temporary credentials/forced change, all-session invalidation on access/
  password changes, same-account Admin reactivation, immutable login email and
  timed login throttling. ADR-053–ADR-057 resolve strength, temporary/normal
  lifetimes, concurrent devices and current-session logout. Revocation storage,
  activity definition and restricted-authentication details remain later design;
  do not re-ask these accepted policies.
- Audit coverage/security events/Admin filters/safe details/no file export are
  resolved by ADR-058–ADR-062. Event storage, logging failure/anti-flooding and
  retention remain database/architecture design. Limits of operational viewing
  for Admissions/AcademicAffairs remain to review, without new editing rights.

Operational-view/UI/history proposals remain NOT accepted and now live only in
`OWNER_REVIEW.md`, A01–A05: Admissions detail, AcademicAffairs detail, Accountant
invoice lookup, Student schedule history and common account-menu placement.
Owner alternatives belong in the fourth column there. Teacher roster/catalogue
lifecycle gaps are A06–A09; remaining security mechanisms are C11/C12 and D03–D09.
Do not write any proposal into requirements before approval or mark stage 1 done
from the creation/approval of this choice table; complete function output E01
still requires separate review of the actual document.

Account/security batch accepted 2026-09-28 — ADR-048–ADR-052:

1. Staff creation/Admin reset: random temporary password shown once for manual
   delivery, mandatory change before business access, no secret in log/audit;
   reset does not unlock/enable an account (ADR-048).
2. Manual lock/disable/reset/own password change invalidate all existing sessions,
   including current/unexpired JWTs; unlock/reactivation never revives them (ADR-049).
3. Admin may reactivate DISABLED → ACTIVE on the same account with role/data/
   history retained, no old sessions or reopening ended business records (ADR-050).
4. Login email immutable on existing accounts, including Admin/staff; no delete/
   recreate bypass (ADR-051).
5. Five consecutive failures within fifteen minutes block new login for fifteen
   minutes; success/expiry resets the counter/window, no status change or session
   revocation. Manual LOCKED requires Admin and DISABLED never auto-reactivates
   (ADR-052). Detailed counter/timer representation remains security design.

Password/session batch accepted 2026-09-28 — ADR-053–ADR-057:

1. Passwords 15–128 characters, permit spaces/Unicode, no mandatory uppercase/
   digit/symbol combination; reject common/compromised passwords and do not require
   periodic changes absent compromise (ADR-053). The 15-character minimum for non-MFA authentication and
   passphrase-friendly policy are informed by OWASP Authentication Cheat Sheet;
   128 is the accepted project maximum, not an OWASP-mandated maximum. Exact counting,
   denylist source and hashing/storage remain later design, no external-service
   or package approval. Source: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html
2. Temporary passwords expire 24 hours after issuance/reset; expiry requires
   Admin to reset again, does not disable/unlock the account (ADR-054).
   First-use mandatory change remains accepted ADR-048.
3. Normal sessions expire absolutely two hours after login or after thirty minutes
   without activity, whichever comes first; no automatic absolute extension,
   refresh or remember-me, require fresh login (ADR-055). These durations are accepted project choices, not externally
   mandated values. Server-side idle/absolute expiry is informed by OWASP Session
   Management Cheat Sheet; implementation and restricted-session mechanics remain
   later design. Source: https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html
4. Permit concurrent login on multiple devices; a new login does not kick out
   existing sessions (ADR-056). No device
   management/session-list feature is proposed; ADR-049 revokes all on its events.
5. Logout invalidates only the current session server-side, including an unexpired
   token; other sessions remain. Do not add a separate logout-all button (ADR-057).
   Mechanism is architecture
   work, not a new table/API/code now.

Do not re-ask these five. Additional validation and detailed activity/restricted-
session/rate-limit enforcement remain later related design.

Audit batch accepted 2026-09-28 — ADR-058–ADR-062:

1. In addition to already-required sensitive-operation audits, record successful
   course/program/Band configuration changes, class lifecycle transitions and
   schedule creation/moves/replacements. Do not log every ordinary page view/read.
   Keep all previously mandatory audits (ADR-058);
   this adds no class-teacher history workflow, undo or extra editing right.
2. Record successful/failed login, timed login-block activation, logout, own
   password change, staff creation/Admin reset and account-state operations,
   plus denied authorization. Distinguish unauthenticated failures from verified
   actors; do not attribute a guessed email to an authenticated actor (ADR-059).
   Event representation, anti-flooding and logging failure
   policy remain architecture work; no monitoring/alerting module is proposed.
3. Admin filters: business date range, actor, action, entity type/ID and result;
   paginated newest-first list (ADR-060). No unrestricted
   free-text request/payload search; dates follow ADR-047.
4. Display event time/actor/action/target/result/reason and relevant changed fields
   where applicable, honoring already-required old/new audits. Never persist
   passwords/hashes/JWT/session secrets or full request/response payloads; minimize
   personal data (ADR-061). This does
   not remove mandatory business history or create profile snapshots under ADR-039.
5. Admin views/searches logs in the application only; no audit CSV/PDF export.
   ADR-062; preserve already-accepted
   Admin-only access and no normal audit edit/delete rights; retention/disposal
   and operational backups remain architecture design, not decided here.

Security-event/minimal-secret logging recommendations were informed by official
OWASP Logging Cheat Sheet; the owner accepted the project-specific coverage/
filters/export scope. Source: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html
Do not re-ask these five. Storage, failure behavior/anti-flooding, retention and
backups remain architecture work in `OWNER_REVIEW.md`, D08/D09/D14; Q-012 choices
are C14/D10 there. Stage 1 is not complete.

## Q-012: What exactly do reports measure?

Partially resolved by ADR-018: Registration counts by course/registration date/
state, distinct from student counts; revenue from confirmed payments by
confirmation date; debt from UNPAID Invoice amounts, overdue as its past-due
subset; training class/state counts, size, session progress and FINALIZED
results. Pass criteria are resolved by ADR-024: FINALIZED final_score >= 50;
the pass-rate cohort/denominator is resolved by ADR-067.

Filters/defaults/drill-down/export scope are resolved by ADR-068–ADR-072.
Remaining: rate display precision and technical query/export details.
ADR-063–ADR-067 resolve current-state/no historical as-of
scope, period billed versus current debt, LESSON progress, distinct students and
pass-rate cohorts. Cancellation business policies are ADR-031. ADR-042's Payment
metadata correction changes neither revenue amount/date nor Invoice PAID.
Do not infer debt by subtracting values with
different date bases or add expense/profit reports from illustrative forms.

Reporting-metrics batch accepted 2026-09-28 — ADR-063–ADR-067:

1. Reports use current record states at query time, not reconstructed historical
   states/debt as of a past date. Date filters select the applicable record/event
   cohort under ADR-018/047; they do not restore old states (ADR-063).
   This does not remove required business/audit history or financial snapshots.
2. Total billed for a selected period sums amounts of currently non-CANCELLED
   Invoices (UNPAID + PAID) issued in that period. It is not outstanding debt or
   revenue. Show current UNPAID debt separately; do not apply the payment-date
   period filter to debt. Course filters may apply consistently (ADR-064).
   No as-of debt, expenses/profit or original-issuance gross/cancellation ledger
   metric is proposed. Detailed UI/date-filter separation follows later review.
3. Student count is distinct student_id in the filtered Registration cohort;
   each course group counts that student once, while the report grand total is
   distinct across all groups, not their sum. Registration count remains separate;
   repeated registrations can increase it without adding another unique student.
   Keep ADR-018's registration/student distinction (ADR-065).
4. Lesson progress = actually-confirmed-held LESSON obligations / program expected
   LESSON count. Non-held historical originals do not contribute; a held linked
   replacement contributes once to the same obligation. FINAL_EXAM appears
   separately, not in lesson numerator/denominator (ADR-066). This does not change
   completion guards or assume elapsed time/attendance alone proves occurrence;
   Q-008 retains occurrence representation. CANCELLED classes are not delivered
   progress; class-state reporting still retains them under ADR-018/026.
5. Pass rate = passing FINALIZED GradeReports / all FINALIZED GradeReports in
   the selected finalization-date period, within selected course/class filters.
   Count each student's enrollment result separately, not distinct people across
   repeats. Unfinalized results are excluded, not failures; empty denominator
   displays "Chưa có dữ liệu", not 0% (ADR-067).
   Keep >=50 passing/rounding under ADR-024/028; rate display precision follows
   later UI/numeric design.

Do not re-ask these five. Report filters/detail/export scope are accepted below;
query/export mechanisms, rate display and other operational viewing remain.
Q-012/stage 1 are not fully done.

Reporting-filters/visibility/export batch accepted 2026-09-28 — ADR-068–ADR-072:

1. Admissions report keeps Course/registration date/current-state filters under
   ADR-018. Default date range: current Vietnam calendar month; default all states
   including CANCELLED/REJECTED. Course selector retains inactive courses for
   historical records (ADR-068), rather than an all-time default.
   No new Admissions report screen/export or registration-on-behalf right.
2. Training inventory/progress filters: Course, Band, Class, assigned Teacher and
   current Class state; default all states and no date filter for current inventory.
   Finalized-result/pass-rate view adds a finalization-date period defaulting to
   the current Vietnam month. Course choices retain inactive referenced courses.
   Selected over only Course/Class/state filters (ADR-069). Preserve
   ADR-063/066/067 metric bases; no class creation-date field or historical
   teacher view is assumed.
3. Finance report defaults revenue/billed period to the current Vietnam month;
   keep their accepted confirmation/issuance date bases. Add Course filtering
   consistently and an overdue-only option for current debt. Debt has no date
   period filter (ADR-070); additional payment-method/confirming-staff report
   filters were not selected.
   Do not reinterpret debt, add as-of reporting or assume report time fields.
4. Director drills into read-only group breakdowns by Course/Band/Class where
   applicable (counts, size, progress, finalized aggregates and financial totals),
   not individual Student registrations/Invoices/identifying personal information.
   Individual-record drill-down was not selected (ADR-071).
   Keep no Director export/business approval and Accountant operational invoice
   lookup unchanged. This adds no Admin/Admissions/AcademicAffairs report rights.
5. Accountant's existing financial CSV permission exports all matching results,
   not only the visible page, using the same selected filters and report columns.
   No hidden fields or extra Student-profile data; export does not broaden scope
   to separate registration/Invoice/profile dumps. Current-page-only CSV was
   not selected (ADR-072). Report query/export consistency, file structure/limits/
   encoding/injection safeguards remain architecture design. No XLSX/audit
   export/new role rights.

Do not re-ask these five. Dates use ADR-047; metric bases and role responsibilities
stay fixed. Remaining technical field constraints and Q-011 operational viewing
are consolidated in `OWNER_REVIEW.md`; accepted profile/other-field policies
above remain fixed. Query/export consistency,
file layout/limits/encoding/injection safeguards and rate display remain later
technical design, not code authority or stage completion.
