import type { FormEvent } from 'react';
import { TextField } from '../../../components/ui/TextField';
import { Button } from '../../../components/ui/Button';

export type LoginFormProps = {
  email: string;
  password: string;
  pending: boolean;
  errors: Partial<Record<'email' | 'password', string>>;
  onChange: (field: 'email' | 'password', value: string) => void;
  onSubmit: () => void;
};

export function LoginForm({
  email,
  password,
  pending,
  errors,
  onChange,
  onSubmit,
}: LoginFormProps) {
  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!pending) {
      onSubmit();
    }
  };

  return (
    <form noValidate onSubmit={handleSubmit} className="grid gap-4">
      <TextField
        id="login-email"
        name="email"
        type="email"
        label="Email"
        autoComplete="username"
        required
        disabled={pending}
        value={email}
        error={errors.email}
        onChange={(event) => onChange('email', event.currentTarget.value)}
      />

      <TextField
        id="login-password"
        name="password"
        type="password"
        label="Mật khẩu"
        autoComplete="current-password"
        required
        disabled={pending}
        value={password}
        error={errors.password}
        onChange={(event) => onChange('password', event.currentTarget.value)}
      />

      <Button type="submit" variant="primary" pending={pending}>
        Đăng nhập
      </Button>

      <p className="text-sm text-neutral-600">
        Quên mật khẩu: liên hệ Admin để đặt lại.
      </p>
    </form>
  );
}
