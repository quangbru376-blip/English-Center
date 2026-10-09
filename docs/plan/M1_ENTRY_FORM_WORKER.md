# Pure EntrySessionForm
Read only this file, frontend-ui/SKILL.md and SKILL_ROUTING.md fully; no references
or other files/commands/network/SQL/Git/validation tools. Write sole file
frontend/src/features/entrySessions/components/EntrySessionForm.tsx ≤130 lines.
Export type SessionFields={date:string;startTime:string;endTime:string;room:string;
capacity:number;graderId:number}. Export function EntrySessionForm props
{initial?:SessionFields;editing:boolean;pending:boolean;
graders:readonly {accountId:number;displayName:string}[];
onSubmit:(value:SessionFields)=>void;onOptions:(window:SessionFields)=>void;onCancel:()=>void}.
Native controlled date/time/text fields labels Ngày thi, Bắt đầu, Kết thúc, Phòng.
Create only Sức chứa number min1max20 and Giáo viên chấm native options with empty
placeholder. Editing retains initial capacity/grader but hides their controls.
Button Xem giáo viên phù hợp invokes onOptions with current fields (no network here).
Submit Lưu on explicit form submit only, preventDefault, disabled/pending guard;
Hủy typebutton. HTML required fields, no hardcoded Teacher/UTC/status/asOf/default date.
Capacity/grader safe integer validation on submit; date/time authoritative server.
No fetch/storage/secrets/dependencies; React only. Use ec tokens/native44px focus
ring styling/Vietnamese/4px spacing; one file ≤130lines. Codex supplies HTTP/shared shells.
