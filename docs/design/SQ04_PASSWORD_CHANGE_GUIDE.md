# SQ04 — Hướng dẫn vẽ sequence tự đổi mật khẩu

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA), mức
tương tác nghiệp vụ Actor → Boundary → Control. Mã thông điệp/bố cục chỉ
là gợi ý trình bày, không là API hoặc quyết định sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ04;
[FUNCTION_SPEC](FUNCTION_SPEC.md) phần chung/NV01;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W02;
[requirements](../requirements.md) ADR-020/048–049/053/098/114–115/140.

## 1. Mục tiêu, hai điểm vào và kết quả

**Mục tiêu:** chủ tài khoản tự đổi mật khẩu bằng mật khẩu hiện tại, gồm:

1. Người dùng có phiên thường, chủ động mở “Đổi mật khẩu” từ menu tài khoản.
2. Người dùng đăng nhập bằng mật khẩu tạm còn hạn, có phiên hạn chế và được
   đưa tới màn hình bắt đổi mật khẩu từ [SQ03](SQ03_LOGIN_LOGOUT_GUIDE.md).

Có thể vẽ chung một hình SQ04 vì phần kiểm tra/lưu mật khẩu giống nhau;
ghi hai điểm vào bằng note. Không yêu cầu người đang ở màn hình bắt đổi phải
truy cập menu nghiệp vụ trước. Nếu cần hai hình, dùng SQ04-A/SQ04-B và giữ
chung quy tắc dưới đây.

**Điều kiện trước:** Account ACTIVE và phiên thường/hạn chế còn hợp lệ; người
thực hiện là chủ tài khoản. Phiên hạn chế phải còn trong hạn mật khẩu tạm.

**Thành công:** mật khẩu mới được lưu an toàn, bỏ yêu cầu đổi mật khẩu tạm
nếu có, thu hồi mọi phiên của tài khoản (kể cả hiện tại) và ghi audit bắt buộc.
Giao diện về đăng nhập; phải đăng nhập mới, không tự nâng phiên hạn chế.

**Không thành công:** không đổi mật khẩu hoặc thu hồi một phần phiên do
thao tác này. Phiên bị hết hạn/thu hồi từ nguyên nhân khác vẫn không hợp lệ;
không phục hồi phiên để người dùng tiếp tục sửa form.

Không phải quên mật khẩu, không đổi mật khẩu người khác. Admin reset nằm ở
SQ05; SQ04 không có Admin tham gia.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Chủ tài khoản | Actor; bất kỳ vai trò nào tự đổi mật khẩu của mình. |
| 2 | Giao diện đổi mật khẩu | Boundary; menu tài khoản hoặc màn hình bắt đổi mật khẩu tạm. |
| 3 | Hệ thống quản lý trung tâm | Control; xác thực phiên/mật khẩu hiện tại, kiểm tra mật khẩu mới, thực hiện thay đổi. |

Thêm message yêu cầu, return message, self-message, activation, `alt` ngoài
cho kiểm tra phiên và `alt` trong cho credential/mật khẩu mới. Một `alt`
cuối trong nhánh hợp lệ thể hiện lưu thành công hoặc lỗi phải hoàn tác.

Không cần lifeline cho từng thiết bị/phiên, Database, bộ hash hoặc API.
Thu hồi mọi phiên là một self-message nghiệp vụ trên Hệ thống.

## 3. Dữ liệu và kiểm tra

| Nội dung | Quy tắc |
| --- | --- |
| Dữ liệu gửi | Mật khẩu hiện tại và mật khẩu mới; không gửi ID tài khoản khác để đổi thay. |
| Mật khẩu hiện tại | Phải đúng; trong phiên hạn chế là mật khẩu tạm đã dùng đăng nhập, vẫn phải nhập lại. |
| Mật khẩu mới | 15–128 Unicode code point, cho khoảng trắng; không trim/NFC, không cắt; không ép tổ hợp chữ hoa/số/ký tự đặc biệt. Chặn mật khẩu phổ biến/đã lộ theo chính sách đã duyệt. |
| Phiên/Account | Kiểm tra tại lúc xử lý và trước khi hoàn tất; hết hạn/reset/khóa đồng thời không được bypass. |
| Tính nguyên tử | Mật khẩu, trạng thái bắt đổi, thu hồi mọi phiên và audit bắt buộc cùng thành công hoặc cùng hoàn tác. |
| Bảo mật thông tin | Không ghi mật khẩu cũ/mới vào log, note ví dụ hoặc kết quả; không lưu/replay credential. |

Nếu giao diện có ô “Nhập lại mật khẩu mới”, có thể kiểm tra khớp tại Giao
diện. Hướng dẫn này không chốt thêm ô đó thành yêu cầu mới; sơ đồ chính dùng
hai dữ liệu nghiệp vụ đã được yêu cầu. Không tự thêm quy tắc mật khẩu mới
phải khác cũ hoặc lịch sử cấm tái sử dụng khi chưa có nguồn được duyệt.

## 4. Thứ tự thông điệp

| Mã | Từ → Đến | Nhãn thông điệp | Vị trí/ý nghĩa |
| --- | --- | --- | --- |
| M01 | Chủ tài khoản → Giao diện | Mở chức năng đổi mật khẩu | Điểm vào phiên thường; phiên hạn chế vào sẵn màn hình từ SQ03, bỏ M01. |
| M02 | Giao diện → Chủ tài khoản | Hiển thị biểu mẫu đổi mật khẩu | Chung cho hai điểm vào. |
| M03 | Chủ tài khoản → Giao diện | Nhập mật khẩu hiện tại, mật khẩu mới và chọn “Lưu” | Không autosave. |
| M04 | Giao diện → Hệ thống | Gửi yêu cầu tự đổi mật khẩu | Đổi tài khoản của phiên hiện tại. |
| M05 | Hệ thống → Hệ thống | Kiểm tra phiên và điều kiện truy cập của chủ tài khoản | `alt` ngoài sau bước này. |
| M06 | Hệ thống → Giao diện | Trả kết quả cần đăng nhập lại | `[Phiên hoặc điều kiện truy cập không còn hợp lệ]`; không thực hiện đổi. |
| M07 | Giao diện → Chủ tài khoản | Thông báo cần đăng nhập lại và hiển thị F01 | Kết thúc nhánh phiên không hợp lệ. |
| M08 | Hệ thống → Hệ thống | Kiểm tra mật khẩu hiện tại và chính sách mật khẩu mới | `[Phiên và điều kiện truy cập hợp lệ]`; đặt `alt` trong sau kiểm tra. |
| M09 | Hệ thống → Giao diện | Trả lỗi mật khẩu hiện tại hoặc mật khẩu mới | `[Mật khẩu hiện tại sai hoặc mật khẩu mới không hợp lệ]`. |
| M10 | Giao diện → Chủ tài khoản | Hiển thị lỗi cần sửa | Không thay credential/thu hồi phiên bởi yêu cầu thất bại. |
| M11 | Hệ thống → Hệ thống | Lưu mật khẩu mới, bỏ yêu cầu đổi tạm nếu có, thu hồi mọi phiên và ghi audit | `[Mật khẩu hiện tại đúng và mật khẩu mới hợp lệ]`; đây là nhóm xử lý nguyên tử, không thông báo thành công giữa chừng. |
| M12 | Hệ thống → Giao diện | Trả kết quả đổi mật khẩu thành công; yêu cầu đăng nhập mới | `alt` kết quả: `[Hoàn tất toàn bộ thay đổi]`. |
| M13 | Giao diện → Chủ tài khoản | Thông báo thành công và hiển thị trang đăng nhập | Không truy cập menu nghiệp vụ bằng phiên cũ. |
| M14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ thay đổi của yêu cầu | `alt` kết quả: `[Không thể hoàn tất hoặc điều kiện thay đổi đồng thời]`. |
| M15 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất thay đổi | Gồm audit bắt buộc lỗi; không báo thành công giả. |
| M16 | Giao diện → Chủ tài khoản | Thông báo không thể hoàn tất; yêu cầu đăng nhập lại nếu phiên không còn hợp lệ | Không mặc định mọi lỗi đều giữ quyền sửa form. |

M01–M16 là mã chỉ dẫn; không phải toàn bộ thông điệp đều chạy nối tiếp.
Nếu hình quá sâu, đặt kiểm tra phiên thành note tiền điều kiện và vẽ nhánh
phiên mất hiệu lực thành hình phụ; không bỏ điều kiện thực thi của Hệ thống.

## 5. Phác thảo bố cục bằng văn bản

```text
SQ04 — Tự đổi mật khẩu

        Chủ tài khoản                       Giao diện đổi mật khẩu                Hệ thống
        |                                   |                                     |
        |   Điểm vào A: phiên thường mở từ menu (M01).                            |
        |   Điểm vào B: phiên hạn chế tới từ SQ03, bỏ M01.                        |
        |- M01: Mở đổi mật khẩu ------------>                                     |
        <. M02: Hiển thị biểu mẫu ..........|                                     |
        |- M03: Nhập và chọn Lưu ----------->                                     |
        |                                   |- M04: Yêu cầu đổi mật khẩu --------->
        |                                   |                                     |----+  M05: Kiểm tra phiên
        |                                   |                                     |<---+  và điều kiện truy cập
+- alt [Phiên hoặc điều kiện truy cập không còn hợp lệ] -----------------------------------------------------------------+
|       |                                   <. M06: Cần đăng nhập lại ............|                                      |
|       <. M07: Thông báo/về F01 ...........|                                     |                                      |
+- [Phiên và điều kiện truy cập hợp lệ] ---------------------------------------------------------------------------------+
|       |                                   |                                     |----+  M08: Kiểm tra mật khẩu         |
|       |                                   |                                     |<---+  hiện tại và mới                |
  +- alt [Mật khẩu hiện tại sai hoặc mật khẩu mới không hợp lệ] -------------------------------------------------------+
| |     |                                   <. M09: Trả lỗi mật khẩu .............|                                    | |
| |     <. M10: Hiển thị lỗi ...............|                                     |                                    | |
  +- [Mật khẩu hiện tại đúng và mật khẩu mới hợp lệ] ------------------------------------------------------------------+
| |     |                                   |                                     |----+  M11: Đổi mật khẩu,           | |
| |     |                                   |                                     |<---+  bỏ yêu cầu đổi tạm nếu có    | |
| |     |   M11 cùng thao tác: thu hồi MỌI phiên và ghi audit bắt buộc.           |                                    | |
    +- alt [Hoàn tất toàn bộ thay đổi] ------------------------------------------------------------------------------+
| | |   |                                   <. M12: Đổi thành công ...............|                                  | | |
| | |   <. M13: Thành công/về F01 ..........|                                     |                                  | | |
    +- [Không thể hoàn tất hoặc điều kiện thay đổi đồng thời] -------------------------------------------------------+
| | |   |                                   |                                     |----+  M14: Hoàn tác thay đổi     | | |
| | |   |                                   |                                     |<---+  của yêu cầu                | | |
| | |   |                                   <. M15: Không thể hoàn tất ...........|                                  | | |
| | |   <. M16: Thông báo lỗi ..............|                                     |                                  | | |
| | |   |   M16: đăng nhập lại nếu phiên hiện tại không còn hợp lệ.               |                                  | | |
    +----------------------------------------------------------------------------------------------------------------+
  +--------------------------------------------------------------------------------------------------------------------+
+------------------------------------------------------------------------------------------------------------------------+
```

Các bước kiểm tra/lưu/hoàn tác là self-message trên Hệ thống. Mọi kết quả
đi Hệ thống → Giao diện → Chủ tài khoản. Lần đăng nhập mới tham chiếu SQ03,
không vẽ một message tự động “đăng nhập bằng mật khẩu mới”.

## 6. Note nên gắn

1. Gần M02: **“Hai điểm vào: tự đổi trong phiên thường hoặc bắt đổi trong
   phiên hạn chế. Cả hai phải nhập đúng mật khẩu hiện tại.”**
2. Gần M11: **“Mật khẩu mới, trạng thái bắt đổi, thu hồi mọi phiên và audit
   bắt buộc cùng thành công hoặc cùng hoàn tác; audit lỗi không đổi một phần.”**
3. Gần M13: **“Mọi phiên cũ, kể cả phiên hiện tại và trên thiết bị khác,
   đều bị thu hồi. Phải đăng nhập mới; không nâng phiên hạn chế tại chỗ.”**
4. Gần M04: **“Không ghi log/lưu/replay mật khẩu. Mất phản hồi không tự
   gửi lại yêu cầu hoặc kết luận chắc chắn rằng thay đổi đã thất bại.”**

Sau mất phản hồi, xác minh trạng thái phiên theo quyền hiện có; nếu cần thì
thực hiện đăng nhập mới bằng thao tác có chủ đích. Không tự tạo endpoint
tra cứu mật khẩu, reset tự động hoặc replay credential để lấy kết quả.

## 7. Cách dựng và checklist

1. Tạo hình **SQ04 — Tự đổi mật khẩu**, đặt ba lifeline như phần 2.
2. Gắn note hai điểm vào; vẽ M01 cho phiên thường, M02–M05 dùng chung.
3. Đặt `alt` ngoài, trong nhánh phiên hợp lệ thêm `alt` kiểm tra mật khẩu.
4. Trong nhánh mật khẩu hợp lệ vẽ M11 và `alt` kết quả; đặt phản hồi đúng nhánh.
5. Thêm activation, note tính nguyên tử và `ref SQ03` cho lần đăng nhập tiếp.

Kiểm tra:

- Không cho đổi khi phiên/Account không hợp lệ hoặc mật khẩu hiện tại sai.
- Mật khẩu tạm không được coi là phiên thường hay bỏ bước nhập hiện tại.
- Thành công thu hồi tất cả phiên, không chỉ phiên hiện tại như đăng xuất.
- Audit tại SQ04 là bắt buộc; không áp quy tắc best-effort của login/logout.
- Lỗi không lưu một phần mật khẩu/trạng thái/thu hồi/audit.
- Không giữ hoặc phục hồi phiên đã mất hiệu lực do reset/khóa đồng thời.
- Không thêm Admin, OTP, email recovery, tự đăng nhập hoặc mật khẩu ví dụ thật.
- Không coi “đã đổi thành công” và “vẫn dùng phiên cũ để làm nghiệp vụ” là cùng kết quả.
