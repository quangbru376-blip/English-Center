[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0Maintenance';$resolved=$null
$result=[ordered]@{phase='W5-maintenance-root';success=$false;privateStoreShared=$false}
try{
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    $principal=[Security.Principal.WindowsPrincipal]::new($identity)
    if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Elevation required'}
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content 'C:\ProgramData\EnglishCenter\M0\ownership.json' -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne $identity.User.Value -or (Test-Path -LiteralPath $root)){throw 'Invalid new owned root'}
    $maint=(Get-LocalUser -Name 'EcM0Maint').SID;$runtime=(Get-LocalUser -Name 'EcM0Runtime').SID
    foreach($role in @(@{Name='EcM0Maint';Sid=$maint},@{Name='EcM0Runtime';Sid=$runtime})){
        if(@($ownership.identities|Where-Object {$_.name -ceq $role.Name -and $_.sid -eq $role.Sid.Value}).Count -ne 1 -or
           @(Get-LocalGroupMember -SID 'S-1-5-32-544'|Where-Object {$_.SID.Value -eq $role.Sid.Value}).Count -ne 0){throw 'Identity drift'}
    }
    $ancestor=[IO.DirectoryInfo]::new([IO.Path]::GetDirectoryName($root))
    while($ancestor){if($ancestor.Exists -and ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
    [IO.Directory]::CreateDirectory($root)|Out-Null
    $acl=[Security.AccessControl.DirectorySecurity]::new();$acl.SetOwner($maint);$acl.SetAccessRuleProtection($true,$false)
    foreach($sid in @($maint,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))}
    $acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($runtime,'ReadAndExecute','ContainerInherit,ObjectInherit','None','Allow'))
    Set-Acl -LiteralPath $root -AclObject $acl
    $actual=Get-Acl -LiteralPath $root
    if(-not $actual.AreAccessRulesProtected -or $actual.Access.Count -ne 4 -or $actual.Owner -cne $maint.Translate([Security.Principal.NTAccount]).Value){throw 'Custody verification failed'}
    $marker=[ordered]@{ownerId=$ownership.ownerId;phase='maintenance_root_prepared';maintenanceSid=$maint.Value;runtimeSid=$runtime.Value}
    [IO.File]::WriteAllText((Join-Path $root 'prepared.json'),($marker|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $result.success=$true
}catch{$result.errorCategory='maintenance_root_failed'}finally{
    if($resolved){$stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None);try{$bytes=[Text.UTF8Encoding]::new($false).GetBytes(($result|ConvertTo-Json));$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}}
}
if(-not $result.success){exit 1}
