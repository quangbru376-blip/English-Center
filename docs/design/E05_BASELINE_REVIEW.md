# E05-v1 — Rà tính nhất quán và truy vết toàn baseline

## Nghiệm thu actual output — 03/10/2026

Owner xác nhận nghiệm thu E04-v1 trước rồi E05-v1 trên đúng các output đã
trình, không sửa phương án. E04 actual package/refinements Accepted ADR-160;
E05 whole-baseline cross-check Accepted ADR-161. Nhãn proposed/draft/review-
pending trong checkpoint cũ là lịch sử trước lượt nghiệm thu này, không còn
owner choice mở của các contract đã trình. Environment DF01–05/D12/D13 vẫn
hoãn/prerequisites; không configured values, runtime evidence hoặc quyền code/
SQL/Git/scaffold/deploy/implementation planning. Hai gate OWNER_REVIEW đã tích.

Ngày 03/10/2026. **Accepted ADR-161 — owner nghiệm thu03/10/2026 sau E04-v1.**
Rà trên đúng actual [E04-v1 package](E04_FINAL_CONTRACT.md) đã Accepted160,
owner nghiệm thu E05 sau E04,trên [OWNER_REVIEW](../plan/OWNER_REVIEW.md).
Thay đổi baseline sau này cần cập nhật affected findings/trace/nguồn. Không
code/SQL/Git/scaffold/deployment/implementationplanning từ nghiệm thu này.

## 1. Baseline được rà và kết luận

Nguồn: [requirements](../requirements.md), [ADR-001–161](../DECISIONS.md),
[E01 functions](FUNCTION_SPEC.md), [function catalog](FUNCTION_CATALOG.md),
[E02 workflows](WORKFLOW_SPEC.md), [E03 database](DATABASE_SPEC.md), E04-v1
bản chính/catalog/DTO/SC01–04/final contract. Provenance từ
[REPORT_BASELINE](../reference/REPORT_BASELINE.md); report-style crosswalk
[BUSINESS_FUNCTION_COMPARISON](../reference/BUSINESS_FUNCTION_COMPARISON.md)
và [37-function table](BUSINESS_FUNCTION_TABLE.md) chỉ giải thích nguồn, không
override Accepted rules. Source Word/illustrative forms giữ nguyên.

Kết luận thiết kế đề xuất: các retained source obligations có actor trigger,
workflow, dữ liệu/invariant, scoped API/DTO/UI và verification obligation;
không phát hiện unresolved product-policy choice mới trong lượt rà này.
RF01–RF04 resolved ADR-156–159. Các technical refinements mới nằm trong
actual E04-v1 Accepted ADR-160; E05 Accepted ADR-161 sau E04.
D12/D13 và environment/evidence prerequisites được liệt kê rõ ở mục 6.

E01/E02/E03 đã Accepted vẫn giữ. E04-v1 đầy đủ tài liệu để review; E05-v1
đầy đủ bản cross-check để đọc có điều kiện. Đây là source/contract review
và structural validation, không proof từng field được implement đúng hoặc
test/race/privacy/performance/recovery pass. Một center/7 roles/37 retained
NV/32 F/21 W/24 tables/230 fields là phạm vi trace, không số code artifacts.

## 2. Matrix riêng từng retained NV

NV title/actor/source nghĩa vụ lấy E01; F lấy E01 mục9, W lấy E02 mục10.
API-xxx nối đúng DTO-xxx tại D10 và [154-row schema/action trace](E04_PARITY_REVIEW.md#6-trace154-từng-route-tới-schemasourcereplay).
Catalog mục6/SC03 cung cấp service/SQL/TX đầy đủ cho W tương ứng; các cells
dưới chỉ tên persistence chính, không cấp role quyền SELECT toàn bảng.
VC-Wxx là designed verification contract tại catalog mục7; case E5-xx ở mục4
nối các boundary xuyên nhiều NV. NV36–39 là effects, không actor/endpoint mới.

| NV / actor | W / F | API / DTO nguồn | Service / persistence chính | Invariant trọng yếu / case |
| --- | --- | --- | --- | --- |
| NV01 Student/7 roles | W01,W02; F01,F02 | 001–005,018 | Students signup TX01 T02/T03; Administration Auth T02/T04/T22 TX13 | Atomic Account/Profile, no auto-login, credential exact/no replay; VC-W01/02, E5-01/02. |
| NV02 Student | W05; F04,F05 | 022–028,037 | Admissions T05/T09/T11–13/T20/T22/T23; FN03/TX06 | Own ACTIVE Course/unfinished unique, immutable Course, cancellation coupled; VC-W05, E5-03/04. |
| NV03 Student | W07; F06,F07 | 029–033,037 | Admissions TX02 T09–12/T15/T16/T23 | Two logical attempts vs booking history, OPEN/prestart/capacity/no overlap; VC-W07, E5-05/06. |
| NV04 Student | W08; F08 | 034 | Admissions T11–13/FN02 | Own completed scores/Band, ABSENT not0, tie first attempt; VC-W08, E5-07. |
| NV05 Student | W09; F09 | 035–037 | Admissions REG atomic T09/T11–13/T22/T23 | BAND_ASSIGNED/no pending retake/live correction race, no Invoice effect; VC-W09, E5-08. |
| NV06 Student | W11; F10 | 046–047 | Finance T09/T20/T21/FN06 | Own current Invoice/accurate fresh print, no receipt/PDF API; VC-W11, E5-09. |
| NV07 Student | W14,W16; F11 | 112–113,129–130 | Academics T14–16 | Own current/history, one replacement obligation/no peer roster/transfer; VC-W14/16, E5-12/13. |
| NV08 Student | W19; F12 | 148–149 | Academics T15/T18/T19 | Own FINALIZED only, no unfinalized placeholder data; VC-W19, E5-16. |
| NV09 Admissions | W10; F13,F14 | 038–039 | Admissions SP01/VW01 T09/T20 | Contact/current profile+Invoice allowed, no DOB/address/Payment/grade; VC-W10, E5-17. |
| NV12 Admissions | W10; F14 | 040–045 | Admissions/Finance TX04/TX06 SP07 T09/T20–24 | CONFIRMED approval+one Invoice, reasoned reject/cancel guards; VC-W10, E5-04/09. |
| NV13 AcademicAffairs | W13; F18 | 074–090 | Academics T05–08/T14 TG01–03/BANDSET/PROGRAM | Whole unused DRAFT configuration, activation100/one exam, freeze any first use; VC-W13, E5-10. |
| NV14 AcademicAffairs | W06; F16,F17 | 054–065,045 | Admissions T10/T12/T22/T23 FN01/TIMETABLE | Create future CLOSED, edits no booking ever, close/open/cancel/grader windows; VC-W06, E5-05/18. |
| NV15 AcademicAffairs | W06,W08; F16,F17 | 054,056,065 | Admissions SP02/FN01 T10–13 | Role-specific roster/current best Band, Invoice status only/no teacher override; VC-W06/08, E5-17. |
| NV16 AcademicAffairs | W14; F19,F20 | 091–094,098–103,090 | Academics T07/T14–19 TX07/FN04 | Explicit lifecycle, full required schedules/HELD/Attendance, completion before grade send; VC-W14, E5-11/14. |
| NV17 AcademicAffairs | W14; F20 | 095–097,090 | Academics T14/T16/Account guards | Preteaching/no dependent data, all timetable conflict checks, old Teacher loses scope; VC-W14, E5-12/17. |
| NV18 AcademicAffairs | W15; F22 | 104–108,090 | Academics TX08 T09/T13–16/T19/T20/T23 | OPEN/matching CourseBand/PAID/capacity, Assignment+DRAFT atomic, no transfer; VC-W15, E5-11/12. |
| NV19 AcademicAffairs | W16; F21 | 114–126,090 | Academics TX09 T14/T16–18/T22/T23/TIMETABLE | Future roots/move targets, all ordered weekly plan, replacement one obligation, explicit HELD; VC-W16, E5-13/18. |
| NV20 AcademicAffairs | W19; F23 | 143–147,090 | Academics SP03/FN03/FN05 TX12 T09/T18/T19 | Full SUBMITTED cohort return/finalize, reason, total/publication/unfinished atomic; VC-W19, E5-15/16. |
| NV21 Teacher | W06; F25 | 066–069 | Admissions SP02 T10–12 | Current grader task only/contact allowlist/no finance, not all-center Student directory; VC-W06, E5-17. |
| NV22 Teacher | W08; F25 | 069–073 | Admissions TX03 FN02 T06/T09/T11–13/T22/T23 | End boundary/absence/manual result/original-current grader correction/no-change; VC-W08, E5-07/08/18. |
| NV23 Teacher | W14,W16; F24 | 109–111,127–128 | Academics T14–16 | Current Class Teacher only; no stale assignment access or profile edits; VC-W14/16, E5-12/17. |
| NV24 Teacher | W17; F26 | 131–133,090 | Academics TX10/TG04 T14–17/T23 | One LESSON delta, start≤now/before COMPLETED, missing≠absent, full context then mixed changes; VC-W17, E5-14/19. |
| NV25 Teacher | W18; F27 | 136–141,090 | Academics TX11/12 FN05/TG05 T07/08/T14–19/T22/T23 | Per-Student score/comment then separate whole-Class submit, missing≠0/RV touch; VC-W18, E5-15/19. |
| NV26 Teacher | W18; F27 | 136–140,142,090 | Academics TX11/12 T18/T19/T22/T23 | RETURNED reason visible, actual corrections, complete cohort resubmit/no partial; VC-W18, E5-15/19. |
| NV27 Accountant | W11; F28 | 048–049 | Finance T05/T09/T20/T21/FN06 | Scoped operational Invoice/Payment/contact; no academic fields or directory export; VC-W11, E5-17. |
| NV28 Accountant | W12; F29 | 050–053 | Finance TX05 T09/T20/T21/T22/T23 | One full Payment/PAID atomic; metadata reason only, immutable amount/paidAt/link; VC-W12, E5-09/18. |
| NV29 Accountant | W20; F30 | 150,154 | Reports SP04/VW06/FN06/SNAPSHOT | Exact18→38 sums, period revenue/billed vs current debt, fresh bounded CSV/report columns; VC-W20, E5-20/21. |
| NV30 Director | W20; F31 | 152 | Reports SP05/VW01/SNAPSHOT | Distinct independently overall/groups, created-period/current-state/no Student IDs; VC-W20, E5-20. |
| NV31 Director | W20; F31 | 153 | Reports SP06/VW03–05/FN04/FN05 | Current inventory date-free, LESSON progress, FINALIZED-date/pass/no-data distinct; VC-W20, E5-13/16/20. |
| NV32 Director | W20; F31 | 151 | Reports SP04/VW06/FN06/SNAPSHOT | Same financial definitions/aggregate scope/no operational records/export; VC-W20, E5-17/20. |
| NV33 Admin | W01,W03; F32 | 006–009,014 | Administration TX01/TX13 T02/T04/T22/T23 | First-only temp32/24h/restricted/revoke, no secret replay/email edit/auto unlock; VC-W01/03, E5-02/22. |
| NV34 Admin | W03; F32 | 010–014 | Administration TX13/ADMINSET T02/T04/T22/T23 | Self/last-active protections concurrent, restore never revives old sessions; VC-W03, E5-02/22. |
| NV35 Admin | W21; F33 | 015–017 | Administration T22 grouped history | Allowlisted paginated newest/operation read, no secrets/edit/purge/export; VC-W21, E5-23. |
| NV36 System effect | W08; F08,F25 | Effect070/072; read034/065/068 | Admissions TX03 FN02/TG03 T11/T13 | Highest completed/tie first/Band seed freeze, no independent system mutation API; VC-W08, E5-07/10. |
| NV37 System effect | W10,W11; F10,F14,F28 | Effect041; read039/046–049 | Finance within Admissions TX04 SP07 T20/T24 | Approval one Invoice snapshot/year/due+7, no standalone issue command; VC-W10/11, E5-09. |
| NV38 System effect | W18,W19; F12,F23,F27 | Read136–137/140/144/149; effect147 | Academics FN05/TX12 T18/T19 | Decimal intermediate/one half-up/NULL missing; final_score only FINALIZED/pass≥50; VC-W18/19, E5-15/16. |
| NV39 System effect | W21; F23,F27,F33 +applicable actions | Required mutation events; read015–017 | Administration audit writer within caller TX T22 | Grouped mandatory old/new retained, security best-effort/90d/IP/actorNULL; VC-W21, E5-23. |

F03 home/W04 và own profile/account-menu/change-password/logout là shared
accepted presentations; không NV40/F34. Full 32F mapping ở E01 mục9 và catalog
mục5, gồm F03 API021 và profile019–020. Academic read-only Attendance134–135
thuộc F20/W17, không gán Teacher write rights cho AcademicAffairs.

## 3. State/data/API/UI parity

| State / nguồn | Dữ liệu và đường API | Điểm đã đối chiếu |
| --- | --- | --- |
| Account ACTIVE/LOCKED/DISABLED | T02/T04, API002–005/009–013 | Throttle không LOCKED; lock/disable/reset/change revoke-all, logout current-only; restore không resurrect. Restricted scope không role khác. |
| Registration PENDING→TESTING→BAND_ASSIGNED→CONFIRMED→ADMITTED | T09/T11–13/T20/T15, API025/030/070/036/041/108 | Initial state/result paths đã sync ADR-133. Placement giữ ADMITTED/derived placed; no COMPLETED Registration enum. Unfinished false CANCELLED/REJECTED hoặc Class COMPLETED+own FINALIZED, không phụ thuộc passing. |
| Registration CANCELLED/REJECTED | API028/042/044 TX06/current guards | Cancellation future active bookings+UNPAID Invoice atomic; started history/Band kept/no more grading; rejection CONFIRMED reason riêng/no refund. New registration testing anew. |
| Entry Session OPEN/CLOSED/CANCELLED | T10/FN01 API055/057–061/064 | New CLOSED/future; start effective CLOSED even stale stored OPEN; CLOSED student mutations blocked, permitted grading after end retained; cancel before start only. |
| Attempt/Booking | T11/T12 API030–032/070–072 | Logical attempt1–2 riêng booking history. ABSENT≠score0; completed result references matching completed booking. No retake after CONFIRMED/no pending-result bypass. |
| Course/Program lifecycle | T05/T07/T08/T06 API075–089 | Explicit lifecycle/whole configuration, no componentCRUD/Band editor; active config100/one exam; first-use freeze retained even Class CANCELLED. |
| Class PLANNED→OPEN→ONGOING→COMPLETED/CANCELLED | T14–19 API092/099–102 | Open at least future LESSON; start full required LESSON/current exam context/≥1student; complete all HELD+Attendance, not grades published; cancel empty preteaching. |
| Schedule PLANNED/HELD/NOT_HELD +current chains | T16 API118/120/122/124/126 | RF03 future root/weekly/move, replacement same obligation/history/no fork; HELD explicit after end while ONGOING; no held inferred from clock/Attendance. |
| Attendance/Score missing vs stored0 | T17/T18/T19 API133/139 | No default rows/delete, expected absence distinct existing RV, validate full cohort/whole supplied intent, delta actual changes preserve equal provenance. Comment omitted/null distinct. |
| Grade DRAFT/RETURNED→SUBMITTED→FINALIZED | T19/API141–142/146–149 | Per-Student saves vs whole-Class lifecycle distinct; AcademicAffairs no DRAFT fields/tokens; publication all cohort atomically; Student own FINALIZED only. |
| Invoice UNPAID/PAID/CANCELLED | T20/T21 API041/051/052/028/044 | Positive18 money/snapshot/dueVN+7, collection exact amount/confirmation time, no partial/paid cancellation/refund; overdue derived warning only. |
| Replay completion | T23/55commands/6lookups/SC02 | Active caller/action/key/bytes/current scope, atomic completion refs not response/secret; wrong-source new intent vs completed replay distinct; window≥24h, exact24h Accepted ADR-160. |

These checks are document parity, not proof SQL triggers or middleware enforce
them. Exact schemas and example values at source remain the contract; trace
cells are summaries, not substitutes for request required/nullable sets.

## 4. Cross-contract review cases và expected evidence

Các cases là review scenarios/oracles, không test code/SQL hoặc implementation
sequence. Catalogue VC/DC nguồn cung cấp thêm từng workflow/object detail.

| Case / nguồn | Scenario | Expected design outcome / proof còn phải có |
| --- | --- | --- |
| E5-01 / DC01,W01 | Signup Account insert thành công rồi Profile failure/unique race | Rollback no orphan/no auto-login; wrong email/password context generic; isolated SQL/API evidence future. |
| E5-02 / DC15,W02/03 | NORMAL/restricted/idle/absolute/temp boundary; lock/reset vs active command | Correct lifetime/revoke all/current logout; Account→sessions lock order, no revive/auto-replay. |
| E5-03 / DC02,W05/19 | Same Course concurrent new enrollment vs finalize failing/passing outcome | Only one unfinished; unfinished cleared only complete+FINALIZED, failing still repeat allowed; whole finalize rollback keeps unfinished. |
| E5-04 / DC03/11,W05/10 | Cancellation vs payment/test-start/grading/confirmation | Source guards/clock serialized, UNPAID+future bookings cancel atomically; started history preserved/no backfill/refund. |
| E5-05 / DC03/04,W06/07 | Closed/full/effective start/last-seat booking, old+new reschedule | OPEN before start on both slots, logical attempt stable, last seat one winner, no old-booking partial cancel. |
| E5-06 / DC05,W07/15/16 | Same Student/Teacher/room/class overlap and exact adjacency | [a,b)/[b,c) allowed, real overlap rejected across class+test; schedule changes recheck placed cohort, no PII conflict leak. |
| E5-07 / DC03/09,W08 | Boundary Band thresholds, best tie, absent vs actual0, correction same parsed score | Seed highest≤score/tie attempt1, absent no value,0 real result; original/current grader/preconfirm/reason, equality400/no fake history. |
| E5-08 / W08/09 | Pending retake or stale correction while confirming Band | No pending-result confirmation; correction vs confirm serialized, input/best/Band same TX, no privileged override. |
| E5-09 / DC11/12,W10–12 | Approval allocator failure/yearVN+7days, duplicate collect, metadata correction | Invoice snapshot/counter/approval audit+marker atomic, paidAt/link/amount immutable, no Bank-ref dedup or second Payment. |
| E5-10 / DC08,W13/14 | Config reorder/name-kind swap vs first Class; first result Band freeze | Whole desired/source sets/hash, unused DRAFT swap atomic, retained first use freezes even later CANCELLED; no scratch-name audit. |
| E5-11 / DC04/07,W14/15 | Placement last seat vs cancel/start, start without full schedules | One eligible OPEN placement+Assignment+DRAFT; no partial/late placement; start explicit/no fake HELD. |
| E5-12 / DC05,W14–16 | Teacher changed/blocked, direct old-Teacher Class/roster/replay access | Correct preteaching/no dependency/all-conflict window; current task rights lost, no separate Teacher-history UI/auto reassignment. |
| E5-13 / DC05/06,W16/20 | Weekly one invalid candidate/future cutoff, multi-hop exam/LESSON replacement | All-or-none/no skip, one current obligation, no dependent data or fork/cycle; report progress counts replacement once/exam separate. |
| E5-14 / DC07,W17/14 | LESSON start/completion race, missing vs absent sheet, HELD afterend | Attendance starts at LESSON start/not fixed24h, complete requires full Attendance/HELD, no postcomplete edits/false absence. |
| E5-15 / DC09/10,W18 | Score/comment save fail vs submit full cohort, absent linked schedule | One Student whole supplied intent, parent reportRV touch for actual changes; linked current type/start, missing totalNULL, whole-class preview never page subset. |
| E5-16 / DC09/10,W19 | Whole finalize one invalid report,49.995/49.994, Student reads before commit | Publish none or all; final totals50.00/49.99 once, final_score only FINALIZED; no DRAFT/SUBMITTED Student leak, unfinished atomic. |
| E5-17 / DC17,all roles | Same target via list/detail/preview/lookup/print/export and changing session/task | 7role field/ownership allowlists consistent, missing/out-of-scope404, Director no facts/exports, AcademicAffairs no DRAFT, SQL DENY alone insufficient. |
| E5-18 / ADR156–159,SC02 | Preview valid but clock passes; all-equal value edit vs wrong-source new key vs matched completed key | Cutoff409, valid-context equality400/no marker, wrong-source409, same completed replay200/no new effect; no accepted rule re-vote. |
| E5-19 / ADR159,SC02/03 | Mixed delta includes unchanged stale row; missing row saved as0; comment omitted vs clear | Validate/hash all supplied items, one invalid abortall; actual changes only/provenance preserved; creation0 real, comment clear distinct bytes. |
| E5-20 / DC16,W20 | Concurrent finance/finalize query, different event periods/debt, distinct Student in2courses, empty pass cohort | One SNAPSHOT count/groups/page/options, exact decimal38 strings; distinct overall independent, inventory date-free/debt not period, no-dataNULL vs0.00. |
| E5-21 / ADR101,W20 | CSV10000/10001records and5000000/5000001bytes, multiline/full-width formula/digits38 | Footer counted/header excluded, BOM+all encoded bytes counted, reject whole before200; exact report columns/fresh query, import-as-Text and app open/save/reopen evidence future. |
| E5-22 / DC04/15,SC02 | Same/different key content, lost first temp response, expired marker/Teacher no longer assigned | Same active intent one commit/secret unavailable; mismatch409 afterscope;404 not proof rollback; no automatic new secret/retry/reconstructionweekly. |
| E5-23 / DC13/14,W21 | Mandatory audit failure vs logout log failure and90dcleanup | Mandatory effects+audit+completion rollback; pure security best-effort never restore session; verified actor/null, secret-free fallback, mandatory retained/IP handling correct. |
| E5-24 / DC17/18,SC03/04 | SQL grants/view-chain/multirow trigger/generated RV, schema/snapshot profile mismatch, backup missed/restored | Allowed outputs not just DENY; correct5table OUTPUT mapping/keyless sets, fail-closed no runtime DDL; missed-run/recovery targets need actual isolated evidence. |

## 5. Findings và resolution

| ID | Finding tại checkpoint | Resolution trong bản actual review |
| --- | --- | --- |
| CF-01 | Markers “E04 incomplete/còn SC02–04” vẫn tồn tại trong dated history | Current front matter/gap register dùng E04-v1 Accepted ADR-160 và E05-v1 Accepted ADR-161; history không authority cho việc cần làm tiếp. Final contract là đầu mối cho technical refinements. |
| CF-02 | Error literals/no-change precedence và first/replay union chưa thống nhất một chỗ | Final contract mục2–4: source authority first, valid-context equality last; matched completion before stale mutation; domain/completion discriminator, canonical selectors và sanitized error registry. |
| CF-03 | Codec UUID/RV/precision/array/presence/expiry có ambiguity parser | Final contract mục3 và SC02: strict D UUID/Base64/scalars, whole supplied normalized intent, ordered plan vs semantic sets; exact24h Accepted ADR-160, compatibility frozen at actual E04 acceptance. |
| CF-04 | Read SNAPSHOT có thể bị hiểu là session/task quyền cũng được đóng băng | Final contract mục2/5: data snapshot separate live authorization boundary; buffer discarded nếu quyền mất trước output, consistent SC03 gate/Account-session order. |
| CF-05 | Weekly marker classId không đủ original created[]; secret markers không credential | SC02/final contract explicit SafeCommandCompletion for weekly/Admin, refetch authorized current data/no invented original batch or secret snapshot. |
| CF-06 | SQL decimal38 lớn hơn .NETdecimal; SP02role/SP03DRAFT projection/grants không thể dùng generic entity | SC03 typed sequential sets/strings/role rows/non-DRAFT children/keyless map/24table grants/real callers. EF5trigger OUTPUT alternative, swaps in same unused-configTX. |
| CF-07 | Requirements có ghi chú E03 dictionary “còn chờ” từ trước nghiệm thu | Sửa status editorial thành E03-v1 Accepted01/10/2026, không sửa business model/type/NULL/230fields/owner marks. |
| CF-08 | “Không lưu lịch sử đổi Teacher” có thể bị hiểu là bỏ scoped command audit | E02 W14 đã Accepted giữ mandatory mutation audit; no separate Teacher-change entity/workflow/UI theo ADR023/058. Clarify wording, không thêm reason/Teacher historic task access. |
| CF-09 | CSV5MB và row-count footer chưa có exact convention được trình | E04-v1 Accepted ADR-160: SI5000000 encoded bytes/all10k data records incl footer;6visible report columns, fresh bounded output/error409. |
| CF-10 | Runtime slots/evidence có thể bị gọi là unresolved business gap hoặc tự điền cho đủ | Final contract DF01–05 explicit: D12/D13 owner-deferred, backup/patch/blocklist/profile prerequisites; finite types/fail-closed set, no guessed numbers or operational authority. |

CF02–06/09 refinements đã Accepted trong actual E04-v1 ADR-160; toàn bộ
cross-check Accepted ADR-161. Các finding có resolution ở mức tài liệu;
không suy ra implementation hoặc runtime evidence. Không còn RF choice mới.

## 6. Excluded/deferred/rubric và giới hạn bằng chứng

| Loại | Phạm vi / nguồn | Kết luận E05 |
| --- | --- | --- |
| Removed source obligations | NV10/NV11/F15, Scholarships và3scholarshiptables ADR012/report baseline | Không giữ endpoint/menu/on-behalf/diagram để tái nhập; retained37NV/32F không renumber. |
| Product exclusions | Online exams/payment gateway/installments/Director approval; ten-point scale/retake afterconfirm/debt penalties/transfers/refunds/emailrecovery/room-holiday modules/versionoverride | Không gap cần lấp, không roadmap/implementation backlog mới. |
| Deferred DF01–02 | D12/D13 exact performance/availability/deployment values, owner đã hoãn | Không hỏi lại. Design acceptance đề xuất thừa nhận deferral, không cấp quyền chọn provider/port/timeout/tunnel. |
| Prerequisites DF03–04 | Separate backup media/access/key/restoreprofile, compatible patch/blocklist pins, schema compatibility/config | Trước hoạt động cần data/authority/profile thật; typed fail-closed chưa có config install. Không đổi Accepted runtime/operator/backup policy. |
| Evidence DF05 | Runtime/SQL/EF/grants/race/browser/CSV/load/restore checks | Chưa thực hiện; architecture baseline acceptance không attestation production-ready/rubric pass. |

Rubric ADR-130/E03 mục12 vẫn giữ24tables/5TG/6VW/6FN/7SP/13TX/4SQLroles,
index candidates gắn real query và evidence ít nhất5queries before/after,
5business transactions+forcedrollback,4principal GRANT/REVOKE/DENY.
7SP/6FN đều có service/API caller trong SC03, views giữ grain/query source;
không SP-per-NV giả hoặc privileged SQLdemo thành app role. 18DC và VC/E5cases
giữ evidence obligations; không coi signatures/counts/Markdown check là pass.

Source Word/oldinstructionkits/formimages/reportalignmentguides không thêm
scholarships/PIIfields/room module/secret logs/tenpointgrade/late retake hoặc
implementation phases. Sequence/report side work vẫn on-demand riêng scope.

## 7. Structural validation và owner acceptance

Validation trên tài liệu: inventory unique154/UUID55/lookups6, one-to-one
registry/method-path/role bindings,55gate/audit classifications; 37NV/32F/21W
coverage và mỗiAPI→DTOsection/action; local links/Markdown tables/24JSON
examples;4full C1 literal byte/hash fixtures. Kiểm fingerprint Accepted E03,
E01/E02/ADR và owner checked/notes preservation. Kết quả chạy kiểm tra ghi tại
checkpoint validation của SC01 sau khi edits xong; không fake “test passed”.

Không tuyên bố đã independently prove every field/function branch của154
schemas hoặc verification execution; owner nhận actual scoped contracts và
crosswalk có findings/deferred/expected evidence rõ, không placeholder artifact.

Owner đã xác nhận nghiệm thu theo thứ tự E04-v1 (ADR-160) rồi E05-v1
(ADR-161) ngày03/10/2026,không sửa output đã trình. E01–E05/complete design
baseline Accepted;OWNER_REVIEW E04/E05 [x],73/75accepted,còn D12/D13 hoãn.
DF01–05 giữ environment/evidence prerequisites,không waive invariants hoặc
permission thực thi. Chờ owner direction riêng trước implementation planning/
code/SQL/Git/scaffold/deployment. Không có runtime evidence mới.
