# V01-G — unmounted typed profile and readiness gates

2026-10-08; base main a1bbea1. Assigned continuation of F readiness closure.
Authority ADR177 covers bounded implementation/context sharing; ADR163 PASS apply.
Status: completed/reviewed PASS/applied ADR163/reverified manager and main.

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

Future concrete probe obligations (G implements orchestration only): runtime probe
must verify compatible actual patch/lock manifest; timezone probe must resolve OS
equivalent UTC+7 without host fallback; database probe must prove least-privileged
principal AND exact schema/modules/results/DB160/snapshot ON; transport probe must
prove actual origin/listen/trust/known-hop and secure cookie/CSRF policy; key probes
must verify usable cryptographic material/rotation/issuer/audience/session lifetime
and actual protected persistent framework store; blocklist probe must check shipped
manifest/integrity/encoding/provenance; execution probe must verify applied budgets/
limits/fixed timers/EC-C1/CSV; maintenance probe must verify configured separate
principal/jobs/retention. Descriptor strings alone are never positive evidence.
Probes must return an asynchronous task promptly; this gate bounds async waiting,
not arbitrary synchronous blocking or forced termination of third-party code.

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
skill_verification: AGY2b90a6c8-151 SUCCESS/exit0; observed complete18/46/101-line
authorized reads in order before sole write, own37-line read, no other tools/reads
or commands/network/SQL/observed scope violations. Original helper retained; only
trailing blank EOF normalized (integrated36lines), no semantic manager change.
Receipt/events hashes in G evidence. Tool events are not an OS-enforced read
sandbox or proof of complete understanding. Codex independently reviewed/tested.

Checks: missing/unknown/unsafe slots, immutable/redacted descriptors, finite and
overflow/equality/remaining deadline boundaries; missing/duplicate/unknown probe,
all fresh success, failure/exception/sanitized cause, timeout/uncooperative probe,
caller cancellation and no stale success. Build --no-restore -warnaserror;
default full non-SQL tests on manager and main. Existing55 SQL/browser opt-in cases
remain skipped; G supplies no new SQL/HTTPS/operator-readiness evidence.

Manager final:92new +891existing =983PASS/55intentional opt-in SKIP/0fail;
solution build --no-restore -warnaserror0warnings0errors. Initial90 targeted PASS;
full run982PASS/1new-test array-covariance error/55SKIP; fixed interface array
construction, then full final983PASS. Earlier xUnit cancellation analyzer errors
fixed without suppression. Core review fixes recheck mutable probe descriptors,
sanitize throwing descriptor, reject whitespace origin, propagate caller cancel
even when a probe throws, and recheck overall deadline before ready.
Review: PASS for bounded G core, no unresolved checks in assigned code scope.
API/DTO/UI/business/money/migration/SQL checks N/A: none changed. Full V01/runtime
remains incomplete until actual dependency adapters and active environment evidence.

Main reverified:983PASS/55intentional opt-in SKIP/0fail; solution build0warnings0errors.
Initial sandbox compiler process cancellation resolved by authorized normal build;
no test/code waiver. Code main233a274/managerd497384; src and test trees identical.
Default entries/frontend/package/schema/grants unchanged; SQL not rerun.
Four owner hashes verified/untracked/unshared/unstaged. Originals/receipts retained.
