param([switch]$Focused,[switch]$Details)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
Add-Type -AssemblyName System.Security
$root='C:\ProgramData\EnglishCenter\DemoOperator';$path=$root+'\accounts.dpapi';$pending=$root+'\accounts-pending.dpapi'
$encoding=[Text.UTF8Encoding]::new($false)
function Protect-Ledger($accounts,[string]$target){
 $clear=$encoding.GetBytes(($accounts|ConvertTo-Json -Depth 6 -Compress))
 try{[IO.File]::WriteAllBytes($target,[Security.Cryptography.ProtectedData]::Protect($clear,$null,'CurrentUser'))}finally{[Array]::Clear($clear,0,$clear.Length)}
}
$clear=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($path),$null,'CurrentUser')
try{$accounts=$encoding.GetString($clear)|ConvertFrom-Json}finally{[Array]::Clear($clear,0,$clear.Length)}
foreach($a in $accounts){if($a.Temporary){$a|Add-Member -NotePropertyName NextPassword -NotePropertyValue ([Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(16)))}}
if(Test-Path $pending){throw 'Pending password workflow exists; inspect recovery before retry'}
Protect-Ledger $accounts $pending
$start=[Diagnostics.ProcessStartInfo]::new('C:\Users\asus\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe')
$start.Arguments='"C:\Study\SOEN\main_workplace\tools\demo\Verify-Demo.cjs"'+$(if($Focused){' --focused'}elseif($Details){' --details'}else{''});$start.UseShellExecute=$false;$start.CreateNoWindow=$true
$start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
$process=[Diagnostics.Process]::Start($start);$errors=$process.StandardError.ReadToEndAsync()
$process.StandardInput.Write(($accounts|ConvertTo-Json -Depth 6 -Compress));$process.StandardInput.Close()
while($null -ne ($line=$process.StandardOutput.ReadLine())){
 $item=$line|ConvertFrom-Json
 if($item.PSObject.Properties.Name -contains 'changed'){
  $a=@($accounts|Where-Object {$_.Email -ceq $item.changed});if($a.Count -ne 1 -or -not $a[0].Temporary){throw 'Unexpected password progress'}
  $a[0].Password=$a[0].NextPassword;$a[0].Temporary=$false;$a[0].PSObject.Properties.Remove('NextPassword');Protect-Ledger $accounts $path
 }
 $line
}
$process.WaitForExit();$errors.GetAwaiter().GetResult();if($process.ExitCode -ne 0){throw 'Demo browser verification failed'}
Remove-Item -LiteralPath $pending
