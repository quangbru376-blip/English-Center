# M0 LOCAL_DEV operational closure

2026-10-09 owner selected the concrete LOCAL_DEV proposal; ADR186 records table
values. Assigned M0/ADR177 covers required bounded operational work, ADR163 PASS
application. No M1–M4/install/push/deploy, owner-file changes or secret sharing.

Preflight scope Codex only, read-only: Windows process elevation boolean/local
account cmdlet presence, SQL Windows-only versus mixed authentication/configured
encryption, existing localhost certificate presence, instance/service availability,
strict certificate-verifying local SQL connection attempt. Print only fixed booleans/
error categories; no credentials/private keys/connection dump/identity listing.
No login/OS account/registry/auth-mode/certificate/trust-store mutation in preflight.
Actual provisioning needs a concrete derived contract after preflight, not guessing
an operator identity or elevating runtime into owner context.

Remaining actual bindings: runtime Windows identity without SQL elevated authority,
separate schema/maintenance contexts, exact declared metadata reads/narrow cleanup
grants, HTTPS and independent SQL trust, protected JWT/current-user DPAPI CSRF store,
approved artifact manifest, finite preview resource budget and actual host/UI wiring.
Do not generate operational keys before identity/custody is bound. Missing values/
authority keep business requests closed. No positive readiness inferred from fixtures.

## Verified read-only preflight
Outside sandbox, current manager process AdministratorToken=false; SQL service
running, Windows-only authentication, current diagnostic SQL sysadmin=true.
No-C/encrypted SQL certificate validation fails with untrusted certificate-chain
category. Port8443 has no listener. One current-user localhost/server-auth-EKU
private-key certificate candidate exists; no key exported/opened, custody/trust
not proved. Initial sandbox certificate/service observation was restricted and is
not the authoritative outside-sandbox result. No provisioning/configuration changes.
Raw fixed SQL outputs in ignored test-results/operator-*.txt; no identity listing.

Actual Windows account provisioning and SQL server certificate/private-key ACL/
service configuration need an elevated Windows manager process. Current process
cannot supply that authority; no permission question substitutes for Windows UAC.
Required operator action: restart Codex with Run as administrator, reopen this
workspace/chat and resume. Runtime/maintenance application identities must still
be non-administrative, separate from the elevated provisioning manager.

After elevation, prepare/review exact identity/custody/SQL-certificate contract
before writes. Preserve D07 baseline; prefer a dedicated operational catalog with
existing migrations rather than changing the retained verification baseline.
No service/auth-mode/global trust changes were made by this preflight. Full M0 open.

W1 supersedes the restart-as-administrator blocker above: bounded per-operation
UAC proven and W1 identities/custody actually PASS; see M0_W1_IDENTITY_CARD.
Actual runtime SQL/SQL TLS/key stores/host binding still pending, not ready.

W2 actual catalog/identity PASS (ADR187): existing6migrations/new owned catalog,
genuine SSPI880runtimefacts/maintenance229denial. TLS still unverified; no runtime
host or keys mounted. Exact SQL cert contract W3 next.

W3 supersedes strict-SQL trust blocker above. Sole LOCAL_DEV SQL certificate
bound/trusted with service key Read only, actual modern driver runtime database
and maintenance probes PASS, schema no-C required-encrypted connection PASS.
Operational modern LPC connection explicitly HostNameInCertificate=localhost;
TrustServerCertificate=false. Other7 probes/runtime web keys/mounting still pending.

W4 actual key custody/restart verification PASS (ADR188): runtime DPAPI JWT/CSRF
and own non-exportable HTTPS key/leaf trust; maintenance/schema reads denied.
Next actual artifact/profile/all9/startup/maintenance identity orchestration/API/UI;
no positive whole-host readiness yet. Defaults remain unmounted.

W5 A/B supersedes missing profile/maintenance dependency above (ADR189): exact
installed profile/5pins/strict SQL version and native key reader actual4PASS;
separate actual maintenance host/3fresh signed proofs/stopped-worker refusal3PASS.
No cleanup reached before20s cancellation (first tick60s). Full host/API/UI and
actual scheduled COMMIT remain required. Next exact M0_W5C_HOST_CARD before code.

W5C supersedes unmounted API/UI/all9 above (ADR190): explicit genuine-runtime
HTTPS host composes existing M0 adapters/UI and all9 fresh probes; actual readonly
ready/CSRF/UI isolation PASS with real maintenance counterpart. No browser JS/auth
mutation/operational Admin or scheduled SQL COMMIT in this case. Full M0 open;
next scoped bootstrap/browser owned-COMMIT/maintenance/usable operator walkthrough.