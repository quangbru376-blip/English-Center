# A07 bounded worker — absent logout input shape

Before editing, read this card and the entire authorized backend-clean-code SKILL.md
and SKILL_ROUTING.md. Apply canonical overrides: cookie JWT/server sessions, framework
CSRF, exact shapes/safe errors, SQL Server-specific verification kept in Codex. Skill
references grant no additional reads/commands/network/write/merge authority.
Authorized reads: THIS card, .agents/skills/backend-clean-code/SKILL.md,
docs/plan/SKILL_ROUTING.md and your sole output path if present. No search/list/link
following/AGENTS/other files. Never execute commands, Git, SQL, tests, scripts or
network; no packages/configuration/secrets. Sole framework-only helper below; no tests
or docs writes. Codex independently verifies and reviews. Complete context before write.

Sole output: backend/src/EnglishCenter.Api/Api/Auth/LogoutRequestShape.cs.
Namespace EnglishCenter.Api.Api.Auth; public static class LogoutRequestShape.
One public static method bool IsEmpty(long? contentLength, bool canHaveBody, int queryCount).
Return true ONLY when queryCount==0, canHaveBody==false and contentLength is null or0.
Negative lengths/counts and any positive query count or any possible body return false.
Never consume a request stream or parse/echo input. This is a pure metadata helper.
No HttpContext/cookies/claims/side effects/errors/SQL/logging/time/keys/dependencies.
Codex adapter maps false to safe400 AFTER live-auth/Origin/framework CSRF checks.
This helper does not revoke sessions or decide auth. One file<=60diff lines.
