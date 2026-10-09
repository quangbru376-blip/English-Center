# M0 W1 — owned Windows identities and operator custody

2026-10-09. Whole-M0/ADR177 and selected LOCAL_DEV ADR186. UAC helper successfully
verified administratorToken=true; ordinary Codex/tool process remains unelevated.
Use bounded per-operation elevated helpers, each rechecks actual Windows authority.
Codex manager only: credential-bearing operational work never goes to AGY.

Exact script scope tools/operator/Initialize-M0Identities.ps1, no backend/Program
changes. Create only EcM0Runtime/EcM0Maint/EcM0Schema local users, fail if any exists.
No Administrator membership; runtime/maintenance/schema remain separate. Random
RAM passwords, owner-current-user DPAPI PSCredential exports only under exact new
C:\ProgramData\EnglishCenter\M0, fail-if-existing root, no reparse ancestors.
Root ACL protected: provisioning owner/SYSTEM/Administrators FullControl only;
no Everyone/Users inherited secret access. Record owned SID/GUID/nonsecret status.
No account secrets in Git/chat/evidence/logs or command arguments. On failure remove
only just-created users whose SID still matches; retain protected failure receipt.
No recursive deletion, reused user mutation, SQL/logins/role grants/schema/TLS/
certificate/registry/service changes in W1. Later phases need exact derived scopes.

implementation_skills backend-clean-code; review_skills code-review. Complete reads;
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs/requirements/operator closure/profile/actual
Windows preflight. applicable_overrides accepted cookie/SQL identity/secret custody;
no SQLite/new packages/defaults. skill_verification pending AST/script/custody review.

Before run: parse PowerShell AST, independently review exact paths/ACL/DPAPI/failure
compensation, manager commit reviewed script then ADR163 apply main. Execute via UAC
only after exact source hash checked. Verify actual owner/SID/ACL/readable DPAPI
credentials without exposing material, no Administrator group membership. D07 remains
untouched and owner4files hash-identical/unshared/untracked. No readiness claim from
provisioning users; separate authenticated SQL/TLS and operational host still needed.

Pre-execution script review PASS2026-10-09: AST0errors, exact three users/root,
DPAPI credential exports, root/file allowlist ACLs, matching-SID compensation,
CreateNew receipt prevents overwrite. Fixed uninitialized receipt/invalid-path write
and guarded credential disposal/per-user cleanup. SHA256 script
75B52281C37A077C138F9B2695E9B483796AB2B2CF5BBDED7328F36FD830CC18.
Operational acceptance remains pending actual provisioning; source-only PASS is
not W1 completion. No backend/frontend build needed for operator-only PowerShell.

Actual W1 PASS2026-10-09: UAC administratorToken true; createdUsers3,
compensated0, noAdministratorMembership true, credentialsProtected true.
Independent post-run matchingSids3/dpapiReadable3/rootAclProtected true.
Manager b5b40cb/main bbff42b; Git blob identical, working-byte SHA differs only
checkout CRLF: main7601C77C6F88603BBB35DE13CDF16325BDE94353493E630309F04FA598D98FD9.
D07 readonly baseline output SHA25610CD3407CE5986394703A9BEC2EE3E42314080809DDABE26906B1F14B593FC8E
unchanged; owner4 hashes/untracked/unstaged unchanged. Real SQL authentication,
TLS/custody/runtime integration still pending; full M0 incomplete.
