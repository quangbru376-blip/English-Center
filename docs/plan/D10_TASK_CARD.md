# D10 — six read-only business views — applied/reverified PASS

Owner2026-10-05 requested D10→D11, then explicitly approved all three concrete
scopes: listed D10 worker cards + listed D11 worker cards/backendSKILL/routing to
Google Antigravity, and migrations/object Down→Up/rollback fixtures/allocator
two-connection races on existing .\SQLEXPRESS/EnglishCenter_D07_20261005 only.
Initial dispatch auto-review rejection was resolved by this explicit approval;
no rejected operation executed. No database creation/drop/other DB/grants/real
seeds/runtime/push/deploy. ADR163 authorizes exact reviewed PASS apply/reverification.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
integration: antigravity-worker. Complete selected skills read; source SHA256:
backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: E03/E04/requirements exact SQLServer160, money18/38, UTC/VN
date/time, NULL/full-set/current role projections prevail over stale SQLite/API/
seed guidance. No dependencies, DTO/routes/UI changes; review checklist N/A there.
read_context: ordered Project Memory/requirements, HANDOFF/FIRST_CODE_HANDOFF,
M0 quality/SKILL_ROUTING, E03 objects/exact schema, E04 signatures/report DTO31–35,
backend/code-review/AGY complete skills. Owner walkthrough below replaces generic
notes/API-doc checklist under routing because this stage only creates SQL objects.

## Implementation / independent review

Six one-grain facts: Registration/current references, Booking/own completed
Attempt score (ABSENT NULL), active current Schedule excluding cancelled Class,
Class with placements pre-aggregated/FN04, FINALIZED stored results/pass threshold,
Invoice +unique Payment with actual amount_paid/paid_at. Explicit projections,
current contacts (no profile snapshot), cancelled history retained where accepted.
No web endpoints or Director SELECT rights. Time predicates stay caller supplied.
Six SQL production scripts accepted unchanged after full source/engine review.

skill_verification:6AGY SUCCESS/exit0, runner diff46/56/23/40/25/26lines; one allowed
SQL each. Compact receipts/audits prove all3complete context reads before write,
zero observed tool/scope violations. Original worker trees retained. Tool events
do not prove OS isolation/comprehension. Manager wrote behavioral/metadata tests,
EF-generated ReadOnlyBusinessViews migration/embedded resources/reverse Down.
No edits to applied D07–D09/model or package locks.

482unit+117SQL PASS/0fail0skip; build0warnings0errors.15new view cases check grain,
replacement/current filtering, cancelled Class, exact columns/widths, current
Account name, FINALIZED-only, actual receipt amount/time. Initial fixture omitted
HELD/NOT_HELD actor/time bundles; Codex corrected fixtures to existing CHECKs.
Accepted Schedule CHECK makes NOT_HELD noncurrent; historical worker wording
"current NOT_HELD" is unreachable, no schema relaxation introduced.
Down→retainedD09catalog→Up PASS; exact6views/4migrations and24table230column baseline.

## Main / owner walkthrough

Manager b6b327c; main18e1edb; backend tree7b9e6911fc06497d8b718d601d828d27f402668e
identical. Main482unit+117SQL reverified. D10-evidence/verification.json has actual
TRX/counter/hash/tree proof. Later D11 adds SPs; D10Schema is pre-D11 checkpoint.
1. Views expose explicit SQL facts at one stable grain.
2. Aggregate placements/schedules before joining to avoid multiplied counts.
3. Immutable migration resources create6views; Down drops only those6.
4. Scoped services/grants/runtime/readiness remain later stages.
