# SQ02 — Hướng dẫn vẽ sequence Admin cấp tài khoản nhân sự

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV33; W01; F32; UC01.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: StaffAccountService.cs](../../../backend/src/EnglishCenter.Api/Services/Administration/StaffAccountService.cs).
- [Code: StaffAccountEndpoints.cs](../../../backend/src/EnglishCenter.Api/Api/Administration/StaffAccountEndpoints.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Admin tạo tài khoản cho nhân sự bằng vai trò cố định, nhận mật khẩu tạm một lần để bàn giao ngoài ứng dụng. Thành công chỉ được báo sau khi Account và audit bắt buộc đã commit; tài khoản chưa có phiên đăng nhập.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ02-A — Tạo tài khoản và bàn giao mật khẩu tạm**; actor Admin.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không thêm email/OTP, cấp Student thay, chọn nhiều role hoặc xem lại mật khẩu tạm.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Admin | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện tài khoản nhân sự | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Quyền | Admin đang có phiên NORMAL hợp lệ; không có đăng ký nhân sự tự phục vụ. |
| Thông tin | Họ tên, email, vai trò nhân sự theo lựa chọn hệ thống cho phép; SĐT tùy chọn theo hợp đồng staff. Không tạo StudentProfile cho tài khoản nhân sự. |
| Duy nhất | Email và SĐT khi có phải chưa sử dụng toàn Account; kiểm cả tình huống hai yêu cầu đồng thời. |
| Credential | Hệ thống sinh mật khẩu tạm ngẫu nhiên 32 ký tự; chỉ lưu hash, hạn 24 giờ, bắt đổi mật khẩu khi login. Không cho Admin nhập mật khẩu thường thay cơ chế tạm. |
| Bàn giao | Hiển thị plaintext trong RAM một lần sau thành công; không có API xem lại secret, không lưu vào storage/log. |
| Nguyên tử | Account ACTIVE với role đã chọn + metadata mật khẩu tạm + mandatory audit + dấu hoàn tất thao tác cùng giao dịch. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Tạo tài khoản và bàn giao mật khẩu tạm

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Admin → Giao diện tài khoản nhân sự | Mở chức năng Tạo tài khoản và bàn giao mật khẩu tạm | Điểm bắt đầu. |
| A02 | Giao diện tài khoản nhân sự → Admin | Hiển thị biểu mẫu và các vai trò được phép chọn | Dựng form tại UI; không tự thêm endpoint đọc mật khẩu/vai trò. |
| A03 | Admin → Giao diện tài khoản nhân sự | Nhập họ tên/email/vai trò/SĐT nếu có và chọn cấp tài khoản | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A04 | Giao diện tài khoản nhân sự → Hệ thống | Gửi yêu cầu cấp tài khoản nhân sự | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A05 | Hệ thống → Hệ thống | Kiểm Admin hiện tại, dữ liệu, vai trò được cấp, email/SĐT duy nhất | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A06 | Hệ thống → Giao diện tài khoản nhân sự | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Sai quyền/dữ liệu/vai trò; email hoặc SĐT trùng kể cả khi phát sinh đồng thời]`; không thực hiện hiệu ứng. |
| A07 | Giao diện tài khoản nhân sự → Admin | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A08 | Hệ thống → Hệ thống | Sinh mật khẩu tạm; tạo Account ACTIVE, hash/hạn 24 giờ/bắt đổi; ghi audit bắt buộc và hoàn tất thao tác nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A09 | Hệ thống → Giao diện tài khoản nhân sự | Thông tin tài khoản đã cấp và mật khẩu tạm chỉ trong phản hồi bàn giao đầu tiên | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A10 | Giao diện tài khoản nhân sự → Admin | Hiển thị kết quả đã hoàn tất và bàn giao mật khẩu tạm một lần | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A11 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A12 | Hệ thống → Giao diện tài khoản nhân sự | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A13 | Giao diện tài khoản nhân sự → Admin | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Admin; G = Giao diện tài khoản nhân sự; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Tạo tài khoản và bàn giao mật khẩu tạm
    N <.. A02 ..| : Hiển thị biểu mẫu và các vai trò được phép chọn
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A03 --> G : Nhập họ tên/email/vai trò/SĐT nếu có và chọn cấp tài khoản
                            |-- A04 --> H : Gửi yêu cầu cấp tài khoản nhân sự
                                                       H --+ A05: Kiểm Admin hiện tại, dữ liệu, vai trò được cấp, email/SĐT duy nhất
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A06 ..| : Trả lỗi điều kiện hiện tại
    N <.. A07 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A08: Sinh mật khẩu tạm; tạo Account ACTIVE, hash/hạn 24 giờ/bắt đổi; ghi…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A09 ..| : Thông tin tài khoản đã cấp và mật khẩu tạm chỉ trong phản hồi bàn giao…
    N <.. A10 ..| : Hiển thị kết quả đã hoàn tất và bàn giao mật khẩu tạm một lần
| |-- else [Không thể hoàn tất] ---------------------------| |
                                                       H --+ A11: Hoàn tác toàn bộ hiệu ứng của lần lưu
                                                       H <--+
                            G <.. A12 ..| : Trả kết quả không thể hoàn tất
    N <.. A13 ..| : Hiển thị thông báo không thể hoàn tất
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
+--------------------- hết opt ------------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

## 5. Các note nên gắn nguyên văn

1. **“Mật khẩu tạm chỉ hiển thị một lần. Copy/đóng bàn giao là thao tác trên Boundary, không phải request đọc lại secret.”**
2. **“Nếu mất phản hồi, kiểm Account/thao tác được phép trước. Nếu đã cấp nhưng mất secret, Admin chủ động reset mới ở SQ05; không tự cấp lại hoặc replay plaintext.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Tài khoản nhân sự không có StudentProfile.
- Secret chỉ xuất hiện sau commit và không nằm trong audit.
- Không auto-login; người nhận dùng SQ03 rồi SQ04.
- Có nhánh trùng đồng thời và rollback bắt buộc.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Email đã được cấp | Không tạo Account thứ hai; không sinh/bàn giao secret thành công giả. |
| Cấp thành công rồi đóng bàn giao | Không đọc lại plaintext; nếu mất secret và cần cấp lại thì Admin reset có chủ đích. |
| Đăng nhập bằng secret sau 24 giờ | SQ03 chặn credential tạm hết hạn; SQ02 không gửi gia hạn tự động. |
