# Pure Course form contract

Read this file, .agents/skills/frontend-ui/SKILL.md and docs/plan/SKILL_ROUTING.md
completely. Do not read other files or follow references. No commands/network/SQL.
Create ONLY frontend/src/features/catalogue/components/CourseForm.tsx <=130 lines.
Use React useState, existing shared TextField from ../../../components/ui/TextField
and Button from ../../../components/ui/Button (do not read those files).
Props: initial?: {code:string;name:string;basePrice:string;status:'ACTIVE'|'INACTIVE'},
editing:boolean, pending:boolean,
onSubmit:(value:{code:string;name:string;basePrice:string;status:'ACTIVE'|'INACTIVE'})=>void,
onCancel:()=>void. Named export CourseForm.
Controlled fields; Vietnamese labels; code readonly when editing, status only
shown for create. Native select label when create. basePrice stays string, never
Number/parseFloat; validate positive ASCII digits <=18 with no leading zero.
Code trim uppercase ASCII 1–30 A-Z digits _ -, name trim Unicode <=150 codepoints.
Show local form error role=alert; explicit submit/cancel; pending disables fields.
No fetching, auth, storage, UUID, audit, logging, business guards or new packages.
Use existing token classes/simple bordered layout. Canonical policy uses exact
VND digit strings and local system font; no external fonts/React Query imports.
