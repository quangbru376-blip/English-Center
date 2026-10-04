# P01 — local bootstrap profile

Updated: 2026-10-04. P02 SDK/package/test-host prerequisites resolved;
restore/build/discovery and ten real host tests verified in the detached P02 tree.
This is a nonsecret inventory and manifest, not a configured business profile.

## Verified inventory and choices

- Windows x64; SDK 10.0.200 / MSBuild 18.3.0; installed ASP.NET Core and
  Microsoft.NETCore.App 10.0.4. Node 24.16.0; AGY 1.2.16.
- Target net10.0; backend/global.json pins SDK 10.0.200 with rollForward=disable.
- SDK help confirms both sln/slnx, default slnx. Choose explicit classic .sln
  for conventional tooling and WebApplicationFactory content-root discovery.
- .NET test runner: VSTest, explicitly pinned in global.json. Use xUnit v3;
  this is the accepted xUnit framework, not another framework. No template install.
- API uses Microsoft.NET.Sdk.Web and the installed shared framework. No API
  package references; no EF, SQL client, Swagger, JWT or frontend packages in P02.
- Tests use Microsoft.NET.Sdk, OutputType=Exe, IsTestProject=true, net10.0,
  ProjectReference to the real API, nullable/implicit usings/analyzers enabled.
- Enable RestorePackagesWithLockFile for both projects. Manager generates and
  reviews exact lockfiles during restore; worker must not fabricate lockfiles.
- Backend NuGet.Config clears inherited feeds and uses only nuget.org v3.
  Restore uses that exact config; locked restore must pass before review closes.

## Exact direct test dependency manifest

| Package | Pin | License | Role/source |
| --- | --- | --- | --- |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.4 | MIT | Real entry-point TestServer, matches installed ASP.NET patch; [NuGet](https://www.nuget.org/packages/Microsoft.AspNetCore.Mvc.Testing/10.0.4). |
| Microsoft.NET.Test.Sdk | 18.10.1 | MIT | VSTest discovery/execution; [NuGet](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/18.10.1). |
| xunit.v3.mtp-off | 4.0.1 | Apache-2.0 | Accepted xUnit framework, explicit VSTest variant for .NET10; [NuGet](https://www.nuget.org/packages/xunit.v3.mtp-off/4.0.1). |
| xunit.runner.visualstudio | 4.0.0 | Apache-2.0 | VSTest adapter; PrivateAssets=all; [NuGet](https://www.nuget.org/packages/xunit.runner.visualstudio/4.0.0). |

Registry and official documentation checked 2026-10-04. Versions are exact
bootstrap selections, not a claim the installed runtime is the latest patch.
Final graph reviewed after restore: 47 packages (four direct/43 transitive),
MIT38/Apache-2.0 nine; normalized content hashes consistent across lock/assets/
cache metadata. NuGet reports no known vulnerable packages on 2026-10-04.
See [P02 task card](P02_TASK_CARD.md) for evidence and exact detached commit.
Do not introduce coverlet, other test frameworks or tools.
[xUnit setup](https://xunit.net/docs/getting-started/v3/getting-started) documents
v3 executable projects and the VSTest adapter/SDK dependencies.
[Microsoft integration testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
documents WebApplicationFactory and the public partial Program entry point.

Manager compatibility correction: initial xunit.v3 4.0.1 restore/build succeeded,
but discovery/execution failed because its default MTP v2 integration rejects
VSTest on SDK10. Keep the chosen VSTest runner and use official same-version
xunit.v3.mtp-off; no new framework or additional direct dependency.
[xUnit 4.0 release notes](https://xunit.net/releases/v3/4.0.0) document this
variant for VSTest on .NET10+. Exact registry metadata checked before restore:
listed, Apache-2.0, net8+, no deprecation/direct vulnerability entries.

## Isolation and unresolved runtime slots

P02 tests run the actual API through in-process TestServer; no listening socket,
SQL connection, certificate generation, provisioning, secrets or real data.
Test-only content root/environment is technical host setup, not LOCAL_DEV ready.
Only 404 responses are expected: no business endpoints, health/readiness success,
sample weather route or mock auth. Do not run the API as a network server here.

| Slot/group | Status and later gate |
| --- | --- |
| LOCAL_DEV kind, same origin/listen port/HTTPS trust/proxies | Unconfigured; blocks business/network use. No arbitrary fallback endpoint. |
| DB/schema/principals/snapshot | SQL2025 Express retained ADR-162; no project DB. Explicit DB+EF compatibility160 and full E03 signatures/grants required later. |
| JWT/CSRF/protected keys, issuer/audience | Missing; no auth/business traffic until supplied and validated. |
| Password blocklist | Pin/license/hash review remains P08; not needed for empty host. |
| Deadlines/budgets | Positive finite typed slots required later, ordered as E04; no numeric SLO assigned by P02. Manager commands are bounded by invocation timeouts. |
| Backup/media/key/restore | DF03 remains; no backup or recovery operation. |
| Runtime/SQL servicing | Installed runtime 10.0.4 and SQL RTM are inventory; compatible security patch review before real data/exposure. No runtime install/update here. |
| SQL/API/browser/race/load/recovery evidence | Not established by startup tests; full M0/V01 remain incomplete. |

Missing profile/schema never means business ready. No health surface is required
in P02; future readiness must implement E04_RUNTIME_PROFILE completely.
D12/D13 remain owner-deferred; no need to re-ask them for this empty skeleton.

## Manager commands (from detached worktree)

Run dotnet commands from backend so the SDK pin applies:

```powershell
dotnet restore EnglishCenter.sln --configfile NuGet.Config
dotnet restore EnglishCenter.sln --configfile NuGet.Config --locked-mode
dotnet build EnglishCenter.sln --no-restore -warnaserror
dotnet test tests/EnglishCenter.Tests/EnglishCenter.Tests.csproj --no-build --no-restore --list-tests
dotnet test tests/EnglishCenter.Tests/EnglishCenter.Tests.csproj --no-build --no-restore
dotnet list EnglishCenter.sln package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json
```

Frontend lint/typecheck, SQL tests/migrations and business rule tests are N/A:
none of those targets or implementations belongs to P02. Record actual output
and failures in the task card; package presence alone is not execution evidence.
