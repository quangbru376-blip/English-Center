import { GraduationCap, UserRound } from 'lucide-react';
import { Badge, Card, EmptyState } from '../../../components/ui/Surfaces';
import { Button } from '../../../components/ui/Button';
import type { Page } from '../../../lib/m1Wire';
import type { ClassDetail } from '../classApi';

export function StudentClassCards({ data, busy, states, read, changePage }: { data: Page<ClassDetail>; busy: boolean; states: Readonly<Record<string,string>>; read: (id: number) => void; changePage: (page: number) => void }) {
  return <section aria-label="Các lớp của tôi"><div className="student-list-heading"><h2>Lớp học của bạn</h2><span className="text-muted text-sm">{data.totalCount} lớp · Trang {data.page}</span></div>
    {!data.items.length ? <EmptyState title="Chưa có lớp phù hợp">Lớp được hiển thị sau khi hồ sơ được xếp lớp. Bạn có thể chọn lịch sử hoặc đổi bộ lọc.</EmptyState> : <ul className="student-grid">{data.items.map(item=><li key={item.id}><Card className="student-registration-card"><div className="student-card-heading"><span className="student-course-icon"><GraduationCap className="ec-icon" aria-hidden="true"/></span><Badge tone={item.status==='COMPLETED'?'success':item.status==='ONGOING'?'info':'neutral'}>{states[item.status]}</Badge></div><h3>{item.code}</h3><p>{item.program.name}</p><dl className="student-facts"><div><dt><UserRound className="ec-icon" aria-hidden="true"/>Giáo viên phụ trách</dt><dd>{item.teacher.displayName}</dd></div></dl><div><Button variant="secondary" disabled={busy} onClick={()=>read(item.id)}>Xem lớp {item.id}</Button></div></Card></li>)}</ul>}
    <div className="student-pagination"><Button variant="secondary" disabled={busy||data.page<=1} onClick={()=>changePage(data.page-1)}>Trang trước</Button><span>Trang {data.page} / {Math.max(1,Math.ceil(data.totalCount/data.pageSize))}</span><Button variant="secondary" disabled={busy||data.page*data.pageSize>=data.totalCount} onClick={()=>changePage(data.page+1)}>Trang sau</Button></div>
  </section>;
}
