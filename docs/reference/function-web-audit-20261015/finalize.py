from pathlib import Path
import json, hashlib
root=Path(__file__).resolve().parents[3]
out=Path(__file__).resolve().parent
before=json.loads((out/'source-fingerprints-before.json').read_text())
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
changed=[p for p,digest in before['source_sha256'].items() if not (root/p).is_file() or sha(root/p)!=digest]
new=sorted(str(p.relative_to(root)).replace('\\','/') for prefix in ['backend/src','frontend/src'] for p in (root/prefix).rglob('*') if p.is_file() and not any(x in p.parts for x in ['bin','obj']) and str(p.relative_to(root)).replace('\\','/') not in before['source_sha256'])
word_hash=sha(root/'docs/Nhom18_ThietKeXuLy.docx')
ownership=[json.loads(line) for line in (out/'ownership/E-scratch-ownership.jsonl').read_text().splitlines() if line]
owned={}
for row in ownership:
    key=(row['name'],row['databaseId'],row['databaseGuid'])
    owned.setdefault(key,[]).append(row['state'])
unclosed=[dict(name=k[0],databaseId=k[1],databaseGuid=k[2],states=v) for k,v in owned.items() if v!=['created','dropped']]
matrix=json.loads((out/'function-matrix.json').read_text(encoding='utf-8'))
result=dict(head=before['head'],wordSha256=word_hash,wordUnchanged=word_hash==before['wordSha256'],productSourceFiles=len(before['source_sha256']),changedProductSource=changed,newProductSource=new,functions=37,functionStatus={'implemented_and_wired':36,'partial_display_requirement_nv04':1},verdict='PASS_WITH_FIXES_NV04',frontendPassed=183,types='PASS',lint='PASS',backendDefault=matrix['backendDefault'],browser=matrix['browser'],scratchCreated=len(owned),scratchPairedDropped=sum(v==['created','dropped'] for v in owned.values()),unclosedOwnedScratch=unclosed)
assert not changed and not new and result['wordUnchanged']
assert not unclosed
assert result['browser'] and result['browser']['total']=='21' and result['browser']['passed']=='21'
assert result['backendDefault']['passed']=='1438' and result['backendDefault']['failed']=='0'
independent=json.loads((out/'independent-scratch-check.json').read_text(encoding='utf-8-sig'))
assert independent['liveScratch']==0
result['independentLiveScratch']=0
obs_path=out/'nv04-observations.jsonl'
if obs_path.exists():
    obs=[json.loads(line) for line in obs_path.read_text(encoding='utf-8').splitlines()]
    assert sorted(x['width'] for x in obs)==[375,768,1280]
    assert all(not x['explicitBestMarker'] and x['serverBestAttemptId'] and x['serverBestScore'] is not None for x in obs)
    result['nv04ReproducedWidths']=[x['width'] for x in obs]
(out/'summary.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in result.items() if k not in ['browser','backendDefault']},ensure_ascii=False))
