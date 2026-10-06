# A08 — Password change / restricted lifecycle

Status2026-10-06: completed and independently reviewed PASS; ready for main application.
Canonical family M0_QUALITY_PLAN A08; dependencies A06,P08,A01 applied PASS.

## Authority and execution boundary

Owner requested A08→R01 on2026-10-06. Exact scope approved; bounded worker and rollback SQL verification complete. Prior A06/A07 external and fixture approval is historical, not blanket
consent. Owner approved exact new payload and rollback SQL scope below on2026-10-06;
SKILL_ROUTING3.4 satisfied; no product decisions reopened. ADR163 applies reviewed PASS authorized
stages to main without another apply question; rejected output stays detached.

External union ONLY A08_VERSION_WORKER.md, R01_STRING_WORKER.md,
.agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md.
Each worker reads own card+full skill/routing+sole output if present; one helper,
no other source/AGENTS/links/commands/SQL/secrets/network. Reduced detached environment.

Approved Codex SQL ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005. Synthetic
Role/Account/AuthSession/Audit fixtures and passwords/signing keys/tokens generated
only in RAM. All fixture DML caller-TX rollback. Finite multiple connections test
Account→deterministic binary SID locks, version/revoke races and no expired-session
resurrection. Existing regression reuses only four D12_fixture_user_0..3 WITHOUT
LOGIN with temporary membership/DENY and previous module/schema drift, all rollback.
No new principals, real accounts/seeds, persistent data/keys, schema/migrations,
packages/grants/profile/owner/server logins/other DB/install/runtime/push/deploy.
Owned SQL COMMIT wrappers are source-reviewed with HTTP fixture commit seams only;
not run to persist changes, no committed runtime/stress readiness claim.
R01 is disconnected: no ReplayOperation writes/lookups, R02/R03/UI/restM0 deferred.

## Skills, sources and verification gate

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
integration: antigravity-worker; Codex manager. Complete skills/routing read.
skill_sources:
- .agents/skills/backend-clean-code/SKILL.md SHA256 `726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b`
- .agents/skills/code-review/SKILL.md SHA256 `04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70`
- docs/plan/SKILL_ROUTING.md SHA256 `adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915`
- antigravity-worker SHA256 `eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b`
applicable_overrides: Accepted requirements/E03/E04/ADR049/053–055/092/097–099/
114/115/140/160/163/170/171; exact DTO/error precedence/raw clock/cookie JWT/
framework CSRF/SQL160. No SQLite, CORS, new packages or inferred runtime defaults.
read_context: ordered Project Memory; requirements auth/privacy; HANDOFF,
FIRST_CODE_HANDOFF,M0_QUALITY_PLAN,implementation-plan; E04 DTO/final/replay/SQL/
runtime; existing A01/P04–P08/A04–A07 code, entity/config and tests.
skill_verification: observed full3authorized contexts before write/zero scope violations;
Codex independently source-reviewed/built/tested; PASS with corrections completed.

Baseline code mained40a4b/docs32c0a79, backend0d407841beeb719bbd753e828a9a5d408549a1bc;
632unit/HTTP+438SQL=1070PASS/buildclean. Full regression required; locked/cache-only
restore if needed, no dependency change/new audit claim. Catalog/modules/columns/
102permissions/four roles/six migrations/profile160/collation/snapshotON unchanged;
post-tests retained dbo business rows/test users/role memberships0 required.
Missing checks mean incomplete. Source review and meaningful unit/HTTP/real SQL
checks performed by Codex; owner notes<=15lines, docs/api/plan/affected memory updated.
Default Program stays unmounted, production keys/principals/profile/full readiness
remain operator prerequisites. Preserve detached worker/manager evidence trees.

## Implementation scope and acceptance

Worker sole helper Common/Security/PasswordChangeVersionGuard.cs. Codex adds
Services/Auth password-change DTO/service, Infrastructure/Auth repository/executor,
Api/Auth strict bounded request/API004 integration, relevant tests and docs only.
Narrow A06 reuse refactor allowed only for caller-TX fresh proof/account locks and
all SID deterministic locking; preserve A03/A05/A06/A07 behavior. No Program/config/
package/mapping/SQL module/migration changes. No Admin reset or business routes.

POST /api/v1/auth/change-password, authenticated NORMAL or PASSWORD_CHANGE only.
Live proof/Origin/framework CSRF before detailed body checks; exact required
currentPassword/newPassword/expectedVersions.account, no query/UUID/id/unknown fields.
Strict scalars/duplicates/Unicode/size/NULL; RV exact8bytes. Passwords stay raw RAM,
never trim/NFC/replay/digest/log/echo. New password15–128 codepoints and existing
offline blocklist; malformed/blocked400 newPassword, wrong current400 currentPassword
VALIDATION_ERROR per accepted API004 refinement. Current raw credential verify
uses existing hasher, no login throttle mutation. Stale account RV409; expired/
revoked/status/security/scope invalid401. Exact time equality expires; restricted
remaining temp24h/min10min cap is rechecked up to commit, no scope auto-upgrade.

Fresh current Account lock before all own Session rows in binary SID order under
finite caller-TX scope. Recheck current proof/role/status/version/revocation/deadline,
current password and account RV before effects. Atomic new password hash,
security_version+1 (fail closed overflow), clear mustChange/temp bundle, revoke all
existing own sessions including current, projected mandatory PASSWORD_CHANGE audit.
No profile/role/status/login-throttle edits, other accounts unaffected. Mandatory
audit failure/cancel/deadline/denied DML rolls back everything; no secret snapshots.
Success204/no body and secure cookie deletion only AFTER successful commit; user
logs in deliberately with new password, old JWT/current/all other devices invalid.
No caller-result/replay/auto resend, no new session issuance or automatic login.

Tests: all7roles+normal/restricted happy paths; raw password Unicode/spaces; blocked/
wrong/missing/unknown/duplicate/query/malformed/stale inputs and error precedence;
invalid/expired/temp/scoped/racing proof; version max; all-session vs other-account
invalidation; retained rowversions/fresh reads; required audit failure/rollback,
finite contention and denied DML; no cookie on failure/commit fault; commit before204;
no password/hash/token in responses/logs/audit. Re-login projection verified with
fixture seams, persisted runtime/committed concurrency remains unproven.

## Completion and independent review — 2026-10-06

Owner-approved A08→R01 implementation completed and independently reviewed PASS
2026-10-06;733unit/HTTP+462SQL=1195PASS/0fail0skip/build0warnings0errors.
Two bounded AGY SUCCESS/full3contexts before write/zero observed scope violations;
Codex source integration/fixes/schema parity/independent tests. A08 exact API004,
normal/restricted current proof and RV, raw credentials/P08 policy; mandatory
hash/security-version/temp-clear/all-own-SID revoke/audit caller-TX unit; secure
cookie deletion only after commit. Account first, explicit SID locks binary order;
per-SID round trips intentional for accepted lock protocol, no invented session cap.
AuditEntry narrow fixed AUTH.PASSWORD_CHANGE factory contains no credentials/hashes.
No automatic scope upgrade/new session/login or credential replay. R01 disconnected
55exact canonical registry/6lookup groups, full schema-aware EC-C1 normalized bytes/
SHA25632binary, frozen literal fixtures, full supplied intent/absence/sets vs ordered
weekly plan. No T23 storage/lookup/mutation/replay routes or55executed workflows claim.
Program unchanged/unmounted; no real Admin/production profile/keys/principals/full
runtime readiness. SQL cores rollback-only; owned executor COMMIT source-reviewed
and explicit HTTP fixture commit seams, not persisted/committed stress proof.
All retained dbo rows except history/test users/role memberships0;4roles/exact102
permissions/6migrations/profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged;
exact full catalog/modules/columns regression PASS. No schema/module/package/grants/
profile/owner/logins/other DB/seed/persistent keys/install/push/deploy changes.
Original detached worker/manager evidence retained. See A08/R01_TASK_CARD/evidence,
notes/A08.md,R01.md; ADR172. Applied/reverified under ADR163 code main3b46460, manager18c0af7; identical backend
4fb65a2e2f0e3ef9fc95dd040cf4438c4156be24. Both manager and main full suites PASS;
compact TRX hashes/checks in A08/R01-evidence/verification-summary.json.
Next canonical R02 completion/TX/lookup primitives → R03 Admin lock pilot; unstarted.
Workerd32988a5-9ff SUCCESS/28diff lines;5observed reads/1write/full3contexts/0violations.
Compact VERSION contract/receipt/scope audit retained; no raw thinking/transcripts/logs.
Review corrections: restricted API004 policy allows PASSWORD_CHANGE; worker string
buffer moved outside loop; API097 has no reason; composite primary then ASCII-ID
sorting and nested identity checks; explicit Account→binary SID lock acquisition.
Shared AuditEntry change is only the required nonsecret mandatory own-change factory.
Checklist scope/business/backend/DTO/security/owner notes/docs PASS. Frontend/new
migrations/money storage/pagination N/A. Locked restore existing cache/emptyfeed only,
NuGetAuditfalse/no package/version changes/new vulnerability assessment claim.
Tests baseline632unit+438SQL, added101unit/HTTP/C1+24SQL. SQL faults after actual
Save/audit, cancellation, temporary DENY Account/Session/Audit, overflow, raw timer
cutoffs and finite competing-connection locks PASS; exact retained0 checks PASS.
