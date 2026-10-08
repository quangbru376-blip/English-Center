# V01-G — unmounted typed profile and readiness gates

2026-10-08; base main a1bbea1. Assigned continuation of F readiness closure.
Authority ADR177 covers bounded implementation/context sharing; ADR163 PASS apply.
Status: PREPARED, not runtime readiness or full V01/M0 completion.

Implement immutable typed descriptors for essential startup groups: explicit
profile kind, runtime/patch manifest reference, database secret/schema references
and DB/EF160/snapshot true; UTC+7; HTTPS origin/explicit listen address, protected
transport trust reference, explicit trusted forwarding IP/CIDR sets; JWT references/
issuer/audience/protected persisted CSRF reference; offline blocklist manifest;
finite deadlines/limits; replay and security-audit maintenance references/interval.
Only references, never load keys/config/secrets. Fixed business timers/EC-C1/CSV/
retention remain accepted constants and existing implementation, not tunable here.
Backup/restore values are a separate operation prerequisite, not invented here.

Pure validation: missing/invalid slots fail closed with fixed enum causes only;
no default profile/port/origin/budget, wildcard bind/trust-all/HTTP downgrade.
CSV whole-response capacity >=5000000 bytes, positive body/weekly preview budgets
(environment resources, not business lesson caps). Deadline SQL conversion bounded
to int.MaxValue milliseconds; lock < SQL < request, CSV/readiness <=request.
Actual remaining-budget lock helper rejects equality/nonpositive remaining time.
Profile/nested descriptors override ToString to fixed names without values.

Readiness orchestration requires one probe for EACH essential category:
RuntimeManifest, TimeZone, DatabaseAuthorityAndCatalog, Transport, JwtKeys,
ProtectedCsrfKeys, OfflineBlocklist, ExecutionLimits, MaintenanceConfiguration.
Missing/duplicate/unknown categories, invalid profile, false/exception/timeout =>
not_ready/503. Success only all fresh positive probes, ready/200. No cached ready.
Caller cancellation propagates; finite overall readiness deadline linked to every
probe; overdue/uncooperative async probe cannot block gate indefinitely. Observe
late faults; cancellation-aware resource disposal remains concrete probe duty.
Only fixed result enums exposed internally; public result literal ready/not_ready.
No actual probes registered or fabricated production evidence; database adapter
deferred until bound least-privileged principal can prove exact catalog/snapshot.
Inspector diagnostic requireRuntimePrincipal=false must never count as readiness.

Manager allowed code: Infrastructure/Readiness/RuntimeProfile.cs,
RuntimeProfileValidator.cs, RuntimeReadinessGate.cs and tests/Readiness/*.cs.
Worker sole DeadlineBudgets.cs <=65 lines. Docs G card/brief/contract/evidence/notes,
affected memory/plans/architecture. No Program/DI/routes/frontend/migration/grants/
SQL/listeners/real environment values/package/lockfile/install/push/deploy edits.
Four owner untracked files preserved hash-identical/unshared/unstaged.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
skill_sources SHA256: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
Complete backend/review/routing/AGY skill+contract read. applicable_overrides:
JWT cookie/framework CSRF, exact E04/SQLServer160/no SQLite/no packages/no new API.
read_context: ordered Project Memory, NEXT/HANDOFF, requirements4.10,
E04_RUNTIME_PROFILE, V01_READINESS_CLOSURE, inspector/Program and E/F evidence.
Worker context exact G_WORKER +backend skill+routing; complete reads before write;
no source/owner/secrets/commands/network/SQL. Standing ADR177 covers exact payload.
skill_verification: pending receipt/tools/diff plus independent Codex checks.

Checks: missing/unknown/unsafe slots, immutable/redacted descriptors, finite and
overflow/equality/remaining deadline boundaries; missing/duplicate/unknown probe,
all fresh success, failure/exception/sanitized cause, timeout/uncooperative probe,
caller cancellation and no stale success. Build --no-restore -warnaserror;
default full non-SQL tests on manager and main. Existing55 SQL/browser opt-in cases
remain skipped; G supplies no new SQL/HTTPS/operator-readiness evidence.
