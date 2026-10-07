const assert=require('node:assert/strict');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();const pageErrors=[];page.on('pageerror',e=>pageErrors.push(e.message));
  let mode='created',calls=[];
  await page.route('**/api/v1/**',async route=>{
   const request=route.request();calls.push({path:new URL(request.url()).pathname,body:request.postData()});
   if(request.url().endsWith('/csrf'))return route.fulfill({json:{requestToken:'synthetic-token'}});
   if(mode==='lost')return route.abort();
   if(mode==='created'||mode==='stale'){await new Promise(resolve=>setTimeout(resolve,mode==='stale'?500:100));return route.fulfill({status:201,json:{outcome:'CREATED',message:'Đã tạo tài khoản.'}});}
   const status={validation:400,conflict:409,csrf:403,server:500}[mode];
   return route.fulfill({status,json:{code:mode==='csrf'?'CSRF_INVALID':mode==='conflict'?'CONFLICT':'VALIDATION_ERROR',message:'Thông báo kiểm chứng.',fieldErrors:{email:['Email minh họa không hợp lệ.']},traceId:'test-u03'}});
  });
  const load=async()=>{calls=[];await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html');await page.getByRole('button',{name:'Lấy ngữ cảnh đăng ký'}).click();await page.getByLabel('Họ và tên',{exact:true}).fill('Tên mẫu');await page.getByLabel('Ngày sinh',{exact:true}).fill('2000-01-01');await page.getByLabel('Email',{exact:true}).fill('sample@example.test');await page.getByLabel('Số điện thoại',{exact:true}).fill('0901234567');await page.getByLabel('Mật khẩu',{exact:true}).fill('  Á chuỗi mẫu  ');};
  for(const width of [375,768,1280]){
   await page.setViewportSize({width,height:900});mode='created';await load();
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
   assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'  Á chuỗi mẫu  ');
   await page.getByRole('button',{name:'Đăng ký tài khoản',exact:true}).dblclick();
   await page.getByRole('heading',{name:'Đăng nhập — điểm chuyển sau đăng ký'}).waitFor();
   assert.equal(calls.filter(c=>c.path.endsWith('/register')).length,1);assert.equal(calls.length,2);
   assert.equal(JSON.parse(calls[1].body).password,'  Á chuỗi mẫu  ');
  }
  for(const failure of ['validation','conflict','csrf','server','lost']){
   mode=failure;await load();await page.getByRole('button',{name:'Đăng ký tài khoản',exact:true}).click();
   await page.getByRole('alert').waitFor();assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'');
   assert.equal(calls.filter(c=>c.path.endsWith('/register')).length,1);
   if(failure==='validation')assert.equal(await page.getByLabel('Email',{exact:true}).getAttribute('aria-invalid'),'true');
   if(failure==='server'||failure==='lost')assert.match(await page.getByRole('alert').innerText(),/Chưa xác định/);
   if(failure==='csrf')assert.equal(await page.getByRole('button',{name:'Lấy ngữ cảnh đăng ký'}).count(),1);
  }
  mode='stale';await load();await page.getByRole('button',{name:'Đăng ký tài khoản',exact:true}).click();
  await page.getByRole('button',{name:'Đổi ngữ cảnh kiểm chứng'}).click();
  await page.getByRole('alert').waitFor();await page.waitForTimeout(600);
  assert.equal(await page.getByRole('heading',{name:'Đăng ký tài khoản học viên'}).count(),1);
  assert.equal(await page.getByLabel('Họ và tên',{exact:true}).inputValue(),'');
  assert.equal(await page.getByLabel('Mật khẩu',{exact:true}).inputValue(),'');
  assert.deepEqual(pageErrors,[]);console.log('U03 browser PASS:375/768/1280, raw password, one send,201→login/no autologin,400/409/403/500/lost, stale response/RAM cleanup, no overflow/errors.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
