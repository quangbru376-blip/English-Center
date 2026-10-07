# U05 pure password form — exact external payload

Read ONLY this complete card, .agents/skills/frontend-ui/SKILL.md and
docs/plan/SKILL_ROUTING.md, in that order, fully before writing. No linked files,
source/AGENTS/search/list/commands/Git/tests/network/SQL/secrets. Own output may
be read after creation only. Codex integrates/tests/reviews; do not edit context.
Create ONLY frontend/src/features/identity/components/PasswordChangeForm.tsx,
one file <=120 added/deleted lines. React19.3/TS6/Tailwind4, no any/new packages.
Canonical overrides: native shared controls/system font/tokens/44px, Vietnamese,
safe envelope not ProblemDetails; cookie JWT, RAM only. Pure controlled view,
no hooks/state/auth/fetch/storage/logs/UUID/timers/retry/navigation/normalization.
Export PasswordFields={currentPassword:string;newPassword:string;confirmation:string}.
Export PasswordChangeFormProps={values:PasswordFields;
errors:Partial<Record<keyof PasswordFields,string>>;pending:boolean;
onChange:(field:keyof PasswordFields,value:string)=>void;onSubmit:()=>void}.
Named PasswordChangeForm export. Import type FormEvent from react, TextField
from ../../../components/ui/TextField and Button from ../../../components/ui/Button.
These extend native props: TextField label/error/hint; Button pending,
variant primary/secondary/danger. Do not read their source.
Native noValidate form, preventDefault then onSubmit only !pending. Three
required controlled password fields, names currentPassword/newPassword/confirmation,
ids password-current/password-new/password-confirmation. Labels Mật khẩu hiện tại,
Mật khẩu mới, Nhập lại mật khẩu mới. Autocomplete current-password/new-password/
new-password respectively. Disable pending; forward currentTarget.value verbatim.
No min/maxLength/pattern/trim/NFC/composition rules. Supplied errors per field.
Button type submit variant primary pending, text Đổi mật khẩu. grid gap-4.
Plain help: Đổi thành công sẽ hủy mọi phiên và yêu cầu đăng nhập lại.
Confirmation is local only, not an API field. No automatic login or fake success.
