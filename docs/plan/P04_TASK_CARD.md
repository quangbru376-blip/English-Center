# P04 — Nonsecret normalization and student DOB

Owner requested continuation after P05 on 2026-10-04; P04 is the next bounded
task in the accepted order. ADR-163 authorizes main application after review PASS.
No P06, password processing, API/binder/error envelope, SQL, uniqueness queries,
migrations, DI wiring, packages or frontend changes.

## Scope and exact API

Six new files, namespaces EnglishCenter.Api.Common.Validation and matching tests:
- backend/src/EnglishCenter.Api/Common/Validation/NonsecretText.cs
- backend/src/EnglishCenter.Api/Common/Validation/ContactNormalizer.cs
- backend/src/EnglishCenter.Api/Common/Validation/StudentBirthDateValidator.cs
- backend/tests/EnglishCenter.Tests/Common/Validation/NonsecretTextTests.cs
- backend/tests/EnglishCenter.Tests/Common/Validation/ContactNormalizerTests.cs
- backend/tests/EnglishCenter.Tests/Common/Validation/StudentBirthDateValidatorTests.cs

NonsecretText static NormalizeRequired(string? value, int maxCodePoints) and
NormalizeOptional(string? value, int maxCodePoints): reject nonpositive max,
reject unpaired UTF16 surrogates, trim edges, normalize NFC, then count Unicode
scalar/code points (not UTF16 units). Required null/blank errors; optional
null/blank becomes null; nonblank over limit errors, never truncate. Preserve
internal whitespace/punctuation/case. This helper is expressly for nonsecret
text; no password API or password transformation. Limits passed by consumers:
name100, address255, catalogue150/100, reason1000. No extra name/age/control/
HTML-stripping restrictions or global name uniqueness.

ContactNormalizer static NormalizeEmail(string? value), NormalizePhone(string?
value), NormalizeOptionalPhone(string? value). Emails: trim edges, required,
ASCII only, <=254 characters, reject every internal whitespace/control, exactly
one address with local@domain syntax. Use BCL MailAddress.TryCreate and exact
parsed.Address equality to avoid accepting display names/lists/comments or
rewriting the supplied address. No provider rewriting/dot removal/tag removal,
no invented TLD/DNS/deliverability rule. Return invariant lower-case comparable
ASCII string for account-wide CI uniqueness; actual uniqueness belongs to DB
and service later. Test dot and +tag distinctions remain distinct.

Phones: trim edges, remove space separators (Unicode SpaceSeparator category,
including ASCII space) and ASCII hyphens. Reject internal tabs/newlines/controls,
parentheses/dots/letters/nonASCII digits or other punctuation; no hidden number
coercion. Compact input must be ten ASCII digits starting0 or equivalent +84
followed by nine ASCII digits; canonical output0xxxxxxxxx. No carrier prefix
catalogue, OTP or foreign/fixed-line extensions. Required null/blank errors;
optional null/whitespace blank becomes null. Separators-only nonblank invalid,
not a null contact. Optional API does not make Student phone optional.

StudentBirthDateValidator sealed class constructed with BusinessClock (reject
null), Validate(DateOnly dateOfBirth) returns same date or throws ArgumentException
when dateOfBirth > clock.GetToday(). DateOnly guarantees actual calendar dates;
strict wire date parsing is P06. Today valid; no minimum/maximum age or staff DOB
rule, DateOnly.MinValue accepted. Use P05 clock; no host date/clock/timezone.

Argument exceptions are bounded primitive errors with no supplied-value echo;
P07 later maps field errors/status, not this task. No DB or API success claims.

## Canonical trace, skills and read scope

Sources loaded by Codex: requirements ADR-073–077/091/C09 and verification cases;
DECISIONS ADR-073–078/091; E04_DTO_SPEC 1/API018 and DATABASE_SPEC account storage.
Unicode and phone separator details above materialize those existing contracts;
no provider/age/name/prefix business rule added.
implementation_skills: backend-clean-code; review_skills: code-review;
integration: antigravity-worker, Codex manager.
skill_sources SHA256 backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915.
applicable_overrides: canonical cookie/session/CSRF and real SQL Server override
stale bearer/SQLite; no auth/DB/API/migration here. Card is execution slice,
not obsolete plan.md/api.md. No integration/plugin other than bounded AGY.
read_context: AGENTS.md, this card, SKILL_ROUTING, complete backend-clean-code,
P05 BusinessClock.cs and VietnamTime.cs; plus exact six output files if present.
Current owner continuation authorizes this bounded AGY scope in existing workflow.
No secrets or wider repo sharing. Worker reads complete six context files before
edit, no links/scans/commands/network/browser/MCP/subagents. Detached only;
max6files/650added+deleted lines, no project/package/lock changes.

## Verification gate

Meaningful xUnit tests: composed/decomposed NFC equality; supplementary rune at
limit and over limit, unpaired high/low surrogate rejection; null/blank required
versus optional, edge trim/internal preservation; names1/100/101, address255/256;
email invalid shapes/display/list/control/nonASCII/length254/255, exact CI
equivalence and dot/tag distinction; phone0/+84/spaces/hyphens equivalence,
wrong lengths/prefix/symbols/nonASCII digits and optional blank distinction.
DOB injected mutable test TimeProvider: yesterday/today/future, no age limit,
Vietnam midnight/year/leap rollover and null clock. No real clock assertions.
Codex audits all tool events/receipt/diff, builds warnings as errors with locked
restore and runs all P02/P05/P04 tests. PASS -> exact snapshot apply + main checks.
skill_verification: pending actual implementation and checks.

## Execution, review and main application — 2026-10-04

Dispatch base4bb7b7a. AGY run81794616-978, conversation
4c80dede-0d7b-41ba-bb70-24a83a4a2cc5; SUCCESS/exit0/exact6files/
613runnerlines/zero violations. Codex inspected all19tool calls:13view_file
(6complete context reads,1missing allowed-file probe,6final reads) and6writes.
No scans/linked reads/commands/network/browser/MCP/subagents observed.
[Contract](p04-evidence/worker-contract.json),
[receipt](p04-evidence/worker-receipt.json),
[hashes](p04-evidence/verification-hashes.json). Raw events remain local only.

Independent initial locked restore and build0warnings/0errors succeeded;
initial tests106pass/5fail: wrong NFC fixture codepoint and malformed UTF16
InlineData serialized into replacement characters before validation. Manager
corrected Vietnamese fixture and constructs raw surrogate strings inside test
using integer scenario attributes, preserving six distinct invalid UTF16 cases.
Added NFC-after-normalization length and optional address boundary assertions.

Manager also closed observed MailAddress parser leniency for unquoted local
trailing dots and empty/edge-hyphen domain labels, without dot/tag rewriting,
DNS/TLD/verification/prefix/age rules. Added exact-address comment/list/control
regressions and zero-prefix phone case proving no carrier catalogue constraint.
This is the bounded ASCII/exact-address BCL profile, not a claim of full RFC or
mail deliverability verification; no email local64 restriction was added beyond
accepted total254. Exceptions never echo supplied data.

Final code6files/627insertions within650cap; complete source/scope and staged
whitespace review PASS. No project/package/lock/Program/P05/frontend changes.
Skill behavior verified by context events, actual normalization/cutoff source,
negative tests and independent validation. skill_verification: **PASS**.
Codex code-review verdict: **PASS after manager corrections**, no unresolved
P04 must-fix. Endpoints/auth/DTO binding/SQL/migrations/UI N/A; actual global
uniqueness enforcement and exception-to-public-error mapping remain future
service/DB/P07 work. No claim of whole slice/M0 readiness.

Detached final6a5269c45ac450b119ff4d076e9801de854fe259 retained at
C:\Users\asus\AppData\Local\Temp\codex-antigravity-worker\worktrees\main_workplace-81794616-978.
Applied under ADR-163 as main d9840bdaa885dab951d2c90f70fe1f46aa5e6ff6.
Exact backend source equality verified. Detached and main locked restore,
warning-as-error build0warnings/0errors and120tests pass/0fail/0skip verified
(79P04 +31P05 +10P02). [Main test counts](p04-evidence/main-test-counts.json).
No push/deployment or installation. P04 complete; next proposed task is P06
strict scalar/version/numeric parsing, not started in this bounded request.

Code flow: callers choose required/optional nonsecret text and field limit;
helper validates UTF16 then trims/NFC/counts scalars. Contacts return comparable
canonical email/phone values for later uniqueness checks. Student DOB validator
uses injected Vietnam BusinessClock; API parsing and public errors come later.
