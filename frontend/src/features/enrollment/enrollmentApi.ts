import type { Endpoint, HttpClient } from '../../lib/http';
import { object, requireWire, text, version, instant, strings, references, course, capabilities } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import type { Completion } from '../../lib/m1Wire';
import { record } from '../../lib/apiError';
import { validId, validUuid } from '../administration/types';
export type CourseRef = {
    id: number;
    code: string;
    name: string;
    status: string;
};
export type CourseChoice = CourseRef & {
    tuition: string;
};
export type Profile = {
    accountId: number;
    studentId: number;
    fullName: string;
    email: string;
    dateOfBirth: string;
    phone: string;
    address: string | null;
    versions: {
        account: string;
        profile: string;
    };
};
export type EntrySession = {
    id: number;
    startAt: string;
    endAt: string;
    room: string;
    status: string;
    effectiveStatus: string;
    version: string;
};
export type Booking = {
    id: number;
    registrationId: number;
    attemptId: number;
    sessionId: number;
    attemptNumber: number;
    status: string;
    bookedAt: string;
    version: string;
    session: EntrySession;
};
export type Attempt = {
    id: number;
    attemptNumber: number;
    version: string;
    score: string | null;
    gradedAt: string | null;
    completedBookingId: number | null;
    currentBooking: Booking | null;
    latestBooking: Booking | null;
};
export type BandAssignment = {
    id: number;
    bestAttemptId: number;
    band: {
        id: number;
        code: string;
    };
    assignedAt: string;
    version: string;
};
export type Entry = {
    attempts: Attempt[];
    bandAssignment: BandAssignment | null;
    bestAttemptId: number | null;
};
export type Invoice = {
    id: number;
    number: string;
    amount: string;
    issuedAt: string;
    dueDate: string;
    status: string;
    isOverdue: boolean;
};
export type Registration = {
    id: number;
    course: CourseRef;
    status: string;
    createdAt: string;
    admissionConfirmedAt: string | null;
    band: {
        id: number;
        code: string;
    } | null;
    invoiceId: number | null;
    classId: number | null;
    version: string;
    entry: Entry;
    invoice: Invoice | null;
    class: {
        classId: number;
        assignmentId: number;
        code: string;
        status: string;
    } | null;
    rejection: {
        at: string;
        reason: string;
    } | null;
    cancellation: {
        at: string;
        reason: string | null;
    } | null;
    admissionApprovedAt: string | null;
    allowedActions: string[];
    disabledReasons: Record<string, string>;
};
export type Manifest = {
    expectedVersions: Readonly<Record<string, unknown>>;
    expectedSet: Readonly<Record<string, unknown>>;
};
export type BookContext = {
    attemptNumber: number;
    attemptId: number | null;
    attemptVersion: string | null;
    currentBookingId: number | null;
};
export type Options = {
    registrationId: number;
    registrationVersion: string;
    eligible: boolean;
    blockedReasons: string[];
    attemptContext: BookContext | null;
    items: (EntrySession & {
        availableSlots: number;
    })[];
    page: number;
    pageSize: number;
    totalCount: number;
};
export type Cancellation = {
    eligible: boolean;
    blockedReasons: string[];
    asOfUtc: string;
    effects: {
        futureBookings: Booking[];
        invoice: Invoice | null;
    };
    manifest: Manifest | null;
};
export type Confirmation = {
    eligible: boolean;
    blockedReasons: string[];
    asOfUtc: string;
    entry: Entry;
    manifest: Manifest | null;
};
export type Home = {
    registrationId: number;
    course: CourseRef;
    status: string;
    createdAt: string;
    currentStep: string;
    nextAction: {
        action: string;
        target: Record<string, number>;
    } | null;
    waitingReason: string | null;
    nearestEvent: ({
        kind: 'ENTRY_TEST';
        bookingId: number;
    } | {
        kind: 'CLASS_SESSION';
        scheduleId: number;
        classId: number;
    }) & {
        registrationId: number;
        startAt: string;
        endAt: string;
        room: string;
    } | null;
    unpaidInvoice: {
        id: number;
        number: string;
        amount: string;
        dueDate: string;
        isOverdue: boolean;
    } | null;
    finalizedReport: {
        gradeReportId: number;
        registrationId: number;
        classId: number;
    } | null;
};
export const studentActions = ['VIEW_ENTRY_RESULTS', 'BOOK_ENTRY_TEST', 'CANCEL_ENTRY_BOOKING', 'RESCHEDULE_ENTRY_BOOKING', 'CANCEL_REGISTRATION', 'CONFIRM_REGISTRATION', 'VIEW_INVOICE', 'VIEW_CLASS', 'VIEW_FINALIZED_REPORT'] as const;
const statuses = ['PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED'];
const date = (v: unknown): v is string => typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v) && Number.isFinite(Date.parse(v + 'T00:00:00Z')) && new Date(v + 'T00:00:00Z').toISOString().slice(0, 10) === v;
const money = (v: unknown): v is string => typeof v === 'string' && /^[1-9]\d{0,17}$/.test(v);
const nullableId = (v: unknown) => v === null || validId(v);
const nullableAt = (v: unknown) => v === null || instant(v);
const score = (v: unknown) => v === null || typeof v === 'string' && /^(?:\d|[1-9]\d|100)\.\d{2}$/.test(v) && BigInt(v.replace('.', '')) <= 10000n;
function bandRef(v: unknown) { const r = object(v, ['id', 'code']); requireWire(validId(r.id) && text(r.code)); }
export function decodeProfile(v: unknown): Profile { const r = object(v, ['accountId', 'studentId', 'fullName', 'email', 'dateOfBirth', 'phone', 'address', 'versions']); const rv = object(r.versions, ['account', 'profile']); requireWire(validId(r.accountId) && validId(r.studentId) && text(r.fullName) && text(r.email) && date(r.dateOfBirth) && typeof r.phone === 'string' && /^0[35789]\d{8}$/.test(r.phone) && (r.address === null || text(r.address)) && version(rv.account) && version(rv.profile)); return r as unknown as Profile; }
export function decodeCourse(v: unknown): CourseChoice { const r = object(v, ['id', 'code', 'name', 'tuition', 'status']); course({ id: r.id, code: r.code, name: r.name, status: r.status }); requireWire(r.status === 'ACTIVE' && money(r.tuition)); return r as unknown as CourseChoice; }
export function decodeSession(v: unknown): EntrySession { const r = object(v, ['id', 'startAt', 'endAt', 'room', 'status', 'effectiveStatus', 'version']); requireWire(validId(r.id) && instant(r.startAt) && instant(r.endAt) && r.startAt < r.endAt && text(r.room) && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.status as string) && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.effectiveStatus as string) && version(r.version)); return r as unknown as EntrySession; }
export function decodeBooking(v: unknown): Booking { const r = object(v, ['id', 'registrationId', 'attemptId', 'sessionId', 'attemptNumber', 'status', 'bookedAt', 'version', 'session']); const s = decodeSession(r.session); requireWire([r.id, r.registrationId, r.attemptId, r.sessionId].every(validId) && [1, 2].includes(r.attemptNumber as number) && ['SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED'].includes(r.status as string) && instant(r.bookedAt) && version(r.version) && s.id === r.sessionId); return r as unknown as Booking; }
export function decodeBand(v: unknown): BandAssignment { const r = object(v, ['id', 'bestAttemptId', 'band', 'assignedAt', 'version']); bandRef(r.band); requireWire(validId(r.id) && validId(r.bestAttemptId) && instant(r.assignedAt) && version(r.version)); return r as unknown as BandAssignment; }
export function decodeEntry(v: unknown): Entry {
    const r = object(v, ['attempts', 'bandAssignment', 'bestAttemptId']);
    requireWire(Array.isArray(r.attempts) && r.attempts.length <= 2 && nullableId(r.bestAttemptId));
    const ids = new Set<number>();
    for (const raw of r.attempts) {
        const a = object(raw, ['id', 'attemptNumber', 'version', 'score', 'gradedAt', 'completedBookingId', 'currentBooking', 'latestBooking']);
        requireWire(validId(a.id) && !ids.has(a.id) && a.attemptNumber === ids.size + 1 && version(a.version) && score(a.score) && nullableAt(a.gradedAt) && nullableId(a.completedBookingId));
        ids.add(a.id);
        for (const key of ['currentBooking', 'latestBooking'])
            if (a[key] !== null) {
                const b = decodeBooking(a[key]);
                requireWire(b.attemptId === a.id && b.attemptNumber === a.attemptNumber && (key !== 'currentBooking' || b.status === 'SCHEDULED'));
            }
        requireWire((a.score === null) === (a.gradedAt === null) && (a.score === null) === (a.completedBookingId === null));
    }
    if (r.bandAssignment !== null) {
        const b = decodeBand(r.bandAssignment);
        requireWire(b.bestAttemptId === r.bestAttemptId && ids.has(b.bestAttemptId));
    }
    else
        requireWire(r.bestAttemptId === null);
    return r as unknown as Entry;
}
function invoice(v: unknown): Invoice { const r = object(v, ['id', 'number', 'amount', 'issuedAt', 'dueDate', 'status', 'isOverdue']); requireWire(validId(r.id) && typeof r.number === 'string' && /^HD-\d{4}-\d{8}$/.test(r.number) && money(r.amount) && instant(r.issuedAt) && date(r.dueDate) && ['UNPAID', 'PAID', 'CANCELLED'].includes(r.status as string) && typeof r.isOverdue === 'boolean'); return r as unknown as Invoice; }
export function decodeRegistration(v: unknown): Registration {
    const r = object(v, ['id', 'course', 'status', 'createdAt', 'admissionConfirmedAt', 'band', 'invoiceId', 'classId', 'version', 'entry', 'invoice', 'class', 'rejection', 'cancellation', 'admissionApprovedAt', 'allowedActions', 'disabledReasons']);
    course(r.course);
    requireWire(validId(r.id) && statuses.includes(r.status as string) && instant(r.createdAt) && nullableAt(r.admissionConfirmedAt) && nullableAt(r.admissionApprovedAt) && nullableId(r.invoiceId) && nullableId(r.classId) && version(r.version));
    decodeEntry(r.entry);
    if (r.band !== null)
        bandRef(r.band);
    if (r.invoice !== null)
        requireWire(invoice(r.invoice).id === r.invoiceId);
    if (r.class !== null) {
        const cls = object(r.class, ['classId', 'assignmentId', 'code', 'status']);
        requireWire(validId(cls.classId) && cls.classId === r.classId && validId(cls.assignmentId) && text(cls.code) && ['DRAFT', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED'].includes(cls.status as string));
    }
    for (const key of ['rejection', 'cancellation'])
        if (r[key] !== null) {
            const reason = object(r[key], ['at', 'reason']);
            requireWire(instant(reason.at) && (text(reason.reason) || key === 'cancellation' && reason.reason === null));
        }
    capabilities(r, studentActions);
    return r as unknown as Registration;
}
function sharedPreview(v: unknown, keys: string[]) { const r = object(v, keys); requireWire(typeof r.eligible === 'boolean' && strings(r.blockedReasons) && instant(r.asOfUtc) && (r.eligible ? r.blockedReasons.length === 0 : r.blockedReasons.length > 0)); if (r.manifest !== null)
    object(r.manifest, ['expectedVersions', 'expectedSet']); requireWire(r.eligible === (r.manifest !== null)); return r; }
export function decodeCancellation(v: unknown): Cancellation {
    const r = sharedPreview(v, ['eligible', 'blockedReasons', 'asOfUtc', 'effects', 'manifest']);
    const effects = object(r.effects, ['futureBookings', 'invoice']);
    requireWire(Array.isArray(effects.futureBookings));
    effects.futureBookings.forEach(decodeBooking);
    if (effects.invoice !== null)
        invoice(effects.invoice);
    if (r.manifest !== null) {
        const m = r.manifest as Manifest;
        const rv = object(m.expectedVersions, ['registration', 'bookings', 'sessions', 'invoice']);
        const set = object(m.expectedSet, ['futureBookingIds', 'invoiceId']);
        requireWire(version(rv.registration) && Array.isArray(set.futureBookingIds) && set.futureBookingIds.every(validId) && nullableId(set.invoiceId));
        references(rv.bookings);
        references(rv.sessions);
        if (rv.invoice !== null)
            references([rv.invoice]);
        requireWire((rv.invoice === null) === (set.invoiceId === null));
    }
    return r as unknown as Cancellation;
}
export function decodeConfirmation(v: unknown): Confirmation {
    const r = sharedPreview(v, ['eligible', 'blockedReasons', 'asOfUtc', 'entry', 'manifest']);
    decodeEntry(r.entry);
    if (r.manifest !== null) {
        const m = r.manifest as Manifest;
        const rv = object(m.expectedVersions, ['registration', 'bandAssignment', 'attempts', 'bookings']);
        const set = object(m.expectedSet, ['bandAssignmentId', 'bestAttemptId', 'attemptIds', 'scheduledBookingIds']);
        requireWire(version(rv.registration) && version(rv.bandAssignment) && validId(set.bandAssignmentId) && validId(set.bestAttemptId));
        references(rv.attempts);
        references(rv.bookings);
        for (const key of ['attemptIds', 'scheduledBookingIds'])
            requireWire(Array.isArray(set[key]) && (set[key] as unknown[]).every(validId));
    }
    return r as unknown as Confirmation;
}
export function decodeOptions(v: unknown): Options {
    const r = object(v, ['registrationId', 'registrationVersion', 'eligible', 'blockedReasons', 'attemptContext', 'items', 'page', 'pageSize', 'totalCount']);
    requireWire(validId(r.registrationId) && version(r.registrationVersion) && typeof r.eligible === 'boolean' && strings(r.blockedReasons));
    const items = page({ items: r.items, page: r.page, pageSize: r.pageSize, totalCount: r.totalCount }, raw => { const o = object(raw, ['id', 'startAt', 'endAt', 'room', 'status', 'effectiveStatus', 'version', 'availableSlots']); const { availableSlots, ...session } = o; decodeSession(session); requireWire(Number.isInteger(availableSlots) && (availableSlots as number) > 0 && (availableSlots as number) <= 20 && o.status === 'OPEN' && o.effectiveStatus === 'OPEN'); return o; });
    if (r.attemptContext !== null) {
        const a = object(r.attemptContext, ['attemptNumber', 'attemptId', 'attemptVersion', 'currentBookingId']);
        requireWire([1, 2].includes(a.attemptNumber as number) && nullableId(a.attemptId) && (a.attemptVersion === null || version(a.attemptVersion)) && (a.attemptId === null) === (a.attemptVersion === null) && nullableId(a.currentBookingId) && (a.currentBookingId === null || a.attemptId !== null));
    }
    requireWire(r.eligible ? r.attemptContext !== null && r.blockedReasons.length === 0 : r.attemptContext === null && r.blockedReasons.length > 0 && items.totalCount === 0);
    return r as unknown as Options;
}
export function decodeHome(v: unknown): Home {
    const r = object(v, ['registrationId', 'course', 'status', 'createdAt', 'currentStep', 'nextAction', 'waitingReason', 'nearestEvent', 'unpaidInvoice', 'finalizedReport']);
    course(r.course);
    requireWire(validId(r.registrationId) && statuses.includes(r.status as string) && instant(r.createdAt) && text(r.currentStep) && (r.waitingReason === null || text(r.waitingReason)));
    if (r.nextAction !== null) {
        const a = object(r.nextAction, ['action', 'target']);
        requireWire(studentActions.includes(a.action as typeof studentActions[number]) && record(a.target));
        const keys = a.action === 'VIEW_INVOICE' ? ['registrationId', 'invoiceId'] : a.action === 'VIEW_CLASS' ? ['registrationId', 'classId'] : a.action === 'VIEW_FINALIZED_REPORT' ? ['registrationId', 'classId', 'gradeReportId'] : ['registrationId'];
        const target = object(a.target, keys);
        requireWire(Object.values(target).every(validId) && target.registrationId === r.registrationId);
    }
    if (r.nearestEvent !== null) {
        requireWire(record(r.nearestEvent));
        const event = object(r.nearestEvent, r.nearestEvent.kind === 'ENTRY_TEST' ? ['kind', 'registrationId', 'bookingId', 'startAt', 'endAt', 'room'] : ['kind', 'registrationId', 'scheduleId', 'classId', 'startAt', 'endAt', 'room']);
        requireWire(['ENTRY_TEST', 'CLASS_SESSION'].includes(event.kind as string) && event.registrationId === r.registrationId && instant(event.startAt) && instant(event.endAt) && text(event.room));
        if (event.kind === 'ENTRY_TEST')
            requireWire(validId(event.bookingId));
        else
            requireWire(validId(event.scheduleId) && validId(event.classId));
    }
    if (r.unpaidInvoice !== null) {
        const i = object(r.unpaidInvoice, ['id', 'number', 'amount', 'dueDate', 'isOverdue']);
        requireWire(validId(i.id) && typeof i.number === 'string' && /^HD-\d{4}-\d{8}$/.test(i.number) && money(i.amount) && date(i.dueDate) && typeof i.isOverdue === 'boolean');
    }
    if (r.finalizedReport !== null) {
        const f = object(r.finalizedReport, ['gradeReportId', 'registrationId', 'classId']);
        requireWire(Object.values(f).every(validId) && f.registrationId === r.registrationId);
    }
    return r as unknown as Home;
}
export function decodeCommand(v: unknown) {
    requireWire(record(v));
    if (Object.hasOwn(v, 'registration')) {
        const r = object(v, ['registration', 'operationId']);
        requireWire(validUuid(r.operationId));
        return { registration: decodeRegistration(r.registration), operationId: r.operationId as string };
    }
    const moved = Object.hasOwn(v, 'oldBooking');
    const r = object(v, moved ? ['oldBooking', 'newBooking', 'entry', 'registrationStatus', 'registrationVersion', 'operationId'] : ['booking', 'entry', 'registrationStatus', 'registrationVersion', 'operationId']);
    requireWire(validUuid(r.operationId) && statuses.includes(r.registrationStatus as string) && version(r.registrationVersion));
    decodeEntry(r.entry);
    const b = decodeBooking(moved ? r.newBooking : r.booking);
    if (moved) {
        const old = decodeBooking(r.oldBooking);
        requireWire(old.registrationId === b.registrationId && old.attemptId === b.attemptId);
    }
    return { registrationId: b.registrationId, operationId: r.operationId as string };
}
export function decodeCompletion(v: unknown, action: string): Completion { const r = object(v, ['outcome', 'operationId', 'resultMode', 'target']); const keys = action === 'STUDENT.ENTRY_BOOKING.RESCHEDULE' ? ['registrationId', 'oldBookingId', 'newBookingId'] : action.startsWith('STUDENT.ENTRY_BOOKING.') ? ['registrationId', 'bookingId'] : ['registrationId']; const target = object(r.target, keys); requireWire(r.outcome === 'COMPLETED' && validUuid(r.operationId) && r.resultMode === 'SAFE_REFERENCE' && Object.values(target).every(validId)); return r as unknown as Completion; }
export const loadProfile = (c: HttpClient, signal: AbortSignal) => c.request({ endpoint: 'studentProfile', slot: 'student-profile', signal, decode: decodeProfile });
export const loadRegistration = (c: HttpClient, id: number, signal: AbortSignal) => c.request({ endpoint: 'studentRegistration', slot: 'student-registration', targetId: id, signal, decode: v => { const result = decodeRegistration(v); requireWire(result.id === id); return result; } });
export const loadHome = (c: HttpClient, p: number, signal: AbortSignal) => c.request({ endpoint: 'studentHome', slot: 'student-home', query: { page: String(p) }, signal, decode: v => page(v, decodeHome) });
export const loadCourses = (c: HttpClient, p: number, signal: AbortSignal) => c.request({ endpoint: 'studentCourses', slot: 'student-courses', query: { page: String(p) }, signal, decode: v => page(v, decodeCourse) });
export const loadOptions = (c: HttpClient, id: number, query: Record<string, string>, signal: AbortSignal) => c.request({ endpoint: 'studentOptions', slot: 'student-options', targetId: id, query, signal, decode: v => { const result = decodeOptions(v); requireWire(result.registrationId === id && (result.attemptContext === null || result.attemptContext.currentBookingId === (query.bookingId ? Number(query.bookingId) : null))); return result; } });
export const loadCalendar = (c: HttpClient, query: Record<string, string>, signal: AbortSignal) => c.request({ endpoint: 'studentCalendar', slot: 'student-calendar', query, signal, decode: v => page(v, decodeBooking) });
export const previewCancel = (c: HttpClient, id: number, signal: AbortSignal) => c.request({ endpoint: 'studentCancelPreview', slot: 'student-preview', targetId: id, signal, decode: decodeCancellation });
export const previewConfirm = (c: HttpClient, id: number, signal: AbortSignal) => c.request({ endpoint: 'studentConfirmPreview', slot: 'student-preview', targetId: id, signal, decode: decodeConfirmation });
export const studentCommand = (c: HttpClient, endpoint: Endpoint, body: unknown, requestId: string, signal: AbortSignal, targetId?: number, registrationId?: number) => c.request({ endpoint, slot: 'student-command', body, requestId, signal, targetId, registrationId, decode: v => {
        const result = decodeCommand(v);
        requireWire(record(v));
        if (result.registration) {
            if (endpoint === 'studentCreate')
                requireWire(record(body) && result.registration.course.id === body.courseId);
            else
                requireWire(result.registration.id === targetId);
        }
        else if (endpoint === 'studentCancelBooking' || endpoint === 'studentReschedule') {
            requireWire(result.registrationId === registrationId);
            requireWire((endpoint === 'studentReschedule' ? (v.oldBooking as Booking) : (v.booking as Booking)).id === targetId);
        }
        else
            requireWire(result.registrationId === targetId);
        return result;
    } });
export const studentLookup = (c: HttpClient, action: string, requestId: string, signal: AbortSignal) => c.request({ endpoint: 'studentResult', slot: 'student-lookup', requestId, query: { action }, signal, decode: v => decodeCompletion(v, action) });
export const updateProfile = (c: HttpClient, body: unknown, signal: AbortSignal) => c.request({ endpoint: 'studentProfileUpdate', slot: 'student-profile-command', body, signal, decode: decodeProfile });
