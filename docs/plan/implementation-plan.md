# Implementation planning — discussion draft

Updated: 2026-10-03. Status: doing (planning only).

## Authority and baseline

Owner explicitly requested discussing the coding plan after E01–E05 acceptance.
This authorizes planning; the milestone proposal below is not yet accepted and
no code/SQL/Git/scaffold/package installation or deployment has begun.
Accepted requirements, E03 database and E04 contracts remain the source of truth;
ADR-160/161 do not need another design vote. Preserve owner D12/D13 deferral.

## Proposed approach

Build small vertical capabilities: database constraints/persistence → service
transaction and authorization → API → Vietnamese UI → verification and demo.
Use conventional code the owner can explain in the oral examination. Preserve
the agreed stack/module boundaries and avoid new package/framework decisions
inside a schedule discussion. Do not equate 154 endpoints with equal-sized tasks.

## Proposed milestones — not an approved execution sequence

| Milestone | Reviewable outcome | Important verification |
| --- | --- | --- |
| M0 — Foundation and identity pilot | Reproducible development setup, agreed schema/migration strategy, seed/bootstrap strategy, common DTO/error/time/money/version conventions, real Student self-registration/login/own context/logout and restricted-password flow; staff/Admin bootstrap strategy explicit. Shared command/replay/audit infrastructure exercised by an actual authorized account command before business UUID commands. | Real SQL Server constraints and trigger/EF behavior for affected tables; session revocation, CSRF, ownership, invalid inputs; no secret replay. Bootstrap is not an Admissions create-Student feature. |
| M1 — Configuration and admissions | Staff account administration needed for actual roles; Course/Band/Program prerequisites; entry sessions; Student Registration/booking; Teacher grading/absence/correction; Band and Student confirmation; Admissions approve/reject/cancel. Approval includes atomic Invoice issuance even before Accountant UI is built. | Last-place and first-use races, two-attempt limits, live time guards, configuration freeze, scope, cancellation dependencies, audit and replay. |
| M2 — Payment and class preparation | Accountant confirmation/correction; Student own Invoice/print; Class lifecycle preparation, placement, concrete/weekly schedules and move/replacement; calendars scoped by role. | Payment-versus-cancellation, duplicate payment, capacity/overlap across tests and lessons, full-batch rollback, current assignment and replay rights. |
| M3 — Teaching and publication | Start/HELD/completion, per-LESSON Attendance, per-Student scores/comment, whole-Class submit/return/resubmit/finalize and Student published results. | Missing versus zero, current obligation chains, grade precision/rounding, whole-cohort atomicity, stale form and completion races, no draft leaks. |
| M4 — Reports and final evidence | Role-scoped reports, bounded CSV, complete audit viewing; close remaining 32F/21W/154API/55commands/6lookups obligations, SQL rubric, responsive/browser/demo and recovery evidence for approved environment. | Snapshot totals/filters, CSV bytes/footer/formula protection, 7-role privacy, all required SQL objects/callers/grants, cross-contract E05 cases, reproducible setup and restore. |

M1–M4 are proposed four implementation phases, not boundaries inherited from
an older kit. Each must be broken into smaller reviewable slices before execution.
Audit writers and security/account controls are implemented with their callers;
M4 completes viewing/reporting/evidence rather than postponing integrity/security.
Each SQL object is introduced with its actual dependent flow. Preserve mandatory
objects and full final schema; decide migration granularity in planning, not by
silently reducing E03. M0 checks affected constraints first, not all future flows.

## Definition of done for each slice

- Trace accepted F/W/API/DTO/TX/data/verification obligations and remaining gaps.
- Use real database/API/UI; no mock-only success for a business transition.
- Verify scope, guards and atomic audit/replay when applicable, stale/conflicting
  writes and meaningful failures; run relevant xUnit/SQL integration checks.
- Demo the flow and preserve evidence; update affected Project Memory.
- Later review does not waive invariants or redefine business rules to pass tests.

## Environment and deferred work

D12 performance targets and D13 concrete hosting/exposure remain deferred.
Agree a runnable development/test profile before execution requiring it; accepted
Secure/same-origin/CSRF and fail-closed rules still apply. Planning may identify
required fields without assigning an unapproved port/provider/budget. Patch pins,
blocklist provenance/license, SQL compatibility, database principals/bootstrap
and test isolation must be resolved for the affected execution slice. Backup
media/keys/isolated restore are prerequisites for actual backup/recovery evidence.
Do not claim load/RPO/RTO success without the approved environment and measurements.

## Discussion inputs and next refinement

Owner deadline/demo date and available hours per week requested; not known yet.
Use those inputs to discuss milestone sizing, dependencies, owner review cadence
and a detailed M0 slice contract. Proposed sequence remains subject to discussion;
no dates, delivery guarantee, implementation authorization or new ADR inferred.
