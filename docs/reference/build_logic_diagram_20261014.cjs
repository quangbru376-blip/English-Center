const fs = require('fs');
const path = require('path');
const { instance } = require('C:/Users/asus/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@viz-js/viz/dist/viz.cjs');
const sharp = require('C:/Users/asus/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
const root = path.resolve(__dirname, '../..');
const dest = path.join(root, 'docs/reference/logic-diagram-20261014');
fs.mkdirSync(dest, {recursive:true});
const schema = JSON.parse(fs.readFileSync(path.join(root, 'docs/reference/database-comparison-20261014/schema-current.json'), 'utf8'));
const current = JSON.parse(fs.readFileSync(path.join(root, 'backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json'), 'utf8'));
const groups = [
  ['identity','TÀI KHOẢN VÀ PHIÊN',['Role','Account','StudentProfile','AuthSession']],
  ['config','CẤU HÌNH ĐÀO TẠO',['Course','Band','TrainingProgram','ScoreComponentConfig']],
  ['entry','ĐĂNG KÝ VÀ THI ĐẦU VÀO',['Registration','EntryTestSession','EntryTestAttempt','EntryTestBooking','BandAssignment']],
  ['class','LỚP LỊCH VÀ KẾT QUẢ',['Class','StudentClassAssignment','Schedule','Attendance','Score','GradeReport']],
  ['finance','HÓA ĐƠN VÀ THANH TOÁN',['Invoice','Payment','InvoiceNumberCounter']],
  ['support','NHẬT KÝ VÀ REPLAY',['AuditLog','ReplayOperation']]
];
const retainAccount = new Set(['StudentProfile:account_id','AuthSession:account_id','AuditLog:actor_account_id','ReplayOperation:actor_account_id','Class:teacher_id','EntryTestSession:grader_id']);
const all = Object.values(schema).flatMap(t=>t.fks);
const edges = all.filter(f=>f.parent!=='Account' || retainAccount.has(f.child+':'+f.columns.join(',')));
const omitted=all.filter(f=>!edges.includes(f));
if (all.length!==50 || Object.keys(schema).length!==24) throw new Error('Unexpected schema');
for(const f of all) if(!current[`fk/dbo/${f.child}/${f.name}`]) throw new Error('Stale comparison schema');
const q=s=>JSON.stringify(s);
let dot=`digraph Logic {
graph [rankdir=RL, newrank=true, compound=true, splines=polyline, nodesep=0.33, ranksep=0.65, bgcolor="white", pad=0.35, fontname="Arial", fontsize=22, labelloc=t, label="SƠ ĐỒ LOGIC DỮ LIỆU\nHệ thống quản lý trung tâm tiếng Anh", outputorder=edgesfirst];
node [shape=box, style="filled", fillcolor="white", color="#202020", penwidth=1.2, fontname="Arial", fontsize=17, margin="0.13,0.14"];
edge [color="#484848", penwidth=1.1, arrowsize=0.75, fontname="Arial", fontsize=11];
`;
for(const [id,label,names] of groups) {
  dot+=`subgraph cluster_${id} { label=${q(label)}; fontsize=15; fontname="Arial"; style="rounded,filled"; color="#aaaaaa"; fillcolor="#f7f7f7"; penwidth=0.7; margin=22;\n`;
  for(const name of names) dot+=`${name} [label=${q(name+(name==='InvoiceNumberCounter'?'\nBộ cấp số theo năm; không có FK':''))}];\n`;
  dot+='}\n';
}
for(const f of edges) {
  const composite=f.columns.length===2;
  const optional=f.columns.some(c=>schema[f.child].columns[c].null==='Y');
  dot+=`${f.child} -> ${f.parent} [id=${q(f.name)}, tooltip=${q(f.child+'('+f.columns.join(',')+') → '+f.parent+'('+f.parent_columns.join(',')+')')}, style=${q(composite?'dashed':'solid')}, `;
  if (f.child==='EntryTestAttempt' && f.parent==='EntryTestBooking') dot+='constraint=false, label="FK ghép", '; 
  if (f.child==='BandAssignment' && f.parent==='EntryTestAttempt') dot+='label="FK ghép", ';
  if (f.child==='Schedule' && f.parent==='Schedule') dot+='label="thay thế", ';
  dot+=`arrowhead=${q(optional?'onormal':'normal')}];\n`;
}
dot+='}\n';
(async()=>{
  const viz = await instance();
  const svg=viz.renderString(dot,{engine:'dot',format:'svg'});
  fs.writeFileSync(path.join(dest,'SO_DO_LOGIC_6_1_20261014.dot'),dot);
  fs.writeFileSync(path.join(dest,'graphviz.svg'),svg);
  fs.writeFileSync(path.join(dest,'layout.json'),JSON.stringify(viz.renderJSON(dot,{engine:'dot'}),null,2));
  fs.writeFileSync(path.join(dest,'model.json'),JSON.stringify({groups,edges,omitted,all},null,2));
  await sharp(Buffer.from(svg),{density:110}).png().toFile(path.join(dest,'draft.png'));
  process.stdout.write(JSON.stringify({tables:24,edges:edges.length,omitted:omitted.length,version:viz.graphvizVersion})+'\n');
})().catch(e=>{process.stderr.write(String(e));process.exitCode=1;});
