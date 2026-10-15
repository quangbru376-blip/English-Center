# SQ13 — Hướng dẫn vẽ sequence tra cứu và in hóa đơn

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Bố cục/mã thông điệp là gợi ý,
không tạo quyền hoặc chức năng sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md) SQ13;
[FUNCTION_SPEC](../FUNCTION_SPEC.md) NV06/NV27;
[WORKFLOW_SPEC](../WORKFLOW_SPEC.md) W11;
[requirements](../../requirements.md) ADR-039/041/043–047/083/096/112/127.

## 1. Mục tiêu và cách chia hình

Hóa đơn đã phát hành khi Tuyển sinh duyệt hồ sơ ở SQ12. **Xem/in không tạo
hóa đơn mới, xác nhận thanh toán hoặc đổi trạng thái Invoice.**

Nên vẽ hai hình vì quyền và dữ liệu khác nhau:

- **SQ13-A — Học viên xem/in:** F10, chỉ Invoice của mình; in bằng trình duyệt,
  lấy lại dữ liệu hợp lệ trước khi mở bản in.
- **SQ13-B — Kế toán tra cứu:** F28, lọc danh sách/chi tiết Invoice và Payment
  đúng quyền kế toán. Không thêm quyền in/xuất danh mục Invoice riêng.

Điều kiện trước: actor đăng nhập bằng phiên thường còn hợp lệ. Invoice
không tồn tại/ngoài scope phải được xử lý mà không lộ dữ liệu người khác.
Không đưa Học viên và Kế toán thành hai actor nối tiếp trên cùng yêu cầu.
Xác nhận thu tiền/sửa Payment thuộc SQ14; CSV báo cáo tài chính thuộc SQ23.

## 2. Thành phần cần đặt trong EA

| Hình | Actor trái | Boundary giữa | Control phải |
| --- | --- | --- | --- |
| SQ13-A | Học viên | Giao diện hóa đơn F10 | Hệ thống quản lý trung tâm |
| SQ13-B | Kế toán | Giao diện hóa đơn F28 | Hệ thống quản lý trung tâm |

Dùng message, phản hồi nét đứt, self-message/activation, `alt` và `opt`.
Các bước kiểm tra/đọc trên Hệ thống. Dựng bản in là self-message trên Giao
diện; chức năng in trình duyệt được mô tả tại Boundary, không cần máy in
hoặc dịch vụ PDF thành lifeline.

## 3. Dữ liệu, phạm vi và trạng thái

| Nội dung | Quy tắc đã chốt |
| --- | --- |
| Học viên được xem | Số Invoice, tên chủ hồ sơ hiện tại, khóa, tiền snapshot, ngày phát hành/hạn, UNPAID/PAID/CANCELLED và cảnh báo/số ngày quá hạn. |
| Học viên không tự thêm | Payment method/reference/người xác nhận hoặc dữ liệu nhân sự ngoài scope; không sửa amount/due_date/status. |
| Kế toán được xem | Invoice mọi trạng thái kể cả CANCELLED, tiền snapshot/ngày/hạn/quá hạn, liên hệ cần thiết; Payment method/reference/amount và người/thời điểm xác nhận nếu có. |
| Kế toán lọc | Số hóa đơn, khóa, tên/email/SĐT học viên, trạng thái hiện tại, khoảng ngày phát hành tùy chọn và chỉ quá hạn. Tìm kiếm cá nhân không URL/storage. |
| Danh sách | Phân trang/sort ổn định; rỗng là không có dữ liệu phù hợp, không nhất thiết là lỗi. |
| Quá hạn | Chỉ Invoice UNPAID với due_date trước ngày Việt Nam hiện tại; ngày đến hạn chưa quá hạn. Chỉ báo, không enum hoặc tiền phạt mới. |
| Snapshot | Tiền hóa đơn không đổi theo giá khóa hiện tại; hạn = ngày phát hành VN + 7 ngày lịch, không tính lại theo ngày xem/in. |
| In | Học viên lấy dữ liệu mới có quyền, dựng A4 dọc/đen trắng, ẩn menu/nút. UNPAID ghi chưa thanh toán và không phải xác nhận thu tiền; CANCELLED ghi đã hủy; PAID đúng dữ liệu. |
| Lỗi lấy mới | Không mở bản in từ dữ liệu cũ như dữ liệu mới khi lỗi/mất quyền. |
| Export | Không API/file PDF/biên lai riêng, dấu/chữ ký giả hoặc quyền in nhân sự. Browser save-as-PDF là chức năng trình duyệt, không API sản phẩm. |

Các phép đọc/in không tự audit mọi lượt xem hoặc chuyển PAID; không tạo
Payment khi người dùng bấm In.

## 4. SQ13-A — Thông điệp Học viên xem/in

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| A01 | Học viên → Giao diện | Mở hóa đơn của mình. |
| A02 | Giao diện → Hệ thống | Yêu cầu đọc Invoice đã chọn. |
| A03 | Hệ thống → Hệ thống | Kiểm tra phiên/ownership và đọc dữ liệu hiện tại. |
| A04 | Hệ thống → Giao diện | Không thể truy cập; `alt [Không thể đọc hóa đơn]`. |
| A05 | Giao diện → Học viên | Thông báo phù hợp; hết phiên thì về đăng nhập, không lộ hóa đơn khác. |
| A06 | Hệ thống → Giao diện | Trả dữ liệu Invoice trong scope Student; `else [Đọc thành công đúng quyền]`. |
| A07 | Giao diện → Học viên | Hiển thị hóa đơn/trạng thái/cảnh báo. |
| A08 | Học viên → Giao diện | Chọn “In hóa đơn”; `opt [Học viên chọn In hóa đơn]`. |
| A09 | Giao diện → Hệ thống | Yêu cầu lấy lại dữ liệu mới trước khi in. |
| A10 | Hệ thống → Hệ thống | Kiểm tra lại quyền và đọc Invoice hiện tại. |
| A11 | Hệ thống → Giao diện | Trả dữ liệu mới có quyền; `alt [Lấy mới thành công đúng quyền]`. |
| A12 | Giao diện → Giao diện | Dựng bản in A4 từ dữ liệu vừa lấy. |
| A13 | Giao diện → Học viên | Mở chức năng in trình duyệt. |
| A14 | Hệ thống → Giao diện | Không thể lấy mới; `else [Lấy mới lỗi hoặc không còn quyền]`. |
| A15 | Giao diện → Học viên | Thông báo lỗi, không mở bản in từ dữ liệu cũ. |

A13 chỉ mở chức năng in, không khẳng định giấy đã in thành công. Người dùng
có thể hủy hộp thoại in; Invoice vẫn giữ trạng thái, không cần message ghi
“đã in” vào Hệ thống. Không chọn In thì chỉ xem và kết thúc.

## 5. SQ13-B — Thông điệp Kế toán tra cứu

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| B01 | Kế toán → Giao diện | Chọn bộ lọc và tra cứu Invoice. |
| B02 | Giao diện → Hệ thống | Gửi yêu cầu danh sách theo bộ lọc. |
| B03 | Hệ thống → Hệ thống | Kiểm tra phiên/role, bộ lọc và phạm vi được xem. |
| B04 | Hệ thống → Giao diện | Lỗi bộ lọc/không có quyền; `alt [Không thể tra cứu]`. |
| B05 | Giao diện → Kế toán | Hiển thị thông báo phù hợp. |
| B06 | Hệ thống → Giao diện | Trả danh sách phân trang; `else [Tra cứu hợp lệ]`. |
| B07 | Giao diện → Kế toán | Hiển thị kết quả hoặc không có dữ liệu phù hợp. |
| B08 | Kế toán → Giao diện | Chọn hóa đơn xem chi tiết; `opt [Kế toán chọn một hóa đơn]`. |
| B09 | Giao diện → Hệ thống | Yêu cầu chi tiết Invoice. |
| B10 | Hệ thống → Hệ thống | Kiểm tra lại quyền và đọc chi tiết hiện tại. |
| B11 | Hệ thống → Giao diện | Trả Invoice/Payment trong scope; `alt [Đọc thành công đúng quyền]`. |
| B12 | Giao diện → Kế toán | Hiển thị chi tiết và hành động được phép theo trạng thái. |
| B13 | Hệ thống → Giao diện | Không thể truy cập; `else [Không thể đọc chi tiết]`. |
| B14 | Giao diện → Kế toán | Thông báo phù hợp, không hiện dữ liệu ngoài quyền. |

Hành động xác nhận thanh toán/sửa metadata nếu được chọn sẽ sang SQ14;
không thực hiện chúng chỉ vì đã mở chi tiết. Không tự đưa nút In/CSV vào F28.

## 6. Phác thảo bố cục bằng đường gạch

```text
SQ13-A — Học viên xem và in hóa đơn

        Học viên                            Giao diện hóa đơn F10                   Hệ thống
        |                                   |                                       |
        |- A01: Mở hóa đơn của mình -------->                                       |
        |                                   |- A02: Yêu cầu đọc hóa đơn ------------>
        |                                   |                                       |----+  A03: Kiểm tra quyền
        |                                   |                                       |<---+  và dữ liệu hiện tại
+- alt [Không thể đọc hóa đơn] --------------------------------------------------------------------------------------------+
|       |                                   <. A04: Không thể truy cập .............|                                      |
|       <. A05: Thông báo phù hợp ..........|                                       |                                      |
+- [Đọc thành công đúng quyền] --------------------------------------------------------------------------------------------+
|       |                                   <. A06: Dữ liệu Student ................|                                      |
|       <. A07: Hiển thị hóa đơn ...........|                                       |                                      |
| +- opt [Học viên chọn In hóa đơn] -------------------------------------------------------------------------------------+ |
| |     |- A08: Chọn In hóa đơn ------------>                                       |                                    | |
| |     |                                   |- A09: Lấy mới trước khi in ----------->                                    | |
| |     |                                   |                                       |----+  A10: Kiểm tra lại            | |
| |     |                                   |                                       |<---+  quyền/dữ liệu hiện tại       | |
| | +- alt [Lấy mới thành công đúng quyền] ----------------------------------------------------------------------------+ | |
| | |   |                                   <. A11: Dữ liệu mới để in ..............|                                  | | |
| | |   |                                   |----+  A12: Dựng bản in A4             |                                  | | |
| | |   |                                   |<---+  từ dữ liệu mới                  |                                  | | |
| | |   <. A13: Mở in trình duyệt ..........|                                       |                                  | | |
| | +- [Lấy mới lỗi hoặc không còn quyền] -----------------------------------------------------------------------------+ | |
| | |   |                                   <. A14: Không thể lấy mới ..............|                                  | | |
| | |   <. A15: Lỗi/không mở in ............|                                       |                                  | | |
| | +------------------------------------------------------------------------------------------------------------------+ | |
| +----------------------------------------------------------------------------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------------------------------+

SQ13-B — Kế toán tra cứu hóa đơn (hình riêng)

        Kế toán                             Giao diện hóa đơn F28                   Hệ thống
        |                                   |                                       |
        |- B01: Chọn lọc/tra cứu ----------->                                       |
        |                                   |- B02: Yêu cầu danh sách -------------->
        |                                   |                                       |----+  B03: Kiểm tra quyền,
        |                                   |                                       |<---+  bộ lọc/phạm vi
+- alt [Không thể tra cứu] ------------------------------------------------------------------------------------------------+
|       |                                   <. B04: Lỗi/không có quyền .............|                                      |
|       <. B05: Thông báo phù hợp ..........|                                       |                                      |
+- [Tra cứu hợp lệ] -------------------------------------------------------------------------------------------------------+
|       |                                   <. B06: Danh sách phân trang ...........|                                      |
|       <. B07: Hiển thị kết quả ...........|                                       |                                      |
| +- opt [Kế toán chọn một hóa đơn] -------------------------------------------------------------------------------------+ |
| |     |- B08: Xem chi tiết --------------->                                       |                                    | |
| |     |                                   |- B09: Yêu cầu chi tiết --------------->                                    | |
| |     |                                   |                                       |----+  B10: Kiểm tra lại            | |
| |     |                                   |                                       |<---+  quyền và đọc chi tiết        | |
| | +- alt [Đọc thành công đúng quyền] --------------------------------------------------------------------------------+ | |
| | |   |                                   <. B11: Invoice/Payment ................|                                  | | |
| | |   <. B12: Hiển thị chi tiết ..........|                                       |                                  | | |
| | +- [Không thể đọc chi tiết] ---------------------------------------------------------------------------------------+ | |
| | |   |                                   <. B13: Không thể truy cập .............|                                  | | |
| | |   <. B14: Thông báo phù hợp ..........|                                       |                                  | | |
| | +------------------------------------------------------------------------------------------------------------------+ | |
| +----------------------------------------------------------------------------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------------------------------+
```

Nhãn rút gọn trong hình chữ đối chiếu bảng thông điệp. A12 là self-message
trên Giao diện; các kiểm tra/đọc trên Hệ thống. Không bỏ lần đọc mới A09/A10
và dùng lại trực tiếp dữ liệu A06 để in.

## 7. Note và cách dựng

Note nên gắn:

1. A03/B03: **“Mọi phép đọc áp quyền hiện tại. Học viên chỉ Invoice của mình;
   Kế toán có phạm vi nghiệp vụ riêng, không dùng chung phép chiếu dữ liệu.”**
2. A10/A12: **“Lấy lại dữ liệu có quyền trước khi mở in; lỗi hoặc mất quyền
   không in dữ liệu cũ như mới.”**
3. A07/A13: **“UNPAID không phải xác nhận thu tiền; trạng thái/tiền/hạn
   lấy từ dữ liệu hiện tại, không thay đổi khi xem/in.”**
4. B12: **“Tra cứu không ghi PAID; thao tác thanh toán thuộc SQ14.
   Không quyền in/export riêng danh mục Invoice của Kế toán.”**

Tạo SQ13-A/B riêng, đặt ba lifeline tương ứng, vẽ bước đọc đầu và `alt`
đọc được/không. Nhánh Student thêm `opt` In và `alt` lấy mới; nhánh
Kế toán thêm `opt` xem chi tiết và `alt` quyền đọc chi tiết. Thêm activation,
note; không cần fragment vòng lặp cho phân trang/lọc từng lần.

## 8. Checklist

- Hóa đơn đã phát hành ở SQ12; đọc không phát hành mới hoặc tạo Payment.
- Hai actor có phạm vi dữ liệu khác nhau; Kế toán không mặc nhiên có quyền in.
- Student chỉ in của mình, luôn fetch mới trước in, không in cũ khi lỗi.
- Hiển thị đúng UNPAID/PAID/CANCELLED; không biên lai/stamp/chữ ký giả.
- Quá hạn chỉ UNPAID due < hôm nay VN; không trạng thái/phạt tự động.
- Tiền snapshot và hạn không đổi theo giá khóa hoặc ngày xem.
- Không có CSV danh mục hóa đơn/API PDF/quyền đọc điểm ngoài scope.
- Danh sách rỗng có thông báo phù hợp, không nhất thiết rơi vào lỗi.
- Lỗi/phiên hết hạn không lộ Invoice/Payment người khác.


## Đối chiếu triển khai hiện tại — 14/10/2026

In Student phải đọc mới; mất quyền hoặc fresh read lỗi thì không in bản cache như chứng từ mới. Accountant chỉ operational lookup ở hình C; không kế thừa quyền in của Student. Các POST search hiện hành vẫn là read, không mutation.

Bộ hướng dẫn này là tài liệu vẽ mức nghiệp vụ; các đoạn thể hiện cơ chế triển khai dùng note/ref theo [README](README.md). Quyền, phiên bản, cohort/membership và điều kiện thời gian được backend kiểm hiện tại; nút/preview cũ không bảo đảm ghi thành công. Cùng key/content đã hoàn tất không tạo hiệu ứng thứ hai; không tự gửi lại khi mất phản hồi. Mandatory audit có rollback theo action; login/logout security-only vẫn best-effort, không audit mọi read.

Nguồn cập nhật: [CURRENT_STATE](../../CURRENT_STATE.md), [SESSION_FOCUS_REVIEW](../../plan/SESSION_FOCUS_REVIEW.md), [E04_REPLAY_SPEC](../E04_REPLAY_SPEC.md), [E04_FINAL_CONTRACT](../E04_FINAL_CONTRACT.md) và [LocalRuntimeServices](../../../backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs). Các hướng dẫn SQ cũ và Word nguồn được giữ nguyên.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Invoice đang xem UNPAID vừa được thu tiền | Student in đọc mới và nhận trạng thái hiện tại; không in cache như dữ liệu mới. |
| Student đổi ID sang Invoice người khác | Không trả detail hoặc cho in. |
| Fresh read khi in lỗi | Không fallback in bản cũ như chứng từ đã xác minh. |
