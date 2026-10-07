import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url);
const load=path=>require(resolve(process.env.EC_TEST_BUILD,path));
const {AuthState}=load('lib/auth.js'); const {HttpClient}=load('lib/http.js');
const {SessionController}=load('features/identity/sessionController.js');
const {identityPath}=load('app/identityNavigation.js');
const context=(extra={})=>({accountId:1,fullName:'Tên mẫu',email:'sample@example.test',role:'Student',scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z',...extra});
const json=(value,status=200,headers={})=>new Response(JSON.stringify(value),{status,headers});
const error=(status,code,headers)=>json({code,message:'Thông báo an toàn',fieldErrors:{},traceId:'test'},status,headers);
function fixture(fetcher,authenticated=false){
 const auth=new AuthState(); if(authenticated)auth.startSession(context());auth.acceptCsrf({requestToken:'synthetic'});
 const timers=new Map();let id=0; const calls=[];
 const client=new HttpClient(auth,async(path,init)=>{calls.push({path,init});return path.endsWith('/csrf')?json({requestToken:'synthetic-next'}):fetcher(path,init);});
 const controller=new SessionController(client,{setTimer:(callback,ms)=>{timers.set(++id,{callback,ms});return id;},clearTimer:id=>timers.delete(id)});
 return {auth,controller,calls,timers};
}
test('login raw credentials once; generic401/429/CSRF never retries or reveals account',async()=>{
 for(const [status,code] of [[401,'AUTHENTICATION_FAILED'],[429,'RATE_LIMITED'],[403,'CSRF_INVALID']]){
  const f=fixture(()=>error(status,code,{'Retry-After':'60'}));
  await f.controller.login('sample@example.test','  Á nguyên văn  ');
  assert.equal(f.calls.length,1);assert.equal(JSON.parse(f.calls[0].init.body).password,'  Á nguyên văn  ');
  assert.equal(f.controller.snapshot.context,null);assert.equal(f.controller.snapshot.retryAfter,status===429?'60':null);
  if(status===401)assert.equal(f.controller.snapshot.notice,'Email hoặc mật khẩu không hợp lệ.');
  if(status===403)assert.equal(f.auth.csrf,null);
  f.controller.dispose();
 }
});
test('new login of same account clears prior RAM; fresh csrf only; double send blocked',async()=>{
 let complete;const f=fixture(()=>new Promise(r=>complete=r),true);let cleared=0,signals=0;
 f.auth.requests.registerCleanup(()=>cleared++);f.controller.setSignalSender(()=>signals++);
 const pending=f.controller.login('sample@example.test','raw');await f.controller.login('sample@example.test','raw');
 complete(json(context()));await pending;
 assert.equal(cleared,1);assert.equal(signals,1);assert.equal(f.controller.snapshot.phase,'ready');
 assert.deepEqual(f.calls.map(x=>x.path),['/api/v1/auth/login','/api/v1/auth/csrf']);
 assert.equal(f.timers.size,1);assert.equal([...f.timers.values()][0].ms,1200000);f.controller.dispose();assert.equal(f.timers.size,0);
});
test('stale lost login/logout cannot clear new authority or emit signal',async()=>{
 for(const action of ['login','logout']){
  let reject;const f=fixture(()=>new Promise((_,r)=>reject=r),true);let signals=0;f.controller.setSignalSender(()=>signals++);
  const pending=action==='login'?f.controller.login('sample@example.test','raw'):f.controller.logout();
  f.auth.startSession(context({accountId:2}));reject(new Error('lost'));await pending;
  assert.equal(f.auth.context.accountId,2);assert.equal(signals,0);assert.equal(f.controller.snapshot.phase,'ready');f.controller.dispose();
 }
});
test('logout204/current401 clears RAM;403 no success; unknown clears locally without repeat',async()=>{
 for(const kind of ['success','expired','forbidden','csrf','lost']){
  const f=fixture(()=>kind==='success'?new Response(null,{status:204}):kind==='expired'?error(401,'AUTHENTICATION_REQUIRED'):kind==='forbidden'?error(403,'FORBIDDEN'):kind==='csrf'?error(403,'CSRF_INVALID'):Promise.reject(new Error('lost')),true);
  let cleanup=0;f.auth.requests.registerCleanup(()=>cleanup++);await f.controller.logout();
  assert.equal(f.calls.filter(x=>x.init.method==='POST').length,1);
  assert.equal(Boolean(f.auth.context),kind==='forbidden'||kind==='csrf');
  if(kind==='lost'){assert.equal(f.controller.snapshot.phase,'unknown');assert.match(f.controller.snapshot.notice,/Chưa xác nhận/);}
  if(kind==='forbidden'||kind==='csrf')assert.doesNotMatch(f.controller.snapshot.notice,/Đã đăng xuất/);
  if(kind==='success'||kind==='expired'||kind==='lost')assert.ok(cleanup>0);
  f.controller.dispose();
 }
});
test('deadline read clears proof and fetches me/CSRF only; server interval not wall time',async()=>{
 const f=fixture(()=>json(context()));await f.controller.checkSession();
 const timer=[...f.timers.values()][0];assert.equal(timer.ms,1200000);f.calls.length=0;
 timer.callback();assert.equal(f.auth.context,null);assert.equal(f.controller.snapshot.phase,'checking');
 await new Promise(r=>setImmediate(r));assert.deepEqual(f.calls.map(x=>x.path),['/api/v1/auth/me','/api/v1/auth/csrf']);
 assert.ok(f.calls.every(x=>x.init.method==='GET'));assert.equal(f.auth.context.effectiveExpiresAt,context().effectiveExpiresAt);f.controller.dispose();
});
test('equal/past deadline fails closed without zero timer loop; disposed and hidden discard late proof',async()=>{
 for(const action of ['checkSession','login']){
  const f=fixture(()=>json(context({effectiveExpiresAt:context().serverTime})));
  await (action==='login'?f.controller.login('sample@example.test','raw'):f.controller.checkSession());
  assert.equal(f.auth.context,null);assert.equal(f.controller.snapshot.phase,'unknown');assert.equal(f.timers.size,0);f.controller.dispose();
 }
 for(const action of ['pause','dispose']){
  let complete;const f=fixture(()=>new Promise(r=>complete=r));const pending=f.controller.checkSession();
  f.controller[action]();complete(json(context()));await pending;assert.equal(f.auth.context,null);assert.equal(f.timers.size,0);f.controller.dispose();
 }
});
test('all normal first-F destinations and restricted/anonymous navigation are fixed',()=>{
 for(const [role,path] of Object.entries({Student:'/student/home',Admissions:'/admissions/registrations',AcademicAffairs:'/academics/entry-test-sessions',Teacher:'/teacher/classes',Accountant:'/finance/invoices',Director:'/reports',Admin:'/admin/accounts'}))assert.equal(identityPath(context({role}),'/register'),path);
 assert.equal(identityPath(context({scope:'PASSWORD_CHANGE'}),'/admin/accounts'),'/account/change-password');
 assert.equal(identityPath(null,'/admin/accounts'),'/login');assert.equal(identityPath(null,'/register'),'/register');
});
