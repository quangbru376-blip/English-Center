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
