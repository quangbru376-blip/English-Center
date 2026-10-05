# Implementation planning — discussion draft

Updated2026-10-05: owner-requested D10→D11 complete/reviewed PASS/applied/reverified.
D10 code main18e1edb/482unit+117SQL; finalD11main6aaa4d7/manager01f7fc3,
backend tree999cd7e67d5a145caea9481281544246963df5e3 identical. Locked build clean/
483unit+182SQL=665PASS/0fail0skip. See D10_TASK_CARD/D11_TASK_CARD and evidence.
Owner explicitly approved exact Google Antigravity payloads and isolated SQL
D10/D11 Down→Up/rollback/allocator two-connection scope. Six one-grain views/seven
exact-signature caller-owned procedures;13AGY SUCCESS/1ERROR rejected, zero observed
scope violations; Codex independent corrections/review/tests. Five migrations,
24tables230cols/5TG6FN6VW7SP;47SP parameters/live24SQLbodies verified. Only existing
EnglishCenter_D07_20261005/profile160/exactcollation/snapshotON; fixtures rollback,
database retained/no other DB changes. No grants/real seeds/runtime/business callers/
push/deploy. NextD12 roles/grants not started; D13/restM0 remain later. ADR163 applies
only authorized reviewed PASS stages. Historical initial scope remains below.


## Authority and baseline

Owner explicitly requested discussing the coding plan after E01–E05 acceptance.
The wider milestone proposal remains a planning baseline. Owner04/10/2026
separately requests next-chat first coding stage: P01 prerequisites then P02
backend/test skeleton under [FIRST_CODE_HANDOFF](FIRST_CODE_HANDOFF.md).
Initial scope implemented/reviewed in detached commit d9eb194; see
[P02 task card](P02_TASK_CARD.md) and [P01 manifest](M0_LOCAL_PROFILE.md).
Owner separately confirmed main application as df7cd9a; locked restore/build
clean and ten real startup tests reverified on main. Original tree retained;
later tasks/SQL/runtime installs/deployment remain outside initial scope,
this application permission grants no later task scope. M0/V01 incomplete.
Accepted requirements, E03 database and E04 contracts remain the source of truth;
ADR-160/161 do not need another design vote. Preserve owner D12/D13 deferral.

## Proposed approach

Build small vertical capabilities: database constraints/persistence → service
transaction and authorization → API → Vietnamese UI → verification and demo.
Use conventional code the owner can explain in the oral examination. Preserve
the agreed stack/module boundaries and avoid new package/framework decisions
during task sizing. Do not equate 154 endpoints with equal-sized tasks.

## Owner direction — quality before calendar throughput

On2026-10-04 the owner explicitly chose task decomposition for code quality,
accounting for output-token limits and uneven task complexity. Deadline and
weekly-hours inputs are not prerequisites for this plan. M0–M4 are coverage
and dependency groups, not calendar commitments or single worker prompts.
The operational decomposition below refines that direction; individual contracts
and the proposed execution sequence remain to be made concrete before coding.

### Three levels of work

- Coverage group: M0–M4 tracks which accepted capabilities must eventually exist.
- Integrated slice: one demonstrable real workflow with all applicable guards,
  persistence/API/UI and required failure/concurrency evidence.
- Generation task: one cohesive code change small enough to implement and review
  completely. A slice can require several tasks; an infrastructure task may have
  a build/testable technical outcome rather than a user-facing screen.

Task completion does not imply slice completion. Partial layers must not be
exposed as a finished workflow, and mocks/stubs do not satisfy integration gates.
Keep every accepted atomic business operation all-or-none even when implementation
is produced in several tasks. Split supporting components, not transaction semantics.

### Size tasks by risk and reviewability

Codex inspects the actual context and estimates implementation plus necessary
tests/documentation before dispatch. Reduce scope when a task combines multiple
independent outcomes, introduces too many unfamiliar dependencies, touches shared
infrastructure and domain rules together, or cannot be fully reviewed and verified
within the available output. Prefer one complex transaction per task; a repetitive
low-risk DTO/mapping group may fit together. File count and diff lines are enforceable
bounds, not proof of task quality; no universal files/lines/token quota is assumed.

Reserve output capacity for failure paths, tests and a concise receipt. Code is
written to files rather than duplicated wholesale in chat. Context readings and
output generation limits are distinct: having read a large baseline does not mean
an entire module can be implemented in one response. Do not omit guards, audit,
replay, errors, tests or explanation just to fit a prompt. If scope no longer fits,
stop at a documented incomplete state, review the partial diff and issue a narrower
follow-up; no silent continuation of an unrelated contract or success claim.

### Required skill routing

[Skill policy](SKILL_ROUTING.md) is a required preflight/review gate for every
code-writing task. Task cards record selected complete skill sources, baseline
overrides and actual verification. Backend/UI skills follow the affected scope;
Codex code-review is required before done. Integration protocols do not replace
implementation skills or authorize extra scope.

### Contract for every generation task

Before code: record the task identifier, verified baseline commit and dependencies,
one observable outcome, exact F/W/API/DTO/TX/data references, allowed/forbidden paths,
conservative file/diff limits, required success/failure/boundary checks, manager-run
validation commands, exclusions and integration obligations. No invented scripts:
commands must exist or be explicitly part of the authorized task being added.
Worker contracts are English and use only the declared read context; following a
linked file requires scope/consent assessment, not implicit broader access.

### Quality gates and continuation

1. Ready: accepted source contracts and dependencies available; task fits output
   and review capacity; no unresolved choice hidden behind a placeholder.
2. Generate: implementation and relevant tests in bounded paths. Worker edit tasks
   use clean Git, validated contracts and detached worktrees. Codex retains secrets,
   architectural decisions and final validation. Analyze smoke test is verified;
   real edit execution still needs validation when first used.
3. Verify: Codex inspects receipt, tool events, changed paths/limits and the actual
   diff; runs applicable build/lint/typecheck/tests independently. Use real SQL
   Server for SQL-specific invariants, locks, triggers, grants and transaction races.
   A failed or unavailable required check leaves that outcome unresolved.
4. Review: behavior matches accepted guards/privacy/errors/save units and is simple
   enough for the owner to explain. Tests cover meaningful edge/failure conditions,
   not just the implementation's happy path. Review dependent callers/interfaces.
5. Record: update the task/slice status, commands/results and remaining integration
   obligations in memory. Next dependent generation starts only on a verified
   baseline; do not accumulate unreviewed dependent diffs. Small independent work
   does not authorize overlapping concurrent worker edits.

Worker changes remain isolated until owner permission to apply to primary. Testing
an approved dependency chain within the same isolated worktree does not itself
approve applying it. No merge/apply permission inferred from the quality principle.

### Illustrative M0 decomposition — proposal, not executable contracts

| Task family | Bounded outcome | Required evidence before dependent work |
| --- | --- | --- |
| Development profile and dependency pins | Concrete compatible development/test configuration and prerequisites | Actual tool/profile checks; do not invent D12/D13 values. |
| Backend skeleton | Agreed backend/test layout builds | Build/test project discovery and a meaningful startup check. |
| Frontend skeleton | Agreed frontend layout and shared HTTP/error boundary build | Existing build/lint/typecheck targets execute; no business success mocks. |
| Identity persistence | A coherent E03 identity/schema/bootstrap foundation | Real SQL migration/constraints and safe bootstrap checks; cross-domain schema dependencies explicit. |
| Shared representations | Exact normalization/time/money/RowVersion contracts, split by complexity | Accepted boundary examples and meaningful invalid-input tests. |
| Student signup | Atomic self-registration service/API | Uniqueness races, rollback, privacy and accepted validation. |
| Session/auth operations | Login/context/logout and session checks, split into bounded operations | Real revocation/timers/CSRF/role checks per operation; no token persistence. |
| Identity UI integration | Real signup/login/own-context/logout flow in small UI tasks | Backend-connected states/permissions and required responsive/browser checks. |
| Restricted password flow | Actual temporary-password lifecycle and relevant UI | One-time secret handling, expiration and all-session invalidation. |
| Audit/replay foundation and first caller | Exact shared contracts then one real authorized Admin command | Atomic completion/audit, current rights, duplicate/conflicting intent and lost-response handling. |

This table is a sizing illustration, not an instruction to generate a whole row at
once. Codex must split further if dependency, error or concurrency complexity warrants.

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

## Next refinement

Detailed quality-based [M0 task plan](M0_QUALITY_PLAN.md) now lists prerequisites,
source trace, small task families/dependencies, verification and proposed scopes
for the first three tasks. Review ordering and materialize each English contract
only after actual prerequisites and file targets are known. Full schema/object
readiness is not replaced by partial-table checks. Existing local SQL2025 Express
is retained by ADR-162 after [local assessment](SQL_INSTANCE_ASSESSMENT.md).
No Developer installation prerequisite; P01 package/profile gates remain.
No deadline input required; no source generation/installation/worker dispatch
from this planning output. M0–M4 and detailed task ordering remain proposals.

## Persisted working order

M0's detailed proposed default sequence and Mermaid live in
[M0_QUALITY_PLAN section11](M0_QUALITY_PLAN.md#11-thứ-tự-mặc-định-đã-lưu-và-sơ-đồ).
Next=P01planning/preflight; schedule is not a prerequisite. Detailed dependency
corrections precede execution. M1–M4 remain coverage proposals pending their own
small task plans. No plan-source disagreement should be resolved from chat alone.
