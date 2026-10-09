[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0'
$connection=$null;$transaction=$null;$resolved=$null
$receipt=[ordered]@{phase='W2-B';success=$false;usersCreated=0;committed=$false;tlsVerified=$false}
try{
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or
      [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolved=$candidate
    $ownership=Get-Content (Join-Path $root 'ownership.json') -Raw | ConvertFrom-Json
    $catalog=Get-Content (Join-Path $root 'catalog.json') -Raw | ConvertFrom-Json
    if($ownership.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or
       $catalog.ownerId -ne $ownership.ownerId -or $catalog.catalog -cne 'EnglishCenter_M0_LOCALDEV' -or
       $catalog.phase -cne 'catalog_logins_ready' -or $env:COMPUTERNAME -notmatch '^[A-Za-z0-9_-]+$'){throw 'Ownership mismatch'}
    foreach($name in @('EcM0Runtime','EcM0Maint','EcM0Schema')){
        $identity=@($ownership.identities | Where-Object {$_.name -ceq $name})
        if($identity.Count -ne 1 -or (Get-LocalUser -Name $name).SID.Value -ne $identity[0].sid){throw 'Windows identity mismatch'}
    }
    Add-Type -AssemblyName System.Data
    $connection=[Data.SqlClient.SqlConnection]::new('Server=lpc:.\SQLEXPRESS;Database=EnglishCenter_M0_LOCALDEV;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Pooling=False;Connect Timeout=5')
    $connection.Open();$command=$connection.CreateCommand();$command.CommandTimeout=10
    $command.CommandText="SELECT r.database_guid FROM sys.database_recovery_status r WHERE r.database_id=DB_ID();"
    if([DBNull]::Value -eq $command.ExecuteScalar()){throw 'Catalog identity unavailable'}
    $actualGuid=$command.ExecuteScalar()
    $command.CommandText='SELECT DB_ID()'
    if([int]$command.ExecuteScalar() -ne $catalog.databaseId -or $actualGuid.ToString() -ne $catalog.databaseGuid){throw 'Catalog identity mismatch'}
    $command.CommandText="SELECT CASE WHEN (SELECT COUNT(*) FROM dbo.__EFMigrationsHistory)=6 AND (SELECT COUNT(*) FROM sys.database_principals WHERE name LIKE 'ec[_]%' AND type='R')=4 AND (SELECT COUNT(*) FROM sys.database_permissions p JOIN sys.database_principals r ON r.principal_id=p.grantee_principal_id WHERE r.name LIKE 'ec[_]%')=102 AND NOT EXISTS(SELECT 1 FROM sys.database_principals WHERE name IN('M0RuntimeUser','M0MaintUser','M0SchemaUser')) THEN 1 ELSE 0 END"
    if([int]$command.ExecuteScalar() -ne 1){throw 'Migration/principal baseline refused'}
    foreach($name in @('EcM0Runtime','EcM0Maint','EcM0Schema')){
        $command.CommandText='SELECT sid FROM sys.server_principals WHERE name=@login AND type=N''U'''
        [void]$command.Parameters.AddWithValue('@login',($env:COMPUTERNAME+'\'+$name))
        $sqlSid=$command.ExecuteScalar();$command.Parameters.Clear()
        $windowsSid=(Get-LocalUser -Name $name).SID
        $expectedSid=New-Object byte[] $windowsSid.BinaryLength
        $windowsSid.GetBinaryForm($expectedSid,0)
        if($sqlSid -isnot [byte[]] -or [Convert]::ToBase64String($sqlSid) -ne [Convert]::ToBase64String($expectedSid)){throw 'SQL login SID mismatch'}
    }
    $transaction=$connection.BeginTransaction();$command.Transaction=$transaction
    $machine=$env:COMPUTERNAME
    $command.CommandText=@"
CREATE USER [M0RuntimeUser] FOR LOGIN [$machine\EcM0Runtime];
CREATE USER [M0MaintUser] FOR LOGIN [$machine\EcM0Maint];
CREATE USER [M0SchemaUser] FOR LOGIN [$machine\EcM0Schema];
ALTER ROLE [ec_app_runtime] ADD MEMBER [M0RuntimeUser];
GRANT CONNECT TO [M0RuntimeUser];
GRANT VIEW DEFINITION TO [M0RuntimeUser];
GRANT SELECT ON OBJECT::dbo.__EFMigrationsHistory (MigrationId,ProductVersion) TO [M0RuntimeUser];
GRANT CONNECT TO [M0MaintUser];
GRANT DELETE ON OBJECT::dbo.AuditLog TO [M0MaintUser];
GRANT DELETE ON OBJECT::dbo.ReplayOperation TO [M0MaintUser];
GRANT SELECT ON OBJECT::dbo.AuditLog (audit_category,security_expires_at,ip_expires_at) TO [M0MaintUser];
GRANT SELECT ON OBJECT::dbo.ReplayOperation (expires_at) TO [M0MaintUser];
GRANT UPDATE ON OBJECT::dbo.AuditLog (ip_address,ip_expires_at) TO [M0MaintUser];
ALTER ROLE [db_owner] ADD MEMBER [M0SchemaUser];
"@
    [void]$command.ExecuteNonQuery();$transaction.Commit();$receipt.committed=$true;$receipt.usersCreated=3
    $catalog.phase='principals_ready'
    [IO.File]::WriteAllText((Join-Path $root 'catalog.json'),($catalog|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
    $receipt.success=$true
}catch{
    $receipt.errorCategory='principal_provisioning_failed'
    if($transaction -and -not $receipt.committed){try{$transaction.Rollback()}catch{$receipt.rollbackUnconfirmed=$true}}
}finally{
    if($transaction){$transaction.Dispose()};if($connection){$connection.Dispose()}
    if($resolved){
        $bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json))
        $stream=[IO.File]::Open($resolved,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
    }
}
if(-not $receipt.success){exit 1}