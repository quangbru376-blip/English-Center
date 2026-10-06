# A06 bounded worker — pure effective deadline calculation

Before editing, read this card and the entire authorized backend-clean-code SKILL.md
and SKILL_ROUTING.md. Apply canonical overrides: cookie JWT/server sessions, framework
CSRF, exact shapes/safe errors, SQL Server-specific verification kept in Codex. Skill
references grant no additional reads/commands/network/write/merge authority.
Authorized reads: THIS card, .agents/skills/backend-clean-code/SKILL.md,
docs/plan/SKILL_ROUTING.md and your sole output path if present. No search/list/link
following/AGENTS/other files. Never execute commands, Git, SQL, tests, scripts or
network; no packages/configuration/secrets. Sole framework-only helper below; no tests
or docs writes. Codex independently verifies and reviews. Complete context before write.

Sole output: backend/src/EnglishCenter.Api/Common/Security/SessionDeadlineMath.cs.
Namespace EnglishCenter.Api.Common.Security; public static class SessionDeadlineMath.
Exactly these public static methods, returning nullable DateTimeOffset:
Normal(DateTimeOffset issuedAt, DateTimeOffset expiresAt, DateTimeOffset lastActivityAt,
       DateTimeOffset jwtExpiresAt)
Restricted(DateTimeOffset issuedAt, DateTimeOffset expiresAt, DateTimeOffset jwtExpiresAt,
           DateTimeOffset temporaryExpiresAt)

Return all results with UTC offset zero; preserve ticks, NEVER truncate/round/extend.
Normal returns min(expiresAt, issuedAt+2hours, lastActivityAt+30minutes,jwtExpiresAt).
Restricted returns min(expiresAt, issuedAt+10minutes,temporaryExpiresAt,jwtExpiresAt),
with no idle calculation. Reject nonsensical intervals by returning null: expiresAt
or jwtExpiresAt<=issuedAt; Normal lastActivityAt<issuedAt or >expiresAt; Restricted
temporaryExpiresAt<=issuedAt. Overflow from AddHours/AddMinutes returns null; catch
only ArgumentOutOfRangeException for interval calculations, no catch-all.
Normalize offsets before comparing; output no mutable state/default clock.
This helper DOES NOT authenticate/authorize/revoke/check current time or read DB/token.
Codex checks Account/status/security/scope/revocation and uses raw clock now<deadline.
No entities/keys/JWT/PII/logging/dependencies. Private minimum helper allowed.
One file<=140diff lines, conventional easily explained C#; no unrelated additions.
