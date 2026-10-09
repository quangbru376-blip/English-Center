[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0Runtime'
$resolved=$null
$receipt=[ordered]@{phase='W4-root';success=$false;administratorToken=$false;runtimeOwned=$false;aclProtected=$false}
try{
    $principal=[Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Elevation required'}
    $receipt.administratorToken=$true
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or
       [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content 'C:\ProgramData\EnglishCenter\M0\ownership.json' -Raw|ConvertFrom-Json
    $catalog=Get-Content 'C:\ProgramData\EnglishCenter\M0\catalog.json' -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or $catalog.ownerId -ne $ownership.ownerId -or
       $catalog.phase -ne 'principals_ready' -or (Test-Path -LiteralPath $root)){throw 'Invalid new runtime custody'}
    $identity=@($ownership.identities | Where-Object {$_.name -ceq 'EcM0Runtime'})
    $runtime=Get-LocalUser -Name 'EcM0Runtime'
    if($identity.Count -ne 1 -or $identity[0].sid -ne $runtime.SID.Value -or
       @(Get-LocalGroupMember -SID 'S-1-5-32-544' | Where-Object {$_.SID.Value -eq $runtime.SID.Value}).Count -ne 0){throw 'Runtime authority mismatch'}
    $ancestor=[IO.DirectoryInfo]::new([IO.Path]::GetDirectoryName($root))
    while($ancestor){if($ancestor.Exists -and ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
    [IO.Directory]::CreateDirectory($root)|Out-Null
    $acl=[Security.AccessControl.DirectorySecurity]::new();$acl.SetOwner($runtime.SID);$acl.SetAccessRuleProtection($true,$false)
    $allowed=@($runtime.SID,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))
    foreach($sid in $allowed){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))}
    Set-Acl -LiteralPath $root -AclObject $acl
    $actual=Get-Acl -LiteralPath $root
    if(-not $actual.AreAccessRulesProtected -or $actual.Access.Count -ne 3 -or $actual.Owner -ne $runtime.SID.Translate([Security.Principal.NTAccount]).Value){throw 'Runtime ACL mismatch'}
    foreach($rule in $actual.Access){if($rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value -notin @($allowed|ForEach-Object {$_.Value}) -or $rule.AccessControlType -ne 'Allow'){throw 'Unexpected runtime access'}}
    $marker=[ordered]@{ownerId=$ownership.ownerId;runtimeSid=$runtime.SID.Value;phase='runtime_root_prepared'}
    [IO.File]::WriteAllText((Join-Path $root 'prepared.json'),($marker|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $receipt.success=$true;$receipt.runtimeOwned=$true;$receipt.aclProtected=$true
}catch{$receipt.errorCategory='runtime_root_provisioning_failed'}finally{
    if($resolved){$bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json));$stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None);try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}}
}
if(-not $receipt.success){exit 1}