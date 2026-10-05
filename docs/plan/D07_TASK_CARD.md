# D07 — model assembly and reviewed migration chain

Current status: D07 complete/reviewed PASS/applied main e4b8364; final D09 main
a7561dd reverified481unit+102SQL PASS. Historical authorization proposals below
were superseded by explicit owner scope; isolated database retained.

Historical D07A initial scope2026-10-05: ordinary model-assembly code/preflight only.
D07A below does not create migration, install tooling, connect/create/alter DB,
execute SQL, or open D08–D13/runtime/API/deployment. At that historical checkpoint family D07 was incomplete.
AGENTS required explicit migration/isolated SQL authority, subsequently granted below.
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
full tests manager then main and exact reviewed source equality. Completed below.

## Historical D07B proposal — later explicitly authorized/completed below

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

## Historical D07C proposal — later explicitly authorized/completed below

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


## D07A main application / reverified PASS

Main378431aeb677e405d9044e15fa27965fc189b8cc under ADR163; reviewed backend tree
3efa3ce638578514481f77bd41c38394f7067c06 identical. Locked restore/build clean/
full478tests PASS/0fail/0skip on main; package locks unchanged. Compact TRX/hash
proof in D07A-evidence/verification.json. No migrations/SQL/tool activation.
D07A complete; family D07 incomplete, D07B/C proposal awaits explicit scope.

## Owner continuation — D07 → D08 → D09, 2026-10-05

Owner requested three families and explicitly selected D07→D08→D09. D07B offline
Design/private dependency/local tool10.0.4 and EF-generated migration are authorized.
D07C SQL creation/execution was rejected by automatic approval review: family
selection alone was not accepted as explicit SQL authority. Concrete bounded SQL
request pending: new fail-if-exists EnglishCenter_D07_20261005 only, compatibility160,
accepted collation/snapshot ON, empty schema Up→Down→Up, five triggers/six UDFs,
transactional fixture validation; no existing DB changes/drop DB/grants/real seeds.
No SQL command executed. Continue detached code and offline validation while pending.
D08/D09 each object gets a separate bounded AGY contract, complete backend skill/routing
and self-contained object task card; no commands/SQL/network/Git/extra reads.

## D07B/C reviewed PASS / owner SQL authority

Owner explicitly approved the bounded new isolated database, profile, empty schema
Up→Down→Up and D08/D09 rollback-fixture SQL verification. Initial automatic rejection
preceded this explicit authority; no rejected SQL operation executed. D07 applied
main e4b8364 from manager b90aeaa: offline cache-only Design/private10.0.4/local tool,
disconnected factory160, generated InitialSchema/designer/snapshot,3independent
migration tests. No pending model changes. 481tests PASS/build0warnings0errors.
Live SQL Up→Down→Up PASS on fresh EnglishCenter_D07_20261005;24tables/230columns/
24PK/2AK/50NO ACTION FK/78IX/70trusted CHECK/20RV; collation exact/compatibility160/
snapshotON and SNAPSHOT read PASS. No seed/real data/grants/runtime/other DB changes.
First sqlcmd execution lacked QUOTED_IDENTIFIER; transaction rollback verified,
then -I applied and full chain reverified. No migration source change needed.
D07Schema.sql retained for pre-D08 verification. Database retained, no DROP DB.
D08/D09 are next authorized families, not evidence of app callers or D13 readiness.


## Main application / reverified PASS

Applied reviewed D07 under ADR163: maine4b8364, managerb90aeaa. Main481unit PASS/build clean; isolated Up→Down→Up/catalog/profile/snapshot PASS.20new Design dependency licenses/actual NuGet SDK normalized hashes vs lock/assets/cache verified; local tool10.0.4 normalized hash verified (legacy raw hash sidecar not used). Private Design absent from publish-check. See D07-evidence/package-verification.json; final D09 also verifies all230live column shapes.
