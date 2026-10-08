# V01-B worker — pure auth-response assertions

Read ONLY this complete file, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md, in that order, before writing. Do not follow links,
read AGENTS/source/reports/owner files/secrets, search/list directories, run commands,
Git/SQL/validation or network tools. Only your own output may be read after writing.
Codex owns fixture/SQL/review; do not claim tests pass.

Create ONLY backend/tests/EnglishCenter.Tests/Api/Integration/IdentitySqlAssertions.cs,
at most80lines. Namespace EnglishCenter.Tests.Api.Integration; internal static class
IdentitySqlAssertions; imports System.Text.Json, Xunit, System.Globalization if needed.
Method internal static void Context(JsonElement body,int id,string role,string scope).
Assert object, exact9names accountId/fullName/email/role/scope/accountVersion/serverTime/
absoluteExpiresAt/effectiveExpiresAt with no extras/duplicates; id/role/scope equal
supplied expected values. fullName/email nonempty string; accountVersion canonical
standard Base64 exactly8decoded bytes. Three instants are UTC strings ending Z,
format yyyy-MM-dd'T'HH:mm:ss.fff'Z'; effectiveExpiresAt<=absoluteExpiresAt and
serverTime<effectiveExpiresAt. Do not assume normal/restricted numeric lifetime
or browser wall time. Throw normal assertion failure on malformed response.
No actual tokens/passwords/cookies/PII fixtures, DTO changes or serializer settings.
No swallowed failures, any network/SQL/files/environment/reflection or dependencies.

Apply selected skill with canonical overrides: cookie JWT/framework CSRF, exact DTO,
real SQL Server verification owned by Codex, no SQLite/generic ProblemDetails claims.
This helper checks response contract, not server authority/commit/browser readiness.
