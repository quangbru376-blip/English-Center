# V01-V — explicit post-listener startup readiness

2026-10-09. Assigned M0/ADR177/163. Codex manager only, no external sharing.
Exact code paths Api/Runtime/StartupReadinessService.cs and tests/Readiness/
StartupReadinessTests.cs. Small conventional BackgroundService, supplied existing
RuntimeReadinessBoundary and IHostApplicationLifetime. No default registration,
Program/DI configuration discovery, selected paths/ports/budgets/secrets/SQL/provisioning.

Wait for ApplicationStarted before checking all9 dependencies: transport proof
requires a listening HTTPS endpoint. Existing Q gate still checks every API call;
startup success grants no later request authority. Link host stop and application
stopping tokens, dispose event registrations, abort pending/uncooperative work via
Q finite waits, sanitized failure requests StopApplication. Only fixed refused
boolean diagnostic; no ready stamp, bypass, retries or secret/exception diagnostics.

implementation_skills backend-clean-code; review_skills code-review. Complete skills
read, skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs/requirements, E04 runtime/Q boundary/ADR183,
actual host lifecycle APIs/tests. applicable_overrides cookie/CSRF/same-origin,
SQL actual evidence and fail-closed all9 retained; no new package or profile defaults.
skill_verification complete: two-file182line diff, cancellation/registration disposal/
fixed redaction/no cached authority reviewed, actual lifecycle checks below.

Verify explicit actual WebApplication/TestServer registration: probe runs only after
ApplicationStarted, each dependency failure stops host, passing startup is not cached
across later API drift. Unit lifecycle checks cancellation before start/during source,
source deadline/redaction. Synthetic probes prove lifecycle only, not actual readiness.
Manager/main build --no-restore -warnaserror/full tests, owner4hashes unchanged;
no SQL rerun. Docs this card/notes/V summary and affected memory/architecture/closure.
Full M0 remains incomplete pending selected actual environment/protected custody.

## Independent review PASS/applied/reverified
Main4daa2e2 / manager22ed355.14targetedPASS; manager/main1217defaultPASS/
63intentional opt-in SKIP/build0warnings0errors. Initial build caught nullable
Task generic in test; corrected before checks, no suppression. All9 dependency
failures request StopApplication only after ApplicationStarted; successful startup
does not cache later API authority, changed profile still503 through Q gate.
Stop before started/during uncooperative source completes without refusal; unavailable
source/deadline refuses with only fixed boolean/name diagnostic, no exception content.
Explicit host registration exercised by TestServer; no actual HTTPS/SQL/key proof.
Source/tests871e83635832a8e337035bb725ea7f8991d0164a /
632c1f97b5aa7133cb56a413f2bd4791a663b13b identical. SQL not rerun; owner4hashes
unchanged/untracked/unstaged/unshared. No default host/route/DTO/schema/package change
(review N/A); API docs describe only optional startup hook. Notes<=15lines. Host
graceful-stop request verified, not a particular process exit code or production PKI.
