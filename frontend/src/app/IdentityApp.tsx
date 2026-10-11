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
import { FinancePage } from '../features/finance/FinancePage';

function Routes({ controller, initialPath }: { controller: SessionController; initialPath?: string }) {
  const subscribe = useCallback((listener: () => void) => controller.subscribe(listener), [controller]);
  const getSnapshot = useCallback(() => controller.snapshot, [controller]);
  const snapshot = useSyncExternalStore(subscribe, getSnapshot);
  const [path, setPath] = useState(initialPath ?? window.location.pathname);
  const effectivePath = identityPath(snapshot.context, path);
  const navigate = useCallback((next: string) => {
    window.history.pushState(null, '', next); setPath(next);
  }, []);
  useEffect(() => {
    const pop = () => { setPath(window.location.pathname); void controller.checkSession(); };
    window.addEventListener('popstate', pop); return () => window.removeEventListener('popstate', pop);
  }, [controller]);
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
  if (snapshot.context?.role === 'Admin' && snapshot.context.scope === 'NORMAL') return <StaffAdminPage controller={controller} initialId={effectivePath.startsWith('/admin/accounts/')?Number(effectivePath.split('/').at(-1)):undefined} onPassword={()=>navigate('/account/change-password')} />;
  if (snapshot.context?.role === 'AcademicAffairs' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Giáo vụ" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/academics/entry-test-sessions')}>Ca thi đầu vào</Button><Button onClick={()=>navigate('/academics/courses')}>Khóa học và Band</Button><Button onClick={()=>navigate('/academics/training-programs')}>Chương trình đào tạo</Button></nav>{effectivePath==='/academics/courses'?<CataloguePage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:effectivePath==='/academics/training-programs'?<ProgramPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<EntrySessionPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Student' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Học viên" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/student/home')}>Hồ sơ và lịch thi</Button><Button onClick={()=>navigate('/student/invoices')}>Hóa đơn của tôi</Button></nav>{effectivePath.startsWith('/student/invoices')?<StudentInvoicesPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<StudentEnrollmentPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>}</>;
  if (snapshot.context?.role === 'Admissions' && snapshot.context.scope === 'NORMAL') return <AdmissionsPage key={effectivePath} initialId={Number(effectivePath.split('/')[3])||undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if (snapshot.context?.role === 'Accountant' && snapshot.context.scope === 'NORMAL') return <FinancePage key={effectivePath} initialId={effectivePath.startsWith('/accountant/invoices/')?Number(effectivePath.split('/')[3]):undefined} initialPaymentId={effectivePath.startsWith('/accountant/payments/')?Number(effectivePath.split('/')[3]):undefined} controller={controller} onPassword={()=>navigate('/account/change-password')}/>;
  if (snapshot.context?.role === 'Teacher' && snapshot.context.scope === 'NORMAL') return <><nav aria-label="Giáo viên" className="mb-4 flex flex-wrap gap-3"><Button onClick={()=>navigate('/teacher/entry-test-sessions')}>Kiểm tra đầu vào</Button><Button onClick={()=>navigate('/teacher/classes')}>Lớp và lịch dạy</Button></nav>{effectivePath==='/teacher/entry-test-sessions'?<TeacherEntryPage controller={controller} onPassword={()=>navigate('/account/change-password')}/>:<SessionPage controller={controller} snapshot={snapshot} onPassword={()=>navigate('/account/change-password')}/>}</>;
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
