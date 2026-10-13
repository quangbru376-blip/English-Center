import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
const require=createRequire(import.meta.url);
const {ReportMetrics}=require(resolve(process.env.EC_TEST_BUILD,'features/reports/components/ReportMetrics.js'));
test('Workbench metrics preserve exact formatted38digit totals and distinct no-data strings',()=>{
  const value='99.999.999.999.999.999.999.999.999.999.999.999.999 ₫';
  const html=renderToStaticMarkup(React.createElement(ReportMetrics,{items:[{label:'Doanh thu',value},{label:'Tỷ lệ đạt',value:'Chưa có dữ liệu'}]}));
  assert.ok(html.includes(value));assert.match(html,/Chưa có dữ liệu/);assert.doesNotMatch(html,/NaN|Infinity|0%/);assert.match(html,/Tổng quan toàn bộ tập lọc/);
});
test('Metric labels and descriptions remain plain escaped text, not markup or interactive data',()=>{
  const html=renderToStaticMarkup(React.createElement(ReportMetrics,{items:[{label:'<script>label</script>',value:'50.00%',note:'A & B'}]}));
  assert.match(html,/&lt;script&gt;label&lt;\/script&gt;/);assert.match(html,/A &amp; B/);assert.doesNotMatch(html,/<script|<input|<canvas|<svg/);
});
