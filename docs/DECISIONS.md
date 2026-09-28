# Decisions

## ADR-001: Use the fixed application stack

Status: Accepted

Context:
The project instruction kit explicitly fixes the technology stack.

Decision:
Use ASP.NET Core Web API with EF Core code-first, SQL Server, and JWT for the
backend; React, Vite, TypeScript, and Tailwind CSS for the frontend; and xUnit
for backend tests.

Reason:
This is a stated project constraint and supports a clear, explainable
university project implementation.

Alternatives considered:
No alternatives have been authorized.

Consequences:
Agents must ask before adding/replacing frameworks or major packages.

## ADR-002: Treat the requirements file as the product source of truth

Status: Superseded by ADR-007 on 2026-09-27

Context:
The instruction kit defines roles, permissions, states, business rules, band
thresholds, and screen priorities in one file.

Decision:
Use `instructions/english-center-agent-kit/docs/requirements.md` as the
canonical product requirements. Treat code and observable behavior as the
strongest evidence of what is currently implemented once code exists.

Reason:
This prevents agents from inventing rules while still allowing stale
implementation documentation to be corrected.

Alternatives considered:
Copying the requirements into repository-level Project Memory was rejected
because it would create another source that can drift.

Consequences:
The owner controls requirement changes. Agents record unresolved issues in
`docs/plan/questions.md`.

## ADR-003: Deliver work as small vertical slices

Status: Accepted

Context:
The planning instruction requires capabilities to span database, API, UI, and
tests and to fit one worker session.

Decision:
Plan implementation as vertical slices of roughly one to three screens, with a
pilot Slice 0 before expanding the plan.

Reason:
This exposes integration problems early and keeps review and handoff bounded.

Alternatives considered:
Building all backend layers before any UI was explicitly rejected by the
planning instruction.

Consequences:
Each slice must define files, API contracts, business rules, runnable
acceptance checks, and out-of-scope work.

## ADR-004: Reuse the existing singular plan convention

Status: Accepted

Context:
The general Project Memory request proposes `docs/plans/`, while the existing
project-planning skill defines `docs/plan/plan.md`, `docs/plan/questions.md`,
and `docs/plan/slices/`.

Decision:
Use `docs/plan/` as the single execution-planning location. Do not create a
parallel `docs/plans/` tree.

Reason:
The Project Memory request says to reuse equivalent existing documentation and
avoid duplicates.

Alternatives considered:
Maintaining both singular and plural plan trees was rejected because status
would drift.

Consequences:
Active/completed state is represented by plan and slice status fields rather
than by moving files between `active/` and `completed/` directories.

## ADR-005: Treat instructions as an input inbox

Status: Accepted

Context:
The owner uses `instructions/` to provide directive and reference files over
time. It is not the application source tree or the durable memory itself.

Decision:
Keep owner-supplied inputs under `instructions/`. Review them before use, then
link adopted inputs from Project Memory or summarize resulting decisions in
the appropriate memory file. Do not assume every file in the directory is
automatically active.

Reason:
This separates raw directives from verified project state and allows new input
packages to be added without silently changing every agent's behavior.

Alternatives considered:
Treating the entire directory as an always-active rule tree was rejected
because unrelated or superseded instruction packages may coexist there.

Consequences:
`AGENTS.md` identifies the loading rule, while `PROJECT_CONTEXT.md` lists the
currently adopted inputs.

## ADR-006: Compose skills and agent integrations by explicit boundaries

Status: Accepted

Context:
The owner intends to use multiple skills, including a skill that links to and
works with another coding agent.

Decision:
Allow multiple skills and agent integrations to coexist. Each selected skill
controls only its declared scope. Cross-agent work must use an explicit task,
handoff, ownership boundary, and verification step; durable state returns to
Project Memory or the active execution plan.

Reason:
Skills can provide complementary capabilities without requiring one permanent
agent workflow for every task.

Alternatives considered:
Forcing the project to choose exactly one global agent workflow was rejected.

Consequences:
When a new integration skill is adopted, document what it delegates, who may
edit which files, how completion is verified, and where the handoff is stored.

## ADR-007: Use normalized Project Memory as the product baseline

Status: Accepted — 2026-09-27

Context:
The owner designated Nhom18_ThietKeGiaoDien.docx as the official foundation,
resolved six review questions, and requested that corrections live in memory
without editing the Word report.

Decision:
Use `docs/requirements.md` as canonical product rules, `ARCHITECTURE.md` for
the normalized target data model, and `reference/REPORT_BASELINE.md` for source
provenance/corrections. The Word report remains unchanged. Illustrative forms
do not define requirements; older diagrams and kit rules do not override memory.

Reason:
Agents need one consistent, repository-readable specification across chats.

Alternatives considered:
Editing Word or continuing to treat older kit requirements as canonical.

Consequences:
ADR-002 is superseded. Preserve source inputs, update memory for future changes,
and leave unresolved issues explicitly open. This changes specification
authority, not the distinction between designed and implemented behavior.

## ADR-008: Limit entry-test retakes per enrollment record

Status: Accepted — 2026-09-27

Context:
The report and old requirements differed on the scope/timing of retakes.

Decision:
Each Registration allows at most two attempts. Register the second only after
the first has a result and before admission confirmation. Choose the highest
completed score for Band. Do not register retakes after CONFIRMED or later.

Reason:
This aligns the limit with EntryTestAttempt.registration_id and avoids changing
placement eligibility after the student commits to admission.

Alternatives considered:
A lifetime per-student limit; retakes after confirmation/admission/placement.

Consequences:
Tests must enforce the record-scoped limit and confirmation cutoff. Absence,
student booking cancellation and pending-retake confirmation were subsequently
resolved by ADR-021; center-wide session operations were subsequently resolved
by ADR-027/ADR-030. History representation remains
for database design.

## ADR-009: Configure scores per program and use a 100-point scale

Status: Accepted — 2026-09-27

Context:
Descriptions attributed weights to Course, while the schema attached them to
TrainingProgram. Mockups showed /10 despite 0–100 schema values.

Decision:
ScoreComponentConfig belongs to TrainingProgram for a course/Band. Weights
sum to 100; component, entry-test and final scores are stored/displayed 0–100.

Reason:
Keep grading configuration aligned with the schema and avoid mixed scales.

Alternatives considered:
Course-wide weights; displaying converted ten-point scores.

Consequences:
Plans and UI follow /100. Configuration history and rounding were initially
open and subsequently resolved by ADR-028.

## ADR-010: Restrict overdue behavior and remove scholarships

Status: Accepted — 2026-09-27

Context:
The report mentioned automatic debt consequences and contained scholarship
entities not consistently defined in its current data dictionary.

Decision:
Overdue UNPAID invoices show warnings and days overdue only. Do not change
student status, cancel records or lock accounts automatically for debt.
Remove all scholarship functionality and tables from the project scope.

Reason:
The owner selected the simpler collection workflow and explicitly removed
scholarships from the system.

Alternatives considered:
Automatic penalties after 15 days; retaining scholarship tables for later use.

Consequences:
Do not scaffold ScholarshipGrant/ScholarshipCandidate/ScholarshipCriteria or
connect scholarships to finance. Future expansion requires a new decision.

## ADR-011: Agree on the complete design before implementation planning

Status: Accepted — 2026-09-27

Context:
The owner explicitly requested agreement on business functions, workflows,
database and system architecture before discussing the implementation plan.

Decision:
Complete and agree on those four design areas, then review their consistency
and obtain owner acceptance of the complete baseline. Only afterward discuss
implementation phases and slices. Do not start Git initialization, scaffold,
Slice 0 or application coding during the design-agreement stage.

Reason:
The implementation must follow a coherent, owner-approved specification rather
than settling core product behavior while coding individual slices.

Consequences:
`docs/plan/design-baseline.md` is the active plan. ADR-003 remains the eventual
implementation delivery approach, but its pilot and slice planning are deferred.
Keep accepted rules, proposals and unresolved questions distinguishable; preserve
the six accepted resolutions and do not edit the official Word report.

## ADR-012: Remove Admissions registration on behalf of students

Status: Accepted — 2026-09-27

Decision:
Remove the distinct Admissions walk-in enrollment and test-booking capabilities
(report functions NV10/NV11). Counter consultation or guidance is still normal
Student self-registration, not an Admissions operation. Students create their
own accounts, course registrations and test bookings. Admissions retains record
lookup/checking, progress tracking and admission approval.

Consequences:
Remove F15 from implementation scope, keeping its code as a source reference.
There are 32 active screen codes and 37 retained functions from the original
39-function catalog, not a finalized count of every detailed software action.
Do not create Admissions menus/APIs/permissions for registration on behalf of
students, including students with existing accounts. Source Word remains unchanged.

## ADR-013: Export reports as CSV

Status: Accepted — 2026-09-27

Decision:
Use CSV for report file exports; XLSX is not required.

Consequences:
This resolves the report's Excel/CSV ambiguity, not report metrics or permissions.
Do not add file export rights to view-only roles or invoice/receipt downloads
without an explicit scope decision; ADR-041 subsequently permits Student-owned
Invoice browser printing only. CSV encoding/security conventions are to
be defined during technical design.

## ADR-014: Support weekly schedule generation and individual sessions

Status: Accepted — 2026-09-27

Decision:
AcademicAffairs can generate dated sessions from a start/end date range,
weekdays, time and room, and add or adjust individual sessions. Check resource
conflicts before saving. Do not implement automatic schedule optimization.

Consequences:
The Schedule target remains concrete dated sessions. Expected lesson count and
date/time/room edit cutoffs were subsequently resolved by ADR-032. Rule
persistence and session-occurrence representation remain design work. ADR-035
later resolves holiday/non-held lesson scope within NV19/F21; ADR-036 resolves
the accepted pre-start move/post-start linked replacement workflow. Individual
LESSON additions are bounded by the program count under ADR-037, not permission
to add extra teaching sessions beyond it.

## ADR-015: Limit unfinished enrollments per student and course

Status: Accepted — 2026-09-27

Decision:
A student may have only one unfinished Registration for the same Course.
Concurrent registrations for different courses are permitted. The student may
repeat a previously completed course.

Consequences:
Prevent duplicate creation under concurrency. This is a lifecycle-scoped rule,
not permanent uniqueness of (student_id, course_id). ADR-022 later resolves the
completion predicate and treatment of cancelled/rejected records; Q-005 retains
database representation and enforcement. Do not
invent a Registration COMPLETED state or infer completion from ADMITTED alone.

## ADR-016: Allow student-owned profile edits and lock confirmed course choice

Status: Accepted — 2026-09-27

Decision:
Students may edit their own personal information. Admissions checks the data
but may not edit it on their behalf. The selected Course on a Registration
cannot change after admission confirmation.

Consequences:
Distinguish mutable personal information from registration/course history.
Do not freeze the whole profile merely because a record is CONFIRMED, grant
other roles profile-edit rights, or assume a course-change workflow before
confirmation. Editable fields and login identity were subsequently resolved
by ADR-019; ADR-038 locks the Course on every existing Registration and
ADR-039 excludes personal-profile snapshots.

## ADR-017: Permit reasoned rejection of confirmed admission records only

Status: Accepted — 2026-09-27

Decision:
Admissions may reject a CONFIRMED Registration, setting REJECTED and recording
a mandatory reason visible to the owning student. Do not reject a record
already approved or paid.

Consequences:
Rejection is distinct from cancellation/refund. Preserve the prior confirmation
history; rejection reason and action attribution/time need support in the
detailed data design. The current Registration field list is incomplete for
this action. Cancellation business effects are resolved by ADR-031; Q-007
retains history/audit representation and attribution design.

## ADR-018: Define the scope and basis of the three report groups

Status: Accepted — 2026-09-27

Decision:
Admissions reporting counts Registration records by course, registration date
and state, distinguishing registration count from student count. Financial
revenue sums confirmed payments using payment-confirmation date; outstanding
debt sums UNPAID Invoice amounts and overdue debt is the past-due subset.
Training reporting covers class counts by state, class size, session progress
and FINALIZED results. Add pass-rate reporting only after agreeing pass criteria.

Consequences:
Accountant and Director must use consistent financial definitions. Issued
invoices are not revenue; current debt must not be derived by subtracting
period revenue from period invoice issuance. Payment-confirmation reporting
does not itself resolve storage of a separate actual-receipt timestamp; ADR-046
subsequently excludes that field. Business date filters follow ADR-047. ADR-063–ADR-067
resolve current-state scope, period billing, distinct students, LESSON progress
and pass-rate cohorts. ADR-068–ADR-072 resolve report filters/detail/export scope;
Q-012 retains display/query/export-mechanism details. No new export
rights, pass threshold or expense/profit reporting is implied.

## ADR-019: Use email login and define student profile fields

Status: Accepted — 2026-09-27

Decision:
Login uses email and password; phone is contact information, not a login
identifier. Student profiles require full name, birth date, email and phone;
address is optional. Students may edit their own name, birth date, phone and
address, but may not self-change their login email in the current scope.
Changed phone numbers must remain valid and unique.

Consequences:
Do not infer mandatory staff birth dates/phones from student requirements,
email verification/SMS workflows, staff email-change privileges or a new
password-change feature solely from field choices; password change was later
accepted by ADR-020. Student edits do not change a confirmed course choice.
Map field nullability and validation details during database/API design;
personal-profile snapshots are excluded by ADR-039. Password delivery/revocation/
reactivation/timed throttling follow ADR-048/049/050/052; ADR-051 excludes Admin
email correction. Password policy/lifetimes/session scope follow ADR-053–ADR-057;
their storage and enforcement remain detailed design.

## ADR-020: Define password changes, Admin protection and account states

Status: Accepted — 2026-09-27

Decision:
Logged-in users may change their own password after supplying the correct
current password. Forgotten passwords are reset by Admin; there is no email
recovery workflow. Admin may not lock/disable their own account or the last
active Admin. ACTIVE permits access; LOCKED blocks access and can be opened
by Admin; DISABLED means discontinued use and blocks access. Both blocked
states retain accounts and business data rather than deleting them.

Consequences:
Protect Admin availability under concurrent operations. Password change is
a shared authenticated action, not registration or permission to reset other
users. ADR-048–ADR-052 subsequently resolve temporary-password delivery/forced
change, all-session invalidation, DISABLED reactivation, immutable login email
and timed login throttling. ADR-053–ADR-057 resolve password policy/lifetimes and
session scope; revocation storage remains later design. Manual LOCKED stays
distinct from timed throttling.

## ADR-021: Rebook unfinished entry attempts and block pending-retake confirmation

Status: Accepted — 2026-09-27

Decision:
Students may cancel/change test bookings before session start and before
admission confirmation without consuming an additional logical attempt.
Retain booking history. Absence has no score, not zero; the student may book
another session to complete the same unfinished attempt, not create a new one.
Attempt two still requires a scored first attempt. Block confirmation while
attempt two is scheduled or already taken but ungraded; wait for its result or
cancel its unstarted booking before confirming with the existing result.
An already-taken ungraded test cannot be cancelled to bypass this check.

Consequences:
The two-attempt limit applies to logical attempts per Registration, not count
of historical bookings. Rebooking does not introduce attempt three or permit
editing a completed score. Preserve cancelled/absent booking history and
control session capacity for active bookings under concurrency. The current
single-session EntryTestAttempt model needs review in database design, not
silent field overwrite or new table creation now. ADR-027 resolves absence and
center cancellation; ADR-030 additionally requires the source session OPEN
for student cancellation/change. Q-004 retains history representation.

## ADR-022: Define enrollment cancellation and finished-record eligibility

Status: Accepted — 2026-09-27

Decision:
Students may cancel their own Registration before CONFIRMED, preserving history.
Admissions may cancel a CONFIRMED record or an ADMITTED record with an UNPAID
Invoice, with a mandatory reason. Cancel any issued UNPAID Invoice together
with the record. There is no paid-record cancellation/refund workflow in scope.
A record completes when its assigned Class is COMPLETED AND that student's
GradeReport is FINALIZED, regardless of passing. CANCELLED/REJECTED also end
the record for repeat-enrollment eligibility. Create a new Registration rather
than erasing or reopening the old record.

Consequences:
Enrollment cancellation differs from test-booking cancellation and rejection.
Preserve confirmation/approval and historical business data. Cancellation and
invoice state changes need an atomic transaction and protection against racing
payment. The completion rule does not create a new Registration status or mean
ADMITTED is course completion. Database representation and cancellation attribution
remain design work in Q-005/Q-007. Dependent-booking effects were subsequently
resolved by ADR-031, and class cancellation by ADR-026. No passing threshold
is implied.

## ADR-023: Restrict placement and teacher changes to pre-teaching lifecycle

Status: Accepted — 2026-09-27

Decision:
Keep Registration ADMITTED after placement; derive the displayed placed state
from StudentClassAssignment rather than adding a Registration state. Place
only into OPEN classes with matching course/Band and remaining capacity, using
ADMITTED records with PAID invoices. No late placement into ONGOING classes.
AcademicAffairs may change the responsible teacher only before ONGOING and
before attendance/grades exist. Recheck all class schedules against the new
teacher's class/test assignments. No changes once teaching has begun.

Consequences:
PLANNED/ONGOING/COMPLETED/CANCELLED classes do not accept new placement. Enforce
placement conditions under concurrency, including class-state changes. Preserve
the one-teacher model without adding a teacher-change history workflow. Class
opening/start/completion prerequisites follow ADR-025; ADR-040 excludes student
transfers. These rights do not authorize editing historical grades.

## ADR-024: Define grade submission, edit locks and the passing threshold

Status: Accepted — 2026-09-27

Decision:
Teachers may enter draft scores while teaching, but submit only when Class is
COMPLETED and all required component scores exist. AcademicAffairs reviews and
finalizes only SUBMITTED reports from COMPLETED classes. Teachers edit component
scores/comments only in DRAFT or RETURNED; SUBMITTED is frozen until returned
by AcademicAffairs with a reason, and FINALIZED remains immutable.
Passing means finalized final_score >= 50/100; below 50 is not passing. Do not
add attendance requirements, component minima or additional classification bands.

Consequences:
Unfinalized results are neither published nor treated as failures. Enforce
editing/report transition rules under concurrency. Completion/re-enrollment
under ADR-022 is independent of passing. Class completion cannot require grade
submission/finalization as a prerequisite, which would create a circular flow.
Precision/rounding was subsequently resolved by ADR-028; whole-class operation
scope is resolved by ADR-034; pass-rate cohort and denominator follow ADR-067.

## ADR-025: Define explicit class opening, teaching start and completion

Status: Accepted — 2026-09-27

Decision:
AcademicAffairs moves PLANNED to OPEN when the program is ACTIVE, a teacher is
assigned and at least one future LESSON is scheduled without conflicts. ADR-032
subsequently adds a full LESSON schedule prerequisite before teaching starts.
AcademicAffairs
moves OPEN to ONGOING when teaching actually starts, no earlier than the first
LESSON and with at least one properly placed student; no higher minimum size.
AcademicAffairs moves ONGOING to COMPLETED after confirming all scheduled
LESSON/FINAL_EXAM sessions occurred and every LESSON has attendance for all
assigned students. These transitions are explicit, not automatic by date/time.
Class completion does not require submitted/finalized grades; those follow
completion under ADR-024. Class cancellation was subsequently resolved by ADR-026.
ADR-036 later refines completion for non-held LESSONs: retain the original as
history, and require its replacement to occur with complete attendance instead
of requiring the non-held original to occur or have fake attendance.

Consequences:
Avoid the circular requirement of finalized grades before class completion.
Preserve chronological class progression and enforce prerequisites against
concurrent schedule/placement/attendance changes. Q-008 retains representation
of actual session occurrence, not already-accepted transition rights; full
schedule/expected-count rules are ADR-032. Class cancellation is ADR-026;
Q-007 retains cancellation attribution/history data design.

## ADR-026: Cancel only empty pre-teaching classes and preserve history

Status: Accepted — 2026-09-27

Decision:
AcademicAffairs may cancel only PLANNED/OPEN classes with no assigned students,
before teaching begins, with a mandatory reason. Set CANCELLED; do not physically
delete the class. Block cancellation when students are assigned, teaching has
begun or the class is ONGOING/COMPLETED. Do not add transfer/refund workflows;
paid records waiting for placement are unaffected. Retain class/schedule history;
CANCELLED-class schedules no longer reserve teacher/room time, appear as upcoming
teaching or count as completed sessions.

Consequences:
Enforce the empty-class guard against concurrent placement and preserve history
while excluding cancelled schedules from active operational queries. The accepted
rule does not introduce a Schedule status or finalize cancellation reason/actor/
time storage. Q-007 retains attribution representation; Registration-to-test-
booking effects were subsequently resolved by ADR-031.

## ADR-027: Define test absence attribution and center cancellation/rescheduling

Status: Accepted — 2026-09-27

Context:
The source report III function 14 covers session creation/capacity/grader
assignment; VI.2 table 9 and VI.3 domain rules define OPEN/CLOSED/CANCELLED and
prohibit testing/grading in CANCELLED sessions. The owner explicitly accepted
the three proposed operational policies; these details supplement the report.

Decision:
The assigned grader records ABSENT after session end for a student confirmed
not to have attended. Missing score alone does not imply absence; no zero score.
Rebooking uses the same unfinished logical attempt under ADR-021.
AcademicAffairs may cancel a non-cancelled session only before start, with a
mandatory reason. Cancel affected active bookings while preserving history;
unfinished attempts remain rebookable without an extra attempt. Students choose
another OPEN session with capacity themselves, subject to existing eligibility.
No automatic reassignment or staff booking on their behalf.
AcademicAffairs changes session date/time/room only before start and while no
booking history exists. Once booked, use reasoned cancellation and a new session
rather than overwriting appointments. No cancellation/rescheduling after start
or erasure of completed scores; CANCELLED sessions cannot be used for grading.

Consequences:
Separate cancelled bookings from unfinished logical attempts; cancellation of
a session is not cancellation of Registration. Protect session mutation against
racing bookings, grading and the start-time cutoff. Retain reason/actor/time and
booking history in the eventual data design. Grader replacement and CLOSED/OPEN
transition rules were subsequently resolved by ADR-030. Registration-to-booking
cancellation effects were subsequently resolved by ADR-031; this decision does
not change ADR-022's enrollment policy.

## ADR-028: Freeze used grading configuration/Band thresholds and define rounding

Status: Accepted — 2026-09-27

Decision:
Freeze a TrainingProgram's course/Band association, score-component list and
weights once any Class references it, including a subsequently cancelled class.
Before first use, AcademicAffairs may configure components/weights; activation
still requires weights totaling exactly 100. No configuration-versioning module
is introduced. Changes to used configuration require a separately agreed
version/snapshot design and reconsideration of course/Band uniqueness; do not
silently create another program for the same pair.
Keep seeded Band thresholds 0/25/45/65/85 and freeze the whole threshold set
after the system's first completed entry-test result. No retrospective placement
recalculation from later configuration changes. An eligible second attempt
still updates best score/Band before confirmation using the same thresholds.
Accept entry/component scores and weights with at most two decimal places;
reject excess precision rather than silently rounding input. Calculate weighted
totals using decimal arithmetic without rounding individual contributions, then
round once to two decimals, half-up (positive midpoint away from zero), for
storage, display and passing comparison. FINALIZED 49.995 becomes 50.00 and
passes; 49.994 becomes 49.99 and fails. Entry Band comparison uses the accepted
entry score without integer rounding.

Consequences:
Preserve historical reproducibility and enforce freezes against concurrent
class creation/configuration edits and first entry-result/threshold edits.
ADR-009's /100/program scope and ADR-024's finalized-only passing remain intact;
their open configuration/rounding details are resolved here. Final database
precision/enforcement design follows in the database stage, not migration now.
Completed entry-score correction permissions/cutoffs/audit and tied-best-attempt
attribution were initially left to Q-006 and subsequently resolved by ADR-029;
ADR-028 itself does not authorize score correction.

## ADR-029: Audit pre-confirmation entry-score corrections and break best-score ties

Status: Accepted — 2026-09-27

Decision:
Only the teacher who entered a COMPLETED entry-test score and remains assigned
to its session may correct it, while Registration is TESTING or BAND_ASSIGNED.
Require a reason and preserve old/new score, actor and time. Recalculate the
highest completed score and Band atomically using ADR-028's frozen thresholds.
Retain both logical attempts; a correction does not consume another attempt.
After admission confirmation, freeze entry scores and assigned Band. Do not
correct CONFIRMED/ADMITTED or ended CANCELLED/REJECTED records. No override for
Admin or AcademicAffairs is in scope. This does not freeze the student's
otherwise editable personal information.
When both COMPLETED attempts share the highest score, select the lower
attempt_number (attempt one) as best_attempt_id; preserve and display both
results. The same score yields the same Band.

Consequences:
Enforce original-grader/current-assignment ownership, permitted record state,
0–100 range and two-decimal input validation. Guard correction against concurrent
confirmation, cancellation and other grading changes; score/audit/Band updates
must remain consistent. Do not delete a score, create an extra attempt, change
an absent result to a completed one or reopen a confirmed record via correction.
Q-006's business policies are resolved by ADR-028/ADR-029. Audit representation
and constraints remain for database design; no tables/migrations are created now.

## ADR-030: Define session opening/closure and pre-start grader replacement

Status: Accepted — 2026-09-27

Decision:
Create EntryTestSession as CLOSED. AcademicAffairs may open/reopen it only
before start_at, with valid time/room, an assigned ACTIVE Teacher, no resource
conflicts and active bookings not exceeding capacity (1–20). Only OPEN future
sessions with capacity accept new bookings. A full session stays OPEN with a
full-capacity indicator; an eligible cancellation releases a place.
AcademicAffairs may close an OPEN session before start, preserving all bookings.
For a CLOSED session, students may view their schedule/results but may not
register, cancel a booking or change away from that session. Student cancellation/
change under ADR-021 additionally requires the source session OPEN; the target
must be OPEN with capacity. Reopening restores eligible pre-start operations.
At start_at, a non-cancelled session is effectively CLOSED and cannot reopen;
enforce the time cutoff even if stored status has not yet been updated.
CLOSED preserves the scheduled test and permits assigned-teacher grading,
post-end absence under ADR-027 and correction under ADR-029. It is not a score
freeze or session cancellation. CANCELLED cannot reopen or be graded.
Only AcademicAffairs may replace the grader of a non-cancelled session, before
start and before any score or ABSENT record exists. Require a reason, preserve
old/new teacher, actor/time, and check the ACTIVE replacement Teacher against
all overlapping class/test assignments. Keep existing bookings unchanged.
No replacement after start or any result; no privileged score-correction override.

Consequences:
Closure does not remove bookings, release their places or bypass pending-retake
confirmation guards. A booked retake in CLOSED still blocks confirmation until
resolved under accepted rules; student cancellation requires eligible reopening
before start. Center session cancellation remains ADR-027; Registration-to-booking
effects were subsequently resolved by ADR-031 and must not be inferred from
individual student booking permissions.
Enforce opening/closure, booking mutations, grader replacement and start-time
guards under concurrency; preserve ADR-029's original/current-grader ownership.
Exact booking/history/audit storage and effective-closure persistence belong to
database/architecture design. No scheduler, new enum, tables or code is added now.

## ADR-031: Cancel dependent future bookings and preserve ended test history

Status: Accepted — 2026-09-27

Decision:
When an authorized Registration cancellation succeeds under ADR-022, cancel its
active bookings in sessions that have not started and release capacity, including
pre-start CLOSED sessions. Show affected bookings before the user confirms the
cancellation. Keep session state and other students' bookings unchanged. This
dependent effect does not grant individual cancellation/change rights in CLOSED
under ADR-030. Do not allow further booking on the cancelled Registration.
For sessions already started, preserve booking/attempt history, existing scores,
COMPLETED/ABSENT results and any assigned Band. Do not cancel an already-taken
ungraded booking, delete scores or infer zero/ABSENT from missing scores.
After Registration CANCELLED, stop score entry/correction and absence recording
for that record, including in CLOSED sessions. Display ungraded history as
"Hồ sơ đã hủy — chưa có kết quả", outside actionable grading work. This display
does not introduce an EntryTestAttempt enum or mark the test COMPLETED/ABSENT.
Repeat enrollment uses a new Registration and starts testing anew; do not copy
old scores/Band. Re-enrollment eligibility under ADR-022 remains intact.
Preserve cancellation actor/time/prior state and reason where already mandatory
under accepted actor rules. Trace dependent booking cancellations to the parent
Registration cancellation. Registration, any issued UNPAID Invoice and future
booking cancellation/capacity release commit together; if any required update
fails, commit none of the changes.

Consequences:
Protect against racing booking changes, session start, grading, admission
confirmation/approval and payment. Cancellation never confirms/reopens the old
Registration or bypasses ADR-021's pending-retake confirmation guards for a
continuing record. No paid-invoice cancellation/refund rights are added.
Q-007's cancellation business policies are resolved by ADR-022/026/031; remaining
history/audit fields, attribution relationships and transaction enforcement are
database-design work. Preserve accepted states; no code or migrations are created.

## ADR-032: Define expected lesson counts, full schedules and schedule edit cutoffs

Status: Accepted — 2026-09-27

Decision:
The expected lesson count belongs to TrainingProgram for a course/Band and is a
positive integer. Count LESSON only, excluding FINAL_EXAM. Freeze this setting
when the first Class references the program, consistent with ADR-028's first-use
configuration freeze; a subsequently cancelled class does not undo first use.
Keep ADR-025's opening rule: at least one valid future LESSON is sufficient for
PLANNED → OPEN. Before OPEN → ONGOING, require the full expected LESSON schedule,
in addition to ADR-025's actual-start and valid-student prerequisites. Schedule
FINAL_EXAM separately; it does not substitute for a required LESSON.
AcademicAffairs may change an individual class session's date/time/room only
before that session starts, with no attendance or related score data, and while
the class is neither COMPLETED nor CANCELLED. Check class/room/teacher conflicts
against both class schedules and entry-test sessions before saving. Preserve
sessions already started or with dependent data; do not overwrite their history.

Reason:
Keep a stable program-level teaching target, allow preparation of OPEN classes
before their timetable is complete, and protect historical attendance/results.

Consequences:
Guard first use versus count edits, teaching start versus timetable changes,
and schedule edits versus time cutoff/attendance/score entry/class transitions.
This decision resolves these Q-011 policies, not the attendance entry/edit window,
actual-session occurrence representation, holidays, extra/makeup-session rules or
the complete database schema. Exact count field/type and score-to-session
dependency representation belong to database design; no schema/code is created.

## ADR-033: Define attendance entry and correction from lesson start to class completion

Status: Accepted — 2026-09-27

Decision:
The assigned Teacher may enter or correct a LESSON's attendance from the time
that lesson starts until the class transitions to COMPLETED. The owner accepted
the lesson-start opening boundary on 2026-09-27; do not enter attendance before
the lesson starts or require waiting for its end. Do not impose a fixed
24/48-hour deadline after each lesson. Once Class is COMPLETED, attendance
is read-only, regardless of whether
GradeReports have been submitted or finalized. Existing assignment/ownership
rules and one attendance record per student/LESSON remain unchanged.

Reason:
Use class completion as the agreed operational cutoff rather than separate
per-lesson deadlines.

Consequences:
Preserve ADR-025: all required LESSON attendance must be complete before
AcademicAffairs may complete the class, with the historical non-held/replacement
lesson refinement subsequently accepted in ADR-036. Protect attendance writes against concurrent completion;
an update must not commit after the class has completed. No reopening or staff
override rights are added. The attendance window is resolved in Q-011: lesson
start inclusive, class completion exclusive. Validate against the current lesson
schedule, including concurrent schedule changes under ADR-032. Allowing attendance
at start does not automatically mark a lesson as fully occurred or complete the
class; actual-session occurrence representation remains Q-008. No application
code or schema changes are created.

## ADR-034: Require whole-class grade submission and review operations

Status: Accepted — 2026-09-27

Decision:
Grade submission, resubmission, AcademicAffairs return and finalization must
process the entire class in one operation, not selected students independently.
The owner chose mandatory whole-class processing for submission/review scope.
"One operation" does not mean one lifetime submission: preserve the reasoned
RETURNED → SUBMITTED correction/resubmission workflow under ADR-024.

Before submission/resubmission, Class must be COMPLETED and every assigned
student must have all required component scores and an editable DRAFT/RETURNED
report. AcademicAffairs returns or finalizes the whole set of SUBMITTED reports
for a COMPLETED class; returning requires a reason and preserves existing
scores/comments for correction. Finalization publishes all students' reports
together. No individual submission, return or finalization is added.

Reason:
Keep the class grade sheet together throughout submission and review, as
required by the owner, without changing existing grading/edit permissions.

Consequences:
All required report transitions succeed together or none do; missing required
scores or an ineligible report blocks the whole action. Protect transitions
against concurrent score edits/review actions. Draft/correction entry may still
edit one student's values; the whole-class constraint applies to report workflow
actions, not requiring re-entry of every student's unchanged scores. SUBMITTED
locks and FINALIZED immutability remain ADR-024. No absent-student zero or new
grade/class state is introduced. Existing per-student GradeReports are retained;
aggregate storage/constraints and audit representation belong to later database
and architecture design. This resolves Q-011 operation granularity; ADR-067
subsequently resolves report cohorts, not the complete design baseline. No code/schema is created.

## ADR-035: Keep non-held class sessions within existing schedule management

Status: Accepted — 2026-09-27

Decision:
Handle class-wide non-held lessons, including holidays, within NV19/F21 schedule
management rather than adding a separate holiday-management module. The current
scope does not include a center-wide holiday calendar, automatic holiday skipping,
automatic makeup scheduling or individual makeup lessons for absent students.
A lesson not held is not a completed teaching session; do not mark every student
absent merely to treat that lesson as delivered. Preserve ADR-025's actual-session
completion requirement and ADR-032's expected LESSON count and schedule edit guards.

Reason:
Address the operational gap in an existing report function without expanding
the project into separate calendar or individual-remediation workflows.

Consequences:
This is scope agreement, not approval of every postponement/replacement rule.
ADR-036 subsequently resolves pre-start moves and post-start linked replacement
of non-held LESSONs without overwriting old schedules. ADR-037 excludes extra
non-replacement lessons; Q-008 retains actual-occurrence representation. No new
Schedule enum, entity or screen code is introduced by this scope decision.
Do not re-ask the agreed module/automation/individual-makeup scope.

## ADR-036: Move future lessons and replace non-held past lessons with preserved history

Status: Accepted — 2026-09-27

Decision:
Use the combined workflow within NV19/F21. Before a LESSON's scheduled start,
AcademicAffairs may move its date/time/room under ADR-032's existing guards and
resource-conflict checks. It remains the same lesson, not an additional LESSON.
Once the scheduled start has been reached but the class-wide lesson was not
held, do not overwrite its old date/time/room. AcademicAffairs records that it
was not held, with a mandatory reason, and creates a linked replacement LESSON
for the same class. Preserve the original schedule, actor and time.

This operation requires a class neither COMPLETED nor CANCELLED and no
attendance or related score data on the old lesson. If dependent data exists,
block the operation; do not erase it or invent a correction override. The
replacement uses the class's current teacher and a valid future schedule, with
the normal class/room/teacher conflicts checked against class and entry-test
schedules. Recording non-occurrence and creating its replacement must remain
consistent; required updates succeed together or none do, including under races.

The non-held original is historical, not a delivered lesson and not an extra
teaching obligation. Its replacement fulfills that original LESSON obligation;
do not count the pair as two LESSONs or change the program's expected count.
Refine ADR-025: completion requires every lesson still required, including
replacements, to have actually occurred with all students' attendance; the
non-held historical original requires neither occurrence nor fake attendance.
FINAL_EXAM requirements, explicit class transitions and grade rules are unchanged.

Reason:
Keep ordinary future rescheduling simple while handling an unheld past appointment
without violating ADR-032's edit cutoff or losing history.

Alternatives considered:
Always retain an original and create a replacement, even before start. This
offers uniform history but adds records/actions for ordinary future moves.

Consequences:
Guard against concurrent attendance/score entry, class completion, repeated
replacement requests and competing resource reservations; do not duplicate a
replacement obligation or publish a partial change. Keep ADR-035's class-wide,
manual scope and existing ownership/privacy boundaries; no holiday automation,
individual makeup or new teacher-change right. Storage of non-occurrence,
replacement links, history/audit and constraints belongs to later database design,
not a new enum/table now. This resolves the accepted Q-011 workflow/counting
policy and refines ADR-025's completion set; actual-occurrence representation
remains Q-008; extra non-replacement lessons were subsequently excluded by
ADR-037. No code is written.

## ADR-037: Exclude LESSONs beyond the program's fixed teaching count

Status: Accepted — 2026-09-27

Decision:
Do not allow additional non-replacement LESSONs beyond the TrainingProgram's
expected LESSON count. The owner rejected extra lessons beyond that count.
AcademicAffairs may prepare the required timetable through weekly generation
or individual additions under ADR-014, but these cannot exceed the program's
LESSON teaching obligations; require the full count before teaching starts
under ADR-032. No enrichment/optional/additional teaching-session exception is
introduced, and the frozen program count cannot be changed to bypass this rule.

Retain ADR-036 moves and linked replacements: replacing a non-held lesson
fulfills the same obligation, not an extra LESSON. Retained historical schedules
may make the raw Schedule row count exceed the program count; do not apply the
limit to all history rows or block a valid replacement on that basis. FINAL_EXAM
remains separate and does not count toward or substitute for LESSONs.

Reason:
Keep class teaching scope fixed to the agreed program and avoid an additional
extra-lesson workflow.

Consequences:
Count teaching obligations, not original-plus-replacement history twice. Guard
weekly/individual creation and replacement against concurrent count changes or
duplicate obligations. Complete classes under ADR-025/036 without adding extra
lessons or reopening ended classes. Exact count/link/history constraints remain
database-design work. Q-011's extra-lesson scope is resolved; no new enum, field,
screen, fee rule or code is added. Do not re-ask this scope decision.

## ADR-038: Keep the selected Course immutable on an existing Registration

Status: Accepted — 2026-09-27

Decision:
Do not edit the selected Course on an existing Registration, including before
confirmation. To choose another Course, the Student cancels their own record
only when permitted by ADR-022/031 and creates a new Registration under the
existing eligibility rules. Keep the old history; the new record starts entry
testing anew, with no transfer of scores or Band.

Reason:
The owner accepted cancellation/new registration instead of direct course
editing and its dependent booking/result rework.

Consequences:
Extend ADR-016's post-confirmation lock to all existing registrations. Do not
grant Student cancellation after CONFIRMED, staff registration on behalf of a
Student, cancellation of a PAID record or refunds. Q-011's course-change scope
is resolved; no code or schema change is introduced.

## ADR-039: Display current personal information without registration snapshots

Status: Accepted — 2026-09-27

Decision:
Do not snapshot personal-profile fields at registration. Existing and historical
registrations display the Student's current name, birth date, phone and address;
changing an editable field under ADR-019 also changes its historical display.
The Invoice amount remains the tuition snapshot at issuance.

Reason:
The owner accepted current-profile display instead of per-registration copies.

Consequences:
No broader profile-edit or login-email-change right is granted. Preserve
business history, required audits and financial snapshots; this is not authority
to rewrite historical events or payments. Q-011's personal-snapshot scope is
resolved; database relationships and field constraints remain later design work.

## ADR-040: Exclude student class transfers after placement

Status: Accepted — 2026-09-27

Decision:
Do not implement student class transfers after placement, even before teaching
starts. First placement continues to require OPEN, matching Course/Band, available
capacity, ADMITTED and PAID under ADR-023. No remove/reassign workflow is granted.

Reason:
The owner selected the simpler scope instead of a pre-teaching transfer workflow.

Consequences:
Do not bypass this rule through Registration/class cancellation, new placement,
refunds or administrative overrides. Preserve existing placement history.
Q-011's transfer scope is resolved; no code is written.

## ADR-041: Allow Student browser printing of their own Invoice

Status: Accepted — 2026-09-27

Decision:
A Student may print their own Invoice using browser printing. Show its accurate
payment state and amount; an UNPAID Invoice must not appear as proof of payment.
Do not add dedicated application PDF export/download or separate receipt issuance.

Reason:
The owner accepted browser printing instead of view-only or a PDF/receipt feature.

Consequences:
Enforce Invoice ownership. Browser-local save-as-PDF is not an application PDF
generation/download function. No staff/Director export or printing right is
implied; report CSV scope under ADR-013 remains unchanged. Q-011's Student
invoice-printing scope is resolved; no implementation is started.

## ADR-042: Permit audited correction of confirmed Payment metadata only

Status: Accepted — 2026-09-27

Decision:
Accountant may correct only method and transaction_reference on a confirmed
Payment, with a mandatory reason and audit of old/new values, actor and time.
Keep amount_paid, invoice_id and payment confirmation date/time immutable.
Retain the same Payment and Invoice PAID; do not cancel/delete Payment or refund.
Transfer payments still require a transaction reference.

Reason:
The owner accepted narrow metadata correction instead of making every confirmed
Payment field immutable or permitting financial reversals.

Consequences:
Keep Payment.amount_paid equal to Invoice.amount. Correction and its audit must
be consistent/all-or-none, guard concurrent corrections and do not create another
Payment. No Admissions/Admin override is granted. Revenue amount and confirmation
date remain unchanged; ADR-046 subsequently excludes a separate actual-receipt time.
Q-011's payment-correction scope is resolved; detailed field validity/storage
belongs to later database/API design, not code now.

## ADR-043: Require positive tuition and exclude free-course payments

Status: Accepted — 2026-09-27

Decision:
Course tuition must be greater than zero. Exclude free courses, zero-value
Invoices/Payments and an automatic free-enrollment path. Existing admission,
Invoice issuance and full external-payment confirmation remain unchanged.

Reason and alternative:
The owner accepted positive tuition rather than a separate free-course workflow.

Consequences:
Enforce positive tuition/Invoice/Payment amounts, preserving Invoice snapshots
and Payment.amount_paid = Invoice.amount. No discount/scholarship/payment bypass
is introduced. Final range constraints belong to database design; no code.

## ADR-044: Use whole-dong VND amounts

Status: Accepted — 2026-09-27

Decision:
Use VND with whole-dong amounts for Course tuition, Invoices, Payments and
financial totals; no fractional dong. Reject fractional monetary values rather
than silently rounding them. Grading precision under ADR-028 is unchanged.

Reason and alternative:
The owner chose whole-dong amounts for simple entry/reconciliation instead of
two-decimal money in the report. This settles the monetary scale conflict.

Consequences:
Preserve exact monetary arithmetic and financial snapshots. Final SQL precision,
range and API representations remain database/architecture-stage work, not a
migration or finalized numeric type now.

## ADR-045: Set invoice due dates seven calendar days after issuance

Status: Accepted — 2026-09-27

Decision:
Set due_date to the local Invoice issue date plus seven calendar days, including
weekends/holidays, using ADR-047's business timezone. An UNPAID Invoice is overdue
only when due_date is before the current business date; the due date itself is
not overdue. Do not add a separate extension/due-date editing workflow.

Reason and alternative:
The owner accepted a fixed seven-calendar-day interval rather than another
interval or a manually selected deadline.

Consequences:
Keep warning/days-overdue only under ADR-010; no automatic cancellation, account
lock, debt penalty or holiday calendar. Persist the issuance-based deadline;
printing/payment corrections do not regenerate it. No application code.

## ADR-046: Keep payment confirmation time without a separate receipt time

Status: Accepted — 2026-09-27

Decision:
paid_at represents payment confirmation time, as labelled in the report. Do not
add a separate actual-receipt date/time field. Revenue remains based on
confirmation time under ADR-018, not a user-backdated date of external receipt.

Reason and alternative:
The owner accepted confirmation time only instead of an additional receipt field.

Consequences:
Preserve ADR-042's immutable confirmation time and metadata-only correction.
External money may have arrived earlier; do not claim the system records that
instant. No new Payment field, backdating right or code is introduced.

## ADR-047: Use Vietnam time consistently for business dates and schedules

Status: Accepted — 2026-09-27

Decision:
Use Asia/Ho_Chi_Minh (UTC+7) for business dates, class/test schedules, Invoice
issue/due-date calculations, overdue comparisons and date-based report filters.
Do not let each browser/device timezone change these business boundaries.

Reason and alternative:
The owner chose a single Vietnam business timezone rather than another timezone
or device-dependent business dates.

Consequences:
Apply consistent day boundaries across roles and midnight/date-range cases.
This settles business timezone only; timestamp storage, serialization, timezone
identifier mapping and exact filter predicates remain architecture-stage design.
No timezone preference feature or implementation is introduced.

## ADR-048: Issue temporary passwords with mandatory change before business access

Status: Accepted — 2026-09-28

Decision:
Staff account creation and Admin password reset generate a random temporary
password, shown once to Admin for direct/manual delivery. Require the account
holder to change it before accessing business functions. Never log/audit the
plaintext password. Reset does not unlock LOCKED or reactivate DISABLED.
Student self-registration and Admin-only forgotten-password reset remain;
no email recovery or Admin-chosen permanent password workflow.

Reason and alternative:
The owner accepted temporary credentials/forced change instead of Admin-chosen
passwords without a first-use change requirement.

Consequences:
Enforce the business-access gate beyond the UI and store only password hashes.
Temporary passwords expire after 24 hours under ADR-054. Generation, restricted
authentication and delivery failure handling await detailed design; no new
Account state/schema/code now.

## ADR-049: Invalidate every existing session on access or password changes

Status: Accepted — 2026-09-28

Decision:
Manual account lock/disable, Admin password reset and own password change
invalidate every existing session for that account, including the current
session and unexpired JWTs. Require fresh login subject to account status and
ADR-048's mandatory password-change gate, not merely expiry of an old token.

Reason and alternative:
The owner chose all-session invalidation instead of letting tokens stay usable
until their expiry.

Consequences:
Unlock/reactivation must not revive old sessions. Failed-login throttling alone
does not revoke sessions (ADR-052). Revocation persistence, request enforcement
and race handling remain architecture design; no token-table/code approval.

## ADR-050: Allow Admin reactivation of the same disabled account

Status: Accepted — 2026-09-28

Decision:
Admin may reactivate DISABLED to ACTIVE on the same account, retaining its role,
business data and history. Do not restore sessions invalidated by ADR-049; the
user must authenticate afresh. Password reset and reactivation are separate.

Reason and alternative:
The owner accepted reactivation rather than treating DISABLED as permanent.

Consequences:
Do not recreate/delete the account, reopen ended business records or alter role
through this action. Keep ADR-020's Admin availability protection and existing
audit of sensitive account changes. Detailed storage/workflow checks await design.

## ADR-051: Exclude changes to login email on existing accounts

Status: Accepted — 2026-09-28

Decision:
Do not permit Admin to change login email on an existing account, including
staff accounts. Retain ADR-019's Student self-edit restriction; login email is
immutable after account creation in the current scope.

Reason and alternative:
The owner chose no email editing rather than an audited Admin correction workflow.

Consequences:
No delete/recreate bypass or additional staff profile-edit right is granted.
Continue validating email/uniqueness at creation and preserve account/business
history. Validation and normalization details remain for detailed design.

## ADR-052: Define timed failed-login throttling independently of account status

Status: Accepted — 2026-09-28

Decision:
Five consecutive incorrect passwords within fifteen minutes block new login
for fifteen minutes. Successful login or expiry resets the failure counter/window.
The restriction does not change Account to LOCKED/DISABLED or revoke existing
sessions. Manual LOCKED still requires Admin unlock; DISABLED cannot auto-reactivate.

Reason and alternative:
The owner accepted timed self-expiry rather than requiring Admin intervention
after each such failure block.

Consequences:
Apply the same account-scoped login rule across roles without confusing it with
debt warnings or manual lock protection. Exact time-window/reset representation,
concurrency, response privacy and additional rate limiting remain detailed
security design; no new enum/schema/code is introduced.

## ADR-053: Use passphrase-friendly passwords and reject common/compromised values

Status: Accepted — 2026-09-28

Decision:
Passwords must contain 15–128 characters. Allow Unicode and spaces without
mandatory uppercase/digit/symbol combinations. Reject common or compromised
passwords; do not require periodic changes absent a security incident. Keep
ADR-048's mandatory change of temporary passwords before business access.

Reason and alternative:
The owner accepted this policy instead of shorter passwords with fixed character
composition. OWASP Authentication guidance informed the passphrase-friendly
policy; the 128-character maximum is a project choice, not an OWASP requirement.
Source: https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html

Consequences:
Apply validation when choosing a password; never silently truncate passwords.
Exact character counting, denylist source/update method and hashing remain
database/architecture design. No external password-check service/package is
authorized by this decision; never send plaintext secrets to an assumed service.

## ADR-054: Expire temporary passwords 24 hours after issuance

Status: Accepted — 2026-09-28

Decision:
Temporary passwords from staff creation or Admin reset expire 24 elapsed hours
after issuance/reset. Once expired, the user must obtain another Admin reset;
the expired credential cannot authenticate or satisfy the mandatory-change gate.
Expiry does not lock/disable/unlock/reactivate the account.

Reason and alternative:
The owner accepted a fixed 24-hour lifetime instead of another duration or none.

Consequences:
Keep one-time display/manual delivery and forced change under ADR-048, Admin-only
forgotten-password recovery under ADR-020, and reset revocation under ADR-049.
Do not add automatic resend/email recovery. Expiry storage and restricted-session
handling remain detailed design; no schema or application code is created.

## ADR-055: Limit normal sessions to two hours absolute and thirty minutes idle

Status: Accepted — 2026-09-28

Decision:
A normal authenticated session expires two hours after login or after thirty
minutes without activity, whichever occurs first. Activity cannot extend the
absolute deadline. Require fresh login; no refresh-token/automatic renewal or
remember-me feature in this scope.

Reason and alternative:
The owner accepted these project-specific limits instead of eight-hour sessions
or a refresh workflow. OWASP Session Management informed server-side enforcement,
not these exact durations.
Source: https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html

Consequences:
Enforce expiry beyond client timers, including unexpired JWTs where idle expiry
occurs first. Activity definition/background requests, restricted-authentication
lifetimes, persistence and request/race handling remain architecture design.
This does not authorize a token table, auto-save feature or implementation.

## ADR-056: Permit concurrent sessions on multiple devices

Status: Accepted — 2026-09-28

Decision:
Allow concurrent login on multiple devices. A new login does not invalidate
existing sessions; each remains subject to expiry and ADR-049 revocation.

Reason and alternative:
The owner accepted concurrent sessions instead of one active session per account.

Consequences:
No device-management/session-list feature or session-count cap is introduced.
Lock/disable/reset/password change still revoke every existing session. Session
identity and persistence remain architecture design; no schema/code now.

## ADR-057: Revoke only the current session on logout

Status: Accepted — 2026-09-28

Decision:
Logout invalidates the current authentication session server-side, including
any unexpired token for that session. Other sessions remain valid subject to
their expiry and ADR-049 events. Do not add a logout-all button.

Reason and alternative:
The owner chose current-session logout instead of revoking all sessions each time.

Consequences:
Deleting a browser token alone is insufficient. The distinction is between
authentication sessions, not an assumed one-session-per-tab model. Session/token
identity, storage and enforcement remain architecture design; no code/schema.

## ADR-058: Extend business audit coverage without logging ordinary page reads

Status: Accepted — 2026-09-28

Decision:
Keep all previously mandatory sensitive-operation audits. Additionally record
successful course/program/Band configuration changes, class lifecycle transitions
and schedule creation/moves/replacements. Do not log every ordinary page view/read.

Reason and alternative:
The owner accepted focused business audit coverage instead of auditing every read.

Consequences:
Preserve required cancellation/correction/approval/payment/placement/grade audits.
This adds neither a class-teacher history workflow, undo nor editing privileges.
Event mapping/group-write attribution and persistence remain detailed design.

## ADR-059: Audit authentication, account operations and denied authorization

Status: Accepted — 2026-09-28

Decision:
Record successful/failed login, activation of the timed login block, logout,
own password change, staff creation/Admin reset, account-state operations and
denied authorization. An unauthenticated failure is not a verified actor;
do not attribute a guessed login email to an authenticated account holder.

Reason and alternative:
The owner accepted success/failure security-event coverage instead of only
successful actions. OWASP Logging guidance informed the recommendation.
Source: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html

Consequences:
Keep account status/session effects under ADR-048–ADR-057 unchanged. Event
representation, anti-flooding and logging failure behavior remain architecture
design; no alerting/monitoring module, extra lock rule or implementation is added.

## ADR-060: Define Admin audit filters and newest-first paginated viewing

Status: Accepted — 2026-09-28

Decision:
Admin filters AuditLog by business date range, actor, action, entity type/ID
and result. Display a paginated newest-first list. Dates follow ADR-047.

Reason and alternative:
The owner accepted these structured filters instead of a smaller filter set.

Consequences:
Preserve Admin-only access and no normal audit editing/deletion. Do not add
unrestricted free-text request/payload search. Page limits, sort tie-breakers,
event/result types and unauthenticated actor representation await detailed design.

## ADR-061: Keep audit details relevant and exclude credentials/full payloads

Status: Accepted — 2026-09-28

Decision:
Audit details contain event time, actor, action, target, result/reason and relevant
changed fields where applicable, preserving already-required old/new audits.
Never persist passwords/password hashes/JWT/session secrets or entire request/
response payloads in audit logs. Minimize personal data in logs.

Reason and alternative:
The owner accepted relevant event details instead of full payload logging.
OWASP Logging guidance informed avoiding secrets and unnecessary personal data.
Source: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html

Consequences:
This does not remove mandatory business history or add personal-profile snapshots
under ADR-039. Password hashes remain in credential storage, never in logs.
Field allowlists, sanitization, IP handling and event persistence remain detailed
database/architecture design; no new schema or code is created.

## ADR-062: Keep audit viewing in the application without file exports

Status: Accepted — 2026-09-28

Decision:
Admin views/searches logs in the application only; no audit CSV/PDF export.
Retain already-accepted Admin-only viewing and no normal edit/delete rights.

Reason and alternative:
The owner accepted in-application viewing instead of adding Admin audit CSV export.

Consequences:
ADR-013 CSV for authorized business-report exports does not grant audit export.
Log retention/disposal and operational backups remain architecture design, not
a retention period or purge feature accepted here. No implementation is started.

## ADR-063: Report current states without historical as-of reconstruction

Status: Accepted — 2026-09-28

Decision:
Reports use current record states at query time, not reconstructed states/debt
as of a past date. Date filters select applicable record/event cohorts under
ADR-018/047; they do not restore old states.

Reason and alternative:
The owner chose current-state reporting instead of historical as-of reporting.

Consequences:
Retain mandatory business/audit history and financial snapshots; this excludes
historical reconstruction reports, not history storage. A previously issued
Invoice later cancelled is evaluated as CANCELLED even when filtering its old
issuance period. Exact consistent-query/API mechanics await architecture design.

## ADR-064: Separate period total billed from current debt and period revenue

Status: Accepted — 2026-09-28

Decision:
Total billed for a selected period sums amounts of currently non-CANCELLED
Invoices (UNPAID + PAID) issued in that period. Show current UNPAID debt separately,
without applying the payment-date period filter to it. Revenue still uses payment
confirmation date under ADR-018. Course filters may apply consistently.

Reason and alternative:
The owner accepted issuance-period billing instead of all-time invoice totals.

Consequences:
Do not call total billed debt/revenue or derive current debt by subtracting
period revenue from billing. No as-of debt, expense/profit or original-issuance
gross/cancellation-ledger metric is introduced. Detailed filter/UI separation,
query consistency and numeric representations remain design work; no code.

## ADR-065: Count distinct students within course groups and across the report

Status: Accepted — 2026-09-28

Decision:
Count distinct student_id in the filtered Registration cohort. Each course group
counts a student once; the report total counts distinct students across all
groups, not their sum. Keep Registration count separate; repeat records can
increase registration count without increasing distinct student count.

Reason and alternative:
The owner accepted distinct per-course and overall counts instead of only a
report-wide distinct count without course subtotals.

Consequences:
For one student registered in two courses, each course has one student while
the overall student total remains one. Apply the same accepted cohort filters
to both measures; never relabel enrollment results as unique people. No code.

## ADR-066: Report LESSON progress separately from final exams

Status: Accepted — 2026-09-28

Decision:
Lesson progress is actually-confirmed-held LESSON obligations divided by the
program's expected LESSON count. Non-held historical originals do not contribute;
a held linked replacement contributes once to the same obligation. Display
FINAL_EXAM separately, not in the lesson numerator/denominator.

Reason and alternative:
The owner chose LESSON-only progress instead of combined lesson/exam progress.

Consequences:
Retain class completion/attendance guards and ADR-036/037 obligation counting.
Elapsed time or attendance alone does not prove occurrence; Q-008 retains that
representation. CANCELLED classes are not delivered progress, but remain in
class-state counts. No new occurrence field/state/schema is assumed here.

## ADR-067: Calculate pass rate from finalized enrollment results in the finalization period

Status: Accepted — 2026-09-28

Decision:
Pass rate is passing FINALIZED GradeReports divided by all FINALIZED GradeReports
in the selected finalization-date period, within selected course/class filters.
Each enrollment result counts separately, not distinct people across repeated
courses. Exclude unfinalized results; they are not failures. With no finalized
results, display "Chưa có dữ liệu", not 0%.

Reason and alternative:
The owner selected result-finalization dates rather than class-completion dates
for the pass-rate cohort.

Consequences:
Use >=50 passing and rounded final scores under ADR-024/028. Zero passes with
a nonempty finalized cohort can be 0%; an empty denominator is no data. Rate
display precision/query design remains later work. No new read/export rights,
grade states or implementation are introduced.

## ADR-068: Default admissions reporting to the current month and retain historical course choices

Status: Accepted — 2026-09-28

Decision:
Admissions-domain reporting uses Course, registration-date and current-state
filters. Default to the current Vietnam calendar month and all states, including
CANCELLED/REJECTED. Retain inactive courses in the selector for historical records.

Reason and alternative:
The owner accepted a bounded current-month default rather than all-time reporting,
without hiding ended records or history for inactive courses.

Consequences:
Keep ADR-018/047/063/065 metric/date bases. This is Director reporting, not a new
Admissions-role report screen/export or registration-on-behalf permission.

## ADR-069: Separate current training inventory filters from finalized-result periods

Status: Accepted — 2026-09-28

Decision:
Training inventory/progress filters are Course, Band, Class, currently assigned
Teacher and current Class state; default all states, with no inventory date filter.
The finalized-result/pass-rate view additionally filters finalization dates,
defaulting to the current Vietnam calendar month. Retain inactive referenced
courses in course choices.

Reason and alternative:
The owner selected these filters rather than only Course/Class/state, separating
current inventory from period result cohorts.

Consequences:
Preserve ADR-063/066/067. Do not infer a Class creation-date field, a historical
teacher view or delivered progress for CANCELLED classes. No new staff report rights.

## ADR-070: Use current-month financial periods and separate current-debt filters

Status: Accepted — 2026-09-28

Decision:
Revenue/billed reporting periods default to the current Vietnam calendar month,
using their accepted confirmation/issuance date bases respectively. Apply Course
filtering consistently. Current debt has an overdue-only option and no date-period
filter; the period selected for revenue/billed amounts must not filter debt.

Reason and alternative:
The owner accepted Course/overdue filters rather than additional payment-method
or confirming-staff report filters, keeping current debt distinct from flows.

Consequences:
Keep ADR-018/045/047/063/064 and consistent definitions for Accountant/Director.
No historical as-of reporting, actual-receipt timestamp or new invoice-lookup
filters are inferred. Date predicate/query design remains later work.

## ADR-071: Limit Director drill-down to non-identifying group aggregates

Status: Accepted — 2026-09-28

Decision:
Director may view read-only Course/Band/Class group breakdowns where applicable:
counts, class size/progress, finalized-result aggregates and financial totals.
Do not expose individual Student registrations, Invoices or identifying personal
information through these reports. Director still cannot export files or approve
business operations.

Reason and alternative:
The owner chose aggregate-only drill-down instead of individual-record access,
matching the Director's accepted oversight responsibility.

Consequences:
Do not assume every grouping applies to every metric. Accountant operational
invoice lookup stays unchanged; this adds no Admin/Admissions/AcademicAffairs
report rights. Group labels must not introduce individual-identifying detail.

## ADR-072: Export all filtered financial-report results with the displayed report columns

Status: Accepted — 2026-09-28

Decision:
Accountant's financial-report CSV exports all results matching the selected
filters, not only the visible page, using the same report columns. Do not add
hidden fields or extra Student-profile data, or turn report export into separate
registration/Invoice/profile dumps.

Reason and alternative:
The owner selected the complete filtered report rather than current-page-only
CSV, while keeping export bounded to the accepted financial-report permission.

Consequences:
Keep CSV/no XLSX under ADR-013, Director no export and Admin no audit export.
Query/export consistency, file layout/limits/encoding and injection safeguards
remain architecture design; this adds no export role, snapshot lock or code.

## ADR-073: Accept Unicode names with a 100-character limit

Status: Accepted — 2026-09-28

Decision:
Trim leading/trailing whitespace; full_name is 1–100 characters after trimming.
Permit Unicode/diacritics and ordinary name punctuation, without a two-word or
ASCII-only requirement. Apply wherever a name is entered, not only Students.

Reason and alternative:
The owner selected the 100-character proposal rather than 200 characters.

Consequences:
No new staff-profile editing permission. Exact Unicode counting/SQL storage
remain database/architecture design; do not truncate overlong values.

## ADR-074: Validate student birth dates without enrollment-age restrictions

Status: Accepted — 2026-09-28

Decision:
Student birth date must be a real calendar date, not after today's Vietnam
business date. Do not add minimum/maximum enrollment ages.

Reason and alternative:
The owner chose date validity/no future dates rather than age-restricted admission.

Consequences:
Use ADR-047 business dates, not device timezone. No mandatory staff birth-date
field or new enrollment restriction; required/editable Student fields stay ADR-019.

## ADR-075: Validate trimmed creation emails with case-insensitive account-wide uniqueness

Status: Accepted — 2026-09-28

Decision:
At account creation, trim the email, validate its format and limit it to 254
characters. Compare uniqueness case-insensitively across all accounts. Do not
apply provider-specific dot removal or +tag rewriting.

Reason and alternative:
The owner selected case-insensitive rather than case-sensitive uniqueness.

Consequences:
Existing emails stay immutable under ADR-019/051. No email verification or
recovery. Exact validator/Unicode counting/canonical storage and consistent login
comparison remain technical design; this is a project limit, not a standards claim.

## ADR-076: Limit contact phones to the accepted Vietnamese mobile-number format

Status: Accepted — 2026-09-28

Decision:
Contact-phone scope is Vietnamese mobile numbers: ten digits starting with 0.
Accept the equivalent +84 form and spaces/hyphens; normalize to one comparable
representation before account-wide uniqueness checks when a phone is present.

Reason and alternative:
The owner selected this bounded format rather than international/fixed-line support.

Consequences:
This is the project's accepted scope, not a claim about every Vietnam telephone
number. No carrier-prefix catalogue, OTP or ownership verification. Student phone
remains required; do not make staff phone universally mandatory. Canonical storage/
validation implementation remains technical design; login still uses email.

## ADR-077: Keep optional student addresses as free text up to 255 characters

Status: Accepted — 2026-09-28

Decision:
Trim Student address; at most 255 Unicode characters, with an empty value allowed.
Use free text without a mandatory province/district catalogue.

Reason and alternative:
The owner selected 255 rather than 500 characters and retained optional entry.

Consequences:
No staff address requirement or personal-profile snapshots/editing rights beyond
ADR-019/039. Exact character counting/SQL null and storage rules remain database
design; no code or whole-baseline acceptance is implied by this batch.

## ADR-078: Bound Unicode catalogue names without additional global uniqueness

Status: Accepted — 2026-09-28

Decision:
Trim catalogue names: Course/TrainingProgram names 1–150 characters and score-
component names 1–100, with Unicode allowed. Do not require global unique Course/
program names; retain accepted component-name uniqueness inside one program.

Reason and alternative:
The owner selected these limits rather than 255 characters for every name.

Consequences:
Existing configuration freezes remain; no extra name-editing rights or silent
truncation. Exact Unicode counting/SQL types remain technical design.

## ADR-079: Use immutable manually entered Course and Class business codes

Status: Accepted — 2026-09-28

Decision:
AcademicAffairs enters Course/Class codes manually: trim/uppercase, 1–30 ASCII
letters A–Z, digits, underscore or hyphen; no spaces/diacritics. Codes remain
unique within each entity type including inactive/cancelled records and cannot
change after creation.

Reason and alternative:
The owner selected manual stable codes rather than system-generated codes.

Consequences:
Do not reuse codes from historical records or add editing exceptions. This does
not prescribe entity-ID or Invoice-number generation, or relax program freezes.
Normalization/unique enforcement remains database/architecture design.

## ADR-080: Normalize consistently entered room codes across all scheduling

Status: Accepted — 2026-09-28

Decision:
Room codes use trim/uppercase and 1–30 A–Z letters, digits, underscore or hyphen,
without spaces/diacritics. Use the same normalized labels for class and entry-
test conflict checks: p101 and P101 denote the same room label. Staff must use
one consistent code per physical room; do not automatically equate other aliases.

Reason and alternative:
The owner chose entered codes rather than a managed room-selection catalogue.

Consequences:
No new room-management module/catalogue entity. Preserve cross-class/test overlap
checks and accepted schedule editing cutoffs; canonical storage/enforcement
remains technical design.

## ADR-081: Use bounded plain-text reasons and optional comments

Status: Accepted — 2026-09-28

Decision:
Trim Unicode plain text, preserving line breaks. Reasons are 1–1000 characters
where already required; Teacher comments/correction notes are optional and at
most 1000 characters. No rich-text/HTML editor.

Reason and alternative:
The owner selected a 1000-character rather than 2000-character maximum.

Consequences:
Do not require reasons on new operations or comments on every result. Existing
read/edit/result locks and mandatory audit reasons stay intact. No truncation;
exact storage/counting/safe rendering remains technical design.

## ADR-082: Preserve bounded Payment references without global uniqueness

Status: Accepted — 2026-09-28

Decision:
Trim Payment.transaction_reference, at most 100 characters; preserve remaining
content/case/punctuation. Transfer requires a nonempty reference; cash may omit
it. No provider-specific pattern or global reference uniqueness requirement.

Reason and alternative:
The owner selected bounded references without the stronger global-unique rule.

Consequences:
Keep one Payment per Invoice, full-payment equality, confirmation time and
reasoned metadata-only corrections/audits. The reference alone is neither a new
deduplication key nor proof of receipt; no bank integration/identity field or
override. Exact storage/validation remains technical design, not code authority.
