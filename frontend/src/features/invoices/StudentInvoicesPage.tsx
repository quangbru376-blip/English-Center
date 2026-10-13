import { AccountActions } from '../../app/AccountActions';
import { useCallback, useEffect, useState } from 'react';
import type { SessionController } from '../identity/sessionController';
import type { Page } from '../../lib/m1Wire';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { Badge, Card, EmptyState } from '../../components/ui/Surfaces';
import { PageHeader } from '../../components/ui/PageHeader';
import { invoiceLabel, vietnamDateTime } from '../../lib/enrollmentDisplay';
import { studentInvoices, studentInvoice } from './invoiceApi';
import type { StudentInvoice, StudentInvoiceDetail } from './invoiceApi';

const money=(v:string)=>v.replace(/\B(?=(\d{3})+(?!\d))/g,'.')+' ₫';
const date=(v:string)=>v.split('-').reverse().join('/');
export function StudentInvoicesPage({controller,onPassword,initialId}:{controller:SessionController;onPassword:()=>void;initialId?:number}) {
  const client=controller.client;
  const [filter,setFilter]=useState({registrationId:'',status:''}),[list,setList]=useState<Page<StudentInvoice>|null>(null),[detail,setDetail]=useState<StudentInvoiceDetail|null>(null),[printCopy,setPrintCopy]=useState<StudentInvoiceDetail|null>(null);
  const clear=useCallback(()=>{setFilter({registrationId:'',status:''});setList(null);setDetail(null);setPrintCopy(null);},[]);
  const task=useOwnedTask(client,clear);
  const run=task.run;
  useEffect(()=>{const timer=window.setTimeout(()=>{if(initialId)void run(signal=>studentInvoice(client,initialId,signal),setDetail,undefined,()=>setDetail(null));else void run(signal=>studentInvoices(client,{page:'1'},signal),setList,undefined,()=>setList(null));},0);return()=>window.clearTimeout(timer);},[client,initialId,run]);
  useEffect(()=>{if(printCopy&&client.auth.context?.role==='Student'&&client.auth.context.scope==='NORMAL'){try{window.print();}finally{setPrintCopy(null);}}},[printCopy,client]);
  const search=(page=1)=>{
    setDetail(null);setPrintCopy(null);
    if(filter.registrationId&&(!/^[1-9]\d{0,9}$/.test(filter.registrationId)||Number(filter.registrationId)>2147483647)){task.setNotice('Mã hồ sơ phải là số nguyên dương hợp lệ.');return;}
    void task.run(signal=>studentInvoices(client,{...Object.fromEntries(Object.entries(filter).filter(([,v])=>v)),page:String(page)},signal),setList,undefined,()=>setList(null));
  };
  const read=(id:number,print=false)=>{setPrintCopy(null);setDetail(null);void task.run(signal=>studentInvoice(client,id,signal),r=>{setDetail(r);if(print)setPrintCopy(r);},undefined,()=>{setDetail(null);setPrintCopy(null);});};
  if(client.auth.context?.role!=='Student'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền xem hóa đơn."/>;
  return <section><div className="invoice-screen student-page"><div><PageHeader title="Hóa đơn của tôi"/><p className="text-muted">Theo dõi học phí và thông tin thanh toán của từng hồ sơ.</p></div><AccountActions><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button><Button variant="secondary" disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></AccountActions>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang đọc hóa đơn…"/>}
    <form onSubmit={e=>{e.preventDefault();if(!task.pending)search();}} className="student-filter"><TextField label="Mã hồ sơ của tôi" value={filter.registrationId} disabled={task.pending} onChange={e=>{setFilter({...filter,registrationId:e.target.value});setList(null);setDetail(null);setPrintCopy(null);}}/><label>Trạng thái hóa đơn<select aria-label="Trạng thái hóa đơn" className="ec-input" value={filter.status} disabled={task.pending} onChange={e=>{setFilter({...filter,status:e.target.value});setList(null);setDetail(null);setPrintCopy(null);}}><option value="">Tất cả</option>{['UNPAID','PAID','CANCELLED'].map(status=><option key={status} value={status}>{invoiceLabel(status)}</option>)}</select></label><Button type="submit" disabled={task.pending}>Tìm hóa đơn</Button></form>
    {list&&<section aria-label="Danh sách hóa đơn"><div className="student-list-heading"><h2>Học phí theo hồ sơ</h2><span className="text-muted text-sm">{list.totalCount} hóa đơn · Trang {list.page}</span></div><div className="student-lines">{list.items.length===0?<EmptyState title="Chưa có hóa đơn phù hợp">Hóa đơn được lập khi hồ sơ đăng ký được duyệt. Bạn có thể đổi bộ lọc để xem lại.</EmptyState>:list.items.map(invoice=><Card key={invoice.id} className="student-line"><div><span className="student-eyebrow">{invoice.number}</span><h3>{invoice.course.name}</h3><p className="text-muted text-sm">Hạn thu {date(invoice.dueDate)}{invoice.isOverdue?' · Quá hạn '+invoice.overdueDays+' ngày':''}</p></div><div><p className="student-invoice-amount">{money(invoice.amount)}</p><Badge tone={invoice.status==='PAID'?'success':invoice.status==='UNPAID'?'warning':'neutral'}>{invoiceLabel(invoice.status)}</Badge></div><Button variant="secondary" disabled={task.pending} onClick={()=>read(invoice.id)}>Xem hóa đơn {invoice.id}</Button></Card>)}</div><div className="student-pagination"><Button variant="secondary" disabled={task.pending||list.page<=1} onClick={()=>search(list.page-1)}>Trang trước</Button><span>Trang {list.page} / {Math.max(1,Math.ceil(list.totalCount/list.pageSize))}</span><Button variant="secondary" disabled={task.pending||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1)}>Trang sau</Button></div></section>}
    {detail&&<Card className="student-detail"><div className="student-card-heading"><div><span className="student-eyebrow">Chi tiết học phí</span><h2>{detail.number}</h2></div><span className="student-invoice-amount">{money(detail.amount)}</span></div><dl className="student-summary"><div><dt>Học viên</dt><dd>{detail.currentStudentName}</dd></div><div><dt>Khóa học</dt><dd>{detail.course.code} · {detail.course.name}</dd></div><div><dt>Phát hành</dt><dd>{vietnamDateTime(detail.issuedAt)}</dd></div><div><dt>Hạn thu</dt><dd>{date(detail.dueDate)}</dd></div></dl><div className="student-invoice-status"><p>{detail.status==='UNPAID'?'Chưa thanh toán — không phải xác nhận đã thu tiền':invoiceLabel(detail.status)}</p>{detail.isOverdue&&<p>Quá hạn {detail.overdueDays} ngày.</p>}</div><p>{detail.paymentGuidance.message}</p><p className="text-muted text-sm">Khi in, thông tin hóa đơn sẽ được đọc lại để giữ đúng trạng thái hiện tại.</p><div><Button disabled={task.pending} onClick={()=>read(detail.id,true)}>Đọc mới và in hóa đơn</Button></div></Card>}
    </div>{printCopy&&<article className="invoice-print"><h1>Hóa đơn {printCopy.number}</h1><p>Học viên: {printCopy.currentStudentName}</p><p>Khóa: {printCopy.course.code} — {printCopy.course.name}</p><p>Số tiền: {money(printCopy.amount)}</p><p>Phát hành: {vietnamDateTime(printCopy.issuedAt)} — Hạn thu: {date(printCopy.dueDate)}</p><p>{printCopy.status==='UNPAID'?'Chưa thanh toán — không phải xác nhận đã thu tiền':invoiceLabel(printCopy.status)}</p>{printCopy.isOverdue&&<p>Quá hạn {printCopy.overdueDays} ngày.</p>}</article>}
    <style>{`.invoice-print{display:none}@media print{@page{size:A4;margin:15mm}body *{visibility:hidden}.invoice-print,.invoice-print *{visibility:visible}.invoice-print{display:block;position:absolute;left:0;top:0;width:100%;color:#000;background:#fff}.invoice-screen{display:none}}`}</style>
  </section>;
}
