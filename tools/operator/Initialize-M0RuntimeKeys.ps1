[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0Runtime'
$keyBytes=$null;$decrypted=$null;$rng=$null;$record=$null;$ownedManifest=$false
$result=[ordered]@{phase='W4-runtime-keys';success=$false;runtimeIdentity=$false;jwtDpapiVerified=$false;httpsPrivateKey=$false;privateExportPerformed=$false}
function Write-NewBytes([string]$path,[byte[]]$bytes){
    $stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
}
try{
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    if($identity.Name -cne ($env:COMPUTERNAME+'\EcM0Runtime')){throw 'Wrong runtime identity'}
    $marker=Get-Content (Join-Path $root 'prepared.json') -Raw|ConvertFrom-Json
    if($marker.phase -cne 'runtime_root_prepared' -or $marker.runtimeSid -ne $identity.User.Value -or
       ((Get-Item -LiteralPath $root).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Invalid prepared root'}
    foreach($name in @('jwt.dpapi','key-manifest.json','https.cer','csrf')){if(Test-Path -LiteralPath (Join-Path $root $name)){throw 'Key artifact already exists'}}
    $acl=Get-Acl -LiteralPath $root
    if(-not $acl.AreAccessRulesProtected -or $acl.Owner -cne $identity.Name -or $acl.Access.Count -ne 3){throw 'Runtime custody ACL mismatch'}
    $allowed=@($identity.User.Value,'S-1-5-18','S-1-5-32-544')
    foreach($rule in $acl.Access){if($rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value -notin $allowed -or $rule.AccessControlType -ne 'Allow'){throw 'Unexpected custody access'}}
    $result.runtimeIdentity=$true
    $now=[DateTimeOffset]::UtcNow
    $record=[ordered]@{ownerId=$marker.ownerId;runtimeSid=$identity.User.Value;phase='keys_preparing';jwtReference='m0-jwt-v1';jwtValidFrom=$now.AddMinutes(-1).ToString('O');jwtValidUntil=$now.AddDays(90).ToString('O');httpsThumbprint=$null;httpsLeafSha256=$null}
    Write-NewBytes (Join-Path $root 'key-manifest.json') ([Text.UTF8Encoding]::new($false).GetBytes(($record|ConvertTo-Json)))
    $ownedManifest=$true
    Add-Type -AssemblyName System.Security
    $keyBytes=New-Object byte[] 32;$rng=[Security.Cryptography.RandomNumberGenerator]::Create();$rng.GetBytes($keyBytes)
    $entropy=[Text.Encoding]::UTF8.GetBytes('EnglishCenter.M0.Jwt.v1')
    $protected=[Security.Cryptography.ProtectedData]::Protect($keyBytes,$entropy,[Security.Cryptography.DataProtectionScope]::CurrentUser)
    Write-NewBytes (Join-Path $root 'jwt.dpapi') $protected
    $decrypted=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes((Join-Path $root 'jwt.dpapi')),$entropy,[Security.Cryptography.DataProtectionScope]::CurrentUser)
    if($decrypted.Length -ne 32){throw 'Invalid protected key'}
    for($i=0;$i -lt 32;$i++){if($decrypted[$i] -ne $keyBytes[$i]){throw 'DPAPI key mismatch'}}
    $result.jwtDpapiVerified=$true
    $certificate=New-SelfSignedCertificate -Type SSLServerAuthentication -Subject 'CN=localhost' -DnsName @('localhost') -CertStoreLocation 'Cert:\CurrentUser\My' -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -Provider 'Microsoft Software Key Storage Provider' -KeyExportPolicy NonExportable -NotBefore (Get-Date).AddMinutes(-5) -NotAfter (Get-Date).AddYears(1) -FriendlyName ('EnglishCenter M0 HTTPS '+$marker.ownerId)
    $record.httpsThumbprint=$certificate.Thumbprint
    if(-not $certificate.HasPrivateKey -or @($certificate.EnhancedKeyUsageList|Where-Object {[string]$_.ObjectId -eq '1.3.6.1.5.5.7.3.1'}).Count -ne 1){throw 'Invalid HTTPS certificate'}
    $private=[Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($certificate)
    try{if($private -isnot [Security.Cryptography.RSACng] -or $private.Key.ExportPolicy -ne [Security.Cryptography.CngExportPolicies]::None){throw 'Exportable HTTPS key'}}finally{if($private){$private.Dispose()}}
    Write-NewBytes (Join-Path $root 'https.cer') $certificate.RawData
    $sha=[Security.Cryptography.SHA256]::Create()
    try{$record.httpsLeafSha256=([BitConverter]::ToString($sha.ComputeHash($certificate.RawData))).Replace('-','')}finally{$sha.Dispose()}
    $record.phase='jwt_https_provisioned'
    [IO.File]::WriteAllText((Join-Path $root 'key-manifest.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $result.success=$true;$result.httpsPrivateKey=$true
}catch{
    $result.errorCategory='runtime_key_provisioning_failed';$result.errorType=$_.Exception.GetType().FullName
    if($ownedManifest){$record.phase='failed_owned_artifacts_retained';[IO.File]::WriteAllText((Join-Path $root 'key-manifest.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))}
}finally{
    if($keyBytes){[Array]::Clear($keyBytes,0,$keyBytes.Length)};if($decrypted){[Array]::Clear($decrypted,0,$decrypted.Length)};if($rng){$rng.Dispose()}
}
$result|ConvertTo-Json -Compress
if(-not $result.success){exit 1}