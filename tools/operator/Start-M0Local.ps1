[CmdletBinding()]
param([switch]$VerifyBrowser,[switch]$ReadOnlyCheck,[string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace';$root='C:\ProgramData\EnglishCenter\M0'
$encoding=[Text.UTF8Encoding]::new($false);$sessions=[Collections.Generic.List[object]]::new()
$receipt=[ordered]@{phase='W5-launch';success=$false;ready=$false;browserPassed=$false;replayCommits=0;auditCommits=0;failedRuns=0;stoppedMaintenanceRefused=$false;forcedStops=0}
$resolved=$null;$client=$null;$node=$null;$appCredential=$null;$payload=$null;$checkpoint='custody'
function Assert-Custody([string]$path,[string]$operatorSid){
    $item=Get-Item -LiteralPath $path;$ancestor=$item
    while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Invalid custody'};if($ancestor -is [IO.FileInfo]){$ancestor=$ancestor.Directory}else{$ancestor=$ancestor.Parent}}
    $acl=Get-Acl -LiteralPath $path;$ownerSid=([Security.Principal.NTAccount]::new($acl.Owner)).Translate([Security.Principal.SecurityIdentifier]).Value
    if(($item -is [IO.DirectoryInfo] -and ($ownerSid -ne $operatorSid -or -not $acl.AreAccessRulesProtected)) -or $ownerSid -notin @($operatorSid,'S-1-5-18','S-1-5-32-544') -or $acl.Access.Count -ne 3){throw 'Invalid custody'}
    $seen=[Collections.Generic.HashSet[string]]::new()
    foreach($rule in $acl.Access){$sid=$rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value;if(-not $seen.Add($sid) -or $sid -notin @($operatorSid,'S-1-5-18','S-1-5-32-544') -or $rule.FileSystemRights -ne 'FullControl' -or $rule.AccessControlType -ne 'Allow'){throw 'Invalid custody'} }
    if($item -is [IO.FileInfo] -and ($item.Length -le 0 -or $item.Length -gt 65536)){throw 'Invalid descriptor size'}
}
function Start-Role([string]$role,[string]$mode,[bool]$audit){
    $credential=Import-Clixml (Join-Path $root ($role+'.credential.clixml'))
    try{
        if($credential -isnot [Management.Automation.PSCredential] -or $credential.UserName -cne ($env:COMPUTERNAME+'\'+$role)){throw 'Invalid credential'}
        $start=[Diagnostics.ProcessStartInfo]::new('C:\Program Files\dotnet\dotnet.exe')
        $start.Arguments='"'+$workspace+'\backend\src\EnglishCenter.Api\bin\Debug\net10.0\EnglishCenter.Api.dll" '+$mode+' --managed-stdin'+$(if($audit){' --verify-audit'}else{''})
        $start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.LoadUserProfile=$true;$start.WorkingDirectory=$workspace
        $start.Domain=$env:COMPUTERNAME;$start.UserName=$role;$start.Password=$credential.Password
        $start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
        $start.EnvironmentVariables['DOTNET_SKIP_FIRST_TIME_EXPERIENCE']='1';$start.EnvironmentVariables['DOTNET_GENERATE_ASPNET_CERTIFICATE']='false'
        $start.EnvironmentVariables['DOTNET_CLI_TELEMETRY_OPTOUT']='1';$start.EnvironmentVariables['DOTNET_NOLOGO']='1'
        $start.EnvironmentVariables['DOTNET_CLI_HOME']='C:\Users\'+$role;$start.EnvironmentVariables['TEMP']='C:\Users\'+$role+'\AppData\Local\Temp';$start.EnvironmentVariables['TMP']=$start.EnvironmentVariables['TEMP']
        $process=[Diagnostics.Process]::Start($start)
        $session=[pscustomobject]@{Role=$role;Process=$process;Output=$process.StandardOutput.ReadToEndAsync();Errors=$process.StandardError.ReadToEndAsync();Stopped=$false}
        $sessions.Add($session);return $session
    }finally{if($credential -is [Management.Automation.PSCredential]){$credential.Password.Dispose()};$credential=$null}
}
function Stop-Role($session){
    if($session.Stopped){return}
    if(-not $session.Process.HasExited){$session.Process.StandardInput.Close();if(-not $session.Process.WaitForExit(40000)){$session.Process.Kill();[void]$session.Process.WaitForExit(5000);$receipt.forcedStops++}}
    $session.Stopped=$true
}
function Read-Health([string]$path){
    $response=$client.GetAsync('https://localhost:8443'+$path).GetAwaiter().GetResult()
    try{return [pscustomobject]@{Status=[int]$response.StatusCode;Body=$response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}}finally{$response.Dispose()}
}
try{
    if($PSVersionTable.PSVersion.Major -lt 7 -or ($VerifyBrowser -and $ReadOnlyCheck)){throw 'Unsupported mode'}
    $operatorSid=[Security.Principal.WindowsIdentity]::GetCurrent().User.Value;Assert-Custody $root $operatorSid
    foreach($name in @('ownership.json','catalog.json','EcM0Runtime.credential.clixml','EcM0Maint.credential.clixml')){Assert-Custody (Join-Path $root $name) $operatorSid}
    $ownership=Get-Content (Join-Path $root 'ownership.json') -Raw|ConvertFrom-Json;$catalog=Get-Content (Join-Path $root 'catalog.json') -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne $operatorSid -or $catalog.ownerId -ne $ownership.ownerId -or $catalog.catalog -cne 'EnglishCenter_M0_LOCALDEV' -or $catalog.phase -cne 'principals_ready'){throw 'Invalid owner'}
    foreach($role in @('EcM0Runtime','EcM0Maint')){$expected=@($ownership.identities|Where-Object {$_.name -ceq $role});if($expected.Count -ne 1 -or (Get-LocalUser -Name $role).SID.Value -ne $expected[0].sid){throw 'Invalid identity'}}
    if(-not $ReceiptPath){$ReceiptPath=Join-Path $workspace ('docs\plan\V01-evidence\test-results\W5E-launch-'+[Guid]::NewGuid().ToString('N')+'.json')}
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith($workspace+'\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $checkpoint='hosts';$timer=[Diagnostics.Stopwatch]::StartNew();$maint=Start-Role 'EcM0Maint' '--m0-maintenance' ([bool]$VerifyBrowser)
    Start-Sleep -Milliseconds 1000
    $runtime=Start-Role 'EcM0Runtime' '--m0-local-runtime' $false
    # Let the bounded 5s startup proof finish before concurrent health probes.
    # The original 20s launch deadline and fresh per-request readiness still apply.
    Start-Sleep -Milliseconds 6000
    $client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(8)
    $checkpoint='readiness'
    while($timer.Elapsed.TotalSeconds -lt 20){
        if($runtime.Process.HasExited -or $maint.Process.HasExited){throw 'Host refused'}
        try{$health=Read-Health '/health/ready';if($health.Status -eq 200 -and $health.Body -ceq 'ready'){$receipt.ready=$true;break}}catch{}
        Start-Sleep -Milliseconds 250
    }
    if(-not $receipt.ready){throw 'Readiness refused'}
    if($VerifyBrowser){
        $checkpoint='browser';Assert-Custody (Join-Path $root 'admin.credential.clixml') $operatorSid;Assert-Custody (Join-Path $root 'admin.json') $operatorSid
        $admin=Get-Content (Join-Path $root 'admin.json') -Raw|ConvertFrom-Json
        if($admin.ownerId -ne $ownership.ownerId -or $admin.databaseId -ne $catalog.databaseId -or $admin.databaseGuid -ne $catalog.databaseGuid -or $admin.phase -notin @('committed','verified')){throw 'Invalid Admin escrow'}
        $appCredential=Import-Clixml (Join-Path $root 'admin.credential.clixml')
        if($appCredential -isnot [Management.Automation.PSCredential] -or $appCredential.UserName -cne 'admin@englishcenter.local'){throw 'Invalid Admin credential'}
        $start=[Diagnostics.ProcessStartInfo]::new('C:\Users\asus\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe')
        $start.Arguments='"'+$workspace+'\tools\operator\Verify-M0Browser.cjs"';$start.UseShellExecute=$false;$start.CreateNoWindow=$true
        $start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
        $node=[Diagnostics.Process]::Start($start);$output=$node.StandardOutput.ReadToEndAsync();$errors=$node.StandardError.ReadToEndAsync()
        $payload=@{email=$appCredential.UserName;password=$appCredential.GetNetworkCredential().Password}|ConvertTo-Json -Compress
        $node.StandardInput.Write($payload);$node.StandardInput.Close();$payload=$null;$appCredential.Password.Dispose();$appCredential=$null
        if(-not $node.WaitForExit(120000)){throw 'Browser deadline'}
        $browserText=$output.GetAwaiter().GetResult();$discarded=$errors.GetAwaiter().GetResult()
        if($node.ExitCode -ne 0){if($discarded -cmatch '^W5-browser refused at [a-z0-9-]+$'){$receipt.browserCheckpoint=$discarded};throw 'Browser refused'}
        $discarded=$null;if($browserText.Length -gt 512){throw 'Invalid browser result'};$browser=$browserText|ConvertFrom-Json
        if($browser.phase -cne 'W5-browser' -or $browser.success -ne $true -or $browser.logins -ne 4 -or $browser.logouts -ne 4 -or $browser.widths -ne 3 -or $browser.tlsTrusted -ne $true){throw 'Invalid browser proof'}
        $receipt.browserPassed=$true;$checkpoint='scheduled-replay'
        while($timer.Elapsed.TotalSeconds -lt 75){if($maint.Process.HasExited -or $runtime.Process.HasExited){throw 'Host stopped'};Start-Sleep -Milliseconds 500}
    }elseif(-not $ReadOnlyCheck){
        [Console]::WriteLine('M0 LOCAL_DEV: https://localhost:8443 — press Enter to stop both owned hosts.')
        [void][Console]::ReadLine()
    }
    $checkpoint='shutdown';Stop-Role $maint
    if($VerifyBrowser -or $ReadOnlyCheck){$health=Read-Health '/health/ready';if($health.Status -ne 503 -or $health.Body -cne 'not_ready'){throw 'Stopped maintenance must refuse'};$receipt.stoppedMaintenanceRefused=$true}
    Stop-Role $runtime
    foreach($session in $sessions){
        $text=$session.Output.GetAwaiter().GetResult();$discarded=$session.Errors.GetAwaiter().GetResult();$discarded=$null
        if($session.Process.ExitCode -ne 0 -or $text.Length -gt 512){throw 'Host exit refused'}
        $summary=$text|ConvertFrom-Json;if($summary.success -ne $true){throw 'Invalid host result'}
        if($session.Role -ceq 'EcM0Maint'){$receipt.replayCommits=[long]$summary.replayCommits;$receipt.auditCommits=[long]$summary.auditCommits;$receipt.failedRuns=[long]$summary.failedRuns}
    }
    if($receipt.forcedStops -ne 0 -or ($VerifyBrowser -and ($receipt.replayCommits -lt 1 -or $receipt.auditCommits -ne 1 -or $receipt.failedRuns -ne 0))){throw 'Maintenance acceptance refused'}
    $receipt.success=$true
}catch{$receipt.errorCategory='local_launch_or_verification_refused';$receipt.checkpoint=$checkpoint}
finally{
    if($node){if(-not $node.HasExited){$node.Kill($true);[void]$node.WaitForExit(5000)};$node.Dispose()}
    foreach($session in $sessions){try{Stop-Role $session}finally{$session.Process.Dispose()}}
    if($appCredential){$appCredential.Password.Dispose()};$appCredential=$null;$payload=$null
    if($client){$client.Dispose()}
    if($resolved){[IO.File]::WriteAllText($resolved,($receipt|ConvertTo-Json),$encoding)}
}
$receipt|ConvertTo-Json
if(-not $receipt.success){exit 1}