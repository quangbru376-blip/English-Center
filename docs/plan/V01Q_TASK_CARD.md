# V01-Q — explicit HTTP readiness boundary

2026-10-08 base main5ddcd44/managera853149. Continue full assigned M0, ADR177/163.
Codex manager implements this small boundary, no external delegation/context.
Status IN PROGRESS. Operator selection remains pending; no guessed profile defaults.

Exact code scope: backend/src/EnglishCenter.Api/Api/Runtime/RuntimeReadinessBoundary.cs
and backend/tests/EnglishCenter.Tests/Readiness/ReadinessBoundaryTests.cs.
Docs: this card, docs/api.md, notes/V01Q.md, Q evidence, current memory/handoffs.
No default Program/DI/frontend/schema/grants/SQL/logins/OS trust/packages/push/deploy.

Contract: explicit existing RuntimeReadinessGate and fresh typed profile source;
positive finite separately supplied profile-load deadline. Null/error/late fault/
timeout fail closed, caller cancellation propagates. No cached successful readiness.
Evaluate all existing mandatory probe categories through unchanged gate, no descriptor
or synthetic production-positive shortcut. Optional explicit installation receives
two distinct canonical non-API health paths; no default routes selected here.
GET/HEAD readiness literal ready/not_ready200/503 with no-store; liveness alive200,
no dependency reads, secret/config/status details or idle/activity mutations.
Other health methods405 with Allow GET, HEAD. Health paths exact match, cannot
overlap /api/v1 or carry query/fragment/traversal. Every /api/v1 request checks fresh
readiness and throws existing sanitized SERVICE_UNAVAILABLE before downstream when
unavailable. Non-API requests pass through; health has no business authority.
RequireReadyAsync is an explicit operator/startup check, not a permanently cached
stamp. Real transport probes need the known HTTPS listener alive; application writes
remain behind this boundary, never expose a business route while readiness is false.

implementation_skills backend-clean-code; review_skills code-review, complete read.
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
applicable_overrides E04 literal health/no activity/cookie/CSRF/same-origin/API001
errors, real SQL only when authorized, no SQLite/new package/route catalog rewrite.
read_context current memory/requirements/E04 runtime/gate/error framework/tests.
skill_verification pending independent scope/build/test review.

Verification: actual HTTP TestServer composed middleware; health methods/no-store/
HEAD/no details, all9 wiring, fresh reload/drift, API pre-caller denial, /api/v10
not confused, resolver cancellation/timeouts/late faults, invalid health-path refusals.
Synthetic probes prove HTTP wiring only, never active runtime/M0-ready evidence.
Full manager/main build --no-restore -warnaserror/tests and owner hashes unchanged.
