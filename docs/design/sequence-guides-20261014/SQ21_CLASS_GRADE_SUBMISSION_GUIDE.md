# SQ21 — Hướng dẫn vẽ sequence Teacher gửi và gửi lại bảng điểm cả lớp

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect, mức Actor → Boundary → Control theo mẫu SQ01. Cách chia hình và mã thông điệp là cách trình bày, không tạo API hoặc quy tắc sản phẩm mới.

**Truy vết:** NV25–26; W18; F27; UC07.

Nguồn đối chiếu:

- [Danh mục sequence cập nhật](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md).
- [FUNCTION_SPEC](../FUNCTION_SPEC.md) và [WORKFLOW_SPEC](../WORKFLOW_SPEC.md), theo mã NV/W ở trên.
- [Requirements](../../requirements.md), [quyết định](../../DECISIONS.md), [tình trạng hiện tại](../../CURRENT_STATE.md).
- [Code: GradeService.cs](../../../backend/src/EnglishCenter.Api/Services/Classes/GradeService.cs).

## 1. Mục tiêu, điểm bắt đầu và điểm kết thúc

Teacher xác nhận gửi toàn bộ cohort bảng điểm. Gửi lần đầu và gửi lại sau RETURNED là hai điểm vào cho whole-Class transition, không một lệnh cho từng học viên.

**Bắt đầu:** actor của từng hình mở chức năng tương ứng bằng phiên NORMAL hợp lệ, trong phạm vi và điều kiện ở phần 3.

**Thành công:** kết quả nêu tại từng hình; với lệnh ghi chỉ sau commit toàn bộ đơn vị lưu, với đọc chỉ sau query và kiểm quyền thành công.

**Không thành công:** không công bố dữ liệu ngoài scope hoặc kết quả mới giả; lệnh ghi lỗi xác định trước commit không để hiệu ứng một phần. Mất phản hồi là tình trạng chưa rõ, khác rollback đã được xác định.

**Cách chia hình:**

- **SQ21-A — Gửi lần đầu toàn lớp**; actor Giáo viên.
- **SQ21-B — Gửi lại sau RETURNED**; actor Giáo viên.

Không nối các hình này như những bước luôn xảy ra sau nhau. Không Teacher gửi một Student, Director duyệt, auto-submit khi đủ điểm hoặc tự chốt sau resubmit.

## 2. Các thành phần cần đặt trong EA

| Trái → phải | Lifeline | Loại và trách nhiệm |
| --- | --- | --- |
| 1 | Giáo viên | Actor; mỗi hình chỉ dùng actor thực hiện hình đó. |
| 2 | Giao diện sổ điểm lớp | Boundary; chọn đúng tên theo hình, nhận thao tác và hiển thị kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm quyền/điều kiện, xử lý hoặc truy vấn và trả kết quả. |

Dùng request liền, return nét đứt, activation và self-message trên Hệ thống. `alt` cho quyền/guard và kết quả; `opt` cho thao tác chủ động sau xem. Không dùng loop cho người dùng gửi lại tự động. Các đối tượng Account/Registration/Class/Score chỉ cần note hoặc tên xử lý, không cần lifeline Database/Controller/Service ở mức nghiệp vụ. Không thêm actor phụ chỉ vì dữ liệu của họ được xử lý.

## 3. Dữ liệu và guard nghiệp vụ

| Nội dung | Quy tắc phải giữ |
| --- | --- |
| Lớp | Current Teacher assignment; Class COMPLETED. Đủ điểm chưa tự submit, ONGOING không submit. |
| Toàn cohort | Mọi assignment/report hiện tại, đủ mọi required component và score-link/context đúng. Nút gửi không thay backend validation toàn lớp. |
| Source state | Mỗi report của toàn cohort phải DRAFT hoặc RETURNED theo predicate backend hiện tại; không SUBMITTED/FINALIZED. Hình A minh họa gửi đầu, hình B minh họa gửi lại sau return; không thêm một invariant đồng nhất khác mà code không kiểm. |
| Form | Không còn sửa chưa lưu tại UI; phải lưu nháp từng Student ở SQ20 trước. Preview chỉ đọc và không giữ quyền gửi. |
| Nguyên tử | Chuyển mọi report thành SUBMITTED, metadata submit/grouped audit/replay cùng giao dịch. Lỗi một report rollback mọi report. |
| Công bố | SUBMITTED chưa visible cho Student; final_score chưa lưu. RETURNED giữ scores/comment, không bắt nhập lại từ đầu. |

Hệ thống kiểm có thẩm quyền lúc xử lý. Kiểm input của Boundary hỗ trợ người dùng, không thay kiểm của Control. Read/preview chỉ phản ánh một thời điểm; phiên bản, membership, tài nguyên và đồng hồ có thể thay đổi trước khi ghi.

## 4. Thứ tự thông điệp và phác thảo từng hình

Mã A01/B01… chỉ dùng trong hướng dẫn, không phải API. Với một request chỉ đi một operand hợp lệ; nhánh lỗi quyền/đọc đầu vào kết thúc trước opt ghi.

### A. Gửi lần đầu toàn lớp

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| A01 | Giáo viên → Giao diện sổ điểm lớp | Mở chức năng Gửi lần đầu toàn lớp | Điểm bắt đầu. |
| A02 | Giao diện sổ điểm lớp → Hệ thống | Đọc toàn bộ context lớp/gradebook và preview SUBMIT với cohort đủ điều kiện | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| A04 | Hệ thống → Giao diện sổ điểm lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| A05 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| A06 | Giáo viên → Giao diện sổ điểm lớp | Xem ảnh hưởng và xác nhận gửi bảng điểm cả lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| A07 | Giao diện sổ điểm lớp → Hệ thống | Gửi yêu cầu SUBMIT cả lớp | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| A08 | Hệ thống → Hệ thống | Kiểm Class COMPLETED, current Teacher, full cohort/versions/manifests, đủ mọi score/current links; mỗi report DRAFT hoặc RETURNED theo predicate và không local unsaved edit | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| A09 | Hệ thống → Giao diện sổ điểm lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Lớp chưa COMPLETE, thiếu score, còn edit chưa lưu hoặc report/cohort/link thay đổi]`; không thực hiện hiệu ứng. |
| A10 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| A11 | Hệ thống → Hệ thống | Chuyển toàn cohort SUBMITTED với submit metadata/audit nhóm và completion trong một giao dịch | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| A12 | Hệ thống → Giao diện sổ điểm lớp | Cả lớp SUBMITTED, chờ Giáo vụ review | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| A13 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| A14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| A15 | Hệ thống → Giao diện sổ điểm lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| A16 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện sổ điểm lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- A01 --> G : Mở chức năng Gửi lần đầu toàn lớp
                            |-- A02 --> H : Đọc toàn bộ context lớp/gradebook và preview SUBMIT với cohort đủ điều…
                                                       H --+ A03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. A04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. A05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- A06 --> G : Xem ảnh hưởng và xác nhận gửi bảng điểm cả lớp
                            |-- A07 --> H : Gửi yêu cầu SUBMIT cả lớp
                                                       H --+ A08: Kiểm Class COMPLETED, current Teacher, full cohort/versions/manifests,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. A09 ..| : Trả lỗi điều kiện hiện tại
    N <.. A10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ A11: Chuyển toàn cohort SUBMITTED với submit metadata/audit nhóm và…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. A12 ..| : Cả lớp SUBMITTED, chờ Giáo vụ review
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

### B. Gửi lại sau RETURNED

| Mã | Từ → Đến | Nhãn thông điệp | Guard/vị trí |
| --- | --- | --- | --- |
| B01 | Giáo viên → Giao diện sổ điểm lớp | Mở chức năng Gửi lại sau RETURNED | Điểm bắt đầu. |
| B02 | Giao diện sổ điểm lớp → Hệ thống | Đọc lý do trả, scores/comment giữ lại; sau lưu sửa SQ20 đọc preview RESUBMIT | Đọc/preview trước lệnh; không commit thay đổi ở bước này. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền/phạm vi đọc và lấy context hiện tại | Nếu không thể đọc thì return lỗi qua Boundary và kết thúc; không đi tới lệnh ghi. |
| B04 | Hệ thống → Giao diện sổ điểm lớp | Trả dữ liệu/điều kiện/phiên bản hiện tại | Return sau đọc thành công; không giữ chỗ hoặc bảo đảm quyền ghi về sau. |
| B05 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị dữ liệu và khả năng thao tác | Chỉ xem chưa làm thay đổi trạng thái. |
| B06 | Giáo viên → Giao diện sổ điểm lớp | Xác nhận gửi lại bảng điểm cả lớp | `opt [Người dùng chủ động chọn thao tác]`; không giả định một popup xác nhận nếu UI chỉ có nút Lưu. |
| B07 | Giao diện sổ điểm lớp → Hệ thống | Gửi yêu cầu RESUBMIT cả lớp | Lệnh ghi theo input/context đã chọn; cùng request identity cho chính lần gửi này. |
| B08 | Hệ thống → Hệ thống | Kiểm Class COMPLETED, current Teacher, full cohort/versions/manifests, đủ mọi score/current links; mỗi report DRAFT hoặc RETURNED theo predicate và không local unsaved edit | Kiểm có thẩm quyền lúc ghi; dữ liệu form/preview cũ không thay kiểm tra này. |
| B09 | Hệ thống → Giao diện sổ điểm lớp | Trả lỗi điều kiện hiện tại | `alt [Không đạt: Report không RETURNED, điểm/link thiếu hợp lệ hoặc có thay đổi đồng thời]`; không thực hiện hiệu ứng. |
| B10 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị lỗi/lý do và cho tải lại context được phép | Kết thúc nhánh bị chặn; không tự sửa/xếp/đặt phương án khác. |
| B11 | Hệ thống → Hệ thống | Chuyển toàn cohort RETURNED→SUBMITTED, cập nhật submit metadata và audit nhóm nguyên tử | `else [Đủ điều kiện thực hiện]`; self-message biểu diễn đơn vị nguyên tử, chưa trả success giữa chừng. |
| B12 | Hệ thống → Giao diện sổ điểm lớp | Cả lớp SUBMITTED ở lần gửi mới | `alt [Toàn bộ thao tác commit thành công]`; return sau commit. |
| B13 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị kết quả đã hoàn tất | Kết thúc thành công; không gọi actor của bước tiếp theo tự động. |
| B14 | Hệ thống → Hệ thống | Hoàn tác toàn bộ hiệu ứng của lần lưu | `else [Lỗi xử lý hoặc kiểm tra cuối không còn đạt trước commit]`; không dùng nhánh này để kết luận timeout sau commit. |
| B15 | Hệ thống → Giao diện sổ điểm lớp | Trả kết quả không thể hoàn tất | Không thành công một phần; phản hồi rollback chỉ khi hệ thống xác định được. |
| B16 | Giao diện sổ điểm lớp → Giáo viên | Hiển thị thông báo không thể hoàn tất | Kết thúc nhánh lỗi xử lý. |

```text
N = Giáo viên; G = Giao diện sổ điểm lớp; H = Hệ thống
N (Actor)              G (Boundary)               H (Control)
    |                       |                          |
    |-- B01 --> G : Mở chức năng Gửi lại sau RETURNED
                            |-- B02 --> H : Đọc lý do trả, scores/comment giữ lại; sau lưu sửa SQ20 đọc preview…
                                                       H --+ B03: Kiểm quyền/phạm vi đọc và lấy context hiện tại
                                                       H <--+
                            G <.. B04 ..| : Trả dữ liệu/điều kiện/phiên bản hiện tại
    N <.. B05 ..| : Hiển thị dữ liệu và khả năng thao tác
+-- opt [Người dùng chủ động thực hiện] ---------------------+
    |-- B06 --> G : Xác nhận gửi lại bảng điểm cả lớp
                            |-- B07 --> H : Gửi yêu cầu RESUBMIT cả lớp
                                                       H --+ B08: Kiểm Class COMPLETED, current Teacher, full cohort/versions/manifests,…
                                                       H <--+
+-- alt [Guard không đạt] ----------------------------------+
                            G <.. B09 ..| : Trả lỗi điều kiện hiện tại
    N <.. B10 ..| : Hiển thị lỗi/lý do và cho tải lại context được phép
|-- else [Guard đạt] ---------------------------------------|
                                                       H --+ B11: Chuyển toàn cohort RETURNED→SUBMITTED, cập nhật submit metadata và…
                                                       H <--+
| +-- alt [Xử lý/query hoàn tất] --------------------------+ |
                            G <.. B12 ..| : Cả lớp SUBMITTED ở lần gửi mới
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

1. **“Từng Score save ở SQ20 và whole-Class submit ở SQ21 là giao dịch riêng. Không gom các lần nhập điểm trước đó vào transaction submit.”**
2. **“RETURNED→sửa/lưu→resubmit→review có thể lặp qua nhiều lần tương tác; không bao thành vòng lặp backend tự động.”**

Với lệnh không chứa credential: nếu mất phản hồi sau gửi, không kết luận rollback và không tự gửi lại; đọc object/operation được phép hoặc dùng tra cứu thủ công đúng họ luồng hiện hành. Không bịa một endpoint lookup chung cho mọi module. Với mật khẩu tạm/credential, không replay plaintext để nhận lại secret.

## 6. Thứ tự dựng sơ đồ trong EA

1. Tạo hình theo tên A/B… ở phần 1; đặt ba lifeline của actor tương ứng.
2. Vẽ phần mở/đọc và phản hồi đúng bảng, thêm nhánh kết thúc khi không truy cập được.
3. Với lệnh ghi, đặt opt thao tác chủ động; với query, đi trực tiếp vào kiểm quyền/yêu cầu.
4. Thêm alt guard và alt kết quả, đặt self-message/return đúng operand; đóng activation tại kết quả mỗi nhánh.
5. Gắn note phần 5; chỉ dùng ref sang SQ khác cho bước diễn ra ở lượt tương tác riêng.
6. Kiểm lại hướng mũi tên, đơn vị lưu và trường hợp bị chặn trước khi xuất hình.

## 7. Checklist trước khi hoàn tất

- loop [Mỗi report trong cohort] nằm trong xử lý Hệ thống và cùng một commit.
- Cohort membership đầy đủ, không chỉ trang hiện tại.
- Không publish hoặc lưu final_score khi SUBMITTED.
- Một invalid report không cho gửi phần còn lại.
- Không có kết quả success sau rollback/lỗi xác định; không gán timeout sau gửi thành thất bại chắc chắn.
- Actor không bỏ qua Boundary, return đi Control → Boundary → Actor; không gọi actor của workflow kế tiếp đồng bộ.
- Note/guard đủ nghĩa khi in đen trắng; màu chỉ hỗ trợ trình bày.

Cơ chế dùng chung được mô tả tại [hướng dẫn sử dụng bộ tài liệu](README.md): phiên/RAM/CSRF, replay/unknown outcome, no-change và audit đúng loại. Không cần kéo hạ tầng đó thành lifeline mới trong sơ đồ nghiệp vụ.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| 14 Student đủ điểm, Student thứ 15 thiếu một component | Không submit 14 người trước; cả lớp bị chặn. |
| Class ONGOING nhưng đã nhập đủ điểm | Không submit; phải Class COMPLETED. |
| Teacher đang có local edits chưa lưu | UI không gửi toàn lớp; lưu nháp rồi đọc preview/context mới. |
