# A02 — operator bootstrap and isolated test identities — reviewed PASS (detached)

Owner2026-10-05 authorized two next families A02→A04 after code74e3085/docs08ff5ba.
Baseline513unit+366SQL PASS. ADR163 applies reviewed authorized PASS stages only.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete selected skills and routing read this turn;
versions unchanged from D13/A01; hashes recorded in their task cards and rechecked.
applicable_overrides: accepted E03/E04/requirements, PasswordHasher/raw password,
SQLServer160/real fixtures, no production seed/startup mutation/runtime bootstrap.
read_context: ordered memory/current ADRs/requirements/HANDOFF/FIRST_CODE_HANDOFF,
M0_QUALITY_PLAN, E04 runtime/SQL and existing P08/EF/audit/SQL test foundations.
skill_verification: complete source review and independent gates PASS; see verified result below.

AGY writes only Common/Security/FixedRoleCodes.cs, from A02_ROLES_WORKER.md plus
backend skill/routing, one file/160lines. No commands/SQL/secrets/other reads.
Codex implements service/repository under Services/Bootstrap and Infrastructure/
Bootstrap, disconnected unit and real SQL tests plus affected docs. Current packages
only; no schema/migration/HasData, runtime connection/DI/routes or provisioning.

Bootstrap is explicit operator invocation with supplied context/profile, normalized
Admin email/name and secret supplied only in memory; P08 policy/hash, no literal
credential/hash fixture or config dump/log/result echo. Atomically ensure exactly7
fixed role codes and at most initial Admin; serialize concurrent bootstrap with
SQL locks and caller-owned TX, no hidden connection or own commit/retry. Existing
Admin (including LOCKED/DISABLED) is not reset/unlocked/overwritten; repeated calls
do not duplicate roles/accounts or alter credentials/status/session/history.
Unknown role set/email collision/cancellation/SQL failure fail closed and rollback.
No plaintext test-identity catalogue or real students: seven role test accounts only
inside rollback fixtures with fresh generated in-memory secrets, separate from real
Admin provisioning. No startup or runtime-role bootstrap path.

Proposed SQL: ONLY EnglishCenter_D07_20261005; bootstrap and seven-role synthetic
accounts inside rollback transactions. Full existing SQL regressions reuse only
D12_fixture_user_0..3, prior transient DENY/catalog drift fixtures and rollback;
no persistent seed/schema/grants/principal/owner/profile/logins/other DB change.
Real Admin secret/provisioning and full runtime/demo identities remain deferred;
the completed implementation must not claim that a real Admin was created.
Gates: worker receipt/scope; locked build/unit/full SQL; repeat/no overwrite,
concurrency/failure/cancellation, runtime bootstrap denied; unchanged catalog and
zero fixture residue. Codex code-review and owner walkthrough before PASS/apply.

## Verified result — 2026-10-05

Owner explicitly approved exact A02→A04 payload and rollback-only SQL scope this turn.
No reauthorization inferred from worker claims. ADR163 permits apply after PASS.
Selected complete skill byte SHA256 in detached checkout (Git LF representation):
- backend-clean-code: `76acec868ed4d918dc1c6616904f78ff2710737d6a06ccf605da783bc914dd23`
- code-review: `1dded1e6699f97c7657c27d2b923992d07ac88bd9642fb74ebe4d93a28be2125`
- routing: `adb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915`
- antigravity-worker: `eb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b`

Worker 98515675-629: SUCCESS/exit0, one authorized helper; complete own card/backend/routing
reads before write, zero observed scope violations. Original worker/manager trees retained.
Manager scope, accepted E03/E04 overrides and source review: PASS, no unresolved findings.
Detached full gates:545unit+375SQL=920PASS/0fail0skip; build0warnings0errors; cache-only
locked restore. SQL exclusively EnglishCenter_D07_20261005; fixtures rollback.
No dependency/mapping/migration/SQL-resource/Program/profile change. No real Admin/runtime
provisioning, other DB/owner/login/persistent grants/keys, push or deployment.

Final scope also extends Common/Auditing/AuditEntry.cs with a narrow internal initial-Admin
factory. SYSTEM.ADMIN.BOOTSTRAP has no pre-existing actor; projected positive roleId,
ACTIVE/mustChangePassword=false only, no operator/email/name/hash/password/IP/expiry.
Operator supplies a regular P08-compliant secret only in RAM. Repository appends this
mandatory event on the caller SERIALIZABLE transaction, never commits or retries.
Canonical EC:ADMINSET lock plus update/range locks serialize creation; explicit finite
SQL/lock deadlines required. Runtime lacks Role INSERT and is denied before bootstrap.
Repeat preserves ACTIVE/LOCKED/DISABLED Admin hash/status/security state; incomplete
fixed-role catalogue fails closed. No reset/unlock/repair of existing Admin.
New SQL9cases: creation/audit/repeat3, seven-role generated secret identities1,
collision+pre-cancel1, runtime denial1, concurrent gate contention1, failure/cancel
following role writes2. All rollback; gate contention does not claim committed load
stress. Injected persistence error assertion corrected to EF DbUpdateException with
InvalidOperationException inner cause; no product code change for that test failure.
Existing A01 actual mandatory audit denial/failure tests remain PASS; no new claim of
bootstrap-specific SQL audit DENY execution. Fixed-role unit1; stage-only main gate514unit.
Owner walkthrough: docs/notes/A02.md. Real Admin provisioning remains deferred.
