const assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});const rows=[];
 try{for(const width of [375,768,1280]){
  const context=await browser.newContext({viewport:{width,height:900},ignoreHTTPSErrors:false});const page=await context.newPage();let checks=0,writes=0;const errors=[];
  page.on('request',r=>{if(/\/auth\/(me|csrf)$/.test(r.url()))checks++;if(r.method()!=='GET')writes++;});page.on('pageerror',()=>errors.push('page-error'));
  await page.goto('https://localhost:8443/login');await page.getByLabel('Email',{exact:true}).waitFor();await page.getByRole('button',{name:'Đăng nhập',exact:true}).waitFor();
  await page.getByLabel('Email',{exact:true}).fill('preview@example.test');const before=checks;
  await page.evaluate(()=>{for(let i=0;i<20;i++)window.dispatchEvent(new Event('focus'));});await page.waitForTimeout(200);
  assert.equal(checks,before);assert.equal(await page.getByLabel('Email',{exact:true}).inputValue(),'preview@example.test');assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);assert.deepEqual(errors,[]);assert.equal(writes,0);
  rows.push({width,focusBurstChecks:checks-before,fieldRetained:true,writes,httpsTrusted:true,noOverflow:true});await context.close();
 }
 fs.writeFileSync(path.resolve(__dirname,'../../../docs/plan/session-focus-evidence/live-browser.json'),JSON.stringify({passed:true,anonymousOnly:true,rows},null,2));process.stdout.write(JSON.stringify({passed:true,widths:3}));
 }finally{await browser.close();}
})().catch(error=>{process.stderr.write(error.stack);process.exitCode=1;});
