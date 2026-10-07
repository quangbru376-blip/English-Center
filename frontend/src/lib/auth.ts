import { exactKeys, record } from './apiError';
import { RequestScope } from './requestScope';

const roles = ['Student', 'Admissions', 'AcademicAffairs', 'Teacher', 'Accountant', 'Director', 'Admin'] as const;
export type AuthContext = Readonly<{
  accountId: number; fullName: string; email: string; role: typeof roles[number];
  scope: 'NORMAL' | 'PASSWORD_CHANGE'; accountVersion: string; serverTime: string;
  absoluteExpiresAt: string; effectiveExpiresAt: string;
}>;
const fields = ['accountId', 'fullName', 'email', 'role', 'scope', 'accountVersion', 'serverTime', 'absoluteExpiresAt', 'effectiveExpiresAt'] as const;
function instant(value: unknown): value is string {
  if (typeof value !== 'string' || !/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d{1,3})?Z$/.test(value)) return false;
  const parsed = new Date(value);
  return Number.isFinite(parsed.getTime()) && parsed.toISOString().replace('.000Z', 'Z') ===
    value.replace(/\.(\d{1,3})Z$/, (_, digits: string) => `.${digits.padEnd(3, '0')}Z`).replace('.000Z', 'Z');
}
export function decodeAuthContext(value: unknown): AuthContext {
  if (!record(value) || !exactKeys(value, fields) || !Number.isInteger(value.accountId) ||
      (value.accountId as number) < 1 || (value.accountId as number) > 2147483647 ||
      typeof value.fullName !== 'string' || !value.fullName || typeof value.email !== 'string' || !value.email ||
      !roles.includes(value.role as typeof roles[number]) || !['NORMAL', 'PASSWORD_CHANGE'].includes(value.scope as string) ||
      typeof value.accountVersion !== 'string' || !/^[A-Za-z0-9+/]{10}[AEIMQUYcgkosw048]=$/.test(value.accountVersion) ||
      !instant(value.serverTime) || !instant(value.absoluteExpiresAt) || !instant(value.effectiveExpiresAt) ||
      Date.parse(value.effectiveExpiresAt) > Date.parse(value.absoluteExpiresAt)) {
    throw new Error('Invalid auth context');
  }
  return Object.freeze(Object.fromEntries(fields.map((field) => [field, value[field]]))) as AuthContext;
}

// Instance-owned RAM only. Caller explicitly marks each new login/session boundary.
export class AuthState {
  readonly requests = new RequestScope();
  #context: AuthContext | null = null;
  #csrf: string | null = null;
  #revision = 0;
  readonly #listeners = new Set<() => void>();
  get revision(): number { return this.#revision; }
  subscribe(listener: () => void): () => void {
    this.#listeners.add(listener);
    return () => { this.#listeners.delete(listener); };
  }
  #notify(): void {
    for (const listener of [...this.#listeners]) {
      try { listener(); } catch { /* One view observer cannot block other RAM cleanup/views. */ }
    }
  }
  get context(): AuthContext | null { return this.#context; }
  get csrf(): string | null { return this.#csrf; }
  clear(): void {
    this.#revision++;
    this.#context = null;
    this.#csrf = null;
    this.requests.invalidate();
    this.#notify();
  }
  startSession(value: unknown): void {
    const context = decodeAuthContext(value);
    this.clear();
    this.#context = context;
    this.#notify();
  }
  refreshContext(value: unknown): void {
    const context = decodeAuthContext(value);
    const prior = this.#context;
    if (!prior || prior.accountId !== context.accountId || prior.role !== context.role ||
        prior.scope !== context.scope || prior.accountVersion !== context.accountVersion ||
        prior.absoluteExpiresAt !== context.absoluteExpiresAt) this.clear();
    this.#context = context;
    this.#notify();
  }
  acceptCsrf(value: unknown): void {
    if (!record(value) || !exactKeys(value, ['requestToken']) || typeof value.requestToken !== 'string' ||
        !value.requestToken || /[\r\n]/.test(value.requestToken)) throw new Error('Invalid CSRF context');
    this.#csrf = value.requestToken;
    this.#notify();
  }
  requireCsrfRecovery(): void { this.#csrf = null; this.requests.invalidate(); this.#notify(); }
  canRequestBusiness(): boolean { return this.#context?.scope === 'NORMAL'; }
}
