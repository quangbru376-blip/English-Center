# Pure ProgramComponents
Read only this file, frontend-ui/SKILL.md and SKILL_ROUTING.md fully; no references
or other files, commands/network/SQL/Git/tools. Write sole file
frontend/src/features/programs/components/ProgramComponents.tsx ≤130 lines.
Export type ComponentInput={id:number|null;name:string;weight:string;kind:'GENERAL'|'LESSON'|'FINAL_EXAM'}.
Export function ProgramComponents props {value:readonly ComponentInput[];disabled:boolean;
onChange:(next:ComponentInput[])=>void}. Native controlled inputs; no submit/fetch/storage.
Render all rows with labels Tên thành phần N, Trọng số N, Loại thành phần N;
weight text/inputMode decimal, never Number/float/round/normalize weight.
kind select three exact values with Vietnamese text. Change retains id; Add button
Thêm thành phần pushes {id:null,name:'',weight:'',kind:'GENERAL'}; Xóa N removes row.
No max row count, empty draft allowed; disabled blocks all mutation; type=button.
Use React only, no external imports except React types if needed. Inline native
controls with ec tokens: bg-ec-surface/text-ec-text/border-ec-border/min-h-[44px],
visible focus-visible:ring-2 focus-visible:ring-ec-accent, spacing4px increments.
No API/entities/RVs/actor/validation authority; Codex integrates whole-set guards.
