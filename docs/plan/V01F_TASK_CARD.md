# V01-F — browser HTTPS to actual owned SQL COMMIT

2026-10-08; base main3f649db; completed/reviewed PASS/applied/reverified under ADR177, PASS apply ADR163.
First closure matrix: V01_READINESS_CLOSURE.md. Codex remains manager; privileged
SQL lifecycle and review stay with Codex. Do not re-ask assigned task aspects.

Six cases reuse unchanged D real Edge/UI/API driver groups1,2,4,5,6,9:
1 signup375/768/1280 commits3Account/Profile/mandatory audits, no authSID/cookie;
2 seven roles commit7SIDs +7independent LOGIN security rows;
4 shared tabs/current logout commit one revoke, independent profile survives;
5 normal/6restricted password204 commit hash/version/temp/allownSID revoke/audit,
  old proof401 and deliberate new login; restricted Admin requests denied;
9 confirmed Admin lock commits once then drops ack; manual safe lookup returns
  single persisted audit/T23/operation, no retry/new intent; target proof revoked.
Assertions observe fresh independent DbContext AFTER browser requests finish;
no outer transaction/savepoint/fixture fake executor/mock API/serial manager gate.
Actual SqlStudentSignup/Login/Current/Logout/Password/Admin executors and SqlSecurityAuditStore
are mapped test-only through existing production adapters/live CSRF resolver.

SQL same E lifecycle: only lpc:.\SQLEXPRESS, one newly absent
EnglishCenter_V01E_20261008_<32hexGUID> scratch DB percase, existing6migrations,
compat160/collation/snapshotON/AUTO_CLOSE OFF/poolingFalse; synthetic8Accounts/7Roles/
1Profile, generated credentials/JWT/RSA RAM-only. Existing E validated name/id/GUID
receipt/finally DROP exactowned DB; retain ignored receipt; no wildcardcleanup.
D07 read-only baseline before/after and scratch absence; no server login/grants,
other DB mutation, real Admin or secrets. E fixed test budgets retained, no SLO.

Only transient loopback Kestrel private HTTP + Node TLS terminator on OS port;
exact current certificate SPKI pin/fingerprint/SAN, default trust/wrong pin deny,
nonce loopback bridge strips client forwarding headers. No OS trust/firewall/
public origin/config/cert store mutation. Node/Edge stopped before DB cleanup.
Passwords/keys/tokens only RAM stdin, no CLI/log/HAR/screenshot/rawdiagnostic.
Gate requires EC_V01_SQL=1 AND EC_V01_OWNED_SQL=1 AND EC_V01_BROWSER=1.
Assets built only in unique OS Temp path with existing Vite; installed Edge and
bundled Playwright; no package install/restore/network download. Test-only assets
are not default production entries. Reuse existing D driver unchanged, including
fixed diagnostic sanitization; no production frontend/backend source change.

Manager allowed files: backend/tests/EnglishCenter.Tests/Api/Integration/
OwnedSqlFixture.cs (expose actual audit recorder and optional restricted role),
OwnedBrowserSqlHostingTests.cs (new6cases/realroutehost/cleanup/SQL assertions).
Worker sole OwnedBrowserAssertions.cs <=85lines for persisted lock/audit reference
projection. No source/default entries/driver/package/schema/migration edits.
Docs: F card/worker/contract/closure matrix/sanitized F evidence/notes and affected
memory/plans. Retain worker/manager originals and receipts. No unrelated tasks.

implementation_skills: backend-clean-code; frontend-ui verification checklist
(existing driver/UI untouched). review_skills: code-review (Codex).
Complete skills/routing/AGY contract read; source SHA256 same unchanged E card:
backend726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b;
frontend448079d6ef3b5b514bbcba4f2a8d5aaf0fc6cc8063f79798f28a6dbbe5128f90;
review04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70;
routingadb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915. Overrides accepted
cookie JWT/framework CSRF/SQLServer160/exactDTO/RV/UUID/noSQLite/noBearer/no packages.
Worker context exactly F_WORKER, backend-clean-code/SKILL.md, SKILL_ROUTING.md,
complete reads before write; no source/AGENTS/links/secrets/owner/listing/commands/
network/SQL/Git/validation, own output read only. ADR177 covers exact payload.

Read context: ordered memory/handoffs/requirements identity+E04 acceptance;
E04_RUNTIME_PROFILE/FINAL_CONTRACT§7/M0§7, actual compatibility inspector/Program,
owned E fixture/executors/audit/CSRF and unchanged D driver/host/assets.
Verification includes E12 regression on both checkouts for changed shared fixture.

Checks: build --no-restore -warnaserror, exact6real HTTPS+ownedSQL on manager/main,
default891unit PASS and new55intentional SQL/browser skips; unchanged production
Git trees/4owner hashes/untracked/unstaged and D07 baseline, scratch0 aftersuite.
Review actual worker tools/diff, sanitize results only, notes<=15lines. PASS apply
ADR163 and reverify. Full V01/M0/profile readiness remains incomplete; F does not
prove active least-privileged login/production HTTPS/PKI/profile/50user load.

## Actual manager review

Manager1da7463 independent F6+E12=18PASS/0fail0skip; default891PASS/55intentional
opt-in SKIP/0fail/946total; build0warnings0errors. Code applied main652e48a under
ADR163; main F6+E12=18PASS/0fail0skip, default891PASS/55opt-in SKIP/buildclean. Git test tree7251f84550e652456497d592c9fb317fa3bc2b18.
AGYefc6e268-2ca SUCCESS; full27/46/101reads before sole69line helper/own69line read,
zero observed scope violations/commands/network/SQL. Raw receipts/events/original
worker retained; observed tools are not an OS-enforced read boundary.
Codex fixed numeric ValueKind and scalar projection comparisons to fixed messages;
namespace/route-host analyzer issues fixed. No production/driver/package changes.
First manager18cases:14PASS/4assertionFAIL because real security audit persists
anonymous/stale protected-read ACCESS_DENIED events absent in D rollback fake
security store. Mandatory mutation/login/logout counts are now separate, all
security projections/type/actor allowlists verified,7role LOGIN actors exactset.
Final18PASS; no service defect or waived audit behavior. All created scratch DBs
cleaned; D07 manager baseline identical, no new cleanup failure. Final main outcome,
receipt counts/owner hashes and sanitized F evidence are in F-verification-summary.

## Final main evidence and verdict

Review PASS, applied ADR163/reverified main652e48a. Manager/main18PASS and
891defaultPASS/55intentional opt-in SKIP; exact test tree7251f84550e652456497d592c9fb317fa3bc2b18.
All54Fcreated scratch DBs normally dropped by captured name/id/GUID;0remaining,
no new ownership recovery; total E+F90created/90cleaned (E earlier1exact recovery).
D07 before/after baseline byte-identical/catalog/profile/102permissions/24empty
business tables/0fixture users/members/4roles6migrations/compat160/snapshotON unchanged.
Four owner file hashes match handoff and remain unshared/untracked/unstaged.
Production backend/src/frontend/src/default entries/packages/driver Git trees unchanged.
Temporary Vite asset build PASS; existing frontend48checks historical, not rerun.
Node/Edge/Kestrel lifecycle ends before exact database cleanup. No raw credential,
cookie/header/body/HAR/screenshot or worker reasoning committed; raw TRX/receipts
ignored. See F-verification-summary and15line owner notes. No unresolved F checks.
Full V01/M0 still incomplete: profile/readiness orchestration/runtime principal,
catalog visibility/protected persisted keys/production PKI/DF01–03 values remain
unverified or owner-deferred. Next prepare unmounted fail-closed contract; do not
choose deferred environment values or make a production-ready claim from fixtures.
