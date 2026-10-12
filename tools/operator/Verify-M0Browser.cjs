const assert = require('node:assert/strict');
const {chromium} = require('C:/Users/asus/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const {randomUUID} = require('node:crypto');
let checkpoint='input', browser, failed=false;
process.on('unhandledRejection',()=>{failed=true;});
(async()=>{
 let text='';for await(const chunk of process.stdin){text+=chunk;if(text.length>4096)throw Error();}
 const input=JSON.parse(text);text=null;
 assert.equal(input.email,'admin@englishcenter.local');assert.ok(typeof input.password==='string'&&input.password.length>=32);
 checkpoint='trusted-edge';browser=await chromium.launch({channel:'msedge',headless:true});
 const errors=[];let logins=0,logouts=0,adminId;
 const api=async(page,path)=>page.evaluate(async path=>{const response=await fetch(path,{credentials:'same-origin'});const body=await response.json();return {status:response.status,body};},path);
 const policy=async(page,authenticated)=>{
  const cookies=await page.context().cookies('https://localhost:8443');
  for(const name of ['__Host-ec-csrf',...(authenticated?['__Host-ec-auth']:[])]){
   const selected=cookies.filter(cookie=>cookie.name===name);assert.equal(selected.length,1);
   assert.ok(selected[0].secure&&selected[0].httpOnly&&selected[0].sameSite==='Strict'&&selected[0].path==='/'&&selected[0].domain==='localhost');
  }
  if(!authenticated)assert.equal(cookies.filter(cookie=>cookie.name==='__Host-ec-auth').length,0);
  const clean=await page.evaluate(()=>Object.keys(localStorage).length===0&&Object.keys(sessionStorage).length===0&&!document.cookie.includes('__Host-ec-'));
  assert.equal(clean,true);
 };
 const open=async(width,context)=>{
  const owned=context??await browser.newContext({viewport:{width,height:900},ignoreHTTPSErrors:false});
  const page=await owned.newPage();page.on('pageerror',()=>errors.push('page-error'));
  await page.goto('https://localhost:8443/login');await page.getByLabel('Email',{exact:true}).waitFor();
  assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);return page;
 };
 const login=async page=>{
  await page.getByLabel('Email',{exact:true}).fill(input.email);await page.getByLabel('Mật khẩu',{exact:true}).fill(input.password);
  const response=page.waitForResponse(response=>response.url().endsWith('/api/v1/auth/login')&&response.request().method()==='POST');
  await page.getByRole('button',{name:'Đăng nhập',exact:true}).click();assert.equal((await response).status(),200);logins++;
  await page.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();
  const me=await api(page,'/api/v1/auth/me');assert.equal(me.status,200);assert.equal(me.body.role,'Admin');assert.equal(me.body.scope,'NORMAL');
  assert.equal((await api(page,'/api/v1/academics/training-programs')).status,403);
  assert.equal((await api(page,'/api/v1/admissions/entry-test-sessions')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/home')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/invoices')).status,403);
  assert.equal((await api(page,'/api/v1/admissions/registrations/1')).status,403);
  assert.equal((await api(page,'/api/v1/admissions/teachers/me/entry-test-sessions')).status,403);
  assert.equal((await api(page,'/api/v1/finance/invoices/1')).status,403);
  assert.equal((await api(page,'/api/v1/academics/classes')).status,403);
  assert.equal((await api(page,'/api/v1/academics/schedules')).status,403);
  assert.equal((await api(page,'/api/v1/academics/teachers/me/classes')).status,403);
  assert.equal((await api(page,'/api/v1/academics/teachers/me/classes/1/attendance')).status,403);
  assert.equal((await api(page,'/api/v1/academics/classes/1/attendance')).status,403);
  assert.equal((await api(page,'/api/v1/academics/teachers/me/classes/1/gradebook')).status,403);
  assert.equal((await api(page,'/api/v1/academics/grade-reviews')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/grade-reports')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/grade-reports/1')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/classes')).status,403);
  assert.equal((await api(page,'/api/v1/academics/teachers/me/schedules')).status,403);
  assert.equal((await api(page,'/api/v1/students/me/schedules')).status,403);
  if(adminId===undefined)adminId=me.body.accountId;assert.equal(me.body.accountId,adminId);await policy(page,true);
 };
 const logout=async page=>{
  const response=page.waitForResponse(response=>response.url().endsWith('/api/v1/auth/logout'));
  await page.getByRole('button',{name:'Đăng xuất',exact:true}).click();assert.equal((await response).status(),204);logouts++;
  await page.getByLabel('Email',{exact:true}).waitFor();await policy(page,false);
 };
 for(const width of [375,768,1280]){
  checkpoint='width-'+width;const page=await open(width);await login(page);
  await page.getByLabel('Tìm theo email',{exact:true}).fill(input.email);
  await page.getByRole('button',{name:'Tìm kiếm',exact:true}).click(); await page.getByRole('button',{name:'Xem #'+adminId,exact:true}).click();
  await page.getByRole('button',{name:'Khóa',exact:true}).waitFor();
  assert.equal(await page.getByRole('button',{name:'Khóa',exact:true}).isDisabled(),true);
  assert.equal((await api(page,'/api/v1/admin/command-results/'+randomUUID()+'?action=ADMIN.ACCOUNT.LOCK')).status,404);
  if(width===375){
   checkpoint='tabs';const shared=await page.context().newPage();await shared.goto('https://localhost:8443');await shared.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();
   const separate=await open(width);await login(separate);await logout(page);
   assert.equal((await api(shared,'/api/v1/auth/me')).status,401);assert.equal((await api(separate,'/api/v1/auth/me')).status,200);
   await logout(separate);await separate.context().close();
  }else await logout(page);
  await page.context().close();
 }
 input.password=null;assert.equal(failed,false);assert.equal(errors.length,0);assert.equal(logins,4);assert.equal(logouts,4);
 process.stdout.write(JSON.stringify({phase:'W5-browser',success:true,widths:3,logins,logouts,tlsTrusted:true}));
})().catch(()=>{process.stderr.write('W5-browser refused at '+checkpoint);process.exitCode=1;}).finally(async()=>{if(browser){try{await browser.close();}catch{process.exitCode=1;}}});
