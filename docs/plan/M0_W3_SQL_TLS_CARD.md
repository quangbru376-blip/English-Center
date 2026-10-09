# M0 W3 — SQL Server certificate and verified local trust

2026-10-09; assigned whole M0 ADR177/LOCAL_DEV ADR186/ADR163 reviewed apply.
Codex-only privileged secret-bearing operation, no external delegation/context.
W1/W2 actual identities/catalog PASS; W2 diagnostic TLS is not readiness.

Exact tools/operator/Initialize-M0SqlTls.ps1 via UAC, current owner/SID and fresh
administrator token, W1/W2 ownership required. Target only MSSQL$SQLEXPRESS,
MSSQL17.SQLEXPRESS SuperSocketNetLib Certificate. Current configured Certificate
empty, ForceEncryption0, service running NT Service\MSSQL$SQLEXPRESS. Fail on drift.
Create one LOCAL_DEV SQL self-signed server certificate LocalMachine\My, CN machine,
SAN machine+localhost, RSA2048/SHA256/server-auth EKU/legacy RSA SChannel CSP,
AT_KEYEXCHANGE/non-exportable/valid one year. Exact private-key file ACL protected:
SYSTEM/Administrators FullControl, SQL service SID Read only. No key export/PFX.
Trust only this certificate's public bytes in LocalMachine\Root (direct leaf trust),
no broad CA/import/download. W1 runtime/maintenance use machine trust. Snapshot old
certificate/ForceEncryption/service before writes in protected owned sql-tls.json;
record own thumbprint/file ownership; no secrets in Git/chat/evidence/args.
Set exact certificate registry value then restart only SQL service once. Do not
alter auth mode/ForceEncryption/network/protocols/ports/service identity/grants/data.
Restarts briefly affect instance connections; D07 data/baseline preserved. Failure
restore exact prior registry value/restart prior service, remove only owned leaf
Root trust entry; retain owned My key/certificate+fixed failure status for inspection.
No arbitrary store deletion or non-owned certificate/private-key mutation.

After restart, actual strictly encrypted connections without -C must work under
runtime/maintenance/schema identities; no positive readiness until modern shipped
SqlClient+production transport/database probes and all9 actually pass. Reverify
D07 byte baseline/owner4, exact service/config and owned key ACL/trust. HTTPS/JWT/
CSRF/runtime mounting are subsequent scoped work, not implicitly done by SQL TLS.

Primary verified SQL17 requirements: Microsoft Learn certificate-requirements and
configure-sql-server-encryption pages, 2026-10-09. Server auth EKU, validity,
legacy CSP AT_KEYEXCHANGE, SQL service key read and SAN client names mandatory.
https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/certificate-requirements?view=sql-server-ver17
https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/configure-sql-server-encryption?view=sql-server-ver17
implementation_skills backend-clean-code; review_skills code-review.
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs/requirements/E04runtime/W1W2 receipts,
actual readonly registry/service/SQL strict failure, official Microsoft pages.
applicable_overrides real SQL identity/TLS/custody, no packages/default bypass.
skill_verification pending AST/independent source review/actual TLS acceptance.
No M1–M4/install/push/deploy or owner4 content/sharing/staging.

Pre-execution source review PASS: AST0errors, actual owner/elevation/registry/service
guards, fixed certificate/key-path checks, exact ACL principals and service Read
only, public-byte direct leaf trust, snapshot before mutation, bounded exact
service restart and restore-on-failure. No private exports/secret args or authority
grants. Actual SQL TLS and baseline acceptance remain pending run.
