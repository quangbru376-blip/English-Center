import type { AdminAccount } from './types';
import { validUuid } from './types';
export type LockIntent = Readonly<{ targetId: number; version: string; requestId: string }>;
// Only called after deliberate confirmation; never mint while loading or retrying.
export function createLockIntent(account: AdminAccount, mint: () => string = () => crypto.randomUUID()): LockIntent {
  if (!account.allowedActions.includes('LOCK') || account.status !== 'ACTIVE') throw new Error('Unavailable lock');
  const requestId = mint(); if (!validUuid(requestId)) throw new Error('Invalid request key');
  return Object.freeze({ targetId: account.id, version: account.version, requestId });
}
