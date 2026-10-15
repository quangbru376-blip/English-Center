[CmdletBinding()]
param()
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\DemoMaintenance';$record=$null;$owned=$false
$result=[ordered]@{phase='W5-maintenance-proof-key';success=$false;nonExportable=$false;noTrustImport=$true}
function Write-New([string]$path,[byte[]]$value){$stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None);try{$stream.Write($value,0,$value.Length)}finally{$stream.Dispose()}}
try{
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    $principal=[Security.Principal.WindowsPrincipal]::new($identity)
    if($identity.Name -cne ($env:COMPUTERNAME+'\EcM0Maint') -or $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Wrong maintenance identity'}
    $marker=Get-Content (Join-Path $root 'prepared.json') -Raw|ConvertFrom-Json
    if($marker.maintenanceSid -ne $identity.User.Value -or $marker.phase -cne 'maintenance_root_prepared' -or $marker.ownerId -notmatch '^[a-f0-9]{32}$'){throw 'Invalid ownership marker'}
    $ancestor=[IO.DirectoryInfo]::new($root)
    while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse custody'};$ancestor=$ancestor.Parent}
    $acl=Get-Acl -LiteralPath $root
    if(-not $acl.AreAccessRulesProtected -or $acl.Access.Count -ne 4 -or $acl.Owner -cne $identity.Name){throw 'Invalid root custody'}
    $allowed=@($identity.User.Value,'S-1-5-18','S-1-5-32-544')
    foreach($rule in $acl.Access){
        $sid=$rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
        if($rule.AccessControlType -ne 'Allow' -or ($sid -in $allowed -and $rule.FileSystemRights -ne 'FullControl') -or
           ($sid -notin $allowed -and ($sid -ne $marker.runtimeSid -or $rule.FileSystemRights -ne 'ReadAndExecute, Synchronize'))){throw 'Unexpected custody access'}
    }
    foreach($name in @('proof.cer','proof-manifest.json')){if(Test-Path -LiteralPath (Join-Path $root $name)){throw 'Proof artifact exists'}}
    $record=[ordered]@{ownerId=$marker.ownerId;maintenanceSid=$marker.maintenanceSid;runtimeSid=$marker.runtimeSid;phase='proof_preparing';thumbprint=$null;leafSha256=$null;pipeName=('EnglishCenter.M0.Maint.'+$marker.ownerId)}
    Write-New (Join-Path $root 'proof-manifest.json') ([Text.UTF8Encoding]::new($false).GetBytes(($record|ConvertTo-Json)))
    $owned=$true
    $cert=New-SelfSignedCertificate -Type Custom -Subject 'CN=EnglishCenter M0 Maintenance Proof' -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -Provider 'Microsoft Software Key Storage Provider' -KeyExportPolicy NonExportable -KeyUsage DigitalSignature -NotBefore (Get-Date).AddMinutes(-5) -NotAfter (Get-Date).AddYears(1) -FriendlyName ('EnglishCenter M0 Maintenance Proof '+$marker.ownerId)
    $record.thumbprint=$cert.Thumbprint
    $key=[Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($cert)
    try{
        if($key -isnot [Security.Cryptography.RSACng] -or $key.Key.ExportPolicy -ne [Security.Cryptography.CngExportPolicies]::None -or $key.KeySize -ne 2048){throw 'Invalid proof key custody'}
        $challenge=[Text.Encoding]::UTF8.GetBytes('EnglishCenter.M0.Maintenance.Custody.v1')
        $signature=$key.SignData($challenge,[Security.Cryptography.HashAlgorithmName]::SHA256,[Security.Cryptography.RSASignaturePadding]::Pss)
        $public=[Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPublicKey($cert)
        try{if(-not $public.VerifyData($challenge,$signature,[Security.Cryptography.HashAlgorithmName]::SHA256,[Security.Cryptography.RSASignaturePadding]::Pss)){throw 'Proof signature failed'}}finally{$public.Dispose()}
    }finally{if($key){$key.Dispose()}}
    Write-New (Join-Path $root 'proof.cer') $cert.RawData
    $sha=[Security.Cryptography.SHA256]::Create();try{$record.leafSha256=([BitConverter]::ToString($sha.ComputeHash($cert.RawData))).Replace('-','')}finally{$sha.Dispose()}
    $record.phase='proof_key_provisioned'
    [IO.File]::WriteAllText((Join-Path $root 'proof-manifest.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $result.success=$true;$result.nonExportable=$true
}catch{
    $result.errorCategory='maintenance_proof_key_failed'
    if($owned){$record.phase='failed_owned_artifacts_retained';[IO.File]::WriteAllText((Join-Path $root 'proof-manifest.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))}
}
$result|ConvertTo-Json -Compress
if(-not $result.success){exit 1}
