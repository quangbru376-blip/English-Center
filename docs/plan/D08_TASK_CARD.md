# D08 — five accepted triggers

Owner authorized D07→D08→D09 and explicitly approved isolated SQL execution on
EnglishCenter_D07_20261005, rollback fixtures, no other DB changes/drop DB/grants.
Implementation skill backend-clean-code; review code-review; AGY protocol.
Source hashes unchanged: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
Accepted E03/E04 SQL Server160/money/time/multirow and scope override stale skill
SQLite/general API/seed rules. No services/auth/runtime/role grants added.

## Read context and bounded workers

Codex read ordered memory, requirements, DATABASE_SPEC10.1, SQL integration4/6,
M0 quality and entity/config/migration source. Five independent single-object
AGY edit contracts; each COMPLETE its D08_TGxx_WORKER.md/backendSKILL/routing,
only these reads plus exact output if present. No commands/SQL/Git/network/
MCP/secrets/search/other reads allowed. Each1file/cap220lines; all five SUCCESS/
exit0,54/72/70/71/94runner lines,5reads1write each; zero observed violations.
Complete context-before-write path/hash/header proof, receipts/contracts/audits
retained D08-evidence. Tool events do not prove OS isolation or comprehension.
Original detached worker trees retained; manager %TEMP%/codex-d08/manager.

## Implementation and review PASS

TG01 used Program course/Band/lesson count/deletion guard, harmless metadata
and no-op allowed. TG02 both sides of component inserts/deletes/moves/values,
exact BIN2/name length. TG03 completed result freezes entire Band set. TG04
Attendance valid current LESSON/same Class/non-NOT_HELD and old/new ended Class.
TG05 same program/current-kind Schedule/GENERAL NULL and old/new submitted/
finalized report lock. Set-based THROW/no audit/result set/own transaction.
Caller must prehold Exclusive Transaction public canonical PROGRAM/BANDSET/
CLASS gates BEFORE DML; trigger verifies APPLOCK_MODE, no late gate acquisition.
First-use/result/Class lifecycle writers must follow same protocol in later slices.
No claim that arbitrary direct parent writes or SNAPSHOT writer paths are safe.

Manager fixed TG01 worker checking gate only for core change: exact contract/E04
requires PROGRAM gate for every affected Program, including rename/status. Other
four production scripts unchanged. EF-generated ConfigurationAndAcademicGuards
migration uses immutable versioned embedded SQL, escaped constant EXEC batch for
SQL CREATE's batch restriction; Down drops only five objects in reverse order.
No physical model change or fake HasTrigger; OUTPUTfalse exactly existing5tables.
SQL tests are separate project (same already pinned xUnit/SDK packages), explicit
run only, hardcoded isolated target/diagnostic Windows auth/trusted local cert;
no app connection policy or startup migration. All data fixture transactions
rollback; fixture setup is relational test data, not implemented business workflow.

Codex independently built0warnings0errors/full481unit tests and50SQL tests PASS/
0fail0skip, no pending model changes. SQL cases: used/cancelled/mixed configuration,
case-only names/code, unused scratch edits, full-set freeze after completion,
wrong Class/type/noncurrent/NULL/component/report state, old-side move/delete,
valid generated ID/RV, missing gates and two-connection finite gate timeout/release.
Catalog exact5enabled triggers/14events/correct tables; physical24tables/230cols/
50FK/78IX/70checks/20RV retained. D08 Down→D07 schema verification→Up PASS;
fixture Course/Role/Account/Band empty. Review scope/security/backend PASS.
D13 readiness/full writers/current auth/audit/grants/TX proofs remain future gates.
Apply exact reviewed snapshot under ADR163, then independently reverify main.


## Main application / reverified PASS

Applied reviewed D08 under ADR163: mainbad4b65, manager514bcea. Main481unit+50SQL PASS/build clean, exact reviewed tree; five enabled triggers/events and rollback verified. Final D09 extends trigger suite to56cases, allPASS.
