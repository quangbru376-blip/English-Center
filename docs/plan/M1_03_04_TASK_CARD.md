# M1 packages 3–4 — assigned 2026-10-09
Status Codex review PASS; apply/main/actual reverify pending. Starts main81bc7fb; manager .worktrees/m1-next.
Scope: Program API081–089 +090 sixProgram actions; AcademicAffairs entry sessions
API054–065 +045 sixSession actions. Retain previous Course4/Admin APIs.
No Teacher grading066–073, Student booking/Registration UI, admissions decisions,
Classes/payments/reports/audit viewer, schema/grants/packages/install/push/deploy.
Canonical accepted requirements/E04 override historic draft labels and skill defaults.
implementation_skills: backend-clean-code, frontend-ui; review_skills: code-review.
skill_sources: complete .agents/skills/*/SKILL.md; SKILL_ROUTING complete;
backend76ACEC868ED4D918DC1C6616904F78FF2710737D6A06CCF605DA783BC914DD23;
frontend54EFB0F14763ACA58DD1BDADC8B5276DA082E0668244335C7B8C48E0521402DF;
review1DDED1E6699F97C7657C27D2B923992D07AC88BD9642FB74EBE4D93A28BE2125;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
applicable_overrides: exact DTO/routes/full manifests/decimal/stringmoney/SQLServer/
JWT-cookie/current session/frameworkCSRF/nativeUI/VNtime/44px/no package.
read_context: ordered memory/requirements/E04 DTO12,15/catalog/SQL/replay/time/
existing services/models/TG01–03/SP02/FN01/current auth/audit/replay/fixtures/UI.
skill_verification: Codex independently reviewed canonical scope/DTO/auth/locks/
SQL/atomic audit/replay/RAM/native UI; manager build/types/lint/unit/ownedHTTP/Edge PASS.
Program: DRAFT incomplete allowed; full source/target component guards, atomic
swap-safe DML, unique pair/names/max1exam, exact100+1examactivation, Class-derived
freeze including CANCELLED, rename/lifecycle separate, B+P gates/current relations.
Sessions: future same-day VNminute create CLOSED; TIMETABLE+sorted REG gates,
sorted involved Accounts→SIDs→domain locks, activeTeacher/cross-Class+Session
half-open resource guards. Full cancellation/grader preview versions/bindings;
membership/time recheck; cancellation effects same audit/operation, no Registration
mutation/rebook/score; coherent SP02 roster/current references/status-only Invoice.
UI native/shared Vietnamese forms, complete component set, saved-only activation,
reasoned preview confirmations, explicit recovery/no automatic retry/no secretstorage.
Write families: Services/Academics, Infrastructure/Academics, Api/Academics;
minimal LocalRuntimeServices/Course lookup/http/navigation integration;
frontend/features/programs and entrySessions, existing app/lib; isolated tests;
api/notes/card/evidence/runbook and affected Project Memory only. Exact final paths
recorded before review. Owner4 metadata hashes only; never read/share/stage contents.
AGY: separate pure ProgramComponents and EntrySessionForm contracts, each one
file≤130lines, read sole public worker spec +frontend skill+SKILL_ROUTING fully.
No source/report/owner/private files, commands/network/SQL/Git/validation. Codex
owns integration/sensitive operational work/review/testing/apply ADR163 under177.
Testing: parsers/manifests/first-use/swap/replay/rollback/auth/time/overlap/history/
full dependent cancellation/races; owned scratchID+GUID cleanup, HTTP/Edge3widths;
full regressions. Actual existing LOCAL_DEV all9/strictTLS mount recheck, no fake
operational Program/Session/Account data. Package5/later stays unassigned.

## Final review write allowlist
backend/src/EnglishCenter.Api/Api/Academics/EntrySessionEndpoints.cs
backend/src/EnglishCenter.Api/Api/Academics/ProgramEndpoints.cs
backend/src/EnglishCenter.Api/Common/Auditing/AuditPolicy.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlAcademicRepository.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlAcademicTransactions.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlEntrySessionExecutor.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlEntrySessionRepository.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlEntrySessionSearch.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlProgramExecutor.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlProgramRepository.cs
backend/src/EnglishCenter.Api/Infrastructure/Academics/SqlSessionRoster.cs
backend/src/EnglishCenter.Api/Infrastructure/Catalogue/SqlCourseExecutor.cs
backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs
backend/src/EnglishCenter.Api/Services/Academics/AcademicAuthority.cs
backend/src/EnglishCenter.Api/Services/Academics/AcademicInput.cs
backend/src/EnglishCenter.Api/Services/Academics/EntrySessionModels.cs
backend/src/EnglishCenter.Api/Services/Academics/EntrySessionService.cs
backend/src/EnglishCenter.Api/Services/Academics/ProgramModels.cs
backend/src/EnglishCenter.Api/Services/Academics/ProgramService.cs
backend/tests/EnglishCenter.Tests/Api/Integration/M1HttpSqlTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/M1ProgramSessionHttpTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/M1ProgramSessionSqlTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/OwnedBrowserSqlHostingTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/OwnedSqlFixture.cs
backend/tests/EnglishCenter.Tests/Common/M1AcademicInputTests.cs
docs/api.md
docs/notes/M1_03_04.md
docs/plan/M0_LOCAL_DEV_RUNBOOK.md
frontend/src/app/IdentityApp.tsx
frontend/src/app/identityNavigation.ts
frontend/src/features/entrySessions/components/EntrySessionForm.tsx
frontend/src/features/entrySessions/pages/EntrySessionPage.tsx
frontend/src/features/entrySessions/sessionApi.ts
frontend/src/features/programs/components/ProgramComponents.tsx
frontend/src/features/programs/pages/ProgramPage.tsx
frontend/src/features/programs/programApi.ts
frontend/src/lib/academicWire.ts
frontend/src/lib/http.ts
frontend/src/lib/m1Wire.ts
frontend/tests/browser/verify-m1.cjs
frontend/tests/m1-next.test.mjs
tools/operator/Verify-M0Browser.cjs
## Codex review — PASS 2026-10-09
No remaining review findings in the assigned scope. Existing schema/migrations,
roles/grants/default Program/packages/owner files unchanged. Source/target lock
union, final-projection cutoff, strict malformed inputs and native label fixes
completed before verdict. Manager build0warnings0errors; default1306PASS/126opt-in
SKIP; 39focusedPASS (15default inputs +24ownedSQL/HTTP including2previous cases).
New2Edge groups PASS at375/768/1280 incl strictTLS negatives, real ownedCOMMIT,
truncated postcommit response/manual lookup/no automatic resend. Eight previous
browser groups also PASS in retained manager-final run; combined10groups covered.
Frontend63PASS/types/lint/build PASS. Raw failed attempts retained; no failed output applied.
AGY00faca0b-526 and be232215-a36: observed sole authorized3fullreads/soleformwrite,
no observed commands/network/SQL/extra context. Both rejected133diffLines>130limit;
worker source never copied/applied. Codex independently implemented the two forms.
142manager scratch DBs created/dropped identity-matched at review checkpoint.
Write allowlist additionally includes this task card; final closure memory/evidence
is limited to AGENTS,CURRENT_STATE,TODO,DECISIONS,ARCHITECTURE,plan/plan,
implementation-plan,NEXT_CHAT_HANDOFF,HANDOFF and M1-evidence/M1-next-summary.json.
Actual LOCAL_DEV principals/keys/strictTLS/all9 mount reverify follows main apply;
positive AcademicAffairs flows use isolated committed scratch fixtures only.
No actual fake Program/Session/Teacher/student/Course rows or new business family.
