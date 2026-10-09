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
  adminDetail: { path: '/api/v1/admin/accounts', method: 'GET', intent: 'read', access: 'admin' },
  adminLock: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminLockResult: { path: '/api/v1/admin/command-results', method: 'GET', intent: 'read', access: 'admin' },
  adminSearch: { path: '/api/v1/admin/accounts/search', method: 'POST', intent: 'read', access: 'admin' },
  staffCreate: { path: '/api/v1/admin/staff-accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminReset: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminUnlock: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminDisable: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminRestore: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminResult: { path: '/api/v1/admin/command-results', method: 'GET', intent: 'read', access: 'admin' },
  courseSearch: { path: '/api/v1/academics/courses', method: 'GET', intent: 'read', access: 'academic' },
  courseDetail: { path: '/api/v1/academics/courses', method: 'GET', intent: 'read', access: 'academic' },
  courseCreate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  courseUpdate: { path: '/api/v1/academics/courses', method: 'PUT', intent: 'write', access: 'academic' },
  courseDeactivate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  courseActivate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  bands: { path: '/api/v1/academics/bands', method: 'GET', intent: 'read', access: 'academic' },
  courseResult: { path: '/api/v1/academics/command-results', method: 'GET', intent: 'read', access: 'academic' },
  programSearch:{path:'/api/v1/academics/training-programs',method:'GET',intent:'read',access:'academic'},
  programDetail:{path:'/api/v1/academics/training-programs',method:'GET',intent:'read',access:'academic'},
  programCreate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programEdit:{path:'/api/v1/academics/training-programs',method:'PUT',intent:'write',access:'academic'},
  programRename:{path:'/api/v1/academics/training-programs',method:'PUT',intent:'write',access:'academic'},
  programPreview:{path:'/api/v1/academics/training-programs',method:'POST',intent:'read',access:'academic'},
  programActivate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programDraft:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programDeactivate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'academic'},
  sessionSearch:{path:'/api/v1/admissions/entry-test-sessions',method:'GET',intent:'read',access:'academic'},
  sessionDetail:{path:'/api/v1/admissions/entry-test-sessions',method:'GET',intent:'read',access:'academic'},
  sessionCreate:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionSchedule:{path:'/api/v1/admissions/entry-test-sessions',method:'PUT',intent:'write',access:'academic'},
  sessionOpen:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionClose:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionCancelPreview:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionCancel:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionGraderPreview:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionGrader:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionRoster:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionOptions:{path:'/api/v1/admissions/entry-test-grader-options',method:'GET',intent:'read',access:'academic'},
  sessionResult:{path:'/api/v1/admissions/command-results',method:'GET',intent:'read',access:'academic'},
} as const;
export type Endpoint = keyof typeof endpoints;
export type HttpResult<T> =
  | Readonly<{ kind: 'success'; value: T }>
  | Readonly<{ kind: 'error'; status: number; error: ApiError; action: ErrorAction; retryAfter: string | null; outcome: 'rejected' | 'unknown' }>
  | Readonly<{ kind: 'discarded' | 'unavailable' | 'unknown' | 'blocked' }>;
export type RequestOptions<T> = {
  endpoint: Endpoint; slot: string; decode: (value: unknown) => T;
  body?: unknown; signal?: AbortSignal;
  targetId?: number; requestId?: string;
  query?: Readonly<Record<string,string>>;
};

// No automatic dispatch, retry, UUID generation, storage, token parsing or logging.
export class HttpClient {
  constructor(readonly auth: AuthState, private readonly fetcher: typeof fetch = (input, init) => globalThis.fetch(input, init)) {}
  async request<T>(options: RequestOptions<T>): Promise<HttpResult<T>> {
    if (!Object.hasOwn(endpoints, options.endpoint)) return { kind: 'blocked' };
    const endpoint = endpoints[options.endpoint];
    if (!endpoint || (endpoint.intent === 'read' && !['adminSearch','sessionRoster','sessionGraderPreview'].includes(options.endpoint) && options.body !== undefined)) return { kind: 'blocked' };
    if (endpoint.access === 'session' && !this.auth.context) return { kind: 'blocked' };
    if (endpoint.access === 'admin' && (this.auth.context?.role !== 'Admin' || this.auth.context.scope !== 'NORMAL')) return { kind: 'blocked' };
    if (endpoint.access === 'academic' && (this.auth.context?.role !== 'AcademicAffairs' || this.auth.context.scope !== 'NORMAL')) return { kind:'blocked' };
    const suffixes:Partial<Record<Endpoint,string>>={adminLock:'lock',adminReset:'reset-password',adminUnlock:'unlock',adminDisable:'disable',adminRestore:'restore',courseUpdate:'details',courseDeactivate:'deactivate',courseActivate:'activate'};
    Object.assign(suffixes,{programEdit:'configuration',programRename:'name',programPreview:'activation-preview',programActivate:'activate',programDraft:'return-to-draft',programDeactivate:'deactivate',sessionSchedule:'schedule',sessionOpen:'open',sessionClose:'close',sessionCancelPreview:'cancellation-preview',sessionCancel:'cancel',sessionGraderPreview:'grader-change-preview',sessionGrader:'change-grader',sessionRoster:'roster/search'});
    const lookup=['adminLockResult','adminResult','courseResult','programResult','sessionResult'].includes(options.endpoint);
    const needsTarget = ['adminDetail','courseDetail','programDetail','sessionDetail'].includes(options.endpoint) || Object.hasOwn(suffixes,options.endpoint);
    const needsKey = lookup || endpoint.intent==='write' && (endpoint.access==='admin'||endpoint.access==='academic');
    if (needsTarget && (!Number.isInteger(options.targetId) || options.targetId! < 1 || options.targetId! > 2147483647) ||
        !needsTarget && options.targetId !== undefined ||
        needsKey && (typeof options.requestId !== 'string' || !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/.test(options.requestId)) ||
        !needsKey && options.requestId !== undefined) return { kind: 'blocked' };
    const allowedQuery=options.endpoint==='courseSearch'?['code','name','status','sort','page','pageSize']:options.endpoint==='programSearch'?['courseId','bandId','name','status','sort','page','pageSize']:options.endpoint==='sessionSearch'?['fromDate','toDate','effectiveStatus','graderId','sort','page','pageSize']:options.endpoint==='sessionOptions'?['date','startTime','endTime','sessionId','page','pageSize']:lookup?['action']:[];
    if(options.query&&Object.keys(options.query).some(key=>!allowedQuery.includes(key)))return {kind:'blocked'};
    const action=options.query?.action;
    if(options.endpoint==='programResult'&&(!action||!['ACADEMIC.PROGRAM.CREATE','ACADEMIC.PROGRAM.UPDATE_CONFIGURATION','ACADEMIC.PROGRAM.RENAME','ACADEMIC.PROGRAM.ACTIVATE','ACADEMIC.PROGRAM.RETURN_TO_DRAFT','ACADEMIC.PROGRAM.DEACTIVATE'].includes(action))||options.endpoint==='sessionResult'&&(!action||!['ACADEMIC.ENTRY_SESSION.CREATE','ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE','ACADEMIC.ENTRY_SESSION.OPEN','ACADEMIC.ENTRY_SESSION.CLOSE','ACADEMIC.ENTRY_SESSION.CANCEL','ACADEMIC.ENTRY_SESSION.CHANGE_GRADER'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='adminResult'&&(!action||!['ADMIN.STAFF.CREATE','ADMIN.ACCOUNT.RESET_PASSWORD','ADMIN.ACCOUNT.LOCK','ADMIN.ACCOUNT.UNLOCK','ADMIN.ACCOUNT.DISABLE','ADMIN.ACCOUNT.RESTORE'].includes(action))||options.endpoint==='courseResult'&&(!action||!['ACADEMIC.COURSE.CREATE','ACADEMIC.COURSE.UPDATE_DETAILS','ACADEMIC.COURSE.DEACTIVATE','ACADEMIC.COURSE.ACTIVATE'].includes(action)))return {kind:'blocked'};
    const query=options.endpoint==='adminLockResult'?'action=ADMIN.ACCOUNT.LOCK':new URLSearchParams(options.query).toString();
    const path=(lookup?`${endpoint.path}/${options.requestId}`:needsTarget?`${endpoint.path}/${options.targetId}${suffixes[options.endpoint]?'/'+suffixes[options.endpoint]:''}`:endpoint.path)+(query?'?'+query:'');
    if (options.endpoint === 'login' && this.auth.context?.scope === 'PASSWORD_CHANGE') return { kind: 'blocked' };
    if (options.endpoint === 'register' && this.auth.context) return { kind: 'blocked' };
    const token = this.auth.csrf;
    if (endpoint.method !== 'GET' && !token) return { kind: 'blocked' };
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
      if (endpoint.method !== 'GET') headers['X-CSRF-TOKEN'] = token!;
      if (needsKey && !lookup) headers['Idempotency-Key'] = options.requestId!;
      let body: string | undefined;
      if (options.body !== undefined) { body = JSON.stringify(options.body); headers['Content-Type'] = 'application/json'; }
      sent = true;
      const response = await this.fetcher(path, {
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
        if (response.status !== expectedStatus && !(response.status===201&&['staffCreate','courseCreate','programCreate','sessionCreate'].includes(options.endpoint))) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
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
