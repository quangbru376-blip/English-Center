param([switch]$CaptureNv04)
$ErrorActionPreference='Stop'
$taskRoot='C:\Study\SOEN\main_workplace'
$taskEvidence=Join-Path $taskRoot 'docs\reference\function-web-audit-20261015'
$taskNodeRoot='C:\Users\asus\.cache\codex-runtimes\codex-primary-runtime\dependencies\node'
$env:PATH=(Join-Path $taskNodeRoot 'bin')+';'+$env:PATH
$env:EC_V01_ASSETS=Join-Path $env:TEMP ('ec-function-web-audit-'+[Guid]::NewGuid().ToString('N'))
$env:EC_PLAYWRIGHT=Join-Path $taskNodeRoot 'node_modules\playwright'
$env:EC_V01_SQL='1'
$env:EC_V01_OWNED_SQL='1'
$env:EC_V01_BROWSER='1'
$env:EC_V01_E_RESULTS=Join-Path $taskEvidence 'ownership'
$env:EC_V01_DRIVER=Join-Path $taskRoot 'frontend\tests\browser\verify-identity.cjs'
$env:EC_M1_DRIVER=Join-Path $taskRoot 'frontend\tests\browser\verify-m1.cjs'
if($CaptureNv04){$env:EC_M1_DRIVER=Join-Path $taskEvidence 'capture-nv04.cjs'}
New-Item -ItemType Directory -Path $env:EC_V01_E_RESULTS -Force | Out-Null
Set-Location (Join-Path $taskRoot 'frontend')
& (Join-Path $taskNodeRoot 'bin\node.exe') tests/browser/build-fixture.mjs *> (Join-Path $taskEvidence $(if($CaptureNv04){'nv04-build.log'}else{'browser-build.log'}))
if($LASTEXITCODE -ne 0){throw 'Browser fixture build failed'}
Set-Location $taskRoot
$taskSelection=@('-class','EnglishCenter.Tests.Api.Integration.OwnedBrowserSqlHostingTests')
$taskStem='functions-web-edge'
$taskLog='browser.log'
if($CaptureNv04){
    $taskCase=Get-Content (Join-Path $taskEvidence 'browser-expanded-cases.json') -Raw | ConvertFrom-Json | Where-Object DisplayName -Like '*(group: 87)'
    if(@($taskCase).Count -ne 1){throw 'Exact NV04 supporting case required'}
    $taskSelection=@('-preEnumerateTheories','-id',$taskCase.ID)
    $taskStem='nv04-reproduction';$taskLog='nv04-browser.log'
}
& dotnet backend/tests/EnglishCenter.Tests/bin/Debug/net10.0/EnglishCenter.Tests.dll @taskSelection -noColor -result-trx (Join-Path $taskEvidence ('browser\'+$taskStem+'.trx')) -result-xml (Join-Path $taskEvidence ('browser\'+$taskStem+'.xml')) *> (Join-Path $taskEvidence $taskLog)
$taskExit=$LASTEXITCODE
Write-Output ('Browser test exit: '+$taskExit)
Get-Content -LiteralPath (Join-Path $taskEvidence $taskLog) -Tail 8
exit $taskExit
