import type { Endpoint, HttpClient } from '../../lib/http';
import { object, requireWire, text, version, instant, decimal, strings, references, capabilities, course, band } from '../../lib/academicWire';
import { completion, page } from '../../lib/m1Wire';
import { validId, validUuid } from '../administration/types';
export type Grader = {
    accountId: number;
    displayName: string;
};
export type Option = Grader & {
    eligible: boolean;
    blockedReasons: string[];
};
export type Options = {
    currentGrader: Option | null;
    items: Option[];
    page: number;
    pageSize: number;
    totalCount: number;
};
export type SessionView = {
    id: number;
    date: string;
    startTime: string;
    endTime: string;
    startAt: string;
    endAt: string;
    room: string;
    capacity: number;
    status: 'OPEN' | 'CLOSED' | 'CANCELLED';
    effectiveStatus: 'OPEN' | 'CLOSED' | 'CANCELLED';
    grader: Grader;
    graderAvailable: boolean;
    availabilityReasons: string[];
    scheduledBookingCount: number;
    availableSlots: number;
    taskCounts: {
        scheduled: number;
        completed: number;
        absent: number;
        cancelled: number;
    };
    version: string;
};
export type Session = SessionView & {
    cancellation: {
        at: string;
        reason: string;
    } | null;
    hasBookingHistory: boolean;
    allowedActions: string[];
    disabledReasons: Record<string, string>;
};
export type Membership = {
    expectedVersions: {
        session: string;
        bookings: {
            id: number;
            version: string;
        }[];
        attempts: {
            id: number;
            version: string;
        }[];
        registrations: {
            id: number;
            version: string;
        }[];
        grader?: string;
    };
    expectedSet: {
        scheduledBookingIds: number[];
        bindings: {
            bookingId: number;
            attemptId: number;
            registrationId: number;
        }[];
        currentGraderId?: number;
    };
};
export type RosterRow = {
    bookingId: number;
    attemptId: number;
    registrationId: number;
    attemptNumber: number;
    bookedAt: string;
    bookingStatus: string;
    registrationStatus: string;
    student: {
        studentId: number;
        fullName: string;
        email: string;
        phone: string;
    };
    course: {
        id: number;
        code: string;
        name: string;
        status: string;
    };
    targetScore: string | null;
    targetGradedAt: string | null;
    currentBand: {
        id: number;
        code: string;
        minScore: string;
    } | null;
    invoiceStatus: string | null;
};
export type CancellationPreview = {
    sessionId: number;
    eligible: boolean;
    blockedReasons: string[];
    asOfUtc: string;
    effects: RosterRow[];
    manifest: Membership | null;
};
export type GraderPreview = {
    sessionId: number;
    oldGrader: Grader;
    newGrader: Grader;
    eligible: boolean;
    blockedReasons: string[];
    conflicts: {
        resource: 'ROOM' | 'GRADER';
        message: string;
    }[];
    asOfUtc: string;
    manifest: Membership | null;
};
const base = ['id', 'date', 'startTime', 'endTime', 'startAt', 'endAt', 'room', 'capacity', 'status', 'effectiveStatus', 'grader', 'graderAvailable', 'availabilityReasons', 'scheduledBookingCount', 'availableSlots', 'taskCounts', 'version'];
const nonnegative = (v: unknown) => Number.isSafeInteger(v) && (v as number) >= 0;
function grader(value: unknown) { const r = object(value, ['accountId', 'displayName']); requireWire(validId(r.accountId) && text(r.displayName)); }
function summary(r: Record<string, unknown>) { requireWire(validId(r.id) && typeof r.date === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(r.date) && typeof r.startTime === 'string' && /^\d{2}:\d{2}$/.test(r.startTime) && typeof r.endTime === 'string' && /^\d{2}:\d{2}$/.test(r.endTime) && instant(r.startAt) && instant(r.endAt) && text(r.room) && validId(r.capacity) && r.capacity <= 20 && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.status as string) && ['OPEN', 'CLOSED', 'CANCELLED'].includes(r.effectiveStatus as string) && typeof r.graderAvailable === 'boolean' && strings(r.availabilityReasons) && nonnegative(r.scheduledBookingCount) && nonnegative(r.availableSlots) && version(r.version)); grader(r.grader); const counts = object(r.taskCounts, ['scheduled', 'completed', 'absent', 'cancelled']); requireWire(Object.values(counts).every(nonnegative)); }
function decodeSession(v: unknown): Session { const r = object(v, [...base, 'cancellation', 'hasBookingHistory', 'allowedActions', 'disabledReasons']); summary(r); requireWire(typeof r.hasBookingHistory === 'boolean'); capabilities(r, ['EDIT_SCHEDULE', 'OPEN', 'CLOSE', 'CANCEL', 'CHANGE_GRADER', 'VIEW_ROSTER']); if (r.cancellation !== null) {
    const c = object(r.cancellation, ['at', 'reason']);
    requireWire(instant(c.at) && text(c.reason));
} return r as Session; }
function manifest(v: unknown, change: boolean) { if (v === null)
    return; const r = object(v, ['expectedVersions', 'expectedSet']); const versions = object(r.expectedVersions, ['session', 'bookings', 'attempts', 'registrations', ...change ? ['grader'] : []]); requireWire(version(versions.session) && (!change || version(versions.grader))); for (const key of ['bookings', 'attempts', 'registrations'])
    references(versions[key]); const set = object(r.expectedSet, ['scheduledBookingIds', 'bindings', ...change ? ['currentGraderId'] : []]); requireWire(Array.isArray(set.scheduledBookingIds) && set.scheduledBookingIds.every(validId) && new Set(set.scheduledBookingIds).size === set.scheduledBookingIds.length && Array.isArray(set.bindings) && (!change || validId(set.currentGraderId))); for (const item of set.bindings) {
    const b = object(item, ['bookingId', 'attemptId', 'registrationId']);
    requireWire(Object.values(b).every(validId));
} const ids = (set.bindings as Membership['expectedSet']['bindings']).map(x => x.bookingId); requireWire(new Set(ids).size === ids.length && [...ids].sort((a, b) => a - b).join(',') === [...set.scheduledBookingIds].sort((a, b) => a - b).join(',')); }
function roster(value: unknown): RosterRow { const r = object(value, ['bookingId', 'attemptId', 'registrationId', 'attemptNumber', 'bookedAt', 'bookingStatus', 'registrationStatus', 'student', 'course', 'targetScore', 'targetGradedAt', 'currentBand', 'invoiceStatus']); requireWire(validId(r.bookingId) && validId(r.attemptId) && validId(r.registrationId) && [1, 2].includes(r.attemptNumber as number) && instant(r.bookedAt) && ['SCHEDULED', 'COMPLETED', 'ABSENT', 'CANCELLED'].includes(r.bookingStatus as string) && text(r.registrationStatus)); const s = object(r.student, ['studentId', 'fullName', 'email', 'phone']); requireWire(validId(s.studentId) && text(s.fullName) && text(s.email) && text(s.phone)); course(r.course); if (r.currentBand !== null)
    band(r.currentBand); requireWire((r.targetScore === null || decimal(r.targetScore)) && (r.targetGradedAt === null || instant(r.targetGradedAt)) && (r.invoiceStatus === null || ['UNPAID', 'PAID', 'CANCELLED'].includes(r.invoiceStatus as string))); return r as RosterRow; }
export const searchSessions = (client: HttpClient, query: Record<string, string>, signal: AbortSignal) => client.request({ endpoint: 'sessionSearch', query, signal, slot: 'session-search', decode: v => page(v, x => { const r = object(x, base); summary(r); return r as SessionView; }) });
export const loadSession = (client: HttpClient, id: number, signal: AbortSignal) => client.request({ endpoint: 'sessionDetail', targetId: id, signal, slot: 'session-detail', decode: v => { const r = decodeSession(v); requireWire(r.id === id); return r; } });
export const sessionCommand = (client: HttpClient, endpoint: Endpoint, body: unknown, requestId: string, signal: AbortSignal, targetId?: number) => client.request({ endpoint, body, requestId, signal, targetId, slot: 'session-command', decode: v => { const r = object(v, ['session', 'operationId']); requireWire(validUuid(r.operationId)); const session = decodeSession(r.session); requireWire(targetId === undefined || session.id === targetId); return { session, operationId: r.operationId }; } });
export const options = (client: HttpClient, query: Record<string, string>, signal: AbortSignal) => client.request({ endpoint: 'sessionOptions', query, signal, slot: 'session-options', decode: v => { const r = object(v, ['currentGrader', 'items', 'page', 'pageSize', 'totalCount']); const decode = (x: unknown) => { const o = object(x, ['accountId', 'displayName', 'eligible', 'blockedReasons']); requireWire(validId(o.accountId) && text(o.displayName) && typeof o.eligible === 'boolean' && strings(o.blockedReasons)); return o as Option; }; const p = page({ items: r.items, page: r.page, pageSize: r.pageSize, totalCount: r.totalCount }, decode); requireWire(p.items.every(x => x.eligible)); return { currentGrader: r.currentGrader === null ? null : decode(r.currentGrader), ...p } as Options; } });
export const cancellation = (client: HttpClient, id: number, signal: AbortSignal) => client.request({ endpoint: 'sessionCancelPreview', targetId: id, signal, slot: 'session-preview', decode: v => { const r = object(v, ['sessionId', 'eligible', 'blockedReasons', 'asOfUtc', 'effects', 'manifest']); requireWire(r.sessionId === id && typeof r.eligible === 'boolean' && strings(r.blockedReasons) && instant(r.asOfUtc) && Array.isArray(r.effects)); r.effects.forEach(roster); manifest(r.manifest, false); requireWire(r.eligible === (r.manifest !== null)); return r as CancellationPreview; } });
export const graderPreview = (client: HttpClient, id: number, graderId: number, signal: AbortSignal) => client.request({ endpoint: 'sessionGraderPreview', targetId: id, body: { graderId }, signal, slot: 'session-preview', decode: v => { const r = object(v, ['sessionId', 'oldGrader', 'newGrader', 'eligible', 'blockedReasons', 'conflicts', 'asOfUtc', 'manifest']); requireWire(r.sessionId === id && typeof r.eligible === 'boolean' && strings(r.blockedReasons) && instant(r.asOfUtc) && Array.isArray(r.conflicts)); grader(r.oldGrader); grader(r.newGrader); requireWire((r.newGrader as Grader).accountId === graderId); for (const item of r.conflicts) {
        const c = object(item, ['resource', 'message']);
        requireWire(['ROOM', 'GRADER'].includes(c.resource as string) && text(c.message));
    } manifest(r.manifest, true); requireWire(r.eligible === (r.manifest !== null)); return r as GraderPreview; } });
export const searchRoster = (client: HttpClient, id: number, body: unknown, signal: AbortSignal) => client.request({ endpoint: 'sessionRoster', targetId: id, body, signal, slot: 'session-roster', decode: v => page(v, roster) });
export const sessionLookup = (client: HttpClient, action: string, requestId: string, signal: AbortSignal) => client.request({ endpoint: 'sessionResult', query: { action }, requestId, signal, slot: 'session-result', decode: v => completion(v, 'sessionId') });
