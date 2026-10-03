# SQ03 — Hướng dẫn vẽ sequence đăng nhập và đăng xuất

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA), mức
tương tác nghiệp vụ: Actor → Boundary → Control. Bố cục và mã thông điệp là
gợi ý trình bày, không là API hoặc quyết định nghiệp vụ mới.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ03;
[FUNCTION_SPEC](FUNCTION_SPEC.md) NV01/phần chung;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W02;
[requirements](../requirements.md) ADR-048–057/097–100/115 và ADR-140.

## 1. Phạm vi và cách chia hình

SQ03 bao gồm đăng nhập, đăng xuất và quy tắc phiên. Nên vẽ **hai hình cùng
nhóm SQ03**: SQ03-A Đăng nhập và SQ03-B Đăng xuất. Hết hạn/thu hồi phiên
có thể đặt bằng note hoặc hình nhỏ SQ03-C nếu cần. Không nối đăng xuất ngay
sau đăng nhập như một bước bắt buộc; đó là các lần tương tác khác nhau.

- Đăng nhập bắt đầu tại F01 với email/mật khẩu, dùng chung bảy vai trò.
- Kết quả: phiên thường và trang đầu đúng vai trò, hoặc phiên hạn chế và
  màn hình bắt đổi mật khẩu tạm; thất bại không cấp phiên mới.
- Đăng xuất bắt đầu từ menu tài khoản trong phiên thường hoặc hạn chế;
  kết thúc khi phiên hiện tại bị thu hồi tại Hệ thống và giao diện về đăng nhập.
- Đổi mật khẩu được mô tả ở [SQ04](SQ04_PASSWORD_CHANGE_GUIDE.md).

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Người dùng | Actor; đại diện một trong bảy vai trò, chưa chọn role khi đăng nhập. |
| 2 | Giao diện tài khoản | Boundary; F01 và menu đăng xuất trong khung chung. |
| 3 | Hệ thống quản lý trung tâm | Control; xác thực, xác định role, tạo/thu hồi phiên và trả kết quả. |

Thêm message liền cho yêu cầu, message nét đứt cho phản hồi, self-message
trên Hệ thống, activation và Combined Fragment `alt`. Có thể dùng `ref SQ04`
để chỉ luồng đổi mật khẩu; không cần vẽ lại SQ04 bên trong SQ03.

Không cần Database, JWT, Cookie, Controller/Service làm lifeline. Không thêm
Admin vào đăng nhập/đăng xuất; Admin chỉ tham gia cấp/reset trong sequence khác.

## 3. Các điều kiện nghiệp vụ cần thể hiện

| Điều kiện | Quy tắc đã chốt |
| --- | --- |
| Thông tin đăng nhập | Email + mật khẩu; không đăng nhập SĐT, không chọn role. |
| Xác thực | Account ACTIVE, credential đúng, không đang bị chặn đăng nhập; mật khẩu tạm còn hạn nếu dùng. |
| Sai mật khẩu | Năm lần sai liên tiếp trong cửa sổ 15 phút chặn đăng nhập mới 15 phút; không chuyển Account sang LOCKED hoặc thu hồi phiên đang tồn tại. Không gán mọi lỗi input/status thành một lần sai mật khẩu. |
| Giới hạn request | Giới hạn login theo IP 100/phút, tách khỏi chặn theo tài khoản; có thể ghi note thay vì diễn giải hạ tầng. |
| Lỗi trước xác thực | Thông báo chung, không tiết lộ email tồn tại hoặc tài khoản LOCKED/DISABLED; guard nội bộ có thể nêu nguyên nhân nhưng message người dùng không được tiết lộ. |
| Phiên thường | Tối đa hai giờ tuyệt đối hoặc 30 phút không hoạt động, tùy mốc đến trước; không refresh/remember-me. |
| Phiên hạn chế | Tối đa 10 phút và không vượt hạn mật khẩu tạm 24 giờ từ lúc cấp/reset; chỉ đổi mật khẩu tạm hoặc đăng xuất. |
| Nhiều phiên | Đăng nhập mới không đẩy phiên khác ra; tab cùng browser profile dùng chung phiên, device/profile khác có phiên độc lập. |
| Nhật ký | Đăng nhập/đăng xuất thuần bảo mật ghi best-effort; lỗi ghi log không bỏ kiểm tra xác thực hoặc hồi sinh phiên đã thu hồi. Không log mật khẩu hoặc token. |

## 4. SQ03-A — Thứ tự thông điệp đăng nhập

| Mã | Từ → Đến | Nhãn thông điệp | Điều kiện/vị trí |
| --- | --- | --- | --- |
| A01 | Người dùng → Giao diện | Mở trang đăng nhập | Bắt đầu. |
| A02 | Giao diện → Người dùng | Hiển thị biểu mẫu đăng nhập | Email/mật khẩu. |
| A03 | Người dùng → Giao diện | Nhập email, mật khẩu và chọn “Đăng nhập” | Không gửi role do người dùng chọn. |
| A04 | Giao diện → Hệ thống | Gửi yêu cầu đăng nhập | Bắt đầu activation xử lý. |
| A05 | Hệ thống → Hệ thống | Kiểm tra điều kiện truy cập và thông tin đăng nhập | Bao gồm chặn, Account và hạn mật khẩu tạm. |
| A06 | Hệ thống → Hệ thống | Cập nhật kiểm soát đăng nhập và ghi sự kiện bảo mật | `alt` ngoài: `[Không đáp ứng điều kiện đăng nhập]`; chỉ tăng đếm sai mật khẩu khi đúng loại sự kiện. |
| A07 | Hệ thống → Giao diện | Trả kết quả đăng nhập không thành công hoặc yêu cầu chờ thử lại | Lỗi không tiết lộ tồn tại/trạng thái Account. |
| A08 | Giao diện → Người dùng | Hiển thị thông báo đăng nhập | Kết thúc nhánh thất bại. |
| A09 | Hệ thống → Hệ thống | Xác định vai trò và loại phiên; đặt lại đếm sai sau đăng nhập thành công | `alt` ngoài: `[Đáp ứng điều kiện đăng nhập]`. |
| A10 | Hệ thống → Hệ thống | Tạo phiên thường và ghi sự kiện đăng nhập | `alt` trong: `[Không bắt buộc đổi mật khẩu tạm]`. |
| A11 | Hệ thống → Giao diện | Trả kết quả đăng nhập với phiên thường và vai trò | Không vẽ secret/token trong kết quả nghiệp vụ. |
| A12 | Giao diện → Người dùng | Hiển thị trang đầu của vai trò | Kết thúc thành công thường. |
| A13 | Hệ thống → Hệ thống | Tạo phiên hạn chế và ghi sự kiện đăng nhập | `alt` trong: `[Bắt buộc đổi mật khẩu tạm]`; credential tạm đã được kiểm tra còn hạn ở A05. |
| A14 | Hệ thống → Giao diện | Trả kết quả cần đổi mật khẩu tạm | Không cấp quyền nghiệp vụ. |
| A15 | Giao diện → Người dùng | Hiển thị màn hình bắt đổi mật khẩu | Gắn `ref SQ04`; không tự nâng thành phiên thường. |

Nếu không thể tạo phiên do lỗi xử lý, không trả thành công/hiển thị trang nghiệp
vụ. Có thể ghi note ngoại lệ này thay vì thêm nhánh lỗi hạ tầng vào hình chính.

## 5. SQ03-B — Thứ tự thông điệp đăng xuất

| Mã | Từ → Đến | Nhãn thông điệp | Ý nghĩa |
| --- | --- | --- | --- |
| B01 | Người dùng → Giao diện | Chọn “Đăng xuất” | Phiên thường hoặc hạn chế. |
| B02 | Giao diện → Hệ thống | Yêu cầu đăng xuất phiên hiện tại | Không là yêu cầu đăng xuất tất cả. |
| B03 | Hệ thống → Hệ thống | Thu hồi phiên hiện tại | Token chưa hết hạn cũng không còn dùng được. |
| B04 | Hệ thống → Hệ thống | Ghi sự kiện đăng xuất | Best-effort; lỗi log không hoàn tác thu hồi. |
| B05 | Hệ thống → Giao diện | Trả kết quả đã đăng xuất | Sau khi phiên đã bị thu hồi. |
| B06 | Giao diện → Người dùng | Xóa trạng thái phiên cục bộ và hiển thị trang đăng nhập | Chỉ xóa UI không thay việc thu hồi tại Hệ thống. |

Tab cùng profile mất hiệu lực phiên chung; không cần vẽ thông báo tự động
đến từng tab. Các phiên khác trên device/profile khác vẫn hợp lệ theo hạn.
Nếu chưa biết yêu cầu đăng xuất đã được xử lý hay chưa do mất phản hồi,
không dùng một nhánh “thành công” giả chỉ vì đã chuyển trang cục bộ.

## 6. Phác thảo bố cục

```text
SQ03-A — Đăng nhập

        Người dùng                          Giao diện tài khoản                   Hệ thống
        |                                   |                                     |
        |- A01: Mở đăng nhập --------------->                                     |
        <. A02: Hiển thị biểu mẫu ..........|                                     |
        |- A03: Nhập và gửi ---------------->                                     |
        |                                   |- A04: Yêu cầu đăng nhập ------------>
        |                                   |                                     |----+  A05: Kiểm tra truy cập
        |                                   |                                     |<---+  và thông tin đăng nhập
+- alt [Không đáp ứng điều kiện đăng nhập] ------------------------------------------------------------------------------+
|       |                                   |                                     |----+  A06: Kiểm soát login           |
|       |                                   |                                     |<---+  và ghi sự kiện bảo mật         |
|       |                                   <. A07: Không thành công/chờ .........|                                      |
|       <. A08: Thông báo chung ............|                                     |                                      |
+- [Đáp ứng điều kiện đăng nhập] ----------------------------------------------------------------------------------------+
|       |                                   |                                     |----+  A09: Role/loại phiên           |
|       |                                   |                                     |<---+  đặt lại đếm sai                |
  +- alt [Không bắt buộc đổi mật khẩu tạm] ----------------------------------------------------------------------------+
| |     |                                   |                                     |----+  A10: Tạo phiên thường        | |
| |     |                                   |                                     |<---+  và ghi sự kiện               | |
| |     |                                   <. A11: Thành công/role ..............|                                    | |
| |     <. A12: Trang đầu role .............|                                     |                                    | |
  +- [Bắt buộc đổi mật khẩu tạm] --------------------------------------------------------------------------------------+
| |     |                                   |                                     |----+  A13: Tạo phiên hạn chế       | |
| |     |                                   |                                     |<---+  và ghi sự kiện               | |
| |     |                                   <. A14: Cần đổi mật khẩu .............|                                    | |
| |     <. A15: Màn hình bắt đổi ...........|                                     |                                    | |
| |     |   ref SQ04 — khi người dùng thực hiện đổi mật khẩu tạm                  |                                    | |
  +--------------------------------------------------------------------------------------------------------------------+
+------------------------------------------------------------------------------------------------------------------------+

SQ03-B — Đăng xuất (hình riêng)

        Người dùng                          Giao diện tài khoản                   Hệ thống
        |                                   |                                     |
        |- B01: Chọn đăng xuất ------------->                                     |
        |                                   |- B02: Yêu cầu đăng xuất ------------>
        |                                   |                                     |----+  B03: Thu hồi phiên
        |                                   |                                     |<---+  hiện tại
        |                                   |                                     |----+  B04: Ghi sự kiện
        |                                   |                                     |<---+  best-effort
        |                                   <. B05: Đã đăng xuất .................|
        <. B06: Xóa state/về F01 ...........|                                     |
```

Các mã là chỉ dẫn đặt message, không một chuỗi mà mọi nhánh cùng chạy.
Đặt self-message trên lifeline Hệ thống; các phản hồi đi qua Giao diện.

## 7. Phiên hết hạn/thu hồi — note hoặc SQ03-C

Phiên không tự hết hạn bằng một message người dùng “đăng xuất”. Khi người
dùng tiếp tục yêu cầu nghiệp vụ, Hệ thống kiểm tra phiên hiện tại. Nếu phiên
đã hết hạn/bị thu hồi: không thực hiện nghiệp vụ, trả kết quả cần đăng nhập
lại; Giao diện đưa về F01. Không thêm lifeline đồng hồ/cron bắt buộc.

Chỉ request nghiệp vụ được xác thực do người dùng chủ động thao tác mới
gia hạn thời điểm hoạt động; gõ phím, di chuột, polling/timer không giữ phiên.
Không gia hạn vượt mốc tuyệt đối hai giờ.

## 8. Note, cách dựng và kiểm tra

Note nên gắn:

1. Tại A05: **“Hệ thống xác định role; lỗi trước xác thực không tiết lộ
   email tồn tại hoặc trạng thái Account.”**
2. Tại A10/A13: **“Phiên thường: tối đa 2 giờ/idle 30 phút. Phiên hạn chế:
   tối đa 10 phút, không vượt hạn mật khẩu tạm; chỉ đổi mật khẩu/logout.”**
3. Tại B03: **“Đăng xuất thu hồi phiên hiện tại; tab cùng profile dùng chung
   phiên, device/profile khác không bị thu hồi bởi thao tác này.”**
4. Tại sự kiện bảo mật: **“Ghi best-effort, không ghi mật khẩu/token;
   lỗi log không bỏ xác thực hoặc phục hồi phiên đã thu hồi.”**

Dựng ba lifeline, vẽ thông điệp đầu, đặt `alt` ngoài rồi `alt` trong theo
phác thảo, thêm activation và note. Dựng SQ03-B trên hình riêng cùng nhóm.
Không cần thêm `loop` đăng nhập lại: mỗi lần bấm là một lần xử lý mới.

Kiểm tra trước khi hoàn tất:

- Lỗi không cấp phiên mới hoặc dẫn tới trang nghiệp vụ.
- Mật khẩu tạm hết hạn không được cấp phiên hạn chế.
- Phiên hạn chế chỉ dẫn tới SQ04 hoặc đăng xuất.
- Đăng nhập mới không thu hồi phiên khác; đăng xuất không thu hồi tất cả.
- Chặn tạm khác LOCKED và không làm mất phiên hiện có.
- Chỉ có một actor đại diện cho lần tương tác; không xếp bảy role nối tiếp.
- Không OTP, email recovery, CAPTCHA, remember-me hoặc giao diện quản lý phiên.
- Không tự gửi lại credential khi mất phản hồi; kiểm tra trạng thái phiên
  trước khi quyết định thao tác tiếp, không đoán kết quả.
