# P01 — local bootstrap profile

## Latest A06/A07 checkpoint — applied and reverified PASS

Owner-approved A06→A07 implementation completed/reviewed PASS/applied/reverified
mained40a4b, manager633671b, identical backend0d407841beeb719bbd753e828a9a5d408549a1bc;
2026-10-06;632unit/HTTP+438SQL=1070PASS/0fail0skip/build0warnings0errors.
Two bounded AGY SUCCESS/full three contexts before write/zero observed violations;
Codex implemented/reviewed/tested JWT/current-state/transaction/HTTP integration.
A06 supplied-key framework JWT validation plus fresh Account→Session SQL guards,
exact9-field API003 /me, live CSRF resolver and explicit caller-TX activity/recheck
hook. Me/CSRF/background never renew idle; restricted scope cannot enter business.
A07 API005 revokes only current SID; commit before204/secure cookie deletion;
independent A01 security audit faults cannot restore revocation or block success.
Invalid auth cookies are removed on401 to permit deliberate anonymous CSRF/login
recovery; response remains401 and does not claim revoke success.403/500 unchanged.
Default Program stays unchanged/unmounted, no real Admin/production profile/keys/
principal or full runtime readiness. SQL cores execute only rollback fixtures;
owned SQL commit paths source-reviewed with explicit HTTP fixture commit seams,
not proof of persisted runtime/committed multi-client concurrency. No A08/restM0.
Retained dbo rows except history/test users/role memberships0;4roles102permissions/
6migrations, profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged; exact catalog
regression PASS. No schema/module/package/grant/profile/owner/logins/other DB/seed/
persistent keys/install/push/deploy change. Original worker/manager detached retained.
See A06/A07_TASK_CARD/evidence, notes/A06.md,A07.md, docs/api.md; ADR171.
Next canonical A08 password change, then R01 canonical replay registry; unstarted.
ADR163 permits applying only already authorized reviewed PASS stages.

## Historical A03/A05 checkpoint

### A03/A05 applied checkpoint (historical) — applied/reverified PASS maina9abdc6

Owner-approved A03→A05 implementation completed/reviewed PASS/applied/reverified maina9abdc6
2026-10-06; 586 unit/HTTP +409 SQL=995 PASS, 0 fail/skip, build0warnings0errors.
Two bounded AGY SUCCESS/full authorized context before write/zero observed violations;
Codex integrated, corrected and tested independently. A03 supplies strict API018
signup, caller-TX Account+StudentProfile+mandatory audit, cross-role uniqueness and
201 without auto-login. A05 supplies generic login/throttle, fresh SQL account locks,
normal/restricted session issuance, supplied JWT signing and secure cookie after commit.
Explicit API adapters remain unmounted in unchanged Program. No real Admin/profile/
keys/login resolver or runtime readiness claim. SQL cores tested in rollback-only
fixtures; HTTP commit boundaries use explicit fixture executors. Owned SQL executor
commit paths source-reviewed, not executed to persist accounts/sessions. Predicate
contention and same-transaction multiple sessions do not prove committed stress.
Only EnglishCenter_D07_20261005 used; retained Account/Profile/Session/Audit0,
D12 fixture users/role members0, four roles/exact102permissions/six migrations,
profile160/Latin1_General_100_CI_AS_SC/snapshotON unchanged; exact catalog regression PASS.
No schema/migration/package/grants/owner/profile/logins/other DB/persistent keys/
push/deploy change. Worker and manager detached evidence trees retained.
See A03/A05_TASK_CARD/evidence, docs/notes/A03.md,A05.md, docs/api.md, ADR170.
Next canonical A06 current-session/me → A07 logout; not started by this batch.
ADR163 permits applying only already authorized reviewed PASS stages.

## Historical A02/A04 checkpoint

Updated: 2026-10-04. P02 SDK/package/test-host prerequisites resolved;
restore/build/discovery and ten real host tests verified in detached P02, then
reverified on main after explicit owner-authorized application (df7cd9a).
This is a nonsecret inventory and manifest, not a configured business profile.

## Latest isolated SQL/tooling checkpoint — 2026-10-05

Updated2026-10-05: owner-approved A02→A04 implementation scope complete/reviewed
PASS/applied/reverified code maina8ad1d5, managerd1612ed; identical backend tree
8a7e73f1bf9114311c9484f5c7c16d639ebd5916. A02 maindf36d98:514unit+375SQL PASS;
final545unit+375SQL=920PASS/0fail0skip/build0warnings0errors/cache-only locked restore.
Two bounded AGY SUCCESS/full3context reads before write/zero observed scope violations;
Codex independent source review, transaction/host checks and fail-closed fixes.
A02 explicit operator bootstrap: exact7roles, first Admin+projected mandatory audit on
caller SERIALIZABLE/EC:ADMINSET; repeat preserves existing Admin credentials/status.
Seven-role synthetic identities use generated RAM-only secrets and rollback. Runtime
role cannot bootstrap. No real Admin provisioned or automatic startup seed/commit.
A04 framework antiforgery API001 integration, exact Origin/header, verified-identity
boundary and safe cookie flags. No-store, no session/idle writes. Explicit origin/keys/
resolver required; private guard rejects token issuance without the boundary. Default
Program remains unmodified and API001 unexposed; test resolver/keys are fixtures only.
Only EnglishCenter_D07_20261005 used; all fixture data/schema/DENY/users/memberships
rolled back, retained0business rows/0test users/0ec-role members. Exact post-fixture
catalog/230columns/24module bodies/102permissions PASS;24tables/6migrations/4roles/
profile160/collation/snapshotON unchanged. No schema/migration/grant/profile/owner/
login/other DB/persistent keys/push/deploy change. Operator real Admin provisioning,
production profile/keys/auth resolver and full runtime readiness remain deferred.
See A02/A04_TASK_CARD/evidence, docs/notes/A02.md, A04.md and docs/api.md; ADR169.
Historical next at A02/A04 completion was A03→A05; see latest checkpoint above. ADR163 continues only for authorized reviewed PASS stages. Historical scope below.

No dependency/runtime/profile change inD10/D11. SQLServer/Design/localdotnet-ef10.0.4
remain cache-only pins fromD07; Windows/local diagnostic trusted certificate only,
not runtime TLS/principal/completeLOCAL_DEV. D07 package hash/license proof retained.

Historical P02 inventory below describes its original checkpoint.

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

## P07/P08/D01 prerequisite checkpoint2026-10-04

P08 offline source manifest is shipped under
backend/src/EnglishCenter.Api/Resources/PasswordBlocklist, exact upstream bytes
preserved with .gitattributes -text. SecLists pinned56da8d991239cc52b0da594db53457504a757670,
MIT,15310Ordinal entries; source/license hashes and quarterly dates in
[P08 card](P08_TASK_CARD.md). Program loads/validates eagerly before host build.
Built-in ASP.NET10.0.4 PasswordHasher requires no package; explicit IdentityV3
SHA512220000/128bit salt/256bit subkey,84character hash fits accepted varchar512.
Five warmed samples mean113.4ms/max117.6ms; no full load/throttle claim.

EF SQLServer provider10.0.4 now pinned/locked for D01. Current API26packages/full
graph74unique packages:64MIT/9Apache2.0/one Microsoft SqlClient SNI6.0.2 license.
Lock/assets/cache normalized content hashes agree; full SNI license retained,
dev/test rights reviewed; distribution remains a future release gate.
[Dependency manifest](D01-evidence/dependency-manifest.json), known vulnerabilities0
on2026-10-04. Mapping compatibility160/dbo/current CI_AS_SC collation chosen per E03.
No runtime SQL connection/profile/principals configured; isolated schema/tooling
now exists as recorded in the latest checkpoint above. No runtime installation.
Mainf6fd902 locked restore/build0warnings0errors/full400tests PASS. Earlier package/
startup inventory above remains the historical P02 checkpoint, not current count.

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
| Password blocklist | P08 resolved: pinned shipped corpus/license/hashes, eager fail-closed loader verified. Quarterly review remains2027-01-04. |
| Deadlines/budgets | Positive finite typed slots required later, ordered as E04; no numeric SLO assigned by P02. Manager commands are bounded by invocation timeouts. |
| Backup/media/key/restore | DF03 remains; no backup or recovery operation. |
| Runtime/SQL servicing | Installed runtime 10.0.4 and SQL RTM are inventory; compatible security patch review before real data/exposure. No runtime install/update here. |
| SQL/API/browser/race/load/recovery evidence | Not established by startup tests; full M0/V01 remain incomplete. |

Missing profile/schema never means business ready. No health surface is required
in P02; future readiness must implement E04_RUNTIME_PROFILE completely.
D12/D13 remain owner-deferred; no need to re-ask them for this empty skeleton.

## Manager commands (detached review and applied main)

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
