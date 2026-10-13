// Generates a local, self-contained index. It never reads secrets or calls APIs.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const evidenceRoot=path.resolve(__dirname,'../../../docs/plan/UI-refresh-evidence');
const captureRoot=process.env.EC_UI_CAPTURE_DIR?path.resolve(process.env.EC_UI_CAPTURE_DIR):path.join(evidenceRoot,'before');
if(!captureRoot.startsWith(evidenceRoot+path.sep))throw new Error('Gallery scope');
const root=process.env.EC_UI_GALLERY_DIR?path.resolve(process.env.EC_UI_GALLERY_DIR):(process.env.EC_UI_CAPTURE_DIR?path.join(evidenceRoot,'stage2'):evidenceRoot);
if(root!==evidenceRoot&&!root.startsWith(evidenceRoot+path.sep))throw new Error('Gallery output scope');
const captureFolder=path.relative(root,captureRoot).replaceAll('\\','/');
const lines = fs.readFileSync(path.join(captureRoot, 'manifest.jsonl'), 'utf8').trim().split(/\r?\n/).map(JSON.parse);
const latest = [...new Map(lines.map(row => [row.file, row])).values()];
for (const row of latest) {
  if (path.basename(row.file) !== row.file) throw new Error('Invalid capture path');
  const hash = crypto.createHash('sha256').update(fs.readFileSync(path.join(captureRoot, row.file))).digest('hex');
  if (hash !== row.sha256) throw new Error('Capture digest mismatch');
}
const escape = s => String(s).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;');
const roles = [...new Set(latest.map(r => r.role))].sort();
const summary = { screenshots: latest.length, roles, widths: [...new Set(latest.map(r => r.width))].sort((a,b) => a-b), groups: [...new Set(latest.map(r => r.group))].sort((a,b) => a-b), digestsVerified: true };
fs.writeFileSync(path.join(root, 'capture-summary.json'), JSON.stringify(summary, null, 2));
fs.writeFileSync(path.join(root, 'captures.json'), JSON.stringify(latest, null, 2));
const cards = latest.map(r => `<article data-role="${escape(r.role)}" data-width="${r.width}" data-search="${escape(r.route+' '+r.label+' '+r.headings.join(' '))}"><h2>${escape(r.role)} · ${r.width}px · G${r.group}</h2><p>${escape(r.route)}</p><p>${escape(r.label)}</p><a href="${escape(captureFolder)}/${escape(r.file)}" target="_blank" rel="noopener"><img loading="lazy" src="${escape(captureFolder)}/${escape(r.file)}" alt="${escape(r.role+' '+r.route+' '+r.label)}"></a></article>`).join('\n');
fs.writeFileSync(path.join(root, 'index.html'), `<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>UI refresh — gallery kiểm chứng</title><style>body{font:16px/1.5 system-ui;margin:24px;background:#f8fafc;color:#1e293b}header{position:sticky;top:0;background:#fff;padding:16px;border:1px solid #cbd5e1}select,input{font:inherit;min-height:44px;margin:4px}main{display:grid;grid-template-columns:repeat(auto-fill,minmax(280px,1fr));gap:16px;margin-top:24px}article{border:1px solid #cbd5e1;background:#fff;padding:12px;overflow-wrap:anywhere}article[hidden]{display:none}h2{font-size:16px}img{width:100%;height:360px;object-fit:contain;object-position:top}p{margin:4px 0}</style><header><h1>Ảnh kiểm chứng giao diện</h1><p>${latest.length} ảnh · ${roles.map(escape).join(', ')} · 375/768/1280 · real Edge/HTTPS/SQL scratch, dữ liệu giả. Mật khẩu được che bằng khối màu. Đối chiếu phiên bản theo thư mục và review đi kèm.</p><label>Vai trò <select id="role"><option value="">Tất cả</option>${roles.map(r=>`<option>${escape(r)}</option>`).join('')}</select></label><label>Kích thước <select id="width"><option value="">Tất cả</option><option>375</option><option>768</option><option>1280</option></select></label><label>Tìm route/trạng thái <input id="search" type="search"></label><p id="count"></p></header><main>${cards}</main><script>const role=document.getElementById('role'),width=document.getElementById('width'),search=document.getElementById('search');function filter(){let n=0;for(const card of document.querySelectorAll('article')){card.hidden=!!((role.value&&role.value!==card.dataset.role)||(width.value&&width.value!==card.dataset.width)||(search.value&&!card.dataset.search.toLocaleLowerCase('vi').includes(search.value.toLocaleLowerCase('vi'))));if(!card.hidden)n++;}document.getElementById('count').textContent=n+' ảnh đang hiển thị';}for(const control of [role,width,search])control.addEventListener('input',filter);filter();</script></html>`);
process.stdout.write(JSON.stringify(summary));
