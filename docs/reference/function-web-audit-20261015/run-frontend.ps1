$ErrorActionPreference='Stop'
$taskRoot='C:\Study\SOEN\main_workplace'
$taskEvidence=Join-Path $taskRoot 'docs\reference\function-web-audit-20261015'
$taskNode='C:\Users\asus\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe'
$taskBuild=Join-Path $taskRoot ('frontend\TestResults\function-audit-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $taskBuild -Force | Out-Null
$taskConfig=@{extends=(Join-Path $taskRoot 'frontend\tsconfig.json').Replace('\','/');compilerOptions=@{module='CommonJS';moduleResolution='node';noEmit=$false;outDir=$taskBuild.Replace('\','/');rootDir=(Join-Path $taskRoot 'frontend\src').Replace('\','/');ignoreDeprecations='6.0'};include=@((Join-Path $taskRoot 'frontend\src\**\*.ts').Replace('\','/'),(Join-Path $taskRoot 'frontend\src\**\*.tsx').Replace('\','/'))} | ConvertTo-Json -Depth 5
[IO.File]::WriteAllText((Join-Path $taskBuild 'compile.json'),$taskConfig)
[IO.File]::WriteAllText((Join-Path $taskBuild 'package.json'),'{"type":"commonjs"}')
Set-Location (Join-Path $taskRoot 'frontend')
& $taskNode node_modules/typescript/bin/tsc --noEmit *> (Join-Path $taskEvidence 'typecheck.log')
if($LASTEXITCODE -ne 0){throw 'Types failed'}
& $taskNode node_modules/eslint/bin/eslint.js . --max-warnings=0 *> (Join-Path $taskEvidence 'lint.log')
if($LASTEXITCODE -ne 0){throw 'Lint failed'}
& $taskNode node_modules/typescript/bin/tsc --project (Join-Path $taskBuild 'compile.json') *> (Join-Path $taskEvidence 'test-compile.log')
if($LASTEXITCODE -ne 0){throw 'Test compile failed'}
$env:EC_TEST_BUILD=$taskBuild
$taskTests=@(Get-ChildItem -LiteralPath tests -Filter '*.test.mjs' | ForEach-Object {$_.FullName})
& $taskNode --test @taskTests *> (Join-Path $taskEvidence 'frontend-tests.log')
$taskExit=$LASTEXITCODE
Write-Output ('Frontend test exit: '+$taskExit)
Get-Content -LiteralPath (Join-Path $taskEvidence 'frontend-tests.log') -Tail 10
exit $taskExit
