# SQ05 — Hướng dẫn vẽ sequence Admin quản lý tài khoản

Ngày: 02/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Bố cục/mã thông điệp là gợi ý,
không tạo quyết định sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](SEQUENCE_PLAN.md) SQ05;
[FUNCTION_SPEC](FUNCTION_SPEC.md) NV33–NV34;
[WORKFLOW_SPEC](WORKFLOW_SPEC.md) W03;
[requirements](../requirements.md) ADR-020/048–052/093/110/115/128.
Chuyển trạng thái đối chiếu hợp đồng Administration đã duyệt ADR-141 trong
[SYSTEM_ARCHITECTURE_SPEC](SYSTEM_ARCHITECTURE_SPEC.md) mục Administration;
không sử dụng các refinements E04 chưa duyệt.

## 1. Mục tiêu và cách chia hình

Admin có phiên thường hợp lệ, thao tác tại F32 trên tài khoản hiện có
(Student hoặc nhân sự). Cấp tài khoản mới thuộc SQ02; tự đổi mật khẩu thuộc SQ04.

Nên vẽ hai hình:

- **SQ05-A — Reset mật khẩu:** tạo mật khẩu tạm, thu hồi mọi phiên, bàn giao
  kết quả một lần cho Admin; tài khoản đích giữ nguyên trạng thái.
- **SQ05-B — Khóa/mở/ngừng dùng/khôi phục:** một lần chọn và thực hiện một
  action theo trạng thái hiện tại, cập nhật Account và audit bắt buộc.

Bốn action của SQ05-B là các lựa chọn khác nhau, không bốn bước luôn chạy
nối tiếp. Không có Admin thứ hai duyệt thao tác hoặc lý do bắt buộc mới.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Admin | Actor, người đang thực hiện thao tác quản lý tài khoản. |
| 2 | Giao diện tài khoản | Boundary, F32; chọn tài khoản, xác nhận tác động và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control, kiểm tra quyền/state, thay đổi Account/phiên và audit. |

Thêm message/return, activation, self-message trên Hệ thống, `alt`,
`opt [Admin xác nhận …]` và note. Chỉ hiển thị khung xác nhận không đổi dữ liệu.
Trong bàn giao mật khẩu tạm, thao tác copy/xóa state là self-message trên
**Giao diện**, không Hệ thống.

Tài khoản đích là đối tượng được quản lý, không cần vẽ thành một actor luôn
tham gia. Không thêm email server, Clipboard hoặc từng thiết bị làm lifeline.
Bàn giao trực tiếp ngoài phần mềm ghi bằng note, không vẽ tự gửi email.

## 3. Điều kiện và chuyển trạng thái

| Action | Trạng thái trước → sau | Ảnh hưởng phiên |
| --- | --- | --- |
| Reset mật khẩu | Giữ ACTIVE/LOCKED/DISABLED hiện tại | Thu hồi mọi phiên; mật khẩu tạm mới hạn 24 giờ, bắt đổi trước nghiệp vụ. |
| Khóa | ACTIVE → LOCKED | Thu hồi mọi phiên. |
| Mở khóa | LOCKED → ACTIVE | Không phục hồi phiên cũ, phải đăng nhập mới. |
| Ngừng sử dụng | ACTIVE hoặc LOCKED → DISABLED | Thu hồi mọi phiên. |
| Khôi phục | DISABLED → ACTIVE | Cùng Account/role/dữ liệu/lịch sử; không phục hồi phiên cũ. |

Guard chung:

- Kiểm tra Admin hiện tại, Account đích và trạng thái khi ghi, kể cả thao tác đồng thời.
- Chặn Admin khóa/ngừng dùng chính mình hoặc Admin ACTIVE cuối cùng.
  Không tự áp guard này thành quy tắc cấm reset/mở/khôi phục không có nguồn.
- Reset không tự mở LOCKED/khôi phục DISABLED; không đổi role/email.
- Chặn sai trạng thái/action; không dùng “đổi status bất kỳ” thay các action.
- Chặn đăng nhập tạm do sai mật khẩu là timer khác, không là LOCKED; không
  vẽ mở khóa như thao tác mặc nhiên xóa timer đó.
- Không đổi Teacher assignment, Class, Registration hoặc kết quả nghiệp vụ
  khi Account bị khóa/ngừng dùng; không xóa lịch sử.
- Credential/state, thu hồi phiên liên quan và audit bắt buộc cùng thành công
  hoặc cùng rollback. Audit lỗi không áp best-effort của login/logout.

## 4. SQ05-A — Thông điệp reset và bàn giao mật khẩu tạm

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| A01 | Admin → Giao diện | Chọn Account đích và reset mật khẩu. |
| A02 | Giao diện → Admin | Hiển thị tài khoản/trạng thái và ảnh hưởng reset. |
| A03 | Admin → Giao diện | Xác nhận reset; trong `opt [Admin xác nhận reset]`. |
| A04 | Giao diện → Hệ thống | Gửi yêu cầu reset tài khoản đã chọn. |
| A05 | Hệ thống → Hệ thống | Kiểm tra quyền, Account hiện tại và xung đột. |
| A06 | Hệ thống → Giao diện | Không thể reset; `alt [Không đủ điều kiện]`. |
| A07 | Giao diện → Admin | Thông báo phù hợp/tải lại Account. |
| A08 | Hệ thống → Hệ thống | Tạo mật khẩu tạm ngẫu nhiên 32 ký tự/hạn 24 giờ, lưu an toàn, bắt đổi, thu hồi mọi phiên và audit; `else [Đủ điều kiện reset]`. |
| A09 | Hệ thống → Giao diện | Trả kết quả một lần sau `[Hoàn tất toàn bộ thay đổi]`; gồm email/role/mật khẩu tạm/hạn từ Hệ thống. |
| A10 | Giao diện → Admin | Hiển thị khối kết quả và cảnh báo không thể xem lại sau đóng. |
| A11 | Admin → Giao diện | Chọn Sao chép; `opt [Admin chủ động sao chép]`. |
| A12 | Giao diện → Giao diện | Sao chép theo quyền trình duyệt; nếu bị chặn hướng dẫn chọn chữ/sao chép thủ công. |
| A13 | Admin → Giao diện | Tick hiểu đóng không xem lại, chọn “Đã ghi nhận — Đóng”. |
| A14 | Giao diện → Giao diện | Xóa plaintext khỏi state khi đóng. |
| A15 | Hệ thống → Hệ thống | Hoàn tác thay đổi của yêu cầu; nhánh `[Không thể hoàn tất]` thay cho A09–A14. |
| A16 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất, không mật khẩu tạm thành công giả. |
| A17 | Giao diện → Admin | Thông báo lỗi/tải lại trạng thái có quyền. |

A13/A14 là đường đóng chủ động khi Admin còn ở khối kết quả; rời trang,
đăng xuất hoặc hết phiên cũng xóa secret khỏi state. Tick không là bằng
chứng đã giao mật khẩu cho chủ tài khoản. Không tự đọc/copy clipboard,
không hứa xóa được nội dung clipboard sau đóng, không in/tải file secret.

**Mất phản hồi hoặc mất mật khẩu tạm:** kiểm tra Account/operation theo quyền
Admin; kết quả kiểm tra không trả lại secret. Không tự lặp reset để lấy lại
mật khẩu. Nếu cần mật khẩu mới, Admin chủ đích xác nhận lần reset mới.
Reset đích là tài khoản của Admin đang gọi cũng thu hồi phiên hiện tại;
không giả định phiên cũ còn quyền đọc Account/operation sau thành công.

## 5. SQ05-B — Thông điệp đổi trạng thái tài khoản

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| B01 | Admin → Giao diện | Chọn Account và một action hợp lệ. |
| B02 | Giao diện → Admin | Hiển thị đối tượng/trạng thái/ảnh hưởng, nút đúng động từ. |
| B03 | Admin → Giao diện | Xác nhận action; `opt [Admin xác nhận thao tác]`. |
| B04 | Giao diện → Hệ thống | Gửi action đã chọn. |
| B05 | Hệ thống → Hệ thống | Kiểm tra quyền, trạng thái, self/last ACTIVE Admin nếu khóa/ngừng dùng. |
| B06 | Hệ thống → Giao diện | Không thể thực hiện; `alt [Không đủ điều kiện]`. |
| B07 | Giao diện → Admin | Thông báo phù hợp và tải lại. |
| B08 | Hệ thống → Hệ thống | Đổi trạng thái và audit nguyên tử; khóa/ngừng dùng thu hồi mọi phiên, mở/khôi phục không phục hồi phiên cũ. |
| B09 | Hệ thống → Giao diện | Trả Account/trạng thái mới; `[Hoàn tất toàn bộ thay đổi]`. |
| B10 | Giao diện → Admin | Thông báo thành công/tải lại Account. |
| B11 | Hệ thống → Hệ thống | Hoàn tác thay đổi; `[Điều kiện đổi đồng thời hoặc lỗi xử lý]`. |
| B12 | Hệ thống → Giao diện | Trả kết quả không thể hoàn tất. |
| B13 | Giao diện → Admin | Thông báo lỗi/tải lại trạng thái. |

B08 thực hiện action đã chọn theo bảng phần 3; không chạy cả bốn action.
Nếu muốn thể hiện riêng, thêm `alt` bốn operand trong B08, hoặc tách
hình con SQ05-B1/B2/B3/B4. Bản quy hoạch này dùng một message chung và note
chuyển trạng thái để tránh lặp các tương tác giống nhau.

## 6. Phác thảo bố cục bằng đường gạch

```text
SQ05-A — Admin reset mật khẩu

        Admin                               Giao diện tài khoản                     Hệ thống
        |                                   |                                       |
        |- A01: Chọn Account/reset --------->                                       |
        <. A02: Đối tượng/ảnh hưởng ........|                                       |
+- opt [Admin xác nhận reset] ---------------------------------------------------------------------------------------------+
|       |- A03: Xác nhận reset ------------->                                       |                                      |
|       |                                   |- A04: Yêu cầu reset ------------------>                                      |
|       |                                   |                                       |----+  A05: Kiểm tra quyền            |
|       |                                   |                                       |<---+  và Account hiện tại            |
| +- alt [Không đủ điều kiện] -------------------------------------------------------------------------------------------+ |
| |     |                                   <. A06: Không thể reset ................|                                    | |
| |     <. A07: Thông báo/tải lại ..........|                                       |                                    | |
| +- [Đủ điều kiện reset] -----------------------------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  A08: Tạo mật khẩu tạm        | |
| |     |                                   |                                       |<---+  hạn 24h/bắt đổi              | |
| |     |   A08 cùng thao tác: lưu an toàn, thu hồi mọi phiên, ghi audit; giữ status Account.                            | |
| | +- alt [Hoàn tất toàn bộ thay đổi] --------------------------------------------------------------------------------+ | |
| | |   |                                   <. A09: Kết quả một lần ................|                                  | | |
| | |   <. A10: Mật khẩu tạm/hạn ...........|                                       |                                  | | |
| | | +- opt [Admin chủ động sao chép] ------------------------------------------------------------------------------+ | | |
| | | | |- A11: Chọn Sao chép -------------->                                       |                                | | | |
| | | | |                                   |----+  A12: Sao chép chủ động          |                                | | | |
| | | | |                                   |<---+  hoặc hướng dẫn thủ công         |                                | | | |
| | | +--------------------------------------------------------------------------------------------------------------+ | | |
| | |   |- A13: Đã hiểu/Đóng --------------->                                       |                                  | | |
| | |   |                                   |----+  A14: Xóa secret khỏi            |                                  | | |
| | |   |                                   |<---+  state giao diện                 |                                  | | |
| | +- [Không thể hoàn tất] -------------------------------------------------------------------------------------------+ | |
| | |   |                                   |                                       |----+  A15: Hoàn tác              | | |
| | |   |                                   |                                       |<---+  thay đổi của yêu cầu       | | |
| | |   |                                   <. A16: Không thể hoàn tất .............|                                  | | |
| | |   <. A17: Thông báo/tải lại ..........|                                       |                                  | | |
| | +------------------------------------------------------------------------------------------------------------------+ | |
| +----------------------------------------------------------------------------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------------------------------+

SQ05-B — Khóa/mở/ngừng dùng/khôi phục (hình riêng)

        Admin                               Giao diện tài khoản                     Hệ thống
        |                                   |                                       |
        |- B01: Chọn Account/action -------->                                       |
        <. B02: Đối tượng/ảnh hưởng ........|                                       |
+- opt [Admin xác nhận thao tác] ------------------------------------------------------------------------------------------+
|       |- B03: Xác nhận action ------------>                                       |                                      |
|       |                                   |- B04: Gửi action đã chọn ------------->                                      |
|       |                                   |                                       |----+  B05: Kiểm tra quyền,           |
|       |                                   |                                       |<---+  state/self/last Admin          |
| +- alt [Không đủ điều kiện] -------------------------------------------------------------------------------------------+ |
| |     |                                   <. B06: Không thể thực hiện ............|                                    | |
| |     <. B07: Thông báo/tải lại ..........|                                       |                                    | |
| +- [Đủ điều kiện] -----------------------------------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  B08: Đổi trạng thái          | |
| |     |                                   |                                       |<---+  và audit nguyên tử           | |
| |     |   B08: khóa/ngừng dùng thu hồi mọi phiên; mở/khôi phục không phục hồi phiên cũ.                                | |
| | +- alt [Hoàn tất toàn bộ thay đổi] --------------------------------------------------------------------------------+ | |
| | |   |                                   <. B09: Account/state mới ..............|                                  | | |
| | |   <. B10: Thành công/tải lại .........|                                       |                                  | | |
| | +- [Điều kiện đổi đồng thời hoặc lỗi xử lý] -----------------------------------------------------------------------+ | |
| | |   |                                   |                                       |----+  B11: Hoàn tác              | | |
| | |   |                                   |                                       |<---+  thay đổi của yêu cầu       | | |
| | |   |                                   <. B12: Không thể hoàn tất .............|                                  | | |
| | |   <. B13: Thông báo/tải lại ..........|                                       |                                  | | |
| | +------------------------------------------------------------------------------------------------------------------+ | |
| +----------------------------------------------------------------------------------------------------------------------+ |
+--------------------------------------------------------------------------------------------------------------------------+
```

Nhãn dài được rút gọn trong hình chữ; bảng thông điệp giữ nội dung đầy đủ.
A12/A14 là self-message trên Giao diện; các kiểm tra/lưu/hoàn tác trên Hệ thống.

## 7. Note và cách dựng

Note nên gắn:

1. A08: **“Reset giữ status Account, tạo mật khẩu tạm 32 ký tự/hạn 24 giờ,
   bắt đổi, thu hồi mọi phiên và audit nguyên tử.”**
2. A10: **“Mật khẩu tạm chỉ hiển thị một lần, không log/storage/replay.
   Đóng/rời/đăng xuất/hết phiên xóa secret khỏi state; clipboard có thể còn.”**
3. B05: **“Khóa/ngừng dùng chặn chính mình và Admin ACTIVE cuối cùng,
   kể cả concurrent. Kiểm tra lại trạng thái khi hoàn tất.”**
4. B08: **“Mở/khôi phục không phục hồi phiên cũ; không đổi role/email,
   phân công giảng dạy hoặc dữ liệu nghiệp vụ.”**

Đặt ba lifeline, vẽ bước chọn/xác nhận, `opt` gửi, các `alt` quyền/kết
quả; vẽ SQ05-A và SQ05-B riêng. Gắn note thay đổi nguyên tử và guard.
Nếu Admin bỏ xác nhận thì kết thúc, không gửi mutation.

## 8. Checklist

- Không cấp tài khoản mới hay tự đổi mật khẩu thay cho luồng đang mô tả.
- Reset không tự mở khóa/khôi phục; mật khẩu tạm không do Admin nhập tùy ý.
- Đúng guard self/last ACTIVE Admin và bảng chuyển trạng thái.
- Khóa/ngừng dùng/reset thu hồi mọi phiên; mở/khôi phục không hồi sinh phiên.
- Audit bắt buộc lỗi rollback cả thay đổi/thu hồi liên quan.
- Không xóa Account/lịch sử hoặc tự phân công lại giáo viên.
- Không thêm lý do/người duyệt thứ hai/OTP/email recovery.
- Secret chỉ một lần; copy có chủ đích, đóng xóa state, không replay secret.
- Timeout không mặc nhiên là thất bại; không tự reset mới khi chưa kiểm tra
  hoặc tự lặp thao tác bằng request identity mới để tạo hiệu ứng khác.
