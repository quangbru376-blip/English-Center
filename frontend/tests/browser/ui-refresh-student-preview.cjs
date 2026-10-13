// Visual review of the dev-only mock; business evidence is in reviewed-edge.trx.
const fs = require('node:fs'), path = require('node:path'), assert = require('node:assert/strict'), crypto = require('node:crypto');
const { chromium } = require(process.env.EC_PLAYWRIGHT);
const { installShellControls } = require('./shell-controls.cjs');
(async () => {
  const root = path.resolve(__dirname, '../../../docs/plan/UI-refresh-evidence/stage3/preview');
  fs.mkdirSync(root, { recursive: true });
  const browser = await chromium.launch({ channel: 'msedge', headless: true });
  const page = await browser.newPage(); installShellControls(page);
  let apiCalls = 0;
  page.on('request', request => { if (new URL(request.url()).pathname.startsWith('/api/')) apiCalls++; });
  const images = [];
  const shot = async (label, width) => {
    assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
    const file = `student-${width}-${label}.png`;
    await page.screenshot({ path: path.join(root, file), fullPage: true, animations: 'disabled' });
    images.push({ file, width, label, sha256: crypto.createHash('sha256').update(fs.readFileSync(path.join(root, file))).digest('hex'), synthetic: true, visualOnly: true });
  };
  try {
    for (const width of [375, 768, 1280]) {
      await page.setViewportSize({ width, height: 900 });
      await page.goto('http://127.0.0.1:5178/tests/ui-refresh-student.html');
      await page.getByRole('heading', { name: 'Trang chủ học viên', exact: true }).waitFor();
      await page.evaluate(() => document.fonts.ready); await shot('home', width);
      await page.getByRole('button', { name: 'Chọn khóa học', exact: true }).click();
      await page.getByRole('heading', { name: 'Chọn khóa học', exact: true }).waitFor(); await shot('courses', width);
      for (const state of ['empty', 'loading', 'error']) {
        await page.getByLabel('Trạng thái xem mẫu', { exact: true }).selectOption(state); await shot(state, width);
      }
      await page.getByLabel('Trạng thái xem mẫu', { exact: true }).selectOption('ready');
      await page.getByRole('button', { name: 'Đăng ký khóa IELTS-F', exact: true }).click();
      await page.getByRole('dialog', { name: 'Xác nhận thay đổi', exact: true }).waitFor(); await shot('registration-confirm', width);
      await page.keyboard.press('Escape'); assert.equal(await page.getByRole('dialog').count(), 0);
    }
    assert.equal(apiCalls, 0);
    fs.writeFileSync(path.join(root, 'receipt.json'), JSON.stringify({ passed: true, widths: [375, 768, 1280], overflow: false, apiCalls, visualOnly: true, images }, null, 2));
    process.stdout.write(JSON.stringify({ passed: true, images: images.length, apiCalls }));
  } finally { await browser.close(); }
})().catch(error => { process.stderr.write(error.stack); process.exitCode = 1; });
