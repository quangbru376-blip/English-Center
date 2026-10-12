import type {HttpClient} from '../../lib/http';
import {object,requireWire,instant,text} from '../../lib/academicWire';
import {page} from '../../lib/m1Wire';
import {validId} from '../administration/types';
import {decodeClass} from './classApi';
import type {ProgramRef} from './classApi';
export type OwnGradeClass={id:number;code:string;program:ProgramRef;teacher:{displayName:string};status:'COMPLETED';assignment:{classAssignmentId:number;registrationId:number;assignedAt:string}};
export type StudentGradeSummary={id:number;class:OwnGradeClass;finalizedAt:string;finalScore:string;passed:boolean};
export type StudentGradeComponent={componentId:number;name:string;kind:'GENERAL'|'LESSON'|'FINAL_EXAM';weight:string;score:string;schedule:{id:number;type:'LESSON'|'FINAL_EXAM';date:string;startTime:string;endTime:string;room:string}|null};
export type StudentGradeDetail=StudentGradeSummary&{components:StudentGradeComponent[];teacherComment:string|null};
export const gradeDecimal=(v:unknown):v is string=>typeof v==='string'&&/^(?:0|[1-9]\d?|100)\.\d{2}$/.test(v)&&Number(v)<=100;
export function validGradeDate(v:unknown):v is string{return typeof v==='string'&&/^\d{4}-\d{2}-\d{2}$/.test(v)&&Number.isFinite(Date.parse(v+'T00:00:00Z'))&&new Date(v+'T00:00:00Z').toISOString().slice(0,10)===v;}
const keys=['id','class','finalizedAt','finalScore','passed'];
function summary(r:Record<string,unknown>):StudentGradeSummary{requireWire(validId(r.id)&&instant(r.finalizedAt)&&gradeDecimal(r.finalScore)&&typeof r.passed==='boolean');requireWire(decodeClass(r.class,'Student').status==='COMPLETED');return r as unknown as StudentGradeSummary;}
export function decodeStudentGradeSummary(v:unknown){return summary(object(v,keys));}
export function decodeStudentGradeDetail(v:unknown,id:number):StudentGradeDetail{
  const r=object(v,[...keys,'components','teacherComment']);summary(r);requireWire(r.id===id&&Array.isArray(r.components)&&r.components.length>0&&(r.teacherComment===null||typeof r.teacherComment==='string'&&Array.from(r.teacherComment).length<=1000));let last=0;
  for(const v of r.components){const c=object(v,['componentId','name','kind','weight','score','schedule']);requireWire(validId(c.componentId)&&c.componentId>last&&text(c.name)&&Array.from(c.name).length<=100&&['GENERAL','LESSON','FINAL_EXAM'].includes(c.kind as string)&&gradeDecimal(c.weight)&&Number(c.weight)>0&&gradeDecimal(c.score));last=c.componentId;
    if(c.kind==='GENERAL')requireWire(c.schedule===null);else{const s=object(c.schedule,['id','type','date','startTime','endTime','room']);requireWire(validId(s.id)&&s.type===c.kind&&validGradeDate(s.date)&&typeof s.startTime==='string'&&typeof s.endTime==='string'&&/^([01]\d|2[0-3]):[0-5]\d$/.test(s.startTime)&&/^([01]\d|2[0-3]):[0-5]\d$/.test(s.endTime)&&s.startTime<s.endTime&&text(s.room));}
  }requireWire((r.components as StudentGradeComponent[]).reduce((total,c)=>total+Number(c.weight.replace('.','')),0)===10000);return r as unknown as StudentGradeDetail;
}
export type StudentGradeFilters={courseId:string;classId:string;fromDate:string;toDate:string;sort:'FINALIZED_ASC'|'FINALIZED_DESC'};
export function studentGradeQuery(filters:StudentGradeFilters,p=1):Record<string,string>{requireWire(validId(p)&&['FINALIZED_ASC','FINALIZED_DESC'].includes(filters.sort));const q:Record<string,string>={sort:filters.sort,page:String(p),pageSize:'20'};for(const k of ['courseId','classId'] as const)if(filters[k]){requireWire(/^[1-9]\d*$/.test(filters[k])&&validId(Number(filters[k])));q[k]=filters[k];}for(const k of ['fromDate','toDate'] as const)if(filters[k]){requireWire(validGradeDate(filters[k]));q[k]=filters[k];}requireWire(!filters.fromDate||!filters.toDate||filters.fromDate<=filters.toDate);return q;}
export const studentGradeList=(c:HttpClient,query:Record<string,string>,s?:AbortSignal)=>c.request({endpoint:'studentGrades',query,signal:s,slot:'student-grade-list',decode:v=>page(v,decodeStudentGradeSummary)});
export const studentGradeDetail=(c:HttpClient,id:number,s?:AbortSignal)=>c.request({endpoint:'studentGrade',targetId:id,signal:s,slot:'student-grade-detail',decode:v=>decodeStudentGradeDetail(v,id)});
export const publishedInstant=(v:string)=>new Intl.DateTimeFormat('vi-VN',{timeZone:'Asia/Ho_Chi_Minh',day:'2-digit',month:'2-digit',year:'numeric',hour:'2-digit',minute:'2-digit'}).format(new Date(v));
