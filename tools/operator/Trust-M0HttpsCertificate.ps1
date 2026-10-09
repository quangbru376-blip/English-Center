[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$resolved=$null;$certificate=$null;$trustAdded=$false
$receipt=[ordered]@{phase='W4-https-trust';success=$false;publicLeafOnly=$false;ownedHashMatched=$false}
try{
    $principal=[Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Elevation required'}
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content 'C:\ProgramData\EnglishCenter\M0\ownership.json' -Raw|ConvertFrom-Json
    $root='C:\ProgramData\EnglishCenter\M0Runtime'
    $marker=Get-Content (Join-Path $root 'prepared.json') -Raw|ConvertFrom-Json
    $keys=Get-Content (Join-Path $root 'key-manifest.json') -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or $marker.ownerId -ne $ownership.ownerId -or $keys.ownerId -ne $ownership.ownerId -or $keys.runtimeSid -ne $marker.runtimeSid -or $keys.phase -cne 'jwt_https_provisioned'){throw 'Invalid ownership'}
    foreach($path in @($root,(Join-Path $root 'https.cer'))){if(((Get-Item -LiteralPath $path).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse custody path'}}
    $publicFile=Get-Item -LiteralPath (Join-Path $root 'https.cer')
    if($publicFile.Length -lt 1 -or $publicFile.Length -gt 65536 -or (Get-FileHash -LiteralPath $publicFile.FullName -Algorithm SHA256).Hash -cne $keys.httpsLeafSha256){throw 'Public certificate hash mismatch'}
    $certificate=[Security.Cryptography.X509Certificates.X509Certificate2]::new([IO.File]::ReadAllBytes($publicFile.FullName))
    $eku=@($certificate.Extensions | Where-Object {$_.Oid.Value -eq '2.5.29.37'})
    if($certificate.Thumbprint -cne $keys.httpsThumbprint -or $certificate.HasPrivateKey -or $certificate.Subject -cne 'CN=localhost' -or $certificate.NotBefore -gt (Get-Date) -or $certificate.NotAfter -le (Get-Date) -or
       $eku.Count -ne 1 -or @($eku[0].EnhancedKeyUsages|Where-Object {$_.Value -eq '1.3.6.1.5.5.7.3.1'}).Count -ne 1 -or @($certificate.Extensions|Where-Object {$_.Oid.Value -eq '2.5.29.17'}).Count -ne 1 -or $certificate.GetNameInfo([Security.Cryptography.X509Certificates.X509NameType]::DnsName,$false) -cne 'localhost'){throw 'Invalid public HTTPS certificate'}
    $receipt.ownedHashMatched=$true
    $store=[Security.Cryptography.X509Certificates.X509Store]::new('Root','LocalMachine');$store.Open([Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    try{
        if($store.Certificates.Find([Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,$certificate.Thumbprint,$false).Count -ne 0){throw 'Leaf trust already exists'}
        $store.Add($certificate);$trustAdded=$true
    }finally{$store.Close()}
    $receipt.success=$true;$receipt.publicLeafOnly=$true
}catch{
    $receipt.errorCategory='https_leaf_trust_failed';$receipt.errorType=$_.Exception.GetType().FullName
    if($trustAdded -and $certificate){
        $cleanup=[Security.Cryptography.X509Certificates.X509Store]::new('Root','LocalMachine');$cleanup.Open([Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        try{$cleanup.Remove($certificate)}finally{$cleanup.Close()}
    }
}finally{
    if($resolved){$bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json));$stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None);try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}}
}
if(-not $receipt.success){exit 1}