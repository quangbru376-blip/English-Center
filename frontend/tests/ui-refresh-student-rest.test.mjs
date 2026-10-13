import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
const require = createRequire(import.meta.url);
const { StudentProfileSummary, StudentBookings } = require(resolve(process.env.EC_TEST_BUILD, 'features/enrollment/components/StudentPersonal.js'));
const { StudentClassCards } = require(resolve(process.env.EC_TEST_BUILD, 'features/classes/components/StudentClassCards.js'));
const noop = () => {};
const render = (component, props) => renderToStaticMarkup(React.createElement(component, props));
test('Profile summary renders escaped existing identity values without introducing editable email or credential fields', () => {
  const html=render(StudentProfileSummary,{profile:{fullName:'An <script>',email:'an@example.test',dateOfBirth:'2000-01-01',phone:'0912345678',address:null},busy:true,editing:false,edit:noop});
  assert.match(html,/An &lt;script&gt;/);assert.match(html,/01\/01\/2000/);assert.match(html,/Chưa có địa chỉ/);
  assert.doesNotMatch(html,/<input|<textarea|password|accountVersion/);assert.match(html,/disabled=""[^>]*>Sửa thông tin cá nhân/);
});
test('Booking agenda distinguishes absence from score and keeps owned Registration link and Vietnam date',()=>{
  const booking={id:2,registrationId:7,attemptNumber:1,status:'ABSENT',session:{startAt:'2026-10-13T03:00:00Z',endAt:'2026-10-13T04:00:00Z',room:'A1'}};
  const html=render(StudentBookings,{data:{items:[booking],page:1,pageSize:20,totalCount:1},busy:true,read:noop,changePage:noop});
  assert.match(html,/Vắng thi/);assert.match(html,/10:00 13\/10\/2026/);assert.match(html,/Xem hồ sơ 7/);
  assert.doesNotMatch(html,/0\/100|Đã chấm điểm|Đã tổ chức/);assert.equal((html.match(/disabled=""/g)||[]).length,3);
});
test('Student class cards display supplied public facts without management counts or peer identities',()=>{
  const item={id:8,code:'CLASS',status:'OPEN',program:{name:'Chương trình'},teacher:{displayName:'Giáo viên',accountId:98765},placedCount:13,maxSize:15,guardContext:{student:'peer-secret'}};
  const html=render(StudentClassCards,{data:{items:[item],page:1,pageSize:20,totalCount:1},states:{OPEN:'Đang nhận học viên'},busy:false,read:noop,changePage:noop});
  assert.match(html,/Giáo viên/);assert.match(html,/Xem lớp 8/);assert.doesNotMatch(html,/98765|13\/15|peer-secret|Sĩ số/);
});
