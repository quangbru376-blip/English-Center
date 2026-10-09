[CmdletBinding()]
param([Parameter(Mandatory=$true)][ValidateSet('Runtime','Maint','Schema')][string]$Mode)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$result=[ordered]@{phase='W2-auth';mode=$Mode;success=$false;windowsIdentityMatched=$false;principalSafe=$false;catalogFacts=0;catalogMatched=$false;forbiddenReadDenied=$false;tlsVerified=$false}
$connection=$null
try{
    $repo='C:\Study\SOEN\main_workplace'
    $expectedName=$env:COMPUTERNAME+'\EcM0'+$Mode
    if([Security.Principal.WindowsIdentity]::GetCurrent().Name -cne $expectedName){throw 'Wrong Windows identity'}
    $result.windowsIdentityMatched=$true
    Add-Type -AssemblyName System.Data
    $connection=[Data.SqlClient.SqlConnection]::new('Server=lpc:.\SQLEXPRESS;Database=EnglishCenter_M0_LOCALDEV;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Pooling=False;Connect Timeout=5')
    $connection.Open();$command=$connection.CreateCommand();$command.CommandTimeout=10
    if($Mode -eq 'Runtime'){
        $command.CommandText=[IO.File]::ReadAllText((Join-Path $repo 'backend/src/EnglishCenter.Api/Infrastructure/Sql/Readiness/RuntimePrincipalRead.sql'))
        $reader=$command.ExecuteReader()
        try{
            if($reader.FieldCount -ne 8 -or -not $reader.Read()){throw 'Missing runtime facts'}
            $expectedFacts=@(1,0,0,0,0,0,0,4)
            for($i=0;$i -lt 8;$i++){if($reader.IsDBNull($i) -or $reader.GetInt32($i) -ne $expectedFacts[$i]){throw 'Unsafe runtime facts'}}
            if($reader.Read() -or $reader.NextResult()){throw 'Unexpected runtime facts'}
        }finally{$reader.Dispose()}
        $result.principalSafe=$true
        $command.CommandText=[IO.File]::ReadAllText((Join-Path $repo 'backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ReadCatalog.sql'))
        $actual=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
        $reader=$command.ExecuteReader()
        try{
            do{
                while($reader.Read()){
                    if($reader.IsDBNull(0) -or $reader.IsDBNull(1) -or $actual.ContainsKey($reader.GetString(0))){throw 'Unreadable catalog'}
                    $actual.Add($reader.GetString(0),$reader.GetString(1))
                }
            }while($reader.NextResult())
        }finally{$reader.Dispose()}
        $expected=Get-Content (Join-Path $repo 'backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json') -Raw | ConvertFrom-Json
        $properties=@($expected.PSObject.Properties)
        if($actual.Count -ne $properties.Count){throw 'Catalog fact count mismatch'}
        foreach($property in $properties){
            if(-not $actual.ContainsKey($property.Name)){throw 'Catalog fact missing'}
            $left=[string]$property.Value;$right=$actual[$property.Name]
            if($property.Name.StartsWith('module/',[StringComparison]::Ordinal)){
                $left=$left.Replace("`r`n","`n").Replace("`r","`n").Trim();$right=$right.Replace("`r`n","`n").Replace("`r","`n").Trim()
                $pattern='^(?:CREATE\s+OR\s+ALTER|CREATE|ALTER)(?:\s+|$)'
                $left=[regex]::Replace($left,$pattern,'CREATE ',[Text.RegularExpressions.RegexOptions]::IgnoreCase).TrimEnd()
                $right=[regex]::Replace($right,$pattern,'CREATE ',[Text.RegularExpressions.RegexOptions]::IgnoreCase).TrimEnd()
            }
            if(-not [string]::Equals($left,$right,[StringComparison]::Ordinal)){throw 'Catalog fact mismatch'}
        }
        $result.catalogFacts=$actual.Count;$result.catalogMatched=$true
        $command.CommandText="SELECT CASE WHEN HAS_PERMS_BY_NAME('dbo.AuditLog','OBJECT','DELETE')=0 AND HAS_PERMS_BY_NAME('dbo.ReplayOperation','OBJECT','DELETE')=0 THEN 1 ELSE 0 END"
        if([int]$command.ExecuteScalar() -ne 1){throw 'Runtime purge authority'}
    }elseif($Mode -eq 'Maint'){
        $source=[IO.File]::ReadAllText((Join-Path $repo 'backend/src/EnglishCenter.Api/Infrastructure/Maintenance/SqlMaintenanceService.cs'))
        $match=[regex]::Match($source,'private const string PrincipalSql = """(.*?)""";',[Text.RegularExpressions.RegexOptions]::Singleline)
        if(-not $match.Success){throw 'Maintenance guard missing'}
        $command.CommandText=$match.Groups[1].Value
        $reader=$command.ExecuteReader()
        try{
            if($reader.FieldCount -ne 9 -or -not $reader.Read()){throw 'Missing maintenance facts'}
            $expectedFacts=@(0,0,0,0,0,0,1,1,1)
            for($i=0;$i -lt 9;$i++){if($reader.IsDBNull($i) -or $reader.GetInt32($i) -ne $expectedFacts[$i]){throw 'Unsafe maintenance facts'}}
            if($reader.Read() -or $reader.NextResult()){throw 'Unexpected maintenance facts'}
        }finally{$reader.Dispose()}
        $result.principalSafe=$true
        $command.CommandText='SELECT TOP(0) password_hash FROM dbo.Account'
        try{[void]$command.ExecuteScalar()}catch [Data.SqlClient.SqlException]{if($_.Exception.Number -eq 229){$result.forbiddenReadDenied=$true}else{throw}}
        if(-not $result.forbiddenReadDenied){throw 'Maintenance business read allowed'}
    }else{
        $command.CommandText="SELECT CASE WHEN USER_NAME()='M0SchemaUser' AND IS_ROLEMEMBER('db_owner')=1 AND IS_SRVROLEMEMBER('sysadmin')=0 AND HAS_PERMS_BY_NAME(NULL,'SERVER','CONTROL SERVER')=0 THEN 1 ELSE 0 END"
        if([int]$command.ExecuteScalar() -ne 1){throw 'Schema authority mismatch'}
        $result.principalSafe=$true
    }
    $result.success=$true
}catch{$result.errorCategory='authentication_or_catalog_failed'}finally{if($connection){$connection.Dispose()}}
$result|ConvertTo-Json -Compress
if(-not $result.success){exit 1}