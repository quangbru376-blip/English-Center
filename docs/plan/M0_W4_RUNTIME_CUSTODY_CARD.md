# M0 W4 — runtime-owned HTTPS/JWT/CSRF custody

2026-10-09 whole M0 ADR177/LOCAL_DEV ADR186/reviewed apply ADR163. Codex only:
no secret/privileged workflow external sharing. W1–W3 actual SQL identities/TLS PASS.

Exact new C:\ProgramData\EnglishCenter\M0Runtime, fail existing. Privileged
Prepare-M0RuntimeCustody.ps1 verifies W1/W2 owner and actual EcM0Runtime SID/non-admin,
creates fresh root owner runtime SID, protected ACL only runtime/SYSTEM/Admin Full.
No operator-unelevated read grant, root listing/reparse/sibling custody access.
Prepared marker contains W1 ownerId/runtimeSid only; no credentials copied to root.

Initialize-M0RuntimeKeys.ps1 executes as genuine EcM0Runtime, current-user identity
matches marker, fresh root no existing jwt/key-manifest/public leaf/CSRF. Generate
32-byte HS256 key in RAM, current-user DPAPI protect with fixed application entropy,
write only encrypted jwt.dpapi. Validity now-1min to now+90days and named jwt-v1
metadata (not secret); refresh explicit operator work, no startup fallback/generation.
Create one non-exportable CurrentUser\My HTTPS localhost certificate with SAN,
RSA2048/SHA256/server-auth EKU/one-year validity. Export PUBLIC certificate bytes
only to https.cer, record thumbprint/leafSHA256/reference/validity in key-manifest.json.
Never private PFX/export/password/secret output or command argument. Failure retains
owned record/artifacts, no arbitrary cert/root deletion or automatic retry.

Exact backend/src/EnglishCenter.Api/Infrastructure/Operator/RuntimeCsrfProvisioner.cs
and explicit Program.cs --m0-provision-csrf branch only, exact single argument,
Windows current EcM0Runtime SID/marker/fresh empty csrf directory before generation.
One explicit IKeyManager.CreateNewKey active now-1min to now+90days, Persist filesystem,
ProtectKeysWithDpapi current-user, DisableAutomaticKeyGeneration. No listener/API/
SQL on operator branch; no-arg Program behavior stays unmounted. Future host keeps
automatic generation disabled and readiness never generates/repairs missing keys.

Trust-M0HttpsCertificate.ps1 later UAC only verifies ownership/public leaf hash/name,
imports solely new leaf public bytes to LocalMachine\Root; no private-key export,
other stores/certs/trust/network or SQL settings. Root trust undo exact owned thumb
only on failure; retain nonsecret receipt. Schema/maintenance cannot decrypt runtime
JWT/CSRF or use private HTTPS key. Store/probe tests verify runtime restart decryption,
signer→validator, persisted key/certificate usage and negative identity/custody guards.
No production runtime mounting from key generation; all9/host/UI/jobs next W5.

implementation_skills backend-clean-code; review_skills code-review.
skill_sources backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
read_context ordered memory/handoffs/requirements/E04runtime/W1–W3 receipts,
current selected profile, actual key/protected CSRF probes and pinned DPAPI dependency.
applicable_overrides accepted JWT-cookie/DPAPI/realSQL/no packages/default keys.
skill_verification pending exact AST/source review/build/tests/real custody checks.
No M1–M4/install/push/deploy/D07 business writes/owner4 changes or sharing.

Root helper pre-execution review PASS: AST0, fixed new path/no reparse ancestor,
W1/current owner/runtime SID/non-admin checks, protected owner runtime ACL allowlist,
no credential copying/key generation. Actual root acceptance pending.

Actual root PASS2026-10-09: admin/runtimeOwned/ACLprotected true; runtime/SYSTEM/
Administrators only. Key script source review PASS AST0: genuine runtime/SID/ACL/
fresh fixed files, CreateNew artifacts, DPAPI32-byte exact RAM round trip/zeroing,
no private export, CNG ExportPolicyNone verified, sole own certificate/public export,
owned-manifest-only failure updates. Actual keys acceptance pending run.

Actual runtime native key helper PASS: genuine identity/JWT DPAPI roundtrip/HTTPS
private key true, privateExportPerformed false. Subsequent exact Program operator
branch + RuntimeCsrfProvisioner.cs implemented, fixed root/genuine runtime marker/
owned key manifest/fresh directory checks, one framework current-user DPAPI key,
disable auto-generation, no listener/SQL. Added one refusal test under manager
identity; actual runtime creation/probe/restart still pending. No default host mount.

HTTPS leaf trust helper source PASS AST0: actual W1/operator/owned public leaf SHA,
CN/SAN/validity/serverEKU/noPrivateKey guards, exact new leaf MachineRoot add only.
No private store/key changes, no SQL/service mutation. Actual trust pending UAC.

CSRF operator branch source review PASS/buildclean/default1218PASS67SKIP.
No-arg host regression remains unchanged; actual runtime encrypted-store acceptance
not yet done. Trust helper compensates only just-added owned leaf on exception.

Actual CSRF operator success true after process exit; no listener/API/SQL.
Derived exact ActualM0RuntimeCustodyTests.cs4readonly opt-in checks, same genuine
Windows gates: persisted DPAPI JWT sign/validate/fresh resolution with RAM zeroing,
existing CSRF decrypt/restart/no writes, own HTTPS persisted non-exportable key
sign/public verify/local chain/no downloads, maintenance root JWT/CSRF read denial.
Tests reference other7probe slots synthetic/unverified; not overall readiness.
No private/tokens/XML output or copying runtime material to other identities.

Custody readonly test source review PASS/build0warnings0errors; fixed Windows guard
inside deferred DPAPI resolver. Actual run pending main workspace; dedicated users
cannot read manager .codex checkout. No access grants/secret copy to bypass isolation.

Negative runtime-store case now runs for both genuine maintenance and schema
Windows identities (SQL db_owner never implies runtime key custody). Same4test
definitions, expected5actual PASS across runtime3/maint1/schema1; no other SQL.

Actual W4 review PASS: runtime native JWT/HTTPS, CSRF operator and HTTPS public
leaf trust provision receipts true. Post-process runtime3/maintenance1/schema1 =
5readonly actual custody PASS, CSRF encrypted key digest unchanged. No secret
export/sharing/logging; dedicated profile TEMP only. Main/manager buildclean/
default1218PASS71intentionalSKIP; identical source/test trees in current memory.
Owner4hashes/untracked/unstaged unchanged. W4 no SQL mutation/rerun; previous W3
strict baseline unchanged at that checkpoint. Full M0 incomplete; W5 next.
