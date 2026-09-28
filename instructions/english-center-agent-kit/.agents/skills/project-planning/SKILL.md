---
name: project-planning
description: Plan and split work for the English Center system. Use whenever starting a feature or phase, splitting screens F01-F33 into tasks, writing or updating anything in docs/plan/, or handing work from Codex (planner) to Antigravity (worker). Also use when a request is vague or spans more than one module.
---
# Project planning

Plans are files, not chat. A worker must be able to finish a slice using only the repo.

## Files
- docs/requirements.md: actors, use cases, screen list F01–F33 (owner maintains; do not rewrite).
- docs/plan/plan.md: the 4 phases; each lists slices with status (todo / doing / review / done).
- docs/plan/slices/<id>-<name>.md: one file per slice, from templates/slice-plan.md.
- docs/plan/questions.md: open questions from workers; the planner answers, then removes them.
- docs/notes/<id>-<name>.md: short post-slice explanation for the owner's oral exam.

## Planner rules (Codex)
1. Cut work into vertical slices: one business capability from database to API to UI, small enough for one worker session (about 1–3 screens). Never plan "all backend first".
2. Order inside a slice: entity + migration, service + business rules, endpoint + DTOs, tests, UI page, connect, verify.
3. Slice 0 is a pilot: login + role-based access + Student list/create. Run it through the full plan, build, review loop before planning the rest. Record token cost and rework in plan.md.
4. Each slice file names the exact files to create or change, the API contract (method, route, roles, request, response, errors), the business rules that apply, and acceptance checks that can be run.
5. Do not add screens or features outside requirements.md. If something looks missing, add it to questions.md for the owner.
6. Never let two agents edit the same module at the same time. Slices in progress must not overlap.
7. Keep plan.md status current whenever a slice changes state.

## Worker rules (Antigravity)
1. Read AGENTS.md, the slice file, and the skills it names. Do only that slice.
2. If anything needed is missing or contradictory, stop and log it in questions.md.
3. When finished: run the acceptance checks, write the docs/notes file, set status to review, and list the changed files.

Read templates/slice-plan.md when writing a new slice file.
