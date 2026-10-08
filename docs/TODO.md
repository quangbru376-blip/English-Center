# TODO

## Current status

- Complete design baseline Accepted03/10/2026: E04-v1 ADR-160 first,then
  E05-v1 ADR-161. E01–E05 done;75ownerrows/73accepted,only D12/D13 deferred.
- Actual E04 package/exact technical refinements and E05 cross-check accepted;
  no further baseline approval pending,no new product question. RF/FE/operator
  decisions preserved. Source Word/report/sequence sidework unchanged.

## Next

- Finish owner-assigned M0; T maintenance SQL effects/authority applied/reverified PASS (1197default+5SQL each, buildclean); Q/S PASS and R catalog feasibility880/880 retained.
- Select concrete M0_LOCAL_DEV_PROPOSAL actual values already asked; do not infer fixture
  values or re-ask accepted decisions. Resolve actual runtime identity/approved metadata bindings and PKI,
  then actual operator bindings/startup integration and scheduler verification. T closes bounded SQL cleanup effects, not real maintenance-login/TLS proof. M0 incomplete.
- See M0_COMPLETION_TASK_CARD/M0_ENVIRONMENT_INPUTS; preserve owner4files and D07.

Historical H/I next:

- V01-H/I applied/reverified H7df8ba8/Id59e905:45+33new/1061defaultPASS+3readonlySQL
  PASS,58opt-in SKIP/buildclean. Three concrete unmounted probes; D07/owner preserved.
  Next runtime/patch manifest and key/protected-store probe contract; no startup mounting.
  Six other dependencies/actual runtime identity/PKI/DF01–03 unresolved; full V01/M0 incomplete.
  See V01HI_TASK_CARD/closure matrix; keep accepted environment deferrals.

Historical V01-G checkpoint:

- V01-G applied/reverified main233a274:92new/983defaultPASS/55opt-in SKIP/buildclean.
  Unmounted typed profile/readiness core only; next bounded concrete read-only probes.
  Actual principal/catalog visibility/protected stores/PKI/DF01–03 remain unresolved.
  Full V01/M0 incomplete; see V01G_TASK_CARD/closure matrix; preserve owner files.

Historical V01-F checkpoint:

- V01-F applied/reverified main652e48a: F6browser→ownedCOMMIT +E12 =18PASS,
  default891PASS/55opt-in SKIP/buildclean.54scratch DBs cleaned; D07/owner files unchanged.
  Next bounded unmounted fail-closed profile/readiness contract; runtime identity,
  metadata visibility/key stores/PKI/DF01–03 unresolved. Full V01/M0 incomplete.
  See V01F_TASK_CARD/V01_READINESS_CLOSURE; keep accepted D12/D13 deferrals.

Historical V01-E checkpoint:

- V01-E applied/reverified PASS main95bb665:12owned COMMIT cases, default891PASS/
  49opt-in SKIP/buildclean. All scratch DBs cleaned by identity; D07/owner files unchanged.
  Next readiness closure matrix/contract: runtime profile/principals/PKI and combined
  browser→owned COMMIT remain unverified. Full V01/M0 incomplete; ADR177 applies.
  See V01E_TASK_CARD/E-verification-summary and ADR178.

Historical checkpoints:

- V01-D applied/reverified PASS main8920098:37integration/891default/48frontend
  PASS,37intentional opt-in SKIP, buildclean; SQL baseline/owner files unchanged.
  Next V01-E: prepare bounded owned COMMIT/cleanup/SNAPSHOT/security audit/races
  contract under standing ADR177. Full V01/M0 incomplete; runtime profile/principal
  readiness remains unverified. See V01D_TASK_CARD/D-verification-summary.
- V01-B→V01-C completed/reviewed PASS/applied ADR163/reverified mainbca0e4e:
  manager/main27HTTP→SQL PASS, default891PASS/27opt-in SKIP/buildclean.
  Owner exact AGY+SQL scopes approved; rollback catalog/profile/102permissions
  unchanged/0retained rows/users/members. See V01BC_TASK_CARD/verification-summary.
  Full V01 incomplete; next prepare specific browser HTTPS/profile/principal/
  owned COMMIT/race evidence scope, no automatic runtime/SQL expansion.
- U03→U04 complete/reviewed PASS/applied ADR163/reverified2026-10-07 maind51564e.
  38frontend tests/types/lint/build/Edge375/768/1280 PASS; two bounded AGY SUCCESS.
  Explicit signup/login/me/logout/session UI seams; default App remains unavailable.
  Owner files/backend unchanged; see U03/U04 cards/evidence and ADR175.
- U05→U06 complete/reviewed PASS/applied ADR163/reverified2026-10-08 mainf83a7d6.
  48frontend tests/types/lint/build/Edge3widths PASS; bounded AGY reviewed independently.
  Exact password change and Admin detail/lock/manual lookup seams; App unmounted.
- V01 preflight/V01-A reviewed PASS2026-10-08:891non-SQL+120approved SQL tests.
  Exact5classes/rollback; before/after catalog/profile/102permissions unchanged,
  0retained rows/users/members. Integrated V01 incomplete; next combined host
  needs a concrete scope; runtime/owned COMMIT/packages/push/deploy unopened.
  See V01_TASK_CARD/NEXT_CHAT_HANDOFF; SQL permission applies only executed V01-A.
- R02→R03 owner-approved scope reviewed PASS2026-10-06:1386tests/buildclean.
  Applied/reverified code mainfcdd21e under ADR163. Explicit Admin007/010/014 plus
  completion protocol; default Program unmounted. See cards/evidence.
- R02/R03 backend checkpoint unchanged; SQL counts below are historical, not rerun.

- D07→D08→D09 owner batch complete/reviewed PASS/applied/reverified code maina7561dd;
  build clean/481unit+102SQL PASS. Cards/evidence in docs/plan/D07/D08/D09.
- D10→D11 complete/reviewed PASS/applied/reverified main6aaa4d7:
  sixviews/sevenSP/fivemigrations,483unit+182SQL PASS/buildclean; task cards/evidence.
- D12 authorized roles/grants scope applied/reverified code mainfc9227f: four roles/
  exact102permissions/six migrations;484unit+335SQL PASS. See D12_TASK_CARD/evidence.
- D13→A01 completed/reviewed PASS/applied/reverified main74e3085:513unit+366SQL
  PASS/buildclean; exact catalog/five EF trigger paths/atomic audit/security fallback.
- A02→A04 authorized scope completed/reviewed PASS/applied/reverified maina8ad1d5:
  545unit+375SQL=920PASS/buildclean; operator bootstrap and framework CSRF boundary.
  No real Admin or runtime activation; see A02/A04 task cards and operator prerequisites.
- A03→A05 reviewed PASS2026-10-06:586unit/HTTP+409SQL=995PASS/buildclean;
  applied/reverified maina9abdc6 under ADR163. Signup/login adapters
  explicitly unmounted; no production profile, real Admin or current-session resolver.
- A06→A07 applied/reverified PASS mained40a4b2026-10-06:632unit/HTTP+438SQL=1070PASS/buildclean;
  main/manager identical backend verified under ADR163. Exact me/logout and live
  session hooks remain explicit/unmounted; production profile/principal/full readiness deferred.
- A08→R01 owner-approved scope reviewed PASS2026-10-06:733unit/HTTP+462SQL=1195PASS,
  buildclean; applied/reverified code main3b46460 under ADR163. API004 and disconnected
  registry55/schema-aware C1 only; no runtime/T23 at that checkpoint. R02→R03 now reviewed above.
  Real login/Windows-linked demo users and runtime/schema/maintenance bindings
  remain operator-profile prerequisites; WITHOUT LOGIN tests do not prove login auth.
- Isolated EnglishCenter_D07_20261005 retained,160/collation/snapshotON;24tables/
  230cols/5triggers/6UDF/6views/7SP/6migrations/4SQLroles, fixtures/users/memberships rolled back. No DROP DB/other DB changes.
- Runtime DB DI/profile/business callers/complete TX/auth/audit/replay/identity bindings/
  fullLOCAL_DEV/M0/V01 still pending; no push/deploy. ADR163 applies reviewed PASS
  stages already authorized, not unrelated stages. D01–D06/prior tasks remain PASS.

SQL assessment04/10/2026 completed: installed SQL2025 Express17.0.1000.7
retained by ADR-162; see [assessment](plan/SQL_INSTANCE_ASSESSMENT.md).
Read-only inventory/type/decimal/JSON/SHA and system-DB snapshot read succeeded;
no app DB creation/config changes. Current RTM lacks newer patches; patch review
before real data/exposure and encrypted-archive backup prerequisites remain.
P01 P02 package compatibility verified; full LOCAL_DEV slots remain unresolved
in M0_LOCAL_PROFILE. Planning checkpoints below predate detached P02 execution.

- M0 order synchronized04/10/2026: docs/plan/M0_QUALITY_PLAN.md section11 holds
  proposed39family default order and Mermaid; dependencies in sections4–7.
  Start=P01 planning/profile. P05beforeP04, A04beforeA03, D07beforeSQLaudit/replay
  evidence explicit; full readiness/skill gates retained.
  SQL target updated to installed SQL2025 Express by ADR-162; no install/code/dispatch.

- Skill audit04/10/2026: AGENTS and docs/plan/SKILL_ROUTING.md now require
  scope-appropriate backend/frontend skills and Codex code-review for code tasks.
  Task cards/worker contexts include skill sources, canonical overrides and actual
  verification.4local skills audited; C2C inactive, no dispatch/code/skill install.

- M0 detailed planning04/10/2026: docs/plan/M0_QUALITY_PLAN.md records task families,
  dependencies, evidence gates and first3proposed scopes. Local SDK10.0.200/
  Node24.16.0/npm11.13.0 checked; SQL .\SQLEXPRESS version17.0.1000.7 Express
  is now the accepted SQL2025 Express local target (ADR-162), assessed in
  docs/plan/SQL_INSTANCE_ASSESSMENT.md. Keep this instance; no Developer install.
  P01 still needs package/profile checks; full DB readiness retained. No app code,
  installation, migration or dispatch performed.

- Analyze smoke and bounded P02 edit complete04/10/2026: SUCCESS/exit0,
  manager event/diff review and independent verification. Future tasks need bounded contracts,
  detached worktrees and owner permission before applying worker changes.
- Discuss proposed M0–M4 implementation milestones in
  `docs/plan/implementation-plan.md`; refine M0 dependencies and small generation
  contracts by complexity/output capacity and quality gates, not calendar speed.
  Owner chose quality-first decomposition04/10/2026; later first-code handoff
  authorizes only P01→P02, see FIRST_CODE_HANDOFF. Wider milestones stay proposals;
  P02 code/packages applied and reverified on main; no SQL/deployment/runtime install.
- Update affected source/trace/ADRs if owner asks for a later design change.

## Deferred / evidence boundary

- D12/D13 performance/deployment values keep original deferral/notes.
- DF01–05 environment/evidence prerequisites remain: backupmedia/access/key/
  isolatedrestore,compatiblepatch/blocklistpins/profilecompatibility/config,
  future SQL/API/EF/grants/race/browser/CSV/load/recovery evidence.
- No business/runtime/SQL evidence from P02 startup tests. Four-phase/Slice0/integration work
  is now open for owner-requested planning; phase/Slice0 proposals are not accepted.
  Integrations remain on-demand and inactive.

## Completed

- 2026-10-03: Actual E04-v1 accepted ADR-160,then E05-v1/full baseline ADR-161;checked2owner gates/stages4–5done,D12/D13notes kept,no implementation/planning/tests.

- 2026-10-03: Completed E04-v1 review-ready package/final contract and prepared E05-v1 cross-check; clarified E03/Teacher-history wording, preserved approvals and deferred choices. No runtime evidence/implementation.

- 2026-10-03: Owner approved RF01/02/04; ADR-157/158/159 and canonical
  requirements/NV/W/E04/memory synchronized. No full E04/E05 acceptance/
  code/SQL/runtime tests; D12/D13 and owner stage gates preserved.


- 2026-10-03 (SC02–04-v1): Added55actions/C1bytes+refs/replay/SP-FN/EF/grants/
  locks/audit/runtime-slot appendices and SC01 consolidated parity review.
  Synced design/memory;RF01/02/04 pending,RF03 Accepted156;no code/SQL/tests.


- 2026-10-03 (D10): Added5 Reports/CSV schemas,154/154 domain DTO coverage.
  Role-scoped snapshot/filter/time/totals/decimal38/distinct/LESSON/FINALIZED
  no-data and fresh all-results bounded CSV documented, with2 JSON/1 CSV
  illustrations. New wire details draft; SC-01 parity/SC-02–04/E04–E05 open.
  Sources/ADR-156/owner marks/side work preserved; no code/SQL/runtime tests.

- 2026-10-03 (D9): Added10 whole-Class grades/review/Student-own FINALIZED
  schemas,149 total/5 remaining. Full-cohort guards/finalize Registrations,
  scoped preview/review/publication/history/unfinished TX/decimal results
  and examples documented. Sources/ADR-156/owner marks/side work preserved;
  RF01/02 still proposed/E04/E05 open. Static checks only, no code/SQL/tests.

- 2026-10-03 (D8): Added9 Attendance/Teacher gradebook/detail/selector/draft-save
  schemas,139 total/15 remaining. Full cohort/delta/absence guards, current
  Teacher/Academic read-only projections, per-Student score-comment TX/report
  version touch, full required-component totals/missing semantics and examples.
  Sources/ADR-156/review marks/earlier side work preserved; RF01/02 still
  proposed/E04/E05 open. Static checks only, no code/SQL/runtime tests.

- 2026-10-03: RF03 approved and recorded ADR-156; synced root/weekly/move-target
  future cutoff/no backfill/live-clock/whole-batch checks to requirements,
  dated NV19/W16 addenda and E04/memory.130/154 DTO drafts unchanged;
  RF01/02/E04/E05 still open, owner marks/E03 physical model preserved.
  No code/SQL/runtime tests; next Attendance/per-Student draft API-131–139.

- 2026-10-03 (D7): Added17 Schedule/scoped-calendar drafts,130 total
  API-001–130/24 remaining. Whole concrete weekly plan, resource/cohort/full
  obligation contexts, move/replacement atomicity, manual HELD/role-safe
  chains and JSON examples written. RF03 clock cutoff proposed, RF01/02 open.
  Accepted sources/owner marks preserved; catalog/memory synced, no code/SQL/
  runtime tests or E acceptance. Full-set/plan pagination clarified.

- 2026-10-03 (D6): Added23 Class/placement/Teacher-Student DTO drafts,
  113 total API-001–113/41 remaining. Full cohort/Schedule/Attendance guards,
  explicit lifecycle/Teacher change, atomic placement/internal DRAFT privacy,
  three role projections and examples/recovery documented. Accepted sources/
  owner marks/earlier report-sequence artifacts preserved; RF01/02 proposed.
  Catalog/memory synced; static checks only, no code/SQL/runtime tests.

- 2026-10-02 (D5): Added17 Course/Band/Program/shared-result DTO drafts,
  90 total API-001–090/64 remaining. Exact source/target component sets,
  selected-relation versions, activation weights/exam checks, used freeze,
  rename/lifecycle separation and lookup role/action pairs/examples written.
  Protected sources/review marks unchanged; E04-RF01/02 not accepted.
  Catalog/memory synced, static checks only; no code/SQL/runtime tests.

- 2026-10-02 (D4): Added20 Session/Teacher DTO drafts,73 total API-001–073/
  81 remaining. Task scopes/full sets/ABSENT vs score/original provenance/recovery
  and safe examples written. E04-RF01/02 proposed and catalog/memory synced;
  protected sources/review marks, no Accepted ADR/code/SQL/runtime tests.

- 2026-10-02 (D3): Added16 DTO drafts Admissions/Student Invoice/Accountant
  Invoice-Payment,53 total API-001–053/101 remaining. Role allowlists/absence/
  immutable money-time/fresh print/lookup role pairs and safe examples written.
  Catalog/memory synced; accepted sources/review marks protected, no runtime tests.

- 2026-10-02 (D2): Added27 proposed DTO entries to E04_DTO_SPEC,37 total
  API-001–037/117 remaining. Admin/audit/home/Registration/booking/confirmation
  schemas/guards/full sets and safe examples written; catalog/memory synced.
  Protected accepted source/review marks; no ADR acceptance/code/SQL/runtime tests.

- 2026-10-02: Created E04-C1 registry/route manifest/W→API→E03→case trace
  and E04-D1's10 schema drafts. Checked document inventory (154 paths/55 UUID/
  6 lookups/32 F/21 W/37 NV), explicit SC gaps; no implementation/tests or E acceptance.

- 2026-10-01: Recorded all23 checked E04-R1 rows plus explicit all-approval
  as ADR-150–155; synchronized displayed contracts and preserved notes. Restored
  stale OWNER_REVIEW E01–E03 acceptance metadata/checks; E04/E05 still unchecked,
  D12/D13 deferred, E03 unchanged. No code, SQL, provisioning or executed tests.

- 2026-10-01: Recorded presented Gói4e acceptance as ADR-149; created
  E04_REMAINING_REVIEW E04-R1 (6 packages/23 unchecked owner-review rows).
  Preserved OWNER_REVIEW gates/notes and E03; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Schedule/scoped calendar4d acceptance
  as ADR-148; prepared Attendance/per-Student draft4e proposal.
  E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Class/placement/scoped reads4c acceptance
  as ADR-147; prepared Schedule/weekly generation/move/replacement/HELD/scoped
  calendar4d proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Course/program/configuration4b acceptance
  as ADR-146; prepared Class/placement/scoped reads4c proposal.
  E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Entry sessions/Teacher grading4a acceptance
  as ADR-145; prepared Course/program/configuration4b proposal.
  E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Admissions/Finance3c acceptance as ADR-144;
  prepared Entry sessions/Teacher grading4a proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Student Admissions3b acceptance as ADR-143;
  prepared Admissions/Finance3c proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Students3a acceptance as ADR-142;
  prepared Student Admissions3b proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Administration2b acceptance as ADR-141;
  prepared Students3a proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 Auth/session2a acceptance as ADR-140;
  prepared Administration2b proposal. E04/E05 unchecked; no code/SQL/tests.

- 2026-10-01: Recorded presented E04 package1 acceptance as ADR-139 and
  prepared Auth/session2a proposal. E04/E05 unchecked; old choices preserved.
  No application/SQL/migration, provisioning, packages or executed tests.

- 2026-10-01: Recorded actual consolidated E03-v1 acceptance; checked only E03,
  marked stage3 done and stage4 doing. Prepared E04 opening draft only.
  No new ADR/code/SQL/migration or runtime tests.

- 2026-09-30: Recorded the owner's explicit full-artifact E02-v1 acceptance;
  checked only E02, marked workflow stage done and E03 database stage doing.
  No new ADR, code, runtime tests or implementation authorization.

- 2026-09-30: Prepared E02-v1 WORKFLOW_SPEC from owner-confirmed sections;
  verified21 W flows and trace coverage of37 retained NV/32 F, linked unchecked
  E02 row and marked stage 2 review. No app tests, code, new ADR or E02 approval.

- 2026-09-30: Recorded the owner's explicit E01-v1 acceptance; checked E01,
  marked business functions done and workflows doing. No E02 acceptance,
  new ADR, code, runtime tests or implementation authorization.

- 2026-09-29: Prepared E01-v1 FUNCTION_SPEC with37 detailed retained functions,
  shared actions, validation/actor matrices and32-screen trace; verified structural
  coverage against39 source NV with NV10/NV11 and F15 excluded. Updated catalog,
  review link/stage1 review and handoff. No E approval/new ADR/code/runtime tests.

- 2026-09-29: Recorded explicit whole-Gói 3 approval as ADR-121–ADR-129,
  checked FE11–FE19 and synced affected specification/target/catalogue/plan
  memory. Preserved 66 previous rows/notes and ADR-001–ADR-120 unchanged;
  68/75 accepted, D12/D13 deferred and E01–E05 pending. No code/operations.

- 2026-09-29: Prepared one bundled Gói 3 review with nine unchecked FE11–FE19
  rows, context/trade-offs/risks in OWNER_REVIEW. Preserved 66 previous rows/
  notes; total 75 (59 accepted/16 open). Updated review handoff, no new product
  approval/ADR, code, operations or E acceptance.

- 2026-09-29: Recorded six newly checked D choices as ADR-114–ADR-119 and
  explicit D17 default owner-operator confirmation as ADR-120. Preserved owner
  notes/other table rows and D12/D13 deferral; synced affected memory.
  59/66 accepted rows, seven open; Gói 3/E still unaccepted. No code/operations.

- 2026-09-29: Recorded complete Gói 2 approval; added eight FE03–FE10 rows
  and ADR-106–ADR-113. Synced accepted frontend contracts/affected memory;
  preserved old approvals and all open D/E. No code/packages/deployment.

- 2026-09-29: Created owner-requested SELF_HOSTED_DEMO.md with local/LAN/
  Internet-tunnel flow, operating cycle, safety and demo fallback. Linked memory;
  no code, installation, public access or D/E acceptance.

- 2026-09-29: Saved FE02 page/navigation as ADR-105 and synced affected memory.
  Hosting question remains advisory; no deployment approval/code/cloud resources.

- 2026-09-29: Saved FE01b menus/landing as ADR-104, synced screen mapping and
  prepared unchecked FE02 page/navigation proposal. No code or E acceptance.

- 2026-09-29: Saved FE01a approval as ADR-103, synced responsive shell and
  prepared unchecked FE01b role menu proposal. No code/stage-output acceptance.

- 2026-09-29: Saved eight Gói 1 D approvals as ADR-095–ADR-102, including
  revised D15 responsive priorities. Synced affected memory/contracts and
  prepared FE01a for Gói 2 discussion one part at a time. No code/E acceptance.

- 2026-09-29: Updated next-chat handoff for frontend discussion, preserving all
  accepted A–C decisions and open D–E choices; added an opening prompt and
  grouped-review guidance. No new layout/library/design approval or code.
- 2026-09-28: Persisted 33 approved A–C items as ADR-083–ADR-094 and synced
  requirements/catalogue/22-entity target/affected memory and owner-review metadata.
  Preserved owner checks and all D/E rows; no code/schema or E/stage acceptance.
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
