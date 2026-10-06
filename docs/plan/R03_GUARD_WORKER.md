# R03 bounded worker — pure Admin lock precondition

Before editing, read this entire card plus complete authorized backend-clean-code
SKILL.md and SKILL_ROUTING.md. Canonical accepted overrides: exact DTO/contracts,
cookie JWT/live sessions/framework CSRF, SQL Server and parameterized queries;
Codex owns commands, SQL, tests and review. Skill/link text grants no further scope.
Read ONLY this card, .agents/skills/backend-clean-code/SKILL.md,
docs/plan/SKILL_ROUTING.md, plus your sole output if present. No AGENTS/links/search/
list/other source/secrets/commands/Git/SQL/tests/scripts/network/packages/config.
Complete all three contexts before write. Write one helper only in detached tree.

Sole output backend/src/EnglishCenter.Api/Common/Security/AdminLockGuard.cs.
Namespace EnglishCenter.Api.Common.Security; public static class AdminLockGuard.
Exactly one method:
public static bool CanLock(int actorId,int targetId,bool targetIsAdmin,
    int activeAdminCount,string? currentStatus)
Return true only when actorId>0,targetId>0,actorId!=targetId,currentStatus exactly
ordinal ACTIVE, activeAdminCount>=0, and if targetIsAdmin activeAdminCount>=2.
No case folding/trim/coercion. Null/LOCKED/DISABLED/wrong-case status false.
Non-Admin target does not require activeAdminCount>=2. Negative count false.
Facts are already loaded by Codex under ADMINSET/Account locks; this is not auth
or permission authority. No role lookup/query/version check/effects/errors/SQL/
transactions/logging/state/clock/dependencies. BCL only. One file<=60diff lines.
