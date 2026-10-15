# SQ25 — Hướng dẫn vẽ sequence Admin tra cứu nhật ký và nhóm cùng thao tác

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV35; NV39 xuyên suốt; W21; F33; UC09.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: AuditEndpoints.cs](../../../backend/src/EnglishCenter.Api/Api/Administration/AuditEndpoints.cs).
- [Code: SqlAuditViewerExecutor.cs](../../../backend/src/EnglishCenter.Api/Infrastructure/Administration/SqlAuditViewerExecutor.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Admin đọc danh sách audit, chi tiết sự kiện và operation group. Ghi audit của các SQ khác là xử lý nội bộ; Admin không phải người được gọi để ghi log hoặc người duyệt giao dịch.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ25-A — Tra cứu danh sách audit**; actor Admin.
- **SQ25-B — Xem chi tiết sự kiện**; actor Admin.
- **SQ25-C — Xem các sự kiện cùng operation**; actor Admin.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không export/purge/edit/undo log, endpoint ghi log tùy ý hoặc xem lại secret từ nhật ký.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Admin | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện nhật ký hoạt động | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Quyền | Admin NORMAL, kiểm lại quyền trước trả list/detail/group. Quyền audit không cấp quyền truy cập đối tượng nghiệp vụ gốc. |
| Danh sách | Bộ lọc theo hợp đồng hiện hành, newest-first/paging ổn định; không tải toàn bộ log xuống rồi lọc trên client. |
| Chi tiết | Old/new/reason theo allowlist action/entity, actor/time/result/operation metadata được phép; không secret/hash/JWT/raw payload. |
| Nhóm | Các sự kiện liên quan cùng operation_id, phân trang/giới hạn đúng hợp đồng. Đọc một nhóm không giả định từng sự kiện có action giống nhau. |
| Retention | Mandatory business/account history giữ; pure-security/IP retention 90 ngày do vận hành. IP đã hết retention hiện Không còn lưu; app không tự khôi phục. |
| Ghi log | Mandatory mutation audit fail rollback cả thao tác; security-only best-effort riêng. Read audit không tự audit mọi page-view. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Tra cứu danh sách audit

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Admin → Giao diện nhật ký hoạt động | Xem/áp dụng bộ lọc nhật ký | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| A02 | Giao diện nhật ký hoạt động → Hệ thống | Yêu cầu danh sách audit theo bộ lọc và trang hiện tại | Request đọc; bắt đầu activation. |
| A03 | Hệ thống → Hệ thống | Kiểm Admin và bộ lọc/phân trang hợp lệ | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| A04 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Admin hoặc bộ lọc/phân trang sai]`; không đi nhánh query thành công. |
| A05 | Giao diện nhật ký hoạt động → Admin | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| A06 | Hệ thống → Hệ thống | Query audit summaries được phép theo thứ tự ổn định | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| A07 | Hệ thống → Giao diện nhật ký hoạt động | Danh sách/tổng/phân trang đúng bộ lọc | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| A08 | Giao diện nhật ký hoạt động → Admin | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| A09 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| A10 | Giao diện nhật ký hoạt động → Admin | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Admin; G = Giao diện nhật ký hoạt động; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Xem/áp dụng bộ lọc nhật ký
                            |-- A02 --> H : Yêu cầu danh sách audit theo bộ lọc và trang hiện tại
                                                       H --+ A03: Kiểm Admin và bộ lọc/phân trang hợp lệ
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. A05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A06: Query audit summaries được phép theo thứ tự ổn định
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A07 ..| : Danh sách/tổng/phân trang đúng bộ lọc
    N <.. A08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. A09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. A10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### B. Xem chi tiết sự kiện

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Admin → Giao diện nhật ký hoạt động | Chọn xem chi tiết sự kiện | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| B02 | Giao diện nhật ký hoạt động → Hệ thống | Yêu cầu chi tiết audit event đã chọn | Request đọc; bắt đầu activation. |
| B03 | Hệ thống → Hệ thống | Kiểm Admin, event tồn tại và projection fields được phép | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| B04 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Admin hoặc event không truy cập được]`; không đi nhánh query thành công. |
| B05 | Giao diện nhật ký hoạt động → Admin | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| B06 | Hệ thống → Hệ thống | Chiếu old/new/reason/actor/time/result theo action/entity allowlist, áp retention IP | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| B07 | Hệ thống → Giao diện nhật ký hoạt động | Chi tiết không chứa secrets hoặc quyền mở object gốc | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| B08 | Giao diện nhật ký hoạt động → Admin | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| B09 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| B10 | Giao diện nhật ký hoạt động → Admin | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Admin; G = Giao diện nhật ký hoạt động; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Chọn xem chi tiết sự kiện
                            |-- B02 --> H : Yêu cầu chi tiết audit event đã chọn
                                                       H --+ B03: Kiểm Admin, event tồn tại và projection fields được phép
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. B05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B06: Chiếu old/new/reason/actor/time/result theo action/entity allowlist, áp…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B07 ..| : Chi tiết không chứa secrets hoặc quyền mở object gốc
    N <.. B08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. B09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. B10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### C. Xem các sự kiện cùng operation

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Admin → Giao diện nhật ký hoạt động | Chọn xem nhóm cùng thao tác | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| C02 | Giao diện nhật ký hoạt động → Hệ thống | Yêu cầu danh sách sự kiện cùng operation_id với phân trang | Request đọc; bắt đầu activation. |
| C03 | Hệ thống → Hệ thống | Kiểm Admin, operation_id và paging/context hợp lệ | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| C04 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Sai quyền Admin hoặc operation/paging không hợp lệ]`; không đi nhánh query thành công. |
| C05 | Giao diện nhật ký hoạt động → Admin | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| C06 | Hệ thống → Hệ thống | Query các summaries liên quan cùng operation và trả page đúng scope | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| C07 | Hệ thống → Giao diện nhật ký hoạt động | Nhóm sự kiện của thao tác, không thay dữ liệu gốc | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| C08 | Giao diện nhật ký hoạt động → Admin | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| C09 | Hệ thống → Giao diện nhật ký hoạt động | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| C10 | Giao diện nhật ký hoạt động → Admin | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Admin; G = Giao diện nhật ký hoạt động; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Chọn xem nhóm cùng thao tác
                            |-- C02 --> H : Yêu cầu danh sách sự kiện cùng operation_id với phân trang
                                                       H --+ C03: Kiểm Admin, operation_id và paging/context hợp lệ
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. C05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C06: Query các summaries liên quan cùng operation và trả page đúng scope
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C07 ..| : Nhóm sự kiện của thao tác, không thay dữ liệu gốc
    N <.. C08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. C09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. C10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Ghi audit bắt buộc nằm trong transaction action nguồn, không có message từ action sang Admin để xin ghi.”**
2. **“Nhóm operation giúp giải thích dependent effects; không có chức năng undo bằng audit detail.”**
3. **“Login/logout/security-only log có best-effort riêng, không gán chung rollback mutation cho mọi loại log.”**

Đọc lỗi không làm dữ liệu nghiệp vụ bị hoàn tác vì đây không phải mutation. Không tự audit mọi lần đọc; khi không còn quyền, không trình bày dữ liệu cũ như kết quả mới đã xác minh.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Mọi hình chỉ đọc; không arrow lưu/xóa audit.
- Không link chi tiết đối tượng ngoài scope Admin.
- Không secrets/raw payload trong return.
- Group theo operation_id, không gộp theo thời gian gần nhau.
- IP hết hạn thể hiện đúng Không còn lưu.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Admin mở event Grade finalize | Xem allowed audit detail/group; không tự có quyền xem sổ điểm Student qua object link. |
| IP hết retention | Hiển thị Không còn lưu, không bịa IP hoặc phục hồi dữ liệu. |
| Một operation có nhiều dependent events | Group theo operation_id, không gộp chỉ vì cùng timestamp. |
