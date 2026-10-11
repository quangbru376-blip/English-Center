import type { HttpClient } from '../../lib/http';
import { object,requireWire,text,instant,version,capabilities,strings } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import type { Completion } from '../../lib/m1Wire';
import { validId,validUuid } from '../administration/types';
import { decodeStudentInvoice } from '../invoices/invoiceApi';
import type { StudentInvoice } from '../invoices/invoiceApi';
export type PaymentMethod='CASH'|'TRANSFER';
export type FinanceAction='COLLECT'|'CORRECT_PAYMENT_METADATA';
type Actor={accountId:number;fullName:string};
export type Payment={id:number;invoiceId:number;amountPaid:string;method:PaymentMethod;transactionReference:string|null;confirmedBy:Actor;paidAt:string;lastCorrection:{by:Actor;at:string;reason:string}|null;version:string};
export type FinanceInvoice=StudentInvoice&{student:{studentId:number;fullName:string;email:string;phone:string};registrationStatus:string;payment:Payment|null;versions:{registration:string;invoice:string};allowedActions:FinanceAction[];disabledReasons:Partial<Record<FinanceAction,string>>};
export type CollectionManifest={expectedVersions:{registration:string;invoice:string};expectedSet:{registrationId:number;paymentId:null}};
export type CollectionPreview={invoiceId:number;eligible:boolean;blockedReasons:string[];asOfUtc:string;amount:string;manifest:CollectionManifest|null};
export const financeCodes={COLLECT:'FINANCE.INVOICE.COLLECT',CORRECT_PAYMENT_METADATA:'FINANCE.PAYMENT.UPDATE_METADATA'};
const invoiceKeys=['id','registrationId','course','number','amount','issuedAt','dueDate','status','isOverdue','overdueDays'];
const money=(v:unknown):v is string=>typeof v==='string'&&/^[1-9]\d{0,17}$/.test(v);
const sameActor=(a:Actor,b:Actor)=>a.accountId===b.accountId&&a.fullName===b.fullName;
function samePayment(a:Payment|null,b:Payment) {
  if(!a)return false;
  const x=a.lastCorrection,y=b.lastCorrection;
  return a.id===b.id&&a.invoiceId===b.invoiceId&&a.amountPaid===b.amountPaid&&a.method===b.method&&a.transactionReference===b.transactionReference&&sameActor(a.confirmedBy,b.confirmedBy)&&a.paidAt===b.paidAt&&a.version===b.version&&(x===null?y===null:y!==null&&sameActor(x.by,y.by)&&x.at===y.at&&x.reason===y.reason);
}
function actor(v:unknown) {const a=object(v,['accountId','fullName']);requireWire(validId(a.accountId)&&text(a.fullName));}
export function decodePayment(v:unknown):Payment {
  const p=object(v,['id','invoiceId','amountPaid','method','transactionReference','confirmedBy','paidAt','lastCorrection','version']);actor(p.confirmedBy);
  requireWire(validId(p.id)&&validId(p.invoiceId)&&money(p.amountPaid)&&['CASH','TRANSFER'].includes(p.method as string)&&instant(p.paidAt)&&version(p.version));
  requireWire(p.transactionReference===null||text(p.transactionReference)&&Array.from(p.transactionReference).length<=100&&p.transactionReference.trim()===p.transactionReference);
  requireWire(p.method!=='TRANSFER'||text(p.transactionReference));
  if(p.lastCorrection!==null){const c=object(p.lastCorrection,['by','at','reason']);actor(c.by);requireWire(instant(c.at)&&text(c.reason)&&Array.from(c.reason).length<=1000);}
  return p as unknown as Payment;
}
export function decodeFinanceInvoice(v:unknown):FinanceInvoice {
  const r=object(v,[...invoiceKeys,'student','registrationStatus','payment','versions','allowedActions','disabledReasons']);
  decodeStudentInvoice(Object.fromEntries(invoiceKeys.map(k=>[k,r[k]])));
  const s=object(r.student,['studentId','fullName','email','phone']),rv=object(r.versions,['registration','invoice']);
  requireWire(validId(s.studentId)&&text(s.fullName)&&text(s.email)&&typeof s.phone==='string'&&/^0[35789]\d{8}$/.test(s.phone)&&['PENDING','TESTING','BAND_ASSIGNED','CONFIRMED','ADMITTED','REJECTED','CANCELLED'].includes(r.registrationStatus as string)&&Object.values(rv).every(version));
  const p=r.payment===null?null:decodePayment(r.payment);requireWire((r.status==='PAID')===(p!==null));
  if(p)requireWire(p.invoiceId===r.id&&p.amountPaid===r.amount);
  capabilities(r,['COLLECT','CORRECT_PAYMENT_METADATA']);
  const allowed=r.allowedActions as string[];requireWire(!allowed.includes('COLLECT')||r.status==='UNPAID'&&r.registrationStatus==='ADMITTED'&&p===null);requireWire(!allowed.includes('CORRECT_PAYMENT_METADATA')||r.status==='PAID'&&r.registrationStatus==='ADMITTED'&&p!==null);
  return r as unknown as FinanceInvoice;
}
export function decodeCollection(v:unknown):CollectionPreview {
  const r=object(v,['invoiceId','eligible','blockedReasons','asOfUtc','amount','manifest']);requireWire(validId(r.invoiceId)&&typeof r.eligible==='boolean'&&strings(r.blockedReasons)&&instant(r.asOfUtc)&&money(r.amount)&&r.eligible===(r.manifest!==null)&&(r.eligible?r.blockedReasons.length===0:r.blockedReasons.length>0));
  if(r.manifest!==null){const m=object(r.manifest,['expectedVersions','expectedSet']),rv=object(m.expectedVersions,['registration','invoice']),s=object(m.expectedSet,['registrationId','paymentId']);requireWire(Object.values(rv).every(version)&&validId(s.registrationId)&&s.paymentId===null);}
  return r as unknown as CollectionPreview;
}
export const financeSearch=(c:HttpClient,body:unknown,signal:AbortSignal)=>c.request({endpoint:'financeSearch',body,signal,slot:'finance-search',decode:v=>page(v,decodeFinanceInvoice)});
export const financeDetail=(c:HttpClient,id:number,signal:AbortSignal)=>c.request({endpoint:'financeDetail',targetId:id,signal,slot:'finance-detail',decode:v=>{const r=decodeFinanceInvoice(v);requireWire(r.id===id);return r;}});
export const collectionPreview=(c:HttpClient,id:number,signal:AbortSignal)=>c.request({endpoint:'financePreview',targetId:id,signal,slot:'finance-preview',decode:v=>{const r=decodeCollection(v);requireWire(r.invoiceId===id);return r;}});
export const financeCommand=(c:HttpClient,action:FinanceAction,target:number,invoiceId:number,body:unknown,requestId:string,signal:AbortSignal)=>c.request({endpoint:action==='COLLECT'?'financeCollect':'financeCorrect',targetId:target,body,requestId,signal,slot:'finance-command',decode:v=>{const r=object(v,['invoice','payment','operationId']),invoice=decodeFinanceInvoice(r.invoice),payment=decodePayment(r.payment);requireWire(validUuid(r.operationId)&&invoice.id===invoiceId&&payment.invoiceId===invoice.id&&samePayment(invoice.payment,payment)&&(action==='COLLECT'?invoice.id===target:payment.id===target));return {invoice,payment,operationId:r.operationId as string};}});
export function decodeFinanceCompletion(v:unknown):Completion {const r=object(v,['outcome','operationId','resultMode','target']),t=object(r.target,['invoiceId','paymentId']);requireWire(r.outcome==='COMPLETED'&&validUuid(r.operationId)&&r.resultMode==='SAFE_REFERENCE'&&Object.values(t).every(validId));return r as unknown as Completion;}
export const financeLookup=(c:HttpClient,action:FinanceAction,requestId:string,invoiceId:number,signal:AbortSignal)=>c.request({endpoint:'financeResult',query:{action:financeCodes[action]},requestId,signal,slot:'finance-result',decode:v=>{const r=decodeFinanceCompletion(v);requireWire(r.target.invoiceId===invoiceId);return r;}});
