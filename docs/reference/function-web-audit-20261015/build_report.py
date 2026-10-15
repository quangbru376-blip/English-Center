from pathlib import Path
import json, re, hashlib, xml.etree.ElementTree as ET

root = Path(__file__).resolve().parents[3]
out = Path(__file__).resolve().parent
source = json.loads((out/'word-section-iii.json').read_text(encoding='utf-8'))
F = 'frontend/src/features/'
B = 'backend/src/EnglishCenter.Api/'
# NV: user navigation, frontend evidence, backend evidence, related browser groups.
mapping = {
1: ('/register; /login; menu tài khoản → Thông tin cá nhân / Đổi mật khẩu / Đăng xuất', F+'enrollment/StudentEnrollmentPage.tsx', B+'Api/Enrollment/EnrollmentEndpoints.cs', '1,2,4,5,6,85'),
2: ('Học viên → Chọn khóa học → chọn khóa; Trang chủ → xem hồ sơ → hủy', F+'enrollment/StudentEnrollmentPage.tsx', B+'Services/Enrollment/StudentEnrollmentService.cs', '85'),
3: ('Học viên → hồ sơ → chọn ca / đổi lịch / hủy đặt chỗ; Lịch thi', F+'enrollment/StudentEnrollmentPage.tsx', B+'Services/Enrollment/StudentEnrollmentService.cs', '85,87'),
4: ('Học viên → chi tiết hồ sơ: đã có điểm từng lượt và Band; chưa hiện riêng điểm tốt nhất/lượt được chọn', F+'enrollment/StudentEnrollmentPage.tsx', B+'Infrastructure/Enrollment/SqlStudentEnrollmentReader.cs', '87'),
5: ('Học viên → hồ sơ đã có Band → xem trước và xác nhận nhập học', F+'enrollment/StudentEnrollmentPage.tsx', B+'Services/Enrollment/StudentEnrollmentService.cs', '87'),
6: ('Học viên → Hóa đơn của tôi → chi tiết → Đọc mới và in hóa đơn; có hướng dẫn thanh toán', F+'invoices/StudentInvoicesPage.tsx', B+'Infrastructure/StudentInvoices/SqlStudentInvoiceReader.cs', '89'),
7: ('Học viên → Lớp của tôi → lớp/lịch hiện tại hoặc lịch sử', F+'classes/ClassesPage.tsx', B+'Api/Classes/ClassEndpoints.cs', '90,91'),
8: ('Học viên → Kết quả của tôi → chi tiết kết quả đã công bố', F+'classes/StudentGradesPage.tsx', B+'Infrastructure/Classes/SqlStudentGradeReads.cs', '94'),
9: ('Tuyển sinh → Hồ sơ tuyển sinh → tìm kiếm / xem hồ sơ', F+'admissions/AdmissionsPage.tsx', B+'Infrastructure/Admissions/SqlAdmissionsSearch.cs', '88'),
12: ('Tuyển sinh → chi tiết hồ sơ → duyệt / từ chối / hủy → xác nhận', F+'admissions/AdmissionsPage.tsx', B+'Services/Admissions/AdmissionsService.cs', '88'),
13: ('Giáo vụ → Khóa học và Band; Chương trình đào tạo → cấu hình / vòng đời', F+'programs/pages/ProgramPage.tsx', B+'Api/Academics/ProgramEndpoints.cs', '82,83'),
14: ('Giáo vụ → Ca thi đầu vào → tạo / mở / đóng / hủy / sửa lịch / đổi người chấm', F+'entrySessions/pages/EntrySessionPage.tsx', B+'Api/Academics/EntrySessionEndpoints.cs', '84'),
15: ('Giáo vụ → Ca thi đầu vào → xem ca → danh sách thí sinh / kết quả / Band', F+'entrySessions/pages/EntrySessionPage.tsx', B+'Infrastructure/Academics/SqlEntrySessionExecutor.cs', '84,86,87'),
16: ('Giáo vụ → Lớp và lịch học → tạo / xem lớp → mở / bắt đầu / hoàn tất / hủy / sửa sĩ số', F+'classes/ClassesPage.tsx', B+'Services/Classes/ClassService.cs', '90,92'),
17: ('Giáo vụ → chi tiết lớp → Đọc lựa chọn giáo viên → xem trước đổi giáo viên', F+'classes/ClassesPage.tsx', B+'Services/Classes/ClassService.cs', '90'),
18: ('Giáo vụ → Lớp và lịch học → Hồ sơ chờ xếp lớp → lớp phù hợp → xem trước xếp', F+'classes/ClassesPage.tsx', B+'Services/Classes/ClassService.cs', '90'),
19: ('Giáo vụ → Lớp và lịch học → lịch: từng buổi / tuần / dời / thay / xác nhận đã tổ chức', F+'academics/components/SchedulesPanel.tsx', B+'Api/Classes/ClassEndpoints.cs', '91,92'),
20: ('Giáo vụ → Duyệt bảng điểm → lớp đã gửi → xem trước trả lại / chốt cả lớp', F+'classes/GradeReviewPage.tsx', B+'Api/Classes/GradeEndpoints.cs', '93,94'),
21: ('Giáo viên → Kiểm tra đầu vào → Đọc ca được phân công → danh sách học viên', F+'entryTasks/TeacherEntryPage.tsx', B+'Api/Enrollment/TeacherEntryEndpoints.cs', '86'),
22: ('Giáo viên → lượt đặt trong ca → Chấm điểm / Xác nhận vắng / Sửa điểm có lý do', F+'entryTasks/TeacherEntryPage.tsx', B+'Services/Enrollment/TeacherEntryService.cs', '86'),
23: ('Giáo viên → Lớp và lịch dạy → chi tiết lớp / danh sách học viên / lịch', F+'classes/ClassesPage.tsx', B+'Api/Classes/ClassEndpoints.cs', '90,91,92'),
24: ('Giáo viên → lớp → Điểm danh buổi học → chọn LESSON → Có mặt/Vắng → lưu', F+'classes/AttendancePanel.tsx', B+'Services/Classes/AttendanceService.cs', '92'),
25: ('Giáo viên → lớp → sổ điểm → học viên → lưu điểm/nhận xét; xem trước gửi cả lớp', F+'classes/GradebookPanel.tsx', B+'Services/Classes/GradeService.cs', '93,94'),
26: ('Giáo viên → sổ điểm lớp đã trả lại → sửa từng học viên → gửi lại cả lớp', F+'classes/GradebookPanel.tsx', B+'Services/Classes/GradeService.cs', '93'),
27: ('Kế toán → Hóa đơn và thanh toán → bộ lọc / chi tiết / cảnh báo quá hạn', F+'finance/FinancePage.tsx', B+'Api/Finance/FinanceEndpoints.cs', '89'),
28: ('Kế toán → chi tiết hóa đơn → xác nhận thu đủ; Payment → sửa phương thức/mã tham chiếu', F+'finance/FinancePage.tsx', B+'Api/Finance/FinanceEndpoints.cs', '89'),
29: ('Kế toán → Báo cáo tài chính → Áp dụng bộ lọc → Tải CSV tài chính', F+'reports/ReportsPage.tsx', B+'Api/Reports/ReportEndpoints.cs', '95'),
30: ('Giám đốc → Báo cáo tổng hợp → Tuyển sinh → bộ lọc / tổng / bảng nhóm', F+'reports/ReportsPage.tsx', B+'Infrastructure/Reports/SqlReportsExecutor.cs', '95'),
31: ('Giám đốc → Báo cáo tổng hợp → Đào tạo → hiện trạng / tiến độ / kết quả đã chốt', F+'reports/ReportsPage.tsx', B+'Infrastructure/Reports/SqlReportsExecutor.cs', '95'),
32: ('Giám đốc → Báo cáo tổng hợp → Tài chính; không có nút CSV hoặc thu tiền', F+'reports/ReportsPage.tsx', B+'Api/Reports/ReportEndpoints.cs', '95'),
33: ('Admin → Tài khoản → cấp nhân sự / chọn tài khoản → đặt lại mật khẩu; khối secret một lần', F+'administration/pages/StaffAdminPage.tsx', B+'Api/Administration/StaffAccountEndpoints.cs', '81'),
34: ('Admin → Tài khoản → chi tiết → khóa / mở / ngừng / khôi phục', F+'administration/pages/StaffAdminPage.tsx', B+'Api/Administration/AdminAccountEndpoints.cs', '9,81'),
35: ('Admin → Nhật ký kiểm toán → lọc → xem sự kiện → xem nhóm thao tác', F+'audit/AuditPage.tsx', B+'Api/Administration/AuditEndpoints.cs', '95'),
36: ('Kết quả tự động trên hồ sơ Student, nhiệm vụ chấm Teacher và danh sách ca của Giáo vụ', F+'entryTasks/TeacherEntryPage.tsx', B+'Services/Enrollment/TeacherEntryService.cs', '86,87'),
37: ('Sau Tuyển sinh duyệt, hóa đơn xuất hiện trong chi tiết hồ sơ, Hóa đơn của tôi và Kế toán', F+'admissions/AdmissionsPage.tsx', B+'Services/Admissions/AdmissionsService.cs', '88,89'),
38: ('Tổng nháp trong sổ điểm; tổng chính thức/kết quả đạt trong màn hình đã công bố của Student', F+'classes/components/StudentGradeResults.tsx', B+'Services/Classes/GradeService.cs', '93,94'),
39: ('Kết quả ghi tự động được Admin xem ở Nhật ký kiểm toán, chi tiết và nhóm thao tác', F+'audit/AuditPage.tsx', B+'Infrastructure/Auditing/SqlAuditAppender.cs', '81–95'),
}
# Resolve exact paths against the real checkout; reject guesses before publication.
for nv, (nav, front, back, groups) in list(mapping.items()):
    if not (root/front).exists():
        candidates=list((root/'frontend/src').rglob(Path(front).name))
        assert len(candidates)==1,(nv,front,candidates)
        front=candidates[0].relative_to(root).as_posix()
    if not (root/back).exists():
        candidates=list((root/'backend/src').rglob(Path(back).name))
        assert len(candidates)==1,(nv,back,candidates)
        back=candidates[0].relative_to(root).as_posix()
    mapping[nv]=(nav,front,back,groups)

def link(path):
    p=root/path
    lines=p.read_text(encoding='utf-8-sig').splitlines()
    line=next((i for i,x in enumerate(lines,1) if re.search(r'export function |public (?:static |sealed |async )?(?:class|void|Task)|internal sealed class',x)),1)
    return f'[{p.name}](<{p.as_posix()}:{line}>)'

browser=out/'browser/functions-web-edge.xml'
result=None
if browser.exists():
    assembly=ET.parse(browser).getroot().find('assembly')
    if assembly is not None:
        result={k:assembly.get(k) for k in ['total','passed','failed','skipped','errors','time']}
        result['cases']=[dict(name=t.get('name'),result=t.get('result')) for t in assembly.findall('.//test')]
default_result=None
if (out/'backend-default.xml').exists():
    assembly=ET.parse(out/'backend-default.xml').getroot().find('assembly')
    if assembly is not None:
        default_result={k:assembly.get(k) for k in ['total','passed','failed','skipped','errors','time']}

head=json.loads((out/'source-fingerprints-before.json').read_text())['head']
parts=[
'# Kiểm tra chức năng phần III có triển khai trên web',
f'Ngày kiểm tra: 15/10/2026. Checkout `{head}`. Nguồn: phần III của `docs/Nhom18_ThietKeXuLy.docx`, đọc trực tiếp OOXML, đủ 37 dòng NV01–NV09 và NV12–NV39 trong tám bảng. SHA-256: `{source["sha256"]}`. Không có tracked insertion/deletion trong phần này. Không sửa Word.',
'## Kết luận',
'Đối chiếu mã nguồn xác nhận **37/37 chức năng có đường thực hiện hoặc kết quả hiển thị trên web**, nhưng **NV04 còn thiếu hiển thị điểm tốt nhất/lượt được chọn** theo đúng điều kiện trong Word. Ma trận ghi 36 chức năng đã có trên web và một chức năng có một phần yêu cầu hiển thị chưa hoàn thiện. Không phát hiện chức năng hoàn toàn chỉ có code nền chưa nối vào web. Không kết luận toàn bộ phần III hoàn tất chỉ từ số lượng chức năng hoặc từ tests đang PASS.',
'Một số thao tác nằm trong chi tiết lớp/hồ sơ/ca thi hoặc menu tài khoản, không có mục menu độc lập. Không dùng số lượng menu để đếm chức năng. Nút có thể bị vô hiệu hóa đúng điều kiện nghiệp vụ hoặc không xuất hiện với vai trò khác.',
'## Bằng chứng nối ứng dụng',
f'- Entry web thật: {link("frontend/src/main.tsx")} → {link("frontend/src/app/App.tsx")} → {link("frontend/src/app/IdentityApp.tsx")}; menu, role branch và các lazy panel đều có trang thực thi.',
f'- Host LOCAL_DEV đăng ký các nhóm API qua {link(B+"Infrastructure/Operator/LocalRuntimeServices.cs")}; không chỉ khai báo trong tài liệu hoặc test.',
f'- Backend lớp học nối thêm grading qua {link(B+"Api/Classes/ClassEndpoints.cs")} → {link(B+"Api/Classes/GradeEndpoints.cs")}; frontend có ClassesPage → GradebookPanel / AttendancePanel / SchedulesPanel.',
'## Đối chiếu đủ 37 dòng',
'“Có” nghĩa là có code xử lý, nối API và giao diện hoặc kết quả UI tương ứng. Cột nhóm thử chỉ các nhóm browser liên quan, không khẳng định mọi điều kiện con đều được click qua trong nhóm đó.',
'| NV | Chức năng nguyên tên trong Word | Trạng thái | Nơi thực hiện hoặc xem trên web | Frontend | Backend | Nhóm browser liên quan |',
'|---|---|---|---|---|---|---|',
]
records=[]
for row in source['rows']:
    nv=row['nv']; nav,front,back,groups=mapping[nv]
    state='Một phần — thiếu hiển thị điểm tốt nhất' if nv==4 else 'Có — kết quả tự động hiển thị' if nv>=36 else 'Có — thao tác/xem trên web'
    parts.append(f'| {nv:02} | {row["name"]} | {state} | {nav} | {link(front)} | {link(back)} | {groups} |')
    records.append(dict(**row,webStatus='PARTIAL_DISPLAY_REQUIREMENT' if nv==4 else 'IMPLEMENTED_AND_WIRED',navigation=nav,frontend=front,backend=back,relatedBrowserGroups=groups))
parts.extend([
'## Phát hiện cần sửa trước khi chốt đủ phần III',
f'**[P2] NV04 — thiếu hiển thị điểm tốt nhất được dùng xếp Band.** Word ghi rõ “hiển thị điểm từng lượt đã chấm, điểm tốt nhất và mức trình độ tương ứng”. Tại [StudentEnrollmentPage.tsx:165](<{(root/(F+"enrollment/StudentEnrollmentPage.tsx")).as_posix()}:165>), UI lặp các lượt với điểm rồi chỉ hiện “Band hiện tại”; không dùng `registration.entry.bestAttemptId` để đánh dấu lượt được chọn hoặc hiện “Điểm tốt nhất”. Preview xác nhận tại dòng171 cũng chỉ lặp điểm và Band. `enrollmentApi.ts` đã nhận/kiểm `bestAttemptId` và `bandAssignment.bestAttemptId`, nên không thiếu backend calculation/data. Cần hiển thị điểm/lượt tốt nhất dựa trên ID server trả, giữ hòa chọn lượt một và vắng không thành0; không tự tính lại quyền hay đổi thuật toán Band. Các test hiện có đọc điểm/Band nhưng không yêu cầu nhãn này, nên browser PASS vẫn không đóng thiếu sót.',
'Verdict đối với yêu cầu “đủ các chức năng và nội dung phải thể hiện trên web”: **PASS WITH FIXES — NV04 là mục bắt buộc sửa**. Đây là phát hiện ở source/UI; chưa sửa code trong lượt audit.',
'## Các điểm cần hiểu đúng khi xem web',
'1. **NV13 — Band chỉ đọc.** Giáo vụ có “Xem bộ Band” để xem ngưỡng 0/25/45/65/85 và trạng thái đóng băng. Không có form sửa ngưỡng Band. Chỉnh cấu hình trọng số/số buổi nằm ở Chương trình đào tạo. Nếu “cấu hình mức trình độ” trong Word được hiểu là tự chỉnh ngưỡng tùy ý thì phần đó không có trên web; baseline hiện hành quy định Band tham chiếu chỉ đọc, nên không tự coi đây là lỗi cần thêm editor.',
'2. **NV16–19, NV23–26 được gom theo lớp.** Xếp lớp/lập lịch/điểm danh/sổ điểm có các khối và nút trong chi tiết lớp; Giáo viên truy cập lớp được phân công. Lưu nháp một học viên khác gửi/trả/chốt cả lớp.',
'3. **NV36–39 không cần màn hình hay nút riêng.** Band tính sau chấm/sửa điểm; hóa đơn phát hành khi Tuyển sinh duyệt; tổng chính thức lưu lúc chốt; nhật ký được ghi theo hành động và Admin đọc tại trang Nhật ký kiểm toán.',
'4. **Thông tin thanh toán NV06 là hướng dẫn chung.** Web hiển thị thông báo trả tiền bên ngoài và chờ Kế toán xác nhận; không có cổng thanh toán hay tài khoản ngân hàng/QR cấu hình riêng. Các nội dung đó không được yêu cầu trong bảng đang đọc.',
'5. **Dữ liệu và trạng thái quyết định thao tác.** Chưa có hồ sơ/ca/lớp đúng điều kiện hoặc đăng nhập sai vai trò sẽ không thấy được tất cả thao tác. Lớp chưa COMPLETED không gửi/chốt bảng điểm; ca CLOSED chặn Student đổi/hủy booking riêng; Student không xem điểm chưa FINALIZED.',
'## Kiểm chứng mới trong lượt này',
'- Typecheck và lint frontend: PASS; log `typecheck.log`, `lint.log`.',
'- Frontend tests: 183 PASS, 0 FAIL, 0 SKIP; `frontend-tests.log`.',
('- Backend mặc định chưa có kết quả cuối.' if default_result is None else f'- Backend mặc định: {default_result["passed"]} PASS, {default_result["failed"]} FAIL, {default_result["skipped"]} opt-in SKIP; `backend-default.xml`. Lượt sandbox trước có 12 lỗi quyền truy cập pipe/store tạm; đã chạy lại cùng test không đổi assertion ngoài sandbox, log ban đầu giữ riêng.'),
('— Chưa có kết quả cuối bộ browser.' if result is None else f'- Edge/HTTPS/API/SQL: {result["passed"]}/{result["total"]} PASS; FAIL={result["failed"]}, SKIP={result["skipped"]}, errors={result["errors"]}; kết quả xUnit `browser/functions-web-edge.xml`, TRX cùng thư mục.'),
'- Bộ browser dùng các test hiện có `OwnedBrowserSqlHostingTests`, `verify-identity.cjs`, `verify-m1.cjs` và các module M2/M3/M4. Các nhóm 81–95 kiểm thao tác UI thực và xác nhận dữ liệu SQL sau COMMIT; chạy tại 375/768/1280 với bảy vai trò và anonymous.',
'- Build fixture dùng cùng component IdentityApp và API thật; dữ liệu/khóa/clock là synthetic trong database thử riêng. Đây không phải test HTTP mock. HTTPS fixture dùng pin SPKI chứng chỉ riêng; kết nối SQL fixture cho phép chứng chỉ thử. Không gọi kết quả này là kiểm chứng mới của custody/trusted HTTPS/strict SQL trên môi trường vận hành LOCAL_DEV.',
'- Lượt VSTest đầu không kết nối được testhost; runner trực tiếp xUnit trong sandbox bị Shared Memory SQL access denied trước khi tạo DB. Giữ log riêng; không dùng các lần bị chặn làm bằng chứng thiếu chức năng. Không giảm assertion hoặc sửa code sản phẩm để vượt lỗi môi trường.',
'- Compile hỗ trợ frontend lần đầu dùng glob Windows không tìm được input; đã sửa riêng script bằng đường dẫn slash, giữ log ban đầu. Typecheck code sản phẩm vẫn PASS.',
'## Giới hạn và việc còn lại',
'Không sửa code sản phẩm, Word hoặc dữ liệu vận hành để hoàn thiện thêm chức năng. Đây là audit mức chức năng hiển thị/thao tác và các luồng browser đại diện; không là chứng nhận toàn bộ 154 endpoint/mọi nhánh validation, full WCAG, tải, backup/restore hoặc nghiệm thu cổng UI4. Recovery M4 vẫn phụ thuộc đầu vào riêng, không nằm trong 37 dòng chức năng cần xuất hiện trên web này.',
'Nội dung Word có vài câu lặp như “đã đã”, “buổi buổi”, “không không tổ chức”; audit diễn giải theo điều kiện nghiệp vụ đã duyệt, không tự sửa báo cáo. Riêng NV36 “sửa thông tin hợp lệ” nên diễn đạt là “sửa điểm đầu vào hợp lệ” khi bạn chỉnh Word.',
])
if (out/'nv04-observations.jsonl').exists():
    observations=[json.loads(x) for x in (out/'nv04-observations.jsonl').read_text(encoding='utf-8').splitlines()]
    parts.extend(['## Tái hiện NV04 trên web',
        'Chạy bổ sung đúng group87 với adapter chỉ quan sát DOM/chụp ảnh và thêm GET đọc lại; giữ nguyên các assertion/thao tác của driver. Đây là một lượt lặp bổ sung, không phải ca độc lập thứ22. Dữ liệu trong ảnh là synthetic.',
        '| Width | Điểm tốt nhất backend trả | Band backend trả | Nhãn điểm/lượt tốt nhất trên UI | Ảnh chi tiết |',
        '|---|---|---|---|---|'])
    for item in observations:
        image_path=out/f'nv04-student-{item["width"]}.png'
        parts.append(f'| {item["width"]} | {item["serverBestScore"]} | {item["band"]} | {"Có" if item["explicitBestMarker"] else "Không có"} | [Xem ảnh](<{image_path.as_posix()}>) |')
    # This table is a single block to preserve Markdown header syntax.
    start=parts.index('## Tái hiện NV04 trên web')
    extra=parts[start:];parts=parts[:start]
else:
    extra=[]
table_end=parts.index('## Phát hiện cần sửa trước khi chốt đủ phần III')
report='\n\n'.join(parts[:11])+'\n\n'+'\n'.join(parts[11:table_end])+'\n\n'+'\n\n'.join(parts[table_end:])+'\n'
if extra:report+='\n'+extra[0]+'\n\n'+extra[1]+'\n\n'+'\n'.join(extra[2:])+'\n'
(out/'REPORT.md').write_text(report,encoding='utf-8')
(out/'function-matrix.json').write_text(json.dumps(dict(sourceSha256=source['sha256'],head=head,count=len(records),functions=records,browser=result,backendDefault=default_result),ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(functions=len(records),browser=result and {k:v for k,v in result.items() if k!='cases'})))
