import type { HttpClient, Endpoint } from '../../lib/http';
import { object, requireWire, strings, instant, version, text } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import { validId, validUuid } from '../administration/types';
import { decodeClass, decodeManifest, decodeSchedule, decodeScheduling, progress, scheduleCandidate } from './classApi';
import type { ClassRole, ClassSummary, Manifest, ScheduleCandidate, ScheduleDetail } from './classApi';
export type WeeklyRule = { fromDate: string; toDate: string; weekdays: number[]; startTime: string; endTime: string; room: string };
export type ScheduleAction = 'WEEKLY' | 'MOVE' | 'REPLACE' | 'HELD';
export const scheduleCodes = { WEEKLY: 'ACADEMIC.SCHEDULE.CREATE_WEEKLY', MOVE: 'ACADEMIC.SCHEDULE.MOVE', REPLACE: 'ACADEMIC.SCHEDULE.REPLACE', HELD:'ACADEMIC.SCHEDULE.CONFIRM_HELD' };
type Conflict = { kind: string; startAt: string; endAt: string; message: string };
export type WeeklyPreview = { class: ClassSummary; rule: WeeklyRule; candidates: ScheduleCandidate[]; candidateCount: number; eligible: boolean; blockedReasons: string[]; conflicts: { candidateIndex: number; candidate: ScheduleCandidate; conflicts: Conflict[] }[]; manifest: Manifest | null };
export type EditPreview = { before: ScheduleDetail; proposed: ScheduleCandidate; reason?: string; eligible: boolean; blockedReasons: string[]; conflicts: Conflict[]; manifest: Manifest | null };
function conflicts(value: unknown) { requireWire(Array.isArray(value)); for (const item of value) { const c = object(item, ['kind','startAt','endAt','message']); requireWire(['CLASS','ROOM','TEACHER','STUDENT'].includes(c.kind as string) && instant(c.startAt) && instant(c.endAt) && text(c.message)); } }
function date(value: unknown): value is string { return typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value) && Number.isFinite(Date.parse(value)) && new Date(value).toISOString().slice(0,10) === value; }
export function weeklyPlan(value: unknown): ScheduleCandidate[] {
  const r = object(value, ['fromDate','toDate','weekdays','startTime','endTime','room']);
  requireWire(date(r.fromDate) && date(r.toDate) && r.fromDate <= r.toDate && Array.isArray(r.weekdays) && r.weekdays.length > 0 && new Set(r.weekdays).size === r.weekdays.length && r.weekdays.every(x => Number.isInteger(x) && x >= 1 && x <= 7));
  scheduleCandidate({type:'LESSON',date:r.fromDate,startTime:r.startTime,endTime:r.endTime,room:r.room});
  const result: ScheduleCandidate[] = [], start = Date.parse(r.fromDate), end = Date.parse(r.toDate);
  requireWire((end - start) / 86400000 < 10000);
  for (let day = start; day <= end; day += 86400000) { const d = new Date(day); if (r.weekdays.includes((d.getUTCDay()+6)%7+1)) result.push({type:'LESSON',date:d.toISOString().slice(0,10),startTime:r.startTime as string,endTime:r.endTime as string,room:r.room as string}); }
  return result;
}
const sameCandidate = (a: ScheduleCandidate,b: ScheduleCandidate) => ['type','date','startTime','endTime','room'].every(k => a[k as keyof ScheduleCandidate] === b[k as keyof ScheduleCandidate]);
export function decodeWeekly(value: unknown, id: number): WeeklyPreview {
  const r = object(value, ['class','rule','candidates','progress','candidateCount','eligible','blockedReasons','conflicts','manifest']);
  requireWire(decodeClass(r.class).id === id && Array.isArray(r.candidates) && r.candidateCount === r.candidates.length);
  progress(r.progress); const plan = weeklyPlan(r.rule); requireWire(plan.length === r.candidates.length); r.candidates.forEach((c,i) => { scheduleCandidate(c); requireWire(sameCandidate(plan[i],c as ScheduleCandidate)); });
  requireWire(typeof r.eligible === 'boolean' && strings(r.blockedReasons) && r.eligible === (r.manifest !== null) && Array.isArray(r.conflicts));
  const seen = new Set<number>(); for(const item of r.conflicts) { const c = object(item,['candidateIndex','candidate','conflicts']); requireWire(Number.isInteger(c.candidateIndex) && (c.candidateIndex as number) >= 0 && (c.candidateIndex as number) < plan.length && !seen.has(c.candidateIndex as number) && sameCandidate(c.candidate as ScheduleCandidate,plan[c.candidateIndex as number])); seen.add(c.candidateIndex as number); conflicts(c.conflicts); }
  if(r.manifest) decodeManifest(r.manifest,'schedule');
  return r as unknown as WeeklyPreview;
}
export function decodeEdit(value: unknown, id: number, replacement: boolean): EditPreview {
  const r = object(value,['before','proposed','eligible','blockedReasons','conflicts','manifest',...replacement?['reason']:[]]);
  const before = decodeSchedule(r.before); requireWire(before.id === id); scheduleCandidate(r.proposed); requireWire((r.proposed as ScheduleCandidate).type === before.type && typeof r.eligible === 'boolean' && strings(r.blockedReasons) && r.eligible === (r.manifest !== null) && (!replacement || text(r.reason))); conflicts(r.conflicts);
  if(r.manifest) { const m = object(r.manifest,['expectedVersions','expectedSet']), rv = object(m.expectedVersions,['class','program','teacher','schedules','schedule']), set = object(m.expectedSet,['assignmentIds','scheduleBindings','resources','cohortTimetables','targetScheduleId','obligationKey','dependencies']); requireWire(version(rv.schedule) && rv.schedule === before.version && set.targetScheduleId === id && set.obligationKey === before.obligationKey); const deps = object(set.dependencies,['scheduleId','hasAttendance','hasLinkedScore']); requireWire(deps.scheduleId === id && deps.hasAttendance === false && deps.hasLinkedScore === false);
    decodeManifest({expectedVersions:Object.fromEntries(Object.entries(rv).filter(([k])=>k !== 'schedule')),expectedSet:Object.fromEntries(Object.entries(set).filter(([k])=>!['targetScheduleId','obligationKey','dependencies'].includes(k)))},'schedule');
    requireWire((rv.schedules as {id:number;version:string}[]).some(x=>x.id === id && x.version === rv.schedule));
  }
  return r as unknown as EditPreview;
}
export const weeklyPreview = (c:HttpClient,id:number,rule:WeeklyRule,signal:AbortSignal) => c.request({endpoint:'weeklyPreview',targetId:id,body:{rule},signal,slot:'weekly-preview',decode:v=>decodeWeekly(v,id)});
export const editPreview = (c:HttpClient,id:number,draft:unknown,replacement:boolean,signal:AbortSignal) => c.request({endpoint:replacement?'scheduleReplacePreview':'scheduleMovePreview',targetId:id,body:draft,signal,slot:'schedule-edit-preview',decode:v=>decodeEdit(v,id,replacement)});
export const scheduleCommand = (c:HttpClient,action:ScheduleAction,id:number,body:unknown,key:string,signal:AbortSignal) => c.request({endpoint:({WEEKLY:'weeklyCreate',MOVE:'scheduleMove',REPLACE:'scheduleReplace',HELD:'heldConfirm'} as Record<ScheduleAction,Endpoint>)[action],targetId:id,body,requestId:key,signal,slot:'schedule-command',decode:v=>{
  const r=object(v,action === 'WEEKLY'?['created','context','operationId']:(action === 'MOVE'||action === 'HELD')?['schedule','operationId']:['oldScheduleId','newScheduleId','classId','obligationKey','operationId']); requireWire(validUuid(r.operationId));
  if(action === 'WEEKLY'){const context=decodeScheduling(r.context,id);requireWire(Array.isArray(r.created) && r.created.length === (body as {candidates:unknown[]}).candidates.length);const seen=new Set<number>();for(const x of r.created){const ref=object(x,['scheduleId','obligationKey']);requireWire(validId(ref.scheduleId) && validUuid(ref.obligationKey) && !seen.has(ref.scheduleId));seen.add(ref.scheduleId);}return {classId:id,scheduleId:undefined,context};}
  if(action === 'MOVE'||action === 'HELD'){const row=decodeSchedule(r.schedule);requireWire(row.id === id);return {classId:row.class.id,scheduleId:id};}
  requireWire(r.oldScheduleId === id && validId(r.newScheduleId) && validId(r.classId) && validUuid(r.obligationKey));return {classId:r.classId,scheduleId:r.newScheduleId};
}});
export const scheduleLookup = (c:HttpClient,action:ScheduleAction,key:string,id:number,signal:AbortSignal) => c.request({endpoint:'classResult',query:{action:scheduleCodes[action]},requestId:key,signal,slot:'schedule-lookup',decode:v=>{const r=object(v,['outcome','operationId','resultMode','target']), t=object(r.target,action === 'WEEKLY'?['classId']:(action === 'MOVE'||action === 'HELD')?['classId','scheduleId']:['classId','oldScheduleId','newScheduleId']);requireWire(r.outcome === 'COMPLETED' && r.resultMode === 'SAFE_REFERENCE' && validUuid(r.operationId) && Object.values(t).every(validId) && t[action === 'WEEKLY'?'classId':(action === 'MOVE'||action === 'HELD')?'scheduleId':'oldScheduleId'] === id);return t as Record<string,number>;}});
export const scheduleDetail = (c:HttpClient,role:ClassRole,id:number,signal:AbortSignal) => c.request({endpoint:role === 'AcademicAffairs'?'academicSchedule':role === 'Teacher'?'teacherSchedule':'studentSchedule',targetId:id,signal,slot:'schedule-detail',decode:v=>{const row=decodeSchedule(v,true,role);requireWire(row.id === id);return row;}});
export const scheduleCalendar = (c:HttpClient,role:ClassRole,query:Record<string,string>,signal:AbortSignal) => c.request({endpoint:role === 'AcademicAffairs'?'academicSchedules':role === 'Teacher'?'teacherSchedules':'studentSchedules',query,signal,slot:'schedule-calendar',decode:v=>{const r=object(v,['items','page','pageSize','totalCount','dateWindow']), w=object(r.dateWindow,['fromDate','toDate']);requireWire([w.fromDate,w.toDate].every(x=>x === null || date(x)));return {...page(Object.fromEntries(['items','page','pageSize','totalCount'].map(k=>[k,r[k]])),x=>decodeSchedule(x,false,role)),dateWindow:w as {fromDate:string|null;toDate:string|null}};}});

export type HeldPreview={schedule:ScheduleDetail;eligible:boolean;blockedReasons:string[];effects:string[];manifest:Manifest|null};
export const heldPreview=(c:HttpClient,id:number,signal:AbortSignal)=>c.request({endpoint:'heldPreview',targetId:id,signal,slot:'held-preview',decode:v=>{const r=object(v,['schedule','eligible','blockedReasons','effects','manifest']);const row=decodeSchedule(r.schedule);requireWire(row.id===id&&typeof r.eligible==='boolean'&&strings(r.blockedReasons)&&strings(r.effects)&&r.eligible===(r.manifest!==null));if(r.manifest){const m=object(r.manifest,['expectedVersions','expectedSet']),rv=object(m.expectedVersions,['class','program','teacher','schedules','schedule']),set=object(m.expectedSet,['assignmentIds','scheduleBindings','targetScheduleId','obligationKey']);requireWire(version(rv.schedule)&&rv.schedule===row.version&&set.targetScheduleId===id&&set.obligationKey===row.obligationKey);decodeManifest({expectedVersions:Object.fromEntries(Object.entries(rv).filter(([k])=>k!=='schedule')),expectedSet:Object.fromEntries(Object.entries(set).filter(([k])=>!['targetScheduleId','obligationKey'].includes(k)))},'transition','START');requireWire((rv.schedules as {id:number;version:string}[]).some(x=>x.id===id&&x.version===rv.schedule));}return r as unknown as HeldPreview;}});
