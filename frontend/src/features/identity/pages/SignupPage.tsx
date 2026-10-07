import { useEffect, useRef, useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import type { HttpClient } from '../../../lib/http';
import { SignupForm } from '../components/SignupForm';
import { loadSignupCsrf, registerStudent } from '../signupApi';
import { emptySignup } from '../types';
import type { SignupErrors } from '../types';
import { validateSignup } from '../signupValidation';

type Notice = { message: string; traceId?: string; kind: 'error' | 'success' };
export function SignupPage({ client, onCreated, onLogin }: {
  client: HttpClient; onCreated: () => void; onLogin: () => void;
}) {
  const [values, setValues] = useState(emptySignup);
  const [errors, setErrors] = useState<SignupErrors>({});
  const [notice, setNotice] = useState<Notice | null>(null);
  const [ready, setReady] = useState(Boolean(client.auth.csrf));
  const [pending, setPending] = useState(false);
  const busy = useRef(false);
  const generation = useRef(0);
  const active = useRef<AbortController | null>(null);

  useEffect(() => {
    const cancel = () => { generation.current++; active.current?.abort(); };
    const unregister = client.auth.requests.registerCleanup(() => {
      cancel(); busy.current = false;
      setValues(emptySignup()); setErrors({}); setReady(false); setPending(false);
      setNotice({ kind: 'error', message: 'Ngữ cảnh đã thay đổi. Vui lòng kiểm tra lại trước khi gửi.' });
    });
    return () => { cancel(); unregister(); };
  }, [client]);

  async function prepare() {
    if (busy.current || client.auth.context) return;
    busy.current = true; setPending(true); setNotice(null);
    const current = generation.current;
    const result = await loadSignupCsrf(client);
    if (current !== generation.current) return;
    setReady(result.kind === 'success');
    if (result.kind !== 'success') setNotice({ kind: 'error', message: 'Chưa lấy được ngữ cảnh đăng ký. Vui lòng kiểm tra lại.' });
    busy.current = false; setPending(false);
  }

  async function submit() {
    if (busy.current || !ready || client.auth.context) return;
    const validation = validateSignup(values); setErrors(validation); setNotice(null);
    if (Object.keys(validation).length) return;
    busy.current = true; setPending(true); active.current = new AbortController();
    const current = generation.current;
    const result = await registerStudent(client, values, active.current.signal);
    if (current !== generation.current) return;
    busy.current = false; setPending(false); active.current = null;
    setValues((previous) => ({ ...previous, password: '' }));
    if (result.kind === 'success') {
      setValues(emptySignup()); setErrors({}); onCreated(); return;
    }
    if (result.kind === 'error' && result.outcome === 'rejected') {
      const fields: SignupErrors = {};
      for (const field of Object.keys(emptySignup()) as (keyof SignupErrors)[]) {
        const messages = result.error.fieldErrors[field];
        if (messages?.length) fields[field] = messages.join(' ');
      }
      setErrors(fields);
      setNotice({ kind: 'error', message: result.action === 'conflict'
        ? 'Thông tin đang có xung đột. Hãy kiểm tra lại; hệ thống không tự gửi lại.'
        : result.error.message, traceId: result.error.traceId });
      return;
    }
    setReady(false);
    setNotice({ kind: 'error', message: result.kind === 'blocked' || result.kind === 'discarded'
      ? 'Chưa gửi đăng ký. Vui lòng lấy lại ngữ cảnh.'
      : 'Chưa xác định tài khoản đã được tạo hay chưa. Không gửi lại tự động; hãy chủ đích đăng nhập hoặc liên hệ Admin để đặt lại mật khẩu.' });
  }

  if (client.auth.context) return <Feedback kind="error" message="Bạn đã đăng nhập; không thể tạo tài khoản từ ngữ cảnh này." />;
  return (
    <section className="mx-auto max-w-xl">
      <PageHeader title="Đăng ký tài khoản học viên" />
      {notice && <div className="mb-4"><Feedback {...notice} /></div>}
      {!ready && <div className="mb-4"><Button pending={pending} onClick={() => void prepare()}>Lấy ngữ cảnh đăng ký</Button></div>}
      <fieldset disabled={!ready} className="min-w-0 border-0 p-0">
        <SignupForm values={values} errors={errors} pending={pending}
          onChange={(field, value) => setValues((previous) => ({ ...previous, [field]: value }))}
          onSubmit={() => void submit()} />
      </fieldset>
      <Button className="mt-4" variant="secondary" disabled={pending} onClick={onLogin}>Đến đăng nhập</Button>
    </section>
  );
}
