# A06 — API003 /me and reusable live-session authorization — preparation

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
skill_verification: preparation, no AGY dispatch/new SQL/code completion yet.

## Authority and proposed execution scope

Implementation is owner-requested. Existing external read and SQL fixture approvals
were for A03/A05 only. Exact new scope below awaits explicit data-sharing/SQL consent
under SKILL_ROUTING3.4; do not treat earlier approvals as blanket future consent.
External union: A06_DEADLINE_WORKER.md, A07_SHAPE_WORKER.md,
backend-clean-code/SKILL.md, SKILL_ROUTING.md only. Each worker reads own card +
complete selected skill/routing and its sole output if it exists. No AGENTS/links/
other source/commands/SQL/network/secrets. One helper each, independent detached trees.

Codex proposed SQL ONLY .\SQLEXPRESS/EnglishCenter_D07_20261005, synthetic
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

AGY sole output Common/Security/SessionDeadlineMath.cs from A06_DEADLINE_WORKER.
Codex Common/Security supplied-key JWT validator/immutable verified references;
Services/Auth current-session policy/context/service and explicit activity/recheck
hooks; Infrastructure/Auth Account→Session repository/finite caller-TX executor;
Api/Auth current-session/CSRF identity resolver, API003 adapter and narrow A04
CsrfBoundary integration preserving its private verified marker/Origin/CSRF order.
Relevant unit/host/SQL tests, cards/evidence/notes/docs/api/affected memory only.
Reuse AuthContextView exactly9fields and P05 raw IsBefore, not credential reissue.
No default Program mapping/DI/profile, migration/SQL module/package/config changes.

JWT comes ONLY from __Host-ec-auth; no Authorization/header/query/body selector.
Framework cryptographic validation uses explicit supplied keys/issuer/audience/
algorithm allowlist and raw-clock lifetime checks, zero extension/leeway beyond
server deadline; no implicit key retrieval/network/development fallback. Validate
required canonical positive Account id/securityVersion,32byte sid, exact scope and
required lifetime claims; missing/malformed/confused/forged token ->safe generic401.
Claims never establish role/contact/ownership; derive current facts from DB.

Every protected request checks live ACTIVE Account, fixed current role, matching
JWT/Account/Session security versions, matching Account/SID relation/scope, no revoke,
issued/not-before and effective JWT/DB/absolute/idle/temp deadline. Equality at expiry
invalid. Normal must not become restricted by stale rights, restricted never upgrades
when DB changes. PASSWORD_CHANGE requires current mustChangePassword/temp bundle and
only CSRF/minimal auth context/password-change future route/logout access; business
scope403 before binding/reads. Manual LOCKED/DISABLED/revoked/version change invalid401;
login throttle does not invalidate existing otherwise-valid sessions.

Fresh Account lock before Session lock under approved SC03 order for transaction
rechecks and explicit activity writes. Me/context/CSRF/login/logout/probes/background
traffic NEVER write last_activity. Reusable approved business-activity hook is server-
selected only on an authenticated authorized interactive business request, checks
current valid NORMAL session before touch; no client identity/expiry/activity header
or automatic heartbeat/mouse/typing/polling keepalive. No real business route mounted.
Future workflow must call transaction recheck after its sorted domain gates and hold
Account/Session until transaction completion; pre-CSRF identity is not final authority.

GET /api/v1/auth/me: exact absent body/query, authenticated NORMAL or PASSWORD_CHANGE.
Current9-field AuthContext200/no-store, safe401 without stale context. Expiry/status/
version races rechecked before context output; do not use cached principal as rights.
API001 uses live A06 resolver for valid identities and does not renew idle. Existing
A03 anonymous-only/A05 normal-scope login guards retain canonical behavior. Pure
ACCESS.DENIED audit best-effort uses actor only after verified live identity, elseNULL;
logging failure never grants access; no token/claim/request snapshots retained.

Checks: real issuer/audience/signature/algorithm/lifetime negatives and wrong claim
shapes; seven roles/current contact/RV/status/version/revoke/Account-SID mismatch;
normal2h/idle30min and raw equality boundaries; restricted min10min/temp cap/no upgrade;
me/CSRF/background no idle writes; explicit activity cannot resurrect expired session;
fresh SQL facts and Account→Session finite contention/recheck; exact9JSON/no tokens/
no cookie renewal, safe401/403/400/503, Origin/CSRF guards and denial-audit failure.
