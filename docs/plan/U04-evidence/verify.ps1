param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference='Stop'
$sources=@('src/app/IdentityShell.tsx','src/app/identityNavigation.ts')+
 @(Get-ChildItem src/components/ui -Filter '*.tsx' | ForEach-Object FullName)+
 @(Get-ChildItem src/lib -Filter '*.ts' | ForEach-Object FullName)+
 @(Get-ChildItem src/features/identity -Recurse -Include '*.ts','*.tsx' | ForEach-Object FullName)
node node_modules/typescript/bin/tsc @sources --ignoreConfig --target ES2022 --module commonjs --moduleResolution node --ignoreDeprecations 6.0 --jsx react-jsx --esModuleInterop --strict --skipLibCheck --outDir $BuildDirectory
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
$env:EC_TEST_BUILD=$BuildDirectory
node --test tests/ui.test.mjs tests/requestScope.test.mjs tests/auth.test.mjs tests/http.test.mjs tests/signup.test.mjs tests/session.test.mjs tests/sessionSignals.test.mjs
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
npm run lint
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
npm run build
exit $LASTEXITCODE
