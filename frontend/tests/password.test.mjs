import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url),load=path=>require(resolve(process.env.EC_TEST_BUILD,path));
const {AuthState}=load('lib/auth.js'),{HttpClient}=load('lib/http.js');
const {SessionController}=load('features/identity/sessionController.js');
const {validatePassword}=load('features/identity/passwordValidation.js');
const {changeOwnPassword}=load('features/identity/passwordApi.js');
const proof=(scope='NORMAL')=>({accountId:1,fullName:'Tên mẫu',email:'sample@example.test',role:'Student',scope,accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z'});
const json=(x,status=200)=>new Response(JSON.stringify(x),{status});
const fail=(status,code)=>json({code,message:'Thông báo an toàn',fieldErrors:{currentPassword:['Mật khẩu hiện tại không hợp lệ.']},traceId:'test'},status);
function fixture(fetcher,scope){const auth=new AuthState();auth.startSession(proof(scope));auth.acceptCsrf({requestToken:'synthetic'});const calls=[];const client=new HttpClient(auth,async(path,init)=>{calls.push({path,init});return path.endsWith('/csrf')?json({requestToken:'next'}):fetcher(path,init);});const controller=new SessionController(client,{setTimer:()=>1,clearTimer:()=>{}});return {auth,client,controller,calls};}
test('raw codepoint validation and local confirmation do not rewrite credentials',()=>{
 const raw='  Á nguyên văn  ';assert.deepEqual(validatePassword({currentPassword:' ',newPassword:raw,confirmation:raw}),{});
 assert.ok(validatePassword({currentPassword:'old',newPassword:'😀'.repeat(14),confirmation:'different'}).newPassword);
 assert.ok(validatePassword({currentPassword:'old',newPassword:'😀'.repeat(129),confirmation:'😀'.repeat(129)}).newPassword);
 assert.equal(validatePassword({currentPassword:'old',newPassword:'a'.repeat(15),confirmation:'a'.repeat(15)}).newPassword,undefined);
});
test('normal/restricted password204 sends exact DTO once then clears session and reads csrf, no login',async()=>{
 for(const scope of ['NORMAL','PASSWORD_CHANGE']){const f=fixture(()=>new Response(null,{status:204}),scope);let signals=0;f.controller.setSignalSender(()=>signals++);let clear=0;f.auth.requests.registerCleanup(()=>clear++);
  await f.controller.changePassword('  old  ','  Á nguyên văn  ',proof().accountVersion);
  assert.deepEqual(JSON.parse(f.calls[0].init.body),{currentPassword:'  old  ',newPassword:'  Á nguyên văn  ',expectedVersions:{account:proof().accountVersion}});
  assert.deepEqual(f.calls.map(x=>x.path),['/api/v1/auth/change-password','/api/v1/auth/csrf']);assert.equal(f.auth.context,null);assert.ok(clear>0);assert.equal(signals,1);assert.equal(f.controller.snapshot.phase,'anonymous');f.controller.dispose();
 }
});
test('wrong current400 preserves proof, csrf403/401/conflict409 distinct; no write retry',async()=>{
 for(const [status,code] of [[400,'VALIDATION_ERROR'],[401,'AUTHENTICATION_REQUIRED'],[403,'CSRF_INVALID'],[409,'CONFLICT']]){const f=fixture(()=>fail(status,code));const result=await f.controller.changePassword('old','a'.repeat(15),proof().accountVersion);
  assert.equal(result.kind,'error');assert.equal(f.calls.length,1);assert.equal(Boolean(f.auth.context),status!==401);if(status===403)assert.equal(f.auth.csrf,null);if(status===409)assert.match(f.controller.snapshot.notice,/tải lại/);f.controller.dispose();
 }
});
test('lost password change clears local proof; stale lost response preserves newer identity',async()=>{
 const f=fixture(()=>Promise.reject(new Error('lost')));await f.controller.changePassword('old','a'.repeat(15),proof().accountVersion);assert.equal(f.auth.context,null);assert.equal(f.calls.length,1);assert.equal(f.controller.snapshot.phase,'unknown');assert.match(f.controller.snapshot.notice,/mật khẩu mới/);f.controller.dispose();
 let reject;const g=fixture(()=>new Promise((_,r)=>reject=r));const pending=g.controller.changePassword('old','a'.repeat(15),proof().accountVersion);g.auth.startSession({...proof(),accountId:2});reject(new Error('lost'));assert.equal((await pending).kind,'discarded');assert.equal(g.auth.context.accountId,2);g.controller.dispose();
});
test('RV mismatch and duplicate deliberate submit are blocked before send',async()=>{
 let complete;const f=fixture(()=>new Promise(r=>complete=r));assert.equal((await changeOwnPassword(f.client,'old','new','AAAAAAAAAAI=')).kind,'blocked');assert.equal(f.calls.length,0);
 const pending=f.controller.changePassword('old','a'.repeat(15),proof().accountVersion);assert.equal((await f.controller.changePassword('old','a'.repeat(15),proof().accountVersion)).kind,'blocked');complete(new Response(null,{status:204}));await pending;assert.equal(f.calls.filter(x=>x.init.method==='POST').length,1);f.controller.dispose();
});
