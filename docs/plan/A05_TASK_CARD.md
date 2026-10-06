# A05 — API002 login/throttle and session issuance — reviewed PASS

Owner2026-10-06 requested continuation of the next two canonical families A03→A05.
Baseline codea8ad1d5/docsc5c3e6e:545unit+375SQL PASS; default host remains unexposed.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete skills/routing read before writing.
skill_sources (byte SHA256 of current primary checkout):
- backend-clean-code: `.agents/skills/backend-clean-code/SKILL.md` SHA256 `726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b`
- code-review: `.agents/skills/code-review/SKILL.md` SHA256 `04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70`
- routing: `docs/plan/SKILL_ROUTING.md` SHA256 `adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915`
- antigravity-worker: `C:/Users/asus/.codex/skills/antigravity-worker/SKILL.md` SHA256 `eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b`
applicable_overrides: accepted requirements/E03/E04, strict DTO/error precedence,
JWT-cookie/framework CSRF/no CORS, SQLServer160, P08 raw password/PasswordHasher,
explicit caller profile/keys/deadlines; no SQLite, guessed defaults or startup seed.
read_context: ordered Project Memory; HANDOFF/FIRST_CODE_HANDOFF/M0_QUALITY_PLAN,
requirements/auth/profile ADRs, E04 DTO/final pipeline/runtime/SQL integration,
existing P04/P05/P06/P07/P08/A01/A02/A04 and EF identity foundations.
skill_verification: Codex review PASS; bounded worker scope audits and independent tests below.
Standing ADR163 applies only independently reviewed authorized PASS code stages.

External proposal is exactly both new worker cards, backend skill and routing.
Each worker reads only its own card + skill/routing (and its sole output if present),
writes one detached helper, no commands/SQL/Git/search/network/secrets/links/other reads.
Owner explicitly approved this exact sharing and rollback SQL scope2026-10-06 ("tôi duyệt").

Codex proposed verification: ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005, generated
synthetic credentials/signing keys in RAM, no real accounts, all database fixtures
within caller transactions rolled back. Existing regression may reuse only the four
D12_fixture_user_0..3 WITHOUT LOGIN and prior rollback drift/DENY fixtures. New tests
may use multiple connections for lock contention, with finite explicit deadlines,
without persistent seed/commit/grants/schema changes. No database owner/profile/login/
other DB/persistent principal/key/installation/push/deploy/runtime activation changes.
Current A03→A05 rollback SQL scope explicitly approved; prior A02/A04 consent historical.
Real operator keys/origin/proxy/DB principals and A06 live-session resolver remain
prerequisites. Test profiles/signers/identity resolvers never become runtime defaults.
No A06–A08/R01–R03/UI/restM0 work. API integration functions remain unmounted in Program.

Gates: accepted contract parity, worker receipt/read-before-write/scope audit, current
locked dependencies/build/unit+host tests/full SQL regressions/exact post-fixture
catalog and empty data/users/memberships; Codex source review and <=15line owner notes.
Record stage limitations honestly; fixture SQL rollback is not runtime/demo readiness,
real principal login authentication or committed multi-client stress proof.

## Implementation scope

AGY: backend/src/EnglishCenter.Api/Common/Security/SessionIssuanceWindow.cs only,
from A05_WINDOW_WORKER.md. Pure fixed issuance deadlines; no account/token/IO/state.
Codex: Services/Auth, Infrastructure/Auth, Api/Auth explicit API002 adapter,
Common/Security JWT issuer +framework-backed login IP limiter, relevant unit/host/SQL
tests and docs/api.md/task evidence/notes/affected memory. Reuse existing pinned
Microsoft.IdentityModel.Tokens and System.IdentityModel.Tokens.Jwt7.7.1 dependencies;
no new PackageReference/lock/version/download/installation. Cryptographic signer,
issuer/audience and trusted-client-IP provider must be explicitly supplied; no keys,
ports, profiles, proxy trust or algorithm chosen as a production default.

POST /api/v1/auth/login binds EXACT email/password strings after A04 Origin/CSRF.
No body JWT or role/session selectors/UUID/replay/credential digests. Account lookup
uses canonical CI email under update/range lock; Account precedes AuthSession locks
and holds through commit against reset/disable/password writers. Bad credentials,
unknown account, manual LOCKED/DISABLED, expired temp and active account throttle all
same401 AUTHENTICATION_FAILED envelope, no email-exists/status/counter/ID leakage.
No actor identity inferred from submitted email in failed security audit.

Persist independent failed_login_count/window/throttledUntil: five consecutive bad
passwords within15min ->block15min; success/expiry resets counting/window; equality
expired. Never status mutation/unlock/enable/revoke existing sessions. Wrong password
counter changes must commit even when response401; exceptions/cancellation roll back.
Serial account locking must avoid lost counter/session version races. Status and temp
expiry rechecked immediately before session DML; clock uses raw cutoff semantics.
P08 Verify success/rehash handling keeps raw secret and compatible approved hashing.

Random256bit sid only server RAM/DB/JWT internal. NORMAL issued+2h/initial effective
min absolute vs30min idle; PASSWORD_CHANGE min10min/temp24h remaining, no auto upgrade.
AuthSession captures current Account securityVersion, supports concurrent device
sessions, no login revocation of other sessions. Signing failure/TX failure yields no
cookie and rolls back counter/session unit. JWT signed using supplied credentials,
validated issuer/audience/algorithm/lifetime claims; lifetime never exceeds session,
no contact/role permission snapshots in token. No real A06 request authorization yet.

Response200 exactly nine AuthContext fields: accountId/fullName/email/role/scope/
accountVersion/serverTime/absoluteExpiresAt/effectiveExpiresAt, no securityVersion/
failed count/sid/JWT/password/profile. JWT only __Host-ec-auth HttpOnly/Secure/Strict/
Path=/ cookie after successful commit, expiry bounded by issued session, no-store.
Context/token fetch does not renew idle. Adapter preserves A04 restricted boundary;
no extra business permission. Existing default Program remains unchanged/unmounted.

IP cap framework100requests/minute, independent account throttle; excess429 with
Retry-After and no account/session changes. Only explicitly trusted client-IP resolver,
no arbitrary forwarded header fallback; missing profile fails503. Verify security
best-effort AUTH.LOGIN/THROTTLED via A01 store/fixed counter; independent audit failure
cannot compensate committed login/security outcome or expose credentials.

Checks: same error privacy across roles/status/unknown/temp/throttle;5/15/15 boundaries,
atomic counter/lock contention, success reset/no status change/no existing-session
revocation, random sid/normal/restricted deadlines, signing verification and bad
profile/commit/fault/cancel paths, secure cookie/exact9fields, IP100/101+Retry-After,
Origin/CSRF precedence and best-effort security audit failure. Fixture-only SQL cores
cannot claim real committed concurrency, production auth resolver or runtime readiness.

## Completion and independent review — 2026-10-06

Review PASS for both integrated authorized stages; combined dependency slice, no A06/A07.
Scope clarification: A03 also extends CsrfBoundary.RequireVerified as an A04 prerequisite;
A05 narrowly extends CredentialHasher.RehashVerified in P08. Shared auth body/deadline
and tests are within the authorized slice, no new library/schema/runtime wiring.
AGY A03 RESULT65bf320b-621 SUCCESS:5reads/1write/24diff lines; A05 WINDOW773dd70d-a63
SUCCESS:5reads/1write/82diff lines. Both read full three authorized context files
before write; zero observed scope violations. Compact contract/receipt/scope audit
retained under respective evidence directories; original detached worker trees retained.
Manager whitespace-normalized new helpers; substantive worker outputs preserved.
Codex independent fixes: fresh locked account data; active throttle expiry priority;
known uniqueness only409; wrapped lock errors; all seven current role contexts; malformed DTO IP cap; raw deadline and NumericDate cutoffs;
verified legacy rehash; no output before commit; independent audit failure preserves result.
Build clean;586unit/HTTP+409SQL=995PASS/0fail/0skip.34 new SQL cases cover A03/A05
rollback/fault/cancel/locks/unique guards/session/counter/temp precision; baseline375.
Unit/HTTP baseline545 +41new: strict shapes, profiles/signing, Origin/CSRF/order,
commit seams, IP gate, audit fault/cancel, exact responses/cookie and missing guards.
Exact catalog/modules/permissions regression PASS; read-only retained check: Account,
StudentProfile,AuthSession,AuditLog,test users,role memberships0;4roles102permissions/
6migrations, compatibility160/collation/snapshotON unchanged.
Review checklist scope/contract/backend/security/owner notes/docs PASS; frontend,
new migration/money/list pagination N/A. Validated Int32 literal SET LOCK_TIMEOUT is
required by SQL grammar and documented ADR170; account SQL remains parameterized.
Limits: supplied production profiles/keys/origin/IP trust/DB principals/full readiness,
real Admin/A06 resolver, owned executor persisted commit and committed concurrency
not verified. Default Program remains unchanged/unmounted. No unapproved scope opened.
Applied/reverified maina9abdc6; manager e5ff366 retained detached, identical backend
53e36afc0d1ad2b9232d04010222e805ca009410. Both main build and all995tests PASS;
verification-summary.json binds exact source/manager/main TRX hashes and retained DB.
