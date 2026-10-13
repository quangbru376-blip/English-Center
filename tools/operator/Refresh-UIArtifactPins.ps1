[CmdletBinding()]
param([switch]$ValidateOnly)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace'
$evidence=Join-Path $workspace 'docs\plan\UI-refresh-evidence\stage6'
$targets=@('C:\ProgramData\EnglishCenter\M0Runtime\patch-manifest.json','C:\ProgramData\EnglishCenter\M0Maintenance\patch-manifest.json')
$receipt=[ordered]@{success=$false;validationOnly=[bool]$ValidateOnly;changedFiles=0;changedPinsPerFile=2;profileKeysAclUnchanged=$false;errorCategory=$null}
$receiptName=if($ValidateOnly){'runtime-pin-validation.json'}else{'runtime-pin-refresh.json'}
if(Test-Path -LiteralPath (Join-Path $evidence $receiptName)){throw 'Receipt already exists'}
$originals=@{};$written=[Collections.Generic.List[string]]::new()
function Canonical($value){return $value|ConvertTo-Json -Depth 8 -Compress}
try {
    $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
    if(-not ([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Admin required'}
    if(@(Get-NetTCPConnection -State Listen -LocalPort 8443 -ErrorAction SilentlyContinue).Count -ne 0){throw 'Operational listener active'}
    if((Get-FileHash -LiteralPath (Join-Path $evidence 'runtime-pins-before.json') -Algorithm SHA256).Hash -cne '1C2D4E0D950609A705974EDDDE9FE907DDD2890AF832A209E02F9C83E360C415'){throw 'Review receipt drift'}
    if((Get-FileHash -LiteralPath (Join-Path $evidence 'runtime-pins-reviewed.json') -Algorithm SHA256).Hash -cne 'A07C9F2AC377289E2D2D7C916C11026585AEEEA0966247B8943BAC5D8029BF0E'){throw 'Review receipt drift'}
    $old=Get-Content -LiteralPath (Join-Path $evidence 'runtime-pins-before.json') -Raw|ConvertFrom-Json
    $new=Get-Content -LiteralPath (Join-Path $evidence 'runtime-pins-reviewed.json') -Raw|ConvertFrom-Json
    if($old.artifacts.Count -ne 5 -or $new.artifacts.Count -ne 5){throw 'Wrong pin set'}
    foreach($key in @('dotNet','aspNet','ef','sql','reviewed','nextReview')){if($old.$key -cne $new.$key){throw 'Version or date change'}}
    $expected=@('backend/global.json','backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj','backend/src/EnglishCenter.Api/packages.lock.json','frontend/package.json','frontend/package-lock.json')
    for($i=0;$i -lt 5;$i++){
        $a=$old.artifacts[$i];$b=$new.artifacts[$i]
        if($a.relativePath -cne $expected[$i] -or $b.relativePath -cne $expected[$i]){throw 'Artifact scope'}
        $artifact=Get-Item -LiteralPath (Join-Path $workspace $expected[$i])
        if($artifact.Length -ne $b.length -or (Get-FileHash -LiteralPath $artifact.FullName -Algorithm SHA256).Hash -cne $b.sha256){throw 'Source drift'}
        if($i -lt 3 -and (Canonical $a) -cne (Canonical $b)){throw 'Backend change'}
        if($i -ge 3 -and (Canonical $a) -ceq (Canonical $b)){throw 'Missing reviewed change'}
    }
    $newText=Get-Content -LiteralPath (Join-Path $evidence 'runtime-pins-reviewed.json') -Raw
    foreach($target in $targets){
        $item=Get-Item -LiteralPath $target;$ancestor=$item
        while($ancestor){if(($ancestor.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0){throw 'Reparse target'};if($ancestor -is [IO.FileInfo]){$ancestor=$ancestor.Directory}else{$ancestor=$ancestor.Parent}}
        if($item.Length -le 0 -or $item.Length -gt 65536){throw 'Descriptor size'}
        $originals[$target]=[IO.File]::ReadAllBytes($target)
        $installed=Get-Content -LiteralPath $target -Raw|ConvertFrom-Json
        if((Canonical $installed) -cne (Canonical $old)){throw 'Installed manifest drift'}
    }
    if(-not $ValidateOnly){
        foreach($target in $targets){
            $beforeAcl=(Get-Acl -LiteralPath $target).Sddl
            $written.Add($target)
            [IO.File]::WriteAllText($target,$newText,[Text.UTF8Encoding]::new($false))
            if((Get-Acl -LiteralPath $target).Sddl -cne $beforeAcl -or (Canonical (Get-Content -LiteralPath $target -Raw|ConvertFrom-Json)) -cne (Canonical $new)){throw 'Write verification failed'}
            $receipt.changedFiles++
        }
    }
    $receipt.profileKeysAclUnchanged=$true;$receipt.success=$true
}catch {
    $receipt.errorCategory='exact_frontend_pin_refresh_refused'
    foreach($target in $written){[IO.File]::WriteAllBytes($target,$originals[$target])}
    $receipt.changedFiles=0
}finally {
    $name=if($ValidateOnly){'runtime-pin-validation.json'}else{'runtime-pin-refresh.json'}
    $path=Join-Path $evidence $name
    if(Test-Path -LiteralPath $path){throw 'Receipt already exists'}
    [IO.File]::WriteAllText($path,($receipt|ConvertTo-Json),[Text.UTF8Encoding]::new($false))
}
if(-not $receipt.success){exit 1}
