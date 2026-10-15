[CmdletBinding()]
param()
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace';$base='C:\ProgramData\EnglishCenter'
$operatorRoot=Join-Path $base 'DemoOperator';$runtimeRoot=Join-Path $base 'DemoRuntime';$maintRoot=Join-Path $base 'DemoMaintenance'
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if(-not ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole('Administrators')){throw 'Windows elevation required'}
$original=Get-Content (Join-Path $base 'M0\ownership.json') -Raw|ConvertFrom-Json
if($original.operatorSid -ne $identity.User.Value){throw 'Wrong operator'}
foreach($path in @($operatorRoot,$runtimeRoot,$maintRoot)){if(Test-Path -LiteralPath $path){throw 'Demo custody exists; refusing replacement'}}
$runtime=(Get-LocalUser EcM0Runtime).SID;$maint=(Get-LocalUser EcM0Maint).SID
foreach($role in @(@{Name='EcM0Runtime';Sid=$runtime},@{Name='EcM0Maint';Sid=$maint})){
 if(@($original.identities|Where-Object {$_.name -ceq $role.Name -and $_.sid -eq $role.Sid.Value}).Count -ne 1 -or @(Get-LocalGroupMember -SID 'S-1-5-32-544'|Where-Object {$_.SID.Value -eq $role.Sid.Value}).Count -ne 0){throw 'Identity drift'}
}
$ownerId=[Guid]::NewGuid().ToString('N')
function New-Custody([string]$path,$owner,$reader){
 $ancestor=[IO.DirectoryInfo]::new($path);while($ancestor){if($ancestor.Exists -and ($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Reparse ancestor'};$ancestor=$ancestor.Parent}
 [void][IO.Directory]::CreateDirectory($path)
 $acl=[Security.AccessControl.DirectorySecurity]::new();$acl.SetOwner($owner);$acl.SetAccessRuleProtection($true,$false)
 foreach($sid in @($owner,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))}
 if($reader){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($reader,'ReadAndExecute','ContainerInherit,ObjectInherit','None','Allow'))}
 Set-Acl -LiteralPath $path -AclObject $acl
}
New-Custody $operatorRoot $identity.User $null
New-Custody $runtimeRoot $runtime $null
New-Custody $maintRoot $maint $runtime
$encoding=[Text.UTF8Encoding]::new($false)
function Write-Json([string]$path,$value){[IO.File]::WriteAllText($path,($value|ConvertTo-Json -Depth 12),$encoding)}
Write-Json (Join-Path $operatorRoot 'ownership.json') @{ownerId=$ownerId;operatorSid=$identity.User.Value;identities=$original.identities;purpose='LOCAL_DEMO';createdAt=[DateTime]::UtcNow}
Write-Json (Join-Path $runtimeRoot 'prepared.json') @{ownerId=$ownerId;runtimeSid=$runtime.Value;phase='runtime_root_prepared'}
Write-Json (Join-Path $maintRoot 'prepared.json') @{ownerId=$ownerId;runtimeSid=$runtime.Value;maintenanceSid=$maint.Value;phase='maintenance_root_prepared'}
# Read the currently approved installed manifest, then verify every copied artifact.
$manifest=Get-Content (Join-Path $base 'M0Runtime\patch-manifest.json') -Raw|ConvertFrom-Json
if($manifest.artifacts.Count -ne 5 -or $manifest.dotNet -cne '10.0.4' -or $manifest.nextReview -cne '2027-01-09'){throw 'Unexpected installed manifest'}
foreach($artifact in $manifest.artifacts){
 $source=Join-Path $workspace $artifact.relativePath;$target=Join-Path ($workspace+'\.local-demo\release') $artifact.relativePath
 if((Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash -cne $artifact.sha256){throw 'Approved artifact mismatch'}
 [void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target));[IO.File]::Copy($source,$target,$false)
}
$profile=Get-Content (Join-Path $base 'M0Runtime\profile.json') -Raw|ConvertFrom-Json
$profile.kind='LOCAL_DEMO';$profile.web.origin='https://localhost:8444';$profile.web.listenPort=8444
$profile.auth.issuer='english-center-demo-local';$profile.auth.audience='english-center-demo-local-ui'
foreach($root in @($runtimeRoot,$maintRoot)){Write-Json (Join-Path $root 'profile.json') $profile;Write-Json (Join-Path $root 'patch-manifest.json') $manifest}
Write-Json ($workspace+'\.local-demo\prepare.json') @{success=$true;ownerId=$ownerId;phase='roots-and-profiles';originalCustodyUnchanged=$true}
