// Bounded visual review of real UI with synthetic transport; never connects to SQL/demo.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
const {installShellControls}=require('./shell-controls.cjs');
const origin='http://127.0.0.1:5179';
const stage=process.argv[2];assert.ok(['before','after'].includes(stage));
const root=path.resolve(__dirname,'../../../docs/plan/ui-spacing-evidence',stage);
fs.mkdirSync(root,{recursive:true});
function fingerprint(){const result={};function scan(dir){for(const e of fs.readdirSync(dir,{withFileTypes:true})){const file=path.join(dir,e.name);if(e.isDirectory())scan(file);else result[path.relative(path.join(__dirname,'../../src'),file)]=crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');}}scan(path.join(__dirname,'../../src'));return result;}
const sourceBefore=fingerprint();
const cases=[['Admin','/admin/accounts'],['Admin','/admin/audit-events'],['Admissions','/admissions/registrations'],['AcademicAffairs','/academics/courses'],['AcademicAffairs','/academics/training-programs'],['AcademicAffairs','/academics/entry-test-sessions'],['AcademicAffairs','/academics/classes'],['AcademicAffairs','/academics/grade-reviews'],['Teacher','/teacher/classes'],['Teacher','/teacher/entry-test-sessions'],['Accountant','/accountant/invoices'],['Accountant','/accountant/reports'],['Director','/reports']];
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true}),images=[],errors=[],unexpected=[];
 const shot=async(page,name,width)=>{
  await page.evaluate(()=>document.fonts.ready);
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true,name+' page overflow');
  const filename=`${width}-${name}.png`;
  await page.screenshot({path:path.join(root,filename),fullPage:true,animations:'disabled'});
  images.push({filename,width,name});
 };
 try{
  for(const width of [375,768,1280]){
   for(const [index,[role,route]] of cases.entries()){
    const context=await browser.newContext({viewport:{width,height:900}}),page=await context.newPage();
    page.on('pageerror',e=>errors.push(e.message));
    let state='empty';
    await page.route('**/api/v1/**',async r=>{
     const req=r.request(),endpoint=new URL(req.url()).pathname,now=new Date(),iso=d=>new Date(+now+d).toISOString();
     let body;
     if(endpoint==='/api/v1/auth/me')body={accountId:1,fullName:'Nhân sự minh họa',email:'spacing@example.test',role,scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:iso(0),effectiveExpiresAt:iso(1200000),absoluteExpiresAt:iso(7200000)};
     else if(endpoint==='/api/v1/auth/csrf')body={requestToken:'synthetic-spacing'};
     else{
      if(req.method()!=='GET'&&!endpoint.endsWith('/search')){unexpected.push(endpoint);return r.abort();}
      if(state==='error')return r.fulfill({status:503,contentType:'application/json',body:JSON.stringify({code:'SERVICE_UNAVAILABLE',message:'Không thể đọc dữ liệu lúc này. Vui lòng thử lại.',fieldErrors:{},traceId:'spacing-synthetic'})});
      if(state==='loading')await new Promise(resolve=>setTimeout(resolve,1500));
      body={items:[],page:1,pageSize:20,totalCount:0};
      if(endpoint.endsWith('/schedules'))body.dateWindow={fromDate:null,toDate:null};
      if(endpoint.includes('/reports/')){
       const today=new Intl.DateTimeFormat('sv-SE',{timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'}).format(now),fromDate=today.slice(0,8)+'01',toDate=new Date(Date.UTC(+today.slice(0,4),+today.slice(5,7),0)).toISOString().slice(0,10);
       body={queriedAt:iso(0),todayVn:today,appliedFilters:{fromDate,toDate,courseId:null,overdueOnly:false},options:{courses:[]},totals:{revenueAmount:'0',billedAmount:'0',currentDebtAmount:'0',overdueDebtAmount:'0'},groups:body};
      }
     }
     await r.fulfill({status:200,contentType:'application/json',body:JSON.stringify(body)});
    });
    await page.goto(origin+route);
    await page.getByText(/Chưa có|Không có|0 tài khoản|0 hồ sơ|0 ca thi|0 chương trình|Tra cứu lúc/).first().waitFor();
    await page.waitForFunction(()=>!Array.from(document.querySelectorAll('[role=status]')).some(e=>e.textContent.includes('Đang')));
    await shot(page,`staff-${index}-${role}`,width);
    if(stage==='after'){
     const labelGaps=await page.locator('label:has(> select)').evaluateAll(labels=>labels.filter(label=>label.getBoundingClientRect().height>0).map(label=>{
      const child=label.querySelector('select'),range=document.createRange();range.selectNodeContents(label);range.setEndBefore(child);
      return child.getBoundingClientRect().top-range.getBoundingClientRect().bottom;
     }));
     for(const gap of labelGaps)assert.ok(gap>=7,`Label/select gap ${route}: ${gap}`);
    }
    if(index===0){
     for(const next of ['loading','error']){
      state=next;await page.getByRole('button',{name:'Tìm kiếm',exact:true}).click();
      await page.getByText(next==='loading'?'Đang xử lý…':'Không thể đọc dữ liệu lúc này. Vui lòng thử lại.',{exact:true}).waitFor();
      await shot(page,'staff-'+next,width);
      if(next==='loading')await page.waitForFunction(()=>!Array.from(document.querySelectorAll('[role=status]')).some(e=>e.textContent.includes('Đang')));
     }
    }
    await context.close();
   }
   const page=await browser.newPage({viewport:{width,height:900}});installShellControls(page);page.on('pageerror',e=>errors.push(e.message));
   await page.route('**/api/**',r=>{unexpected.push(new URL(r.request().url()).pathname);return r.abort();});
   await page.goto(origin+'/tests/ui-spacing.html');await page.getByRole('heading',{name:'Rà khoảng cách thành phần',exact:true}).waitFor();
   await shot(page,'shared-forms-calendar',width);
   if(stage==='after'){await page.getByRole('button',{name:'Đổi dạng lịch mẫu',exact:true}).click();await shot(page,'shared-calendar-week',width);}
   const opener=page.getByRole('button',{name:'Mở xác nhận mẫu',exact:true});await opener.click();await page.getByRole('dialog').waitFor();
   const gaps=await page.getByRole('dialog').evaluate(dialog=>{const [a,b,c]=[...dialog.querySelectorAll('p, .ec-modal-body > button')];return {paragraph:b.getBoundingClientRect().top-a.getBoundingClientRect().bottom,action:c?c.getBoundingClientRect().top-b.getBoundingClientRect().bottom:null};});
   await shot(page,'shared-dialog',width);await page.keyboard.press('Escape');assert.equal(await opener.evaluate(e=>document.activeElement===e),true);
   if(stage==='after'){assert.ok(gaps.paragraph>=15,'Dialog paragraph gap');assert.ok(gaps.action>=15,'Dialog action gap');}
   await page.goto(origin+'/tests/ui-refresh-student.html');await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();await shot(page,'student-home',width);
   // This visual-only fixture exposes buttons directly without the production drawer.
   await page.getByRole('button',{name:'Chọn khóa học',exact:true}).click();await page.getByRole('heading',{name:'Chọn khóa học',exact:true}).waitFor();await shot(page,'student-courses',width);
   if(stage==='after'){
    await page.unroute('**/api/**');
    await page.route('**/api/**',r=>{
     const endpoint=new URL(r.request().url()).pathname;
     if(endpoint==='/api/v1/auth/me')return r.fulfill({status:401,contentType:'application/json',body:JSON.stringify({code:'UNAUTHENTICATED',message:'Vui lòng đăng nhập.',fieldErrors:{},traceId:'synthetic'})});
     if(endpoint==='/api/v1/auth/csrf')return r.fulfill({status:200,contentType:'application/json',body:JSON.stringify({requestToken:'synthetic-anonymous'})});
     unexpected.push(endpoint);return r.abort();
    });
    for(const [name,route]of [['login','/login'],['signup','/register']]){
     await page.goto(origin+route);await page.getByLabel('Email',{exact:true}).waitFor();await shot(page,'auth-'+name,width);
    }
   }
   await page.close();
  }
  assert.deepEqual(errors,[]);assert.deepEqual(unexpected,[]);
  assert.deepEqual(fingerprint(),sourceBefore,'Source changed during capture');
  fs.writeFileSync(path.join(root,'receipt.json'),JSON.stringify({passed:true,stage,widths:[375,768,1280],mockHttpOnly:true,liveDemoRequests:0,errors,unexpected,images,sourceFingerprints:sourceBefore},null,2));
  process.stdout.write(JSON.stringify({passed:true,stage,images:images.length}));
 }finally{await browser.close();}
})().catch(e=>{process.stderr.write(e.stack);process.exitCode=1;});
