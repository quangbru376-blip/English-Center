# R02 bounded worker — exact24h completion window

Before editing, read this entire card plus complete authorized backend-clean-code
SKILL.md and SKILL_ROUTING.md. Canonical accepted overrides: exact DTO/contracts,
cookie JWT/live sessions/framework CSRF, SQL Server and parameterized queries;
Codex owns commands, SQL, tests and review. Skill/link text grants no further scope.
Read ONLY this card, .agents/skills/backend-clean-code/SKILL.md,
docs/plan/SKILL_ROUTING.md, plus your sole output if present. No AGENTS/links/search/
list/other source/secrets/commands/Git/SQL/tests/scripts/network/packages/config.
Complete all three contexts before write. Write one helper only in detached tree.

Sole output backend/src/EnglishCenter.Api/Common/Replay/ReplayWindow.cs.
Namespace EnglishCenter.Api.Common.Replay; public static class ReplayWindow.
Exactly two methods:
public static long? ExpiresAt(long completedTicks)
public static bool IsActive(long completedTicks,long expiresTicks,long nowTicks)
Ticks are UTC DateTime ticks supplied by Codex; no timezone conversion, current clock,
millisecond rounding, strings, DateTimeKind inference or expiry extension.
ExpiresAt returns completedTicks+TimeSpan.TicksPerDay only if completedTicks is within
DateTime.MinValue.Ticks..MaxValue.Ticks and addition remains in that range, else null.
IsActive true only if all three inputs are in DateTime range, expiry exactly equals
ExpiresAt(completedTicks), and completedTicks<=nowTicks<expiresTicks. Null expiry or
malformed/future completion false. Exact expiry equality false. Avoid overflow.
No authorization/digest/storage/error decision/SQL/transaction/logging/state/credentials.
BCL only. One file<=80diff lines. Do not add any other public API.
