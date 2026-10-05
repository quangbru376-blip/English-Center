# D05 — Attendance, Score and GradeReport EF mapping

Authorized continuation2026-10-05 after D04 PASS. Codex manager/independent tests/
review; bounded AGY per entity. Apply final PASS under ADR-163. Mapping only:
no D06/SQL execution/migrations/triggers/seeds/packages/DI/services/API/profile/
installation/push/deploy. Full P01/M0/V01 and live SQL/TX readiness incomplete.
implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: EF10.0.4/SQLServer160 pinned; exact E03 T17–T19 and
requirements/ADR135; no SQLite/migrations for disconnected mapping-only stage.
No JWT bearer change, APIs or business workflows. Explicit FK indexes ADR-164.
This task card is manager's self-contained digest; skill grants no authority.

## Worker authority / read_context

Codex read ordered AGENTS/Project Memory, requirements, execution handoffs and
DATABASE_SPEC T17–T19/check/index sections. Worker must VIEW COMPLETE this card,
COMPLETE backend skill and COMPLETE routing before edit. Read ONLY these three
context files and its own two output files if present. No AGENTS/memory/links/
other source/tests/search/list/scans/commands/Git/network/browser/MCP/subagents/
secrets. Use only view_file and authorized write/replace tools. Stop if blocked.
Three independent contracts Domain/Training/<Entity>.cs and Infrastructure/
Configurations/<Entity>Configuration.cs; exact2files each. Caps Attendance240,
Score280, GradeReport400lines. Codex alone integrates3DbSets/configs, writes
AssessmentMappingTests.cs and adapts old global counts to prior subsets; FK
coverage remains whole-model and recognizes nullable-FK nonnull filtered indexes.

## Common rules

Namespace EnglishCenter.Api.Domain.Training; configs Infrastructure.Configurations.
Singular exact dbo tables Attendance/Score/GradeReport. Sealed plain entities,
public get/set; nullable optional values/navigations for loadedness; required
strings string.Empty; byte[] RowVersion=[]. No collection navs/extra fields,
shadow properties, computed totals/pass flags/weights/counts/history/seeds.
PK int explicit IDENTITY(1,1). Explicit SQL types/requiredness/length/Unicode/
precision. Event DateTime UTC datetime2(3) HasPrecision(3); Guid? operations
uniqueidentifier. RowVersion byte[] rowversion N IsRowVersion/concurrency generated.
Decimal CLR decimal/decimal? SQL decimal(5,2) HasPrecision(5,2). All FK NO ACTION,
exact scalar links only; no invented composite keys or trigger metadata.
No SQL defaults except GradeReport.status DRAFT HasSentinel(string.Empty).
Attendance/Score entered_at are explicit server-supplied last-write timestamps,
no default/CLR current time; metadata cannot identify actor or server clock.
Global CI_AS_SC inherited. Status enum BIN2 exact literals plus DATALENGTH=LEN.
Mandatory event members explicitly IS NOT NULL; all-null branch explicitly NULL
so SQL CHECK UNKNOWN cannot admit half bundles. Finalized/returned rules below.

Parents supplied without reading source: Domain.Training.Schedule.ScheduleId,
StudentClassAssignment.ClassAssignmentId; Domain.Identity.Account.AccountId;
Domain.Academics.ScoreComponentConfig.ComponentId. All parent PK int.

## Attendance —7columns

AttendanceId/attendance_id int N PK; ScheduleId/schedule_id int N;
ClassAssignmentId/class_assignment_id int N; Present/present bit N no default;
EnteredBy/entered_by int N; EnteredAt/entered_at datetime2(3) N no default;
RowVersion/row_version rowversion N. Navs Schedule, ClassAssignment,
EnteredByAccount(Account), nullable. Three required many FKs:
FK_Attendance_Schedule_schedule_id;
FK_Attendance_StudentClassAssignment_class_assignment_id;
FK_Attendance_Account_entered_by. All NO ACTION.
UQ_Attendance_ScheduleAssignment(schedule_id,class_assignment_id) unique unfiltered;
IX_Attendance_Assignment(class_assignment_id,attendance_id);
IX_Attendance_EnteredBy(entered_by), both nonunique unfiltered.
No CHECKs. No MISSING enum/default present/absent or automatically seeded roster.
Same Class/current LESSON/not NOT_HELD/Teacher/start/window/Class locks are later
TG04/service/TX10; existence FKs alone do not prove those checks.

## Score —8columns

ScoreId/score_id int N PK; ClassAssignmentId/class_assignment_id int N;
ComponentId/component_id int N; ScheduleId/schedule_id int Y;
ScoreValue/score_value decimal(5,2) N no default;
EnteredBy/entered_by int N; EnteredAt/entered_at datetime2(3) N no default;
RowVersion/row_version rowversion N. Navs ClassAssignment, Component,
Schedule, EnteredByAccount(Account), nullable. Required many FKs:
FK_Score_StudentClassAssignment_class_assignment_id;
FK_Score_ScoreComponentConfig_component_id; FK_Score_Account_entered_by.
Optional many FK_Score_Schedule_schedule_id. All NO ACTION.
UQ_Score_AssignmentComponent(class_assignment_id,component_id) unique unfiltered;
IX_Score_Component(component_id,score_id) nonunique unfiltered;
IX_Score_Schedule(schedule_id,score_id) nonunique WHERE [schedule_id] IS NOT NULL;
IX_Score_EnteredBy(entered_by) nonunique unfiltered.
CK_Score_Score [score_value] >= 0 AND [score_value] <= 100.
No SQL default score, no nullable score/blank row or deletion workflow.
GENERAL requires NULL Schedule, LESSON/FINAL_EXAM requires matching current
occurrence/Class/type, component same Program, actor/windows/report state and
parent RV touch are later TG05/service/TX11; no single-table fake kind CHECK.

## GradeReport —14columns

ReportId/report_id int N PK; ClassAssignmentId/class_assignment_id int N;
FinalScore/final_score decimal(5,2) Y no default;
Status/status varchar(16) N default DRAFT HasSentinel(string.Empty);
TeacherComment/teacher_comment nvarchar(2000) Y;
SubmittedBy/submitted_by int Y; SubmittedAt/submitted_at datetime2(3) Y;
SubmittedOperationId/submitted_operation_id uniqueidentifier Y;
ReviewedBy/reviewed_by int Y; ReviewedAt/reviewed_at datetime2(3) Y;
ReviewOperationId/review_operation_id uniqueidentifier Y;
ReturnedReason/returned_reason nvarchar(2000) Y;
FinalizedAt/finalized_at datetime2(3) Y; RowVersion/row_version rowversion N.
Navs ClassAssignment, SubmittedByAccount(Account), ReviewedByAccount(Account).
FK_GradeReport_StudentClassAssignment_class_assignment_id required UNIQUE one-to-one
WithOne() no inverse; FK_GradeReport_Account_submitted_by and
FK_GradeReport_Account_reviewed_by optional many. All NO ACTION.
UQ_GradeReport_Assignment(class_assignment_id) unique unfiltered;
IX_GradeReport_StateFinalized(status,finalized_at,report_id);
IX_GradeReport_SubmittedBy(submitted_by); IX_GradeReport_ReviewedBy(reviewed_by),
all three nonunique unfiltered.
Five CHECKs:
CK_GradeReport_Status exact DRAFT/SUBMITTED/RETURNED/FINALIZED BIN2/padding guard.
CK_GradeReport_FinalResult: iff FINALIZED final_score/finalized_at/reviewed_by/
reviewed_at/review_operation_id all NOT NULL, final_score0–100 and
finalized_at=reviewed_at; other states final_score/finalized_at both NULL.
CK_GradeReport_SubmitBundle: submitted_by/submitted_at/submitted_operation_id
ALL NOT NULL OR (status DRAFT AND ALL three NULL). This allows complete prior
submit for DRAFT but requires complete latest submit in every other state.
CK_GradeReport_ReviewBundle: reviewed_by/reviewed_at/review_operation_id ALL NULL
OR ALL NOT NULL. FINALIZED additionally required by FinalResult, RETURNED below.
CK_GradeReport_ReturnedReason: status <> RETURNED OR (returned_reason IS NOT NULL
AND LEN(returned_reason)>0 AND reviewed_by/reviewed_at/review_operation_id ALL
NOT NULL). Do not force returned_reason NULL on other states; latest reason and
review may be retained after resubmit. Do not invent immutable-report triggers.
Optional comment/reason normalized/codepoint checks are future validators;
whole-class submit/return/finalize, decimal half-up total, DRAFT atomic creation,
locks/authorization/audit history/is_unfinished update require later service/TX.
No GradeHistory, stored draft total/weight/pass flag or individual publication.

## Codex verification / skill_verification

Completed below: independent full signatures/defaults/precision/identity/RV/FK NO ACTION/
exact indexes/filter and NULL-safe final/submit/review/return bundles. D01–D04
signatures unchanged. Expected D05 adds29columns/10FK(7required,3optional)/11IX/
6CHECK/3RV; full19tables/180columns/19PK/2AK/43FK/62IX/52CHECKs/17RV.
Locked restore/build/full tests manager then main, exact reviewed tree equality,
package locks unchanged, disconnected DDL/closed connection only. No live SQL/
workflow enforcement or publication-readiness claim. D06 not started.

## Review PASS — manager 3ed9cf5, 2026-10-05

skill_verification: three AGY runs SUCCESS/exit0, exact2files/caps each, zero
observed scope violations. Full card140lines/backend46lines/routing101lines
viewed before first write; hashes/header/step proof retained in D05-evidence.
Audit helper normalized Windows path separators for Attendance's forward-slash
paths; no missing/ranged context read or broadened authority. Tool-event headers
are observed evidence, not OS read isolation or proof every line was understood.
Codex independently reviewed all six source files and wrote14 new test cases.

| Fragment | Run | Reads/writes | Runner diff lines |
| --- | --- | --- | --- |
| Attendance | 9572b2a2-311 | 7/2 | 109 |
| Score | 305abb61-ab3 | 7/2 | 139 |
| GradeReport | 0aa37e38-460 | 5/2 | 187 |

All dispatch base4ef8e58. Original worker trees remain at
%TEMP%/codex-antigravity-worker/worktrees/main_workplace-<run>; manager at
%TEMP%/codex-d05/manager. No observed commands/network/MCP. Codex made GradeReport
SQL types explicit, aligned BIN2 collation with Latin1_General_100_BIN2, and made
Attendance FK requiredness explicit. Removed unused usings. Global FK lookup
checker now admits nonunique leading-FK indexes filtered only by nullable-FK
IS NOT NULL while preserving unique completed-booking coverage. First regression
caught an accidentally removed checker exception; fixed before PASS. No changes
to existing production mappings/conventions beyond three registrations/DbSets.

Locked restore/build0warnings0errors/full454tests PASS/0fail/0skip, including
14new D05 cases. Full19tables/180columns/19PK/2AK/43NO ACTION FK/62indexes/
52CHECKs/17RV. D05 adds29columns/10FK(7required,3optional)/11indexes/6CHECKs/3RV.
D01–D04 signatures remain unchanged; all43FK lookup coverage verified. Score's
nullable Schedule FK does not fabricate kind/current/Class validation; the
filtered dependency lookup has exact accepted columns/filter. No default present/
score/entered timestamp or seeded missing rows. FinalScore nullable decimal(5,2)
is confined to FINALIZED by NULL-safe FinalResult including complete review and
same finalized/review time; complete latest submit required outside DRAFT.
Review bundle all-null/all-present and RETURNED reason/review mandatory, with
reason retained on resubmit. No stored draft total/weight/pass/GradeHistory.

Final scope10backend files/733insertions/9deletions; diff check clean. No package/
lock/project/Program/API/DI/profile/migration/SQL/seed/trigger changes. Closed
connection and disconnected generated DDL verified. Scope/backend/security review
PASS; migration/API/UI/workflow/live SQL checks N/A to mapping-only completion.
TG04/TG05/current Class/type/kind/role/window/report locks, parent RV touch,
whole-class all-or-none/audit/history/half-up calculation/publication and atomic
DRAFT creation remain future service/TX/SQL integration gates. No unresolved D05
blocker. Card supplies owner review notes; no route/API docs change required.

Owner explanation: Attendance stores an explicitly recorded present/absent value;
Score stores an explicitly entered component score including0; GradeReport keeps
submission/review metadata and only a finalized total. Missing rows remain missing,
and workflow services calculate/validate/publish later.
Reviewed manager3ed9cf54ab7e4432aa4e75901e6c8451e82cad8f, backend Git tree
2cfec2e8bac0415753cf8fb06d4d7f09b3e6ed67. Apply under ADR-163 then reverify main.
Next proposed D06 financial/audit/replay/counter mappings not started.


## Main application / reverified PASS

Main9e843b860e83da58da9006d91036f75de1d99162 under ADR-163; identical reviewed
backend tree2cfec2e8bac0415753cf8fb06d4d7f09b3e6ed67. Locked restore/build
0warnings0errors/full454tests PASS/0fail/0skip on main. Package locks unchanged;
compact TRX counters/hashes in D05-evidence/verification.json. No SQL/DB/migrations/
installation/push/deployment. No unresolved D05 blocker. D06 not started.
