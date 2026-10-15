// Observation adapter: keep the existing browser assertions and fixture behavior.
const fs = require('node:fs');
const path = require('node:path');
const Module = require('node:module');
const original = path.resolve(__dirname, '../../../frontend/tests/browser/verify-m1.cjs');
const output = __dirname;
let source = fs.readFileSync(original, 'utf8');
const anchor = 'const widthCheck=async p=>assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);';
if (source.split(anchor).length !== 2) throw new Error('Observation anchor drift');
source = source.replace(anchor, `const widthCheck=async p=>{
 assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),true);
 const detail=p.locator('#student-registration-detail');
 if(f.group===87&&await detail.count()){
  const label=await detail.getAttribute('aria-label'),id=Number(label.match(/\\d+/)[0]);
  const result=await api(p,'/api/v1/students/me/registrations/'+id);assert.equal(result.status,200);
  const entry=result.body.entry,selected=entry.attempts.find(x=>x.id===entry.bestAttemptId),text=await detail.innerText();
  const explicitBestMarker=/điểm (?:tốt nhất|cao nhất)|lượt (?:tốt nhất|được chọn)/i.test(text);
  assert.ok(selected&&selected.score!==null,'Backend chosen completed result exists');
  const width=p.viewportSize().width;
  require('node:fs').appendFileSync(${JSON.stringify(path.join(output,'nv04-observations.jsonl'))},JSON.stringify({width,registrationId:id,serverBestAttemptId:entry.bestAttemptId,serverBestScore:selected.score,band:entry.bandAssignment.band.code,explicitBestMarker,synthetic:true})+'\\n');
  await detail.screenshot({path:require('node:path').join(${JSON.stringify(output)},'nv04-student-'+width+'.png'),animations:'disabled'});
 }
};`);
const driver = new Module(original, module);
driver.filename = original;
driver.paths = Module._nodeModulePaths(path.dirname(original));
driver._compile(source, original);
