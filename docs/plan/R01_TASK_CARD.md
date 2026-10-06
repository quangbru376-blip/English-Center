# R01 — Canonical registry and EC-C1 digest primitives

Status2026-10-06: task/worker contract prepared; exact external consent pending.
Canonical family M0_QUALITY_PLAN R01; dependencies P04–P06 applied PASS.

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
