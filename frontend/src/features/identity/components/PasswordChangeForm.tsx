import type { FormEvent } from 'react';
import { TextField } from '../../../components/ui/TextField';
import { Button } from '../../../components/ui/Button';

export interface PasswordFields {
  currentPassword: string;
  newPassword: string;
  confirmation: string;
}

export interface PasswordChangeFormProps {
  values: PasswordFields;
  errors: Partial<Record<keyof PasswordFields, string>>;
  pending: boolean;
  onChange: (field: keyof PasswordFields, value: string) => void;
  onSubmit: () => void;
}

export function PasswordChangeForm({
  values,
  errors,
  pending,
  onChange,
  onSubmit,
}: PasswordChangeFormProps) {
  const handleSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!pending) {
      onSubmit();
    }
  };

  return (
    <form onSubmit={handleSubmit} noValidate className="grid gap-4">
      <TextField
        id="password-current"
        name="currentPassword"
        type="password"
        label="Mật khẩu hiện tại"
        value={values.currentPassword}
        onChange={(e) => onChange('currentPassword', e.currentTarget.value)}
        error={errors.currentPassword}
        autoComplete="current-password"
        required
        disabled={pending}
      />

      <TextField
        id="password-new"
        name="newPassword"
        type="password"
        label="Mật khẩu mới"
        value={values.newPassword}
        onChange={(e) => onChange('newPassword', e.currentTarget.value)}
        error={errors.newPassword}
        autoComplete="new-password"
        required
        disabled={pending}
      />

      <TextField
        id="password-confirmation"
        name="confirmation"
        type="password"
        label="Nhập lại mật khẩu mới"
        value={values.confirmation}
        onChange={(e) => onChange('confirmation', e.currentTarget.value)}
        error={errors.confirmation}
        autoComplete="new-password"
        required
        disabled={pending}
      />

      <p className="text-sm text-neutral-600">
        Đổi thành công sẽ hủy mọi phiên và yêu cầu đăng nhập lại.
      </p>

      <Button type="submit" variant="primary" pending={pending}>
        Đổi mật khẩu
      </Button>
    </form>
  );
}
