import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url);
const {AuthState}=require(resolve(process.env.EC_TEST_BUILD,'lib/auth.js'));
const {HttpClient}=require(resolve(process.env.EC_TEST_BUILD,'lib/http.js'));
const {decodeApiError}=require(resolve(process.env.EC_TEST_BUILD,'lib/apiError.js'));
const context=()=>({accountId:1,fullName:'Tên mẫu',email:'sample@example.test',role:'Student',scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z'});
const json=(value,status=200,headers={})=>new Response(JSON.stringify(value),{status,headers});
const envelope=(code)=>({code,message:'Thông báo an toàn',fieldErrors:{},traceId:'test-trace'});
const identity=value=>value;
const options=(endpoint='me',slot='read',body)=>({endpoint,slot,body,decode:identity});
const ready=()=>{const a=new AuthState();a.startSession(context());a.acceptCsrf({requestToken:'opaque'});return a;};
test('explicit csrf bootstrap and me use cookie/no-store and update only RAM without idle renewal',async()=>{
 const auth=new AuthState();const calls=[];
 const client=new HttpClient(auth,async(path,init)=>{calls.push([path,init]);return json({requestToken:'opaque'});});
 assert.equal((await client.request(options('csrf'))).kind,'success');assert.equal(auth.csrf,'opaque');
 const init=calls[0][1];assert.equal(init.credentials,'same-origin');assert.equal(init.cache,'no-store');assert.equal(init.redirect,'error');assert.equal(init.method,'GET');assert.equal(init.body,undefined);
 assert.deepEqual(Object.keys(init.headers),['Accept']);
 const me=new HttpClient(auth,async()=>json(context()));await me.request(options());assert.equal(auth.context.accountId,1);assert.equal(auth.csrf,null);
});
test('credentials sent exactly once with raw Unicode/spaces; successful login starts new session and drops old csrf',async()=>{
 const auth=new AuthState();auth.acceptCsrf({requestToken:'opaque'});let calls=0;
 const raw='  Á thử chuỗi  ';const client=new HttpClient(auth,async(path,init)=>{calls++;assert.equal(path,'/api/v1/auth/login');assert.equal(JSON.parse(init.body).password,raw);assert.equal(init.headers['X-CSRF-TOKEN'],'opaque');assert.ok(!('Authorization' in init.headers));assert.ok(!('Origin' in init.headers));return json(context());});
 assert.equal((await client.request(options('login','credentials',{email:'sample@example.test',password:raw}))).kind,'success');assert.equal(calls,1);assert.equal(auth.csrf,null);
});
test('unsafe without csrf, invalid endpoint/query/body, anonymous protected and restricted login fail before send',async()=>{
 let calls=0;const auth=new AuthState();const client=new HttpClient(auth,async()=>{calls++;return json({});});
 for(const input of [options('login'),options('logout'),options('https://other.test'),options('__proto__'),options('me?email=pii'),options('csrf','read',{})])assert.equal((await client.request(input)).kind,'blocked');
 auth.startSession({...context(),scope:'PASSWORD_CHANGE'});auth.acceptCsrf({requestToken:'opaque'});
 assert.equal((await client.request(options('login'))).kind,'blocked');assert.equal(calls,0);
});
test('generic login401 retains identity; current protected401 clears all sensitive RAM',async()=>{
 const auth=ready();let clear=0;auth.requests.registerCleanup(()=>clear++);
 const client=new HttpClient(auth,async()=>json(envelope('AUTHENTICATION_FAILED'),401));
 assert.equal((await client.request(options('login'))).action,'credentials');assert.ok(auth.context);assert.equal(clear,0);
 const protectedClient=new HttpClient(auth,async()=>json(envelope('AUTHENTICATION_REQUIRED'),401));
 assert.equal((await protectedClient.request(options())).action,'session');assert.equal(auth.context,null);assert.equal(auth.csrf,null);assert.equal(clear,1);
});
test('old401 ignoring abort cannot clear newer session; reverse completion is discarded',async()=>{
 const auth=ready();const resolvers=[];const client=new HttpClient(auth,()=>new Promise(resolve=>resolvers.push(resolve)));
 const old=client.request(options());auth.startSession(context());const current=client.request(options());
 resolvers[1](json(context()));assert.equal((await current).kind,'success');resolvers[0](json(envelope('AUTHENTICATION_REQUIRED'),401));assert.equal((await old).kind,'discarded');assert.ok(auth.context);
 const a=client.request(options());const b=client.request(options());resolvers[3](json(context()));assert.equal((await b).kind,'success');resolvers[2](json(context()));assert.equal((await a).kind,'discarded');
});
test('403 csrf invalid clears token/drafts with no automatic retry; forbidden stays distinct',async()=>{
 const auth=ready();let calls=0,cleared=0;auth.requests.registerCleanup(()=>cleared++);
 const client=new HttpClient(auth,async()=>{calls++;return json(envelope('CSRF_INVALID'),403);});
 const result=await client.request(options('password','write',{}));assert.equal(result.action,'csrf');assert.equal(auth.csrf,null);assert.equal(cleared,1);assert.equal(calls,1);assert.ok(auth.context);
 const forbidden=new HttpClient(auth,async()=>json(envelope('FORBIDDEN'),403));assert.equal((await forbidden.request(options())).action,'forbidden');
});
test('409 requires review;404 safe;429 exposes Retry-After without retry;500 write is unknown',async()=>{
 for(const [status,code,action] of [[409,'CONFLICT','conflict'],[404,'NOT_FOUND','not-found'],[429,'RATE_LIMITED','rate-limit'],[500,'INTERNAL_ERROR','server']]){
  let calls=0;const client=new HttpClient(ready(),async()=>{calls++;return json(envelope(code),status,{'Retry-After':'60'});});
  const result=await client.request(options('password','write',{}));assert.equal(result.action,action);assert.equal(calls,1);assert.equal(result.outcome,status===500?'unknown':'rejected');assert.equal(result.retryAfter,status===429?'60':null);
 }
});
test('lost/cancelled/malformed successful write never claims rollback or resends',async()=>{
 for(const fetcher of [async()=>{throw new Error('synthetic transport');},async()=>new Response('bad'),async()=>json({})]){
  let calls=0;const client=new HttpClient(ready(),async(...args)=>{calls++;return fetcher(...args);});
  const result=await client.request({...options('password','write',{}),decode:()=>{throw new Error('invalid success');}});assert.equal(result.kind,'unknown');assert.equal(calls,1);
 }
 const abort=new AbortController();let resolve;const client=new HttpClient(ready(),()=>new Promise(r=>resolve=r));
 const result=client.request({...options('password','write',{}),signal:abort.signal});abort.abort();resolve(new Response(null,{status:204}));assert.equal((await result).kind,'unknown');
});
test('malformed protected401 clears RAM; malformed envelope rejected without details',async()=>{
 assert.throws(()=>decodeApiError({...envelope('CONFLICT'),details:'internal'}));assert.throws(()=>decodeApiError({...envelope('CONFLICT'),fieldErrors:{name:[1]}}));
 const auth=ready();const client=new HttpClient(auth,async()=>new Response('bad',{status:401}));assert.equal((await client.request(options())).kind,'unavailable');assert.equal(auth.context,null);
});
test('logout/password only clear after valid successful response; logout204 no JSON parse',async()=>{
 const auth=ready();const client=new HttpClient(auth,async()=>new Response(null,{status:204}));
 const result=await client.request({...options('logout'),decode:value=>{assert.equal(value,undefined);return undefined;}});
 assert.equal(result.kind,'success');assert.equal(auth.context,null);assert.equal(auth.csrf,null);
});
test('old csrf ignored after session change; wrong success status and malformed me never publish authority',async()=>{
 const auth=ready();let resolve;const client=new HttpClient(auth,()=>new Promise(r=>resolve=r));
 const pending=client.request(options('csrf'));auth.startSession(context());resolve(json({requestToken:'old'}));assert.equal((await pending).kind,'discarded');assert.equal(auth.csrf,null);
 const malformed=new HttpClient(auth,async()=>json({accountId:1}));assert.notEqual((await malformed.request(options())).kind,'success');assert.equal(auth.context,null);
 const wrong=new HttpClient(ready(),async()=>json({},202));assert.equal((await wrong.request(options('password','write',{}))).kind,'unknown');
});
test('lost login clears old local authority without claiming success; stale lost login cannot clear newer identity',async()=>{
 const auth=ready();const client=new HttpClient(auth,async()=>{throw new Error('synthetic');});
 assert.equal((await client.request(options('login','credentials',{password:'synthetic'}))).kind,'unknown');assert.equal(auth.context,null);assert.equal(auth.csrf,null);
 auth.startSession(context());auth.acceptCsrf({requestToken:'opaque'});let reject;
 const stale=new HttpClient(auth,()=>new Promise((_,r)=>reject=r));const pending=stale.request(options('login','credentials',{}));
 auth.startSession(context());reject(new Error('synthetic'));assert.equal((await pending).kind,'unknown');assert.ok(auth.context);
});
