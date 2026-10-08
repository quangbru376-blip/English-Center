import type { HttpClient } from '../../lib/http';
import { decodeAdminAccount, decodeLockCompletion, decodeLockResult } from './types';
import type { LockIntent } from './lockIntent';
export const loadAdminAccount = (client: HttpClient, targetId: number, signal?: AbortSignal) => client.request({
  endpoint: 'adminDetail', slot: 'admin-detail', targetId, signal, decode: value => {
    const account=decodeAdminAccount(value); if(account.id!==targetId)throw new Error('Wrong target'); return account;
  } });
export const lockAdminAccount = (client: HttpClient, intent: LockIntent, signal?: AbortSignal) => client.request({
  endpoint: 'adminLock', slot: 'admin-lock', targetId: intent.targetId, requestId: intent.requestId, signal,
  body: { expectedVersions: { account: intent.version } }, decode: value => decodeLockResult(value,intent.targetId) });
export const lookupAdminLock = (client: HttpClient, requestId: string, targetId: number, signal?: AbortSignal) => client.request({
  endpoint: 'adminLockResult', slot: 'admin-lock-result', requestId, signal, decode: value => decodeLockCompletion(value,targetId) });
