import type React from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';

export type SignupFields = {
  fullName: string;
  dateOfBirth: string;
  email: string;
  phone: string;
  address: string;
  password: string;
};

export type SignupFormProps = {
  values: SignupFields;
  pending: boolean;
  errors: Partial<Record<keyof SignupFields, string>>;
  onChange: (field: keyof SignupFields, value: string) => void;
  onSubmit: () => void;
};

export function SignupForm(props: SignupFormProps) {
  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!props.pending) {
      props.onSubmit();
    }
  };

  return (
    <form noValidate onSubmit={handleSubmit} className="grid w-full gap-4">
      <TextField
        id="signup-fullName"
        name="fullName"
        type="text"
        label="Họ và tên"
        value={props.values.fullName}
        error={props.errors.fullName}
        disabled={props.pending}
        required
        autoComplete="name"
        onChange={(event) => props.onChange('fullName', event.currentTarget.value)}
      />

      <TextField
        id="signup-dateOfBirth"
        name="dateOfBirth"
        type="date"
        label="Ngày sinh"
        value={props.values.dateOfBirth}
        error={props.errors.dateOfBirth}
        disabled={props.pending}
        required
        autoComplete="bday"
        onChange={(event) => props.onChange('dateOfBirth', event.currentTarget.value)}
      />

      <TextField
        id="signup-email"
        name="email"
        type="email"
        label="Email"
        value={props.values.email}
        error={props.errors.email}
        disabled={props.pending}
        required
        autoComplete="email"
        onChange={(event) => props.onChange('email', event.currentTarget.value)}
      />

      <TextField
        id="signup-phone"
        name="phone"
        type="tel"
        inputMode="tel"
        label="Số điện thoại"
        value={props.values.phone}
        error={props.errors.phone}
        disabled={props.pending}
        required
        autoComplete="tel"
        onChange={(event) => props.onChange('phone', event.currentTarget.value)}
      />

      <TextField
        id="signup-address"
        name="address"
        type="text"
        label="Địa chỉ (không bắt buộc)"
        value={props.values.address}
        error={props.errors.address}
        disabled={props.pending}
        autoComplete="street-address"
        onChange={(event) => props.onChange('address', event.currentTarget.value)}
      />

      <TextField
        id="signup-password"
        name="password"
        type="password"
        label="Mật khẩu"
        hint="15–128 ký tự; giữ nguyên khoảng trắng và ký tự Unicode."
        value={props.values.password}
        error={props.errors.password}
        disabled={props.pending}
        required
        autoComplete="new-password"
        onChange={(event) => props.onChange('password', event.currentTarget.value)}
      />

      <Button type="submit" variant="primary" pending={props.pending}>
        Đăng ký tài khoản
      </Button>
    </form>
  );
}
