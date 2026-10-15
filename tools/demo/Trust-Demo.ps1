$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
$root='C:\ProgramData\EnglishCenter\DemoRuntime'
$owner=Get-Content 'C:\ProgramData\EnglishCenter\DemoOperator\ownership.json' -Raw|ConvertFrom-Json
$prepared=Get-Content ($root+'\prepared.json') -Raw|ConvertFrom-Json
if($owner.operatorSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value -or $prepared.ownerId -ne $owner.ownerId){throw 'Owner mismatch'}
$key=Get-Content ($root+'\key-manifest.json') -Raw|ConvertFrom-Json
$cert=[Security.Cryptography.X509Certificates.X509Certificate2]::new($root+'\https.cer')
if($key.phase -cne 'jwt_https_provisioned' -or $cert.HasPrivateKey -or $cert.Thumbprint -cne $key.httpsThumbprint -or (Get-FileHash ($root+'\https.cer') -Algorithm SHA256).Hash -cne $key.httpsLeafSha256 -or $cert.Subject -cne 'CN=localhost' -or $cert.NotAfter -lt (Get-Date)){throw 'Invalid public certificate'}
$store=[Security.Cryptography.X509Certificates.X509Store]::new('Root','LocalMachine')
try{$store.Open('ReadWrite');if($store.Certificates.Find('FindByThumbprint',$cert.Thumbprint,$false).Count){throw 'Already trusted'};$store.Add($cert)}finally{$store.Close()}
[IO.File]::WriteAllText('C:\Study\SOEN\main_workplace\.local-demo\trust.json','{"success":true,"publicLeafOnly":true}')
