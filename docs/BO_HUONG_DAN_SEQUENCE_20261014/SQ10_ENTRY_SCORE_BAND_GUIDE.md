# SQ10 — Hướng dẫn vẽ sequence Teacher chấm, ghi vắng, sửa điểm và tính Band

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV04,15,22,36; W08; F08,F16–17,F25; UC03.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: TeacherEntryService.cs](../../../backend/src/EnglishCenter.Api/Services/Enrollment/TeacherEntryService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Teacher được phân công xử lý một kết quả đầu vào sau ca kết thúc. Hệ thống đồng thời cập nhật Booking/Attempt, kết quả tốt nhất và Band; Student/ Giáo vụ xem là lượt đọc độc lập.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ10-A — Nhập điểm đầu vào**; actor Giáo viên.
- **SQ10-B — Ghi vắng thi**; actor Giáo viên.
- **SQ10-C — Sửa điểm có lý do**; actor Giáo viên.
- **SQ10-D — Student xem kết quả**; actor Học viên.
- **SQ10-E — Giáo vụ theo dõi kết quả**; actor Giáo vụ.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không Admin/Giáo vụ override điểm, tái chấm sau CONFIRMED hoặc tự ghi vắng từ ô trống.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo viên; Học viên; Giáo vụ | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện chấm thi đầu vào; Giao diện kết quả đầu vào; Giao diện quản lý ca thi | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Thời điểm | Initial score và ABSENT chỉ từ session end; ca không CANCELLED, Registration chưa CONFIRMED/REJECTED/CANCELLED; CLOSED không chặn. |
| Điểm | 0–100, tối đa 2 số lẻ; 0 là điểm thật. ABSENT không có score và không hoàn thành kết quả. |
| Sửa | Result COMPLETED; original score Teacher vẫn là grader được giao; hồ sơ TESTING/BAND_ASSIGNED; cần reason. Điểm bằng parsed score cũ là no-change, không ghi audit mutation chỉ vì reason khác. |
| Best/Band | Chọn score COMPLETED cao nhất; hòa chọn Attempt 1. Band có min_score lớn nhất ≤ best; seed 0/25/45/65/85. |
| Hiệu ứng | Điểm đầu chuyển BAND_ASSIGNED; điểm lượt hai giữ trạng thái. First completed result khóa cả bộ ngưỡng Band. Sửa hợp lệ tính lại best/Band nguyên tử. |
| Phân quyền đọc | Student chỉ kết quả của hồ sơ mình; Giáo vụ theo dõi không có quyền chấm/sửa thay Teacher. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Nhập điểm đầu vào

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo viên → Giao diện chấm thi đầu vào | Mở chức năng Nhập điểm đầu vào | Điểm bắt đầu. |
| A02 | Giao diện chấm thi đầu vào → Hệ thống | Đọc ca được giao, booking học viên và khả năng nhập điểm | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện chấm thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo viên → Giao diện chấm thi đầu vào | Nhập điểm và chọn lưu kết quả | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện chấm thi đầu vào → Hệ thống | Gửi yêu cầu lưu điểm đầu vào | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm grader hiện tại, end cutoff, Registration/booking, score hợp lệ và các phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện chấm thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Chưa end, mất phân công, hồ sơ đã khóa, score sai hoặc booking đã xử lý]`; không thực hiện hiệu ứng. |
| A10 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Ghi score/provenance, booking COMPLETED; tính best/Band, cập nhật BAND_ASSIGNED và first-use freeze; audit và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện chấm thi đầu vào | Điểm đã lưu, lượt tốt nhất và Band hiện tại | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện chấm thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện chấm thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Nhập điểm đầu vào
                            |-- A02 --> H : Đọc ca được giao, booking học viên và khả năng nhập điểm
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Nhập điểm và chọn lưu kết quả
                            |-- A07 --> H : Gửi yêu cầu lưu điểm đầu vào
                                                       H --+ A08: Kiểm grader hiện tại, end cutoff, Registration/booking, score hợp lệ và…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Ghi score/provenance, booking COMPLETED; tính best/Band, cập nhật…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Điểm đã lưu, lượt tốt nhất và Band hiện tại
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

### B. Ghi vắng thi

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo viên → Giao diện chấm thi đầu vào | Mở chức năng Ghi vắng thi | Điểm bắt đầu. |
| B02 | Giao diện chấm thi đầu vào → Hệ thống | Đọc booking chưa xử lý trong ca được phân công | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện chấm thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo viên → Giao diện chấm thi đầu vào | Chọn xác nhận vắng cho booking | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện chấm thi đầu vào → Hệ thống | Gửi yêu cầu ghi vắng booking | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm từ session end, phân công và booking chưa hoàn tất, hồ sơ còn hợp lệ | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện chấm thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Chưa end hoặc booking đã COMPLETED/ca bị hủy/hồ sơ khóa]`; không thực hiện hiệu ứng. |
| B10 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Ghi booking ABSENT và người/thời điểm xác nhận; giữ score NULL/Attempt; audit bắt buộc nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện chấm thi đầu vào | Booking ABSENT; học viên có thể tự đặt lại cùng lượt nếu đủ điều kiện | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện chấm thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện chấm thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Ghi vắng thi
                            |-- B02 --> H : Đọc booking chưa xử lý trong ca được phân công
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Chọn xác nhận vắng cho booking
                            |-- B07 --> H : Gửi yêu cầu ghi vắng booking
                                                       H --+ B08: Kiểm từ session end, phân công và booking chưa hoàn tất, hồ sơ còn hợp…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Ghi booking ABSENT và người/thời điểm xác nhận; giữ score NULL/Attempt;…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Booking ABSENT; học viên có thể tự đặt lại cùng lượt nếu đủ điều kiện
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

### C. Sửa điểm có lý do

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Giáo viên → Giao diện chấm thi đầu vào | Mở chức năng Sửa điểm có lý do | Điểm bắt đầu. |
| C02 | Giao diện chấm thi đầu vào → Hệ thống | Đọc kết quả COMPLETED và context sửa hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện chấm thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Giáo viên → Giao diện chấm thi đầu vào | Nhập điểm mới, lý do và chọn sửa điểm | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện chấm thi đầu vào → Hệ thống | Gửi yêu cầu sửa điểm với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm original grader còn assigned, hồ sơ TESTING/BAND_ASSIGNED, reason, score/phiên bản và có delta thực | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện chấm thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Đã CONFIRMED, sai grader, thiếu reason, stale hoặc điểm không đổi]`; không thực hiện hiệu ứng. |
| C10 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Sửa score, giữ provenance nhập đầu; tính lại best/Band; ghi old/new/reason audit và hoàn tất cùng giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện chấm thi đầu vào | Điểm mới và Band tính lại | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện chấm thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện chấm thi đầu vào → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện chấm thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Sửa điểm có lý do
                            |-- C02 --> H : Đọc kết quả COMPLETED và context sửa hiện tại
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Nhập điểm mới, lý do và chọn sửa điểm
                            |-- C07 --> H : Gửi yêu cầu sửa điểm với lý do
                                                       H --+ C08: Kiểm original grader còn assigned, hồ sơ TESTING/BAND_ASSIGNED, reason,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Sửa score, giữ provenance nhập đầu; tính lại best/Band; ghi…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Điểm mới và Band tính lại
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

### D. Student xem kết quả

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| D01 | Học viên → Giao diện kết quả đầu vào | Mở kết quả đầu vào | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| D02 | Giao diện kết quả đầu vào → Hệ thống | Đọc kết quả các lượt và Band của hồ sơ riêng | Request đọc; bắt đầu activation. |
| D03 | Hệ thống → Hệ thống | Kiểm ownership và phạm vi DTO Student | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| D04 | Hệ thống → Giao diện kết quả đầu vào | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Hồ sơ ngoài scope/hết phiên]`; không đi nhánh query thành công. |
| D05 | Giao diện kết quả đầu vào → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| D06 | Hệ thống → Hệ thống | Chiếu các kết quả/histories được phép và best/Band hiện tại | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| D07 | Hệ thống → Giao diện kết quả đầu vào | Kết quả của mình; vắng/chưa chấm hiển thị đúng nghĩa | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| D08 | Giao diện kết quả đầu vào → Học viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| D09 | Hệ thống → Giao diện kết quả đầu vào | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| D10 | Giao diện kết quả đầu vào → Học viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện kết quả đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- D01 --> G : Mở kết quả đầu vào
                            |-- D02 --> H : Đọc kết quả các lượt và Band của hồ sơ riêng
                                                       H --+ D03: Kiểm ownership và phạm vi DTO Student
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. D04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. D05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ D06: Chiếu các kết quả/histories được phép và best/Band hiện tại
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. D07 ..| : Kết quả của mình; vắng/chưa chấm hiển thị đúng nghĩa
    N <.. D08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. D09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. D10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### E. Giáo vụ theo dõi kết quả

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| E01 | Giáo vụ → Giao diện quản lý ca thi | Mở phần kết quả của ca/hồ sơ | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| E02 | Giao diện quản lý ca thi → Hệ thống | Đọc ca/kết quả/Band trong phạm vi quản lý | Request đọc; bắt đầu activation. |
| E03 | Hệ thống → Hệ thống | Kiểm vai trò và dữ liệu được phép quản lý | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| E04 | Hệ thống → Giao diện quản lý ca thi | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Không còn quyền truy cập]`; không đi nhánh query thành công. |
| E05 | Giao diện quản lý ca thi → Giáo vụ | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| E06 | Hệ thống → Hệ thống | Chiếu trạng thái và kết quả quản lý, không cấp action chấm/sửa | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| E07 | Hệ thống → Giao diện quản lý ca thi | Kết quả/Band để theo dõi | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| E08 | Giao diện quản lý ca thi → Giáo vụ | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| E09 | Hệ thống → Giao diện quản lý ca thi | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| E10 | Giao diện quản lý ca thi → Giáo vụ | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý ca thi; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- E01 --> G : Mở phần kết quả của ca/hồ sơ
                            |-- E02 --> H : Đọc ca/kết quả/Band trong phạm vi quản lý
                                                       H --+ E03: Kiểm vai trò và dữ liệu được phép quản lý
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. E04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. E05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ E06: Chiếu trạng thái và kết quả quản lý, không cấp action chấm/sửa
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. E07 ..| : Kết quả/Band để theo dõi
    N <.. E08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. E09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. E10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Không gọi Student xác nhận như bước tự động sau chấm. Xác nhận là SQ11 ở thời điểm khác.”**
2. **“Một lỗi tính Band/freeze/audit làm rollback cả kết quả; không hiển thị score mới với Band cũ.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Chấm từ end, không từ start.
- ABSENT riêng với score 0.
- Hòa score giữ best Attempt 1.
- Sửa không thay original graded_by/graded_at; lịch sử correction nằm trong audit.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Hai lượt đều 65 điểm | Best Attempt 1; Band min_score=65 theo seed; vẫn giữ cả hai kết quả. |
| Student không tới thi, không nhập score | Teacher phải chủ động ghi ABSENT sau end; ô trống không tự thành vắng hoặc 0. |
| Original grader sửa 65→64 trước CONFIRMED | Tính lại best/Band nguyên tử; giữ provenance chấm đầu và audit correction reason. |
