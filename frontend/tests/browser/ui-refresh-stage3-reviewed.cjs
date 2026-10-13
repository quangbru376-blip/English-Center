// Stage3 capture/state adapter for the real SQL/HTTPS acceptance driver.
// No app routes, request bodies, fixture semantics or assertions are replaced.
const fs = require('node:fs');
const path = require('node:path');
const Module = require('node:module');
const crypto = require('node:crypto');
const evidenceRoot = path.resolve(__dirname, '../../../docs/plan/UI-refresh-evidence');
const root = process.env.EC_UI_CAPTURE_DIR ? path.resolve(process.env.EC_UI_CAPTURE_DIR) : path.join(evidenceRoot, 'before');
if (!root.startsWith(evidenceRoot + path.sep)) throw new Error('Capture output must stay inside task evidence');
fs.mkdirSync(root, { recursive: true });
const original = path.join(__dirname, 'verify-m1.cjs');
let source = fs.readFileSync(original, 'utf8');
function replaceOnce(from, to) {
  if (source.split(from).length !== 2) throw new Error('Capture anchor drift');
  source = source.replace(from, to);
}
replaceOnce(' const errors=[];', ` const errors=[];
 const captureRoles=new WeakMap(),captureRun=require('node:crypto').randomUUID().slice(0,8);let captureIndex=0,studentTour=false;
 const capture=async(p,label)=>{
  const width=p.viewportSize().width;
  const role=captureRoles.get(p)??'Anonymous';
  const panel=p.locator('.ec-account-panel:visible');
  if(await panel.count()){const box=await panel.boundingBox();assert.ok(box.x>=0&&box.x+box.width<=width,'Account panel viewport');}
  const route=new URL(p.url()).pathname;
  const name='g'+f.group+'-'+captureRun+'-'+String(++captureIndex).padStart(3,'0')+'-'+role+'-'+width+'-'+label.replace(/[^a-zA-Z0-9_-]/g,'_').slice(0,100)+'.png';
  const file=require('node:path').join(${JSON.stringify(root)},name);
  await p.screenshot({path:file,fullPage:true,animations:'disabled',maskColor:'#e5f3ef',mask:[p.locator('input[type="password"], [aria-label="Mật khẩu tạm"]')]});
  const headings=await p.locator('h1,h2').allTextContents();
  const digest=require('node:crypto').createHash('sha256').update(require('node:fs').readFileSync(file)).digest('hex');
  require('node:fs').appendFileSync(require('node:path').join(${JSON.stringify(root)},'manifest.jsonl'),JSON.stringify({file:name,group:f.group,role,width,route,label,headings,sha256:digest,synthetic:true})+'\\n');
 };
`);
replaceOnce('await p.getByLabel(\'Email\',{exact:true}).waitFor(); return p;', `await p.getByLabel('Email',{exact:true}).waitFor();
   if(captureIndex===0){const saved=p.viewportSize();for(const width of [375,768,1280]){await p.setViewportSize({width,height:900});await capture(p,'login');}await p.goto(f.origin+'/register');await p.getByLabel('Họ và tên',{exact:true}).waitFor();for(const width of [375,768,1280]){await p.setViewportSize({width,height:900});await capture(p,'signup');}await p.goto(f.origin+'/login');await p.getByLabel('Email',{exact:true}).waitFor();await p.setViewportSize(saved??{width:1280,height:720});}return p;`);
replaceOnce("proof(await api(p,'/api/v1/auth/me'),role,scope);await policy(p,true);", `proof(await api(p,'/api/v1/auth/me'),role,scope);await policy(p,true);
   captureRoles.set(p,role);const savedViewport=p.viewportSize();
   if(scope==='NORMAL'){
    const accountToggle=p.getByRole('button',{name:'Menu tài khoản',exact:true});
    await accountToggle.click();await p.locator('.ec-account-panel').waitFor({state:'visible'});await p.keyboard.press('Escape');assert.equal(await accountToggle.getAttribute('aria-expanded'),'false');assert.equal(await accountToggle.evaluate(e=>e===document.activeElement),true);
    await p.setViewportSize({width:375,height:900});const menuToggle=p.getByRole('button',{name:'Mở menu',exact:true});await menuToggle.click();const drawer=p.getByRole('dialog',{name:'Điều hướng',exact:true});await drawer.waitFor();
    const buttons=drawer.locator('button:not(:disabled)');await buttons.first().focus();await p.keyboard.press('Shift+Tab');assert.equal(await buttons.last().evaluate(e=>e===document.activeElement),true);await p.keyboard.press('Tab');assert.equal(await buttons.first().evaluate(e=>e===document.activeElement),true);
    await capture(p,'drawer-open');await p.keyboard.press('Escape');assert.equal(await menuToggle.getAttribute('aria-expanded'),'false');assert.equal(await menuToggle.evaluate(e=>e===document.activeElement),true);await p.setViewportSize(savedViewport??{width:1280,height:720});
   }
   for(const width of [375,768,1280]){await p.setViewportSize({width,height:900});await capture(p,'landing-'+scope);}
   await p.setViewportSize(savedViewport??{width:1280,height:720});
   if(f.group===85&&role==='Student'&&!studentTour){
    studentTour=true;
    const shot=async label=>{for(const width of [375,768,1280]){await p.setViewportSize({width,height:900});await capture(p,label);}await p.setViewportSize(savedViewport??{width:1280,height:720});};
    let homeReads=0, courseReads=0, writes=0;
    const watch=req=>{if(req.url().includes('/students/me/home'))homeReads++;if(req.url().includes('/students/me/courses'))courseReads++;if(req.url().includes('/students/me/')&&req.method()!=='GET')writes++;};
    p.on('request',watch);
    let loaded=p.waitForResponse(r=>r.url().includes('/students/me/home'));
    await p.goto(f.origin+'/student/home');assert.equal((await loaded).status(),200);await p.getByText(/hồ sơ — trang/).waitFor();await shot('home-auto-loaded');assert.equal(homeReads,1);
    loaded=p.waitForResponse(r=>r.url().includes('/students/me/courses'));
    await p.getByRole('button',{name:'Chọn khóa học',exact:true}).click();assert.equal((await loaded).status(),200);await p.getByText(/khóa học — trang/).waitFor();await shot('courses-auto-loaded');assert.equal(courseReads,1);assert.equal(writes,0);
    const coursesPattern='**/students/me/courses?*';let failures=0;
    await p.route(coursesPattern,route=>{failures++;return route.fulfill({status:503,contentType:'application/json',body:JSON.stringify({code:'SERVICE_NOT_READY',message:'Chưa đọc được khóa học. Thử lại khi dịch vụ sẵn sàng.',fieldErrors:{},traceId:'synthetic-ui-state'})});});
    await p.getByRole('button',{name:'Đọc khóa học đang mở',exact:true}).click();await p.getByText('Chưa đọc được khóa học. Thử lại khi dịch vụ sẵn sàng.',{exact:true}).waitFor();assert.equal(await p.locator('.student-course-card').count(),0);await shot('courses-read-error');assert.equal(failures,1);
    await p.unroute(coursesPattern);
    await p.route(coursesPattern,route=>route.fulfill({status:200,contentType:'application/json',body:JSON.stringify({items:[],page:1,pageSize:20,totalCount:0})}));
    await p.getByRole('button',{name:'Đọc khóa học đang mở',exact:true}).click();await p.getByRole('heading',{name:'Chưa có khóa học đang mở',exact:true}).waitFor();await shot('courses-empty');
    await p.unroute(coursesPattern);
    let unblock;const blocked=new Promise(resolve=>{unblock=resolve;});
    await p.route(coursesPattern,async route=>{await blocked;await route.continue();});
    loaded=p.waitForResponse(r=>r.url().includes('/students/me/courses'));
    await p.getByRole('button',{name:'Đọc khóa học đang mở',exact:true}).click();await p.getByRole('status',{name:'Đang đọc hoặc xử lý yêu cầu học viên…',exact:true}).waitFor();await shot('courses-loading');unblock();assert.equal((await loaded).status(),200);await p.unroute(coursesPattern);await p.locator('.student-course-card').first().waitFor();
    loaded=p.waitForResponse(r=>r.url().endsWith('/students/me/profile'));
    await p.getByRole('button',{name:'Thông tin cá nhân',exact:true}).click();assert.equal((await loaded).status(),200);await p.getByRole('button',{name:'Sửa thông tin cá nhân',exact:true}).waitFor();await shot('profile-auto-read');
    await p.getByRole('button',{name:'Sửa thông tin cá nhân',exact:true}).click();await p.getByLabel('Họ và tên',{exact:true}).fill('Synthetic unsaved draft');await shot('profile-draft');
    const dismiss=async dialog=>{assert.equal(dialog.type(),'confirm');await dialog.dismiss();};p.once('dialog',dismiss);
    await p.getByRole('button',{name:'Chọn khóa học',exact:true}).click();assert.equal(await p.getByLabel('Họ và tên',{exact:true}).inputValue(),'Synthetic unsaved draft');assert.equal(await p.getByRole('heading',{name:'Thông tin cá nhân',exact:true}).count(),1);
    p.once('dialog',async dialog=>dialog.accept());loaded=p.waitForResponse(r=>r.url().includes('/students/me/courses'));await p.getByRole('button',{name:'Chọn khóa học',exact:true}).click();assert.equal((await loaded).status(),200);assert.equal(await p.getByLabel('Họ và tên',{exact:true}).count(),0);assert.equal(writes,0);
    p.off('request',watch);
    require('node:fs').appendFileSync(require('node:path').join(${JSON.stringify(root)},'state-checks.jsonl'),JSON.stringify({group:f.group,widths:[375,768,1280],initialHomeReads:1,initialCourseReads:1,readFailureNoRetry:true,staleCardsCleared:true,empty:true,loading:true,profileDirtyDismissPreserved:true,profileDirtyAcceptCleared:true,mutations:0})+'\\n');
    await p.goto(f.origin+'/student/home');await p.getByRole('button',{name:'Đọc trang chủ',exact:true}).waitFor();
   }`);
replaceOnce('const widthCheck=async p=>assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);', `const widthCheck=async p=>{await capture(p,checkpoint);const fits=await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth);if(!fits){const layout=await p.evaluate(()=>Array.from(document.querySelectorAll('main *')).map(e=>({tag:e.tagName,classes:e.getAttribute('class'),left:e.getBoundingClientRect().left,right:e.getBoundingClientRect().right})).filter(e=>e.left<0||e.right>innerWidth).slice(0,30));require('node:fs').appendFileSync(require('node:path').join(${JSON.stringify(root)},'overflow.jsonl'),JSON.stringify({group:f.group,checkpoint,layout})+'\\n');}assert.equal(fits,true);};`);
const firstOptions = "await p.getByRole('button',{name:'Chọn lịch kiểm tra đầu vào',exact:true}).click();await p.getByRole('radio').first().check();";
replaceOnce(firstOptions, `await p.getByRole('button',{name:'Chọn lịch kiểm tra đầu vào',exact:true}).click();
 await p.getByRole('radio').first().check();
 const optionsCard=p.locator('#student-booking-options');
 await optionsCard.getByLabel('Từ ngày',{exact:true}).fill('2026-10-13');
 await p.getByText('Bộ lọc đã thay đổi. Đọc ca phù hợp trước khi chọn lịch.',{exact:true}).waitFor();
 assert.equal(await p.getByRole('radio').count(),0);assert.equal(await p.getByRole('button',{name:'Xem lại ca đã chọn',exact:true}).isDisabled(),true);await capture(p,'booking-filter-invalidated');
 await optionsCard.getByLabel('Từ ngày',{exact:true}).fill('');
 await p.getByRole('button',{name:'Đọc ca phù hợp',exact:true}).click();await p.getByRole('radio').first().waitFor();assert.equal(await p.getByRole('radio').first().isChecked(),false);await p.getByRole('radio').first().check();`);
const bookingPreview = "await p.getByRole('button',{name:'Xem lại ca đã chọn',exact:true}).click();";
if (source.split(bookingPreview).length !== 3) throw new Error('Booking preview anchor drift');
source = source.replaceAll(bookingPreview, bookingPreview + "await p.getByRole('dialog').waitFor();await capture(p,'booking-options-confirmation');");
const unknownReady = "await p.getByRole('button',{name:'Kiểm tra kết quả',exact:true}).waitFor();";
source = source.replaceAll(unknownReady, unknownReady + "await capture(p,'unknown-manual-lookup');");
// Compiling with the original filename preserves all existing relative modules.
const driver = new Module(original, module);
driver.filename = original;
driver.paths = Module._nodeModulePaths(path.dirname(original));
fs.writeFileSync(path.join(root, 'driver-source-sha256.txt'), crypto.createHash('sha256').update(fs.readFileSync(original)).digest('hex'));
driver._compile(source, original);
