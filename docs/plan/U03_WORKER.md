# U03 signup form — exact external worker payload

Prepared2026-10-07 by Codex manager for owner-requested U03→U04 continuation.
Read ONLY this complete file, `.agents/skills/frontend-ui/SKILL.md` and
`docs/plan/SKILL_ROUTING.md`, in that order, fully before writing.
No links/other files/AGENTS/list/search/commands/Git/tests/network/SQL/secrets.
Own output may be read only after creation. Do not modify contexts.

Create ONLY `frontend/src/features/identity/components/SignupForm.tsx`;
one file, at most160 added/deleted lines. React19.3/TypeScript6 strict/Tailwind4,
React types explicit, no any, packages, assets, fonts or icons libraries.

Canonical requirements override stale skill guidance: system font, existing
tokens/shared controls,44px controls, plain Vietnamese copy, no ProblemDetails,
JWT-cookie/CSRF RAM only, no storage/PII URLs/logging. This is a pure view:
no hooks/state/validation/normalization/fetch/auth/navigation/automatic success.
Server owns password blocklist/uniqueness/DOB/contact rules. No composition,
confirmation password/age/OTP/CAPTCHA/consent/forgot-password workflow.

Export SignupFields = {fullName:string;dateOfBirth:string;email:string;
phone:string;address:string;password:string}. Export SignupFormProps with:
values:SignupFields; pending:boolean; errors:Partial<Record<keyof SignupFields,string>>;
onChange:(field:keyof SignupFields,value:string)=>void; onSubmit:()=>void.
Named export function SignupForm(props:SignupFormProps).

Use imported named TextField from '../../../components/ui/TextField', Button
from '../../../components/ui/Button'. Existing TextField props extend native
input props plus label/hint/error strings; Button extends native button props
plus pending:boolean and variant:'primary'|'secondary'|'danger'.
These relative imports resolve from features/identity/components to src/components.
Do not read those modules; Codex typechecks and integrates independently.

Render a native form with noValidate. onSubmit prevents default; call supplied
onSubmit only when !pending. All6 fields controlled, native name and id using
the exact field keys (prefix signup- for ids), visible Vietnamese labels:
Họ và tên; Ngày sinh; Email; Số điện thoại; Địa chỉ (không bắt buộc); Mật khẩu.
Types text/date/email/tel/text/password respectively. Required except address.
Disabled while pending. Each passes its supplied field error. Phone inputMode tel.
Use autocomplete name/bday/email/tel/street-address/new-password respectively.
Password hint: '15–128 ký tự; giữ nguyên khoảng trắng và ký tự Unicode.'
No password maxlength/minlength/trim/NFC/truncation/composition pattern.
Changes forward event.currentTarget.value verbatim, never echo values in hints.
Submit Button type submit pending=props.pending with text 'Đăng ký tài khoản'.
Simple grid gap-4, full width, no decorative panels or standalone page heading.
No inline field/form success, error logging, role/status selection or API effects.
Return concise limitations; suggest manager checks, never run commands.
