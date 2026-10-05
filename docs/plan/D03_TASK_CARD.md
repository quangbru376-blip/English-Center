# D03 — Admissions registration, entry tests and Band mapping

Authorized owner continuation2026-10-05 after D02 PASS. Codex manager; three
bounded AGY fragments, independent Codex tests/review, apply only final PASS
under ADR-163. Mapping only. No D04, SQL execution, migrations, triggers, seed,
packages, profile/DI, endpoints, workflows, push/deployment or installation.

implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: accepted EF10.0.4/SQLServer compatibility160; disconnected
metadata tests here, no SQLite or migrations. Canonical SQL/null-bundle, exact
decimal, JWT-cookie/full-set/error contracts override stale skill instructions.

## Bounded worker authority / read_context

Codex read AGENTS and Project Memory in order, requirements, DATABASE_SPEC T09–T13
and sections8/11, applicable execution plan. This self-contained handoff supplies
exact accepted data contract. Worker MUST view COMPLETE this card, COMPLETE
backend-clean-code/SKILL.md and COMPLETE SKILL_ROUTING.md before writing.
ONLY these three context files plus its exact allowed output files if existing
may be read. DO NOT read AGENTS, memory, baseline, links, tests, package files or
other production files; no search/list/scans/commands/network/browser/MCP/Git/
subagents. Manager performed repository preflight; worker must stop rather than
repeat it outside this allowlist. Use only view_file and allowed file writes.

Fragment A: Registration + EntryTestSession entity/config,4files,700diff lines.
Fragment B: EntryTestAttempt + EntryTestBooking entity/config,4files,600diff lines.
Fragment C: BandAssignment entity/config,2files,220diff lines.
Entities in Domain/Admissions; configs Infrastructure/Configurations.
Codex alone registers five DbSets/configs in existing EnglishCenterDbContext,
writes AdmissionsMappingTests.cs, adjusts D02 whole-model assertions to its
8-table subset without weakening D01/D02 signatures. No other production edits.
Do not read unavailable parent source. Their exact accepted key names/types:
Domain.Identity.Account.AccountId int; StudentProfile.StudentId int;
Domain.Academics.Course.CourseId int; Band.BandId int.

## Shared mapping rules

Tables dbo named EXACT Registration/EntryTestSession/EntryTestAttempt/
EntryTestBooking/BandAssignment. Column dictionaries below use camel-to-Pascal
CLR properties, e.g. registration_id→RegistrationId, cancelled_by→CancelledBy.
Use sealed plain entities/public get-set, string.Empty required strings,
nullable optional strings and actors/timestamps/Guid; RowVersion byte[]=[].
Navigation references nullable for loadedness; FK nullability is explicit.
Business PK int IDENTITY(1,1), explicit int column; every row_version required
rowversion/IsRowVersion() generated concurrency. No shadow/extra columns,
persisted attempt status, cached booked count, best_score or frozen flags.
All FKs NO ACTION; define explicit names as below. Global model collation
Latin1_General_100_CI_AS_SC inherited. Explicit SQL types/lengths/precision/nulls.
No SQL defaults except enumerated below. No lazy-loading/collections needed.
Check enum EXACT case using COLLATE Latin1_General_100_BIN2 IN (...) AND
DATALENGTH([column])=LEN([column]); prior enums need explicit IS NOT NULL too.
Every iff bundle has two OR branches, with all mandatory members IS NOT NULL
in the matching-state branch and all members IS NULL in the other branch.
SQL CHECK accepts UNKNOWN: equality/IN alone cannot replace IS NOT NULL.
Mandatory reasons add LEN(reason)>0; optional cancellation reasons may be NULL.

## A1 Registration —20columns

registration_id int N PK; student_id int N; course_id int N;
status varchar(16) N default PENDING HasSentinel(string.Empty);
is_unfinished bit N default true HasSentinel(true), entity initialized true;
created_at datetime2(3) N default SYSUTCDATETIME();
admission_confirmed_at datetime2(3) Y;
admission_approved_by int Y; admission_approved_at datetime2(3) Y;
rejected_by int Y; rejected_at datetime2(3) Y; rejection_reason nvarchar(2000) Y;
rejected_from_status varchar(16) Y; rejection_operation_id uniqueidentifier Y;
cancelled_by int Y; cancelled_at datetime2(3) Y; cancellation_reason nvarchar(2000) Y;
cancelled_from_status varchar(16) Y; cancellation_operation_id uniqueidentifier Y;
row_version rowversion N. DateTime CLR, Guid? operation IDs, strings/actors nullable.
Navs Student (StudentProfile), Course, AdmissionApprovedByAccount, RejectedByAccount,
CancelledByAccount. FKs respectively named FK_Registration_StudentProfile_student_id,
FK_Registration_Course_course_id, FK_Registration_Account_admission_approved_by,
FK_Registration_Account_rejected_by, FK_Registration_Account_cancelled_by.
UQ_Registration_Unfinished(student_id,course_id) WHERE [is_unfinished] = 1.
IX_Registration_StudentCreated(student_id,created_at,registration_id);
IX_Registration_CourseStateCreated(course_id,status,created_at,registration_id);
IX_Registration_ApprovedBy(admission_approved_by), RejectedBy(rejected_by),
CancelledBy(cancelled_by), unfiltered nonunique actor indexes.
CK_Registration_Status exact PENDING/TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED/
REJECTED/CANCELLED (NO COMPLETED).
CK_Registration_ApprovedBundle actor/time both NULL OR both NOT NULL.
CK_Registration_RejectedBundle iff status REJECTED: actor/time/reason/prior/
operation ALL NOT NULL, reason LEN>0, prior EXACT CONFIRMED plus padding guard;
else all five NULL. CK_Registration_CancelledBundle iff status CANCELLED:
actor/time/prior/operation NOT NULL; reason optional; prior EXACT one of PENDING/
TESTING/BAND_ASSIGNED/CONFIRMED/ADMITTED plus padding guard; else ALL five NULL.
Do not impose cross-table unfinished predicate, role or actor-specific reason
in CHECK. Admission approval history can survive cancellation.

## A2 EntryTestSession —12columns

session_id int N PK; start_at datetime2(3) N; end_at datetime2(3) N;
room varchar(30) N; capacity int N; grader_id int N;
status varchar(16) N default CLOSED HasSentinel(string.Empty);
cancelled_by int Y; cancelled_at datetime2(3) Y; cancellation_reason nvarchar(2000) Y;
cancellation_operation_id uniqueidentifier Y; row_version rowversion N.
Navs Grader and CancelledByAccount (Account), FKs FK_EntryTestSession_Account_grader_id,
FK_EntryTestSession_Account_cancelled_by.
IX_Session_GraderStart(grader_id,start_at,end_at,session_id);
IX_Session_RoomStart(room,start_at,end_at,session_id);
IX_Session_CancelledBy(cancelled_by) unfiltered.
CK_EntryTestSession_Status exact OPEN/CLOSED/CANCELLED.
CK_EntryTestSession_Capacity [capacity] BETWEEN 1 AND 20.
CK_EntryTestSession_Time start_at < end_at; DATEPART(second,start/end)=0 AND
DATEPART(millisecond,start/end)=0; CAST(DATEADD(minute,420,start_at) AS date)=
CAST(DATEADD(minute,420,end_at) AS date). No invented max duration.
CK_EntryTestSession_Room DATALENGTH BETWEEN1AND30, DATALENGTH=LEN, BIN2
NOT LIKE '%[^A-Z0-9_-]%'. CK_EntryTestSession_CancelledBundle iff CANCELLED:
actor/time/reason/operation all NOT NULL, reason LEN>0; else all four NULL.
Resources/time/role/occupancy/effective CLOSED remain future service/UDF/TX checks.

## B1 EntryTestAttempt —8columns

attempt_id int N PK; registration_id int N; attempt_number int N;
score decimal(5,2) Y (decimal?); graded_by int Y; graded_at datetime2(3) Y;
completed_booking_id int Y; row_version rowversion N. No defaults for result.
Navs Registration, GradedByAccount (Account), CompletedBooking (EntryTestBooking).
FK_Attempt_Registration_registration_id; FK_Attempt_Account_graded_by;
FK_Attempt_Booking_CompletedBookingAttempt (completed_booking_id,attempt_id)→
EntryTestBooking(booking_id,attempt_id), optional composite NO ACTION.
Do NOT add separate scalar completed_booking_id FK.
AK_Attempt_IdRegistration(attempt_id,registration_id) alternate key, not index.
UQ_Attempt_RegistrationNumber(registration_id,attempt_number);
UQ_Attempt_CompletedBooking(completed_booking_id) WHERE [completed_booking_id] IS NOT NULL;
IX_Attempt_GradedBy(graded_by) unfiltered.
CK_EntryTestAttempt_Number [attempt_number] IN (1,2);
CK_EntryTestAttempt_Score [score] IS NULL OR ([score]>=0 AND [score]<=100);
CK_EntryTestAttempt_ResultBundle score/graded_by/graded_at/completed_booking_id
ALL NULL OR ALL NOT NULL. No persisted state; NULL is ungraded/absent,0 valid score.

## B2 EntryTestBooking —14columns

booking_id int N PK; attempt_id int N; session_id int N;
status varchar(16) N default SCHEDULED HasSentinel(string.Empty);
booked_at datetime2(3) N default SYSUTCDATETIME();
absent_by int Y; absent_at datetime2(3) Y; absent_operation_id uniqueidentifier Y;
cancelled_by int Y; cancelled_at datetime2(3) Y; cancellation_reason nvarchar(2000) Y;
cancelled_from_status varchar(16) Y; cancellation_operation_id uniqueidentifier Y;
row_version rowversion N.
Navs Attempt, Session, AbsentByAccount, CancelledByAccount; FKs
FK_Booking_Attempt_attempt_id, FK_Booking_Session_session_id,
FK_Booking_Account_absent_by, FK_Booking_Account_cancelled_by.
AK_Booking_IdAttempt(booking_id,attempt_id) alternate key for result composite FK.
Two DISTINCT MODEL INDEX NAMES on same property attempt_id:
UQ_Booking_Current WHERE [status] = 'SCHEDULED'; UQ_Booking_Completed WHERE
[status] = 'COMPLETED'. Use HasIndex(property, unique model index name) for each
so EF does not overwrite one with another. Both unique.
IX_Booking_AttemptHistory(attempt_id,booked_at,booking_id), descending false/true/true;
IX_Booking_SessionState(session_id,status,booking_id);
IX_Booking_AbsentBy(absent_by), IX_Booking_CancelledBy(cancelled_by), unfiltered.
CK_EntryTestBooking_Status exact SCHEDULED/COMPLETED/ABSENT/CANCELLED.
CK_EntryTestBooking_AbsenceBundle iff ABSENT actor/time/operation all NOT NULL,
else all three NULL. CK_EntryTestBooking_CancellationBundle iff CANCELLED:
actor/time/prior/operation NOT NULL; prior EXACT SCHEDULED plus padding guard,
reason optional; else all five NULL.
Cycle: insert Attempt with result NULL→Booking→set result in future transaction;
no cascades, no fake required composite result link at Attempt creation.

## C BandAssignment —6columns

band_assignment_id int N PK; registration_id int N; best_attempt_id int N;
band_id int N; assigned_at datetime2(3) N default SYSUTCDATETIME();
row_version rowversion N. Navs Registration, BestAttempt (EntryTestAttempt), Band.
FK_BandAssignment_Registration_registration_id unique one-to-one WithOne() no
inverse navigation; FK_BandAssignment_Attempt_BestAttemptRegistration composite
(best_attempt_id,registration_id)→Attempt(attempt_id,registration_id) required;
FK_BandAssignment_Band_band_id required. All NO ACTION. No extra scalar best FK.
UQ_BandAssignment_Registration(registration_id) unique;
IX_BandAssignment_Band(band_id); IX_BandAssignment_BestAttempt(best_attempt_id,
registration_id) covers exact composite FK. No CHECK best result cross-table.

## Codex verification / skill_verification

Completed below: independently inspected columns/null/default/type/precision/ID/RV, exact
FKs/AKs/filter/index order, NULL-safe bundles, minute/VN-date checks, no shadow/
extra state fields. Preserve old signatures, locked restore/build/full tests,
disconnected SQLServer DDL and closed connection; no engine enforcement claim.
Expected total13tables/116columns/13PK/2AK/23FK/37indexes/34CHECKs/12RV;
D03 adds5tables/60columns/17FK/21indexes/15checks. Package locks unchanged.
Activation/transitions/unfinished/result↔COMPLETED/best tie/role/time/occupancy/
freeze/audit/replay require future service/TX/real SQL evidence; not D03 claims.

## Review PASS — manager bdcd9ad,2026-10-05

skill_verification: three AGY runs SUCCESS/exit0; exact writes/read scopes/caps,
zero observed violations. Whole D03 card186lines, backend skill46lines and
routing101lines viewed before first write in every run, hashes/step/header
proof retained in D03-evidence. Tool-event audit is not OS read isolation or
proof of unobserved activity. Codex independently reviewed every entity/config
against T09–T13, requirements and compatibility policy, wrote17new tests.

| Fragment | Run | Reads/writes | Runner diff lines |
| --- | --- | --- | --- |
| Registration/Session | 0942acef-2f4 | 8/4 | 399 |
| Attempt/Booking | ef25b7d5-bdf | 11/4 | 347 |
| BandAssignment | 6bde16ab-40b | 7/2 | 100 |

All dispatch base60e1656. Worker snapshots retained under
%TEMP%/codex-antigravity-worker/worktrees/main_workplace-<run>; manager tree
%TEMP%/codex-d03/manager. No rejected D03 run; no observed commands/network/MCP.
Manager made explicit IDENTITY(1,1) for5PK and assigned_at precision3. Final model
uses explicit FK indexes and removes only EF ForeignKeyIndexConvention to avoid
redundant composite completed-booking index. All23FKs independently checked for
lookup coverage; D01/D02 physical signatures retain PASS. EF recreates removed
FK indexes ([EF10.0.4 source](https://raw.githubusercontent.com/dotnet/efcore/v10.0.4/src/EFCore/Metadata/Conventions/ForeignKeyIndexConvention.cs)); no direct metadata-index removal workaround retained.

Locked restore/build0warnings0errors/full427tests PASS/0fail/0skip, including
17new D03 cases. Exact full model13tables/116columns/13PK/2AK/23FK/37indexes/
34CHECKs/12RV. D03 adds60columns/17FK(9required,8optional)/21indexes/15checks;
FKs all NO ACTION. Composite result→Booking is optional until result exists;
best Attempt/Registration ownership is composite required; no duplicate scalar
FKs. Distinct filtered UQ current/completed Booking remain simultaneously present.
No default score/absence/actors/times except accepted creation/recalculation
server UTC fields. Iff event bundles explicitly guard every mandatory NULL;
optional cancellation reason remains optional by actor rule. Strict literal
case/padding, attempt1–2, score0–100, capacity1–20 and minute/VN same-date/room
checks mapped. No additional persisted attempt status/booked_count/best_score.

Final scope13backend files/1157insertions/8deletions, diff check clean. No package/
lock/project/Program.cs/API/DI/profile/migration/trigger/seed changes. Closed SQL
connection and disconnected generated DDL verified. This proves physical mapping,
not live constraints/TX/result↔COMPLETED/best tie/role/occupancy/freeze/replay.
Applicable schema/security/scope/backend review PASS; API/UI/workflow/migration/
real SQL checks remain future integration gates, N/A to D03 mapping completion.
Owner explanation: five data classes preserve enrollment and appointments,
with optional ungraded result and same-owner composite links. Services implement
workflow later; this card supplies review notes, no API/UI docs change required.
Reviewed manager bdcd9ad9e524d6bb741231c0683bb49c7f75402d, backend Git tree
3f22776db8abd00175a1376a79dc3eee595c264d. Main verification follows below.
Next proposed D04 Class/Assignment/Schedule, not started; no further scope opened.

## Main application / reverified PASS

Main244353fe43ec58599954c9097b92c638c62e9255 under ADR-163, identical reviewed
backend tree3f22776db8abd00175a1376a79dc3eee595c264d. Locked restore/build
0warnings0errors/full427tests PASS/0fail/0skip on main. Package locks unchanged;
no SQL/DB/migrations/installation/push/deployment. No unresolved D03 blocker.
