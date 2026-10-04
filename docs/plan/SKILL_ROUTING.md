# Skill routing và compatibility — áp dụng khi viết code

Cập nhật04/10/2026. Owner yêu cầu rà skill và bảo đảm agent dùng skill phù hợp
mỗi lần viết code. Đã đọc AGENTS và toàn bộ4local SKILL.md, cùng antigravity-worker.
Đây là quy trình chọn skill cho task, không mở workflow kết nối/delegation/code.

## 1. Kết quả rà scope

| Skill | Kết luận | Task và vai trò áp dụng |
| --- | --- | --- |
| [backend-clean-code](../../.agents/skills/backend-clean-code/SKILL.md) | Phù hợp với compatibility overrides bên dưới | Mọi sửa backend, DTO/API docs, entities/migrations/SQL integration, auth/validation; implementer đọc và áp dụng, reviewer kiểm. |
| [frontend-ui](../../.agents/skills/frontend-ui/SKILL.md) | Phù hợp với compatibility overrides | Mọi sửa frontend/components/styles/forms/HTTP/UI integration; implementer đọc và áp dụng, reviewer kiểm. |
| [code-review](../../.agents/skills/code-review/SKILL.md) | Phù hợp cho reviewer Codex | Review mọi task code trước done và integrated slice trước nghiệm thu; build/tests/checks do Codex chạy, không giao worker tự nghiệm thu. |
| antigravity-worker — C:/Users/asus/.codex/skills/antigravity-worker/SKILL.md | Phù hợp cho delegation có phạm vi | Chỉ Codex manager khi dùng AGY; là integration protocol, không thay backend/frontend skill. Analyze verified, edit chưa chạy. |
| [codex-with-chatgpt](../../.agents/skills/codex-with-chatgpt/SKILL.md) | Tùy chọn, không tự dùng trong workflow hiện tại | Chỉ khi owner gọi C2C; phải xác định lại integration scope/data access. Không tự kết nối/tunnel/cài/update hoặc chuyển manager khỏi Codex. |

Không chọn thêm skill từ catalog lớn chỉ vì cùng từ khóa. Một task full-stack
có thể cần cả backend và frontend skill; nếu quá lớn thì chia trước dispatch.
P01 planning/read-only không cần ép code skill; security/SQL review vẫn dùng
nguồn E03/E04 chính thức. Không tự cài skill `project-structure` hoặc
`ui-ux-pro-max` vì checklist nhắc tên; layout/tokens đã có accepted source.

## 2. Thứ tự authority và compatibility overrides

User instructions/session authorization và canonical accepted requirements/E01–E05
được giữ. Skills chỉ áp dụng phần tương thích, không tự đổi contract. Khi có
mâu thuẫn mới chưa được giải quyết trong nguồn, Codex ghi rõ và xin quyết định
thật sự cần; không tự lấy một checklist cũ làm quyền đảo ADR. Source:
[requirements](../requirements.md), [E04 final](../design/E04_FINAL_CONTRACT.md),
[DTO](../design/E04_DTO_SPEC.md), [SQL](../design/E04_SQL_INTEGRATION_SPEC.md),
[runtime](../design/E04_RUNTIME_PROFILE.md), [architecture layout](../ARCHITECTURE.md).

| Skill wording cần điều chỉnh | Effective rule từ baseline |
| --- | --- |
| JWT bearer/token handling | JWT qua HttpOnly/Secure cookie với server session/revocation, same-origin/framework CSRF/Origin. Browser không giữ/đọc JWT; auth state và CSRF request token chỉ RAM, restricted scope/current rights bắt buộc. ADR099/140/160. |
| PasswordHasher hoặc BCrypt | Dùng ASP.NET Core PasswordHasher ADR114, không tự thêm BCrypt/package hoặc sửa quy tắc password. |
| SQLite in-memory service tests | Pure unit tests có thể không dùng DB; SQL-specific evidence dùng SQL2022 Developer thật. SQLite không chứng minh SQL locks/RV/constraints/trigger/SP/grants/snapshot. |
| Every list paged | Các list/search được phân trang theo contract; full guard/cohort/component sets và preview/save units phải đủ theo exact DTO. Không ép page vào whole-set context. |
| Plural REST/DataAnnotations generic rules | Routes và method/DTO đã chốt là exact source; không đổi tên endpoint để hợp guideline. Edge validators phải giữ strict scalar/presence/NULL/parser/error precedence; DataAnnotations không thay thế E04 validation. |
| LINQ-only/UTC/money18 guidance | Parameterized EF/raw-SQL/SP/UDF/views/triggers theo E03/E04 được dùng đúng caller/TX. UTC cho event, date/time Việt Nam giữ accepted shapes; tiền đơn18,0/tổng38,0/API digit strings; không ép tổng vào .NET decimal. |
| CORS frontend-origin as default | Giữ same-origin architecture; không tự thêm cross-origin để dễ chạy. Concrete origins/HTTPS/proxy theo authorized profile, không bypass Secure/CSRF. |
| Money via Intl.NumberFormat | Giữ exact digit strings; không Number-convert tiền lớn làm mất chính xác. Vietnamese formatting theo accepted representation; kiểm boundaries/import/export. |
| External font/tokens file wins/40px touches | Accepted system font/local FE direction và khoảng44px target; không skill/token file nào override accepted FE. Không tự tải font hoặc thêm design tool/library. |
| Add Student button/generic confirmations | Student tự đăng ký; không Admissions tạo hộ. Chỉ render đúng role/save unit/actions và sensitive-confirmation contract; không thêm workflow hoặc extra confirmation layers. |
| docs/api.md/Swagger for every route change | Giữ154accepted catalog/DTO; implementation API docs/annotations chỉ khi đúng task scope và package manifest. Docs path phải explicit trong contract; owner baseline không bị worker sửa để khớp bug. |
| Review migration/notes/checklist for all changes | Migration khi thực sự đổi schema; không yêu cầu migration cho UI-only task. Reviewer đánh dấu N/A có lý do, dùng task card cùng slice/source trace. Giải thích code ngắn cho owner; không trộn task-done và slice-done. |
| C2C owns planning/review, auto repair/setup | Workflow hiện tại Codex là manager; C2C không tự chạy hoặc nhận authority từ việc audit/installation. Nếu owner gọi sau này, role/scope/consent phải cụ thể; không tự mở network/update từ policy này. |

Giữ nguyên các SKILL.md đã cài. Các overrides được đặt ở AGENTS/chính sách này
để mọi agent biết trước khi dùng; không giả rằng văn bản skill cũ đã được sửa.

## 3. Preflight bắt buộc trước mỗi task viết code

Task card phải ghi: `implementation_skills`, `review_skills`, `skill_sources`,
`applicable_overrides`, `read_context`, `skill_verification` cùng scope/checks.

1. Codex chọn theo loại thay đổi, đọc đầy đủ skill lần đầu dùng trong session,
   re-read khi nội dung thay đổi; thông báo user tên skill được dùng lần đầu.
2. Ghi path và revision/hash skill trong task card; thêm policy cùng canonical
   references cần cho task. Không chỉ ghi tên rồi đoán instructions.
3. Nếu có worker, English objective yêu cầu worker đọc toàn bộ selected local
   SKILL.md và policy trước edit, nêu overrides cụ thể. Thêm paths đó vào
   `context_files` và explicit read allowlist; không đọc links ngoài scope.
4. Kiểm data-sharing consent/phạm vi external đọc tương ứng. Smoke consent8files
   không là blanket consent cho local skills/toàn source trong mọi future job.
   Khi chưa đủ read scope, Codex hoàn thiện contract, chưa dispatch.
5. Giữ `allowed_paths`/`forbidden_paths`/caps; skill không mở thêm quyền sửa/docs,
   chạy command, cài packages, connect hoặc merge. AGY giữ no validation commands.
6. Review skill áp cho Codex; không yêu cầu worker tự chạy code-review checks.

Runner schema hiện có không có skill enforcement riêng. Task-card metadata ở
trên không tự biến thành runner guard: embed requirements vào objective/criteria
và context_files hợp lệ. Đây là manager preflight/review gate; tool-event read
checks không phải OS-enforced read sandbox hoặc bằng chứng tuyệt đối mọi line
đã được hiểu. Không quảng cáo automatic skill enforcement khi chưa có.

## 4. Mapping M0 task families

| Task | Implementation skills | Review khi có code |
| --- | --- | --- |
| P01 profile/inventory/planning | Không bắt buộc code skill; E04 runtime trực tiếp | Rà tài liệu/prerequisites, chưa code-review execution. |
| P02,P04–P08 | backend-clean-code | code-review và backend overrides. |
| P03 | frontend-ui | code-review và frontend overrides. |
| D01–D13,A01–A08,R01–R03 | backend-clean-code | code-review; real SQL evidence tại task liên quan. |
| U01–U06 | frontend-ui; thêm backend-clean-code nếu task thật sự sửa backend | code-review và relevant UI/Auth/API sources. |
| V01 | Không generation task; cả2implementation skills làm checklist theo scope | Codex code-review, integrated SQL/API/browser evidence. |

Nếu bất kỳ task trên dùng AGY, Codex thêm antigravity-worker cho protocol nhưng
worker vẫn dùng implementation skill đúng phạm vi. Không mặc định mọi task giao
AGY; nhỏ hoặc secret-bearing/privileged operations giữ ở Codex.

## 5. Gate kết thúc task

Codex ghi skill sources/overrides thực tế; với worker kiểm read tools/events và
scope cùng receipt/diff. Review nội dung và tự chạy validation tương ứng; lời
worker nói đã dùng skill không đủ bằng chứng. Nếu skill chưa đọc/requirements
bị bỏ qua hoặc worker đọc vượt scope: giữ task review/incomplete/rejected theo
vi phạm, sửa bằng contract hẹp rồi kiểm lại; không đánh done hoặc tự merge.
Review verdict PASS/PASS WITH FIXES/FAIL có unresolved checks; missing required
check không được gọi complete. Áp dụng worker changes vào main vẫn cần owner.
