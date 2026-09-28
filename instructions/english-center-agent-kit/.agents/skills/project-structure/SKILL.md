---
name: project-structure
description: Folder and file layout rules for the English Center system. Use whenever creating, moving, renaming or deleting files or folders, scaffolding the solution, adding a new module, or when the repo feels messy. Covers backend, frontend and docs.
---
# Project structure

Every folder has one purpose. Group by business module first, technical layer second.

## Layout
```text
/
  AGENTS.md
  .agents/skills/
  docs/              requirements.md, plan/, notes/, api.md, reference/ (old SQL script)
  backend/
    src/EnglishCenter.Api/
      Modules/<Module>/        one folder per business module
        <Name>Controller.cs
        <Name>Service.cs, I<Name>Service.cs
        <Name>Repository.cs, I<Name>Repository.cs
        Dtos/
      Domain/                  entities and enums (shared)
      Infrastructure/          AppDbContext, Configurations/, Migrations/, Auth/
      Common/                  error middleware, exceptions, paging and response types
      Program.cs
    tests/EnglishCenter.Tests/ mirrors Modules/
  frontend/
    src/
      app/                     router, layout shell, providers
      features/<module>/       pages/, components/, api.ts, types.ts
      components/ui/           shared building blocks (Button, Input, Table, Modal...)
      lib/                     http client, formatters, auth helpers
      styles/                  tokens and global css
```

Modules follow the departments in requirements.md: Students, Admissions, Academics (courses, classes, schedule, teachers, attendance, grades), Finance (tuition, invoices, payments), Reports (board), Administration (users, roles). Rename or add modules only through plan.md.

## Rules
1. One file, one responsibility. Past about 200 lines, split by responsibility.
2. No dumping-ground folders (`Utils`, `Helpers`, `Misc`). `Common/` and `lib/` hold only what 3+ modules use.
3. Before creating a file, check whether an existing file already owns that job.
4. Naming: C# files PascalCase matching the type name; React components PascalCase.tsx; other frontend files kebab-case; URLs plural kebab-case nouns.
5. A module never uses another module's Repository. Cross-module needs go through that module's Service.
6. Frontend features never import from other features. Shared code goes to components/ui or lib.
7. No dead code, commented-out code, `old`/`copy`/`final` files, temp test files or unused packages. Delete what your change orphans.
8. Config: appsettings.json (non-secret) plus user-secrets or .env (secret). Commit `.env.example`, never real secrets. Ignore bin, obj, node_modules, dist.
9. When a task adds files, list them at the end with a one-line purpose each.
