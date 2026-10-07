import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require = createRequire(import.meta.url);
const {AuthState,decodeAuthContext}=require(resolve(process.env.EC_TEST_BUILD,'lib/auth.js'));
const context=()=>({accountId:1,fullName:'Tên mẫu',email:'sample@example.test',role:'Student',scope:'NORMAL',accountVersion:'AAAAAAAAAAE=',serverTime:'2026-10-07T01:00:00Z',absoluteExpiresAt:'2026-10-07T03:00:00Z',effectiveExpiresAt:'2026-10-07T01:20:00Z'});
test('auth exact nine fields, role, RV and real UTC instants reject malformed data',()=>{
 for(const patch of [{extra:1},{accountId:0},{role:'Owner'},{scope:'ADMIN'},{accountVersion:'AAAAAAAAAAF='},{serverTime:'2026-02-30T01:00:00Z'},{serverTime:'2026-10-07T01:00:00+00:00'},{effectiveExpiresAt:'2026-10-07T04:00:00Z'}]) assert.throws(()=>decodeAuthContext({...context(),...patch}));
 assert.ok(Object.isFrozen(decodeAuthContext(context())));
});
test('same-account explicit new session invalidates tickets and RAM before publication',()=>{
 const state=new AuthState(); state.startSession(context());state.acceptCsrf({requestToken:'opaque'});
 const ticket=state.requests.begin('read');let cleared=0;
 state.requests.registerCleanup(()=>{assert.equal(state.context,null);assert.equal(state.csrf,null);cleared++;});
 state.startSession(context()); assert.equal(ticket.isCurrent(),false);assert.equal(cleared,1);assert.equal(state.csrf,null);
});
test('me does not renew or clear RAM when authority identity unchanged; changes invalidate',()=>{
 const state=new AuthState();state.startSession(context());let cleared=0;state.requests.registerCleanup(()=>cleared++);
 state.acceptCsrf({requestToken:'opaque'});state.refreshContext({...context(),serverTime:'2026-10-07T01:01:00Z'});
 assert.equal(cleared,0);assert.equal(state.csrf,'opaque');assert.equal(state.context.effectiveExpiresAt,context().effectiveExpiresAt);
 state.refreshContext({...context(),scope:'PASSWORD_CHANGE'});assert.equal(cleared,1);assert.equal(state.canRequestBusiness(),false);
 state.clear();assert.equal(state.context,null);assert.equal(state.csrf,null);
});
test('token shape rejects extras/empty/header injection, recovery clears RAM with throwing callback',()=>{
 const state=new AuthState();for(const value of [{requestToken:''},{requestToken:'a\r\nb'}, {requestToken:'opaque',extra:1}])assert.throws(()=>state.acceptCsrf(value));
 state.acceptCsrf({requestToken:'opaque'});let cleared=false;
 state.requests.registerCleanup(()=>{throw new Error('synthetic');});state.requests.registerCleanup(()=>{cleared=true;});
 state.requireCsrfRecovery();assert.equal(state.csrf,null);assert.equal(cleared,true);
});
