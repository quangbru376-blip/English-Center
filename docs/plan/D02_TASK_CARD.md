# D02 — Course/Band/TrainingProgram/ScoreComponentConfig mapping

Authorized continuation2026-10-05 after D01 PASS. Codex manager; bounded AGY
implementation per object, Codex independent tests/review; apply only PASS under
ADR-163. Mapping only: no D03, migrations, SQL execution, seed, new packages,
DI/connection/profile, routes, services, triggers, installation, push/deployment.

implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: accepted EF SQLServer10.0.4/compatibility160 already pinned;
no SQLite evidence, no migration in mapping-only stage, exact decimal types,
no entities in API. Existing JWT-cookie/full-set/error overrides still apply.

## Worker authority and read_context

Manager read AGENTS and Project Memory in mandated order, requirements and
accepted DATABASE_SPEC T05–T08/sections4,8,11, ADR079 and plan. This card is a
self-contained manager handoff of those sources. Worker MUST view this COMPLETE
card, COMPLETE backend SKILL.md and COMPLETE SKILL_ROUTING.md before edits.
Worker receives ONLY those three files, plus its two allowed production output
paths if they exist. Do NOT read AGENTS, other Project Memory, baseline, links,
directory lists, tests, scripts, packages, user profiles or secrets. Manager
performed repository preflight; this bounded delegation does not authorize
repeating it. Only view_file and write/replace allowed files; no commands,
network/browser/MCP/search/subagents/Git/tests. Stop instead of broadening scope.

Each object is a separate contract, two files, maximum220 diff lines. Worker
writes Domain/Academics/<Entity>.cs and Infrastructure/Configurations/<Entity>Configuration.cs.
Codex alone edits existing EnglishCenterDbContext.cs to register four DbSets and
four configurations, adds AcademicsMappingTests.cs, and adapts D01 whole-model
assertions to inspect its four-table subset, retaining all D01 contracts.
No other production writes. Preserve style: sealed plain entities, public get/set,
strings string.Empty, byte[] RowVersion=[], nullable reference navigations for
loadedness even though int FK required. No collection navigation is needed.

## Exact physical contract

All tables dbo, model collation Latin1_General_100_CI_AS_SC. Every column NOT NULL.
Business IDs int IDENTITY(1,1) PK. Each row_version rowversion, IsRowVersion(),
generated/concurrency; no shadow/extra fields, seed, frozen flag or version table.
No default for values unless specified; CLR initialization is not SQL default.
All relationships required, many-to-one NO ACTION; name FK explicitly as below.
Use explicit column names/types/lengths/Unicode and decimal precision/scale.

Course: CourseId/course_id int PK; CourseCode/course_code varchar(30);
CourseName/course_name nvarchar(300); BasePrice/base_price decimal(18,0);
Status/status varchar(16) EXPLICIT (no DB default); RowVersion/row_version.
UQ_Course_Code(course_code); IX_Course_Status_Id(status,course_id).
CK_Course_Price: [base_price] > 0.
CK_Course_Status: [status] COLLATE Latin1_General_100_BIN2 IN ('ACTIVE','INACTIVE')
AND DATALENGTH([status]) = LEN([status]).
CK_Course_Code: DATALENGTH([course_code]) BETWEEN 1 AND 30 AND
[course_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9_-]%'.
No name unique, code editing/price snapshots handled by future service.

Band: BandId/band_id int PK; BandCode/band_code varchar(30);
MinScore/min_score decimal(5,2); RowVersion/row_version.
UQ_Band_Code(band_code); UQ_Band_MinScore(min_score).
CK_Band_MinScore: [min_score] >= 0 AND [min_score] <= 100.
CK_Band_Code: DATALENGTH([band_code]) BETWEEN 1 AND 30 AND
[band_code] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^ -~]%'
AND LEN([band_code]) > 0.
Technical code uses nonempty printable ASCII; no invented uppercase Course
pattern on Band. Seeds0/25/45/65/85 and freeze/zero-coverage later, not this task.

TrainingProgram: ProgramId/program_id int PK; CourseId/course_id int;
BandId/band_id int; ProgramName/program_name nvarchar(300);
Status/status varchar(16) SQL default DRAFT with HasSentinel(string.Empty);
ExpectedLessonCount/expected_lesson_count int; RowVersion/row_version.
Nullable navigation Course? Course and Band? Band (integer FK still required).
FK_TrainingProgram_Course_course_id and FK_TrainingProgram_Band_band_id NO ACTION.
UQ_Program_CourseBand(course_id,band_id) unconditional incl INACTIVE;
IX_Program_Band(band_id); IX_Program_Status_Id(status,program_id).
CK_TrainingProgram_Status: [status] COLLATE Latin1_General_100_BIN2 IN
('DRAFT','ACTIVE','INACTIVE') AND DATALENGTH([status]) = LEN([status]).
CK_TrainingProgram_ExpectedLessons: [expected_lesson_count] > 0.
No default count, global name unique, frozen flag, cross-table weight-sum CHECK.

ScoreComponentConfig: ComponentId/component_id int PK; ProgramId/program_id int;
ComponentName/component_name nvarchar(200) explicit Latin1_General_100_CI_AS_SC;
Weight/weight decimal(5,2); Kind/kind varchar(16); RowVersion/row_version.
Nullable TrainingProgram? Program navigation; required FK integer.
FK_ScoreComponentConfig_TrainingProgram_program_id NO ACTION.
UQ_Component_ProgramName(program_id,component_name);
UQ_Component_FinalExam(program_id) filter EXACT [kind] = 'FINAL_EXAM'.
These two indexes cover program FK: do not add redundant standalone IX.
CK_ScoreComponentConfig_Kind: [kind] COLLATE Latin1_General_100_BIN2 IN
('GENERAL','LESSON','FINAL_EXAM') AND DATALENGTH([kind]) = LEN([kind]).
CK_ScoreComponentConfig_Weight: [weight] > 0 AND [weight] <= 100.
CK_ScoreComponentConfig_Name: LEN([component_name]) > 0.
No weight/kind SQL default. Service validates NFC/codepoints/raw precision,
exactly one FINAL_EXAM and weight sum100 before ACTIVE; later triggers/TX
protect first-use freeze. Metadata does not prove those workflow invariants.

## Codex verification / skill_verification

Pending. Independently verify exact columns, types/nullability, ID/RV generation,
no shadow/seeds/defaults beyond DRAFT, exact indexes/filters/collation, required
FKs NO ACTION and disconnected SQLServer DDL; retain all D01 semantics. Test
literal case/padding guards and range predicates as metadata, not live SQL.
Full model expected8tables/56columns/7RV/6FK, no migrations/history/cascade.
Locked restore/build/tests in reviewed detached manager tree then main; package
locks and source tree equality. Audit worker receipts/tool events/read ranges.
Reject any unauthorized read or write; never apply rejected worker output.
