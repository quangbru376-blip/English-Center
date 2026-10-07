export type SignupDraft = {
  fullName: string; dateOfBirth: string; email: string; phone: string;
  address: string; password: string;
};
export type SignupErrors = Partial<Record<keyof SignupDraft, string>>;
export type RegisterStudentRequest = Readonly<{
  fullName: string; dateOfBirth: string; email: string; phone: string;
  password: string; address?: string | null;
}>;
export type RegisterStudentResult = Readonly<{ outcome: 'CREATED'; message: string }>;
export function emptySignup(): SignupDraft {
  return { fullName: '', dateOfBirth: '', email: '', phone: '', address: '', password: '' };
}
