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
