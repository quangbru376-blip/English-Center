[CmdletBinding()]
param([switch]$Stop)
$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$workspace='C:\Study\SOEN\main_workplace';$root='C:\ProgramData\EnglishCenter\DemoOperator';$state=$workspace+'\.local-demo'
if($Stop){[IO.File]::WriteAllText(($state+'\stop.request'),'stop');return}
if(Test-Path ($state+'\running.json')){
 $prior=Get-Content ($state+'\running.json') -Raw|ConvertFrom-Json
 if(Get-Process -Id $prior.launcherPid -ErrorAction SilentlyContinue){throw 'Demo launcher already running'}
}
if(Test-Path ($state+'\stop.request')){Remove-Item -LiteralPath ($state+'\stop.request')}
if(Test-Path ($state+'\launcher-error.txt')){[IO.File]::Move(($state+'\launcher-error.txt'),($state+'\launcher-error-'+[DateTime]::UtcNow.Ticks+'.txt'))}
$owner=Get-Content ($root+'\ownership.json') -Raw|ConvertFrom-Json
if($owner.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or $owner.purpose -cne 'LOCAL_DEMO' -or -not (Test-Path ($root+'\seed.json'))){throw 'Demo ownership/seed missing'}
if(Get-NetTCPConnection -State Listen -LocalPort 8444 -ErrorAction SilentlyContinue){throw 'Demo port occupied'}
$sessions=[Collections.Generic.List[object]]::new();$client=$null
try{
 foreach($role in @('EcM0Maint','EcM0Runtime')){
  $credential=Import-Clixml ('C:\ProgramData\EnglishCenter\M0\'+$role+'.credential.clixml')
  try{
   if($credential.UserName -cne ($env:COMPUTERNAME+'\'+$role)){throw 'Credential identity mismatch'}
   $mode=if($role -eq 'EcM0Maint'){'maintenance'}else{'runtime'}
   $start=[Diagnostics.ProcessStartInfo]::new('C:\Program Files\dotnet\dotnet.exe')
   $start.Arguments='"'+$workspace+'\.local-demo\release\host\EnglishCenter.Demo.dll" '+$mode
   $start.UseShellExecute=$false;$start.CreateNoWindow=$true;$start.LoadUserProfile=$true;$start.WorkingDirectory=$workspace
   $start.Domain=$env:COMPUTERNAME;$start.UserName=$role;$start.Password=$credential.Password
   $start.RedirectStandardInput=$true;$start.RedirectStandardOutput=$true;$start.RedirectStandardError=$true
   $start.EnvironmentVariables['DOTNET_CLI_HOME']='C:\Users\'+$role
   $start.EnvironmentVariables['TEMP']='C:\Users\'+$role+'\AppData\Local\Temp';$start.EnvironmentVariables['TMP']=$start.EnvironmentVariables['TEMP']
   $process=[Diagnostics.Process]::Start($start)
   $sessions.Add([pscustomobject]@{Role=$role;Process=$process;Output=$process.StandardOutput.ReadToEndAsync();Errors=$process.StandardError.ReadToEndAsync()})
  }finally{$credential.Password.Dispose()}
  if($role -eq 'EcM0Maint'){Start-Sleep -Milliseconds 1000}
 }
 Start-Sleep -Milliseconds 6000
 $client=[Net.Http.HttpClient]::new();$client.Timeout=[TimeSpan]::FromSeconds(8)
 $response=$client.GetAsync('https://localhost:8444/health/ready').GetAwaiter().GetResult()
 try{if([int]$response.StatusCode -ne 200 -or $response.Content.ReadAsStringAsync().GetAwaiter().GetResult() -cne 'ready'){throw 'Demo readiness refused'}}finally{$response.Dispose()}
 [IO.File]::WriteAllText(($state+'\running.json'),(@{launcherPid=$PID;startedAt=[DateTime]::UtcNow;url='https://localhost:8444';ready=$true;childPids=@($sessions|ForEach-Object {$_.Process.Id})}|ConvertTo-Json))
 Write-Output 'Demo ready: https://localhost:8444 (use -Stop from another PowerShell window to stop gracefully).'
 while(-not (Test-Path ($state+'\stop.request'))){foreach($session in $sessions){if($session.Process.HasExited){throw ('Demo process exited: '+$session.Role+' code '+$session.Process.ExitCode)}};Start-Sleep -Seconds 1}
}catch{[IO.File]::WriteAllText(($state+'\launcher-error.txt'),$_.Exception.Message);throw}
finally{
 foreach($session in $sessions){
  if(-not $session.Process.HasExited){$session.Process.StandardInput.Close();if(-not $session.Process.WaitForExit(40000)){$session.Process.Kill();[void]$session.Process.WaitForExit(5000)}}
  [IO.File]::WriteAllText(($state+'\'+$session.Role+'-output.txt'),$session.Output.GetAwaiter().GetResult())
  [IO.File]::WriteAllText(($state+'\'+$session.Role+'-error.txt'),$session.Errors.GetAwaiter().GetResult());$session.Process.Dispose()
 }
 if($client){$client.Dispose()}
 [IO.File]::WriteAllText(($state+'\stopped.json'),(@{launcherPid=$PID;stoppedAt=[DateTime]::UtcNow}|ConvertTo-Json))
}
