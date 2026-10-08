# V01-H → V01-I — concrete read-only readiness probes

2026-10-08, base main50d62a8 (G code233a274), owner assigned TWO next packages.
ADR177 task completion/context-sharing/delegation; ADR163 reviewed PASS apply.
Status PREPARED. Codex manager, AGY bounded pure policies only, originals retained.

H: implement TimeZoneReadinessProbe and OfflineBlocklistReadinessProbe, unmounted.
Timezone resolves only Asia/Ho_Chi_Minh through BCL, Windows mapping SE Asia Standard
Time allowed on Windows if IANA lookup absent. No TimeZoneInfo.Local/host fallback.
Resolved ID must match one of those IDs; base/current offset+current/future rules
must preserve UTC+7, no current/future DST/base-offset delta. Historical rules before
today may differ. Explicit TimeProvider; injected resolver only test seam.
Offline probe binds exact opaque manifest reference to explicitly supplied absolute
local corpus directory, no profile-string-as-path/default/network/secret resolution.
Fresh bounded async manifest read (4096bytes; metadata resource cap, no business/SLO
limit); strict pinned metadata/duplicate-free shapes/license/revision/counts/dates,
reviewed2026-10-04/next2027-01-04, today in [review,next). Missing/tampered/overdue
fails closed. Add cancellation-aware bounded OfflinePasswordBlocklist.LoadAsync;
actual pinned lengths73026/100206/1072bytes, hashes/strict UTF8/counts/ordinal union
reuse existing P08 validation. Existing Load/Program behavior unchanged. No plaintext
corpus/credentials/config/path/exceptions in diagnostics or worker context.

I: implement DatabaseReadinessProbe, fresh owned context per call, bind exact opaque
connection reference plus fixed current embedded schema manifest reference. Supplied
connection resolver only RAM; never read config/env/vault/default/fallback. Context
always EF SQL160, pooling false, finite connect/command cancellation, no retries.
Require encrypted local SQL Express connection with explicit database, no attach-file,
user-instance, packet/network/tunnel/config changes. Existing diagnostic certificate
trust setting only explicit fixture; no new production PKI decision/inferred ready.
Read-only principal query: runtime membership known1, non-dbo, non-sysadmin, no DB/
server CONTROL, no extra effective DB role beyond public/ec_app_runtime, no direct
effective-token grant beyond CONNECT. NULL/unknown => unsafe. Then call existing
DatabaseCompatibilityInspector requireRuntimePrincipal:true only, exact catalog/
module/profile160/snapshotON checks. Do not change inspector/manifest/grants or add
VIEW DEFINITION. Runtime-role catalog invisibility still fails closed, no repair.
Context/connection disposed finally, cancellation propagates; no audit/history/
session/default writes. Results only Pass/NotReady, no cause/config value dump.
Full active runtime principal/login/topology/key/PKI/DF01–03 remains unresolved.

Manager allowed H: Infrastructure/Readiness/TimeZoneReadinessProbe.cs,
OfflineBlocklistReadinessProbe.cs; Common/Security/OfflinePasswordBlocklist.cs
(shared pinned validation + async bounded loader); tests/Readiness/LocalDependencyProbeTests.cs.
Manager allowed I: Infrastructure/Readiness/DatabaseReadinessProbe.cs,
RuntimePrincipalRead.sql (embedded through already existing SQL resource glob: place
Infrastructure/Sql/Readiness/RuntimePrincipalRead.sql instead, no csproj edit),
tests/Readiness/DatabaseReadinessProbeTests.cs and DatabaseReadinessSqlTests.cs.
Worker H sole BlocklistManifestPolicy.cs<=100lines; I sole RuntimePrincipalPolicy.cs
<=45lines, both under Infrastructure/Readiness. No other code/default entries/DI/
frontend/package/migration/schema/grants/config/login/real Admin changes.
Docs HI card, H/I worker briefs/contracts/sanitized evidence/notes/affected memory.

Verification H: real shipped corpus/manifest and BCL timezone; synthetic missing/
tampered/oversized/duplicate/overdue/reference mismatch/current-future DST/cancel;
fresh recheck/no file mutation/no cache. Temp synthetic copies only, exact-owned temp
directory cleanup checked inside OS temp fixture root, no owner/source modification.
I non-SQL: disconnected/ref mismatch/invalid connection/NULL privilege/unsafe role/
resolved connection cancellation/unavailability; real SqlServer options160 and read-
only query/inspector no-diagnostic escape verified. No SQLite or fake SQL success.
I opt-in exact read-only local .\SQLEXPRESS/EnglishCenter_D07_20261005: actual diagnostic
identity must NOT pass runtime probe (1case); matching gate503 and repeated fresh probe
denial (2cases). Existing baseline.sql before/after: same catalog/profile/102permissions/
24empty tables/0users/members; no CREATE/ALTER/DROP/DML fixture or principal actions.
EC_V01_READINESS_SQL=1 only, other SQL/browser gates0. This is negative runtime evidence,
never proof of configured runtime login. No other DB or server auth changes.
Both stages: solution build --no-restore -warnaserror/default full tests on manager/main.
H/I counters recorded separately; unknown/negative evidence never called full V01-ready.

implementation_skills backend-clean-code; review_skills code-review (Codex).
skill_sources SHA256 backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
Complete skills/AGY contract/routing read. applicable_overrides E04 accepted fixed
UTC+7/offline pins/SQLServer160/cookie JWT/CSRF/exact DTO/noSQLite/no packages/new API.
read_context ordered memory+NEXT/HANDOFF, requirements4.10, E04_RUNTIME_PROFILE,
G contract/code/closure matrix, actual P08 loader/manifest and D13 inspector/catalog.
Worker read_context ONLY corresponding WORKER brief, complete backend skill/routing;
no links/source/owner/AGENTS/secrets/commands/search/listing/network/SQL/Git/validation.
Own output read allowed. Exact two payloads covered by current task+ADR177; no repeat
permission gate. skill_verification pending actual receipts/read-tools/diffs/checks.
Four owner files remain hash-identical/untracked/unshared/unstaged. No restM0/business
family/startup/exposure/install/push/deploy; accepted owner environment deferrals kept.
