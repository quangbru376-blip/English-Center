# D12 — four SQL roles and isolated permission evidence — applied/reverified PASS within approved scope

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
skill_verification: four AGY SUCCESS/exit0; complete three context reads before
write and zero observed worker scope violations. Exact outputs independently
reviewed; no worker validation claims used as evidence.

## Explicitly approved worker/data-sharing scope

Four independent, sequential bounded contracts: D12_RUNTIME_WORKER.md,
D12_FINANCE_WORKER.md, D12_ACADEMICS_WORKER.md, D12_DIRECTOR_WORKER.md.
Each writes only matching Infrastructure/Sql/D12/{RUNTIME,FINANCE,ACADEMICS,DIRECTOR}.sql
in a detached AGY worktree; at most1file/220diff lines. External payload consists
of those four cards plus .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md. No AGENTS/other source reads, commands, SQL execution,
network tools, migrations, principals, secrets or apply authority for worker.
Codex handles migration lifecycle, actual permission tests, review and apply.

## Explicitly approved isolated SQL execution

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

## Independent review and verification

Owner explicitly approved the exact six-file external payload and the isolated
roles/migration/four WITHOUT LOGIN test-user scope before dispatch/execution.
Four worker outputs preserve exact accepted object rights without manager source
changes; original detached trees/receipts/compact read audits retained as evidence.
Codex generated immutable LeastPrivilegeRoles migration and independent unit/SQL
tests, reviewed unchanged model/package locks/D07–D11 resources. Down preflights
all role memberships before any removal. No broad grants or grant options.

484unit+335SQL=819PASS/0fail0skip; build0warnings0errors/cache-only locked restore.
153new SQL cases cover full96 runtime table-operation matrix,24role/view cases,
actual runtime update/unused DRAFT component insert/delete, six direct UDF calls,
all seven procedure execution paths including operational business rejection,
allowed report sets equal operator projections, reporting base reads/deletes/
updates forbidden, counter direct access denied while allocator works, effective
ALTER/CONTROL/history-delete0, DDL/delegation/impersonation blocked, DENY overriding
direct user grant and reporting REVOKE removing its sole view path. Reverts and
fixtures/users/memberships/temporary permission changes rollback.
Exact102permissions/four dbo-owned custom roles/six migrations/profile PASS;
Down→D11 exact catalog and noD12roles→Up PASS. Existing24table230column/model and
24live SQL module-body regression remain PASS. No migration history/table drift.

Diagnostic limitation: ALTER/history-delete failures can be15517 in the local
Windows owner context; they are paired with independent effective-rights0 checks,
not treated alone as authorization proof. Impersonation is tested against another
of the four isolated role principals. An initial extra WITHOUT LOGIN diagnostic
probe was created outside the approved four-name set; it was rolled back, absence
verified, and discarded from final source. No persistent extra principal/grant.
This manager scope deviation is recorded separately from zero observed AGY scope
violations. Login/Windows-linked identity provisioning/authentication, owner profile
health and full E03 login demonstration remain unverified operator prerequisites.
No owner/database-owner/server-login changes were made to mask those limitations.
Next proposed M0.D13 is not authorized or started by this D11/D12 request.

## Main checkpoint

Manager9318f40 applied as mainfc9227f under ADR163; backend tree
c0aa1f9f379d42ec390d7722b29f77ea2d8da371 identical. Main locked cache-only restore,
both builds0warnings0errors,484unit+335SQL=819PASS/0fail0skip. ExactD12 catalog and
final rollback verified after main tests. Evidence JSON includes actual main TRX
hashes/counters, source hashes and both tree identities. Repository contains no
unreviewed worker output. Original workers/manager tree retained for inspection.
