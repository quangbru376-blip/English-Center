# V01-S — explicit read-only catalog grant allowlist

2026-10-08. Full assigned M0/ADR177/163; Codex only, no external sharing.
R established880/880 unchanged facts with temporary database VIEW DEFINITION and
column SELECT on __EFMigrationsHistory(MigrationId,ProductVersion). Explicit VIEW
SECURITY DEFINITION was redundant and is not part of this candidate.

Exact code path backend/src/EnglishCenter.Api/Infrastructure/Sql/Readiness/
RuntimePrincipalRead.sql only. Update the direct effective-grant exclusion:
permit exact GRANT(state G, no W/D) database CONNECT or VIEW DEFINITION(class0),
and object(class1) SELECT on only the two explicit history column IDs. Unknown
object/column IDs cannot match. No broad table SELECT, extra business DML, schema,
server, ownership, role, CONTROL, impersonation or delegation privileges allowed.
Existing7nullable safety facts/Express8th column/mandatory runtime inspector remain
unchanged. Missing permissions still fails exact catalog; descriptor is not proof.
This is explicit reviewed policy implementation, not hidden permanent grant repair.

SQL verification .\\SQLEXPRESS/EnglishCenter_D07_20261005 only; before/after baseline.
Three independent caller-owned rollback transactions, fail-if-exists user
V01S_catalog_fixture_0 WITHOUT LOGIN plus ec_app_runtime membership/candidate reads.
Case approved metadata: direct-grant exclusion0; add redundant direct SELECT on
dbo.Account: exclusion1; replace VIEW DEFINITION with WITH GRANT OPTION: exclusion1.
EXECUTE AS USER only to execute exact updated8column fixed principal SELECT; REVERT
then ROLLBACK success/error. No COMMIT/business DML/server login/OS/other DB changes.
Not positive runtime-login proof; actual Windows authentication/certificate/store
and remaining nullable facts still must be verified. Temp user/grants/role membership
must be absent after each case; role definitions/exact102grants/catalog unchanged.
Ignored outputs test-results/S-{approved,business,delegation}.sql/.txt,S-before.txt,
S-after.txt. Record only case outcomes/fact count/baseline hashes, no identities.

Skills backend-clean-code/code-review/routing complete read; hashes from V01Q.
read_context R evidence/requirements/E04/I guard/expected catalog/baseline.
applicable_overrides SQL Server actual/nullable-fail-closed/no owner fallback/no
package/newAPI/config defaults. skill_verification complete; see final actual SQL/full-test review below.
No Program/DI/frontend/inspector/schema/expected manifest/permanent grants changed.
Docs this card, Q/R/S evidence/notes, decisions/current memory/handoffs/closure.
Manager/main full build --no-restore -warnaserror/tests; existing opt-in read-only
DatabaseReadinessSql tests diagnostic denial rerun; owner4hashes preserved.

## Independent component review PASS/applied/reverified
S maina6b4828/manager41b166d, exact1SQL file12insertions/1deletion.
Three SQL cases8facts each: metadata approved(exclusion0), direct Account SELECT
denied(exclusion1), metadata WITH GRANT OPTION denied(exclusion1). Each transaction
rolled back with fixture-user absence; D07 before/after byte-identical digest
10CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E.
Manager/main1197PASS/58intentional opt-in SKIP/build0warnings0errors, plus3actual
readonly diagnostic denial tests PASS each. No permanent grant or positive runtime
authority claimed; strict7facts/Express/mandatory inspector preserved. Source/tests
trees b4b552ec5c9638fead0ab5d81166f6bad30a2cda /
ab39300509baafcef4f4d4a1dd3747931e2704e0 identical on both checkouts.
skill_verification complete: actual SQL semantics, whitelist state/object/column/
unknown-ID behavior, authority source/scope and full regression reviewed. Notes/
V01RS/QRS evidence/ADR184 updated. No business route/DTO/schema change (N/A).
