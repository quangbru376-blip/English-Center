# A07 — API005 current-session logout — reviewed PASS

Owner2026-10-06 requests starting the next two canonical families A06→A07.
Baseline codea9abdc6/docs87b4885:586unit/HTTP+409SQL=995PASS; default host unmounted.
implementation_skills: backend-clean-code; review_skills: code-review (Codex).
integration: antigravity-worker; Codex manager. Complete skills and routing read.
skill_sources (primary checkout raw-byte hashes):
- backend-clean-code: `.agents/skills/backend-clean-code/SKILL.md` SHA256 `726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b`
- code-review: `.agents/skills/code-review/SKILL.md` SHA256 `04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70`
- routing: `docs/plan/SKILL_ROUTING.md` SHA256 `adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915`
- antigravity-worker: `C:/Users/asus/.codex/skills/antigravity-worker/SKILL.md` SHA256 `eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b`
applicable_overrides: requirements/Accepted E03/E04 and ADR049/055/057/092/097/
098/099/115/140/163/170; cookie JWT/framework CSRF, exact DTOs/precedence, SQL160
real fixtures, explicit origin/keys/profile/deadlines; no SQLite/CORS/new packages.
read_context: ordered Project Memory, requirements relevant auth/privacy/acceptance;
HANDOFF/FIRST_CODE_HANDOFF/implementation-plan/M0_QUALITY_PLAN; accepted E04 DTO/
final contract/runtime/SQL lock order and existing A01/A04/A05 identity code.
skill_verification: independent Codex source/build/test review PASS; bounded read-before-write/scope audit below.

## Authority and proposed execution scope

Implementation is owner-requested. Existing external read and SQL fixture approvals
were for A03/A05 only. Owner explicitly approved this exact new data-sharing/SQL scope2026-10-06.
SKILL_ROUTING3.4 gate satisfied; earlier approvals remain historical.
External union: A06_DEADLINE_WORKER.md, A07_SHAPE_WORKER.md,
backend-clean-code/SKILL.md, SKILL_ROUTING.md only. Each worker reads own card +
complete selected skill/routing and its sole output if it exists. No AGENTS/links/
other source/commands/SQL/network/secrets. One helper each, independent detached trees.

Codex verified SQL ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005, synthetic
Account/Role/AuthSession fixtures and RAM-generated passwords/signing keys/tokens,
caller transactions rollback only. Multiple connections permitted for finite lock/
revocation/denied-update contention; all fixture DML rolls back. Existing regression
may reuse only four D12_fixture_user_0..3 WITHOUT LOGIN plus temporary membership/
DENY and previous module/schema drift rollback. No persisted login/signup/revoke/
activity traffic or committed concurrency claim; owned executor commit paths source
review plus explicit HTTP fixture seams. No principal/profile/schema/grant/owner/
login/other DB/persistent seed/key/package/install/push/deploy/runtime activation.
No A08/password change/replay/R/Admin/UI/restM0 implementation opened.
Standing ADR163 applies independently reviewed PASS implementation to main without
another apply question. Preserve rejected/unreviewed worker output detached.

## Gates

Codex verifies full bounded reads-before-write/receipt/diff/scope and independently
builds/tests/reviews. Unit/HTTP/pure signing tests + real rollback SQL + full current
regression, exact catalog/modules/permissions and retained0business rows/test users/
role memberships; unchanged four roles102permissions/six migrations/profile160/
collation/snapshotON. Missing checks mean incomplete, not PASS. Owner notes<=15lines,
docs/api.md/plan and affected memory updated. Profile/principal/production-key/full
readiness and committed runtime/stress limitations recorded, never claimed proven.

## Implementation scope

AGY sole output Api/Auth/LogoutRequestShape.cs from A07_SHAPE_WORKER.
Codex Services/Auth logout/current-session revocation service; Infrastructure/Auth
caller-TX Account→Session guarded repository and explicit owned-operation executor;
Api/Auth API005 adapter, reuse A04 guard/A06 validator/A01 security recorder and safe
cookie options. Relevant unit/host/SQL tests and task evidence/notes/API/affected memory.
No A08 reset/password change/revoke-all action/logout-all/device management/replay.

POST /api/v1/auth/logout has no business body/query/UUID/account/device selectors.
Valid current NORMAL or PASSWORD_CHANGE session required. Live auth before framework
Origin/CSRF, exact input validation afterward, final Account→Session recheck on the
same mutation transaction before revocation. Revoked/expired/missing/version-invalid
session401; wrong Origin/CSRF403 must not claim server logout or reach revocation.

Set only the current sid's revoked_at at UTCms under finite locks, without touching
last_activity/securityVersion/other sessions/Account status or deleting session rows.
Transaction commits before204 and __Host-ec-auth cookie deletion. Unknown commit,
cancellation/revoke failure must not fake success/clear cookie prematurely; no retry/
credential replay. Multiple tabs sharing SID lose this session; other devices/SIDs
remain valid. Repeated logout after revocation401, not invented idempotent204.
Security AUTH.LOGOUT audit runs independently after successful revocation commit,
verified actor only; failures/cancellation increment fixed A01 counter and cannot
restore the committed session or withhold successful cookie deletion. No secrets.

Cookie removal preserves host-only Secure/HttpOnly/SameSiteStrict/Path=/, no Domain;
no new JWT/session/CSRF token issued by logout. Client sensitive-RAM clearing is future
UI responsibility; invalid session401 UI can still become logged-out. Default Program
remains unchanged/unmounted; production profile/principal/key readiness stays deferred.

Checks: normal/restricted/seven-role logout; current SID only/other device persists;
real SQL revocation/update denial/fault/cancel rollback and concurrent Account→Session
contention/late revoke/version/expiry; no activity renewal; 204 exact empty body/cookie
flags after commit only; already-expired401, CSRF403, extra body/query400; audit-failure
postcommit cannot undo logout. Distinguish rollback fixtures from persistent traffic.

## Completion and independent review — 2026-10-06

Both integrated authorized stages reviewed PASS; source/evidence retained manager
detached, ready for ADR163 apply and main reverification. No A08/restM0 activation.
A06 AGY8aba3a3b-d0a SUCCESS/116diff lines; A07 AGY442d3e33-d34 SUCCESS/10diff lines.
Each5observed reads/1write/full3authorized context reads before write/0observed
scope violations; helpers independently inspected and tested. Original trees retained.
No raw AGY thinking/transcripts or test credential/key/JWT logs committed; compact
contract/receipt/scope evidence stored. Whitespace normalized in manager integration.
Scope clarification: narrow shared A05 SqlLoginRepository fresh-load state reset;
A04 denied observer hook; invalid live-cookie401 cleanup as A06 recovery prerequisite.
No new product rule or external payload was introduced. See ADR171 and explicit diff.

Codex checklist scope/business/backend/DTO/security/owner-notes/docs PASS; frontend,
new migrations, money, pagination N/A. Full locked restore from existing global cache
using an empty feed/NuGetAuditfalse, no package/version change or new vulnerability
assessment. Solution and SQL build0warnings0errors.632unit/HTTP+438SQL=1070PASS/
0fail0skip; baseline586+409, added46unit/HTTP+29SQL. JWT algorithm/signature/issuer/
audience/type/shape/raw timing, seven roles/current context, restricted/idle/revocation,
callback recovery, commit/cookie/audit failure seams PASS. Actual caller SQL fault after
Save/cancel, temporary runtime DENY UPDATE, two-connection finite contention, other SID
preserved, non-renewing reads and explicit interactive touch PASS; fixtures rollback.
Exact full catalog/modules/columns/102permissions regression PASS. Read-only retained
all dbo rows except migration history0, test users0, ec-role members0;4roles/6migrations,
profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged.
Limits: no owned SQL persistent commit executed, committed multi-client stress/profile/
Windows-linked login principals/production keys/network TLS/full readiness evidence.
Future business workflows must recheck/touch in their own TX after domain gates;
no full application permission/ownership matrix is claimed. Program stays unmounted.
