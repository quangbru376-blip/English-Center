import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

const require = createRequire(import.meta.url);
const load = p => require(resolve(process.env.EC_TEST_BUILD, p));
const { StudentProfileForm } = load('features/enrollment/components/StudentProfileForm.js');
const { EntryScoreForm } = load('features/entryTasks/components/EntryScoreForm.js');
const render = (component, props) => renderToStaticMarkup(React.createElement(component, props));
const profile = { fullName: 'Fixture', dateOfBirth: '2000-01-01', phone: '0912345678', address: '' };
const noop = () => {};

test('own profile uses four labeled fields and never emits immutable identity inputs', () => {
  const html = render(StudentProfileForm, { value: profile, disabled: false, onChange: noop, onSubmit: noop, onCancel: noop });
  assert.equal((html.match(/<input /g) || []).length, 4);
  for (const label of ['Họ và tên', 'Ngày sinh', 'Số điện thoại', 'Địa chỉ']) assert.ok(html.includes(label));
  assert.match(html, /type="date"/); assert.match(html, /type="tel"/);
  assert.doesNotMatch(html, /name="(?:email|accountId|studentId|password)"/);
  assert.match(html, /maxLength="200"/); assert.match(html, /maxLength="510"/);
});

test('disabled profile blocks submit callbacks and disables fields and both actions', () => {
  let submits = 0, prevented = 0;
  const props = { value: profile, disabled: true, onChange: noop, onSubmit: () => submits++, onCancel: noop };
  StudentProfileForm(props).props.onSubmit({ preventDefault: () => prevented++ });
  assert.equal(submits, 0); assert.equal(prevented, 1);
  assert.equal((render(StudentProfileForm, props).match(/disabled=""/g) || []).length, 6);
});

test('profile submits once explicitly without rewriting the draft', () => {
  let submits = 0, changes = 0;
  StudentProfileForm({ value: profile, disabled: false, onChange: () => changes++, onSubmit: () => submits++, onCancel: noop }).props.onSubmit({ preventDefault: noop });
  assert.equal(submits, 1); assert.equal(changes, 0);
});

test('absence form never includes a score or reason input and explicitly distinguishes zero', () => {
  const html = render(EntryScoreForm, { mode: 'MARK_ABSENT', value: { score: '', reason: '' }, disabled: false, onChange: noop, onSubmit: noop, onCancel: noop });
  assert.doesNotMatch(html, /<input /); assert.ok(html.includes('Vắng không phải điểm 0.'));
  assert.ok(html.includes('Xác nhận vắng')); assert.match(html, /type="button"/);
});

test('grade keeps empty and precise decimal drafts as text without fabricating zero', () => {
  for (const score of ['', '80.50', '100.001']) {
    const html = render(EntryScoreForm, { mode: 'GRADE', value: { score, reason: '' }, disabled: false, onChange: noop, onSubmit: noop, onCancel: noop });
    assert.match(html, /type="text"/); assert.match(html, /inputMode="decimal"/);
    assert.ok(html.includes(`value="${score}"`)); assert.equal((html.match(/<input /g) || []).length, 1);
  }
});

test('correction requires reason and disabled score form blocks submission', () => {
  let submits = 0;
  const props = { mode: 'CORRECT_SCORE', value: { score: '0.00', reason: '' }, disabled: true, onChange: noop, onSubmit: () => submits++, onCancel: noop };
  const html = render(EntryScoreForm, props);
  assert.ok(html.includes('Lý do sửa điểm')); assert.match(html, /maxLength="2000"/);
  assert.equal((html.match(/required=""/g) || []).length, 2);
  assert.equal((html.match(/disabled=""/g) || []).length, 4);
  EntryScoreForm(props).props.onSubmit({ preventDefault: noop }); assert.equal(submits, 0);
});
