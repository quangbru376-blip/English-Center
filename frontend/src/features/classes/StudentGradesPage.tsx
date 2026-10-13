import { AccountActions } from '../../app/AccountActions';
import {useCallback,useEffect,useState} from 'react';
import type {SessionController} from '../identity/sessionController';
import type {Page} from '../../lib/m1Wire';
import {useOwnedTask} from '../../lib/useOwnedTask';
import {Button} from '../../components/ui/Button';
import {Badge, Card, EmptyState} from '../../components/ui/Surfaces';
import {PageHeader} from '../../components/ui/PageHeader';
import {Feedback} from '../../components/ui/Feedback';
import {StudentGradeResults} from './components/StudentGradeResults';
import {studentGradeList,studentGradeDetail,studentGradeQuery,publishedInstant} from './studentGradeApi';
import type {StudentGradeSummary,StudentGradeDetail,StudentGradeFilters} from './studentGradeApi';
const emptyFilters:StudentGradeFilters={courseId:'',classId:'',fromDate:'',toDate:'',sort:'FINALIZED_DESC'};
export function StudentGradesPage({controller,onPassword,initialId}:{controller:SessionController;onPassword:()=>void;initialId?:number}){
  const c=controller.client,[filters,setFilters]=useState(emptyFilters),[list,setList]=useState<Page<StudentGradeSummary>|null>(null),[detail,setDetail]=useState<StudentGradeDetail|null>(null);
  const clear=useCallback(()=>{setList(null);setDetail(null);setFilters(emptyFilters);},[]),task=useOwnedTask(c,clear);
  const run=task.run;
  function search(p=1){let q:Record<string,string>;try{q=studentGradeQuery(filters,p);}catch{task.setNotice('Kiểm tra mã khóa học, mã lớp và khoảng ngày.');return;}setDetail(null);setList(null);void task.run(s=>studentGradeList(c,q,s),setList);}
  const read=useCallback((id:number)=>{setDetail(null);void run(s=>studentGradeDetail(c,id,s),setDetail);},[c,run]);
  useEffect(()=>{const timer=window.setTimeout(()=>{if(initialId)void run(s=>studentGradeDetail(c,initialId,s),setDetail);else void run(s=>studentGradeList(c,studentGradeQuery(emptyFilters,1),s),setList);},0);return()=>window.clearTimeout(timer);},[initialId,c,run]);
  if(c.auth.context?.role!=='Student'||c.auth.context.scope!=='NORMAL')return null;
  function field(k:keyof StudentGradeFilters,v:string){setFilters(f=>({...f,[k]:v}));setList(null);setDetail(null);task.setNotice(null);}
  return <section className="student-page"><div><PageHeader title="Kết quả của tôi"/><p className="text-muted">Chỉ hiển thị kết quả đã được công bố.</p></div><AccountActions><Button onClick={onPassword}>Đổi mật khẩu</Button></AccountActions><AccountActions><Button disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></AccountActions>
    {task.pending&&<Feedback kind="loading" message="Đang đọc kết quả…"/>}{task.notice&&<Feedback kind="error" message={task.notice}/>}
    <div className="student-filter">{(['courseId','classId','fromDate','toDate'] as const).map(k=><label key={k}>{({courseId:'Mã khóa học',classId:'Mã lớp',fromDate:'Công bố từ ngày',toDate:'Công bố đến ngày'})[k]}<input aria-label={({courseId:'Mã khóa học',classId:'Mã lớp',fromDate:'Công bố từ ngày',toDate:'Công bố đến ngày'})[k]} className="ec-input" type={k.endsWith('Date')?'date':'text'} inputMode={k.endsWith('Id')?'numeric':undefined} disabled={task.pending} value={filters[k]} onChange={e=>field(k,e.target.value)}/></label>)}<label>Thứ tự công bố<select aria-label="Thứ tự công bố" className="ec-input" disabled={task.pending} value={filters.sort} onChange={e=>field('sort',e.target.value)}><option value="FINALIZED_DESC">Mới nhất trước</option><option value="FINALIZED_ASC">Cũ nhất trước</option></select></label></div>
    <Button disabled={task.pending} onClick={()=>search()}>Đọc kết quả công bố</Button>
    {list&&<section aria-label="Kết quả đã công bố"><div className="student-list-heading"><h2>Kết quả học tập của bạn</h2><p className="text-muted text-sm">Trang {list.page} · {list.totalCount} kết quả</p></div>{!list.items.length&&<EmptyState title="Chưa có kết quả được công bố">Chưa có kết quả được công bố hoặc không có kết quả phù hợp bộ lọc.</EmptyState>}<div className="student-lines">{list.items.map(result=><Card className="student-line" key={result.id}><div><span className="student-eyebrow">Công bố {publishedInstant(result.finalizedAt)}</span><h3>{result.class.code} · {result.class.program.name}</h3></div><div><p className="student-invoice-amount">{result.finalScore}<span className="text-sm text-muted"> /100</span></p><Badge tone={result.passed?'success':'warning'}>Kết quả: {result.passed?'Đạt':'Chưa đạt'}</Badge></div><Button variant="secondary" disabled={task.pending} onClick={()=>read(result.id)}>Xem kết quả {result.id}</Button></Card>)}</div><div className="student-pagination"><Button variant="secondary" disabled={task.pending||list.page===1} onClick={()=>search(list.page-1)}>Trang trước</Button><span>Trang {list.page} / {Math.max(1,Math.ceil(list.totalCount/list.pageSize))}</span><Button variant="secondary" disabled={task.pending||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1)}>Trang sau</Button></div></section>}
    {detail&&<StudentGradeResults classCode={detail.class.code} programName={detail.class.program.name} finalizedLabel={publishedInstant(detail.finalizedAt)} finalScore={detail.finalScore} passed={detail.passed} teacherComment={detail.teacherComment} components={detail.components.map(x=>({...x,scheduleLabel:x.schedule?`${x.schedule.date.split('-').reverse().join('/')} · ${x.schedule.startTime}–${x.schedule.endTime} · ${x.schedule.room}`:null}))}/>}
  </section>;
}
