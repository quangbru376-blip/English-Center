[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ReceiptPath)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = 'C:\ProgramData\EnglishCenter\M0'
$names = @('EcM0Runtime','EcM0Maint','EcM0Schema')
$created = @()
$ownedRoot = $false
$resolvedReceipt = $null
$owner = [Security.Principal.WindowsIdentity]::GetCurrent().User
$administrator = [Security.Principal.WindowsPrincipal]::new([Security.Principal.WindowsIdentity]::GetCurrent())
$receipt = [ordered]@{ phase='W1'; success=$false; administratorToken=$false; createdUsers=0; compensatedUsers=0; credentialsProtected=$false; noAdministratorMembership=$false }
try {
    if (-not $administrator.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Elevation required' }
    $receipt.administratorToken = $true
    $candidateReceipt = [IO.Path]::GetFullPath($ReceiptPath)
    $receiptRoot = 'C:\Study\SOEN\main_workplace\docs\plan\V01-evidence\test-results\'
    if (-not $candidateReceipt.StartsWith($receiptRoot,[StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetExtension($candidateReceipt) -ne '.json' -or (Test-Path -LiteralPath $candidateReceipt)) { throw 'Invalid new receipt path' }
    $resolvedReceipt = $candidateReceipt
    if (Test-Path -LiteralPath $root) { throw 'Custody root already exists' }
    foreach ($name in $names) {
        if (Get-LocalUser -Name $name -ErrorAction SilentlyContinue) { throw 'Identity already exists' }
    }
    $ancestor = [IO.DirectoryInfo]::new([IO.Path]::GetDirectoryName($root))
    while ($null -ne $ancestor) {
        if ($ancestor.Exists -and (($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)) { throw 'Reparse custody ancestor' }
        $ancestor = $ancestor.Parent
    }
    $parent = [IO.Path]::GetDirectoryName($root)
    if (-not (Test-Path -LiteralPath $parent)) { [IO.Directory]::CreateDirectory($parent) | Out-Null }
    [IO.Directory]::CreateDirectory($root) | Out-Null
    $ownedRoot = $true
    $acl = [Security.AccessControl.DirectorySecurity]::new()
    $acl.SetOwner($owner); $acl.SetAccessRuleProtection($true,$false)
    foreach ($sid in @($owner,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))) {
        $rule = [Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow')
        $acl.AddAccessRule($rule)
    }
    Set-Acl -LiteralPath $root -AclObject $acl
    $manifest = [ordered]@{ ownerId=[Guid]::NewGuid().ToString('N'); operatorSid=$owner.Value; phase='provisioning'; identities=@() }
    foreach ($name in $names) {
        $bytes = New-Object byte[] 32
        $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
        try {
            $rng.GetBytes($bytes)
            $password = [Convert]::ToBase64String($bytes) + 'aA1!'
            $secure = ConvertTo-SecureString -String $password -AsPlainText -Force
            try {
                $user = New-LocalUser -Name $name -Password $secure -AccountNeverExpires -PasswordNeverExpires -UserMayNotChangePassword -Description 'English Center LOCAL_DEV M0 dedicated identity'
                $created += [pscustomobject]@{ name=$name; sid=$user.SID.Value }
                $credential = [Management.Automation.PSCredential]::new(($env:COMPUTERNAME+'\'+$name),$secure)
                $credential | Export-Clixml -LiteralPath (Join-Path $root ($name+'.credential.clixml'))
                $credential = $null
                $manifest.identities += [ordered]@{ name=$name; sid=$user.SID.Value; credentialFile=($name+'.credential.clixml') }
            } finally { $secure.Dispose(); $password=$null }
        } finally { [Array]::Clear($bytes,0,$bytes.Length); $rng.Dispose() }
    }
    $adminMembers = @(Get-LocalGroupMember -SID 'S-1-5-32-544')
    foreach ($identity in $created) {
        $actual = Get-LocalUser -Name $identity.name
        if ($actual.SID.Value -ne $identity.sid -or @($adminMembers | Where-Object { $_.SID.Value -eq $identity.sid }).Count -ne 0) { throw 'Identity authority mismatch' }
        $credential = Import-Clixml -LiteralPath (Join-Path $root ($identity.name+'.credential.clixml'))
        try {
            if ($credential -isnot [Management.Automation.PSCredential] -or $credential.GetNetworkCredential().Password.Length -lt 32) { throw 'Protected credential unavailable' }
        } finally { if ($credential -is [Management.Automation.PSCredential]) { $credential.Password.Dispose() }; $credential=$null }
    }
    $actualAcl = Get-Acl -LiteralPath $root
    if (-not $actualAcl.AreAccessRulesProtected -or $actualAcl.Owner -ne $owner.Translate([Security.Principal.NTAccount]).Value) { throw 'Custody ACL mismatch' }
    $allowedSids = @($owner.Value,'S-1-5-18','S-1-5-32-544')
    foreach ($custodyPath in @($root) + @($created | ForEach-Object { Join-Path $root ($_.name+'.credential.clixml') })) {
        $custodyAcl = Get-Acl -LiteralPath $custodyPath
        foreach ($rule in $custodyAcl.Access) {
            if ($rule.IdentityReference.Translate([Security.Principal.SecurityIdentifier]).Value -notin $allowedSids -or $rule.AccessControlType -ne 'Allow') { throw 'Unexpected custody access' }
        }
    }
    $manifest.phase = 'identities_ready'
    [IO.File]::WriteAllText((Join-Path $root 'ownership.json'),($manifest | ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
    $receipt.success=$true; $receipt.createdUsers=$created.Count; $receipt.credentialsProtected=$true; $receipt.noAdministratorMembership=$true
} catch {
    foreach ($identity in $created) {
        try {
            $actual = Get-LocalUser -Name $identity.name -ErrorAction SilentlyContinue
            if ($null -ne $actual -and $actual.SID.Value -eq $identity.sid) {
                Remove-LocalUser -SID $actual.SID
                $receipt.compensatedUsers++
            }
        } catch { $receipt.cleanupIncomplete = $true }
    }
    $receipt.errorCategory = 'provisioning_failed'
    $receipt.createdUsers = $created.Count
    # The new protected root is retained for operator inspection, never recursively deleted.
    if ($ownedRoot) {
        [IO.File]::WriteAllText((Join-Path $root 'failed.json'),($receipt | ConvertTo-Json),[Text.UTF8Encoding]::new($false))
    }
}
if ($resolvedReceipt) {
    $receiptBytes = [Text.UTF8Encoding]::new($false).GetBytes(($receipt | ConvertTo-Json))
    $receiptStream = [IO.File]::Open($resolvedReceipt,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try { $receiptStream.Write($receiptBytes,0,$receiptBytes.Length) } finally { $receiptStream.Dispose() }
}
if (-not $receipt.success) { exit 1 }

