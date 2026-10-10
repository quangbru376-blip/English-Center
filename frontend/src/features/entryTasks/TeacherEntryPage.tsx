import { useCallback, useState } from 'react';
import { registrationLabel, bookingLabel, sessionLabel, scoreLabel, vietnamDateTime } from '../../lib/enrollmentDisplay';
import type { SessionController } from '../identity/sessionController';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { Modal } from '../../components/ui/Modal';
import { PageHeader } from '../../components/ui/PageHeader';
import { useOwnedTask } from '../../lib/useOwnedTask';
import type { Page } from '../../lib/m1Wire';
import { EntryScoreForm } from './components/EntryScoreForm';
import { teacherSessions, teacherRoster, teacherTask, teacherCommand, teacherLookup, validateScore } from './teacherApi';
import type { TeacherSession, TeacherRoster, TeacherTask } from './teacherApi';
const emptyFilter = { fromDate: '', toDate: '', effectiveStatus: '', sort: 'START_ASC' };
const emptyRoster = { name: '', email: '', phone: '', bookingStatus: '', view: 'current', sort: 'BOOKED_ASC' };
const actionCode = { GRADE: 'TEACHER.ENTRY_RESULT.CREATE', MARK_ABSENT: 'TEACHER.ENTRY_BOOKING.ABSENCE', CORRECT_SCORE: 'TEACHER.ENTRY_RESULT.CORRECT' };
export function TeacherEntryPage({ controller, onPassword }: {
    controller: SessionController;
    onPassword: () => void;
}) {
    const client = controller.client;
    const [filter, setFilter] = useState(emptyFilter), [sessions, setSessions] = useState<Page<TeacherSession> | null>(null), [session, setSession] = useState<TeacherSession | null>(null), [rosterFilter, setRosterFilter] = useState(emptyRoster), [roster, setRoster] = useState<Page<TeacherRoster> | null>(null), [detail, setDetail] = useState<TeacherTask | null>(null);
    const [mode, setMode] = useState<'GRADE' | 'MARK_ABSENT' | 'CORRECT_SCORE' | null>(null), [fields, setFields] = useState({ score: '', reason: '' }), [unknown, setUnknown] = useState<{
        requestId: string;
        action: string;
        booking: number;
    } | null>(null);
    const clear = useCallback(() => { setFilter(emptyFilter); setSessions(null); setSession(null); setRosterFilter(emptyRoster); setRoster(null); setDetail(null); setMode(null); setFields({ score: '', reason: '' }); setUnknown(null); }, []);
    const task = useOwnedTask(client, clear);
    const search = (page = 1) => void task.run(signal => teacherSessions(client, { ...Object.fromEntries(Object.entries(filter).filter(([, v]) => v)), page: String(page) }, signal), setSessions);
    const read = (id: number) => void task.run(signal => teacherTask(client, id, signal), value => { setDetail(value); setFields({ score: '', reason: '' }); setMode(null); }, undefined, () => { setDetail(null); setMode(null); });
    const searchRoster = (page = 1) => { if (session)
        void task.run(signal => teacherRoster(client, session.id, { ...Object.fromEntries(Object.entries(rosterFilter).filter(([, v]) => v)), page }, signal), setRoster, undefined, () => { setRoster(null); setDetail(null); setMode(null); setFields({ score: '', reason: '' }); }); };
    function save() {
        if (!mode || !detail || unknown)
            return;
        const reason = fields.reason.normalize('NFC').trim();
        if (mode !== 'MARK_ABSENT' && !validateScore(fields.score)) {
            task.setNotice('Nhập điểm từ 0 đến 100, tối đa hai chữ số thập phân.');
            return;
        }
        if (mode === 'CORRECT_SCORE' && (!reason || Array.from(reason).length > 1000)) {
            task.setNotice('Lý do bắt buộc, tối đa 1.000 ký tự.');
            return;
        }
        const manifest = mode === 'GRADE' ? detail.gradeManifest : mode === 'MARK_ABSENT' ? detail.absenceManifest : detail.correctionManifest;
        if (!manifest)
            return;
        const requestId = crypto.randomUUID(), action = actionCode[mode], booking = detail.booking.id;
        const body = { ...manifest, ...mode !== 'MARK_ABSENT' ? { score: fields.score } : {}, ...mode === 'CORRECT_SCORE' ? { reason } : {} };
        setFields({ score: '', reason: '' });
        setMode(null);
        void task.run(signal => teacherCommand(client, mode, detail, body, requestId, signal), r => { setDetail(r.task); setRoster(null); setSessions(null); }, () => { setDetail(null); setRoster(null); setUnknown({ requestId, action, booking }); }, () => { setDetail(null); setRoster(null); });
    }
    if (client.auth.context?.role !== 'Teacher' || client.auth.context.scope !== 'NORMAL')
        return <Feedback kind="error" message="Không có quyền xem nhiệm vụ giáo viên."/>;
    return <section><PageHeader title="Nhiệm vụ kiểm tra đầu vào"/><p className="mb-3">Lịch và thời gian hiển thị theo giờ Việt Nam.</p><div className="mb-4 flex flex-wrap gap-3"><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button><Button variant="secondary" disabled={task.pending} onClick={() => void controller.logout()}>Đăng xuất</Button></div>
    <form className="grid gap-3 md:grid-cols-2" onSubmit={e => { e.preventDefault(); search(); }}><TextField label="Từ ngày" type="date" value={filter.fromDate} onChange={e => setFilter({ ...filter, fromDate: e.currentTarget.value })}/><TextField label="Đến ngày" type="date" value={filter.toDate} onChange={e => setFilter({ ...filter, toDate: e.currentTarget.value })}/><label>Trạng thái ca<select aria-label="Trạng thái ca" className="block min-h-[44px] border" value={filter.effectiveStatus} onChange={e => setFilter({ ...filter, effectiveStatus: e.currentTarget.value })}><option value="">Tất cả</option>{['OPEN', 'CLOSED', 'CANCELLED'].map(x => <option key={x} value={x}>{sessionLabel(x)}</option>)}</select></label><label>Sắp xếp ca<select aria-label="Sắp xếp ca" className="block min-h-[44px] border" value={filter.sort} onChange={e => setFilter({ ...filter, sort: e.currentTarget.value })}><option value="START_ASC">Bắt đầu tăng dần</option><option value="START_DESC">Bắt đầu giảm dần</option></select></label><Button type="submit" pending={task.pending}>Đọc ca được phân công</Button></form>
    {task.notice && <Feedback kind="error" message={task.notice}/>}
    {sessions && <><p>{sessions.totalCount} ca — trang {sessions.page}</p>{!sessions.items.length && <Feedback kind="empty" message="Chưa có ca được phân công phù hợp."/>}<ul>{sessions.items.map(s => <li key={s.id} className="my-3 border p-3"><p>{s.date.split('-').reverse().join('/')} {s.startTime}–{s.endTime} · {s.room} · {sessionLabel(s.effectiveStatus)}</p><p>Đang đặt {s.taskCounts.scheduled} · Đã chấm {s.taskCounts.completed} · Vắng {s.taskCounts.absent}</p><Button disabled={task.pending} onClick={() => { setSession(s); setRoster(null); setDetail(null); setMode(null); }}>Xem nhiệm vụ ca {s.id}</Button></li>)}</ul><Button disabled={task.pending || sessions.page === 1} onClick={() => search(sessions.page - 1)}>Ca trang trước</Button><Button disabled={task.pending || sessions.page * sessions.pageSize >= sessions.totalCount} onClick={() => search(sessions.page + 1)}>Ca trang sau</Button></>}
    {session && <article className="mt-4 border p-4"><h2 className="font-semibold">Ca {session.id} · {session.room}</h2><form className="grid gap-3 md:grid-cols-3" onSubmit={e => { e.preventDefault(); searchRoster(); }}>{(['name', 'email', 'phone'] as const).map((key, i) => <TextField key={key} label={['Tên học viên', 'Email học viên', 'Điện thoại học viên'][i]} value={rosterFilter[key]} onChange={e => setRosterFilter({ ...rosterFilter, [key]: e.currentTarget.value })}/>)}<label>Phạm vi<select aria-label="Phạm vi" className="block min-h-[44px] border" value={rosterFilter.view} onChange={e => setRosterFilter({ ...rosterFilter, view: e.currentTarget.value })}><option value="current">Đang đặt</option><option value="history">Toàn bộ lịch sử</option></select></label><label>Trạng thái đặt<select aria-label="Trạng thái đặt" className="block min-h-[44px] border" value={rosterFilter.bookingStatus} onChange={e => setRosterFilter({ ...rosterFilter, bookingStatus: e.currentTarget.value })}><option value="">Tất cả</option>{['SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED'].map(x => <option key={x} value={x}>{bookingLabel(x)}</option>)}</select></label><Button type="submit" pending={task.pending}>Đọc danh sách học viên</Button></form>
      {roster && <><p>{roster.totalCount} lượt — trang {roster.page}</p>{!roster.items.length && <Feedback kind="empty" message="Không có lượt đặt phù hợp."/>}<ul>{roster.items.map(r => <li key={r.bookingId} className="my-3 border p-3"><p>{r.student.fullName} · {r.student.email} · {r.student.phone}</p><p>{r.course.name} · Lượt {r.attemptNumber} · {bookingLabel(r.bookingStatus)}</p><p>Điểm ca này: {scoreLabel(r.targetScore)} · Band hiện tại: {r.currentBand?.code ?? 'Chưa có'}</p><Button disabled={task.pending} onClick={() => read(r.bookingId)}>Xem lượt đặt {r.bookingId}</Button></li>)}</ul><Button disabled={task.pending || roster.page === 1} onClick={() => searchRoster(roster.page - 1)}>Danh sách trang trước</Button><Button disabled={task.pending || roster.page * roster.pageSize >= roster.totalCount} onClick={() => searchRoster(roster.page + 1)}>Danh sách trang sau</Button></>}
    </article>}
    {detail && <article className="mt-4 border p-4"><h2 className="font-semibold">{detail.student.fullName} · Lượt {detail.booking.attemptNumber}</h2><p>{detail.course.name} · {registrationLabel(detail.registrationStatus)} · {bookingLabel(detail.booking.status)}</p><p>Điểm lượt đặt này: {scoreLabel(detail.targetResult?.score ?? null)}</p>{detail.targetResult && <p>Giáo viên chấm ban đầu: {detail.targetResult.originalGraderId} · {vietnamDateTime(detail.targetResult.gradedAt)}</p>}{detail.absence && <p>Đã xác nhận vắng. Lượt thi được giữ để học viên đặt lại.</p>}<ul>{detail.relatedAttempts.map(a => <li key={a.id}>Lượt {a.attemptNumber}: {scoreLabel(a.score)}</li>)}</ul><p>Band hiện tại: {detail.bandAssignment?.band.code ?? 'Chưa có'}</p><div className="flex flex-wrap gap-3">{(['GRADE', 'MARK_ABSENT', 'CORRECT_SCORE'] as const).map((a, i) => <Button key={a} disabled={task.pending || !!unknown || !detail.allowedActions.includes(a)} onClick={() => { setMode(a); setFields({ score: a === 'CORRECT_SCORE' ? detail.targetResult?.score ?? '' : '', reason: '' }); }}>{['Chấm điểm', 'Xác nhận vắng', 'Sửa điểm có lý do'][i]}</Button>)}<Button disabled={task.pending} onClick={() => read(detail.booking.id)}>Tải lại lượt đặt</Button></div><ul>{Object.entries(detail.disabledReasons).map(([a, r]) => <li key={a}>{r}</li>)}</ul></article>}
    {unknown && <div className="mt-4 flex flex-wrap gap-3"><Button disabled={task.pending} onClick={() => void task.run(signal => teacherLookup(client, unknown.action, unknown.requestId, signal), r => { setUnknown(null); read(r.target.bookingId); })}>Kiểm tra kết quả</Button><Button disabled={task.pending} onClick={() => read(unknown.booking)}>Đối chiếu lượt đặt</Button><Button disabled={task.pending} onClick={() => { setUnknown(null); setDetail(null); task.setNotice('Kết quả cũ chưa xác nhận; tải lại nhiệm vụ trước khi tạo ý định mới.'); }}>Kết thúc đối chiếu</Button></div>}
    <Modal open={!!mode} title={mode === 'GRADE' ? 'Ghi điểm đầu vào' : mode === 'MARK_ABSENT' ? 'Xác nhận vắng thi' : 'Sửa điểm đầu vào'} onRequestClose={() => { if (!task.pending) {
        setMode(null);
        setFields({ score: '', reason: '' });
    } }}>{mode && <EntryScoreForm mode={mode} value={fields} disabled={task.pending || !!unknown} onChange={setFields} onSubmit={save} onCancel={() => { setMode(null); setFields({ score: '', reason: '' }); }}/>}</Modal>
  </section>;
}
