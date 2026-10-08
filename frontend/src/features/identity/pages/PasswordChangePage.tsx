import { useEffect, useRef, useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import { Modal } from '../../../components/ui/Modal';
import type { SessionController, SessionSnapshot } from '../sessionController';
import { PasswordChangeForm } from '../components/PasswordChangeForm';
import { emptyPassword, validatePassword } from '../passwordValidation';
import type { PasswordErrors } from '../passwordValidation';

export function PasswordChangePage({ controller, snapshot, onBack }: {
  controller: SessionController; snapshot: SessionSnapshot; onBack: () => void;
}) {
  const [values, setValues] = useState(emptyPassword); const [errors, setErrors] = useState<PasswordErrors>({});
  const [leave, setLeave] = useState(false); const [reloadRequired, setReloadRequired] = useState(false);
  const epoch = useRef(0); const busy = useRef(false);
  const dirty = Object.values(values).some(Boolean);
  useEffect(() => {
    const cleanup = () => { epoch.current++; busy.current = false; };
    const off = controller.client.auth.requests.registerCleanup(() => {
      cleanup(); setValues(emptyPassword()); setErrors({}); setLeave(false);
    });
    return () => { cleanup(); off(); };
  }, [controller]);
  useEffect(() => {
    if (!dirty) return;
    const warn = (event: BeforeUnloadEvent) => { event.preventDefault(); };
    const pop = (event: PopStateEvent) => {
      event.stopImmediatePropagation(); window.history.pushState(null, '', '/account/change-password'); setLeave(true);
    };
    window.addEventListener('beforeunload', warn); window.addEventListener('popstate', pop, true);
    return () => { window.removeEventListener('beforeunload', warn); window.removeEventListener('popstate', pop, true); };
  }, [dirty]);
  async function submit() {
    if (busy.current || !snapshot.csrfReady || reloadRequired || !snapshot.context) return;
    const next = validatePassword(values); setErrors(next); if (Object.keys(next).length) return;
    busy.current = true; const generation = epoch.current;
    const result = await controller.changePassword(values.currentPassword, values.newPassword, snapshot.context.accountVersion);
    if (generation !== epoch.current) return;
    busy.current = false; setValues(emptyPassword());
    if (result.kind === 'error' && result.outcome === 'rejected') {
      const fields: PasswordErrors = {};
      for (const key of ['currentPassword', 'newPassword'] as const) {
        const messages = result.error.fieldErrors[key]; if (messages?.length) fields[key] = messages.join(' ');
      }
      setErrors(fields); if (result.action === 'conflict') setReloadRequired(true);
    }
  }
  const pending = snapshot.phase === 'pending';
  return <section className="mx-auto max-w-xl">
    <PageHeader title={snapshot.context?.scope === 'PASSWORD_CHANGE' ? 'Đổi mật khẩu bắt buộc' : 'Đổi mật khẩu'} />
    {snapshot.notice && <div className="mb-4"><Feedback kind="error" message={snapshot.notice} /></div>}
    {reloadRequired && <p className="mb-4">Cần tải lại thông tin tài khoản trước khi gửi mới.</p>}
    <fieldset disabled={!snapshot.csrfReady || reloadRequired} className="min-w-0 border-0 p-0">
      <PasswordChangeForm values={values} errors={errors} pending={pending}
        onChange={(field, value) => setValues(previous => ({ ...previous, [field]: value }))} onSubmit={() => void submit()} />
    </fieldset>
    <div className="mt-4 flex flex-wrap gap-3">
      <Button disabled={pending} variant="secondary" onClick={() => { setReloadRequired(false); void controller.checkSession(); }}>Tải lại ngữ cảnh</Button>
      {snapshot.context?.scope === 'NORMAL' && <Button disabled={pending} variant="secondary" onClick={() => dirty ? setLeave(true) : onBack()}>Quay lại</Button>}
      <Button disabled={pending || !snapshot.csrfReady} variant="secondary" onClick={() => void controller.logout()}>Đăng xuất</Button>
    </div>
    <Modal open={leave} title="Bỏ dữ liệu chưa gửi?" onRequestClose={() => setLeave(false)}>
      <p>Mật khẩu chưa gửi sẽ bị xóa khi rời màn hình.</p>
      <Button variant="danger" onClick={() => { setValues(emptyPassword()); setLeave(false); onBack(); }}>Bỏ thay đổi</Button>
    </Modal>
  </section>;
}
