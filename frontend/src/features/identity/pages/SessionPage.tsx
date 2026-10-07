import { Button } from '../../../components/ui/Button';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import { normalLandings } from '../../../app/identityNavigation';
import type { SessionController, SessionSnapshot } from '../sessionController';

const roleNames = { Student: 'Học viên', Admissions: 'Tuyển sinh', AcademicAffairs: 'Giáo vụ', Teacher: 'Giáo viên', Accountant: 'Kế toán', Director: 'Giám đốc', Admin: 'Quản trị viên' };

export function SessionPage({ controller, snapshot }: { controller: SessionController; snapshot: SessionSnapshot }) {
  const context = snapshot.context; if (!context) return null;
  const restricted = context.scope === 'PASSWORD_CHANGE';
  return <section>
    <PageHeader title={restricted ? 'Đổi mật khẩu bắt buộc' : normalLandings[context.role].title} />
    <details className="mb-4 border border-[var(--ec-border)] p-3">
      <summary className="min-h-[44px] cursor-pointer py-2">Tài khoản</summary>
      <p>{context.fullName}</p><p>{context.email}</p><p>Vai trò: {roleNames[context.role]}</p>
    </details>
    {snapshot.notice && <div className="mb-4"><Feedback kind="error" message={snapshot.notice} /></div>}
    <Feedback kind="empty" message={restricted
      ? 'Phiên hạn chế chỉ cho phép đổi mật khẩu và đăng xuất. Form đổi mật khẩu chưa được triển khai.'
      : 'Chức năng nghiệp vụ của màn hình này chưa được triển khai.'} />
    <div className="mt-4 flex flex-wrap gap-3">
      <Button disabled={!snapshot.csrfReady} onClick={() => void controller.logout()}>Đăng xuất</Button>
      <Button variant="secondary" onClick={() => void controller.checkSession()}>Kiểm tra lại phiên</Button>
    </div>
  </section>;
}
