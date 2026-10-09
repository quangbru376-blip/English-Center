[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\M0'
$catalog='EnglishCenter_M0_LOCALDEV'
$resolvedReceipt=$null
$connection=$null
$created=$false
$logins=@()
$receipt=[ordered]@{phase='W2-A';success=$false;catalogCreated=$false;loginsCreated=0;tlsVerified=$false}
$record=$null
try {
    $candidate=[IO.Path]::GetFullPath($ReceiptPath)
    if(-not $candidate.StartsWith('C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\',[StringComparison]::OrdinalIgnoreCase) -or
       [IO.Path]::GetExtension($candidate) -ne '.json' -or (Test-Path -LiteralPath $candidate)){throw 'Invalid receipt'}
    $resolvedReceipt=$candidate
    $ownership=Get-Content -LiteralPath (Join-Path $root 'ownership.json') -Raw | ConvertFrom-Json
    if($ownership.phase -ne 'identities_ready' -or $ownership.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or
       (Test-Path -LiteralPath (Join-Path $root 'catalog.json'))){throw 'Invalid custody ownership'}
    $names=@('EcM0Runtime','EcM0Maint','EcM0Schema')
    if(@($ownership.identities).Count -ne 3 -or $env:COMPUTERNAME -notmatch '^[A-Za-z0-9_-]+$'){throw 'Invalid identities'}
    foreach($name in $names){
        $stored=@($ownership.identities | Where-Object {$_.name -ceq $name})
        if($stored.Count -ne 1 -or (Get-LocalUser -Name $name).SID.Value -ne $stored[0].sid){throw 'Identity mismatch'}
    }
    Add-Type -AssemblyName System.Data
    $connection=[Data.SqlClient.SqlConnection]::new('Server=lpc:.\SQLEXPRESS;Database=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Pooling=False;Connect Timeout=5')
    $connection.Open()
    $command=$connection.CreateCommand();$command.CommandTimeout=10
    $command.CommandText="SELECT CASE WHEN IS_SRVROLEMEMBER('sysadmin')=1 AND DB_ID(@catalog) IS NULL THEN 1 ELSE 0 END"
    [void]$command.Parameters.AddWithValue('@catalog',$catalog)
    if([int]$command.ExecuteScalar() -ne 1){throw 'Provisioning preflight refused'}
    foreach($name in $names){
        $command.Parameters.Clear();$command.CommandText='SELECT COUNT(*) FROM sys.server_principals WHERE name=@login'
        [void]$command.Parameters.AddWithValue('@login',($env:COMPUTERNAME+'\'+$name))
        if([int]$command.ExecuteScalar() -ne 0){throw 'Login already exists'}
    }
    $command.Parameters.Clear()
    $command.CommandText='CREATE DATABASE [EnglishCenter_M0_LOCALDEV] COLLATE Latin1_General_100_CI_AS_SC;'
    [void]$command.ExecuteNonQuery();$created=$true;$receipt.catalogCreated=$true
    $command.CommandText="SELECT d.database_id,r.database_guid FROM sys.databases d JOIN sys.database_recovery_status r ON r.database_id=d.database_id WHERE d.name=@catalog"
    [void]$command.Parameters.AddWithValue('@catalog',$catalog)
    $reader=$command.ExecuteReader()
    try{
        if(-not $reader.Read()){throw 'Catalog identity missing'}
        $record=[ordered]@{ownerId=$ownership.ownerId;catalog=$catalog;databaseId=$reader.GetInt32(0);databaseGuid=$reader.GetGuid(1).ToString();phase='created';logins=@()}
        if($reader.Read()){throw 'Ambiguous catalog'}
    }finally{$reader.Dispose()}
    [IO.File]::WriteAllText((Join-Path $root 'catalog.json'),($record|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
    $command.Parameters.Clear()
    $command.CommandText='ALTER DATABASE [EnglishCenter_M0_LOCALDEV] SET AUTO_CLOSE OFF; ALTER DATABASE [EnglishCenter_M0_LOCALDEV] SET COMPATIBILITY_LEVEL=160; ALTER DATABASE [EnglishCenter_M0_LOCALDEV] SET ALLOW_SNAPSHOT_ISOLATION ON;'
    [void]$command.ExecuteNonQuery()
    foreach($name in $names){
        $login=$env:COMPUTERNAME+'\'+$name
        # Machine/name were validated against the fixed allowlist; SQL identifiers are quoted.
        $command.CommandText='CREATE LOGIN ['+$login.Replace(']',']]')+'] FROM WINDOWS WITH DEFAULT_DATABASE=[EnglishCenter_M0_LOCALDEV];'
        [void]$command.ExecuteNonQuery();$logins+=$name;$record.logins+=$name
        [IO.File]::WriteAllText((Join-Path $root 'catalog.json'),($record|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
    }
    $record.phase='catalog_logins_ready'
    [IO.File]::WriteAllText((Join-Path $root 'catalog.json'),($record|ConvertTo-Json -Depth 4),[Text.UTF8Encoding]::new($false))
    $receipt.success=$true
}catch{
    $receipt.errorCategory='catalog_provisioning_failed'
    $receipt.ownedArtifactsRetained=$created
}finally{
    if($null -ne $connection){$connection.Dispose()}
    $receipt.loginsCreated=$logins.Count
    if($resolvedReceipt){
        $bytes=[Text.UTF8Encoding]::new($false).GetBytes(($receipt|ConvertTo-Json))
        $stream=[IO.File]::Open($resolvedReceipt,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
    }
}
if(-not $receipt.success){exit 1}