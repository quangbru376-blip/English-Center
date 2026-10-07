const assert = require('node:assert/strict');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
(async () => {
 const browser = await chromium.launch({channel:'msedge',headless:true});
 try {
  const page = await browser.newPage();
  const errors=[]; page.on('pageerror', e=>errors.push(e.message));
  for(const width of [375,768,1280]) {
   await page.setViewportSize({width,height:900});
   await page.goto(process.env.EC_UI_URL + '/tests/ui-fixture.html');
   await page.getByRole('heading',{name:'Kiểm chứng component U02'}).waitFor();
   assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
   assert.equal(await page.getByLabel('Chuỗi nguyên văn').inputValue(),'  Á thử chuỗi  ');
   for (const control of [page.getByLabel('Tên hiển thị'),page.getByLabel('Lựa chọn mẫu'),page.getByRole('button',{name:'Mở hộp thoại'})]) {
    assert.ok(await control.evaluate(e=>e.getBoundingClientRect().height)>=44,'minimum control height');
   }
   const colors=await page.evaluate(()=>{
    const style=getComputedStyle(document.documentElement);
    return Object.fromEntries(['primary','text','muted','danger','success','border','surface','background'].map(k=>[k,style.getPropertyValue('--ec-'+k).trim()]));
   });
   const luminance=hex=>hex.match(/[a-f\d]{2}/gi).map(v=>parseInt(v,16)/255).map(v=>v<=0.04045?v/12.92:((v+0.055)/1.055)**2.4).reduce((sum,v,i)=>sum+v*[0.2126,0.7152,0.0722][i],0);
   const contrast=(a,b)=>{const values=[luminance(a),luminance(b)].sort((a,b)=>b-a);return (values[0]+0.05)/(values[1]+0.05);};
   for(const token of ['primary','text','muted','danger','success'])assert.ok(contrast(colors[token],colors.surface)>=4.5,token+' contrast');
   assert.ok(contrast(colors.border,colors.surface)>=3,'control boundary contrast');
   await page.getByRole('button',{name:'Mở hộp thoại'}).click();
   const dialog=page.getByRole('dialog'); await dialog.waitFor();
   for(let i=0;i<5;i++) { await page.keyboard.press('Tab'); assert.equal(await page.evaluate(()=>document.querySelector('dialog').contains(document.activeElement)),true); }
   await page.keyboard.press('Escape'); await dialog.waitFor({state:'hidden'});
   assert.equal(await page.getByRole('button',{name:'Mở hộp thoại'}).evaluate(e=>e===document.activeElement),true);
   await page.getByRole('button',{name:'Mở hộp thoại'}).click();
   await page.mouse.click(1,1); await dialog.waitFor({state:'hidden'});
  }
  await page.emulateMedia({reducedMotion:'reduce'});
  assert.equal(await page.getByRole('button',{name:'Mở hộp thoại'}).evaluate(e=>getComputedStyle(e).transitionDuration),'1e-05s');
  await page.goto(process.env.EC_UI_URL + '/');
  await page.keyboard.press('Tab'); assert.equal(await page.locator('.skip-link').evaluate(e=>e===document.activeElement),true);
  await page.keyboard.press('Enter'); assert.equal(await page.locator('#main-content').evaluate(e=>e===document.activeElement),true);
  assert.deepEqual(errors,[]); console.log('Browser U02 PASS: 3 widths, no overflow; native dialog focus/Escape/backdrop/restore; raw input, skip link, reduced motion; no page errors.');
 } finally { await browser.close(); }
})().catch(e=>{console.error(e);process.exitCode=1;});
