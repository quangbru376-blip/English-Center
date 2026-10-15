# SQ22 — Hướng dẫn vẽ sequence Giáo vụ trả lại/chốt và Student xem điểm đã công bố

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV08,20,38; W19; F23,F12; UC07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: GradeService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/GradeService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Giáo vụ review bảng điểm lớp COMPLETED đã SUBMITTED, rồi chọn RETURN hoặc FINALIZE. Student đọc FINALIZED ở thời điểm khác, không tham gia xác nhận hoặc chờ một cuộc gọi đồng bộ từ Giáo vụ.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ22-A — Trả lại bảng điểm cả lớp**; actor Giáo vụ.
- **SQ22-B — Chốt/công bố bảng điểm cả lớp**; actor Giáo vụ.
- **SQ22-C — Student xem danh sách kết quả đã chốt**; actor Học viên.
- **SQ22-D — Student xem chi tiết một kết quả đã chốt**; actor Học viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không individual finalize, mở lại FINALIZED, Teacher tự sửa khi SUBMITTED hoặc approval tier Director.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo vụ; Học viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện duyệt bảng điểm; Giao diện kết quả học tập | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Review | AcademicAffairs NORMAL, Class COMPLETED, toàn cohort SUBMITTED cho RETURN/FINALIZE; quyền review không gồm Teacher DRAFT values. |
| RETURN | Reason bắt buộc, cả cohort RETURNED; giữ score/comment, metadata/reason/audit history. Không tự sửa điểm thay Teacher. |
| FINALIZE | Kiểm toàn current cohort, Score/config/schedule/report/Registration dependencies. Tính decimal weighted total, half-up một lần 2 chữ số; lưu final_score chỉ tại đây. |
| Đạt | FINALIZED rounded final_score≥50; không minimum component hoặc attendance passing gate mới. Thiếu điểm chặn chốt, khác kết quả không đạt. |
| Hồ sơ | Class COMPLETED + report FINALIZED làm hết unfinished theo policy kể cả không đạt; Registration vẫn không có status COMPLETED mới. |
| Bất biến | FINALIZED không reopen/override; không quyền chốt cho Teacher/Director/Admin. Toàn lớp + totals/metadata/unfinished/audit/replay commit cùng nhau. |
| Student | Chỉ own FINALIZED list/detail, server total/passed/components/comment/context đã công bố; không peer/DRAFT/SUBMITTED/RETURNED values. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Trả lại bảng điểm cả lớp

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo vụ → Giao diện duyệt bảng điểm | Mở chức năng Trả lại bảng điểm cả lớp | Điểm bắt đầu. |
| A02 | Giao diện duyệt bảng điểm → Hệ thống | Đọc review cohort và preview RETURN hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện duyệt bảng điểm | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo vụ → Giao diện duyệt bảng điểm | Nhập lý do và xác nhận trả lại cả lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện duyệt bảng điểm → Hệ thống | Gửi yêu cầu RETURN cả lớp với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm Class COMPLETED, toàn reports SUBMITTED, cohort/manifests/versions và reason hợp lệ | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện duyệt bảng điểm | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Thiếu reason, cohort không toàn SUBMITTED hoặc dữ liệu review cũ]`; không thực hiện hiệu ứng. |
| A10 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Đổi toàn reports RETURNED, lưu reviewer/time/reason, giữ score/comment; grouped audit và completion nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện duyệt bảng điểm | Cả lớp RETURNED với lý do cho Teacher sửa/gửi lại | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện duyệt bảng điểm | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện duyệt bảng điểm; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Trả lại bảng điểm cả lớp
                            |-- A02 --> H : Đọc review cohort và preview RETURN hiện tại
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Nhập lý do và xác nhận trả lại cả lớp
                            |-- A07 --> H : Gửi yêu cầu RETURN cả lớp với lý do
                                                       H --+ A08: Kiểm Class COMPLETED, toàn reports SUBMITTED, cohort/manifests/versions…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Đổi toàn reports RETURNED, lưu reviewer/time/reason, giữ score/comment;…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Cả lớp RETURNED với lý do cho Teacher sửa/gửi lại
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

### B. Chốt/công bố bảng điểm cả lớp

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo vụ → Giao diện duyệt bảng điểm | Mở chức năng Chốt/công bố bảng điểm cả lớp | Điểm bắt đầu. |
| B02 | Giao diện duyệt bảng điểm → Hệ thống | Đọc preview FINALIZE toàn cohort với các tổng tính từ server | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện duyệt bảng điểm | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo vụ → Giao diện duyệt bảng điểm | Xác nhận chốt bảng điểm toàn lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện duyệt bảng điểm → Hệ thống | Gửi yêu cầu FINALIZE cả lớp | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm COMPLETED/toàn SUBMITTED, đủ mọi component/current valid links, frozen weights và toàn manifests/versions | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện duyệt bảng điểm | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Thiếu điểm/config không hợp lệ, report/score/roster/schedule đổi hoặc lỗi tính/ghi]`; không thực hiện hiệu ứng. |
| B10 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Tính/lưu final_score rounded một lần; mọi report FINALIZED/review metadata, xử lý unfinished và audit nhóm/completion nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện duyệt bảng điểm | Toàn lớp FINALIZED; Student được đọc kết quả riêng sau commit | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện duyệt bảng điểm | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện duyệt bảng điểm → Giáo vụ | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo vụ; G = Giao diện duyệt bảng điểm; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Chốt/công bố bảng điểm cả lớp
                            |-- B02 --> H : Đọc preview FINALIZE toàn cohort với các tổng tính từ server
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Xác nhận chốt bảng điểm toàn lớp
                            |-- B07 --> H : Gửi yêu cầu FINALIZE cả lớp
                                                       H --+ B08: Kiểm COMPLETED/toàn SUBMITTED, đủ mọi component/current valid links,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Tính/lưu final_score rounded một lần; mọi report FINALIZED/review…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Toàn lớp FINALIZED; Student được đọc kết quả riêng sau commit
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

### C. Student xem danh sách kết quả đã chốt

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| C01 | Học viên → Giao diện kết quả học tập | Mở danh sách kết quả học tập đã chốt | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| C02 | Giao diện kết quả học tập → Hệ thống | Yêu cầu danh sách FINALIZED của Student hiện tại theo lọc/phân trang | Request đọc; bắt đầu activation. |
| C03 | Hệ thống → Hệ thống | Kiểm ownership/Student và FINALIZED hiện tại, projection không chứa quản trị hoặc peer data | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| C04 | Hệ thống → Giao diện kết quả học tập | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Ngoài ownership, chưa FINALIZED hoặc hết phiên]`; không đi nhánh query thành công. |
| C05 | Giao diện kết quả học tập → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| C06 | Hệ thống → Hệ thống | Chiếu official score/passed/components/comment/lịch công bố được phép từ hệ thống | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| C07 | Hệ thống → Giao diện kết quả học tập | Danh sách các lần học riêng đã FINALIZED, đúng paging và scope | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| C08 | Giao diện kết quả học tập → Học viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| C09 | Hệ thống → Giao diện kết quả học tập | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| C10 | Giao diện kết quả học tập → Học viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện kết quả học tập; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- C01 --> G : Mở danh sách kết quả học tập đã chốt
                            |-- C02 --> H : Yêu cầu danh sách FINALIZED của Student hiện tại theo lọc/phân trang
                                                       H --+ C03: Kiểm ownership/Student và FINALIZED hiện tại, projection không chứa…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. C04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. C05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ C06: Chiếu official score/passed/components/comment/lịch công bố được phép…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. C07 ..| : Danh sách các lần học riêng đã FINALIZED, đúng paging và scope
    N <.. C08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. C09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. C10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### D. Student xem chi tiết một kết quả đã chốt

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| D01 | Học viên → Giao diện kết quả học tập | Chọn một kết quả đã chốt để xem chi tiết | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| D02 | Giao diện kết quả học tập → Hệ thống | Yêu cầu chi tiết kết quả FINALIZED đã chọn | Request đọc; bắt đầu activation. |
| D03 | Hệ thống → Hệ thống | Kiểm Student hiện tại, ownership, report FINALIZED và scope dữ liệu công bố | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| D04 | Hệ thống → Giao diện kết quả học tập | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Report không FINALIZED hoặc ngoài ownership]`; không đi nhánh query thành công. |
| D05 | Giao diện kết quả học tập → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| D06 | Hệ thống → Hệ thống | Chiếu official score/passed/components/comment/context lịch công bố được phép, không peer hoặc management data | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| D07 | Hệ thống → Giao diện kết quả học tập | Chi tiết lần học riêng FINALIZED | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| D08 | Giao diện kết quả học tập → Học viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| D09 | Hệ thống → Giao diện kết quả học tập | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| D10 | Giao diện kết quả học tập → Học viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện kết quả học tập; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- D01 --> G : Chọn một kết quả đã chốt để xem chi tiết
                            |-- D02 --> H : Yêu cầu chi tiết kết quả FINALIZED đã chọn
                                                       H --+ D03: Kiểm Student hiện tại, ownership, report FINALIZED và scope dữ liệu…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. D04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. D05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ D06: Chiếu official score/passed/components/comment/context lịch công bố…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. D07 ..| : Chi tiết lần học riêng FINALIZED
    N <.. D08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. D09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. D10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Chốt là all-or-none cho cả lớp. Không thông báo một Student thành công trong lúc các report còn đang lưu.”**
2. **“Decimal intermediate, half-up hai chữ số một lần rồi so ≥50. Không dùng floating point hoặc round từng component.”**
3. **“Student không nhận message tự động từ Giáo vụ; hình C là query mới sau khi đã FINALIZED.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- RETURN và FINALIZE là hai action riêng.
- Không đồng nhất Class COMPLETE với grade FINALIZE.
- Unfinished được xử lý cả khi không đạt.
- Không dữ liệu unpublished trong Student payload.
- Có nhánh race return/finalize và rollback toàn cohort.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Hai score 49,99 và 50,00, weight mỗi component 50% | Tổng decimal 49,995; half-up một lần thành 50,00 và đạt sau FINALIZED. |
| Kết quả chốt 40,00 | Không đạt nhưng report FINALIZED và hồ sơ hết unfinished theo policy, không giữ hồ sơ mở chỉ vì trượt. |
| Một report lỗi khi finalize | Rollback cả cohort/totals/unfinished/audit; Student không thấy công bố từng phần. |
