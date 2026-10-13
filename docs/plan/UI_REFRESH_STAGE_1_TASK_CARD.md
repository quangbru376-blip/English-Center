# UI refresh giai đoạn1 — chuẩn bị và đóng băng hành vi

Owner13/10/2026 giao hoàn thành giai đoạn đầu; active plan UI_REFRESH_PLAN.md.
Giai đoạn1 chính là giai đoạn0 trong kế hoạch gốc. Không redesign trong task này.
Branch ui-refresh từ26067e0; existing owner edits/untracked retained, không reset,
stage hoặc commit chúng. Product frontend treeb2fb22a757468e0f29aa6adc8269cec48c3e24bc.

implementation_skills: frontend-ui (tooling/inventory); review_skills: code-review.
skill_sources: .agents/skills/frontend-ui/SKILL.md SHA256
448079D6EF3B5B514BBCBA4F2A8D5AAF0FC6CC8063F79798F28A6DBBE5128F90;
code-review/SKILL.md SHA25604B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70.
applicable_overrides: SKILL_ROUTING canonical cookies/CSRF/SQL/exactmoney/no packages;
new owner UI decisions override stale visual direction. No worker/delegation.
read_context: AGENTS/memory/requirements/plan + existing frontend source/tests,
existing OwnedBrowserSqlHostingTests/OwnedSqlFixture and baseline fixtures.

Allowed writes: UI_REFRESH_PLAN/INVENTORY/STAGE_1_TASK_CARD/STAGE_1_REVIEW,
UI-refresh-evidence, docs/notes/UI_REFRESH_01.md, concise memory headers,
frontend/tests/browser/ui-refresh-baseline.cjs, ui-refresh-source-manifest.cjs
and ui-refresh-gallery.cjs.
Forbidden: frontend/src, package manifests/locks, backend source/tests/migrations,
default host/operational profile/key custody/D07, owner supplied files/runbook edits.
Generated frontend dist/TestResults/backend bin/obj are validation outputs only.

Outcome: six sizeable execution stages/four output gates;32F inventory with F15
removed; exact route/view/component/state mapping, proposed approved-scope initial
reads/menu/recovery; baseline tests/source selector hashes and synthetic before
screenshots from unchanged real Edge/HTTPS/SQL harness. No implementation PASS claim.

Verification commands (repo root unless stated):
- frontend: npm run build; npm run lint (build includes typecheck).
- frontend tests: existing U06 recipe with all src TS/TSX excluding main CSS entry,
  compiled CommonJS to frontend/TestResults/UI-refresh-baseline, EC_TEST_BUILD and
  NODE_PATH=node_modules, node --test tests/*.test.mjs. No new test framework.
- dotnet test backend/EnglishCenter.sln --no-restore, logger/results under evidence.
- frontend: EC_V01_ASSETS=<current environment TEMP>/englishcenter-ui-refresh-assets-20261013,
  node tests/browser/build-fixture.mjs. Sandbox and unsandbox TEMP differ; assets must
  be created in same execution environment as acceptance runner.
- Browser: existing EC_V01_SQL=1/EC_V01_OWNED_SQL=1/EC_V01_BROWSER=1;
  EC_V01_E_RESULTS=<repo>/docs/plan/UI-refresh-evidence/owned;
  EC_M1_DRIVER=<repo>/frontend/tests/browser/ui-refresh-baseline.cjs;
  EC_V01_DRIVER=<repo>/frontend/tests/browser/verify-identity.cjs;
  EC_PLAYWRIGHT=<bundled dependency runtime>/node_modules/playwright;
  existing project --no-build --no-restore, filter OwnedBrowserSqlHostingTests
  groups81–88 plus M2/M3 functions, M4ReportsAuditRealEdgeThreeWidths separately.
- frontend: node tests/browser/ui-refresh-source-manifest.cjs; screenshots manifest
  digest/viewport/group/role/route/headings only, passwords masked. No raw payload logs.
- Final review: git diff --check; compare source/package hashes; ownership receipts
  created/dropped exact ID/GUID pairs, independently query remaining owned catalogs
  and listeners after acceptance. No wildcard cleanup or primary database access.

Scope authorization covers synthetic isolated fixture COMMIT/browser effects to
finish baseline, using existing6migrations and fixture cleanup. Prior actual runtime
evidence is retained, not rerun or promoted. SQL/shared-memory diagnostic fixture
uses its existing TLS rules; browser binds only fixture certificate pin with negative
untrusted/wrong-pin tests; this is not production key/trust servicing.

Record initial failures honestly: frontend compile incorrectly included main.tsx
without Vite CSS types; corrected harness scope/no product fix. First sandbox .NET
execution denied Windows runner access and was cancelled; unsandbox final run used.
First browser attempt had mismatched TEMP assets; owned DB was dropped and corrected
assets built in runner environment before final run. Final results belong in review.

Stage1 ends at a concrete cổng1 review package. Owner UI_REFRESH_PLAN requires
approval of inventory/interaction before phase2; this is output acceptance, not a
repeat of A/nav/rights/package choices. Missing required capture remains incomplete
until resolved; don't claim screenshots prove every possible state/validation case.
