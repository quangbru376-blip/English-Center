# D07 — model assembly and reviewed migration chain

Continuation2026-10-05: authorized ordinary model-assembly code/preflight only.
D07A below does not create migration, install tooling, connect/create/alter DB,
execute SQL, or open D08–D13/runtime/API/deployment. Family D07 incomplete.
AGENTS latest explicitly requires migration/isolated SQL scope authorization.
Implementation skills backend-clean-code; review code-review; integration AGY.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
Overrides: accepted E04 SQL table-scoped OUTPUT false exactly five affected
Tables; no global disable or fake HasTrigger/trigger SQL. No SQLite/migration
checklist expansion into unauthorized scope. ADR163 applies authorized PASS code.

## D07A bounded worker / read_context

Codex read AGENTS and ordered memory, requirements/money/replay baseline,
HANDOFF/FIRST_CODE/M0/D06, accepted E04_SQL_INTEGRATION_SPEC EF section.
Worker VIEW COMPLETE this card, COMPLETE backend SKILL, COMPLETE routing before
edit. Read ONLY these three context paths and five allowed config files below.
No links/AGENTS/memory/other source/tests/search/scans/commands/Git/network/MCP/
browser/subagents/secrets. Only view_file and authorized write/replace tools.
Five existing files under backend/src/EnglishCenter.Api/Infrastructure/Configurations:
BandConfiguration.cs, TrainingProgramConfiguration.cs,
ScoreComponentConfigConfiguration.cs, AttendanceConfiguration.cs, ScoreConfiguration.cs.
Only change each table-builder mapping to call UseSqlOutputClause(false).
Existing ToTable callbacks: add one call using existing table variable. Attendance
simple ToTable must become callback with same dbo table name. Preserve schema,
columns/defaults/checks/indexes/FKs/relationships/precision/identity/rowversions.
No HasTrigger, SQL/trigger bodies, service/DbContext changes, new files/packages.
Cap5changed files/80diff lines. Codex writes ModelAssemblyTests.cs independently.

## D07A verification / skill_verification

Expected full signature unchanged24tables/230columns/24PK/2AK/50NO ACTION FK/
78indexes/70CHECKs/20RV. Runtime and design-time OUTPUT metadata exactly false
on Band/TrainingProgram/ScoreComponentConfig/Attendance/Score and true on other19;
SQL table metadata check confirms no global disable. No trigger objects claimed.
Closed connection/generated DDL, old physical signatures, locked restore/build/
full tests manager then main and exact reviewed source equality. Pending.

## D07B concrete proposal — migration/tooling authorization pending

Observed2026-10-05: no local/global dotnet tool registrations; NuGet cache contains
Microsoft.EntityFrameworkCore.Design10.0.4 and dotnet-ef10.0.4. API references only
SqlServer10.0.4. No factory/migrations or SQL project profile exists.
Proposed bounded offline scope: private build-only Design10.0.4 dependency and
backend-local tool manifest dotnet-ef10.0.4 using verified cache; no global install
or network download unless separately needed/authorized. Manager handles manifest/
locks/tool activation/generation. Design-time factory uses disconnected fixture
connection/provider compatibility160, no runtime DI/credentials/auto-migration.
Generate initial migration/designer/snapshot and review creation order/indexes/
NULL-safe checks/identity/types; add deferred cyclic result FK after both Attempt
and Booking created. InitialDown must handle cycle before dropping tables.
Generate and review SQL only, no DB execution; test empty→model schema operations,
pending-model changes/rollback script and locked build/full tests. Apply code
only after Codex PASS under ADR163. D07B is NOT started/authorized by this card.

## D07C later isolated SQL scope — authorization pending

Proposed target .\SQLEXPRESS / EnglishCenter_D07_20261005, fresh empty disposable
project database only; fail if name already exists, never adopt/reset/drop an
existing database. Windows-authenticated manager runs scoped setup/migration/
schema checks; compatibility160/collation Latin1_General_100_CI_AS_SC/snapshot ON
only in that new DB. No existing user DB/instance settings/SQL update/grants/seeds/
D08triggers/UDF/views/SP/runtime connection/push/deployment. Keep DB for evidence,
no DROP cleanup until separately authorized. Schema/SQL execution does not prove
all business triggers/TX/readiness; D07 complete only after required gates pass.

## D07A review PASS — manager8474804, 2026-10-05

skill_verification: AGY56d6e3a1-81f SUCCESS/exit0, exact5config files/13runner
diff lines,8reads/5writes, zero observed scope violations. Complete card70lines/
backend46lines/routing101lines viewed before first write; compact path/hash/
step/header proof in D07A-evidence. No observed commands/network/MCP. Tool events
are not OS read isolation or proof every line understood. Dispatch base755fbce;
worker %TEMP%/codex-antigravity-worker/worktrees/main_workplace-56d6e3a1-81f,
manager %TEMP%/codex-d07a/manager, both retained. No manager production fixes.

Codex independently reviewed all five diffs against E04 EF trigger integration;
only table-scoped UseSqlOutputClause(false), no HasTrigger/SQL/schema changes.
Four new cases verify exact disabled set and19enabled tables at runtime/design/
relational metadata, absence of invented triggers and unchanged physical model.
Locked restore/build0warnings0errors/full478tests PASS/0fail/0skip. Original
24tables/230columns/24PK/2AK/50FK/78IX/70CHECK/20RV retained; old signatures and
FK lookup coverage PASS. Final6backend files/72insertions/1deletion, diff clean.
No packages/tool activation/migrations/SQL/DI/API/profile changes. Closed DB/
generated DDL verified. Scope/backend/security review PASS; live EF generated
ID/RV with real triggers, migration chain and SQL remain unverified D07/D13 gates.
This is D07A completion only; full D07 incomplete. Card supplies owner notes.

Reviewed manager8474804913ae1b7784df915c67266fe564f6db64; backend Git tree
3efa3ce638578514481f77bd41c38394f7067c06. Apply only authorized metadata/test code
under ADR163 then reverify main. D07B migration/tooling and D07C SQL await explicit
scope approval described above; no D08–D13 scope opened.
