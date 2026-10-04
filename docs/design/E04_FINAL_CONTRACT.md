# E04-v1 — Hợp đồng tổng hợp để nghiệm thu

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Ngày 03/10/2026. **Accepted ADR-160 — owner nghiệm thu03/10/2026.** Đây là phần
kết luận của [E04](SYSTEM_ARCHITECTURE_SPEC.md), không thêm cổng duyệt.
Nguồn nghiệp vụ: [requirements](../requirements.md), [ADRs](../DECISIONS.md),
[E01](FUNCTION_SPEC.md), [E02](WORKFLOW_SPEC.md), [E03](DATABASE_SPEC.md).
RF01–RF04 đã Accepted ADR-156–159, không trình lại các lựa chọn đó.

## 1. Phạm vi bản E04-v1 thực tế

| Tài liệu thuộc bản nghiệm thu | Nội dung và cách đọc |
| --- | --- |
| [Bản chính](SYSTEM_ARCHITECTURE_SPEC.md) | Modular monolith, module ownership, Auth/session/CSRF, 7 roles, backend/frontend/SQL/operation boundaries; các gói đã Accepted ADR-139–155. |
| [Catalog](E04_CONTRACT_CATALOG.md) | 154 method/path, 55 UUID commands, 6 caller-result paths, 32 F frontend URLs/states, 21 W→NV/API/service/TX/E03/cases. |
| [DTO](E04_DTO_SPEC.md) | Request/response fields, required/nullable, exact scalars, search/sort/page, role/task projections, full guard manifests, 154 domain schemas và CSV. |
| [SC-02](E04_REPLAY_SPEC.md) | 55 actions/normalized intent/C1 bytes/SHA256, result references, replay/lookups/no-op/expiry/compatibility. |
| [SC-03](E04_SQL_INTEGRATION_SPEC.md) | 7 SP/6 FN signatures/result sets, EF-trigger mappings, 24-table grants, transaction/lock/read isolation và audit allowlists. |
| [SC-04](E04_RUNTIME_PROFILE.md) | Typed slots, fail-closed startup/readiness, cancellation/unknown, operator/runbook/recovery prerequisites và D12/D13 deferred. |
| [SC-01](E04_PARITY_REVIEW.md) | Domain parity findings, 154 route→schema/action rows và giới hạn bằng chứng tài liệu. |
| Bản kết luận này | Giải quyết ambiguity còn lại, chuẩn ưu tiên, error/result/UI contracts, deferred register và acceptance boundary. |

Trong phạm vi refinement E04-v1 Accepted ADR-160, bản kết luận này và dated addenda là
refinement cuối của checkpoint D10/SC02–04-v1. Chúng thay các placeholder
“còn SC-02/03/04”, không thay requirements/Accepted ADR. Nếu phát hiện lệch
nguồn Accepted thì sửa draft và ghi finding; không dùng quy tắc ưu tiên để
lật policy. DTO miền quyết định fields/NULL/sort; SC-02 quyết định action/bytes/
replay; SC-03 quyết định SQL mapping/gates/audit; SC-04 quyết định profile slots.
Không suy 154 API = 154 controllers hoặc đếm schema = chứng minh chạy đúng.

## 2. Request pipeline và quyền

1. Áp dụng transport/body-size/content-type/deadline guards của profile và
   sanitized errors; không log body, query PII, cookies hoặc credentials.
2. Với protected path, kiểm verified session/Account/security version/lifetime,
   NORMAL hoặc PASSWORD_CHANGE và role/action allowlist trước khi trả detailed
   validation/target diagnostics. Anonymous chỉ Auth bootstrap/login/signup
   theo schema; login vẫn generic failure/throttle như policy đã Accepted.
3. Kiểm Origin/framework CSRF trên POST/PUT, gồm POST search/preview/login;
   GET không business mutation. Same-origin không miễn CSRF; unsafe request
   thiếu token không tới Service/DML. Restricted scope chỉ Auth context/
   password change/logout; không mount hoặc lấy business data.
4. Bind exact allowlist, reject duplicate/unknown properties, validate types/
   field normalization/internal manifest consistency. DTO `expectedSet` là
   claim cần kiểm, không authority. Read-only preview không giữ chỗ/khóa tới save.
5. UUID command normalize/hash toàn supplied intent rồi vào request gate.
   Active completion: current read scope→digest compare→safe replay; không
   kiểm stale RV/source-state/time của một mutation mới trước nhận ra replay.
6. New command: acquire complete sorted gates, Account/session và domain locks;
   reload full relationships/membership/absence/versions, current permissions,
   states/resource/time guards; kiểm lại clock ngay trước DML. Context thay đổi
   abort whole unit; không bỏ equal/stale delta items hoặc tự merge.
7. Actual effects+required audit+T23 (khi command có UUID) cùng connection/TX.
   Commit xong mới trả success/first-only credential; failure known precommit
   rollback, commit acknowledgement uncertain dùng UNKNOWN recovery.

Không tuyên bố response lỗi chứng minh rollback khi client mất kết nối.
Auth/ownership failure không echo target/PII/old RV. State/time/uniqueness
checks chỉ trên targets đã authorized; last-Admin/self, task assignment,
Student own và field-level privacy áp dụng cả list/detail/preview/replay/export.

Read nhiều result sets giữ một short caller SNAPSHOT cho dữ liệu, count/page/
options/context. Current session/task authorization không lấy từ snapshot cũ.
Service kiểm quyền live lúc vào và trước bàn giao buffered DTO/file/completion;
hoàn tất/dispose data SNAPSHOT rồi final authorization trong short READ COMMITTED
scope transaction, recheck Account/session và assignment relation theo SC-03
lock order. Không authorize bằng session rows trong data snapshot cũ. Nếu quyền
mất trước authorization cuối thì bỏ buffered result và trả safe 401/403/404.
Lock/transaction không giữ qua browser confirmation hoặc truyền file. Quyền
được đánh giá tại authorization boundary; không hứa thu hồi bytes đã gửi.

## 3. Scalars, C1 và compatibility

| Điểm từng còn mơ hồ | Contract Accepted cho E04-v1 |
| --- | --- |
| UUID header/lookup | Chỉ UUID dạng D 36 ASCII characters, hyphens đúng vị trí; nhận hex upper/lower, normalize lowercase. Reject braces/URN/N-form/whitespace/multiple header values/empty UUID. Không suy time/caller/authorization từ UUID. |
| RowVersion | Standard padded Base64 đúng 8 bytes; reject URL-safe alphabet/whitespace/noncanonical padding hoặc unused-bit spellings. Canonical encode cùng bytes, không endian/int conversion. |
| JSON scalar | Required/nullable riêng; integer IDs/count/present đúng JSON integer lexeme và range, không exponent/fraction/string/bool coercion. Decimal scores/weights/VND là string theo DTO, không Number arithmetic. |
| Date/time | LocalDate exact YYYY-MM-DD; MinuteTime exact HH:mm, actual date/same-day/start<end. Server-produced Instant UTC YYYY-MM-DDTHH:mm:ss.fffZ; không nhận client clock làm authority. |
| Sets/order | SC-02 array rules áp cho toàn 55 requests, gồm all versions/absence/source sets. Weekly candidates ordered plan phải đúng preview normalization; không sửa plan sai bằng sort/trim. |
| Grade comment | Omitted giữ omitted trong body/digest; provided blank/null→null. Omitted khác clear, clear already-null new intent là no-change sau valid guards. |
| Trace/error | traceId opaque correlation, không chứa PII/sid/request body; fieldErrors public field paths, arrays messages, map rỗng khi không field error. Không old/new values trong lỗi. |
| Replay retention | Đề xuất chính xác 24 giờ tính từ completed_at, đạt E03 minimum; now>=expires_at expired dù row chưa cleanup. Cleanup/operator cadence giữ environment slot, không đổi audit retention. |

EC-C1/action keys/result reference shape freeze khi actual E04 được owner
duyệt. Không có codec-version DB field mới; preserve/drain window theo SC-02
trước incompatible change, không tự xóa markers hoặc thử nhiều codec để match.
Không cần pin patch/framework package ở lượt này; typed compatibility/profile
prerequisites được xác định, concrete values deferred theo mục 7.

## 4. Error registry và kết quả/replay

| HTTP / code Accepted ADR-160 | Semantics / recovery |
| --- | --- |
| 400 VALIDATION_ERROR | Shape/type/normalization/required/range/precision/malformed version/key/set binding. Không marker. |
| 400 NO_CHANGE | New valid-context all-equal P/G hoặc RF02 unchanged grader/score; field errors nơi có field phù hợp, no effect/audit/new marker. Không áp rule này cho signup/profile/password exceptions ngoài phạm vi ADR-159. |
| 401 AUTHENTICATION_FAILED | Login credential failure chung; không email existence/account-state leak. |
| 401 AUTHENTICATION_REQUIRED | Session invalid/expired/revoked; clear sensitive RAM→deliberate login. |
| 403 FORBIDDEN / CSRF_INVALID | Role/restricted scope hoặc CSRF/Origin; CSRF refetch không automatic mutation retry. |
| 404 NOT_FOUND / RESULT_NOT_CONFIRMED | Target thiếu/ngoài scope hoặc authorized lookup không active/currently readable caller marker. Absence không proof rollback. |
| 409 CONFLICT | Stale RV/membership/absence/relationships/capacity/overlap/config freeze/account resource state hoặc elapsed save-time cutoff. Reload/review intent, không overwrite. |
| 409 STATE_CONFLICT | New-key transition sai source state; không lấy kết quả của intent khác làm success. |
| 409 IDEMPOTENCY_CONTENT_MISMATCH | Active same actor/action/key khác C1 bytes, sau current read authorization. Marker bất biến. |
| 409 CSV_EXPORT_LIMIT | Whole encoded export vượt 10.000 data records hoặc 5.000.000 bytes; JSON error trước CSV headers, không partial success. |
| 413 REQUEST_TOO_LARGE | Body vượt profile transport cap; không lesson/date business limit mới hoặc CSV output-cap code. |
| 429 RATE_LIMITED | Accepted login IP cap/Retry-After, không tự LOCKED/revoke existing sessions. |
| 503 SERVICE_UNAVAILABLE | Dependency/readiness hoặc known transient deadline/lock failure sau known abort; không raw SQL/config. Client vẫn UNKNOWN nếu mất write response. |
| 500 INTERNAL_ERROR | Unexpected/integrity/contract drift, mandatory audit failure; sanitized envelope/traceId, không partial data. |

Precedence trên authorized new commands: malformed/internal bindings400;
missing/out-of-scope404; current state/time/relationship/version/set conflict409;
chỉ sau valid current context mới equality400 NO_CHANGE. Khi nhiều live guards
fail, không cam kết client nhận một specific conflicting field; cùng safe409.
Completed replay có nhánh riêng trước live mutation/equality guards. Known
constraint/domain conflict409; applock -1/-2/-3 là abort cần rollback, Accepted
503 khi timeout/cancel/deadlock known; -999/configuration hoặc unexpected500.
Không retry EF execution strategy tự động với these writes/secret operations.

First-response status/fields lấy từng DTO-xxx. Completed replay luôn200:
- API-008/009 và API-120 dùng SafeCommandCompletion với outcome=COMPLETED,
  operationId/resultMode/target; create/reset luôn SECRET_RESULT_UNAVAILABLE,
  weekly SAFE_REFERENCE/classId để refetch, không invented created[] snapshot.
- Commands khác có thể trả authorized current CommandResult theo D10 với
  operationId cũ; nếu chỉ safe refs còn đọc được thì completion fallback.
- 6 lookup paths luôn completion, không detail/current entity/credential.

Frontend phân biệt completion bằng required `outcome=COMPLETED` discriminator;
domain CommandResult không dùng property này. Không infer từ HTTP201/200,
presence of temporaryPassword hoặc version mới. Projection của replay là
current data, không state snapshot at original commit; UI không gắn provenance
hay timestamps mới lên original operation. Secret clear khi rời/logout/expiry,
lost response tra state/marker rồi owner chủ đích reset nếu cần theo policy.

## 5. SQL/EF/audit refinement đã rà

SC-03 là exact Accepted signatures/result-set map, không DDL. Mỗi SP/FN có
real application caller; sequential sets/NULL/bundle/grain/role allowlists
được map trước serialize. SQL SUM decimal38→varchar38 digits tránh .NETdecimal
range, không double. SP07 chỉ allocator trong TX04, counter/audit/replay cùng
rollback. 5 trigger tables có table-scoped OUTPUT alternative, generated ID/RV
phải lấy sau DML; config name/kind swaps intermediate không commit/audit riêng.

24-table grants và 4 roles lấy tên ec_app_runtime/ec_finance_reporting/
ec_academics_reporting/ec_director_reporting của E03; không generic alias
users hoặc DB quyền thành web role. Runtime no DDL/backups/history purge,
T24 allocator-only; reporting outputs/ownership chain phải kiểm khi có SQL.

Class Teacher change giữ mandatory scoped mutation audit của E02 W14 đã
Accepted; wording “không lưu lịch sử đổi giáo viên” ở mô tả ban đầu nghĩa không
thêm separate history entity/workflow/UI. Không bỏ command audit đã duyệt,
không bắt reason mới hoặc cho Teacher cũ tiếp tục task access. Attendance
ADR-149 giữ latest enteredBy/enteredAt và T23; không tự bổ sung mandatory
AuditLog old/new Attendance history mới. Mixed-delta only actual mutations
đổi provenance, không discard supplied equal item trước hash/validation.

Protocol resource set không thêm gates out-of-order. Business-gate discovery
không authorize writes; reload changed set abort/reprepare before DML. Khi
replay/lookup cần bảo vệ current assignment, acquire đúng existing scope gates
(entry TIMETABLE và related REG; Class CLASS), sorted trước Account/session
và domain locks; không đòi source-state window cũ để đọc completion. Không
commit/update completion hoặc tạo audit/effects lại khi recheck.

Expired T23 row còn tồn tại không được runtime update/delete/reuse partition:
lookup vẫn404; command cùng actor/action/key đã hết hạn nhưng row còn giữ
trả409 CONFLICT, không DML hoặc marker thứ hai đụng unique constraint. Sau
maintenance cleanup không thể biết key từng dùng; expiry không bảo đảm dedup
vô hạn. UI luôn đối chiếu state/history trước deliberate new intent/new key,
không tái dùng expired key để xin cấp secret hoặc tái tạo Schedule root.

Nguồn kỹ thuật đã đối chiếu03/10/2026: [EF SQL Server triggers/OUTPUT](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/misc),
[sp_getapplock return/order/rollback](https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql?view=sql-server-ver16),
[SQL SUM precision](https://learn.microsoft.com/en-us/sql/t-sql/functions/sum-transact-sql?view=sql-server-ver16).
Nguồn xác nhận platform constraints, không chứng minh contract implementation.

## 6. Frontend/export contract

Catalog mục5 là đủ 32 F/8 state patterns, account-menu exceptions không F mới.
Cache/query keys gồm verified identity/role/task/filters/page; discard responses
thuộc old session hoặc request superseded. Logout/revoke/identity change clear
RAM cache/drafts/preview/secret; không persisted query cache/service worker
lưu API/PII. No-store áp cho Auth/protected DTO/errors/export; search PII POST
body; URL/browser storage không chứa names/email/phone/grades/tokens.

No optimistic business success: response committed/refetch mới đánh dấu saved.
409 deliberate reload/review; NO_CHANGE giữ form với giải thích, không “Đã lưu”.
Unknown intent giữ same action/content/key trong RAM, không auto-resubmit/new
UUID; session mất thì clear sensitive content, không recover bằng storage.
Per-LESSON Attendance/per-Student scores-comment và whole-Class send/review
giữ save units; reload subset phải refetch đầy đủ relevant guard context,
không dùng saved-changed refs làm full cohort. Current preview inputs đổi thì
invalidate; no browser unsaved edits ngầm đưa vào send/finalize.

CSV D10 fixed6 columns và whole-set footer; 10.000 data records tính footer,
không header/physical lines. 5MB đề xuất SI=5.000.000 encoded bytes gồm BOM/
header/footer/quotes/apostrophe/CRLF; đúng cap được xuất. Fresh same-filter
SNAPSHOT/SP04, buffer whole bounded output trước200. Runtime responseBuffer
phải đủ accepted CSV cap; body/weekly budgets không silently cap nghiệp vụ.
Narrow kỳ không hứa làm date-free debt biến mất. Formula mitigation không
sửa DB text; exact digit strings/import money-as-Text, open-save-reopen cần
future spreadsheet evidence. Director không export, Student own print fresh,
Admin audit in-app only. Không export jobs/PDF/XLSX/hidden person dumps.

375/768/1280, keyboard/focus/labels/non-color/44px direction và Chrome/Edge
là verification obligations, chưa visual/accessibility/runtime test passes.

## 7. Deferred register và acceptance boundary

| ID | Phần giữ hoãn / nguồn | Gate áp dụng và ảnh hưởng hiện tại |
| --- | --- | --- |
| DF-01 | D12: numerical performance/availability SLO/deadlines/budgets và benchmark environment | Không chốt giá trị hoặc claim 50 users đạt; finite typed budgets/order/fail-closed đã specified. Chặn cấu hình/chạy/claim performance, không thiếu business/API contract. |
| DF-02 | D13: concrete host/provider/domain/origin/port/HTTPS/trusted proxy/firewall/access/start-stop | Same-origin/Secure/CSRF/no SQL public giữ fixed; missing active-profile fields fail closed. Chặn LAN/Internet exposure, không triển khai từ E04 acceptance. |
| DF-03 | Backup media/path/capacity/access/key/isolated restore target | Policy/operator/RPO24h-RTO2h targets giữ Accepted; cần environment profile và authority trước backup/recovery, không promise đạt khi laptop tắt. |
| DF-04 | Compatible patch/package lock, SQL compatibility identity, offline blocklist pinned revisions/license/hash | Runtime/source choices Accepted; pin/verify chỉ sau baseline và owner direction. Không cài package hoặc dùng latest suy đoán. |
| DF-05 | Runtime/SQL/browser/CSV/race/load/restore evidence | VC-W01–21, VC-REPLAY, DC01–18/rubric là designed evidence obligations; chưa executable tests hoặc results. Chặn claim implemented/verified, không thay owner design acceptance bằng số test giả. |

DF-01/02 kế thừa owner deferral, không hỏi lại; DF-03/04 là deployment
prerequisites đã nêu trong accepted runbook, không new product question.
DF-05 là phase-of-evidence boundary, không waive invariant/SQL rubric.

SC-01–04 và actual E04-v1 package được owner nghiệm thu03/10/2026,
Accepted ADR-160,giữ deferred register DF01–05. E05-v1 và complete baseline
được nghiệm thu sau E04 trong cùng lời xác nhận,ADR-161. Các contract đã trình
là accepted design,không runtime/production-ready attestation. Thay đổi thiết
kế sau này phải cập nhật nguồn/trace/ADRs affected và xin quyết định khi cần.
Không implementation planning/code/SQL/Git/scaffold/deploy từ nghiệm thu này.

## Cập nhật SQL target sau nghiệm thu — 04/10/2026

ADR-162 thay riêng SQL choice ADR-119 bằng SQL2025 Express đang có sau
[assessment](../plan/SQL_INSTANCE_ASSESSMENT.md). Giữ .NET10/EF10 và toàn bộ
E03/E04 object/DTO/TX/grants/readiness contracts; DB/EF targetcompatibility160
chưa cấu hình. Encrypted backup dùng scheduler/archive encryption ngoài engine,
không suy Express có SQL Agent/native encryption; ADR-118 và DF03 giữ nguyên.
Đây là amendment môi trường đã được owner cho phép, không mở lại E01–E05
hoặc thay counts/trace cases; EF/race/load/recovery vẫn chưa được kiểm chứng.
