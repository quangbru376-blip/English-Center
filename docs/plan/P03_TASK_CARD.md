# P03 — frontend skeleton

Date: 2026-10-04. Status: reviewed PASS after manager corrections; detached, awaiting owner main application.
Manager: Codex. Implementer: Antigravity via antigravity-worker.
Owner explicitly authorized this individual P03 task after P02 main application.
This supersedes the initial exclusion of P03 only for this bounded skeleton.
No SQL, later M0 tasks, business pages, deployment or main application authorized.

## Objective and prerequisite evidence

Create the conventional React/Vite/TypeScript/Tailwind skeleton with real build,
lint and typecheck scripts. P02 applied/reverified on main df7cd9a; current clean
preparation HEAD0646be1. Actual dispatch base is recorded in the validated
English contract after committing this task card.
Node24.16.0/npm11.13.0 verified. Exact registry metadata/engines/peer ranges and
official React/Vite/Tailwind/typescript-eslint documentation checked2026-10-04.
TypeScript7 latest is outside typescript-eslint's >=4.8.4 <6.1 range: choose
compatible6.0.3; @types/node24.19.1 matches installed Node major.

## Exact package manifest

Only runtime dependencies react19.3.0 and react-dom19.3.0 (matching peer).
Exact devDependencies, no ^/~ or additional direct packages:

| Package | Pin | License / purpose |
| --- | --- | --- |
| vite | 8.3.2 | MIT / bundler |
| @vitejs/plugin-react | 6.1.1 | MIT / official React plugin, Vite8 peer |
| typescript | 6.0.3 | Apache-2.0 / compiler |
| tailwindcss | 4.3.3 | MIT / accepted CSS framework |
| @tailwindcss/vite | 4.3.3 | MIT / official Tailwind integration, Vite8 peer |
| eslint | 10.12.0 | MIT / lint |
| @eslint/js | 10.0.1 | MIT / standard rules, ESLint10 peer |
| globals | 17.13.0 | MIT / browser and config globals |
| typescript-eslint | 8.71.0 | MIT / TypeScript lint, ESLint10/TS6 peers |
| eslint-plugin-react-hooks | 7.1.1 | MIT / React correctness, ESLint10 peer |
| eslint-plugin-react-refresh | 0.5.7 | MIT / Vite component export rules |
| @types/react | 19.3.0 | MIT / React types |
| @types/react-dom | 19.3.0 | MIT / ReactDOM types |
| @types/node | 24.19.1 | MIT / Node config types |

React/DOM are MIT. Source registry: https://registry.npmjs.org (exact package
version endpoints). Vite/ESLint engines include installed Node24. No templates,
global installs, optional React compiler/Babel packages, router, React Query,
icons, UI/form/state/table kits or tests package without a current consumer.
Manager alone generates frontend/package-lock.json, reviews graph/provenance/
licenses/integrities/vulnerability findings and installs project-local packages.
Use registry.npmjs.org explicitly and --ignore-scripts; do not run third-party
install lifecycle scripts. No worker package commands or fabricated locks.

## Exact worker write paths

1. .gitignore (retain existing rules; add node_modules/, dist/, npm debug logs)
2. frontend/package.json
3. frontend/index.html
4. frontend/vite.config.ts
5. frontend/tsconfig.json
6. frontend/eslint.config.js
7. frontend/src/main.tsx
8. frontend/src/app/App.tsx
9. frontend/src/styles/index.css

Caps: nine files, 400 changed lines. Manager may additionally generate only
frontend/package-lock.json. Forbid backend/**, docs/**, .agents/**, instructions/**,
.codex/**, frontend/package-lock.json, credentials/.env*/keys, deployment and
all paths outside exact list. No empty marker folders or unused abstractions.

## Worker read context / consent

Proposed complete context: AGENTS.md, docs/plan/P03_TASK_CARD.md,
docs/plan/SKILL_ROUTING.md, .agents/skills/frontend-ui/SKILL.md.
Then only the nine allowed implementation paths if present. No linked-file
loading, directory scans/search, commands, browser/network/MCP/subagents.
Codex loaded canonical memory/requirements; task card carries relevant overrides.
Owner explicitly confirmed sharing exactly these four context files and existing
nine implementation paths before dispatch. Prior P02 consent was not reused.

## Acceptance criteria / skills

implementation_skills: frontend-ui; review_skills: code-review (Codex).
frontend skill SHA256448079D6EF3B5B514BBCBA4F2A8D5AAF0FC6CC8063F79798F28A6DBBE5128F90;
code-review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
Worker reads complete frontend skill and routing before edits; links grant no
additional authority. Canonical requirements/E04 override stale tokens/auth/
font/40px/money guidance. System font, light white/gray, blue primary, readable
Vietnamese, no gradients/cards/dashboard stats/sample data/external assets.

- private=true, type=module, packageManager=npm@11.13.0; engines Node24/npm11.
- Scripts: dev and preview bound127.0.0.1; typecheck=tsc --noEmit;
  build=npm run typecheck && vite build; lint=eslint . --max-warnings=0.
- Single tsconfig includes src and vite.config.ts, strict/noEmit,
  noUnusedLocals/noUnusedParameters, ES2022/DOM, bundler resolution,
  react-jsx, vite/client and node types; no any/suppressions.
- Vite plugins React and Tailwind only; no proxy/backend origin/CORS business
  configuration. ESLint flat config covers config JS/TS and all TS/TSX source,
  recommended JS/TypeScript/hooks/refresh rules; ignores generated dist/node_modules.
- index.html lang=vi, UTF8, viewport, Vietnamese title, root and real main module.
- main.tsx verifies root then renders real App under StrictMode, imports styles.
- App contains only semantic main/h1 and honest unavailable-functionality text;
  no business actions, API/fetch/auth/storage/service worker/router/network assets.
- CSS imports Tailwind v4; system font/light neutral page and blue heading.
  Minimal Tailwind utilities used by App actually appear in built CSS.
- Accessible readable text at375/768/1280, no horizontal page overflow. No
  interactive controls, so focus/actions/loading/server-error states N/A here.
- Codex verifies receipt/events/actual scope, dependencies, real scripts,
  browser render at three widths and absence of external/API requests.

No SQL/backend/business/API tests or migrations; these are N/A for P03.
No test suite mirroring static markup. Owner walkthrough: HTML loads main;
React renders one App; Tailwind styles it; scripts check TS/lint and bundle.
Build does not mean business readiness or full LOCAL_DEV configured.

## Manager verification / execution record

Validated English contract, scoped consent, worker run and independent review complete.
From detached frontend: npm install --package-lock-only --ignore-scripts
--registry=https://registry.npmjs.org; npm ci --ignore-scripts with same registry;
npm run typecheck; npm run lint; npm run build; npm audit --json with same registry.
Inspect exact lock graph and git diff --check; compare SDK/backend source unchanged.
Manager may serve built static preview on loopback solely for three-width browser
review, then stop it. This is no business/API/HTTPS profile, LAN or deployment.
Record actual outputs, worker/manager corrections and verdict; preserve detached
result until separate owner approval to apply. Do not auto-start P05 or other M0.

### Verified execution

- Actual clean dispatch base and initial detached HEAD independently checked:
  3f72bc7c5dc88861d7801110885f2fd1c5fa78df. Earlier eeed057 was preparation.
- Run4151e913-734, conversation35832fe7-4043-4acd-9a13-b14c1351ea70;
  AGY SUCCESS/exit0, runner review_required, zero write-policy violations.
  [Original contract](p03-evidence/worker-contract.json),
  [original receipt](p03-evidence/worker-receipt.json).
- Codex audited all24tool invocations:15view_file, eight write_to_file,
  one replace_file_content. Four complete context reads, gitignore read,
  one allowed absent package.json probe, nine final-file reads. No extra path,
  command/search/directory/browser/network/MCP/subagent invocation.
- Worker changed exactly nine allowed files,193runner diff lines, within400.
  Complete frontend skill/routing reads occurred before editing; actual layout,
  source/config and behavior checked against canonical overrides by Codex.
- Worktree: C:/Users/asus/AppData/Local/Temp/codex-antigravity-worker/worktrees/main_workplace-4151e913-734.
- Final detached commit c5e4eac0452f62fcd6bc788a209c9919d81d4576:
  ten files/3667Git insertions (nine source/config paths185lines plus manager
  package-lock.json3482lines). No backend changes. Original worker counts use
  the runner's trailing-line convention; generated lock not a worker edit.

### Manager corrections and dependency evidence

Within existing App.tsx/index.html only: corrected product title to
"Quản lý trung tâm tiếng Anh", replaced implementation jargon with
"Các chức năng hiện chưa khả dụng.", and added inline empty favicon to stop
the browser's verified default favicon404 console error. No extra UI or routes.
Codex generated lockfile and ran npm ci --ignore-scripts; no lifecycle scripts,
global/template/browser installation or additional direct package introduced.

Lock v3 has224resolved records, sixteen exact direct deps. All218nonbundled
records have registry.npmjs.org archive URLs and integrity; six optional WASI
inBundle records inherit their verified @tailwindcss/oxide-wasm32-wasi parent
archive/integrity. npm ci verifies fetched archive integrity; npm ls --all
--omit=optional succeeded with no peer errors. Installed172packages on Windows,
optional other-platform graph retained in lock; these counts are distinct.
License expressions reviewed: MIT163, Apache-2.0 15, MPL-2.0 24, ISC11,
BSD-2-Clause6, BSD-3-Clause2, BlueOak-1.0.0 one, CC-BY-4.0 one, 0BSD one.
Non-MIT tool/native/data packages are development dependencies, not added UI
libraries; preserve their supplied notices when redistributing dependencies.
No known vulnerabilities reported by npm audit (all severity counts0) on2026-10-04.
Lock SHA256 AC45C67BA3BF5E5938D66AB1073A680AB190CC5506CD4414DC55E473615087CE.

### Independent commands and browser review

| Gate | Actual result |
| --- | --- |
| npm install --package-lock-only --ignore-scripts with explicit registry | Exit0, generated manager lock. |
| npm ci --ignore-scripts with explicit registry | Exit0, locked local install, no script execution. |
| npm run typecheck | Exit0, actual source/Vite config strict noEmit. |
| npm run lint | Exit0, max-warnings0; JS/TS/hooks/refresh configuration active. |
| npm run build | Exit0; executes typecheck then Vite8.3.2/Tailwind;16modules, HTML/JS/CSS generated. |
| npm audit --json with explicit registry | Exit0, vulnerabilities total0. |
| Source/scope/diff | Exact10finalpaths; diff --cached --check passed; backend unchanged; no API/auth/storage/any/source suppressions. |
| Built-page browser | All375/768/1280 widths pass, document scrollWidth equals viewport; Vietnamese/system font; real Tailwind padding16/32/48px and heading24/30/30px. |

CUA runtime failed to initialize twice; manager used already-bundled
Playwright1.62.1 with installed Edge headless instead, no new project package.
Served actual built page via npm run preview -- --port4178 --strictPort bound
127.0.0.1 only; stopped via Ctrl+C, no remaining port4178listener.
All three renders had no console/page errors and only same-origin HTML/JS/CSS
requests, no API/external requests. Screenshots visually inspected at all widths.
Measured actual rendered heading/body contrast6.53:1/7.25:1 against page background.
[Browser measurements](p03-evidence/browser-evidence.json),
[contrast measurements](p03-evidence/contrast-evidence.json),
[375px](p03-evidence/viewport-375.png), [768px](p03-evidence/viewport-768.png),
[1280px](p03-evidence/viewport-1280.png).

Contract SHA25602862E7C9C4778F7065D90C422F52D86EF9C64207DE68BBE7D87102BE5C8836A;
receipt6F2E3764C2D3AC3A835152A7AF52146FE1D47C22B3AE02275F594DF7F83672B8.
Raw events retained only in the local runner run directory, not Project Memory;
events SHA256C2E17DCCD74F4DB3BE0E525C60D1A91C8C1296FFFA05518FDF1EACD72766272E.

### Verdict and next action

**PASS for final detached P03 after manager corrections.** No remaining P03
must-fix findings. skill_verification complete via actual event/source/script/
browser checks. Strict application/config types checked; tsconfig skipLibCheck
skips third-party declaration checking. No business/API/backend/SQL tests,
route guards/forms/loading/error/submitting/keyboard controls to verify: N/A
with static noninteractive App. Full design tokens/shared UI belong to later
consumers; this skeleton uses baseline system font/neutrals/blue only.
This is not full accessibility/business/security/readiness or M0/V01 evidence.

Primary receives only manager docs/evidence; no frontend source/gitignore
apply/merge/push. Retain detached tree for owner review. Next: separate owner
approval to apply this exact P03 result, then direction for another bounded task.
Do not infer P05/SQL/rest M0/deployment permission.
