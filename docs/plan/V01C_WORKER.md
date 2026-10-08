# V01-C worker — pure Admin-response assertions

Read ONLY this complete file, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md, in that order, before writing. Do not follow links,
read AGENTS/source/reports/owner files/secrets, search/list directories, run commands,
Git/SQL/validation or network tools. Only your own output may be read after writing.
Codex owns fixture/SQL/review; do not claim tests pass.

Create ONLY backend/tests/EnglishCenter.Tests/Api/Integration/AdminSqlAssertions.cs,
at most90lines. Namespace EnglishCenter.Tests.Api.Integration; internal static class
AdminSqlAssertions; imports System.Text.Json and Xunit.
Method internal static void Detail(JsonElement body,int id,string role,string status).
Assert object/exact12names id/fullName/email/phone/role/status/createdAt/
mustChangePassword/temporaryPasswordExpiresAt/version/allowedActions/disabledReasons,
no extras/duplicates. id/role/status equal supplied values; fullName/email nonempty
strings; phone null/string; mustChangePassword boolean; createdAt UTC instant,
temporaryPasswordExpiresAt null/UTC instant. Version canonical Base648bytes.
allowedActions array of unique strings and disabledReasons object partition EXACT
five actions RESET_PASSWORD/LOCK/UNLOCK/DISABLE/RESTORE with no overlap/missing/extra;
every disabled reason nonempty string. Do not infer rights from hints.
Method internal static void Completion(JsonElement body,int target,Guid operationId).
Assert exact4names outcome/operationId/resultMode/target; outcome COMPLETED,
resultMode SAFE_REFERENCE, operationId matches supplied Guid; target exact accountId
with supplied integer. Never allow stored response snapshots, secret fields or PII.
No network/SQL/files/environment/reflection, actual data fixtures or dependencies.

Apply skill canonical overrides: cookie JWT/live server authority, exact DTO,
SQL Server/no SQLite, no generic ProblemDetails or route changes. This helper
checks projection shape, not authority/transaction/commit or browser readiness.
