const assert=require('node:assert/strict');
const fs=require('node:fs');
const {chromium}=require('C:/Users/asus/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const {installShellControls}=require('../../frontend/tests/browser/shell-controls.cjs');
let browser,checkpoint='input';
(async()=>{
 let raw='';for await(const part of process.stdin)raw+=part;
 const accounts=JSON.parse(raw);raw=null;
 const passwordChanges=accounts.filter(a=>a.Temporary).length;
 const focused=process.argv.includes('--focused');
 const details=process.argv.includes('--details');
 browser=await chromium.launch({channel:'msedge',headless:true});
 const results=[],errors=[];
 const directory='C:/Study/SOEN/main_workplace/.local-demo/browser';fs.mkdirSync(directory,{recursive:true});
 const login=async(page,a)=>{
  await page.goto('https://localhost:8444/login');
  await page.getByLabel('Email',{exact:true}).fill(a.Email);await page.getByLabel('Mật khẩu',{exact:true}).fill(a.Password);
  const response=page.waitForResponse(r=>r.url().endsWith('/api/v1/auth/login')&&r.request().method()==='POST');
  await page.getByRole('button',{name:'Đăng nhập',exact:true}).click();assert.equal((await response).status(),200);
 };
 // Complete the real password-change workflow for every synthetic staff account.
 for(const a of accounts.filter(a=>a.Temporary)){
  checkpoint='password-'+a.Email;const context=await browser.newContext({ignoreHTTPSErrors:false});const page=await context.newPage();installShellControls(page);
  await login(page,a);await page.getByLabel('Mật khẩu hiện tại',{exact:true}).fill(a.Password);
  await page.getByLabel('Mật khẩu mới',{exact:true}).fill(a.NextPassword);await page.getByLabel('Nhập lại mật khẩu mới',{exact:true}).fill(a.NextPassword);
  const changed=page.waitForResponse(r=>r.url().endsWith('/api/v1/auth/change-password')&&r.request().method()==='POST');
  await page.getByRole('button',{name:'Đổi mật khẩu',exact:true}).click();assert.equal((await changed).status(),204);
  a.Password=a.NextPassword;a.Temporary=false;
  // Nonsecret progress; the invoking operator already holds the encrypted pending ledger.
  process.stdout.write(JSON.stringify({changed:a.Email})+'\n');await context.close();
 }
 const roleAccounts=['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director','Admin'].map(role=>accounts.find(a=>a.Email===(role==='Student'?'student45@demo.example':role==='Teacher'?'teacher08@demo.example':role.toLowerCase()+'01@demo.example')));
 const paths={Student:['/student/home','/student/invoices','/student/classes','/student/grade-reports'],Admissions:['/admissions/registrations'],AcademicAffairs:['/academics/entry-test-sessions','/academics/courses','/academics/training-programs','/academics/classes','/academics/grade-reviews'],Teacher:['/teacher/classes','/teacher/entry-test-sessions'],Accountant:['/accountant/invoices','/accountant/reports'],Director:['/reports'],Admin:['/admin/accounts','/admin/audit-events']};
 for(const width of (focused?[375]:details?[1280]:[375,768,1280]))for(const a of roleAccounts.filter(a=>details?['Student','Teacher'].includes(a.Role):!focused||['Admissions','Accountant','Teacher'].includes(a.Role))){
  checkpoint=a.Role+'-'+width;const context=await browser.newContext({viewport:{width,height:900},ignoreHTTPSErrors:false});const page=await context.newPage();installShellControls(page);
  page.on('pageerror',()=>errors.push(checkpoint));page.on('response',r=>{if(r.url().includes('/api/')&&r.status()>=500)errors.push(checkpoint+' server-'+r.status());});await login(page,a);await page.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();
  const me=await page.evaluate(async()=>{const r=await fetch('/api/v1/auth/me');return {status:r.status,body:await r.json()};});assert.equal(me.status,200);assert.equal(me.body.role,a.Role);assert.equal(me.body.scope,'NORMAL');
  const casePaths=[...paths[a.Role]];
  if(a.Role==='Student'||a.Role==='Teacher'){
   const path=a.Role==='Student'?'/api/v1/students/me/grade-reports':'/api/v1/academics/teachers/me/classes';
   const data=await page.evaluate(async(path)=>{const r=await fetch(path);return {status:r.status,body:await r.json()};},path);
   assert.equal(data.status,200);assert.ok(data.body.items.length>0);
   if(details){const id=data.body.items[0].id;assert.ok(Number.isInteger(id));casePaths.push(a.Role==='Student'?'/student/grade-reports/'+id:'/teacher/classes/'+id+'/gradebook');if(a.Role==='Teacher')casePaths.push('/teacher/classes/'+id+'/attendance');}
  }
  for(const path of casePaths){
   checkpoint=a.Role+'-'+width+'-'+path;await page.goto('https://localhost:8444'+path);await page.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();
   await page.waitForLoadState('networkidle',{timeout:20000});assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
   assert.equal(await page.getByText('Đã xảy ra lỗi hệ thống.',{exact:true}).count(),0);
  }
  await page.screenshot({path:directory+'/'+a.Role+'-'+width+'.png',fullPage:true});
  const response=page.waitForResponse(r=>r.url().endsWith('/api/v1/auth/logout')&&r.request().method()==='POST');await page.getByRole('button',{name:'Đăng xuất',exact:true}).click();assert.equal((await response).status(),204);await context.close();results.push({role:a.Role,width,paths:casePaths.length});
 }
 assert.deepEqual(errors,[]);
 fs.writeFileSync(directory+(focused?'/summary-focused.json':details?'/summary-details.json':passwordChanges?'/summary.json':'/summary-final.json'),JSON.stringify({success:true,realEdge:true,trustedHttps:true,passwordChanges,cases:results},null,2));
 process.stdout.write(JSON.stringify({success:true,cases:results.length})+'\n');await browser.close();
})().catch(async(error)=>{process.stderr.write('Demo browser refused at '+checkpoint+': '+error.name+' '+error.message.slice(0,1200)+'\n');if(browser)await browser.close();process.exitCode=1;});
