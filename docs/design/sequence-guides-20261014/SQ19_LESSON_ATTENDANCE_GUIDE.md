# SQ19 — Hướng dẫn vẽ sequence Teacher lưu điểm danh một LESSON

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV24; W17; F26; UC07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: AttendanceService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/AttendanceService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Teacher lưu các giá trị Có mặt/Vắng được chọn cho một LESSON. Cho phép lưu một phần roster có chủ đích, nhưng tất cả giá trị gửi trong một lần phải cùng thành công hoặc rollback.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ19-A — Đọc sheet và lưu các thay đổi của một LESSON**; actor Giáo viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không điểm danh FINAL_EXAM, đi trễ/có phép/không phép hoặc tự xác nhận buổi đã tổ chức.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện điểm danh theo buổi | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Cửa sổ | Teacher hiện assigned; serverNow≥LESSON start; Class không COMPLETED/CANCELLED, LESSON current và không NOT_HELD. LESSON HELD vẫn được sửa trong cửa sổ nếu Class chưa COMPLETED. |
| Đơn vị | Một save = một LESSON và tập changes người dùng gửi; không phải cả lớp qua mọi buổi và không ép điền mọi Student ngay lần đầu. |
| Giá trị | Present=1 Có mặt, Present=0 Vắng; không gửi Student chưa chọn. Missing record khác Vắng; row chưa có với 0 vẫn là delta tạo mới. |
| Kiểm request | Kiểm đầy đủ roster/phiên bản/expected absence của mọi item supplied trước no-change, kể cả item bằng giá trị cũ. Một item stale/sai abort tất cả. |
| No-change | Chỉ actual changed subset cập nhật enteredBy/enteredAt; unchanged giữ provenance. Toàn supplied values giống thì từ chối no-change, không marker mới. |
| Audit hiện tại | Không ghi AuditLog nghiệp vụ Attendance history riêng. Lưu latest enteredBy/enteredAt và replay completion theo code; đừng sao chép audit bắt buộc của Grade sang Attendance. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Đọc sheet và lưu các thay đổi của một LESSON

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo viên → Giao diện điểm danh theo buổi | Mở chức năng Đọc sheet và lưu các thay đổi của một LESSON | Điểm bắt đầu. |
| A02 | Giao diện điểm danh theo buổi → Hệ thống | Đọc LESSON được giao, roster, các Attendance đã có và capability/versions | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện điểm danh theo buổi | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện điểm danh theo buổi → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo viên → Giao diện điểm danh theo buổi | Chọn Có mặt/Vắng cho các Student cần ghi/sửa và chọn lưu buổi | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện điểm danh theo buổi → Hệ thống | Gửi yêu cầu lưu tập điểm danh một LESSON | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm current Teacher, giờ/occurrence/Class, full supplied versions/absence/roster và có actual delta | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện điểm danh theo buổi | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Mất phân công, ngoài window, row/member stale, input sai hoặc không thay đổi]`; không thực hiện hiệu ứng. |
| A10 | Giao diện điểm danh theo buổi → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Lưu actual changed Attendance với enteredBy/enteredAt và replay completion nguyên tử; không ghi AuditLog Attendance riêng | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện điểm danh theo buổi | Sheet mới, saved subset và phiên bản thực tế | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện điểm danh theo buổi → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện điểm danh theo buổi | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện điểm danh theo buổi → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện điểm danh theo buổi; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Đọc sheet và lưu các thay đổi của một LESSON
                            |-- A02 --> H : Đọc LESSON được giao, roster, các Attendance đã có và…
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Chọn Có mặt/Vắng cho các Student cần ghi/sửa và chọn lưu buổi
                            |-- A07 --> H : Gửi yêu cầu lưu tập điểm danh một LESSON
                                                       H --+ A08: Kiểm current Teacher, giờ/occurrence/Class, full supplied…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Lưu actual changed Attendance với enteredBy/enteredAt và replay…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Sheet mới, saved subset và phiên bản thực tế
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

## 5. Các note nên gắn nguyên văn

1. **“Chưa ghi không là Vắng. Một lần save cho một LESSON; các giá trị được gửi cùng thành công hoặc rollback.”**
2. **“Attendance không tự HELD. Thiếu required Attendance chặn Class COMPLETE; sau COMPLETE không được sửa Attendance.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Không default-all hoặc blank→absent.
- Có nhánh item stale dù item không đổi.
- Có mặt/Vắng đều là thao tác chủ động.
- Không vẽ AuditLog history riêng hoặc Student xem điểm danh nếu không có quyền.
- Không autosave/tự mở cửa sổ 24/48 giờ.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Chưa có Attendance, Teacher chọn Vắng | Tạo row present=0 là thay đổi thật; missing không tương đương 0. |
| Một item unchanged nhưng RV stale trong batch | Validation chặn toàn lần lưu, không bỏ qua item để lưu phần khác. |
| LESSON HELD, Class chưa COMPLETE | Có thể sửa Attendance trong cửa sổ; HELD không tự khóa như Class COMPLETED. |
