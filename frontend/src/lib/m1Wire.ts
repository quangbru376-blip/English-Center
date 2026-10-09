import { exactKeys, record } from './apiError';
import { validId, validUuid } from '../features/administration/types';
export type Completion=Readonly<{outcome:'COMPLETED';operationId:string;resultMode:'SAFE_REFERENCE'|'SECRET_RESULT_UNAVAILABLE';target:Readonly<Record<string,number>>}>;
export function completion(value:unknown,key:'accountId'|'courseId'|'programId'|'sessionId',secret=false):Completion{
  if(!record(value)||!exactKeys(value,['outcome','operationId','resultMode','target'])||value.outcome!=='COMPLETED'||!validUuid(value.operationId)||
    value.resultMode!==(secret?'SECRET_RESULT_UNAVAILABLE':'SAFE_REFERENCE')||!record(value.target)||!exactKeys(value.target,[key])||!validId(value.target[key]))throw new Error('Invalid completion');
  return value as Completion;
}
export type Page<T>=Readonly<{items:readonly T[];page:number;pageSize:number;totalCount:number}>;
export function page<T>(value:unknown,decode:(row:unknown)=>T):Page<T>{
  if(!record(value)||!exactKeys(value,['items','page','pageSize','totalCount'])||!Array.isArray(value.items)||!validId(value.page)||
    !Number.isInteger(value.pageSize)||(value.pageSize as number)<1||(value.pageSize as number)>100||!Number.isSafeInteger(value.totalCount)||(value.totalCount as number)<0||value.items.length>(value.pageSize as number))throw new Error('Invalid page');
  return {items:value.items.map(decode),page:value.page,pageSize:value.pageSize as number,totalCount:value.totalCount as number};
}
