---
name: code-review
description: Review a finished slice of the English Center system against its plan, the business rules and the project skills. Use whenever asked to review, check, audit or approve code, a diff, a pull request or a completed slice, and before any slice is marked done.
---
# Code review (reviewer: Codex)

Review the diff of one slice against its slice file. Report problems; do not rewrite code unless asked. Be specific and honest: no praise padding, no vague "looks fine".

## Steps
1. Read AGENTS.md, the slice file in docs/plan/slices/, and the diff. Run the build, tests and acceptance checks yourself and report the real results.
2. Go through the checklist. For each failure give: severity, file:line, what is wrong, which rule it breaks, and a one-sentence fix.
3. End with a verdict (PASS, PASS WITH FIXES listing the must-fix items, or FAIL) and a list of anything you could not verify.

## Checklist
Scope
- [ ] Only files named in the slice changed; no extra features, libraries or refactors.
- [ ] All acceptance checks pass when run.
Business rules
- [ ] Each applicable rule from AGENTS.md is enforced in the service and covered by a test.
Backend (backend-clean-code)
- [ ] Controllers are thin; no business logic or DbContext outside service/repository.
- [ ] No entities in responses; DTOs validated; lists paged.
- [ ] Every endpoint has explicit authorization; students see only their own data.
- [ ] Errors go through the global handler; no swallowed exceptions.
- [ ] Migration present and named; constraints and indexes set; no N+1; money is decimal.
Frontend (frontend-ui)
- [ ] Only allowed libraries; shared components reused, not duplicated.
- [ ] Loading, empty and error states; works at 375/768/1280px; Vietnamese copy and formats.
- [ ] Design tokens used; no "AI template" patterns.
Structure (project-structure)
- [ ] Files in the right place, one responsibility each, no dead code or stray files.
Security
- [ ] No secrets committed; no SQL built from strings; no sensitive data in logs.
Owner understanding
- [ ] docs/notes file exists, matches the real code flow, max 15 lines.
- [ ] Nothing is so clever the owner could not explain it in an oral exam; flag anything that is.
Docs
- [ ] docs/api.md and plan.md status updated.
