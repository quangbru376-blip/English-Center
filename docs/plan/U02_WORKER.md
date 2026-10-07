# U02 controls worker — exact external payload

Prepared 2026-10-07 by Codex. Implementation authorized by owner; exact new
external sharing awaits consent. Codex is manager and reviewer.

## Read and command boundary

Read ONLY this complete file, `.agents/skills/frontend-ui/SKILL.md`, and
`docs/plan/SKILL_ROUTING.md`, in that order, fully before writing. Do not follow
links, inspect AGENTS/other files, list/search directories, run commands, Git,
validation, network, SQL, or read secrets. The sole outputs below may be read
after you create them. Never modify the three context files. No other reads.

## Output boundary

Create exactly these new files, at most 5 files / 500 added+deleted lines:

- `frontend/src/components/ui/Button.tsx`
- `frontend/src/components/ui/TextField.tsx`
- `frontend/src/components/ui/SelectField.tsx`
- `frontend/src/components/ui/Feedback.tsx`
- `frontend/src/styles/tokens.css`

Existing project: React/react-dom 19.3.0, TypeScript6.0.3 strict, Tailwind4.3.3;
ES modules, react-jsx automatic runtime. No packages, external assets/fonts,
router, icons library, form/cache/test library. Import React types explicitly
with `import type`; React useId may be imported normally. No `any`.

## Accepted source overrides

Canonical requirements ADR103/108–113 and E04 final override stale skill advice:
system fonts, white/gray surfaces, solid blue primary, visible focus, approximately
44px minimum controls, reduced motion; no decorative gradients/emoji.
Only installed React/Tailwind are available; skill's listed optional packages
are NOT permission to import/add them. Server errors are code/message/fieldErrors/
traceId, not ProblemDetails. Exact money digit strings never Number-convert.
JWT cookie only; no token/storage/HTTP/business behavior in these components.
No automatic confirmations, save, normalization, truncation or password changes.

## Exact component API and behavior

Use named exports, conventional small functions, token-based CSS custom-property
colors in Tailwind arbitrary values. Codex integrates CSS imports and callers.

1. `Button`: props extend React ButtonHTMLAttributes<HTMLButtonElement>, with
   `variant?: 'primary' | 'secondary' | 'danger'` and `pending?: boolean`.
   Default type=button, variant=primary. Pending disables the native button,
   sets aria-busy and displays Vietnamese progress text alongside children.
   Preserve explicit caller type=submit; native disabled remains disabled.
   Compose caller className with base classes, never allow prop spread to
   override effective pending/disabled or type accidentally. No form submission
   interception, click retries or internal business state.
2. `TextField`: props extend InputHTMLAttributes<HTMLInputElement>, with required
   `label: string`, optional `hint?: string`, `error?: string`. Use supplied id
   or stable useId. Visible label htmlFor; link hint/error using aria-describedby
   (also preserve caller descriptions), set aria-invalid when error is present.
   Spread native props with effective id and accessibility values afterwards.
   Controlled/uncontrolled input semantics are left to caller; do not change
   value/defaultValue, input type, autocomplete or raw password content. Do not
   use HTML maxLength to truncate accepted Unicode passwords automatically.
3. `SelectField`: same label/hint/error behavior, extending
   SelectHTMLAttributes<HTMLSelectElement>; caller provides native option children.
   Do not invent options, role values or business validation.
4. `Feedback`: props `kind: 'loading' | 'empty' | 'no-matches' | 'error' | 'success'`,
   `message: string`, optional `traceId?: string`. Plain escaped text, no HTML
   injection. Error role=alert, loading/success role=status, empty/no-matches
   normal text. Distinguish empty and no matches with Vietnamese default headings;
   errors remain visible; traceId rendered as labelled text, never logged.
5. `tokens.css`: :root variables `--ec-primary`, `--ec-primary-hover`,
   `--ec-surface`, `--ec-background`, `--ec-text`, `--ec-muted`, `--ec-border`,
   `--ec-danger`, `--ec-success`, `--ec-warning`, `--ec-focus`, `--ec-radius`.
   Text contrast >=4.5:1 for normal text on surfaces. Use system fonts only.
   Include a prefers-reduced-motion rule without global focus removal.

No App modification, identity pages, navigation menus, backend, mocks, tests,
documentation edits, commands or additional abstractions. Return a concise
file list and limitations. Codex runs all checks and reviews actual events/diff.
