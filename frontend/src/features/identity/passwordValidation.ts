export type PasswordDraft = { currentPassword: string; newPassword: string; confirmation: string };
export type PasswordErrors = Partial<Record<keyof PasswordDraft, string>>;
export const emptyPassword = (): PasswordDraft => ({ currentPassword: '', newPassword: '', confirmation: '' });
export function validatePassword(values: PasswordDraft): PasswordErrors {
  const errors: PasswordErrors = {};
  if (!values.currentPassword) errors.currentPassword = 'Vui lòng nhập mật khẩu hiện tại.';
  const length = Array.from(values.newPassword).length;
  if (length < 15 || length > 128) errors.newPassword = 'Mật khẩu mới cần từ 15 đến 128 ký tự.';
  if (values.confirmation !== values.newPassword) errors.confirmation = 'Mật khẩu nhập lại chưa khớp.';
  return errors;
}
