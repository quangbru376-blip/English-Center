# V01-E worker — persisted audit projection assertions

Read ONLY this complete file, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md, in order before writing. No links/source/AGENTS/owner
reads, listing/search/commands/Git/network/SQL/validation. Own output read allowed.
Create ONLY backend/tests/EnglishCenter.Tests/Api/Integration/OwnedAuditAssertions.cs
<=85lines. Namespace EnglishCenter.Tests.Api.Integration; internal static class
OwnedAuditAssertions. Use Xunit, System.Text.Json, and
EnglishCenter.Api.Domain.Administration.AuditLog; no fixtures or DB operations.

AuditLog has public AuditId:int, ActorAccountId:int?, Action/EntityType/Result/
AuditCategory:string, EntityId/Details/IpAddress:string?, OperationId:Guid,
OccurredAt:DateTime, SecurityExpiresAt/IpExpiresAt:DateTime?.
Implement Security(AuditLog row,string action,int? actor,bool success,DateTime at,
string? ip=null). Require positive AuditId, nonempty OperationId, exact actor,
action, EntityType='Account', EntityId actor invariant decimal string or null,
Result SUCCESS/FAILURE, Category SECURITY, Details null, OccurredAt=at,
SecurityExpiresAt=at+90days, IpAddress=ip, IpExpiresAt=null or at+90days as ip.

Implement PasswordChange(AuditLog row,int actor,long previous,int revoked,bool temp).
Require positive AuditId/nonempty OperationId, actor/EntityId exact, Action
AUTH.PASSWORD_CHANGE/Account/SUCCESS/MANDATORY, null security/IP expiries/IP.
Parse Details JSON: root exactly old/new. old exactly mustChangePassword,
securityVersion equals temp/previous; new exactly mustChangePassword=false,
securityVersion=previous+1, revokedSessionCount=revoked. Strict fields/types;
no password/hash/SID/token/credential fields allowed. Do not print Details or
accept secrets as arguments. Use fixed secret-free assertions/messages.

Apply backend skill with routing accepted overrides: cookie JWT, strict exact
projection and SQLServer-specific evidence, no SQLite/bearer/packages. This helper
does not claim commit/rollback/security readiness; Codex runs actual verification.
