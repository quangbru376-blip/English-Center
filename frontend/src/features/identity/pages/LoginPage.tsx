import { useEffect, useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import { LoginForm } from '../components/LoginForm';
import type { SessionController, SessionSnapshot } from '../sessionController';

export function LoginPage({ controller, snapshot, onRegister }: {
  controller: SessionController; snapshot: SessionSnapshot; onRegister: () => void;
}) {
  const [email, setEmail] = useState(''); const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<Partial<Record<'email' | 'password', string>>>({});
  useEffect(() => controller.client.auth.requests.registerCleanup(() => { setEmail(''); setPassword(''); setErrors({}); }), [controller]);
  function submit() {
    const next: typeof errors = {};
    if (!email.trim()) next.email = 'Vui lòng nhập email.';
    if (!password) next.password = 'Vui lòng nhập mật khẩu.';
    setErrors(next); if (Object.keys(next).length) return;
    void controller.login(email, password).finally(() => setPassword(''));
  }
  return <section className="mx-auto max-w-xl">
    <PageHeader title="Đăng nhập" />
    {snapshot.notice && <div className="mb-4"><Feedback kind="error" message={snapshot.notice} /></div>}
    {snapshot.retryAfter && <p className="mb-4 text-sm">Thời gian chờ theo server: {snapshot.retryAfter}</p>}
    {!snapshot.csrfReady && <Button className="mb-4" onClick={() => void controller.prepare()}>Lấy ngữ cảnh đăng nhập</Button>}
    <fieldset disabled={!snapshot.csrfReady} className="min-w-0 border-0 p-0">
      <LoginForm email={email} password={password} errors={errors} pending={snapshot.phase === 'pending'}
        onChange={(field, value) => field === 'email' ? setEmail(value) : setPassword(value)} onSubmit={submit} />
    </fieldset>
    <div className="mt-4 flex flex-wrap gap-3">
      <Button variant="secondary" onClick={onRegister}>Đăng ký tài khoản học viên</Button>
      <Button variant="secondary" onClick={() => void controller.checkSession()}>Kiểm tra lại phiên</Button>
    </div>
  </section>;
}
