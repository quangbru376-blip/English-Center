import {lazy,Suspense} from 'react';
import type {ComponentProps} from 'react';
import type {GradebookPanel as Book} from './GradebookPanel';
import type {GradeReviewPage as Review} from './GradeReviewPage';
import type {AttendancePanel as Attendance} from './AttendancePanel';
import {Feedback} from '../../components/ui/Feedback';
const LazyBook=lazy(()=>import('./GradebookPanel').then(module=>({default:module.GradebookPanel})));
const LazyReview=lazy(()=>import('./GradeReviewPage').then(module=>({default:module.GradeReviewPage})));
const LazyAttendance=lazy(()=>import('./AttendancePanel').then(module=>({default:module.AttendancePanel})));
export function GradebookPanel(props:ComponentProps<typeof Book>){return <Suspense fallback={<Feedback kind="loading" message="Đang mở sổ điểm…"/>}><LazyBook {...props}/></Suspense>;}
export function GradeReviewPage(props:ComponentProps<typeof Review>){return <Suspense fallback={<Feedback kind="loading" message="Đang mở bảng điểm…"/>}><LazyReview {...props}/></Suspense>;}
export function AttendancePanel(props:ComponentProps<typeof Attendance>){return <Suspense fallback={<Feedback kind="loading" message="Đang mở điểm danh…"/>}><LazyAttendance {...props}/></Suspense>;}

const LazyStudent=lazy(()=>import('./StudentGradesPage').then(module=>({default:module.StudentGradesPage})));
export function StudentGradesPage(props:ComponentProps<typeof import('./StudentGradesPage').StudentGradesPage>){return <Suspense fallback={<Feedback kind="loading" message="Đang mở kết quả…"/>}><LazyStudent {...props}/></Suspense>;}
