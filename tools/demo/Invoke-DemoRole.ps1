[CmdletBinding()]
param([Parameter(Mandatory)][ValidateSet('EcM0Runtime','EcM0Maint')][string]$Role,[Parameter(Mandatory)][ValidateSet('keys','csrf')][string]$Action)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace'
$credential=Import-Clixml ('C:\ProgramData\EnglishCenter\M0\'+$Role+'.credential.clixml')
try{
 if($credential.UserName -cne ($env:COMPUTERNAME+'\'+$Role)){throw 'Wrong credential'}
 $start=[Diagnostics.ProcessStartInfo]::new()
 if($Action -eq 'keys'){
  $start.FileName='C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe'
  $script=if($Role -eq 'EcM0Runtime'){'Initialize-DemoRuntimeKeys.ps1'}else{'Initialize-DemoMaintenanceProofKey.ps1'}
  $start.Arguments='-NoProfile -ExecutionPolicy Bypass -File "'+$workspace+'\tools\demo\'+$script+'"'
 }else{
  if($Role -ne 'EcM0Runtime'){throw 'CSRF runtime only'}
  $start.FileName='C:\Program Files\dotnet\dotnet.exe';$start.Arguments='"'+$workspace+'\.local-demo\release\host\EnglishCenter.Demo.dll" csrf'
 }
 $start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.LoadUserProfile=$true;$start.WorkingDirectory=$workspace
 $start.Domain=$env:COMPUTERNAME;$start.UserName=$Role;$start.Password=$credential.Password
 $start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
 $start.EnvironmentVariables['DOTNET_CLI_HOME']='C:\Users\'+$Role
 $process=[Diagnostics.Process]::Start($start);$output=$process.StandardOutput.ReadToEndAsync();$errors=$process.StandardError.ReadToEndAsync()
 if(-not $process.WaitForExit(60000)){throw 'Role provisioning deadline'}
 $output.GetAwaiter().GetResult();$errors.GetAwaiter().GetResult();if($process.ExitCode -ne 0){throw 'Role provisioning failed'}
}finally{$credential.Password.Dispose()}
