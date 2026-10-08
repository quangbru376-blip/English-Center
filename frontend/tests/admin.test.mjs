import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url),load=x=>require(resolve(process.env.EC_TEST_BUILD,x));
const {AuthState}=load('lib/auth.js'),{HttpClient}=load('lib/http.js');
const {decodeAdminAccount,decodeLockResult,decodeLockCompletion}=load('features/administration/types.js');
const {createLockIntent}=load('features/administration/lockIntent.js');
const {loadAdminAccount,lockAdminAccount,lookupAdminLock}=load('features/administration/adminApi.js');
const key='12345678-1234-1234-1234-123456789abc';
const account=()=>({id:2,fullName:'Tên kiểm chứng',email:'sample@example.test',phone:null,role:'Student',status:'ACTIVE',createdAt:'2026-10-07T01:00:00.000Z',mustChangePassword:false,temporaryPasswordExpiresAt:null,version:'AAAAAAAAAAE=',allowedActions:['RESET_PASSWORD','LOCK','DISABLE'],disabledReasons:{UNLOCK:'Không phù hợp.',RESTORE:'Không phù hợp.'}});
const auth=(role='Admin',scope='NORMAL')=>{const a=new AuthState();a.startSession({accountId:1,fullName:'Người kiểm chứng',email:'actor@example.test',role,scope,accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z'});a.acceptCsrf({requestToken:'synthetic'});return a;};
const json=(x,status=200)=>new Response(JSON.stringify(x),{status});
test('exact Admin account12fields immutable; no extra secrets, malformed versions/capabilities/instants',()=>{
 const parsed=decodeAdminAccount(account());assert.ok(Object.isFrozen(parsed));assert.ok(Object.isFrozen(parsed.allowedActions));
 for(const extra of [{password:'bad'},{version:'bad'},{id:0},{createdAt:'2026-02-30T01:00:00.000Z'},{allowedActions:['LOCK','LOCK']},{disabledReasons:{LOCK:'bad'}}])assert.throws(()=>decodeAdminAccount({...account(),...extra}));
 assert.throws(()=>decodeLockResult({account:account(),operationId:key},3));
 const completion={outcome:'COMPLETED',operationId:key,resultMode:'SAFE_REFERENCE',target:{accountId:2}};assert.equal(decodeLockCompletion(completion,2).target.accountId,2);
 for(const extra of [{account:account()},{resultMode:'SECRET_RESULT_UNAVAILABLE'},{target:{accountId:3}},{target:{accountId:2,email:'bad'}}])assert.throws(()=>decodeLockCompletion({...completion,...extra},2));
});
test('only normal Admin may dispatch; malformed routes/key blocked without fetch',async()=>{
 let sent=0;const fetcher=async()=>{sent++;return json(account());};
 for(const role of ['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director'])assert.equal((await loadAdminAccount(new HttpClient(auth(role),fetcher),2)).kind,'blocked');
 assert.equal((await loadAdminAccount(new HttpClient(auth('Admin','PASSWORD_CHANGE'),fetcher),2)).kind,'blocked');
 const client=new HttpClient(auth(),fetcher);for(const id of [0,-1,1.5,2147483648,'2/lock'])assert.equal((await loadAdminAccount(client,id)).kind,'blocked');
 assert.equal((await lookupAdminLock(client,key+'?action=OTHER',2)).kind,'blocked');assert.equal(sent,0);
});
test('frozen deliberate lock intent exact RV/body/header, no implicit UUID mint or retries',async()=>{
 let mints=0;const intent=createLockIntent(decodeAdminAccount(account()),()=>{mints++;return key;});assert.ok(Object.isFrozen(intent));assert.equal(mints,1);
 const calls=[];const client=new HttpClient(auth(),async(path,init)=>{calls.push({path,init});return json({account:{...account(),status:'LOCKED',allowedActions:['RESET_PASSWORD','UNLOCK','DISABLE'],disabledReasons:{LOCK:'Khóa rồi.',RESTORE:'Không phù hợp.'}},operationId:key});});
 assert.equal((await lockAdminAccount(client,intent)).kind,'success');assert.equal(calls.length,1);assert.equal(calls[0].path,'/api/v1/admin/accounts/2/lock');assert.equal(calls[0].init.headers['Idempotency-Key'],key);
 assert.deepEqual(JSON.parse(calls[0].init.body),{expectedVersions:{account:'AAAAAAAAAAE='}});assert.throws(()=>createLockIntent({...account(),status:'LOCKED'},()=>key));
});
test('unknown lock no auto repeat; lookup404 is not rollback and lookup200 contains safe reference only',async()=>{
 let writes=0;const client=new HttpClient(auth(),async()=>{writes++;throw new Error('lost');});assert.equal((await lockAdminAccount(client,createLockIntent(account(),()=>key))).kind,'unknown');assert.equal(writes,1);assert.ok(client.auth.context);
 const calls=[];const lookup=new HttpClient(auth(),async(path,init)=>{calls.push({path,init});return json({code:'RESULT_NOT_CONFIRMED',message:'Chưa xác nhận',fieldErrors:{},traceId:'test'},404);});assert.equal((await lookupAdminLock(lookup,key,2)).status,404);assert.equal(calls[0].path,'/api/v1/admin/command-results/'+key+'?action=ADMIN.ACCOUNT.LOCK');assert.equal(calls[0].init.method,'GET');assert.equal(calls[0].init.body,undefined);assert.equal(calls.length,1);
});
test('stale detail and old lock completion cannot publish after identity change',async()=>{
 let complete;const a=auth(),client=new HttpClient(a,()=>new Promise(r=>complete=r));const read=loadAdminAccount(client,2);a.startSession({...a.context,accountId:3});complete(json(account()));assert.equal((await read).kind,'discarded');
 a.acceptCsrf({requestToken:'synthetic'});const write=lockAdminAccount(client,createLockIntent(account(),()=>key));a.startSession({...a.context,accountId:4});complete(json({account:account(),operationId:key}));assert.equal((await write).kind,'unknown');assert.equal(a.context.accountId,4);
});
