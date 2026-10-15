# SQ15 — Hướng dẫn vẽ sequence quản lý khóa và cấu hình đào tạo

Ngày cập nhật: 14/10/2026. Hướng dẫn vẽ thủ công trong Enterprise Architect (EA),
mức nghiệp vụ Actor → Boundary → Control. Mã thông điệp/cách chia hình là
gợi ý trình bày, không tạo quyết định sản phẩm mới.

Nguồn: [SEQUENCE_PLAN](../../reference/SEQUENCE_DANH_MUC_CAP_NHAT_20261014.md) SQ15;
[FUNCTION_SPEC](../FUNCTION_SPEC.md) NV13;
[WORKFLOW_SPEC](../WORKFLOW_SPEC.md) W13;
[requirements](../../requirements.md) ADR-028/032/078–079/085/087/090/115.
Đối chiếu phạm vi Course/program đã duyệt ADR-146 tại
[SYSTEM_ARCHITECTURE_SPEC](../SYSTEM_ARCHITECTURE_SPEC.md) mục 10; đối chiếu thêm hợp đồng và triển khai hiện hành; cách vẽ không tạo quy tắc mới.

## 1. Phạm vi và cách chia hình

Actor là Giáo vụ (AcademicAffairs), có phiên thường hợp lệ tại F18.
Không có Director phê duyệt cấu hình.

Nên vẽ ba hình:

- **SQ15-A — Quản lý khóa:** tạo, sửa tên/giá, ngừng/kích hoạt lại; mỗi lần
  xử lý một action. Nếu cần minh họa riêng, tách các hình con cùng nhóm A.
- **SQ15-B — Cấu hình chương trình DRAFT:** tạo hoặc lưu toàn bộ cấu hình
  chương trình và thành phần điểm.
- **SQ15-C — Kích hoạt/khôi phục chương trình:** kiểm tra cấu hình đã lưu,
  preview điều kiện, xác nhận và chuyển ACTIVE.

Đổi tên chương trình, chuyển về DRAFT và ngừng chương trình là action riêng
theo bảng phần 3, có thể dùng bố cục xử lý chung như A; không ghép vào một
lần lưu cấu hình hoặc bắt tất cả action chạy nối tiếp.

**Band:** đọc bộ tham chiếu/ngưỡng seed để chọn Band của chương trình.
Phạm vi hiện tại không có editor thêm/xóa Band, sửa ngưỡng hoặc mở khóa.
Nếu cần vẽ riêng đoạn đọc: Giáo vụ yêu cầu danh sách Band → Giao diện gửi
yêu cầu → Hệ thống kiểm tra quyền/trả tham chiếu → Giao diện hiển thị.
Không vẽ lưu ngưỡng như một action mới.

## 2. Thành phần cần đặt trong EA

| Trái → phải | Tên lifeline | Loại/vai trò |
| --- | --- | --- |
| 1 | Giáo vụ | Actor AcademicAffairs, quản lý cấu hình đào tạo. |
| 2 | Giao diện khóa & chương trình | Boundary F18; danh mục, form cấu hình, preview và kết quả. |
| 3 | Hệ thống quản lý trung tâm | Control; kiểm tra quyền, dữ liệu/state/freeze, lưu và audit. |

Dùng message yêu cầu, return nét đứt, activation, self-message trên Hệ thống,
`alt`, `opt` xác nhận và note. Không cần Database, Course/Program hoặc
từng thành phần điểm làm lifeline. Không biến mỗi component thành một
request lưu riêng; B lưu cả cấu hình cùng thao tác.

Các hình bắt đầu khi đã mở danh mục/form và được phép xem. Đọc danh mục/
chi tiết có thể thêm trước bước đầu: yêu cầu → kiểm tra quyền/đọc hiện tại
→ trả dữ liệu → hiển thị; đọc không thay state. Form cũ không bảo đảm được ghi.

## 3. Các action và điều kiện nghiệp vụ

| Đối tượng/action | Quy tắc và kết quả |
| --- | --- |
| Tạo Course | Mã thủ công chuẩn hóa duy nhất; tên, học phí dương VND nguyên đồng; trạng thái ACTIVE/INACTIVE hợp lệ. |
| Sửa Course | Chỉ tên/giá trong lần lưu thông tin; code bất biến, trạng thái dùng action riêng. Giá mới chỉ cho Invoice phát hành sau, không sửa snapshot đã phát hành. |
| Course ngừng/kích hoạt lại | ACTIVE ↔ INACTIVE cùng record/code; INACTIVE chặn đăng ký khóa mới, không tự hủy hồ sơ/chương trình/lớp đang có. |
| Tạo Program | Course/Band hợp lệ, cặp duy nhất kể cả lịch sử INACTIVE; tạo DRAFT với cấu hình/thành phần hợp lệ về cấu trúc. |
| Lưu cấu hình Program | Chỉ DRAFT chưa có Class/phụ thuộc; lưu toàn bộ Course/Band, số LESSON, thành phần/kind/trọng số và tên theo đơn vị lưu đã chốt. |
| Kích hoạt Program | DRAFT → ACTIVE; hoặc INACTIVE → ACTIVE để khôi phục. Kiểm cấu hình đã lưu đầy đủ, giữ freeze hiện có. |
| Về DRAFT | ACTIVE → DRAFT chỉ khi chưa có Class; phải thực hiện rõ trước sửa cấu hình ACTIVE chưa dùng. |
| Ngừng Program | ACTIVE → INACTIVE; không tự dừng lớp đang dạy/chấm hoặc thay trạng thái học viên/bảng điểm. |
| Đổi tên Program | Action riêng, được phép khi đúng quyền kể cả đã dùng; không mở khóa các trường cấu hình bảo vệ. |
| Khóa cấu hình | Có bất kỳ Class tham chiếu, kể cả CANCELLED, thì khóa Course/Band, tập thành phần, tên component/kind/weights/số LESSON. Không mở khóa khi ngừng/khôi phục hoặc hủy lớp. |
| Bộ Band | Seed 0/25/45/65/85; khóa toàn bộ ngưỡng khi có kết quả đầu vào COMPLETED đầu tiên. SQ15 không cấp quyền sửa seed/override. |

**Validation cấu hình:**

- Tên Course/Program 1–150, component 1–100 sau trim; không ép tên Course/
  Program duy nhất toàn hệ thống; component name duy nhất trong chương trình.
- Số LESSON là số nguyên dương, không tính FINAL_EXAM.
- Kind gồm GENERAL/LESSON/FINAL_EXAM; weight >0 đến 100, tối đa hai số lẻ.
- DRAFT có thể chưa đủ tổng 100 hoặc chưa có FINAL_EXAM, nhưng từng trường
  luôn hợp lệ, không có nhiều hơn một FINAL_EXAM.
- Kích hoạt bắt buộc tổng trọng số đúng 100 và đúng một FINAL_EXAM.
  Không tự chuẩn hóa weight, làm tròn input sai hoặc thêm component còn thiếu.
- Không kích hoạt khi form còn sửa chưa lưu; preview kiểm cấu hình đã lưu,
  không giữ trạng thái. Hệ thống kiểm tra lại khi hoàn tất.
- Ghi cấu hình/audit bắt buộc cùng thành công hoặc rollback; first-use đồng
  thời không được làm cấu hình nửa cũ/nửa mới.
- Không duplicate program/version mới để né unique Course–Band/freeze;
  không thêm quyền xóa Course/Program/Band hoặc purge history.

## 4. SQ15-A — Thông điệp quản lý khóa

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| A01 | Giáo vụ → Giao diện | Chọn action và nhập thông tin; action trạng thái xác nhận đối tượng/ảnh hưởng trước gửi, hủy xác nhận thì không gửi A02. |
| A02 | Giao diện → Hệ thống | Yêu cầu tạo/sửa thông tin/đổi trạng thái khóa; một action mỗi lần. |
| A03 | Hệ thống → Hệ thống | Kiểm quyền, dữ liệu, mã khóa/state và trạng thái hiện tại. |
| A04 | Hệ thống → Giao diện | Không thể thực hiện; `alt [Không đủ điều kiện]`. |
| A05 | Giao diện → Giáo vụ | Hiển thị lỗi/tải lại. |
| A06 | Hệ thống → Hệ thống | Thực hiện action và audit nguyên tử; `else [Đủ điều kiện]`. |
| A07 | Hệ thống → Giao diện | Trả khóa sau thao tác; `[Hoàn tất toàn bộ thay đổi]`. |
| A08 | Giao diện → Giáo vụ | Thông báo thành công/tải lại. |
| A09 | Hệ thống → Hệ thống | Hoàn tác; `[Trùng/stale hoặc lỗi xử lý]`. |
| A10 | Hệ thống → Giao diện | Không thể hoàn tất. |
| A11 | Giao diện → Giáo vụ | Thông báo/tải lại, không ghi đè form cũ. |

Trong action sửa chỉ gửi thông tin cho phép, không sửa code; không có tác
động tự cập nhật các Invoice đã phát hành.

## 5. SQ15-B — Thông điệp tạo/lưu chương trình DRAFT

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| B01 | Giáo vụ → Giao diện | Nhập/chỉnh toàn bộ cấu hình và chọn Lưu. |
| B02 | Giao diện → Hệ thống | Gửi toàn bộ cấu hình chương trình/thành phần trong một yêu cầu. |
| B03 | Hệ thống → Hệ thống | Kiểm quyền, dữ liệu/quan hệ/unique; khi sửa kiểm DRAFT/chưa dùng/phụ thuộc và tập component hiện tại. |
| B04 | Hệ thống → Giao diện | Không thể lưu; `[Không đủ điều kiện hoặc cấu hình sai]`. |
| B05 | Giao diện → Giáo vụ | Lỗi/tải lại, không ghi một phần component. |
| B06 | Hệ thống → Hệ thống | Tạo hoặc lưu Program/thành phần và audit nguyên tử; `[Đủ điều kiện lưu DRAFT]`. |
| B07 | Hệ thống → Giao diện | Trả cấu hình DRAFT; `[Hoàn tất toàn bộ thay đổi]`. |
| B08 | Giao diện → Giáo vụ | Đã lưu, chưa kích hoạt. |
| B09 | Hệ thống → Hệ thống | Hoàn tác cả lần lưu; `[Trùng/first-use đồng thời hoặc lỗi xử lý]`. |
| B10 | Hệ thống → Giao diện | Không thể hoàn tất. |
| B11 | Giao diện → Giáo vụ | Thông báo/tải lại cấu hình. |

Program mới chưa có state trước đó: tạo DRAFT, không kiểm phải tồn tại một
Program DRAFT mới được tạo. B03 kiểm DRAFT/freeze chỉ khi sửa record đã có.

## 6. SQ15-C — Thông điệp kích hoạt/khôi phục chương trình

| Mã | Từ → Đến | Nhãn/ý nghĩa |
| --- | --- | --- |
| C01 | Giáo vụ → Giao diện | Yêu cầu kích hoạt/khôi phục chương trình đã lưu. |
| C02 | Giao diện → Hệ thống | Yêu cầu preview cấu hình đã lưu, chưa đổi state. |
| C03 | Hệ thống → Hệ thống | Kiểm quyền/cấu hình/quan hệ/state và điều kiện kích hoạt. |
| C04 | Hệ thống → Giao diện | Trả kết quả preview đạt/không đạt hoặc lỗi quyền. |
| C05 | Giao diện → Giáo vụ | Hiển thị điều kiện/ảnh hưởng; không tự sửa cấu hình thiếu. |
| C06 | Giáo vụ → Giao diện | Xác nhận; `opt [Preview đạt, không sửa chưa lưu và Giáo vụ xác nhận]`. |
| C07 | Giao diện → Hệ thống | Yêu cầu kích hoạt/khôi phục. |
| C08 | Hệ thống → Hệ thống | Kiểm tra lại state, quan hệ/toàn cấu hình và freeze hiện tại. |
| C09 | Hệ thống → Hệ thống | Chuyển ACTIVE và audit nguyên tử; `[Hoàn tất kích hoạt nguyên tử]`. |
| C10 | Hệ thống → Giao diện | Trả Program ACTIVE. |
| C11 | Giao diện → Giáo vụ | Thành công/tải lại. |
| C12 | Hệ thống → Hệ thống | Không ghi/hoàn tác nếu đã ghi; `[Điều kiện không đạt/thay đổi hoặc lỗi xử lý]`. |
| C13 | Hệ thống → Giao diện | Không thể kích hoạt. |
| C14 | Giao diện → Giáo vụ | Lỗi, đọc/preview lại. |

Về DRAFT/ngừng/đổi tên không dùng preview kích hoạt C02/C03 để kiểm;
dùng yêu cầu action riêng và kiểm tương ứng theo phần 3. Không áp thêm
Course ACTIVE guard cho mọi action ngoài các điều kiện đã được chốt.

## 7. Phác thảo bố cục bằng đường gạch

```text
SQ15-A — Tạo/sửa khóa hoặc đổi trạng thái (mỗi lần một action)

        Giáo vụ                             Giao diện khóa & chương trình           Hệ thống
        |                                   |                                       |
        |- A01: Chọn action/nhập ----------->                                       |
        |                                   |- A02: Yêu cầu thao tác khóa ---------->
        |                                   |                                       |----+  A03: Kiểm tra quyền,
        |                                   |                                       |<---+  dữ liệu/state/mã khóa
+- alt [Không đủ điều kiện] -------------------------------------------------------------------------------------------------+
|       |                                   <. A04: Không thể thực hiện ............|                                        |
|       <. A05: Lỗi/tải lại ................|                                       |                                        |
+- [Đủ điều kiện] -----------------------------------------------------------------------------------------------------------+
|       |                                   |                                       |----+  A06: Lưu action                  |
|       |                                   |                                       |<---+  và audit nguyên tử               |
| +- alt [Hoàn tất toàn bộ thay đổi] --------------------------------------------------------------------------------------+ |
| |     |                                   <. A07: Khóa sau thao tác ..............|                                      | |
| |     <. A08: Thành công/tải lại .........|                                       |                                      | |
| +- [Trùng/stale hoặc lỗi xử lý] -----------------------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  A09: Hoàn tác                  | |
| |     |                                   |                                       |<---+  thay đổi của yêu cầu           | |
| |     |                                   <. A10: Không thể hoàn tất .............|                                      | |
| |     <. A11: Thông báo/tải lại ..........|                                       |                                      | |
| +------------------------------------------------------------------------------------------------------------------------+ |
+----------------------------------------------------------------------------------------------------------------------------+

SQ15-B — Tạo/lưu cấu hình chương trình DRAFT

        Giáo vụ                             Giao diện khóa & chương trình           Hệ thống
        |                                   |                                       |
        |- B01: Nhập cấu hình -------------->                                       |
        |                                   |- B02: Gửi toàn bộ cấu hình ----------->
        |                                   |                                       |----+  B03: Kiểm tra quyền,
        |                                   |                                       |<---+  DRAFT/unused/tập thành phần
+- alt [Không đủ điều kiện hoặc cấu hình sai] -------------------------------------------------------------------------------+
|       |                                   <. B04: Không thể lưu ..................|                                        |
|       <. B05: Lỗi/tải lại ................|                                       |                                        |
+- [Đủ điều kiện lưu DRAFT] -------------------------------------------------------------------------------------------------+
|       |                                   |                                       |----+  B06: Lưu chương trình,           |
|       |                                   |                                       |<---+  thành phần và audit              |
| +- alt [Hoàn tất toàn bộ thay đổi] --------------------------------------------------------------------------------------+ |
| |     |                                   <. B07: Cấu hình DRAFT .................|                                      | |
| |     <. B08: Đã lưu/chưa kích hoạt ......|                                       |                                      | |
| +- [Trùng/first-use đồng thời hoặc lỗi xử lý] ---------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  B09: Hoàn tác                  | |
| |     |                                   |                                       |<---+  toàn bộ lần lưu                | |
| |     |                                   <. B10: Không thể hoàn tất .............|                                      | |
| |     <. B11: Thông báo/tải lại ..........|                                       |                                      | |
| +------------------------------------------------------------------------------------------------------------------------+ |
+----------------------------------------------------------------------------------------------------------------------------+

SQ15-C — Preview và kích hoạt/khôi phục chương trình

        Giáo vụ                             Giao diện khóa & chương trình           Hệ thống
        |                                   |                                       |
        |- C01: Yêu cầu kích hoạt ---------->                                       |
        |                                   |- C02: Preview cấu hình đã lưu -------->
        |                                   |                                       |----+  C03: Kiểm tra cấu hình
        |                                   |                                       |<---+  và điều kiện kích hoạt
        |                                   <. C04: Kết quả preview ................|
        <. C05: Điều kiện/ảnh hưởng ........|                                       |
+- opt [Preview đạt, không sửa chưa lưu và Giáo vụ xác nhận] ----------------------------------------------------------------+
|       |- C06: Xác nhận kích hoạt --------->                                       |                                        |
|       |                                   |- C07: Yêu cầu kích hoạt -------------->                                        |
|       |                                   |                                       |----+  C08: Kiểm tra lại                |
|       |                                   |                                       |<---+  state/cấu hình/freeze            |
| +- alt [Hoàn tất kích hoạt nguyên tử] -----------------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  C09: ACTIVE và audit           | |
| |     |                                   |                                       |<---+  giữ freeze/lịch sử             | |
| |     |                                   <. C10: Program ACTIVE .................|                                      | |
| |     <. C11: Thành công/tải lại .........|                                       |                                      | |
| +- [Điều kiện không đạt/thay đổi hoặc lỗi xử lý] ------------------------------------------------------------------------+ |
| |     |                                   |                                       |----+  C12: Không ghi/                | |
| |     |                                   |                                       |<---+  hoàn tác nếu đã ghi            | |
| |     |                                   <. C13: Không thể kích hoạt ............|                                      | |
| |     <. C14: Lỗi/preview lại ............|                                       |                                      | |
| +------------------------------------------------------------------------------------------------------------------------+ |
+----------------------------------------------------------------------------------------------------------------------------+
```

Nhãn trong hình chữ rút gọn, bảng thông điệp là nội dung đầy đủ. Các kiểm
tra/lưu/hoàn tác là self-message trên Hệ thống. Mỗi lần chạy một nhánh/action.

## 8. Note, cách dựng và checklist

Note nên gắn:

1. A06: **“Giá mới chỉ cho Invoice phát hành sau; không cập nhật hóa đơn cũ.”**
2. B03/B06: **“Lưu cả cấu hình cùng thao tác. DRAFT có thể chưa đủ tổng 100/
   FINAL_EXAM, nhưng từng trường phải hợp lệ. Đã có Class thì cấu hình khóa,
   kể cả Class CANCELLED; đổi tên Program không mở khóa cấu hình.”**
3. C03/C08: **“ACTIVE cần tổng trọng số 100, đúng một FINAL_EXAM và số LESSON
   nguyên dương; preview không giữ trạng thái, kiểm lại khi hoàn tất.”**
4. C09: **“Khôi phục giữ freeze/lịch sử; INACTIVE không tự dừng lớp đang có.”**

Dựng ba hình A/B/C, cùng ba lifeline; đặt fragment như phác thảo, activation
và note. Các action Program phụ có thể tách hình con; không thêm quy trình duyệt.

Kiểm tra trước hoàn tất:

- Chỉ Giáo vụ; không Director duyệt hoặc Admin mở khóa cấu hình.
- Course code bất biến, unique Course–Band; weight theo Program, không Course.
- DRAFT save khác activation; không ép draft phải đủ tổng 100 ngay khi lưu.
- Freeze chỉ các trường cấu hình bảo vệ, không cấm đổi tên Program được phép.
- Không editor Band, thêm ngưỡng, version/snapshot mới hoặc xóa lịch sử né khóa.
- Preview không kích hoạt; thay input cần lưu và kiểm tra lại.
- Một lỗi component/unique/stale/audit không lưu một phần.
- Mất phản hồi thì đọc lại record/operation đúng quyền; gửi lặp không tạo
  hiệu ứng thứ hai, không tự chọn mã/Program mới để né xung đột.


## Đối chiếu triển khai hiện tại — 14/10/2026

No-change sau kiểm quyền/state/versions: Course/Program edit toàn business values giống thì không tạo mutation/audit/replay marker mới; reason hoặc thay biểu diễn số không đủ. Band là reference-only trong UI hiện tại. Đổi tên Program là action riêng và không giải first-Class freeze.

Bộ hướng dẫn này là tài liệu vẽ mức nghiệp vụ; các đoạn thể hiện cơ chế triển khai dùng note/ref theo [README](README.md). Quyền, phiên bản, cohort/membership và điều kiện thời gian được backend kiểm hiện tại; nút/preview cũ không bảo đảm ghi thành công. Cùng key/content đã hoàn tất không tạo hiệu ứng thứ hai; không tự gửi lại khi mất phản hồi. Mandatory audit có rollback theo action; login/logout security-only vẫn best-effort, không audit mọi read.

Nguồn cập nhật: [CURRENT_STATE](../../CURRENT_STATE.md), [SESSION_FOCUS_REVIEW](../../plan/SESSION_FOCUS_REVIEW.md), [E04_REPLAY_SPEC](../E04_REPLAY_SPEC.md), [E04_FINAL_CONTRACT](../E04_FINAL_CONTRACT.md) và [LocalRuntimeServices](../../../backend/src/EnglishCenter.Api/Infrastructure/Operator/LocalRuntimeServices.cs). Các hướng dẫn SQ cũ và Word nguồn được giữ nguyên.


## Ví dụ tự kiểm tra khi vẽ

Các tình huống giả định dưới đây dùng để rà sơ đồ, không phải kết quả chạy kiểm thử mới.

| Tình huống | Kết quả phải thể hiện |
| --- | --- |
| Program đã có Class nhưng Class CANCELLED | Cấu hình vẫn freeze; hủy lớp không mở quyền sửa weight/count. |
| DRAFT tổng weight 90 | Có thể lưu các field hợp lệ, nhưng không ACTIVE cho đến tổng 100 và đúng một FINAL_EXAM. |
| Course tăng học phí | Invoice đã issue giữ snapshot; chỉ Invoice phát hành sau lấy giá mới. |
