const assert = require('node:assert/strict');

function assertBrowserPolicy(metadata, authenticated) {
  assert.ok(metadata, 'Metadata must be provided');
  assert.ok(Array.isArray(metadata.cookies), 'metadata.cookies must be an array');
  assert.ok(Array.isArray(metadata.javascriptCookieNames), 'metadata.javascriptCookieNames must be an array');
  assert.ok(Array.isArray(metadata.localStorageKeys), 'metadata.localStorageKeys must be an array');
  assert.ok(Array.isArray(metadata.sessionStorageKeys), 'metadata.sessionStorageKeys must be an array');

  assert.equal(metadata.localStorageKeys.length, 0, 'localStorageKeys must be empty');
  assert.equal(metadata.sessionStorageKeys.length, 0, 'sessionStorageKeys must be empty');

  const csrfCookies = metadata.cookies.filter((c) => c && c.name === '__Host-ec-csrf');
  assert.equal(csrfCookies.length, 1, 'Expected exactly one __Host-ec-csrf cookie');

  const authCookies = metadata.cookies.filter((c) => c && c.name === '__Host-ec-auth');
  if (authenticated) {
    assert.equal(authCookies.length, 1, 'Expected exactly one __Host-ec-auth cookie');
  } else {
    assert.equal(authCookies.length, 0, '__Host-ec-auth cookie must be absent when anonymous');
  }

  const expectedCookies = [csrfCookies[0]];
  if (authenticated) {
    expectedCookies.push(authCookies[0]);
  }

  for (const cookie of expectedCookies) {
    assert.equal(cookie.secure, true, 'Cookie secure must be true');
    assert.equal(cookie.httpOnly, true, 'Cookie httpOnly must be true');
    assert.equal(cookie.sameSite, 'Strict', 'Cookie sameSite must be Strict');
    assert.equal(cookie.path, '/', 'Cookie path must be /');
    assert.equal(cookie.domain, '127.0.0.1', 'Cookie domain must be 127.0.0.1');
  }

  assert.equal(
    metadata.javascriptCookieNames.includes('__Host-ec-csrf'),
    false,
    '__Host-ec-csrf must not be visible in javascriptCookieNames'
  );
  assert.equal(
    metadata.javascriptCookieNames.includes('__Host-ec-auth'),
    false,
    '__Host-ec-auth must not be visible in javascriptCookieNames'
  );

  return undefined;
}

module.exports = {
  assertBrowserPolicy,
};
