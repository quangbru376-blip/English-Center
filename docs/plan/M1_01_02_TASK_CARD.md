# M1 packages 1 and 2 — assigned 2026-10-09

Status: implementation in progress; no PASS or main application yet.
Owner assigns full code/review/tests/PASS application; ADR177/163 apply.
Manager checkout .worktrees/m1-packages starts at d46ff8a; owner4 absent here.

Scope: API006–014 staff search/create/reset/lock/unlock/disable/restore;
API074–080 Course CRUD-by-contract/lifecycle and full read-only seeded Band;
API090 lookup restricted to four Course actions. No Band mutation, programs,
admissions, payment, classes, M2–M4, schema/package/install/push/deploy changes.
Reuse session/CSRF/deadline/Account->SID locks, EC-C1/T23 and mandatory audit.
Staff secrets first response after owned COMMIT only, replay safe completion,
32 random characters/24h/must-change, reset preserves status and revokes all.
ADMINSET protects self/last active Admin; optimistic versions and no repeated effects.
Course code immutable/unique, exact positive VND string, no-op400, no cascading
historical changes; Band full coherent DB set/frozen flag without GET repair.
Reads: initial live authority, separate SNAPSHOT buffered data, final live authority.
UI: real role-scoped endpoints, search PII body/RAM, one-time credential clear/copy/
ack/expiry/logout, explicit confirmations/unknown manual lookup/no automatic retry.

implementation_skills: backend-clean-code, frontend-ui
review_skills: code-review (Codex)
skill_sources: .agents/skills/*/SKILL.md; hashes recorded before delegation.
applicable_overrides: SKILL_ROUTING §2; accepted E04/requirements override generic
REST/bearer/SQLite/pagination/money rules. No new packages or business limits.
read_context: ordered memory, requirements, E04 DTO/catalog/final/replay/SQL,
W01/W03/W13, existing identity/admin/SQL/UI code and relevant test fixtures.
skill_verification: pending diff/read-event review and independent validation.

AGY: sole pure Course form, <=130 lines/one file; read only M1_COURSE_FORM_WORKER,
frontend-ui/SKILL.md and SKILL_ROUTING; no source/owner/private files/commands/network/
SQL/Git. Standing assigned-aspects authority covers this explicit reduced context.
Codex retains all sensitive transaction/runtime work and final review/tests/apply.

Verification: full existing backend/frontend regression; meaningful parser and
service guards; real isolated owned-COMMIT SQL/API tests (existing scratch fixture
with ID/GUID checked cleanup, never D07/operational fake data), rollback/audit/replay/
stale/role/session/self/last-admin/races, actual Edge 375/768/1280 UI checks.
LOCAL_DEV mounting and explicit reviewed artifact re-pin after source build needed
for usable host; no automatic drift repair. Protected keys/credentials never shared.
Owner4 metadata hashes before/after and exact staging allowlists mandatory.
