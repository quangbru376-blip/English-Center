# V01-D — browser HTTPS to rollback SQL

2026-10-08. Base main3d35cc7; status: implementation/execution authorized ADR177.
V01-B/C remain applied PASS. This is the next verification substep of V01.
Codex owns the host, SQL transaction, browser driver and independent review.
No new product workflow or production runtime profile is opened by preparation.

## Implementation contract

Reuse the retained V01 HTTP/SQL manager worktree, preserving original commits.
Manager may change ONLY IntegrationSqlHost.cs and add BrowserSqlHostingTests.cs
under backend/tests/EnglishCenter.Tests/Api/Integration; frontend/tests/browser/
identity-entry.html, identity-entry.tsx, build-fixture.mjs, verify-identity.cjs;
and this card, V01D_WORKER.md, D-worker-contract.json, sanitized D evidence/notes
and affected AGENTS/DECISIONS/CURRENT_STATE/TODO/handoffs. No production src/default entry,
project/package/lock/config/schema changes. Build with existing dependencies,
--no-restore; frontend fixture output is temporary, not a deploy artifact.

IntegrationSqlHost keeps the existing default TestServer mode and27cases.
Add an explicit browser mode whose origin is the actual selected HTTPS loopback
port; preserve exact Origin comparison, headers and framework CSRF. No Origin/auth-cookie/header rewriting, mock API interception or alternate auth policy.
Mount the same implemented endpoints and services with the existing caller-TX
SQL bridge. Serve a fixture entry that instantiates IdentityApp/HttpClient/AuthState
using relative /api/v1 requests. Production App/main/Program remain unmounted.

## Authorized execution scope — ADR177

One test process owns Kestrel and a child Node TLS terminator, both bound ONLY
to127.0.0.1 on OS-selected ports,
temporary static frontend assets and headless installed Edge via bundled
Playwright. No LAN/public bind, tunnel, production proxy/firewall change or downloads.
Generate a short-lived localhost certificate/private key and JWT/CSRF keys
in RAM. Certificate SAN must match127.0.0.1; trust only its exact public-key pin
in the isolated browser process. Verify the loaded certificate fingerprint and
the browser pin mechanism before application traffic. If pinning cannot be
verified, stop; no ignoreHTTPSErrors/global certificate-error bypass or system
trust-store changes. This tests fixture TLS transport, not production PKI.

Only lpc:.\\SQLEXPRESS/EnglishCenter_D07_20261005, diagnostic Windows connection,
compatibility160, existing24tables/4roles102permissions/6migrations/snapshotON.
Only synthetic Role/Account/StudentProfile/AuthSession/AuditLog/ReplayOperation
rows in one outer transaction per scenario; serialized per-operation savepoints,
rollback ALL at teardown, including failures/cancellation. No owned executor
COMMIT, independent persisted security audit, principals/users/memberships,
DENY/GRANT, schema/migrations, real credentials or other database.
Generated passwords/keys/tokens travel through RAM/anonymous stdin only, never
CLI arguments/files/console/trace/HAR/screenshots. Evidence keeps statuses/counts/
metadata and hashes only; cookie values/passwords/bodies/headers stay unrecorded.
The managed diagnostic connection is NOT least-privileged runtime-principal proof.

## Ten browser scenarios and acceptance

1. Real signup201 creates Account/Profile/mandatory audit; no auth cookie/session;
   transition to login only. Verify at375/768/1280 without page horizontal scroll.
2. Seven-role deliberate login/me: real Secure/HttpOnly/Strict/path=/host-only
   cookies, nine-field response and matching fresh SQL identity; no JWT in JS.
3. Authenticated mutation without CSRF and with mismatched Origin403; no effects.
4. Same browser-profile tabs share cookie state; current-SID logout clears both
   on their next server check and leaves a separate browser-profile SID valid.
5. Normal password change204 revokes all own SIDs, clears auth, returns to login;
   old cookies rejected, deliberate new login required; no automatic upgrade.
6. Restricted password change has the same revocation/no-auto-login boundary;
   restricted identity cannot request Admin data or mutation.
7. Wrong current password400 and stale RV409 preserve hash/version/sessions/audit.
8. Admin detail/confirmed one UUID lock: target status/version/session revoke,
   mandatory audit and T23; matched replay adds no effects/activity/second marker.
9. Manual safe lookup returns exact completion reference; missing result404
   remains unknown, without automatic write retry or rollback inference.
10. Other six roles deny Admin access; storage remains empty and auth/CSRF request
    state RAM-only; page errors absent; default opt-out opens no SQL or socket.

Read-only baseline.sql before/after must match B/C digest and24empty tables,
0fixture users/members; abort on unexpected baseline. Always stop browser/server
and rollback/dispose before reporting. Re-run B/C27cases only if explicitly
included in execution approval; default unit suite must remain SQL/socket-free.
Apply only independently reviewed PASS implementation under standing ADR163.

## Bounded delegation and skills

Optional AGY sole pure cookie/storage metadata assertion<=70lines, as specified
by V01D_WORKER.md/D-worker-contract.json. External context exactly that worker
card, frontend-ui/SKILL.md and SKILL_ROUTING.md; no source/report/owner files,
secrets, commands/browser/network/SQL. Exact new payload is authorized ADR177;
new standing task authority applies, not prior B/C consent. Validate contract before dispatch on clean manager.
implementation_skills: backend-clean-code/frontend-ui; review_skills: code-review.
skill_sources SHA256: backend726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b;
frontend448079d6ef3b5b514bbcba4f2a8d5aaf0fc6cc8063f79798f28a6dbbe5128f90;
review04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70;
routingadb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915;
AGYeb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b.
Complete selected skills/policy/AGY contract read. applicable_overrides: accepted
requirements/E04 exact DTO/cookie JWT/framework CSRF/live rights/RAM/native44px/
SQLServer160; no bearer/SQLite/ReactQuery/package addition or business-rule change.
read_context: ordered memory/handoffs/requirements, E04_RUNTIME_PROFILE,
M0_LOCAL_PROFILE/V01 cards, actual IntegrationSqlHost/Program/IdentityApp/cookie policy.
skill_verification: source audit confirms actual gaps; no runtime/SQL/browser run
or worker dispatch in preparation. Contract review only, not implementation PASS.

## Remaining gates after D

V01-E must separately define owned COMMIT cleanup/recovery authority, separate
SNAPSHOT executor, independent persisted security audit and multi-client races.
Runtime-principal/login provisioning, complete operator profile/key persistence,
production certificate trust/readiness and DF prerequisites stay deferred.
D success alone cannot close V01/M0. No restM0/packages/install/push/deploy.

## Implemented TLS fixture refinement — ADR177

Initial Windows Kestrel RAM certificate handshake failed SEC_E_NO_CREDENTIALS
8009030E before application traffic. No key persisted/imported to fix it.
Node HTTPS now holds the generated key/certificate in RAM and forwards to a
private loopback Kestrel endpoint. It preserves original Host/Origin/auth headers,
strips incoming Forwarded/X-Forwarded/X-V01-Bridge fields and supplies a random
RAM-only bridge nonce. Kestrel checks loopback+constant-time nonce before assigning
HTTPS scheme; direct calls without proof400. This is test-only trusted termination,
not production forwarding/readiness proof. Both listener/process lifetimes belong
to each rollback scenario. Negative default-trust/wrong-SPKI browsers reject the
certificate; only exact-pin browser succeeds. Node independently verifies public
certificate DER SHA256/SPKI and trusts only supplied public certificate/SAN.
Group9 drops the real response after upstream processing, without intercepting or
fabricating API output; manual safe lookup confirms the same completion.
A rejected route.fetch prototype emitted ephemeral fixture headers in failed
local diagnostics. Header lines were redacted from its ignored TRX; final driver
avoids route.fetch, catches asynchronous failures and C# whitelists checkpoint
metadata, discarding raw stderr. No token/password/keys enter committed evidence.
## Manager review — PASS2026-10-08

Final clean build0warnings0errors;37integration cases PASS/0fail/0skip (D10+B/C27).
Default891PASS/37intentional opt-in SKIP/0fail; skips are not counted as PASS.
Frontend48tests/0fail/0skip +typecheck/lint/build PASS. Both negative trust probes,
wrong pin, direct unauthenticated bridge400, real signup3widths and lost-response
completion lookup execute against actual fixture, without mock API responses.
Baseline catalog/profile/permissions digest unchanged;24empty tables/users/members0.
AGY49915a6c-e94 SUCCESS/full3reads/sole53line helper/zero observed scope violations;
Codex reviewed events/diff and independently integrated/tested. Original retained.
Source/backend/frontend entries/packages/schema unchanged; API/migration docs N/A.
Owner explanation docs/notes/V01D.md11lines. Windows TLS prototype/header diagnostic
failure and build-lock retry corrected; final clean assembly reverified37PASS.
Apply reviewed bounded stage ADR163, then reverify main. Full V01 remains incomplete.
