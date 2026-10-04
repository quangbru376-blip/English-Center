# P06 — Strict scalars and exact numeric representations

Owner authorized next-stage continuation on2026-10-04 after P04. ADR-163 permits
main application after Codex PASS. Two independently reviewable bounded parts:
A scalar/version codec; B numeric codec. No P07/API binding/error envelopes,
routes/DTOs/auth/SQL/migrations/DI/packages/frontend changes.

## Part A — Four new files, 500-line cap

Namespace EnglishCenter.Api.Common.Serialization, tests matching namespace:
- backend/src/EnglishCenter.Api/Common/Serialization/StrictScalars.cs
- backend/src/EnglishCenter.Api/Common/Serialization/RowVersionCodec.cs
- backend/tests/EnglishCenter.Tests/Common/Serialization/StrictScalarsTests.cs
- backend/tests/EnglishCenter.Tests/Common/Serialization/RowVersionCodecTests.cs

Static StrictScalars.NormalizeUuid(string? value) returns lowercase D string,
exact36ASCII with hyphens8/13/18/23 and remaining hex; Guid.Empty invalid.
NormalizeUuidHeader(IReadOnlyList<string>? values) requires exactly one value
and same parser; no trim/coercion, no UUID version/time restriction.
ParseLocalDate(string?) exact10ASCII yyyy-MM-dd and real DateOnly;
ParseMinuteTime(string?) exact5ASCII HH:mm and real TimeOnly; no seconds/offset/
whitespace/Unicode digits, invariant parsing. No DOB/same-day schedule rules here.
ParseInt32(JsonElement value,int min,int max): Number only, raw JSON integer
lexeme (optional minus then ASCII digits; no dot/exponent/+); TryGetInt32 and
explicit caller range. Reject min>max. No coercion from string/bool/null;
-0 is a valid JSON integer, normalized to0 by later output. JSON document parse/
presence/unknown properties/error precedence are P07, not this primitive.

Static RowVersionCodec.Decode(string? value) requires canonical standard padded
Base64 exactly8bytes (12characters, one trailing =), no whitespace/URL-safe/
unpadding/extra padding/unused-bit alternatives. Decode then exact re-encode
comparison; return original eight bytes, no numeric/endian conversion.
Encode(ReadOnlySpan<byte> bytes) requires exactly8bytes; same canonical format.
Argument errors never echo input. No BigInteger or row-version interpretation.

Tests: UUID uppercase normalization and invalid shapes/empty/multiple header;
exact LocalDate leap validity and date/time whitespace/width/Unicode rejection;
JSON int type/lexeme/range/endpoints/overflow and invalid config;
RV known byte vector roundtrip, zero/allFF bytes valid, 7/9byte errors,
unused bits (AAAAAAAAAAB= versus AAAAAAAAAAA=), alphabet/spacing/padding errors.

## Part B — Four new files, 500-line cap

- backend/src/EnglishCenter.Api/Common/Serialization/ExactNumbers.cs
- backend/src/EnglishCenter.Api/Common/Serialization/ReportAmountCodec.cs
- backend/tests/EnglishCenter.Tests/Common/Serialization/ExactNumbersTests.cs
- backend/tests/EnglishCenter.Tests/Common/Serialization/ReportAmountCodecTests.cs

Static ExactNumbers.ParseMoney(string?) -> decimal: ASCII digits only, positive
<=999999999999999999, no sign/space/dot/exponent/separator, never round/truncate.
Leading zeros allowed then canonical output via FormatMoney(decimal) shortest
digits, same positive/range/integer guard. String length can reject unreasonable
spellings only if not changing valid leading-zero equivalence; prefer trim
leading zeros for numeric range checks, without trimming whitespace.
ParseScore(string?) -> decimal: ASCII digits with optional dot followed by1–2
digits, 0–100; ParseWeight(string?) same grammar but >0–100. Integer part present;
no signs/exponent/commas/space/3decimals/trailing dot, no rounding before validation.
FormatScore(decimal), FormatWeight(decimal): same range and <=2decimal-value
precision, invariant fixed2. Numerically exact1.000 is fine; input string1.000
is not. Do not implement weighted averages/report percent/business sums.

Static ReportAmountCodec.Parse(string?) -> BigInteger: canonical0 or nonzero
ASCII digit string with no leading zeros, max38digits/all9decimal38max.
Format(BigInteger) same 0..10^38-1 range, invariant canonical digits. Keep exact
above .NET decimal range; BigInteger is built-in, no package.
No JSON Number conversion/floating point or generic serialization framework.

Tests: Money bounds/overflow/zero/negative/fraction and canonical leading-zero
equivalence; score/weight0/100/precision/canonical80/80.5/80.50 and invalid
grammar/range; decimal formatter refuses fractional money/3decimal score;
report0/max38/max+1/decimal-overflow-but-valid exact roundtrip, noncanonical
spellings rejected; culture-independent output. No host culture/clock assertions.

## Authority, skills and verification

Canonical sources loaded by Codex: requirements ADR-091, E04_FINAL_CONTRACT
section3, E04_REPLAY_SPEC section2 normalized scalar table and E04_DTO_SPEC
aliases/31.1. No stale drafts override accepted final contract.
implementation_skills: backend-clean-code; review_skills: code-review;
integration: antigravity-worker with Codex manager.
skill_sources SHA256 backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
applicable_overrides: accepted cookie/session/SQL Server override stale bearer/
SQLite guidance; these and DB/migrations/API docs are N/A for pure codecs.
read_context per worker: AGENTS.md, this card, SKILL_ROUTING and complete
backend-clean-code only, plus exact current part output files if present.
Owner continuation covers this bounded AGY workflow; no secrets/wider sharing.
Each worker reads all4contexts fully, no links/scans/commands/network/browser/
MCP/subagents; only4files of current part, no other part/P04/P05 modifications.
Codex audits all events/receipt/full source/scope; locked restore, warning-as-error
build, full tests. Each PASS exact snapshot auto-applied and main reverified.
skill_verification pending actual checks; no worker validation claims accepted.
