import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
import { readFileSync } from 'node:fs';
const require = createRequire(import.meta.url);
const { Textarea, Pagination, Badge, Avatar, Table } = require(resolve(process.env.EC_TEST_BUILD, 'components/ui/Surfaces.js'));
const render = (C, props) => renderToStaticMarkup(React.createElement(C, props));
test('Textarea links label, supplied descriptions, hint and error without losing raw text', () => {
  const html = render(Textarea, { id:'comment', label:'Nhận xét', hint:'Gợi ý', error:'Lỗi', 'aria-describedby':'outside', defaultValue:'  Tiếng Việt\n<plain>  ' });
  assert.match(html,/for="comment"/); assert.match(html,/aria-describedby="outside comment-hint comment-error"/); assert.match(html,/aria-invalid="true"/); assert.match(html,/  Tiếng Việt\n&lt;plain&gt;  /);
});
test('Pagination disables both ends on an empty or one-page projection', () => {
  for (const totalPages of [0,1]) { const html = render(Pagination,{page:1,totalPages,onChange:()=>{}});assert.equal((html.match(/disabled=""/g)||[]).length,2);assert.match(html,/Trang 1 \/ 1/); }
});
test('Badges retain explicit state text and table retains accessible caption', () => {
  assert.match(render(Badge,{tone:'warning',children:'Chưa thanh toán'}),/Chưa thanh toán/);
  assert.match(render(Table,{caption:'Dữ liệu',children:null}),/<caption[^>]*>Dữ liệu<\/caption>/);
  assert.match(render(Avatar,{name:'Nguyễn Minh Anh'}),/aria-hidden="true">MA/);
});
const css=readFileSync(new URL('../src/styles/tokens.css',import.meta.url),'utf8');
const token=n=>css.match(new RegExp('--ec-'+n+':\\s*(#[0-9a-f]{6})','i'))[1];
function luminance(hex){const values=hex.slice(1).match(/../g).map(v=>parseInt(v,16)/255).map(v=>v<=0.04045?v/12.92:((v+0.055)/1.055)**2.4);return values[0]*0.2126+values[1]*0.7152+values[2]*0.0722;}
function contrast(a,b){const x=luminance(a),y=luminance(b);return (Math.max(x,y)+0.05)/(Math.min(x,y)+0.05);}
test('Brand, body, muted and all semantic text/background pairs reach 4.5 contrast',()=>{
  const pairs=[['primary','surface'],['primary-hover','surface'],['text','background'],['muted','background'],['text','surface'],['muted','surface'],['brand-dark','accent'],['on-primary','brand-dark'],['accent','brand-dark'],['danger','danger-soft'],['success','success-soft'],['warning','warning-soft'],['info','info-soft'],['brand-dark','primary-soft']];
  for(const [a,b] of pairs)assert.ok(contrast(token(a),token(b))>=4.5,`${a}/${b}`);
});
