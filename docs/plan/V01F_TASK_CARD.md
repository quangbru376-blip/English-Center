# V01-F — browser HTTPS to actual owned SQL COMMIT

2026-10-08; base main3f649db; implementing under ADR177, PASS apply ADR163.
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
Actual SqlSignup/Login/Current/Logout/Password/Admin executors and SqlSecurityAuditStore
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
Complete skills/routing/AGY contract read; hashes same E card. Overrides accepted
cookie JWT/framework CSRF/SQLServer160/exactDTO/RV/UUID/noSQLite/noBearer/no packages.
Worker context exactly F_WORKER, backend-clean-code/SKILL.md, SKILL_ROUTING.md,
complete reads before write; no source/AGENTS/links/secrets/owner/listing/commands/
network/SQL/Git/validation, own output read only. ADR177 covers exact payload.

Checks: build --no-restore -warnaserror, exact6real HTTPS+ownedSQL on manager/main,
default891unit PASS and new55intentional SQL/browser skips; unchanged production
Git trees/4owner hashes/untracked/unstaged and D07 baseline, scratch0 aftersuite.
Review actual worker tools/diff, sanitize results only, notes<=15lines. PASS apply
ADR163 and reverify. Full V01/M0/profile readiness remains incomplete; F does not
prove active least-privileged login/production HTTPS/PKI/profile/50user load.
