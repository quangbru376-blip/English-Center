# Project Context

## Purpose

Build a responsive Vietnamese web application that supports the end-to-end
operation of an English language center: student registration, placement
testing, admission, invoicing and payment confirmation, class assignment,
scheduling, attendance, grading, and reporting.

This is a university final project for the course "Công nghệ phần mềm". The
owner must be able to explain the code in an oral examination, so conventional,
clear implementation is preferred over clever abstractions.

## Users and modules

The system has seven fixed, single-role account types:

- `Student`: self-registration, enrollment, placement tests, invoices,
  schedules, and finalized grades.
- `Admissions`: enrollment records and admission approval.
- `AcademicAffairs`: test sessions, courses, classes, schedules, class
  assignment, and grade finalization.
- `Teacher`: assigned classes/test sessions, test scores, attendance, and
  gradebooks.
- `Accountant`: invoices, payment confirmation, and financial reporting.
- `Director`: read-only management reporting.
- `Admin`: staff accounts, account locking/reset, and activity logs.

The original catalog describes F01–F33; F15 was removed by ADR-012, leaving
32 screen codes in scope without renumbering. Screen behavior and business
rules are defined in `docs/requirements.md`, not in this file. Screen priorities
in the older instruction kit remain planning proposals.

## Fixed technology stack

- Backend: ASP.NET Core Web API, C#, EF Core code-first, SQL Server, JWT.
- Frontend: React, Vite, TypeScript, Tailwind CSS.
- Backend tests: xUnit.
- UI: Vietnamese, responsive at 375 px, 768 px, and 1280 px.

Adding or replacing a framework/package requires owner approval.

## Important constraints

- Agree on the complete business/workflow/database/architecture baseline
  before discussing implementation planning. The active design plan is
  `docs/plan/design-baseline.md`; accepted E01–E05 now precede bounded coding.
  P02 empty backend/test skeleton is applied and verified on main; business
  implementation remains pending. See `docs/plan/P02_TASK_CARD.md`.
- Correct business rules and complete end-to-end flows take priority over
  polish.
- Payments happen outside the application and are recorded manually.
- Students perform their own account/course/test registration. Counter advice
  uses that same workflow; Admissions has no registration-on-behalf capability.
- Report file exports use CSV, not XLSX. AcademicAffairs may generate weekly
  schedules as dated sessions and add/edit individual sessions with conflict checks.
- Entry tests are in person; teachers enter scores.
- Each account has exactly one fixed role.
- Login uses email/password, not phone. Students require name, birth date,
  email and phone; address is optional. Student-owned edits cover name, birth
  date, phone and address, not login email (ADR-019).
- Logged-in users may change their password using the current password;
  forgotten passwords go through Admin, not email recovery. Admin cannot
  lock/disable self or the last active Admin (ADR-020).
- Each enrollment record allows at most two placement attempts. A retake
  requires a first-attempt result and must be registered before confirmation.
- Logical attempts differ from booking history: pre-start cancellation/change
  and absence rebooking do not create extra attempts. Absence is not a zero
  score; scheduled/taken-ungraded retakes block confirmation (ADR-021).
- Grading weights belong to the TrainingProgram for a course/Band. All scores
  are stored and displayed on a 0–100 scale. Submit/finalize only after Class
  COMPLETED; passing requires FINALIZED final_score >= 50 (ADR-024).
- Freeze program course/Band/components/weights after first Class use and the
  whole Band threshold set after the first completed entry test. Inputs accept
  at most two decimals; final totals round once half-up to two decimals before
  storage/display/passing comparison (ADR-028).
- Only the original entry-score teacher still assigned to the session may
  correct a COMPLETED result in TESTING/BAND_ASSIGNED, with reason/audit and
  atomic best-score/Band recalculation. No correction after confirmation or
  cancellation/rejection, no privileged override; ties favor attempt one
  while retaining both results (ADR-029).
- Overdue invoices produce warnings, not automatic student status changes.
- Entry sessions start CLOSED; AcademicAffairs opens/reopens only before start
  with valid resources/ACTIVE grader/capacity checks. CLOSED blocks all student
  booking mutations while preserving grading/absence/correction rights. At start,
  sessions are effectively CLOSED. Reasoned/audited grader replacement is allowed
  only before start and any score/absence, with conflict checks (ADR-030).
- Scholarships and their tables are outside the project scope.
- Registration cancellation atomically cancels active future test bookings,
  including pre-start CLOSED, and any issued UNPAID Invoice. Keep started test
  history/scores/Band, stop grading/correction/absence on the cancelled record;
  new enrollment starts testing anew without copying scores/Band (ADR-031).
- Program expected lesson count is a positive integer, LESSON only, frozen
  after first Class use. Full LESSON schedules are required before teaching
  starts, not before class opening. Class-session date/time/room edits require
  pre-start, no attendance/related scores and non-ended class (ADR-032).
- Assigned teachers enter/correct LESSON attendance from lesson start until
  before Class COMPLETED; no fixed 24/48-hour per-lesson limit. After completion,
  attendance is read-only regardless of GradeReport state (ADR-033).
- Submit/resubmit, return and finalize grades for the whole class in one
  all-or-none operation, never individual reports; preserve reasoned return,
  correction/resubmission and existing edit locks (ADR-034).
- Secrets must not be committed.
- Non-held class-wide lessons belong to existing NV19/F21 scheduling, not a
  separate holiday module, center-wide calendar, automatic holiday/makeup
  scheduling or individual makeup workflow (ADR-035). Move future LESSONs under
  ADR-032; for non-held past LESSONs, AcademicAffairs records reason/history and
  creates a linked replacement only with no dependent data and a non-ended
  class. Replacement fulfills one LESSON obligation and must occur with complete
  attendance before completion; no fake attendance for the old lesson (ADR-036).
- No extra LESSONs beyond the program's fixed count; weekly/individual additions
  fill required teaching obligations. Linked replacements do not add obligations,
  retained history may exceed the raw row count, and FINAL_EXAM remains separate
  (ADR-037).
- Existing Registration Course is immutable; changing choice requires valid
  cancellation/new registration and testing anew (ADR-038). Historical personal
  information uses the current profile, not registration snapshots; Invoice
  amount/business audit history remains preserved (ADR-039).
- Student class transfers after placement are excluded, even before teaching
  (ADR-040). Students may browser-print their own Invoice with accurate payment
  state, no dedicated application PDF export or separate receipt (ADR-041).
- Accountant may correct confirmed Payment method/reference only, with mandatory
  reason and old/new/actor/time audit. Money, Invoice link and confirmation time
  are immutable; no cancel/delete/refund or duplicate Payment (ADR-042).
- Tuition/Invoice/Payment amounts are positive VND whole dong, no free-course
  path. Invoice due dates are local issue date +7 calendar days including holidays;
  warning-only overdue behavior remains. Payment stores confirmation time only,
  with no separate actual-receipt field. Business dates/schedules/report filters
  use Asia/Ho_Chi_Minh (UTC+7) (ADR-043–ADR-047).
- Staff creation/Admin reset uses one-time-displayed random temporary passwords
  and mandatory change before business access, no secret logs; reset does not
  unlock/enable (ADR-048). Lock/disable/reset/own password change invalidate all
  existing sessions, even unexpired JWTs; reactivation never restores them (ADR-049).
- Admin may reactivate DISABLED on the same account with role/data/history kept
  (ADR-050); existing login emails cannot change, including Admin/staff (ADR-051).
  Five consecutive bad passwords in fifteen minutes block new login fifteen
  minutes; success/expiry resets counting, without status change or revoking
  current sessions. Manual LOCKED and DISABLED remain separate (ADR-052).
- Passwords use 15–128 characters, allow Unicode/spaces, no composition or
  routine expiry rules; block common/compromised passwords (ADR-053). Temporary
  credentials expire after 24 hours, then require Admin reset (ADR-054).
- Normal sessions expire after two hours from login or thirty minutes idle,
  whichever first, without refresh/remember-me (ADR-055). Concurrent device
  sessions are allowed (ADR-056); logout revokes only the current session
  server-side, no logout-all button (ADR-057). ADR-049 still revokes all on its
  events. Persistence/revocation/timers are accepted ADR-092; activity/restricted-
  tab/idle activity, restricted authentication and cookie/CSRF transport are
  accepted separately in ADR-097–ADR-099 (D03/D04/D06).
- Audit keeps mandatory business history and adds configuration/class-lifecycle/
  schedule changes plus authentication/account/denied-access events (ADR-058/059).
  Admin has structured filters/newest-first pagination (ADR-060), relevant event
  details without credentials/full payloads (ADR-061), in-app viewing only/no
  audit CSV/PDF export (ADR-062). Operation grouping and history are accepted
  in ADR-093; D08/D09 separately accepted ADR-115/116: mandatory mutation
  audit atomic/rollback, pure security events best-effort/secret-free fallback,
  mandatory history retained and pure security logs/IP expire operationally
  after 90 days. No app purge/export or unauthenticated actor from email.
- Reports evaluate current states, not past-date reconstructions (ADR-063).
  Period billing sums non-CANCELLED Invoices by issue date; current UNPAID debt
  stays separate (ADR-064). Count distinct students within Course groups and
  across the report, not summed group counts (ADR-065).
- Training progress is confirmed-held LESSON obligations/program count, exams
  separate (ADR-066). Pass rate uses FINALIZED enrollment results by finalization
  date, excluding unfinalized results; an empty cohort is no data (ADR-067).
- Report filters/defaults and detail/export scope are accepted ADR-068–ADR-072:
  current-month periods/all-state defaults, inactive historical course choices,
  date-free current inventory/debt, training/current Teacher and financial Course/
  overdue filters. Director only applicable non-identifying group aggregates/no
  export; Accountant CSV all filtered report results/columns, no hidden/profile
  fields or separate record dumps. Query/snapshot/rate display are accepted
  ADR-094; D10 file mechanisms are accepted in ADR-101: UTF-8 BOM CSV,
  comma/CRLF, 10,000-row or 5 MB cap and formula safeguards. No additional
  staff report rights or implementation authority.
- Profile validation ADR-073–ADR-077: trimmed Unicode name 1–100, real/nonfuture
  Student DOB without age limits, creation email <=254/account-wide case-insensitive
  uniqueness/no provider rewriting, Vietnamese-mobile phone format with equivalent
  0/+84 normalization and uniqueness, optional free-text address <=255. No email
  verification/OTP or new staff required fields; exact Unicode/ASCII email/phone/
  NULL/storage contracts are accepted ADR-091. Other detailed outputs still need review.
- Remaining-field policies ADR-078–ADR-082: trimmed catalogue names 150/component
  names 100; immutable manual Course/Class codes and consistent room codes using
  uppercase 1–30 A–Z/digits/_/-; plain-text reasons/optional comments <=1000;
  Payment references <=100 with no global uniqueness. Existing configuration/
  result locks and one Payment per Invoice stay intact; no room module. Q-011
  operational viewing/UI/history/catalogue lifecycle are accepted ADR-083–ADR-089.
- Owner approved all 33 A–C items in `docs/plan/OWNER_REVIEW.md`, recorded
  ADR-083–ADR-094: scoped read/UI/catalogue workflow, occurrence/final-exam/
  scheduling and data contracts. Target includes EntryTestBooking/AuthSession;
  Owner subsequently approved Gói 1 D01–D04/D06–D07/D10/D15, recorded
  ADR-095–ADR-102 on 29/09/2026: module/server ownership, API/errors/pagination,
  idle/shared-tab and restricted sessions, cookies/CSRF, login privacy/rate limit,
  CSV and future verification with Student-mobile/staff-desktop priorities.
  On 29/09/2026 updated owner checks accepted D05/D08/D09/D11/D14/D16 as
  ADR-114–ADR-119; one explicit reply accepted D17 as ADR-120. Password hashing/
  offline blocklist, audit failures/retention, one-center 10,000-student/50-user
  reference scope, encrypted daily backup (7 daily/4 weekly/monthly restore,
  RPO24h/RTO2h targets), .NET10/EF10/SQL2022 Developer non-production
  (SQL choice subsequently superseded by SQL2025 Express ADR-162) and
  owner as sole default operator are accepted, not implemented/verified.
  D12/D13 are deferred until hosting/software is settled; E01-v1 and E02-v1
  were accepted on 2026-09-30. E03-v1 was accepted on 2026-10-01; E04/E05
  are now accepted as actual E04-v1/E05-v1 (ADR-160/161).
  Gói 2 was
  initially reviewed one part at a time: FE01a shell, FE01b menus/landing and
  FE02 navigation accepted ADR-103–ADR-105. Owner then approved all original
  Gói 2 FE01–FE10; FE03–FE10 are ADR-106–ADR-113: responsive lists, permission
  cues, explicit forms, request states, confirmations, local components,
  Vietnamese formats and visual direction. Preserve prior PII/navigation
  details. Owner explicitly approved Gói 3 FE11–FE19 on 29/09/2026, recorded
  ADR-121–ADR-129: Student home, scoped calendars/weekly preview, per-lesson
  attendance and per-Student draft saves, whole-class grade actions, reports,
  own-Invoice printing, one-time credential handoff and audit details.
  These are accepted design, not E acceptance or implemented behavior.
  On-demand laptop test/demo and
  other-device access is known (SELF_HOSTED_DEMO.md); concrete deployment/
  backup destination/access remains unresolved. Do not re-ask the D17 operator
  unless owner requests a change. No full-baseline, new-package or code approval.
- Accepted rules are in `docs/requirements.md` and `docs/DECISIONS.md`.
  Outstanding proposals remain questions, not implementation authority.
- Missing or contradictory business rules must be recorded in
  `docs/plan/questions.md`; agents must not invent answers.

## Terminology

- **Band**: placement level 1–5 derived from the highest eligible placement
  test score using the `Band.min_score` reference data from the report's design.
- **Slice**: a small vertical capability spanning data, API, UI, and tests.
- **Project Memory**: durable repository documentation shared by agents; it is
  not conversation history.
- **Instruction inbox**: `instructions/`, where the owner places directive and
  reference files for evaluation. Files there are not automatically active.
- **Agent integration skill**: a skill that delegates or coordinates bounded
  work with another coding/planning agent while preserving a repository-level
  handoff.

## Canonical memory and input references

Current actual acceptance03/10/2026: owner accepted E04-v1 (ADR-160) first,
then E05-v1 and complete E01–E04 baseline (ADR-161),without amendments.
E01–E05/stages1–5done;OWNER_REVIEW75rows/73accepted,onlyD12/D13deferred,
notes preserved. Exact presented E04 refinements are accepted design contracts,
not implementation/runtime evidence. DF01–05 remain environment/evidence
prerequisites;no code/SQL/Git/scaffold/deployment/implementationplan authority.
Owner requested implementation-plan discussion03/10/2026; draft at
`docs/plan/implementation-plan.md`, proposed M0–M4 pending discussion.
Detailed proposed M0 working order and diagram are stored in
`docs/plan/M0_QUALITY_PLAN.md` section11; first task=P01 profile/preflight.
Task quality/skill gates apply; no execution started. Do not re-ask approved
policy/outputs. Owner authorized retaining installed SQL if suitable; local
SQL2025 Express17.0.1000.7 is retained by ADR-162 after assessment in
`docs/plan/SQL_INSTANCE_ASSESSMENT.md`. No Developer install prerequisite;
P01 packages/profile, patch review and real integration/recovery evidence remain.
Owner04/10/2026 now requests next-chat handoff to implement first code stage:
P01 prerequisites then P02 backend/test skeleton, scoped in
`docs/plan/FIRST_CODE_HANDOFF.md`. No code started. Worker primary apply remains
owner-gated; later tasks/SQL changes/runtime installs/deployment not authorized.
Earlier planning-only statements describe the preceding stage, not a blocker
to the separately authorized P01→P02 work.

- Product rules/screen catalog: `docs/requirements.md`.
- Accepted decisions including actual E04/E05: `docs/DECISIONS.md` ADR001–162,
  preserve each existing Accepted/Superseded status.
- E01: `docs/design/FUNCTION_SPEC.md`/`FUNCTION_CATALOG.md`,37NV/32F.
- E02: `docs/design/WORKFLOW_SPEC.md`,21W;Accepted30/09/2026.
- E03: `docs/design/DATABASE_SPEC.md`,24tables/230fields/5TG/6VW/6FN/7SP/
  13TX/4roles;Accepted01/10/2026,not deployed schema.
- E04-v1 Accepted03/10/2026: `docs/design/SYSTEM_ARCHITECTURE_SPEC.md`,
  `E04_FINAL_CONTRACT.md`,`E04_CONTRACT_CATALOG.md`,`E04_DTO_SPEC.md`,
  `E04_REPLAY_SPEC.md`,`E04_SQL_INTEGRATION_SPEC.md`,`E04_RUNTIME_PROFILE.md`,
  `E04_PARITY_REVIEW.md`;154API/154DTO/55UUID/6lookups/SC01–04 contracts.
- E05-v1 Accepted03/10/2026: `docs/design/E05_BASELINE_REVIEW.md`,37NV
  individualtrace/stateparity/24cases/findings/exclusions/DF01–05/rubric.
  Source Word/E01/E02/E03/priorADRs/sidework unchanged;no new runtime evidence.
- Official report source and normalized corrections:
  `docs/reference/REPORT_BASELINE.md`.
- Project-local skill store: `.agents/skills/`; contains `backend-clean-code`,
  `frontend-ui`, `code-review`, `codex-with-chatgpt`. Required task selection and
  canonical compatibility overrides: `docs/plan/SKILL_ROUTING.md`. Integration
  skills remain opt-in; Codex retains manager/review authority.
- The older kit under `instructions/english-center-agent-kit/` supplies stack,
  code style, structure, and planning templates where compatible with memory.
  Its requirements and role assignments must not override newer decisions.
- Owner-designated DBMS330284 rubric at
  `instructions/HuongDan_Project_Rubric_DBMS330284.docx (1).pdf` supplies
  database technical and application-integration acceptance criteria for
  E03–E04 (ADR-130). Its separate course-report/submission rules do not
  replace this project's product requirements or accepted business grading.

The owner designated the Word report as the official project foundation and
authorized preserving its normalized content in memory on 27/09/2026. The Word
file is unchanged. Its forms are illustrative; older diagrams/mockups are not
authoritative when they conflict with accepted rules. New instruction inputs
must be reviewed before adoption.
