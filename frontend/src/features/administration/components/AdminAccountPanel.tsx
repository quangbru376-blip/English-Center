import { Button } from '../../../components/ui/Button';

export interface AdminAccountPanelProps {
  account: {
    id: number;
    fullName: string;
    email: string;
    phone: string | null;
    role: string;
    status: string;
    mustChangePassword: boolean;
  };
  createdLabel: string;
  temporaryExpiryLabel: string;
  canLock: boolean;
  lockDisabledReason: string | null;
  pending: boolean;
  onRequestLock: () => void;
  onReload: () => void;
}

export function AdminAccountPanel({
  account,
  createdLabel,
  temporaryExpiryLabel,
  canLock,
  lockDisabledReason,
  pending,
  onRequestLock,
  onReload,
}: AdminAccountPanelProps) {
  return (
    <section className="rounded-[var(--ec-radius)] border border-[var(--ec-border)] bg-[var(--ec-surface)] p-6">
      <h2 className="mb-4 text-lg font-semibold text-[var(--ec-text)]">Chi tiết tài khoản</h2>

      <dl className="grid grid-cols-1 gap-x-6 gap-y-4 sm:grid-cols-2">
        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Mã tài khoản</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.id}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Họ và tên</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.fullName}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Email</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.email}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Số điện thoại</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.phone ?? 'Không có'}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Vai trò</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.role}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Trạng thái</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.status}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Ngày tạo</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{createdLabel}</dd>
        </div>

        <div className="min-w-0 break-words">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Yêu cầu đổi mật khẩu</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{account.mustChangePassword ? 'Có' : 'Không'}</dd>
        </div>

        <div className="min-w-0 break-words sm:col-span-2">
          <dt className="text-sm font-medium text-[var(--ec-muted)]">Hạn mật khẩu tạm</dt>
          <dd className="mt-1 text-sm text-[var(--ec-text)]">{temporaryExpiryLabel}</dd>
        </div>
      </dl>

      <div className="mt-6 flex flex-wrap items-center gap-3">
        <Button
          variant="danger"
          disabled={!canLock || pending}
          pending={pending}
          onClick={onRequestLock}
        >
          Khóa tài khoản
        </Button>
        <Button
          variant="secondary"
          disabled={pending}
          onClick={onReload}
        >
          Tải lại
        </Button>
      </div>

      {!canLock && (
        <p className="mt-3 text-sm text-[var(--ec-text)]">
          {lockDisabledReason || 'Không thể khóa trong trạng thái hiện tại.'}
        </p>
      )}

      <p className="mt-2 text-xs text-[var(--ec-muted)]">
        Khóa sẽ hủy mọi phiên của tài khoản; không xóa dữ liệu.
      </p>
    </section>
  );
}
