# U04 login form — exact external worker payload

Prepared2026-10-07 by Codex manager for owner-requested U03→U04 continuation.
Read ONLY this complete file, `.agents/skills/frontend-ui/SKILL.md` and
`docs/plan/SKILL_ROUTING.md`, in that order, fully before writing.
No links/other files/AGENTS/list/search/commands/Git/tests/network/SQL/secrets.
Own output may be read only after creation. Do not modify contexts.

Create ONLY `frontend/src/features/identity/components/LoginForm.tsx`;
one file, at most120 added/deleted lines. React19.3/TypeScript6 strict/Tailwind4;
explicit React type imports, no any/dependencies/assets/fonts/icons library.

Canonical overrides: system fonts/existing tokens/shared controls/44px;
Vietnamese UI; JWT only HttpOnly cookie, context/CSRF RAM; safe error envelope
not ProblemDetails. Pure controlled form only, no hooks/state/fetch/storage/
logs/auth/UUID/retry/timers/signals/navigation. Codex implements all side effects.
No role switcher, remember-me, refresh, CAPTCHA/OTP/email recovery or real
change-password controls (U05 is outside scope). No generic success markup.

Export LoginFormProps = {email:string;password:string;pending:boolean;
errors:Partial<Record<'email'|'password',string>>;
onChange:(field:'email'|'password',value:string)=>void;onSubmit:()=>void}.
Named export LoginForm(props:LoginFormProps).

Import named TextField from '../../../components/ui/TextField', Button from
'../../../components/ui/Button'. TextField extends native input props with
label/hint/error; Button extends native button props plus pending and variant
primary/secondary/danger. Do not read these files; Codex verifies integration.

Native form noValidate; onSubmit preventDefault and callback only when !pending.
Two controlled required fields ids login-email/login-password and native names
email/password. Visible labels 'Email'/'Mật khẩu'; types email/password;
autocomplete username/current-password. Disabled while pending; supplied errors
attached per field. Forward event.currentTarget.value verbatim; no password
trim/NFC/coercion/maxlength/minlength/pattern or inferred account status.
Button type submit pending supplied, text 'Đăng nhập'. Simple grid gap-4.
Plain help text 'Quên mật khẩu: liên hệ Admin để đặt lại.'; no recovery link/API.
Do not expose whether an email/account exists or is locked/throttled internally.
Return concise result/limitations; suggest manager checks, never run commands.
