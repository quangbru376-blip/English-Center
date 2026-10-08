import { useCallback, useEffect, useRef, useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';
import { Modal } from '../../../components/ui/Modal';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import type { SessionController } from '../../identity/sessionController';
import { AdminAccountPanel } from '../components/AdminAccountPanel';
import { loadAdminAccount, lockAdminAccount, lookupAdminLock } from '../adminApi';
import { createLockIntent } from '../lockIntent';
import type { AdminAccount } from '../types';

const roles = { Student:'Học viên',Admissions:'Tuyển sinh',AcademicAffairs:'Giáo vụ',Teacher:'Giáo viên',Accountant:'Kế toán',Director:'Giám đốc',Admin:'Quản trị viên' };
const states = { ACTIVE:'Đang hoạt động',LOCKED:'Đã khóa',DISABLED:'Ngừng sử dụng' };
const dateLabel = (instant: string | null) => instant ? new Intl.DateTimeFormat('vi-VN',{dateStyle:'short',timeStyle:'short',timeZone:'Asia/Ho_Chi_Minh'}).format(new Date(instant)) : 'Không có';
type Unconfirmed = { requestId:string; targetId:number };

export function AdminAccountPage({ controller, initialId, onDetail, onPassword }: {
  controller:SessionController;initialId?:number;onDetail:(id:number)=>void;onPassword:()=>void;
}) {
  const client=controller.client;
  const [input,setInput]=useState(initialId?.toString()??''); const [account,setAccount]=useState<AdminAccount|null>(null);
  const [preview,setPreview]=useState<AdminAccount|null>(null);const [unknown,setUnknown]=useState<Unconfirmed|null>(null);
  const [notice,setNotice]=useState<string|null>(null);const [pending,setPending]=useState(false);
  const epoch=useRef(0),busy=useRef(false),abort=useRef<AbortController|null>(null);
  const [lastTarget,setLastTarget]=useState<number|null>(initialId??null);
  const permitted=client.auth.context?.role==='Admin'&&client.auth.context.scope==='NORMAL';
  useEffect(()=>{
    const cancel=()=>{epoch.current++;abort.current?.abort();busy.current=false;};
    const off=client.auth.requests.registerCleanup(()=>{cancel();setLastTarget(null);setInput('');setAccount(null);setPreview(null);setUnknown(null);setNotice(null);setPending(false);});
    return()=>{cancel();off();};
  },[client]);
  const read=useCallback(async(id:number)=>{
    if(busy.current||client.auth.context?.role!=='Admin'||client.auth.context.scope!=='NORMAL')return;
    busy.current=true;setPending(true);setAccount(null);setPreview(null);setNotice(null);setLastTarget(id);
    const current=epoch.current;abort.current=new AbortController();const result=await loadAdminAccount(client,id,abort.current.signal);
    if(current!==epoch.current)return;busy.current=false;setPending(false);
    if(result.kind==='success'){setAccount(result.value);setInput(id.toString());}
    else setNotice(result.kind==='error'?result.error.message:'Chưa đọc được tài khoản. Hãy kiểm tra lại.');
  },[client]);
  useEffect(()=>{if(initialId)void read(initialId);},[initialId,read]);
  async function confirm(){
    if(!preview||busy.current||!client.auth.csrf)return;
    let intent;
    try{intent=createLockIntent(preview);}catch{setPreview(null);setNotice('Chưa tạo được mã yêu cầu. Lệnh chưa gửi.');return;}
    setPreview(null);setUnknown(null);setNotice(null);busy.current=true;setPending(true);
    const current=epoch.current;abort.current=new AbortController();const result=await lockAdminAccount(client,intent,abort.current.signal);
    if(current!==epoch.current)return;busy.current=false;setPending(false);
    if(result.kind==='success'){setAccount(result.value.account);setNotice('Server đã xác nhận thao tác. Dữ liệu đang hiển thị là trạng thái hiện tại.');}
    else{
      setAccount(null);
      if(result.kind==='error'&&result.outcome==='rejected')setNotice(result.status===409?'Dữ liệu hoặc điều kiện đã thay đổi. Tải lại rồi xác nhận intent mới; không tự gửi lại.':result.error.message);
      else if(result.kind==='blocked')setNotice('Chưa gửi lệnh. Hãy kiểm tra lại ngữ cảnh.');
      else{setInput('');setUnknown({requestId:intent.requestId,targetId:intent.targetId});setNotice('Chưa xác định kết quả khóa. Kiểm tra kết quả hoặc tải lại; không tự gửi lại.');}
    }
  }
  async function checkResult(){
    if(!unknown||busy.current)return;busy.current=true;setPending(true);const current=epoch.current;
    abort.current=new AbortController();const result=await lookupAdminLock(client,unknown.requestId,unknown.targetId,abort.current.signal);
    if(current!==epoch.current)return;busy.current=false;setPending(false);
    setNotice(result.kind==='success'?'Có completion đã xác nhận. Tải lại tài khoản để xem trạng thái hiện tại.':result.kind==='error'&&result.status===404?
      'Chưa xác nhận được kết quả. Điều này không chứng minh lệnh đã rollback; hãy đối chiếu trạng thái.':result.kind==='error'?result.error.message:'Chưa đọc được kết quả. Không tự gửi lại lệnh.');
    if(result.kind==='success')setUnknown(null);
  }
  if(!permitted)return <Feedback kind="error" message="Không có quyền xem tài khoản Admin."/>;
  return <section>
    <PageHeader title="Tài khoản"/>
    <p className="mb-4">Pilot chỉ xem chi tiết và khóa tài khoản theo ID; các chức năng khác chưa triển khai.</p>
    <form className="mb-4 flex flex-wrap items-end gap-3" onSubmit={event=>{
      event.preventDefault();if(pending)return;
      if(!/^[1-9]\d{0,9}$/.test(input)||Number(input)>2147483647){setNotice('ID tài khoản không hợp lệ.');return;}
      setUnknown(null);onDetail(Number(input));void read(Number(input));
    }}>
      <TextField id="admin-target" label="ID tài khoản" inputMode="numeric" value={input} disabled={pending} onChange={event=>setInput(event.currentTarget.value)}/>
      <Button type="submit" pending={pending}>Xem tài khoản</Button>
    </form>
    {notice&&<div className="mb-4"><Feedback kind="error" message={notice}/></div>}
    {pending&&<Feedback kind="loading" message="Đang xử lý…"/>}
    {account&&<AdminAccountPanel account={{...account,role:roles[account.role],status:states[account.status]}}
      createdLabel={dateLabel(account.createdAt)} temporaryExpiryLabel={dateLabel(account.temporaryPasswordExpiresAt)}
      canLock={!unknown&&account.allowedActions.includes('LOCK')&&account.status==='ACTIVE'&&Boolean(client.auth.csrf)}
      lockDisabledReason={unknown?'Kết quả cũ chưa xác nhận; cần đối chiếu trước intent mới.':!client.auth.csrf?'Cần lấy lại CSRF trước thao tác.':account.disabledReasons.LOCK??null} pending={pending}
      onRequestLock={()=>setPreview(account)} onReload={()=>void read(account.id)}/>}
    {unknown&&<Button className="mr-3 mt-4" disabled={pending} variant="secondary" onClick={()=>void checkResult()}>Kiểm tra kết quả khóa</Button>}
    {unknown&&account&&<Button className="mt-4" disabled={pending} variant="secondary" onClick={()=>{
      setUnknown(null);setNotice('Bạn đã chủ đích kết thúc đối chiếu. Kết quả cũ vẫn chưa xác nhận; lệnh tiếp theo là intent mới.');
    }}>Kết thúc đối chiếu để tạo intent mới</Button>}
    {!account&&lastTarget&&<Button className="mt-4" disabled={pending} variant="secondary" onClick={()=>void read(lastTarget)}>Tải lại tài khoản</Button>}
    <div className="mt-4 flex flex-wrap gap-3">
      <Button disabled={pending} variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button>
      <Button disabled={pending||!client.auth.csrf} variant="secondary" onClick={()=>void controller.logout()}>Đăng xuất</Button>
      <Button disabled={pending} variant="secondary" onClick={()=>void controller.checkSession()}>Kiểm tra lại phiên</Button>
    </div>
    <Modal open={Boolean(preview)} title="Xác nhận khóa tài khoản" onRequestClose={()=>setPreview(null)}>
      <p className="break-words">Khóa tài khoản #{preview?.id} — {preview?.fullName} ({preview?.email})?</p>
      <p className="mb-4">Hủy mọi phiên của tài khoản này; giữ dữ liệu và lịch sử. Server kiểm tra lại quyền, trạng thái và phiên bản.</p>
      <Button variant="danger" disabled={pending} onClick={()=>void confirm()}>Xác nhận khóa</Button>
    </Modal>
  </section>;
}
