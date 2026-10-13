import { AccountActions } from '../../../app/AccountActions';
import { useCallback,useEffect,useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';
import { Modal } from '../../../components/ui/Modal';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import { useOwnedTask } from '../../../lib/useOwnedTask';
import type { Page } from '../../../lib/m1Wire';
import type { SessionController } from '../../identity/sessionController';
import { loadAdminAccount } from '../adminApi';
import type { AdminAccount } from '../types';
import { StaffCreateForm } from '../components/StaffCreateForm';
import { createStaff,searchAccounts,staffActions,staffCommand,staffLookup } from '../staffApi';
import type { Credential,StaffAction,StaffFields } from '../staffApi';
type Intent={requestId:string;action:string;target:number|null};
export function StaffAdminPage({controller,onPassword,initialId}:{controller:SessionController;onPassword:()=>void;initialId?:number}){
  const client=controller.client;
  const [name,setName]=useState(''),[email,setEmail]=useState(''),[phone,setPhone]=useState(''),[role,setRole]=useState(''),[status,setStatus]=useState('');
  const [data,setData]=useState<Page<AdminAccount>|null>(null),[account,setAccount]=useState<AdminAccount|null>(null);
  const [creating,setCreating]=useState(false),[preview,setPreview]=useState<StaffAction|null>(null),[secret,setSecret]=useState<Credential|null>(null),[unknown,setUnknown]=useState<Intent|null>(null);
  const clear=useCallback(()=>{setName('');setEmail('');setPhone('');setRole('');setStatus('');setData(null);setAccount(null);setCreating(false);setPreview(null);setSecret(null);setUnknown(null);},[]);
  const task=useOwnedTask(client,clear);
  const run=task.run;
  useEffect(()=>{const timer=window.setTimeout(()=>{if(initialId)void run(signal=>loadAdminAccount(client,initialId,signal),setAccount);else void run(signal=>searchAccounts(client,{page:1},signal),setData,undefined,()=>setData(null));},0);return()=>window.clearTimeout(timer);},[client,initialId,run]);
  const closeSecret=()=>{const own=secret?.accountId===client.auth.context?.accountId;setSecret(null);if(own)void controller.checkSession();};
  useEffect(()=>{if(!secret)return;const timer=setTimeout(()=>setSecret(null),Math.max(0,Date.parse(secret.temporaryPasswordExpiresAt)-Date.now()));return()=>clearTimeout(timer);},[secret]);
  const search=(page=1)=>void task.run(signal=>searchAccounts(client,{page,...name.trim()?{name}: {},...email.trim()?{email}:{},...phone.trim()?{phone}:{},...role?{role}:{},...status?{status}:{}},signal),setData);
  const read=(id:number)=>void task.run(signal=>loadAdminAccount(client,id,signal),setAccount);
  function issued(result:Credential|{outcome:'COMPLETED'}){setCreating(false);setAccount(null);if('temporaryPassword' in result)setSecret(result);else task.setNotice('Lệnh đã hoàn tất; mật khẩu cũ không thể lấy lại. Đối chiếu tài khoản trước khi chủ đích reset mới.');}
  function create(value:StaffFields){
    if(unknown)return;const requestId=crypto.randomUUID();void task.run(signal=>createStaff(client,value,requestId,signal),issued,()=>{setCreating(false);setUnknown({requestId,action:'ADMIN.STAFF.CREATE',target:null});});
  }
  function confirm(){
    if(!account||!preview||unknown)return;const target=account,action=preview,requestId=crypto.randomUUID();setPreview(null);
    void task.run(signal=>staffCommand(client,action,target.id,target.version,requestId,signal),result=>{
      if('account' in result)setAccount(result.account);else issued(result);
      setData(null);
    },()=>{setAccount(null);setUnknown({requestId,action:staffActions[action].path,target:target.id});},()=>{setAccount(null);setData(null);});
  }
  if(client.auth.context?.role!=='Admin'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền quản trị tài khoản."/>;
  return <section className="ec-work-page">
    <PageHeader title="Quản trị tài khoản"/>
    <div className="mb-4 flex flex-wrap gap-3"><Button disabled={task.pending||Boolean(unknown)||Boolean(secret)} onClick={()=>{setAccount(null);setCreating(true);}}>Thêm nhân sự</Button><AccountActions><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button></AccountActions><AccountActions><Button variant="secondary" disabled={task.pending} onClick={()=>{setSecret(null);void controller.logout();}}>Đăng xuất</Button></AccountActions></div>
    <form className="ec-work-filter mb-4 grid gap-3 md:grid-cols-3" onSubmit={e=>{e.preventDefault();search();}}>
      <TextField label="Tìm theo tên" value={name} disabled={task.pending} onChange={e=>setName(e.currentTarget.value)}/>
      <TextField label="Tìm theo email" value={email} disabled={task.pending} onChange={e=>setEmail(e.currentTarget.value)}/>
      <TextField label="Số điện thoại" value={phone} disabled={task.pending} onChange={e=>setPhone(e.currentTarget.value)}/>
      <label>Vai trò<select className="block min-h-[44px] w-full border" value={role} disabled={task.pending} onChange={e=>setRole(e.currentTarget.value)}><option value="">Tất cả</option>{['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director','Admin'].map(x=><option key={x}>{x}</option>)}</select></label>
      <label>Trạng thái<select className="block min-h-[44px] w-full border" value={status} disabled={task.pending} onChange={e=>setStatus(e.currentTarget.value)}><option value="">Tất cả</option>{['ACTIVE','LOCKED','DISABLED'].map(x=><option key={x}>{x}</option>)}</select></label>
      <Button type="submit" pending={task.pending}>Tìm kiếm</Button>
    </form>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang xử lý…"/>}
    {data&&<><p>{data.totalCount} tài khoản — trang {data.page}</p>{data.items.length===0?<Feedback kind="empty" message="Không có tài khoản phù hợp."/>:<div className="ec-work-table" role="region" aria-label="Danh sách tài khoản theo bộ lọc" tabIndex={0}><table className="w-full text-left"><caption className="sr-only">Danh sách tài khoản theo bộ lọc</caption><thead><tr><th>Họ tên</th><th>Email</th><th>Vai trò</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>{data.items.map(a=><tr key={a.id} className="border-b"><td>{a.fullName}</td><td>{a.email}</td><td>{a.role}</td><td>{a.status}</td><td><Button variant="secondary" disabled={task.pending} onClick={()=>read(a.id)}>Xem #{a.id}</Button></td></tr>)}</tbody></table></div>}
      <Button disabled={task.pending||data.page===1} onClick={()=>search(data.page-1)}>Trang trước</Button> <Button disabled={task.pending||data.page*data.pageSize>=data.totalCount} onClick={()=>search(data.page+1)}>Trang sau</Button></>}
    {account&&<article className="ec-work-card mt-4 border p-4"><h2 className="font-semibold break-words">#{account.id} — {account.fullName}</h2><p className="break-words">{account.email} · {account.role} · {account.status}</p><p>{account.mustChangePassword?'Bắt buộc đổi mật khẩu tạm':'Mật khẩu thông thường'}</p>
      <div className="mt-3 flex flex-wrap gap-3">{(Object.keys(staffActions) as StaffAction[]).map(action=><Button key={action} variant={action==='LOCK'||action==='DISABLE'?'danger':'secondary'} title={account.disabledReasons[action]} disabled={task.pending||Boolean(unknown)||Boolean(secret)||!account.allowedActions.includes(action)} onClick={()=>setPreview(action)}>{staffActions[action].label}</Button>)}<Button variant="secondary" disabled={task.pending} onClick={()=>read(account.id)}>Tải lại tài khoản</Button></div></article>}
    {unknown&&<div className="mt-4 space-x-3"><Button disabled={task.pending} onClick={()=>void task.run(signal=>staffLookup(client,unknown.action,unknown.requestId,signal),()=>{task.setNotice('Đã có completion. Tải lại để xem trạng thái; mật khẩu tạm không được trả lại.');setUnknown(null);})}>Kiểm tra kết quả</Button>{unknown.target&&<Button disabled={task.pending} onClick={()=>read(unknown.target!)}>Đối chiếu tài khoản</Button>}<Button variant="secondary" disabled={task.pending} onClick={()=>{setUnknown(null);setAccount(null);setData(null);task.setNotice('Kết thúc đối chiếu theo lựa chọn của bạn; kết quả cũ chưa xác nhận. Tải lại trước intent mới.');}}>Kết thúc đối chiếu</Button></div>}
    <Modal open={creating} title="Cấp tài khoản nhân sự" onRequestClose={()=>{if(!task.pending)setCreating(false);}}><StaffCreateForm pending={task.pending} onSubmit={create} onCancel={()=>setCreating(false)}/></Modal>
    <Modal open={Boolean(preview)} title="Xác nhận thao tác tài khoản" onRequestClose={()=>setPreview(null)}><p className="break-words">{preview&&staffActions[preview].label}: #{account?.id} — {account?.fullName} ({account?.email})?</p><p className="my-3">Khóa/ngừng dùng/reset hủy mọi phiên. Reset giữ trạng thái và yêu cầu đổi mật khẩu tạm. Mở/kích hoạt lại không khôi phục phiên cũ.</p><Button variant="danger" disabled={task.pending} onClick={confirm}>Xác nhận</Button></Modal>
    <Modal open={Boolean(secret)} title="Bàn giao mật khẩu tạm một lần" onRequestClose={closeSecret}><p className="break-words">{secret?.email}</p><p className="my-3 break-all select-all" aria-label="Mật khẩu tạm">{secret?.temporaryPassword}</p><p>Hết hạn sau 24 giờ. Người nhận phải đổi mật khẩu trước nghiệp vụ. Đóng cửa sổ sẽ xóa khỏi giao diện.</p><div className="mt-3 flex gap-3"><Button onClick={()=>{if(secret)void navigator.clipboard.writeText(secret.temporaryPassword).catch(()=>task.setNotice('Không sao chép được. Hãy bàn giao trực tiếp.'));}}>Sao chép mật khẩu</Button><Button onClick={closeSecret}>Đã bàn giao — đóng</Button></div></Modal>
  </section>;
}
