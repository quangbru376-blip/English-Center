import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
const require = createRequire(import.meta.url);
const { StudentActionReasons } = require(resolve(process.env.EC_TEST_BUILD, 'features/enrollment/components/StudentActionReasons.js'));
const actions = [{ action: 'BOOK_ENTRY_TEST', label: 'Chọn lịch kiểm tra đầu vào' }, { action: 'CANCEL_REGISTRATION', label: 'Hủy hồ sơ' }, { action: 'CONFIRM_REGISTRATION', label: 'Xác nhận kết quả' }];
const render = props => renderToStaticMarkup(React.createElement(StudentActionReasons, { actions, ...props }));
test('fresh registration shows only the visible unavailable confirmation reason', () => {
  const message = 'Trạng thái hồ sơ, kết quả hoặc thời điểm hiện tại chưa cho phép.';
  const html = render({ allowedActions: ['BOOK_ENTRY_TEST', 'CANCEL_REGISTRATION'], reasons: Object.fromEntries([...actions.map(x => x.action), 'CANCEL_ENTRY_BOOKING', 'RESCHEDULE_ENTRY_BOOKING', 'OTHER'].map(x => [x, message])) });
  assert.equal((html.match(/<li/g) || []).length, 1);
  assert.ok(html.includes('Xác nhận kết quả:'));
  assert.equal(html.split(message).length - 1, 1);
  assert.ok(!html.includes('Hủy hồ sơ:') && !html.includes('Chọn lịch kiểm tra đầu vào:'));
});
test('same server reason groups visible actions once, distinct reasons remain escaped', () => {
  const html = render({ allowedActions: [], reasons: { BOOK_ENTRY_TEST: 'A & B', CANCEL_REGISTRATION: 'A & B', CONFIRM_REGISTRATION: '<script>Không khả dụng</script>' } });
  assert.equal((html.match(/<li/g) || []).length, 2);
  assert.ok(html.includes('Chọn lịch kiểm tra đầu vào, Hủy hồ sơ:'));
  assert.ok(html.includes('&lt;script&gt;') && !html.includes('<script>'));
});
test('no unavailable visible reasons produces no empty list', () => {
  assert.equal(render({ allowedActions: actions.map(x => x.action), reasons: { CONFIRM_REGISTRATION: 'Old reason' } }), '');
  assert.equal(render({ allowedActions: [], reasons: {} }), '');
});
