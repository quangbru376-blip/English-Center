# V01-D worker — pure browser metadata assertions

Read this full file, .agents/skills/frontend-ui/SKILL.md, then
docs/plan/SKILL_ROUTING.md completely. These are the ONLY authorized input files.
Do not follow links, read source/AGENTS/owner files, list/search, run commands,
use browser/network/SQL/Git or validate. Codex owns privileged fixture/tests/review.

Create ONLY frontend/tests/browser/cookie-policy.cjs, at most70lines.
CommonJS; require only node:assert/strict. Export { assertBrowserPolicy }.
Signature: assertBrowserPolicy(metadata, authenticated).
metadata.cookies contains plain objects with ONLY name/domain/path/secure/
httpOnly/sameSite. metadata.javascriptCookieNames, localStorageKeys and
sessionStorageKeys are string arrays. No values/tokens/credentials are supplied.

Require exactly one __Host-ec-csrf cookie and, when authenticated, exactly one
__Host-ec-auth cookie. When anonymous, auth cookie must be absent. For each
expected cookie: secure=true,httpOnly=true,sameSite='Strict',path='/',
domain='127.0.0.1' (host-only browser domain, no leading dot).
Require neither expected cookie name visible in javascriptCookieNames;
localStorageKeys and sessionStorageKeys empty. Ignore unrelated cookie names.
Throw assertion failures with fixed secret-free messages; return undefined.
Use frontend-ui with routing canonical overrides: JWT HttpOnly cookie, RAM-only
state/framework CSRF/same-origin, no bearer/ReactQuery/packages. This pure helper
does not prove TLS trust, backend authority, SQL or production readiness.
Only your own output may be read after the three inputs. Do not modify docs,
production source/config/packages/locks or any other path. No code execution.
