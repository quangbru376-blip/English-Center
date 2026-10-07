import { exactKeys, record } from '../../lib/apiError';
import type { HttpClient, HttpResult } from '../../lib/http';
import type { RegisterStudentRequest, RegisterStudentResult } from './types';

export function decodeRegistration(value: unknown): RegisterStudentResult {
  if (!record(value) || !exactKeys(value, ['outcome', 'message']) ||
      value.outcome !== 'CREATED' || typeof value.message !== 'string' || !value.message) {
    throw new Error('Invalid registration result');
  }
  return Object.freeze({ outcome: 'CREATED', message: value.message });
}

// Explicit bootstrap only. No registration/credential retry follows this read.
export function loadSignupCsrf(client: HttpClient): Promise<HttpResult<void>> {
  return client.request({ endpoint: 'csrf', slot: 'identity-csrf', decode: (value) => {
    if (!record(value) || !exactKeys(value, ['requestToken']) || typeof value.requestToken !== 'string' || !value.requestToken) {
      throw new Error('Invalid CSRF context');
    }
  } });
}

export function registerStudent(client: HttpClient, input: RegisterStudentRequest, signal?: AbortSignal): Promise<HttpResult<RegisterStudentResult>> {
  const body: RegisterStudentRequest = {
    fullName: input.fullName, dateOfBirth: input.dateOfBirth, email: input.email,
    phone: input.phone, password: input.password,
    ...(input.address === undefined ? {} : { address: input.address }),
  };
  return client.request({ endpoint: 'register', slot: 'identity-signup', body, signal, decode: decodeRegistration });
}
