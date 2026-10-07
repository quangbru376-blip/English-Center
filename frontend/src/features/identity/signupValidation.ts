import type { SignupDraft, SignupErrors } from './types';

// Submit-time hints only; server validates normalization, uniqueness, Vietnam DOB
// and the pinned offline password policy. Raw input is never rewritten here.
export function validateSignup(values: SignupDraft): SignupErrors {
  const errors: SignupErrors = {};
  const required = ['fullName', 'dateOfBirth', 'email', 'phone'] as const;
  for (const field of required) if (!values[field].trim()) errors[field] = 'Vui lòng nhập trường này.';
  const passwordLength = Array.from(values.password).length;
  if (passwordLength < 15 || passwordLength > 128) errors.password = 'Mật khẩu cần từ 15 đến 128 ký tự.';
  const limits = { fullName: 100, email: 254, address: 255 } as const;
  for (const field of Object.keys(limits) as (keyof typeof limits)[]) {
    if (Array.from(values[field].trim().normalize('NFC')).length > limits[field]) {
      errors[field] = `Trường này không được vượt quá ${limits[field]} ký tự.`;
    }
  }
  if (values.dateOfBirth && (!/^\d{4}-\d{2}-\d{2}$/.test(values.dateOfBirth) ||
      !Number.isFinite(Date.parse(values.dateOfBirth + 'T00:00:00Z')) ||
      new Date(values.dateOfBirth + 'T00:00:00Z').toISOString().slice(0, 10) !== values.dateOfBirth)) {
    errors.dateOfBirth = 'Ngày sinh không hợp lệ.';
  }
  return errors;
}
