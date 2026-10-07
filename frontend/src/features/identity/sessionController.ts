import { decodeAuthContext } from '../../lib/auth';
import type { AuthContext } from '../../lib/auth';
import type { HttpClient, HttpResult } from '../../lib/http';
import { exactKeys, record } from '../../lib/apiError';

export type SessionSnapshot = Readonly<{
  phase: 'anonymous' | 'checking' | 'pending' | 'ready' | 'unknown';
  context: AuthContext | null; csrfReady: boolean; notice: string | null; retryAfter: string | null;
}>;
export type SessionClock = Readonly<{
  setTimer: (callback: () => void, milliseconds: number) => ReturnType<typeof setTimeout>;
  clearTimer: (timer: ReturnType<typeof setTimeout>) => void;
}>;
const browserClock: SessionClock = { setTimer: (callback, delay) => setTimeout(callback, delay), clearTimer: (timer) => clearTimeout(timer) };
function csrf(value: unknown): void {
  if (!record(value) || !exactKeys(value, ['requestToken']) || typeof value.requestToken !== 'string' || !value.requestToken) throw new Error('Invalid CSRF context');
}
function empty(value: unknown): void { if (value !== undefined) throw new Error('Unexpected response'); }

// Explicit lifecycle only; reads/timers/signals never renew activity or retry writes.
export class SessionController {
  #snapshot: SessionSnapshot;
  readonly #listeners = new Set<() => void>();
  #operation = 0;
  #disposed = false;
  #abort: AbortController | null = null;
  #timer: ReturnType<typeof setTimeout> | null = null;
  #timedContext: AuthContext | null = null;
  #unsubscribe: () => void;
  #sendChanged: () => void = () => {};
  constructor(readonly client: HttpClient, private readonly clock: SessionClock = browserClock) {
    this.#snapshot = Object.freeze({ phase: 'anonymous', context: client.auth.context, csrfReady: Boolean(client.auth.csrf), notice: null, retryAfter: null });
    this.#unsubscribe = client.auth.subscribe(() => this.#sync());
  }
  get snapshot(): SessionSnapshot { return this.#snapshot; }
  subscribe(listener: () => void): () => void { this.#listeners.add(listener); return () => { this.#listeners.delete(listener); }; }
  setSignalSender(sender: () => void): void { this.#sendChanged = sender; }
  #publish(update: Partial<SessionSnapshot>): void {
    if (this.#disposed) return;
    this.#snapshot = Object.freeze({ ...this.#snapshot, ...update, context: this.client.auth.context, csrfReady: Boolean(this.client.auth.csrf) });
    for (const listener of [...this.#listeners]) { try { listener(); } catch { /* Isolate view observers without logging data. */ } }
  }
  #sync(): void {
    const context = this.client.auth.context;
    if (context !== this.#timedContext) {
      if (this.#timer !== null) this.clock.clearTimer(this.#timer);
      this.#timer = null; this.#timedContext = context;
      if (context) {
        const budget = Math.min(Date.parse(context.effectiveExpiresAt), Date.parse(context.absoluteExpiresAt)) - Date.parse(context.serverTime);
        // Relative timer uses server interval and elapsed timer time, never browser wall clock.
        this.#timer = this.clock.setTimer(() => { this.#timer = null; void this.checkSession(); }, Math.max(0, Math.min(budget, 2147483647)));
      }
    }
    this.#publish({});
  }
  #begin(phase: SessionSnapshot['phase']): number {
    this.#operation++; this.#abort?.abort(); this.#abort = new AbortController();
    this.#publish({ phase, notice: null, retryAfter: null }); return this.#operation;
  }
  #current(operation: number): boolean { return !this.#disposed && operation === this.#operation; }
  async #token(operation: number): Promise<boolean> {
    const result = await this.client.request({ endpoint: 'csrf', slot: 'identity-csrf', signal: this.#abort?.signal, decode: csrf });
    return this.#current(operation) && result.kind === 'success';
  }
  async prepare(): Promise<void> {
    if (this.#disposed || this.#snapshot.phase === 'pending') return;
    const operation = this.#begin('checking'); const ready = await this.#token(operation);
    if (this.#current(operation)) this.#publish({ phase: this.client.auth.context ? 'ready' : 'anonymous', notice: ready ? null : 'Chưa lấy được ngữ cảnh gửi. Vui lòng kiểm tra lại.' });
  }
  async checkSession(): Promise<void> {
    if (this.#disposed) return;
    const interruptedWrite = this.#snapshot.phase === 'pending';
    const operation = this.#begin('checking'); this.client.auth.clear();
    const result = await this.client.request({ endpoint: 'me', slot: 'identity-me', signal: this.#abort?.signal, decode: decodeAuthContext });
    if (!this.#current(operation)) return;
    const context = this.client.auth.context;
    if (result.kind === 'success' && context && Date.parse(context.effectiveExpiresAt) > Date.parse(context.serverTime)) {
      const ready = await this.#token(operation);
      if (this.#current(operation)) this.#publish({ phase: 'ready', notice: interruptedWrite ? 'Thao tác trước chưa được xác nhận; không tự gửi lại.' : ready ? null : 'Chưa lấy được CSRF. Vui lòng kiểm tra lại trước thao tác.' });
      return;
    }
    this.client.auth.clear();
    if (result.kind === 'error' && result.status === 401) {
      await this.#token(operation);
      if (this.#current(operation)) this.#publish({ phase: 'anonymous', notice: interruptedWrite ? 'Thao tác trước chưa được xác nhận; vui lòng kiểm tra lại.' : null });
    } else this.#publish({ phase: 'unknown', notice: 'Chưa xác minh được phiên. Nội dung nhạy cảm đã được xóa; hãy kiểm tra lại.' });
  }
  async login(email: string, password: string): Promise<void> {
    if (this.#disposed || this.#snapshot.phase === 'pending' || !this.client.auth.csrf || this.client.auth.context?.scope === 'PASSWORD_CHANGE') return;
    const operation = this.#begin('pending'); const revision = this.client.auth.revision;
    const prior = this.client.auth.context;
    const result = await this.client.request({ endpoint: 'login', slot: 'identity-login', body: { email, password }, signal: this.#abort?.signal, decode: decodeAuthContext });
    if (!this.#current(operation)) return;
    if (result.kind === 'success') {
      if (this.client.auth.revision !== revision + 1) { this.#publish({ phase: this.client.auth.context ? 'ready' : 'unknown' }); return; }
      const context = this.client.auth.context;
      if (!context || Date.parse(context.effectiveExpiresAt) <= Date.parse(context.serverTime)) {
        this.client.auth.clear(); this.#sendChanged();
        this.#publish({ phase: 'unknown', notice: 'Chưa xác minh được thời hạn phiên. Hãy kiểm tra lại.' }); return;
      }
      this.#sendChanged();
      const ready = await this.#token(operation);
      if (this.#current(operation)) this.#publish({ phase: 'ready', notice: ready ? null : 'Đã xác thực; cần lấy lại CSRF trước thao tác.' });
    } else {
      if (this.client.auth.context && this.client.auth.context !== prior) { this.#publish({ phase: 'ready' }); return; }
      this.#failure(result, false);
    }
  }
  async logout(): Promise<void> {
    if (this.#disposed || this.#snapshot.phase === 'pending' || !this.client.auth.context || !this.client.auth.csrf) return;
    const prior = this.client.auth.context; const operation = this.#begin('pending');
    const result = await this.client.request({ endpoint: 'logout', slot: 'identity-logout', signal: this.#abort?.signal, decode: empty });
    if (!this.#current(operation)) return;
    if (this.client.auth.context && this.client.auth.context !== prior) { this.#publish({ phase: 'ready' }); return; }
    if (result.kind === 'success' || result.kind === 'error' && result.status === 401) {
      this.#sendChanged(); await this.#token(operation);
      if (this.#current(operation)) this.#publish({ phase: 'anonymous', notice: result.kind === 'success' ? 'Đã đăng xuất.' : 'Phiên không còn hợp lệ.' });
    } else this.#failure(result, true);
  }
  #failure(result: HttpResult<unknown>, logout: boolean): void {
    if (result.kind === 'error' && result.outcome === 'rejected') {
      const notice = result.action === 'credentials' ? 'Email hoặc mật khẩu không hợp lệ.' : result.action === 'csrf'
        ? 'Ngữ cảnh gửi không hợp lệ. Hãy kiểm tra lại; chưa xác nhận thao tác thành công.'
        : result.action === 'rate-limit' ? 'Có quá nhiều yêu cầu. Vui lòng chờ và thử lại chủ đích.' : result.error.message;
      this.#publish({ phase: this.client.auth.context ? 'ready' : 'anonymous', notice, retryAfter: result.action === 'rate-limit' ? result.retryAfter : null });
      return;
    }
    this.client.auth.clear(); this.#sendChanged();
    this.#publish({ phase: 'unknown', notice: logout ? 'Chưa xác nhận đăng xuất tại server. RAM đã xóa; không tự gửi lại.' : 'Chưa xác định kết quả đăng nhập. Hãy kiểm tra phiên; không gửi lại mật khẩu tự động.' });
  }
  pause(): void {
    if (this.#disposed) return;
    this.#begin('checking'); this.client.auth.clear();
  }
  dispose(): void {
    if (this.#disposed) return;
    this.#disposed = true; this.#operation++; this.#abort?.abort(); this.#unsubscribe();
    this.client.auth.clear();
    if (this.#timer !== null) this.clock.clearTimer(this.#timer);
    this.#listeners.clear(); this.#sendChanged = () => {};
  }
}
