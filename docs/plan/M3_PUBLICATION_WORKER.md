# Sole bounded Student published result presentation

Fully read this file, .agents/skills/frontend-ui/SKILL.md and docs/plan/SKILL_ROUTING.md before writing. These EXACT3 files are the only permitted reads. Do not follow links/read AGENTS/source/report/config. No commands/Git/tests/SQL/network. Sole write frontend/src/features/classes/components/StudentGradeResults.tsx, max125 source/165 diff lines. React type imports only, no dependencies.

Export type PublishedComponent = {componentId:number;name:string;kind:'GENERAL'|'LESSON'|'FINAL_EXAM';weight:string;score:string;scheduleLabel:string|null}.
Export type StudentGradeResultsProps = {classCode:string;programName:string;finalizedLabel:string;finalScore:string;passed:boolean;teacherComment:string|null;components:ReadonlyArray<PublishedComponent>}.
Export function StudentGradeResults(props:StudentGradeResultsProps).

Pure read-only presentation with Vietnamese labelled heading/result, final score string verbatim, passed boolean supplied server displayed 'Đạt'/'Chưa đạt'; no calculating pass/rounding/totals/normalizing strings. Show classCode/programName/finalizedLabel, component name/kind label ('Chung','Buổi học','Thi cuối khóa'), weight% and score verbatim, scheduleLabel if present. Null comment text 'Chưa có nhận xét', non-null plain React text escaping; no dangerousHTML. Native semantic dl/articles/section, responsive stacked rows min-width0/break-words/no horizontal page overflow, system font, current CSS variables --ec-border/--ec-surface/--ec-text. No buttons, links, hooks, state/effects, fetch/storage/UUID, editable fields, workflow decisions, internal IDs/provenance/version/peers/return reasons. Codex supplies formatting and validated authorized props.
