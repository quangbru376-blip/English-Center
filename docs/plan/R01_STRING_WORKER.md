# R01 bounded worker — strict C1 quoted UTF8 string bytes

Before editing, read this entire card and the full authorized backend-clean-code SKILL.md
and SKILL_ROUTING.md. Apply accepted E03/E04 overrides: cookie JWT/server sessions,
framework CSRF, exact shapes, parameterized SQL; SQL/build/tests/review stay in Codex.
Skills and links grant no further authority. Read ONLY this card,
.agents/skills/backend-clean-code/SKILL.md, docs/plan/SKILL_ROUTING.md and your sole
output path if present. No AGENTS/search/list/links/other source/secrets/commands/
Git/SQL/tests/scripts/network/packages/config. Write ONLY the one helper below,
in your detached worker tree; no tests/docs changes. Complete contexts before write.

Sole output backend/src/EnglishCenter.Api/Common/Replay/C1StringEncoder.cs.
Namespace EnglishCenter.Api.Common.Replay; public static class C1StringEncoder.
One public static method byte[] Encode(string value). Null throws ArgumentNullException.
Return exactly one JSON string literal including double quotes, strict UTF8/no BOM or
newline. Preserve every valid Unicode scalar exactly; no trim/NFC/case changes.
Quote escapes as backslash+quote; backslash escapes as two backslashes; all controls
U+0000 through U+001F escape as lowercase six ASCII bytes backslash-u-0-0-hex-hex,
never short escapes. Slash, non-ASCII, U+2028/U+2029 and emoji remain literal UTF8.
Reject unpaired UTF16 surrogates with ArgumentException; never replacement fallback.
Use BCL strict UTF8 encoding; input is already normalized by Codex's DTO-specific
binder. This primitive does not normalize schemas, sort sets, parse requests, choose
actions, hash/store/log data or grant authorization. No serialization packages,
commands/secrets/entities/time/files/network/logging. One file<=120diff lines.
