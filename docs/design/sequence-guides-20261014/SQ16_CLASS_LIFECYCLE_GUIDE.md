# SQ16 — Hướng dẫn vẽ sequence Giáo vụ quản lý lớp và giáo viên lớp

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV16–17,23; W14; F19–20,F24; UC06–07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: ClassService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/ClassService.cs).
- [Code: SqlClassRepository.cs](../../../backend/src/EnglishCenter.Api/Infrastructure/Classes/SqlClassRepository.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Giáo vụ tạo lớp, quản lý khả năng mở/bắt đầu/hoàn tất/hủy và thay Teacher trước khi dạy. Thời gian trôi qua không tự chuyển Class state; mỗi chuyển trạng thái là một lệnh có chủ đích.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ16-A — Tạo lớp PLANNED**; actor Giáo vụ.
- **SQ16-B — Mở hoặc bắt đầu lớp**; actor Giáo vụ.
- **SQ16-C — Hoàn tất lớp**; actor Giáo vụ.
- **SQ16-D — Hủy lớp chưa có học viên/chưa dạy**; actor Giáo vụ.
- **SQ16-E — Đổi Teacher hoặc sức chứa**; actor Giáo vụ.
- **SQ16-F — Teacher xem lớp được giao**; actor Giáo viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không Director approval, automatic lifecycle, chuyển lớp/refund hoặc đổi Teacher sau teaching.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo vụ; Giáo viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện quản lý lớp; Giao diện lớp được phân công | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Tạo | Class PLANNED, code thủ công duy nhất/bất biến, Program ACTIVE với cấu hình hợp lệ, Teacher ACTIVE, max_size 1–15. First Class use khóa cấu hình Program, kể cả lớp sau đó CANCELLED. |
| OPEN | Từ PLANNED; Program ACTIVE, Teacher hợp lệ, ít nhất một LESSON tương lai; không tự mở ngay sau tạo. |
| ONGOING | Từ OPEN; không trước first LESSON actual start, ít nhất 1 placed Student, đủ expected LESSON schedule. Không tự start do clock. |
| COMPLETED | Từ ONGOING, có Student; current LESSON count đúng expected, đúng một current FINAL_EXAM; tất cả current obligations HELD và Attendance đủ mỗi Student/mỗi LESSON. Chưa yêu cầu grades SUBMITTED/FINALIZED. |
| CANCELLED | Chỉ PLANNED/OPEN trống, chưa dạy; lý do; giữ history, giải phóng upcoming resources. Không hủy lớp có Student rồi hoàn tiền/chuyển lớp. |
| Teacher/capacity | Đổi Teacher hoặc capacity chỉ khi BeforeTeaching: PLANNED/OPEN, chưa Attendance/Score, chưa current LESSON tới start hoặc HELD. Teacher mới ACTIVE/không xung đột; capacity 1–15 và không thấp hơn placed count. All-equal edit không tạo mutation mới. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Tạo lớp PLANNED

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo vụ → Giao diện quản lý lớp | Mở chức năng Tạo lớp PLANNED | Điểm bắt đầu. |
| A02 | Giao diện quản lý lớp → Hệ thống | Đọc Program/Teacher có thể chọn và danh mục lớp | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện quản lý lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện quản lý lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo vụ → Giao diện quản lý lớp | Nhập code, Program, Teacher, sức chứa và chọn tạo lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện quản lý lớp → Hệ thống | Gửi yêu cầu tạo lớp PLANNED | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm Program/Teacher hợp lệ, code duy nhất, capacity, cấu hình và phiên bản phụ thuộc | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện quản lý lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Code trùng, Program/Teacher không hợp lệ, cấu hình/phụ thuộc đổi hoặc capacity sai]`; không thực hiện hiệu ứng. |
| A10 | Giao diện quản lý lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Tạo Class PLANNED, bảo vệ first-use freeze của Program, audit và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện quản lý lớp | Lớp PLANNED đã tạo | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện quản lý lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện quản lý lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện quản lý lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Tạo lớp PLANNED
                            |-- A02 --> H : Đọc Program/Teacher có thể chọn và danh mục lớp
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Nhập code, Program, Teacher, sức chứa và chọn tạo lớp
                            |-- A07 --> H : Gửi yêu cầu tạo lớp PLANNED
                                                       H --+ A08: Kiểm Program/Teacher hợp lệ, code duy nhất, capacity, cấu hình và phiên…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Tạo Class PLANNED, bảo vệ first-use freeze của Program, audit và hoàn…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Lớp PLANNED đã tạo
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

### B. Mở hoặc bắt đầu lớp

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo vụ → Giao diện quản lý lớp | Mở chức năng Mở hoặc bắt đầu lớp | Điểm bắt đầu. |
| B02 | Giao diện quản lý lớp → Hệ thống | Đọc lớp và preview riêng cho OPEN hoặc START | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện quản lý lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện quản lý lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo vụ → Giao diện quản lý lớp | Chọn một action Mở lớp hoặc Bắt đầu lớp và xác nhận | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện quản lý lớp → Hệ thống | Gửi lệnh OPEN hoặc START đã chọn | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm source state và điều kiện OPEN/ONGOING trong bảng guard; roster, lịch, Program, Teacher và giờ hiện tại | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện quản lý lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Sai source state, chưa first start, thiếu Student/lịch hoặc phiên bản thay đổi]`; không thực hiện hiệu ứng. |
| B10 | Giao diện quản lý lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Đổi PLANNED→OPEN hoặc OPEN→ONGOING theo action; audit và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện quản lý lớp | Trạng thái lớp mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện quản lý lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện quản lý lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện quản lý lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Mở hoặc bắt đầu lớp
                            |-- B02 --> H : Đọc lớp và preview riêng cho OPEN hoặc START
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Chọn một action Mở lớp hoặc Bắt đầu lớp và xác nhận
                            |-- B07 --> H : Gửi lệnh OPEN hoặc START đã chọn
                                                       H --+ B08: Kiểm source state và điều kiện OPEN/ONGOING trong bảng guard; roster,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Đổi PLANNED→OPEN hoặc OPEN→ONGOING theo action; audit và hoàn tất…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Trạng thái lớp mới
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

### C. Hoàn tất lớp

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Giáo vụ → Giao diện quản lý lớp | Mở chức năng Hoàn tất lớp | Điểm bắt đầu. |
| C02 | Giao diện quản lý lớp → Hệ thống | Đọc preview completion với đầy đủ nghĩa vụ lịch và coverage điểm danh | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện quản lý lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện quản lý lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Giáo vụ → Giao diện quản lý lớp | Xác nhận hoàn tất lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện quản lý lớp → Hệ thống | Gửi lệnh COMPLETE lớp | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm Class ONGOING, mọi nghĩa vụ HELD và mọi required Attendance, roster/current schedule và các phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện quản lý lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Còn NOT_HELD/PLANNED/thiếu điểm danh hoặc dependency thay đổi]`; không thực hiện hiệu ứng. |
| C10 | Giao diện quản lý lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Chuyển Class COMPLETED và audit nguyên tử; chặn sửa Attendance, giữ GradeReport và Registration theo trạng thái hiện có | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện quản lý lớp | Class COMPLETED; Teacher có thể gửi điểm khi đạt SQ21 | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện quản lý lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện quản lý lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện quản lý lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Hoàn tất lớp
                            |-- C02 --> H : Đọc preview completion với đầy đủ nghĩa vụ lịch và coverage điểm danh
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Xác nhận hoàn tất lớp
                            |-- C07 --> H : Gửi lệnh COMPLETE lớp
                                                       H --+ C08: Kiểm Class ONGOING, mọi nghĩa vụ HELD và mọi required Attendance,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Chuyển Class COMPLETED và audit nguyên tử; chặn sửa Attendance, giữ…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Class COMPLETED; Teacher có thể gửi điểm khi đạt SQ21
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

### D. Hủy lớp chưa có học viên/chưa dạy

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| D01 | Giáo vụ → Giao diện quản lý lớp | Mở chức năng Hủy lớp chưa có học viên/chưa dạy | Điểm bắt đầu. |
| D02 | Giao diện quản lý lớp → Hệ thống | Đọc preview hủy lớp và lịch liên quan | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| D03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| D04 | Hệ thống → Giao diện quản lý lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| D05 | Giao diện quản lý lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| D06 | Giáo vụ → Giao diện quản lý lớp | Nhập lý do và xác nhận hủy lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| D07 | Giao diện quản lý lớp → Hệ thống | Gửi lệnh CANCEL lớp với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| D08 | Hệ thống → Hệ thống | Kiểm PLANNED/OPEN trống, chưa teaching/Attendance/Score, reason và tập phụ thuộc hiện tại | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| D09 | Hệ thống → Giao diện quản lý lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Lớp có placement/đã dạy, source state sai hoặc dependency thay đổi]`; không thực hiện hiệu ứng. |
| D10 | Giao diện quản lý lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| D11 | Hệ thống → Hệ thống | Chuyển CANCELLED, ghi cancel metadata và audit; giữ history, giải phóng tài nguyên tương lai theo policy | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| D12 | Hệ thống → Giao diện quản lý lớp | Lớp CANCELLED | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| D13 | Giao diện quản lý lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| D14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| D15 | Hệ thống → Giao diện quản lý lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| D16 | Giao diện quản lý lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- D01 --> G : Mở chức năng Hủy lớp chưa có học viên/chưa dạy
                            |-- D02 --> H : Đọc preview hủy lớp và lịch liên quan
                                                       H --+ D03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. D04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. D05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- D06 --> G : Nhập lý do và xác nhận hủy lớp
                            |-- D07 --> H : Gửi lệnh CANCEL lớp với lý do
                                                       H --+ D08: Kiểm PLANNED/OPEN trống, chưa teaching/Attendance/Score, reason và tập…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. D09 ..| : Trả lỗi điều kiện hiện tại
    N <.. D10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ D11: Chuyển CANCELLED, ghi cancel metadata và audit; giữ history, giải phóng…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. D12 ..| : Lớp CANCELLED
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

### E. Đổi Teacher hoặc sức chứa

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| E01 | Giáo vụ → Giao diện quản lý lớp | Mở chức năng Đổi Teacher hoặc sức chứa | Điểm bắt đầu. |
| E02 | Giao diện quản lý lớp → Hệ thống | Đọc Teacher/capacity và khả năng sửa hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| E03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| E04 | Hệ thống → Giao diện quản lý lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| E05 | Giao diện quản lý lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| E06 | Giáo vụ → Giao diện quản lý lớp | Chọn một thay đổi được phép và xác nhận lưu | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| E07 | Giao diện quản lý lớp → Hệ thống | Gửi lệnh đổi Teacher hoặc capacity đã chọn | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| E08 | Hệ thống → Hệ thống | Kiểm guard action, Teacher/resource conflicts hoặc capacity≥placed count, versions và delta thực | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| E09 | Hệ thống → Giao diện quản lý lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Đã dạy, Teacher/xung đột không hợp lệ, giảm capacity dưới roster hoặc không đổi]`; không thực hiện hiệu ứng. |
| E10 | Giao diện quản lý lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| E11 | Hệ thống → Hệ thống | Cập nhật trường của action, audit old/new và hoàn tất nguyên tử; không đổi Program hoặc roster | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| E12 | Hệ thống → Giao diện quản lý lớp | Lớp với thông tin mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| E13 | Giao diện quản lý lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| E14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| E15 | Hệ thống → Giao diện quản lý lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| E16 | Giao diện quản lý lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện quản lý lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- E01 --> G : Mở chức năng Đổi Teacher hoặc sức chứa
                            |-- E02 --> H : Đọc Teacher/capacity và khả năng sửa hiện tại
                                                       H --+ E03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. E04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. E05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- E06 --> G : Chọn một thay đổi được phép và xác nhận lưu
                            |-- E07 --> H : Gửi lệnh đổi Teacher hoặc capacity đã chọn
                                                       H --+ E08: Kiểm guard action, Teacher/resource conflicts hoặc capacity≥placed…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. E09 ..| : Trả lỗi điều kiện hiện tại
    N <.. E10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ E11: Cập nhật trường của action, audit old/new và hoàn tất nguyên tử; không…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. E12 ..| : Lớp với thông tin mới
    N <.. E13 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ E14: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. E15 ..| : Trả kết quả không thể hoàn tất
    N <.. E16 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### F. Teacher xem lớp được giao

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| F01 | Giáo viên → Giao diện lớp được phân công | Mở danh sách hoặc chi tiết lớp được giao | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| F02 | Giao diện lớp được phân công → Hệ thống | Đọc các lớp/roster/lịch thuộc phân công hiện tại | Request đọc; bắt đầu activation. |
| F03 | Hệ thống → Hệ thống | Kiểm Teacher và current assignment | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| F04 | Hệ thống → Giao diện lớp được phân công | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Không còn phân công/hết phiên]`; không đi nhánh query thành công. |
| F05 | Giao diện lớp được phân công → Giáo viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| F06 | Hệ thống → Hệ thống | Chiếu roster/lịch và khả năng giảng dạy theo quyền Teacher | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| F07 | Hệ thống → Giao diện lớp được phân công | Lớp được giao, không có quyền lifecycle Giáo vụ | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| F08 | Giao diện lớp được phân công → Giáo viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| F09 | Hệ thống → Giao diện lớp được phân công | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| F10 | Giao diện lớp được phân công → Giáo viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện lớp được phân công; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- F01 --> G : Mở danh sách hoặc chi tiết lớp được giao
                            |-- F02 --> H : Đọc các lớp/roster/lịch thuộc phân công hiện tại
                                                       H --+ F03: Kiểm Teacher và current assignment
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. F04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. F05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ F06: Chiếu roster/lịch và khả năng giảng dạy theo quyền Teacher
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. F07 ..| : Lớp được giao, không có quyền lifecycle Giáo vụ
    N <.. F08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. F09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. F10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Class COMPLETED chưa công bố điểm; SQ21/SQ22 là các thao tác tiếp theo, không gọi tự động trong COMPLETE.”**
2. **“Teacher bị khóa tài khoản không tự mất assignment hoặc được tự thay bằng người khác.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Guard đủ từng transition, không dùng set status tùy ý.
- COMPLETE kiểm whole coverage, không chỉ trang điểm danh.
- Hủy chỉ lớp trống/chưa dạy.
- First-use freeze không được giải sau hủy lớp.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Lớp đủ HELD nhưng thiếu một Attendance bắt buộc | COMPLETE bị chặn, không tự ghi Vắng cho row thiếu. |
| Class COMPLETE khi GradeReport còn DRAFT | Được nếu coverage giảng dạy/Attendance đạt; không tự FINALIZE. |
| Lớp OPEN có 1 Student, chọn CANCEL | Bị chặn; không tự chuyển Student hoặc refund. |
