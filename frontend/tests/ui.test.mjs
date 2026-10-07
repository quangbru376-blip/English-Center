import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

const require = createRequire(import.meta.url);
const load = (file) => require(resolve(process.env.EC_TEST_BUILD, 'components/ui', file));
const { Button } = load('Button.js');
const { TextField } = load('TextField.js');
const { SelectField } = load('SelectField.js');
const { Feedback } = load('Feedback.js');
const render = (component, props, children) => renderToStaticMarkup(React.createElement(component, props, children));

test('pending blocks clicks and preserves explicit submit type', () => {
  const html = render(Button, { type: 'submit', pending: true, disabled: false }, 'Lưu');
  assert.match(html, /type="submit"/);
  assert.match(html, /disabled=""/);
  assert.match(html, /aria-busy="true"/);
  assert.match(html, /Lưu/);
});
test('ordinary buttons do not submit accidentally', () => {
  assert.match(render(Button, {}, 'Đóng'), /type="button"/);
  assert.match(render(Button, { disabled: true, pending: false }, 'Đóng'), /disabled=""/);
});
test('field links visible label, caller hint and error', () => {
  const html = render(TextField, { id: 'field', label: 'Nhãn', hint: 'Gợi ý', error: 'Lỗi', 'aria-describedby': 'outside' });
  assert.match(html, /for="field"/);
  assert.match(html, /id="field"/);
  assert.match(html, /aria-invalid="true"/);
  const description = html.match(/aria-describedby="([^"]+)"/)[1].split(' ');
  assert.ok(description.includes('outside'));
  for (const id of description.filter((id) => id !== 'outside')) assert.ok(html.includes(`id="${id}"`));
});
test('password input keeps spaces and Unicode exactly and does not truncate', () => {
  const raw = '  Á thử chuỗi  ';
  const html = render(TextField, { label: 'Mật khẩu', type: 'password', defaultValue: raw });
  assert.ok(html.includes(`value="${raw}"`));
  assert.ok(!html.toLowerCase().includes('maxlength'));
});
test('select uses native options and linked errors', () => {
  const html = render(SelectField, { id: 'select', label: 'Chọn', error: 'Lỗi' }, React.createElement('option', { value: 'one' }, 'Một'));
  assert.match(html, /for="select"/);
  assert.match(html, /<select/);
  assert.match(html, /aria-invalid="true"/);
  assert.match(html, /<option value="one">Một<\/option>/);
});
test('feedback escapes content and exposes distinct persistent states', () => {
  const html = render(Feedback, { kind: 'error', message: '<script>bad</script>', traceId: 'safe-trace' });
  assert.match(html, /role="alert"/);
  assert.ok(!html.includes('<script>'));
  assert.match(html, /safe-trace/);
  assert.notEqual(render(Feedback, { kind: 'empty', message: 'Mẫu' }), render(Feedback, { kind: 'no-matches', message: 'Mẫu' }));
  assert.match(render(Feedback, { kind: 'loading', message: 'Tải' }), /role="status"/);
});
