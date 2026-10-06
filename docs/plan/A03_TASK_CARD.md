# A03 — API018 Student self-registration — reviewed PASS

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

AGY: backend/src/EnglishCenter.Api/Api/Identity/RegisterStudentResult.cs only,
from A03_RESULT_WORKER.md. No credentials or domain entities in its response.
Codex: Services/Identity, Infrastructure/Identity, explicit API018 adapter under
Api/Identity; narrow Common/Auditing/AuditEntry.cs initial Student audit factory,
relevant unit/host/SQL tests, docs/api.md, task evidence/notes/affected memory only.
No schema/migration/package/config changes. Reuse strict P06 JSON/scalars and P07
safe field errors, P04 name/email/phone/address/DOB, P05 Vietnam clock and P08 hashing.

POST /api/v1/auth/register (API018 exact E04 route manifest):
anonymous-only boundary, framework Origin/CSRF before exact binding. Fields exactly
fullName/dateOfBirth/email/phone/password required; address optional/null/blank toNULL.
Unknown/duplicate/incorrect type/null required ->400, raw password not normalized,
no client role/status/accountId/studentId/temp/security/version/UUID/extra fields.
Normalized email and nonNULL phone unique across ALL accounts, no provider rewriting.
Nonfuture Vietnam DOB/no invented age rule. Role code Student must already exist;
no auto-bootstrap roles/Admin. Server creates Account ACTIVE/securityVersion1/regular
credential and StudentProfile together, audited in same TX01, no session/auto-login.
Known normalized collision/race409 with safe field errors/no owner ID; unknown SQL500.
Implement Account/Profile/audit on caller transaction; explicit operation executor
owns normal request begin/commit/rollback; response must follow successful commit.
No retry/replay/request UUID/secret digest; cancellation/failure rolls back all writes.

Signup is outside SC02 UUID action registry. Narrow technical STUDENT.ACCOUNT.CREATE
mandatory action/null unauthenticated actor, generated Account reference +positive
roleId/status/mustChangePassword projection only; no email/name/phone/DOB/address/
password/hash/full request. Document technical choice, no product contract reopening.
Success201 exactly outcome CREATED + Vietnamese message, no ID/profile/JWT/Location.
Host tests use explicit profiles/operation executors; real SQL cores enlist fixture TX
and roll back. Clearly distinguish adapter tests from real committed runtime traffic.

Checks: strict fields/raw policy, birth-date/Vietnam day boundary, email CI/+tag and
0/+84 phone equivalence across roles, exact fields/no cookie, signup+audit atomicity,
failure/cancel rollback, competing SQL predicate/unique backstop, no session/replay.

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
Source/evidence remains manager detached until ADR163 apply and main reverification.
