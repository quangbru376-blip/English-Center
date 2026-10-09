[CmdletBinding()]
param([ValidateSet('Runtime','Maintenance')][string]$Target='Runtime',[Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$resolved=$null;$root=$null;$created=[Collections.Generic.List[string]]::new()
$receipt=[ordered]@{phase='W5-profile';success=$false;target=$Target;artifactPins=0;noSecrets=$true}
try{
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    $principal=[Security.Principal.WindowsPrincipal]::new($identity)
    if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Elevation required'}
    $workspace='C:\Study\SOEN\main_workplace'
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith($workspace+'\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or
       [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content 'C:\ProgramData\EnglishCenter\M0\ownership.json' -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne $identity.User.Value){throw 'Invalid operator'}
    $root=if($Target -ceq 'Runtime'){'C:\ProgramData\EnglishCenter\M0Runtime'}else{'C:\ProgramData\EnglishCenter\M0Maintenance'}
    $role=if($Target -ceq 'Runtime'){'EcM0Runtime'}else{'EcM0Maint'}
    $sid=(Get-LocalUser -Name $role).SID
    $marker=Get-Content (Join-Path $root 'prepared.json') -Raw|ConvertFrom-Json
    $expectedSid=if($Target -ceq 'Runtime'){$marker.runtimeSid}else{$marker.maintenanceSid}
    if($marker.ownerId -ne $ownership.ownerId -or $expectedSid -ne $sid.Value -or
       @($ownership.identities | Where-Object {$_.name -ceq $role -and $_.sid -eq $sid.Value}).Count -ne 1){throw 'Invalid custody owner'}
    $acl=Get-Acl -LiteralPath $root
    if(-not $acl.AreAccessRulesProtected -or $acl.Owner -ne $sid.Translate([Security.Principal.NTAccount]).Value){throw 'Invalid custody ACL'}
    $expectedRules=if($Target -ceq 'Runtime'){3}else{4}
    if($acl.Access.Count -ne $expectedRules){throw 'Invalid custody rule count'}
    $seen=[Collections.Generic.HashSet[string]]::new()
    foreach($rule in $acl.Access){
        $ruleSid=$rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
        $runtimeSid=(Get-LocalUser -Name 'EcM0Runtime').SID.Value
        if(-not $seen.Add($ruleSid) -or ($ruleSid -in @($sid.Value,'S-1-5-18','S-1-5-32-544') -and $rule.FileSystemRights -ne 'FullControl') -or $rule.AccessControlType -ne 'Allow' -or ($ruleSid -notin @($sid.Value,'S-1-5-18','S-1-5-32-544') -and
           -not ($Target -ceq 'Maintenance' -and $ruleSid -eq $runtimeSid -and $rule.FileSystemRights -eq 'ReadAndExecute, Synchronize'))){throw 'Unexpected custody access'}
    }
    $ancestor=[IO.DirectoryInfo]::new($root)
    while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse custody'};$ancestor=$ancestor.Parent}
    $review=Get-Content (Join-Path $workspace 'docs\plan\M0_PATCH_REVIEW.md') -Raw
    $match=[regex]::Match($review,'(?s)```json\s*(.*?)\s*```')
    if(-not $match.Success){throw 'Missing reviewed manifest'}
    $manifest=$match.Groups[1].Value|ConvertFrom-Json
    if($manifest.dotNet -cne '10.0.4' -or $manifest.aspNet -cne '10.0.4' -or $manifest.ef -cne '10.0.4' -or
       $manifest.sql -cne '17.0.1000.7' -or $manifest.reviewed -cne '2026-10-09' -or $manifest.nextReview -cne '2027-01-09'){throw 'Unreviewed versions'}
    $paths=@('backend/global.json','backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj','backend/src/EnglishCenter.Api/packages.lock.json','frontend/package.json','frontend/package-lock.json')
    if($manifest.artifacts.Count -ne 5){throw 'Wrong pin set'}
    foreach($name in $paths){
        $pin=@($manifest.artifacts|Where-Object {$_.relativePath -ceq $name})
        $path=Join-Path $workspace $name
        $item=Get-Item -LiteralPath $path
        $ancestor=$item
        while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse artifact'};if($ancestor -is [IO.FileInfo]){$ancestor=$ancestor.Directory}else{$ancestor=$ancestor.Parent}}
        if($pin.Count -ne 1 -or $item.Length -ne $pin[0].length -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -cne $pin[0].sha256){throw 'Artifact drift'}
        $receipt.artifactPins++
    }
    $profile=Get-Content (Join-Path $workspace 'docs\plan\M0_LOCAL_DEV_PROFILE_TEMPLATE.json') -Raw|ConvertFrom-Json
    $profile.runtime.patchManifestReference='m0-patch-reviewed-v1'
    $profile.database.connectionSecretReference='m0-local-runtime'
    $profile.database.schemaManifestReference='embedded:EnglishCenter.Api.Infrastructure.Compatibility.ExpectedCatalog.json'
    $profile.web.certificateSecretReference='m0-runtime-https';$profile.web.clientTrustReference='m0-runtime-https-leaf'
    $profile.auth.jwtKeySecretReferences=@('m0-jwt-v1');$profile.auth.protectedPersistedCsrfStoreReference='m0-csrf-current-user'
    $profile.maintenance.replayCleanupJobReference='m0-replay-cleanup';$profile.maintenance.securityAuditCleanupJobReference='m0-audit-cleanup'
    $profile.maintenance.maintenancePrincipalReference='m0-local-maintenance'
    $outputs=@(@{Name='profile.json';Json=($profile|ConvertTo-Json -Depth 8)},@{Name='patch-manifest.json';Json=$match.Groups[1].Value})
    foreach($output in $outputs){if(Test-Path -LiteralPath (Join-Path $root $output.Name)){throw 'Descriptor already exists'}}
    foreach($output in $outputs){
        $path=Join-Path $root $output.Name
        $stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        $created.Add($path)
        try{$bytes=[Text.UTF8Encoding]::new($false).GetBytes($output.Json);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
    }
    $receipt.success=$true
}catch{
    $receipt.errorCategory='profile_installation_failed'
    foreach($path in $created){if([IO.Path]::GetDirectoryName($path) -ceq $root){Remove-Item -LiteralPath $path -Force}}
}finally{
    if($resolved){$bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json));$stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None);try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}}
}
if(-not $receipt.success){exit 1}
