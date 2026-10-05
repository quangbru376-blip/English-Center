# Isolated SQL verification

Ordinary backend/EnglishCenter.sln tests stay disconnected. Run the explicit
EnglishCenter.SqlTests project only after the reviewed migration chain is applied
by Codex/operator to .\SQLEXPRESS / EnglishCenter_D07_20261005. This runner uses
Windows authentication and local diagnostic TrustServerCertificate, never runtime
credentials. It creates fixture data in transactions and always rolls back.
Fixtures verify relational behavior, not complete business/auth/audit workflows.
Do not change the target to an existing business database.

    dotnet test backend/tests/EnglishCenter.SqlTests/EnglishCenter.SqlTests.csproj

D07Schema.sql is the initial pre-D08 catalog/profile/snapshot gate. D08Schema.sql
checks five enabled triggers, table/event matches and fixture rollback. Migrations
are explicit offline-generated/reviewed artifacts; no startup auto-migration or
runtime DB configuration is added. Never edit an applied versioned SQL resource;
make a new reviewed migration/resource version for future changes.

D09Schema.sql verifies all six UDF signatures and the three-migration chain. The
explicit SQL suite covers guards/functions/full live column-model agreement.
Runtime callers, roles/grants, complete service transactions and full readiness
remain later tasks; this runner does not configure an application connection.

D10 adds six one-grain fact views; D10Schema.sql is the pre-D11 gate. D11 adds
seven procedures; D11Schema.sql verifies47typed parameters and the five-migration
chain. Gate scripts describe their own checkpoint (D09 excludes views, D10
excludes procedures); use the gate matching the migration being verified.
SP01–06 require a caller-owned active SNAPSHOT transaction. SP07 requires a
caller transaction, locks the year counter row/range, and never commits or
creates Invoice/audit/replay records. Report amounts are varchar(38) after
decimal(38,0) aggregation. Scope-miss/validation errors still require future
service mappings and current session authorization. Views are internal SQL
facts, without new endpoint/reporting grants. Race fixtures use finite local
diagnostic lock timeout, then rollback both connections. No committed fixture.

D12 creates four dbo-owned ec_* roles with exactly102object permission entries.
D12Schema.sql checks exact grants/DENYs, role ownership and six migrations; use
D11Schema.sql only after D12 Down. Down refuses while any of the four roles has
members. No login/user binding or runtime connection is installed by migration.
Permission tests use four WITHOUT LOGIN users and EXECUTE AS USER; users, role
memberships, fixtures and GRANT/REVOKE demonstrations rollback. Actual report/view
outputs and counter allocation are checked alongside rejected reads/writes/DDL.
ALTER/history-delete may hit owner-resolution15517 in this diagnostic environment;
zero ALTER/CONTROL/history-delete rights are checked independently. Impersonation
denial is tested against another isolated role member, avoiding Windows dbo lookup.
This does not prove login authentication, deployment identities, full runtime
readiness or service authorization. Those remain separate operator/D13 checks.
