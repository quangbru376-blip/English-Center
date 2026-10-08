import { exactKeys, record } from '../../lib/apiError';
export type AdminAccount = Readonly<{ id: number; fullName: string; email: string; phone: string | null;
  role: 'Student' | 'Admissions' | 'AcademicAffairs' | 'Teacher' | 'Accountant' | 'Director' | 'Admin';
  status: 'ACTIVE' | 'LOCKED' | 'DISABLED'; createdAt: string; mustChangePassword: boolean;
  temporaryPasswordExpiresAt: string | null; version: string; allowedActions: readonly string[]; disabledReasons: Readonly<Record<string, string>> }>;
export type LockResult = Readonly<{ account: AdminAccount; operationId: string }>;
export type LockCompletion = Readonly<{ outcome: 'COMPLETED'; operationId: string; resultMode: 'SAFE_REFERENCE'; target: Readonly<{ accountId: number }> }>;
export const validId = (value: unknown): value is number => Number.isInteger(value) && (value as number) > 0 && (value as number) <= 2147483647;
export const validUuid = (value: unknown): value is string => typeof value === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/.test(value);
const instant = (value: unknown): value is string => typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z$/.test(value) && Number.isFinite(Date.parse(value)) && new Date(value).toISOString() === value;
const actions = ['RESET_PASSWORD', 'LOCK', 'UNLOCK', 'DISABLE', 'RESTORE'];
const fields = ['id', 'fullName', 'email', 'phone', 'role', 'status', 'createdAt', 'mustChangePassword', 'temporaryPasswordExpiresAt', 'version', 'allowedActions', 'disabledReasons'];
export function decodeAdminAccount(value: unknown): AdminAccount {
  if (!record(value) || !exactKeys(value, fields) || !validId(value.id) || typeof value.fullName !== 'string' || !value.fullName || typeof value.email !== 'string' || !value.email ||
    !(value.phone === null || typeof value.phone === 'string' && value.phone) || !['Student','Admissions','AcademicAffairs','Teacher','Accountant','Director','Admin'].includes(value.role as string) ||
    !['ACTIVE','LOCKED','DISABLED'].includes(value.status as string) || !instant(value.createdAt) || typeof value.mustChangePassword !== 'boolean' ||
    !(value.temporaryPasswordExpiresAt === null || instant(value.temporaryPasswordExpiresAt)) || typeof value.version !== 'string' || !/^[A-Za-z0-9+/]{10}[AEIMQUYcgkosw048]=$/.test(value.version) ||
    !Array.isArray(value.allowedActions) || value.allowedActions.some(x=>!actions.includes(x)) || new Set(value.allowedActions).size !== value.allowedActions.length ||
    !record(value.disabledReasons) || Object.entries(value.disabledReasons).some(([key,reason])=>!actions.includes(key) || (value.allowedActions as unknown[]).includes(key) || typeof reason !== 'string' || !reason) ||
    actions.some(action=>!(value.allowedActions as unknown[]).includes(action) && !Object.hasOwn(value.disabledReasons as object,action))) throw new Error('Invalid Admin account');
  const allowed = Object.freeze([...value.allowedActions]); const disabled = Object.freeze({ ...value.disabledReasons });
  return Object.freeze({ ...value, allowedActions: allowed, disabledReasons: disabled }) as AdminAccount;
}
export function decodeLockResult(value: unknown, targetId: number): LockResult {
  if (!record(value) || !exactKeys(value, ['account','operationId']) || !validUuid(value.operationId)) throw new Error('Invalid lock result');
  const account = decodeAdminAccount(value.account); if (account.id !== targetId) throw new Error('Wrong target');
  return Object.freeze({ account, operationId: value.operationId });
}
export function decodeLockCompletion(value: unknown, targetId: number): LockCompletion {
  if (!record(value) || !exactKeys(value, ['outcome','operationId','resultMode','target']) || value.outcome !== 'COMPLETED' || !validUuid(value.operationId) ||
    value.resultMode !== 'SAFE_REFERENCE' || !record(value.target) || !exactKeys(value.target, ['accountId']) || value.target.accountId !== targetId) throw new Error('Invalid lock completion');
  return Object.freeze({ outcome:'COMPLETED',operationId:value.operationId,resultMode:'SAFE_REFERENCE',target:Object.freeze({accountId:targetId}) });
}
