# M0 W5C — actual runtime/API/UI composition
2026-10-09; finish assigned M0 ADR177, PASS apply ADR163, LOCAL_DEV ADR186.
A/B actual PASS retained; W5C not implemented. No M1–M4/install/push/deploy/owner4.

Exact backend scope: Program.cs explicit single --m0-local-runtime branch;
Infrastructure/Operator/LocalRuntimeHost.cs, LocalRuntimeServices.cs,
LocalRuntimeReadiness.cs. Optional changes to existing local bindings only for
verified integration defects, recorded before edits. Genuine non-admin EcM0Runtime
only, exact fresh protected profile/current own keys, no defaults/provisioning,
seeding/migration/repair/fallback credentials. Session owns host/key/provider disposal.
Kestrel127.0.0.1:8443/owned localhost certificate only, no forwarding.

Actual nine dependencies: fresh loaded patch descriptor/5pins/NET+ASP+EF+SQL;
BCL Vietnam time; runtime-only strict SQL7facts+catalog880; actual live HTTPS/SQL
and real CSRF options; own JWT sign/validate; existing encrypted CSRF store;
pinned offline corpus; installed execution limits; separate live signed maintenance.
Fresh profile and all9 per API/readiness request; actual post-listener startup gate
stops host on failure, never bypasses later Q. Health /health/live and /health/ready
GET/HEAD no-store fixed bodies. Missing/drift/deadline stays503/closed.
DP protection current-user/application EnglishCenter.M0.Local/existing filesystem
keys with auto-generation disabled, no startup/probe write. SQL factories exact owned
M0 catalog/integrated SSPI/strict TLS/EF160. Each production executor owns fresh
caller transaction/COMMIT; independent security audit retains fixed counter fallback.

Mount exact existing M0 endpoints only: csrf/register/login/me/logout/change-password,
Admin account detail/lock/manual command-result lookup. Existing live current-session
CSRF identity/restricted scope/native RV/replay/mandatory audits retained. No new
business routes/roles/fixtures/passwords in this host. Routing precedes metadata gates;
error middleware covers body/readiness/transport/auth JSON checks. No token/SQL dumps.
Local IP resolver uses real loopback RemoteIpAddress only, never forwarded headers.
No CORS or automatic retries, same-origin cookie only. Limits3000/10/30000/25000/5000,
262144/5000000/10000 selected, CSV/weekly business routes remain outside M0.

Exact frontend scope: src/app/App.tsx mounts existing IdentityApp with stable
instance-owned AuthState/HttpClient. IdentityApp.tsx comment/status only if needed.
Existing pages/components/signals/guards/raw credential behavior reused; no packages.
Existing main entry/css kept, production build served same-origin from frontend/dist
only (local non-reparse physical root), never workspace/owner files. Unknown API
cannot receive SPA HTML through policy bypass. Existing UI routes only.

Codex implementations/review only for composition/private operational context.
No new AGY payload/context yet; if delegated, prepare exact independent nonsecret
contract before dispatch. Existing sole codec worker context remains as audited.
Tests derived exact backend LocalRuntimeHostTests.cs, ActualM0RuntimeHostTests.cs;
frontend existing tests/typecheck/lint/build and actual Edge375/768/1280 checks.
Initial actual acceptance readonly host/readiness/CSRF negatives only; no operational
Admin/roles or business seeds. Derive separate explicit owned bootstrap/real browser
COMMIT/scheduled maintenance contract before operational business writes. Full M0
requires those additional actual checks and owner walkthrough, no fixture promotion.

implementation_skills backend-clean-code/frontend-ui; review_skills code-review.
Complete skills read, routing/canonical JWT cookie/SQL Server/DTO/money overrides.
Hashes backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
frontend448079D6EF3B5B514BBCBA4F2A8D5AAF0FC6CC8063F79798F28A6DBBE5128F90;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/requirements/E04/runtime/W1–W5/M0 adapters/owned executor
source/earlier synthetic browser fixtures as API wiring reference only.
skill_verification pending source review/build/tests/actual runtime/browser evidence.

Derived local key-binding integration hardening before C edits: extend existing
LocalRuntimeKeyStore.cs custody snapshot/ACL checks to installed profile/patch
manifest and existing CSRF XML set/directory. Reject add/remove/change/broadened
custody during host lifetime, no automatic key/profile reload. This prevents
cached framework/signer state diverging from fresh readiness files; operator rotation
requires explicit reviewed host restart. Existing references/execution unchanged.

Derived exact LocalUiFiles.cs readonly provider: pin only current built files under
frontend/dist, bounded128files/depth4/selected5MB per file; reject reparse entries
before recursion, no directory listing/watch/physical-path shortcut. File reads
verify initial SHA/length under no-write/no-delete handle before serving. Fresh
runtime-manifest category also checks UI set/hash drift. Prevent workspace/owner
file exposure through later symlink/file replacement; changes require restart.

Exact LocalUiFilesTests.cs derived before tests: hash drift/add/remove/path escape/
private extension refusal on disposable local files, no owner contents. Static
HTML/assets no-store; provider rejects report/docx/md/json/map and reparse entries
before opening. Both files/directories bounded128. No extra library/test packages.

Initial actual test exact RealHttpsHostChecksAllNineAndServesOnlyBuiltUiReadOnly:
bounded15s real EcM0Runtime host plus bounded20s actual maint counterpart; HTTPS
ready/live/CSRF token-cookie/UI/source-path isolation/unmounted API503/query400,
no auth denial or mutation endpoint (avoids independent security-audit writes).
Token/cookie only RAM/no sensitive assertion output. No setup seeds/SQL writes.

Runtime composition forces Production environment (no inherited Development
exception page); cleared logger providers prevent request/key/SQL payload output.
Existing frontend tests emitted to ignored temporary CJS directory with type marker
only, no project dependency/package manifest changes or installation.

Intermediate source review PASS after middleware-order/Production/custody/UI-provider
fixes: manager1263defaultPASS78intentionalSKIP/build0warnings0errors; frontend48tests/
lint/types/build PASS. No actual all9 host yet; readonly genuine-profile acceptance
requires applied main binaries/UI build. No operational business writes approved
by this initial test contract; later separate fixture-free closure scope pending.

First actual runtime attempt proved all9 ready/HTTPS/CSRF/UI before a test expectation
mismatch: /docs/PROJECT_CONTEXT.md correctly404(empty), not SPA HTML (fallback
excludes file-extension paths). Corrected isolation assertion to404/empty; no source
exposure/production fix/SQL mutation. Whole actual C test remains pending recheck.

W5C supersedes unmounted API/UI/all9 above (ADR190): explicit genuine-runtime
HTTPS host composes existing M0 adapters/UI and all9 fresh probes; actual readonly
ready/CSRF/UI isolation PASS with real maintenance counterpart. No browser JS/auth
mutation/operational Admin or scheduled SQL COMMIT in this case. Full M0 open;
next scoped bootstrap/browser owned-COMMIT/maintenance/usable operator walkthrough.