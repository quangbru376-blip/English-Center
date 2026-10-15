from pathlib import Path
import json, heapq, math, html, collections, hashlib, zipfile
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.pagesizes import A3,landscape
import pypdfium2 as pdfium

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/reference/logic-grid-20261014';OUT.mkdir(exist_ok=True)
STEM='SO_DO_LOGIC_6_1_THANG_HANG_20261014'
source=json.loads((ROOT/'docs/reference/logic-diagram-20261014/model.json').read_text(encoding='utf-8'))
schema=json.loads((ROOT/'docs/reference/database-comparison-20261014/schema-current.json').read_text(encoding='utf-8'))
rows=[
 ['AuthSession','Role','Account','StudentProfile','Registration','Invoice'],
 ['AuditLog','ReplayOperation','Course','TrainingProgram','BandAssignment','Payment'],
 ['ScoreComponentConfig','Band','Class','StudentClassAssignment','EntryTestAttempt','EntryTestBooking'],
 ['InvoiceNumberCounter','Score','Schedule','Attendance','GradeReport','EntryTestSession']
]
W,H=1540,900;BOXW,BOXH=200,60;STEP=10
boxes={name:(40+250*col,120+180*row,BOXW,BOXH) for row,names in enumerate(rows) for col,name in enumerate(names)}
assert set(boxes)==set(schema) and len(boxes)==24
blocked=set()
for x,y,w,h in boxes.values():
 for gx in range(math.ceil((x-7)/STEP),math.floor((x+w+7)/STEP)+1):
  for gy in range(math.ceil((y-7)/STEP),math.floor((y+h+7)/STEP)+1):blocked.add((gx,gy))
segments=collections.Counter();at_node=collections.defaultdict(collections.Counter);used_ports=collections.Counter()
def seg(a,b):return tuple(sorted((a,b)))
def ports(name):
 x,y,w,h=boxes[name];cx=x+w/2;cy=y+h/2;p=[]
 for shift in [-20,0,20]:
  p += [((int((x-10)/10),int((cy+shift)/10)),(x,cy+shift),2),
        ((int((x+w+10)/10),int((cy+shift)/10)),(x+w,cy+shift),0),
        ((int((cx+shift)/10),int((y-10)/10)),(cx+shift,y),3),
        ((int((cx+shift)/10),int((y+h+10)/10)),(cx+shift,y+h),1)]
 return p
directions=[(1,0),(0,1),(-1,0),(0,-1)]
def route(edge):
 child,parent=edge['child'],edge['parent']
 if child==parent:
  x,y,w,h=boxes[child]
  return [(x+w,y+20),(x+w+25,y+20),(x+w+25,y+40),(x+w,y+40)]
 starts=ports(child);targets={p[0]:p for p in ports(parent)}
 def heuristic(x,y):return min(abs(x-a)+abs(y-b) for a,b in targets)*10
 queue=[];best={};back={};origin={};serial=0
 for point,border,direction in starts:
  state=(*point,direction)
  cost=used_ports[(child,border)]*250
  if state in best and best[state]<=cost:continue
  best[state]=cost;origin[state]=(point,border,direction)
  heapq.heappush(queue,(cost+heuristic(*point),serial,state));serial+=1
 winner=None;winner_cost=float('inf')
 while queue:
  estimate,_,state=heapq.heappop(queue)
  if estimate>=winner_cost:break
  x,y,last=state;cost=best[state]
  if (x,y) in targets:
   point,border,outdir=targets[(x,y)]
   total=cost+used_ports[(parent,border)]*250+(0 if last==(outdir+2)%4 else 30)
   if total<winner_cost:winner=state;winner_cost=total
  for direction,(dx,dy) in enumerate(directions):
   nx,ny=x+dx,y+dy
   if not(1<=nx<=152 and 10<=ny<=79) or (nx,ny) in blocked:continue
   if direction==(last+2)%4:continue
   a=(x,y);b=(nx,ny);orientation=direction%2
   penalty=segments[seg(a,b)]*180+at_node[b][1-orientation]*22
   turn=26 if direction!=last else 0
   newcost=cost+10+turn+penalty
   new=(nx,ny,direction)
   if newcost>=best.get(new,float('inf')):continue
   best[new]=newcost;back[new]=state;origin[new]=origin[state]
   heapq.heappush(queue,(newcost+heuristic(nx,ny),serial,new));serial+=1
 if winner is None:raise ValueError('No route '+edge['name'])
 chain=[winner];state=winner
 while state in back:state=back[state];chain.append(state)
 chain.reverse()
 src=origin[winner];target=targets[winner[:2]]
 used_ports[(child,src[1])]+=1;used_ports[(parent,target[1])]+=1
 points=[src[1]]+[(x*STEP,y*STEP) for x,y,_ in chain]+[target[1]]
 for a,b in zip(chain,chain[1:]):
  aa=a[:2];bb=b[:2];segments[seg(aa,bb)]+=1
  orientation=0 if aa[1]==bb[1] else 1
  at_node[aa][orientation]+=1;at_node[bb][orientation]+=1
 # Keep only actual right-angle turns; all segments remain horizontal/vertical.
 clean=[points[0]]
 for i in range(1,len(points)-1):
  before=clean[-1];now=points[i];after=points[i+1]
  if before[0]==now[0]==after[0] or before[1]==now[1]==after[1]:continue
  if now!=before:clean.append(now)
 if points[-1]!=clean[-1]:clean.append(points[-1])
 return clean

edges=source['edges']
# Short local connections first; route wider links through the available corridors.
def distance(e):
 a=boxes[e['child']];b=boxes[e['parent']]
 return abs(a[0]-b[0])+abs(a[1]-b[1])
routed=[]
for edge in sorted(edges,key=distance):
 path=route(edge);routed.append(dict(edge,path=path))
for edge in routed:
 for a,b in zip(edge['path'],edge['path'][1:]):
  assert a!=b and (a[0]==b[0] or a[1]==b[1]),(edge['name'],a,b)
  for name,(x,y,w,h) in boxes.items():
   if a[0]==b[0]:assert not(x<a[0]<x+w and max(min(a[1],b[1]),y)<min(max(a[1],b[1]),y+h)),(edge['name'],name)
   else:assert not(y<a[1]<y+h and max(min(a[0],b[0]),x)<min(max(a[0],b[0]),x+w)),(edge['name'],name)

legend=[
 'Mũi tên: bảng con → bảng cha. Đầu rỗng: FK nullable. Nét đứt: FK ghép. Giao cắt đường nối không tạo quan hệ.',
 'Đủ 24 bảng; 32 FK chính trên hình. 18 FK người xử lý về Account được liệt kê trong phụ lục nguồn.',
 'InvoiceNumberCounter không có FK. Sơ đồ logic không trình bày toàn bộ cột hoặc cardinality của mô hình vật lý.'
]
pdfmetrics.registerFont(TTFont('GridArial',r'C:\Windows\Fonts\arial.ttf'))
pdfmetrics.registerFont(TTFont('GridArialBold',r'C:\Windows\Fonts\arialbd.ttf'))
svg=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}pt" height="{H}pt" viewBox="0 0 {W} {H}">',
     '<rect width="100%" height="100%" fill="white"/>',
     '<g font-family="Arial" fill="black">',
     f'<text x="{W/2}" y="47" font-size="24" font-weight="bold" text-anchor="middle">SƠ ĐỒ LIÊN KẾT LOGIC</text>',
     f'<text x="{W/2}" y="78" font-size="18" text-anchor="middle">Hệ thống quản lý trung tâm tiếng Anh</text>','</g>']
pw,ph=landscape(A3);pdfpath=ROOT/'docs'/f'{STEM}.pdf'
c=canvas.Canvas(str(pdfpath),pagesize=(pw,ph));c.setTitle('Sơ đồ logic 6.1 các đường nối thẳng và bảng thẳng hàng')
scale=min((pw-42)/W,(ph-42)/H);left=(pw-W*scale)/2;bottom=(ph-H*scale)/2
c.saveState();c.translate(left,bottom);c.scale(scale,scale)
def y(v):return H-v
def pdfline(points,color='black',width=1.2,dashed=False):
 c.saveState();c.setStrokeColor(color);c.setLineWidth(width);c.setLineJoin(0)
 if dashed:c.setDash(6,4)
 p=c.beginPath();p.moveTo(points[0][0],y(points[0][1]))
 for x,yy in points[1:]:p.lineTo(x,y(yy))
 c.drawPath(p);c.restoreState()
c.setFont('GridArialBold',24);c.drawCentredString(W/2,y(47),'SƠ ĐỒ LIÊN KẾT LOGIC')
c.setFont('GridArial',18);c.drawCentredString(W/2,y(78),'Hệ thống quản lý trung tâm tiếng Anh')
for edge in routed:
 points=edge['path'];pts=' '.join(f'{x},{yy}' for x,yy in points)
 composite=len(edge['columns'])==2;optional=any(schema[edge['child']]['columns'][x]['null']=='Y' for x in edge['columns'])
 svg.append(f'<g id="{edge["name"]}"><title>{html.escape(edge["child"]+" → "+edge["parent"])}</title>')
 svg.append(f'<polyline points="{pts}" fill="none" stroke="white" stroke-width="5" stroke-linejoin="miter"/>')
 svg.append(f'<polyline points="{pts}" fill="none" stroke="black" stroke-width="1.2" stroke-linejoin="miter"'+(' stroke-dasharray="6 4"' if composite else '')+'/>')
 pdfline(points,'white',5);pdfline(points,'black',1.2,composite)
 tip=points[-1];prev=points[-2];dx=tip[0]-prev[0];dy=tip[1]-prev[1];length=math.hypot(dx,dy);ux,uy=dx/length,dy/length
 arrow=[tip,(tip[0]-9*ux+3.5*uy,tip[1]-9*uy-3.5*ux),(tip[0]-9*ux-3.5*uy,tip[1]-9*uy+3.5*ux)]
 svg.append('<polygon points="'+' '.join(f'{x},{yy}' for x,yy in arrow)+'" fill="'+('white' if optional else 'black')+'" stroke="black" stroke-width="1.2"/></g>')
 p=c.beginPath();p.moveTo(arrow[0][0],y(arrow[0][1]))
 for x,yy in arrow[1:]:p.lineTo(x,y(yy))
 p.close();c.setFillColor('white' if optional else 'black');c.setStrokeColor('black');c.setLineWidth(1.2);c.drawPath(p,fill=1,stroke=1)
for name,(x,yy,w,h) in boxes.items():
 svg.append(f'<g id="table-{name}"><rect x="{x}" y="{yy}" width="{w}" height="{h}" fill="white" stroke="black" stroke-width="1.4"/><text x="{x+w/2}" y="{yy+h/2+5.3}" font-family="Arial" font-size="15.5" font-weight="bold" text-anchor="middle">{name}</text></g>')
 c.setFillColor('white');c.setStrokeColor('black');c.setLineWidth(1.4);c.rect(x,y(yy+h),w,h,fill=1,stroke=1)
 c.setFillColor('black');c.setFont('GridArialBold',15.5);assert pdfmetrics.stringWidth(name,'GridArialBold',15.5)<w-12,name
 c.drawCentredString(x+w/2,y(yy+h/2+5.3),name)
for i,text in enumerate(legend):
 yy=817+24*i
 svg.append(f'<text x="40" y="{yy}" font-family="Arial" font-size="15" fill="black">{html.escape(text)}</text>')
 c.setFont('GridArial',15);c.drawString(40,y(yy),text)
svg.append('</svg>');svgpath=ROOT/'docs'/f'{STEM}.svg';svgpath.write_text('\n'.join(svg),encoding='utf-8')
c.restoreState();c.showPage();c.save()
pdf=pdfium.PdfDocument(str(pdfpath));image=pdf[0].render(scale=3).to_pil();image.save(ROOT/'docs'/f'{STEM}.png');image.thumbnail((1800,1800));image.save(OUT/'preview.png')
(OUT/'layout.json').write_text(json.dumps({'boxes':boxes,'routes':routed,'omitted':source['omitted']},indent=2),encoding='utf-8')
(OUT/'verification.json').write_text(json.dumps({'tables':24,'shown_fk':32,'appendix_fk':18,'all_connector_segments_axis_aligned':True,'connector_node_interior_collisions':0,'same_width_height':True,'layout':'6 columns x 4 rows','pdf_pages':len(pdf),'shared_grid_segments':sum(v-1 for v in segments.values() if v>1)},indent=2),encoding='utf-8')
readme=f'''# Sơ đồ logic 6.1 đường thẳng và bảng thẳng hàng

Bản riêng theo yêu cầu bố cục như ảnh báo cáo. 24 bảng bố trí thành 6 cột ×4 hàng, hộp chữ nhật cùng kích thước, nét đen/nền trắng. Tất cả đường nối là đoạn ngang/dọc và góc vuông; self-link Schedule cũng là đường gấp vuông, không có đường cong.

- [PNG](../../{STEM}.png), [SVG vector](../../{STEM}.svg), [PDF A3 ngang](../../{STEM}.pdf).
- [Bố cục và đường nối có tọa độ](layout.json).
- [18 FK actor và danh mục đầy đủ](../logic-diagram-20261014/README.md).

Giữ cùng 24 bảng/32FK chính của bản trước; 18FK actor phụ trỏ về Account ở phụ lục. Mũi tên từ bảng con tới bảng cha; đầu rỗng là nullable, nét đứt là FK ghép. Điểm giao cắt không là junction hoặc quan hệ mới. InvoiceNumberCounter không có FK. Không sửa báo cáo Word, source schema hoặc dữ liệu.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
report_hash=hashlib.sha256((ROOT/'docs/Nhom18_ThietKeXuLy.docx').read_bytes()).hexdigest().upper()
assert report_hash=='40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B'
print('Built aligned diagram: 24 boxes, 32 FK routes, orthogonal segments only, no box-interior crossings.')
