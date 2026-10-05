# D11 — seven procedures — applied/reverified PASS

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

SP01 admissions search viaVW01, literal LIKE escaping/exact enum/page<=100 and
createdAt/id direction. SP02 actual current Teacher/AcademicAffairs scope viaVW02,
FN01 passed clock, Teacher20columns/AA21 (invoiceStatus absent forTeacher), current
SCHEDULED/history retained, missing/wrong assignment tagged error. SP03 five exact
visible report/config/score/linked Schedule sets, no DRAFT tokens/comments; historical
Schedule refs retained. SP04 separate revenue receipt/billed issue/current debt
cohorts viaVW06/FN06, overdueOnly debt-only, SUMdecimal38 then varchar38, CSV both
page argsNULL. SP05 created/current cohort viaVW01, whole distinctStudent independent
of group sum. SP06 date-free inventory viaVW04 +FINALIZED-period results viaVW05,
cancelled progress/examsNULL, stored rounded pass>=50, decimal half-up rates.
Options full/not paged. SP07 caller-owned year row/range UPDLOCK/HOLDLOCK, format
varchar16 HD-YYYY-NNNNNNNN, year1..9999/counter8digit bounds, no own commit/Invoice.

SP01–06 fail closed without active caller SNAPSHOT; service owns connection/TX/
authorization/projection. Temporary staging only, no business writes/audit/replay.
SP07 only counter DML within caller TX. New immutable EF migration
ReadOnlyQueriesAndInvoiceAllocator creates7SP, Down drops only7; unchanged tables,
model, D07–D10 resources and package locks. No startup DB connection/auto-migrate.

skill_verification:7AGY SUCCESS/exit0 and1AGY ERROR rejected. Zero observed tool/
scope violations;3complete context reads beforewrite proven in compact success
audits. ERROR original retained/not adopted; fresh SP06 same authorized payload
succeeded. Manager independently corrected SP01 guessed assignment column/catalog
status validation, SP02 invented view/page cap/402-character escaped-name buffer,
SP04/SP05 real view paths and consistent historical options, SP06 exact date bounds/
SNAPSHOT/count intermediate, SP07 year error code. All originals retained.
SP06 historical card typo varchar16 contradicted exact Band schema30; actual source
and output preserve30. Context audit hashes describe actual dispatched versions.

483unit+182SQL PASS/0fail0skip, build0warnings0errors, locked cache-only restore.
65new SQL cases beyondD10: role/restricted/scope, stable ties/out-of-range pages,
literal escaping/enum/page/date rejection, exact ordered grade/role result sets,
binary8 versions/varchar38 amounts, DRAFT exclusion, linked noncurrent Schedule,
receipt versus issue/debt cohorts/cancelled invoice/due equality, repeated learner
across/same Course counts, cancelled inventory/result retention,66.67% half-up,
no-dataNULL/whole-set totals, allocator missing TX/year/overflow/max/padding,
two connections finite timeout while first holds new year range/created counter
row, release on rollback then allocation1/zero persisted fixtures. Existing counter
increments tested within rollback fixture, not committed production counter race.
19digit monetary aggregate verified (beyond individual18digit cap); no fabricated
29+digit data-load proof. Exact47parameters/7SP/6VW/6FN/5TG/5migrations catalog PASS;
24live SQL module bodies equal immutable resources after ONLY engine CREATE-header
and CRLF normalization. Initial text verifier corrected for engine blanked OR ALTER.
Down→D10catalog→Up PASS; all230live tablecolumns still agree with EF baseline.

## Main / owner walkthrough

Manager01f7fc3; main6aaa4d7; identical backend tree999cd7e67d5a145caea9481281544246963df5e3.
Main483unit+182SQL=665PASS/0fail0skip/build0warnings0errors. D11-evidence verification
JSON retains actual TRX/hash/tree proof, source/scopes and rejected receipt/audit.
1. Service startsSNAPSHOT; read SP returns exact count/page/options without commit.
2. Reports aggregate whole filtered facts before paging and project no learner IDs.
3. Allocator holds counter lock until caller commits or rolls back the whole approval.
4. Real callers/error-to-HTTP mappings/session auth/audit/replay/grants/readiness and
full business races remain later; this SQL stage does not certify them. NextD12
roles/grants not started or authorized by completed two-package batch.
