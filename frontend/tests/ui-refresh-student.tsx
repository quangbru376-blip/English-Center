// Dev-only visual review. No HttpClient, identity, credentials or business writes.
import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Home as HomeIcon, BookOpen, CalendarDays, GraduationCap, Receipt, ClipboardList } from 'lucide-react';
import { WorkspaceShell } from '../src/app/WorkspaceShell';
import { StudentHome, StudentCourses } from '../src/features/enrollment/components/StudentOverview';
import type { Home, CourseChoice } from '../src/features/enrollment/enrollmentApi';
import { Button } from '../src/components/ui/Button';
import { PageHeader } from '../src/components/ui/PageHeader';
import { Feedback } from '../src/components/ui/Feedback';
import { Card, Skeleton } from '../src/components/ui/Surfaces';
import { Modal } from '../src/components/ui/Modal';
import '../src/styles/index.css';

const courses: CourseChoice[] = [
  { id: 1, code: 'IELTS-F', name: 'IELTS nền tảng', status: 'ACTIVE', tuition: '4800000' },
  { id: 2, code: 'ENG-C', name: 'Tiếng Anh giao tiếp', status: 'ACTIVE', tuition: '3500000' },
  { id: 3, code: 'TOEIC', name: 'TOEIC định hướng 650', status: 'ACTIVE', tuition: '4200000' },
];
const items: Home[] = [
  { registrationId: 1, course: courses[0], status: 'TESTING', createdAt: '2026-10-10T03:00:00Z', currentStep: 'Đang kiểm tra đầu vào', nextAction: { action: 'VIEW_ENTRY_RESULTS', target: { registrationId: 1 } }, waitingReason: 'Đã đặt ca kiểm tra đầu vào. Giáo viên sẽ cập nhật kết quả sau buổi thi.', nearestEvent: { kind: 'ENTRY_TEST', bookingId: 1, registrationId: 1, startAt: '2026-10-15T02:00:00Z', endAt: '2026-10-15T03:00:00Z', room: 'A102' }, unpaidInvoice: null, finalizedReport: null },
  { registrationId: 2, course: courses[1], status: 'ADMITTED', createdAt: '2026-10-08T03:00:00Z', currentStep: 'Đang chờ thanh toán học phí', nextAction: { action: 'VIEW_INVOICE', target: { registrationId: 2, invoiceId: 1 } }, waitingReason: null, nearestEvent: null, unpaidInvoice: { id: 1, number: 'HD-2026-00000001', amount: '3500000', dueDate: '2026-10-20', isOverdue: false }, finalizedReport: null },
  { registrationId: 3, course: courses[2], status: 'ADMITTED', createdAt: '2026-07-10T03:00:00Z', currentStep: 'Đã công bố kết quả học tập', nextAction: { action: 'VIEW_FINALIZED_REPORT', target: { registrationId: 3, classId: 1, gradeReportId: 1 } }, waitingReason: null, nearestEvent: null, unpaidInvoice: null, finalizedReport: { gradeReportId: 1, registrationId: 3, classId: 1 } },
];
function Preview() {
  const [view, setView] = useState<'home'|'courses'>('home');
  const [state, setState] = useState<'ready'|'empty'|'loading'|'error'>('ready');
  const [selected, setSelected] = useState<CourseChoice | null>(null), [notice, setNotice] = useState<string | null>(null);
  const busy = state === 'loading';
  const showDetail = () => setNotice('Đây là bản xem mẫu. Luồng hồ sơ/đặt ca thực tế được kiểm chứng riêng bằng Edge và SQL scratch trong bộ ảnh đi kèm.');
  const changeView = (next: 'home'|'courses') => { setView(next); setNotice(null); };
  return <WorkspaceShell context={{ accountId: 1, fullName: 'Nguyễn Minh Anh', email: 'preview@example.test', role: 'Student', scope: 'NORMAL', accountVersion: 'AAAAAAAAAAE=', serverTime: '2026-10-13T03:00:00Z', effectiveExpiresAt: '2026-10-13T03:20:00Z', absoluteExpiresAt: '2026-10-13T05:00:00Z' }} navigation={[
    { label:'Trang chủ', icon:HomeIcon, selected:view==='home', onClick:()=>changeView('home') },
    { label:'Chọn khóa học', icon:BookOpen, selected:view==='courses', onClick:()=>changeView('courses') },
    { label:'Lịch thi', icon:CalendarDays, selected:false, onClick:showDetail },
    { label:'Lớp của tôi', icon:GraduationCap, selected:false, onClick:showDetail },
    { label:'Hóa đơn của tôi', icon:Receipt, selected:false, onClick:showDetail },
    { label:'Kết quả của tôi', icon:ClipboardList, selected:false, onClick:showDetail },
  ]}>
    <section className="student-page"><Card><p className="text-muted">Bản xem mẫu với dữ liệu minh họa. Trang này không đăng nhập, không gọi API và không lưu thay đổi.</p></Card>
      <div className="student-intro"><PageHeader title={view==='home'?'Trang chủ học viên':'Chọn khóa học'}/><label>Trạng thái xem mẫu<select aria-label="Trạng thái xem mẫu" value={state} onChange={event=>setState(event.currentTarget.value as typeof state)}><option value="ready">Có dữ liệu</option><option value="empty">Chưa có dữ liệu</option><option value="loading">Đang tải</option><option value="error">Lỗi đọc dữ liệu</option></select></label></div>
      {notice && <Card><p>{notice}</p></Card>}
      {state==='loading' ? <Skeleton/> : state==='error' ? <Feedback kind="error" message="Chưa đọc được dữ liệu. Hãy kiểm tra rồi thử lại."/> : view==='home' ?
        <StudentHome data={{items:state==='empty'?[]:items,page:1,pageSize:20,totalCount:state==='empty'?0:items.length}} busy={busy} read={showDetail} changePage={()=>{}} chooseCourses={()=>changeView('courses')} openNext={showDetail} openReport={showDetail}/> :
        <StudentCourses data={{items:state==='empty'?[]:courses,page:1,pageSize:20,totalCount:state==='empty'?0:courses.length}} busy={busy} blocked={busy} register={setSelected} changePage={()=>{}}/>}
      <Modal open={!!selected} title="Xác nhận thay đổi" onRequestClose={()=>setSelected(null)}><p>Tạo hồ sơ cho khóa {selected?.name}. Học phí hiện tại chỉ là thông tin tham khảo; hóa đơn được lập khi hồ sơ được duyệt.</p><p className="text-muted mt-4">Hộp thoại minh họa, không gửi yêu cầu đăng ký.</p><Button className="mt-4" variant="secondary" onClick={()=>setSelected(null)}>Quay lại</Button></Modal>
    </section>
  </WorkspaceShell>;
}
const root = document.getElementById('root');
if (import.meta.env.DEV && root) createRoot(root).render(<Preview/>);
