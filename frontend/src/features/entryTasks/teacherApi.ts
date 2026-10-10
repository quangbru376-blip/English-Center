import type { HttpClient } from '../../lib/http';
import { object, requireWire, text, version, instant, references, strings, capabilities, course, band, decimal } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import type { Completion } from '../../lib/m1Wire';
import { validId, validUuid } from '../administration/types';
import { decodeBooking, decodeBand } from '../enrollment/enrollmentApi';
import type { Booking, BandAssignment, CourseRef, Manifest } from '../enrollment/enrollmentApi';
export type TeacherSession = {
    id: number;
    date: string;
    startTime: string;
    endTime: string;
    startAt: string;
    endAt: string;
    room: string;
    status: string;
    effectiveStatus: string;
    taskCounts: {
        scheduled: number;
        completed: number;
        absent: number;
        cancelled: number;
    };
    version: string;
};
export type Contact = {
    studentId: number;
    fullName: string;
    email: string;
    phone: string;
};
export type TeacherRoster = {
    bookingId: number;
    attemptId: number;
    registrationId: number;
    attemptNumber: number;
    bookedAt: string;
    bookingStatus: string;
    registrationStatus: string;
    student: Contact;
    course: CourseRef;
    targetScore: string | null;
    targetGradedAt: string | null;
    currentBand: {
        id: number;
        code: string;
        minScore: string;
    } | null;
};
export type RelatedAttempt = {
    id: number;
    attemptNumber: number;
    version: string;
    score: string | null;
    gradedAt: string | null;
    completedBookingId: number | null;
};
export type TeacherTask = {
    booking: Booking;
    student: Contact;
    course: CourseRef;
    registrationStatus: string;
    registrationVersion: string;
    relatedAttempts: RelatedAttempt[];
    bandAssignment: BandAssignment | null;
    targetResult: {
        score: string;
        gradedAt: string;
        originalGraderId: number;
    } | null;
    absence: {
        at: string;
        actorId: number;
    } | null;
    cancellation: {
        at: string;
        reason: string | null;
    } | null;
    allowedActions: string[];
    disabledReasons: Record<string, string>;
    gradeManifest: Manifest | null;
    absenceManifest: Manifest | null;
    correctionManifest: Manifest | null;
};
function contact(v: unknown) { const r = object(v, ['studentId', 'fullName', 'email', 'phone']); requireWire(validId(r.studentId) && text(r.fullName) && text(r.email) && typeof r.phone === 'string' && /^0[35789]\d{8}$/.test(r.phone)); }
const nullableScore = (v: unknown) => v === null || typeof v === 'string' && /^(0|[1-9]\d?|100)\.\d{2}$/.test(v) && BigInt(v.replace('.', '')) <= 10000n;
const nullableAt = (v: unknown) => v === null || instant(v);
const status = (v: unknown) => ['PENDING', 'TESTING', 'BAND_ASSIGNED', 'CONFIRMED', 'ADMITTED', 'REJECTED', 'CANCELLED'].includes(v as string);
export function decodeTeacherSession(v: unknown): TeacherSession { const r = object(v, ['id', 'date', 'startTime', 'endTime', 'startAt', 'endAt', 'room', 'status', 'effectiveStatus', 'taskCounts', 'version']); const count = object(r.taskCounts, ['scheduled', 'completed', 'absent', 'cancelled']); requireWire(validId(r.id) && typeof r.date === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(r.date) && typeof r.startTime === 'string' && /^\d{2}:\d{2}$/.test(r.startTime) && typeof r.endTime === 'string' && /^\d{2}:\d{2}$/.test(r.endTime) && instant(r.startAt) && instant(r.endAt) && r.startAt < r.endAt && text(r.room) && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.status as string) && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.effectiveStatus as string) && version(r.version) && Object.values(count).every(x => Number.isSafeInteger(x) && (x as number) >= 0)); return r as unknown as TeacherSession; }
export function decodeTeacherRoster(v: unknown): TeacherRoster { const r = object(v, ['bookingId', 'attemptId', 'registrationId', 'attemptNumber', 'bookedAt', 'bookingStatus', 'registrationStatus', 'student', 'course', 'targetScore', 'targetGradedAt', 'currentBand']); contact(r.student); course(r.course); requireWire([r.bookingId, r.attemptId, r.registrationId].every(validId) && [1, 2].includes(r.attemptNumber as number) && instant(r.bookedAt) && ['SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED'].includes(r.bookingStatus as string) && status(r.registrationStatus) && nullableScore(r.targetScore) && nullableAt(r.targetGradedAt)); if (r.currentBand !== null)
    band(r.currentBand); return r as unknown as TeacherRoster; }
function manifest(v: unknown, api: number, task: TeacherTask) {
    const m = object(v, ['expectedVersions', 'expectedSet']);
    const keys = ['session', 'registration', 'attempt', 'booking', 'relatedAttempts', ...api === 71 ? [] : ['bandAssignment']];
    const rv = object(m.expectedVersions, keys);
    for (const key of ['session', 'registration', 'attempt', 'booking'])
        requireWire(version(rv[key]));
    references(rv.relatedAttempts);
    const set = object(m.expectedSet, ['attemptIds', 'currentBookingId', 'completedBookingId', ...api === 71 ? [] : ['bandAssignmentId']]);
    requireWire(Array.isArray(set.attemptIds) && set.attemptIds.every(validId) && new Set(set.attemptIds).size === set.attemptIds.length && set.attemptIds.length === task.relatedAttempts.length && task.relatedAttempts.every(a => (set.attemptIds as number[]).includes(a.id)));
    requireWire(set.currentBookingId === (api === 72 ? null : task.booking.id) && set.completedBookingId === (api === 72 ? task.booking.id : null));
    if (api !== 71) {
        requireWire(set.bandAssignmentId === (task.bandAssignment?.id ?? null));
        if (rv.bandAssignment !== null)
            references([rv.bandAssignment]);
        requireWire((rv.bandAssignment === null) === (task.bandAssignment === null));
    }
}
export function decodeTeacherTask(v: unknown): TeacherTask {
    const r = object(v, ['booking', 'student', 'course', 'registrationStatus', 'registrationVersion', 'relatedAttempts', 'bandAssignment', 'targetResult', 'absence', 'cancellation', 'allowedActions', 'disabledReasons', 'gradeManifest', 'absenceManifest', 'correctionManifest']);
    decodeBooking(r.booking);
    contact(r.student);
    course(r.course);
    requireWire(status(r.registrationStatus) && version(r.registrationVersion) && Array.isArray(r.relatedAttempts) && r.relatedAttempts.length <= 2);
    const ids = new Set<number>();
    for (const raw of r.relatedAttempts) {
        const a = object(raw, ['id', 'attemptNumber', 'version', 'score', 'gradedAt', 'completedBookingId']);
        requireWire(validId(a.id) && !ids.has(a.id) && a.attemptNumber === ids.size + 1 && version(a.version) && nullableScore(a.score) && nullableAt(a.gradedAt) && (a.completedBookingId === null || validId(a.completedBookingId)));
        ids.add(a.id);
    }
    const booking = r.booking as Booking;
    requireWire(ids.has(booking.attemptId));
    if (r.bandAssignment !== null)
        decodeBand(r.bandAssignment);
    if (r.targetResult !== null) {
        const result = object(r.targetResult, ['score', 'gradedAt', 'originalGraderId']);
        requireWire(decimal(result.score) && nullableScore(result.score) && instant(result.gradedAt) && validId(result.originalGraderId) && booking.status === 'COMPLETED');
    }
    if (r.absence !== null) {
        const a = object(r.absence, ['at', 'actorId']);
        requireWire(instant(a.at) && validId(a.actorId) && booking.status === 'ABSENT');
    }
    if (r.cancellation !== null) {
        const c = object(r.cancellation, ['at', 'reason']);
        requireWire(instant(c.at) && (c.reason === null || text(c.reason)));
    }
    capabilities(r, ['GRADE', 'MARK_ABSENT', 'CORRECT_SCORE']);
    requireWire(strings(r.allowedActions));
    const task = r as unknown as TeacherTask;
    for (const [key, api, action] of [['gradeManifest', 70, 'GRADE'], ['absenceManifest', 71, 'MARK_ABSENT'], ['correctionManifest', 72, 'CORRECT_SCORE']] as const) {
        requireWire((task[key] !== null) === task.allowedActions.includes(action));
        if (task[key] !== null)
            manifest(task[key], api, task);
    }
    return task;
}
export function teacherSessions(c: HttpClient, query: Record<string, string>, signal: AbortSignal) { return c.request({ endpoint: 'teacherSessions', slot: 'teacher-sessions', query, signal, decode: v => page(v, decodeTeacherSession) }); }
export function teacherRoster(c: HttpClient, id: number, body: unknown, signal: AbortSignal) { return c.request({ endpoint: 'teacherRoster', slot: 'teacher-roster', targetId: id, body, signal, decode: v => page(v, decodeTeacherRoster) }); }
export function teacherTask(c: HttpClient, id: number, signal: AbortSignal) { return c.request({ endpoint: 'teacherTask', slot: 'teacher-task', targetId: id, signal, decode: v => { const result = decodeTeacherTask(v); requireWire(result.booking.id === id); return result; } }); }
export function teacherCommand(c: HttpClient, action: 'GRADE' | 'MARK_ABSENT' | 'CORRECT_SCORE', task: TeacherTask, body: unknown, requestId: string, signal: AbortSignal) { return c.request({ endpoint: action === 'GRADE' ? 'teacherGrade' : action === 'MARK_ABSENT' ? 'teacherAbsence' : 'teacherCorrect', slot: 'teacher-command', body, requestId, targetId: action === 'CORRECT_SCORE' ? task.booking.attemptId : task.booking.id, signal, decode: v => { const r = object(v, ['task', 'operationId']); requireWire(validUuid(r.operationId)); const result = decodeTeacherTask(r.task); requireWire(result.booking.id === task.booking.id && result.booking.attemptId === task.booking.attemptId && result.booking.registrationId === task.booking.registrationId); return { task: result, operationId: r.operationId as string }; } }); }
export function teacherLookup(c: HttpClient, action: string, requestId: string, signal: AbortSignal) { return c.request({ endpoint: 'teacherResult', slot: 'teacher-lookup', query: { action }, requestId, signal, decode: v => { const r = object(v, ['outcome', 'operationId', 'resultMode', 'target']); const target = object(r.target, ['bookingId', 'attemptId', 'registrationId']); requireWire(r.outcome === 'COMPLETED' && validUuid(r.operationId) && r.resultMode === 'SAFE_REFERENCE' && Object.values(target).every(validId)); return r as unknown as Completion; } }); }
export function validateScore(score: string) { return /^(0|[1-9]\d{0,2})(\.\d{1,2})?$/.test(score) && BigInt((score.split('.')[0] + (score.split('.')[1] ?? '').padEnd(2, '0'))) <= 10000n; }
