# D06 — Invoice, Payment, AuditLog, ReplayOperation and InvoiceNumberCounter mapping

Authorized continuation2026-10-05 after D05 PASS; Codex manager/tests/review.
Bounded AGY fragments Finance(Invoice/Payment), AuditReplay(AuditLog/ReplayOperation),
Counter(InvoiceNumberCounter). Apply final PASS ADR-163. Mapping only; no D07/
SQL execution/migrations/triggers/seeds/packages/services/API/profile/DI/cleanup/
installation/push/deploy. Full P01/M0/V01 and SQL/TX/runtime readiness incomplete.
implementation_skills: backend-clean-code; review_skills: code-review.
skill_sources: backend726586298710499CD9E5CE7A1E3A2A9FCFD00C07C1F73DFCE34CF58E7DA8EE3B;
review04B72A9627A717BBDF017A3275974B62BBE41F23440E86E5341954E8863A7E70;
routingADB7938FE69E00C991AEFB4EB114854F124EB3E2AC3963763937225646D3E915;
AGY EB6DC1064B48EC44754406EE804CF287BE7B3B9243E2D5C80FDACBDD3364C54B.
applicable_overrides: EF10.0.4/SQLServer160 pinned; E03 T20–T24/ADR136 and E04
Accepted ADR160. No SQLite/migration for disconnected mapping, no JWT bearer
change. Exact money/event/business-date/null contracts; explicit FK indexes ADR164.

## read_context / worker authority

Codex read ordered AGENTS/Project Memory, requirements, latest handoffs/M0,
DATABASE_SPEC T20–T24/check/index sections and E04 exact replay retention.
Worker must VIEW COMPLETE this card, COMPLETE backend skill and COMPLETE routing
before writing. Read ONLY these three context files and own allowed output paths
if present. No AGENTS/memory/links/other source/tests/search/list/scans/commands/
Git/network/browser/MCP/subagents/secrets. Only view_file and allowed file writes.
Stop if blocked; skills grant no extra authority. Finance4files/cap650lines,
AuditReplay4files/cap650lines, Counter2files/cap160lines. No read/write cross-fragment.
Codex alone integrates5DbSets/configs, writes FinanceInfrastructureMappingTests.cs,
adapts D05 whole-model counts to prior19-table subset. FK coverage stays global.

## Common physical rules

Domain.Finance: Invoice, Payment, InvoiceNumberCounter; Domain.Administration:
AuditLog, ReplayOperation. Config namespace EnglishCenter.Api.Infrastructure.Configurations.
Exact singular dbo tables. Plain sealed publicgetset entities, required strings
string.Empty, optional strings/actors/events/Guid nullable; byte arrays [];
nullable navs for loadedness. No collections/additional state/shadow/computed fields.
Four new int PK IDENTITY(1,1); Counter key is smallint NOT identity. All scalar FK
NO ACTION explicit requiredness. Money CLR decimal SQL decimal(18,0) precision18,0.
UTC event DateTime datetime2(3) HasPrecision(3); business DateOnly date; operations
Guid uniqueidentifier, no default/generator. Explicit lengths/types/Unicode.
RowVersion byte[] rowversion N IsRowVersion only Invoice/Payment/Counter, NOT
append-only AuditLog/ReplayOperation. No defaults except Invoice.status UNPAID
HasSentinel(string.Empty) and Counter.last_value0. No clock/year/money defaults.
Inherited CI_AS_SC; exact enums BIN2 Latin1_General_100_BIN2 plus DATALENGTH=LEN.
All mandatory bundle members explicitly IS NOT NULL; NULL branches explicit.
String Unicode max lengths represent UTF16 storage; codepoint/NFC validators later.
Parents without source reads: Domain.Admissions.Registration.RegistrationId int;
Domain.Identity.Account.AccountId int. Invoice.InvoiceId int supplied in Finance.

## Invoice —13columns

InvoiceId/invoice_id int N PK; InvoiceNumber/invoice_number varchar(16) N;
RegistrationId/registration_id int N; Amount/amount decimal(18,0) N;
IssuedAt/issued_at datetime2(3) N; DueDate/due_date date N DateOnly;
Status/status varchar(16) N default UNPAID/sentinelEmpty;
CancelledBy/cancelled_by int Y; CancelledAt/cancelled_at datetime2(3) Y;
CancellationReason/cancellation_reason nvarchar(2000) Y;
CancelledFromStatus/cancelled_from_status varchar(16) Y;
CancellationOperationId/cancellation_operation_id uniqueidentifier Y;
RowVersion/row_version rowversion N. Navs Registration, CancelledByAccount(Account).
FK_Invoice_Registration_registration_id required UNIQUE one-to-one WithOne()
no inverse; FK_Invoice_Account_cancelled_by optional many; both NO ACTION.
UQ_Invoice_Number(invoice_number) unique unfiltered;
UQ_Invoice_Registration(registration_id) unique unfiltered;
IX_Invoice_StateDue(status,due_date,invoice_id);
IX_Invoice_Issued(issued_at,invoice_id); IX_Invoice_CancelledBy(cancelled_by),
all3nonunique unfiltered. Four CHECKs:
CK_Invoice_Amount [amount] > 0;
CK_Invoice_Status exact UNPAID/PAID/CANCELLED BIN2/padding;
CK_Invoice_NumberPattern DATALENGTH(invoice_number)=16 AND DATALENGTH=LEN AND
invoice_number COLLATE Latin1_General_100_BIN2 LIKE
'HD-[0-9][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]';
CK_Invoice_CancelledBundle iff CANCELLED cancelled_by/cancelled_at/
cancelled_from_status/cancellation_operation_id ALL NOT NULL; prior exact UNPAID
BIN2/padding. cancellation_reason OPTIONAL in cancelled branch (workflow-specific
parent reason); all5including reason NULL in other states. No OVERDUE flag/state.
Due VN issue+7, number year/sequence range, immutable snapshot/link/dates, payment
exists iff PAID, admission atomicity/role/clock and cancellation remain later TX.

## Payment —11columns

PaymentId/payment_id int N PK; InvoiceId/invoice_id int N;
AmountPaid/amount_paid decimal(18,0) N; Method/method varchar(16) N no default;
TransactionReference/transaction_reference nvarchar(200) Y;
ConfirmedBy/confirmed_by int N; PaidAt/paid_at datetime2(3) N;
LastCorrectedBy/last_corrected_by int Y; LastCorrectedAt/last_corrected_at datetime2(3) Y;
LastCorrectionReason/last_correction_reason nvarchar(2000) Y;
RowVersion/row_version rowversion N. Navs Invoice, ConfirmedByAccount(Account),
LastCorrectedByAccount(Account). FK_Payment_Invoice_invoice_id required UNIQUE
one-to-one WithOne() no inverse; FK_Payment_Account_confirmed_by required many;
FK_Payment_Account_last_corrected_by optional many; all NO ACTION.
UQ_Payment_Invoice(invoice_id) unique unfiltered; IX_Payment_Paid(paid_at,payment_id),
IX_Payment_ConfirmedBy(confirmed_by), IX_Payment_LastCorrectedBy(last_corrected_by)
nonunique unfiltered. Reference NOT unique. Four CHECKs:
CK_Payment_Amount [amount_paid] > 0;
CK_Payment_Method exact CASH/TRANSFER BIN2/padding;
CK_Payment_Reference method <> TRANSFER OR (transaction_reference IS NOT NULL AND
LEN(transaction_reference)>0), do not require NULL reference for CASH;
CK_Payment_CorrectionBundle actor/time/reason ALL NULL OR ALL NOT NULL and LEN(reason)>0.
Equality Invoice.amount, metadata-only correction/immutability, PAID atomicity,
audit/roles/time/replay remain later service/TX. No receipt date/refund/bank fields.

## AuditLog —13columns, append-only NO rowversion

AuditId/audit_id int N PK; ActorAccountId/actor_account_id int Y;
Action/action varchar(80) N; EntityType/entity_type varchar(64) N;
EntityId/entity_id varchar(100) Y; Result/result varchar(16) N;
Details/details nvarchar(max) Y plain string? Unicode; OccurredAt/occurred_at datetime2(3) N;
IpAddress/ip_address varchar(45) Y; OperationId/operation_id uniqueidentifier N;
AuditCategory/audit_category varchar(16) N;
SecurityExpiresAt/security_expires_at datetime2(3) Y; IpExpiresAt/ip_expires_at datetime2(3) Y.
Nav ActorAccount(Account). FK_AuditLog_Account_actor_account_id optional many NO ACTION.
Four indexes: IX_Audit_Time(occurred_at DESC,audit_id DESC);
IX_Audit_Operation(operation_id,audit_id) ASC;
IX_Audit_ActorTime(actor_account_id ASC,occurred_at DESC,audit_id DESC);
IX_Audit_EntityTime(entity_type ASC,entity_id ASC,occurred_at DESC,audit_id DESC).
No additional expiry/action/result indexes or audit trigger. Five CHECKs:
CK_AuditLog_Result exact SUCCESS/FAILURE BIN2/padding;
CK_AuditLog_Category exact MANDATORY/SECURITY BIN2/padding;
CK_AuditLog_Details details IS NULL OR ISJSON(details)=1;
CK_AuditLog_SecurityExpiry (category SECURITY AND security_expires_at IS NOT NULL
AND security_expires_at >= DATEADD(day,90,occurred_at)) OR (category MANDATORY
AND security_expires_at IS NULL). Do not expire mandatory rows.
CK_AuditLog_IPBundle (ip_address IS NULL AND ip_expires_at IS NULL) OR
(ip_address IS NOT NULL AND ip_expires_at IS NOT NULL AND
ip_expires_at = DATEADD(day,90,occurred_at)). Mandatory rows may have expiring IP.
Authenticated actor/allowlist/secret-safe JSON/IP trust/append-only grants/cleanup
and retention clock source remain future service/SQL gates, no purge implemented.

## ReplayOperation —10columns, completed append-only NO rowversion

ReplayId/replay_id int N PK; ActorAccountId/actor_account_id int N;
ActionCode/action_code varchar(80) N; RequestId/request_id uniqueidentifier N;
InputDigest/input_digest binary(32) N byte[]=[] HasMaxLength(32)/IsFixedLength();
OperationId/operation_id uniqueidentifier N; ResultMode/result_mode varchar(32) N;
ResultReference/result_reference nvarchar(max) N plain string=string.Empty Unicode;
CompletedAt/completed_at datetime2(3) N; ExpiresAt/expires_at datetime2(3) N.
Nav ActorAccount(Account); FK_ReplayOperation_Account_actor_account_id required many NO ACTION.
UQ_Replay_ActorActionRequest(actor_account_id,action_code,request_id) unique unfiltered;
IX_Replay_Expiry(expires_at), IX_Replay_Operation(operation_id) nonunique unfiltered.
Three CHECKs: CK_ReplayOperation_Mode exact SAFE_REFERENCE/SECRET_RESULT_UNAVAILABLE
BIN2/padding; CK_ReplayOperation_ResultReference ISJSON(result_reference)=1;
CK_ReplayOperation_Expiry expires_at >= DATEADD(hour,24,completed_at).
E03 minimum>=24h CHECK retained; E04 exact24h is future service configuration.
No native JSON mapping, secret/raw response/PII snapshot store. SHA256 C1 bytes,
JSON reference schema/allowlist, request locks/digest comparison/expired-key409,
no overwrite/current rights/replay completion atomicity/cleanup grants later.

## InvoiceNumberCounter —3columns

IssueYear/issue_year short smallint N PK ValueGeneratedNever() (NO identity/default);
LastValue/last_value int N default0; RowVersion/row_version rowversion N.
No nav/FK/index beyond PK/seed. CK_InvoiceNumberCounter_Year issue_year BETWEEN1AND9999;
CK_InvoiceNumberCounter_Value last_value BETWEEN0AND99999999. No allocator/reset API.
Year VN, serialize absent-year row/increment/overflow/rollback/non-reuse and SP07
remain later SQL/TX. Counter.initial0 does not generate invoice numbers here.

## Codex verification / skill_verification

Pending physical signature/identity-vs-year/RV/defaults/decimal18/date/nullable
bundles/JSON/binary32/retention/enum/numberpattern/index directions/FK checks.
Expected adds50columns/7FK(4required,3optional)/16IX/18CHECK/3RV; full24tables/
230columns/24PK/2AK/50FK/78IX/70CHECK/20RV. D01–D05 signatures unchanged.
Locked restore/build/full tests manager then main, exact backend tree equality,
locks unchanged, generated DDL/closed connection only; no live SQL evidence.
D07 not started, requires separately authorized migration/isolated SQL execution.
