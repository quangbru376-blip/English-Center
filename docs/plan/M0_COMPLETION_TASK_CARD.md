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
