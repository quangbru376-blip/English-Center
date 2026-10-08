# V01-N/O/P — remaining M0 runtime boundaries

2026-10-08 base main dd73266 / manager bc0d589. Owner repeated completing M0;
ADR177 covers bounded implementation/delegation/checks; ADR163 reviewed PASS apply.
Status IN PROGRESS. Codex manager; AGY sole pure maintenance-principal guard.
No active environment values inferred, M1–M4/packages/install/push/deploy/owner edits.

N: install explicit request deadline + bounded RAM body middleware; cancellation
propagated/restored, oversized known/streamed bodies413 before caller, deadline503
only before response starts; never infer rollback/auto-retry from cancellation.
No disk spill. Installed binding is created only by extension installing middleware;
ExecutionLimits probe compares bound descriptors; CSV response capacity/helper and
preview resource descriptor are explicit, future CSV/weekly callers remain separate.
No claim non-cooperative arbitrary handler can be forcibly stopped. Response helper
holds full bounded bytes before caller sends headers; no partial-file success.

O: direct local profile only; install exact HTTPS host + existing real CSRF boundary.
Transport probe checks installed actual antiforgery cookie/header options; binds
certificate/trust references and exact approved leaf SHA256, performs real socket
TLS1.2/1.3 handshake against exact loopback listen address/port and origin hostname.
Pin trust explicitly scoped to this client/cert, with independent SAN/time/server-EKU/
key-size checks; no system/browser global bypass or OS trust changes. Separate fresh
SQL connection must require Encrypt and TrustServerCertificate=False, actual open
through SqlClient (no query/DML). Existing I authority/catalog guard remains mandatory.
Missing/replaced ref/pin/options/listener/SQL trust fails closed. No LAN/proxy support
silently guessed: forwarded/nonlocal profile stays NotReady in this direct adapter.

P: explicit configured separate maintenance service/connection reference and two
scheduled job bindings; finite replay/audit intervals. Read-only readiness checks
fresh actual principal: no dbo/sysadmin/DB or server CONTROL/runtime/db_ddladmin,
known DELETE on AuditLog/ReplayOperation and UPDATE on IP expiry/address columns.
Unknown facts deny. Scheduler uses supplied independent principal, owned connection/
Predicate-column SELECT is required: Replay.expires_at and Audit.audit_category,
security_expires_at,ip_expires_at only; no broad SELECT. Direct extra server grants
also deny. Microsoft DELETE/UPDATE documentation confirms WHERE requires SELECT.
transaction, fixed parameterized cleanup: replay expired only; SECURITY expired only;
IP expiry clear only; never mandatory-history DELETE or arbitrary purge API. No
automatic mounting/scheduling from this task. Failed cleanup fixed counters only.
Retention clocks are accepted24h/90d existing stored cutoffs; no grace/default policy.

## Exact manager code paths (under backend)

src/EnglishCenter.Api/Infrastructure/Readiness/LocalSqlReadinessConnection.cs;
DatabaseReadinessProbe.cs (delegate unchanged preparation to shared helper);
ExecutionLimitsReadinessProbe.cs; TransportReadinessProbe.cs;
MaintenanceReadinessProbe.cs; Infrastructure/Maintenance/SqlMaintenanceService.cs;
Api/Runtime/ExecutionBoundary.cs; Api/Runtime/LocalTransportBoundary.cs.
tests/EnglishCenter.Tests/Readiness/RuntimeBoundaryTests.cs;
TransportProbeTests.cs; MaintenanceBoundaryTests.cs.
Worker sole Infrastructure/Readiness/MaintenancePrincipalPolicy.cs <=50lines.
No Program/frontend/API route/default/schema/migration/grant/login/config changes.
No actual cleanup executed against retained D07; live SQL remains unopened in this
first component contract. SQL command semantics require separately materialized
rollback test contract before cleanup can be called operationally verified.
Docs this card/NOP worker brief/P contract/sanitized evidence/notes/current memory,
LOCAL_DEV proposal and explicit runtime metadata/access review preparation.

## Skills and sharing

implementation_skills backend-clean-code; review_skills code-review (Codex).
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
Complete skills/routing/worker contract read; relevant accepted E04/requirements,
actual SQL roles/auth/CSRF/profile/readiness audited. applicable_overrides cookie
JWT/CSRF/SQL17Express/160/noSQLite/no new packages/API/SLO/defaults.
Worker read_context ONLY V01P_WORKER.md + complete backend skill + routing;
own output read allowed, no source/links/AGENTS/owner/secrets/search/listing/commands/
network/SQL/Git/validation. Current M0 assignment+ADR177 covers exact sharing;
no repeat permission gate. skill_verification pending actual events/diff/tests.

Verification: real middleware streamed/known caps/RAM cleanup/cooperative deadline/
restoration; explicit missing binding denial and buffer capacity; actual ephemeral
loopback TLS/SAN/expired/weak/wrong pin/options/ref/deadline (keys/certs only RAM);
Windows Schannel cannot use this ephemeral private key. Tests use existing Node
TLS via hidden child process, generated PEM through stdin only, OS-chosen loopback
port, no fixture files/trust-store writes; bounded process termination in finally.
maintenance unknown/unsafe principal/config/job intervals/ref/cancellation/SQL source
guards, no fake positive SQL. Manager/main full default tests/build --no-restore
-warnaserror; owner hashes/source-scope checks. No actual operator profile/startup/
cleanup/SQL-principal/PKI/whole-M0 readiness claim from these fixture tests.
