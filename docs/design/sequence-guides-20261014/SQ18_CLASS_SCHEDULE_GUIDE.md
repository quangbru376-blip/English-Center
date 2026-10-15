# SQ18 — Hướng dẫn vẽ sequence Lập lịch, dời/thay buổi, xác nhận HELD và xem lịch

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV07,19,23; W16; F21,F11,F24; UC06–07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: ScheduleService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/ScheduleService.cs).
- [Code: ClassService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/ClassService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Giáo vụ quản lý các buổi có ngày cụ thể và nghĩa vụ buổi học. Tách lịch tuần, buổi riêng, dời trước start, replacement sau buổi không tổ chức, xác nhận HELD và đọc lịch đúng vai trò.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ18-A — Preview và lưu batch lịch tuần LESSON**; actor Giáo vụ.
- **SQ18-B — Tạo một buổi LESSON hoặc FINAL_EXAM**; actor Giáo vụ.
- **SQ18-C — Dời buổi trước start**; actor Giáo vụ.
- **SQ18-D — Ghi không tổ chức và tạo buổi thay thế**; actor Giáo vụ.
- **SQ18-E — Xác nhận đã tổ chức**; actor Giáo vụ.
- **SQ18-F — Student xem lịch riêng**; actor Học viên.
- **SQ18-G — Teacher xem lịch giảng dạy**; actor Giáo viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không recurring record không ngày, cron tự HELD, holiday automation hoặc tạo extra lesson thay replacement.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo vụ; Học viên; Giáo viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện lịch lớp; Giao diện lớp và lịch học; Giao diện lớp và lịch dạy | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Thời gian | Ngày/giờ VN theo phút, start<end cùng ngày; mọi root/candidate tuần/đích dời start>serverNow tại kiểm lưu, không backfill. |
| Nghĩa vụ | LESSON count theo Program; FINAL_EXAM riêng sau last current LESSON. Replacement cùng Class/type/obligation, không tính thành thêm một nghĩa vụ. |
| Xung đột | Khoảng [start,end), kiểm phòng/Teacher/Class/Student trên cả ca thi và lịch lớp. Hai buổi liền kề ở end=start không trùng. |
| Preview | Ngày/count/conflicts cụ thể, không giữ chỗ/giữ đồng hồ; đổi input làm preview cũ hết hiệu lực. Một candidate sai chặn cả weekly batch. |
| Move | Nguồn current PLANNED trước start, không Attendance/linked Score, Class không COMPLETED/CANCELLED và Teacher hợp lệ; đích future đúng chronology. All-equal ngày/giờ/phòng là no-change sau kiểm context. |
| Replacement | Nguồn current PLANNED đã tới start, chưa Attendance/linked Score, Class chưa ended và Teacher hợp lệ; reason bắt buộc, đích future đúng chronology. Giữ nguồn NOT_HELD/history và tạo linked replacement cùng Class/type/obligation nguyên tử. |
| HELD | Giáo vụ explicit, Class ONGOING, occurrence current PLANNED và serverNow≥end (ngay end được phép); không undo. Thời gian/Attendance không tự HELD. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Preview và lưu batch lịch tuần LESSON

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo vụ → Giao diện lịch lớp | Mở chức năng Preview và lưu batch lịch tuần LESSON | Điểm bắt đầu. |
| A02 | Giao diện lịch lớp → Hệ thống | Đọc lớp/Program/current schedules và context lập lịch tuần | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện lịch lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện lịch lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo vụ → Giao diện lịch lớp | Nhập khoảng ngày, thứ, giờ, phòng và chọn xem trước lịch tuần | Chưa gửi lệnh lưu; đây là input của preview. |
| A07 | Giao diện lịch lớp → Hệ thống | Yêu cầu preview lịch tuần theo rule vừa nhập | Request đọc riêng trước opt xác nhận lưu. |
| A08 | Hệ thống → Hệ thống | Mở rộng rule thành candidate cụ thể; kiểm count/thời gian/xung đột và xây manifest | Không tạo Schedule hoặc giữ chỗ; input sai trả lỗi, conflict trả context blocked theo hợp đồng. |
| A09 | Hệ thống → Giao diện lịch lớp | Trả candidate/count/conflicts/eligible và manifest khi đủ điều kiện | Chỉ preview, không ghi dữ liệu. |
| A10 | Giao diện lịch lớp → Giáo vụ | Hiển thị preview toàn batch; chỉ cho xác nhận khi đủ điều kiện | Sửa rule làm preview cũ hết hiệu lực; nếu preview bị chặn thì dừng để sửa/read lại. |
| A11 | Giáo vụ → Giao diện lịch lớp | Giữ đúng input đã preview và xác nhận lưu toàn batch tuần | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A12 | Giao diện lịch lớp → Hệ thống | Gửi yêu cầu lưu toàn bộ lịch tuần đã preview | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A13 | Hệ thống → Hệ thống | Kiểm lại toàn candidates còn tương lai, count không vượt, lớp/phụ thuộc/versions và mọi xung đột | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A14 | Hệ thống → Giao diện lịch lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Một ngày đã start, count vượt, xung đột mới hoặc preview/input/phiên bản không còn đúng]`; không thực hiện hiệu ứng. |
| A15 | Giao diện lịch lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A16 | Hệ thống → Hệ thống | Tạo toàn bộ root LESSON/obligations và audit nhóm cùng giao dịch; một lỗi rollback toàn batch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A17 | Hệ thống → Giao diện lịch lớp | Toàn bộ lịch tuần đã lưu | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A18 | Giao diện lịch lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A19 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A20 | Hệ thống → Giao diện lịch lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A21 | Giao diện lịch lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện lịch lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Preview và lưu batch lịch tuần LESSON
                            |-- A02 --> H : Đọc lớp/Program/current schedules và context lập lịch tuần
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
    |-- A06 --> G : Nhập khoảng ngày, thứ, giờ, phòng và chọn xem trước lịch tuần
                            |-- A07 --> H : Yêu cầu preview lịch tuần theo rule vừa nhập
                                                       H --+ A08: Mở rộng rule thành candidate cụ thể; kiểm count/thời gian/xung đột và…
                                                       H <--+
                            G <.. A09 ..| : Trả candidate/count/conflicts/eligible và manifest khi đủ điều kiện
    N <.. A10 ..| : Hiển thị preview toàn batch; chỉ cho xác nhận khi đủ điều kiện
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A11 --> G : Giữ đúng input đã preview và xác nhận lưu toàn batch tuần
                            |-- A12 --> H : Gửi yêu cầu lưu toàn bộ lịch tuần đã preview
                                                       H --+ A13: Kiểm lại toàn candidates còn tương lai, count không vượt, lớp/phụ…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A14 ..| : Trả lỗi điều kiện hiện tại
    N <.. A15 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A16: Tạo toàn bộ root LESSON/obligations và audit nhóm cùng giao dịch; một…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A17 ..| : Toàn bộ lịch tuần đã lưu
    N <.. A18 ..| : Hiển thị kết quả đã hoàn tất
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ A19: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. A20 ..| : Trả kết quả không thể hoàn tất
    N <.. A21 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### B. Tạo một buổi LESSON hoặc FINAL_EXAM

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo vụ → Giao diện lịch lớp | Mở chức năng Tạo một buổi LESSON hoặc FINAL_EXAM | Điểm bắt đầu. |
| B02 | Giao diện lịch lớp → Hệ thống | Đọc lịch và điều kiện tạo buổi riêng | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện lịch lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện lịch lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo vụ → Giao diện lịch lớp | Nhập ngày/giờ/phòng/type và xác nhận tạo | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện lịch lớp → Hệ thống | Gửi yêu cầu tạo một buổi cụ thể | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm type/count/chronology, FINAL_EXAM sau last current LESSON, future start và xung đột | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện lịch lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Giờ không hợp lệ, quá count, sai chronology hoặc trùng tài nguyên]`; không thực hiện hiệu ứng. |
| B10 | Giao diện lịch lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Tạo root Schedule PLANNED/current và obligation; audit bắt buộc nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện lịch lớp | Buổi riêng đã tạo | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện lịch lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện lịch lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện lịch lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện lịch lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Tạo một buổi LESSON hoặc FINAL_EXAM
                            |-- B02 --> H : Đọc lịch và điều kiện tạo buổi riêng
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Nhập ngày/giờ/phòng/type và xác nhận tạo
                            |-- B07 --> H : Gửi yêu cầu tạo một buổi cụ thể
                                                       H --+ B08: Kiểm type/count/chronology, FINAL_EXAM sau last current LESSON, future…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Tạo root Schedule PLANNED/current và obligation; audit bắt buộc nguyên…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Buổi riêng đã tạo
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

### C. Dời buổi trước start

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Giáo vụ → Giao diện lịch lớp | Mở chức năng Dời buổi trước start | Điểm bắt đầu. |
| C02 | Giao diện lịch lớp → Hệ thống | Đọc occurrence nguồn, đích dời và preview tác động/xung đột | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| C04 | Hệ thống → Giao diện lịch lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| C05 | Giao diện lịch lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| C06 | Giáo vụ → Giao diện lịch lớp | Chọn ngày/giờ/phòng mới và xác nhận dời | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| C07 | Giao diện lịch lớp → Hệ thống | Gửi yêu cầu dời occurrence trước start | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| C08 | Hệ thống → Hệ thống | Kiểm nguồn trước start/chưa Attendance hoặc linked Score, đích tương lai, chronology/resources/versions và delta thực | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| C09 | Hệ thống → Giao diện lịch lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Nguồn đã start/có dữ liệu, đích đã tới/trùng lịch, stale hoặc không đổi]`; không thực hiện hiệu ứng. |
| C10 | Giao diện lịch lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| C11 | Hệ thống → Hệ thống | Dời buổi theo hợp đồng, giữ cùng nghĩa vụ và history/audit nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| C12 | Hệ thống → Giao diện lịch lớp | Lịch hiện tại sau dời | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| C13 | Giao diện lịch lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| C14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| C15 | Hệ thống → Giao diện lịch lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| C16 | Giao diện lịch lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện lịch lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở chức năng Dời buổi trước start
                            |-- C02 --> H : Đọc occurrence nguồn, đích dời và preview tác động/xung đột
                                                       H --+ C03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. C04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. C05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- C06 --> G : Chọn ngày/giờ/phòng mới và xác nhận dời
                            |-- C07 --> H : Gửi yêu cầu dời occurrence trước start
                                                       H --+ C08: Kiểm nguồn trước start/chưa Attendance hoặc linked Score, đích tương…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C09 ..| : Trả lỗi điều kiện hiện tại
    N <.. C10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C11: Dời buổi theo hợp đồng, giữ cùng nghĩa vụ và history/audit nguyên tử
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C12 ..| : Lịch hiện tại sau dời
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

### D. Ghi không tổ chức và tạo buổi thay thế

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| D01 | Giáo vụ → Giao diện lịch lớp | Mở chức năng Ghi không tổ chức và tạo buổi thay thế | Điểm bắt đầu. |
| D02 | Giao diện lịch lớp → Hệ thống | Đọc occurrence không tổ chức và preview replacement tương lai | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| D03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| D04 | Hệ thống → Giao diện lịch lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| D05 | Giao diện lịch lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| D06 | Giáo vụ → Giao diện lịch lớp | Nhập lý do, ngày/giờ/phòng buổi thay và xác nhận | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| D07 | Giao diện lịch lớp → Hệ thống | Gửi yêu cầu ghi NOT_HELD và tạo replacement | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| D08 | Hệ thống → Hệ thống | Kiểm source occurrence/window/dependencies, reason, đích future hợp lệ/không trùng và cùng Class/type/obligation | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| D09 | Hệ thống → Giao diện lịch lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Nguồn/đích không hợp lệ, có dữ liệu chặn replacement, thiếu reason hoặc phụ thuộc đổi]`; không thực hiện hiệu ứng. |
| D10 | Giao diện lịch lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| D11 | Hệ thống → Hệ thống | Giữ occurrence cũ NOT_HELD/history, tạo linked replacement current cùng obligation; audit nhóm/hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| D12 | Hệ thống → Giao diện lịch lớp | Buổi cũ không tổ chức và buổi thay thế tương lai | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| D13 | Giao diện lịch lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| D14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| D15 | Hệ thống → Giao diện lịch lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| D16 | Giao diện lịch lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện lịch lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- D01 --> G : Mở chức năng Ghi không tổ chức và tạo buổi thay thế
                            |-- D02 --> H : Đọc occurrence không tổ chức và preview replacement tương lai
                                                       H --+ D03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. D04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. D05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- D06 --> G : Nhập lý do, ngày/giờ/phòng buổi thay và xác nhận
                            |-- D07 --> H : Gửi yêu cầu ghi NOT_HELD và tạo replacement
                                                       H --+ D08: Kiểm source occurrence/window/dependencies, reason, đích future hợp…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. D09 ..| : Trả lỗi điều kiện hiện tại
    N <.. D10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ D11: Giữ occurrence cũ NOT_HELD/history, tạo linked replacement current cùng…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. D12 ..| : Buổi cũ không tổ chức và buổi thay thế tương lai
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

### E. Xác nhận đã tổ chức

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| E01 | Giáo vụ → Giao diện lịch lớp | Mở chức năng Xác nhận đã tổ chức | Điểm bắt đầu. |
| E02 | Giao diện lịch lớp → Hệ thống | Đọc occurrence và preview HELD | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| E03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| E04 | Hệ thống → Giao diện lịch lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| E05 | Giao diện lịch lớp → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| E06 | Giáo vụ → Giao diện lịch lớp | Xác nhận buổi đã tổ chức | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| E07 | Giao diện lịch lớp → Hệ thống | Gửi yêu cầu xác nhận occurrence HELD | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| E08 | Hệ thống → Hệ thống | Kiểm Class ONGOING, serverNow≥end, occurrence current PLANNED và full manifest/versions | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| E09 | Hệ thống → Giao diện lịch lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Chưa end, Class không ONGOING, NOT_HELD/history hoặc occurrence đã đổi]`; không thực hiện hiệu ứng. |
| E10 | Giao diện lịch lớp → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| E11 | Hệ thống → Hệ thống | Ghi occurrence HELD với metadata/audit bắt buộc và hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| E12 | Hệ thống → Giao diện lịch lớp | Buổi HELD; coverage nghĩa vụ được tính lại | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| E13 | Giao diện lịch lớp → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| E14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| E15 | Hệ thống → Giao diện lịch lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| E16 | Giao diện lịch lớp → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện lịch lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- E01 --> G : Mở chức năng Xác nhận đã tổ chức
                            |-- E02 --> H : Đọc occurrence và preview HELD
                                                       H --+ E03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. E04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. E05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- E06 --> G : Xác nhận buổi đã tổ chức
                            |-- E07 --> H : Gửi yêu cầu xác nhận occurrence HELD
                                                       H --+ E08: Kiểm Class ONGOING, serverNow≥end, occurrence current PLANNED và full…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. E09 ..| : Trả lỗi điều kiện hiện tại
    N <.. E10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ E11: Ghi occurrence HELD với metadata/audit bắt buộc và hoàn tất nguyên tử
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. E12 ..| : Buổi HELD; coverage nghĩa vụ được tính lại
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

### F. Student xem lịch riêng

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| F01 | Học viên → Giao diện lớp và lịch học | Mở lớp hoặc lịch học của mình | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| F02 | Giao diện lớp và lịch học → Hệ thống | Đọc lớp/lịch theo StudentClassAssignment của mình | Request đọc; bắt đầu activation. |
| F03 | Hệ thống → Hệ thống | Kiểm Student ownership và scope đọc | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| F04 | Hệ thống → Giao diện lớp và lịch học | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Ngoài ownership/hết phiên]`; không đi nhánh query thành công. |
| F05 | Giao diện lớp và lịch học → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| F06 | Hệ thống → Hệ thống | Chiếu lịch/nghĩa vụ hiện tại và history được phép | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| F07 | Hệ thống → Giao diện lớp và lịch học | Lịch học riêng, không có action dời/HELD | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| F08 | Giao diện lớp và lịch học → Học viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| F09 | Hệ thống → Giao diện lớp và lịch học | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| F10 | Giao diện lớp và lịch học → Học viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện lớp và lịch học; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- F01 --> G : Mở lớp hoặc lịch học của mình
                            |-- F02 --> H : Đọc lớp/lịch theo StudentClassAssignment của mình
                                                       H --+ F03: Kiểm Student ownership và scope đọc
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. F04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. F05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ F06: Chiếu lịch/nghĩa vụ hiện tại và history được phép
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. F07 ..| : Lịch học riêng, không có action dời/HELD
    N <.. F08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. F09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. F10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### G. Teacher xem lịch giảng dạy

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| G01 | Giáo viên → Giao diện lớp và lịch dạy | Mở lịch lớp được giao | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| G02 | Giao diện lớp và lịch dạy → Hệ thống | Đọc lịch theo current Teacher assignment | Request đọc; bắt đầu activation. |
| G03 | Hệ thống → Hệ thống | Kiểm Teacher/phân công hiện tại | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| G04 | Hệ thống → Giao diện lớp và lịch dạy | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Mất phân công/hết phiên]`; không đi nhánh query thành công. |
| G05 | Giao diện lớp và lịch dạy → Giáo viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| G06 | Hệ thống → Hệ thống | Chiếu lịch và context giảng dạy được phép | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| G07 | Hệ thống → Giao diện lớp và lịch dạy | Lịch được giao, không có action Giáo vụ | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| G08 | Giao diện lớp và lịch dạy → Giáo viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| G09 | Hệ thống → Giao diện lớp và lịch dạy | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| G10 | Giao diện lớp và lịch dạy → Giáo viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện lớp và lịch dạy; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- G01 --> G : Mở lịch lớp được giao
                            |-- G02 --> H : Đọc lịch theo current Teacher assignment
                                                       H --+ G03: Kiểm Teacher/phân công hiện tại
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. G04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. G05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ G06: Chiếu lịch và context giảng dạy được phép
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. G07 ..| : Lịch được giao, không có action Giáo vụ
    N <.. G08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. G09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. G10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Preview là một read riêng trước lệnh lưu. Khi vẽ A, tách request preview và return candidates trước opt xác nhận lưu; không đặt toàn batch preview và save trong một transaction.”**
2. **“loop [Mỗi candidate tuần] chỉ đặt trong kiểm/tạo của Hệ thống; commit đặt sau loop và rollback toàn bộ nếu bất kỳ candidate không đạt.”**
3. **“NOT_HELD cũ + replacement là một obligation. HELD là action Giáo vụ, không suy từ việc Teacher nhập Attendance.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Từng action có guard thời gian riêng.
- Weekly all-or-none, không skip ngày xung đột.
- Nguồn NOT_HELD không tính progress cùng replacement.
- Không thêm tự bù lịch ngày nghỉ hoặc undo HELD.
- Hai actor đọc Student/Teacher đặt ở hình riêng.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Preview thứ Hai/thứ Tư đạt, khi lưu thứ Tư trùng phòng | Rollback cả batch, không chỉ lưu thứ Hai. |
| Buổi không tổ chức được thay | Old NOT_HELD và future replacement cùng obligation; progress không đếm 2 buổi. |
| serverNow đúng end của occurrence current PLANNED, Class ONGOING | HELD được phép theo predicate ≥end; không đòi chờ thêm phút. |
