# Project Planning

This file indexes the current planning stage and later implementation work.
Complete design agreement in `design-baseline.md` is done: E01–E05 accepted,
actual E04-v1 ADR-160 then E05-v1/full baseline ADR-161 on2026-10-03.
Owner requested coding-plan discussion on2026-10-03. Planning is now active
in [implementation-plan.md](implementation-plan.md), with proposed M0–M4
groups pending discussion. Owner chose quality-bounded generation tasks on
2026-10-04, sized by complexity/output and verification, not calendar throughput.
No application slice/code/SQL/Git/scaffold/
deployment is active or authorized by the planning request.

## Preparation

| Item | Status | Notes |
|---|---|---|
| Project Memory bootstrap | done | Repository-level memory created. |
| Official report adoption and six resolutions | done | Canonical `docs/requirements.md`; Word unchanged. |
| Complete detailed report transfer and business agreement | done | E01/requirements/source provenance accepted; Word unchanged. |
| Agree on workflows, database and architecture | done | E02/E03/E04-v1 accepted; actual E04 ADR-160, environment/evidence prerequisites retained. |
| Cross-check and accept the complete design | done | Actual E05-v1/full baseline accepted ADR-161 after E04,03/10/2026. |
| Confirm repository and initialize Git | deferred | Only with separate owner-authorized scope; no Git action from design acceptance. |
| Scaffold backend and frontend | deferred | Only during authorized implementation. |
| Resolve Slice 0 scope | doing | M0 identity pilot proposed; refine during owner-requested planning, no code. |
| Register agent integration contracts | todo | Add each contract when its skill is supplied. |

## Phase 1–4

The older instruction kit proposes four phases for 33 screens but does not
define their boundaries. Owner has now accepted the complete design and requested implementation
planning. M1–M4 boundaries and an M0 pilot are explicitly proposed in the
new discussion draft; they are not accepted merely because the kit says four phases.

Product plans must now use `docs/requirements.md`, the normalized target model
in `docs/ARCHITECTURE.md`, and accepted ADRs. Old instruction-kit requirements
or Word diagrams must not reinstate removed or superseded behavior. The complete design baseline is accepted. The current stage is planning
discussion; no implementation slice, scaffolding or application coding has
been activated by this request.

## Slice status vocabulary

Use only: `todo`, `doing`, `review`, `done`.
