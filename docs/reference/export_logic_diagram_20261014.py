from pathlib import Path
import json,re,csv,hashlib
from lxml import etree
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.pagesizes import A3,landscape
from reportlab.lib.colors import HexColor
import pypdfium2 as pdfium

ROOT=Path(__file__).resolve().parents[2]
DIR=ROOT/'docs/reference/logic-diagram-20261014'
NAME='SO_DO_LOGIC_6_1_20261014'
model=json.loads((DIR/'model.json').read_text(encoding='utf-8'))
layout=json.loads((DIR/'layout.json').read_text(encoding='utf-8'))
parser=etree.XMLParser(resolve_entities=False,no_network=True)
svg=etree.fromstring((DIR/'graphviz.svg').read_bytes(),parser)
NS='{http://www.w3.org/2000/svg}'
x0,y0,width,height=map(float,svg.get('viewBox').split())
legend=[
 'Mũi tên: bảng con → bảng cha. Đầu đặc: FK bắt buộc; đầu rỗng: FK nullable. Nét đứt: FK ghép.',
 'Đủ 24 bảng; hình thể hiện 32 FK chính. 18 FK người xử lý → Account được liệt kê trong tài liệu kèm theo.',
 'InvoiceNumberCounter không có FK. Đây là sơ đồ logic theo source hiện tại, không trình bày toàn bộ cột và cardinality.'
]
# Draw cluster headings above crossing edges, with a white halo for legibility.
graph=svg.find(NS+'g')
overlay=etree.Element(NS+'g')
for group in graph.findall(NS+'g'):
    if group.get('class')=='cluster':
        for text in list(group.findall(NS+'text')):
            group.remove(text);text.set('paint-order','stroke fill');text.set('stroke','white');text.set('stroke-width','4');text.set('stroke-linejoin','round');overlay.append(text)
graph.append(overlay)
legend_height=82
svg.set('height',f'{height+legend_height:.2f}pt')
svg.set('viewBox',f'0 0 {width:.2f} {height+legend_height:.2f}')
for i,text in enumerate(legend):
    t=etree.SubElement(svg,NS+'text',x='25',y=str(height+23+i*22),fill='#303030',attrib={'font-family':'Arial','font-size':'16'})
    t.text=text
svgpath=ROOT/'docs'/f'{NAME}.svg'
etree.ElementTree(svg).write(str(svgpath),encoding='utf-8',xml_declaration=True)

pdfmetrics.registerFont(TTFont('LogicArial',r'C:\Windows\Fonts\arial.ttf'))
pdfmetrics.registerFont(TTFont('LogicArialBold',r'C:\Windows\Fonts\arialbd.ttf'))

def draw_ops(c,ops):
    c.saveState();size=17;color='#000000'
    for op in ops:
        k=op['op']
        if k in ('c','C'):
            value=op['color'][:7]
            if k=='c':c.setStrokeColor(HexColor(value));color=value
            else:c.setFillColor(HexColor(value))
        elif k=='S':
            style=op['style'];m=re.match(r'setlinewidth\(([^)]+)\)',style)
            if m:c.setLineWidth(float(m.group(1)))
            elif style=='dashed':c.setDash(6,4)
            elif style=='dotted':c.setDash(1,3)
            elif style=='solid':c.setDash()
        elif k=='F':size=op['size'];c.setFont('LogicArial',size)
        elif k=='T':
            x,y=op['pt'];text=op['text'];c.setFillColor(HexColor(color))
            if op['align']=='c':c.drawCentredString(x,y,text)
            elif op['align']=='r':c.drawRightString(x,y,text)
            else:c.drawString(x,y,text)
        elif k in ('p','P','L','b','B'):
            pts=op['points'];p=c.beginPath();p.moveTo(*pts[0])
            if k in ('b','B'):
                assert (len(pts)-1)%3==0
                for i in range(1,len(pts),3):p.curveTo(*(pts[i]+pts[i+1]+pts[i+2]))
            else:
                for pt in pts[1:]:p.lineTo(*pt)
            if k in ('p','P','B'):p.close()
            c.drawPath(p,stroke=1,fill=1 if k in ('P','B') else 0)
        elif k in ('e','E'):
            x,y,rx,ry=op['rect'];c.ellipse(x-rx,y-ry,x+rx,y+ry,fill=k=='E',stroke=1)
        else:raise ValueError('Unhandled drawing operation '+k)
    c.restoreState()

pdfpath=ROOT/'docs'/f'{NAME}.pdf'
pw,ph=landscape(A3);c=canvas.Canvas(str(pdfpath),pagesize=(pw,ph))
c.setTitle('Sơ đồ logic dữ liệu 6.1 theo dự án hiện tại')
c.setAuthor('English Center Management System')
pad=25.2;totalh=height+legend_height;scale=min((pw-44)/width,(ph-44)/totalh)
left=(pw-width*scale)/2;bottom=(ph-totalh*scale)/2
c.saveState();c.translate(left,bottom+legend_height*scale);c.scale(scale,scale);c.translate(pad,pad)
clusters=[x for x in layout['objects'] if x.get('name','').startswith('cluster_')]
nodes=[x for x in layout['objects'] if not x.get('name','').startswith('cluster_')]
for cl in clusters:draw_ops(c,cl.get('_draw_',[]))
for edge in layout['edges']:
    for key in ['_draw_','_hdraw_','_tdraw_','_ldraw_']:draw_ops(c,edge.get(key,[]))
for node in nodes:
    draw_ops(c,node.get('_draw_',[]));draw_ops(c,node.get('_ldraw_',[]))
for cl in clusters:
    labels=cl.get('_ldraw_',[])
    for op in labels:
        if op['op']=='T':
            x,y=op['pt'];w=pdfmetrics.stringWidth(op['text'],'LogicArial',15)
            c.setFillColor(HexColor('#ffffff'));c.rect(x-w/2-3,y-2,w+6,18,fill=1,stroke=0)
    draw_ops(c,labels)
draw_ops(c,layout.get('_ldraw_',[]))
c.restoreState()
c.setFont('LogicArial',16*scale);c.setFillColor(HexColor('#303030'))
for i,text in enumerate(legend):c.drawString(left+25*scale,bottom+(legend_height-23-i*22)*scale,text)
c.showPage();c.save()
pdf=pdfium.PdfDocument(str(pdfpath));assert len(pdf)==1
png=pdf[0].render(scale=3).to_pil()
pngpath=ROOT/'docs'/f'{NAME}.png';png.save(pngpath)
png.thumbnail((1800,1800));png.save(DIR/'preview.png')

with (DIR/'DANH_MUC_50_FK.csv').open('w',encoding='utf-8-sig',newline='') as f:
    writer=csv.writer(f);writer.writerow(['FK','Bảng con','Cột con','Bảng cha','Cột cha','Trên hình 6.1'])
    shown={x['name'] for x in model['edges']}
    for edge in model['all']:writer.writerow([edge['name'],edge['child'],','.join(edge['columns']),edge['parent'],','.join(edge['parent_columns']),'Có' if edge['name'] in shown else 'Phụ lục actor'])

rows='\n'.join('| '+f['child']+' | '+', '.join(f['columns'])+' | Account(account_id) |' for f in model['omitted'])
readme=f'''# Sơ đồ logic dữ liệu mục 6.1

Ngày: 14/10/2026. Sơ đồ thay thế hình cũ theo 24 bảng trong dự án. Word nguồn không thay đổi.

- [PNG độ phân giải cao](../../SO_DO_LOGIC_6_1_20261014.png): chèn vào Word.
- [SVG vector](../../SO_DO_LOGIC_6_1_20261014.svg): phóng to/in rõ và chỉnh sửa bằng công cụ vector.
- [PDF một trang A3 ngang](../../SO_DO_LOGIC_6_1_20261014.pdf): xem/in; khi in A4 ngang chọn Fit to page.
- [Nguồn Graphviz DOT](SO_DO_LOGIC_6_1_20261014.dot): chỉnh tên/nhóm/bố cục từ mô hình.
- [Danh mục 50 FK](DANH_MUC_50_FK.csv): phân biệt 32 FK trên hình và 18 FK trong bảng dưới.

## Cách đọc

Mũi tên đi từ bảng con chứa FK tới bảng cha. Đầu đặc cho FK bắt buộc, đầu rỗng khi FK nullable; không là ký hiệu một/nhiều. Nét đứt chỉ hai FK ghép. Các đường cắt nhau không tạo quan hệ hay junction nếu không đi vào một hộp bảng; hộp nhóm chỉ tổ chức bố cục, không SQL schema khác dbo.

- EntryTestAttempt(completed_booking_id,attempt_id) → EntryTestBooking(booking_id,attempt_id), tùy chọn theo result bundle.
- BandAssignment(best_attempt_id,registration_id) → EntryTestAttempt(attempt_id,registration_id), bắt buộc.
- Schedule self-link là replaces_schedule_id → schedule_id, không tự tạo buổi thay.
- InvoiceNumberCounter không FK sang Invoice. Audit entity_id và operation correlations không được vẽ thành FK giả.

Sơ đồ6.1 chỉ tên bảng/quan hệ logic chính, không mô tả toàn bộ cột hoặc cardinality của sơ đồ quan hệ vật lý6.4. Có đủ24 bảng,32FK chính;18FK người xử lý cùng trỏ Account được liệt kê để giữ khả năng đối chiếu đủ50FK. Teacher của Class và grader của Session được giữ trên hình vì là phân công vận hành chính.

## 18 FK người xử lý được trình bày riêng

| Bảng con | Cột FK | Tham chiếu |
| --- | --- | --- |
{rows}

Nullable của từng cột và NO ACTION được xem ở [đối chiếu6.3.4](../database-comparison-20261014/6_3_4_RANG_BUOC_THAM_CHIEU.md). FK về Account chỉ kiểm tồn tại, không tự chứng minh current role/ACTIVE/ownership. Không thêm các actor phụ này như bảng hoặc role mới.

## Kiểm chứng

Nguồn: schema trích và đối chiếu migration/EF snapshot trong lượt database, kiểm lại danh mục FK với ExpectedCatalog hiện tại. 24 tên bảng khớp source; 32 +18 =50FK, hai composite/self-reference được giữ đúng; không học bổng, không Attempt.session_id, không Assignment.student_id. Không chạy SQL/migration hoặc thay dữ liệu.
'''
(DIR/'README.md').write_text(readme,encoding='utf-8')
assert len(svg.xpath('.//*[local-name()="g" and @class="node"]'))==24
assert len(svg.xpath('.//*[local-name()="g" and @class="edge"]'))==32
assert len(model['omitted'])==18
report_hash=hashlib.sha256((ROOT/'docs/Nhom18_ThietKeXuLy.docx').read_bytes()).hexdigest().upper()
assert report_hash=='40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B'
(DIR/'verification.json').write_text(json.dumps({'tables':24,'displayed_fk':32,'appendix_fk':18,'all_fk':50,'pdf_pages':1,'pdf_size':'A3 landscape','png_size':pdf[0].render(scale=3).to_pil().size,'source_report_unchanged':True,'hashes':{p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in [svgpath,pdfpath,pngpath]}},indent=2),encoding='utf-8')
print('Exported SVG, one-page A3 PDF, PNG; 24 nodes and 32 + 18 FKs verified.')
