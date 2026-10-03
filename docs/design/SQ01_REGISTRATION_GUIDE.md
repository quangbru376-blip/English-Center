# SQ01 — Hướng dẫn vẽ sequence tự đăng ký tài khoản học viên

Ngày: 02/10/2026. Phạm vi: hướng dẫn vẽ thủ công trong Enterprise Architect
(EA), ở mức tương tác nghiệp vụ với giao diện và hệ thống. Cách bố trí và
đánh số thông điệp dưới đây là gợi ý trình bày; không tạo quyết định sản phẩm.

Nguồn đối chiếu:

- [Quy hoạch sequence](SEQUENCE_PLAN.md): SQ01.
- [E01 — FUNCTION_SPEC](FUNCTION_SPEC.md): NV01 và validation phần 1.2.
- [E02 — WORKFLOW_SPEC](WORKFLOW_SPEC.md): phần Student tự đăng ký trong W01.
- [Requirements](../requirements.md): tài khoản và ADR-142, đăng ký nguyên tử,
  không auto-login, không tự thử lại khi kết quả không rõ.

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

**Mục tiêu:** người chưa đăng nhập tự tạo tài khoản Student và hồ sơ học viên
của mình qua màn hình F02.

**Bắt đầu:** người đăng ký mở chức năng đăng ký tài khoản.

**Thành công:** tài khoản Student, hồ sơ học viên và nhật ký bắt buộc được
tạo đầy đủ; giao diện thông báo thành công. Người dùng chưa được tự đăng nhập.

**Không thành công:** dữ liệu sai, email/SĐT trùng hoặc không thể hoàn tất
việc tạo dữ liệu; không để lại tài khoản/hồ sơ được tạo một phần.

SQ01 chỉ bao phủ phần tự đăng ký trong NV01/W01. Đăng nhập thuộc SQ03;
cấp tài khoản nhân sự thuộc SQ02; đăng ký khóa học thuộc SQ07.

## 2. Các thành phần cần đặt trong EA

Đặt ba lifeline từ trái sang phải:

| Thứ tự | Tên hiển thị | Loại/vai trò | Trách nhiệm |
| --- | --- | --- | --- |
| 1 | Người đăng ký | Actor | Mở chức năng, nhập thông tin, gửi đăng ký, nhận kết quả. |
| 2 | Giao diện đăng ký tài khoản | Boundary | Hiển thị F02, nhận thao tác, gửi yêu cầu và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control | Kiểm tra thông tin, tạo tài khoản/hồ sơ, ghi nhật ký và trả kết quả. |

Các thành phần bổ sung trên sơ đồ:

| Thành phần | Cần dùng thế nào |
| --- | --- |
| Message | Mũi tên liền cho thao tác/yêu cầu; tên bằng ngôn ngữ nghiệp vụ. |
| Return message | Mũi tên nét đứt cho kết quả xử lý; phản hồi cho actor có thể ghi “Hiển thị …”. |
| Self-message | Trên Hệ thống để mô tả kiểm tra, tạo dữ liệu và hoàn tác. |
| Activation | Thanh xử lý trên Giao diện khi gửi yêu cầu và trên Hệ thống khi xử lý; kết thúc tại phản hồi của nhánh tương ứng. |
| Combined Fragment `alt` ngoài | Phân biệt kiểm tra không đạt và kiểm tra đạt. |
| Combined Fragment `alt` bên trong | Phân biệt tạo thành công, trùng phát sinh đồng thời và lỗi xử lý. |
| Guard | Đặt trong `[...]` ở đầu mỗi operand, dùng nguyên điều kiện ở phần 4. |
| Note | Ghi dữ liệu đầu vào, tính nguyên tử và không tự đăng nhập. |

Không cần lifeline Database, Account, StudentProfile, Controller hoặc Service
ở mức này. Account/StudentProfile được nhắc trong nội dung xử lý và note.
Không cần `opt`/`loop` cho một lần gửi đăng ký. Người dùng sửa rồi gửi lại là
một lần xử lý mới, không cần bao toàn sơ đồ trong vòng lặp vô hạn.

## 3. Thông tin gửi đăng ký và kiểm tra nghiệp vụ

**Dữ liệu gửi:** họ tên, ngày sinh, email, số điện thoại, mật khẩu; địa chỉ tùy chọn.
Role Student do hệ thống xác định, không cho người đăng ký chọn role nhân sự.

| Trường/quy tắc | Kiểm tra cần hiểu; không nhất thiết viết hết lên mũi tên |
| --- | --- |
| Họ tên | Bắt buộc, trim, 1–100 Unicode code point. |
| Ngày sinh | Ngày hợp lệ, không sau ngày Việt Nam hiện tại; không tự thêm điều kiện tuổi. |
| Email | Bắt buộc, đúng định dạng theo E01, tối đa 254; không trùng Account, không phân biệt hoa-thường. |
| SĐT | Bắt buộc, số di động VN theo E01; chuẩn hóa trước so trùng toàn Account. |
| Mật khẩu | 15–128 code point, cho Unicode/khoảng trắng; không trim; chặn mật khẩu phổ biến/đã lộ theo chính sách đã duyệt. |
| Địa chỉ | Tùy chọn, trim, tối đa 255; trống tương đương không cung cấp. |
| Tạo dữ liệu | Tài khoản Student + hồ sơ học viên + nhật ký bắt buộc cùng thành công hoặc cùng hoàn tác. |

Đặt kiểm tra có thẩm quyền trên Hệ thống. Nếu giao diện kiểm tra giúp người
dùng, điều đó không thay kiểm tra của Hệ thống. Không ghi mật khẩu vào note
ví dụ, nhật ký hoặc thông báo kết quả; chỉ ghi tên trường “mật khẩu”.

## 4. Thứ tự thông điệp để vẽ

| Mã | Từ → Đến | Nhãn trên mũi tên | Vị trí/ý nghĩa |
| --- | --- | --- | --- |
| M01 | Người đăng ký → Giao diện | Mở chức năng đăng ký tài khoản | Trước các fragment. |
| M02 | Giao diện → Người đăng ký | Hiển thị biểu mẫu đăng ký | Không cần gọi Hệ thống chỉ để dựng biểu mẫu. |
| M03 | Người đăng ký → Giao diện | Nhập thông tin và chọn “Đăng ký” | Dữ liệu theo phần 3. |
| M04 | Giao diện → Hệ thống | Gửi yêu cầu đăng ký tài khoản học viên | Bắt đầu xử lý yêu cầu. |
| M05 | Hệ thống → Hệ thống | Kiểm tra dữ liệu và email/SĐT đã sử dụng | Kiểm tra trước khi tạo. |
| M06 | Hệ thống → Giao diện | Trả lỗi đăng ký | `alt` ngoài: `[Dữ liệu không hợp lệ hoặc email/SĐT đã được sử dụng]`. |
| M07 | Giao diện → Người đăng ký | Hiển thị lỗi cần sửa | Kết thúc nhánh không đạt. |
| M08 | Hệ thống → Hệ thống | Tạo tài khoản Student, hồ sơ học viên và nhật ký bắt buộc | `alt` ngoài: `[Dữ liệu hợp lệ và chưa phát hiện email/SĐT trùng]`; thực hiện việc tạo rồi xét kết quả trong `alt` bên trong. |
| M09 | Hệ thống → Giao diện | Trả kết quả đăng ký thành công | `alt` bên trong: `[Tạo đầy đủ dữ liệu thành công]`. |
| M10 | Giao diện → Người đăng ký | Thông báo đăng ký thành công | Kết thúc thành công; không tự đăng nhập. |
| M11 | Hệ thống → Hệ thống | Hoàn tác việc tạo dữ liệu | `alt` bên trong: `[Email/SĐT bị đăng ký đồng thời]`. |
| M12 | Hệ thống → Giao diện | Trả lỗi email/SĐT đã được sử dụng | Xử lý trường hợp kiểm tra ban đầu đạt nhưng trùng lúc tạo. |
| M13 | Giao diện → Người đăng ký | Hiển thị lỗi cần sửa | Kết thúc nhánh trùng. |
| M14 | Hệ thống → Hệ thống | Hoàn tác việc tạo dữ liệu | `alt` bên trong: `[Không thể hoàn tất do lỗi xử lý]`. |
| M15 | Hệ thống → Giao diện | Trả kết quả đăng ký không thành công | Lỗi tạo tài khoản/hồ sơ hoặc nhật ký bắt buộc. |
| M16 | Giao diện → Người đăng ký | Thông báo không thể hoàn tất đăng ký | Kết thúc nhánh lỗi xử lý. |

M01–M16 là mã tham khảo trong hướng dẫn, không là tên operation/API. Có thể
để EA đánh số theo nhánh; không hiểu M06 đến M16 là các bước đều chạy nối tiếp.
Mỗi lần xử lý chỉ đi qua nhánh có guard tương ứng.

## 5. Phác thảo bố cục bằng văn bản

```text
Người đăng ký              Giao diện đăng ký              Hệ thống
      |                           |                          |
      |-- M01: Mở đăng ký -------->|                          |
      |<-- M02: Biểu mẫu ----------|                          |
      |-- M03: Nhập và gửi -------->|                          |
      |                           |-- M04: Yêu cầu đăng ký -->|
      |                           |                          |-- M05: Kiểm tra
      |                           |                          |   dữ liệu/trùng
      |                           |                          |
+------------------------- alt ngoài -------------------------+
| [Dữ liệu không hợp lệ hoặc email/SĐT đã được sử dụng]         |
|     |                           |<-- M06: Lỗi -------------|
|     |<-- M07: Hiển thị lỗi ------|                          |
|------------------------------------------------------------|
| [Dữ liệu hợp lệ và chưa phát hiện email/SĐT trùng]            |
|     |                           |                          |-- M08: Tạo
|     |                           |                          |   dữ liệu
| +---------------------- alt bên trong -------------------+ |
| | [Tạo đầy đủ dữ liệu thành công]                         | |
| |   |                           |<-- M09: Thành công -----| |
| |   |<-- M10: Thông báo ---------|                        | |
| |--------------------------------------------------------| |
| | [Email/SĐT bị đăng ký đồng thời]                        | |
| |   |                           |                        | |-- M11: Hoàn tác
| |   |                           |<-- M12: Lỗi trùng -------| |
| |   |<-- M13: Hiển thị lỗi ------|                        | |
| |--------------------------------------------------------| |
| | [Không thể hoàn tất do lỗi xử lý]                       | |
| |   |                           |                        | |-- M14: Hoàn tác
| |   |                           |<-- M15: Không thành công-| |
| |   |<-- M16: Thông báo ---------|                        | |
| +--------------------------------------------------------+ |
+------------------------------------------------------------+
```

Khung trên chỉ giúp hình dung bố cục. Trong EA, các self-message M05/M08/
M11/M14 phải nối từ lifeline Hệ thống trở về chính lifeline đó và nằm trong
operand tương ứng; không đặt chúng ngoài fragment như giới hạn của bản chữ.

## 6. Các note nên gắn và thứ tự dựng sơ đồ

Ba note có thể dùng nguyên văn:

1. Gắn gần M03/M04: **“Thông tin đăng ký: họ tên, ngày sinh, email, SĐT,
   mật khẩu và địa chỉ tùy chọn. Hệ thống xác định role Student.”**
2. Gắn gần M08: **“Tài khoản Student, hồ sơ học viên và nhật ký bắt buộc
   được tạo cùng một thao tác; lỗi thì hoàn tác, không lưu một phần.
   Email/SĐT trùng bị chặn kể cả khi có yêu cầu đồng thời.”**
3. Gắn gần M10: **“Đăng ký thành công không tự đăng nhập và không tạo hồ sơ
   đăng ký khóa học. Đăng nhập được mô tả riêng ở SQ03.”**

Thứ tự thao tác để dựng:

1. Tạo Sequence Diagram, đặt tên **SQ01 — Tự đăng ký tài khoản học viên**.
2. Đặt actor/boundary/control và ba lifeline theo phần 2.
3. Vẽ M01–M05 từ trên xuống; thêm activation khi có xử lý yêu cầu.
4. Tạo `alt` ngoài với hai operand; đặt M06/M07 trong operand không đạt.
5. Trong operand đạt, vẽ M08 rồi tạo `alt` bên trong với ba operand.
6. Điền thông điệp M09–M16 vào đúng nhánh; khép activation theo từng kết quả.
7. Gắn note, kiểm tra hướng mũi tên và đường kết thúc mỗi nhánh.

## 7. Kiểm tra trước khi hoàn tất

- Đủ ba lifeline, actor không gọi trực tiếp Hệ thống bỏ qua Giao diện.
- Mỗi phản hồi Hệ thống đi về Giao diện rồi hiển thị cho Người đăng ký.
- Guard phân biệt được dữ liệu sai/trùng, thành công và lỗi xử lý.
- Có xử lý trùng phát sinh đồng thời; kiểm tra ban đầu không bảo đảm tạo được.
- Không có kết quả thành công sau bước hoàn tác hoặc lỗi ghi nhật ký bắt buộc.
- Không thêm nhân viên duyệt tài khoản, OTP, email xác minh, cổng thanh toán,
  đăng nhập tự động hoặc đăng ký khóa học.
- Màu chỉ hỗ trợ đọc; vẫn hiểu được sơ đồ qua nhãn/guard khi in đen trắng.

Nếu mất phản hồi sau khi gửi, không kết luận chắc chắn rằng đăng ký thất bại
và không tự gửi lại thông tin đăng ký/mật khẩu. Có thể ghi note ngoại lệ này
thay vì thêm nhánh mạng vào sơ đồ nghiệp vụ chính; không tự thêm chức năng
tra cứu công khai tài khoản để kiểm chứng.
