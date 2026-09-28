# Full Design Agreement

Status: doing
Owner direction: 2026-09-27

## Objective and boundary

Agree on the entire software design before discussing the implementation plan:
business functions → workflows → database → system architecture → consistency
review and owner acceptance. This is a design plan, not authorization to code.

Use the official report through its normalized memory and provenance in
`../reference/REPORT_BASELINE.md`. Preserve the six accepted resolutions and
the accepted stack. Do not edit Word or treat illustrative forms as requirements.
Do not initialize Git, scaffold applications, write migrations, implement APIs,
activate slices or invoke external coding-agent connections during this stage.

## Stages and acceptance outputs

| Stage | Status | Output to agree with the owner |
|---|---|---|
| 1. Business functions | doing | Complete module/function catalog, actor permissions, inputs/outputs, validations, scope and correspondence with the report/screens. |
| 2. Workflows | todo | Normal and exception paths; permitted state transitions, actors, prerequisites, effects and transaction boundaries. |
| 3. Database | todo | Data dictionary: types, lengths/precision, nullability, PK/FK, relationships, unique/check constraints, indexes and historical-data rules. |
| 4. System architecture | todo | Backend/frontend boundaries, module responsibilities, interfaces, security, errors, logging and runtime/deployment assumptions. |
| 5. Consistency and acceptance | todo | Trace functions through workflows, data and architecture; resolve contradictions and obtain owner acceptance of the complete baseline. |

Each stage's proposals remain proposals until accepted by the owner. Mark a
stage done only when its output is agreed and recorded. Later discoveries may
require revisiting an earlier stage; record the affected decision explicitly.

## Current baseline and gaps

- `../requirements.md` records substantial normalized business rules, roles,
  states, screens and the six accepted resolutions. It is not yet a complete
  transfer of every functional detail or database field constraint.
- `../ARCHITECTURE.md` contains the target architecture and 20-entity model;
  neither constitutes implemented behavior or a fully finalized schema.
- Q-004–Q-010 in `questions.md` require business/design agreement. Add further
  verified gaps as they arise rather than silently supplying defaults.
- Q-001/Q-002 concern later implementation phases/pilot scope and are deferred.
- Agent integration contracts are required before their workflows are used;
  installing a skill does not start an integration.

## Working method and durable outputs

Owner preference updated 2026-09-28: consolidate all known remaining choices
in `OWNER_REVIEW.md`, with four columns (issue, recommendation, owner agreement,
owner alternative). This replaces sequential chat batches. The file contains
50 unaccepted design choices and 5 gated output-acceptance items; all were
initially unchecked. Do not approve unseen documents, infer agreement from blank
cells or re-ask accepted decisions. Present the report baseline,
any conflict or gap, and clearly labelled recommendations for the owner to decide. Keep
accepted content in `../requirements.md`, architecture/schema design in
`../ARCHITECTURE.md`, significant decisions in `../DECISIONS.md`, and remaining
questions in `questions.md`. Link focused design documents if detail outgrows
these files; avoid competing sources of truth.

Cross-check that every in-scope function has an actor, workflow, data support
and architectural owner, and that all state changes preserve the agreed rules.
Define expected acceptance behavior during design; executable code/tests belong
to later implementation. Any unresolved in-scope business rule prevents final
baseline acceptance unless the owner explicitly excludes or defers its feature.

## Next action and handoff

The review draft is `../design/FUNCTION_CATALOG.md`: all 39 source references in
report section III, with NV10/NV11 removed by ADR-012. F15 is also removed;
remaining codes are not renumbered. Q-011/Q-012 track functional/report gaps.
Stage 1 remains doing; a catalog is not yet an agreed detailed specification.

Walk-in registration scope, CSV exports and weekly/individual schedule entry
are accepted (ADR-012–ADR-014). Enrollment uniqueness/repeat-course rules,
student-owned profile editing/confirmed course lock and reasoned rejection
are accepted (ADR-015–ADR-017). Core report scope/count/date bases are accepted
(ADR-018); Q-012 retains only report refinements. Student required/editable fields
and email/password login are accepted (ADR-019). Own-password change, Admin
protection and account-state meanings are accepted (ADR-020). Test rebooking,
absence and pending-retake confirmation rules are accepted (ADR-021); Q-004
retains history/audit representation and effective-closure persistence after
ADR-027/ADR-030's accepted session operational policies.
Enrollment cancellation/unpaid-invoice coupling and finished-record criteria
are accepted (ADR-022): Class COMPLETED and student's GradeReport FINALIZED,
regardless of passing; CANCELLED/REJECTED also permit a new record with history
preserved. Q-005 retains database encoding; Q-007 retains cancellation/history/
audit representation after ADR-026/031. Placement/teacher changes are accepted (ADR-023):
OPEN-only placement, retained ADMITTED/derived placement display, teacher changes
only before teaching/attendance/grades with all schedule conflicts checked.
Grade submission/finalization, edit locks and finalized >= 50/100 passing are
accepted (ADR-024, Q-009 resolved); rounding is resolved by ADR-028 and mandatory
whole-class operation scope by ADR-034. Pass-rate cohort follows ADR-067;
Q-012 retains query/export-mechanism and display refinements after ADR-068–072.
Class opening/start/completion prerequisites
are accepted (ADR-025): explicit AcademicAffairs actions, no automatic date-based
transition or finalized-grade prerequisite. Q-008 retains actual-session
occurrence representation; full-schedule rules are ADR-032. Class cancellation
is accepted (ADR-026): empty PLANNED/OPEN only, AcademicAffairs/reason, history
preserved and cancelled schedules excluded from operational resource reservation.
Q-007 retains attribution/history representation. Test absence attribution
and center cancellation/rescheduling are accepted (ADR-027); source provenance
records report state rules versus owner-approved workflow additions. Program
configuration freezes after first Class use, Band thresholds freeze after first
completed entry test, and two-decimal inputs/half-up final rounding are accepted
(ADR-028). Audited entry-score correction by original/current grader only in
TESTING/BAND_ASSIGNED, post-confirmation/ended-record locks, no privileged
override and ties favoring attempt one are accepted (ADR-029; Q-006 business
policies resolved). Session creation as CLOSED, pre-start guarded opening/
reopening, CLOSED student-booking locks, effective closure at start and reasoned/
audited pre-start/pre-result grader replacement are accepted (ADR-030).
Registration cancellation atomically cancels active future bookings (including
pre-start CLOSED) and any issued UNPAID Invoice; preserve started/result/Band
history, stop result mutations on cancelled records, audit/guard the operation,
and start testing anew on repeat enrollment (ADR-031; Q-007 business policies
resolved). Program-level positive LESSON count/first-use freeze, full LESSON
schedule before teaching start (not opening), and pre-start/no-dependent-data
session date/time/room edit cutoffs are accepted (ADR-032).
Attendance entry/correction opens at LESSON start and closes before Class
COMPLETED, with no fixed
24/48-hour per-lesson limit; attendance becomes read-only regardless of grade
state (ADR-033). The complete-attendance prerequisite remains ADR-025.
Whole-class submission/resubmission, reasoned return and finalization are accepted
(ADR-034); all-or-none transitions, required scores for every student and existing
edit locks. Individual draft/correction entry and resubmission remain allowed.
Non-held class-wide lessons stay within NV19/F21, with no separate holiday module,
center-wide calendar/automatic holiday skipping or makeup scheduling/individual
makeup lessons; non-held is not delivered or all-student absence (ADR-035).
Future LESSON moves remain ADR-032; after scheduled start, non-held LESSONs are
preserved with reason/actor/time and replaced through a linked same-class lesson
by AcademicAffairs, no dependent data/non-ended class/current teacher/conflict
checks/consistent writes. One LESSON obligation, replacement occurrence/attendance
required for completion instead of the old historical lesson (ADR-036).
Extra LESSONs beyond the fixed program count are excluded; weekly/individual
creation fills required obligations, linked replacements do not add obligations
and historical row count is not the limit; FINAL_EXAM separate (ADR-037).
Accepted five-decision batch ADR-038–ADR-042: no direct Course changes on existing
registrations, no personal snapshots/current-profile historical display, no class
transfers, Student-owned Invoice browser printing only, and audited Accountant
Payment method/reference corrections only. Existing financial snapshots and
immutable money/Invoice/confirmation time remain; no cancel/delete/refund.
Accepted money/time batch ADR-043–ADR-047: positive tuition/no free-course path,
VND whole dong, due date local issue date +7 calendar days including holidays,
confirmation time only/no separate receipt time, UTC+7 business dates/schedules/
report filters. Numeric types/ranges and time storage/serialization remain later
database/architecture design; no code/schema approval.
Accepted account/security batch ADR-048–ADR-052: temporary password shown once
for manual delivery/forced change before business access, all-session revocation
on lock/disable/reset/password change, Admin same-account DISABLED reactivation,
immutable login email and login-only 5-failure/15-minute-window/15-minute throttle.
Manual LOCKED/DISABLED stay distinct; no schema/code approval.
Accepted password/session batch ADR-053–ADR-057: passwords 15–128 characters,
Unicode/spaces/no mandatory composition/common-compromised rejection/no routine
rotation; temporary expiry 24 hours; normal absolute two-hour/idle thirty-minute
expiry without refresh/remember-me; concurrent devices; server-side current-session
logout without logout-all. ADR-049 all-session revocation remains. Activity,
restricted-session definitions and enforcement remain database/architecture work.
Accepted audit batch ADR-058–ADR-062: mandatory business audits plus configuration/
class-lifecycle/schedule changes; authentication/account/denied-access events;
structured Admin filters/newest-first pagination; relevant details without secrets/
full payloads; no audit CSV/PDF export. Event storage, anti-flooding/failure and
retention/disposal/backup remain database/architecture design.
Accepted report-metric batch ADR-063–ADR-067: current-state/no past-date reporting;
issuance-period non-CANCELLED billing/current debt separate; distinct students
per Course and overall; LESSON-only confirmed-held obligations with exams separate;
pass rate from FINALIZED enrollment results by finalization date/empty cohort no
data. Query consistency/occurrence representation/rate display remain design.
Accepted report-filter/detail/export batch ADR-068–ADR-072: current-month periods,
all-state defaults, inactive historical course choices, training current Teacher/
Band/Class/state filters with a separate result period, financial Course/overdue
filters with date-free current debt, Director non-identifying group-only detail
and Accountant all-filtered-results/report-columns CSV. Query/export mechanisms
remain later design; no additional staff report rights.
Accepted profile-validation batch ADR-073–ADR-077: trimmed Unicode name 1–100,
real/nonfuture Student DOB/no age restriction, email <=254/account-wide
case-insensitive uniqueness/no provider rewriting, Vietnamese-mobile phone scope
with equivalent 0/+84 normalization, optional free-text address <=255. No email
verification/OTP or new staff required fields; exact validation/storage remains
technical design.
Accepted remaining-field batch ADR-078–ADR-082: catalogue name limits, immutable
manual Course/Class codes, normalized room codes across all schedules, plain-text
reasons/optional comments and bounded Payment references without global uniqueness.
Existing result/configuration freezes and one Payment per Invoice stay intact.
Next-chat handoff: `HANDOFF.md`. Active owner review: `OWNER_REVIEW.md`, A01–A05
for the previous five unaccepted Q-011 operational-view/UI/history proposals,
then the remaining catalogue/workflow/database/architecture choices in the same
file. Read owner checks/alternatives and sync only accepted content into canonical
memory. Compile complete stage outputs in order; E01–E05 must wait for the actual
documents and review. Do not re-ask ADR-001–ADR-082 or mark stage 1 done early.
Actual-session occurrence
representation remains Q-008; detailed security mechanisms remain later design.
Do not treat rejection as cancellation or ADMITTED as course completion.
Preserve the six resolutions and exclude illustrative forms. Then work through
the remaining stages in order. After the owner accepts the whole baseline,
ask to move to implementation planning and update `plan.md`.

## Remaining discussion estimate — provisional, 2026-09-28

Historical agent estimate after ADR-077, not an owner-approved schedule or
acceptance. The later owner request replaced chat rounds with `OWNER_REVIEW.md`;
its 55 rows are the current inventory, not a guarantee against new genuine gaps.
Retain the earlier estimate below for context, not as the next-action protocol:

| Remaining output | Estimated discussion rounds | Focus |
|---|---|---|
| Business-function closure | 2–3 | Remaining field constraints, operational read scope/UI/history, complete function output review. |
| Workflow output | 1–2 | Consolidate accepted transitions/normal-exception paths and resolve genuine gaps, not re-ask policies. |
| Database output | 2–3 | Booking versus attempt history; actual occurrence/replacement/history; dictionary/relationships/constraints/concurrency/security-state support. |
| Architecture output | 2–3 | Interfaces/errors/report export/security mechanisms; scale/performance/privacy/reliability/maintenance and deployment assumptions. |
| Cross-check and whole-baseline acceptance | 1 | Trace functions/workflows/data/architecture and resolve any contradictions. |

Total estimate: 8–12 rounds including review/acceptance, roughly 30–50 decision/
proposal items, not 30–50 new business rules. Cohesive technical proposals should
be reviewed as blocks, not individual questions for every SQL column. These are
planning estimates based on known gaps; new conflicts or revised owner choices
can change them. No stage is complete merely because its estimated rounds end;
accepted design outputs still determine completion. No implementation planning
or code is authorized by this estimate.
