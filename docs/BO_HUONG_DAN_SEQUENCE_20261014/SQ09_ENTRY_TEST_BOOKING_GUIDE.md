# SQ09 — Hướng dẫn vẽ sequence Student đặt, đổi, hủy và đặt lại ca thi

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV03; W07; F06–07; UC03.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: StudentEnrollmentService.cs](../../../backend/src/EnglishCenter.Api/Services/Enrollment/StudentEnrollmentService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Student quản lý appointment của lượt thi logic thuộc Registration của mình. Tách đặt ca, đổi ca, hủy ca; đặt lại dùng cùng lượt, còn lượt hai là một Attempt mới đúng điều kiện.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ09-A — Đặt ca, gồm lượt đầu/lượt hai/đặt lại**; actor Học viên.
- **SQ09-B — Đổi ca trong cùng lượt**; actor Học viên.
- **SQ09-C — Hủy booking**; actor Học viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không lịch thi online, tự đặt thay, tự thi lại sau xác nhận hoặc tự chọn ca khác khi xung đột.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Học viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện đặt ca thi | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Chủ sở hữu | Student NORMAL, Registration của mình chưa CONFIRMED/REJECTED/CANCELLED; backend xác định lượt đủ điều kiện. |
| Ca đích | OPEN, chưa start, còn chỗ, không trùng ca/lịch lớp của Student. Tối đa một booking SCHEDULED hiện tại cho một Attempt. |
| Lượt hai | Lượt một có completed score; tối đa 2 Attempt; đặt lượt hai giữ BAND_ASSIGNED nhưng chặn xác nhận khi còn chờ. |
| Lượt và history | Booking là lịch hẹn; ABSENT hoặc center cancellation cho đặt lại cùng Attempt. Không tạo Attempt thứ ba từ việc đổi/hủy/đặt lại. |
| Ca nguồn | Student hủy/đổi booking chỉ khi ca nguồn OPEN và trước start; CLOSED chặn dù còn thời gian. Hủy Registration ở SQ07 có hợp đồng phụ thuộc khác. |
| Trạng thái | Booking đầu chuyển PENDING→TESTING; hủy/vắng không tự kéo hồ sơ về PENDING hoặc xóa Band. Đổi sang cùng ca là no-change sau kiểm context. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Đặt ca, gồm lượt đầu/lượt hai/đặt lại

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Học viên → Giao diện đặt ca thi | Mở chức năng Đặt ca, gồm lượt đầu/lượt hai/đặt lại | Điểm bắt đầu. |
| A02 | Giao diện đặt ca thi → Hệ thống | Đọc hồ sơ/lượt hiện tại và các ca OPEN phù hợp theo bộ lọc | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện đặt ca thi | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện đặt ca thi → Học viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Học viên → Giao diện đặt ca thi | Chọn ca và xác nhận đặt cho lượt đang đủ điều kiện | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện đặt ca thi → Hệ thống | Gửi yêu cầu đặt ca cho lượt đủ điều kiện | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm ownership, trạng thái hồ sơ/lượt, completed score nếu lượt hai, chỗ và xung đột ca đích trước start | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện đặt ca thi | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Hồ sơ đã xác nhận, chưa có điểm lượt một, chỗ cuối bị lấy hoặc trùng lịch]`; không thực hiện hiệu ứng. |
| A10 | Giao diện đặt ca thi → Học viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Tạo Attempt nếu cần theo quy tắc lượt; tạo Booking SCHEDULED; cập nhật Registration cần thiết, chỗ, audit và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện đặt ca thi | Booking hiện tại và số lượt thực tế | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện đặt ca thi → Học viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện đặt ca thi | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện đặt ca thi → Học viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện đặt ca thi; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Đặt ca, gồm lượt đầu/lượt hai/đặt lại
                            |-- A02 --> H : Đọc hồ sơ/lượt hiện tại và các ca OPEN phù hợp theo bộ lọc
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Chọn ca và xác nhận đặt cho lượt đang đủ điều kiện
                            |-- A07 --> H : Gửi yêu cầu đặt ca cho lượt đủ điều kiện
                                                       H --+ A08: Kiểm ownership, trạng thái hồ sơ/lượt, completed score nếu lượt hai,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Tạo Attempt nếu cần theo quy tắc lượt; tạo Booking SCHEDULED; cập nhật…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Booking hiện tại và số lượt thực tế
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

### B. Đổi ca trong cùng lượt

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Học viên → Giao diện đặt ca thi | Mở chức năng Đổi ca trong cùng lượt | Điểm bắt đầu. |
| B02 | Giao diện đặt ca thi → Hệ thống | Đọc booking nguồn và ca đích cùng điều kiện đổi hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện đặt ca thi | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện đặt ca thi → Học viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Học viên → Giao diện đặt ca thi | Chọn ca mới và xác nhận đổi | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện đặt ca thi → Hệ thống | Gửi yêu cầu đổi booking sang ca mới cùng lượt | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm nguồn OPEN/trước start, đích OPEN/trước start/còn chỗ, cùng Attempt, không xung đột và có thay đổi ca | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện đặt ca thi | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Nguồn vừa đóng/start, đích hết chỗ/trùng lịch, dữ liệu cũ hoặc cùng ca]`; không thực hiện hiệu ứng. |
| B10 | Giao diện đặt ca thi → Học viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Hủy booking nguồn, tạo booking đích cùng Attempt; cập nhật chỗ hai ca và audit trong một giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện đặt ca thi | Booking mới cùng lượt và history booking cũ | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện đặt ca thi → Học viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện đặt ca thi | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện đặt ca thi → Học viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện đặt ca thi; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Đổi ca trong cùng lượt
                            |-- B02 --> H : Đọc booking nguồn và ca đích cùng điều kiện đổi hiện tại
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Chọn ca mới và xác nhận đổi
                            |-- B07 --> H : Gửi yêu cầu đổi booking sang ca mới cùng lượt
                                                       H --+ B08: Kiểm nguồn OPEN/trước start, đích OPEN/trước start/còn chỗ, cùng…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Hủy booking nguồn, tạo booking đích cùng Attempt; cập nhật chỗ hai ca…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Booking mới cùng lượt và history booking cũ
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

### C. Hủy booking

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Học viên → Giao diện đặt ca thi | Mở chức năng Hủy booking | Điểm bắt đầu. |
| C02 | Giao diện đặt ca thi → Hệ thống | Đọc booking và khả năng hủy của hồ sơ riêng | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện đặt ca thi | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện đặt ca thi → Học viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Học viên → Giao diện đặt ca thi | Chọn hủy booking và xác nhận | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện đặt ca thi → Hệ thống | Gửi yêu cầu hủy booking hiện tại | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm booking SCHEDULED, nguồn OPEN trước start, hồ sơ chưa bị khóa và phiên bản hiện tại | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện đặt ca thi | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Ca CLOSED/đã start hoặc hồ sơ/booking đã đổi]`; không thực hiện hiệu ứng. |
| C10 | Giao diện đặt ca thi → Học viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Đổi booking CANCELLED, giải phóng chỗ, giữ Attempt/history/Band và ghi audit nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện đặt ca thi | Booking đã hủy; quyền đặt tiếp theo được tính lại | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện đặt ca thi → Học viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện đặt ca thi | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện đặt ca thi → Học viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện đặt ca thi; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Hủy booking
                            |-- C02 --> H : Đọc booking và khả năng hủy của hồ sơ riêng
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Chọn hủy booking và xác nhận
                            |-- C07 --> H : Gửi yêu cầu hủy booking hiện tại
                                                       H --+ C08: Kiểm booking SCHEDULED, nguồn OPEN trước start, hồ sơ chưa bị khóa và…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Đổi booking CANCELLED, giải phóng chỗ, giữ Attempt/history/Band và ghi…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Booking đã hủy; quyền đặt tiếp theo được tính lại
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

1. **“Đặt lại sau ABSENT/center-cancelled là nhánh A với Attempt có sẵn; không gọi chấm điểm hoặc tự đánh vắng.”**
2. **“Thay bộ lọc ngày làm lựa chọn ca cũ hết hiệu lực. Ca hiển thị còn chỗ không giữ chỗ cho Student.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Phân biệt Attempt với Booking trong note.
- Đổi ca all-or-none, không hủy nguồn trước rồi hy vọng đặt đích.
- Nhánh hai người tranh chỗ cuối không quá capacity.
- CLOSED nguồn chặn hủy/đổi booking; không nhầm với SQ07.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Attempt 1 ABSENT, Student chọn ca mới | Tạo Booking khác cho Attempt 1; không biến thành Attempt 2. |
| Lượt 1 chưa có completed score | Không đặt lượt 2. |
| Hai Student tranh chỗ cuối | Không vượt capacity; lệnh còn lại conflict/read lại, không tự chọn ca khác. |
