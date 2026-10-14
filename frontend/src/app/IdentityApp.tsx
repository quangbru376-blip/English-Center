import { lazy, Suspense, useCallback, useEffect, useMemo, useState, useSyncExternalStore } from 'react';
import { IdentityShell } from './IdentityShell';
import { WorkspaceShell } from './WorkspaceShell';
import type { NavigationItem } from './WorkspaceShell';
import { AccountActions } from './AccountActions';
import { BookOpen, CalendarDays, ClipboardList, GraduationCap, Home, Library, Receipt, Settings, ShieldCheck, Users } from 'lucide-react';
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
import { TeacherEntryPage } from '../features/entryTasks/TeacherEntryPage';
import { AdmissionsPage } from '../features/admissions/AdmissionsPage';
import { StudentInvoicesPage } from '../features/invoices/StudentInvoicesPage';
import { ReportsPanel,AuditPanel } from '../features/reports/ReportPanels';
import { FinancePage } from '../features/finance/FinancePage';
import { StudentGradesPage, GradeReviewPage } from '../features/classes/GradePanels';
import { GradebookPanel } from '../features/classes/GradePanels';
import { AttendancePanel } from '../features/classes/GradePanels';
import { ClassesPage } from '../features/classes/ClassesPage';
const StudentEnrollmentPage = lazy(() => import('../features/enrollment/StudentEnrollmentPage').then(module => ({ default: module.StudentEnrollmentPage })));

function Routes({ controller, initialPath }: { controller: SessionController; initialPath?: string }) {
  const subscribe = useCallback((listener: () => void) => controller.subscribe(listener), [controller]);
  const getSnapshot = useCallback(() => controller.snapshot, [controller]);
  const snapshot = useSyncExternalStore(subscribe, getSnapshot);
  const [path, setPath] = useState(initialPath ?? window.location.pathname);
  const [storedStudentView, setStudentViewRequest] = useState<{ view: 'home'|'courses'|'calendar'|'profile'; sequence: number; revision: number; owner: HttpClient }>({ view: 'home', sequence: 0, revision: controller.client.auth.revision, owner: controller.client });
  const authRevision = controller.client.auth.revision;
  const studentViewRequest = useMemo(() => storedStudentView.owner === controller.client && storedStudentView.revision === authRevision ? storedStudentView : { view: 'home' as const, sequence: 0, revision: authRevision, owner: controller.client }, [storedStudentView, controller.client, authRevision]);
  const [studentPending, setStudentPending] = useState(false);
  const effectivePath = identityPath(snapshot.context, path);
  const navigate = useCallback((next: string) => {
    if(document.querySelector('[data-unsaved-attendance="true"], [data-unsaved-grade="true"], [data-unsaved-student="true"]')&&!window.confirm('Có dữ liệu chưa lưu. Rời trang và bỏ thay đổi?'))return false;
    window.history.pushState(null, '', next); setPath(next); return true;
  }, []);
  useEffect(() => {
    const pop = () => { if(document.querySelector('[data-unsaved-attendance="true"], [data-unsaved-grade="true"], [data-unsaved-student="true"]')&&!window.confirm('Có dữ liệu chưa lưu. Rời trang và bỏ thay đổi?')){window.history.pushState(null,'',path);return;}setPath(window.location.pathname); void controller.checkSession(); };
    window.addEventListener('popstate', pop); return () => window.removeEventListener('popstate', pop);
  }, [controller,path]);
  useEffect(() => {
    if (snapshot.phase === 'checking' || snapshot.phase === 'pending' || snapshot.phase === 'unknown') return;
    if (window.location.pathname !== effectivePath) window.history.replaceState(null, '', effectivePath);
    document.getElementById('main-content')?.focus();
  }, [effectivePath, snapshot.context, snapshot.phase]);
  const requestStudentView = (view: typeof studentViewRequest.view) => {
    if (!navigate('/student/home')) return;
    setStudentViewRequest(previous => ({ view, sequence: previous.sequence + 1, revision: authRevision, owner: controller.client }));
  };
  const renderContent = () => {
  if (snapshot.phase === 'checking' || snapshot.phase === 'pending' && effectivePath !== '/account/change-password') return <Feedback kind="loading" message="Đang xác minh phiên đăng nhập…" />;
  if (snapshot.phase === 'unknown') return <>
    <Feedback kind="error" message={snapshot.notice ?? 'Chưa xác minh được phiên.'} />
    <Button className="mt-4" onClick={() => void controller.checkSession()}>Kiểm tra lại phiên</Button>
    <Button className="ml-3 mt-4" variant="secondary" onClick={() => { navigate('/login'); void controller.prepare(); }}>Đến đăng nhập</Button>
  </>;
  if (snapshot.context && effectivePath === '/account/change-password') return <PasswordChangePage controller={controller} snapshot={snapshot} onBack={() => navigate('/')} />;
  if(snapshot.context?.role==='Admin'&&snapshot.context.scope==='NORMAL')return <>{effectivePath.startsWith('/admin/audit-events')||effectivePath.startsWith('/admin/operations')?<AuditPanel key={effectivePath} controller={controller} initialId={effectivePath.startsWith('/admin/audit-events/')?Number(effectivePath.split('/')[3]):undefined} initialOperation={effectivePath.startsWith('/admin/operations/')?effectivePath.split('/')[3]:undefined} onPassword={()=>navigate('/account/change-password')}/>:<StaffAdminPage controller={controller} initialId={effectivePath.startsWith('/admin/accounts/')?Number(effectivePath.split('/').at(-1)):undefined} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if(snapshot.context?.role==='Director'&&snapshot.context.scope==='NORMAL')return <ReportsPanel controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if (snapshot.context?.role === 'AcademicAffairs' && snapshot.context.scope === 'NORMAL') return <>{effectivePath==='/academics/grade-reviews'?<GradeReviewPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/classes'?<ClassesPage controller={controller} role="AcademicAffairs" onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/courses'?<CataloguePage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/training-programs'?<ProgramPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<EntrySessionPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Student' && snapshot.context.scope === 'NORMAL') return <>{effectivePath.startsWith('/student/grade-reports')?<StudentGradesPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/student/classes'?<ClassesPage controller={controller} role="Student" onPassword={()=>navigate('/account/change-password')}/>:effectivePath.startsWith('/student/invoices')?<StudentInvoicesPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<StudentEnrollmentPage viewRequest={studentViewRequest} onBusyChange={setStudentPending} onChooseCourses={()=>requestStudentView('courses')} onInvoice={id=>navigate('/student/invoices/'+id)} onClass={()=>navigate('/student/classes')} onGradeReport={id=>navigate('/student/grade-reports/'+id)} controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Admissions' && snapshot.context.scope === 'NORMAL') return <AdmissionsPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if(snapshot.context?.role==='Accountant'&&snapshot.context.scope==='NORMAL')return <>{effectivePath==='/accountant/reports'?<ReportsPanel controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<FinancePage key={effectivePath} initialId={effectivePath.startsWith('/accountant/invoices/')?Number(effectivePath.split('/')[3]):undefined} initialPaymentId={effectivePath.startsWith('/accountant/payments/')?Number(effectivePath.split('/')[3]):undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Teacher' && snapshot.context.scope === 'NORMAL') return <>{effectivePath==='/teacher/entry-test-sessions'?<TeacherEntryPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath.endsWith('/gradebook')||effectivePath.endsWith('/grade-draft')?<GradebookPanel key={effectivePath} standalone onPassword={()=>navigate('/account/change-password')} controller={controller} classId={Number(effectivePath.split('/')[3])} initialAssignment={effectivePath.endsWith('/grade-draft')?Number(effectivePath.split('/')[5]):undefined}/>:effectivePath.includes('/attendance')?<AttendancePanel key={effectivePath} controller={controller} role="Teacher" classId={Number(effectivePath.split('/')[3])} initialSchedule={effectivePath.includes('/lessons/')?Number(effectivePath.split('/')[5]):undefined} parentBusy={false} onLock={()=>{}}/>:<ClassesPage controller={controller} role="Teacher" onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context) return <SessionPage controller={controller} snapshot={snapshot} onPassword={() => navigate('/account/change-password')} />;
  if (effectivePath === '/register') return <SignupPage client={controller.client}
    onCreated={() => { navigate('/login'); void controller.prepare(); }} onLogin={() => navigate('/login')} />;
  return <LoginPage controller={controller} snapshot={snapshot} onRegister={() => navigate('/register')} />;
  };
  const context = snapshot.context;
  if (!context || context.scope !== 'NORMAL' || snapshot.phase === 'checking' || snapshot.phase === 'unknown' || snapshot.phase === 'pending' && effectivePath !== '/account/change-password') return <IdentityShell>{renderContent()}</IdentityShell>;
  const link = (label: string, target: string, icon: NavigationItem['icon']): NavigationItem => ({ label, icon, selected: effectivePath === target || effectivePath.startsWith(target + '/') || target === '/admin/audit-events' && effectivePath.startsWith('/admin/operations/') || target === '/accountant/invoices' && effectivePath.startsWith('/accountant/payments/'), onClick: () => navigate(target) });
  const studentLink = (label: string, view: typeof studentViewRequest.view, icon: NavigationItem['icon']): NavigationItem => ({ label, icon, disabled: studentPending, selected: effectivePath === '/student/home' && studentViewRequest.view === view, onClick: () => requestStudentView(view) });
  const menus: Record<typeof context.role, NavigationItem[]> = {
    Student: [studentLink('Trang chủ', 'home', Home), studentLink('Chọn khóa học', 'courses', BookOpen), studentLink('Lịch thi', 'calendar', CalendarDays), link('Lớp của tôi', '/student/classes', GraduationCap), link('Hóa đơn của tôi', '/student/invoices', Receipt), link('Kết quả của tôi', '/student/grade-reports', ClipboardList)],
    Admissions: [link('Hồ sơ tuyển sinh', '/admissions/registrations', Users)],
    AcademicAffairs: [link('Ca thi đầu vào', '/academics/entry-test-sessions', CalendarDays), link('Khóa học và Band', '/academics/courses', BookOpen), link('Chương trình đào tạo', '/academics/training-programs', Library), link('Lớp và lịch học', '/academics/classes', GraduationCap), link('Duyệt bảng điểm', '/academics/grade-reviews', ClipboardList)],
    Teacher: [link('Lớp và lịch dạy', '/teacher/classes', GraduationCap), link('Kiểm tra đầu vào', '/teacher/entry-test-sessions', ClipboardList)],
    Accountant: [link('Hóa đơn và thanh toán', '/accountant/invoices', Receipt), link('Báo cáo tài chính', '/accountant/reports', ClipboardList)],
    Director: [link('Báo cáo tổng hợp', '/reports', ClipboardList)],
    Admin: [link('Tài khoản', '/admin/accounts', Users), link('Nhật ký kiểm toán', '/admin/audit-events', ShieldCheck)],
  };
  const content = <Suspense fallback={<Feedback kind="loading" message="Đang mở nội dung…"/>}>{renderContent()}</Suspense>;
  const standaloneAttendance = context.role === 'Teacher' && effectivePath.includes('/attendance');
  return <WorkspaceShell context={context} navigation={menus[context.role]}>{context.role === 'Student' && <AccountActions><Button disabled={studentPending} onClick={studentLink('Thông tin cá nhân', 'profile', Users).onClick}>Thông tin cá nhân</Button></AccountActions>}{standaloneAttendance && <AccountActions><Button onClick={() => navigate('/account/change-password')}><Settings className="ec-icon" aria-hidden="true"/>Đổi mật khẩu</Button><Button disabled={!snapshot.csrfReady} onClick={() => void controller.logout()}>Đăng xuất</Button></AccountActions>}{content}</WorkspaceShell>;
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
    let lastVisibility = document.visibilityState;
    const visibility = () => {
      const current = document.visibilityState;
      if (current === lastVisibility) return;
      lastVisibility = current;
      if (current === 'hidden') session.pause();
      else if (current === 'visible') void session.checkSession();
    };
    document.addEventListener('visibilitychange', visibility);
    void session.checkSession().then(() => { if (active) setEntry({ owner: client, session }); });
    return () => {
      active = false;
      document.removeEventListener('visibilitychange', visibility);
      signals.close(); session.dispose();
    };
  }, [client]);
  return controller ? <Routes controller={controller} initialPath={initialPath} /> : <IdentityShell><Feedback kind="loading" message="Đang xác minh phiên đăng nhập…" /></IdentityShell>;
}
