import { decodeApiError, errorAction } from './apiError';
import type { ApiError, ErrorAction } from './apiError';
import { AuthState } from './auth';

const endpoints = {
  gradebook:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  gradeDraft:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  gradeSave:{path:'/api/v1/academics/teachers/me/classes',method:'PUT',intent:'write',access:'teacher'},
  gradeOptions:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  gradePreview:{path:'/api/v1/academics/teachers/me/classes',method:'POST',intent:'read',access:'teacher'},
  gradeSubmit:{path:'/api/v1/academics/teachers/me/classes',method:'POST',intent:'write',access:'teacher'},
  gradeResubmit:{path:'/api/v1/academics/teachers/me/classes',method:'POST',intent:'write',access:'teacher'},
  gradeResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'teacher'},
  gradeReviews:{path:'/api/v1/academics/grade-reviews',method:'GET',intent:'read',access:'academic'},
  gradeReview:{path:'/api/v1/academics/grade-reviews/classes',method:'GET',intent:'read',access:'academic'},
  gradeReturnPreview:{path:'/api/v1/academics/grade-reviews/classes',method:'POST',intent:'read',access:'academic'},
  gradeReturn:{path:'/api/v1/academics/grade-reviews/classes',method:'POST',intent:'write',access:'academic'},
  gradeReviewResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'academic'},

  attendanceSummary:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  attendanceSheet:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  attendanceSave:{path:'/api/v1/academics/teachers/me/classes',method:'PUT',intent:'write',access:'teacher'},
  academicAttendanceSummary:{path:'/api/v1/academics/classes',method:'GET',intent:'read',access:'academic'},
  academicAttendanceSheet:{path:'/api/v1/academics/classes',method:'GET',intent:'read',access:'academic'},
  attendanceResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'teacher'},
  heldPreview:{path:'/api/v1/academics/schedules',method:'POST',intent:'read',access:'academic'},
  heldConfirm:{path:'/api/v1/academics/schedules',method:'POST',intent:'write',access:'academic'},
  classStart:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classComplete:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  weeklyPreview:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  weeklyCreate:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  scheduleMovePreview:{path:'/api/v1/academics/schedules',method:'POST',intent:'read',access:'academic'},
  scheduleMove:{path:'/api/v1/academics/schedules',method:'PUT',intent:'write',access:'academic'},
  scheduleReplacePreview:{path:'/api/v1/academics/schedules',method:'POST',intent:'read',access:'academic'},
  scheduleReplace:{path:'/api/v1/academics/schedules',method:'POST',intent:'write',access:'academic'},
  teacherSchedules:{path:'/api/v1/academics/teachers/me/schedules',method:'GET',intent:'read',access:'teacher'},
  teacherSchedule:{path:'/api/v1/academics/teachers/me/schedules',method:'GET',intent:'read',access:'teacher'},
  studentSchedules:{path:'/api/v1/students/me/schedules',method:'GET',intent:'read',access:'student'},
  studentSchedule:{path:'/api/v1/students/me/schedules',method:'GET',intent:'read',access:'student'},
  classSearch:{path:'/api/v1/academics/classes',method:'GET',intent:'read',access:'academic'},
  classDetail:{path:'/api/v1/academics/classes',method:'GET',intent:'read',access:'academic'},
  classCreate:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classCapacity:{path:'/api/v1/academics/classes',method:'PUT',intent:'write',access:'academic'},
  classTeacherPreview:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  classTeacher:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classTransition:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  classOpen:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classCancel:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classRoster:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  classTeachers:{path:'/api/v1/academics/class-teacher-options',method:'GET',intent:'read',access:'academic'},
  classCandidates:{path:'/api/v1/academics/placement-candidates/search',method:'POST',intent:'read',access:'academic'},
  classCandidate:{path:'/api/v1/academics/placement-candidates',method:'GET',intent:'read',access:'academic'},
  classOptions:{path:'/api/v1/academics/placement-candidates',method:'GET',intent:'read',access:'academic'},
  classPlacement:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  classAssign:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classScheduling:{path:'/api/v1/academics/classes',method:'GET',intent:'read',access:'academic'},
  classSchedulePreview:{path:'/api/v1/academics/classes',method:'POST',intent:'read',access:'academic'},
  classSchedule:{path:'/api/v1/academics/classes',method:'POST',intent:'write',access:'academic'},
  classResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'academic'},
  academicSchedules:{path:'/api/v1/academics/schedules',method:'GET',intent:'read',access:'academic'},
  academicSchedule:{path:'/api/v1/academics/schedules',method:'GET',intent:'read',access:'academic'},
  teacherClasses:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  teacherClass:{path:'/api/v1/academics/teachers/me/classes',method:'GET',intent:'read',access:'teacher'},
  teacherClassRoster:{path:'/api/v1/academics/teachers/me/classes',method:'POST',intent:'read',access:'teacher'},
  studentClasses:{path:'/api/v1/students/me/classes',method:'GET',intent:'read',access:'student'},
  studentClass:{path:'/api/v1/students/me/classes',method:'GET',intent:'read',access:'student'},
  csrf: { path: '/api/v1/auth/csrf', method: 'GET', intent: 'read', access: 'public' },
  me: { path: '/api/v1/auth/me', method: 'GET', intent: 'read', access: 'public' },
  register: { path: '/api/v1/auth/register', method: 'POST', intent: 'write', access: 'public' },
  login: { path: '/api/v1/auth/login', method: 'POST', intent: 'write', access: 'public' },
  logout: { path: '/api/v1/auth/logout', method: 'POST', intent: 'write', access: 'session' },
  password: { path: '/api/v1/auth/change-password', method: 'POST', intent: 'write', access: 'session' },
  adminDetail: { path: '/api/v1/admin/accounts', method: 'GET', intent: 'read', access: 'admin' },
  adminLock: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminLockResult: { path: '/api/v1/admin/command-results', method: 'GET', intent: 'read', access: 'admin' },
  adminSearch: { path: '/api/v1/admin/accounts/search', method: 'POST', intent: 'read', access: 'admin' },
  staffCreate: { path: '/api/v1/admin/staff-accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminReset: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminUnlock: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminDisable: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminRestore: { path: '/api/v1/admin/accounts', method: 'POST', intent: 'write', access: 'admin' },
  adminResult: { path: '/api/v1/admin/command-results', method: 'GET', intent: 'read', access: 'admin' },
  courseSearch: { path: '/api/v1/academics/courses', method: 'GET', intent: 'read', access: 'academic' },
  courseDetail: { path: '/api/v1/academics/courses', method: 'GET', intent: 'read', access: 'academic' },
  courseCreate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  courseUpdate: { path: '/api/v1/academics/courses', method: 'PUT', intent: 'write', access: 'academic' },
  courseDeactivate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  courseActivate: { path: '/api/v1/academics/courses', method: 'POST', intent: 'write', access: 'academic' },
  bands: { path: '/api/v1/academics/bands', method: 'GET', intent: 'read', access: 'academic' },
  courseResult: { path: '/api/v1/academics/command-results', method: 'GET', intent: 'read', access: 'academic' },
  programSearch:{path:'/api/v1/academics/training-programs',method:'GET',intent:'read',access:'academic'},
  programDetail:{path:'/api/v1/academics/training-programs',method:'GET',intent:'read',access:'academic'},
  programCreate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programEdit:{path:'/api/v1/academics/training-programs',method:'PUT',intent:'write',access:'academic'},
  programRename:{path:'/api/v1/academics/training-programs',method:'PUT',intent:'write',access:'academic'},
  programPreview:{path:'/api/v1/academics/training-programs',method:'POST',intent:'read',access:'academic'},
  programActivate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programDraft:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programDeactivate:{path:'/api/v1/academics/training-programs',method:'POST',intent:'write',access:'academic'},
  programResult:{path:'/api/v1/academics/command-results',method:'GET',intent:'read',access:'academic'},
  sessionSearch:{path:'/api/v1/admissions/entry-test-sessions',method:'GET',intent:'read',access:'academic'},
  sessionDetail:{path:'/api/v1/admissions/entry-test-sessions',method:'GET',intent:'read',access:'academic'},
  sessionCreate:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionSchedule:{path:'/api/v1/admissions/entry-test-sessions',method:'PUT',intent:'write',access:'academic'},
  sessionOpen:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionClose:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionCancelPreview:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionCancel:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionGraderPreview:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionGrader:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'write',access:'academic'},
  sessionRoster:{path:'/api/v1/admissions/entry-test-sessions',method:'POST',intent:'read',access:'academic'},
  sessionOptions:{path:'/api/v1/admissions/entry-test-grader-options',method:'GET',intent:'read',access:'academic'},
  sessionResult:{path:'/api/v1/admissions/command-results',method:'GET',intent:'read',access:'academic'},
  studentProfile:{path:'/api/v1/students/me/profile',method:'GET',intent:'read',access:'student'},
  studentProfileUpdate:{path:'/api/v1/students/me/profile',method:'PUT',intent:'write',access:'student'},
  studentHome:{path:'/api/v1/students/me/home',method:'GET',intent:'read',access:'student'},
  studentCourses:{path:'/api/v1/students/me/courses',method:'GET',intent:'read',access:'student'},
  studentCourse:{path:'/api/v1/students/me/courses',method:'GET',intent:'read',access:'student'},
  studentRegistrations:{path:'/api/v1/students/me/registrations',method:'GET',intent:'read',access:'student'},
  studentRegistration:{path:'/api/v1/students/me/registrations',method:'GET',intent:'read',access:'student'},
  studentCreate:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentCancelPreview:{path:'/api/v1/students/me/registrations',method:'POST',intent:'read',access:'student'},
  studentCancel:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentOptions:{path:'/api/v1/students/me/registrations',method:'GET',intent:'read',access:'student'},
  studentBook:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentCancelBooking:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentReschedule:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentCalendar:{path:'/api/v1/students/me/entry-test-bookings',method:'GET',intent:'read',access:'student'},
  studentResults:{path:'/api/v1/students/me/registrations',method:'GET',intent:'read',access:'student'},
  studentConfirmPreview:{path:'/api/v1/students/me/registrations',method:'POST',intent:'read',access:'student'},
  studentConfirm:{path:'/api/v1/students/me/registrations',method:'POST',intent:'write',access:'student'},
  studentResult:{path:'/api/v1/students/me/command-results',method:'GET',intent:'read',access:'student'},
  teacherSessions:{path:'/api/v1/admissions/teachers/me/entry-test-sessions',method:'GET',intent:'read',access:'teacher'},
  teacherSession:{path:'/api/v1/admissions/teachers/me/entry-test-sessions',method:'GET',intent:'read',access:'teacher'},
  teacherRoster:{path:'/api/v1/admissions/teachers/me/entry-test-sessions',method:'POST',intent:'read',access:'teacher'},
  teacherTask:{path:'/api/v1/admissions/teachers/me/entry-test-bookings',method:'GET',intent:'read',access:'teacher'},
  teacherGrade:{path:'/api/v1/admissions/teachers/me/entry-test-bookings',method:'POST',intent:'write',access:'teacher'},
  teacherAbsence:{path:'/api/v1/admissions/teachers/me/entry-test-bookings',method:'POST',intent:'write',access:'teacher'},
  teacherCorrect:{path:'/api/v1/admissions/teachers/me/entry-test-attempts',method:'PUT',intent:'write',access:'teacher'},
  teacherResult:{path:'/api/v1/admissions/teachers/me/command-results',method:'GET',intent:'read',access:'teacher'},
  admissionsSearch:{path:'/api/v1/admissions/registrations/search',method:'POST',intent:'read',access:'admissions'},
  admissionsDetail:{path:'/api/v1/admissions/registrations',method:'GET',intent:'read',access:'admissions'},
  admissionsApproval:{path:'/api/v1/admissions/registrations',method:'POST',intent:'read',access:'admissions'},
  admissionsApprove:{path:'/api/v1/admissions/registrations',method:'POST',intent:'write',access:'admissions'},
  admissionsReject:{path:'/api/v1/admissions/registrations',method:'POST',intent:'write',access:'admissions'},
  admissionsCancellation:{path:'/api/v1/admissions/registrations',method:'POST',intent:'read',access:'admissions'},
  admissionsCancel:{path:'/api/v1/admissions/registrations',method:'POST',intent:'write',access:'admissions'},
  admissionsResult:{path:'/api/v1/admissions/command-results',method:'GET',intent:'read',access:'admissions'},
  studentInvoices:{path:'/api/v1/students/me/invoices',method:'GET',intent:'read',access:'student'},
  studentInvoice:{path:'/api/v1/students/me/invoices',method:'GET',intent:'read',access:'student'},
  financeSearch:{path:'/api/v1/finance/invoices/search',method:'POST',intent:'read',access:'accountant'},
  financeDetail:{path:'/api/v1/finance/invoices',method:'GET',intent:'read',access:'accountant'},
  financePreview:{path:'/api/v1/finance/invoices',method:'POST',intent:'read',access:'accountant'},
  financeCollect:{path:'/api/v1/finance/invoices',method:'POST',intent:'write',access:'accountant'},
  financeCorrect:{path:'/api/v1/finance/payments',method:'PUT',intent:'write',access:'accountant'},
  financeResult:{path:'/api/v1/finance/command-results',method:'GET',intent:'read',access:'accountant'},
} as const;
export type Endpoint = keyof typeof endpoints;
export type HttpResult<T> =
  | Readonly<{ kind: 'success'; value: T }>
  | Readonly<{ kind: 'error'; status: number; error: ApiError; action: ErrorAction; retryAfter: string | null; outcome: 'rejected' | 'unknown' }>
  | Readonly<{ kind: 'discarded' | 'unavailable' | 'unknown' | 'blocked' }>;
export type RequestOptions<T> = {
  endpoint: Endpoint; slot: string; decode: (value: unknown) => T;
  body?: unknown; signal?: AbortSignal;
  targetId?: number; requestId?: string;
  registrationId?: number;
  scheduleId?: number;
  assignmentId?:number;
  componentId?:number;
  query?: Readonly<Record<string,string>>;
};

// No automatic dispatch, retry, UUID generation, storage, token parsing or logging.
export class HttpClient {
  constructor(readonly auth: AuthState, private readonly fetcher: typeof fetch = (input, init) => globalThis.fetch(input, init)) {}
  async request<T>(options: RequestOptions<T>): Promise<HttpResult<T>> {
    if (!Object.hasOwn(endpoints, options.endpoint)) return { kind: 'blocked' };
    const endpoint = endpoints[options.endpoint];
    if (!endpoint || (endpoint.intent === 'read' && !['gradePreview','gradeReturnPreview','weeklyPreview','scheduleMovePreview','scheduleReplacePreview','classTeacherPreview','classTransition','classRoster','teacherClassRoster','classCandidates','classPlacement','classSchedulePreview','financeSearch','admissionsSearch','adminSearch','sessionRoster','sessionGraderPreview','teacherRoster'].includes(options.endpoint) && options.body !== undefined)) return { kind: 'blocked' };
    if (endpoint.access === 'session' && !this.auth.context) return { kind: 'blocked' };
    if (endpoint.access === 'admin' && (this.auth.context?.role !== 'Admin' || this.auth.context.scope !== 'NORMAL')) return { kind: 'blocked' };
    if (endpoint.access === 'academic' && (this.auth.context?.role !== 'AcademicAffairs' || this.auth.context.scope !== 'NORMAL')) return { kind:'blocked' };
    if (endpoint.access === 'student' && (this.auth.context?.role !== 'Student' || this.auth.context.scope !== 'NORMAL') || endpoint.access === 'teacher' && (this.auth.context?.role !== 'Teacher' || this.auth.context.scope !== 'NORMAL')) return {kind:'blocked'};
    if (endpoint.access === 'admissions' && (this.auth.context?.role !== 'Admissions' || this.auth.context.scope !== 'NORMAL')) return {kind:'blocked'};
    if (endpoint.access === 'accountant' && (this.auth.context?.role !== 'Accountant' || this.auth.context.scope !== 'NORMAL')) return {kind:'blocked'};
    const suffixes:Partial<Record<Endpoint,string>>={adminLock:'lock',adminReset:'reset-password',adminUnlock:'unlock',adminDisable:'disable',adminRestore:'restore',courseUpdate:'details',courseDeactivate:'deactivate',courseActivate:'activate'};
    Object.assign(suffixes,{programEdit:'configuration',programRename:'name',programPreview:'activation-preview',programActivate:'activate',programDraft:'return-to-draft',programDeactivate:'deactivate',sessionSchedule:'schedule',sessionOpen:'open',sessionClose:'close',sessionCancelPreview:'cancellation-preview',sessionCancel:'cancel',sessionGraderPreview:'grader-change-preview',sessionGrader:'change-grader',sessionRoster:'roster/search'});
    Object.assign(suffixes,{studentCancelPreview:'cancellation-preview',studentCancel:'cancel',studentOptions:'entry-test-options',studentBook:'entry-test-bookings',studentCancelBooking:'cancel',studentReschedule:'reschedule',studentResults:'entry-results',studentConfirmPreview:'confirmation-preview',studentConfirm:'confirm',teacherRoster:'roster/search',teacherGrade:'score',teacherAbsence:'absence',teacherCorrect:'score'});
    Object.assign(suffixes,{admissionsApproval:'approval-preview',admissionsApprove:'approve',admissionsReject:'reject',admissionsCancellation:'cancellation-preview',admissionsCancel:'cancel'});
    Object.assign(suffixes,{financePreview:'collection-preview',financeCollect:'collect',financeCorrect:'metadata'});
    Object.assign(suffixes,{weeklyPreview:'weekly-schedule-preview',weeklyCreate:'weekly-schedules',scheduleMovePreview:'move-preview',scheduleMove:'time-room',scheduleReplacePreview:'replacement-preview',scheduleReplace:'replace'});
    Object.assign(suffixes,{classCapacity:'capacity',classTeacherPreview:'teacher-change-preview',classTeacher:'change-teacher',classTransition:'transition-preview',classOpen:'open',classCancel:'cancel',classRoster:'roster/search',teacherClassRoster:'roster/search',classOptions:'classes',classPlacement:'placement-preview',classAssign:'assignments',classScheduling:'scheduling-context',classSchedulePreview:'schedule-preview',classSchedule:'schedules'});
    Object.assign(suffixes,{classStart:'start',classComplete:'complete',heldPreview:'held-preview',heldConfirm:'confirm-held',attendanceSummary:'attendance',academicAttendanceSummary:'attendance',attendanceSheet:'attendance',academicAttendanceSheet:'attendance',attendanceSave:'attendance'});
    Object.assign(suffixes,{gradebook:'gradebook',gradeDraft:'grade-draft',gradeSave:'grade-draft',gradeOptions:'schedule-options',gradePreview:'grade-submission-preview',gradeSubmit:'submit-grades',gradeResubmit:'resubmit-grades',gradeReturnPreview:'review-preview',gradeReturn:'return'});
    const gradeNested=['gradeDraft','gradeSave'].includes(options.endpoint),gradeComponent=options.endpoint==='gradeOptions';
    const scopedId=(id:unknown)=>Number.isInteger(id)&&(id as number)>=1&&(id as number)<=2147483647;
    if(gradeNested?!scopedId(options.assignmentId):options.assignmentId!==undefined)return {kind:'blocked'};
    if(gradeComponent?!scopedId(options.componentId):options.componentId!==undefined)return {kind:'blocked'};
    const attendanceNested=['attendanceSheet','academicAttendanceSheet','attendanceSave'].includes(options.endpoint);
    if(attendanceNested?(!Number.isInteger(options.scheduleId)||options.scheduleId!<1||options.scheduleId!>2147483647):options.scheduleId!==undefined)return {kind:'blocked'};
    const lookup=['gradeResult','gradeReviewResult','attendanceResult','classResult','financeResult','admissionsResult','adminLockResult','adminResult','courseResult','programResult','sessionResult','studentResult','teacherResult'].includes(options.endpoint);
    const nested=['studentCancelBooking','studentReschedule'].includes(options.endpoint);
    const needsTarget = ['gradeReview','teacherSchedule','studentSchedule','classDetail','classCandidate','academicSchedule','teacherClass','studentClass','financeDetail','admissionsDetail','studentInvoice','adminDetail','courseDetail','programDetail','sessionDetail','studentCourse','studentRegistration','teacherSession','teacherTask'].includes(options.endpoint) || Object.hasOwn(suffixes,options.endpoint);
    const needsKey = lookup || endpoint.intent==='write' && (endpoint.access==='accountant'||endpoint.access==='admissions'||endpoint.access==='admin'||endpoint.access==='academic'||endpoint.access==='teacher'||endpoint.access==='student'&&options.endpoint!=='studentProfileUpdate');
    if(nested&&(!Number.isInteger(options.registrationId)||options.registrationId!<1||options.registrationId!>2147483647)||!nested&&options.registrationId!==undefined)return {kind:'blocked'};
    if (needsTarget && (!Number.isInteger(options.targetId) || options.targetId! < 1 || options.targetId! > 2147483647) ||
        !needsTarget && options.targetId !== undefined ||
        needsKey && (typeof options.requestId !== 'string' || options.requestId==='00000000-0000-0000-0000-000000000000' || !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/.test(options.requestId)) ||
        !needsKey && options.requestId !== undefined) return { kind: 'blocked' };
    const allowedQuery=options.endpoint==='gradeReviews'?['reviewStatus','courseId','classId','teacherId','sort','page','pageSize']:['attendanceSummary','academicAttendanceSummary'].includes(options.endpoint)?['view','sort','page','pageSize']:['teacherSchedules','studentSchedules'].includes(options.endpoint)?['fromDate','toDate','classId','view','sort','page','pageSize']:options.endpoint==='classSearch'?['classCode','courseId','bandId','programId','teacherId','status','sort','page','pageSize']:options.endpoint==='classTeachers'?['classId']:options.endpoint==='classOptions'?['page','pageSize','sort']:options.endpoint==='teacherClasses'?['status','courseId','sort','page','pageSize']:options.endpoint==='studentClasses'?['view','courseId','sort','page','pageSize']:options.endpoint==='academicSchedules'?['fromDate','toDate','classId','courseId','teacherId','view','sort','page','pageSize']:options.endpoint==='courseSearch'?['code','name','status','sort','page','pageSize']:options.endpoint==='programSearch'?['courseId','bandId','name','status','sort','page','pageSize']:options.endpoint==='sessionSearch'?['fromDate','toDate','effectiveStatus','graderId','sort','page','pageSize']:options.endpoint==='sessionOptions'?['date','startTime','endTime','sessionId','page','pageSize']:['studentHome','studentCourses'].includes(options.endpoint)?['page','pageSize']:options.endpoint==='studentRegistrations'?['courseId','status','page','pageSize']:options.endpoint==='studentOptions'?['fromDate','toDate','bookingId','page','pageSize']:options.endpoint==='studentCalendar'?['fromDate','toDate','registrationId','view','page','pageSize']:options.endpoint==='teacherSessions'?['fromDate','toDate','effectiveStatus','sort','page','pageSize']:options.endpoint==='studentInvoices'?['registrationId','status','page','pageSize']:lookup?['action']:[];
    if(options.query&&Object.keys(options.query).some(key=>!allowedQuery.includes(key)))return {kind:'blocked'};
    const action=options.query?.action;
    if(options.endpoint==='classResult'&&(!action||!['ACADEMIC.CLASS.CREATE','ACADEMIC.CLASS.UPDATE_CAPACITY','ACADEMIC.CLASS.CHANGE_TEACHER','ACADEMIC.CLASS.OPEN','ACADEMIC.CLASS.START','ACADEMIC.CLASS.COMPLETE','ACADEMIC.CLASS.CANCEL','ACADEMIC.CLASS.PLACE_REGISTRATION','ACADEMIC.SCHEDULE.CREATE','ACADEMIC.SCHEDULE.CREATE_WEEKLY','ACADEMIC.SCHEDULE.MOVE','ACADEMIC.SCHEDULE.REPLACE','ACADEMIC.SCHEDULE.CONFIRM_HELD'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='financeResult'&&(!action||!['FINANCE.INVOICE.COLLECT','FINANCE.PAYMENT.UPDATE_METADATA'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='admissionsResult'&&(!action||!['ADMISSIONS.REGISTRATION.APPROVE','ADMISSIONS.REGISTRATION.REJECT','ADMISSIONS.REGISTRATION.CANCEL'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='studentResult'&&(!action||!['STUDENT.REGISTRATION.CREATE','STUDENT.REGISTRATION.CANCEL','STUDENT.ENTRY_BOOKING.CREATE','STUDENT.ENTRY_BOOKING.CANCEL','STUDENT.ENTRY_BOOKING.RESCHEDULE','STUDENT.REGISTRATION.CONFIRM'].includes(action))||options.endpoint==='teacherResult'&&(!action||!['TEACHER.ENTRY_RESULT.CREATE','TEACHER.ENTRY_BOOKING.ABSENCE','TEACHER.ENTRY_RESULT.CORRECT'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='programResult'&&(!action||!['ACADEMIC.PROGRAM.CREATE','ACADEMIC.PROGRAM.UPDATE_CONFIGURATION','ACADEMIC.PROGRAM.RENAME','ACADEMIC.PROGRAM.ACTIVATE','ACADEMIC.PROGRAM.RETURN_TO_DRAFT','ACADEMIC.PROGRAM.DEACTIVATE'].includes(action))||options.endpoint==='sessionResult'&&(!action||!['ACADEMIC.ENTRY_SESSION.CREATE','ACADEMIC.ENTRY_SESSION.UPDATE_SCHEDULE','ACADEMIC.ENTRY_SESSION.OPEN','ACADEMIC.ENTRY_SESSION.CLOSE','ACADEMIC.ENTRY_SESSION.CANCEL','ACADEMIC.ENTRY_SESSION.CHANGE_GRADER'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='adminResult'&&(!action||!['ADMIN.STAFF.CREATE','ADMIN.ACCOUNT.RESET_PASSWORD','ADMIN.ACCOUNT.LOCK','ADMIN.ACCOUNT.UNLOCK','ADMIN.ACCOUNT.DISABLE','ADMIN.ACCOUNT.RESTORE'].includes(action))||options.endpoint==='courseResult'&&(!action||!['ACADEMIC.COURSE.CREATE','ACADEMIC.COURSE.UPDATE_DETAILS','ACADEMIC.COURSE.DEACTIVATE','ACADEMIC.COURSE.ACTIVATE'].includes(action)))return {kind:'blocked'};
    if(options.endpoint==='attendanceResult'&&action!=='TEACHER.ATTENDANCE.SAVE_DELTA')return {kind:'blocked'};
    if(options.endpoint==='gradeResult'&&(!action||!['TEACHER.GRADE_DRAFT.SAVE_DELTA','TEACHER.CLASS.SUBMIT_GRADES','TEACHER.CLASS.RESUBMIT_GRADES'].includes(action))||options.endpoint==='gradeReviewResult'&&action!=='ACADEMIC.CLASS.RETURN_GRADES')return {kind:'blocked'};
    const query=options.endpoint==='adminLockResult'?'action=ADMIN.ACCOUNT.LOCK':new URLSearchParams(options.query).toString();
    const path=(lookup?`${endpoint.path}/${options.requestId}`:nested?`${endpoint.path}/${options.registrationId}/entry-test-bookings/${options.targetId}/${suffixes[options.endpoint]}`:gradeNested?`${endpoint.path}/${options.targetId}/assignments/${options.assignmentId}/grade-draft`:gradeComponent?`${endpoint.path}/${options.targetId}/components/${options.componentId}/schedule-options`:attendanceNested?`${endpoint.path}/${options.targetId}/lessons/${options.scheduleId}/attendance`:needsTarget?`${endpoint.path}/${options.targetId}${suffixes[options.endpoint]?'/'+suffixes[options.endpoint]:''}`:endpoint.path)+(query?'?'+query:'');
    if (options.endpoint === 'login' && this.auth.context?.scope === 'PASSWORD_CHANGE') return { kind: 'blocked' };
    if (options.endpoint === 'register' && this.auth.context) return { kind: 'blocked' };
    const token = this.auth.csrf;
    if (endpoint.method !== 'GET' && !token) return { kind: 'blocked' };
    const ticket = this.auth.requests.begin(options.slot);
    const controller = new AbortController();
    const abort = () => controller.abort();
    ticket.signal.addEventListener('abort', abort, { once: true });
    options.signal?.addEventListener('abort', abort, { once: true });
    if (ticket.signal.aborted || options.signal?.aborted) abort();
    let sent = false;
    const uncertainWrite = (): HttpResult<T> => {
      if (ticket.isCurrent() && ['login', 'logout', 'password'].includes(options.endpoint)) this.auth.clear();
      return { kind: 'unknown' };
    };
    try {
      if (controller.signal.aborted) return { kind: 'discarded' };
      const headers: Record<string, string> = { Accept: 'application/json' };
      if (endpoint.method !== 'GET') headers['X-CSRF-TOKEN'] = token!;
      if (needsKey && !lookup) headers['Idempotency-Key'] = options.requestId!;
      let body: string | undefined;
      if (options.body !== undefined) { body = JSON.stringify(options.body); headers['Content-Type'] = 'application/json'; }
      sent = true;
      const response = await this.fetcher(path, {
        method: endpoint.method, credentials: 'same-origin', cache: 'no-store', redirect: 'error',
        headers, body, signal: controller.signal,
      });
      body = undefined;
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
      let payload: unknown;
      try { payload = response.status === 204 ? undefined : await response.json(); }
      catch {
        if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
        if (response.status === 401 && options.endpoint !== 'login') this.auth.clear();
        if (response.status === 403) this.auth.requireCsrfRecovery();
        return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
      }
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'discarded' };
      if (response.ok) {
        const expectedStatus = options.endpoint === 'register' ? 201 :
          options.endpoint === 'logout' || options.endpoint === 'password' ? 204 : 200;
        if (response.status !== expectedStatus && !(response.status===201&&['weeklyCreate','scheduleReplace','classCreate','classAssign','classSchedule','financeCollect','staffCreate','courseCreate','programCreate','sessionCreate','studentCreate','studentBook'].includes(options.endpoint))) return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
        let value: T;
        try { value = options.decode(payload); }
        catch {
          if (options.endpoint === 'me') this.auth.clear();
          if (options.endpoint === 'csrf') this.auth.requireCsrfRecovery();
          return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
        }
        if (options.endpoint === 'csrf') {
          try { this.auth.acceptCsrf(payload); }
          catch { this.auth.requireCsrfRecovery(); return { kind: 'unavailable' }; }
        }
        if (options.endpoint === 'me') {
          try { this.auth.refreshContext(payload); }
          catch { this.auth.clear(); return { kind: 'unavailable' }; }
        }
        if (options.endpoint === 'login') this.auth.startSession(payload);
        if (options.endpoint === 'logout' || options.endpoint === 'password') this.auth.clear();
        return { kind: 'success', value };
      }
      let error: ApiError;
      try { error = decodeApiError(payload); }
      catch {
        if (response.status === 401 && options.endpoint !== 'login') this.auth.clear();
        if (response.status === 403) this.auth.requireCsrfRecovery();
        return endpoint.intent === 'write' ? uncertainWrite() : { kind: 'unavailable' };
      }
      const action = errorAction(response.status, error.code);
      // Only a still-current response may invalidate a current identity.
      if (action === 'session') this.auth.clear();
      if (action === 'csrf') this.auth.requireCsrfRecovery();
      if (endpoint.intent === 'write' && response.status >= 500) uncertainWrite();
      return { kind: 'error', status: response.status, error, action,
        retryAfter: response.status === 429 ? response.headers.get('Retry-After') : null,
        outcome: endpoint.intent === 'write' && response.status >= 500 ? 'unknown' : 'rejected' };
    } catch {
      if (!ticket.isCurrent() || controller.signal.aborted) return endpoint.intent === 'write' && sent ? uncertainWrite() : { kind: 'discarded' };
      return endpoint.intent === 'write' && sent ? uncertainWrite() : { kind: 'unavailable' };
    } finally {
      ticket.signal.removeEventListener('abort', abort);
      options.signal?.removeEventListener('abort', abort);
      ticket.finish();
    }
  }
}
