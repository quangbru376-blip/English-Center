# English Center Management System — Agent Rules

University final project (course "Công nghệ phần mềm"): a responsive web app for an English language center. Roles (7, one fixed role per account): Student, Admissions, AcademicAffairs (giáo vụ), Teacher, Accountant, Director, Admin. The UI has 33 screens (F01–F33) built in 4 phases. The owner must explain every part of the code in an oral exam (vấn đáp), so clarity beats cleverness.

## Priority
A complete, working product comes first; stop at "good", not "perfect". Order of importance: correct business rules, then working end-to-end flows, then a clean consistent UI, then polish. Do not gold-plate: once a slice's acceptance checks pass, add no extra animations, optimisations, abstractions or features. If time runs short, cut scope (lowest-priority screens in plan.md), never the quality of finished slices.

## Stack (fixed; do not change or add anything without asking)
- Backend: ASP.NET Core Web API (C#), EF Core code-first, SQL Server, JWT auth
- Frontend: React + Vite + TypeScript + Tailwind CSS
- Tests: xUnit for backend services
- Folder layout and naming: skill `project-structure`

## Roles
- Codex = planner and reviewer. Writes plans in docs/plan/, reviews with skill `code-review`. Does not build large features itself.
- Antigravity = worker. Implements exactly one plan slice at a time.
- Chat history is not shared. Everything a worker needs must be in this file, the skills, or docs/plan/. If something is missing or contradictory, STOP and write the question in docs/plan/questions.md. Never guess business rules.

## Skills: read the matching one before working
| Task | Skill |
|---|---|
| Planning, splitting screens into slices, handoff | project-planning |
| Creating, moving or deleting files/folders | project-structure |
| Any backend, API or database change | backend-clean-code |
| Any page, component, style or UI wiring | frontend-ui |
| Reviewing a finished slice | code-review |

## Business rules (decided; never violate)
- Each class has exactly one teacher. No history of teacher changes is kept.
- Each invoice is paid by one single payment (no installments).
- Students may retake the entry test; the placement band comes from the highest-scoring attempt.
- Scholarship data (ScholarshipGrant) is separate from finance tables: no link to Invoice or Payment.
- Audit logging is done in the application layer, not as a database table.
- Schema of record: EF Core entities + migrations. The original T-SQL script in docs/reference/ is reference only.
docs/requirements.md is the source of truth (roles, permission matrix, states, rules BR-xx, Band table, screen priorities). A rule that is not there or here must be asked about, not invented.

## Working rules
1. Think first: state assumptions; ask when unclear; propose a simpler way if one exists.
2. Simple first: the minimum code that solves the task. No speculative abstractions, extra libraries or unrequested features.
3. Surgical changes: touch only what the task needs; match existing style; remove only what your own change orphaned.
4. Read before write: open the files you will change and their callers; reuse existing code before adding new.
5. Verify: decide how to check the result (build, test, run, look at the page) and actually run it before saying done. Report what you really ran and its output.
6. Explainable code: the owner has no frontend background. Prefer plain, conventional patterns, short files, clear names. Comments explain WHY only.
7. No secrets in git. Use user-secrets or .env; keep .env.example.

## Commands (update when the project is scaffolded)
- Backend run: `dotnet run --project backend/src/EnglishCenter.Api`
- Backend test: `dotnet test backend`
- New migration: `dotnet ef migrations add <Name> --project backend/src/EnglishCenter.Api`
- Frontend: `cd frontend && npm run dev` / `npm run build` / `npm run lint`

## Definition of done (a slice is done only when all are true)
- Backend and frontend build with no new errors or warnings; tests for its business rules pass.
- docs/api.md updated; docs/notes/<slice>.md written (max 15 lines: page → API → controller → service → repository → tables).
- Status in docs/plan/plan.md updated. No dead code, no stray files.
