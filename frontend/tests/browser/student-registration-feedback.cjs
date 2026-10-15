const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
const {installShellControls}=require('./shell-controls.cjs');
const root=path.resolve(__dirname,'../../../docs/plan/student-registration-feedback-evidence');fs.mkdirSync(root,{recursive:true});
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true}),results=[];
 try{for(const width of [375,768,1280]){
  const context=await browser.newContext({viewport:{width,height:900}}),page=await context.newPage();installShellControls(page);
  const errors=[],writes=[];page.on('pageerror',e=>errors.push(e.message));
  const actions=['VIEW_ENTRY_RESULTS','BOOK_ENTRY_TEST','CANCEL_ENTRY_BOOKING','RESCHEDULE_ENTRY_BOOKING','CANCEL_REGISTRATION','CONFIRM_REGISTRATION','VIEW_INVOICE','VIEW_CLASS','VIEW_FINALIZED_REPORT'];
  const allowed=['VIEW_ENTRY_RESULTS','BOOK_ENTRY_TEST','CANCEL_REGISTRATION'];
  const course={id:1,code:'DEMO01',name:'Tiếng Anh giao tiếp',status:'ACTIVE'};
  const registration={id:186,course,status:'PENDING',createdAt:'2026-10-15T06:02:00.000Z',admissionConfirmedAt:null,band:null,invoiceId:null,classId:null,version:'AAAAAAAAAAE=',entry:{attempts:[],bandAssignment:null,bestAttemptId:null},invoice:null,class:null,rejection:null,cancellation:null,admissionApprovedAt:null,allowedActions:allowed,disabledReasons:Object.fromEntries(actions.filter(a=>!allowed.includes(a)).map(a=>[a,'Trạng thái hồ sơ, kết quả hoặc thời điểm hiện tại chưa cho phép.']))};
  await page.route('**/api/v1/**',async r=>{
   const req=r.request(),endpoint=new URL(req.url()).pathname,now=Date.now();let body;
   if(endpoint.endsWith('/auth/me'))body={accountId:1,fullName:'Học viên minh họa',email:'student@example.test',role:'Student',scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:new Date(now).toISOString(),effectiveExpiresAt:new Date(now+1200000).toISOString(),absoluteExpiresAt:new Date(now+7200000).toISOString()};
   else if(endpoint.endsWith('/auth/csrf'))body={requestToken:'synthetic-registration'};
   else if(endpoint.endsWith('/students/me/home'))body={items:[{registrationId:186,course,status:'PENDING',createdAt:registration.createdAt,currentStep:'Chờ đăng ký thi',nextAction:{action:'BOOK_ENTRY_TEST',target:{registrationId:186}},waitingReason:null,nearestEvent:null,unpaidInvoice:null,finalizedReport:null}],page:1,pageSize:20,totalCount:1};
   else if(endpoint.endsWith('/students/me/registrations/186'))body=registration;
   else{writes.push(endpoint);return r.abort();}
   assert.equal(req.method(),'GET');await r.fulfill({status:200,contentType:'application/json',body:JSON.stringify(body)});
  });
  await page.goto('http://127.0.0.1:5179/student/home');await page.getByRole('button',{name:'Xem hồ sơ 186',exact:true}).click();
  const detail=page.getByRole('region',{name:'Hồ sơ 186',exact:true});
  // Card is a labelled section, exposed as a region by its accessible name.
  await detail.waitFor();await page.evaluate(()=>document.fonts.ready);
  const reasons=detail.getByRole('list',{name:'Lý do thao tác chưa khả dụng',exact:true});
  assert.equal(await reasons.locator('li').count(),1);assert.match(await reasons.innerText(),/^Xác nhận kết quả:/);
  assert.equal(await detail.locator('.student-attempts').count(),0);
  assert.equal(await detail.getByRole('button',{name:'Chọn lịch kiểm tra đầu vào',exact:true}).isEnabled(),true);
  assert.equal(await detail.getByRole('button',{name:'Xem trước hủy hồ sơ',exact:true}).isEnabled(),true);
  assert.equal(await detail.getByRole('button',{name:'Xem trước xác nhận kết quả',exact:true}).isDisabled(),true);
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
  await detail.screenshot({path:path.join(root,`registration-${width}.png`)});assert.deepEqual(errors,[]);assert.deepEqual(writes,[]);
  results.push({width,reasonRows:1,emptyAttemptGrid:false,bookAndCancelEnabled:true,confirmationDisabled:true,overflow:false,apiMutations:0});await context.close();
 }
 fs.writeFileSync(path.join(root,'browser.json'),JSON.stringify({passed:true,mockHttpOnly:true,realRegistration186Untouched:true,cases:results},null,2));process.stdout.write(JSON.stringify({passed:true,cases:results.length}));
 }finally{await browser.close();}
})().catch(e=>{process.stderr.write(e.stack);process.exitCode=1;});
