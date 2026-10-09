import { useCallback, useEffect, useRef, useState } from 'react';
import type { HttpClient,HttpResult } from './http';
// A page task belongs to the current auth epoch. Logout and unmount abort it;
// a late response never repopulates cleared PII or a credential dialog.
export function useOwnedTask(client:HttpClient,clear:()=>void){
  const [pending,setPending]=useState(false),[notice,setNotice]=useState<string|null>(null);
  const owner=useRef({epoch:0,busy:false,abort:null as AbortController|null});const cleanup=useRef(clear);
  useEffect(()=>{cleanup.current=clear;},[clear]);
  useEffect(()=>{
    const state=owner.current;
    const cancel=()=>{state.epoch++;state.abort?.abort();state.busy=false;setPending(false);setNotice(null);cleanup.current();};
    const off=client.auth.requests.registerCleanup(cancel);
    return()=>{state.epoch++;state.abort?.abort();off();};
  },[client]);
  const run=useCallback(async<T,>(work:(signal:AbortSignal)=>Promise<HttpResult<T>>,accept:(value:T)=>void,unknown?:()=>void,rejected?:()=>void)=>{
    const state=owner.current;if(state.busy)return;state.busy=true;setPending(true);setNotice(null);
    const epoch=state.epoch;state.abort=new AbortController();const result=await work(state.abort.signal);
    if(epoch!==state.epoch)return;state.busy=false;setPending(false);
    if(result.kind==='success')accept(result.value);
    else if(result.kind==='unknown'||result.kind==='error'&&result.outcome==='unknown'){unknown?.();setNotice('Chưa xác định kết quả. Kiểm tra kết quả hoặc tải lại; không tự gửi lại.');}
    else {rejected?.();setNotice(result.kind==='error'?result.error.message:'Chưa đọc được dữ liệu. Hãy kiểm tra ngữ cảnh rồi thử lại.');}
  },[]);
  return {pending,notice,setNotice,run};
}
