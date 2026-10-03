# SQ07 — Hướng dẫn vẽ sequence đăng ký khóa học và hủy hồ sơ

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Bố cục và mã thông điệp là gợi ý
trình bày, không tạo chức năng hoặc quyết định sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ07;
[FUNCTION_SPEC](FUNCTION_SPEC.md) NV02/phần validation chung;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W05;
[requirements](../requirements.md) ADR-015–017/021–022/031/038–039/090/093/121/133.

## 1. Mục tiêu và cách chia hình

SQ07 là đăng ký **khóa học**, không phải tạo tài khoản ở SQ01. Học viên đã
có tài khoản và phiên thường hợp lệ.

Nên vẽ hai hình cùng nhóm:

- **SQ07-A — Đăng ký khóa học:** xem khóa, chọn khóa, tạo Registration PENDING.
- **SQ07-B — Hủy hồ sơ:** xem ảnh hưởng dự kiến, xác nhận hủy, chuyển CANCELLED.

Hai hình là hai lần tương tác riêng; không vẽ hủy là bước bắt buộc sau đăng ký.
Hồ sơ cũ có thể được mở từ F05/F03. Không cần vẽ lại đăng nhập; ghi tiền
điều kiện và tham chiếu SQ03 nếu cần.

**Thành công khi tạo:** đúng một hồ sơ mới của học viên cho khóa chọn, trạng
thái PENDING, ghi audit bắt buộc; chưa có lượt thi/Band/Invoice/lớp.

**Thành công khi hủy:** hồ sơ CANCELLED, kết thúc điều kiện chưa hoàn tất;
booking hoạt động chưa bắt đầu bị hủy, chỗ được giải phóng, giữ lịch sử và
không tiếp tục chấm/ghi vắng/sửa điểm cho hồ sơ đã hủy.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Học viên | Actor Student, tự thao tác trên hồ sơ của mình. |
| 2 | Giao diện khóa & hồ sơ | Boundary, F04/F05; hiển thị khóa/hồ sơ/preview hủy và kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control, kiểm tra quyền/điều kiện, tạo/hủy hồ sơ và xử lý phụ thuộc. |

Dùng message yêu cầu, phản hồi nét đứt, self-message trên Hệ thống,
activation, khung `alt` và note. SQ07-B thêm `opt [Học viên đồng ý hủy]`
sau preview: đóng preview/không đồng ý thì không gửi lệnh hủy, không đổi dữ liệu.

Không cần lifeline Tuyển sinh, Teacher, Course hoặc Database. Tuyển sinh hủy
hồ sơ sau CONFIRMED là SQ12, không đưa quyền đó vào actor Học viên ở SQ07.

## 3. Dữ liệu và điều kiện

| Nội dung | Quy tắc đã chốt |
| --- | --- |
| Tạo | Học viên chọn Course ACTIVE; Hệ thống xác định chủ hồ sơ/trạng thái/thời điểm, không nhận Band/status/student_id khác do người dùng tự khai báo. |
| Một hồ sơ chưa kết thúc | Chỉ một Registration chưa kết thúc cho cùng Học viên/Course, kể cả yêu cầu đồng thời; được học khóa khác đồng thời. |
| Định nghĩa kết thúc | CANCELLED/REJECTED hoặc Class COMPLETED và GradeReport của lượt học đó FINALIZED; không cần đạt môn. Không có Registration COMPLETED. |
| Đăng ký lại | Tạo hồ sơ mới/thi lại từ đầu, không copy điểm/Band. Course của hồ sơ hiện có bất biến. |
| Hủy bởi Student | Chỉ hồ sơ của mình trước CONFIRMED: PENDING/TESTING/BAND_ASSIGNED; không hủy CONFIRMED/ADMITTED/đã kết thúc bằng quyền Student. |
| Booking liên quan | Hủy booking hoạt động chưa start, kể cả ca CLOSED trước start; không dùng guard hủy booking độc lập của SQ09 để chặn việc hủy hồ sơ này. |
| Lịch đã bắt đầu | Giữ lịch sử/lượt thi/điểm/Band; hủy hồ sơ không xóa dữ liệu đã có và ngừng xử lý kết quả tiếp. |
| Preview | Hiển thị ảnh hưởng đúng scope, không giữ chỗ/khóa trạng thái; phải kiểm tra lại khi gửi hủy. |
| Lý do | Không tự thêm yêu cầu Student phải nhập lý do hủy; lý do bắt buộc của Tuyển sinh thuộc luồng khác. |
| Nguyên tử | Tạo hoặc hủy cùng các cập nhật phụ thuộc và audit bắt buộc; lỗi thì không lưu một phần. |

W05 mô tả cơ chế hủy chung có Invoice UNPAID nếu có. Với **Student trước
CONFIRMED** trong SQ07, hóa đơn chưa được phát hành theo hành trình hợp lệ.
Vì vậy không vẽ Invoice là dữ liệu sẵn có hoặc tự tạo hóa đơn để hủy. Hủy
hồ sơ sau xác nhận/duyệt, kèm hóa đơn UNPAID, nằm ở SQ12 của Tuyển sinh.

## 4. SQ07-A — Thứ tự thông điệp tạo hồ sơ

| Mã | Từ → Đến | Nhãn thông điệp | Vị trí/ý nghĩa |
| --- | --- | --- | --- |
| A01 | Học viên → Giao diện | Xem khóa học | Bắt đầu F04. |
| A02 | Giao diện → Hệ thống | Yêu cầu danh sách khóa được phép xem | Quyền Student. |
| A03 | Hệ thống → Giao diện | Trả danh sách khóa được phép xem | Không tạo hồ sơ khi đọc. |
| A04 | Giao diện → Học viên | Hiển thị khóa học | Chỉ Course ACTIVE được chọn để tạo. |
| A05 | Học viên → Giao diện | Chọn khóa và đăng ký | Không chọn Band/lớp. |
| A06 | Giao diện → Hệ thống | Yêu cầu tạo hồ sơ đăng ký khóa | Gửi khóa chọn. |
| A07 | Hệ thống → Hệ thống | Kiểm tra quyền, khóa ACTIVE và hồ sơ chưa kết thúc | Kiểm tra tại lúc ghi, không tin danh sách cũ. |
| A08 | Hệ thống → Giao diện | Trả kết quả không thể đăng ký | `alt [Không đủ điều kiện tạo hồ sơ]`. |
| A09 | Giao diện → Học viên | Hiển thị thông báo phù hợp | Không lộ hồ sơ người khác. |
| A10 | Hệ thống → Hệ thống | Tạo Registration PENDING và ghi audit bắt buộc | `else [Đủ điều kiện tạo hồ sơ]`; nguyên tử. |
| A11 | Hệ thống → Giao diện | Trả hồ sơ mới PENDING | `alt` trong: `[Tạo đầy đủ thành công]`. |
| A12 | Giao diện → Học viên | Thông báo thành công và hiển thị hồ sơ | Bước tiếp theo là đặt ca ở SQ09, không tự đặt. |
| A13 | Hệ thống → Hệ thống | Hoàn tác thay đổi của yêu cầu | `else [Trùng đồng thời hoặc không thể hoàn tất]`. |
| A14 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất | Không có hồ sơ/audit tạo một phần. |
| A15 | Giao diện → Học viên | Thông báo lỗi và tải lại hồ sơ có quyền | Không tự tạo yêu cầu mới để né hồ sơ trùng. |

Nếu đọc danh sách thất bại/phiên mất hiệu lực, không đi tiếp từ kết quả cũ
như một lựa chọn còn hợp lệ. Có thể ghi note thay vì thêm nhánh vào hình chính.

## 5. SQ07-B — Thứ tự thông điệp hủy hồ sơ

| Mã | Từ → Đến | Nhãn thông điệp | Vị trí/ý nghĩa |
| --- | --- | --- | --- |
| B01 | Học viên → Giao diện | Chọn hồ sơ của mình và yêu cầu hủy | Bắt đầu từ chi tiết hồ sơ. |
| B02 | Giao diện → Hệ thống | Yêu cầu xem trước ảnh hưởng hủy | Chưa ghi thay đổi. |
| B03 | Hệ thống → Hệ thống | Kiểm tra quyền, trạng thái và các booking bị ảnh hưởng | Tính theo thời điểm hiện tại. |
| B04 | Hệ thống → Giao diện | Trả kết quả không thể hủy | `alt [Không đủ điều kiện hủy]`. |
| B05 | Giao diện → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh không đủ quyền/điều kiện. |
| B06 | Hệ thống → Giao diện | Trả ảnh hưởng hủy dự kiến | `else [Đủ điều kiện tại thời điểm preview]`. |
| B07 | Giao diện → Học viên | Hiển thị ảnh hưởng và yêu cầu xác nhận | Chưa CANCELLED. |
| B08 | Học viên → Giao diện | Xác nhận hủy hồ sơ | `opt [Học viên đồng ý hủy]`. |
| B09 | Giao diện → Hệ thống | Gửi yêu cầu hủy hồ sơ | Không gửi nếu đóng preview. |
| B10 | Hệ thống → Hệ thống | Kiểm tra lại quyền, trạng thái và booking | Preview cũ có thể không còn đúng. |
| B11 | Hệ thống → Hệ thống | Hủy hồ sơ/booking tương lai, giải phóng chỗ, cập nhật kết thúc và audit | Nhánh kết quả `[Hoàn tất hủy nguyên tử]`; không xóa lịch sử đã bắt đầu. |
| B12 | Hệ thống → Giao diện | Trả hồ sơ CANCELLED | Sau khi hoàn tất tất cả cập nhật. |
| B13 | Giao diện → Học viên | Thông báo đã hủy và tải lại hồ sơ | Không trở về PENDING. |
| B14 | Hệ thống → Hệ thống | Không ghi hoặc hoàn tác thay đổi của yêu cầu | `[Điều kiện đổi hoặc không thể hoàn tất]`; nếu guard thất bại thì không ghi. |
| B15 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất hủy | Gồm race xác nhận/start hoặc audit lỗi. |
| B16 | Giao diện → Học viên | Thông báo lỗi và tải lại hồ sơ | Không dùng preview cũ để ghi đè. |

Không suy B11 luôn chạy sau B10; đó là một nhánh kết quả của lần thực hiện.
Lỗi/điều kiện thay đổi đi B14–B16, không có thông báo đã hủy.

## 6. Phác thảo bố cục bằng đường gạch

```text
SQ07-A — Đăng ký khóa học

        Học viên                            Giao diện khóa & hồ sơ                Hệ thống
        |                                   |                                     |
        |- A01: Xem khóa học --------------->                                     |
        |                                   |- A02: Yêu cầu danh sách khóa ------->
        |                                   <. A03: Danh sách được phép xem ......|
        <. A04: Hiển thị khóa học ..........|                                     |
        |- A05: Chọn khóa/đăng ký ---------->                                     |
        |                                   |- A06: Yêu cầu tạo hồ sơ ------------>
        |                                   |                                     |----+  A07: Kiểm tra quyền,
        |                                   |                                     |<---+  khóa và hồ sơ chưa kết thúc
+- alt [Không đủ điều kiện tạo hồ sơ] -----------------------------------------------------------------------------------+
|       |                                   <. A08: Không thể đăng ký ............|                                      |
|       <. A09: Thông báo phù hợp ..........|                                     |                                      |
+- [Đủ điều kiện tạo hồ sơ] ---------------------------------------------------------------------------------------------+
|       |                                   |                                     |----+  A10: Tạo PENDING               |
|       |                                   |                                     |<---+  và audit cùng thao tác         |
| +- alt [Tạo đầy đủ thành công] --------------------------------------------------------------------------------------+ |
| |     |                                   <. A11: Hồ sơ PENDING ................|                                    | |
| |     <. A12: Thành công/hồ sơ ...........|                                     |                                    | |
| +- [Trùng đồng thời hoặc không thể hoàn tất] ------------------------------------------------------------------------+ |
| |     |                                   |                                     |----+  A13: Hoàn tác                | |
| |     |                                   |                                     |<---+  thay đổi của yêu cầu         | |
| |     |                                   <. A14: Không thể hoàn tất ...........|                                    | |
| |     <. A15: Lỗi/tải lại hồ sơ ..........|                                     |                                    | |
| +--------------------------------------------------------------------------------------------------------------------+ |
+------------------------------------------------------------------------------------------------------------------------+

SQ07-B — Học viên hủy hồ sơ (hình riêng)

        Học viên                            Giao diện khóa & hồ sơ                Hệ thống
        |                                   |                                     |
        |- B01: Chọn hồ sơ/hủy ------------->                                     |
        |                                   |- B02: Yêu cầu preview hủy ---------->
        |                                   |                                     |----+  B03: Kiểm tra quyền,
        |                                   |                                     |<---+  state và ảnh hưởng hủy
+- alt [Không đủ điều kiện hủy] -----------------------------------------------------------------------------------------+
|       |                                   <. B04: Không thể hủy ................|                                      |
|       <. B05: Thông báo phù hợp ..........|                                     |                                      |
+- [Đủ điều kiện tại thời điểm preview] ---------------------------------------------------------------------------------+
|       |                                   <. B06: Ảnh hưởng dự kiến ............|                                      |
|       <. B07: Hiển thị/xác nhận ..........|                                     |                                      |
| +- opt [Học viên đồng ý hủy] ----------------------------------------------------------------------------------------+ |
| |     |- B08: Xác nhận hủy --------------->                                     |                                    | |
| |     |                                   |- B09: Gửi yêu cầu hủy -------------->                                    | |
| |     |                                   |                                     |----+  B10: Kiểm tra lại            | |
| |     |                                   |                                     |<---+  quyền/state/booking          | |
| | +- alt [Hoàn tất hủy nguyên tử] ---------------------------------------------------------------------------------+ | |
| | |   |                                   |                                     |----+  B11: CANCELLED,            | | |
| | |   |                                   |                                     |<---+  hủy booking tương lai      | | |
| | |   |   B11: giải phóng chỗ, giữ lịch sử, cập nhật kết thúc hồ sơ và audit cùng thao tác.                        | | |
| | |   |                                   <. B12: Hồ sơ CANCELLED ..............|                                  | | |
| | |   <. B13: Đã hủy/tải lại .............|                                     |                                  | | |
| | +- [Điều kiện đổi hoặc không thể hoàn tất] ----------------------------------------------------------------------+ | |
| | |   |                                   |                                     |----+  B14: Không lưu/            | | |
| | |   |                                   |                                     |<---+  hoàn tác nếu đã ghi        | | |
| | |   |                                   <. B15: Không thể hoàn tất ...........|                                  | | |
| | |   <. B16: Lỗi/tải lại hồ sơ ..........|                                     |                                  | | |
| | +----------------------------------------------------------------------------------------------------------------+ | |
| +--------------------------------------------------------------------------------------------------------------------+ |
+------------------------------------------------------------------------------------------------------------------------+
```

Các message dài được rút gọn trong hình chữ; dùng bảng thông điệp để đặt
nhãn đầy đủ trong EA. Tất cả self-message nằm trên Hệ thống trong đúng operand.
`opt` không tạo nhánh hủy dữ liệu khi Học viên chỉ đóng preview.

## 7. Note và cách dựng

Note nên gắn:

1. A07/A10: **“Một hồ sơ chưa kết thúc/Học viên/Course. Hồ sơ mới PENDING;
   chưa đặt ca, chưa có Band/hóa đơn/lớp; không copy kết quả cũ.”**
2. B07/B10: **“Preview không giữ trạng thái; Hệ thống kiểm tra lại khi hủy.”**
3. B11: **“Hủy hồ sơ + booking hoạt động chưa start (kể cả ca CLOSED trước
   start) + giải phóng chỗ + kết thúc hồ sơ + audit bắt buộc cùng thao tác.
   Giữ lịch sử đã bắt đầu, điểm và Band; ngừng xử lý kết quả trên hồ sơ hủy.”**
4. B03: **“Student chỉ hủy trước CONFIRMED. Hủy bởi Tuyển sinh sau xác nhận
   hoặc duyệt là SQ12, không cấp quyền đó cho Student.”**

Tạo hai hình SQ07-A/B; đặt ba lifeline, vẽ các thông điệp đầu, đặt fragment
theo phác thảo rồi điền phản hồi đúng nhánh. Thêm activation và các note.
Không thêm `loop` hoặc tự động chuyển sang thi/thu tiền sau tạo hồ sơ.

## 8. Checklist

- Tài khoản và hồ sơ cá nhân đã có từ SQ01; không tạo lại chúng trong SQ07.
- Khóa còn ACTIVE lúc tạo; chỉ một hồ sơ chưa kết thúc cho cùng khóa/học viên.
- Thành công tạo PENDING; không TESTING cho tới booking đầu tiên ở SQ09.
- Hủy Student không áp cho CONFIRMED/ADMITTED; không tự yêu cầu lý do mới.
- Preview chưa hủy; đóng preview không làm thay đổi dữ liệu.
- Booking CLOSED nhưng chưa start vẫn bị hủy phụ thuộc khi hủy hồ sơ hợp lệ.
- Lịch sử không bị xóa, không refund/chuyển khóa/chuyển lớp/mở lại hồ sơ.
- Audit lỗi thì rollback; không báo thành công sau lỗi.
- Gửi lặp không tạo hiệu ứng thứ hai; mất phản hồi thì đọc lại hồ sơ/operation
  có quyền, không đoán thất bại rồi tự tạo/hủy bằng một yêu cầu mới.
