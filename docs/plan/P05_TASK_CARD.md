# P05 — Controlled clock and Vietnam time

Owner authorized P05 and automatic main application after Codex review PASS on
2026-10-04. P03 applied as 6cb2590862cac2469e4dce76b704f72cbdd254a0;
main npm ci (ignore scripts), lint, typecheck/build PASS; source equals reviewed
c5e4eac. Only P05 is open; no P04, SQL, migrations, deployment or new packages.

## Contract

Implement exactly four new files (namespace EnglishCenter.Api.Common.Time and
EnglishCenter.Tests.Common.Time):
- backend/src/EnglishCenter.Api/Common/Time/BusinessClock.cs
- backend/src/EnglishCenter.Api/Common/Time/VietnamTime.cs
- backend/tests/EnglishCenter.Tests/Common/Time/BusinessClockTests.cs
- backend/tests/EnglishCenter.Tests/Common/Time/VietnamTimeTests.cs

BusinessClock is a sealed concrete class injected with built-in TimeProvider.
GetUtcNow normalizes offset to UTC and truncates submillisecond ticks (never
rounds into the future). GetToday captures one clock sample and converts to
Vietnam DateOnly. IsBefore(cutoff) uses strict instant comparison; equality is
expired. It must use the raw provider instant for comparison, so precision
formatting cannot alter the guard. Reject a null provider. No DI/endpoint change
until a consumer task integrates this primitive.

VietnamTime is a static conversion helper using explicit Asia/Ho_Chi_Minh through
TimeZoneInfo, never TimeZoneInfo.Local/device timezone or DateTime.Now/Today.
ToDate(DateTimeOffset) uses that zone. ToUtc(DateOnly, TimeOnly) accepts minute
precision only; rejects seconds/subsecond input; uses an Unspecified local
DateTime and rejects conversion outside DateTimeOffset representable bounds
instead of allowing TimeZoneInfo silent saturation. InclusiveDateRange(from,to)
returns named (StartInclusive, EndExclusive) UTC DateTimeOffset boundaries for
[from 00:00 Vietnam, day after to 00:00 Vietnam). Reject reversed dates,
unrepresentable lower boundary and DateOnly.MaxValue end with argument errors;
do not clamp. FormatInstant returns invariant yyyy-MM-ddTHH:mm:ss.fffZ after UTC
normalization and truncation. No client parsing or DTO/API error mapping (P06/P07).

Canonical sources: ADR-047/091, requirements C08, E04_RUNTIME_PROFILE,
E04_FINAL_CONTRACT and E04_DTO_SPEC inclusive report periods. Verify OS zone
mapping equivalent to UTC+7 for modern business dates. Historical IANA offsets
remain zone data; do not replace with host zone or invent historical rules.

Tests use test-owned TimeProvider fake with mutable DateTimeOffset, no package.
Cover fixed/moving time, offset and millisecond normalization, null, single clock
sample for today, Vietnam midnight/year/leap-day rollover, before/equal/after
cutoff including submillisecond precision, modern zone UTC+7, minute rejection,
explicit conversion and format with nonzero offsets/invariant culture, inclusive
date periods across month/year/leap day, reversed/max/min representability errors.
Use exact expected instants, no wall-clock assertions or timezone mutation.

## Skills and manager gates

Read complete backend-clean-code and SKILL_ROUTING; Codex uses code-review.
Source SHA256: backend skill
726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review 04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routing ADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
Canonical cookie/session/CSRF and real SQL Server rules override stale bearer/
SQLite skill advice, but auth/DB/routes/DTOs/migrations are N/A in P05. Task card
is the execution slice; no obsolete plan.md/api.md or migration required.

Worker reads only four context files AGENTS.md, this card, SKILL_ROUTING and
backend-clean-code, plus exact four implementation paths if present. Complete
context reads required. No linked files/scans/commands/network/browser/MCP/
subagents/secrets; detached worktree only; four files/600 added+deleted lines.
Codex inspects all tool events, receipt, complete sources and actual scope, then
locked restore, build warnings as errors and full tests (P02 startup preserved).
No fabricated validation claims. PASS permits main apply under standing owner
authorization, followed by main verification. Retain detached evidence.

## Execution

## Verified execution — 2026-10-04

implementation_skills: backend-clean-code; review_skills: code-review;
integration: antigravity-worker (Codex manager). skill_sources and
applicable_overrides/read_context are recorded above; skill_verification PASS.
Context sharing for this bounded P05 continuation follows the owner's explicit
P05 instruction in the established AGY workflow; no wider source/secret access.

Dispatch base 6237ec4; AGY run59975c99-865, conversation
 a026e033-24d7-48f9-8093-6c48011b56b9. SUCCESS/exit0;
receipt exactly4files/494runnerlines/zero violations. All13tool calls audited:
9view_file (4complete contexts,1missing allowed-file probe,4final sources) and
4write_to_file; no scans, commands, network, browser, MCP or subagents.
[Contract](p05-evidence/worker-contract.json),
[receipt](p05-evidence/worker-receipt.json),
[hashes](p05-evidence/verification-hashes.json).
Raw events retained only in local runner directory; no transcript in memory.

Initial worker build0warnings/0errors/tests39pass. Codex found and corrected
ToDate upper overflow: TimeZoneInfo.ConvertTime can silently saturate; explicit
local-tick bound now throws instead. Reject invalid/ambiguous zone times rather
than guessing. Replaced year1 fixed-offset test with representable round-trip,
since historical zone data can vary by OS. Added overflow/non-Gregorian culture
regressions and strengthened submillisecond cutoff test with fractional cutoff.
Final exact4files/522insertions, under600cap; staged diff --check PASS.
No package/project/lock/Program/frontend changes. Historical timezone transition
rules and alternate OS runtimes not executed; modern UTC+7 mapping verified on
this installed Windows/.NET10 runtime. No claim of Linux/historical parity.

Codex verdict: **PASS after manager corrections**, no unresolved P05 must-fix.
Locked restore, warning-as-error build0warnings/0errors, full41tests
(31time +10startup) pass/0fail/0skip in detached worktree. Exact dates/offsets,
millisecond truncation, moving/single-sample clock, equality and fractional
cutoff, minute guard, date ranges/overflow and invariant formatting verified.
Auth/DB/DTO/API/UI/SQL/migration checks N/A: this task adds pure time primitives.
Exception-to-400 mapping and DI integration belong to their future consumers.

Final detached 6e5abe576f90292ab0bf1290885a5448e2eb6b99 at
C:\Users\asus\AppData\Local\Temp\codex-antigravity-worker\worktrees\main_workplace-59975c99-865.
Applied automatically under owner authorization as main
87846864655158d2ae7bff6b311932f59284dffd. Main source equals reviewed detached;
main locked restore/build0warnings/0errors/full41pass/0fail/0skip reverified.
[Test counts](p05-evidence/main-test-counts.json). Both original P03/P05 trees
retained; no push/deployment. P05 complete; next proposed dependency task P04
requires a bounded scope, and is not started by this request.

Code flow: consumers inject TimeProvider into BusinessClock to control now;
VietnamTime converts instants/local minutes and inclusive dates using explicit
Vietnam zone; only formatting/storage clock output truncates precision; cutoff
comparison preserves actual instant. No API consumer exists yet.
