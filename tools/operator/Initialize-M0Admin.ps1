[CmdletBinding()]
param([switch]$VerifyExisting,[Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0'
$workspace='C:\Study\SOEN\main_workplace'
$encoding=[Text.UTF8Encoding]::new($false)
$receipt=[ordered]@{phase='W5-bootstrap';success=$false;created=$false;verified=$false;escrowProtected=$false;noPlaintextOutput=$true}
$resolved=$null;$appCredential=$null;$schemaCredential=$null;$process=$null;$password=$null;$payload=$null
function Assert-OwnedPath([string]$path,[string]$operatorSid){
    $item=Get-Item -LiteralPath $path
    $ancestor=$item
    while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse custody'};if($ancestor -is [IO.FileInfo]){$ancestor=$ancestor.Directory}else{$ancestor=$ancestor.Parent}}
    if($item -is [IO.FileInfo] -and ($item.Length -le 0 -or $item.Length -gt 65536)){throw 'Invalid custody size'}
    $acl=Get-Acl -LiteralPath $path
    $ownerSid=([Security.Principal.NTAccount]::new($acl.Owner)).Translate([Security.Principal.SecurityIdentifier]).Value
    if(($item -is [IO.DirectoryInfo] -and ($ownerSid -ne $operatorSid -or -not $acl.AreAccessRulesProtected)) -or
       ($item -is [IO.FileInfo] -and $ownerSid -notin @($operatorSid,'S-1-5-18','S-1-5-32-544')) -or $acl.Access.Count -ne 3){throw 'Invalid custody'}
    $seen=[Collections.Generic.HashSet[string]]::new()
    foreach($rule in $acl.Access){$sid=$rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value;if(-not $seen.Add($sid) -or $sid -notin @($operatorSid,'S-1-5-18','S-1-5-32-544') -or $rule.FileSystemRights -ne 'FullControl' -or $rule.AccessControlType -ne 'Allow'){throw 'Invalid custody access'}}
}
try{
    $operatorSid=[Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    Assert-OwnedPath $root $operatorSid
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith($workspace+'\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    foreach($name in @('ownership.json','catalog.json','EcM0Schema.credential.clixml')){Assert-OwnedPath (Join-Path $root $name) $operatorSid}
    $ownership=Get-Content (Join-Path $root 'ownership.json') -Raw|ConvertFrom-Json
    $catalog=Get-Content (Join-Path $root 'catalog.json') -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne $operatorSid -or $ownership.ownerId -notmatch '^[a-f0-9]{32}$' -or $catalog.ownerId -ne $ownership.ownerId -or $catalog.catalog -cne 'EnglishCenter_M0_LOCALDEV' -or $catalog.phase -cne 'principals_ready' -or $catalog.databaseId -le 4 -or ([Guid]$catalog.databaseGuid) -eq [Guid]::Empty){throw 'Invalid catalog ownership'}
    $schema=@($ownership.identities|Where-Object {$_.name -ceq 'EcM0Schema'})
    if($schema.Count -ne 1 -or (Get-LocalUser -Name 'EcM0Schema').SID.Value -ne $schema[0].sid){throw 'Invalid schema identity'}
    $secretPath=Join-Path $root 'admin.credential.clixml';$markerPath=Join-Path $root 'admin.json'
    if($VerifyExisting){
        Assert-OwnedPath $secretPath $operatorSid;Assert-OwnedPath $markerPath $operatorSid
        $marker=Get-Content -LiteralPath $markerPath -Raw|ConvertFrom-Json
        if($marker.ownerId -ne $ownership.ownerId -or $marker.databaseId -ne $catalog.databaseId -or $marker.databaseGuid -ne $catalog.databaseGuid -or $marker.email -cne 'admin@englishcenter.local' -or $marker.phase -notin @('escrow_prepared','committed','verified')){throw 'Invalid Admin custody'}
        $appCredential=Import-Clixml -LiteralPath $secretPath
    }else{
        if((Test-Path -LiteralPath $secretPath) -or (Test-Path -LiteralPath $markerPath)){throw 'Admin custody already exists'}
        $bytes=New-Object byte[] 32;$rng=[Security.Cryptography.RandomNumberGenerator]::Create()
        try{$rng.GetBytes($bytes);$password=[Convert]::ToBase64String($bytes);$secure=ConvertTo-SecureString -String $password -AsPlainText -Force;$appCredential=[Management.Automation.PSCredential]::new('admin@englishcenter.local',$secure)}finally{[Array]::Clear($bytes,0,$bytes.Length);$rng.Dispose();$password=$null}
        $appCredential|Export-Clixml -LiteralPath $secretPath -NoClobber
        $escrowStream=[IO.File]::Open($secretPath,[IO.FileMode]::Open,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)
        try{$escrowStream.Flush($true)}finally{$escrowStream.Dispose()}
        $roundTrip=Import-Clixml -LiteralPath $secretPath
        try{if($roundTrip -isnot [Management.Automation.PSCredential] -or $roundTrip.UserName -cne $appCredential.UserName -or $roundTrip.GetNetworkCredential().Password -cne $appCredential.GetNetworkCredential().Password){throw 'Escrow roundtrip refused'}}finally{if($roundTrip -is [Management.Automation.PSCredential]){$roundTrip.Password.Dispose()};$roundTrip=$null}
        $marker=[ordered]@{ownerId=$ownership.ownerId;databaseId=$catalog.databaseId;databaseGuid=$catalog.databaseGuid;email='admin@englishcenter.local';phase='escrow_prepared'}
        $stream=[IO.File]::Open($markerPath,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try{$markerBytes=$encoding.GetBytes(($marker|ConvertTo-Json));$stream.Write($markerBytes,0,$markerBytes.Length);$stream.Flush($true)}finally{$stream.Dispose()}
        Assert-OwnedPath $secretPath $operatorSid;Assert-OwnedPath $markerPath $operatorSid
    }
    if($appCredential -isnot [Management.Automation.PSCredential] -or $appCredential.UserName -cne 'admin@englishcenter.local'){throw 'Invalid escrow credential'}
    $receipt.escrowProtected=$true
    $schemaCredential=Import-Clixml (Join-Path $root 'EcM0Schema.credential.clixml')
    if($schemaCredential -isnot [Management.Automation.PSCredential] -or $schemaCredential.UserName -cne ($env:COMPUTERNAME+'\EcM0Schema')){throw 'Invalid Windows credential'}
    $start=[Diagnostics.ProcessStartInfo]::new('C:\Program Files\dotnet\dotnet.exe')
    $start.Arguments='"'+$workspace+'\backend\src\EnglishCenter.Api\bin\Debug\net10.0\EnglishCenter.Api.dll" --m0-bootstrap'
    $start.WorkingDirectory=$workspace;$start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.LoadUserProfile=$true
    $start.Domain=$env:COMPUTERNAME;$start.UserName='EcM0Schema';$start.Password=$schemaCredential.Password
    $start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
    foreach($name in @('DOTNET_SKIP_FIRST_TIME_EXPERIENCE','DOTNET_CLI_TELEMETRY_OPTOUT')){$start.EnvironmentVariables[$name]='1'}
    $start.EnvironmentVariables['DOTNET_GENERATE_ASPNET_CERTIFICATE']='false';$start.EnvironmentVariables['DOTNET_NOLOGO']='1'
    $start.EnvironmentVariables['DOTNET_CLI_HOME']='C:\Users\EcM0Schema';$start.EnvironmentVariables['TEMP']='C:\Users\EcM0Schema\AppData\Local\Temp';$start.EnvironmentVariables['TMP']=$start.EnvironmentVariables['TEMP']
    $process=[Diagnostics.Process]::Start($start)
    $stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync()
    $password=$appCredential.GetNetworkCredential().Password
    $payload=@{databaseId=[int]$catalog.databaseId;databaseGuid=$catalog.databaseGuid;verifyOnly=[bool]$VerifyExisting;password=$password}|ConvertTo-Json -Compress
    $process.StandardInput.Write($payload);$process.StandardInput.Close();$password=$null;$payload=$null
    if(-not $process.WaitForExit(45000)){throw 'Bootstrap deadline'}
    $output=$stdout.GetAwaiter().GetResult();$discarded=$stderr.GetAwaiter().GetResult();$discarded=$null
    if($process.ExitCode -ne 0 -or $output.Length -gt 512){throw 'Bootstrap refused or result unknown'}
    $result=$output|ConvertFrom-Json
    if($result.phase -cne 'W5-bootstrap' -or $result.success -ne $true -or ($VerifyExisting -and ($result.verified -ne $true -or $result.created -ne $false)) -or (-not $VerifyExisting -and ($result.created -ne $true -or $result.verified -ne $false))){throw 'Invalid bootstrap result'}
    $marker.phase=if($VerifyExisting){'verified'}else{'committed'}
    [IO.File]::WriteAllText($markerPath,($marker|ConvertTo-Json),$encoding)
    $receipt.success=$true;$receipt.created=[bool]$result.created;$receipt.verified=[bool]$result.verified
}catch{$receipt.errorCategory='bootstrap_refused_or_result_unknown'}
finally{
    if($process){if(-not $process.HasExited){$process.Kill();[void]$process.WaitForExit(5000)};$process.Dispose()}
    if($appCredential){$appCredential.Password.Dispose()};if($schemaCredential){$schemaCredential.Password.Dispose()}
    $appCredential=$null;$schemaCredential=$null;$password=$null;$payload=$null
    if($resolved){[IO.File]::WriteAllText($resolved,($receipt|ConvertTo-Json),$encoding)}
}
$receipt|ConvertTo-Json
if(-not $receipt.success){exit 1}