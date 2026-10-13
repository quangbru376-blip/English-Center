// Checks scoped initial effects in the real DEV main/StrictMode entry with mock
// transport. POST search is read-only; this is not SQL/authentication evidence.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{
  const browser=await chromium.launch({channel:'msedge',headless:true}),results=[];
  const cases=[
    ['Admin','/admin/accounts',['/api/v1/admin/accounts/search']],
    ['Admin','/admin/audit-events',['/api/v1/admin/audit-events']],
    ['Admissions','/admissions/registrations',['/api/v1/admissions/registrations/search']],
    ['AcademicAffairs','/academics/courses',['/api/v1/academics/courses']],
    ['AcademicAffairs','/academics/training-programs',['/api/v1/academics/training-programs']],
    ['AcademicAffairs','/academics/entry-test-sessions',['/api/v1/admissions/entry-test-sessions']],
    ['AcademicAffairs','/academics/classes',['/api/v1/academics/classes','/api/v1/academics/schedules']],
    ['AcademicAffairs','/academics/grade-reviews',['/api/v1/academics/grade-reviews']],
    ['Teacher','/teacher/classes',['/api/v1/academics/teachers/me/classes','/api/v1/academics/teachers/me/schedules']],
    ['Teacher','/teacher/entry-test-sessions',['/api/v1/admissions/teachers/me/entry-test-sessions']],
    ['Accountant','/accountant/invoices',['/api/v1/finance/invoices/search']],
    ['Accountant','/accountant/reports',['/api/v1/reports/accountant/finance']],
    ['Director','/reports',['/api/v1/reports/director/finance']],
  ];
  try{for(const [role,route,expected] of cases){
    const context=await browser.newContext(),page=await context.newPage(),seen=[];
    const now=new Date(),iso=delta=>new Date(+now+delta).toISOString();
    await page.route('**/api/v1/**',async requestRoute=>{
      const request=requestRoute.request(),url=new URL(request.url()),endpoint=url.pathname;
      let body;
      if(endpoint==='/api/v1/auth/me')body={accountId:1,fullName:'Synthetic Staff',email:'staff@example.test',role,scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:iso(0),effectiveExpiresAt:iso(1200000),absoluteExpiresAt:iso(7200000)};
      else if(endpoint==='/api/v1/auth/csrf')body={requestToken:'synthetic-staff-strict'};
      else{
        assert.ok(expected.includes(endpoint),`Unexpected business endpoint ${endpoint}`);
        assert.equal(request.method(),endpoint.endsWith('/search')?'POST':'GET');seen.push(endpoint);
        body={items:[],page:1,pageSize:20,totalCount:0};
        if(endpoint.endsWith('/schedules'))body.dateWindow={fromDate:null,toDate:null};
        if(endpoint.includes('/reports/')){
          const today=new Intl.DateTimeFormat('sv-SE',{timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'}).format(now),fromDate=today.slice(0,8)+'01';
          const last=new Date(Date.UTC(Number(today.slice(0,4)),Number(today.slice(5,7)),0)),toDate=last.toISOString().slice(0,10);
          body={queriedAt:iso(0),todayVn:today,appliedFilters:{fromDate,toDate,courseId:null,overdueOnly:false},options:{courses:[]},totals:{revenueAmount:'0',billedAmount:'0',currentDebtAmount:'0',overdueDebtAmount:'0'},groups:{items:[],page:1,pageSize:20,totalCount:0}};
        }
      }
      await requestRoute.fulfill({status:200,contentType:'application/json',body:JSON.stringify(body)});
    });
    await page.goto('http://127.0.0.1:5178'+route);
    // Wait for decoded read data rather than just the request counter.
    await page.getByText(/Chưa có|Không có|0 tài khoản|0 hồ sơ|0 ca thi|0 chương trình|Tra cứu lúc/).first().waitFor();
    await page.waitForFunction(()=>!Array.from(document.querySelectorAll('[role=status]')).some(e=>e.textContent.includes('Đang')));
    assert.deepEqual(seen.sort(),expected.slice().sort());results.push({role,route,reads:seen.length,onePerExpectedEndpoint:true,mutations:0});await context.close();
  }
  const root=path.resolve(__dirname,'../../../docs/plan/UI-refresh-evidence/stage5');fs.writeFileSync(path.join(root,'strict-mode.json'),JSON.stringify({passed:true,realDevMainStrictMode:true,mockHttpOnly:true,cases:results},null,2));process.stdout.write(JSON.stringify({passed:true,cases:results.length}));
  }finally{await browser.close();}
})().catch(error=>{process.stderr.write(error.stack);process.exitCode=1;});
