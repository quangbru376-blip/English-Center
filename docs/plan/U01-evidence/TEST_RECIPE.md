# U02/U01 technical verification recipe

Use existing frontend dependencies; never install packages for this scope.
From `frontend`, run `npm run typecheck`, `npm run lint`, `npm run build`.
Compile only the shared source to a fresh temporary CommonJS build using installed
TypeScript; Node tests use built-in node:test and installed React SSR.

```powershell
$testBuild = Join-Path $env:TEMP 'ec-u02-u01-tests'
$sourceFiles = @('src/app/IdentityShell.tsx') +
  @(Get-ChildItem src/components/ui -Filter '*.tsx' | ForEach-Object { $_.FullName }) +
  @(Get-ChildItem src/lib -Filter '*.ts' | ForEach-Object { $_.FullName })
node node_modules/typescript/bin/tsc @sourceFiles --ignoreConfig --target ES2022 --module commonjs --moduleResolution node --ignoreDeprecations 6.0 --jsx react-jsx --esModuleInterop --strict --skipLibCheck --outDir $testBuild
if ($LASTEXITCODE -ne 0) { throw 'Compile failed' }
New-Item -ItemType Junction -Path (Join-Path $testBuild 'node_modules') -Target (Resolve-Path node_modules).Path
$env:EC_TEST_BUILD = $testBuild
node --test tests/ui.test.mjs tests/requestScope.test.mjs tests/auth.test.mjs tests/http.test.mjs
```

Use a fresh temporary directory to avoid overwriting another build/junction.
For browser checks, use the existing bundled Playwright returned by
`load_workspace_dependencies`; set `EC_PLAYWRIGHT` to that module's absolute path.
Start frontend Vite on an unused loopback port, set `EC_UI_URL` to that URL and,
from the repository root, run `node docs/plan/U02-evidence/browser-check.cjs`.
The script uses installed Edge headless and synthetic `/tests/ui-fixture.html`;
close the test server afterward. It needs no package or browser download.
24 tests plus three-width browser checks prove shared seams, not real backend
runtime/production auth/domain pages/SQL/cross-tab or server commit outcomes.
