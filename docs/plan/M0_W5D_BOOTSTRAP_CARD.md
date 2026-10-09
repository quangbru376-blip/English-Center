# M0 W5D — explicit operational Admin bootstrap
2026-10-09 assigned full M0 ADR177; reviewed PASS apply ADR163; LOCAL_DEV ADR186.
W5C actual readonly all9 host PASS; this separate scope authorizes operational
business seed in owned EnglishCenter_M0_LOCALDEV only. No D07/other DB/schema/
migration/grants/new OS users/key rotation/packages/install/M1–M4/push/deploy.

Exact code: Infrastructure/Operator/LocalAdminBootstrap.cs; Program.cs single
--m0-bootstrap branch; tools/operator/Initialize-M0Admin.ps1; tests/Readiness/
LocalAdminBootstrapTests.cs. Reuse existing BootstrapService/SqlBootstrapRepository
unchanged, caller SERIALIZABLE/EC:ADMINSET, mandatory initial-Admin audit in same TX.
Dedicated genuine nonadmin EcM0Schema only; strict integrated local TLS, exact
catalog name/database_id/database_guid from existing protected operator descriptor,
M0SchemaUser/db_owner/no sysadmin/CONTROL SERVER, existing6 migration identity set.
Finite30s operation, command10s/lock3000ms, no execution retry/automatic repair.

Initial operational reference data: exact7 canonical roles plus one first Admin,
email admin@englishcenter.local/name LOCAL_DEV Administrator; no staff/student/test
accounts or fixture promotion. Random32byte application password only RAM; parent
current provisioning operator protects PSCredential with current-user DPAPI CLIXML
in existing C:\ProgramData\EnglishCenter\M0\admin.credential.clixml BEFORE SQL
COMMIT. Exact owner/SYSTEM/Admin3 protected ACL, no reparse. Separate admin.json
owner/catalog identity/phase escrow_prepared then committed, no plaintext/hash.
Escrow retained on unknown result; never generate/reset retry. Explicit -VerifyExisting
reads same escrow, performs identity/migration/Admin credential verification readonly,
no Create/COMMIT/credential reset. Fresh mode refuses existing escrow/marker/Admin.

Parent owns schema Windows credential DPAPI file already provisioned; launches
actual child with ProcessStartInfo.Password SecureString/LoadUserProfile. Application
secret + public catalog identity only RAM redirected stdin JSON <=4096chars/strict
shape; no args/env/clipboard/URL/file/source/log/worker sharing of plaintext. Output
fixed booleans/phase only, exceptions discarded; per-run receipt ignored/nonsecret.
No runtime HTTP/startup bootstrap path. Existing Admin of any status is preserved;
wrong/missing identity/schema/secret aborts before write; unknown commit no replay.

Codex-only privileged composition; no additional external AGY context or keys.
Implementation backend-clean-code, review code-review complete files read; routing
hash ADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70.
Canonical SQLServer/current-user DPAPI/raw password/JWT cookie/DTO overrides.
Verification: wrong operator/cancel/strict bounded secret parser tests; no secret
assertion output. Build/default tests; PowerShell AST/source review; actual schema
child initial COMMIT and subsequent readonly escrow/hash verification. Fresh SQL
checks exact7roles/oneAdmin/one mandatory bootstrap audit/no profiles/sessions;
no auth/browser requests yet. Full M0 still needs browser/actual scheduled jobs and
operator launch/walkthrough; derive those scopes separately before further writes.
Source review PASS before operational execution: genuine schema gate/catalog identity/
strict TLS/migrations, no startup bootstrap; existing A02 owns roles/Admin/audit TX.
8new parser/identity/cancellation cases PASS;1271defaultPASS/78intentionalSKIP,
build0warnings0errors. PowerShell AST0errors; explicit protected escrow Flush(true)
and DPAPI roundtrip before stdin/COMMIT, bounded child45s/kill wait5s. No secret
literal/source/log/external context or SQL concatenation. Operational proof pending.
First operational attempt refused before escrow/child/SQL: W1 UAC-created vault
files owner Administrators, protected root owner provisioning operator. Verified
all3 ACL entries unchanged. Derived custody compatibility accepts only existing
trusted file-owner operator/SYSTEM/Administrators; root still exact operator and
protected3rules. No ACL change/widening, new principals or COMMIT occurred. Source
review PASS for this compatibility fix, AST0errors; initial mode may run again
because both Admin files absent and child was never launched, no unknown COMMIT.
Second attempt created durable escrow but result refused; readonly VerifyExisting
also refused. Independent readonly owned-catalog counts Role/Account/Audit/Profile/
Session all0, so no retained bootstrap effects. Keep escrow; no fresh retry/reset.
Derived safe helper diagnostic numeric failureLine/childExited/exitCode only, never
raw exception/stdout/stderr/secret. Next readonly verification only until cause fixed.
Derived fixed code-stage checkpoint callback before operations (identity/input/
connection/catalog/migrations/credential/freshness/bootstrap/commit); only literal
allowlist forwarded by CLI/helper, no exception/SQL/secret diagnostics. This enables
readonly verification to locate refusal while no write retry is authorized by a
refusal alone. Existing parser/identity tests retained, source review checks literals.
Actual readonly schema facts id/GUID/name/user/db_owner/sysadmin/CONTROL and original
login all matched; actual SQL_VARIANT_PROPERTY(DB_ID(),'BaseType') reports smallint
on this SQL17 instance despite primary DB_ID docs listing int. Direct GetInt32
projection refused. Fix SELECT CONVERT(int,DB_ID()) preserves strict identity checks;
no permissions broadened. Derived explicit -RecoverPrepared reuses only existing
owned escrow_prepared credential/marker/same DBid+GUID; conflicting VerifyExisting
refuses. No fresh random password/overwrites/reset; child still requires zero Role/
Account/Audit/Profile/Session under SERIALIZABLE before A02. This exact recovery is
assigned operational completion ADR177, manually dispatched after reviewed fix and
zero-count evidence; never automatic retry. Existing wrong/occupied Admin refused.