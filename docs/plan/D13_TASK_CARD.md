# M0.D13 — SQL compatibility and EF trigger integration — preparation

Owner2026-10-05 requests two next families after D12: M0.D13 then A01.
Base1d5ba8f/codefc9227f; D12 remains484unit+335SQL PASS. Standing ADR163 allows
exact independently reviewed PASS apply/reverify, not unrelated work.

implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Previously read complete skills/contract/routing;
unchanged hashes verified this turn: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: E03/E04 exact schema/signatures/SQLServer160/real SQL tests
prevail over generic SQLite/seeding/API skill guidance. No package/routes/UI changes.
read_context: ordered memory, requirements, HANDOFF/FIRST_CODE_HANDOFF, M0 quality,
E04 SQL trigger/output/grants/signatures, runtime readiness contract, D12 evidence,
current EF model/migrations and existing SQL test helpers. No decisions reopened.
skill_verification: not dispatched or verified yet; no implementation PASS claim.

## Code scope

Bounded AGY creates only Infrastructure/Compatibility/CatalogCompatibility.cs
from D13_CATALOG_WORKER.md and backendSKILL/routing; one file/220diff lines.
Codex implements expected manifest and read-only catalog inspection under
Infrastructure/Compatibility/, independent unit/SQL tests and verification docs.
Compare complete expected physical names/types/nullability/identity/collation/RV,
keys/FK/index/check signatures, migrations, object signatures/bodies and role
permissions; count-only success is forbidden. Snapshot/compatibility prerequisites
checked. Inspector uses caller-supplied context/finite diagnostic command timeout
and cancellation, no hidden connection string or startup connection/mutation.

EF integration verifies generated IDs/new8byte RV on INSERT/UPDATE of exactly
Band/TrainingProgram/ScoreComponentConfig/Attendance/Score, table-scoped OUTPUTfalse,
positive and invalid writes, rollback and discard stale tracked generated state.
Missing/drift/unreadable objects/SQL failure fail closed, with safe cause classes.

Compatibility evidence is separate from full business/runtime readiness. Runtime
principal/config/login/keys/deadline slots remain absent: no runtime-ready claim,
host endpoints/DI wiring/migration auto-apply or relaxation to dbo. In particular
runtime has no migration-history SELECT grant: inspection denied under current
role must return not-ready; do not silently add a grant or bypass with operator.
Actual operator diagnostic compatibility may pass without certifying runtime.

## SQL proposal awaiting explicit approval

Only existing .\\SQLEXPRESS/EnglishCenter_D07_20261005. Read complete catalog and
execute EF test-fixture INSERT/UPDATE/DELETE as diagnostic operator. Reuse only
the four D12_fixture_user_0..3 WITHOUT LOGIN principal names/role membership in
rollback fixtures when checking privilege/unreadable behavior. For negative drift
checks, transactionally ALTER a selected existing view/function/procedure/trigger
body or table column/index/check/FK, or DROP a selected module; always rollback
before further tests. No applied source edits or persistent schema/grant change.
Validate signatures against immutable sources again after fixtures. Do not alter
database snapshot/collation/compatibility/owner, create/drop DB/server login, add
principals beyond four names, seed real data, configure runtime or push/deploy.
Failing fixture/cancellation rolls back; retained database baseline stays D12.

Required gates: build/locked restore/unit+real SQL regressions, exact healthy
catalog and intentionally missing/drifted cases, five EF generated ID/RV paths,
failure/cancellation/unreadable role and unchanged D12 baseline after rollback.
Codex code-review before PASS/apply, record actual results and limits here.
