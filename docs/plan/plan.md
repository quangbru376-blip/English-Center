# Project Planning

Owner-authorized U05→U06 complete/reviewed PASS/applied ADR163/reverified2026-10-08.
Code mainf83a7d6 / manager0ff2ae7; identical frontend4050338d256ad268fe11cc1de7180e39f08a1458.
48frontend tests/0fail0skip, typecheck/lint/build and Edge375/768/1280 PASS on both.
Two bounded AGY SUCCESS/full3contexts before sole writes/zero observed violations;
Codex independent integration/fixes/review; originals and receipt/event hashes retained.
U05 exact raw password/RV/local-only confirmation, normal/restricted scope,204→login;
no auto-login/upgrade/retry/storage;400/403/409/500/lost/dirty navigation verified.
U06 explicit Admin NORMAL detail/lock pilot, frozen target/RV/UUID after confirmation,
current response and manual safe lookup;404 is not rollback, no polling/write replay.
All6other roles/restricted Admin blocked; stale identity response clears preview.
Default App/main unmounted; backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a unchanged.
SQL not rerun; real cookie/HTTPS/profile/keys/principals/runtime proof unverified.
Four owner files unchanged/unshared/untracked/unstaged. See U05/U06 cards/evidence,
notes and ADR176. Next V01 unstarted; no restM0/runtime/SQL/packages/push/deploy scope.

Previous dated checkpoints below are historical.

Owner-authorized U03→U04 completed/reviewed PASS/applied ADR163/reverified2026-10-07.
Code maind51564e / managerd18a34a; identical frontend851877ef7396885e51d39518697659277fa0effc.
38frontend tests/0fail0skip, typecheck/lint/build and Edge375/768/1280 PASS on both.
Two bounded AGY SUCCESS/full3contexts before write/zero observed violations;
Codex independently integrated/fixed/tested/reviewed; original trees retained.
U03 exact signup201→login/no auto-login; raw credentials and unknown recovery.
U04 explicit IdentityApp login/me/logout, normal/restricted gates, server-interval
read-only deadline checks and literal-only cross-tab revalidation without echo.
Default App/main stay unavailable; no mounted runtime, production cookie or SQL claim.
Backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a unchanged; SQL not rerun.
Four owner files unchanged/unshared/untracked/unstaged. See U03/U04 cards/evidence,
notes and ADR175. Next U05→U06 unstarted; no restM0/runtime/SQL/packages/push/deploy
scope opened; actual production keys/profile/principals remain operator prerequisites.

Earlier checkpoints below are historical; current next scope is above.

Latest2026-10-06: R02→R03 reviewed PASS;1386tests/buildclean, applied/reverified mainfcdd21e under ADR163. Explicit Admin detail/lock/lookup and completion protocol;
unmounted/rollback-only SQL. Next U02→U01 unstarted. See R02/R03 cards/evidence.

Latest2026-10-06: A08→R01 owner-approved scope reviewed PASS;1195tests/buildclean.
Applied/reverified main3b46460 under ADR163; exact password change and disconnected
registry55/C1, no R02/R03/runtime/storage activation. See cards/evidence.

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
