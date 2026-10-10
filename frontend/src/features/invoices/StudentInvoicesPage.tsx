import { useCallback, useEffect, useState } from 'react';
import type { SessionController } from '../identity/sessionController';
import type { Page } from '../../lib/m1Wire';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
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
  useEffect(()=>{if(initialId)void run(signal=>studentInvoice(client,initialId,signal),setDetail,undefined,()=>setDetail(null));},[client,initialId,run]);
  useEffect(()=>{if(printCopy&&client.auth.context?.role==='Student'&&client.auth.context.scope==='NORMAL'){try{window.print();}finally{setPrintCopy(null);}}},[printCopy,client]);
  const search=(page=1)=>{
    setDetail(null);setPrintCopy(null);
    if(filter.registrationId&&(!/^[1-9]\d{0,9}$/.test(filter.registrationId)||Number(filter.registrationId)>2147483647)){task.setNotice('Mã hồ sơ phải là số nguyên dương hợp lệ.');return;}
    void task.run(signal=>studentInvoices(client,{...Object.fromEntries(Object.entries(filter).filter(([,v])=>v)),page:String(page)},signal),setList,undefined,()=>setList(null));
  };
  const read=(id:number,print=false)=>{setPrintCopy(null);setDetail(null);void task.run(signal=>studentInvoice(client,id,signal),r=>{setDetail(r);if(print)setPrintCopy(r);},undefined,()=>{setDetail(null);setPrintCopy(null);});};
  if(client.auth.context?.role!=='Student'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền xem hóa đơn."/>;
  return <section><div className="invoice-screen"><PageHeader title="Hóa đơn của tôi"/><div className="mb-4 flex flex-wrap gap-3"><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button><Button variant="secondary" disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></div>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang đọc hóa đơn…"/>}
    <form onSubmit={e=>{e.preventDefault();if(!task.pending)search();}} className="flex flex-wrap gap-3"><TextField label="Mã hồ sơ của tôi" value={filter.registrationId} disabled={task.pending} onChange={e=>setFilter({...filter,registrationId:e.target.value})}/><label>Trạng thái hóa đơn<select aria-label="Trạng thái hóa đơn" className="block min-h-11 border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2 text-[var(--ec-text)]" value={filter.status} disabled={task.pending} onChange={e=>setFilter({...filter,status:e.target.value})}><option value="">Tất cả</option>{['UNPAID','PAID','CANCELLED'].map(s=><option key={s} value={s}>{invoiceLabel(s)}</option>)}</select></label><Button type="submit" disabled={task.pending}>Tìm hóa đơn</Button></form>
    {list&&<div className="my-4">{list.items.length===0?<Feedback kind="empty" message="Chưa có hóa đơn phù hợp."/>:list.items.map(i=><article key={i.id} className="my-3 border-b border-[var(--ec-border)] pb-3"><p>{i.number} — {i.course.name} — {money(i.amount)}</p><p>{invoiceLabel(i.status)} · Hạn thu {date(i.dueDate)}{i.isOverdue?' · Quá hạn '+i.overdueDays+' ngày':''}</p><Button disabled={task.pending} onClick={()=>read(i.id)}>Xem hóa đơn {i.id}</Button></article>)}<div className="flex gap-3"><Button disabled={task.pending||list.page<=1} onClick={()=>search(list.page-1)}>Trang trước</Button><Button disabled={task.pending||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1)}>Trang sau</Button></div></div>}
    {detail&&<article className="my-4 border-t border-[var(--ec-border)] pt-4"><h2 className="text-lg font-semibold">{detail.number}</h2><p>Học viên: {detail.currentStudentName}</p><p>Khóa: {detail.course.code} — {detail.course.name}</p><p>Học phí: {money(detail.amount)}</p><p>Phát hành: {vietnamDateTime(detail.issuedAt)} · Hạn thu: {date(detail.dueDate)}</p><p>{detail.status==='UNPAID'?'Chưa thanh toán — không phải xác nhận đã thu tiền':invoiceLabel(detail.status)}</p>{detail.isOverdue&&<p>Quá hạn {detail.overdueDays} ngày.</p>}<p>{detail.paymentGuidance.message}</p><Button disabled={task.pending} onClick={()=>read(detail.id,true)}>Đọc mới và in hóa đơn</Button></article>}
    </div>{printCopy&&<article className="invoice-print"><h1>Hóa đơn {printCopy.number}</h1><p>Học viên: {printCopy.currentStudentName}</p><p>Khóa: {printCopy.course.code} — {printCopy.course.name}</p><p>Số tiền: {money(printCopy.amount)}</p><p>Phát hành: {vietnamDateTime(printCopy.issuedAt)} — Hạn thu: {date(printCopy.dueDate)}</p><p>{printCopy.status==='UNPAID'?'Chưa thanh toán — không phải xác nhận đã thu tiền':invoiceLabel(printCopy.status)}</p>{printCopy.isOverdue&&<p>Quá hạn {printCopy.overdueDays} ngày.</p>}</article>}
    <style>{`.invoice-print{display:none}@media print{@page{size:A4;margin:15mm}body *{visibility:hidden}.invoice-print,.invoice-print *{visibility:visible}.invoice-print{display:block;position:absolute;left:0;top:0;width:100%;color:#000;background:#fff}.invoice-screen{display:none}}`}</style>
  </section>;
}
