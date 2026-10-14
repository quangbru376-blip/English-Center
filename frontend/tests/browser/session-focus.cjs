// Effect lifecycle test against the real dev main.tsx/StrictMode entry.
// Mock HTTP state is not an authentication/SQL acceptance proof.
const fs = require('node:fs'), path = require('node:path'), assert = require('node:assert/strict');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
const { installShellControls } = require('./shell-controls.cjs');
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  try {
    const page = await browser.newPage(); installShellControls(page);
    let holdMe = false, releaseMe, revoke = false;
    const calls = { me: 0, csrf: 0, home: 0, courses: 0, invoices: 0, classes: 0, schedules: 0, grades: 0, mutations: 0, unexpected: 0 };
    const now = new Date(), context = { accountId: 1, fullName: 'Synthetic StrictMode', email: 'strict@example.test', role: 'Student', scope: 'NORMAL', accountVersion: 'AAAAAAAAAAE=', serverTime: now.toISOString(), effectiveExpiresAt: new Date(+now + 1200000).toISOString(), absoluteExpiresAt: new Date(+now + 7200000).toISOString() };
    await page.route('**/api/v1/**', async route => {
      const request = route.request(), url = new URL(request.url());
      if (request.method() !== 'GET') calls.mutations++;
      let body;
      if (url.pathname === '/api/v1/auth/me') {
        calls.me++;
        if (holdMe) await new Promise(resolve => { releaseMe = resolve; });
        if (revoke) return route.fulfill({status:401,contentType:'application/json',body:JSON.stringify({code:'SESSION_INVALID',message:'Phiên không còn hợp lệ.',fieldErrors:{},traceId:'synthetic-focus'})});
        body = context;
      }
      else if (url.pathname === '/api/v1/auth/csrf') {calls.csrf++;body = { requestToken: 'synthetic-strict-only' };}
      else if (url.pathname === '/api/v1/students/me/home') { calls.home++; body = { items: [], page: 1, pageSize: 20, totalCount: 0 }; }
      else if (url.pathname === '/api/v1/students/me/courses') { calls.courses++; body = { items: [], page: 1, pageSize: 20, totalCount: 0 }; }
      else if (url.pathname === '/api/v1/students/me/invoices') { calls.invoices++; body = { items: [], page: 1, pageSize: 20, totalCount: 0 }; }
      else if (url.pathname === '/api/v1/students/me/classes') { calls.classes++; body = { items: [], page: 1, pageSize: 20, totalCount: 0 }; }
      else if (url.pathname === '/api/v1/students/me/schedules') { calls.schedules++; body = { items: [], page: 1, pageSize: 20, totalCount: 0, dateWindow: { fromDate:null,toDate:null } }; }
      else if (url.pathname === '/api/v1/students/me/grade-reports') { calls.grades++; body = { items: [], page: 1, pageSize: 20, totalCount: 0 }; }
      else { calls.unexpected++; return route.abort(); }
      await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(body) });
    });
    await page.goto('http://127.0.0.1:5178/');
    await page.getByRole('heading', { name: 'Bắt đầu với một khóa học', exact: true }).waitFor();
    assert.equal(calls.home, 1);
    await page.getByRole('button', { name: 'Chọn khóa học', exact: true }).click();
    await page.getByRole('heading', { name: 'Chưa có khóa học đang mở', exact: true }).waitFor();
    assert.equal(calls.courses, 1);
    await page.getByRole('button', { name: 'Chọn khóa học', exact: true }).click();
    assert.equal(await page.getByRole('heading', { name: 'Chưa có khóa học đang mở', exact: true }).count(), 1);
    assert.equal(calls.courses, 1); assert.equal(calls.home, 1);
    await page.getByRole('button', {name:'Hóa đơn của tôi',exact:true}).click();
    await page.getByRole('heading', {name:'Chưa có hóa đơn phù hợp',exact:true}).waitFor();assert.equal(calls.invoices,1);
    await page.getByRole('button', {name:'Lớp của tôi',exact:true}).click();
    await page.getByRole('heading', {name:'Chưa có lớp phù hợp',exact:true}).waitFor();
    await page.getByText('Không có lịch học phù hợp.', {exact:true}).waitFor();assert.equal(calls.classes,1);assert.equal(calls.schedules,1);
    const input=page.getByRole('textbox').first();await input.fill('123');
    const before={...calls};
    await page.evaluate(()=>{for(let i=0;i<20;i++){window.dispatchEvent(new Event('focus'));document.dispatchEvent(new Event('visibilitychange'));}});
    await page.waitForTimeout(200);assert.deepEqual(calls,before);assert.equal(await input.inputValue(),'123');
    holdMe=true;
    await page.evaluate(()=>{Object.defineProperty(document,'visibilityState',{configurable:true,get:()=> 'hidden'});for(let i=0;i<3;i++)document.dispatchEvent(new Event('visibilitychange'));});
    await page.getByText('Đang xác minh phiên đăng nhập…',{exact:true}).waitFor();
    assert.equal(await page.getByRole('textbox').count(),0);assert.equal(calls.me,before.me);
    const returned=page.waitForRequest(r=>r.url().endsWith('/api/v1/auth/me'));
    await page.evaluate(()=>{Object.defineProperty(document,'visibilityState',{configurable:true,get:()=> 'visible'});for(let i=0;i<10;i++){document.dispatchEvent(new Event('visibilitychange'));window.dispatchEvent(new Event('focus'));}});
    await returned;await page.waitForTimeout(100);assert.equal(calls.me,before.me+1);assert.ok(releaseMe);
    holdMe=false;releaseMe();
    await page.getByRole('heading',{name:'Chưa có lớp phù hợp',exact:true}).waitFor();
    assert.equal(calls.me,before.me+1);assert.equal(calls.classes,before.classes+1);assert.equal(calls.schedules,before.schedules+1);
    const afterReturn=calls.me;
    const signalRequest=page.waitForRequest(r=>r.url().endsWith('/api/v1/auth/me'));
    await page.evaluate(()=>{const channel=new BroadcastChannel('ec-identity-v1');channel.postMessage('IDENTITY_CHANGED');setTimeout(()=>channel.close(),100);});await signalRequest;
    await page.getByRole('heading',{name:'Chưa có lớp phù hợp',exact:true}).waitFor();assert.equal(calls.me,afterReturn+1);
    revoke=true;
    await page.evaluate(()=>{Object.defineProperty(document,'visibilityState',{configurable:true,get:()=> 'hidden'});document.dispatchEvent(new Event('visibilitychange'));Object.defineProperty(document,'visibilityState',{configurable:true,get:()=> 'visible'});document.dispatchEvent(new Event('visibilitychange'));});
    await page.getByLabel('Email',{exact:true}).waitFor();assert.equal(await page.getByRole('heading',{name:'Chưa có lớp phù hợp',exact:true}).count(),0);assert.equal(calls.me,afterReturn+2);
    assert.equal(calls.mutations, 0); assert.equal(calls.unexpected, 0);
    const root = path.resolve(__dirname, '../../../docs/plan/session-focus-evidence');
    fs.writeFileSync(path.join(root, 'strict-mode.json'), JSON.stringify({passed:true,realDevMainStrictMode:true,mockHttpOnly:true,focusBurstNoRequests:true,visibleDuplicatesNoRequests:true,filterDraftRetainedOnFocus:true,hiddenClearsFields:true,visibleBurstOneMe:true,identitySignalRechecks:true,revoked401ClearsToLogin:true,...calls}, null, 2));
    process.stdout.write(JSON.stringify({ passed: true, ...calls }));
  } finally { await browser.close(); }
})().catch(error => { process.stderr.write(error.stack); process.exitCode = 1; });
