---
name: backend-clean-code
description: Backend rules for the English Center system (ASP.NET Core, EF Core, SQL Server). Use for ANY change to controllers, services, repositories, DTOs, entities, migrations, authentication, authorization, validation, error handling or API docs, even small endpoint or query changes.
---
# Backend clean code

Layers, one direction only: Controller → Service → Repository → DbContext.

## Layer duties
- Controller: bind the request, call ONE service method, return the result. No business rules, no DbContext. Aim for under 15 lines per action.
- Service: all business rules and multi-step workflows (for example enroll student + create invoice) in one transaction. Throw typed exceptions for rule violations.
- Repository: EF Core queries and saves only. Not generic: one thin repository per module with intention-revealing methods (GetActiveByClassAsync). No business decisions.
- Entities never leave the API. Requests and responses are DTOs, mapped by hand in small mapping methods. No AutoMapper, no MediatR, no generic repository. Ask before adding any package.

## API
1. REST: plural kebab-case nouns, correct verbs and status codes (200/201/204/400/401/403/404/409).
2. Every list endpoint is paged (page, pageSize with a maximum) and returns items + total.
3. Validate shape at the edge with DataAnnotations on DTOs; business validation lives in services.
4. One global exception middleware returns ProblemDetails with a stable `code` and a Vietnamese `message`. Never leak stack traces or SQL errors.
5. Any route or DTO change updates docs/api.md and Swagger annotations in the same task.

## Auth and security
1. JWT bearer. Roles: Student, Admissions, AcademicAffairs, Teacher, Accountant, Director, Admin (see the permission matrix in requirements.md). Use named policies, not scattered role strings.
2. Every endpoint has an explicit `[Authorize(...)]` or `[AllowAnonymous]`. Default is deny.
3. Hash passwords with ASP.NET Core `PasswordHasher` (or BCrypt). Never log or store tokens or secrets.
4. A student may only read their own data: enforce in the service using the id from the token, never an id from the request body.
5. CORS allows only the frontend origin. Only parameterised queries (LINQ); never build SQL from strings.
6. Audit logging is application-layer: services call a log service for sensitive actions (payments, grades, role changes).

## Database
1. EF Core code-first. Every schema change is a migration with a meaningful name. Never hand-edit the database or an applied migration.
2. Enforce constraints in the database too: keys, foreign keys, unique indexes, required columns, check constraints. Configure in Infrastructure/Configurations/<Entity>Configuration.cs, not with attributes on entities.
3. Index columns used for lookup and filtering (student code, class id + date).
4. Money is `decimal(18,0)` in VND, never float. Store times in UTC.
5. Reads use `AsNoTracking()` and project to DTOs with `Select`. No lazy loading; avoid N+1 (Include or projection on purpose).
6. Seed only roles, one admin account and reference data, in a dedicated seeding class.

## Code style
- async all the way; pass CancellationToken from controller down.
- Short single-purpose methods; names say what, not how. Nullable reference types on.
- Comments explain WHY only; rewrite unclear code instead of commenting it.
- Enums and constants instead of magic strings and numbers.

## Tests
Each business rule in AGENTS.md and each service workflow gets an xUnit test: one success path and one failure path. Test services against the real DbContext on SQLite in-memory, and say so when a behaviour differs from SQL Server. Do not unit-test controllers.
