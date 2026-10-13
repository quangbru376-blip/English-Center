import { AccountActions } from '../../app/AccountActions';
import { useCallback, useEffect, useState } from 'react';
import type { SessionController } from '../identity/sessionController';
import type { Page } from '../../lib/m1Wire';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { PageHeader } from '../../components/ui/PageHeader';
import { Modal } from '../../components/ui/Modal';
import { registrationLabel, bookingLabel, invoiceLabel, scoreLabel, vietnamDateTime } from '../../lib/enrollmentDisplay';
import { AdmissionReasonForm } from './components/AdmissionReasonForm';
import { searchAdmissions, readAdmission, previewApproval, previewAdmissionCancel, admissionCommand, admissionLookup } from './admissionsApi';
import type { AdmissionSummary, AdmissionDetail, Approval, Cancellation, AdmissionAction } from './admissionsApi';

const emptyFilter={name:'',email:'',phone:'',courseId:'',status:'',createdFromDate:'',createdToDate:'',sort:'CREATED_DESC'};
const labels={APPROVE:'Duyệt hồ sơ',REJECT:'Từ chối hồ sơ',CANCEL:'Hủy hồ sơ'};
const money=(v:string)=>v.replace(/\B(?=(\d{3})+(?!\d))/g,'.')+' ₫';
export function AdmissionsPage({controller,onPassword,initialId}:{controller:SessionController;onPassword:()=>void;initialId?:number}) {
  const client=controller.client;
  const [filters,setFilters]=useState(emptyFilter),[list,setList]=useState<Page<AdmissionSummary>|null>(null),[detail,setDetail]=useState<AdmissionDetail|null>(null);
  const [action,setAction]=useState<AdmissionAction|null>(null),[preview,setPreview]=useState<Approval|Cancellation|null>(null),[reason,setReason]=useState('');
  const [intent,setIntent]=useState<{action:AdmissionAction;id:number;body:unknown;summary:string}|null>(null),[unknown,setUnknown]=useState<{action:AdmissionAction;id:number;requestId:string}|null>(null);
  const clear=useCallback(()=>{setFilters(emptyFilter);setList(null);setDetail(null);setAction(null);setPreview(null);setReason('');setIntent(null);setUnknown(null);},[]);
  const task=useOwnedTask(client,clear);
  const run=task.run;
  useEffect(()=>{const timer=window.setTimeout(()=>{if(initialId)void run(signal=>readAdmission(client,initialId,signal),setDetail,undefined,()=>setDetail(null));else void run(signal=>searchAdmissions(client,{sort:'CREATED_DESC',page:1},signal),setList,undefined,()=>setList(null));},0);return()=>window.clearTimeout(timer);},[client,initialId,run]);
  const resetDraft=()=>{setAction(null);setPreview(null);setReason('');setIntent(null);};
  const read=(id:number)=>{resetDraft();void task.run(signal=>readAdmission(client,id,signal),setDetail,undefined,()=>setDetail(null));};
  const search=(page=1)=>{
    if(filters.courseId&&!/^[1-9]\d{0,9}$/.test(filters.courseId)||Number(filters.courseId)>2147483647){task.setNotice('Mã định danh khóa phải là số nguyên dương hợp lệ.');return;}
    resetDraft();setDetail(null);
    const body={...Object.fromEntries(Object.entries(filters).filter(([key,v])=>v&&key!=='courseId')),...filters.courseId?{courseId:Number(filters.courseId)}:{},page};
    void task.run(signal=>searchAdmissions(client,body,signal),setList,undefined,()=>setList(null));
  };
  function prepare(next:AdmissionAction) {
    if(!detail||unknown)return;resetDraft();setAction(next);
    if(next==='REJECT')return;
    if(next==='APPROVE')void task.run(signal=>previewApproval(client,detail.id,signal),setPreview,undefined,()=>{setPreview(null);setAction(null);});
    else void task.run(signal=>previewAdmissionCancel(client,detail.id,signal),setPreview,undefined,()=>{setPreview(null);setAction(null);});
  }
  function propose() {
    if(!detail||!action||unknown)return;
    const normalized=reason.normalize('NFC').trim();
    if(action!=='APPROVE'&&(!normalized||Array.from(normalized).length>1000)){task.setNotice('Lý do bắt buộc, tối đa 1.000 ký tự.');return;}
    if(action!=='REJECT'&&(!preview?.eligible||!preview.manifest))return;
    const body=action==='REJECT'?{reason:normalized,expectedVersions:{registration:detail.version}}:{...structuredClone(preview!.manifest),...action==='CANCEL'?{reason:normalized}:{}};
    const summary=action==='APPROVE'&&preview&&'course'in preview?`${preview.course.code}: ${preview.course.name} — học phí ${money(preview.course.tuition)}; phát hành hóa đơn chưa thanh toán, không xếp lớp.`:action==='CANCEL'&&preview&&'effects'in preview?`Hủy ${preview.effects.futureBookings.length} lượt đặt tương lai; ${preview.effects.invoice?'hủy hóa đơn '+preview.effects.invoice.number:'chưa có hóa đơn'}. Giữ kết quả và lịch sử đã có.`:'Giữ kết quả thi và Band; không phát hành hóa đơn.';
    setIntent({action,id:detail.id,body,summary});
  }
  function submit() {
    if(!intent||unknown)return;const frozen=intent,requestId=crypto.randomUUID();resetDraft();setList(null);
    void task.run(signal=>admissionCommand(client,frozen.action,frozen.id,frozen.body,requestId,signal),r=>setDetail(r.registration),()=>{setDetail(null);setUnknown({action:frozen.action,id:frozen.id,requestId});},()=>setDetail(null));
  }
  if(client.auth.context?.role!=='Admissions'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền tuyển sinh."/>;
  return <section className="ec-work-page"><PageHeader title="Hồ sơ tuyển sinh"/><p className="mb-3">Ngày và thời gian theo giờ Việt Nam. Tuyển sinh không sửa hồ sơ cá nhân hoặc điểm.</p>
    <div className="mb-4 flex flex-wrap gap-3"><AccountActions><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button></AccountActions><AccountActions><Button variant="secondary" disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></AccountActions></div>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang xử lý…"/>}
    {unknown&&<div className="my-4"><Feedback kind="error" message="Chưa xác định kết quả. Không gửi lại lệnh; hãy tra cứu thủ công và đọc hồ sơ mới."/><Button disabled={task.pending} onClick={()=>void task.run(signal=>admissionLookup(client,unknown.action,unknown.requestId,unknown.id,signal),()=>{const id=unknown.id;setUnknown(null);read(id);})}>Tra cứu kết quả thủ công</Button></div>}
    <form onSubmit={e=>{e.preventDefault();if(!task.pending&&!unknown)search();}} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      {(['name','email','phone','courseId','createdFromDate','createdToDate'] as const).map((key,i)=><TextField key={key} label={['Tên học viên','Email học viên','Số điện thoại','Mã định danh khóa','Tạo từ ngày','Tạo đến ngày'][i]} type={key.includes('Date')?'date':'text'} value={filters[key]} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,[key]:e.target.value})}/>)}
      <label>Trạng thái hồ sơ<select aria-label="Trạng thái hồ sơ" className="block min-h-11 w-full border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2 text-[var(--ec-text)]" value={filters.status} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,status:e.target.value})}><option value="">Tất cả</option>{['PENDING','TESTING','BAND_ASSIGNED','CONFIRMED','ADMITTED','REJECTED','CANCELLED'].map(s=><option key={s} value={s}>{registrationLabel(s)}</option>)}</select></label>
      <label>Thứ tự<select aria-label="Thứ tự hồ sơ" className="block min-h-11 w-full border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2 text-[var(--ec-text)]" value={filters.sort} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,sort:e.target.value})}><option value="CREATED_DESC">Mới nhất trước</option><option value="CREATED_ASC">Cũ nhất trước</option></select></label><Button type="submit" disabled={task.pending||!!unknown}>Tìm hồ sơ</Button>
    </form>
    {list&&<div className="my-4">{list.items.length===0?<Feedback kind="empty" message="Không có hồ sơ phù hợp."/>:list.items.map(r=><article key={r.id} className="ec-work-card my-3 border-b border-[var(--ec-border)] pb-3"><p>{r.student.fullName} — {r.course.code}: {r.course.name}</p><p>{r.student.email} · {r.student.phone} · {registrationLabel(r.status)}</p><p>{vietnamDateTime(r.createdAt)}</p><Button disabled={task.pending||!!unknown} onClick={()=>read(r.id)}>Xem hồ sơ {r.id}</Button></article>)}<div className="flex gap-3"><Button disabled={task.pending||!!unknown||list.page<=1} onClick={()=>search(list.page-1)}>Trang trước</Button><Button disabled={task.pending||!!unknown||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1)}>Trang sau</Button></div></div>}
    {detail&&<article className="ec-work-card my-4 border-t border-[var(--ec-border)] pt-4"><h2 className="text-lg font-semibold">Hồ sơ {detail.id} — {detail.student.fullName}</h2><p>{detail.student.email} · {detail.student.phone}</p><p>{detail.course.code}: {detail.course.name} — {registrationLabel(detail.status)} — Band {detail.band?.code??'Chưa có'}</p>
      {detail.invoice&&<p>Hóa đơn {detail.invoice.number}: {money(detail.invoice.amount)} — {invoiceLabel(detail.invoice.status)}</p>}
      {detail.rejection&&<p>Lý do từ chối: {detail.rejection.reason}</p>}{detail.cancellation&&<p>Lý do hủy: {detail.cancellation.reason??'Học viên hủy hồ sơ'}</p>}
      {detail.entry.attempts.map(a=><p key={a.id}>Lượt thi {a.attemptNumber}: {scoreLabel(a.score)}{a.latestBooking?' · '+bookingLabel(a.latestBooking.status):''}</p>)}
      <div className="my-3 flex flex-wrap gap-3"><Button disabled={task.pending||!!unknown} onClick={()=>read(detail.id)}>Đọc lại hồ sơ</Button>{(['APPROVE','REJECT','CANCEL'] as const).map(a=><Button key={a} disabled={task.pending||!!unknown||!detail.allowedActions.includes(a)} onClick={()=>prepare(a)}>{labels[a]}</Button>)}</div>
      {Object.entries(detail.disabledReasons).map(([key,value])=><p key={key} className="text-sm">{labels[key as AdmissionAction]}: {value}</p>)}
      {action==='APPROVE'&&preview&&'course'in preview&&<div><p>Học phí hiện tại: {money(preview.course.tuition)}. Duyệt sẽ phát hành một hóa đơn chưa thanh toán; không xếp lớp.</p>{!preview.eligible&&<Feedback kind="error" message={preview.blockedReasons.join(' ')}/>}<Button disabled={task.pending||!preview.eligible||!!unknown} onClick={propose}>Chuẩn bị duyệt</Button></div>}
      {action==='CANCEL'&&preview&&'effects'in preview&&<div><p>Lượt đặt tương lai bị hủy: {preview.effects.futureBookings.map(b=>b.id).join(', ')||'Không có'}. Hóa đơn: {preview.effects.invoice?.number??'Chưa có'}.</p>{!preview.eligible&&<Feedback kind="error" message={preview.blockedReasons.join(' ')}/>}</div>}
      {(action==='REJECT'||action==='CANCEL'&&preview?.eligible)&&<AdmissionReasonForm value={reason} disabled={task.pending||!!unknown} actionLabel="Chuẩn bị xác nhận" onChange={setReason} onSubmit={propose} onCancel={resetDraft}/>}</article>}
    <Modal open={intent!==null} title={intent?labels[intent.action]:'Xác nhận'} onRequestClose={()=>setIntent(null)}><p>Xác nhận {intent?labels[intent.action].toLowerCase():''} #{intent?.id}. Giá và điều kiện sẽ được kiểm tra lại khi lưu.</p><p>{intent?.summary}</p>{intent&&intent.action!=='APPROVE'&&<p className="whitespace-pre-wrap">Lý do: {(intent.body as {reason:string}).reason}</p>}<Button disabled={task.pending||!!unknown} onClick={submit}>Xác nhận thao tác</Button></Modal>
  </section>;
}
