import type { AuthContext } from '../lib/auth';
export const normalLandings: Record<AuthContext['role'], Readonly<{ path: string; title: string }>> = {
  Student: { path: '/student/home', title: 'Trang chủ học viên' },
  Admissions: { path: '/admissions/registrations', title: 'Hồ sơ tuyển sinh' },
  AcademicAffairs: { path: '/academics/entry-test-sessions', title: 'Ca thi đầu vào' },
  Teacher: { path: '/teacher/classes', title: 'Lớp và lịch dạy' },
  Accountant: { path: '/finance/invoices', title: 'Hóa đơn và thanh toán' },
  Director: { path: '/reports', title: 'Báo cáo tổng hợp' },
  Admin: { path: '/admin/accounts', title: 'Tài khoản' },
};
export function identityPath(context: AuthContext | null, requested: string): string {
  if (!context) return requested === '/register' ? '/register' : '/login';
  if (context.scope === 'PASSWORD_CHANGE') return '/account/change-password';
  if (requested === '/account/change-password') return requested;
  if (context.role === 'Admin' && /^\/admin\/accounts\/[1-9]\d{0,9}$/.test(requested) && Number(requested.split('/').at(-1)) <= 2147483647) return requested;
  return normalLandings[context.role].path;
}
