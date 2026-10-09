[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0'
$registry='HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\MSSQL17.SQLEXPRESS\MSSQLServer\SuperSocketNetLib'
$serviceName='MSSQL$SQLEXPRESS'
$resolved=$null;$record=$null;$certificate=$null;$key=$null;$trustAdded=$false;$bindingChanged=$false
$oldCertificate='';$oldEncryption=0
$receipt=[ordered]@{phase='W3';success=$false;administratorToken=$false;serviceRunning=$false;keyAclProtected=$false;directLeafTrust=$false;rollbackConfirmed=$false}
try{
    $principal=[Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
    if(-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Elevation required'}
    $receipt.administratorToken=$true
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or
       [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content (Join-Path $root 'ownership.json') -Raw|ConvertFrom-Json
    $catalog=Get-Content (Join-Path $root 'catalog.json') -Raw|ConvertFrom-Json
    if($ownership.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or $catalog.ownerId -ne $ownership.ownerId -or
       $catalog.phase -ne 'principals_ready' -or (Test-Path (Join-Path $root 'sql-tls.json'))){throw 'Invalid custody'}
    $instance=(Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL').SQLEXPRESS
    $service=Get-CimInstance Win32_Service -Filter "Name='MSSQL`$SQLEXPRESS'"
    $settings=Get-ItemProperty -LiteralPath $registry
    $oldCertificate=[string]$settings.Certificate;$oldEncryption=[int]$settings.ForceEncryption
    if($instance -cne 'MSSQL17.SQLEXPRESS' -or $service.StartName -cne 'NT Service\MSSQL$SQLEXPRESS' -or $service.State -ne 'Running' -or
       $oldCertificate.Length -ne 0 -or $oldEncryption -ne 0 -or $env:COMPUTERNAME -notmatch '^[A-Za-z0-9_-]+$'){throw 'SQL TLS preflight drift'}
    $record=[ordered]@{ownerId=$ownership.ownerId;phase='preparing';oldCertificate=$oldCertificate;forceEncryption=$oldEncryption;serviceWasRunning=$true;thumbprint=$null;privateKeyFile=$null}
    [IO.File]::WriteAllText((Join-Path $root 'sql-tls.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $certificate=New-SelfSignedCertificate -Type SSLServerAuthentication -Subject ('CN='+$env:COMPUTERNAME) -DnsName @($env:COMPUTERNAME,'localhost') -CertStoreLocation 'Cert:\LocalMachine\My' -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -Provider 'Microsoft RSA SChannel Cryptographic Provider' -KeySpec KeyExchange -KeyExportPolicy NonExportable -NotBefore (Get-Date).AddMinutes(-5) -NotAfter (Get-Date).AddYears(1) -FriendlyName ('EnglishCenter M0 SQL '+$ownership.ownerId)
    if(-not $certificate.HasPrivateKey -or @($certificate.EnhancedKeyUsageList | Where-Object {$_.ObjectId.Value -eq '1.3.6.1.5.5.7.3.1'}).Count -ne 1){throw 'Invalid server-auth certificate'}
    $record.thumbprint=$certificate.Thumbprint
    $key=$certificate.PrivateKey
    if($key -isnot [Security.Cryptography.RSACryptoServiceProvider] -or $key.CspKeyContainerInfo.KeyNumber -ne [Security.Cryptography.KeyNumber]::Exchange -or $key.CspKeyContainerInfo.Exportable){throw 'Invalid SQL key provider'}
    $keyRoot=[IO.Path]::Combine([Environment]::GetFolderPath('CommonApplicationData'),'Microsoft\Crypto\RSA\MachineKeys')
    $keyPath=[IO.Path]::GetFullPath((Join-Path $keyRoot $key.CspKeyContainerInfo.UniqueKeyContainerName))
    if(-not $keyPath.StartsWith($keyRoot+'\',[StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path -LiteralPath $keyPath) -or
       ((Get-Item -LiteralPath $keyPath).Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Invalid owned private key path'}
    $record.privateKeyFile=$keyPath
    [IO.File]::WriteAllText((Join-Path $root 'sql-tls.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $acl=[Security.AccessControl.FileSecurity]::new();$acl.SetAccessRuleProtection($true,$false)
    $adminSid=[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544');$systemSid=[Security.Principal.SecurityIdentifier]::new('S-1-5-18')
    $sqlSid=[Security.Principal.NTAccount]::new('NT Service\MSSQL$SQLEXPRESS').Translate([Security.Principal.SecurityIdentifier])
    $acl.SetOwner($adminSid)
    foreach($sid in @($adminSid,$systemSid)){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','Allow'))}
    $acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sqlSid,'Read','Allow'))
    Set-Acl -LiteralPath $keyPath -AclObject $acl
    $actual=Get-Acl -LiteralPath $keyPath
    if(-not $actual.AreAccessRulesProtected -or $actual.Access.Count -ne 3){throw 'Private key ACL mismatch'}
    foreach($rule in $actual.Access){
        $ruleSid=$rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value
        if($rule.AccessControlType -ne 'Allow' -or $ruleSid -notin @($adminSid.Value,$systemSid.Value,$sqlSid.Value)){throw 'Unexpected key access'}
        if($ruleSid -eq $sqlSid.Value -and $rule.FileSystemRights -ne [Security.AccessControl.FileSystemRights]::Read){throw 'SQL key write authority'}
    }
    $receipt.keyAclProtected=$true
    $store=[Security.Cryptography.X509Certificates.X509Store]::new('Root','LocalMachine')
    $store.Open([Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
    try{$store.Add([Security.Cryptography.X509Certificates.X509Certificate2]::new($certificate.RawData));$trustAdded=$true}finally{$store.Close()}
    $receipt.directLeafTrust=$true
    Set-ItemProperty -LiteralPath $registry -Name Certificate -Value $certificate.Thumbprint
    $bindingChanged=$true
    Stop-Service -Name $serviceName
    Start-Service -Name $serviceName
    $running=Get-Service -Name $serviceName;$running.WaitForStatus([ServiceProcess.ServiceControllerStatus]::Running,[TimeSpan]::FromSeconds(20))
    $receipt.serviceRunning=$true;$record.phase='sql_tls_bound'
    [IO.File]::WriteAllText((Join-Path $root 'sql-tls.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    $receipt.success=$true
}catch{
    $receipt.errorCategory='sql_tls_provisioning_failed'
    if($bindingChanged){
        try{
            Set-ItemProperty -LiteralPath $registry -Name Certificate -Value $oldCertificate
            if((Get-Service $serviceName).Status -eq 'Running'){Stop-Service -Name $serviceName}
            Start-Service -Name $serviceName
            $receipt.rollbackConfirmed=(Get-Service $serviceName).Status -eq 'Running'
        }catch{$receipt.rollbackConfirmed=$false}
    }
    if($trustAdded -and $certificate){
        $store=[Security.Cryptography.X509Certificates.X509Store]::new('Root','LocalMachine');$store.Open([Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        try{$store.Remove([Security.Cryptography.X509Certificates.X509Certificate2]::new($certificate.RawData))}finally{$store.Close()}
    }
    if($record){$record.phase='failed_owned_artifacts_retained';[IO.File]::WriteAllText((Join-Path $root 'sql-tls.json'),($record|ConvertTo-Json),[Text.UTF8Encoding]::new($false))}
}finally{
    if($key){$key.Dispose()}
    if($resolved){
        $bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json))
        $stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
    }
}
if(-not $receipt.success){exit 1}