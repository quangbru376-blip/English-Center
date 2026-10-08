# V01-P worker — pure maintenance principal guard
Read ONLY this complete brief, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md in order before edit. Apply accepted overrides.
No source/links/AGENTS/owner/secrets/search/listing/commands/network/SQL/Git/tests.
Own sole output read allowed. Create ONLY
backend/src/EnglishCenter.Api/Infrastructure/Readiness/MaintenancePrincipalPolicy.cs
<=50lines, namespace EnglishCenter.Api.Infrastructure.Readiness; BCL only.
public static class MaintenancePrincipalPolicy; public static bool IsSafe(
bool? isDbo,bool? isSysadmin,bool? controlsDatabase,bool? controlsServer,
bool? isRuntime,bool? isDdlAdmin,bool? canDeleteAudit,bool? canDeleteReplay,
bool? canClearIp).
Pure/no IO/no logging. Return true ONLY first six exclusions explicitly false
and final three capabilities explicitly true. Every null/unknown denies.
No caching/defaults/extra APIs/files/commands. Do not inspect or modify SQL.
