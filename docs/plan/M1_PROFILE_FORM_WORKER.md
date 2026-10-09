# Public pure StudentProfileForm contract
Read exactly this entire file, .agents/skills/frontend-ui/SKILL.md and docs/plan/SKILL_ROUTING.md fully before writing; do not follow links/read any other file.
Write ONLY frontend/src/features/enrollment/components/StudentProfileForm.tsx, ≤120lines and≤200diffLines. No commands/network/SQL/Git/validation.
Use React controlled native/shared Vietnamese UI, 44px targets, no dependencies/storage/fetch/UUID/auth/business permission decisions. Canonical policies override stale skill defaults.
Import Button from ../../../components/ui/Button and TextField from ../../../components/ui/TextField without reading them.
Export type ProfileFields={fullName:string;dateOfBirth:string;phone:string;address:string}; export function StudentProfileForm({value,disabled,onChange,onSubmit,onCancel}:{value:ProfileFields;disabled:boolean;onChange:(value:ProfileFields)=>void;onSubmit:()=>void;onCancel:()=>void}).
Use native form controlled inputs labelled Họ và tên (required maxLength200), Ngày sinh (type date required), Số điện thoại (required type tel maxLength30), Địa chỉ (optional maxLength510). Preserve raw draft; server owns NFC/validation/normalization/real date/unique phone, no guessed age/no truncation.
On submit preventDefault and call onSubmit once only when !disabled. All controls disabled while disabled. Lưu explicit type submit, Hủy type button calls onCancel. No email/password/ID/version hidden inputs. No auto-save/confirm or extra component.
Only pure form; parent owns validation/versions/unknown handling. Max120lines, no tests/docs.
