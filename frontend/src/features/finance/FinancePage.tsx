import { AccountActions } from '../../app/AccountActions';
import { useCallback,useEffect,useState } from 'react';
import type { SessionController } from '../identity/sessionController';
import type { Page } from '../../lib/m1Wire';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { PageHeader } from '../../components/ui/PageHeader';
import { Modal } from '../../components/ui/Modal';
import { invoiceLabel,vietnamDateTime } from '../../lib/enrollmentDisplay';
import { PaymentFields } from './components/PaymentFields';
import { financeSearch,financeDetail,collectionPreview,financeCommand,financeLookup } from './financeApi';
import type { FinanceInvoice,CollectionPreview,FinanceAction,PaymentMethod } from './financeApi';
const initialFilter={invoiceNumber:'',name:'',email:'',phone:'',courseId:'',status:'',issueFromDate:'',issueToDate:'',sort:'ISSUED_DESC',overdueOnly:false};
const money=(v:string)=>v.replace(/\B(?=(\d{3})+(?!\d))/g,'.')+' ₫';
const labels={COLLECT:'Xác nhận thanh toán',CORRECT_PAYMENT_METADATA:'Sửa thông tin thanh toán'};
type Intent={action:FinanceAction;target:number;invoiceId:number;number:string;amount:string;method:PaymentMethod;reference:string|null;reason:string;body:unknown};
export function FinancePage({controller,onPassword,initialId,initialPaymentId}:{controller:SessionController;onPassword:()=>void;initialId?:number;initialPaymentId?:number}) {
  const client=controller.client;
  const [filters,setFilters]=useState(initialFilter),[applied,setApplied]=useState<unknown>(null),[list,setList]=useState<Page<FinanceInvoice>|null>(null),[detail,setDetail]=useState<FinanceInvoice|null>(null);
  const [action,setAction]=useState<FinanceAction|null>(null),[preview,setPreview]=useState<CollectionPreview|null>(null),[method,setMethod]=useState<PaymentMethod>('CASH'),[reference,setReference]=useState(''),[reason,setReason]=useState('');
  const [intent,setIntent]=useState<Intent|null>(null),[unknown,setUnknown]=useState<{action:FinanceAction;invoiceId:number;requestId:string}|null>(null);
  const [invoiceLocator,setInvoiceLocator]=useState('');
  const clear=useCallback(()=>{setFilters(initialFilter);setApplied(null);setList(null);setDetail(null);setAction(null);setPreview(null);setMethod('CASH');setReference('');setReason('');setIntent(null);setUnknown(null);setInvoiceLocator('');},[]);
  const task=useOwnedTask(client,clear),run=task.run;
  useEffect(()=>{const timer=window.setTimeout(()=>{if(initialId)void run(s=>financeDetail(client,initialId,s),setDetail,undefined,()=>setDetail(null));else if(!initialPaymentId)void run(s=>financeSearch(client,{sort:'ISSUED_DESC',overdueOnly:false,page:1},s),v=>{setList(v);setApplied({sort:'ISSUED_DESC',overdueOnly:false});},undefined,()=>setList(null));},0);return()=>window.clearTimeout(timer);},[client,initialId,initialPaymentId,run]);
  const resetDraft=()=>{setAction(null);setPreview(null);setMethod('CASH');setReference('');setReason('');setIntent(null);};
  const read=(id:number)=>{resetDraft();setDetail(null);void task.run(s=>financeDetail(client,id,s),v=>{if(initialPaymentId&&v.payment?.id!==initialPaymentId){task.setNotice('Thanh toán không thuộc hóa đơn này. Chọn đúng hóa đơn để đối chiếu.');return;}setDetail(v);},undefined,()=>setDetail(null));};
  function search(page=1,paging=false) {
    if(filters.courseId&&(!/^[1-9]\d{0,9}$/.test(filters.courseId)||Number(filters.courseId)>2147483647)){task.setNotice('Mã định danh khóa phải là số nguyên dương hợp lệ.');return;}
    const body=paging?applied:{...Object.fromEntries(Object.entries(filters).filter(([k,v])=>v!==''&&k!=='courseId')),...filters.courseId?{courseId:Number(filters.courseId)}:{}};
    if(!body)return;resetDraft();setDetail(null);
    void task.run(s=>financeSearch(client,{...body as object,page},s),v=>{setList(v);setApplied(body);},undefined,()=>setList(null));
  }
  function prepare(next:FinanceAction) {
    if(!detail||unknown)return;resetDraft();setAction(next);
    if(next==='COLLECT')void task.run(s=>collectionPreview(client,detail.id,s),p=>{if(p.manifest?.expectedSet.registrationId!==detail.registrationId&&p.eligible){setAction(null);task.setNotice('Ngữ cảnh hóa đơn không khớp. Đọc lại hóa đơn.');return;}setPreview(p);},undefined,()=>{setAction(null);setPreview(null);});
    else if(detail.payment){setMethod(detail.payment.method);setReference(detail.payment.transactionReference??'');}
  }
  function propose() {
    if(!detail||!action||unknown)return;
    const ref=reference.normalize('NFC').trim()||null,why=reason.normalize('NFC').trim();
    if(ref&&Array.from(ref).length>100||method==='TRANSFER'&&!ref){task.setNotice('Chuyển khoản cần mã giao dịch; mã tối đa 100 ký tự.');return;}
    if(action==='CORRECT_PAYMENT_METADATA'&&(!why||Array.from(why).length>1000)){task.setNotice('Lý do sửa bắt buộc, tối đa 1.000 ký tự.');return;}
    if(action==='COLLECT'&&(!preview?.eligible||!preview.manifest)||action==='CORRECT_PAYMENT_METADATA'&&!detail.payment)return;
    if(action==='CORRECT_PAYMENT_METADATA'&&detail.payment?.method===method&&detail.payment.transactionReference===ref){task.setNotice('Chưa có thay đổi phương thức hoặc mã giao dịch.');return;}
    const body=action==='COLLECT'?{amount:preview!.amount,method,transactionReference:ref,...structuredClone(preview!.manifest)}:{method,transactionReference:ref,reason:why,expectedVersions:{payment:detail.payment!.version,invoice:detail.versions.invoice}};
    setIntent({action,target:action==='COLLECT'?detail.id:detail.payment!.id,invoiceId:detail.id,number:detail.number,amount:action==='COLLECT'?preview!.amount:detail.amount,method,reference:ref,reason:why,body});
  }
  function submit() {
    if(!intent||unknown)return;const frozen=intent,key=crypto.randomUUID();resetDraft();setList(null);
    void task.run(s=>financeCommand(client,frozen.action,frozen.target,frozen.invoiceId,frozen.body,key,s),r=>setDetail(r.invoice),()=>{setDetail(null);setUnknown({action:frozen.action,invoiceId:frozen.invoiceId,requestId:key});},()=>setDetail(null));
  }
  if(client.auth.context?.role!=='Accountant'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền kế toán."/>;
  return <section className="ec-work-page"><PageHeader title="Hóa đơn và thanh toán"/><p className="mb-3">Đối chiếu tiền đã nhận bên ngoài ứng dụng. Ngày và thời gian theo giờ Việt Nam.</p>
    <div className="mb-4 flex flex-wrap gap-3"><AccountActions><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button></AccountActions><AccountActions><Button variant="secondary" disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></AccountActions></div>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang xử lý…"/>}
    {initialPaymentId&&<form className="ec-work-filter my-4 flex flex-wrap items-end gap-3" onSubmit={e=>{e.preventDefault();if(task.pending||unknown)return;if(!/^[1-9]\d{0,9}$/.test(invoiceLocator)||Number(invoiceLocator)>2147483647){task.setNotice('Mã hóa đơn phải là số nguyên dương hợp lệ.');return;}read(Number(invoiceLocator));}}><p>Thanh toán {initialPaymentId}: nhập mã hóa đơn liên quan hoặc tìm hóa đơn bên dưới để đọc và đối chiếu trước khi sửa.</p><TextField label="Mã hóa đơn liên quan" value={invoiceLocator} disabled={task.pending||!!unknown} onChange={e=>setInvoiceLocator(e.target.value)}/><Button type="submit" disabled={task.pending||!!unknown}>Đối chiếu hóa đơn liên quan</Button></form>}
    {unknown&&<div className="my-4"><Feedback kind="error" message="Chưa xác định kết quả. Không gửi lại lệnh; tra cứu thủ công và đọc hóa đơn mới để đối chiếu."/><div className="ec-actions"><Button disabled={task.pending} onClick={()=>void task.run(s=>financeLookup(client,unknown.action,unknown.requestId,unknown.invoiceId,s),()=>{const id=unknown.invoiceId;setUnknown(null);read(id);})}>Tra cứu kết quả thủ công</Button><Button disabled={task.pending} onClick={()=>read(unknown.invoiceId)}>Đọc hóa đơn để đối chiếu</Button></div></div>}
    <form onSubmit={e=>{e.preventDefault();if(!task.pending&&!unknown)search();}} className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      {(['invoiceNumber','name','email','phone','courseId','issueFromDate','issueToDate'] as const).map((key,i)=><TextField key={key} label={['Số hóa đơn','Tên học viên','Email học viên','Số điện thoại','Mã định danh khóa','Phát hành từ ngày','Phát hành đến ngày'][i]} type={key.includes('Date')?'date':'text'} value={filters[key]} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,[key]:e.target.value})}/>)}
      <label>Trạng thái hóa đơn<select aria-label="Trạng thái hóa đơn" className="block min-h-11 w-full border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2" value={filters.status} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,status:e.target.value})}><option value="">Tất cả</option>{['UNPAID','PAID','CANCELLED'].map(s=><option key={s} value={s}>{invoiceLabel(s)}</option>)}</select></label>
      <label>Thứ tự<select aria-label="Thứ tự hóa đơn" className="block min-h-11 w-full border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2" value={filters.sort} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,sort:e.target.value})}><option value="ISSUED_DESC">Mới nhất trước</option><option value="ISSUED_ASC">Cũ nhất trước</option></select></label>
      <label className="flex min-h-11 items-center gap-2"><input type="checkbox" checked={filters.overdueOnly} disabled={task.pending||!!unknown} onChange={e=>setFilters({...filters,overdueOnly:e.target.checked})}/>Chỉ hóa đơn quá hạn</label><Button type="submit" disabled={task.pending||!!unknown}>Tìm hóa đơn</Button>
    </form>
    {list&&<div className="my-4">{list.items.length===0?<Feedback kind="empty" message="Không có hóa đơn phù hợp."/>:list.items.map(i=><article key={i.id} className="ec-work-card my-3 border-b border-[var(--ec-border)] pb-3"><p>{i.number} — {i.student.fullName} — {i.course.code}: {i.course.name}</p><p>{money(i.amount)} — {invoiceLabel(i.status)}{i.isOverdue?` — Quá hạn ${i.overdueDays} ngày`:''}</p><Button disabled={task.pending||!!unknown} onClick={()=>read(i.id)}>Xem hóa đơn {i.id}</Button></article>)}<div className="ec-actions"><Button disabled={task.pending||!!unknown||list.page<=1} onClick={()=>search(list.page-1,true)}>Trang trước</Button><Button disabled={task.pending||!!unknown||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1,true)}>Trang sau</Button></div></div>}
    {detail&&<article className="ec-work-card my-4 border-t border-[var(--ec-border)] pt-4"><h2 className="text-lg font-semibold">{detail.number} — {detail.student.fullName}</h2><p>{detail.student.email} · {detail.student.phone}</p><p>{detail.course.code}: {detail.course.name} — {money(detail.amount)} — {invoiceLabel(detail.status)}</p><p>Phát hành: {vietnamDateTime(detail.issuedAt)} · Hạn thu: {detail.dueDate.split('-').reverse().join('/')}</p>{detail.isOverdue&&<p>Quá hạn {detail.overdueDays} ngày. Cảnh báo không tự hủy hồ sơ.</p>}
      {detail.payment&&<div className="ec-stack"><p>Đã xác nhận: {vietnamDateTime(detail.payment.paidAt)} bởi {detail.payment.confirmedBy.fullName}</p><p>Phương thức: {detail.payment.method==='CASH'?'Tiền mặt':'Chuyển khoản'} · Mã giao dịch: {detail.payment.transactionReference??'Không có'}</p>{detail.payment.lastCorrection&&<p className="whitespace-pre-wrap">Lần sửa gần nhất: {detail.payment.lastCorrection.by.fullName} · {vietnamDateTime(detail.payment.lastCorrection.at)} · {detail.payment.lastCorrection.reason}</p>}</div>}
      <div className="my-3 flex flex-wrap gap-3"><Button disabled={task.pending} onClick={()=>read(detail.id)}>Đọc lại hóa đơn</Button>{(['COLLECT','CORRECT_PAYMENT_METADATA'] as const).map(a=><Button key={a} disabled={task.pending||!!unknown||!detail.allowedActions.includes(a)} onClick={()=>prepare(a)}>{labels[a]}</Button>)}</div>
      {Object.entries(detail.disabledReasons).map(([key,value])=><p key={key} className="text-sm">{labels[key as FinanceAction]}: {value}</p>)}
      {preview&&!preview.eligible&&<Feedback kind="error" message={preview.blockedReasons.join(' ')}/>}
      {(action==='CORRECT_PAYMENT_METADATA'||action==='COLLECT'&&preview?.eligible)&&<div className="ec-stack"><p>{action==='COLLECT'?`Số tiền cần đối chiếu đủ: ${money(preview!.amount)}`:'Sửa chỉ phương thức và mã giao dịch; giữ nguyên số tiền, hóa đơn và ngày xác nhận.'}</p><PaymentFields method={method} reference={reference} reason={reason} correction={action==='CORRECT_PAYMENT_METADATA'} disabled={task.pending||!!unknown} onMethod={setMethod} onReference={setReference} onReason={setReason} onSubmit={propose} onCancel={resetDraft}/></div>}</article>}
    <Modal open={intent!==null} title={intent?labels[intent.action]:'Xác nhận'} onRequestClose={()=>setIntent(null)}><p>{intent?.number} — {intent?money(intent.amount):''}</p><p>{intent?.action==='COLLECT'?'Xác nhận đã đối chiếu và nhận đủ tiền bên ngoài ứng dụng.':'Giữ nguyên số tiền và thời điểm xác nhận ban đầu.'}</p><p>{intent?.method==='CASH'?'Tiền mặt':'Chuyển khoản'} · Mã giao dịch: {intent?.reference??'Không có'}</p>{intent?.action==='CORRECT_PAYMENT_METADATA'&&<p className="whitespace-pre-wrap">Lý do sửa: {intent.reason}</p>}<Button disabled={task.pending||!!unknown} onClick={submit}>Xác nhận thao tác</Button></Modal>
  </section>;
}
