# P02 — backend and test skeleton

Date: 2026-10-04. Status: reviewed PASS after manager fixes; owner-authorized main apply complete and reverified PASS.
Manager: Codex. Implementer: Antigravity via antigravity-worker.
Authority: current owner request and FIRST_CODE_HANDOFF, P01 prerequisites →
P02 only. Owner separately confirmed application of this exact reviewed result
to main on 2026-10-04. This grants no later task/push/deployment authority.

## Objective and dependency evidence

Implement one conventional net10.0 backend/test skeleton that builds and whose
xUnit tests start the real API entry point using WebApplicationFactory.
The empty host must not expose business routes or claim readiness.
P01 manifest: M0_LOCAL_PROFILE.md. SDK 10.0.200/runtime10.0.4/AGY1.2.16 verified;
both solution formats supported, explicit .sln selected. Final four direct
test dependencies and nuget.org source verified by restore/build/discovery/tests.
Manager selected xunit.v3.mtp-off 4.0.1 after actual compatibility failure of
the initial xunit.v3 4.0.1 variant. No EF or new framework selection.

Reviewed prior documentation snapshot: 1054888. The actual clean dispatch HEAD
is captured in the English contract objective and runner preflight evidence
after this preparation is committed; do not hardcode the old handoff HEAD.
Final result must record the actual full base SHA and detached HEAD independently.

## Worker implementation paths — exact allowlist

1. .gitignore
2. backend/global.json
3. backend/NuGet.Config
4. backend/EnglishCenter.sln
5. backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj
6. backend/src/EnglishCenter.Api/Program.cs
7. backend/tests/EnglishCenter.Tests/EnglishCenter.Tests.csproj
8. backend/tests/EnglishCenter.Tests/Infrastructure/StartupTests.cs

Limits: eight changed files, 320 added/deleted lines. No folder-wide writes.
Manager restore may additionally generate only the two project-level
packages.lock.json files as approved bootstrap artifacts; manager reviews them
separately. Worker must not create lockfiles or invoke commands.

Forbid docs/**, instructions/**, .agents/**, .codex/**, .git/**, frontend/**,
**/Migrations/**, **/appsettings*.json, **/launchSettings.json, **/.env*,
**/*.pem, **/*.key, **/*credentials*, **/packages.lock.json and all files
outside the eight implementation paths. No schema/DDL/DML, business entities,
controllers/DTOs, auth, middleware from later P tasks, OpenAPI/weather examples,
readiness/liveness endpoint or deployment. Never inspect user-profile credentials.

## Authorized read context proposed for one run

- docs/plan/P02_TASK_CARD.md (complete)
- docs/plan/M0_LOCAL_PROFILE.md (complete)
- .agents/skills/backend-clean-code/SKILL.md (complete)
- docs/plan/SKILL_ROUTING.md (complete)
- docs/plan/FIRST_CODE_HANDOFF.md (complete)
- AGENTS.md (complete)

After context, worker may inspect only the eight implementation paths if they
exist. This owner-authorized bounded task overrides broad linked-document
loading for the worker: Codex has read canonical memory/accepted sources and
provides the relevant P02 constraints here. Links grant no extra read scope.
No discovery/search/directory scans, browser/network/MCP/subagent tools or shell.
Manager must verify actual tools/events, not just a claimed receipt.
Owner explicitly approved sharing exactly these six context files and existing
eight implementation paths after contract validation, before dispatch.
The old analyze smoke consent was not reused as blanket consent.

## Acceptance criteria

- Exact layout with API/test projects in the solution; net10.0, nullable,
  implicit usings, SDK analyzers, TreatWarningsAsErrors enabled.
- global.json SDK10.0.200/rollForward disable/test.runner VSTest; no SDK install.
- NuGet.Config clear inherited sources and add only nuget.org v3.
- API package-free minimal CreateBuilder/Build/Run entry point, public partial
  Program for the real test host. No fake success or route mapping.
- Tests use only the four manifest dependencies; OutputType Exe/IsTestProject,
  project reference to API, test packages private as appropriate. Lockfile
  generation enabled; no coverlet/extra packages/template install.
- StartupTests exercises WebApplicationFactory<Program> of the real API.
  Parameterized requests to unimplemented root/auth/health paths return 404
  with an empty body in an isolated in-process host with no business profile.
  At least two environment cases (Development and Production) must show the
  same absence of success; no Kestrel network listener or DB/config secrets.
- No trivial Assert.True(true), duplicate simulated host or mock business APIs.
- .gitignore covers bin/obj/test results and local secret files, never locks or
  source; do not create empty marker files or abstractions without current use.
- Codex independently restores, locks, builds with warnings as errors, discovers
  and runs tests, audits dependencies, scans generated source and reviews diff.

## Skills and canonical overrides

implementation_skills: backend-clean-code.
review_skills: code-review (Codex); antigravity-worker protocol (Codex).
skill_sources (SHA256, checked 2026-10-04):

| Source | Hash |
| --- | --- |
| .agents/skills/backend-clean-code/SKILL.md | 726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B |
| .agents/skills/code-review/SKILL.md | 04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70 |
| docs/plan/SKILL_ROUTING.md | ADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915 |
| C:/Users/asus/.codex/skills/antigravity-worker/SKILL.md | EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B |

applicable_overrides: E03/E04/requirements/ADR160–162 win over stale skill
wording; no bearer/browser JWT, BCrypt, SQLite SQL-evidence, extra packages,
generic repository/MediatR or invented routes. Full-set DTO/money/time/save-unit
rules remain unchanged but are not implemented by P02. No migrations needed
without schema changes. No controller tests: these are actual host boundary
tests. docs/api.md is N/A with no implemented business routes; short owner
notes below replace a separate worker docs edit. Frontend checks N/A.

## Manager verification and review gates

Run exact commands in M0_LOCAL_PROFILE from detached backend. Inspect receipt,
SUCCESS/exit0/policy violations, all tool calls, actual changed files/line count,
package graph/lock hashes/licenses/vulnerability report and git diff --check.
Check detached HEAD equals dispatch HEAD and primary has no backend code.
Review verdict must be PASS/PASS WITH FIXES/FAIL with unresolved evidence.
No SQL/grant/race/security/business readiness claim from startup tests.
skill_verification: complete. Codex reviewed all actual tool calls, all source,
the final lock graph, compiler/analyzer output and ten real host tests.

## Owner walkthrough (skeleton only)

1. Program builds an empty ASP.NET Core host, exposing no application endpoints.
2. The test project references that real entry point and hosts it in memory.
3. Requests demonstrate that missing functionality/profile cannot report success.
4. Solution/SDK/dependency pins make builds reproducible; locks follow restore.
5. Owner approved main application; the original detached review tree is retained.

## Execution record

### Identity and isolation

- Prior pending documentation reviewed/preserved: 1054888.
- Clean validated dispatch base and initial detached HEAD independently checked:
  72696e5cba4670344434a98e3cd1562ddeeded20.
- Run 11968703-a8c; conversation 716c7731-129a-4e30-98cc-7b8e3a02af3f;
  AGY SUCCESS/exit0, runner review_required, zero write-policy violations.
- [Original English contract](p02-evidence/worker-contract.json) and
  [original receipt](p02-evidence/worker-receipt.json) are immutable run evidence;
  initial dependency wording reflects the original dispatch, not the manager fix.
- Detached worktree:
  C:/Users/asus/AppData/Local/Temp/codex-antigravity-worker/worktrees/main_workplace-11968703-a8c.
- Final reviewed detached commit: d9eb194a7d53c321cbfad691abba730388588385.
  Diff against dispatch base: ten files, 576 Git insertions, zero deletions;
  eight skeleton files 141 lines plus two manager lockfiles 435 lines.
  Runner counted 149 worker lines (includes trailing split lines), within 320.
- Primary received documentation/evidence only; no backend/.gitignore source,
  merge/cherry-pick/apply/push. Worktree retained; owner application still pending.

### Actual worker behavior and manager corrections

Codex inspected every tool event: 24 tool invocations, only view_file (16) and
write_to_file (8). Six complete context reads, two allowed missing-file probes,
then eight final-file reads; no extra paths, commands, browser/network/MCP or
subagent calls. Worker read the complete backend skill and routing policy before
editing. All eight actual writes matched the exact allowlist.

Original worker code did not pass the manager gates unchanged. Codex made two
small corrections within existing detached paths, with backend-clean-code and
canonical overrides retained:

1. StartupTests.cs: supply TestContext.Current.CancellationToken to GetAsync
   and ReadAsStringAsync; resolves two xUnit1051 errors without suppressions.
2. Test csproj: replace xunit.v3 with official xunit.v3.mtp-off at the same
   4.0.1 pin. Default package MTP v2 rejected VSTest on SDK10 during actual
   discovery/test. Final variant keeps accepted xUnit/VSTest and four direct
   dependencies; verified official registry metadata before corrected restore.

Manager generated/reviewed exactly two packages.lock.json files. No other code
edits, runtime install, SQL connection/configuration, endpoint or framework.

### Independent verification (final source)

Commands are the exact M0_LOCAL_PROFILE commands, from detached backend:

| Gate | Actual result |
| --- | --- |
| Restore via NuGet.Config | Exit0; API package-free, test graph resolved. |
| Locked restore | Exit0; final manifests/lockfiles consistent. |
| Build --no-restore -warnaserror | Exit0; 0 warnings, 0 errors. |
| Test --list-tests | Exit0; ten distinct discovered theory cases. |
| Test --no-build --no-restore | Exit0; passed10, failed0, skipped0 (626ms reported test duration). |
| Vulnerable --include-transitive | Exit0; no known vulnerable packages reported for either project from nuget.org on 2026-10-04. |
| Graph/license/content hashes | 47 packages: four direct/43 transitive; MIT38/Apache-2.0 nine; every package version/contentHash matches lock/assets/NuGet cache metadata. |
| Scope/source/diff | Exact ten final files; git diff --cached --check passed; no business routes/EF/auth/SQL/secrets/trivial assertion/analyzer suppression. |

Real test name:
EnglishCenter.Tests.Infrastructure.StartupTests.UnimplementedPath_ReturnsNotFoundWithEmptyBody_InGivenEnvironment.
Cases: Development and Production × /, /auth, /api/auth/login, /health,
/health/ready; each actually hosts API Program through WebApplicationFactory
and verifies 404 plus empty body. Probe names are absent paths, not adopted API
contracts. TestServer is in-process; no API network listener or business profile.

Final [resolved package/license/hash inventory](p02-evidence/resolved-packages.json).
API lock SHA256 A4364D81A6DE2707D66FBCED18FBBC926C882AB96E1D1AEBBB17E871285B9D75;
test lock SHA256 15D9472972F2CA4414557E0410944F088F1583C68A5899DFD465929D11B8BC8A.
Contract SHA256 7B9B91891DE33D0BA97CB3FC6AB3CB894846761A621DA583551D477189A5AB4C;
receipt SHA256 45223BDFC243EA575CD98A1DE5FA4F0970B0D1D8C4ED5567C054D2456AF4ED65.
Raw events retained only in the local runner run directory (not Project Memory);
events SHA256 13C09F5EB73090AEDF685BF2BB39949BD082BDBEC61E47D372AEBB8B6DF2E003.
NuGet normalized contentHash is distinct from a signed archive's raw SHA512;
audit compares normalized lock/assets/cache metadata, not unlike hash formats.

### Review verdict and remaining boundary

**PASS for final detached P02 after both manager fixes.** No unresolved P02
must-fix findings. Backend skill/review checks for DTO/auth/errors/migrations,
frontend/API docs/business tests are N/A because no such implementation exists;
owner walkthrough above explains every runtime component. Dependency audit is
the current NuGet known-vulnerability report, not a full security certification.

P01 P02 prerequisites complete. Full LOCAL_DEV/profile/DB/readiness/security/
patch/blocklist/backup/SQL/API/browser/race/load/recovery evidence remains
unconfigured or unverified as listed in M0_LOCAL_PROFILE. M0/V01 not done.
Next action: owner direction for a new bounded task. P03/SQL/rest M0 remain
outside scope; do not auto-dispatch.

### Owner-authorized main application — 2026-10-04

Owner explicitly confirmed the proposal to apply code and lockfiles from
d9eb194 to main. Clean primary pre-apply HEAD a6b4fa4f1c00cf6af7ff199221e83bc7f4295be9;
cherry-pick succeeded without conflicts, producing
df7cd9ac3b7f8b54e6b2814255ca5c424d768b99. Exactly the ten reviewed files applied;
no new source edits or dependencies, no push/deployment. Earlier detached-only
statements in the execution record describe the worker/review stage.

Codex used the same backend-clean-code/code-review sources and routing overrides
listed above; no new worker dispatch or sharing needed. Independently reran from
primary backend: locked restore, build --no-restore -warnaserror, test discovery
and test --no-build --no-restore. All exit0; build0warnings/0errors, discovery10,
tests10pass/0fail/0skip (447ms reported test duration). Source diff between
reviewed detached commit and main for .gitignore/backend is empty; both lock
SHA256 values above unchanged. git diff --check passed. **PASS on main.**
No need to repeat unchanged package audit; original graph/provenance/license/
vulnerability evidence above remains applicable. Full LOCAL_DEV/DB/business
readiness and M0/V01 remain incomplete. Original detached tree retained.
