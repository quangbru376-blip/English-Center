import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createRequire } from 'node:module';
import { resolve } from 'node:path';
const require=createRequire(import.meta.url);
const {createSessionSignals}=require(resolve(process.env.EC_TEST_BUILD,'features/identity/sessionSignals.js'));
const {AuthState}=require(resolve(process.env.EC_TEST_BUILD,'lib/auth.js'));
test('signal is literal only; malformed ignored; receiving never echoes; close idempotent',()=>{
 let receive,changed=0,closed=0,removed=0;const sent=[];
 const channel={postMessage:x=>sent.push(x),close:()=>closed++,addEventListener:(_,fn)=>receive=fn,removeEventListener:(_,fn)=>{assert.equal(fn,receive);removed++;}};
 const port=createSessionSignals(()=>changed++,()=>channel);port.send();assert.deepEqual(sent,['IDENTITY_CHANGED']);
 for(const data of [null,{},['IDENTITY_CHANGED'],'OTHER'])receive({data});assert.equal(changed,0);
 receive({data:'IDENTITY_CHANGED'});assert.equal(changed,1);assert.equal(sent.length,1);
 port.close();port.close();port.send();receive({data:'IDENTITY_CHANGED'});assert.equal(changed,1);assert.equal(closed,1);assert.equal(removed,1);
});
test('unsupported/throwing signal transport permits lifecycle without storage fallback',()=>{
 const absent=createSessionSignals(()=>assert.fail('unexpected'),()=>{throw new Error('unsupported');});absent.send();absent.close();
 const failing=createSessionSignals(()=>{},()=>({postMessage:()=>{throw new Error('transport');},close:()=>{},addEventListener:()=>{},removeEventListener:()=>{}}));assert.doesNotThrow(()=>failing.send());failing.close();
});
test('throwing auth observer cannot stop later cleanup/view; unsubscribe works',()=>{
 const auth=new AuthState();let notified=0,cleared=0;auth.requests.registerCleanup(()=>cleared++);
 auth.subscribe(()=>{throw new Error('view');});const off=auth.subscribe(()=>notified++);
 auth.clear();assert.equal(notified,1);assert.equal(cleared,1);off();auth.clear();assert.equal(notified,1);assert.equal(cleared,2);
});
