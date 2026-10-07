import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require = createRequire(import.meta.url);
const load = (file) => require(resolve(process.env.EC_TEST_BUILD, file));
const { validateSignup } = load('features/identity/signupValidation.js');
const { registerStudent, decodeRegistration, loadSignupCsrf } = load('features/identity/signupApi.js');
const { AuthState } = load('lib/auth.js');
const { HttpClient } = load('lib/http.js');
const draft = () => ({ fullName: 'Tên mẫu', dateOfBirth: '2000-01-01', email: 'sample@example.test', phone: '0901234567', address: '', password: '  Á chuỗi mẫu  ' });
test('submit validation keeps raw password and counts codepoints without composition/age rule', () => {
  const input = draft(); const before = { ...input };
  assert.deepEqual(validateSignup(input), {}); assert.deepEqual(input, before);
  assert.deepEqual(validateSignup({ ...input, password: ' '.repeat(15), dateOfBirth: '2027-01-01' }), {});
  assert.ok(validateSignup({ ...input, password: 'a'.repeat(14) }).password);
  assert.ok(validateSignup({ ...input, password: 'a'.repeat(129) }).password);
  assert.ok(validateSignup({ ...input, dateOfBirth: '2026-02-30' }).dateOfBirth);
  assert.ok(validateSignup({ ...input, fullName: ' ' }).fullName);
});
test('exact created result rejects extra identities/credentials and malformed success', () => {
  for (const value of [{ outcome: 'CREATED' }, { outcome: 'PENDING', message: 'Mẫu' }, { outcome: 'CREATED', message: 'Mẫu', accountId: 1 }]) assert.throws(() => decodeRegistration(value));
  assert.ok(Object.isFrozen(decodeRegistration({ outcome: 'CREATED', message: 'Đã tạo tài khoản.' })));
});
test('explicit csrf then signup sends only accepted keys and raw password once; never auto-login', async () => {
  const auth = new AuthState(); const calls = [];
  const client = new HttpClient(auth, async (path, init) => {
    calls.push([path, init]);
    return path.endsWith('/csrf') ? Response.json({ requestToken: 'synthetic-token' }) : Response.json({ outcome: 'CREATED', message: 'Đã tạo tài khoản.' }, { status: 201 });
  });
  await loadSignupCsrf(client); const input = { ...draft(), role: 'Admin', studentId: 1 };
  assert.equal((await registerStudent(client, input)).kind, 'success');
  const body = JSON.parse(calls[1][1].body); assert.equal(body.password, input.password);
  assert.deepEqual(Object.keys(body).sort(), ['address', 'dateOfBirth', 'email', 'fullName', 'password', 'phone']);
  assert.deepEqual(calls.map(call => call[0]), ['/api/v1/auth/csrf', '/api/v1/auth/register']); assert.equal(auth.context, null);
});
test('lost signup response stays unknown and never resends or authenticates', async () => {
  const auth = new AuthState(); auth.acceptCsrf({ requestToken: 'synthetic-token' }); let calls = 0;
  const client = new HttpClient(auth, async () => { calls++; throw new Error('Synthetic transport'); });
  assert.equal((await registerStudent(client, draft())).kind, 'unknown'); assert.equal(calls, 1); assert.equal(auth.context, null);
});
