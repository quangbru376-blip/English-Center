import { useCallback, useEffect, useRef, useState } from 'react';
import { AccountActions } from '../../app/AccountActions';
import { registrationLabel, bookingLabel, sessionLabel, invoiceLabel, classLabel, scoreLabel, vietnamDateTime } from '../../lib/enrollmentDisplay';
import type { SessionController } from '../identity/sessionController';
import type { Endpoint } from '../../lib/http';
import type { Page } from '../../lib/m1Wire';
import { useOwnedTask } from '../../lib/useOwnedTask';
import { Button } from '../../components/ui/Button';
import { TextField } from '../../components/ui/TextField';
import { Feedback } from '../../components/ui/Feedback';
import { Modal } from '../../components/ui/Modal';
import { Card, Badge, Skeleton } from '../../components/ui/Surfaces';
import { StudentProfileSummary, StudentBookings } from './components/StudentPersonal';
import { StudentHome, StudentCourses } from './components/StudentOverview';
import { PageHeader } from '../../components/ui/PageHeader';
import { StudentProfileForm } from './components/StudentProfileForm';
import type { ProfileFields } from './components/StudentProfileForm';
import { validProfileDraft } from './profileValidation';
import { loadProfile, loadRegistration, loadHome, loadCourses, loadOptions, loadCalendar, previewCancel, previewConfirm, studentCommand, studentLookup, updateProfile } from './enrollmentApi';
import type { Profile, Home, CourseChoice, Registration, Options, Booking, Cancellation, Confirmation } from './enrollmentApi';
const blank: ProfileFields = { fullName: '', dateOfBirth: '', phone: '', address: '' };
const emptyDates = { fromDate: '', toDate: '' };
const money = (v: string) => v.replace(/\B(?=(\d{3})+(?!\d))/g, '.') + ' ₫';
const at = vietnamDateTime;
type Intent = {
    endpoint: Endpoint;
    body: unknown;
    action: string;
    target?: number;
    registration?: number;
    summary: string;
};
export function StudentEnrollmentPage({ controller, onPassword, onGradeReport, onInvoice, onClass, onChooseCourses, viewRequest, onBusyChange }: {
    controller: SessionController;
    onPassword: () => void;
    onGradeReport?: (id:number) => void;
    onInvoice?: (id: number) => void;
    onClass?: () => void;
    onChooseCourses?: () => void;
    viewRequest?: { view: 'home'|'courses'|'calendar'|'profile'; sequence: number };
    onBusyChange?: (busy: boolean) => void;
}) {
    const client = controller.client;
    const [view, setView] = useState<'home' | 'courses' | 'calendar' | 'profile'>(viewRequest?.view ?? 'home'), [home, setHome] = useState<Page<Home> | null>(null), [courses, setCourses] = useState<Page<CourseChoice> | null>(null), [registration, setRegistration] = useState<Registration | null>(null), [options, setOptions] = useState<Options | null>(null), [dates, setDates] = useState(emptyDates), [selected, setSelected] = useState(0);
    const [calendar, setCalendar] = useState<Page<Booking> | null>(null), [calendarView, setCalendarView] = useState('current'), [profile, setProfile] = useState<Profile | null>(null), [draft, setDraft] = useState(blank), [editing, setEditing] = useState(false), [profileUnknown, setProfileUnknown] = useState(false);
    const [preview, setPreview] = useState<Cancellation | Confirmation | null>(null), [intent, setIntent] = useState<Intent | null>(null), [unknown, setUnknown] = useState<{
        requestId: string;
        action: string;
        registration?: number;
    } | null>(null);
    const [optionsDirty, setOptionsDirty] = useState(false);
    const [success, setSuccess] = useState<string | null>(null);
    const [moving, setMoving] = useState<Booking | null>(null);
    const clear = useCallback(() => { setOptionsDirty(false); setSuccess(null); setView('home'); setHome(null); setCourses(null); setRegistration(null); setOptions(null); setMoving(null); setDates(emptyDates); setSelected(0); setCalendar(null); setCalendarView('current'); setProfile(null); setDraft(blank); setEditing(false); setProfileUnknown(false); setPreview(null); setIntent(null); setUnknown(null); }, []);
    const task = useOwnedTask(client, clear);
    useEffect(() => { onBusyChange?.(task.pending); return () => onBusyChange?.(false); }, [onBusyChange, task.pending]);
    const profileDirty = editing && (!profile || JSON.stringify(draft) !== JSON.stringify({ fullName: profile.fullName, dateOfBirth: profile.dateOfBirth, phone: profile.phone, address: profile.address ?? '' }));
    const setNotice = task.setNotice;
    const changeView = useCallback((next: 'home'|'courses'|'calendar'|'profile', confirmed = false) => {
        if (next === view) return;
        if (!confirmed && profileDirty && !window.confirm('Có dữ liệu chưa lưu. Rời trang và bỏ thay đổi?')) return;
        setOptionsDirty(false); setView(next); setSuccess(null); setNotice(null); setHome(null); setCourses(null); setCalendar(null); setDates(emptyDates); setCalendarView('current'); setRegistration(null); setOptions(null); setMoving(null); setSelected(0); setPreview(null); setIntent(null);
        if (!profileUnknown) { setEditing(false); setDraft(blank); }
    }, [profileUnknown, profileDirty, setNotice, view]);
    const run = task.run;
    useEffect(() => {
        if (client.auth.context?.role !== 'Student' || client.auth.context.scope !== 'NORMAL') return;
        const timer = window.setTimeout(() => {
            if (view === 'home') void run(signal => loadHome(client, 1, signal), setHome, undefined, () => setHome(null));
            if (view === 'courses') void run(signal => loadCourses(client, 1, signal), setCourses, undefined, () => setCourses(null));
            if (view === 'profile') void run(signal => loadProfile(client, signal), setProfile, undefined, () => setProfile(null));
            if (view === 'calendar') void run(signal => loadCalendar(client, { view: 'current', page: '1' }, signal), setCalendar, undefined, () => setCalendar(null));
        }, 0);
        return () => window.clearTimeout(timer);
    }, [client, view, run]);
    useEffect(() => { if (registration) document.getElementById('student-registration-detail')?.focus(); }, [registration]);
    useEffect(() => { if (options) document.getElementById('student-booking-options')?.focus(); }, [options]);
    const previousViewRequest = useRef(viewRequest);
    useEffect(() => { if (viewRequest && viewRequest !== previousViewRequest.current) { previousViewRequest.current = viewRequest; changeView(viewRequest.view, true); } }, [viewRequest, changeView]);
    const read = (id: number) => void task.run(signal => loadRegistration(client, id, signal), r => { setRegistration(r); setOptions(null); setMoving(null); setSelected(0); setPreview(null); setIntent(null); }, undefined, () => { setRegistration(null); setOptions(null); setMoving(null); setPreview(null); setIntent(null); });
    const readHome = (page = 1) => void task.run(signal => loadHome(client, page, signal), setHome, undefined, () => setHome(null));
    const readCourses = (page = 1) => void task.run(signal => loadCourses(client, page, signal), setCourses, undefined, () => setCourses(null));
    const readProfile = () => void task.run(signal => loadProfile(client, signal), r => { setProfile(r); if (!editing)
        setDraft({ fullName: r.fullName, dateOfBirth: r.dateOfBirth, phone: r.phone, address: r.address ?? '' }); }, undefined, () => setProfile(null));
    const readCalendar = (page = 1) => void task.run(signal => loadCalendar(client, { ...Object.fromEntries(Object.entries(dates).filter(([, v]) => v)), view: calendarView, page: String(page) }, signal), setCalendar, undefined, () => setCalendar(null));
    const readOptions = (page = 1, source: Booking | null = moving) => { if (registration)
        void task.run(signal => loadOptions(client, registration.id, { ...Object.fromEntries(Object.entries(dates).filter(([, v]) => v)), ...(source ? { bookingId: String(source.id) } : {}), page: String(page) }, signal), r => { setOptionsDirty(false); setOptions(r); setMoving(source); setSelected(0); }, undefined, () => { setOptions(null); setMoving(null); setSelected(0); }); };
    function pickSlot() {
        if (!options || !registration || optionsDirty)
            return;
        const s = options.items.find(x => x.id === selected), a = options.attemptContext;
        if (!s || !a)
            return;
        if (moving) {
            if (a.attemptId !== moving.attemptId || a.currentBookingId !== moving.id || !a.attemptVersion)
                return;
            propose({ endpoint: 'studentReschedule', target: moving.id, registration: registration.id, action: 'STUDENT.ENTRY_BOOKING.RESCHEDULE', summary: `Đổi lượt đặt ${moving.id} từ ${at(moving.session.startAt)} sang ${at(s.startAt)} tại ${s.room}. Lượt thi và lịch sử được giữ nguyên.`, body: { targetSessionId: s.id, expectedVersions: { registration: options.registrationVersion, attempt: a.attemptVersion, booking: moving.version, sourceSession: moving.session.version, targetSession: s.version } } });
        }
        else
            propose({ endpoint: 'studentBook', target: registration.id, action: 'STUDENT.ENTRY_BOOKING.CREATE', summary: `Đặt lượt ${a.attemptNumber} vào ${at(s.startAt)} tại ${s.room}. Chỗ được kiểm tra lại khi xác nhận.`, body: { targetSessionId: s.id, attemptContext: a, expectedVersions: { registration: options.registrationVersion, targetSession: s.version } } });
    }
    function propose(value: Intent) { if (!unknown) {
        setSuccess(null);
        setIntent({ ...value, body: structuredClone(value.body) });
        setPreview(null);
    } }
    function submit() {
        if (!intent || unknown)
            return;
        const frozen = intent, requestId = crypto.randomUUID();
        setIntent(null);
        setPreview(null);
        setOptions(null);
        setMoving(null);
        setSelected(0);
        void task.run(signal => studentCommand(client, frozen.endpoint, frozen.body, requestId, signal, frozen.target, frozen.registration), r => { setSuccess('Thao tác đã hoàn tất. Đối chiếu thông tin hồ sơ bên dưới.'); setHome(null); setCalendar(null); if (r.registration)
            setRegistration(r.registration);
        else
            read(r.registrationId); }, () => { setRegistration(null); setHome(null); setCalendar(null); setUnknown({ requestId, action: frozen.action, registration: frozen.registration ?? (frozen.endpoint === 'studentCreate' ? undefined : frozen.target) }); }, () => { setRegistration(null); setHome(null); setCalendar(null); });
    }
    function saveProfile() {
        if (!profile || profileUnknown)
            return;
        if (!validProfileDraft(draft)) {
            task.setNotice('Kiểm tra tên, ngày sinh không ở tương lai, số di động Việt Nam và địa chỉ tối đa 255 ký tự.');
            return;
        }
        const body = { ...draft, address: draft.address || null, expectedVersions: profile.versions };
        void task.run(signal => updateProfile(client, body, signal), r => { setProfile(r); setDraft({ fullName: r.fullName, dateOfBirth: r.dateOfBirth, phone: r.phone, address: r.address ?? '' }); setEditing(false); }, () => { setProfile(null); setProfileUnknown(true); }, () => { setProfile(null); });
    }
    function cancelBooking(b: Booking) {
        if (!registration)
            return;
        const a = registration.entry.attempts.find(x => x.id === b.attemptId);
        if (!a)
            return;
        propose({ endpoint: 'studentCancelBooking', target: b.id, registration: registration.id, action: 'STUDENT.ENTRY_BOOKING.CANCEL', summary: `Hủy lượt đặt ${b.id} vào ${at(b.session.startAt)}. Lượt thi và lịch sử được giữ nguyên.`, body: { expectedVersions: { registration: registration.version, attempt: a.version, booking: b.version, sourceSession: b.session.version } } });
    }
    if (client.auth.context?.role !== 'Student' || client.auth.context.scope !== 'NORMAL')
        return <Feedback kind="error" message="Không có quyền đọc hồ sơ học viên."/>;
    const mutationBlocked = task.pending || !!unknown;
    const openNext = (item: Home) => {
        if (item.nextAction?.action === 'VIEW_FINALIZED_REPORT' && onGradeReport) onGradeReport(item.nextAction.target.gradeReportId);
        else if (item.nextAction?.action === 'VIEW_INVOICE' && onInvoice) onInvoice(item.nextAction.target.invoiceId);
        else if (item.nextAction?.action === 'VIEW_CLASS' && onClass) onClass();
        else read(item.registrationId);
    };
    const titles = { home: 'Trang chủ học viên', courses: 'Chọn khóa học', calendar: 'Lịch thi của tôi', profile: 'Thông tin cá nhân' };

    return <section className="student-page" data-unsaved-student={profileDirty ? "true" : undefined}><div className="student-intro"><div><PageHeader title={titles[view]}/><p className="text-muted">Lịch và thời gian hiển thị theo giờ Việt Nam.</p></div><Badge tone="info">Góc học tập của bạn</Badge></div>{!viewRequest && <nav aria-label="Học viên" className="mb-4 flex flex-wrap gap-3">{(['home', 'courses', 'calendar', 'profile'] as const).map((v, i) => <Button key={v} variant={view === v ? 'primary' : 'secondary'} disabled={task.pending} onClick={() => changeView(v)}>{['Trang chủ', 'Chọn khóa học', 'Lịch thi', 'Thông tin cá nhân'][i]}</Button>)}</nav>}<AccountActions><Button variant="secondary" onClick={onPassword}>Đổi mật khẩu</Button><Button variant="secondary" disabled={task.pending} onClick={() => void controller.logout()}>Đăng xuất</Button></AccountActions>
    {task.notice && <Feedback kind="error" message={task.notice}/>}
    {success && <Feedback kind="success" message={success}/>}
    {view === 'home' && <><div><Button variant="secondary" disabled={task.pending} onClick={() => readHome()}>Đọc trang chủ</Button></div>{home && <StudentHome data={home} busy={task.pending} read={read} changePage={readHome} chooseCourses={onChooseCourses ?? (() => changeView('courses'))} openNext={openNext} openReport={onGradeReport}/>}</>}
    {view === 'courses' && <><div><Button variant="secondary" disabled={task.pending} onClick={() => readCourses()}>Đọc khóa học đang mở</Button></div>{courses && <StudentCourses data={courses} busy={task.pending} blocked={mutationBlocked} changePage={readCourses} register={c => propose({ endpoint: 'studentCreate', body: { courseId: c.id }, action: 'STUDENT.REGISTRATION.CREATE', summary: `Tạo hồ sơ cho khóa ${c.name}. Học phí trên là giá hiện tại, hóa đơn được lập khi hồ sơ được duyệt.` })}/>}</>}
    {task.pending && <Skeleton label="Đang đọc hoặc xử lý yêu cầu học viên…"/>}
    {view === 'calendar' && <><form className="student-filter" onSubmit={e => { e.preventDefault(); readCalendar(); }}><DateFields dates={dates} disabled={task.pending} change={value=>{setDates(value);setCalendar(null);}}/><label>Phạm vi lịch<select aria-label="Phạm vi lịch" className="ec-input" value={calendarView} disabled={task.pending} onChange={e => {setCalendarView(e.currentTarget.value);setCalendar(null);}}><option value="current">Đang đặt</option><option value="history">Toàn bộ lịch sử</option></select></label><Button type="submit" pending={task.pending}>Đọc lịch thi</Button></form>{calendar && <StudentBookings data={calendar} busy={task.pending} read={read} changePage={readCalendar}/>}</>}
    {view === 'profile' && <><Button disabled={task.pending} onClick={readProfile}>Đọc thông tin cá nhân</Button>{profile && <StudentProfileSummary profile={profile} busy={task.pending} editing={editing} edit={() => { setDraft({ fullName: profile.fullName, dateOfBirth: profile.dateOfBirth, phone: profile.phone, address: profile.address ?? '' }); setEditing(true); }}/>}{editing && <StudentProfileForm value={draft} disabled={task.pending || profileUnknown || !profile} onChange={setDraft} onSubmit={saveProfile} onCancel={() => { if (!profileUnknown) {
        setEditing(false);
        setDraft(blank);
    } }}/>} {profileUnknown && <div className="student-recovery"><p>Kết quả lưu chưa xác định. Đọc lại thông tin rồi đối chiếu với bản nháp; thao tác cũ không được gửi lại.</p><Button disabled={task.pending} onClick={readProfile}>Đọc thông tin để đối chiếu</Button><Button disabled={task.pending || !profile} onClick={() => { setProfileUnknown(false); task.setNotice('Đã đối chiếu. Bạn có thể chủ đích lưu bản nháp như một thay đổi mới.'); }}>Đã đối chiếu, giữ bản nháp</Button><Button disabled={task.pending || !profile} onClick={() => { if (profile) {
        setDraft({ fullName: profile.fullName, dateOfBirth: profile.dateOfBirth, phone: profile.phone, address: profile.address ?? '' });
        setProfileUnknown(false);
        setEditing(false);
    } }}>Dùng thông tin máy chủ</Button></div>}</>}
    {registration && <Card className="student-detail" id="student-registration-detail" tabIndex={-1} aria-label={`Hồ sơ ${registration.id}`}><div className="student-card-heading"><div><span className="student-eyebrow">{registration.course.code} · Đăng ký {at(registration.createdAt)}</span><h2>Hồ sơ {registration.id} · {registration.course.name}</h2></div><Badge tone="info">{registrationLabel(registration.status)}</Badge></div>{registration.rejection && <p>Từ chối: {registration.rejection.reason}</p>}{registration.cancellation && <p>Đã hủy lúc {at(registration.cancellation.at)}</p>}{(registration.invoice || registration.class) && <dl className="student-summary">{registration.invoice && <div><dt>Học phí · {registration.invoice.number}</dt><dd>{money(registration.invoice.amount)} · {invoiceLabel(registration.invoice.status)}</dd></div>}{registration.class && <div><dt>Lớp của tôi</dt><dd>{registration.class.code} · {classLabel(registration.class.status)}</dd></div>}</dl>}<h3 className="mt-3 font-semibold">Kết quả kiểm tra đầu vào</h3>{!registration.entry.attempts.length && <p>Chưa có lượt thi.</p>}<ul className="student-attempts">{registration.entry.attempts.map(a => <li key={a.id}><h3>Lượt {a.attemptNumber}</h3><p>{scoreLabel(a.score)}</p>{a.currentBooking && <><p>Đang đặt: {at(a.currentBooking.session.startAt)} · {a.currentBooking.session.room} · {sessionLabel(a.currentBooking.session.effectiveStatus)}</p><Button disabled={mutationBlocked || !registration.allowedActions.includes('CANCEL_ENTRY_BOOKING')} onClick={() => cancelBooking(a.currentBooking!)}>Hủy lượt đặt {a.currentBooking.id}</Button><Button disabled={mutationBlocked || !registration.allowedActions.includes('RESCHEDULE_ENTRY_BOOKING')} onClick={() => readOptions(1, a.currentBooking!)}>Đổi lịch lượt đặt {a.currentBooking.id}</Button></>}{a.latestBooking && !a.currentBooking && <p>Lượt đặt gần nhất: {bookingLabel(a.latestBooking.status)} · {at(a.latestBooking.session.startAt)}</p>}</li>)}</ul><p>Band hiện tại: <strong>{registration.entry.bandAssignment?.band.code ?? 'Chưa có'}</strong></p><div className="student-actions"><Button disabled={task.pending} onClick={() => read(registration.id)}>Tải lại hồ sơ</Button><Button disabled={mutationBlocked || !registration.allowedActions.includes('BOOK_ENTRY_TEST')} onClick={() => readOptions(1, null)}>Chọn lịch kiểm tra đầu vào</Button><Button disabled={mutationBlocked || !registration.allowedActions.includes('CANCEL_REGISTRATION')} onClick={() => void task.run(signal => previewCancel(client, registration.id, signal), setPreview)}>Xem trước hủy hồ sơ</Button><Button disabled={mutationBlocked || !registration.allowedActions.includes('CONFIRM_REGISTRATION')} onClick={() => void task.run(signal => previewConfirm(client, registration.id, signal), setPreview)}>Xem trước xác nhận kết quả</Button></div><ul className="student-reasons">{Object.entries(registration.disabledReasons).map(([a, r]) => <li key={a}>{r}</li>)}</ul></Card>}
    {options && registration && <Card className="student-options" id="student-booking-options" tabIndex={-1} aria-label="Chọn ca thi"><h2 className="font-semibold">{moving ? 'Chọn ca thay thế' : 'Chọn ca thi'}</h2><form className="grid gap-3 md:grid-cols-2" onSubmit={e => { e.preventDefault(); readOptions(); }}><DateFields dates={dates} disabled={task.pending} change={value => { setDates(value); setOptionsDirty(true); setSelected(0); setIntent(null); setPreview(null); }}/><Button type="submit" pending={task.pending}>Đọc ca phù hợp</Button></form><ul>{options.blockedReasons.map((r, i) => <li key={i}>{r}</li>)}</ul>{optionsDirty && <Feedback kind="empty" message="Bộ lọc đã thay đổi. Đọc ca phù hợp trước khi chọn lịch."/>}{!optionsDirty && !options.items.length && <Feedback kind="empty" message="Chưa có ca phù hợp; hãy chọn khoảng ngày khác hoặc quay lại sau."/>}<fieldset className="student-slot-list" disabled={task.pending || optionsDirty}><legend className="mb-3">Ca đích — cần chọn chủ đích</legend>{!optionsDirty && options.items.map(s => <label key={s.id} className="student-slot"><input type="radio" name="entry-slot" checked={selected === s.id} onChange={() => setSelected(s.id)}/><span><strong>{at(s.startAt)}</strong><span className="student-fact-note">Đến {at(s.endAt)} · Phòng {s.room} · Còn {s.availableSlots} chỗ</span></span></label>)}</fieldset><Button disabled={task.pending || optionsDirty || options.page === 1} onClick={() => readOptions(options.page - 1)}>Ca trang trước</Button><Button disabled={task.pending || optionsDirty || options.page * options.pageSize >= options.totalCount} onClick={() => readOptions(options.page + 1)}>Ca trang sau</Button><Button disabled={mutationBlocked || optionsDirty || !selected || !options.eligible} onClick={pickSlot}>Xem lại ca đã chọn</Button></Card>}
    {unknown && <div className="student-recovery"><h2>Đối chiếu kết quả thao tác</h2><p>Chưa xác định kết quả. Bạn có thể kiểm tra và đọc lại hồ sơ; thao tác cũ không được gửi lại tự động.</p><div className="student-actions"><Button disabled={task.pending} onClick={() => void task.run(signal => studentLookup(client, unknown.action, unknown.requestId, signal), r => { setUnknown(null); read(r.target.registrationId); })}>Kiểm tra kết quả</Button>{unknown.registration && <Button disabled={task.pending} onClick={() => read(unknown.registration!)}>Đối chiếu hồ sơ</Button>}<Button disabled={task.pending} onClick={() => { setUnknown(null); setRegistration(null); setHome(null); task.setNotice('Kết quả cũ chưa xác nhận; đọc lại hồ sơ trước khi tạo ý định mới.'); }}>Kết thúc đối chiếu</Button></div></div>}
    <Modal open={!!intent} title="Xác nhận thay đổi" onRequestClose={() => { if (!task.pending)
        setIntent(null); }}><p>{intent?.summary}</p><Button disabled={mutationBlocked} onClick={submit}>Xác nhận</Button><Button variant="secondary" disabled={task.pending} onClick={() => setIntent(null)}>Quay lại</Button></Modal>
    <Modal open={!!preview} title={preview && 'effects' in preview ? 'Hủy hồ sơ' : 'Xác nhận kết quả đầu vào'} onRequestClose={() => { if (!task.pending)
        setPreview(null); }}><ul>{preview?.blockedReasons.map((r, i) => <li key={i}>{r}</li>)}</ul>{preview && 'effects' in preview ? <><p>{preview.effects.futureBookings.length} lượt đặt tương lai sẽ bị hủy; lịch sử và kết quả được giữ nguyên.</p><ul>{preview.effects.futureBookings.map(b => <li key={b.id}>{at(b.session.startAt)} · {b.session.room}</li>)}</ul>{preview.effects.invoice?.status === 'UNPAID' && <p>Hóa đơn {preview.effects.invoice.number} chưa thanh toán sẽ bị hủy.</p>}</> : preview && <><p>Band: {preview.entry.bandAssignment?.band.code}</p><ul>{preview.entry.attempts.map(a => <li key={a.id}>Lượt {a.attemptNumber}: {scoreLabel(a.score)}</li>)}</ul><p>Xác nhận gửi hồ sơ sang bước chờ duyệt tuyển sinh.</p></>}
      <Button disabled={mutationBlocked || !preview?.eligible || !preview.manifest || !registration} onClick={() => { if (preview?.manifest && registration) {
        const cancel = 'effects' in preview;
        const frozen: Intent = { endpoint: cancel ? 'studentCancel' : 'studentConfirm', target: registration.id, action: cancel ? 'STUDENT.REGISTRATION.CANCEL' : 'STUDENT.REGISTRATION.CONFIRM', summary: '', body: structuredClone(preview.manifest) };
        const requestId = crypto.randomUUID();
        setPreview(null);
        setOptions(null);
        setMoving(null);
        setSelected(0);
        void task.run(signal => studentCommand(client, frozen.endpoint, frozen.body, requestId, signal, frozen.target), r => { setSuccess('Thao tác đã hoàn tất. Đối chiếu thông tin hồ sơ bên dưới.'); setHome(null); setCalendar(null); if (r.registration)
            setRegistration(r.registration); }, () => { setRegistration(null); setUnknown({ requestId, action: frozen.action, registration: frozen.target }); }, () => { setRegistration(null); setHome(null); });
    } }}>Xác nhận</Button></Modal>
  </section>;
}
function DateFields({ dates, change, disabled }: {
    dates: typeof emptyDates;
    disabled?: boolean;
    change: (v: typeof emptyDates) => void;
}) { return <><TextField label="Từ ngày" type="date" disabled={disabled} value={dates.fromDate} onChange={e => change({ ...dates, fromDate: e.currentTarget.value })}/><TextField label="Đến ngày" type="date" disabled={disabled} value={dates.toDate} onChange={e => change({ ...dates, toDate: e.currentTarget.value })}/></>; }
