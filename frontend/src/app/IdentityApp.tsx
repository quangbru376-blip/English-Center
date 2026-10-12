import { useCallback, useEffect, useState, useSyncExternalStore } from 'react';
import { IdentityShell } from './IdentityShell';
import { identityPath } from './identityNavigation';
import { Button } from '../components/ui/Button';
import { Feedback } from '../components/ui/Feedback';
import type { HttpClient } from '../lib/http';
import { SessionController } from '../features/identity/sessionController';
import { createSessionSignals } from '../features/identity/sessionSignals';
import { SignupPage } from '../features/identity/pages/SignupPage';
import { LoginPage } from '../features/identity/pages/LoginPage';
import { SessionPage } from '../features/identity/pages/SessionPage';
import { PasswordChangePage } from '../features/identity/pages/PasswordChangePage';
import { StaffAdminPage } from '../features/administration/pages/StaffAdminPage';
import { CataloguePage } from '../features/catalogue/pages/CataloguePage';
import { ProgramPage } from '../features/programs/pages/ProgramPage';
import { EntrySessionPage } from '../features/entrySessions/pages/EntrySessionPage';
import { StudentEnrollmentPage } from '../features/enrollment/StudentEnrollmentPage';
import { TeacherEntryPage } from '../features/entryTasks/TeacherEntryPage';
import { AdmissionsPage } from '../features/admissions/AdmissionsPage';
import { StudentInvoicesPage } from '../features/invoices/StudentInvoicesPage';
import { ReportsPanel,AuditPanel } from '../features/reports/ReportPanels';
import { FinancePage } from '../features/finance/FinancePage';
import { StudentGradesPage, GradeReviewPage } from '../features/classes/GradePanels';
import { GradebookPanel } from '../features/classes/GradePanels';
import { AttendancePanel } from '../features/classes/GradePanels';
import { ClassesPage } from '../features/classes/ClassesPage';

function Routes({ controller, initialPath }: { controller: SessionController; initialPath?: string }) {
  const subscribe = useCallback((listener: () => void) => controller.subscribe(listener), [controller]);
  const getSnapshot = useCallback(() => controller.snapshot, [controller]);
  const snapshot = useSyncExternalStore(subscribe, getSnapshot);
  const [path, setPath] = useState(initialPath ?? window.location.pathname);
  const effectivePath = identityPath(snapshot.context, path);
  const navigate = useCallback((next: string) => {
    if(document.querySelector('[data-unsaved-attendance="true"], [data-unsaved-grade="true"]')&&!window.confirm('Có dữ liệu chưa lưu. Rời trang và bỏ thay đổi?'))return;
    window.history.pushState(null, '', next); setPath(next);
  }, []);
  useEffect(() => {
    const pop = () => { if(document.querySelector('[data-unsaved-attendance="true"], [data-unsaved-grade="true"]')&&!window.confirm('Có dữ liệu chưa lưu. Rời trang và bỏ thay đổi?')){window.history.pushState(null,'',path);return;}setPath(window.location.pathname); void controller.checkSession(); };
    window.addEventListener('popstate', pop); return () => window.removeEventListener('popstate', pop);
  }, [controller,path]);
  useEffect(() => {
    if (snapshot.phase === 'checking' || snapshot.phase === 'pending' || snapshot.phase === 'unknown') return;
    if (window.location.pathname !== effectivePath) window.history.replaceState(null, '', effectivePath);
    document.getElementById('main-content')?.focus();
  }, [effectivePath, snapshot.context, snapshot.phase]);
  if (snapshot.phase === 'checking' || snapshot.phase === 'pending' && effectivePath !== '/account/change-password') return <Feedback kind="loading" message="Đang kiểm tra ngữ cảnh…" />;
  if (snapshot.phase === 'unknown') return <>
    <Feedback kind="error" message={snapshot.notice ?? 'Chưa xác minh được phiên.'} />
    <Button className="mt-4" onClick={() => void controller.checkSession()}>Kiểm tra lại phiên</Button>
    <Button className="ml-3 mt-4" variant="secondary" onClick={() => { navigate('/login'); void controller.prepare(); }}>Đến đăng nhập</Button>
  </>;
  if (snapshot.context && effectivePath === '/account/change-password') return <PasswordChangePage controller={controller} snapshot={snapshot} onBack={() => navigate('/')} />;
  if(snapshot.context?.role==='Admin'&&snapshot.context.scope==='NORMAL')return <><nav aria-label="Quản trị" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/admin/accounts')}>Tài khoản</Button><Button onClick={()=>navigate('/admin/audit-events')}>Nhật ký kiểm toán</Button></nav>{effectivePath.startsWith('/admin/audit-events')||effectivePath.startsWith('/admin/operations')?<AuditPanel key={effectivePath} controller={controller} initialId={effectivePath.startsWith('/admin/audit-events/')?Number(effectivePath.split('/')[3]):undefined} initialOperation={effectivePath.startsWith('/admin/operations/')?effectivePath.split('/')[3]:undefined} onPassword={()=>navigate('/account/change-password')}/>:<StaffAdminPage controller={controller} initialId={effectivePath.startsWith('/admin/accounts/')?Number(effectivePath.split('/').at(-1)):undefined} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if(snapshot.context?.role==='Director'&&snapshot.context.scope==='NORMAL')return <ReportsPanel controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if (snapshot.context?.role === 'AcademicAffairs' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Giáo vụ" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/academics/entry-test-sessions')}>Ca thi đầu vào</Button><Button onClick={()=>navigate('/academics/courses')}>Khóa học và Band</Button><Button onClick={()=>navigate('/academics/training-programs')}>Chương trình đào tạo</Button><Button onClick={()=>navigate('/academics/classes')}>Lớp và lịch học</Button><Button onClick={()=>navigate('/academics/grade-reviews')}>Duyệt bảng điểm</Button></nav>{effectivePath==='/academics/grade-reviews'?<GradeReviewPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/classes'?<ClassesPage controller={controller} role="AcademicAffairs" onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/courses'?<CataloguePage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/training-programs'?<ProgramPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<EntrySessionPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Student' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Học viên" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/student/home')}>Hồ sơ và lịch thi</Button><Button onClick={()=>navigate('/student/invoices')}>Hóa đơn của tôi</Button><Button onClick={()=>navigate('/student/classes')}>Lớp của tôi</Button><Button onClick={()=>navigate('/student/grade-reports')}>Kết quả của tôi</Button></nav>{effectivePath.startsWith('/student/grade-reports')?<StudentGradesPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/student/classes'?<ClassesPage controller={controller} role="Student" onPassword={()=>navigate('/account/change-password')}/>:effectivePath.startsWith('/student/invoices')?<StudentInvoicesPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<StudentEnrollmentPage onGradeReport={id=>navigate('/student/grade-reports/'+id)} controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Admissions' && snapshot.context.scope === 'NORMAL') return <AdmissionsPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if(snapshot.context?.role==='Accountant'&&snapshot.context.scope==='NORMAL')return <><nav aria-label="Kế toán" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/accountant/invoices')}>Hóa đơn và thanh toán</Button><Button onClick={()=>navigate('/accountant/reports')}>Báo cáo tài chính</Button></nav>{effectivePath==='/accountant/reports'?<ReportsPanel controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<FinancePage key={effectivePath} initialId={effectivePath.startsWith('/accountant/invoices/')?Number(effectivePath.split('/')[3]):undefined} initialPaymentId={effectivePath.startsWith('/accountant/payments/')?Number(effectivePath.split('/')[3]):undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Teacher' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Giáo viên" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/teacher/entry-test-sessions')}>Kiểm tra đầu vào</Button><Button onClick={()=>navigate('/teacher/classes')}>Lớp và lịch dạy</Button></nav>{effectivePath==='/teacher/entry-test-sessions'?<TeacherEntryPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath.endsWith('/gradebook')||effectivePath.endsWith('/grade-draft')?<GradebookPanel key={effectivePath} standalone onPassword={()=>navigate('/account/change-password')} controller={controller} classId={Number(effectivePath.split('/')[3])} initialAssignment={effectivePath.endsWith('/grade-draft')?Number(effectivePath.split('/')[5]):undefined}/>:effectivePath.includes('/attendance')?<AttendancePanel key={effectivePath} controller={controller} role="Teacher" classId={Number(effectivePath.split('/')[3])} initialSchedule={effectivePath.includes('/lessons/')?Number(effectivePath.split('/')[5]):undefined} parentBusy={false} onLock={()=>{}}/>:<ClassesPage controller={controller} role="Teacher" onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context) return <SessionPage controller={controller} snapshot={snapshot} onPassword={() => navigate('/account/change-password')} />;
  if (effectivePath === '/register') return <SignupPage client={controller.client}
    onCreated={() => { navigate('/login'); void controller.prepare(); }} onLogin={() => navigate('/login')} />;
  return <LoginPage controller={controller} snapshot={snapshot} onRegister={() => navigate('/register')} />;
}

// Same-origin M0 entry; all identity and CSRF state remains instance-owned RAM.
export function IdentityApp({ client, initialPath }: { client: HttpClient; initialPath?: string }) {
  const [entry, setEntry] = useState<{ owner: HttpClient; session: SessionController } | null>(null);
  const controller = entry?.owner === client ? entry.session : null;
  useEffect(() => {
    const session = new SessionController(client);
    const signals = createSessionSignals(() => { void session.checkSession(); });
    session.setSignalSender(signals.send);
    let active = true;
    const focus = () => { if (document.visibilityState === 'visible') void session.checkSession(); };
    const visibility = () => document.visibilityState === 'hidden' ? session.pause() : void session.checkSession();
    window.addEventListener('focus', focus); document.addEventListener('visibilitychange', visibility);
    void session.checkSession().then(() => { if (active) setEntry({ owner: client, session }); });
    return () => {
      active = false;
      window.removeEventListener('focus', focus); document.removeEventListener('visibilitychange', visibility);
      signals.close(); session.dispose();
    };
  }, [client]);
  return <IdentityShell>{controller ? <Routes controller={controller} initialPath={initialPath} /> : <Feedback kind="loading" message="Đang kiểm tra ngữ cảnh…" />}</IdentityShell>;
}
