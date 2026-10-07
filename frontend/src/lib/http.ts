import { decodeApiError, errorAction } from './apiError';
import type { ApiError, ErrorAction } from './apiError';
import { AuthState } from './auth';

const endpoints = {
  csrf: { path: '/api/v1/auth/csrf', method: 'GET', intent: 'read', access: 'public' },
  me: { path: '/api/v1/auth/me', method: 'GET', intent: 'read', access: 'public' },
  register: { path: '/api/v1/auth/register', method: 'POST', intent: 'write', access: 'public' },
  login: { path: '/api/v1/auth/login', method: 'POST', intent: 'write', access: 'public' },
  logout: { path: '/api/v1/auth/logout', method: 'POST', intent: 'write', access: 'session' },
  password: { path: '/api/v1/auth/change-password', method: 'POST', intent: 'write', access: 'session' },
} as const;
export type Endpoint = keyof typeof endpoints;
export type HttpResult<T> =
  | Readonly<{ kind: 'success'; value: T }>
  | Readonly<{ kind: 'error'; status: number; error: ApiError; action: ErrorAction; retryAfter: string | null; outcome: 'rejected' | 'unknown' }>
  | Readonly<{ kind: 'discarded' | 'unavailable' | 'unknown' | 'blocked' }>;
export type RequestOptions<T> = {
  endpoint: Endpoint; slot: string; decode: (value: unknown) => T;
  body?: unknown; signal?: AbortSignal;
};

// No automatic dispatch, retry, UUID generation, storage, token parsing or logging.
export class HttpClient {
  constructor(readonly auth: AuthState, private readonly fetcher: typeof fetch = fetch) {}
  async request<T>(options: RequestOptions<T>): Promise<HttpResult<T>> {
    if (!Object.hasOwn(endpoints, options.endpoint)) return { kind: 'blocked' };
    const endpoint = endpoints[options.endpoint];
    if (!endpoint || (endpoint.intent === 'read' && options.body !== undefined)) return { kind: 'blocked' };
    if (endpoint.access === 'session' && !this.auth.context) return { kind: 'blocked' };
    if (options.endpoint === 'login' && this.auth.context?.scope === 'PASSWORD_CHANGE') return { kind: 'blocked' };
    if (options.endpoint === 'register' && this.auth.context) return { kind: 'blocked' };
    const token = this.auth.csrf;
    if (endpoint.intent === 'write' && !token) return { kind: 'blocked' };
    const ticket = this.auth.requests.begin(options.slot);
    const controller = new AbortController();
    const abort = () => controller.abort();
    ticket.signal.addEventListener('abort', abort, { once: true });
    options.signal?.addEventListener('abort', abort, { once: true });
    if (ticket.signal.aborted || options.signal?.aborted) abort();
    let sent = false;
    const uncertainWrite = (): HttpResult<T> => {
      if (ticket.isCurrent() && ['login', 'logout', 'password'].includes(options.endpoint)) this.auth.clear();
      return { kind: 'unknown' };
    };
    try {
      if (controller.signal.aborted) return { kind: 'discarded' };
      const headers: Record<string, string> = { Accept: 'application/json' };
      if (endpoint.intent === 'write') headers['X-CSRF-TOKEN'] = token!;
      let body: string | undefined;
      if (options.body !== undefined) { body = JSON.stringify(options.body); headers['Content-Type'] = 'application/json'; }
      sent = true;
      const response = await this.fetcher(endpoint.path, {
        method: endpoint.method, credentials: 'same-origin', cache: 'no-store', redirect: 'error',
        headers, body, signal: controller.signal,
      });
      body = undefined;
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
      let payload: unknown;
      try { payload = response.status === 204 ? undefined : await response.json(); }
      catch {
        if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
        if (response.status === 401 && options.endpoint !== 'login') this.auth.clear();
        if (response.status === 403) this.auth.requireCsrfRecovery();
        return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
      }
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
      if (response.ok) {
        const expectedStatus = options.endpoint === 'register' ? 201 :
          options.endpoint === 'logout' || options.endpoint === 'password' ? 204 : 200;
        if (response.status !== expectedStatus) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
        let value: T;
        try { value = options.decode(payload); }
        catch {
          if (options.endpoint === 'me') this.auth.clear();
          if (options.endpoint === 'csrf') this.auth.requireCsrfRecovery();
          return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
        }
        if (options.endpoint === 'csrf') {
          try { this.auth.acceptCsrf(payload); }
          catch { this.auth.requireCsrfRecovery(); return { kind: 'unavailable' }; }
        }
        if (options.endpoint === 'me') {
          try { this.auth.refreshContext(payload); }
          catch { this.auth.clear(); return { kind: 'unavailable' }; }
        }
        if (options.endpoint === 'login') this.auth.startSession(payload);
        if (options.endpoint === 'logout' || options.endpoint === 'password') this.auth.clear();
        return { kind: 'success', value };
      }
      let error: ApiError;
      try { error = decodeApiError(payload); }
      catch {
        if (response.status === 401 && options.endpoint !== 'login') this.auth.clear();
        if (response.status === 403) this.auth.requireCsrfRecovery();
        return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
      }
      const action = errorAction(response.status, error.code);
      // Only a still-current response may invalidate a current identity.
      if (action === 'session') this.auth.clear();
      if (action === 'csrf') this.auth.requireCsrfRecovery();
      if (endpoint.intent === 'write' && response.status >= 500) uncertainWrite();
      return { kind: 'error', status: response.status, error, action,
        retryAfter: response.status === 429 ? response.headers.get('Retry-After') : null,
        outcome: endpoint.intent === 'write' && response.status >= 500 ? 'unknown' : 'rejected' };
    } catch {
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' && sent ? uncertainWrite() : { kind: 'discarded' };
      return endpoint.intent === 'write' && sent ? uncertainWrite() : { kind: 'unavailable' };
    } finally {
      ticket.signal.removeEventListener('abort', abort);
      options.signal?.removeEventListener('abort', abort);
      ticket.finish();
    }
  }
}
