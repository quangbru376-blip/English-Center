import {lazy,Suspense} from 'react';
import type {SessionController} from '../identity/sessionController';
import {Feedback} from '../../components/ui/Feedback';
const Reports=lazy(()=>import('./ReportsPage').then(m=>({default:m.ReportsPage})));
const Audit=lazy(()=>import('../audit/AuditPage').then(m=>({default:m.AuditPage})));
export function ReportsPanel(p:{controller:SessionController;onPassword:()=>void}){return <Suspense fallback={<Feedback kind="loading" message="Đang mở báo cáo…"/>}><Reports {...p}/></Suspense>;}
export function AuditPanel(p:{controller:SessionController;onPassword:()=>void;initialId?:number;initialOperation?:string}){return <Suspense fallback={<Feedback kind="loading" message="Đang mở lịch sử…"/>}><Audit {...p}/></Suspense>;}
