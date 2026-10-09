# Public pure EntryScoreForm contract
Read exactly this entire file, .agents/skills/frontend-ui/SKILL.md and docs/plan/SKILL_ROUTING.md fully before writing; do not follow links/read any other file.
Write ONLY frontend/src/features/entryTasks/components/EntryScoreForm.tsx, ≤120lines and≤200diffLines. No commands/network/SQL/Git/validation.
Use React controlled native/shared Vietnamese UI, 44px targets, no dependencies/storage/fetch/UUID/auth/business permission decisions. Canonical policies override stale skill defaults.
Import Button from ../../../components/ui/Button and TextField from ../../../components/ui/TextField without reading them.
Export type ScoreFields={score:string;reason:string}; export function EntryScoreForm({mode,value,disabled,onChange,onSubmit,onCancel}:{mode:'GRADE'|'CORRECT_SCORE'|'MARK_ABSENT';value:ScoreFields;disabled:boolean;onChange:(value:ScoreFields)=>void;onSubmit:()=>void;onCancel:()=>void}).
GRADE/CORRECT_SCORE show required Điểm (0–100), type text inputMode decimal, preserve raw exact string, no parseFloat/round/default0. CORRECT_SCORE also required Lý do sửa điểm maxLength2000; GRADE no reason. MARK_ABSENT shows plain persistent Xác nhận học viên không dự thi. Vắng không phải điểm 0. No score/reason input for absence.
Use form submit preventDefault, call onSubmit once only !disabled; labels Lưu điểm/Sửa điểm/Xác nhận vắng by mode; explicit submit. Hủy type button calls onCancel, all inputs/buttons disabled while disabled. Parent owns one absence confirmation/permissions/exact manifests/time guards/errors; no extra modal/confirmation layer.
Only pure form; no ability to choose Student/Band/grader/versions. Max120lines, no tests/docs.
