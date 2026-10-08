# V01-I worker — pure principal facts policy
Read ONLY this complete brief, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md in order before writing. Apply canonical overrides.
No source/AGENTS/links/owner/secrets/listing/search/commands/network/SQL/Git/tests.
Own sole output read allowed. Codex owns SQL/adapters/integration/review/tests.
Create ONLY backend/src/EnglishCenter.Api/Infrastructure/Readiness/RuntimePrincipalPolicy.cs
<=45lines, namespace EnglishCenter.Api.Infrastructure.Readiness.
public static class RuntimePrincipalPolicy; public static bool IsSafe(bool? isRuntime,
bool? isDbo,bool? isSysadmin,bool? controlsDatabase,bool? controlsServer,
bool? hasExtraDatabaseRole,bool? hasDirectEffectiveGrant).
True only isRuntime==true AND all other six==false; NULL/unknown always false.
No casts/coalescing NULL into false, defaults, overloads, IO/SQL/logging/commands.
Only BCL, no data/context/string principal names or secret values.
