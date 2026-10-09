# M0 completion — owner assignment 2026-10-08

Base main663d0a5 / managerb45084a. Owner requests completing M0, not only two packages.
ADR177 covers necessary bounded implementation/delegation/context sharing/checks;
ADR163 covers applying independently reviewed PASS. Codex manager; AGY policy only.
Status IN PROGRESS. No M1–M4, packages/install, push/deploy or owner-file changes.

## Completion conditions and order

1. V01-J: fresh runtime/approved patch-artifact checks, exact reference binding.
2. V01-K: usable actual JWT signing/validation material, explicit rotation windows.
3. V01-L: protected persisted framework CSRF keys, read-only proof of existing
   encrypted keys, active non-revoked usable material and restart decryption.
4. Prepare subsequent concrete transport, applied execution and maintenance contracts.
5. Bind explicit actual profile/principals/keys, integrate only M0 routes/UI and
   verify least privilege/catalog, startup refusal, readiness and browser→owned SQL.
6. Final trace/owner walkthrough; full M0 only after required real evidence passes.

Operator environment fields remain required, not invented: origin/listen/port,
finite deadlines/body/preview envelope, PKI/key custody, maintenance bindings.
Existing SQL runtime cannot read the exact catalog; do not silently elevate it,
switch to owner inspection or disable readiness. Missing input keeps M0 incomplete.
An asynchronous missing-values question is pending; independent code proceeds.

## First code scope: J/K/L (separate reviewable probes, unmounted)

Manager may create Infrastructure/Readiness/RuntimeManifestReadinessProbe.cs,
JwtKeyReadinessProbe.cs, ProtectedCsrfReadinessProbe.cs in backend/src/EnglishCenter.Api;
tests/EnglishCenter.Tests/Readiness/KeyAndManifestProbeTests.cs and
ProtectedCsrfProbeTests.cs. No default Program/frontend/schema/grant changes in this
first contract. Future integration requires its own exact scoped contract.
AGY sole RuntimePatchPolicy.cs <=100 lines in Infrastructure/Readiness.
Manager docs: this card, V01J_WORKER.md, J-worker-contract.json, sanitized JKL
verification summary, notes/V01JKL.md and affected current memory/handoff/closure.

J requires approved version/date/pins metadata supplied explicitly, bound to opaque
patch reference; actual Environment/ASP.NET/EF versions and supplied fresh read-only
SQL-version resolver must match. No version latest lookup/install or inferred approval.
Fresh local artifact SHA256/length checks for exact SDK/API csproj/API lock/frontend
package/frontend lock paths; reject missing/tampered/reparse/remote/traversal inputs.
No default manifest/config/env load, no caching ready. Approved manifest contains no secrets.
K binds exact issuer/audience/reference set and an explicit current signing reference.
Fresh resolver returns borrowed material (probe does not dispose owner keys); reject
duplicate/missing/future/expired/incompatible material. Active signing material must
cover a complete two-hour session. Real signer→validator round trip with generated RAM
SID/account1 proves usable keys, not real account/session issuance. No tokens logged.
L uses an explicitly bound read-only persisted XML repository and existing framework
key manager. Reject plaintext/missing/revoked/inactive/unusable keys; decrypt existing
encrypted descriptor and use framework authenticated-encryption challenge in RAM.
Never CreateNewKey/Protect(auto-key generation)/StoreElement/revoke; no readiness writes.
Actual durable key provisioning/custody remains the later operator binding.
All probes propagate caller cancellation, observe late faults, fresh reads and finite
profile deadlines. Error output only NotReady, no exceptions/secrets/paths dumped.

## Skills and sharing

implementation_skills backend-clean-code; review_skills code-review (Codex).
skill_sources SHA256 backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
Complete relevant skills/contract/routing read. applicable_overrides accepted cookie
JWT/CSRF/NET10/EF10/SQL17Express/160/noSQLite/no new package/API/defaults.
read_context ordered Project Memory, NEXT/HANDOFF, requirements4.10, E04 runtime,
M0 plan/closure, actual readiness/auth/framework APIs. Worker reads ONLY J brief,
complete backend skill and routing; own sole output read allowed. No source/links/
AGENTS/owner/secrets/search/listing/commands/network/SQL/Git/testing. ADR177 plus
current whole-M0 assignment covers exact sharing. No repeat permission question.
skill_verification pending actual event/diff review and manager tests.

## Verification

Manager/main solution --no-restore -warnaserror; default full non-SQL suite. New tests
real shipped nonsecret artifacts/loaded runtime, tamper/delete/reference/version/date,
actual HMAC/RSA/ECDSA material and wrong key/rotation/issuer, cancellation/unavailable.
L real framework DPAPI-protected XML in exact Guid-owned Temp directory on Windows,
recreated key manager proves persistence/decryption; plaintext/disabled/revoked/corrupt
inputs fail. No StoreElement during checks. Temp deletes restricted to exact owned
Guid/root; raw key/token/connection contents never evidence or external context.
Four owner files baseline hashes preserved/unshared/untracked/unstaged.
This scope supplies probes, not active environment or full-M0 acceptance.

## V01-M — explicit profile reader, next independent code scope

2026-10-08: J/K/L manager47new+1061=1108PASS/58opt-in SKIP/buildclean;
review PASS and code applied main47c9c7f from manager4ff6f12; main recheck pending.
AGY82e61f9f-153 SUCCESS/sole96line policy/full28+46+101 context reads then write,
only own output reread, zero observed violations; EOF normalized only. Raw original
retained. L uses local Windows current-user DPAPI store only, explicit positive
file count/byte limits and exact framework decryptor; no generic vault provisioning.

M manager-only exact code scope Infrastructure/Readiness/RuntimeProfileReader.cs
and tests/EnglishCenter.Tests/Readiness/RuntimeProfileReaderTests.cs. No delegation
needed for this small strict-boundary implementation. Same skills/versions/overrides.
Explicit absolute local file and positive byte bound, fresh bounded async read;
no current-directory/env/default file discovery. Strict camelCase existing typed
record shape; JSON duplicates/unknown fields/stringified numbers/integer or case-
variant enums/depth>8 invalid. Deserialize only reference descriptors, validate
through existing RuntimeProfileValidator. Fixed enum failures, redacted result;
caller cancellation propagates, deadline supplied finite explicit bound. A valid
parse is never ready, does not resolve keys/SQL or mount Program. Preserve runtime
versions/owner deferrals; no new activeprofile values. Verify real Guid-owned
Temp profile files, exact roundtrip and malformed/refused/fresh/cancel cases, full
manager/main suites/build. Update affected docs after verification/apply PASS.

## Independent review — J/K/L/M PASS; full assignment still IN PROGRESS

JKL manager/main47new+1061=1108PASS/58intentional opt-in SKIP; M adds24 cases,
final both1132PASS/58SKIP/0fail, builds0warnings0errors --no-restore -warnaserror.
JKL main47c9c7f/manager4ff6f12; M mainfecb90e/manager7d99619. Identical src
51d5c80913fe325ee9d9db7d70c943e5a8b8003f/tests ae4f240eb42662384dd58e74851d76a9a57d4a24.
Main applied only after manager review/full PASS, then independently reverified.
Skill verification: actual five AGY tools match allowlist, full allowed reads before
write; sole96line output, zero observed violations, EOF-only integrated normalization.
Original run82e61f9f-153/receipt/events/policy retained; hashes in sanitized summary.
Codex review checks scope/cancellation/redaction/actual file and cryptographic checks.
Initial JKL38PASS/9fail: DPAPI encryptedSecret namespace check wrong; corrected to
framework namespace, not weakened. Final DPAPI restart/revocation/future/expiry/
unknown-decryptor/corruption/duplicate/bounds/read-only byte equality PASS. Full
ancestor reparse check and SQL-version reference binding strengthened before final.
JWT active-key proof cannot be satisfied merely by a retiring rotation key; actual
three algorithms/wrong key/weak key/date/rotation/identity/unknown exceptions tested.
Profile duplicate/unknown/case/integer enum/stringified number/UTF8/size/fresh deletion/
missing groups/no defaults/caller cancellation tested. Valid source is not ready.
Frontend/default entries/schema/grants/packages untouched, SQL not rerun, owner4
hash-identical/unshared/untracked/unstaged. Notes15lines; no API route/DTO/migration
change (review checklist N/A). New probes/readers remain unmounted with no active
config or secret provisioning. Actual profile values asked are still pending;
remaining three categories, runtime authentication/catalog, PKI/custody and maintenance
bindings prevent closing full M0. See M0_ENVIRONMENT_INPUTS; preserve owner deferrals.

## V01-N/O/P component PASS, M0 remains IN PROGRESS
All9 concrete categories implemented/unmounted. See V01NOP_TASK_CARD/ADR182/NOP summary; main d145072 and manager63cff80 both1169PASS/58opt-in SKIP/buildclean. Pending concrete LOCAL_DEV selection and actual runtime/catalog/PKI/protected-store/maintenance/startup integration; no fixture promotion or repeated authorization gate.

## V01-Q/R/S PASS — whole M0 still IN PROGRESS
Q explicit HTTP readiness and S narrow metadata allowlist applied/reverified;1197defaultPASS/58opt-in SKIP/buildclean and3read-only SQL denial cases each. R metadata candidate/ablation880/880facts, S3rollback grant cases. See cards/QRS summary/ADR183–184. No permanent grants or runtime activation; next actual profile/principal/PKI/custody/maintenance/startup integration.

## V01-T PASS — whole M0 still IN PROGRESS
Maintenance SQL effects/authority main1f0f0e9/manager7dbcb4d applied/reverified:
1197defaultPASS/63opt-in SKIP/buildclean and5SQL PASS each. Database grant-option
loophole fixed; exact90d/24h expiry effects/mandatory retention/IP cleanup/future/
idempotence/rollback verified,10scratch DBs removed, D07/owner unchanged. Actual
principal/TLS/scheduler and profile/custody/startup remain open; see T card/summary.

## V01-U PASS — whole M0 still IN PROGRESS
2026-10-09 main8bc6a1a/manager942403b,6scheduler lifecycle cases PASS,
1203defaultPASS/63opt-in SKIP/buildclean each. Actual loops with synthetic timers/
RAM resolvers prove independent periods/non-overlap/coalescing/stop/refusal. Source/
SQL/owner unchanged; actual scheduled SQL authentication/TLS/owned COMMIT and
profile/custody/startup still open. See U card/summary.

## V01-V PASS — whole M0 still IN PROGRESS
2026-10-09 main4daa2e2/manager22ed355,14startup lifecycle cases PASS,
1217defaultPASS/63opt-in SKIP/buildclean each. Explicit post-listener Q check
requests host stop if unavailable; no cached request authority/default registration.
Actual environment/all9 wiring/principals/TLS/custody/scheduled SQL still open.

## LOCAL_DEV selected — actual OS authority blocks provisioning
2026-10-09 owner selected proposal table and10000preview work units, ADR186.
Reference-only M0_LOCAL_DEV_PROFILE_TEMPLATE.json prepared; UNBOUND refs are not
secrets or ready evidence, not loaded by Program. Read-only preflight manager
Windows AdministratorToken=false; SQL Windows-only/sysadmin diagnostic; strict
encrypted SQL chain untrusted. One existing localhost key candidate,8443free.
No provisioning/SQL/config/key/trust changes. Operator must restart Codex elevated
to prepare/apply bounded Windows identity/server TLS contract. See M0_OPERATOR_CLOSURE;
do not replace true authentication or certificate trust with fixture/owner bypass.
