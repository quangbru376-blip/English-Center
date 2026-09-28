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
  `docs/plan/design-baseline.md`; application implementation has not started.
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
  events. Activity/restricted-session definitions and enforcement await design.
- Audit keeps mandatory business history and adds configuration/class-lifecycle/
  schedule changes plus authentication/account/denied-access events (ADR-058/059).
  Admin has structured filters/newest-first pagination (ADR-060), relevant event
  details without credentials/full payloads (ADR-061), in-app viewing only/no
  audit CSV/PDF export (ADR-062). Storage/failure/retention remain later design.
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
  fields or separate record dumps. Query/export mechanisms remain later design;
  no implementation authority or additional staff report rights.
- Profile validation ADR-073–ADR-077: trimmed Unicode name 1–100, real/nonfuture
  Student DOB without age limits, creation email <=254/account-wide case-insensitive
  uniqueness/no provider rewriting, Vietnamese-mobile phone format with equivalent
  0/+84 normalization and uniqueness, optional free-text address <=255. No email
  verification/OTP or new staff required fields; exact validators/storage remain
  technical design. Other field and operational-view details still need review.
- Remaining-field policies ADR-078–ADR-082: trimmed catalogue names 150/component
  names 100; immutable manual Course/Class codes and consistent room codes using
  uppercase 1–30 A–Z/digits/_/-; plain-text reasons/optional comments <=1000;
  Payment references <=100 with no global uniqueness. Existing configuration/
  result locks and one Payment per Invoice stay intact; no room module. Q-011
  operational viewing/UI/history and catalogue lifecycle remain to review.
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

- Product rules and screen catalog: `docs/requirements.md`.
- Detailed business-function review: `docs/design/FUNCTION_CATALOG.md`;
  outstanding refinements are proposals, not automatically accepted rules.
- Accepted technical/product decisions: `docs/DECISIONS.md`.
- Official report source and normalized corrections:
  `docs/reference/REPORT_BASELINE.md`.
- Project-local skill store: `.agents/skills/`; currently contains
  `codex-with-chatgpt`. The skill is discovered in the current catalog.
- The older kit under `instructions/english-center-agent-kit/` supplies stack,
  code style, structure, and planning templates where compatible with memory.
  Its requirements and role assignments must not override newer decisions.

The owner designated the Word report as the official project foundation and
authorized preserving its normalized content in memory on 27/09/2026. The Word
file is unchanged. Its forms are illustrative; older diagrams/mockups are not
authoritative when they conflict with accepted rules. New instruction inputs
must be reviewed before adoption.
