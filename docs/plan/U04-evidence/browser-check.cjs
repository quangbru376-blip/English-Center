const assert=require('node:assert/strict');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const ctx=await browser.newContext();let authority=null,mode='normal',calls=[];const errors=[];
  const proof=(restricted=false)=>({accountId:1,fullName:'Tên kiểm chứng',email:'sample@example.test',role:'Student',scope:restricted?'PASSWORD_CHANGE':'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z'});
  const failure=(route,status,code)=>route.fulfill({status,json:{code,message:'Thông báo kiểm chứng.',fieldErrors:{},traceId:'test-u04'},headers:status===429?{'Retry-After':'60'}:{}});
  await ctx.route('**/api/v1/**',async route=>{
   const req=route.request(),path=new URL(req.url()).pathname;calls.push({path,method:req.method(),body:req.postData()});
   if(path.endsWith('/csrf'))return route.fulfill({json:{requestToken:'synthetic-token'}});
   if(path.endsWith('/me'))return authority?route.fulfill({json:authority}):failure(route,401,'AUTHENTICATION_REQUIRED');
   if(path.endsWith('/login')){
    if(mode==='credentials')return failure(route,401,'AUTHENTICATION_FAILED');
    if(mode==='rate')return failure(route,429,'RATE_LIMITED');
    if(mode==='csrf')return failure(route,403,'CSRF_INVALID');
    if(mode==='lost-login')return route.abort();
    await new Promise(r=>setTimeout(r,80));authority=proof(mode==='restricted');return route.fulfill({json:authority});
   }
   if(path.endsWith('/logout')){
    if(mode==='forbidden')return failure(route,403,'FORBIDDEN');
    if(mode==='lost-logout')return route.abort();
    authority=null;return mode==='expired'?failure(route,401,'AUTHENTICATION_REQUIRED'):route.fulfill({status:204});
   }
   if(path.endsWith('/register'))return route.fulfill({status:201,json:{outcome:'CREATED',message:'Đã tạo tài khoản.'}});
   throw new Error('Unexpected endpoint '+path);
  });
  const page=await ctx.newPage();page.on('pageerror',e=>errors.push(e.message));
  const load=async()=>{authority=null;calls=[];await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session');await page.getByLabel('Email',{exact:true}).waitFor();};
  const login=async()=>{await page.getByLabel('Email',{exact:true}).fill('sample@example.test');await page.getByLabel('Mật khẩu',{exact:true}).fill('  Á nguyên văn  ');await page.getByRole('button',{name:'Đăng nhập',exact:true}).dblclick();};
  const noOverflow=async()=>assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
  for(const width of [375,768,1280]){
   mode='normal';await page.setViewportSize({width,height:900});await load();await login();
   await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();assert.ok(page.url().endsWith('/student/home'));await noOverflow();
   const writes=calls.filter(x=>x.method==='POST');assert.equal(writes.length,1);assert.equal(JSON.parse(writes[0].body).password,'  Á nguyên văn  ');
   assert.equal(await page.locator('input[type=password]').count(),0);
   await page.getByRole('button',{name:'Đăng xuất',exact:true}).click();await page.getByLabel('Email',{exact:true}).waitFor();assert.equal(authority,null);
  }
  for(const kind of ['credentials','rate','csrf','lost-login']){
   mode=kind;await load();await login();
   await page.getByText(kind==='credentials'?'Email hoặc mật khẩu không hợp lệ.':kind==='rate'?'Có quá nhiều yêu cầu. Vui lòng chờ và thử lại chủ đích.':kind==='csrf'?'Ngữ cảnh gửi không hợp lệ. Hãy kiểm tra lại; chưa xác nhận thao tác thành công.':'Chưa xác định kết quả đăng nhập. Hãy kiểm tra phiên; không gửi lại mật khẩu tự động.',{exact:true}).waitFor();
   assert.equal(calls.filter(x=>x.method==='POST').length,1);if(kind!=='lost-login')assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'');
   if(kind==='rate')await page.getByText('Thời gian chờ theo server: 60',{exact:true}).waitFor();
  }
  mode='normal';await load();
  await page.getByRole('button',{name:'Đăng ký tài khoản học viên',exact:true}).click();
  await page.getByLabel('Họ và tên',{exact:true}).waitFor();
  const prepareSignup=page.getByRole('button',{name:'Lấy ngữ cảnh đăng ký',exact:true});if(await prepareSignup.count())await prepareSignup.click();
  for(const [label,value] of [['Họ và tên','Tên mẫu'],['Ngày sinh','2000-01-01'],['Email','sample@example.test'],['Số điện thoại','0901234567'],['Mật khẩu','  Á chuỗi mẫu  ']])await page.getByLabel(label,{exact:true}).fill(value);
  await page.getByRole('button',{name:'Đăng ký tài khoản',exact:true}).click();await page.getByLabel('Email',{exact:true}).waitFor();
  assert.equal(await page.getByLabel('Email',{exact:true}).inputValue(),'');assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'');
  assert.equal(calls.filter(x=>x.method==='POST').length,1);assert.ok(calls.find(x=>x.path.endsWith('/register')));assert.equal(authority,null);
  await page.getByLabel('Email',{exact:true}).focus();await page.keyboard.press('Tab');assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).evaluate(el=>document.activeElement===el),true);
  mode='restricted';await load();await login();await page.getByRole('heading',{name:'Đổi mật khẩu bắt buộc',exact:true}).waitFor();
  assert.ok(page.url().endsWith('/account/change-password'));assert.equal(await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).count(),0);await noOverflow();
  for(const kind of ['forbidden','lost-logout','expired']){
   mode='normal';await load();await login();await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();mode=kind;
   await page.getByRole('button',{name:'Đăng xuất',exact:true}).click();
   if(kind==='forbidden'){await page.getByText('Thông báo kiểm chứng.',{exact:true}).waitFor();assert.ok(authority);assert.equal(await page.getByText('Đã đăng xuất.',{exact:true}).count(),0);}
   else if(kind==='lost-logout'){await page.getByText('Chưa xác nhận đăng xuất tại server. RAM đã xóa; không tự gửi lại.',{exact:true}).waitFor();assert.equal(await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).count(),0);}
   else await page.getByLabel('Email',{exact:true}).waitFor();
   assert.equal(calls.filter(x=>x.path.endsWith('/logout')).length,1);
  }
  // Real same-origin channel across two tabs: receiving revalidates and never echoes.
  mode='normal';await load();await login();await page.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();
  const second=await ctx.newPage();second.on('pageerror',e=>errors.push(e.message));await second.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session');await second.getByRole('heading',{name:'Trang chủ học viên',exact:true}).waitFor();
  const baseline=calls.length;await second.getByRole('button',{name:'Đăng xuất',exact:true}).click();await second.getByLabel('Email',{exact:true}).waitFor();await page.getByLabel('Email',{exact:true}).waitFor();
  const after=calls.length;await page.waitForTimeout(250);assert.equal(calls.length,after);assert.ok(calls.slice(baseline).some(x=>x.path.endsWith('/me')));assert.equal(await page.getByLabel('Email',{exact:true}).inputValue(),'');
  assert.equal(await page.evaluate(()=>localStorage.length+sessionStorage.length),0);assert.deepEqual(errors,[]);
  console.log('U04 Edge 375/768/1280/raw single-send/privacy/CSRF/normal/restricted/logout/cross-tab/no-storage PASS');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
