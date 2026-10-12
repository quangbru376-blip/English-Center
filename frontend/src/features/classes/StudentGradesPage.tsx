import {useCallback,useEffect,useState} from 'react';
import type {SessionController} from '../identity/sessionController';
import type {Page} from '../../lib/m1Wire';
import {useOwnedTask} from '../../lib/useOwnedTask';
import {Button} from '../../components/ui/Button';
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
  useEffect(()=>{if(initialId)void run(s=>studentGradeDetail(c,initialId,s),setDetail);},[initialId,c,run]);
  if(c.auth.context?.role!=='Student'||c.auth.context.scope!=='NORMAL')return null;
  function field(k:keyof StudentGradeFilters,v:string){setFilters(f=>({...f,[k]:v}));setList(null);setDetail(null);task.setNotice(null);}
  return <section className="min-w-0 break-words"><h1 className="text-xl font-semibold">Kết quả của tôi</h1><p>Chỉ hiển thị kết quả đã được công bố.</p><Button onClick={onPassword}>Đổi mật khẩu</Button><Button disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button>
    {task.pending&&<Feedback kind="loading" message="Đang đọc kết quả…"/>}{task.notice&&<Feedback kind="error" message={task.notice}/>}
    <div className="my-3 grid gap-3 sm:grid-cols-2">{(['courseId','classId','fromDate','toDate'] as const).map(k=><label key={k}>{({courseId:'Mã khóa học',classId:'Mã lớp',fromDate:'Công bố từ ngày',toDate:'Công bố đến ngày'})[k]}<input aria-label={({courseId:'Mã khóa học',classId:'Mã lớp',fromDate:'Công bố từ ngày',toDate:'Công bố đến ngày'})[k]} className="block min-h-11 w-full border p-2" type={k.endsWith('Date')?'date':'text'} inputMode={k.endsWith('Id')?'numeric':undefined} disabled={task.pending} value={filters[k]} onChange={e=>field(k,e.target.value)}/></label>)}<label>Thứ tự công bố<select aria-label="Thứ tự công bố" className="block min-h-11 w-full border p-2" disabled={task.pending} value={filters.sort} onChange={e=>field('sort',e.target.value)}><option value="FINALIZED_DESC">Mới nhất trước</option><option value="FINALIZED_ASC">Cũ nhất trước</option></select></label></div>
    <Button disabled={task.pending} onClick={()=>search()}>Đọc kết quả công bố</Button>
    {list&&<div>{!list.items.length&&<Feedback kind="empty" message="Chưa có kết quả được công bố hoặc không có kết quả phù hợp bộ lọc."/>}{list.items.map(x=><article className="my-3 border p-3" key={x.id}><h2>{x.class.code} · {x.class.program.name}</h2><p>{publishedInstant(x.finalizedAt)} · Điểm {x.finalScore} · {x.passed?'Đạt':'Chưa đạt'}</p><Button disabled={task.pending} onClick={()=>read(x.id)}>Xem kết quả {x.id}</Button></article>)}<p>Trang {list.page} · {list.totalCount} kết quả</p><Button disabled={task.pending||list.page===1} onClick={()=>search(list.page-1)}>Trang trước</Button><Button disabled={task.pending||list.page*list.pageSize>=list.totalCount} onClick={()=>search(list.page+1)}>Trang sau</Button></div>}
    {detail&&<StudentGradeResults classCode={detail.class.code} programName={detail.class.program.name} finalizedLabel={publishedInstant(detail.finalizedAt)} finalScore={detail.finalScore} passed={detail.passed} teacherComment={detail.teacherComment} components={detail.components.map(x=>({...x,scheduleLabel:x.schedule?`${x.schedule.date.split('-').reverse().join('/')} · ${x.schedule.startTime}–${x.schedule.endTime} · ${x.schedule.room}`:null}))}/>}
  </section>;
}
