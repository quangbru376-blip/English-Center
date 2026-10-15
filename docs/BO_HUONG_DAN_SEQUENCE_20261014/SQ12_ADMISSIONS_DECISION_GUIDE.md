# SQ12 — Hướng dẫn vẽ sequence Tuyển sinh duyệt, từ chối hoặc hủy hồ sơ

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV09,12,37; W10–11; F13–14; UC02.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: AdmissionsService.cs](../../../backend/src/EnglishCenter.Api/Services/Admissions/AdmissionsService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Admissions tra cứu hồ sơ và chọn một quyết định. Duyệt là điểm phát hành Invoice duy nhất, khác Student xác nhận nhập học và khác Accountant ghi nhận thanh toán.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ12-A — Duyệt và phát hành Invoice**; actor Tuyển sinh.
- **SQ12-B — Từ chối hồ sơ**; actor Tuyển sinh.
- **SQ12-C — Hủy hồ sơ theo quyền Tuyển sinh**; actor Tuyển sinh.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không ghi danh thay, Director duyệt, chia nhỏ giao dịch phát hành hoặc mở quyền finance detail cho Admissions.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Tuyển sinh | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện chi tiết hồ sơ tuyển sinh | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Duyệt/từ chối | Registration CONFIRMED có Band; phiên Admissions NORMAL. Từ chối cần lý do, không có Director approval tier. |
| Phát hành | Duyệt chuyển ADMITTED và phát hành đúng một Invoice UNPAID: số duy nhất không tái dùng, snapshot học phí dương tại issue, due_date ngày VN +7 lịch; gaps số được phép. |
| Hủy | CONFIRMED hoặc ADMITTED còn Invoice UNPAID theo guard; lý do bắt buộc; đã PAID/đã xếp lớp chặn. Không hủy hồ sơ tùy trạng thái. |
| Phụ thuộc | Hủy Invoice UNPAID nếu có và active future bookings kể cả ca CLOSED trước start; giữ kết quả/histories đã phát sinh và chặn grading tiếp trên hồ sơ CANCELLED. |
| Scope | Chỉ dữ liệu tuyển sinh được phép; Invoice ở màn này là trạng thái cần thiết, không phải toàn finance detail. |
| Đồng thời | Kiểm Band/current state, invoice uniqueness, payment, membership và phiên bản cuối. Race duyệt/giá hoặc hủy/thanh toán không lưu nửa thao tác. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Duyệt và phát hành Invoice

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Mở chức năng Duyệt và phát hành Invoice | Điểm bắt đầu. |
| A02 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Đọc hồ sơ CONFIRMED, Band và preview điều kiện duyệt | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Chọn duyệt hồ sơ và xác nhận | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Gửi yêu cầu duyệt hồ sơ | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm Admissions, hồ sơ/Band hiện tại, học phí tại phát hành, chưa Invoice trùng và các phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Hồ sơ không CONFIRMED, thiếu Band, giá/phiên bản đổi hoặc Invoice đã có]`; không thực hiện hiệu ứng. |
| A10 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Chuyển ADMITTED; cấp số và Invoice UNPAID snapshot học phí/due date; audit nhóm và hoàn tất trong một giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Hồ sơ ADMITTED cùng thông tin phát hành hóa đơn được phép xem | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Tuyển sinh; G = Giao diện chi tiết hồ sơ tuyển sinh; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Duyệt và phát hành Invoice
                            |-- A02 --> H : Đọc hồ sơ CONFIRMED, Band và preview điều kiện duyệt
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Chọn duyệt hồ sơ và xác nhận
                            |-- A07 --> H : Gửi yêu cầu duyệt hồ sơ
                                                       H --+ A08: Kiểm Admissions, hồ sơ/Band hiện tại, học phí tại phát hành, chưa…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Chuyển ADMITTED; cấp số và Invoice UNPAID snapshot học phí/due date;…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Hồ sơ ADMITTED cùng thông tin phát hành hóa đơn được phép xem
    N <.. A13 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ A14: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. A15 ..| : Trả kết quả không thể hoàn tất
    N <.. A16 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### B. Từ chối hồ sơ

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Mở chức năng Từ chối hồ sơ | Điểm bắt đầu. |
| B02 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Đọc hồ sơ và điều kiện từ chối | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Nhập lý do và xác nhận từ chối | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Gửi yêu cầu từ chối hồ sơ với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm CONFIRMED, quyền, lý do và phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Sai trạng thái, thiếu lý do hoặc dữ liệu cũ]`; không thực hiện hiệu ứng. |
| B10 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Chuyển REJECTED, ghi lý do/người/thời gian và kết thúc unfinished theo policy; audit bắt buộc nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Hồ sơ REJECTED với lý do chủ hồ sơ được xem | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Tuyển sinh; G = Giao diện chi tiết hồ sơ tuyển sinh; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Từ chối hồ sơ
                            |-- B02 --> H : Đọc hồ sơ và điều kiện từ chối
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Nhập lý do và xác nhận từ chối
                            |-- B07 --> H : Gửi yêu cầu từ chối hồ sơ với lý do
                                                       H --+ B08: Kiểm CONFIRMED, quyền, lý do và phiên bản
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Chuyển REJECTED, ghi lý do/người/thời gian và kết thúc unfinished theo…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Hồ sơ REJECTED với lý do chủ hồ sơ được xem
    N <.. B13 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ B14: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. B15 ..| : Trả kết quả không thể hoàn tất
    N <.. B16 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### C. Hủy hồ sơ theo quyền Tuyển sinh

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Mở chức năng Hủy hồ sơ theo quyền Tuyển sinh | Điểm bắt đầu. |
| C02 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Đọc preview tác động hủy lên hồ sơ, Invoice và booking tương lai | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Tuyển sinh → Giao diện chi tiết hồ sơ tuyển sinh | Nhập lý do và xác nhận hủy | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện chi tiết hồ sơ tuyển sinh → Hệ thống | Gửi yêu cầu hủy hồ sơ với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm trạng thái được hủy, lý do, chưa PAID/chưa placement, toàn tập phụ thuộc và giờ booking | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Đã PAID/xếp lớp, sai source state, dependency/race hoặc reason thiếu]`; không thực hiện hiệu ứng. |
| C10 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Chuyển CANCELLED, hủy Invoice UNPAID/booking tương lai hợp lệ, xử lý unfinished, audit nhóm và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Hồ sơ đã hủy và ảnh hưởng tương ứng | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện chi tiết hồ sơ tuyển sinh | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện chi tiết hồ sơ tuyển sinh → Tuyển sinh | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Tuyển sinh; G = Giao diện chi tiết hồ sơ tuyển sinh; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Hủy hồ sơ theo quyền Tuyển sinh
                            |-- C02 --> H : Đọc preview tác động hủy lên hồ sơ, Invoice và booking tương lai
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Nhập lý do và xác nhận hủy
                            |-- C07 --> H : Gửi yêu cầu hủy hồ sơ với lý do
                                                       H --+ C08: Kiểm trạng thái được hủy, lý do, chưa PAID/chưa placement, toàn tập phụ…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Chuyển CANCELLED, hủy Invoice UNPAID/booking tương lai hợp lệ, xử lý…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Hồ sơ đã hủy và ảnh hưởng tương ứng
    N <.. C13 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ C14: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. C15 ..| : Trả kết quả không thể hoàn tất
    N <.. C16 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Duyệt + ADMITTED + Invoice UNPAID là một giao dịch. Không có bước commit duyệt trước rồi gọi Accountant tạo hóa đơn.”**
2. **“Student xác nhận SQ11 không tạo Invoice; Thanh toán SQ14 không phát hành Invoice.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Ba action là lựa chọn, không nối tiếp trong một lần.
- Không có Payment hoặc PAID trong nhánh duyệt.
- Replay duyệt không cấp Invoice thứ hai.
- Hủy giữ lịch sử thi/Band và không refund.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Student CONFIRMED, Admissions duyệt | ADMITTED và đúng một Invoice UNPAID cùng commit; không PAID/placement. |
| Payment thắng race trước hủy | Lệnh hủy bị chặn; không tạo Invoice CANCELLED đã PAID. |
| Audit phát hành lỗi | Không giữ Registration ADMITTED mà thiếu Invoice hoặc ngược lại. |
