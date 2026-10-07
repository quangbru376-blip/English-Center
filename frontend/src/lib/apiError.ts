export type ApiError = Readonly<{
  code: string; message: string; fieldErrors: Readonly<Record<string, readonly string[]>>; traceId: string;
}>;

export function record(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}

export function exactKeys(value: Record<string, unknown>, keys: readonly string[]): boolean {
  return Object.keys(value).length === keys.length && keys.every((key) => Object.hasOwn(value, key));
}

export function decodeApiError(value: unknown): ApiError {
  if (!record(value) || !exactKeys(value, ['code', 'message', 'fieldErrors', 'traceId']) ||
      typeof value.code !== 'string' || !value.code || typeof value.message !== 'string' ||
      typeof value.traceId !== 'string' || !value.traceId || !record(value.fieldErrors)) {
    throw new Error('Invalid error envelope');
  }
  const fields: Record<string, readonly string[]> = Object.create(null) as Record<string, readonly string[]>;
  for (const [key, messages] of Object.entries(value.fieldErrors)) {
    if (!Array.isArray(messages) || !messages.every((message: unknown) => typeof message === 'string')) {
      throw new Error('Invalid field errors');
    }
    fields[key] = Object.freeze([...messages]) as readonly string[];
  }
  return Object.freeze({ code: value.code, message: value.message, fieldErrors: Object.freeze(fields), traceId: value.traceId });
}

export type ErrorAction = 'fields' | 'credentials' | 'session' | 'forbidden' | 'csrf' | 'not-found' | 'conflict' | 'rate-limit' | 'server';
export function errorAction(status: number, code: string): ErrorAction {
  if (status === 401) return code === 'AUTHENTICATION_FAILED' ? 'credentials' : 'session';
  if (status === 403) return code === 'CSRF_INVALID' ? 'csrf' : 'forbidden';
  if (status === 400) return 'fields';
  if (status === 404) return 'not-found';
  if (status === 409) return 'conflict';
  if (status === 429) return 'rate-limit';
  return 'server';
}
