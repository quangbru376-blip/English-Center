# V01-B → V01-C — opt-in HTTP/SQL verification

2026-10-08; base main483dd01. Owner requested next two tasks. These are bounded
verification substeps of V01, not new product families. Status: completed/reviewed
PASS/applied ADR163/reverified2026-10-08 code mainbca0e4e/manager7ed9f85;
identical six-file test tree32decf9a213277debd9e6c9cc17783efec711aea.
Owner explicitly approved both AGY payloads and both exact rollback SQL scopes.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
skill_sources SHA256: backend726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b;
review04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70;
routingadb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915;
AGYeb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b.
Complete relevant skills read; canonical overrides exact DTO/errors/cookie JWT/
framework CSRF/live SQL authority/caller-TX/SQLServer160, no SQLite/new packages.
read_context: ordered memory, requirements identity/Admin, M0/V01/hand-offs,
E04 runtime/DTO and implemented endpoints/services/repositories/existing tests.
skill_verification: Codex checks actual worker events/diff and all required tests.

## V01-B

Manager writes test-only IntegrationSqlGate.cs, IntegrationSqlHost.cs,
IdentitySqlHostingTests.cs under backend/tests/EnglishCenter.Tests/Api/Integration.
Worker sole IdentitySqlAssertions.cs<=80lines; reads ONLY V01B_WORKER + complete
backend skill + routing. Exact3files external context, not linked files/source.
Verify signup201/no session, login/current me for7roles, normal/restricted password
change/re-login/no upgrade, current-SID logout/other session survival, CSRF denial,
wrong current400/stale RV409, duplicate signup and injected post-write failure rollback. Real service/SQL repositories
behind existing HTTP endpoints in in-process TestServer; no listening socket.

## V01-C

Manager writes AdminSqlHostingTests.cs in same folder. Worker sole
AdminSqlAssertions.cs<=90lines; exact V01C_WORKER/backend skill/routing reads only.
Verify fresh Admin detail12fields, lock/revocation/audit/T23, stale RV/self denial,
matched replay no repeat effects and safe lookup/current authority, other6roles and
restricted deny, invalid UUID/CSRF, missing completion404 without rollback inference.

## Execution and boundaries

Two AGY contracts: edit, separate detached output, sole file, no commands/network/
SQL/packages/Git/merge; Codex manager writes privileged fixture and independently
reviews/tests. Validate contracts before dispatch, keep original receipts/trees.
External union exactly V01B_WORKER.md/V01C_WORKER.md/backend skill/routing; permission
for prior frontend payloads/V01-A does not cover these. Owner files never copied.

Proposed SQL only .\\SQLEXPRESS/EnglishCenter_D07_20261005. Seed synthetic seven Roles,
Accounts, StudentProfile/AuthSession/AuditLog/ReplayOperation in ONE outer transaction
per scenario; passwords/JWT keys/cookies/tokens RAM-only. Each operation has a SQL
savepoint to rollback failed service effects. No owned executor COMMIT; test adapters
intentionally supply caller-TX services/repositories. Pure security logging uses a
test sink; independent persisted SQL audit is not proved. Admin detail uses service
caller-TX projection; production separate SNAPSHOT executor remains unexercised.
No permission user/membership/DENY/GRANT, schema/migration/modules/principals/logins,
production profile/key provisioning, package manifests/locks or other database.
Assert baseline before/after via V01-evidence/baseline.sql;24empty tables/4roles/
102permissions/6migrations/profile unchanged. Abort on drift; always dispose/rollback.

Opt-in environment gate EC_V01_SQL=1; new tests skip before opening SQL otherwise.
Run only IdentitySqlHostingTests for B, then AdminSqlHostingTests for C, exact filters.
Build --no-restore with existing dependencies; no package additions/downloads.
Default Program/frontend entry untouched; no browser/HTTPS/network/runtime claims.
Both task checks must PASS before apply main under ADR163; rejected output detached.
Affected docs/cards/notes/evidence only; default unit suite must remain SQL-free.
Full V01/M0 incomplete: actual browser/HTTPS/principals/owned COMMIT/races deferred.

## Historical preparation checkpoint — before explicit SQL approval

Owner approved BOTH exact AGY payloads2026-10-08; SQL question remains unanswered.
AGY B120c4c8b-5d2 SUCCESS sole71lines; C97c1123f-992 SUCCESS sole84lines;
full3context reads in order before sole write, own output read only thereafter,
zero observed scope violations. Runner read boundaries are not OS sandbox proof.
Codex fixed C target from scalar to exact accountId object and tightened UTC format;
original outputs/receipts/events retained. Worker claims are not execution evidence.
Manager managed worktree C:/Users/asus/.codex/worktrees/v01-http-sql/main_workplace;
base483dd01, prep f548b81, initial fixture WIPf87912c (not approved code main).
Build --no-restore -warnaserror:0warnings/0errors. SQL opt-out suite891PASS/27SKIP/
0fail, confirming new opt-in tests skipped.15B+12C cases await execution permission;
do not count skipped tests as integrated PASS or apply test sources under ADR163.
Injection throws MandatoryAuditException after real service writes then rolls back
the savepoint; it is a post-write transaction seam, not a forced SQL audit DENY test.
No SQL used in this preparation turn; V01-A remains the latest actual SQL evidence.

## Historical continuation execution gate — resolved2026-10-08

Read-only designated baseline probe PASS: identical catalog digest,
24empty business tables/102permissions. The attempt to start the15B tests was
rejected by automatic approval review before process creation: a generic
"continue" did not clearly authorize the new state-changing HTTP/SQL fixture scope.
No B/C SQL test ran. Do not bypass/retry through indirect commands. One explicit
combined B/C rollback approval question is pending; prior AGY sharing remains
approved. Original manager code00443a6 was detached/unapplied/incomplete then.

## Final independent review and evidence

Owner explicitly approved15B+12C after the auto-review rejection; approved execution
then succeeded. No permission bypass. Manager15Identity+12Admin PASS/0fail/0skip;
main27PASS/0fail/0skip. Default unit suite891PASS/27intentional opt-in SKIP/0fail;
SQL cases are separately executed PASS, not inferred from skips. Build0warnings/
0errors/no-restore. Actual counts/TRX hashes in BC-verification-summary.json;
raw test logs stay locally ignored and are never shared with AGY.

Codex strengthened clock evidence: me after2minutes does not renew activity;
matched replay after1minute does not renew activity or repeat mutation/audit/T23.
Before/after manager/main catalog/profile/permission digest identical
0DE381587B6706797C2F38FB3C64DE20A3E19083E391E4684EF3820CEE6FBEF7.
24tables/0retained business rows/0fixture users/0members/4roles102permissions/
6migrations/compatibility160/snapshotON unchanged. Four owner hashes unchanged;
untracked/unshared/unstaged. Selected skills and actual worker scope verified.
Production backend/srcd1f943e467e2407dc12f7ac0c51f6e50d71b81fb and frontend
4050338d256ad268fe11cc1de7180e39f08a1458 unchanged; no route/DTO/default entry/
package/schema changes. Migration/API docs/frontend responsive checks N/A for
test-only additions; notes V01B/V01C explain actual fixture flow within15lines.

Verdict PASS for these two bounded verification tasks, not full V01/M0. Retain
manager/worker originals. Browser HTTPS/real runtime principals/owned COMMIT/
separate production SNAPSHOT executor/independent persisted security audit and
committed multi-client race proof remain unverified; no new authority for them.
