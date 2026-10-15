import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { Modal } from '../src/components/ui/Modal';
import { Button } from '../src/components/ui/Button';
import { LocalCalendar } from '../src/components/ui/LocalCalendar';
import { ProgramComponents } from '../src/features/programs/components/ProgramComponents';
import type { ComponentInput } from '../src/features/programs/components/ProgramComponents';
import { GradeDraftFields } from '../src/features/classes/components/GradeDraftFields';
import { AttendanceFields } from '../src/features/classes/components/AttendanceFields';
import { WeeklyFields } from '../src/features/classes/components/WeeklyFields';
import '../src/styles/index.css';

// Dev-only synthetic presentation fixture; no client, credentials or API requests.
function SpacingReview() {
  const [open, setOpen] = useState(false);
  const [week, setWeek] = useState(false);
  const [components, setComponents] = useState<ComponentInput[]>([
    { id: 1, name: 'Đánh giá thường xuyên', weight: '40', kind: 'GENERAL' },
    { id: 2, name: 'Thi cuối khóa', weight: '60', kind: 'FINAL_EXAM' },
  ]);
  return <main className="ec-styleguide ec-shell-main"><section className="ec-work-page">
    <h1>Rà khoảng cách thành phần</h1><p>Dữ liệu minh họa cục bộ dành cho kiểm tra bố cục.</p>
    <article className="ec-work-card"><h2>Thành phần chương trình</h2><ProgramComponents value={components} disabled={false} onChange={setComponents}/></article>
    <article className="ec-work-card"><h2>Điểm từng học viên</h2><GradeDraftFields fields={[{ componentId: 1, name: 'Đánh giá thường xuyên', kind: 'GENERAL', value: '75', scheduleId: '', options: [] }, { componentId: 2, name: 'Thi cuối khóa', kind: 'FINAL_EXAM', value: '', scheduleId: '', options: [{ id: 9, label: 'Thi cuối khóa · 15/11/2026', disabled: false }], error: 'Vui lòng kiểm tra điểm và buổi thi áp dụng.' }]} comment="" disabled={false} dirty onScore={()=>{}} onSchedule={()=>{}} onComment={()=>{}} onSave={()=>{}} onDiscard={()=>{}}/></article>
    <article className="ec-work-card"><h2>Điểm danh</h2><AttendanceFields rows={[{ classAssignmentId: 1, fullName: 'Nguyễn Minh Anh', present: null }]} disabled={false} onChange={()=>{}} onSave={()=>{}} onCancel={()=>{}}/></article>
    <article className="ec-work-card"><h2>Lập lịch tuần</h2><WeeklyFields fromDate="2026-11-01" toDate="2026-11-30" startTime="18:00" endTime="19:30" room="P101" weekdays={[1,3,5]} disabled={false} onFromDate={()=>{}} onToDate={()=>{}} onStartTime={()=>{}} onEndTime={()=>{}} onRoom={()=>{}} onWeekdays={()=>{}} onPreview={()=>{}} onCancel={()=>{}}/></article>
    <article className="ec-work-card"><h2>Lịch giảng dạy</h2><Button onClick={()=>setWeek(!week)}>Đổi dạng lịch mẫu</Button><LocalCalendar items={[{ id: 1, date: '2026-11-02', startTime: '18:00', endTime: '19:30', room: 'P101', type: 'LESSON', occurrenceState: 'PLANNED', isCurrent: true, class: { code: 'IELTS-01' } }]} week={week} fromDate="2026-11-02" disabled={false} onSelect={()=>{}}/></article>
    <Button onClick={()=>setOpen(true)}>Mở xác nhận mẫu</Button>
    <Modal open={open} title="Xác nhận thay đổi lịch học" onRequestClose={()=>setOpen(false)}><p>IELTS-01 · Ngày 02/11/2026 · Phòng P101</p><p>Máy chủ kiểm tra lại toàn bộ lịch, quyền và dữ liệu hiện tại trước khi lưu.</p><Button onClick={()=>setOpen(false)}>Xác nhận lịch</Button></Modal>
  </section></main>;
}
if (import.meta.env.DEV) createRoot(document.getElementById('root')!).render(<SpacingReview/>);
