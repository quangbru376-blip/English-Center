$ErrorActionPreference='Stop';Set-StrictMode -Version Latest
Add-Type -AssemblyName System.Security
$root='C:\ProgramData\EnglishCenter\DemoOperator'
$owner=Get-Content ($root+'\ownership.json') -Raw|ConvertFrom-Json
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
if($owner.operatorSid -ne $identity.User.Value){throw 'Wrong demo operator'}
$directory=Join-Path $env:LOCALAPPDATA 'EnglishCenterDemo'
if(-not (Test-Path -LiteralPath $directory)){[void][IO.Directory]::CreateDirectory($directory)}
$item=Get-Item -LiteralPath $directory;if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Reparse handoff directory'}
$acl=[Security.AccessControl.DirectorySecurity]::new();$acl.SetOwner($identity.User);$acl.SetAccessRuleProtection($true,$false)
foreach($sid in @($identity.User,[Security.Principal.SecurityIdentifier]::new('S-1-5-18'),[Security.Principal.SecurityIdentifier]::new('S-1-5-32-544'))){$acl.AddAccessRule([Security.AccessControl.FileSystemAccessRule]::new($sid,'FullControl','ContainerInherit,ObjectInherit','None','Allow'))}
Set-Acl -LiteralPath $directory -AclObject $acl
$clear=[Security.Cryptography.ProtectedData]::Unprotect([IO.File]::ReadAllBytes($root+'\accounts.dpapi'),$null,'CurrentUser')
try{$accounts=[Text.Encoding]::UTF8.GetString($clear)|ConvertFrom-Json}finally{[Array]::Clear($clear,0,$clear.Length)}
function Escape([string]$value){[Net.WebUtility]::HtmlEncode($value)}
$rows=foreach($a in $accounts){'<tr><td>'+(Escape $a.Role)+'</td><td>'+(Escape $a.Email)+'</td><td><code>'+(Escape $a.Password)+'</code></td><td>'+$(if($a.Temporary){'Mật khẩu tạm, bắt đổi trong 24 giờ'}elseif($a.Email -eq 'student49@demo.example'){'LOCKED'}elseif($a.Email -eq 'student50@demo.example'){'DISABLED'}else{'Sẵn sàng đăng nhập'})+'</td></tr>'}
$html=@'
<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'"><title>Tài khoản demo trung tâm Anh ngữ</title>
<style>body{font:16px system-ui;max-width:1200px;margin:32px auto;padding:16px;color:#163c3a}table{border-collapse:collapse;width:100%}th,td{border:1px solid #b7cfcc;padding:10px;text-align:left}code{user-select:all}tr:nth-child(even){background:#f0f7f5}</style>
<h1>Tài khoản demo localhost</h1><p>Mở <a href="https://localhost:8444">https://localhost:8444</a>. Đây là danh tính giả lập. Trang này chỉ là bản bàn giao trên máy của bạn, có chứa mật khẩu; không nằm trong Git hoặc website.</p>
<p>Bắt đầu: Student01 mới; Student02 lịch sử lượt hai; Student03 chờ chấm; Student09–12 có Band; Student13–15 chờ duyệt; Student17–23 học phí; Student29–32 đang học; Student33–36 bảng nháp; Student37–40 bị trả; Student41–44 chờ chốt; Student45–48 đã có kết quả. Teacher02 chấm đầu vào, Teacher08 điểm danh/điểm đang học. Các nhân viên đã đổi mật khẩu qua web.</p>
<table><thead><tr><th>Vai trò</th><th>Email</th><th>Mật khẩu hiện tại</th><th>Trạng thái</th></tr></thead><tbody>
'@
$html+=($rows -join "`n")+'</tbody></table></html>'
$path=Join-Path $directory 'tai-khoan-demo.html'
[IO.File]::WriteAllText($path,$html,[Text.UTF8Encoding]::new($false))
$accounts=$null;$html=$null;$rows=$null
Write-Output $path
