const assert = require('node:assert/strict');
const https = require('node:https'), http = require('node:http');
const { X509Certificate, createHash, randomUUID } = require('node:crypto');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
const { assertBrowserPolicy } = require('./cookie-policy.cjs');
let checkpoint = 'input', server, asyncFailure=false; process.on('unhandledRejection',()=>{asyncFailure=true;});
const input = async () => { let text = ''; for await (const chunk of process.stdin) text += chunk; return JSON.parse(text); };
(async () => {
 const f = await input(), roles = ['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director','Admin'];
 const cert = new X509Certificate(f.certificate);
 assert.equal(createHash('sha256').update(cert.raw).digest('hex').toUpperCase(), f.fingerprint);
 assert.equal(createHash('sha256').update(cert.publicKey.export({type:'spki',format:'der'})).digest('base64'), f.pin);
 const rawRequest = (path, method='GET', body, headers={}) => new Promise((resolve,reject) => {
  const req = https.request(f.origin+path, {method,ca:f.certificate,headers}, res => { let text=''; res.on('data',c=>text+=c); res.on('end',()=>resolve({status:res.statusCode,body:text&&res.headers['content-type']?.includes('application/json')?JSON.parse(text):null})); });
  req.on('error',reject); if(body) req.write(JSON.stringify(body)); req.end();
 });
 server=https.createServer({cert:f.certificate,key:f.privateKey},(req,res)=>{
  if(req.headers.host!==new URL(f.origin).host){res.writeHead(400);res.end();return;}
  const headers={...req.headers}; for(const name of Object.keys(headers))if(name==='forwarded'||name.startsWith('x-forwarded-')||name==='x-v01-bridge')delete headers[name];
  headers['x-v01-bridge']=f.bridge;
  const upstream=http.request(f.backend+req.url,{method:req.method,headers},reply=>{if(f.group===9&&req.url.endsWith('/lock')){reply.resume();reply.on('end',()=>res.destroy());}else{res.writeHead(reply.statusCode,reply.headers);reply.pipe(res);}});
  upstream.on('error',()=>{if(!res.headersSent)res.writeHead(502);res.end();});req.on('aborted',()=>upstream.destroy());req.pipe(upstream);
 });
 await new Promise((resolve,reject)=>{server.once('error',reject);server.listen(Number(new URL(f.origin).port),'127.0.0.1',resolve);});
 checkpoint='tls-certificate'; assert.equal((await rawRequest('/')).status,200);
 checkpoint='bridge-negative'; const direct=await new Promise((resolve,reject)=>{http.get(f.backend,res=>{res.resume();resolve(res.statusCode);}).on('error',reject);});assert.equal(direct,400);
 checkpoint='tls-negative';
 const untrusted = await chromium.launch({channel:'msedge',headless:true});
 try { const p=await untrusted.newPage(); let rejected=false; try { await p.goto(f.origin,{timeout:15000}); } catch(e) { rejected=String(e).includes('ERR_CERT_AUTHORITY_INVALID'); } assert.ok(rejected); }
 finally { await untrusted.close(); }
 checkpoint='tls-wrong-pin';
 const wrongPin=await chromium.launch({channel:'msedge',headless:true,args:['--ignore-certificate-errors-spki-list='+createHash('sha256').update('wrong fixture pin').digest('base64')]});
 try{const p=await wrongPin.newPage();let rejected=false;try{await p.goto(f.origin,{timeout:15000});}catch(e){rejected=String(e).includes('ERR_CERT_AUTHORITY_INVALID');}assert.ok(rejected);}finally{await wrongPin.close();}
 checkpoint='tls-pinned';
 const browser = await chromium.launch({channel:'msedge',headless:true,args:['--ignore-certificate-errors-spki-list='+f.pin]});
 const errors=[];
 try {
  const newPage = async(context) => { const ctx=context??await browser.newContext(); const p=await ctx.newPage(); p.on('pageerror',()=>errors.push('page-error')); await p.goto(f.origin+'/login'); await p.getByLabel('Email',{exact:true}).waitFor(); return p; };
  const api = async(p,path,method='GET',body,token,key) => {checkpoint='group-'+f.group+'-api-'+method+'-'+path.split('?')[0].replace(/\d+/g,'N'); const result=await p.evaluate(async ({path,method,body,token,key}) => {
   const headers={}; if(body)headers['Content-Type']='application/json'; if(token)headers['X-CSRF-Token']=token; if(key)headers['Idempotency-Key']=key;
   const res=await fetch(path,{method,headers,credentials:'same-origin',body:body?JSON.stringify(body):undefined});const text=await res.text();return {status:res.status,body:text?JSON.parse(text):null};
  },{path,method,body,token,key}); checkpoint+='-status-'+result.status;return result; };
  const csrf = async p => {const r=await api(p,'/api/v1/auth/csrf');assert.equal(r.status,200);return r.body.requestToken;};
  const policy = async(p,authenticated) => {
   const cookies=(await p.context().cookies(f.origin)).map(({name,domain,path,secure,httpOnly,sameSite})=>({name,domain,path,secure,httpOnly,sameSite}));
   const state=await p.evaluate(()=>({javascriptCookieNames:document.cookie.split(';').map(c=>c.split('=')[0].trim()).filter(Boolean),localStorageKeys:Object.keys(localStorage),sessionStorageKeys:Object.keys(sessionStorage)}));
   checkpoint='group-'+f.group+'-cookie-policy';assertBrowserPolicy({cookies,...state},authenticated);
  };
  const proof = (r,role,scope='NORMAL') => {assert.equal(r.status,200);assert.deepEqual(Object.keys(r.body).sort(),['accountId','accountVersion','absoluteExpiresAt','effectiveExpiresAt','email','fullName','role','scope','serverTime'].sort());assert.equal(r.body.accountId,f.accounts[role]);assert.equal(r.body.role,role);assert.equal(r.body.scope,scope);};
  const login = async(p,role,password=f.password,scope='NORMAL') => {
   await p.getByLabel('Email',{exact:true}).fill(role.toLowerCase()+'@example.invalid');await p.getByLabel('Mật khẩu',{exact:true}).fill(password);
   const response=p.waitForResponse(r=>r.url().endsWith('/api/v1/auth/login')&&r.request().method()==='POST');await p.getByRole('button',{name:'Đăng nhập',exact:true}).click();assert.equal((await response).status(),200);
   if(scope==='PASSWORD_CHANGE')await p.getByLabel('Mật khẩu hiện tại',{exact:true}).waitFor();else await p.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();
   proof(await api(p,'/api/v1/auth/me'),role,scope);await policy(p,true);
  };
  const logout = async p => {const response=p.waitForResponse(r=>r.url().endsWith('/api/v1/auth/logout'));await p.getByRole('button',{name:'Đăng xuất',exact:true}).click();assert.equal((await response).status(),204);await p.getByLabel('Email',{exact:true}).waitFor();await policy(p,false);};
  const passwordBody = async p => ({currentPassword:f.password,newPassword:f.password+'Z',expectedVersions:{account:(await api(p,'/api/v1/auth/me')).body.accountVersion}});
  const denyAdmin = async p => {assert.equal((await api(p,'/api/v1/admin/accounts/'+f.accounts.Teacher)).status,403);assert.equal((await api(p,'/api/v1/admin/accounts/'+f.accounts.Teacher+'/lock','POST',{expectedVersions:{account:'AAAAAAAAAAE='}},await csrf(p),randomUUID())).status,403);};
  checkpoint='group-'+f.group;

  const widthCheck=async p=>assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
  if(f.group===81){
    const target=await newPage();await login(target,'Teacher');
    for(const width of [375,768,1280]){
      checkpoint='staff-width-'+width;const p=await newPage();await p.setViewportSize({width,height:900});await login(p,'Admin');
      await p.getByRole('button',{name:'Thêm nhân sự',exact:true}).click();await p.getByLabel('Họ và tên',{exact:true}).fill('Nhân sự kiểm chứng '+width);await p.getByLabel('Email đăng nhập',{exact:true}).fill('staff'+width+'@example.invalid');
      const created=p.waitForResponse(r=>r.url().endsWith('/staff-accounts'));await p.getByRole('button',{name:'Cấp tài khoản',exact:true}).click();assert.equal((await created).status(),201);
      await p.getByRole('dialog',{name:'Bàn giao mật khẩu tạm một lần'}).waitFor();await widthCheck(p);
      await p.getByRole('button',{name:'Đã bàn giao — đóng',exact:true}).click();assert.equal(await p.getByLabel('Mật khẩu tạm',{exact:true}).textContent(),'');
      if(width===375){
        checkpoint='staff-actions';await p.getByLabel('Tìm theo email',{exact:true}).fill('teacher@example.invalid');
        const search=p.waitForResponse(r=>r.url().endsWith('/accounts/search'));await p.getByRole('button',{name:'Tìm kiếm',exact:true}).click();assert.equal((await search).status(),200);
        await p.getByRole('button',{name:'Xem #'+f.accounts.Teacher,exact:true}).click();
        for(const [label,path] of [['Khóa','lock'],['Mở khóa','unlock'],['Ngừng sử dụng','disable'],['Kích hoạt lại','restore'],['Đặt lại mật khẩu','reset-password']]){
          checkpoint='staff-'+path;await p.getByRole('button',{name:label,exact:true}).click();const response=p.waitForResponse(r=>r.url().endsWith('/'+path));await p.getByRole('dialog').getByRole('button',{name:'Xác nhận',exact:true}).click();assert.equal((await response).status(),200);
          if(path==='reset-password'){await p.getByRole('dialog',{name:'Bàn giao mật khẩu tạm một lần'}).waitFor();await p.getByRole('button',{name:'Đã bàn giao — đóng',exact:true}).click();}
        }
        assert.equal((await api(target,'/api/v1/auth/me')).status,401);
      }
      await widthCheck(p);await policy(p,true);await logout(p);await p.context().close();
    }
    await target.context().close();
  }else if(f.group===82){
    for(const width of [375,768,1280]){
      checkpoint='course-width-'+width;const p=await newPage();await p.setViewportSize({width,height:900});await login(p,'AcademicAffairs');await p.getByRole('button',{name:'Khóa học và Band',exact:true}).click();
      await p.getByRole('button',{name:'Xem bộ Band',exact:true}).click();await p.getByText('Band5: từ 85.00 điểm',{exact:true}).waitFor();
      await p.getByRole('button',{name:'Thêm khóa học',exact:true}).click();await p.getByLabel('Mã khóa học',{exact:true}).last().fill('IELTS_'+width);await p.getByLabel('Tên khóa học',{exact:true}).last().fill('IELTS '+width);await p.getByLabel('Học phí cơ bản (VND)',{exact:true}).fill('999999999999999999');
      const create=p.waitForResponse(r=>r.url().endsWith('/academics/courses')&&r.request().method()==='POST');await p.getByRole('button',{name:'Tạo mới',exact:true}).click();assert.equal((await create).status(),201);
      await p.getByRole('button',{name:'Sửa tên/học phí',exact:true}).click();await p.getByLabel('Tên khóa học',{exact:true}).last().fill('IELTS đã cập nhật '+width);await p.getByLabel('Học phí cơ bản (VND)',{exact:true}).fill('123456789012345678');
      const save=p.waitForResponse(r=>r.url().endsWith('/details'));await p.getByRole('button',{name:'Lưu',exact:true}).click();assert.equal((await save).status(),200);
      for(const [label,path] of [['Ngừng sử dụng','deactivate'],['Kích hoạt lại','activate']]){
        const button=p.getByRole('button',{name:label,exact:true});await button.click();const response=p.waitForResponse(r=>r.url().endsWith('/'+path));await p.getByRole('dialog').getByRole('button',{name:'Xác nhận',exact:true}).click();assert.equal((await response).status(),200);
      }
      await widthCheck(p);await p.getByRole('button',{name:'Tìm khóa học',exact:true}).click();await p.getByRole('button',{name:'Xem IELTS_'+width,exact:true}).waitFor();await policy(p,true);await logout(p);await p.context().close();
    }
  }else throw new Error('Invalid M1 group');
  assert.equal(asyncFailure,false);assert.deepEqual(errors,[]);process.stdout.write(JSON.stringify({group:f.group,tlsPinned:true,passed:true}));
 }finally{await browser.close();}
})().catch(e=>{const code=/^[A-Z_]+$/.test(e.code??'')?e.code:'ASSERTION';const scalar=v=>typeof v==='number'||typeof v==='boolean'?String(v):'redacted';process.stderr.write('V01-D failed at '+checkpoint+' '+code+' actual='+scalar(e.actual)+' expected='+scalar(e.expected));process.exitCode=1;}).finally(async()=>{if(server){server.closeAllConnections();await new Promise(resolve=>server.close(resolve));}});
