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

## ADR-083: Define operational detail visibility

Status: Accepted — 2026-09-28

Context:
The owner checked A01, A02, A03, A06 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- A01: Tra cứu mọi trạng thái hồ sơ, kể cả đã kết thúc/CANCELLED/REJECTED. Chi tiết: tên/email/SĐT hiện tại, khóa, kết quả đầu vào/Band, xác nhận/duyệt/trạng thái, số tiền và trạng thái hóa đơn, lớp đã xếp. Không xem phương thức/mã giao dịch Payment, điểm danh/bảng điểm đào tạo, ngày sinh/địa chỉ hoặc danh sách hồ sơ cá nhân toàn hệ thống; không thêm quyền sửa/đăng ký thay/xuất.
- A02: Xem tên/email/SĐT hiện tại, khóa/kết quả đầu vào/Band phục vụ ca thi, xếp và quản lý lớp. Hóa đơn chỉ hiển thị trạng thái, không số tiền/Payment. Xem lịch/điểm danh và bảng điểm SUBMITTED/RETURNED/FINALIZED, không giá trị nháp DRAFT của giáo viên. Không thêm quyền nhập/sửa điểm, sửa hồ sơ hoặc tài chính.
- A03: Tra mọi trạng thái gồm CANCELLED; lọc số hóa đơn/khóa/tên-email-SĐT học viên/trạng thái/khoảng ngày phát hành tùy chọn/chỉ quá hạn. Chi tiết hóa đơn, Payment và người/thời điểm xác nhận cùng thông tin liên hệ cần thiết; không điểm/điểm danh. Tách tra cứu nghiệp vụ này khỏi kỳ báo cáo và quyền CSV; không thêm xuất riêng hồ sơ/hóa đơn, in hoặc sửa ngoài quyền đã chốt.
- A06: Trong ca/lớp đang được phân công, xem mã học viên nội bộ, tên/email/SĐT hiện tại và dữ liệu thi/điểm danh/điểm thuộc nhiệm vụ của mình; không ngày sinh/địa chỉ/hóa đơn/Payment hoặc danh sách học viên toàn trung tâm. Không còn được phân công thì mất quyền xem theo nhiệm vụ đó, không thêm quyền sửa hồ sơ.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Limits personal/financial disclosure to the role's current task; no additional edit/export rights. Related: ADR-016/019/029/071.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-084: Place account actions and Student schedule history in existing UI

Status: Accepted — 2026-09-28

Context:
The owner checked A04, A05 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- A04: Có hai phần Hiện tại/Lịch sử, chọn lớp của chính mình khi học nhiều khóa hoặc học lại. Lớp hoàn thành chỉ xem; buổi không tổ chức ghi rõ và liên kết buổi thay thế, không hiện như lịch sắp tới hoặc hai buổi đã dạy. Không xem lớp người khác/điểm chưa FINALIZED; không ngầm chốt enum lưu lịch.
- A05: Dùng menu tài khoản chung trên giao diện hiện có: mọi vai trò tự đổi mật khẩu; chỉ Học viên sửa hồ sơ cá nhân theo các trường đã chốt. Không thêm mã F hoặc quyền sửa thông tin nhân sự. Giữ kiểm tra mật khẩu hiện tại, email bất biến, hủy phiên và bắt đổi mật khẩu tạm.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Refines existing screens without adding F codes, staff profile editing or unpublished grades. Related: ADR-019/020/039/049.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-085: Define catalogue lifecycle and immutable class-program links

Status: Accepted — 2026-09-28

Context:
The owner checked A07, A08, A09 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- A07: Giáo vụ sửa tên/giá cơ sở; giá mới chỉ áp dụng hóa đơn phát hành sau đó, không sửa snapshot tiền đã phát hành. Course INACTIVE chặn đăng ký mới; các hồ sơ đã tồn tại tiếp tục quy trình hợp lệ, không tự hủy hồ sơ/lớp/chương trình. Cho khôi phục ACTIVE trên cùng khóa; mã khóa vẫn bất biến.
- A08: Trước lớp đầu tiên: cấu hình ở DRAFT; chuyển ACTIVE khi Band/khóa hợp lệ, số LESSON dương, thành phần hợp lệ và tổng trọng số 100. Muốn sửa cấu hình ACTIVE chưa dùng thì đưa về DRAFT và kiểm tra lại. INACTIVE chặn tạo/mở lớp mới, không dừng lớp đã dạy/chấm; khôi phục ACTIVE không xóa khóa cấu hình đã sử dụng. Không tạo chương trình trùng khóa/Band hoặc version để né khóa.
- A09: Không đổi TrainingProgram của Class sau khi tạo; chọn sai thì xử lý hủy lớp hợp lệ/tạo lớp mới, không chuyển học viên. Giáo vụ sửa sức chứa chỉ trước ONGOING, trong 1–15 và không dưới sĩ số đã xếp. Giữ nguyên giới hạn đổi giáo viên, mã lớp và điều kiện hủy đã chốt.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Preserves issued money snapshots and first-use freezes; inactive configuration does not cancel existing work. Related: ADR-023/026/028/032/040/079.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-086: Gate initial entry grading at test end and enforce effective closure

Status: Accepted — 2026-09-28

Context:
The owner checked A10, B04 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- A10: Chỉ cho giáo viên được phân công nhập điểm đầu vào lần đầu từ khi ca kết thúc; không áp hạn 24/48 giờ, nhưng phải còn hồ sơ hợp lệ trước xác nhận và ca không CANCELLED. Không nới cửa sổ sửa điểm ADR-029; ABSENT vẫn chỉ sau ca kết thúc và không suy từ thiếu điểm. Điểm đào tạo giữ cửa sổ lớp ONGOING/COMPLETED và DRAFT/RETURNED đã chốt.
- B04: API luôn tính trạng thái hiệu lực: ca chưa hủy và đã đến giờ bắt đầu là CLOSED dù dữ liệu cũ còn OPEN. Ghi CLOSED cùng thao tác ghi hợp lệ kế tiếp; truy vấn chỉ đọc không âm thầm sửa dữ liệu. Không cần cron mới để quyền đóng ca đúng giờ; tất cả thao tác kiểm tra giờ server trong transaction, không mở lại ca đã bắt đầu.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Adds an initial-entry cutoff, not a new correction right. Closure does not need a scheduler or writes on reads. Related: ADR-027/029/030/031.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-087: Confirm actual occurrence and model linked lessons and final exams

Status: Accepted — 2026-09-28

Context:
The owner checked B01, B02, C02, C03 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- B01: Giáo vụ xác nhận riêng từng LESSON/FINAL_EXAM sau giờ kết thúc dự kiến và khi lớp ONGOING; lưu người/thời điểm. Không tự suy từ thời gian hoặc điểm danh. Không có nút đảo HELD về chưa diễn ra; lịch chưa được xác nhận tiếp tục chặn hoàn thành. Không thay cửa sổ nhập điểm danh đã duyệt.
- B02: Mỗi lớp có một nghĩa vụ FINAL_EXAM, riêng số LESSON, sau LESSON cuối theo lịch hiện hành; cấu hình có một thành phần điểm loại FINAL_EXAM. Nếu thi cả lớp không tổ chức, mở rộng quy trình thay thế có lý do/liên kết sang FINAL_EXAM, chỉ khi chưa điểm phụ thuộc và lớp chưa kết thúc; giữ buổi cũ, một nghĩa vụ, không thi lại cá nhân. Đây là bổ sung đã duyệt cho FINAL_EXAM ngoài LESSON của ADR-036.
- C02: Schedule có occurrence_state PLANNED/HELD/NOT_HELD, người/thời điểm xác nhận và lý do/người/thời điểm không tổ chức; có mã nghĩa vụ và replaces_schedule_id cùng lớp/loại. Một lịch hiện hành cho mỗi nghĩa vụ; chuỗi thay thế giữ lịch sử nhưng không tăng số buổi. Lịch lớp CANCELLED bị loại bằng trạng thái lớp, không suy HELD từ điểm danh hoặc đồng hồ.
- C03: ScoreComponentConfig có kind GENERAL/LESSON/FINAL_EXAM; loại này cũng khóa từ lớp đầu tiên. Score có schedule_id bắt buộc với LESSON/FINAL_EXAM, cùng lớp/loại và buổi hiện hành; GENERAL để NULL. Vẫn một Score/học viên/thành phần, không biến thành điểm mỗi buổi vô hạn. Điểm gắn buổi chỉ nhập sau khi buổi đã bắt đầu; không nhập cho buổi NOT_HELD. Phối hợp B02 đã duyệt.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Extends linked replacement to the single FINAL_EXAM obligation; preserves no individual retakes/extra lessons. Adds occurrence and score dependencies to the target model. Related: ADR-025/032/033/035/036/037/066.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-088: Use minute-precision same-day intervals and atomic timetable generation

Status: Accepted — 2026-09-28

Context:
The owner checked B03, B05 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- B03: Mỗi ca/buổi bắt đầu-kết thúc trong cùng ngày Việt Nam, bắt đầu trước kết thúc, độ chính xác phút; khoảng thời gian dạng [bắt đầu, kết thúc) nên được học/thi nối tiếp đúng giờ. Ngoài xung đột phòng/giáo viên đã chốt, chặn đặt ca/xếp lớp làm học viên trùng ca hoặc lịch lớp còn hiệu lực; không tự chuyển lớp/ca để xử lý.
- B05: Nhập khoảng ngày/thứ/giờ/phòng, xem trước các buổi cụ thể, số nghĩa vụ còn thiếu và mọi xung đột; không tự bỏ ngày nghỉ. Chỉ lưu toàn bộ nếu tất cả hợp lệ và không vượt số LESSON; nếu lỗi không lưu phần nào. Không tự sinh lịch sau này và không tạo entity quy tắc lặp; thêm từng buổi vẫn được phép.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Back-to-back sessions are valid, student overlaps are blocked alongside resource conflicts; weekly batches are all-or-none. Related: ADR-014/023/032/037.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-089: Preserve blocked assignments and require sensitive-action conflict previews

Status: Accepted — 2026-09-28

Context:
The owner checked B06, B07 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- B06: Không tự đổi người phụ trách, hủy lớp/ca/hồ sơ hoặc xóa dữ liệu. Người bị chặn không thực hiện nghiệp vụ; giao diện báo rõ tài khoản phụ trách không khả dụng. Đổi người chỉ trong cửa sổ đã chốt; ngoài cửa sổ thì chờ Admin mở/khôi phục tài khoản hợp lệ, không thêm quyền đổi giáo viên giữa khóa hoặc sửa điểm ngoại lệ.
- B07: Trước thanh toán/chốt hoặc trả bảng điểm cả lớp/hủy/reset/khóa-ngừng tài khoản, hiển thị đối tượng và ảnh hưởng; lý do chỉ ở nơi đã bắt buộc. Khi dữ liệu thay đổi sau khi mở form, báo xung đột và yêu cầu tải lại/kiểm tra, không ghi đè tự động. Không thêm người phê duyệt thứ hai; preview hủy hồ sơ ADR-031 vẫn giữ.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Account restoration remains distinct from reassignment; no post-start teacher replacement, second approver or silent overwrite. Related: ADR-023/029/031/049/050.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-090: Separate test bookings and enforce relational lifecycle integrity

Status: Accepted — 2026-09-28

Context:
The owner checked C01, C04, C06, C10, C15, C16 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- C01: Thêm EntryTestBooking liên kết EntryTestAttempt và EntryTestSession; mỗi lần đặt/hủy/vắng có bản ghi riêng, không ghi đè lịch sử. Attempt duy nhất theo hồ sơ/số lượt 1–2; một booking SCHEDULED hiện tại/lượt chưa hoàn thành, kết quả trỏ booking đã hoàn thành. API suy diễn trạng thái lượt từ kết quả/booking hiện tại; vắng/đặt lại không tăng lượt, không tạo lượt ba.
- C04: Lưu Registration.is_unfinished do server quản lý; unique có điều kiện trên student_id/course_id khi true. Cập nhật cờ cùng transaction hủy/từ chối hoặc khi Class COMPLETED và GradeReport FINALIZED; kiểm tra cả dữ liệu gốc khi ghi để tránh cờ lệch. Không thêm Registration COMPLETED, không yêu cầu đạt môn.
- C06: Dùng int IDENTITY cho các bảng nghiệp vụ; khóa kỹ thuật phiên/operation là định danh ngẫu nhiên. FK hạn chế xóa dữ liệu lịch sử, không cascade xóa hồ sơ/điểm/Invoice/Payment/audit. Mã học viên hiển thị là ID nội bộ, không tạo mã tự nhập mới; Course/Class code vẫn tách khỏi PK. Chỉ bỏ cấu hình nháp chưa dùng khi không có phụ thuộc và quyền hiện có cho phép.
- C10: Đặt unique cho email chuẩn hóa/SĐT có giá trị, code khóa-lớp, cặp khóa-Band, tên thành phần trong chương trình, hồ sơ-số lượt, assignment/hồ sơ, điểm/học viên-thành phần, điểm danh/học viên-buổi, Invoice/hồ sơ và Payment/Invoice. Tên thành phần so không phân biệt hoa-thường nhưng có phân biệt dấu; không unique Payment reference. Index FK/cột lọc trạng thái-ngày/Teacher/phòng và phân trang; kiểm tra đúng vai trò/cùng lớp-chương trình bằng server trong transaction, không chỉ FK tồn tại.
- C15: Khi tạo Class, khóa kiểm tra/cấu hình TrainingProgram cùng transaction và từ đó suy khóa bất biến bằng sự tồn tại của bất kỳ Class, kể cả CANCELLED. Bộ Band bị khóa khi có bất kỳ kết quả đầu vào COMPLETED; tuần tự hóa thao tác cấu hình và chấm đầu tiên. Không nút mở khóa/xóa lịch sử để né freeze, không thêm bảng version hoặc snapshot hồ sơ cá nhân.
- C16: Số hóa đơn server cấp HD-YYYY-NNNNNNNN, năm theo ngày phát hành Việt Nam, bộ đếm DB tăng an toàn; không yêu cầu liên tục không khoảng trống/không tái dùng. Quan hệ một hồ sơ tối đa một Invoice, một Invoice tối đa một Payment, một assignment tối đa một GradeReport; các FK nghiệp vụ bắt buộc NOT NULL khi đối tượng đó tồn tại, actor có thể NULL chỉ với sự kiện chưa xác thực. Score chưa nhập là thiếu bản ghi, không 0; finalized_score/Band/Payment chỉ có khi đạt điều kiện tương ứng. Dictionary cuối sẽ ghi rõ từng trường, không mặc định tất cả nullable.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Adds EntryTestBooking and the unfinished flag; identity/FK/unique/freeze and invoice-number contracts are accepted, but the complete dictionary/ERD is not yet accepted. Related: ADR-015/017/021/022/028/031/032/079/082.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-091: Specify exact numeric, time and normalized text representations

Status: Accepted — 2026-09-28

Context:
The owner checked C07, C08, C09 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- C07: Tiền đơn decimal(18,0), dương và không vượt 999.999.999.999.999.999 VND; tổng báo cáo decimal(38,0), API tiền dạng chuỗi số để frontend không mất chính xác. Điểm/trọng số decimal(5,2): điểm 0–100, trọng số >0 đến 100/tổng đúng 100; số LESSON int dương. Tính tổng điểm bằng decimal trung gian rồi half-up một lần như ADR-028; từ chối vượt range, không tự cắt/làm tròn đầu vào.
- C08: Các thời điểm sự kiện lưu UTC datetime2(3), API ISO-8601 có Z. Ngày sinh/hạn thu dùng date; lịch dùng ngày Việt Nam và time(0), ràng buộc phút/same-day B03, chuyển UTC khi so thời điểm sự kiện. Backend áp Asia/Ho_Chi_Minh cho ranh giới ngày/kỳ; không phụ thuộc timezone máy học viên hoặc server.
- C09: Đếm ký tự theo Unicode code point, không byte/UTF-16 unit; từ chối surrogate không hợp lệ, chuẩn hóa NFC văn bản không phải mật khẩu, cấp nvarchar đủ dung lượng tương ứng. Mật khẩu giữ nguyên, không trim/NFC/đổi hoa-thường. Email nhận một địa chỉ ASCII dạng local@domain, không tên hiển thị/khoảng trắng/ký tự điều khiển; trim/so trùng không phân biệt hoa-thường nhưng không bỏ dấu/chấm/+tag. SĐT chuẩn 0xxxxxxxxx từ quy tắc đã duyệt; chuỗi tùy chọn trim rỗng thành NULL, trường bắt buộc rỗng báo lỗi.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Money strings prevent frontend precision loss; Unicode code points require sufficient SQL storage. Passwords remain unnormalized. These are project design contracts, not implemented validators. Related: ADR-028/043/044/047/053/073–082.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-092: Persist revocable authentication sessions and independent login timers

Status: Accepted — 2026-09-28

Context:
The owner checked C11, C12 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- C11: Thêm AuthSession với sid ngẫu nhiên, account_id, issued/expires/last_activity/revoked_at và phiên hạn chế nếu có; Account có security_version. JWT mang sid/version, mọi yêu cầu kiểm tra DB/status/hạn phiên; reset/đổi mật khẩu/khóa/ngừng tăng version và hủy phiên theo ADR-049. Không chỉ đợi JWT hết hạn, không Redis hoặc module quản lý thiết bị.
- C12: Lưu failed_login_count, failure_window_started_at và login_throttled_until độc lập status tài khoản; tính UTC/cập nhật nguyên tử. Lưu must_change_password/temp_issued_at/temp_expires_at, chỉ hash mật khẩu; không lưu bản rõ để Admin xem lại. Throttle/expiry không tự unlock/enable; giữ nguyên 5 lần/15 phút và mật khẩu tạm 24 giờ đã duyệt.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Adds AuthSession/account security version and UTC timers; does not accept cookie transport, idle-activity definition or restricted-session lifetime from D03/D04/D06. Related: ADR-048–ADR-057.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-093: Group audit history and serialize critical writes with replay protection

Status: Accepted — 2026-09-28

Context:
The owner checked C05, C13 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- C05: Lưu actor/time/reason/prior-state ở đối tượng khi cần, AuditLog giữ trường cũ/mới theo allowlist. Dùng operation_id chung cho thao tác mẹ và thay đổi phụ thuộc, gồm mọi GradeReport trong gửi/gửi lại/trả/chốt cả lớp. Giữ lịch sử lần gửi/trả/chốt, không chỉ timestamp cuối; không thêm workflow lịch sử đổi giáo viên lớp hoặc nút sửa/xóa log.
- C13: Transaction ngắn; rowversion phát hiện form cũ, đồng thời khóa tuần tự tài nguyên/capacity/đối tượng để xử lý đặt ca-xếp lớp-thanh toán-hủy-chấm/xác nhận/đổi cấu hình và thu hồi phiên. Không coi rowversion một mình chống được trùng lịch. Thao tác nghiệp vụ quan trọng có request_id theo actor/action, lưu dấu kết quả cùng transaction tối thiểu 24 giờ: gửi lại cùng nội dung không tạo tác động mới, khác nội dung báo 409; kiểm tra quyền hiện tại khi nhận lại kết quả. Không lưu/phát lại mật khẩu tạm. Cơ sở kỹ thuật: [EF Core concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency).

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
History/operation IDs and short transactions preserve all-or-none behavior. Rowversion alone is insufficient for aggregate/range guards. Audit-failure/retention policies D08/D09 remain unaccepted. Related: ADR-029/030/031/034/042/049/058–062.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-094: Read consistent report snapshots and display two-decimal rates

Status: Accepted — 2026-09-28

Context:
The owner checked C14 in `docs/plan/OWNER_REVIEW.md` and explicitly
confirmed approval of A–C, excluding D/E. No alternative was entered.

Decision:
- C14: Mỗi lượt báo cáo/export đọc một snapshot DB nhất quán, trả queried_at; CSV chạy một truy vấn mới theo cùng bộ lọc/cột, không hứa giữ nguyên ảnh chụp trang xem trước. Tỷ lệ đạt/tiến độ hiển thị 2 chữ số lẻ half-up, không làm tròn các thành phần đếm; mẫu số rỗng giữ “Chưa có dữ liệu”. Giữ nguyên nhóm thời gian/trạng thái hiện tại và tổng distinct đã duyệt.

Reason and alternatives:
Adopt the owner's approved recommendation; unselected alternatives are not
requirements. Preserve the previous accepted business boundaries.

Consequences:
Each export is a fresh consistent query, not a frozen prior page or historical as-of report. CSV format/limits/security D10 remain unaccepted. Related: ADR-063–ADR-072.
Acceptance is of design choices only, not E01–E05 outputs, implementation,
new packages, Git initialization or scaffolding.

## ADR-095: Define module ownership and server authority

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D01, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
Một backend modular monolith dùng stack và tên module mục tiêu hiện có: Students quản lý hồ sơ riêng; Admissions xử lý đăng ký/thi đầu vào; Academics quản lý cấu hình/lớp/lịch/điểm; Finance quản lý hóa đơn/thanh toán; Reports đọc báo cáo; Administration quản lý tài khoản/phiên/audit. API DTO → application workflow/transaction → EF Core/SQL Server; quy tắc/quyền/tiền/tổng điểm do server quyết định, frontend chỉ nhập/hiển thị. React tổ chức theo chức năng, không microservice/event bus/framework kiến trúc mới; liên module cùng DB để giữ all-or-none.

Reason and alternatives:
Keep ownership clear and critical cross-module operations atomic on the accepted stack. Avoid adding distributed coordination to the university project.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-001/011/093. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-096: Use versioned DTO APIs with structured errors and stable pagination

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D02, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
API /api/v1; DTO riêng, lỗi code/message/fieldErrors/traceId, không stack trace/SQL/payload bí mật. 400 dữ liệu sai, 401 chưa/hết xác thực, 403 sai vai trò, 404 bản ghi không thuộc phạm vi được xem, 409 trạng thái/cạnh tranh; danh sách mặc định 20, tối đa 100/trang, sort ổn định có ID phụ. Server kiểm tra role và ownership ở mọi endpoint, không chỉ ẩn nút.

Reason and alternatives:
Give clients a consistent contract for field errors, authorization and concurrent edits while limiting list payloads. Full endpoint/DTO designs remain E04 work.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-083/089/091/093. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-097: Count interactive requests as idle activity and share sessions across browser tabs

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D03, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
Chỉ request nghiệp vụ được xác thực do người dùng thao tác mới cập nhật last_activity; không background polling/healthcheck/timer giữ phiên. Các tab cùng browser profile chia cookie/phiên, đăng xuất ở một tab hủy phiên chung; thiết bị/profile khác có phiên riêng. Giữ 2 giờ tuyệt đối/30 phút idle, không refresh; không tạo heartbeat chỉ từ di chuột/gõ phím.

Reason and alternatives:
Prevent background traffic from silently extending sessions and make same-browser logout behavior predictable. Typing without a submitted business request does not extend idle time.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-049/055/056/057/092. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-098: Limit temporary-password authentication to a ten-minute restricted session

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D04, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
Đăng nhập bằng mật khẩu tạm còn hạn chỉ cấp phiên hạn chế tối đa 10 phút, không vượt hạn 24 giờ của mật khẩu; chỉ được đổi mật khẩu tạm/đăng xuất, không nghiệp vụ. Sau đổi, hủy mọi phiên rồi về đăng nhập bình thường; không tự nâng phiên hạn chế thành phiên thường. Reset/hết hạn vẫn do Admin, không email khôi phục.

Reason and alternatives:
Keep temporary access limited to the password-change gate. The user must log in again after changing the temporary password; the restricted deadline can be earlier than ten minutes.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-048/049/054/092. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-099: Use same-origin secure JWT cookies with CSRF protection

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D06, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
JWT ký/kiểm issuer-audience-exp/sid-version, đặt cookie host-only HttpOnly/Secure/SameSite=Strict, không localStorage/sessionStorage. Frontend/API cùng origin; yêu cầu đổi dữ liệu và login có CSRF token/kiểm Origin, không thao tác nghiệp vụ qua GET. HTTPS, escape văn bản thuần, truy vấn tham số hóa, secret ngoài mã nguồn, không lưu hồ sơ/điểm/tokens bền trong browser cache hoặc log đầy đủ. Nguồn: [OWASP Session](https://cheatsheetseries.owasp.org/cheatsheets/Session_Management_Cheat_Sheet.html), [OWASP CSRF](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html).

Reason and alternatives:
Keep browser tokens inaccessible to ordinary JavaScript and define explicit protection for cookie-authenticated writes. Same-origin deployment must support the accepted contract.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-049/092/095. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-100: Keep login errors private and rate-limit login requests by IP

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D07, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
Thông báo đăng nhập chung, không tiết lộ email có tồn tại hoặc trạng thái trước xác thực. Bổ sung giới hạn request login theo IP: 100/phút, vượt trả 429/Retry-After; đây là ngưỡng hạ tầng đề xuất, tách khỏi throttle tài khoản 5 lần/15 phút đã duyệt và không hủy phiên. IP chỉ tin từ proxy được cấu hình; không CAPTCHA/OTP/module cảnh báo mới.

Reason and alternatives:
Avoid revealing account existence/status before authentication and bound login traffic separately from account-scoped password throttling. The IP threshold is a project choice, not a measured capacity.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-052/059/060/092. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-101: Specify bounded UTF-8 financial CSV exports and formula safeguards

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D10, and
asked to review Gói 2 one part at a time. The approved choice is recorded in
`docs/plan/OWNER_REVIEW.md`.

Decision:
CSV UTF-8 BOM, dấu phẩy, CRLF, quote/escape đúng ô; cùng bộ lọc/cột và toàn bộ kết quả theo ADR-072. Giới hạn 10.000 dòng dữ liệu hoặc 5 MB: vượt thì từ chối toàn file và đề nghị thu hẹp lọc, không xuất âm thầm một phần. Ô văn bản nguy hiểm công thức kể cả ký tự full-width được thêm dấu nháy đơn bên trong ô đã quote; giữ dữ liệu DB gốc, kiểm thử trên Excel/LibreOffice mục tiêu và cảnh báo không bảo đảm an toàn sau chỉnh/lưu/mở lại ở mọi phần mềm. Không XLSX/PDF/quyền xuất mới. Nguồn: [OWASP CSV Injection](https://community.owasp.org/attacks/CSV_Injection).

Reason and alternatives:
Bound export size and provide a consistent spreadsheet-oriented format. Formula mitigation is limited to the tested target software; DB text remains unchanged and large exports require narrower filters.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-013/063–072/091/094. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Remaining choices D05/D08/D09/D11–D14/D16/D17 are not accepted by this item.

## ADR-102: Define role-sensitive responsive priorities and the verification scope

Status: Accepted — 2026-09-29

Context:
The owner approved frontend discussion Gói 1, specifically D15, and
asked to review Gói 2 one part at a time. The approval includes the revised
responsive wording presented in Gói 1.

Decision:
Thiết kế bộ kiểm chứng xUnit nghiệp vụ/API, integration trên SQL Server test riêng cho ràng buộc/transaction/races; kiểm tra thủ công 7 vai trò và luồng đầu-cuối/boundary thời gian/số tiền/CSV. UI tiếng Việt; luồng Học viên ưu tiên 375 px, màn hình nghiệp vụ dày của nhân sự ưu tiên 1280 px; tất cả dùng được và kiểm tra ở 375/768/1280 px, nhãn/lỗi rõ, tab bằng bàn phím, không chỉ dựa màu; kiểm thử Chrome/Edge bản ổn định hiện hành. Không thêm test framework, test code hoặc tuyên bố kiểm thử đã chạy.

Reason and alternatives:
Match Student mobile use and dense staff workflows while keeping every in-scope flow usable at all three accepted sizes. This defines future verification, not an executed test result.
The owner chose the proposed recommendation; no alternative was requested.

Consequences:
Related: ADR-001/011/047/091. This is an accepted design contract, not implemented
behavior, permission to add packages/code or E01–E05 output acceptance.
Shell/navigation, colors, fonts and component-library proposals in Gói 2
remain subject to their own review.

## ADR-103: Use a shared responsive application shell for seven roles

Status: Accepted — 2026-09-29

Context:
The owner approved FE01a after requesting that frontend Gói 2 be reviewed
one part at a time. The approved presentation includes moving the account
name/role into the account menu on mobile. OWNER_REVIEW is the approval source.

Decision:
- 1280 px: left sidebar about 240 px, showing icons and menu labels; header
  above the content area.
- 768 px: left navigation about 72 px with expansion to view labels;
  icon labels available on hover/focus.
- 375 px: compact header with a menu button and a drawer from the left
  over the content; single-column page content.
- Header shows the screen name and account area. Mobile account name/role
  appears inside the account menu; preserve ADR-084 account actions.
- Content holds page title, primary action and screen data. Clearly mark
  the selected menu and focus; close the drawer on selection, backdrop or
  Escape and return focus to its opener.
- Seven roles share the shell structure with their authorized menus.
  Role-specific menu grouping and labels are reviewed separately as FE01b.

Reason and alternatives:
The owner chose the proposed shell without an alternative. Desktop navigation
supports switching staff tasks; the mobile drawer preserves screen space.
Tablet expansion keeps the compact icon navigation understandable.

Consequences:
Related: ADR-083/084/102. FE01a is accepted design, not implementation or
E acceptance. FE01b, page/form/state choices, colors/fonts/packages and Gói 3
remain subject to review; fixed roles and business rights are unchanged.

## ADR-104: Group existing screens into role-specific menus and define login landing

Status: Accepted — 2026-09-29

Context:
The owner approved FE01b after agreeing FE01a (ADR-103), while reviewing
frontend Gói 2 one part at a time. OWNER_REVIEW maps menus to existing F codes.

Decision:
Học viên: Trang chủ (F03), Khóa học & hồ sơ (F04–F05), Thi đầu vào (F06–F09), Hóa đơn (F10), Lớp & lịch (F11), Bảng điểm (F12). Tuyển sinh: Hồ sơ tuyển sinh (F13–F14). Giáo vụ: Ca thi đầu vào (F16–F17), Khóa & chương trình (F18, gồm Band/trọng số), Lớp học (F19–F20), Lịch học & thi (F21), Xếp lớp (F22), Duyệt bảng điểm (F23). Giáo viên: Lớp & lịch dạy (F24), Thi đầu vào (F25), Điểm danh (F26), Sổ điểm (F27). Kế toán: Hóa đơn & thanh toán (F28–F29), Báo cáo học phí (F30). Giám đốc: Báo cáo tổng hợp (F31). Admin: Tài khoản (F32), Nhật ký (F33). Menu theo đúng một vai trò, các hành động tạo/sửa/duyệt nằm trong màn hình tương ứng; đăng nhập mới vào mục đầu của vai trò. F01/F02 là màn hình xác thực trước đăng nhập, menu tài khoản ADR-084 giữ riêng. Nhóm menu không tạo mã F hoặc dashboard mới cho nhân sự.

Học viên Thi đầu vào groups booking, test schedule, results/Band and admission
confirmation. Giáo vụ Khóa & chương trình includes Band and weight configuration.
Giám đốc Báo cáo tổng hợp contains admissions, training and finance sections.

Reason and alternatives:
The owner selected the proposed grouping and labels without an alternative.
Grouping related screens keeps navigation concise while retaining all current
screen references and business functions.

Consequences:
Related: ADR-012/083/084/103. Initial fresh login goes to the first listed menu
for that fixed role; no new staff dashboard or role-switch capability is created.
Business actions stay in the relevant screen with existing guards. FE02 page/
navigation behavior, other Gói 2 choices, Gói 3 and E outputs remain unapproved.
This records accepted design, not application implementation or package approval.

## ADR-105: Use addressable pages and preserve list navigation context

Status: Accepted — 2026-09-29

Context:
The owner approved FE02 while reviewing frontend Gói 2 one part at a time.
OWNER_REVIEW records the accepted proposal; the accompanying hosting question
does not approve deployment or maintenance choices.

Decision:
- Lists, details and complex forms have separate URLs for direct access,
  reload and browser Back/Forward; no routing library is selected here.
- Details show an object/state summary and authorized sections or tabs.
  Complex program/class/schedule/gradebook forms use dedicated pages.
  Modals serve confirmations, reasons or short forms; mobile may use almost
  the full screen.
- Returning to a list preserves filters, page and sort. Non-identifying
  course/Band/state/date-period filters and page/sort may appear in URLs.
  Name/email/phone searches stay only in working memory, never in URLs,
  localStorage or sessionStorage.
- Lists and details have clear breadcrumbs and a return path. Every route
  retains D02 authorization, ownership and error contracts; no permissions
  or business workflow change.

Reason and alternatives:
The owner accepted the proposed split without an alternative. Addressable
pages support navigation and complex work; short modal interactions keep
local context. Volatile personal search filters reduce durable disclosure.

Consequences:
Related: ADR-084/096/099/103/104. Reload can restore URL-safe list context,
but must not persist personal search terms. Later responsive-list/form/state/
style choices, packages, Gói 3 and E outputs remain unapproved. Hosting
D11–D14/D16/D17 remains under discussion. No application implementation.

## ADR-106: Use responsive lists and a per-student mobile gradebook

Status: Accepted — 2026-09-29

Context:
After separately approving FE01a/FE01b/FE02, the owner approved the entire
original frontend Gói 2 FE01–FE10 in this chat. OWNER_REVIEW records FE03;
the later detailed shell/menu/navigation decisions retain precedence.

Decision:
Use authorized full tables at 1280 px, reduced-column tables with separate
details at 768 px, and labelled cards/lists avoiding horizontal tables at
375 px. Gradebooks use a desktop matrix with a fixed Student column and a
per-Student mobile view. Lists use pagination, not infinite scroll; preserve
ADR-105 navigation context and volatile personal searches. The original FE01
shell uses no bottom navigation or real role switcher; ADR-103/104 remains.

Reason and alternatives:
Adopt the owner's proposal approval without a requested alternative. Dense
tables support staff work; labelled mobile lists avoid squeezing every column.
Different presentations require equivalent data/action verification by size.

Consequences:
Related: ADR-083/091/096/102–105. No new read/edit rights, F codes or library.
This does not approve FE14 attendance/grade draft save granularity. Gói 3,
nine remaining D choices and E01–E05 stay open. No code or tests executed.

## ADR-107: Separate absent permissions from temporarily unavailable actions

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE04; OWNER_REVIEW is the approval source. Existing
role, ownership, lifecycle and blocked-assignment rules remain authoritative.

Decision:
Hide actions entirely outside the user's permissions. For an authorized role
whose current prerequisites are unmet, show a disabled action with a specific
reason. Show explicit “Chỉ xem” content rather than a disabled edit form for
read-only data. Warn when the responsible account is LOCKED/DISABLED without
suggesting replacement outside the accepted window. Server checks remain.

Reason and alternatives:
The owner chose the existing proposal without an alternative. Explanations
make unmet prerequisites understandable; the UI is not an authorization layer.

Consequences:
Related: ADR-023/029/030/049/050/083/089/096. Disabled reasons must not reveal
data outside the permitted scope. No override/reassignment or extra rights.
Accepted design only; D/E and Gói 3 are not approved by this item.

## ADR-108: Save forms explicitly and preserve server-authoritative validation

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE05 in OWNER_REVIEW, preserving ADR-105 personal
data restrictions and the previously accepted field/concurrency contracts.

Decision:
No autosave; provide an explicit save button. Validate early on the client,
but server validation decides. Show field errors below fields and form-wide
errors at the top. Warn before leaving unsaved changes. Uppercase permitted
Course/Class/room inputs on blur; do not truncate/round invalid over-limit
data or normalize passwords. Display the canonical phone after successful
save. Do not automatically merge or overwrite stale forms, or persist
sensitive drafts in localStorage/sessionStorage.

Reason and alternatives:
The owner accepted the proposal without an alternative. Explicit saves expose
the transaction boundary; warnings help avoid losing in-memory edits but do
not guarantee recovery on shutdown/session expiry. There is no autosave recovery.

Consequences:
Related: ADR-028/073–082/089/091/093/096/099/105. Fixed codes remain immutable
after creation. FE14 batch save granularity is not accepted here. No new form
library, validation rules, code or E acceptance.

## ADR-109: Define visible loading, submission and API error states

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE06; D02 errors and C13 replay safety were already
accepted, not replaced by frontend feedback.

Decision:
Use structure-matching skeletons for initial loading; distinguish no data
from no filter matches. Disable submission and show progress while pending.
For 409 explain the state/concurrency conflict and require reload/rechecking,
never silent overwrite. Authenticated 401/expiry returns to login with a clear
reason; 403 shows insufficient rights; 404 shares missing/out-of-scope wording.
Keep generic pre-authentication login errors. Success uses short feedback;
important errors remain visible, not solely transient toasts. Use D02
fieldErrors/traceId without sensitive payload/stack disclosure.

Reason and alternatives:
The owner selected the proposal without an alternative. Visible state explains
what the user can do next; disabling a button alone cannot prevent duplicate
effects after network failure or another tab.

Consequences:
Related: ADR-055/057/089/093/096–100. Preserve request_id safeguards and inspect
current state after an uncertain write outcome. Do not persist sensitive
forms to survive login or extend idle through background activity. No code,
test results or Gói 3/D/E approval.

## ADR-110: Use one explicit sensitive-action confirmation pattern

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE07. ADR-089 already requires sensitive-action
previews; the one-time temporary-password delivery pattern FE18 is Gói 3.

Decision:
Use a shared confirmation pattern showing target/current state, direct
effects and dependent data. Require reasons only where already mandatory.
Name the actual action on the button, such as payment confirmation, record
cancellation or whole-class finalization. No two-layer confirmation for
ordinary actions. One-time temporary-password display is a separate exception;
its exact delivery/copy/close acknowledgment FE18 remains unapproved.

Reason and alternatives:
The owner accepted the proposal without an alternative. Clear impact/action
labels keep confirmation meaningful without adding repetitive ordinary prompts.

Consequences:
Related: ADR-031/034/042/048/054/081/089/093/105. No second approver, new reason
requirement, operation permission or FE18 approval. Recheck state/rights at
commit; preview is not a reservation or guarantee. Design only, no code/E.

## ADR-111: Prefer accessible local Tailwind components without new UI libraries

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE08. The fixed stack requires explicit permission
before adding frameworks/packages; no implementation exists yet.

Decision:
Use Tailwind with accessible project-local components. Do not add UI, form,
table, calendar or chart libraries in the baseline. Request a package only
for a concrete need that cannot reasonably be met with the accepted stack.

Reason and alternatives:
The owner accepted the original proposal without an alternative. This reduces
dependency scope but puts keyboard/focus/semantics verification on the local
component implementation; using Tailwind does not prove accessibility.

Consequences:
Related: ADR-001/011/099/102/103. Not an instruction to build components now,
nor a change to pre-existing stack dependencies. No new package authorized.
Gói 3/D/E stay open.

## ADR-112: Present Vietnamese labels and exact localized business values

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE09. Numeric/time/text storage and API contracts
ADR-091 remain distinct from presentation.

Decision:
Use Vietnamese status labels and internal English enums. Display dates as
dd/MM/yyyy, 24-hour times as HH:mm, and “Giờ Việt Nam” where ambiguity matters.
Use vi-VN display, for example 1.234.567 ₫ and 75,50/100. Money remains decimal
digit strings over the API and must not pass through a lossy numeric type.
Do not change existing precision/range/rounding, timezone or validation.

Reason and alternatives:
The owner selected the proposal without an alternative. Localized display
is readable while exact transport avoids changing large monetary values.
Formatting must not become another calculation or mutate stored data.

Consequences:
Related: ADR-028/043/044/047/091/094/102. Device locale/timezone cannot override
Vietnam business boundaries. No new enum/state/currency/library, or code/E.

## ADR-113: Use a light administrative visual direction with accessible cues

Status: Accepted — 2026-09-29

Context:
The owner approved Gói 2 FE10 after reviewing the entire common frontend
proposal. OWNER_REVIEW records the approved direction, not a rendered mockup.

Decision:
Use a light, compact administrative/academic UI with system fonts, white/gray
surfaces and blue as the primary color. Red/yellow/green statuses include
text or icons, focus is clear, touch targets are at least approximately 44 px,
and reduced motion is supported. This is not a new brand identity.

Reason and alternatives:
The owner accepted the proposal without an alternative. System fonts avoid a
new font dependency; compact staff layouts still require legible spacing,
usable touch targets and verification of actual color contrast/keyboard use.

Consequences:
Related: ADR-001/102/103/111/112. Exact tokens/assets and implementation checks
remain design-detail/verification work, not an accessibility pass. Gói 2
FE01–FE10 is now accepted; Gói 3 FE11–FE19, nine D choices and E01–E05 remain
unapproved. No code, package installation, deployment or public tunnel.

## ADR-114: Hash passwords locally and use a pinned offline blocklist — D05

Status: Accepted — 2026-09-29

Context:
The owner checked D05 in OWNER_REVIEW. ADR-048/053–056/091/098 already fix
password semantics, one-time delivery, expiry and restricted sessions.

Decision:
Use ASP.NET Core PasswordHasher with parameters appropriate to the selected
runtime, documented when implementation dependencies are locked. Generate
32-character temporary passwords using a cryptographically secure generator;
show Admin once, never persist/log plaintext. Merge the offline SecLists
10k-most-common and probable-v2_top-12000 breached/common-password lists.
Pin their revision, preserve licensing when integrating and review updates
quarterly. Never send entered passwords to an external checking service.

Reason and alternatives:
The owner accepted the proposal without an alternative. Local hashing and
offline checks avoid external password disclosure. A finite list cannot detect
every previously exposed password; it does not replace hashing or throttling.
Sources: [SecLists lists](https://github.com/danielmiessler/SecLists/tree/master/Passwords/Common-Credentials),
[license](https://github.com/danielmiessler/SecLists/blob/master/LICENSE).

Consequences:
Related: ADR-053/054/091/098/119. No concrete hash parameters, list revision or
packages installed yet. Existing exact Unicode/space/length/password rules
remain unchanged; frontend secret-delivery interaction FE18 is still open.

Implementation checkpoint2026-10-04: P08 applied/reverified PASS2d3c9c8,
IdentityV3/PBKDF2-HMAC-SHA512220000iterations/128bit salt/256bit subkey,84char
output fits varchar512. Shipped SecLists revision56da8d991239cc52b0da594db53457504a757670
and license/content hashes verified; exact Ordinal union15310entries, raw password
policy unchanged. Secure32character generator has no plaintext persistence/logs.
See [P08 task card](plan/P08_TASK_CARD.md) for sources/verification/benchmark.
FE18/once-only delivery, expiry/TX/session behavior are still future integration.

## ADR-115: Roll back mandatory audited mutations; keep security-event logging best-effort — D08

Status: Accepted — 2026-09-29

Context:
The owner checked D08. ADR-058–062/093 require business/account histories and
secret-free security events but had not settled failure handling.

Decision:
Mandatory business/account-change audit writes share the mutation transaction.
If they fail, roll back the whole operation, including related status,
credential and session-revocation changes. Authentication/logout/access-denial
events use best-effort logging with bounded, secret-free operational fallback.
Logging failure must not bypass security checks or restore an already revoked
logout session. Record the logging incident for the operator; do not add user
notifications or a new notification subsystem.

Reason and alternatives:
The owner accepted the proposal without an alternative. Committing mandatory
mutations without their history would violate traceability; making pure security
event logs transactional with access checks could obstruct safe logout.
The distinction must be explicit in every workflow.

Consequences:
Related: ADR-048/057/058/059/061/092/093/120. Mandatory account changes remain
atomic even when they include revocation; pure logout must not be undone for
an event-log failure. No secrets/full payloads in fallback, no code/tests yet.

## ADR-116: Preserve mandatory histories and expire security-only logs/IP after 90 days — D09

Status: Accepted — 2026-09-29

Context:
The owner checked D09. Mandatory business/account audit differs from pure
authentication/access telemetry; app users have no audit deletion/export rights.

Decision:
Keep mandatory business/account audit with project history, with no in-app
delete button. Operationally clean up pure authentication/access logs and IP
after 90 days, never applying this limit to delete mandatory history. Store IP
only where needed for security; remove expired IP while retaining mandatory
audit fields. Record every event actually processed. Rate-limited requests may
be aggregated by count/IP/minute window, but throttle counters must not be lost.
Unauthenticated actor is NULL, not the email supplied in a login attempt.

Reason and alternatives:
The owner accepted the proposal without an alternative. Bounded telemetry/IP
reduces retention and flooding while retaining required accountability.
This is the accepted project policy, not a claim of legal compliance.

Consequences:
Related: ADR-058–062/093/100/115/120. Operators must distinguish event categories
and redact expired IP without deleting mandatory audit. No app purge/export,
secret retention or assumption that cleanup has already run.

## ADR-117: Validate one-center scope against 10,000 students and 50 concurrent users — D11

Status: Accepted — 2026-09-29

Context:
The owner checked D11. The target is a student project used on demand, with
historical courses/registrations preserved and no deployed load measurements.

Decision:
Use one center, up to 10,000 students and 50 concurrent users as the expected
validation envelope. Preserve historical courses/registrations. Do not add
multi-branch, multi-tenant or distributed architecture.

Reason and alternatives:
The owner accepted the proposal without an alternative. A bounded reference
scope fits the accepted modular monolith and makes future validation concrete;
it is not evidence that these users already exist or performance is proven.

Consequences:
Related: ADR-095/102. Do not implement a business rejection for student 10,001.
D12 performance/availability remains deferred; actual environment and measured
results are still required before claiming a capacity or service guarantee.

## ADR-118: Adopt encrypted daily SQL backups and reference recovery targets — D14

Status: Accepted — 2026-09-29

Context:
The owner checked D14. Actual hosting/performance choices D12/D13 are deferred;
no system/data backup exists at this design stage.

Decision:
Once the system exists, back up SQL daily, encrypted and stored separately
from the primary data drive. Retain seven daily and four weekly copies; test
restore monthly. Reference targets: RPO at most 24 hours and RTO at most
two hours. The owner must designate storage and operational access before
actual operation; owner responsibility is now settled by ADR-120.

Reason and alternatives:
The owner accepted the proposal without an alternative. Separate encrypted
copies and restore trials address data loss, but targets are not demonstrated
guarantees. On-demand laptop shutdown can interrupt daily automation; the
operating procedure must account for missed runs without silently reducing
the accepted cadence or claiming an unverified RPO/RTO.

Consequences:
Related: ADR-093/120. Destination, credentials/key handling, automation and
reference restore environment still need concrete configuration. No backup,
data copy or restore executed now; D12/D13 deferral and E acceptance unchanged.

## ADR-119: Select .NET 10/EF Core 10 and SQL Server 2022 Developer for non-production — D16

Status: Accepted — 2026-09-29; SQL selection superseded by ADR-162 on 2026-10-04.
.NET/EF/stack and compatible-patch policy retained.

Context:
The owner checked D16. The accepted ASP.NET Core/React stack stays unchanged;
the intended use is development, testing and internal project demonstration.

Decision:
Select .NET 10 LTS/EF Core 10 and SQL Server 2022 Developer for non-production
dev/test/internal demos, not production data/operation. Keep React/Vite/
TypeScript/Tailwind/xUnit. After full baseline acceptance, choose compatible
supported patch releases and lock dependencies in the implementation contract;
do not request approval for each compatible patch. Production requires its own
edition/license choice before operation.

Reason and alternatives:
The owner accepted the proposal without an alternative. Supported LTS runtime
and a development/test SQL edition fit the intended scope; Developer does not
authorize production. Sources: [Microsoft .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core),
[SQL Server 2022 editions](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022?view=sql-server-ver16).

Consequences:
Related: ADR-001/011/114/118. No installed-version/machine-compatibility claim,
patch lock, new package, setup or deployment. This does not accept D13 hosting,
public exposure, E artifacts or production licensing.

## ADR-120: Assign the project owner as the default sole operator — D17

Status: Accepted — 2026-09-29

Context:
In OWNER_REVIEW the owner requested a single explicit choice of operator and
a default thereafter. Asked once, the owner confirmed responsibility for all
release, secrets, backup/restore and project maintenance.

Decision:
The project owner is the default person responsible for all those operations.
Ask again only if the owner requests a change of responsible person. App Admin
continues only account/audit administration within existing rights; this
assignment creates no app Admin/DBA privilege or automatic infrastructure access.
Handoff documentation must cover setup/configuration, the rights matrix,
password reset/session revocation, recovery/error handling and requirement →
workflow → data/API/test traceability. Review dependencies, secrets and blocklist
periodically before release, including the quarterly blocklist cadence ADR-114.
Do not activate integrations/coding agents or transmit project data externally.

Reason and alternatives:
The owner expressly selected the proposed default, not another operator.
A persistent assignment avoids repeated ownership questions while keeping
operational responsibility distinct from application permissions.

Consequences:
Related: ADR-011/048/057/060/114/118/119. Preserve the owner's original D17 note
and record the resolved checkbox. Storage/access setup still needs configuration
before operation. This approval authorizes no installation, release, tunnel,
backup execution, delegation, code or E acceptance.

## ADR-121: Use per-registration Student home cards — FE11

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE11 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Dùng các thẻ theo từng hồ sơ/khóa của chính học viên: bước hiện tại, việc có thể làm tiếp, ca thi/buổi học gần nhất, hóa đơn chưa thanh toán/quá hạn và liên kết kết quả đã FINALIZED. Mỗi hồ sơ có nút dẫn tới thao tác hợp lệ hoặc lời giải thích đang chờ chấm/duyệt/thanh toán/xếp lớp; không ép một tiến trình chung khi học nhiều khóa. Tiến trình chỉ là cách trình bày dữ liệu hiện có, không enum/điểm tiến độ mới; không hiện điểm DRAFT/SUBMITTED/RETURNED cho học viên. Chưa có hồ sơ thì dẫn tới chọn khóa. Không thêm dashboard nhân sự, hộp thư hoặc thông báo tự động.

Reason and alternatives:
Thẻ riêng từng hồ sơ giữ đúng ngữ cảnh khi học nhiều khóa; không chọn một tiến trình chung dễ lẫn trạng thái.

Consequences:
Trang chủ là dữ liệu suy diễn đúng scope, không module thông báo hoặc trạng thái lưu mới.
Related: ADR-083/084/104. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-122: Use scoped local calendars with week and agenda views — FE12

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE12 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Dùng component lịch cục bộ tái sử dụng trong F07/F11/F21/F24, chỉ dữ liệu đúng scope từng màn hình. Desktop có lịch tuần và danh sách theo ngày; 375/768 px ưu tiên danh sách theo ngày, nút Hôm nay/chọn ngày và lọc lớp của mình hoặc lớp được phép quản lý. Click buổi mở chi tiết và hành động hợp lệ; không kéo-thả/resize để đổi lịch, không thêm calendar library. Phân biệt LESSON/FINAL_EXAM/ca đầu vào bằng chữ kèm màu. Lịch Học viên giữ Hiện tại/Lịch sử đã chốt; lịch bị hủy/NOT_HELD nằm ở lịch sử, liên kết buổi thay thế và không tính đôi. Chọn ngày không tự suy buổi đã HELD; không đổi quyền đặt ca CLOSED hoặc cửa sổ sửa lịch.

Reason and alternatives:
Lịch tuần hỗ trợ nhìn tổng quan desktop, agenda phù hợp mobile. Chỉ agenda ít công hơn nhưng kém tổng quan; không chọn kéo-thả để tránh lẫn quyền dời lịch.

Consequences:
Hai cách xem phải tương đương về dữ liệu/quyền; không suy HELD từ thời gian, không cấp quyền sửa lịch mới.
Related: ADR-026/030/032/036/084/087/103/111. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-123: Invalidate weekly schedule previews and commit complete valid batches — FE13

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE13 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Hai bước Nhập quy tắc → Xem trước: khoảng ngày/thứ/giờ/phòng cho LESSON, danh sách ngày cụ thể, số nghĩa vụ đã có/còn thiếu/sắp tạo và xung đột phòng/giáo viên/lớp/học viên trong phạm vi được xem. Nút Tạo toàn bộ lịch chỉ khả dụng khi batch hợp lệ; sửa đầu vào làm mất hiệu lực preview. Có lỗi thì quay lại sửa/tạo lại preview hoặc dùng luồng thêm buổi riêng, không tự bỏ dòng lỗi/ngày nghỉ hoặc lưu phần hợp lệ. Server kiểm tra lại lúc commit; preview không giữ chỗ tài nguyên. 409 yêu cầu xem trước lại, không ghi đè. FINAL_EXAM quản lý riêng, không tự sinh kèm LESSON; không thêm quy tắc lặp lưu DB/automation.

Reason and alternatives:
Preview giúp đối chiếu các ngày cụ thể; giữ all-or-none thay vì tự bỏ ngày lỗi hoặc lưu một phần, vì các lựa chọn đó làm thay đổi nghĩa vụ lịch đã duyệt.

Consequences:
Preview không đặt giữ chỗ. Server phải kiểm tra lại tài nguyên, số nghĩa vụ, quyền và trạng thái tại commit.
Related: ADR-032/037/088/093/096/108. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-124: Save attendance per lesson and draft grades per student — FE14

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE14 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Điểm danh: nút Lưu điểm danh buổi gửi các giá trị đã đánh dấu/thay đổi của một LESSON trong một batch all-or-none; có lỗi thì không lưu phần nào. Ban đầu Chưa ghi, giáo viên chọn Có mặt/Vắng; không mặc định cả lớp có mặt/vắng, không biến chưa ghi thành dữ liệu. Có thể lưu khi còn người chưa ghi, vẫn chặn hoàn thành lớp đến khi đầy đủ; Chưa ghi không là enum Attendance mới. Điểm nháp: Lưu học viên lưu các điểm thành phần đã nhập/thay đổi và nhận xét của một học viên cùng nhau, all-or-none cho lần lưu đó; desktop nút từng hàng, mobile cùng đơn vị trong trang học viên. Cho thiếu thành phần khi lưu nháp; ô chưa có điểm không thành 0, bỏ trống không tự xóa điểm đã lưu. Tổng từ server, thiếu điểm ghi Chưa đủ điểm. Hiện Chưa lưu/Đã lưu, cảnh báo rời khi còn sửa; không autosave/import Excel/lưu browser bền/merge form cũ. Gửi bảng điểm toàn lớp là thao tác riêng FE15, không đổi cửa sổ/quyền hay thêm quyền xóa điểm.

Reason and alternatives:
Đơn vị lưu theo buổi/học viên làm phạm vi thành công-thất bại rõ trên cả desktop/mobile. Lưu toàn lớp điểm nháp khiến một lỗi chặn mọi sửa; lưu từng ô thủ công tăng request và dễ sót.

Consequences:
FE14 chốt đơn vị lưu nháp, không thay đơn vị gửi/trả/chốt toàn lớp. Thiếu bản ghi không là Có mặt/Vắng/0; không thêm quyền xóa điểm.
Related: ADR-024/028/033/034/087/093/106/108. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-125: Separate draft saves from whole-class grade workflow actions — FE15

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE15 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Tách rõ Lưu nháp và Gửi/Gửi lại bảng điểm cả lớp. Giáo viên xem trạng thái lưu/thiếu thành phần theo từng học viên; chặn gửi khi còn sửa chưa lưu, lớp chưa COMPLETED, điểm thiếu hoặc trạng thái chưa hợp lệ. Giáo vụ xem bảng SUBMITTED/RETURNED/FINALIZED theo quyền, không giá trị DRAFT. Gửi/trả/chốt dùng một confirmation có mã lớp, số học viên, trạng thái và ảnh hưởng; trả lại bắt buộc lý do và giữ điểm/nhận xét. Không checkbox chọn riêng người gửi/trả/chốt, không duyệt hai tầng. Server kiểm tra mọi học viên và chuyển toàn lớp hoặc không ai; 409 tải lại/rà lại, không tự ghi đè. Student chỉ thấy FINALIZED; chốt bất biến, không thêm mở khóa/đảo trạng thái.

Reason and alternatives:
Hiển thị rõ lưu nháp khác gửi cả lớp giúp tránh gửi khi còn dữ liệu chưa lưu. Không chọn chuyển trạng thái từng học viên hoặc thêm tầng duyệt, vì trái phạm vi đã chốt.

Consequences:
Chuyển trạng thái cả lớp vẫn all-or-none và kiểm tra tại server; Student chỉ thấy FINALIZED, không mở khóa bảng điểm.
Related: ADR-024/028/034/067/083/089/093/110/124. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-126: Apply report filters explicitly and show authorized cards and tables — FE16

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE16 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Ưu tiên thẻ chỉ số và bảng chi tiết nhóm, chưa cần biểu đồ hoặc chart library. F31 có Tuyển sinh/Đào tạo/Tài chính; F30 chỉ tài chính theo quyền. Nút Áp dụng bộ lọc chạy query mới; khi đang sửa lọc, kết quả cũ ghi rõ bộ lọc đang áp dụng và queried_at, không trộn hai lần query. Tách doanh thu/tổng phải thu trong kỳ, công nợ hiện tại không kỳ ngày, tiến độ LESSON/FINAL_EXAM và kỳ kết quả chốt. Mẫu số rỗng là Chưa có dữ liệu, không 0%; tổng từ toàn tập lọc, không cộng trang/nhóm distinct. Director chỉ drill-down nhóm không định danh, không export. Accountant CSV theo bộ lọc đã áp dụng/toàn tập/cột đã chốt, query mới có thể khác trang trước, giữ giới hạn/an toàn D10. Không thêm metric, profile dump, xuất XLSX/PDF, lưu report-as-of hoặc quyền báo cáo mới.

Reason and alternatives:
Thẻ/bảng thuận tiện đối chiếu số và không thêm thư viện. Biểu đồ trực quan hơn nhưng tăng phần phải duy trì; không chọn trộn dữ liệu cũ với bộ lọc đang sửa.

Consequences:
Tách query cũ/bộ lọc nháp/queried_at, giữ cohort, distinct và giới hạn CSV. Không thêm metric, dữ liệu định danh cho Director hoặc quyền xuất.
Related: ADR-063–072/091/094/101/111. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-127: Refresh authorized Student invoice data before browser printing — FE17

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE17 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Nút In hóa đơn chỉ cho học viên sở hữu; lấy lại dữ liệu hợp lệ trước khi mở bản in, lỗi/không còn quyền thì không in bản cũ như dữ liệu mới. Bố cục A4 dọc gọn/đen-trắng rõ, ẩn menu/nút; chỉ số hóa đơn, tên hiện tại của chủ hồ sơ, khóa, tiền snapshot, ngày phát hành/hạn và trạng thái được phép xem. UNPAID ghi Chưa thanh toán — không phải xác nhận đã thu tiền; CANCELLED ghi Đã hủy; PAID ghi Đã thanh toán đúng dữ liệu. Cảnh báo quá hạn theo quy tắc hiện có, không tự đánh PAID khi bấm in. Không thêm mẫu biên lai, chữ ký/đóng dấu giả, trường Payment ngoài scope, API/file PDF hoặc quyền in cho nhân sự; dùng chức năng in của trình duyệt.

Reason and alternatives:
Lấy lại dữ liệu và ghi rõ trạng thái tránh trình bày bản cũ như mới hoặc UNPAID như chứng từ thu tiền. Không chọn API PDF/biên lai vì ngoài phạm vi.

Consequences:
Chỉ Student sở hữu in qua browser; bản in không tạo Payment, quyền nhân sự, chữ ký hoặc dấu xác nhận mới.
Related: ADR-039/041/043–047/083/091/112. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-128: Use a one-time credential result block with explicit optional copying — FE18

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE18 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Sau cấp/reset thành công, hiển thị khối kết quả riêng trong F32 có email/vai trò, mật khẩu tạm và hạn 24 giờ từ server, cảnh báo không thể xem lại. Nút Sao chép chỉ ghi clipboard khi Admin chủ động bấm và trình duyệt cho phép; nếu bị chặn thì chọn chữ/sao chép thủ công. Không tự copy/đọc clipboard/gửi email/in hoặc tải file secret; cảnh báo clipboard có thể còn giữ mật khẩu sau đóng, không hứa tự xóa clipboard. Có checkbox Tôi hiểu sau khi đóng sẽ không xem lại được và nút Đã ghi nhận — Đóng; không coi tick là bằng chứng người dùng đã nhận mật khẩu. Đóng/rời màn hình/đăng xuất/hết phiên xóa plaintext khỏi state, không URL/storage/cache/log/replay secret; refresh/tắt tab có thể làm mất và không cam kết ngăn được. Mất phản hồi cấp/reset: kiểm tra trạng thái trước, không tự lặp để lấy secret; nếu không nhận được mật khẩu thì Admin xác nhận reset mới có chủ đích. Không tự mở khóa tài khoản; không đổi chính sách generator/expiry/quyền đã chốt.

Reason and alternatives:
Khối kết quả/cảnh báo/ghi nhận hỗ trợ tránh đóng nhầm. Chỉ sao chép thủ công ít rủi ro clipboard hơn nhưng bất tiện với mật khẩu 32 ký tự; copy tùy chọn không chứng minh bàn giao thành công.

Consequences:
Không hứa xóa clipboard hoặc chống mọi refresh/tắt tab. Mất phản hồi cần kiểm tra trạng thái, không tự retry/reset để lấy secret; reset mới cần chủ đích Admin.
Related: ADR-048/049/054/091/093/098/099/109/110/114. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-129: Show paginated audit event details and operation links — FE19

Status: Accepted — 2026-09-29

Context:
Ngày 29/09/2026 owner yêu cầu ghi nhận Gói 3 sau lượt trình toàn bộ FE11–FE19
và đối chiếu OWNER_REVIEW. Đây là phê duyệt trực tiếp trong chat, không suy
đồng ý từ ô trống hoặc từ Gói 2; không có ngoại lệ được nêu. Ô FE19 được
cập nhật từ xác nhận này; đề xuất/ghi chú đã có được giữ nguyên.

Decision:
Giữ danh sách từng sự kiện mới nhất trước/phân trang và bộ lọc đã chốt; chi tiết mở riêng với thời gian/người/hành động/đối tượng/kết quả/lý do, trường cũ → mới theo allowlist. Sự kiện chưa xác thực ghi Chưa xác thực, không gán email nhập làm actor. Chi tiết có liên kết xem các sự kiện cùng operation_id để hiểu thao tác cả lớp/hủy phụ thuộc, vẫn phân trang/đúng quyền và không đổi đơn vị trang thành nhóm tùy ý. Không raw request-response/secret/full payload, không sửa/xóa/khôi phục dữ liệu hoặc xuất file. IP chỉ khi được lưu hợp lệ và chưa hết hạn; đã dọn thì ghi Không còn lưu, không dựng lại IP. Bản ghi gộp rate-limit nếu có ghi số lần/cửa sổ, không giả thành từng actor đã xác thực.

Reason and alternatives:
Giữ từng sự kiện phân trang, liên kết operation_id giúp hiểu thay đổi cả lớp/phụ thuộc mà không đổi đơn vị trang. Raw payload hoặc xuất file không được chọn vì vượt scope/quy tắc giữ log.

Consequences:
Chi tiết chỉ allowlist/cũ-mới, actor chưa xác thực không gán email. Không tái dựng IP đã dọn, không sửa/xóa/undo/export.
Related: ADR-058–062/093/096/115/116. Quyền, cửa sổ nghiệp vụ, kiểu dữ liệu và stack đã chốt
không đổi. Đây là hợp đồng thiết kế, chưa là code/test pass hoặc nghiệm thu
E01–E05. D12/D13 vẫn hoãn; không cho phép cài đặt, Git/scaffold, migration,
triển khai/tunnel hoặc tự kích hoạt integration.

## ADR-130: Apply the DBMS330284 database rubric to E03–E04

Status: Accepted — 2026-09-30

Context:
Owner supplied `instructions/HuongDan_Project_Rubric_DBMS330284.docx (1).pdf`
as the source of database requirements and explicitly accepted applying its
database technical and application-integration criteria to E03–E04. The PDF
is for a separate DBMS course; the existing English Center product baseline
and accepted ADRs remain authoritative for business behavior.

Decision:
E03 must design an ERD and relational schema of at least eight related 3NF
tables, with at least five meaningful CHECK/UNIQUE/DEFAULT constraints, five
triggers, five views, five indexes (at least one non-PK), five stored procedures,
five scalar/table-valued functions, five transaction scenarios, a concurrency
or recovery case, and at least four distinct SQL Server Role/Login permissions.
Procedures use TRY...CATCH. E04 must specify how the application invokes
procedures/functions and uses database permissions; implementation/demo must
later prove these objects work and explain index benefit. The seven application
roles are not silently equated with SQL Server principals. Trigger choices
must preserve application-layer mandatory AuditLog writes and established
transaction/authorization rules. No new product feature or blanket delete
permission is inferred from the rubric's generic CRUD examples.

Reason and alternatives:
The rubric's measurable criteria matter for the owner's assessment. Ignoring
them risks an otherwise valid domain model missing required database work;
adopting the whole DBMS course's report/submission format would silently
change this project's scope. Apply the technical and integration requirements
only, and retain the accepted business contracts.

Consequences:
Track proposed SQL objects and their real workflow uses in E03/E04, then
verify quantities, behavior, concurrency, permissions, index effects and
application calls during implementation. This acceptance is not E03/E04
sign-off and authorizes no SQL scripts, migration, scaffolding or app code now.

## ADR-131: Accept E03 package A's overall data model and consistency conventions

Status: Accepted — 2026-10-01

Context:
Owner explicitly replied đồng ý to the presented E03 package A after selecting
the domain-grouped approach. DATABASE_SPEC E03-v0.1 is the review artifact;
this approval covers the overall model and the four presented conventions,
not unseen domain-field or SQL-object details in packages B–G.

Decision:
Keep the 22 target entities and add ReplayOperation and InvoiceNumberCounter
as technical persistence tables: 24 tables in the E03 overall model.
ReplayOperation records operation results for at least24 hours without secret
responses; InvoiceNumberCounter supports safe Vietnam-year invoice numbering.
Use FK/unique constraints for relations, current personal profiles and issued
Invoice tuition snapshots. Schedule replacements retain obligation identity
and predecessor links; use a server-owned is_current projection for at most
one current Schedule per obligation. Mutable edit paths use rowversion;
component changes also touch TrainingProgram version and Score/comment saves
touch GradeReport version, so parent tokens observe child changes. Accepted
money/score/time/Unicode contracts remain the foundation.

Reason and alternatives:
Separate technical stores make replay and numbering explicit in the dictionary.
A current Schedule projection supports a filtered unique constraint over
replacement history. Parent-version updates detect stale aggregate edits where
child-only rowversion would leave the parent token unchanged. Detailed storage
and enforcement alternatives remain reviewable in the corresponding packages.

Consequences:
Sync package A acceptance and target-table count; next review B (Role, Account,
StudentProfile, AuthSession). Individual fields/constraints, SQL objects and
locking protocol in B–G remain proposals until reviewed. E03 stays unchecked
and stage3 doing until the owner accepts the consolidated artifact; no SQL,
migration, application code or execution is authorized by this approval.

## ADR-132: Accept E03 package B's account, profile and session persistence design

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented package B following ADR-131. Its
scope is Role, Account, StudentProfile and AuthSession persistence, using
existing identity, role and session policies as inputs rather than reopening them.

Decision:
Keep seven fixed Role rows with unique codes and one required Role FK per
Account; do not rely on fixed numeric role IDs. Account stores normalized
unique email, unique phone when present (required for Student), name, hash
and security state. Separate login-failure counters/throttle from manual
LOCKED/DISABLED status. Temporary-credential issue/expiry form a consistent
bundle. StudentProfile is one per Student Account and stores DOB/address;
name/phone remain in Account. Profile edits check both rowversions and save
all requested changes atomically.
Use email varchar(254), phone varchar(10), name nvarchar(200), address
nvarchar(510) and hash varchar(512); hash capacity must be checked against
the selected runtime, not silently truncated. AuthSession uses a random
256-bit binary key, Account FK, UTC issue/expiry/activity/revocation,
security-version snapshot and session scope. DB validation follows accepted
session policy; activity updates are monotonic and never undo revocation.
Student registration creates Account/Profile/mandatory audit together;
security mutations and related revocation share their mandatory transaction.

Reason and alternatives:
Splitting identity/contact from Student-only fields avoids repeated profile
data and imposing Student fields on staff. Separate throttle state avoids
conflating a temporary login delay with administrative account blocking.
Checking both profile tokens detects stale edits across the two-row aggregate;
checking only one could overwrite changes made to the other row. This costs
an additional concurrency check and can require users to reload after a conflict.

Consequences:
Record the presented B design in DATABASE_SPEC and affected memory. Additional
unpresented constraint/index/enforcement details remain subject to review;
this is not blanket acceptance of every SQL detail or consolidated E03.
Next review C's configuration and entry-test persistence, including the proposed
PENDING-to-TESTING transition. E03 stays unchecked; no code or SQL execution.

## ADR-133: Accept E03 package C's configuration and entry-test persistence design

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented package C: Course/Band, TrainingProgram/
ScoreComponentConfig, Registration, EntryTestSession/Attempt/Booking and
BandAssignment. The existing PENDING enum lacked an explicit path in E02's
transition matrix; the proposed refinement was presented with this package.

Decision:
Keep separate Course/Band tables with unique codes/thresholds and program
uniqueness per Course/Band. Program owns component configuration; use
nvarchar(300) for Course/program names, nvarchar(200) for component names,
decimal(18,0) money and decimal(5,2) scores/thresholds/weights. Activation checks
weight total100 and exactly one FINAL_EXAM component. Detect configuration
freeze from retained use history rather than a duplicated frozen flag;
price edits do not rewrite issued Invoice snapshots.
Registration uniqueness applies only while is_unfinished; retain cancellation/
rejection actor/time/reason. Creation starts PENDING, first booking changes it
to TESTING, and first result changes it to BAND_ASSIGNED. Attempt-two booking
keeps BAND_ASSIGNED and pending-result confirmation guards; cancellation/absence
does not roll it back or erase Band. This refines an existing enum, not a new
state, permission or retake policy.
Separate sessions, logical attempts and historical bookings; changing a booking
creates a new appointment row. Unique constraints protect one current booking
per attempt; composite FK identifies the result's booking in that same attempt.
Effective closure follows time checks, without an additional stored state or
cached occupancy count. One current BandAssignment per Registration references
its best attempt without copying best_score. Grading/correction, best/Band and
mandatory audit commit together; absence/ungraded does not imply score zero.

Reason and alternatives:
Normalized configuration and booking history preserve existing business locks
and avoid overwriting appointments. Deriving freeze/closure/occupancy avoids
duplicated flags and counts that can drift, at the cost of guarded reads and
transactions. A retained current Band projection speeds operational reads but
requires atomic recalculation. Explicit PENDING transitions fill E02's omitted
creation/first-booking edge without reopening the accepted admission workflow.

Consequences:
Sync the approved transition in requirements and W05/W07/W08/matrix of E02-v1
with a dated refinement note; E01/E02 remain accepted. C approval covers the
presented design, not unseen column/CHECK/index/locking or SQL-object details.
Next review D's Class/assignment/Schedule persistence. E03 remains unchecked;
no SQL, migration, application code, deployment or executed tests are authorized.

## ADR-134: Accept E03 package D's Class, placement and Schedule persistence design

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented package D after ADR-133. Existing
Class lifecycle, placement rights, occurrence/replacement and conflict rules
remain inputs; this records the storage and transaction design presented.

Decision:
Class stores program/Teacher, unique class code, initial PLANNED status,
capacity, rowversion and cancellation actor/time/reason. Derive size from
StudentClassAssignment rather than a second mutable counter.
Each Registration has at most one assignment; retain assigning actor/time
and reference Class/Registration without duplicating student_id. Create its
GradeReport DRAFT in the same placement/mandatory-audit transaction. Do not
prepopulate Score or Attendance; this creates no DRAFT viewing rights.
Schedule stores Vietnam date/minute times, room/type, obligation identifier,
predecessor, current projection and occurrence/history bundles. Enforce one
current occurrence per already-scheduled obligation and at most one current
FINAL_EXAM per Class. An allowed pre-start move updates the existing row with
old/new audit; a non-held replacement retains NOT_HELD history and creates a
future occurrence with the same Class/type/obligation, counted once. HELD
retains its authorized confirming actor/time. Cancelled Class schedules remain
historical but do not occupy resources.
Placement, weekly batches, moves and replacements recheck prerequisites and
conflicts at commit and persist mandatory audit atomically; previews reserve
no resources. Existing completion/Teacher/capacity/time restrictions remain.

Reason and alternatives:
Derived size avoids an additional counter that can drift. Explicit assignment
provenance and early DRAFT creation give each enrolled learner a stable report
aggregate, at the cost of storing reports before any score. Obligation and
predecessor links retain replacement history without counting extra lessons;
overwriting the old occurrence would lose it. Current projection requires
transactional maintenance and a service check for exactly one current row.

Consequences:
Sync D's presented design, initial report creation in W15 and target memory;
additional unpresented column/CHECK/index/SQL locking details remain reviewable.
Next review E's Attendance/Score/GradeReport persistence. E03 stays unchecked,
stage3 doing; no SQL, migration, application code or executed tests.

## ADR-135: Accept E03 package E's Attendance, Score and GradeReport persistence design

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented package E after ADR-134. Attendance
windows, per-LESSON/per-Student saves, whole-Class review, publication and
FINALIZED locks remain the accepted inputs; this records their persistence.

Decision:
Attendance is unique per Class assignment/LESSON, with required present and
no default. Unrecorded attendance has no row. Score is unique per assignment/
component, decimal(5,2), without default/empty rows; zero is valid but absent
Score is not zero. Both retain the last recording actor/time and rowversion.
GENERAL Score has no Schedule; LESSON/FINAL_EXAM must reference a current
occurrence of that same Class/type, retaining the accepted entry windows.
GradeReport is unique per assignment and initially DRAFT (ADR-134). Store
final_score only at FINALIZED; compute draft/submitted totals from current
scores, showing incomplete components explicitly. Comments/reasons use
nvarchar(2000) with the accepted1000-code-point limit and required/optional
semantics. Score/comment saves touch the parent report version; stale edits
conflict, not silently overwrite. Save granularity remains ADR-124.
Latest submit/review fields support queries; each send/resend/return/finalize
is retained in mandatory AuditLog grouped by the whole-Class operation.
Do not add GradeHistory or a stored passing flag. Finalization calculates and
rounds once, publishes and updates Registration's unfinished predicate in one
transaction. Existing Student-own/AcademicAffairs visibility and FINALIZED
immutability remain unchanged.

Reason and alternatives:
No default rows distinguish missing attendance/scores from deliberately entered
values. Finalized-only final_score avoids a stored draft total going stale
after edits, at the cost of computing interim totals. Latest metadata plus
mandatory immutable audit preserve all review cycles without another history
table. Parent concurrency tokens protect the aggregate even for child-only edits.

Consequences:
Sync the presented E design and finalized-only storage in requirements/E02.
Additional unpresented CHECK/index/SQL enforcement remains reviewable; next
review F's Invoice/Payment/AuditLog/ReplayOperation/InvoiceNumberCounter.
E03 stays unchecked and stage3 doing; no SQL, migration, code or executed tests.

## ADR-136: Accept E03 package F's financial, audit and technical-store persistence design

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented package F after ADR-135: Invoice,
Payment, AuditLog, ReplayOperation and InvoiceNumberCounter. Existing financial,
audit, replay and numbering policies remain inputs, not reopened decisions.

Decision:
Invoice is unique per Registration and by invoice number; amount is the issuance
snapshot, issued_at is UTC and due_date is a Vietnam date. Store UNPAID/PAID/
CANCELLED, derive overdue and retain cancellation provenance. Do not store a
personal-profile snapshot. Payment is at most one per Invoice for its full
amount; Payment, PAID and mandatory audit commit together. Transfer reference
is required, not globally unique, stored as nvarchar(200) with the accepted
100-code-point limit. Allowed method/reference corrections retain latest actor/
time/reason plus each event's audit; money, link and confirmation remain immutable.
AuditLog is application-written with allowlisted JSON, not an audit trigger.
Separate MANDATORY from SECURITY with expiry metadata for the accepted90-day
security/IP retention; preserve mandatory history. No edit/purge/export API,
secrets or raw payloads. Maintenance permissions remain subject to G/E04 review.
ReplayOperation is unique by verified actor/action/request_id; keep a content
digest and safe result references for at least24h. Same key/content has no new
effect; different content conflicts409. Recheck rights before returning results.
Temporary-password issue/reset keeps only a completed marker, never the secret
or a replayable plaintext response. Mutations/audit/replay share their transaction.
InvoiceNumberCounter is one row per Vietnam issuance year and allocates
HD-YYYY-NNNNNNNN within approval's transaction. Never reuse issued numbers;
eight-digit exhaustion fails and rolls back, without resetting the counter.

Reason and alternatives:
Snapshots preserve issued amounts while derived overdue avoids a second state
that can drift. Atomic collection avoids paid status without its Payment/audit.
Safe references and marker-only credential results support duplicate-request
handling without durable secret/PII responses; a lost credential response still
requires an intentional new reset. A yearly counter keeps numbering coupled to
approval rather than a separately committed allocator, at the cost of contention
within that year. Exact locking and verification remain the next review topic.

Consequences:
Record the presented F acceptance and affected target memory. Unpresented SQL
field/CHECK/index/hash serialization/cleanup permissions/locking details remain
reviewable; this is not blanket approval of the dictionary or consolidated E03.
Next present G1 SQL-object roles and application paths, then G2 SQL permissions,
transactions/concurrency/snapshot/indexes and rubric verification. Both are still
proposals. E03 stays unchecked and stage3 doing; no SQL, migration, application
code, deployment or executed tests are authorized by this approval.

## ADR-137: Accept E03 package G1's SQL-object purposes, counts and application paths

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented G1 after ADR-136. ADR-130 adopts the
DBMS technical/application-integration rubric; G1 proposes meaningful SQL
objects while keeping accepted application ownership and product rules.

Decision:
Use five triggers to protect used Program/component/Band configuration and
Attendance/Score relationships and state locks. Keep six views for admissions,
test bookings, current timetable, Class/progress, FINALIZED grades and finance
facts with explicit one-object grains and no multiplied aggregates.
Use six read-only functions for effective session state, Band selection,
unfinished Registration, Class obligations, grade totals and overdue warnings.
Missing scores are not zero; functions introduce no mutation, state or penalty.
Use seven procedures for admissions search, test roster, grade review, three
management reports and invoice-number allocation. The allocator participates
in approval's caller transaction, not separate approval or commit.
Services keep authorization/ownership/workflow and application-written audit;
views/procedures grant no new product visibility. Procedures use parameterized
inputs/allowlisted sort and TRY...CATCH error handling. Repositories must actually
call the mapped procedures/functions, not merely create them for a count.

Reason and alternatives:
Meaningful reusable queries/calculations satisfy the adopted rubric while
retaining clear service-owned workflows. Only LINQ/inline queries would not
demonstrate actual application SP/UDF calls. Moving all workflows into procedures
would scatter authorization/audit/replay. The chosen balance costs SQL lifecycle,
mapping and service/function parity checks, including trigger-compatible EF writes.

Consequences:
Record the presented purposes/counts/application paths in DATABASE_SPEC and
affected memory. G1 approval is not blanket acceptance of names/signatures,
every SQL detail, grants or race-safety. Next review G2 SQL permissions,
transaction/locking/snapshot/indexes and rubric verification; then consolidate
the actual E03 artifact. E04 still specifies DTOs/signatures/mapping and callers.
E03 stays unchecked and stage3 doing. No SQL, migration, application code,
deployment or executed tests are authorized by this design approval.

## ADR-138: Accept E03 package G2's SQL permissions, consistency and verification strategy

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented G2 after ADR-137. The adopted DBMS
rubric requires real SQL permissions, transactions/concurrency and query/index
evidence. Existing seven web roles, save units, audit/replay, reference scale
and on-demand laptop operation remain settled inputs.

Decision:
Use four distinct SQL permission groups: least-privilege application runtime;
finance reporting on its finance view/procedure; academics reporting on its
timetable/Class views and training procedure; Director reporting via the three
aggregate report procedures only, without identifying fact/base-table access.
Reporting principals cannot collect payment, enter/finalize grades or mutate
business data. These are separate from the seven web roles. Demonstration
principals use a test database with synthetic data and add no app permissions.
Keep schema/backup/cleanup operational principal separate from runtime; never
put that credential in browser/source or grant runtime db_owner/sysadmin.
EF writes, invoice allocator, mandatory audit and replay share connection and
transaction; failure rolls back the entire accepted save unit. Recheck rights,
session/state and concurrency guards before committing. Use transaction-owned
sp_getapplock request/object gates together with rowversion and DB constraints.
Start with one common timetable gate for occupancy mutations; unrelated
timetable operations may wait, requiring measurement at the accepted50-user
reference scope, not an unverified performance guarantee.
Use one SNAPSHOT transaction per report/export for consistent totals/details;
export remains a fresh query. Choose indexes for real queries, not duplicate
PK/UQ or create every candidate merely for rubric counts. Demonstrate at least
five before/after queries and five rollback cases: booking, approval/Invoice,
collection, timetable batch/replacement, whole-Class grade finalization. Verify
GRANT/REVOKE/DENY, two-connection races and actual application SP/UDF calls.

Reason and alternatives:
Object permissions plus server authorization preserve scope without a SQL login
for every web user/role. A separate operator principal reduces runtime power.
Common timetable locking is easier to reason about than immediate resource/day
partitioning, at the cost of concurrency. Rowversion/unique alone do not prove
cross-record conflict/capacity safety. Transaction snapshots cost version-store
resources, and useful indexes cost writes/storage; actual measurement remains
required. Changing lock partitioning after measurement requires a design review.

Consequences:
Record the presented G2 mechanisms and verification objectives, and present
DATABASE_SPEC E03-v1 (Review), including the remaining physical-detail scope
and E04 handoff limits, for actual artifact acceptance. Exact SQL signatures,
grant lists, timeout/runtime settings and implementation proofs remain review/
E04 or later work, not inferred from this package approval. Preserve old ADRs,
E03 unchecked and stage3 doing until the owner accepts the consolidated artifact.
No SQL, migration, application code, provisioning or tests are authorized here.

## ADR-139: Accept E04 package1's shared API version, replay-key and private-search transport

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented E04 Gói1 after whole-artifact E03-v1
acceptance. Established ownership, save units, concurrency/replay/privacy and
error contracts remain unchanged. This approval concerns the presented shared
wire-format choices, not unseen details or the complete E04 architecture.

Decision:
Represent mutable aggregate versions in DTOs as opaque Base64 strings.
Mutations carry expectedVersions; profile edits carry both Account and
StudentProfile versions. A stale version returns409 without overwriting.
Use a UUID in the Idempotency-Key header for commands with the accepted replay
contract. Retry of the same intent retains key and content; a changed/new
intent gets a new key. Never replay temporary plaintext credentials.
Personal search filters such as name/email/phone go in a POST body, not URL
path/query. Non-identifying filters may use GET. Keep sensitive payloads out
of search logs and durable browser storage, and retain server authorization.

Reason and alternatives:
DTO version maps accommodate multi-aggregate forms better than introducing a
parallel ETag/If-Match mechanism. A header separates replay metadata from each
business DTO; a body request_id is possible but repetitive. GET for personal
search would expose PII in URL history/logging; POST for all lists is possible
but unnecessary. The choices require token validation, intent/key management
and privacy-safe logging; POST itself does not secure a payload.

Consequences:
Record exactly this presented scope in SYSTEM_ARCHITECTURE_SPEC and affected
memory. Further mapper/key-list/encoding validation/canonical digest/replay
pipeline details remain to be reviewed in the actual E04 artifact; do not
infer blanket acceptance from this package. Next propose Auth/session API
mechanics, then Administration and the remaining module/frontend/SQL/runtime
contracts. E04/E05 remain unchecked; D12/D13 deferred. No application code,
SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-140: Accept E04 package2a's Auth endpoints, minimal context and CSRF exchange

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói2a after ADR-139. Existing account,
password, cookie/JWT, restricted-session, idle, revocation and privacy policies
remain inputs. This records the presented API mechanics, not whole E04
acceptance or unseen pipeline/runtime details.

Decision:
Use five routes under /api/v1: GET /auth/csrf for request-token bootstrap;
POST /auth/login for credential verification, JWT cookie and minimal context;
GET /auth/me for current authorized context without extending idle;
POST /auth/change-password with current/new password and Account version,
followed by fresh login on success; POST /auth/logout for current-session
server revocation and cookie removal.
Context carries account identity/contact/role/scope, Account version and
server time/deadlines, not JWT, raw session IDs or hashes.
Use ASP.NET Core's antiforgery mechanism. Keep the request token only in RAM,
send X-CSRF-TOKEN and obtain it again after login/logout. Keep the previously
accepted same-origin secure cookies and token/Origin checks.
Invalid CSRF returns403/CSRF_INVALID; obtain fresh context/token but require
deliberate user resubmission, never automatic mutation retry. Invalid sessions
still return401. Do not put passwords into replay storage or retain them for
retry after an unknown password-change response.

Reason and alternatives:
Framework antiforgery avoids designing a custom token protocol. Request-token
JSON/RAM and a header fit the accepted browser-storage constraints, compared
with a JavaScript-readable token cookie. A minimal context supports role/scope
routing and password changes without exposing bearer credentials. Bootstrap
and identity changes cost additional requests and cross-tab stale-token handling.

Consequences:
Record the presented scope in SYSTEM_ARCHITECTURE_SPEC and affected memory;
unpresented projection/cache/status/pipeline particulars remain subject to the
actual consolidated E04 review. Next propose Administration2b endpoints for
staff creation, state/reset, safe unknown results and audit reading. Preserve
accepted policies and old ADRs; E04/E05 unchecked, D12/D13 deferred. No code,
SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-141: Accept E04 package2b's Administration command, result and audit API groups

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói2b after ADR-140. Account roles,
staff creation/Student self-registration, reset/state guards, one-time
credentials, audit visibility and retention remain settled requirements.
This records the presented API groups and mechanics, not whole E04 acceptance.

Decision:
Under /api/v1/admin, use POST /accounts/search for personal filters and
GET /accounts/{id} for versioned administrative information, not learning or
finance profiles. Use POST /staff-accounts with name/email/one existing staff
role/optional phone; do not create Student on behalf of another person.
Use POST /accounts/{id}/reset-password and separate /lock, /unlock, /disable,
/restore commands rather than arbitrary field/status editing.
Staff-create/reset/four state commands use Idempotency-Key; reset/state
commands also send Account version. Effects, mandatory audit and replay
completion must remain transactionally consistent.
First successful reset/create response alone may show the temporary password.
Replay confirms completion without regenerating or returning a password.
GET /command-results/{requestId}?action=... reads only the calling Admin's
safe outcome; no result is not proof of failure. Inspect authorized Account/
audit before deliberately initiating a new reset.
Provide paginated audit list/detail/related-operation APIs with existing
filters/scopes. No role/email editing, Account deletion, audit export or
extra business access for Admin.

Reason and alternatives:
Separate command routes make input/effects/authorization clearer than a
generic mutable Account API, at the cost of more endpoints. A secret-free
outcome read supports lost-response handling better than guessing from a
mutable Account alone; absence must not be treated as failure. Full response
replay is rejected because credentials and personal snapshots cannot be cached.

Consequences:
Record precisely the presented groups in SYSTEM_ARCHITECTURE_SPEC and affected
memory. Unpresented exact DTO fields/statuses/action codes/transitions/query
signatures remain for actual consolidated E04 review, not blanket acceptance.
Next propose Students3a registration/profile/home/catalogue API contracts;
then complete Admissions/Finance/Academics/reports/frontend/SQL/runtime and
verification. E04/E05 unchecked, D12/D13 deferred; old ADRs unchanged. No code,
SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-142: Accept E04 package3a's Student registration, profile, home and catalogue API

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói3a after ADR-141. Existing Student
self-registration/contact edit, home-card, Course visibility and privacy
policies remain settled. The approval concerns the presented six endpoints
and save/unknown-result mechanics, not the complete E04 artifact.

Decision:
Use POST /api/v1/auth/register for self-registration, atomically creating
Student Account/Profile and then returning to login without auto-created session.
Use GET/PUT /api/v1/students/me/profile for own profile: email is read-only,
and saving full name/DOB/phone/address sends both Account/Profile versions.
Stale returns409 without overwriting.
Use GET /api/v1/students/me/home for per-Registration current step/next action,
nearest events, own invoices and FINALIZED grade links.
Use GET /api/v1/students/me/courses and /courses/{id} for ACTIVE selection
catalogue with pagination/exact tuition strings; referenced old Course
information remains in own history.
Resolve /me ownership from verified session, not another Student ID. Private
endpoints require Student NORMAL. Registration/profile saves do not use
Idempotency-Key: registration uniqueness/transaction and two profile versions
are their guards, with no password/full-form replay cache.
Never auto retry an unknown result. Deliberate login may check uncertain
signup; reread profile before deciding on another save. Home links do not
replace backend authorization/state checks.

Reason and alternatives:
Per-screen DTOs and /me routes separate personal and business scopes better
than a broad /students/{id} entity response, at the cost of separate navigation
requests. Atomic creation and two-version contact saves preserve consistency.
No auto-login keeps authentication explicit. Replay caching of sensitive forms
or credentials is unnecessary and incompatible with the accepted privacy scope.

Consequences:
Record exactly the presented scope in SYSTEM_ARCHITECTURE_SPEC and affected
memory. Unpresented exact DTO/cache/error/sort/query particulars remain for
consolidated E04 review. Next propose Student Admissions3b Registration/test/
confirmation contracts; then remaining staff/Finance/Academics/report/frontend/
SQL/runtime/verification. E04/E05 unchecked, D12/D13 deferred, old ADRs unchanged.
No code, SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-143: Accept E04 package3b's Student Admissions API groups and preview/replay mechanics

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói3b after ADR-142. Registration,
entry-test attempts/bookings, confirmation, ownership and concurrency rules
remain accepted requirements. This approval concerns the presented API groups
and mechanics, not every detailed guard token or the complete E04 artifact.

Decision:
Under /api/v1/students/me provide own Registration list/detail/create,
separate cancellation-preview/cancel, entry-test-options/bookings, separate
Booking cancel/reschedule, own test calendar/results, separate confirmation-
preview/confirm and caller-scoped command-results.
Creating/cancelling Registration, booking/cancelling/rescheduling a test and
confirming admission are six Idempotency-Key commands. Effects, mandatory
audit and replay completion share a transaction.
Rescheduling cancels the old Booking and creates the new Booking in one
transaction, not independent UI cancel-then-create requests.
Forms/previews carry versions of related records and the attempt/booking set
seen by the user, not just the Registration token. Preview does not reserve
a slot. Server rechecks authorization/state/time/capacity/overlap at commit;
conflicts return409 for deliberate reload, not automatic reselection/retry.
Confirmation does not approve admission or create an Invoice. Outcome reads
are caller-only; no result does not prove failure. Existing business rules,
roles and visibility remain unchanged.

Reason and alternatives:
Explicit commands and read-only previews expose effects clearly and protect
cross-record consistency better than generic Registration/Booking PATCH.
Atomic rescheduling avoids partial cancellation if the destination cannot be
booked. The cost is more preview requests and related tokens/set validation;
a server-side preview reservation/state store is unnecessary for this scope.

Consequences:
Record exactly the presented groups/mechanics in SYSTEM_ARCHITECTURE_SPEC
and affected memory. Unpresented endpoint/DTO/status/guard-key/digest/UDF
signatures remain for consolidated E04 review, not blanket acceptance.
Next propose Admissions/Finance3c approval/Invoice/collection/correction
contracts, then complete remaining Academics/reports/frontend/SQL/runtime/
verification. E04/E05 unchecked, D12/D13 deferred, old ADRs unchanged. No code,
SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-144: Accept E04 package3c's Admissions/Finance API groups and atomic financial mechanics

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói3c after ADR-143. Admission,
Invoice snapshots, external collection, metadata corrections, role scopes and
Student printing retain the already accepted E01/E02/E03 business rules.
Approval covers the presented groups/mechanics, not every detailed route,
DTO, SQL signature or the full E04 artifact.

Decision:
Provide Admissions search/detail with role-limited data; separate approval
preview/approve, rejection and cancellation-preview/cancel commands.
Approval commits Registration ADMITTED and Invoice UNPAID together; server
owns Invoice number, amount and dates. Rejection/cancellation require the
existing reasons; cancellation effects commit together under accepted guards.
Student reads only own Invoice list/detail and fetches fresh authorized
data before browser printing; no dedicated PDF API.
Accountant has Invoice search/detail, collection-preview and collect for
money reconciled outside the application. Payment and Invoice PAID commit
together. Metadata-only correction has reason and old/new history, without
changing amount/time or creating a second Payment.
These five mutation commands use Idempotency-Key and related-record versions.
Business effects, mandatory audit and completion results share one transaction.
Unknown responses use caller-scoped result lookup; absence of a result is
not proof of failure. Financial detail/compound preview reads use one consistent
snapshot. Role-specific responses do not give Admissions/Student Accountant
Payment information. Reports/CSV remain a separate later package.

Reason and alternatives:
Explicit commands and role-specific DTOs protect workflow boundaries and
visibility better than generic CRUD/shared entity responses. One approval
transaction avoids an admitted record without an Invoice; one collection
transaction avoids divergent Invoice/Payment state. Fresh own-Invoice fetch
avoids printing stale data as current. Costs are additional preview requests,
DTOs and consistency tests; financial snapshot/version-store overhead needs
environment validation, not an assumed performance result.

Consequences:
Mark only presented Gói3c groups/mechanics accepted in the E04 draft and
affected memory. Exact endpoint/DTO/status/guard-key/digest/SQL/settings
refinements not separately presented remain for consolidated artifact review.
Next propose Gói4a EntryTestSession/grader/roster and Teacher score/absence/
correction contracts, then remaining Academics/reports/frontend/integration/
runtime/verification. E04/E05 unchecked, D12/D13 deferred, old ADRs unchanged.
No code, SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-145: Accept E04 package4a's entry-session management and Teacher grading API groups

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted the presented Gói4a after ADR-144, then requested
continuation after an interrupted documentation turn. Existing W06/W08 rights,
time cutoffs, attempts, score/Band and history rules remain accepted inputs.
No ADR was written by the interrupted turn. This records only the presented
groups/mechanics, not all detailed routes or the entire E04 artifact.

Decision:
AcademicAffairs receives session list/detail/create, permitted schedule edits,
separate open/close commands, cancellation-preview/cancel, and grader options/
change preview/reasoned change. Session cancellation commits the session,
affected bookings and required history together; no automatic rebooking.
Roster DTOs differ for AcademicAffairs and Teacher. Teacher can read only
currently assigned sessions/tasks, with no financial or out-of-task profile data.
Initial scoring and absence are separate per-student commands. Scoring updates
the result and Band in one transaction; absence is not score zero.
Score correction is a separate reasoned command, retaining original/current
grader constraints and atomically updating best result/Band and old-new history.
Nine mutation commands use Idempotency-Key and related versions/record sets:
create session, edit schedule, open, close, cancel, change grader, initial
score, absence and score correction. Effects, mandatory audit and completion
share a transaction. Server rechecks rights/time/capacity/conflicts; previews
do not reserve. Unknown responses use caller-scoped result lookup, with no
automatic rescoring/absence. Use adopted roster/effective-state/Band SQL
objects through real application paths; services keep authorization/workflows.

Reason and alternatives:
Task-specific DTOs and explicit commands make session management, grading and
absence boundaries clearer than generic status/score CRUD or one shared staff
roster. One-person scoring avoids accidental batch grading/absence; atomic
session cancellation avoids missing dependent bookings. Costs are additional
DTOs, preview requests, related-context checks and tests.

Consequences:
Mark presented Gói4a groups/mechanics accepted in the E04 draft and affected
memory. Exact endpoint/status/DTO/guard-key/SQL signatures, read-isolation and
runtime refinements not separately presented still require consolidated review.
Next propose Gói4b Course/TrainingProgram/components/Band-read API, then Class/
placement/Schedule/Attendance/grades/reports/frontend/integration/runtime/
verification. E04/E05 unchecked; D12/D13 deferred; old ADRs unchanged. No code,
SQL/migration, provisioning, packages or executed tests are authorized.

## ADR-146: Accept E04 package4b's catalogue/configuration API groups and whole-configuration saves

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted presented Gói4b after ADR-145. Course/program
lifecycle, first-use freezes, money, components and seed Band thresholds remain
accepted E01/E02/E03 inputs. This approval is of presented API groups/mechanics,
not all exact DTOs/routes/SQL signatures or complete E04 acceptance.

Decision:
Provide Course list/detail/create and a name-price save that cannot edit code
or status. Deactivation/restoration are separate commands with explicit effects,
without cascading cancellation of existing records/classes.
Create TrainingProgram DRAFT and save its configuration and entire component
set in one transaction. Use activation-preview of saved configuration before
the separate activation command; return-to-draft/deactivation follow existing
guards. Provide a separate Program name command that remains usable when
configuration is frozen, without opening protected fields.
Band API reads the accepted threshold set and derived freeze state from DB;
no new seed editor or unlock command.
Ten write commands use Idempotency-Key; effects, mandatory audit and completion
share a transaction. Configuration forms carry Program versions and the full
component set. One invalid/stale component aborts the entire save unit.
Server rechecks guards; conflicts require deliberate reload, not overwrite.
Unknown results use caller-scoped lookup, not automatic reactivation.

Reason and alternatives:
Whole-configuration saves fit the complex form and avoid partial program
updates compared with separate component CRUD calls. Explicit lifecycle/name
commands preserve freeze boundaries while retaining allowed metadata edits.
Costs are larger configuration payloads, related-set validation and tests.

Consequences:
Mark presented Gói4b accepted in the draft and affected memory; unpresented
endpoint/DTO/status/guard-key/SQL/runtime refinements remain for consolidated
review. Next propose Gói4c Class management/placement/scoped reads, then
Schedule/Attendance/grades/reports/frontend/integration/runtime/verification.
E04/E05 unchecked, D12/D13 deferred and old ADRs unchanged. No code, SQL/
migration, provisioning, packages or executed tests are authorized.

## ADR-147: Accept E04 package4c's Class/placement/scoped-read groups and explicit commands

Status: Accepted — 2026-10-01

Context:
Owner accepted the presented Gói4c following ADR-146. E01/E02/E03 Class,
placement, timetable and lifecycle guards remain accepted inputs. This records
the presented API groups/mechanics, not every detailed route/DTO/guard key or
whole E04 acceptance.

Decision:
Provide Class list/detail/create PLANNED and capacity saves within accepted
windows, without generic code/program/state edits. Teacher options and change
preview precede an explicit change command; recheck current Teacher validity,
every affected schedule and existing no-teaching/no-dependent-data guards.
Open/start/complete/cancel are four explicit commands with prerequisite/effect
previews. Cancellation requires its accepted reason; completion never submits
or finalizes grades automatically.
Provide eligible Registration search and suitable Class options. Empty options
mean waiting for a class, not automatic creation, placement or Band changes.
Preview and place one Registration at a time. Assignment, initial DRAFT
GradeReport, mandatory audit and completion commit together; no default Score
or Attendance rows and no leakage of the internal DRAFT.
AcademicAffairs management reads, Teacher currently assigned Class reads and
Student own-Class reads use separate scopes; Student receives no peer roster.
Eight write commands use Idempotency-Key and related versions/record sets:
create Class, capacity, change Teacher, open, start, complete, cancel and place.
Server rechecks live state/capacity/payment/conflicts; previews reserve nothing.
Stale conflicts require deliberate reload. Unknown results use caller-scoped
lookup, not automatic repeated placement. No transfer/removal/reopen/refund.

Reason and alternatives:
Explicit lifecycle commands and single-Registration placement keep effects and
scope visible, compared with generic Class CRUD or automatic bulk placement.
Atomic placement prevents assignments without initial reports. Costs are more
preview requests, related-context checks and role-specific projections.

Consequences:
Mark presented Gói4c accepted in the draft and affected memory. Unpresented
endpoint/DTO/status/guard-key/SQL/read-isolation refinements still require
consolidated review. Next propose Gói4d Schedule/weekly generation/move/
replacement/HELD/scoped calendar reads, then Attendance/grades/reports/
frontend/integration/runtime/verification. E04/E05 unchecked, D12/D13 deferred
and old ADRs unchanged. No code, SQL/migration, packages, provisioning or
executed tests are authorized.

## ADR-148: Accept E04 package4d's Schedule/calendar groups and explicit occurrence commands

Status: Accepted — 2026-10-01

Context:
Owner accepted the presented Gói4d following ADR-147. Existing E01/E02/E03
scheduling windows, obligations, replacement history and HELD rules remain
accepted inputs. Approval covers presented groups/mechanics, not every detailed
route/DTO/guard key/SQL refinement or complete E04 acceptance.

Decision:
Provide AcademicAffairs management calendars, Teacher calendars for currently
assigned Classes and Student own-Class calendars, with current/history separated.
Individual LESSON and FINAL_EXAM creation use concrete date/time/room and
conflict previews; an exam is separate, never automatically generated with lessons.
Weekly creation previews every concrete LESSON and saves the whole valid batch
or none. Do not silently skip errors/holidays or preserve a recurring rule/job.
Pre-start date/time/room moves use previews and retain the same occurrence,
only inside accepted windows and without dependent data.
Non-held replacement requires a reason, preserves the old occurrence/history
and creates a valid future replacement in one transaction. Both belong to the
same obligation; replacement never increases the LESSON requirement/count.
AcademicAffairs confirms HELD one occurrence at a time after scheduled end
while Class ONGOING. Do not infer occurrence from clock/Attendance, undo HELD
or complete Class automatically.
Five write commands use Idempotency-Key and related versions: create-one,
create-weekly, move, replace and confirm-held. Server rechecks at commit;
preview reserves nothing. Conflict requires re-preview; unknown responses
require caller-scoped result lookup before a deliberate retry. No partial saves,
automatic retries or side effects beyond the chosen command.

Reason and alternatives:
Concrete previews and explicit commands expose effects and preserve history,
compared with generic Schedule CRUD/state setters or automatic partial saving.
An atomic batch/replacement avoids mismatches between the displayed plan and
the saved timetable. Costs are additional preview calls and related-context
validation; background recurrence/holiday handling is outside accepted scope.

Consequences:
Mark presented Gói4d accepted in the draft and affected memory. Unpresented
endpoint/DTO/status/guard-manifest/SQL/read-isolation/runtime refinements still
require consolidated review. Next propose Gói4e Attendance and per-Student
draft Score/comment saves, then whole-Class submit/review/publication, reports,
frontend/integration/runtime/verification. E04/E05 unchecked, D12/D13 deferred
and old ADRs unchanged. No code, SQL/migration, packages, provisioning or
executed tests are authorized.

## ADR-149: Accept E04 package4e's scoped Attendance and per-Student draft-save groups

Status: Accepted — 2026-10-01

Context:
Owner accepted the presented Gói4e after ADR-148 and asked for the remaining
packages in a Markdown review file editable like OWNER_REVIEW. Existing entry
windows, missing-value semantics and save granularity remain accepted inputs.
Approval covers the presented groups/mechanics, not all detailed routes/DTO/
guard-key/SQL refinements or complete E04 acceptance.

Decision:
Teacher reads Attendance only for currently assigned Classes; AcademicAffairs
reads it for management/completeness without entering it on Teacher's behalf.
Save only deliberately chosen/changed Attendance values of one LESSON in an
all-or-none batch. One invalid value aborts that save; unrecorded Students stay
unrecorded, not present/absent by default.
Teacher reads the assigned Class gradebook, configured components/weights,
scores/comments and applicable return reason. No new AcademicAffairs or
Student visibility into Teacher DRAFT values.
Save changed component scores and comment of one Student together. Desktop
uses per-row save, mobile the same unit in a Student page. Missing scores show
incomplete; blank is not zero or deletion of a stored score. Server calculates
totals; draft saves do not submit or publish grades.
Two write commands use Idempotency-Key and related versions, including the
parent GradeReport version when scores/comments change. Server rechecks current
rights, schedule and state at commit. Retain accepted windows, no autosave,
default-all or silent overwrite. Unknown results use caller-scoped lookup
before a deliberate retry. Whole-Class sending/review remains a separate package.

Reason and alternatives:
Explicit one-LESSON and one-Student saves expose the saved unit and its status,
compared with per-cell autosave or full-Class draft replacement. All-or-none
saves avoid partial scores/comment updates; missing data stays distinguishable
from deliberate absence/zero. Costs are explicit save clicks, delta/context
tracking and unknown-result handling.

Consequences:
Mark presented Gói4e accepted and synchronize affected memory. Unpresented
endpoint/DTO/status/guard-manifest/SQL/integration refinements still require
consolidated review. At the owner's request, create E04_REMAINING_REVIEW.md for
remaining package proposals and owner marks; OWNER_REVIEW retains stage-level
E04/E05 acceptance, and no package tick accepts unseen future artifacts.
E04/E05 unchecked, D12/D13 deferred and old ADRs unchanged. No code, SQL/
migration, packages, provisioning, tests or external integration are authorized.

## ADR-150: Accept E04 package4f's whole-Class grade submission, review and Student publication

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package4f (4F01–4F04) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 4F01 — Teacher gửi/gửi lại: Read-only preview toàn Class trả mã lớp/cohort/state/missing components/versions; hai command riêng Gửi và Gửi lại, không Student selection. Frontend xử lý mọi local unsaved edit trước preview; Class COMPLETED, đủ mọi học viên/component, current Teacher, đúng report states mới ghi. Không tự gửi khi đủ điểm, không auto resubmit, không thêm lý do Teacher khi chưa có policy.
- 4F02 — Giáo vụ review: Danh sách lớp và chi tiết review đúng quyền qua SP03; không DRAFT values. Preview action RETURN hoặc FINALIZE, rồi command riêng. RETURN bắt buộc lý do/giữ scores-comment; FINALIZE chốt/công bố cả cohort cùng lúc. Một confirmation có Class/count/state/effects; không checkbox chọn vài học viên/duyệt hai tầng/reopen/override.
- 4F03 — Student xem kết quả: Hai reads own FINALIZED list/detail, chọn đúng lượt học khi học nhiều khóa/học lại. Server trả component/weights/scores/comment/final_score/pass đúng scope; không classmates, DRAFT/SUBMITTED/RETURNED values hoặc export/PDF mới. Trước công bố chỉ hiển thị thông báo chưa có kết quả được công bố khi cần, không gọi total nháp là điểm chính thức.
- 4F04 — Cohort/TX/replay: Cả4 commands submit/resubmit/return/finalize dùng UUID Idempotency-Key và full Class–Assignment–GradeReport context, không page subset. TX12 all-or-none statuses/history/audit/completion; finalize tính decimal/half-up một lần, lưu final_score và cập nhật Registration.is_unfinished cùng TX. Giữ mỗi lần workflow theo operation_id; 409 rà lại, unknown tra caller result, no automatic effects.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section4 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section14.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
Separate whole-Class previews and explicit commands preserve cohort atomicity and published-grade boundaries. Compared with per-Student submission or automatic publication, this needs full-set/version validation and an additional preview but avoids partial lifecycle effects.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-151: Accept E04 package5's role-scoped reports and fresh all-results financial CSV

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package5 (501–503) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 501 — Báo cáo tài chính: API role-specific Accountant/Director cùng metric đã chốt: revenue theo paid_at, billed theo issued_at của non-CANCELLED Invoice, current UNPAID debt/overdue riêng không period. Response applied filters/queried_at/whole-set totals/group page; exact money strings. Accountant vẫn dùng Invoice nghiệp vụ qua Gói3c, report không biến thành profile/Invoice dump; Director chỉ nhóm không định danh.
- 502 — Tuyển sinh/đào tạo Director: Hai API riêng Admissions và Training. Admissions Registration count và distinct Student count theo cùng cohort; overall distinct tính riêng, không sum nhóm. Training trả inventory/current obligations tách finalized-results period; HELD LESSON/expected và FINAL_EXAM riêng, replacement không count đôi. No-data khác0%; defaults/history Course theo ADR đã chốt; không hidden Student detail/export.
- 503 — CSV Accountant: Một read-only export endpoint chạy fresh SNAPSHOT theo applied financial filters/cột report/toàn kết quả, không UUID mutation marker/job/export store. Kiểm quyền mới, max10.000 dòng hoặc5MB; vượt reject toàn file, không stream phần hợp lệ trước rồi báo thành công. UTF8BOM/comma/CRLF/quote/formula safeguards đúng ADR-101; không DB text rewrite, XLSX/PDF hoặc Director export.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section5 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section15.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
Separate role projections and metric-specific periods preserve the accepted aggregate and current-state semantics. A fresh whole-result export avoids stale/page-only downloads; snapshot queries and size preflight cost additional server work. Individual Director data, historical as-of reporting and export jobs are not selected.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-152: Accept E04 package6's frontend route, state, form and authentication boundaries

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package6 (601–604) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 601 — Route/feature contract: Một route manifest cho F01–F33 trừ F15, paths theo feature/role, list-detail-complex-form URLs hỗ trợ direct link/Back/Forward. Auth context tối thiểu dùng chung shell; menu guards chỉ UX, API vẫn auth. Ghi route→F→role→API→loading/error/empty contract; không staff dashboard mới hoặc thư viện router/cache/form tự thêm.
- 602 — Request/server-state: Một typed API client chung cho cookie/CSRF/envelope/traceId; trạng thái idle/loading/success/empty/error/unknown tách rõ. Key dữ liệu gồm caller scope/filter/page; discard response cũ khi request mới hoặc phiên đổi. Sau write cập nhật/refetch projections liên quan có quyền, không optimistic update trạng thái quan trọng, background polling giữ phiên hay automatic mutation retry. RAM không là durable storage.
- 603 — Form/preview/intent: Form giữ draft trong RAM, explicit save; expectedVersions/context cùng dữ liệu đã load. Sửa input invalidate preview. Disable double-click khi pending, UUID một intent; unknown giữ intent/content để tra kết quả, sửa payload là intent mới sau xử lý kết quả cũ. 400 chỉ lỗi trường,409 deliberate reload/review, no merge/overwrite. Unsaved edits warn khi rời; blank không tự0/delete/absent.
- 604 — Phiên/CSRF/secret UX: 401 xóa auth/sensitive RAM/preview/secret và về login;403 phân biệt CSRF_INVALID với role denial, lấy CSRF/context mới theo Auth contract nhưng không tự phát lại write. Logout/session changes giữa tab chỉ signal không secret, dữ liệu scope cũ bị vô hiệu; quyền cuối cùng server kiểm. Temporary password chỉ first response/one-time RAM/explicit copy/clear khi rời đúng FE18; không replay/login credential cache.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section6 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section16.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
A shared typed client and explicit draft/preview/command states keep stale-session and unknown-result handling consistent. Compared with optimistic critical writes, autosave, persistent sensitive caches or automatic write retries, this requires more state transitions and deliberate user actions without changing accepted UX scope.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-153: Accept E04 package7's backend/SQL, canonical replay, audit and least-privilege integration

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package7 (701–705) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 701 — DTO/contract manifest: Hoàn thiện từng endpoint: role/ownership, request-response/error fields, source row/aggregate tokens, full-set/expected-absence semantics, UUID bắt buộc/không dùng, audit và TX. Base64/version không cấp quyền; field chỉ xuất theo role, stable page/sort. Ghi rõ các chi tiết Gói1–4e chưa trình, không suy toàn draft đã duyệt.
- 702 — Canonical replay digest: Giữ input_digest binary(32)/SHA-256 đã duyệt ở E03 T23, không đổi hash hoặc thêm trường DB. Đề xuất canonical serialization theo DTO đã validate/normalize/allowlist, bao gồm target/expected versions/set/intent content; actor/action từ verified session. Order cố định; set chỉ sort khi semantics không phụ thuộc thứ tự. Không hash raw HTTP JSON hoặc chứa CSRF/JWT/password/temp secret. Marker vẫn completion/ref/mode đúng T23, không response PII snapshot. Exact UTF8 byte representation/contract compatibility trong cửa sổ replay phải trình ở E04.
- 703 — SQL callers/query mapping: Repository gọi thật SP01–06 đúng paths Admissions/roster/review/reports; SP07 allocator trong approval TX. FN01–06 có callers/parity đã định, không tồn tại chỉ để đủ rubric. Keyless read DTO mappings/parameterization/sort allowlist; actor_id từ verified session, không client tự khai. Trigger tables dùng EF write mapping tương thích; phải có integration evidence sau triển khai, không giả SaveChanges mặc định đã đủ.
- 704 — TX/locks/read consistency: Command matrix nối mỗi write với TX01–13 và request/aggregate/TIMETABLE gates theo E03§9.1, shared connection/TX cho effects/mandatory audit/replay. Reload/recheck rights/session/time/relationships sau locks; resource set đổi thì rollback/reprepare theo cùng order. Child insert/absence/full cohort không giả Class RV tự thay. Report SNAPSHOT đã chốt; compound preview/detail cần coherent read snapshot ngắn, không giữ qua user confirmation.
- 705 — Audit/DB privilege contract: Matrix event/allowlisted old-new/reason/operation_id; mandatory fail rollback, pure-security best-effort và retention/IP90 ngày đúng policy. Runtime chỉ grants cần cho real paths, schema/operator riêng;4 SQL demo roles không là7 web roles và không cấp db_owner/sysadmin. Admin audit reads không mở nghiệp vụ khác. Unknown/replay kiểm current rights; không log secret/full payload hoặc app purge/export audit.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section7 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section17.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
Explicit DTO/command/caller matrices expose transaction ownership and real rubric integration. Canonical normalized intent is preferable to raw JSON hashing; fixed semantics require compatibility fixtures. Shared caller transactions and scoped grants avoid independently committed effects and broad DB privileges, at the cost of mapping and integration verification.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-154: Accept E04 package8's laptop operation and secure configuration design boundaries

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package8 (801–804) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 801 — Mô hình cùng origin: Soạn hồ sơ dev/demo khác nhau: dev vẫn frontend/API cùng origin qua cấu hình phù hợp; demo đề xuất ASP.NET phục vụ SPA build và /api/v1, frontend gọi đường dẫn tương đối. Chỉ web được đưa qua cổng truy cập đã được phép, SQL không public. LAN/tunnel là profile thiết kế chờ D13, không tự bật. Link thay đổi thì dùng phiên/login mới, không tắt Secure/CSRF để chữa lỗi.
- 802 — Config/proxy/timeout/health: Config typed và validate khi startup: timezone VN, trusted origin/proxy, JWT/CSRF/DB/crypto keys ngoài repo, exact target runtime theo ADR-119. Fail closed thiếu config; readiness không leak DB/secrets hoặc cập nhật idle. Khóa/query/request có deadline hữu hạn, cancel/timeout→unknown/lookup khi cần; không giữ transaction chờ user. Số timeout/ports/proxy addresses và benchmark cần hồ sơ môi trường trước khi chốt, không tự điền ngưỡng/SLO D12.
- 803 — Backup/recovery hồ sơ: Giữ daily encrypted SQL backup/7 ngày4 tuần/monthly restore/RPO24h-RTO2h là targets đã chốt. Soạn nơi lưu riêng khỏi ổ primary, quyền operator/key tách backup và checklist restore DB test độc lập. Laptop tắt: ghi nhận missed run, kiểm/catch-up khi bật; không gọi backup cũ là fresh hoặc bảo đảm RPO khi đã lỡ. Đường dẫn/dung lượng/media/access chưa có phải owner xác định trước chạy thật; không backup/restore ở lượt thiết kế.
- 804 — Runbook demo/bảo trì: Một checklist trước-trong-sau buổi: kiểm bản build/data giả/config/DB/web/readiness/login7roles trên thiết bị khách; chỉ bật tunnel sau được phép; mất mạng/write unknown tra kết quả; tắt đường public sau demo, theo dõi backup/session/secret. Bạn vận hành theo ADR-120; không role Admin/DBA mới. Dependency/security/blocklist review theo chính sách; rollout/rollback chi tiết chỉ lập sau full baseline và yêu cầu triển khai.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section8 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section18.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
Same-origin demo design and operator runbooks fit on-demand laptop use. They require valid configuration, separate recovery media and honest missed-backup handling. Always-on/cloud hosting, a concrete tunnel/provider/domain/port/proxy setup and numerical performance guarantees are not selected by these rows; D12/D13 stay deferred.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-155: Accept E04 package9's traceability, risk-based verification and separate E04/E05 acceptance

Status: Accepted — 2026-10-01

Context:
Owner explicitly accepted all23 displayed rows of E04_REMAINING_REVIEW E04-R1
in chat and saved all23 checked, with no alternative notes. This ADR records
package9 (901–903) as displayed in that file, including its explanatory
contracts and limits; E01/E02/E03 and older accepted decisions stay unchanged.

Decision:
- 901 — Trace hoàn chỉnh: Một matrix37 NV/32 F/21 W→actor/endpoint/DTO→Service/TX→24 tables/SQL objects→UI/errors→verification case. Chỉ rõ removed NV10/11/F15, deferred/out-of-scope và E04 refinements chưa rà. Mỗi source obligation có path thật dự kiến, không ép một SP/NV hoặc đếm file thay độ phủ. E05 sẽ rà matrix thực tế; hiện chưa tạo complete trace mới.
- 902 — Bộ ca kiểm chứng: Thiết kế xUnit nghiệp vụ/API, SQL Server test riêng/race hai connection/forced failure/audit-replay/trigger-EF/grants-SP-UDF/index before-after theo rubric. All7 roles/ownership/list-detail-preview-replay-export, clocks/decimals/missing/whole-Class/cohort/no partial/secret retention; browser375/768/1280 và Chrome/Edge khi triển khai. Data giả tách DB chính; không framework mới/code tests hoặc claim pass.
- 903 — Quy trình đóng draft: Sau ghi nhận các dòng được duyệt, tổng hợp E04 đủ endpoint/DTO/mapping/frontend/runtime/verification, đối chiếu những chi tiết cũ chưa trình. Mọi gap/mâu thuẫn liệt kê để xử lý hoặc owner loại/hoãn rõ; trình bản E04 thực tế để nghiệm thu riêng trong OWNER_REVIEW. Sau E04 mới rà toàn baseline/E05 thực tế; không tick E04/E05 từ23 dòng này và không nhảy sang implementation plan.

The accepted source contract and illustrations are retained in
[the owner review](plan/E04_REMAINING_REVIEW.md) section9 and consolidated
into [SYSTEM_ARCHITECTURE_SPEC](design/SYSTEM_ARCHITECTURE_SPEC.md) section19.
Exact DTO/guard/signature/read-isolation/runtime refinements not already displayed
must still be written and reviewed in the actual consolidated E04 artifact.

Reason and alternatives:
One end-to-end trace and risk-based cases provide reviewable coverage rather than file counts or happy-path-only checks. Completing the actual artifact before its separate acceptance prevents package marks from approving unseen refinements. Verification breadth costs later test effort; there is no running application or execution evidence now.

Consequences:
Record these presented rows as accepted and synchronize affected Project Memory.
This accepts the displayed design/process only, not an unseen complete artifact
or successful runtime/SQL/browser/load/recovery tests. OWNER_REVIEW retains
separate unchecked E04/E05 gates; D12/D13 remain deferred with original notes.
No code, SQL/migration, Git/scaffold, packages, provisioning, backup/restore,
public exposure or external integration is authorized.

## ADR-156: Require future start for new Schedule roots and move destinations (E04-RF03)

Status: Accepted — 2026-10-03

Context:
E01 NV19/E02 W16 and ADR-032/036/148 already define the old-slot pre-start
move window, future linked replacements and after-end HELD. Root creation/
weekly creation and the destination of a pre-start move had no explicit
future cutoff. E04-D7 presented E04-RF03 separately as a proposal; the owner
replied with agreement after the RF03 summary on2026-10-03.

Decision:
- A new root Schedule (LESSON or FINAL_EXAM), every concrete LESSON in a
  weekly batch, and a move destination must have startAt>serverNow when the
  service validates the saving transaction. Equality or a passed start after
  valid-shaped input is409; malformed date/time remains400.
- The service checks the authoritative clock again at saving, independent
  of RowVersion and preview time. A preview does not reserve time/resources
  or grant an exception; the client cannot supply an authoritative asOfUtc.
- If any weekly candidate fails this cutoff, save none: all-or-none,
  rollback/no skip/trim/automatic date movement. The owner edits the input
  and previews again; no backfill workflow for old lessons/Attendance/Score/
  HELD is added.
- Keep existing old-slot move, replacement-future/old-start and HELD-after-end/
  ONGOING windows, dependencies, obligation count, conflicts and role rights.
  This also applies when moving a current replacement occurrence; origin
  history does not exempt its new destination from the future cutoff.

Reason and alternatives:
A future-only scheduling flow keeps intended scheduling separate from entering
past events, with one server-clock boundary for individual/weekly creation
and moves. Allowing past roots/destinations would require a separately designed
backfill/provenance workflow; that option was not selected. The tradeoff is
that a valid preview can become blocked while the owner is preparing to save.

Consequences:
Synchronize requirements and NV19/W16/Schedule API notes as a dated addendum,
without changing earlier accepted ADRs or E03 physical schema/SQL objects.
Only RF03 is resolved by this reply: RF01 (EntryTestSession creation) and RF02
(entry grader/score no-op outcome) remain proposed. Other D7 wire schemas and
the whole E04/E05 artifacts are not accepted by inference. No implementation,
SQL, provisioning or runtime-test evidence is authorized or claimed.

Related:
ADR-032/036/088/148; [RF03 resolution](plan/questions.md).

## ADR-157: Require future start when creating EntryTestSession (E04-RF01)

Status: Accepted — 2026-10-03

Context:
Entry-session opening/edit/cancellation/grader windows were already accepted,
but initial creation had no explicit clock cutoff. The owner approved the
three-item RF01/RF02/RF04 review on 2026-10-03. This records RF01 only.

Decision:
- AcademicAffairs creates EntryTestSession only when startAt>serverNow during
  saving-transaction validation, independent of preview time and rowversions.
  Equality or a passed start with otherwise valid input returns409; malformed
  dates/times remain400. No past-session backfill path is added.
- A new session remains CLOSED; same-day/minute/capacity/ACTIVE Teacher and
  room/Teacher conflict guards, existing booking/grading windows and roles stay.
  Failure leaves no session/audit/completion partial effects.

Reason and alternatives:
A future-only creation flow keeps scheduling separate from entering past
activity. Allowing backfill would require a separately defined provenance and
workflow; that option was not selected. A valid form can become blocked by
elapsed time before saving and must be revised deliberately.

Consequences:
Synchronize canonical requirements and dated NV14/W06/API-055 addenda.
RF03 remains ADR-156; no E03 physical/schema/SQL changes. This does not accept
all E04 refinements, E04/E05, D12/D13 or authorize implementation/planning.

Related:
ADR-030/145/156; [RF01 resolution](plan/questions.md).

## ADR-158: Reject unchanged entry grader and corrected score intents (E04-RF02)

Status: Accepted — 2026-10-03

Context:
Reasoned grader replacement and original-grader score correction were already
accepted, while their equal-value command outcome was unresolved. The owner
approved RF02 with RF01/RF04 on 2026-10-03.

Decision:
- For a new intent, after identity/task authorization and valid current-context
  checks, selecting the existing grader or correcting to the same parsed score
  returns400 with the relevant graderId/score field error.
- No business mutation, changed provenance, mandatory mutation audit or new
  ReplayOperation completion is created for that unchanged request. A reason
  alone does not make a score/grader change.
- Stale versions/membership/state/time context still returns409; equality does
  not suppress an invalid context. Current/original-grader and correction reason
  requirements, pre-confirmation/ended-record restrictions remain unchanged.
- An already completed same-key intent follows authorized replay before new
  mutation guards, with no repeated effects. It is not a new no-change request.

Reason and alternatives:
Rejecting an unchanged intent avoids fictitious correction/reassignment history.
A200 no-change success would require distinct result semantics and could imply
an action occurred; that alternative was not selected. UI separates no-change
input errors from stale-context conflicts and actual success.

Consequences:
Synchronize requirements, NV14/NV22/W06/W08 and API-064/072/no-op registry.
Exact wire code strings/envelopes, preview shapes and canonical digest codec
remain E04 refinements for artifact review. No privileged correction override,
T23 fields/modes, implementation or E04/E05 acceptance follows this approval.

Related:
ADR-029/030/145; [RF02 resolution](plan/questions.md).

## ADR-159: Reject unchanged value edits and preserve delta provenance (E04-RF04)

Status: Accepted — 2026-10-03

Context:
SC02/SC01 review grouped unresolved no-change outcomes for other value edits
and Attendance/grade-draft delta saves. The owner approved RF04 alongside
RF01/RF02; save units and current authorization/state/history remain inputs.

Decision:
- Other value edits that leave all business values unchanged after field
  normalization and valid current-context checks return400, without mutation,
  provenance touch, mandatory mutation audit or new completion marker.
  This covers Booking reschedule, Payment metadata, EntrySession schedule,
  Course details, Program configuration/name, Class capacity/Teacher and
  Schedule move; reason-only edits are not business changes.
- For Attendance and per-Student grade-draft delta batches, validate the entire
  supplied request, including every item/version/expected absence/full cohort/
  relationship/state/clock guard, before evaluating equality. In a mixed batch,
  save actual changes atomically in the existing save unit; unchanged rows keep
  enteredBy/enteredAt and existing history. Any invalid/stale item aborts the
  whole save even when its value appears equal. An entirely equal batch returns
  400 with no new marker. A missing row newly recorded as0 is an actual change.
- Omitted teacherComment preserves the current value; explicit NULL/normalized
  blank clears it. Clearing an alreadyNULL value is unchanged. Real score/comment
  mutations still touch the parent report version within TX11; unchanged values
  alone do not touch it or create correction history.
- A new-key state-transition command must satisfy its source state; wrong-source
  requests return409 rather than claim another intent's completion. Creates do
  not treat existing domain rows as successful creation. A deliberate Admin
  reset still issues a new temporary credential and revokes sessions under
  existing rules; completed same-key replay never resets/issues a secret again.
- Required audit and full rollback semantics for actual changes remain intact;
  these rules do not allow partial validation, silent merge, delete or bypass.

Reason and alternatives:
Rejecting all-equal saves and retaining unchanged-row provenance keeps history
accurate. A200 no-change completion alternative was not selected; it would
need new success/replay semantics. Mixed batches remain useful without a fake
write of every supplied row; the cost is explicit UI no-change handling.

Consequences:
Synchronize canonical requirements, affected NV/W contracts, DTOs and SC02.
The no-change400/wrong-source409 behavior is accepted; exact NO_CHANGE/
STATE_CONFLICT spellings and remaining response/codec/SP/EF details still
belong to actual E04 review. No T23 fields/result modes, web rights, schema,
code/SQL/deployment/planning or whole E04/E05 acceptance is authorized.

Related:
ADR-042/093/139/149/157/158; [RF04 resolution](plan/questions.md).
## ADR-160: Accept actual consolidated E04-v1 architecture and technical contracts

Status: Accepted — 2026-10-03

Context:
The owner was presented the actual E04-v1 package/final contract and E05-v1
cross-check, with explicit E04-before-E05 acceptance and preserved D12/D13
DF01–05 boundaries. The owner confirmed both outputs in that order. This
records actual E04 acceptance, not an inference from earlier package/RF votes.

Decision:
Accept SYSTEM_ARCHITECTURE_SPEC E04-v1 and its actual catalog/154-domain-DTO/
SC01-parity/SC02-replay/SC03-SQL-integration/SC04-runtime/FINAL_CONTRACT package.
This includes154 method/path/55 UUID commands/6 caller-result paths/32F/21W,
strict UUID/8byte RowVersion/scalars/C1 normalized full-intent bytes/SHA256,
error precedence/current read authorization and completion unions, exact24h
replay retention/expired retained-key409/compatibility,7SP/6FN result mapping,
5trigger EF write strategy/24-table grants/locks/audit classifications,
frontend states and fresh financial CSV6columns/footer/10000datarecords
including footer/5000000encodedbytes. Runtime typed slots/fail-closed/readiness/
cancellation/operator/recovery prerequisites and verification obligations
are accepted design. Preserve all prior policy/source-state/save-unit/role/
privacy invariants and physical E03 schema, including RF156–159.

Reason:
The complete contracts and parity findings are now reviewable actual artifacts,
with source obligations/scoped projections/transaction/replay/SQL/UI boundaries
and explicit environment/evidence deferrals. This closes design gaps without
pretending there is an implementation or successful runtime verification.

Consequences:
Mark only OWNER_REVIEW E04 accepted first and stage4 done. D12/D13 remain
deferred with original notes; DF01–05 remain environment/evidence prerequisites,
not configured provider/port/deadline/storage/package values or waived invariants.
Earlier proposed/draft labels in dated checkpoints describe pre-acceptance
status; exact presented contracts are accepted through this actual artifact.
No app code/SQL/migration/Git/scaffold/packages/deployment/exposure/backup-
restore/integration or implementation planning is authorized by acceptance.

Related:
[Actual E04-v1](design/SYSTEM_ARCHITECTURE_SPEC.md),
[final contract and DF01–05](design/E04_FINAL_CONTRACT.md),
[owner gate](plan/OWNER_REVIEW.md); ADR-139–159.

## ADR-161: Accept E05-v1 consistency review and complete design baseline

Status: Accepted — 2026-10-03

Context:
After the explicit E04-first proposal, the owner confirmed E04-v1 then the
actual E05-v1 cross-check. E04 is accepted above without artifact amendments;
the prepared E05 therefore remains the exact baseline that was reviewed.

Decision:
Accept E05_BASELINE_REVIEW E05-v1 and complete E01–E04 design baseline:
37retained NV individually traced through32F/21W/API-DTO/service/TX/data/UI/
verification obligations, state/data/API/UI parity,24cross-contract cases,
10resolved-document findings, excluded scope, DB rubric/evidence boundaries
and DF01–05. NV10/NV11/F15 remain removed, RF01–04 settled, prior ADR statuses
remain unchanged. No additional unresolved in-scope product decision was
identified in the review. D12/D13 remain expressly owner-deferred.

Reason:
The owner accepted concrete outputs and their limitations after source/contract
cross-check, rather than accepting unseen future artifacts or count-based
implementation claims. Future changes must update the affected baseline trace.

Consequences:
Mark OWNER_REVIEW E05 accepted after E04, stage5 done and design-baseline done.
75owner rows now73accepted/2deferred (D12/D13), preserving all owner notes.
This is design agreement, not evidence of implemented APIs/SQL/EF/grants/CSV/
browser/race/load/recovery. Await separate owner direction before discussing
implementation planning or performing code/SQL/Git/scaffold/deployment.

Related:
[E05-v1 cross-check](design/E05_BASELINE_REVIEW.md),
[design agreement](plan/design-baseline.md), [owner gate](plan/OWNER_REVIEW.md);
ADR-011/130/155/160.

## ADR-162: Retain installed SQL Server 2025 Express after capability assessment

Status: Accepted — 2026-10-04

Context:
The owner explicitly authorized retaining the installed SQL version if suitable
and synchronizing all affected Project Memory instead of installing another
version. Read-only inspection verified SQL2025 Express64-bit17.0.1000.7 RTM,
Windows-authenticated access and a running SQLEXPRESS instance. E03/E04 need
core relational/programmability/transaction features, not Enterprise-only ones.

Decision:
Replace only ADR-119's SQL2022 Developer choice with installed SQL2025 Express
for local non-production dev/test/internal demo; keep .NET10/EF10 and remaining
stack. Keep the existing instance; no edition/major replacement prerequisite.
Use explicit compatibility160 in future isolated project/test databases and EF
provider configuration to preserve E03 SQL/types without automatic native JSON
mapping changes. This is a target, not an ALTER executed against existing DBs.

Keep ADR-118 encryption/separate media/retention/restore targets. Express lacks
SQL Agent/native backup encryption: external scheduling and encryption of backup
archives must cover protected temporary plaintext, independent keys, checked
errors and decrypt/restore evidence before operation. No backup tool/package/key/
destination is selected or installed here. Do not count plaintext .bak as policy
compliant or native encryption/Agent as available.

Observed RTM is not a current patch pin; review compatible supported17.x updates
before real data/public exposure, preserving ADR-119's patch policy. No update
performed or latest-build readiness claim. Edition suitability permits proceeding
with P01 package/local-profile work; it does not complete P01 or SQL/EF readiness.

Reason and evidence:
[SQL instance assessment](plan/SQL_INSTANCE_ASSESSMENT.md) records live checks,
Microsoft references, resource limits and unverified paths. Core features fit
accepted design; Express capacity is a constraint to measure, not proof of
10,000-student/50-concurrent performance. No business cap added.

Consequences:
Synchronize current contracts/memory and mark historical SQL choice superseded,
without rewriting source report or altering approval/notes. M0 task order and
quality/skill gates unchanged; D12/D13 owner decisions remain deferred. Other chat
starts with P01 then P02 subject to its explicit execution authorization. No app
code/DDL/DML, database creation, existing DB/config changes, installation, worker
dispatch, merge/push or deployment in this assessment.

## ADR-163: Apply authorized implementation stages automatically after review PASS

Status: Accepted — 2026-10-04

Owner explicitly requested applying P03 and continuing P05, and from now on
applying each reviewed PASS stage to main, notifying only when an issue needs
attention. Codex remains manager; bounded AGY contracts remain detached until
independent review, scope checks and required validation produce PASS.
Codex may then apply the exact reviewed snapshot and reverify main without a
new approval question. Problems keep the stage incomplete until resolved;
request owner action only when needed. This grants application authority for
stages authorized for implementation, not permission to start all M0 tasks,
change SQL/migrations, install runtimes, push or deploy. P03/P05 applied and
reverified; their task cards hold actual commit/evidence details.
## ADR-164: Explicit EF foreign-key lookup indexes during model assembly

Status: Implemented — 2026-10-05, Codex technical choice within authorized D03.

D03 composite result FK caused EF10.0.4 to create a redundant two-column index
beside the accepted unique nonnull completed_booking_id lookup. ConfigureConventions
removes ForeignKeyIndexConvention; entity configurations specify reviewed indexes
explicitly. No other conventions removed. D01/D02 physical signatures remain
unchanged and every FK lookup is independently covered across the13-table model.

Future mappings must specify required FK/query indexes and keep the full coverage
check passing. This avoids automatic unreviewed physical indexes, does not change
keys/ownership/NO ACTION deletion or grant migration/SQL execution authority.
See D03_TASK_CARD and its exact metadata/DDL tests; real SQL performance remains
an integration gate. No new business restriction or owner design decision.


## ADR-165: Preheld writer-gate checks and immutable SQL migration resources

Status: Implemented — 2026-10-05, technical choice within authorized D08/D09.

Accepted E04 requires all writers use Exclusive Transaction DbPrincipal=public
canonical PROGRAM/BANDSET/CLASS gates before DML. Five triggers verify the relevant
old/new gate set with APPLOCK_MODE and fail closed on missing/nonexclusive owner;
they do not acquire locks after DML, avoiding a reversed gate/row-lock protocol.
TG01 also checks harmless metadata mutations' existing PROGRAM gate (BANDSET is
not newly required for rename). Future first-use/result/lifecycle/service writers
must acquire the same correctly ordered gates and reload before writing; these
trigger tests do not certify arbitrary direct parent mutations or SNAPSHOT writers.

SQL object migrations embed versioned immutable resources and invoke escaped
constant EXEC batches so CREATE object is first in its dynamic batch. No user SQL
fragment enters this helper. New changes require new migration/resource versions;
no edit to applied source. Down reverses object creation. Dedicated SqlTests is
explicitly run against the owner-approved isolated DB, with fixture rollback and
finite diagnostic gate-timeout probes; it is separate from disconnected ordinary
solution tests. No runtime auto-migration/connection/grants/readiness authority.

D07/D08/D09 cards record source/scope/review/actual SQL and main verification.

## ADR-166: Caller-owned SQL read snapshots and bounded invoice allocation

Status: Implemented — 2026-10-05, technical integration within authorizedD11.

Accepted E04 service owns connection/transaction/auth/projection. SP01–06 verify
active committable caller SNAPSHOT (current-session isolation5) and never begin/
commit/rollback it. Count/page/options and parent EF reads share this transaction.
SP07 verifies caller TX, UPDLOCK/HOLDLOCK year PK row/missing range, year1..9999/
counter<=99999999, no independent commit/Invoice/audit/replay. No runtime grants.
Tagged errors51101authorization/51102scope/51103filters/51104readTX/51107allocatorTX/
51108year-or-overflow are internal SQL contracts; future services map them safely
while enforcing current web session/rights. No new web authorization established.

Reports aggregate decimal38 before exact varchar38 strings, full-set totals and
distinct cohorts independent of paging; options retain accepted history. Creation
resources immutable; SQL Server stores OR ALTER header as blanked CREATE, so tests
normalize only header/newlines and compare entire24module bodies. Source/47parameter/
scope/migration rollback/main665tests evidence in D10/D11 cards. D12principal grants
and D13readiness/full caller races remain later, not inferred from catalog counts.


## ADR-167: Object-scoped SQL roles and bounded database-principal evidence

Status: Implemented — 2026-10-05 within explicitly approved M0.D12 scope.

Four dbo-owned custom ec_* roles implement E03/E04 object rights only: runtime
table matrix plus7SP/6FN/6VW, finance VW06/SP04, academics VW03–04/SP06, director
SP04–06 only. Reporting explicit base-table SELECT DENYs; no schema grants/fixed
role membership/grant options or direct counter rights. Membership does not confer
dbo ownership. Applied D07–D11 resources remain immutable; a new migration creates
roles with fail-if-exists and preflights all memberships before Down drops roles.

Approved four WITHOUT LOGIN users/EXECUTE AS USER prove actual database operations,
allowed outputs/ownership chains and grant/revoke/deny behavior with rollback.
This is not login authentication or completed real deployment/demo identities.
E03 login/Windows-linked demo and runtime/schema/maintenance identity separation
remain operator-profile prerequisites; no secret/server-login/owner changes here.
Owner-resolution15517 on ALTER/history-delete is paired with effective-rights0;
not misreported as permission-only failure. Cross-role impersonation uses isolated
test members. An initial additional manager diagnostic probe was rolled back and
verified absent; final tests use the approved four names. No extra persistent user.
Exact matrix/roundtrip/main819test evidence is retained in D12_TASK_CARD/evidence.
D13 structural/EF diagnostic is now applied (ADR168); full runtime readiness,
runtime connections and business authorization remain later.


## ADR-168: Pinned structural catalog diagnostics and explicit audit persistence boundaries

Status: Implemented — 2026-10-05, owner-authorized D13→A01 scope; no product rule change.

D13 compares complete ordinal native catalog facts and immutable module bodies, not
object counts. Only leading CREATE/ALTER header and line endings normalize; body
changes remain drift. Native CHECK/default spelling is pinned from independently
verified D12 state with immutable-source provenance and disconnected EF parity;
not a generic SQL semantic parser or auto-refresh. Diagnostic command timeout and
cancellation are supplied explicitly; caller connection/transaction ownership remains.
Operator structural PASS never certifies production runtime profile/login readiness.
Restricted unreadable metadata/history fails closed; no permission relaxation.

A01 uses immutable projected event snapshots, accepted E04 field families and54SC02
mandatory action policies; API133 extra history remains excluded. Child `.EFFECT`
shares operation and requires positive projected parent references. Required reasons
use existing NFC/trim1000code-point normalization. Security technical action strings
AUTH.LOGIN/LOGOUT/ACCESS_DENIED/THROTTLED are selected by four typed event kinds;
no request action strings, credentials/payload/digest/raw errors in fallback.
Mandatory adapter never owns commit or catches away failure: caller must roll back
business/revocation/replay/counter/children on sanitized SQL failure or cancellation.
Pure security persistence is independently supplied; failure/cancellation increments
fixed thread-safe saturating numeric incidents and never compensates security state.
SECURITY/IP exact90days; MANDATORY no expiry/default IP. No cleanup/runtime DI/routes.
Real callers, replay/no-op suppression and non-SC02 password/bootstrap integration
remain in later authorized families. Scope/evidence: D13/A01_TASK_CARD/evidence.


## ADR-169 — Explicit operator bootstrap and guarded CSRF integration

Status: Implemented — 2026-10-05; owner-approved A02→A04 scope, no product rule change.

Bootstrap uses the canonical EC:ADMINSET gate and SERIALIZABLE role/account range locks.
Caller owns the complete transaction/rollback; repository never commits/retries. Supplied
finite SQL/lock deadlines and operator Role/Account INSERT rights are required. Exact
seven role codes are checked; an existing Admin in any status is never recreated/reset/
unlocked or given a new credential. Initial Admin receives an operator-selected regular
P08-compliant password only in memory (not generated temporary staff onboarding).
SYSTEM.ADMIN.BOOTSTRAP is an internal technical mandatory action with null actor because
no Account actor exists yet. Fixed projection contains roleId/ACTIVE/mustChangePassword
false only, no contact/credential/operator snapshot or expiry/IP. It shares caller TX.
Real operator provisioning is deferred; rollback fixtures are not a real Admin seed.

A04 uses framework antiforgery with explicit supplied key provider/canonical HTTPS origin
and verified-identity resolver contract. No production ephemeral-key/config fallback.
__Host-ec-csrf and __Host-ec-auth require Secure/HttpOnly/SameSiteStrict/Path=/, no Domain.
A05 still owns real JWT/session cookie issuance and expiry; A06 owns live session/account
verification without idle renewal. Unsafe methods require one exact Origin and framework
header token; form fallback is rejected. Transport/profile and identity precede CSRF,
normal-scope restriction follows it. Unclassified API metadata and invalid identities
fail closed. API001 requires a private verified-boundary marker before token minting,
rejects body/query, emits only requestToken/no-store, never touches sessions.
Production Program stays unmodified/unexposed until its dependencies are authorized;
fixture resolver headers and ephemeral provider do not prove runtime authentication.
A04 is CSRF/HTTP boundary; Student signup is A03. No requirement was reopened.
Scope/evidence: A02/A04_TASK_CARD and evidence; implementation exposure: docs/api.md.

## ADR-170 — Explicit signup/login integration and bounded issuance

Status: Implemented — 2026-10-06; owner approved exact A03→A05 worker sharing and
isolated rollback fixtures. No product decision reopened or runtime activation.
Anonymous signup uses STUDENT.ACCOUNT.CREATE internal mandatory action, null actor,
Account reference and fixed roleId/ACTIVE/mustChangePassword projection; no PII/secrets.
A04's private verified guard now protects all three explicit auth adapters. Required
caller body limit is enforced in RAM before CSRF, strict DTO binding afterward.
Scoped finite SQL/lock deadlines restore prior connection settings. SQL Server SET
LOCK_TIMEOUT requires a numeric literal: only validated Int32/invariant formatting
is emitted; all account selectors remain parameterized. No user text enters this SQL.
Fresh locked Account values replace unchanged EF tracked snapshots; preexisting modified
tracked accounts are rejected. Known email/phone constraints and deadlock/lock timeout
map409; other SQL errors remain sanitized500. Wrong-password counters commit before401.
Verified legacy hashes can rehash without applying new-password strength rules again.
JWT NumericDate expiry rounds DOWN to seconds, never extending DB datetime2(3) expiry;
a temporary credential with no usable whole-second JWT interval gets generic401.
Raw clock cutoff is checked before session DML, commit and HTTP cookie. Account status
and security version come from the locked row; no session upgrade/revocation/idle renew.
Production key/principal/origin/proxy/current-session resolver remains explicit and
unconfigured. Approved fixture execution is rollback-only; owned executor commit paths
are source-reviewed plus HTTP fixture seams, not persisted SQL or committed stress proof.

## ADR-171 — Fresh live-session authority and current-session logout integration

Status: Implemented — 2026-10-06; owner approved bounded A06→A07 payload/rollback SQL.
No product decision reopened. Supplied JWT keys/issuer/audience/algorithm/token byte
limit are explicit; framework validates signature, issuer/audience/type and raw clock
lifetime, with no skew extending deadlines. Strict required private claims are positive
canonical Account/version,32byte canonical SID, exact scope and integer iat/nbf/exp.
Proof cannot supply role/contact permissions; fresh SQL Account→Session locks guard
current status/version/SID relation/revocation/time/scope and fixed current role.
Temporary bundle must satisfy the existing exact24h invariant; restricted never upgrades.
Fresh SQL snapshots copied over an unchanged tracked entity explicitly reset Unchanged;
otherwise EF can retain false modified flags. SqlLoginRepository receives that narrow
A05 prerequisite fix; dirty caller entities still fail STATE_CONFLICT, never overwritten.
Private proof stored by the live resolver plus A04 verified marker protects adapters.
Current context checks occur again after pre-CSRF resolution. Explicit business activity
hook requires valid NORMAL session and caller transaction; no real business route mounted.
Invalid cookie401 schedules secure host-only cookie removal at response start so global
error clearing cannot strand anonymous CSRF/login recovery. It preserves401/no fake
server revocation;403/500 do not delete a valid session cookie. Deliberate refetch only.
Current logout rechecks live facts, updates one SID, raw cutoff checked before commit,
then A01 independent AUTH.LOGOUT and204/cookie removal. Logging failure cannot compensate.
Read/current revoke SQL cores verified rollback-only; owned request commit paths source
reviewed plus HTTP fixture seams. No persisted concurrency/runtime/readiness claim.


## ADR-172: Implement A08 transaction and immutable EC-C1 primitives

Accepted implementation refinement2026-10-06 under owner-approved A08→R01 scope;
no accepted product contract changed. Exact API004 allows normal/restricted proof,
mandatory caller-TX own-password mutation with projected AUTH.PASSWORD_CHANGE audit;
no credential/hash snapshot or new replay action. Account serializes protocol writers;
explicit binary-SID lock queries enforce acquisition order (bulk ORDER BY is insufficient).
Round trips are intentional, no unapproved session cap. Secure cookie deletion after
commit only; production owned COMMIT paths source-reviewed/HTTP fixture seams while
SQL validation remains rollback-only. Default Program stays unchanged/unmounted.
R01 freezes exact55registry/6lookup metadata, schema-aware full normalized intent,
immutable copy-out EC-C1 bytes/SHA25632. Sets vs ordered weekly plan, composite primary
then remaining ASCII ID keys, required absence and optional-null behavior stay accepted.
No live-state filtering/RV comparison during digest, T23 storage/lookups or workflows.
Codex corrects worker buffer placement and independently tests; skills grant no extra
scope. Only existing isolated DB, rolled-back four fixture users/DENY/regression drift;
no schema/profile/principal/package/runtime/deployment authority opened. R02/R03 deferred.

## ADR-173 — R02 completion protocol and first Admin lock caller

Implemented2026-10-06 under owner-approved R02→R03 scope; no product decision changed.
Request applock uses exact caller/action/lowercase UUID partition, Exclusive/Transaction/
public and positive finite timeout; all negative results abort with accepted503/500.
T23 append-only completion stores exact registry safe references, digest/operation/
UTCms/exact24h expiry. Current stored-target read authority precedes digest mismatch.
R03 discovers request/stored IDs only to prelock a complete ascending Account set after
request/ADMINSET, then locks binary SIDs individually; discovery grants no authority.
Changed route key intent returns409 mismatch after current stored-target authority,
not false404 or new-target RV check. Malformed persisted reference fails closed after
live proof. Admin detail uses short SNAPSHOT data buffering and a separate final live
authorization transaction; current-session reads/replay/lookup never renew activity.
New lock activity is an explicit same-TX A06 hook. Self/last-active/RV/state/version/
all-session revoke, mandatory audit and completion share one transaction. API014 supports
all six accepted Admin selectors as safe metadata only, not extra Admin mutations.
SQL callers checked only rollback fixtures, owned COMMIT source-reviewed with real
HTTP fixture commit seams; no persisted/committed stress/readiness claim. Program/
profile/keys/principals/packages/schema/grants unchanged, next U02/U01 deferred.

## ADR-174 — U02 controls and U01 explicit RAM/session boundary

Implemented2026-10-07 within owner-authorized U02→U01 scope; no product rule changed.
Use installed React/Tailwind/system fonts and native inputs/select/buttons/dialog;
44px controls, visible focus/contrast, explicit keyboard wrap and opener restoration.
Tailwind source scan is bounded to frontend src/tests, avoiding unrelated text.
HttpClient exposes only fixed existing auth routes and explicit calls: cookie
same-origin/no-store/redirect-error, current RAM CSRF for writes, exact error/context
validation. /me may deliberately probe cookie identity from empty RAM; no renewal.
RequestScope abort/epoch/latest-slot prevents stale publication even if abort ignored.
Each login marks a new boundary even for the same account; authority changes/current
401 clear RAM before publication. CSRF errors require deliberate context/token recovery,
never write retry. Unknown login/logout/password clears only still-current local
identity and reports unconfirmed server outcome; stale failures cannot clear a new session.
Cleanup hooks hold no ambient caches, storage, JWT/raw SID, UUID/retry bodies or timers.
Real identity UI/cross-tab/routing/runtime remain deferred. Two bounded AGY runs reviewed
by Codex;24frontend tests and technical browser fixture prove these seams only.

## ADR-175 — U03 signup and U04 explicit identity UI lifecycle

Implemented/reviewed PASS/applied ADR163/reverified2026-10-07 under owner U03→U04;
no product rule or API exposure changed. Only pure forms delegated through exact
approved external contexts; Codex independently controls integration/tests/review.
Signup sends exact accepted fields/raw password once, validates submit hints only,
filters public field errors, never auto-login/resend or infer rollback from lost201.
IdentityApp is explicit supplied-client integration; default unavailable App remains.
Normal first-F destinations and restricted password-change gate carry no business
UI authority. Fresh me proof precedes protected rendering after startup/tab signal/
focus/visibility/deadline; reads never renew idle or send mutation/refresh heartbeat.
Timers use server deadline interval and elapsed setTimeout, not browser wall time;
equal/past proof fails closed. Incoming literal-only channel signals clear RAM and
revalidate without echo/storage; no PII/context/token/deadline is transmitted.
New login even same account invalidates old RAM, then fetches CSRF; generic401 and
429 do not resend. Logout204/current401 clears local proof;403 does not claim success;
unknown removes sensitive RAM and reports unconfirmed server revocation. Epochs and
controller operations protect newer identity from stale responses. Disposal clears
RAM/listeners/timers; supplied-client changes cannot render another client's context.
Codex fixes native fetch receiver through lexical globalThis.fetch wrapper; technical
Edge fixture verifies default fetch.38frontend tests and three-width/two-tab fixtures
prove these seams; real cookies/runtime/SQL/other-device stress remain unverified.
No packages/backend/schema/profile/keys/principals/push/deploy/U05+ scope opened.

## ADR-176 — U05 own password UI and U06 bounded Admin lock pilot

Implemented/reviewed PASS/applied ADR163/reverified2026-10-08 within owner continuation.
Two exact prepared payloads authorized by continuation; sole pure views delegated,
Codex owns source integration/fixes/tests/review. No product/API exposure change.
Password form preserves raw current/new credential, local confirmation only; exact
expectedVersions.account from current authority. Wrong current400 preserves proof,
403 needs deliberate CSRF recovery,409 clears raw values/requires reload.204 clears
all RAM/signals/timers-sensitive state and returns login without scope upgrade;
unknown clears credentials/proof and offers deliberate new-password login/Admin.
Dirty navigation uses shared dialog/beforeunload without persistence promise.
Admin NORMAL alone reads exact12field current detail and confirms lock effect.
Fixed descriptor transport validates target Int32/UUID; one new UUID after deliberate
confirmation freezes target/RV/body, not on reads/retry. Advisory capabilities do not
replace server self/last-active/state/RV/revocation/audit/T23 guards.200 displays
server current projection, never optimistic status or a historical snapshot claim.
Unknown clears target PII/preview and keeps safe request/target reference in RAM.
Manual same-caller ADMIN.ACCOUNT.LOCK lookup200 is safe completion only;404 does not
prove rollback. Refetch and an explicit decision precede a new unresolved intent;
no polling, automatic mutation replay, secret retrieval or extra Admin commands.
Epochs/abort/cleanup prevent stale identity responses and destroy RAM on boundaries.
Worker default import/noncanonical colors corrected by Codex with original retained.
48tests and Edge3widths/roles/scopes/dialog/unknown/stale checks PASS on manager/main.
Default App/main, backend, packages/schema/profile/principals remain unchanged;
SQL not rerun, real production cookie/HTTPS/operator/runtime/V01 proof unverified.

## ADR-177 — standing authority to finish assigned tasks

Owner explicitly expanded authority2026-10-08: from now on each assigned task
includes the aspects necessary to complete it. Codex prepares concrete bounded
contracts, delegates via antigravity-worker where useful, runs relevant checks
and applies reviewed PASS under ADR163 without requesting the same permission.
Current V01-D payload/loopback HTTPS/browser/rollback SQL scope is authorized.
Historical pending gates are superseded for that task. This is task-bound
execution authority, not assignment of unrelated features or publication.
Keep owner files protected, secrets out of external context/evidence, rejected
output detached and operational changes bounded by the concrete task contract.

## ADR-178 — V01-E real owned COMMIT in identity-owned scratch databases

2026-10-08. Accepted within assigned V01-E/standing completion authority ADR177.
Direct production executors use their real owned transactions and COMMIT on new,
previously absent EnglishCenter_V01E_20261008_<32hexGUID> scratch DBs on local
SQLEXPRESS. Apply only the existing six migrations; synthetic identities/keys in
fixture scope, no real operator principals. Retained D07 remains read-only.
Pooling disabled, compatibility160/snapshotON/AUTO_CLOSE OFF. AUTO_CLOSE inherited
ON briefly hid recovery GUID during the first cleanup; exact recorded name/id/guid
recovery removed that scratch DB before rerun. Cleanup never adopts a DB: require
captured database_id and database_guid plus strict name validation, then DROP only
that fixture-created DB. No wildcard deletion, original DB or server login changes.
Manager/main exact12cases verify persisted owned effects, actual three-context
SNAPSHOT/final live guard, independent security audit plus timeout fallback, and
bounded two-client races. This complements D browser/caller-rollback evidence;
it does not establish a combined browser→owned COMMIT or least-privileged runtime
profile/principal/production PKI. Full V01/M0 remains incomplete. Test-only changes,
no production/default entry/package change. See V01E_TASK_CARD and E evidence.

## ADR-179 — unmounted essential profile/readiness core (2026-10-08)

V01-G implements immutable typed essential descriptors and reference-only static
validation without choosing D12/D13/DF01–03 environment values. Fixed160/snapshot,
UTC+7/HTTPS/CSV capacity and finite deadline ordering are enforced; no defaults or
loaded secrets. SQL timeout conversion must fit int milliseconds; lock<SQL<request,
CSV/readiness<=request, actual remaining lock budgets strictly larger than lock.
Nine mandatory fresh probe categories gate readiness; missing/duplicate/unknown,
false/exception/timeout fail closed, no cached-ready fallback. Public result remains
literal ready/not_ready200/503, internal diagnostics fixed enums. Caller cancellation
propagates; late async faults observed, concrete probes own cancellation/resource
cleanup and must return tasks promptly. Future probes must prove real dependencies,
including non-dbo/ec_app_runtime authority and exact catalog/snapshot; owner-mode
SQL inspection or descriptor presence cannot establish readiness. No probes/DI/health
routes/config loader/Program mounting supplied. Backup/recovery values remain gated
before those operations. G manager/main983PASS/55opt-in SKIP/buildclean; no SQL run,
frontend/schema/package/default entry changes. Full V01/M0 incomplete. Next bounded
concrete probe contract; accepted owner deferrals preserved, no repeat questions.
