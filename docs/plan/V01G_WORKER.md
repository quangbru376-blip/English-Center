# V01-G worker — pure deadline gates
Read ONLY this complete file, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md in order before writing. Apply canonical overrides.
No source/AGENTS/links/owner/secrets/listing/search/commands/network/SQL/Git/tests.
You may read your own sole output. Codex owns integration and verification.
Create ONLY backend/src/EnglishCenter.Api/Infrastructure/Readiness/DeadlineBudgets.cs
<=65 lines, namespace EnglishCenter.Api.Infrastructure.Readiness.
public static class DeadlineBudgets, two pure public static bool methods:
IsValid(int lockMs,int sqlSeconds,int requestMs,int csvMs,int readinessMs)
and CanAcquireLock(int lockMs,int remainingSqlMs,int remainingRequestMs).
IsValid: all five positive; convert SQL seconds to milliseconds without overflow;
reject SQL milliseconds greater than int.MaxValue; require lockMs < SQL ms,
SQL ms < requestMs, lockMs < requestMs, csvMs <= requestMs, readinessMs <= requestMs.
No invented defaults, operator SLOs, clamp, TimeSpan infinity or configuration.
CanAcquireLock: all three positive, lockMs strictly below BOTH remaining budgets.
No IO/async/exception/logging/configuration/packages or other types/files.
Use long arithmetic before multiplication, never unchecked int multiplication.
