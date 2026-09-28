# Current State

Last updated: 2026-09-28

## Current task

Agree on the complete design baseline with the owner before discussing the
implementation plan. A substantial normalized report baseline and the six
accepted resolutions are saved, but detailed functional specifications and
field constraints still require completion. Application implementation has
not started in this workspace.

## What has been done

- Inspected the workspace and the complete local instruction kit.
- Confirmed both copies of `requirements.md` under `instructions/` are
  byte-identical.
- Confirmed `instructions/english-center-agent-kit/docs/plan/` contains only
  empty scaffolding; there is no `plan.md`, question file, or slice plan yet.
- Created repository-level Project Memory linked to the canonical instruction
  files rather than duplicating them.
- Inspected the downloaded `codex-with-chatgpt` skill and its C2C protocol.
- Created the local `.agents/skills/codex-with-chatgpt/` package; discovery is
  confirmed in this session. This task did not start or verify its connection.
- Clarified that `instructions/` is an owner-managed input inbox, not the
  project source tree or Project Memory.
- Clarified that multiple skills and agent integrations may coexist; their
  responsibilities will be defined per workflow rather than forced into one
  global exclusive role model.
- Read the official Word report's full text/tables and distinct images; excluded
  its illustrative forms from requirements. The source Word remains unchanged.
- Created canonical `docs/requirements.md` and `docs/reference/REPORT_BASELINE.md`.
- Recorded per-record retakes before confirmation, per-program weights, /100
  grading, warning-only overdue behavior and removal of scholarships.
- Normalized the 20-entity target model; ADR-007 supersedes the old requirement
  source. Outstanding questions remain explicitly open in Q-004–Q-010.
- Recorded owner-approved uniqueness/repeat-enrollment, student-owned profile
  editing/course lock and reasoned CONFIRMED-only rejection (ADR-015–ADR-017).
  Detailed lifecycle predicates and rejection-data design remain open.
- Recorded accepted reporting scopes/date/count bases (ADR-018). Q-012 now
  contains remaining detail questions, not the already-agreed core definitions.
- Recorded email/password login and student required/editable fields (ADR-019);
  administrative account operations and validation/security parameters remain open.
- Recorded authenticated password change, Admin availability protection and
  account-state meanings (ADR-020); ADR-048–ADR-052 subsequently resolve delivery,
  revocation, reactivation, login-email immutability and timed throttling.
- Recorded accepted test rebooking/absence/pending-retake rules (ADR-021).
  Logical attempts must be distinguished from bookings; target history schema,
  absence actor and center cancellation details remain unfinalized.
- Recorded enrollment cancellation rights/unpaid-invoice coupling and finished-
  record eligibility (ADR-022). Completion requires Class COMPLETED and the
  student's GradeReport FINALIZED, not passing; CANCELLED/REJECTED also permit
  new enrollment while keeping history. Database encoding remains open.
- Recorded OPEN-only placement, ADMITTED retained/derived placement display,
  and pre-teaching teacher changes with full schedule checks (ADR-023). Class
  lifecycle prerequisites subsequently follow ADR-025 and transfer scope is
  excluded by ADR-040.
- Recorded COMPLETED-only grade submission/finalization, DRAFT/RETURNED edit
  rights, SUBMITTED/FINALIZED locks and finalized >= 50/100 passing (ADR-024).
  Rounding was subsequently resolved by ADR-028 and whole-class operation
  scope by ADR-034; pass-rate cohort subsequently resolved by ADR-067.
- Recorded explicit AcademicAffairs class opening/start/completion prerequisites
  (ADR-025). No automatic date-based transitions or finalized-grade prerequisite;
  session occurrence representation remains open; full-schedule rules were
  subsequently resolved by ADR-032.
- Recorded empty PLANNED/OPEN-only class cancellation by AcademicAffairs with
  reason/history preservation and removal of cancelled schedules from resource
  reservations/upcoming teaching/completed-session counts (ADR-026).
- Recorded assigned-grader post-session absence, pre-start reasoned center
  cancellation with historical bookings/same-attempt self-rebooking, and
  date/time/room edit cutoffs (ADR-027). Source provenance distinguishes report
  CANCELLED-state rules from owner-approved operational additions.
- Recorded first-use program/Band configuration freezes and two-decimal input/
  half-up final-total rounding before storage/display/passing (ADR-028).
  Entry-score correction and tied-best attribution were subsequently resolved
  by ADR-029.
- Recorded audited original/current-grader-only entry-score corrections in
  TESTING/BAND_ASSIGNED, locks after confirmation or cancellation/rejection,
  no privileged override and ties favoring attempt one (ADR-029).
- Prepared `docs/plan/HANDOFF.md` for the next chat to resume full design
  agreement without assuming code or whole-baseline acceptance.
- Recorded session creation as CLOSED, guarded pre-start opening/reopening,
  CLOSED student-booking locks, effective closure at start and audited pre-start/
  pre-result grader replacement (ADR-030). Grading/absence/correction remain
  available under ADR-027/029; pending-retake confirmation guards remain intact.
- Recorded atomic Registration/UNPAID Invoice/future-booking cancellation,
  including pre-start CLOSED, preserved started test/scores/Band history and
  stopped grading/correction/absence on cancelled records (ADR-031). Repeat
  enrollment starts testing anew; Q-007 retains data/audit representation only.
- Recorded program-level positive LESSON counts frozen after first Class use,
  full LESSON timetable required before teaching start (not opening), and
  pre-start/no-dependent-data date/time/room edit cutoffs (ADR-032). Attendance
  entry/correction window was subsequently resolved by ADR-033;
  actual-session occurrence representation remains open.
- Recorded attendance entry/correction cutoff before Class COMPLETED, with
  no fixed 24/48-hour per-lesson deadline; COMPLETED attendance is read-only
  even before grade submission/finalization (ADR-033). Assignment rights and
  complete-attendance prerequisite for class completion remain unchanged.
  Owner also accepted opening attendance at LESSON start, not before or only
  after lesson end; the whole attendance window is resolved under ADR-033.
- Recorded mandatory whole-class grade submission/resubmission, reasoned
  return and finalization/publication, with all-or-none transitions (ADR-034).
  All students require complete component scores before sending; individual
  draft/correction entry and correction/resubmission remain allowed. Existing
  edit locks remain; data representation is deferred, no code was implemented.
- Recorded non-held class-wide lesson scope within NV19/F21, excluding a
  separate holiday module/center-wide calendar, automatic holiday skipping/
  makeup scheduling and individual makeup lessons (ADR-035). Non-held sessions
  are not delivered lessons or a reason to mark all students absent. Concrete
  postponement/replacement rules were subsequently resolved by ADR-036.
- Recorded pre-start moves under ADR-032 and post-start non-held LESSON handling
  by AcademicAffairs with reason, preserved history and linked replacement:
  no old dependent data/non-ended class, current teacher/conflict checks and
  consistent all-or-none writes. One original/replacement LESSON obligation;
  completion checks replacement occurrence/attendance, not fake old attendance
  (ADR-036). Data representation remains open; extra non-replacement LESSONs
  were subsequently excluded by ADR-037. No code/schema was added.
- Recorded exclusion of extra LESSON teaching obligations beyond the fixed
  program count (ADR-037). Weekly/individual additions fill the required count;
  linked replacements preserve one obligation, retained history may exceed the
  raw row count, and FINAL_EXAM remains separate. No code/schema was added.

- Accepted Course immutability/current-profile historical display/no transfers/
  Student-owned Invoice browser printing/Accountant metadata-only Payment
  correction (ADR-038–ADR-042), preserving financial snapshots and cancellation
  limits. No application implementation started.
- Accepted positive tuition/no free-course path, VND whole dong, Invoice due date
  local issue date +7 calendar days, confirmation time only/no separate actual
  receipt field, and UTC+7 business dates/schedules/report filters (ADR-043–ADR-047).
  Synced affected specification/catalog/questions/handoff; no code/schema added.

- Accepted temporary-password delivery/forced change, all-session invalidation
  on lock/disable/reset/password change, same-account DISABLED reactivation,
  immutable login email and timed failed-login throttling (ADR-048–ADR-052).
  Synced affected design memory. No source code/schema or whole-baseline approval.

- Accepted passphrase-friendly 15–128-character passwords, 24-hour temporary
  credentials, two-hour absolute/thirty-minute idle normal sessions, concurrent
  devices and server-side current-session logout/no logout-all (ADR-053–ADR-057).
  Synced affected specification/handoff; no implementation or stage completion.

- Accepted business/security audit coverage, structured Admin filters/pagination,
  relevant secret-free event details and no audit CSV/PDF export (ADR-058–ADR-062).
  Synced affected memory. Prepared five Q-012 metric proposals, not accepted rules;
  implementation and the full-baseline acceptance remain pending.

- Accepted current-state/no historical as-of reporting, issuance-period billing/
  current debt separation, distinct students, LESSON-only progress and FINALIZED
  pass-rate cohorts (ADR-063–ADR-067). Report-filter proposals were reviewed next;
  no code/schema/stage completion.

- Accepted report defaults/filters, Director aggregate-only detail and Accountant
  all-filtered-results/report-columns CSV (ADR-068–ADR-072). Synced affected
  memory. Profile validation was reviewed next; no code, final schema or
  stage/full-baseline completion.

- Accepted name/DOB/email/contact-phone/address validation (ADR-073–ADR-077),
  with no new verification, age restrictions or staff editing/required fields.
  Updated affected design memory; remaining catalogue/reference/reason fields
  and operational viewing still need review. No code or baseline completion.

- Accepted catalogue names, immutable manually entered Course/Class codes,
  consistent room codes, plain-text reasons/optional comments and bounded Payment
  references without global uniqueness (ADR-078–ADR-082). Synced affected memory;
  Q-011 operational viewing/UI/history proposals remain unaccepted. No code,
  schema or stage/full-baseline acceptance.

## Relevant files

- `docs/design/FUNCTION_CATALOG.md` — 39 source references, NV10/NV11 removed;
  remaining detail review is not final approval.
- `AGENTS.md`
- `docs/PROJECT_CONTEXT.md`
- `docs/ARCHITECTURE.md`
- `docs/CURRENT_STATE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `docs/plan/plan.md`
- `docs/plan/design-baseline.md`
- `docs/plan/questions.md`
- `docs/plan/HANDOFF.md` — next-chat resume guide; canonical memory takes precedence.
- `instructions/english-center-agent-kit/`
- `docs/requirements.md`
- `docs/reference/REPORT_BASELINE.md`
- `.agents/skills/codex-with-chatgpt/SKILL.md`

## Known issues and blockers

- `C:\Study\SOEN\main_workplace` is not currently a Git repository.
- No backend, frontend, database schema/migrations, tests, or runtime config
  exist in the workspace, so implementation status cannot be verified.
- The four-phase breakdown mentioned by the instruction kit is not defined.
- The requested skill name was `chatgpt-with-codex`, while the installed skill
  is named `codex-with-chatgpt`.
- The local skill is present in this session's active catalog. Its runtime
  checkout remains external; the global `c2c` command was not linked at the
  last check, but its documented Node entry point is available.
- The C2C checkout's Git status could not be read in the sandbox because Git
  considers its ownership unsafe. No global Git configuration was changed.
- Report screenshots are UI previews; they do not prove implemented APIs,
  database integration or successful tests.
- Undefined behaviors Q-004–Q-010 must be resolved or explicitly excluded by
  the owner before the complete design baseline is accepted.
- Function detail gaps Q-011 and report definitions Q-012 were identified by
  rechecking report sections I–III/V. Walk-in registration, CSV and schedule
  entry are now resolved by ADR-012–ADR-014; other proposals remain unaccepted.
- Windows sandbox currently fails setup refresh; documentation changes used
  apply_patch through elevated execution.

## Next action

Agree on remaining function/field details Q-011 and report refinements Q-012;
do not re-ask rules resolved by ADR-015–ADR-082. Q-006/Q-007/Q-009 business policies
are resolved. Resume from `docs/plan/HANDOFF.md` and the owner's consolidated
approval file `docs/plan/OWNER_REVIEW.md` (created 2026-09-28): 50 unaccepted
design choices plus 5 output-acceptance gates, all unchecked. The owner now
prefers editing this four-column table over sequential chat questions; the
five Q-011 proposals are A01–A05 there. Read the owner's checks/alternatives,
record only approved choices, then complete the design outputs in stage order.
Do not approve unseen outputs or mark a stage done from creating the table.
Q-010 remaining-field policies are accepted ADR-078–ADR-082; technical
validators/storage remain later design. Profile validation is
accepted ADR-073–ADR-077; required/editable fields stay ADR-019.
Report filters/defaults/Director detail/Accountant CSV scope are accepted
ADR-068–ADR-072; query/export mechanisms and rate display remain later design.
Metric bases/current-state scope are accepted ADR-063–ADR-067; do not re-ask.
Audit coverage/events/Admin filters/safe details/no export are accepted
ADR-058–ADR-062; storage/failure/anti-flooding/retention remain later design.
Keep stage 1 doing until the complete function output is accepted. Remaining
discussion estimate in the active plan is historical; the table is the current
review surface, not a promise that no further genuine gaps can be discovered.
Password policy/temporary 24-hour expiry/normal 2-hour or 30-minute-idle
expiry/concurrent devices/current-session logout are accepted ADR-053–ADR-057.
ADR-048–ADR-052 resolves delivery/mandatory change/revocation/reactivation/email/
throttling; activity definition/restricted authentication/storage/enforcement
remain later security design.
Money/deadline/business timezone rules are accepted in
ADR-043–ADR-047; remaining numeric/time storage constraints are later design work. Course
changes/profile snapshots/transfers/Invoice printing/Payment correction scope
are resolved by ADR-038–ADR-042. Extra LESSONs are excluded (ADR-037).
ADR-035 resolves scope within NV19/F21 and ADR-036 resolves the accepted
pre-start move/post-start linked replacement/counting/completion workflow;
storage/history/audit remain later design work. Whole-class grade operation scope
is resolved by ADR-034. Attendance opens at LESSON start
and closes before Class COMPLETED (ADR-033), no fixed per-lesson deadline;
do not re-ask either boundary. Expected lesson counts/full schedules
and date/time/room edit cutoffs are accepted in ADR-032. Registration cancellation effects
are accepted in ADR-031; CLOSED/OPEN and grader replacement remain ADR-030.
Q-008 retains actual-session occurrence representation,
not accepted opening/start/completion rules.
Q-005 retains database encoding and
Q-007 retains cancellation/history/audit representation and enforcement.
Q-004 now tracks booking/history representation, cancellation/grader-change
audit and effective-closure persistence, not accepted session operational rules.
Stage 1 remains in progress; database/architecture
follow after the full functional/workflow agreement.
Preserve the six accepted decisions and leave Word unchanged. Do not start Git
initialization, scaffolding, Slice 0 or application coding now.
