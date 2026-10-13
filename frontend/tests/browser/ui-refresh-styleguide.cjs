const assert = require('node:assert/strict');
const fs = require('node:fs'), path = require('node:path');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
const output = path.resolve(__dirname, '../../../docs/plan/UI-refresh-evidence/stage2/styleguide');
fs.mkdirSync(output, { recursive: true });
(async () => {
  const browser = await chromium.launch({ channel:'msedge', headless:true });
  try {
    const page = await browser.newPage(), errors=[];
    page.on('pageerror',()=>errors.push('page-error'));
    await page.goto('http://127.0.0.1:5178/tests/ui-refresh-styleguide.html');
    await page.getByRole('heading',{name:'Một nền giao diện. Mọi màn hình.',exact:true}).waitFor();
    await page.evaluate(()=>document.fonts.ready);
    for(const width of [375,768,1280]) {
      await page.setViewportSize({width,height:900});
      assert.equal(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
      assert.equal(await page.evaluate(()=>document.fonts.check('16px "Be Vietnam Pro"')),true);
      await page.screenshot({path:path.join(output,'styleguide-'+width+'.png'),fullPage:true,animations:'disabled'});
      await page.getByRole('tab',{name:'Tổng quan',exact:true}).focus();
      await page.keyboard.press('ArrowRight');
      assert.equal(await page.getByRole('tab',{name:'Chi tiết',exact:true}).getAttribute('aria-selected'),'true');
      await page.keyboard.press('Home');
      assert.equal(await page.getByRole('tab',{name:'Tổng quan',exact:true}).getAttribute('aria-selected'),'true');
      const opener=page.getByRole('button',{name:'Mở hộp thoại mẫu',exact:true});
      await opener.click(); await page.getByRole('dialog',{name:'Xác nhận thay đổi',exact:true}).waitFor();
      await page.screenshot({path:path.join(output,'dialog-'+width+'.png'),fullPage:true,animations:'disabled'});
      await page.keyboard.press('Escape');assert.equal(await opener.evaluate(e=>e===document.activeElement),true);
    }
    assert.deepEqual(errors,[]);
    fs.writeFileSync(path.join(output,'receipt.json'),JSON.stringify({passed:true,widths:[375,768,1280],fontLoaded:true,noPageOverflow:true,tabsKeyboard:true,dialogEscapeFocusRestored:true},null,2));
    process.stdout.write('styleguide PASS');
  } finally { await browser.close(); }
})().catch(()=>{process.stderr.write('Styleguide check failed');process.exitCode=1;});
