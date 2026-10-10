import type { HttpClient } from '../../lib/http';
import { object, requireWire, text, instant, course } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import { validId } from '../administration/types';
import type { CourseRef } from '../enrollment/enrollmentApi';

export type StudentInvoice={id:number;registrationId:number;course:CourseRef;number:string;amount:string;issuedAt:string;dueDate:string;status:string;isOverdue:boolean;overdueDays:number};
export type StudentInvoiceDetail=StudentInvoice&{currentStudentName:string;paymentGuidance:{message:string}};
const keys=['id','registrationId','course','number','amount','issuedAt','dueDate','status','isOverdue','overdueDays'];
function check(r:Record<string,unknown>) {
  course(r.course);requireWire(validId(r.id)&&validId(r.registrationId)&&typeof r.number==='string'&&/^HD-\d{4}-\d{8}$/.test(r.number)&&typeof r.amount==='string'&&/^[1-9]\d{0,17}$/.test(r.amount)&&instant(r.issuedAt)&&typeof r.dueDate==='string'&&/^\d{4}-\d{2}-\d{2}$/.test(r.dueDate)&&Number.isFinite(Date.parse(r.dueDate+'T00:00:00Z'))&&new Date(r.dueDate+'T00:00:00Z').toISOString().slice(0,10)===r.dueDate&&['UNPAID','PAID','CANCELLED'].includes(r.status as string)&&typeof r.isOverdue==='boolean'&&Number.isInteger(r.overdueDays)&&(r.overdueDays as number)>=0&&r.isOverdue===((r.overdueDays as number)>0)&&(r.status==='UNPAID'||r.isOverdue===false));
}
export function decodeStudentInvoice(v:unknown):StudentInvoice {const r=object(v,keys);check(r);return r as unknown as StudentInvoice;}
export function decodeStudentInvoiceDetail(v:unknown):StudentInvoiceDetail {const r=object(v,[...keys,'currentStudentName','paymentGuidance']);check(r);const guidance=object(r.paymentGuidance,['message']);requireWire(text(r.currentStudentName)&&text(guidance.message));return r as unknown as StudentInvoiceDetail;}
export const studentInvoices=(client:HttpClient,query:Record<string,string>,signal:AbortSignal)=>client.request({endpoint:'studentInvoices',query,signal,slot:'student-invoices',decode:v=>page(v,decodeStudentInvoice)});
export const studentInvoice=(client:HttpClient,id:number,signal:AbortSignal)=>client.request({endpoint:'studentInvoice',targetId:id,signal,slot:'student-invoice',decode:v=>{const r=decodeStudentInvoiceDetail(v);requireWire(r.id===id);return r;}});
