# A08 — Password change / restricted lifecycle

Status2026-10-06: task/worker contract prepared; exact external/SQL consent pending.
Canonical family M0_QUALITY_PLAN A08; dependencies A06,P08,A01 applied PASS.

## Authority and execution boundary

Owner requested A08→R01 on2026-10-06. Implementation planning is started; no worker
or SQL run yet. Prior A06/A07 external and fixture approval is historical, not blanket
consent. Exact new payload and SQL scope below prepared for owner approval per
SKILL_ROUTING3.4; no product decisions reopened. ADR163 applies reviewed PASS authorized
stages to main without another apply question; rejected output stays detached.

External union ONLY A08_VERSION_WORKER.md, R01_STRING_WORKER.md,
.agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md.
Each worker reads own card+full skill/routing+sole output if present; one helper,
no other source/AGENTS/links/commands/SQL/secrets/network. Reduced detached environment.

Proposed Codex SQL ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005. Synthetic
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
skill_verification: pending actual bounded read-before-write/scope audit and independent
Codex review/build/tests. A worker success report alone is not acceptance.

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
