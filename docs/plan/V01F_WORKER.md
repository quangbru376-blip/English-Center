# V01-F worker — persisted lock projection only
Read ONLY this whole file, .agents/skills/backend-clean-code/SKILL.md and
 docs/plan/SKILL_ROUTING.md in order before writing. Apply canonical cookie/SQL
 overrides. No links/source/AGENTS/owner/secrets/listing/search/command/network/SQL/
 Git/validation. You may read your own sole output. Codex owns all privileged work.
Create ONLY backend/tests/EnglishCenter.Tests/Api/Integration/OwnedBrowserAssertions.cs
<=85lines. Namespace EnglishCenter.Tests.Api.Integration; internalstatic class
OwnedBrowserAssertions; pure publicstatic void Lock(AuditLog audit,ReplayOperation
marker,int actor,int target,int revoked,DateTime at). Types in
EnglishCenter.Api.Domain.Administration. Use xUnit and System.Text.Json only.
AuditLog public props: AuditId(int),ActorAccountId(int?),Action/EntityType/Result/
AuditCategory(string),EntityId/Details/IpAddress(string?),OperationId(Guid),
OccurredAt(DateTime),SecurityExpiresAt/IpExpiresAt(DateTime?). ReplayOperation:
ReplayId/ActorAccountId(int),ActionCode/ResultMode/ResultReference(string),
RequestId/OperationId(Guid),InputDigest(byte[]),CompletedAt/ExpiresAt(DateTime).
Assert positive IDs/nonempty UUIDs/sameOperationId/exactactor; both actions
ADMIN.ACCOUNT.LOCK; auditEntityType Account/EntityId target invariant decimal/
SUCCESS/MANDATORY/OccurredAt at/nullsecurity-IPexpiry/IP. MarkerInputDigest32bytes,
CompletedAtat/ExpiresAtat+24h. Do not assume ResultMode; reference JSON strictroot
exact outcome,target; outcome COMPLETED; target exactaccountId=inttarget.
AuditDetails strictroot exactlyold,new; old exactstatus ACTIVE/securityVersion1;
new exactstatus LOCKED/securityVersion2/revokedSessionCount revoked.
Fixed messages only; never print Details/ResultReference or accept credentials.
Strict JSON object/type/property count checks; use TryGetInt32/TryGetInt64.
Catch malformed JSON into fixed assertion failure; Assert.Fail notTrue(false).
No DbContext/DB/fixture/realexecutor/commands/IO/runtime configuration.
