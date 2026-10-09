import type { Endpoint,HttpClient } from '../../lib/http';
import { exactKeys,record } from '../../lib/apiError';
import { completion,page } from '../../lib/m1Wire';
import type { Completion } from '../../lib/m1Wire';
import { decodeAdminAccount,decodeLockResult,validId,validUuid } from './types';
export type StaffFields={fullName:string;email:string;phone?:string;role:string};
export type Credential=Readonly<{accountId:number;email:string;role:string;temporaryPassword:string;temporaryPasswordExpiresAt:string;operationId:string;accountVersion:string}>;
export const staffActions={RESET_PASSWORD:{endpoint:'adminReset',path:'ADMIN.ACCOUNT.RESET_PASSWORD',label:'Đặt lại mật khẩu'},
  LOCK:{endpoint:'adminLock',path:'ADMIN.ACCOUNT.LOCK',label:'Khóa'},UNLOCK:{endpoint:'adminUnlock',path:'ADMIN.ACCOUNT.UNLOCK',label:'Mở khóa'},
  DISABLE:{endpoint:'adminDisable',path:'ADMIN.ACCOUNT.DISABLE',label:'Ngừng sử dụng'},RESTORE:{endpoint:'adminRestore',path:'ADMIN.ACCOUNT.RESTORE',label:'Kích hoạt lại'}} as const;
export type StaffAction=keyof typeof staffActions;
const instant=(v:unknown):v is string=>typeof v==='string'&&/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/.test(v)&&Number.isFinite(Date.parse(v))&&new Date(v).toISOString()===v;
export function credential(value:unknown):Credential|Completion{
  if(record(value)&&value.outcome==='COMPLETED')return completion(value,'accountId',true);
  if(!record(value)||!exactKeys(value,['accountId','email','role','temporaryPassword','temporaryPasswordExpiresAt','operationId','accountVersion'])||!validId(value.accountId)||typeof value.email!=='string'||
    !['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director','Admin'].includes(value.role as string)||typeof value.temporaryPassword!=='string'||!/^[A-Za-z0-9_-]{32}$/.test(value.temporaryPassword)||
    !instant(value.temporaryPasswordExpiresAt)||!validUuid(value.operationId)||typeof value.accountVersion!=='string'||!/^[A-Za-z0-9+/]{10}[AEIMQUYcgkosw048]=$/.test(value.accountVersion))throw new Error('Invalid credential');
  return value as Credential;
}
export const searchAccounts=(client:HttpClient,body:Record<string,unknown>,signal:AbortSignal)=>client.request({endpoint:'adminSearch',slot:'staff-search',body,signal,decode:v=>page(v,decodeAdminAccount)});
export const createStaff=(client:HttpClient,body:StaffFields,requestId:string,signal:AbortSignal)=>client.request({endpoint:'staffCreate',slot:'staff-create',body,requestId,signal,decode:credential});
export const staffCommand=(client:HttpClient,action:StaffAction,id:number,version:string,requestId:string,signal:AbortSignal)=>client.request({endpoint:staffActions[action].endpoint as Endpoint,slot:'staff-command',body:{expectedVersions:{account:version}},targetId:id,requestId,signal,decode:v=>{
  if(action!=='RESET_PASSWORD')return decodeLockResult(v,id);const result=credential(v);
  if(('accountId' in result?result.accountId:result.target.accountId)!==id)throw new Error('Wrong target');return result;
}});
export const staffLookup=(client:HttpClient,action:string,requestId:string,signal:AbortSignal)=>client.request({endpoint:'adminResult',slot:'staff-result',requestId,query:{action},signal,decode:v=>completion(v,'accountId',action==='ADMIN.STAFF.CREATE'||action==='ADMIN.ACCOUNT.RESET_PASSWORD')});
