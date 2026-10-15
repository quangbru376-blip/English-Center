"""Source-only report/schema comparison; no SQL execution or source report edits."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from lxml import etree
import re, json, hashlib, collections

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/reference/database-comparison-20261014'
OUT.mkdir(exist_ok=True)
(OUT/'tables').mkdir(exist_ok=True)
NS={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
REPORT=ROOT/'docs/Nhom18_ThietKeXuLy.docx'
CATPATH=ROOT/'backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json'
CAT=json.loads(CATPATH.read_text(encoding='utf-8-sig'))
with ZipFile(REPORT) as z:
    BODY=list(etree.fromstring(z.read('word/document.xml')).find('w:body',NS))
def txt(e): return ''.join(e.xpath('.//w:t/text()',namespaces=NS))
OLD={}
for i in range(272,315,2):
    name=txt(BODY[i-1]).split(' - ')[1]
    OLD[name]=[dict(zip(['name','type','null','key','meaning'],
                      [txt(c) for c in row.xpath('./w:tc',namespaces=NS)]))
               for row in BODY[i].xpath('./w:tr',namespaces=NS)[1:]]
ORDER=list(OLD)+['ReplayOperation','InvoiceNumberCounter']
assert len(ORDER)==24

def shape(value):
    p=value.split('|');t=p[0];length=int(p[1]);precision=int(p[2]);scale=int(p[3])
    if t=='timestamp': t='rowversion'
    elif t in ['varchar','nvarchar','binary','varbinary']:
        t=f'{t}({"max" if length==-1 else length//2 if t=="nvarchar" else length})'
    elif t=='decimal':t=f'decimal({precision},{scale})'
    elif t in ['time','datetime2']:t=f'{t}({scale})'
    return {'type':t,'null':'Y' if p[4]=='1' else 'N','identity':p[5]=='1','collation':p[7]}

SCHEMA={t:{'columns':{},'keys':[],'indexes':[],'fks':[],'checks':[],'defaults':{}} for t in ORDER}
for key,value in CAT.items():
    bits=key.split('/');kind=bits[0]
    if len(bits)<3 or bits[1]!='dbo' or bits[2] not in SCHEMA:continue
    t=SCHEMA[bits[2]]
    if kind=='column':t['columns'][bits[3]]=shape(value)
    elif kind=='key':
        k,cols=value.split('|');t['keys'].append({'name':bits[3],'kind':k,'columns':cols.split(',')})
    elif kind=='index':
        p=value.split('|');columns=[x.split(':')[0] for x in p[6].split(',') if x]
        t['indexes'].append({'name':bits[3],'unique':p[1]=='1','pk':p[2]=='1','constraint':p[3]=='1','filter':p[5],'columns':columns})
    elif kind=='fk':
        p=value.split('|');pairs=[x.split(':') for x in p[5].split(',')]
        t['fks'].append({'name':bits[3],'child':bits[2],'parent':p[0].split('.')[-1],
                         'columns':[x[0] for x in pairs],'parent_columns':[x[1] for x in pairs],
                         'on_delete':p[1],'on_update':p[2],'disabled':p[3],'untrusted':p[4]})
    elif kind=='check':t['checks'].append({'name':bits[3],'sql':value.split('|',2)[2]})
    elif kind=='default':t['defaults'][bits[3]]=value

# Parse only balanced C# source calls. SQL strings remain inert data.
def balanced(s,start,opening='(',closing=')'):
    depth=0;quoted=False;escape=False
    for i in range(start,len(s)):
        c=s[i]
        if quoted:
            if escape:escape=False
            elif c=='\\':escape=True
            elif c=='"':quoted=False
            continue
        if c=='"':quoted=True
        elif c==opening:depth+=1
        elif c==closing:
            depth-=1
            if depth==0:return s[start+1:i],i+1
    raise ValueError('Unbalanced C# call')

MIGPATH=ROOT/'backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs'
mig=MIGPATH.read_text(encoding='utf-8');migration={}
for m in re.finditer(r'migrationBuilder\.CreateTable\(',mig):
    block,_=balanced(mig,m.end()-1)
    table=re.search(r'\bname:\s*"([^"]+)"',block).group(1)
    cols={}
    for p in re.finditer(r'\b(\w+)\s*=\s*table\.Column<[^>]+>\(',block):
        args,end=balanced(block,p.end()-1)
        ty=re.search(r'type:\s*"([^"]+)"',args).group(1)
        null='Y' if re.search(r'nullable:\s*true',args) else 'N'
        suffix=block[end:end+100]
        identity=bool(re.match(r'\s*\.Annotation\("SqlServer:Identity"',suffix))
        cols[p.group(1)]={'type':ty,'null':null,'identity':identity}
    migration[table]=cols
assert set(migration)==set(SCHEMA)
assert sum(len(t['columns']) for t in SCHEMA.values())==230
for table,cols in migration.items():
    assert set(cols)==set(SCHEMA[table]['columns']),table
    for col,info in cols.items():
        actual=SCHEMA[table]['columns'][col]
        assert all(actual[x]==v for x,v in info.items()),(table,col,info,actual)
assert all(f['on_delete']=='0' and f['on_update']=='0' and f['disabled']=='0' and f['untrusted']=='0'
           for t in SCHEMA.values() for f in t['fks'])
up=mig.split('protected override void Down')[0]
assert set(re.findall(r'"(FK_[^"]+)"',up))=={f['name'] for t in SCHEMA.values() for f in t['fks']}
assert set(re.findall(r'table\.CheckConstraint\("([^"]+)"',up))=={c['name'] for t in SCHEMA.values() for c in t['checks']}
assert len(re.findall(r'migrationBuilder\.CreateIndex\(',up))==78
assert len(re.findall(r'table\.PrimaryKey\(',up))==24
assert len(re.findall(r'table\.UniqueConstraint\(',up))==2

SNAPPATH=ROOT/'backend/src/EnglishCenter.Api/Infrastructure/Migrations/EnglishCenterDbContextModelSnapshot.cs'
snaptext=SNAPPATH.read_text(encoding='utf-8');snapshot={}
for m in re.finditer(r'modelBuilder\.Entity\("[^"]+", b =>\s*\{',snaptext):
    block,_=balanced(snaptext,m.end()-1,'{','}')
    tm=re.search(r'b\.ToTable\("([^"]+)"',block)
    if tm is None:continue
    name=tm.group(1);cols={}
    pk_match=re.search(r'b\.HasKey\(([^;]+)\)',block)
    primary_properties=re.findall(r'"([^"]+)"',pk_match.group(1)) if pk_match else []
    for pm in re.finditer(r'b\.Property<([^>]+)>\("([^"]+)"\)',block):
        fragment=block[pm.start():block.index(';',pm.start())]
        cm=re.search(r'HasColumnName\("([^"]+)"\)',fragment)
        typem=re.search(r'HasColumnType\("([^"]+)"\)',fragment)
        if cm is None or typem is None:continue
        clr=pm.group(1)
        nullable=(clr.endswith('?') or clr in ['string','byte[]'] and '.IsRequired()' not in fragment) and pm.group(2) not in primary_properties
        cols[cm.group(1)]={'type':typem.group(1),'null':'Y' if nullable else 'N'}
    snapshot[name]=cols
assert set(snapshot)==set(SCHEMA)
for name,cols in snapshot.items():
    assert set(cols)==set(SCHEMA[name]['columns']),name
    for col,info in cols.items():
        assert all(SCHEMA[name]['columns'][col][k]==v for k,v in info.items()),(name,col,info)

OLDFK=[]
for i in range(429,973):
    text=txt(BODY[i])
    m=re.match(r'(\d+)\. Ràng buộc:\s*(\w+)\(([^)]+)\)\s*→\s*(\w+)\(([^)]+)\)',text)
    if m:
        number,child,cols,parent,pcols=m.groups()
        OLDFK.append({'number':int(number),'child':child,'columns':[x.strip() for x in cols.split(',')],
                      'parent':parent,'parent_columns':[x.strip() for x in pcols.split(',')],'body_index':i})
assert len(OLDFK)==49
ACTFK=[f for t in SCHEMA.values() for f in t['fks']]
assert len(ACTFK)==50
def fksig(f):return f['child'],tuple(f['columns']),f['parent'],tuple(f['parent_columns'])
matches={fksig(f):f for f in ACTFK}
assert all(fksig(f) in matches for f in OLDFK)
MISSINGFK=[f for f in ACTFK if fksig(f) not in {fksig(x) for x in OLDFK}]
assert len(MISSINGFK)==1 and MISSINGFK[0]['child']=='ReplayOperation'

def esc(s):return str(s).replace('|','\\|').replace('\n',' ')
def table(head,rows):
    return '| '+' | '.join(head)+' |\n| '+' | '.join('---' for _ in head)+' |\n'+ '\n'.join('| '+' | '.join(esc(x) for x in row)+' |' for row in rows)+'\n'
COMMON='''Ngày đối chiếu: 14/10/2026. Nguồn báo cáo: `Nhom18_ThietKeXuLy.docx`, phần VI. Bản Word gốc được giữ nguyên.

“Hiện tại” trong tài liệu là schema/SQL/service trong source dự án, đối chiếu InitialSchema, EF snapshot/configurations và ExpectedCatalog; không phải kết quả truy vấn SQL live ở lượt này. Không chạy migration, sửa database hoặc chạy lại các bộ test.

'''
def write(name,title,body):
    (OUT/name).write_text('# '+title+'\n\n'+COMMON+body+'\n\nNguồn dùng chung và phương pháp: [README](README.md).\n',encoding='utf-8')

NOTES={
 'Role':['Bảng 6.2 dùng đúng role code, nhưng 6.3.1 viết STUDENT/ACADEMIC_AFFAIRS… sai literal. SQL CHECK chấp nhận Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin, đúng case.','Bảy account role cố định khác bốn database role phân quyền SQL; không suy ra một bảng permission hoặc RBAC động.'],
 'Account':['Không có locked_until. LoginThrottledUntil là timer chặn login mới, khác Account LOCKED; failed_login_count không tự chuyển Account LOCKED.','Mật khẩu tạm, securityVersion và revoke-all được thực thi theo lệnh tài khoản; CHECK temp bundle không tự đổi credential hoặc thu hồi SID.','Phone nullable ở SQL cho staff; Student signup/profile yêu cầu phone tại validation/service.'],
 'StudentProfile':['Sửa profile kiểm RV Account và Profile trong cùng giao dịch; audit chỉ changedFields. Không replay completion/receipt hoặc revoke-all trong API Profile hiện tại.','DOB không sau ngày VN hiện tại do validation/service, không có CHECK gọi đồng hồ. Staff không có Profile dù FK riêng không kiểm role.'],
 'AuthSession':['sid là binary(32), không PK int/IDENTITY. Phiên NORMAL tối đa 2h/idle30m; PASSWORD_CHANGE tối đa10m không vượt hạn secret tạm.','Me/CSRF/background focus không renew idle. Revoked/absolute expiry/securityVersion/current status được kiểm server; CK temporal không tự chứng minh session còn quyền.'],
 'Course':['base_price >0, không ≥0 như 6.3.1; VND nguyên đồng. Giá mới chỉ Invoice phát hành sau, không sửa snapshot cũ.','Course code bất biến, trạng thái ACTIVE/INACTIVE giữ lịch sử; name không unique toàn hệ thống.'],
 'Band':['Không max_score. Min-score lớn nhất ≤ best score; seed 0/25/45/65/85.','First completed entry result khóa threshold set qua protocol và trigger; bảng tham chiếu không có editor ngưỡng trong UI hiện tại.'],
 'TrainingProgram':['Không approved_by/approved_at hoặc Director approval. Activation cần cấu hình đã lưu tổng weight100, đúng1 FINAL_EXAM và LESSON count dương.','First Class tham chiếu khóa cấu hình, kể cả lớp CANCELLED; đổi tên Program là action riêng không giải freeze.'],
 'ScoreComponentConfig':['Kind GENERAL/LESSON/FINAL_EXAM. Weight >0 đến100, input≤2 số lẻ; component_name unique trong Program theo collation.','Filtered unique FINAL_EXAM bảo vệ tối đa1, không bảo đảm có đúng1 khi ACTIVE; activation/service kiểm tổng và sự tồn tại.'],
 'Registration':['is_unfinished là predicate có lưu và filtered unique Student/Course; không permanent unique để chặn học lại. Không thêm Registration COMPLETED.','Chỉ CONFIRMED→ADMITTED khi Admissions duyệt; ADMITTED + Invoice nguyên tử. Metadata confirmation/approval giữ khi hủy sau đó, không xóa lịch sử.'],
 'EntryTestSession':['Mới CLOSED; OPEN trước start cho booking; effectively CLOSED từ start không đồng nghĩa read tự UPDATE.','start_at/end_at UTC datetime2(3), không session_date/session_time của hình 6.4. Grader hiện tại và overlap/capacity là guard/service.'],
 'EntryTestAttempt':['Không có status hoặc session_id. Logical Attempt giữ attempt_number và bundle score/graded_by/graded_at/completed_booking_id.','Trạng thái lịch hẹn nằm EntryTestBooking; completed_booking_id FK ghép cùng Attempt. Sửa score giữ original graded_by/graded_at.'],
 'EntryTestBooking':['Đây mới là liên kết Attempt→Session và nơi lưu SCHEDULED/COMPLETED/ABSENT/CANCELLED. Hình cũ thiếu bảng này.','Một SCHEDULED và một COMPLETED tối đa cho Attempt bằng filtered UQ; history có nhiều booking. Rebook sau ABSENT không tạo lượt mới.'],
 'BandAssignment':['FK ghép best_attempt_id/registration_id chặn Attempt khác hồ sơ; service chọn best completed score, hòa chọn lượt1.','Không có UQ riêng best_attempt_id. Không được viết tên unique index không tồn tại; uniqueness theo nghĩa nghiệp vụ suy từ FK ghép và UQ Registration.'],
 'Class':['PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED; không approved_by/approved_at. OPEN nhận placement, ONGOING không nhận mới.','COMPLETE kiểm đủ current obligations HELD và required Attendance; chưa chốt điểm. Hủy chỉ lớp PLANNED/OPEN trống/chưa teaching.'],
 'StudentClassAssignment':['PK class_assignment_id, FK registration_id thay student_id/assignment_id của hình cũ. Student xác định qua Registration.student_id.','Placement ADMITTED/PAID/Class OPEN trước buổi đầu/cùngCourseBand/cònchỗ/khôngtrùng; tạo DRAFT GradeReport cùng commit. Không chuyển/gỡ lớp sau xếp.'],
 'Schedule':['Root/replacement cùng obligation_key; một current occurrence/nghĩa vụ, một current FINAL_EXAM/Class; không unique Class/type trên toàn lịch sử.','HELD do Giáo vụ khi now≥end, Class ONGOING/current PLANNED; Attendance hoặc clock không tự HELD. Move/replacement/lịch tuần có các guard riêng.'],
 'Attendance':['Không row là chưa ghi; bit0 là Vắng có chủ đích, không default present. Một lần save tập supplied changes của một LESSON nguyên tử.','Không AuditLog nghiệp vụ Attendance history riêng trong code hiện tại: latest enteredBy/At và replay; không thêm audit theo suy đoán.','TG04 chặn sai Class/type/current occurrence và Class ended; current Teacher/time/RV ở service.'],
 'Score':['GENERAL schedule_id NULL; LESSON/FINAL_EXAM cần current occurrence cùngClass/type, khôngNOT_HELD, từ start. TG05 bảo vệ relation, service bảo vệ clock/quyền.','Lưu actual changes + comment một Student/touch GradeReport RV; không submit một Student. Không override score đặc biệt cho Admin/Giáo vụ.'],
 'GradeReport':['Không approved_by/approved_at; reviewer fields và operation IDs đầy đủ. final_score chỉ khi FINALIZED; tổng nháp tính khi đọc.','RETURNED reason bắt buộc; latest reason có thể giữ sau resubmit và finalize. Không có CHECK bắt returned_reason NULL ở FINALIZED như đoạn 6.3.2 cũ.','SUBMIT/RETURN/FINALIZE toànClass, decimal half-up2 mộtlần; passed khi FINALIZED≥50; không reopen.'],
 'Invoice':['amount >0; số HD-YYYY-NNNNNNNN do allocator cấp, không invoice_id. Issue/due snapshot, ngày VN+7 lịch.','Chỉ tạo cùng duyệt tuyển sinh; UNPAID/PAID/CANCELLED, overdue là flag không state. Không Invoice PAID thiếu Payment qua workflow chuẩn.'],
 'Payment':['amount_paid >0 và bằng Invoice snapshot theo transaction guard; CHECK amount dương chưa chứng minh bằng Invoice.','Mã đúng CASH/TRANSFER, không BANK_TRANSFER. transaction_reference không unique; TRANSFER cần reference.','Correction chỉ method/reference với reason; giữ invoice/amount/confirmedBy/paidAt; latest correction và audit.'],
 'AuditLog':['Actor NULL khi chưa xác thực; không biến Hệ thống thành account role. Polymorphic entity_type/entity_id và operation_id không FK sang mọi business entity.','MANDATORY lưu cùng mutation; SECURITY best-effort/retention riêng. Admin viewer không edit/purge/export; vận hành có cleanup security/IP theo policy.'],
 'ReplayOperation':['Thiếu hoàn toàn ở 6.2 cũ. Completion receipt chống hiệu ứng ghi lặp theo actor/action/request, cùng transaction mutation; no marker khi rollback.','Không lưu credential/raw PII response; SECRET_RESULT_UNAVAILABLE không replay plaintext. Không mọi API đều dùng bảng này: Profile và self-password có hợp đồng riêng.'],
 'InvoiceNumberCounter':['Thiếu ở 6.2 cũ. Một row/năm VN, issue_year smallint PK, last_value 0–99.999.999 và internal RV.','Allocator row/range lock trong duyệt, không reset/reuse số đã phát hành; không hứa số liên tục và không FK sang Invoice.']
}

def table_docs():
    summaries=[]
    for number,name in enumerate(ORDER,1):
        actual=SCHEMA[name];old=OLD.get(name,[]);oldmap={x['name']:x for x in old}
        columns=list(migration[name])
        missing=[x for x in columns if x not in oldmap]
        excess=[x for x in oldmap if x not in actual['columns']]
        diffs=[x for x in columns if x in oldmap and any(actual['columns'][x][k]!=oldmap[x][k] for k in ['type','null'])]
        title=f'T{number:02} — {name}: đối chiếu chi tiết bảng'
        body=f'# {title}\n\n'+COMMON
        body+=f'**Vị trí nguồn:** mục 6.2, bảng {number:02} — {name}.\n\n' if old else '**Vị trí nguồn:** chưa được liệt kê trong mục 6.2; cần bổ sung bảng kỹ thuật, không thêm màn hình nghiệp vụ.\n\n'
        body+=f'**Kết luận cấu trúc:** '+(f'{len(old)} cột trong báo cáo đều khớp tên/kiểu SQL/NULL với source hiện tại.' if old and not(missing or excess or diffs) else f'Bổ sung {len(missing)} cột của bảng hiện tại.')+' Khóa/default và quy tắc xử lý được đối chiếu riêng bên dưới; “khớp cấu trúc” không đồng nghĩa mọi mô tả nghiệp vụ đều đúng.\n\n'
        body+='## 1. Từng cột\n\n'
        rows=[]
        for col in columns:
            info=actual['columns'][col];r=oldmap.get(col)
            roles=[]
            for key in actual['keys']:
                if col in key['columns']:roles.append(key['kind']+': '+key['name'])
            if info['identity']:roles.append('IDENTITY(1,1)')
            for fk in actual['fks']:
                if col in fk['columns']:roles.append('FK: '+fk['name'])
            if col in actual['defaults']:roles.append('DEFAULT '+actual['defaults'][col])
            if col=='row_version':roles.append('Concurrency token tự sinh')
            before=(r['type']+'; NULL='+r['null']+'; '+r['key']) if r else 'Chưa có'
            now=info['type']+'; NULL='+info['null']+'; '+('; '.join(roles) or 'Không default/PK/FK riêng')
            status='Khớp tên/kiểu/NULL' if r and col not in diffs else 'Bổ sung' if not r else 'Cần sửa'
            rows.append((col,before,now,status))
        body+=table(['Cột','Báo cáo 6.2','Source hiện tại','Đánh giá'],rows)
        body+='\n`NULL=Y` là nullable. rowversion tương ứng SQL catalog type timestamp (8 byte), không cột ngày giờ. “Không default” không có nghĩa app không cung cấp giá trị server. SQL default và giá trị do service nhập là hai cơ chế khác nhau.\n\n'
        body+='## 2. Khóa, indexes và CHECK thực tế\n\n'
        body+=table(['Tên','Loại','Cột/điều kiện'],[(k['name'],k['kind'],', '.join(k['columns'])) for k in actual['keys']])
        nonkeys=[x for x in actual['indexes'] if not(x['pk'] or x['constraint'])]
        body+='\n'+(table(['Index','Unique','Cột','Filter'],[(x['name'],'Có' if x['unique'] else 'Không',', '.join(x['columns']),x['filter'] or 'Không filtered') for x in nonkeys]) if nonkeys else 'Không có index riêng ngoài index của PK/AK trên bảng này.\n')
        body+='\n'+(table(['CHECK','Predicate SQL'],[(x['name'],x['sql']) for x in actual['checks']]) if actual['checks'] else 'Bảng không có CHECK đơn bảng riêng; không suy ra là không có kiểm tra nghiệp vụ.\n')
        body+='\n## 3. Mô tả và nghiệp vụ cần giữ/sửa\n\n'+'\n'.join('- '+n for n in NOTES[name])
        body+='\n\n## 4. Nguồn và liên kết\n\n'
        body+=f'- [Configuration của bảng](../../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/{name}Configuration.cs).\n'
        body+='- [InitialSchema](../../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs).\n- [ExpectedCatalog](../../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json).\n- [DATABASE_SPEC](../../../design/DATABASE_SPEC.md), từ điển tương ứng.\n- [Tổng hợp mục 6.2](../6_2_CHI_TIET_BANG.md); [README](../README.md).\n'
        file=f'tables/T{number:02}_{name}.md';(OUT/file).write_text(body,encoding='utf-8')
        summaries.append((f'T{number:02}',name,len(old),len(columns),'Khớp tên/kiểu/NULL' if old and not diffs and not missing and not excess else 'Thiếu bảng',f'[{name}]({file})'))
    return summaries

def section_logic():
    rows=[
      ('ScholarshipGrant / ScholarshipCandidate / ScholarshipCriteria','Cả ba còn trong hình','Không có trong schema hoặc phạm vi requirements','Sai phạm vi; bỏ cả bảng và liên kết.'),
      ('Attempt → Session','Đường nối trực tiếp','Registration → Attempt → Booking → Session','Thay đường nối; Attempt không có session_id.'),
      ('StudentProfile → Course','Đường nối trực tiếp','Liên hệ qua Registration.student_id/course_id','Không vẽ thành FK trực tiếp.'),
      ('Course → Band','Đường nối trực tiếp trong hình','Course/Band là hai FK của TrainingProgram','Đặt TrainingProgram làm liên kết theo cặp Course/Band.'),
      ('Assignment → StudentProfile','Đường nối trực tiếp','Assignment.registration_id → Registration.student_id → Profile','Thay bằng đường qua Registration.'),
      ('AuthSession','Chưa có','Phiên → Account','Bổ sung bảng và quan hệ.'),
      ('EntryTestBooking','Chưa có','Booking → Attempt/Session và FK result ngược Attempt','Bổ sung; phân biệt lượt logic với lịch hẹn.'),
      ('AuditLog','Chưa có','Actor Account optional, operation/entity correlation','Bổ sung; không FK giả entity_id tới mọi bảng.'),
      ('ReplayOperation','Chưa có','Receipt → Account actor','Bổ sung bảng kỹ thuật, không màn hình mới.'),
      ('InvoiceNumberCounter','Chưa có','Bộ cấp số theo năm, không FK sang Invoice','Bổ sung riêng; liên hệ chức năng không là FK.')]
    body='''## 1. Kết luận

**Hình 6.1 cần vẽ lại**, không chỉ đổi vài nhãn. Hình còn ba bảng học bổng ngoài phạm vi và thiếu năm bảng trong schema hiện tại. Mục 6.2 đã dùng 22 bảng nghiệp vụ mới nên không đồng nhất với hình này dù cùng có thể thấy 22 hộp bảng.

[Ảnh gốc trích từ 6.1](report-268-0.png). Việc trích ảnh không sửa ảnh hay Word nguồn.

## 2. Đối chiếu từng nhóm quan hệ

'''+table(['Nội dung','Trong hình báo cáo','Hiện tại','Sửa báo cáo'],rows)
    body+='''
Các đường nối nhận xét trên dựa vào hình đã xem; không dùng chúng để suy ra PK/FK vật lý chưa ghi. Danh mục 50 FK chính xác có ở [6.3.4](6_3_4_RANG_BUOC_THAM_CHIEU.md).

## 3. Đường dữ liệu nghiệp vụ đúng để vẽ

```mermaid
flowchart LR
  P[StudentProfile] --> R[Registration]
  C[Course] --> R
  R --> A[EntryTestAttempt]
  A --> B[EntryTestBooking]
  S[EntryTestSession] --> B
  R --> BA[BandAssignment]
  R --> I[Invoice]
  I --> Pay[Payment]
  R --> CA[StudentClassAssignment]
  CL[Class] --> CA
  CA --> G[GradeReport]
```

Đây là sơ đồ đường dữ liệu đọc từ cha tới con để giải thích, **không là hướng FK hoặc mô hình đủ 24 bảng**. Khi vẽ schema, dùng bảng con → bảng cha theo cột ở 6.3.4. Best/Band còn tham chiếu Attempt/Band; Class tham chiếu Program/Teacher; các FK actor cần thể hiện hoặc tách trang.

## 4. Bố cục nên dùng khi thay hình

1. Tài khoản: Role, Account, StudentProfile, AuthSession.
2. Cấu hình: Course, Band, TrainingProgram, ScoreComponentConfig.
3. Đăng ký/đầu vào: Registration, EntryTestSession, EntryTestAttempt, EntryTestBooking, BandAssignment.
4. Đào tạo: Class, StudentClassAssignment, Schedule, Attendance, Score, GradeReport.
5. Tài chính: Invoice, Payment, InvoiceNumberCounter.
6. Xuyên suốt: AuditLog, ReplayOperation.

Chú giải phân biệt quan hệ FK với xử lý cùng giao dịch hoặc operation correlation. Tránh nối InvoiceNumberCounter→Invoice hay AuditLog.entity_id→mọi bảng như FK. Đánh dấu các quan hệ 0..1 từ nullable/unique và dùng note cho nghĩa vụ “phải tạo cùng transaction”.

## 5. Đoạn thuyết minh thay thế

Schema hiện tại gồm 24 bảng do ứng dụng quản lý: 22 bảng nghiệp vụ và hai bảng kỹ thuật ReplayOperation, InvoiceNumberCounter. Học viên đăng ký khóa qua Registration; mỗi hồ sơ có tối đa hai lượt thi logic EntryTestAttempt, mỗi lượt có lịch sử EntryTestBooking liên kết ca thi. Duyệt hồ sơ phát hành Invoice; xác nhận thu tiền tạo Payment. Học viên đủ điều kiện được xếp qua StudentClassAssignment gắn Registration với Class, đồng thời tạo GradeReport DRAFT. Lịch có occurrence và nghĩa vụ buổi riêng để giữ lịch sử thay thế. Tài khoản/phiên, audit và replay phục vụ kiểm quyền, lịch sử và giao dịch, không bổ sung các chức năng học bổng.
'''
    write('6_1_SO_DO_LOGIC.md','6.1 — Đối chiếu sơ đồ logic',body)

def section_tables(summaries):
    body='''## 1. Kết luận

**Không cần thay toàn bộ từ điển 6.2.** Đối chiếu từng cột cho thấy 22 bảng/217 cột đã có đều khớp **tên cột, kiểu SQL và NULL** với migration/catalog hiện tại. Vấn đề chính là thiếu hai bảng kỹ thuật và mô tả chưa đầy đủ PK/AK/index/CHECK/default hoặc còn chữ “đề xuất”. Điều này khác hai hình 6.1/6.4 và các đoạn ràng buộc cũ.

Cần bổ sung ReplayOperation (10 cột), InvoiceNumberCounter (3 cột), đưa tổng thành **24 bảng/230 cột**. Không sửa dữ liệu vì thấy từ điển thiếu bảng.

## 2. Đối chiếu từng bảng

'''+table(['Mã','Bảng','Cột báo cáo','Cột hiện tại','Đánh giá cấu trúc','File chi tiết'],summaries)
    body+='''
Mỗi file chi tiết ghi đủ cột, PK/AK, identity, default, indexes/filter, CHECK và các mô tả cần sửa/giữ. Không nhầm timestamp trong SQL catalog với cột thời gian: timestamp ở đây là rowversion.

## 3. Những bổ sung chung

| Nội dung | Cách cập nhật |
| --- | --- |
| Trạng thái “đề xuất” | Các kiểu/length/default dưới đây đã có trong source migration/configuration; gọi là thiết kế triển khai hiện hành, không các đề xuất chưa lựa chọn. |
| UNIQUE/index | Ghi tên/cột/filter thật, nhất là unfinished Registration, Booking current/completed, Schedule current obligation/exam và Program FINAL_EXAM. |
| Collation | Model dùng Latin1_General_100_CI_AS_SC; các CHECK enum/mã có BIN2 riêng. Đừng suy enum được nhập mọi kiểu hoa-thường vì collation tên mặc định CI. |
| Chuỗi Unicode | SQL nvarchar capacity và giới hạn code point của validation là hai tầng; không đổi nvarchar(200) thành 100 chỉ vì họ tên hạn100 code point. |
| Thời gian | Sự kiện UTC datetime2(3); ngày/giờ lịch VN date/time(0) theo phút. Hạn nhập ca/root schedule future, HELD từ end là service guard, không cứ đọc là ghi. |
| Dữ liệu thiếu | Score/Attendance chưa nhập là thiếu row hoặc NULL result; không default0/Vắng. |
| FK actor | Chỉ chứng minh tài khoản tồn tại; quyền/role/ownership/current task do transaction/service kiểm. |
| Bảng kỹ thuật | ReplayOperation không cache credential/PII; InvoiceNumberCounter không endpoint reset, không FK sang Invoice. |

## 4. Bộ số đếm dùng trong báo cáo

- 24 bảng/230 cột do ứng dụng quản lý; 24 PK, 2 AK, 50 FK, 70 CHECK, 20 rowversion.
- 78 indexes ngoài indexes của PK/AK; trong 78 có 28 unique indexes và 50 non-unique indexes. Tổng indexes vật lý của 24 bảng là 104 = 78 + 24 PK + 2 AK.
- 21 default constraints được ghi trong catalog nguồn.
- Nếu tính cả bảng framework __EFMigrationsHistory: thêm một bảng, hai cột và một PK/index. Không ghi 25/232/105 như số bảng/cột/index nghiệp vụ.

Số đếm này kiểm từ source/canonical catalog và đối chiếu migration; không là kết quả đo live SQL mới.
'''
    write('6_2_CHI_TIET_BANG.md','6.2 — Đối chiếu chi tiết các bảng dữ liệu',body)

def section_domains():
    rows=[
      ('Role','STUDENT/ADMISSIONS/ACADEMIC_AFFAIRS…','Student/Admissions/AcademicAffairs/Teacher/Accountant/Director/Admin; CK_Role_Code dùng BIN2','Sai literal; thay toàn bộ tập role_code.'),
      ('Account','Email hợp lệ; ACTIVE/LOCKED/DISABLED; count≥0; mô tả khóa theo đếm sai','Enum/count khớp; thêm security_version>0/phone/temp bundle. Login throttle riêng, không manual LOCKED','Khớp một phần; sửa giải thích đếm sai/khóa.'),
      ('Course','base_price≥0; ACTIVE/INACTIVE','CK_Course_Price: base_price>0; enum khớp','Sai biên tiền; 0 không hợp lệ.'),
      ('Band','min_score0–100, không max_score','CK_Band_MinScore khớp, ngưỡng min lớn nhất≤score; seed/freeze','Khớp; thêm first-result freeze và seed.'),
      ('TrainingProgram','DRAFT/ACTIVE/INACTIVE, không Director approval','Enum khớp; expected_lesson_count>0; activation/freeze ở service/trigger','Khớp; bổ sung count và tầng enforcement.'),
      ('ScoreComponentConfig','weight>0≤100','CK_Weight khớp; thêm kind GENERAL/LESSON/FINAL_EXAM và component name','Thiếu miền kind.'),
      ('Registration','Bảy trạng thái đã chốt','PENDING/TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED/REJECTED/CANCELLED; is_unfinished bit riêng','Khớp; không thêm COMPLETED hoặc trạng thái Đã xếp lớp.'),
      ('EntryTestSession','capacity1–20, OPEN/CLOSED/CANCELLED','Khớp; ca mới CLOSED; start/end phút UTC cùng ngày VN, room/membership/current grader guard','Khớp; thêm effective CLOSED từ start.'),
      ('EntryTestAttempt','attempt1/2; status SCHEDULED/COMPLETED/ABSENT/CANCELLED','Attempt chỉ number/score/bundle. Status nằm EntryTestBooking','Sai đối tượng; chuyển status sang Booking.'),
      ('EntryTestBooking','Chưa có mục miền riêng','SCHEDULED/COMPLETED/ABSENT/CANCELLED, bundle absence/cancel và indexes theo Attempt','Thiếu mục; bổ sung.'),
      ('Class','max1–15, PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED','Khớp CK; lifecycle guards riêng, no approval state','Khớp.'),
      ('Schedule','LESSON/FINAL_EXAM','Thêm PLANNED/HELD/NOT_HELD, is_current/obligation, minute/window rules','Thiếu occurrence state và nghĩa vụ.'),
      ('Attendance','present0/1','bit bắt buộc, không default; thiếu row không Vắng','Khớp; nêu rõ missing khác0.'),
      ('Score','0–100','Khớp CK decimal(5,2); input dư2 số lẻ bị chặn trước lưu; kind/schedule guard','Khớp; không để SQL convert làm tròn input sai.'),
      ('GradeReport','DRAFT/SUBMITTED/RETURNED/FINALIZED, final0–100','Khớp enum/range; final_score NULL trước FINALIZED, total draft tính khi đọc','Khớp; thêm điều kiện lưu final.'),
      ('Invoice','amount≥0; UNPAID/PAID/CANCELLED','amount>0; enum khớp, number pattern/due+7; overdue không state','Sai biên tiền.'),
      ('Payment','amount_paid≥0; CASH/BANK_TRANSFER','amount_paid>0; CASH/TRANSFER; reference khi TRANSFER','Sai biên và literal phương thức.'),
      ('AuditLog','action/entity/time bắt buộc, actor NULL khi tự động, IP hợp lệ','actor NULL khi chưa xác thực; thêm SUCCESS/FAILURE, MANDATORY/SECURITY, JSON và retention bundles','Cần sửa ý nghĩa actor NULL và bổ sung miền kỹ thuật.'),
      ('AuthSession','Chưa có mục miền riêng','NORMAL/PASSWORD_CHANGE, temporal bounds, binary32 SID, security version; auth deadline/idle ở service','Thiếu; bổ sung.'),
      ('ReplayOperation','Chưa có','SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE, JSON result_reference, expires≥completed+24h','Thiếu bảng và miền.'),
      ('InvoiceNumberCounter','Chưa có','issue_year1–9999; last_value0–99.999.999, initial0','Thiếu bảng và miền.')]
    body='## 1. Đối chiếu các miền giá trị\n\n'+table(['Đối tượng','Báo cáo 6.3.1','Hiện tại','Kết luận/sửa'],rows)
    body+='''
## 2. Cách phân tầng kiểm tra khi viết lại

1. SQL type/NOT NULL/CHECK bảo vệ range, enum hoặc bundle trên row.
2. Validation kiểm định dạng email/mobile VN, Unicode/NFC/code point và precision input; capacity nvarchar không thay validator.
3. Service transaction kiểm role/ownership/current task/trạng thái liên bảng/cutoff thời gian.
4. Trigger bảo vệ các invariant cụ thể và đòi caller đã giữ gate; không tự kiểm tất cả quyền hoặc xử lý workflow.

Không nói “SQL CHECK bảo đảm email đúng/Student có Profile/grader đúng Teacher” nếu predicate chỉ kiểm scalar hoặc FK. Không nói bit0 là Vắng khi row chưa tồn tại.

## 3. Câu sửa ưu tiên

- Role.role_code thuộc đúng bảy literal triển khai: Student, Admissions, AcademicAffairs, Teacher, Accountant, Director, Admin.
- Course.base_price, Invoice.amount và Payment.amount_paid là decimal(18,0) dương; counter kỹ thuật last_value được phép0, không áp quy tắc tiền dương vào counter.
- Payment.method thuộc CASH hoặc TRANSFER; TRANSFER cần transaction_reference.
- EntryTestAttempt.attempt_number thuộc1/2; trạng thái lịch hẹn SCHEDULED/COMPLETED/ABSENT/CANCELLED thuộc EntryTestBooking.status. Một Attempt có kết quả khi bundle result đầy đủ và trỏ completed Booking đúng lượt.
- Audit actor_account_id nullable cho ngữ cảnh chưa xác thực; sự kiện tự động không phải lý do để tùy tiện bỏ actor đã xác minh của action nguồn.

Predicate CK thật của mỗi bảng có trong [các file bảng](6_2_CHI_TIET_BANG.md), lấy trực tiếp source catalog để không viết lại sai tên/biên.
'''
    write('6_3_1_RANG_BUOC_MIEN_GIA_TRI.md','6.3.1 — Đối chiếu ràng buộc miền giá trị',body)

def section_bundles():
    rows=[
      ('Account','locked_until chỉ khi LOCKED, timer khóa tạm','Không có locked_until. LoginThrottledUntil là timer login độc lập với ACTIVE/LOCKED/DISABLED','Sai cột/mô hình; thay bằng failed-login window/throttled-until và temp bundle.'),
      ('EntryTestSession','end_at>start_at','Khớp; CK_Time còn kiểm phút và cùng ngày Việt Nam','Giữ và bổ sung chính xác cửa sổ/lý do khi hủy.'),
      ('Registration','ApprovedBundle chỉ ADMITTED; confirmation trước/ sau; không đề cập giữ sau hủy','CK ApprovedBundle chỉ buộc by/at cùngNULL/cùngcó; workflow giữ confirmation/approval khi CANCELLED sau ADMITTED. Rejected/Cancelled có full bundle/prior-state/operation','Không xóa lịch sử theo status hiện tại; tách CK với state machine service.'),
      ('EntryTestAttempt','status COMPLETED buộc score/by/at, trạng thái khác NULL','Không status. CK_ResultBundle: score/by/at/completed_booking_id cùngNULL hoặc cùngcó','Thay bằng bốn trường; trạng thái COMPLETED kiểm trên Booking liên quan.'),
      ('Schedule','end_time>start_time','Khớp nhưng thiếu seconds=0, current/state, held/not-held bundles và replacement metadata','Bổ sung đầy đủ; thay thế không xóa source occurrence.'),
      ('GradeReport','Submit/review required theo state; FINALIZED reason phảiNULL','Submit/review bundle gồm operation IDs. FINALIZED cần score/finalized/review, finalizedAt=reviewedAt; ReturnedReason chỉ bắt buộc khi RETURNED, không CHECK épNULL ở FINALIZED','Sửa rule reason; metadata gần nhất có thể giữ qua resubmit/finalize.'),
      ('Invoice','due_date không trước ngày issue','Service issue lấy ngày VN+7 lịch, không chỉ≥issue; CK invoice còn number/amount/cancel bundle','Nêu +7 lịch và snapshot, không suy CK đơn bảng đảm bảo workflow.'),
      ('Payment','BANK_TRANSFER cần reference','TRANSFER cần reference; correction by/at/reason cùngNULL hoặc cùngcó; amount>0','Sửa enum, thêm correction bundle và bất biến paid/link/amount.')]
    body='## 1. Đối chiếu tám nhóm trong báo cáo\n\n'+table(['Bảng','Báo cáo 6.3.2','Hiện tại','Cách sửa'],rows)
    body+='''
## 2. Các bundle/cặp thuộc tính bị bỏ sót

- Account.must_change_password=true: temp_issued_at/temp_expires_at cùng có và expires=issued+24h; false: hai mốc NULL.
- AuthSession: issued≤last_activity≤expires, expires>issued, revoke NULL hoặc≥issued; loại scope và role authority độc lập.
- EntryTestBooking: ABSENT có absent_by/at/operation; CANCELLED có cancel actor/time/prior SCHEDULED/operation; giữ các bundle theo đúng source state.
- Class/EntryTestSession/Invoice: cancellation bundles đầy đủ với reason/operation/prior-state theo từng CHECK.
- Schedule: current=1 cho PLANNED/HELD, current=0 cho NOT_HELD; HeldBundle và NotHeldBundle tách nhau; no self replacement.
- AuditLog: SECURITY có security expiry≥occurred+90d, MANDATORY expiryNULL; IP có thì IP expiry đúng+90d; details JSON khi có.
- ReplayOperation: JSON result reference, mode đúng và expiry≥completed+24h.

## 3. Predicate CHECK cụ thể để cập nhật

'''
    bundles=[(name,c['name'],c['sql']) for name,t in SCHEMA.items() for c in t['checks']
             if any(x in c['name'] for x in ['Bundle','FinalResult','Submit','Review','ReturnedReason','Lifetime','RevokedAt','CurrentState','NotHeld','HeldBundle','SecurityExpiry','IPBundle','Expiry','Time','NoSelf'])]
    body+=table(['Bảng','CHECK','Predicate đang có'],bundles)
    body+='''
SQL CHECK chứa NULL phải dùng IS NULL/IS NOT NULL rõ; SQL UNKNOWN không thay guard. Thời điểm server, role của actor và trạng thái parent là điều kiện workflow/service, không chỉ bundle CHECK. Không nhận actor/time/client tự khai để bỏ kiểm phiên.

## 4. Ví dụ sửa thuyết minh

Thay đoạn “EntryTestAttempt.status=COMPLETED…” bằng: EntryTestAttempt lưu kết quả đầu vào gồm score, graded_by, graded_at và completed_booking_id; bốn trường cùng NULL khi chưa có kết quả hoặc cùng có khi đã chấm. Foreign key ghép bảo đảm completed Booking thuộc đúng Attempt; service kiểm booking COMPLETED và quyền grader. ABSENT không tạo score0.

Thay đoạn “FINALIZED phải xóa returned_reason” bằng: RETURNED bắt buộc có lý do; khi gửi lại/chốt, dữ liệu review/submit gần nhất và latest return reason được giữ theo implementation, audit giữ lịch sử các lần. FINALIZED yêu cầu final_score/finalized_at và review bundle đầy đủ, không mở lại.
'''
    write('6_3_2_RANG_BUOC_LIEN_THUOC_TINH.md','6.3.2 — Đối chiếu ràng buộc liên thuộc tính trong một bộ',body)

def section_uniques():
    oldrows=[]
    meanings={
      'Role.role_code.':'Khớp UQ_Role_Code; literal role_code sửa ở 6.3.1.',
      'Account.email.':'Khớp UQ_Account_Email, CI collation + normalization, kể cả LOCKED/DISABLED.',
      'Account.phone khi có giá trị.':'Khớp filtered UQ_Account_Phone WHERE phone IS NOT NULL.',
      'StudentProfile.account_id.':'Khớp UQ_StudentProfile_Account; service thêm rule chỉ Student.',
      'Course.course_code.':'Khớp UQ_Course_Code; bất biến không tái dùng.',
      'Band.band_code.':'Khớp UQ_Band_Code.',
      'Band.min_score.':'Khớp UQ_Band_MinScore.',
      'Class.class_code.':'Khớp UQ_Class_Code, kể cả CANCELLED.',
      'Invoice.invoice_number.':'Khớp UQ_Invoice_Number; allocator không dùng invoice_id làm số.'}
    for i in range(406,428):
        text=txt(BODY[i])
        if not text:continue
        if i==419:result='Không có standalone UQ best_attempt_id. Tính duy nhất về nghĩa được suy từ FK (best_attempt_id,registration_id) + unique registration_id; không thêm index giả vào danh mục.'
        elif i==426 or i==427:result='Đúng hướng nghiệp vụ nhưng không là UNIQUE đơn bảng; kiểm khoảng [start,end), resource/gates và dữ liệu liên bảng trong transaction. Bổ sung Student và test-versus-class overlap.'
        elif i==415:result='Khớp UQ_Program_CourseBand trên mọi trạng thái. Không version/new Program cùng cặp để né INACTIVE/freeze.'
        elif i==416:result='Tên component unique trong Program bằng UQ_Component_ProgramName. Tổng weight100 thuộc activation transaction, không cùng loại UNIQUE; thêm filtered unique tối đa1 FINAL_EXAM.'
        elif i==417:result='Khớp UQ_Attempt_RegistrationNumber + CHECK number1/2; lượt2 cần lượt1 completed score và trướcCONFIRMED ở service, không chỉ tồn tại row1.'
        elif i==418:result='Khớp UQ_BandAssignment_Registration.'
        elif i==420:result='Khớp UQ_Assignment_Registration; không transfer/delete assignment qua UI.'
        elif i==421:result='Khớp UQ_Attendance_ScheduleAssignment; thiếu record khác Vắng.'
        elif i==422:result='Khớp UQ_Score_AssignmentComponent; component/link guards khác UNIQUE.'
        elif i==423:result='Khớp UQ_GradeReport_Assignment; service placement còn tạo reportDRAFT cùng commit.'
        elif i==424:result='Khớp UQ_Invoice_Registration; issuance cùng admission approval.'
        elif i==425:result='Khớp UQ_Payment_Invoice; đủamount/fullpayment ở service transaction.'
        else:result=meanings.get(text,'Đối chiếu danh mục unique cụ thể bên dưới.')
        oldrows.append((text,result))
    body='''## 1. Kết luận

Phần lớn uniqueness trong báo cáo khớp nhưng chưa đủ filtered unique/AK thực tế. Hai dòng chống chồng lịch và dòng tổng weight100 đang đặt lẫn với UNIQUE; chúng là invariant/guard khác. Best_attempt_id không có một UNIQUE index riêng dù cardinality tương ứng được suy ra từ các ràng buộc phối hợp.

## 2. Đối chiếu từng dòng của phần 6.3.3

'''+table(['Báo cáo','Đối chiếu/cách sửa'],oldrows)
    body+='\n## 3. Danh mục unique indexes hiện tại (ngoài PK/AK)\n\n'
    indexes=[(name,i) for name,t in SCHEMA.items() for i in t['indexes'] if i['unique'] and not(i['pk'] or i['constraint'])]
    assert len(indexes)==28
    body+=table(['Bảng','Index','Cột','Filter'],[(name,i['name'],', '.join(i['columns']),i['filter'] or 'Không filtered') for name,i in indexes])
    body+='\n## 4. Alternate keys cho hai FK ghép\n\n'
    aks=[(name,k) for name,t in SCHEMA.items() for k in t['keys'] if k['kind']=='UQ']
    assert len(aks)==2
    body+=table(['Bảng','AK','Cột','Lý do'],[(name,k['name'],', '.join(k['columns']),
                'Principal key cho completed_booking_id/attempt_id' if name=='EntryTestBooking' else 'Principal key cho best_attempt_id/registration_id') for name,k in aks])
    body+='''
## 5. Những UNIQUE không nên tự bổ sung

- Không permanent unique Registration(student_id,course_id): sẽ chặn học lại sau lượt trước đã kết thúc. Dùng filtered is_unfinished=1 hiện tại.
- Payment.transaction_reference không unique; mã giao dịch/tiền mặt có hợp đồng validation và correction riêng.
- Không unique Schedule(class_id,session_type) cho toàn history: có nhiều LESSON và các replacement. Dùng current obligation/current exam/predecessor đúng filter.
- Không unique score_value, BandAssignment.best_attempt_id riêng hoặc bảng StudentClassAssignment(student_id) khi không có cột student_id.
- Capacity/max_size và overlaps không thể thay bằng unique indexes đã liệt kê. Kiểm trước read chưa đủ chống hai lệnh tranh chỗ cuối.

Ràng buộc SQL bảo vệ cấu trúc, service bảo vệ trạng thái và thời điểm; không mô tả index như tác nhân tự chọn ca/lớp khi conflict.
'''
    write('6_3_3_RANG_BUOC_LIEN_BO.md','6.3.3 — Đối chiếu ràng buộc liên bộ trong cùng quan hệ',body)

def section_fks():
    rows=[]
    for old in OLDFK:
        now=matches[fksig(old)]
        child=f'{old["child"]}({", ".join(old["columns"])})'
        parent=f'{old["parent"]}({", ".join(old["parent_columns"])})'
        null=', '.join(c+'='+SCHEMA[old['child']]['columns'][c]['null'] for c in old['columns'])
        rows.append((old['number'],child+' → '+parent,now['name'],null,'Khớp endpoints; NO ACTION delete/update'))
    body='''## 1. Kết luận

**49 quan hệ tham chiếu được viết bằng chữ trong báo cáo đều khớp endpoints/cột với source hiện tại**, kể cả hai FK ghép. Không cần xóa/vẽ lại toàn bộ danh mục này vì hai hình cũ sai. Schema có **50 FK**; quan hệ bị thiếu là ReplayOperation.actor_account_id → Account.account_id.

FK catalogue nguồn ghi delete/update NO ACTION, không disabled/untrusted. Đây là đối chiếu baseline nguồn, không là truy vấn trạng thái FK live mới.

## 2. Đối chiếu từng FK trong báo cáo

'''+table(['STT báo cáo','Quan hệ','Tên FK hiện tại','NULL phía con','Kết quả'],rows)
    body+='\n## 3. Quan hệ phải bổ sung\n\n'
    f=MISSINGFK[0]
    body+=table(['Quan hệ','Tên constraint','NULL','Delete/update'],[
       ('ReplayOperation(actor_account_id) → Account(account_id)',f['name'],'N','NO ACTION / NO ACTION')])
    body+='''
InvoiceNumberCounter không có FK sang Invoice. Không thêm giả FK AuditLog(entity_id), ReplayOperation(result_reference) hoặc các operation_id sang một bảng business để làm đủ số quan hệ.

## 4. Hai FK ghép cần ghi/vẽ đúng

1. EntryTestAttempt(completed_booking_id,attempt_id) → EntryTestBooking(booking_id,attempt_id): completed_booking_id nullable, attempt_id bắt buộc. AK_Booking_IdAttempt là principal key; CK result bundle buộc link có cùng score/by/time. FK chứng minh cùng lượt, **không** chứng minh Booking.status COMPLETED: service kiểm điều đó.
2. BandAssignment(best_attempt_id,registration_id) → EntryTestAttempt(attempt_id,registration_id): cả hai bắt buộc. AK_Attempt_IdRegistration chặn chọn Attempt của hồ sơ khác. Best completed/highest/tie-break/threshold là service/FN02, không chỉ FK.

Không vẽ thêm scalar FK trùng composite chỉ để tăng số constraint. Vòng Attempt↔Booking được tạo theo thứ tự Attempt chưa kết quả → Booking → result, không tự cascade insert hai row.

## 5. Bảng tầm ảnh hưởng Thêm/Xóa/Sửa

Quy ước bảng con/cha trong báo cáo vẫn dùng được nếu cập nhật tên cột theo danh mục trên:

- INSERT/UPDATE FK phía con: kiểm parent hoặc link ghép tồn tại; nullable đi kèm CHECK bundle tương ứng.
- DELETE/UPDATE referenced key phía cha: NO ACTION chặn nếu còn tham chiếu; không cascade hoặc SET NULL làm mất lịch sử.
- DELETE row con không bị chính FK đó chặn, nhưng có thể bị FK khác trỏ vào, trigger/permission hoặc service policy; dấu “−” không đồng nghĩa ứng dụng có nút xóa.
- Mọi Account actor FK chỉ chứng minh Account tồn tại. Role ACTIVE/quyền/current task/ownership phải kiểm theo thao tác; không đặt CHECK buộc actor lịch sử luôn ACTIVE vì sau này tài khoản có thể bị khóa.

## 6. Các ghi chú phải thống nhất với phần khác

Danh mục FK của báo cáo đã dùng Booking/Registration và các trường reviewer mới, trong khi hình 6.4 còn session_id/student_id/approved_by cũ. Lấy danh mục FK này và source column ở 6.2 làm chuẩn để vẽ lại hình. actor nullable của AuditLog là ngữ cảnh chưa xác thực; operational retention security/IP không phải ứng dụng được purge mandatory history.
'''
    write('6_3_4_RANG_BUOC_THAM_CHIEU.md','6.3.4 — Đối chiếu ràng buộc tham chiếu và bảng tầm ảnh hưởng',body)

def section_relational():
    data=[
      (974,974,'Sai vị trí status; bổ sung tie-break','Attempt không có status; kết quả đầy đủ + completed_booking đúng lượt/trạng thái và highest completed score. Hòa chọn Attempt1.','FK ghép + result bundle + TeacherEntryService; best selection trong transaction.'),
      (975,975,'Khớp; bổ sung freeze','Band có min_score lớn nhất≤best score; seed0/25/45/65/85, first completed result freeze set.','FN02 ufn_BandForScore + BANDSET gate/TG03 + service.'),
      (976,976,'Khớp; cần atomic','First score/result/best/Band cùng chuyển BAND_ASSIGNED; điểm lượt2 giữ BAND_ASSIGNED.','TeacherEntryService + Registration/BandAssignment transaction.'),
      (977,977,'Khớp nhưng thiếu guard lượt chờ','Student own BAND_ASSIGNED có Band, không lượt SCHEDULED/taken-ungraded; CONFIRMED chưa Invoice.','StudentEnrollmentService current result/booking/RV guards.'),
      (978,978,'Khớp','CONFIRMED có Band và current Admissions NORMAL; literal role Admissions.','AdmissionsService kiểm role/state/versions.'),
      (979,979,'Khớp; bổ sung counter/due','ADMITTED + một Invoice UNPAID + counter + audit/replay cùng giao dịch, học phí snapshot tại issue và ngày VN+7.','UQ_Invoice_Registration/Number + SP07 allocator + AdmissionsService.'),
      (980,980,'Khớp một phần; cần current task/time','Teacher current grader; initial score/ABSENT từ ca end; correction original grader vẫn assigned, reason/trướcCONFIRMED.','TeacherEntryService; FK actor không kiểm role/time.'),
      (981,981,'Sai đối tượng đếm','Đếm EntryTestBooking SCHEDULED của Session, không EntryTestAttempt trong ca. Booking CANCELLED không chiếm chỗ; attempts không có session_id.','SqlStudentEnrollmentRepository capacity recheck/gates.'),
      (982,982,'Khớp; lịch sử không phụ thuộc ACTIVE mãi','Teacher role/ACTIVE khi phân công; Account bị block không tự đổi assignment.','ClassService/Teacher guards; FK chỉ existence.'),
      (983,989,'Sai cho phép ONGOING; thiếu guard','Placement chỉ Class OPEN trước buổi đầu; ADMITTED/PAID/sameCourseBand, Student ACTIVE/current Teacher hợp lệ/còn chỗ/không trùng, chưa assignment.','ClassPreviews.Placement + ClassService + UQ_Assignment_Registration.'),
      (990,990,'Khớp; bổ sung scope/gates','Domain gates/state locks, Account/SID authority, phiên bản/manifests, deadline và commit recheck; không chỉ check capacity ở form.','Enrollment/Class executors và transaction repositories; không riêng FK/UNIQUE.'),
      (991,991,'Thiếu cross-type/Student','Half-open [start,end); phòng/Teacher/Class và Student. Kiểm cả ca thi với lớp, occurrence current và booking effective phù hợp.','SqlClassTimetable/SqlSchedulePreviews + transaction/gates.'),
      (992,992,'Khớp; bổ sung Student/half-open','Ca thi đối chiếu phòng/grader với ca/lớp; Student booking đối chiếu test-and-class lịch; end=start không overlap.','EntrySession resource guards + Student timetable guards.'),
      (993,993,'Khớp nhưng thiếu current/window','Attendance chỉ current LESSON không NOT_HELD; từ start đến trước Class COMPLETED, current Teacher.','TG04 relation/ended Class + AttendanceService time/authority.'),
      (994,994,'Khớp','Schedule.ClassId=Assignment.ClassId; FK từng cột riêng không chứng minh bằng nhau.','TG04 và service validation.'),
      (995,995,'Khớp; thêm kind/link','Component cùng Class.Program; GENERAL scheduleNULL, LESSON/FINAL_EXAM current cùngClass/type khôngNOT_HELD.','TG05 và GradeService/GradeWire.ScheduleValid.'),
      (996,996,'Sai ngoại lệ override','Chỉ current assigned Teacher nhập/sửa draft trong window/state cho phép; không quyền sửa đặc biệt Admin/Giáo vụ. entered_by latest actual writer không actor tùy nhập.','GradeService authority/task/provenance; audit actual changes.'),
      (997,999,'Thiếu completeness/round/storage','Decimal weighted total trên đủ frozen components; half-up2 một lần. Thiếu điểm là Chưa đủ điểm; total draft tính khi đọc, final_score chỉ lưu khi FINALIZED; passed≥50.','FN05 ufn_GradeTotal + GradeService FINALIZE/CK_FinalResult.'),
      (1000,1000,'Thiếu whole-Class/COMPLETED','Teacher gửi toàn cohort Class COMPLETED, mọi report DRAFT/RETURNED đủđiểm/current links/versions, không unsaved edit; không auto/individual submit.','GradeWire.WorkflowEligible + GradeService transition + audit/replay.'),
      (1001,1001,'Khớp nhưng thiếu whole cohort/reason','AcademicAffairs RETURN/FINALIZE toànClass COMPLETED/toànSUBMITTED; RETURN cầnreason; FINALIZED immutable, không tier Director.','GradeService/grade preview/manifests và mandatory grouped audit.'),
      (1002,1002,'Khớp; thêm ownership và projection','Student chỉ own FINALIZED list/detail; không DRAFT/SUBMITTED/RETURNED, peer hoặc quản trị context.','Student grade reader hiện hành + current ownership/session guards.'),
      (1003,1003,'Khớp ở workflow, không CHECK tự động','Invoice PAID cùng đúngmột Payment theo transaction chuẩn; UQ_Payment_Invoice bảo vệ tốiđa1. Không chỉ FK là tự đổi PAID.','FinanceService confirmation transaction + unique.'),
      (1004,1004,'Khớp ở service','Payment.amount_paid đúng Invoice snapshot đầyđủmộtlần; both >0. CK amount>0 không chứng minh equality cross-table.','Finance transaction validation/locks.'),
      (1005,1005,'Khớp role; cần current authority','Literal Accountant, current NORMAL/ACTIVE authority; confirmedBy/At do server, giữ khi metadata correction.','FinanceService; FK existence không role.'),
      (1006,1006,'Khớp; bổ sung audit/replay','Payment + Invoice PAID + mandatory audit + completion receipt cùng commit; lỗi rollback tất cả, retry không payment2.','Finance executor/service + ReplayOperation/UQ.'),
      (1007,1007,'Khớp cho action trong danh mục, không mọi read','Mandatory audit theo action catalog trong cùng transaction. Login/logout/security-only best-effort; Attendance không AuditLog history riêng; Profile audit chỉ changedFields.','AuditPolicy/AttendanceService/SqlOwnProfileExecutor và service từng action.')]
    rows=[]
    for lo,hi,status,current,enforcement in data:
        quoted=' '.join(txt(BODY[i]) for i in range(lo,hi+1))
        rows.append((quoted,status,current,enforcement))
    body='''## 1. Đối chiếu toàn bộ các quy tắc trong 6.3.5

Các dòng danh sách placement và công thức tổng được gộp theo đúng một quy tắc để đọc được; không bỏ các điều kiện ở báo cáo. Chú ý FK/UNIQUE/CHECK đơn bảng không tự thực thi mọi quy tắc dưới đây.

'''+table(['Quy tắc trong báo cáo','Đánh giá','Hiện tại/cách sửa','Tầng thực thi'],rows)
    body+='''
## 2. Quy tắc cần bổ sung vì báo cáo còn thiếu

- Tạo assignment đồng thời tạo GradeReport DRAFT; Registration vẫn ADMITTED và Đã xếp lớp chỉ là projection. Không tạo Score/Attendance0 khi placement.
- Unfinished=0 khi REJECTED/CANCELLED hoặc lượt học Class COMPLETED + GradeReport FINALIZED, kể cả không đạt. Trước đó không đăng ký trùng Student/Course; sau kết thúc có thể tạo Registration mới, không tái dùng kết quả/Band cũ.
- Program ACTIVE cần weight100/đúng1FINAL_EXAM/positiveLESSONcount; first Class dùng freeze core/components/weights/count kể cả Class CANCELLED. Không duplicate Course/Band Program né freeze.
- Weekly schedule save all-or-none; preview không giữ chỗ/clock, candidate nào start≤now làm cả batch không lưu. Move/replacement giữ nghĩa vụ; source NOT_HELD/history, không đếm replacement thành buổi thêm.
- HELD do Giáo vụ khi now≥end và Class ONGOING/current PLANNED; Attendance/time không tự HELD. Class COMPLETE cần mọi current obligations HELD và đủrequired Attendance; chưa yêu cầu submit/finalize.
- Draft per Student khác whole-Class submit/return/finalize. Missing score khác0, missing Attendance khácVắng; full supplied validation trước no-change. GradeReport RV được touch khi Score/comment đổi thực.
- Account reset/change-password/lock/disable thu hồi phiên theo policy; unlock/restore không hồi sinh SID. Profile edit không revoke hoặc replay marker. Last ACTIVE Admin và self-lock guard thuộc transaction, không FK.
- Reports dùng consistent SNAPSHOT query; revenue theo Payment confirmation, billed noncancelled Invoice issue, debt current UNPAID. Tổng toàn filtered set; CSV query mới; Director aggregate-only.
- Unknown outcome sau gửi không đồng nghĩa rollback. Manual lookup/read state đúng quyền, không automatic resend; receipt không chứa plaintext credential.

## 3. Transaction granularity nên ghi trong báo cáo

| Thao tác | Đơn vị nguyên tử |
| --- | --- |
| Đặt/đổi/hủy booking | Attempt cần thiết + old/new Booking + Registration/chỗ + audit theo action/replay. |
| Chấm/sửa score đầu vào | Result bundle + completed Booking + best/Band/Registration/freeze + audit/replay. |
| Duyệt hồ sơ | ADMITTED + Invoice + counter + mandatory audit/replay. |
| Xác nhận thu tiền | Payment + PAID + audit/replay. |
| Xếp lớp | Assignment + DRAFT GradeReport + audit/replay. |
| Lập lịch tuần/thay buổi | Cả batch hoặc old NOT_HELD+new replacement cùng nghĩa vụ + audit/replay. |
| Điểm danh | Actual changed subset của supplied items một LESSON + enteredBy/At + replay; không AuditLog Attendance history riêng. |
| Điểm nháp | Scores/comment một Student + parent report RV + audit/replay. |
| Gửi/trả/chốt điểm | Toàn cohort reports; khi chốt thêm totals/publication/unfinished + grouped audit/replay. |
| Profile | Account/Profile cùng hai RV + changedFields audit; không replay completion hoặc revoke. |
| Quản trị credential/status | Account/securityVersion/temp state + related revocations + mandatory audit; secret không lưu vào receipt. |

Không viết “mọi bước từ đăng ký tới chốt điểm là một transaction”; đó là nhiều lượt của nhiều actor. SQL triggers đòi gate preheld nhưng không thay service kiểm current authority/clock hoặc tự triển khai workflow.
'''
    write('6_3_5_RANG_BUOC_LIEN_QUAN_HE.md','6.3.5 — Đối chiếu ràng buộc liên quan hệ và giao dịch',body)

def section_diagram():
    rows=[
      ('ScholarshipGrant/Candidate/Criteria','Có đủ ba bảng/quan hệ trong ảnh','Loại khỏi hình; không source tables và requirements ngoài phạm vi','Sai scope.'),
      ('Account.locked_until','Có trong ảnh','Không có; failure_window_started_at/login_throttled_until/security_version/temp fields/RV','Thay tập cột tài khoản.'),
      ('Band.max_score','Có trong ảnh','Không có; min_score unique và threshold selection','Bỏ max_score, giữ min_score.'),
      ('EntryTestAttempt.session_id','Có FK trực tiếp ca thi','Không có; Booking.attempt_id/session_id nối hai bảng','Bỏ session_id và thêm Booking.'),
      ('EntryTestSession','session_date/session_time, thiếugrader/state','start_at/end_at/room/capacity/grader_id/status/cancel bundle/RV','Thay bằng schema bảng 12 cột hiện tại.'),
      ('StudentClassAssignment','assignment_id/class_id/student_id','class_assignment_id/class_id/registration_id/assigned_by/assigned_at','Thay PK và FK Student bằng Registration.'),
      ('GradeReport','assignment_id/approved_by/approved_at','class_assignment_id/reviewed_by/reviewed_at/review_operation_id, submitoperation/reason/comment/finalized/RV','Bỏ approval tier; giữ reviewer hiện hành.'),
      ('Class','approved_by/approved_at','Không có; status/cancel metadata/RV; explicit lifecycle Giáo vụ','Bỏ hai cột approval.'),
      ('TrainingProgram','approved_by/approved_at','Không có; expected_lesson_count/RV/frozen configuration','Bỏ approval; thêm cấu hình đúng.'),
      ('Score','assignment_id, thiếu schedule_id/RV','class_assignment_id + component_id + schedule_id nullable theo kind + provenance/RV','Cập nhật tên/link.'),
      ('Attendance','assignment_id, thiếu entered metadata/RV','class_assignment_id/entered_by/entered_at/RV','Cập nhật tập cột 7 cột.'),
      ('Invoice','Thiếu số và issuance/cancel fields','invoice_number/issued_at/amountsnapshot/due/cancelbundle/RV','Vẽ đủ 13 cột hoặc dùng bản rút gọn có chú giải.'),
      ('Payment','Thiếu reference/correction/RV','transaction_reference/correction metadata/RV','Vẽ đủ 11 cột; methodTRANSFER.'),
      ('AuthSession/AuditLog','Chưa có','Thêm các bảng với đúng field/FK/nullability','Thiếu.'),
      ('ReplayOperation/InvoiceNumberCounter','Chưa có','Thêm hai bảng kỹ thuật; Counter không FK','Thiếu.'),
      ('Rowversion/operation IDs/filtered keys','Không thể hiện đầy đủ','Phản ánh 20 RV, 2AK và conditional indexes theo schema source','Bổ sung hoặc chú giải rút gọn rõ.')]
    body='''## 1. Kết luận

**Ảnh 6.4 là snapshot sơ đồ cũ, không phải hình của schema đang triển khai.** Không dùng ảnh này để chứng minh database hiện tại hoặc để sửa lại code cho giống báo cáo. Mục 6.2 và danh mục FK bằng chữ đã mới hơn ảnh.

[Ảnh gốc trích từ 6.4](report-1011-0.png). Chưa tạo hình thay thế trong lượt đối chiếu này.

## 2. Đối chiếu những nhóm cột/quan hệ nhìn thấy trong ảnh

'''+table(['Đối tượng','Ảnh cũ','Hiện tại','Đánh giá'],rows)
    body+='''
## 3. Danh sách phải có khi vẽ lại

Dùng đủ 24 bảng và 230 cột ở [6.2](6_2_CHI_TIET_BANG.md) và file chi tiết từng bảng. Dùng 50 FK ở [6.3.4](6_3_4_RANG_BUOC_THAM_CHIEU.md). Không gõ tay từ ảnh cũ; đặc biệt các tên assignment_id, session_date, approved_by đã không khớp model.

Các quan hệ 0..1 cần thấy rõ:

- Account có tối đa một StudentProfile; staff không có Profile theo service.
- Registration có tối đa một BandAssignment, Invoice và StudentClassAssignment; chưa đi tới bước đó thì có thể chưa có.
- Invoice có tối đa một Payment; UNPAID chưa có Payment.
- StudentClassAssignment có tối đa một GradeReport theo unique; placement tạo DRAFT cùng transaction để assignment vận hành luôn có report.
- Attempt có completed Booking optional theo result bundle; Booking lịch sử nối cùng Attempt bắt buộc, không giới hạn history bằng số logical Attempt.
- Schedule root không predecessor; mỗi predecessor có tối đa một successor; một current occurrence/obligation, tối đa một current FINAL_EXAM/Class.

Hai FK ghép phải ghi cả hai cột, không vẽ scalar link thay thế. AuthSession SID binary32 và Counter issue_year PK không IDENTITY; các PK int nghiệp vụ/Replay có IDENTITY. AuditLog actor optional; Replay actor bắt buộc.

## 4. Cách trình bày dễ đọc và đủ kiểm chứng

1. Một trang tổng thể 24 bảng, ưu tiên đường nghiệp vụ chính; actor FK nhiều đường có thể tách trang phụ.
2. Trang chi tiết từng miền: identity/session; cấu hình/entry; lớp/lịch/kết quả; finance/audit/replay. Bảng có field/PK/AK/FK/RV đầy đủ.
3. Chú giải ghi PK, FK ghép, nullable, unique/filter; không dùng một màu chung khiến optional/mandatory khó đọc khi in.
4. Dùng NO ACTION cho các FK; không cascade-delete dữ liệu lịch sử.
5. InvoiceNumberCounter đứng riêng: allocator là xử lý, không relationship FK. Audit entity/operation correlation ghi bằng note, không nối FK giả.

## 5. Đoạn thuyết minh thay thế

Sơ đồ quan hệ phản ánh model triển khai hiện hành gồm 24 bảng do ứng dụng quản lý, 24 khóa chính, hai alternate key phục vụ FK ghép và 50 khóa ngoại NO ACTION. Các liên kết của lịch thi đi qua EntryTestBooking; liên kết học viên với lớp đi qua Registration và StudentClassAssignment. Metadata xử lý, lịch sử occurrence/replacement, optimistic rowversion và hai bảng kỹ thuật bảo vệ giao dịch được giữ theo schema. Một số điều kiện như quyền actor, capacity/overlap, freeze và whole-Class finalization được kiểm ở transaction/service/trigger, không chỉ thể hiện bằng đường FK trên hình.
'''
    write('6_4_SO_DO_QUAN_HE.md','6.4 — Đối chiếu sơ đồ quan hệ trong báo cáo',body)

def section_objects():
    kinds={'TR':'Trigger','V':'View','IF':'Inline TVF','FN':'Scalar UDF','P':'Stored procedure'}
    modules=[(k.split('/')[-1],v.strip()) for k,v in CAT.items() if k.startswith('module-kind/')]
    rows=[]
    sqlroot=ROOT/'backend/src/EnglishCenter.Api/Infrastructure/Sql'
    for name,kind in modules:
        sources=[]
        for p in sqlroot.rglob('*.sql'):
            s=p.read_text(encoding='utf-8-sig')
            if re.search(r'CREATE\s+OR\s+ALTER\s+(?:TRIGGER|VIEW|FUNCTION|PROCEDURE)\s+(?:\[?dbo\]?\.)\[?'+re.escape(name)+r'\]?(?:\s|\()',s,re.I):sources.append(p)
        assert len(sources)==1,(name,sources)
        rel=sources[0].relative_to(ROOT).as_posix()
        params=sum(k.startswith(f'parameter/dbo/{name}/') for k in CAT)
        rows.append((name,kinds[kind],params,f'[{sources[0].name}](../../../{rel})'))
    role_rows=[]
    for k,v in CAT.items():
        if k.startswith('role/'):
            role=k.split('/')[1]
            permissions=[val for key,val in CAT.items() if key.startswith('permission/'+role+'/')]
            role_rows.append((role,len(permissions),sum(x.startswith('G') for x in permissions),sum(x.startswith('D') for x in permissions)))
    body='''Đây là **phụ lục bổ sung**, không giả định báo cáo cũ đã có một mục trigger/view/function/procedure riêng. Phần VI cũ chỉ có hình, từ điển và ràng buộc; cần dẫn cơ chế triển khai SQL để các mô tả “Hệ thống bảo đảm” có nguồn.

## 1. SQL objects trong source hiện tại

- 5 triggers cho used-Program/component freeze, Band threshold freeze, Attendance relation và Score relation/grade guard.
- 6 read-only views; 6 UDF gồm 5 inline TVF và 1 scalar UDF; 7 stored procedures cho queries và invoice number allocator.
- SP có tổng 47 parameters; không dùng tổng parameter của cả functions và SP làm số SP parameters.

'''+table(['Đối tượng','Loại','Số parameters','Source'],rows)
    body+='''
## 2. Trigger không thay toàn bộ workflow

TG01/TG02 bảo vệ cấu hình đã dùng, TG03 Band freeze, TG04 Attendance class/schedule/ended-Class, TG05 Score component/schedule/submitted/finalized guards. Caller phải prehold đúng transaction gates; trigger kiểm APPLOCK_MODE, không tự acquire sau DML. Các rule current role/session/ownership/deadline, source-state transition và full manifests còn do application transaction thực hiện.

Các function/view là nguồn tính/chiếu đọc, không tự cập nhật Band, finalize report hoặc xác nhận Payment. SP allocator cấp số trong transaction duyệt, không là chức năng phát hành độc lập cho Kế toán.

## 3. Database roles khác account roles

'''+table(['Database role','Số permissions catalog','GRANT','DENY'],role_rows)
    body+='''
Tổng 102 permission entries trong baseline nguồn; bốn database roles khác bảy account roles trong Role. Không đưa ec_app_runtime hoặc ec_finance_reporting thành role đăng nhập ứng dụng. Nguồn role/grants ở migration LeastPrivilegeRoles và SQL D12. Việc có role/grant trong source không một mình chứng minh một login live đã được bind đúng hoặc readiness hiện tại.

## 4. Những thứ không bổ sung như bảng sản phẩm

Reports không tạo bảng fact riêng, không lịch sử PII snapshot hoặc bảng học bổng; dùng views/queries nhất quán. __EFMigrationsHistory là bảng framework, không bảng nghiệp vụ thứ25. Sao lưu/phục hồi M4 còn cần đầu vào vận hành: không thêm bảng backup hoặc tuyên bố restore hoàn tất từ các migration trên.
'''
    write('PHU_LUC_SQL_OBJECTS_VA_PHAN_QUYEN.md','Phụ lục — SQL objects và phân quyền database hiện tại',body)

def build():
    summaries=table_docs()
    section_logic();section_tables(summaries);section_domains();section_bundles()
    section_uniques();section_fks();section_relational();section_diagram();section_objects()
    index='''# Đối chiếu database với báo cáo — 14/10/2026

## Kết luận chính

- Hai hình 6.1/6.4 là mô hình cũ, còn học bổng và các field/quan hệ không tồn tại trong dự án; cần vẽ lại.
- 22 bảng và 217 cột trong từ điển 6.2 **khớp tên, kiểu SQL và NULL**. Thiếu ReplayOperation10cột + InvoiceNumberCounter3cột để đủ 24 bảng/230cột.
- 49 FK viết bằng chữ trong 6.3.4 **khớp endpoints**. Bổ sung một FK ReplayOperation→Account; schema có50FK.
- Ràng buộc còn sai role literal, locked_until, status trên Attempt, tiền≥0, BANK_TRANSFER, placement ONGOING, đặc quyền override điểm và reason sau finalize; từng lỗi có nguồn/cách sửa trong file tương ứng.

Không vì hình cũ sai mà thay code hoặc xóa phần từ điển đang đúng. Bộ này chỉ tạo tài liệu riêng, không sửa Word nguồn/migration/schema.

## Mỗi phần báo cáo một file

| Phần báo cáo | File đối chiếu |
| --- | --- |
| 6.1 — Sơ đồ logic | [Đối chiếu sơ đồ logic](6_1_SO_DO_LOGIC.md) |
| 6.2 — Chi tiết các bảng | [Tổng hợp và liên kết từng bảng](6_2_CHI_TIET_BANG.md) |
| 6.3.1 — Miền giá trị | [Đối chiếu miền](6_3_1_RANG_BUOC_MIEN_GIA_TRI.md) |
| 6.3.2 — Liên thuộc tính | [Đối chiếu các bundle/cặp thuộc tính](6_3_2_RANG_BUOC_LIEN_THUOC_TINH.md) |
| 6.3.3 — Liên bộ cùng quan hệ | [Đối chiếu uniqueness/indexes](6_3_3_RANG_BUOC_LIEN_BO.md) |
| 6.3.4 — Tham chiếu | [Đối chiếu từng FK/bảng tầm ảnh hưởng](6_3_4_RANG_BUOC_THAM_CHIEU.md) |
| 6.3.5 — Liên quan hệ | [Đối chiếu workflow và transaction](6_3_5_RANG_BUOC_LIEN_QUAN_HE.md) |
| 6.4 — Sơ đồ quan hệ | [Đối chiếu hình schema cũ](6_4_SO_DO_QUAN_HE.md) |
| Phụ lục bổ sung | [SQL objects/database roles](PHU_LUC_SQL_OBJECTS_VA_PHAN_QUYEN.md) |

Mục6.2 có **24 file riêng theo bảng** trong `tables/`; mỗi file ghi từng cột, khóa/default/index/filter/CHECK và ý nghĩa cần cập nhật. Hai bảng chưa có trong báo cáo được đánh dấu bổ sung, không giả ghi “khớp”.

## Cách dùng

1. Sửa hình theo6.1/6.4 và quan hệ50FK.
2. Giữ cấu trúc217cột đúng của6.2, thêm hai bảng kỹ thuật và cập nhật metadata được chỉ rõ.
3. Sửa các đoạn ràng buộc theo6.3.1–6.3.5. Phân biệt SQL constraint với validation và transactional service/trigger.
4. Nếu cần chứng minh các cơ chế bảo vệ, dẫn phụlục SQL objects/source; không nói CHECK/FK tự chứng minh current role, trạng thái liên bảng, capacity hoặc clock.

## Phương pháp và nguồn

- Báo cáo Word được đọc qua OOXML, lấy đủ22 bảng/cột,49 dòng FK và các đoạn ràng buộc. Hai ảnh được trích và xem trực tiếp. Vị trí chỉ số XML trong bằng chứng là block index, **không phải số trang Word**.
- [InitialSchema](../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/20261005083517_InitialSchema.cs): parse24CreateTable/230cột để đối chiếu tên/kiểu/NULL/IDENTITY.
- [ExpectedCatalog](../../../backend/src/EnglishCenter.Api/Infrastructure/Compatibility/ExpectedCatalog.json): structural baseline FK/keys/indexes/CHECK/defaults/module/permissions; không dùng bản trong bin như source.
- [EF snapshot](../../../backend/src/EnglishCenter.Api/Infrastructure/Migrations/EnglishCenterDbContextModelSnapshot.cs) và [configurations](../../../backend/src/EnglishCenter.Api/Infrastructure/Configurations/AccountConfiguration.cs): đối chiếu model/source; không suy bằng source sample fixtures.
- [DATABASE_SPEC](../../design/DATABASE_SPEC.md), [requirements](../../requirements.md), [WORKFLOW_SPEC](../../design/WORKFLOW_SPEC.md), [CURRENT_STATE](../../CURRENT_STATE.md) và code service/SQL modules hiện hành: đối chiếu ý nghĩa và quyền.
- [Requirements](../../requirements.md) xác định học bổng ngoài phạm vi cả chức năng lẫn tables; không hồi sinh ba bảng học bổng từ hình cũ.

Số đếm nghiệp vụ/source:24 bảng/230cột/24PK/2AK/50FK/70CHECK/20RV/78non-keyindexes (tổng104 indexes vớiPK/AK),6migrations. Bảng framework __EFMigrationsHistory không tính trong24bảng.

## Giới hạn kiểm chứng

Lượt này không kết nối SQL live, không chạy migration hoặc bộ test runtime mới; không thay grants/login/key/custody/data. “Hiện tại” là source/model/catalog chuẩn đang trong dự án; không tuyên bố kiểm tra live DB mới. Các linked sources ngoài thư mục vẫn nằm trong workspace; ZIP chỉ đóng gói các file đối chiếu, ảnh nguồn và bằng chứng trích/phân tích.

Word nguồn giữ SHA256: `40B87ABBDF8C5CF65541827CA21B4240E7D3EB8985589505B7DA6D4C57DF2B4B`.
'''
    (OUT/'README.md').write_text(index,encoding='utf-8')
    extracted={'section_range':[266,1013],'tables':OLD,'foreign_keys':OLDFK,
               'constraints':{str(i):txt(BODY[i]) for i in range(315,1008) if txt(BODY[i])}}
    (OUT/'report-extract.json').write_text(json.dumps(extracted,ensure_ascii=False,indent=2),encoding='utf-8')
    (OUT/'schema-current.json').write_text(json.dumps(SCHEMA,ensure_ascii=False,indent=2),encoding='utf-8')
    replacements={
        'lượt1':'lượt 1','lượt2':'lượt 2','min-score':'min_score',
        'cùngCourseBand':'cùng Course/Band','sameCourseBand':'cùng Course/Band',
        'trướcCONFIRMED':'trước CONFIRMED','reportDRAFT':'report DRAFT',
        'đủamount':'đủ amount','đúngmột':'đúng một','đầyđủmộtlần':'đầy đủ một lần',
        'đủđiểm':'đủ điểm','cònchỗ':'còn chỗ','khôngtrùng':'không trùng',
        'toànClass':'toàn Class','toànSUBMITTED':'toàn SUBMITTED','cầnreason':'cần reason',
        'requiredAttendance':'required Attendance','currentgrader':'current grader',
        'weight100':'weight 100','đúng1FINAL_EXAM':'đúng 1 FINAL_EXAM',
        'positiveLESSONcount':'LESSON count dương','current Teacher/time':'Teacher hiện tại/thời gian',
        'source tables':'bảng trong source','role literal':'literal vai trò',
        'phụlục':'phụ lục','non-keyindexes':'indexes ngoài PK/AK'}
    for p in OUT.rglob('*.md'):
        text=p.read_text(encoding='utf-8')
        for before,after in replacements.items():text=text.replace(before,after)
        text=re.sub(r'\b(\d+)(bảng|cột|PK|AK|FK|CHECK|RV|migrations|indexes|parameters)\b',r'\1 \2',text)
        text=text.replace('Lấy 24CreateTable','Lấy 24 CreateTable').replace('parse24CreateTable','parse 24 CreateTable')
        p.write_text(text,encoding='utf-8')
    return summaries

if __name__=='__main__':
    result=build()
    print(f'Built 8 report sections + SQL appendix + 24 table files: {OUT}')
