import type {HttpClient} from '../../lib/http';
import {object,requireWire,instant} from '../../lib/academicWire';
import {page} from '../../lib/m1Wire';
import {validId,validUuid} from '../administration/types';
import {localDate} from '../reports/reportWire';
import {auditActions,auditEntities,auditFields,auditEntityStates,auditActionRules} from './auditRegistry';
export type AuditSummary={id:number;occurredAt:string;actorId:number|null;action:string;entityType:string;entityId:string|null;result:'SUCCESS'|'FAILURE';operationId:string};
export type AuditScalar=string|number|boolean|null;
export type AuditDetail=AuditSummary&{reason:string|null;changes:{field:string;oldValue:AuditScalar;newValue:AuditScalar}[];ipAddress:string|null};
export type AuditFilters={fromDate:string;toDate:string;actorId:string;action:string;entityType:string;entityId:string;result:string};
export const emptyAuditFilters:AuditFilters={fromDate:'',toDate:'',actorId:'',action:'',entityType:'',entityId:'',result:''};
const summaryKeys=['id','occurredAt','actorId','action','entityType','entityId','result','operationId'];
function summary(r:Record<string,unknown>):AuditSummary{requireWire(validId(r.id)&&instant(r.occurredAt)&&(r.actorId===null||validId(r.actorId))&&typeof r.action==='string'&&(auditActions as readonly string[]).includes(r.action)&&typeof r.entityType==='string'&&(auditEntities as readonly string[]).includes(r.entityType)&&actionEntity(r.action,r.entityType)&&(r.entityId===null||typeof r.entityId==='string'&&/^[1-9]\d{0,9}$/.test(r.entityId)&&validId(Number(r.entityId)))&&['SUCCESS','FAILURE'].includes(r.result as string)&&validUuid(r.operationId));return r as unknown as AuditSummary;}
export function decodeAuditSummary(v:unknown){return summary(object(v,summaryKeys));}
function viewerFields(r:AuditSummary){const action=r.action.replace(/\.EFFECT$/,'');if(action==='STUDENT.PROFILE.UPDATE')return '';if(action==='AUTH.PASSWORD_CHANGE')return 'mustChangePassword securityVersion revokedSessionCount';if(action==='SYSTEM.BAND.SEED')return 'bandCode minScore';if(['STUDENT.ACCOUNT.CREATE','SYSTEM.ADMIN.BOOTSTRAP'].includes(action))return 'roleId status mustChangePassword';if(['AUTH.LOGIN','AUTH.LOGOUT','AUTH.ACCESS_DENIED','AUTH.THROTTLED'].includes(action))return '';if(action==='FINANCE.PAYMENT.UPDATE_METADATA')return 'method transactionReference';if(action==='TEACHER.ENTRY_BOOKING.ABSENCE')return 'status absentBy absentAt absentOperationId';if(r.entityType==='Account')return action==='ADMIN.STAFF.CREATE'?'roleId status mustChangePassword tempIssuedAt tempExpiresAt':'status mustChangePassword tempIssuedAt tempExpiresAt securityVersion revokedSessionCount';return auditFields[r.entityType]??'';}
function actionEntity(action:string,entity:string){const special:Record<string,string>={'SYSTEM.BAND.SEED':'Band','STUDENT.PROFILE.UPDATE':'StudentProfile','SYSTEM.ADMIN.BOOTSTRAP':'Account','STUDENT.ACCOUNT.CREATE':'Account','AUTH.PASSWORD_CHANGE':'Account','AUTH.LOGIN':'Account','AUTH.LOGOUT':'Account','AUTH.ACCESS_DENIED':'Account','AUTH.THROTTLED':'Account'};if(special[action])return special[action]===entity;const effect=action.endsWith('.EFFECT'),rule=auditActionRules[effect?action.slice(0,-7):action];return rule!==undefined&&(effect?rule.entities.includes(entity):rule.root===entity);}
function plain(v:unknown,max:number,min=1){return typeof v==='string'&&Array.from(v).length>=min&&Array.from(v).length<=max;}
function fieldValue(entity:string,field:string,v:unknown):boolean{
  if(v===null)return true;
  if(field.endsWith('OperationId')||field==='obligationKey')return validUuid(v);
  if(field.endsWith('Id')||field.endsWith('By'))return validId(v);
  if(field==='securityVersion')return typeof v==='number'&&Number.isSafeInteger(v)&&v>=1;
  if(field==='revokedSessionCount')return typeof v==='number'&&Number.isSafeInteger(v)&&v>=0;
  if(['mustChangePassword','isUnfinished','isCurrent'].includes(field))return typeof v==='boolean';
  if(['basePrice','amount','amountPaid'].includes(field))return typeof v==='string'&&/^[1-9]\d{0,17}$/.test(v);
  if(['score','scoreValue','finalScore','minScore','weight'].includes(field))return typeof v==='string'&&/^(?:0|[1-9]\d?|100)\.\d{2}$/.test(v)&&Number(v)<=100&&(field!=='weight'||Number(v)>0);
  if(field.endsWith('At'))return instant(v);
  if(field==='status')return typeof v==='string'&&(auditEntityStates[entity]??[]).includes(v);
  if(field==='cancelledFromStatus')return v==='SCHEDULED';
  if(field==='method')return v==='CASH'||v==='TRANSFER';
  if(field==='kind')return ['GENERAL','LESSON','FINAL_EXAM'].includes(v as string);
  if(field==='sessionType')return v==='LESSON'||v==='FINAL_EXAM';
  if(field==='occurrenceState')return ['PLANNED','HELD','NOT_HELD'].includes(v as string);
  if(field==='capacity')return validId(v)&&v<=20;
  if(field==='maxSize')return validId(v)&&v<=15;
  if(field==='attemptNumber')return v===1||v===2;
  if(field==='expectedLessonCount')return validId(v);
  if(field==='sessionDate'||field==='dueDate')return localDate(v);
  if(field==='startTime'||field==='endTime')return typeof v==='string'&&/^([01]\d|2[0-3]):[0-5]\d$/.test(v);
  if(field==='invoiceNumber')return typeof v==='string'&&/^HD-\d{4}-\d{8}$/.test(v);
  if(field==='courseCode'||field==='room')return typeof v==='string'&&/^[A-Z0-9_-]{1,30}$/.test(v);
  if(field==='bandCode')return plain(v,30);
  if(field==='courseName'||field==='programName')return plain(v,300);
  if(field==='componentName'||field==='transactionReference')return plain(v,100);
  if(field==='teacherComment')return plain(v,1000,0);
  return false;
}
export function decodeAuditDetail(v:unknown,id:number):AuditDetail{const r=object(v,[...summaryKeys,'reason','changes','ipAddress']),s=summary(r);requireWire(r.id===id&&(r.reason===null||plain(r.reason,1000)&&typeof r.reason==='string'&&r.reason.trim()===r.reason&&r.reason.normalize('NFC')===r.reason)&&Array.isArray(r.changes)&&(r.ipAddress===null||typeof r.ipAddress==='string'&&r.ipAddress.length<=45&&/^[0-9a-fA-F:.]+$/.test(r.ipAddress)));const allowed=viewerFields(s).split(' '),seen=new Set<string>();for(const v of r.changes){const c=object(v,['field','oldValue','newValue']);requireWire(typeof c.field==='string'&&allowed.includes(c.field)&&!seen.has(c.field)&&fieldValue(s.entityType,c.field,c.oldValue)&&fieldValue(s.entityType,c.field,c.newValue));seen.add(c.field);}return r as unknown as AuditDetail;}
export function auditQuery(f:AuditFilters,p=1):Record<string,string>{requireWire(validId(p));const q:Record<string,string>={page:String(p),pageSize:'20'};for(const k of ['fromDate','toDate'] as const)if(f[k]){requireWire(localDate(f[k])&&(k!=='toDate'||f[k]!=='9999-12-31'));q[k]=f[k];}requireWire(!f.fromDate||!f.toDate||f.fromDate<=f.toDate);if(f.actorId){requireWire(/^[1-9]\d*$/.test(f.actorId)&&validId(Number(f.actorId)));q.actorId=f.actorId;}if(f.action){requireWire((auditActions as readonly string[]).includes(f.action));q.action=f.action;}if(f.entityType){requireWire((auditEntities as readonly string[]).includes(f.entityType));q.entityType=f.entityType;}if(f.entityId){requireWire(/^[1-9]\d{0,9}$/.test(f.entityId)&&validId(Number(f.entityId)));q.entityId=f.entityId;}if(f.result){requireWire(['SUCCESS','FAILURE'].includes(f.result));q.result=f.result;}return q;}
export const auditList=(c:HttpClient,q:Record<string,string>,s?:AbortSignal)=>c.request({endpoint:'auditEvents',query:q,signal:s,slot:'audit-read',decode:v=>page(v,decodeAuditSummary)});
export const auditDetail=(c:HttpClient,id:number,s?:AbortSignal)=>c.request({endpoint:'auditEvent',targetId:id,signal:s,slot:'audit-detail',decode:v=>decodeAuditDetail(v,id)});
export const auditOperation=(c:HttpClient,operationId:string,p=1,s?:AbortSignal)=>c.request({endpoint:'auditOperation',operationId,query:{page:String(p),pageSize:'20'},signal:s,slot:'audit-read',decode:v=>page(v,row=>{const r=decodeAuditSummary(row);requireWire(r.operationId===operationId);return r;})});
export const auditValue=(v:AuditScalar)=>v===null?'Không có':typeof v==='boolean'?(v?'Có':'Không'):String(v);
