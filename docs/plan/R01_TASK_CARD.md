# R01 — Canonical registry and EC-C1 digest primitives

Status2026-10-06: completed and independently reviewed PASS; ready for main application.
Canonical family M0_QUALITY_PLAN R01; dependencies P04–P06 applied PASS.

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

Worker sole helper Common/Replay/C1StringEncoder.cs. Codex Common/Replay typed
immutable registry/codec/normalized-intent primitives and schema-aware binding;
necessary tests/docs only. No T23 storage, marker locks/completion/lookup/replay route,
permissions/new business endpoints/UI/SQL writes/R02/R03. Registry is descriptive,
never a replacement for current live authorization or implementation of55 workflows.

Match accepted E04_REPLAY_SPEC exactly55 API/action/route/method/actor/target/
lookup/result-mode/no-op mappings and six lookup groups. Case-sensitive canonical
ASCII actions, no API-ID/capability alias or client action/actor authority. Only
API008/009 SECRET_RESULT_UNAVAILABLE; other53 SAFE_REFERENCE. No auth/password/signup
commands in C1 registry. Target includes ALL route business IDs only; create target{}.

Envelope action/body/contract EC-C1/target, sorted ASCII object keys recursively,
no whitespace/BOM/newline, strict UTF8. C1StringEncoder handles exact quoting/control
escapes/literal non-ASCII. SHA256 binary32bytes only; no plaintext envelope/body/hash
logs, no client digest/secret fields. Immutable/copy-out bytes and type-safe bounded
input prevent accidental mutation. Whole supplied intent/manifests retained, no
live-state filtering or RV comparison during normalization.

Per55 exact DTOs validate required/nullable/optional presence, duplicates/unknowns,
valid UTF16/strict numeric forms before digest. Reuse P04 exact ID/RV/UUID/money/
date/time/score; NFC/trim/text/email/phone/code policies only for fields declaring
normalization. Optional staffphone/paymentreference omitted/blank/null become null;
teacherComment omitted stays omitted vs nullclear. Required arrays serialize[];
unique numeric/composite sets sort canonical identities and reject duplicates,
not dedup. Program desired components existing IDs then null IDs normalized-name/
kind/weight, names under E03 CI accent-sensitive policy; weekly weekdays sorted,
candidates ordered concrete plan validated date/start order, never repaired by sort.
Guard/absence/source items never dropped because unchanged or removed from target.

Checks: independently read registry parity55/6/all metadata; frozen literal F1/F2/
F4/F5 exact lengths+SHA from accepted doc; F3 omitted/null distinction using complete
valid manifests. Equivalence NFC/phone/score/field-order/whitespace/setpermutation;
negative changedRV/absence/target/action/full source/unknown/duplicate/badUnicode/
numbercoercion/overprecision and ordered-plan corruption. Verify all action schema
coverage, control escape/u2028/slash/emoji/ticks not inadvertently normalized.
No claim that registry coverage executes domain mutations or proves R02 completion.

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
Worker234f6d2a-ef7 SUCCESS/83diff lines;5observed reads/1write/full3contexts/0violations.
Compact STRING contract/receipt/scope audit retained; no raw thinking/transcripts/logs.
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
