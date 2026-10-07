import { IdentityShell } from './IdentityShell';
import { PageHeader } from '../components/ui/PageHeader';

export default function App() {
  return (
    <IdentityShell>
      <PageHeader title="Quản lý trung tâm tiếng Anh" />
      <p className="text-[var(--ec-muted)]">Các chức năng hiện chưa khả dụng.</p>
    </IdentityShell>
  );
}
