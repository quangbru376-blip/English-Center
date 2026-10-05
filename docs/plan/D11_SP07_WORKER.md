# D11 / SP07 — usp_AllocateInvoiceNumber

Read COMPLETE this card, .agents/skills/backend-clean-code/SKILL.md and docs/plan/SKILL_ROUTING.md first. Read ONLY these3paths and exact output if it exists. Do not follow links/read AGENTS/other source/memory. No commands/search/scans/network/SQL/Git/MCP/browser/subagents/secrets. Only view_file/write_to_file/replace_file_content. Codex independently tests/reviews/applies. SQLServer160/exact decimal/time/NULL/full-set/current role/privacy E03/E04 override stale SQLite/API/seed rules. No packages/grants/GO/USE/migration/GETDATE or security/profile outputs. One CREATE OR ALTER PROCEDURE; SET NOCOUNT ON; TRY...CATCH THROW. Explicit projections, typed NULL, bigint counts and checked bigint offset=(CONVERT(bigint,@page)-1)*@pageSize. Values parameters only, literal contains LIKE escape '~' then '%' '_' '[' (REPLACE nested), no user SQL/sort concatenation. Null pair bounds follow canonical. No business writes except SP07 counter. No silent truncation, fake0, guessed seed, audit or replay.

Manager SQL integration choices: SP01–06 REQUIRE existing caller transaction with isolation_level=5 (SNAPSHOT) and XACT_STATE()=1. If absent/wrong isolation THROW51104. Never begin/commit/rollback caller transaction. This preserves service-owned transaction plus same-snapshot parent EF reads/options. Read SPs may use table variables/temp staging only, no business DML. Error51101 unauthorized active role/restricted account;51102 scope miss (parent absent/Teacher not assigned/no visible grade report) maps404 in future service;51103 invalid filters/page/date/sort; SP07 absent transaction51107/year/overflow51108. TRY/CATCH THROW without own rollback. Current account active AND must_change_password=0 for role defenses; role_code exact accepted case. SP04–06 no actor parameter or web auth claim (service future).

Exact output: backend/src/EnglishCenter.Api/Infrastructure/Sql/D11/SP07.sql

Accepted exact signature/result contract:
dbo.usp_AllocateInvoiceNumber → API-041/TX04

Parameter issueYear smallint N, server VN year1..9999 consistent issuedAt.
Requires caller TX; absent TX controlled failure, no independent BEGIN/COMMIT.
RS1 exactly one row invoiceNumber E03(T20.invoice_number) N varchar format
HD-YYYY-NNNNNNNN; counter T24 locked row/range, checked8digit overflow failure.
No invoiceId/accountId/replay key/customer/money inputs. Allocation +Invoice+
ADMITTED+audit+completion same TX, duplicate replay never invokes SP07.
Return new row only after caller commit to client, gaps allowed/no reuse.


Manager integration requirements:
Require XACT_STATE()=1 and@@TRANCOUNT>0, no isolation requirement. @issueYear smallint range1..9999; counter use SELECT last_value WITH(UPDLOCK,HOLDLOCK) by PK year covering missing-range. Existinglast>=99999999 THROW51108 BEFORE update; missing insertlast1 otherwise updateold+1. Single result invoiceNumber varchar16 exactly HD-YYYY-NNNNNNNN (pad year4/counter8). Row/range remains locked untilcaller commit/rollback. No ownTX or commit/savepoint/rollback, noInvoice/Registration/Audit/replay changes. Failures THROW with unchangedcallerownership. Zero new deps/sequence/SQLobjects. SET XACT_ABORT ON acceptable no rollback claim; managertests failurepath/rollback/counterexisting andmissingrange race.


Exact initial schema:

CREATE TABLE [dbo].[InvoiceNumberCounter] (
    [issue_year] smallint NOT NULL,
    [last_value] int NOT NULL DEFAULT 0,
    [row_version] rowversion NOT NULL,
    CONSTRAINT [PK_InvoiceNumberCounter] PRIMARY KEY ([issue_year]),
    CONSTRAINT [CK_InvoiceNumberCounter_Value] CHECK ([last_value] BETWEEN 0 AND 99999999),
    CONSTRAINT [CK_InvoiceNumberCounter_Year] CHECK ([issue_year] BETWEEN 1 AND 9999)
);