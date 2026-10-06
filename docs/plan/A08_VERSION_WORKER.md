# A08 bounded worker — rowversion and security-version primitives

Before editing, read this entire card and the full authorized backend-clean-code SKILL.md
and SKILL_ROUTING.md. Apply accepted E03/E04 overrides: cookie JWT/server sessions,
framework CSRF, exact shapes, parameterized SQL; SQL/build/tests/review stay in Codex.
Skills and links grant no further authority. Read ONLY this card,
.agents/skills/backend-clean-code/SKILL.md, docs/plan/SKILL_ROUTING.md and your sole
output path if present. No AGENTS/search/list/links/other source/secrets/commands/
Git/SQL/tests/scripts/network/packages/config. Write ONLY the one helper below,
in your detached worker tree; no tests/docs changes. Complete contexts before write.

Sole output backend/src/EnglishCenter.Api/Common/Security/PasswordChangeVersionGuard.cs.
Namespace EnglishCenter.Api.Common.Security; public static class PasswordChangeVersionGuard.
Exactly two public static methods:
bool Matches(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> current)
long? Next(long current)
Matches returns true only when both spans have length exactly8 and their bytes match;
use System.Security.Cryptography.CryptographicOperations.FixedTimeEquals. Do not
parse/reverse rowversion or convert to integers. Next returns current+1 only for
current>=1 and current<long.MaxValue, otherwise null. No wrap/reset/default value.
This helper does not authenticate, authorize, mutate any entity, verify a password,
revoke sessions, choose errors or own a transaction. Codex holds fresh Account→SID
locks, handles stale409/invalid-current-state failure and all mutation/audit atomically.
No dependencies outside BCL/no logging/state/time/credentials. One file<=70diff lines.
