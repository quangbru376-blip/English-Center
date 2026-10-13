import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
const require = createRequire(import.meta.url);
const { StudentHome, StudentCourses } = require(resolve(process.env.EC_TEST_BUILD, 'features/enrollment/components/StudentOverview.js'));
const noop = () => {};
const course = { id: 1, code: 'IELTS', name: 'Tiếng Anh <căn bản>', status: 'ACTIVE' };
const home = { registrationId: 10, course, status: 'TESTING', currentStep: 'Đang kiểm tra đầu vào', waitingReason: null, nearestEvent: null, unpaidInvoice: null, finalizedReport: null, nextAction: null };
const props = items => ({ data: { items, page: 1, pageSize: 20, totalCount: 41 }, busy: false, read: noop, changePage: noop, chooseCourses: noop, openNext: noop, openReport: noop });
const render = (component, props) => renderToStaticMarkup(React.createElement(component, props));

test('Home shows server page count without inventing totals from the loaded subset', () => {
  const html = render(StudentHome, props([home]));
  assert.match(html, /41 hồ sơ/); assert.match(html, /Trang 1 \/ 3/);
  assert.match(html, /Tiếng Anh &lt;căn bản&gt;/);
  assert.doesNotMatch(html, /0 ₫|0\/100|Kết quả đã công bố|Học phí chưa thanh toán|%/);
  assert.doesNotMatch(html, /Xem bước tiếp theo/);
});
test('Home preserves distinct registration facts, exact large money and published-only links', () => {
  const item = { ...home, unpaidInvoice: { number: 'HD-2026-00000001', amount: '999999999999999999', dueDate: '2026-10-13', isOverdue: true }, finalizedReport: { gradeReportId: 42, classId: 7 }, nearestEvent: { kind: 'ENTRY_TEST', startAt: '2026-10-13T03:00:00Z', room: 'A1' } };
  const html = render(StudentHome, props([item]));
  assert.match(html, /999\.999\.999\.999\.999\.999 ₫/);
  assert.match(html, /13\/10\/2026/); assert.match(html, /10:00/);
  assert.match(html, /Quá hạn thanh toán/); assert.match(html, /Xem kết quả 42/);
  assert.doesNotMatch(html, /NaN|DRAFT|SUBMITTED/);
});
test('Course cards and home controls block all callbacks during a pending operation', () => {
  const html = render(StudentCourses, { data: { items: [{ ...course, tuition: '12000000' }], page: 1, pageSize: 20, totalCount: 1 }, busy: true, blocked: true, register: noop, changePage: noop });
  assert.match(html, /12\.000\.000 ₫/); assert.equal((html.match(/disabled=""/g) ?? []).length, 3);
  assert.match(render(StudentHome, { ...props([home]), busy: true }), /disabled=""[^>]*>Xem hồ sơ 10/);
});
test('Empty responses show guidance while retaining truthful zero count and bounded pagination', () => {
  const html = render(StudentHome, { ...props([]), data: { items: [], page: 1, pageSize: 20, totalCount: 0 } });
  assert.match(html, /0 hồ sơ/); assert.match(html, /Bắt đầu với một khóa học/);
  assert.match(html, /Trang 1 \/ 1/); assert.doesNotMatch(html, /student-registration-card/);
});
