import type { HttpClient } from '../../lib/http';
import { object, requireWire, text, version, instant, strings, references, course, capabilities } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import type { Completion } from '../../lib/m1Wire';
import { record } from '../../lib/apiError';
import { validId, validUuid } from '../administration/types';
import { decodeEntry, decodeBooking } from '../enrollment/enrollmentApi';
import type { Entry, Booking, CourseRef } from '../enrollment/enrollmentApi';

export type AdmissionInvoice = {id:number;number:string;amount:string;status:string};
export type AdmissionSummary = {id:number;student:{studentId:number;fullName:string;email:string;phone:string};course:CourseRef;status:string;createdAt:string;admissionConfirmedAt:string|null;admissionApprovedAt:string|null;band:{id:number;code:string}|null;invoice:AdmissionInvoice|null;class:{classId:number;assignmentId:number;code:string;status:string}|null;version:string};
export type AdmissionDetail = AdmissionSummary & {entry:Entry;rejection:{at:string;reason:string}|null;cancellation:{at:string;reason:string|null}|null;allowedActions:string[];disabledReasons:Record<string,string>};
export type AdmissionManifest = {expectedVersions:Record<string,unknown>;expectedSet:Record<string,unknown>};
export type Approval = {registrationId:number;eligible:boolean;blockedReasons:string[];asOfUtc:string;course:CourseRef&{tuition:string;version:string};band:{id:number;code:string}|null;manifest:AdmissionManifest|null};
export type Cancellation = {registrationId:number;eligible:boolean;blockedReasons:string[];asOfUtc:string;effects:{futureBookings:Booking[];invoice:AdmissionInvoice|null};manifest:AdmissionManifest|null};
export type AdmissionAction = 'APPROVE'|'REJECT'|'CANCEL';
export const admissionCodes = {APPROVE:'ADMISSIONS.REGISTRATION.APPROVE',REJECT:'ADMISSIONS.REGISTRATION.REJECT',CANCEL:'ADMISSIONS.REGISTRATION.CANCEL'};
const summaryKeys = ['id','student','course','status','createdAt','admissionConfirmedAt','admissionApprovedAt','band','invoice','class','version'];
const statuses = ['PENDING','TESTING','BAND_ASSIGNED','CONFIRMED','ADMITTED','REJECTED','CANCELLED'];
const money = (v:unknown):v is string => typeof v==='string'&&/^[1-9]\d{0,17}$/.test(v);
const nullableAt = (v:unknown) => v===null||instant(v);
function band(v:unknown) {const b=object(v,['id','code']);requireWire(validId(b.id)&&text(b.code));}
export function decodeAdmissionInvoice(v:unknown):AdmissionInvoice {const i=object(v,['id','number','amount','status']);requireWire(validId(i.id)&&typeof i.number==='string'&&/^HD-\d{4}-\d{8}$/.test(i.number)&&money(i.amount)&&['UNPAID','PAID','CANCELLED'].includes(i.status as string));return i as unknown as AdmissionInvoice;}
function checkSummary(r:Record<string,unknown>) {
  const contact=object(r.student,['studentId','fullName','email','phone']);course(r.course);
  requireWire(validId(r.id)&&validId(contact.studentId)&&text(contact.fullName)&&text(contact.email)&&typeof contact.phone==='string'&&/^0[35789]\d{8}$/.test(contact.phone)&&statuses.includes(r.status as string)&&instant(r.createdAt)&&nullableAt(r.admissionConfirmedAt)&&nullableAt(r.admissionApprovedAt)&&version(r.version));
  if(r.band!==null)band(r.band);if(r.invoice!==null)decodeAdmissionInvoice(r.invoice);
  if(r.class!==null){const c=object(r.class,['classId','assignmentId','code','status']);requireWire(validId(c.classId)&&validId(c.assignmentId)&&text(c.code)&&['PLANNED','OPEN','ONGOING','COMPLETED','CANCELLED'].includes(c.status as string));}
}
export function decodeAdmissionSummary(v:unknown):AdmissionSummary {const r=object(v,summaryKeys);checkSummary(r);return r as unknown as AdmissionSummary;}
export function decodeAdmissionDetail(v:unknown):AdmissionDetail {
  const r=object(v,[...summaryKeys,'entry','rejection','cancellation','allowedActions','disabledReasons']);checkSummary(r);
  const entry=decodeEntry(r.entry);for(const a of entry.attempts)for(const b of [a.currentBooking,a.latestBooking])if(b)requireWire(b.registrationId===r.id);
  for(const k of ['rejection','cancellation'])if(r[k]!==null){const reason=object(r[k],['at','reason']);requireWire(instant(reason.at)&&(text(reason.reason)||k==='cancellation'&&reason.reason===null));}
  capabilities(r,['APPROVE','REJECT','CANCEL']);return r as unknown as AdmissionDetail;
}
function preview(v:unknown,keys:string[]) {const r=object(v,keys);requireWire(validId(r.registrationId)&&typeof r.eligible==='boolean'&&strings(r.blockedReasons)&&instant(r.asOfUtc)&&r.eligible===(r.manifest!==null)&&(r.eligible?r.blockedReasons.length===0:r.blockedReasons.length>0));return r;}
export function decodeApproval(v:unknown):Approval {
  const r=preview(v,['registrationId','eligible','blockedReasons','asOfUtc','course','band','manifest']),c=object(r.course,['id','code','name','status','tuition','version']);
  course({id:c.id,code:c.code,name:c.name,status:c.status});requireWire(money(c.tuition)&&version(c.version));if(r.band!==null)band(r.band);
  if(r.manifest!==null){const m=object(r.manifest,['expectedVersions','expectedSet']),rv=object(m.expectedVersions,['registration','course','bandAssignment']),set=object(m.expectedSet,['courseId','bandAssignmentId','invoiceId']);requireWire(Object.values(rv).every(version)&&validId(set.courseId)&&set.courseId===c.id&&validId(set.bandAssignmentId)&&set.invoiceId===null&&r.band!==null);}
  return r as unknown as Approval;
}
export function decodeAdmissionCancellation(v:unknown):Cancellation {
  const r=preview(v,['registrationId','eligible','blockedReasons','asOfUtc','effects','manifest']),e=object(r.effects,['futureBookings','invoice']);
  requireWire(Array.isArray(e.futureBookings));const bookings=e.futureBookings.map(decodeBooking);requireWire(new Set(bookings.map(b=>b.id)).size===bookings.length&&bookings.every(b=>b.registrationId===r.registrationId&&b.status==='SCHEDULED'&&b.session.startAt>(r.asOfUtc as string)));
  const invoice=e.invoice===null?null:decodeAdmissionInvoice(e.invoice);
  if(r.manifest!==null){const m=object(r.manifest,['expectedVersions','expectedSet']),rv=object(m.expectedVersions,['registration','bookings','sessions','invoice']),set=object(m.expectedSet,['futureBookingIds','invoiceId']);requireWire(version(rv.registration));references(rv.bookings);references(rv.sessions);requireWire(Array.isArray(set.futureBookingIds)&&set.futureBookingIds.every(validId)&&new Set(set.futureBookingIds).size===set.futureBookingIds.length&&set.futureBookingIds.length===bookings.length&&bookings.every(b=>(set.futureBookingIds as number[]).includes(b.id)));
    const bvs=rv.bookings as {id:number;version:string}[],svs=rv.sessions as {id:number;version:string}[];requireWire(bvs.length===bookings.length&&bookings.every(b=>bvs.some(x=>x.id===b.id&&x.version===b.version))&&svs.length===new Set(bookings.map(b=>b.sessionId)).size&&bookings.every(b=>svs.some(x=>x.id===b.sessionId&&x.version===b.session.version)));
    if(invoice){const iv=object(rv.invoice,['id','version']);requireWire(iv.id===invoice.id&&version(iv.version)&&set.invoiceId===invoice.id);}else requireWire(rv.invoice===null&&set.invoiceId===null);
  }
  return r as unknown as Cancellation;
}
function bound<T extends {registrationId:number}>(v:T,id:number):T {requireWire(v.registrationId===id);return v;}
export const searchAdmissions=(client:HttpClient,body:unknown,signal:AbortSignal)=>client.request({endpoint:'admissionsSearch',body,signal,slot:'admissions-search',decode:v=>page(v,decodeAdmissionSummary)});
export const readAdmission=(client:HttpClient,id:number,signal:AbortSignal)=>client.request({endpoint:'admissionsDetail',targetId:id,signal,slot:'admissions-detail',decode:v=>{const r=decodeAdmissionDetail(v);requireWire(r.id===id);return r;}});
export const previewApproval=(client:HttpClient,id:number,signal:AbortSignal)=>client.request({endpoint:'admissionsApproval',targetId:id,signal,slot:'admissions-preview',decode:v=>bound(decodeApproval(v),id)});
export const previewAdmissionCancel=(client:HttpClient,id:number,signal:AbortSignal)=>client.request({endpoint:'admissionsCancellation',targetId:id,signal,slot:'admissions-preview',decode:v=>bound(decodeAdmissionCancellation(v),id)});
export const admissionCommand=(client:HttpClient,action:AdmissionAction,id:number,body:unknown,requestId:string,signal:AbortSignal)=>client.request({endpoint:action==='APPROVE'?'admissionsApprove':action==='REJECT'?'admissionsReject':'admissionsCancel',targetId:id,body,requestId,signal,slot:'admissions-command',decode:v=>{const r=object(v,['registration','operationId']);requireWire(validUuid(r.operationId));const registration=decodeAdmissionDetail(r.registration);requireWire(registration.id===id);return {registration,operationId:r.operationId as string};}});
export function decodeAdmissionCompletion(v:unknown,action:AdmissionAction):Completion {
  const r=object(v,['outcome','operationId','resultMode','target']);requireWire(record(r.target));const keys=action==='APPROVE'?['registrationId','invoiceId']:action==='CANCEL'&&Object.hasOwn(r.target,'invoiceId')?['registrationId','invoiceId']:['registrationId'];const target=object(r.target,keys);requireWire(r.outcome==='COMPLETED'&&validUuid(r.operationId)&&r.resultMode==='SAFE_REFERENCE'&&Object.values(target).every(validId));return r as unknown as Completion;
}
export const admissionLookup=(client:HttpClient,action:AdmissionAction,requestId:string,id:number,signal:AbortSignal)=>client.request({endpoint:'admissionsResult',query:{action:admissionCodes[action]},requestId,signal,slot:'admissions-result',decode:v=>{const r=decodeAdmissionCompletion(v,action);requireWire(r.target.registrationId===id);return r;}});
