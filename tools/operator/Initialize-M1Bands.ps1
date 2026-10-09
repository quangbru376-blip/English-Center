[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0'
$workspace='C:\Study\SOEN\main_workplace'
$receipt=[ordered]@{phase='M1-band-reference';success=$false;noPlaintextOutput=$true}
$resolved=$null;$schemaCredential=$null;$process=$null
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
    if($ownership.operatorSid -ne $operatorSid -or $catalog.ownerId -ne $ownership.ownerId -or $catalog.catalog -cne 'EnglishCenter_M0_LOCALDEV' -or $catalog.phase -cne 'principals_ready' -or $catalog.databaseId -le 4 -or ([Guid]$catalog.databaseGuid) -eq [Guid]::Empty){throw 'Invalid catalog ownership'}
    $schema=@($ownership.identities|Where-Object {$_.name -ceq 'EcM0Schema'})
    if($schema.Count -ne 1 -or (Get-LocalUser -Name 'EcM0Schema').SID.Value -ne $schema[0].sid){throw 'Invalid schema identity'}
    $schemaCredential=Import-Clixml (Join-Path $root 'EcM0Schema.credential.clixml')
    if($schemaCredential -isnot [Management.Automation.PSCredential] -or $schemaCredential.UserName -cne ($env:COMPUTERNAME+'\EcM0Schema')){throw 'Invalid credential binding'}
    $start=[Diagnostics.ProcessStartInfo]::new('C:\Program Files\dotnet\dotnet.exe')
    $start.Arguments='"'+$workspace+'\backend\src\EnglishCenter.Api\bin\Debug\net10.0\EnglishCenter.Api.dll" --m1-seed-bands '+$catalog.databaseId+' '+([Guid]$catalog.databaseGuid).ToString('D')
    $start.WorkingDirectory=$workspace;$start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.LoadUserProfile=$true
    $start.Domain=$env:COMPUTERNAME;$start.UserName='EcM0Schema';$start.Password=$schemaCredential.Password
    $start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
    $start.EnvironmentVariables['DOTNET_NOLOGO']='1';$start.EnvironmentVariables['DOTNET_GENERATE_ASPNET_CERTIFICATE']='false'
    $start.EnvironmentVariables['DOTNET_CLI_HOME']='C:\Users\EcM0Schema';$start.EnvironmentVariables['TEMP']='C:\Users\EcM0Schema\AppData\Local\Temp';$start.EnvironmentVariables['TMP']=$start.EnvironmentVariables['TEMP']
    $process=[Diagnostics.Process]::Start($start);$stdout=$process.StandardOutput.ReadToEndAsync();$stderr=$process.StandardError.ReadToEndAsync();$process.StandardInput.Close()
    if(-not $process.WaitForExit(45000)){throw 'Reference seed deadline'}
    $output=$stdout.GetAwaiter().GetResult();$discarded=$stderr.GetAwaiter().GetResult();$discarded=$null
    if($process.ExitCode -ne 0 -or $output.Length -gt 512){throw 'Reference seed refused or result unknown'}
    $result=$output|ConvertFrom-Json
    if($result.phase -cne 'M1-band-reference' -or $result.success -ne $true){throw 'Invalid reference seed result'}
    $receipt.success=$true
}catch{$receipt.errorCategory='reference_seed_refused_or_result_unknown';$receipt.failureLine=$_.InvocationInfo.ScriptLineNumber}
finally{
    if($process){if(-not $process.HasExited){$process.Kill();[void]$process.WaitForExit(5000)};$process.Dispose()}
    if($schemaCredential){$schemaCredential.Password.Dispose()};$schemaCredential=$null
    if($resolved){[IO.File]::WriteAllText($resolved,($receipt|ConvertTo-Json),[Text.UTF8Encoding]::new($false))}
}
$receipt|ConvertTo-Json
if(-not $receipt.success){exit 1}
