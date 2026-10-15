# SQ08 — Hướng dẫn vẽ sequence Giáo vụ quản lý ca thi đầu vào

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV14–15,21; W06; F16–17,F25; UC03.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: EntrySessionService.cs](../../../backend/src/EnglishCenter.Api/Services/Academics/EntrySessionService.cs).
- [Code: TeacherEntryService.cs](../../../backend/src/EnglishCenter.Api/Services/Enrollment/TeacherEntryService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Giáo vụ chuẩn bị, mở/đóng, hủy ca hoặc thay grader. Mỗi action là lần xử lý riêng; Teacher chỉ đọc ca được phân công ở hình E, không tham gia duyệt lệnh Giáo vụ.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ08-A — Tạo hoặc sửa thông tin ca**; actor Giáo vụ.
- **SQ08-B — Mở hoặc đóng ca**; actor Giáo vụ.
- **SQ08-C — Hủy ca và các booking hoạt động**; actor Giáo vụ.
- **SQ08-D — Thay giáo viên chấm**; actor Giáo vụ.
- **SQ08-E — Teacher xem ca được giao**; actor Giáo viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không auto-rebook, thông báo email, Teacher phê duyệt ca hoặc đổi grader sau kết quả.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo vụ; Giáo viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện ca thi đầu vào; Giao diện ca thi được phân công | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Tạo ca | Ca mới CLOSED; ngày/giờ VN cùng ngày, start<end và start>serverNow lúc lưu; phòng chuẩn hóa, capacity 1–20, Teacher ACTIVE, không trùng phòng/grader với ca thi hoặc lớp. |
| Sửa | Date/time/room chỉ trước start và chưa có booking; sức chứa/những action khác phải theo dữ liệu phụ thuộc và khả năng hiện tại. Không dùng sửa như đổi grader ngầm. |
| Mở/đóng | Mở/reopen trước start, đủ tài nguyên/chỗ; đóng OPEN trước start giữ booking. Đến start effectively CLOSED, read không tự ghi trạng thái. |
| Hủy | Chỉ trước start, cần lý do; hủy mọi booking hoạt động của ca trong cùng giao dịch, giữ logical Attempt/history; không tự đặt ca khác. |
| Đổi grader | Trước start, chưa có score/ABSENT, cần lý do và Teacher thay hợp lệ/không xung đột. Cùng parsed grader là no-change, lý do khác không biến thành thay đổi. |
| Thẩm quyền | Nút hoặc preview cũ không bảo đảm action còn hợp lệ; kiểm full membership/phiên bản/đồng hồ tại lưu. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Tạo hoặc sửa thông tin ca

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo vụ → Giao diện ca thi đầu vào | Mở chức năng Tạo hoặc sửa thông tin ca | Điểm bắt đầu. |
| A02 | Giao diện ca thi đầu vào → Hệ thống | Đọc danh mục ca và lựa chọn phòng/Teacher; khi sửa đọc ca và phụ thuộc hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện ca thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo vụ → Giao diện ca thi đầu vào | Nhập ngày/giờ/phòng/capacity/Teacher theo form và chọn lưu | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện ca thi đầu vào → Hệ thống | Gửi yêu cầu tạo hoặc sửa thông tin ca | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm phiên bản/phụ thuộc, thời gian tương lai, dữ liệu ca và toàn bộ xung đột tài nguyên | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện ca thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Dữ liệu sai, thời điểm đã tới, có booking chặn sửa, trùng tài nguyên hoặc form cũ]`; không thực hiện hiệu ứng. |
| A10 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Tạo ca CLOSED hoặc sửa các trường được phép; audit bắt buộc và hoàn tất thao tác cùng giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện ca thi đầu vào | Ca đã lưu và phiên bản mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện ca thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện ca thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Tạo hoặc sửa thông tin ca
                            |-- A02 --> H : Đọc danh mục ca và lựa chọn phòng/Teacher; khi sửa đọc ca và phụ thuộc…
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Nhập ngày/giờ/phòng/capacity/Teacher theo form và chọn lưu
                            |-- A07 --> H : Gửi yêu cầu tạo hoặc sửa thông tin ca
                                                       H --+ A08: Kiểm phiên bản/phụ thuộc, thời gian tương lai, dữ liệu ca và toàn bộ…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Tạo ca CLOSED hoặc sửa các trường được phép; audit bắt buộc và hoàn tất…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Ca đã lưu và phiên bản mới
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

### B. Mở hoặc đóng ca

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo vụ → Giao diện ca thi đầu vào | Mở chức năng Mở hoặc đóng ca | Điểm bắt đầu. |
| B02 | Giao diện ca thi đầu vào → Hệ thống | Đọc ca, trạng thái, khả năng mở/đóng và ảnh hưởng hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện ca thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo vụ → Giao diện ca thi đầu vào | Chọn một action Mở hoặc Đóng và xác nhận | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện ca thi đầu vào → Hệ thống | Gửi lệnh mở hoặc đóng ca đã chọn | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm action/source state, trước start, Teacher/phòng/capacity và phiên bản ca | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện ca thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Sai source state, đã start, tài nguyên hoặc phiên bản thay đổi]`; không thực hiện hiệu ứng. |
| B10 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Đổi trạng thái ca theo action; giữ các booking đang có; audit bắt buộc nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện ca thi đầu vào | Ca OPEN hoặc CLOSED theo action đã chọn | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện ca thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện ca thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Mở hoặc đóng ca
                            |-- B02 --> H : Đọc ca, trạng thái, khả năng mở/đóng và ảnh hưởng hiện tại
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Chọn một action Mở hoặc Đóng và xác nhận
                            |-- B07 --> H : Gửi lệnh mở hoặc đóng ca đã chọn
                                                       H --+ B08: Kiểm action/source state, trước start, Teacher/phòng/capacity và phiên…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Đổi trạng thái ca theo action; giữ các booking đang có; audit bắt buộc…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Ca OPEN hoặc CLOSED theo action đã chọn
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

### C. Hủy ca và các booking hoạt động

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Giáo vụ → Giao diện ca thi đầu vào | Mở chức năng Hủy ca và các booking hoạt động | Điểm bắt đầu. |
| C02 | Giao diện ca thi đầu vào → Hệ thống | Đọc ca và preview đầy đủ booking/Registration sẽ bị ảnh hưởng | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện ca thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Giáo vụ → Giao diện ca thi đầu vào | Nhập lý do và xác nhận hủy ca | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện ca thi đầu vào → Hệ thống | Gửi yêu cầu hủy ca với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm trước start, lý do, ca và toàn bộ tập booking/Registration đúng phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện ca thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Ca đã start, membership mới, booking/Registration đổi hoặc thiếu lý do]`; không thực hiện hiệu ứng. |
| C10 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Đổi ca CANCELLED, hủy booking hoạt động, giữ Attempt/history; audit nhóm và dấu hoàn tất trong một giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện ca thi đầu vào | Ca đã hủy và danh sách ảnh hưởng đã thực hiện | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện ca thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện ca thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Hủy ca và các booking hoạt động
                            |-- C02 --> H : Đọc ca và preview đầy đủ booking/Registration sẽ bị ảnh hưởng
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Nhập lý do và xác nhận hủy ca
                            |-- C07 --> H : Gửi yêu cầu hủy ca với lý do
                                                       H --+ C08: Kiểm trước start, lý do, ca và toàn bộ tập booking/Registration đúng…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Đổi ca CANCELLED, hủy booking hoạt động, giữ Attempt/history; audit…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Ca đã hủy và danh sách ảnh hưởng đã thực hiện
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

### D. Thay giáo viên chấm

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| D01 | Giáo vụ → Giao diện ca thi đầu vào | Mở chức năng Thay giáo viên chấm | Điểm bắt đầu. |
| D02 | Giao diện ca thi đầu vào → Hệ thống | Đọc grader hiện tại, danh sách Teacher thay và ảnh hưởng/xung đột | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| D03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| D04 | Hệ thống → Giao diện ca thi đầu vào | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| D05 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| D06 | Giáo vụ → Giao diện ca thi đầu vào | Chọn Teacher khác, nhập lý do và xác nhận đổi grader | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| D07 | Giao diện ca thi đầu vào → Hệ thống | Gửi yêu cầu đổi giáo viên chấm với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| D08 | Hệ thống → Hệ thống | Kiểm trước start/chưa score hoặc ABSENT, Teacher thay, lý do, phiên bản và thay đổi thực | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| D09 | Hệ thống → Giao diện ca thi đầu vào | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Không còn điều kiện thay, trùng giờ, grader không hợp lệ hoặc cùng grader]`; không thực hiện hiệu ứng. |
| D10 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| D11 | Hệ thống → Hệ thống | Thay grader và ghi old/new/reason audit cùng giao dịch; không sửa kết quả thi | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| D12 | Hệ thống → Giao diện ca thi đầu vào | Ca với grader mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| D13 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| D14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| D15 | Hệ thống → Giao diện ca thi đầu vào | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| D16 | Giao diện ca thi đầu vào → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện ca thi đầu vào; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- D01 --> G : Mở chức năng Thay giáo viên chấm
                            |-- D02 --> H : Đọc grader hiện tại, danh sách Teacher thay và ảnh hưởng/xung đột
                                                       H --+ D03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. D04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. D05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- D06 --> G : Chọn Teacher khác, nhập lý do và xác nhận đổi grader
                            |-- D07 --> H : Gửi yêu cầu đổi giáo viên chấm với lý do
                                                       H --+ D08: Kiểm trước start/chưa score hoặc ABSENT, Teacher thay, lý do, phiên bản…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. D09 ..| : Trả lỗi điều kiện hiện tại
    N <.. D10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ D11: Thay grader và ghi old/new/reason audit cùng giao dịch; không sửa kết…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. D12 ..| : Ca với grader mới
    N <.. D13 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ D14: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. D15 ..| : Trả kết quả không thể hoàn tất
    N <.. D16 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### E. Teacher xem ca được giao

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| E01 | Giáo viên → Giao diện ca thi được phân công | Mở danh sách hoặc chọn ca thi | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| E02 | Giao diện ca thi được phân công → Hệ thống | Đọc danh sách/chi tiết ca được phân công cho Teacher hiện tại | Request đọc; bắt đầu activation. |
| E03 | Hệ thống → Hệ thống | Kiểm vai trò Teacher và phân công hiện tại của ca | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| E04 | Hệ thống → Giao diện ca thi được phân công | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Hết quyền/phân công hoặc ca ngoài phạm vi]`; không đi nhánh query thành công. |
| E05 | Giao diện ca thi được phân công → Giáo viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| E06 | Hệ thống → Hệ thống | Chiếu thông tin ca/booking và khả năng chấm theo quyền Teacher | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| E07 | Hệ thống → Giao diện ca thi được phân công | Ca được giao và các học viên trong phạm vi | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| E08 | Giao diện ca thi được phân công → Giáo viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| E09 | Hệ thống → Giao diện ca thi được phân công | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| E10 | Giao diện ca thi được phân công → Giáo viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện ca thi được phân công; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- E01 --> G : Mở danh sách hoặc chọn ca thi
                            |-- E02 --> H : Đọc danh sách/chi tiết ca được phân công cho Teacher hiện tại
                                                       H --+ E03: Kiểm vai trò Teacher và phân công hiện tại của ca
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. E04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. E05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ E06: Chiếu thông tin ca/booking và khả năng chấm theo quyền Teacher
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. E07 ..| : Ca được giao và các học viên trong phạm vi
    N <.. E08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. E09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. E10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“CLOSED vẫn cho chấm/vắng/sửa điểm khi guard SQ10 đạt. CLOSED không xóa booking hoặc score.”**
2. **“Hủy ca không tăng attempt_number và không gọi Student để tự động đặt lại.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- OPEN/CLOSED/CANCELLED là action/state riêng.
- Hủy kiểm tập booking đầy đủ, không chỉ trang đang nhìn.
- Đến start không có cron/read tự ghi CLOSED.
- Teacher không sửa ca hoặc đổi grader.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Đóng ca có 10 booking | Giữ 10 booking; ngăn Student đặt/hủy/đổi riêng; Teacher vẫn có thể chấm khi đạt guard. |
| Hủy ca trước start | Hủy booking hoạt động nguyên tử, giữ Attempt cho Student tự đặt lại; không auto-book. |
| Đổi grader sang cùng Teacher nhưng reason khác | No-change sau kiểm context; không ghi mutation audit mới. |
