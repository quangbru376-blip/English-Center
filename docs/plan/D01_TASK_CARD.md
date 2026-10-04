# D01 — Role/Account/StudentProfile/AuthSession EF mapping

Third family in owner-authorized P07→P08→D01 batch2026-10-04. ADR-163 autoapply
Codex PASS. D02–D13, migrations/DB provisioning/SQL execution/seed/auth/query
services/endpoints/readiness/deployment outside scope. Metadata/SQL generation
checks establish mapping, not real-engine enforcement; D07+integration remain.
implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: accepted .NET10/EF10/SQL2025Express compatibility160;
no SQLite or migrations for this mapping-only task, no entities in API. Necessary
provider dependency pin is within accepted stack/ADR119 patch-selection authority.
read_context: AGENTS, this card, routing, complete backend skill; own explicit
prior D01 source when Part B. No links/scans/commands/network/MCP/subagents/secrets.

## P01 dependency prerequisite

Codex pins Microsoft.EntityFrameworkCore.SqlServer10.0.4 (MIT), matching installed
ASP.NET10.0.4/.NET10, from nuget.org official registry. No Tools/Design/InMemory/
SQLite/lazy loading packages. Lock full graph/content hashes, check licenses and
known vulnerabilities before use; no automatic retry execution strategy.
Source https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer/10.0.4.
Verified prerequisite8510b82: API26packages, full graph74unique packages
(MIT64/Apache2.0 nine/SqlClient SNI6.0.2 Microsoft license one). Complete SNI
license read and retained in D01-evidence; dev/test permitted, distribution
terms belong to future release review. Lock/assets/cache normalized hashes agree;
signed archive hashes are distinct by design. NuGet known vulnerabilities0 for
both projects. Locked restore/build0warnings0errors/376PASS before mappings.
Schema dbo, default Latin1_General_100_CI_AS_SC, email case-insensitive explicitly.
SQL constraints below use bracket identifiers and BIN2 phone-digit check so
non-ASCII numerals cannot satisfy ranges. No generated DDL is executed.

## Part A — Entities/context/Role mapping,6production files/330lines

Codex writes RoleMappingTests.cs independently (1file/150lines).
Enum-literal CHECK comparisons use BIN2 to enforce exact accepted role/status/
scope spellings while email unique comparison remains CI.

Domain/Identity/{Role,Account,StudentProfile,AuthSession}.cs,
Infrastructure/EnglishCenterDbContext.cs, Infrastructure/Configurations/RoleConfiguration.cs,
and matching tests Infrastructure/RoleMappingTests.cs.
Entities sealed plain classes, no annotations/business logic/DTO/credential
logging or overrides that print data. Integer keys, byte[] RV/Sid, DateOnly DOB,
DateTime UTC event columns; optional nullable fields exactly dictionary. RoleCode,
AccountStatus,SessionScope strings use accepted literals; no dynamic roles.
Properties names PascalCase corresponding SQL names below. Required strings
initialized string.Empty; RV/Sid Array.Empty<byte>(); Role nav Account.Role,
StudentProfile.Account,AuthSession.Account; Account.StudentProfile optional.
No account collections needed. Context constructor DbContextOptions only,
DbSets4entities and explicit configuration calls; Part A explicitly configure
Role and ignore other3entities until Part B adds exact mappings, avoiding EF
convention tables/shadow FK. Default schema/collation, no DI or connection in
Program. Configuration separate class IEntityTypeConfiguration<Role>.
Role: role_id int identityPK; role_code varchar32 required unique named
UQ_Role_Code; role_name nvarchar200 required; CHECK code in exact7 Student,
Admissions,AcademicAffairs,Teacher,Accountant,Director,Admin. No role seeds untilA02.
Test real SqlServer provider compatibility160, disconnected inert test connection
string; inspect model/DDL and no generated Account/Profile/Session yet. No DB IO.

## Part B — Three mappings/context,4production files/350lines

Codex writes IdentityMappingTests.cs independently (1file/350lines).

Infrastructure/Configurations/{Account,StudentProfile,AuthSession}Configuration.cs,
existing DbContext and tests Infrastructure/IdentityMappingTests.cs. Read Part A
entities/context/Role configuration only in addition to mandatory context.
Remove ignores; explicit mappings, no shadow columns/unexpected cascade.

Account: account_id int identityPK; role_id int required FK Role NO ACTION;
email varchar254 N unique UQ_Account_Email CI collation; phone varchar10 Y
filtered unique UQ_Account_Phone WHERE [phone] IS NOT NULL; password_hash varchar512 N;
full_name nvarchar200 N; status varchar16 N default ACTIVE; created_at datetime2(3) N
default SYSUTCDATETIME(); security_version bigint N default1; failed_login_count int
N default0; failure_window_started_at/login_throttled_until datetime2(3) Y;
must_change_password bit N explicit value(no guessed default); temp_issued_at/
temp_expires_at datetime2(3) Y; row_version SQL rowversion N IsRowVersion.
CHECK status ACTIVE/LOCKED/DISABLED; version>0; counter>=0; temp bundle true both
nonnull AND expiry=DATEADD(hour,24,issued), false bothNULL (avoid UNKNOWN loophole);
phone NULL OR DATALENGTH=10 AND first0 AND NOT LIKE '%[^0-9]%' COLLATE BIN2.
Index IX_Account_Role_Status_Id(role_id,status,account_id). No login SQL/services.

StudentProfile: student_id int identityPK; account_id int N FK Account NO ACTION,
unique UQ_StudentProfile_Account; date_of_birth date N; address nvarchar510 Y;
row_version RV N. Account.StudentProfile optional one-to-one. No DOB-current-date
or Student role SQL check (service constraints future). No profile/name snapshots.

AuthSession: sid binary32 N PK no generated GUID; account_id int N FK Account
NO ACTION; issued_at datetime2(3) N default SYSUTCDATETIME(); expires_at and
last_activity_at datetime2(3) N require explicit service values; revoked_at sameY;
security_version bigint N snapshot; session_scope varchar24 N NORMAL/PASSWORD_CHANGE;
row_version RV N. CHECK scope, issued<=activity<=expires AND expires>issued,
revoked NULL OR revoked>=issued. Index IX_AuthSession_Account_Revoked and
IX_AuthSession_Expires. Do not storeJWT/cookie/CSRF/password or extend expiry.

Use individual named CHECKs and exact SQL null handling. UTC read materialization
is a service/reader concern before real caller evidence; mappings are datetime2,
never datetimeoffset/DateTime.Now defaults. Future service guards enforce raw
canonical input, role/profile invariant and expiry/version/revocation atomically.
Tests independent expected column/type/null/default/index/FK/check signatures for
all4tables; RV generation/concurrency flags, DateOnly SQL date, SQL provider DDL,
noncascade relationships, Sid32 fixed length; no modelHasData, no extra table or
token fields. Tests do not claim real SQL constraint/locking/RV behavior.
skill_verification: complete Codex verification/source review; see checkpoint
for worker audit/rejections, source versions, tests and integration boundaries.
Status: complete within mapping scope; Codex review PASS applied/reverified main.

## Verified checkpoint2026-10-04

A runa4ddce79-515 SUCCESS/exit0/6files141runner lines/zero violations;
18tools12reads6writes, complete authorized contexts and selected skill/policy.
Codex independently added2Role metadata/DDL tests. Reviewed/applied210c5ad,
locked restore/build0warnings0errors/392PASS. Partial other3entities ignored
at that checkpoint; removed only when complete mappings were reviewed.

B run8e4ef2fe-9c7 runner SUCCESS/301lines/no write-cap violations but **manager
REJECTED**:20read attempts outside allowlist across13distinct documentation/test
paths, including one missing path.40tools36reads4writes; no commands/network/
secret read observed. Runner receipt checks mutations, not enforceable read
sandbox. Worker output never applied. Manager attempted a scoped stop after
detecting overreads; run had already exited, no process was killed.

Codex independently implemented B in fresh detached
C:/Users/asus/AppData/Local/Temp/codex-d01-manager/identity-mapping fromc24f543;
no copied failed-worker source. Four production mappings/context and two test
files changed259insertions/5deletions. Reviewed/appliede7ca9b4/400PASS. Final
review fixed trailing-space enum comparison: BIN2 alone still follows SQL string
padding; add DATALENGTH=LEN only for these ASCII literal sets. Existing Role
configuration and matching metadata tests included in this bounded repair.
[Microsoft string comparison](https://learn.microsoft.com/en-us/sql/t-sql/language-elements/string-comparison-assignment)
supports the padding behavior; real-engine negative cases remain D07 integration.
Reviewedc40a378/mainf6fd902, locked restore/build0warnings0errors/400PASS/0fail/0skip.

Final model exactly4tables/33columns/4PK/3NO ACTION FK/7indexes/9CHECKs, no shadow
columns/seeds/token storage; all3RV properties generated/concurrency/ignored-on-save.
Identity keys and32byte binary Sid map correctly; optional phone filtered unique,
email CI, UTF16 capacities/date/datetime2(3)/hash512/defaults match accepted E03.
Status empty CLR sentinel allows server ACTIVE default without inserting empty
status. Context is not registered against an invented DB/profile in Program.
SQL-provider metadata and DDL generation performed disconnected; no DB provision,
SQL execution/migration/locking/RV runtime/auth readiness claim. All such later
integration checks are explicitly open. No D02 started.
skill_verification: complete worker context/policy audit, manager independent
source review and checks PASS. Original successful/rejected/manager trees retained.
