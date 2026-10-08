# V01-U — maintenance scheduler lifecycle

2026-10-09. Whole-M0 assignment/ADR177/163. Codex manager; small local verification
only, no worker/context sharing. Exact code path backend/tests/EnglishCenter.Tests/
Readiness/MaintenanceSchedulerTests.cs; production changes require a documented
failure and scoped card amendment before editing. No SQL/network/packages/defaults.

Exercise actual SqlMaintenanceService.StartAsync/StopAsync/ExecuteTask and its two
PeriodicTimer loops with a test-only manual TimeProvider/ITimer. Resolver returns
missing material or a pending RAM task; never supplies a connection. Verify distinct
explicit periods, no initial run, independent failure accounting, fresh later tick,
single in-flight run per job/coalesced ticks, shutdown despite uncooperative resolver,
late fault observation, invalid schedule refusal and timer disposal. Test time/values
are synthetic, not approved profile or positive scheduled SQL/auth/TLS evidence.

implementation_skills backend-clean-code; review_skills code-review, already fully
read in this conversation and SHA256 unchanged. skill_sources backend
726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs, requirements ADR116, E04 runtime, T card,
actual service/maintenance tests/profile fixture. applicable_overrides unchanged
cookie transport/SQL-specific evidence/90d-IP mandatory-retained/24h-replay; no SQLite.
skill_verification pending actual diff/build/tests/review.

Manager/main build --no-restore -warnaserror, targeted new tests and full default
suite, exact source/test trees, owner4hash preservation. No SQL rerun for test-only
change. Docs this card/notes/U summary, affected CURRENT_STATE/TODO/handoffs/closure
and M0 completion/environment status. Full M0 remains incomplete until real bindings.
