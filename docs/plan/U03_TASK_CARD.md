# U03 — F02 signup UI and exact API018 seam

Status Codex review PASS2026-10-07, detached; main apply waits batch completion.
Base main8ff239d; codefb748e7; backend10ecb75c6fbd6e7155d8ed4b6a4723e74112e88a.
Codex manager; antigravity-worker bounded pure view only. U03 must PASS before U04.
Owner approved exact two payloads2026-10-07; U03 generated/reviewed, main not applied.

implementation_skills: frontend-ui; review_skills: code-review.
skill_sources: same SHA256 versions as U02_TASK_CARD (re-read full current skills
and routing2026-10-07), plus full antigravity contract reference. No new skill.
applicable_overrides: exact API018 DTO/201/no auto-login; cookie/CSRF RAM;
raw15–128 codepoint password; no provider rewriting/age/extra workflow/packages;
server safe code/message/fieldErrors/traceId, no optimistic creation or auto resend.
read_context: worker exactly U03_WORKER + frontend-ui skill + SKILL_ROUTING;
no owner report/reference/source/runtime/secret files shared. Manager ordered
memory/requirements/handoff/plan, E04 auth/signup/FE contracts and actual U01 code.
skill_verification: PASS; exact full3contexts before write, sole117-line output, no observed extra reads/commands/violations.

## Exact manager scope

Worker sole components/SignupForm.tsx as contract. Manager permitted:
frontend/src/features/identity/{types,signupApi,signupValidation}.ts,
frontend/src/features/identity/pages/SignupPage.tsx,
frontend/tests/signup.test.mjs, frontend/tests/identity-fixture.{html,tsx},
docs/notes/U03.md, this card/worker/contract/evidence and affected Project Memory.
Dependency fix scope: frontend/src/lib/http.ts for verified native-fetch receiver
bug (browser default fetch unavailable while lexical wrapper succeeds); browser
fixture regression required. No endpoint/transport/authority policy expansion.
No backend/App/main/package/config changes. Explicit exported page receives
existing HttpClient and onCreated callback; synthetic fixture mounts it for checks.
Default unavailable-functionality App stays unchanged until runtime authority.

Only fullName/dateOfBirth/email/phone/password required, address optional;
no role/status/identity fields. Validate on submit, preserve raw password;
server owns normalized uniqueness/blocklist/Vietnam date and complete validation.
One send per deliberate submit, pending double-submit guard, current-request check,
abort/unmount and registered RAM cleanup. CSRF bootstrap explicit; invalid CSRF
offers deliberate recovery, no mutation retry. Filter field errors to public keys;
escaped form/trace message,409 reload/review,500/lost response UNKNOWN, not rollback.
On201 exact CREATED/message only: clear password/draft and go login, never login.
Unknown offers deliberate login/Admin reset; no UUID/T23/email-exists/auto resubmit.
Do not persist or place fields in URL/logs; all form RAM cleared on identity boundary.

## Gates

Existing npm typecheck/lint/build; Node tests with existing TypeScript/React,
browser375/768/1280 labels/keyboard/raw Unicode, field errors/409/pending/201/
CSRF/network unknown and stale response cleanup. Fake API only synthetic fixture,
no runtime/SQL/readiness claim. Notes<=15 lines; code-review PASS then main apply/
reverify under ADR163. Four owner untracked files unchanged/unshared/unstaged.
No U05/restM0/runtime/SQL/install/push/deploy. Consent must cover exact new payload.

Review: AGYc1486cf8-494 SUCCESS; original tree retained. Manager28Node tests
PASS/types/lint/build PASS; Edge375/768/1280 signup success400/409/CSRF500/lost/
stale response+RAM cleanup PASS. Native-fetch receiver dependency bug fixed and
verified by default browser fetch; no endpoint/authority change. Source/structure/
security/frontend/notes gates PASS; backend/schema/API exposure N/A unchanged.
Real runtime/profile/cookies/SQL integrated proof unverified/outside scope; no V01 claim.
