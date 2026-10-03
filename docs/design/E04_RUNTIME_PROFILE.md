# E04 — Hồ sơ config, readiness và vận hành có điều kiện

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Phiên bản E04-SC04-v1 trong E04-v1, 03/10/2026. **Accepted ADR-160 với environment deferred; chưa chạy.**
Nguồn [E04 mục18](SYSTEM_ARCHITECTURE_SPEC.md), ADR-114–120/154,
[SELF_HOSTED_DEMO](SELF_HOSTED_DEMO.md), [questions](../plan/questions.md).
D12/D13 vẫn owner-deferred. Bảng typed config là contract tài liệu, không appsettings,
.env, script startup, software installation hoặc implementation plan.
Owner sole operator ADR-120; không hỏi lại người hoặc mục đích chạy theo buổi.

[Final contract DF01–05](E04_FINAL_CONTRACT.md#7-deferred-register-và-acceptance-boundary)
phân biệt completed design contracts và environment/evidence prerequisites.
Response buffer phải hỗ trợ full CSV cap; missing required profile slots vẫn
fail closed, không chọn số timeout/body/weekly cap thay owner D12/D13.

## 1. Typed config — giá trị cố định và environment slots

N=phải có đúng kiểu; fixed=nguồn Accepted; slot=chưa giá trị, fail closed nếu
profile cần field mà chưa được cung cấp. Không fallback development secrets.
Không log config dump/connection string/keys/public claims; diagnostics chỉ key
name/error class/traceId secret-free. Profile type không là web role/new package.

| Config group / fields | Type / validation / nguồn |
| --- | --- |
| Profile.kind | enum LOCAL_DEV/LOCAL_DEMO/LAN_DEMO/INTERNET_DEMO, explicit N; chọn environment chưa làm D13 accepted. Không auto tunnel from profile. |
| Runtime.target | fixed .NET10/EF10/SQL2022 Developer non-production ADR-119; compatible patch/lock manifest slot sau baseline, không tự pin latest/cài package. |
| Business.timeZone | fixed Asia/Ho_Chi_Minh; OS resolver mapping tương đương UTC+7 phải verify, không silently host timezone. Event clock UTC milliseconds, local schedules minute. |
| Database.connection | secret reference N, catalog/server/auth configured slot; runtime principal ec_app_runtime only. No browser SQL/sa/db_owner, separate schema/maintenance principal. |
| Database.schemaCompatibility | expected E03/migration/object signature identity N slot khi có implementation; check24 tables/5TG/6VW/6FN/7SP/RV shapes, not count alone. Missing drift not auto migration/startup repair. |
| Database.snapshotRequired | fixed true for compound snapshots/report contracts; readiness checks prerequisite ALLOW_SNAPSHOT_ISOLATION, RCSI alone insufficient. Không tự ALTER DB. |
| Web.origin,listenEndpoint | URI/address+port N slot D13; same UI/API origin and relative /api/v1. No wildcard public bind or default arbitrary port. |
| Proxy.forwardingEnabled,knownProxies,knownNetworks | explicit bool +validated IP/CIDR sets slots D13; forwarded identity only from trusted hop. Empty set when direct profile, never trust all to repair HTTPS/IP. |
| Transport.https,cookiePolicy | valid profile transport certificate/trust slot; HttpOnly/Secure/SameSite+CSRF theo ADR-099, no disable Secure/Origin/CSRF for mobile/tunnel access. |
| Auth.jwtKeys,issuer,audience | cryptographic key secret refs +explicit strings slots, compatible rotation/issuer scope; lifetime cannot exceed server sessions; no hardcoded example keys. |
| Auth.csrfProtectionKeys | persisted protected key store/reference slot, server validated framework policy, RAM request token/header/refetch ADR-099/140. Không gửi key cho browser. |
| Auth.sessionTimers | fixed normal2h/idle30min; PASSWORD_CHANGE min10min,temp remaining; login throttle5 failures/15min/block15min; temp24h ADR-052/054/055/098. Auth/me/csrf/readiness/timers not idle renewal. |
| Password.blocklistManifest | paths/checksum/license/review-date N slot for approved offline sources ADR-114, encoding/validation errors fail startup; không external compromise API/claim exhaustive leaked coverage. |
| Replay.codec | Accepted EC-C1/actions frozen ADR-160, retention exact24h, cleanup interval slot operator. No secrets/result snapshots/version DB column. |
| Audit.retention | fixed security/IP90d, mandatory retained; cleanup principal/job slot, no runtime DML deletion or purge API. Mandatory failure rollback; pure security bounded fallback. |
| Limits.csv | fixed max10000 rows/max5MB/UTF8 BOM/comma/CRLF/formula safeguard ADR-101; exact encoded layout D10 draft, cap definition consistency checked before headers. |
| Deadlines.lockMs,sqlSeconds,requestMs,csvMs | positive bounded integers N slots D12/D13 profile; no zero/infinite defaults. lock budget<remaining SQL/request budget, CSV budget fits request; fail invalid ordering/overflow. Chưa số/thông lượng/SLO. |
| Limits.requestBodyBytes,responseBufferBytes,weeklyPreviewBudget | positive integers N environment slots; validation error semantics Accepted413/503 sanitized, no hidden business lesson/date cap. Concrete normal-use envelope phải owner rà nếu chặn accepted workflow. |
| Backup.destination,accessIdentity,encryptionKeyReference | non-primary storage URI/path+principal+secret ref slots before backup/recovery; key separate from backup bytes/repo. No guessed drive/provider/public bucket. |
| Backup.policy | fixed daily encrypted SQL,7daily/4weekly/monthly test restore; RPO24h/RTO2h targets ADR-118. Schedule time/media/capacity/missed-run slot; laptop off not promise SLA. |

Startup validation chỉ dependency/profile/secret/config, không check password
của7 test accounts hoặc seed real students. Secrets cần sử dụng có thể loaded
securely nhưng không xuất diagnostic. Incompatible/missing typed slots của
profile đang chạy→no business traffic; không silently lower security/reuse old
config. Environment values chỉ là hồ sơ còn phải chọn ở đúng D12/D13 khi đến lúc.

## 2. Startup, liveness/readiness và deadlines

Phân biệt process alive với ability to serve **compatible** authenticated
business reads/writes. Health endpoints là infrastructure surfaces đề xuất,
**không thêm API-155** vào154 business routes hoặc idle renewal path.
Tên path/public exposure/network scope quyết trong D13; không tự mở port.

Startup: validate profile/secret references/key material/timezone/deadlines/
blocklist→connect least-privileged DB→check schema/object/result compatibility
and snapshot prerequisite→ready only if all essential checks pass.
No startup migrations/provisioning/seed mutations, Internet tunnel or restore.
No report promise before SQL/config ready. Runtime SQL option/principal drift
readiness503; liveness can remain200 process alive without saying business ready.

Readiness response minimal literal ready/not_ready with HTTP200/503; liveness
alive only. No server/db names/account/connection/keys/runtime full version,
stack/SQL text/PII. Internal trace/cause class safe operator log, not public dump.
Probe bounded short read no business mutations/replay/audit/history/defaults,
no AuthSession activity touch. Secret-free internal counters don't create web
Admin monitoring panel/role. Exact probe cadence/deadline remains profile slot.

Before dispatch: client disables submit while one intent pending; deadline isn't
authorization to new key. Server propagates cancellation to all SQL operations,
no shared transaction left pending; rollback when commit not started and known
failure, dispose resources. If commit decision/ack uncertain, no success/failure
claim from client deadline; UNKNOWN→authorized read/caller-result; no automatic
retry, credential replay or circuit-breaker resubmit. Client abort is not proof
SQL didn't commit. GET/export errors can retry deliberately as fresh query;
no response partial snapshot/file counted as successful export.

SQL command timeout/all negative applock returns map sanitized conflict or
service failure by known cause; no raw constraint/deadlock detail. Deadlines
slots require actual measurements later; design does not invent throughput
numbers for50-user reference scope. No claim RTO2h/latency was tested.

## 3. Prerequisites theo thời điểm, không implementation phases

| Thời điểm / gate | Hồ sơ cần có / condition |
| --- | --- |
| Trước chuyển sang coding | Actual E04 review +RF decisions, actual E05/full baseline acceptance và owner direction cho implementation riêng; contract refs/compatibility/audit/guard gaps resolved hoặc explicit deferred. Không phases/Slice0/Git plan lúc này. |
| Trước local test/demo chạy được | Approved baseline; compatible runtime/profile/SQL/schema/snapshot/principals/keys/blocklist/deadlines; test data giả/isolated DB và meaningful integration evidence. Đây là future prerequisite, không quyền cài/chạy hiện tại. |
| Trước LAN demo | D13 listen/network/HTTPS/client-trust/origin/firewall/access scope settled; device375/768/1280 theo D15; no SQL public; permission bật access cụ thể. |
| Trước Internet demo | D13 provider/public HTTPS/trusted forwarding/origin/start-stop/access scope settled +authorization exposure; revoke obsolete origin sessions/login mới khi link đổi; no relaxed cookies/CSRF. D12 performance/availability profile settled when owner revisits. |
| Trước backup thật | Storage distinct primary/access/capacity/key custodianship chosen; daily policy missed-run behavior documented; no key co-located plaintext or mistaken old backup fresh. |
| Trước recovery/claim target đạt | Separate restore test DB/machine/path/credential authority +verified encrypted backup/readable key, schema/runtime compatibility, integrity/history/replay/auth checks; record measured recovery duration/data-loss scope. Not restore onto primary casually. |

## 4. Runbook contract cho owner operator

Trước buổi: check profile/ready/current backup status/data giả/expiry/key access/
7roles scoped flows/client path đúng origin; thiết bị khách chỉ UI/API. Connection
failure/readiness mismatch stop business writes, no database role elevation để
chữa. Public link chỉ sau authority đúng scope, startup-stop fields còn D13.

Trong buổi: active failures record trace secret-free; unknown writes tra caller
result/current state và giữ original intent, không click repeated/reset secret
automatically; revoked session clear RAM/login deliberate; no fake occurrence/
attendance/grade/payment để vượt demo. Changed link new session/CSRF/context.

Sau buổi: stop external route/profile theo approved access policy, clear sensitive
RAM/temporary credential handoff, inspect pending unknown intents, current backup
and missed-run status. Laptop off missed daily: next authorized startup operator
check/catch-up/record last-success time; không nói RPO24h attained khi lỡ lịch.
Quarterly dependency/approved offline blocklist review giữ ADR-114/120; no new
packages/provider/release plan implied. Restore monthly isolated evidence.

Backup restore point cũng chứa old sessions/replay/security state. Trước mở lại
business traffic, hồ sơ recovery phải vô hiệu sessions tại restore boundary,
check Account versions/mandatory history/counter/no Invoice number reuse; intents
committed sau backup có thể mất cả marker lẫn business data. Operator reconciliation
phải dựa history/actual externally confirmed payments, không auto replay inputs
hoặc giả restore biết lost commits. Chi tiết recovery invalidation/reconciliation
còn phải rà với môi trường thật; không schema fields/new app refund capability.

## 5. Deferred và trạng thái SC-04

SC-04 đã có typed groups, fail-closed/readiness/cancel/unknown/runbook/prerequisite
contract draft. **Environment slots vẫn explicit deferred** D12/D13 hoặc backup
media/access prerequisites; không gọi SC04 production-ready. Không hỏi lại các
accepted timers/operator/runtime/backup purpose. No config values installed,
provider/domain/port/proxy/backup path chosen, tests/runbook execution hoặc
implementation plan. Actual E04 review must acknowledge every unresolved slot.
