# M1 packages5–6 — assigned2026-10-09
Status IN PROGRESS. Start mainf764b87; manager .worktrees/m1-student-teacher.
Package5 Student own profile/home/course selection/Registration/entry booking,
calendar/results/confirmation APIs019–037. Package6 Teacher current entry tasks
APIs066–073: initial score, explicit absence and reasoned original-grader correction.
No Admissions approval/rejection/cancellation APIs038–044, Invoice UI046–047,
Class/training/Attendance/gradebook/report/M2–M4/schema/grants/packages/install/push/deploy.
Existing Home own invoice/class/finalized references are read-only required projections,
not new mutation/routes or additional roles. No synthetic operational row promotion.
implementation_skills: backend-clean-code, frontend-ui; review_skills: code-review.
skill_sources: complete local .agents/skills/*/SKILL.md and SKILL_ROUTING;
backend76ACEC868ED4D918DC1C6616904F78FF2710737D6A06CCF605DA783BC914DD23;
frontend54EFB0F14763ACA58DD1BDADC8B5276DA082E0668244335C7B8C48E0521402DF;
review1DDED1E6699F97C7657C27D2B923992D07AC88BD9642FB74EBE4D93A28BE2125;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
applicable_overrides: exact E04 DTO/routes/canonical55/SQLServer/cookie/CSRF/native
VNtime/44px/full sets/decimal strings/RAM/no package. Skill old defaults not authority.
read_context: ordered memory/requirements/accepted E04 DTO4,5,8,9,13 and registry/
SQL6–7/FN02–03/TG01–03/SP02/existing auth/audit/replay/entities/fixtures/UI.
skill_verification: pending Codex independent code review/build/tests/realSQL/HTTP/Edge.
Student actor resolves own Profile, nested ownership before tokens; immutableCourse,
filtered unfinished uniqueness, complete own history/attempt≤2/current vs latest.
OPEN future sources/targets; last-seat/student cross-Class+entry conflicts under
TIMETABLE; reuse ABSENT/CANCELLED logical attempt, second only after first score;
reschedule cancel+new same attempt atomic. No zero from null/no auto absent.
Student cancellation full future Booking/Session/Invoice manifests incl CLOCK drift,
CLOSED future effects; preserve started/history/score/Band; mandatory effects/T23 TX.
Confirm full attempts/currentBookings/Band snapshot, reject pending retake, no Invoice.
Profile full Account/Profile RV and phone uniqueness, no securityVersion/revoke/replay.
Teacher current assignment mandatory for every list/detail/command/replay/lookup;
SP02 exact Teacher columns excludes invoiceStatus entirely. Full related≤2 manifests,
current/completed binding/Band presence guards. Grade/absence endAt inclusive, CLOSED
allowed/CANCELLED blocked/pre-confirmation; absence preserves Attempt/Band.
Initial grade atomically Booking/Attempt/Band/Registration audit/T23; FN02 underBANDSET,
best tie attempt1; correction original+current grader, reason, keep initial provenance,
same-value400 only after guards; all data/time/audit/RV races rechecked beforeCOMMIT.
R→sorted existing business gates→involved Accounts→binarySID→domainTxx/PK;
trusted resources discover/reload refusal beforeDML, no out-of-order gate expansion.
Reads live authority→SNAPSHOT→final live authority, no renewal/mutation/polling.
UI own Student context/explicit forms/preview confirmations and Teacher task scope,
PII body/RAM only, exact DTO decoding, unknown manual lookup/no autoresend.
Write families: Services/Enrollment, Infrastructure/Enrollment, Api/Enrollment;
minimal Academics shared authority/roster models and LocalRuntimeServices mount;
frontend/features/enrollment and entryTasks, existing app/lib; isolated tests;
api/notes/card/evidence/runbook and affected Project Memory only. Final exact allowlist
recorded before review. Owner4 metadata only; preserve untracked/unshared/unstaged.
AGY two independent pure StudentProfileForm and EntryScoreForm contracts, ≤120lines/
≤200diffLines each. Only own public worker spec +frontend skill +routing full reads;
no source/report/owner/secrets/commands/network/SQL/Git/validation. Codex integration.
Verification: ownership/wrongrole/restricted, profile uniqueness/RVs/no revoke,
unfinished/last-seat/reschedule/cancel-clock/end-time/retake/absence/zero/tie/correction/
confirm/cancel races/audit failure/replay/assignment loss/Teacher privacy, unit/owned
SQL/HTTP/strictHTTPS Edge3widths/full regression; scratchID/GUID cleanup.
Actual existing LOCAL_DEV all9/strictTLS/route guards reverify; fake fixtures scratchonly.
Owner ADR177 covers assigned aspects/context/verification, ADR163 apply reviewedPASS;
no repeated gates. Full task incomplete until review/apply/main/operational closure.

Foundation checkpoint2026-10-09 IN PROGRESS; full-batch review FAIL/incomplete,
not applied main. Main stillf764b87/code564343b packages1–4. Current manager reused.
Implemented unmounted OwnProfileInput/SqlOwnProfileExecutor, read/lock repository,
role read/write transactions, Teacher manifests/projections/service/command executor,
changed-field-only profile audit and missing TESTING/BAND_ASSIGNED audit enums.
No Api/Enrollment adapters, runtime mount, Student commands/read executor, Teacher
read/SP02/lookup executor or integrated role pages/HttpClient registration yet.
See M1_05_06_REVIEW.md for precise remaining work; do not open another batch.
AGYe1112347-32d SUCCESS96diffLines/StudentProfileForm;09ee3daa-93d SUCCESS88diffLines/
EntryScoreForm. Observed exactly full worker10lines/skill44lines/routing101lines
reads and sole authorized writes per run; no commands/network/SQL/extra context.
Codex fixed named UI imports and score paragraph token; six new form tests PASS.
Original worktrees/receipts/events retained in user Temp; no raw events copied.
Manager35new RAM contracts/1341defaultPASS133opt-inSKIP, seven current owned SQL
foundation cases PASS, frontend69/types/lint/build PASS/buildclean. SQL earlier5+7
and final7 runs created19GUID scratch databases, all ID/GUID-paired dropped.
SQL includes unique-phone rollback/no revoke, audit rollback, zero/correction/
absence/replay/equality/current assignment/malformed-body precedence/two-client grade.
It does not establish Student workflows/HTTP/browser/runtime-principal readiness.
Owner4 hashes verified unchanged/unshared/untracked/unstaged; D07/runtime untouched.
Source families added only the explicitly authorized Enrollment directories and
two audit primitives; tests only M1EnrollmentContractTests/M1EnrollmentFoundationSqlTests/
m1-enrollment-forms.test.mjs; frontend only two pure components. Docs/evidence only.
Next continue Student Enrollment service and repository within this contract,
then complete Teacher reads/lookup/HTTP/UI and full verification/apply main.
