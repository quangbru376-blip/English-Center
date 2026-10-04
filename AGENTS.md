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
On 2026-10-04 the owner requested a handoff for the next chat to begin the
first coding stage: complete P01 prerequisites then implement/review P02
backend/test skeleton under docs/plan/FIRST_CODE_HANDOFF.md. P01 P02 prerequisites
and reviewed P02 PASS are now recorded in docs/plan/P02_TASK_CARD.md. The owner
separately confirmed main application; P02 is applied and reverified on main
at df7cd9a, with the original detached worktree retained for evidence.
P03 is applied/reverified PASS at 6cb2590; P05 at 8784686, with31time
plus10startup tests passing; see docs/plan/P03_TASK_CARD.md and P05_TASK_CARD.md.
Standing owner authorization (ADR-163): apply reviewed PASS stages already
authorized for implementation to main without asking again. Report issues
requiring owner action. P04 applied/reverified PASS at d9840bd with120tests passing; see
 docs/plan/P04_TASK_CARD.md. P04 complete; P06 is not started. This permission
does not open unrelated tasks, SQL/migrations, installs, push or deployment.

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
