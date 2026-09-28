# Project Planning

This file indexes the current planning stage and later implementation work.
The active stage is full design agreement in `design-baseline.md`. Agree on
business functions, workflows, database and system architecture before
discussing implementation planning. No application slice is active or authorized.

## Preparation

| Item | Status | Notes |
|---|---|---|
| Project Memory bootstrap | done | Repository-level memory created. |
| Official report adoption and six resolutions | done | Canonical `docs/requirements.md`; Word unchanged. |
| Complete detailed report transfer and business agreement | doing | See `design-baseline.md`; the saved baseline is not yet a complete detailed specification. |
| Agree on workflows, database and architecture | todo | Resolve in-scope open business rules before baseline acceptance. |
| Cross-check and accept the complete design | todo | Owner acceptance precedes implementation planning. |
| Confirm repository and initialize Git | deferred | Only during authorized implementation; workspace is not a Git repository. |
| Scaffold backend and frontend | deferred | Only during authorized implementation. |
| Resolve Slice 0 scope | deferred | Discuss after complete design acceptance; see `questions.md`. |
| Register agent integration contracts | todo | Add each contract when its skill is supplied. |

## Phase 1–4

The older instruction kit proposes four phases for 33 screens but does not
define their boundaries. Phase grouping and slice plans are deferred until
the owner accepts the complete design and moves to implementation planning.
Do not invent or activate phases during the current design discussion.

Product plans must now use `docs/requirements.md`, the normalized target model
in `docs/ARCHITECTURE.md`, and accepted ADRs. Old instruction-kit requirements
or Word diagrams must not reinstate removed or superseded behavior. Independent
design work may continue while questions are open, but the complete baseline
requires agreement on all in-scope product rules. No implementation slice,
scaffolding or application coding is authorized during this stage.

## Slice status vocabulary

Use only: `todo`, `doing`, `review`, `done`.
