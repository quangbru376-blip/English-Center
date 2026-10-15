$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace';$local=$workspace+'\.local-demo';$destination=$workspace+'\docs\plan\local-demo-evidence'
[void][IO.Directory]::CreateDirectory($destination)
$browser=Get-Content ($local+'\browser\summary-final.json') -Raw|ConvertFrom-Json
if(-not $browser.success -or $browser.cases.Count -ne 21){throw 'Final browser acceptance missing'}
$client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(8)
try{$response=$client.GetAsync('https://localhost:8444/health/ready').GetAwaiter().GetResult();$health=@{status=[int]$response.StatusCode;body=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult()};if($health.status -ne 200 -or $health.body -cne 'ready'){throw 'Demo not ready'}}finally{$client.Dispose()}
$sqlText=& 'C:\Program Files\dotnet\dotnet.exe' ($local+'\release\host\EnglishCenter.Demo.dll') check
if($LASTEXITCODE -ne 0){throw 'Demo SQL validation failed'};$sql=$sqlText|ConvertFrom-Json
[xml]$tests=Get-Content ($local+'\backend-tests.xml') -Raw
$baseline=Get-Content ($workspace+'\docs\reference\function-web-audit-20261015\source-fingerprints-before.json') -Raw|ConvertFrom-Json
$changed=@($baseline.source_sha256.PSObject.Properties|Where-Object {((Get-FileHash -LiteralPath (Join-Path $workspace $_.Name) -Algorithm SHA256).Hash).ToLowerInvariant() -cne $_.Value}|ForEach-Object Name)
$hashes=[ordered]@{};foreach($file in Get-ChildItem ($workspace+'\tools\demo') -File){$hashes['tools/demo/'+$file.Name]=(Get-FileHash $file.FullName -Algorithm SHA256).Hash}
foreach($path in $changed){$hashes[$path]=(Get-FileHash (Join-Path $workspace $path) -Algorithm SHA256).Hash}
$pictures=@();foreach($file in Get-ChildItem ($local+'\browser') -Filter '*.png'){
 [IO.File]::Copy($file.FullName,(Join-Path $destination $file.Name),$true);$pictures+=@{name=$file.Name;sha256=(Get-FileHash $file.FullName -Algorithm SHA256).Hash}
}
$wordStream=[IO.File]::Open(($workspace+'\docs\Nhom18_ThietKeXuLy.docx'),'Open','Read','ReadWrite');try{$wordHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($wordStream))}finally{$wordStream.Dispose()}
$summary=[ordered]@{recordedAt=[DateTime]::UtcNow;url='https://localhost:8444';catalog='EnglishCenter_DEMO_LOCAL';health=$health;sql=$sql;defaultBackend=@{total=$tests.assemblies.assembly.total;passed=$tests.assemblies.assembly.passed;failed=$tests.assemblies.assembly.failed;skipped=$tests.assemblies.assembly.skipped;errors=$tests.assemblies.assembly.errors};browser=$browser;staffPasswordChanges=17;productFilesCompared=@($baseline.source_sha256.PSObject.Properties).Count;changedProductFiles=$changed;sourceHashes=$hashes;screenshots=$pictures;wordSha256=$wordHash;wordWrittenByThisTask=$false;wordEarlierAuditHash='250FE2E5CC57F904A93011041F39644550E32FCEA378CF2CEF928C60537C0520';credentialsIncluded=$false;knownLimits=@('localhost only','no load test','M4 recovery inputs pending','NV04 explicit best-score display remains separate','one early background restart stop and interrupted browser runs retained locally; final managed run supersedes them')}
$summary['verifiedStop']=Get-Content ($local+'\verified-stop.json') -Raw|ConvertFrom-Json
$summary['detailBrowser']=Get-Content ($local+'\browser\summary-details.json') -Raw|ConvertFrom-Json
[IO.File]::WriteAllText(($destination+'\summary.json'),($summary|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
Write-Output 'Nonsecret demo evidence saved.'
