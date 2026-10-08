# V01-J worker — pure approved runtime-patch policy
Read ONLY this complete brief, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md in order before writing. Apply accepted overrides.
No source/AGENTS/links/owner/secrets/search/listing/commands/network/SQL/Git/tests.
Own sole output read allowed. Codex implements IO, keys, probes and tests.
Create ONLY backend/src/EnglishCenter.Api/Infrastructure/Readiness/RuntimePatchPolicy.cs
<=100lines, namespace EnglishCenter.Api.Infrastructure.Readiness. BCL only.
Create sealed records RuntimeArtifactPin(string RelativePath,long Length,string Sha256)
and RuntimePatchManifest(System.Version DotNet,System.Version AspNet,System.Version Ef,
System.Version Sql,System.DateOnly Reviewed,System.DateOnly NextReview,
System.Collections.Immutable.ImmutableArray<RuntimeArtifactPin> Artifacts).
Both record ToString overrides return only their type name, never values.
public static class RuntimePatchPolicy; public static bool IsCurrent(
RuntimePatchManifest? manifest,System.DateOnly today). Pure/no IO/no logging.
Reject null versions/manifest/default arrays/null pins/empty values; versions have
Major10/10/10/17 respectively, Minor>=0, Build>=0 (explicit patch required).
Reviewed<=today<NextReview; NextReview strictly after Reviewed. No hardcoded dates,
no assumed approved patch version or automatic latest lookup.
Artifacts exactly five, unique ordinal exact RelativePath names:
backend/global.json
backend/src/EnglishCenter.Api/EnglishCenter.Api.csproj
backend/src/EnglishCenter.Api/packages.lock.json
frontend/package.json
frontend/package-lock.json
Each Length>0, SHA256 exactly64 ASCII hexadecimal characters (either case).
Reject duplicates/missing/extra paths. Return false on ordinary invalid values.
No global/static mutable cache. Do not create another file or run commands.
