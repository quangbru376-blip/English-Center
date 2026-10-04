# P08 — Offline password policy and credentials

Owner batch P07→P08→D01 on2026-10-04; ADR-163 autoapply after Codex PASS.
implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: ADR114 ASP.NET PasswordHasher only; original password
unchanged/no trim/NFC/case conversion,15–128 Unicode code points, reject invalid
UTF16, no composition/periodic expiry rule. Pure tests no SQLite. No auth routes,
credential DB writes/Admin delivery UI/audit/session/SQL/migrations.
read_context: AGENTS, this card, routing, complete backend skill; Part B also
Part A implementation only. Worker no commands/network/scans/linked reads,
browser/MCP/subagents/secrets. Each fresh detached contract is bounded.

## P01 source prerequisite — verified by Codex

SecLists pinned commit56da8d991239cc52b0da594db53457504a757670, MIT.
Source directory Passwords/Common-Credentials at
https://github.com/danielmiessler/SecLists/tree/56da8d991239cc52b0da594db53457504a757670/Passwords/Common-Credentials.
- 10k-most-common.txt:73026bytes/10001records,
  SHA25668782D6A4A19A4768D5F15DD66BD534E7A33055CC755411E33F16D18C50FDCCE.
- probable-v2_top-12000.txt:100206bytes/12645records,
  SHA256EA4C906EBB0B26790C549A047962573F72CCC26F42212B83D70165D9C03FB72B.
- LICENSE:SHA2563DBDC93D5F8829DE0941744841730A09C106D0732E5AE0E98CA1D77BE7DED66C.
Strict UTF8 verified; merged15310Ordinal entries. Preserve original bytes and
license under Resources/PasswordBlocklist. Review2026-10-04; next2027-01-04.
Finite corpus is not exhaustive breach detection. Case-sensitive full-string
Ordinal lookup preserves accepted raw semantics; no substring/fuzzy transforms.

## Part A — Loader and policy,2production files/250lines

Common/Security/OfflinePasswordBlocklist.cs and PasswordPolicy.cs plus matching
tests under backend/tests/EnglishCenter.Tests/Common/Security. Loader static
Load(string directory) verifies all3files SHA256 constants before UTF8 decode,
exact records10001/12645 and merged15310; validates license too. Fail closed with
fixed InvalidOperationException text/no inner exception on missing/tampered/
invalid corpus; never includes file contents. Immutable private HashSet Ordinal,
Contains(string) and Count only; no enumeration/logging/raw corpus exposed.
Split LF and remove only line-ending CR; skip terminal empty line, no trim/NFC/
case conversion. Public internal test helper unnecessary: test real shipped
corpus, then copied directory tampered/deleted/license-corrupt. Policy constructor
requires blocklist; Validate(string?) returns SAME string instance if valid,
otherwise ApiErrorException.Validation("password") fixed error. Scan UTF16 manually
or Rune.DecodeFromUtf16, reject unpaired surrogate, count codepoints15–128 then
reject exact blocklist match. Test actual listed15+character password privately
from corpus without printing it, Unicode supplementary boundaries, spaces/raw
normalization distinction, invalid UTF16 created inside tests rather than attrs.

## Part B — Hashing/generator,2production files/220lines

CredentialHasher.cs, TemporaryPasswordGenerator.cs; Codex writes CredentialTests.cs (1file/250lines).
Hash(string password) uses injected PasswordPolicy then ASP.NET Core built-in
PasswordHasher<object> IdentityV3 explicitly220000 PBKDF2-HMAC-SHA512 iterations,
128bit random salt/256bit subkey. No custom crypto/no package. Verify(string?
hash,string? password) returns PasswordVerificationResult; null/invalidUTF16/
over128codepoints/malformed hashes fail safely, no policy/blocklist gate when
verifying existing password, no rehash DB side effect. Preserve input; never log.
Hashed DB values trusted server outputs; constrain canonical base64 decoded
length61/currentV3 format, PRF2/salt16/iterations positive <=220000 before verify
to avoid attacker-controlled expensive iterations. Older V3 SHA512 lower work
factor can return SuccessRehashNeeded. Unknown/corrupt formats Failed.
Generator injected policy, Generate() returns32ASCII chars from64-character
alphabet A-Z a-z 0-9 -_, each RandomNumberGenerator.GetInt32 unbiased. Recheck
policy/blocklist up to64attempts then fail fixed text. No store/log/cache/plaintext
field. UI/service once-only delivery is future integration, not claimed here.
Tests random salted hashes differ/verify/wrong password/raw spaces/case/NFC,
decoded algorithm/header/length84<512, rehash-needed oldV3, malformed payload,
100generatedlength/policy checks and distinct samples (not proof of randomness).
Manager measures220k latency separately, not a full load/throttle claim.

Codex integration: source bytes/license/provenance manifest copied exactly;
csproj copies resources to output+publish. Program eagerly loads validated
blocklist before host build and registers singleton policy/hasher/generator.
No credential network/logging and no business routes. Existing startup tests
exercise actual eager load; direct loader tests establish missing/corrupt fail.
skill_verification: pending worker audit, Codex build/tests/review/main recheck.
Status: source integrated with exact-byte .gitattributes, output/publish copying; P07 main357PASS. P08 code in progress.

