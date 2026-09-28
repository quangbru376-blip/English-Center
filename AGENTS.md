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

The current stage is full design agreement, tracked in
`docs/plan/design-baseline.md`: business functions, workflows, database, then
system architecture. Agree on the complete baseline with the owner before
discussing the implementation plan. Do not start Slice 0, Git initialization,
scaffolding or application coding during this stage.

Project-local skills live in `.agents/skills/`. Read the selected skill's
complete `SKILL.md` before using it. Installing a skill does not automatically
start its connection or delegation workflow.

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
