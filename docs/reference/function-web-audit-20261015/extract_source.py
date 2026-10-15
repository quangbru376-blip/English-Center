from pathlib import Path
from zipfile import ZipFile
import xml.etree.ElementTree as ET
import hashlib, json, subprocess

root = Path(__file__).resolve().parents[3]
out = Path(__file__).resolve().parent
doc = root / 'docs/Nhom18_ThietKeXuLy.docx'
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
with ZipFile(doc) as z:
    body = ET.fromstring(z.read('word/document.xml')).find('w:body', ns)
    active = False
    group = ''
    rows = []
    tracked = 0
    for node in body:
        text = ''.join(t.text or '' for t in node.findall('.//w:t', ns))
        if node.tag.endswith('}p') and text.startswith('III.'):
            active = True
            continue
        if active and node.tag.endswith('}p') and text.startswith('IV.'):
            break
        if not active:
            continue
        tracked += len(node.findall('.//w:ins', ns)) + len(node.findall('.//w:del', ns))
        if node.tag.endswith('}p') and text.strip():
            group = text.strip()
        if node.tag.endswith('}tbl'):
            for tr in node.findall('w:tr', ns):
                cells = [''.join(t.text or '' for t in cell.findall('.//w:t', ns)) for cell in tr.findall('w:tc', ns)]
                if cells and cells[0].isdigit():
                    assert len(cells) == 5
                    rows.append(dict(nv=int(cells[0]), role=group, name=cells[1], conditions=cells[2], form=cells[3], notes=cells[4]))
assert len(rows) == 37
assert [x['nv'] for x in rows] == list(range(1,10)) + list(range(12,40))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
data = dict(source=str(doc), sha256=sha(doc), section='III. Bảng chức năng nghiệp vụ', trackedChanges=tracked, count=len(rows), rows=rows)
(out/'word-section-iii.json').write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
files = sorted(p for prefix in ['backend/src','frontend/src'] for p in (root/prefix).rglob('*') if p.is_file() and not any(x in p.parts for x in ['bin','obj']))
metadata = dict(head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=root,text=True).strip(), source_sha256={str(p.relative_to(root)).replace('\\','/'):sha(p) for p in files}, wordSha256=sha(doc))
(out/'source-fingerprints-before.json').write_text(json.dumps(metadata,indent=2),encoding='utf-8')
print(json.dumps(dict(rows=len(rows), trackedChanges=tracked, wordSha256=sha(doc), sourceFiles=len(files))))
