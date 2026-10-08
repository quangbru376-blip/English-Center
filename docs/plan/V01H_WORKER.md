# V01-H worker — pure pinned metadata policy
Read ONLY this complete brief, .agents/skills/backend-clean-code/SKILL.md and
docs/plan/SKILL_ROUTING.md in order before writing. Apply canonical overrides.
No source/AGENTS/links/owner/secrets/listing/search/commands/network/SQL/Git/tests.
Own sole output read allowed. Codex owns IO/probes/integration/review/tests.
Create ONLY backend/src/EnglishCenter.Api/Infrastructure/Readiness/BlocklistManifestPolicy.cs
<=100lines, namespace EnglishCenter.Api.Infrastructure.Readiness.
public static class BlocklistManifestPolicy; public static bool IsCurrent(
System.Text.Json.JsonElement manifest,System.DateOnly today). Pure/no IO/no logging.
Root exactly8unique keys: license,revision,source,mergedCount,files,nextReviewDate,
comparison,reviewDate. Correct JSON types, no duplicates/extra/missing keys.
Exact strings MIT; revision56da8d991239cc52b0da594db53457504a757670; SecLists;
comparison "Ordinal exact full string; no transforms". mergedCount int15310.
Dates exact pinned strings2026-10-04/2027-01-04; today >=2026-10-04 and <2027-01-04.
files array exactly3unique paths, order irrelevant. Object exact keys path/sha256/
records for corpus, path/sha256 for license. records int10001 or12645, exact path/hash:
10k-most-common.txt /68782D6A4A19A4768D5F15DD66BD534E7A33055CC755411E33F16D18C50FDCCE /10001
probable-v2_top-12000.txt /EA4C906EBB0B26790C549A047962573F72CCC26F42212B83D70165D9C03FB72B /12645
LICENSE /3DBDC93D5F8829DE0941744841730A09C106D0732E5AE0E98CA1D77BE7DED66C
All scalar strings ordinal; numbers JsonValueKind.Number+TryGetInt32; wrong kind/
duplicate/malformed root/array/property types =>false, no exceptions or fallback.
Use System.Text.Json/BCL only; actual corpus bytes/credentials never supplied.
