import type { Endpoint,HttpClient } from '../../lib/http';
import { exactKeys,record } from '../../lib/apiError';
import { completion,page } from '../../lib/m1Wire';
import { validId,validUuid } from '../administration/types';
export type Course=Readonly<{id:number;code:string;name:string;basePrice:string;status:'ACTIVE'|'INACTIVE';version:string;allowedActions:readonly string[];disabledReasons:Readonly<Record<string,string>>}>;
export type Band=Readonly<{id:number;code:string;minScore:string;version:string}>;
export type Bands=Readonly<{items:readonly Band[];isThresholdSetFrozen:boolean}>;
const version=(v:unknown)=>typeof v==='string'&&/^[A-Za-z0-9+/]{10}[AEIMQUYcgkosw048]=$/.test(v);
export function decodeCourse(v:unknown):Course{
  if(!record(v)||!exactKeys(v,['id','code','name','basePrice','status','version','allowedActions','disabledReasons'])||!validId(v.id)||typeof v.code!=='string'||!/^[A-Z0-9_-]{1,30}$/.test(v.code)||typeof v.name!=='string'||!v.name||typeof v.basePrice!=='string'||!/^[1-9]\d{0,17}$/.test(v.basePrice)||!['ACTIVE','INACTIVE'].includes(v.status as string)||!version(v.version)||!Array.isArray(v.allowedActions)||v.allowedActions.some(x=>!['EDIT_DETAILS','DEACTIVATE','ACTIVATE'].includes(x))||new Set(v.allowedActions).size!==v.allowedActions.length||!record(v.disabledReasons)||Object.entries(v.disabledReasons).some(([key,reason])=>!['EDIT_DETAILS','DEACTIVATE','ACTIVATE'].includes(key)||(v.allowedActions as unknown[]).includes(key)||typeof reason!=='string'||!reason)||['EDIT_DETAILS','DEACTIVATE','ACTIVATE'].some(key=>!(v.allowedActions as unknown[]).includes(key)&&!Object.hasOwn(v.disabledReasons as object,key)))throw new Error('Invalid Course');return v as Course;
}
function result(v:unknown){if(!record(v)||!exactKeys(v,['course','operationId'])||!validUuid(v.operationId))throw new Error('Invalid Course result');return {course:decodeCourse(v.course),operationId:v.operationId};}
function decodeBands(v:unknown):Bands{
  if(!record(v)||!exactKeys(v,['items','isThresholdSetFrozen'])||!Array.isArray(v.items)||v.items.length!==5||typeof v.isThresholdSetFrozen!=='boolean')throw new Error('Invalid Band set');
  const scores=['0.00','25.00','45.00','65.00','85.00'];
  const items=v.items.map((b,i)=>{if(!record(b)||!exactKeys(b,['id','code','minScore','version'])||!validId(b.id)||b.code!=='Band'+(i+1)||b.minScore!==scores[i]||!version(b.version))throw new Error('Invalid Band');return b as Band;});
  if(new Set(items.map(b=>b.id)).size!==5)throw new Error('Invalid Band identity');return {items,isThresholdSetFrozen:v.isThresholdSetFrozen};
}
export const courseActions={EDIT_DETAILS:{endpoint:'courseUpdate',action:'ACADEMIC.COURSE.UPDATE_DETAILS',label:'Sửa tên/học phí'},DEACTIVATE:{endpoint:'courseDeactivate',action:'ACADEMIC.COURSE.DEACTIVATE',label:'Ngừng sử dụng'},ACTIVATE:{endpoint:'courseActivate',action:'ACADEMIC.COURSE.ACTIVATE',label:'Kích hoạt lại'}} as const;
export const searchCourses=(client:HttpClient,query:Record<string,string>,signal:AbortSignal)=>client.request({endpoint:'courseSearch',slot:'course-search',query,signal,decode:v=>page(v,decodeCourse)});
export const loadCourse=(client:HttpClient,id:number,signal:AbortSignal)=>client.request({endpoint:'courseDetail',slot:'course-detail',targetId:id,signal,decode:v=>{const c=decodeCourse(v);if(c.id!==id)throw new Error('Wrong Course');return c;}});
export const loadBands=(client:HttpClient,signal:AbortSignal)=>client.request({endpoint:'bands',slot:'bands',signal,decode:decodeBands});
export const courseCommand=(client:HttpClient,endpoint:Endpoint,body:unknown,requestId:string,signal:AbortSignal,targetId?:number)=>client.request({endpoint,slot:'course-command',body,requestId,signal,targetId,decode:v=>{const r=result(v);if(targetId!==undefined&&r.course.id!==targetId)throw new Error('Wrong Course');return r;}});
export const courseLookup=(client:HttpClient,action:string,requestId:string,signal:AbortSignal)=>client.request({endpoint:'courseResult',slot:'course-result',query:{action},requestId,signal,decode:v=>completion(v,'courseId')});
