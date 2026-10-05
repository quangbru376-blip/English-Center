# English Center Management System — Project Memory

This file is the entry point for agents working in this workspace. Before any
non-trivial task, read these files in order:

1. `docs/PROJECT_CONTEXT.md`
2. `docs/ARCHITECTURE.md`
3. `docs/CURRENT_STATE.md`
4. `docs/DECISIONS.md`
5. `docs/TODO.md`

Then read the applicable execution plan in `docs/plan/` and only the
instruction files or skills relevant to the task.

For product planning, implementation, and review, also read
`docs/requirements.md`. It is the canonical product specification, normalized
from the official report and the owner's accepted decisions. Source provenance
is in `docs/reference/REPORT_BASELINE.md`. Do not modify the source Word report.

The full design baseline E01–E05 is Accepted (ADR-160/161), tracked in
`docs/plan/design-baseline.md`. The current stage is quality-based implementation
planning in `docs/plan/implementation-plan.md` and `docs/plan/M0_QUALITY_PLAN.md`.
P02/P03/P05/P04/P06 remain applied PASS. On2026-10-04 the owner requested
three next families P07→P08→D01 in one turn; this batch is complete/reviewed
PASS/applied/reverified, final code mainf6fd902 full400tests PASS. See
CURRENT_STATE and docs/plan/P07_TASK_CARD.md, P08_TASK_CARD.md, D01_TASK_CARD.md.
P07 supplies safe error/binding primitives; P08 pinned offline/raw credential
helpers; D01 four-table EF mapping without SQL execution or migrations. One
D01 AGY overread run rejected; Codex independently completed the final mappings.
On2026-10-05 D02 continuation completed/reviewed PASS/applied/reverified main5a4ba3d:
Course/Band/TrainingProgram/ScoreComponentConfig mapping, full8tables/56columns,
410tests PASS/build0warnings0errors. Four AGY runs had zero observed scope violations;
Codex fixed/reviewed and independently tested. See docs/plan/D02_TASK_CARD.md.
D03 continuation2026-10-05 completed/reviewed PASS/applied/reverified main244353f:
Registration/EntryTestSession/EntryTestAttempt/EntryTestBooking/BandAssignment,
full13tables/116columns/2AK/23NO ACTION FK/37indexes/34checks/12RV;
427tests PASS/build0warnings0errors. Three AGY runs had zero observed violations;
Codex independently tested and reviewed. See docs/plan/D03_TASK_CARD.md.
FK indexes are explicit under ADR-164; full lookup coverage verified.
D04 continuation2026-10-05 completed/reviewed PASS/applied/reverified main e741176:
Class/StudentClassAssignment/Schedule mapping, full16tables/151columns/33FK/
51indexes/46checks/14RV;440tests PASS/build0warnings0errors. Three AGY runs had
zero observed scope violations; Codex independently tested/fixed/reviewed.
See docs/plan/D04_TASK_CARD.md.
D05 continuation2026-10-05 completed/reviewed PASS/applied/reverified main9e843b8:
Attendance/Score/GradeReport mapping, full19tables/180columns/43FK/62indexes/
52checks/17RV;454tests PASS/build0warnings0errors. Three AGY runs had zero
observed scope violations; Codex independently tested/fixed/reviewed.
See docs/plan/D05_TASK_CARD.md.
D06 continuation2026-10-05 completed/reviewed PASS/applied/reverified main543fcf0:
Invoice/Payment/AuditLog/ReplayOperation/InvoiceNumberCounter mapping,
complete24tables/230columns/50FK/78indexes/70checks/20RV;474tests PASS/build clean.
Three bounded AGY runs stayed within observed scopes; Codex independent tests/
fixes/review. See docs/plan/D06_TASK_CARD.md.
D07→D08→D09 owner-requested batch2026-10-05 complete/reviewed PASS/applied/
reverified code maina7561dd:481unit+102SQL PASS/build0warnings0errors. D07 adds
private Design/localdotnet-ef10.0.4 from cache/disconnected160factory/InitialSchema;
owner explicitly approved isolated SQL new EnglishCenter_D07_20261005 only,
fail-if-exists/profile160/collation/snapshotON/migration+object rollback fixtures.
Up→Down→Up/schema PASS:24tables230cols50FK78IX70CHECK20RV; all230columns verified.
D08 exact5set-based preheld-gate guards, D09 six pure exact-signature UDFs;
11bounded AGY SUCCESS/zero observed violations, Codex independent fixes/review.
Three migrations; fixtures rolled back, database retained/no DROP DB. See D07/
D08/D09_TASK_CARD and evidence. D10/D11 subsequently owner-authorized2026-10-05,
complete/reviewed PASS/applied/reverified main6aaa4d7:483unit+182SQL PASS/buildclean;
6views/7SP/5migrations, exact47params/live24module bodies. Exact AGY payload +
existing isolated SQL scope explicitly approved;13SUCCESS/1ERROR rejected, no
observed scope violations. See D10/D11_TASK_CARD/evidence. NextD12 not started;
no roles/grants/runtime/other DB/restM0/push/deploy authority from this batch.
Historical initial scope is retained in
FIRST_CODE_HANDOFF; latest checkpoint and explicit current owner scope govern.
Standing owner authority ADR-163: apply Codex-reviewed PASS stages already
authorized for implementation to main without asking again. Report issues
requiring owner action. Keep unreviewed/rejected worker output detached.
This authority does not open unrelated tasks, SQL/migrations, installation,
push or deployment. Original reviewed/rejected trees are retained as evidence.
Project-local skills live in `.agents/skills/`. Read the selected skill's
complete `SKILL.md` before using it. Installing a skill does not automatically
start its connection or delegation workflow.

Before every code-writing task, follow `docs/plan/SKILL_ROUTING.md`. Select and
read the applicable complete skills: backend-clean-code for backend/DTO/API/SQL
work, frontend-ui for frontend/UI work, both for changes spanning both scopes.
Codex uses code-review before code tasks or integrated slices are marked done.
Record selected skills, source versions, canonical overrides and verification
in the task card. Worker contracts must explicitly include the selected skill
files/policy in authorized read context and require their use; skills grant no
additional write/command/network/merge authority. Codex verifies actual behavior.
Accepted E03/E04/requirements override stale skill guidance, including JWT-cookie
transport, SQL Server-specific tests, full-set DTOs and exact money representations.
Use antigravity-worker only for bounded AGY delegation, with Codex as manager.
codex-with-chatgpt remains opt-in; reviewing a skill never starts its integration.

`instructions/` is an input inbox supplied by the owner. It is not the source
tree, Project Memory, or an automatically active rule set. An instruction file
becomes authoritative only when the owner says so or Project Memory explicitly
adopts it. Currently adopted English Center inputs are listed in
`docs/PROJECT_CONTEXT.md`.

Older instruction-kit requirements, diagrams, and rules are references only
where compatible with canonical memory. Do not reintroduce scholarships,
ten-point grading, retakes after confirmation, or automated debt penalties.
The report's illustrative forms are not a requirements source. Unresolved
business rules are tracked in `docs/plan/questions.md`, not silently guessed.

Multiple skills and agent integrations may be used together. Follow each
selected skill's scope and handoff protocol; do not assume one integration
owns the whole project. Durable results and handoff state must return to
Project Memory or the applicable execution plan.

The implementation is the strongest evidence of current behavior. If code and
documentation disagree, verify the behavior and update stale documentation;
do not silently reverse an accepted decision.

After meaningful work, update only the memory affected by the change:

- `CURRENT_STATE.md` for implementation status, blockers, and the next action.
- `TODO.md` for short-term work and priority changes.
- `DECISIONS.md` for significant technical or contract decisions.
- `ARCHITECTURE.md` when the implemented architecture changes.
- The active file in `docs/plan/` when a multi-step task advances or changes.

Keep memory concise, current, and actionable. Never store chat transcripts,
hidden reasoning, secrets, temporary logs, or facts that have not been
verified.
