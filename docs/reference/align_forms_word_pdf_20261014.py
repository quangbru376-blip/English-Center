from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from lxml import etree as E
import shutil, hashlib

root=Path(__file__).resolve().parents[2]
source=root/'docs/BIEU_MAU_BAN_IN_CHUYEN_NGHIEP_20261014.docx'
qa=root/'docs/reference/word-pdf-alignment-20261014'
qa.mkdir(exist_ok=True)
backup=qa/'before.docx'
if not backup.exists():shutil.copy2(source,backup)
candidate=qa/'aligned.docx'
w='http://schemas.openxmlformats.org/wordprocessingml/2006/main'
ns={'w':w}
def tag(s):return '{'+w+'}'+s
with ZipFile(source) as z:
    entries={i.filename:(i,z.read(i.filename)) for i in z.infolist()}
document=E.fromstring(entries['word/document.xml'][1])
for table in document.findall('.//w:tbl',ns):
    grid=table.find('w:tblGrid',ns)
    if grid is None:continue
    widths=[int(c.get(tag('w'))) for c in grid]
    props=table.find('w:tblPr',ns)
    width=props.find('w:tblW',ns)
    if width is None:width=E.SubElement(props,tag('tblW'))
    width.set(tag('type'),'dxa');width.set(tag('w'),str(sum(widths)))
    layout=props.find('w:tblLayout',ns)
    if layout is None:layout=E.SubElement(props,tag('tblLayout'))
    layout.set(tag('type'),'fixed')
settings=E.fromstring(entries['word/settings.xml'][1])
view=settings.find('w:view',ns)
if view is None:view=E.Element(tag('view'));settings.insert(0,view)
view.set(tag('val'),'print')
zoom=settings.find('w:zoom',ns)
if zoom is None:zoom=E.SubElement(settings,tag('zoom'))
zoom.set(tag('percent'),'100');zoom.set(tag('val'),'none')
for name,tree in [('word/document.xml',document),('word/settings.xml',settings)]:
    entries[name]=(entries[name][0],E.tostring(tree,encoding='UTF-8',xml_declaration=True,standalone=True))
with ZipFile(candidate,'w',ZIP_DEFLATED) as z:
    for info,data in entries.values():z.writestr(info,data)
print(candidate)
print('Reference PDF SHA256',hashlib.sha256(source.with_suffix('.pdf').read_bytes()).hexdigest())
