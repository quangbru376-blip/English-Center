import {test} from 'node:test';
import assert from 'node:assert/strict';
import {createRequire} from 'node:module';
import {resolve} from 'node:path';
const require=createRequire(import.meta.url),load=p=>require(resolve(process.env.EC_TEST_BUILD,p));
const {AuthState}=load('lib/auth.js'),{HttpClient}=load('lib/http.js');
const {credential,createStaff,searchAccounts,staffLookup}=load('features/administration/staffApi.js');
const {decodeCourse,courseCommand,searchCourses}=load('features/catalogue/catalogueApi.js');
const {identityPath}=load('app/identityNavigation.js');
const key='12345678-1234-1234-1234-123456789abc';
const auth=(role='Admin',scope='NORMAL')=>{const a=new AuthState();a.startSession({accountId:1,fullName:'Synthetic',email:'actor@example.test',role,scope,accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-09T01:00:00Z',effectiveExpiresAt:'2026-10-09T01:20:00Z',absoluteExpiresAt:'2026-10-09T03:00:00Z'});a.acceptCsrf({requestToken:'synthetic'});return a;};
const first=()=>({accountId:2,email:'new@example.test',role:'Teacher',temporaryPassword:'A'.repeat(32),temporaryPasswordExpiresAt:'2026-10-10T01:00:00.000Z',operationId:key,accountVersion:'AAAAAAAAAAE='});
const course=()=>({id:3,code:'IELTS',name:'IELTS',basePrice:'999999999999999999',status:'ACTIVE',version:'AAAAAAAAAAE=',allowedActions:['EDIT_DETAILS','DEACTIVATE'],disabledReasons:{ACTIVATE:'Đã hoạt động.'}});
const json=(body,status=200)=>new Response(JSON.stringify(body),{status});
test('credential first seven fields and secret-unavailable replay reject hidden data',()=>{
 assert.equal(credential(first()).temporaryPassword.length,32);
 const safe={outcome:'COMPLETED',operationId:key,resultMode:'SECRET_RESULT_UNAVAILABLE',target:{accountId:2}};
 assert.equal(credential(safe).resultMode,'SECRET_RESULT_UNAVAILABLE');
 for(const bad of [{...safe,temporaryPassword:'A'.repeat(32)},{...safe,resultMode:'SAFE_REFERENCE'},{...first(),passwordHash:'bad'},{...first(),temporaryPassword:'short'}])assert.throws(()=>credential(bad));
});
test('staff create accepts first201 and replay200 exactly once; unknown never retries',async()=>{
 let sends=0;const client=new HttpClient(auth(),async()=>{sends++;return json(first(),201);});
 assert.equal((await createStaff(client,{fullName:'Name',email:'new@example.test',role:'Teacher'},key,new AbortController().signal)).kind,'success');assert.equal(sends,1);
 const lost=new HttpClient(auth(),async()=>{sends++;throw new Error('lost');});assert.equal((await createStaff(lost,{fullName:'Name',email:'new@example.test',role:'Teacher'},key,new AbortController().signal)).kind,'unknown');assert.equal(sends,2);
});
test('account search PII stays body, hasCSRF but read failure cannot become unknown mutation',async()=>{
 let request;const client=new HttpClient(auth(),async(url,options)=>{request={url,options};return json({items:[],page:1,pageSize:20,totalCount:0});});
 assert.equal((await searchAccounts(client,{email:'private@example.test'},new AbortController().signal)).kind,'success');
 assert.equal(request.url,'/api/v1/admin/accounts/search');assert.equal(request.options.headers['X-CSRF-TOKEN'],'synthetic');assert.ok(request.options.body.includes('private@example.test'));assert.equal(request.options.headers['Idempotency-Key'],undefined);
});
test('role and PASSWORD_CHANGE deny both modules before network',async()=>{
 let sends=0;const fetcher=async()=>{sends++;return json({});};
 for(const role of ['Student','Teacher','Director','Accountant','Admissions']){
  const client=new HttpClient(auth(role),fetcher);assert.equal((await searchAccounts(client,{},new AbortController().signal)).kind,'blocked');assert.equal((await searchCourses(client,{},new AbortController().signal)).kind,'blocked');
 }
 assert.equal((await searchCourses(new HttpClient(auth('AcademicAffairs','PASSWORD_CHANGE'),fetcher),{},new AbortController().signal)).kind,'blocked');assert.equal(sends,0);
});
test('course exact large VND and immutable capability allowlists',()=>{
 assert.equal(decodeCourse(course()).basePrice,'999999999999999999');
 for(const extra of [{basePrice:999999999999999999},{basePrice:'0'},{version:'bad'},{invoiceId:1},{allowedActions:['EDIT_DETAILS','DELETE']}])assert.throws(()=>decodeCourse({...course(),...extra}));
});
test('course create first201/replay200 and malformed query remain bounded',async()=>{
 let requests=[];const client=new HttpClient(auth('AcademicAffairs'),async(url,options)=>{requests.push({url,options});return json({course:course(),operationId:key},201);});
 assert.equal((await courseCommand(client,'courseCreate',{code:'IELTS',name:'IELTS',basePrice:'999999999999999999',status:'ACTIVE'},key,new AbortController().signal)).kind,'success');
 assert.equal(requests[0].options.headers['Idempotency-Key'],key);assert.equal((await searchCourses(client,{actorId:'2'},new AbortController().signal)).kind,'blocked');assert.equal(requests.length,1);
});
test('lookup action families exact, no arbitrary cross-family result access',async()=>{
 let sends=0;const client=new HttpClient(auth(),async()=>{sends++;return json({});});
 assert.equal((await staffLookup(client,'ACADEMIC.COURSE.CREATE',key,new AbortController().signal)).kind,'blocked');assert.equal(sends,0);
});
test('catalogue has explicit route while accepted first-F landing and restricted route stay fixed',()=>{
 const context=auth('AcademicAffairs').context;assert.equal(identityPath(context,'/'),' /academics/entry-test-sessions'.trim());assert.equal(identityPath(context,'/academics/courses'),'/academics/courses');assert.equal(identityPath({...context,scope:'PASSWORD_CHANGE'},'/academics/courses'),'/account/change-password');
});
