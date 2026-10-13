# Kế hoạch tân trang giao diện — Trung tâm tiếng Anh

Ngày chốt định hướng: 13/10/2026. Trạng thái: giai đoạn1 chuẩn bị COMPLETE,
review PASS và owner đã duyệt cổng1. Giai đoạn2 COMPLETE/review PASS; owner đã duyệt cổng2 bằng “tiếp tục đi”.
Giai đoạn3 Student pilot COMPLETE/review PASS; owner đã duyệt cổng3 bằng “tiếp tục”.
Giai đoạn4 Student còn lại và giai đoạn5 màn nhân sự COMPLETE/review PASS.
Giai đoạn6 COMPLETE/review PASS/applied ADR163/main reverified; cổng4 owner xem đầu ra tổng thể còn chờ.
Be Vietnam Pro và lucide đã cài, license và token contrast đã kiểm chứng.
Nguồn: kế hoạch owner gửi trong chat và các câu trả lời 1A, 2A, 3A, 4A, 5A, 6A.
File này là hợp đồng định hướng cho UI refresh. Các cổng bên dưới duyệt đầu ra
cụ thể, không hỏi lại các lựa chọn đã chốt.

## 1. Mục tiêu và phạm vi

Giao diện tiếng Việt trẻ trung, rõ ràng, phù hợp học viên; màn nhân sự gọn,
ưu tiên hiệu quả thao tác. Giữ tên tạm **Trung tâm tiếng Anh**, wordmark chữ
kèm biểu tượng hình học đơn giản, để có thể thay thương hiệu sau.
Chưa thêm thông tin, tài khoản hoặc quyền riêng cho phụ huynh.

Được đổi style, bố cục, component trình bày và vị trí điều hướng. Owner cho
phép gộp điều hướng trùng, tự tải dữ liệu khi vào màn và đặt nút kiểm tra phiên
trong khu vực phục hồi lỗi. Lập bản đồ tương tác cụ thể tại giai đoạn 0 để
duyệt tại cổng 1; không cần hỏi lại quyền cho ba hướng cải thiện này.

Giữ nguyên route, endpoint, DTO/tên field, id/data-testid, validation, quyền,
ownership, nghiệp vụ, các save unit và hợp đồng error/replay. Không sửa backend,
schema/grants/custody, không tạo dữ liệu mẫu vào database vận hành.
Giữ xử lý dữ liệu chưa lưu, phiên hạn chế, logout/expiry/tab signals, RAM cleanup,
request epoch, CSRF và unknown/manual lookup; không tự gửi lại mutation, kéo dài
phiên hoặc đưa dữ liệu nhạy cảm vào URL/storage/cache. Tự tải chỉ là đọc có
scope qua client hiện có, không background polling/heartbeat hoặc write tự động.
Không thay read không gia hạn idle thành thao tác gia hạn phiên.

M0–M3 và M4 báo cáo/audit đã hoàn tất theo evidence hiện có. UI refresh là
phạm vi riêng; M4 backup/recovery vẫn chờ operator inputs, không được coi là đã
đóng bởi việc làm đẹp giao diện.

## 2. Hướng thiết kế đã chốt

- Bảng A: teal và vàng nắng. Primary dự kiến #0F766E, accent #FBBF24 với chữ
  tối, chữ slate #1E293B, nền sáng hơi ấm. Đây là đầu vào token; kiểm contrast
  thực tế của mọi trạng thái trước khi chốt styleguide. Semantic success,
  warning, danger, information tách khỏi màu thương hiệu.
- Font Be Vietnam Pro, tự lưu bằng @fontsource, weight 400/500/600/700; chọn
  subset hỗ trợ cả chữ Việt và Latin cần dùng. Type scale 14/16/20/24/32/40,
  line-height thân bài >=1.5; không cắt dấu.
- Token spacing bội 4px; radius 8px cho input/nút, 12px cho card/modal,
  pill cho badge/avatar; tối đa hai mức shadow nhẹ. Icon lucide thống nhất
  theo token kích thước/nét. Hover/focus/press 150–200ms, reduced motion;
  không animation cấp trang, gradient, emoji icon, glassmorphism hoặc mascot.
- SVG cùng phong cách, tối đa 4–5 hình cho auth/home/empty. Kiểm giấy phép
  từng nguồn/asset trước khi đưa vào repo, lưu provenance và attribution khi
  cần; không suy mọi bộ SVG đều cho phép recolor/redistribute. Ưu tiên file
  local, decorative SVG không tạo thông tin/thống kê giả. Không bitmap AI.
- Lấy cảm hứng từ phân cấp/điều hướng của arizona.edu và card/status của
  leetcode.com; không sao chép ảnh lớn, thương hiệu hoặc dashboard dày đặc.

## 3. Khung và component

Học viên: thanh điều hướng trên desktop, menu drawer trên mobile; không tab
dưới. Nhân sự: sidebar thu gọn được. Account menu chứa thông tin cá nhân chỉ
Student, đổi mật khẩu và logout theo quyền. Một nguồn cấu hình điều hướng,
không giữ hai hàng điều hướng lặp. Giữ điểm đến và các view hiện có; inventory
phải phân biệt URL navigation với local-state navigation trước khi nối menu.

Đăng nhập/đăng ký: desktop bố cục thương hiệu/minh họa và form; mobile đầu
trang gọn cùng form. Màn có mật khẩu giữ nguyên exact raw input, validation,
temporary credential/CSRF và quy tắc clear secret.

Tất cả vai trò dùng được ở **375/768/1280**. Màn nhân sự ưu tiên desktop nhưng
responsive vẫn bắt buộc. Bảng được cuộn trong container riêng hoặc đổi sang
card có nhãn, không làm toàn trang tràn ngang; chức năng/dữ liệu không bị mất.

Tailwind hiện có là v4.3.3. Nâng cấp `styles/tokens.css`, Tailwind theme và
component nền hiện có; không khởi tạo UI kit mới hoặc thay stack để tân trang.
Owner đã chọn thêm Be Vietnam Pro và lucide khi triển khai phần nền. Chỉ thêm
shadcn/Radix/Recharts hoặc dependency khác khi có nhu cầu cụ thể được duyệt;
không cài toàn bộ danh sách thư viện trong bản kế hoạch gốc. Ưu tiên công cụ
Edge/browser acceptance hiện có; Playwright mới không là prerequisite và chưa
được chọn để cài. Khóa phiên bản dependency thực tế trong task card.

Component dùng chung dự kiến: Button, Input/TextField, Select, Textarea,
FormField, Card, Badge, Table, Dialog/Modal, Tabs, Alert/Feedback, PageHeader,
EmptyState, Skeleton, Pagination, Avatar; Toast chỉ khi cần, không là nơi duy
nhất hiển thị lỗi quan trọng. Giữ props/DOM contracts mà callers/tests phụ thuộc.
Thiếu component nền: báo và bổ sung ở giai đoạn nền, không tạo bản riêng mỗi trang.

## 4. Giai đoạn và bốn cổng duyệt

Owner13/10/2026 giao chia thành các giai đoạn đủ lớn và hoàn thành giai đoạn
đầu. Không chia theo từng component hay ngày làm việc. Mỗi giai đoạn đóng một
kết quả tích hợp, có thể gồm nhiều nhóm commit nhỏ nhưng không giảm kiểm chứng.

| Giai đoạn thực thi | Phạm vi đủ lớn / đầu ra kết thúc | Cổng |
|---|---|---|
| 1 — Chuẩn bị và đóng băng hành vi (giai đoạn0 gốc) | Inventory32F/7roles, route-view-action mapping, baseline tests, ảnh before, đề xuất tương tác và risk/coverage register | Cổng1 |
| 2 — Design system + khung + xác thực (giai đoạn1–2 gốc) | Token/font/icon/component dùng chung, dev styleguide, Student/staff shell, login/signup/password/recovery đã tích hợp | Cổng2 |
| 3 — Lát cắt học viên mẫu (3a) | Home/course/Registration/booking/chi tiết với đầy đủ states, pending/unknown, responsive và tests | Cổng3 |
| 4 — Toàn bộ cổng học viên còn lại (3b) | Invoice/print, lớp/lịch, published results, own profile/password nhất quán với mẫu; hồi quy Student | Theo mẫu đã duyệt |
| 5 — Toàn bộ màn nội bộ (4) | Admissions/AcademicAffairs/Teacher/Accountant/Admin/Director theo pattern, tất cả3widths; hồi quy7roles | Theo mẫu đã duyệt |
| 6 — Nghiệm thu và bàn giao (5) | Cross-role/full-flow checks, accessibility, ảnh after/so sánh, dọn scope, review/apply/main reverify và hướng dẫn | Cổng4 |

Giai đoạn1 được giao và đã hoàn tất: UI_REFRESH_INVENTORY.md,
UI_REFRESH_STAGE_1_REVIEW.md và UI-refresh-evidence/index.html. Giai đoạn2
đã được owner mở bằng phản hồi “ok tôi duyệt”13/10/2026. Owner “tiếp tục đi”
duyệt cổng2 và giao giai đoạn3; kết quả UI_REFRESH_STAGE_3_REVIEW.md PASS,
178frontend/21Edge/StrictMode và178ảnh scratch+18ảnh preview; cổng3 được owner duyệt bằng “tiếp tục”; giai đoạn4 đã review PASS
181frontend/21Edge,593ảnh; xem UI_REFRESH_STAGE_4_REVIEW.md.

### Giai đoạn 0 — inventory và đóng băng hành vi

- Kiểm working tree; bảo toàn thay đổi owner đang có. Tạo nhánh riêng
  `ui-refresh`/checkout sạch phù hợp khi bắt đầu thực thi, không reset hoặc
  stage các file owner. Mỗi giai đoạn có nhóm commit riêng.
- Chạy baseline build/lint/typecheck/frontend tests và relevant acceptance
  bằng targets hiện có. Ghi commands, baseline source, PASS/FAIL/SKIP đúng nghĩa;
  không lấy số test cũ làm kết quả mới. Không rerun D07 chỉ vì UI refresh.
- Tạo `docs/plan/UI_REFRESH_INVENTORY.md`: F01–F33, F15 removed, từng vai trò,
  route/view/component, dạng trang, action, trạng thái, selector và test hiện có.
  F là screen code, không giả mỗi F tương ứng một file/URL.
- Ghi dữ liệu/API mà home thực sự có, pagination và giới hạn scope. Không
  tổng hợp các trang chưa tải thành thống kê toàn tập hoặc thông báo mới.
- Kiểm kê hai nút và các recovery controls: “Đọc trang chủ” hiện tải loadHome;
  “Kiểm tra lại phiên” gọi me/CSRF và phục hồi ngữ cảnh, không phải debug thuần.
  Đề xuất initial-load/refetch và vị trí recovery với tương tác/session giữ đúng.
- Đề xuất gộp menu dựa trên route/view mapping, phân biệt hồ sơ và lịch thi.
- Chụp before các màn/trạng thái tại ba widths trong môi trường scratch có
  dữ liệu synthetic và bảy vai trò, tách khỏi operational catalog; không dùng
  credential thật trong ảnh/evidence. Chuẩn bị capture workflow từ harness
  hiện có; ghi các màn chưa chụp được và lý do, không claim đủ coverage.

**Cổng 1:** owner xem inventory, baseline/before và bản đồ tương tác/menu.
Chỉ sau phản hồi mới thực hiện phần nền/layout phụ thuộc đầu ra đã duyệt.

### Giai đoạn 1–2 — nền thiết kế, khung và xác thực

- Token/theme/font/icon, nâng cấp component dùng chung và tài liệu
  `docs/plan/UI_REFRESH_DESIGN_SYSTEM.md`.
- Styleguide dev-only bằng mechanism hiện có, không thêm production route
  hoặc expose fixture. Hiển thị biến thể, states, focus và contrast evidence.
- Dựng khung Student/staff và auth theo mục 3; kiểm drawer keyboard/focus
  restoration, menu quyền, raw password, login/restricted/logout/unknown.
- Chụp token/component/auth/shell ở ba widths, kiểm build/lint/types/tests.

**Cổng 2:** owner xem styleguide và khung/auth. Bảng A đã chọn; duyệt cách
hiện thực cụ thể, không hỏi lại A/B/C.

### Giai đoạn 3a — lát cắt học viên mẫu

Trang chủ, Chọn khóa học, Hồ sơ/đặt ca/chi tiết: minh họa dashboard/list/form/
detail dựa trên dữ liệu hiện có. Home dùng thẻ từng Registration, current step,
eligible action, next event, unpaid invoice và published result theo scope;
lời chào thân thiện, không tiến độ/thống kê bịa hoặc điểm chưa FINALIZED.
Kiểm state/interaction/audit-replay invariants và ba widths bằng evidence thực.

**Cổng 3:** owner duyệt lát cắt mẫu trước khi nhân rộng.

### Giai đoạn 3b–4 — nhân rộng đã duyệt

Hoàn thiện Invoice/print, Class/calendar/results/profile/password của Student;
sau đó Admissions, AcademicAffairs, Teacher, Accountant, Admin và Director.
Màn nhân sự vẫn kiểm đủ ba widths. Director ưu tiên card và bảng nhóm đúng
quyền; biểu đồ không là yêu cầu bắt buộc và không thêm thư viện tự động.
Tiền chính xác không chuyển sang JavaScript Number để vẽ/hiển thị; không
trộn dữ liệu các queries, không biến no-data thành0 hoặc suy tổng từ page.

Thực hiện theo mẫu đã duyệt, báo tiến độ/ảnh mỗi nhóm, không thêm cổng duyệt
mỗi vai trò. Nếu phát sinh quyết định làm thay đổi hợp đồng, trình đúng vấn
đề cụ thể. Worker chỉ task nhỏ theo file allowlist sau một pilot nhỏ; không
sửa token/component nền/logic ngoài scope. Mỗi delegation cần contract/context
bảo vệ owner files và review độc lập theo chính sách dự án; không dispatch
worker từ việc hoàn thiện kế hoạch này.

### Giai đoạn 5 — nghiệm thu

Rà states/accessibility/responsive, dọn CSS/component thừa trong scope; chụp
after và bảng before/after với route/view/role/state/viewport/source. Chạy
checks phù hợp cho changed callers và browser flows; đủ required evidence
mới review PASS, apply ADR163 và reverify main. Cập nhật memory/runbook liên quan.

**Cổng 4:** owner xem kết quả tổng thể và các giới hạn còn lại.

## 5. Checklist nghiệm thu

- Chỉ dùng semantic token/component nền, không hardcode màu/font/spacing
  trực tiếp trong component. Values đặc biệt phải thuộc token/variant được ghi.
- Loading/empty/error khi áp dụng; form có validation và pending/disabled;
  write có success/conflict/unknown và recovery đúng hợp đồng. Không ép lỗi
  validation vào màn chỉ đọc hoặc tạo state nghiệp vụ mới.
- Label/focus/keyboard rõ; contrast chữ thường >=4.5:1, controls/focus có
  contrast phù hợp và không dựa riêng vào màu; mobile touch targets >=44px.
  Tiếng Việt không cắt dấu, không page overflow tại375px, reduced motion.
- Giữ route/DTO/fields/selectors/rights/session/unsaved/replay/unknown behavior;
  tương tác initial read/menu/recovery đúng đầu ra cổng1. Invoice print và
  one-time credential handoff không rò rỉ/sai trạng thái.
- Build/lint/typecheck/tests và relevant real browser checks PASS; lưu evidence
  trước/sau, source fingerprints và exclusions. Screenshot không thay thế
  functional verification; không sửa test để che regression nghiệp vụ.

## 6. Ưu tiên nguồn và điểm còn phải kiểm chứng

Plan này thay hướng màu/font cũ ADR103/113 và bố cục trình bày trong phạm vi
UI refresh bằng quyết định owner13/10/2026; không thay quyền và invariant
requirements/E03/E04. Theo SKILL_ROUTING, frontend-ui bắt buộc khi viết UI,
code-review trước done; backend-clean-code nếu sau này có backend task riêng.
Các gợi ý stale của skill (React Query/router/money Number) không cấp quyền
thêm package hoặc override exact money/current client/canonical contracts.

Đã chốt tất cả sáu câu hỏi; không còn câu hỏi màu/brand/nav/phụ huynh.
Cần duyệt các outputs bốn cổng; giai đoạn1 đã có baseline UI/tests/source freeze
và ảnh representative. Giai đoạn2 đã hoàn thành design system/auth/shell:174
frontend tests,1438 backend PASS/325intentional opt-in SKIP,21Edge cases PASS;
73ảnh cuối và6ảnh styleguide. Xem UI_REFRESH_STAGE_2_REVIEW.md và evidence/stage2.
Nhánh ui-refresh giữ working tree đã review; frontend/font/icon thay đổi, backend
nghiệp vụ/schema/runtime/custody không đổi. Cổng2 đã duyệt; Student pilot giai đoạn3 đã review PASS; cổng3 đã duyệt; giai đoạn4 đã hoàn thành theo mẫu, không thêm gate riêng.

Giai đoạn5–6 hoàn tất:183frontend/1438backend+325opt-inSKIP/21uniqueEdge main PASS,
genuine LOCAL_DEV PASS; xem UI_REFRESH_STAGE_6_REVIEW/UI_REFRESH_HANDOFF và
UI-refresh-evidence/stage6/before-after.html (30cặp), summary.json. Cổng4 chưa duyệt.
