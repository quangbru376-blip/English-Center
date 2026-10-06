# Project Planning

Latest2026-10-06: owner-approved A06→A07 applied/reverified PASS mained40a4b;1070tests/buildclean,
ADR163 main application/reverification complete. Explicit /me/logout/live session
integration remains unmounted. Next A08→R01, not started; HANDOFF/cards are current.
Earlier A03/A05 and first-code planning below are historical.

Latest2026-10-06: owner-approved A03→A05 reviewed PASS;586unit/HTTP+409SQL=995PASS.
Explicit signup/login integration remains unmounted; production profile/auth resolver
and full readiness deferred. Applied/reverified maina9abdc6 under ADR163; see current
HANDOFF and A03/A05 task cards. Next canonical A06→A07, not opened by this batch.
The preparation and first-code scope below are historical.

This file indexes the current planning stage and later implementation work.
Complete design agreement in `design-baseline.md` is done: E01–E05 accepted,
actual E04-v1 ADR-160 then E05-v1/full baseline ADR-161 on2026-10-03.
Owner requested coding-plan discussion on2026-10-03. Planning is now active
in [implementation-plan.md](implementation-plan.md), with proposed M0–M4
groups pending discussion. Owner chose quality-bounded generation tasks on
2026-10-04, sized by complexity/output and verification, not calendar throughput.
The planning request did not authorize execution. Owner04/10/2026 now asks
for next-chat first code: [FIRST_CODE_HANDOFF](FIRST_CODE_HANDOFF.md), P01
necessary prerequisites → P02 backend/test skeleton only. No code yet;
primary apply/later tasks/SQL/runtime installation/deployment remain gated.

## Preparation

| Item | Status | Notes |
|---|---|---|
| Project Memory bootstrap | done | Repository-level memory created. |
| Official report adoption and six resolutions | done | Canonical `docs/requirements.md`; Word unchanged. |
| Complete detailed report transfer and business agreement | done | E01/requirements/source provenance accepted; Word unchanged. |
| Agree on workflows, database and architecture | done | E02/E03/E04-v1 accepted; actual E04 ADR-160, environment/evidence prerequisites retained. |
| Cross-check and accept the complete design | done | Actual E05-v1/full baseline accepted ADR-161 after E04,03/10/2026. |
| Confirm repository and preserve baseline in Git | done | Owner requested cleanup; accepted docs/local skills/planning preserved in local commits, checkout clean; no push. |
| Scaffold backend and frontend | next (backend only) | P01→P02 authorized for next chat; P03/frontend and remaining tasks outside initial scope. |
| Resolve Slice 0 scope | doing | M0 identity pilot proposed; refine during owner-requested planning, no code. |
| Register agent integration contracts | doing | AGY analyze smoke contract/receipt verified; edit untested. Skill routing required; C2C inactive. |

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

## Working-order source

[M0_QUALITY_PLAN](M0_QUALITY_PLAN.md) section11 is the single detailed source
for proposed39family default order and Mermaid diagram; sections4–7 hold actual
dependencies. Current next task=P01 planning/preflight. M0–M4 remain coverage
proposals; no code/installation/worker edit activated. CURRENT_STATE/TODO/HANDOFF
must reference this order rather than replacing it with a chat-only shorthand.
