# P03 — frontend skeleton

Date: 2026-10-04. Status: prepared; worker not dispatched.
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
Specific external sharing of these four files pending owner scoped consent;
the prior P02 consent is not reused for a different task/context.

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

Pending validated English contract, scoped consent, worker run and review.
From detached frontend: npm install --package-lock-only --ignore-scripts
--registry=https://registry.npmjs.org; npm ci --ignore-scripts with same registry;
npm run typecheck; npm run lint; npm run build; npm audit --json with same registry.
Inspect exact lock graph and git diff --check; compare SDK/backend source unchanged.
Manager may serve built static preview on loopback solely for three-width browser
review, then stop it. This is no business/API/HTTPS profile, LAN or deployment.
Record actual outputs, worker/manager corrections and verdict; preserve detached
result until separate owner approval to apply. Do not auto-start P05 or other M0.
