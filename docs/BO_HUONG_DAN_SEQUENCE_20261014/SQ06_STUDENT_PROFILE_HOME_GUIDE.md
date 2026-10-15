# SQ06 — Hướng dẫn vẽ sequence Hồ sơ cá nhân và trang chủ Student

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** Thao tác chung; phép chiếu NV02–08; W04; F03 và menu hồ sơ; UC01–07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: EnrollmentEndpoints.cs](../../../backend/src/EnglishCenter.Api/Api/Enrollment/EnrollmentEndpoints.cs).
- [Code: LocalRuntimeServices.cs](../../../backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs).
- [Code: SqlOwnProfileExecutor.cs](../../../backend/src/EnglishCenter.Api/Infrastructure/Enrollment/SqlOwnProfileExecutor.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Student đọc trang chủ theo từng Registration và sửa các trường cá nhân được phép. Tách đọc trang chủ với lưu hồ sơ: mở trang không là lệnh cập nhật và không tạo tiến độ nghiệp vụ mới.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ06-A — Đọc trang chủ**; actor Học viên.
- **SQ06-B — Sửa hồ sơ cá nhân**; actor Học viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không thêm đổi email/avatar/upload, tiến độ lưu riêng hoặc staff sửa Profile thay Student.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Học viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện trang chủ học viên; Giao diện hồ sơ cá nhân | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Ownership | Phiên NORMAL của Student; backend xác định Student của phiên, không tin studentId do người dùng tự nhập. |
| Trường được sửa | Họ tên 1–100 code point, ngày sinh không sau ngày Việt Nam hiện tại, SĐT di động VN duy nhất, địa chỉ tùy chọn ≤255. Email chỉ đọc. |
| Đơn vị lưu | Thông tin Account và StudentProfile lưu cùng thao tác, kiểm phiên bản cả hai; không merge âm thầm form cũ. |
| Trang chủ | Trình bày theo mỗi Registration và next action do quyền/điều kiện hiện tại xác định; không gộp các lần học thành một state chung. |
| Điểm | Chỉ có link/giá trị Student được đọc; không đưa DRAFT/SUBMITTED/RETURNED vào payload trang chủ. |
| Hợp đồng Profile riêng | Backend hiện tại kiểm hai phiên bản rồi lưu/audit changedFields; không nhánh reject toàn dữ liệu giống như các lệnh replay. Không securityVersion increment/revoke-all và không replay completion/receipt. Không ép quy tắc no-change chung vào SQ06. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Đọc trang chủ

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Học viên → Giao diện trang chủ học viên | Mở trang chủ | Điểm bắt đầu: người dùng mở/áp lọc/chọn chi tiết; nếu UI tải mặc định, request phát sinh khi mở, không thêm click giả. |
| A02 | Giao diện trang chủ học viên → Hệ thống | Đọc các hồ sơ, tình trạng hóa đơn/lớp/lịch và liên kết kết quả được phép | Request đọc; bắt đầu activation. |
| A03 | Hệ thống → Hệ thống | Kiểm Student hiện tại và phạm vi sở hữu của từng dữ liệu trả về | Kiểm quyền/phạm vi trước truy vấn và trước trả kết quả hiện tại. |
| A04 | Hệ thống → Giao diện trang chủ học viên | Trả lỗi truy cập hoặc dữ liệu truy vấn | `alt [Không đủ điều kiện đọc: Hết phiên hoặc không có quyền Student]`; không đi nhánh query thành công. |
| A05 | Giao diện trang chủ học viên → Học viên | Hiển thị thông báo phù hợp | Kết thúc nhánh bị chặn; không lộ dữ liệu ngoài scope. |
| A06 | Hệ thống → Hệ thống | Dựng các phép chiếu theo Registration; lấy số liệu/khả năng thao tác hiện tại từ hệ thống | `else [Được đọc và yêu cầu hợp lệ]`; chỉ query/chiếu/tính toán, không đổi business state. |
| A07 | Hệ thống → Giao diện trang chủ học viên | Các thẻ hồ sơ và liên kết đúng quyền; không có dữ liệu kết quả chưa công bố | `alt [Query hoàn tất và quyền vẫn hợp lệ]`; return nét đứt. |
| A08 | Giao diện trang chủ học viên → Học viên | Hiển thị kết quả với đúng context của request | Kết thúc thành công; rỗng hợp lệ vẫn là thành công. |
| A09 | Hệ thống → Giao diện trang chủ học viên | Trả lỗi không thể hoàn tất đọc | `else [Query lỗi hoặc quyền không còn hợp lệ]`; không trả snapshot thành công giả. |
| A10 | Giao diện trang chủ học viên → Học viên | Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện trang chủ học viên; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở trang chủ
                            |-- A02 --> H : Đọc các hồ sơ, tình trạng hóa đơn/lớp/lịch và liên kết kết quả được phép
                                                       H --+ A03: Kiểm Student hiện tại và phạm vi sở hữu của từng dữ liệu trả về
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A04 ..| : Trả lỗi truy cập hoặc dữ liệu truy vấn
    N <.. A05 ..| : Hiển thị thông báo phù hợp
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A06: Dựng các phép chiếu theo Registration; lấy số liệu/khả năng thao tác…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A07 ..| : Các thẻ hồ sơ và liên kết đúng quyền; không có dữ liệu kết quả chưa…
    N <.. A08 ..| : Hiển thị kết quả với đúng context của request
| |-- else [Không thể hoàn tất] ---------------------------| |
                            G <.. A09 ..| : Trả lỗi không thể hoàn tất đọc
    N <.. A10 ..| : Hiển thị lỗi; không gắn dữ liệu cũ thành kết quả mới
| +------------------ hết alt kết quả --------------------+ |
+--------------------- hết alt guard ------------------------+
```

Tên rút gọn N/G/H chỉ dùng để căn cột; trong EA dùng tên lifeline đầy đủ ở trên. Nhãn dài được rút gọn trong bản chữ, **lấy toàn nhãn và guard ở bảng thông điệp** khi vẽ. Các mũi tên kết quả nét đứt, self-message quay về H; mỗi nhánh loại trừ nhau. Không nối các dòng nhánh lỗi rồi success thành một đường chạy.

### B. Sửa hồ sơ cá nhân

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Học viên → Giao diện hồ sơ cá nhân | Mở chức năng Sửa hồ sơ cá nhân | Điểm bắt đầu. |
| B02 | Giao diện hồ sơ cá nhân → Hệ thống | Đọc tên/email/ngày sinh/SĐT/địa chỉ và thông tin phiên bản hiện tại | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện hồ sơ cá nhân | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện hồ sơ cá nhân → Học viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Học viên → Giao diện hồ sơ cá nhân | Sửa các trường được phép và chọn lưu hồ sơ | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện hồ sơ cá nhân → Hệ thống | Gửi yêu cầu lưu hồ sơ cá nhân | Lệnh lưu Profile với hai phiên bản hiện tại; không request key/replay receipt. |
| B08 | Hệ thống → Hệ thống | Kiểm ownership, phiên bản Account/Profile, dữ liệu hợp lệ và SĐT duy nhất; không tự áp reject-all-equal của lệnh replay | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện hồ sơ cá nhân | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Sai dữ liệu, SĐT trùng hoặc phiên bản Account/Profile cũ]`; không thực hiện hiệu ứng. |
| B10 | Giao diện hồ sơ cá nhân → Học viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Cập nhật các trường Account/Profile được phép và mandatory audit chỉ changedFields trong cùng giao dịch; không revocation hoặc replay completion | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện hồ sơ cá nhân | Hồ sơ cá nhân mới và phiên bản mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện hồ sơ cá nhân → Học viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện hồ sơ cá nhân | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện hồ sơ cá nhân → Học viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Học viên; G = Giao diện hồ sơ cá nhân; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Sửa hồ sơ cá nhân
                            |-- B02 --> H : Đọc tên/email/ngày sinh/SĐT/địa chỉ và thông tin phiên bản hiện tại
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Sửa các trường được phép và chọn lưu hồ sơ
                            |-- B07 --> H : Gửi yêu cầu lưu hồ sơ cá nhân
                                                       H --+ B08: Kiểm ownership, phiên bản Account/Profile, dữ liệu hợp lệ và SĐT duy…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Cập nhật các trường Account/Profile được phép và mandatory audit chỉ…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Hồ sơ cá nhân mới và phiên bản mới
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

1. **“Email đăng nhập bất biến. Sửa hồ sơ cá nhân không đổi Course/Band/kết quả và không thu hồi phiên như đổi mật khẩu.”**
2. **“Trang chủ đọc các dữ liệu có sẵn; không auto-book, auto-confirm hoặc tự tạo audit page-view.”**
3. **“Lưu Profile không có replay completion hoặc lookup receipt. Audit chỉ tên trường thay đổi; nếu request toàn giá trị giống thì không tự vẽ nhánh backend NO_CHANGE không có trong code.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

**Riêng lưu Profile hiện tại:** không có replay receipt/lookup thao tác cho API này. Mất phản hồi thì đọc lại hồ sơ và so trạng thái hiện tại; không vẽ ref tra cứu receipt hoặc tự gửi lại PATCH với request key. Audit Profile chỉ ghi tên các trường thay đổi, không old/new PII.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- Hai hình đọc/lưu có điểm bắt đầu riêng.
- Thể hiện kiểm cả Account và Profile trước lưu.
- SĐT trùng đồng thời không gây lưu một phần.
- Không công bố điểm nháp qua trang chủ.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Form cũ chỉ sửa địa chỉ nhưng tên đã đổi ở client khác | Kiểm Account/Profile versions; không ghi đè âm thầm từ form cũ. |
| Student thay email trên request tự tạo | Không có quyền sửa email; không lưu field ngoài hợp đồng. |
| Trang chủ có một report SUBMITTED | Không trả các giá trị chưa công bố cho Student. |
