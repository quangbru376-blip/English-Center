# A02 — operator bootstrap and isolated test identities — preparation

Owner2026-10-05 authorized two next families A02→A04 after code74e3085/docs08ff5ba.
Baseline513unit+366SQL PASS. ADR163 applies reviewed authorized PASS stages only.
implementation_skills: backend-clean-code; review_skills: code-review (Codex);
integration: antigravity-worker. Complete selected skills and routing read this turn;
versions unchanged from D13/A01; hashes recorded in their task cards and rechecked.
applicable_overrides: accepted E03/E04/requirements, PasswordHasher/raw password,
SQLServer160/real fixtures, no production seed/startup mutation/runtime bootstrap.
read_context: ordered memory/current ADRs/requirements/HANDOFF/FIRST_CODE_HANDOFF,
M0_QUALITY_PLAN, E04 runtime/SQL and existing P08/EF/audit/SQL test foundations.
skill_verification: preparation only; no worker dispatch or PASS claim yet.

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
