import { useCallback, useState } from 'react';
import type { SessionController } from '../identity/sessionController';
import type { Page } from '../../lib/m1Wire';
import { page } from '../../lib/m1Wire';
import { object, requireWire, version, text, strings } from '../../lib/academicWire';
import { validId } from '../administration/types';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { PageHeader } from '../../components/ui/PageHeader';
import { Modal } from '../../components/ui/Modal';
import { vietnamDateTime } from '../../lib/enrollmentDisplay';
import { searchPrograms, loadProgram } from '../programs/programApi';
import type { Program, ProgramSummary } from '../programs/programApi';
import { SchedulesPanel } from './SchedulesPanel';
import { ClassFields } from './components/ClassFields';
import { classList, classDetail, classPreview, classCommand, classLookup, decodeCandidate, decodeClass, decodeScheduling } from './classApi';
import type { ClassRole, ClassDetail, ClassAction, Preview, Candidate, ScheduleCandidate } from './classApi';
type TeacherOption = {
    accountId: number;
    displayName: string;
    status: string;
    version: string;
    eligible: boolean;
    blockedReasons: string[];
};
type RosterRow = {
    classAssignmentId: number;
    registrationId: number;
    assignedAt: string;
    student: {
        studentId: number;
        fullName: string;
        email: string;
        phone: string;
    };
    invoiceStatus?: string;
    entryScore?: string;
};
const labels: Record<ClassAction, string> = { CREATE: 'Tạo lớp', CAPACITY: 'Đổi sĩ số', TEACHER: 'Đổi giáo viên', OPEN: 'Mở lớp', CANCEL: 'Hủy lớp', ASSIGN: 'Xếp học viên', SCHEDULE: 'Tạo lịch' };
const states: Record<string, string> = { PLANNED: 'Đang chuẩn bị', OPEN: 'Đang nhận học viên', ONGOING: 'Đang giảng dạy', COMPLETED: 'Đã hoàn thành', CANCELLED: 'Đã hủy' };
const emptyWindow: ScheduleCandidate = { type: 'LESSON', date: '', startTime: '09:00', endTime: '10:00', room: '' };
type Intent = {
    action: ClassAction;
    id?: number;
    body: unknown;
    description: string;
};
function id(value: string) { return /^[1-9]\d{0,9}$/.test(value) && Number(value) <= 2147483647 ? Number(value) : null; }
export function ClassesPage({ controller, role, onPassword }: {
    controller: SessionController;
    role: ClassRole;
    onPassword: () => void;
}) {
    const c = controller.client;
    const [list, setList] = useState<Page<ClassDetail> | null>(null), [detail, setDetail] = useState<ClassDetail | null>(null), [courseId, setCourseId] = useState(''), [classCode, setClassCode] = useState(''), [view, setView] = useState('current'), [classStatus, setClassStatus] = useState('');
    const [code, setCode] = useState(''), [programId, setProgramId] = useState(''), [teacherId, setTeacherId] = useState(''), [size, setSize] = useState('15'), [programs, setPrograms] = useState<Page<ProgramSummary> | null>(null), [program, setProgram] = useState<Program | null>(null), [teachers, setTeachers] = useState<TeacherOption[]>([]), [creating, setCreating] = useState(false);
    const [preview, setPreview] = useState<Preview | null>(null), [previewAction, setPreviewAction] = useState<ClassAction | null>(null), [reason, setReason] = useState(''), [windowValue, setWindow] = useState(emptyWindow), [intent, setIntent] = useState<Intent | null>(null), [unknown, setUnknown] = useState<{
        action: ClassAction;
        id?: number;
        key: string;
    } | null>(null);
    const [contact, setContact] = useState({ name: '', email: '', phone: '' }), [roster, setRoster] = useState<Page<RosterRow> | null>(null), [candidates, setCandidates] = useState<Page<Candidate> | null>(null), [candidate, setCandidate] = useState<Candidate | null>(null), [options, setOptions] = useState<Page<{
        class: ClassDetail;
        eligible: boolean;
        blockedReasons: string[];
    }> | null>(null);
    const [scheduleLocked,setScheduleLocked]=useState(false);
    const [scheduling, setScheduling] = useState<{
        assignments: unknown[];
        schedules: unknown[];
        currentObligations: unknown[];
    } | null>(null);
    const resetDraft = () => { setPreview(null); setPreviewAction(null); setIntent(null); setReason(''); };
    const clear = useCallback(() => { setList(null); setDetail(null); setCourseId(''); setClassCode(''); setView('current'); setClassStatus(''); setCode(''); setProgramId(''); setTeacherId(''); setSize('15'); setPrograms(null); setProgram(null); setTeachers([]); setCreating(false); setPreview(null); setPreviewAction(null); setReason(''); setWindow(emptyWindow); setIntent(null); setUnknown(null); setContact({ name: '', email: '', phone: '' }); setRoster(null); setCandidates(null); setCandidate(null); setOptions(null);      setScheduling(null); }, []);
    const task = useOwnedTask(c, clear);
    const disabled = task.pending || !!unknown || scheduleLocked;
    function search(number = 1) { if (courseId && !id(courseId)) {
        task.setNotice('Mã khóa phải là số nguyên dương.');
        return;
    } resetDraft(); setDetail(null); setRoster(null); const query: Record<string, string> = { page: String(number), ...courseId ? { courseId } : {} }; if (role === 'AcademicAffairs' && classCode)
        query.classCode = classCode; if (role === 'Student')
        query.view = view;
    else if (classStatus)
        query.status = classStatus; void task.run(s => classList(c, role, query, s), setList, undefined, () => setList(null)); }
    function read(key: number) { resetDraft(); setScheduling(null); setDetail(null); setRoster(null); setTeachers([]); setTeacherId(''); setSize(''); setWindow(emptyWindow); void task.run(s => classDetail(c, role, key, s), r => { setDetail(r); if (role !== 'Student')
        setSize(String(r.maxSize)); }, undefined, () => setDetail(null)); }
    function loadTeachers(key?: number) { void task.run(s => c.request({ endpoint: 'classTeachers', query: key ? { classId: String(key) } : undefined, signal: s, slot: 'class-teachers', decode: v => { const r = object(v, ['items', 'currentTeacherId']); requireWire(Array.isArray(r.items) && (r.currentTeacherId === null || validId(r.currentTeacherId))); return r.items.map(x => { const a = object(x, ['accountId', 'displayName', 'status', 'version', 'eligible', 'blockedReasons']); requireWire(validId(a.accountId) && text(a.displayName) && version(a.version) && typeof a.eligible === 'boolean' && strings(a.blockedReasons)); return a as TeacherOption; }); } }), setTeachers, undefined, () => setTeachers([])); }
    function prepare(action: ClassAction) { if (!detail || unknown)
        return; resetDraft(); setPreviewAction(action); const body = action === 'TEACHER' ? { teacherId: id(teacherId) } : action === 'ASSIGN' ? { registrationId: candidate?.registrationId } : action === 'SCHEDULE' ? { candidate: windowValue } : { action }; if (action === 'TEACHER' && !id(teacherId) || action === 'ASSIGN' && !candidate) {
        task.setNotice('Chọn giáo viên hoặc hồ sơ hợp lệ.');
        return;
    } void task.run(s => classPreview(c, detail.id, action === 'TEACHER' ? 'teacher' : action === 'ASSIGN' ? 'placement' : action === 'SCHEDULE' ? 'schedule' : 'transition', body, s), setPreview, undefined, () => setPreview(null)); }
    function propose(action: ClassAction) {
        if (unknown)
            return;
        if (action === 'CREATE') {
            const selected = teachers.find(t => t.accountId === id(teacherId));
            const n = id(size);
            if (!program || program.id !== id(programId) || !selected?.eligible || !n || n > 15) {
                task.setNotice('Chọn chương trình, giáo viên và sĩ số từ 1 đến 15.');
                return;
            }
            setIntent({ action, body: { classCode: code, programId: program.id, teacherId: selected.accountId, maxSize: n, expectedVersions: { program: program.version, teacher: selected.version, components: program.components.map(x => ({ id: x.id, version: x.version })) }, expectedSet: { componentIds: program.components.map(x => x.id) } }, description: `${code} · ${program.name} · ${selected.displayName} · tối đa ${n} học viên` });
            return;
        }
        if (!detail)
            return;
        if (action === 'CAPACITY') {
            const n = id(size);
            if (!n || n > 15) {
                task.setNotice('Sĩ số phải từ 1 đến 15.');
                return;
            }
            setIntent({ action, id: detail.id, body: { maxSize: n, expectedVersions: { class: detail.version }, expectedSet: { assignmentIds: detail.guardContext.assignments.map(x => x.id) } }, description: `${detail.code}: ${detail.maxSize} → ${n} học viên` });
            return;
        }
        if (previewAction !== action || !preview?.eligible || !preview.manifest)
            return;
        if (action === 'CANCEL' && !reason.trim()) {
            task.setNotice('Nhập lý do hủy lớp.');
            return;
        }
        const body = { ...structuredClone(preview.manifest), ...action === 'CANCEL' ? { reason } : action === 'TEACHER' ? { teacherId: preview.proposedTeacher!.accountId } : action === 'ASSIGN' ? { registrationId: (preview.candidate as Candidate).registrationId } : action === 'SCHEDULE' ? { candidate: preview.candidate } : {} };
        const description = action === 'ASSIGN' ? `${detail.code} · ${(preview.candidate as Candidate).student.fullName}` : action === 'TEACHER' ? `${detail.code} · ${preview.proposedTeacher!.displayName}` : action === 'SCHEDULE' ? `${detail.code} · ${windowValue.type === 'LESSON' ? 'Buổi học' : 'Thi cuối khóa'} ${windowValue.date.split('-').reverse().join('/')} ${windowValue.startTime}–${windowValue.endTime} · ${windowValue.room}` : action === 'CANCEL' ? `${detail.code} · ${reason}` : detail.code;
        setIntent({ action, id: detail.id, body, description });
    }
    function submit() { if (!intent || unknown)
        return; const frozen = intent, key = crypto.randomUUID(); resetDraft(); setDetail(null); setList(null); setRoster(null); setCandidates(null); setOptions(null);   void task.run(s => classCommand(c, frozen.action, frozen.id, frozen.body, key, s), result => { setCreating(false); setCandidate(null); task.setNotice(`Đã thực hiện ${labels[frozen.action].toLowerCase()}. Đọc lại lớp ${result.classId} để đối chiếu.`); read(result.classId); }, () => setUnknown({ action: frozen.action, id: frozen.id, key }), () => setDetail(null)); }
    function lookup() { if (!unknown)
        return; void task.run(s => classLookup(c, unknown.action, unknown.key, unknown.id, s), r => { setUnknown(null); task.setNotice(`Đã xác nhận hoàn tất. Mã lớp: ${r.target.classId}. Đọc lại lớp để đối chiếu.`); read(r.target.classId); }); }
    const filters = () => Object.fromEntries(Object.entries(contact).filter(([, v]) => v !== ''));
    function readRoster(number = 1) { if (!detail)
        return; void task.run(s => c.request({ endpoint: role === 'Teacher' ? 'teacherClassRoster' : 'classRoster', targetId: detail.id, body: { ...filters(), page: number }, signal: s, slot: 'class-roster', decode: v => page(v, x => { const keys = ['classAssignmentId', 'registrationId', 'assignedAt', 'student', ...role === 'AcademicAffairs' ? ['course', 'band', 'entryScore', 'invoiceStatus'] : []]; const r = object(x, keys), st = object(r.student, ['studentId', 'fullName', 'email', 'phone']); requireWire(validId(r.classAssignmentId) && validId(r.registrationId) && validId(st.studentId) && text(st.fullName) && text(st.email) && text(st.phone)); return r as RosterRow; }) }), setRoster, undefined, () => setRoster(null)); }
    function findCandidates(number = 1) { setCandidate(null); setOptions(null); resetDraft(); void task.run(s => c.request({ endpoint: 'classCandidates', body: { ...filters(), ...courseId ? { courseId: id(courseId) } : {}, page: number }, signal: s, slot: 'class-candidates', decode: v => page(v, x => decodeCandidate(x)) }), setCandidates, undefined, () => setCandidates(null)); }
    function candidateOptions(row: Candidate, number = 1) { setCandidate(row); setOptions(null); resetDraft(); void task.run(s => c.request({ endpoint: 'classOptions', targetId: row.registrationId, query: { page: String(number) }, signal: s, slot: 'class-options', decode: v => page(v, x => { const r = object(x, ['class', 'eligible', 'blockedReasons', 'conflicts']); const cl = decodeClass(r.class); requireWire(typeof r.eligible === 'boolean' && strings(r.blockedReasons)); return { class: cl, eligible: r.eligible, blockedReasons: r.blockedReasons }; }) }), setOptions, undefined, () => setCandidate(null)); }
    if (c.auth.context?.role !== role || c.auth.context.scope !== 'NORMAL')
        return <Feedback kind="error" message="Không có quyền xem lớp."/>;
    const paging = (p: Page<unknown>, load: (number: number) => void) => <div className="flex gap-3"><Button disabled={disabled || p.page <= 1} onClick={() => load(p.page - 1)}>Trang trước</Button><Button disabled={disabled || p.page * p.pageSize >= p.totalCount} onClick={() => load(p.page + 1)}>Trang sau</Button></div>;
    return <section><PageHeader title={role === 'Student' ? 'Lớp của tôi' : role === 'Teacher' ? 'Lớp phụ trách' : 'Lớp, xếp học viên và lịch học'}/><Button onClick={onPassword}>Đổi mật khẩu</Button><Button disabled={task.pending} onClick={() => void controller.logout()}>Đăng xuất</Button>{task.pending && <Feedback kind="loading" message="Đang xử lý…"/>}{task.notice && <Feedback kind="error" message={task.notice}/>}{unknown && <div><p>Chưa xác định kết quả. Không tạo thao tác mới trước khi đối chiếu.</p><Button disabled={task.pending} onClick={lookup}>Kiểm tra kết quả</Button>{unknown.id && <Button disabled={task.pending} onClick={() => read(unknown.id!)}>Đọc lớp để đối chiếu</Button>}</div>}
    <form className="my-4 flex flex-wrap gap-3" onSubmit={e => { e.preventDefault(); search(); }}><TextField label="Mã định danh khóa" value={courseId} onChange={e => setCourseId(e.target.value)} disabled={disabled}/>{role === 'AcademicAffairs' && <TextField label="Mã lớp" value={classCode} disabled={disabled} onChange={e => setClassCode(e.target.value)}/>}<label>{role === 'Student' ? 'Phạm vi' : 'Trạng thái'}<select aria-label={role === 'Student' ? 'Phạm vi' : 'Trạng thái'} value={role === 'Student' ? view : classStatus} onChange={e => role === 'Student' ? setView(e.target.value) : setClassStatus(e.target.value)} disabled={disabled} className="block min-h-11 border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2">{role === 'Student' ? <><option value="current">Đang học</option><option value="history">Lịch sử</option></> : <><option value="">Tất cả</option>{Object.entries(states).map(([k, v]) => <option key={k} value={k}>{v}</option>)}</>}</select></label><Button type="submit" disabled={disabled}>Tìm lớp</Button></form>
    {list && <div>{!list.items.length && <Feedback kind="empty" message="Không có lớp phù hợp."/>}{list.items.map(x => <article key={x.id} className="my-3 border-b border-[var(--ec-border)] pb-3"><p>{x.code} · {x.program.name} · {x.teacher.displayName} · {states[x.status]}</p><Button disabled={task.pending} onClick={() => read(x.id)}>Xem lớp {x.id}</Button></article>)}{paging(list, search)}</div>}
    {role === 'AcademicAffairs' && <div className="my-4"><Button disabled={disabled} onClick={() => { setCreating(true); setDetail(null); resetDraft(); }}>Tạo lớp mới</Button>{creating && <div><Button disabled={disabled} onClick={() => void task.run(s => searchPrograms(c, { status: 'ACTIVE', page: '1' }, s), setPrograms)}>Đọc chương trình đang dùng</Button><Button disabled={disabled} onClick={() => loadTeachers()}>Đọc giáo viên</Button>{programs && paging(programs, n => void task.run(s => searchPrograms(c, { status: 'ACTIVE', page: String(n) }, s), setPrograms))}<ClassFields code={code} programId={programId} teacherId={teacherId} maxSize={size} programs={(programs?.items ?? []).map(p => ({ id: p.id, label: p.name }))} teachers={teachers.map(t => ({ id: t.accountId, label: t.displayName, disabled: !t.eligible }))} disabled={disabled} onCode={setCode} onProgram={v => { setProgramId(v); setProgram(null); if (id(v))
        void task.run(s => loadProgram(c, id(v)!, s), setProgram); }} onTeacher={setTeacherId} onMaxSize={setSize} onSubmit={() => propose('CREATE')} onCancel={() => setCreating(false)}/></div>}</div>}
    {detail && <article className="my-4 border-t border-[var(--ec-border)] pt-4"><h2 className="text-lg font-semibold">{detail.code} · {states[detail.status]}</h2><p>{detail.program.course.name} · {detail.program.band.code} · {detail.program.name} · {detail.teacher.displayName}</p>{role !== 'Student' && <p>Sĩ số: {detail.placedCount}/{detail.maxSize} · Nghĩa vụ buổi học: {detail.prerequisites.currentLessonCount}/{detail.prerequisites.expectedLessonCount}</p>}{detail.cancellation && <p className="whitespace-pre-wrap">Hủy lúc {vietnamDateTime(detail.cancellation.at)} · {detail.cancellation.reason}</p>}<Button disabled={task.pending} onClick={() => read(detail.id)}>Đọc lại lớp</Button>
      {role === 'AcademicAffairs' && <div className="my-3 grid gap-3"><div className="flex flex-wrap gap-3"><TextField label="Sĩ số tối đa" value={size} onChange={e => setSize(e.target.value)} disabled={disabled}/><Button disabled={disabled || !detail.allowedActions.includes('EDIT_CAPACITY')} onClick={() => propose('CAPACITY')}>Chuẩn bị đổi sĩ số</Button><Button disabled={disabled} onClick={() => loadTeachers(detail.id)}>Đọc lựa chọn giáo viên</Button></div><label>Giáo viên mới<select aria-label="Giáo viên mới" className="block min-h-11 border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2" value={teacherId} disabled={disabled} onChange={e => { setTeacherId(e.target.value); resetDraft(); }}><option value="">Chọn giáo viên</option>{teachers.map(t => <option key={t.accountId} value={t.accountId} disabled={!t.eligible}>{t.displayName}{t.eligible ? '' : ' · chưa thể chọn'}</option>)}</select></label><div className="flex flex-wrap gap-3">{(['TEACHER', 'OPEN', 'CANCEL'] as const).map(a => <Button key={a} disabled={disabled || !detail.allowedActions.includes(a === 'TEACHER' ? 'CHANGE_TEACHER' : a)} onClick={() => prepare(a)}>Xem trước {labels[a].toLowerCase()}</Button>)}</div><TextField label="Lý do hủy lớp" value={reason} disabled={disabled} onChange={e => { setReason(e.target.value); setIntent(null); }}/>
        <h3 className="font-semibold">Thêm một lịch cụ thể</h3><Button disabled={disabled} onClick={() => void task.run(s => c.request({ endpoint: 'classScheduling', targetId: detail.id, signal: s, slot: 'class-scheduling', decode: v => decodeScheduling(v, detail.id) }), setScheduling, undefined, () => setScheduling(null))}>Đọc đầy đủ bối cảnh lập lịch</Button>{scheduling && <p>{scheduling.assignments.length} học viên · {scheduling.schedules.length} lịch được lưu · {scheduling.currentObligations.length} nghĩa vụ hiện hành.</p>}<label>Loại lịch<select aria-label="Loại lịch" value={windowValue.type} className="block min-h-11 border border-[var(--ec-border)] bg-[var(--ec-surface)] p-2" disabled={disabled} onChange={e => { setWindow({ ...windowValue, type: e.target.value as ScheduleCandidate['type'] }); resetDraft(); }}><option value="LESSON">Buổi học</option><option value="FINAL_EXAM">Thi cuối khóa</option></select></label><div className="grid gap-3 sm:grid-cols-2">{(['date', 'startTime', 'endTime', 'room'] as const).map((k, i) => <TextField key={k} label={['Ngày', 'Bắt đầu', 'Kết thúc', 'Phòng'][i]} type={k === 'date' ? 'date' : k === 'room' ? 'text' : 'time'} value={windowValue[k]} disabled={disabled} onChange={e => { setWindow({ ...windowValue, [k]: e.target.value }); resetDraft(); }}/>)}</div><Button disabled={disabled || !detail.allowedActions.includes('CREATE_SCHEDULE')} onClick={() => prepare('SCHEDULE')}>Xem trước lịch</Button>
        {candidate && <div><p>Hồ sơ đã thanh toán: {candidate.student.fullName} · {candidate.course.name} · {candidate.band.code}</p><Button disabled={disabled} onClick={() => prepare('ASSIGN')}>Xem trước xếp học viên vào lớp này</Button></div>}{Object.entries(detail.disabledReasons).map(([k, v]) => <p key={k}>{v}</p>)}</div>}
      {role !== 'Student' && <div><h3 className="font-semibold">Danh sách học viên</h3><div className="grid gap-3 sm:grid-cols-3">{(['name', 'email', 'phone'] as const).map((k, i) => <TextField key={k} label={['Tên học viên', 'Email học viên', 'Số điện thoại'][i]} value={contact[k]} disabled={disabled} onChange={e => setContact({ ...contact, [k]: e.target.value })}/>)}</div><Button disabled={disabled} onClick={() => readRoster()}>Đọc danh sách học viên</Button>{roster && <div>{!roster.items.length && <Feedback kind="empty" message="Không có học viên phù hợp."/>}{roster.items.map(r => <p key={r.classAssignmentId}>{r.student.fullName} · {r.student.email} · {r.student.phone}{r.invoiceStatus ? ' · Đã thanh toán' : ''}</p>)}{paging(roster, readRoster)}</div>}</div>}
    </article>}
    {preview && <div className="my-4 border border-[var(--ec-border)] p-3"><h3>Xem trước {previewAction ? labels[previewAction].toLowerCase() : ''}</h3>{preview.blockedReasons.map(x => <p key={x}>{x}</p>)}{preview.conflicts?.map((x, i) => <p key={i}>{x.message} · {vietnamDateTime(x.startAt)}–{vietnamDateTime(x.endAt)}</p>)}{preview.effects?.map(x => <p key={x}>{x}</p>)}{preview.eligible && previewAction && <Button disabled={disabled} onClick={() => propose(previewAction)}>Chuẩn bị xác nhận</Button>}</div>}
    {role === 'AcademicAffairs' && <div className="my-4 border-t border-[var(--ec-border)] pt-4"><h2 className="text-lg font-semibold">Hồ sơ chờ xếp lớp</h2>{!detail && <div className="grid gap-3 sm:grid-cols-3">{(['name', 'email', 'phone'] as const).map((k, i) => <TextField key={k} label={['Tên học viên', 'Email học viên', 'Số điện thoại'][i]} value={contact[k]} disabled={disabled} onChange={e => setContact({ ...contact, [k]: e.target.value })}/>)}</div>}<Button disabled={disabled} onClick={() => findCandidates()}>Tìm hồ sơ đã thanh toán</Button>{candidates && <div>{!candidates.items.length && <Feedback kind="empty" message="Không có hồ sơ chờ xếp lớp."/>}{candidates.items.map(x => <article className="my-3" key={x.registrationId}><p>{x.student.fullName} · {x.student.email} · {x.course.name} · {x.band.code} · Đã thanh toán</p><Button disabled={disabled} onClick={() => void task.run(s => c.request({ endpoint: 'classCandidate', targetId: x.registrationId, signal: s, slot: 'class-candidate', decode: v => { const row = decodeCandidate(v, true); requireWire(row.registrationId === x.registrationId); return row; } }), setCandidate, undefined, () => setCandidate(null))}>Xem hồ sơ {x.registrationId}</Button><Button disabled={disabled} onClick={() => candidateOptions(x)}>Lớp phù hợp cho hồ sơ {x.registrationId}</Button></article>)}{paging(candidates, findCandidates)}</div>}{options && <div>{!options.items.length && <Feedback kind="empty" message="Đang chờ lớp phù hợp."/>}{options.items.map(x => <article key={x.class.id} className="my-3"><p>{x.class.code} · {x.class.placedCount}/{x.class.maxSize}</p>{x.blockedReasons.map(r => <p key={r}>{r}</p>)}<Button disabled={task.pending} onClick={() => read(x.class.id)}>Chọn lớp {x.class.code} để xem trước</Button></article>)}{candidate && paging(options, n => candidateOptions(candidate, n))}</div>}
    </div>}
    <SchedulesPanel controller={controller} role={role} classId={detail?.id} parentBusy={task.pending || unknown!==null} onLock={setScheduleLocked} onClass={read}/>
    <Modal open={intent !== null} title={intent ? labels[intent.action] : 'Xác nhận'} onRequestClose={() => setIntent(null)}><p className="whitespace-pre-wrap">{intent?.description}</p><p>Kiểm tra thông tin trước khi xác nhận. Máy chủ kiểm tra lại quyền, lịch và dữ liệu hiện tại.</p><Button disabled={disabled} onClick={submit}>Xác nhận thao tác</Button></Modal>
  </section>;
}
