# D04 — Class, StudentClassAssignment and Schedule EF mapping

Authorized continuation2026-10-05 after D03 PASS. Codex manager/independent tests/
review. Bounded AGY per entity; apply final PASS under ADR-163. Mapping only:
no D05, SQL execution/migrations/triggers/seed/packages/DI/profile/services/routes,
installation/push/deploy. Runtime readiness and M0/V01 remain incomplete.
implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: EF10.0.4/SQLServer160 already pinned; no SQLite or migration
in mapping-only task; exact accepted date/time/NULL contracts. JWT-cookie/full-set/
error rules still override stale skill guidance. FK indexes explicit ADR-164;
no automatic FK index convention to fill missing indexes.

## Worker authority / read_context

Codex read AGENTS/Project Memory in mandated order, requirements, DATABASE_SPEC
T14–T16/sections8/11 and current execution handoffs. This card is the exact
self-contained manager digest. Worker must VIEW COMPLETE this card, COMPLETE
backend SKILL.md and COMPLETE SKILL_ROUTING.md before writing. ONLY those three
context files and its own two allowed output paths if existing may be read.
Do not read AGENTS/memory/baseline/links/tests/other source. No search/list/scans/
commands/Git/network/browser/MCP/subagents/secrets. Only view_file and allowed
file write/replace tools; stop rather than broaden scope. Skills add no authority.

Three separate contracts: Domain/Training/<Entity>.cs and
Infrastructure/Configurations/<Entity>Configuration.cs,2files each. Caps:
Class320lines; StudentClassAssignment200; Schedule420. Exact namespace Training.
Codex alone edits existing DbContext for3DbSets/config registrations, writes
TrainingMappingTests.cs and adapts D03 global counts to its13-table subset;
whole-model FK lookup coverage stays global. No other production edits.

## Common physical rules

Tables dbo named EXACT Class/StudentClassAssignment/Schedule; int PK
IDENTITY(1,1), explicit SQL types/length/Unicode/precision/requiredness. Sealed
plain entities/public get-set; required strings string.Empty, nullable optional
strings/actors/timestamps/Guid; byte[] RowVersion=[]. Nullable navigations for
loadedness, FK requiredness explicit. NO ACTION every FK; no shadow/additional
columns/collection navs, cascade, seeds/caches/version flags. No SQL defaults
except explicit below. Event DateTime UTC datetime2(3), business DateOnly date,
TimeOnly time(0) precision0. No invented actor/time from zero CLR initialization.
Global CI_AS_SC inherited. Exact enums BIN2 IN(literals) AND DATALENGTH=LEN to
reject case/padding. Iff bundles must have mandatory members IS NOT NULL in
state branch and ALL members IS NULL in other branch; reasons LEN>0. This
avoids SQL CHECK accepting UNKNOWN. Optional pointer CHECK explicit NULL branch.

Existing parent key types/names supplied without reading their source:
Domain.Academics.TrainingProgram.ProgramId int;
Domain.Identity.Account.AccountId int;
Domain.Admissions.Registration.RegistrationId int.

## Class —12columns

ClassId/class_id int N PK; ProgramId/program_id int N; TeacherId/teacher_id int N;
ClassCode/class_code varchar(30) N; Status/status varchar(16) N default PLANNED
HasSentinel(string.Empty); MaxSize/max_size int N (no default);
CreatedAt/created_at datetime2(3) N default SYSUTCDATETIME();
CancelledBy/cancelled_by int Y; CancelledAt/cancelled_at datetime2(3) Y;
CancellationReason/cancellation_reason nvarchar(2000) Y;
CancellationOperationId/cancellation_operation_id uniqueidentifier Y;
RowVersion/row_version rowversion N IsRowVersion().
Navs Program(TrainingProgram), Teacher(Account), CancelledByAccount(Account).
FK_Class_TrainingProgram_program_id / FK_Class_Account_teacher_id required many;
FK_Class_Account_cancelled_by optional many; all NO ACTION.
UQ_Class_Code(class_code) unique unconditional incl CANCELLED;
IX_Class_ProgramState(program_id,status,class_id);
IX_Class_TeacherState(teacher_id,status,class_id); IX_Class_CancelledBy(cancelled_by).
CK_Class_Status exact PLANNED/OPEN/ONGOING/COMPLETED/CANCELLED.
CK_Class_MaxSize [max_size] BETWEEN 1 AND 15.
CK_Class_Code DATALENGTH([class_code]) BETWEEN 1 AND 30 AND DATALENGTH=LEN,
[class_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'.
CK_Class_CancellationBundle iff status CANCELLED: actor/time/reason/operation
ALL NOT NULL, LEN(reason)>0; else all four NULL. No current_size/frozen flag.
Program linkage/code immutability, Teacher role/availability, actual placed count,
opening/start/completion/cancellation constraints are later services/SQL/TX.

## StudentClassAssignment —5columns, immutable insert association, NO rowversion

ClassAssignmentId/class_assignment_id int N PK; ClassId/class_id int N;
RegistrationId/registration_id int N; AssignedBy/assigned_by int N;
AssignedAt/assigned_at datetime2(3) N default SYSUTCDATETIME().
Navs Class, Registration, AssignedByAccount(Account). FKs
FK_Assignment_Class_class_id required many;
FK_Assignment_Registration_registration_id required UNIQUE one-to-one WithOne()
no inverse nav; FK_Assignment_Account_assigned_by required many. All NO ACTION.
UQ_Assignment_Registration(registration_id) unique unconditional;
IX_Assignment_Class(class_id,class_assignment_id);
IX_Assignment_AssignedBy(assigned_by). No CHECK/defaults beyond assigned_at;
no student_id, state or rowversion. No update/delete/move workflow. Future W15
checks OPEN/payment/Course/Band/capacity/resources and creates GradeReport atomically;
this task does not fabricate GradeReport or Score/Attendance rows.

## Schedule —18columns

ScheduleId/schedule_id int N PK; ClassId/class_id int N;
SessionType/session_type varchar(16) N (no default);
ObligationKey/obligation_key uniqueidentifier N (no default; service random GUID);
ReplacesScheduleId/replaces_schedule_id int Y;
IsCurrent/is_current bit N default true HasSentinel(true), CLR initialized true;
SessionDate/session_date date N DateOnly; StartTime/start_time and EndTime/end_time
 time(0) N TimeOnly/precision0; Room/room varchar(30) N;
OccurrenceState/occurrence_state varchar(16) N default PLANNED HasSentinel(string.Empty);
HeldBy/held_by int Y; HeldAt/held_at datetime2(3) Y;
NotHeldBy/not_held_by int Y; NotHeldAt/not_held_at datetime2(3) Y;
NotHeldReason/not_held_reason nvarchar(2000) Y;
ReplacementOperationId/replacement_operation_id uniqueidentifier Y;
RowVersion/row_version rowversion N IsRowVersion().
Navs Class, ReplacesSchedule(Schedule), HeldByAccount(Account), NotHeldByAccount(Account).
FK_Schedule_Class_class_id required many; FK_Schedule_Schedule_replaces_schedule_id
optional one-to-one WithOne() no inverse nav; FK_Schedule_Account_held_by and
FK_Schedule_Account_not_held_by optional many. All NO ACTION. Only scalar
predecessor FK to schedule_id; same Class/type/key and no cycle enforced later,
not invented additional composite keys in mapping.
UQ_Schedule_CurrentObligation(class_id,obligation_key) WHERE [is_current] = 1;
UQ_Schedule_CurrentFinalExam(class_id) WHERE [is_current] = 1 AND [session_type] = 'FINAL_EXAM';
UQ_Schedule_Predecessor(replaces_schedule_id) WHERE [replaces_schedule_id] IS NOT NULL.
All three unique; NO global unique history/obligation/class-type.
IX_Schedule_ClassCurrentDate(class_id,is_current,session_date,start_time,schedule_id);
IX_Schedule_RoomDate(room,session_date,start_time,end_time,schedule_id);
IX_Schedule_HeldBy(held_by); IX_Schedule_NotHeldBy(not_held_by), unfiltered.
CK_Schedule_Type exact LESSON/FINAL_EXAM.
CK_Schedule_State exact PLANNED/HELD/NOT_HELD.
CK_Schedule_Time [start_time] < [end_time] AND DATEPART(second,[start_time])=0
AND DATEPART(second,[end_time])=0. No invented max duration/date defaults.
CK_Schedule_Room same Class code ASCII pattern/length/padding guard with [room].
CK_Schedule_NoSelfReplacement [replaces_schedule_id] IS NULL OR
[replaces_schedule_id] <> [schedule_id].
CK_Schedule_CurrentState ([occurrence_state] IN ('PLANNED','HELD') AND [is_current]=1)
OR ([occurrence_state]='NOT_HELD' AND [is_current]=0).
CK_Schedule_HeldBundle iff occurrence_state HELD: held_by/held_at both NOT NULL;
else both NULL. CK_Schedule_NotHeldBundle iff occurrence_state NOT_HELD:
not_held_by/not_held_at/not_held_reason/replacement_operation_id all NOT NULL,
LEN(reason)>0; else all four NULL.
No automatic HELD/CANCELLED based on time, no deletion or replacement service.
Filtered unique proves maximum one current; exactly one, predecessor same Class/
type/key, cycle detection/future replacement/whole-TX and conflicts remain future.

## Codex verification / skill_verification

Pending independent physical signatures, no shadow/seeds, ID/RV/immutable no-RV,
exact defaults/precision/nullability, required/optional FK/cardinality/NO ACTION,
conditional unique filters/history, NULL-safe bundles/current-state/time/ASCII
checks. D01–D03 signatures remain intact; global FK lookup coverage checked.
Expected full16tables/151columns/16PK/2AK/33FK/51indexes/46CHECKs/14RV;
D04 adds35columns/10FK(6required,4optional)/14indexes/12checks/2RV. Package locks
unchanged. Locked restore/build/full tests in manager then main, source equality;
closed DB and generated DDL only, no claim of SQL/TX/service readiness.
