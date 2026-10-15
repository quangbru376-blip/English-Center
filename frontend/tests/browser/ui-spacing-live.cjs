// Anonymous release smoke only: no account credentials or business mutations.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {chromium}=require(process.env.EC_PLAYWRIGHT);
const root=path.resolve(__dirname,'../../../docs/plan/ui-spacing-evidence');
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true}),errors=[],writes=[],results=[];
 try{
  const context=await browser.newContext({ignoreHTTPSErrors:false}),page=await context.newPage();
  page.on('pageerror',e=>errors.push(e.message));
  page.on('request',r=>{if(new URL(r.url()).pathname.startsWith('/api/')&&r.method()!=='GET')writes.push(new URL(r.url()).pathname);});
  const manifest=JSON.parse(fs.readFileSync(path.join(root,'release-files.json'))),css=manifest.find(f=>f.path.endsWith('.css'));
  for(const width of [375,768,1280]){
   await page.setViewportSize({width,height:900});await page.goto('https://localhost:8444/login');
   await page.getByLabel('Email',{exact:true}).waitFor();await page.evaluate(()=>document.fonts.ready);
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
   const response=await page.evaluate(async asset=>{const r=await fetch('/'+asset);return {status:r.status,bytes:Array.from(new Uint8Array(await r.arrayBuffer()))};},css.path);assert.equal(response.status,200);
   assert.equal(crypto.createHash('sha256').update(Buffer.from(response.bytes)).digest('hex'),css.sha256);
   assert.ok((await page.content()).includes(css.path));
   await page.screenshot({path:path.join(root,`live-login-${width}.png`),fullPage:true});results.push({width,noPageOverflow:true,reviewedCssServed:true});
  }
  assert.deepEqual(errors,[]);assert.deepEqual(writes,[]);await context.close();
  fs.writeFileSync(path.join(root,'live-ui.json'),JSON.stringify({passed:true,trustedHttps:true,anonymousOnly:true,apiMutations:0,sessionRevocations:0,errors,cases:results},null,2));
  process.stdout.write(JSON.stringify({passed:true,cases:results.length,apiMutations:0}));
 }finally{await browser.close();}
})().catch(()=>{process.stderr.write('Anonymous live UI verification failed; no request headers recorded.');process.exitCode=1;});
