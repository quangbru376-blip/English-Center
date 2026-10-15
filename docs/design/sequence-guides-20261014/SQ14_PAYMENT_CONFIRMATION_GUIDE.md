# SQ14 — Hướng dẫn vẽ sequence Kế toán xác nhận thu tiền và sửa metadata

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV28; W12; F29; UC04.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: FinanceService.cs](../../../backend/src/EnglishCenter.Api/Services/Finance/FinanceService.cs).
- [Code: CanonicalSchemas.cs](../../../backend/src/EnglishCenter.Api/Common/Replay/CanonicalSchemas.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Kế toán ghi nhận khoản tiền đã nhận ngoài ứng dụng cho Invoice đã phát hành, hoặc sửa method/reference của Payment hiện có với lý do. Hai action có guard và hiệu ứng khác nhau.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ14-A — Xác nhận đã thu đủ tiền**; actor Kế toán.
- **SQ14-B — Sửa phương thức/mã giao dịch**; actor Kế toán.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không gateway, trả góp, refund, số tiền âm, ngày thu do người dùng chọn hoặc biên lai riêng mới.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Kế toán | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện xác nhận thanh toán | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Thu tiền | Invoice UNPAID, Registration hợp lệ; số tiền đúng toàn bộ amount snapshot, VND nguyên đồng, chỉ một Payment/Invoice. |
| Phương thức | Mã code hiện hành CASH hoặc TRANSFER (không BANK_TRANSFER); TRANSFER cần transactionReference. Trên hình có thể ghi Tiền mặt/Chuyển khoản. |
| Ngày/người | confirmedBy và confirmedAt từ phiên/đồng hồ server; không cho người dùng chọn ngày doanh thu quá khứ. |
| Sửa metadata | Chỉ method/reference, reason bắt buộc; không đổi amount/paid_at/Invoice link. All-equal normalized metadata là no-change, reason khác chưa đủ tạo delta. |
| Nguyên tử | Tạo Payment + Invoice PAID + mandatory audit + hoàn tất thao tác; sửa metadata + old/new/reason audit là giao dịch riêng. |
| Race | Invoice/Registration/version được kiểm lại khi lưu; cancellation và payment không đồng thời thắng. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Xác nhận đã thu đủ tiền

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Kế toán → Giao diện xác nhận thanh toán | Mở chức năng Xác nhận đã thu đủ tiền | Điểm bắt đầu. |
| A02 | Giao diện xác nhận thanh toán → Hệ thống | Đọc Invoice, Registration, amount snapshot và khả năng xác nhận | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện xác nhận thanh toán | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Kế toán → Giao diện xác nhận thanh toán | Nhập phương thức/reference/số tiền và xác nhận đã thu đủ | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện xác nhận thanh toán → Hệ thống | Gửi yêu cầu xác nhận thu đủ tiền | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm Invoice UNPAID/chưa Payment, hồ sơ hợp lệ, amount đúng, method/reference và phiên bản | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện xác nhận thanh toán | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Amount khác snapshot, thiếu reference, đã PAID/hủy, dữ liệu cũ hoặc Payment trùng]`; không thực hiện hiệu ứng. |
| A10 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Tạo Payment với confirmer/time server; đổi Invoice PAID; audit và dấu hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện xác nhận thanh toán | Payment đã ghi và Invoice PAID | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện xác nhận thanh toán | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Kế toán; G = Giao diện xác nhận thanh toán; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Xác nhận đã thu đủ tiền
                            |-- A02 --> H : Đọc Invoice, Registration, amount snapshot và khả năng xác nhận
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Nhập phương thức/reference/số tiền và xác nhận đã thu đủ
                            |-- A07 --> H : Gửi yêu cầu xác nhận thu đủ tiền
                                                       H --+ A08: Kiểm Invoice UNPAID/chưa Payment, hồ sơ hợp lệ, amount đúng,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Tạo Payment với confirmer/time server; đổi Invoice PAID; audit và dấu…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Payment đã ghi và Invoice PAID
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

### B. Sửa phương thức/mã giao dịch

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Kế toán → Giao diện xác nhận thanh toán | Mở chức năng Sửa phương thức/mã giao dịch | Điểm bắt đầu. |
| B02 | Giao diện xác nhận thanh toán → Hệ thống | Đọc Payment và Invoice với method/reference hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện xác nhận thanh toán | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Kế toán → Giao diện xác nhận thanh toán | Sửa method/reference, nhập lý do và xác nhận lưu | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện xác nhận thanh toán → Hệ thống | Gửi yêu cầu sửa method/reference với lý do | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm quyền, Payment/Invoice versions, metadata hợp lệ, reason và thay đổi thực | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện xác nhận thanh toán | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Reason thiếu, reference sai/thiếu, phiên bản cũ hoặc metadata không đổi]`; không thực hiện hiệu ứng. |
| B10 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Chỉ cập nhật method/reference, giữ amount/time/link; audit old/new/reason và dấu hoàn tất nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện xác nhận thanh toán | Payment với metadata mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện xác nhận thanh toán | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện xác nhận thanh toán → Kế toán | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Kế toán; G = Giao diện xác nhận thanh toán; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Sửa phương thức/mã giao dịch
                            |-- B02 --> H : Đọc Payment và Invoice với method/reference hiện tại
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Sửa method/reference, nhập lý do và xác nhận lưu
                            |-- B07 --> H : Gửi yêu cầu sửa method/reference với lý do
                                                       H --+ B08: Kiểm quyền, Payment/Invoice versions, metadata hợp lệ, reason và thay…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Chỉ cập nhật method/reference, giữ amount/time/link; audit…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Payment với metadata mới
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

## 5. Các note nên gắn nguyên văn

1. **“Thu tiền diễn ra bên ngoài ứng dụng. Không vẽ gọi ngân hàng/cổng thanh toán rồi tự nhận callback.”**
2. **“Metadata correction không đổi doanh thu theo ngày xác nhận; không cần tính lại amount hoặc phát hành Invoice.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Chỉ CASH/TRANSFER theo code.
- Payment tạo một lần và PAID cùng commit.
- Không làm mới confirmedAt khi sửa metadata.
- Race hủy/thu tiền không cho PAID Invoice CANCELLED.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Invoice 2.000.000 VND, nhập 1.000.000 | Không partial payment; không tạo Payment hoặc PAID. |
| TRANSFER thiếu reference | Lỗi validation, không ghi. |
| Sửa reference sau khi PAID | Giữ amount/confirmedAt/link; doanh thu theo ngày xác nhận cũ. |
