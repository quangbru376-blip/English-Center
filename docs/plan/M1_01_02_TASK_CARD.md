# M1 packages 1 and 2 — assigned 2026-10-09

Status: COMPLETE; Codex review PASS/applied ADR163/reverified2026-10-09.
Main code8ef95cc / manager6315b09; genuine LOCAL_DEV acceptance PASS.
Owner assigns full code/review/tests/PASS application; ADR177/163 apply.
Manager checkout .worktrees/m1-packages starts at d46ff8a; owner4 absent here.

Scope: API006–014 staff search/create/reset/lock/unlock/disable/restore;
API074–080 Course CRUD-by-contract/lifecycle and full read-only seeded Band;
API090 lookup restricted to four Course actions. No Band mutation, programs,
admissions, payment, classes, M2–M4, schema/package/install/push/deploy changes.
Reuse session/CSRF/deadline/Account->SID locks, EC-C1/T23 and mandatory audit.
Staff secrets first response after owned COMMIT only, replay safe completion,
32 random characters/24h/must-change, reset preserves status and revokes all.
ADMINSET protects self/last active Admin; optimistic versions and no repeated effects.
Course code immutable/unique, exact positive VND string, no-op400, no cascading
historical changes; Band full coherent DB set/frozen flag without GET repair.
Reads: initial live authority, separate SNAPSHOT buffered data, final live authority.
UI: real role-scoped endpoints, search PII body/RAM, one-time credential clear/copy/
ack/expiry/logout, explicit confirmations/unknown manual lookup/no automatic retry.

implementation_skills: backend-clean-code, frontend-ui
review_skills: code-review (Codex)
skill_sources: .agents/skills/*/SKILL.md; hashes recorded before delegation.
applicable_overrides: SKILL_ROUTING §2; accepted E04/requirements override generic
REST/bearer/SQLite/pagination/money rules. No new packages or business limits.
read_context: ordered memory, requirements, E04 DTO/catalog/final/replay/SQL,
W01/W03/W13, existing identity/admin/SQL/UI code and relevant test fixtures.
skill_verification: complete Codex diff/contract/security/read-event review and
independent build/tests. See verification below; canonical overrides retained.

AGY: sole pure Course form, <=130 lines/one file; read only M1_COURSE_FORM_WORKER,
frontend-ui/SKILL.md and SKILL_ROUTING; no source/owner/private files/commands/network/
SQL/Git. Standing assigned-aspects authority covers this explicit reduced context.
Codex retains all sensitive transaction/runtime work and final review/tests/apply.

Verification: full existing backend/frontend regression; meaningful parser and
service guards; real isolated owned-COMMIT SQL/API tests (existing scratch fixture
with ID/GUID checked cleanup, never D07/operational fake data), rollback/audit/replay/
stale/role/session/self/last-admin/races, actual Edge 375/768/1280 UI checks.
Explicit LOCAL_DEV host mounts these two modules; no-argument host stays unmounted.
Existing five manifest pins cover package/global.json files and remain unchanged;
no re-pin is needed. Built UI files are freshly pinned by the existing host lifetime.
Band reference initialization is an explicit schema-identity operator command,
never GET/default startup repair. Protected keys/credentials never shared.
Owner4 metadata hashes before/after and exact staging allowlists mandatory.

## Review and independent verification
Codex verdict PASS2026-10-09 for implementation; no outstanding must-fix finding.
No migration is applicable: existing24-table model/constraints/grants unchanged.
Thin adapters, service authority/deadline/RV/lock/replay guards, exact DTOs, decimal
internal money/string wire, caller-owned mandatory audit+T23 and post-COMMIT first
credentials reviewed. Secrets are absent from replay/audit/ToString/log/storage.
Independent periods/profile/keys/CSRF/cookies remain existing accepted M0 behavior.
Frontend reuses native/shared UI; Vietnamese/3widths/keyboard/clear/no-auto-retry.
Review fixes: exact audit numeric/status whitelist, blank query400, accepted first-F
landing preserved, UTC/target credential decoder, stale-command detail clearing,
deep-link detail and self-reset post-handoff session check, EC tokens/focus/pending.
Seed accepts only empty or exact5Bands; partial set refuses. Same TX stores5 secret-
free mandatory SYSTEM.BAND.SEED rows; audit failure rolls back all reference rows.

Manager build0warnings0errors;1291defaultPASS/102intentional opt-inSKIP.
Frontend56PASS/0fail0skip;lint/types/build PASS. Owned verification:20 SQL cases,
2 HTTP→ownedSQL cases and8 Edge→ownedSQL groups all PASS (30 unique cases).
Last aggregate29PASS/1test-assertionFAIL was corrected to exact pre-existing
MandatoryAuditException; final individual audit case PASS including seed rollback.
The two final M1 Edge groups were rerun against final assets:2PASS; all3widths.
96 scratch databases created/ID+GUID-matched dropped,0unmatched; never operational
fake staff/Course or D07 mutation. Original failed receipts remain evidence.
SQL races cover mutually disabling Admins and duplicate Course code; mandatory
failure, stale RV, wrong roles, secret replay and status-preserving reset verified.
Whole future M1/product55 routes, LAN/load/backup/deploy are outside this verdict.

AGY18075799-be1 SUCCESS129diffLines/soleCourseForm; exact3full reads/solewrite,
no observed out-of-scope read/command/network/SQL. Codex changed pending/style.
Receipt SHA256 6A6B1472430E746C8B37683B19D0CFD86F3C5D31946A06ABA99D4ECA2CC11BEF;
events FC8802460135A9E3CC95D40F3201C2293757D2A721C4DDC3A1D66F8A50A1B4E2.
Skill SHA256: backend76ACEC868ED4D918DC1C6616904F78FF2710737D6A06CCF605DA783BC914DD23;
frontend54EFB0F14763ACA58DD1BDADC8B5276DA082E0668244335C7B8C48E0521402DF;
review1DDED1E6699F97C7657C27D2B923992D07AC88BD9642FB74EBE4D93A28BE2125;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.

## Main application and actual LOCAL_DEV
ADR163 apply main8ef95cc; manager6315b09 identical backend src
b51b8ef26989cd3d86973d42b7854c633e1379fe / tests
8a1c7de8717342da8377f2cb0c1c42ff9cef1693 / frontend
6cc7baf922f49e56e69c14bda5235ee85140f418.
Main1291defaultPASS/102intentional opt-inSKIP/build0warnings0errors;
24new ownedSQL/HTTP/Edge cases PASS0fail0skip; frontend56/lint/types/build PASS.
Actual EcM0Schema strict TLS/catalog identity/M1-band-reference initialization PASS;
second invocation verifies exact set readonly, no repeat DML/audit. Fresh SQL proves
7roles/oneunchangedACTIVEAdmin/Profile0/Band5/Course0/6migrations/activeSID0,
5Band mandatory audits/exact1operation. No operational fake staff/student/Course.
Actual genuine runtime+maintenance fresh all9/strict HTTPS Edge3widths/Admin4login/
4logout/self-detail/self-lock-disabled/multi-context/manual404 PASS. ReplayCOMMIT1,
explicit auditCOMMIT1/failed0; maintenance stop503/forcedStops0; listener stopped.
Readonly diagnostic initially refused legacy hostname/reference/native driver
loading; final existing Windows SqlClient/SNI with canonical strict hostname PASS.
No trust bypass/driver installation or runtime configuration change was made.
All new24 main scratch databases identity-cleaned. Manager96+main24=120created/
120dropped,0unmatched. Owner4 hashes unchanged/unshared/untracked/unstaged.
D07 untouched, no rerun baseline claim. No schema/grants/packages/push/deploy.
Whole M1 remains IN PROGRESS; package3 and later require a new owner assignment.
Evidence: M1-evidence/M1-summary.json; raw TRX/ownership receipts remain ignored.

## Reviewed write allowlist
backend/src/EnglishCenter.Api/Api/Administration/AdminAccountEndpoints.cs
backend/src/EnglishCenter.Api/Api/Administration/StaffAccountEndpoints.cs
backend/src/EnglishCenter.Api/Api/Catalogue/CourseEndpoints.cs
backend/src/EnglishCenter.Api/Common/Auditing/AuditEntry.cs
backend/src/EnglishCenter.Api/Common/Auditing/AuditPolicy.cs
backend/src/EnglishCenter.Api/Infrastructure/Administration/SqlAdminAccountRepository.cs
backend/src/EnglishCenter.Api/Infrastructure/Administration/SqlStaffAccountExecutor.cs
backend/src/EnglishCenter.Api/Infrastructure/Catalogue/SqlCourseExecutor.cs
backend/src/EnglishCenter.Api/Infrastructure/Catalogue/SqlCourseRepository.cs
backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalBandReferenceSeed.cs
backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs
backend/src/EnglishCenter.Api/Program.cs
backend/src/EnglishCenter.Api/Services/Administration/AdminAccountService.cs
backend/src/EnglishCenter.Api/Services/Administration/StaffAccountInput.cs
backend/src/EnglishCenter.Api/Services/Administration/StaffAccountService.cs
backend/src/EnglishCenter.Api/Services/Catalogue/CourseModels.cs
backend/src/EnglishCenter.Api/Services/Catalogue/CourseService.cs
backend/tests/EnglishCenter.Tests/Api/Integration/M1HttpSqlTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/M1StaffCourseSqlTests.cs
backend/tests/EnglishCenter.Tests/Api/Integration/OwnedBrowserSqlHostingTests.cs
backend/tests/EnglishCenter.Tests/Common/M1InputTests.cs
docs/notes/M1_01_02.md
docs/plan/M1_01_02_TASK_CARD.md
frontend/src/app/IdentityApp.tsx
frontend/src/app/identityNavigation.ts
frontend/src/features/administration/components/StaffCreateForm.tsx
frontend/src/features/administration/pages/StaffAdminPage.tsx
frontend/src/features/administration/staffApi.ts
frontend/src/features/catalogue/catalogueApi.ts
frontend/src/features/catalogue/components/CourseForm.tsx
frontend/src/features/catalogue/pages/CataloguePage.tsx
frontend/src/lib/http.ts
frontend/src/lib/m1Wire.ts
frontend/src/lib/useOwnedTask.ts
frontend/tests/browser/verify-identity.cjs
frontend/tests/browser/verify-m1.cjs
frontend/tests/m1.test.mjs
tools/operator/Initialize-M1Bands.ps1
tools/operator/Verify-M0Browser.cjs
