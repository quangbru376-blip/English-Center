const assert = require('node:assert/strict');
const https = require('node:https'), http = require('node:http');
const { X509Certificate, createHash, randomUUID } = require('node:crypto');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
const { assertBrowserPolicy } = require('./cookie-policy.cjs');
const { installShellControls } = require('./shell-controls.cjs');
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
  const newPage = async(context) => { const ctx=context??await browser.newContext(); const p=await ctx.newPage(); installShellControls(p); p.on('pageerror',()=>errors.push('page-error')); await p.goto(f.origin+'/login'); await p.getByLabel('Email',{exact:true}).waitFor(); return p; };
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
   if(scope==='NORMAL'){const menu=p.getByRole('button',{name:'Menu tài khoản',exact:true});if(await menu.getAttribute('aria-expanded')==='true')await menu.click();}
   proof(await api(p,'/api/v1/auth/me'),role,scope);await policy(p,true);
  };
  const logout = async p => {const response=p.waitForResponse(r=>r.url().endsWith('/api/v1/auth/logout'));await p.getByRole('button',{name:'Đăng xuất',exact:true}).click();assert.equal((await response).status(),204);await p.getByLabel('Email',{exact:true}).waitFor();await policy(p,false);};
  const passwordBody = async p => ({currentPassword:f.password,newPassword:f.password+'Z',expectedVersions:{account:(await api(p,'/api/v1/auth/me')).body.accountVersion}});
  const denyAdmin = async p => {assert.equal((await api(p,'/api/v1/admin/accounts/'+f.accounts.Teacher)).status,403);assert.equal((await api(p,'/api/v1/admin/accounts/'+f.accounts.Teacher+'/lock','POST',{expectedVersions:{account:'AAAAAAAAAAE='}},await csrf(p),randomUUID())).status,403);};
  checkpoint='group-'+f.group;
  if(f.group===1){
   for(const width of [375,768,1280]){
    const p=await newPage();await p.setViewportSize({width,height:900});await p.goto(f.origin+'/register');await p.getByLabel('Họ và tên',{exact:true}).fill('Synthetic browser');await p.getByLabel('Ngày sinh',{exact:true}).fill('2000-01-01');await p.getByLabel('Email',{exact:true}).fill('signup'+width+'@example.invalid');await p.getByLabel('Số điện thoại',{exact:true}).fill('091234'+String(width).padStart(4,'0'));await p.getByLabel('Mật khẩu',{exact:true}).fill(f.password);
    assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
    const response=p.waitForResponse(r=>r.url().endsWith('/api/v1/auth/register'));await p.getByRole('button',{name:'Đăng ký tài khoản',exact:true}).click();assert.equal((await response).status(),201);await p.getByLabel('Email',{exact:true}).waitFor();await policy(p,false);assert.equal((await api(p,'/api/v1/auth/me')).status,401);await p.context().close();
   }
  }else if(f.group===2){for(const role of roles){const p=await newPage();await login(p,role);await p.context().close();}}
  else if(f.group===3){
   const p=await newPage();await login(p,'Teacher');const body=await passwordBody(p);assert.equal((await api(p,'/api/v1/auth/change-password','POST',body)).status,403);
   const cookies=(await p.context().cookies(f.origin)).map(c=>c.name+'='+c.value).join('; ');
   assert.equal((await rawRequest('/api/v1/auth/change-password','POST',body,{'Content-Type':'application/json',Cookie:cookies,Origin:'https://wrong.invalid','X-CSRF-Token':await csrf(p)})).status,403);proof(await api(p,'/api/v1/auth/me'),'Teacher');
  }else if(f.group===4){
   const p=await newPage();await login(p,'Teacher');const same=await p.context().newPage();await same.goto(f.origin);await same.getByRole('button',{name:'Đăng xuất',exact:true}).waitFor();const other=await newPage();await login(other,'Teacher');await logout(p);assert.equal((await api(same,'/api/v1/auth/me')).status,401);await same.reload();await same.getByLabel('Email',{exact:true}).waitFor();proof(await api(other,'/api/v1/auth/me'),'Teacher');await policy(same,false);
  }else if(f.group===5||f.group===6){
   const restricted=f.group===6, scope=restricted?'PASSWORD_CHANGE':'NORMAL';const p=await newPage();await login(p,'Teacher',f.password,scope);const other=await newPage();await login(other,'Teacher',f.password,scope);
   if(restricted)await denyAdmin(p);else await p.getByRole('button',{name:'Đổi mật khẩu',exact:true}).click();
   await p.getByLabel('Mật khẩu hiện tại',{exact:true}).fill(f.password);await p.getByLabel('Mật khẩu mới',{exact:true}).fill(f.password+'Z');await p.getByLabel('Nhập lại mật khẩu mới',{exact:true}).fill(f.password+'Z');const response=p.waitForResponse(r=>r.url().endsWith('/api/v1/auth/change-password'));await p.getByRole('button',{name:'Đổi mật khẩu',exact:true}).click();assert.equal((await response).status(),204);await p.getByLabel('Email',{exact:true}).waitFor();await policy(p,false);assert.equal((await api(other,'/api/v1/auth/me')).status,401);const deliberate=await newPage();await login(deliberate,'Teacher',f.password+'Z');
  }else if(f.group===7){
   const p=await newPage();await login(p,'Teacher');const body=await passwordBody(p);assert.equal((await api(p,'/api/v1/auth/change-password','POST',{...body,currentPassword:f.password+'wrong'},await csrf(p))).status,400);assert.equal((await api(p,'/api/v1/auth/change-password','POST',{...body,expectedVersions:{account:'AAAAAAAAAAA='}},await csrf(p))).status,409);proof(await api(p,'/api/v1/auth/me'),'Teacher');
  }else if(f.group===8||f.group===9){
   const target=await newPage();await login(target,'Teacher');const p=await newPage();await login(p,'Admin');await p.getByLabel('Tìm theo email',{exact:true}).fill('teacher@example.invalid');await p.getByRole('button',{name:'Tìm kiếm',exact:true}).click();await p.getByRole('button',{name:'Xem #'+f.accounts.Teacher,exact:true}).click();await p.getByRole('button',{name:'Khóa',exact:true}).waitFor();let writes=0,key,body;
   p.on('request',r=>{if(r.url().endsWith('/lock')){writes++;key=r.headers()['idempotency-key'];body=r.postDataJSON();}});

   await p.getByRole('button',{name:'Khóa',exact:true}).click();await p.getByRole('dialog').waitFor();await p.getByRole('button',{name:'Xác nhận',exact:true}).click();
   if(f.group===8){await p.getByRole('button',{name:'Mở khóa',exact:true}).waitFor();assert.equal(writes,1);const replay=await api(p,'/api/v1/admin/accounts/'+f.accounts.Teacher+'/lock','POST',body,await csrf(p),key);assert.equal(replay.status,200);assert.equal(replay.body.account.status,'LOCKED');}
   else {await p.getByRole('button',{name:'Kiểm tra kết quả',exact:true}).waitFor();assert.equal(writes,1);await p.getByRole('button',{name:'Kiểm tra kết quả',exact:true}).click();await p.getByText('Đã có completion. Tải lại để xem trạng thái; mật khẩu tạm không được trả lại.',{exact:true}).waitFor();assert.equal(writes,1);}
   const lookup=await api(p,'/api/v1/admin/command-results/'+key+'?action=ADMIN.ACCOUNT.LOCK');assert.equal(lookup.status,200);assert.deepEqual(Object.keys(lookup.body).sort(),['outcome','operationId','resultMode','target'].sort());assert.equal(lookup.body.outcome,'COMPLETED');assert.deepEqual(lookup.body.target,{accountId:f.accounts.Teacher});assert.equal((await api(p,'/api/v1/admin/command-results/'+randomUUID()+'?action=ADMIN.ACCOUNT.LOCK')).status,404);assert.equal((await api(target,'/api/v1/auth/me')).status,401);
  }else if(f.group===10){for(const role of roles.filter(r=>r!=='Admin')){const p=await newPage();await login(p,role);await denyAdmin(p);await policy(p,true);await p.context().close();}}
  else throw new Error('Invalid group');
  assert.equal(asyncFailure,false);assert.deepEqual(errors,[]);process.stdout.write(JSON.stringify({group:f.group,tlsPinned:true,passed:true}));
 }finally{await browser.close();}
})().catch(e=>{const code=/^[A-Z_]+$/.test(e.code??'')?e.code:'ASSERTION';const scalar=v=>typeof v==='number'||typeof v==='boolean'?String(v):'redacted';process.stderr.write('V01-D failed at '+checkpoint+' '+code+' actual='+scalar(e.actual)+' expected='+scalar(e.expected));process.exitCode=1;}).finally(async()=>{if(server){server.closeAllConnections();await new Promise(resolve=>server.close(resolve));}});
