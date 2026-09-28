# TODO

## Active

- Full design agreement with the owner, tracked in
  `docs/plan/design-baseline.md`. Begin with the complete business function
  catalog; the current normalized report baseline is not yet a complete
  detailed implementation specification.

## Next

- Complete functions, actor permissions, inputs/outputs and validations from
  the official report, excluding illustrative forms.
- Review `docs/design/FUNCTION_CATALOG.md` (39 source references, NV10/NV11
  removed). Resume using `docs/plan/HANDOFF.md` and the owner's single approval
  file `docs/plan/OWNER_REVIEW.md`: 50 design choices and 5 gated output reviews,
  all initially unchecked. Read owner marks/alternatives and record only accepted
  choices; no more sequential five-question batches. A01–A05 hold the previous
  Q-011 proposals; the table also covers catalogue/workflow/database/architecture
  gaps. Complete actual outputs before requesting E01–E05 acceptance.
  Remaining-field policies are accepted
  ADR-078–ADR-082; profile validation accepted
  ADR-073–ADR-077; required/editable fields remain ADR-019. Report defaults/filters/Director
  group-only detail/Accountant all-filtered-results CSV are accepted ADR-068–072;
  metric bases/current-state scope ADR-063–067 stay fixed. Then remaining field/
  Q-011 operational details; stage 1 is not yet complete.
  Audit coverage/security events/Admin filters/safe details/no audit export are
  accepted ADR-058–ADR-062, not storage/failure/retention or code approval.
  Password policy/
  temporary 24-hour expiry/normal 2-hour or 30-minute-idle expiry/concurrent devices/
  current-session logout are accepted ADR-053–ADR-057; do not re-ask.
  Delivery/mandatory change/revocation/reactivation/email/throttling are accepted
  (ADR-048–ADR-052), no code. Money/deadline/business
  timezone rules are accepted (ADR-043–ADR-047), not schema/code approval.
  Course immutability/current-profile display/no transfers/Student browser
  Invoice printing/Accountant metadata-only Payment correction are accepted
  (ADR-038–ADR-042); do not re-ask. Extra
  LESSONs beyond the program count are excluded (ADR-037). Pre-start move/post-start linked
  replacement and completion/counting policy are accepted (ADR-036), leaving
  storage/history/audit design. ADR-035 resolves scope within NV19/F21, no
  holiday module/automation/individual makeup. Mandatory whole-class grade submission/
  resubmission, return and finalization are accepted (ADR-034).
  Attendance entry/correction opens at LESSON start
  and closes before Class COMPLETED, no fixed per-lesson deadline
  (ADR-033). Expected lesson counts/full schedules and
  date/time/room edit cutoffs are accepted (ADR-032); then remaining action/report
  refinements. Entry-score correction and ties are accepted (ADR-029;
  Q-006 business policies resolved). Configuration freezes and
  precision/rounding are accepted (ADR-028). Test absence attribution
  and center cancellation/rescheduling are accepted (ADR-027). Class
  cancellation is accepted (ADR-026); dependent-booking cancellation/ended
  history rules are accepted (ADR-031). Q-007 retains data/audit representation.
  Class opening/start/completion are accepted (ADR-025); Q-008
  retains actual-session occurrence representation. Grade submission/
  finalization, edit permissions and passing are accepted (ADR-024, Q-009 resolved).
  Placement/teacher changes
  are accepted (ADR-023).
  Enrollment cancellation/completion are
  accepted (ADR-022); Q-005 retains database enforcement. Test-booking rules
  are accepted (ADR-021); session opening/closure and grader replacement are
  accepted (ADR-030). Q-004 retains history/audit representation and effective-
  closure persistence. Detailed security storage/enforcement still remains open.
  Q-005/Q-007/Q-011/Q-012 track remaining lifecycle/cancellation/detail gaps;
  the draft does not mark stage 1 complete.
- Agree on workflows, state transitions, failure/cancellation cases and the
  business questions in `docs/plan/questions.md`.
- Finalize the database data dictionary, relationships, constraints and
  history rules against the agreed workflows.
- Finalize system architecture, module boundaries, security, interface
  conventions and runtime/deployment assumptions.
- Cross-check the complete design and obtain owner acceptance; only then
  discuss implementation phases, slices, Git setup and scaffolding.
- As each additional skill or coding-agent integration is supplied, review it
  and record its scope, ownership boundary, handoff, and verification method.
- Set up/verify a skill connection only when its workflow is invoked; local
  `codex-with-chatgpt` discovery is already confirmed.

## Blocked

- Architecture and implementation progress cannot be documented beyond target
  constraints because no source code exists.
- The four-phase implementation breakdown cannot be recorded because it is
  mentioned but not defined in the available instructions.
- Q-004–Q-010 identify undefined product details; do not implement their answers
  as assumed rules. Independent design analysis may continue, but the complete
  baseline cannot be accepted while in-scope business rules remain unresolved.

## Completed

- 2026-09-28: Consolidated known remaining design decisions into the owner's
  four-column `docs/plan/OWNER_REVIEW.md` (55 unchecked items), with gated final
  document acceptance and no re-asking ADR-001–ADR-082. Linked affected memory/
  handoff; creation of the file accepts no new rules or stage/code work.
- 2026-09-28: Accepted remaining-field policies ADR-078–ADR-082 and synced
  affected design memory; Q-011 viewing/UI/history proposals remain unaccepted.
  No code/schema or stage completion.
- 2026-09-28: Accepted name/DOB/email/phone/address validation (ADR-073–ADR-077).
  Synced affected memory. Estimated remaining discussion by design stage in
  the active plan; estimate is not owner acceptance or a completion guarantee.
  No code/schema or stage completion.
- 2026-09-28: Accepted report defaults/filters, Director non-identifying group-only
  detail and Accountant all-filtered-results/report-columns CSV (ADR-068–ADR-072).
  Synced affected memory/handoff; prepared Q-010 profile-validation proposals,
  subsequently accepted in ADR-073–ADR-077. No code/schema or stage completion.
- 2026-09-28: Accepted current-state/no historical as-of reporting, period billing/
  current debt separation, distinct students, LESSON-only progress and finalized-
  result pass-rate cohort (ADR-063–ADR-067). Synced affected design memory and
  prepared unaccepted filter/visibility/export proposals; no code/schema.
- 2026-09-28: Accepted business/security audit coverage, Admin structured filters/
  newest-first pagination, relevant secret-free details and no audit CSV/PDF export
  (ADR-058–ADR-062). Synced design memory/handoff; prepared unaccepted Q-012
  metric proposals. No code/schema or stage completion.
- 2026-09-28: Accepted 15–128-character passphrase-friendly passwords, temporary
  24-hour expiry, normal two-hour absolute/thirty-minute idle sessions, concurrent
  devices and server-side current-session logout/no logout-all (ADR-053–ADR-057).
  Updated affected design documents/handoff; no code/schema or stage completion.
- 2026-09-28: Accepted temporary-password delivery/forced change, all-session
  invalidation on lock/disable/reset/password change, same-account DISABLED
  reactivation, immutable login email and timed login throttling (ADR-048–ADR-052).
  Updated affected specification/memory/handoff; no code/schema.
- 2026-09-27: Accepted positive tuition/no free-course path, VND whole dong,
  due date local issue date +7 calendar days, confirmation time only/no separate
  receipt field, UTC+7 business dates/schedules/report filters (ADR-043–ADR-047).
  Synchronized affected memory; no code/schema added.
- 2026-09-27: Saved the owner's five accepted decisions (ADR-038–ADR-042):
  immutable Registration Course, no personal snapshots, no class transfers,
  Student-owned browser Invoice printing and audited Accountant Payment
  method/reference correction only. Synced affected design memory; no code.
- 2026-09-27: Excluded extra LESSON teaching obligations beyond the fixed program
  count (ADR-037). Weekly/individual creation fills required count; replacement
  remains one obligation despite retained history rows; FINAL_EXAM separate.
- 2026-09-27: Accepted combined future LESSON moves/past non-held LESSON linked
  replacements by AcademicAffairs/reason/history, no dependent data/non-ended
  class/current teacher/conflict checks. Replacement fulfills one obligation;
  completion requires its actual occurrence/attendance, not fake old attendance
  (ADR-036). Data representation remains open; extra LESSONs were later excluded
  by ADR-037.
- 2026-09-27: Accepted non-held class-wide lesson handling within NV19/F21,
  no holiday module/center-wide calendar/automatic holiday or makeup scheduling/
  individual makeup (ADR-035). Non-held lessons are not delivered or marked
  all absent to bypass completion; concrete operations later resolved ADR-036.
- 2026-09-27: Accepted mandatory whole-class grade submission/resubmission,
  reasoned return and finalization/publication, with all-or-none transitions
  and all students' required scores before sending (ADR-034). Kept individual
  draft/correction entry, resubmission and existing edit locks; no code/schema.
- 2026-09-27: Accepted attendance entry/correction until before Class COMPLETED,
  no fixed 24/48-hour per-lesson deadline, then read-only regardless of grade
  state (ADR-033). Opening boundary also accepted at LESSON start, not before
  or only after lesson end; the whole attendance window is resolved.
- 2026-09-27: Accepted program-level positive LESSON counts and first-use freeze,
  full LESSON schedules before teaching start, and pre-start/no-dependent-data
  schedule date/time/room edits (ADR-032). Attendance closing deadline was
  subsequently resolved by ADR-033, including the LESSON-start opening boundary.
- 2026-09-27: Accepted future booking cancellation with Registration (including
  CLOSED), started-test history/result locks, new-record testing and atomic
  cancellation/audit rules (ADR-031); Q-007 business policies resolved.
- 2026-09-27: Accepted session CLOSED creation/opening/closure/reopening,
  CLOSED student-booking locks and audited pre-start/pre-result grader
  replacement (ADR-030); synchronized next-chat handoff.
- 2026-09-27: Accepted reasoned/audited entry-score corrections by original/
  current grader before confirmation, post-confirmation/ended-record locks
  and equal-best ties favoring attempt one (ADR-029); wrote next-chat handoff.
- 2026-09-27: Accepted first-use grading-configuration/Band freezes and
  two-decimal inputs/half-up final rounding before passing comparison (ADR-028).
- 2026-09-27: Accepted assigned-grader post-session absence and pre-start center
  cancellation/rescheduling cutoffs (ADR-027), with source provenance recorded.
- 2026-09-27: Accepted empty pre-teaching class cancellation with mandatory
  reason, preserved history and cancelled-schedule resource release (ADR-026).
- 2026-09-27: Accepted explicit class opening/start/completion prerequisites
  (ADR-025), leaving occurrence representation/full-schedule/cancellation gaps.
- 2026-09-27: Accepted COMPLETED-only grade submission/finalization, edit locks
  and finalized >= 50/100 passing (ADR-024); kept rounding/report-cohort gaps.
- 2026-09-27: Accepted OPEN-only placement, retained ADMITTED/derived placement
  display and pre-teaching teacher changes with schedule checks (ADR-023).
- 2026-09-27: Accepted enrollment cancellation/unpaid-invoice coupling and
  finished-record/re-enrollment criteria (ADR-022), preserving history.
- 2026-09-27: Accepted test booking cancellation/change, absence rebooking and
  pending-retake confirmation guards (ADR-021), preserving schema questions.
- 2026-09-27: Accepted own-password change, Admin self/last-active protection
  and account-state meanings (ADR-020); delivery/revocation/reactivation/email/
  throttling subsequently resolved by ADR-048–ADR-052, password/session policy
  by ADR-053–ADR-057; detailed security enforcement remains open.
- 2026-09-27: Accepted email/password login and required/editable student fields
  (ADR-019); remaining validation/account-operation questions preserved.
- 2026-09-27: Accepted recruitment, finance and training report scope/bases
  (ADR-018), retaining remaining report details as questions.
- 2026-09-27: Accepted enrollment uniqueness/repeat-course rules, own-profile
  editing/confirmed course lock and reasoned rejection (ADR-015–ADR-017).
- 2026-09-27: Removed Admissions registration-on-behalf functions and F15;
  accepted CSV exports and weekly/individual schedule entry (ADR-012–ADR-014).
- 2026-09-26: Bootstrapped repository-level Project Memory from verified
  workspace evidence.
- 2026-09-26: Audited the available instruction kit and downloaded C2C skill.
- 2026-09-26: Clarified the instruction-inbox and composable-skill model.
- 2026-09-26: Copied codex-with-chatgpt into `.agents/skills/`.
- 2026-09-27: Adopted the official report and recorded six owner resolutions
  into canonical memory without changing Word; local skill discovery confirmed.
