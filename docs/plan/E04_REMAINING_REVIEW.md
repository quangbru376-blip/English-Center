# Bảng duyệt các gói còn lại — E04

Phiên bản: E04-R1, ngày 01/10/2026. Trạng thái: **23/23 mục đã được owner duyệt và ghi ADR-150–ADR-155; E04/E05 chưa nghiệm thu**.

File được tạo theo yêu cầu trực tiếp của bạn sau khi duyệt Gói4e. Đây là nơi
duyệt **các đề xuất gói còn lại**, không phải kế hoạch triển khai. Có **6 gói,
23 mục duyệt**; owner đã tích đủ23 ô và xác nhận đồng ý toàn bộ ngày01/10/2026.
Ghi nhận Gói4f/5/6/7/8/9 lần lượt ADR-150–ADR-155; không hỏi lại ADR-001–ADR-155.
Dấu duyệt và nội dung/ghi chú owner được giữ nguyên; không suy nghiệm thu E04/E05.

Nguồn: [requirements](../requirements.md), [quyết định](../DECISIONS.md),
[E01](../design/FUNCTION_SPEC.md), [E02](../design/WORKFLOW_SPEC.md),
[E03](../design/DATABASE_SPEC.md), [E04 đang soạn](../design/SYSTEM_ARCHITECTURE_SPEC.md),
[kế hoạch baseline](design-baseline.md), [bảng nghiệm thu chính](OWNER_REVIEW.md).
Nếu phát hiện đề xuất trái nguồn đã duyệt, dừng mục đó để đối chiếu, không
âm thầm thay quyết định cũ.

## 1. Cách điền và giới hạn lời duyệt

- Đồng ý nguyên đề xuất: đổi `[ ]` thành `[x]` ở cột **Duyệt** của dòng.
- Muốn điều chỉnh: ghi phương án trong cột **Ý kiến / phương án khác**.
  Nếu vừa tích vừa ghi khác đề xuất, agent phải đọc ý kiến, không bỏ qua.
- Chưa quyết định hoặc muốn bàn sâu hơn: để trống ô, ghi điều cần bàn.
- Có thể duyệt từng mục hoặc cả gói bằng cách tích các dòng trong gói đó;
  không cần trả lời lại từng mục trên chat.
- Sau khi sửa, nhắn **“Tôi đã cập nhật E04_REMAINING_REVIEW.md”**.
  Agent đọc file thực tế, ghi nhận đúng phạm vi/phương án đã duyệt vào ADR/
  Project Memory và đồng bộ E04; không tự tích hộ những dòng còn trống.
- Dấu duyệt áp dụng đề xuất **đang hiển thị trong file này**, không áp dụng
  mọi chi tiết được viết thêm sau đó. Endpoint bên dưới là đề xuất contract;
  exact DTO/guard manifests/SQL signatures phải có trong bản E04 tổng hợp
  thực tế để bạn rà, không hứa tài liệu chưa tồn tại đã được nghiệm thu.
- **OWNER_REVIEW.md vẫn giữ ô nghiệm thu E04/E05 duy nhất.** File mới là
  ngoại lệ được bạn yêu cầu cho duyệt từng gói, không sao chép hai cổng đó
  hoặc thay dấu/ghi chú D12/D13.
- Đồng ý thiết kế không cho phép code, migration, cài package, nối agent,
  Git/scaffold, mở tunnel, backup thật hoặc công khai web.

Giả định làm việc giữ nguyên: một trung tâm,7 vai trò, UI tiếng Việt;
mốc kiểm chứng10.000 Student/50 đồng thời chưa được đo; laptop chạy theo
buổi test/demo, không24/7; bạn vận hành; dữ liệu giả cho dev/test/demo.
D12/D13 tiếp tục hoãn theo ghi chú gốc. Không có giả định quyền/nghiệp vụ mới.

## 2. Đã chốt — chỉ để định vị, không duyệt lại

| Phần                                                                     | Trạng thái                                       |
| ------------------------------------------------------------------------ | ------------------------------------------------ |
| E01 chức năng / E02 workflow / E03 database                              | Đã nghiệm thu bản thực tế; stages1–3 done.       |
| Gói1 API chung                                                           | Phần đã trình duyệt ADR-139.                     |
| Gói2a Auth / 2b Administration                                           | Phần đã trình duyệt ADR-140–141.                 |
| Gói3a Students / 3b Student Admissions / 3c Admissions–Finance           | Phần đã trình duyệt ADR-142–144.                 |
| Gói4a Ca thi–chấm đầu vào / 4b Cấu hình / 4c Class–xếp lớp / 4d Schedule | Phần đã trình duyệt ADR-145–148.                 |
| Gói4e Điểm danh một LESSON / điểm nháp một học viên                      | Bạn đã đồng ý; ghi ADR-149.                      |
| E04 / E05                                                                | Chưa nghiệm thu; không suy từ các gói đã đồng ý. |

Các chi tiết bổ sung chưa từng trình trong E04-v0.1 vẫn cần rà bản tổng hợp.
FE01–FE19, quyền xem, cửa sổ nhập, số buổi, /100, missing≠0/Vắng,
whole-Class workflow, audit/replay/backup/runtime/người vận hành là đầu vào
đã duyệt, không phải câu hỏi mới.

## 3. Tổng quan các gói còn lại

| Gói | Mục duyệt | Phần cần hoàn thiện                                               | Thứ tự hợp lý                                              |
| --- | --------- | ----------------------------------------------------------------- | ---------------------------------------------------------- |
| 4f  | 4F01–4F04 | Gửi/gửi lại/trả/chốt cả lớp và Student đọc điểm FINALIZED         | Sau4e.                                                     |
| 5   | 501–503   | API báo cáo tuyển sinh/đào tạo/tài chính và CSV Accountant        | Sau dữ liệu/grade workflow.                                |
| 6   | 601–604   | Contract frontend: route, server-state, form/intent, phiên/secret | Bám API và FE đã chốt.                                     |
| 7   | 701–705   | Contract backend–SQL, DTO/version/replay/audit/permissions        | Đối chiếu tất cả commands/reads.                           |
| 8   | 801–804   | Hồ sơ vận hành laptop, cấu hình an toàn, backup và runbook        | Soạn nguyên tắc; lựa chọn môi trường D12/D13 chưa tự chốt. |
| 9   | 901–903   | Trace, thiết kế kiểm chứng và quy trình tổng hợp nghiệm thu       | Sau khi các gói trên nhất quán.                            |

Bạn có thể đọc/điền tất cả trong một lần. Thứ tự là phụ thuộc thiết kế,
không phải sprint, slice, lịch code hay cam kết số ngày triển khai.

## 4. Gói4f — Gửi, trả lại, chốt và xem bảng điểm

- Kế thừa W18/W19, NV20/NV25–26/NV38–39, F27/F23/F12; E03 TX12/FN03/FN05/
  SP03. Không hỏi lại whole-Class, điểm đạt≥50, frozen weights hoặc FINALIZED
  bất biến. Chỉ xin duyệt cách tổ chức contract và phản hồi API.

| Mã / phần                  | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                        | Duyệt | Ý kiến / phương án khác |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------- |
| 4F01 — Teacher gửi/gửi lại | Read-only preview toàn Class trả mã lớp/cohort/state/missing components/versions; hai command riêng Gửi và Gửi lại, không Student selection. Frontend xử lý mọi local unsaved edit trước preview; Class COMPLETED, đủ mọi học viên/component, current Teacher, đúng report states mới ghi. Không tự gửi khi đủ điểm, không auto resubmit, không thêm lý do Teacher khi chưa có policy.                  | [x]   |                         |
| 4F02 — Giáo vụ review      | Danh sách lớp và chi tiết review đúng quyền qua SP03; không DRAFT values. Preview action RETURN hoặc FINALIZE, rồi command riêng. RETURN bắt buộc lý do/giữ scores-comment; FINALIZE chốt/công bố cả cohort cùng lúc. Một confirmation có Class/count/state/effects; không checkbox chọn vài học viên/duyệt hai tầng/reopen/override.                                                                   | [x]   |                         |
| 4F03 — Student xem kết quả | Hai reads own FINALIZED list/detail, chọn đúng lượt học khi học nhiều khóa/học lại. Server trả component/weights/scores/comment/final_score/pass đúng scope; không classmates, DRAFT/SUBMITTED/RETURNED values hoặc export/PDF mới. Trước công bố chỉ hiển thị thông báo chưa có kết quả được công bố khi cần, không gọi total nháp là điểm chính thức.                                                 | [x]   |                         |
| 4F04 — Cohort/TX/replay    | Cả4 commands submit/resubmit/return/finalize dùng UUID Idempotency-Key và full Class–Assignment–GradeReport context, không page subset. TX12 all-or-none statuses/history/audit/completion; finalize tính decimal/half-up một lần, lưu final_score và cập nhật Registration.is_unfinished cùng TX. Giữ mỗi lần workflow theo operation_id; 409 rà lại, unknown tra caller result, no automatic effects. | [x]   |                         |

### Contract minh họa của Gói4f

Mọi path dưới /api/v1; đây là các routes đề xuất để hoàn thiện inventory:

| Nhóm                  | Read/preview                                                                                                                             | Command                                                      |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Teacher hiện assigned | POST /academics/teachers/me/classes/{id}/grade-submission-preview với action SUBMIT/RESUBMIT                                             | POST .../{id}/submit-grades; POST .../{id}/resubmit-grades   |
| AcademicAffairs       | GET /academics/grade-reviews; GET /academics/grade-reviews/classes/{id}; POST .../classes/{id}/review-preview với action RETURN/FINALIZE | POST .../classes/{id}/return; POST .../classes/{id}/finalize |
| Student own           | GET /students/me/grade-reports; GET /students/me/grade-reports/{id}                                                                      | Không write.                                                 |
| Caller result         | GET /academics/command-results/{requestId} với allowlist tương ứng actor/action                                                          | Không quyền mới từ marker.                                   |

Preview không lưu state/giữ khóa qua user confirmation. Full expected set
gồm đúng cohort report IDs/RVs; Score/comment child save đã chạm parent RV.
Server vẫn kiểm live toàn required components/schedules/states, không tin
client isComplete/total/actor. Không buộc đã FINALIZED để Class COMPLETED;
Class completion và grade publication vẫn là hai workflow riêng.

Lựa chọn khuyến nghị: explicit commands + full-cohort preview.
Generic report PATCH hoặc per-Student submit ngắn hơn nhưng mở đường sai
whole-Class invariant; không chọn. Đánh đổi là payload/context lớn hơn, nhưng
Class tối đa15 học viên và phải recheck transaction, không coi nhỏ là miễn race.

## 5. Gói5 — Reports và CSV

Kế thừa W20, NV29–32, F30/F31, ADR-063–072/094/101/126 và E03 SP04–06.
Tên nhóm báo cáo không cấp report rights cho Admissions/AcademicAffairs/Admin.

| Mã / phần                         | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                            | Duyệt | Ý kiến / phương án khác |
| --------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------- |
| 501 — Báo cáo tài chính           | API role-specific Accountant/Director cùng metric đã chốt: revenue theo paid_at, billed theo issued_at của non-CANCELLED Invoice, current UNPAID debt/overdue riêng không period. Response applied filters/queried_at/whole-set totals/group page; exact money strings. Accountant vẫn dùng Invoice nghiệp vụ qua Gói3c, report không biến thành profile/Invoice dump; Director chỉ nhóm không định danh.   | [x]   |                         |
| 502 — Tuyển sinh/đào tạo Director | Hai API riêng Admissions và Training. Admissions Registration count và distinct Student count theo cùng cohort; overall distinct tính riêng, không sum nhóm. Training trả inventory/current obligations tách finalized-results period; HELD LESSON/expected và FINAL_EXAM riêng, replacement không count đôi. No-data khác0%; defaults/history Course theo ADR đã chốt; không hidden Student detail/export. | [x]   |                         |
| 503 — CSV Accountant              | Một read-only export endpoint chạy fresh SNAPSHOT theo applied financial filters/cột report/toàn kết quả, không UUID mutation marker/job/export store. Kiểm quyền mới, max10.000 dòng hoặc5MB; vượt reject toàn file, không stream phần hợp lệ trước rồi báo thành công. UTF8BOM/comma/CRLF/quote/formula safeguards đúng ADR-101; không DB text rewrite, XLSX/PDF hoặc Director export.                    | [x]   |                         |

Routes đề xuất:
GET /reports/accountant/finance; GET /reports/director/finance;
GET /reports/director/admissions; GET /reports/director/training;
GET /reports/accountant/finance/csv — tất cả dưới /api/v1.

Mỗi report/export nhiều query dùng một SNAPSHOT nhất quán; queried_at từ
server, không historical-as-of. Filters không PII có thể ở query; page/sort
allowlist và ID ổn định. Kỳ ngày VN chuyển UTC boundaries thống nhất; không
lấy billed−period revenue làm debt. UI giữ kết quả cũ kèm applied filters/time
khi đang sửa bộ lọc; query mới lỗi không trình bản cũ như kết quả mới.

Khuyến nghị endpoints theo report/role, có thể dùng chung query service với
projection quyền riêng. Một endpoint trả mọi fields rồi frontend ẩn bớt ít
routes nhưng dễ leak; không chọn. CSV synchronous có giới hạn đơn giản hơn
background jobs, đổi lại phải kiểm kích thước trước phát file và kiểm timeout.

## 6. Gói6 — Frontend contract để triển khai nhất quán

Kế thừa FE01–FE19, không xin duyệt lại menu/sidebar/responsive/page forms/
confirmation/PII/history/calendar/printing/temporary-password UI.
Gói này nối các quyết định UI với API, không tạo mockup hoặc code.

| Mã / phần                    | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                                           | Duyệt | Ý kiến / phương án khác |
| ---------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------- |
| 601 — Route/feature contract | Một route manifest cho F01–F33 trừ F15, paths theo feature/role, list-detail-complex-form URLs hỗ trợ direct link/Back/Forward. Auth context tối thiểu dùng chung shell; menu guards chỉ UX, API vẫn auth. Ghi route→F→role→API→loading/error/empty contract; không staff dashboard mới hoặc thư viện router/cache/form tự thêm.                                                                                           | [x]   |                         |
| 602 — Request/server-state   | Một typed API client chung cho cookie/CSRF/envelope/traceId; trạng thái idle/loading/success/empty/error/unknown tách rõ. Key dữ liệu gồm caller scope/filter/page; discard response cũ khi request mới hoặc phiên đổi. Sau write cập nhật/refetch projections liên quan có quyền, không optimistic update trạng thái quan trọng, background polling giữ phiên hay automatic mutation retry. RAM không là durable storage. | [x]   |                         |
| 603 — Form/preview/intent    | Form giữ draft trong RAM, explicit save; expectedVersions/context cùng dữ liệu đã load. Sửa input invalidate preview. Disable double-click khi pending, UUID một intent; unknown giữ intent/content để tra kết quả, sửa payload là intent mới sau xử lý kết quả cũ. 400 chỉ lỗi trường,409 deliberate reload/review, no merge/overwrite. Unsaved edits warn khi rời; blank không tự0/delete/absent.                        | [x]   |                         |
| 604 — Phiên/CSRF/secret UX   | 401 xóa auth/sensitive RAM/preview/secret và về login;403 phân biệt CSRF_INVALID với role denial, lấy CSRF/context mới theo Auth contract nhưng không tự phát lại write. Logout/session changes giữa tab chỉ signal không secret, dữ liệu scope cũ bị vô hiệu; quyền cuối cùng server kiểm. Temporary password chỉ first response/one-time RAM/explicit copy/clear khi rời đúng FE18; không replay/login credential cache. | [x]   |                         |

Quy tắc frontend không chứng minh commit thất bại khi mạng lỗi. Bảng điểm/
Invoice chỉ lấy projection server; nút/hành động derived không bypass guards.
Unknown signup/profile/password vẫn theo ngoại lệ không replay credential-form
đã chốt; không ép tất cả POST vào UUID workflow.

Khuyến nghị tách **server data**, **local form draft**, **preview context** và
**command intent**. Một state object dùng cho mọi thứ ít cấu trúc hơn nhưng
dễ dùng dữ liệu cũ làm quyền hoặc lặp intent; không chọn. Thư viện cụ thể nếu
cần thêm phải có approval dependency riêng ở thời điểm thích hợp sau baseline,
không dùng việc duyệt file này làm quyền cài package.

## 7. Gói7 — Hợp đồng backend/SQL, replay, audit và quyền DB

Kế thừa E03-v1 và Gói1–4e. Không thay số bảng/fields/objects/grants policy,
không đưa mọi workflow vào SP, không SQL/migration/connection thật.

| Mã / phần                         | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Duyệt | Ý kiến / phương án khác |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----- | ----------------------- |
| 701 — DTO/contract manifest       | Hoàn thiện từng endpoint: role/ownership, request-response/error fields, source row/aggregate tokens, full-set/expected-absence semantics, UUID bắt buộc/không dùng, audit và TX. Base64/version không cấp quyền; field chỉ xuất theo role, stable page/sort. Ghi rõ các chi tiết Gói1–4e chưa trình, không suy toàn draft đã duyệt.                                                                                                                                                                                                                               | [x]   |                         |
| 702 — Canonical replay digest     | Giữ input_digest binary(32)/SHA-256 đã duyệt ở E03 T23, không đổi hash hoặc thêm trường DB. Đề xuất canonical serialization theo DTO đã validate/normalize/allowlist, bao gồm target/expected versions/set/intent content; actor/action từ verified session. Order cố định; set chỉ sort khi semantics không phụ thuộc thứ tự. Không hash raw HTTP JSON hoặc chứa CSRF/JWT/password/temp secret. Marker vẫn completion/ref/mode đúng T23, không response PII snapshot. Exact UTF8 byte representation/contract compatibility trong cửa sổ replay phải trình ở E04. | [x]   |                         |
| 703 — SQL callers/query mapping   | Repository gọi thật SP01–06 đúng paths Admissions/roster/review/reports; SP07 allocator trong approval TX. FN01–06 có callers/parity đã định, không tồn tại chỉ để đủ rubric. Keyless read DTO mappings/parameterization/sort allowlist; actor_id từ verified session, không client tự khai. Trigger tables dùng EF write mapping tương thích; phải có integration evidence sau triển khai, không giả SaveChanges mặc định đã đủ.                                                                                                                                  | [x]   |                         |
| 704 — TX/locks/read consistency   | Command matrix nối mỗi write với TX01–13 và request/aggregate/TIMETABLE gates theo E03§9.1, shared connection/TX cho effects/mandatory audit/replay. Reload/recheck rights/session/time/relationships sau locks; resource set đổi thì rollback/reprepare theo cùng order. Child insert/absence/full cohort không giả Class RV tự thay. Report SNAPSHOT đã chốt; compound preview/detail cần coherent read snapshot ngắn, không giữ qua user confirmation.                                                                                                          | [x]   |                         |
| 705 — Audit/DB privilege contract | Matrix event/allowlisted old-new/reason/operation_id; mandatory fail rollback, pure-security best-effort và retention/IP90 ngày đúng policy. Runtime chỉ grants cần cho real paths, schema/operator riêng;4 SQL demo roles không là7 web roles và không cấp db_owner/sysadmin. Admin audit reads không mở nghiệp vụ khác. Unknown/replay kiểm current rights; không log secret/full payload hoặc app purge/export audit.                                                                                                                                           | [x]   |                         |

Tổ chức đề xuất: Controller bind DTO → Service auth/workflow/TX → Repository
EF/SP/UDF parameterized trên connection caller. Services khác cùng DB có thể
tham gia operation nhưng không tự commit độc lập. Không microservice/event bus
hoặc “generic repository” che loss of transaction ownership.

SHA-256/input_digest và schema T23 là đầu vào đã Accepted, không xin duyệt
lại. Phần đề xuất702 chỉ hoàn thiện canonical serialization/compatibility:
không hash raw JSON vì thứ tự fields/formatting không nhất thiết là intent khác.
Khuyến nghị DTO allowlist có byte representation xác định và fixtures same-
intent/different-intent; giữ tương thích khi marker cũ còn hạn. Đánh đổi là
phải quản lý contract/canonical normalization thay vì serialize DTO tùy ý.
Không thêm key-id/hash algorithm/ReplayOperation field, không hứa digest tự
giải quyết authorization/concurrency. Nguyên tắc loại secrets khỏi logs tham chiếu
[OWASP Logging](https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html).

701/703–705 duyệt cách hoàn thiện contract trong artifact; signature, exact
guard manifest/read snapshot/EF trigger strategy chưa viết phải được owner
đọc ở bản tổng hợp, không coi dấu tick là duyệt mọi refinement tương lai.
Không thay grants thực tế hoặc tạo SQL users tại lượt này.

## 8. Gói8 — Hồ sơ vận hành laptop và bảo mật cấu hình

Kế thừa [SELF_HOSTED_DEMO](../design/SELF_HOSTED_DEMO.md), ADR-114–120.
Đây là **thiết kế runbook/config boundaries**, không cài đặt/provisioning.
Không chọn nhà cung cấp tunnel, public domain, chứng chỉ, port/firewall hay
cam kết uptime; D12/D13 vẫn hoãn đến khi nơi chạy/phần mềm được chốt.
Khi đủ bối cảnh, các lựa chọn cụ thể trở về đúng D12/D13 trong OWNER_REVIEW,
không dùng dòng801 để lách hai mục đó.

| Mã / phần                         | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                                                                              | Duyệt | Ý kiến / phương án khác |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------- |
| 801 — Mô hình cùng origin         | Soạn hồ sơ dev/demo khác nhau: dev vẫn frontend/API cùng origin qua cấu hình phù hợp; demo đề xuất ASP.NET phục vụ SPA build và /api/v1, frontend gọi đường dẫn tương đối. Chỉ web được đưa qua cổng truy cập đã được phép, SQL không public. LAN/tunnel là profile thiết kế chờ D13, không tự bật. Link thay đổi thì dùng phiên/login mới, không tắt Secure/CSRF để chữa lỗi.                                                                                | [x]   |                         |
| 802 — Config/proxy/timeout/health | Config typed và validate khi startup: timezone VN, trusted origin/proxy, JWT/CSRF/DB/crypto keys ngoài repo, exact target runtime theo ADR-119. Fail closed thiếu config; readiness không leak DB/secrets hoặc cập nhật idle. Khóa/query/request có deadline hữu hạn, cancel/timeout→unknown/lookup khi cần; không giữ transaction chờ user. Số timeout/ports/proxy addresses và benchmark cần hồ sơ môi trường trước khi chốt, không tự điền ngưỡng/SLO D12. | [x]   |                         |
| 803 — Backup/recovery hồ sơ       | Giữ daily encrypted SQL backup/7 ngày4 tuần/monthly restore/RPO24h-RTO2h là targets đã chốt. Soạn nơi lưu riêng khỏi ổ primary, quyền operator/key tách backup và checklist restore DB test độc lập. Laptop tắt: ghi nhận missed run, kiểm/catch-up khi bật; không gọi backup cũ là fresh hoặc bảo đảm RPO khi đã lỡ. Đường dẫn/dung lượng/media/access chưa có phải owner xác định trước chạy thật; không backup/restore ở lượt thiết kế.                    | [x]   |                         |
| 804 — Runbook demo/bảo trì        | Một checklist trước-trong-sau buổi: kiểm bản build/data giả/config/DB/web/readiness/login7roles trên thiết bị khách; chỉ bật tunnel sau được phép; mất mạng/write unknown tra kết quả; tắt đường public sau demo, theo dõi backup/session/secret. Bạn vận hành theo ADR-120; không role Admin/DBA mới. Dependency/security/blocklist review theo chính sách; rollout/rollback chi tiết chỉ lập sau full baseline và yêu cầu triển khai.                       | [x]   |                         |

Khuyến nghị một origin cho UI/API, vận hành theo buổi và cấu hình tách secrets.
Separate public frontend/API thêm CORS/cookie/proxy phức tạp và không thuộc
quyết định cùng-origin; không chọn. Hosted24/7/VPS hoặc tự mở port DB không
phải scope hiện tại. Duyệt runbook không cho phép phát link công khai.

**Còn cần dữ kiện trước hoạt động:** provider/domain/HTTPS/trusted proxy,
startup-stop/service account/firewall cần D13; availability/performance cụ thể
cần D12; backup media/path/quyền/restore machine phải có hồ sơ thật. Không hỏi
lại các dữ kiện này ngay ở lượt tạo file và không mặc định đã được cung cấp.
Exact patch/package locks chọn tương thích sau baseline, không pin theo suy đoán.

## 9. Gói9 — Truy vết, thiết kế kiểm chứng và tổng hợp

Kế thừa D15/E03 DC01–18/rubric ADR-130. Duyệt kế hoạch **bằng chứng cần có**,
không phải tuyên bố test hoặc benchmark đã chạy.

| Mã / phần                  | Đề xuất để duyệt                                                                                                                                                                                                                                                                                                                                                                                                        | Duyệt | Ý kiến / phương án khác |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | ----------------------- |
| 901 — Trace hoàn chỉnh     | Một matrix37 NV/32 F/21 W→actor/endpoint/DTO→Service/TX→24 tables/SQL objects→UI/errors→verification case. Chỉ rõ removed NV10/11/F15, deferred/out-of-scope và E04 refinements chưa rà. Mỗi source obligation có path thật dự kiến, không ép một SP/NV hoặc đếm file thay độ phủ. E05 sẽ rà matrix thực tế; hiện chưa tạo complete trace mới.                                                                          | [x]   |                         |
| 902 — Bộ ca kiểm chứng     | Thiết kế xUnit nghiệp vụ/API, SQL Server test riêng/race hai connection/forced failure/audit-replay/trigger-EF/grants-SP-UDF/index before-after theo rubric. All7 roles/ownership/list-detail-preview-replay-export, clocks/decimals/missing/whole-Class/cohort/no partial/secret retention; browser375/768/1280 và Chrome/Edge khi triển khai. Data giả tách DB chính; không framework mới/code tests hoặc claim pass. | [x]   |                         |
| 903 — Quy trình đóng draft | Sau ghi nhận các dòng được duyệt, tổng hợp E04 đủ endpoint/DTO/mapping/frontend/runtime/verification, đối chiếu những chi tiết cũ chưa trình. Mọi gap/mâu thuẫn liệt kê để xử lý hoặc owner loại/hoãn rõ; trình bản E04 thực tế để nghiệm thu riêng trong OWNER_REVIEW. Sau E04 mới rà toàn baseline/E05 thực tế; không tick E04/E05 từ23 dòng này và không nhảy sang implementation plan.                              | [x]   |                         |

Khuyến nghị coverage matrix + risk-based verification, tận dụng DC/TX/rubric
đã chốt. Chỉ kiểm “happy path” dễ làm nhanh nhưng không chứng minh scope,
race, replay hoặc rollback. Chạy full load/browser/SQL ở giai đoạn thiết kế
không có ứng dụng không phải bằng chứng; mọi execution đợi authority sau này.

903 duyệt **quy trình**, không nghiệm thu tài liệu chưa tồn tại. NFR targets/
backup/SQL counts vẫn cần đo chứng minh sau triển khai; E04 là chốt thiết kế,
không biến thành bài kiểm tra runtime hiện tại hoặc cấp quyền vận hành.

## 10. Sau các gói còn lại, còn hai cổng nghiệm thu

| Cổng                | Cần có trước khi chủ dự án duyệt                                                                                                                                                                                                                                   | Nơi duyệt                        |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------- |
| E04 — Kiến trúc/API | SYSTEM_ARCHITECTURE_SPEC hoàn chỉnh thực tế, contract inventory/mapping/guards/frontend/runtime/verification; gap/deferred rõ và nhất quán E01–E03. Các chi tiết chưa thể chốt vì D12/D13 phải ghi minh bạch, không claim đã đầy đủ nếu còn prerequisite bắt buộc. | Ô E04 ở OWNER_REVIEW, chưa tích. |
| E05 — Toàn baseline | Đầu ra rà requirements→functions→workflows→DB→architecture, matrix và mọi mâu thuẫn/phần loại-hoãn được owner xử lý.                                                                                                                                               | Ô E05 ở OWNER_REVIEW, chưa tích. |

Chỉ sau nghiệm thu toàn baseline mới bàn kế hoạch triển khai nếu bạn yêu cầu.
Không có thêm gói nghiệp vụ mới mặc định ngoài danh sách này; nếu rà thấy gap
thực sự, agent nêu cụ thể/phạm vi/ảnh hưởng để bạn quyết định, không tự kéo dài
bằng các gói chưa có lý do.

## 11. Nhật ký ghi nhận bảng duyệt

Chỉ agent cập nhật mục này **sau khi đọc thay đổi của owner**. Giữ nguyên
checkbox/ý kiến của owner; không chép transcript, secret hoặc temporary logs.

| Ngày       | Mục đã ghi nhận                                                                              | Quyết định / nơi đồng bộ                       |
| ---------- | -------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| 01/10/2026 | Tạo E04-R1 theo yêu cầu owner;23 ô mới đều trống. Gói4e đã duyệt riêng, không nằm trong23 ô. | Gói4e ADR-149; các gói4f/5/6/7/8/9 chưa duyệt. |
| 01/10/2026 | Owner tích đủ23 ô và xác nhận trực tiếp đồng ý toàn bộ nội dung đang hiển thị; không có phương án khác. | 4F01–4F04→ADR-150;501–503→151;601–604→152;701–705→153;801–804→154;901–903→155. Đồng bộ E04/requirements/Project Memory; E04/E05 chưa tích, D12/D13 hoãn. |
