import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url);
const {RequestScope}=require(resolve(process.env.EC_TEST_BUILD,'lib/requestScope.js'));
test('superseded requests cannot publish, old finish cannot remove newer request',()=>{
 const scope=new RequestScope();const old=scope.begin('slot');const newer=scope.begin('slot');
 assert.equal(old.signal.aborted,true);assert.equal(old.isCurrent(),false);old.finish();old.finish();assert.equal(newer.isCurrent(),true);
 newer.finish();assert.equal(newer.isCurrent(),false);assert.throws(()=>scope.begin(''));
});
test('epoch invalidation defeats ignored abort and runs every snapshot cleanup',()=>{
 const scope=new RequestScope();const a=scope.begin('a');const b=scope.begin('b');let calls=0;
 scope.registerCleanup(()=>{throw new Error('synthetic');});const unregister=scope.registerCleanup(()=>calls++);
 scope.invalidate();assert.equal(a.isCurrent(),false);assert.equal(b.signal.aborted,true);assert.equal(calls,1);
 unregister();unregister();scope.invalidate();assert.equal(calls,1);assert.equal(scope.begin('a').isCurrent(),true);
});
