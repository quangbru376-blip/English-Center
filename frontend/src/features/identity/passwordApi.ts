import type { HttpClient, HttpResult } from '../../lib/http';

export function changeOwnPassword(client: HttpClient, currentPassword: string, newPassword: string,
  accountVersion: string, signal?: AbortSignal): Promise<HttpResult<void>> {
  const context = client.auth.context;
  if (!context || context.accountVersion !== accountVersion) return Promise.resolve({ kind: 'blocked' });
  return client.request({ endpoint: 'password', slot: 'identity-password', signal,
    body: { currentPassword, newPassword, expectedVersions: { account: accountVersion } },
    decode: (value) => { if (value !== undefined) throw new Error('Unexpected password response'); } });
}
