# V01-P read-only SQL grammar verification

2026-10-08. Necessary independent review under assigned full M0/ADR177.
Codex only, no AGY context expansion. Existing .\\SQLEXPRESS and retained
EnglishCenter_D07_20261005 only. Before/after execute unchanged baseline.sql.
Extract the exact fixed PrincipalSql SELECT from SqlMaintenanceService.cs into
ignored test-results/P-principal.sql; run once with installed SQLCMD -E -C -b.
SQLCMD diagnostic certificate trust is only the operator diagnostic connection;
it does not replace strict runtime transport or prove maintenance authority.

Allowed SQL: baseline read-only catalog/profile/table-count queries and exact
maintenance principal SELECT. No DML, cleanup, BEGIN/COMMIT, EXECUTE AS, user,
role/grant/login/schema/migration/config/other database changes. Nine returned
facts must parse and reject current privileged diagnostic identity. Record only
sanitized fact counts/status and before/after file digests, no identity/secrets.

This verifies actual SQL syntax/catalog functions and privileged denial only.
Real least-privileged authority, cutoff cleanup effects and scheduled operation
remain unverified until their separate operator/fixture contract is materialized.
Skills/backend/review/routing versions and overrides are those in V01NOP_TASK_CARD.
