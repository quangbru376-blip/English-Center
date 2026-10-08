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
import { AdminAccountPage } from '../features/administration/pages/AdminAccountPage';

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
  if (snapshot.context?.role === 'Admin' && snapshot.context.scope === 'NORMAL') return <AdminAccountPage controller={controller}
    initialId={effectivePath.startsWith('/admin/accounts/') ? Number(effectivePath.split('/').at(-1)) : undefined}
    onDetail={id=>navigate(`/admin/accounts/${id}`)} onPassword={()=>navigate('/account/change-password')} />;
  if (snapshot.context) return <SessionPage controller={controller} snapshot={snapshot} onPassword={() => navigate('/account/change-password')} />;
  if (effectivePath === '/register') return <SignupPage client={controller.client}
    onCreated={() => { navigate('/login'); void controller.prepare(); }} onLogin={() => navigate('/login')} />;
  return <LoginPage controller={controller} snapshot={snapshot} onRegister={() => navigate('/register')} />;
}

// Explicit opt-in integration seam. Default App/main do not instantiate this host.
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
