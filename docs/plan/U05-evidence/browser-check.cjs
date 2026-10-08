const assert=require('node:assert/strict'),{chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{const browser=await chromium.launch({channel:'msedge',headless:true});try{
 const page=await browser.newPage();const errors=[];page.on('pageerror',e=>errors.push(e.message));let mode='ok',scope='NORMAL',active=true,calls=[];
 const proof=()=>({accountId:1,fullName:'Tên mẫu',email:'sample@example.test',role:'Student',scope,accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-08T01:00:00Z',absoluteExpiresAt:'2026-10-08T03:00:00Z',effectiveExpiresAt:'2026-10-08T01:20:00Z'});
 await page.route('**/api/v1/**',async route=>{const req=route.request(),path=new URL(req.url()).pathname;calls.push({path,body:req.postData()});
  if(path.endsWith('/csrf'))return route.fulfill({json:{requestToken:'synthetic'}});
  if(path.endsWith('/me'))return active?route.fulfill({json:proof()}):route.fulfill({status:401,json:{code:'AUTHENTICATION_REQUIRED',message:'Phiên hết hiệu lực',fieldErrors:{},traceId:'test'}});
  if(path.endsWith('/change-password')){await new Promise(r=>setTimeout(r,80));if(mode==='lost')return route.abort();if(mode==='ok'){active=false;return route.fulfill({status:204});}
   const status={wrong:400,csrf:403,conflict:409,server:500}[mode];return route.fulfill({status,json:{code:mode==='csrf'?'CSRF_INVALID':mode==='conflict'?'CONFLICT':'VALIDATION_ERROR',message:'Thông báo kiểm chứng.',fieldErrors:{currentPassword:['Mật khẩu hiện tại không hợp lệ.']},traceId:'test'}});}
  throw new Error('Unexpected '+path);
 });
 const load=async()=>{calls=[];active=true;await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session');if(scope==='NORMAL')await page.getByRole('button',{name:'Đổi mật khẩu',exact:true}).click();await page.getByLabel('Mật khẩu hiện tại',{exact:true}).waitFor();};
 const fill=async()=>{await page.getByLabel('Mật khẩu hiện tại',{exact:true}).fill('  old  ');await page.getByLabel('Mật khẩu mới',{exact:true}).fill('  Á nguyên văn  ');await page.getByLabel('Nhập lại mật khẩu mới',{exact:true}).fill('  Á nguyên văn  ');};
 for(const width of [375,768,1280])for(scope of ['NORMAL','PASSWORD_CHANGE']){mode='ok';await page.setViewportSize({width,height:900});await load();await fill();assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
  await page.getByRole('button',{name:'Đổi mật khẩu',exact:true}).dblclick();await page.getByLabel('Email',{exact:true}).waitFor();const writes=calls.filter(x=>x.body);assert.equal(writes.length,1);assert.deepEqual(JSON.parse(writes[0].body),{currentPassword:'  old  ',newPassword:'  Á nguyên văn  ',expectedVersions:{account:'AAAAAAAAAAE='}});assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'');assert.equal(calls.some(x=>x.path.endsWith('/login')),false);
 }
 scope='NORMAL';for(mode of ['wrong','csrf','conflict','server','lost']){await load();await fill();await page.getByRole('button',{name:'Đổi mật khẩu',exact:true}).click();
  if(mode==='server'||mode==='lost')await page.getByText('Chưa xác định kết quả đổi mật khẩu. Hãy chủ đích đăng nhập bằng mật khẩu mới hoặc liên hệ Admin; không tự gửi lại.',{exact:true}).waitFor();
  else{await page.getByLabel('Mật khẩu hiện tại',{exact:true}).waitFor();await page.waitForTimeout(150);assert.equal(await page.getByLabel('Mật khẩu mới',{exact:true}).inputValue(),'');if(mode==='wrong')assert.equal(await page.getByLabel('Mật khẩu hiện tại',{exact:true}).getAttribute('aria-invalid'),'true');if(mode==='conflict')assert.equal(await page.getByRole('button',{name:'Đổi mật khẩu',exact:true}).isDisabled(),true);}
  assert.equal(calls.filter(x=>x.body).length,1);
 }
 mode='ok';await load();await fill();await page.getByRole('button',{name:'Quay lại',exact:true}).click();await page.getByRole('dialog').waitFor();await page.keyboard.press('Escape');assert.equal(await page.getByLabel('Mật khẩu mới',{exact:true}).inputValue(),'  Á nguyên văn  ');
 await page.getByRole('button',{name:'Quay lại',exact:true}).click();await page.getByRole('button',{name:'Bỏ thay đổi',exact:true}).click();await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();assert.deepEqual(errors,[]);
 console.log('U05 Edge3widths/all scopes/raw one-send/204 login/400/403/409/500/lost/dirty-dialog PASS');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
