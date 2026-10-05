# D12 — four SQL roles and isolated permission evidence — preparation

Owner2026-10-05 requested D11 and D12. D11 remains applied PASS at code
6aaa4d7 (483unit+182SQL); do not regenerate its immutable SQL. D12 implementation
is now requested; D13 readiness, runtime connections, production principals,
server logins, secrets, other databases, push/deploy remain outside this task.
ADR163 applies only after Codex independent review and required checks PASS.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
integration: antigravity-worker. Complete skills and contract reference read.
skill_sources SHA256: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: E03 section10.5/E04 SQL section5 exact object-level rights,
SQLServer160/real SQL tests override SQLite/generic seeds/API skill guidance.
No new dependencies, API/DTO/UI changes; corresponding review checklist N/A.
read_context: ordered Project Memory, requirements, HANDOFF/FIRST_CODE_HANDOFF,
M0 quality/local profile, E03 role contract, E04 grants/runtime profile, routing
and complete selected skills. Worker reads only own card plus backendSKILL/routing.
skill_verification: not dispatched; no review/test/PASS claim yet.

## Concrete worker scope awaiting data-sharing approval

Four independent, sequential bounded contracts: D12_RUNTIME_WORKER.md,
D12_FINANCE_WORKER.md, D12_ACADEMICS_WORKER.md, D12_DIRECTOR_WORKER.md.
Each writes only matching Infrastructure/Sql/D12/{RUNTIME,FINANCE,ACADEMICS,DIRECTOR}.sql
in a detached AGY worktree; at most1file/220diff lines. External payload consists
of those four cards plus .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md. No AGENTS/other source reads, commands, SQL execution,
network tools, migrations, principals, secrets or apply authority for worker.
Codex handles migration lifecycle, actual permission tests, review and apply.

## Isolated SQL execution proposal

Only existing .\\SQLEXPRESS/EnglishCenter_D07_20261005. A new immutable migration
creates four dbo-owned ec_* roles and exact object grants/explicit reporting
base-table DENYs, with fail-if-role-exists preconditions. No blanket schema grants,
fixed role membership, owners changed, EXECUTE AS modules or applied SQL edits.
Down removes only these four roles after checking no members; Up recreates them.

Codex tests actual operations through four transaction-scoped WITHOUT LOGIN test
users, each belonging to exactly one role; EXECUTE AS USER and guaranteed REVERT,
all users/memberships/data and grant/revoke demonstration changes rolled back.
Test allowed views/SPs/UDFs/table DML, forbidden direct reads/mutations/DDL/history
deletes/counter access, output privacy and GRANT→REVOKE→denied behavior. Confirm
runtime is neither dbo nor db_owner, reporting SP ownership chains work with base
DENY. Apply Down→D11 checkpoint→Up and regress existing unit/SQL/module-body tests.
Keep database and four roles; create/drop no database and create no server login.

This proves database permission enforcement under impersonated database principals;
it does not prove login authentication or provision real runtime/operator identities.
E03 four login/Windows-linked demo users and runtime/schema/maintenance identity
bindings remain operator-profile prerequisites; do not represent them as complete.
No credentials or server-level permission changes are needed for this proposal.

Owner walkthrough: migration gives each role only listed objects; SQL tests execute
as a role member, show permitted projections and rejected operations, then rollback.
Tests use an operator connection for setup only, never claim it is the runtime user.
