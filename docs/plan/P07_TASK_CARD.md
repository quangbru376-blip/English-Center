# P07 — Error envelope and strict binding

Owner requested P07 → P08 → D01 in one batch on2026-10-04. ADR-163 permits
applying each Codex PASS to main. No P09/D02, SQL execution/migrations or deploy.

implementation_skills: backend-clean-code. review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: accepted E04 envelope replaces ProblemDetails; no bearer
auth/SQLite/business routes/Swagger/migrations. Auth/CSRF/session implementations
belong to A04–A06; this group supplies edge primitives and verifies ordering with
test-only guards, without claiming those later controls implemented.
read_context: AGENTS.md, this card, routing, complete backend skill; Part A also
Program.cs, StartupTests.cs. No linked reads, scans, commands, network, secrets,
MCP/browser/subagents. Only listed output files may be written in detached tree.

## Part A — Typed safe errors and global middleware

Allowed five files: Common/Errors/ApiErrorException.cs and ApiErrorMiddleware.cs
under backend/src/EnglishCenter.Api; existing Program.cs; corresponding
ApiErrorTests.cs and ApiErrorHostingTests.cs under backend/tests/EnglishCenter.Tests/Common/Errors.
Initial5file/550line run7e67640c-145 rejected:677lines exceeded cap; no changes applied. Fresh retry writes only3production files/300lines; Codex writes2test files independently under450lines. .NET10 built-in APIs only, no packages/config/lockfiles.

Registry exact HTTP/code pairs:400 VALIDATION_ERROR,NO_CHANGE;401
AUTHENTICATION_FAILED,AUTHENTICATION_REQUIRED;403 FORBIDDEN,CSRF_INVALID;404
NOT_FOUND,RESULT_NOT_CONFIRMED;409 CONFLICT,STATE_CONFLICT,
IDEMPOTENCY_CONTENT_MISMATCH,CSV_EXPORT_LIMIT;413 REQUEST_TOO_LARGE;429
RATE_LIMITED;503 SERVICE_UNAVAILABLE;500 INTERNAL_ERROR.
Represent with ApiErrorCode enum and ApiErrorException taking that enum and
optional public field path. Static Validation(field) convenience factory.
Fixed safe Vietnamese messages, no input/custom exception messages. Field path
only simple ASCII letters/digits/dot/underscore/bracket notation, starts letter,
max128; invalid programmer configuration throws ArgumentException. fieldErrors
empty object or one path→fixed Vietnamese string array. Invalid enum rejected.
Middleware InvokeAsync(HttpContext): catch typed failures, BadHttpRequestException
(413 maps413; others400), unexpected exceptions500. Exactly camelCase code,
message,fieldErrors,traceId JSON; no other details. Generate opaque server GUID
traceId; log only traceId/code, never exception/body/query/credentials/PII.
Request-aborted OperationCanceledException does not write a response; started
responses rethrow. Register before endpoints; existing unmapped404 stays empty.
Tests all registry mappings, safe output on secret-bearing unexpected exception,
typed field errors, generic ArgumentException remains500, request cancellation,
started response, and actual WebApplicationFactory pipeline using only test-host
IStartupFilter endpoints. No public test/business routes in Program.

## Part B — Strict JSON object primitive

Separate contract after A. Exact allowlist/required fields, duplicate properties
at every depth, malformed JSON, wrong root/type, required absent vs explicit
null, strict parser adapters for P04–P06. No implicit JSON coercion. Validators
run only after caller auth/CSRF guards; test host proves failing guard prevents
body inspection and generic safe response. No SQL/state/live guards implemented.

Part B production only: Common/Serialization/StrictJsonObject.cs (1file/220lines).
Codex independently writes StrictJsonObjectTests.cs and BindingPrecedenceTests.cs
(2files/350lines). Read context includes reviewed ApiErrorException.cs and
StrictScalars.cs only in addition to mandatory files. Sealed StrictJsonObject
Parse(string? json,IReadOnlyCollection<string> allowed,IReadOnlyCollection<string>
required) returns detached cloned JsonElements in private dictionary, Ordinal
names, reject unknown/duplicate root properties and duplicates recursively at
every depth including escaped equivalent names. Object root only, no comments/
trailing commas, maxdepth64. Missing required property400; explicit null presence
kept for caller optional semantics. Unknown/untrusted names never echoed in
fieldErrors; root/malformed/duplicate errors use Validation() empty fields.
Validate caller schema required subset allowed and distinct nonempty names before
parsing as programmer ArgumentException, not client400. Only declared simple
ASCII camelCase field names allowed. Contains(field) preserves presence.
ReadString(field) requires present string (including empty as raw value), no
coercion/trim/normalization; ReadNullableString(field) absent/null→null but wrong
type400. ReadBoolean(field) exact JSON true/false; ReadInt32(field,min,max) delegates
StrictScalars with range config checked before binding. ReadParsed<T>(field,
Func<string?,T> parser) uses raw required string then P04/P06 parser; catch only
parser ArgumentException/FormatException/OverflowException into Validation(field),
no raw value echoed. Null parser and invalid schema/config remain programmer500.
Tests malformed/type/unknown/case/duplicate nested/escaped names, absent/null,
integer lexemes and P04/P06 adapters including exact money/report38/RV/time,
guard-before-binding test middleware with failing auth/CSRF/precondition and
malformed JSON proves401/403 before400; authorized malformed gets400. This is
ordering evidence for primitives/test hosts, not an authentication implementation.
Transport/body-size/content-type/deadline profile enforcement and real auth/CSRF/
request gate/state precedence are integration gates when routes/profile exist.

skill_verification: selected skills read in full by Codex; worker read/tool scope,
source review, locked restore/build/tests and main recheck pending.
Status: complete within primitive scope, Codex review PASS applied/reverified main.

## Verified checkpoint2026-10-04

A SUCCESS run341d74e7-a7e3files265runner lines/zero scope violations. Initial
run7e67640c-145 rejected677>550, never applied. Manager removed redundant enum/
method aliases, sealed typed exception, made fieldErrors snapshots independent,
cleared unstarted response and added no-store.34new independent tests; reviewed
effc28e/main653e1cc, locked restore/build0warnings0errors/321PASS.
B SUCCESS21cd1e2d-8771file196lines/zero violations. Manager fixed strict UTF8
encoding and escaped-surrogate decoding so malformed input is400 not500;36new
tests include test-only guard precedence and real P04/P06 adapters. Reviewed
8ddfb37/main5a49afa. Main5f266a9 rechecked locked restore/build0warnings0errors/
357PASS/0fail/0skip after fixing duplicate SDK Content in P08 prerequisite.
No business/test endpoints in production, no dependencies/DB/auth implementation.
Test guards establish ordering only; full transport/auth/CSRF/live SQL/replay
precedence remains a later integration gate, explicitly not claimed implemented.
skill_verification: complete backend skill/routing reads in successful runs;
Codex audited file-only tools, caps and source behavior, ran checks independently.
A did not emit an explicit AGENTS view event; its card/skill/routing reads and
actual behavior were independently verified. B explicitly read AGENTS too.
Worktrees retained. Source/diff and every tool path reviewed; no worker commands.


