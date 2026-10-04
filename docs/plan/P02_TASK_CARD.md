# P02 — backend and test skeleton

Date: 2026-10-04. Status: prepared; worker not dispatched.
Manager: Codex. Implementer: Antigravity via antigravity-worker.
Authority: current owner request and FIRST_CODE_HANDOFF, P01 prerequisites →
P02 only. Applying code to primary is separately prohibited until owner approval.

## Objective and dependency evidence

Implement one conventional net10.0 backend/test skeleton that builds and whose
xUnit tests start the real API entry point using WebApplicationFactory.
The empty host must not expose business routes or claim readiness.
P01 manifest: M0_LOCAL_PROFILE.md. SDK 10.0.200/runtime10.0.4/AGY1.2.16 verified;
both solution formats supported, explicit .sln selected. Exact four direct
test dependencies and nuget.org source resolved; actual restore compatibility
is a manager verification gate. No EF or new framework selection.

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
Sharing these six context files is pending explicit scoped consent; the old
analyze smoke consent is not reused as blanket consent.

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
skill_verification: pending actual worker events, diff, compiler and tests.

## Owner walkthrough (skeleton only)

1. Program builds an empty ASP.NET Core host, exposing no application endpoints.
2. The test project references that real entry point and hosts it in memory.
3. Requests demonstrate that missing functionality/profile cannot report success.
4. Solution/SDK/dependency pins make builds reproducible; locks follow restore.
5. All source remains in a detached worktree until owner approves application.

## Execution record

Pending: scoped consent, validated English contract, detached worker run,
manager build/discovery/tests/audit/review. P01 only resolved P02 prerequisites;
full LOCAL_DEV remains unconfigured. P03/SQL/rest M0 are outside current scope.
