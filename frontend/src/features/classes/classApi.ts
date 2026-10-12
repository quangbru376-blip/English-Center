import type { Endpoint, HttpClient } from '../../lib/http';
import { capabilities, object, requireWire, text, version, instant, course, band, strings, references } from '../../lib/academicWire';
import { page } from '../../lib/m1Wire';
import { validId, validUuid } from '../administration/types';
export type ClassRole = 'AcademicAffairs' | 'Teacher' | 'Student';
export type Teacher = {
    accountId: number;
    displayName: string;
    status: string;
};
export type ProgramRef = {
    id: number;
    name: string;
    course: {
        id: number;
        code: string;
        name: string;
        status: string;
    };
    band: {
        id: number;
        code: string;
        minScore: string;
    };
    status: string;
    expectedLessonCount: number;
};
export type ClassSummary = {
    id: number;
    code: string;
    program: ProgramRef;
    teacher: Teacher;
    status: string;
    maxSize: number;
    placedCount: number;
    createdAt: string;
    version: string;
};
export type Binding = {
    id: number;
    classId: number;
    obligationKey: string;
    sessionType: string;
    isCurrent: boolean;
    replacesScheduleId: number | null;
    version?: string;
};
export type Manifest = {
    expectedVersions: Record<string, unknown>;
    expectedSet: Record<string, unknown>;
};
export type ClassDetail = ClassSummary & {
    prerequisites: Record<string, number | string | null>;
    cancellation: {
        at: string;
        reason: string;
    } | null;
    allowedActions: string[];
    disabledReasons: Record<string, string>;
    guardContext: {
        program: string;
        teacher: string;
        assignments: {
            id: number;
            registrationId: number;
        }[];
        schedules: Binding[];
    };
    assignment?: {
        classAssignmentId: number;
        registrationId: number;
        assignedAt: string;
    };
};
export type Candidate = {
    registrationId: number;
    student: {
        studentId: number;
        fullName: string;
        email: string;
        phone: string;
    };
    course: ProgramRef['course'];
    band: ProgramRef['band'];
    entryScore: string;
    registrationStatus: string;
    invoiceStatus: string;
    admittedAt: string;
    registrationVersion: string;
    bandAssignment?: {
        id: number;
        version: string;
    };
    invoice?: {
        id: number;
        status: string;
        version: string;
    };
    assignment?: null;
};
export type Preview = {
    class: ClassSummary;
    eligible: boolean;
    blockedReasons: string[];
    conflicts?: {
        kind: string;
        startAt: string;
        endAt: string;
        message: string;
    }[];
    manifest: Manifest | null;
    candidate?: Candidate | ScheduleCandidate;
    proposedTeacher?: Teacher;
    action?: string;
    effects?: string[];
    prerequisites?: Record<string, unknown>;
    progress?: Record<string, unknown>;
};
export type ScheduleCandidate = {
    type: 'LESSON' | 'FINAL_EXAM';
    date: string;
    startTime: string;
    endTime: string;
    room: string;
};
export type ScheduleDetail = {
    id: number;
    class: {
        id: number;
        code: string;
        status: string;
        program: ProgramRef;
        teacher: Teacher;
    };
    type: string;
    date: string;
    startTime: string;
    endTime: string;
    startAt: string;
    endAt: string;
    room: string;
    occurrenceState: string;
    isCurrent: boolean;
    obligationKey: string;
    replacesScheduleId: number | null;
    replacementScheduleId: number | null;
    currentScheduleId: number;
    version: string;
    held: unknown;
    notHeld: unknown;
    replacementChain: unknown[];
    dependencies: {
        hasAttendance: boolean;
        hasLinkedScore: boolean;
    };
    allowedActions: string[];
    disabledReasons: Record<string, string>;
};
export type ScopedScheduleDetail = Omit<ScheduleDetail, 'version' | 'held' | 'notHeld' | 'dependencies' | 'allowedActions' | 'disabledReasons' | 'class'> & { class: Omit<ScheduleDetail['class'], 'teacher'> & { teacher: Teacher | Pick<Teacher, 'displayName'> } };
export type ScheduleRead = ScheduleDetail | ScopedScheduleDetail;
export const classCodes = { CREATE: 'ACADEMIC.CLASS.CREATE', CAPACITY: 'ACADEMIC.CLASS.UPDATE_CAPACITY', TEACHER: 'ACADEMIC.CLASS.CHANGE_TEACHER', OPEN: 'ACADEMIC.CLASS.OPEN', CANCEL: 'ACADEMIC.CLASS.CANCEL', ASSIGN: 'ACADEMIC.CLASS.PLACE_REGISTRATION', SCHEDULE: 'ACADEMIC.SCHEDULE.CREATE' };
export type ClassAction = keyof typeof classCodes;
const summaryKeys = ['id', 'code', 'program', 'teacher', 'status', 'maxSize', 'placedCount', 'createdAt', 'version'];
const localDate = (v: unknown): v is string => typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v) && Number.isFinite(Date.parse(v)) && new Date(v).toISOString().slice(0,10) === v;
const minuteTime = (v: unknown): v is string => typeof v === 'string' && /^(?:[01]\d|2[0-3]):[0-5]\d$/.test(v);
const status = (s: unknown) => ['PLANNED', 'OPEN', 'ONGOING', 'COMPLETED', 'CANCELLED'].includes(s as string);
const count = (v: unknown) => Number.isInteger(v) && (v as number) >= 0;
function program(v: unknown) { const p = object(v, ['id', 'name', 'course', 'band', 'status', 'expectedLessonCount']); requireWire(validId(p.id) && text(p.name) && validId(p.expectedLessonCount) && ['DRAFT', 'ACTIVE', 'INACTIVE'].includes(p.status as string)); course(p.course); band(p.band); }
function teacher(v: unknown) { const p = object(v, ['accountId', 'displayName', 'status']); requireWire(validId(p.accountId) && text(p.displayName) && ['ACTIVE', 'LOCKED', 'DISABLED'].includes(p.status as string)); }
function base(r: Record<string, unknown>, role: ClassRole) { requireWire(validId(r.id) && text(r.code) && status(r.status)); program(r.program); if (role === 'Student') {
    requireWire(text(object(r.teacher, ['displayName']).displayName));
    const a = object(r.assignment, ['classAssignmentId', 'registrationId', 'assignedAt']);
    requireWire(validId(a.classAssignmentId) && validId(a.registrationId) && instant(a.assignedAt));
}
else {
    teacher(r.teacher);
    requireWire(validId(r.maxSize) && (r.maxSize as number) <= 15 && count(r.placedCount) && (r.placedCount as number) <= (r.maxSize as number) && instant(r.createdAt));
    if (role === 'AcademicAffairs')
        requireWire(version(r.version));
} }
const roleKeys = (role: ClassRole) => role === 'AcademicAffairs' ? summaryKeys : role === 'Teacher' ? summaryKeys.filter(k => k !== 'version') : ['id', 'code', 'program', 'teacher', 'status', 'assignment'];
export function decodeClass(v: unknown, role: ClassRole = 'AcademicAffairs', detail = false): ClassDetail {
    const keys = [...roleKeys(role)];
    if (detail)
        keys.push(...role === 'AcademicAffairs' ? ['prerequisites', 'cancellation', 'allowedActions', 'disabledReasons', 'guardContext'] : role === 'Teacher' ? ['prerequisites', 'cancellation'] : ['cancellation']);
    const r = object(v, keys);
    base(r, role);
    if (detail) {
        if (r.cancellation !== null) {
            const c = object(r.cancellation, ['at', 'reason']);
            requireWire(instant(c.at) && text(c.reason));
        }
        if (role !== 'Student')
            prerequisites(r.prerequisites);
        if (role === 'AcademicAffairs') {
            capabilities(r, ['CREATE_SCHEDULE', 'EDIT_CAPACITY', 'CHANGE_TEACHER', 'PREVIEW_TRANSITION', 'OPEN', 'CANCEL', 'VIEW_ROSTER', 'PLACE_REGISTRATION']);
            const g = object(r.guardContext, ['program', 'teacher', 'assignments', 'schedules']);
            requireWire(version(g.program) && version(g.teacher));
            assignments(g.assignments);
            bindings(g.schedules, true);
            requireWire((g.schedules as {
                classId: number;
            }[]).every(x => x.classId === r.id));
        }
    }
    return r as unknown as ClassDetail;
}
function prerequisites(v: unknown) { const r = object(v, ['expectedLessonCount', 'currentLessonCount', 'heldLessonCount', 'currentFinalExamCount', 'heldFinalExamCount', 'markedLessonAttendanceCount', 'requiredLessonAttendanceCount', 'firstLessonStartAt']); requireWire(validId(r.expectedLessonCount) && Object.entries(r).filter(([k]) => k !== 'firstLessonStartAt').every(([, v]) => count(v)) && (r.firstLessonStartAt === null || instant(r.firstLessonStartAt))); }
function assignments(v: unknown) { requireWire(Array.isArray(v)); const ids = new Set<number>(); for (const x of v) {
    const r = object(x, ['id', 'registrationId']);
    requireWire(validId(r.id) && validId(r.registrationId) && !ids.has(r.id));
    ids.add(r.id);
} }
function bindings(v: unknown, rv = false) { requireWire(Array.isArray(v)); const ids = new Set<number>(); for (const x of v) {
    const r = object(x, ['id', 'classId', 'obligationKey', 'sessionType', 'isCurrent', 'replacesScheduleId', ...rv ? ['version'] : []]);
    requireWire(validId(r.id) && validId(r.classId) && validUuid(r.obligationKey) && ['LESSON', 'FINAL_EXAM'].includes(r.sessionType as string) && typeof r.isCurrent === 'boolean' && (r.replacesScheduleId === null || validId(r.replacesScheduleId)) && (!rv || version(r.version)) && !ids.has(r.id));
    ids.add(r.id);
} }
function ids(v: unknown) { requireWire(Array.isArray(v) && v.every(validId) && new Set(v).size === v.length); }
function timetable(v: unknown) { const t = object(v, ['bookings', 'assignments', 'schedules']); for (const key of ['bookings', 'assignments', 'schedules']) {
    requireWire(Array.isArray(t[key]));
    for (const x of t[key]) {
        const keys = key === 'bookings' ? ['bookingId', 'attemptId', 'sessionId', 'bookingVersion', 'sessionVersion'] : key === 'assignments' ? ['classAssignmentId', 'classId', 'classVersion'] : ['scheduleId', 'classId', 'version'];
        const r = object(x, keys);
        requireWire(Object.entries(r).every(([k, v]) => k.toLowerCase().endsWith('version') ? version(v) : validId(v)));
    }
} }
function sameIds(left: unknown, right: unknown, leftKey: string, rightKey?: string) {
    requireWire(Array.isArray(left) && Array.isArray(right));
    const a = left.map(x => (x as Record<string, unknown>)[leftKey]);
    const b = right.map(x => rightKey ? (x as Record<string, unknown>)[rightKey] : x);
    requireWire(a.length === b.length && new Set(a).size === a.length && new Set(b).size === b.length && a.every(x => b.includes(x)));
}
export function decodeManifest(v: unknown, kind: 'teacher' | 'transition' | 'placement' | 'schedule', action?: string): Manifest {
    const m = object(v, ['expectedVersions', 'expectedSet']);
    const rvKeys = kind === 'teacher' ? ['class', 'program', 'currentTeacher', 'proposedTeacher', 'schedules'] : kind === 'placement' ? ['registration', 'bandAssignment', 'invoice', 'class', 'program', 'teacher', 'classSchedules'] : ['class', 'program', 'teacher', 'schedules'];
    const setKeys = kind === 'placement' ? ['bandAssignmentId', 'invoiceId', 'registrationAssignmentId', 'classAssignmentIds', 'classScheduleBindings', 'studentTimetable'] : ['assignmentIds', 'scheduleBindings', ...kind === 'teacher' || action === 'CANCEL' ? ['hasAttendance', 'hasScores'] : [], ...kind === 'schedule' ? ['resources', 'cohortTimetables'] : []];
    const rv = object(m.expectedVersions, rvKeys), s = object(m.expectedSet, setKeys);
    for (const [k, x] of Object.entries(rv))
        if (k === 'schedules' || k === 'classSchedules')
            references(x);
        else
            requireWire(version(x));
    ids(s[kind === 'placement' ? 'classAssignmentIds' : 'assignmentIds']);
    bindings(s[kind === 'placement' ? 'classScheduleBindings' : 'scheduleBindings']);
    sameIds(rv[kind === 'placement' ? 'classSchedules' : 'schedules'], s[kind === 'placement' ? 'classScheduleBindings' : 'scheduleBindings'], 'id', 'id');
    if (kind === 'teacher' || action === 'CANCEL')
        requireWire(s.hasAttendance === false && s.hasScores === false);
    if (kind === 'placement') {
        requireWire(validId(s.bandAssignmentId) && validId(s.invoiceId) && s.registrationAssignmentId === null);
        timetable(s.studentTimetable);
    }
    if (kind === 'schedule') {
        const r = object(s.resources, ['classSchedules', 'entrySessions']);
        requireWire(Array.isArray(r.classSchedules) && Array.isArray(r.entrySessions) && Array.isArray(s.cohortTimetables));
        for (const x of r.classSchedules) {
            const a = object(x, ['scheduleId', 'classId', 'scheduleVersion', 'classVersion']);
            requireWire(validId(a.scheduleId) && validId(a.classId) && version(a.scheduleVersion) && version(a.classVersion));
        }
        for (const x of r.entrySessions) {
            const a = object(x, ['sessionId', 'version']);
            requireWire(validId(a.sessionId) && version(a.version));
        }
        for (const x of s.cohortTimetables) {
            const a = object(x, ['classAssignmentId', 'timetable']);
            requireWire(validId(a.classAssignmentId));
            timetable(a.timetable);
        }
        sameIds(s.cohortTimetables, s.assignmentIds, 'classAssignmentId');
    }
    return m as unknown as Manifest;
}
export function decodeCandidate(v: unknown, detail = false): Candidate { const r = object(v, ['registrationId', 'student', 'course', 'band', 'entryScore', 'registrationStatus', 'invoiceStatus', 'admittedAt', 'registrationVersion', ...detail ? ['bandAssignment', 'invoice', 'assignment'] : []]); const s = object(r.student, ['studentId', 'fullName', 'email', 'phone']); requireWire(validId(r.registrationId) && validId(s.studentId) && text(s.fullName) && text(s.email) && text(s.phone) && typeof r.entryScore === 'string' && /^\d+\.\d{2}$/.test(r.entryScore) && r.registrationStatus === 'ADMITTED' && r.invoiceStatus === 'PAID' && instant(r.admittedAt) && version(r.registrationVersion)); course(r.course); band(r.band); if (detail) {
    const b = object(r.bandAssignment, ['id', 'version']), i = object(r.invoice, ['id', 'status', 'version']);
    requireWire(validId(b.id) && version(b.version) && validId(i.id) && i.status === 'PAID' && version(i.version) && r.assignment === null);
} return r as unknown as Candidate; }
export function decodePreview(v: unknown, kind: 'teacher' | 'transition' | 'placement' | 'schedule'): Preview {
    const r = object(v, ['class', 'eligible', 'blockedReasons', 'manifest', ...kind === 'transition' ? ['action', 'prerequisites', 'effects'] : kind === 'teacher' ? ['proposedTeacher', 'conflicts'] : kind === 'placement' ? ['candidate', 'conflicts'] : ['candidate', 'progress', 'conflicts']]);
    decodeClass(r.class);
    requireWire(typeof r.eligible === 'boolean' && strings(r.blockedReasons) && r.eligible === (r.manifest !== null));
    if (kind === 'placement')
        decodeCandidate(r.candidate, true);
    if (kind === 'teacher')
        teacher(r.proposedTeacher);
    if (kind === 'transition') {
        requireWire(['OPEN', 'START', 'COMPLETE', 'CANCEL'].includes(r.action as string) && strings(r.effects));
        prerequisites(r.prerequisites);
    }
    else {
        requireWire(Array.isArray(r.conflicts));
        for (const x of r.conflicts) {
            const c = object(x, ['kind', 'startAt', 'endAt', 'message']);
            requireWire(['CLASS', 'ROOM', 'TEACHER', 'STUDENT'].includes(c.kind as string) && instant(c.startAt) && instant(c.endAt) && text(c.message));
        }
    }
    if (r.manifest) {
        const m = decodeManifest(r.manifest, kind, r.action as string);
        const set = m.expectedSet;
        requireWire((set[kind === 'placement' ? 'classScheduleBindings' : 'scheduleBindings'] as {
            classId: number;
        }[]).every(x => x.classId === (r.class as ClassSummary).id));
        if (kind === 'placement') {
            const candidate = r.candidate as Candidate;
            requireWire(set.bandAssignmentId === candidate.bandAssignment!.id && set.invoiceId === candidate.invoice!.id && m.expectedVersions.registration === candidate.registrationVersion && m.expectedVersions.bandAssignment === candidate.bandAssignment!.version && m.expectedVersions.invoice === candidate.invoice!.version);
        }
    }
    if (kind === 'schedule') {
        scheduleCandidate(r.candidate);
        progress(r.progress);
    }
    return r as unknown as Preview;
}
export function progress(v: unknown) { const r = object(v, ['expectedLessonCount', 'lessonObligationCount', 'heldLessonObligationCount', 'remainingLessonObligationCount', 'finalExamScheduleId', 'heldFinalExamCount']); requireWire(validId(r.expectedLessonCount) && Object.entries(r).filter(([k]) => k !== 'finalExamScheduleId').every(([, v]) => count(v)) && (r.finalExamScheduleId === null || validId(r.finalExamScheduleId))); }
export function scheduleCandidate(v: unknown) { const r = object(v, ['type', 'date', 'startTime', 'endTime', 'room']); requireWire(['LESSON', 'FINAL_EXAM'].includes(r.type as string) && localDate(r.date) && minuteTime(r.startTime) && minuteTime(r.endTime) && r.startTime < r.endTime && typeof r.room === 'string' && /^[A-Z0-9_-]{1,30}$/.test(r.room)); }
function provenance(value: unknown, keys: string[]) { if (value === null)
    return; const r = object(value, keys); requireWire(validId(r.actorAccountId) && instant(r.at) && (!keys.includes('reason') || text(r.reason))); }
export function decodeSchedule(v: unknown, detail?: boolean, role?: 'AcademicAffairs'): ScheduleDetail;
export function decodeSchedule(v: unknown, detail: boolean, role: ClassRole): ScheduleRead;
export function decodeSchedule(v: unknown, detail = true, role: ClassRole = 'AcademicAffairs'): ScheduleRead { const r = object(v, ['id', 'class', 'type', 'date', 'startTime', 'endTime', 'startAt', 'endAt', 'room', 'occurrenceState', 'isCurrent', 'obligationKey', 'replacesScheduleId', 'replacementScheduleId', 'currentScheduleId', ...role === 'AcademicAffairs' ? ['version'] : [], ...detail ? role === 'AcademicAffairs' ? ['held', 'notHeld', 'replacementChain', 'dependencies', 'allowedActions', 'disabledReasons'] : ['replacementChain'] : []]); const c = object(r.class, ['id', 'code', 'status', 'program', 'teacher']); requireWire(validId(c.id) && text(c.code) && status(c.status)); program(c.program); if(role === 'Student') requireWire(text(object(c.teacher,['displayName']).displayName)); else teacher(c.teacher); scheduleCandidate(Object.fromEntries(['type', 'date', 'startTime', 'endTime', 'room'].map(k => [k, r[k]]))); requireWire(validId(r.id) && instant(r.startAt) && instant(r.endAt) && (role !== 'AcademicAffairs' || version(r.version)) && validUuid(r.obligationKey) && typeof r.isCurrent === 'boolean' && ['PLANNED', 'HELD', 'NOT_HELD'].includes(r.occurrenceState as string) && validId(r.currentScheduleId) && [r.replacesScheduleId, r.replacementScheduleId].every(v => v === null || validId(v))); requireWire(r.startAt === new Date(`${r.date}T${r.startTime}:00+07:00`).toISOString() && r.endAt === new Date(`${r.date}T${r.endTime}:00+07:00`).toISOString() && (!r.isCurrent || r.id === r.currentScheduleId) && (r.occurrenceState !== 'NOT_HELD' || r.isCurrent === false)); if (detail) {
    if(role === 'AcademicAffairs') {
    const d = object(r.dependencies, ['hasAttendance', 'hasLinkedScore']);
    requireWire(typeof d.hasAttendance === 'boolean' && typeof d.hasLinkedScore === 'boolean' && Array.isArray(r.replacementChain));
    capabilities(r, ['VIEW_CLASS','MOVE','REPLACE']);
    provenance(r.held, ['actorAccountId', 'at']);
    provenance(r.notHeld, ['actorAccountId', 'at', 'reason']);
    }
    requireWire(Array.isArray(r.replacementChain));
    let previous: number | null = null;
    const chainIds = new Set<number>();
    for (const value of r.replacementChain) {
        const link = object(value, ['scheduleId', 'replacesScheduleId', 'date', 'startTime', 'endTime', 'occurrenceState', 'isCurrent']);
        requireWire(validId(link.scheduleId) && !chainIds.has(link.scheduleId) && link.replacesScheduleId === previous && typeof link.isCurrent === 'boolean' && ['PLANNED', 'HELD', 'NOT_HELD'].includes(link.occurrenceState as string));
        scheduleCandidate({ type: r.type, date: link.date, startTime: link.startTime, endTime: link.endTime, room: r.room });
        chainIds.add(link.scheduleId);
        previous = link.scheduleId;
    }
    requireWire(chainIds.has(r.id as number) && previous === r.currentScheduleId);
    const chain = r.replacementChain as {scheduleId:number;replacesScheduleId:number|null;isCurrent:boolean;occurrenceState:string}[];
    requireWire(chain.filter(x=>x.isCurrent).length === 1 && chain.at(-1)!.isCurrent && chain.slice(0,-1).every(x=>!x.isCurrent && x.occurrenceState === 'NOT_HELD'));
    const index = chain.findIndex(x=>x.scheduleId === r.id);requireWire(chain[index].replacesScheduleId === r.replacesScheduleId && (chain[index+1]?.scheduleId ?? null) === r.replacementScheduleId);
} return r as unknown as ScheduleRead; }
const root = (role: ClassRole) => role === 'AcademicAffairs' ? 'class' : role === 'Teacher' ? 'teacherClass' : 'studentClass';
export const classList = (c: HttpClient, role: ClassRole, query: Record<string, string>, signal: AbortSignal) => c.request({ endpoint: role === 'AcademicAffairs' ? 'classSearch' : role === 'Teacher' ? 'teacherClasses' : 'studentClasses', query, signal, slot: 'classes-list', decode: v => page(v, x => decodeClass(x, role)) });
export const classDetail = (c: HttpClient, role: ClassRole, id: number, signal: AbortSignal) => c.request({ endpoint: (root(role) + (role === 'AcademicAffairs' ? 'Detail' : '')) as Endpoint, targetId: id, signal, slot: 'class-detail', decode: v => { const r = decodeClass(v, role, true); requireWire(r.id === id); return r; } });
export const classPreview = (c: HttpClient, id: number, kind: 'teacher' | 'transition' | 'placement' | 'schedule', body: unknown, signal: AbortSignal) => c.request({ endpoint: kind === 'teacher' ? 'classTeacherPreview' : kind === 'transition' ? 'classTransition' : kind === 'placement' ? 'classPlacement' : 'classSchedulePreview', targetId: id, body, signal, slot: 'class-preview', decode: v => { const r = decodePreview(v, kind); requireWire(r.class.id === id); const input = body as {
        teacherId?: number;
        registrationId?: number;
        action?: string;
        candidate?: ScheduleCandidate;
    }; if (kind === 'teacher')
        requireWire(r.proposedTeacher!.accountId === input.teacherId); if (kind === 'placement')
        requireWire((r.candidate as Candidate).registrationId === input.registrationId); if (kind === 'transition')
        requireWire(r.action === input.action); if (kind === 'schedule') {
        const candidate = r.candidate as ScheduleCandidate;
        requireWire(['type', 'date', 'startTime', 'endTime', 'room'].every(k => candidate[k as keyof ScheduleCandidate] === input.candidate?.[k as keyof ScheduleCandidate]));
    } return r; } });
const commandEndpoints: Record<ClassAction, Endpoint> = { CREATE: 'classCreate', CAPACITY: 'classCapacity', TEACHER: 'classTeacher', OPEN: 'classOpen', CANCEL: 'classCancel', ASSIGN: 'classAssign', SCHEDULE: 'classSchedule' };
export const classCommand = (c: HttpClient, action: ClassAction, id: number | undefined, body: unknown, key: string, signal: AbortSignal) => c.request({ endpoint: commandEndpoints[action], targetId: id, body, requestId: key, signal, slot: 'class-command', decode: v => { const r = object(v, action === 'SCHEDULE' ? ['schedule', 'operationId'] : action === 'ASSIGN' ? ['assignment', 'class', 'operationId'] : ['class', 'operationId']); requireWire(validUuid(r.operationId)); if (action === 'SCHEDULE') {
        const s = decodeSchedule(r.schedule);
        requireWire(s.class.id === id);
        return { classId: s.class.id, scheduleId: s.id };
    } const cl = decodeClass(r.class, 'AcademicAffairs', action !== 'ASSIGN'); requireWire(id === undefined || cl.id === id); if (action === 'ASSIGN') {
        const a = object(r.assignment, ['classAssignmentId', 'classId', 'registrationId', 'assignedAt']);
        requireWire(validId(a.classAssignmentId) && a.classId === id && a.registrationId === (body as {
            registrationId: number;
        }).registrationId && instant(a.assignedAt));
    } return { classId: cl.id, scheduleId: undefined }; } });
export const classLookup = (c: HttpClient, action: ClassAction, key: string, id: number | undefined, signal: AbortSignal) => c.request({ endpoint: 'classResult', query: { action: classCodes[action] }, requestId: key, signal, slot: 'class-lookup', decode: v => { const r = object(v, ['outcome', 'operationId', 'resultMode', 'target']), target = object(r.target, action === 'ASSIGN' ? ['classId', 'registrationId', 'classAssignmentId'] : action === 'SCHEDULE' ? ['classId', 'scheduleId'] : ['classId']); requireWire(r.outcome === 'COMPLETED' && validUuid(r.operationId) && r.resultMode === 'SAFE_REFERENCE' && Object.values(target).every(validId)); const result = { ...r, target: target as Record<string, number> }; requireWire(id === undefined || result.target.classId === id); return result; } });
export function decodeScheduling(value: unknown, classId: number) {
    const r = object(value, ['class', 'progress', 'schedules', 'currentObligations', 'assignments', 'expectedVersions']);
    requireWire(decodeClass(r.class).id === classId);
    progress(r.progress);
    bindings(r.schedules, true);
    bindings(r.currentObligations, true);
    assignments(r.assignments);
    const versions = object(r.expectedVersions, ['class', 'program', 'teacher', 'schedules']);
    requireWire(version(versions.class) && version(versions.program) && version(versions.teacher));
    references(versions.schedules);
    sameIds(versions.schedules, r.schedules, 'id', 'id');
    const schedules = r.schedules as {
        id: number;
        classId: number;
        isCurrent: boolean;
        version: string;
    }[];
    const current = r.currentObligations as typeof schedules;
    requireWire(schedules.every(x => x.classId === classId) && current.length === schedules.filter(x => x.isCurrent).length && current.every(x => schedules.some(s => s.id === x.id && s.isCurrent && s.version === x.version)));
    return r as unknown as {
        assignments: unknown[];
        schedules: unknown[];
        currentObligations: unknown[];
    };
}
