# V01-E — owned SQL commit, independent audit and bounded races

2026-10-08; base main95e92c2. Owner continuation/ADR177 authorizes all necessary
task aspects; ADR163 applies only independently reviewed PASS. Status: completed/reviewed PASS/applied/reverified.
Codex manager owns privileged fixture, execution, review and cleanup.

## Concrete SQL lifecycle and safety scope

Only local lpc:.\SQLEXPRESS. Original EnglishCenter_D07_20261005 is read-only
baseline before/after. Each case creates ONE previously absent scratch database
named EnglishCenter_V01E_20261008_<32hex GUID>; never adopt/reuse an existing DB.
Explicit collation Latin1_General_100_CI_AS_SC, compatibility160, snapshotON and AUTO_CLOSE OFF;
apply ONLY existing six EF migrations using cached dependencies/no restore.
No edits to migrations, original DB/catalog/permissions, server login, real
operator account or production profile. SQL diagnostic Windows connection is
not proof of configured least-privileged runtime identity.

Seed only synthetic seven Roles/eight Accounts (two Admins)/one StudentProfile,
generated password/hash/JWT key RAM-only. Owned production executors must use
fresh independent DbContexts and their real transactions/COMMIT; no caller outer
transaction, fake executor, persisted response/credential snapshot or API mock.
Observe persisted effects through additional connections after completion.
Use actual SqlSecurityAuditStore fresh connections, including bounded real table
lock/timeout failures; no temporary user/DENY/grant needed.

Cleanup finally disposes all own contexts, clears only own connection pools and
drops ONLY the exact database this fixture successfully created, after matching
database_id/database_guid from master and validated fixed name/GUID suffix.
Ownership metadata name/id/guid is local ignored evidence, no connection secret.
Never DROP existing D07/other DB, never broad wildcard cleanup; fail closed on
ownership mismatch. If interrupted, use that receipt to prepare verified exact
cleanup before continuing; do not infer authority from a matching name alone.
Assert no V01E scratch databases before/after suite and unchanged original baseline.
Test profile budgets SQL10s/lock1000ms, security audit1s timeout in failure case;
these are bounded fixture values, not operator profile/SLO decisions.

## Twelve cases

1. Owned signup persists Account/Profile/mandatory audit, no AuthSession.
2. Two clients same signup email/phone: one committed result/one conflict,
   exactly one Account/Profile/audit, no duplicate effects.
3. Two login SIDs persist; current me reads; owned logout revokes current only;
   independent LOGIN/LOGOUT security history visible and other SID survives.
4–5. Normal/restricted own password commits hash/version/temp clear/all-own-SID
   revoke/mandatory audit, old proof rejected, deliberate new login required.
6. Held AuditLog table gate causes real mandatory persistence failure after write;
   owned password transaction rolls back hash/version/session effects.
7. Independent security audit timeout after login/logout COMMIT cannot undo their
   outcome; incident counter increments and no partial history is claimed.
8. Production Admin detail executor uses initial live TX, separate SNAPSHOT and
   final live TX; record transaction isolation/independent context identities.
9. Revoke Admin between snapshot buffer and final guard; buffered detail denied.
10. Two clients same Admin lock UUID: one mutation/audit/T23, same operation ref.
11. Two different UUIDs/same target RV: one commit/one conflict, one marker/audit.
12. Password change versus current-SID logout: one valid serialized outcome;
    surviving/revoked SIDs/hash/version/audit match the actual winner.

Races use separate owned connections, an explicit simultaneous-start barrier and
bounded completion; no load/throughput/large stress claim. Direct owned executor
proof complements D browser rollback, not browser→owned COMMIT/runtime readiness.

## File and delegation scope

Manager adds ONLY OwnedSqlFixture.cs and OwnedSqlExecutorTests.cs under
backend/tests/EnglishCenter.Tests/Api/Integration; worker sole OwnedAuditAssertions.cs
<=85lines. No production source/default entries/frontend/packages/locks changes.
Docs: this card/V01E_WORKER/E-worker-contract, sanitized E evidence/notes and
affected Project Memory/active plans. No raw TRX/worker reasoning committed.

AGY reads ONLY V01E_WORKER.md, complete backend-clean-code/SKILL.md and
SKILL_ROUTING.md; no links/source/AGENTS/report/owner/secrets/listing/search,
commands/network/SQL/Git/validation. Codex validates clean manager contract before
dispatch, reviews actual tool events/diff and runs all checks independently.
External sharing covered by ADR177 for this concrete bounded task, no blanket
source sharing inferred. Keep original manager/worker worktrees and receipts.

implementation_skills: backend-clean-code; review_skills: code-review (Codex).
skill_sources SHA256: backend726586298710499cd9e5ce7a1e3a2a9fcfd00c07c1f73dfce34cf58e7da8ee3b;
review04b72a9627a717bbdf017a3275974b62bbe41f23440e86e5341954e8863a7e70;
routingadb7938fe69e00c991aefb4eb114854f124eb3e2ac3963763937225646d3e915;
AGYeb6dc1064b48ec44754406ee804cf287be7b3b9243e2d5c80fdacbdd3364c54b.
applicable_overrides: requirements/E04 exact inputs/DTO/errors, JWT-cookie proof,
mandatory caller-TX services/owned commit, SQLServer160, no bearer/SQLite/packages.
read_context: ordered memory/requirements/handoffs/V01D and actual owned executor/
audit/replay/auth sources; complete selected skills/policy/AGY contract already read.
skill_verification: record actual events/source checks/results in final evidence.

Default gate EC_V01_OWNED_SQL=1 in addition to EC_V01_SQL=1; skip before any
connection/database creation otherwise. Build --no-restore -warnaserror; run exact
12cases sequential collection, then default SQL/socket-free suite. Apply reviewed
PASS under ADR163 and reverify exact cases/main cleanup. UI/API/migration changes
N/A for this fixture; notes <=15lines. Full V01/M0/profile readiness still incomplete.



## Evidence and final review

Manager3a98377/main95bb665 share backend test tree
55255cc6d158eeb3c6b35b4d373ddfc48d7a89dd. Exact12cases PASS/0fail0skip on both;
default891PASS/49intentional opt-in SKIP/0fail/940total; builds0warnings0errors.
Raw TRX/ownership receipt remain ignored; sanitized results/hashes in
V01-evidence/E-verification-summary.json; owner explanation notes/V01E.md14lines.
No frontend/API/DTO/schema change; historical D frontend48PASS not rerun.
Original production src trees/default entries/packages unchanged, four owner files
hash-identical/unshared/untracked/unstaged. Scope checks PASS, no unresolved E checks.

AGYe3b66916-9de SUCCESS sole76line assertion; observed full32/46/101 input reads
before write and own76line output read only; no commands/network/SQL/other paths.
This is observed tool behavior, not OS-enforced read sandbox or proof of comprehension.
Original output/receipt/events retained; Codex fixed xUnit analyzer and TryGet
numeric diagnostics, then integrated and independently compiled/executed.

First manager run11PASS/1cleanupFAIL: inherited AUTO_CLOSE ON briefly hid recovery
GUID. Read-only probe matched exact retained receipt name/id/guid; manager dropped
only that scratch DB and logged recovery. Scratch AUTO_CLOSE OFF fixes ownership
visibility; final manager/main12PASS. All36created DBs accounted for:35normal DROP,
1exact recovery DROP,0remaining. D07 baseline before/after byte-identical with
catalog/profile/102permissions digest0DE381587B6706797C2F38FB3C64DE20A3E19083E391E4684EF3820CEE6FBEF7;
24empty tables/0fixture users/members/4roles/6migrations/compat160/snapshotON unchanged.

Review verdict PASS for bounded V01-E, applied ADR163 and reverified. Direct owned
COMMIT/independent audit/SNAPSHOT/two-client races are now evidenced; diagnostic
Windows identity does not prove least-privileged runtime profile. Full V01/M0
remains incomplete; next readiness closure matrix/contract for production profile,
principals/PKI and combined browser→owned COMMIT. No unrelated tasks/push/deploy.
