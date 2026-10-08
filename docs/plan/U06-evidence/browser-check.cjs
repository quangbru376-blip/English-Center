const assert=require('node:assert/strict'),{chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{const browser=await chromium.launch({channel:'msedge',headless:true});try{
 const page=await browser.newPage();const errors=[];page.on('pageerror',e=>errors.push(e.message));let mode='ok',role='Admin',scope='NORMAL',calls=[],locked=false;
 const operation='12345678-1234-1234-1234-123456789abc';
 const proof=()=>({accountId:1,fullName:'Người kiểm chứng',email:'actor@example.test',role,scope,accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-08T01:00:00Z',absoluteExpiresAt:'2026-10-08T03:00:00Z',effectiveExpiresAt:'2026-10-08T01:20:00Z'});
 const account=()=>({id:2,fullName:'Tên tài khoản kiểm chứng',email:'target@example.test',phone:null,role:'Student',status:locked?'LOCKED':'ACTIVE',createdAt:'2026-10-08T01:00:00.000Z',mustChangePassword:false,temporaryPasswordExpiresAt:null,version:'AAAAAAAAAAE=',allowedActions:locked?['RESET_PASSWORD','UNLOCK','DISABLE']:['RESET_PASSWORD','LOCK','DISABLE'],disabledReasons:locked?{LOCK:'Đã khóa.',RESTORE:'Không phù hợp.'}:{UNLOCK:'Không phù hợp.',RESTORE:'Không phù hợp.'}});
 await page.route('**/api/v1/**',async route=>{const req=route.request(),path=new URL(req.url()).pathname;calls.push({path,url:req.url(),body:req.postData(),headers:req.headers()});
  if(path.endsWith('/csrf'))return route.fulfill({json:{requestToken:'synthetic'}});
  if(path.endsWith('/me'))return route.fulfill({json:proof()});
  if(path.endsWith('/accounts/2'))return route.fulfill({json:account()});
  if(path.endsWith('/lock')){await new Promise(r=>setTimeout(r,mode==='stale'?400:100));if(mode==='lost')return route.abort();if(mode==='ok'||mode==='stale'){locked=true;return route.fulfill({json:{account:account(),operationId:operation}});}
   return route.fulfill({status:mode==='conflict'?409:500,json:{code:mode==='conflict'?'CONFLICT':'INTERNAL_ERROR',message:'Thông báo kiểm chứng.',fieldErrors:{},traceId:'test'}});}
  if(path.includes('/command-results/')){assert.ok(req.url().endsWith('?action=ADMIN.ACCOUNT.LOCK'));return mode==='completed'?route.fulfill({json:{outcome:'COMPLETED',operationId:operation,resultMode:'SAFE_REFERENCE',target:{accountId:2}}}):route.fulfill({status:404,json:{code:'RESULT_NOT_CONFIRMED',message:'Chưa xác nhận',fieldErrors:{},traceId:'test'}});}
  throw new Error('Unexpected '+path);
 });
 const load=async(direct=false)=>{calls=[];locked=false;await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session'+(direct?'&path=/admin/accounts/2':''));await page.getByLabel('ID tài khoản',{exact:true}).waitFor();if(!direct){await page.getByLabel('ID tài khoản',{exact:true}).fill('2');await page.getByRole('button',{name:'Xem tài khoản',exact:true}).click();}await page.getByRole('button',{name:'Khóa tài khoản',exact:true}).waitFor();};
 const confirm=async()=>{await page.getByRole('button',{name:'Khóa tài khoản',exact:true}).click();await page.getByRole('dialog').waitFor();await page.getByRole('button',{name:'Xác nhận khóa',exact:true}).dblclick();};
 for(const width of [375,768,1280]){mode='ok';await page.setViewportSize({width,height:900});await load(width===768);assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
  await page.getByRole('button',{name:'Khóa tài khoản',exact:true}).click();await page.keyboard.press('Escape');assert.equal(calls.some(x=>x.body),false);await confirm();
  await page.getByText('Server đã xác nhận thao tác. Dữ liệu đang hiển thị là trạng thái hiện tại.',{exact:true}).waitFor();const writes=calls.filter(x=>x.body);assert.equal(writes.length,1);assert.equal(writes[0].path,'/api/v1/admin/accounts/2/lock');assert.deepEqual(JSON.parse(writes[0].body),{expectedVersions:{account:'AAAAAAAAAAE='}});assert.match(writes[0].headers['idempotency-key'],/^[0-9a-f-]{36}$/);assert.equal(await page.getByRole('button',{name:'Khóa tài khoản',exact:true}).isDisabled(),true);
  assert.equal(await page.getByRole('button',{name:/Mở khóa|Reset|Ngừng sử dụng|Khôi phục/}).count(),0);
 }
 for(mode of ['conflict','server','lost']){await load();await confirm();await page.getByRole('button',{name:'Tải lại tài khoản',exact:true}).waitFor();assert.equal(calls.filter(x=>x.body).length,1);
  if(mode==='conflict'){await page.getByText('Dữ liệu hoặc điều kiện đã thay đổi. Tải lại rồi xác nhận intent mới; không tự gửi lại.',{exact:true}).waitFor();}
  else{await page.getByRole('button',{name:'Kiểm tra kết quả khóa',exact:true}).click();await page.getByText('Chưa xác nhận được kết quả. Điều này không chứng minh lệnh đã rollback; hãy đối chiếu trạng thái.',{exact:true}).waitFor();assert.equal(calls.filter(x=>x.path.includes('command-results')).length,1);await page.waitForTimeout(150);assert.equal(calls.filter(x=>x.body).length,1);mode='completed';await page.getByRole('button',{name:'Kiểm tra kết quả khóa',exact:true}).click();await page.getByText('Có completion đã xác nhận. Tải lại tài khoản để xem trạng thái hiện tại.',{exact:true}).waitFor();}
 }
 mode='stale';await load();await confirm();role='Teacher';await page.evaluate(()=>{const c=new BroadcastChannel('ec-identity-v1');c.postMessage('IDENTITY_CHANGED');c.close();});
 await page.getByRole('heading',{name:'Lớp và lịch dạy',exact:true}).waitFor();await page.waitForTimeout(450);assert.equal(await page.getByText('target@example.test',{exact:true}).count(),0);assert.equal(calls.filter(x=>x.body).length,1);
 for(role of ['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director']){calls=[];await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session&path=/admin/accounts/2');await page.waitForTimeout(180);assert.equal(calls.some(x=>x.path.includes('/admin/')),false);assert.equal(await page.getByLabel('ID tài khoản',{exact:true}).count(),0);}
 role='Admin';scope='PASSWORD_CHANGE';calls=[];await page.goto(process.env.EC_UI_URL+'/tests/identity-fixture.html?session&path=/admin/accounts/2');await page.getByLabel('Mật khẩu hiện tại',{exact:true}).waitFor();assert.equal(calls.some(x=>x.path.includes('/admin/')),false);
 assert.deepEqual(errors,[]);assert.equal(await page.evaluate(()=>localStorage.length+sessionStorage.length),0);console.log('U06 Edge3widths/detail/dialog/Escape/single UUID write/409/500/lost/manual lookup404+200/role+scope gates PASS');
}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
