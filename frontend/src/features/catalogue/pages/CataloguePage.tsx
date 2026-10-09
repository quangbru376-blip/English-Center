import { useCallback,useState } from 'react';
import { Button } from '../../../components/ui/Button';
import { TextField } from '../../../components/ui/TextField';
import { Modal } from '../../../components/ui/Modal';
import { Feedback } from '../../../components/ui/Feedback';
import { PageHeader } from '../../../components/ui/PageHeader';
import { useOwnedTask } from '../../../lib/useOwnedTask';
import type { Page } from '../../../lib/m1Wire';
import type { SessionController } from '../../identity/sessionController';
import { CourseForm } from '../components/CourseForm';
import { courseActions,courseCommand,courseLookup,loadBands,loadCourse,searchCourses } from '../catalogueApi';
import type { Course,Bands } from '../catalogueApi';
type Action=keyof typeof courseActions;
export function CataloguePage({controller,onPassword}:{controller:SessionController;onPassword:()=>void}){
  const client=controller.client;
  const [code,setCode]=useState(''),[name,setName]=useState(''),[status,setStatus]=useState(''),[sort,setSort]=useState('CODE_ASC');
  const [data,setData]=useState<Page<Course>|null>(null),[course,setCourse]=useState<Course|null>(null),[bands,setBands]=useState<Bands|null>(null);
  const [form,setForm]=useState<'create'|'edit'|null>(null),[preview,setPreview]=useState<Action|null>(null);
  const [unknown,setUnknown]=useState<{requestId:string;action:string;target:number|null}|null>(null);
  const clear=useCallback(()=>{setCode('');setName('');setStatus('');setSort('CODE_ASC');setData(null);setCourse(null);setBands(null);setForm(null);setPreview(null);setUnknown(null);},[]);
  const task=useOwnedTask(client,clear);
  const search=(page=1)=>void task.run(signal=>searchCourses(client,{page:String(page),sort,...code?{code}:{},...name?{name}:{},...status?{status}:{}},signal),setData);
  const read=(id:number)=>void task.run(signal=>loadCourse(client,id,signal),setCourse);
  function save(values:{code:string;name:string;basePrice:string;status:'ACTIVE'|'INACTIVE'}){
    if(unknown||!form||form==='edit'&&!course)return;const editing=form==='edit',requestId=crypto.randomUUID(),target=editing?course:null;
    const body=editing?{name:values.name,basePrice:values.basePrice,expectedVersions:{course:target!.version}}:values;
    void task.run(signal=>courseCommand(client,editing?'courseUpdate':'courseCreate',body,requestId,signal,target?.id),result=>{setCourse(result.course);setData(null);setForm(null);},()=>{setForm(null);setCourse(null);setUnknown({requestId,action:editing?'ACADEMIC.COURSE.UPDATE_DETAILS':'ACADEMIC.COURSE.CREATE',target:target?.id??null});},()=>{setForm(null);setCourse(null);setData(null);});
  }
  function confirm(){
    if(!course||!preview||unknown)return;const target=course,action=courseActions[preview],requestId=crypto.randomUUID();setPreview(null);
    void task.run(signal=>courseCommand(client,action.endpoint,{expectedVersions:{course:target.version}},requestId,signal,target.id),result=>{setCourse(result.course);setData(null);},()=>{setCourse(null);setUnknown({requestId,action:action.action,target:target.id});},()=>{setCourse(null);setData(null);});
  }
  if(client.auth.context?.role!=='AcademicAffairs'||client.auth.context.scope!=='NORMAL')return <Feedback kind="error" message="Không có quyền quản lý danh mục."/>;
  return <section><PageHeader title="Khóa học và Band"/>
    <div className="mb-4 flex flex-wrap gap-3"><Button disabled={task.pending||Boolean(unknown)} onClick={()=>{setCourse(null);setForm('create');}}>Thêm khóa học</Button><Button variant="secondary" disabled={task.pending} onClick={()=>void task.run(signal=>loadBands(client,signal),setBands)}>Xem bộ Band</Button><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button><Button variant="secondary" disabled={task.pending} onClick={()=>void controller.logout()}>Đăng xuất</Button></div>
    <form className="mb-4 grid gap-3 md:grid-cols-3" onSubmit={e=>{e.preventDefault();search();}}><TextField label="Mã khóa học" value={code} disabled={task.pending} onChange={e=>setCode(e.currentTarget.value)}/><TextField label="Tên khóa học" value={name} disabled={task.pending} onChange={e=>setName(e.currentTarget.value)}/>
      <label>Trạng thái<select className="block min-h-[44px] w-full border" value={status} disabled={task.pending} onChange={e=>setStatus(e.currentTarget.value)}><option value="">Tất cả</option><option value="ACTIVE">Hoạt động</option><option value="INACTIVE">Ngừng sử dụng</option></select></label>
      <label>Sắp xếp<select className="block min-h-[44px] w-full border" value={sort} disabled={task.pending} onChange={e=>setSort(e.currentTarget.value)}><option value="CODE_ASC">Mã tăng dần</option><option value="CODE_DESC">Mã giảm dần</option></select></label><Button type="submit" pending={task.pending}>Tìm khóa học</Button></form>
    {task.notice&&<Feedback kind="error" message={task.notice}/>} {task.pending&&<Feedback kind="loading" message="Đang xử lý…"/>}
    {data&&<><p>{data.totalCount} khóa học — trang {data.page}</p>{data.items.length===0?<Feedback kind="empty" message="Chưa có khóa học phù hợp."/>:<div className="overflow-x-auto"><table className="w-full text-left"><thead><tr><th>Mã</th><th>Tên</th><th className="text-right">Học phí (VND)</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>{data.items.map(c=><tr key={c.id} className="border-b"><td>{c.code}</td><td>{c.name}</td><td className="text-right">{BigInt(c.basePrice).toLocaleString('vi-VN')}</td><td>{c.status==='ACTIVE'?'Hoạt động':'Ngừng sử dụng'}</td><td><Button variant="secondary" disabled={task.pending} onClick={()=>read(c.id)}>Xem {c.code}</Button></td></tr>)}</tbody></table></div>}
      <Button disabled={task.pending||data.page===1} onClick={()=>search(data.page-1)}>Trang trước</Button> <Button disabled={task.pending||data.page*data.pageSize>=data.totalCount} onClick={()=>search(data.page+1)}>Trang sau</Button></>}
    {course&&<article className="mt-4 border p-4"><h2 className="font-semibold break-words">{course.code} — {course.name}</h2><p>{BigInt(course.basePrice).toLocaleString('vi-VN')} VND · {course.status==='ACTIVE'?'Hoạt động':'Ngừng sử dụng'}</p><div className="mt-3 flex flex-wrap gap-3">{(Object.keys(courseActions) as Action[]).map(action=><Button key={action} variant={action==='DEACTIVATE'?'danger':'secondary'} disabled={task.pending||Boolean(unknown)||!course.allowedActions.includes(action)} title={course.disabledReasons[action]} onClick={()=>action==='EDIT_DETAILS'?setForm('edit'):setPreview(action)}>{courseActions[action].label}</Button>)}<Button disabled={task.pending} variant="secondary" onClick={()=>read(course.id)}>Tải lại khóa học</Button></div></article>}
    {bands&&<article className="mt-4 border p-4"><h2 className="font-semibold">Band tham chiếu</h2><p>{bands.isThresholdSetFrozen?'Bộ ngưỡng đã đóng băng sau kết quả thi đầu tiên.':'Chưa có kết quả thi hoàn tất.'} Bộ Band chỉ đọc.</p><ul>{bands.items.map(b=><li key={b.id}>{b.code}: từ {b.minScore} điểm</li>)}</ul></article>}
    {unknown&&<div className="mt-4 flex flex-wrap gap-3"><Button disabled={task.pending} onClick={()=>void task.run(signal=>courseLookup(client,unknown.action,unknown.requestId,signal),result=>{task.setNotice('Completion đã xác nhận. Tải lại khóa học để xem trạng thái hiện tại.');setUnknown(null);read(result.target.courseId);})}>Kiểm tra kết quả</Button>{unknown.target&&<Button disabled={task.pending} onClick={()=>read(unknown.target!)}>Đối chiếu khóa học</Button>}<Button disabled={task.pending} variant="secondary" onClick={()=>{setUnknown(null);setCourse(null);setData(null);task.setNotice('Kết quả cũ chưa xác nhận; tải lại trước khi tạo intent mới.');}}>Kết thúc đối chiếu</Button></div>}
    <Modal open={Boolean(form)} title={form==='edit'?'Sửa tên và học phí':'Tạo khóa học'} onRequestClose={()=>{if(!task.pending)setForm(null);}}><CourseForm key={form==='edit'?course?.version:'create'} editing={form==='edit'} initial={form==='edit'&&course?course:undefined} pending={task.pending} onSubmit={save} onCancel={()=>setForm(null)}/></Modal>
    <Modal open={Boolean(preview)} title="Xác nhận trạng thái khóa học" onRequestClose={()=>setPreview(null)}><p className="break-words">{preview&&courseActions[preview].label}: {course?.code} — {course?.name}?</p><p className="my-3">Ngừng khóa chỉ chặn đăng ký mới. Hồ sơ, hóa đơn, chương trình và lớp hiện có được giữ nguyên.</p><Button variant="danger" onClick={confirm} disabled={task.pending}>Xác nhận</Button></Modal>
  </section>;
}
